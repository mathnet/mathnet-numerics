// (c) Microsoft Corporation 2005-2009. 


module Microsoft.FSharp.Compiler.AbstractIL.Internal.BinaryConstants 

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library

type table = Table of int
let tag_of_table (Table n) = n

let tab_Module               = Table 0  
let tab_TypeRef              = Table 1  
let tab_TypeDef              = Table 2  
let tab_FieldPtr             = Table 3  
let tab_Field                = Table 4  
let tab_MethodPtr            = Table 5  
let tab_Method               = Table 6  
let tab_ParamPtr             = Table 7  
let tab_Param                = Table 8  
let tab_InterfaceImpl        = Table 9  
let tab_MemberRef            = Table 10 
let tab_Constant             = Table 11 
let tab_CustomAttribute      = Table 12 
let tab_FieldMarshal         = Table 13 
let tab_Permission           = Table 14 
let tab_ClassLayout          = Table 15 
let tab_FieldLayout          = Table 16 
let tab_StandAloneSig        = Table 17 
let tab_EventMap             = Table 18 
let tab_EventPtr             = Table 19 
let tab_Event                = Table 20 
let tab_PropertyMap          = Table 21 
let tab_PropertyPtr          = Table 22 
let tab_Property             = Table 23 
let tab_MethodSemantics      = Table 24 
let tab_MethodImpl           = Table 25 
let tab_ModuleRef            = Table 26 
let tab_TypeSpec             = Table 27 
let tab_ImplMap              = Table 28 
let tab_FieldRVA             = Table 29 
let tab_ENCLog               = Table 30 
let tab_ENCMap               = Table 31 
let tab_Assembly             = Table 32 
let tab_AssemblyProcessor    = Table 33 
let tab_AssemblyOS           = Table 34 
let tab_AssemblyRef          = Table 35 
let tab_AssemblyRefProcessor = Table 36 
let tab_AssemblyRefOS        = Table 37 
let tab_File                 = Table 38 
let tab_ExportedType         = Table 39 
let tab_ManifestResource     = Table 40 
let tab_Nested               = Table 41 
let tab_GenericParam           = Table 42 
let tab_MethodSpec           = Table 43 
let tab_GenericParamConstraint = Table 44

let tab_UserStrings           = Table 0x70 (* Special encoding of embedded UserString tokens - See 1.9 Partition III *) 

(* Sorted bit-vector as stored by CLR V1: 00fa 0133 0002 0000 *)
(* But what does this mean?  The ECMA spec does not say! *)
(* Metainfo -schema reports sorting as shown below. *)
(* But some sorting, e.g. EventMap does not seem to show *)

(* Which tables are sorted and by which column *)
let sorted_table_info = 
  [ (tab_InterfaceImpl,0); 
    (tab_Constant, 1);
    (tab_CustomAttribute, 0);
    (tab_FieldMarshal, 0);
    (tab_Permission, 1);
    (tab_ClassLayout, 2);
    (tab_FieldLayout, 1);
    (tab_MethodSemantics, 2);
    (tab_MethodImpl, 0);
    (tab_ImplMap, 1);
    (tab_FieldRVA, 1);
    (tab_Nested, 0);
    (tab_GenericParam, 2); 
    (tab_GenericParamConstraint, 0); ]
    
type typeDefOrRef_tag = TypeDefOrRefOrSpecTag of int32
let tdor_TypeDef = TypeDefOrRefOrSpecTag 0x00
let tdor_TypeRef = TypeDefOrRefOrSpecTag 0x01
let tdor_TypeSpec = TypeDefOrRefOrSpecTag 0x2
let mkTypeDefOrRefOrSpecTag x = 
    match x with 
    | 0x00 -> tdor_TypeDef // nb. avoid reallocation 
    | 0x01 -> tdor_TypeRef
    | 0x02 -> tdor_TypeSpec
    | _ -> invalid_arg "mkTypeDefOrRefOrSpecTag"

type hasConstant_tag = HasConstantTag of int32
let hc_FieldDef  = HasConstantTag 0x0
let hc_ParamDef  = HasConstantTag 0x1
let hc_Property = HasConstantTag 0x2

let mkHasConstantTag x = 
    match x with 
    | 0x00l -> hc_FieldDef
    | 0x01l -> hc_ParamDef
    | 0x02l -> hc_Property
    | _ -> invalid_arg "mkHasConstantTag"

type hasCustomAttribute_tag	= HasCustomAttributeTag of int32
let hca_MethodDef				= HasCustomAttributeTag 0x0
let hca_FieldDef				= HasCustomAttributeTag 0x1
let hca_TypeRef					= HasCustomAttributeTag 0x2
let hca_TypeDef					= HasCustomAttributeTag 0x3
let hca_ParamDef				= HasCustomAttributeTag 0x4
let hca_InterfaceImpl			= HasCustomAttributeTag 0x5
let hca_MemberRef				= HasCustomAttributeTag 0x6
let hca_Module					= HasCustomAttributeTag 0x7
let hca_Permission				= HasCustomAttributeTag 0x8
let hca_Property				= HasCustomAttributeTag 0x9
let hca_Event					= HasCustomAttributeTag 0xa
let hca_StandAloneSig			= HasCustomAttributeTag 0xb
let hca_ModuleRef				= HasCustomAttributeTag 0xc
let hca_TypeSpec				= HasCustomAttributeTag 0xd
let hca_Assembly				= HasCustomAttributeTag 0xe
let hca_AssemblyRef				= HasCustomAttributeTag 0xf
let hca_File					= HasCustomAttributeTag 0x10
let hca_ExportedType			= HasCustomAttributeTag 0x11
let hca_ManifestResource        = HasCustomAttributeTag 0x12
let hca_GenericParam            = HasCustomAttributeTag 0x13
let hca_GenericParamConstraint  = HasCustomAttributeTag 0x14
let hca_MethodSpec              = HasCustomAttributeTag 0x15

