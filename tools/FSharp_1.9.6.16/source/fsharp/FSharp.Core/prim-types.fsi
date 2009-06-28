//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

#nowarn "35" // This construct is deprecated: the treatment of this operator is now handled directly by the F# compiler and its meaning may not be redefined.
#nowarn "61" // The containing type can use 'null' as a representation value for its nullary union case. This member will be compiled as a static member.
#nowarn "62" // This construct is for compatibility with OCaml. The syntax 'module ... : sig .. end' is deprecated unless OCaml compatibility is enabled. Consider using 'module ... = begin .. end'.

/// Basic F# type definitions, functions and operators 
namespace Microsoft.FSharp.Core

    open System

    /// An abbreviation for the .NET type <c>System.Object</c>
    type obj = System.Object

    /// An abbreviation for the .NET type <c>System.Exception</c>
    type exn = System.Exception

    /// An abbreviation for the .NET type <c>System.IntPtr</c>
    type nativeint = System.IntPtr
    /// An abbreviation for the .NET type <c>System.UIntPtr</c>
    type unativeint = System.UIntPtr

    /// An abbreviation for the .NET type <c>System.String</c>
    type string = System.String

    /// An abbreviation for the .NET type <c>System.Single</c>
    type float32 = System.Single
    /// An abbreviation for the .NET type <c>System.Double</c>
    type float = System.Double
    /// An abbreviation for the .NET type <c>System.Single</c>
    type single = System.Single
    /// An abbreviation for the .NET type <c>System.Double</c>
    type double = System.Double

    /// An abbreviation for the .NET type <c>System.SByte</c>
    type sbyte = System.SByte
    /// An abbreviation for the .NET type <c>System.Byte</c>
    type byte = System.Byte
    /// An abbreviation for the .NET type <c>System.SByte</c>
    type int8 = System.SByte
    /// An abbreviation for the .NET type <c>System.Byte</c>
    type uint8 = System.Byte

    /// An abbreviation for the .NET type <c>System.Int16</c>
    type int16 = System.Int16
    /// An abbreviation for the .NET type <c>System.UInt16</c>
    type uint16 = System.UInt16

    /// An abbreviation for the .NET type <c>System.Int32</c>
    type int32 = System.Int32
    /// An abbreviation for the .NET type <c>System.UInt32</c>
    type uint32 = System.UInt32

    /// An abbreviation for the .NET type <c>System.Int64</c>
    type int64 = System.Int64
    /// An abbreviation for the .NET type <c>System.UInt64</c>
    type uint64 = System.UInt64

    /// An abbreviation for the .NET type <c>System.Char</c>
    type char = System.Char
    /// An abbreviation for the .NET type <c>System.Boolean</c>
    type bool = System.Boolean
    /// An abbreviation for the .NET type <c>System.Decimal</c>
    type decimal = System.Decimal

    /// An abbreviation for the .NET type <c>System.Int32</c>
    type int = int32

    /// Single dimensional, zero-based arrays, written 'int[]', 'string[]' etc.
    /// Use the values in the <c>Array</c> module to manipulate values 
    /// of this type, or the notation 'arr.[x]' to get/set array
    /// values.   
    type 'T ``[]`` = (# "!0[]" #)

    /// Two dimensional arrays, typically zero-based. 
    ///
    /// Use the values in the <c>Array2D</c> module
    /// to manipulate values of this type, or the notation 'arr.[x,y]' to get/set array
    /// values.   
    ///
    /// Non-zero-based arrays can also be created using methods on the System.Array type.
    type 'T ``[,]`` = (# "!0[0 ... , 0 ... ]" #)

    /// Three dimensional arrays, typically zero-based. Non-zero-based arrays
    /// can be created using methods on the System.Array type.
    ///
    /// Use the values in the <c>Array3D</c> module
    /// to manipulate values of this type, or the notation 'arr.[x1,x2,x3]' to get and set array
    /// values.   
    type 'T ``[,,]`` = (# "!0[0 ...,0 ...,0 ...]" #)

    /// Four dimensional arrays, typically zero-based. Non-zero-based arrays
    /// can be created using methods on the System.Array type.
    ///
    /// Use the values in the <c>Array4D</c> module
    /// to manipulate values of this type, or the notation 'arr.[x1,x2,x3,x4]' to get and set array
    /// values.   
    type 'T ``[,,,]`` = (# "!0[0 ...,0 ...,0 ...,0 ...]" #)

    /// Single dimensional, zero-based arrays, written 'int[]', 'string[]' etc.
    /// Use the values in the <c>Array</c> module to manipulate values 
    /// of this type, or the notation 'arr.[x]' to get/set array
    /// values.   
    type 'T array = 'T[]
            
           
    /// Represents a managed pointer in F# code.
    type 'T byref = (# "!0&" #)

    /// Represents an unmanaged pointer in F# code.
    ///
    /// This type should only be used when writing F# code that interoperates
    /// with native code.  Use of this type in F# code may result in
    /// unverifiable code being generated.  Conversions to and from the 
    /// <c>nativeint</c> type may be required. Values of this type can be generated
    /// by the functions in the <c>NativeInterop.NativePtr</c> module.
    type nativeptr<'T> = (# "native int" #)

    /// This type is for internal use by the F# code generator
    type 'T ilsigptr = (# "!0*" #)



#if FX_ATLEAST_40
    // we get IStructrualEquatable and IStructuralComparable from System
    type IStructuralEquatable = System.Collections.IStructuralEquatable
    type IStructuralComparable = System.Collections.IStructuralComparable
#else
    //-------------------------------------------------------------------------
    // Structural equality
    type IStructuralEquatable =
        interface 
            abstract Equals: o:obj * comp:System.Collections.IEqualityComparer -> bool
            abstract GetHashCode: comp:System.Collections.IEqualityComparer -> int
        end
    
    //-------------------------------------------------------------------------    
    // Structural comparison
    and IStructuralComparable =
        interface
            abstract CompareTo: o:obj * comp:System.Collections.IComparer -> int
        end
