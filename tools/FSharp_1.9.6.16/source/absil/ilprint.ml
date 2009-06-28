// (c) Microsoft Corporation. All rights reserved

module (* internal *) Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiConstants 
open Microsoft.FSharp.Compiler.AbstractIL.IL 

open System.Text
open System.IO

let emit_tailcalls = ref true

#if DEBUG
let tailcall_via_ldftn = ref false
let call_via_ldftn = ref false
let pretty () = true

// -------------------------------------------------------------------- 
// Pretty printing
// --------------------------------------------------------------------


let tyvar_generator = 
  let i = ref 0 in 
  fun n -> 
    incr i; n^string !i

(* Carry an environment because the way we print method variables *)
(* depends on the gparams of the current scope. *)
type ppenv = 
    { ppenvClassFormals: int;
      ppenvMethodFormals: int }
let ppenv_enter_method  mgparams env = 
    {env with ppenvMethodFormals=mgparams}
let ppenv_enter_tdef gparams env =
    {env with ppenvClassFormals=List.length gparams; ppenvMethodFormals=0}
let mk_ppenv = { ppenvClassFormals=0; ppenvMethodFormals=0 }
let debug_ppenv = mk_ppenv 
let ppenv_enter_modul env = { env with  ppenvClassFormals=0; ppenvMethodFormals=0 }

(* -------------------------------------------------------------------- 
 * Pretty printing - output streams
 * -------------------------------------------------------------------- *)

let output_int os (i:int) = output_string os (string i)
let output_hex_digit os i = 
  assert (i >= 0 & i < 16);
  if i > 9 then output_char os (char (int32 'A' + (i-10))) 
  else output_char os (char (int32 '0' + i))