let mkHasCustomAttributeTag x = 
    match x with 
    | 0x00 -> hca_MethodDef  
    | 0x01 -> hca_FieldDef 
    | 0x02 -> hca_TypeRef  
    | 0x03 -> hca_TypeDef 
    | 0x04 -> hca_ParamDef 
    | 0x05 -> hca_InterfaceImpl 
    | 0x06 -> hca_MemberRef 
    | 0x07 -> hca_Module 
    | 0x08 -> hca_Permission 
    | 0x09 -> hca_Property 
    | 0x0a -> hca_Event 
    | 0x0b -> hca_StandAloneSig 
    | 0x0c -> hca_ModuleRef 
    | 0x0d -> hca_TypeSpec 
    | 0x0e -> hca_Assembly 
    | 0x0f -> hca_AssemblyRef 
    | 0x10 -> hca_File 
    | 0x11 -> hca_ExportedType 
    | 0x12 -> hca_ManifestResource
    | 0x13 -> hca_GenericParam
    | 0x14 -> hca_GenericParamConstraint
    | 0x15 -> hca_MethodSpec 
    | _ -> HasCustomAttributeTag x

type hasFieldMarshal_tag = HasFieldMarshalTag of int32
let hfm_FieldDef =  HasFieldMarshalTag 0x00
let hfm_ParamDef =  HasFieldMarshalTag 0x01

let mkHasFieldMarshalTag x = 
    match x with 
    | 0x00 -> hfm_FieldDef 
    | 0x01 -> hfm_ParamDef 
    | _ -> HasFieldMarshalTag x

type hasDeclSecurity_tag = HasDeclSecurityTag of int32
let hds_TypeDef =  HasDeclSecurityTag 0x00
let hds_MethodDef =  HasDeclSecurityTag 0x01
let hds_Assembly =  HasDeclSecurityTag 0x02

let mkHasDeclSecurityTag x = 
    match x with 
    | 0x00 -> hds_TypeDef 
    | 0x01 -> hds_MethodDef 
    | 0x02 -> hds_Assembly 
    | _ -> HasDeclSecurityTag x

type memberRefParent_tag = MemberRefParentTag of int32
let mrp_TypeRef = MemberRefParentTag 0x01
let mrp_ModuleRef = MemberRefParentTag 0x02
let mrp_MethodDef = MemberRefParentTag 0x03
let mrp_TypeSpec  = MemberRefParentTag 0x04

let mkMemberRefParentTag x = 
    match x with 
    | 0x01 -> mrp_TypeRef 
    | 0x02 -> mrp_ModuleRef 
    | 0x03 -> mrp_MethodDef 
    | 0x04 -> mrp_TypeSpec  
    | _ -> MemberRefParentTag x

type hasSemantics_tag = HasSemanticsTag of int32
let hs_Event =  HasSemanticsTag 0x00
let hs_Property =  HasSemanticsTag 0x01

let mkHasSemanticsTag x = 
    match x with 
    | 0x00 -> hs_Event 
    | 0x01 -> hs_Property 
    | _ -> HasSemanticsTag x

type methodDefOrRef_tag = MethodDefOrRefTag of int32
let mdor_MethodDef =  MethodDefOrRefTag 0x00
let mdor_MemberRef =  MethodDefOrRefTag 0x01
let mdor_MethodSpec =  MethodDefOrRefTag 0x02

let mkMethodDefOrRefTag x = 
    match x with 
    | 0x00 -> mdor_MethodDef 
    | 0x01 -> mdor_MemberRef 
    | 0x02 -> mdor_MethodSpec 
    | _ -> MethodDefOrRefTag x

type memberForwarded_tag = MemberForwardedTag of int32
let mf_FieldDef =  MemberForwardedTag 0x00
let mf_MethodDef =  MemberForwardedTag 0x01

let mkMemberForwardedTag x = 
    match x with 
    | 0x00 -> mf_FieldDef 
    | 0x01 -> mf_MethodDef 
    | _ -> MemberForwardedTag x

type implementation_tag = ImplementationTag of int32
let i_File =  ImplementationTag 0x00
let i_AssemblyRef =  ImplementationTag 0x01
let i_ExportedType =  ImplementationTag 0x02

let mkImplementationTag x = 
    match x with 
    | 0x00 -> i_File 
    | 0x01 -> i_AssemblyRef 
    | 0x02 -> i_ExportedType 
    | _ -> ImplementationTag x

type customAttributeType_tag = CustomAttributeTypeTag of int32
let cat_MethodDef =  CustomAttributeTypeTag 0x02
let cat_MemberRef =  CustomAttributeTypeTag 0x03

let mkCustomAttributeTypeTag x = 
    match x with 
    | 0x02 -> cat_MethodDef 
    | 0x03 -> cat_MemberRef 
    | _ -> CustomAttributeTypeTag x

type resolutionScope_tag = ResolutionScopeTag of int32
let rs_Module =  ResolutionScopeTag 0x00
let rs_ModuleRef =  ResolutionScopeTag 0x01
let rs_AssemblyRef  =  ResolutionScopeTag 0x02
let rs_TypeRef =  ResolutionScopeTag 0x03

let mkResolutionScopeTag x = 
    match x with 
    | 0x00 -> rs_Module 
    | 0x01 -> rs_ModuleRef 
    | 0x02 -> rs_AssemblyRef  
    | 0x03 -> rs_TypeRef 
    | _ -> ResolutionScopeTag x

