//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

/// This namespace contains constructs for reflecting on the representation of
/// F# values and types. It augments the design of System.Reflection.
namespace Microsoft.FSharp.Reflection 

open System
open System.Reflection
open Microsoft.FSharp.Core
open Microsoft.FSharp.Primitives.Basics
open Microsoft.FSharp.Collections

//---------------------------------------------------------------------
// F# reified type inspection.

///Represents a case of a discriminated union type
[<Sealed>]
type UnionCaseInfo =
    /// The name of the case
    member Name : string
    /// The type in which the case occurs
    member DeclaringType: Type
    
    /// Return the custom attributes associated with the case
    member GetCustomAttributes: unit -> obj[]
    /// Return the custom attributes associated with the case matching the given attribute type
    member GetCustomAttributes: attributeType:System.Type -> obj[]

    /// The fields associated with the case, represented by PropertyInfo 
    member GetFields: unit -> PropertyInfo []

    /// The integer tag for the case
    member Tag: int


[<AbstractClass; Sealed>]
/// Contains operations associated with constructing and analyzing values associated with F# types such as records, unions and tuples
type FSharpValue = 

    /// Create an instance of a record type
    ///
    /// Assumes the given input is a record type. If not, ArgumentException is raised.
    static member MakeRecord: recordType:Type * values:obj [] * ?bindingFlags:BindingFlags  -> obj

    /// Read a field from a record value
    ///
    /// Assumes the given input is a record value. If not, ArgumentException is raised.
    static member GetRecordField:  record:obj * info:PropertyInfo -> obj

    /// Read all the fields from a record value
    ///
    /// Assumes the given input is a record value. If not, ArgumentException is raised.
    static member GetRecordFields:  record:obj * ?bindingFlags:BindingFlags  -> obj[]

    
    /// Precompute a function for reading a particular field from a record.
    /// Assumes the given type is a RecordType with a field of the given name. 
    /// If not, ArgumentException is raised during pre-computation.
    ///
    /// Using the computed function will typically be faster than executing a corresponding call to Value.GetInfo
    /// because the path executed by the computed function is optimized given the knowledge that it will be
    /// used to read values of the given type.
    static member PreComputeRecordFieldReader : info:PropertyInfo -> (obj -> obj)

    /// Precompute a function for reading all the fields from a record. The fields are returned in the
    /// same order as the fields reported by a call to Microsoft.FSharp.Reflection.Type.GetInfo for
    /// this type.
    ///
    /// Assumes the given type is a RecordType. 
    /// If not, ArgumentException is raised during pre-computation.
    ///
    /// Using the computed function will typically be faster than executing a corresponding call to Value.GetInfo
    /// because the path executed by the computed function is optimized given the knowledge that it will be
    /// used to read values of the given type.
    static member PreComputeRecordReader : recordType:Type  * ?bindingFlags:BindingFlags -> (obj -> obj[])


    /// Precompute a function for constructing a record value. 
    ///
    /// Assumes the given type is a RecordType.
    /// If not, ArgumentException is raised during pre-computation.
    static member PreComputeRecordConstructor : recordType:Type  * ?bindingFlags:BindingFlags -> (obj[] -> obj)

    /// Get a ConstructorInfo for a record type
    static member PreComputeRecordConstructorInfo: recordType:Type * ?bindingFlags:BindingFlags -> ConstructorInfo
    
    /// Create a union case value
    static member MakeUnion: unionCase:UnionCaseInfo * args:obj [] * ?bindingFlags:BindingFlags -> obj

    /// Identify the union case and its fields for an object
    ///
    /// Assumes the given input is a union case value. If not, ArgumentException is raised.
    ///
    /// If the type is not given, then the runtime type of the input object is used to identify the
    /// relevant union type. The type should always be given if the input object may be null. For example, 
    /// option values may be represented using the 'null'.
    static member GetUnionFields:  value:obj * unionType:Type * ?bindingFlags:BindingFlags -> UnionCaseInfo * obj []
    
    ///
    /// Assumes the given type is a union type. 
    /// If not, ArgumentException is raised during pre-computation.
    ///
    /// Using the computed function is more efficient than calling GetUnionCase
    /// because the path executed by the computed function is optimized given the knowledge that it will be
    /// used to read values of the given type.
    static member PreComputeUnionTagReader          : unionType:Type  * ?bindingFlags:BindingFlags -> (obj -> int)

    /// Precompute a property or static method for reading an integer representing the case tag of a union type.
    static member PreComputeUnionTagMemberInfo : unionType:Type  * ?bindingFlags:BindingFlags -> MemberInfo

    /// Precompute a function for reading all the fields for a particular discriminator case of a union type
    ///
    /// Using the computed function will typically be faster than executing a corresponding call to GetFields
    static member PreComputeUnionReader       : unionCase:UnionCaseInfo  * ?bindingFlags:BindingFlags -> (obj -> obj[])

    /// Precompute a function for constructing a discriminated union value for a particular union case. 
    static member PreComputeUnionConstructor : unionCase:UnionCaseInfo  * ?bindingFlags:BindingFlags -> (obj[] -> obj)

    /// A method that constructs objects of the given case
    static member PreComputeUnionConstructorInfo: unionCase:UnionCaseInfo * ?bindingFlags:BindingFlags -> MethodInfo

    /// Create an instance of a tuple type
    ///
    /// Assumes at least one element is given. If not, ArgumentException is raised.
    static member MakeTuple: tupleElements:obj[] * tupleType:Type -> obj

    /// Read a field from a tuple value
    ///
    /// Assumes the given input is a tuple value. If not, ArgumentException is raised.
    static member GetTupleField: tuple:obj * index:int -> obj

    /// Read all fields from a tuple 
    ///
    /// Assumes the given input is a tuple value. If not, ArgumentException is raised.
    static member GetTupleFields: tuple:obj -> obj []
    
    /// Precompute a function for reading the values of a particular tuple type
    ///
    /// Assumes the given type is a TupleType.
    /// If not, ArgumentException is raised during pre-computation.
    static member PreComputeTupleReader           : tupleType:Type -> (obj -> obj[])
    
    /// Get information that indicates how to read a field of a tuple
    static member PreComputeTuplePropertyInfo: tupleType:Type * index:int -> PropertyInfo * (Type * int) option
    
    /// Precompute a function for reading the values of a particular tuple type
    ///
    /// Assumes the given type is a TupleType.
    /// If not, ArgumentException is raised during pre-computation.
    static member PreComputeTupleConstructor      : tupleType:Type -> (obj[] -> obj)

    /// Get a method that constructs objects of the given tuple type. 
    /// For small tuples, no additional typoe will be returned.
    /// 
    /// For large tuples, an additional type is returned indicating that
    /// a nested encoding has been used for the tuple type. In this case
    /// the suffix portion of the tuple type has the given type and an
    /// object of this type must be created and passed as the last argument 
    /// to the ConstructorInfo. A recursive call to PreComputeTupleConstructorInfo 
    /// can be used to determine the constructor for that the suffix type.
    static member PreComputeTupleConstructorInfo: tupleType:Type -> ConstructorInfo * Type option

    /// Build a typed function from object from a dynamic function implementation
    static member MakeFunction           : functionType:Type * implementation:(obj -> obj) -> obj

    /// Read all the fields from a value built using an instance of an F# exception declaration
    ///
    /// Assumes the given input is an F# exception value. If not, ArgumentException is raised.
    static member GetExceptionFields:  exn:obj * ?bindingFlags:BindingFlags  -> obj[]


