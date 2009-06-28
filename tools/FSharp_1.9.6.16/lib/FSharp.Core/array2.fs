//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Collections

    open System.Diagnostics
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Primitives.Basics

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Array2D =

        // Define the primitive operations. 
        // Note: the "type" syntax is for the type parameter for inline 
        // polymorphic IL. This helps the compiler inline these fragments, 
        // i.e. work out the correspondence between IL and F# type variables. 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let length1 (arr: 'a[,]) =  (# "ldlen.multi 2 0" arr : int #)  
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let length2 (arr: 'a[,]) =  (# "ldlen.multi 2 1" arr : int #)  
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let base1 (arr: 'a[,]) = arr.GetLowerBound(0)  
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let base2 (arr: 'a[,]) = arr.GetLowerBound(1) 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let get (arr: 'a[,]) (n:int) (m:int) =  (# "ldelem.multi 2 !0" type ('a) arr n m : 'a #)  
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let set (arr: 'a[,]) (n:int) (m:int) (x:'a) =  (# "stelem.multi 2 !0" type ('a) arr n m x #)  

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let zeroCreate (n:int) (m:int) = (# "newarr.multi 2 !0" type ('a) n m : 'a[,] #)

        let zeroCreateBased (b1:int) (b2:int) (n1:int) (n2:int) = 
            if (b1 = 0 && b2 = 0) then 
                // Note: this overload is available on Compact Framework and Silverlight
                (System.Array.CreateInstance(typeof<'a>, [|n1;n2|]) :?> 'a[,])
            else
#if FX_NO_BASED_ARRAYS
                raise (new System.NotSupportedException("arrays with non-zero base may not be created on this platform"))
#else
                (System.Array.CreateInstance(typeof<'a>, [|n1;n2|],[|b1;b2|]) :?> 'a[,])
#endif

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let zero_create_based b1 b2 n1 n2 =  zeroCreateBased b1 b2 n1 n2

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let createBased b1 b2 n m (x:'a) = 
            let arr = (zeroCreateBased b1 b2 n m : 'a[,])  
            for i = b1 to b1+n - 1 do 
              for j = b2 to b2+m - 1 do 
                arr.[i,j] <- x
            arr

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let initBased b1 b2 n m f = 
            let arr = (zeroCreateBased b1 b2 n m : 'a[,])  
            for i = b1 to b1+n - 1 do 
              for j = b2 to b2+m - 1 do 
                arr.[i,j] <- f i j
            arr


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let create n m (x:'a) = 
            createBased 0 0 n m x

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let init n m f = 
            initBased 0 0 n m f

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iter f arr = 
            let count1 = length1 arr 
            let count2 = length2 arr 
            let b1 = base1 arr 
            let b2 = base2 arr 
            for i = b1 to b1+count1 - 1 do 
              for j = b1 to b2+count2 - 1 do 
                f arr.[i,j]

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iteri (f : int -> int -> 'a -> unit) (arr:'a[,]) =
            let count1 = length1 arr 
            let count2 = length2 arr 
            let b1 = base1 arr 
            let b2 = base2 arr 
            for i = b1 to b1+count1 - 1 do 
              for j = b1 to b2+count2 - 1 do 
                f i j arr.[i,j]

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let map f arr = 
            initBased (base1 arr) (base2 arr) (length1 arr) (length2 arr) (fun i j -> f arr.[i,j])

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let mapi f arr = 
            initBased (base1 arr) (base2 arr) (length1 arr) (length2 arr) (fun i j -> f i j arr.[i,j])

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let copy arr = 
            initBased (base1 arr) (base2 arr) (length1 arr) (length2 arr) (fun i j -> arr.[i,j])
            
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let rebase arr = 
           let b1 = base1 arr
           let b2 = base2 arr
           init (length1 arr) (length2 arr) (fun i j -> arr.[b1+i,b2+j])


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let copyTo(source: 'a[,],sourceIndex1,sourceIndex2,target: 'a[,],targetIndex1,targetIndex2,count1,count2) =
            if sourceIndex1 < 0 then invalidArg "sourceIndex1" "index must be positive"
            if sourceIndex2 < 0 then invalidArg "sourceIndex2" "index must be positive"
            if targetIndex1 < 0 then invalidArg "targetIndex1" "index must be positive"
            if targetIndex2 < 0 then invalidArg "targetIndex2" "index must be positive"
            if sourceIndex1 + count1 > length1 source then invalidArg "count1" "out of range"
            if sourceIndex2 + count2 > length2 source then invalidArg "count2" "out of range"
            if targetIndex1 + count1 > length1 target then invalidArg "count1" "out of range"
            if targetIndex2 + count2 > length2 target then invalidArg "count2" "out of range"

            for i = 0 to count1 - 1 do
                for j = 0 to count2 - 1 do
                    target.[targetIndex1+i,targetIndex2+j] <- source.[sourceIndex1+i,sourceIndex2+j]


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let zero_create count1 count2 = zeroCreate count1 count2 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let create_based base1 base2 count1 count2 initial = createBased base1 base2 count1 count2 initial

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let init_based base1 base2 count1 count2 f = initBased base1 base2 count1 count2 f

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let sub (source : 'a[,])  sourceIndex1 sourceIndex2 count1 count2 = 
            if sourceIndex1 < 0 then invalidArg "sourceIndex1" "index must be positive"
            if sourceIndex2 < 0 then invalidArg "sourceIndex2" "index must be positive"
            if count1 < 0 then invalidArg "count1" "length must be positive"
            if count2 < 0 then invalidArg "count2" "length must be positive"
            if sourceIndex1 + count1 > length1 source then invalidArg "count1" "out of range"
            if sourceIndex2 + count2 > length2 source then invalidArg "count2" "out of range"

            let res = zeroCreate count1 count2
            for i = 0 to count1 - 1 do
                for j = 0 to count2 - 1 do
                    res.[i,j] <- source.[sourceIndex1 + i,sourceIndex2 + j]
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let blit (source : 'a[,])  sourceIndex1 sourceIndex2 target targetIndex1 targetIndex2 count1 count2 = 
            copyTo(source,sourceIndex1,sourceIndex2,target,targetIndex1,targetIndex2,count1,count2)
        
