//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

/// This namespace contains some common collections in a style primarily designed for use from F#.  

namespace Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Primitives.Basics
    open System
    open System.Collections.Generic

    /// Common notions of comparison identity used with sorted data structures.
    module ComparisonIdentity = 
      
        /// Structural comparison.  Compare using Operators.compare.
        val Structural<'T> : IComparer<'T> 
        
        /// Convert an existing IComparer object into a comparison function with a fast entry point
        /// If comparer was originally built using ComparisonIdentity.FromFunction then the original function will be
        /// returned
        val GetFastComparisonFunction : comparer:IComparer<'T> -> OptimizedClosures.FastFunc2<'T,'T,int> 

        /// Convert an existing IComparer object into a comparison function with a fast entry point
        val GetFastStructuralComparisonFunction : unit -> OptimizedClosures.FastFunc2<'T,'T,int> 
        
        /// Compare using the given comparer function
        val FromFunction : comparer:('T -> 'T -> int) -> IComparer<'T> 
        
    /// Common notions of value identity used with hash tables.
    module HashIdentity = 

        /// Structural hashing.  Hash using Operators.(=) and Operators.hash.
        
        // inline justification: allows specialization of structural hash functions based on type
        val inline Structural<'T> : IEqualityComparer<'T> 
        
        val LimitedStructural<'T> : limit: int -> IEqualityComparer<'T> 
        
        /// Physical hashing (hash on reference identity of objects, and the contents of value types).  
        /// Hash using LanguagePrimitives.PhysicalEquality and LanguagePrimitives.PhysicalHash,
        /// That is, for value types use GetHashCode and Object.Equals (if no other optimization available),
        /// and for reference types use System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode and 
        /// reference equality. 
        //
        // inline justification: allows specialization of reference hash functions based on type
        val Reference<'T>   : IEqualityComparer<'T> 
        
        /// Hash using the given hashing and equality functions
        //
        // inline justification: allows inlining of hash functions 
        val inline FromFunctions<'T> : hasher:('T -> int) -> equality:('T -> 'T -> bool) -> IEqualityComparer<'T> 

    