type typeOrMethodDef_tag = TypeOrMethodDefTag of int32
let tomd_TypeDef = TypeOrMethodDefTag 0x00
let tomd_MethodDef = TypeOrMethodDefTag 0x01

let mkTypeOrMethodDefTag x = 
    match x with 
    | 0x00 -> tomd_TypeDef 
    | 0x01 -> tomd_MethodDef
    | _ -> TypeOrMethodDefTag x

let et_END = 0x00
let et_VOID = 0x01
let et_BOOLEAN = 0x02
let et_CHAR = 0x03
let et_I1 = 0x04
let et_U1 = 0x05
let et_I2 = 0x06
let et_U2 = 0x07
let et_I4 = 0x08
let et_U4 = 0x09
let et_I8 = 0x0a
let et_U8 = 0x0b
let et_R4 = 0x0c
let et_R8 = 0x0d
let et_STRING = 0x0e
let et_PTR = 0x0f
let et_BYREF = 0x10
let et_VALUETYPE      = 0x11
let et_CLASS          = 0x12
let et_VAR            = 0x13
let et_ARRAY          = 0x14
let et_WITH           = 0x15
let et_TYPEDBYREF     = 0x16
let et_I              = 0x18
let et_U              = 0x19
let et_FNPTR          = 0x1B
let et_OBJECT         = 0x1C
let et_SZARRAY        = 0x1D
let et_MVAR           = 0x1e
let et_CMOD_REQD      = 0x1F
let et_CMOD_OPT       = 0x20

let et_SENTINEL       = 0x41 (* sentinel for varargs *)
let et_PINNED         = 0x45