let output_qstring os s =
  output_char os '"';
  for i = 0 to String.length s - 1 do
    let c = String.get s i in
    if (c >= '\000' & c <= '\031') or (c >= '\127' & c <= '\255') then 
      let c' = int32 c in
      output_char os '\\';
      output_int os (c'/64);
      output_int os ((c' % 64) / 8);
      output_int os (c' % 8) 
    else if (c = '"')  then 
      (output_char os '\\'; output_char os '"')
    else if (c = '\\')  then 
      (output_char os '\\'; output_char os '\\')
    else 
      output_char os c
  done;
  output_char os '"'
let output_sqstring os s =
  output_char os '\'';
  for i = 0 to String.length s - 1 do
    let c = s.[i] in
    if (c >= '\000' & c <= '\031') or (c >= '\127' & c <= '\255') then 
      let c' = int32 c in
      output_char os '\\';
      output_int os (c'/64);
      output_int os ((c' % 64) / 8);
      output_int os (c' % 8) 
    else if (c = '\\')  then 
      (output_char os '\\'; output_char os '\\')
    else if (c = '\'')  then 
      (output_char os '\\'; output_char os '\'')
    else 
      output_char os c
  done;
  output_char os '\''

let output_list sep f os a =
  if List.length a > 0 then 
    begin 
      f os (List.hd a);
      List.iter (fun x -> output_string os sep; f os x) (List.tl a)
    end
let output_parens f os a = output_string os "("; f os a; output_string os ")"
let output_angled f os a = output_string os "<"; f os a; output_string os ">"
let output_bracks f os a = output_string os "["; f os a; output_string os "]"

let output_id os n = output_sqstring os n

let output_label os n = output_string os n

(* let output_data_label os ((l,_): DataLabel) = output_string os l *)
(* REVIEW: get data labels right when using indexes *)

let output_lid os lid = output_list "." output_string os lid;;
let string_of_type_name (_,n) = n

let output_byte os i = 
  output_hex_digit os (i / 16);
  output_hex_digit os (i % 16)

let output_bytes os bytes = 
  for i = 0 to Bytes.length bytes - 1 do
    output_byte os (Bytes.get bytes i);
    output_string os " "
  done


let bits_of_float32 (x:float32) = System.BitConverter.ToInt32(System.BitConverter.GetBytes(x),0)
let bits_of_float (x:float) = System.BitConverter.DoubleToInt64Bits(x)

let output_u8 os (x:byte) = output_string os (string (int x))
let output_i8 os (x:sbyte) = output_string os (string (int x))
let output_u16 os (x:uint16) = output_string os (string (int x))
let output_i16 os (x:int16) = output_string os (string (int x))
let output_u32 os (x:uint32) = output_string os (string (int64 x))
let output_i32 os (x:int32) = output_string os (string x) 
let output_u64 os (x:uint64) = output_string os (string (int64 x))
let output_i64 os (x:int64) = output_string os (string x) 
let output_ieee32 os (x:float32) = (output_string os "float32 ("; output_string os (string (bits_of_float32 x)); output_string os ")")
let output_ieee64 os (x:float) = (output_string os "float64 ("; output_string os (string (bits_of_float x)); output_string os ")")

let rec goutput_scoref env os = function 
  | ScopeRef_local -> ()
  | ScopeRef_assembly aref ->
      output_string os "["; output_sqstring os aref.Name; output_string os "]"
  | ScopeRef_module mref ->
      output_string os "[.module "; output_sqstring os mref.Name; output_string os "]" 

and goutput_type_name_ref env os (scoref,enc,n) = 
  goutput_scoref env os scoref;
  output_list "/" output_sqstring os (enc@[n])
and goutput_tref env os (x:ILTypeRef) = 
  goutput_type_name_ref env os (x.Scope,x.Enclosing,x.Name)

and goutput_typ env os ty =
  match ty with 
  | Type_boxed tr ->  goutput_tspec env os tr
  | Type_tyvar tv ->  
      (* Special rule to print method type variables in Generic EE preferred form *)
      (* when an environment is available to help us do this. *)
      let cgparams = env.ppenvClassFormals in 
      let mgparams = env.ppenvMethodFormals in 
      if int tv < cgparams then begin
        output_string os "!";
        output_tyvar os tv
      end else if int tv -  cgparams <  mgparams then begin
        output_string os "!!";
        output_int os (int tv -  cgparams);
      end else begin
        output_string os "!";
        output_tyvar os tv;
        output_int os (int tv)
      end
  | Type_byref typ -> goutput_typ env os typ; output_string os "&"
  | Type_ptr typ ->  goutput_typ env os typ; output_string   os "*"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_SByte.Name ->  output_string os "int8" 
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_Int16.Name ->  output_string os "int16"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_Int32.Name ->  output_string os "int32"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_Int64.Name ->  output_string os "int64"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_IntPtr.Name ->  output_string os "native int"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_Byte.Name ->  output_string os "unsigned int8" 
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_UInt16.Name ->  output_string os "unsigned int16"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_UInt32.Name ->  output_string os "unsigned int32"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_UInt64.Name ->  output_string os "unsigned int64"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_UIntPtr.Name ->  output_string os "native unsigned int"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_Double.Name ->  output_string os "float64"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_Single.Name ->  output_string os "float32"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_Bool.Name ->  output_string os "bool"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_Char.Name ->  output_string os "char"
  | Type_value tspec when tspec.Name = ecmaILGlobals.tspec_TypedReference.Name ->  output_string os "refany"
  | Type_value tspec ->
      output_string os "value class ";
      goutput_tref env os tspec.TypeRef;
      output_string os " ";
      goutput_gactuals env os tspec.GenericArgs
  | Type_void ->  output_string os "void"
  | Type_array (bounds,ty) -> 
      goutput_typ env os ty;
      output_string os "[";
      output_arr_bounds os bounds;
      output_string os "]";
  | Type_fptr csig ->
      output_string os "method ";
      goutput_typ env os csig.ReturnType;
      output_string os " *(";
      output_list "," (goutput_typ env) os csig.ArgTypes;
      output_string os ")"
  | _ -> output_string os "NaT"
  
and output_tyvar os d =  
  output_u16 os d; ()

and goutput_ldtoken_info env os = function
  | Token_type x -> goutput_typ env os x
  | Token_method x -> output_string os "method "; goutput_mspec env os x
  | Token_field x -> output_string os "field "; goutput_fspec env os x

and goutput_typ_with_shortened_class_syntax env os = function
    Type_boxed tspec when tspec.GenericArgs = mk_empty_gactuals -> 
      goutput_tref env os tspec.TypeRef
  | typ2 -> goutput_typ env os typ2

and goutput_gactuals env os inst = 
  if inst = [] then () 
  else begin 
    output_string os "<";
    goutput_gactual env os (List.hd inst);
    List.iter (fun x -> output_string os ", "; goutput_gactual env os x) (List.tl inst);
    output_string os ">";
  end 

and goutput_gactual env os ty = goutput_typ env os ty

and goutput_tspec env os tspec = 
      output_string os "class ";
      goutput_tref env os tspec.TypeRef;
      output_string os " ";
      goutput_gactuals env os tspec.GenericArgs;

and output_arr_bounds os = function 
  | bounds when bounds = Rank1ArrayShape -> ()
  | ILArrayShape l ->
      output_list "," 
  (fun os -> function
    | (None,None)  -> output_string os ""
    | (None,Some sz) -> 
        output_int os sz
    | (Some lower,None) -> 
        output_int os lower; 
        output_string os " ... "
    | (Some lower,Some d) -> 
        output_int os lower;
        output_string os " ... ";
        output_int os d)
  os 
  l
  
and goutput_permission env os p = 
  let output_security_action  os x = 
    output_string os 
      (match x with 
    | SecAction_request ->  "request"
    | SecAction_demand ->  "demand"
    | SecAction_assert->  "assert"
    | SecAction_deny->  "deny"
    | SecAction_permitonly->  "permitonly"
    | SecAction_linkcheck->  "linkcheck"
    | SecAction_inheritcheck->  "inheritcheck"
    | SecAction_reqmin->  "reqmin"
    | SecAction_reqopt->  "reqopt"
    | SecAction_reqrefuse->  "reqrefuse"
    | SecAction_prejitgrant->  "prejitgrant"
    | SecAction_prejitdeny->  "prejitdeny"
    | SecAction_noncasdemand->  "noncasdemand"
    | SecAction_noncaslinkdemand->  "noncaslinkdemand"
    | SecAction_noncasinheritance->  "noncasinheritance" 
    | SecAction_linkdemandchoice -> "linkdemandchoice"
    | SecAction_inheritancedemandchoice -> "inheritancedemandchoice"
    | SecAction_demandchoice -> "demandchoice") in 



  begin match p with 
  | PermissionSet (sa,b) -> 
      output_string os " .permissionset ";
      output_security_action os sa ;
      output_string os " = (" ;
      output_bytes os b ;
      output_string os ")" ;
  end
  
and goutput_security_decls env os ps =  output_list " " (goutput_permission env)  os (dest_security_decls ps)

and goutput_gparam env os gf =  
  output_string os (tyvar_generator gf.gpName);
  output_parens (output_list "," (goutput_typ env)) os gf.gpConstraints

and goutput_gparams env os b = 
  if b = [] then () 
  else begin output_string os "<"; output_list "," (goutput_gparam env) os b;  output_string os ">"; () end

and output_bcc os bcc =
  output_string os  
    (match bcc with 
    | CC_fastcall -> "fastcall "
    | CC_stdcall -> "stdcall "
    | CC_thiscall -> "thiscall "
    | CC_cdecl -> "cdecl "
    | CC_default -> " "
    | CC_vararg -> "vararg ")

and output_callconv os (Callconv (hasthis,cc)) = 
  output_string os  
    (match hasthis with 
      CC_instance -> "instance " 
    | CC_instance_explicit -> "explicit "
    | CC_static -> "") ;
  output_bcc os cc

and goutput_dlocref env os = function 
  | dref when 
       is_tref_typ dref && 
       is_toplevel_tname (tref_of_typ dref).Name &&
       (tref_of_typ dref).Scope = ScopeRef_local -> 
   ()
  | dref when 
       is_tref_typ dref && 
       is_toplevel_tname (tref_of_typ dref).Name ->
   goutput_scoref env os (tref_of_typ dref).Scope;
   output_string os "::"
  | ty ->goutput_typ_with_shortened_class_syntax env os ty;  output_string os "::" 

and goutput_callsig env os (csig:ILCallingSignature) =
  output_callconv os csig.CallingConv;
  output_string os " ";
  goutput_typ env os csig.ReturnType;
  output_parens (output_list "," (goutput_typ env)) os csig.ArgTypes

and goutput_mref env os (mref:ILMethodRef) =
  output_callconv os mref.CallingConv;
  output_string os " ";
  goutput_typ_with_shortened_class_syntax env os mref.ReturnType;
  output_string os " ";
  (* no quotes for ".ctor" *)
  let name = mref.Name in 
  begin if name = ".ctor" or name = ".cctor" then output_string os name else output_id os name; end;
  output_parens (output_list "," (goutput_typ env)) os mref.ArgTypes

and goutput_mspec env os (mspec:ILMethodSpec) = 
  let fenv = 
    ppenv_enter_method mspec.GenericArity
      (ppenv_enter_tdef (gparams_of_inst (inst_of_typ mspec.EnclosingType)) env) in 
  output_callconv os mspec.CallingConv;
  output_string os " ";
  goutput_typ fenv os mspec.FormalReturnType;
  output_string os " ";
  goutput_dlocref env os mspec.EnclosingType;
  output_string os " ";
  let name = mspec.Name in 
  begin if name = ".ctor" or name = ".cctor" then output_string os name else output_id os name; end;
  goutput_gactuals env os mspec.GenericArgs;
  output_parens (output_list "," (goutput_typ fenv)) os mspec.FormalArgTypes;

and goutput_vararg_mspec env os (mspec, varargs) =
   match varargs with 
   | None -> goutput_mspec env os mspec
   | Some varargs' -> 
       let fenv = 
         ppenv_enter_method mspec.GenericArity
           (ppenv_enter_tdef (gparams_of_inst (inst_of_typ mspec.EnclosingType)) env) in 
       output_callconv os mspec.CallingConv;
       output_string os " ";
       goutput_typ fenv os mspec.FormalReturnType;
       output_string os " ";
       goutput_dlocref env os mspec.EnclosingType;
       let name = mspec.Name in 
       begin if name = ".ctor" or name = ".cctor" then output_string os name else output_id os name; end;
       goutput_gactuals env os mspec.GenericArgs;
       output_string os "(";
       output_list "," (goutput_typ fenv) os mspec.FormalArgTypes;
       output_string os ",...,";
       output_list "," (goutput_typ fenv) os varargs';
       output_string os ")";

and goutput_vararg_sig env os (csig:ILCallingSignature,varargs) =
   match varargs with 
   | None -> goutput_callsig env os csig; ()
   | Some varargs' -> 
       goutput_typ env os csig.ReturnType; 
       output_string os " (";
       let argtys = csig.ArgTypes in 
       if argtys = [] then () else begin
   goutput_typ env os (List.hd argtys);
   List.iter (fun ty -> output_string os ","; goutput_typ env os ty) (List.tl argtys);
       end;
       output_string os ",...,"; 
       output_list "," (goutput_typ env) os varargs';
       output_string os ")"; 

and goutput_fspec env os (x:ILFieldSpec) =
  let fenv = ppenv_enter_tdef (gparams_of_inst (inst_of_typ x.EnclosingType)) env in 
  goutput_typ fenv os x.FormalType;
  output_string os " ";
  goutput_dlocref env os x.EnclosingType;
  output_id os x.Name
    
let output_member_access os access = 
  output_string os 
  (match access with 
  | MemAccess_public -> "public"
  | MemAccess_private  -> "private"
  | MemAccess_compilercontrolled  -> "privatescope"
  | MemAccess_family  -> "family"
  | MemAccess_famandassem -> "famandassem"
  | MemAccess_famorassem -> "famorassem"
  | MemAccess_assembly -> "assembly")

let output_type_access os access = 
  match access with 
  | TypeAccess_public -> output_string os "public"
  | TypeAccess_private  -> output_string os "private"
  | TypeAccess_nested  ILMemberAccess -> output_string os "nested "; output_member_access os ILMemberAccess

let output_encoding os e = 
  match e with 
  | TypeEncoding_ansi -> output_string os " ansi "
  | TypeEncoding_autochar  -> output_string os " autochar "
  | TypeEncoding_unicode -> output_string os " unicode "
let output_field_init os = function
  | FieldInit_string s -> output_string os "= "; output_string os s
  | FieldInit_bool x-> output_string os "= bool"; output_parens output_string os (if x then "true" else "false")
  | FieldInit_char x-> output_string os "= char"; output_parens output_u16 os x
  | FieldInit_int8 x-> output_string os "= int8"; output_parens output_i8 os x
  | FieldInit_int16 x-> output_string os "= int16"; output_parens output_i16 os x
  | FieldInit_int32 x-> output_string os "= int32"; output_parens output_i32 os x
  | FieldInit_int64 x-> output_string os "= int64"; output_parens output_i64 os x
  | FieldInit_uint8 x-> output_string os "= uint8"; output_parens output_u8 os x
  | FieldInit_uint16 x-> output_string os "= uint16"; output_parens output_u16 os x
  | FieldInit_uint32 x-> output_string os "= uint32"; output_parens output_u32 os x
  | FieldInit_uint64 x-> output_string os "= uint64"; output_parens output_u64 os x
  | FieldInit_single x-> output_string os "= float32"; output_parens output_ieee32 os x
  | FieldInit_double x-> output_string os "= float64"; output_parens output_ieee64 os x
  | FieldInit_ref-> output_string os "= nullref" 

let output_at os b =
   Printf.fprintf os " at (* no labels for data available, data = %a *)" (output_parens output_bytes) b

let output_option f os = function None -> () | Some x -> f os x
    
let goutput_alternative_ref env os alt = 
  output_id os (name_of_alt alt); output_parens (output_list "," (fun os fdef -> goutput_typ env os fdef.fdType)) os (Array.to_list (fdefs_of_alt alt))

let goutput_curef env os (IlxUnionRef(tref,alts,_)) =
  output_string os " .classunion import ";
  goutput_tref env os tref;
  output_parens (output_list "," (goutput_alternative_ref env)) os (Array.to_list alts)
    
let goutput_cuspec env os (IlxUnionSpec(IlxUnionRef(tref,_,_),i)) =
  output_string os "class /* classunion */ ";
  goutput_tref env os  tref;
  goutput_gactuals env os i

let goutput_cloref env os (IlxClosureRef(tref,_,fvs)) =
  output_string os " .closure import ";
  goutput_tref env os tref;
  output_parens (output_list "," (fun os fv -> goutput_typ env os (typ_of_freevar fv))) os fvs;
  output_string os "{ /* closure-ref - REVIEW */ }"
    
let goutput_clospec env os (IlxClosureSpec(IlxClosureRef(tref,_,_) as cloref,i) as clospec) =
  output_string os "class /* closure */ ";
  goutput_cloref env os cloref;

  goutput_tref env os tref;
  goutput_gactuals env os i

let output_basic_type os x = 
  output_string os 
    (match x with
    | DT_I1 ->  "i1"
    | DT_U1 ->  "u1"
    | DT_I2 ->  "i2"
    | DT_U2 ->  "u2"
    | DT_I4 -> "i4"
    | DT_U4 -> "u4"
    | DT_I8 -> "i8"
    | DT_U8 -> "u8"
    | DT_R4 -> "r4"
    | DT_R8 -> "r8"
    | DT_R  -> "r"
    | DT_I  -> "i"
    | DT_U  -> "u"
    | DT_REF  -> "ref")

let output_custom_attr_data os data = 
  output_string os " = "; output_parens output_bytes os data
      
let goutput_custom_attr env os attr =
  output_string os " .custom ";
  goutput_mspec env os attr.customMethod;
  output_custom_attr_data os attr.customData

let goutput_custom_attrs env os attrs =
  List.iter (fun attr -> goutput_custom_attr env os attr;  output_string os "\n" ) (dest_custom_attrs attrs)

let goutput_fdef tref env os fd =
  output_string os " .field ";
  begin match fd.fdOffset with Some i -> output_string os "["; output_i32 os i; output_string os "] " | None -> () end;
  begin match fd.fdMarshal with Some i -> output_string os "// marshal attribute not printerd\n"; | None -> () end;  (* REVIEW *)
  output_member_access os fd.fdAccess;
  output_string os " ";
  if fd.fdStatic then output_string os " static ";
  if fd.fdLiteral then output_string os " literal ";
  if fd.fdSpecialName then output_string os " specialname rtspecialname ";
  if fd.fdInitOnly then output_string os " initonly ";
  if fd.fdNotSerialized then output_string os " notserialized ";
  goutput_typ env os fd.fdType;
  output_string os " ";
  output_id os fd.fdName;
  output_option output_at os  fd.fdData; 
  output_option output_field_init os fd.fdInit;
  output_string os "\n";
  goutput_custom_attrs env os fd.fdCustomAttrs


let output_alignment os =  function
    Aligned -> ()
  | Unaligned_1 -> output_string os "unaligned. 1 "
  | Unaligned_2 -> output_string os "unaligned. 2 "
  | Unaligned_4 -> output_string os "unaligned. 4 "

let output_volatility os =  function
    Nonvolatile -> ()
  | Volatile -> output_string os "volatile. "
let output_tailness os =  function
  | Tailcall when !emit_tailcalls -> output_string os "tail. "
  | Tailcall  -> output_string os " /* tail. */ "
  | _ -> ()
let output_after_tailcall os =  function
  | Tailcall  -> output_string os " ret "
  | _ -> ()
let rec goutput_apps env os =  function
  | Apps_tyapp (actual,cs) -> 
      output_angled (goutput_gactual env) os actual;
      output_string os " ";
      output_angled (goutput_gparam env) os (gparam_of_gactual actual) ;
      output_string os " ";
      goutput_apps env os cs
  | Apps_app(ty,cs) ->  
      output_parens (goutput_typ env) os ty;
      output_string os " ";
      goutput_apps env os cs
  | Apps_done ty ->  
      output_string os "--> "; 
      goutput_typ env os ty

/// utilities to help print out short forms of instructions
let output_short_u16 os (x:uint16) =
     if int x < 256 then (output_string os ".s "; output_u16 os x)
     else (output_string os " "; output_u16 os x)
let output_short_i32 os i32 =
     if  i32 < 256 && 0 >= i32 then (output_string os ".s "; output_i32 os i32)
     else (output_string os " "; output_i32 os i32 )

let output_code_label os lab = 
  output_string os (string_of_code_label lab)

let goutput_local env os l = 
  goutput_typ env os l.localType;
  if l.localPinned then output_string os " pinned"

let goutput_param env os l = 
 (* REVIEW: more stuff  *)
  begin match l.paramName with 
      None ->  goutput_typ env os l.paramType;
    | Some n -> goutput_typ env os l.paramType; output_string os " "; output_sqstring os n
  end

let goutput_params env os ps = 
  output_parens (output_list "," (goutput_param env)) os ps

let goutput_freevar env os l = 
  goutput_typ env os l.fvType; output_string os " "; output_sqstring os l.fvName 

let goutput_freevars env os ps = 
  output_parens (output_list "," (goutput_freevar env)) os ps

let output_source os (s:ILSourceMarker) = 
  if s.Document.File = "" then () else begin
    output_string os " .line ";
    output_int os s.Line;
    if s.Column <> -1 then begin 
      output_string os " : ";
      output_int os s.Column;
    end;
    output_string os " /* - ";
    output_int os s.EndLine;
    if s.Column <> -1 then begin 
      output_string os " : ";
      output_int os s.EndColumn;
    end;
    output_string os "*/ ";
    output_sqstring os s.Document.File
  end


let rec goutput_instr env os inst =
  match inst with
  | si when is_noarg_instr si ->
       output_lid os (words_of_noarg_instr si)
  | I_brcmp (cmp,tg1,tg2)  -> 
      output_string os 
  begin match cmp with 
  | BI_beq -> "beq"
  | BI_bgt -> "bgt"
  | BI_bgt_un -> "bgt.un"
  | BI_bge -> "bge"
  | BI_bge_un -> "bge.un"
  | BI_ble -> "ble"
  | BI_ble_un -> "ble.un"
  | BI_blt -> "blt"
  | BI_blt_un -> "blt.un"
  | BI_bne_un -> "bne.un"
  | BI_brfalse -> "brfalse"
  | BI_brtrue -> "brtrue"
  end;
      output_string os " "; 
      output_code_label os tg1
  | I_br  tg -> output_string os "/* br "; output_code_label os tg;  output_string os "*/"; 
  | I_leave tg  -> output_string os "leave "; output_code_label os tg
  | I_call  (tl,mspec,varargs)  -> 
      output_tailness os tl;
      output_string os "call ";
      goutput_vararg_mspec env os (mspec,varargs);
      output_after_tailcall os tl;
  | I_calli (tl,mref,varargs) -> 
      output_tailness os tl;
      output_string os "calli ";
      goutput_vararg_sig env os (mref,varargs);
      output_after_tailcall os tl;
  | I_ldarg u16 -> output_string os "ldarg"; output_short_u16 os u16
  | I_ldarga  u16 -> output_string os "ldarga "; output_u16 os u16
  | I_arith (AI_ldc (dt, NUM_I4 x)) -> 
      output_string os "ldc."; output_basic_type os dt; output_short_i32 os x
  | I_arith (AI_ldc (dt, NUM_I8 x)) -> 
      output_string os "ldc."; output_basic_type os dt; output_string os " "; output_i64 os x
  | I_arith (AI_ldc (dt, NUM_R4 x)) -> 
      output_string os "ldc."; output_basic_type os dt; output_string os " "; output_ieee32 os x
  | I_arith (AI_ldc (dt, NUM_R8 x)) -> 
      output_string os "ldc."; output_basic_type os dt; output_string os " "; output_ieee64 os x
  | I_ldftn mspec ->  output_string os "ldftn "; goutput_mspec env os mspec
  | I_ldvirtftn mspec -> output_string os "ldvirtftn "; goutput_mspec env os mspec
  | I_ldind (al,vol,dt) -> 
      output_alignment os al; 
      output_volatility os vol;
      output_string os "ldind.";
      output_basic_type os dt 
  | I_cpblk (al,vol)  -> 
      output_alignment os al; 
      output_volatility os vol;
      output_string os "cpblk"
  | I_initblk (al,vol)  -> 
      output_alignment os al; 
      output_volatility os vol;
      output_string os "initblk"
  | I_ldloc u16 -> output_string os "ldloc"; output_short_u16 os u16
  | I_ldloca  u16 -> output_string os "ldloca "; output_u16 os u16
  | I_starg u16 -> output_string os "starg "; output_u16 os u16
  | I_stind (al,vol,dt) -> 
      output_alignment os al; 
      output_volatility os vol;
      output_string os "stind.";
      output_basic_type os dt 
  | I_stloc u16 -> output_string os "stloc"; output_short_u16 os u16
  | I_switch (l,dflt) -> output_string os "switch "; output_parens (output_list "," output_code_label) os l
  | I_callvirt  (tl,mspec,varargs) -> 
      output_tailness os tl;
      output_string os "callvirt ";
      goutput_vararg_mspec env os (mspec,varargs);
      output_after_tailcall os tl;
  | I_callconstraint  (tl,ty,mspec,varargs) -> 
      output_tailness os tl;
      output_string os "constraint. ";
      goutput_typ env os ty;
      output_string os " callvirt ";
      goutput_vararg_mspec env os (mspec,varargs);
      output_after_tailcall os tl;
  | I_castclass ty  -> output_string os "castclass "; goutput_typ env os ty
  | I_isinst  ty  -> output_string os "isinst "; goutput_typ env os ty
  | I_ldfld (al,vol,fspec)  -> 
      output_alignment os al; 
      output_volatility os vol;
      output_string os "ldfld ";
      goutput_fspec env os fspec
  | I_ldflda  fspec -> 
      output_string os "ldflda " ;
      goutput_fspec env os fspec
  | I_ldsfld  (vol,fspec) -> 
      output_volatility os vol;
      output_string os "ldsfld ";
      goutput_fspec env os fspec
  | I_ldsflda fspec -> 
      output_string os "ldsflda ";
      goutput_fspec env os fspec
  | I_stfld (al,vol,fspec)  -> 
      output_alignment os al; 
      output_volatility os vol;
      output_string os "stfld ";
      goutput_fspec env os fspec
  | I_stsfld  (vol,fspec) -> 
      output_volatility os vol;
      output_string os "stsfld ";
      goutput_fspec env os fspec
  | I_ldtoken  tok  -> output_string os "ldtoken ";  goutput_ldtoken_info env os tok 
  | I_refanyval ty  -> output_string os "refanyval "; goutput_typ env os ty
  | I_refanytype  -> output_string os "refanytype"
  | I_mkrefany  typ -> output_string os "mkrefany "; goutput_typ env os typ
  | I_ldstr s -> 
      output_string os "ldstr "; 
      output_string os s
  | I_newobj  (mspec,varargs) -> 
      (* newobj: IL has a special rule that the CC is always implicitly "instance" and need *)
      (* not be mentioned explicitly *)
      output_string os "newobj "; 
      goutput_vararg_mspec env os (mspec,varargs)
  | I_stelem    dt      -> output_string os "stelem."; output_basic_type os dt 
  | I_ldelem    dt      -> output_string os "ldelem."; output_basic_type os dt 

  | I_newarr    (shape,typ) -> 
      if shape = Rank1ArrayShape then begin
        output_string os "newarr "; 
        goutput_typ_with_shortened_class_syntax env os typ
      end else begin
        output_string os "newobj void ";
        goutput_dlocref env os (mk_array_ty(typ,shape));
        output_string os ".ctor";
        let rank = shape.Rank in 
        output_parens (output_list "," (goutput_typ env)) os (Array.to_list (Array.create ( rank) ecmaILGlobals.typ_int32))
      end
  | I_stelem_any (shape,dt)     -> 
      if shape = Rank1ArrayShape then begin
        output_string os "stelem.any "; goutput_typ env os dt 
      end else begin
        output_string os "call instance void ";
        goutput_dlocref env os (mk_array_ty(dt,shape));
        output_string os "Set";
        let rank = shape.Rank in 
        output_parens (output_list "," (goutput_typ env)) os (Array.to_list (Array.create ( rank) ecmaILGlobals.typ_int32) @ [dt])
      end
  | I_ldelem_any (shape,tok) -> 
      if shape = Rank1ArrayShape then begin
        output_string os "ldelem.any "; goutput_typ env os tok 
      end else begin
        output_string os "call instance ";
        goutput_typ env os tok;
        output_string os " ";
        goutput_dlocref env os (mk_array_ty(tok,shape));
        output_string os "Get";
        let rank = shape.Rank in 
        output_parens (output_list "," (goutput_typ env)) os (Array.to_list (Array.create ( rank) ecmaILGlobals.typ_int32))
      end
  | I_ldelema   (ro,shape,tok)  -> 
      if ro = ReadonlyAddress then output_string os "readonly. ";
      if shape = Rank1ArrayShape then begin
        output_string os "ldelema "; goutput_typ env os tok 
      end else begin
        output_string os "call instance ";
        goutput_typ env os (Type_byref tok);
        output_string os " ";
        goutput_dlocref env os (mk_array_ty(tok,shape));
        output_string os "Address";
        let rank = shape.Rank in 
        output_parens (output_list "," (goutput_typ env)) os (Array.to_list (Array.create ( rank) ecmaILGlobals.typ_int32))
      end
  | I_box       tok     -> output_string os "box "; goutput_typ env os tok
  | I_unbox     tok     -> output_string os "unbox "; goutput_typ env os tok
  | I_unbox_any tok     -> output_string os "unbox.any "; goutput_typ env os tok
  | I_initobj   tok     -> output_string os "initobj "; goutput_typ env os tok
  | I_ldobj (al,vol,tok)        -> 
      output_alignment os al; 
      output_volatility os vol;
      output_string os "ldobj "; 
      goutput_typ env os tok
  | I_stobj  (al,vol,tok) -> 
      output_alignment os al; 
      output_volatility os vol;
      output_string os "stobj "; 
      goutput_typ env os tok
  | I_cpobj tok -> output_string os "cpobj "; goutput_typ env os tok
  | I_sizeof  tok -> output_string os "sizeof "; goutput_typ env os tok
  | I_seqpoint  s -> output_source os s
  |  (EI_ilzero ty) -> output_string os "ilzero "; goutput_typ env os ty
  | I_other e when is_ilx_ext_instr e -> 
      begin match (dest_ilx_ext_instr e) with 
      | (EI_castdata (check,ty,n))      -> 
          if not check then output_string os "/* unchecked. */ ";
          output_string os "castdata ";
          goutput_cuspec env os ty;
          output_string os ",";
          output_int os n
      | (EI_isdata (ty,n))  -> 
          output_string os "isdata "; 
          goutput_cuspec env os ty; 
          output_string os ",";  
          output_int os n
      |  (EI_brisdata (ty,n,tg1,_)) -> 
          output_string os "brisdata "; 
          goutput_cuspec env os ty; 
          output_string os ",";  
          output_string os "(";  
          output_int os n;
          output_string os ",";  
          output_code_label os tg1;
          output_string os ")"
      | (EI_lddata (ty,n,m))  -> 
          output_string os "lddata "; 
          goutput_cuspec env os ty; 
          output_string os ",";  
          output_int os n; 
          output_string os ","; 
          output_int os m
      | (EI_lddatatag ty) -> 
          output_string os "lddatatag "; 
          goutput_cuspec env os ty
      |  (EI_stdata (ty,n,m)) -> 
          output_string os "stdata "; 
          goutput_cuspec env os ty; 
          output_string os ",";  
          output_int os n; 
          output_string os ","; 
          output_int os m
      |  (EI_newdata (ty,n))  -> 
          output_string os "newdata "; 
          goutput_cuspec env os ty; 
          output_string os ",";  
          output_int os n
      |  (EI_datacase (complete,ty,l,_))  -> 
          output_string os (if complete then "datacase" else "dataswitch");
          output_string os " ";  
          goutput_cuspec env os ty;
          output_string os ",";  
          output_parens (output_list "," (fun os (x,y) -> output_int os x;  output_string os ",";  output_code_label os y)) os l
      |  (EI_ldenv n) -> output_string os "ldenv ";  output_int os n
      |  (EI_stenv n) -> output_string os "stenv ";  output_int os n
      |  (EI_ldenva n) -> output_string os "ldenva ";  output_int os n
      |  (EI_newclo clospec) -> output_string os "newclo "; goutput_clospec env os clospec
      |  (EI_isclo clospec) -> output_string os "isclo "; goutput_clospec env os clospec
      |  (EI_stclofld (clospec,n)) -> output_string os "stclofld "; goutput_clospec env os clospec;  output_string os " "; output_int os n
      |  (EI_castclo clospec) -> output_string os "castclo "; goutput_clospec env os clospec
      |  (EI_callclo (tl,clospec,apps)) ->  
          output_tailness os tl; 
          output_string os "callclo "; 
          goutput_clospec env os clospec;
          output_string os ", "; 
          goutput_apps env os apps;
          output_after_tailcall os tl;
      |  (EI_callfunc (tl,cs)) -> 
          output_tailness os tl; 
          output_string os "callfunc "; 
          goutput_apps env os cs;
          output_after_tailcall os tl;
      | (EI_ldftn_then_call (mr1,(tl,mr2,varargs))) -> 
          output_string os "/* ldftn_then_call */ ldftn ";
          goutput_mspec env os mr1;
          output_string os " "; 
          goutput_instr env os (I_call (tl,mr2,varargs))
      | (EI_ld_instance_ftn_then_newobj (mr1,_,(mr2,varargs)))  -> 
          output_string os "/* ld_instance_ftn_then_newobj */ ldftn ";
          goutput_mspec env os mr1;
          output_string os " "; 
          goutput_instr env os (I_newobj (mr2,varargs))
      end
  | si -> 
      output_string os "<printing for this instruction is not implemented>"

let output_if b f os x = if b then f os x else ()

let goutput_ilmbody env os il =
  if il.ilZeroInit then output_string os " .zeroinit\n";
  (* Add one to .maxstack if doing "calli" testing . *)
  output_string os " .maxstack ";
  output_i32 os (if !tailcall_via_ldftn or !call_via_ldftn then il.ilMaxStack+1 else il.ilMaxStack);
  output_string os "\n";
  let output_susp os susp = 
    match susp with
    | Some s -> 
        output_string os "\nbr "; output_code_label os s; output_string os "\n" 
    | _ -> () in 
  let commit_susp os susp lab = 
    match susp with
    | Some s when s <> lab -> output_susp os susp
    | _ -> () in 
  if il.ilLocals <> [] then begin
    output_string os " .locals(";
    goutput_local env os (List.hd il.ilLocals); 
    List.iter (fun l -> output_string os ",\n"; goutput_local env os l) (List.tl il.ilLocals); 
    output_string os ")\n"
  end;
  (* Print the code by left-to-right traversal *)
  begin 
    let rec goutput_block env os (susp,block) = 
      match block with 
      | ILBasicBlock bb ->  
    commit_susp os susp bb.bblockLabel;
    output_code_label os bb.bblockLabel; output_string os ": \n"  ;
    Array.iter (fun i -> goutput_instr env os i; output_string os "\n") bb.bblockInstrs;
    (fallthrough_of_bblock bb)
      | GroupBlock (_,l) -> 
    let new_susp = ref susp in
    List.iter (fun c -> new_susp := goutput_code env os (!new_susp,c)) l;
    !new_susp
      | RestrictBlock (_,c) -> goutput_code env os (susp,c)
      | TryBlock (c,seh) -> 

(* REVIEW: The code entry need not be unique.  
 Take into account the second half of the following.

ECMA: 
  There are only two ways to enter a try block from outside its lexical body:
  - Branching to or falling into the try block’s first instruction. The branch may be made using a conditional branch, an unconditional branch, or a leave instruction. 
  - Using a leave instruction from that try’s catch block. In this case, correct CIL code may branch to any instruction within the try block, not just its first instruction, so long as that branch target is not protected by yet another try, nested withing the first.

*)

    commit_susp os susp (unique_entry_of_code c);
    output_string os " .try {\n";
    let susp = goutput_code env os (None,c) in 
    if (susp <> None) then output_string os "// warning: fallthrough at end of try\n";
    output_string os "\n}";
    begin match seh with 
      FaultBlock flt -> 
        output_string os "fault {\n";
        output_susp os (goutput_code env os (None,flt));
        output_string os "\n}"
    | FinallyBlock flt -> 
        output_string os "finally {\n";
        output_susp os (goutput_code env os (None,flt));
        output_string os "\n}";
    | FilterCatchBlock clauses -> 
        List.iter 
           (fun (flt,ctch) -> 
              match flt with 
                  | TypeFilter typ ->
          output_string os " catch ";
          goutput_typ_with_shortened_class_syntax env os typ;
          output_string os "{\n";
          output_susp os (goutput_code env os (None,ctch));
          output_string os "\n}"
      | CodeFilter fltcode -> 
          output_string os "filter {\n";
          output_susp os (goutput_code env os (None,fltcode));
          output_string os "\n} catch {\n";
          output_susp os (goutput_code env os (None,ctch));
          output_string os "\n}";)
    clauses
    end;
    None

    and goutput_code env os (susp,code) =
      goutput_block env os (susp,code) in

    let goutput_topcode env os code = 
      let final_susp = 
  goutput_code env os (Some (unique_entry_of_code code),code)  in
      (match final_susp with Some s  -> output_string os "\nbr "; output_code_label os s; output_string os "\n" | _ -> ()) in

    goutput_topcode env os il.ilCode;

  end;
;;


let goutput_mbody is_entrypoint env os md =
  begin match md.mdCodeKind with 
  | MethodCodeKind_native -> output_string os "native "
  | MethodCodeKind_il -> output_string os "cil "
  | MethodCodeKind_runtime -> output_string os "runtime "
  end;
  output_string os (if md.mdInternalCall then "internalcall " else " ");
  output_string os (if md.mdManaged then "managed " else " ");
  output_string os (if md.mdForwardRef then "forwardref " else " ");
  output_string os " \n{ \n"  ;
  goutput_security_decls env os md.mdSecurityDecls;
  goutput_custom_attrs env os md.mdCustomAttrs;
  begin match dest_mbody md.mdBody with 
    | MethodBody_il il -> goutput_ilmbody env os il
    | _ -> ()
  end;
  if is_entrypoint then output_string os " .entrypoint";
  output_string os "\n";
  output_string os "}\n"
  
let goutput_mdef env os md =
  let attrs = 
      match md.mdKind with
        | MethodKind_virtual vinfo -> 
            "virtual "^
            (if vinfo.virtFinal then "final " else "")^
            (if vinfo.virtNewslot then "newslot " else "")^
            (if vinfo.virtStrict then " strict " else "")^
            (if vinfo.virtAbstract then " abstract " else "")^
              "  "
        | MethodKind_nonvirtual ->     ""
        | MethodKind_ctor -> "rtspecialname"
        | MethodKind_static -> 
            "static "^
            (match dest_mbody md.mdBody with 
              MethodBody_pinvoke (attr) -> 
                "pinvokeimpl(\""^ attr.pinvokeWhere.Name^"\" as \""^ attr.pinvokeName ^"\""^
                begin match attr.pinvokeCallconv with 
                | PInvokeCallConvNone -> ""
                | PInvokeCallConvCdecl -> " cdecl"
                | PInvokeCallConvStdcall -> " stdcall"
                | PInvokeCallConvThiscall -> " thiscall" 
                | PInvokeCallConvFastcall -> " fastcall"
                | PInvokeCallConvWinapi -> " winapi"
                end ^
                begin match attr.PInvokeCharEncoding with 
                | PInvokeEncodingNone -> ""
                | PInvokeEncodingAnsi -> " ansi"
                | PInvokeEncodingUnicode -> " unicode"
                | PInvokeEncodingAuto -> " autochar"
                end ^
                (if attr.pinvokeNoMangle then " nomangle" else "") ^
                (if attr.pinvokeLastErr then " lasterr" else "") ^
                (* todo: PInvokeThrowOnUnmappableChar, PInvokeCharBestFit *)
                      ")"
              | _ -> 
                  "")
        | MethodKind_cctor -> "specialname rtspecialname static" in 
  let is_entrypoint = md.mdEntrypoint in 
  let menv = ppenv_enter_method (List.length md.mdGenericParams) env in 
  output_string os " .method ";
  if md.mdHideBySig then output_string os "hidebysig ";
  if md.mdReqSecObj then output_string os "reqsecobj ";
  if md.mdSpecialName then output_string os "specialname ";
  if md.mdUnmanagedExport then output_string os "unmanagedexp ";
  output_member_access os md.mdAccess;
  output_string os " ";
  output_string os attrs;
  output_string os " ";
  output_callconv os md.mdCallconv;
  output_string os " ";
  (goutput_typ menv) os md.Return.Type;
  output_string os " ";
  output_id os md.mdName ;
  output_string os " ";
  (goutput_gparams env) os md.mdGenericParams;
  output_string os " ";
  (goutput_params menv) os md.mdParams;
  output_string os " ";
  if md.mdSynchronized then output_string os "synchronized ";
  if md.mdMustRun then output_string os "/* mustrun */ ";
  if md.mdPreserveSig then output_string os "preservesig ";
  (goutput_mbody is_entrypoint menv) os md;
  output_string os "\n"

let goutput_pdef env os pd =
    output_string os  "property\n\tgetter: ";
    (match pd.propGet with None -> () | Some mref -> goutput_mref env os mref);
    output_string os  "\n\tsetter: ";
    (match pd.propSet with None -> () | Some mref -> goutput_mref env os mref)

let goutput_superclass env os = function 
    None -> ()
  | Some typ -> output_string os "extends "; (goutput_typ_with_shortened_class_syntax env) os typ

let goutput_superinterfaces env os imp =
  if imp = [] then () else
  output_string os "implements ";
  output_list "," (goutput_typ_with_shortened_class_syntax env) os imp;;

let goutput_implements env os imp =
  if imp = [] then () else
  output_string os "implements ";
  output_list "," (goutput_typ_with_shortened_class_syntax env) os imp;;

let the = function Some x -> x  | None -> failwith "the"

let output_type_layout_info os info =
  if info.typeSize <> None then (output_string os " .size "; output_i32 os (the info.typeSize));
  if info.typePack <> None then (output_string os " .pack "; output_u16 os (the info.typePack))

let split_type_layout = function
  | TypeLayout_auto -> "auto",(fun os () -> ())
  | TypeLayout_sequential info ->  "sequential", (fun os () -> output_type_layout_info os info)
  | TypeLayout_explicit info ->  "explicit", (fun os () -> output_type_layout_info os info)

      
let goutput_fdefs tref env os fdefs = 
  List.iter (fun f -> (goutput_fdef tref env) os f; output_string os "\n" ) (dest_fdefs fdefs)
let goutput_mdefs env os mdefs = 
  List.iter (fun f -> (goutput_mdef env) os f; output_string os "\n" ) (dest_mdefs mdefs)
let goutput_pdefs env os pdefs = 
  List.iter (fun f -> (goutput_pdef env) os f; output_string os "\n" ) (dest_pdefs pdefs)

let rec goutput_tdef (enc) env contents os cd =
  let env = ppenv_enter_tdef cd.tdGenericParams env in 
  let layout_attr,pp_layout_decls = split_type_layout cd.tdLayout in 
  if is_toplevel_tname cd.tdName then 
    begin 
      if contents then begin  
  let tref = (mk_nested_tref (ScopeRef_local,enc,cd.tdName)) in 
  goutput_mdefs env os cd.tdMethodDefs;
  goutput_fdefs tref env os cd.tdFieldDefs;
  goutput_pdefs env os cd.tdProperties;
    (* REVIEW: warn if top level module contains anything else?? *)
      end 
    end
  else begin
    let isclo = 
      match cd.tdKind with 
      | TypeDef_other e when is_ilx_ext_type_def_kind e ->
          begin match dest_ilx_ext_type_def_kind e with 
          | ETypeDef_closure _ ->  true
          | _ -> false
          end
      | _ -> false in 
    let isclassunion = 
      match cd.tdKind with 
      | TypeDef_other e when is_ilx_ext_type_def_kind e ->
          begin match dest_ilx_ext_type_def_kind e with 
          | ETypeDef_classunion _ ->  true
          | _ -> false
          end
      | _ -> false in 
    if not (isclo or isclassunion) or contents then begin
      output_string os "\n";
      begin match cd.tdKind with 
      | TypeDef_class | TypeDef_enum | TypeDef_delegate | TypeDef_valuetype -> output_string os ".class "
      | TypeDef_interface ->  output_string os ".class  interface "
      | TypeDef_other e when is_ilx_ext_type_def_kind e -> 
          begin match dest_ilx_ext_type_def_kind e with 
          | ETypeDef_closure _ ->  output_string os ".closure "
          | ETypeDef_classunion  _ ->  output_string os ".classunion "
          end
      | TypeDef_other _ -> failwith "unknown extension" 
      end;
      output_init_semantics os cd.tdInitSemantics;
      output_string os " ";
      output_type_access os cd.tdAccess;
      output_string os " ";
      output_encoding os cd.tdEncoding;
      output_string os " ";
      output_string os layout_attr;
      output_string os " ";
      if cd.IsSealed then  output_string os "sealed ";
      if cd.IsAbstract then  output_string os "abstract ";
      if cd.tdSerializable then  output_string os "serializable ";
      if cd.tdComInterop then  output_string os "import ";
      output_sqstring os cd.tdName ;
      goutput_gparams env os cd.tdGenericParams;
      output_string os "\n\t";
      if isclo then 
        match cd.tdKind with 
        | TypeDef_other e when is_ilx_ext_type_def_kind e ->
            begin match dest_ilx_ext_type_def_kind e with 
            | ETypeDef_closure cloinfo ->  goutput_freevars env os cloinfo.cloFreeVars
            | _ -> ()
            end 
        | _ -> ()
      else 
        begin
          goutput_superclass env os cd.tdExtends;
          output_string os "\n\t";
        end;
      goutput_implements env os cd.tdImplements;
      output_string os "\n{\n ";
      if contents then begin
        let tref = (mk_nested_tref (ScopeRef_local,enc,cd.tdName)) in 
        goutput_custom_attrs env os cd.tdCustomAttrs;
        goutput_security_decls env os cd.tdSecurityDecls;
        pp_layout_decls os ();
        goutput_fdefs tref env os cd.tdFieldDefs;
        goutput_mdefs env os cd.tdMethodDefs;
        begin match cd.tdKind with 
        | TypeDef_other e when is_ilx_ext_type_def_kind e -> 
            begin match dest_ilx_ext_type_def_kind e with 
            | ETypeDef_closure x ->  
                output_string os "\n.apply ";
                (goutput_lambdas env) os x.cloStructure;
                output_string os "\n { ";
                (goutput_ilmbody env) os (Lazy.force x.cloCode);
                output_string os "}\n";
            | ETypeDef_classunion x ->  
                Array.iter (fun x -> output_string os " .alternative "; 
                                   (goutput_alternative_ref env) os x) x.cudAlternatives;
            end
        | _ -> ()
        end;
      end;
      
      goutput_tdefs contents  (enc@[cd.tdName]) env os cd.tdNested;
      output_string os "\n}";
    end;
  end

and output_init_semantics os f =
  match f with 
    TypeInit_beforefield -> output_string os "beforefieldinit";
  | TypeInit_beforeany -> ()

and goutput_lambdas env os lambdas = 
  match lambdas with
   | Lambdas_forall (gf,l) -> 
       output_angled (goutput_gparam env) os gf; 
       output_string os " "; 
       (goutput_lambdas env) os l
   | Lambdas_lambda (ps,l) ->  
       output_parens (goutput_param env) os ps; 
       output_string os " ";
       (goutput_lambdas env) os l
   | Lambdas_return typ -> output_string os "--> "; (goutput_typ env) os typ
  
and goutput_tdefs contents (enc) env os td =
  List.iter (goutput_tdef enc env contents os) (dest_tdefs td)

let output_ver os (a,b,c,d) =
    output_string os " .ver ";
    output_u16 os a;
    output_string os " : ";
    output_u16 os b;
    output_string os " : ";
    output_u16 os c;
    output_string os " : ";
    output_u16 os d

let output_locale os s = output_string os " .Locale "; output_qstring os s

let output_hash os x = 
    output_string os " .hash = "; output_parens output_bytes os x 
let output_publickeytoken os x = 
  output_string os " .publickeytoken = "; output_parens output_bytes os x 
let output_publickey os x = 
  output_string os " .publickey = "; output_parens output_bytes os x 

let output_publickeyinfo os = function
  | PublicKey k -> output_publickey os k
  | PublicKeyToken k -> output_publickeytoken os k

let output_assref os (aref:ILAssemblyRef) =
  output_string os " .assembly extern ";
  output_sqstring os aref.Name;
 (if aref.Retargetable then output_string os " retargetable "); 
  output_string os " { ";
  (output_option output_hash) os aref.Hash;
  (output_option output_publickeyinfo) os aref.PublicKey;
  (output_option output_ver) os aref.Version;
  (output_option output_locale) os aref.Locale;
  output_string os " } "

let output_modref os (modref:ILModuleRef) =
  output_string os (if modref.HasMetadata then " .module extern " else " .file nometadata " );
  output_sqstring os modref.Name;
  (output_option output_hash) os modref.Hash

let goutput_resource env os r = 
  output_string os " .mresource ";
  output_string os (match r.resourceAccess with Resource_public -> " public " | Resource_private -> " private ");
  output_sqstring os r.resourceName;
  output_string os " { ";
  goutput_custom_attrs env os r.resourceCustomAttrs;
  begin match r.resourceWhere with 
  | Resource_local _ -> 
      output_string os " /* loc nyi */ "; 
  | Resource_file (mref,off) ->
      output_string os " .file "; 
      output_sqstring os mref.Name;
      output_string os "  at "; 
      output_i32 os off 
  | Resource_assembly aref -> 
      output_string os " .assembly extern "; 
      output_sqstring os aref.Name
  end;
  output_string os " }\n "

let goutput_manifest env os m = 
  output_string os " .assembly "; 
  begin match m.manifestLongevity with 
            | LongevityUnspecified -> ()
            | LongevityLibrary -> output_string os "library "; 
            | LongevityPlatformAppDomain -> output_string os "platformappdomain "; 
            | LongevityPlatformProcess -> output_string os "platformprocess "; 
            | LongevityPlatformSystem  -> output_string os "platformmachine "; 
  end ;
  output_sqstring os m.manifestName;
  output_string os " { \n";
  output_string os ".hash algorithm "; output_i32 os m.manifestAuxModuleHashAlgorithm; output_string os "\n";
  goutput_custom_attrs env os m.manifestCustomAttrs;
  goutput_security_decls env os m.manifestSecurityDecls;
  (output_option output_publickey) os m.manifestPublicKey;
  (output_option output_ver) os m.manifestVersion;
  (output_option output_locale) os m.manifestLocale;
  output_string os " } \n"


 (* REVIEW - other assembly data *)
(*      manifestExportedTypes: ILExportedTypes;
      manifestEntrypointElsewhere: ILScopeRef option; 
*)

let output_module_fragment_aux refs os  modul = 
  try 
    let env = mk_ppenv in 
    let env = ppenv_enter_modul env in 
    goutput_tdefs false ([]) env os modul.modulTypeDefs;
    goutput_tdefs true ([]) env os modul.modulTypeDefs;
  with e ->  
    output_string os "*** Error during printing : "; output_string os (e.ToString()); flush os;
    rethrow(); 
    raise e

let output_module_fragment os  modul = 
  let refs = refs_of_module modul in 
  output_module_fragment_aux refs os  modul;
  refs

let output_module_refs os refs = 
  let env = mk_ppenv in 
  List.iter (fun  x -> output_assref os x; output_string os "\n") refs.refsAssembly;
  List.iter (fun x -> output_modref os x; output_string os "\n") refs.refsModul
  
let goutput_module_manifest env os modul = 
  output_string os " .module "; output_sqstring os modul.modulName;
  goutput_custom_attrs env os modul.modulCustomAttrs;
  output_string os " .imagebase "; output_i32 os modul.modulImageBase;
  output_string os " .file alignment "; output_i32 os modul.modulPhysAlignment;
  output_string os " .subsystem "; output_i32 os modul.modulSubSystem;
  output_string os " .corflags "; output_i32 os ((if modul.modulILonly then 0x0001 else 0) ||| (if modul.modul32bit then 0x0002 else 0));
  List.iter (fun r -> goutput_resource env os r) (dest_resources modul.modulResources);
  output_string os "\n";
  (output_option (goutput_manifest env)) os modul.modulManifest

let output_module os  modul = 
  try 
    let refs = refs_of_module modul in 
    let env = mk_ppenv in 
    let env = ppenv_enter_modul env in 
    output_module_refs  os refs;
    goutput_module_manifest env os modul;
    output_module_fragment_aux refs os  modul;
  with e ->  
    output_string os "*** Error during printing : "; output_string os (e.ToString()); flush os;
    raise e


(* -------------------------------------------------------------------- 
 * Debug printing functions...
 * -------------------------------------------------------------------- *)

let output_scoref os x = goutput_scoref debug_ppenv os x
let output_fdef os x = goutput_fdef ecmaILGlobals.tref_Object debug_ppenv os x
let output_alternative os x = goutput_alternative_ref debug_ppenv os x
let output_curef os x = goutput_curef debug_ppenv os x
let output_cloref os x = goutput_cloref debug_ppenv os x
let output_cuspec os x = goutput_cuspec debug_ppenv os x
let output_instr os x = goutput_instr debug_ppenv os x
let output_ilmbody os x = goutput_ilmbody debug_ppenv os x
let output_mbody os x = goutput_mbody false debug_ppenv os x
let output_mdef os x = goutput_mdef debug_ppenv os x
let output_superclass os x = goutput_superclass debug_ppenv os x
let output_superinterfaces os x = goutput_superinterfaces debug_ppenv os x
let output_implements os x = goutput_implements debug_ppenv os x
let output_fdefs os x = goutput_fdefs ecmaILGlobals.tref_Object debug_ppenv os x
let output_mdefs os x = goutput_mdefs debug_ppenv os x
let output_typ os x = goutput_typ debug_ppenv os x
let output_ldtoken_info os x = goutput_ldtoken_info debug_ppenv os x
let output_typ_with_shortened_class_syntax os x = goutput_typ_with_shortened_class_syntax debug_ppenv os x
let output_gactuals os x = goutput_gactuals debug_ppenv os x
let output_tspec os x = goutput_tspec debug_ppenv os x
let output_tref os x = goutput_tref debug_ppenv os x
let output_gparam os x = goutput_gparam debug_ppenv os x
let output_gparams os x = goutput_gparams debug_ppenv os x
let output_gactual os x = goutput_gactual debug_ppenv os x
let output_dlocref os x = goutput_dlocref debug_ppenv os x
let output_callsig os x = goutput_callsig debug_ppenv os x
let output_mref os x = goutput_mref debug_ppenv os x
let output_mspec os x = goutput_mspec debug_ppenv os x
let output_vararg_mspec os x = goutput_vararg_mspec debug_ppenv os x
let output_vararg_sig os x = goutput_vararg_sig debug_ppenv os x
let output_fspec os x = goutput_fspec debug_ppenv os x
let output_apps os x = goutput_apps debug_ppenv os x
let output_tdef os x = goutput_tdef  [] debug_ppenv true os x
let output_custom_attr os x = goutput_custom_attr debug_ppenv os x
let output_module_manifest os x = goutput_module_manifest debug_ppenv os x
let output_clospec os x = goutput_clospec debug_ppenv os x

let output_sig_of_mdef os (md:ILMethodDef) =
  output_callconv os md.mdCallconv;
  output_string os " ";
  output_typ os md.Return.Type;
  output_string os " ";
  output_sqstring os md.mdName;
  output_parens (output_list "," output_typ) os md.ParameterTypes

let gen_print_module f2 outfile  modul =
  let os = open_out outfile in 
  try 
    let res = f2 os modul  in 
    close_out os;
    res
  with e -> 
    (try close_out os with _ -> ());
    rethrow(); 
    raise e

let print_module y z = gen_print_module output_module y z
  
let string_of_instr (x :ILInstr) =
    let strBuilder = new StringBuilder() in
    let strWriter  = new StringWriter(strBuilder,System.Globalization.CultureInfo.CurrentCulture) in
    let os = text_writer_to_out_channel (strWriter :> TextWriter) in
    output_instr os x;
    strBuilder.ToString()
#endif
    

    
  
