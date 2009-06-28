//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Collections

    #nowarn "51"

    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Primitives.Basics
    open System
    open System.Diagnostics
    open System.Collections
    open System.Collections.Generic

    module HashIdentity = 
                
        let inline Structural<'a> : IEqualityComparer<'a> = 
            // type-specialize some common cases to generate more efficient functions 
            { new IEqualityComparer<'a> with 
                  member self.GetHashCode(x) = LanguagePrimitives.GenericHash(x) 
                  member self.Equals(x,y) = LanguagePrimitives.GenericEquality x y }
              
        let LimitedStructural<'T>(limit) : IEqualityComparer<'T> = 
            LanguagePrimitives.FastLimitedGenericEqualityComparer<'T>(limit)
              
        let Reference<'T> : IEqualityComparer<'T> = 
            { new IEqualityComparer<'T> with
                  member self.GetHashCode(x) = LanguagePrimitives.PhysicalHash(x) 
                  member self.Equals(x,y) = LanguagePrimitives.PhysicalEquality x y }

        let inline FromFunctions hash eq : IEqualityComparer<'a> = 
            let eq = OptimizedClosures.FastFunc2<_,_,_>.Adapt(eq)
            { new IEqualityComparer<'a> with 
                member self.GetHashCode(x) = hash x
                member self.Equals(x,y) = eq.Invoke(x,y)  }


    module ComparisonIdentity = 

        let inline MakeFastStructuralComparisonFunction<'a>() : OptimizedClosures.FastFunc2<'a,'a,int> = 
            OptimizedClosures.FastFunc2<'a,'a,int>.Adapt(LanguagePrimitives.GenericComparison)

        let Char    = MakeFastStructuralComparisonFunction<char>()
        let String  = MakeFastStructuralComparisonFunction<string>()
        let SByte   = MakeFastStructuralComparisonFunction<sbyte>()
        let Int16   = MakeFastStructuralComparisonFunction<int16>()
        let Int32   = MakeFastStructuralComparisonFunction<int32>()
        let Int64   = MakeFastStructuralComparisonFunction<int64>()
        let IntPtr  = MakeFastStructuralComparisonFunction<nativeint>()
        let Byte    = MakeFastStructuralComparisonFunction<byte>()
        let UInt16  = MakeFastStructuralComparisonFunction<uint16>()
        let UInt32  = MakeFastStructuralComparisonFunction<uint32>()
        let UInt64  = MakeFastStructuralComparisonFunction<uint64>()
        let UIntPtr = MakeFastStructuralComparisonFunction<unativeint>()

        /// Use a type-indexed table to ensure we only create a single FastStructuralComparison function
        /// for each type
        [<CodeAnalysis.SuppressMessage("Microsoft.Performance","CA1812:AvoidUninstantiatedInternalClasses")>]     
        type FastStructuralComparisonTable<'a>() = 
            static let f : OptimizedClosures.FastFunc2<'a,'a,int> = 
                match typeof<'a> with 
                | ty when ty.Equals(typeof<byte>) -> unbox (box Byte)
                | ty when ty.Equals(typeof<char>) -> unbox (box Char)
                | ty when ty.Equals(typeof<sbyte>) -> unbox (box SByte)
                | ty when ty.Equals(typeof<int16>) -> unbox (box Int16)
                | ty when ty.Equals(typeof<int32>) -> unbox (box Int32)
                | ty when ty.Equals(typeof<int64>) -> unbox (box Int64)
                | ty when ty.Equals(typeof<nativeint>) -> unbox (box IntPtr)
                | ty when ty.Equals(typeof<uint16>) -> unbox (box UInt16)
                | ty when ty.Equals(typeof<uint32>) -> unbox (box UInt32)
                | ty when ty.Equals(typeof<uint64>) -> unbox (box UInt64)
                | ty when ty.Equals(typeof<unativeint>) -> unbox (box UIntPtr)
                | ty when ty.Equals(typeof<string>) -> unbox (box String)
                | _ -> MakeFastStructuralComparisonFunction<'a>()
            static member Function : OptimizedClosures.FastFunc2<'a,'a,int> = f
        
        let GetFastStructuralComparisonFunction<'a>() : OptimizedClosures.FastFunc2<'a,'a,int> = 
            FastStructuralComparisonTable<'a>.Function

        /// If an IComparer also implements this interface then library implementations
        /// have the option of using the given FastComparisonFunction 
        /// for implementing comparison semantics. Calling the FastFunc2 implementation
        /// tends to be faster than calling IComparer. Furthermore most F# comparers are 
        /// ultimately specified using FastFunc2 values, hence making a direct call to 
        /// a FastFunc2 value avoids a double indirection.
        type IHasFastComparisonFunction<'a> =
            interface IComparer<'a> 
            abstract FastComparisonFunction : OptimizedClosures.FastFunc2<'a,'a,int>

        let GetFastComparisonFunction(icomparer : IComparer<'a>) : OptimizedClosures.FastFunc2<'a,'a,int> = 
            match box icomparer with 
            | :? IHasFastComparisonFunction<'a> as x -> x.FastComparisonFunction
            | _ -> OptimizedClosures.FastFunc2<_,_,_>.Adapt(fun x y -> icomparer.Compare(x,y))

        let Structural<'a> : IComparer<'a> = 
            let comparer = MakeFastStructuralComparisonFunction<'a>()
            { new IComparer<'a> with
                  member self.Compare(x,y) = LanguagePrimitives.GenericComparison x y
              interface IHasFastComparisonFunction<'a> with 
                 member self.FastComparisonFunction = comparer }
            
        let FromFunction comparer = 
            let comparer = OptimizedClosures.FastFunc2<'a,'a,int>.Adapt(comparer)
            { new IComparer<'a> with
                  member self.Compare(x,y) = comparer.Invoke(x,y)
              interface IHasFastComparisonFunction<'a> with 
                 member self.FastComparisonFunction = comparer } 