let i_nop           = 0x00 
let i_break         = 0x01 
let i_ldarg_0       = 0x02 
let i_ldarg_1       = 0x03 
let i_ldarg_2       = 0x04 
let i_ldarg_3       = 0x05 
let i_ldloc_0       = 0x06 
let i_ldloc_1       = 0x07 
let i_ldloc_2       = 0x08 
let i_ldloc_3       = 0x09 
let i_stloc_0       = 0x0a 
let i_stloc_1       = 0x0b 
let i_stloc_2       = 0x0c 
let i_stloc_3       = 0x0d 
let i_ldarg_s       = 0x0e 
let i_ldarga_s      = 0x0f 
let i_starg_s       = 0x10 
let i_ldloc_s       = 0x11 
let i_ldloca_s      = 0x12 
let i_stloc_s       = 0x13 
let i_ldnull        = 0x14 
let i_ldc_i4_m1     = 0x15 
let i_ldc_i4_0      = 0x16 
let i_ldc_i4_1      = 0x17 
let i_ldc_i4_2      = 0x18 
let i_ldc_i4_3      = 0x19 
let i_ldc_i4_4      = 0x1a 
let i_ldc_i4_5      = 0x1b 
let i_ldc_i4_6      = 0x1c 
let i_ldc_i4_7      = 0x1d 
let i_ldc_i4_8      = 0x1e 
let i_ldc_i4_s      = 0x1f 
let i_ldc_i4        = 0x20 
let i_ldc_i8        = 0x21 
let i_ldc_r4        = 0x22 
let i_ldc_r8        = 0x23 
let i_dup           = 0x25 
let i_pop           = 0x26 
let i_jmp           = 0x27 
let i_call          = 0x28 
let i_calli         = 0x29 
let i_ret           = 0x2a 
let i_br_s          = 0x2b 
let i_brfalse_s     = 0x2c 
let i_brtrue_s      = 0x2d 
let i_beq_s         = 0x2e 
let i_bge_s         = 0x2f 
let i_bgt_s         = 0x30 
let i_ble_s         = 0x31 
let i_blt_s         = 0x32 
let i_bne_un_s      = 0x33 
let i_bge_un_s      = 0x34 
let i_bgt_un_s      = 0x35 
let i_ble_un_s      = 0x36 
let i_blt_un_s      = 0x37 
let i_br            = 0x38 
let i_brfalse       = 0x39 
let i_brtrue        = 0x3a 
let i_beq           = 0x3b 
let i_bge           = 0x3c 
let i_bgt           = 0x3d 
let i_ble           = 0x3e 
let i_blt           = 0x3f 
let i_bne_un        = 0x40 
let i_bge_un        = 0x41 
let i_bgt_un        = 0x42 
let i_ble_un        = 0x43 
let i_blt_un        = 0x44 
let i_switch        = 0x45 
let i_ldind_i1      = 0x46 
let i_ldind_u1      = 0x47 
let i_ldind_i2      = 0x48 
let i_ldind_u2      = 0x49 
let i_ldind_i4      = 0x4a 
let i_ldind_u4      = 0x4b 
let i_ldind_i8      = 0x4c 
let i_ldind_i       = 0x4d 
let i_ldind_r4      = 0x4e 
let i_ldind_r8      = 0x4f 
let i_ldind_ref     = 0x50 
let i_stind_ref     = 0x51 
let i_stind_i1      = 0x52 
let i_stind_i2      = 0x53 
let i_stind_i4      = 0x54 
let i_stind_i8      = 0x55 
let i_stind_r4      = 0x56 
let i_stind_r8      = 0x57 
let i_add           = 0x58 
let i_sub           = 0x59 
let i_mul           = 0x5a 
let i_div           = 0x5b 
let i_div_un        = 0x5c 
let i_rem           = 0x5d 
let i_rem_un        = 0x5e 
let i_and           = 0x5f 
let i_or            = 0x60 
let i_xor           = 0x61 
let i_shl           = 0x62 
let i_shr           = 0x63 
let i_shr_un        = 0x64 
let i_neg           = 0x65 
let i_not           = 0x66 
let i_conv_i1       = 0x67 
let i_conv_i2       = 0x68 
let i_conv_i4       = 0x69 
let i_conv_i8       = 0x6a 
let i_conv_r4       = 0x6b 
let i_conv_r8       = 0x6c 
let i_conv_u4       = 0x6d 
let i_conv_u8       = 0x6e 
let i_callvirt      = 0x6f 
let i_cpobj         = 0x70 
let i_ldobj         = 0x71 
let i_ldstr         = 0x72 
let i_newobj        = 0x73 
let i_castclass     = 0x74 
let i_isinst        = 0x75 
let i_conv_r_un     = 0x76 
let i_unbox         = 0x79 
let i_throw         = 0x7a 
let i_ldfld         = 0x7b 
let i_ldflda        = 0x7c 
let i_stfld         = 0x7d 
let i_ldsfld        = 0x7e 
let i_ldsflda       = 0x7f 
let i_stsfld        = 0x80 
let i_stobj         = 0x81 
let i_conv_ovf_i1_un= 0x82 
let i_conv_ovf_i2_un= 0x83 
let i_conv_ovf_i4_un= 0x84 
let i_conv_ovf_i8_un= 0x85 
let i_conv_ovf_u1_un= 0x86 
let i_conv_ovf_u2_un= 0x87 
let i_conv_ovf_u4_un= 0x88 
let i_conv_ovf_u8_un= 0x89 
let i_conv_ovf_i_un = 0x8a 
let i_conv_ovf_u_un = 0x8b 
let i_box           = 0x8c 
let i_newarr        = 0x8d 
let i_ldlen         = 0x8e 
let i_ldelema       = 0x8f 
let i_ldelem_i1     = 0x90 
let i_ldelem_u1     = 0x91 
let i_ldelem_i2     = 0x92 
let i_ldelem_u2     = 0x93 
let i_ldelem_i4     = 0x94 
let i_ldelem_u4     = 0x95 
let i_ldelem_i8     = 0x96 
let i_ldelem_i      = 0x97 
let i_ldelem_r4     = 0x98 
let i_ldelem_r8     = 0x99 
let i_ldelem_ref    = 0x9a 
let i_stelem_i      = 0x9b 
let i_stelem_i1     = 0x9c 
let i_stelem_i2     = 0x9d 
let i_stelem_i4     = 0x9e 
let i_stelem_i8     = 0x9f 
let i_stelem_r4     = 0xa0 
let i_stelem_r8     = 0xa1 
let i_stelem_ref    = 0xa2 
let i_conv_ovf_i1   = 0xb3 
let i_conv_ovf_u1   = 0xb4 
let i_conv_ovf_i2   = 0xb5 
let i_conv_ovf_u2   = 0xb6 
let i_conv_ovf_i4   = 0xb7 
let i_conv_ovf_u4   = 0xb8 
let i_conv_ovf_i8   = 0xb9 
let i_conv_ovf_u8   = 0xba 
let i_refanyval     = 0xc2 
let i_ckfinite      = 0xc3 
let i_mkrefany      = 0xc6 
let i_ldtoken       = 0xd0 
let i_conv_u2       = 0xd1 
let i_conv_u1       = 0xd2 
let i_conv_i        = 0xd3 
let i_conv_ovf_i    = 0xd4 
let i_conv_ovf_u    = 0xd5 
let i_add_ovf       = 0xd6 
let i_add_ovf_un    = 0xd7 
let i_mul_ovf       = 0xd8 
let i_mul_ovf_un    = 0xd9 
let i_sub_ovf       = 0xda 
let i_sub_ovf_un    = 0xdb 
let i_endfinally    = 0xdc 
let i_leave         = 0xdd 
let i_leave_s       = 0xde 
let i_stind_i       = 0xdf 
let i_conv_u        = 0xe0 
let i_arglist        = 0xfe00
let i_ceq        = 0xfe01
let i_cgt        = 0xfe02
let i_cgt_un        = 0xfe03
let i_clt        = 0xfe04
let i_clt_un        = 0xfe05
let i_ldftn        = 0xfe06 
let i_ldvirtftn    = 0xfe07 
let i_ldarg          = 0xfe09 
let i_ldarga         = 0xfe0a 
let i_starg          = 0xfe0b 
let i_ldloc          = 0xfe0c 
let i_ldloca         = 0xfe0d 
let i_stloc          = 0xfe0e 
let i_localloc     = 0xfe0f 
let i_endfilter    = 0xfe11 
let i_unaligned   = 0xfe12 
let i_volatile    = 0xfe13 
let i_constrained    = 0xfe16
let i_readonly    = 0xfe1e
let i_tail           = 0xfe14 
let i_initobj        = 0xfe15 
let i_cpblk          = 0xfe17 
let i_initblk        = 0xfe18 
let i_rethrow        = 0xfe1a 
let i_sizeof         = 0xfe1c 
let i_refanytype   = 0xfe1d 

let i_ldelem_any = 0xa3
let i_stelem_any = 0xa4
let i_unbox_any = 0xa5

