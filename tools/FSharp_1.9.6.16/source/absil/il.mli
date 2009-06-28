// (c) Microsoft Corporation. All rights reserved 

/// The "unlinked" view of .NET metadata and code.  Central to 
///  to Abstract IL library
module (* internal *) Microsoft.FSharp.Compiler.AbstractIL.IL 

open Internal.Utilities

#light

// ====================================================================
// .NET binaries can be converted to the data structures below by using 
// the functions in the "Ilread" module. 
//
// Constituent types are listed in ascending order of complexity, 
// all the way up to the type "assembly".  Types are often specified
// via a concrete representation for the type (e.g. a record), though
// later versions of this toolkit may make these types abstract.
// Types are followed by a collection of abstract functions
// of the form "dest_XYZ" and "ABC_of_XYZ" to
// access information from objects.  Sometimes these
// abstract access functions are not complete, i.e. you may have
// to use the concrete representation directly.
//
// The second part of the file (after the definition of all the types) 
// specifies a large set of utilities for building objects belonging to 
// the types.  You will only need to become familiar with these if you 
// are transforming code or writing a code-generating compiler.
// 
// Several other utilities are also defined in this file:
//   1. A code builder for turning linear sequences of instructions 
//      augmented with exception tables into the more structured 
//      format used for code.  
//
//   2. The "typ_XYZ", "tspec_XYZ" and "mspec_XYZ" values which 
//      can be used to reference types in the "mscorlib" assembly.
//
//   3. The "rescope_XYZ" functions which can be used to lift a piece of
//      metadata from one assembly and transform it to a piece of metadata
//      suitable for use from another assembly.  The transformation adjusts
//      references in the metadata to take into account the assembly
//      where the metadata will now be located.
//
//   4. The "inst_XYZ" utilities to replace type variables
//      by types.  These are associated with generics.
//
//   5. The "intern_XYZ" tables for reducing the memory used by 
//      generated constructs.
//
//   6. The "refs_of_XYZ" utilities for finding all the assemblies 
//      referenced by a module.
//
//   7. A somewhat obscure facility to allow new instructions and types
//      to be added to the IL.  This is used by ILX.
// ==================================================================== 

// A note on strings:  Strings in this module represent slightly 
// different things depending on whether you are accessing the 
// library using OCaml or a .NET language:
//
//   F# (and any other .NET language): 
//      The type 'string' in this file repesents a Unicode string.
//

// We often use the type "byte[]" where we want a type that can faithfully
// represent Unicode strings.

 
// Guids - REVIEW: adjust these to the System.Guid type
type Guid = byte[]

type ILPlatform = 
    | X86
    | AMD64
    | IA64

/// Debug info.  Values of type "source" can be attached at sequence 
/// points and some other locations. 
[<Sealed>]
type ILSourceDocument =
    static member Create : language: Guid option * vendor: Guid option * documentType: Guid option * file: string -> ILSourceDocument
    member Language: Guid option
    member Vendor: Guid option
    member DocumentType: Guid option
    member File: string


[<Sealed>]
type ILSourceMarker =
    static member Create : document: ILSourceDocument * line: int * column: int * endLine:int * endColumn: int-> ILSourceMarker
    member Document: ILSourceDocument
    member Line: int
    member Column: int
    member EndLine: int
    member EndColumn: int

/// Extensibility: ignore these unless you are generating ILX
/// structures directly.
type IlxExtensionType 
type IlxExtensionTypeKind 
type IlxExtensionInstr 


type Locale = string
type PublicKey = 
    | PublicKey of byte[]
    | PublicKeyToken of byte[]
    member IsKey: bool
    member IsKeyToken: bool
    member Key: byte[]
    member KeyToken: byte[]

type ILVersionInfo = uint16 * uint16 * uint16 * uint16

[<Sealed>]
type ILAssemblyRef =
    static member Create : name: string * hash: byte[] option * publicKey: PublicKey option * retargetable: bool * version: ILVersionInfo option * locale: Locale option -> ILAssemblyRef

    static member FromAssembly : System.Reflection.Assembly -> ILAssemblyRef

    member Name: string;
    /// The fully qualified name of the assembly reference, e.g. mscorlib, Version=1.0.3705 etc.
    member QualifiedName: string; 
    member Hash: byte[] option;
    member PublicKey: PublicKey option;
    /// CLI says this indicates if the assembly can be retargeted (at runtime) to be from a different publisher. 
    member Retargetable: bool;  
    member Version: ILVersionInfo option;
    member Locale: Locale option
    interface System.IComparable

[<Sealed>]
type ILModuleRef =
    static member Create : name: string * hasMetadata: bool * hash: byte[] option -> ILModuleRef
    member Name: string;
    member HasMetadata: bool;
    member Hash: byte[] option; 

/// Scope references
///
/// Scope references are the bits of metadata attached to type names
/// that indicate where a type can be found. CIL has three 
/// kinds: local, module and assembly references:
///   o Local: the type must reside in the same module as the scope reference
///   o Module: the type must reside in the indicated module in the same
///     assembly as the scope reference
///   o Assembly: The type must reside in the indicated assembly.
///     These have no implicit context. Assembly references can end up 
///     binding to the assembly containing the reference, i.e. 
///     may be self or mutually referential.
///
///     Assembly reference may also resolve to type in an 
///     auxiliary module of an assembly when the assembly 
///     has an "exported types" (here called "classes elsewhere") table.
///
/// We represent these references by values embedded within type
/// references.  These values are usually "shared" across the data
/// structures for a module, i.e. one such value is created for each
/// assembly or module reference, and this value is reused within each
/// type object.
///
/// Note that as with method references the term structure is not 
/// _linked_, i.e. a "ILScopeRef" is still a _reference_ to a scope, 
/// not the scope itself.  Because the structure is not linked, 
/// the Abstract IL toolset does not require 
/// strongly connected inputs: you can manipulate an assembly
/// without loading all its dependent assemblies.  This is the primary
/// difference between Abstract IL and Reflection, and it can be both
/// a blessing and a curse depending on the kind of manipulation you
/// wish to perform.
///
/// Similarly, you can manipulate individual modules within
/// an assembly without having the whole assembly loaded.  (But note that
/// most assemblies are single-module in any case).
///
/// [ILScopeRef]'s _cannot_ be compared for equality in the way that
/// might be expected, in these sense that two ILScopeRef's may 
/// resolve to the same assembly/module even though they are not equal.  
///
///   Aside: People have suggested normalizing all scope references
///          so that this would be possible, and early versions of this
///          toolkit did this.  However, this meant that in order to load
///          each module you had to tell the toolkit which assembly it belonged to.
///          Furthermore, you had to know the exact resolved details of 
///          each assembly the module refers to.  This is
///          effectively like having a "fully-linked" view of the graph
///          of assemblies, like that provided in the Ilbind module.  This is really problematic for compile-time tools,
///          as, for example, the policy for linking at the runtime-machine
///          may actually alter the results of linking.  If such compile-time
///          assumptions are to be made then the tool built on top
///          of the toolkit rather than the toolkit itself should
///          make them.
///
/// Scope references, type references, field references and method references
/// can be "bound" to particular assemblies using the functions in "Ilbind".  
/// This simulates the resolution/binding process performed by a Common Language
/// Runtime during execution.  Various tests and derived operations
/// can then be performed on the results of binding.  See the Ilbind module
/// for more details.  Many (but not all) analyses should rightly be built on top of
/// Ilbind.
type ILScopeRef = 
    // ... in M.
    | ScopeRef_local 
    // ... be in the given module of A. 
    | ScopeRef_module of ILModuleRef   
    // ... be in some module of the given assembly. 
    | ScopeRef_assembly of ILAssemblyRef  
    static member Local: ILScopeRef
    static member Module: ILModuleRef -> ILScopeRef
    static member Assembly: ILAssemblyRef -> ILScopeRef
    member IsLocalRef: bool
    member IsModuleRef: bool
    member IsAssemblyRef: bool
    member ModuleRef: ILModuleRef
    member AssemblyRef: ILAssemblyRef
    member QualifiedName: string

/// Calling conventions.  
///
/// For nearly all purposes you simply want to use CC_default combined
/// with CC_instance or CC_static, i.e.
///   ILCallingConv.Instance == Callconv(CC_instance, CC_default): for an instance method
///   ILCallingConv.Static   == Callconv(CC_static, CC_default): for a static method
///
/// CC_instance_explicit is only used by Managed C++, and indicates 
/// that the 'this' pointer is actually explicit in the signature. 
type ILArgumentConvention = 
    | CC_default
    | CC_cdecl 
    | CC_stdcall 
    | CC_thiscall 
    | CC_fastcall 
    | CC_vararg
      
type ILThisConvention =
    /// accepts an implicit 'this' pointer 
    | CC_instance           
    /// accepts an implicit 'this' pointer 
    | CC_instance_explicit  
    /// no 'this' pointer is passed
    | CC_static             

type ILCallingConv =
    | Callconv of ILThisConvention * ILArgumentConvention
    member IsInstance : bool
    member IsInstanceExplicit : bool
    member IsStatic : bool
    member ThisConv : ILThisConvention
    member BasicConv : ILArgumentConvention
    static member Instance : ILCallingConv
    static member Static   : ILCallingConv

/// Array shapes. For most purposes, including verification, the
/// rank is the only thing that matters.
 
type ILArrayBound = int32 option 
type ILArrayBounds = ILArrayBound * ILArrayBound
type ILArrayShape =
    | ILArrayShape of ILArrayBounds list (* lobound/size pairs *)
    member Rank : int
    static member SingleDimensional: ILArrayShape

/// Bounds for a single dimensional, zero based array 
val Rank1ArrayShape: ILArrayShape 

type ILBoxity = 
    | AsObject
    | AsValue


/// Type refs, i.e. references to types in some .NET assembly
[<Sealed>]
type ILTypeRef =
    /// Create a ILTypeRef
    static member Create : scope: ILScopeRef * enclosing: string list * name: string -> ILTypeRef

    /// Where is the type, i.e. is it in this module, in another module in this assembly or in another assembly? 
    member Scope: ILScopeRef
    /// The list of enclosing type names for a nested type. If non-nil then the first of these also contains the namespace.
    member Enclosing: string list
    /// The name of the type. This also contains the namespace if Enclosing is empty 
    member Name: string
    member FullName: string
    member QualifiedName: string
    interface System.IComparable
    
/// Type specs and types.  
///
/// These are the types that appear syntactically in 
/// .NET binaries.  They can be resolved to bound types (see ilbind.ml).
///
/// Generic type definitions must be combined with
/// an instantiation to form a type.  Throughout this file, 
/// a "ref" refers to something that is uninstantiated, and
/// a "spec" to a ref that is combined with the relevant instantiations.
 
[<Sealed>]
type ILTypeSpec =
    static member Create : typeRef:ILTypeRef * instantiation:ILGenericArgs -> ILTypeSpec

    /// Which type is being referred to?
    member TypeRef: ILTypeRef
    /// The type instantiation if the type is generic, otherwise empty
    member GenericArgs: ILGenericArgs
    member Scope: ILScopeRef
    member Enclosing: string list
    member Name: string
    member FullName: string

and ILType =
    /// Used only in return and pointer types.
    | Type_void                   
    /// Array types 
    | Type_array of ILArrayShape * ILType 
    /// Unboxed types, including builtin types.
    | Type_value of ILTypeSpec     
    /// Reference types.  Also may be used for parents of members even if for members in value types. 
    | Type_boxed of ILTypeSpec     
    /// Unmanaged pointers.  Nb. the type is used by tools and for binding only, not by the verifier.
    | Type_ptr of ILType             
    /// Managed pointers.
    | Type_byref of ILType           
    /// ILCode pointers. 
    | Type_fptr of ILCallingSignature        
    /// Reference a generic arg. 
    | Type_tyvar of uint16           
    /// Custom modifiers. 
    | Type_modified of            
          /// True if modifier is "required" 
          bool *                  
          /// The class of the custom modifier. 
          ILTypeRef *                   
          /// The type being modified. 
          ILType                     
    member TypeSpec : ILTypeSpec
    member Boxity : ILBoxity
    member TypeRef : ILTypeRef
    member IsNominal : bool
    member GenericArgs : ILGenericArgs
    member IsTyvar : bool

and ILCallingSignature =  
    { callsigCallconv: ILCallingConv;
      callsigArgs: ILType list;
      callsigReturn: ILType }
    member CallingConv : ILCallingConv
    member ArgTypes: ILType list
    member ReturnType: ILType

