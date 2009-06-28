(* (c) Microsoft Corporation. All rights reserved  *)

/// Internal use only.  Code and constants shared between binary reader/writer.
module Microsoft.FSharp.Compiler.AbstractIL.Internal.BinaryConstants 

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 


type table = Table of int
val tag_of_table : table -> int
val tab_Module : table
val tab_TypeRef : table
val tab_TypeDef : table
val tab_FieldPtr : table
val tab_Field : table
val tab_MethodPtr : table
val tab_Method : table
val tab_ParamPtr : table
val tab_Param : table
val tab_InterfaceImpl : table
val tab_MemberRef : table
val tab_Constant : table
val tab_CustomAttribute : table
val tab_FieldMarshal : table
val tab_Permission : table
val tab_ClassLayout : table
val tab_FieldLayout : table
val tab_StandAloneSig : table
val tab_EventMap : table
val tab_EventPtr : table
val tab_Event : table
val tab_PropertyMap : table
val tab_PropertyPtr : table
val tab_Property : table
val tab_MethodSemantics : table
val tab_MethodImpl : table
val tab_ModuleRef : table
val tab_TypeSpec : table
val tab_ImplMap : table
val tab_FieldRVA : table
val tab_ENCLog : table
val tab_ENCMap : table
val tab_Assembly : table
val tab_AssemblyProcessor : table
val tab_AssemblyOS : table
val tab_AssemblyRef : table
val tab_AssemblyRefProcessor : table
val tab_AssemblyRefOS : table
val tab_File : table
val tab_ExportedType : table
val tab_ManifestResource : table
val tab_Nested : table
val tab_GenericParam : table
val tab_GenericParamConstraint : table
val tab_MethodSpec : table
val tab_UserStrings : table
val sorted_table_info : (table * int) list
type typeDefOrRef_tag = TypeDefOrRefOrSpecTag of int32
val tdor_TypeDef : typeDefOrRef_tag
val tdor_TypeRef : typeDefOrRef_tag
val tdor_TypeSpec : typeDefOrRef_tag
type hasConstant_tag = HasConstantTag of int32
val hc_FieldDef : hasConstant_tag
val hc_ParamDef : hasConstant_tag
val hc_Property : hasConstant_tag
type hasCustomAttribute_tag = HasCustomAttributeTag of int32
val hca_MethodDef : hasCustomAttribute_tag
val hca_FieldDef : hasCustomAttribute_tag
val hca_TypeRef : hasCustomAttribute_tag
val hca_TypeDef : hasCustomAttribute_tag
val hca_ParamDef : hasCustomAttribute_tag
val hca_InterfaceImpl : hasCustomAttribute_tag
val hca_MemberRef : hasCustomAttribute_tag
val hca_Module : hasCustomAttribute_tag
val hca_Permission : hasCustomAttribute_tag
val hca_Property : hasCustomAttribute_tag
val hca_Event : hasCustomAttribute_tag
val hca_StandAloneSig : hasCustomAttribute_tag
val hca_ModuleRef : hasCustomAttribute_tag
val hca_TypeSpec : hasCustomAttribute_tag
val hca_Assembly : hasCustomAttribute_tag
val hca_AssemblyRef : hasCustomAttribute_tag
val hca_File : hasCustomAttribute_tag
val hca_ExportedType : hasCustomAttribute_tag
val hca_ManifestResource : hasCustomAttribute_tag
type hasFieldMarshal_tag = HasFieldMarshalTag of int32
val hfm_FieldDef : hasFieldMarshal_tag
val hfm_ParamDef : hasFieldMarshal_tag
type hasDeclSecurity_tag = HasDeclSecurityTag of int32
val hds_TypeDef : hasDeclSecurity_tag
val hds_MethodDef : hasDeclSecurity_tag
val hds_Assembly : hasDeclSecurity_tag
type memberRefParent_tag = MemberRefParentTag of int32
val mrp_TypeRef : memberRefParent_tag
val mrp_ModuleRef : memberRefParent_tag
val mrp_MethodDef : memberRefParent_tag
val mrp_TypeSpec : memberRefParent_tag
type hasSemantics_tag = HasSemanticsTag of int32
val hs_Event : hasSemantics_tag
val hs_Property : hasSemantics_tag
type methodDefOrRef_tag = MethodDefOrRefTag of int32
val mdor_MethodDef : methodDefOrRef_tag
val mdor_MemberRef : methodDefOrRef_tag
type memberForwarded_tag = MemberForwardedTag of int32
val mf_FieldDef : memberForwarded_tag
val mf_MethodDef : memberForwarded_tag
type implementation_tag = ImplementationTag of int32
val i_File : implementation_tag
val i_AssemblyRef : implementation_tag
val i_ExportedType : implementation_tag
type customAttributeType_tag = CustomAttributeTypeTag of int32
val cat_MethodDef : customAttributeType_tag
val cat_MemberRef : customAttributeType_tag
type resolutionScope_tag = ResolutionScopeTag of int32
val rs_Module : resolutionScope_tag
val rs_ModuleRef : resolutionScope_tag
val rs_AssemblyRef : resolutionScope_tag
val rs_TypeRef : resolutionScope_tag
type typeOrMethodDef_tag = TypeOrMethodDefTag of int32
val tomd_TypeDef : typeOrMethodDef_tag
val tomd_MethodDef : typeOrMethodDef_tag