let mk_ldc i = (((mk_ldc_i32 (i))))
let noarg_instrs  = lazy 
 [
  i_ldc_i4_0,           mk_ldc 0;
  i_ldc_i4_1,           mk_ldc 1;
  i_ldc_i4_2,           mk_ldc 2;
  i_ldc_i4_3,           mk_ldc 3;
  i_ldc_i4_4,           mk_ldc 4;
  i_ldc_i4_5,           mk_ldc 5;
  i_ldc_i4_6,           mk_ldc 6;
  i_ldc_i4_7,           mk_ldc 7;
  i_ldc_i4_8,           mk_ldc 8;
  i_ldc_i4_m1,           mk_ldc (0-1);
  0x0a,            (I_stloc (uint16 ( 0)));
  0x0b,            (I_stloc (uint16 ( 1)));
  0x0c,            (I_stloc (uint16 ( 2)));
  0x0d,            (I_stloc (uint16 ( 3)));
  0x06,            (I_ldloc (uint16 ( 0)));
  0x07,            (I_ldloc (uint16 ( 1)));
  0x08,            (I_ldloc (uint16 ( 2)));
  0x09,            (I_ldloc (uint16 ( 3)));
  0x02,            (I_ldarg (uint16 ( 0)));
  0x03,            (I_ldarg (uint16 ( 1)));
  0x04,            (I_ldarg (uint16 ( 2)));
  0x05,            (I_ldarg (uint16 ( 3)));
  0x2a,              (I_ret);
  0x58,              (I_arith AI_add);
  0xd6,        (I_arith AI_add_ovf);
  0xd7,   (I_arith AI_add_ovf_un);
  0x5f,              (I_arith AI_and);  
  0x5b,              (I_arith AI_div); 
  0x5c,         (I_arith AI_div_un);
  0xfe01,              (I_arith AI_ceq);  
  0xfe02,              (I_arith AI_cgt );
  0xfe03,         (I_arith AI_cgt_un);
  0xfe04,              (I_arith AI_clt);
  0xfe05,         (I_arith AI_clt_un);
  0x67,        (I_arith (AI_conv DT_I1));  
  0x68,   (I_arith (AI_conv DT_I2));  
  0x69,   (I_arith (AI_conv DT_I4));  
  0x6a,   (I_arith (AI_conv DT_I8));  
  0xd3,   (I_arith (AI_conv DT_I));  
  0x6b,   (I_arith (AI_conv DT_R4));  
  0x6c,   (I_arith (AI_conv DT_R8));  
  0xd2,   (I_arith (AI_conv DT_U1));  
  0xd1,   (I_arith (AI_conv DT_U2));  
  0x6d,   (I_arith (AI_conv DT_U4));  
  0x6e,   (I_arith (AI_conv DT_U8));  
  0xe0,   (I_arith (AI_conv DT_U));  
  0x76,   (I_arith (AI_conv DT_R));  
  0xb3,   (I_arith (AI_conv_ovf DT_I1));  
  0xb5,   (I_arith (AI_conv_ovf DT_I2));  
  0xb7,   (I_arith (AI_conv_ovf DT_I4));  
  0xb9,   (I_arith (AI_conv_ovf DT_I8));  
  0xd4,   (I_arith (AI_conv_ovf DT_I));  
  0xb4,   (I_arith (AI_conv_ovf DT_U1));  
  0xb6,   (I_arith (AI_conv_ovf DT_U2));  
  0xb8,   (I_arith (AI_conv_ovf DT_U4));  
  0xba,   (I_arith (AI_conv_ovf DT_U8));  
  0xd5,   (I_arith (AI_conv_ovf DT_U));  
  0x82,   (I_arith (AI_conv_ovf_un DT_I1));  
  0x83,   (I_arith (AI_conv_ovf_un DT_I2));  
  0x84,   (I_arith (AI_conv_ovf_un DT_I4));  
  0x85,   (I_arith (AI_conv_ovf_un DT_I8));  
  0x8a,   (I_arith (AI_conv_ovf_un DT_I));  
  0x86,   (I_arith (AI_conv_ovf_un DT_U1));  
  0x87,   (I_arith (AI_conv_ovf_un DT_U2));  
  0x88,   (I_arith (AI_conv_ovf_un DT_U4));  
  0x89,   (I_arith (AI_conv_ovf_un DT_U8));  
  0x8b,   (I_arith (AI_conv_ovf_un DT_U));  
  0x9c,   (I_stelem DT_I1);  
  0x9d,   (I_stelem DT_I2);  
  0x9e,   (I_stelem DT_I4);  
  0x9f,   (I_stelem DT_I8);  
  0xa0,   (I_stelem DT_R4);  
  0xa1,   (I_stelem DT_R8);  
  0x9b,   (I_stelem DT_I);  
  0xa2,   (I_stelem DT_REF);  
  0x90,   (I_ldelem DT_I1);  
  0x92,   (I_ldelem DT_I2);  
  0x94,   (I_ldelem DT_I4);  
  0x96,   (I_ldelem DT_I8);  
  0x91,   (I_ldelem DT_U1);  
  0x93,   (I_ldelem DT_U2);  
  0x95,   (I_ldelem DT_U4);  
  0x98,   (I_ldelem DT_R4);  
  0x99,   (I_ldelem DT_R8);  
  0x97,   (I_ldelem DT_I);  
  0x9a,   (I_ldelem DT_REF);  
  0x5a,   (I_arith AI_mul  );
  0xd8,   (I_arith AI_mul_ovf);
  0xd9,   (I_arith AI_mul_ovf_un);
  0x5d,   (I_arith AI_rem  );
  0x5e,   (I_arith AI_rem_un ); 
  0x62,   (I_arith AI_shl ); 
  0x63,   (I_arith AI_shr ); 
  0x64,   (I_arith AI_shr_un);
  0x59,   (I_arith AI_sub  );
  0xda,   (I_arith AI_sub_ovf);
  0xdb,   (I_arith AI_sub_ovf_un); 
  0x61,   (I_arith AI_xor);  
  0x60,   (I_arith AI_or);     
  0x65,   (I_arith AI_neg);     
  0x66,   (I_arith AI_not);     
  i_ldnull,   (I_arith AI_ldnull);   
  i_dup,   (I_arith AI_dup);   
  i_pop,   (I_arith AI_pop);
  i_ckfinite,   (I_arith AI_ckfinite);
  i_nop,   (I_arith AI_nop);
  i_break,   (I_break);
  i_arglist,   (I_arglist);
  i_endfilter,   (I_endfilter);
  i_endfinally,   I_endfinally;
  i_refanytype,   (I_refanytype);
  i_localloc,   (I_localloc);
  i_throw,   (I_throw);
  i_ldlen,   (I_ldlen);
  i_rethrow,       (I_rethrow);
];;