/// Generic parameters.  Actual generic parameters are  
/// always types.  Formal generic parameter declarations
/// may include the bounds, if any, on the generic parameter.
and ILGenericParameterDefs = ILGenericParameterDef list
and ILGenericArgs = ILType list
and ILGenericVariance = 
  | NonVariant            
  | CoVariant             
  | ContraVariant         
and ILGenericParameterDef =
    { gpName: string;
      gpConstraints: ILType list; 
      gpVariance: ILGenericVariance; 
      gpReferenceTypeConstraint: bool;     
      gpNotNullableValueTypeConstraint: bool;  
      gpDefaultConstructorConstraint: bool; }
    member Name : string
    /// At most one is the parent type, the others are interface types 
    member Constraints: ILType list
    /// Variance of type parameters, only applicable to generic parameters for generic interfaces and delegates 
    member Variance: ILGenericVariance
    /// The type argument must be a reference type 
    member HasReferenceTypeConstraint: bool
    /// The type argument must be a value type, but not Nullable 
    member HasNotNullableValueTypeConstraint: bool
    /// The type argument must have a public nullary constructor 
    member HasDefaultConstructorConstraint: bool

/// Accessors on types
 
val is_array_ty: ILType -> bool
val dest_array_ty: ILType -> ILArrayShape * ILType
val tspec_of_typ: ILType -> ILTypeSpec
val boxity_of_typ: ILType -> ILBoxity
val tref_of_typ: ILType -> ILTypeRef
val is_tref_typ: ILType -> bool
val inst_of_typ: ILType -> ILGenericArgs
val is_tyvar_ty: ILType -> bool

/// Formal identities of methods.  Method refs refer to methods on 
/// named types.  In general you should work with ILMethodSpec objects
/// rather than MethodRef objects, because ILMethodSpec objects carry
/// information about how generic methods are instantiated.  MethodRef
/// objects are only used at a few places in the Abstract IL syntax
/// and if analyzing or generating IL you will be unlikely to come across
/// these.

[<Sealed>]
type ILMethodRef =
     static member Create : enclosingTypeRef: ILTypeRef *
                            callingConv: ILCallingConv *
                            name: string *
                            genericArity: int *
                            argTypes: ILType list *
                            returnType: ILType 
                              -> ILMethodRef

     member EnclosingTypeRef: ILTypeRef
     member CallingConv: ILCallingConv
     member Name: string
     member GenericArity: int
     member ArgCount: int
     member ArgTypes: ILType list
     member ReturnType: ILType
     member CallingSignature: ILCallingSignature
     
     
/// Formal identities of fields.
 
type ILFieldRef = 
    { frefParent: ILTypeRef;
      frefName: string;
      frefType: ILType }
    member EnclosingTypeRef: ILTypeRef
    member Name: string
    member Type: ILType

/// Method specs and field specs
///
/// A ILMethodSpec is everything given at the callsite (apart from
/// whether the call is a tailcall and whether it is passing
/// varargs - see the instruction set below).  It is made up of 
///   1) a (possibly generic) ILMethodRef
///   2) a "usage type" that indicates the how the type 
///      containing the declaration is being used (as
///      a value class, a boxed value class, an instantiated
///      generic class or whatever - see below)
///   3) an instantiation in the case where the method is generic.
///
/// In this unbound form of the metadata, the enclosing type may 
/// be Type_boxed even when the member is a member of a value type or
/// enumeration.  This is because the binary format of the metadata
/// does not carry enough information in a MemberRefParent to determine
/// from the binary alone whether the enclosing type is a value type or
/// not.

[<Sealed>]
type ILMethodSpec =
     static member Create : ILType * ILMethodRef * ILGenericArgs -> ILMethodSpec
     member MethodRef: ILMethodRef
     member EnclosingType: ILType 
     member GenericArgs: ILGenericArgs
     member CallingConv: ILCallingConv
     member GenericArity: int
     member Name: string
     member FormalArgTypes: ILType list
     member FormalReturnType: ILType
      
val dest_mspec             : ILMethodSpec -> ILMethodRef * ILType * ILGenericArgs     

/// Field specs.  The data given for a ldfld, stfld etc. instruction.
type ILFieldSpec =
    { fspecFieldRef: ILFieldRef;
      fspecEnclosingType: ILType }    
    member FieldRef: ILFieldRef
    member EnclosingType: ILType
    member EnclosingTypeRef: ILTypeRef
    member Name: string
    member FormalType: ILType

val actual_typ_of_fspec: ILFieldSpec -> ILType

/// ILCode labels.  In structured code each code label
/// refers to a basic block somewhere in the code of the method.

type ILCodeLabel = int

type ILBasicType =
  | DT_R
  | DT_I1
  | DT_U1
  | DT_I2
  | DT_U2
  | DT_I4
  | DT_U4
  | DT_I8
  | DT_U8
  | DT_R4
  | DT_R8
  | DT_I
  | DT_U
  | DT_REF

type ILTokenSpec = 
  | Token_type of ILType 
  | Token_method of ILMethodSpec 
  | Token_field of ILFieldSpec

type ILConstSpec = 
  | NUM_I4 of int32
  | NUM_I8 of int64
  | NUM_R4 of single
  | NUM_R8 of double

type Tailcall = 
  | Tailcall
  | Normalcall

type Alignment = 
  | Aligned
  | Unaligned_1
  | Unaligned_2
  | Unaligned_4

type Volatility = 
  | Volatile
  | Nonvolatile

type ReadonlySpec = 
  | ReadonlyAddress
  | NormalAddress

type varargs = ILType list option

type ILComparisonInstr = 
  | BI_beq        
  | BI_bge        
  | BI_bge_un     
  | BI_bgt        
  | BI_bgt_un        
  | BI_ble        
  | BI_ble_un        
  | BI_blt        
  | BI_blt_un 
  | BI_bne_un 
  | BI_brfalse 
  | BI_brtrue 

type ILArithInstr = 
  | AI_add    
  | AI_add_ovf
  | AI_add_ovf_un
  | AI_and    
  | AI_div   
  | AI_div_un
  | AI_ceq      
  | AI_cgt      
  | AI_cgt_un   
  | AI_clt     
  | AI_clt_un  
  | AI_conv      of ILBasicType
  | AI_conv_ovf  of ILBasicType
  | AI_conv_ovf_un  of ILBasicType
  | AI_mul       
  | AI_mul_ovf   
  | AI_mul_ovf_un
  | AI_rem       
  | AI_rem_un       
  | AI_shl       
  | AI_shr       
  | AI_shr_un
  | AI_sub       
  | AI_sub_ovf   
  | AI_sub_ovf_un   
  | AI_xor       
  | AI_or        
  | AI_neg       
  | AI_not       
  | AI_ldnull    
  | AI_dup       
  | AI_pop
  | AI_ckfinite 
  | AI_nop
  | AI_ldc       of ILBasicType * ILConstSpec

/// The instruction set.                                                     
///
/// In general we don't categorize instructions, as different 
/// instruction groups are relevant for different types of operations. 
/// However we do collect the branch and compare instructions together 
/// because they all take an address, and the ILArithInstr ones because 
/// none of them take any direct arguments. 
type ILInstr = 
  (* Basic *)
  | I_arith of ILArithInstr
  | I_ldarg     of uint16
  | I_ldarga    of uint16
  | I_ldind     of Alignment * Volatility * ILBasicType
  | I_ldloc     of uint16
  | I_ldloca    of uint16
  | I_starg     of uint16
  | I_stind     of  Alignment * Volatility * ILBasicType
  | I_stloc     of uint16

  (* Control transfer *)
  | I_br    of  ILCodeLabel
  | I_jmp   of ILMethodSpec
  | I_brcmp of ILComparisonInstr * ILCodeLabel * ILCodeLabel (* second label is fall-through *)
  | I_switch    of (ILCodeLabel list * ILCodeLabel) (* last label is fallthrough *)
  | I_ret 

   (* Method call *)
  | I_call     of Tailcall * ILMethodSpec * varargs
  | I_callvirt of Tailcall * ILMethodSpec * varargs
  | I_callconstraint of Tailcall * ILType * ILMethodSpec * varargs
  | I_calli    of Tailcall * ILCallingSignature * varargs
  | I_ldftn    of ILMethodSpec
  | I_newobj   of ILMethodSpec  * varargs
  
  (* Exceptions *)
  | I_throw
  | I_endfinally
  | I_endfilter
  | I_leave     of  ILCodeLabel

  (* Object instructions *)
  | I_ldsfld      of Volatility * ILFieldSpec
  | I_ldfld       of Alignment * Volatility * ILFieldSpec
  | I_ldsflda     of ILFieldSpec
  | I_ldflda      of ILFieldSpec 
  | I_stsfld      of Volatility  *  ILFieldSpec
  | I_stfld       of Alignment * Volatility * ILFieldSpec
  | I_ldstr       of string
  | I_isinst      of ILType
  | I_castclass   of ILType
  | I_ldtoken     of ILTokenSpec
  | I_ldvirtftn   of ILMethodSpec

  (* Value type instructions *)
  | I_cpobj       of ILType
  | I_initobj     of ILType
  | I_ldobj       of Alignment * Volatility * ILType
  | I_stobj       of Alignment * Volatility * ILType
  | I_box         of ILType
  | I_unbox       of ILType
  | I_unbox_any   of ILType
  | I_sizeof      of ILType

  (* Generalized array instructions. In AbsIL these instructions include *)
  (* both the single-dimensional variants (with ILArrayShape == Rank1ArrayShape) *)
  (* and calls to the "special" multi-dimensional "methods" such as *)
  (*   newobj void string[,]::.ctor(int32, int32) *)
  (*   call string string[,]::Get(int32, int32) *)
  (*   call string& string[,]::Address(int32, int32) *)
  (*   call void string[,]::Set(int32, int32,string) *)
  (* The IL reader transforms calls of this form to the corresponding *)
  (* generalized instruction with the corresponding ILArrayShape *)
  (* argument. This is done to simplify the IL and make it more uniform. *)
  (* The IL writer then reverses this when emitting the binary. *)
  | I_ldelem      of ILBasicType
  | I_stelem      of ILBasicType
  | I_ldelema     of ReadonlySpec * ILArrayShape * ILType (* ILArrayShape = Rank1ArrayShape for single dimensional arrays *)
  | I_ldelem_any  of ILArrayShape * ILType (* ILArrayShape = Rank1ArrayShape for single dimensional arrays *)
  | I_stelem_any  of ILArrayShape * ILType (* ILArrayShape = Rank1ArrayShape for single dimensional arrays *)
  | I_newarr      of ILArrayShape * ILType (* ILArrayShape = Rank1ArrayShape for single dimensional arrays *)
  | I_ldlen

  (* "System.TypedReference" related instructions: almost *)
  (* no languages produce these, though they do occur in mscorlib.dll *)
  (* System.TypedReference represents a pair of a type and a byref-pointer *)
  (* to a value of that type. *)
  | I_mkrefany    of ILType
  | I_refanytype  
  | I_refanyval   of ILType
  | I_rethrow

  (* Debug-specific *)
  (* I_seqpoint is a fake instruction to represent a sequence point: *)
  (* the next instruction starts the execution of the *)
  (* statement covered by the given range - this is a *)
  (* dummy instruction and is not emitted *)
  | I_break 
  | I_seqpoint of ILSourceMarker 

  (* Varargs - C++ only *)
  | I_arglist  

  (* Local aggregates, i.e. stack allocated data (alloca) : C++ only *)
  | I_localloc
  | I_cpblk of Alignment * Volatility
  | I_initblk of Alignment  * Volatility

  (* FOR EXTENSIONS, e.g. MS-ILX *)  
  | EI_ilzero of ILType
  | EI_ldlen_multi      of int32 * int32
  | I_other    of IlxExtensionInstr

/// Basic Blocks
/// A basic block is a list of instructions ending in an unconditionally
/// branching instruction. A basic block has a label which must be unique
/// within the method it is located in.  Only the first instruction of
/// a basic block can be the target of a branch.
///
///   Details: The last instruction is always a control flow instruction,
///   i.e. branch, tailcall, throw etc.
/// 
///   For example
///       B1:  ldarg 1
///            pop
///            ret
///
///   will be one basic block:
///       ILBasicBlock("B1", [| I_ldarg(1); I_arith(AI_pop); I_ret |])

type ILBasicBlock = 
    { bblockLabel: ILCodeLabel;
      bblockInstrs: ILInstr array }
    member Label : ILCodeLabel
    member Instructions: ILInstr array

