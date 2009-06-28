// (c) Microsoft Corporation 2005-2009. 

#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities
#else
namespace Microsoft.FSharp.Collections
#endif

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ResizeArray = 

    let length (arr: ResizeArray<'a>) =  arr.Count
    let get (arr: ResizeArray<'a>) (n:int) =  arr.[n]
    let set (arr: ResizeArray<'a>) (n:int) (x:'a) =  arr.[n] <- x
    let create  (n:int) x = new ResizeArray<_> (seq { for i in 1 .. n -> x })
    let init (n:int) (f: int -> 'a) =  new ResizeArray<_> (seq { for i in 0 .. n-1 -> f i })

    let blit (arr1:ResizeArray<'a>) start1 (arr2: ResizeArray<'a>) start2 len =
        if start1 < 0 then invalidArg "start1" "index must be positive"
        if start2 < 0 then invalidArg "start2" "index must be positive"
        if len < 0 then invalidArg "len" "length must be positive"
        if start1 + len > length arr1 then invalidArg "start1" "(start1+len) out of range"
        if start2 + len > length arr2 then invalidArg "start2" "(start2+len) out of range"
        for i = 0 to len - 1 do 
            arr2.[start2+i] <- arr1.[start1 + i]

    let concat (arrs:ResizeArray<'a> list) = new ResizeArray<_> (seq { for arr in arrs do for x in arr do yield x })
    let append (arr1:ResizeArray<'a>) (arr2:ResizeArray<'a>) = concat [arr1; arr2]

    let sub (arr:ResizeArray<'a>) start len =
        if start < 0 then invalidArg "start" "index must be positive"
        if len < 0 then invalidArg "len" "length must be positive"
        if start + len > length arr then invalidArg "len" "length must be positive"
        new ResizeArray<_> (seq { for i in start .. start+len-1 -> arr.[i] })

    let fill (arr:ResizeArray<'a>) (start:int) (len:int) (x:'a) =
        if start < 0 then invalidArg "start" "index must be positive"
        if len < 0 then invalidArg "len" "length must be positive"
        if start + len > length arr then invalidArg "len" "length must be positive"
        for i = start to start + len - 1 do 
            arr.[i] <- x

    let copy      (arr:ResizeArray<'a>) = new ResizeArray<_>(arr)

    let to_list (arr:ResizeArray<_>) =
        let mutable res = []
        for i = length arr - 1 downto 0 do
            res <- arr.[i] :: res
        res

    let of_list (l: _ list) =
        let len = l.Length
        let res = new ResizeArray<_>(len)
        let rec add = function
          | [] -> ()
          | e::l -> res.Add(e); add l
        add l
        res

    let iter f (arr:ResizeArray<_>) = 
        for i = 0 to arr.Count - 1 do
            f arr.[i]

    let map f (arr:ResizeArray<_>) =
        let len = length arr
        let res = new ResizeArray<_>(len)
        for i = 0 to len - 1 do
            res.Add(f arr.[i])
        res

    let mapi f (arr:ResizeArray<_>) =
        let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
        let len = length arr
        let res = new ResizeArray<_>(len)
        for i = 0 to len - 1 do
            res.Add(f.Invoke(i, arr.[i]))
        res
        
    let iteri f (arr:ResizeArray<_>) =
        let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
        for i = 0 to arr.Count - 1 do
            f.Invoke(i, arr.[i])

    let exists (f: 'a -> bool) (arr:ResizeArray<'a>) =
        let len = length arr 
        let rec loop i = i < len && (f arr.[i] || loop (i+1))
        loop 0

    let for_all f (arr:ResizeArray<_>) =
        let len = length arr
        let rec loop i = i >= len || (f arr.[i] && loop (i+1))
        loop 0

    let indexNotFound() = raise (new System.Collections.Generic.KeyNotFoundException("An index satisfying the predicate was not found in the collection"))

    let find f (arr:ResizeArray<_>) = 
        let rec loop i = 
            if i >= length arr then indexNotFound()
            elif f arr.[i] then arr.[i]
            else loop (i+1)
        loop 0

    let first f (arr:ResizeArray<_>) =
        let rec loop i = 
            if i >= length arr then None else
            match f arr.[i] with 
            | None -> loop(i+1)
            | res -> res
        loop 0

    let tryfind f (arr:ResizeArray<_>) = 
        let rec loop i = 
            if i >= length arr then None
            elif f arr.[i] then Some arr.[i]
            else loop (i+1)
        loop 0

    let iter2 f (arr1:ResizeArray<'a>) (arr2:ResizeArray<'b>) = 
        let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
        let len1 = length arr1
        if len1 <> length arr2 then invalidArg "arr2" "the arrays have different lengths"
        for i = 0 to len1 - 1 do 
            f.Invoke(arr1.[i], arr2.[i])

    let map2 f (arr1:ResizeArray<'a>) (arr2:ResizeArray<'b>) = 
        let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
        let len1 = length arr1
        if len1 <> length arr2 then invalidArg "arr2" "the arrays have different lengths"
        let res = new ResizeArray<_>(len1)
        for i = 0 to len1 - 1 do
            res.Add(f.Invoke(arr1.[i], arr2.[i]))
        res

    let choose f (arr:ResizeArray<_>) = 
        let res = new ResizeArray<_>() 
        for i = 0 to length arr - 1 do
            match f arr.[i] with 
            | None -> ()
            | Some b -> res.Add(b)
        res

    let filter f (arr:ResizeArray<_>) = 
        let res = new ResizeArray<_>() 
        for i = 0 to length arr - 1 do 
            let x = arr.[i] 
            if f x then res.Add(x)
        res

    let partition f (arr:ResizeArray<_>) = 
      let res1 = new ResizeArray<_>()
      let res2 = new ResizeArray<_>()
      for i = 0 to length arr - 1 do 
          let x = arr.[i] 
          if f x then res1.Add(x) else res2.Add(x)
      res1, res2

    let rev (arr:ResizeArray<_>) = 
      let len = length arr 
      let res = new ResizeArray<_>(len)
      for i = len - 1 downto 0 do 
          res.Add(arr.[i])
      res

    let fold_right (f : 'a -> 'b -> 'b) (arr:ResizeArray<'a>) (acc: 'b) =
        let mutable res = acc 
        let len = length arr 
        for i = len - 1 downto 0 do 
            res <- f (get arr i) res
        res

    let fold_left (f : 'a -> 'b -> 'a) (acc: 'a) (arr:ResizeArray<'b>) =
        let mutable res = acc 
        let len = length arr 
        for i = 0 to len - 1 do 
            res <- f res (get arr i)
        res

    let to_array (arr:ResizeArray<'a>) = arr.ToArray()
    let of_array (arr :  'a array) = new ResizeArray<_>(arr)
    let to_seq (arr : ResizeArray<'a>) = arr :> seq<_>

    let sort f (arr :  ResizeArray<'a>) = arr.Sort (System.Comparison(f))
    let sort_by f (arr :  ResizeArray<'a>) = arr.Sort (System.Comparison(fun x y -> compare (f x) (f y)))


    let exists2 f (arr1:ResizeArray<_>) (arr2:ResizeArray<_>) =
        let len1 = length arr1
        if len1 <> length arr2 then invalidArg "arr2" "the arrays have different lengths"
        let rec loop i = i < len1 && (f arr1.[i] arr2.[i] || loop (i+1))
        loop 0

    let find_index f (arr:ResizeArray<_>) =
        let rec go n = if n >= length arr then indexNotFound() elif f arr.[n] then n else go (n+1)
        go 0

    let find_indexi f (arr: ResizeArray<_>) =
        let rec go n = if n >= length arr then indexNotFound() elif f n arr.[n] then n else go (n+1)
        go 0

    let fold_sub_left f acc (arr: ResizeArray<_>) start fin = 
        let mutable res = acc
        for i = start to fin do
            res <- f res arr.[i] 
        res

    let fold_sub_right f (arr: ResizeArray<_>) start fin acc = 
        let mutable res = acc 
        for i = fin downto start do
            res <- f arr.[i] res
        res

    let reduce_left f (arr : ResizeArray<_>) =
        let arrn = length arr
        if arrn = 0 then invalidArg "arr" "the input array may not be empty"
        else fold_sub_left f arr.[0] arr 1 (arrn - 1)
        
    let reduce_right f (arr: ResizeArray<_>) = 
        let arrn = length arr
        if arrn = 0 then invalidArg "arr" "the input array may not be empty"
        else fold_sub_right f arr 0 (arrn - 2) arr.[arrn - 1]

    let fold_left2 f (acc: 'a) (arr1:ResizeArray<'a1>) (arr2:ResizeArray<'a2>) =
        let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
        let mutable res = acc 
        let len = length arr1
        if len <> length arr2 then invalidArg "arr2" "the arrays have different lengths"
        for i = 0 to len - 1 do
            res <- f.Invoke(res,arr1.[i],arr2.[i])
        res

    let fold_right2 f (arr1:ResizeArray<'a1>) (arr2: ResizeArray<'a2>) (acc: 'b) =
        let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
        let mutable res = acc 
        let len = length arr1
        if len <> length arr2 then invalidArg "arr2" "the arrays have different lengths"
        for i = len - 1 downto 0 do 
            res <- f.Invoke(arr1.[i],arr2.[i],res)
        res

    let for_all2 f (arr1: ResizeArray<_>) (arr2: ResizeArray<_>) = 
        let len1 = length arr1
        if len1 <> length arr2 then invalidArg "arr2" "the arrays have different lengths"
        let rec loop i = i >= len1 || (f arr1.[i] arr2.[i] && loop (i+1))
        loop 0
        
    let is_empty (arr: ResizeArray<_>) = length (arr:ResizeArray<_>) = 0
    
    let iteri2 f (arr1: ResizeArray<'a>) (arr2: ResizeArray<'b>) =
        let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
        let len1 = length arr1
        if len1 <> length arr2 then invalidArg "arr2" "the arrays have different lengths"
        for i = 0 to len1 - 1 do 
            f.Invoke(i,arr1.[i], arr2.[i])

    let mapi2 (f: int -> 'a -> 'b -> 'c) (arr1: ResizeArray<'a>) (arr2: ResizeArray<'b>) = 
        let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
        let len1 = length arr1
        if len1 <> length arr2 then invalidArg "arr2" "the arrays have different lengths"
        init len1 (fun i -> f.Invoke(i, arr1.[i], arr2.[i]))

    let scan_sub_right f (arr: ResizeArray<'a>) start fin acc = 
        let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
        let mutable state = acc
        let res = create (2+fin-start) acc
        for i = fin downto start do
            state <- f.Invoke(arr.[i], state)
            res.[i - start] <- state
        res

    let scan_sub_left f  acc (arr : ResizeArray<'a>) start fin = 
        let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
        let mutable state = acc
        let res = create (fin-start+2) acc
        for i = start to fin do
            state <- f.Invoke(state, arr.[i])
            res.[i - start+1] <- state
        res

    let scan_left f acc (arr : ResizeArray<'a>) = 
        let arrn = length arr
        scan_sub_left f acc arr 0 (arrn - 1)

    let scan_right f (arr : ResizeArray<'a>) acc = 
        let arrn = length arr
        scan_sub_right f arr 0 (arrn - 1) acc

    let singleton x =
        let res = new ResizeArray<_>(1)
        res.Add(x)
        res

    let tryfind_index f (arr: ResizeArray<'a>) = 
        let rec go n = if n >= length arr then None elif f arr.[n] then Some n else go (n+1)
        go 0
        
    let tryfind_indexi f (arr: ResizeArray<'a>) = 
        let rec go n = if n >= length arr then None elif f n arr.[n] then Some n else go (n+1)
        go 0
    
    let zip (arr1:ResizeArray<_>) (arr2: ResizeArray<_>) = 
        let len1 = length arr1 
        if len1 <> length arr2 then invalidArg "arr2" "the arrays have different lengths"
        init len1 (fun i -> arr1.[i], arr2.[i])

    let unzip (arr:ResizeArray<_>) = 
        let len = length arr
        let res1 = new ResizeArray<_>(len)
        let res2 = new ResizeArray<_>(len)
        for i = 0 to len - 1 do 
            let x,y = arr.[i] 
            res1.Add(x)
            res2.Add(y)
        res1,res2

    let combine (arr1:ResizeArray<'a>) (arr2:ResizeArray<'b>) = zip arr1 arr2
    let split (arr:ResizeArray<_>) = unzip arr