let is_noarg_instr i = 
  match i with 
  | I_arith (AI_ldc (DT_I4, NUM_I4 n)) when (-1) <= n && n <= 8 -> true
  | I_stloc n | I_ldloc n | I_ldarg n when n <= 3us -> true
  | I_ret
  | I_arith AI_add
  | I_arith AI_add_ovf
  | I_arith AI_add_ovf_un
  | I_arith AI_and  
  | I_arith AI_div 
  | I_arith AI_div_un
  | I_arith AI_ceq  
  | I_arith AI_cgt 
  | I_arith AI_cgt_un
  | I_arith AI_clt
  | I_arith AI_clt_un
  | I_arith (AI_conv DT_I1)  
  | I_arith (AI_conv DT_I2)  
  | I_arith (AI_conv DT_I4)  
  | I_arith (AI_conv DT_I8)  
  | I_arith (AI_conv DT_I)  
  | I_arith (AI_conv DT_R4)  
  | I_arith (AI_conv DT_R8)  
  | I_arith (AI_conv DT_U1)  
  | I_arith (AI_conv DT_U2)  
  | I_arith (AI_conv DT_U4)  
  | I_arith (AI_conv DT_U8)  
  | I_arith (AI_conv DT_U)  
  | I_arith (AI_conv DT_R)  
  | I_arith (AI_conv_ovf DT_I1)  
  | I_arith (AI_conv_ovf DT_I2)  
  | I_arith (AI_conv_ovf DT_I4)  
  | I_arith (AI_conv_ovf DT_I8)  
  | I_arith (AI_conv_ovf DT_I)  
  | I_arith (AI_conv_ovf DT_U1)  
  | I_arith (AI_conv_ovf DT_U2)  
  | I_arith (AI_conv_ovf DT_U4)  
  | I_arith (AI_conv_ovf DT_U8)  
  | I_arith (AI_conv_ovf DT_U)  
  | I_arith (AI_conv_ovf_un DT_I1)  
  | I_arith (AI_conv_ovf_un DT_I2)  
  | I_arith (AI_conv_ovf_un DT_I4)  
  | I_arith (AI_conv_ovf_un DT_I8)  
  | I_arith (AI_conv_ovf_un DT_I)  
  | I_arith (AI_conv_ovf_un DT_U1)  
  | I_arith (AI_conv_ovf_un DT_U2)  
  | I_arith (AI_conv_ovf_un DT_U4)  
  | I_arith (AI_conv_ovf_un DT_U8)  
  | I_arith (AI_conv_ovf_un DT_U)  
  | I_stelem DT_I1  
  | I_stelem DT_I2  
  | I_stelem DT_I4  
  | I_stelem DT_I8  
  | I_stelem DT_R4  
  | I_stelem DT_R8  
  | I_stelem DT_I  
  | I_stelem DT_REF  
  | I_ldelem DT_I1  
  | I_ldelem DT_I2  
  | I_ldelem DT_I4  
  | I_ldelem DT_I8  
  | I_ldelem DT_U1  
  | I_ldelem DT_U2  
  | I_ldelem DT_U4  
  | I_ldelem DT_R4  
  | I_ldelem DT_R8  
  | I_ldelem DT_I  
  | I_ldelem DT_REF  
  | I_arith AI_mul  
  | I_arith AI_mul_ovf
  | I_arith AI_mul_ovf_un
  | I_arith AI_rem  
  | I_arith AI_rem_un  
  | I_arith AI_shl  
  | I_arith AI_shr  
  | I_arith AI_shr_un
  | I_arith AI_sub  
  | I_arith AI_sub_ovf
  | I_arith AI_sub_ovf_un 
  | I_arith AI_xor  
  | I_arith AI_or     
  | I_arith AI_neg     
  | I_arith AI_not     
  | I_arith AI_ldnull   
  | I_arith AI_dup   
  | I_arith AI_pop
  | I_arith AI_ckfinite
  | I_arith AI_nop
  | I_break
  | I_arglist
  | I_endfilter
  | I_endfinally
  | I_refanytype
  | I_localloc
  | I_throw
  | I_ldlen
  | I_rethrow -> true
  | _ -> false

let brcmp_map = lazy
    (Dictionary.of_list
        [ BI_beq , i_beq
        ; BI_bgt , i_bgt
        ; BI_bgt_un , i_bgt_un
        ; BI_bge , i_bge
        ; BI_bge_un , i_bge_un
        ; BI_ble , i_ble
        ; BI_ble_un , i_ble_un
        ; BI_blt , i_blt
        ; BI_blt_un , i_blt_un
        ; BI_bne_un , i_bne_un
        ; BI_brfalse , i_brfalse
        ; BI_brtrue , i_brtrue ])