val fallthrough_of_bblock: ILBasicBlock -> ILCodeLabel option


/// These nodes indicate a particular local variable has the given source 
/// language name within a GroupBlock. Note this does not effect local 
/// variable numbering, which is global over the whole method. 
type ILDebugMapping =
    { localNum: int;
      localName: string; }
    member LocalVarIndex : int
    member Name: string

/// ILCode
/// 
/// The code for a method is made up of a "code" object.  Each "code"
/// object gives the contents of the method in a "semi-structured" form, i.e.
///   1. The structure implicit in the IL exception handling tables
///      has been made explicit
///   2. No relative offsets are used in the code: all branches and
///      switch targets are made explicit as labels.
///   3. All "fallthroughs" from one basic block to the next have
///      been made explicit, by adding extra "branch" instructions to
///      the end of basic blocks which simply fallthrough to another basic
///      block.
///
/// You can convert a straight-line sequence of instructions to structured
/// code by using build_code and 
/// Most of the interesting code is contained in BasicBlocks. If you're
/// just interested in getting started with the format then begin
/// by simply considering methods which do not contain any branch 
/// instructions, or methods which do not contain any exception handling
/// constructs.
///
/// The above format has the great advantage that you can insert and 
/// delete new code blocks without needing to fixup relative offsets
/// or exception tables.  
///
/// ILBasicBlock(bblock)
///   See above
///
/// GroupBlock(localDebugInfo, blocks)
///   A set of blocks, with interior branching between the blocks.  For example
///       B1:  ldarg 1
///            br B2
///
///       B2:  pop
///            ret
///
///   will be two basic blocks
///       let b1 = ILBasicBlock("B1", [| I_ldarg(1); I_br("B2") |])
///       let b2 = ILBasicBlock("B2", [| I_arith(AI_pop); I_ret |])
///       GroupBlock([], [b1; b2])
///
///   A GroupBlock can include a list of debug info records for locally 
///   scoped local variables.  These indicate that within the given blocks
///   the given local variables are used for the given Debug info 
///   will only be recorded for local variables
///   declared in these nodes, and the local variable will only appear live 
///   in the debugger for the instructions covered by this node. So if you 
///   omit or erase these nodes then no debug info will be emitted for local 
///   variables.  If necessary you can have one outer ScopeBlock which specifies 
///   the information for all the local variables 
///  
///   Not all the destination labels used within a group of blocks need
///   be satisfied by that group alone.  For example, the interior "try" code
///   of "try"-"catch" construct may be:
///       B1:  ldarg 1
///            br B2
///
///       B2:  pop
///            leave B3
///
///   Again there will be two basic blocks grouped together:
///       let b1 = ILBasicBlock("B1", [| I_ldarg(1); I_br("B2") |])
///       let b2 = ILBasicBlock("B2", [| I_arith(AI_pop); I_leave("B3") |])
///       GroupBlock([], [b1; b2])
///   Here the code must be embedded in a method where "B3" is a label 
///   somewhere in the method.
///
/// RestrictBlock(labels,code) 
///   This block hides labels, i.e. the given set of labels represent
///   wiring which is purely internal to the given code block, and may not
///   be used as the target of a branch by any blocks which this block
///   is placed alongside.
///
///   For example, if a method is made up of:
///       B1:  ldarg 1
///            br B2
///
///       B2:  ret
///
///   then the label "B2" is internal.  The overall code will
///   be two basic blocks grouped together, surrounded by a RestrictBlock.
///   The label "B1" is then the only remaining visible entry to the method
///   and execution will begin at that label.
///
///       let b1 = ILBasicBlock("B1", [| I_ldarg(1); I_br("B2") |])
///       let b2 = ILBasicBlock("B2", [| I_arith(AI_pop); I_leave("B3") |])
///       let gb1 = GroupBlock([], [b1; b2])
///       RestrictBlock(["B2"], gb1)
///
///   RestrictBlock is necessary to build well-formed code.  
///
/// TryBlock(trycode,seh)
///
///   A try-catch, try-finally or try-fault block.  
///   If an exception is raised while executing
///   an instruction in 'trycode' then the exception handler given by
///   'seh' is executed.
///
/// Well-formedness conditions for code:
///
///   Well-formed code includes nodes which explicitly "hide" interior labels.
///   For example, the code object for a method may have only one entry
///   label which is not hidden, and this label will be the label where 
///   execution begins.  
///
///   Both filter and catch blocks must have one 
///   and only one entry.  These entry labels are not visible 
///   outside the filter and catch blocks. Filter has no 
///   exits (it always uses endfilter), catch may have exits. 
///   The "try" block can have multiple entries, i.e. you can branch 
///   into a try from outside.  They can have multiple exits, each of 
///   which will be a "leave".
///
type ILCode = 
    | ILBasicBlock of ILBasicBlock
    | GroupBlock of ILDebugMapping list * ILCode list
    | RestrictBlock of ILCodeLabel list * ILCode
    | TryBlock of ILCode * ILExceptionBlock

///   The 'seh' specification can have several forms:
///
///     FilterCatchBlock
///       A multi-try-filter-catch block.  Execute the
///       filters in order to determine which 'catch' block to catch the
///       exception with. There are two kinds of filters - one for 
///       filtering exceptions by type and one by an instruction sequence. 
///       Note that filter blocks can't contain any exception blocks. 
///
and ILExceptionBlock = 
  | FaultBlock of ILCode 
  | FinallyBlock of ILCode
  | FilterCatchBlock of (ILFilterBlock * ILCode) list

and ILFilterBlock = 
  | TypeFilter of ILType
  | CodeFilter of ILCode

val labels_of_code: ILCode -> ILCodeLabel list
val unique_entry_of_code: ILCode -> ILCodeLabel

/// Field Init

type ILFieldInit = 
  | FieldInit_string of string
  | FieldInit_bool of bool
  | FieldInit_char of uint16
  | FieldInit_int8 of sbyte
  | FieldInit_int16 of int16
  | FieldInit_int32 of int32
  | FieldInit_int64 of int64
  | FieldInit_uint8 of byte
  | FieldInit_uint16 of uint16
  | FieldInit_uint32 of uint32
  | FieldInit_uint64 of uint64
  | FieldInit_single of single
  | FieldInit_double of double
  | FieldInit_ref

/// Native Types, for marshalling to the native C interface.
/// These are taken directly from the ILASM syntax, and don't really
/// correspond yet to the ECMA Spec (Partition II, 7.4).  

type ILNativeType = 
  | NativeType_empty
  | NativeType_custom of Guid * string * string * byte[] (* guid,nativeTypeName,custMarshallerName,cookieString *)
  | NativeType_fixed_sysstring of int32
  | NativeType_fixed_array of int32
  | NativeType_currency
  | NativeType_lpstr
  | NativeType_lpwstr
  | NativeType_lptstr
  | NativeType_byvalstr
  | NativeType_tbstr
  | NativeType_lpstruct
  | NativeType_struct
  | NativeType_void
  | NativeType_bool
  | NativeType_int8
  | NativeType_int16
  | NativeType_int32
  | NativeType_int64
  | NativeType_float32
  | NativeType_float64
  | NativeType_unsigned_int8
  | NativeType_unsigned_int16
  | NativeType_unsigned_int32
  | NativeType_unsigned_int64
  | NativeType_array of ILNativeType option * (int32 * int32 option) option (* optional idx of parameter giving size plus optional additive i.e. num elems *)
  | NativeType_int
  | NativeType_unsigned_int
  | NativeType_method
  | NativeType_as_any
  | (* COM interop *) NativeType_bstr
  | (* COM interop *) NativeType_iunknown
  | (* COM interop *) NativeType_idsipatch
  | (* COM interop *) NativeType_interface
  | (* COM interop *) NativeType_error               
  | (* COM interop *) NativeType_safe_array of ILNativeVariantType * string option 
  | (* COM interop *) NativeType_ansi_bstr
  | (* COM interop *) NativeType_variant_bool

and ILNativeVariantType = 
  | VariantType_empty
  | VariantType_null
  | VariantType_variant
  | VariantType_currency
  | VariantType_decimal               
  | VariantType_date               
  | VariantType_bstr               
  | VariantType_lpstr               
  | VariantType_lpwstr               
  | VariantType_iunknown               
  | VariantType_idispatch               
  | VariantType_safearray               
  | VariantType_error               
  | VariantType_hresult               
  | VariantType_carray               
  | VariantType_userdefined               
  | VariantType_record               
  | VariantType_filetime
  | VariantType_blob               
  | VariantType_stream               
  | VariantType_storage               
  | VariantType_streamed_object               
  | VariantType_stored_object               
  | VariantType_blob_object               
  | VariantType_cf                
  | VariantType_clsid
  | VariantType_void 
  | VariantType_bool
  | VariantType_int8
  | VariantType_int16                
  | VariantType_int32                
  | VariantType_int64                
  | VariantType_float32                
  | VariantType_float64                
  | VariantType_unsigned_int8                
  | VariantType_unsigned_int16                
  | VariantType_unsigned_int32                
  | VariantType_unsigned_int64                
  | VariantType_ptr                
  | VariantType_array of ILNativeVariantType                
  | VariantType_vector of ILNativeVariantType                
  | VariantType_byref of ILNativeVariantType                
  | VariantType_int                
  | VariantType_unsigned_int                


/// Local variables
type Local = 
    { localType: ILType;
      localPinned: bool }
    member Type: ILType
    member IsPinned: bool
      
val typ_of_local: Local -> ILType

/// IL method bodies
type ILMethodBody = 
    { ilZeroInit: bool;
      /// strictly speakin should be a uint16 
      ilMaxStack: int32; 
      ilNoInlining: bool;
      ilLocals: Local list;
      ilCode: ILCode;
      ilSource: ILSourceMarker option }

/// Member Access
type ILMemberAccess = 
    | MemAccess_assembly
    | MemAccess_compilercontrolled
    | MemAccess_famandassem
    | MemAccess_famorassem
    | MemAccess_family
    | MemAccess_private 
    | MemAccess_public 

type ILAttributeElement = 
    /// Represents a custom attribute parameter of type 'string'. These may be null, in which case they are encoded in a special
    /// way as indicated by Ecma-335 Partition II.
    | CustomElem_string of string  option 
    | CustomElem_bool of bool
    | CustomElem_char of char
    | CustomElem_int8 of sbyte
    | CustomElem_int16 of int16
    | CustomElem_int32 of int32
    | CustomElem_int64 of int64
    | CustomElem_uint8 of byte
    | CustomElem_uint16 of uint16
    | CustomElem_uint32 of uint32
    | CustomElem_uint64 of uint64
    | CustomElem_float32 of single
    | CustomElem_float64 of double
    | CustomElem_type of ILType 
    | CustomElem_tref of ILTypeRef  
    | CustomElem_array of ILAttributeElement list

/// Named args: values and flags indicating if they are fields or properties 
type ILAttributeNamedArg =  (string * ILType * bool * ILAttributeElement)

/// Custom attributes.  See 'decode_il_attrib_data' for a helper to parse the byte[] 
/// to ILAttributeElement's as best as possible.  
type ILAttribute =
    { customMethod: ILMethodSpec;  
      customData: byte[] }
    member Data: byte[]
    member Method: ILMethodSpec

type ILAttributes 

val dest_custom_attrs: ILAttributes -> ILAttribute list

/// Method parameters and return values

type ILParameter = 
    { paramName: string option;
      paramType: ILType;
      paramDefault: ILFieldInit option;  
      /// Marshalling map for parameters. COM Interop only. 
      paramMarshal: ILNativeType option; 
      paramIn: bool;
      paramOut: bool;
      paramOptional: bool;
      paramCustomAttrs: ILAttributes }
    member Name: string option
    member Type: ILType
    member Default: ILFieldInit option 
    member Marshal: ILNativeType option 
    member IsIn: bool
    member IsOut: bool
    member IsOptional: bool
    member CustomAttrs: ILAttributes;

val typs_of_params : ILParameter list -> ILType list

/// Method return values
type ILReturnValue = 
    { returnMarshal: ILNativeType option;
      returnType: ILType; 
      returnCustomAttrs: ILAttributes }
    member Type: ILType
    member Marshal: ILNativeType option 
    member CustomAttrs: ILAttributes

/// Security ILPermissions
/// 
/// Attached to various structures...

