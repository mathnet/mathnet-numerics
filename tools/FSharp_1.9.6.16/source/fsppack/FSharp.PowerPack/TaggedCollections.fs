// (c) Microsoft Corporation 2005-2009. 


#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities.Collections.Tagged
#else
namespace Microsoft.FSharp.Collections.Tagged
#endif

    #nowarn "51"
    #nowarn "69" // interface implementations in augmentations
    #nowarn "60" // override implementations in augmentations
    #nowarn "191" // The struct, record or union type 'Map' has an explicit implementation of 'Object.GetHashCode'. Consider implementing an override for 'System.Object.Equals(obj)

    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open System
    open System.Collections.Generic
#if INTERNALIZED_POWER_PACK
    open Internal.Utilities
    open Internal.Utilities.Collections
#else
    open Microsoft.FSharp.Collections
#endif


    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    type SetTree<'a> = 
        | SetEmpty                                          // height = 0   
        | SetNode of 'a * SetTree<'a> *  SetTree<'a> * int    // height = int 
#if ONE
        | SetOne  of 'a                                     // height = 1   
#endif
        // OPTIMIZATION: store SetNode(k,SetEmpty,SetEmpty,1) --->  SetOne(k) 


    // CONSIDER: SetTree<'a> = SetEmpty | SetNode of 'a  * SetTree<'a> *  SetTree<'a> * int
    //  with SetOne = SetNode of (x,null,null,1)

    module RawSetOps = 
        let empty = SetEmpty

        let height t = 
            match t with 
            | SetEmpty -> 0
#if ONE
            | SetOne _ -> 1
#endif
            | SetNode (_,_,_,h) -> h

#if CHECKED
        let rec checkInvariant t =
            // A good sanity check, loss of balance can hit perf 
            match t with 
            | SetEmpty -> true
            | SetOne _ -> true
            | SetNode (k,t1,t2,h) ->
                let h1 = height t1 in
                let h2 = height t2 in
                (-2 <= (h1 - h2) && (h1 - h2) <= 2) && checkInvariant t1 && checkInvariant t2
#else
        let inline SetOne(x) = SetNode(x,SetEmpty,SetEmpty,1)
#endif

        let tolerance = 2

        let mk l hl k r hr = 
#if ONE
            if hl = 0 && hr = 0 then SetOne (k)
            else
