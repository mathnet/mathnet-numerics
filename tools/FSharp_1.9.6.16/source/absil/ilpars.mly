%{
// (c) Microsoft Corporation 2005-2009. 

open Internal.Utilities
open Internal.Utilities.Text

open Microsoft.FSharp.Text
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 

module Ilascii = Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiConstants 
module Ildiag = Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 
module Ilx = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 
module Illib = Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

open Illib
open Ildiag 
open Il     
open Ilascii
open Ilx
  
let pfailwith s = 
    stderr.WriteLine ("*** error: "^s); 
    raise Parsing.RecoverableParseError 

/// vararg sentinels
type sig_arg = SigArg of (string option * ILType)  | Sentinel

let decode_varargs args = 
  let rec normals = function 
    | [] -> ([],None)
    | Sentinel :: t -> ([],Some (varargs t))
    | SigArg (_,p)::t -> let (n,r) = normals t in (p::n, r)
  and varargs = function 
    | [] -> []
    | SigArg (_,ty):: t ->  let l = varargs t in ty::l
    | Sentinel :: t -> pfailwith "two sentinels in vararg call" in
  normals args;;


type 'a resolved_at_mspec_scope = 
    ResolvedAtMethodSpecScope of (ILGenericParameterDefs -> 'a)

let no_mspec_scope x = ResolvedAtMethodSpecScope (fun cgparams -> x)
let resolve_mspec_scope (ResolvedAtMethodSpecScope f) x = f x
let resolve_mspec_scope_then (ResolvedAtMethodSpecScope f) g = 
  ResolvedAtMethodSpecScope (fun x -> resolve_mspec_scope (g(f x)) x)

let resolve_mspec_scope_to_formal_scope tspeco obj = 
  match tspeco with 
    None ->  resolve_mspec_scope obj mk_empty_gparams
  | Some (tspec:ILTypeSpec) -> resolve_mspec_scope obj (gparams_of_inst tspec.GenericArgs)

let resolve_mspec_scope_to_current_scope obj = 
    resolve_mspec_scope obj mk_empty_gparams


let find_mscorlib_aref() = 
  match (!parse_ilGlobals).mscorlib_scoref with
  | ScopeRef_assembly aref -> aref
  | _ -> pfailwith "mscorlib_scoref not set to valid assembly reference in parse_ilGlobals"

let find_aref nm = 
  if nm = "mscorlib" then find_mscorlib_aref() else
  pfailwith ("Undefined assembly ref '" ^ nm ^ "'") 

%} 

/*-----------------------------------------------------------------------
 * The YACC Grammar
 *----------------------------------------------------------------------*/

%token <int64> VAL_INT64     /* 342534523534534      0x34FA434644554 */
%token <int32> VAL_INT32_ELIPSES     /* 342534523534534... */
%token <double> VAL_FLOAT64        /* -334234 24E-34 */
%token <Ilascii.arg_instr> INSTR_ARG
%token <Ilascii.i32_instr> INSTR_I 
%token <Ilascii.i32_i32_instr> INSTR_I32_I32
%token <Ilascii.i64_instr> INSTR_I8 
%token <Ilascii.real_instr> INSTR_R 
%token <Ilascii.loc_instr> INSTR_LOC
%token <Ilascii.method_instr> INSTR_METHOD 
%token <Ilascii.none_instr> INSTR_NONE 
%token <Ilascii.string_instr> INSTR_STRING 
%token <Ilascii.switch_instr> INSTR_SWITCH 
%token <Ilascii.tok_instr> INSTR_TOK 
%token <Ilascii.type_instr> INSTR_TYPE 
%token <Ilascii.int_type_instr> INSTR_INT_TYPE 
%token <Ilascii.valuetype_instr> INSTR_VALUETYPE 
%token <int>     VAL_HEXBYTE    /* 05 1A FA */
%token <string>  VAL_ID                 /* testing343 */
%token <string>  VAL_DOTTEDNAME                 /* testing343.abd */
%token <string>  VAL_QSTRING    /* "Hello World\n" */
%token <string>  VAL_SQSTRING   /* 'Hello World\n' */
%token AMP  
%token BANG  
%token BOOL  
%token BYTEARRAY
%token CDECL  
%token CHAR  
%token CLASS 
%token COMMA  
%token DCOLON
%token DEFAULT  
%token DOT  
%token DOT_CCTOR   
%token DOT_CTOR   
%token ELIPSES
%token EOF  
%token EXPLICIT  
%token FASTCALL  
%token FIELD   
%token FLOAT32  
%token FLOAT64  
%token GREATER  
%token INSTANCE  
%token INT  
%token INT16  
%token INT32  
%token INT64  
%token INT8  
%token LBRACK  
%token LESS  
%token LPAREN  
%token METHOD   
%token NATIVE  
%token OBJECT
%token PLUS
%token RBRACK
%token RPAREN   
%token SLASH
%token STAR  
%token STDCALL  
%token STRING  
%token THISCALL  
%token TYPEDREF  
%token UINT  
%token UINT16  
%token UINT32  
%token UINT64  
%token UINT8  
%token UNMANAGED  
%token UNSIGNED  
%token VALUE  
%token VALUETYPE
%token VARARG  
%token VOID  

/* %type <ILModuleDef> modul */
%type <string> name1 
%type <ILType resolved_at_mspec_scope> typ
%type <ILInstr array> top_instrs
%type <ILType> top_typ
%start top_instrs top_typ

/**************************************************************************/
%%      

/* ENTRYPOINT */
top_typ: typ EOF
       { resolve_mspec_scope $1 [] }

/* ENTRYPOINT */
top_instrs: instrs2 EOF
       { Array.of_list $1 }


compQstring: 
     VAL_QSTRING { $1 }
   | compQstring PLUS VAL_QSTRING { $1 ^ $3 }

methodName: 
     DOT_CTOR
       { ".ctor" }
   | DOT_CCTOR
       { ".cctor" }
   | name1
       { $1 }

instrs2: 
   | instr instrs2 
        { $1 []  :: $2  } 
   | { [] }


methodSpec: methodSpecMaybeArrayMethod
    { let  data,varargs = $1 in 
      mk_mspec_in_typ data,varargs }


methodSpecMaybeArrayMethod: 
     callConv typ typSpec DCOLON methodName opt_actual_tyargs LPAREN sigArgs0 RPAREN
       { let callee_class_typ = resolve_mspec_scope_to_current_scope $3 in 
         let gscope = (if Ilx.gen_is_array_ty callee_class_typ then None else Some (tspec_of_typ callee_class_typ)) in
         let argtys_n_varargs = resolve_mspec_scope_to_formal_scope gscope $8 in
         let (argtys,varargs) = decode_varargs argtys_n_varargs in 
         let minst = resolve_mspec_scope_to_current_scope $6 in 
         let callee_retty = resolve_mspec_scope_to_formal_scope gscope $2 in 
         (callee_class_typ, $1, $5, argtys, callee_retty, minst), varargs }

instr_r_head: 
     INSTR_R LPAREN 
        { lexing_bytearray := true; $1 }

instr: 
     INSTR_NONE                                         
        {  ($1 ()) }
   | INSTR_ARG int32
        {  ($1 (uint16 ( ( $2)))) }
   | INSTR_LOC int32
        {  ($1 (uint16 ( ( $2)))) }
   | INSTR_I int32                                      
        {  ($1 $2) }
   | INSTR_I32_I32 int32 int32                                  
        {  ($1 ($2,$3)) }
   | INSTR_I8 int64                                     
        {  ($1 $2) }
   | INSTR_R float64                                    
        {  ($1 (NUM_R8 $2)) }
   | INSTR_R int64
        {  ($1 (NUM_R8 (float $2))) }
   | INSTR_METHOD methodSpecMaybeArrayMethod 
        {  
             begin
               let  ((encl_typ, cc, nm, argtys, retty, minst) as data),varargs = $2 in 
               if Ilx.gen_is_array_ty encl_typ then 
                 (fun prefixes -> 
                   let (shape,ty) = gen_dest_array_ty encl_typ in 
                   begin match nm with
                   | "Get" -> I_ldelem_any(shape,ty) 
                   | "Set" ->  I_stelem_any(shape,ty) 
                   | "Address" ->  I_ldelema((if prefixes=[Prefix_Readonly] then ReadonlyAddress else NormalAddress), shape,ty) 
                   | ".ctor" ->   I_newarr(shape,ty) 
                   | _ -> failwith "bad method on array type"
                   end)
               else 
               
                 $1 (mk_mspec_in_typ data, varargs)
             end }
   | INSTR_TYPE typSpec                                 
        {  ($1 (resolve_mspec_scope_to_current_scope $2)) }
   | INSTR_INT_TYPE int32 typSpec                               
        {  ($1 ( $2,resolve_mspec_scope_to_current_scope $3)) }
   | INSTR_VALUETYPE typSpec                            
        { let vtr = 
             match resolve_mspec_scope_to_current_scope $2 with 
               (* Type_boxed tr -> Type_value tr
             | Type_value vtr as typ -> typ
             | *) typ ->  typ in
           ($1 vtr) }
   | INSTR_TOK typSpec                          
        {  ($1 (Token_type (resolve_mspec_scope_to_current_scope $2)))  }

/*-----------------------------------------------
 * Formal signatures of methods etc. 
 *---------------------------------------------*/

sigArgs0:  
        {  no_mspec_scope [] }
   | sigArgs1   { $1 }

sigArgs1: 
   sigArgs1a
        { ResolvedAtMethodSpecScope (fun c -> List.map (fun obj -> resolve_mspec_scope obj c) (List.rev $1)) }

sigArgs1a: 
     sigArg
        { [$1] }
   | sigArgs1a COMMA sigArg
        { $3:: $1 }

sigArg: 
     ELIPSES
       { no_mspec_scope Sentinel }
   | typ opt_id
       { resolve_mspec_scope_then $1 (fun ty -> 
         no_mspec_scope (SigArg($2, ty))) }



opt_id:  { None } | id { Some $1 }

 
/*-----------------------------------------------
 * Type names
 *---------------------------------------------*/
name1: 
   | id
        { $1 }
   | VAL_DOTTEDNAME     
        { $1 }
   | name1 DOT id       
        { $1 ^"."^ $3 }

className:
     LBRACK name1 RBRACK slashedName
        { let (enc,nm) = $4 in 
          let aref = find_aref $2 in 
          ScopeRef_assembly aref, enc, nm }
   | slashedName
        { let enc, nm = $1 in (ScopeRef_local, enc, nm) }

slashedName: 
     name1 
        { ([],$1) } 
   | name1 SLASH slashedName
        { let (enc,nm) = $3 in ($1::enc, nm)  } 

typeNameInst:
     className opt_actual_tyargs 
        { let (a,b,c) = $1 in 
          resolve_mspec_scope_then $2 (fun inst -> 
          no_mspec_scope ( (mk_tspec ( (mk_nested_tref (a,b,c)), inst)))) }


typeName:
     className 
        { let (a,b,c) = $1 in 
          no_mspec_scope ( (mk_tspec ( (mk_nested_tref (a,b,c)), mk_empty_gactuals))) }


typSpec: 
     typeName   
        { resolve_mspec_scope_then $1 (fun tref -> 
          no_mspec_scope (Type_boxed tref))  }
   | typ                
        { $1 }
   | LPAREN typ RPAREN  
        { $2 }


callConv: 
     INSTANCE callKind 
        { Callconv (CC_instance,$2) }
   | EXPLICIT callKind 
        { Callconv (CC_instance_explicit,$2) }
   | callKind 
        { Callconv (CC_static,$1) }

callKind: 
     /* EMPTY */  
      { CC_default }
   | DEFAULT          
      { CC_default }
   | VARARG
      { CC_vararg }
   | UNMANAGED CDECL
      { CC_cdecl }
   | UNMANAGED STDCALL 
      { CC_stdcall }
   | UNMANAGED THISCALL 
      { CC_thiscall }
   | UNMANAGED FASTCALL 
      { CC_fastcall }
                
/*-----------------------------------------------
 * The full algebra of types, typically producing results 
 * awaiting further info about how to fix up type
 * variable numbers etc.
 *---------------------------------------------*/

typ: STRING
       { no_mspec_scope (!parse_ilGlobals).typ_String } 
   | OBJECT
       { no_mspec_scope (!parse_ilGlobals).typ_Object } 
   | CLASS typeNameInst
       { resolve_mspec_scope_then $2 (fun tspec -> 
          no_mspec_scope (Type_boxed tspec)) } 
   | VALUE CLASS typeNameInst
       { resolve_mspec_scope_then $3 (fun tspec -> 
         no_mspec_scope (Type_value tspec)) } 
   | VALUETYPE typeNameInst
       { resolve_mspec_scope_then $2 (fun tspec -> 
         no_mspec_scope (Type_value tspec)) } 
   | typ LBRACK RBRACK  
       { resolve_mspec_scope_then $1 (fun ty -> no_mspec_scope (mk_sdarray_ty ty)) } 
   | typ LBRACK bounds1 RBRACK 
       { resolve_mspec_scope_then $1 (fun ty -> no_mspec_scope (mk_array_ty (ty,ILArrayShape $3))) }
   | typ AMP
       { resolve_mspec_scope_then $1 (fun ty -> no_mspec_scope (Type_byref ty)) }
   | typ STAR
       { resolve_mspec_scope_then $1 (fun ty -> no_mspec_scope (Type_ptr ty)) }

   | TYPEDREF
       { no_mspec_scope (!parse_ilGlobals).typ_TypedReference }
   | CHAR
       { no_mspec_scope (!parse_ilGlobals).typ_char }
   | VOID
       { no_mspec_scope Type_void }
   | BOOL
       { no_mspec_scope (!parse_ilGlobals).typ_bool }
   | INT8
       { no_mspec_scope (!parse_ilGlobals).typ_int8 }
   | INT16              
       { no_mspec_scope (!parse_ilGlobals).typ_int16 }
   | INT32              
       { no_mspec_scope (!parse_ilGlobals).typ_int32 }
   | INT64              
       { no_mspec_scope (!parse_ilGlobals).typ_int64 }
   | FLOAT32            
       { no_mspec_scope (!parse_ilGlobals).typ_float32 }
   | FLOAT64            
       { no_mspec_scope (!parse_ilGlobals).typ_float64 }
   | UNSIGNED INT8      
       { no_mspec_scope (!parse_ilGlobals).typ_uint8 }
   | UNSIGNED INT16     
       { no_mspec_scope (!parse_ilGlobals).typ_uint16 }
   | UNSIGNED INT32     
       { no_mspec_scope (!parse_ilGlobals).typ_uint32 }
   | UNSIGNED INT64     
       { no_mspec_scope (!parse_ilGlobals).typ_uint64 }
   | UINT8      
       { no_mspec_scope (!parse_ilGlobals).typ_uint8 }
   | UINT16     
       { no_mspec_scope (!parse_ilGlobals).typ_uint16 }
   | UINT32     
       { no_mspec_scope (!parse_ilGlobals).typ_uint32 }
   | UINT64     
       { no_mspec_scope (!parse_ilGlobals).typ_uint64 }
   | NATIVE INT         
       { no_mspec_scope (!parse_ilGlobals).typ_IntPtr }
   | NATIVE UNSIGNED INT  
       { no_mspec_scope (!parse_ilGlobals).typ_UIntPtr }
   | NATIVE UINT  
       { no_mspec_scope (!parse_ilGlobals).typ_UIntPtr }

   | BANG int32
       { no_mspec_scope (Type_tyvar (uint16 ( $2)))  }


bounds1:  
     bound 
       { [$1] }
   | bounds1 COMMA bound
       { $1 @ [$3] }
   
bound: 
     /*EMPTY*/          
       { (None, None) } 
   | int32
       { (None, Some $1) } 
   | int32 ELIPSES int32
       { (Some $1, Some ($3 - $1 + 1)) }       
   | int32 ELIPSES
       { (Some $1, None) } 
/* We need to be able to parse all of */
/* ldc.r8     0. */
/* float64(-657435.)     */
/* and int32[0...,0...] */
/* The problem is telling an integer-followed-by-ellipses from a floating-point-nubmer-followed-by-dots */
   | VAL_INT32_ELIPSES int32
       { (Some $1, Some ($2 - $1 + 1)) }       
   | VAL_INT32_ELIPSES
       { (Some $1, None) } 
                                
id: 
     VAL_ID
       { $1 }
   | VAL_SQSTRING
       { $1 }

int32: 
     VAL_INT64
       { int32 $1 }

int64: 
     VAL_INT64
       { $1 }

float64: 
     VAL_FLOAT64
       { $1 }
   | FLOAT64 LPAREN int64 RPAREN
       { System.BitConverter.Int64BitsToDouble $3 }

opt_actual_tyargs: 
      /* EMPTY */ 
        { no_mspec_scope mk_empty_gactuals }
   | actual_tyargs 
        { resolve_mspec_scope_then $1 (fun res -> 
          no_mspec_scope  res) }

actual_tyargs:
     LESS actualTypSpecs GREATER 
        { $2 } 

actualTypSpecs: 
     typSpec
        { resolve_mspec_scope_then $1 (fun res -> 
          no_mspec_scope [ res]) }
   | actualTypSpecs COMMA typSpec
        { resolve_mspec_scope_then $1 (fun x -> 
          resolve_mspec_scope_then $3 (fun y -> 
          no_mspec_scope (x @ [ y]))) }

