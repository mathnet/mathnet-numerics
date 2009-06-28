//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Collections

    open System
    open System.Diagnostics
    open System.Collections.Generic
    open System.Diagnostics.CodeAnalysis
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Primitives.Basics

    /// Basic operations on arrays
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Array = 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let length (array: array<_>)    = array.Length

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let init n f      = Microsoft.FSharp.Primitives.Basics.Array.init n f

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let zeroCreate n    = Microsoft.FSharp.Primitives.Basics.Array.zeroCreate n

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let create (n:int) (x:'T) =
            let array = (zeroCreate n : array<'T>) 
            for i = 0 to n - 1 do 
                array.[i] <- x
            array


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let isEmpty (array: array<'T>) = (array.Length = 0)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let empty<'T> = ([| |] : 'T [])

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let blit (array1 : array<'T>) sourceIndex (array2: array<'T>) targetIndex count = 
            if sourceIndex < 0 then invalidArg "sourceIndex" "index must be positive"
            if count < 0 then invalidArg "count" "length must be positive"
            if targetIndex < 0 then invalidArg "targetIndex" "index must be positive"
            if sourceIndex + count > array1.Length then invalidArg "sourceIndex" "out of range"
            if targetIndex + count > array2.Length then invalidArg "targetIndex" "out of range"
            for i = 0 to count - 1 do 
                array2.[targetIndex+i] <- array1.[sourceIndex + i]

        let rec concatAddLengths (arrs:array<'T> array) i acc =
            if i >= arrs.Length then acc 
            else concatAddLengths arrs (i+1) (acc + arrs.[i].Length)

        let rec concatBlit (arrs:array<array<'T>>) i j (tgt:array<'T>) =
            if i < arrs.Length then 
                let h = arrs.[i]
                let len = h.Length in  
                for i = 0 to len - 1 do 
                    tgt.[j+i] <- h.[i]
                concatBlit arrs (i+1) (j+len) tgt
                
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let concatArrays (arrs : array<array<'T>>) =
            let res = zeroCreate (concatAddLengths arrs 0 0) 
            concatBlit arrs 0 0 res ;
            res            

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let concat (arrs: seq<array<'T>>) = 
            let arrs = Seq.to_array arrs in
            concatArrays arrs

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]                         
        let collect (f : 'T -> array<'U>)  (input : array<'T>) : array<'U>=
                let inputLength = input.Length
                let result = Array.zeroCreate<array<'U>> inputLength
                for i = 0 to inputLength - 1 do
                    result.[i] <- f input.[i]
                concatArrays result

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let append (array1:array<'T>) (array2:array<'T>) = 
            let n1 = array1.Length 
            let n2 = array2.Length 
            let res = zeroCreate (n1 + n2)
            for i = 0 to n1 - 1 do 
                res.[i] <- array1.[i]
            for i = 0 to n2 - 1 do 
                res.[i+n1] <- array2.[i]
            res            

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let copy (array: array<'T>) =
            let len = array.Length 
            let res = zeroCreate len 
            for i = 0 to len - 1 do 
                res.[i] <- array.[i]
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let to_list array = Microsoft.FSharp.Primitives.Basics.List.of_array array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let of_list xs  = Microsoft.FSharp.Primitives.Basics.List.to_array xs

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iter f (array: array<'T>) = 
            let len = array.Length
            for i = 0 to len - 1 do 
                f array.[i]

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let map (f: 'T -> 'U) (array:array<'T>) =
            let len = array.Length
            let res = (zeroCreate len : array<'U>) 
            for i = 0 to len - 1 do 
                res.[i] <- f array.[i]
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iter2 f (array1: array<'T>) (array2: array<'U>) = 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let len1 = array1.Length 
            if len1 <> array2.Length then invalidArg "array2" "the arrays have different lengths";
            for i = 0 to len1 - 1 do 
                f.Invoke(array1.[i], array2.[i])

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let map2 f (array1: array<'T>) (array2: array<'U>) = 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let len1 = array1.Length 
            if len1 <> array2.Length then invalidArg "array2" "the arrays have different lengths";
            let res = zeroCreate len1 
            for i = 0 to len1 - 1 do 
                res.[i] <- f.Invoke(array1.[i], array2.[i])
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let mapi2 f (array1: array<'T>) (array2: array<'U>) = 
            let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
            let len1 = array1.Length 
            if len1 <> array2.Length then invalidArg "array2" "the arrays have different lengths";
            let res = zeroCreate len1 
            for i = 0 to len1 - 1 do 
                res.[i] <- f.Invoke(i,array1.[i], array2.[i])
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iteri f (array:array<'T>) =
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let len = array.Length
            for i = 0 to len - 1 do 
                f.Invoke(i, array.[i])

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iteri2 f (array1: array<'T>) (array2: array<'U>) = 
            let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
            let len1 = array1.Length 
            if len1 <> array2.Length then invalidArg "array2" "the arrays have different lengths";
            for i = 0 to len1 - 1 do 
                f.Invoke(i,array1.[i], array2.[i])

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let mapi (f : int -> 'T -> 'U) (array: array<'T>) =
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let len = array.Length
            let res = zeroCreate<'U> len 
            for i = 0 to len - 1 do 
                res.[i] <- f.Invoke(i,array.[i])
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let exists (f: 'T -> bool) (array:array<'T>) =
            let len = array.Length
            let rec loop i = i < len && (f array.[i] || loop (i+1))
            loop 0

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let exists2 f (array1: array<_>) (array2: array<_>) = 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let len1 = array1.Length
            if len1 <> array2.Length then invalidArg "array2" "the arrays have different lengths"
            let rec loop i = i < len1 && (f.Invoke(array1.[i], array2.[i]) || loop (i+1))
            loop 0

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let forall (f: 'T -> bool) (array:array<'T>) =
            let len = array.Length
            let rec loop i = i >= len || (f array.[i] && loop (i+1))
            loop 0

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let forall2 f (array1: array<_>) (array2: array<_>) = 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let len1 = array1.Length
            if len1 <> array2.Length then invalidArg "array2" "the arrays have different lengths"
            let rec loop i = i >= len1 || (f.Invoke(array1.[i], array2.[i]) && loop (i+1))
            loop 0

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let pick f (array: array<_>) = 
            let rec loop i = 
                if i >= array.Length then raise (System.Collections.Generic.KeyNotFoundException()) else 
                match f array.[i] with 
                | None -> loop(i+1)
                | Some res -> res
            loop 0 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryPick f (array: array<_>) = 
            let rec loop i = 
                if i >= array.Length then None else 
                match f array.[i] with 
                | None -> loop(i+1)
                | res -> res
            loop 0 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let choose f (array: array<_>) = 
            let res = new System.Collections.Generic.List<_>() // ResizeArray
            for i = 0 to array.Length - 1 do 
                match f array.[i] with 
                | None -> ()
                | Some b -> res.Add(b)
            res.ToArray()

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let filter f (array: array<_>) = 
            let res = new System.Collections.Generic.List<_>() // ResizeArray
            for i = 0 to array.Length - 1 do 
                let x = array.[i] 
                if f x then res.Add(x)
            res.ToArray()

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let partition f (array: array<_>) = 
            let res1 = new System.Collections.Generic.List<_>() // ResizeArray
            let res2 = new System.Collections.Generic.List<_>() // ResizeArray
            for i = 0 to array.Length - 1 do 
                let x = array.[i] 
                if f x then res1.Add(x) else res2.Add(x)
            res1.ToArray(), res2.ToArray()

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let find f (array: array<_>) = 
            let rec loop i = 
                if i >= array.Length then raise (System.Collections.Generic.KeyNotFoundException()) else 
                if f array.[i] then array.[i]  else loop (i+1)
            loop 0 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryFind f (array: array<_>) = 
            let rec loop i = 
                if i >= array.Length then None else 
                if f array.[i] then Some array.[i]  else loop (i+1)
            loop 0 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let zip (array1: array<_>) (array2: array<_>) = 
            let len1 = array1.Length 
            if len1 <> array2.Length then invalidArg "array2" "the arrays have different lengths"
            let res = zeroCreate len1 
            for i = 0 to len1 - 1 do 
                res.[i] <- (array1.[i],array2.[i])
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let zip3 (array1: array<_>) (array2: array<_>) (array3: array<_>) = 
            let len1 = array1.Length
            if len1 <> array2.Length then invalidArg "array2" "the arrays have different lengths"
            if len1 <> array3.Length then invalidArg "array3" "the arrays have different lengths"
            let res = zeroCreate len1 
            for i = 0 to len1 - 1 do 
                res.[i] <- (array1.[i],array2.[i],array3.[i])
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let unzip (array: array<_>) = 
            let len = array.Length 
            let res1 = zeroCreate len 
            let res2 = zeroCreate len 
            for i = 0 to len - 1 do 
                let x,y = array.[i] 
                res1.[i] <- x;
                res2.[i] <- y;
            res1,res2

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let unzip3 (array: array<_>) = 
            let len = array.Length 
            let res1 = zeroCreate len 
            let res2 = zeroCreate len 
            let res3 = zeroCreate len 
            for i = 0 to len - 1 do 
                let x,y,z = array.[i] 
                res1.[i] <- x;
                res2.[i] <- y;
                res3.[i] <- z;
            res1,res2,res3

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let rev (array: array<_>) = 
            let len = array.Length 
            let res = zeroCreate len 
            for i = 0 to len - 1 do 
                res.[(len - i) - 1] <- array.[i]
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold (f : 'T -> 'U -> 'T) (acc: 'T) (array:array<'U>) =
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let mutable state = acc 
            let len = array.Length
            for i = 0 to len - 1 do 
                state <- f.Invoke(state,array.[i])
            state

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let foldBack (f : 'T -> 'U -> 'U) (array:array<'T>) (acc: 'U) =
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let mutable res = acc 
            let len = array.Length
            for i = len - 1 downto 0 do 
                res <- f.Invoke(array.[i],res)
            res


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let foldBack2 f (array1:'T1[]) (array2:'T2 []) (acc: 'U) =
            let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
            let mutable res = acc 
            let len = array1.Length
            if len <> array2.Length then invalidArg "array2" "the arrays have different lengths"
            for i = len - 1 downto 0 do 
                res <- f.Invoke(array1.[i],array2.[i],res)
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold2  f (acc: 'T) (array1:'T1[]) (array2:'T2 []) =
            let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
            let mutable state = acc 
            let len = array1.Length
            if len <> array2.Length then invalidArg "array2" "the arrays have different lengths"
            for i = 0 to len - 1 do 
                state <- f.Invoke(state,array1.[i],array2.[i])
            state


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let foldSubLeft f acc (array : array<_>) start fin = 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let mutable res = acc 
            for i = start to fin do
                res <- f.Invoke(res,array.[i])
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let foldSubRight f (array : array<_>) start fin acc = 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let mutable res = acc 
            for i = fin downto start do
                res <- f.Invoke(array.[i],res)
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let scanSubRight f (array : array<_>) start fin initState = 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let mutable state = initState 
            let res = create (2+fin-start) initState 
            for i = fin downto start do
                state <- f.Invoke(array.[i],state);
                res.[i - start] <- state
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let scanSubLeft f  initState (array : array<_>) start fin = 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let mutable state = initState 
            let res = create (2+fin-start) initState 
            for i = start to fin do
                state <- f.Invoke(state,array.[i]);
                res.[i - start+1] <- state
            res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let scan f acc (array : array<_>) = 
            let arrn = array.Length
            scanSubLeft f acc array 0 (arrn - 1)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let scanBack f (array : array<_>) acc = 
            let arrn = array.Length
            scanSubRight f array 0 (arrn - 1) acc

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let reduce f (array : array<_>) = 
            let arrn = array.Length
            if arrn = 0 then invalidArg "array" "the input array may not be empty"
            else foldSubLeft f array.[0] array 1 (arrn - 1) 
        
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let reduceBack f (array : array<_>) = 
            let arrn = array.Length
            if arrn = 0 then invalidArg "array" "the input array may not be empty"
            else foldSubRight f array 0 (arrn - 2) array.[arrn - 1]

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let sortInPlaceWith f (array : array<'T>) =
            let len = array.Length 
            if len < 2 then () 
            elif len = 2 then 
                let c = f array.[0] array.[1] 
                if c > 0 then
                    let tmp = array.[0] 
                    array.[0] <- array.[1]; 
                    array.[1] <- tmp
            else 
                System.Array.Sort(array, ComparisonIdentity.FromFunction(f))

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let sortInPlaceBy (f: 'T -> 'U) (array : array<'T>) =
            let len = array.Length 
            if len < 2 then () 
            elif len = 2 then 
                let c = Operators.compare (f array.[0]) (f array.[1])
                if c > 0 then
                    let tmp = array.[0] 
                    array.[0] <- array.[1]; 
                    array.[1] <- tmp
            else 
                System.Array.Sort(array, ComparisonIdentity.FromFunction(fun x y -> Operators.compare (f x) (f y)))

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let sortWith (f: 'T -> 'T -> int) (array : array<'T>) =
            let array = copy array
            sortInPlaceWith f array;
            array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let sortBy f array =
            let array = copy array
            sortInPlaceBy f array;
            array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let sort array = 
            sortWith Operators.compare array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let sortInPlace array = 
            sortInPlaceWith Operators.compare array
            
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let to_seq a = Seq.of_array a

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let of_seq  ie = Seq.to_array ie

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let findIndex f (array : array<_>) = 
            let len = array.Length 
            let rec go n = if n >= len then raise (System.Collections.Generic.KeyNotFoundException()) elif f array.[n] then n else go (n+1)
            go 0

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryFindIndex f (array : array<_>) = 
            let len = array.Length 
            let rec go n = if n >= len then None elif f array.[n] then Some n else go (n+1)
            go 0 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let permute p (array : array<_>) =  Microsoft.FSharp.Primitives.Basics.Array.permute p array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let inline sum (array:array< (^T) >) : ^T = 
            let mutable acc = LanguagePrimitives.GenericZero< (^T) >
            for i = 0 to array.Length - 1 do
                acc <- Checked.(+) acc array.[i]
            acc

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let inline sumBy (f: 'T -> ^U) (array:array<'T>) : ^U = 
            let mutable acc = LanguagePrimitives.GenericZero< (^U) >
            for i = 0 to array.Length - 1 do
                acc <- Checked.(+) acc (f array.[i])
            acc

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let min (array:array<_>) = 
            if array.Length = 0 then invalidArg "array" "the array is empty"
            let mutable acc = array.[0]
            for i = 1 to array.Length - 1 do
                acc <- Operators.min acc array.[i]
            acc

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let minBy f (array:array<_>) = 
            if array.Length = 0 then invalidArg "array" "the array is empty"
            let mutable acc = f array.[0]
            let mutable acc_v = array.[0]
            for i = 1 to array.Length - 1 do
                let cur = f array.[i]
                if cur < acc then
                    acc <- cur
                    acc_v <- array.[i]
            acc_v

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let max (array:array<_>) = 
            if array.Length = 0 then invalidArg "array" "the array is empty"
            let mutable acc = array.[0]
            for i = 1 to array.Length - 1 do
                acc <- Operators.max acc array.[i]
            acc

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let maxBy f (array:array<_>) = 
            if array.Length = 0 then invalidArg "array" "the array is empty"
            let mutable acc = f array.[0]
            let mutable acc_v = array.[0]
            for i = 1 to array.Length - 1 do
                let cur = f array.[i]
                if cur > acc then
                    acc <- cur
                    acc_v <- array.[i]
            acc_v

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let inline average      (array:array<_>) = Seq.average array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let inline averageBy f (array:array<_>) = Seq.averageBy f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let sub (array:array<'T>) (startIndex:int) (count:int) =
            if startIndex < 0 then invalidArg "startIndex" "index must be positive"
            if count < 0 then invalidArg "count" "length must be positive"
            if startIndex + count > array.Length then invalidArg "count" "out of range"

            let res = (zeroCreate count : array<'T>)  
            for i = 0 to count - 1 do 
                res.[i] <- array.[startIndex + i]
            res


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let is_empty array = isEmpty array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let get (array:array<_>) n = array.[n]

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let set (array:array<_>) n v = array.[n] <- v

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let zero_create n = Array.zeroCreate n 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let fill (array:array<'T>) (startIndex:int) (count:int) (x:'T) =
            if startIndex < 0 then invalidArg "startIndex" "index must be positive"
            if count < 0 then invalidArg "count" "length must be positive"
            for i = startIndex to startIndex + count - 1 do 
                array.[i] <- x

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold_left f z array = fold f z array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold_right f array z = foldBack f array z

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold_left2 f z array1 array2 = fold2 f z array1 array2

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold_right2 f array1 array2 z = foldBack2 f array1 array2 z

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let reduce_left f array = reduce f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let reduce_right f array = reduceBack f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let scan_left f z array = scan f z array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let scan_right f array z = scanBack f array z

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let for_all f array = forall f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let for_all2 f array1 array2 = forall2 f array1 array2

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let tryfind f array = tryFind f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let find_index f array = findIndex f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryfind_index f array = tryFindIndex f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let first f array = tryPick f array

(*
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let sort f array = sortInPlaceWith f array
*)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let sort_by f array = sortInPlaceBy f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let inline sum_by f array = sumBy f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let inline average_by f array = averageBy f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let min_by  f array = minBy f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let max_by  f array = maxBy f array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let find_indexi f (array : array<_>) = 
            let len = array.Length 
            let rec go n = if n >= len then raise (System.Collections.Generic.KeyNotFoundException()) elif f n array.[n] then n else go (n+1)
            go 0

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryfind_indexi f (array : array<_>) = 
            let len = array.Length 
            let rec go n = if n >= len then None elif f n array.[n] then Some n else go (n+1)
            go 0 


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let split array = unzip array

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let combine array1 array2 = zip array1 array2

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let make (n:int) (x:'T) = create n x

#if FX_ATLEAST_40   
        module Parallel =
            open System.Threading
            
            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
            let choose f (c: array<'T>) = 
                let inputLength = c.Length
                let lastInputIndex = inputLength - 1

                let isChosen = zeroCreate<bool> inputLength
                let results = zeroCreate<'U> inputLength
                
                Parallel.For(0, inputLength, (fun i -> 
                    match f c.[i] with 
                    | None -> () 
                    | Some v -> 
                        isChosen.[i] <- true; 
                        results.[i] <- v
                )) |> ignore         
                                                                                      
                let mutable outputLength = 0                
                for i = 0 to lastInputIndex do 
                    if isChosen.[i] then 
                        outputLength <- outputLength + 1
                        
                let output = zeroCreate<'U> outputLength
                let mutable curr = 0
                for i = 0 to lastInputIndex do 
                    if isChosen.[i] then 
                        output.[curr] <- results.[i]
                        curr <- curr + 1
                output
                
            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]                         
            let collect (f : 'T -> array<'U>)  (input : array<'T>) : array<'U>=
                let inputLength = input.Length
                let result = zeroCreate<array<'U>> inputLength
                Parallel.For(0, inputLength, 
                    (fun i -> result.[i] <- f input.[i])) |> ignore
                concatArrays result
                
            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
            let map (f: 'T -> 'U) (input : array<'T>) : array<'U>=
                let inputLength = input.Length
                let result = zeroCreate<'U> inputLength
                Parallel.For(0, inputLength, fun i ->
                    result.[i] <- f input.[i]) |> ignore
                result
                
            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
            let mapi f (input: array<'T>) =
                let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
                let inputLength = input.Length
                let result = zeroCreate inputLength 
                Parallel.For(0, inputLength, fun i ->
                    result.[i] <- f.Invoke (i, input.[i])) |> ignore
                result
                
            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]               
            let iter f (input : array<'T>) =
                Parallel.For (0, input.Length, fun i -> f input.[i]) |> ignore  
                
            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]               
            let iteri f (input : array<'T>) =
                let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
                Parallel.For (0, input.Length, fun i -> f.Invoke(i, input.[i])) |> ignore        
                
            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]               
            let init count f =
                let result = zeroCreate count
                Parallel.For (0, count, fun i -> result.[i] <- f i) |> ignore
                result
                
            [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
            let partition predicate (array : array<'T>) =
                let inputLength = array.Length
                let lastInputIndex = inputLength - 1

                let isTrue = zeroCreate<bool> inputLength
                Parallel.For(0, inputLength, 
                    fun i -> isTrue.[i] <- predicate array.[i]
                    ) |> ignore
                
                let mutable trueLength = 0
                for i in 0 .. lastInputIndex do
                    if isTrue.[i] then trueLength <- trueLength + 1
                
                let trueResult = zeroCreate<'T> trueLength
                let falseResult = zeroCreate<'T> (inputLength - trueLength)
                let mutable iTrue = 0
                let mutable iFalse = 0
                for i = 0 to lastInputIndex do
                    if isTrue.[i] then
                        trueResult.[iTrue] <- array.[i]
                        iTrue <- iTrue + 1
                    else
                        falseResult.[iFalse] <- array.[i]
                        iFalse <- iFalse + 1

                (trueResult, falseResult)
#endif               