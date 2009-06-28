// (c) Microsoft Corporation 2005-2009
namespace FSharp.PowerPack.Metadata

open System.Collections.ObjectModel

type [<Sealed>] Range = 
   member Document : string
   member StartLine : int
   member StartColumn : int
   member EndLine : int
   member EndColumn : int
     
type [<Sealed>] FSharpAssembly = 

    /// Get the object representing the F# core library (FSharp.Core.dll) for the running program
    static member FSharpLibrary : FSharpAssembly

    /// This is one way of starting the loading process off. Dependencies are automatically
    /// resolved by calling System.Reflection.Assembly.Load.
    static member FromAssembly : System.Reflection.Assembly -> FSharpAssembly

    /// This is one way of starting the loading process off. 
    static member FromFile : fileName: string (* * loader:System.Func<string,FSharpAssembly> *) -> FSharpAssembly

    /// Holds the full qualified assembly name
    member QualifiedName: string; 
    
    /// Return the System.Reflection.Assembly object for the assembly
    member ReflectionAssembly: System.Reflection.Assembly
      
    /// Return the System.Reflection.Assembly object for the assembly
    member GetEntity : string -> FSharpEntity
      
      /// A hint as to where does the code for the CCU live (e.g what was the tcConfig.implicitIncludeDir at compilation time for this DLL?) 
    member CodeLocation: string; 
      
      /// A handle to the full specification of the contents of the module contained in this Assembly 
    member Entities:  ReadOnlyCollection<FSharpEntity>

and [<Sealed>] FSharpEntity = 

      /// Return the FSharpEntity corresponding to a .NET type
    static member FromType : System.Type -> FSharpEntity

      /// The name of the type, possibly with `n mangling  
    member Name: string;

      /// Get the fully qualified name of the type or module
    member QualifiedName: string; 


      /// The declaration location for the type constructor 
    member Range: Range; 

      /// Indicates the type is a measure, type or exception abbreviation
    member IsAbbreviation   : bool

      /// Indicates the type is a struct
    member IsStruct  : bool


      /// Indicates the entity is an F# module definition
    member IsModule: bool; 

      /// Get the generic parameters, possibly including unit-of-measure parameters
    member GenericParameters: ReadOnlyCollection<FSharpGenericParameter>

      /// Indicates that a module is compiled to a class with the given mangled name. The mangling is reversed during lookup 
    member HasFSharpModuleSuffix : bool

      /// Indicates the entity is a measure definition
    member IsMeasure: bool;

      /// Indicates an F# exception declaration
    member IsExceptionDeclaration: bool; 

    /// If true, then this is a reference to something in some .NET assembly from another .NET language
    member IsExternal : bool

    /// Return the System.Type for the type
    ///
    /// Raises InvalidOperationException if the type is an abbreviation or has an assembly code representation.
    member GetReflectionType : unit -> System.Type  


      /// The declared documentation for the type or module 
    member XmlDoc: ReadOnlyCollection<string>;

      /// Indicates the type is implemented through a mapping to IL assembly code. THis is only
      /// true for types in FSharp.Core.dll
    member HasAssemblyCodeRepresentation: bool 

    
      /// Indicates the type prefers the "tycon&lt;a,b&gt;" syntax for display etc. 
    member UsesPrefixDisplay: bool;                   

      /// The declared attributes for the type 
    member Attributes: ReadOnlyCollection<FSharpAttribute>;     

      /// Interface implementations - boolean indicates compiler-generated 
    member Implements : ReadOnlyCollection<FSharpType>;  

      /// Base type, if any 
    member BaseType : FSharpType;


      /// Properties, methods etc. with implementations, also values in a module
    member MembersOrValues : ReadOnlyCollection<FSharpMemberOrVal>;

    member NestedEntities : ReadOnlyCollection<FSharpEntity>

      /// The fields of the class, struct or enum 
    member RecordFields : ReadOnlyCollection<FSharpRecordField>

    member AbbreviatedType   : FSharpType 

      /// The cases of a discriminated union
    member UnionCases : ReadOnlyCollection<FSharpUnionCase>


#if TODO
      /// Indicates the type is implemented as IL assembly code using a closed type in ILDASM syntax
      // NOTE: consider returning a System.Type
    member GetAssemblyCodeRepresentation : unit -> string 


    //   /// Indicates the type is a delegate with the given Invoke signature 
    // member TyconDelegateSlotSig : SlotSig option


      /// The declared accessibility of the type
    member Accessibility: FSharpAccessibility; 

      /// The declared accessibility of the representation, not taking signatures into account 
    member RepresentationAccessibility: FSharpAccessibility;
