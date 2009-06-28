(* (c) Microsoft Corporation 2005-2008.  *)


#light

//-------------------------------------------------------------------------
// Define Initial Environment.  A bunch of types and values are hard-wired 
// into the compiler.  This lets the compiler perform particular optimizations
// for these types and values, for example emitting optimized calls for
// comparison and hashing functions.  The compiler generates the compiled code 
// for these types and values when the the --compiling-fslib switch is 
// provided when linking the FSharp.Core.dll assembly.
//------------------------------------------------------------------------- 
module Microsoft.FSharp.Compiler.Env 

open Internal.Utilities
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library

open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.PrettyNaming

let private env_range = rangeN "startup" 0

let vara = mk_rigid_typar "a" env_range
let varb = mk_rigid_typar "b" env_range
let private varc = mk_rigid_typar "c" env_range
let private vard = mk_rigid_typar "d" env_range
let private vare = mk_rigid_typar "e" env_range
let private varf = mk_rigid_typar "f" env_range
let private varg = mk_rigid_typar "g" env_range

let vara_ty = mk_typar_ty vara 
let varb_ty = mk_typar_ty varb 
let private varc_ty = mk_typar_ty varc
let private vard_ty = mk_typar_ty vard
let private vare_ty = mk_typar_ty vare
let private varf_ty = mk_typar_ty varf
let private varg_ty = mk_typar_ty varg

type public val_info = Intrinsic of NonLocalPath * string * Tast.typ
let internal vref_for_val_info (Intrinsic(mvr,nm,ty)) = mk_nonlocal_vref mvr nm  
let private vref_for_val_sinfo (mvr,nm) = mk_nonlocal_vref mvr nm  

//-------------------------------------------------------------------------
// Access the initial environment: names
//------------------------------------------------------------------------- *)

let private  lib_MFOperators_name            = lib_MF_name ^ ".Core.Operators"
let internal lib_MFOperatorsChecked_name     = lib_MF_name ^ ".Core.Operators.Checked"
let private  lib_MFControl_name              = lib_MF_name ^ ".Control"
let private  lib_MFColl_name                 = lib_MF_name ^ ".Collections"
let private  lib_MFLanguagePrimitives_name   = lib_MF_name ^ ".Core.LanguagePrimitives"
let private  lib_MFLanguagePrimitivesIntrinsicFunctions_name   = lib_MF_name ^ ".Core.LanguagePrimitives.IntrinsicFunctions"
let private  lib_MFText_name                 = lib_MF_name ^ ".Text"
let private  lib_MFIEnumerable_name          = lib_MF_name ^ ".Collections.SeqModule"
let private  lib_MFIEnumerableSequenceExpressionHelpers_name   = lib_MF_name ^ ".Collections.SequenceExpressionHelpers"

let public lib_MLLib_OCaml_name            = lib_MF_name ^ ".Compatibility.OCaml"
let public lib_MLLib_FSharp_name           = lib_MF_name ^ ""
let public lib_FSLib_Pervasives_name       = lib_MF_name ^ ".Core.ExtraTopLevelOperators" 
let public lib_MLLib_Pervasives_name       = lib_MF_name ^ ".Compatibility.OCaml.Pervasives" 
let private  lib_Quotations_name             = lib_MF_name ^ ".Quotations"
let private  lib_Quotations_ExprModule_name       = lib_MF_name ^ ".Quotations.Impl"
let private  lib_MFLanguagePrimitivesHashCompare_name = lib_MF_name ^ ".Core.LanguagePrimitives.HashCompare"
let private  lib_MFLanguagePrimitivesIntrinsicOperators_name = lib_MF_name ^ ".Core.LanguagePrimitives.IntrinsicOperators"

// for 4.0 LazyInit / Lazy type
let private lib_MFLazy_name             = lib_MFControl_name ^ ".LazyExtensions"

let private lib_MFOperators_path                     = IL.split_namespace lib_MFOperators_name |> Array.of_list
let private lib_MFOperatorsChecked_path              = IL.split_namespace lib_MFOperatorsChecked_name |> Array.of_list
let public lib_MFControl_path                       = IL.split_namespace lib_MFControl_name 
let public lib_MFColl_path                          = IL.split_namespace lib_MFColl_name 
let private lib_MFText_path                          = IL.split_namespace lib_MFText_name |> Array.of_list
let private lib_MFLanguagePrimitives_path            = IL.split_namespace lib_MFLanguagePrimitives_name |> Array.of_list
let private lib_MFLanguagePrimitivesIntrinsicFunctions_path   = IL.split_namespace lib_MFLanguagePrimitivesIntrinsicFunctions_name |> Array.of_list
let private lib_MFLanguagePrimitivesIntrinsicOperators_path   = IL.split_namespace lib_MFLanguagePrimitivesIntrinsicOperators_name |> Array.of_list
let private lib_MFLanguagePrimitivesHashCompare_path          = IL.split_namespace lib_MFLanguagePrimitivesHashCompare_name |> Array.of_list
let private lib_MFIEnumerable_path                            = IL.split_namespace lib_MFIEnumerable_name |> Array.of_list
let private lib_MFIEnumerableSequenceExpressionHelpers_path   = IL.split_namespace lib_MFIEnumerableSequenceExpressionHelpers_name |> Array.of_list
let private lib_Quotations_path                               = IL.split_namespace lib_Quotations_name |> Array.of_list
let private lib_FSLib_Pervasives_path                         = IL.split_namespace lib_FSLib_Pervasives_name |> Array.of_list

let private lib_MFLazy_path     = IL.split_namespace lib_MFLazy_name |> Array.of_list

let private lib_MF_arr_path                          = lib_MF_path |> Array.of_list
let private lib_MFCore_arr_path                      = lib_MFCore_path |> Array.of_list
let private lib_MFControl_arr_path                   = lib_MFControl_path |> Array.of_list
let private lib_MFColl_arr_path                      = lib_MFColl_path |> Array.of_list



//-------------------------------------------------------------------------
// Access the initial environment: helpers to build references
//------------------------------------------------------------------------- *)

let internal mk_top_nlpath ccu p = NLPath(ccu,p)
let private mk_mono_typ tcref = TType_app(tcref,[])
let internal mk_nonlocal_top_nlpath_tcref ccu path n = mk_nonlocal_tcref (mk_top_nlpath ccu path) n 

let internal mk_MF_tcref ccu n = mk_nonlocal_top_nlpath_tcref ccu lib_MF_arr_path n 
let internal mk_MFCore_tcref ccu n = mk_nonlocal_top_nlpath_tcref ccu lib_MFCore_arr_path n 
let internal mk_MFColl_tcref ccu n = mk_nonlocal_top_nlpath_tcref ccu lib_MFColl_arr_path n 
let internal mk_MFIEnumerableSequenceExpressionHelpers_tcref ccu n = mk_nonlocal_top_nlpath_tcref ccu lib_MFIEnumerableSequenceExpressionHelpers_path n 

let internal mk_MFControl_nlpath ccu = mk_top_nlpath ccu lib_MFControl_arr_path
let internal mk_MFControl_tcref ccu n = mk_nonlocal_tcref (mk_MFControl_nlpath ccu) n 

let internal mk_MFLanguagePrimitives_nlpath  ccu = NLPath(ccu,lib_MFLanguagePrimitives_path)
let internal mk_MFLanguagePrimitivesIntrinsicOperators_nlpath  ccu = NLPath(ccu,lib_MFLanguagePrimitivesIntrinsicOperators_path)
let internal mk_MFLanguagePrimitivesIntrinsicFunctions_nlpath  ccu = NLPath(ccu,lib_MFLanguagePrimitivesIntrinsicFunctions_path)
let internal mk_MFLanguagePrimitivesHashCompare_nlpath  ccu = NLPath(ccu,lib_MFLanguagePrimitivesHashCompare_path)
let internal mk_MFText_nlpath ccu = NLPath(ccu,lib_MFText_path)
let internal mk_MFText_tcref ccu n = (mk_nonlocal_tcref (mk_MFText_nlpath ccu) n : TyconRef)
let internal mk_MFIEnumerable_nlpath ccu = NLPath(ccu,lib_MFIEnumerable_path)
let internal mk_MFIEnumerableSequenceExpressionHelpers_nlpath ccu = NLPath(ccu,lib_MFIEnumerableSequenceExpressionHelpers_path)
let internal mk_MFOperators_nlpath ccu = NLPath(ccu,lib_MFOperators_path)
let internal mk_MFOperatorsChecked_nlpath ccu = NLPath(ccu,lib_MFOperatorsChecked_path)

let internal mk_mscorlib_tcref ccu p = 
    let a,b = split_type_name p 
    mk_nonlocal_tcref (NLPath(ccu,Array.of_list a)) b

let internal mk_mscorlib_mono_typ ccu n = mk_mono_typ(mk_mscorlib_tcref ccu n)

type public BuiltinAttribInfo =
    | AttribInfo of ILTypeRef * TyconRef 
    member this.TyconRef = let (AttribInfo(tref,tcref)) = this in tcref
    member this.TypeRef  = let (AttribInfo(tref,tcref)) = this in tref

//-------------------------------------------------------------------------
// Table of all these "globals"
//------------------------------------------------------------------------- *)