val mkTypeDefOrRefOrSpecTag: int32 -> typeDefOrRef_tag
val mkHasConstantTag : int32 -> hasConstant_tag
val mkHasCustomAttributeTag : int32 -> hasCustomAttribute_tag
val mkHasFieldMarshalTag : int32 -> hasFieldMarshal_tag
val mkHasDeclSecurityTag : int32 -> hasDeclSecurity_tag
val mkMemberRefParentTag : int32 -> memberRefParent_tag
val mkHasSemanticsTag : int32 -> hasSemantics_tag
val mkMethodDefOrRefTag : int32 -> methodDefOrRef_tag
val mkMemberForwardedTag : int32 -> memberForwarded_tag
val mkImplementationTag : int32 -> implementation_tag
val mkCustomAttributeTypeTag : int32 -> customAttributeType_tag
val mkResolutionScopeTag : int32 -> resolutionScope_tag
val mkTypeOrMethodDefTag : int32 -> typeOrMethodDef_tag

val et_END : int
val et_VOID : int
val et_BOOLEAN : int
val et_CHAR : int
val et_I1 : int
val et_U1 : int
val et_I2 : int
val et_U2 : int
val et_I4 : int
val et_U4 : int
val et_I8 : int
val et_U8 : int
val et_R4 : int
val et_R8 : int
val et_STRING : int
val et_PTR : int
val et_BYREF : int
val et_VALUETYPE : int
val et_CLASS : int
val et_VAR : int
val et_ARRAY : int
val et_WITH : int
val et_TYPEDBYREF : int
val et_I : int
val et_U : int
val et_FNPTR : int
val et_OBJECT : int
val et_SZARRAY : int
val et_MVAR : int
val et_CMOD_REQD : int
val et_CMOD_OPT : int
val et_SENTINEL : int
val et_PINNED : int
val i_nop : int
val i_break : int
val i_ldarg_0 : int
val i_ldarg_1 : int
val i_ldarg_2 : int
val i_ldarg_3 : int
val i_ldloc_0 : int
val i_ldloc_1 : int
val i_ldloc_2 : int
val i_ldloc_3 : int
val i_stloc_0 : int
val i_stloc_1 : int
val i_stloc_2 : int
val i_stloc_3 : int
val i_ldarg_s : int
val i_ldarga_s : int
val i_starg_s : int
val i_ldloc_s : int
val i_ldloca_s : int
val i_stloc_s : int
val i_ldnull : int
val i_ldc_i4_m1 : int
val i_ldc_i4_0 : int
val i_ldc_i4_1 : int
val i_ldc_i4_2 : int
val i_ldc_i4_3 : int
val i_ldc_i4_4 : int
val i_ldc_i4_5 : int
val i_ldc_i4_6 : int
val i_ldc_i4_7 : int
val i_ldc_i4_8 : int
val i_ldc_i4_s : int
val i_ldc_i4 : int
val i_ldc_i8 : int
val i_ldc_r4 : int
val i_ldc_r8 : int
val i_dup : int
val i_pop : int
val i_jmp : int
val i_call : int
val i_calli : int
val i_ret : int
val i_br_s : int
val i_brfalse_s : int
val i_brtrue_s : int
val i_beq_s : int
val i_bge_s : int
val i_bgt_s : int
val i_ble_s : int
val i_blt_s : int
val i_bne_un_s : int
val i_bge_un_s : int
val i_bgt_un_s : int
val i_ble_un_s : int
val i_blt_un_s : int
val i_br : int
val i_brfalse : int
val i_brtrue : int
val i_beq : int
val i_bge : int
val i_bgt : int
val i_ble : int
val i_blt : int
val i_bne_un : int
val i_bge_un : int
val i_bgt_un : int
val i_ble_un : int
val i_blt_un : int
val i_switch : int
val i_ldind_i1 : int
val i_ldind_u1 : int
val i_ldind_i2 : int
val i_ldind_u2 : int
val i_ldind_i4 : int
val i_ldind_u4 : int
val i_ldind_i8 : int
val i_ldind_i : int
val i_ldind_r4 : int
val i_ldind_r8 : int
val i_ldind_ref : int
val i_stind_ref : int
val i_stind_i1 : int
val i_stind_i2 : int
val i_stind_i4 : int
val i_stind_i8 : int
val i_stind_r4 : int
val i_stind_r8 : int
val i_add : int
val i_sub : int
val i_mul : int
val i_div : int
val i_div_un : int
val i_rem : int
val i_rem_un : int
val i_and : int
val i_or : int
val i_xor : int
val i_shl : int
val i_shr : int
val i_shr_un : int
val i_neg : int
val i_not : int
val i_conv_i1 : int
val i_conv_i2 : int
val i_conv_i4 : int
val i_conv_i8 : int
val i_conv_r4 : int
val i_conv_r8 : int
val i_conv_u4 : int
val i_conv_u8 : int
val i_callvirt : int
val i_cpobj : int
val i_ldobj : int
val i_ldstr : int
val i_newobj : int
val i_castclass : int
val i_isinst : int
val i_conv_r_un : int
val i_unbox : int
val i_throw : int
val i_ldfld : int
val i_ldflda : int
val i_stfld : int
val i_ldsfld : int
val i_ldsflda : int
val i_stsfld : int
val i_stobj : int
val i_conv_ovf_i1_un : int
val i_conv_ovf_i2_un : int
val i_conv_ovf_i4_un : int
val i_conv_ovf_i8_un : int
val i_conv_ovf_u1_un : int
val i_conv_ovf_u2_un : int
val i_conv_ovf_u4_un : int
val i_conv_ovf_u8_un : int
val i_conv_ovf_i_un : int
val i_conv_ovf_u_un : int
val i_box : int
val i_newarr : int
val i_ldlen : int
val i_ldelema : int
val i_ldelem_i1 : int
val i_ldelem_u1 : int
val i_ldelem_i2 : int
val i_ldelem_u2 : int
val i_ldelem_i4 : int
val i_ldelem_u4 : int
val i_ldelem_i8 : int
val i_ldelem_i : int
val i_ldelem_r4 : int
val i_ldelem_r8 : int
val i_ldelem_ref : int
val i_stelem_i : int
val i_stelem_i1 : int
val i_stelem_i2 : int
val i_stelem_i4 : int
val i_stelem_i8 : int
val i_stelem_r4 : int
val i_stelem_r8 : int
val i_stelem_ref : int
val i_conv_ovf_i1 : int
val i_conv_ovf_u1 : int
val i_conv_ovf_i2 : int
val i_conv_ovf_u2 : int
val i_conv_ovf_i4 : int
val i_conv_ovf_u4 : int
val i_conv_ovf_i8 : int
val i_conv_ovf_u8 : int
val i_refanyval : int
val i_ckfinite : int
val i_mkrefany : int
val i_ldtoken : int
val i_conv_u2 : int
val i_conv_u1 : int
val i_conv_i : int
val i_conv_ovf_i : int
val i_conv_ovf_u : int
val i_add_ovf : int
val i_add_ovf_un : int
val i_mul_ovf : int
val i_mul_ovf_un : int
val i_sub_ovf : int
val i_sub_ovf_un : int
val i_endfinally : int
val i_leave : int
val i_leave_s : int
val i_stind_i : int
val i_conv_u : int
val i_arglist : int
val i_ceq : int
val i_cgt : int
val i_cgt_un : int
val i_clt : int
val i_clt_un : int
val i_ldftn : int
val i_ldvirtftn : int
val i_ldarg : int
val i_ldarga : int
val i_starg : int
val i_ldloc : int
val i_ldloca : int
val i_stloc : int
val i_localloc : int
val i_endfilter : int
val i_unaligned : int
val i_volatile : int
val i_constrained : int
val i_readonly : int
val i_tail : int
val i_initobj : int
val i_cpblk : int
val i_initblk : int
val i_rethrow : int
val i_sizeof : int
val i_refanytype : int
val i_ldelem_any : int
val i_stelem_any : int
val i_unbox_any : int
val noarg_instrs : Lazy<(int * ILInstr) list>
val is_noarg_instr : ILInstr -> bool
val brcmp_map : System.Collections.Generic.Dictionary<ILComparisonInstr,int> Lazy.t
val brcmp_smap : System.Collections.Generic.Dictionary<ILComparisonInstr, int> Lazy.t
val nt_VOID : int
val nt_BOOLEAN : int
val nt_I1 : int
val nt_U1 : int
val nt_I2 : int
val nt_U2 : int
val nt_I4 : int
val nt_U4 : int
val nt_I8 : int
val nt_U8 : int
val nt_R4 : int
val nt_R8 : int
val nt_SYSCHAR : int
val nt_VARIANT : int
val nt_CURRENCY : int
val nt_PTR : int
val nt_DECIMAL : int
val nt_DATE : int
val nt_BSTR : int
val nt_LPSTR : int
val nt_LPWSTR : int
val nt_LPTSTR : int
val nt_FIXEDSYSSTRING : int
val nt_OBJECTREF : int
val nt_IUNKNOWN : int
val nt_IDISPATCH : int
val nt_STRUCT : int
val nt_INTF : int
val nt_SAFEARRAY : int
val nt_FIXEDARRAY : int
val nt_INT : int
val nt_UINT : int
val nt_NESTEDSTRUCT : int
val nt_BYVALSTR : int
val nt_ANSIBSTR : int
val nt_TBSTR : int
val nt_VARIANTBOOL : int
val nt_FUNC : int
val nt_ASANY : int
val nt_ARRAY : int
val nt_LPSTRUCT : int
val nt_CUSTOMMARSHALER : int
val nt_ERROR : int
val nt_MAX : int
val vt_EMPTY : int32
val vt_NULL : int32
val vt_I2 : int32
val vt_I4 : int32
val vt_R4 : int32
val vt_R8 : int32
val vt_CY : int32
val vt_DATE : int32
val vt_BSTR : int32
val vt_DISPATCH : int32
val vt_ERROR : int32
val vt_BOOL : int32
val vt_VARIANT : int32
val vt_UNKNOWN : int32
val vt_DECIMAL : int32
val vt_I1 : int32
val vt_UI1 : int32
val vt_UI2 : int32
val vt_UI4 : int32
val vt_I8 : int32
val vt_UI8 : int32
val vt_INT : int32
val vt_UINT : int32
val vt_VOID : int32
val vt_HRESULT : int32
val vt_PTR : int32
val vt_SAFEARRAY : int32
val vt_CARRAY : int32
val vt_USERDEFINED : int32
val vt_LPSTR : int32
val vt_LPWSTR : int32
val vt_RECORD : int32
val vt_FILETIME : int32
val vt_BLOB : int32
val vt_STREAM : int32
val vt_STORAGE : int32
val vt_STREAMED_OBJECT : int32
val vt_STORED_OBJECT : int32
val vt_BLOB_OBJECT : int32
val vt_CF : int32
val vt_CLSID : int32
val vt_VECTOR : int32
val vt_ARRAY : int32
val vt_BYREF : int32
val native_type_map : (int * ILNativeType) list Lazy.t
val native_type_rmap : (ILNativeType * int) list Lazy.t
val variant_type_map : (ILNativeVariantType * int32) list Lazy.t
val variant_type_rmap : (int32 * ILNativeVariantType) list Lazy.t
val secaction_map : (ILSecurityAction * int) list Lazy.t
val secaction_rmap : (int * ILSecurityAction) list Lazy.t
val e_CorILMethod_TinyFormat : int32
val e_CorILMethod_FatFormat : int32
val e_CorILMethod_FormatMask : int32
val e_CorILMethod_MoreSects : int32
val e_CorILMethod_InitLocals : int32
val e_CorILMethod_Sect_EHTable : int32
val e_CorILMethod_Sect_FatFormat : int32
val e_CorILMethod_Sect_MoreSects : int32
val e_COR_ILEXCEPTION_CLAUSE_EXCEPTION : int32
val e_COR_ILEXCEPTION_CLAUSE_FILTER : int32
val e_COR_ILEXCEPTION_CLAUSE_FINALLY : int32
val e_COR_ILEXCEPTION_CLAUSE_FAULT : int32

val e_IMAGE_CEE_CS_CALLCONV_FASTCALL : int
val e_IMAGE_CEE_CS_CALLCONV_STDCALL : int
val e_IMAGE_CEE_CS_CALLCONV_THISCALL : int
val e_IMAGE_CEE_CS_CALLCONV_CDECL : int
val e_IMAGE_CEE_CS_CALLCONV_VARARG : int

val e_IMAGE_CEE_CS_CALLCONV_FIELD : int
val e_IMAGE_CEE_CS_CALLCONV_LOCAL_SIG : int
val e_IMAGE_CEE_CS_CALLCONV_GENERICINST : int
val e_IMAGE_CEE_CS_CALLCONV_PROPERTY : int

val e_IMAGE_CEE_CS_CALLCONV_INSTANCE : int
val e_IMAGE_CEE_CS_CALLCONV_INSTANCE_EXPLICIT : int
val e_IMAGE_CEE_CS_CALLCONV_GENERIC : int