type ILSecurityAction = 
    | SecAction_request 
    | SecAction_demand
    | SecAction_assert
    | SecAction_deny
    | SecAction_permitonly
    | SecAction_linkcheck 
    | SecAction_inheritcheck
    | SecAction_reqmin
    | SecAction_reqopt
    | SecAction_reqrefuse
    | SecAction_prejitgrant
    | SecAction_prejitdeny
    | SecAction_noncasdemand
    | SecAction_noncaslinkdemand
    | SecAction_noncasinheritance
    | SecAction_linkdemandchoice
    | SecAction_inheritancedemandchoice
    | SecAction_demandchoice

type ILPermission =
    | PermissionSet of ILSecurityAction * byte[]

/// Abstract type equivalent to ILPermission list - use helpers 
/// below to construct/destruct these 
type ILPermissions 

val dest_security_decls: ILPermissions -> ILPermission list

/// PInvoke attributes.
type PInvokeCallingConvention =
    | PInvokeCallConvNone
    | PInvokeCallConvCdecl
    | PInvokeCallConvStdcall
    | PInvokeCallConvThiscall
    | PInvokeCallConvFastcall
    | PInvokeCallConvWinapi

type PInvokeCharEncoding =
    | PInvokeEncodingNone
    | PInvokeEncodingAnsi
    | PInvokeEncodingUnicode
    | PInvokeEncodingAuto

type PInvokeCharBestFit =
    | PInvokeBestFitUseAssem
    | PInvokeBestFitEnabled
    | PInvokeBestFitDisabled

type PInvokeThrowOnUnmappableChar =
    | PInvokeThrowOnUnmappableCharUseAssem
    | PInvokeThrowOnUnmappableCharEnabled
    | PInvokeThrowOnUnmappableCharDisabled

type PInvokeMethod =
    { pinvokeWhere: ILModuleRef;
      pinvokeName: string;
      pinvokeCallconv: PInvokeCallingConvention;
      PInvokeCharEncoding: PInvokeCharEncoding;
      pinvokeNoMangle: bool;
      pinvokeLastErr: bool;
      PInvokeThrowOnUnmappableChar: PInvokeThrowOnUnmappableChar;
      PInvokeCharBestFit: PInvokeCharBestFit }
    member Where: ILModuleRef
    member Name: string
    member CallingConv: PInvokeCallingConvention
    member CharEncoding: PInvokeCharEncoding
    member NoMangle: bool
    member LastError: bool
    member ThrowOnUnmappableChar: PInvokeThrowOnUnmappableChar
    member CharBestFit: PInvokeCharBestFit


/// [OverridesSpec] - refer to a method declaration in a superclass 
/// or superinterface. Used for overriding/method impls.  Includes
/// a type for the parent for the same reason that a method specs
/// includes the type of the enclosing type, i.e. the type
/// gives the "ILGenericArgs" at which the parent type is being used.

type OverridesSpec =
    | OverridesSpec of ILMethodRef * ILType
    member MethodRef: ILMethodRef
    member EnclosingType: ILType 

type ILMethodVirtualInfo =
    { virtFinal: bool; 
      virtNewslot: bool; 
      virtStrict: bool; (* mdCheckAccessOnOverride *)
      virtAbstract: bool; }
    member IsFinal: bool
    member IsNewSlot: bool
    member IsCheckAccessOnOverride: bool
    member IsAbstract: bool


type MethodKind =
    | MethodKind_static 
    | MethodKind_cctor 
    | MethodKind_ctor 
    | MethodKind_nonvirtual 
    | MethodKind_virtual of ILMethodVirtualInfo

type MethodBody =
    | MethodBody_il of ILMethodBody
    | MethodBody_pinvoke of PInvokeMethod       (* platform invoke to native  *)
    | MethodBody_abstract
    | MethodBody_native

type MethodCodeKind =
    | MethodCodeKind_il
    | MethodCodeKind_native
    | MethodCodeKind_runtime

type LazyMethodBody  

val dest_mbody : LazyMethodBody -> MethodBody 

/// Method definitions.
///
/// There are several different flavours of methods (constructors,
/// abstract, virtual, static, instance, class constructors).  There
/// is no perfect factorization of these as the combinations are not
/// independent.  

type ILMethodDef = 
    { mdName: string;
      mdKind: MethodKind;
      mdCallconv: ILCallingConv;
      mdParams: ILParameter list;
      mdReturn: ILReturnValue;
      mdAccess: ILMemberAccess;
      mdBody: LazyMethodBody;   
      mdCodeKind: MethodCodeKind;   
      mdInternalCall: bool;
      mdManaged: bool;
      mdForwardRef: bool;
      mdSecurityDecls: ILPermissions;
      /// Note: some methods are marked "HasSecurity" even if there are no permissions attached, e.g. if they use SuppressUnmanagedCodeSecurityAttribute 
      mdHasSecurity: bool; 
      mdEntrypoint:bool;
      mdReqSecObj: bool;
      mdHideBySig: bool;
      mdSpecialName: bool;
      /// The method is exported to unmanaged code using COM interop.
      mdUnmanagedExport: bool; 
      mdSynchronized: bool;
      mdPreserveSig: bool;
      /// .NET 2.0 feature: SafeHandle finalizer must be run 
      mdMustRun: bool; 
      mdExport: (int32 * string option) option; 
      mdVtableEntry: (int32 * int32) option;
     
      mdGenericParams: ILGenericParameterDefs;
      mdCustomAttrs: ILAttributes; }
      
    member Name: string;
    //mdKind: MethodKind;
    //Body: LazyMethodBody;   
    //CodeKind: MethodCodeKind;   
    member CallingConv: ILCallingConv;
    member Parameters: ILParameter list;
    member ParameterTypes: ILType list;
    member Return: ILReturnValue;
    member Access: ILMemberAccess;
    member IsInternalCall: bool;
    member IsManaged: bool;
    member IsForwardRef: bool;
    member SecurityDecls: ILPermissions;
    /// Note: some methods are marked "HasSecurity" even if there are no permissions attached, e.g. if they use SuppressUnmanagedCodeSecurityAttribute 
    member HasSecurity: bool; 
    member IsEntrypoint:bool;
    member IsReqSecObj: bool;
    member IsHideBySig: bool;
    /// The method is exported to unmanaged code using COM interop. 
    member IsUnmanagedExport: bool; 
    member IsSynchronized: bool;
    member IsPreserveSig: bool;
    /// Whidbey feature: SafeHandle finalizer must be run 
    member IsMustRun: bool; 
    //member Export: (int32 * string option) option; 
    //member VtableEntry: (int32 * int32) option;  
    member GenericParams: ILGenericParameterDefs;
    member CustomAttrs: ILAttributes;
    member IsIL : bool
    member Code : ILCode option
    member Locals : Local list
    member IsNoInline : bool
    member MaxStack : int32
    member IsZeroInit : bool
    
    /// .cctor methods.  The predicates (IsClassInitializer,IsConstructor,IsStatic,IsNonVirtualInstance,IsVirtual) form a complete, non-overlapping classification of this type
    member IsClassInitializer: bool
    /// .ctor methods.  The predicates (IsClassInitializer,IsConstructor,IsStatic,IsNonVirtualInstance,IsVirtual) form a complete, non-overlapping classification of this type
    member IsConstructor: bool
    /// static methods.  The predicates (IsClassInitializer,IsConstructor,IsStatic,IsNonVirtualInstance,IsVirtual) form a complete, non-overlapping classification of this type
    member IsStatic: bool
    /// instance methods that are not virtual.  The predicates (IsClassInitializer,IsConstructor,IsStatic,IsNonVirtualInstance,IsVirtual) form a complete, non-overlapping classification of this type
    member IsNonVirtualInstance: bool
    /// instance methods that are virtual or abstract or implement an interface slot.  The predicates (IsClassInitializer,IsConstructor,IsStatic,IsNonVirtualInstance,IsVirtual) form a complete, non-overlapping classification of this type
    member IsVirtual: bool
    
    member IsFinal: bool
    member IsNewSlot: bool
    member IsCheckAccessOnOverride : bool
    member IsAbstract: bool
  
val ilmbody_of_mdef: ILMethodDef -> ILMethodBody
val callsig_of_mdef: ILMethodDef -> ILCallingSignature

/// Tables of methods.  Logically equivalent to a list of methods but
/// the table is kept in a form optimized for looking up methods by 
/// name and arity.

/// abstract type equivalent to [ILMethodDef list] 
type ILMethodDefs 

val dest_mdefs: ILMethodDefs -> ILMethodDef list
val find_mdefs_by_name: string -> ILMethodDefs -> ILMethodDef list
val find_mdefs_by_arity: string * int -> ILMethodDefs -> ILMethodDef list

/// Field definitions
type ILFieldDef = 
    { fdName: string;
      fdType: ILType;
      fdStatic: bool;
      fdAccess: ILMemberAccess;
      fdData:  byte[] option;
      fdInit: ILFieldInit option;  
      fdOffset:  int32 option; 
      fdSpecialName: bool;
      fdMarshal: ILNativeType option; 
      fdNotSerialized: bool;
      fdLiteral: bool ;
      fdInitOnly: bool;
      fdCustomAttrs: ILAttributes; }
    member Name: string;
    member Type: ILType;
    member IsStatic: bool;
    member Access: ILMemberAccess;
    member Data:  byte[] option;
    member LiteralValue: ILFieldInit option;  
    /// The explicit offset in byte[] when explicit layout is used.
    member Offset:  int32 option; 
    member Marshal: ILNativeType option; 
    member NotSerialized: bool;
    member IsLiteral: bool ;
    member IsInitOnly: bool;
    member CustomAttrs: ILAttributes;

val typ_of_fdef : ILFieldDef -> ILType
val name_of_fdef: ILFieldDef -> string

/// Tables of fields.  Logically equivalent to a list of fields but
/// the table is kept in a form optimized for looking up fields by 
/// name.
type ILFieldDefs 

val dest_fdefs: ILFieldDefs -> ILFieldDef list
val filter_fdefs: (ILFieldDef -> bool) -> ILFieldDefs -> ILFieldDefs
val find_fdefs: string -> ILFieldDefs -> ILFieldDef list

/// Event definitions
type ILEventDef =
    { eventType: ILType option; 
      eventName: string;
      eventRTSpecialName: bool;
      eventSpecialName: bool;
      eventAddOn: ILMethodRef; 
      eventRemoveOn: ILMethodRef;
      eventFire: ILMethodRef option;
      eventOther: ILMethodRef list;
      eventCustomAttrs: ILAttributes; }
    member Type: ILType option; 
    member Name: string;
    member AddMethod: ILMethodRef; 
    member RemoveMethod: ILMethodRef;
    member FireMethod: ILMethodRef option;
    member OtherMethods: ILMethodRef list;
    member CustomAttrs: ILAttributes; 

/// Table of those events in a type definition.
type ILEventDefs 

val dest_edefs: ILEventDefs -> ILEventDef list
val filter_edefs: (ILEventDef -> bool) -> ILEventDefs -> ILEventDefs
val find_edefs: string -> ILEventDefs -> ILEventDef list

/// Property definitions
type ILPropertyDef =
    { propName: string;
      propRTSpecialName: bool;
      propSpecialName: bool;
      propSet: ILMethodRef option;
      propGet: ILMethodRef option;
      propCallconv: ILThisConvention;
      propType: ILType;          
      propInit: ILFieldInit option;
      propArgs: ILType list;
      propCustomAttrs: ILAttributes; }
    member Name: string;
    member SetMethod: ILMethodRef option;
    member GetMethod: ILMethodRef option;
    member CallingConv: ILThisConvention;
    member Type: ILType;          
    member Init: ILFieldInit option;
    member Args: ILType list;
    member CustomAttrs: ILAttributes; 

/// Table of those properties in a type definition.
type PropertyDefs 

val dest_pdefs: PropertyDefs -> ILPropertyDef  list
val filter_pdefs: (ILPropertyDef -> bool) -> PropertyDefs -> PropertyDefs
val find_pdefs: string -> PropertyDefs -> ILPropertyDef list

/// Method Impls
///
/// If there is an entry (pms --&gt; ms) in this table, then method [ms] 
/// is used to implement method [pms] for the purposes of this class 
/// and its subclasses. 
type ILMethodImplDef =
    { mimplOverrides: OverridesSpec;
      mimplOverrideBy: ILMethodSpec }

type ILMethodImplDefs 

val dest_mimpls: ILMethodImplDefs -> ILMethodImplDef list