[<AbstractClass; Sealed>]
/// Contains operations associated with constructing and analyzing F# types such as records, unions and tuples
type FSharpType =

    /// Read all the fields from a record value, in declaration order
    ///
    /// Assumes the given input is a record value. If not, ArgumentException is raised.
    static member GetRecordFields: recordType:Type * ?bindingFlags:BindingFlags -> PropertyInfo[]

    /// Get the cases of a union type.
    ///
    /// Assumes the given type is a union type. If not, ArgumentException is raised during pre-computation.
    static member GetUnionCases: unionType:Type * ?bindingFlags:BindingFlags -> UnionCaseInfo[]

    /// Return a <c>System.Type</c> representing the F# function type with the given domain and range
    static member MakeFunctionType: domain:Type * range:Type -> Type

    /// Return a <c>System.Type</c> representing an F# tuple type with the given element types
    static member MakeTupleType: types:Type[] -> Type

    /// Return true if the <c>typ</c> is a representation of an F# tuple type 
    static member IsTuple : typ:Type -> bool

    /// Return true if the <c>typ</c> is a representation of an F# function type or the runtime type of a closure implementing an F# function type
    static member IsFunction : typ:Type -> bool

    /// Return true if the <c>typ</c> is a <c>System.Type</c> value corresponding to the compiled form of an F# module 
    static member IsModule: typ:Type -> bool

    /// Return true if the <c>typ</c> is a representation of an F# record type 
    static member IsRecord: typ:Type * ?bindingFlags:BindingFlags -> bool

    /// Return true if the <c>typ</c> is a representation of an F# union type or the runtime type of a value of that type
    static member IsUnion: typ:Type * ?bindingFlags:BindingFlags -> bool

    /// Get the tuple elements from the representation of an F# tuple type  
    static member GetTupleElements : tupleType:Type -> Type[]

    /// Get the domain and range types from an F# function type  or from the runtime type of a closure implementing an F# type
    static member GetFunctionElements : functionType:Type -> Type * Type

    /// Read all the fields from an F# exception declaration, in declaration order
    ///
    /// Assumes <c>exceptionType</c> is an exception representation type. If not, ArgumentException is raised.
    static member GetExceptionFields: exceptionType:Type * ?bindingFlags:BindingFlags -> PropertyInfo[]

    /// Return true if the <c>typ</c> is a representation of an F# exception declaration
    static member IsExceptionRepresentation: exceptionType:Type * ?bindingFlags:BindingFlags -> bool