#endif
      

and [<Sealed>] FSharpUnionCase =
      /// Name of the case 
    member Name: string; 
      /// Range of the name of the case 
    member Range : Range
    /// Data carried by the case. 
    member Fields: ReadOnlyCollection<FSharpRecordField>;
      /// Return type constructed by the case. Normally exactly the type of the enclosing type, sometimes an abbreviation of it 
    member ReturnType: FSharpType;
      /// Name of the case in generated IL code 
    member CompiledName: string;
      /// Documentation for the case 
    member XmlDoc: ReadOnlyCollection<string>;

#if TODO
      ///  Indicates the declared visibility of the union constructor, not taking signatures into account 
    member Accessibility: FSharpAccessibility; 
#endif
      /// Attributes, attached to the generated static method to make instances of the case 
    member Attributes: ReadOnlyCollection<FSharpAttribute>;

and [<Sealed>] FSharpRecordField =
    /// Is the field declared in F#? 
    member IsMutable: bool;
      /// Documentation for the field 
    member XmlDoc: ReadOnlyCollection<string>;
      /// The type of the field, w.r.t. the generic parameters of the enclosing type constructor 
    member Type: FSharpType;
      /// Indicates a static field 
    member IsStatic: bool;
      /// Indicates a compiler generated field, not visible to Intellisense or name resolution 
    member IsCompilerGenerated: bool;
      /// Declaration-location of the field 
    member Range: Range;
      /// Attributes attached to generated property 
    member PropertyAttributes: ReadOnlyCollection<FSharpAttribute>; 
      /// Attributes attached to generated field 
    member FieldAttributes: ReadOnlyCollection<FSharpAttribute>; 
      /// Name of the field 
    member Name : string

#if TODO
      /// The default initialization info, for static literals 
    member LiteralValue: obj; 
      ///  Indicates the declared visibility of the field, not taking signatures into account 
    member Accessibility: FSharpAccessibility; 
#endif

and [<Sealed>] FSharpAccessibility = 
#if TODO
    member IsPublic : bool
    member IsPrivate : bool
    member IsInternal : bool
#endif
    
and [<Sealed>] FSharpGenericParameter = 
    member Name: string
    member Range : Range; 
       
    /// Is this a measure variable
    member IsMeasure : bool

    /// The documentation for the type parameter. 
    member XmlDoc : ReadOnlyCollection<string>;
       
    /// Is this a ^a type variable
    member IsSolveAtCompileTime : bool 

    /// The declared attributes of the type parameter. 
    member Attributes: ReadOnlyCollection<FSharpAttribute>;                      
       
#if TODO
    /// The declared or inferred constraints for the type parameter
    member Constraints: ReadOnlyCollection<FSharpGenericParameterConstraint>; 
#endif


/// An F# discriminated union, as an object model
and [<Sealed>] FSharpGenericParameterConstraint = 
#if TODO
    /// Indicates a constraint that a type is a subtype of the given type 
    member IsCoercesToType : bool
    member GetCoercesToTypeTarget : unit -> FSharpType 

    /// Indicates a default value for an inference type variable should it be netiher generalized nor solved 
    member IsDefaultsToType : bool
    member GetDefaultsToTypePriority : unit -> int
    member GetDefaultsToTypeTarget : unit -> FSharpType

    /// Indicates a constraint that a type has a 'null' value 
    member IsSupportsNull  : bool

    /// Indicates a constraint that a type has a member with the given signature 
    member IsMayResolveMemberConstraint : bool
    member GetMayResolveMemberConstraintSources : unit -> ReadOnlyCollection<FSharpType>
    member GetMayResolveMemberConstraintMemberName : string 
    member GetMayResolveMemberConstraintIsStatc : bool
    member GetMayResolveMemberConstraintArgumentType : unit -> ReadOnlyCollection<FSharpType>
    member GetMayResolveMemberConstraintReturnType : unit -> FSharpType 

    /// Indicates a constraint that a type is a non-Nullable value type 
    member IsIsNotNullableValueType : bool
    
    /// Indicates a constraint that a type is a reference type 
    member IsIsReferenceType  : bool

    /// Indicates a constraint that a type is a simple choice between one of the given ground types. See format.ml 
    member IsSimpleChoice : bool
    member GetSimpleChoiceChoices : unit -> ReadOnlyCollection<FSharpType>

    /// Indicates a constraint that a type has a parameterless constructor 
    member IsRequiresDefaultConstructor  : bool

    /// Indicates a constraint that a type is an enum with the given underlying 
    member IsIsEnum : bool
    member GetIsEnumUnderlying : unit -> FSharpType 
    
    /// Indicates a constraint that a type is a delegate from the given tuple of args to the given return type *)
    member IsIsDelegate : bool
    member GetIsDelegateArgumentType : unit -> FSharpType 
    member GetIsDelegateReturnType : unit -> FSharpType 