/// Type Layout information
type ILTypeDefLayout =
  | TypeLayout_auto
  | TypeLayout_sequential of ILTypeDefLayoutInfo
  | TypeLayout_explicit of ILTypeDefLayoutInfo 

and ILTypeDefLayoutInfo =
    { typeSize: int32 option;
      typePack: uint16 option } 
    member Size: int32 option
    member Pack: uint16 option 

/// Type init semantics
type ILTypeDefInitSemantics =
    | TypeInit_beforefield
    | TypeInit_beforeany

/// Default Unicode encoding for P/Invoke  within a type
type ILDefaultPInvokeEncoding =
    | TypeEncoding_ansi
    | TypeEncoding_autochar
    | TypeEncoding_unicode

/// Type Access
type ILTypeDefAccess =
    | TypeAccess_public 
    | TypeAccess_private
    | TypeAccess_nested of ILMemberAccess 

/// A categorization of type definitions into "kinds"

(*-------------------------------------------------------------------
 *
 * A note for the nit-picky.... In theory, the "kind" of a type 
 * definition can only be  partially determined prior to binding.  
 * For example, you cannot really, absolutely tell if a type is 
 * really, absolutely a value type until you bind the 
 * super class and test it for type equality against System.ValueType.  
 * However, this is unbearably annoying, as it means you 
 * have to load "mscorlib" and perform bind operations 
 * in order to be able to determine some quite simple 
 * things.  So we approximate by simply looking at the name
 * of the superclass when loading.
 * ------------------------------------------------------------------ *)

type ILTypeDefKind =
  | TypeDef_class
  | TypeDef_valuetype
  | TypeDef_interface
  | TypeDef_enum 
  | TypeDef_delegate 
  (* FOR EXTENSIONS, e.g. MS-ILX *)  
  | TypeDef_other of IlxExtensionTypeKind

(* ------------------------------------------------------------------ 
 * Type Names
 *
 * The name of a type stored in the tdName field is as follows:
 *   - For outer types it is, for example, System.String, i.e.
 *     the namespace followed by the type name.
 *   - For nested types, it is simply the type name.  The namespace
 *     must be gleaned from the context in which the nested type
 *     lies.
 * ------------------------------------------------------------------ *)

type NamespaceAndTypename = string list * string

val split_namespace: string -> string list

val split_namespace_array: string -> string[]

/// The [split_type_name] utility helps you split a string representing
/// a type name into the leading namespace elements (if any), the
/// names of any nested types and the type name itself.  This function
/// memoizes and interns the splitting of the namespace portion of
/// the type name. 
val split_type_name: string -> NamespaceAndTypename

val split_type_name_array: string -> string[] * string


/// Type Definitions 
///
/// As for methods there are several important constraints not encoded 
/// in the type definition below, for example that the super class of
/// an interface type is always None, or that enumerations always
/// have a very specific form.
type ILTypeDef =  
    { tdKind: ILTypeDefKind;
      tdName: string;  
      tdGenericParams: ILGenericParameterDefs;  
      tdAccess: ILTypeDefAccess;  
      tdAbstract: bool;
      tdSealed: bool; 
      tdSerializable: bool; 
      tdComInterop: bool; (* Class or interface generated for COM interop *) 
      tdLayout: ILTypeDefLayout;
      tdSpecialName: bool;
      tdEncoding: ILDefaultPInvokeEncoding;
      tdNested: ILTypeDefs;
      tdImplements: ILType list;  
      tdExtends: ILType option; 
      tdMethodDefs: ILMethodDefs;
      tdSecurityDecls: ILPermissions;
      tdHasSecurity: bool; (* Note: some classes are marked "HasSecurity" even if there are no permissions attached, e.g. if they use SuppressUnmanagedCodeSecurityAttribute *)
      tdFieldDefs: ILFieldDefs;
      tdMethodImpls: ILMethodImplDefs;
      tdInitSemantics: ILTypeDefInitSemantics;
      tdEvents: ILEventDefs;
      tdProperties: PropertyDefs;
      tdCustomAttrs: ILAttributes; }
    member IsClass: bool;
    member IsValueType: bool;
    member IsInterface: bool;
    member IsEnum: bool;
    member IsDelegate: bool;
    member Name: string;  
    member GenericParams: ILGenericParameterDefs;  
    member Access: ILTypeDefAccess;  
    member IsAbstract: bool;
    member IsSealed: bool; 
    member IsSerializable: bool; 
    /// Class or interface generated for COM interop 
    member IsComInterop: bool; 
    member Layout: ILTypeDefLayout;
    member IsSpecialName: bool;
    member Encoding: ILDefaultPInvokeEncoding;
    member NestedTypes: ILTypeDefs;
    member Implements: ILType list;  
    member Extends: ILType option; 
    member SecurityDecls: ILPermissions;
    /// Note: some classes are marked "HasSecurity" even if there are no permissions attached, e.g. if they use SuppressUnmanagedCodeSecurityAttribute 
    member HasSecurity: bool; 
    member Fields: ILFieldDefs;
    member Methods: ILMethodDefs;
    member MethodImpls: ILMethodImplDefs;
    member Events: ILEventDefs;
    member Properties: PropertyDefs;
    member InitSemantics: ILTypeDefInitSemantics;
    member CustomAttrs: ILAttributes;

/// Tables of named type definitions.  The types and table may contain on-demand
/// (lazy) computations, e.g. the actual reading of some aspects
/// of a type definition may be delayed if the reader being used supports
/// this.
///
/// This is an abstract type equivalent to "ILTypeDef list" 
and ILTypeDefs 

val is_value_or_enum_tdef: ILTypeDef -> bool

/// Find the method definition corresponding to the given property or 
/// event operation. These are always in the same class as the property 
/// or event. This is useful especially if your code is not using the Ilbind 
/// API to bind references. 
val resolve_mref: ILTypeDef -> ILMethodRef -> ILMethodDef
val iter_tdefs: (ILTypeDef -> unit) -> ILTypeDefs -> unit
val dest_tdefs: ILTypeDefs -> ILTypeDef  list

/// Calls to [find_tdef] will result in any laziness in the overall 
/// set of ILTypeDefs being read in in addition 
/// to the details for the type found, but the remaining individual 
/// type definitions will not be read. 
val find_tdef: string -> ILTypeDefs -> ILTypeDef

val dest_lazy_tdefs: ILTypeDefs -> (string list * string * ILAttributes * ILTypeDef Lazy.t) list

val tname_for_toplevel: string
val is_toplevel_tname: string -> bool

