//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

#nowarn "25" // Incomplete match expressions 
#nowarn "35" // This construct is deprecated: the treatment of this operator is now handled directly by the F# compiler and its meaning may not be redefined.
#nowarn "44" // This construct is deprecated. This function is for use by compiled F# code and should not be used directly
#nowarn "52" // The value has been copied to ensure the original is not mutated by this operation
#nowarn "61" // The containing type can use 'null' as a representation value for its nullary union case. This member will be compiled as a static member.
#nowarn "62" // This construct is for compatibility with OCaml. The syntax 'module ... : sig .. end' is deprecated unless OCaml compatibility is enabled. Consider using 'module ... = begin .. end'.
#nowarn "69" // Interface implementations in augmentations are now deprecated. Interface implementations should be given on the initial declaration of a type.
#nowarn "77" // Member constraints with the name 'Exp' are given special status by the F# compiler as certain .NET types are implicitly augmented with this member. This may result in compilation failures if you attempt to invoke the member constraint from your own code.

namespace Microsoft.FSharp.Core

    //-------------------------------------------------------------------------
    // Basic type abbreviations

    open System
    open System.Collections
    open System.Collections.Generic
    open System.Diagnostics
    open System.Globalization
    open System.Text
    
    
    type obj = System.Object
    type exn = System.Exception
    type nativeint = System.IntPtr
    type unativeint = System.UIntPtr
    type string = System.String
    type float32 = System.Single
    type float = System.Double
    type single = System.Single
    type double = System.Double
    type sbyte = System.SByte
    type byte = System.Byte
    type int8 = System.SByte
    type uint8 = System.Byte
    type int16 = System.Int16
    type uint16 = System.UInt16
    type int32 = System.Int32
    type uint32 = System.UInt32
    type int64 = System.Int64
    type uint64 = System.UInt64
    type char = System.Char
    type bool = System.Boolean
    type decimal = System.Decimal
    type int = int32

    type ``[]``<'T> = (# "!0[]" #)
    type ``[,]``<'T> = (# "!0[0 ...,0 ...]" #)
    type ``[,,]``<'T> = (# "!0[0 ...,0 ...,0 ...]" #)
    type ``[,,,]``<'T> = (# "!0[0 ...,0 ...,0 ...,0 ...]" #)

    type array<'T> = 'T[]

    type byref<'T> = (# "!0&" #)

    type nativeptr<'T> = (# "native int" #)
    type ilsigptr<'T> = (# "!0*" #)