[<StructuralEquality(false); StructuralComparison(false)>]
type public TcGlobals = 
    { ilg : ILGlobals;
      ilxPubCloEnv : Pubclo.cenv;
      compilingFslib: bool;
      mlCompatibility : bool;
      directoryToResolveRelativePaths : string;
      fslibCcu: ccu; 
      sysCcu: ccu; 
      better_tcref_map: TyconRef -> tinst -> Tast.typ option;
      refcell_tcr: TyconRef;
      option_tcr : TyconRef;
      choice2_tcr : TyconRef;
      choice3_tcr : TyconRef;
      choice4_tcr : TyconRef;
      choice5_tcr : TyconRef;
      choice6_tcr : TyconRef;
      choice7_tcr : TyconRef;
      list_tcr_canon   : TyconRef;
      lazy_tcr_canon   : TyconRef; 
      
      // These have a slightly different behaviour when compiling GetFSharpCoreLibraryName 
      // hence they are 'methods' on the TcGlobals structure. 

      ucref_eq : UnionCaseRef -> UnionCaseRef -> bool;
      vref_eq  : ValRef         -> ValRef         -> bool;

      refcell_tcr_nice: TyconRef;
      option_tcr_nice : TyconRef;
      list_tcr_nice   : TyconRef; 

      format_tcr      : TyconRef;
      expr_tcr        : TyconRef;
      raw_expr_tcr        : TyconRef;
      int_tcr         : TyconRef; 
      nativeint_tcr   : TyconRef; 
      unativeint_tcr  : TyconRef;
      int32_tcr       : TyconRef;
      int16_tcr       : TyconRef;
      int64_tcr       : TyconRef;
      uint16_tcr      : TyconRef;
      uint32_tcr      : TyconRef;
      uint64_tcr      : TyconRef;
      sbyte_tcr       : TyconRef;
      decimal_tcr     : TyconRef;
      date_tcr        : TyconRef;
      pdecimal_tcr    : TyconRef;
      byte_tcr        : TyconRef;
      bool_tcr        : TyconRef;
      string_tcr      : TyconRef;
      obj_tcr         : TyconRef;
      unit_tcr_canon  : TyconRef;
      unit_tcr_nice   : TyconRef;
      exn_tcr         : TyconRef;
      char_tcr        : TyconRef;
      float_tcr       : TyconRef;
      float32_tcr     : TyconRef;
      pfloat_tcr      : TyconRef;
      pfloat32_tcr    : TyconRef;
      pint_tcr        : TyconRef;
      pint8_tcr       : TyconRef;
      pint16_tcr      : TyconRef;
      pint64_tcr      : TyconRef;
      byref_tcr       : TyconRef;
      nativeptr_tcr   : TyconRef;
      ilsigptr_tcr    : TyconRef;
      fastFunc_tcr    : TyconRef;
      array_tcr       : TyconRef;
      seq_tcr         : TyconRef;
      seq_base_tcr    : TyconRef;
      il_arr1_tcr     : TyconRef;
      il_arr2_tcr     : TyconRef;
      il_arr3_tcr     : TyconRef;
      il_arr4_tcr     : TyconRef;
      tuple1_tcr      : TyconRef;
      tuple2_tcr      : TyconRef;
      tuple3_tcr      : TyconRef;
      tuple4_tcr      : TyconRef;
      tuple5_tcr      : TyconRef;
      tuple6_tcr      : TyconRef;
      tuple7_tcr      : TyconRef;
      tuple8_tcr      : TyconRef;

      fslib_IEvent2_tcr         : TyconRef;
      fslib_IDelegateEvent_tcr: TyconRef;
      system_Nullable_tcref            : TyconRef; 
      system_GenericIComparable_tcref            : TyconRef; 
      system_GenericIEquatable_tcref            : TyconRef; 
      system_IndexOutOfRangeException_tcref : TyconRef;
      int_ty         : Tast.typ;
      nativeint_ty   : Tast.typ; 
      unativeint_ty  : Tast.typ; 
      int32_ty       : Tast.typ; 
      int16_ty       : Tast.typ; 
      int64_ty       : Tast.typ; 
      uint16_ty      : Tast.typ; 
      uint32_ty      : Tast.typ; 
      uint64_ty      : Tast.typ; 
      sbyte_ty       : Tast.typ; 
      byte_ty        : Tast.typ; 
      bool_ty        : Tast.typ; 
      string_ty      : Tast.typ; 
      obj_ty         : Tast.typ; 
      unit_ty        : Tast.typ; 
      exn_ty         : Tast.typ; 
      char_ty        : Tast.typ; 
      decimal_ty                   : Tast.typ; 
      float_ty                     : Tast.typ; 
      float32_ty                   : Tast.typ; 
      system_Array_typ             : Tast.typ; 
      system_Object_typ            : Tast.typ; 
      system_IDisposable_typ       : Tast.typ; 
      system_Value_typ             : Tast.typ; 
      system_Delegate_typ : Tast.typ;
      system_MulticastDelegate_typ : Tast.typ;
      system_Enum_typ              : Tast.typ; 
      system_Exception_typ         : Tast.typ; 
      system_Int32_typ             : Tast.typ; 
      system_String_typ            : Tast.typ; 
      system_Type_typ              : Tast.typ; 
      system_TypedReference_tcref    : TyconRef; 
      system_ArgIterator_tcref       : TyconRef; 
      system_SByte_tcref : TyconRef; 
      system_Int16_tcref : TyconRef; 
      system_Int32_tcref : TyconRef; 
      system_Int64_tcref : TyconRef; 
      system_IntPtr_tcref : TyconRef; 
      system_Bool_tcref : TyconRef; 
      system_Char_tcref : TyconRef; 
      system_Byte_tcref : TyconRef; 
      system_UInt16_tcref : TyconRef; 
      system_UInt32_tcref : TyconRef; 
      system_UInt64_tcref : TyconRef; 
      system_UIntPtr_tcref : TyconRef; 
      system_Single_tcref : TyconRef; 
      system_Double_tcref : TyconRef; 
      system_RuntimeArgumentHandle_tcref : TyconRef; 
      system_RuntimeTypeHandle_typ : Tast.typ;
      system_MarshalByRefObject_tcref : TyconRef;
      system_MarshalByRefObject_typ : Tast.typ;
      system_Array_tcref          : TyconRef;
      system_Object_tcref          : TyconRef;
      system_Void_tcref            : TyconRef;
      mk_IComparable_ty            : Tast.typ;
      mk_IConvertible_ty            : Tast.typ;
      mk_IFormattable_ty            : Tast.typ;
      mk_IStructuralComparable_ty : Tast.typ;
      mk_IStructuralEquatable_ty : Tast.typ;
      mk_IComparer_ty : Tast.typ;
      mk_IEqualityComparer_ty : Tast.typ;
      tcref_System_Collections_IComparer : TyconRef;
      tcref_System_Collections_IEqualityComparer : TyconRef;
      tcref_System_IStructuralComparable : TyconRef;
      tcref_System_IStructuralEquatable : TyconRef;
      tcref_LanguagePrimitives  : TyconRef;
      attrib_AttributeUsageAttribute   : BuiltinAttribInfo;
      attrib_ParamArrayAttribute       : BuiltinAttribInfo;
      attrib_IDispatchConstantAttribute : BuiltinAttribInfo;
      attrib_IUnknownConstantAttribute  : BuiltinAttribInfo;
      attrib_SystemObsolete            : BuiltinAttribInfo;
      attrib_DllImportAttribute        : BuiltinAttribInfo;
      attrib_NonSerializedAttribute    : BuiltinAttribInfo;
      attrib_AutoSerializableAttribute : BuiltinAttribInfo;
      attrib_StructLayoutAttribute     : BuiltinAttribInfo;
      attrib_TypeForwardedToAttribute     : BuiltinAttribInfo;
      attrib_ComVisibleAttribute       : BuiltinAttribInfo;
      attrib_ComImportAttribute        : BuiltinAttribInfo;
      attrib_FieldOffsetAttribute      : BuiltinAttribInfo;
      attrib_MarshalAsAttribute        : BuiltinAttribInfo;
      attrib_InAttribute               : BuiltinAttribInfo;
      attrib_OutAttribute              : BuiltinAttribInfo;
      attrib_OptionalAttribute         : BuiltinAttribInfo;
      attrib_ThreadStaticAttribute     : BuiltinAttribInfo;
      attrib_SpecialNameAttribute     : BuiltinAttribInfo;
      attrib_ContextStaticAttribute    : BuiltinAttribInfo;
      attrib_FlagsAttribute            : BuiltinAttribInfo;
      attrib_DefaultMemberAttribute    : BuiltinAttribInfo;
      attrib_DebuggerDisplayAttribute  : BuiltinAttribInfo;
      attrib_DebuggerTypeProxyAttribute  : BuiltinAttribInfo;
      tcref_System_Collections_Generic_IList       : TyconRef;
      tcref_System_Collections_Generic_ICollection : TyconRef;
      tcref_System_Collections_Generic_IEnumerable : TyconRef;
      tcref_System_Collections_Generic_IEnumerator : TyconRef;

      attrib_RequireQualifiedAccessAttribute        : BuiltinAttribInfo; 
      attrib_OverloadIDAttribute                    : BuiltinAttribInfo; 
      attrib_EntryPointAttribute                    : BuiltinAttribInfo; 
      attrib_DefaultAugmentationAttribute           : BuiltinAttribInfo; 
      attrib_OCamlCompatibilityAttribute            : BuiltinAttribInfo; 
      attrib_ExperimentalAttribute                  : BuiltinAttribInfo; 
      attrib_UnverifiableAttribute                  : BuiltinAttribInfo; 
      attrib_LiteralAttribute                       : BuiltinAttribInfo; 
      attrib_ConditionalAttribute                   : BuiltinAttribInfo; 
      attrib_OptionalArgumentAttribute              : BuiltinAttribInfo; 
      attrib_RequiresExplicitTypeArgumentsAttribute : BuiltinAttribInfo; 
      attrib_DefaultValueAttribute                  : BuiltinAttribInfo; 
      attrib_ClassAttribute                         : BuiltinAttribInfo; 
      attrib_InterfaceAttribute                     : BuiltinAttribInfo; 
      attrib_StructAttribute                        : BuiltinAttribInfo; 
      attrib_ReflectedDefinitionAttribute           : BuiltinAttribInfo; 
      attrib_AutoOpenAttribute                      : BuiltinAttribInfo; 
      attrib_CompilationRepresentationAttribute     : BuiltinAttribInfo; 
      attrib_CLIEventAttribute                      : BuiltinAttribInfo; 
      //attrib_PermitNullLiteralAttribute             : BuiltinAttribInfo; 
      attrib_ReferenceEqualityAttribute             : BuiltinAttribInfo; 
      attrib_StructuralEqualityAttribute            : BuiltinAttribInfo; 
      attrib_StructuralComparisonAttribute          : BuiltinAttribInfo; 
      attrib_SealedAttribute                        : BuiltinAttribInfo; 
      attrib_AbstractClassAttribute                 : BuiltinAttribInfo; 
      attrib_GeneralizableValueAttribute            : BuiltinAttribInfo;
      attrib_MeasureAttribute                       : BuiltinAttribInfo;
      attrib_MeasureableAttribute                   : BuiltinAttribInfo;
      attrib_NoDynamicInvocationAttribute           : BuiltinAttribInfo;

      
      cons_ucref : UnionCaseRef;
      nil_ucref : UnionCaseRef;
      (* These are the library values the compiler needs to know about *)
      seq_vref                  : ValRef;
      and_vref                  : ValRef;
      and2_vref                 : ValRef;
      addrof_vref               : ValRef;
      addrof2_vref              : ValRef;
      or_vref                   : ValRef;
      or2_vref                  : ValRef;
      generic_equality_inner_vref    : ValRef;
      generic_equality_withc_inner_vref : ValRef;
      generic_comparison_inner_vref   : ValRef;
      generic_comparison_withc_inner_vref   : ValRef;
      generic_comparison_outer_vref   : ValRef;
      generic_hash_inner_vref: ValRef;
      generic_hash_withc_inner_vref : ValRef;
      poly_eq_inner_vref        : ValRef;
      bitwise_or_vref           : ValRef;
      seq_info                  : val_info;
      rethrow_info              : val_info;
      rethrow_vref              : ValRef;      
      typeof_info               : val_info;
      typeof_vref               : ValRef;
      typedefof_vref            : ValRef;
      enum_vref                 : ValRef;
      new_decimal_info          : val_info;
      generic_comparison_withc_outer_info : val_info;
      generic_comparison_outer_info   : val_info;
      generic_equality_outer_info    : val_info;
      generic_equality_withc_outer_info : val_info;
      generic_hash_withc_outer_info : val_info;
      
      generic_hash_withc_tuple2_vref : ValRef;
      generic_hash_withc_tuple3_vref : ValRef;
      generic_hash_withc_tuple4_vref : ValRef;
      generic_hash_withc_tuple5_vref : ValRef;
      generic_equals_withc_tuple2_vref : ValRef;
      generic_equals_withc_tuple3_vref : ValRef;
      generic_equals_withc_tuple4_vref : ValRef;
      generic_equals_withc_tuple5_vref : ValRef;
      generic_compare_withc_tuple2_vref : ValRef;
      generic_compare_withc_tuple3_vref : ValRef;
      generic_compare_withc_tuple4_vref : ValRef;
      generic_compare_withc_tuple5_vref : ValRef;

      create_instance_info      : val_info;
      unbox_vref                : ValRef;
      unbox_fast_vref           : ValRef;
      istype_vref               : ValRef;
      istype_fast_vref          : ValRef;
      get_generic_comparer_info                : val_info;
      get_generic_equality_comparer_info                : val_info;
      unbox_info                : val_info;
      unbox_fast_info           : val_info;
      istype_info               : val_info;
      istype_fast_info          : val_info;

      dispose_info              : val_info;

      range_op_vref             : ValRef;
      arr1_lookup_vref        : ValRef;
      arr2_lookup_vref        : ValRef;
      arr3_lookup_vref        : ValRef;
      arr4_lookup_vref        : ValRef;
      seq_map_concat_vref       : ValRef;
      seq_map_concat_info       : val_info;
      seq_using_info            : val_info;
      seq_using_vref            : ValRef;
      seq_delay_info            : val_info;
      seq_delay_vref            : ValRef;
      seq_append_info           : val_info;
      seq_append_vref           : ValRef;
      seq_generated_info        : val_info;
      seq_generated_vref        : ValRef;
      seq_finally_info          : val_info;
      seq_finally_vref          : ValRef;
      seq_of_functions_info     : val_info;
      seq_of_functions_vref     : ValRef;
      seq_to_array_info         : val_info;
      seq_to_list_info          : val_info;
      seq_map_info              : val_info;
      seq_map_vref              : ValRef;
      seq_singleton_info        : val_info;
      seq_singleton_vref        : ValRef;
      seq_empty_info            : val_info;
      seq_empty_vref            : ValRef;
      new_format_info           : val_info;
      raise_info           : val_info;
      lazy_force_info           : val_info;
      lazy_create_info          : val_info;


      array_get_info             : val_info;
      generic_hash_info             : val_info;
      unpickle_quoted_info       : val_info;
      cast_quotation_info        : val_info;
      lift_value_info            : val_info;
      //splice_vref                  : ValRef;
      sprintf_vref                  : ValRef;
      splice_expr_vref                  : ValRef;
      splice_raw_expr_vref                  : ValRef;
      new_format_vref                  : ValRef;

      // A list of types that are explicitly suppressed from the F# intellisense (such as "FSharpList", "Option")
      // Note that the suppression checks for the precise name of the type
      // ("FSharpList", "List"), so the lowercase versions ("list") are visible
      suppressed_types           : TyconRef list;
      
      /// Memoization table to help minimize the number of ILSourceDocument objects we create
      memoize_file : int -> IL.ILSourceDocument;      
      
 } 


