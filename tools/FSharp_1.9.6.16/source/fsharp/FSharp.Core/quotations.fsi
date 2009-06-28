//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

/// Types and functions related to expression quotations
namespace Microsoft.FSharp.Quotations

#if FX_MINIMAL_REFLECTION // not on Compact Framework 
#else
open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Reflection
open System
open System.Reflection

[<Sealed>]
/// Information at the binding site of a variable
type Var =
    /// The type associated with the variable
    member Type : Type
    /// The declared name of the variable
    member Name : string
    /// Indicates if the variable represents a mutable storage location
    member IsMutable: bool
    /// Create a new variable with the given name, type and mutability
    new : name:string * typ:Type * ?isMutable : bool -> Var
    /// Fetch or create a new variable with the given name and type from a global pool of shared variables
    /// indexed by name and type
    static member Global : name:string * typ:Type -> Var
    
    interface System.IComparable

/// Quoted expressions annotated with System.Type values. 
[<Class>]
type Expr =
    
    /// Substitute through the given expression using the given functions
    /// to map variables to new values.  The functions must give consistent results
    /// at each application.  Variable renaming may occur on the target expression
    /// if variable capture occurs.
    member Substitute : substitution:(Var -> Expr option) -> Expr 

    /// Get the free expression variables of an expression as a list
    member GetFreeVars : unit -> seq<Var>

    /// Returns type of an expression
    member Type : Type

    /// Returns the custom attributes of an expression
    member CustomAttributes : Expr list

    override Equals : obj:obj -> bool 
    
    /// Build an expression that represents getting the address of a value
    static member AddressOf : target:Expr -> Expr
    
    /// Build an expression that represents setting the value held at a particular address
    static member AddressSet : target:Expr * value:Expr -> Expr
    
    /// Build an expression that represents the application of a first class function value to a single argument
    static member Application: functionExpr:Expr * argument:Expr -> Expr
    
    /// Build an expression that represents the application of a first class function value to multiple arguments
    static member Applications: functionExpr:Expr * arguments:list<list<Expr>> -> Expr
    
    /// Build an expression that represents a call to an static method or module-bound function
    static member Call : methodInfo:MethodInfo * arguments:list<Expr> -> Expr

    /// Build an expression that represents a call to an instance method associated with an object
    static member Call : obj:Expr * methodInfo:MethodInfo * arguments:list<Expr> -> Expr

    /// Build an expression that represents the coercion of an expression to a type
    static member Coerce : source:Expr * target:Type -> Expr 

    /// Build 'if ... then ... else' expressions    
    static member IfThenElse : guard:Expr * thenExpr:Expr * elseExpr:Expr -> Expr 

    /// Build a 'for i = ... to ... do ...' expression that represent loops over integer ranges
    static member ForIntegerRangeLoop: loopVariable:Var * start:Expr * endExpr:Expr * body:Expr -> Expr 

    /// Build an expression that represents the access of a static field 
    static member FieldGet: fieldInfo:FieldInfo -> Expr 

    /// Build an expression that represents the access of a field of an object
    static member FieldGet: obj:Expr * fieldInfo:FieldInfo -> Expr 

    /// Build an expression that represents writing to a static field 
    static member FieldSet: fieldInfo:FieldInfo * value:Expr -> Expr 

    /// Build an expression that represents writing to a field of an object
    static member FieldSet: obj:Expr * fieldInfo:FieldInfo * value:Expr -> Expr 

    /// Build an expression that represents the constrution of an F# function value
    static member Lambda : parameter:Var * body:Expr -> Expr

    /// Build expressions associated with 'let' constructs
    static member Let : letVariable:Var * letExpr:Expr * body:Expr -> Expr 

    /// Build recursives expressions associated with 'let rec' constructs
    static member LetRec : bindings:(Var * Expr) list * body:Expr -> Expr 

    /// Build an expression that represents the invocation of an object constructor
    static member NewObject: constructorInfo:ConstructorInfo * arguments:Expr list -> Expr 


    /// Build an expression that represents the invocation of a default object constructor
    static member DefaultValue: expressionType:Type -> Expr 


    /// Build an expression that represents the creation of an F# tuple value
    static member NewTuple: elements:Expr list -> Expr 

    /// Build record-construction expressions 
    static member NewRecord: recordType:Type * elements:Expr list -> Expr 

    /// Build an expression that represents the creation of an array value initialized with the given elements
    static member NewArray: elementType:Type * elements:Expr list -> Expr 

    /// Build an expression that represents the creation of a delegate value for the given type
    static member NewDelegate: delegateType:Type * parameters:Var list * body:Expr -> Expr 

    /// Build an expression that represents the creation of a union case value
    static member NewUnionCase: unionCase:UnionCaseInfo * arguments:Expr list -> Expr 

    /// Build an expression that represents reading a property of an object
    static member PropGet: obj:Expr * property:PropertyInfo  * ?indexerArgs: Expr list -> Expr 

    /// Build an expression that represents reading a static property 
    static member PropGet: property:PropertyInfo * ?indexerArgs: Expr list -> Expr 

    /// Build an expression that represents writing to a property of an object
    static member PropSet: obj:Expr * property:PropertyInfo * value:Expr * ?indexerArgs: Expr list -> Expr 

    /// Build an expression that represents writing to a static property 
    static member PropSet: property:PropertyInfo * value:Expr * ?indexerArgs: Expr list -> Expr 

    /// Build an expression that represents a nested quotation literal
    static member Quote: inner:Expr -> Expr 

    /// Build an expression that represents the sequential execution of one expression followed by another
    static member Sequential: first:Expr * second:Expr -> Expr 

    /// Build an expression that represents a try/with construct for exception filtering and catching 
    static member TryWith: body:Expr * filterVar:Var * filterBody:Expr * catchVar:Var * catchBody:Expr -> Expr 

    /// Build an expression that represents a try/finally construct 
    static member TryFinally: body:Expr * compensation:Expr -> Expr 

    /// Build an expression that represents getting a field of a tuple
    static member TupleGet: tuple:Expr * index:int -> Expr 


    /// Build an expression that represents a type test
    static member TypeTest: source:Expr * target:Type -> Expr 

    /// Build an expression that represents a test of a value is of a particular union case
    static member UnionCaseTest: source:Expr * unionCase:UnionCaseInfo -> Expr 

    /// Build an expression that represents a constant value of a particular type
    static member Value : value:obj * expressionType:Type -> Expr
    
    /// Build an expression that represents a constant value 
    static member Value : value:'T -> Expr

    /// Build an expression that represents a variable
    static member Var : variable:Var -> Expr
    
    /// Build an expression that represents setting a mutable variable
    static member VarSet : variable:Var * value:Expr -> Expr
    
    /// Build an expression that represents a while loop
    static member WhileLoop : guard:Expr * body:Expr -> Expr

    //----------------    


    /// Return a new typed expression given an underlying runtime-typed expression.
    /// A type annotation is usually required to use this function, and 
    /// using an incorrect type annotation may result in a later runtime exception.
    static member Cast : source:Expr -> Expr<'T> 

    /// Try and find a stored reflection definition for the given method. Stored reflection
    /// definitions are added to an F# assembly through the use of the [&lt;ReflectedDefinition&gt;] attribute.
    static member TryGetReflectedDefinition : methodBase:MethodBase -> Expr option
    
    /// This function is called automatically when quotation syntax (&lt;@ @&gt;) and related typed-expression
    /// quotations are used. The bytes are a pickled binary representation of an unlinked form of the qutoed expression,
    /// and the System.Type argument is any type in the assembly where the quoted
    /// expression occurs, i.e. it helps scope the interpretation of the cross-assembly
    /// references in the bytes.
    static member Deserialize : qualifyingType:System.Type * spliceTypes:list<System.Type> * spliceExprs:list<Expr> * value:byte[] -> Expr
    
    /// Permit interactive environments such as F# Interactive
    /// to explicitly register new pickled resources that represent persisted 
    /// top level definitions. The string indicates a unique name for the resources
    /// being added. The format for the bytes is the encoding generated by the F# compiler.
    static member RegisterReflectedDefinitions: assembly:Assembly * resource:string * serializedValue:byte[] -> unit

    /// Fetch or create a new variable with the given name and type from a global pool of shared variables
    /// indexed by name and type. The type is given by the expicit or inferred type parameter
    static member GlobalVar<'T> : name:string -> Expr<'T>