#if FX_ATLEAST_40
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

    //-------------------------------------------------------------------------
    // Unit

    and [<CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")>] // No op_equality on unit  
        Unit =
        { dummy:int }
        override x.GetHashCode() = 0
        override x.Equals(obj:obj) = 
            match obj with null -> true | :? Unit -> true | _ -> false
        interface System.IComparable with 
            member x.CompareTo(obj:obj) = 0
        
    and unit = Unit

    //-------------------------------------------------------------------------
    // .NET enumerations

    type SourceConstructFlags = 
       | None = 0
       | SumType = 1
       | RecordType = 2
       | ObjectType = 3
       | Field = 4
       | Exception = 5
       | Closure = 6
       | Module = 7
       | UnionCase = 8
       | Value = 9
       | KindMask = 31
       | NonpublicRepresentation = 32

    [<Flags>]
    type CompilationRepresentationFlags = 
       | None = 0
       | Static = 1
       | Instance = 2      
       | ModuleSuffix = 4  // append 'Module' to the end of a non-unique module
       | UseNullAsTrueValue = 8  // Note if you change this then change CompilationRepresentationFlags_PermitNull further below
       | Event = 16


    [<AttributeUsage(AttributeTargets.Class,AllowMultiple=false)>]
    type SealedAttribute(value:bool) =
        inherit System.Attribute()
        member x.Value = value
        new() = new SealedAttribute(true)
      
    [<AttributeUsage(AttributeTargets.Class,AllowMultiple=false)>]
    [<Sealed>]
    type AbstractClassAttribute() =
        inherit System.Attribute()
      
    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type DefaultAugmentationAttribute(value:bool) = 
        inherit System.Attribute()
        member x.Value = value

    [<AttributeUsage (AttributeTargets.Property,AllowMultiple=false)>]  
    [<Sealed>]
    type CLIEventAttribute() = 
        inherit System.Attribute()

    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type AutoSerializableAttribute(value:bool) = 
        inherit System.Attribute()
        member x.Value = value

    [<AttributeUsage (AttributeTargets.Field,AllowMultiple=false)>]  
    [<Sealed>]
    type DefaultValueAttribute(check:bool) = 
        inherit System.Attribute()
        member x.Check = check
        new() = new DefaultValueAttribute(true)

    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type EntryPointAttribute() = 
        inherit System.Attribute()

    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type ReferenceEqualityAttribute(value:bool) = 
        inherit System.Attribute()
        member x.Value = value
        new() = ReferenceEqualityAttribute(true)

    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type StructuralComparisonAttribute(value:bool) = 
        inherit System.Attribute()
        member x.Value = value
        new() = StructuralComparisonAttribute(true)

    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type StructuralEqualityAttribute(value:bool) = 
        inherit System.Attribute()
        member x.Value = value
        new() = StructuralEqualityAttribute(true)

    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type OverloadIDAttribute(uniqueName:string)  =
        inherit System.Attribute()
        member x.UniqueName = uniqueName

    [<AttributeUsage (AttributeTargets.Method,AllowMultiple=false)>]  
    [<Sealed>]
    type ReflectedDefinitionAttribute() =
        inherit System.Attribute()

    [<AttributeUsage (AttributeTargets.Struct,AllowMultiple=false)>]  
    [<Sealed>]
    type StructAttribute() =
        inherit System.Attribute()

    [<AttributeUsage (AttributeTargets.GenericParameter ||| AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type MeasureAttribute() =
        inherit System.Attribute()

    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type MeasureAnnotatedAbbreviationAttribute() =
        inherit System.Attribute()

    [<AttributeUsage (AttributeTargets.Interface,AllowMultiple=false)>]  
    [<Sealed>]
    type InterfaceAttribute() =
        inherit System.Attribute()

    [<AttributeUsage (AttributeTargets.Class,AllowMultiple=false)>]  
    [<Sealed>]
    type ClassAttribute() =
        inherit System.Attribute()

    [<AttributeUsage(AttributeTargets.Field,AllowMultiple=false)>]
    [<Sealed>]
    type LiteralAttribute() =
        inherit System.Attribute()

    [<AttributeUsage(AttributeTargets.Assembly,AllowMultiple=false)>]
    [<Sealed>]
    type FSharpInterfaceDataVersionAttribute(major:int,minor:int,release:int)  =
        inherit System.Attribute()
        member x.Major = major
        member x.Minor = minor
        member x.Release = release

    [<AttributeUsage(AttributeTargets.All,AllowMultiple=false)>]
    [<Sealed>]
    type CompilationMappingAttribute(sourceConstructFlags:SourceConstructFlags,
                                     variantNumber:int,
                                     sequenceNumber:int)  =
        inherit System.Attribute()
        member x.SourceConstructFlags = sourceConstructFlags
        member x.SequenceNumber = sequenceNumber
        member x.VariantNumber = variantNumber
        new(sourceConstructFlags) = CompilationMappingAttribute(sourceConstructFlags,0,0)
        new(sourceConstructFlags,sequenceNumber) = CompilationMappingAttribute(sourceConstructFlags,0,sequenceNumber)

    //-------------------------------------------------------------------------
    [<AttributeUsage(AttributeTargets.All,AllowMultiple=false)>]
    [<Sealed>]
    type CompilationRepresentationAttribute (flags : CompilationRepresentationFlags) =
        inherit System.Attribute()
        member x.Flags = flags

    [<AttributeUsage(AttributeTargets.All,AllowMultiple=false)>]
    [<Sealed>]
    type ExperimentalAttribute(message:string) =
        inherit System.Attribute()
        member x.Message = message

    [<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Interface ||| AttributeTargets.Struct ||| AttributeTargets.Delegate ||| AttributeTargets.Enum,AllowMultiple=false)>]
    [<Sealed>]
    type StructuredFormatDisplayAttribute(value:string) =
        inherit System.Attribute()
        member x.Value = value

    [<AttributeUsage(AttributeTargets.All,AllowMultiple=false)>]
    [<Sealed>]
    type OCamlCompatibilityAttribute(message:string) =
        inherit System.Attribute()
        member x.Message = message
        new() = OCamlCompatibilityAttribute(null)

    [<AttributeUsage(AttributeTargets.Method,AllowMultiple=false)>]
    [<Sealed>]
    type UnverifiableAttribute() =
        inherit System.Attribute()

    [<AttributeUsage(AttributeTargets.Method,AllowMultiple=false)>]
    [<Sealed>]
    type NoDynamicInvocationAttribute() =
        inherit System.Attribute()

    [<AttributeUsage(AttributeTargets.Parameter,AllowMultiple=false)>]
    [<Sealed>]
    type OptionalArgumentAttribute() =
        inherit System.Attribute()
      
    [<AttributeUsage(AttributeTargets.Method,AllowMultiple=false)>]
    [<Sealed>]
    type GeneralizableValueAttribute() =
        inherit System.Attribute()
      
    [<AttributeUsage(AttributeTargets.Method,AllowMultiple=false)>]
    [<Sealed>]
    type RequiresExplicitTypeArgumentsAttribute() =
        inherit System.Attribute()
      
    [<AttributeUsage(AttributeTargets.Class,AllowMultiple=false)>]
    [<Sealed>]
    type RequireQualifiedAccessAttribute() =
        inherit System.Attribute()
      
    [<AttributeUsage (AttributeTargets.Class ||| AttributeTargets.Assembly,AllowMultiple=true)>]  
    [<Sealed>]
    type AutoOpenAttribute(path:string) =
        inherit System.Attribute()
        member x.Path = path
        new() =  AutoOpenAttribute("")

    [<MeasureAnnotatedAbbreviation>] type float<[<Measure>] 'u> = float 
    [<MeasureAnnotatedAbbreviation>] type float32<[<Measure>] 'u> = float32
    [<MeasureAnnotatedAbbreviation>] type decimal<[<Measure>] 'u> = decimal
    [<MeasureAnnotatedAbbreviation>] type int<[<Measure>] 'u> = int
    [<MeasureAnnotatedAbbreviation>] type int8<[<Measure>] 'u> = int8
    [<MeasureAnnotatedAbbreviation>] type int16<[<Measure>] 'u> = int16
    [<MeasureAnnotatedAbbreviation>] type int64<[<Measure>] 'u> = int64


    module BasicInlinedOperations =  
        let inline unboxPrim<'T>(x:obj) = (# "unbox.any !0" type ('T) x : 'T #)
        let inline box     (x:'T) = (# "box !0" type ('T) x : obj #)
        let inline not     (b:bool) = (# "ceq" b false : bool #)
        let inline (=)     (x:int)   (y:int)    = (# "ceq" x y : bool #) 
        let inline (<>)    (x:int)   (y:int)    = not(# "ceq" x y : bool #) 
        let inline (>=)    (x:int)   (y:int)    = not(# "clt" x y : bool #)
        let inline (>=.)   (x:int64) (y:int64)  = not(# "clt" x y : bool #)
        let inline (>=...) (x:char)  (y:char)   = not(# "clt" x y : bool #)
        let inline (<=...) (x:char)  (y:char)   = not(# "cgt" x y : bool #)

        let inline (/)     (x:int)    (y:int)    = (# "div" x y : int #)
        let inline (+)     (x:int)    (y:int)    = (# "add" x y : int #)
        let inline (+.)    (x:int64)  (y:int64)  = (# "add" x y : int64 #)
        let inline (+..)   (x:uint64) (y:uint64) = (# "add" x y : uint64 #)
        let inline ( *. )  (x:int64)  (y:int64)  = (# "mul" x y : int64 #)
        let inline ( *.. ) (x:uint64) (y:uint64) = (# "mul" x y : uint64 #)
        let inline (^)     (x:string) (y:string) = System.String.Concat(x,y)
        let inline (<<<)   (x:int)    (y:int)    = (# "shl" x y : int #)
        let inline ( * )   (x:int)    (y:int)    = (# "mul" x y : int #)
        let inline (-)     (x:int)    (y:int)    = (# "sub" x y : int #)
        let inline (-.)    (x:int64)  (y:int64)  = (# "sub" x y : int64 #)
        let inline (-..)   (x:uint64) (y:uint64) = (# "sub" x y : uint64 #)
        let inline (>)     (x:int)    (y:int)    = (# "cgt" x y : bool #)
        let inline (<)     (x:int)    (y:int)    = (# "clt" x y : bool #)
        
        [<NoDynamicInvocation>]
        let inline rethrow() = unboxPrim(# "rethrow ldnull" : System.Object #)
        
        let inline int_of_byte (b:byte) = (# "" b : int #)
        let inline raise (e: System.Exception) = (# "throw" e : 'U #)
        let inline length (x: 'T[]) = (# "ldlen conv.i4" x : int #)
        let inline zero_create (n:int) = (# "newarr !0" type ('T) n : 'T[] #)
        let inline get (arr: 'T[]) (n:int) =  (# "ldelem.any !0" type ('T) arr n : 'T #)
        let set (arr: 'T[]) (n:int) (x:'T) =  (# "stelem.any !0" type ('T) arr n x #)


        let inline objEq (xobj:obj) (yobj:obj) = (# "ceq" xobj yobj : bool #)
        let inline int64Eq (x:int64) (y:int64) = (# "ceq" x y : bool #) 
        let inline int32Eq (x:int32) (y:int32) = (# "ceq" x y : bool #) 
        let inline floatEq (x:float) (y:float) = (# "ceq" x y : bool #) 
        let inline float32Eq (x:float32) (y:float32) = (# "ceq" x y : bool #) 
        let inline charEq (x:char) (y:char) = (# "ceq" x y : bool #) 
        let inline intOrder (x:int) (y:int) = if (# "clt" x y : bool #) then (0-1) else (# "cgt" x y : int #)
        let inline int64Order (x:int64) (y:int64) = if (# "clt" x y : bool #) then (0-1) else (# "cgt" x y : int #)
        let inline byteOrder (x:byte) (y:byte) = if (# "clt" x y : bool #) then (0-1) else (# "cgt" x y : int #)
        let inline byteEq (x:byte) (y:byte) = (# "ceq" x y : bool #) 
        let inline int64 (x:int) = (# "conv.i8" x  : int64 #)
        let inline int32 (x:int64) = (# "conv.i4" x  : int32 #)

        let inline typeof<'T> =
            let tok = (# "ldtoken !0" type('T) : System.RuntimeTypeHandle #)
            System.Type.GetTypeFromHandle(tok) 

        let inline typedefof<'T> = 
            let ty = typeof<'T> 
            if ty.IsGenericType then ty.GetGenericTypeDefinition() else ty
        
        let inline sizeof<'T>  =
            (# "sizeof !0" type('T) : int #) 

        let inline unsafeDefault<'T> : 'T = (# "ilzero !0" type ('T) : 'T #)
        let inline isinstPrim<'T>(x:obj) = (# "isinst !0" type ('T) x : obj #)
        let inline castclassPrim<'T>(x:obj) = (# "castclass !0" type ('T) x : 'T #)
        let inline notnullPrim<'T when 'T : not struct>(x:'T) = (# "ldnull cgt.un" x : bool #)
        let inline iscastPrim<'T when 'T : not struct>(x:obj) = (# "isinst !0" type ('T) x : 'T #)


    open BasicInlinedOperations

#if FX_ATLEAST_40
#else        
    module TupleUtils =
    
        open BasicInlinedOperations

        // adapted from System.Tuple::CombineHashCodes
        let inline mask (n:int) (m:int) = (# "and" n m : int #)
        let inline opshl (x:int) (n:int) : int =  (# "shl" x (mask n 31) : int #)
        let inline opxor (x:int) (y:int) : int = (# "xor" x y : int32 #)
        let inline combine_tuple_hashes (h1 : int) (h2 : int) = (opxor ((opshl h1  5) + h1)  h2)

        let combine_tuple_hash_codes (codes : int []) =
            let mutable (num : int32) = codes.Length - 1
            
            while (num > 1) do
                let mutable i = 0
                while ((i * 2) < (num+1)) do
                    let index = i * 2
                    let num' = index + 1
                    if index = num then
                        set codes i (get codes index)
                        num <- i
                    else
                        set codes i (combine_tuple_hashes (get codes index) (get codes num))
                        if num' = num then
                            num <- i
                    i <- i + 1
            combine_tuple_hashes (get codes 0) (get codes 1)


    type Tuple<'T1>(t1:'T1) = 
        member t.Item1 = t1
        interface IStructuralComparable 
        interface IStructuralEquatable 
        interface IComparable 
            
#if TUPLE_STRUXT
    // NOTE: Tuple`2 is a struct type. 
    // WARNING: If you change additional tuple types to be structs then you must change 'highestTupleStructType' in tastops.ml
#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("({Item1},{Item2})")>]
#endif
    [<Struct>]
    type Tuple<'T1,'T2> = 
        new (v1,v2) = { Item1 = v1; Item2 = v2 }
        val Item1 : 'T1 
        val Item2 : 'T2
#else
    type Tuple<'T1,'T2>(t1:'T1, t2:'T2) =  
        member t.Item1 = t1
        member t.Item2 = t2
        interface IStructuralComparable 
        interface IStructuralEquatable 
        interface IComparable 
#endif

#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("({Item1},{Item2},{Item3})")>]
#endif
    type Tuple<'T1,'T2,'T3>(t1:'T1,t2:'T2,t3:'T3) =       
        member t.Item1 = t1
        member t.Item2 = t2
        member t.Item3 = t3
        interface IStructuralComparable 
        interface IStructuralEquatable 
        interface IComparable 

#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("({Item1},{Item2},{Item3},{Item4})")>]
#endif
    type Tuple<'T1,'T2,'T3,'T4>(t1:'T1,t2:'T2,t3:'T3,t4:'T4) = 
        member t.Item1 = t1
        member t.Item2 = t2
        member t.Item3 = t3
        member t.Item4 = t4
        interface IStructuralComparable 
        interface IStructuralEquatable 
        interface IComparable 

#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("({Item1},{Item2},{Item3},{Item4},{Item5})")>]
#endif
    type Tuple<'T1,'T2,'T3,'T4,'T5>(t1:'T1,t2:'T2,t3:'T3,t4:'T4,t5:'T5) = 
        member t.Item1 = t1
        member t.Item2 = t2
        member t.Item3 = t3
        member t.Item4 = t4
        member t.Item5 = t5
        interface IStructuralComparable 
        interface IStructuralEquatable 
        interface IComparable 

#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("({Item1},{Item2},{Item3},{Item4},{Item5},{Item6})")>]
#endif
    type Tuple<'T1,'T2,'T3,'T4,'T5,'T6>(t1:'T1,t2:'T2,t3:'T3,t4:'T4,t5:'T5,t6:'T6) = 
        member t.Item1 = t1
        member t.Item2 = t2
        member t.Item3 = t3
        member t.Item4 = t4
        member t.Item5 = t5
        member t.Item6 = t6
        interface IStructuralComparable 
        interface IStructuralEquatable 
        interface IComparable 

#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("({Item1},{Item2},{Item3},{Item4},{Item5},{Item6},{Item7})")>]
#endif
    type Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7>(t1:'T1,t2:'T2,t3:'T3,t4:'T4,t5:'T5,t6:'T6,t7:'T7) = 
        member t.Item1 = t1
        member t.Item2 = t2
        member t.Item3 = t3
        member t.Item4 = t4
        member t.Item5 = t5
        member t.Item6 = t6
        member t.Item7 = t7
        interface IStructuralComparable 
        interface IStructuralEquatable 
        interface IComparable 
            
#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("({Item1},{Item2},{Item3},{Item4},{Item5},{Item6},{Item7},{Rest})")>]
#endif
    type Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'TRest>(t1:'T1,t2:'T2,t3:'T3,t4:'T4,t5:'T5,t6:'T6,t7:'T7,rest:'TRest) = 
        member t.Item1 = t1
        member t.Item2 = t2
        member t.Item3 = t3
        member t.Item4 = t4
        member t.Item5 = t5
        member t.Item6 = t6
        member t.Item7 = t7
        member t.Rest = rest
        interface IStructuralComparable 
        interface IStructuralEquatable 
        interface IComparable 
#endif


    //-------------------------------------------------------------------------
    // The main aim here is to bootsrap the definition of structural hashing 
    // and comparison.  Calls to these form part of the auto-generated 
    // code for each new datatype.

    module LanguagePrimitives =  
   

        let inline DefaultValue<'T when 'T : null> = unsafeDefault<'T>

        [<CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")>]  // nested module OK              
        module IntrinsicOperators =        
            //-------------------------------------------------------------------------
            // Lazy and/or.  Laziness added by the F# compiler.
            
            let (&) x y = if x then y else false
            let (&&) x y = if x then y else false
            let (or) x y = if x then true else y
            let (||) x y = if x then true else y
            
            //-------------------------------------------------------------------------
            // Address-of
            // Note, "raise<'T> : exn -> 'T" is manually inlined below.
            // Byref usage checks prohibit type instantiations involving byrefs.

            [<NoDynamicInvocation>]
            let inline (~&)  (x : 'T) : 'T byref     = 
                 let e = new System.ArgumentException("first class uses of address-of operators are not permitted") in
                 (# "throw" (e :> System.Exception) : 'T byref #)
                 
            [<NoDynamicInvocation>]
            let inline (~&&) (x : 'T) : nativeptr<'T> = 
                 let e = new System.ArgumentException("first class uses of address-of operators are not permitted") in
                 (# "throw" (e :> System.Exception) : nativeptr<'T> #)     
          
        
        open IntrinsicOperators
        [<CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")>]  // nested module OK
        module IntrinsicFunctions =        
            
            //-------------------------------------------------------------------------
            // Unboxing, type casts, type tests

            type TypeNullnessSemantics = int
            // .NET reference types
            let TypeNullnessSemantics_NullIsExtraValue = 1
            // F# types with [<UseNullAsTrueValue>]
            let TypeNullnessSemantics_NullTrueValue = 2
            // F# record, union, tuple, function types
            let TypeNullnessSemantics_NullNotLiked = 3
            // structs
            let TypeNullnessSemantics_NullNever = 4
            
            // duplicated from above since we're using integers in this section
            let CompilationRepresentationFlags_PermitNull = 8

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]             
            type TypeInfo<'T>() = 
               // Compute an on-demand per-instantiation static field
               static let info = 
                         let ty = typeof<'T>
                         if ty.IsValueType 
                         then TypeNullnessSemantics_NullNever else
                         let mappingAttrs = ty.GetCustomAttributes(typeof<CompilationMappingAttribute>,false)
                         if mappingAttrs.Length = 0 
                         then TypeNullnessSemantics_NullIsExtraValue
                         elif ty.Equals(typeof<unit>) then 
                             TypeNullnessSemantics_NullTrueValue
                         else
                             let reprAttrs = ty.GetCustomAttributes(typeof<CompilationRepresentationAttribute>,false)
                             if reprAttrs.Length = 0 
                             then TypeNullnessSemantics_NullNotLiked 
                             else
                                 let reprAttr = get reprAttrs 0
                                 let reprAttr = (# "unbox.any !0" type (CompilationRepresentationAttribute) reprAttr : CompilationRepresentationAttribute #)
                                 if (# "and" reprAttr.Flags CompilationRepresentationFlags_PermitNull : int #) = 0
                                 then TypeNullnessSemantics_NullNotLiked
                                 else TypeNullnessSemantics_NullTrueValue

               // Publish the results of that compuation
               static member TypeInfo = info
                         

            // Note: cheap nullness test for generic value:
            //  IL_0000:  ldarg.1
            //  IL_0001:  box        !TKey
            //  IL_0006:  brtrue.s   IL_000e

            // worst case: nothing known about source or destination
            let UnboxGeneric<'T>(x:obj) = 
                if notnullPrim(x) or TypeInfo<'T>.TypeInfo <> TypeNullnessSemantics_NullNotLiked then 
                    unboxPrim<'T>(x)
                else
                    //System.Console.WriteLine("UnboxGeneric, x = {0}, 'T = {1}", x, typeof<'T>)
                    raise (System.NullReferenceException()) 

            // better: source is NOT TypeNullnessSemantics_NullNotLiked 
            let inline UnboxFast<'T>(x:obj) = 
                // assert not(TypeInfo<'T>.TypeInfo = TypeNullnessSemantics_NullNotLiked)
                unboxPrim<'T>(x)


            // worst case: nothing known about source or destination
            let TypeTestGeneric<'T>(x:obj) = 
                if notnullPrim(isinstPrim<'T>(x)) then true
                elif notnullPrim(x) then false
                else (TypeInfo<'T>.TypeInfo = TypeNullnessSemantics_NullTrueValue)

            // quick entry: source is NOT TypeNullnessSemantics_NullTrueValue 
            let inline TypeTestFast<'T>(x:obj) = 
                //assert not(TypeInfo<'T>.TypeInfo = TypeNullnessSemantics_NullTrueValue)
                notnullPrim(isinstPrim<'T>(x)) 

            let Dispose<'T when 'T :> System.IDisposable >(x:'T) = 
                match box x with 
                | null -> ()
                | _ -> x.Dispose()

            let inline MakeDecimal lo med hi isNegative scale =  new System.Decimal(lo,med,hi,isNegative,scale)

            let inline GetString (s: string) (n:int) =   s.Chars(n)

            let inline CreateInstance<'T when 'T : (new : unit -> 'T) >() = 
                 (System.Activator.CreateInstance() : 'T)

            [<NoDynamicInvocation>]
            let inline retype (x:'T) : 'U = (# "" x : 'U #)

            let inline GetArray (arr: 'T array) (n:int) =  (# "ldelem.any !0" type ('T) arr n : 'T #)  
            let inline SetArray (arr: 'T array) (n:int,x:'T) =  (# "stelem.any !0" type ('T) arr n x #)  


            let inline GetArraySub arr (start:int) (len:int) =
                let len1 = (if len < 0 then 0 else len)
                let dst = zero_create len   
                for i = 0 to len - 1 do 
                    SetArray dst (i,GetArray arr (start + i))
                dst

            let inline SetArraySub arr (start:int) (len:int) (src:_[]) =
                for i = 0 to len - 1 do 
                    SetArray arr (start+i,GetArray src i)


            let inline GetArray2D (arr: 'T[,])  (n1:int,n2:int)                 = (# "ldelem.multi 2 !0" type ('T) arr n1 n2 : 'T #)  
            let inline SetArray2D (arr: 'T[,])  (n1:int,n2:int,x:'T)          = (# "stelem.multi 2 !0" type ('T) arr n1 n2 x #)  
            let inline GetArray2DLength1 (arr: 'T[,]) =  (# "ldlen.multi 2 0" arr : int #)  
            let inline GetArray2DLength2 (arr: 'T[,]) =  (# "ldlen.multi 2 1" arr : int #)  

            let inline Array2DZeroCreate (n:int) (m:int) = (# "newarr.multi 2 !0" type ('T) n m : 'T[,] #)
            let GetArray2DSub (src: 'T[,]) src1 src2 len1 len2 =
                let len1 = (if len1 < 0 then 0 else len1)
                let len2 = (if len2 < 0 then 0 else len2)
                let dst = Array2DZeroCreate len1 len2
                for i = 0 to len1 - 1 do
                    for j = 0 to len2 - 1 do
                        SetArray2D dst (i,j,GetArray2D src (src1 + i,src2 + j))
                dst

            let SetArray2DSub (dst: 'T[,]) src1 src2 len1 len2 src =
                for i = 0 to len1 - 1 do
                    for j = 0 to len2 - 1 do
                        SetArray2D dst (src1+i,src2+j,GetArray2D src (i,j))


            let inline GetArray3D (arr: 'T[,,]) (n1:int,n2:int,n3:int)        = (# "ldelem.multi 3 !0" type ('T) arr n1 n2 n3 : 'T #)  
            let inline SetArray3D (arr: 'T[,,]) (n1:int,n2:int,n3:int,x:'T) = (# "stelem.multi 3 !0" type ('T) arr n1 n2 n3 x #)  
            let inline GetArray3DLength1 (arr: 'T[,,]) =  (# "ldlen.multi 3 0" arr : int #)  
            let inline GetArray3DLength2 (arr: 'T[,,]) =  (# "ldlen.multi 3 1" arr : int #)  
            let inline GetArray3DLength3 (arr: 'T[,,]) =  (# "ldlen.multi 3 2" arr : int #)  

            let inline Array3DZeroCreate (n1:int) (n2:int) (n3:int) = (# "newarr.multi 3 !0" type ('T) n1 n2 n3 : 'T[,,] #)

            let GetArray3DSub (src: 'T[,,]) src1 src2 src3 len1 len2 len3 =
                let len1 = (if len1 < 0 then 0 else len1)
                let len2 = (if len2 < 0 then 0 else len2)
                let len3 = (if len3 < 0 then 0 else len3)
                let dst = Array3DZeroCreate len1 len2 len3
                for i = 0 to len1 - 1 do
                    for j = 0 to len2 - 1 do
                        for k = 0 to len3 - 1 do
                            SetArray3D dst (i,j,k,GetArray3D src (src1+i,src2+j,src3+k))
                dst

            let SetArray3DSub (dst: 'T[,,]) src1 src2 src3 len1 len2 len3 src =
                for i = 0 to len1 - 1 do
                    for j = 0 to len2 - 1 do
                        for k = 0 to len3 - 1 do
                            SetArray3D dst (src1+i,src2+j,src3+k,GetArray3D src (i,j,k))



            let inline GetArray4D (arr: 'T[,,,]) (n1:int,n2:int,n3:int,n4:int)       = (# "ldelem.multi 4 !0" type ('T) arr n1 n2 n3 n4 : 'T #)  
            let inline SetArray4D (arr: 'T[,,,]) (n1:int,n2:int,n3:int,n4:int,x:'T) = (# "stelem.multi 4 !0" type ('T) arr n1 n2 n3 n4 x #)  
            let inline Array4DLength1 (arr: 'T[,,,]) =  (# "ldlen.multi 4 0" arr : int #)  
            let inline Array4DLength2 (arr: 'T[,,,]) =  (# "ldlen.multi 4 1" arr : int #)  
            let inline Array4DLength3 (arr: 'T[,,,]) =  (# "ldlen.multi 4 2" arr : int #)  
            let inline Array4DLength4 (arr: 'T[,,,]) =  (# "ldlen.multi 4 3" arr : int #)  

            let inline Array4DZeroCreate (n1:int) (n2:int) (n3:int) (n4:int) = (# "newarr.multi 4 !0" type ('T) n1 n2 n3 n4 : 'T[,,,] #)

            let GetArray4DSub (src: 'T[,,,]) src1 src2 src3 src4 len1 len2 len3 len4 =
                let len1 = (if len1 < 0 then 0 else len1)
                let len2 = (if len2 < 0 then 0 else len2)
                let len3 = (if len3 < 0 then 0 else len3)
                let len4 = (if len4 < 0 then 0 else len4)
                let dst = Array4DZeroCreate len1 len2 len3 len4
                for i = 0 to len1 - 1 do
                    for j = 0 to len2 - 1 do
                        for k = 0 to len3 - 1 do
                          for m = 0 to len4 - 1 do
                              SetArray4D dst (i,j,k,m,GetArray4D src (src1+i,src2+j,src3+k,src3+m))
                dst

            let SetArray4DSub (dst: 'T[,,,]) src1 src2 src3 src4 len1 len2 len3 len4 src =
                for i = 0 to len1 - 1 do
                    for j = 0 to len2 - 1 do
                        for k = 0 to len3 - 1 do
                          for m = 0 to len4 - 1 do
                            SetArray4D dst (src1+i,src2+j,src3+k,src4+m,GetArray4D src (i,j,k,m))


        module HashCompare = 
        
            let PhysicalEqualityObj (xobj:obj) (yobj:obj) = 
                  objEq xobj yobj 
                   || (match xobj with 
                        | null -> false
                        | :? System.ValueType -> xobj.Equals(yobj)
                        | _ -> false)

            // NOTE: compiler/optimizer is aware of this function and optimizes calls to it in many situations
            // where it is known that PhysicalEqualityObj is identical to reference comparison
            let PhysicalEqualityIntrinsic (x:'T) (y:'T) = 
                  PhysicalEqualityObj (box x) (box y)

            let rec inline PhysicalEqualityFast (x:'T) (y:'T) = 
                  PhysicalEqualityIntrinsic x y
                  when 'T : bool   = (# "ceq" x y : bool #)
                  when 'T : int    = (# "ceq" x y : bool #)
                  when 'T : sbyte  = (# "ceq" x y : bool #)
                  when 'T : int16  = (# "ceq" x y : bool #)
                  when 'T : int32  = (# "ceq" x y : bool #)
                  when 'T : int64  = (# "ceq" x y : bool #)
                  when 'T : byte   = (# "ceq" x y : bool #)
                  when 'T : uint16 = (# "ceq" x y : bool #)
                  when 'T : uint32 = (# "ceq" x y : bool #)
                  when 'T : uint64 = (# "ceq" x y : bool #)
                  when 'T : float  = (# "ceq" x y : bool #)
                  when 'T : char   = (# "ceq" x y : bool #)

            let PhysicalHashObj (xobj: obj) = 
#if FX_NO_GET_HASH_CODE_HELPER
                  xobj.GetHashCode()
#else
                  match xobj with
                  | :? System.ValueType as v -> v.GetHashCode()
                  | _ -> 
                   System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(xobj)
#endif
          
            let PhysicalHashIntrinsic (x: 'T) = PhysicalHashObj (box x)
            
            let inline PhysicalHashFast (x: 'T) = 
                  PhysicalHashIntrinsic x
                  // NOTE: only specialziations on value types are allowed, and 
                  // any type specializations on value types must return identical values to 
                  // the mscorlib implementations of the ::GetHashCode methods on the given types. 
                  when 'T : bool   = (# "" x : int #)
                  when 'T : int    = (# "" x : int #)
                  when 'T : int32  = (# "" x : int #)
                  when 'T : byte   = (# "" x : int #)
                  when 'T : uint32 = (# "" x : int #)


            //-------------------------------------------------------------------------
            // Bi-modal generic comparison helper implementation.
            //
            // For structured data (records, tuples, unions) the IComparable implementation on 
            // types is run in either Equivalence Relation or Partial Equivalence Relation (PER) mode.
            //
            // In the second mode, generic comparison of two NaN values is recorded by clearing
            // the PERMode.mode variable. PER mode can also be thought of as "no pair of NaNs detected yet" mode.
            // Entry points that care about NaN pairs set the PERMode.mode variable and if it remains un-set by
            // the end of their execution then no NaN pair was detected.
            //
            // From the library spec point of view:
            //   - GenericComparisonWithComparer will (a) make IComparable calls in the same mode as 
            //     called and (b) if in PER node then a NaN pair will cause the outer operation that entered
            //     PER mode to return false.
            //
            //   - GenericLessThan, GenericGreaterThan etc. enters PER mode and retores the previous PER mode on exit
            //
            //   - GenericComparison clears PER mode and retores the previous PER mode on exit
            //
            //   - GenericEquality is implemented via obj.Equals and always has PER semantics
            //
            // Note that some representations chosen by F# are legitimately allowed to be null, e.g. the empty list. 
            // However, because null values don't support the polymorphic virtual comparison operation CompareTo 
            // the test for nullness must be made on the caller side.
            //------------------------------------------------------------------------- 

            
            let FailGenericComparison (obj: obj)  = 
                raise (new System.ArgumentException("Failure during generic comparison: the type "^obj.GetType().ToString()^" does not implement the System.IComparable interface. This error may be arise from the use of a function such as 'compare', 'max' or 'min' or a data structure such as 'Set' or 'Map' whose keys contain instances of this type"))
            
            type GenericComparer(throwsOnPER:bool) = 
                
                member c.ThrowsOnPER() = throwsOnPER
                abstract CompareC : obj * obj -> int

                interface System.Collections.IComparer 
               
            type NaNException(s) =
                inherit System.ArgumentException(s)                                                 
                    
            let rec GenericCompare (comp:GenericComparer) (xobj:obj,yobj:obj) = 
                (*if objEq xobj yobj then 0 else *)
                  //System.Console.WriteLine("xobj = {0}, yobj = {1}, NaNs = {2}, PERMode.mode = {3}", [| xobj; yobj; box PER_NAN_PAIR_DETECTED; box PERMode.mode |])
                  match xobj,yobj with 
                   | null,null -> 0
                   | null,_ -> -1
                   | _,null -> 1
#if INVARIANT_CULTURE_STRING_COMPARISON
                   // Use invariant culture comparison for strings
                   | (:? string as x),(:? string as y) -> System.String.Compare(x, y, false, CultureInfo.InvariantCulture)
#else
                   // Use Ordinal comparison for strings
                   | (:? string as x),(:? string as y) -> System.String.CompareOrdinal(x, y)
#endif
                   // Permit structural comparison on arrays
                   | (:? System.Array as arr1),_ -> 
                       match arr1,yobj with 
                       // Fast path
                       | (:? (obj[]) as arr1) , (:? (obj[]) as arr2)      -> GenericComparisonObjArrayWithComparer comp arr1 arr2
                       // Fast path
                       | (:? (byte[]) as arr1), (:? (byte[]) as arr2)     -> GenericComparisonByteArray arr1 arr2
                       | _                    , (:? System.Array as arr2) -> GenericComparisonArbArrayWithComparer comp arr1 arr2
                       | _ -> FailGenericComparison xobj
                   // Check for IStructuralComparable
                   | (:? IStructuralComparable as x),_ ->
                       x.CompareTo(yobj,comp)
                   // Check for IComparable
                   | (:? System.IComparable as x),_ -> 
                       match xobj,yobj with 
                       | (:? float as x),(:? float as y) -> 
                            if System.Double.IsNaN x && System.Double.IsNaN y && (comp.ThrowsOnPER()) then 
                                raise (NaNException("float NaN Comparison"))
                       | (:? float32 as x),(:? float32 as y) -> 
                            if System.Single.IsNaN x && System.Single.IsNaN y && (comp.ThrowsOnPER()) then 
                                raise (NaNException("float32 NaN Comparison"))
                       | _ -> ()
                       x.CompareTo(yobj)
                   | (:? nativeint as x),(:? nativeint as y) -> if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                   | (:? unativeint as x),(:? unativeint as y) -> if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                   | _,(:? IStructuralComparable as yc) ->
                       let res = yc.CompareTo(xobj,comp)
                       if res < 0 then 1 elif res > 0 then -1 else 0
                   | _,(:? System.IComparable as yc) -> 
                       // Note -c doesn't work here: be careful of comparison function returning minint
                       let c = yc.CompareTo(xobj) in 
                       if c < 0 then 1 elif c > 0 then -1 else 0
                   | _ -> FailGenericComparison xobj

            /// specialcase: Core implementation of structural comparison on arbitrary arrays.
            and GenericComparisonArbArrayWithComparer (comp:GenericComparer) (x:System.Array) (y:System.Array) : int  =
#if FX_NO_ARRAY_LONG_LENGTH            
                if x.Rank = 1 && y.Rank = 1 then 
                    let lenx = x.Length
                    let leny = y.Length 
                    let c = intOrder lenx leny 
                    if c <> 0 then c else
                    let rec check i =
                       if i >= lenx then 0 else 
                       let c = GenericCompare comp ((x.GetValue(i)),(y.GetValue(i)))
                       if c <> 0 then c else check (i + 1)
                    check 0
                elif x.Rank = 2 && y.Rank = 2 then 
                    let lenx0 = x.GetLength(0)
                    let leny0 = y.GetLength(0)
                    let c = intOrder lenx0 leny0 
                    if c <> 0 then c else
                    let lenx1 = x.GetLength(1)
                    let leny1 = y.GetLength(1)
                    let c = intOrder lenx1 leny1 
                    if c <> 0 then c else
                    let basex0 = (x.GetLowerBound(0))
                    let basex1 = (x.GetLowerBound(1))
                    let basey0 = (y.GetLowerBound(0))
                    let basey1 = (y.GetLowerBound(1))
                    let c = intOrder basex0 basey0
                    if c <> 0 then c else
                    let c = intOrder basex1 basey1
                    if c <> 0 then c else
                    let rec check0 i =
                       let rec check1 j = 
                           if j >= lenx1 then 0 else
                           let c = GenericCompare comp ((x.GetValue(i + basex0,j + basex1)), (y.GetValue(i + basey0,j + basey1)))
                           if c <> 0 then c else check1 (j + 1)
                       if i >= lenx0 then 0 else 
                       let c = check1 0
                       if c <> 0 then c else
                       check0 (i + 1)
                    check0 0
                else
                    let c = intOrder x.Rank y.Rank
                    if c <> 0 then c else
                    let ndims = x.Rank
                    // check lengths 
                    let rec precheck k = 
                        if k >= ndims then 0 else
                        let c = intOrder (x.GetLength(k)) (y.GetLength(k))
                        if c <> 0 then c else
                        let c = intOrder (x.GetLowerBound(k)) (y.GetLowerBound(k))
                        if c <> 0 then c else
                        precheck (k+1)
                    let c = precheck 0 
                    if c <> 0 then c else
                    let idxs : int[] = zero_create ndims 
                    let rec checkN k baseIdx i lim =
                       if i >= lim then 0 else
                       set idxs k (baseIdx + i)
                       let c = 
                           if k = ndims - 1
                           then GenericCompare comp ((x.GetValue(idxs)), (y.GetValue(idxs)))
                           else check (k+1) 
                       if c <> 0 then c else 
                       checkN k baseIdx (i + 1) lim
                    and check k =
                       if k >= ndims then 0 else
                       let baseIdx = x.GetLowerBound(k)
                       checkN k baseIdx 0 (x.GetLength(k))
                    check 0
#else
                if x.Rank = 1 && y.Rank = 1 then 
                    let lenx = x.LongLength
                    let leny = y.LongLength 
                    let c = int64Order lenx leny 
                    if c <> 0 then c else
                    let rec check i =
                       if i >=. lenx then 0 else 
                       let c = GenericCompare comp ((x.GetValue(i)), (y.GetValue(i)))
                       if c <> 0 then c else check (i +. 1L)
                    check 0L
                elif x.Rank = 2 && y.Rank = 2 then 
                    let lenx0 = x.GetLongLength(0)
                    let leny0 = y.GetLongLength(0)
                    let c = int64Order lenx0 leny0 
                    if c <> 0 then c else
                    let lenx1 = x.GetLongLength(1)
                    let leny1 = y.GetLongLength(1)
                    let c = int64Order lenx1 leny1 
                    if c <> 0 then c else
                    let basex0 = int64 (x.GetLowerBound(0))
                    let basex1 = int64 (x.GetLowerBound(1))
                    let basey0 = int64 (y.GetLowerBound(0))
                    let basey1 = int64 (y.GetLowerBound(1))
                    let c = int64Order basex0 basey0
                    if c <> 0 then c else
                    let c = int64Order basex1 basey1
                    if c <> 0 then c else
                    let rec check0 i =
                       let rec check1 j = 
                           if j >=. lenx1 then 0 else
                           let c = GenericCompare comp ((x.GetValue(i +. basex0,j +. basex1)), (y.GetValue(i +. basey0,j +. basey1)))
                           if c <> 0 then c else check1 (j +. 1L)
                       if i >=. lenx0 then 0 else 
                       let c = check1 0L
                       if c <> 0 then c else
                       check0 (i +. 1L)
                    check0 0L
                else
                    let c = intOrder x.Rank y.Rank
                    if c <> 0 then c else
                    let ndims = x.Rank
                    // check lengths 
                    let rec precheck k = 
                        if k >= ndims then 0 else
                        let c = int64Order (x.GetLongLength(k)) (y.GetLongLength(k))
                        if c <> 0 then c else
                        let c = intOrder (x.GetLowerBound(k)) (y.GetLowerBound(k))
                        if c <> 0 then c else
                        precheck (k+1)
                    let c = precheck 0 
                    if c <> 0 then c else
                    let idxs : int64[] = zero_create ndims 
                    let rec checkN k baseIdx i lim =
                       if i >=. lim then 0 else
                       set idxs k (baseIdx +. i)
                       let c = 
                           if k = ndims - 1
                           then GenericCompare comp ((x.GetValue(idxs)), (y.GetValue(idxs)))
                           else check (k+1) 
                       if c <> 0 then c else 
                       checkN k baseIdx (i +. 1L) lim
                    and check k =
                       if k >= ndims then 0 else
                       let baseIdx = x.GetLowerBound(k)
                       checkN k (int64 baseIdx) 0L (x.GetLongLength(k))
                    check 0
#endif                
              
            /// optimized case: Core implementation of structural comparison on object arrays.
            and GenericComparisonObjArrayWithComparer (comp:GenericComparer) (x:obj[]) (y:obj[]) : int  =
                let lenx = x.Length 
                let leny = y.Length 
                let c = intOrder lenx leny 
                if c <> 0 then c 
                else
                    let mutable i = 0
                    let mutable res = 0  
                    while i < lenx do 
                        let c = GenericCompare comp ((get x i), (get y i)) 
                        if c <> 0 then (res <- c; i <- lenx) 
                        else i <- i + 1
                    res

            /// optimized case: Core implementation of structural comparison on arrays.
            and GenericComparisonByteArray (x:byte[]) (y:byte[]) : int =
                let lenx = x.Length 
                let leny = y.Length 
                let c = intOrder lenx leny 
                if c <> 0 then c 
                else
                    let mutable i = 0 
                    let mutable res = 0 
                    while i < lenx do 
                        let c = byteOrder (get x i) (get y i) 
                        if c <> 0 then (res <- c; i <- lenx) 
                        else i <- i + 1
                    res

            type GenericComparer with
                
                interface System.Collections.IComparer with
                    override c.Compare(x:obj,y:obj) = GenericCompare c (x,y)
                override c.CompareC(x:obj,y:obj) =  GenericCompare c (x,y)
            
            let fsComparer        = GenericComparer(true)  
            let fsComparerNoThrow = GenericComparer(false) 
                    
            // NOTE: compiler/optimizer is aware of this function and optimizes calls to it in many situations
            // where it is known how GenericComparisonObjBimodal will dispatch via IComparable.
            let GenericComparisonWithComparerIntrinsic<'T> (comp:System.Collections.IComparer) (x:'T) (y:'T) : int = 
                // if 'T = float then x.CompareTo(y)
                // if 'T = float32 then x.CompareTo(y)
                comp.Compare(box x, box y)

            let inline GenericComparisonWithComparerFast<'T> (comp:System.Collections.IComparer) (x:'T) (y:'T) : int = 
                 GenericComparisonWithComparerIntrinsic comp x y
                 when 'T : bool   = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : sbyte  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : int16  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : int32  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : int    = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : int64  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : nativeint  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : byte   = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : uint16 = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : uint32 = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : uint64 = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : unativeint = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 // Multi-modal direct implementation for float/float32 all of "clt", "cgt" and "ceq" may fail on NaN. In this case we must detect our mode
                 // and possibly change PERMode.mode. We do this by bailing out to call GenericComparisonWithComparerIntrinsic
                 when 'T : float  = if (# "clt" x y : bool #) then (-1) else if (# "cgt" x y : bool #) then 1 else if (# "ceq" x y : bool #) then 0 else GenericComparisonWithComparerIntrinsic comp x y
                 when 'T : float32 = if (# "clt" x y : bool #) then (-1) else if (# "cgt" x y : bool #) then 1 else if (# "ceq" x y : bool #) then 0 else GenericComparisonWithComparerIntrinsic comp x y
                 when 'T : char   = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : string = 
#if INVARIANT_CULTURE_STRING_COMPARISON
                     // NOTE: we don't have to null check here because System.String.Compare
                     // gives reliable results on null values.
                     System.String.Compare((# "" x : string #) ,(# "" y : string #), false, CultureInfo.InvariantCulture)
#else
                     // NOTE: we don't have to null check here because System.String.CompareOrdinal
                     // gives reliable results on null values.
                     System.String.CompareOrdinal((# "" x : string #) ,(# "" y : string #))
#endif


            let GenericComparisonIntrinsic<'T> (x:'T) (y:'T) : int = 
                GenericComparisonWithComparerIntrinsic (fsComparerNoThrow :> System.Collections.IComparer) x y


            // Check for 'T = float or 'T = float32 (in case we didn't resolve this statically).
            // In these cases we must use the right "clt" to cope with NaN.  Note "<" etc. take
            // on a different meaning for floats nested inside data structures than when immediately 
            // applied to type float.  This gives us the IEEE semantics when "<" etc. are used on
            // floats, and a sound structural semantics when used on structured data, though 
            // comparisons on structural data containing floats do not get optimized.
            let GenericLessThanIntrinsic (x:'T) (y:'T) = 
                try
                    (# "clt" (GenericComparisonWithComparerIntrinsic fsComparer x y) 0 : bool #)
                with
                    | :? NaNException -> false
                    
            
            let GenericGreaterThanIntrinsic (x:'T) (y:'T) = 
                try
                    (# "cgt" (GenericComparisonWithComparerIntrinsic fsComparer x y) 0 : bool #)
                with
                    | :? NaNException -> false
            
             
            let GenericGreaterOrEqualIntrinsic (x:'T) (y:'T) = 
                try
                    (# "cgt" (GenericComparisonWithComparerIntrinsic fsComparer x y) (-1) : bool #)
                with
                    | :? NaNException -> false
                    
            
            
            let GenericLessOrEqualIntrinsic (x:'T) (y:'T) = 
                try
                    (# "clt" (GenericComparisonWithComparerIntrinsic fsComparer x y) 1 : bool #)
                with
                    | :? NaNException -> false


            /// Core implementation of structural comparison on arbitrary values.
            let inline GenericComparisonFast<'T> (x:'T) (y:'T) : int = 
                 GenericComparisonIntrinsic x y
                 when 'T : bool   = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : sbyte  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : int16  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : int32  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : int    = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : int64  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : nativeint  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : byte   = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : uint16 = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : uint32 = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : uint64 = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : unativeint = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : float  = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : float32 = if (# "clt" x y : bool #) then (-1) else (# "cgt" x y : int #)
                 when 'T : char   = if (# "clt.un" x y : bool #) then (-1) else (# "cgt.un" x y : int #)
                 when 'T : string = 
#if INVARIANT_CULTURE_STRING_COMPARISON
                     // NOTE: we don't have to null check here because System.String.Compare
                     // gives reliable results on null values.
                     System.String.Compare((# "" x : string #) ,(# "" y : string #), false, CultureInfo.InvariantCulture)
#else
                     // NOTE: we don't have to null check here because System.String.CompareOrdinal
                     // gives reliable results on null values.
                     System.String.CompareOrdinal((# "" x : string #) ,(# "" y : string #))
#endif

              
            let inline GenericLessThanFast (x:'T) (y:'T) = 
                GenericLessThanIntrinsic x y
                when 'T : bool   = (# "clt" x y : bool #)
                when 'T : int    = (# "clt" x y : bool #)
                when 'T : sbyte  = (# "clt" x y : bool #)
                when 'T : int16  = (# "clt" x y : bool #)
                when 'T : int32  = (# "clt" x y : bool #)
                when 'T : int64  = (# "clt" x y : bool #)
                when 'T : byte   = (# "clt.un" x y : bool #)
                when 'T : uint16 = (# "clt.un" x y : bool #)
                when 'T : uint32 = (# "clt.un" x y : bool #)
                when 'T : uint64 = (# "clt.un" x y : bool #)
                when 'T : unativeint = (# "clt.un" x y : bool #)
                when 'T : nativeint  = (# "clt" x y : bool #)
                when 'T : float  = (# "clt" x y : bool #) 
                when 'T : float32= (# "clt" x y : bool #) 
                when 'T : char   = (# "clt" x y : bool #)
              
            let inline GenericGreaterThanFast (x:'T) (y:'T) = 
                GenericGreaterThanIntrinsic x y
                when 'T : bool       = (# "cgt" x y : bool #)
                when 'T : int        = (# "cgt" x y : bool #)
                when 'T : sbyte      = (# "cgt" x y : bool #)
                when 'T : int16      = (# "cgt" x y : bool #)
                when 'T : int32      = (# "cgt" x y : bool #)
                when 'T : int64      = (# "cgt" x y : bool #)
                when 'T : nativeint  = (# "cgt" x y : bool #)
                when 'T : byte       = (# "cgt.un" x y : bool #)
                when 'T : uint16     = (# "cgt.un" x y : bool #)
                when 'T : uint32     = (# "cgt.un" x y : bool #)
                when 'T : uint64     = (# "cgt.un" x y : bool #)
                when 'T : unativeint = (# "cgt.un" x y : bool #)
                when 'T : float      = (# "cgt" x y : bool #) 
                when 'T : float32    = (# "cgt" x y : bool #) 
                when 'T : char       = (# "cgt" x y : bool #)

            let inline GenericLessOrEqualFast (x:'T) (y:'T) = 
                GenericLessOrEqualIntrinsic x y
                when 'T : bool       = not (# "cgt" x y : bool #)
                when 'T : int        = not (# "cgt" x y : bool #)
                when 'T : sbyte      = not (# "cgt" x y : bool #)
                when 'T : int16      = not (# "cgt" x y : bool #)
                when 'T : int32      = not (# "cgt" x y : bool #)
                when 'T : int64      = not (# "cgt" x y : bool #)
                when 'T : nativeint  = not (# "cgt" x y : bool #)
                when 'T : byte       = not (# "cgt.un" x y : bool #)
                when 'T : uint16     = not (# "cgt.un" x y : bool #)
                when 'T : uint32     = not (# "cgt.un" x y : bool #)
                when 'T : uint64     = not (# "cgt.un" x y : bool #)
                when 'T : unativeint = not (# "cgt.un" x y : bool #)
                when 'T : float      = not (# "cgt.un" x y : bool #) 
                when 'T : float32    = not (# "cgt.un" x y : bool #) 
                when 'T : char       = not(# "cgt" x y : bool #)

            let inline GenericGreaterOrEqualFast (x:'T) (y:'T) = 
                GenericGreaterOrEqualIntrinsic x y
                when 'T : bool       = not (# "clt" x y : bool #)
                when 'T : int        = not (# "clt" x y : bool #)
                when 'T : sbyte      = not (# "clt" x y : bool #)
                when 'T : int16      = not (# "clt" x y : bool #)
                when 'T : int32      = not (# "clt" x y : bool #)
                when 'T : int64      = not (# "clt" x y : bool #)
                when 'T : nativeint  = not (# "clt" x y : bool #)
                when 'T : byte       = not (# "clt.un" x y : bool #)
                when 'T : uint16     = not (# "clt.un" x y : bool #)
                when 'T : uint32     = not (# "clt.un" x y : bool #)
                when 'T : uint64     = not (# "clt.un" x y : bool #)
                when 'T : unativeint = not (# "clt.un" x y : bool #)
                when 'T : float      = not (# "clt.un" x y : bool #) 
                when 'T : float32    = not (# "clt.un" x y : bool #)
                when 'T : char       = not (# "clt" x y : bool #)

            let GenericMinimumFast x y = if GenericLessThanFast x y then x else y
            let GenericMaximumFast x y = if GenericLessThanFast x y then y else x


            //-------------------------------------------------------------------------
            // EQUALITY
            //------------------------------------------------------------------------- 


            /// optimized case: Core implementation of structural equality on arrays.
            let GenericEqualityByteArray (x:byte[]) (y:byte[]) : bool=
                let lenx = x.Length 
                let leny = y.Length 
                let c = (lenx = leny)
                if not c then c 
                else
                    let mutable i = 0 
                    let mutable res = true
                    while i < lenx do 
                        let c = byteEq (get x i) (get y i) 
                        if not c then (res <- false; i <- lenx) 
                        else i <- i + 1
                    res

            /// optimized case: Core implementation of structural equality on arrays.
            let GenericEqualityInt32Array (x:int[]) (y:int[]) : bool=
                let lenx = x.Length 
                let leny = y.Length 
                let c = (lenx = leny)
                if not c then c 
                else
                    let mutable i = 0 
                    let mutable res = true
                    while i < lenx do 
                        let c = int32Eq (get x i) (get y i) 
                        if not c then (res <- false; i <- lenx) 
                        else i <- i + 1
                    res

            /// optimized case: Core implementation of structural equality on arrays
            let GenericEqualitySingleArray (x:float32[]) (y:float32[]) : bool=
                let lenx = x.Length 
                let leny = y.Length 
                let c = (lenx = leny)
                if not c then c 
                else
                    let mutable i = 0 
                    let mutable res = true
                    while i < lenx do 
                        let c = float32Eq (get x i) (get y i) 
                        if not c then (res <- false; i <- lenx) 
                        else i <- i + 1
                    res

            /// optimized case: Core implementation of structural equality on arrays.
            let GenericEqualityDoubleArray (x:float[]) (y:float[]) : bool=
                let lenx = x.Length 
                let leny = y.Length 
                let c = (lenx = leny)
                if not c then c 
                else
                    let mutable i = 0 
                    let mutable res = true
                    while i < lenx do 
                        let c = floatEq (get x i) (get y i) 
                        if not c then (res <- false; i <- lenx) 
                        else i <- i + 1
                    res

            /// optimized case: Core implementation of structural equality on arrays.
            let GenericEqualityCharArray (x:char[]) (y:char[]) : bool=
                let lenx = x.Length 
                let leny = y.Length 
                let c = (lenx = leny)
                if not c then c 
                else
                    let mutable i = 0 
                    let mutable res = true
                    while i < lenx do 
                        let c = charEq (get x i) (get y i) 
                        if not c then (res <- false; i <- lenx) 
                        else i <- i + 1
                    res

            /// optimized case: Core implementation of structural equality on arrays.
            let GenericEqualityInt64Array (x:int64[]) (y:int64[]) : bool=
                let lenx = x.Length 
                let leny = y.Length 
                let c = (lenx = leny)
                if not c then c 
                else
                    let mutable i = 0 
                    let mutable res = true
                    while i < lenx do 
                        let c = int64Eq (get x i) (get y i) 
                        if not c then (res <- false; i <- lenx) 
                        else i <- i + 1
                    res



            let rec GenericEqualityObj (iec:System.Collections.IEqualityComparer) ((xobj:obj),(yobj:obj)) : bool = 
                (*if objEq xobj yobj then true else  *)
                match xobj,yobj with 
                 | null,null -> true
                 | null,_ -> false
                 | _,null -> false
                 | (:? string as xs),(:? string as ys) -> System.String.Equals(xs,ys)
                 // Permit structural equality on arrays
                 | (:? System.Array as arr1),_ -> 
                     match arr1,yobj with 
                     // Fast path
                     | (:? (obj[]) as arr1) ,    (:? (obj[]) as arr2)      -> GenericEqualityObjArray iec arr1 arr2
                     // Fast path
                     | (:? (byte[]) as arr1),    (:? (byte[]) as arr2)     -> GenericEqualityByteArray arr1 arr2
                     | (:? (int32[]) as arr1),   (:? (int32[]) as arr2)   -> GenericEqualityInt32Array arr1 arr2
                     | (:? (int64[]) as arr1),   (:? (int64[]) as arr2)   -> GenericEqualityInt64Array arr1 arr2
                     | (:? (char[]) as arr1),    (:? (char[]) as arr2)     -> GenericEqualityCharArray arr1 arr2
                     | (:? (float32[]) as arr1), (:? (float32[]) as arr2) -> GenericEqualitySingleArray arr1 arr2
                     | (:? (float[]) as arr1),   (:? (float[]) as arr2)     -> GenericEqualityDoubleArray arr1 arr2
                     | _                    ,    (:? System.Array as arr2) -> GenericEqualityArbArray iec arr1 arr2
                     | _ -> xobj.Equals(yobj)
                 | (:? IStructuralEquatable as x1),_ -> x1.Equals(yobj,iec)
                 // Ensure NaN semantics on recursive calls
                 | (:? float as f1), (:? float as f2) -> (# "ceq" f1 f2 : bool #)
                 | (:? float32 as f1), (:? float32 as f2) -> (# "ceq" f1 f2 : bool #)
                 | _ -> xobj.Equals(yobj)

            /// specialcase: Core implementation of structural equality on arbitrary arrays.
            and GenericEqualityArbArray (iec:System.Collections.IEqualityComparer) (x:System.Array) (y:System.Array) : bool =
#if FX_NO_ARRAY_LONG_LENGTH
                if x.Rank = 1 && y.Rank = 1 then 
                    // check lengths 
                    let lenx = x.Length
                    let leny = y.Length 
                    (int32Eq lenx leny) &&
                    // check contents
                    let rec check i = (i >= lenx) || (GenericEqualityObj iec ((x.GetValue(i)),(y.GetValue(i))) && check (i + 1))
                    check 0                    
                elif x.Rank = 2 && y.Rank = 2 then 
                    // check lengths 
                    let lenx0 = x.GetLength(0)
                    let leny0 = y.GetLength(0)
                    (int32Eq lenx0 leny0) && 
                    let lenx1 = x.GetLength(1)
                    let leny1 = y.GetLength(1)
                    (int32Eq lenx1 leny1) && 
                    let basex0 = x.GetLowerBound(0)
                    let basex1 = x.GetLowerBound(1)
                    let basey0 = y.GetLowerBound(0)
                    let basey1 = y.GetLowerBound(1)
                    (int32Eq basex0 basey0) && 
                    (int32Eq basex1 basey1) && 
                    // check contents
                    let rec check0 i =
                       let rec check1 j = (j >= lenx1) || (GenericEqualityObj iec ((x.GetValue(basex0 + i,basex1 + j)), (y.GetValue(basey0 + i,basey1 + j))) && check1 (j + 1))
                       (i >= lenx0) || (check1 0 && check0 (i + 1))
                    check0 0
                else 
                    (x.Rank = y.Rank) && 
                    let ndims = x.Rank
                    // check lengths 
                    let rec precheck k = 
                        (k >= ndims) || 
                        (int32Eq (x.GetLength(k)) (y.GetLength(k)) && 
                         int32Eq (x.GetLowerBound(k)) (y.GetLowerBound(k)) && 
                         precheck (k+1))
                    precheck 0 &&
                    let idxs : int32[] = zero_create ndims 
                    // check contents
                    let rec checkN k baseIdx i lim =
                       (i >= lim) ||
                       (set idxs k (baseIdx + i);
                        (if k = ndims - 1 
                         then GenericEqualityObj iec ((x.GetValue(idxs)),(y.GetValue(idxs)))
                         else check (k+1)) && 
                        checkN k baseIdx (i + 1) lim)
                    and check k = 
                       (k >= ndims) || 
                       (let baseIdx = x.GetLowerBound(k)
                        checkN k baseIdx 0 (x.GetLength(k)))
                           
                    check 0
#else
                if x.Rank = 1 && y.Rank = 1 then 
                    // check lengths 
                    let lenx = x.LongLength
                    let leny = y.LongLength 
                    (int64Eq lenx leny) &&
                    // check contents
                    let rec check i = (i >=. lenx) || (GenericEqualityObj iec ((x.GetValue(i)),(y.GetValue(i))) && check (i +. 1L))
                    check 0L                    
                elif x.Rank = 2 && y.Rank = 2 then 
                    // check lengths 
                    let lenx0 = x.GetLongLength(0)
                    let leny0 = y.GetLongLength(0)
                    (int64Eq lenx0 leny0) && 
                    let lenx1 = x.GetLongLength(1)
                    let leny1 = y.GetLongLength(1)
                    (int64Eq lenx1 leny1) && 
                    let basex0 = int64 (x.GetLowerBound(0))
                    let basex1 = int64 (x.GetLowerBound(1))
                    let basey0 = int64 (y.GetLowerBound(0))
                    let basey1 = int64 (y.GetLowerBound(1))
                    (int64Eq basex0 basey0) && 
                    (int64Eq basex1 basey1) && 
                    // check contents
                    let rec check0 i =
                       let rec check1 j = (j >=. lenx1) || (GenericEqualityObj iec ((x.GetValue(basex0 +. i,basex1 +. j)),(y.GetValue(basey0 +. i,basey1 +. j))) && check1 (j +. 1L))
                       (i >=. lenx0) || (check1 0L && check0 (i +. 1L))
                    check0 0L
                else 
                    (x.Rank = y.Rank) && 
                    let ndims = x.Rank
                    // check lengths 
                    let rec precheck k = 
                        (k >= ndims) || 
                        (int64Eq (x.GetLongLength(k)) (y.GetLongLength(k)) && 
                         int32Eq (x.GetLowerBound(k)) (y.GetLowerBound(k)) && 
                         precheck (k+1))
                    precheck 0 &&
                    let idxs : int64[] = zero_create ndims 
                    // check contents
                    let rec checkN k baseIdx i lim =
                       (i >=. lim) ||
                       (set idxs k (baseIdx +. i);
                        (if k = ndims - 1
                         then GenericEqualityObj iec ((x.GetValue(idxs)),(y.GetValue(idxs)))
                         else check (k+1)) && 
                        checkN k baseIdx (i +. 1L) lim)
                    and check k = 
                       (k >= ndims) || 
                       (let baseIdx = x.GetLowerBound(k)
                        checkN k (int64 baseIdx) 0L (x.GetLongLength(k)))
                           
                    check 0
#endif                    
              
            /// optimized case: Core implementation of structural equality on object arrays.
            and GenericEqualityObjArray iec (x:obj[]) (y:obj[]) : bool =
                let lenx = x.Length 
                let leny = y.Length 
                let c = (lenx = leny )
                if not c then c 
                else
                    let mutable i = 0
                    let mutable res = true
                    while i < lenx do 
                        let c = GenericEqualityObj iec ((get x i),(get y i))
                        if not c then (res <- false; i <- lenx) 
                        else i <- i + 1
                    res


            let fsEqualityComparer = 
                { new System.Collections.IEqualityComparer with
                    override iec.Equals(x:obj,y:obj) = GenericEqualityObj iec (x,y)
                    override iec.GetHashCode(x:obj) = raise (InvalidOperationException "this object is for recursive equality calls and may not be used for hashing") }

            let rec GenericEqualityIntrinsic (x : 'T) (y : 'T) : bool = 
                fsEqualityComparer.Equals((box x), (box y))
                
            let rec GenericEqualityWithComparerIntrinsic (comp : System.Collections.IEqualityComparer) (x : 'T) (y : 'T) : bool =
                //comp.Equals((box x),(box y))
                GenericEqualityObj comp ((box x),(box y))
                

            let inline GenericEqualityFast (x : 'T) (y : 'T) : bool = 
                  GenericEqualityIntrinsic x y
                  when 'T : bool    = (# "ceq" x y : bool #)
                  when 'T : int     = (# "ceq" x y : bool #)
                  when 'T : sbyte   = (# "ceq" x y : bool #)
                  when 'T : int16   = (# "ceq" x y : bool #)
                  when 'T : int32   = (# "ceq" x y : bool #)
                  when 'T : int64   = (# "ceq" x y : bool #)
                  when 'T : byte    = (# "ceq" x y : bool #)
                  when 'T : uint16  = (# "ceq" x y : bool #)
                  when 'T : uint32  = (# "ceq" x y : bool #)
                  when 'T : uint64  = (# "ceq" x y : bool #)
                  when 'T : float   = (# "ceq" x y : bool #)
                  when 'T : float32 = (# "ceq" x y : bool #)
                  when 'T : char    = (# "ceq" x y : bool #)
                  when 'T : string  = System.String.Equals((# "" x : string #),(# "" y : string #))
                  
            let inline GenericEqualityWithComparerFast (comp : System.Collections.IEqualityComparer) (x : 'T) (y : 'T) : bool = 
                  GenericEqualityWithComparerIntrinsic comp x y
                  when 'T : bool    = (# "ceq" x y : bool #)
                  when 'T : int     = (# "ceq" x y : bool #)
                  when 'T : sbyte   = (# "ceq" x y : bool #)
                  when 'T : int16   = (# "ceq" x y : bool #)
                  when 'T : int32   = (# "ceq" x y : bool #)
                  when 'T : int64   = (# "ceq" x y : bool #)
                  when 'T : byte    = (# "ceq" x y : bool #)
                  when 'T : uint16  = (# "ceq" x y : bool #)
                  when 'T : uint32  = (# "ceq" x y : bool #)
                  when 'T : uint64  = (# "ceq" x y : bool #)
                  when 'T : float   = (# "ceq" x y : bool #)
                  when 'T : float32 = (# "ceq" x y : bool #)
                  when 'T : char    = (# "ceq" x y : bool #)
                  when 'T : string  = System.String.Equals((# "" x : string #),(# "" y : string #))


            let inline GenericInequalityFast (x:'T) (y:'T) = (not(GenericEqualityFast x y) : bool)


            //-------------------------------------------------------------------------
            // HASHING
            //------------------------------------------------------------------------- 



            let defaultHashNodes = 18 

            type CountLimitedHasher(sz:int) =
                [<DefaultValue>]
                val mutable nodeCount : int
                
                member x.Fresh() = 
                    if (System.Threading.Interlocked.CompareExchange(&(x.nodeCount), sz, 0) = 0) then 
                        x
                    else
                        new CountLimitedHasher(sz)
                
                interface IEqualityComparer 
                    

            type UnlimitedHasher() =
                
                interface IEqualityComparer 
                    

            let fsUnlimitedHasher = UnlimitedHasher()
             
            let inline HashCombine nr x y = (x <<< 1) + y + 631 * nr

            let GenericHashObjArray (iec : System.Collections.IEqualityComparer) (x: obj[]) : int =
                  let len = x.Length 
                  let mutable i = len - 1 
                  if i > defaultHashNodes then i <- defaultHashNodes // limit the hash
                  let mutable acc = 0   
                  while (i >= 0) do 
                      // NOTE: GenericHash* call decreases nr 
                      acc <- HashCombine i acc (iec.GetHashCode(x.GetValue(i)));
                      i <- i - 1
                  acc

            // optimized case - byte arrays 
            let GenericHashByteArray (iec : System.Collections.IEqualityComparer) (x: byte[]) : int =
                  let len = length x 
                  let mutable i = len - 1 
                  if i > defaultHashNodes then i <- defaultHashNodes // limit the hash
                  let mutable acc = 0   
                  while (i >= 0) do 
                      acc <- HashCombine i acc (int_of_byte (get x i));
                      i <- i - 1
                  acc

            // optimized case - int arrays 
            let GenericHashInt32Array (iec : System.Collections.IEqualityComparer) (x: int[]) : int =
                  let len = length x 
                  let mutable i = len - 1 
                  if i > defaultHashNodes then i <- defaultHashNodes // limit the hash
                  let mutable acc = 0   
                  while (i >= 0) do 
                      acc <- HashCombine i acc (get x i);
                      i <- i - 1
                  acc

            // optimized case - int arrays 
            let GenericHashInt64Array (iec : System.Collections.IEqualityComparer) (x: int64[]) : int =
                  let len = length x 
                  let mutable i = len - 1 
                  if i > defaultHashNodes then i <- defaultHashNodes // limit the hash
                  let mutable acc = 0   
                  while (i >= 0) do 
                      acc <- HashCombine i acc (int32 (get x i));
                      i <- i - 1
                  acc

            // special case - arrays do not by default have a decent structural hashing function
            let GenericHashArbArray (iec : System.Collections.IEqualityComparer) (x: System.Array) : int =
                  match x.Rank  with 
                  | 1 -> 
                    let b = x.GetLowerBound(0) 
                    let len = x.Length 
                    let mutable i = b + len - 1 
                    if i > b + defaultHashNodes  then i <- b + defaultHashNodes  // limit the hash
                    let mutable acc = 0                  
                    while (i >= b) do 
                        // NOTE: GenericHash* call decreases nr 
                        acc <- HashCombine i acc (iec.GetHashCode(x.GetValue(i)));
                        i <- i - 1
                    acc
                  | n -> 
                     HashCombine 10 (x.GetLength(0)) (x.GetLength(1)) 

            // Core implementation of structural hashing.  Search for the IStructuralHash interface.
            // Arrays and strings are structurally hashed through a separate technique.
            // Objects which do not support this interface get hashed using a virtual
            // call to the standard Object::GetStructuralHashCode().
            let rec GenericHashParamObj (iec : System.Collections.IEqualityComparer) (x: obj) : int =
                  match x with 
                  | null -> 0 
                  | (:? System.Array as a) -> 
                      match a with 
                      | :? (obj[]) as oa -> GenericHashObjArray iec oa 
                      | :? (byte[]) as ba -> GenericHashByteArray iec ba 
                      | :? (int[]) as ba -> GenericHashInt32Array iec ba 
                      | :? (int64[]) as ba -> GenericHashInt64Array iec ba 
                      | _ -> GenericHashArbArray iec a 
                  // REVIEW: System.Array implements IStructuralEquatable in Dev10
                  | :? IStructuralEquatable as a ->    
                      a.GetHashCode(iec)
                  | _ -> 
                      x.GetHashCode()


            // optimized case - hash on things compatible with obj[]


            type CountLimitedHasher with
                
                interface System.Collections.IEqualityComparer with
                    override iec.Equals(x:obj,y:obj) =
                        GenericEqualityObj iec (x,y)
                    override iec.GetHashCode(x:obj) =
                        iec.nodeCount <- iec.nodeCount - 1
                        if iec.nodeCount > 0 then
                            GenericHashParamObj iec  x
                        else
                            -1
               
            type UnlimitedHasher with
                
                interface System.Collections.IEqualityComparer with
                    override iec.Equals(x:obj,y:obj) = GenericEqualityObj iec (x,y)
                    override iec.GetHashCode(x:obj) = GenericHashParamObj iec  x

            // Represents an call to structural hashing that was not optimized by static conditionals.
            // NOTE: compiler/optimizer is aware of this function and optimizes calls to it in many situations
            let GenericHashIntrinsic x = GenericHashParamObj fsUnlimitedHasher (box(x))

            let LimitedGenericHashIntrinsic limit x = GenericHashParamObj (CountLimitedHasher(limit)) (box(x))

            // Represents a recursive call to structural hashing that was not optimized by static conditionals.
            // For unoptimized recursive calls we fetch the generic hasher
            let GenericHashWithComparerIntrinsic<'T> (comp : System.Collections.IEqualityComparer) (x : 'T) : int =
                GenericHashParamObj comp (box(x))
                
                    
             /// Core entry into structural hashing.  Hash to a given depth limit.
            let inline GenericHashWithComparerFast (comp : System.Collections.IEqualityComparer) (x:'T) : int = 
                GenericHashWithComparerIntrinsic comp x 
                when 'T : bool   = (# "" x : int #)
                when 'T : int    = (# "" x : int #)
                when 'T : int32  = (# "" x : int #)
                when 'T : byte   = (# "" x : int #)
                when 'T : uint32 = (# "" x : int #)                          
                when 'T : string = 
                    match (# "" x : obj #) with 
                    | null -> 0 
                    | _ -> (# "call instance int32 [mscorlib]System.String:: GetHashCode()" x : int #)
                    
             /// Core entry into structural hashing.  Hash to a given depth limit.
            let inline GenericHashFast (x:'T) : int = 
                GenericHashIntrinsic x 
                when 'T : bool   = (# "" x : int #)
                when 'T : int    = (# "" x : int #)
                when 'T : int32  = (# "" x : int #)
                when 'T : byte   = (# "" x : int #)
                when 'T : uint32 = (# "" x : int #)                    
                when 'T : string = 
                 match (# "" x : obj #) with 
                 | null -> 0 
                 | _ -> (# "call instance int32 [mscorlib]System.String:: GetHashCode()" x : int #)

             /// Core entry into structural hashing.  Hash to a given depth limit.
            let inline GenericLimitedHashFast (limit:int) (x:'T) : int = 
                LimitedGenericHashIntrinsic limit x 
                when 'T : bool   = (# "" x : int #)
                when 'T : int    = (# "" x : int #)
                when 'T : int32  = (# "" x : int #)
                when 'T : byte   = (# "" x : int #)
                when 'T : uint32 = (# "" x : int #)                    
                when 'T : string = 
                 match (# "" x : obj #) with 
                 | null -> 0 
                 | _ -> (# "call instance int32 [mscorlib]System.String:: GetHashCode()" x : int #)


            let inline FastHashTuple2 (comparer:System.Collections.IEqualityComparer) (x1,x2) = 
                TupleUtils.combine_tuple_hashes (GenericHashWithComparerFast comparer x1) (GenericHashWithComparerFast comparer x2)

            let inline FastHashTuple3 (comparer:System.Collections.IEqualityComparer) (x1,x2,x3) =
                TupleUtils.combine_tuple_hashes (TupleUtils.combine_tuple_hashes (GenericHashWithComparerFast comparer x1) (GenericHashWithComparerFast comparer x2)) (GenericHashWithComparerFast comparer x3)

            let inline FastHashTuple4 (comparer:System.Collections.IEqualityComparer) (x1,x2,x3,x4) = 
                TupleUtils.combine_tuple_hashes (TupleUtils.combine_tuple_hashes (GenericHashWithComparerFast comparer x1) (GenericHashWithComparerFast comparer x2)) (TupleUtils.combine_tuple_hashes (GenericHashWithComparerFast comparer x3) (GenericHashWithComparerFast comparer x4))

            let inline FastHashTuple5 (comparer:System.Collections.IEqualityComparer) (x1,x2,x3,x4,x5) = 
                TupleUtils.combine_tuple_hashes (TupleUtils.combine_tuple_hashes (TupleUtils.combine_tuple_hashes (GenericHashWithComparerFast comparer x1) (GenericHashWithComparerFast comparer x2)) (TupleUtils.combine_tuple_hashes (GenericHashWithComparerFast comparer x3) (GenericHashWithComparerFast comparer x4))) (GenericHashWithComparerFast comparer x5)

            let inline FastEqualsTuple2 (comparer:System.Collections.IEqualityComparer) (x1,x2) (y1,y2) = 
                GenericEqualityWithComparerFast comparer x1 y1 &&
                GenericEqualityWithComparerFast comparer x2 y2

            let inline FastEqualsTuple3 (comparer:System.Collections.IEqualityComparer) (x1,x2,x3) (y1,y2,y3) = 
                GenericEqualityWithComparerFast comparer x1 y1 &&
                GenericEqualityWithComparerFast comparer x2 y2 &&
                GenericEqualityWithComparerFast comparer x3 y3

            let inline FastEqualsTuple4 (comparer:System.Collections.IEqualityComparer) (x1,x2,x3,x4) (y1,y2,y3,y4) = 
                GenericEqualityWithComparerFast comparer x1 y1 &&
                GenericEqualityWithComparerFast comparer x2 y2 &&
                GenericEqualityWithComparerFast comparer x3 y3 &&
                GenericEqualityWithComparerFast comparer x4 y4

            let inline FastEqualsTuple5 (comparer:System.Collections.IEqualityComparer) (x1,x2,x3,x4,x5) (y1,y2,y3,y4,y5) = 
                GenericEqualityWithComparerFast comparer x1 y1 &&
                GenericEqualityWithComparerFast comparer x2 y2 &&
                GenericEqualityWithComparerFast comparer x3 y3 &&
                GenericEqualityWithComparerFast comparer x4 y4 &&
                GenericEqualityWithComparerFast comparer x5 y5

            let inline FastCompareTuple2 (comparer:System.Collections.IComparer) (x1,x2) (y1,y2) =
                let  n = GenericComparisonWithComparerFast comparer x1 y1
                if n <> 0 then n else
                GenericComparisonWithComparerFast comparer x2 y2

            let inline FastCompareTuple3 (comparer:System.Collections.IComparer) (x1,x2,x3) (y1,y2,y3) =
                let  n = GenericComparisonWithComparerFast comparer x1 y1
                if n <> 0 then n else
                let  n = GenericComparisonWithComparerFast comparer x2 y2
                if n <> 0 then n else
                GenericComparisonWithComparerFast comparer x3 y3

            let inline FastCompareTuple4 (comparer:System.Collections.IComparer) (x1,x2,x3,x4) (y1,y2,y3,y4) = 
                let  n = GenericComparisonWithComparerFast comparer x1 y1
                if n <> 0 then n else
                let  n = GenericComparisonWithComparerFast comparer x2 y2
                if n <> 0 then n else
                let  n = GenericComparisonWithComparerFast comparer x3 y3
                if n <> 0 then n else
                GenericComparisonWithComparerFast comparer x4 y4
            
            let inline FastCompareTuple5 (comparer:System.Collections.IComparer) (x1,x2,x3,x4,x5) (y1,y2,y3,y4,y5) =
                let  n = GenericComparisonWithComparerFast comparer x1 y1
                if n <> 0 then n else
                let  n = GenericComparisonWithComparerFast comparer x2 y2
                if n <> 0 then n else
                let  n = GenericComparisonWithComparerFast comparer x3 y3
                if n <> 0 then n else
                let  n = GenericComparisonWithComparerFast comparer x4 y4
                if n <> 0 then n else
                GenericComparisonWithComparerFast comparer x5 y5



        let inline GenericEquality               x y = HashCompare.GenericEqualityFast               x y
        let inline GenericEqualityWithComparer comp   x y = HashCompare.GenericEqualityWithComparerFast   comp x y
        let inline GenericComparison             x y = HashCompare.GenericComparisonFast             x y
        let inline GenericComparisonWithComparer comp x y = HashCompare.GenericComparisonWithComparerFast comp x y
        let inline GenericLessThan               x y = HashCompare.GenericLessThanFast               x y
        let inline GenericGreaterThan            x y = HashCompare.GenericGreaterThanFast            x y
        let inline GenericLessOrEqual            x y = HashCompare.GenericLessOrEqualFast            x y
        let inline GenericGreaterOrEqual         x y = HashCompare.GenericGreaterOrEqualFast         x y
        let inline GenericMinimum                x y = if HashCompare.GenericLessThanFast x y then x else y
        let inline GenericMaximum                x y = if HashCompare.GenericLessThanFast x y then y else x

        let inline PhysicalEquality x y     = HashCompare.PhysicalEqualityFast x y
        let inline PhysicalHash x         = HashCompare.PhysicalHashFast x
        let FSharpComparer() = HashCompare.fsComparer :> System.Collections.IComparer
        let FSharpEqualityComparer() = HashCompare.fsUnlimitedHasher :> System.Collections.IEqualityComparer

        let inline GenericHash x                = HashCompare.GenericHashFast x
        let inline GenericLimitedHash limit x   = HashCompare.GenericLimitedHashFast limit x
        let inline GenericHashWithComparer comp x = HashCompare.GenericHashWithComparerFast comp x

        let inline MakeGenericEqualityComparer<'T>() = 
            // type-specialize some common cases to generate more efficient functions 
            { new System.Collections.Generic.IEqualityComparer<'T> with 
                  member self.GetHashCode(x) = GenericHash x 
                  member self.Equals(x,y) = GenericEquality x y }

        let inline MakeGenericLimitedEqualityComparer<'T>(limit:int) = 
            // type-specialize some common cases to generate more efficient functions 
            { new System.Collections.Generic.IEqualityComparer<'T> with 
                  member self.GetHashCode(x) = GenericLimitedHash limit x 
                  member self.Equals(x,y) = GenericEquality x y }

(*
        let CharIEquality    = MakeGenericEqualityComparer<char>()
        let StringIEquality  = MakeGenericEqualityComparer<string>()
        let SByteIEquality   = MakeGenericEqualityComparer<sbyte>()
        let Int16IEquality   = MakeGenericEqualityComparer<int16>()
        let Int32IEquality   = MakeGenericEqualityComparer<int32>()
        let Int64IEquality   = MakeGenericEqualityComparer<int64>()
        let IntPtrIEquality  = MakeGenericEqualityComparer<nativeint>()
        let ByteIEquality    = MakeGenericEqualityComparer<byte>()
        let UInt16IEquality  = MakeGenericEqualityComparer<uint16>()
        let UInt32IEquality  = MakeGenericEqualityComparer<uint32>()
        let UInt64IEquality  = MakeGenericEqualityComparer<uint64>()
        let UIntPtrIEquality = MakeGenericEqualityComparer<unativeint>()
        let FloatIEquality   = MakeGenericEqualityComparer<float>()
        let Float32IEquality = MakeGenericEqualityComparer<float32>()
        let DecimalIEquality = MakeGenericEqualityComparer<decimal>()

        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]     
        type FastGenericEqualityComparerTable<'T>() = 
            static let f : System.Collections.Generic.IEqualityComparer<'T> = 
                match typeof<'T> with 
                | ty when ty.Equals(typeof<byte>)       -> unboxPrim (box ByteIEquality)
                | ty when ty.Equals(typeof<char>)       -> unboxPrim (box CharIEquality)
                | ty when ty.Equals(typeof<sbyte>)      -> unboxPrim (box SByteIEquality)
                | ty when ty.Equals(typeof<int16>)      -> unboxPrim (box Int16IEquality)
                | ty when ty.Equals(typeof<int32>)      -> unboxPrim (box Int32IEquality)
                | ty when ty.Equals(typeof<int64>)      -> unboxPrim (box Int64IEquality)
                | ty when ty.Equals(typeof<nativeint>)  -> unboxPrim (box IntPtrIEquality)
                | ty when ty.Equals(typeof<uint16>)     -> unboxPrim (box UInt16IEquality)
                | ty when ty.Equals(typeof<uint32>)     -> unboxPrim (box UInt32IEquality)
                | ty when ty.Equals(typeof<uint64>)     -> unboxPrim (box UInt64IEquality)
                | ty when ty.Equals(typeof<unativeint>) -> unboxPrim (box UIntPtrIEquality)
                | ty when ty.Equals(typeof<float>)      -> unboxPrim (box FloatIEquality)
                | ty when ty.Equals(typeof<float32>)    -> unboxPrim (box Float32IEquality)
                | ty when ty.Equals(typeof<decimal>)    -> unboxPrim (box DecimalIEquality)
                | ty when ty.Equals(typeof<string>)     -> unboxPrim (box StringIEquality)
                | _ -> MakeGenericEqualityComparer<'T>()
            static member Function : System.Collections.Generic.IEqualityComparer<'T> = f

        let FastGenericEqualityComparer<'T> = FastGenericEqualityComparerTable<'T>.Function
*)
        let inline FastGenericEqualityComparer<'T> = MakeGenericEqualityComparer<'T>() 
        let inline FastLimitedGenericEqualityComparer<'T>(limit) = MakeGenericLimitedEqualityComparer<'T>(limit) 

        let inline MakeFastGenericComparer<'T>()  = 
            { new System.Collections.Generic.IComparer<'T> with 
                 member __.Compare(x,y) = GenericComparison x y }

        let CharComparer    = MakeFastGenericComparer<char>()
        let StringComparer  = MakeFastGenericComparer<string>()
        let SByteComparer   = MakeFastGenericComparer<sbyte>()
        let Int16Comparer   = MakeFastGenericComparer<int16>()
        let Int32Comparer   = MakeFastGenericComparer<int32>()
        let Int64Comparer   = MakeFastGenericComparer<int64>()
        let IntPtrComparer  = MakeFastGenericComparer<nativeint>()
        let ByteComparer    = MakeFastGenericComparer<byte>()
        let UInt16Comparer  = MakeFastGenericComparer<uint16>()
        let UInt32Comparer  = MakeFastGenericComparer<uint32>()
        let UInt64Comparer  = MakeFastGenericComparer<uint64>()
        let UIntPtrComparer = MakeFastGenericComparer<unativeint>()
        let FloatComparer   = MakeFastGenericComparer<float>()
        let Float32Comparer = MakeFastGenericComparer<float32>()
        let DecimalComparer = MakeFastGenericComparer<decimal>()

        /// Use a type-indexed table to ensure we only create a single FastStructuralComparison function
        /// for each type
        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]     
        type FastGenericComparerTable<'T>() = 
            static let f : System.Collections.Generic.IComparer<'T>  = 
                match typeof<'T> with 
                | ty when ty.Equals(typeof<byte>)       -> unboxPrim (box ByteComparer)
                | ty when ty.Equals(typeof<char>)       -> unboxPrim (box CharComparer)
                | ty when ty.Equals(typeof<sbyte>)      -> unboxPrim (box SByteComparer)
                | ty when ty.Equals(typeof<int16>)      -> unboxPrim (box Int16Comparer)
                | ty when ty.Equals(typeof<int32>)      -> unboxPrim (box Int32Comparer)
                | ty when ty.Equals(typeof<int64>)      -> unboxPrim (box Int64Comparer)
                | ty when ty.Equals(typeof<nativeint>)  -> unboxPrim (box IntPtrComparer)
                | ty when ty.Equals(typeof<uint16>)     -> unboxPrim (box UInt16Comparer)
                | ty when ty.Equals(typeof<uint32>)     -> unboxPrim (box UInt32Comparer)
                | ty when ty.Equals(typeof<uint64>)     -> unboxPrim (box UInt64Comparer)
                | ty when ty.Equals(typeof<unativeint>) -> unboxPrim (box UIntPtrComparer)
                | ty when ty.Equals(typeof<float>)      -> unboxPrim (box FloatComparer)
                | ty when ty.Equals(typeof<float32>)    -> unboxPrim (box Float32Comparer)
                | ty when ty.Equals(typeof<decimal>)    -> unboxPrim (box DecimalComparer)
                | ty when ty.Equals(typeof<string>)     -> unboxPrim (box StringComparer)
                | _ -> MakeFastGenericComparer<'T>()
            static member Function : System.Collections.Generic.IComparer<'T> = f
        
        let FastGenericComparer<'T> = FastGenericComparerTable<'T>.Function


        let inline retype (x:'T) : 'U = (# "" x : 'U #)

        let inline EnumOfValue (u : 'u) : 'e when 'e : enum<'u> = 
            unboxPrim<'e>(box u)
            when 'e : 'e = (retype u : 'e)

        let inline EnumToValue (e : 'e) : 'u when 'e : enum<'u> = 
            unboxPrim<'u>(box e)
             // According to the somewhat subtle rules of static optimizations,
             // this condition is used whenever ^T is resolved to a nominal type
            when 'e : 'e = (retype e : 'u)



        let inline format_error() = raise (new System.FormatException("Input string was not in a correct format"))

        // Parse formats
        //      DDDDDDDD
        //      -DDDDDDDD
        //      0xHHHHHHHH
        //      -0xHHHHHHHH
        //      0bBBBBBBB
        //      -0bBBBBBBB
        //      0oOOOOOOO
        //      -0oOOOOOOO
        // without leading/trailing spaces.
        ///
        // Note: Parse defaults to NumberStyles.Integer =  AllowLeadingWhite ||| AllowTrailingWhite ||| AllowLeadingSign
        // However, that is not the required behaviour of 'int32', 'int64' etc. when used on string
        // arguments: we explicitly disallow AllowLeadingWhite ||| AllowTrailingWhite 
        // and only request AllowLeadingSign.

        let is_oxb c = 
#if FX_NO_TO_LOWER_INVARIANT
            let c = System.Char.ToLower c
#else
            let c = System.Char.ToLowerInvariant c
#endif
            charEq c 'x' || charEq c 'o' || charEq c 'b'

        let is_0oxb (s:string) p l = 
            l >= p + 2 && charEq (s.Chars(p)) '0' && is_oxb (s.Chars(p+1))

        let get_0oxb (s:string) p l = 
            if is_0oxb s p l
#if FX_NO_TO_LOWER_INVARIANT
            then p+2,System.Char.ToLower(s.Chars(p+1) )
#else
            then p+2,System.Char.ToLowerInvariant(s.Chars(p+1) )
#endif
            else p,'d' 

        let getSign32 (s:string) p l = 
            if (l >= p + 1 && charEq (s.Chars(p)) '-') 
            then 1,(-1) 
            else 0,1 

        let getSign64 (s:string) p l = 
            if (l >= p + 1 && charEq (s.Chars(p)) '-') 
            then 1, -1L
            else 0,1L 

        let parseOctalUInt64 (s:string) p l = 
            let rec parse n acc = if n < l then parse (n+1) (acc *.. 8UL +.. (let c = s.Chars(n) in if c >=... '0' && c <=... '7' then Convert.ToUInt64(c) -.. Convert.ToUInt64('0') else format_error())) else acc in
            parse p 0UL

        let parseBinaryUInt64 (s:string) p l = 
            let rec parse n acc = if n < l then parse (n+1) (acc *.. 2UL +.. (match s.Chars(n) with '0' -> 0UL | '1' -> 1UL | _ -> format_error())) else acc in          
            parse p 0UL

        let ParseUInt32 (s:string) = 
            let s = s.Trim() 
            let l = s.Length 
            let p = 0 
            let p,specifier = get_0oxb s p l in
            if p >= l then format_error() else
            match specifier with 
            | 'x' -> UInt32.Parse( s.Substring(p), NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture)
            | 'b' -> Convert.ToUInt32(parseBinaryUInt64 s p l)
            | 'o' -> Convert.ToUInt32(parseOctalUInt64 s p l)
            | _ -> UInt32.Parse(s.Substring(p), NumberStyles.Integer, CultureInfo.InvariantCulture) in

        let inline int32_of_uint32 (x:uint32) = (# "" x  : int32 #)
        let inline int64_of_uint64 (x:uint64) = (# "" x  : int64 #)

        let ParseInt32 (s:string) = 
            let s = s.Trim() in
            let l = s.Length in 
            let p = 0 in 
            let p,sign = getSign32 s p l in
            let p,specifier = get_0oxb s p l in
            if p >= l then format_error() else
            match Char.ToLower(specifier,CultureInfo.InvariantCulture(*FxCop:1304*)) with 
            | 'x' -> sign * (int32_of_uint32 (Convert.ToUInt32(UInt64.Parse(s.Substring(p), NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture))))
            | 'b' -> sign * (int32_of_uint32 (Convert.ToUInt32(parseBinaryUInt64 s p l)))
            | 'o' -> sign * (int32_of_uint32 (Convert.ToUInt32(parseOctalUInt64 s p l)))
            | _ -> Int32.Parse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture)

        let ParseInt64 (s:string) = 
            let s = s.Trim() 
            let l = s.Length 
            let p = 0 
            let p,sign = getSign64 s p l in
            let p,specifier = get_0oxb s p l in
            if p >= l then format_error() else
            match Char.ToLower(specifier,CultureInfo.InvariantCulture(*FxCop:1304*)) with 
            | 'x' -> sign *. Int64.Parse(s.Substring(p), NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture)
            | 'b' -> sign *. (int64_of_uint64 (parseBinaryUInt64 s p l))
            | 'o' -> sign *. (int64_of_uint64 (parseOctalUInt64 s p l))
            | _ -> Int64.Parse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture)

        let ParseUInt64     (s:string) : uint64 = 
            let s = s.Trim() in
            let l = s.Length in 
            let p = 0 in 
            let p,specifier = get_0oxb s p l in
            if p >= l then format_error() else
            match specifier with 
            | 'x' -> UInt64.Parse(s.Substring(p), NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture)
            | 'b' -> parseBinaryUInt64 s p l
            | 'o' -> parseOctalUInt64 s p l
            | _ -> UInt64.Parse(s.Substring(p), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture) 


        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
        type GenericZeroDynamicImplTable<'T>() = 
            static let result : 'T = 
                // The dynamic implementation
                let aty = typeof<'T>
                if   aty.Equals(typeof<sbyte>)      then unboxPrim<'T> (box 0y)
                elif aty.Equals(typeof<int16>)      then unboxPrim<'T> (box 0s)
                elif aty.Equals(typeof<int32>)      then unboxPrim<'T> (box 0)
                elif aty.Equals(typeof<int64>)      then unboxPrim<'T> (box 0L)
                elif aty.Equals(typeof<nativeint>)  then unboxPrim<'T> (box 0n)
                elif aty.Equals(typeof<byte>)       then unboxPrim<'T> (box 0uy)
                elif aty.Equals(typeof<uint16>)     then unboxPrim<'T> (box 0us)
                elif aty.Equals(typeof<uint32>)     then unboxPrim<'T> (box 0u)
                elif aty.Equals(typeof<uint64>)     then unboxPrim<'T> (box 0UL)
                elif aty.Equals(typeof<unativeint>) then unboxPrim<'T> (box 0un)
                elif aty.Equals(typeof<decimal>)    then unboxPrim<'T> (box 0M)
                elif aty.Equals(typeof<float>)      then unboxPrim<'T> (box 0.0)
                elif aty.Equals(typeof<float32>)    then unboxPrim<'T> (box 0.0f)
                else 
                   let pinfo = aty.GetProperty("Zero")
                   unboxPrim<'T> (pinfo.GetValue(null,null))
            static member Result : 'T = result
                   
        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
        type GenericOneDynamicImplTable<'T>() = 
            static let result : 'T = 
                // The dynamic implementation
                let aty = typeof<'T>
                if   aty.Equals(typeof<sbyte>)      then unboxPrim<'T> (box 1y)
                elif aty.Equals(typeof<int16>)      then unboxPrim<'T> (box 1s)
                elif aty.Equals(typeof<int32>)      then unboxPrim<'T> (box 1)
                elif aty.Equals(typeof<int64>)      then unboxPrim<'T> (box 1L)
                elif aty.Equals(typeof<nativeint>)  then unboxPrim<'T> (box 1n)
                elif aty.Equals(typeof<byte>)       then unboxPrim<'T> (box 1uy)
                elif aty.Equals(typeof<uint16>)     then unboxPrim<'T> (box 1us)
                elif aty.Equals(typeof<char>)       then unboxPrim<'T> (box (retype 1us : char))
                elif aty.Equals(typeof<uint32>)     then unboxPrim<'T> (box 1u)
                elif aty.Equals(typeof<uint64>)     then unboxPrim<'T> (box 1UL)
                elif aty.Equals(typeof<unativeint>) then unboxPrim<'T> (box 1un)
                elif aty.Equals(typeof<decimal>)    then unboxPrim<'T> (box 1M)
                elif aty.Equals(typeof<float>)      then unboxPrim<'T> (box 1.0)
                elif aty.Equals(typeof<float32>)    then unboxPrim<'T> (box 1.0f)
                else 
                   let pinfo = aty.GetProperty("One")
                   unboxPrim<'T> (pinfo.GetValue(null,null))

            static member Result : 'T = result

        let GenericZeroDynamic< 'T >() : 'T = GenericZeroDynamicImplTable<'T>.Result
        let GenericOneDynamic< 'T >() : 'T = GenericOneDynamicImplTable<'T>.Result

        let inline GenericZero< ^T when ^T : (static member Zero : ^T) > : ^T =
            GenericZeroDynamic<(^T)>()
            when ^T : int32       = 0
            when ^T : float       = 0.0
            when ^T : float32     = 0.0f
            when ^T : int64       = 0L
            when ^T : uint64      = 0UL
            when ^T : uint32      = 0ul
            when ^T : nativeint   = 0n
            when ^T : unativeint  = 0un
            when ^T : int16       = 0s
            when ^T : uint16      = 0us
            when ^T : sbyte       = 0y
            when ^T : byte        = 0uy
            when ^T : decimal     = 0M
             // According to the somewhat subtle rules of static optimizations,
             // this condition is used whenever ^T is resolved to a nominal type
            when ^T : ^T = (^T : (static member Zero : ^T) ())


        let inline GenericOne< ^T when ^T : (static member One : ^T) > : ^T =
            GenericOneDynamic<(^T)>()
            when ^T : int32       = 1
            when ^T : float       = 1.0
            when ^T : float32     = 1.0f
            when ^T : int64       = 1L
            when ^T : uint64      = 1UL
            when ^T : uint32      = 1ul
            when ^T : nativeint   = 1n
            when ^T : unativeint  = 1un
            when ^T : int16       = 1s
            when ^T : uint16      = 1us
            when ^T : char        = (retype 1us : char)
            when ^T : sbyte       = 1y
            when ^T : byte        = 1uy
            when ^T : decimal     = 1M
             // According to the somewhat subtle rules of static optimizations,
             // this condition is used whenever ^T is resolved to a nominal type
             // That is, not in the generic implementation of '+'
            when ^T : ^T = (^T : (static member One : ^T) ())

        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
        type GenericDivideByIntDynamicImplTable<'T>() = 
            static let result : ('T -> int -> 'T) = 
                // The dynamic implementation
                let aty = typeof<'T>
                if aty.Equals(typeof<decimal>)    then unboxPrim<_> (box (fun (x:decimal) (n:int) -> System.Decimal.Divide(x, System.Convert.ToDecimal(n))))
                elif aty.Equals(typeof<float>)      then unboxPrim<_> (box (fun (x:float) (n:int) -> (# "div" x ((# "conv.r8" n  : float #)) : float #)))
                elif aty.Equals(typeof<float32>)    then unboxPrim<_> (box (fun (x:float32) (n:int) -> (# "div" x ((# "conv.r4" n  : float32 #)) : float32 #)))
                else 
                    match aty.GetMethod("DivideByInt",[| aty; typeof<int> |]) with 
                    | null -> raise (NotSupportedException "dynamic invocation of DivideByInt involving coercions")
                    | m -> (fun x n -> unboxPrim<_> (m.Invoke(null,[| box x; box n |])))

            static member Result : ('T -> int -> 'T) = result

        let DivideByIntDynamic<'T> x n = GenericDivideByIntDynamicImplTable<('T)>.Result x n

        let inline DivideByInt< ^T when ^T : (static member DivideByInt : ^T * int -> ^T) > (x:^T) (n:int) : ^T =
            DivideByIntDynamic<'T> x n
            when ^T : float       = (# "div" x ((# "conv.r8" (n:int)  : float #)) : float #)
            when ^T : float32     = (# "div" x ((# "conv.r4" (n:int)  : float32 #)) : float32 #)
            when ^T : decimal     = System.Decimal.Divide((retype x:decimal), System.Convert.ToDecimal(n))
            when ^T : ^T = (^T : (static member DivideByInt : ^T * int -> ^T) (x, n))


        // Dynamic implementation of addition operator resolution
        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
        type AdditionDynamicImplTable<'T,'U,'V>() = 
            static let impl : ('T -> 'U -> 'V) = 
                // The dynamic implementation
                let aty = typeof<'T>
                let bty = typeof<'U>
                let cty = typeof<'V> 
                let dyn() = 
                    let ameth = aty.GetMethod("op_Addition",[| aty; bty |])
                    let bmeth = if aty.Equals(bty) then null else bty.GetMethod("op_Addition",[| aty; bty |])
                    match ameth,bmeth  with 
                    | null, null -> raise (NotSupportedException "dynamic invocation of op_Addition involving coercions")
                    | m,null | null,m -> (fun x y -> unboxPrim<_> (m.Invoke(null,[| box x; box y |])))
                    | _ -> raise (NotSupportedException "dynamic invocation of op_Addition involving overloading")
                        
                if aty.Equals(bty) && bty.Equals(cty) then
                    if aty.Equals(typeof<sbyte>)        then unboxPrim<_> (box (fun (x:sbyte)      (y:sbyte)      -> (# "conv.i1" (# "add" x y : int32 #) : sbyte #)))
                    elif aty.Equals(typeof<int16>)      then unboxPrim<_> (box (fun (x:int16)      (y:int16)      -> (# "conv.i2" (# "add" x y : int32 #) : int16 #)))
                    elif aty.Equals(typeof<int32>)      then unboxPrim<_> (box (fun (x:int32)      (y:int32)      -> (# "add" x y : int32 #)))
                    elif aty.Equals(typeof<int64>)      then unboxPrim<_> (box (fun (x:int64)      (y:int64)      -> (# "add" x y : int64 #)))
                    elif aty.Equals(typeof<nativeint>)  then unboxPrim<_> (box (fun (x:nativeint)  (y:nativeint)  -> (# "add" x y : nativeint #)))
                    elif aty.Equals(typeof<byte>)       then unboxPrim<_> (box (fun (x:byte)       (y:byte)       -> (# "conv.u1" (# "add" x y : uint32 #) : byte #)))
                    elif aty.Equals(typeof<uint16>)     then unboxPrim<_> (box (fun (x:uint16)     (y:uint16)     -> (# "conv.u2" (# "add" x y : uint32 #) : uint16 #)))
                    elif aty.Equals(typeof<uint32>)     then unboxPrim<_> (box (fun (x:uint32)     (y:uint32)     -> (# "add" x y : uint32 #)))
                    elif aty.Equals(typeof<uint64>)     then unboxPrim<_> (box (fun (x:uint64)     (y:uint64)     -> (# "add" x y : uint64 #)))
                    elif aty.Equals(typeof<unativeint>) then unboxPrim<_> (box (fun (x:unativeint) (y:unativeint) -> (# "add" x y : unativeint #)))
                    elif aty.Equals(typeof<float>)      then unboxPrim<_> (box (fun (x:float)      (y:float)      -> (# "add" x y : float #)))
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_> (box (fun (x:float32)    (y:float32)    -> (# "add" x y : float32 #)))
                    elif aty.Equals(typeof<string>)     then unboxPrim<_> (box (fun (x:string)     (y:string)     -> System.String.Concat(x,y)))
                    else dyn()
                else dyn()

            static member Impl : ('T -> 'U -> 'V) = impl

        let AdditionDynamic<'T,'U,'V> x n  = AdditionDynamicImplTable<'T,'U,'V>.Impl x n

        // Dynamic implementation of checked addition operator resolution
        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
        type CheckedAdditionDynamicImplTable<'T,'U,'V>() = 
            static let impl : ('T -> 'U -> 'V) = 
                // The dynamic implementation
                let aty = typeof<'T>
                let bty = typeof<'U>
                let cty = typeof<'V> 
                let dyn() = 
                    let ameth = aty.GetMethod("op_Addition",[| aty; bty |])
                    let bmeth = if aty.Equals(bty) then null else bty.GetMethod("op_Addition",[| aty; bty |])
                    match ameth,bmeth  with 
                    | null, null -> raise (NotSupportedException "dynamic invocation of op_Addition involving coercions")
                    | m,null | null,m -> (fun x y -> unboxPrim<_> (m.Invoke(null,[| box x; box y |])))
                    | _ -> raise (NotSupportedException "dynamic invocation of op_Addition involving overloading")
                        
                if aty.Equals(bty) && bty.Equals(cty) then
                    if aty.Equals(typeof<sbyte>)        then unboxPrim<_> (box (fun (x:sbyte)      (y:sbyte)      -> (# "conv.ovf.i1" (# "add.ovf" x y : int32 #) : sbyte #)))
                    elif aty.Equals(typeof<int16>)      then unboxPrim<_> (box (fun (x:int16)      (y:int16)      -> (# "conv.ovf.i2" (# "add.ovf" x y : int32 #) : int16 #)))
                    elif aty.Equals(typeof<int32>)      then unboxPrim<_> (box (fun (x:int32)      (y:int32)      -> (# "add.ovf" x y : int32 #)))
                    elif aty.Equals(typeof<int64>)      then unboxPrim<_> (box (fun (x:int64)      (y:int64)      -> (# "add.ovf" x y : int64 #)))
                    elif aty.Equals(typeof<nativeint>)  then unboxPrim<_> (box (fun (x:nativeint)  (y:nativeint)  -> (# "add.ovf" x y : nativeint #)))
                    elif aty.Equals(typeof<byte>)       then unboxPrim<_> (box (fun (x:byte)       (y:byte)       -> (# "conv.ovf.u1.un" (# "add.ovf.un" x y : uint32 #) : byte #)))
                    elif aty.Equals(typeof<uint16>)     then unboxPrim<_> (box (fun (x:uint16)     (y:uint16)     -> (# "conv.ovf.u2.un" (# "add.ovf.un" x y : uint32 #) : uint16 #)))
                    elif aty.Equals(typeof<char>)       then unboxPrim<_> (box (fun (x:char)       (y:char)       -> (# "conv.ovf.u2.un" (# "add.ovf.un" x y : uint32 #) : char #)))
                    elif aty.Equals(typeof<uint32>)     then unboxPrim<_> (box (fun (x:uint32)     (y:uint32)     -> (# "add.ovf.un" x y : uint32 #)))
                    elif aty.Equals(typeof<uint64>)     then unboxPrim<_> (box (fun (x:uint64)     (y:uint64)     -> (# "add.ovf.un" x y : uint64 #)))
                    elif aty.Equals(typeof<unativeint>) then unboxPrim<_> (box (fun (x:unativeint) (y:unativeint) -> (# "add.ovf.un" x y : unativeint #)))
                    elif aty.Equals(typeof<float>)      then unboxPrim<_> (box (fun (x:float)      (y:float)      -> (# "add" x y : float #)))
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_> (box (fun (x:float32)    (y:float32)    -> (# "add" x y : float32 #)))
                    elif aty.Equals(typeof<string>)     then unboxPrim<_> (box (fun (x:string)     (y:string)     -> System.String.Concat(x,y)))
                    else dyn()
                else dyn()


            static member Impl : ('T -> 'U -> 'V) = impl

        let CheckedAdditionDynamic<'T,'U,'V> x n  = CheckedAdditionDynamicImplTable<'T,'U,'V>.Impl x n


        // Dynamic implementation of addition operator resolution
        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
        type MultiplyDynamicImplTable<'T,'U,'V>() = 
            static let impl : ('T -> 'U -> 'V) = 
                // The dynamic implementation
                let aty = typeof<'T>
                let bty = typeof<'U>
                let cty = typeof<'V> 
                let dyn() = 
                    let ameth = aty.GetMethod("op_Multiply",[| aty; bty |])
                    let bmeth = if aty.Equals(bty) then null else bty.GetMethod("op_Multiply",[| aty; bty |])
                    match ameth,bmeth  with 
                    | null, null -> raise (NotSupportedException "dynamic invocation of op_Multiply involving coercions")
                    | m,null | null,m -> (fun x y -> unboxPrim<_> (m.Invoke(null,[| box x; box y |])))
                    | _ -> raise (NotSupportedException "dynamic invocation of op_Multiply involving overloading")
                        
                if aty.Equals(bty) && bty.Equals(cty) then
                    if aty.Equals(typeof<sbyte>)        then unboxPrim<_> (box (fun (x:sbyte)      (y:sbyte)      -> (# "conv.i1" (# "mul" x y : int32 #) : sbyte #)))
                    elif aty.Equals(typeof<int16>)      then unboxPrim<_> (box (fun (x:int16)      (y:int16)      -> (# "conv.i2" (# "mul" x y : int32 #) : int16 #)))
                    elif aty.Equals(typeof<int32>)      then unboxPrim<_> (box (fun (x:int32)      (y:int32)      -> (# "mul" x y : int32 #)))
                    elif aty.Equals(typeof<int64>)      then unboxPrim<_> (box (fun (x:int64)      (y:int64)      -> (# "mul" x y : int64 #)))
                    elif aty.Equals(typeof<nativeint>)  then unboxPrim<_> (box (fun (x:nativeint)  (y:nativeint)  -> (# "mul" x y : nativeint #)))
                    elif aty.Equals(typeof<byte>)       then unboxPrim<_> (box (fun (x:byte)       (y:byte)       -> (# "conv.u1" (# "mul" x y : uint32 #) : byte #)))
                    elif aty.Equals(typeof<uint16>)     then unboxPrim<_> (box (fun (x:uint16)     (y:uint16)     -> (# "conv.u2" (# "mul" x y : uint32 #) : uint16 #)))
                    elif aty.Equals(typeof<uint32>)     then unboxPrim<_> (box (fun (x:uint32)     (y:uint32)     -> (# "mul" x y : uint32 #)))
                    elif aty.Equals(typeof<uint64>)     then unboxPrim<_> (box (fun (x:uint64)     (y:uint64)     -> (# "mul" x y : uint64 #)))
                    elif aty.Equals(typeof<unativeint>) then unboxPrim<_> (box (fun (x:unativeint) (y:unativeint) -> (# "mul" x y : unativeint #)))
                    elif aty.Equals(typeof<float>)      then unboxPrim<_> (box (fun (x:float)      (y:float)      -> (# "mul" x y : float #)))
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_> (box (fun (x:float32)    (y:float32)    -> (# "mul" x y : float32 #)))
                    elif aty.Equals(typeof<string>)     then unboxPrim<_> (box (fun (x:string)     (y:string)     -> System.String.Concat(x,y)))
                    else dyn()
                else dyn()

            static member Impl : ('T -> 'U -> 'V) = impl

        let MultiplyDynamic<'T,'U,'V> x n  = MultiplyDynamicImplTable<'T,'U,'V>.Impl x n

        // Dynamic implementation of checked addition operator resolution
        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
        type CheckedMultiplyDynamicImplTable<'T,'U,'V>() = 
            static let impl : ('T -> 'U -> 'V) = 
                // The dynamic implementation
                let aty = typeof<'T>
                let bty = typeof<'U>
                let cty = typeof<'V> 
                let dyn() = 
                    let ameth = aty.GetMethod("op_Multiply",[| aty; bty |])
                    let bmeth = if aty.Equals(bty) then null else bty.GetMethod("op_Multiply",[| aty; bty |])
                    match ameth,bmeth  with 
                    | null, null -> raise (NotSupportedException "dynamic invocation of op_Multiply involving coercions")
                    | m,null | null,m -> (fun x y -> unboxPrim<_> (m.Invoke(null,[| box x; box y |])))
                    | _ -> raise (NotSupportedException "dynamic invocation of op_Multiply involving overloading")
                        
                if aty.Equals(bty) && bty.Equals(cty) then
                    if aty.Equals(typeof<sbyte>)        then unboxPrim<_> (box (fun (x:sbyte)      (y:sbyte)      -> (# "conv.ovf.i1" (# "mul.ovf" x y : int32 #) : sbyte #)))
                    elif aty.Equals(typeof<int16>)      then unboxPrim<_> (box (fun (x:int16)      (y:int16)      -> (# "conv.ovf.i2" (# "mul.ovf" x y : int32 #) : int16 #)))
                    elif aty.Equals(typeof<int32>)      then unboxPrim<_> (box (fun (x:int32)      (y:int32)      -> (# "mul.ovf" x y : int32 #)))
                    elif aty.Equals(typeof<int64>)      then unboxPrim<_> (box (fun (x:int64)      (y:int64)      -> (# "mul.ovf" x y : int64 #)))
                    elif aty.Equals(typeof<nativeint>)  then unboxPrim<_> (box (fun (x:nativeint)  (y:nativeint)  -> (# "mul.ovf" x y : nativeint #)))
                    elif aty.Equals(typeof<byte>)       then unboxPrim<_> (box (fun (x:byte)       (y:byte)       -> (# "conv.ovf.u1.un" (# "mul.ovf.un" x y : uint32 #) : byte #)))
                    elif aty.Equals(typeof<uint16>)     then unboxPrim<_> (box (fun (x:uint16)     (y:uint16)     -> (# "conv.ovf.u2.un" (# "mul.ovf.un" x y : uint16 #) : uint16 #)))
                    elif aty.Equals(typeof<uint32>)     then unboxPrim<_> (box (fun (x:uint32)     (y:uint32)     -> (# "mul.ovf.un" x y : uint32 #)))
                    elif aty.Equals(typeof<uint64>)     then unboxPrim<_> (box (fun (x:uint64)     (y:uint64)     -> (# "mul.ovf.un" x y : uint64 #)))
                    elif aty.Equals(typeof<unativeint>) then unboxPrim<_> (box (fun (x:unativeint) (y:unativeint) -> (# "mul.ovf.un" x y : unativeint #)))
                    elif aty.Equals(typeof<float>)      then unboxPrim<_> (box (fun (x:float)      (y:float)      -> (# "mul" x y : float #)))
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_> (box (fun (x:float32)    (y:float32)    -> (# "mul" x y : float32 #)))
                    elif aty.Equals(typeof<string>)     then unboxPrim<_> (box (fun (x:string)     (y:string)     -> System.String.Concat(x,y)))
                    else dyn()
                else dyn()

            static member Impl : ('T -> 'U -> 'V) = impl

        let CheckedMultiplyDynamic<'T,'U,'V> x n  = CheckedMultiplyDynamicImplTable<'T,'U,'V>.Impl x n




    open BasicInlinedOperations
    open LanguagePrimitives
    open LanguagePrimitives.IntrinsicOperators
    open LanguagePrimitives.IntrinsicFunctions

#if FX_ATLEAST_40
#else        
    type Tuple<'T1> with 
        interface IStructuralComparable with
            override t.CompareTo (other:obj,comparer:System.Collections.IComparer) =
                let t2 : Tuple<'T1> = other :?> Tuple<'T1>
                comparer.Compare((box t.Item1),(box t2.Item1))
        interface IStructuralEquatable with
            override t.Equals(other:obj,comparer:System.Collections.IEqualityComparer) =
                let t2 : Tuple<'T1> = other :?> Tuple<'T1>
                comparer.Equals((box t.Item1),(box t2.Item1))
            override t.GetHashCode(comparer:System.Collections.IEqualityComparer) =
                comparer.GetHashCode(t.Item1)
        interface IComparable with
            override t.CompareTo(other:obj) =
                (t :> IStructuralComparable).CompareTo(other,(HashCompare.fsComparer :> System.Collections.IComparer))
        override t.GetHashCode() =
            (t :> IStructuralEquatable).GetHashCode(HashCompare.fsUnlimitedHasher)
        override t.Equals(other:obj) =
            (t :> IStructuralEquatable).Equals(other,HashCompare.fsEqualityComparer)

    type Tuple<'T1,'T2> with 
        override x.ToString() = 
            let sb = StringBuilder(10) in 
            sb.Append("(").Append(box(x.Item1).ToString()).Append(",")
                          .Append(box(x.Item2).ToString()).Append(")").ToString()
        interface IStructuralComparable with
            override t.CompareTo (other:obj,comparer:System.Collections.IComparer) =
                let t2 : Tuple<'T1,'T2> = other :?> Tuple<'T1,'T2>
                HashCompare.FastCompareTuple2 comparer (retype t : ('T1 * 'T2)) (retype t2)
        interface IStructuralEquatable with
            override t.Equals (other:obj,comparer:System.Collections.IEqualityComparer) =
                let t2 : Tuple<'T1,'T2> = other :?> Tuple<'T1,'T2>
                HashCompare.FastEqualsTuple2 comparer (retype t : ('T1 * 'T2)) (retype t2)
            override t.GetHashCode(comparer:System.Collections.IEqualityComparer) =
                HashCompare.FastHashTuple2 comparer (retype t : ('T1 * 'T2))
        interface IComparable with
            override t.CompareTo(other:obj) =
                (t :> IStructuralComparable).CompareTo(other, (HashCompare.fsComparer :> System.Collections.IComparer))
        override t.GetHashCode() =
            (t :> IStructuralEquatable).GetHashCode(HashCompare.fsUnlimitedHasher)
        override t.Equals(other:obj) =
            (t :> IStructuralEquatable).Equals(other,HashCompare.fsEqualityComparer)
                    

    type Tuple<'T1,'T2,'T3> with
        override x.ToString() = 
            let sb = StringBuilder(10) in 
            sb.Append("(").Append(box(x.Item1).ToString()).Append(",")
                          .Append(box(x.Item2).ToString()).Append(",")
                          .Append(box(x.Item3).ToString()).Append(")").ToString()
        interface IStructuralComparable with
            override t.CompareTo (other:obj,comparer:System.Collections.IComparer) =
                let t2 : Tuple<'T1,'T2,'T3> = other :?> Tuple<'T1,'T2,'T3>
                HashCompare.FastCompareTuple3 comparer (retype t : ('T1 * 'T2 * 'T3)) (retype t2)
        interface IStructuralEquatable with
            override t.Equals (other:obj,comparer:System.Collections.IEqualityComparer) =
                let t2 : Tuple<'T1,'T2,'T3> = other :?> Tuple<'T1,'T2,'T3>
                HashCompare.FastEqualsTuple3 comparer (retype t : ('T1 * 'T2 * 'T3)) (retype t2)
            override t.GetHashCode(comparer:System.Collections.IEqualityComparer) =
                HashCompare.FastHashTuple3 comparer (retype t : ('T1 * 'T2 * 'T3))
        interface IComparable with
            override t.CompareTo(other:obj) =
                (t :> IStructuralComparable).CompareTo(other, (HashCompare.fsComparer :> System.Collections.IComparer))
        override t.GetHashCode() =
            (t :> IStructuralEquatable).GetHashCode(HashCompare.fsUnlimitedHasher)
        override t.Equals(other:obj) =
            (t :> IStructuralEquatable).Equals(other,HashCompare.fsEqualityComparer)
            
    type Tuple<'T1,'T2,'T3,'T4> with
        override x.ToString() = 
            let sb = StringBuilder(10) in 
            sb.Append("(").Append(box(x.Item1).ToString()).Append(",")
                          .Append(box(x.Item2).ToString()).Append(",")
                          .Append(box(x.Item3).ToString()).Append(",")
                          .Append(box(x.Item4).ToString()).Append(")").ToString()
        interface IStructuralComparable with
            override t.CompareTo (other:obj,comparer:System.Collections.IComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4> = other :?> Tuple<'T1,'T2,'T3,'T4>
                HashCompare.FastCompareTuple4 comparer (retype t : ('T1 * 'T2 * 'T3 * 'T4)) (retype t2)
        interface IStructuralEquatable with
            override t.Equals (other:obj,comparer:System.Collections.IEqualityComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4> = other :?> Tuple<'T1,'T2,'T3,'T4>
                HashCompare.FastEqualsTuple4 comparer (retype t : ('T1 * 'T2 * 'T3 * 'T4)) (retype t2)
            override t.GetHashCode(comparer:System.Collections.IEqualityComparer) =
                HashCompare.FastHashTuple4 comparer (retype t : ('T1 * 'T2 * 'T3 * 'T4))
        interface IComparable with
            override t.CompareTo(other:obj) =
                (t :> IStructuralComparable).CompareTo(other, (HashCompare.fsComparer :> System.Collections.IComparer))
        override t.GetHashCode() =
            (t :> IStructuralEquatable).GetHashCode(HashCompare.fsUnlimitedHasher)
        override t.Equals(other:obj) =
            (t :> IStructuralEquatable).Equals(other,HashCompare.fsEqualityComparer)

    type Tuple<'T1,'T2,'T3,'T4,'T5> with
        override x.ToString() = 
            let sb = StringBuilder(10) in 
            sb.Append("(").Append(box(x.Item1).ToString()).Append(",")
                          .Append(box(x.Item2).ToString()).Append(",")
                          .Append(box(x.Item3).ToString()).Append(",")
                          .Append(box(x.Item4).ToString()).Append(",")
                          .Append(box(x.Item5).ToString()).Append(")").ToString()
        interface IStructuralComparable with
            override t.CompareTo (other:obj,comparer:System.Collections.IComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4,'T5> = other :?> Tuple<'T1,'T2,'T3,'T4,'T5>
                HashCompare.FastCompareTuple5 comparer (retype t : ('T1 * 'T2 * 'T3 * 'T4 * 'T5)) (retype t2)
        interface IStructuralEquatable with
            override t.Equals (other:obj,comparer:System.Collections.IEqualityComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4,'T5> = other :?> Tuple<'T1,'T2,'T3,'T4,'T5>
                HashCompare.FastEqualsTuple5 comparer (retype t : ('T1 * 'T2 * 'T3 * 'T4 * 'T5)) (retype t2)
            override t.GetHashCode(comparer:System.Collections.IEqualityComparer) =
                HashCompare.FastHashTuple5 comparer (retype t : ('T1 * 'T2 * 'T3 * 'T4 * 'T5))
        interface IComparable with
            override t.CompareTo(other:obj) =
                (t :> IStructuralComparable).CompareTo(other, (HashCompare.fsComparer :> System.Collections.IComparer))
        override t.GetHashCode() =
            (t :> IStructuralEquatable).GetHashCode(HashCompare.fsUnlimitedHasher)
        override t.Equals(other:obj) =
            (t :> IStructuralEquatable).Equals(other,HashCompare.fsEqualityComparer)

    type Tuple<'T1,'T2,'T3,'T4,'T5,'T6> with
        override x.ToString() = 
            let sb = StringBuilder(10) 
            sb.Append("(").Append(box(x.Item1).ToString()).Append(",")
                          .Append(box(x.Item2).ToString()).Append(",")
                          .Append(box(x.Item3).ToString()).Append(",")
                          .Append(box(x.Item4).ToString()).Append(",")
                          .Append(box(x.Item5).ToString()).Append(",")
                          .Append(box(x.Item6).ToString()).Append(")").ToString()
        interface IStructuralComparable with
            override t.CompareTo (other:obj,comparer:System.Collections.IComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4,'T5,'T6> = other :?> Tuple<'T1,'T2,'T3,'T4,'T5,'T6>
                let  mutable n = comparer.Compare((box t.Item1),(box t2.Item1))
                if n <> 0 then
                    n
                else
                    n <- comparer.Compare((box t.Item2), (box t2.Item2)) ;
                    if n <> 0 then
                        n
                    else
                        n <- comparer.Compare((box t.Item3), (box t2.Item3));
                        if n <> 0 then
                            n
                        else
                            n <- comparer.Compare((box t.Item4), (box t2.Item4));
                            if n <> 0 then
                                n
                            else
                                n <- comparer.Compare((box t.Item5), (box t2.Item5));
                                if n <> 0 then
                                    n
                                else
                                    comparer.Compare((box t.Item6), (box t2.Item6))
        interface IStructuralEquatable with
            override t.Equals (other:obj,comparer:System.Collections.IEqualityComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4,'T5,'T6> = other :?> Tuple<'T1,'T2,'T3,'T4,'T5,'T6>
                comparer.Equals((box t.Item1),(box t2.Item1)) && comparer.Equals((box t.Item2),(box t2.Item2)) && comparer.Equals((box t.Item3),(box t2.Item3)) && 
                    comparer.Equals((box t.Item4),(box t2.Item4)) && comparer.Equals((box t.Item5),(box t2.Item5)) && comparer.Equals((box t.Item6),(box t2.Item6))
            override t.GetHashCode(comparer:System.Collections.IEqualityComparer) =
                TupleUtils.combine_tuple_hash_codes [|(comparer.GetHashCode(t.Item1))  ; (comparer.GetHashCode(t.Item2)) ; (comparer.GetHashCode(t.Item3)) ; (comparer.GetHashCode(t.Item4)) ; (comparer.GetHashCode(t.Item5)) ; (comparer.GetHashCode(t.Item6))|]
        interface IComparable with
            override t.CompareTo(other:obj) =
                (t :> IStructuralComparable).CompareTo(other, (HashCompare.fsComparer :> System.Collections.IComparer))
        override t.GetHashCode() =
            (t :> IStructuralEquatable).GetHashCode(HashCompare.fsUnlimitedHasher)
        override t.Equals(other:obj) =
            (t :> IStructuralEquatable).Equals(other,HashCompare.fsEqualityComparer)

    type Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7> with
        override x.ToString() = 
            let sb = StringBuilder(10) 
            sb.Append("(").Append(box(x.Item1).ToString()).Append(",")
                          .Append(box(x.Item2).ToString()).Append(",")
                          .Append(box(x.Item3).ToString()).Append(",")
                          .Append(box(x.Item4).ToString()).Append(",")
                          .Append(box(x.Item5).ToString()).Append(",")
                          .Append(box(x.Item6).ToString()).Append(",")
                          .Append(box(x.Item7).ToString()).Append(")").ToString()
        interface IStructuralComparable with
            override t.CompareTo (other:obj,comparer:System.Collections.IComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7> = other :?> Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7>
                let  mutable n = comparer.Compare((box t.Item1),(box t2.Item1))
                if n <> 0 then
                    n
                else
                    n <- comparer.Compare((box t.Item2), (box t2.Item2)) ;
                    if n <> 0 then
                        n
                    else
                        n <- comparer.Compare((box t.Item3), (box t2.Item3));
                        if n <> 0 then
                            n
                        else
                            n <- comparer.Compare((box t.Item4), (box t2.Item4));
                            if n <> 0 then
                                n
                            else
                                n <- comparer.Compare((box t.Item5), (box t2.Item5));
                                if n <> 0 then
                                    n
                                else
                                    n <- comparer.Compare((box t.Item6), (box t2.Item6));
                                    if n <> 0 then
                                        n
                                    else
                                        comparer.Compare((box t.Item7), (box t2.Item7))
        interface IStructuralEquatable with
            override t.Equals (other:obj,comparer:System.Collections.IEqualityComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7> = other :?> Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7>
                comparer.Equals((box t.Item1),(box t2.Item1)) && comparer.Equals((box t.Item2),(box t2.Item2)) && comparer.Equals((box t.Item3),(box t2.Item3)) && 
                    comparer.Equals((box t.Item4),(box t2.Item4)) && comparer.Equals((box t.Item5),(box t2.Item5)) && comparer.Equals((box t.Item6),(box t2.Item6)) && comparer.Equals((box t.Item7),(box t2.Item7))
            override t.GetHashCode(comparer:System.Collections.IEqualityComparer) =
                TupleUtils.combine_tuple_hash_codes [|(comparer.GetHashCode(t.Item1))  ; (comparer.GetHashCode(t.Item2)) ; (comparer.GetHashCode(t.Item3)) ; (comparer.GetHashCode(t.Item4)) ; (comparer.GetHashCode(t.Item5)) ; (comparer.GetHashCode(t.Item6)) ; (comparer.GetHashCode(t.Item7))|]
        interface IComparable with
            override t.CompareTo(other:obj) =
                (t :> IStructuralComparable).CompareTo(other, (HashCompare.fsComparer :> System.Collections.IComparer))
        override t.GetHashCode() =
            (t :> IStructuralEquatable).GetHashCode(HashCompare.fsUnlimitedHasher)
        override t.Equals(other:obj) =
            (t :> IStructuralEquatable).Equals(other,HashCompare.fsEqualityComparer)
            
    type Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'TRest> with
        override x.ToString() = 
            let sb = StringBuilder(10) 
            sb.Append("(").Append(box(x.Item1).ToString()).Append(",")
                          .Append(box(x.Item2).ToString()).Append(",")
                          .Append(box(x.Item3).ToString()).Append(",")
                          .Append(box(x.Item4).ToString()).Append(",")
                          .Append(box(x.Item5).ToString()).Append(",")
                          .Append(box(x.Item6).ToString()).Append(",")
                          .Append(box(x.Item7).ToString()).Append(",")
                          .Append("...").Append(")").ToString()
        interface IStructuralComparable with
            override t.CompareTo (other:obj,comparer:System.Collections.IComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'TRest> = other :?> Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'TRest>
                let  mutable n = comparer.Compare((box t.Item1),(box t2.Item1))
                if n <> 0 then
                    n
                else
                    n <- comparer.Compare((box t.Item2), (box t2.Item2)) ;
                    if n <> 0 then
                        n
                    else
                        n <- comparer.Compare((box t.Item3), (box t2.Item3));
                        if n <> 0 then
                            n
                        else
                            n <- comparer.Compare((box t.Item4), (box t2.Item4));
                            if n <> 0 then
                                n
                            else
                                n <- comparer.Compare((box t.Item5), (box t2.Item5));
                                if n <> 0 then
                                    n
                                else
                                    n <- comparer.Compare((box t.Item6), (box t2.Item6));
                                    if n <> 0 then
                                        n
                                    else
                                        n <- comparer.Compare((box t.Item7), (box t2.Item7))
                                        if n <> 0 then
                                            n
                                        else
                                            comparer.Compare((box t.Rest), (box t2.Rest))
        interface IStructuralEquatable with
            override t.Equals (other:obj,comparer:System.Collections.IEqualityComparer) =
                let t2 : Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'TRest> = other :?> Tuple<'T1,'T2,'T3,'T4,'T5,'T6,'T7,'TRest>
                comparer.Equals((box t.Item1),(box t2.Item1)) && comparer.Equals((box t.Item2),(box t2.Item2)) && comparer.Equals((box t.Item3),(box t2.Item3)) && 
                    comparer.Equals((box t.Item4),(box t2.Item4)) && comparer.Equals((box t.Item5),(box t2.Item5)) && comparer.Equals((box t.Item6),(box t2.Item6)) && 
                    comparer.Equals((box t.Item7),(box t2.Item7)) && comparer.Equals((box t.Rest),(box t2.Rest))
            override t.GetHashCode(comparer:System.Collections.IEqualityComparer) =
                TupleUtils.combine_tuple_hash_codes [|(comparer.GetHashCode(t.Item1))  ; (comparer.GetHashCode(t.Item2)) ; (comparer.GetHashCode(t.Item3)) ; (comparer.GetHashCode(t.Item4)) ; (comparer.GetHashCode(t.Item5)) ; (comparer.GetHashCode(t.Item6)) ; (comparer.GetHashCode(t.Item7)) ; (comparer.GetHashCode(t.Rest))|]
        interface IComparable with
            override t.CompareTo(other:obj) =
                (t :> IStructuralComparable).CompareTo(other, (HashCompare.fsComparer :> System.Collections.IComparer))
        override t.GetHashCode() =
            (t :> IStructuralEquatable).GetHashCode(HashCompare.fsUnlimitedHasher)
        override t.Equals(other:obj) =
            (t :> IStructuralEquatable).Equals(other,HashCompare.fsEqualityComparer)
#endif

    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2> = 
      | Choice1Of2 of 'T1 
      | Choice2Of2 of 'T2
    
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3> = 
      | Choice1Of3 of 'T1 
      | Choice2Of3 of 'T2
      | Choice3Of3 of 'T3
    
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3,'T4> = 
      | Choice1Of4 of 'T1 
      | Choice2Of4 of 'T2
      | Choice3Of4 of 'T3
      | Choice4Of4 of 'T4
    
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3,'T4,'T5> = 
      | Choice1Of5 of 'T1 
      | Choice2Of5 of 'T2
      | Choice3Of5 of 'T3
      | Choice4Of5 of 'T4
      | Choice5Of5 of 'T5
    
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3,'T4,'T5,'T6> = 
      | Choice1Of6 of 'T1
      | Choice2Of6 of 'T2
      | Choice3Of6 of 'T3
      | Choice4Of6 of 'T4
      | Choice5Of6 of 'T5
      | Choice6Of6 of 'T6
    
    [<DefaultAugmentation(false)>]
    type Choice<'T1,'T2,'T3,'T4,'T5,'T6,'T7> = 
      | Choice1Of7 of 'T1
      | Choice2Of7 of 'T2
      | Choice3Of7 of 'T3
      | Choice4Of7 of 'T4
      | Choice5Of7 of 'T5
      | Choice6Of7 of 'T6
      | Choice7Of7 of 'T7
          
    //-------------------------------------------------------------------------
    // F#-specific Exceptions

    exception Failure of string with 
        override x.Message  = (match (x :> exn) with Failure(s) -> s)

    exception MatchFailure of string * int * int with 
        override x.Message  = (match (x :> exn) with MatchFailure(s,n,m) -> "The match cases were incomplete")

    exception DynamicInvocationNotSupported of string 

    //-------------------------------------------------------------------------
    // Function Values

    [<AbstractClass>]
    type TypeFunc() = 
        abstract Specialize<'T> : unit -> obj

    [<AbstractClass>]
    type FastFunc<'T,'Res>() = 
        abstract Invoke : 'T -> 'Res

    module OptimizedClosures = 

          [<AbstractClass>]
          type FastFunc2 <'T,'U,'V>() = 
              inherit FastFunc<'T,('U -> 'V)>()
              abstract Invoke : 'T * 'U -> 'V
              override f.Invoke(t) = (fun u -> f.Invoke(t,u))
              static member Adapt(f : 'T -> 'U -> 'V) = 
                  match box f with 
                  | :? FastFunc2 <'T,'U,'V> as f -> f
                  | _ -> { new FastFunc2<'T,'U,'V>() with 
                              member x.Invoke(t,u) = f t u }

          [<AbstractClass>]
          type FastFunc3<'T,'U,'V,'W>() = 
              inherit FastFunc<'T,('U -> 'V -> 'W)>()
              abstract Invoke : 'T * 'U * 'V -> 'W
              override f.Invoke(t) = (fun u v -> f.Invoke(t,u,v))
              static member Adapt(f : 'T -> 'U -> 'V -> 'W) = 
                  match box f with 
                  | :? FastFunc3<'T,'U,'V,'W> as f -> f
                  | :? FastFunc2<'T,'U,FastFunc<'V,'W>> as f ->
                         { new FastFunc3<'T,'U,'V,'W>() with 
                              member x.Invoke(t,u,v) = f.Invoke(t,u).Invoke(v) }
                  | _ -> { new FastFunc3<'T,'U,'V,'W>() with 
                              member x.Invoke(t,u,v) = f t u v }

          [<AbstractClass>]
          type FastFunc4 <'T,'U,'V,'W,'X>() = 
              inherit FastFunc<'T,('U -> 'V -> 'W -> 'X)>()
              abstract Invoke : 'T * 'U * 'V * 'W -> 'X
              static member Adapt(f : 'T -> 'U -> 'V -> 'W -> 'X) = 
                  match box f with 
                  | :? FastFunc4<'T,'U,'V,'W,'X> as f -> f
                  | :? FastFunc3<'T,'U,'V,FastFunc<'W,'X>> as f ->
                         { new FastFunc4<'T,'U,'V,'W,'X>() with 
                              member x.Invoke(t,u,v,w) = f.Invoke(t,u,v).Invoke(w) }
                  | :? FastFunc2<'T,'U,FastFunc<'V,FastFunc<'W,'X>>> as f ->
                         { new FastFunc4<'T,'U,'V,'W,'X>() with 
                              member x.Invoke(t,u,v,w) = f.Invoke(t,u).Invoke(v).Invoke(w) }
                  | _ -> { new FastFunc4<'T,'U,'V,'W,'X>() with 
                              member x.Invoke(t,u,v,w) = f t u v w }
              override f.Invoke(t) = (fun u v w -> f.Invoke(t,u,v,w))

          [<AbstractClass>]
          type FastFunc5<'T,'U,'V,'W,'X,'Y>() =
              inherit FastFunc<'T,('U -> 'V -> 'W -> 'X -> 'Y)>()
              abstract Invoke : 'T * 'U * 'V * 'W * 'X -> 'Y
              override f.Invoke(t) = (fun u v w x -> f.Invoke(t,u,v,w,x))
              static member Adapt(f : 'T -> 'U -> 'V -> 'W -> 'X -> 'Y) = 
                  match box f with 
                  | :? FastFunc5<'T,'U,'V,'W,'X,'Y> as f -> f
                  | :? FastFunc4<'T,'U,'V,'W,FastFunc<'X,'Y>> as f ->
                         { new FastFunc5<'T,'U,'V,'W,'X,'Y>() with 
                              member ff.Invoke(t,u,v,w,x) = f.Invoke(t,u,v,w).Invoke(x) }
                  | :? FastFunc3<'T,'U,'V,FastFunc<'W,FastFunc<'X,'Y>>> as f ->
                         { new FastFunc5<'T,'U,'V,'W,'X,'Y>() with 
                              member ff.Invoke(t,u,v,w,x) = f.Invoke(t,u,v).Invoke(w).Invoke(x) }
                  | :? FastFunc2<'T,'U,FastFunc<'V,FastFunc<'W,FastFunc<'X,'Y>>>> as f ->
                         { new FastFunc5<'T,'U,'V,'W,'X,'Y>() with 
                              member ff.Invoke(t,u,v,w,x) = f.Invoke(t,u).Invoke(v).Invoke(w).Invoke(x) }
                  | _ -> { new FastFunc5<'T,'U,'V,'W,'X,'Y>() with 
                              member ff.Invoke(t,u,v,w,x) = f t u v w x }
          
          let inline invokeFast2((f1 : FastFunc<'T,('U -> 'V)>), t,u) =
              match f1 with
              | :? FastFunc2<'T,'U,'V> as f2 -> f2.Invoke(t,u)
              | _                            -> (f1.Invoke(t)) u     
          
          let inline invokeFast3((f1 : FastFunc<'T,('U -> 'V -> 'W)>), t,u,v) =
               match f1 with
               | :? FastFunc3<'T,'U,'V,'W>      as f3 -> f3.Invoke(t,u,v)
               | :? FastFunc2<'T,'U,('V -> 'W)> as f2 -> (f2.Invoke(t,u)) v
               | _                                    -> (f1.Invoke(t)) u v
               
          let inline invokeFast4((f1 : FastFunc<'T,('U -> 'V -> 'W -> 'X)>), t,u,v,w) =
               match f1 with
               | :? FastFunc4<'T,'U,'V,'W,'X>         as f4 -> f4.Invoke(t,u,v,w)
               | :? FastFunc3<'T,'U,'V,('W -> 'X)>    as f3 -> (f3.Invoke(t,u,v)) w
               | :? FastFunc2<'T,'U,('V -> 'W -> 'X)> as f2 -> (f2.Invoke(t,u)) v w
               | _                                          -> (f1.Invoke(t)) u v w

          let inline invokeFast5((f1 : FastFunc<'T,('U -> 'V -> 'W -> 'X -> 'Y)>), t,u,v,w,x) =
               match f1 with
               | :? FastFunc5<'T,'U,'V,'W,'X,'Y>             as f5 -> f5.Invoke(t,u,v,w,x)
               | :? FastFunc4<'T,'U,'V,'W,('X -> 'Y)>        as f4 -> (f4.Invoke(t,u,v,w)) x
               | :? FastFunc3<'T,'U,'V,('W -> 'X -> 'Y)>     as f3 -> (f3.Invoke(t,u,v)) w x
               | :? FastFunc2<'T,'U,('V -> 'W -> 'X -> 'Y)>  as f2 -> (f2.Invoke(t,u)) v w x
               | _                                                 -> (f1.Invoke(t)) u v w x


    type FastFunc<'T,'Res> with
        [<CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")>]
        [<OverloadID("FromConverter")>]   
        static member op_Implicit(f : System.Converter<_,_>) : ('T -> 'Res) =  (fun t -> f.Invoke(t))
        [<CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")>]
        [<OverloadID("ToConverter")>]   
        static member op_Implicit( f : ('T -> 'Res) ) =  new System.Converter<'T,'Res>(f)

        static member FromConverter(f : System.Converter<_,_>) : ('T -> 'Res) =  (fun t -> f.Invoke(t))
        static member ToConverter( f : ('T -> 'Res) ) =  new System.Converter<'T,'Res>(f)

        static member InvokeFast(f:FastFunc<_,_>,(t:'T),(u:'Res))       = OptimizedClosures.invokeFast2(f,t,u) 
        static member InvokeFast(f:FastFunc<_,_>,(t:'T),(u:'Res),v)     = OptimizedClosures.invokeFast3(f,t,u,v)
        static member InvokeFast(f:FastFunc<_,_>,(t:'T),(u:'Res),v,w)   = OptimizedClosures.invokeFast4(f,t,u,v,w)
        static member InvokeFast(f:FastFunc<_,_>,(t:'T),(u:'Res),v,w,x) = OptimizedClosures.invokeFast5(f,t,u,v,w,x)

    [<AbstractClass>]
    [<Sealed>]
    type FuncConvert = 
        [<OverloadID("Action1")>]
        static member  ToFastFunc( f : Action<_>) = (fun t -> f.Invoke(t))
        [<OverloadID("Converter1")>]
        static member  ToFastFunc( f : Converter<_,_>) = (fun t -> f.Invoke(t))
        [<OverloadID("FuncFromTupled1")>]
        static member FuncFromTupled (f:'T -> 'Res) = (fun a -> f a)
        [<OverloadID("FuncFromTupled2")>]
        static member FuncFromTupled (f:'T1 * 'T2 -> 'Res) = (fun a b -> f (a, b))
        [<OverloadID("FuncFromTupled3")>]
        static member FuncFromTupled (f:'T1 * 'T2 * 'T3 -> 'Res) = (fun a b c -> f (a, b, c))
        [<OverloadID("FuncFromTupled4")>]
        static member FuncFromTupled (f:'T1 * 'T2 * 'T3 * 'T4 -> 'Res) = (fun a b c d -> f (a, b, c, d))
        [<OverloadID("FuncFromTupled5")>]
        static member FuncFromTupled (f:'T1 * 'T2 * 'T3 * 'T4 * 'T5 -> 'Res) = (fun a b c d e-> f (a, b, c, d, e))



    //-------------------------------------------------------------------------
    // Refs
    //-------------------------------------------------------------------------

    [<assembly:CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Scope="member", Target="Microsoft.FSharp.Core.Ref`1.#_contents")>]
    do()

#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("{contents}")>]
#endif
    type Ref<'T> = 
        { 
#if FX_NO_DEBUG_DISPLAYS
#else
          [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
          mutable contents: 'T }
        member x.Value 
            with get() = x.contents
            and  set v = x.contents <- v

    and 'T ref = Ref<'T> 

    //-------------------------------------------------------------------------
    // Options
    //-------------------------------------------------------------------------

    [<DefaultAugmentation(false)>]
#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("Some({Value})")>]
#endif
    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Option")>]
    type Option<'T> = 
        | None :       'T option
        | Some : value:'T -> 'T option 

        [<CompilationRepresentation(CompilationRepresentationFlags.Instance)>]
        member x.Value = match x with Some x -> x | None -> raise (new System.InvalidOperationException("Option.Value"))

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member x.IsNone = match x with None -> true | _ -> false

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member x.IsSome = match x with Some _ -> true | _ -> false

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        static member None : 'T option = None

        static member Some(x) : 'T option = Some(x)

        override x.ToString() = "Some("^x.Value.ToString()^")"

    and 'T option = 'T Option 



//============================================================================
//============================================================================
namespace Microsoft.FSharp.Collections

    //-------------------------------------------------------------------------
    // Lists
    //-------------------------------------------------------------------------

    open System.Collections.Generic
    open System.Diagnostics
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicFunctions
    open Microsoft.FSharp.Core.BasicInlinedOperations

    [<DefaultAugmentation(false)>]
#if FX_NO_DEBUG_PROXIES
#else
    [<System.Diagnostics.DebuggerTypeProxyAttribute(typedefof<ListDebugView<_>>)>]
#endif
#if FX_NO_DEBUG_DISPLAYS
#else
    [<DebuggerDisplay("{DebugDisplay}")>]
#endif
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")>]
    type FSharpList<'T> = 
       | ([])  :                 'T list
       | (::)  : _Head: 'T * _Tail: 'T list -> 'T list
       interface System.Collections.Generic.IEnumerable<'T>
       interface System.Collections.IEnumerable
        
    and 'T list = FSharpList<'T>
    and List<'T> = FSharpList<'T>

    //-------------------------------------------------------------------------
    // List (debug view)
    //-------------------------------------------------------------------------

    and 
       ListDebugView<'T>(l:list<'T>) =

           let rec count l n = match l with [] -> n | _::t -> if n > 50 then n else count t (n+1) 

#if FX_NO_DEBUG_DISPLAYS
#else
           [<DebuggerBrowsable(DebuggerBrowsableState.RootHidden)>]
#endif
           member x.Items =
               let n = count l 0 
               let items = zero_create n 
               let rec copy (items: 'T[]) l i = 
                 match l with
                 | [] -> () 
                 | h::t -> if i < n then (SetArray items (i,h); copy items t (i+1)) 
               copy items l 0;
               items

    type ResizeArray<'T> = System.Collections.Generic.List<'T>

    //-------------------------------------------------------------------------
    // List (augmentation)
    //-------------------------------------------------------------------------

    module PrivateListHelpers = 

        let notStarted() = raise (new System.InvalidOperationException("Enumeration has not started. Call MoveNext."))
        let alreadyFinished() = raise (new System.InvalidOperationException("Enumeration already finished."))
        let nonempty x = match x with [] -> false | _ -> true

        type ListEnumerator<'T> (s: 'T list) = 
              let mutable curr = s 
              let mutable started = false 

              member x.GetCurrent() = 
                  if started then 
                      match curr with 
                      | [] -> alreadyFinished()
                      | h::t -> h
                  else 
                      notStarted()

              interface IEnumerator<'T> with 
                  member x.Current = x.GetCurrent()

              interface System.Collections.IEnumerator with 
                  member x.MoveNext() = 
                       if started then 
                         match curr with 
                         | h :: t -> (curr <- t; nonempty curr)
                         | _ -> false
                       else 
                         (started <- true; nonempty curr)
                  member x.Current = box (x.GetCurrent())
                  member x.Reset() = started <- false; curr <- s

              interface System.IDisposable with 
                  member x.Dispose() = () 

        let mkListEnumerator s = (new ListEnumerator<'T>(s) :> IEnumerator<'T>)

        let rec lengthAcc acc l = match l with [] -> acc | h::t -> lengthAcc (acc+1) t 

        let rec nth l n = 
            match l with 
            | [] -> raise (new System.ArgumentException("the index was outside the range of elements in the list","n"))
            | h::t -> 
               if n < 0 then raise (new System.ArgumentException("the index was negative","n"))
               elif n = 0 then h
               else nth t (n - 1)

    type FSharpList<'T> with
#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member l.Length = PrivateListHelpers.lengthAcc 0 l

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member l.DebugDisplay = 
           let n = l.Length
           let txt = 
               if n > 1000 then "Length > 1000"
               else System.String.Concat( [| "Length = "; n.ToString() |])
           new System.Text.StringBuilder(txt)
       
        
        
        member l.Head   = match l with a :: b -> a | [] -> raise (System.InvalidOperationException("the list is empty"))
        member l.Tail   = match l with a :: b -> b | [] -> raise (System.InvalidOperationException("the list is empty"))
        
#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member l.IsCons = match l with [] -> false | _ -> true

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member l.IsEmpty  = match l with [] -> true | _ -> false
        member l.Item with get(index) = PrivateListHelpers.nth l index

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        static member Empty       : 'T list = []

        static member Cons(head,tail) : 'T list = head::tail

        interface IEnumerable<'T> with
            member l.GetEnumerator() = PrivateListHelpers.mkListEnumerator l

        interface System.Collections.IEnumerable with
            member l.GetEnumerator() = (PrivateListHelpers.mkListEnumerator l :> System.Collections.IEnumerator)

    type seq<'T> = IEnumerable<'T>

        
//-------------------------------------------------------------------------
// Operators
//-------------------------------------------------------------------------


namespace Microsoft.FSharp.Core

    open System
    open System.Diagnostics              
    open System.Collections.Generic
    open System.Globalization
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicFunctions
    open Microsoft.FSharp.Core.BasicInlinedOperations
    open Microsoft.FSharp.Collections



    [<CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1046:DoNotOverloadOperatorEqualsOnReferenceTypes")>]
    module Operators = 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let seq x = x (* temp addition for comprehension/workflow transitions *)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline unbox (x:obj) = UnboxGeneric(x)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline box   (x:'T)  = (# "box !0"       type ('T) x : obj #)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let raise (e: exn) = (# "throw" e : 'T #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline not (b:bool) = (# "ceq" b false : bool #)
           

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline objEq (xobj:obj) (yobj:obj) = (# "ceq" xobj yobj : bool #)


        let inline (<) x y = GenericLessThan x y
        let inline (>) x y = GenericGreaterThan x y
        let inline (>=) x y = GenericGreaterOrEqual x y
        let inline (<=) x y = GenericLessOrEqual x y
        let inline (=) x y = GenericEquality x y
        let inline (<>) x y = not (GenericEquality x y)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline compare (x:'T) (y:'T) = GenericComparison x y

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline max x y = GenericMaximum x y

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline min x y = GenericMinimum x y


(*
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let ``assert`` (b) = System.Diagnostics.Debug.Assert(b)
*)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let failwith s = raise (Failure(s))

        let InvalidArgument message = (new ArgumentException(message:string) :> exn)

        type InvalidArgumentException = ArgumentException

        let (|InvalidArgument|_|) (exn:exn) = match exn with :? ArgumentException as e -> Some(e.Message) | _ -> None


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let invalid_arg message = raise (InvalidArgument(message))

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let invalidArg (arg:string) (msg:string) = raise (new System.ArgumentException(msg,arg))

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let nullArg (arg:string) = raise (new System.ArgumentNullException(arg))        

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let invalidOp s = raise (System.InvalidOperationException(s))

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let invalid_op s = raise (System.InvalidOperationException(s))

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let not_found() = raise (new KeyNotFoundException("The item, key or index was not found in the collection"))

        [<NoDynamicInvocation>]
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline rethrow() = unbox<_>(# "rethrow ldnull" : System.Object #)
        

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let fst (a,b) = a

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let snd (a,b) = b

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let ignore x = ()

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let ref x = { contents = x }

        let (:=) x y = x.contents <- y

        let (!) x = x.contents

        let inline (|>) x f = f x

        let inline (||>) (x1,x2) f = f x1 x2

        let inline (|||>) (x1,x2,x3) f = f x1 x2 x3

        let inline (<|) f x = f x

        let inline (<||) f (x1,x2) = f x1 x2

        let inline (<|||) f (x1,x2,x3) = f x1 x2 x3

        let inline (>>) f g x = g(f x)

        let inline (<<) f g x = f(g x)

        let (^) (x:string) (y:string) = System.String.Concat(x,y)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let defaultArg x y = match x with None -> y | Some v -> v
        

        let inline (~-) (x: ^T) : ^T = 
            (^T : (static member (~-) : ^T -> ^T) (x))
             when ^T : int32     = (# "neg" x  : int32 #)
             when ^T : float     = (# "neg" x  : float #)
             when ^T : float32   = (# "neg" x  : float32 #)
             when ^T : int64     = (# "neg" x  : int64 #)
             when ^T : int16     = (# "neg" x  : int16 #)
             when ^T : nativeint = (# "neg" x  : nativeint #)
             when ^T : sbyte     = (# "neg" x  : sbyte #)
             when ^T : decimal   = (# "" (System.Decimal.op_UnaryNegation((# "" x : decimal #))) : ^T #)


        let inline (+) (x: ^T) (y: ^U) : ^V = 
             AdditionDynamic<(^T),(^U),(^V)>  x y 
             when ^T : int32       and ^U : int32      = (# "add" x y : int32 #)
             when ^T : float       and ^U : float      = (# "add" x y : float #)
             when ^T : float32     and ^U : float32    = (# "add" x y : float32 #)
             when ^T : int64       and ^U : int64      = (# "add" x y : int64 #)
             when ^T : uint64      and ^U : uint64     = (# "add" x y : uint64 #)
             when ^T : uint32      and ^U : uint32     = (# "add" x y : uint32 #)
             when ^T : nativeint   and ^U : nativeint  = (# "add" x y : nativeint #)
             when ^T : unativeint  and ^U : unativeint = (# "add" x y : unativeint #)
             when ^T : int16       and ^U : int16      = (# "conv.i2" (# "add" x y : int32 #) : int16 #)
             when ^T : uint16      and ^U : uint16     = (# "conv.u2" (# "add" x y : uint32 #) : uint16 #)
             when ^T : char        and ^U : char       = (# "conv.u2" (# "add" x y : uint32 #) : char #)
             when ^T : sbyte       and ^U : sbyte      = (# "conv.i1" (# "add" x y : int32 #) : sbyte #)
             when ^T : byte        and ^U : byte       = (# "conv.u1" (# "add" x y : uint32 #) : byte #)
             when ^T : string      and ^U : string     = (# "" (System.String.Concat((# "" x : string #),(# "" y : string #))) : ^T #)
             when ^T : decimal     and ^U : decimal    = (# "" (System.Decimal.op_Addition((# "" x : decimal #),(# "" y : decimal #))) : ^V #)
             // According to the somewhat subtle rules of static optimizations,
             // this condition is used whenever ^T is resolved to a nominal type
             // That is, not in the generic implementation of '+'
             when ^T : ^T = ((^T or ^U): (static member (+) : ^T * ^U -> ^V) (x,y))

        let inline (-) (x: ^T) (y: ^U) : ^V = 
             ((^T or ^U): (static member (-) : ^T * ^U -> ^V) (x,y))
             when ^T : int32      and ^U : int32      = (# "sub" x y : int32 #)
             when ^T : float      and ^U : float      = (# "sub" x y : float #)
             when ^T : float32    and ^U : float32    = (# "sub" x y : float32 #)
             when ^T : int64      and ^U : int64      = (# "sub" x y : int64 #)
             when ^T : uint64     and ^U : uint64     = (# "sub" x y : uint64 #)
             when ^T : uint32     and ^U : uint32     = (# "sub" x y : uint32 #)
             when ^T : nativeint  and ^U : nativeint  = (# "sub" x y : nativeint #)
             when ^T : unativeint and ^U : unativeint = (# "sub" x y : unativeint #)
             when ^T : int16       and ^U : int16      = (# "conv.i2" (# "sub" x y : int32 #) : int16 #)
             when ^T : uint16      and ^U : uint16     = (# "conv.u2" (# "sub" x y : uint32 #) : uint16 #)
             when ^T : sbyte       and ^U : sbyte      = (# "conv.i1" (# "sub" x y : int32 #) : sbyte #)
             when ^T : byte        and ^U : byte       = (# "conv.u1" (# "sub" x y : uint32 #) : byte #)
             when ^T : decimal     and ^U : decimal    = (# "" (System.Decimal.op_Subtraction((# "" x : decimal #),(# "" y : decimal #))) : ^V #)


        let inline ( * ) (x: ^T) (y: ^U) : ^V = 
             MultiplyDynamic<(^T),(^U),(^V)>  x y 
             when ^T : int32      and ^U : int32      = (# "mul" x y : int32 #)
             when ^T : float      and ^U : float      = (# "mul" x y : float #)
             when ^T : float32    and ^U : float32    = (# "mul" x y : float32 #)
             when ^T : int64      and ^U : int64      = (# "mul" x y : int64 #)
             when ^T : uint64     and ^U : uint64     = (# "mul" x y : uint64 #)
             when ^T : uint32     and ^U : uint32     = (# "mul" x y : uint32 #)
             when ^T : nativeint  and ^U : nativeint  = (# "mul" x y : nativeint #)
             when ^T : unativeint and ^U : unativeint = (# "mul" x y : unativeint #)
             when ^T : int16       and ^U : int16      = (# "conv.i2" (# "mul" x y : int32 #) : int16 #)
             when ^T : uint16      and ^U : uint16     = (# "conv.u2" (# "mul" x y : uint32 #) : uint16 #)
             when ^T : sbyte       and ^U : sbyte      = (# "conv.i1" (# "mul" x y : int32 #) : sbyte #)
             when ^T : byte        and ^U : byte       = (# "conv.u1" (# "mul" x y : uint32 #) : byte #)
             when ^T : decimal     and ^U : decimal    = (# "" (System.Decimal.op_Multiply((# "" x : decimal #),(# "" y : decimal #))) : ^V #)
             // According to the somewhat subtle rules of static optimizations,
             // this condition is used whenever ^T is resolved to a nominal type
             // That is, not in the generic implementation of '*'
             when ^T : ^T = ((^T or ^U): (static member (*) : ^T * ^U -> ^V) (x,y))

        let inline ( / ) (x: ^T) (y: ^U) : ^V = 
             ((^T or ^U): (static member (/) : ^T * ^U -> ^V) (x,y))
             when ^T : int32       and ^U : int32      = (# "div" x y : int32 #)
             when ^T : float       and ^U : float      = (# "div" x y : float #)
             when ^T : float32     and ^U : float32    = (# "div" x y : float32 #)
             when ^T : int64       and ^U : int64      = (# "div" x y : int64 #)
             when ^T : uint64      and ^U : uint64     = (# "div.un" x y : uint64 #)
             when ^T : uint32      and ^U : uint32     = (# "div.un" x y : uint32 #)
             when ^T : nativeint   and ^U : nativeint  = (# "div" x y : nativeint #)
             when ^T : unativeint  and ^U : unativeint = (# "div.un" x y : unativeint #)
             when ^T : int16       and ^U : int16      = (# "conv.i2" (# "div" x y : int32 #) : int16 #)
             when ^T : uint16      and ^U : uint16     = (# "conv.u2" (# "div.un" x y : uint32 #) : uint16 #)
             when ^T : sbyte       and ^U : sbyte      = (# "conv.i1" (# "div" x y : int32 #) : sbyte #)
             when ^T : byte        and ^U : byte       = (# "conv.u1" (# "div.un" x y : uint32 #) : byte #)
             when ^T : decimal     and ^U : decimal    = (# "" (System.Decimal.op_Division((# "" x : decimal #),(# "" y : decimal #))) : ^V #)

        let inline ( % ) (x: ^T) (y: ^U) : ^V = 
             ((^T or ^U): (static member (%) : ^T * ^U -> ^V) (x,y))
             when ^T : int32       and ^U : int32      = (# "rem" x y : int32 #)
             when ^T : float       and ^U : float      = (# "rem" x y : float #)
             when ^T : float32     and ^U : float32    = (# "rem" x y : float32 #)
             when ^T : int64       and ^U : int64      = (# "rem" x y : int64 #)
             when ^T : uint64      and ^U : uint64     = (# "rem.un" x y : uint64 #)
             when ^T : uint32      and ^U : uint32     = (# "rem.un" x y : uint32 #)
             when ^T : nativeint   and ^U : nativeint  = (# "rem" x y : nativeint #)
             when ^T : unativeint  and ^U : unativeint = (# "rem.un" x y : unativeint #)
             when ^T : int16       and ^U : int16      = (# "conv.i2" (# "rem"    x y : int32  #) : int16  #)
             when ^T : uint16      and ^U : uint16     = (# "conv.u2" (# "rem.un" x y : uint32 #) : uint16 #)
             when ^T : sbyte       and ^U : sbyte      = (# "conv.i1" (# "rem"    x y : int32  #) : sbyte  #)
             when ^T : byte        and ^U : byte       = (# "conv.u1" (# "rem.un" x y : uint32 #) : byte   #)
             when ^T : decimal     and ^U : decimal    = (# "" (System.Decimal.op_Modulus((# "" x : decimal #),(# "" y : decimal #))) : ^V #)

        let inline (~+) (x: ^T) : ^T =
             (^T: (static member (~+) : ^T -> ^T) (x))
             when ^T : int32      = x
             when ^T : float      = x
             when ^T : float32    = x
             when ^T : int64      = x
             when ^T : uint64     = x
             when ^T : uint32     = x
             when ^T : int16      = x
             when ^T : uint16     = x
             when ^T : nativeint  = x
             when ^T : unativeint = x
             when ^T : sbyte      = x
             when ^T : byte       = x
             when ^T : decimal    = x

        let inline mask (n:int) (m:int) = (# "and" n m : int #)
        let inline (<<<) (x: ^T) (n:int) : ^T = 
             (^T: (static member (<<<) : ^T * int -> ^T) (x,n))
             when ^T : int32      = (# "shl" x (mask n 31) : int #)
             when ^T : uint32     = (# "shl" x (mask n 31) : uint32 #)
             when ^T : int64      = (# "shl" x (mask n 63) : int64 #)
             when ^T : uint64     = (# "shl" x (mask n 63) : uint64 #)
             when ^T : nativeint  = (# "shl" x n : nativeint #)
             when ^T : unativeint = (# "shl" x n : unativeint #)
             when ^T : int16      = (# "conv.i2" (# "shl" x (mask n 15) : int32  #) : int16 #)
             when ^T : uint16     = (# "conv.u2" (# "shl" x (mask n 15) : uint32 #) : uint16 #)
             when ^T : sbyte      = (# "conv.i1" (# "shl" x (mask n 7 ) : int32  #) : sbyte #)
             when ^T : byte       = (# "conv.u1" (# "shl" x (mask n 7 ) : uint32 #) : byte #)

        let inline (>>>) (x: ^T) (n:int) : ^T = 
             (^T: (static member (>>>) : ^T * int -> ^T) (x,n))
             when ^T : int32      = (# "shr"    x (mask n 31) : int32 #)
             when ^T : uint32     = (# "shr.un" x (mask n 31) : uint32 #)
             when ^T : int64      = (# "shr"    x (mask n 63) : int64 #)
             when ^T : uint64     = (# "shr.un" x (mask n 63) : uint64 #)
             when ^T : nativeint  = (# "shr"    x n : nativeint #)
             when ^T : unativeint = (# "shr.un" x n : unativeint #)
             when ^T : int16      = (# "conv.i2" (# "shr"    x (mask n 15) : int32  #) : int16 #)
             when ^T : uint16     = (# "conv.u2" (# "shr.un" x (mask n 15) : uint32 #) : uint16 #)
             when ^T : sbyte      = (# "conv.i1" (# "shr"    x (mask n 7 ) : int32  #) : sbyte #)
             when ^T : byte       = (# "conv.u1" (# "shr.un" x (mask n 7 ) : uint32 #) : byte #)

        let inline (&&&) (x: ^T) (y: ^T) : ^T = 
             (^T: (static member (&&&) : ^T * ^T -> ^T) (x,y))
             when ^T : int32      = (# "and" x y : int32 #)
             when ^T : int64      = (# "and" x y : int64 #)
             when ^T : uint64     = (# "and" x y : uint64 #)
             when ^T : uint32     = (# "and" x y : uint32 #)
             when ^T : int16      = (# "and" x y : int16 #)
             when ^T : uint16     = (# "and" x y : uint16 #)
             when ^T : nativeint  = (# "and" x y : nativeint #)
             when ^T : unativeint = (# "and" x y : unativeint #)
             when ^T : sbyte      = (# "and" x y : sbyte #)
             when ^T : byte       = (# "and" x y : byte #)

        let inline (|||) (x: ^T) (y: ^T) : ^T = 
             (^T: (static member (|||) : ^T * ^T -> ^T) (x,y))
             when ^T : int32      = (# "or" x y : int32 #)
             when ^T : int64      = (# "or" x y : int64 #)
             when ^T : uint64     = (# "or" x y : uint64 #)
             when ^T : uint32     = (# "or" x y : uint32 #)
             when ^T : int16      = (# "or" x y : int16 #)
             when ^T : uint16     = (# "or" x y : uint16 #)
             when ^T : nativeint  = (# "or" x y : nativeint #)
             when ^T : unativeint = (# "or" x y : unativeint #)
             when ^T : sbyte      = (# "or" x y : sbyte #)
             when ^T : byte       = (# "or" x y : byte #)

        let inline (^^^) (x: ^T) (y: ^T) : ^T = 
             (^T: (static member (^^^) : ^T * ^T -> ^T) (x,y))
             when ^T : int32      = (# "xor" x y : int32 #)
             when ^T : int64      = (# "xor" x y : int64 #)
             when ^T : uint64     = (# "xor" x y : uint64 #)
             when ^T : uint32     = (# "xor" x y : uint32 #)
             when ^T : int16      = (# "xor" x y : int16 #)
             when ^T : uint16     = (# "xor" x y : uint16 #)
             when ^T : nativeint  = (# "xor" x y : nativeint #)
             when ^T : unativeint = (# "xor" x y : unativeint #)
             when ^T : sbyte      = (# "xor" x y : sbyte #)
             when ^T : byte       = (# "xor" x y : byte #)

        let inline (~~~) (x: ^T) : ^T = 
             (^T: (static member (~~~) : ^T -> ^T) (x))
             when ^T : int32      = (# "not" x : int32 #)
             when ^T : int64      = (# "not" x : int64 #)
             when ^T : uint64     = (# "not" x : uint64 #)
             when ^T : uint32     = (# "not" x : uint32 #)
             when ^T : nativeint  = (# "not" x : nativeint #)
             when ^T : unativeint = (# "not" x : unativeint #)
             when ^T : int16      = (# "conv.i2" (# "not" x : int32  #) : int16 #)
             when ^T : uint16     = (# "conv.u2" (# "not" x : uint32 #) : uint16 #)
             when ^T : sbyte      = (# "conv.i1" (# "not" x : int32  #) : sbyte #)
             when ^T : byte       = (# "conv.u1" (# "not" x : uint32 #) : byte #)

        let inline castToString (x:'T) = (# "" x : string #)  // internal

        let nonempty x = match x with [] -> false | _ -> true

        // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
        // tail cons cells is permitted in carefully written library code.
        let setFreshConsTail cons t = 
            cons.(::).1 <- t

        // Return the last cons it the chain
        let rec appendToFreshConsTail cons xs = 
            match xs with 
            | [] -> cons
            | h::t -> 
                let cons2 = [h]
                setFreshConsTail cons cons2
                appendToFreshConsTail cons2 t

        // let rec (@) x y = match x with [] -> y | (h::t) -> h :: (t @ y)
        let (@) l1 l2 = 
            match l1 with
            | [] -> l2
            | (h::t) -> 
            match l2 with
            | [] -> l1
            | _ -> 
              let res = [h] 
              let cons = appendToFreshConsTail res t 
              setFreshConsTail cons l2;
              res
        

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let incr x = x.contents <- x.contents + 1

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let decr x = x.contents <- x.contents - 1
#if FX_NO_EXIT
#else
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let exit (n:int) = System.Environment.Exit(n); failwith "System.Environment.Exit did not exit!"
#endif
        let inline byte_of_string (s:string)       = (# "conv.ovf.u1" (ParseUInt32 s) : byte #)
        let inline sbyte_of_string (s:string)      = (# "conv.ovf.i1" (ParseInt32 s)  : sbyte #)
        let inline int16_of_string (s:string)      = (# "conv.ovf.i2" (ParseInt32 s)  : int16 #)
        let inline uint16_of_string (s:string)     = (# "conv.ovf.u2" (ParseUInt32 s) : uint16 #)
        let inline nativeint_of_string (s:string)  = (# "conv.ovf.i"  (ParseInt64 s)  : nativeint #)
        let inline unativeint_of_string (s:string) = (# "conv.ovf.u"  (ParseInt64 s)  : unativeint #)

        let inline float_of_string (s:string)   = Double.Parse(s,NumberStyles.Float, CultureInfo.InvariantCulture)

        let inline float32_of_string (s:string) = Single.Parse(s,NumberStyles.Float, CultureInfo.InvariantCulture)
            

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline byte (x: ^T) = 
            (^T : (static member ToByte: ^T -> byte) (x))
             when ^T : string     = byte_of_string (castToString x)
             when ^T : float      = (# "conv.u1" x  : byte #)
             when ^T : float32    = (# "conv.u1" x  : byte #)
             when ^T : int64      = (# "conv.u1" x  : byte #)
             when ^T : int32      = (# "conv.u1" x  : byte #)
             when ^T : int16      = (# "conv.u1" x  : byte #)
             when ^T : nativeint  = (# "conv.u1" x  : byte #)
             when ^T : sbyte      = (# "conv.u1" x  : byte #)
             when ^T : uint64     = (# "conv.u1" x  : byte #)
             when ^T : uint32     = (# "conv.u1" x  : byte #)
             when ^T : uint16     = (# "conv.u1" x  : byte #)
             when ^T : char       = (# "conv.u1" x  : byte #)
             when ^T : unativeint = (# "conv.u1" x  : byte #)
             when ^T : byte       = (# "conv.u1" x  : byte #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline sbyte (x: ^T) = 
            (^T : (static member ToSByte: ^T -> sbyte) (x))
             when ^T : string     = sbyte_of_string (castToString x)
             when ^T : float     = (# "conv.i1" x  : sbyte #)
             when ^T : float32   = (# "conv.i1" x  : sbyte #)
             when ^T : int64     = (# "conv.i1" x  : sbyte #)
             when ^T : int32     = (# "conv.i1" x  : sbyte #)
             when ^T : int16     = (# "conv.i1" x  : sbyte #)
             when ^T : nativeint = (# "conv.i1" x  : sbyte #)
             when ^T : sbyte     = (# "conv.i1" x  : sbyte #)
             when ^T : uint64     = (# "conv.i1" x  : sbyte #)
             when ^T : uint32     = (# "conv.i1" x  : sbyte #)
             when ^T : uint16     = (# "conv.i1" x  : sbyte #)
             when ^T : char       = (# "conv.i1" x  : sbyte #)
             when ^T : unativeint = (# "conv.i1" x  : sbyte #)
             when ^T : byte     = (# "conv.i1" x  : sbyte #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline uint16 (x: ^T) = 
            (^T : (static member ToUInt16: ^T -> uint16) (x))
             when ^T : string     = uint16_of_string (castToString x)
             when ^T : float     = (# "conv.u2" x  : uint16 #)
             when ^T : float32   = (# "conv.u2" x  : uint16 #)
             when ^T : int64     = (# "conv.u2" x  : uint16 #)
             when ^T : int32     = (# "conv.u2" x  : uint16 #)
             when ^T : int16     = (# "conv.u2" x  : uint16 #)
             when ^T : nativeint = (# "conv.u2" x  : uint16 #)
             when ^T : sbyte     = (# "conv.u2" x  : uint16 #)
             when ^T : uint64     = (# "conv.u2" x  : uint16 #)
             when ^T : uint32     = (# "conv.u2" x  : uint16 #)
             when ^T : uint16     = (# "conv.u2" x  : uint16 #)
             when ^T : char       = (# "conv.u2" x  : uint16 #)
             when ^T : unativeint = (# "conv.u2" x  : uint16 #)
             when ^T : byte     = (# "conv.u2" x  : uint16 #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline int16 (x: ^T) = 
            (^T : (static member ToInt16: ^T -> int16) (x))
             when ^T : string     = int16_of_string (castToString x)
             when ^T : float     = (# "conv.i2" x  : int16 #)
             when ^T : float32   = (# "conv.i2" x  : int16 #)
             when ^T : int64     = (# "conv.i2" x  : int16 #)
             when ^T : int32     = (# "conv.i2" x  : int16 #)
             when ^T : int16     = (# "conv.i2" x  : int16 #)
             when ^T : nativeint = (# "conv.i2" x  : int16 #)
             when ^T : sbyte     = (# "conv.i2" x  : int16 #)
             when ^T : uint64     = (# "conv.i2" x  : int16 #)
             when ^T : uint32     = (# "conv.i2" x  : int16 #)
             when ^T : uint16     = (# "conv.i2" x  : int16 #)
             when ^T : char       = (# "conv.i2" x  : int16 #)
             when ^T : unativeint = (# "conv.i2" x  : int16 #)
             when ^T : byte     = (# "conv.i2" x  : int16 #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline uint32 (x: ^T) = 
            (^T : (static member ToUInt32: ^T -> uint32) (x))
             when ^T : string     = ParseUInt32 (castToString x)
             when ^T : float     = (# "conv.u4" x  : uint32 #)
             when ^T : float32   = (# "conv.u4" x  : uint32 #)

             when ^T : int64     = (# "conv.u4" x  : uint32 #)
             when ^T : nativeint = (# "conv.u4" x  : uint32 #)
             
             // For integers shorter that 32 bits, we must first 
             // sign-widen the signed integer to 32 bits, and then 
             // "convert" from signed int32 to unsigned int32
             // This is a no-op on IL stack (ECMA 335 Part III 1.5 Tables 8 & 9)
             when ^T : int32     = (# "" x : uint32 #)
             when ^T : int16     = (# "" x : uint32 #)
             when ^T : sbyte     = (# "" x : uint32 #)             
             
             
             when ^T : uint64     = (# "conv.u4" x  : uint32 #)
             when ^T : uint32     = (# "conv.u4" x  : uint32 #)
             when ^T : uint16     = (# "conv.u4" x  : uint32 #)
             when ^T : char       = (# "conv.u4" x  : uint32 #)
             when ^T : unativeint = (# "conv.u4" x  : uint32 #)
             when ^T : byte     = (# "conv.u4" x  : uint32 #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline int32 (x: ^T) = 
            (^T : (static member ToInt32: ^T -> int32) (x))
             when ^T : string     = ParseInt32 (castToString x)
             when ^T : float     = (# "conv.i4" x  : int32 #)
             when ^T : float32   = (# "conv.i4" x  : int32 #)
             when ^T : int64     = (# "conv.i4" x  : int32 #)
             when ^T : nativeint = (# "conv.i4" x  : int32 #)
             
             // For integers shorter that 32 bits, we sign-widen the signed integer to 32 bits
             // This is a no-op on IL stack (ECMA 335 Part III 1.5 Tables 8 & 9)
             when ^T : int32     = (# "" x  : int32 #)
             when ^T : int16     = (# "" x  : int32 #)
             when ^T : sbyte     = (# "" x  : int32 #)
             
             when ^T : uint64     = (# "conv.i4" x  : int32 #)             
             when ^T : uint32     = (# "" x  : int32 #) // Signed<->Unsigned conversion is a no-op on IL stack
             when ^T : uint16     = (# "conv.i4" x  : int32 #)
             when ^T : char       = (# "conv.i4" x  : int32 #)
             when ^T : unativeint = (# "conv.i4" x  : int32 #)
             when ^T : byte     = (# "conv.i4" x  : int32 #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId="int")>]
        let inline int    x = int32  x         


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline enum< ^T when ^T : enum<int32> > (x:int32) : ^T = EnumOfValue x

        let ( |KeyValue| ) (kvp : KeyValuePair<'T,'U>) = (kvp.Key, kvp.Value)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let infinity     = System.Double.PositiveInfinity

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let nan          = System.Double.NaN 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let infinityf     = System.Single.PositiveInfinity

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let nanf          = System.Single.NaN 

        //let max_float    = System.Double.MaxValue 
        //let min_float    =  0x0010000000000000LF
        //let epsilon_float = 0x3CB0000000000000LF // Int64.float_of_bits 4372995238176751616L


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline uint64 (x: ^T) = 
            (^T : (static member ToUInt64: ^T -> uint64) (x))
             when ^T : string     = ParseUInt64 (castToString x)
             when ^T : float     = (# "conv.u8" x  : uint64 #)
             when ^T : float32   = (# "conv.u8" x  : uint64 #)
                          
             // we must first sign-widen the signed integer to 64 bits, and then 
             // "convert" from signed int64 to unsigned int64             
             // conv.i8 sign-widens the input, and on IL stack, 
             // conversion from signed to unsigned is a no-op (ECMA 335 Part III 1.5 Table 8)
             when ^T : int64     = (# "" x  : uint64 #)
             when ^T : int32     = (# "conv.i8" x  : uint64 #)
             when ^T : int16     = (# "conv.i8" x  : uint64 #)
             when ^T : nativeint = (# "conv.i8" x  : uint64 #)
             when ^T : sbyte     = (# "conv.i8" x  : uint64 #)
             
             
             when ^T : uint64     = (# "" x  : uint64 #)
             when ^T : uint32     = (# "conv.u8" x  : uint64 #)
             when ^T : uint16     = (# "conv.u8" x  : uint64 #)
             when ^T : char       = (# "conv.u8" x  : uint64 #)
             when ^T : unativeint = (# "conv.u8" x  : uint64 #)
             when ^T : byte     = (# "conv.u8" x  : uint64 #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline int64 (x: ^T) = 
            (^T : (static member ToInt64: ^T -> int64) (x))
             when ^T : string     = ParseInt64 (castToString x)
             when ^T : float     = (# "conv.i8" x  : int64 #)
             when ^T : float32   = (# "conv.i8" x  : int64 #)
             when ^T : int64     = (# "conv.i8" x  : int64 #)
             when ^T : int32     = (# "conv.i8" x  : int64 #)
             when ^T : int16     = (# "conv.i8" x  : int64 #)
             when ^T : nativeint = (# "conv.i8" x  : int64 #)
             when ^T : sbyte     = (# "conv.i8" x  : int64 #)
             
             // When converting unsigned integer, we should zero-widen them, NOT sign-widen 
             // No-op for uint64, conv.u8 for uint32, for smaller types conv.u8 and conv.i8 are identical.
             // For nativeint, conv.u8 works correctly both in 32 bit and 64 bit case.
             when ^T : uint64     = (# "" x  : int64 #)             
             when ^T : uint32     = (# "conv.u8" x  : int64 #)
             when ^T : uint16     = (# "conv.u8" x  : int64 #)
             when ^T : char       = (# "conv.u8" x  : int64 #)
             when ^T : unativeint = (# "conv.u8" x  : int64 #)
             when ^T : byte     = (# "conv.u8" x  : int64 #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId="float32")>]
        let inline float32 (x: ^T) = 
            (^T : (static member ToSingle : ^T -> float32) (x))
             when ^T : string     = float32_of_string (castToString x)
             when ^T : float     = (# "conv.r4" x  : float32 #)
             when ^T : float32   = (# "" x  : float32 #)
             when ^T : int64     = (# "conv.r4" x  : float32 #)
             when ^T : int32     = (# "conv.r4" x  : float32 #)
             when ^T : int16     = (# "conv.r4" x  : float32 #)
             when ^T : nativeint = (# "conv.r4" x  : float32 #)
             when ^T : sbyte     = (# "conv.r4" x  : float32 #)
             when ^T : uint64     = (# "conv.r.un conv.r4" x  : float32 #)
             when ^T : uint32     = (# "conv.r.un conv.r4" x  : float32 #)
             when ^T : uint16     = (# "conv.r.un conv.r4" x  : float32 #)
             when ^T : char       = (# "conv.r.un conv.r4" x  : float32 #)
             when ^T : unativeint = (# "conv.r.un conv.r4" x  : float32 #)
             when ^T : byte     = (# "conv.r.un conv.r4" x  : float32 #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId="float")>]
        let inline float (x: ^T) = 
            (^T : (static member ToDouble : ^T -> float) (x))
             when ^T : string     = float_of_string (castToString x)
             when ^T : float     = (# "" x  : float #)
             when ^T : float32   = (# "conv.r8" x  : float #)
             when ^T : int64     = (# "conv.r8" x  : float #)
             when ^T : int32     = (# "conv.r8" x  : float #)
             when ^T : int16     = (# "conv.r8" x  : float #)
             when ^T : nativeint = (# "conv.r8" x  : float #)
             when ^T : sbyte     = (# "conv.r8" x  : float #)
             when ^T : uint64     = (# "conv.r.un conv.r8" x  : float #)
             when ^T : uint32     = (# "conv.r.un conv.r8" x  : float #)
             when ^T : uint16     = (# "conv.r.un conv.r8" x  : float #)
             when ^T : char       = (# "conv.r.un conv.r8" x  : float #)
             when ^T : unativeint = (# "conv.r.un conv.r8" x  : float #)
             when ^T : byte       = (# "conv.r.un conv.r8" x  : float #)
             when ^T : decimal    = (System.Convert.ToDouble((# "" x : decimal #))) 


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline decimal (x: ^T) = 
            (^T : (static member ToDecimal : ^T -> decimal) (x))
             when ^T : string     = (System.Decimal.Parse(castToString x,NumberStyles.Float,CultureInfo.InvariantCulture))
             when ^T : float      = (System.Convert.ToDecimal((# "" x : float #))) 
             when ^T : float32    = (System.Convert.ToDecimal((# "" x : float32 #))) 
             when ^T : int64      = (System.Convert.ToDecimal((# "" x : int64 #))) 
             when ^T : int32      = (System.Convert.ToDecimal((# "" x : int32 #))) 
             when ^T : int16      = (System.Convert.ToDecimal((# "" x : int16 #))) 
             when ^T : nativeint  = (System.Convert.ToDecimal(int64 (# "" x : nativeint #))) 
             when ^T : sbyte      = (System.Convert.ToDecimal((# "" x : sbyte #))) 
             when ^T : uint64     = (System.Convert.ToDecimal((# "" x : uint64 #))) 
             when ^T : uint32     = (System.Convert.ToDecimal((# "" x : uint32 #))) 
             when ^T : uint16     = (System.Convert.ToDecimal((# "" x : uint16 #))) 
             when ^T : unativeint = (System.Convert.ToDecimal(uint64 (# "" x : unativeint #))) 
             when ^T : byte       = (System.Convert.ToDecimal((# "" x : byte #))) 
             when ^T : decimal    = (# "" x : decimal #)

        // Recall type names.
        // Framework names:     sbyte, byte,  int16, uint16, int32, uint32, int64, uint64, single,  double.
        // C# names:            sbyte, byte,  short, ushort, int,   uint,   long,  ulong,  single,  double.
        // F# names:            sbyte, byte,  int16, uint16, int,   uint32, int64, uint64, float32, float.
        // Systematic names:    int8,  uint8, int16, uint16, int32, uint32, int64, uint64, float32, float64.

        // The names float/float32 are being migrated to double/single

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline single x = float32 x

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline double x = float x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline unativeint (x: ^T) = 
            (^T : (static member ToUIntPtr: ^T -> unativeint) (x))
             when ^T : string     = unativeint_of_string (castToString x)
             when ^T : float     = (# "conv.u" x  : unativeint #)
             when ^T : float32   = (# "conv.u" x  : unativeint #)
             
             // Narrower signed types we sign-extend.
             // Same length signed types we leave as such (so -1 gets reinterpreted as unsigned MaxValue).
             // Wider signed types we truncate.
             // conv.i does just that for both 32 and 64 bit case of nativeint, and conversion from nativeint is no-op.
             when ^T : int64     = (# "conv.i" x  : unativeint #)
             when ^T : int32     = (# "conv.i" x  : unativeint #)
             when ^T : int16     = (# "conv.i" x  : unativeint #)
             when ^T : nativeint = (# "" x  : unativeint #)
             when ^T : sbyte     = (# "conv.i" x  : unativeint #)
             
             when ^T : uint64     = (# "conv.u" x  : unativeint #)
             when ^T : uint32     = (# "conv.u" x  : unativeint #)
             when ^T : uint16     = (# "conv.u" x  : unativeint #)
             when ^T : char       = (# "conv.u" x  : unativeint #)
             when ^T : unativeint = (# "" x  : unativeint #)
             when ^T : byte       = (# "conv.u" x  : unativeint #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline nativeint (x: ^T) = 
            (^T : (static member ToIntPtr: ^T -> nativeint) (x))
             when ^T : string     = nativeint_of_string (castToString x)
             when ^T : float      = (# "conv.i" x  : nativeint #)
             when ^T : float32    = (# "conv.i" x  : nativeint #)
                         
             when ^T : int64      = (# "conv.i" x  : nativeint #)
             when ^T : int32      = (# "conv.i" x  : nativeint #)
             when ^T : int16      = (# "conv.i" x  : nativeint #)
             when ^T : nativeint  = (# "conv.i" x  : nativeint #)
             when ^T : sbyte      = (# "conv.i" x  : nativeint #)

             // Narrower unsigned types we zero-extend.
             // Same length unsigned types we leave as such (so unsigned MaxValue (all-bits-set) gets reinterpreted as -1).
             // Wider unsigned types we truncate.
             // conv.u does just that for both 32- and 64-bit-wide nativeint, and conversion from unativeint is no-op.
             when ^T : uint64     = (# "conv.u" x  : nativeint #)
             when ^T : uint32     = (# "conv.u" x  : nativeint #)
             when ^T : uint16     = (# "conv.u" x  : nativeint #)
             when ^T : char       = (# "conv.u" x  : nativeint #)
             when ^T : unativeint = (# "" x  : nativeint #)
             when ^T : byte       = (# "conv.i" x  : nativeint #)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline string (x: ^T) = 
             match box x with 
             | :? System.IFormattable as f -> f.ToString("g",CultureInfo.InvariantCulture)
             | obj ->  obj.ToString()
             when ^T : float      = (# "" x : float      #).ToString("g",CultureInfo.InvariantCulture)
             when ^T : float32    = (# "" x : float32    #).ToString("g",CultureInfo.InvariantCulture)
             when ^T : int64      = (# "" x : int64      #).ToString("g",CultureInfo.InvariantCulture)
             when ^T : int32      = (# "" x : int32      #).ToString("g",CultureInfo.InvariantCulture)
             when ^T : int16      = (# "" x : int16      #).ToString("g",CultureInfo.InvariantCulture)
             when ^T : nativeint  = (# "" x : nativeint  #).ToString()
             when ^T : sbyte      = (# "" x : sbyte      #).ToString("g",CultureInfo.InvariantCulture)
             when ^T : uint64     = (# "" x : uint64     #).ToString("g",CultureInfo.InvariantCulture)
             when ^T : uint32     = (# "" x : uint32     #).ToString("g",CultureInfo.InvariantCulture)
             when ^T : int16      = (# "" x : int16      #).ToString("g",CultureInfo.InvariantCulture)
             when ^T : unativeint = (# "" x : unativeint #).ToString()
             when ^T : byte       = (# "" x : byte       #).ToString("g",CultureInfo.InvariantCulture)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline char (x: ^T) = 
            (^T : (static member ToChar: ^T -> char) (x))
#if FX_NO_CHAR_PARSE
             when ^T : string     = (castToString x).[0]
#else
             when ^T : string     = (System.Char.Parse(castToString x))
#endif
             when ^T : float      = (# "conv.u2" x  : char #)
             when ^T : float32    = (# "conv.u2" x  : char #)
             when ^T : int64      = (# "conv.u2" x  : char #)
             when ^T : int32      = (# "conv.u2" x  : char #)
             when ^T : int16      = (# "conv.u2" x  : char #)
             when ^T : nativeint  = (# "conv.u2" x  : char #)
             when ^T : sbyte      = (# "conv.u2" x  : char #)
             when ^T : uint64     = (# "conv.u2" x  : char #)
             when ^T : uint32     = (# "conv.u2" x  : char #)
             when ^T : uint16     = (# "conv.u2" x  : char #)
             when ^T : char       = (# "conv.u2" x  : char #)
             when ^T : unativeint = (# "conv.u2" x  : char #)
             when ^T : byte       = (# "conv.u2" x  : char #)

        
        module Attributes = 
            open System.Runtime.CompilerServices

#if FX_NO_DEFAULT_DEPENDENCY_TYPE
#else
            [<Dependency("FSharp.Core",LoadHint.Always)>] 
            [<assembly: System.Runtime.CompilerServices.DefaultDependency(System.Runtime.CompilerServices.LoadHint.Always)>] 
#endif
            [<assembly: System.Runtime.InteropServices.ComVisible(false)>]
            [<assembly: System.CLSCompliant(true)>]

#if BE_SECURITY_TRANSPARENT
            [<assembly: System.Security.SecurityTransparent>] // assembly is fully transparent
            [<assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level2)>] // v4 transparency; soon to be the default, but not yet
#else
#if FX_NO_SECURITY_PERMISSIONS
#else
#if FX_SIMPLE_SECURITY_PERMISSIONS
            [<assembly: System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum)>]
#else
            [<assembly: System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.RequestOptional,Name="Nothing") >]
            [<assembly: System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.RequestMinimum, Execution=true)>]
#endif
#endif
#endif
            do ()


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline lock (lockobj : 'T when 'T : not struct) f  = 
            System.Threading.Monitor.Enter(lockobj);
            try f()
            finally
                System.Threading.Monitor.Exit(lockobj)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let using (ie : 'T when 'T :> System.IDisposable) f = 
            try f(ie)
            finally match (box ie) with null -> () | _ -> ie.Dispose()


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline typeof<'T> = BasicInlinedOperations.typeof<'T>


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline typedefof<'T> = BasicInlinedOperations.typedefof<'T>


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline sizeof<'T> = BasicInlinedOperations.sizeof<'T>


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline hash  (x: 'T) = LanguagePrimitives.GenericHash x

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline limitedHash (limit:int)  (x: 'T) = LanguagePrimitives.GenericLimitedHash limit x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let id x = x

        // std* are TypeFunctions with the effect of reading the property on instantiation.
        // So, direct uses of stdout should capture the current System.Console.Out at that point.
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let stdin<'T>  = System.Console.In

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let stdout<'T> = System.Console.Out

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let stderr<'T> = System.Console.Error

            
        let inline defaultof<'T> = unsafeDefault<'T>


        module Unchecked = 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline defaultof<'T> = unsafeDefault<'T>

        module Checked = 
        
            let inline (+) (x: ^T) (y: ^U) : ^V = 
                 CheckedAdditionDynamic<(^T),(^U),(^V)>  x y 
                 when ^T : int32       and ^U : int32      = (# "add.ovf" x y : int32 #)
                 when ^T : float       and ^U : float      = (# "add" x y : float #)
                 when ^T : float32     and ^U : float32    = (# "add" x y : float32 #)
                 when ^T : int64       and ^U : int64      = (# "add.ovf" x y : int64 #)
                 when ^T : uint64      and ^U : uint64     = (# "add.ovf.un" x y : uint64 #)
                 when ^T : uint32      and ^U : uint32     = (# "add.ovf.un" x y : uint32 #)
                 when ^T : nativeint   and ^U : nativeint  = (# "add.ovf" x y : nativeint #)
                 when ^T : unativeint  and ^U : unativeint = (# "add.ovf.un" x y : unativeint #)
                 when ^T : int16       and ^U : int16      = (# "conv.ovf.i2" (# "add.ovf" x y : int32 #) : int16 #)
                 when ^T : uint16      and ^U : uint16     = (# "conv.ovf.u2.un" (# "add.ovf.un" x y : uint32 #) : uint16 #)
                 when ^T : char        and ^U : char       = (# "conv.ovf.u2.un" (# "add.ovf.un" x y : uint32 #) : char #)
                 when ^T : sbyte       and ^U : sbyte      = (# "conv.ovf.i1" (# "add.ovf" x y : int32 #) : sbyte #)
                 when ^T : byte        and ^U : byte       = (# "conv.ovf.u1.un" (# "add.ovf.un" x y : uint32 #) : byte #)
                 when ^T : string      and ^U : string     = (# "" (System.String.Concat((# "" x : string #),(# "" y : string #))) : ^T #)
                 when ^T : decimal     and ^U : decimal    = (# "" (System.Decimal.op_Addition((# "" x : decimal #),(# "" y : decimal #))) : ^V #)
                 // According to the somewhat subtle rules of static optimizations,
                 // this condition is used whenever ^T is resolved to a nominal type
                 // That is, not in the generic implementation of '+'
                 when ^T : ^T = ((^T or ^U): (static member (+) : ^T * ^U -> ^V) (x,y))
             
            let inline (-) (x: ^T) (y: ^U) : ^V = 
                 ((^T or ^U): (static member (-) : ^T * ^U -> ^V) (x,y))
                 when ^T : int32      and ^U : int32      = (# "sub.ovf" x y : int32 #)
                 when ^T : float      and ^U : float      = (# "sub" x y : float #)
                 when ^T : float32    and ^U : float32    = (# "sub" x y : float32 #)
                 when ^T : int64      and ^U : int64      = (# "sub.ovf" x y : int64 #)
                 when ^T : uint64     and ^U : uint64     = (# "sub.ovf.un" x y : uint64 #)
                 when ^T : uint32     and ^U : uint32     = (# "sub.ovf.un" x y : uint32 #)
                 when ^T : nativeint  and ^U : nativeint  = (# "sub.ovf" x y : nativeint #)
                 when ^T : unativeint and ^U : unativeint = (# "sub.ovf.un" x y : unativeint #)
                 when ^T : int16       and ^U : int16      = (# "conv.ovf.i2" (# "sub.ovf" x y : int32 #) : int16 #)
                 when ^T : uint16      and ^U : uint16     = (# "conv.ovf.u2.un" (# "sub.ovf.un" x y : uint32 #) : uint16 #)
                 when ^T : sbyte       and ^U : sbyte      = (# "conv.ovf.i1" (# "sub.ovf" x y : int32 #) : sbyte #)
                 when ^T : byte        and ^U : byte       = (# "conv.ovf.u1.un" (# "sub.ovf.un" x y : uint32 #) : byte #)
                 when ^T : decimal     and ^U : decimal    = (# "" (System.Decimal.op_Subtraction((# "" x : decimal #),(# "" y : decimal #))) : ^V #)

            let inline (~-) (x: ^T) : ^T = 
                (^T : (static member (~-) : ^T -> ^T) (x))
                 when ^T : int32     = (# "sub.ovf" 0 x  : int32 #)
                 when ^T : float     = (# "neg" x  : float #)
                 when ^T : float32   = (# "neg" x  : float32 #)
                 when ^T : int64     = (# "sub.ovf" 0L x  : int64 #)
                 when ^T : int16     = (# "sub.ovf" 0s x  : int16 #)
                 when ^T : nativeint = (# "sub.ovf" 0n x  : nativeint #)
                 when ^T : sbyte     = (# "sub.ovf" 0y x  : sbyte #)
                 when ^T : decimal   = (# "" (System.Decimal.op_UnaryNegation((# "" x : decimal #))) : ^T #)

            let inline ( * ) (x: ^T) (y: ^U) : ^V = 
                 CheckedMultiplyDynamic<(^T),(^U),(^V)>  x y 
                 when ^T : sbyte       and ^U : sbyte      = (# "conv.ovf.i1" (# "mul.ovf" x y : int32 #) : sbyte #)
                 when ^T : int16       and ^U : int16      = (# "conv.ovf.i2" (# "mul.ovf" x y : int32 #) : int16 #)
                 when ^T : int64      and ^U : int64      = (# "mul.ovf" x y : int64 #)
                 when ^T : int32      and ^U : int32      = (# "mul.ovf" x y : int32 #)
                 when ^T : nativeint  and ^U : nativeint  = (# "mul.ovf" x y : nativeint #)
                 when ^T : byte        and ^U : byte       = (# "conv.ovf.u1.un" (# "mul.ovf.un" x y : uint32 #) : byte #)
                 when ^T : uint16      and ^U : uint16     = (# "conv.ovf.u2.un" (# "mul.ovf.un" x y : uint32 #) : uint16 #)
                 when ^T : uint32     and ^U : uint32     = (# "mul.ovf.un" x y : uint32 #)
                 when ^T : uint64     and ^U : uint64     = (# "mul.ovf.un" x y : uint64 #)
                 when ^T : unativeint and ^U : unativeint = (# "mul.ovf.un" x y : unativeint #)
                 when ^T : float      and ^U : float      = (# "mul" x y : float #)
                 when ^T : float32    and ^U : float32    = (# "mul" x y : float32 #)
                 when ^T : decimal     and ^U : decimal    = (# "" (System.Decimal.op_Multiply((# "" x : decimal #),(# "" y : decimal #))) : ^V #)
                 // According to the somewhat subtle rules of static optimizations,
                 // this condition is used whenever ^T is resolved to a nominal type
                 // That is, not in the generic implementation of '*'
                 when ^T : ^T = ((^T or ^U): (static member (*) : ^T * ^U -> ^V) (x,y))


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline byte (x: ^T) = 
                (^T : (static member ToByte: ^T -> byte) (x))
                 when ^T : string     = byte_of_string (castToString x)
                 when ^T : float     = (# "conv.ovf.u1" x  : byte #)
                 when ^T : float32   = (# "conv.ovf.u1" x  : byte #)
                 when ^T : int64     = (# "conv.ovf.u1" x  : byte #)
                 when ^T : int32     = (# "conv.ovf.u1" x  : byte #)
                 when ^T : int16     = (# "conv.ovf.u1" x  : byte #)
                 when ^T : nativeint = (# "conv.ovf.u1" x  : byte #)
                 when ^T : sbyte     = (# "conv.ovf.u1" x  : byte #)
                 when ^T : uint64     = (# "conv.ovf.u1.un" x  : byte #)
                 when ^T : uint32     = (# "conv.ovf.u1.un" x  : byte #)
                 when ^T : uint16     = (# "conv.ovf.u1.un" x  : byte #)
                 when ^T : char       = (# "conv.ovf.u1.un" x  : byte #)
                 when ^T : unativeint = (# "conv.ovf.u1.un" x  : byte #)
                 when ^T : byte     = (# "conv.ovf.u1.un" x  : byte #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline sbyte (x: ^T) = 
                (^T : (static member ToSByte: ^T -> sbyte) (x))
                 when ^T : string     = sbyte_of_string (castToString x)
                 when ^T : float     = (# "conv.ovf.i1" x  : sbyte #)
                 when ^T : float32   = (# "conv.ovf.i1" x  : sbyte #)
                 when ^T : int64     = (# "conv.ovf.i1" x  : sbyte #)
                 when ^T : int32     = (# "conv.ovf.i1" x  : sbyte #)
                 when ^T : int16     = (# "conv.ovf.i1" x  : sbyte #)
                 when ^T : nativeint = (# "conv.ovf.i1" x  : sbyte #)
                 when ^T : sbyte     = (# "conv.ovf.i1" x  : sbyte #)
                 when ^T : uint64     = (# "conv.ovf.i1.un" x  : sbyte #)
                 when ^T : uint32     = (# "conv.ovf.i1.un" x  : sbyte #)
                 when ^T : uint16     = (# "conv.ovf.i1.un" x  : sbyte #)
                 when ^T : char       = (# "conv.ovf.i1.un" x  : sbyte #)
                 when ^T : unativeint = (# "conv.ovf.i1.un" x  : sbyte #)
                 when ^T : byte     = (# "conv.ovf.i1.un" x  : sbyte #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline uint16 (x: ^T) = 
                (^T : (static member ToUInt16: ^T -> uint16) (x))
                 when ^T : string     = uint16_of_string (castToString x)
                 when ^T : float      = (# "conv.ovf.u2" x  : uint16 #)
                 when ^T : float32    = (# "conv.ovf.u2" x  : uint16 #)
                 when ^T : int64      = (# "conv.ovf.u2" x  : uint16 #)
                 when ^T : int32      = (# "conv.ovf.u2" x  : uint16 #)
                 when ^T : int16      = (# "conv.ovf.u2" x  : uint16 #)
                 when ^T : nativeint  = (# "conv.ovf.u2" x  : uint16 #)
                 when ^T : sbyte      = (# "conv.ovf.u2" x  : uint16 #)
                 when ^T : uint64     = (# "conv.ovf.u2.un" x  : uint16 #)
                 when ^T : uint32     = (# "conv.ovf.u2.un" x  : uint16 #)
                 when ^T : uint16     = (# "conv.ovf.u2.un" x  : uint16 #)
                 when ^T : char       = (# "conv.ovf.u2.un" x  : uint16 #)
                 when ^T : unativeint = (# "conv.ovf.u2.un" x  : uint16 #)
                 when ^T : byte       = (# "conv.ovf.u2.un" x  : uint16 #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline char (x: ^T) = 
                (^T : (static member ToChar: ^T -> char) (x))
#if FX_NO_CHAR_PARSE
                 when ^T : string     = (castToString x).[0]
#else
                 when ^T : string     = (System.Char.Parse(castToString x))
#endif
                 when ^T : float      = (# "conv.ovf.u2" x  : char #)
                 when ^T : float32    = (# "conv.ovf.u2" x  : char #)
                 when ^T : int64      = (# "conv.ovf.u2" x  : char #)
                 when ^T : int32      = (# "conv.ovf.u2" x  : char #)
                 when ^T : int16      = (# "conv.ovf.u2" x  : char #)
                 when ^T : nativeint  = (# "conv.ovf.u2" x  : char #)
                 when ^T : sbyte      = (# "conv.ovf.u2" x  : char #)
                 when ^T : uint64     = (# "conv.ovf.u2.un" x  : char #)
                 when ^T : uint32     = (# "conv.ovf.u2.un" x  : char #)
                 when ^T : uint16     = (# "conv.ovf.u2.un" x  : char #)
                 when ^T : char       = (# "conv.ovf.u2.un" x  : char #)
                 when ^T : unativeint = (# "conv.ovf.u2.un" x  : char #)
                 when ^T : byte       = (# "conv.ovf.u2.un" x  : char #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline int16 (x: ^T) = 
                (^T : (static member ToInt16: ^T -> int16) (x))
                 when ^T : string     = int16_of_string (castToString x)
                 when ^T : float     = (# "conv.ovf.i2" x  : int16 #)
                 when ^T : float32   = (# "conv.ovf.i2" x  : int16 #)
                 when ^T : int64     = (# "conv.ovf.i2" x  : int16 #)
                 when ^T : int32     = (# "conv.ovf.i2" x  : int16 #)
                 when ^T : int16     = (# "conv.ovf.i2" x  : int16 #)
                 when ^T : nativeint = (# "conv.ovf.i2" x  : int16 #)
                 when ^T : sbyte     = (# "conv.ovf.i2" x  : int16 #)
                 when ^T : uint64     = (# "conv.ovf.i2.un" x  : int16 #)
                 when ^T : uint32     = (# "conv.ovf.i2.un" x  : int16 #)
                 when ^T : uint16     = (# "conv.ovf.i2.un" x  : int16 #)
                 when ^T : char       = (# "conv.ovf.i2.un" x  : int16 #)
                 when ^T : unativeint = (# "conv.ovf.i2.un" x  : int16 #)
                 when ^T : byte     = (# "conv.ovf.i2.un" x  : int16 #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline uint32 (x: ^T) = 
                (^T : (static member ToUInt32: ^T -> uint32) (x))
                 when ^T : string     = ParseUInt32 (castToString x)
                 when ^T : float     = (# "conv.ovf.u4" x  : uint32 #)
                 when ^T : float32   = (# "conv.ovf.u4" x  : uint32 #)
                 when ^T : int64     = (# "conv.ovf.u4" x  : uint32 #)
                 when ^T : int32     = (# "conv.ovf.u4" x  : uint32 #)
                 when ^T : int16     = (# "conv.ovf.u4" x  : uint32 #)
                 when ^T : nativeint = (# "conv.ovf.u4" x  : uint32 #)
                 when ^T : sbyte     = (# "conv.ovf.u4" x  : uint32 #)
                 when ^T : uint64     = (# "conv.ovf.u4.un" x  : uint32 #)
                 when ^T : uint32     = (# "conv.ovf.u4.un" x  : uint32 #)
                 when ^T : uint16     = (# "conv.ovf.u4.un" x  : uint32 #)
                 when ^T : char       = (# "conv.ovf.u4.un" x  : uint32 #)
                 when ^T : unativeint = (# "conv.ovf.u4.un" x  : uint32 #)
                 when ^T : byte     = (# "conv.ovf.u4.un" x  : uint32 #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline int32 (x: ^T) = 
                (^T : (static member ToInt32: ^T -> int32) (x))
                 when ^T : string     = ParseInt32 (castToString x)
                 when ^T : float     = (# "conv.ovf.i4" x  : int32 #)
                 when ^T : float32   = (# "conv.ovf.i4" x  : int32 #)
                 when ^T : int64     = (# "conv.ovf.i4" x  : int32 #)
                 when ^T : int32     = (# "conv.ovf.i4" x  : int32 #)
                 when ^T : int16     = (# "conv.ovf.i4" x  : int32 #)
                 when ^T : nativeint = (# "conv.ovf.i4" x  : int32 #)
                 when ^T : sbyte     = (# "conv.ovf.i4" x  : int32 #)
                 when ^T : uint64     = (# "conv.ovf.i4.un" x  : int32 #)
                 when ^T : uint32     = (# "conv.ovf.i4.un" x  : int32 #)
                 when ^T : uint16     = (# "conv.ovf.i4.un" x  : int32 #)
                 when ^T : char       = (# "conv.ovf.i4.un" x  : int32 #)
                 when ^T : unativeint = (# "conv.ovf.i4.un" x  : int32 #)
                 when ^T : byte     = (# "conv.ovf.i4.un" x  : int32 #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId="int")>]
            let inline int x = int32 x


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline uint64 (x: ^T) = 
                (^T : (static member ToUInt64: ^T -> uint64) (x))
                 when ^T : string     = ParseUInt64 (castToString x)
                 when ^T : float     = (# "conv.ovf.u8" x  : uint64 #)
                 when ^T : float32   = (# "conv.ovf.u8" x  : uint64 #)
                 when ^T : int64     = (# "conv.ovf.u8" x  : uint64 #)
                 when ^T : int32     = (# "conv.ovf.u8" x  : uint64 #)
                 when ^T : int16     = (# "conv.ovf.u8" x  : uint64 #)
                 when ^T : nativeint = (# "conv.ovf.u8" x  : uint64 #)
                 when ^T : sbyte     = (# "conv.ovf.u8" x  : uint64 #)
                 when ^T : uint64     = (# "conv.ovf.u8.un" x  : uint64 #)
                 when ^T : uint32     = (# "conv.ovf.u8.un" x  : uint64 #)
                 when ^T : uint16     = (# "conv.ovf.u8.un" x  : uint64 #)
                 when ^T : char       = (# "conv.ovf.u8.un" x  : uint64 #)
                 when ^T : unativeint = (# "conv.ovf.u8.un" x  : uint64 #)
                 when ^T : byte     = (# "conv.ovf.u8.un" x  : uint64 #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline int64 (x: ^T) = 
                (^T : (static member ToInt64: ^T -> int64) (x))
                 when ^T : string     = ParseInt64 (castToString x)
                 when ^T : float     = (# "conv.ovf.i8" x  : int64 #)
                 when ^T : float32   = (# "conv.ovf.i8" x  : int64 #)
                 when ^T : int64     = (# "conv.ovf.i8" x  : int64 #)
                 when ^T : int32     = (# "conv.ovf.i8" x  : int64 #)
                 when ^T : int16     = (# "conv.ovf.i8" x  : int64 #)
                 when ^T : nativeint = (# "conv.ovf.i8" x  : int64 #)
                 when ^T : sbyte     = (# "conv.ovf.i8" x  : int64 #)
                 when ^T : uint64     = (# "conv.ovf.i8.un" x  : int64 #)
                 when ^T : uint32     = (# "conv.ovf.i8.un" x  : int64 #)
                 when ^T : uint16     = (# "conv.ovf.i8.un" x  : int64 #)
                 when ^T : char       = (# "conv.ovf.i8.un" x  : int64 #)
                 when ^T : unativeint = (# "conv.ovf.i8.un" x  : int64 #)
                 when ^T : byte     = (# "conv.ovf.i8.un" x  : int64 #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline unativeint (x: ^T) = 
                (^T : (static member ToUIntPtr: ^T -> unativeint) (x))
                 when ^T : string     = unativeint_of_string (castToString x)
                 when ^T : float     = (# "conv.ovf.u" x  : unativeint #)
                 when ^T : float32   = (# "conv.ovf.u" x  : unativeint #)
                 when ^T : int64     = (# "conv.ovf.u" x  : unativeint #)
                 when ^T : int32     = (# "conv.ovf.u" x  : unativeint #)
                 when ^T : int16     = (# "conv.ovf.u" x  : unativeint #)
                 when ^T : nativeint = (# "conv.ovf.u" x  : unativeint #)
                 when ^T : sbyte     = (# "conv.ovf.u" x  : unativeint #)
                 when ^T : uint64     = (# "conv.ovf.u.un" x  : unativeint #)
                 when ^T : uint32     = (# "conv.ovf.u.un" x  : unativeint #)
                 when ^T : uint16     = (# "conv.ovf.u.un" x  : unativeint #)
                 when ^T : char       = (# "conv.ovf.u.un" x  : unativeint #)
                 when ^T : unativeint = (# "conv.ovf.u.un" x  : unativeint #)
                 when ^T : byte     = (# "conv.ovf.u.un" x  : unativeint #)


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let inline nativeint (x: ^T) = 
                (^T : (static member ToIntPtr: ^T -> nativeint) (x))
                 when ^T : string     = nativeint_of_string (castToString x)
                 when ^T : float     = (# "conv.ovf.i" x  : nativeint #)
                 when ^T : float32   = (# "conv.ovf.i" x  : nativeint #)
                 when ^T : int64     = (# "conv.ovf.i" x  : nativeint #)
                 when ^T : int32     = (# "conv.ovf.i" x  : nativeint #)
                 when ^T : int16     = (# "conv.ovf.i" x  : nativeint #)
                 when ^T : nativeint = (# "conv.ovf.i" x  : nativeint #)
                 when ^T : sbyte     = (# "conv.ovf.i" x  : nativeint #)
                 when ^T : uint64     = (# "conv.ovf.i.un" x  : nativeint #)
                 when ^T : uint32     = (# "conv.ovf.i.un" x  : nativeint #)
                 when ^T : uint16     = (# "conv.ovf.i.un" x  : nativeint #)
                 when ^T : char       = (# "conv.ovf.i.un" x  : nativeint #)
                 when ^T : unativeint = (# "conv.ovf.i.un" x  : nativeint #)
                 when ^T : byte     = (# "conv.ovf.i.un" x  : nativeint #)

        module OperatorIntrinsics =  begin

            
            open System.Collections
            
            let notStarted() = raise (new System.InvalidOperationException("Enumeration has not started. Call MoveNext."))
            let alreadyFinished() = raise (new System.InvalidOperationException("Enumeration already finished."))

            // Notes on "inline" with range ienumerable generation.
            // "inline" is used to ensure explicit lambda arguments can be reduced by the optimiser.
            // However, it is not used to ensure that primitive ops like add,sub etc. are direct calls.

            type mode = NotStarted | Running | Finished 

            let inline makeEnumerator (z:'z) (canStart:'z->bool) (canStep:'z->bool) (doReset:'z->unit) (current:'z->'T) =
                // Generate enumerator from mutable state "z".
                // Marked "inline" to ensure argument functions are reduced (by optimiser).
                let mode = ref NotStarted
                let getCurrent() = 
                    match !mode with
                    | NotStarted -> notStarted()
                    | Running    -> current z
                    | Finished   -> alreadyFinished()
                {   new IEnumerator<'T> with
                        member obj.Current = getCurrent()
                    interface System.Collections.IEnumerator with 
                        member obj.Current = box (getCurrent())
                        member obj.MoveNext() = 
                            match !mode with
                            | NotStarted -> if canStart z then (mode := Running; true) else (mode := Finished; false)
                            | Running    -> if canStep  z then true                    else (mode := Finished; false)
                            | Finished   -> false
                        member obj.Reset() = 
                            mode := NotStarted
                            doReset z                            
                    interface System.IDisposable with 
                        member obj.Dispose() = ()
                }

            let makeEmptyEnumerator() =
                makeEnumerator () (fun () -> false) (fun () -> false) (fun () -> ())
                                  (fun () -> failwith "this makeEmptyEnumerator getter should be dead code")

            let makeSingletonEnumerator v =
                let doReset  () = ()
                let current  () = v
                let canStart () = true
                let canStep  () = false
                makeEnumerator () canStart canStep doReset current

            let id x = x
            let makeProperIntegralRangeEnumerator (<<) add (x0,dx,x1) f = 
                // NOTE: The ordering << is an argument.
                // << will be < or > depending on direction.
                // Assumes x0 << x1 and zero << dx (i.e. it's a "proper" range).
                //--------
                // NOTE: "inline" so << becomes direct operation (should be IL comparision operation)
                let z = ref x0
                let doReset  z = z := x0
                let current  z = f !z
                let canStart z = true
                let canStep  z = let x  = !z
                                 let x' = add !z dx
                                 if    x' << x             then false           // x+dx has wrapped around
                                 elif  x' = x              then false           // x+dx has not moved (unexpected, dx<>0)
                                 elif  x << x' && x' << x1 then (z := x'; true) // x+dx has moved towards end point
                                 elif  x' = x1             then (z := x'; true) // x+dx has reached end point
                                 else (* x1 << x' then *)       false           // x+dx has passed end point
                makeEnumerator z canStart canStep doReset current

            let integralRangeStepEnumerator (zero,add) x0 dx x1 f =
                // Generates sequence z_i where z_i = f (x0 + i.dx) while x0 + i.dx is in region (x0,x1)
                // NOTE: could/should drop "inline", trade indirect add against code size.
                if x0 = x1 then
                    makeSingletonEnumerator (f x0)
                elif x0 < x1 then
                    if dx < zero then
                        makeEmptyEnumerator() // interval increasing, step decreasing 
                    else
                        makeProperIntegralRangeEnumerator (<) add (x0,dx,x1) f // generate proper increasing sequence
                else
                    if dx > zero then
                        makeEmptyEnumerator() // interval decreasing, step increasing
                    else                       
                        makeProperIntegralRangeEnumerator (>) add (x0,dx,x1) f // generate proper decreasing sequence

            let integralRangeEnumerator (dx,add,x0,x1) =
                // Generates sequence z_i where z_i = (x0 + i.dx) while x0 + i.dx is in region (x0,x1)
                if x0 = x1 then
                    makeSingletonEnumerator x0
                elif x0 < x1 then
                    makeProperIntegralRangeEnumerator (<) add (x0,dx,x1) id // generate proper increasing sequence
                else
                    makeEmptyEnumerator() // interval decreasing, step increasing

            let integralRange<'T> (one:'T, add:'T -> 'T -> 'T, n:'T,m:'T) =
                let gen() = integralRangeEnumerator (one,add,n,m)
                { new IEnumerable<'T> with 
                      member x.GetEnumerator() = gen() 
                  interface System.Collections.IEnumerable with 
                      member x.GetEnumerator() = (gen() :> System.Collections.IEnumerator) }

            let integralRangeStep<'T,'U> (zero:'U) (add:'T -> 'U -> 'T) (n:'T,step:'U,m:'T) =
                if step = zero then invalidArg "step" "the step of a range cannot be zero";
                let gen() = integralRangeStepEnumerator (zero,add) n step m id
                { new IEnumerable<'T> with 
                      member x.GetEnumerator() = gen() 
                  interface System.Collections.IEnumerable with 
                      member x.GetEnumerator() = (gen() :> System.Collections.IEnumerator) }

            let isNaN x = x <> x // NaN is the only value that does not equal itself.
            let makeProperFloatingRangeStepEnumerator (<<) add (x0,dx,x1) = 
                // NOTE: The ordering << is an argument.
                // << will be < or > depending on direction.
                // Assumes x0 << x1 and zero << dx.
                // NOTE: "inline" ensures << is comparision op
                let z = ref x0
                let doReset  z = z := x0
                let current  z = !z
                let canStart z = true
                let canStep  z = let x  = !z
                                 let x' = add !z dx
                                 // NOTE: The following code is similar to the integer case, but there are differences.
                                 // * With floats, there is a NaN case to consider.
                                 // * With floats, there should not be any wrapping around arithmetic.
                                 // * With floats, x + dx == x is possible when dx>0.                                 
                                 if   x' = x                then false              // no movement, loss of precision
                                 elif x << x' && x' << x1   then (z := x'; true)    // advanced towards end point
                                 elif x' = x1               then (z := x'; true)    // hit end point
                                 elif x1 << x'              then false              // passed beyond end point
                                                                                    // [includes x' infinite, x1 finite case]
                                 elif isNaN x'              then false              // x' has become NaN, which is possible...
                                                                                    // e.g. -infinity + infinity = NaN
                                 elif x' << x               then failwith           // x + dx should not move against <<
                                                                   "Broken invariant in F# floating point range"
                                 else                            false              // Are there any cases left? [i think not]
                makeEnumerator z canStart canStep doReset current

            let floatingRangeStepEnumerator (zero,add) x0 step x1 =
                if step = zero then invalidArg "step" "the step of a range cannot be zero";
                if isNaN x0  then invalidArg "x0" "start of a range cannot be NaN";
                if isNaN step  then invalidArg "step" "step of a range cannot be NaN";
                if isNaN x1  then invalidArg "x1" "end of a range cannot be NaN";
                if x0 = x1 then
                    makeSingletonEnumerator x0
                elif x0 < x1 then
                    if step < zero then
                        makeEmptyEnumerator() // interval increasing, step decreasing 
                    else
                        makeProperFloatingRangeStepEnumerator (<) add (x0,step,x1) // generate proper increasing sequence
                else
                    if step > zero then
                        makeEmptyEnumerator() // interval decreasing, step increasing
                    else                       
                        makeProperFloatingRangeStepEnumerator (>) add (x0,step,x1) // generate proper decreasing sequence

            // When is a System.Double an System.Int32?
            let minIntR = -2147483648.0
            let maxIntR =  2147483647.0
            let asInt x = if minIntR <= x && x <= maxIntR && System.Math.Floor x = x then Some (int x) else None

            type ops<'T> =
                 { zero    : 'T;
                   add     : 'T -> 'T -> 'T;
                   sub     : 'T -> 'T -> 'T;
                   mul     : 'T -> 'T -> 'T;
                   div     : 'T -> 'T -> 'T;
                   ofInt   : int -> 'T;
                   toFloat : 'T -> float }

            // When a floating range looks like an exact number of steps, generate using {x0+i.dx} for i from an integer range.
            let optimisedFloatingRangeEnumerator (ops:ops<'T>) x0 dx x1 =                                                 
                match asInt (ops.toFloat (ops.div (ops.sub x1 x0) dx)) with
                | Some n -> 
                    integralRangeStepEnumerator (0,(+))    0  1  n  (fun i -> ops.add x0 (ops.mul (ops.ofInt i) dx))
                | None ->
                    floatingRangeStepEnumerator (ops.zero,ops.add) x0 dx x1

            let floatingRange ops (n,step,m) =
                let gen() = optimisedFloatingRangeEnumerator ops n step m 
                { new IEnumerable<'T> with 
                      member x.GetEnumerator() = gen() 
                  interface System.Collections.IEnumerable with 
                      member x.GetEnumerator() = (gen() :> System.Collections.IEnumerator) }

            let floatOps   :ops<float> = {zero=0.0; add=(+); sub=(-); mul=( * );div=(/);ofInt=float;toFloat=float}
            let float32Ops :ops<float32>= {zero=0.0f; add=(+); sub=(-); mul=( * );div=(/);ofInt=float32;toFloat=float}
            
            let RangeInt32   (n,step,m) : seq<int>        = integralRangeStep 0    (+) (n,step,m)
            let RangeInt64   (n,step,m) : seq<int64>      = integralRangeStep 0L   (+) (n,step,m)
            let RangeUInt64  (n,step,m) : seq<uint64>     = integralRangeStep 0UL  (+) (n,step,m)
            let RangeUInt32  (n,step,m) : seq<uint32>     = integralRangeStep 0ul  (+) (n,step,m)
            let RangeIntPtr  (n,step,m) : seq<nativeint>  = integralRangeStep 0n   (+) (n,step,m)
            let RangeUIntPtr (n,step,m) : seq<unativeint> = integralRangeStep 0un  (+) (n,step,m)
            let RangeInt16   (n,step,m) : seq<int16>      = integralRangeStep 0s   (+) (n,step,m)
            let RangeUInt16  (n,step,m) : seq<uint16>     = integralRangeStep 0us  (+) (n,step,m)
            let RangeSByte   (n,step,m) : seq<sbyte>      = integralRangeStep 0y   (+) (n,step,m)
            let RangeByte    (n,step,m) : seq<byte>       = integralRangeStep 0uy  (+) (n,step,m)
            let RangeDouble  (n,step,m) : seq<float>      = floatingRange floatOps   (n,step,m)
            let RangeSingle  (n,step,m) : seq<float32>    = floatingRange float32Ops (n,step,m)
            let RangeGeneric   (one,add,n,m) : seq<'T> = integralRange (one,add,n,m)
            let RangeStepGeneric   (zero,add,n,step,m) : seq<'T> = integralRangeStep zero add  (n,step,m)
            let RangeChar (n:char, m:char) = 
                integralRange ((retype 1us : char),(fun (x:char) (y:char) -> retype ((retype x : uint16) + (retype y : uint16)) ),n,m)


            let inline to_float (x:float32) = (# "conv.r8" x : float #)
            let inline to_float32 (x:float) = (# "conv.r4" x : float32 #)

            let inline ComputePowerGenericInlined one mul x n =
                let rec loop n = 
                    match n with 
                    | 0 -> one
                    | 1 -> x
                    | 2 -> mul x x 
                    | 3 -> mul (mul x x) x
                    | 4 -> let v = mul x x in mul v v
                    | _ -> 
                        let v = loop (n/2) in 
                        let v = mul v v in
                        if n%2 = 0 then v else mul v x in
                loop n


            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowByte (x:byte) n = ComputePowerGenericInlined  1uy Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowSByte (x:sbyte) n = ComputePowerGenericInlined  1y Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowInt16 (x:int16) n = ComputePowerGenericInlined  1s Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowUInt16 (x:uint16) n = ComputePowerGenericInlined  1us Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowInt32 (x:int32) n = ComputePowerGenericInlined  1 Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowUInt32 (x:uint32) n = ComputePowerGenericInlined  1u Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowInt64 (x:int64) n = ComputePowerGenericInlined  1L Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowUInt64 (x:uint64) n = ComputePowerGenericInlined  1UL Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowIntPtr (x:nativeint) n = ComputePowerGenericInlined  1n Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowUIntPtr (x:unativeint) n = ComputePowerGenericInlined  1un Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowSingle (x:float32) n = ComputePowerGenericInlined  1.0f Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowDouble (x:float) n = ComputePowerGenericInlined  1.0 Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowDecimal (x:decimal) n = ComputePowerGenericInlined  1.0M Checked.( * ) x n 

            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let PowGeneric (one,mul,x:'T,n) = ComputePowerGenericInlined  one mul x n 

            

            let inline GetArraySlice (arr: _[]) (start,finish) = 
                   let start  = (match start with None -> 0 | Some n -> n) 
                   let finish = (match finish with None -> arr.Length - 1 | Some n -> n) 
                   GetArraySub arr start (finish - start + 1)

            let inline SetArraySlice (dst: _[]) (start,finish,src:_[]) = 
                   let start  = (match start with None -> 0 | Some n -> n) 
                   let finish = (match finish with None -> dst.Length - 1 | Some n -> n) 
                   SetArraySub dst start (finish - start + 1) src

            let GetArraySlice2D (arr: _[,]) (start1,finish1,start2,finish2) = 
                   let start1  = (match start1 with None -> 0 | Some n -> n) 
                   let start2  = (match start2 with None -> 0 | Some n -> n) 
                   let finish1 = (match finish1 with None -> GetArray2DLength1 arr - 1 | Some n -> n) 
                   let finish2 = (match finish2 with None -> GetArray2DLength2 arr - 1 | Some n -> n) 
                   let len1 = (finish1 - start1 + 1)
                   let len2 = (finish2 - start2 + 1)
                   GetArray2DSub arr start1 start2 len1 len2

            let SetArraySlice2D (dst: _[,]) (start1,finish1,start2,finish2,src:_[,]) = 
                   let start1  = (match start1 with None -> 0 | Some n -> n) 
                   let start2  = (match start2 with None -> 0 | Some n -> n) 
                   let finish1 = (match finish1 with None -> GetArray2DLength1 dst - 1 | Some n -> n) 
                   let finish2 = (match finish2 with None -> GetArray2DLength2 dst - 1 | Some n -> n) 
                   SetArray2DSub dst start1 start2 (finish1 - start1 + 1) (finish2 - start2 + 1) src

            let GetArraySlice3D (arr: _[,,]) (start1,finish1,start2,finish2,start3,finish3) = 
                   let start1  = (match start1 with None -> 0 | Some n -> n) 
                   let start2  = (match start2 with None -> 0 | Some n -> n) 
                   let start3  = (match start3 with None -> 0 | Some n -> n) 
                   let finish1 = (match finish1 with None -> GetArray3DLength1 arr - 1 | Some n -> n) 
                   let finish2 = (match finish2 with None -> GetArray3DLength2 arr - 1 | Some n -> n) 
                   let finish3 = (match finish3 with None -> GetArray3DLength3 arr - 1 | Some n -> n) 
                   let len1 = (finish1 - start1 + 1)
                   let len2 = (finish2 - start2 + 1)
                   let len3 = (finish3 - start3 + 1)
                   GetArray3DSub arr start1 start2 start3 len1 len2 len3

            let SetArraySlice3D (dst: _[,,]) (start1,finish1,start2,finish2,start3,finish3,src:_[,,]) = 
                   let start1  = (match start1 with None -> 0 | Some n -> n) 
                   let start2  = (match start2 with None -> 0 | Some n -> n) 
                   let start3  = (match start3 with None -> 0 | Some n -> n) 
                   let finish1 = (match finish1 with None -> GetArray3DLength1 dst - 1 | Some n -> n) 
                   let finish2 = (match finish2 with None -> GetArray3DLength2 dst - 1 | Some n -> n) 
                   let finish3 = (match finish3 with None -> GetArray3DLength3 dst - 1 | Some n -> n) 
                   SetArray3DSub dst start1 start2 start3 (finish1 - start1 + 1) (finish2 - start2 + 1) (finish3 - start3 + 1) src

            let GetArraySlice4D (arr: _[,,,]) (start1,finish1,start2,finish2,start3,finish3,start4,finish4) = 
                   let start1  = (match start1 with None -> 0 | Some n -> n) 
                   let start2  = (match start2 with None -> 0 | Some n -> n) 
                   let start3  = (match start3 with None -> 0 | Some n -> n) 
                   let start4  = (match start4 with None -> 0 | Some n -> n) 
                   let finish1 = (match finish1 with None -> Array4DLength1 arr - 1 | Some n -> n) 
                   let finish2 = (match finish2 with None -> Array4DLength2 arr - 1 | Some n -> n) 
                   let finish3 = (match finish3 with None -> Array4DLength3 arr - 1 | Some n -> n) 
                   let finish4 = (match finish4 with None -> Array4DLength4 arr - 1 | Some n -> n) 
                   let len1 = (finish1 - start1 + 1)
                   let len2 = (finish2 - start2 + 1)
                   let len3 = (finish3 - start3 + 1)
                   let len4 = (finish4 - start4 + 1)
                   GetArray4DSub arr start1 start2 start3 start4 len1 len2 len3 len4

            let SetArraySlice4D (dst: _[,,,]) (start1,finish1,start2,finish2,start3,finish3,start4,finish4,src:_[,,,]) = 
                   let start1  = (match start1 with None -> 0 | Some n -> n) 
                   let start2  = (match start2 with None -> 0 | Some n -> n) 
                   let start3  = (match start3 with None -> 0 | Some n -> n) 
                   let start4  = (match start4 with None -> 0 | Some n -> n) 
                   let finish1 = (match finish1 with None -> Array4DLength1 dst - 1 | Some n -> n) 
                   let finish2 = (match finish2 with None -> Array4DLength2 dst - 1 | Some n -> n) 
                   let finish3 = (match finish3 with None -> Array4DLength3 dst - 1 | Some n -> n) 
                   let finish4 = (match finish4 with None -> Array4DLength4 dst - 1 | Some n -> n) 
                   SetArray4DSub dst start1 start2 start3 start4 (finish1 - start1 + 1) (finish2 - start2 + 1) (finish3 - start3 + 1) (finish4 - start4 + 1) src

            let inline GetStringSlice (str:string) (start,finish) = 
                   let start  = (match start with None -> 0 | Some n -> n) 
                   let finish = (match finish with None -> str.Length - 1 | Some n -> n) 
                   str.Substring(start, finish-start+1)



            let inline abs_impl (x: ^T) : ^T = 
                 (^T: (static member Abs : ^T -> ^T) (x))
                 when ^T : int32       = let x : int32     = retype x in System.Math.Abs(x)
                 when ^T : float       = let x : float     = retype x in System.Math.Abs(x)
                 when ^T : float32     = let x : float32   = retype x in System.Math.Abs(x)
                 when ^T : int64       = let x : int64     = retype x in System.Math.Abs(x)
                 when ^T : nativeint   = 
                    let x : nativeint = retype x in 
                    if x >= 0n then x else 
                    let res = -x in 
                    if res < 0n then raise (System.OverflowException("Negating the minimum value of a twos complement number is invalid."))
                    res
                 when ^T : int16       = let x : int16     = retype x in System.Math.Abs(x)
                 when ^T : sbyte       = let x : sbyte     = retype x in System.Math.Abs(x)
                 when ^T : decimal     = System.Math.Abs(retype x : decimal) 

            let inline  acos_impl(x: ^T) : ^T = 
                 (^T: (static member Acos : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Acos(retype x)
                 when ^T : float32     = System.Math.Acos(to_float (retype x)) |> to_float32

            let inline  asin_impl(x: ^T) : ^T = 
                 (^T: (static member Asin : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Asin(retype x)
                 when ^T : float32     = System.Math.Asin(to_float (retype x)) |> to_float32

            let inline  atan_impl(x: ^T) : ^T = 
                 (^T: (static member Atan : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Atan(retype x)
                 when ^T : float32     = System.Math.Atan(to_float (retype x)) |> to_float32

            let inline  atan2_impl(x: ^T) (y: ^T) : 'U = 
                 (^T: (static member Atan2 : ^T * ^T -> 'U) (x,y))
                 when ^T : float       = System.Math.Atan2(retype x, retype y)
                 when ^T : float32     = System.Math.Atan2(to_float (retype x), to_float(retype y)) |> to_float32

            let inline  ceil_impl(x: ^T) : ^T = 
                 (^T: (static member Ceiling : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Ceiling(retype x : float)
                 when ^T : float32     = System.Math.Ceiling(to_float (retype x)) |> to_float32

            let inline  exp_impl(x: ^T) : ^T = 
                 (^T: (static member Exp : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Exp(retype x)
                 when ^T : float32     = System.Math.Exp(to_float (retype x)) |> to_float32

            let inline floor_impl (x: ^T) : ^T = 
                 (^T: (static member Floor : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Floor(retype x : float)
                 when ^T : float32     = System.Math.Floor(to_float (retype x)) |> to_float32

            let inline truncate_impl (x: ^T) : ^T = 
                 (^T: (static member Truncate : ^T -> ^T) (x))
#if FX_NO_TRUNCATE
                 when ^T : float       = 0.0
                 when ^T : float32     = 0.0
#else
                 when ^T : float       = System.Math.Truncate(retype x : float) 
                 when ^T : float32     = System.Math.Truncate(to_float (retype x))  |> to_float32
#endif

            let inline round_impl (x: ^T) : ^T = 
                 (^T: (static member Round : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Round(retype x : float)
                 when ^T : float32     = System.Math.Round(to_float (retype x)) |> to_float32

            let inline sign_impl (x: ^T) : int = 
                 (^T: (member Sign : int) (x))
                 when ^T : int32       = System.Math.Sign(retype x : int32)
                 when ^T : int64       = System.Math.Sign(retype x : int64)
                 when ^T : nativeint   = if (retype x : nativeint) < 0n then -1 else if (retype x : nativeint) > 0n then 1 else 0
                 when ^T : int16       = System.Math.Sign(retype x : int16)
                 when ^T : sbyte       = System.Math.Sign(retype x : sbyte)
                 when ^T : float       = System.Math.Sign(retype x : float)
                 when ^T : float32     = System.Math.Sign(to_float (retype x)) 
                 when ^T : decimal     = System.Math.Sign(retype x : decimal) 

            let inline  log_impl(x: ^T) : ^T = 
                 (^T: (static member Log : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Log(retype x)
                 when ^T : float32     = System.Math.Log(to_float (retype x)) |> to_float32

            let inline  log10_impl(x: ^T) : ^T = 
                 (^T: (static member Log10 : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Log10(retype x)
                 when ^T : float32     = System.Math.Log10(to_float (retype x)) |> to_float32

            let inline  sqrt_impl(x: ^T) : ^U = 
                 (^T: (static member Sqrt : ^T -> ^U) (x))
                 when ^T : float       = System.Math.Sqrt(retype x : float)
                 when ^T : float32     = System.Math.Sqrt(to_float (retype x)) |> to_float32

            let inline  cos_impl(x: ^T) : ^T = 
                 (^T: (static member Cos : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Cos(retype x)
                 when ^T : float32     = System.Math.Cos(to_float (retype x)) |> to_float32

            let inline  cosh_impl(x: ^T) : ^T = 
                 (^T: (static member Cosh : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Cosh(retype x)
                 when ^T : float32     = System.Math.Cosh(to_float (retype x)) |> to_float32

            let inline  sin_impl(x: ^T) : ^T = 
                 (^T: (static member Sin : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Sin(retype x)
                 when ^T : float32     = System.Math.Sin(to_float (retype x)) |> to_float32

            let inline  sinh_impl(x: ^T) : ^T = 
                 (^T: (static member Sinh : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Sinh(retype x)
                 when ^T : float32     = System.Math.Sinh(to_float (retype x)) |> to_float32

            let inline  tan_impl(x: ^T) : ^T = 
                 (^T: (static member Tan : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Tan(retype x)
                 when ^T : float32     = System.Math.Tan(to_float (retype x)) |> to_float32


            let inline  tanh_impl(x: ^T) : ^T = 
                 (^T: (static member Tanh : ^T -> ^T) (x))
                 when ^T : float       = System.Math.Tanh(retype x)
                 when ^T : float32     = System.Math.Tanh(to_float (retype x)) |> to_float32

            let inline  pow_impl (x: ^T) (y: ^T) : ^T = 
                 (^T: (static member Pow : ^T * ^T -> ^T) (x,y))
                 when ^T : float       = System.Math.Pow((retype x : float), (retype y: float))
                 when ^T : float32     = System.Math.Pow(to_float (retype x), to_float(retype y)) |> to_float32

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            let UnaryDynamicImpl nm : ('T -> 'U) =
                 let aty = typeof<'T>
                 let minfo = aty.GetMethod(nm, [| aty |])
                 (fun x -> unboxPrim<_>(minfo.Invoke(null,[| box x|])))

            let BinaryDynamicImpl nm : ('T -> 'T -> 'U) =
                 let aty = typeof<'T>
                 let minfo = aty.GetMethod(nm,[| aty;aty |])
                 (fun x  y-> unboxPrim<_>(minfo.Invoke(null,[| box x; box y|])))

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]                
            type AbsDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if   aty.Equals(typeof<sbyte>)      then unboxPrim<_>(fun (x:sbyte)     -> abs_impl x)
                    elif aty.Equals(typeof<int16>)      then unboxPrim<_>(fun (x:int16)     -> abs_impl x)
                    elif aty.Equals(typeof<int32>)      then unboxPrim<_>(fun (x:int32)     -> abs_impl x)
                    elif aty.Equals(typeof<int64>)      then unboxPrim<_>(fun (x:int64)     -> abs_impl x)
                    elif aty.Equals(typeof<nativeint>)  then unboxPrim<_>(fun (x:nativeint) -> abs_impl x)
                    elif aty.Equals(typeof<float>)      then unboxPrim<_>(fun (x:float)     -> abs_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> abs_impl x)
                    elif aty.Equals(typeof<decimal>)    then unboxPrim<_>(fun (x:decimal)   -> abs_impl x)
                    else UnaryDynamicImpl "Abs" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type AcosDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> acos_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> acos_impl x)
                    else UnaryDynamicImpl "Acos" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type AsinDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> asin_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> asin_impl x)
                    else UnaryDynamicImpl "Asin" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type AtanDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> atan_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> atan_impl x)
                    else UnaryDynamicImpl "Atan" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type Atan2DynamicImplTable<'T,'U>() = 
                static let result : ('T -> 'T -> 'U) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)  (y:float)   -> atan2_impl x y)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32) (y:float32)  -> atan2_impl x y)
                    else BinaryDynamicImpl "Atan2"
                static member Result : ('T -> 'T -> 'U) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type CeilingDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> ceil_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> ceil_impl x)
                    else UnaryDynamicImpl "Ceiling" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type ExpDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> exp_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> exp_impl x)
                    else UnaryDynamicImpl "Exp" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type FloorDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> floor_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> floor_impl x)
                    else UnaryDynamicImpl "Floor" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type TruncateDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> truncate_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> truncate_impl x)
                    else UnaryDynamicImpl "Truncate" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type RoundDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> round_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> round_impl x)
                    else UnaryDynamicImpl "Round" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type SignDynamicImplTable<'T>() = 
                static let result : ('T -> int) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> sign_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> sign_impl x)
                    elif aty.Equals(typeof<nativeint>)    then unboxPrim<_>(fun (x:nativeint)   -> sign_impl x)
                    elif aty.Equals(typeof<decimal>)    then unboxPrim<_>(fun (x:decimal)   -> sign_impl x)
                    elif aty.Equals(typeof<int16>)    then unboxPrim<_>(fun (x:int16)   -> sign_impl x)
                    elif aty.Equals(typeof<int32>)    then unboxPrim<_>(fun (x:int32)   -> sign_impl x)
                    elif aty.Equals(typeof<int64>)    then unboxPrim<_>(fun (x:int64)   -> sign_impl x)
                    elif aty.Equals(typeof<sbyte>)    then unboxPrim<_>(fun (x:sbyte)   -> sign_impl x)
                    else UnaryDynamicImpl "Sign" 
                static member Result : ('T -> int) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type LogDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> log_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> log_impl x)
                    else UnaryDynamicImpl "Log" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type Log10DynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> log10_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> log10_impl x)
                    else UnaryDynamicImpl "Log10" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type SqrtDynamicImplTable<'T,'U>() = 
                static let result : ('T -> 'U) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> sqrt_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> sqrt_impl x)
                    else UnaryDynamicImpl "Sqrt" 
                static member Result : ('T -> 'U) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type CosDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> cos_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> cos_impl x)
                    else UnaryDynamicImpl "Cos" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type CoshDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> cosh_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> cosh_impl x)
                    else UnaryDynamicImpl "Cosh" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type SinDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> sin_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> sin_impl x)
                    else UnaryDynamicImpl "Sin" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type SinhDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> sinh_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> sinh_impl x)
                    else UnaryDynamicImpl "Sinh" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type TanDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> tan_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> tan_impl x)
                    else UnaryDynamicImpl "Tan" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type TanhDynamicImplTable<'T>() = 
                static let result : ('T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)     -> tanh_impl x)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32)   -> tanh_impl x)
                    else UnaryDynamicImpl "Tanh" 
                static member Result : ('T -> 'T) = result

            [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]
            type PowDynamicImplTable<'T>() = 
                static let result : ('T -> 'T -> 'T) = 
                    let aty = typeof<'T>
                    if aty.Equals(typeof<float>)        then unboxPrim<_>(fun (x:float)   (y:float)  -> pow_impl x y)
                    elif aty.Equals(typeof<float32>)    then unboxPrim<_>(fun (x:float32) (y:float32)  -> pow_impl x y)
                    else BinaryDynamicImpl "Pow" 
                static member Result : ('T -> 'T -> 'T) = result

            let AbsDynamic x            = AbsDynamicImplTable<_>.Result x
            let AcosDynamic x           = AcosDynamicImplTable<_>.Result x
            let AsinDynamic x           = AsinDynamicImplTable<_>.Result x
            let AtanDynamic x           = AtanDynamicImplTable<_>.Result x
            let Atan2Dynamic x y        = Atan2DynamicImplTable<_,_>.Result x y
            let CeilingDynamic x        = CeilingDynamicImplTable<_>.Result x 
            let ExpDynamic x            = ExpDynamicImplTable<_>.Result x 
            let FloorDynamic x          = FloorDynamicImplTable<_>.Result x 
            let TruncateDynamic x       = TruncateDynamicImplTable<_>.Result x 
            let RoundDynamic x          = RoundDynamicImplTable<_>.Result x 
            let SignDynamic x           = SignDynamicImplTable<_>.Result x 
            let LogDynamic x            = LogDynamicImplTable<_>.Result x 
            let Log10Dynamic x          = Log10DynamicImplTable<_>.Result x 
            let SqrtDynamic x           = SqrtDynamicImplTable<_,_>.Result x 
            let CosDynamic x            = CosDynamicImplTable<_>.Result x 
            let CoshDynamic x           = CoshDynamicImplTable<_>.Result x 
            let SinDynamic x            = SinDynamicImplTable<_>.Result x 
            let SinhDynamic x           = SinhDynamicImplTable<_>.Result x 
            let TanDynamic x            = TanDynamicImplTable<_>.Result x 
            let TanhDynamic x           = TanhDynamicImplTable<_>.Result x 
            let PowDynamic x y          = PowDynamicImplTable<_>.Result x y


        end
        open OperatorIntrinsics
        
                   
        let inline (..) (n:^T) (m:^T) = 
           RangeGeneric (GenericOne< (^T) >, Checked.( + ),n,m)
           when ^T : int32       = RangeInt32   (retype n,1    ,retype m)
           when ^T : float       = RangeDouble  (retype n,1.0  ,retype m)
           when ^T : float32     = RangeSingle  (retype n,1.0f ,retype m)
           when ^T : int64       = RangeInt64   (retype n,1L   ,retype m)
           when ^T : uint64      = RangeUInt64  (retype n,1UL  ,retype m)
           when ^T : uint32      = RangeUInt32  (retype n,1ul  ,retype m)
           when ^T : nativeint   = RangeIntPtr  (retype n,1n   ,retype m)
           when ^T : unativeint  = RangeUIntPtr (retype n,1un  ,retype m)
           when ^T : int16       = RangeInt16   (retype n,1s   ,retype m)
           when ^T : uint16      = RangeUInt16  (retype n,1us  ,retype m)
           when ^T : sbyte       = RangeSByte   (retype n,1y   ,retype m)
           when ^T : byte        = RangeByte    (retype n,1uy  ,retype m)
           when ^T : char        = RangeChar    (retype n,retype m)

        let inline (.. ..) (n:^T) (step:^U) (m:^T) = 
           RangeStepGeneric (GenericZero< (^U) >, Checked.( + ),n,step,m)
           when ^T : int32       = RangeInt32   (retype n ,retype step ,retype m)
           when ^T : float       = RangeDouble  (retype n ,retype step ,retype m)
           when ^T : float32     = RangeSingle  (retype n ,retype step ,retype m)
           when ^T : int64       = RangeInt64   (retype n ,retype step ,retype m)
           when ^T : uint64      = RangeUInt64  (retype n ,retype step ,retype m)
           when ^T : uint32      = RangeUInt32  (retype n ,retype step ,retype m)
           when ^T : nativeint   = RangeIntPtr  (retype n ,retype step ,retype m)
           when ^T : unativeint  = RangeUIntPtr (retype n ,retype step ,retype m)
           when ^T : int16       = RangeInt16   (retype n ,retype step ,retype m)
           when ^T : uint16      = RangeUInt16  (retype n ,retype step ,retype m)
           when ^T : sbyte       = RangeSByte   (retype n ,retype step ,retype m)
           when ^T : byte        = RangeByte    (retype n ,retype step ,retype m)
        

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline abs (x: ^T) : ^T = 
             AbsDynamic x
             when ^T : ^T = abs_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  acos(x: ^T) : ^T = 
             AcosDynamic x
             when ^T : ^T       = acos_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  asin(x: ^T) : ^T = 
             AsinDynamic x
             when ^T : ^T       = asin_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  atan(x: ^T) : ^T = 
             AtanDynamic x
             when ^T : ^T       = atan_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  atan2(x: ^T) (y: ^T) : 'U = 
             Atan2Dynamic x y
             when ^T : ^T       = (atan2_impl x y : 'U)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  ceil(x: ^T) : ^T = 
             CeilingDynamic x
             when ^T : ^T       = ceil_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  exp(x: ^T) : ^T = 
             ExpDynamic x
             when ^T : ^T       = exp_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline floor (x: ^T) : ^T = 
             FloorDynamic x
             when ^T : ^T       = floor_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline truncate (x: ^T) : ^T = 
             TruncateDynamic x
             when ^T : ^T       = truncate_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline round (x: ^T) : ^T = 
             RoundDynamic x
             when ^T : ^T       = round_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline sign (x: ^T) : int = 
             SignDynamic x
             when ^T : ^T       = sign_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  log(x: ^T) : ^T = 
             LogDynamic x
             when ^T : ^T       = log_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  log10(x: ^T) : ^T = 
             Log10Dynamic x
             when ^T : ^T       = log10_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  sqrt(x: ^T) : ^U = 
             SqrtDynamic x
             when ^T : ^T       = (sqrt_impl x : ^U)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  cos(x: ^T) : ^T = 
             CosDynamic x
             when ^T : ^T       = cos_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  cosh(x: ^T) : ^T = 
             CoshDynamic x
             when ^T : ^T       = cosh_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  sin(x: ^T) : ^T = 
             SinDynamic x
             when ^T : ^T       = sin_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  sinh(x: ^T) : ^T = 
             SinhDynamic x
             when ^T : ^T       = sinh_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  tan(x: ^T) : ^T = 
             TanDynamic x
             when ^T : ^T       = tan_impl x


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline  tanh(x: ^T) : ^T = 
             TanhDynamic x
             when ^T : ^T       = tanh_impl x

        let inline  ( ** ) (x: ^T) (y: ^T) : ^T = 
             PowDynamic x y
             when ^T : ^T       = pow_impl x y


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline gpown  (x: ^T) n =
            let v = PowGeneric (GenericOne< (^T) >, Checked.( * ), x,n) 
            if n < 0 then GenericOne< (^T) > / v
            else v

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let inline pown  (x: ^T) n =
             (if n = Int32.MinValue then gpown x (n+1) / x else gpown x n)
             when ^T : int32 = 
                         (let x = (retype x : int32) in
                          if  x = 2 && n >= 0 && n < 31 then 1 <<< n 
                          elif n >= 0 then PowInt32 x n 
                          else 1 / PowInt32 x n)
             when ^T : int64 = 
                         (let x = (retype x : int64) in
                          if  x = 2L && n >= 0 && n < 63 then 1L <<< n 
                          elif n >= 0 then PowInt64 x n 
                          else 1L / PowInt64 x n)
             when ^T : int16 = 
                         (let x = (retype x : int16) in
                          if  x = 2s && n >= 0 && n < 15 then 1s <<< n 
                          elif n >= 0 then PowInt16 x n 
                          else 1s / PowInt16 x n)
             when ^T : sbyte = 
                         (let x = (retype x : sbyte) in
                          if  x = 2y && n >= 0 && n < 7 then 1y <<< n 
                          elif n >= 0 then PowSByte x n 
                          else 1y / PowSByte x n)
             when ^T : nativeint = 
                         (let x = (retype x : nativeint) in
                          if  x = 2n && n >= 0 && n < 31 then 1n <<< n 
                          elif n >= 0 then PowIntPtr x n 
                          else 1n / PowIntPtr x n)
             when ^T : uint32 = 
                         (let x = (retype x : uint32) in
                          if  x = 2u && n >= 0 && n <= 31 then 1u <<< n 
                          elif n >= 0 then PowUInt32 x n 
                          else 1u / PowUInt32 x n)
             when ^T : uint64 = 
                         (let x = (retype x : uint64) in
                          if  x = 2UL && n >= 0 && n <= 63 then 1UL <<< n 
                          elif n >= 0 then PowUInt64 x n 
                          else 1UL / PowUInt64 x n)
             when ^T : uint16 = 
                         (let x = (retype x : uint16) in
                          if  x = 2us && n >= 0 && n <= 15 then 1us <<< n 
                          elif n >= 0 then PowUInt16 x n 
                          else 1us / PowUInt16 x n)
             when ^T : byte = 
                         (let x = (retype x : byte) in
                          if  x = 2uy && n >= 0 && n <= 7 then 1uy <<< n 
                          elif n >= 0 then PowByte x n 
                          else 1uy / PowByte x n)
             when ^T : unativeint = 
                         (let x = (retype x : unativeint) in
                          if  x = 2un && n >= 0 && n <= 31 then 1un <<< n 
                          elif n >= 0 then PowUIntPtr x n 
                          else 1un / PowUIntPtr x n)

             when ^T : float       = 
                         (let x = (retype x : float) in
                         if n >= 0 then PowDouble x n else 1.0 /  PowDouble x n)
             when ^T : float32     = 
                         (let x = (retype x : float32) in
                          if n >= 0 then PowSingle x n else 1.0f /  PowSingle x n)
             when ^T : decimal     = 
                         (let x = (retype x : decimal) in
                          if n >= 0 then PowDecimal x n else 1.0M /  PowDecimal x n)


        
        




//============================================================================
//============================================================================

namespace Microsoft.FSharp.Control
    open System.Diagnostics
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.Operators

    exception Undefined
    
    type LazyFailure(exn:exn) =
        static let undefined = new LazyFailure(Undefined)
        member x.Exception = exn
        static member Undefined = undefined
    
#if FX_ATLEAST_40
    open System.Threading    

    module LazyExtensions = 
        type System.Threading.LazyInit<'T> with
            static member Create(f : unit -> 'T) =
                let creator = new System.Func<'T>(f)
                System.Threading.LazyInit<'T>(creator,System.Threading.LazyInitMode.EnsureSingleExecution)
            static member CreateFromValue(value : 'T) =
                System.Threading.LazyInit<'T>.Create(fun () -> value)
            member x.IsDelayed = not(x.IsInitialized)
            member x.IsForced = x.IsInitialized
            member x.IsException = false
            member x.Force() = x.Value
            member x.SynchronizedForce() = x.Value
            member x.UnsynchronizedForce() = x.Value
            
    type Lazy<'T> = System.Threading.LazyInit<'T>

    and 'T ``lazy`` = Lazy<'T>       
#else

        
    [<DefaultAugmentation(false)>]
    [<StructuralEquality(false); StructuralComparison(false)>]
    [<StructuredFormatDisplay("{StructuredFormatDisplay}")>]                                                     
    type Lazy<'T> = 
        { /// This field holds the result of a successful computation. It's initial value is Unchecked.defaultof
          mutable value : 'T
          /// This field holds either the function to run or a LazyFailure object recording the exception raised 
          /// from running the function. It is null if the thunk has been evaluated successfully.
          mutable funcOrException: obj }
        static member Create(f: (unit->'T)) : Lazy<'T> = 
            { value = Unchecked.defaultof<'T>;
              funcOrException = box(f); }
        static member CreateFromValue(x:'T) : Lazy<'T> = 
            { value = x;
              funcOrException = null; }
        member x.IsDelayed = (match x.funcOrException with null -> false | :? LazyFailure -> false | _ -> true)
        member x.IsException = (match x.funcOrException with null -> false | :? LazyFailure -> true | _ -> false)
        member x.IsForced = (match x.funcOrException with null -> true | _ -> false)
        member x.Force() =  
            match x.funcOrException with 
            | null -> x.value 
            | _ -> 
                // Enter the lock in case another thread is in the process of evaluting the result
                System.Threading.Monitor.Enter(x);
                try 
                    x.UnsynchronizedForce()
                finally
                    System.Threading.Monitor.Exit(x)
        member x.UnsynchronizedForce() = 
            match x.funcOrException with 
            | null -> x.value 
            | :? LazyFailure as res -> 
                  raise(res.Exception)
            | :? (unit -> 'T) as f -> 
                  x.funcOrException <- box(LazyFailure.Undefined)
                  try 
                      let res = f () 
                      x.funcOrException <- null; 
                      x.value <- res; 
                      res
                  with e -> 
                      x.funcOrException <- box(new LazyFailure(e)); 
                      rethrow()
            | _ -> 
                failwith "unreachable"
        member x.SynchronizedForce() = x.Force()
#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Collapsed)>]
#endif
        member x.Value = x.Force()

        member internal x.StructuredFormatDisplay = x.ToString() // for the StructuredFormatDisplay attribute
        override x.ToString() = 
            if x.IsForced then 
                if box x.Value = null then
                    "<null>"
                else
                    x.Value.ToString()
            else
                match x.funcOrException with 
                | :? LazyFailure as res -> 
                    match res.Exception with 
                    | Undefined -> "<evaluating>"
                    | e ->  e.ToString()
                | _ -> 
                    "<unevaluated>"
        
    and 'T ``lazy`` = Lazy<'T>    
#endif



//============================================================================
//============================================================================


namespace Microsoft.FSharp.Control

    open System.Diagnostics
    open Microsoft.FSharp.Core

    type IDelegateEvent<'Del when 'Del :> System.Delegate > =
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="AddHandler")>]
        abstract AddHandler: handler:'Del -> unit
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="RemoveHandler")>]
        abstract RemoveHandler: handler:'Del -> unit 

    type IEvent<'Del,'Args when 'Del : delegate<'Args,unit> and 'Del :> System.Delegate > =
        abstract Add: callback:('Args -> unit) -> unit
        inherit IDelegateEvent<'Del>

    [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",Justification="This type is an all-purpose Handler and is deliberately dsigned to have a simple name")>]
    type Handler<'Args> =  delegate of sender:obj * args:'Args -> unit 

    type IEvent<'Args> = IEvent<Handler<'Args>, 'Args>
    
    // FxCop suppressions 
    open System.Diagnostics.CodeAnalysis
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_Addition`3(!!0,!!1)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_BitwiseAnd`1(!!0,!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_BitwiseOr`1(!!0,!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_Division`3(!!0,!!1)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_Equality`1(!!0,!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_ExclusiveOr`1(!!0,!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_GreaterThanOrEqual`1(!!0,!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_GreaterThan`1(!!0,!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_LeftShift`1(!!0,System.Int32)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_LessThanOrEqual`1(!!0,!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_LessThan`1(!!0,!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_LogicalNot`1(!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_Modulus`3(!!0,!!1)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_Multiply`3(!!0,!!1)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_RightShift`1(!!0,System.Int32)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_Subtraction`3(!!0,!!1)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_UnaryNegation`1(!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators.#op_UnaryPlus`1(!!0)",Justification="This is an F# primitive operator name")>]
    do()

    // FxCop suppressions 
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators+Checked.#op_Addition`3(!!0,!!1)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators+Checked.#op_Multiply`3(!!0,!!1)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators+Checked.#op_Subtraction`3(!!0,!!1)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Core.Operators+Checked.#op_UnaryNegation`1(!!0)",Justification="This is an F# primitive operator name")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="Microsoft.FSharp.Quotations.Expr.#Type",Justification="This appears to be a false warning from FxCop")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Scope="member", Target="Microsoft.FSharp.Quotations.Var.#Type",Justification="This appears to be a false warning from FxCop")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope="member", Target="Microsoft.FSharp.Control.Event`1.#Publish",Justification="")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope="type", Target="Microsoft.FSharp.Collections.Set`1",Justification="Adding suffix 'Collection' would break the simple user model of this type, akin to 'List'")>]
    
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope="type", Target="Microsoft.FSharp.Collections.List`1+_Empty",Justification="This is a compilation residue from a public discrimianted union, which are allowed in FSharp.Core.dll")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope="type", Target="Microsoft.FSharp.Collections.List`1+_Cons",Justification="This is a compilation residue from a public discrimianted union, which are allowed in FSharp.Core.dll")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2224:OverrideEqualsOnOverloadingOperatorEquals", Scope="type", Target="Microsoft.FSharp.Core.Operators",Justification="This is from the use of op_Equality as a primitive F# operator name. We do not need any override")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Scope="member", Target="Microsoft.FSharp.Control.Event`1.#Publish",Justification="This appears to be a false warning from FxCop")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Scope="member", Target="Microsoft.FSharp.Core.Operators.#raise`1(System.Exception)",Justification="No event required here")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Layout",Justification="This functionality is scheduled for deletion from FSharp.Core.dll")>]
    
    
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", Scope="member", Target="Microsoft.FSharp.Quotations.Expr.#Value(System.Object,System.Type)", MessageId="0#")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", Scope="member", Target="Microsoft.FSharp.Quotations.Expr.#Value`1(!!0)", MessageId="0#")>]

    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.LanguagePrimitives+HashCompare",Justification="The Microsoft.FSharp.Core namespace uses some nested modules and types by design")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Operators+Checked",Justification="The Microsoft.FSharp.Core namespace uses some nested modules and types by design")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Operators+Unchecked",Justification="The Microsoft.FSharp.Core namespace uses some nested modules and types by design")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Operators+OperatorIntrinsics",Justification="The Microsoft.FSharp.Core namespace uses some nested modules and types by design")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.OptimizedClosures+FastFunc2`3",Justification="The Microsoft.FSharp.Core namespace uses some nested modules and types by design")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.OptimizedClosures+FastFunc3`4",Justification="The Microsoft.FSharp.Core namespace uses some nested modules and types by design")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.OptimizedClosures+FastFunc4`5",Justification="The Microsoft.FSharp.Core namespace uses some nested modules and types by design")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.OptimizedClosures+FastFunc5`6",Justification="The Microsoft.FSharp.Core namespace uses some nested modules and types by design")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Operators+OperatorIntrinsics",Justification="The Microsoft.FSharp.Core namespace uses some nested modules and types by design")>]

    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice2Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice4Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice5Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice6Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice1Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice2Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice3Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`4+_Choice4Of4",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`4+_Choice1Of4",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`4+_Choice2Of4",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`4+_Choice3Of4",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice1Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice7Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice4Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice5Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice6Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice1Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice3Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`3+_Choice1Of3",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`3+_Choice2Of3",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`3+_Choice3Of3",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`2+_Choice1Of2",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`2+_Choice2Of2",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice4Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice5Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice2Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice3Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Layout+_Attr",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Joint+_Breakable",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Layout+_Node",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Layout+_Leaf",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Joint+_Unbreakable",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Joint+_Broken",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Collections.List`1+_Cons",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Scope="type", Target="Microsoft.FSharp.Collections.List`1+_Empty",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]


    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Control.UndefinedException",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Choice`5",Justification="F# implements IComparable as a way to implement its generic equality")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.DynamicInvocationNotSupportedException",Justification="F# implements IComparable as a way to implement its generic equality")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.FailureException",Justification="F# implements IComparable as a way to implement its generic equality")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.MatchFailureException",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Choice`4",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Choice`7",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Choice`6",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Choice`3",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Choice`2",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Tuple`2",Justification="This type will be deleted once Dev10 tuple types are shared")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Tuple`7",Justification="This type will be deleted once Dev10 tuple types are shared")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Tuple`6",Justification="This type will be deleted once Dev10 tuple types are shared")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Tuple`5",Justification="This type will be deleted once Dev10 tuple types are shared")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Tuple`4",Justification="This type will be deleted once Dev10 tuple types are shared")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Tuple`3",Justification="This type will be deleted once Dev10 tuple types are shared")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Option`1",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Core.Ref`1",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Quotations.Var",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Layout",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.FormatOptions",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Joint",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Collections.Map`2",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Collections.Set`1",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Scope="type", Target="Microsoft.FSharp.Collections.List`1",Justification="F# implements IComparable as a way to implement its generic, structural equality. Corresponding relational operators are only required if the type is expected to be used directly from other .NET languages with ground comparison semantics")>]

    [<assembly: SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", Scope="member", Target="Microsoft.FSharp.Core.Ref`1.#contents", MessageId="contents",Justification="This lowercase public name is provided for compatibility with OCaml. For technical reasons it can't be placed in a different DLL")>]


    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice4Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice5Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice6Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice7Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice1Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice2Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`7+_Choice3Of7",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`4+_Choice4Of4",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`4+_Choice1Of4",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`4+_Choice2Of4",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`4+_Choice3Of4",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice1Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice4Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice5Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice6Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice1Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice2Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`6+_Choice3Of6",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`3+_Choice1Of3",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`3+_Choice2Of3",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`3+_Choice3Of3",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`2+_Choice1Of2",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`2+_Choice2Of2",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice4Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice5Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice2Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Core.Choice`5+_Choice3Of5",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Joint+_Unbreakable",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Layout+_Attr",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Joint+_Breakable",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Layout+_Node",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Layout+_Leaf",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Text.StructuredPrintfImpl.Joint+_Broken",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Collections.List`1+_Cons",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]
    [<assembly: SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Scope="type", Target="Microsoft.FSharp.Collections.List`1+_Empty",Justification="Public discriminated unions compile to nested types with names containing underscores and are allowed in FSharp.Core.dll because they are used to implement language primitives")>]


    [<assembly: SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Scope="type", Target="Microsoft.FSharp.Control.UndefinedException",Justification="Like F# record types, F# exception declarations implement one primary constructor which accepts initial values for all fields")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Scope="type", Target="Microsoft.FSharp.Core.DynamicInvocationNotSupportedException",Justification="Like F# record types, F# exception declarations implement one primary constructor which accepts initial values for all fields")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Scope="type", Target="Microsoft.FSharp.Core.FailureException",Justification="Like F# record types, F# exception declarations implement one primary constructor which accepts initial values for all fields")>]
    [<assembly: SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Scope="type", Target="Microsoft.FSharp.Core.MatchFailureException",Justification="Like F# record types, F# exception declarations implement one primary constructor which accepts initial values for all fields")>]
    do()