#if DEBUG
// This global is only used during debug output 
let internal global_g = ref (None : TcGlobals option)
#endif

let internal mk_tcGlobals (compilingFslib,sysCcu,ilg,fslibCcu,directoryToResolveRelativePaths,mlCompatibility,sysTypes) = 
  let int_tcr        = mk_MFCore_tcref fslibCcu "int"
  let nativeint_tcr  = mk_MFCore_tcref fslibCcu "nativeint"
  let unativeint_tcr = mk_MFCore_tcref fslibCcu "unativeint"
  let int32_tcr      = mk_MFCore_tcref fslibCcu "int32"
  let int16_tcr      = mk_MFCore_tcref fslibCcu "int16"
  let int64_tcr      = mk_MFCore_tcref fslibCcu "int64"
  let uint16_tcr     = mk_MFCore_tcref fslibCcu "uint16"
  let uint32_tcr     = mk_MFCore_tcref fslibCcu "uint32"
  let uint64_tcr     = mk_MFCore_tcref fslibCcu "uint64"
  let sbyte_tcr      = mk_MFCore_tcref fslibCcu "sbyte"
  let decimal_tcr    = mk_MFCore_tcref fslibCcu "decimal"
  let pdecimal_tcr   = mk_MFCore_tcref fslibCcu "decimal`1"
  let byte_tcr       = mk_MFCore_tcref fslibCcu "byte"
  let bool_tcr       = mk_MFCore_tcref fslibCcu "bool"
  let string_tcr     = mk_MFCore_tcref fslibCcu "string"
  let obj_tcr        = mk_MFCore_tcref fslibCcu "obj"
  let unit_tcr_canon = mk_MFCore_tcref fslibCcu "Unit"
  let unit_tcr_nice  = mk_MFCore_tcref fslibCcu "unit"
  let exn_tcr        = mk_MFCore_tcref fslibCcu "exn"
  let char_tcr       = mk_MFCore_tcref fslibCcu "char"
  let float_tcr      = mk_MFCore_tcref fslibCcu "float"  
  let float32_tcr    = mk_MFCore_tcref fslibCcu "float32"
  let pfloat_tcr     = mk_MFCore_tcref fslibCcu "float`1"  
  let pfloat32_tcr   = mk_MFCore_tcref fslibCcu "float32`1"  
  let pint_tcr       = mk_MFCore_tcref fslibCcu "int`1"  
  let pint8_tcr      = mk_MFCore_tcref fslibCcu "int8`1"  
  let pint16_tcr     = mk_MFCore_tcref fslibCcu "int16`1"  
  let pint64_tcr     = mk_MFCore_tcref fslibCcu "int64`1"  
  let byref_tcr      = mk_MFCore_tcref fslibCcu "byref`1"
  let nativeptr_tcr  = mk_MFCore_tcref fslibCcu "nativeptr`1"
  let ilsigptr_tcr   = mk_MFCore_tcref fslibCcu "ilsigptr`1"
  let array_tcr      = mk_MFCore_tcref fslibCcu "array`1"
  let fastFunc_tcr   = mk_MFCore_tcref fslibCcu "FastFunc`2"
  let lazy_tcr = 
    if sysTypes then 
        mk_mscorlib_tcref sysCcu "System.Threading.LazyInit`1" 
    else 
        mk_MFControl_tcref  fslibCcu "Lazy`1"
  let fslib_IEvent2_tcr          = mk_MFControl_tcref fslibCcu "IEvent`2"
  let fslib_IDelegateEvent_tcr = mk_MFControl_tcref fslibCcu "IDelegateEvent`1"
  let option_tcr_nice     = mk_MFCore_tcref fslibCcu "option`1"
  let list_tcr_not_nice   = mk_MFColl_tcref fslibCcu "FSharpList`1"
  let list_tcr_nice       = mk_MFColl_tcref fslibCcu "list`1"
  let seq_tcr             = mk_MFColl_tcref fslibCcu "seq`1"
  let format_tcr          = mk_MFText_tcref     fslibCcu "PrintfFormat`5" 
  let format4_tcr          = mk_MFText_tcref     fslibCcu "PrintfFormat`4" 
  let date_tcr            = mk_mscorlib_tcref sysCcu"System.DateTime"
  let IEnumerable_tcr     = mk_mscorlib_tcref sysCcu "System.Collections.Generic.IEnumerable`1"
  let IEnumerator_tcr     = mk_mscorlib_tcref sysCcu "System.Collections.Generic.IEnumerator`1"
  let expr_tcr            = mk_nonlocal_tcref (NLPath(fslibCcu,lib_Quotations_path)) "Expr`1" 
  let raw_expr_tcr        = mk_nonlocal_tcref (NLPath(fslibCcu,lib_Quotations_path)) "Expr"  
  let il_arr1_tcr = mk_MFCore_tcref fslibCcu "[]`1"
  let il_arr2_tcr    = mk_MFCore_tcref fslibCcu "[,]`1";
  let il_arr3_tcr    = mk_MFCore_tcref fslibCcu "[,,]`1";
  let il_arr4_tcr    = mk_MFCore_tcref fslibCcu "[,,,]`1";
  
  let bool_ty         = mk_mono_typ bool_tcr   
  let int_ty          = mk_mono_typ int_tcr    
  let obj_ty          = mk_mono_typ obj_tcr    
  let string_ty       = mk_mono_typ string_tcr
  let byte_ty         = mk_mono_typ byte_tcr
  let decimal_ty      = mk_mscorlib_mono_typ sysCcu "System.Decimal"
  let unit_ty         = mk_mono_typ unit_tcr_nice 
  let system_Type_typ = mk_mscorlib_mono_typ sysCcu "System.Type" 
  
  (* local helpers to build value infos *)
  let mk_byref_typ ty = TType_app(byref_tcr, [ty]) 
  let mk_nativeptr_typ ty = TType_app(nativeptr_tcr, [ty]) 
  let mk_fun_ty d r = TType_fun (d,r) 
  let (-->) d r = mk_fun_ty d r
  let mk_forall_ty d r = TType_forall (d,r)
  let try_mk_forall_ty d r = if d = [] then r else mk_forall_ty d r 
  let (+->) d r = try_mk_forall_ty d r 

  let mk_IComparer_ty = mk_mscorlib_mono_typ sysCcu "System.Collections.IComparer";
  let mk_IEqualityComparer_ty = mk_mscorlib_mono_typ sysCcu "System.Collections.IEqualityComparer";

  let mk_binop_ty ty      = ty --> (ty --> ty) 
  let mk_rel_ty ty        = ty --> (ty --> bool_ty) 
  let mk_compare_ty ty    = ty --> (ty --> int_ty)
  let mk_compare_withc_ty ty = mk_IComparer_ty --> (ty --> (ty --> int_ty))
  let mk_equality_withc_ty ty = mk_IEqualityComparer_ty --> (ty --> (ty --> bool_ty))
  let mk_hash_ty ty = ty --> int_ty
  let mk_hash_withc_ty ty = mk_IEqualityComparer_ty --> (ty --> int_ty)

  let mk_option_ty ty = TType_app(option_tcr_nice,[ty])
  let mk_list_ty ty   = TType_app(list_tcr_nice,[ty])
  let mk_seq_ty ty1   = TType_app(seq_tcr,[ty1])
  let mk_IEnumerator_ty ty1   = TType_app(IEnumerator_tcr,[ty1])
  let mk_array_typ ty  = TType_app(array_tcr, [ty])
  let mk_array2_typ ty  = TType_app(il_arr2_tcr, [ty])
  let mk_array3_typ ty  = TType_app(il_arr3_tcr, [ty])
  let mk_array4_typ ty  = TType_app(il_arr4_tcr, [ty])
  let mk_lazy_ty ty   = TType_app(lazy_tcr, [ty])
  
  let mk_format_ty aty bty cty dty ety = TType_app(format_tcr, [aty;bty;cty;dty; ety]) 
  let mk_format4_ty aty bty cty dty = TType_app(format4_tcr, [aty;bty;cty;dty]) 
  let mk_expr_ty aty = TType_app(expr_tcr, [aty]) 
  let mk_raw_expr_ty = TType_app(raw_expr_tcr, []) 
  let cons_ucref = mk_ucref list_tcr_not_nice "op_ColonColon" 
  let nil_ucref  = mk_ucref list_tcr_not_nice "op_Nil" 

  
  (* value infos *)
  let fslib_MFLanguagePrimitives_nlpath = mk_MFLanguagePrimitives_nlpath fslibCcu 
  let fslib_MFIEnumerable_nlpath = mk_MFIEnumerable_nlpath fslibCcu 
  let fslib_MFIEnumerableSequenceExpressionHelpers_nlpath = mk_MFIEnumerableSequenceExpressionHelpers_nlpath fslibCcu 
  let fslib_MFText_nlpath       = mk_MFText_nlpath fslibCcu 
  let fslib_MFControl_nlpath       = mk_MFControl_nlpath fslibCcu 
  let fslib_MFOperators_nlpath  = mk_MFOperators_nlpath fslibCcu 
  let fslib_MFOperatorsChecked_nlpath = mk_MFOperatorsChecked_nlpath fslibCcu
  
  let fslib_MFLanguagePrimitivesIntrinsicOperators_nlpath = mk_MFLanguagePrimitivesIntrinsicOperators_nlpath fslibCcu 
  let fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath = mk_MFLanguagePrimitivesIntrinsicFunctions_nlpath fslibCcu 
  let fslib_MFLanguagePrimitivesHashCompare_nlpath = mk_MFLanguagePrimitivesHashCompare_nlpath fslibCcu 
  let mfm = mk_top_nlpath fslibCcu (Array.append lib_MF_arr_path [| "Math" |]) 

  let tuple1_tcr      = if sysTypes then (mk_mscorlib_tcref sysCcu "System.Tuple`1") else (mk_MFCore_tcref fslibCcu "Tuple`1")
  let tuple2_tcr      = if sysTypes then (mk_mscorlib_tcref sysCcu "System.Tuple`2") else (mk_MFCore_tcref fslibCcu "Tuple`2")
  let tuple3_tcr      = if sysTypes then (mk_mscorlib_tcref sysCcu "System.Tuple`3") else (mk_MFCore_tcref fslibCcu "Tuple`3")
  let tuple4_tcr      = if sysTypes then (mk_mscorlib_tcref sysCcu "System.Tuple`4") else (mk_MFCore_tcref fslibCcu "Tuple`4")
  let tuple5_tcr      = if sysTypes then (mk_mscorlib_tcref sysCcu "System.Tuple`5") else (mk_MFCore_tcref fslibCcu "Tuple`5")
  let tuple6_tcr      = if sysTypes then (mk_mscorlib_tcref sysCcu "System.Tuple`6") else (mk_MFCore_tcref fslibCcu "Tuple`6")
  let tuple7_tcr      = if sysTypes then (mk_mscorlib_tcref sysCcu "System.Tuple`7") else (mk_MFCore_tcref fslibCcu "Tuple`7")
  let tuple8_tcr      = if sysTypes then (mk_mscorlib_tcref sysCcu "System.Tuple`8") else (mk_MFCore_tcref fslibCcu "Tuple`8")
  
  let choice2_tcr     = mk_MFCore_tcref fslibCcu "Choice`2" 
  let choice3_tcr     = mk_MFCore_tcref fslibCcu "Choice`3" 
  let choice4_tcr     = mk_MFCore_tcref fslibCcu "Choice`4" 
  let choice5_tcr     = mk_MFCore_tcref fslibCcu "Choice`5" 
  let choice6_tcr     = mk_MFCore_tcref fslibCcu "Choice`6" 
  let choice7_tcr     = mk_MFCore_tcref fslibCcu "Choice`7" 
  let tcref_eq x y = prim_tcref_eq compilingFslib fslibCcu  x y
  let vref_eq  x y = prim_vref_eq compilingFslib fslibCcu x y
  let ucref_eq x y = prim_ucref_eq compilingFslib fslibCcu x y
  (* let modref_eq = prim_modref_eq compilingFslib fslibCcu in  *)

  let suppressed_types = 
    [ // REVIEW: Decide whether we want to filter 'List' type as well.
      // mk_MFColl_tcref fslibCcu "List`1";
      mk_MFColl_tcref fslibCcu "FSharpList`1"; 
      mk_MFCore_tcref fslibCcu "Option`1";
      mk_MFCore_tcref fslibCcu "Ref`1"; 
      mk_MFCore_tcref fslibCcu "Unit" ] 

  let decode_tuple_ty l = 
      match l with 
      | [t1;t2;t3;t4;t5;t6;t7;marker] -> 
          match marker with 
          | TType_app(tcref,[t8]) when tcref_eq tcref tuple1_tcr -> TType_tuple [t1;t2;t3;t4;t5;t6;t7;t8]
          | TType_app(tcref,[TType_tuple t8plus]) -> 
              TType_tuple ([t1;t2;t3;t4;t5;t6;t7] @ t8plus)
          | _ -> TType_tuple l 
      | _ -> TType_tuple l 

  let mk_MFCore_attrib nm : BuiltinAttribInfo = 
      let tname = nm ^ "." ^ nm
      AttribInfo(mk_tref(Msilxlib.scoref (), nm),mk_MFCore_tcref fslibCcu nm) 

  let mk_mscorlib_attrib nm : BuiltinAttribInfo = 
      AttribInfo(mk_tref (ilg.mscorlib_scoref,nm), mk_mscorlib_tcref sysCcu nm)

  let normalize filename = 
      let n = String.length filename 
      let res = Buffer.create n 
      for i = 0 to n-1 do 
        let c = String.get filename i 
        Buffer.add_char res (match c with '/' -> '\\' | _ -> c);
      done;
      Buffer.contents res

  let mk_doc filename = ILSourceDocument.Create(language=None, vendor=None, documentType=None, file=filename)
  // Build the memoization table for files
  let memoize_file = new MemoizationTable<int,ILSourceDocument> (file_of_file_idx >> Filename.fullpath directoryToResolveRelativePaths >> normalize >> mk_doc)

  let generic_comparison_outer_info       = Intrinsic(fslib_MFLanguagePrimitives_nlpath,           "GenericComparison"                    ,[vara] +->     mk_compare_ty vara_ty) 
  let generic_comparison_withc_outer_info = Intrinsic(fslib_MFLanguagePrimitives_nlpath,           "GenericComparisonWithComparer"        ,[vara] +->     mk_compare_withc_ty  vara_ty) 


  let generic_hash_withc_tuple2_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastHashTuple2"                      ,[vara;varb] +->               mk_hash_withc_ty (decode_tuple_ty [vara_ty; varb_ty]))   
  let generic_hash_withc_tuple3_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastHashTuple3"                      ,[vara;varb;varc] +->          mk_hash_withc_ty (decode_tuple_ty [vara_ty; varb_ty; varc_ty]))   
  let generic_hash_withc_tuple4_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastHashTuple4"                      ,[vara;varb;varc;vard] +->     mk_hash_withc_ty (decode_tuple_ty [vara_ty; varb_ty; varc_ty; vard_ty]))   
  let generic_hash_withc_tuple5_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastHashTuple5"                      ,[vara;varb;varc;vard;vare] +->mk_hash_withc_ty (decode_tuple_ty [vara_ty; varb_ty; varc_ty; vard_ty; vare_ty]))   

  let generic_equals_withc_tuple2_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastEqualsTuple2"                      ,[vara;varb]+->               mk_equality_withc_ty (decode_tuple_ty [vara_ty; varb_ty]))   
  let generic_equals_withc_tuple3_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastEqualsTuple3"                      ,[vara;varb;varc] +->          mk_equality_withc_ty (decode_tuple_ty [vara_ty; varb_ty; varc_ty]))   
  let generic_equals_withc_tuple4_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastEqualsTuple4"                      ,[vara;varb;varc;vard]+->     mk_equality_withc_ty (decode_tuple_ty [vara_ty; varb_ty; varc_ty; vard_ty]))   
  let generic_equals_withc_tuple5_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastEqualsTuple5"                      ,[vara;varb;varc;vard;vare] +-> mk_equality_withc_ty (decode_tuple_ty [vara_ty; varb_ty; varc_ty; vard_ty; vare_ty]))   

  let generic_compare_withc_tuple2_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastCompareTuple2"                      ,[vara;varb]+->               mk_compare_withc_ty (decode_tuple_ty [vara_ty; varb_ty]))   
  let generic_compare_withc_tuple3_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastCompareTuple3"                      ,[vara;varb;varc]+->          mk_compare_withc_ty (decode_tuple_ty [vara_ty; varb_ty; varc_ty]))   
  let generic_compare_withc_tuple4_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastCompareTuple4"                      ,[vara;varb;varc;vard]+->     mk_compare_withc_ty (decode_tuple_ty [vara_ty; varb_ty; varc_ty; vard_ty]))   
  let generic_compare_withc_tuple5_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,           "FastCompareTuple5"                      ,[vara;varb;varc;vard;vare]+->mk_compare_withc_ty (decode_tuple_ty [vara_ty; varb_ty; varc_ty; vard_ty; vare_ty]))   


  let generic_equality_outer_info         = Intrinsic(fslib_MFLanguagePrimitives_nlpath,           "GenericEquality"                      ,[vara] +->     mk_rel_ty vara_ty) 
  let get_generic_comparer_info      = Intrinsic(fslib_MFLanguagePrimitives_nlpath,           "FSharpComparer"              ,unit_ty --> mk_IComparer_ty) 
  let get_generic_equality_comparer_info      = Intrinsic(fslib_MFLanguagePrimitives_nlpath,           "FSharpEqualityComparer"              ,unit_ty -->  mk_IEqualityComparer_ty) 
  let generic_equality_withc_outer_info   = Intrinsic(fslib_MFLanguagePrimitives_nlpath,           "GenericEqualityWithComparer"          ,[vara] +->     mk_equality_withc_ty vara_ty)
  let generic_hash_withc_outer_info       = Intrinsic(fslib_MFLanguagePrimitives_nlpath,           "GenericHashWithComparer"              ,[vara] +->     mk_hash_withc_ty vara_ty)

  let generic_equality_inner_info         = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,                  "GenericEqualityIntrinsic"             ,[vara] +->     mk_rel_ty vara_ty)
  let generic_equality_withc_inner_info   = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,                  "GenericEqualityWithComparerIntrinsic" ,[vara] +->     mk_equality_withc_ty vara_ty)
  let generic_comparison_inner_info       = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,                  "GenericComparisonIntrinsic"           ,[vara] +->     mk_compare_ty vara_ty)
  let generic_comparison_withc_inner_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,                  "GenericComparisonWithComparerIntrinsic",[vara] +->    mk_compare_withc_ty vara_ty)

  let generic_hash_inner_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,                              "GenericHashIntrinsic"                 ,[vara] +->     mk_hash_ty vara_ty)
  let generic_hash_withc_inner_info = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath,                        "GenericHashWithComparerIntrinsic"     ,[vara] +->     mk_hash_withc_ty  vara_ty)
  
  //let unary_neg_sinfo =            fslib_MFLanguagePrimitivesIntrinsicOperators_nlpath, (CompileOpName "~-")        
  let and_info =                   Intrinsic(fslib_MFLanguagePrimitivesIntrinsicOperators_nlpath, (CompileOpName "&")        ,(mk_rel_ty bool_ty)) 
  let addrof_info =                Intrinsic(fslib_MFLanguagePrimitivesIntrinsicOperators_nlpath, (CompileOpName "~&")        ,([vara] +-> (vara_ty --> mk_byref_typ vara_ty)))   
  let addrof2_info =               Intrinsic(fslib_MFLanguagePrimitivesIntrinsicOperators_nlpath, (CompileOpName "~&&")        ,([vara] +-> (vara_ty --> mk_nativeptr_typ vara_ty)))   
  let and2_info =                  Intrinsic(fslib_MFLanguagePrimitivesIntrinsicOperators_nlpath, (CompileOpName "&&")         ,(mk_rel_ty bool_ty) ) 
  let or_info =                    Intrinsic(fslib_MFLanguagePrimitivesIntrinsicOperators_nlpath, "or"                         ,(mk_rel_ty bool_ty)) 
  let or2_info =                   Intrinsic(fslib_MFLanguagePrimitivesIntrinsicOperators_nlpath, (CompileOpName "||")         ,(mk_rel_ty bool_ty)) 
  let create_instance_info       = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "CreateInstance"    ,([vara] +-> (unit_ty --> vara_ty))) 
  let unbox_info                 = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "UnboxGeneric"    ,([vara] +-> (obj_ty --> vara_ty))) 

  let unbox_fast_info            = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "UnboxFast"    ,([vara] +-> (obj_ty --> vara_ty))) 
  let istype_info                = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "TypeTestGeneric"    ,([vara] +-> (obj_ty --> bool_ty))) 
  let istype_fast_info           = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "TypeTestFast"    ,([vara] +-> (obj_ty --> bool_ty))) 

  let dispose_info               = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "Dispose"    ,([vara] +-> (vara_ty --> unit_ty))) 

  let poly_eq_inner_info         = Intrinsic(fslib_MFLanguagePrimitivesHashCompare_nlpath, "PhysicalEqualityIntrinsic"      ,([vara]                +-> mk_rel_ty vara_ty))  
  let bitwise_or_info            = Intrinsic(fslib_MFOperators_nlpath, "op_BitwiseOr"      ,([vara]                +-> mk_binop_ty vara_ty))  
  let bitwise_and_info           = Intrinsic(fslib_MFOperators_nlpath, "op_BitwiseAnd"     ,([vara]                +-> mk_binop_ty vara_ty))  
  let raise_info                 = Intrinsic(fslib_MFOperators_nlpath, "raise"             ,([vara]           +-> (mk_mscorlib_mono_typ sysCcu "System.Exception" --> vara_ty)))  
  let rethrow_info               = Intrinsic(fslib_MFOperators_nlpath, "rethrow"           ,([vara]                +-> (unit_ty --> vara_ty)))  
  let typeof_info                = Intrinsic(fslib_MFOperators_nlpath, "typeof"            ,([vara]                +-> system_Type_typ))  
  let typedefof_info             = Intrinsic(fslib_MFOperators_nlpath, "typedefof"         ,([vara]                +-> system_Type_typ))  
  let enum_info                  = Intrinsic(fslib_MFOperators_nlpath, "enum"              ,([vara]                +-> (int_ty --> vara_ty)))  
  let range_op_info              = Intrinsic(fslib_MFOperators_nlpath, "op_Range"      ,([vara]                +-> (vara_ty --> (vara_ty --> mk_seq_ty vara_ty))))  
  let arr1_lookup_info         = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "GetArray",([vara;] +-> (mk_array_typ vara_ty --> (int_ty --> vara_ty))))  
  let arr2_lookup_info         = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "GetArray2D",([vara;] +-> (mk_array2_typ vara_ty --> (TType_tuple [int_ty;int_ty] --> vara_ty))))  
  let arr3_lookup_info         = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "GetArray3D",([vara;] +-> (mk_array3_typ vara_ty --> (TType_tuple [int_ty;int_ty;int_ty] --> vara_ty))))  
  let arr4_lookup_info         = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "GetArray4D",([vara;] +-> (mk_array4_typ vara_ty --> (TType_tuple [int_ty;int_ty;int_ty;int_ty] --> vara_ty))))  
  let seq_map_concat_info        = Intrinsic(fslib_MFIEnumerable_nlpath,                   "collect"                 ,([vara;varb;varc] +-> ((vara_ty --> varb_ty) --> (mk_seq_ty vara_ty --> mk_seq_ty varc_ty))))  
  let seq_delay_info             = Intrinsic(fslib_MFIEnumerable_nlpath,                   "delay"                   ,([varb] +-> ((unit_ty --> mk_seq_ty varb_ty) --> mk_seq_ty varb_ty)))  
  let seq_append_info            = Intrinsic(fslib_MFIEnumerable_nlpath,                   "append"                   ,([varb] +-> (mk_seq_ty varb_ty --> (mk_seq_ty varb_ty --> mk_seq_ty varb_ty))))  
  let seq_using_info             = Intrinsic(fslib_MFIEnumerableSequenceExpressionHelpers_nlpath,                   "EnumerateUsing"                   ,([vara;varb;varc] +-> (vara_ty --> ((vara_ty --> varb_ty) --> mk_seq_ty varc_ty))))  
  let seq_generated_info         = Intrinsic(fslib_MFIEnumerableSequenceExpressionHelpers_nlpath,                   "EnumerateWhile"                   ,([varb] +-> ((unit_ty --> bool_ty) --> (mk_seq_ty varb_ty --> mk_seq_ty varb_ty))))  
  let seq_finally_info           = Intrinsic(fslib_MFIEnumerableSequenceExpressionHelpers_nlpath,                   "EnumerateThenFinally"                   ,([varb] +-> (mk_seq_ty varb_ty --> ((unit_ty --> unit_ty) --> mk_seq_ty varb_ty))))  
  let seq_of_functions_info      = Intrinsic(fslib_MFIEnumerableSequenceExpressionHelpers_nlpath,                   "EnumerateFromFunctions"                 ,([vara;varb]           +-> ((unit_ty --> vara_ty)  --> ((vara_ty --> bool_ty) --> ((vara_ty --> varb_ty) --> mk_seq_ty varb_ty)))))  
  let seq_to_array_info          = Intrinsic(fslib_MFIEnumerable_nlpath,                   "to_array"                      ,([varb]                +-> (mk_seq_ty varb_ty  --> mk_array_typ varb_ty)))  
  let seq_to_list_info           = Intrinsic(fslib_MFIEnumerable_nlpath,                   "to_list"                      ,([varb]                +-> (mk_seq_ty varb_ty  --> mk_list_ty varb_ty)))  
  let seq_map_info               = Intrinsic(fslib_MFIEnumerable_nlpath,                   "map"                          ,([vara;varb]      +-> ((vara_ty --> varb_ty) --> (mk_seq_ty vara_ty --> mk_seq_ty varb_ty))))  
  let seq_singleton_info         = Intrinsic(fslib_MFIEnumerable_nlpath,                   "singleton"                    ,([vara]                +-> (vara_ty --> (mk_seq_ty vara_ty))))  
  let seq_empty_info             = Intrinsic(fslib_MFIEnumerable_nlpath,                   "empty"                        ,([vara]                +-> (mk_seq_ty vara_ty)))  
  let new_format_info            = Intrinsic(fslib_MFText_nlpath,                          "PrintfFormat`5.ctor.1"               ,([vara;varb;varc;vard;vare]                +-> (string_ty --> mk_format_ty vara_ty varb_ty varc_ty vard_ty vare_ty)))  
  let sprintf_info               = Intrinsic(NLPath(fslibCcu,lib_FSLib_Pervasives_path), "sprintf"               ,([vara]                +-> (mk_format4_ty vara_ty unit_ty string_ty string_ty)))  
  let lazy_force_info            = 
    // LazyInit\Value for > 4.0
    if sysTypes then
                                   Intrinsic(NLPath(fslibCcu,lib_MFLazy_path),        "LazyInit`1.Force.1"                      ,([vara]                +-> (mk_lazy_ty vara_ty --> (unit_ty --> vara_ty)))) 
    else
                                   Intrinsic(fslib_MFControl_nlpath,                    "Lazy`1.Force.1"                        ,([vara]                +-> (mk_lazy_ty vara_ty --> (unit_ty --> vara_ty))))                                   
  let lazy_create_info           = 
    if sysTypes then
                                   Intrinsic(NLPath(fslibCcu,lib_MFLazy_path),                    "LazyInit`1.Create.1.Static"                            ,([vara]                +-> ((unit_ty --> vara_ty) --> mk_lazy_ty vara_ty)))
    else
                                   Intrinsic(fslib_MFControl_nlpath,                    "Lazy`1.Create.1.Static"                        ,([vara]                +-> ((unit_ty --> vara_ty) --> mk_lazy_ty vara_ty)))  
  let seq_info                   = Intrinsic(NLPath(fslibCcu,lib_FSLib_Pervasives_path),"seq",([vara] +-> (mk_seq_ty vara_ty --> mk_seq_ty vara_ty)))  
  let splice_expr_info           = Intrinsic(NLPath(fslibCcu,lib_FSLib_Pervasives_path),"op_Splice",([vara] +-> (mk_expr_ty vara_ty --> vara_ty)))  
  let splice_raw_expr_info           = Intrinsic(NLPath(fslibCcu,lib_FSLib_Pervasives_path),"op_SpliceUntyped",([vara] +-> (mk_raw_expr_ty --> vara_ty)))  
  let new_decimal_info     = Intrinsic(fslib_MFLanguagePrimitivesIntrinsicFunctions_nlpath, "MakeDecimal"                ,(int_ty --> (int_ty --> (int_ty --> (bool_ty --> (byte_ty --> decimal_ty))))))  
  let array_get_info             = Intrinsic(mk_MFLanguagePrimitivesIntrinsicFunctions_nlpath fslibCcu, "GetArray"       ,([vara]                +-> (mk_array_typ vara_ty --> (int_ty --> vara_ty)))) 
  let generic_hash_info             = Intrinsic(mk_MFLanguagePrimitivesIntrinsicFunctions_nlpath fslibCcu,                    "StructuralHash"                       ,[vara] +->    (vara_ty --> int_ty)) 
  let poly_hash_info             = Intrinsic(mk_MFLanguagePrimitivesIntrinsicFunctions_nlpath fslibCcu, "StructuralHash" ,([vara]                +-> (vara_ty             --> int_ty))  ) 
  let unpickle_quoted_info       = Intrinsic(NLPath(fslibCcu,lib_Quotations_path),   "Expr.Deserialize.4.Static"      ,([] +-> (decode_tuple_ty [system_Type_typ ;mk_list_ty system_Type_typ ;mk_list_ty mk_raw_expr_ty ; mk_array_typ byte_ty] --> mk_raw_expr_ty ))) 
  let cast_quotation_info        = Intrinsic(NLPath(fslibCcu,lib_Quotations_path),   "Expr.Cast.1.Static"      ,([vara] +-> (mk_raw_expr_ty --> mk_expr_ty vara_ty))) 
  let lift_value_info            = Intrinsic(NLPath(fslibCcu,lib_Quotations_path),   "Expr.Value.1.Static"      ,([vara] +-> (vara_ty --> mk_expr_ty vara_ty))) 
    
  { ilg=ilg;
    ilxPubCloEnv=Pubclo.new_cenv(ilg);
    compilingFslib=compilingFslib;
    mlCompatibility=mlCompatibility;
    directoryToResolveRelativePaths=directoryToResolveRelativePaths;
    (* modref_eq = modref_eq; *)
    ucref_eq = ucref_eq;
    vref_eq = vref_eq;
    fslibCcu       = fslibCcu;
    sysCcu         = sysCcu;
    refcell_tcr       = mk_MFCore_tcref     fslibCcu "Ref`1";
    option_tcr        = mk_MFCore_tcref     fslibCcu "Option`1";
    list_tcr_canon    = mk_MFColl_tcref     fslibCcu "FSharpList`1";
    lazy_tcr_canon    = lazy_tcr;
    refcell_tcr_nice  = mk_MFCore_tcref     fslibCcu "ref`1";
    option_tcr_nice   = mk_MFCore_tcref     fslibCcu "option`1";
    list_tcr_nice     = mk_MFColl_tcref     fslibCcu "list`1";
    format_tcr       = format_tcr;
    expr_tcr       = expr_tcr;
    raw_expr_tcr       = raw_expr_tcr;
    int_tcr        = int_tcr;
    nativeint_tcr  = nativeint_tcr;
    unativeint_tcr = unativeint_tcr;
    int32_tcr      = int32_tcr;
    int16_tcr      = int16_tcr;
    int64_tcr      = int64_tcr;
    uint16_tcr     = uint16_tcr;
    uint32_tcr     = uint32_tcr;
    uint64_tcr     = uint64_tcr;
    sbyte_tcr      = sbyte_tcr;
    decimal_tcr    = decimal_tcr;
    date_tcr    = date_tcr;
    pdecimal_tcr   = pdecimal_tcr;
    byte_tcr       = byte_tcr;
    bool_tcr       = bool_tcr;
    string_tcr     = string_tcr;
    obj_tcr        = obj_tcr;
    unit_tcr_canon = unit_tcr_canon;
    unit_tcr_nice  = unit_tcr_nice;
    exn_tcr        = exn_tcr;
    char_tcr       = char_tcr;
    float_tcr      = float_tcr;
    float32_tcr    = float32_tcr;
    pfloat_tcr     = pfloat_tcr;
    pfloat32_tcr   = pfloat32_tcr;
    pint_tcr       = pint_tcr;
    pint8_tcr      = pint8_tcr;
    pint16_tcr     = pint16_tcr;
    pint64_tcr     = pint64_tcr;
    byref_tcr      = byref_tcr;
    nativeptr_tcr  = nativeptr_tcr;
    ilsigptr_tcr   = ilsigptr_tcr;
    fastFunc_tcr = fastFunc_tcr;
    fslib_IEvent2_tcr      = fslib_IEvent2_tcr;
    fslib_IDelegateEvent_tcr      = fslib_IDelegateEvent_tcr;
    array_tcr      = array_tcr;
    seq_tcr        = seq_tcr;
    seq_base_tcr = mk_MFIEnumerableSequenceExpressionHelpers_tcref fslibCcu "GeneratedSequenceBase`1";
    il_arr1_tcr    = il_arr1_tcr;
    il_arr2_tcr    = il_arr2_tcr;
    il_arr3_tcr    = il_arr3_tcr;
    il_arr4_tcr    = il_arr4_tcr;
    tuple1_tcr     = tuple1_tcr;
    tuple2_tcr     = tuple2_tcr;
    tuple3_tcr     = tuple3_tcr;
    tuple4_tcr     = tuple4_tcr;
    tuple5_tcr     = tuple5_tcr;
    tuple6_tcr     = tuple6_tcr;
    tuple7_tcr     = tuple7_tcr;
    tuple8_tcr     = tuple8_tcr;
    choice2_tcr    = choice2_tcr;
    choice3_tcr    = choice3_tcr;
    choice4_tcr    = choice4_tcr;
    choice5_tcr    = choice5_tcr;
    choice6_tcr    = choice6_tcr;
    choice7_tcr    = choice7_tcr;
    int_ty        = int_ty;
    nativeint_ty  = mk_mono_typ nativeint_tcr;
    unativeint_ty = mk_mono_typ unativeint_tcr;
    int32_ty      = mk_mono_typ int32_tcr;
    int16_ty      = mk_mono_typ int16_tcr;
    int64_ty      = mk_mono_typ int64_tcr;
    uint16_ty     = mk_mono_typ uint16_tcr;
    uint32_ty     = mk_mono_typ uint32_tcr;
    uint64_ty     = mk_mono_typ uint64_tcr;
    sbyte_ty      = mk_mono_typ sbyte_tcr;
    byte_ty       = byte_ty;
    bool_ty       = bool_ty;
    string_ty     = string_ty;
    obj_ty        = mk_mono_typ obj_tcr;
    unit_ty       = unit_ty;
    exn_ty        = mk_mono_typ exn_tcr;
    char_ty       = mk_mono_typ char_tcr;
    decimal_ty    = mk_mono_typ decimal_tcr;
    float_ty      = mk_mono_typ float_tcr; 
    float32_ty    = mk_mono_typ float32_tcr;
    memoize_file  = memoize_file.Apply;

    system_Array_typ     = mk_mscorlib_mono_typ sysCcu "System.Array";
    system_Object_typ    = mk_mscorlib_mono_typ sysCcu "System.Object";
    system_IDisposable_typ    = mk_mscorlib_mono_typ sysCcu "System.IDisposable";
    system_Value_typ     = mk_mscorlib_mono_typ sysCcu "System.ValueType";
    system_Delegate_typ     = mk_mscorlib_mono_typ sysCcu "System.Delegate";
    system_MulticastDelegate_typ     = mk_mscorlib_mono_typ sysCcu "System.MulticastDelegate";
    system_Enum_typ      = mk_mscorlib_mono_typ sysCcu "System.Enum";
    system_Exception_typ = mk_mscorlib_mono_typ sysCcu "System.Exception";
    system_String_typ    = mk_mscorlib_mono_typ sysCcu "System.String";
    system_Int32_typ     = mk_mscorlib_mono_typ sysCcu "System.Int32";
    system_Type_typ                  = system_Type_typ;
    system_TypedReference_tcref        = mk_mscorlib_tcref sysCcu "System.TypedReference" ;
    system_ArgIterator_tcref           = mk_mscorlib_tcref sysCcu "System.ArgIterator" ;
    system_RuntimeArgumentHandle_tcref =  mk_mscorlib_tcref sysCcu "System.RuntimeArgumentHandle";
    system_SByte_tcref =  mk_mscorlib_tcref sysCcu "System.SByte";
    system_Int16_tcref =  mk_mscorlib_tcref sysCcu "System.Int16";
    system_Int32_tcref =  mk_mscorlib_tcref sysCcu "System.Int32";
    system_Int64_tcref =  mk_mscorlib_tcref sysCcu "System.Int64";
    system_IntPtr_tcref =  mk_mscorlib_tcref sysCcu "System.IntPtr";
    system_Bool_tcref =  mk_mscorlib_tcref sysCcu "System.Boolean"; 
    system_Byte_tcref =  mk_mscorlib_tcref sysCcu "System.Byte";
    system_UInt16_tcref =  mk_mscorlib_tcref sysCcu "System.UInt16";
    system_Char_tcref =  mk_mscorlib_tcref sysCcu "System.Char";
    system_UInt32_tcref =  mk_mscorlib_tcref sysCcu "System.UInt32";
    system_UInt64_tcref =  mk_mscorlib_tcref sysCcu "System.UInt64";
    system_UIntPtr_tcref =  mk_mscorlib_tcref sysCcu "System.UIntPtr";
    system_Single_tcref =  mk_mscorlib_tcref sysCcu "System.Single";
    system_Double_tcref =  mk_mscorlib_tcref sysCcu "System.Double";
    system_RuntimeTypeHandle_typ = mk_mscorlib_mono_typ sysCcu "System.RuntimeTypeHandle";
    system_MarshalByRefObject_tcref = mk_mscorlib_tcref sysCcu "System.MarshalByRefObject";
    system_MarshalByRefObject_typ = mk_mscorlib_mono_typ sysCcu "System.MarshalByRefObject";
    
    system_Array_tcref  = mk_mscorlib_tcref sysCcu "System.Array";
    system_Object_tcref  = mk_mscorlib_tcref sysCcu "System.Object";
    system_Void_tcref    = mk_mscorlib_tcref sysCcu "System.Void";
    system_IndexOutOfRangeException_tcref    = mk_mscorlib_tcref sysCcu "System.IndexOutOfRangeException";
    system_Nullable_tcref = mk_mscorlib_tcref sysCcu "System.Nullable`1";
    system_GenericIComparable_tcref = mk_mscorlib_tcref sysCcu "System.IComparable`1";
    system_GenericIEquatable_tcref = mk_mscorlib_tcref sysCcu "System.IEquatable`1";
    mk_IComparable_ty    = mk_mscorlib_mono_typ sysCcu "System.IComparable";

    mk_IStructuralComparable_ty = 
        if sysTypes then 
            mk_mscorlib_mono_typ sysCcu "System.Collections.IStructuralComparable";
        else
            let tcref_IStructuralComparable = mk_MFCore_tcref fslibCcu "IStructuralComparable" 
            TType_app(tcref_IStructuralComparable,[])
        
    mk_IStructuralEquatable_ty = 
        if sysTypes then
            mk_mscorlib_mono_typ sysCcu "System.Collections.IStructuralEquatable";
        else
            let tcref_IStructuralEquatable = mk_MFCore_tcref fslibCcu "IStructuralEquatable" 
            TType_app(tcref_IStructuralEquatable,[])

    mk_IComparer_ty = mk_IComparer_ty;
    mk_IEqualityComparer_ty = mk_IEqualityComparer_ty;
    tcref_System_Collections_IComparer = mk_mscorlib_tcref sysCcu "System.Collections.IComparer";
    tcref_System_Collections_IEqualityComparer = mk_mscorlib_tcref sysCcu "System.Collections.IEqualityComparer";
    
    tcref_System_IStructuralComparable =
        if sysTypes then
            mk_mscorlib_tcref sysCcu "System.Collections.IStructuralComparable"
        else
            mk_MFCore_tcref fslibCcu "IStructuralComparable"
    tcref_System_IStructuralEquatable  = 
        if sysTypes then
            mk_mscorlib_tcref sysCcu "System.Collections.IStructuralEquatable";
        else
            mk_MFCore_tcref fslibCcu "IStructuralEquatable" 
            
    tcref_LanguagePrimitives = mk_MFCore_tcref fslibCcu "LanguagePrimitives";

    mk_IConvertible_ty    = mk_mscorlib_mono_typ sysCcu "System.IConvertible";
    mk_IFormattable_ty    = mk_mscorlib_mono_typ sysCcu "System.IFormattable";

    tcref_System_Collections_Generic_IList       = mk_mscorlib_tcref sysCcu "System.Collections.Generic.IList`1";
    tcref_System_Collections_Generic_ICollection = mk_mscorlib_tcref sysCcu "System.Collections.Generic.ICollection`1";
    tcref_System_Collections_Generic_IEnumerable = IEnumerable_tcr;
    tcref_System_Collections_Generic_IEnumerator = IEnumerator_tcr;

    attrib_AttributeUsageAttribute = mk_mscorlib_attrib "System.AttributeUsageAttribute";
    attrib_ParamArrayAttribute     = mk_mscorlib_attrib "System.ParamArrayAttribute";
    attrib_IDispatchConstantAttribute  = mk_mscorlib_attrib "System.Runtime.CompilerServices.IDispatchConstantAttribute";
    attrib_IUnknownConstantAttribute  = mk_mscorlib_attrib "System.Runtime.CompilerServices.IUnknownConstantAttribute";
    
    attrib_SystemObsolete          = mk_mscorlib_attrib "System.ObsoleteAttribute";
    attrib_DllImportAttribute      = mk_mscorlib_attrib "System.Runtime.InteropServices.DllImportAttribute";
    attrib_StructLayoutAttribute   = mk_mscorlib_attrib "System.Runtime.InteropServices.StructLayoutAttribute";
    attrib_TypeForwardedToAttribute   = mk_mscorlib_attrib "System.Runtime.CompilerServices.TypeForwardedToAttribute";
    attrib_ComVisibleAttribute     = mk_mscorlib_attrib "System.Runtime.InteropServices.ComVisibleAttribute";
    attrib_ComImportAttribute      = mk_mscorlib_attrib "System.Runtime.InteropServices.ComImportAttribute";
    attrib_FieldOffsetAttribute    = mk_mscorlib_attrib "System.Runtime.InteropServices.FieldOffsetAttribute" ;
    attrib_MarshalAsAttribute      = mk_mscorlib_attrib "System.Runtime.InteropServices.MarshalAsAttribute";
    attrib_InAttribute             = mk_mscorlib_attrib "System.Runtime.InteropServices.InAttribute" ;
    attrib_OutAttribute            = mk_mscorlib_attrib "System.Runtime.InteropServices.OutAttribute" ;
    attrib_OptionalAttribute       = mk_mscorlib_attrib "System.Runtime.InteropServices.OptionalAttribute" ;
    attrib_ThreadStaticAttribute   = mk_mscorlib_attrib "System.ThreadStaticAttribute";
    attrib_SpecialNameAttribute   = mk_mscorlib_attrib "System.Runtime.CompilerServices.SpecialNameAttribute";
    attrib_ContextStaticAttribute  = mk_mscorlib_attrib "System.ContextStaticAttribute";
    attrib_FlagsAttribute          = mk_mscorlib_attrib "System.FlagsAttribute";
    attrib_DefaultMemberAttribute  = mk_mscorlib_attrib "System.Reflection.DefaultMemberAttribute";
    attrib_DebuggerDisplayAttribute  = mk_mscorlib_attrib "System.Diagnostics.DebuggerDisplayAttribute";
    attrib_DebuggerTypeProxyAttribute  = mk_mscorlib_attrib "System.Diagnostics.DebuggerTypeProxyAttribute";
    
    attrib_NonSerializedAttribute                 = mk_mscorlib_attrib "System.NonSerializedAttribute";
    attrib_AutoSerializableAttribute              = mk_MFCore_attrib "AutoSerializableAttribute";
    attrib_OverloadIDAttribute                    = mk_MFCore_attrib "OverloadIDAttribute";
    attrib_RequireQualifiedAccessAttribute        = mk_MFCore_attrib "RequireQualifiedAccessAttribute";
    attrib_EntryPointAttribute                    = mk_MFCore_attrib "EntryPointAttribute";
    attrib_DefaultAugmentationAttribute           = mk_MFCore_attrib "DefaultAugmentationAttribute";
    attrib_OCamlCompatibilityAttribute            = mk_MFCore_attrib "OCamlCompatibilityAttribute";
    attrib_ExperimentalAttribute                  = mk_MFCore_attrib "ExperimentalAttribute";
    attrib_UnverifiableAttribute                  = mk_MFCore_attrib "UnverifiableAttribute";
    attrib_LiteralAttribute                       = mk_MFCore_attrib "LiteralAttribute";
    attrib_ConditionalAttribute                   = mk_mscorlib_attrib "System.Diagnostics.ConditionalAttribute";
    attrib_OptionalArgumentAttribute              = mk_MFCore_attrib "OptionalArgumentAttribute";
    attrib_RequiresExplicitTypeArgumentsAttribute = mk_MFCore_attrib "RequiresExplicitTypeArgumentsAttribute";
    attrib_DefaultValueAttribute                  = mk_MFCore_attrib "DefaultValueAttribute";
    attrib_ClassAttribute                         = mk_MFCore_attrib "ClassAttribute";
    attrib_InterfaceAttribute                     = mk_MFCore_attrib "InterfaceAttribute";
    attrib_StructAttribute                        = mk_MFCore_attrib "StructAttribute";
    attrib_ReflectedDefinitionAttribute           = mk_MFCore_attrib "ReflectedDefinitionAttribute";
    attrib_AutoOpenAttribute           = mk_MFCore_attrib "AutoOpenAttribute";
    attrib_CompilationRepresentationAttribute     = mk_MFCore_attrib "CompilationRepresentationAttribute";
    attrib_CLIEventAttribute                      = mk_MFCore_attrib "CLIEventAttribute";
    attrib_ReferenceEqualityAttribute             = mk_MFCore_attrib "ReferenceEqualityAttribute";
    attrib_StructuralEqualityAttribute            = mk_MFCore_attrib "StructuralEqualityAttribute";
    attrib_StructuralComparisonAttribute          = mk_MFCore_attrib "StructuralComparisonAttribute";
    attrib_SealedAttribute                        = mk_MFCore_attrib "SealedAttribute";
    attrib_AbstractClassAttribute                 = mk_MFCore_attrib "AbstractClassAttribute";
    attrib_GeneralizableValueAttribute            = mk_MFCore_attrib "GeneralizableValueAttribute";
    attrib_MeasureAttribute                       = mk_MFCore_attrib "MeasureAttribute";
    attrib_MeasureableAttribute                   = mk_MFCore_attrib "MeasureAnnotatedAbbreviationAttribute";
    attrib_NoDynamicInvocationAttribute                       = mk_MFCore_attrib "NoDynamicInvocationAttribute";

    // Build a map that uses the "canonical" F# type names and TyconRef's for these
    // in preference to the .NET type names. Doing this normalization is a fairly performance critical
    // piece of code as it is frequently invoked in the process of converting .NET metadata to F# internal
    // compiler data structures (see import.ml).
    better_tcref_map = 
       begin 
        let entries1 = 
         [ "Int32", int_tcr; 
           "IntPtr", nativeint_tcr; 
           "UIntPtr", unativeint_tcr;
           "Int16",int16_tcr; 
           "Int64",int64_tcr; 
           "UInt16",uint16_tcr;
           "UInt32",uint32_tcr;
           "UInt64",uint64_tcr;
           "SByte",sbyte_tcr;
           "Decimal",decimal_tcr;
           "Byte",byte_tcr;
           "Boolean",bool_tcr;
           "String",string_tcr;
           "Object",obj_tcr;
           "Exception",exn_tcr;
           "Char",char_tcr;
           "Double",float_tcr;
           "Single",float32_tcr;] 
             |> List.map (fun (nm,tcr) -> 
                   let ty = mk_mono_typ tcr 
                   nm, mk_mscorlib_tcref sysCcu ("System."^nm), (fun _ -> ty)) 
        let entries2 =
            [ "IEnumerable`2", IEnumerable_tcr, (fun tinst -> mk_seq_ty (List.nth tinst 0));
              "FastFunc`2",    fastFunc_tcr, (fun tinst -> mk_fun_ty (List.nth tinst 0) (List.nth tinst 1));
              "Tuple`1",       tuple1_tcr, decode_tuple_ty;
              "Tuple`2",       tuple2_tcr, decode_tuple_ty;
              "Tuple`3",       tuple3_tcr, decode_tuple_ty;
              "Tuple`4",       tuple4_tcr, decode_tuple_ty;
              "Tuple`5",       tuple5_tcr, decode_tuple_ty;
              "Tuple`6",       tuple6_tcr, decode_tuple_ty;
              "Tuple`7",       tuple7_tcr, decode_tuple_ty;
              "Tuple`8",       tuple8_tcr, decode_tuple_ty;] 
        let entries = (entries1 @ entries2)
        
        if compilingFslib then 
            // This map is for use when building FSharp.Core.dll. The backing Tycon's may not yet exist for
            // the TyconRef's we have inour hands, hence we can't dereference them to find their stamps.

            // So this dictionary is indexed by names.
            let dict = 
                entries 
                |> List.map (fun (nm,tcref,builder) -> nm, (fun tcref2 tinst -> if tcref_eq tcref tcref2 then Some(builder tinst) else None)) 
                |> Dictionary.of_list  
            (fun tcref tinst -> 
                 match Dictionary.tryfind dict tcref.MangledName with
                 | Some builder -> builder tcref tinst
                 | _ -> None)  
        else
            // This map is for use in normal times (not building FSharp.Core.dll). It is indexed by tcref stamp which is 
            // faster than the indexing technique used in the case above.
            //
            // So this dictionary is indexed by integers.
            let dict = 
                entries  
                |> List.map (fun (_,tcref,builder) -> tcref.Stamp, builder) 
                |> Dictionary.of_list 
            (fun tcref2 tinst -> 
                 match Dictionary.tryfind dict tcref2.Stamp with
                 | Some builder -> Some(builder tinst)
                 | _ -> None)  
       end;
           
      new_decimal_info = new_decimal_info;
      seq_info    = seq_info;
      generic_equality_inner_vref     = vref_for_val_info generic_equality_inner_info;
      generic_equality_withc_inner_vref  = vref_for_val_info generic_equality_withc_inner_info;
      generic_comparison_inner_vref    = vref_for_val_info generic_comparison_inner_info;
      generic_comparison_withc_inner_vref    = vref_for_val_info generic_comparison_withc_inner_info;
      generic_comparison_outer_vref    = vref_for_val_info generic_comparison_outer_info;
      generic_comparison_withc_outer_info    = generic_comparison_withc_outer_info;
      generic_comparison_outer_info    = generic_comparison_outer_info;

      generic_equality_outer_info     = generic_equality_outer_info;
      generic_equality_withc_outer_info  = generic_equality_withc_outer_info;

      generic_hash_withc_outer_info = generic_hash_withc_outer_info;
      generic_hash_inner_vref = vref_for_val_info generic_hash_inner_info;
      generic_hash_withc_inner_vref = vref_for_val_info generic_hash_withc_inner_info;

      seq_vref    = (vref_for_val_info seq_info) ;
      and_vref    = (vref_for_val_info and_info) ;
      and2_vref   = (vref_for_val_info and2_info);
      addrof_vref = (vref_for_val_info addrof_info);
      addrof2_vref = (vref_for_val_info addrof2_info);
      or_vref     = (vref_for_val_info or_info);
      //splice_vref     = (vref_for_val_info splice_info);
      splice_expr_vref     = (vref_for_val_info splice_expr_info);
      splice_raw_expr_vref     = (vref_for_val_info splice_raw_expr_info);
      or2_vref    = (vref_for_val_info or2_info); 
      poly_eq_inner_vref         = vref_for_val_info poly_eq_inner_info;
      bitwise_or_vref            = vref_for_val_info bitwise_or_info;
      raise_info                 = raise_info;
      rethrow_info               = rethrow_info;
      rethrow_vref               = vref_for_val_info rethrow_info;
      typeof_info                = typeof_info;
      typeof_vref                = vref_for_val_info typeof_info;
      typedefof_vref             = vref_for_val_info typedefof_info;
      enum_vref                  = vref_for_val_info enum_info;
      range_op_vref              = vref_for_val_info range_op_info;
      arr1_lookup_vref         = vref_for_val_info arr1_lookup_info;
      arr2_lookup_vref         = vref_for_val_info arr2_lookup_info;
      arr3_lookup_vref         = vref_for_val_info arr3_lookup_info;
      arr4_lookup_vref         = vref_for_val_info arr4_lookup_info;
      seq_singleton_vref         = vref_for_val_info seq_singleton_info;
      seq_map_concat_vref        = vref_for_val_info seq_map_concat_info;
      seq_map_concat_info        = seq_map_concat_info;
      seq_using_info             = seq_using_info;
      seq_using_vref             = vref_for_val_info seq_using_info;
      seq_delay_info             = seq_delay_info;
      seq_delay_vref             = vref_for_val_info  seq_delay_info;
      seq_append_info            = seq_append_info;
      seq_append_vref            = vref_for_val_info  seq_append_info;
      seq_generated_info         = seq_generated_info;
      seq_generated_vref         = vref_for_val_info  seq_generated_info;
      seq_finally_info           = seq_finally_info;
      seq_finally_vref           = vref_for_val_info  seq_finally_info;
      seq_of_functions_info      = seq_of_functions_info;
      seq_of_functions_vref      = vref_for_val_info  seq_of_functions_info;
      seq_map_info               = seq_map_info;
      seq_map_vref               = vref_for_val_info  seq_map_info;
      seq_singleton_info         = seq_singleton_info;
      seq_empty_info             = seq_empty_info;
      seq_empty_vref             = vref_for_val_info  seq_empty_info;
      new_format_info            = new_format_info;
      new_format_vref            = vref_for_val_info new_format_info;
      sprintf_vref               = vref_for_val_info sprintf_info;
      unbox_vref                 = vref_for_val_info unbox_info;
      unbox_fast_vref            = vref_for_val_info unbox_fast_info;
      istype_vref                = vref_for_val_info istype_info;
      istype_fast_vref           = vref_for_val_info istype_fast_info;
      unbox_info                 = unbox_info;
      get_generic_comparer_info                 = get_generic_comparer_info;
      get_generic_equality_comparer_info                 = get_generic_equality_comparer_info;
      dispose_info               = dispose_info;
      unbox_fast_info            = unbox_fast_info;
      istype_info                = istype_info;
      istype_fast_info           = istype_fast_info;
      lazy_force_info=lazy_force_info;
      lazy_create_info=lazy_create_info;
      create_instance_info       = create_instance_info;
      seq_to_list_info           = seq_to_list_info;
      seq_to_array_info          = seq_to_array_info;
      array_get_info             = array_get_info;
      generic_hash_info             = generic_hash_info;
      unpickle_quoted_info       = unpickle_quoted_info;
      cast_quotation_info       = cast_quotation_info;
      lift_value_info            = lift_value_info;


      generic_hash_withc_tuple2_vref = vref_for_val_info generic_hash_withc_tuple2_info;
      generic_hash_withc_tuple3_vref = vref_for_val_info generic_hash_withc_tuple3_info;
      generic_hash_withc_tuple4_vref = vref_for_val_info generic_hash_withc_tuple4_info;
      generic_hash_withc_tuple5_vref = vref_for_val_info generic_hash_withc_tuple5_info;
      generic_equals_withc_tuple2_vref = vref_for_val_info generic_equals_withc_tuple2_info;
      generic_equals_withc_tuple3_vref = vref_for_val_info generic_equals_withc_tuple3_info;
      generic_equals_withc_tuple4_vref = vref_for_val_info generic_equals_withc_tuple4_info;
      generic_equals_withc_tuple5_vref = vref_for_val_info generic_equals_withc_tuple5_info;
      generic_compare_withc_tuple2_vref = vref_for_val_info generic_compare_withc_tuple2_info;
      generic_compare_withc_tuple3_vref = vref_for_val_info generic_compare_withc_tuple3_info;
      generic_compare_withc_tuple4_vref = vref_for_val_info generic_compare_withc_tuple4_info;
      generic_compare_withc_tuple5_vref = vref_for_val_info generic_compare_withc_tuple5_info;


      cons_ucref = cons_ucref;
      nil_ucref = nil_ucref;
      
      suppressed_types = suppressed_types;
   }
     
let public mk_mscorlib_attrib g nm : BuiltinAttribInfo = 
      AttribInfo(mk_tref (g.ilg.mscorlib_scoref,nm), mk_mscorlib_tcref g.sysCcu nm)