let brcmp_smap = lazy
    (Dictionary.of_list
        [ BI_beq , i_beq_s
        ; BI_bgt , i_bgt_s
        ; BI_bgt_un , i_bgt_un_s
        ; BI_bge , i_bge_s
        ; BI_bge_un , i_bge_un_s
        ; BI_ble , i_ble_s
        ; BI_ble_un , i_ble_un_s
        ; BI_blt , i_blt_s
        ; BI_blt_un , i_blt_un_s
        ; BI_bne_un , i_bne_un_s
        ; BI_brfalse , i_brfalse_s
        ; BI_brtrue , i_brtrue_s ])

(* From corhdr.h *)

let nt_VOID        = 0x1
let nt_BOOLEAN     = 0x2
let nt_I1          = 0x3
let nt_U1          = 0x4
let nt_I2          = 0x5
let nt_U2          = 0x6
let nt_I4          = 0x7
let nt_U4          = 0x8
let nt_I8          = 0x9
let nt_U8          = 0xa
let nt_R4          = 0xb
let nt_R8          = 0xc
let nt_SYSCHAR     = 0xd
let nt_VARIANT     = 0xe
let nt_CURRENCY    = 0xf
let nt_PTR         = 0x10
let nt_DECIMAL     = 0x11
let nt_DATE        = 0x12
let nt_BSTR        = 0x13
let nt_LPSTR       = 0x14
let nt_LPWSTR      = 0x15
let nt_LPTSTR      = 0x16
let nt_FIXEDSYSSTRING  = 0x17
let nt_OBJECTREF   = 0x18
let nt_IUNKNOWN    = 0x19
let nt_IDISPATCH   = 0x1a
let nt_STRUCT      = 0x1b
let nt_INTF        = 0x1c
let nt_SAFEARRAY   = 0x1d
let nt_FIXEDARRAY  = 0x1e
let nt_INT         = 0x1f
let nt_UINT        = 0x20
let nt_NESTEDSTRUCT  = 0x21
let nt_BYVALSTR    = 0x22
let nt_ANSIBSTR    = 0x23
let nt_TBSTR       = 0x24
let nt_VARIANTBOOL = 0x25
let nt_FUNC        = 0x26
let nt_ASANY       = 0x28
let nt_ARRAY       = 0x2a
let nt_LPSTRUCT    = 0x2b
let nt_CUSTOMMARSHALER = 0x2c
let nt_ERROR       = 0x2d
let nt_MAX = 0x50

(* From c:/clrenv.i386/Crt/Inc/i386/hs.h *)

let vt_EMPTY = 0
let vt_NULL = 1
let vt_I2 = 2
let vt_I4 = 3
let vt_R4 = 4
let vt_R8 = 5
let vt_CY = 6
let vt_DATE = 7
let vt_BSTR = 8
let vt_DISPATCH = 9
let vt_ERROR = 10
let vt_BOOL = 11
let vt_VARIANT = 12
let vt_UNKNOWN = 13
let vt_DECIMAL = 14
let vt_I1 = 16
let vt_UI1 = 17
let vt_UI2 = 18
let vt_UI4 = 19
let vt_I8 = 20
let vt_UI8 = 21
let vt_INT = 22
let vt_UINT = 23
let vt_VOID = 24
let vt_HRESULT  = 25
let vt_PTR = 26
let vt_SAFEARRAY = 27
let vt_CARRAY = 28
let vt_USERDEFINED = 29
let vt_LPSTR = 30
let vt_LPWSTR = 31
let vt_RECORD = 36
let vt_FILETIME = 64
let vt_BLOB = 65
let vt_STREAM = 66
let vt_STORAGE = 67
let vt_STREAMED_OBJECT = 68
let vt_STORED_OBJECT = 69
let vt_BLOB_OBJECT = 70
let vt_CF = 71
let vt_CLSID = 72
let vt_VECTOR = 0x1000
let vt_ARRAY = 0x2000
let vt_BYREF = 0x4000

 
let native_type_map = 
 lazy [ nt_CURRENCY , NativeType_currency
  ; nt_BSTR , (* COM interop *) NativeType_bstr
  ; nt_LPSTR , NativeType_lpstr
  ; nt_LPWSTR , NativeType_lpwstr
  ;  nt_LPTSTR, NativeType_lptstr
  ; nt_IUNKNOWN , (* COM interop *) NativeType_iunknown
  ; nt_IDISPATCH , (* COM interop *) NativeType_idsipatch
  ; nt_BYVALSTR , NativeType_byvalstr
  ; nt_TBSTR , NativeType_tbstr
  ; nt_LPSTRUCT , NativeType_lpstruct
  ; nt_INTF , (* COM interop *) NativeType_interface
  ; nt_STRUCT , NativeType_struct
  ; nt_ERROR , (* COM interop *) NativeType_error               
  ; nt_VOID , NativeType_void 
  ; nt_BOOLEAN , NativeType_bool
  ; nt_I1 , NativeType_int8
  ; nt_I2 , NativeType_int16
  ; nt_I4 , NativeType_int32
  ; nt_I8, NativeType_int64
  ; nt_R4 , NativeType_float32
  ; nt_R8 , NativeType_float64
  ; nt_U1 , NativeType_unsigned_int8
  ; nt_U2 , NativeType_unsigned_int16
  ; nt_U4 , NativeType_unsigned_int32
  ; nt_U8, NativeType_unsigned_int64
  ; nt_INT , NativeType_int
  ;  nt_UINT, NativeType_unsigned_int
  ;  nt_ANSIBSTR, (* COM interop *) NativeType_ansi_bstr
  ;  nt_VARIANTBOOL, (* COM interop *) NativeType_variant_bool
  ; nt_FUNC , NativeType_method
  ;  nt_ASANY, NativeType_as_any ]

