// (c) Microsoft Corporation 2005-2009.  

namespace Microsoft.FSharp.Collections

open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.Operators
open Microsoft.FSharp.Control
open Microsoft.FSharp.Collections
open System
open System.Collections.Generic

#nowarn "21" // recursive initialization
#nowarn "40" // recursive initialization

type LazyList<'T> =
    { mutable status : LazyCellStatus< 'T > }
    
    member x.Force = 
        match x.status with 
        | LazyCellStatus.Delayed f -> 
            x.status <- Exception Undefined; 
            begin 
              try let res = f () in x.status <- LazyCellStatus.Value res; res 
              with e -> x.status <- LazyCellStatus.Exception(e); rethrow()
            end
        | LazyCellStatus.Value x -> x
        | LazyCellStatus.Exception e -> raise e
    
    member s.GetEnumeratorImpl() = 
            let getcell (x : LazyList<'T>) = x.Force
            let to_seq s = Seq.unfold (fun ll -> match getcell ll with CellEmpty -> None | CellCons(a,b) -> Some(a,b)) s 
            (to_seq s).GetEnumerator()
            
    interface IEnumerable<'T> with
        member s.GetEnumerator() = s.GetEnumeratorImpl()

    interface System.Collections.IEnumerable with
        override s.GetEnumerator() = (s.GetEnumeratorImpl() :> System.Collections.IEnumerator)

and LazyCellStatus<'T> =
    | Delayed of (unit -> LazyListCell <'T> )
    | Value of LazyListCell <'T> 
    | Exception of System.Exception

and LazyListCell<'T> = 
    | CellCons of 'T * LazyList<'T> 
    | CellEmpty

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module LazyList = 

    let lzy f = { status = Delayed f }
    let force (x: LazyList<'T>) = x.Force

    let notlazy v = { status = Value v }
    
    type 'T t = LazyList<'T>
    type 'T llist = LazyList<'T>

    type LazyItem<'T> = Cons of 'T * LazyList<'T> | Empty
    type 'T item = 'T LazyItem
    let get (x : LazyList<'T>) = match force x with CellCons (a,b) -> Some(a,b) | CellEmpty -> None
    let getcell (x : LazyList<'T>) = force x 
    let empty () = notlazy CellEmpty
    let make x = (x : LazyList<'T>)
    let consc x l = CellCons(x,l)
    let cons x l = lzy(fun () -> (consc x l))
    let consf x l = lzy(fun () -> (consc x (lzy(fun () ->  (force (l()))))))

    let rec unfold f z = 
      lzy(fun () -> 
          match f z with
          | None       -> CellEmpty
          | Some (x,z) -> CellCons (x,unfold f z))

    let rec append l1  l2 = lzy(fun () ->  (appendc l1 l2))
    and appendc l1 l2 =
      match getcell l1 with
      | CellEmpty -> force l2
      | CellCons(a,b) -> consc a (append b l2)

    let delayed f = lzy(fun () ->  (getcell (f())))
    let repeat x = 
      let rec s = cons x (delayed (fun () -> s)) in s

    let rec map f s = 
      lzy(fun () ->  
        match getcell s with
        | CellEmpty -> CellEmpty
        | CellCons(a,b) -> consc (f a) (map f b))

    let rec map2 f s1 s2 =  
      lzy(fun () -> 
        match getcell s1, getcell s2  with
        | CellCons(a1,b1),CellCons(a2,b2) -> consc (f a1 a2) (map2 f b1 b2)
        | _ -> CellEmpty)

    let rec combine s1 s2 = 
      lzy(fun () -> 
        match getcell s1, getcell s2  with
        | CellCons(a1,b1),CellCons(a2,b2) -> consc (a1,a2) (combine b1 b2)
        | _ -> CellEmpty)

    let rec concat s1 = 
      lzy(fun () -> 
        match getcell s1 with
        | CellCons(a,b) -> appendc a (concat b)
        | CellEmpty -> CellEmpty)
      
    let rec filter p s1= lzy(fun () ->  filterc p s1)
    and filterc p s1 =
        match getcell s1 with
        | CellCons(a,b) -> if p a then consc a (filter p b) else filterc p b
        | CellEmpty -> CellEmpty
      
    let rec first p s1 =
        match getcell s1 with
        | CellCons(a,b) -> if p a then Some a else first p b
        | CellEmpty -> None

    let indexNotFound() = raise (new System.Collections.Generic.KeyNotFoundException("An index satisfying the predicate was not found in the collection"))

    let find p s1 =
        match first p s1 with
        | Some a -> a
        | None   -> indexNotFound()

    let find_all p s1= filter p s1 (* deprecated *)
    let flatten s1= concat s1      (* deprecated *)

    let rec folds f acc s1 = 
      lzy(fun () -> 
        match getcell s1 with
        | CellCons(a,b) -> let acc' = f acc a in consc acc' (folds f acc' b)
        | CellEmpty -> CellEmpty)

    let hd s = 
      match getcell s with
      | CellCons(a,b) -> a
      | CellEmpty -> invalidArg "s" "the list is empty"

    let tl s = 
      match getcell s with
      | CellCons(a,b) -> b
      | CellEmpty -> invalidArg "s" "the list is empty"

    let nonempty s1 =
      match getcell s1 with
      | CellCons(a,b) -> true
      | CellEmpty -> false

    let rec take n s = 
      lzy(fun () -> 
        if n < 0 then invalidArg "n" "the number must not be negative"
        elif n = 0 then CellEmpty 
        else
          match getcell s with
          | CellCons(a,s) -> consc a (take (n-1) s)
          | CellEmpty -> invalidArg "n" "not enough items in the list" )

    let rec drop n s = 
      lzy(fun () -> 
        if n < 0 then invalidArg "n" "the value must not be negative"
        else dropc n s)
    and dropc n s =
      if n = 0 then force s 
      else  
        match getcell s with
        | CellCons(a,s) -> dropc (n-1) s
        | CellEmpty -> invalidArg "n" "not enough items in the list"

    let rec of_list l = 
      lzy(fun () -> 
        match l with [] -> CellEmpty | h :: t -> consc h (of_list t))
      
    let rec to_list s = 
      match getcell s with
      | CellEmpty -> []
      | CellCons(h,t) -> h :: to_list t
      
    let rec copy_from i a = 
      lzy(fun () -> 
        if i >= Array.length a then CellEmpty 
        else consc a.[i] (copy_from (i+1) a))
      
    let rec copy_to (arr: _[]) s i = 
      match getcell s with
      | CellEmpty -> ()
      | CellCons(a,b) -> arr.[i] <- a; copy_to arr b (i+1)

      
    let of_array a = copy_from 0 a
    let to_array s = Array.of_list (to_list s)
      
    let rec mem x s = 
      match getcell s with
      | CellEmpty -> false
      | CellCons(a,b) -> x = a || mem x b

    let rec length_aux n s = 
      match getcell s with
      | CellEmpty -> 0
      | CellCons(_,b) -> length_aux (n+1) b

    let length s = length_aux 0 s

    let to_seq (s: LazyList<'T>) = (s :> IEnumerable<_>)

    // Note: this doesn't dispose of the IEnumerator if the iteration is not run to the end
    let rec of_fresh_IEnumerator (e : IEnumerator<_>) = 
      lzy(fun () -> 
        if e.MoveNext() then 
          consc e.Current (of_fresh_IEnumerator e)
        else 
           e.Dispose()
           CellEmpty)
      
    let of_seq (c : IEnumerable<_>) =
      of_fresh_IEnumerator (c.GetEnumerator()) 
      
    let (|Cons|Nil|) l = match getcell l with CellCons(a,b) -> Cons(a,b) | CellEmpty -> Nil