#endif    

    /// The type 'unit', which has only one value "()". This value is special and
    /// always uses the representation 'null'.
    and Unit =
           interface System.IComparable
        
    /// The type 'unit', which has only one value "()". This value is special and
    /// always uses the representation 'null'.
    and unit = Unit
    
    /// Indicates the relationship between a compiled entity in a CLI binary and an element in F# source code
    type SourceConstructFlags = 
       /// Indicates that the compiled entity has no relationship to an element in F# source code
       | None = 0
       /// Indicates that the compiled entity is part of the representation of an F# union type declaration
       | SumType = 1
       /// Indicates that the compiled entity is part of the representation of an F# record type declaration
       | RecordType = 2
       /// Indicates that the compiled entity is part of the representation of an F# class or other object type declaration
       | ObjectType = 3
       /// Indicates that the compiled entity is part of the representation of an F# record or union case field declaration
       | Field = 4
       /// Indicates that the compiled entity is part of the representation of an F# exception declaration
       | Exception = 5
       /// Indicates that the compiled entity is part of the representation of an F# closure
       | Closure = 6
       /// Indicates that the compiled entity is part of the representation of an F# module declaration
       | Module = 7
       /// Indicates that the compiled entity is part of the representation of an F# union case declaration
       | UnionCase = 8
       /// Indicates that the compiled entity is part of the representation of an F# value declaration
       | Value = 9
       /// The mask of values related to the kind of the compiled entity
       | KindMask = 31
       /// Indicates that the compiled entity had private or internal representation in F# source code
       | NonpublicRepresentation = 32

    [<Flags>]
        /// Indicates one or more adjustments to the compiled representation of an F# type or member
    type CompilationRepresentationFlags = 
       /// No special compilation representation
       | None = 0
       /// Compile an instance member as 'static' 
       | Static = 1
       /// Compile a member as 'instance' even if <c>null</c> is used as a representation for this type
       | Instance = 2
       /// append 'Module' to the end of a module whose name clashes with a type name in the same namespace
       | ModuleSuffix = 4  
       /// Permit the use of <c>null</c> as a representation for nullary discriminators in a discriminated union
       | UseNullAsTrueValue = 8
       /// Compile a property as a .NET event
       | Event = 16

    //-------------------------------------------------------------------------
    // F#-specific Attributes


    /// Adding this attribute to class definition makes it sealed, which means it may not
    /// be extended or implemented.
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    type SealedAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> SealedAttribute
        
        /// The value of the attribute, indicating whether the type is sealed or not
        member Value: bool
        /// Create an instance of the attribute
        new : value:bool -> SealedAttribute

    /// Adding this attribute to class definition makes it abstract, which means it need not
    /// implement all its methods. Instances of abstract classes may not be constructed directly.
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type AbstractClassAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> AbstractClassAttribute


    /// Adding this attribute to the let-binding for the definition of a top-level 
    /// value makes the quotation expression that implements the value available
    /// for use at runtime.
    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type ReflectedDefinitionAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> ReflectedDefinitionAttribute

    /// Adding this attribute to a type causes it to be represented using a .NET struct.
    [<AttributeUsage (AttributeTargets.Struct,AllowMultiple=false)>]  
    [<Sealed>]
    type StructAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> StructAttribute

    /// Adding this attribute to a type causes it to be interpreted as a unit of measure.
    /// This may only be used under very limited conditions.
    [<AttributeUsage (AttributeTargets.GenericParameter ||| AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type MeasureAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> MeasureAttribute

    /// Adding this attribute to a type causes it to be interpreted as a refined type, currently limited to measure-parameterized types.
    /// This may only be used under very limited conditions.
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type MeasureAnnotatedAbbreviationAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> MeasureAnnotatedAbbreviationAttribute

    /// Adding this attribute to a type causes it to be represented using a .NET interface.
    [<AttributeUsage (AttributeTargets.Interface,AllowMultiple=false)>]  
    [<Sealed>]
    type InterfaceAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> InterfaceAttribute

    /// Adding this attribute to a type causes it to be represented using a .NET class.
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type ClassAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> ClassAttribute

    /// Adding this attribute to a value causes it to be compiled as a .NET constant literal.
    [<AttributeUsage (AttributeTargets.Field,AllowMultiple=false)>]  
    [<Sealed>]
    type LiteralAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> LiteralAttribute

    /// Adding this attribute to a property with event type causes it to be compiled with as a .NET 
    /// Common Language Infrastructure metadata event, through a syntactic translation to a pair of
    /// 'add_EventName' and 'remove_EventName' methods.
    [<AttributeUsage (AttributeTargets.Property,AllowMultiple=false)>]  
    [<Sealed>]
    type CLIEventAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> CLIEventAttribute

    /// Adding this attribute to a discriminated union with value false
    /// turns off the generation of standard helper member tester, constructor 
    /// and accessor members for the generated .NET class for that type.
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type DefaultAugmentationAttribute =
        inherit System.Attribute
        /// The value of the attribute, indicating whether the type has a default augmentation or not
        member Value: bool
        /// Create an instance of the attribute
        new : value:bool -> DefaultAugmentationAttribute

    /// Adding this attribute to a function indicates it is the entrypoint for an application.
    /// If this absent is not speficied for an EXE then the initialization implicit in the
    /// module bindings in the last file in the compilation sequence are used as the entrypoint.
    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type EntryPointAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> EntryPointAttribute

    /// Adding this attribute to a record or union type disables the automatic generation
    /// of overrides for 'System.Object.Equals(obj)', 'System.Object.GetHashCode()' 
    /// and 'System.IComparable' for the type. The type will by default use reference equality.
    /// This is identical to adding attributes StructuralEquality(false) and StructuralComparison(false).
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type ReferenceEqualityAttribute =
        inherit System.Attribute
        member Value: bool
        /// Create an instance of the attribute
        new : unit -> ReferenceEqualityAttribute
        /// Create an instance of the attribute
        new : value:bool -> ReferenceEqualityAttribute

    /// Adding this attribute to a record, union or struct type with value 'false' 
    /// confirms the automatic generation of overrides for 'System.Object.Equals(obj)' 
    /// and 'System.Object.GetHashCode()' for the type. This attribute is usually used in 
    /// conjunction with StructuralComparison(false) to generate a type that supports
    /// structural equality but not structural comparison.
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type StructuralEqualityAttribute =
        inherit System.Attribute
        /// The value of the attribute, indicating whether the type uses structural equality or not
        member Value: bool
        /// Create an instance of the attribute
        new : unit -> StructuralEqualityAttribute
        /// Create an instance of the attribute
        new : value:bool -> StructuralEqualityAttribute

    /// Adding this attribute to a record, union or struct type with value 'false' disables the automatic generation
    /// of implementations for 'System.IComparable' for the type.
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type StructuralComparisonAttribute =
        inherit System.Attribute
        /// The value of the attribute, indicating whether the type uses structural comparison or not
        member Value: bool
        /// Create an instance of the attribute
        new : unit -> StructuralComparisonAttribute
        /// Create an instance of the attribute
        new : value:bool -> StructuralComparisonAttribute

    /// Adding this attribute to a field declaration means that the field is 
    /// not initialized. During type checking a constraint is asserted that the field type supports 'null'. 
    /// If the 'check' value is false then the constraint is not asserted. 
    [<AttributeUsage (AttributeTargets.Field,AllowMultiple=false)>]  
    [<Sealed>]
    type DefaultValueAttribute =
        inherit System.Attribute
        /// Indicates if a constraint is asserted that the field type supports 'null'
        member Check: bool
        /// Create an instance of the attribute
        new : unit -> DefaultValueAttribute
        /// Create an instance of the attribute
        new : check: bool -> DefaultValueAttribute

    /// This attribute is added automatically for all optional arguments
    [<AttributeUsage (AttributeTargets.Parameter,AllowMultiple=false)>]  
    [<Sealed>]
    type OptionalArgumentAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> OptionalArgumentAttribute

    /// Adding this attribute to a type, value or member requires that 
    /// uses of the construct must explicitly instantiate any generic type parameters.
    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type RequiresExplicitTypeArgumentsAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> RequiresExplicitTypeArgumentsAttribute

    /// Adding this attribute to a non-function value with generic parameters indicates that 
    /// uses of the construct can give rise to generic code through type inference. 
    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type GeneralizableValueAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> GeneralizableValueAttribute

    /// Adding the OverloadID attribute to a member permits it to
    /// be part of a group overloaded by the same name and arity.  The string
    /// must be a unique name amongst those in the overload set.  Overrides
    /// of this method, if permitted, must be given the same OverloadID,
    /// and the OverloadID must be specified in both signature and implementation
    /// files if signature files are used.
    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type OverloadIDAttribute =
        inherit System.Attribute
        /// A unique identifier for this overloaded member within a given overload set
        member UniqueName: string
        /// Create an instance of the attribute
        new : uniqueName:string -> OverloadIDAttribute

    /// Adding this attribute to a type with value 'false' disables the behaviour where F# makes the
    /// type Serializable by default.
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type AutoSerializableAttribute =
        inherit System.Attribute
        /// The value of the attribute, indicating whether the type is automatically marked serializable or not
        member Value: bool
        /// Create an instance of the attribute
        new : value:bool -> AutoSerializableAttribute

    /// This attribute is added to generated assemblies to indicate the 
    /// version of the data schema used to encode additional F#
    /// specific information in the resource attached to compiled F# libraries.
    [<AttributeUsage (AttributeTargets.Assembly,AllowMultiple=false)>]  
    [<Sealed>]
    type FSharpInterfaceDataVersionAttribute =
        inherit System.Attribute
        /// The major version number of the F# version associated with the attribute
        member Major: int
        /// The minor version number of the F# version associated with the attribute
        member Minor: int
        /// The release number of the F# version associated with the attribute
        member Release: int
        /// Create an instance of the attribute
        new : major:int * minor:int * release:int -> FSharpInterfaceDataVersionAttribute

    /// This attribute is inserted automatically by the F# compiler to tag 
    /// types and methods in the gneerated .NET code with flags indicating the correspondence with
    /// original source constructs.  It is used by the functions in the
    /// Microsoft.FSharp.Reflection library to reverse-map compiled constructs
    /// to their original forms.  It is not intended for use from use code.
    [<AttributeUsage (AttributeTargets.All,AllowMultiple=false)>]  
    [<Sealed>]
    type CompilationMappingAttribute =
        inherit System.Attribute
        /// Indicates the relationship between the compiled entity and F# source code
        member SourceConstructFlags : SourceConstructFlags
        /// Indicates the sequence number of the entity, if any, in a linear sequence of elements with F# source code
        member SequenceNumber : int
        /// Indicates the variant number of the entity, if any, in a linear sequence of elements with F# source code
        member VariantNumber : int
        /// Create an instance of the attribute
        new : sourceConstructFlags:SourceConstructFlags -> CompilationMappingAttribute
        /// Create an instance of the attribute
        new : sourceConstructFlags:SourceConstructFlags * sequenceNumber: int -> CompilationMappingAttribute
        /// Create an instance of the attribute
        new : sourceConstructFlags:SourceConstructFlags * variantNumber : int * sequenceNumber : int -> CompilationMappingAttribute

    /// This attribute is used to adjust the runtime representation for a type. 
    /// For example, it may be used to note that the <c>null</c> representation
    /// may be used for a type.  This affects how some constructs are compiled.
    [<AttributeUsage (AttributeTargets.All,AllowMultiple=false)>]  
    [<Sealed>]
    type CompilationRepresentationAttribute =
        inherit System.Attribute
        /// Indicates one or more adjustments to the compiled representation of an F# type or member
        member Flags : CompilationRepresentationFlags
        /// Create an instance of the attribute
        new : flags:CompilationRepresentationFlags -> CompilationRepresentationAttribute

    /// This attribute is used to tag values that are part of an experimental library
    /// feature
    [<AttributeUsage (AttributeTargets.All,AllowMultiple=false)>]  
    [<Sealed>]
    type ExperimentalAttribute =
        inherit System.Attribute
        /// Indicates the warning message to be emitted when F# source code uses this construct
        member Message: string
        /// Create an instance of the attribute
        new : message:string-> ExperimentalAttribute

    /// This attribute is used to mark how a type is displayed by default when using 
    /// '%A' printf formatting patterns and other two-dimensional text-based display layouts. 
    /// In this version of F# the only valid values are of the form <c>PreText {PropertyName} PostText</c>.
    /// The property name indicates a property to evaluate and to display instead of the object itself. 
    [<AttributeUsage (AttributeTargets.Class ||| AttributeTargets.Interface ||| AttributeTargets.Struct ||| AttributeTargets.Delegate ||| AttributeTargets.Enum,AllowMultiple=false)>]  
    [<Sealed>]
    type StructuredFormatDisplayAttribute =
        inherit System.Attribute
        /// Indicates the text to display by default when objects of this type are displayed 
        /// using '%A' printf formatting patterns and other two-dimensional text-based display 
        /// layouts. 
        member Value: string
        /// Create an instance of the attribute
        new : value:string-> StructuredFormatDisplayAttribute


    /// This attribute is used to tag values, modules and types that are only
    /// present in F# to permit a degree of code-compatibility and cross-compilation
    /// with other implementations of ML-familty languages, in particular OCaml. The
    /// use of the construct will give a warning unless the --ml-compatibility flag
    /// is specified.
    [<AttributeUsage (AttributeTargets.All,AllowMultiple=false)>]  
    [<Sealed>]
    type OCamlCompatibilityAttribute =
        inherit System.Attribute
        /// Indicates the warning message to be emitted when F# source code uses this construct
        member Message: string
        /// Create an instance of the attribute
        new : unit -> OCamlCompatibilityAttribute
        /// Create an instance of the attribute
        new : message:string -> OCamlCompatibilityAttribute

    /// This attribute is used to tag values whose use will result in the generation
    /// of unverifiable code. These values are inevitably marked 'inline' to ensure that
    /// the unverifiable constructs are not present in the actual code for the F# library,
    /// but are rather copied to the source code of the caller.
    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type UnverifiableAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> UnverifiableAttribute

    /// This attribute is used to tag values that may not be dynamically invoked at runtime. This is
    /// typically added to inlined functions whose implementations include unverifiable code. It
    /// causes the method body emitted for the inlined function to raise an exception if 
    /// dynamically invoked, rather than including the unverifiable code in the generated
    /// assembly.
    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type NoDynamicInvocationAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> NoDynamicInvocationAttribute


    /// This attribute is used to indicate that references to a the elements of a module, record or union 
    /// type require explicit qualified access.
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type RequireQualifiedAccessAttribute =
        inherit System.Attribute
        /// Create an instance of the attribute
        new : unit -> RequireQualifiedAccessAttribute

    /// This attribute is used for two purposes. When applied to an assembly, it must be given a string
    /// argument, and this argument must indicate a valid module or namespace in that assembly. Source
    /// code files compiled with a reference to this assembly are processed in an environment
    /// where the given path is automatically oepned.
    ///
    /// When applied to a module within an assembly, then the attribute must not be given any arguments.
    /// When the enclosing namespace is opened in user source code, the module is also implicitly opened.
    [<AttributeUsage (AttributeTargets.Class ||| AttributeTargets.Assembly ,AllowMultiple=true)>]  
    [<Sealed>]
    type AutoOpenAttribute =
        inherit System.Attribute
        /// Indicates the namespace or module to be automatically opened when an assembly is referenced
        /// or an enclosing module opened.
        member Path: string
        /// Create an attribute used to mark a module as 'automatically opened' when the enclosing namespace is opened
        new : unit -> AutoOpenAttribute
        /// Create an attribute used to mark a namespace or module path to be 'automatically opened' when an assembly is referenced
        new : path:string-> AutoOpenAttribute

    //-------------------------------------------------------------------------
    // Units of measure

    [<MeasureAnnotatedAbbreviation>] 
    /// The type of floating point numbers, annotated with a unit of measure. The unit
    /// of measure is erased in compiled code and when values of this type
    /// are analyzed using reflection. The type is representationally equivalent to 
    /// <c>System.Double</c>.
    type float<[<Measure>] 'U> = float

    [<MeasureAnnotatedAbbreviation>] 
    /// The type of floating point numbers, annotated with a unit of measure. The unit
    /// of measure is erased in compiled code and when values of this type
    /// are analyzed using reflection. The type is representationally equivalent to 
    /// <c>System.Single</c>.
    type float32<[<Measure>] 'U> = float32
    
    [<MeasureAnnotatedAbbreviation>] 
    /// The type of decimal numbers, annotated with a unit of measure. The unit
    /// of measure is erased in compiled code and when values of this type
    /// are analyzed using reflection. The type is representationally equivalent to 
    /// <c>System.Decimal</c>.
    type decimal<[<Measure>] 'U> = decimal

    [<MeasureAnnotatedAbbreviation>] 
    /// The type of 32-bit signed integer numbers, annotated with a unit of measure. The unit
    /// of measure is erased in compiled code and when values of this type
    /// are analyzed using reflection. The type is representationally equivalent to 
    /// <c>System.Int32</c>.
    type int<[<Measure>] 'U> = int

    [<MeasureAnnotatedAbbreviation>] 
    /// The type of 8-bit signed integer numbers, annotated with a unit of measure. The unit
    /// of measure is erased in compiled code and when values of this type
    /// are analyzed using reflection. The type is representationally equivalent to 
    /// <c>System.SByte</c>.
    type int8<[<Measure>] 'U> = int8

    [<MeasureAnnotatedAbbreviation>] 
    /// The type of 16-bit signed integer numbers, annotated with a unit of measure. The unit
    /// of measure is erased in compiled code and when values of this type
    /// are analyzed using reflection. The type is representationally equivalent to 
    /// <c>System.Int16</c>.
    type int16<[<Measure>] 'U> = int16

    [<MeasureAnnotatedAbbreviation>] 
    /// The type of 64-bit signed integer numbers, annotated with a unit of measure. The unit
    /// of measure is erased in compiled code and when values of this type
    /// are analyzed using reflection. The type is representationally equivalent to 
    /// <c>System.Int64</c>.
    type int64<[<Measure>] 'U> = int64

    //[<Measure>] 
    //type MeasureProduct<[<Measure>] 'U1, [<Measure>] 'U2> = decimal


    /// Language primitives associated with the F# language
    module LanguagePrimitives =


        /// Compare two values for equality
        val inline GenericEquality : e1:'T -> e2:'T -> bool
        
        /// Compare two values for equality
        val inline GenericEqualityWithComparer : comp:System.Collections.IEqualityComparer -> e1:'T -> e2:'T -> bool

        /// Compare two values 
        val inline GenericComparison : e1:'T -> e2:'T -> int

        /// Compare two values. May be called as a recursive case from an implementation of System.IComparable to
        /// ensure consistent NaN comparison semantics.
        val inline GenericComparisonWithComparer : comp:System.Collections.IComparer -> e1:'T -> e2:'T -> int

        /// Compare two values   
        val inline GenericLessThan : e1:'T -> e2:'T -> bool

        /// Compare two values   
        val inline GenericGreaterThan : e1:'T -> e2:'T -> bool

        /// Compare two values   
        val inline GenericLessOrEqual : e1:'T -> e2:'T -> bool

        /// Compare two values   
        val inline GenericGreaterOrEqual : e1:'T -> e2:'T -> bool

        /// Take the minimum of two values structurally according to the order given by GenericComparison
        val inline GenericMinimum : e1:'T -> e2:'T -> 'T

        /// Take the maximum of two values structurally according to the order given by GenericComparison
        val inline GenericMaximum : e1:'T -> e2:'T -> 'T


        /// Reference/physical equality. 
        /// True if boxed versions of the inputs are reference-equal, OR if
        /// both are primitive numeric types and the implementation of Object.Equals for the type
        /// of the first argument returns true on the boxed versions of the inputs.  
        val inline PhysicalEquality : e1:'T -> e2:'T -> bool

        /// The physical hash.  Hashes on the object identity, except for value types,
        /// where we hash on the contents.
        val inline PhysicalHash : obj:'T -> int
        
        /// Return an F# comparer object suitable for hashing and equality. This hashing behaviour
        /// of the returned comparer is not limited by an overall node count when hashing F#
        /// records, lists and union types.
        val FSharpEqualityComparer : unit -> System.Collections.IEqualityComparer

        /// A static F# comparer object
        val FSharpComparer : unit -> System.Collections.IComparer

        /// Make an F# comparer object for the given type
        val FastGenericComparer<'T> : System.Collections.Generic.IComparer<'T>

        /// Make an F# hash/equality object for the given type
        val inline FastGenericEqualityComparer<'T> : System.Collections.Generic.IEqualityComparer<'T>

        /// Make an F# hash/equality object for the given type using node-limited hashing when hashing F#
        /// records, lists and union types.
        val inline FastLimitedGenericEqualityComparer<'T> : limit: int -> System.Collections.Generic.IEqualityComparer<'T>

        /// Hash a value according to its structure.  This hash is not limited by an overall node count when hashing F#
        /// records, lists and union types.
        val inline GenericHash : obj:'T -> int
        
        /// Hash a value according to its structure.  Use the given limit to restrict the hash when hashing F#
        /// records, lists and union types.
        val inline GenericLimitedHash : limit: int -> obj:'T -> int
        
        /// Recursively hash a part of a value according to its structure.  
        val inline GenericHashWithComparer : comparer : System.Collections.IEqualityComparer -> obj:'T -> int

        /// Generate a null value for reference types.
        val inline DefaultValue<'T when 'T : null> : 'T

        /// Build an enum value from an underlying value
        val inline EnumOfValue : value:'T -> 'Enum when 'Enum : enum<'T>

        /// Get the underlying value for an enum value
        val inline EnumToValue : enum:'Enum -> 'T when 'Enum : enum<'T>

        /// Parse an int32 according to the rules used by the overloaded 'int32' conversion operator when applied to strings
        val ParseInt32 : s:string -> int32

        /// Parse an uint32 according to the rules used by the overloaded 'uint32' conversion operator when applied to strings
        val ParseUInt32 : s:string -> uint32

        /// Parse an int64 according to the rules used by the overloaded 'int64' conversion operator when applied to strings
        val ParseInt64 : s:string -> int64

        /// Parse an uint64 according to the rules used by the overloaded 'uint64' conversion operator when applied to strings
        val ParseUInt64 : s:string -> uint64

        /// Resolves to the zero value for any primitive numeric type or any type with a static member called 'Zero'
        [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
        val GenericZeroDynamic : unit -> 'T 

        /// Resolves to the zero value for any primitive numeric type or any type with a static member called 'Zero'
        [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
        val GenericOneDynamic : unit -> 'T 

        /// A compiler intrinsic that implements dynamic invocations to the '+' operator
        [<Obsolete("This function is for use by dynamic invocations of F# code and should not be used directly")>]
        val AdditionDynamic : x:'T -> y:'U -> 'V

        /// A compiler intrinsic that implements dynamic invocations to the checked '+' operator
        [<Obsolete("This function is for use by dynamic invocations of F# code and should not be used directly")>]
        val CheckedAdditionDynamic : x:'T -> y:'U -> 'V

        /// A compiler intrinsic that implements dynamic invocations to the '+' operator
        [<Obsolete("This function is for use by dynamic invocations of F# code and should not be used directly")>]
        val MultiplyDynamic : x:'T -> y:'U -> 'V

        /// A compiler intrinsic that implements dynamic invocations to the checked '+' operator
        [<Obsolete("This function is for use by dynamic invocations of F# code and should not be used directly")>]
        val CheckedMultiplyDynamic : x:'T -> y:'U -> 'V

        /// A compiler intrinsic that implements dynamic invocations for the DivideByInt primitive
        [<Obsolete("This function is for use by dynamic invocations of F# code and should not be used directly")>]
        val DivideByIntDynamic : x:'T -> y:int -> 'T

        /// Resolves to the zero value for any primitive numeric type or any type with a static member called 'Zero'
        val inline GenericZero< ^T > : ^T when ^T : (static member Zero : ^T) 

        /// Resolves to the one value for any primitive numeric type or any type with a static member called 'One'
        val inline GenericOne< ^T > : ^T when ^T : (static member One : ^T) 

        /// Divide a floating point value by an integer
        val inline DivideByInt< ^T >  : x:^T -> y:int -> ^T when ^T : (static member DivideByInt : ^T * int -> ^T) 

        //-------------------------------------------------------------------------

        /// The F# compiler emits calls to some of the functions in this module as part of the compiled form of some language constructs
        module IntrinsicOperators = 

            /// Binary 'and'.  When used as a binary operator the right hand value is evaluated only on demand
            [<OCamlCompatibility("Consider using '&&' instead")>]
            val ( & ) : e1:bool -> e2:bool -> bool
            /// Binary 'and'.  When used as a binary operator the right hand value is evaluated only on demand
            val ( && ) : e1:bool -> e2:bool -> bool
            /// Binary 'or'.  When used as a binary operator the right hand value is evaluated only on demand
            //[<OCamlCompatibility("Use '||' instead")>]
            val ( or ) : e1:bool -> e2:bool -> bool
            /// Binary 'or'.  When used as a binary operator the right hand value is evaluated only on demand
            val ( || ) : e1:bool -> e2:bool -> bool
            /// Address-of. Uses of this value may result in the generation of unverifiable code.
            [<NoDynamicInvocation>]
            val inline ( ~& ) : obj:'T -> 'T byref
            /// Address-of. Uses of this value may result in the generation of unverifiable code.
            [<NoDynamicInvocation>]
            val inline ( ~&& ) : obj:'T -> nativeptr<'T>

        //-------------------------------------------------------------------------

        /// The F# compiler emits calls to some of the functions in this module as part of the compiled form of some language constructs
        module IntrinsicFunctions = 

            /// A compiler intrinsic that implements the ':?>' operator
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val UnboxGeneric<'T> : source:obj -> 'T

            /// A compiler intrinsic that implements the ':?>' operator
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline UnboxFast<'T> : source:obj -> 'T

            /// A compiler intrinsic that implements the ':?' operator
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val TypeTestGeneric<'T> : source:obj -> bool

            /// A compiler intrinsic that implements the ':?' operator
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline TypeTestFast<'T> : source:obj -> bool 

            /// Primitive used by pattern match compilation 
            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline GetString : source:string -> index:int -> char

            /// This function implements calls to default constructors
            /// acccessed by 'new' constraints.
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline CreateInstance : unit -> 'T when 'T : (new : unit -> 'T)

            /// This function implements parsing of decimal constants
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline MakeDecimal : low:int -> medium:int -> high:int -> isNegative:bool -> scale:byte -> decimal

            /// A compiler intrinsic for the efficeint compilation of sequence expressions
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val Dispose<'T when 'T :> System.IDisposable > : resource:'T -> unit

            /// The standard overloaded associative (indexed) lookup operator
            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline GetArray     : source:'T[] -> index:int -> 'T                           
            
            /// The standard overloaded associative (2-indexed) lookup operator
            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline GetArray2D    : source:'T[,] -> index1:int * index2:int -> 'T    
            
            /// The standard overloaded associative (3-indexed) lookup operator
            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline GetArray3D   : source:'T[,,] ->index1:int * index2:int * index3:int -> 'T  
            
            /// The standard overloaded associative (4-indexed) lookup operator
            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline GetArray4D   : source:'T[,,,] ->index1:int * index2:int * index3:int * index4:int -> 'T
            
            /// The standard overloaded associative (indexed) mutation operator
            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline SetArray   : target:'T[] -> index:int * value:'T -> unit      
            
            /// The standard overloaded associative (2-indexed) mutation operator
            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline SetArray2D  : target:'T[,] -> index1:int * index2:int * value:'T -> unit    
            
            /// The standard overloaded associative (3-indexed) mutation operator
            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline SetArray3D : target:'T[,,] -> index1:int * index2:int * index3:int * value:'T -> unit  

            /// The standard overloaded associative (4-indexed) mutation operator
            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val inline SetArray4D : target:'T[,,,] -> index1:int * index2:int * index3:int * index4:int * value:'T -> unit  

        /// The F# compiler emits calls to some of the functions in this module as part of the compiled form of some language constructs
        module HashCompare =
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val PhysicalHashIntrinsic : input:'T -> int
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val PhysicalEqualityIntrinsic : x:'T -> y:'T -> bool
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericHashIntrinsic : input:'T -> int
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val LimitedGenericHashIntrinsic : limit: int -> input:'T -> int
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericHashWithComparerIntrinsic : comp:System.Collections.IEqualityComparer -> input:'T -> int           
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericComparisonWithComparerIntrinsic : comp:System.Collections.IComparer -> x:'T -> y:'T -> int
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericComparisonIntrinsic : x:'T -> y:'T -> int
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericEqualityIntrinsic : x:'T -> y:'T -> bool
            // A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericEqualityWithComparerIntrinsic : comp:System.Collections.IEqualityComparer -> x:'T -> y:'T -> bool
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericLessThanIntrinsic : x:'T -> y:'T -> bool
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericGreaterThanIntrinsic : x:'T -> y:'T -> bool
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericGreaterOrEqualIntrinsic : x:'T -> y:'T -> bool
            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val GenericLessOrEqualIntrinsic : x:'T -> y:'T -> bool

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastHashTuple2 : comparer:System.Collections.IEqualityComparer -> tuple:('T1 * 'T2) -> int

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastHashTuple3 : comparer:System.Collections.IEqualityComparer -> tuple:('T1 * 'T2 * 'T3) -> int

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastHashTuple4 : comparer:System.Collections.IEqualityComparer -> tuple:('T1 * 'T2 * 'T3 * 'T4) -> int

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastHashTuple5 : comparer:System.Collections.IEqualityComparer -> tuple:('T1 * 'T2 * 'T3 * 'T4 * 'T5) -> int

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastEqualsTuple2 : comparer:System.Collections.IEqualityComparer -> tuple1:('T1 * 'T2) -> tuple2:('T1 * 'T2) -> bool

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastEqualsTuple3 : comparer:System.Collections.IEqualityComparer -> tuple1:('T1 * 'T2 * 'T3) -> tuple2:('T1 * 'T2 * 'T3) -> bool

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastEqualsTuple4 : comparer:System.Collections.IEqualityComparer -> tuple1:('T1 * 'T2 * 'T3 * 'T4) -> tuple2:('T1 * 'T2 * 'T3 * 'T4) -> bool

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastEqualsTuple5 : comparer:System.Collections.IEqualityComparer -> tuple1:('T1 * 'T2 * 'T3 * 'T4 * 'T5) -> tuple2:('T1 * 'T2 * 'T3 * 'T4 * 'T5) -> bool

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastCompareTuple2 : comparer:System.Collections.IComparer -> tuple1:('T1 * 'T2) -> tuple2:('T1 * 'T2) -> int

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastCompareTuple3 : comparer:System.Collections.IComparer -> tuple1:('T1 * 'T2 * 'T3) -> tuple2:('T1 * 'T2 * 'T3) -> int

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastCompareTuple4 : comparer:System.Collections.IComparer -> tuple1:('T1 * 'T2 * 'T3 * 'T4) -> tuple2:('T1 * 'T2 * 'T3 * 'T4) -> int

            /// A primitive entry point used by the F# compiler for optimization purposes. 
            [<Obsolete("This function is a primitive library routine used by optimized F# code and should not be used directly")>]
            val inline FastCompareTuple5 : comparer:System.Collections.IComparer -> tuple1:('T1 * 'T2 * 'T3 * 'T4 * 'T5) -> tuple2:('T1 * 'T2 * 'T3 * 'T4 * 'T5) -> int


#if FX_ATLEAST_40
#else
    //-------------------------------------------------------------------------
    // F# Tuple Types


    /// Compiled versions of F# tuple types.  These are not used directly, though
    /// these compiled forms are seen by other .NET languages.
    type Tuple<'T1> =
        interface IStructuralEquatable
        interface IStructuralComparable
        interface IComparable
        new : 'T1 -> Tuple<'T1>
        member Item1 : 'T1 with get
#if TUPLE_STRUXT
    [<Struct>]
    type Tuple<'T1,'T2> = 
        new : 'T1 * 'T2 -> Tuple<'T1,'T2> 
        val Item1 : 'T1 
        val Item2 : 'T2 //                
#else
    type Tuple<'T1,'T2> =  
        interface IStructuralEquatable
        interface IStructuralComparable
        interface IComparable
        new : 'T1 * 'T2 -> Tuple<'T1,'T2>
        member Item1 : 'T1 with get
        member Item2 : 'T2 with get
#endif

    type Tuple<'T1,'T2,'T3> = 
        interface IStructuralEquatable
        interface IStructuralComparable
        interface IComparable
        new : 'T1 * 'T2 * 'T3 -> Tuple<'T1,'T2,'T3>
        member Item1 : 'T1 with get
        member Item2 : 'T2 with get
        member Item3 : 'T3 with get

    type Tuple<'T1,'T2,'T3,'T4> = 
        interface IStructuralEquatable
        interface IStructuralComparable
        interface IComparable
        new : 'T1 * 'T2 * 'T3 * 'T4 -> Tuple<'T1,'T2,'T3,'T4>
        member Item1 : 'T1 with get
        member Item2 : 'T2 with get
        member Item3 : 'T3 with get
        member Item4 : 'T4 with get

    type Tuple<'T1,'T2,'T3,'T4,'T5> = 
        interface IStructuralEquatable
        interface IStructuralComparable
        interface IComparable
        new : 'T1 * 'T2 * 'T3 * 'T4 * 'T5 -> Tuple<'T1,'T2,'T3,'T4,'T5>
        member Item1 : 'T1 with get
        member Item2 : 'T2 with get
        member Item3 : 'T3 with get
        member Item4 : 'T4 with get
        member Item5 : 'T5 with get

    type Tuple<'T1,'T2,'T3,'T4,'T5,'T6> = 
        interface IStructuralEquatable
        interface IStructuralComparable
        interface IComparable
        new : 'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6-> Tuple<'T1,'T2,'T3,'T4,'T5,'T6>
        member Item1 : 'T1 with get
        member Item2 : 'T2 with get
        member Item3 : 'T3 with get
        member Item4 : 'T4 with get
        member Item5 : 'T5 with get
        member Item6 : 'T6 with get

    type Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7> = 
        interface IStructuralEquatable
        interface IStructuralComparable
        interface IComparable
        new : 'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6 * 'T7 -> Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7>
        member Item1 : 'T1 with get
        member Item2 : 'T2 with get
        member Item3 : 'T3 with get
        member Item4 : 'T4 with get
        member Item5 : 'T5 with get
        member Item6 : 'T6 with get
        member Item7 : 'T7 with get
        
    type Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'TRest> = 
        interface IStructuralEquatable
        interface IStructuralComparable
        interface IComparable
        new : 'T1 * 'T2 * 'T3 * 'T4 * 'T5 * 'T6 * 'T7 * 'TRest -> Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'TRest>
        member Item1 : 'T1 with get
        member Item2 : 'T2 with get
        member Item3 : 'T3 with get
        member Item4 : 'T4 with get
        member Item5 : 'T5 with get
        member Item6 : 'T6 with get
        member Item7 : 'T7 with get
        member Rest  : 'TRest with get
#endif

    //-------------------------------------------------------------------------
    // F# Choice Types


    /// Helper types for active patterns with 2 choices.
    [<DefaultAugmentation(false)>]
    //[<UnqualfiedLabels(false)>]
    type Choice<'T1,'T2> = 
      /// Choice 1 of 2 choices
      | Choice1Of2 of 'T1 
      /// Choice 2 of 2 choices
      | Choice2Of2 of 'T2
    
    /// Helper types for active patterns with 3 choices.
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3> = 
      /// Choice 1 of 3 choices
      | Choice1Of3 of 'T1 
      /// Choice 2 of 3 choices
      | Choice2Of3 of 'T2
      /// Choice 3 of 3 choices
      | Choice3Of3 of 'T3
    
    /// Helper types for active patterns with 4 choices.
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3,'T4> = 
      /// Choice 1 of 4 choices
      | Choice1Of4 of 'T1 
      /// Choice 2 of 4 choices
      | Choice2Of4 of 'T2
      /// Choice 3 of 4 choices
      | Choice3Of4 of 'T3
      /// Choice 4 of 4 choices
      | Choice4Of4 of 'T4
    
    /// Helper types for active patterns with 5 choices.
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3,'T4,'T5> = 
      /// Choice 1 of 5 choices
      | Choice1Of5 of 'T1 
      /// Choice 2 of 5 choices
      | Choice2Of5 of 'T2
      /// Choice 3 of 5 choices
      | Choice3Of5 of 'T3
      /// Choice 4 of 5 choices
      | Choice4Of5 of 'T4
      /// Choice 5 of 5 choices
      | Choice5Of5 of 'T5
    
    /// Helper types for active patterns with 6 choices.
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3,'T4,'T5,'T6> = 
      /// Choice 1 of 6 choices
      | Choice1Of6 of 'T1 
      /// Choice 2 of 6 choices
      | Choice2Of6 of 'T2
      /// Choice 3 of 6 choices
      | Choice3Of6 of 'T3
      /// Choice 4 of 6 choices
      | Choice4Of6 of 'T4
      /// Choice 5 of 6 choices
      | Choice5Of6 of 'T5
      /// Choice 6 of 6 choices
      | Choice6Of6 of 'T6
    
    /// Helper types for active patterns with 7 choices.
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3,'T4,'T5,'T6,'T7> = 
      /// Choice 1 of 7 choices
      | Choice1Of7 of 'T1 
      /// Choice 2 of 7 choices
      | Choice2Of7 of 'T2
      /// Choice 3 of 7 choices
      | Choice3Of7 of 'T3
      /// Choice 4 of 7 choices
      | Choice4Of7 of 'T4
      /// Choice 5 of 7 choices
      | Choice5Of7 of 'T5
      /// Choice 6 of 7 choices
      | Choice6Of7 of 'T6
      /// Choice 7 of 7 choices
      | Choice7Of7 of 'T7
    
    //-------------------------------------------------------------------------
    // F# Exception Types

    
    /// This exception is raised by 'failwith'
    exception Failure of string

    /// Non-exhaustive match failures will raise the MatchFailure exception
    exception MatchFailure of string * int * int
    /// Dynamic invocations of functions marked with the NoDynamicInvocationAttribute attribute raise this exception
    exception DynamicInvocationNotSupported of string 


    //-------------------------------------------------------------------------
    // F# Function Types


    /// The .NET type used to represent F# first-class type function values.  This type is for use
    /// by compiled F# code.
    [<AbstractClass>]
    type TypeFunc =
        /// Specialize the type function at a given type
        abstract Specialize<'T> : unit -> obj
        /// Construct an instance of an F# first class type function value 
        new : unit -> TypeFunc

    /// The .NET type used to represent F# function values.  This type is not
    /// typically used directly, though may be used from other .NET languages.
    [<AbstractClass>]
    type FastFunc<'T,'Result> = 
        /// Invoke an F# first class function value with one argument
        abstract member Invoke : func:'T -> 'Result
        [<OverloadID("ToConverter")>]
        /// Convert an F# first class function value to a value of type <c>System.Converter</c>
        static member op_Implicit : func:('T -> 'Result) -> System.Converter <'T,'Result>
        [<OverloadID("FromConverter")>]
        /// Convert an value of type <c>System.Converter</c> to a F# first class function value 
        static member op_Implicit : converter:System.Converter <'T,'Result> -> ('T -> 'Result)

        /// Convert an F# first class function value to a value of type <c>System.Converter</c>
        static member ToConverter : func:('T -> 'Result) -> System.Converter <'T,'Result>
        /// Convert an value of type <c>System.Converter</c> to a F# first class function value 
        static member FromConverter : converter:System.Converter <'T,'Result> -> ('T -> 'Result)

        /// Invoke an F# first class function value with five curried arguments. In some cases this
        /// will result in a more efficient application than applying the arguments successively.
        static member InvokeFast : func:FastFunc <'T,('Result -> 'V -> 'W -> 'X -> 'Y)> * arg1:'T * arg2:'Result * arg3:'V * arg4:'W * arg5:'X -> 'Y
        /// Invoke an F# first class function value with four curried arguments. In some cases this
        /// will result in a more efficient application than applying the arguments successively.
        static member InvokeFast : func:FastFunc <'T,('Result -> 'V -> 'W -> 'X)> * arg1:'T * arg2:'Result * arg3:'V * arg4:'W -> 'X
        /// Invoke an F# first class function value with three curried arguments. In some cases this
        /// will result in a more efficient application than applying the arguments successively.
        static member InvokeFast : func:FastFunc <'T,('Result -> 'V -> 'W)> * arg1:'T * arg2:'Result * arg3:'V -> 'W
        /// Invoke an F# first class function value with two curried arguments. In some cases this
        /// will result in a more efficient application than applying the arguments successively.
        static member InvokeFast : func:FastFunc <'T,('Result -> 'V)> * arg1:'T * arg2:'Result -> 'V

        /// Construct an instance of an F# first class function value 
        new : unit -> FastFunc <'T,'Result>

    [<AbstractClass>]
    [<Sealed>]
    /// Helper functions for converting F# first class function values to and from .NET representaions
    /// of functions using delegates.
    type FuncConvert = 
        [<OverloadID("Action1")>]
        /// Convert the given Action delegate object to an F# function value
        static member  ToFastFunc       : action:Action<'T>            -> ('T -> unit)
        
        [<OverloadID("Converter1")>]
        /// Convert the given Converter delegate object to an F# function value
        static member  ToFastFunc       : converter:Converter<'T,'U>          -> ('T -> 'U)
        
        [<OverloadID("FuncFromTupled1")>]
        /// A utility funcion to convert function values from tupled to curried form
        static member FuncFromTupled : func:('T -> 'U) -> ('T -> 'U)
        
        [<OverloadID("FuncFromTupled2")>]
        /// A utility funcion to convert function values from tupled to curried form
        static member FuncFromTupled : func:('T1 * 'T2 -> 'U) -> ('T1 -> 'T2 -> 'U)
        
        [<OverloadID("FuncFromTupled3")>]
        /// A utility funcion to convert function values from tupled to curried form
        static member FuncFromTupled : func:('T1 * 'T2 * 'T3 -> 'U) -> ('T1 -> 'T2 -> 'T3 -> 'U)
        
        [<OverloadID("FuncFromTupled4")>]
        /// A utility funcion to convert function values from tupled to curried form
        static member FuncFromTupled : func:('T1 * 'T2 * 'T3 * 'T4 -> 'U) -> ('T1 -> 'T2 -> 'T3 -> 'T4 -> 'U)
        
        [<OverloadID("FuncFromTupled5")>]
        /// A utility funcion to convert function values from tupled to curried form
        static member FuncFromTupled : func:('T1 * 'T2 * 'T3 * 'T4 * 'T5 -> 'U) -> ('T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'U)

    /// An implementation module used to hold some private implementations of function
    /// value invocation.
    module OptimizedClosures =

        /// The .NET type used to represent F# function values that accept
        /// two iterated (curried) arguments without intervening execution.  This type should not
        /// typically used directly from either F# code or from other .NET languages.
        [<AbstractClass>]
        type FastFunc2<'T1,'T2,'Result> = 
            inherit FastFunc <'T1,('T2 -> 'Result)>
            /// Invoke the optimized function value with two curried arguments 
            abstract member Invoke : arg1:'T1 * arg2:'T2 -> 'Result
            /// Adapt an F# first class function value to be an optimized function value that can 
            /// accept two curried arguments without intervening execution. 
            static member Adapt : func:('T1 -> 'T2 -> 'Result) -> FastFunc2<'T1,'T2,'Result>
            /// Construct an optimized function value that can accept two curried 
            /// arguments without intervening execution.
            new : unit -> FastFunc2 <'T1,'T2,'Result>

        /// The .NET type used to represent F# function values that accept
        /// three iterated (curried) arguments without intervening execution.  This type should not
        /// typically used directly from either F# code or from other .NET languages.
        [<AbstractClass>]
        type FastFunc3<'T1,'T2,'T3,'Result> = 
            inherit FastFunc <'T1,('T2 -> 'T3 -> 'Result)>
            /// Invoke an F# first class function value that accepts three curried arguments 
            /// without intervening execution
            abstract member Invoke : arg1:'T1 * arg2:'T2 * arg3:'T3 -> 'Result
            /// Adapt an F# first class function value to be an optimized function value that can 
            /// accept three curried arguments without intervening execution. 
            static member Adapt : func:('T1 -> 'T2 -> 'T3 -> 'Result) -> FastFunc3<'T1,'T2,'T3,'Result>
            /// Construct an optimized function value that can accept three curried 
            /// arguments without intervening execution.
            new : unit -> FastFunc3 <'T1,'T2,'T3,'Result>

        /// The .NET type used to represent F# function values that accept
        /// four iterated (curried) arguments without intervening execution.  This type should not
        /// typically used directly from either F# code or from other .NET languages.
        [<AbstractClass>]
        type FastFunc4<'T1,'T2,'T3,'T4,'Result> = 
            inherit FastFunc <'T1,('T2 -> 'T3 -> 'T4 -> 'Result)>
            /// Invoke an F# first class function value that accepts four curried arguments 
            /// without intervening execution
            abstract member Invoke : arg1:'T1 * arg2:'T2 * arg3:'T3 * arg4:'T4 -> 'Result
            /// Adapt an F# first class function value to be an optimized function value that can 
            /// accept four curried arguments without intervening execution. 
            static member Adapt : func:('T1 -> 'T2 -> 'T3 -> 'T4 -> 'Result) -> FastFunc4<'T1,'T2,'T3,'T4,'Result>
            /// Construct an optimized function value that can accept four curried 
            /// arguments without intervening execution.
            new : unit -> FastFunc4 <'T1,'T2,'T3,'T4,'Result>

        /// The .NET type used to represent F# function values that accept
        /// five iterated (curried) arguments without intervening execution.  This type should not
        /// typically used directly from either F# code or from other .NET languages.
        [<AbstractClass>]
        type FastFunc5<'T1,'T2,'T3,'T4,'T5,'Result> = 
            inherit FastFunc <'T1,('T2 -> 'T3 -> 'T4 -> 'T5 -> 'Result)>
            /// Invoke an F# first class function value that accepts five curried arguments 
            /// without intervening execution
            abstract member Invoke : arg1:'T1 * arg2:'T2 * arg3:'T3 * arg4:'T4 * arg5:'T5 -> 'Result
            /// Adapt an F# first class function value to be an optimized function value that can 
            /// accept five curried arguments without intervening execution. 
            static member Adapt : func:('T1 -> 'T2 -> 'T3 -> 'T4 -> 'T5 -> 'Result) -> FastFunc5<'T1,'T2,'T3,'T4,'T5,'Result>
            /// Construct an optimized function value that can accept five curried 
            /// arguments without intervening execution.
            new : unit -> FastFunc5 <'T1,'T2,'T3,'T4,'T5,'Result>


    //-------------------------------------------------------------------------
    // F# Mutable Reference Cells


    /// The type of mutable references.  Use the functions [:=] and [!] to get and
    /// set values of this type.
    type Ref <'T> = 
        {  /// The current value of the reference cell
           [<OCamlCompatibility("Consider using 'Value' instead")>]
           mutable contents: 'T;}
        /// The current value of the reference cell
        member Value : 'T with get,set
            
    /// The type of mutable references.  Use the functions [:=] and [!] to get and
    /// set values of this type.
    and 'T ref = Ref<'T>

    //-------------------------------------------------------------------------
    // F# Option Types


    /// The type of optional values.  When used from other .NET languages the
    /// empty option is the <c>null</c> value.  
    ///
    /// Use the constructors <c>Some</c> and <c>None</c> to create values of this type.
    /// Use the values in the <c>Option</c> module to manipulate values of this type,
    /// or pattern match against the values directly.
    ///
    /// <c>None</c> values will appear as the value <c>null</c> to other .NET languages.
    /// Instance methods on this type will appear as static methods to other .NET languages
    /// due to the use of <c>null</c> as a value representation.
    [<DefaultAugmentation(false)>]
    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    type Option <'T> =
        /// The representation of "No value"
        | None :       'T option
        /// The representation of "Value of type 'T"
        | Some : value:'T -> 'T option 
        /// Create an option value that is a 'None' value.
        static member None : 'T option
        /// Create an option value that is a 'Some' value.
        static member Some : value:'T -> 'T option
        [<CompilationRepresentation(CompilationRepresentationFlags.Instance)>]
        /// Get the value of a 'Some' option. A NullReferenceException is raised if the option is 'None'.
        member Value : 'T
        /// Return 'true' if the option is a 'Some' value.
        member IsSome : bool
        /// Return 'true' if the option is a 'None' value.
        member IsNone : bool
  
    /// The type of optional values.  When used from other .NET languages the
    /// empty option is the <c>null</c> value.  
    ///
    /// Use the constructors <c>Some</c> and <c>None</c> to create values of this type.
    /// Use the values in the <c>Option</c> module to manipulate values of this type,
    /// or pattern match against the values directly.
    ///
    /// 'None' values will appear as the value <c>null</c> to other .NET languages.
    /// Instance methods on this type will appear as static methods to other .NET languages
    /// due to the use of <c>null</c> as a value representation.
    and 'T option = Option<'T>


namespace Microsoft.FSharp.Collections

    open System
    open System.Collections.Generic
    open Microsoft.FSharp.Core

    /// The type of immutable singly-linked lists.  
    ///
    /// Use the constructors <c>[]</c> and <c>::</c> (infix) to create values of this type, or
    /// the notation <c>[1;2;3]</c>.   Use the values in the <c>List</c> module to manipulate 
    /// values of this type, or pattern match against the values directly.
    type FSharpList<'T> =
        | ([])  :                 'T list
        | (::)  : _Head: 'T * _Tail: 'T list -> 'T list
        /// Returns an empty list of a particular type
        static member Empty : 'T list
        
        /// Gets the number of items contained in the list
        member Length : int
        /// Gets a value indicating if the list contains no entries
        member IsEmpty : bool

        /// Gets the first element of the list
        member Head : 'T

        /// Gets the tail of the list, which is a list containing all the elements of the list, excluding the first element 
        member Tail : 'T list

        /// Get the element of the list at the given position. Note lists are represented
        /// as linked lists so this is an O(n) operation.
        member Item : index:int -> 'T with get 
        
        /// Returns a list with <c>head</c> as its first element and <c>tail</c> as its subsequent elements
        static member Cons : head:'T * tail:'T list -> 'T list
        
        interface System.Collections.Generic.IEnumerable<'T>
        interface System.Collections.IEnumerable
        
    /// An abbreviation for the type of immutable singly-linked lists.  
    ///
    /// Use the constructors <c>[]</c> and <c>::</c> (infix) to create values of this type, or
    /// the notation <c>[1;2;3]</c>.   Use the values in the <c>List</c> module to manipulate 
    /// values of this type, or pattern match against the values directly.
    and 'T list = FSharpList<'T>

    /// An abbreviation for the type of immutable singly-linked lists.  
    and List<'T> = FSharpList<'T>

    /// An abbreviation for the .NET type <c>System.Collections.Generic.List&lt;_&gt;</c>
    type ResizeArray<'T> = System.Collections.Generic.List<'T>

    /// An abbreviation for the .NET type <c>System.Collections.Generic.IEnumerable&lt;_&gt;</c>
    type seq<'T> = IEnumerable<'T>



namespace Microsoft.FSharp.Core

    open System
    open System.Collections.Generic
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Collections


    /// Basic F# Operators. This module is automatically opened in all F# code.
    module Operators = 

        // Arithmetic operators.  These operators are overloaded and can be used
        // on any pair of types that satisfies the constraint, e.g. the
        // '+' function can be used on any type that supports the "op_Addition" 
        // constraint.  This includes all .NET types that support the op_Addition 
        // operator.  The standard integral and floating point types support 
        // constraints as follows:
        //   - The built-in integral types are:
        //          sbyte, byte, int16, uint16, int32, unit32, 
        //          int64, uint64, nativeint, unativeint
        //
        //   - The built-in floating point types are:
        //          float, float32
        //
        //   - The built-in numeric types are these combined
        //
        //    All built-in numeric types support:
        //        'ty.(+)   : (ty,ty) -> ty
        //        'ty.(-)   : (ty,ty) -> ty
        //        'ty.( * ) : (ty,ty) -> ty
        //        'ty.(/)   : (ty,ty) -> ty
        //        'ty.(%)   : (ty,ty) -> ty
        //        'ty.(~+)  : (ty)    -> ty
        //
        //    All signed numeric types support:
        //        'ty.(~-)  : (ty)    -> ty
        
        
        /// Overloaded unary negation.
        val inline ( ~- ) : n:^T -> ^T when ^T : (static member ( ~- ) : ^T -> ^T) and default ^T : int

        /// Overloaded addition operator
        val inline ( + ) : x:^T -> y:^U -> ^V  when (^T or ^U) : (static member ( + ) : ^T * ^U    -> ^V) and default ^U : ^V and default ^V : ^T and default ^V : ^U and default ^T : ^V and default ^T : ^U and default ^T : int
        
        /// Overloaded subtraction operator
        val inline ( - ) : x:^T -> y:^U -> ^V  when (^T or ^U) : (static member ( - ) : ^T * ^U    -> ^V) and default ^U : ^V and default ^V : ^T and default ^V : ^U and default ^T : ^V and default ^T : ^U and default ^T : int
        
        /// Overloaded multiplication operator
        val inline ( * ) : x:^T -> y:^U -> ^V  when (^T or ^U) : (static member ( * ) : ^T * ^U    -> ^V) and default ^U : ^V and default ^V : ^T and default ^V : ^U and default ^T : ^V and default ^T : ^U and default ^T : int
        
        /// Overloaded division operator
        val inline ( / ) : x:^T -> y:^U -> ^V  when (^T or ^U) : (static member ( / ) : ^T * ^U    -> ^V) and default ^U : ^V and default ^V : ^T and default ^V : ^U and default ^T : ^V and default ^T : ^U and default ^T : int
        
        /// Overloaded modulo operator
        val inline ( % ) : x:^T -> y:^U -> ^V    when (^T or ^U) : (static member ( % ) : ^T * ^U    -> ^V) and default ^U : ^V and default ^V : ^T and default ^V : ^U and default ^T : ^V and default ^T : ^U and default ^T : int
        
        /// Overloaded logical-AND operator
        val inline (&&&): x:^T -> y:^T -> ^T     when ^T : (static member (&&&) : ^T * ^T    -> ^T) and default ^T : int
        
        /// Overloaded logical-OR operator
        val inline (|||) : x:^T -> y:^T -> ^T    when ^T : (static member (|||) : ^T * ^T    -> ^T) and default ^T : int
        
        /// Overloaded logical-XOR operator
        val inline (^^^) : x:^T -> y:^T -> ^T    when ^T : (static member (^^^) : ^T * ^T    -> ^T) and default ^T : int
        
        /// Overloaded byte-shift left operator by a specified number of bits
        val inline (<<<) : value:^T -> shift:int32 -> ^T when ^T : (static member (<<<) : ^T * int32 -> ^T) and default ^T : int
        
        /// Overloaded byte-shift right operator by a specified number of bits
        val inline (>>>) : value:^T -> shift:int32 -> ^T when ^T : (static member (>>>) : ^T * int32 -> ^T) and default ^T : int
        
        /// Overloaded logical-NOT operator
        val inline (~~~)  : value:^T -> ^T         when ^T : (static member (~~~) : ^T         -> ^T) and default ^T : int
        
        /// Overloaded prefix=plus operator
        val inline (~+) : value:^T -> ^T           when ^T : (static member (~+)  : ^T         -> ^T) and default ^T : int
        
        /// Structural less-than comparison
        val inline ( < ) : x:'T -> y:'T -> bool
        
        /// Structural greater-than
        val inline ( > ) : x:'T -> y:'T -> bool
        
        ///Structural greater-than-or-equal
        val inline ( >= ) : x:'T -> y:'T -> bool
        
        ///Structural less-than-or-equal comparison
        val inline ( <= ) : x:'T -> y:'T -> bool
        
        ///Structural equality
        val inline ( = ) : x:'T -> y:'T -> bool
        
        ///Structural inequality
        val inline ( <> ) : x:'T -> y:'T -> bool

        /// Compose two functions, the function on the left being applied first
        val inline (>>): func1:('T -> 'U) -> func2:('U -> 'V) -> ('T -> 'V) 
        
        /// Compose two functions, the function on the right being applied first
        val inline (<<): func2:('U -> 'V) -> func1:('T -> 'U) -> ('T -> 'V) 
        
        /// Apply a function to a value, the value being on the left, the function on the right
        val inline (|>): arg:'T -> func:('T -> 'U) -> 'U
        /// Apply a function to two values, the values being a pair on the left, the function on the right
        val inline (||>): arg1:'T1 * arg2:'T2 -> func:('T1 -> 'T2 -> 'U) -> 'U
        /// Apply a function to three values, the values being a triple on the left, the function on the right
        val inline (|||>): arg1:'T1 * arg2:'T2 * arg3:'T3 -> func:('T1 -> 'T2 -> 'T3 -> 'U) -> 'U
        
        /// Apply a function to a value, the value being on the right, the function on the left
        val inline (<|): func:('T -> 'U) -> arg1:'T -> 'U

        /// Apply a function to two values, the values being a pair on the right, the function on the left
        val inline (<||): func:('T1 -> 'T2 -> 'U) -> arg1:'T1 * arg2:'T2 -> 'U

        /// Apply a function to three values, the values being a triple on the right, the function on the left
        val inline (<|||): func:('T1 -> 'T2 -> 'T3 -> 'U) -> arg1:'T1 * arg2:'T2 * arg3:'T3 -> 'U

        /// Used to specify a default value for an optional argument in the implementation of a function
        val defaultArg : arg:'T option -> defaultValue:'T -> 'T 

        /// Concatenate two strings.  The overlaoded operator '+' may also be used.
        val (^): s1:string -> s2:string -> string

        /// Raises an exception
        val raise : exn:System.Exception -> 'T
        
        /// Rethrows an exception. This should only be used when handling an exception
        [<NoDynamicInvocation>]
        val inline rethrow : unit -> 'T

        /// Return the first element of a tuple, <c>fst (a,b) = a</c>.
        val fst : tuple:('T1 * 'T2) -> 'T1
        
        /// Return the second element of a tuple, <c>snd (a,b) = b</c>.
        val snd : tuple:('T1 * 'T2) -> 'T2

        /// Generic comparison
        val inline compare: e1:'T -> e2:'T -> int
        /// Maximum based on generic comparison
        val inline max : e1:'T -> e2:'T -> 'T
        /// Minimum based on generic comparison
        val inline min : e1:'T -> e2:'T -> 'T

        /// Ignore the passed value. This is often used to throw away results of a computation.
        val ignore : value:'T -> unit

        /// Unboxes a strongly typed value. This is the inverse of <c>box</c>, unbox&lt;t&gt;(box&lt;t&gt; a) equals a.
        val inline unbox : value:obj -> 'T
        /// Boxes a strongly typed value.
        val inline box : value:'T -> obj

        /// Throw a <c>FailureException</c> exception
        val failwith : message:string -> 'T 

        /// Throw an <c>ArgumentException</c> exception
        val invalidArg : argumentName:string -> message:string -> 'T 

        /// Throw an <c>ArgumentNullException</c> exception
        val nullArg : argumentName:string -> 'T 

        /// Throw an <c>InvalidOperationException</c> exception
        val invalidOp : message:string -> 'T 

        /// The identity function
        val id : x:'T -> 'T 

        /// Create a mutable reference cell
        val ref : value:'T -> 'T ref

        /// Assign to a mutable reference cell
        val ( := ) : cell:'T ref -> value:'T -> unit

        /// Dereference a mutable reference cell
        val ( ! ) : cell:'T ref -> 'T

        /// Decrement a mutable reference cell containing an integer
        val decr: cell:int ref -> unit

        /// Increment a mutable reference cell containing an integer
        val incr: cell:int ref -> unit

        /// Concatenate two lists.
        val (@): list1:'T list -> list2:'T list -> 'T list

        /// Negate a logical value. <c>not true</c> equals <c>false</c> and <c>not false</c> equals <c>true</c>
        val inline not : value:bool -> bool

#if FX_NO_EXIT
#else
        /// Exit the current hardware isolated process, if security settings permit,
        /// otherwise raise an exception.  Calls <c>System.Environment.Exit</c>.
        val exit: exitcode:int -> 'T   when default 'T : obj
#endif

        /// Equivalent to <c>System.Double.PositiveInfinity</c>
        val infinity: float

        /// Equivalent to <c>System.Double.NaN</c>
        val nan: float

        /// Equivalent to <c>System.Single.PositiveInfinity</c>
        val infinityf: float32

        /// Equivalent to <c>System.Single.NaN</c>
        val nanf: float32

        /// Reads the value of the property <c>System.Console.In</c>. 
        val stdin<'T> : System.IO.TextReader      

        /// Reads the value of the property <c>System.Console.Error</c>. 
        val stderr<'T> : System.IO.TextWriter

        /// Reads the value of the property <c>System.Console.Out</c>.
        val stdout<'T> : System.IO.TextWriter

        /// The standard overloaded range operator, e.g. <c>[n..m]</c> for lists, <c>seq {n..m}</c> for sequences
        val inline (..)    : start:^T       -> finish:^T -> seq< ^T >    
                                when ^T : (static member (+)   : ^T * ^T -> ^T) 
                                and ^T : (static member One  : ^T) 
                                and default ^T : int
        
        /// The standard overloaded skip range operator, e.g. <c>[n..skip..m]</c> for lists, <c>seq {n..skip..m}</c> for sequences
        val inline (.. ..) : start:^T -> step:^U -> finish:^T -> seq< ^T >    
                                when (^T or ^U) : (static member (+)   : ^T * ^U -> ^T) 
                                and ^U : (static member Zero : ^U) 
                                and default ^U : ^T
                                and default ^T : int
        
        /// Execute the function as a mutual-exlcusion region using the input value as a lock. 
        val inline lock: lockObject:'lock -> action:(unit -> 'T) -> 'T when 'lock : not struct

        /// Clean up resources associated with the input object after the completion of the given function.
        /// Cleanup occurs even when an exception is raised by the protected
        /// code. 
        val using: resource:('T :> System.IDisposable) -> action:('T -> 'U) -> 'U


        /// Generate a System.Type runtime represenation of a static type.
        /// The static type is still maintained on the value returned.
        [<RequiresExplicitTypeArguments>] 
        val inline typeof<'T> : System.Type

        /// Generate a System.Type representation for a type definition. If the
        /// input type is a generic type instantiation then return the 
        /// generic type definition associated with all such instantiations.
        [<RequiresExplicitTypeArguments>] 
        val inline typedefof<'T> : System.Type

        /// Returns the internal size of a type in bytes. For example, <c>sizeof&lt;int&gt;</c> returns 4.
        [<RequiresExplicitTypeArguments>] 
        val inline sizeof<'T> : int
        
        /// A generic hash function, designed to return equal hash values for items that are 
        /// equal according to the "=" operator. By default it will use structural hashing
        /// for F# union, record and tuple types, hashing the complete contents of the 
        /// type. The exact behaviour of the function can be adjusted on a 
        /// type-by-type basis by implementing GetHashCode for each type.
        val inline hash: obj:'T -> int

        /// A generic hash function. This function has the same behaviour as 'hash', 
        /// however the default structural hashing for F# union, record and tuple 
        /// types stops when the given limit of nodes is reached. The exact behaviour of 
        /// the function can be adjusted on a type-by-type basis by implementing 
        /// GetHashCode for each type.
        val inline limitedHash: limit: int -> obj:'T -> int


        /// Absolute value of the given number
        val inline abs      : value:^T -> ^T       when ^T : (static member Abs      : ^T -> ^T)      and default ^T : int
        
        /// Inverse cosine of the given number
        val inline acos     : value:^T -> ^T       when ^T : (static member Acos     : ^T -> ^T)      and default ^T : float
        
        /// Inverse sine of the given number
        val inline asin     : value:^T -> ^T       when ^T : (static member Asin     : ^T -> ^T)      and default ^T : float
        
        /// Inverse tangent of the given number
        val inline atan     : value:^T -> ^T       when ^T : (static member Atan     : ^T -> ^T)      and default ^T : float
        
        /// Inverse tangent of <c>x/y</c> where <c>x</c> and <c>y</c> are specified separately
        val inline atan2    : y:^T -> x:^T -> 'U when ^T : (static member Atan2    : ^T * ^T -> 'U) and default ^T : float
        
        /// Ceiling of the given number
        val inline ceil     : value:^T -> ^T       when ^T : (static member Ceiling  : ^T -> ^T)      and default ^T : float
        
        /// Exponential of the given number
        val inline exp      : value:^T -> ^T       when ^T : (static member Exp      : ^T -> ^T)      and default ^T : float

        /// Floor of the given number
        val inline floor    : value:^T -> ^T       when ^T : (static member Floor    : ^T -> ^T)      and default ^T : float

        /// Sign of the given number
        val inline sign     : value:^T -> int      when ^T : (member Sign    : int)      and default ^T : float

        /// Round the given number
        val inline round    : value:^T -> ^T       when ^T : (static member Round    : ^T -> ^T)      and default ^T : float

        /// Natural logarithm of the given number
        val inline log      : value:^T -> ^T       when ^T : (static member Log      : ^T -> ^T)      and default ^T : float

        /// Logarithm to base 10 of the given number
        val inline log10    : value:^T -> ^T       when ^T : (static member Log10    : ^T -> ^T)      and default ^T : float

        /// Square root of the given number
        val inline sqrt     : value:^T -> ^U       when ^T : (static member Sqrt     : ^T -> ^U)      and default ^U : ^T and default ^T : ^U and default ^T : float 

        /// Cosine of the given number
        val inline cos      : value:^T -> ^T       when ^T : (static member Cos      : ^T -> ^T)      and default ^T : float

        /// Hyperbolic cosine  of the given number
        val inline cosh     : value:^T -> ^T       when ^T : (static member Cosh     : ^T -> ^T)      and default ^T : float
        
        /// Sine of the given number
        val inline sin      : value:^T -> ^T       when ^T : (static member Sin      : ^T -> ^T)      and default ^T : float
        
        /// Hyperbolic sine of the given number
        val inline sinh     : value:^T -> ^T       when ^T : (static member Sinh     : ^T -> ^T)      and default ^T : float
        
        /// Tangent of the given number
        val inline tan      : value:^T -> ^T       when ^T : (static member Tan      : ^T -> ^T)      and default ^T : float
        
        /// Hyperbolic tangent of the given number
        val inline tanh     : value:^T -> ^T       when ^T : (static member Tanh     : ^T -> ^T)      and default ^T : float

#if FX_NO_TRUNCATE
#else
        /// Overloaded truncate operator.
        val inline truncate : value:^T -> ^T       when ^T : (static member Truncate : ^T -> ^T)      and default ^T : float
#endif

        /// Overloaded power operator.
        val inline ( **  )  : x:^T -> y:^T -> ^T when ^T : (static member Pow : ^T * ^T -> ^T) and default ^T : float

        /// Overloaded power operator. If <c>n > 0</c> then equivalent to <c>x*...*x</c> for <c>n</c> occurrences of <c>x</c>. 
        val inline pown  : x:^T -> n:int -> ^T when ^T : (static member One : ^T) 
                                               and  ^T : (static member ( * ) : ^T * ^T -> ^T) 
                                               and  ^T : (static member ( / ) : ^T * ^T -> ^T) 
                                               and default ^T : int

        /// Converts the argument to byte. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>Byte.Parse()</c> on strings and otherwise requires a <c>ToByte</c> method on the input type
        val inline byte       : value:^T -> byte       when ^T : (static member ToByte    : ^T -> byte)       and default ^T : int        
        
        /// Converts the argument to signed byte. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>SByte.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToSByte</c> method on the input type
        val inline sbyte      : value:^T -> sbyte      when ^T : (static member ToSByte   : ^T -> sbyte)      and default ^T : int
        
        /// Converts the argument to signed 16-bit integer. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>Int16.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToInt16</c> method on the input type
        val inline int16      : value:^T -> int16      when ^T : (static member ToInt16   : ^T -> int16)      and default ^T : int
        
        /// Converts the argument to unsigned 16-bit integer. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>UInt16.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToUInt16</c> method on the input type
        val inline uint16     : value:^T -> uint16     when ^T : (static member ToUInt16  : ^T -> uint16)     and default ^T : int
        
        /// Converts the argument to signed 32-bit integer. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>Int32.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToInt32</c> method on the input type
        val inline int        : value:^T -> int        when ^T : (static member ToInt32   : ^T -> int)        and default ^T : int
        
        /// Converts the argument to a particular enum type.
        val inline enum       : value:int32 -> ^U        when ^U : enum<int32> 

        /// Converts the argument to signed 32-bit integer. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>Int32.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToInt32</c> method on the input type
        val inline int32      : value:^T -> int32      when ^T : (static member ToInt32   : ^T -> int32)      and default ^T : int

        /// Converts the argument to unsigned 32-bit integer. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>UInt32.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToUInt32</c> method on the input type
        val inline uint32     : value:^T -> uint32     when ^T : (static member ToUInt32  : ^T -> uint32)     and default ^T : int

        /// Converts the argument to signed 64-bit integer. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>Int64.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToInt64</c> method on the input type
        val inline int64      : value:^T -> int64      when ^T : (static member ToInt64   : ^T -> int64)      and default ^T : int

        /// Converts the argument to unsigned 64-bit integer. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>UInt64.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToUInt64</c> method on the input type
        val inline uint64     : value:^T -> uint64     when ^T : (static member ToUInt64  : ^T -> uint64)     and default ^T : int

        /// Converts the argument to 32-bit float. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>Single.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToSingle</c> method on the input type
        val inline float32    : value:^T -> float32    when ^T : (static member ToSingle  : ^T -> float32)    and default ^T : int

        /// Converts the argument to 64-bit float. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>Double.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToDouble</c> method on the input type
        val inline float      : value:^T -> float      when ^T : (static member ToDouble  : ^T -> float)      and default ^T : int

        /// Converts the argument to 32-bit float. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>Single.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToSingle</c> method on the input type
        val inline single     : value:^T -> single     when ^T : (static member ToSingle  : ^T -> single)     and default ^T : int

        /// Converts the argument to 64-bit float. This is a direct conversion for all 
        /// primitive numeric types. For strings, the input is converted using <c>Double.Parse()</c>  with InvariantCulture settings. Otherwise the operation requires and invokes a <c>ToDouble</c> method on the input type
        val inline double     : value:^T -> float      when ^T : (static member ToDouble  : ^T -> double)     and default ^T : int

        /// Converts the argument to signed native integer. This is a direct conversion for all 
        /// primitive numeric types and <c>ToIntPtr</c> method otherwise)
        val inline nativeint  : value:^T -> nativeint  when ^T : (static member ToIntPtr  : ^T -> nativeint)  and default ^T : int

        /// Converts the argument to unsigned native integer using a direct conversion for all 
        /// primitive numeric types and requiring a <c>ToUintPtr</c> method otherwise
        val inline unativeint : value:^T -> unativeint when ^T : (static member ToUIntPtr : ^T -> unativeint) and default ^T : int
        
        /// Converts the argument to a string using <c>ToString</c>.
        /// For standard integer and floating point values the <c>ToString</c> conversion uses <c>CultureInfo.InvariantCulture</c>.
        /// Note, native integer <c>ToString</c> does not support specifying <c>CultureInfo</c>.
        val inline string  : value:^T -> string

        /// Converts the argument to System.Decimal using a direct conversion for all 
        /// primitive numeric types and requiring a <c>ToDecimal</c> method otherwise
        val inline decimal : value:^T -> decimal when ^T : (static member ToDecimal : ^T -> decimal) and default ^T : int

        /// Converts the argument to character. Numeric inputs are converted according to the UTF-16 
        /// encoding for characters. String inputs must be exactly one character long.
        /// For other types a static member ToChar must exist on the type.
        val inline char        : value:^T -> char      when ^T : (static member ToChar   : ^T -> char)        and default ^T : int

        /// An active pattern to match values of type <c>System.Collections.Generic.KeyValuePair</c>
        val ( |KeyValue| ): keyValuePair:KeyValuePair<'T,'U> -> 'T * 'U

#if DONT_INCLUDE_DEPRECATED
#else
        [<OCamlCompatibility("Consider using 'invalidArg' instead")>]
        val invalid_arg : message:string -> 'T 

        [<Obsolete("This function has been renamed to 'invalidOp'")>]
        val invalid_op : message:string -> 'T 

        /// Throw an <c>KeyNotFoundException</c> exception
        [<OCamlCompatibility("Consider using 'raise (KeyNotFoundException(message))' instead")>]
        val not_found : unit -> 'T 

        [<Obsolete("The F# exception type 'InvalidArgument' is now identical to 'System.ArgumentException'")>]
        val InvalidArgument : string -> exn
        
        [<Obsolete("The F# exception type 'InvalidArgument' is now identical to 'System.ArgumentException'")>]
        val (|InvalidArgument|_|) : exn -> string option
        
        [<Obsolete("The F# exception type 'InvalidArgument' is now identical to 'System.ArgumentException'")>]
        type InvalidArgumentException = ArgumentException
#endif


        /// A module of compiler intrinsic functions for efficient implementations of F# integer ranges
        /// and dynamic invocations of other F# operators
        module OperatorIntrinsics =

            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            /// Get a slice of an array
            val inline GetArraySlice : source:'T[] -> start:int option * finish:int option -> 'T[] 

            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            /// Set a slice of an array
            val inline SetArraySlice : target:'T[] -> start:int option * finish:int option * source:'T[] -> unit

            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            /// Get a slice of an array
            val GetArraySlice2D : source:'T[,] -> start1:int option * finish1:int option * start2:int option * finish2:int option -> 'T[,]

            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            /// Set a slice of an array
            val SetArraySlice2D : target:'T[,] -> start1:int option * finish1:int option * start2:int option * finish2:int option * source:'T[,] -> unit

            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            /// Get a slice of an array
            val GetArraySlice3D : source:'T[,,] -> start1:int option * finish1:int option * start2:int option * finish2:int option * start3:int option * finish3:int option -> 'T[,,]

            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            /// Set a slice of an array
            val SetArraySlice3D : target:'T[,,] -> start1:int option * finish1:int option * start2:int option * finish2:int option * start3:int option * finish3:int option * source:'T[,,] -> unit

            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            /// Get a slice of an array
            val GetArraySlice4D : source:'T[,,,] -> start1:int option * finish1:int option * start2:int option * finish2:int option * start3:int option * finish3:int option * start4:int option * finish4:int option -> 'T[,,,]

           // [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            /// Set a slice of an array
            val SetArraySlice4D : target:'T[,,,] -> start1:int option * finish1:int option * start2:int option * finish2:int option * start3:int option * finish3:int option * start4:int option * finish4:int option * source:'T[,,,] -> unit

            //[<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            /// Get a slice from a string
            val inline GetStringSlice : source:string -> start:int option * finish:int option -> string

            /// Generate a range of integers  
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeInt32        : start:int        * step:int        * stop:int        -> seq<int>  
            /// Generate a range of float values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeDouble      : start:float      * step:float      * stop:float      -> seq<float>
            /// Generate a range of float32 values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeSingle    : start:float32    * step:float32    * stop:float32    -> seq<float32> 
            /// Generate a range of int64 values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeInt64      : start:int64      * step:int64      * stop:int64      -> seq<int64> 
            /// Generate a range of uint64 values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeUInt64     : start:uint64     * step:uint64     * stop:uint64     -> seq<uint64> 
            /// Generate a range of uint32 values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeUInt32     : start:uint32     * step:uint32     * stop:uint32     -> seq<uint32> 
            /// Generate a range of nativeint values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeIntPtr  : start:nativeint  * step:nativeint  * stop:nativeint  -> seq<nativeint> 
            /// Generate a range of unativeint values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeUIntPtr : start:unativeint * step:unativeint * stop:unativeint -> seq<unativeint> 
            /// Generate a range of int16 values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeInt16      : start:int16      * step:int16      * stop:int16      -> seq<int16> 
            /// Generate a range of uint16 values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeUInt16     : start:uint16     * step:uint16     * stop:uint16     -> seq<uint16> 
            /// Generate a range of sbyte values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeSByte      : start:sbyte      * step:sbyte      * stop:sbyte      -> seq<sbyte> 
            /// Generate a range of byte values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeByte       : start:byte       * step:byte       * stop:byte       -> seq<byte> 
            /// Generate a range of char values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeChar       : start:char                          * stop:char       -> seq<char> 
            /// Generate a range of values using the given zero, add, start, step and stop values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeGeneric   : one:'T * add:('T -> 'T -> 'T) * start:'T   * stop:'T       -> seq<'T> 
            /// Generate a range of values using the given zero, add, start, step and stop values
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RangeStepGeneric   : zero:'U * add:('T -> 'U -> 'T) * start:'T   * step:'U       * stop:'T       -> seq<'T> 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val AbsDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val AcosDynamic : x:'T -> 'T 


            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val AsinDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val AtanDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val Atan2Dynamic : y:'T -> x:'T -> 'U

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val CeilingDynamic : x:'T -> 'T 


            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val ExpDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val FloorDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val TruncateDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val RoundDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val SignDynamic : 'T -> int

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val LogDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val Log10Dynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val SqrtDynamic : 'T -> 'U

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val CosDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val CoshDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val SinDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val SinhDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val TanDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val TanhDynamic : x:'T -> 'T 

            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowDynamic : x:'T -> y:'T -> 'T 

            
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'byte'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowByte : x:byte -> n:int -> byte
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'sbyte'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowSByte : x:sbyte -> n:int -> sbyte
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'int16'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowInt16 : x:int16 -> n:int -> int16
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'uint16'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowUInt16 : x:uint16 -> n:int -> uint16
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'int32'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowInt32 : x:int32 -> n:int -> int32
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'uint32'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowUInt32 : x:uint32 -> n:int -> uint32
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'int64'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowInt64 : x:int64 -> n:int -> int64
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'uint64'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowUInt64 : x:uint64 -> n:int -> uint64
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'nativeint'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowIntPtr : x:nativeint -> n:int -> nativeint
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'unativeint'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowUIntPtr : x:unativeint -> n:int -> unativeint
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'float32'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowSingle : x:float32 -> n:int -> float32
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'float'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowDouble : x:float -> n:int -> float
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator on values of type 'decimal'
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowDecimal : x:decimal -> n:int -> decimal
            /// This is a library intrinsic. Calls to this function may be generated by uses of the generic 'pown' operator 
            [<Obsolete("This function is for use by compiled F# code and should not be used directly")>]
            val PowGeneric : one:'T * mul: ('T -> 'T -> 'T) * value:'T * exponent:int -> 'T

        /// This module contains basic operations which do not apply runtime and/or static checks
        module Unchecked =

            /// Generate a defult value for any type. This is null for reference types, 
            /// For structs, this is struct value where all fields have the default value. 
            /// This function is unsafe in the sense that some F# values do not have proper <c>null</c> values.
            [<RequiresExplicitTypeArguments>] 
            val inline defaultof<'T> : 'T

        /// This module contains the basic arithmetic operations with overflow checks.
        module Checked =
            /// Overloaded unary negation (checks for overflow)
            [<NoDynamicInvocation>]
            val inline ( ~- ) : value:^T -> ^T when ^T : (static member ( ~- ) : ^T -> ^T) and default ^T : int

            /// Overloaded subtraction operator (checks for overflow)
            [<NoDynamicInvocation>]
            val inline ( - ) : x:^T -> y:^U -> ^V  when (^T or ^U) : (static member ( - ) : ^T * ^U    -> ^V) and default ^U : ^V and default ^V : ^T and default ^V : ^U and default ^T : ^V and default ^T : ^U and default ^T : int

            /// Overloaded addition operator (checks for overflow)
            val inline ( + ) : x:^T -> y:^U -> ^V  when (^T or ^U) : (static member ( + ) : ^T * ^U    -> ^V) and default ^U : ^V and default ^V : ^T and default ^V : ^U and default ^T : ^V and default ^T : ^U and default ^T : int

            /// Overloaded multiplication operator (checks for overflow)
            [<NoDynamicInvocation>]
            val inline ( * ) : x:^T -> y:^U -> ^V  when (^T or ^U) : (static member ( * ) : ^T * ^U    -> ^V) and default ^U : ^V and default ^V : ^T and default ^V : ^U and default ^T : ^V and default ^T : ^U and default ^T : int

            /// Converts the argument to byte. This is a direct conversion for all 
            /// primitive numeric types and <c>ToByte</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline byte       : value:^T -> byte       when ^T : (static member ToByte    : ^T -> byte)       and default ^T : int

            /// Converts the argument to signed byte. This is a direct conversion for all 
            /// primitive numeric types and <c>ToSByte</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline sbyte      : value:^T -> sbyte      when ^T : (static member ToSByte   : ^T -> sbyte)      and default ^T : int

            /// Converts the argument to signed 16-bit integer. This is a direct conversion for all 
            /// primitive numeric types and <c>ToInt16</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline int16      : value:^T -> int16      when ^T : (static member ToInt16   : ^T -> int16)      and default ^T : int

            /// Converts the argument to unsigned 16-bit integer. This is a direct conversion for all 
            /// primitive numeric types and <c>ToUInt16</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline uint16     : value:^T -> uint16     when ^T : (static member ToUInt16  : ^T -> uint16)     and default ^T : int

            /// Converts the argument to signed 32-bit integer. This is a direct conversion for all 
            /// primitive numeric types and <c>ToInt32</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline int        : value:^T -> int        when ^T : (static member ToInt32   : ^T -> int)        and default ^T : int

            /// Converts the argument to signed 32-bit integer. This is a direct conversion for all 
            /// primitive numeric types and <c>ToInt32</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline int32      : value:^T -> int32      when ^T : (static member ToInt32   : ^T -> int32)      and default ^T : int

            /// Converts the argument to unsigned 32-bit integer. This is a direct conversion for all 
            /// primitive numeric types and <c>ToUInt32</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline uint32     : value:^T -> uint32     when ^T : (static member ToUInt32  : ^T -> uint32)     and default ^T : int

            /// Converts the argument to signed 64-bit integer. This is a direct conversion for all 
            /// primitive numeric types and <c>ToInt64</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline int64      : value:^T -> int64      when ^T : (static member ToInt64   : ^T -> int64)      and default ^T : int

            /// Converts the argument to unsigned 64-bit integer. This is a direct conversion for all 
            /// primitive numeric types and <c>ToUInt64</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline uint64     : value:^T -> uint64     when ^T : (static member ToUInt64  : ^T -> uint64)     and default ^T : int

            /// Converts the argument to signed native integer. This is a direct conversion for all 
            /// primitive numeric types and <c>ToIntPtr</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline nativeint  : value:^T -> nativeint  when ^T : (static member ToIntPtr  : ^T -> nativeint)  and default ^T : int

            /// Converts the argument to unsigned native integer. This is a direct conversion for all 
            /// primitive numeric types and <c>ToUIntPtr</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline unativeint : value:^T -> unativeint when ^T : (static member ToUIntPtr : ^T -> unativeint) and default ^T : int

            /// Converts the argument to unicode character based on UTF16 encoding (a direct conversion for all 
            /// primitive numeric types and <c>ToUIntPtr</c> method otherwise)
            [<NoDynamicInvocation>]
            val inline char        : value:^T -> char      when ^T : (static member ToChar   : ^T -> char)        and default ^T : int



namespace Microsoft.FSharp.Control
    open Microsoft.FSharp.Core
    
    /// An exeption type raised when the evaluation of a lazy value recursively depend upon itself
    exception Undefined  
    
#if FX_ATLEAST_40

    [<AutoOpen>]
    module LazyExtensions =
        type System.Threading.LazyInit<'T> with
            /// Create a lazy computation that evaluates to the result of the given function when forced          
            static member Create : creator:(unit -> 'T) -> System.Threading.LazyInit<'T>
            /// Create a lazy computation that evaluates to the given value when forced
            static member CreateFromValue : value:'T -> System.Threading.LazyInit<'T>
            /// Indicates if the lazy value has yet to be computed 
            [<System.Obsolete("This method is now deprecated, please use not(x.IsInitialized)")>]  
            member IsDelayed : bool
            /// Indicates if the lazy value has been successfully computed
            [<System.Obsolete("This method is now deprecated, please use x.IsInitialized")>]  
            member IsForced : bool
            /// Indicates if the lazy value is being computed or the computation raised an exception
            [<System.Obsolete("This method is now deprecated")>]  
            member IsException : bool
            /// Force the execution of this value and return its result. Same as Value. Mutual exclusion is used to 
            /// prevent other threads also computing the value. If the value is re-forced during its own computation
            /// the <c>Undefined</c> exception is raised.
            [<System.Obsolete("This method is now deprecated, please use x.Value")>]  
            member Force : unit -> 'T
            // Same as Force
            [<System.Obsolete("This method is now deprecated")>]  
            member SynchronizedForce : unit -> 'T
            /// Same as Force, except no lock is taken. 
            [<System.Obsolete("This method is now deprecated")>]  
            member UnsynchronizedForce : unit -> 'T

    /// The type of delayed computations.
    /// 
    /// Use the values in the <c>Lazy</c> module to manipulate 
    /// values of this type, and the notation 'lazy expr' to create values
    /// of this type.
    type Lazy<'T> = System.Threading.LazyInit<'T>
    and 'T ``lazy`` = Lazy<'T>        
    

#else
  

    /// The type of delayed computations.
    /// 
    /// Use the values in the <c>Lazy</c> module to manipulate 
    /// values of this type, and the notation 'lazy expr' to create values
    /// of this type.
    [<Sealed>]
    type Lazy <'T> =
        /// Indicates if the lazy value has been successfully computed
        member IsForced : bool
        /// Indicates if the lazy value is being computed or the computation raised an exception
        member IsException : bool
        /// Indicates if the lazy value has yet to be computed 
        member IsDelayed : bool
        /// Same as Force
        member SynchronizedForce : unit -> 'T
        /// Same as Force, except no lock is taken. 
        member UnsynchronizedForce : unit -> 'T
        /// Force the execution of this value and return its result. Same as Value. Mutual exclusion is used to 
        /// prevent other threads also computing the value. If the value is re-forced during its own computation
        /// the <c>Undefined</c> exception is raised.
        member Force : unit -> 'T
        /// Force the execution of this value and return its result. Same as Value. Mutual exclusion is used to 
        /// prevent other threads also computing the value.
        member Value : 'T
        /// Create a lazy computation that evaluates to the result of the given function when forced
        static member Create : creator:(unit -> 'T) -> Lazy<'T>
        /// Create a lazy computation that evaluates to the given value when forced
        static member CreateFromValue : value:'T -> Lazy<'T>
    /// The type of delayed computations.
    /// 
    /// Use the values in the <c>Lazy</c> module to manipulate 
    /// values of this type, and the notation 'lazy expr' to create values
    /// of this type.
    and 'T ``lazy`` = Lazy<'T>
#endif


        
namespace Microsoft.FSharp.Control

    open Microsoft.FSharp.Core

    /// F# gives special status to non-virtual instance member properties compatible with type IDelegateEvent, 
    /// generating approriate .NET metadata to make the member appear to other .NET languages as a
    /// .NET event.
    type IDelegateEvent<'Del when 'Del :> System.Delegate > =
        /// Connect a handler delegate object to the event.  A handler can
        /// be later removed using RemoveHandler.  The listener will
        /// be invoked when the event is fired.
        abstract AddHandler: handler:'Del -> unit
        /// Remove a listener delegate from an event listener store
        abstract RemoveHandler: handler:'Del -> unit 

    ///The family of first class event values for delegate types that satisfy the F# delegate constraint.
    type IEvent<'Del,'Args when 'Del : delegate<'Args,unit> and 'Del :> System.Delegate > =
        /// Connect a listener function to the event. The listener will
        /// be invoked when the event is fired.
        abstract Add: callback:('Args -> unit) -> unit
        inherit IDelegateEvent<'Del>


    /// A delegate type associated with the F# event type <c>IEvent&lt;_&gt;</c>
    type Handler<'T> =  delegate of sender:obj * args:'T -> unit 

    /// First-class listening points (i.e. objects that permit you to register a 'callback'
    /// activated when the event is triggered). See the module <c>Event</c>
    /// for functions to create events.
    type IEvent<'T> = IEvent<Handler<'T>, 'T>