val ungenericize_tname: string -> string (* e.g. List`1 --> List *)

/// "Classes Elsewhere" - classes in auxiliary modules.
///
/// Manifests include declarations for all the classes in an 
/// assembly, regardless of which module they are in.
///
/// The ".class extern" construct describes so-called exported types -- 
/// these are public classes defined in the auxiliary modules of this assembly,
/// i.e. modules other than the manifest-carrying module. 
/// 
/// For example, if you have a two-module 
/// assembly (A.DLL and B.DLL), and the manifest resides in the A.DLL, 
/// then in the manifest all the public classes declared in B.DLL should
/// be defined as exported types, i.e., as ".class extern". The public classes 
/// defined in A.DLL should not be defined as ".class extern" -- they are 
/// already available in the manifest-carrying module. The union of all 
/// public classes defined in the manifest-carrying module and all 
/// exported types defined there is the set of all classes exposed by 
/// this assembly. Thus, by analysing the metadata of the manifest-carrying 
/// module of an assembly, you can identify all the classes exposed by 
/// this assembly, and where to find them.
///
/// Nested classes found in external modules should also be located in 
/// this table, suitably nested inside another "ILExportedType"
/// definition.

/// these are only found in the "Nested" field of ILExportedType objects 
type ILNestedExportedType =
    { nestedExportedTypeName: string;
      nestedExportedTypeAccess: ILMemberAccess;
      nestedExportedTypeNested: ILNestedExportedTypes;
      nestedExportedTypeCustomAttrs: ILAttributes } 

and ILNestedExportedTypes 

/// these are only found in the ILExportedTypes table in the manifest 
type ILExportedType =
    { exportedTypeScope: ILScopeRef;
      /// [Namespace.]Name
      exportedTypeName: string;
      exportedTypeForwarder: bool;
      exportedTypeAccess: ILTypeDefAccess;
      exportedTypeNested: ILNestedExportedTypes;
      exportedTypeCustomAttrs: ILAttributes } 
    member ScopeRef: ILScopeRef
    member IsForwarder: bool
    member Name: string
    member Access: ILTypeDefAccess
    member Nested: ILNestedExportedTypes
    member CustomAttrs: ILAttributes 

type ILExportedTypes 

val dest_nested_exported_types: ILNestedExportedTypes -> ILNestedExportedType  list
val dest_exported_types: ILExportedTypes -> ILExportedType list
val find_exported_type: string -> ILExportedTypes -> ILExportedType

type ILResourceAccess = 
    | Resource_public 
    | Resource_private 

type ILResourceLocation = 
    | Resource_local of (unit -> byte[])  (* resources may be re-read each time this function is called *)
    | Resource_file of ILModuleRef * int32
    | Resource_assembly of ILAssemblyRef

/// "Manifest ILResources" are chunks of resource data, being one of:
///   - the data section of the current module (byte[] of resource given directly) 
///  - in an external file in this assembly (offset given in the ILResourceLocation field) 
///   - as a resources in another assembly of the same name.  
type ILResource =
    { resourceName: string;
      resourceWhere: ILResourceLocation;
      resourceAccess: ILResourceAccess;
      resourceCustomAttrs: ILAttributes }
    member Name: string
    member Location: ILResourceLocation
    member Access: ILResourceAccess
    member CustomAttrs: ILAttributes 

/// Table of resources in a module
type ILResources 

val dest_resources: ILResources -> ILResource  list


type ILAssemblyLongevity =
  | LongevityUnspecified
  | LongevityLibrary
  | LongevityPlatformAppDomain
  | LongevityPlatformProcess
  | LongevityPlatformSystem

/// The main module of an assembly is a module plus some manifest information.
type ILAssemblyManifest = 
    { manifestName: string;
      manifestAuxModuleHashAlgorithm: int32; 
      manifestSecurityDecls: ILPermissions;
      manifestPublicKey: byte[] option;  
      manifestVersion: ILVersionInfo option;
      manifestLocale: Locale option;
      manifestCustomAttrs: ILAttributes;
      manifestLongevity: ILAssemblyLongevity; 
      manifestDisableJitOptimizations: bool;
      manifestJitTracking: bool;
      manifestRetargetable: bool;
      manifestExportedTypes: ILExportedTypes;
      manifestEntrypointElsewhere: ILModuleRef option;
    } 
    member Name: string;

    /// This is ID of the algorithm used for the hashes of auxiliary 
    /// files in the assembly.   These hashes are stored in the 
    /// ILModuleRef.Hash fields of this assembly. These are not cryptographic 
    /// hashes: they are simple file hashes. The algorithm is normally 
    /// 0x00008004 indicating the SHA1 hash algorithm.  
    member AuxModuleHashAlgorithm: int32; 

    member SecurityDecls: ILPermissions;

    /// This is the public key used to sign this 
    /// assembly (the signature itself is stored elsewhere: see the 
    /// binary format, and may not have been written if delay signing 
    /// is used).  (member Name, member PublicKey) forms the full 
    /// public name of the assembly.  
    member PublicKey: byte[] option;  

    member Version: ILVersionInfo option;
    member Locale: Locale option;
    member CustomAttrs: ILAttributes;
    member AssemblyLongevity: ILAssemblyLongevity; 
    member DisableJitOptimizations: bool;
    member JitTracking: bool;
    member Retargetable: bool;

    /// Records the types impemented by this asssembly in auxiliary 
    /// modules. 
    member ExportedTypes: ILExportedTypes;

    /// Records whether the entrypoint resides in another module. 
    member EntrypointElsewhere: ILModuleRef option;
    
/// One module in the "current" assembly, either a main-module or
/// an auxiliary module.  The main module will have a manifest.
///
/// An assembly is built by joining together a "main" module plus 
/// several auxiliary modules. 
type ILModuleDef = 
    { modulManifest: ILAssemblyManifest option;
      modulCustomAttrs: ILAttributes;
      modulName: string;
      modulTypeDefs: ILTypeDefs;
      modulSubSystem: int32;
      modulDLL: bool;
      modulILonly: bool;
      modulPlatform: ILPlatform option;
      modul32bit: bool;
      modul64bit: bool;
      modulVirtAlignment: int32;
      modulPhysAlignment: int32;
      modulImageBase: int32;
      modulResources: ILResources; 
      modulNativeResources: byte[] Lazy.t list; (* e.g. win86 resources, as the exact contents of a .res or .obj file *)
(*     modulFixups: fixups; *) }
    member Manifest: ILAssemblyManifest option;
    member ManifestOfAssembly: ILAssemblyManifest 
    member CustomAttrs: ILAttributes;
    member Name: string;
    member TypeDefs: ILTypeDefs;
    member SubSystemFlags: int32;
    member IsDLL: bool;
    member IsILOnly: bool;
    member Platform: ILPlatform option;
    member Is32Bit: bool;
    member Is64Bit: bool;
    member VirtualAlignment: int32;
    member PhysicalAlignment: int32;
    member ImageBase: int32;
    member Resources: ILResources; 
    member NativeResources: byte[] Lazy.t list

val module_is_mainmod: ILModuleDef -> bool
val assname_of_mainmod: ILModuleDef -> string

// ====================================================================
// PART 2
// 
// Making metadata.  Where no explicit constructor
// is given, you should create the concrete datatype directly, 
// e.g. by filling in all appropriate record fields.
// ==================================================================== *)

/// A table of common references to items in mscorlib. Version-neutral references 
/// can be generated using ecmaILGlobals.  If you have already loaded a particular 
/// version of mscorlib you should reference items via an ILGlobals for that particular 
/// version of mscorlib built using mk_ILGlobals. 
[<StructuralEquality(false); StructuralComparison(false)>]
type ILGlobals = 
    { mscorlib_scoref: ILScopeRef;
      mscorlibAssemblyName: string;
      tref_Object: ILTypeRef
      ; tspec_Object: ILTypeSpec
      ; typ_Object: ILType
      ; tref_String: ILTypeRef
      ; typ_String: ILType
      ; typ_StringBuilder: ILType
      ; typ_AsyncCallback: ILType
      ; typ_IAsyncResult: ILType
      ; typ_IComparable: ILType
      ; tref_Type: ILTypeRef
      ; typ_Type: ILType
      ; tref_Missing: ILTypeRef
      ; typ_Missing: ILType
      ; typ_Activator: ILType
      ; typ_Delegate: ILType
      ; typ_ValueType: ILType
      ; typ_Enum: ILType
      ; tspec_TypedReference: ILTypeSpec
      ; typ_TypedReference: ILType
      ; typ_MulticastDelegate: ILType
      ; typ_Array: ILType
      ; tspec_Int64: ILTypeSpec
      ; tspec_UInt64: ILTypeSpec
      ; tspec_Int32: ILTypeSpec
      ; tspec_UInt32: ILTypeSpec
      ; tspec_Int16: ILTypeSpec
      ; tspec_UInt16: ILTypeSpec
      ; tspec_SByte: ILTypeSpec
      ; tspec_Byte: ILTypeSpec
      ; tspec_Single: ILTypeSpec
      ; tspec_Double: ILTypeSpec
      ; tspec_IntPtr: ILTypeSpec
      ; tspec_UIntPtr: ILTypeSpec
      ; tspec_Char: ILTypeSpec
      ; tspec_Bool: ILTypeSpec
      ; typ_int8: ILType
      ; typ_int16: ILType
      ; typ_int32: ILType
      ; typ_int64: ILType
      ; typ_uint8: ILType
      ; typ_uint16: ILType
      ; typ_uint32: ILType
      ; typ_uint64: ILType
      ; typ_float32: ILType
      ; typ_float64: ILType
      ; typ_bool: ILType
      ; typ_char: ILType
      ; typ_IntPtr: ILType
      ; typ_UIntPtr: ILType
      ; typ_RuntimeArgumentHandle: ILType
      ; typ_RuntimeTypeHandle: ILType
      ; typ_RuntimeMethodHandle: ILType
      ; typ_RuntimeFieldHandle: ILType
      ; typ_Byte: ILType
      ; typ_Int16: ILType
      ; typ_Int32: ILType
      ; typ_Int64: ILType
      ; typ_SByte: ILType
      ; typ_UInt16: ILType
      ; typ_UInt32: ILType
      ; typ_UInt64: ILType
      ; typ_Single: ILType
      ; typ_Double: ILType
      ; typ_Bool: ILType
      ; typ_Char: ILType
      ; typ_SerializationInfo: ILType
      ; typ_StreamingContext: ILType
      ; tref_SecurityPermissionAttribute : ILTypeRef
      ; tspec_Exception: ILTypeSpec
      ; typ_Exception: ILType }

/// Build the table of commonly used references given a ILScopeRef for mscorlib. 
val mk_ILGlobals : ILScopeRef -> string option -> ILGlobals


/// When writing a binary the fake "toplevel" type definition (called <Module>)
/// must come first. [dest_tdefs_with_toplevel_first] puts it first, and 
/// creates it in the returned list as an empty typedef if it 
/// doesn't already exist.
val dest_tdefs_with_toplevel_first: ILGlobals -> ILTypeDefs -> ILTypeDef list

/// Note: not all custom attribute data can be decoded without binding types.  In particular 
/// enums must be bound in order to discover the size of the underlying integer. 
/// The following assumes enums have size int32. 
/// It also does not completely decode System.Type attributes 
val decode_il_attrib_data: 
    ILGlobals -> 
    ILAttribute -> 
      ILAttributeElement list *  (* fixed args *)
      ILAttributeNamedArg list (* named args: values and flags indicating if they are fields or properties *) 

/// Generate simple references to assemblies and modules
val mk_simple_assref: string -> ILAssemblyRef
val mk_simple_modref: string -> ILModuleRef
val scoref_for_modname: string -> ILScopeRef

val mk_empty_gactuals: ILGenericArgs
val mk_tyvar_ty: uint16 -> ILType

/// Make type refs
val mk_nested_tref: ILScopeRef * string list * string -> ILTypeRef
val mk_tref: ILScopeRef * string -> ILTypeRef
val mk_tref_in_tref: ILTypeRef * string -> ILTypeRef

/// Make type specs
val mk_nongeneric_tspec: ILTypeRef -> ILTypeSpec
val mk_tspec: ILTypeRef * ILGenericArgs -> ILTypeSpec

/// Make types
val mk_typ: ILBoxity -> ILTypeSpec -> ILType
val mk_named_typ: ILBoxity -> ILTypeRef -> ILGenericArgs -> ILType
val mk_boxed_typ: ILTypeRef -> ILGenericArgs -> ILType
val mk_value_typ: ILTypeRef -> ILGenericArgs -> ILType
val mk_nongeneric_boxed_typ: ILTypeRef -> ILType
val mk_nongeneric_value_typ: ILTypeRef -> ILType
val mk_array_ty: ILType * ILArrayShape -> ILType
val mk_sdarray_ty: ILType -> ILType

/// Make method references and specs
val mk_mref: ILTypeRef * ILCallingConv * string * int * ILType list * ILType -> ILMethodRef
val mk_mspec: ILMethodRef * ILBoxity * ILGenericArgs * ILGenericArgs -> ILMethodSpec
val mk_mref_mspec_in_typ: ILMethodRef * ILType * ILGenericArgs -> ILMethodSpec
val mk_mspec_in_typ: ILType * ILCallingConv * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec

/// Construct references to methods on a given type 
val mk_nongeneric_mspec_in_typ: ILType * ILCallingConv * string * ILType list * ILType -> ILMethodSpec

/// Construct references to methods given a ILTypeSpec
val mk_mspec_in_tspec: ILTypeSpec * ILBoxity * ILCallingConv * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec
val mk_nongeneric_mspec_in_tspec: ILTypeSpec * ILBoxity * ILCallingConv * string * ILType list * ILType -> ILMethodSpec

/// Construct references to instance methods 
val mk_instance_mspec_in_tref: ILTypeRef * ILBoxity * string * ILType list * ILType * ILGenericArgs * ILGenericArgs -> ILMethodSpec
val mk_instance_mspec_in_tspec: ILTypeSpec * ILBoxity * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec
val mk_instance_mspec_in_typ: ILType * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec
val mk_instance_mspec_in_boxed_tspec: ILTypeSpec * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec
val mk_instance_mspec_in_nongeneric_boxed_tref: ILTypeRef * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec

/// Construct references to non-generic methods 
val mk_nongeneric_mspec_in_tref: ILTypeRef * ILBoxity * ILCallingConv * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec
val mk_nongeneric_mspec_in_nongeneric_tref: ILTypeRef * ILBoxity * ILCallingConv * string * ILType list * ILType -> ILMethodSpec

/// Construct references to non-generic instance methods 
val mk_nongeneric_instance_mspec_in_tref: ILTypeRef * ILBoxity * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec
val mk_nongeneric_instance_mspec_in_tspec: ILTypeSpec * ILBoxity * string * ILType list * ILType -> ILMethodSpec
val mk_nongeneric_instance_mspec_in_typ: ILType * string * ILType list * ILType -> ILMethodSpec
val mk_nongeneric_instance_mspec_in_boxed_tspec: ILTypeSpec * string * ILType list * ILType -> ILMethodSpec
val mk_nongeneric_instance_mspec_in_nongeneric_boxed_tref: ILTypeRef * string * ILType list * ILType -> ILMethodSpec

/// Construct references to static methods 
val mk_static_mspec_in_nongeneric_boxed_tref: ILTypeRef * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec
val mk_static_mspec_in_boxed_tspec: ILTypeSpec * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec
val mk_static_mspec_in_typ: ILType * string * ILType list * ILType * ILGenericArgs -> ILMethodSpec

/// Construct references to static, non-generic methods 
val mk_static_nongeneric_mspec_in_nongeneric_boxed_tref: ILTypeRef * string * ILType list * ILType -> ILMethodSpec
val mk_static_nongeneric_mspec_in_boxed_tspec: ILTypeSpec * string * ILType list * ILType -> ILMethodSpec
val mk_static_nongeneric_mspec_in_typ: ILType * string * ILType list * ILType -> ILMethodSpec

/// Construct references to toplevel methods in modules.  Usually compiler generated. 
val mk_toplevel_static_mspec: ILScopeRef -> string * ILType list * ILType * ILGenericArgs -> ILMethodSpec
val mk_toplevel_static_nongeneric_mspec: ILScopeRef -> string * ILType list * ILType -> ILMethodSpec

/// Construct references to constructors 
val mk_ctor_mspec: ILTypeRef * ILBoxity * ILType list * ILGenericArgs -> ILMethodSpec
val mk_nongeneric_ctor_mspec: ILTypeRef * ILBoxity * ILType list -> ILMethodSpec
val mk_ctor_mspec_for_boxed_tspec: ILTypeSpec * ILType list -> ILMethodSpec
val mk_ctor_mspec_for_typ: ILType * ILType list -> ILMethodSpec
val mk_ctor_mspec_for_nongeneric_boxed_tref: ILTypeRef * ILType list -> ILMethodSpec

/// Construct references to fields 
val mk_fref_in_tref: ILTypeRef * string * ILType -> ILFieldRef
val mk_fspec: ILFieldRef * ILType -> ILFieldSpec
val mk_fspec_in_typ: ILType * string * ILType -> ILFieldSpec
val mk_fspec_in_tspec: ILTypeSpec * ILBoxity * string * ILType -> ILFieldSpec
val mk_fspec_in_boxed_tspec: ILTypeSpec * string * ILType -> ILFieldSpec
val mk_fspec_in_nongeneric_boxed_tref: ILTypeRef * string * ILType -> ILFieldSpec

val mk_callsig: ILCallingConv * ILType list * ILType -> ILCallingSignature

/// Make generalized verions of possibly-generic types,
/// e.g. Given the ILTypeDef for List, return the type "List<T>".

val generalize_tref: ILTypeRef -> ILGenericParameterDef list -> ILTypeSpec

val gparam_of_gactual: ILType -> ILGenericParameterDef
val gparams_of_inst: ILGenericArgs -> ILGenericParameterDefs
val generalize_gparams: ILGenericParameterDefs -> ILGenericArgs

/// Make custom attributes 
val mk_custom_attribute_mref: 
    ILGlobals 
    -> ILMethodSpec 
       * ILAttributeElement list (* fixed args: values and implicit types *) 
       * ILAttributeNamedArg list (* named args: values and flags indicating if they are fields or properties *) 
      -> ILAttribute

val mk_custom_attribute: 
    ILGlobals 
    -> ILTypeRef * ILType list * 
       ILAttributeElement list (* fixed args: values and implicit types *) * 
       ILAttributeNamedArg list (* named args: values and flags indicating if they are fields or properties *) 
         -> ILAttribute

val mk_permission_set : ILGlobals -> ILSecurityAction * (ILTypeRef * (string * ILType * ILAttributeElement) list) list -> ILPermission

/// Making code.
val check_code:  ILCode -> ILCode
val generate_code_label: unit -> ILCodeLabel
val string_of_code_label : ILCodeLabel -> string

/// Make some code that is a straight line sequence of instructions. 
/// The function will add a "return" if the last instruction is not an exiting instruction 
val nonbranching_instrs_to_code: ILInstr list -> ILCode 

/// Make some code that is a straight line sequence of instructions, then do 
/// some control flow.  The first code label is the entry label of the generated code. 
val nonbranching_instrs_then: ILCodeLabel -> ILInstr list -> ILInstr -> ILCode 
val nonbranching_instrs_then_br: ILCodeLabel -> ILInstr list -> ILCodeLabel -> ILCode

/// Make a basic block. The final instruction must be control flow 
val nonbranching_instrs: ILCodeLabel -> ILInstr list -> ILCode

/// Some more primitive helpers 
val mk_bblock: ILBasicBlock -> ILCode
val mk_group_block: ILCodeLabel list * ILCode list -> ILCode

/// Helpers for codegen: scopes for allocating new temporary variables.
type tmps 
val alloc_tmp: tmps -> Local -> uint16
val new_tmps : int -> tmps
val get_tmps : tmps -> Local list

/// Derived functions for making some common patterns of instructions
val mk_normal_call: ILMethodSpec -> ILInstr
val mk_normal_callvirt: ILMethodSpec -> ILInstr
val mk_normal_callconstraint: ILType * ILMethodSpec -> ILInstr
val mk_normal_newobj: ILMethodSpec -> ILInstr
val mk_nongeneric_call_superclass_constructor: ILType list * ILTypeRef -> ILInstr list
val mk_call_superclass_constructor : ILType list * ILTypeSpec -> ILInstr list
val mk_normal_stfld: ILFieldSpec -> ILInstr
val mk_normal_stsfld: ILFieldSpec -> ILInstr
val mk_normal_ldsfld: ILFieldSpec -> ILInstr
val mk_normal_ldfld: ILFieldSpec -> ILInstr
val mk_normal_ldflda: ILFieldSpec -> ILInstr
val mk_normal_stind: ILBasicType -> ILInstr
val mk_normal_ldind: ILBasicType -> ILInstr
val mk_normal_cpind: ILBasicType -> ILInstr list
val mk_normal_ldobj: ILType -> ILInstr
val mk_normal_stobj: ILType -> ILInstr 
val mk_ldc_i32: int32 -> ILInstr
val ldarg_0: ILInstr

val and_tailness: Tailcall -> bool -> Tailcall

/// Derived functions for making return, parameter and local variable
/// objects for use in method definitions.
val mk_param: string option * ILType -> ILParameter
val mk_unnamed_param: ILType -> ILParameter
val mk_named_param: string * ILType -> ILParameter
val mk_return: ILType -> ILReturnValue
val mk_local: ILType -> Local

/// Make a formal generic parameters
val mk_empty_gparams: ILGenericParameterDefs

/// Make method definitions
val mk_ilmbody: initlocals:bool * Local list * int * ILCode * ILSourceMarker option -> ILMethodBody
val mk_impl: bool * Local list * int * ILCode * ILSourceMarker option -> MethodBody

val mk_ctor: ILMemberAccess * ILParameter list * MethodBody -> ILMethodDef
val mk_nongeneric_nothing_ctor: ILSourceMarker option -> ILTypeRef -> ILParameter list -> ILMethodDef
val mk_static_mdef: ILGenericParameterDefs * string * ILMemberAccess * ILParameter list * ILReturnValue * MethodBody -> ILMethodDef
val mk_static_nongeneric_mdef: string * ILMemberAccess * ILParameter list * ILReturnValue * MethodBody -> ILMethodDef
val mk_cctor: MethodBody -> ILMethodDef
val mk_generic_virtual_mdef: string * ILMemberAccess * ILGenericParameterDefs * ILParameter list * ILReturnValue * MethodBody -> ILMethodDef
val mk_generic_instance_mdef: string * ILMemberAccess * ILGenericParameterDefs * ILParameter list * ILReturnValue * MethodBody -> ILMethodDef
val mk_virtual_mdef: string * ILMemberAccess * ILParameter list * ILReturnValue * MethodBody -> ILMethodDef
val mk_instance_mdef: string * ILMemberAccess * ILParameter list * ILReturnValue * MethodBody -> ILMethodDef


/// Make field definitions
val mk_instance_fdef: string * ILType * ILFieldInit option * ILMemberAccess -> ILFieldDef
val mk_static_fdef: string * ILType * ILFieldInit option * byte[] option * ILMemberAccess -> ILFieldDef

/// Make a type definition
val mk_generic_class: string * ILTypeDefAccess * ILGenericParameterDefs * ILType * ILType list * ILMethodDefs * ILFieldDefs * PropertyDefs * ILEventDefs * ILAttributes -> ILTypeDef
val mk_simple_tdef: ILGlobals -> string * ILTypeDefAccess * ILMethodDefs * ILFieldDefs * PropertyDefs * ILEventDefs * ILAttributes -> ILTypeDef
val mk_toplevel_tdef: ILGlobals -> ILMethodDefs * ILFieldDefs -> ILTypeDef

/// Make a type definition for a value type used to point to raw data.
/// These are useful when generating array initialization code 
/// according to the 
///   ldtoken    field valuetype '<PrivateImplementationDetails>'/'$$struct0x6000127-1' '<PrivateImplementationDetails>'::'$$method0x6000127-1'
///   call       void System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class System.Array,valuetype System.RuntimeFieldHandle)
/// idiom.
val mk_rawdata_vtdef:  ILGlobals -> string * size:int32 * pack:uint16 -> ILTypeDef

/// Injecting code into existing code blocks.  A branch will
/// be added from the given instructions to the (unique) entry of
/// the code, and the first instruction will be the new entry
/// of the method.  The instructions should be non-branching.

val prepend_instrs_to_code: ILInstr list -> ILCode -> ILCode
val prepend_instrs_to_mdef: ILInstr list -> ILMethodDef -> ILMethodDef

/// Injecting initialization code into a class.
/// Add some code to the end of the .cctor for a type.  Create a .cctor
/// if one doesn't exist already.
val prepend_instrs_to_cctor: ILInstr list -> ILSourceMarker option -> ILTypeDef -> ILTypeDef

/// Derived functions for making some simple constructors
val mk_storage_ctor: ILSourceMarker option * ILInstr list * ILTypeSpec * (string * ILType) list * ILMemberAccess -> ILMethodDef
val mk_simple_storage_ctor: ILSourceMarker option * ILTypeSpec option * ILTypeSpec * (string * ILType) list * ILMemberAccess -> ILMethodDef
val mk_simple_storage_ctor_with_param_names: ILSourceMarker option * ILTypeSpec option * ILTypeSpec * (string * string * ILType) list * ILMemberAccess -> ILMethodDef

val mk_delegate_mdefs: ILGlobals -> ILParameter list * ILReturnValue -> ILMethodDef list

/// Given a delegate type definition which lies in a particular scope, 
/// make a reference to its constructor
val mk_ctor_mspec_for_delegate: ILGlobals -> ILTypeRef * ILGenericArgs * bool -> ILMethodSpec 

/// The toplevel "class" for a module or assembly.
val typ_for_toplevel: ILScopeRef -> ILType

/// Making tables of custom attributes, etc.
val mk_custom_attrs: ILAttribute list -> ILAttributes
val mk_computed_custom_attrs: (unit -> ILAttribute list) -> ILAttributes

val mk_security_decls: ILPermission list -> ILPermissions
val mk_lazy_security_decls: (ILPermission list) Lazy.t -> ILPermissions

val mk_mbody : MethodBody -> LazyMethodBody
val mk_lazy_mbody : MethodBody Lazy.t -> LazyMethodBody

val mk_events: ILEventDef list -> ILEventDefs
val mk_lazy_events: (ILEventDef list) Lazy.t -> ILEventDefs

val mk_properties: ILPropertyDef list -> PropertyDefs
val mk_lazy_properties: (ILPropertyDef list) Lazy.t -> PropertyDefs

val mk_mdefs: ILMethodDef list -> ILMethodDefs
val mk_lazy_mdefs: (ILMethodDef list) Lazy.t -> ILMethodDefs
val add_mdef:  ILMethodDef -> ILMethodDefs -> ILMethodDefs

val mk_fdefs: ILFieldDef list -> ILFieldDefs
val mk_lazy_fdefs: (ILFieldDef list) Lazy.t -> ILFieldDefs

val mk_mimpls: ILMethodImplDef list -> ILMethodImplDefs
val mk_lazy_mimpls: (ILMethodImplDef list) Lazy.t -> ILMethodImplDefs

val mk_tdefs: ILTypeDef  list -> ILTypeDefs

/// Create table of types which is loaded/computed on-demand, and whose individual 
/// elements are also loaded/computed on-demand. Any call to [dest_tdefs] will 
/// result in the laziness being forced.  Operations can examine the
/// custom attributes and name of each type in order to decide whether
/// to proceed with examining the other details of the type.
/// 
/// Note that individual type definitions may contain further delays 
/// in their method, field and other tables. 
val mk_lazy_tdefs: ((string list * string * ILAttributes * ILTypeDef Lazy.t) list) Lazy.t -> ILTypeDefs
val add_tdef: ILTypeDef -> ILTypeDefs -> ILTypeDefs

val mk_nested_exported_types: ILNestedExportedType list -> ILNestedExportedTypes
val mk_lazy_nested_exported_types: (ILNestedExportedType list) Lazy.t -> ILNestedExportedTypes

val mk_exported_types: ILExportedType list -> ILExportedTypes
val mk_lazy_exported_types: (ILExportedType list) Lazy.t ->   ILExportedTypes

val mk_resources: ILResource list -> ILResources
val mk_lazy_resources: (ILResource list) Lazy.t -> ILResources

/// Making modules
val mk_simple_mainmod: assemblyName:string -> moduleName:string -> dll:bool -> ILTypeDefs -> int32 option -> Locale option -> int -> ILModuleDef

/// Default values for some of the strange flags in a module.
val default_modulSubSystem: int32
val default_modulVirtAlignment: int32
val default_modulPhysAlignment: int32
val default_modulImageBase: int32

/// Generate references to existing type definitions, method definitions
/// etc.  Useful for generating references, e.g. to a  class we're processing
/// Also used to reference type definitions that we've generated.  [ILScopeRef] 
/// is normally ScopeRef_local, unless we've generated the ILTypeDef in
/// an auxiliary module or are generating multiple assemblies at 
/// once.

val tref_for_nested_tdef : ILScopeRef -> ILTypeDef list * ILTypeDef               -> ILTypeRef
val tspec_for_nested_tdef: ILScopeRef -> ILTypeDef list * ILTypeDef               -> ILTypeSpec
val mref_for_mdef        : ILScopeRef -> ILTypeDef list * ILTypeDef -> ILMethodDef -> ILMethodRef
val fref_for_fdef        : ILScopeRef -> ILTypeDef list * ILTypeDef -> ILFieldDef  -> ILFieldRef

val mk_mref_to_mdef: ILTypeRef * ILMethodDef -> ILMethodRef
val mk_fref_to_fdef: ILTypeRef * ILFieldDef -> ILFieldRef

val assref_for_manifest: ILAssemblyManifest -> ILAssemblyRef
val assref_for_mainmod: ILModuleDef -> ILAssemblyRef
val modref_for_modul: ILModuleDef -> ILModuleRef


// -------------------------------------------------------------------- 
// Rescoping.
//
// Given an object O1 referenced from where1 (e.g. O1 binds to some  
// result R when referenced from where1), and given that SR2 resolves to where1 from where2, 
// produce a new O2 for use from where2 (e.g. O2 binds to R from where2)
//
// So, ILScopeRef tells you how to reference the original scope from 
// the new scope. e.g. if ILScopeRef is:
//    [ScopeRef_local] then the object is returned unchanged
//    [ScopeRef_module m] then an object is returned 
//                        where all ScopeRef_local references 
//                        become ScopeRef_module m
//    [ScopeRef_assembly m] then an object is returned 
//                         where all ScopeRef_local and ScopeRef_module references 
//                        become ScopeRef_assembly m
// -------------------------------------------------------------------- 

/// Rescoping. The first argument tells the function how to reference the original scope from 
/// the new scope. 
val rescope_scoref: ILScopeRef -> ILScopeRef -> ILScopeRef
/// Rescoping. The first argument tells the function how to reference the original scope from 
/// the new scope. 
val rescope_tspec: ILScopeRef -> ILTypeSpec -> ILTypeSpec
/// Rescoping. The first argument tells the function how to reference the original scope from 
/// the new scope. 
val rescope_typ: ILScopeRef -> ILType -> ILType
/// Rescoping. The first argument tells the function how to reference the original scope from 
/// the new scope. 
val rescope_mspec: ILScopeRef -> ILMethodSpec -> ILMethodSpec
/// Rescoping. The first argument tells the function how to reference the original scope from 
/// the new scope. 
val rescope_ospec: ILScopeRef -> OverridesSpec -> OverridesSpec
/// Rescoping. The first argument tells the function how to reference the original scope from 
/// the new scope. 
val rescope_mref: ILScopeRef -> ILMethodRef -> ILMethodRef 
/// Rescoping. The first argument tells the function how to reference the original scope from 
/// the new scope. 
val rescope_fref: ILScopeRef -> ILFieldRef -> ILFieldRef
/// Rescoping. The first argument tells the function how to reference the original scope from 
/// the new scope. 
val rescope_fspec: ILScopeRef -> ILFieldSpec -> ILFieldSpec


//-----------------------------------------------------------------------
// The ILCode Builder utility.
//----------------------------------------------------------------------

type ExceptionClause = 
  | SEH_finally of (ILCodeLabel * ILCodeLabel)
  | SEH_fault  of (ILCodeLabel * ILCodeLabel)
  | SEH_filter_catch of (ILCodeLabel * ILCodeLabel) * (ILCodeLabel * ILCodeLabel)
  | SEH_type_catch of ILType * (ILCodeLabel * ILCodeLabel)

type ILExceptionSpec = 
    { exnRange: (ILCodeLabel * ILCodeLabel);
      exnClauses: ExceptionClause list }

type ILLocalSpec = 
    { locRange: (ILCodeLabel * ILCodeLabel);
      locInfos: ILDebugMapping list }

/// build_code: Build code from a sequence of instructions.
/// 
/// e.g. "build_code meth resolver instrs exns locals"
/// 
/// This makes the basic block structure of code from more primitive
/// information, i.e. an array of instructions.
///   [meth]: for debugging and should give the name of the method.
///   [resolver]: should return the instruction indexes referred to 
///               by code-label strings in the instruction stream.
///   [instrs]: the instructions themselves, perhaps with attributes giving 
///             debugging information
///   [exns]: the table of exception-handling specifications
///           for the method.  These are again given with respect to labels which will
///           be mapped to pc's by [resolver].  
///   [locals]: the table of specifications of when local variables are live and
///           should appear in the debug info.
/// 
/// If the input code is well-formed, the function will returns the 
/// chop up the instruction sequence into basic blocks as required for
/// the exception handlers and then return the tree-structured code
/// corresponding to the instruction stream.
/// A new set of code labels will be used throughout the resulting code.
/// 
/// The input can be badly formed in many ways: exception handlers might
/// overlap, or scopes of local variables may overlap badly with 
/// exception handlers.
val build_code:
    string ->
    (ILCodeLabel -> int) -> 
    ILInstr array -> 
    ILExceptionSpec list -> 
    ILLocalSpec list -> 
    ILCode

// -------------------------------------------------------------------- 
// The instantiation utilities.
// -------------------------------------------------------------------- 

/// Instantiate type variables that occur within types and other items. 
val inst_typ_aux: int -> ILGenericArgs -> ILType -> ILType

/// Instantiate type variables that occur within types and other items. 
val inst_typ: ILGenericArgs -> ILType -> ILType

/// Instantiate type variables that occur within types and other items. 
val inst_inst: ILGenericArgs -> ILGenericArgs -> ILGenericArgs

/// Instantiate type variables that occur within types and other items. 
val inst_tspec: ILGenericArgs -> ILTypeSpec -> ILTypeSpec

/// Instantiate type variables that occur within types and other items. 
val inst_callsig: ILGenericArgs -> ILCallingSignature -> ILCallingSignature

/// Instantiate type variables that occur within types and other items. 
val inst_read: ILGenericArgs -> uint16 -> ILType

/// Instantiate type variables that occur within types and other items. 
val inst_add: ILGenericArgs -> ILGenericArgs -> ILGenericArgs

/// Names of some commonly used things in mscorlib...
val mscorlib_module_name: string

/// This is a 'vendor neutral' way of referencing mscorlib. 
val ecma_public_token: PublicKey
/// This is a 'vendor neutral' way of referencing mscorlib. 
val ecma_mscorlib_scoref: ILScopeRef
/// This is a 'vendor neutral' collection of references to items in mscorlib. 
val ecmaILGlobals: ILGlobals


/// Some commonly used methods 
val mspec_RuntimeHelpers_InitializeArray: ILGlobals -> ILMethodSpec 
val mspec_RunClassConstructor: ILGlobals -> ILMethodSpec
val mspec_StringBuilder_string: ILGlobals -> ILMethodSpec
val mk_RunClassConstructor: ILGlobals -> ILTypeSpec -> ILInstr list

val mk_mscorlib_exn_newobj: ILGlobals -> string -> ILInstr

/// Some commonly used custom attibutes 
val mk_DebuggableAttribute: ILGlobals -> bool (* debug tracking *) * bool (* disable JIT optimizations *) -> ILAttribute
val mk_DebuggableAttribute_v2: ILGlobals -> bool (* jitTracking *) * bool (* ignoreSymbolStoreSequencePoints *) * bool (* disable JIT optimizations *) * bool (* enable EnC *) -> ILAttribute

val mk_CompilerGeneratedAttribute          : ILGlobals -> ILAttribute
val mk_DebuggerNonUserCodeAttribute        : ILGlobals -> ILAttribute
val mk_DebuggerHiddenAttribute             : ILGlobals -> ILAttribute
val mk_DebuggerDisplayAttribute            : ILGlobals -> string -> ILAttribute
val mk_DebuggerTypeProxyAttribute          : ILGlobals -> ILType -> ILAttribute
val mk_DebuggerBrowsableNeverAttribute     : ILGlobals -> ILAttribute
val mk_DebuggerBrowsableRootHiddenAttribute: ILGlobals -> ILAttribute
val mk_DebuggerBrowsableCollapsedAttribute : ILGlobals -> ILAttribute

val add_mdef_generated_attrs : ILGlobals -> ILMethodDef -> ILMethodDef
val add_pdef_generated_attrs : ILGlobals -> ILPropertyDef -> ILPropertyDef
val add_fdef_generated_attrs : ILGlobals -> ILFieldDef -> ILFieldDef

val add_pdef_never_attrs : ILGlobals -> ILPropertyDef -> ILPropertyDef
val add_fdef_never_attrs : ILGlobals -> ILFieldDef -> ILFieldDef

/// Discriminating different important built-in types
val typ_is_Object: ILGlobals -> ILType -> bool
val typ_is_String: ILGlobals -> ILType -> bool
val typ_is_SByte: ILGlobals -> ILType -> bool
val typ_is_Byte: ILGlobals -> ILType -> bool
val typ_is_Int16: ILGlobals -> ILType -> bool
val typ_is_UInt16: ILGlobals -> ILType -> bool
val typ_is_Int32: ILGlobals -> ILType -> bool
val typ_is_UInt32: ILGlobals -> ILType -> bool
val typ_is_Int64: ILGlobals -> ILType -> bool
val typ_is_UInt64: ILGlobals -> ILType -> bool
val typ_is_IntPtr: ILGlobals -> ILType -> bool
val typ_is_UIntPtr: ILGlobals -> ILType -> bool
val typ_is_Bool: ILGlobals -> ILType -> bool
val typ_is_Char: ILGlobals -> ILType -> bool
val typ_is_TypedReference: ILGlobals -> ILType -> bool
val typ_is_Double: ILGlobals -> ILType -> bool
val typ_is_Single: ILGlobals -> ILType -> bool

/// Get a public key token from a public key.
val sha1_hash_bytes : byte[] -> byte[] (* SHA1 hash *)

/// Get a version number from a CLR version string, e.g. 1.0.3705.0
val parse_version: string -> ILVersionInfo
val version_to_string: ILVersionInfo -> string
val version_compare: ILVersionInfo -> ILVersionInfo -> int
val version_max: ILVersionInfo -> ILVersionInfo -> ILVersionInfo
val version_min: ILVersionInfo -> ILVersionInfo -> ILVersionInfo


/// Decompose a type definition according to its kind.
type ILEnumInfo =
    { enumValues: (string * ILFieldInit) list;  
      enumType: ILType }

val typ_of_enum_info: ILEnumInfo -> ILType

val info_for_enum: string * ILFieldDefs -> ILEnumInfo

val memoize_on: ('a -> 'key) -> ('a -> 'b) -> ('a -> 'b)
val memoize: mapping:('a -> 'b) -> ('a -> 'b)

// -------------------------------------------------------------------- 
// For completeness.  These do not occur in metadata but tools that
// care about the existence of properties and events in the metadata
// can benefit from them.
// -------------------------------------------------------------------- 

[<Sealed>]
type ILEventRef =
    static member Create : ILTypeRef * string -> ILEventRef
    member EnclosingTypeRef: ILTypeRef
    member Name: string

[<Sealed>]
type ILEventSpec =
     static member Create : ILEventRef * ILType -> ILEventSpec
     member EventRef: ILEventRef
     member EnclosingType: ILType

[<Sealed>]
type ILPropertyRef =
    static member Create : ILTypeRef * string -> ILPropertyRef
    member EnclosingTypeRef: ILTypeRef
    member Name: string

[<Sealed>]
type ILPropertySpec =
    static member Create : ILPropertyRef * ILType -> ILPropertySpec
    member PropertyRef: ILPropertyRef
    member EnclosingType: ILType

val tref_of_pref : ILPropertyRef -> ILTypeRef
val tref_of_eref : ILEventRef -> ILTypeRef
val mk_pref : ILTypeRef * string -> ILPropertyRef
val mk_eref : ILTypeRef * string -> ILEventRef
val mk_pspec : ILPropertyRef * ILType -> ILPropertySpec
val mk_espec : ILEventRef * ILType -> ILEventSpec
val name_of_pref : ILPropertyRef -> string
val name_of_eref : ILEventRef -> string
val enclosing_typ_of_pspec : ILPropertySpec -> ILType
val enclosing_typ_of_espec : ILEventSpec -> ILType
val pref_of_pspec : ILPropertySpec -> ILPropertyRef
val eref_of_espec : ILEventSpec -> ILEventRef
val eref_for_edef : ILScopeRef -> ILTypeDef list * ILTypeDef -> ILEventDef  -> ILEventRef
val pref_for_pdef : ILScopeRef -> ILTypeDef list * ILTypeDef -> ILPropertyDef  -> ILPropertyRef


// -------------------------------------------------------------------- 
// The referenced-assemblies utility.

val runningOnMono: bool

type ILReferences = 
    { refsAssembly: ILAssemblyRef list; 
      refsModul: ILModuleRef list; }
    member AssemblyReferences: ILAssemblyRef list
    member ModuleReferences: ILModuleRef list

/// Find the full set of assemblies referenced by a module 
val refs_of_module: ILModuleDef -> ILReferences
val empty_refs: ILReferences

// -------------------------------------------------------------------- 
// The following functions are used to define an extension to the IL. In reality the only extension is ILX

type ILInstrSetExtension<'a> = 
    { instrExtDests: ('a -> ILCodeLabel list);
      instrExtFallthrough: ('a -> ILCodeLabel option);
      instrExtIsTailcall: ('a -> bool);
      instrExtRelabel: (ILCodeLabel -> ILCodeLabel) -> 'a -> 'a; }

type ILTypeDefKindExtension<'a> = Type_def_kind_extension

val define_instr_extension: 'a ILInstrSetExtension -> ('a -> IlxExtensionInstr) * (IlxExtensionInstr -> bool) * (IlxExtensionInstr -> 'a)
val define_type_def_kind_extension: 'a ILTypeDefKindExtension -> ('a -> IlxExtensionTypeKind) * (IlxExtensionTypeKind -> bool) * (IlxExtensionTypeKind -> 'a)