let native_type_rmap = lazy (List.map (fun (x,y) -> (y,x)) (Lazy.force native_type_map))
    
let variant_type_map = 
 lazy [ VariantType_empty                 , vt_EMPTY
  ; VariantType_null                  , vt_NULL 
  ; VariantType_variant               , vt_VARIANT       
  ; VariantType_currency              , vt_CY         
  ; VariantType_decimal               , vt_DECIMAL   
  ; VariantType_date                  , vt_DATE       
  ; VariantType_bstr                  , vt_BSTR       
  ; VariantType_lpstr                 , vt_LPSTR     
  ; VariantType_lpwstr                , vt_LPWSTR    
  ; VariantType_iunknown              , vt_UNKNOWN   
  ; VariantType_idispatch             , vt_DISPATCH    
  ; VariantType_safearray             , vt_SAFEARRAY 
  ; VariantType_error                 ,  vt_ERROR         
  ; VariantType_hresult               , vt_HRESULT   
  ; VariantType_carray                , vt_CARRAY    
  ; VariantType_userdefined           , vt_USERDEFINED 
  ; VariantType_record                , vt_RECORD
  ; VariantType_filetime              , vt_FILETIME  
  ; VariantType_blob                  , vt_BLOB       
  ; VariantType_stream                , vt_STREAM     
  ; VariantType_storage               , vt_STORAGE          
  ; VariantType_streamed_object       , vt_STREAMED_OBJECT
  ; VariantType_stored_object         , vt_STORED_OBJECT 
  ; VariantType_blob_object           , vt_BLOB_OBJECT 
  ; VariantType_cf                    , vt_CF        
  ; VariantType_clsid                 , vt_CLSID                    
  ; VariantType_void                  , vt_VOID          
  ; VariantType_bool                  , vt_BOOL        
  ; VariantType_int8                  , vt_I1            
  ; VariantType_int16                 , vt_I2  
  ; VariantType_int32                 , vt_I4         
  ; VariantType_int64                 , vt_I8        
  ; VariantType_float32               , vt_R4         
  ; VariantType_float64               , vt_R8         
  ; VariantType_unsigned_int8         , vt_UI1       
  ; VariantType_unsigned_int16        , vt_UI2       
  ; VariantType_unsigned_int32        , vt_UI4       
  ; VariantType_unsigned_int64        , vt_UI8       
  ; VariantType_ptr                   , vt_PTR       
  ; VariantType_int                   , vt_INT               
  ; VariantType_unsigned_int                , vt_UINT            ]

let variant_type_rmap = lazy (List.map (fun (x,y) -> (y,x)) (Lazy.force variant_type_map))

let secaction_map =
  lazy
    [ SecAction_request , 0x0001
      ; SecAction_demand , 0x0002
      ; SecAction_assert , 0x0003
      ; SecAction_deny , 0x0004
      ; SecAction_permitonly  , 0x0005
      ; SecAction_linkcheck  , 0x0006
      ; SecAction_inheritcheck , 0x0007
      ; SecAction_reqmin , 0x0008
      ; SecAction_reqopt , 0x0009
      ; SecAction_reqrefuse , 0x000a
      ; SecAction_prejitgrant , 0x000b
      ; SecAction_prejitdeny , 0x000c
      ; SecAction_noncasdemand , 0x000d
      ; SecAction_noncaslinkdemand , 0x000e
      ; SecAction_noncasinheritance , 0x000f
      ; SecAction_linkdemandchoice , 0x0010
      ; SecAction_inheritancedemandchoice , 0x0011
      ; SecAction_demandchoice , 0x0012 ]

let secaction_rmap = lazy (List.map (fun (x,y) -> (y,x)) (Lazy.force secaction_map))

let e_CorILMethod_TinyFormat = 0x02
let e_CorILMethod_FatFormat = 0x03
let e_CorILMethod_FormatMask = 0x03
let e_CorILMethod_MoreSects = 0x08
let e_CorILMethod_InitLocals = 0x10


let e_CorILMethod_Sect_EHTable = 0x1
let e_CorILMethod_Sect_FatFormat = 0x40
let e_CorILMethod_Sect_MoreSects = 0x80

let e_COR_ILEXCEPTION_CLAUSE_EXCEPTION = 0x0
let e_COR_ILEXCEPTION_CLAUSE_FILTER = 0x1
let e_COR_ILEXCEPTION_CLAUSE_FINALLY = 0x2
let e_COR_ILEXCEPTION_CLAUSE_FAULT = 0x4

let e_IMAGE_CEE_CS_CALLCONV_FASTCALL = 0x04
let e_IMAGE_CEE_CS_CALLCONV_STDCALL = 0x02
let e_IMAGE_CEE_CS_CALLCONV_THISCALL = 0x03
let e_IMAGE_CEE_CS_CALLCONV_CDECL = 0x01
let e_IMAGE_CEE_CS_CALLCONV_VARARG = 0x05
let e_IMAGE_CEE_CS_CALLCONV_FIELD = 0x06
let e_IMAGE_CEE_CS_CALLCONV_LOCAL_SIG = 0x07
let e_IMAGE_CEE_CS_CALLCONV_PROPERTY = 0x08

let e_IMAGE_CEE_CS_CALLCONV_GENERICINST = 0x0a
let e_IMAGE_CEE_CS_CALLCONV_GENERIC = 0x10
let e_IMAGE_CEE_CS_CALLCONV_INSTANCE = 0x20
let e_IMAGE_CEE_CS_CALLCONV_INSTANCE_EXPLICIT = 0x40