#endif

and FSharpInlineAnnotation = 
   | PsuedoValue = 3
   /// Indictes the value is inlined but the code for the function still exists, e.g. to satisfy interfaces on objects, but that it is also always inlined 
   | AlwaysInline = 2
   | OptionalInline = 1
   | NeverInline = 0

and [<Sealed>] FSharpMemberOrVal = 
    member Range: Range
    
    /// The typars of the member or value
    member GenericParameters: ReadOnlyCollection<FSharpGenericParameter>

    /// The full type of the member or value when used as a first class value
    member Type: FSharpType

    /// Is this a compiler generated value
    member IsCompilerGenerated : bool

    /// Is this a must-inline value

    member InlineAnnotation : FSharpInlineAnnotation

    /// Is this a mutable value
    member IsMutable : bool

    /// Is this a module or member value
    member IsModuleValueOrMember : bool

    /// Is this an extension member?
    member IsExtensionMember : bool

    /// Is this an implicit constructor?
    member IsImplicitConstructor : bool
    
    /// Is this an F# type function
    member IsTypeFunction : bool

      /// The member name in compiled code
    member CompiledName: string

    member CurriedParameterGroups : ReadOnlyCollection<ReadOnlyCollection<FSharpParameter>>

    member ReturnParameter : FSharpParameter

      /// Custom attributes attached to the value. These contain references to other values (i.e. constructors in types). Mutable to fixup  
      /// these value references after copying a colelction of values. 
    member Attributes: ReadOnlyCollection<FSharpAttribute>

      /// XML documentation attached to a value.
    member XmlDoc: ReadOnlyCollection<string>; 

     
#if TODO
    /// Is this "base" in "base.M(...)"
    member IsBaseValue : bool

    /// Is this the "x" in "type C() as x = ..."
    member IsConstructorThisValue : bool

    /// Is this the "x" in "member x.M = ..."
    member IsMemberThisValue : bool

    /// Is this a [&lt;Literal&gt;] value, and if so what value?
    member LiteralValue : obj // may be null

      /// How visible is this? 
    member Accessibility : FSharpAccessibility
      /// Get the module, type or namespace where this value appears. For 
      /// an extension member this is the type being extended 
    member ApparentParent: FSharpEntity

     /// Get the module, type or namespace where this value is compiled
    member ActualParent: FSharpEntity;

#endif


and [<Sealed>] FSharpParameter =
    member Name: string
    member Range : Range; 
    member Type : FSharpType; 
    member Attributes: ReadOnlyCollection<FSharpAttribute>


and [<Sealed>] FSharpType =

    /// Indicates the type is constructed using a named entity
    member IsNamed : bool
    /// Get the named entity for a type constructed using a named entity
    member NamedEntity : FSharpEntity 
    /// Get the generic arguments for a tuple type, a function type or a type constructed using a named entity
    member GenericArguments : ReadOnlyCollection<FSharpType>
    
    /// Indicates the type is a tuple type. The GenericArguments property returns the elements of the tuple type.
    member IsTuple : bool

    /// Indicates the type is a function type. The GenericArguments property returns the domain and range of the function type.
    member IsFunction : bool

    /// Indicates the type is a variable type, whether declared, generalized or an inference type parameter  
    member IsGenericParameter : bool
    /// Get the generic parameter data for a generic parameter type
    member GenericParameter : FSharpGenericParameter
    /// Get the index for a generic parameter type
    member GenericParameterIndex : int


and [<Sealed>] FSharpAttribute = 
    member Value : obj  
    
    member GetReflectionType: unit -> System.Type 



