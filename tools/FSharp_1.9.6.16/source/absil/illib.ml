// (c) Microsoft Corporation. All rights reserved(

#light

module Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

open System.Collections
open System.Collections.Generic
open Internal.Utilities
open Internal.Utilities.Pervasives

let notlazy v = Lazy.CreateFromValue v

let isSome x = match x with None -> false | _ -> true
let isNone x = match x with None -> true | _ -> false
let isNil x = match x with [] -> true | _ -> false
let nonNil x = match x with [] -> false | _ -> true
let isNonNull (x : 'a) = match (x :> obj) with null -> false | _ -> true
let nonNull msg x = if isNonNull x then x else failwith ("null: " ^ msg) 
let (===) x y = (x == y)

//-------------------------------------------------------------------------
// Library: arrays,lists,options
//-------------------------------------------------------------------------

module Array = 

    let mapq f inp =
        match inp with
        | [| |] -> inp
        | _ -> 
            let res = Array.map f inp 
            let len = inp.Length 
            let mutable eq = true
            let mutable i = 0 
            while eq && i < len do 
                if not (inp.[i] === res.[i]) then eq <- false;
                i <- i + 1
            if eq then inp else res

    let for_all2 f (arr1:'a array) (arr2:'a array) =
        let len1 = arr1.Length 
        let len2 = arr2.Length 
        if len1 <> len2 then invalid_arg "Array.for_all2"
        let rec loop i = (i >= len1) || (f arr1.[i] arr2.[i] && loop (i+1))
        loop 0

    let mapfold f s l = 
        let mutable acc = s
        let n = Array.length l
        let mutable res = Array.zeroCreate n
        for i = 0 to n - 1 do
            let h',s' = f acc l.[i]
            res.[i] <- h';
            acc <- s'
        res, acc

    // REVIEW: systematically eliminate fmap/mapfold duplication
    let fmap f s l = 
        let mutable acc = s
        let n = Array.length l
        let mutable res = Array.zeroCreate n
        for i = 0 to n - 1 do
            let s',h' = f acc l.[i]
            res.[i] <- h'
            acc <- s'
        acc, res

    let order eltOrder (xs:array<_>) (ys:array<_>) = 
        let c = compare xs.Length ys.Length 
        if c <> 0 then c else
        let rec loop i = 
            if i >= xs.Length then 0 else
            let c = eltOrder xs.[i] ys.[i]
            if c <> 0 then c else
            loop (i+1)
        loop 0

    let existsOne p l = 
        let rec array_forall_aux p l n =
          (n >= Array.length l) || (p l.[n] && array_forall_aux p l (n+1))

        let rec array_exists_one_aux p l n =
            (n < Array.length l) && 
            (if p l.[n] then array_forall_aux (fun x -> not (p x)) l (n+1) else array_exists_one_aux p l (n+1))
          
        array_exists_one_aux p l 0

    
module Option = 
    let mapfold f s opt = 
        match opt with 
        | None -> None,s 
        | Some x -> let x',s' = f s x in Some x',s'

    let otherwise opt dflt = 
        match opt with 
        | None -> dflt 
        | Some x -> x

    // REVIEW: systematically eliminate fmap/mapfold duplication
    let fmap f z l = 
        match l with 
        | None   -> z,None
        | Some x -> let z,x = f z x
                    z,Some x

    let fold f z x = 
        match x with 
        | None -> z 
        | Some x -> f z x


let (+??) opt dflt = match opt with None -> dflt() | Some x -> opt
let (+?) opt dflt = match opt with None -> dflt() | Some x -> x

let the = function None -> failwith "the"  | Some x -> x

module List = 

    let existsi f xs = 
       let rec loop i xs = match xs with [] -> false | h::t -> f i h || loop (i+1) t
       loop 0 xs
    
    let lengthsEqAndForall2 p l1 l2 = 
        List.length l1 = List.length l2 &&
        List.forall2 p l1 l2

    let rec findi n f l = 
        match l with 
        | [] -> None
        | h::t -> if f h then Some (h,n) else findi (n+1) f t

    let chop n l = 
        if n = List.length l then (l,[]) else // avoids allocation unless necessary 
        let rec loop n l acc = 
            if n <= 0 then (List.rev acc,l) else 
            match l with 
            | [] -> failwith "List.chop: overchop"
            | (h::t) -> loop (n-1) t (h::acc) 
        loop n l [] 

    let take n l = 
        if n = List.length l then l else 
        let rec loop acc n l = 
            match l with
            | []    -> List.rev acc
            | x::xs -> if n<=0 then List.rev acc else loop (x::acc) (n-1) xs

        loop [] n l

    let rec drop n l = 
        match l with 
        | []    -> []
        | x::xs -> if n=0 then l else drop (n-1) xs


    let splitChoose select l =
        let rec ch acc1 acc2 l = 
            match l with 
            | [] -> List.rev acc1,List.rev acc2
            | x::xs -> 
                match select x with
                | Choice1Of2 sx -> ch (sx::acc1) acc2 xs
                | Choice2Of2 sx -> ch acc1 (sx::acc2) xs

        ch [] [] l

    let mapq (f: 'a -> 'a) inp =
        assert not (typeof<'a>.IsValueType) 
        match inp with
        | [] -> inp
        | _ -> 
            let res = List.map f inp 
            let rec check l1 l2 = 
                match l1,l2 with 
                | h1::t1,h2::t2 -> 
                    System.Runtime.CompilerServices.RuntimeHelpers.Equals(h1,h2) && check t1 t2
                | _ -> true
            if check inp res then inp else res
        
    let frontAndBack l = 
        let rec loop acc l = 
            match l with
            | [] -> 
                System.Diagnostics.Debug.Assert(false, "empty list")
                invalidArg "l" "empty list" 
            | [h] -> List.rev acc,h
            | h::t -> loop  (h::acc) t
        loop [] l


    let headAndTail l =
        match l with 
        | [] -> 
            System.Diagnostics.Debug.Assert(false, "empty list")
            failwith "List.headAndTail"
        | h::t -> h,t

    let unzip4 l = 
        let a,b,cd = List.unzip3 (List.map (fun (x,y,z,w) -> (x,y,(z,w))) l)
        let c,d = List.unzip cd
        a,b,c,d

    let rec iter3 f l1 l2 l3 = 
        match l1,l2,l3 with 
        | h1::t1, h2::t2, h3::t3 -> f h1 h2 h3; iter3 f t1 t2 t3
        | [], [], [] -> ()
        | _ -> failwith "iter3"

    /// warning: not tail recursive 
    let rec takeUntil p l = 
      match l with
      | []    -> [],[]
      | x::xs -> if p x then [],l else let a,b = takeUntil p xs in x::a,b

    let rec order eltOrder xs ys =
        match xs,ys with
        | [],[]       ->  0
        | [],ys       -> -1
        | xs,[]       ->  1
        | x::xs,y::ys -> let cxy = eltOrder x y
                         if cxy=0 then order eltOrder xs ys else cxy


    let rec last l = match l with [] -> failwith "last" | [h] -> h | h::t -> last t

    let replicate x n = 
        Array.to_list (Array.create x n)

    let range n m = [ n .. m ]


    // must be tail recursive 
    let mapfold f s l = 
        // microbenchmark suggested this implementation is faster than the simpler recursive one, and this function is called a lot
        let mutable s = s
        let mutable r = []
        let mutable l = l
        let mutable finished = false
        while not finished do
          match l with
          | x::xs -> let x',s' = f s x
                     s <- s'
                     r <- x' :: r
                     l <- xs
          | _ -> finished <- true
        List.rev r, s

    let mapNth n f xs =
        let rec mn i = function
          | []    -> []
          | x::xs -> if i=n then f x::xs else x::mn (i+1) xs
       
        mn 0 xs

    let rec until p l = match l with [] -> [] | h::t -> if p h then [] else h :: until p t 

    let count pred xs = List.fold (fun n x -> if pred x then n+1 else n) 0 xs

    let rec private repeatA n x acc = if n <= 0 then acc else repeatA (n-1) x (x::acc)
    let repeat n x = repeatA n x []

    (* WARNING: not tail-recursive *)
    let mapHeadTail fhead ftail = function
      | []    -> []
      | [x]   -> [fhead x]
      | x::xs -> fhead x :: List.map ftail xs

    let collectFold f s l = 
      let l, s = mapfold f s l
      List.concat l, s

    let singleton x = [x]

    // note: must be tail-recursive 
    let rec private fmapA f z l acc =
      match l with
      | []    -> z,List.rev acc
      | x::xs -> let z,x = f z x
                 fmapA f z xs (x::acc)
                 
    // note: must be tail-recursive 
    // REVIEW: systematically eliminate fmap/mapfold duplication
    let fmap f z l = fmapA f z l []

    let collect2 f xs ys = List.concat (List.map2 f xs ys)

    let iterSquared f xss = xss |> List.iter (List.iter f)
    let collectSquared f xss = xss |> List.collect (List.collect f)
    let mapSquared f xss = xss |> List.map (List.map f)
    let mapfoldSquared f xss = xss |> mapfold (mapfold f)
    let forallSquared f xss = xss |> List.forall (List.forall f)
    let mapiSquared f xss = xss |> List.mapi (fun i xs -> xs |> List.mapi (fun j x -> f i j x))
    let existsSquared f xss = xss |> List.exists (fun xs -> xs |> List.exists (fun x -> f x))

module String = 
    let order (a:string) (b:string) = Operators.compare a b
   
    let isUpper (s:string) = 
        s.Length >= 1 && System.Char.IsUpper s.[0] && not (System.Char.IsLower s.[0])
        

    let tryDropPrefix s t = 
        let lens = String.length s
        let lent = String.length t
        if (lens >= lent && (String.sub s 0 lent = t)) then  
            Some(String.sub s lent (lens - lent) ) 
        else 
            None

    let tryDropSuffix s t = 
        let lens = String.length s
        let lent = String.length t
        if (lens >= lent && (String.sub s (lens-lent) lent = t)) then 
            Some (String.sub s 0 (lens - lent))
        else
            None

    let hasPrefix s t = isSome (tryDropPrefix s t)
    let dropPrefix s t = match (tryDropPrefix s t) with Some(res) -> res | None -> failwith "String.dropPrefix"

    let hasSuffix s t = isSome (tryDropSuffix s t)
    let dropSuffix s t = match (tryDropSuffix s t) with Some(res) -> res | None -> failwith "dropSuffix"

    let isAllLower s = ((String.lowercase s) = s)    

// Make .NET Dictionaries look like the OCaml Hashtbl type. .NET Dictionaries are
// generally faster and we're gradually converting most code over to use them.
module Dictionary = 

    type ('a,'b) t = Dictionary<'a,'b>
    
    let inline create (n:int) = 
        new System.Collections.Generic.Dictionary<_,_>(n, HashIdentity.Structural)
        
    let inline of_list l = 
        let dict = new System.Collections.Generic.Dictionary<_,_>(List.length l, HashIdentity.Structural)
        l |> List.iter (fun (k,v) -> dict.Add(k,v))
        dict

    let mem (t : Dictionary<_,_>) x = t.ContainsKey(x)
    let find (t : Dictionary<_,_>) x = t.[x]
    let replace (t : Dictionary<_,_>) x y = t.[x] <- y
    let tryfind (t : Dictionary<_,_>) x = if t.ContainsKey(x) then Some(t.[x]) else None
    let add (t : Dictionary<_,_>) x y = t.[x] <- y
    let fold f (t : Dictionary<_,_>) z = t |> Seq.fold (fun acc kvp -> f kvp.Key kvp.Value acc) z
    let iter f (t : Dictionary<_,_>) = t |> Seq.iter (fun kvp -> f kvp.Key kvp.Value) 
    
    
//---------------------------------------------------
// Lists as sets. This is almost always a bad data structure and
// we should gradually eliminate these from the compiler.  

module ListSet =
    let insert e l =
        if List.mem e l then l else e::l

//---------------------------------------------------
// Misc

/// Get an initialization hole 
let getHole r = match !r with None -> failwith "getHole" | Some x -> x

module ResizeArray = 

    type 'a t = ResizeArray<'a>

    let length (bb: ResizeArray<_>) = bb.Count

    let create sz = new ResizeArray<_>(sz:int)

    let to_array arrl = Seq.to_array arrl
    
    let add (bb: ResizeArray<_>) i = bb.Add(i)
    let replace (bb: ResizeArray<_>) i x = bb.[i] <- x

    let get (bb: ResizeArray<_>) i = bb.[i]

    let choosei p arrl = 
        let rec aux i = 
            if i >= length arrl then None else 
            match p i (get arrl i) with 
            | None -> aux(i+1) 
            | res -> res 
        aux 0
    let to_list (bb: ResizeArray<_>) = ResizeArray.to_list bb
  
module Map = 
    let tryFindMulti k map = match Map.tryfind k map with Some res -> res | None -> []


//-------------------------------------------------------------------------
// Library: flat list  (immutable arrays)
//------------------------------------------------------------------------
#if FLAT_LIST_AS_ARRAY_STRUCT
[<Struct>]
type FlatList<'a> =
    val internal array : 'a[]
    internal new (arr: 'a[]) = { array = (match arr with null -> null | arr -> if arr.Length = 0 then null else arr) }
    member x.Item with get(n:int) = x.array.[n]
    member x.Length = match x.array with null -> 0 | arr -> arr.Length
    member x.IsEmpty = match x.array with null -> true | _ -> false
    static member Empty : FlatList<'a> = FlatList(null)
    interface IEnumerable<'a> with 
        member x.GetEnumerator() : IEnumerator<'a> = 
            match x.array with 
            | null -> Seq.empty.GetEnumerator()
            | arr -> (arr :> IEnumerable<'a>).GetEnumerator()
    interface IEnumerable with 
        member x.GetEnumerator() : IEnumerator = 
            match x.array with 
            | null -> (Seq.empty :> IEnumerable).GetEnumerator()
            | arr -> (arr :> IEnumerable).GetEnumerator()


[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FlatList =

    let empty<'a> = FlatList<'a>.Empty

    let collect (f: 'a -> FlatList<'a>) (x:FlatList<_>) = 
        match x.array with 
        | null -> FlatList.Empty 
        | arr -> 
           if arr.Length = 1 then f arr.[0]
           else FlatList(Array.map (fun x -> match (f x).array with null -> [| |] | arr -> arr) arr |> Array.concat)

    let exists f (x:FlatList<_>) = 
        match x.array with 
        | null -> false 
        | arr -> Array.exists f arr

    let filter f (x:FlatList<_>) = 
        match x.array with 
        | null -> FlatList.Empty 
        | arr -> FlatList(Array.filter f arr)

    let fold f acc (x:FlatList<_>) = 
        match x.array with 
        | null -> acc 
        | arr -> Array.fold_left f acc arr

    let fold2 f acc (x:FlatList<_>) (y:FlatList<_>) = 
        match x.array,y.array with 
        | null,null -> acc 
        | null,_ | _,null -> invalidArg "x" "mismatched list lengths"
        | arr1,arr2 -> Array.fold_left2 f acc arr1 arr2

    let foldBack f (x:FlatList<_>) acc  = 
        match x.array with 
        | null -> acc 
        | arr -> Array.fold_right f arr acc

    let foldBack2 f (x:FlatList<_>) (y:FlatList<_>) acc = 
        match x.array,y.array with 
        | null,null -> acc 
        | null,_ | _,null -> invalidArg "x" "mismatched list lengths"
        | arr1,arr2 -> Array.fold_right2 f arr1 arr2 acc

    let map2 f (x:FlatList<_>) (y:FlatList<_>) = 
        match x.array,y.array with 
        | null,null -> FlatList.Empty 
        | null,_ | _,null -> invalidArg "x" "mismatched list lengths"
        | arr1,arr2 -> FlatList(Array.map2 f arr1 arr2)

    let forall f (x:FlatList<_>) = 
        match x.array with 
        | null -> true 
        | arr -> Array.forall f arr

    let forall2 f (x1:FlatList<_>) (x2:FlatList<_>) = 
        match x1.array, x2.array with 
        | null,null -> true
        | null,_ | _,null -> invalidArg "x1" "mismatched list lengths"
        | arr1,arr2 -> Array.for_all2 f arr1 arr2

    let iter2 f (x1:FlatList<_>) (x2:FlatList<_>) = 
        match x1.array, x2.array with 
        | null,null -> ()
        | null,_ | _,null -> invalidArg "x1" "mismatched list lengths"
        | arr1,arr2 -> Array.iter2 f arr1 arr2

    let partition f (x:FlatList<_>) = 
        match x.array with 
        | null -> FlatList.Empty,FlatList.Empty 
        | arr -> 
            let arr1,arr2 = Array.partition f arr 
            FlatList(arr1),FlatList(arr2)

    let (* inline *) sum (x:FlatList<int>) = 
        match x.array with 
        | null -> 0 
        | arr -> Array.sum arr

    let (* inline *) sum_by (f: 'a -> int) (x:FlatList<'a>) = 
        match x.array with 
        | null -> 0 
        | arr -> Array.sum_by f arr

    let unzip (x:FlatList<_>) = 
        match x.array with 
        | null -> FlatList.Empty,FlatList.Empty 
        | arr -> let arr1,arr2 = Array.unzip arr in FlatList(arr1),FlatList(arr2)

    let physicalEquality (x:FlatList<_>) (y:FlatList<_>) = x.array === y.array 

    let tryfind f (x:FlatList<_>) = 
        match x.array with 
        | null -> None 
        | arr -> Array.tryfind f arr

    let concat (x:FlatList<_>) = 
        match x.array with 
        | null -> FlatList.Empty 
        | arr -> FlatList(Array.concat arr)

    let order eltOrder (xs:FlatList<_>) (ys:FlatList<_>) =
        match xs.array,ys.array with 
        | null,null -> 0
        | _,null -> 1
        | null,_ -> -1
        | arr1,arr2 -> Array.order eltOrder arr1 arr2

    let isEmpty (x:FlatList<_>) = x.IsEmpty
    let one(x) = FlatList([| x |])

    let toMap (x:FlatList<_>) = match x.array with null -> Map.empty | arr -> Map.of_array arr
    let length (x:FlatList<_>) = x.Length

    let map f (x:FlatList<_>) = 
        match x.array with 
        | null -> FlatList.Empty 
        | arr -> FlatList(Array.map f arr)

    let mapi f (x:FlatList<_>) = 
        match x.array with 
        | null -> FlatList.Empty 
        | arr -> FlatList(Array.mapi f arr)

    let iter f (x:FlatList<_>) = 
        match x.array with 
        | null -> ()
        | arr -> Array.iter f arr

    let iteri f (x:FlatList<_>) = 
        match x.array with 
        | null -> ()
        | arr -> Array.iteri f arr

    let mapq f (x:FlatList<_>) = 
        match x.array with 
        | null -> x
        | arr -> 
            let arr' = Array.map f arr in 
            let n = arr.Length in 
            let rec check i = if i >= n then true else arr.[i] === arr'.[i] && check (i+1) 
            if check 0 then x else FlatList(arr')

    let to_list (x:FlatList<_>) = 
        match x.array with 
        | null -> [] 
        | arr -> Array.to_list arr

    let append(l1 : FlatList<'a>) (l2 : FlatList<'a>) = 
        match l1.array, l2.array with 
        | null,_ -> l2
        | _,null -> l1
        | arr1, arr2 -> FlatList(Array.append arr1 arr2)
        
    let of_list(l) = 
        match l with 
        | [] -> FlatList.Empty 
        | l -> FlatList(Array.of_list l)

    let init n f = 
        if n = 0 then 
            FlatList.Empty 
        else 
            FlatList(Array.init n f)

    let zip (x:FlatList<_>) (y:FlatList<_>) = 
        match x.array,y.array with 
        | null,null -> FlatList.Empty
        | null,_ | _,null -> invalidArg "x" "mismatched list lengths"
        | arr1,arr2 -> FlatList(Array.zip arr1 arr2)

    let mapfold f acc (x:FlatList<_>) = 
        match x.array with
        | null -> 
            FlatList.Empty,acc
        | arr -> 
            let  arr,acc = Array.mapfold f acc x.array
            FlatList(arr),acc

    // REVIEW: systematically eliminate fmap/mapfold duplication
    let fmap f acc (x:FlatList<_>) = 
        match x.array with
        | null -> 
            acc,FlatList.Empty
        | arr -> 
            let  acc,arr = Array.fmap f acc x.array
            acc,FlatList(arr)
#endif
#if FLAT_LIST_AS_LIST

#else
type FlatList<'a> ='a list

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FlatList =
    let empty<'a> : 'a list = []
    let collect (f: 'a -> FlatList<'a>) (x:FlatList<_>) =  List.collect f x
    let exists f (x:FlatList<_>) = List.exists f x
    let filter f (x:FlatList<_>) = List.filter f x
    let fold f acc (x:FlatList<_>) = List.fold f acc x
    let fold2 f acc (x:FlatList<_>) (y:FlatList<_>) = List.fold_left2 f acc x y
    let foldBack f (x:FlatList<_>) acc  = List.foldBack f x acc
    let foldBack2 f (x:FlatList<_>) (y:FlatList<_>) acc = List.fold_right2 f x y acc
    let map2 f (x:FlatList<_>) (y:FlatList<_>) = List.map2 f x y
    let forall f (x:FlatList<_>) = List.forall f x
    let forall2 f (x1:FlatList<_>) (x2:FlatList<_>) = List.for_all2 f x1 x2
    let iter2 f (x1:FlatList<_>) (x2:FlatList<_>) = List.iter2 f x1 x2 
    let partition f (x:FlatList<_>) = List.partition f x
    let (* inline *) sum (x:FlatList<int>) = List.sum x
    let (* inline *) sum_by (f: 'a -> int) (x:FlatList<'a>) = List.sum_by f x
    let unzip (x:FlatList<_>) = List.unzip x
    let physicalEquality (x:FlatList<_>) (y:FlatList<_>) = x === y
    let tryfind f (x:FlatList<_>) = List.tryfind f x
    let concat (x:FlatList<_>) = List.concat x
    let order eltOrder (xs:FlatList<_>) (ys:FlatList<_>) = List.order eltOrder xs ys
    let isEmpty (x:FlatList<_>) = List.is_empty x
    let one(x) = [x]
    let toMap (x:FlatList<_>) = Map.of_list x
    let length (x:FlatList<_>) = List.length x
    let map f (x:FlatList<_>) = List.map f x
    let mapi f (x:FlatList<_>) = List.mapi f x
    let iter f (x:FlatList<_>) = List.iter f x
    let iteri f (x:FlatList<_>) = List.iteri f x
    let mapq f (x:FlatList<_>) = List.mapq f x
    let to_list (x:FlatList<_>) = x
    let append(l1 : FlatList<'a>) (l2 : FlatList<'a>) =  List.append l1 l2
    let of_list(l) = l
    let init n f = List.init n f
    let zip (x:FlatList<_>) (y:FlatList<_>) = List.zip x y
    let mapfold f acc (x:FlatList<_>) =  List.mapfold f acc x
    // REVIEW: systematically eliminate fmap/mapfold duplication
    let fmap f acc (x:FlatList<_>) =  List.fmap f acc x
#endif

#if FLAT_LIST_AS_ARRAY

type FlatList<'a> ='a array

type FlatListEmpty<'a>() =
    // cache the empty array in a generic static field
    static let empty : 'a array = [| |]
    static member Empty : 'a array = empty

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FlatList =
    let empty<'a> : 'a array = FlatListEmpty<'a>.Empty
    let collect (f: 'a -> FlatList<'a>) (x:FlatList<_>) =  x |> Array.map f |> Array.concat 
    let exists f x = Array.exists f x
    let filter f x = Array.filter f x
    let fold f acc x = Array.fold_left f acc x
    let fold2 f acc x y = Array.fold_left2 f acc x y
    let foldBack f x acc  = Array.fold_right f x acc
    let foldBack2 f x y acc = Array.fold_right2 f x y acc
    let map2 f x y = Array.map2 f x y
    let forall f x = Array.forall f x
    let forall2 f x1 x2 = Array.for_all2 f x1 x2
    let iter2 f x1 x2 = Array.iter2 f x1 x2 
    let partition f x = Array.partition f x
    let (* inline *) sum (x:FlatList<int>) = Array.sum x
    let (* inline *) sum_by (f: 'a -> int) (x:FlatList<'a>) = Array.sum_by f x
    let unzip x = Array.unzip x
    let physicalEquality (x:FlatList<_>) (y:FlatList<_>) = x === y
    let tryfind f x = Array.tryfind f x
    let concat x = Array.concat x
    let order eltOrder xs ys = Array.order eltOrder xs ys
    let isEmpty x = Array.is_empty x
    let one(x) = [| x |]
    let toMap x = Map.of_array x
    let length x = Array.length x
    let map f x = Array.map f x
    let mapi f x = Array.mapi f x
    let iter f x = Array.iter f x
    let iteri f x = Array.iteri f x
    let mapq f x = Array.mapq f x
    let to_list x = Array.to_list x
    let append l1 l2  =  Array.append l1 l2
    let of_list l = Array.of_list l
    let init n f = Array.init n f
    let zip  x y  = Array.zip x y
    let mapfold f acc x =  Array.mapfold f acc x
    // REVIEW: systematically eliminate fmap/mapfold duplication
    let fmap f acc x =  Array.fmap f acc x
#endif

type ResultOrException<'tresult> =
    | Result of 'tresult
    | Exception of System.Exception
                     

/// Computations that can cooperatively yield by returning a continuation
///
///    - Any yield of a NotYetDone should typically be "abandonable" without adverse consequences. No resource release
///      will be called when the computation is abandoned.
///
///    - Computations suspend via a NotYetDone may use local state (mutables), where these are
///      captured by the NotYetDone closure. Computations do not need to be restartable.
///
///    - The key thing is that you can take an Eventually value and run it with 
///      Eventually.repeatedlyProgressUntilDoneOrTimeShareOver
type Eventually<'T> = 
    | Done of 'T 
    | NotYetDone of (unit -> Eventually<'T>)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Eventually = 
    let rec box e = 
        match e with 
        | Done x -> Done (Operators.box x) 
        | NotYetDone (work) -> NotYetDone (fun () -> box (work()))

    let rec force e = 
        match e with 
        | Done x -> x 
        | NotYetDone (work) -> force (work())

    let repeatedlyProgressUntilDoneOrTimeShareOver timeShareInMilliseconds runner e = 
        let sw = new System.Diagnostics.Stopwatch() 
        let rec runTimeShare e = 
          runner (fun () -> 
            sw.Reset()
            sw.Start(); 
            let rec loop(e) = 
                match e with 
                | Done _ -> e
                | NotYetDone (work) -> 
                    if sw.ElapsedMilliseconds > timeShareInMilliseconds then 
                        sw.Stop();
                        NotYetDone(fun () -> runTimeShare e) 
                    else 
                        loop(work())
            loop(e))
        runTimeShare e

    let rec bind k e = 
        match e with 
        | Done x -> k x 
        | NotYetDone work -> NotYetDone (fun () -> bind k (work()))

    let fold f acc seq = 
        (Done acc,seq) ||> Seq.fold  (fun acc x -> acc |> bind (fun acc -> f acc x))
        
    let rec catch e = 
        match e with 
        | Done x -> Done(Result x)
        | NotYetDone work -> 
            NotYetDone (fun () -> 
                let res = try Result(work()) with | e -> Exception e 
                match res with 
                | Result cont -> catch cont
                | Exception e -> Done(Exception e))
    
    let delay f = NotYetDone (fun () -> f())

    let tryFinally e compensation =    
        catch (e) 
        |> bind (fun res ->  compensation();
                             match res with 
                             | Result v -> Eventually.Done v
                             | Exception e -> raise e)

    let tryWith e handler =    
        catch e 
        |> bind (function Result v -> Done v | Exception e -> handler e)
    
type EventuallyBuilder() = 
    member x.Bind(e,k) = Eventually.bind k e
    member x.Return(v) = Eventually.Done v
    member x.Combine(e1,e2) = e1 |> Eventually.bind (fun () -> e2)
    member x.TryWith(e,handler) = Eventually.tryWith e handler
    member x.TryFinally(e,compensation) =  Eventually.tryFinally e compensation
    //member x.Using(resource:System.IDisposable,e) = Eventually.tryFinally (e resource)  resource.Dispose
    member x.Delay(f) = Eventually.delay f
    member x.Zero() = Eventually.Done ()


let eventually = new EventuallyBuilder()

(*
let _ = eventually { return 1 }
let _ = eventually { let x = 1 in return 1 }
let _ = eventually { let! x = eventually { return 1 } in return 1 }
let _ = eventually { try return (failwith "") with _ -> return 1 }
let _ = eventually { use x = null in return 1 }
*)

//---------------------------------------------------------------------------
// generate unique stamps
//---------------------------------------------------------------------------

type UniqueStampGenerator<'a>() = 
    let encodeTab = new Dictionary<'a,int>()
    let mutable nItems = 0
    let encode str = 
        if encodeTab.ContainsKey(str)
        then
            encodeTab.[str]
        else
            let idx = nItems
            encodeTab.[str] <- idx
            nItems <- nItems + 1
            idx
    member this.Encode(str)  = encode str

//---------------------------------------------------------------------------
// memoize tables (all entries cached, never collected)
//---------------------------------------------------------------------------
    
type MemoizationTable<'a,'b>(compute: 'a -> 'b, ?canMemoize, ?keyHash, ?keyEquals) = 
    
    let table = 
        match keyHash,keyEquals with 
        | Some h, Some e -> new System.Collections.Generic.Dictionary<'a,'b>(HashIdentity.FromFunctions h e) 
        | _ -> new System.Collections.Generic.Dictionary<'a,'b>() 
    member t.Apply(x) = 
        if (match canMemoize with None -> true | Some f -> f x) then 
            let mutable res = Unchecked.defaultof<'b>
            let ok = table.TryGetValue(x,&res)
            if ok then res 
            else
                lock table (fun () -> 
                    let mutable res = Unchecked.defaultof<'b> 
                    let ok = table.TryGetValue(x,&res)
                    if ok then res 
                    else
                        let res = compute x
                        table.[x] <- res;
                        res)
        else compute x


type LazyWithContextFailure(exn:exn) =
    static let undefined = new LazyWithContextFailure(Undefined)
    member x.Exception = exn
    static member Undefined = undefined
        
/// Just like "Lazy" but EVERY forcer must provide an instance of "ctxt", e.g. to help track errors
/// on forcing back to at least one sensible user location
[<DefaultAugmentation(false)>]
[<StructuralEquality(false); StructuralComparison(false)>]
type LazyWithContext<'a,'ctxt> = 
    { /// This field holds the result of a successful computation. It's initial value is Unchecked.defaultof
      mutable value : 'a
      /// This field holds either the function to run or a LazyWithContextFailure object recording the exception raised 
      /// from running the function. It is null if the thunk has been evaluated successfully.
      mutable funcOrException: obj }
    static member Create(f: ('ctxt->'a)) : LazyWithContext<'a,'ctxt> = 
        { value = Unchecked.defaultof<'a>;
          funcOrException = box(f); }
    static member NotLazy(x:'a) : LazyWithContext<'a,'ctxt> = 
        { value = x;
          funcOrException = null; }
    member x.IsDelayed = (match x.funcOrException with null -> false | :? LazyWithContextFailure -> false | _ -> true)
    member x.IsException = (match x.funcOrException with null -> false | :? LazyWithContextFailure -> true | _ -> false)
    member x.IsForced = (match x.funcOrException with null -> true | _ -> false)
    member x.Force(ctxt:'ctxt) =  
        match x.funcOrException with 
        | null -> x.value 
        | _ -> 
            // Enter the lock in case another thread is in the process of evaluting the result
            System.Threading.Monitor.Enter(x);
            try 
                x.UnsynchronizedForce(ctxt)
            finally
                System.Threading.Monitor.Exit(x)

    member x.UnsynchronizedForce(ctxt) = 
        match x.funcOrException with 
        | null -> x.value 
        | :? LazyWithContextFailure as res -> 
              raise(res.Exception)
        | :? ('ctxt -> 'a) as f -> 
              x.funcOrException <- box(LazyWithContextFailure.Undefined)
              try 
                  let res = f ctxt 
                  x.funcOrException <- null; 
                  x.value <- res; 
                  res
              with e -> 
                  x.funcOrException <- box(new LazyWithContextFailure(e)); 
                  rethrow()
        | _ -> 
            failwith "unreachable"

    member x.SynchronizedForce(ctxt) = x.Force(ctxt)
    

