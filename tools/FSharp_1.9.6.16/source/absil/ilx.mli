// (c) Microsoft Corporation. All rights reserved

/// ILX extensions to Abstract IL types and instructions F# 
module (* internal *) Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 

open Il

(* -------------------------------------------------------------------- 
 * Union and closure references 
 * -------------------------------------------------------------------- *)

type IlxUnionAlternative = 
    { altName: string;
      altFields: ILFieldDef array;
      altCustomAttrs: ILAttributes }

and IlxUnionRef = IlxUnionRef of ILTypeRef * IlxUnionAlternative array * bool (* cudNullPermitted *)
and IlxUnionSpec = IlxUnionSpec of IlxUnionRef * ILGenericArgs

and IlxClosureLambdas = 
  | Lambdas_forall of ILGenericParameterDef * IlxClosureLambdas
  | Lambdas_lambda of ILParameter * IlxClosureLambdas
  | Lambdas_return of ILType
and IlxClosureRef = IlxClosureRef of ILTypeRef * IlxClosureLambdas * IlxClosureFreeVar list 
and IlxClosureSpec = IlxClosureSpec of IlxClosureRef * ILGenericArgs
and IlxClosureFreeVar = 
    { fvName: string ; 
      fvCompilerGenerated:bool; 
      fvType: ILType }


/// IlxClosureApps - i.e. types being applied at a callsite
type IlxClosureApps = 
  | Apps_tyapp of ILType * IlxClosureApps 
  | Apps_app of ILType * IlxClosureApps 
  | Apps_done of ILType

/// ILX extensions to the intruction set
///

type IlxInstr = 
  | EI_lddata of IlxUnionSpec * int * int
  | EI_isdata of IlxUnionSpec * int
  | EI_brisdata of IlxUnionSpec * int * ILCodeLabel * ILCodeLabel
  | EI_castdata of bool * IlxUnionSpec * int
  | EI_stdata of IlxUnionSpec * int * int
  | EI_datacase of (bool * IlxUnionSpec * (int * ILCodeLabel) list * ILCodeLabel) (* last label is fallthrough, bool is whether to leave value on the stack for each case *)
  | EI_lddatatag of IlxUnionSpec
  | EI_newdata of IlxUnionSpec * int
  | EI_newclo of IlxClosureSpec
  | EI_castclo of IlxClosureSpec
  | EI_isclo of IlxClosureSpec
  | EI_callclo of Tailcall * IlxClosureSpec * IlxClosureApps
  | EI_stclofld  of (IlxClosureSpec * int)  
  | EI_stenv  of int
  | EI_ldenv  of int
  | EI_ldenva  of int
  | EI_callfunc of Tailcall * IlxClosureApps
  | EI_ldftn_then_call of ILMethodSpec * (Tailcall * ILMethodSpec * varargs)  (* special: for internal use only *)
  | EI_ld_instance_ftn_then_newobj of ILMethodSpec * ILCallingSignature * (ILMethodSpec * varargs)  (* special: for internal use only *)

val mk_ilx_ext_instr: (IlxInstr -> IlxExtensionInstr)
val is_ilx_ext_instr: (IlxExtensionInstr -> bool)
val dest_ilx_ext_instr: (IlxExtensionInstr -> IlxInstr)

val mk_IlxInstr: IlxInstr -> ILInstr

// -------------------------------------------------------------------- 
// ILX extensions to the kinds of type definitions available
// -------------------------------------------------------------------- 

type IlxClosureInfo = 
    { cloStructure: IlxClosureLambdas;
      cloFreeVars: IlxClosureFreeVar list;  
      cloCode: (ILMethodBody Lazy.t);
      cloSource: ILSourceMarker option}

and IlxUnionInfo = 
    { /// Is the representation public? 
      cudReprAccess: ILMemberAccess; 
      /// Are the representation helpers public? 
      cudHelpersAccess: ILMemberAccess; 
      /// Generate the helpers? 
      cudHelpers: bool; 
      cudDebugProxies: bool; 
      cudDebugDisplayAttributes: ILAttribute list;
      cudAlternatives: IlxUnionAlternative array;
      cudNullPermitted: bool;
      /// Debug info for generated code for classunions 
      cudWhere: ILSourceMarker option;  
    }