/// Type-carrying quoted expressions.  Expressions are generated either
/// by quotations in source text or programatically
and 
    [<Class>]
    Expr<'T> =
        inherit Expr
        /// Get the raw expression associated with this type-carrying expression
        member Raw : Expr


[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
/// Contains a set of primitive F# active patterns to analyze F# expression objects
module Patterns =
    
    /// An active pattern to recognize expressions that represent getting the address of a value
    val (|AddressOf|_|)       : input:Expr -> Expr option
    /// An active pattern to recognize expressions that represent setting the value held at an address 
    val (|AddressSet|_|)      : input:Expr -> (Expr * Expr) option
    /// An active pattern to recognize expressions that represent applications of first class function values
    val (|Application|_|)     : input:Expr -> (Expr * Expr) option
    /// An active pattern to recognize expressions that represent calls to static and instance methods, and functions defined in modules
    val (|Call|_|)            : input:Expr -> (Expr option * MethodInfo * Expr list) option
    /// An active pattern to recognize expressions that represent coercions from one type to another
    val (|Coerce|_|)          : input:Expr -> (Expr * Type) option
    /// An active pattern to recognize expressions that represent getting a static or instance field 
    val (|FieldGet|_|)        : input:Expr -> (Expr option * FieldInfo) option
    /// An active pattern to recognize expressions that represent setting a static or instance field 
    val (|FieldSet|_|)        : input:Expr -> (Expr option * FieldInfo * Expr) option
    /// An active pattern to recognize expressions that represent loops over integer ranges
    val (|ForIntegerRangeLoop|_|) : input:Expr -> (Var * Expr * Expr * Expr) option
    /// An active pattern to recognize expressions that represent while loops 
    val (|WhileLoop|_|)       : input:Expr -> (Expr * Expr) option
    /// An active pattern to recognize expressions that represent conditionals
    val (|IfThenElse|_|)      : input:Expr -> (Expr * Expr * Expr) option
    /// An active pattern to recognize expressions that represent first class function values
    val (|Lambda|_|)          : input:Expr -> (Var * Expr) option
    /// An active pattern to recognize expressions that represent let bindings
    val (|Let|_|)             : input:Expr -> (Var * Expr * Expr) option
    /// An active pattern to recognize expressions that represent recursive let bindings of one or more variables
    val (|LetRec|_|)          : input:Expr -> ((Var * Expr) list * Expr) option
    /// An active pattern to recognize expressions that represent the construction of arrays 
    val (|NewArray|_|)        : input:Expr -> (Type * Expr list) option
    /// An active pattern to recognize expressions that represent invocations of a default constructor of a struct
    val (|DefaultValue|_|)    : input:Expr -> Type option
    /// An active pattern to recognize expressions that represent construction of delegate values
    val (|NewDelegate|_|)     : input:Expr -> (Type * Var list * Expr) option
    /// An active pattern to recognize expressions that represent invocation of object constructors
    val (|NewObject|_|)       : input:Expr -> (ConstructorInfo * Expr list) option
    /// An active pattern to recognize expressions that represent construction of record values
    val (|NewRecord|_|)       : input:Expr -> (Type * Expr list) option
    /// An active pattern to recognize expressions that represent construction of particular union case values
    val (|NewUnionCase|_|)    : input:Expr -> (UnionCaseInfo * Expr list) option
    /// An active pattern to recognize expressions that represent construction of tuple values
    val (|NewTuple|_|)        : input:Expr -> (Expr list) option
    /// An active pattern to recognize expressions that represent the read of a static or instance property, or a non-function value declared in a module
    val (|PropGet|_|)         : input:Expr -> (Expr option * PropertyInfo * Expr list) option
    /// An active pattern to recognize expressions that represent setting a static or instance property, or a non-function value declared in a module
    val (|PropSet|_|)         : input:Expr -> (Expr option * PropertyInfo * Expr list * Expr) option
    /// An active pattern to recognize expressions that represent a nested quotation literal
    val (|Quote|_|)           : input:Expr -> Expr option 
    /// An active pattern to recognize expressions that represent sequential exeuction of one expression followed by another
    val (|Sequential|_|)      : input:Expr -> (Expr * Expr) option 
    /// An active pattern to recognize expressions that represent a try/with construct for exception filtering and catching 
    val (|TryWith|_|)        : input:Expr -> (Expr * Var * Expr * Var * Expr) option 
    /// An active pattern to recognize expressions that represent a try/finally construct 
    val (|TryFinally|_|)      : input:Expr -> (Expr * Expr) option 
    /// An active pattern to recognize expressions that represent getting a tuple field
    val (|TupleGet|_|)        : input:Expr -> (Expr * int) option 
    /// An active pattern to recognize expressions that represent a dynamic type test
    val (|TypeTest|_|)        : input:Expr -> (Expr * Type) option 
    /// An active pattern to recognize expressions that represent a test if a value is of a particular union case
    val (|UnionCaseTest|_|)   : input:Expr -> (Expr * UnionCaseInfo) option 
    /// An active pattern to recognize expressions that represent a constant value
    val (|Value|_|)           : input:Expr -> (obj * Type) option
    /// An active pattern to recognize expressions that represent a variable
    val (|Var|_|)             : input:Expr -> Var option
    /// An active pattern to recognize expressions that represent setting a mutable variable
    val (|VarSet|_|)          : input:Expr -> (Var * Expr) option
    
    //----------------------------------------------------------------
    // Helpers

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
/// Contains a set of derived F# active patterns to analyze F# expression objects
module DerivedPatterns =    
    //val (|NewList|_|)       : input:Expr -> Expr list option
    /// An active pattern to recognize expressions that represent a (possibly curried or tupled) first class function value
    val (|Lambdas|_|)       : input:Expr -> (Var list list * Expr) option
    /// An active pattern to recognize expressions that represent the application of a (possibly curried or tupled) first class function value
    val (|Applications|_|)  : input:Expr -> (Expr * Expr list list) option
    /// An active pattern to recognize expressions of the form <c>a &amp;&amp; b</c> 
    val (|AndAlso|_|)       : input:Expr -> (Expr * Expr) option
    /// An active pattern to recognize expressions of the form <c>a || b</c> 
    val (|OrElse|_|)        : input:Expr -> (Expr * Expr) option

    /// An active pattern to recognize <c>()</c> constant expressions
    val (|Unit|_|)          : input:Expr -> unit option 
    /// An active pattern to recognize constant boolean expressions
    val (|Bool|_|)          : input:Expr -> bool option 
    /// An active pattern to recognize constant string expressions
    val (|String|_|)        : input:Expr -> string option 
    /// An active pattern to recognize constant 32-bit floating point number expressions
    val (|Single|_|)        : input:Expr -> float32 option 
    /// An active pattern to recognize constant 64-bit floating point number expressions
    val (|Double|_|)        : input:Expr -> float option 
    /// An active pattern to recognize constant unicode character expressions
    val (|Char|_|)          : input:Expr -> char  option 
    /// An active pattern to recognize constant signed byte expressions
    val (|SByte|_|)         : input:Expr -> sbyte option 
    /// An active pattern to recognize constant byte expressions
    val (|Byte|_|)          : input:Expr -> byte option 
    /// An active pattern to recognize constant int16 expressions
    val (|Int16|_|)         : input:Expr -> int16 option 
    /// An active pattern to recognize constant unsigned int16 expressions
    val (|UInt16|_|)        : input:Expr -> uint16 option 
    /// An active pattern to recognize constant int32 expressions
    val (|Int32|_|)         : input:Expr -> int32 option 
    /// An active pattern to recognize constant unsigned int32 expressions
    val (|UInt32|_|)        : input:Expr -> uint32 option 
    /// An active pattern to recognize constant int64 expressions
    val (|Int64|_|)         : input:Expr -> int64 option 
    /// An active pattern to recognize constant unsigned int64 expressions
    val (|UInt64|_|)        : input:Expr -> uint64 option 
    /// A parameterized active pattern to recognize calls to a specified function or method
    val (|SpecificCall|_|)  : templateParameter:Expr -> (Expr -> (list<Type> * list<Expr>) option)

    /// An active pattern to recognize methods that have an associated ReflectedDefinition
    val (|MethodWithReflectedDefinition|_|) : methodBase:MethodBase -> Expr option
    
    /// An active pattern to recognize property getters or values in modules that have an associated ReflectedDefinition
    val (|PropertyGetterWithReflectedDefinition|_|) : propertyInfo:PropertyInfo -> Expr option
    /// An active pattern to recognize property setters that have an associated ReflectedDefinition
    val (|PropertySetterWithReflectedDefinition|_|) : propertyInfo:PropertyInfo -> Expr option

/// Active patterns for traversing, visiting, rebuilding and tranforming expressions in a generic way
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ExprShape =
    /// An active pattern that performs a complete decomposition viewing the expression tree as a binding structure
    val (|ShapeVar|ShapeLambda|ShapeCombination|) : 
            input:Expr -> Choice<Var,                // Var
                                 (Var * Expr),       // Lambda
                                 (obj * list<Expr>)> // ConstApp
    /// Re-build combination expressions. The first parameter should be an object
    /// returned by the <c>ShapeCombination</c> case of the active pattern in this module.
    val RebuildShapeCombination  : shape:obj * arguments:list<Expr> -> Expr

#endif
