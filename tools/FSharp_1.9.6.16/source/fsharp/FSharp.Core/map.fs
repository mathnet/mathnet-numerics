// (c) Microsoft Corporation 2005-2008.

#light

namespace Microsoft.FSharp.Collections

    open System
    open System.Collections.Generic
    open System.Diagnostics
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Primitives.Basics

    [<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
    type MapTree<'Key,'Value> = 
        | MapEmpty 
        | MapOne of 'Key * 'Value
        | MapNode of 'Key * 'Value * MapTree<'Key,'Value> *  MapTree<'Key,'Value> * int
            // REVIEW: performance rumour has it that the data held in MapNode and MapOne should be
            // exactly one cache line. It is currently ~7 and 4 words respectively. 

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module MapTree = 

        let empty = MapEmpty 

        let height  = function
          | MapEmpty -> 0
          | MapOne _ -> 1
          | MapNode(_,_,_,_,h) -> h

        let is_empty m = 
            match m with 
            | MapEmpty -> true
            | _ -> false

        let mk l k v r = 
            match l,r with 
            | MapEmpty,MapEmpty -> MapOne(k,v)
            | _ -> 
                let hl = height l 
                let hr = height r 
                let m = if hl < hr then hr else hl 
                MapNode(k,v,l,r,m+1)

        let rebalance t1 k v t2 =
            let t1h = height t1 
            let t2h = height t2 
            if  t2h > t1h + 2 then (* right is heavier than left *)
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
            | MapOne _ -> acc + 1
            | MapNode(k2,v2,l,r,h) -> sizeAux (sizeAux (acc+1) l) r 

        let size x = sizeAux 0 x

        let rec add (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) k v m = 
            match m with 
            | MapEmpty -> MapOne(k,v)
            | MapOne(k2,v2) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0   then MapNode (k,v,MapEmpty,m,2)
                elif c = 0 then MapOne(k,v)
                else            MapNode (k,v,m,MapEmpty,2)
            | MapNode(k2,v2,l,r,h) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0 then rebalance (add comparer k v l) k2 v2 r
                elif c = 0 then MapNode(k,v,l,r,h)
                else rebalance l k2 v2 (add comparer k v r) 

        let rec find (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) k m = 
            match m with 
            | MapEmpty -> raise (System.Collections.Generic.KeyNotFoundException())
            | MapOne(k2,v2) -> 
                let c = comparer.Invoke(k,k2) 
                if c = 0 then v2
                else raise (System.Collections.Generic.KeyNotFoundException())
            | MapNode(k2,v2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0 then find comparer k l
                elif c = 0 then v2
                else find comparer k r

        let rec tryfind (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) k m = 
            match m with 
            | MapEmpty -> None
            | MapOne(k2,v2) -> 
                let c = comparer.Invoke(k,k2) 
                if c = 0 then Some v2
                else None
            | MapNode(k2,v2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0 then tryfind comparer k l
                elif c = 0 then Some v2
                else tryfind comparer k r

        let partition1 (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) f k v (acc1,acc2) = 
            if f k v then (add comparer k v acc1,acc2) else (acc1,add comparer k v acc2) 
        
        let rec partitionAux (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) f s acc = 
            match s with 
            | MapEmpty -> acc
            | MapOne(k,v) -> partition1 comparer f k v acc
            | MapNode(k,v,l,r,_) -> 
                let acc = partitionAux comparer f r acc 
                let acc = partition1 comparer f k v acc
                partitionAux comparer f l acc

        let partition (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) f s = partitionAux comparer f s (empty,empty)

        let filter1 (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) f k v acc = if f k v then add comparer k v acc else acc 

        let rec filterAux (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) f s acc = 
            match s with 
            | MapEmpty -> acc
            | MapOne(k,v) -> filter1 comparer f k v acc
            | MapNode(k,v,l,r,_) ->
                let acc = filterAux comparer f l acc
                let acc = filter1 comparer f k v acc
                filterAux comparer f r acc

        let filter (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) f s = filterAux comparer f s empty

        let rec spliceOutSuccessor m = 
            match m with 
            | MapEmpty -> failwith "internal error: Map.splice_out_succ_or_pred"
            | MapOne(k2,v2) -> k2,v2,MapEmpty
            | MapNode(k2,v2,l,r,_) ->
                match l with 
                | MapEmpty -> k2,v2,r
                | _ -> let k3,v3,l' = spliceOutSuccessor l in k3,v3,mk l' k2 v2 r

        let rec remove (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) k m = 
            match m with 
            | MapEmpty -> empty
            | MapOne(k2,v2) -> 
                let c = comparer.Invoke(k,k2) 
                if c = 0 then MapEmpty else m
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

        let rec mem (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) k m = 
            match m with 
            | MapEmpty -> false
            | MapOne(k2,v2) -> (comparer.Invoke(k,k2) = 0)
            | MapNode(k2,v2,l,r,_) -> 
                let c = comparer.Invoke(k,k2) 
                if c < 0 then mem comparer k l
                else (c = 0 || mem comparer k r)

        let rec iter f m = 
            match m with 
            | MapEmpty -> ()
            | MapOne(k2,v2) -> f k2 v2
            | MapNode(k2,v2,l,r,_) -> iter f l; f k2 v2; iter f r

        let rec tryPick f m = 
            match m with 
            | MapEmpty -> None
            | MapOne(k2,v2) -> f k2 v2 
            | MapNode(k2,v2,l,r,_) -> 
                match tryPick f l with 
                | Some x as res -> res 
                | None -> 
                match f k2 v2 with 
                | Some x as res -> res 
                | None -> 
                tryPick f r

        let rec exists f m = 
            match m with 
            | MapEmpty -> false
            | MapOne(k2,v2) -> f k2 v2
            | MapNode(k2,v2,l,r,_) -> exists f l || f k2 v2 || exists f r

        let rec for_all f m = 
            match m with 
            | MapEmpty -> true
            | MapOne(k2,v2) -> f k2 v2
            | MapNode(k2,v2,l,r,_) -> for_all f l && f k2 v2 && for_all f r

        let rec map f m = 
            match m with 
            | MapEmpty -> empty
            | MapOne(k,v) -> MapOne(k,f v)
            | MapNode(k,v,l,r,h) -> 
                let l2 = map f l 
                let v2 = f v 
                let r2 = map f r 
                MapNode(k,v2,l2, r2,h)

        let rec mapi f m = 
            match m with
            | MapEmpty -> empty
            | MapOne(k,v) -> MapOne(k,f k v)
            | MapNode(k,v,l,r,h) -> 
                let l2 = mapi f l 
                let v2 = f k v 
                let r2 = mapi f r 
                MapNode(k,v2, l2, r2,h)

        let rec foldBack (f:OptimizedClosures.FastFunc3<_,_,_,_>) m x = 
            match m with 
            | MapEmpty -> x
            | MapOne(k,v) -> f.Invoke(k,v,x)
            | MapNode(k,v,l,r,h) -> 
                let x = foldBack f r x
                let x = f.Invoke(k,v,x)
                foldBack f l x

        let rec fold (f:OptimizedClosures.FastFunc3<_,_,_,_>) x m  = 
            match m with 
            | MapEmpty -> x
            | MapOne(k,v) -> f.Invoke(x,k,v)
            | MapNode(k,v,l,r,h) -> 
                let x = fold f x l
                let x = f.Invoke(x,k,v)
                fold f x r

        let foldSection (comparer: OptimizedClosures.FastFunc2<'Value,'Value,int>) lo hi f m x =
            let rec fold_from_to f m x = 
                match m with 
                | MapEmpty -> x
                | MapOne(k,v) ->
                    let clo_k = comparer.Invoke(lo,k)
                    let ck_hi = comparer.Invoke(k,hi)
                    let x = if clo_k <= 0 && ck_hi <= 0 then f k v x else x
                    x
                | MapNode(k,v,l,r,h) ->
                    let clo_k = comparer.Invoke(lo,k)
                    let ck_hi = comparer.Invoke(k,hi)
                    let x = if clo_k < 0                then fold_from_to f l x else x
                    let x = if clo_k <= 0 && ck_hi <= 0 then f k v x                     else x
                    let x = if ck_hi < 0                then fold_from_to f r x else x
                    x
           
            if comparer.Invoke(lo,hi) = 1 then x else fold_from_to f m x


        let to_list m = 
            let rec loop m acc = 
                match m with 
                | MapEmpty -> acc
                | MapOne(k,v) -> (k,v)::acc
                | MapNode(k,v,l,r,h) -> loop l ((k,v)::loop r acc)
            loop m []
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
        [<StructuralEquality(false); StructuralComparison(false)>]
        type iterator<'Key,'Value> = 
             { /// invariant: always collapseLHS result 
               mutable stack: MapTree<'Key,'Value> list;  
               /// true when MoveNext has been called   
               mutable started : bool }

        // collapseLHS:
        // a) Always returns either [] or a list starting with SetOne.
        // b) The "fringe" of the set stack is unchanged. 
        let rec collapseLHS stack =
            match stack with
            | []                           -> []
            | MapEmpty             :: rest -> collapseLHS rest
            | MapOne (k,v)         :: rest -> stack
            | (MapNode(k,v,l,r,h)) :: rest -> collapseLHS (l :: MapOne (k,v) :: r :: rest)
          
        let mkIterator s = { stack = collapseLHS [s]; started = false }

        let not_started() = raise (new System.InvalidOperationException("Enumeration has not started. Call MoveNext."))
        let already_finished() = raise (new System.InvalidOperationException("Enumeration already finished."))

        let current i =
            if i.started then
                match i.stack with
                  | MapOne (k,v) :: _ -> new KeyValuePair<_,_>(k,v)
                  | []            -> already_finished()
                  | _             -> failwith "Please report error: Map iterator, unexpected stack for current"
            else
                not_started()

        let rec moveNext i =
          if i.started then
            match i.stack with
              | MapOne _ :: rest -> i.stack <- collapseLHS rest;
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



#if FX_NO_DEBUG_PROXIES
#else
    [<System.Diagnostics.DebuggerTypeProxy(typedefof<MapDebugView<_,_>>)>]
#endif
#if FX_NO_DEBUG_DISPLAYS
#else
    [<System.Diagnostics.DebuggerDisplay("Count = {Count}")>]
#endif
    [<Sealed>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")>]
    type Map<'Key,'Value>(comparer: OptimizedClosures.FastFunc2<'Key,'Key,int>, tree: MapTree<'Key,'Value>) =

        static let empty = 
                 let comparer = ComparisonIdentity.GetFastStructuralComparisonFunction<'Key>() 
                 new Map<'Key,'Value>(comparer,MapTree<_,_>.MapEmpty)

        static member Empty : Map<'Key,'Value> = empty

        static member Create(ie : IEnumerable<_>) : Map<'Key,'Value> = 
           let comparer = ComparisonIdentity.GetFastStructuralComparisonFunction<'Key>() 
           Map<_,_>(comparer,MapTree.of_seq comparer ie)
    
        static member Create() : Map<'Key,'Value> = empty

        new(ie : seq<_>) = 
           let comparer = ComparisonIdentity.GetFastStructuralComparisonFunction<'Key>() 
           Map<_,_>(comparer,MapTree.of_seq comparer ie)
    
#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member internal m.Comparer = comparer
        //[<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
        member internal m.Tree = tree
        member m.Add(k,v) : Map<'Key,'Value> = Map<'Key,'Value>(comparer,MapTree.add comparer k v tree)
#if FX_NO_DEBUG_DISPLAYS
#else
        [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
#endif
        member m.IsEmpty = MapTree.is_empty tree
        member m.Item with get(k : 'Key) = MapTree.find comparer k tree
        member m.TryPick(f) = MapTree.tryPick f tree 
        member m.Exists(f) = MapTree.exists f tree 
        member m.Filter(f)  : Map<'Key,'Value> = Map<'Key,'Value>(comparer ,MapTree.filter comparer f tree)
        member m.ForAll(f) = MapTree.for_all f tree 
        member m.Fold f acc =  
            let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
            MapTree.foldBack f tree acc
        member m.FoldSection (lo:'Key) (hi:'Key) f (acc:'z) = MapTree.foldSection comparer lo hi f tree acc 
        member m.Iterate f = MapTree.iter f tree
        member m.MapRange f  = Map<'Key,'b>(comparer,MapTree.map f tree)
        member m.Map f  = Map<'Key,'b>(comparer,MapTree.mapi f tree)
        member m.Partition(f)  : Map<'Key,'Value> * Map<'Key,'Value> = 
            let r1,r2 = MapTree.partition comparer f tree  in 
            Map<'Key,'Value>(comparer,r1), Map<'Key,'Value>(comparer,r2)
        member m.Count = MapTree.size tree
        member m.ContainsKey(k) = MapTree.mem comparer k tree
        member m.Remove(k)  : Map<'Key,'Value> = Map<'Key,'Value>(comparer,MapTree.remove comparer k tree)
        member m.TryFind(k) = MapTree.tryfind comparer k tree
        member m.ToList() = MapTree.to_list tree
        member m.ToArray() = MapTree.to_array tree

        static member of_list(l) : Map<'Key,'Value> = 
           let comparer = ComparisonIdentity.GetFastStructuralComparisonFunction<'Key>() 
           Map<_,_>(comparer,MapTree.of_list comparer l)
           
        member this.ComputeHashCode() = 
            let combineHash x y = (x <<< 1) + y + 631 
            let mutable res = 0
            for (KeyValue(x,y)) in this do
                res <- combineHash res (hash x)
                res <- combineHash res (hash y)
            abs res

        override this.Equals(that) = 
            match that with 
            | :? Map<'Key,'Value> -> 
                ((this :> System.IComparable).CompareTo(that) = 0)
            | _ -> false

        override this.GetHashCode() = this.ComputeHashCode()

        interface IEnumerable<KeyValuePair<'Key, 'Value>> with
            member s.GetEnumerator() = MapTree.mkIEnumerator tree

        interface System.Collections.IEnumerable with
            member s.GetEnumerator() = (MapTree.mkIEnumerator tree :> System.Collections.IEnumerator)

        interface IDictionary<'Key, 'Value> with 
            member s.Item 
                with get x = s.[x]            
                and  set x v = raise (NotSupportedException("Map values may not be mutated"))

            // REVIEW: this implementation could avoid copying the Values to an array    
            member s.Keys = ([| for kvp in s -> kvp.Key |] :> ICollection<'Key>)

            // REVIEW: this implementation could avoid copying the Values to an array    
            member s.Values = ([| for kvp in s -> kvp.Value |] :> ICollection<'Value>)

            member s.Add(k,v) = raise (NotSupportedException("Map values may not be mutated"))
            member s.ContainsKey(k) = s.ContainsKey(k)
            member s.TryGetValue(k,r) = if s.ContainsKey(k) then (r <- s.[k]; true) else false
            member s.Remove(k : 'Key) = (raise (NotSupportedException("Map values may not be mutated")) : bool)

        interface ICollection<KeyValuePair<'Key, 'Value>> with 
            member s.Add(x) = raise (NotSupportedException("Map values may not be mutated"));
            member s.Clear() = raise (NotSupportedException("Map values may not be mutated"));
            member s.Remove(x) = raise (NotSupportedException("Map values may not be mutated"));
            member s.Contains(x) = s.ContainsKey(x.Key) && s.[x.Key] = x.Value
            member s.CopyTo(arr,i) = MapTree.copyToArray tree arr i
            member s.IsReadOnly = true
            member s.Count = s.Count

        interface System.IComparable with 
            member m.CompareTo(obj: obj) = 
                match obj with 
                | :? Map<'Key,'Value>  as m2->
                    Seq.compare 
                       (fun (kvp1 : KeyValuePair<_,_>) (kvp2 : KeyValuePair<_,_>)-> 
                           let c = comparer.Invoke(kvp1.Key,kvp2.Key) in 
                           if c <> 0 then c else Operators.compare kvp1.Value kvp2.Value)
                       m m2 
                | _ -> 
                    invalidArg "obj" "the two obejcts have different types and are not comparable"
#if FX_NO_DEBUG_PROXIES
#else
    and 
        [<Sealed>]
        MapDebugView<'Key,'Value>(v: Map<'Key,'Value>)  =  

         [<System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)>]
         member x.Items = v |> Seq.truncate 1000 |> Seq.map (fun kvp -> { key = kvp.Key; value=kvp.Value})  |> Seq.to_array 

    and 
        [<DebuggerDisplay("{value}", Name="[{key}]", Type="")>]
        MapDebugViewKeyValuePair = { key:obj; value:obj } 
            
#endif
        

namespace Microsoft.FSharp.Collections

    open System
    open System.Diagnostics
    open System.Collections.Generic
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Primitives.Basics

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Map = 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let isEmpty (m:Map<_,_>) = m.IsEmpty
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let is_empty (m:Map<_,_>) = m.IsEmpty

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let add k v (m:Map<_,_>) = m.Add(k,v)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let find k (m:Map<_,_>) = m.[k]

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryFind k (m:Map<_,_>) = m.TryFind(k)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryfind k (m:Map<_,_>) = m.TryFind(k)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let remove k (m:Map<_,_>) = m.Remove(k)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let contains k (m:Map<_,_>) = m.ContainsKey(k)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iter f (m:Map<_,_>) = m.Iterate(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryPick f (m:Map<_,_>) = m.TryPick(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let first f (m:Map<_,_>) = m.TryPick(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let pick f (m:Map<_,_>) = match tryPick f m with None -> raise (System.Collections.Generic.KeyNotFoundException()) | Some res -> res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let exists f (m:Map<_,_>) = m.Exists(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let filter f (m:Map<_,_>) = m.Filter(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let partition f (m:Map<_,_>) = m.Partition(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let forall f (m:Map<_,_>) = m.ForAll(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let for_all f m = forall f m

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let mapRange f (m:Map<_,_>) = m.MapRange(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let mapi f (m:Map<_,_>) = m.Map(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let map f (m:Map<_,_>) = m.Map(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold f z (m:Map<_,_>) = 
            let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
            MapTree.fold f z m.Tree

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold_left f z m = fold f z m

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let foldBack f (m:Map<_,_>) z = 
            let f = OptimizedClosures.FastFunc3<_,_,_,_>.Adapt(f)
            MapTree.foldBack  f m.Tree z
        
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let fold_right f m z = foldBack f m z

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let to_seq (m:Map<_,_>) = m |> Seq.map (fun kvp -> kvp.Key, kvp.Value)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let findIndex f (m : Map<_,_>) = m |> to_seq |> Seq.pick (fun (k,v) -> if f k v then Some(k) else None)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let find_index f m = findIndex f m

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryFindIndex f (m : Map<_,_>) = m |> to_seq |> Seq.tryPick (fun (k,v) -> if f k v then Some(k) else None)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let tryfind_index f m = tryFindIndex f m

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let of_list (l: ('Key * 'Value) list) = Map<_,_>.of_list(l)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let of_seq l = Map<_,_>.Create(l)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let of_array (l: ('Key * 'Value) array) = of_seq l

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let to_list (m:Map<_,_>) = m.ToList()

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let to_array (m:Map<_,_>) = m.ToArray()

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let empty<'Key,'Value> = Map<'Key,'Value>.Empty

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let mem k (m:Map<_,_>) = m.ContainsKey(k)