#endif
              let m = if hl < hr then hr else hl 
              SetNode(k,l,r,m+1)

        let rebalance t1 k t2 =
            let t1h = height t1 
            let t2h = height t2
            if  t2h > t1h + tolerance then // right is heavier than left 
                match t2 with 
                | SetNode(t2k,t2l,t2r,_) -> 
                    // one of the nodes must have height > height t1 + 1 
                    let t2lh = height t2l
                    if t2lh > t1h + 1 then  // balance left: combination 
                        match t2l with 
                        | SetNode(t2lk,t2ll,t2lr,t2lh) ->
                            let l = mk t1 t1h k t2ll (height t2ll)
                            let r = mk t2lr (height t2lr) t2k t2r (height t2r)
                            mk l (height l) t2lk r (height r)
                        | _ -> failwith "rebalance"
                    else // rotate left 
                        let l = mk t1 t1h k t2l t2lh
                        mk l (height l) t2k t2r (height t2r)
                | _ -> failwith "rebalance"
            else
                if  t1h > t2h + tolerance then // left is heavier than right 
                    match t1 with 
                    | SetNode(t1k,t1l,t1r,_) -> 
                        // one of the nodes must have height > height t2 + 1 
                        let t1rh = height t1r
                        if t1rh > t2h + 1 then 
                            // balance right: combination 
                            match t1r with 
                            | SetNode(t1rk,t1rl,t1rr,t1rh) ->
                                let l = mk t1l (height t1l) t1k t1rl (height t1rl)
                                let r = mk t1rr (height t1rr) k t2 t2h
                                mk l (height l) t1rk r (height r)
                            | _ -> failwith "rebalance"
                        else
                            let r = mk t1r t1rh k t2 t2h
                            mk t1l (height t1l) t1k r (height r)
                    | _ -> failwith "rebalance"
                else mk t1 t1h k t2 t2h

        let rec add (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k t = 
            match t with 
            | SetNode (k2,l,r,h) -> 
                let c = comparer.Invoke(k,k2) 
                if   c < 0 then rebalance (add comparer k l) k2 r
                elif c = 0 then t
                else            rebalance l k2 (add comparer k r)
#if ONE
            | SetOne(k2) -> 
                // nb. no check for rebalance needed for small trees, also be sure to reuse node already allocated 
                let c = comparer.Invoke(k,k2) 
                if c < 0   then SetNode (k,SetEmpty,t,2)
                elif c = 0 then t
                else            SetNode (k,t,SetEmpty,2)                  
#endif
            | SetEmpty -> SetOne(k)

        let rec balance comparer t1 k t2 =
            // Given t1 < k < t2 where t1 and t2 are "balanced",
            // return a balanced tree for <t1,k,t2>.
            // Recall: balance means subtrees heights differ by at most "tolerance"
            match t1,t2 with
            | SetEmpty,t2  -> add comparer k t2 // drop t1 = empty 
            | t1,SetEmpty  -> add comparer k t1 // drop t2 = empty 
#if ONE
            | SetOne k1,t2 -> add comparer k (add comparer k1 t2)
            | t1,SetOne k2 -> add comparer k (add comparer k2 t1)
#endif
            | SetNode(k1,t11,t12,t1h),SetNode(k2,t21,t22,t2h) ->
                // Have:  (t11 < k1 < t12) < k < (t21 < k2 < t22)
                // Either (a) h1,h2 differ by at most 2 - no rebalance needed.
                //        (b) h1 too small, i.e. h1+2 < h2
                //        (c) h2 too small, i.e. h2+2 < h1 
                if   t1h+tolerance < t2h then
                    // case: b, h1 too small 
                    // push t1 into low side of t2, may increase height by 1 so rebalance 
                    rebalance (balance comparer t1 k t21) k2 t22
                elif t2h+tolerance < t1h then
                    // case: c, h2 too small 
                    // push t2 into high side of t1, may increase height by 1 so rebalance 
                    rebalance t11 k1 (balance comparer t12 k t2)
                else
                    // case: a, h1 and h2 meet balance requirement 
                    mk t1 t1h k t2 t2h

        let rec split (comparer : OptimizedClosures.FastFunc2<'a,'a,int>) pivot t =
            // Given a pivot and a set t
            // Return { x in t s.t. x < pivot }, pivot in t? , { x in t s.t. x > pivot } 
            match t with
            | SetNode(k1,t11,t12,h1) ->
                let c = comparer.Invoke(pivot,k1)
                if   c < 0 then // pivot t1 
                    let t11_lo,havePivot,t11_hi = split comparer pivot t11
                    t11_lo,havePivot,balance comparer t11_hi k1 t12
                elif c = 0 then // pivot is k1 
                    t11,true,t12
                else            // pivot t2 
                    let t12_lo,havePivot,t12_hi = split comparer pivot t12
                    balance comparer t11 k1 t12_lo,havePivot,t12_hi
#if ONE
            | SetOne k1 ->
                let c = comparer.Invoke(k1,pivot)
                if   c < 0 then t       ,false,SetEmpty // singleton under pivot 
                elif c = 0 then SetEmpty,true ,SetEmpty // singleton is    pivot 
                else            SetEmpty,false,t        // singleton over  pivot 
#endif
            | SetEmpty  -> 
                SetEmpty,false,SetEmpty
        
        let rec spliceOutSuccessor t = 
            match t with 
            | SetEmpty -> failwith "internal error: Map.splice_out_succ_or_pred"
#if ONE
            | SetOne (k2) -> k2,empty
#endif
            | SetNode (k2,l,r,_) ->
                match l with 
                | SetEmpty -> k2,r
                | _ -> let k3,l' = spliceOutSuccessor l in k3,mk l' (height l') k2 r (height r)

        let rec remove (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k t = 
            match t with 
            | SetEmpty -> t
#if ONE
            | SetOne (k2) -> 
                let c = comparer.Invoke(k,k2) 
                if   c = 0 then empty
                else            t
#endif
            | SetNode (k2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if   c < 0 then rebalance (remove comparer k l) k2 r
                elif c = 0 then 
                  match l,r with 
                  | SetEmpty,_ -> r
                  | _,SetEmpty -> l
                  | _ -> 
                      let sk,r' = spliceOutSuccessor r 
                      mk l (height l) sk r' (height r')
                else rebalance l k2 (remove comparer k r) 

        let rec mem (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k t = 
            match t with 
            | SetNode(k2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if   c < 0 then mem comparer k l
                elif c = 0 then true
                else mem comparer k r
#if ONE
            | SetOne(k2) -> (comparer.Invoke(k,k2) = 0)
#endif
            | SetEmpty -> false

        let rec iter f t = 
            match t with 
            | SetNode(k2,l,r,_) -> iter f l; f k2; iter f r
#if ONE
            | SetOne(k2) -> f k2
#endif
            | SetEmpty -> ()            

        // Fold, left-to-right. 
        //
        // NOTE: This matches OCaml behaviour, though differs from the
        // behaviour of Map.fold_right which folds right-to-left.
        let rec fold f m x = 
            match m with 
            | SetNode(k,l,r,h) -> fold f r (f k (fold f l x))
#if ONE
            | SetOne(k) -> f k x
#endif
            | SetEmpty -> x                

        let rec foldr f m x = 
            match m with 
            | SetNode(k,l,r,h) -> foldr f l (f k (foldr f r x))
#if ONE
            | SetOne(k) -> f k x
#endif
            | SetEmpty -> x

        let rec for_all f m = 
            match m with 
            | SetNode(k2,l,r,h) -> f k2 && for_all f l && for_all f r
#if ONE
            | SetOne(k2) -> f k2
#endif
            | SetEmpty -> true          

        let rec exists f m = 
            match m with 
            | SetNode(k2,l,r,h) -> f k2 || exists f l || exists f r
#if ONE
            | SetOne(k2) -> f k2
#endif
            | SetEmpty -> false         

        let is_empty m = match m with  | SetEmpty -> true | _ -> false

        let subset comparer a b  = for_all (fun x -> mem comparer x b) a

        let rec elementsAux m acc = 
            match m with 
            | SetNode(k2,l,r,_) -> k2 :: (elementsAux l (elementsAux r acc))
#if ONE
            | SetOne(k2) -> k2 :: acc
#endif
            | SetEmpty -> acc                

        let elements a  = elementsAux a []

        let rec filterAux comparer f s acc = 
            match s with 
            | SetNode(k,l,r,_) -> 
                let acc = if f k then add comparer k acc else acc 
                filterAux comparer f l (filterAux comparer f r acc)
#if ONE
            | SetOne(k) -> if f k then add comparer k acc else acc
#endif
            | SetEmpty -> acc           

        let filter comparer f s = filterAux comparer f s empty

        let rec diffAux comparer m acc = 
            match m with 
            | SetNode(k,l,r,_) -> diffAux comparer l (diffAux comparer r (remove comparer k acc))
#if ONE
            | SetOne(k) -> remove comparer k acc
#endif
            | SetEmpty -> acc           

        let diff comparer a b = diffAux comparer b a

        let rec cardinalAux s acc = 
            match s with 
            | SetNode(k,l,r,_) -> cardinalAux l (cardinalAux r (acc+1))
#if ONE
            | SetOne(k) -> acc+1
#endif
            | SetEmpty -> acc           

        let cardinal s = cardinalAux s 0
        let size s = cardinal s 

        let rec union comparer t1 t2 =
            // Perf: tried bruteForce for low heights, but nothing significant 
            match t1,t2 with               
            | SetNode(k1,t11,t12,h1),SetNode(k2,t21,t22,h2) -> // (t11 < k < t12) AND (t21 < k2 < t22) 
                // Divide and Quonquer:
                //   Suppose t1 is largest.
                //   Split t2 using pivot k1 into lo and hi.
                //   Union disjoint subproblems and then combine. 
                if h1 > h2 then
                  let lo,_,hi = split comparer k1 t2 in
                  balance comparer (union comparer t11 lo) k1 (union comparer t12 hi)
                else
                  let lo,_,hi = split comparer k2 t1 in
                  balance comparer (union comparer t21 lo) k2 (union comparer t22 hi)
            | SetEmpty,t -> t
            | t,SetEmpty -> t
#if ONE
            | SetOne k1,t2 -> add comparer k1 t2
            | t1,SetOne k2 -> add comparer k2 t1
#endif

        let rec intersectionAux comparer b m acc = 
            match m with 
            | SetNode(k,l,r,_) -> 
                let acc = intersectionAux comparer b r acc 
                let acc = if mem comparer k b then add comparer k acc else acc 
                intersectionAux comparer b l acc
#if ONE
            | SetOne(k) -> 
                if mem comparer k b then add comparer k acc else acc
#endif
            | SetEmpty -> acc

        let intersection comparer a b = intersectionAux comparer b a empty

        let partition1 comparer f k (acc1,acc2) = if f k then (add comparer k acc1,acc2) else (acc1,add comparer k acc2) 
        
        let rec partitionAux comparer f s acc = 
            match s with 
            | SetNode(k,l,r,_) -> 
                let acc = partitionAux comparer f r acc 
                let acc = partition1 comparer f k acc
                partitionAux comparer f l acc
#if ONE
            | SetOne(k) -> partition1 comparer f k acc
#endif
            | SetEmpty -> acc           

        let partition comparer f s = partitionAux comparer f s (empty,empty)

        // It's easier to get many less-important algorithms right using this active pattern
        let (|MatchSetNode|MatchSetEmpty|) s = 
            match s with 
            | SetNode(k2,l,r,_) -> MatchSetNode(k2,l,r)
#if ONE
            | SetOne(k2) -> MatchSetNode(k2,SetEmpty,SetEmpty)
#endif
            | SetEmpty -> MatchSetEmpty
        
        let rec nextElemCont (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k s cont = 
            match s with 
            | MatchSetNode(k2,l,r) -> 
                let c = comparer.Invoke(k,k2) 
                if   c < 0 then nextElemCont comparer k l (function None -> cont(Some(k2)) | res -> res)
                elif c = 0 then cont(minimumElementOpt r) 
                else nextElemCont comparer k r cont
            | MatchSetEmpty -> cont(None)

        and nextElem comparer k s = nextElemCont comparer k s (fun res -> res)
        
        and prevElemCont (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k s cont = 
            match s with 
            | MatchSetNode(k2,l,r) -> 
                let c = comparer.Invoke(k,k2) 
                if   c > 0 then prevElemCont comparer k r (function None -> cont(Some(k2)) | res -> res)
                elif c = 0 then cont(maximumElementOpt r) 
                else prevElemCont comparer k l cont
            | MatchSetEmpty -> cont(None)

        and prevElem comparer k s = prevElemCont comparer k s (fun res -> res)
        
        and minimumElementAux s n = 
            match s with 
            | SetNode(k,l,r,_) -> minimumElementAux l k
#if ONE
            | SetOne(k) -> k
#endif
            | SetEmpty -> n

        and minimumElementOpt s = 
            match s with 
            | SetNode(k,l,r,_) -> Some(minimumElementAux l k)
#if ONE
            | SetOne(k) -> Some k
#endif
            | SetEmpty -> None

        and maximumElementAux s n = 
            match s with 
            | SetNode(k,l,r,_) -> maximumElementAux r k
#if ONE
            | SetOne(k) -> k
#endif
            | SetEmpty -> n             

        and maximumElementOpt s = 
            match s with 
            | SetNode(k,l,r,_) -> Some(maximumElementAux r k)
#if ONE
            | SetOne(k) -> Some(k)
#endif
            | SetEmpty -> None

        let minimumElement s = 
            match minimumElementOpt s with 
            | Some(k) -> k
            | None -> failwith "minimumElement"            

        let maximumElement s = 
            match maximumElementOpt s with 
            | Some(k) -> k
            | None -> failwith "maximumElement"


        //--------------------------------------------------------------------------
        // Imperative left-to-right iterators.
        //--------------------------------------------------------------------------

        type 'a iterator = { mutable stack: SetTree<'a> list;  // invariant: always collapseLHS result 
                             mutable started : bool           // true when MoveNext has been called   
                           }

        // collapseLHS:
        // a) Always returns either [] or a list starting with SetOne.
        // b) The "fringe" of the set stack is unchanged.
        let rec collapseLHS stack =
            match stack with
            | []                       -> []
            | SetEmpty         :: rest -> collapseLHS rest
#if ONE
            | SetOne k         :: rest -> stack
#else
            | SetNode(k,SetEmpty,SetEmpty,h) :: rest -> stack
#endif
            | SetNode(k,l,r,h) :: rest -> collapseLHS (l :: SetOne k :: r :: rest)
          
        let mkIterator s = { stack = collapseLHS [s]; started = false }

        let not_started() = raise (new System.InvalidOperationException("Enumeration has not started. Call MoveNext."))
        let already_finished() = raise (new System.InvalidOperationException("Enumeration already finished."))

        let current i =
            if i.started then
                match i.stack with
#if ONE
                  | SetOne k :: _ -> k
#else
                  | SetNode( k,_,_,_) :: _ -> k
#endif
                  | []            -> already_finished()
                  | _             -> failwith "Please report error: Set iterator, unexpected stack for current"
            else
                not_started()

        let rec moveNext i =
            if i.started then
                match i.stack with
#if ONE
                  | SetOne k :: rest -> 
#else
                  | SetNode(k,_,_,_) :: rest -> 
#endif
                        i.stack <- collapseLHS rest;
                        i.stack <> []
                  | [] -> false
                  | _ -> failwith "Please report error: Set iterator, unexpected stack for moveNext"
            else
                i.started <- true;  // The first call to MoveNext "starts" the enumeration.
                i.stack <> []

        let mkIEnumerator s = 
            let i = ref (mkIterator s) 
            { new IEnumerator<_> with 
                  member x.Current = current !i
              interface System.Collections.IEnumerator with 
                  member x.Current = box (current !i)
                  member x.MoveNext() = moveNext !i
                  member x.Reset() = i :=  mkIterator s
              interface System.IDisposable with 
                  member x.Dispose() = () }

        //--------------------------------------------------------------------------
        // Set comparison.  This can be expensive.
        //--------------------------------------------------------------------------

        let rec compareStacks (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) l1 l2 =
            match l1,l2 with 
            | [],[] ->  0
            | [],_  -> -1
            | _ ,[] ->  1
            | (SetEmpty  _ :: t1),(SetEmpty    :: t2) -> compareStacks comparer t1 t2
#if ONE
            | (SetOne(n1k) :: t1),(SetOne(n2k) :: t2) -> 
                 let c = comparer.Invoke(n1k,n2k) 
                 if c <> 0 then c else compareStacks comparer t1 t2
            | (SetOne(n1k) :: t1),(SetNode(n2k,SetEmpty,n2r,_) :: t2) -> 
                 let c = comparer.Invoke(n1k,n2k) 
                 if c <> 0 then c else compareStacks comparer (empty :: t1) (n2r :: t2)
            | (SetNode(n1k,(SetEmpty as emp),n1r,_) :: t1),(SetOne(n2k) :: t2) -> 
                 let c = comparer.Invoke(n1k,n2k) 
                 if c <> 0 then c else compareStacks comparer (n1r :: t1) (emp :: t2)
#endif
            | (SetNode(n1k,(SetEmpty as emp),n1r,_) :: t1),(SetNode(n2k,SetEmpty,n2r,_) :: t2) -> 
                 let c = comparer.Invoke(n1k,n2k) 
                 if c <> 0 then c else compareStacks comparer (n1r :: t1) (n2r :: t2)
#if ONE
            | (SetOne(n1k) :: t1),_ -> 
                compareStacks comparer (empty :: SetOne(n1k) :: t1) l2
#endif
            | (SetNode(n1k,n1l,n1r,_) :: t1),_ -> 
                compareStacks comparer (n1l :: SetNode(n1k,empty,n1r,0) :: t1) l2
#if ONE
            | _,(SetOne(n2k) :: t2) -> 
                compareStacks comparer l1 (empty :: SetOne(n2k) :: t2)
#endif
            | _,(SetNode(n2k,n2l,n2r,_) :: t2) -> 
                compareStacks comparer l1 (n2l :: SetNode(n2k,empty,n2r,0) :: t2)
                
        let compare comparer s1 s2 = 
            match s1,s2 with 
            | SetEmpty,SetEmpty -> 0
            | SetEmpty,_ -> -1
            | _,SetEmpty -> 1
            | _ -> compareStacks comparer [s1] [s2]

        let choose s = minimumElement s

        let to_list s = 
            let rec loop m x = 
                match m with 
                | SetNode(k,l,r,h) -> loop l (k :: (loop r x))
#if ONE
                | SetOne(k) -> k :: x
#endif
                | SetEmpty -> x
            loop s []            

        let copyToArray s (arr: _[]) i =
            let j = ref i 
            iter (fun x -> arr.[!j] <- x; j := !j + 1) s

        let to_array s = 
            let n = (cardinal s) 
            let res = Array.zeroCreate n 
            copyToArray s res 0;
            res

        let rec mkFromEnumerator comparer acc (e : IEnumerator<_>) = 
            if e.MoveNext() then 
              mkFromEnumerator comparer (add comparer e.Current acc) e
            else acc
          
        let of_seq comparer (c : IEnumerable<_>) =
            use ie = c.GetEnumerator()
            mkFromEnumerator comparer empty ie 

        let of_array comparer l = Array.fold (fun acc k -> add comparer k acc) empty l    


    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    type MapTree<'key,'a> = 
        | MapEmpty 
#if ONE 
        | MapOne of 'key * 'a
#endif
        // Note: performance rumour has it that the data held in this node should be
        // exactly one cache line. It is currently ~7 words. Thus it might be better to
        // move to a n-way tree.
        | MapNode of 'key * 'a * MapTree<'key,'a> *  MapTree<'key,'a> * int


    module RawMapOps = 

        let empty = MapEmpty 

        let inline height x  = 
          match x with 
          | MapEmpty -> 0
#if ONE 
          | MapOne _ -> 1
#endif
          | MapNode(_,_,_,_,h) -> h

        let is_empty m = 
            match m with 
            | MapEmpty -> true
            | _ -> false

        let mk l k v r = 
#if ONE 
            match l,r with 
            | MapEmpty,MapEmpty -> MapOne(k,v)
            | _ -> 
#endif
                let hl = height l 
                let hr = height r 
                let m = if hl < hr then hr else hl 
                MapNode(k,v,l,r,m+1)

        let rebalance t1 k v t2 =
            let t1h = height t1 
            if  height t2 > t1h + 2 then (* right is heavier than left *)
                match t2 with 
                | MapNode(t2k,t2v,t2l,t2r,t2h) -> 
                   (* one of the nodes must have height > height t1 + 1 *)
                   if height t2l > t1h + 1 then  (* balance left: combination *)
                     match t2l with 
                     | MapNode(t2lk,t2lv,t2ll,t2lr,t2lh) ->
                        mk (mk t1 k v t2ll) t2lk t2lv (mk t2lr t2k t2v t2r) 
                     | _ -> failwith "rebalance"
                   else (* rotate left *)
                     mk (mk t1 k v t2l) t2k t2v t2r
                | _ -> failwith "rebalance"
            else
                let t2h = height t2 
                if  t1h > t2h + 2 then (* left is heavier than right *)
                  match t1 with 
                  | MapNode(t1k,t1v,t1l,t1r,t1h) -> 
                    (* one of the nodes must have height > height t2 + 1 *)
                      if height t1r > t2h + 1 then 
                      (* balance right: combination *)
                        match t1r with 
                        | MapNode(t1rk,t1rv,t1rl,t1rr,t1rh) ->
                            mk (mk t1l t1k t1v t1rl) t1rk t1rv (mk t1rr k v t2)
                        | _ -> failwith "rebalance"
                      else
                        mk t1l t1k t1v (mk t1r k v t2)
                  | _ -> failwith "rebalance"
                else mk t1 k v t2

        let rec sizeAux acc m = 
            match m with  
            | MapEmpty -> acc
#if ONE 
            | MapOne _ -> acc + 1
#endif
            | MapNode(k2,v2,l,r,h) -> sizeAux (sizeAux (acc+1) l) r 

#if ONE 
#else
        let MapOne(k,v) = MapNode(k,v,MapEmpty,MapEmpty,1)
#endif
        
        let size x = sizeAux 0 x

        let rec add (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k v m = 
            match m with 
            | MapEmpty -> MapOne(k,v)
#if ONE 
            | MapOne(k2,v2) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0   then MapNode (k,v,MapEmpty,m,2)
                elif c = 0 then MapOne(k,v)
                else            MapNode (k,v,m,MapEmpty,2)
#endif
            | MapNode(k2,v2,l,r,h) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0 then rebalance (add comparer k v l) k2 v2 r
                elif c = 0 then MapNode(k,v,l,r,h)
                else rebalance l k2 v2 (add comparer k v r) 

        let indexNotFound() = raise (new System.Collections.Generic.KeyNotFoundException("An index satisfying the predicate was not found in the collection"))

        let rec find (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k m = 
            match m with 
            | MapEmpty -> indexNotFound()
#if ONE 
            | MapOne(k2,v2) -> 
                let c = comparer.Invoke(k,k2) 
                if c = 0 then v2
                else indexNotFound()
#endif
            | MapNode(k2,v2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0 then find comparer k l
                elif c = 0 then v2
                else find comparer k r

        let rec tryfind (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k m = 
            match m with 
            | MapEmpty -> None
#if ONE 
            | MapOne(k2,v2) -> 
                let c = comparer.Invoke(k,k2) 
                if c = 0 then Some v2
                else None
#endif
            | MapNode(k2,v2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0 then tryfind comparer k l
                elif c = 0 then Some v2
                else tryfind comparer k r

        let partition1 (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) f k v (acc1,acc2) = 
            if f k v then (add comparer k v acc1,acc2) else (acc1,add comparer k v acc2) 
        
        let rec partitionAux (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) f s acc = 
            match s with 
            | MapEmpty -> acc
#if ONE 
            | MapOne(k,v) -> partition1 comparer f k v acc
#endif
            | MapNode(k,v,l,r,_) -> 
                let acc = partitionAux comparer f r acc 
                let acc = partition1 comparer f k v acc
                partitionAux comparer f l acc

        let partition (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) f s = partitionAux comparer f s (empty,empty)

        let filter1 (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) f k v acc = if f k v then add comparer k v acc else acc 

        let rec filterAux (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) f s acc = 
            match s with 
            | MapEmpty -> acc
#if ONE 
            | MapOne(k,v) -> filter1 comparer f k v acc
#endif
            | MapNode(k,v,l,r,_) ->
                let acc = filterAux comparer f l acc
                let acc = filter1 comparer f k v acc
                filterAux comparer f r acc

        let filter (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) f s = filterAux comparer f s empty

        let rec spliceOutSuccessor m = 
            match m with 
            | MapEmpty -> failwith "internal error: Map.splice_out_succ_or_pred"
#if ONE 
            | MapOne(k2,v2) -> k2,v2,MapEmpty
#endif
            | MapNode(k2,v2,l,r,_) ->
                match l with 
                | MapEmpty -> k2,v2,r
                | _ -> let k3,v3,l' = spliceOutSuccessor l in k3,v3,mk l' k2 v2 r

        let rec remove (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k m = 
            match m with 
            | MapEmpty -> empty
#if ONE 
            | MapOne(k2,v2) -> 
                let c = comparer.Invoke(k,k2) 
                if c = 0 then MapEmpty else m
#endif
            | MapNode(k2,v2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0 then rebalance (remove comparer k l) k2 v2 r
                elif c = 0 then 
                  match l,r with 
                  | MapEmpty,_ -> r
                  | _,MapEmpty -> l
                  | _ -> 
                      let sk,sv,r' = spliceOutSuccessor r 
                      mk l sk sv r'
                else rebalance l k2 v2 (remove comparer k r) 

        let rec mem (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) k m = 
            match m with 
            | MapEmpty -> false
#if ONE 
            | MapOne(k2,v2) -> (comparer.Invoke(k,k2) = 0)
#endif
            | MapNode(k2,v2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0 then mem comparer k l
                else (c = 0 || mem comparer k r)

        let rec iter f m = 
            match m with 
            | MapEmpty -> ()
#if ONE 
            | MapOne(k2,v2) -> f k2 v2
#endif
            | MapNode(k2,v2,l,r,_) -> iter f l; f k2 v2; iter f r

        let rec first f m = 
            match m with 
            | MapEmpty -> None
#if ONE 
            | MapOne(k2,v2) -> f k2 v2 
#endif
            | MapNode(k2,v2,l,r,_) -> 
                match first f l with 
                | Some x as res -> res 
                | None -> 
                match f k2 v2 with 
                | Some x as res -> res 
                | None -> first f r

        let rec exists f m = 
            match m with 
            | MapEmpty -> false
#if ONE 
            | MapOne(k2,v2) -> f k2 v2
#endif
            | MapNode(k2,v2,l,r,_) -> f k2 v2 || exists f l || exists f r

        let rec for_all f m = 
            match m with 
            | MapEmpty -> true
#if ONE 
            | MapOne(k2,v2) -> f k2 v2
#endif
            | MapNode(k2,v2,l,r,_) -> f k2 v2 && for_all f l && for_all f r

        let rec map f m = 
            match m with 
            | MapEmpty -> empty
#if ONE 
            | MapOne(k,v) -> MapOne(k,f v)
#endif
            | MapNode(k,v,l,r,h) -> let v2 = f v in MapNode(k,v2,map f l, map f r,h)

        let rec mapi f m = 
            match m with
            | MapEmpty -> empty
#if ONE 
            | MapOne(k,v) -> MapOne(k,f k v)
#endif
            | MapNode(k,v,l,r,h) -> let v2 = f k v in MapNode(k,v2, mapi f l, mapi f r,h)

        // Fold, right-to-left. 
        //
        // NOTE: This matches OCaml behaviour, when last checked with OCaml 3.08.
        // However it differs from the behaviour of Set.fold which folds left-to-right.
        let rec fold f m x = 
            match m with 
            | MapEmpty -> x
#if ONE 
            | MapOne(k,v) -> f k v x
#endif
            | MapNode(k,v,l,r,h) -> fold f l (f k v (fold f r x))

        let foldSection (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) lo hi f m x =
            let rec fold_from_to f m x = 
                match m with 
                | MapEmpty -> x
#if ONE 
                | MapOne(k,v) ->
                    let clo_k = comparer.Invoke(lo,k)
                    let ck_hi = comparer.Invoke(k,hi)
                    let x = if clo_k <= 0 && ck_hi <= 0 then f k v x else x
                    x
#endif
                | MapNode(k,v,l,r,h) ->
                    let clo_k = comparer.Invoke(lo,k)
                    let ck_hi = comparer.Invoke(k,hi)
                    let x = if clo_k < 0                then fold_from_to f l x else x
                    let x = if clo_k <= 0 && ck_hi <= 0 then f k v x                     else x
                    let x = if ck_hi < 0                then fold_from_to f r x else x
                    x
           
            if comparer.Invoke(lo,hi) = 1 then x else fold_from_to f m x

        let rec fmap (comparer: OptimizedClosures.FastFunc2<'a,'a,int>) f m z acc = 
            match m with 
            | MapEmpty -> acc,z
#if ONE 
            | MapOne(k,v) -> 
                let v',z = f k v z
                add comparer k v' acc,z
#endif
            | MapNode(k,v,l,r,h) -> 
                let acc,z = fmap comparer f r z acc
                let v',z = f k v z
                let acc = add comparer k v' acc 
                fmap comparer f l z acc

        let to_list m = fold (fun k v acc -> (k,v) :: acc) m []
        let to_array m = m |> to_list |> Array.of_list
        let of_list comparer l = List.fold (fun acc (k,v) -> add comparer k v acc) empty l

        
        let rec mkFromEnumerator comparer acc (e : IEnumerator<_>) = 
            if e.MoveNext() then 
                let (x,y) = e.Current 
                mkFromEnumerator comparer (add comparer x y acc) e
            else acc
          
        let of_seq comparer (c : seq<_>) =
            use ie = c.GetEnumerator()
            mkFromEnumerator comparer empty ie 
          
        let copyToArray s (arr: _[]) i =
            let j = ref i 
            s |> iter (fun x y -> arr.[!j] <- KeyValuePair(x,y); j := !j + 1)


        /// Imperative left-to-right iterators.
        type iterator<'key,'a> = 
             { /// invariant: always collapseLHS result 
               mutable stack:  MapTree<'key,'a> list;  
               /// true when MoveNext has been called   
               mutable started : bool }

        // collapseLHS:
        // a) Always returns either [] or a list starting with SetOne.
        // b) The "fringe" of the set stack is unchanged. 
        let rec collapseLHS stack =
            match stack with
            | []                           -> []
            | MapEmpty             :: rest -> collapseLHS rest
#if ONE 
            | MapOne (k,v)         :: rest -> stack
#else
            | (MapNode(k,v,MapEmpty,MapEmpty,h)) :: rest -> stack
#endif
            | (MapNode(k,v,l,r,h)) :: rest -> collapseLHS (l :: MapOne (k,v) :: r :: rest)
          
        let mkIterator s = { stack = collapseLHS [s]; started = false }

        let not_started() = raise (new System.InvalidOperationException("Enumeration has not started. Call MoveNext."))
        let already_finished() = raise (new System.InvalidOperationException("Enumeration already finished."))

        let current i =
            if i.started then
                match i.stack with
#if ONE
                  | MapOne (k,v) :: _ -> new KeyValuePair<_,_>(k,v)
#else
                  | (MapNode(k,v,MapEmpty,MapEmpty,h)) :: rest -> new KeyValuePair<_,_>(k,v)
#endif
                  | []            -> already_finished()
                  | _             -> failwith "Please report error: Map iterator, unexpected stack for current"
            else
                not_started()

        let rec moveNext i =
          if i.started then
            match i.stack with
#if ONE
              | MapOne _ :: rest -> 
#else
              | (MapNode(_,_,MapEmpty,MapEmpty,_)) :: rest -> 
#endif
                  i.stack <- collapseLHS rest;
                  i.stack <> []
              | [] -> false
              | _ -> failwith "Please report error: Map iterator, unexpected stack for moveNext"
          else
              i.started <- true;  (* The first call to MoveNext "starts" the enumeration. *)
              i.stack <> []

        let mkIEnumerator s = 
          let i = ref (mkIterator s) 
          { new IEnumerator<_> with 
                member self.Current = current !i
            interface System.Collections.IEnumerator with
                member self.Current = box (current !i)
                member self.MoveNext() = moveNext !i
                member self.Reset() = i :=  mkIterator s
            interface System.IDisposable with 
                member self.Dispose() = ()}

    [<StructuralEquality(false); StructuralComparison(false)>]
    type Set<'a,'comparerTag> when 'comparerTag :> IComparer<'a> = 
        { comparer: OptimizedClosures.FastFunc2<'a,'a,int>;
          tree: SetTree<'a> }
        interface System.IComparable 
        interface ICollection<'a> 
        interface IEnumerable<'a> 
        interface System.Collections.IEnumerable
        override this.Equals(that) = 
            match that with
            // Cast to the exact same type as this, otherwise not equal.
            | :? Set<'a,'comparerTag> as that -> ((this :> System.IComparable).CompareTo(that) = 0)
            | _ -> false

    open RawSetOps
    module SetOps = 
        let baked comparer t = { comparer=comparer ; tree=t }
        let refresh s t      = { comparer=s.comparer; tree=t }
        let fresh comparer   = baked comparer empty

    open SetOps

    type Set<'a,'comparerTag> with 

        static member Empty(comparer: 'comparerTag) : Set<'a,'comparerTag> =  
            let comparer =  ComparisonIdentity.GetFastComparisonFunction(comparer)
            fresh comparer

        member s.Add(x) : Set<'a,'comparerTag> = refresh s (add s.comparer x s.tree)
        member s.Remove(x) : Set<'a,'comparerTag> = refresh s (remove s.comparer x s.tree)
        member s.Count = size s.tree
        member s.Contains(x) = mem s.comparer  x s.tree
        member s.Iterate(x) = iter  x s.tree
        member s.Fold f x  = fold f s.tree x

#if CHECKED
        member s.CheckBalanceInvariant = checkInvariant s.tree // diagnostics...
#endif
        member s.IsEmpty  = is_empty s.tree

        member s.Partition f  : Set<'a,'comparerTag> *  Set<'a,'comparerTag> = 
            match s.tree with 
            | SetEmpty -> s,s
            | _ -> let t1,t2 = partition s.comparer f s.tree in refresh s t1, refresh s t2

        member s.Filter f  : Set<'a,'comparerTag> = 
            match s.tree with 
            | SetEmpty -> s
            | _ -> filter s.comparer f s.tree |> refresh s

        member s.Exists f = exists f s.tree

        member s.ForAll f = for_all f s.tree

        static member (-) ((a: Set<'a,'comparerTag>),(b: Set<'a,'comparerTag>)) = Set<_,_>.Difference(a,b)

        static member (+)  ((a: Set<'a,'comparerTag>),(b: Set<'a,'comparerTag>)) = Set<_,_>.Union(a,b)

        static member Intersection((a: Set<'a,'comparerTag>),(b: Set<'a,'comparerTag>)) : Set<'a,'comparerTag>  = 
            match b.tree with 
            | SetEmpty -> b  (* A INTER 0 = 0 *)
            | _ -> 
               match a.tree with 
               | SetEmpty -> a (* 0 INTER B = 0 *)
               | _ -> intersection a.comparer  a.tree b.tree |> refresh a
           
        static member Union((a: Set<'a,'comparerTag>),(b: Set<'a,'comparerTag>)) : Set<'a,'comparerTag>  = 
            match b.tree with 
            | SetEmpty -> a  (* A U 0 = A *)
            | _ -> 
               match a.tree with 
               | SetEmpty -> b  (* 0 U B = B *)
               | _ -> union a.comparer  a.tree b.tree |> refresh a

        static member Difference((a: Set<'a,'comparerTag>),(b: Set<'a,'comparerTag>)) : Set<'a,'comparerTag>  = 
            match a.tree with 
            | SetEmpty -> a (* 0 - B = 0 *)
            | _ -> 
                match b.tree with 
                | SetEmpty -> a (* A - 0 = A *)
                | _ -> diff a.comparer  a.tree b.tree |> refresh a

        static member Equality((a: Set<'a,'comparerTag>),(b: Set<'a,'comparerTag>)) = 
            (RawSetOps.compare a.comparer  a.tree b.tree = 0)

        static member Compare((a: Set<'a,'comparerTag>),(b: Set<'a,'comparerTag>)) = 
            RawSetOps.compare a.comparer  a.tree b.tree

        member x.Choose = choose x.tree

        member x.MinimumElement = minimumElement x.tree

        member x.MaximumElement = maximumElement x.tree

        member x.IsSubsetOf((y: Set<'a,'comparerTag>)) = subset x.comparer x.tree y.tree 

        member x.IsSupersetOf((y: Set<'a,'comparerTag>)) = subset x.comparer y.tree x.tree

        member x.ToList () = to_list x.tree

        member x.ToArray () = to_array x.tree

        interface System.IComparable with
            // Cast s2 to the exact same type as s1, see 4884.
            // It is not OK to cast s2 to seq<'a>, since different compares could permute the elements.
            member s1.CompareTo(s2: obj) = RawSetOps.compare s1.comparer s1.tree ((s2 :?> Set<'a,'comparerTag>).tree)

        member this.ComputeHashCode() = 
                let combineHash x y = (x <<< 1) + y + 631 
                let mutable res = 0
                for x in this do
                    res <- combineHash res (hash x)
                abs res

        override this.GetHashCode() = this.ComputeHashCode()
          
        interface ICollection<'a> with 
            member s.Add(x) = raise (new System.NotSupportedException("ReadOnlyCollection"))
            member s.Clear() = raise (new System.NotSupportedException("ReadOnlyCollection"))
            member s.Remove(x) = raise (new System.NotSupportedException("ReadOnlyCollection"))
            member s.Contains(x) = mem s.comparer x s.tree
            member s.CopyTo(arr,i) = copyToArray s.tree arr i
            member s.get_IsReadOnly() = true
            member s.get_Count() = cardinal s.tree  

        interface IEnumerable<'a> with
            member s.GetEnumerator() = mkIEnumerator s.tree

        interface System.Collections.IEnumerable with
            override s.GetEnumerator() = (mkIEnumerator s.tree :> System.Collections.IEnumerator)

        static member Singleton(comparer,x) : Set<'a,'comparerTag>  = 
            Set<_,_>.Empty(comparer).Add(x)

        static member Create(comparer : 'comparerTag,l : seq<'a>) : Set<'a,'comparerTag> = 
            let comparer =  ComparisonIdentity.GetFastComparisonFunction(comparer)
            baked comparer (of_seq comparer l)


    open RawMapOps

#if FX_NO_DEBUG_DISPLAYS
#else
    [<System.Diagnostics.DebuggerDisplay ("Count = {Count}")>]
#endif
    [<StructuralEquality(false); StructuralComparison(false)>]
    type Map<'key,'a,'comparerTag> when 'comparerTag :> IComparer<'key> =  
        { comparer: OptimizedClosures.FastFunc2<'key,'key,int>; 
          tree: MapTree<'key,'a> }
        interface System.IComparable
        interface IEnumerable<KeyValuePair<'key, 'a>>
        interface System.Collections.IEnumerable
        override this.Equals(that) = 
            match that with
            // Cast to the exact same type as this, otherwise not equal.
            | :? Map<'key,'a,'comparerTag> as that -> ((this :> System.IComparable).CompareTo(that) = 0)
            | _ -> false

    module MapOps = 
        let baked comparer t =     { comparer=comparer; tree=t }
        let fresh comparer =       baked comparer empty 
        let refresh s t =    { comparer=s.comparer;tree=t }

    open MapOps 

    type Map<'key,'a,'comparerTag> with 
        static member Empty(comparer : ('comparerTag :> IComparer<'key>)) : Map<'key,'a,'comparerTag> = 
            let comparer =  ComparisonIdentity.GetFastComparisonFunction(comparer)
            fresh comparer
        member m.Add(k,v) : Map<'key,'a,'comparerTag> = refresh m (add m.comparer k v m.tree)
        member m.IsEmpty = is_empty m.tree
        member m.Item with get(k : 'key) = find m.comparer k m.tree
        member m.First(f) = first f m.tree 
        member m.Exists(f) = exists f m.tree 
        member m.Filter(f)  : Map<'key,'a,'comparerTag> = filter m.comparer f m.tree |> refresh m 
        member m.ForAll(f) = for_all f m.tree 
        member m.Fold f acc = fold f m.tree acc
        member m.FoldSection (lo:'key) (hi:'key) f (acc:'z) = foldSection m.comparer lo hi f m.tree acc 
        member m.FoldAndMap f z  : Map<'key,'b,'comparerTag> * _ = let tree,z = fmap m.comparer f m.tree z empty in refresh m tree,z
        member m.Iterate f = iter f m.tree
        member m.MapRange f  : Map<'key,'b,'comparerTag> = refresh m (map f m.tree)
        member m.Map f  : Map<'key,'b,'comparerTag> = refresh m (mapi f m.tree)
        member m.Partition(f)  : Map<'key,'a,'comparerTag> * Map<'key,'a,'comparerTag> = let r1,r2 = partition m.comparer f m.tree  in refresh m r1, refresh m r2
        member m.Count = size m.tree
        member m.ContainsKey(k) = mem m.comparer k m.tree
        member m.Remove(k)  : Map<'key,'a,'comparerTag> = refresh m (remove m.comparer k m.tree)
        member m.TryFind(k) = tryfind m.comparer k m.tree
        member m.ToList() = to_list m.tree
        member m.ToArray() = to_array m.tree

        static member FromList((comparer : ('comparerTag :> IComparer<'key>)),l) : Map<'key,'a,'comparerTag> = 
            let comparer =  ComparisonIdentity.GetFastComparisonFunction(comparer)
            baked comparer (of_list comparer l)

        static member Create((comparer : ('comparerTag :> IComparer<'key>)),(ie : IEnumerable<_>)) : Map<'key,'a,'comparerTag> = 
            let comparer =  ComparisonIdentity.GetFastComparisonFunction(comparer)
            baked comparer (of_seq comparer ie)
    
        interface IEnumerable<KeyValuePair<'key, 'a>> with
            member s.GetEnumerator() = mkIEnumerator s.tree

        interface System.Collections.IEnumerable with
            override s.GetEnumerator() = (mkIEnumerator s.tree :> System.Collections.IEnumerator)

        interface System.IComparable with 
             member m1.CompareTo(m2: obj) = 
                 Seq.compare 
                   (fun (kvp1 : KeyValuePair<_,_>) (kvp2 : KeyValuePair<_,_>)-> 
                       let c = m1.comparer.Invoke(kvp1.Key,kvp2.Key) in 
                       if c <> 0 then c else Operators.compare kvp1.Value kvp2.Value)
                   // Cast m2 to the exact same type as m1, see 4884.
                   // It is not OK to cast m2 to seq<KeyValuePair<'key,'a>>, since different compares could permute the KVPs.
                   m1 (m2 :?> Map<'key,'a,'comparerTag>)

        member this.ComputeHashCode() = 
            let combineHash x y = (x <<< 1) + y + 631 
            let mutable res = 0
            for KeyValue(x,y) in this do
                res <- combineHash res (hash x)
                res <- combineHash res (hash y)
            abs res

        override this.GetHashCode() = this.ComputeHashCode()


    type Map<'key,'a> = Map<'key, 'a, IComparer<'key>>    
    type Set<'a> = Set<'a, IComparer<'a>>    