type IlxTypeDefKind = 
 | ETypeDef_closure of IlxClosureInfo
 | ETypeDef_classunion of IlxUnionInfo

val mk_ilx_ext_type_def_kind: (IlxTypeDefKind -> IlxExtensionTypeKind)
val is_ilx_ext_type_def_kind: (IlxExtensionTypeKind -> bool)
val dest_ilx_ext_type_def_kind: (IlxExtensionTypeKind -> IlxTypeDefKind)

val mk_IlxTypeDefKind: IlxTypeDefKind -> ILTypeDefKind

(* -------------------------------------------------------------------- 
 * These are now obsolete
 * -------------------------------------------------------------------- *)

val mk_array_ty_old: ILArrayShape * ILType -> ILType
val gen_mk_array_ty: ILArrayShape * ILType -> ILType 
val gen_is_array_ty: ILType -> bool
val gen_dest_array_ty: ILType -> ILArrayShape * ILType 

(* -------------------------------------------------------------------- 
 * MS-ILX constructs: Closures, thunks, classunions
 * -------------------------------------------------------------------- *)
val inst_apps_aux: int -> ILGenericArgs -> IlxClosureApps -> IlxClosureApps
val dest_func_app: IlxClosureApps -> ILType * IlxClosureApps
val dest_tyfunc_app: IlxClosureApps -> ILType * IlxClosureApps

val formal_freevar_type_of_cloref : IlxClosureRef -> int -> ILType

val tref_of_clospec               : IlxClosureSpec -> ILTypeRef
val cloref_of_clospec             : IlxClosureSpec -> IlxClosureRef
val formal_freevar_type_of_clospec: IlxClosureSpec -> int -> ILType
val actual_freevar_type_of_clospec: IlxClosureSpec -> int -> ILType
val formal_freevars_of_clospec    : IlxClosureSpec -> IlxClosureFreeVar list
val actual_freevars_of_clospec    : IlxClosureSpec -> IlxClosureFreeVar list
val actual_lambdas_of_clospec     : IlxClosureSpec -> IlxClosureLambdas
val formal_lambdas_of_clospec     : IlxClosureSpec -> IlxClosureLambdas
val inst_of_clospec               : IlxClosureSpec -> ILGenericArgs

val generalize_cloref: ILGenericParameterDefs -> IlxClosureRef -> IlxClosureSpec


(* -------------------------------------------------------------------- 
 * MS-ILX: Unions
 * -------------------------------------------------------------------- *)

val objtype_of_cuspec       : IlxUnionSpec -> ILType
val inst_of_cuspec          : IlxUnionSpec -> ILGenericArgs
val alts_of_cuspec          : IlxUnionSpec -> IlxUnionAlternative list
val altsarray_of_cuspec     : IlxUnionSpec -> IlxUnionAlternative array
val tref_of_cuspec          : IlxUnionSpec -> ILTypeRef 
val nullPermitted_of_cuspec : IlxUnionSpec -> bool

val actual_typ_of_cuspec_field: IlxUnionSpec -> int -> int -> ILType
val alt_of_cuspec             : IlxUnionSpec -> int -> IlxUnionAlternative
val fdef_of_cuspec            : IlxUnionSpec -> int -> int -> ILFieldDef

val fdefs_of_alt  : IlxUnionAlternative -> ILFieldDef array
val fdef_of_alt   : IlxUnionAlternative -> int -> ILFieldDef 
val name_of_alt   : IlxUnionAlternative -> string
val alt_is_nullary: IlxUnionAlternative -> bool

val mk_freevar: string * bool * ILType -> IlxClosureFreeVar
val name_of_freevar: IlxClosureFreeVar -> string 
val typ_of_freevar: IlxClosureFreeVar -> ILType
