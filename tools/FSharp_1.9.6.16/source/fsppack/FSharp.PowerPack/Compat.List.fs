// (c) Microsoft Corporation 2005-2009. 

#nowarn "62"

#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities
#else
namespace Microsoft.FSharp.Compatibility
#endif


open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open System.Collections.Generic
open System.Diagnostics

module List = 

    let invalidArg arg msg = raise (new System.ArgumentException((msg:string),(arg:string)))        

    let nonempty x = match x with [] -> false | _ -> true
    let rec contains x l = match l with [] -> false | h::t -> x = h || contains x t
    let mem x l = contains x l
    let rec memq x l = match l with [] -> false | h::t -> LanguagePrimitives.PhysicalEquality x h || memq x t

    let rec rev_map2_acc (f:OptimizedClosures.FastFunc2<_,_,_>) l1 l2 acc =
        match l1,l2 with 
        | [],[] -> acc
        | h1::t1, h2::t2 -> rev_map2_acc f t1 t2 (f.Invoke(h1,h2) :: acc)
        | _ -> invalidArg "l2" "the lists have different lengths"

    let rev_map2 f l1 l2 = 
        let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
        rev_map2_acc f l1 l2 []

    let rec rev_append l1 l2 = 
        match l1 with 
        | [] -> l2
        | h::t -> rev_append t (h::l2)


    let rec rev_map_acc f l acc =
        match l with 
        | [] -> acc
        | h::t -> rev_map_acc f t (f h :: acc)

    let rev_map f l = rev_map_acc f l []

    let indexNotFound() = raise (new System.Collections.Generic.KeyNotFoundException("An index satisfying the predicate was not found in the collection"))

    let rec assoc x l = 
        match l with 
        | [] -> indexNotFound()
        | ((h,r)::t) -> if x = h then r else assoc x t

    let rec try_assoc x l = 
        match l with 
        | [] -> None
        | ((h,r)::t) -> if x = h then Some(r) else try_assoc x t

    let rec mem_assoc x l = 
        match l with 
        | [] -> false
        | ((h,r)::t) -> x = h || mem_assoc x t

    let rec remove_assoc x l = 
        match l with 
        | [] -> []
        | (((h,r) as p) ::t) -> if x = h then t else p:: remove_assoc x t

    let rec assq x l = 
        match l with 
        | [] -> indexNotFound()
        | ((h,r)::t) -> if LanguagePrimitives.PhysicalEquality x h then r else assq x t

    let rec try_assq x l = 
        match l with 
        | [] -> None
        | ((h,r)::t) -> if LanguagePrimitives.PhysicalEquality x h then Some(r) else try_assq x t

    let rec mem_assq x l = 
        match l with 
        | [] -> false
        | ((h,r)::t) -> LanguagePrimitives.PhysicalEquality x h || mem_assq x t

    let rec remove_assq x l = 
        match l with 
        | [] -> []
        | (((h,r) as p) ::t) -> if LanguagePrimitives.PhysicalEquality x h then t else p:: remove_assq x t

    let scanReduce f l = 
        match l with 
        | [] -> invalidArg "l" "the input list is empty"
        | (h::t) -> List.scan f h t
    let scan1_left f l = scanReduce f l

    let scanArraySubRight<'a,'b> (f:OptimizedClosures.FastFunc2<'a,'b,'b>) (arr:_[]) start fin initState = 
        let mutable state = initState in 
        let mutable res = [state] in 
        for i = fin downto start do
            state <- f.Invoke(arr.[i], state);
            res <- state :: res
        res


    let scanReduceBack f l = 
        match l with 
        | [] -> invalidArg "l" "the input list is empty"
        | _ -> 
            let f = OptimizedClosures.FastFunc2<_,_,_>.Adapt(f)
            let arr = Array.of_list l in 
            let arrn = Array.length arr in
            scanArraySubRight f arr 0 (arrn - 2) arr.[arrn - 1]
    let scan1_right f l = scanReduceBack f l


    let find_indexi f list = 
        let rec loop n = function[] -> indexNotFound() | h::t -> if f n h then n else loop (n+1) t
        loop 0 list


    let tryfind_indexi f list = 
        let rec loop n = function [] -> None | h::t -> if f n h then Some n else loop (n+1) t
        loop 0 list
