//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Collections

    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Collections
    open System.Collections
    open System.Collections.Generic
    open System.Diagnostics

    (* A classic functional language implementation of binary trees *)

    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    type (* internal *) SetTree<'T> = 
        | SetEmpty                                          // height = 0   
        | SetNode of 'T * SetTree<'T> *  SetTree<'T> * int    // height = int 
        | SetOne  of 'T                                     // height = 1   
            // OPTIMIZATION: store SetNode(k,SetEmpty,SetEmpty,1) --->  SetOne(k) 
            // REVIEW: performance rumour has it that the data held in SetNode and SetOne should be
            // exactly one cache line on typical architectures. They are currently 
            // ~6 and 3 words respectively. 

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module internal SetTree = 

        let height t = 
            match t with 
            | SetEmpty -> 0
            | SetOne _ -> 1
            | SetNode (_,_,_,h) -> h

    #if CHECKED
        let rec checkInvariant t =
            // A good sanity check, loss of balance can hit perf 
            match t with 
            | SetEmpty -> true
            | SetOne _ -> true
            | SetNode (k,t1,t2,h) ->
                let h1 = height t1 
                let h2 = height t2 
                (-2 <= (h1 - h2) && (h1 - h2) <= 2) && checkInvariant t1 && checkInvariant t2
    #endif

        let tolerance = 2

        let mk l k r = 
            match l,r with 
            | SetEmpty,SetEmpty -> SetOne (k)
            | _ -> 
              let hl = height l 
              let hr = height r 
              let m = if hl < hr then hr else hl 
              SetNode(k,l,r,m+1)

        let rebalance t1 k t2 =
            let t1h = height t1 
            let t2h = height t2 
            if  t2h > t1h + tolerance then // right is heavier than left 
                match t2 with 
                | SetNode(t2k,t2l,t2r,t2h) -> 
                    // one of the nodes must have height > height t1 + 1 
                    if height t2l > t1h + 1 then  // balance left: combination 
                        match t2l with 
                        | SetNode(t2lk,t2ll,t2lr,t2lh) ->
                            mk (mk t1 k t2ll) t2lk (mk t2lr t2k t2r) 
                        | _ -> failwith "rebalance"
                    else // rotate left 
                        mk (mk t1 k t2l) t2k t2r
                | _ -> failwith "rebalance"
            else
                if  t1h > t2h + tolerance then // left is heavier than right 
                    match t1 with 
                    | SetNode(t1k,t1l,t1r,t1h) -> 
                        // one of the nodes must have height > height t2 + 1 
                        if height t1r > t2h + 1 then 
                            // balance right: combination 
                            match t1r with 
                            | SetNode(t1rk,t1rl,t1rr,t1rh) ->
                                mk (mk t1l t1k t1rl) t1rk (mk t1rr k t2)
                            | _ -> failwith "rebalance"
                        else
                            mk t1l t1k (mk t1r k t2)
                    | _ -> failwith "rebalance"
                else mk t1 k t2

        let rec add (comparer: OptimizedClosures.FastFunc2<'T,'T,int>) k t = 
            match t with 
            | SetNode (k2,l,r,h) -> 
                let c = comparer.Invoke(k,k2) 
                if   c < 0 then rebalance (add comparer k l) k2 r
                elif c = 0 then t
                else            rebalance l k2 (add comparer k r)
            | SetOne(k2) -> 
                // nb. no check for rebalance needed for small trees, also be sure to reuse node already allocated 
                let c = comparer.Invoke(k,k2) 
                if c < 0   then SetNode (k,SetEmpty,t,2)
                elif c = 0 then t
                else            SetNode (k,t,SetEmpty,2)                  
            | SetEmpty -> SetOne(k)

        let rec balance comparer t1 k t2 =
            // Given t1 < k < t2 where t1 and t2 are "balanced",
            // return a balanced tree for <t1,k,t2>.
            // Recall: balance means subtrees heights differ by at most "tolerance"
            match t1,t2 with
            | SetEmpty,t2  -> add comparer k t2 // drop t1 = empty 
            | t1,SetEmpty  -> add comparer k t1 // drop t2 = empty 
            | SetOne k1,t2 -> add comparer k (add comparer k1 t2)
            | t1,SetOne k2 -> add comparer k (add comparer k2 t1)
            | SetNode(k1,t11,t12,h1),SetNode(k2,t21,t22,h2) ->
                // Have:  (t11 < k1 < t12) < k < (t21 < k2 < t22)
                // Either (a) h1,h2 differ by at most 2 - no rebalance needed.
                //        (b) h1 too small, i.e. h1+2 < h2
                //        (c) h2 too small, i.e. h2+2 < h1 
                if   h1+tolerance < h2 then
                    // case: b, h1 too small 
                    // push t1 into low side of t2, may increase height by 1 so rebalance 
                    rebalance (balance comparer t1 k t21) k2 t22
                elif h2+tolerance < h1 then
                    // case: c, h2 too small 
                    // push t2 into high side of t1, may increase height by 1 so rebalance 
                    rebalance t11 k1 (balance comparer t12 k t2)
                else
                    // case: a, h1 and h2 meet balance requirement 
                    mk t1 k t2

        let rec split (comparer : OptimizedClosures.FastFunc2<'T,'T,int>) pivot t =
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
            | SetOne k1 ->
                let c = comparer.Invoke(k1,pivot)
                if   c < 0 then t       ,false,SetEmpty // singleton under pivot 
                elif c = 0 then SetEmpty,true ,SetEmpty // singleton is    pivot 
                else            SetEmpty,false,t        // singleton over  pivot 
            | SetEmpty  -> 
                SetEmpty,false,SetEmpty
        
        let rec spliceOutSuccessor t = 
            match t with 
            | SetEmpty -> failwith "internal error: Map.splice_out_succ_or_pred"
            | SetOne (k2) -> k2,SetEmpty
            | SetNode (k2,l,r,_) ->
                match l with 
                | SetEmpty -> k2,r
                | _ -> let k3,l' = spliceOutSuccessor l in k3,mk l' k2 r

        let rec remove (comparer: OptimizedClosures.FastFunc2<'T,'T,int>) k t = 
            match t with 
            | SetEmpty -> t
            | SetOne (k2) -> 
                let c = comparer.Invoke(k,k2) 
                if   c = 0 then SetEmpty
                else            t
            | SetNode (k2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if   c < 0 then rebalance (remove comparer k l) k2 r
                elif c = 0 then 
                  match l,r with 
                  | SetEmpty,_ -> r
                  | _,SetEmpty -> l
                  | _ -> 
                      let sk,r' = spliceOutSuccessor r 
                      mk l sk r'
                else rebalance l k2 (remove comparer k r) 

        let rec mem (comparer: OptimizedClosures.FastFunc2<'T,'T,int>) k t = 
            match t with 
            | SetNode(k2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if   c < 0 then mem comparer k l
                elif c = 0 then true
                else mem comparer k r
            | SetOne(k2) -> (comparer.Invoke(k,k2) = 0)
            | SetEmpty -> false

        let rec iter f t = 
            match t with 
            | SetNode(k2,l,r,_) -> iter f l; f k2; iter f r
            | SetOne(k2) -> f k2
            | SetEmpty -> ()            

        let rec fold_right f m x = 
            match m with 
            | SetNode(k,l,r,h) -> fold_right f l (f k (fold_right f r x))
            | SetOne(k) -> f k x
            | SetEmpty -> x

        let rec fold_left f x m = 
            match m with 
            | SetNode(k,l,r,h) -> 
                let x = fold_left f x l in 
                let x = f x k
                fold_left f x r
            | SetOne(k) -> f x k
            | SetEmpty -> x

        let rec for_all f m = 
            match m with 
            | SetNode(k2,l,r,h) -> f k2 && for_all f l && for_all f r
            | SetOne(k2) -> f k2
            | SetEmpty -> true          

        let rec exists f m = 
            match m with 
            | SetNode(k2,l,r,h) -> f k2 || exists f l || exists f r
            | SetOne(k2) -> f k2
            | SetEmpty -> false         

        let is_empty m = match m with  | SetEmpty -> true | _ -> false

        let subset comparer a b  = for_all (fun x -> mem comparer x b) a

        let rec filterAux comparer f s acc = 
            match s with 
            | SetNode(k,l,r,_) -> 
                let acc = if f k then add comparer k acc else acc 
                filterAux comparer f l (filterAux comparer f r acc)
            | SetOne(k) -> if f k then add comparer k acc else acc
            | SetEmpty -> acc           

        let filter comparer f s = filterAux comparer f s SetEmpty

        let rec diffAux comparer m acc = 
            match m with 
            | SetNode(k,l,r,_) -> diffAux comparer l (diffAux comparer r (remove comparer k acc))
            | SetOne(k) -> remove comparer k acc
            | SetEmpty -> acc           

        let diff comparer a b = diffAux comparer b a

        let rec countAux s acc = 
            match s with 
            | SetNode(k,l,r,_) -> countAux l (countAux r (acc+1))
            | SetOne(k) -> acc+1
            | SetEmpty -> acc           

        let count s = countAux s 0

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
            | SetOne k1,t2 -> add comparer k1 t2
            | t1,SetOne k2 -> add comparer k2 t1

        let rec intersectionAux comparer b m acc = 
            match m with 
            | SetNode(k,l,r,_) -> 
                let acc = intersectionAux comparer b r acc 
                let acc = if mem comparer k b then add comparer k acc else acc 
                intersectionAux comparer b l acc
            | SetOne(k) -> 
                if mem comparer k b then add comparer k acc else acc
            | SetEmpty -> acc

        let intersection comparer a b = intersectionAux comparer b a SetEmpty

        let partition1 comparer f k (acc1,acc2) = if f k then (add comparer k acc1,acc2) else (acc1,add comparer k acc2) 
        
        let rec partitionAux comparer f s acc = 
            match s with 
            | SetNode(k,l,r,_) -> 
                let acc = partitionAux comparer f r acc 
                let acc = partition1 comparer f k acc
                partitionAux comparer f l acc
            | SetOne(k) -> partition1 comparer f k acc
            | SetEmpty -> acc           

        let partition comparer f s = partitionAux comparer f s (SetEmpty,SetEmpty)

        // It's easier to get many less-important algorithms right using this active pattern
        let (|MatchSetNode|MatchSetEmpty|) s = 
            match s with 
            | SetNode(k2,l,r,_) -> MatchSetNode(k2,l,r)
            | SetOne(k2) -> MatchSetNode(k2,SetEmpty,SetEmpty)
            | SetEmpty -> MatchSetEmpty
        
        let rec nextElemCont (comparer: OptimizedClosures.FastFunc2<'T,'T,int>) k s cont = 
            match s with 
            | MatchSetNode(k2,l,r) -> 
                let c = comparer.Invoke(k,k2) 
                if   c < 0 then nextElemCont comparer k l (function None -> cont(Some(k2)) | res -> res)
                elif c = 0 then cont(minimumElementOpt r) 
                else nextElemCont comparer k r cont
            | MatchSetEmpty -> cont(None)

        and nextElem comparer k s = nextElemCont comparer k s (fun res -> res)
        
        and prevElemCont (comparer: OptimizedClosures.FastFunc2<'T,'T,int>) k s cont = 
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
            | SetOne(k) -> k
            | SetEmpty -> n

        and minimumElementOpt s = 
            match s with 
            | SetNode(k,l,r,_) -> Some(minimumElementAux l k)
            | SetOne(k) -> Some k
            | SetEmpty -> None

        and maximumElementAux s n = 
            match s with 
            | SetNode(k,l,r,_) -> maximumElementAux r k
            | SetOne(k) -> k
            | SetEmpty -> n             

        and maximumElementOpt s = 
            match s with 
            | SetNode(k,l,r,_) -> Some(maximumElementAux r k)
            | SetOne(k) -> Some(k)
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

        [<StructuralEquality(false); StructuralComparison(false)>]
        type iterator<'T> = 
            { mutable stack: SetTree<'T> list;  // invariant: always collapseLHS result 
              mutable started : bool           // true when MoveNext has been called   
            }

        // collapseLHS:
        // a) Always returns either [] or a list starting with SetOne.
        // b) The "fringe" of the set stack is unchanged.
        let rec collapseLHS stack =
            match stack with
            | []                       -> []
            | SetEmpty         :: rest -> collapseLHS rest
            | SetOne k         :: rest -> stack
            | SetNode(k,l,r,h) :: rest -> collapseLHS (l :: SetOne k :: r :: rest)
          
        let mkIterator s = { stack = collapseLHS [s]; started = false }

        let not_started() = raise (new System.InvalidOperationException("Enumeration has not started. Call MoveNext."))
        let already_finished() = raise (new System.InvalidOperationException("Enumeration already finished."))

        let current i =
            if i.started then
                match i.stack with
                  | SetOne k :: _ -> k
                  | []            -> already_finished()
                  | _             -> failwith "Please report error: Set iterator, unexpected stack for current"
            else
                not_started()

        let rec moveNext i =
            if i.started then
                match i.stack with
                  | SetOne k :: rest -> ( i.stack <- collapseLHS rest;
                                          i.stack <> []
                                        )
                  | [] -> false
                  | _ -> failwith "Please report error: Set iterator, unexpected stack for moveNext"
            else
                i.started <- true;  // The first call to MoveNext "starts" the enumeration.
                i.stack <> []

        let mkIEnumerator s = 
            let i = ref (mkIterator s) 
            { new IEnumerator<_> with 
                  member x.Current = current !i
              interface IEnumerator with 
                  member x.Current = box (current !i)
                  member x.MoveNext() = moveNext !i
                  member x.Reset() = i :=  mkIterator s
              interface System.IDisposable with 
                  member x.Dispose() = () }

        //--------------------------------------------------------------------------
        // Set comparison.  This can be expensive.
        //--------------------------------------------------------------------------

        let rec compareStacks (comparer: OptimizedClosures.FastFunc2<'T,'T,int>) l1 l2 =
            match l1,l2 with 
            | [],[] ->  0
            | [],_  -> -1
            | _ ,[] ->  1
            | (SetEmpty  _ :: t1),(SetEmpty    :: t2) -> compareStacks comparer t1 t2
            | (SetOne(n1k) :: t1),(SetOne(n2k) :: t2) -> 
                 let c = comparer.Invoke(n1k,n2k) 
                 if c <> 0 then c else compareStacks comparer t1 t2
            | (SetOne(n1k) :: t1),(SetNode(n2k,SetEmpty,n2r,_) :: t2) -> 
                 let c = comparer.Invoke(n1k,n2k) 
                 if c <> 0 then c else compareStacks comparer (SetEmpty :: t1) (n2r :: t2)
            | (SetNode(n1k,(SetEmpty as emp),n1r,_) :: t1),(SetOne(n2k) :: t2) -> 
                 let c = comparer.Invoke(n1k,n2k) 
                 if c <> 0 then c else compareStacks comparer (n1r :: t1) (emp :: t2)
            | (SetNode(n1k,(SetEmpty as emp),n1r,_) :: t1),(SetNode(n2k,SetEmpty,n2r,_) :: t2) -> 
                 let c = comparer.Invoke(n1k,n2k) 
                 if c <> 0 then c else compareStacks comparer (n1r :: t1) (n2r :: t2)
            | (SetOne(n1k) :: t1),_ -> 
                compareStacks comparer (SetEmpty :: SetOne(n1k) :: t1) l2
            | (SetNode(n1k,n1l,n1r,_) :: t1),_ -> 
                compareStacks comparer (n1l :: SetNode(n1k,SetEmpty,n1r,0) :: t1) l2
            | _,(SetOne(n2k) :: t2) -> 
                compareStacks comparer l1 (SetEmpty :: SetOne(n2k) :: t2)
            | _,(SetNode(n2k,n2l,n2r,_) :: t2) -> 
                compareStacks comparer l1 (n2l :: SetNode(n2k,SetEmpty,n2r,0) :: t2)
                
        let compare comparer s1 s2 = 
            match s1,s2 with 
            | SetEmpty,SetEmpty -> 0
            | SetEmpty,_ -> -1
            | _,SetEmpty -> 1
            | _ -> compareStacks comparer [s1] [s2]

        let choose s = minimumElement s

        let to_list s = 
            let rec loop m acc = 
                match m with 
                | SetNode(k,l,r,h) -> loop l (k :: loop r acc)
                | SetOne(k) ->  k ::acc
                | SetEmpty -> acc
            loop s []

        let copyToArray s (arr: _[]) i =
            let j = ref i 
            iter (fun x -> arr.[!j] <- x; j := !j + 1) s

        let to_array s = 
            let n = (count s) 
            let res = Array.zeroCreate n 
            copyToArray s res 0;
            res



        let rec mkFromEnumerator comparer acc (e : IEnumerator<_>) = 
          if e.MoveNext() then 
            mkFromEnumerator comparer (add comparer e.Current acc) e
          else acc
          
        let of_seq comparer (c : IEnumerable<_>) =
          use ie = c.GetEnumerator()
          mkFromEnumerator comparer SetEmpty ie 

        let of_array comparer l = Array.fold (fun acc k -> add comparer k acc) SetEmpty l    


    [<Sealed>]
#if FX_NO_DEBUG_PROXIES
#else
    [<System.Diagnostics.DebuggerTypeProxy(typedefof<SetDebugView<_>>)>]
#endif
#if FX_NO_DEBUG_DISPLAYS
#else
    [<System.Diagnostics.DebuggerDisplay("Count = {Count}")>]
#endif
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Set")>]
    
    type Set<'T>(comparer:OptimizedClosures.FastFunc2<'T,'T,int>, tree: SetTree<'T>) = 

        // We use .NET generics per-instantiation static fields to avoid allocating a new object for each empty
        // set (it is just be a lookup into a .NET table of type-instantiation-indexed static fields).

        static let empty : Set<'T> = 
                 let comparer = ComparisonIdentity.GetFastStructuralComparisonFunction<'T>() 
                 new Set<'T>(comparer,SetEmpty)

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member internal set.Comparer = comparer
        //[<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
        member internal set.Tree : SetTree<'T> = tree

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        static member Empty :Set<'T> = empty

        member s.Add(x) : Set<'T> = Set<'T>(s.Comparer,SetTree.add s.Comparer x s.Tree )
        member s.Remove(x) : Set<'T> = Set<'T>(s.Comparer,SetTree.remove s.Comparer x s.Tree)
        member s.Count = SetTree.count s.Tree
        member s.Contains(x) = SetTree.mem s.Comparer  x s.Tree
        member s.Iterate(x) = SetTree.iter  x s.Tree
        member s.Fold f z  = SetTree.fold_left (fun x z -> f z x) z s.Tree 

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member s.IsEmpty  = SetTree.is_empty s.Tree

        member s.Partition f  : Set<'T> *  Set<'T> = 
            match s.Tree with 
            | SetEmpty -> s,s
            | _ -> let t1,t2 = SetTree.partition s.Comparer f s.Tree in Set<_>(s.Comparer,t1), Set<_>(s.Comparer,t2)

        member s.Filter f  : Set<'T> = 
            match s.Tree with 
            | SetEmpty -> s
            | _ -> Set<_>(s.Comparer,SetTree.filter s.Comparer f s.Tree)

        member s.Map f  : Set<'U> = 
            let comparer = ComparisonIdentity.GetFastStructuralComparisonFunction<'U>()
            Set<_>(comparer,SetTree.fold_left (fun acc k -> SetTree.add comparer (f k) acc) (SetTree<_>.SetEmpty) s.Tree)

        member s.Exists f = SetTree.exists f s.Tree

        member s.forall f = SetTree.for_all f s.Tree

        static member (-) (a: Set<'T>, b: Set<'T>) = Set<_>.Subtract(a,b)

        [<System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates")>]
        static member (+) (a: Set<'T>, b: Set<'T>) = Set<_>.Union(a,b)

        static member Intersection(a: Set<'T>, b: Set<'T>) : Set<'T>  = 
            match b.Tree with 
            | SetEmpty -> b  (* A INTER 0 = 0 *)
            | _ -> 
            match a.Tree with 
            | SetEmpty -> a (* 0 INTER B = 0 *)
            | _ -> Set<_>(a.Comparer,SetTree.intersection a.Comparer a.Tree b.Tree)
           
        static member Union(a: Set<'T>, b: Set<'T>) : Set<'T>  = 
            match b.Tree with 
            | SetEmpty -> a  (* A U 0 = A *)
            | _ -> 
            match a.Tree with 
            | SetEmpty -> b  (* 0 U B = B *)
            | _ -> Set<_>(a.Comparer,SetTree.union a.Comparer  a.Tree b.Tree)

        static member Union(sets:seq<Set<'T>>) : Set<'T>  = 
            Seq.fold (fun s1 s2 -> Set<_>.Union(s1,s2)) Set<'T>.Empty sets

        static member Intersection(sets:seq<Set<'T>>) : Set<'T>  = 
            Seq.reduce (fun s1 s2 -> Set<_>.Intersection(s1,s2)) sets

        static member Subtract(a: Set<'T>, b: Set<'T>) : Set<'T>  = 
            match a.Tree with 
            | SetEmpty -> a (* 0 - B = 0 *)
            | _ -> 
            match b.Tree with 
            | SetEmpty -> a (* A - 0 = A *)
            | _ -> Set<_>(a.Comparer,SetTree.diff a.Comparer  a.Tree b.Tree)

        static member Equality(a: Set<'T>, b: Set<'T>) = (SetTree.compare a.Comparer  a.Tree b.Tree = 0)

        static member Compare(a: Set<'T>, b: Set<'T>) = SetTree.compare a.Comparer  a.Tree b.Tree

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member x.Choose = SetTree.choose x.Tree

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member x.MinimumElement = SetTree.minimumElement x.Tree

#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member x.MaximumElement = SetTree.maximumElement x.Tree
        member x.GetNextElement(e) = SetTree.nextElem x.Comparer e x.Tree
        member x.GetPreviousElement(e) = SetTree.prevElem x.Comparer  e x.Tree

        member x.IsSubsetOf(y: Set<'T>) = SetTree.subset x.Comparer x.Tree y.Tree 
        member x.IsSupersetOf(y: Set<'T>) = SetTree.subset x.Comparer y.Tree x.Tree
        member x.ToList () = SetTree.to_list x.Tree
        member x.ToArray () = SetTree.to_array x.Tree

        member this.ComputeHashCode() = 
            let combineHash x y = (x <<< 1) + y + 631 
            let mutable res = 0
            for x in this do
                res <- combineHash res (hash x)
            abs res

        override this.GetHashCode() = this.ComputeHashCode()

        override this.Equals(that) = 
            match that with 
            | :? Set<'T> as that -> 
                 ((this :> System.IComparable).CompareTo(that) = 0)
            | _ -> false

        interface System.IComparable with 
            member this.CompareTo(that: obj) = SetTree.compare this.Comparer this.Tree ((that :?> Set<'T>).Tree)
          
        interface ICollection<'T> with 
            member s.Add(x)      = raise (new System.NotSupportedException("ReadOnlyCollection"))
            member s.Clear()     = raise (new System.NotSupportedException("ReadOnlyCollection"))
            member s.Remove(x)   = raise (new System.NotSupportedException("ReadOnlyCollection"))
            member s.Contains(x) = SetTree.mem s.Comparer x s.Tree
            member s.CopyTo(arr,i) = SetTree.copyToArray s.Tree arr i
            member s.get_IsReadOnly() = true
            member s.get_Count() = SetTree.count s.Tree  

        interface IEnumerable<'T> with
            member s.GetEnumerator() = SetTree.mkIEnumerator s.Tree

        interface IEnumerable with
            override s.GetEnumerator() = (SetTree.mkIEnumerator s.Tree :> IEnumerator)

        static member Singleton(x:'T) : Set<'T> = Set<'T>.Empty.Add(x)

        new (elements : seq<'T>) = 
            let comparer = ComparisonIdentity.GetFastStructuralComparisonFunction<'T>()
            Set<_>(comparer,SetTree.of_seq comparer elements)
          
        static member Create(elements : seq<'T>) =  Set<'T>(elements)
          
        static member FromArray(arr : 'T array) : Set<'T> = 
            let comparer = ComparisonIdentity.GetFastStructuralComparisonFunction<'T>()
            Set<_>(comparer,SetTree.of_array comparer arr)

    and 
        [<Sealed>]
        SetDebugView<'T>(v: Set<'T>)  =  

#if FX_NO_DEBUG_DISPLAYS
#else
             [<System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)>]
#endif
             member x.Items = v |> Seq.truncate 1000 |> Seq.to_array 

namespace Microsoft.FSharp.Collections

    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Collections
    open System.Collections
    open System.Collections.Generic
    open System.Diagnostics

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Set = 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let isEmpty (s : Set<'T>) = s.IsEmpty

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let is_empty (s : Set<'T>) = s.IsEmpty

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let contains x (s : Set<'T>) = s.Contains(x)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let mem x (s : Set<'T>) = s.Contains(x)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let add x (s : Set<'T>) = s.Add(x)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let singleton x = Set<'T>.Singleton(x)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let remove x (s : Set<'T>) = s.Remove(x)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let union (s1 : Set<'T>)  (s2 : Set<'T>)  = Set<'T>.Union(s1,s2)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let unionMany sets  = Set<_>.Union(sets)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let union_all sets  = unionMany sets

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let intersect (s1 : Set<'T>)  (s2 : Set<'T>)  = Set<'T>.Intersection(s1,s2)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let intersectMany sets  = Set<_>.Intersection(sets)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let intersect_all sets  = intersectMany sets

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iter f (s : Set<'T>)  = s.Iterate(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let empty<'T> : Set<'T> = Set<'T>.Empty

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let forall f (s : Set<'T>) = s.forall f

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let for_all f (s : Set<'T>) = s.forall f

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let exists f (s : Set<'T>) = s.Exists f

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let filter f (s : Set<'T>) = s.Filter f

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let partition f (s : Set<'T>) = s.Partition f 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold f z (s : Set<'T>) = SetTree.fold_left f z s.Tree

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold_left f z s = fold f z s

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let foldBack f (s : Set<'T>) z = SetTree.fold_right f s.Tree z

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold_right f s z = foldBack f s z

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let map f (s : Set<'T>) = s.Map f

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let count (s : Set<'T>) = s.Count

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let minimumElement (s : Set<'T>) = s.MinimumElement

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let maximumElement (s : Set<'T>) = s.MaximumElement

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let choose (s : Set<'T>) = s.Choose

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let of_list l = Set<_>(List.to_seq l)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let of_array (l : 'T array) = Set<'T>.FromArray(l)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let to_list (s : Set<'T>) = s.ToList()

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let to_array (s : Set<'T>) = s.ToArray()

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let to_seq (s : Set<'T>) = (s :> seq<'T>)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let of_seq (c : seq<_>) = Set<_>(c)


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let next_elt x (s : Set<'T>) = s.GetNextElement(x)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let prev_elt x (s : Set<'T>) = s.GetPreviousElement(x)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let min_elt (s : Set<'T>) = s.MinimumElement

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let max_elt (s : Set<'T>) = s.MaximumElement

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let size (s: Set<'T>) = s.Count

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let compare (s1: Set<'T>) (s2: Set<'T>) = Set<_>.Compare(s1,s2)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let subset (s1: Set<'T>) (s2: Set<'T>) = s1.IsSubsetOf(s2)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let diff (s1: Set<'T>) (s2: Set<'T>) = s1 - s2
