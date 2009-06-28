//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Primitives.Basics 

open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core.Operators
open System.Diagnostics.CodeAnalysis                                    
open System.Collections.Generic
open System.Runtime.InteropServices


module internal List = 

    let arrayZeroCreate (n:int) = (# "newarr !0" type ('a) n : 'a array #)

    [<SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>]      
    let nonempty x = match x with [] -> false | _ -> true

    let rec iter f x = match x with [] -> () | (h::t) -> f h; iter f t

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let inline setFreshConsTail cons t = cons.(::).1 <- t
    let inline freshConsNoTail h = h :: (# "ldnull" : 'a list #)


    let rec mapToFreshConsTail cons f x = 
        match x with
        | [] -> 
            setFreshConsTail cons [];
        | (h::t) -> 
            let cons2 = freshConsNoTail (f h)
            setFreshConsTail cons cons2;
            mapToFreshConsTail cons2 f t

    let map f x = 
        match x with
        | [] -> []
        | [h] -> [f h]
        | (h::t) -> 
            let cons = freshConsNoTail (f h)
            mapToFreshConsTail cons f t
            cons

    let rec mapiToFreshConsTail cons (f:OptimizedClosures.FastFunc2<_,_,_>) x i = 
        match x with
        | [] -> 
            setFreshConsTail cons [];
        | (h::t) -> 
            let cons2 = freshConsNoTail (f.Invoke(i,h))
            setFreshConsTail cons cons2;
            mapiToFreshConsTail cons2 f t (i+1)

    let mapi f x = 
        match x with
        | [] -> []
        | [h] -> [f 0 h]
        | (h::t) -> 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let cons = freshConsNoTail (f.Invoke(0,h))
            mapiToFreshConsTail cons f t 1
            cons

    let rec map2ToFreshConsTail cons (f:OptimizedClosures.FastFunc2<_,_,_>) xs1 xs2 = 
        match xs1,xs2 with
        | [],[] -> 
            setFreshConsTail cons [];
        | (h1::t1),(h2::t2) -> 
            let cons2 = freshConsNoTail (f.Invoke(h1,h2))
            setFreshConsTail cons cons2;
            map2ToFreshConsTail cons2 f t1 t2
        | _ -> invalidArg "xs2" "the lists had different lengths"

    let map2 f xs1 xs2 = 
        match xs1,xs2 with
        | [],[] -> []
        | (h1::t1),(h2::t2) -> 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let cons = freshConsNoTail (f.Invoke(h1,h2))
            map2ToFreshConsTail cons f t1 t2
            cons
        | _ -> invalidArg "xs2" "the lists had different lengths"

    let rec forall f xs1 = 
        match xs1 with 
        | [] -> true
        | (h1::t1) -> f h1 && forall f t1

    let rec exists f xs1 = 
        match xs1 with 
        | [] -> false
        | (h1::t1) -> f h1 || exists f t1

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec revAcc xs acc = 
        match xs with 
        | [] -> acc
        | h::t -> revAcc t (h::acc)

    let rev xs = 
        match xs with 
        | [] -> xs
        | [h] -> xs
        | h1::h2::t -> revAcc t [h2;h1]

    // return the last cons it the chain
    let rec appendToFreshConsTail cons xs = 
        match xs with 
        | [] -> 
            setFreshConsTail cons []
            cons
        | h::t -> 
            let cons2 = freshConsNoTail h
            setFreshConsTail cons cons2
            appendToFreshConsTail cons2 t

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec collectToFreshConsTail (f:'a -> 'b list) (list:'a list) cons = 
        match list with 
        | [] -> 
            setFreshConsTail cons []
        | h::t -> 
            collectToFreshConsTail f t (appendToFreshConsTail cons (f h))

    let rec collect (f:'a -> 'b list) (list:'a list) = 
        match list with
        | [] -> []
        | [h] -> f h
        | _ ->
            let cons = freshConsNoTail (Unchecked.defaultof<'b>)
            collectToFreshConsTail f list cons
            cons.Tail 

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec filterToFreshConsTail cons f l = 
        match l with 
        | [] -> 
            setFreshConsTail cons [];
        | h::t -> 
            if f h then 
                let cons2 = freshConsNoTail h 
                setFreshConsTail cons cons2;
                filterToFreshConsTail cons2 f t
            else 
                filterToFreshConsTail cons f t
      
    let rec filter f l = 
        match l with 
        | [] -> []
        | [h] -> if f h then l else []
        | h::t -> 
            if f h then   
                let cons = freshConsNoTail h 
                filterToFreshConsTail cons f t; 
                cons
            else 
                filter f t

    let iteri f x = 
        let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
        let rec loop n x = match x with [] -> () | (h::t) -> f.Invoke(n,h); loop (n+1) t
        loop 0 x

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec concatToFreshConsTail cons h1 l = 
        match l with 
        | [] -> setFreshConsTail cons h1
        | h2::t -> concatToFreshConsTail (appendToFreshConsTail cons h1) h2 t
      
    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec concatToEmpty l = 
        match l with 
        | [] -> []
        | []::t -> concatToEmpty t 
        | (h::t1)::tt2 -> 
            let res = freshConsNoTail h
            concatToFreshConsTail res t1 tt2;
            res

    let seqToList (e : IEnumerable<'T>) = 
        match e with 
        | :? list<'T> as l -> l
        | _ -> 
            use ie = e.GetEnumerator()
            let mutable res = [] 
            while ie.MoveNext() do
                res <- ie.Current :: res
            rev res

    let concat (l : seq<_>) = 
        match seqToList l with 
        | [] -> []
        | [h] -> h
        | [h1;h2] -> h1 @ h2
        | l -> concatToEmpty l

    let rec initToFreshConsTail cons i n f = 
        if i < n then 
          let cons2 = freshConsNoTail (f i)
          setFreshConsTail cons cons2;
          initToFreshConsTail cons2 (i+1) n f 
        else 
          setFreshConsTail cons []
           
      
    let init n f = 
        if n < 0 then  invalidArg "n" "the length may not be negative"
        if n = 0 then [] 
        else 
            let res = freshConsNoTail (f 0)
            initToFreshConsTail res 1 n f
            res

     
      
    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec partitionToFreshConsTails consL consR p l = 
        match l with 
        | [] -> 
            setFreshConsTail consL [];
            setFreshConsTail consR [];
            
        | h::t -> 
            let cons' = freshConsNoTail h
            if p h then 
                setFreshConsTail consL cons';
                partitionToFreshConsTails cons' consR p t
            else 
                setFreshConsTail consR cons';
                partitionToFreshConsTails consL cons' p t
      
    let rec partitionToFreshConsTailLeft consL p l = 
        match l with 
        | [] -> 
            setFreshConsTail consL [];
            []
        | h::t -> 
            let cons' = freshConsNoTail h 
            if p h then 
                setFreshConsTail consL cons';
                partitionToFreshConsTailLeft cons'  p t
            else 
                partitionToFreshConsTails consL cons' p t; 
                cons'

    let rec partitionToFreshConsTailRight consR p l = 
        match l with 
        | [] -> 
            setFreshConsTail consR [];
            []
        | h::t -> 
            let cons' = freshConsNoTail h 
            if p h then 
                partitionToFreshConsTails cons' consR p t; 
                cons'
            else 
                setFreshConsTail consR cons';
                partitionToFreshConsTailRight cons' p t

    let partition p l = 
        match l with 
        | [] -> [],[]
        | [h] -> if p h then l,[] else [],l
        | h::t -> 
            let cons = freshConsNoTail h 
            if p h 
            then cons, (partitionToFreshConsTailLeft cons p t)
            else (partitionToFreshConsTailRight cons p t), cons
           
    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec unzipToFreshConsTail cons1a cons1b x = 
        match x with 
        | [] -> 
            setFreshConsTail cons1a []
            setFreshConsTail cons1b []
        | ((h1,h2)::t) -> 
            let cons2a = freshConsNoTail h1
            let cons2b = freshConsNoTail h2
            setFreshConsTail cons1a cons2a;
            setFreshConsTail cons1b cons2b;
            unzipToFreshConsTail cons2a cons2b t

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let unzip x = 
        match x with 
        | [] -> 
            [],[]
        | ((h1,h2)::t) -> 
            let res1a = freshConsNoTail h1
            let res1b = freshConsNoTail h2
            unzipToFreshConsTail res1a res1b t; 
            res1a,res1b

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec unzip3ToFreshConsTail cons1a cons1b cons1c x = 
        match x with 
        | [] -> 
            setFreshConsTail cons1a [];
            setFreshConsTail cons1b [];
            setFreshConsTail cons1c [];
        | ((h1,h2,h3)::t) -> 
            let cons2a = freshConsNoTail h1
            let cons2b = freshConsNoTail h2
            let cons2c = freshConsNoTail h3
            setFreshConsTail cons1a cons2a;
            setFreshConsTail cons1b cons2b;
            setFreshConsTail cons1c cons2c;
            unzip3ToFreshConsTail cons2a cons2b cons2c t

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let unzip3 x = 
        match x with 
        | [] -> 
            [],[],[]
        | ((h1,h2,h3)::t) -> 
            let res1a = freshConsNoTail h1
            let res1b = freshConsNoTail h2
            let res1c = freshConsNoTail h3 
            unzip3ToFreshConsTail res1a res1b res1c t; 
            res1a,res1b,res1c

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec zipToFreshConsTail cons xs1 xs2 = 
        match xs1,xs2 with 
        | [],[] -> 
            setFreshConsTail cons []
        | (h1::t1),(h2::t2) -> 
            let cons2 = freshConsNoTail (h1,h2)
            setFreshConsTail cons cons2;
            zipToFreshConsTail cons2 t1 t2
        | _ -> 
            invalidArg "xs2" "the input lists had different lengths"

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let zip  xs1 xs2 = 
        match xs1,xs2 with 
        | [],[] -> []
        | (h1::t1),(h2::t2) -> 
            let res = freshConsNoTail (h1,h2)
            zipToFreshConsTail res t1 t2; 
            res
        | _ -> 
            invalidArg "xs2" "the input lists had different lengths"

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let rec zip3ToFreshConsTail cons xs1 xs2 xs3 = 
        match xs1,xs2,xs3 with 
        | [],[],[] -> 
            setFreshConsTail cons [];
        | (h1::t1),(h2::t2),(h3::t3) -> 
            let cons2 = freshConsNoTail (h1,h2,h3)
            setFreshConsTail cons cons2;
            zip3ToFreshConsTail cons2 t1 t2 t3
        | _ -> 
            invalidArg "xs1" "the input lists had different lengths"

    // optimized mutation-based implementation. This code is only valid in fslib, where mutation of private
    // tail cons cells is permitted in carefully written library code.
    let zip3 xs1 xs2 xs3 = 
        match xs1,xs2,xs3 with 
        | [],[],[] -> 
            []
        | (h1::t1),(h2::t2),(h3::t3) -> 
            let res = freshConsNoTail (h1,h2,h3) 
            zip3ToFreshConsTail res t1 t2 t3; 
            res
        | _ -> 
            invalidArg "xs1" "the input lists had different lengths"

    let to_array (l:'a list) =
        let len = l.Length 
        let res = (arrayZeroCreate len : 'a array) 
        let mutable lref = l 
        for i = 0 to len - 1 do 
            res.[i] <- lref.(::).0;
            lref <- lref.(::).1
        res

    let of_array (arr:'a array) =
        let len = arr.Length
        let mutable res = ([]: 'a list) 
        for i = len - 1 downto 0 do 
            res <- arr.[i] :: res
        res

    module StableSortImplementation =
        // Internal copy of stable sort
        let rec revAppend xs1 xs2 = 
            match xs1 with 
            | [] -> xs2
            | h::t -> revAppend t (h::xs2)
        let half x = x >>> 1 

        let rec merge cmp a b acc = 
            match a,b with 
            | [], a | a,[] -> revAppend acc a
            | x::a', y::b' -> if cmp x y > 0 then merge cmp a  b' (y::acc) else merge cmp a' b  (x::acc)

        let sort2 cmp x y = 
            if cmp x y > 0 then [y;x] else [x;y]

        let sort3 cmp x y z = 
            let cxy = cmp x y
            let cyz = cmp y z
            if cxy > 0 && cyz < 0 then 
                if cmp x z > 0 then [y;z;x] else [y;x;z]
            elif cxy < 0 && cyz > 0 then 
                if cmp x z > 0 then [z;x;y] else [x;z;y]
            elif cxy > 0 then 
                if cyz > 0 then  [z;y;x]
                else [y;z;x]
            else 
                if cyz > 0 then [z;x;y]
                else [x;y;z] 

        let trivial a = match a with [] | [_] -> true | _ -> false
            
        (* tail recursive using a ref *)

        let rec stableSortInner cmp la ar =
          if la < 4 then (* sort two || three new entries *)
            match !ar with 
             | x::y::b -> 
                  if la = 2 then ( ar := b; sort2 cmp x y )
                  else begin
                    match b with 
                    | z::c -> ( ar := c; sort3 cmp x y z )
                    | _ -> failwith "never" 
                  end
             | _ -> failwith "never"
          else (* divide *)
            let lb = half la
            let sb = stableSortInner cmp lb ar
            let sc = stableSortInner cmp (la - lb) ar
            merge cmp sb sc []

        let stableSort cmp (a: 'a list) = 
            if trivial a then a else
            let ar = ref a
            stableSortInner cmp a.Length ar
        
    let sortWith cmp a = StableSortImplementation.stableSort cmp a
    
module internal Array = 

    let inline zeroCreate (n:int) = (# "newarr !0" type ('a) n : 'a array #)

    let init (n:int) (f: int -> 'a) = 
        let arr = (zeroCreate n : 'a array)  
        for i = 0 to n - 1 do 
            arr.[i] <- f i
        arr

    let permute indexMap (arr : _[]) = 
        let res  = zeroCreate arr.Length
        let inv = zeroCreate arr.Length
        for i = 0 to arr.Length - 1 do 
            let j = indexMap i 
            if j < 0 or j >= arr.Length then invalidArg "indexMap" "the function did not compute a permutation" 
            res.[j] <- arr.[i]
            inv.[j] <- 1uy
        for i = 0 to arr.Length - 1 do 
            if inv.[i] <> 1uy then invalidArg "indexMap" "the function did not compute a permutation"
        res


