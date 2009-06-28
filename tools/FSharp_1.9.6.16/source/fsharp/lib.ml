// (c) Microsoft Corporation. All rights reserved

#light

module (* internal *) Microsoft.FSharp.Compiler.Lib

open System.IO
open Internal.Utilities
open Internal.Utilities.Pervasives
open System.Diagnostics
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library

/// is this the developer-debug build? 
let debug = false 
let verbose = false
let progress = ref false 
let tracking = ref false // intended to be a general hook to control diagnostic output when tracking down bugs

(*-------------------------------------------------------------------------
!* Library: bits
 *------------------------------------------------------------------------*)

module Bits = 
    let b0 n =  (n          &&& 0xFF)  
    let b1 n =  ((n >>> 8)  &&& 0xFF) 
    let b2 n =  ((n >>> 16) &&& 0xFF) 
    let b3 n =  ((n >>> 24) &&& 0xFF) 

    let rec pown32 n = if n = 0 then 0  else (pown32 (n-1) ||| (1  <<<  (n-1)))
    let rec pown64 n = if n = 0 then 0L else (pown64 (n-1) ||| (1L <<< (n-1)))
    let mask32 m n = (pown32 n) <<< m
    let mask64 m n = (pown64 n) <<< m


module List = 
    let noRepeats xOrder xs =
        let s = Zset.addList   xs (Zset.empty xOrder) // build set 
        Zset.elements s          // get elements... no repeats 


(*-------------------------------------------------------------------------
!* Library: files
 *------------------------------------------------------------------------*)

module Filename = 
    let fullpath cwd nm = 
        let p = if Path.IsPathRooted(nm) then nm else Path.Combine(cwd,nm)
        try Path.GetFullPath(p) with 
        | :? System.ArgumentException 
        | :? System.ArgumentNullException 
        | :? System.NotSupportedException 
        | :? System.IO.PathTooLongException 
        | :? System.Security.SecurityException -> p

    let hasSuffixCaseInsensitive suffix filename = (* case-insensitive *)
      Filename.check_suffix (String.lowercase filename) (String.lowercase suffix)

    let isDll file = hasSuffixCaseInsensitive ".dll" file 


//-------------------------------------------------------------------------
// Library: projections
//------------------------------------------------------------------------

type 'a order = 'a -> 'a -> int

let foldOn p f z x = f z (p x)
let maxOn f x1 x2 = if f x1 > f x2 then x1 else x2
let orderOn p pxOrder x xx = pxOrder (p x) (p xx)

//-------------------------------------------------------------------------
// Library: Bool
//------------------------------------------------------------------------

module Bool = 
    let order (a:bool) (b:bool) = Operators.compare a b

module Int32 = 
    let order (a:int) (b:int) = Operators.compare a b

module Int64 = 
    let order (a:int64) (b:int64) = Operators.compare a b

//-------------------------------------------------------------------------
// Library: Strings
//------------------------------------------------------------------------

module String =

    /// When generating HTML documentation,
    /// some servers/filesystem are case insensitive.
    /// This leads to collisions on type names, e.g. complex and Complex.
    ///
    /// This function does partial disambiguation, by prefixing lowercase strings with _.
    let underscoreLowercase s =
      if String.isAllLower s then "_"^s else s
      
module Pair = 
    let order (compare1,compare2) (a1,a2) (aa1,aa2) =
        let res1 = compare1 a1 aa1
        if res1 <> 0 then res1 else compare2 a2 aa2

let fmap2'2 f z (a1,a2)       = let z,a2 = f z a2 in z,(a1,a2)


(*-------------------------------------------------------------------------
!* Library: Map extensions
 *------------------------------------------------------------------------*)

module Map = 
    let tryFindMulti k map = match Map.tryfind k map with Some res -> res | None -> []

(*-------------------------------------------------------------------------
!* Library: Name maps
 *------------------------------------------------------------------------*)

type 'a NameMap = Map<string,'a>
type nameset = string Zset.t
type NameMultiMap<'a> = 'a list NameMap

module Nameset =
    let of_list l : nameset = List.foldBack Zset.add l (Zset.empty String.order)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module NameMap = 

    let empty = Map.empty
    let domain m = Map.fold_right (fun x _ acc -> Zset.add x acc) m (Zset.empty String.order)
    let domainL m = Zset.elements (domain m)
    let range m = List.rev (Map.fold_right (fun _ x sofar -> x :: sofar) m [])
    let fold f (m:'a NameMap) z = Map.fold_right f m z
    let forall f m = Map.fold_right (fun x y sofar -> sofar && f x y) m true
    let exists f m = Map.fold_right (fun x y sofar -> sofar or f x y) m false
    let of_keyed_list f l = List.foldBack (fun x acc -> Map.add (f x) x acc) l Map.empty
    let of_list l : 'a NameMap = Map.of_list l
    let of_FlatList (l:FlatList<_>) : 'a NameMap = FlatList.toMap l
    let to_list (l: 'a NameMap) = Map.to_list l
    let layer (m1 : 'a NameMap) m2 = Map.fold_right Map.add m1 m2

    (* not a very useful function - only called in one place - should be changed *)
    let layerAdditive addf m1 m2 = 
      Map.fold_right (fun x y sofar -> Map.add x (addf (Map.tryFindMulti x sofar) y) sofar) m1 m2

    let union unionf m1 m2 = 
      Map.fold_right
        (fun x1 y1 sofar -> 
          Map.add 
            x1 
            (match Map.tryfind x1 sofar with 
              | Some res -> (unionf y1 res) 
              | None -> y1) 
            sofar)
        m1 m2

    (* For every entry in m2 find an entry in m1 and fold *)
    let subfold2 errf f m1 m2 acc =
        Map.fold_right (fun n x2 acc -> try f n (Map.find n m1) x2 acc with Not_found -> errf n x2) m2 acc

    let suball2 errf p m1 m2 = subfold2 errf (fun _ x1 x2 acc -> p x1 x2 & acc) m1 m2 true

    let mapfold f s (l: 'a NameMap) = 
        Map.fold_right (fun x y (l',s') -> let y',s'' = f s' x y in Map.add x y' l',s'') l (Map.empty,s)

    let mapFoldRange f s (l: 'a NameMap) = 
        Map.fold_right (fun x y (l',s') -> let y',s'' = f s' y in Map.add x y' l',s'') l (Map.empty,s)

    let foldRange f (l: 'a NameMap) acc = Map.fold_right (fun _ y acc -> f y acc) l acc

    let filterRange f (l: 'a NameMap) = Map.fold_right (fun x y acc -> if f y then Map.add x y acc else acc) l Map.empty

    let mapFilter f (l: 'a NameMap) = Map.fold_right (fun x y acc -> match f y with None -> acc | Some y' -> Map.add x y' acc) l Map.empty

    let map f (l : 'a NameMap) = Map.mapi (fun _ x -> f x) l

    let iter f (l : 'a NameMap) = Map.iter (fun k v -> f v) l

    let iteri f (l : 'a NameMap) = Map.iter f l

    let mapi f (l : 'a NameMap) = Map.mapi f l

    let partition f (l : 'a NameMap) = Map.filter (fun _ x-> f x) l, Map.filter (fun _ x -> not (f x)) l

    let mem v (m: 'a NameMap) = Map.mem v m

    let find v (m: 'a NameMap) = Map.find v m

    let tryfind v (m: 'a NameMap) = Map.tryfind v m 

    let add v x (m: 'a NameMap) = Map.add v x m

    let is_empty (m: 'a NameMap) = (Map.is_empty  m)

    let existsInRange p m =  Map.fold_right (fun _ y acc -> acc or p y) m false 

    let tryFindInRange p m = 
        Map.fold_right (fun _ y acc -> 
             match acc with 
             | None -> if p y then Some y else None 
             | _ -> acc) m None 

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module NameMultiMap = 
    let existsInRange f (m: NameMultiMap<'a>) = NameMap.exists (fun _ l -> List.exists f l) m
    let find v (m: NameMultiMap<'a>) = match Map.tryfind v m with None -> [] | Some r -> r
    let add v x (m: NameMultiMap<'a>) = NameMap.add v (x :: find v m) m
    let range (m: NameMultiMap<'a>) = Map.fold_right (fun _ x sofar -> x @ sofar) m []
    let chooseRange f (m: NameMultiMap<'a>) = Map.fold_right (fun _ x sofar -> List.choose f x @ sofar) m []
    let map f (m: NameMultiMap<'a>) = NameMap.map (List.map f) m 
    let empty : NameMultiMap<'a> = Map.empty
    let initBy f xs : NameMultiMap<'a> = xs |> Seq.group_by f |> Seq.map (fun (k,v) -> (k,List.of_seq v)) |> Map.of_seq 


//---------------------------------------------------------------------------
// Library: Pre\Post checks
//------------------------------------------------------------------------- 
module Check = 
    
    /// Throw System.InvalidOperationException() if argument is None.
    /// If there is a value (e.g. Some(value)) then value is returned.
    let NotNone argname (arg:'a option) : 'a = 
        match arg with 
        | None -> raise (new System.InvalidOperationException(argname))
        | Some x -> x

    /// Throw System.ArgumentNullException() if argument is null.
    let ArgumentNotNull arg argname = 
        match box(arg) with 
        | null -> raise (new System.ArgumentNullException(argname))
        | _ -> ()
       
        
    /// Throw System.ArgumentNullException() if array argument is null.
    /// Throw System.ArgumentOutOfRangeException() is array argument is empty.
    let ArrayArgumentNotNullOrEmpty (arr:'a[]) argname = 
        ArgumentNotNull arr argname
        if (0 = arr.Length) then
            raise (new System.ArgumentOutOfRangeException(argname))

    /// Throw System.ArgumentNullException() if string argument is null.
    /// Throw System.ArgumentOutOfRangeException() is string argument is empty.
    let StringArgumentNotNullOrEmpty (s:string) argname = 
        ArgumentNotNull s argname
        if s.Length == 0 then
            raise (new System.ArgumentNullException(argname))

//-------------------------------------------------------------------------
// Library 
//------------------------------------------------------------------------

module Imap = 
    let empty () = Zmap.empty Int32.order

    type t<'a> =  Zmap.t<int,'a>
    let add k v (t:t<'a>) = Zmap.add k v t
    let find k (t:t<'a>) = Zmap.find k t
    let tryfind k (t:t<'a>) = Zmap.tryfind k t
    let remove  k (t:t<'a>) = Zmap.remove k t
    let mem     k (t:t<'a>)  = Zmap.mem k t
    let iter    f (t:t<'a>)  = Zmap.iter f t
    let map     f (t:t<'a>)  = Zmap.map f t 
    let fold     f (t:t<'a>)  z = Zmap.fold f t z

module I64map = 
    let empty () = Zmap.empty Int64.order

    type t<'a> =  Zmap.t<int64,'a>
    let add k v (t:t<'a>) = Zmap.add k v t
    let find k (t:t<'a>) = Zmap.find k t
    let tryfind k (t:t<'a>) = Zmap.tryfind k t
    let remove  k (t:t<'a>) = Zmap.remove k t
    let mem     k (t:t<'a>)  = Zmap.mem k t
    let iter    f (t:t<'a>)  = Zmap.iter f t
    let map     f (t:t<'a>)  = Zmap.map f t 
    let fold     f (t:t<'a>)  z = Zmap.fold f t z

//-------------------------------------------------------------------------
// Library: generalized association lists
//------------------------------------------------------------------------

module ListAssoc = 

    /// Treat a list of key-value pairs as a lookup collection.
    /// This function looks up a value based on a match from the supplied
    /// predicate function.
    let rec find f x l = 
      match l with 
      | [] -> raise Not_found
      | (x',y)::t -> if f x x' then y else find f x t

    /// Treat a list of key-value pairs as a lookup collection.
    /// This function returns true if two keys are the same according to the predicate
    /// function passed in.
    let rec containsKey (f:'key->'key->bool) (x:'key) (l:('key*'value) list) : bool = 
      match l with 
      | [] -> false
      | (x',y)::t -> f x x' || containsKey f x t

//-------------------------------------------------------------------------
// Library: lists as generalized sets
//------------------------------------------------------------------------

module ListSet = 
    (* NOTE: O(n)! *)
    let rec mem f x l = 
        match l with 
        | [] -> false
        | x'::t -> f x x' or mem f x t

    (* NOTE: O(n)! *)
    let insert f x l = if mem f x l then l else x::l
    let unionFavourRight f l1 l2 = 
        if l2 = [] then l1
        else if l1 = [] then l2 
        else List.foldBack (insert f) l1 l2 (* nb. fold_right to preserve natural orders *)

    (* NOTE: O(n)! *)
    let rec private gen_index_aux eq x l n =
        match l with
        | [] -> raise Not_found
        | (h::t) -> if eq h x then n else gen_index_aux eq x t (n+1)

    let findIndex eq x l = gen_index_aux eq x l 0

    let rec remove f x l = 
        match l with 
        | (h::t) -> if f x h then t else h:: remove f x t
        | [] -> []

    (* NOTE: quadratic! *)
    let rec subtract f l1 l2 = 
      match l2 with 
      | (h::t) -> subtract f (remove (fun y2 y1 ->  f y1 y2) h l1) t
      | [] -> l1

    let isSubsetOf f l1 l2 = List.forall (fun x1 -> mem f x1 l2) l1
    (* nb. preserve orders here: f must be applied to elements of l1 then elements of l2*)
    let isSupersetOf f l1 l2 = List.forall (fun x2 -> mem (fun y2 y1 ->  f y1 y2) x2 l1) l2
    let equals f l1 l2 = isSubsetOf f l1 l2 && isSupersetOf f l1 l2

    let unionFavourLeft f l1 l2 = 
      if l2 = [] then l1 
      else if l1 = [] then l2 
      else l1 @ (subtract f l2 l1)


    (* NOTE: not tail recursive! *)
    let rec intersect f l1 l2 = 
      match l2 with 
      | (h::t) -> if mem f h l1 then h::intersect f l1 t else intersect f l1 t
      | [] -> []

    (* NOTE: quadratic! *)
    // Note: if duplicates appear, keep the ones toward the _front_ of the list
    let setify f l = List.foldBack (insert f) (List.rev l) [] |> List.rev


module FlatListSet = 
    let remove f x l = FlatList.filter (fun y -> not (f x y)) l

//-------------------------------------------------------------------------
// Library: pairs
//------------------------------------------------------------------------

let pair_map f1 f2 (a,b) = (f1 a, f2 b)
let triple_map f1 f2 f3 (a,b,c) = (f1 a, f2 b, f3 c)
let map_fst f (a,b) = (f a, b)
let map_snd f (a,b) = (a, f b)
let map_acc_fst f s (x,y) =  let x',s = f s x in  (x',y),s
let map_acc_snd f s (x,y) =  let y',s = f s y in  (x,y'),s
let pair a b = a,b      

let p13 (x,y,z) = x
let p23 (x,y,z) = y
let p33 (x,y,z) = z

let map1'2 f (a1,a2)       = (f a1,a2)
let map2'2 f (a1,a2)       = (a1,f a2)
let map1'3 f (a1,a2,a3)     = (f a1,a2,a3)
let map2'3 f (a1,a2,a3)     = (a1,f a2,a3)
let map3'3 f (a1,a2,a3)     = (a1,a2,f a3)
let map3'4 f (a1,a2,a3,a4)     = (a1,a2,f a3,a4)
let map4'4 f (a1,a2,a3,a4)   = (a1,a2,a3,f a4)
let map5'5 f (a1,a2,a3,a4,a5) = (a1,a2,a3,a4,f a5)
let map6'6 f (a1,a2,a3,a4,a5,a6) = (a1,a2,a3,a4,a5,f a6)
let foldl'2 (f1,f2)    acc (a1,a2)         = f2 (f1 acc a1) a2
let foldl1'2 f1    acc (a1,a2)         = f1 acc a1
let foldl'3 (f1,f2,f3) acc (a1,a2,a3)      = f3 (f2 (f1 acc a1) a2) a3
let map'2 (f1,f2)    (a1,a2)     = (f1 a1, f2 a2)
let map'3 (f1,f2,f3) (a1,a2,a3)  = (f1 a1, f2 a2, f3 a3)

//---------------------------------------------------------------------------
// Zmap rebinds
//------------------------------------------------------------------------- 

module Zmap = 
    let force  k   mp           = match Zmap.tryfind k mp with Some x -> x | None -> failwith "Zmap.force: lookup failed"

    let mapKey key f mp =
      match f (Zmap.tryfind key mp) with
      | Some fx -> Zmap.add key fx mp       
      | None    -> Zmap.remove key mp

(*---------------------------------------------------------------------------
!* Zset
 *------------------------------------------------------------------------- *)

module Zset =
    let of_list order xs = Zset.addList   xs (Zset.empty order)

    // CLEANUP NOTE: move to Zset?
    let rec fixpoint f (s as s0) =
        let s = f s
        if Zset.equal s s0 then s0           (* fixed *)
                           else fixpoint f s (* iterate *)



(*---------------------------------------------------------------------------
!* Misc
 *------------------------------------------------------------------------- *)

let equalOn f x y = (f x) = (f y)


(*---------------------------------------------------------------------------
!* Buffer printing utilities
 *------------------------------------------------------------------------- *)

let bufs f = 
    let buf = Buffer.create 100 in f buf; Buffer.contents buf

let buff os f x = 
    let buf = Buffer.create 100 in f buf x; Buffer.output_buffer os buf

// Converts "\n" into System.Environment.NewLine before writing to os. See lib.ml:buff
let writeViaBufferWithEnvironmentNewLines  os f x = 
    let buf = Buffer.create 100 in f buf x;
    let text = buf.ToString()
    let text = text.Replace("\n",System.Environment.NewLine)
    output_string os text
        
//---------------------------------------------------------------------------
// Imperative Graphs 
//---------------------------------------------------------------------------

module NodeGraph = 
    type ('id,'data) node = { nodeId: 'id; nodeData: 'data; mutable nodeNeighbours: ('id,'data) node list }

    type ('id,'data) graph = { id: ('data -> 'id);
                               ord: 'id order;
                               nodes: ('id,'data) node list;
                               edges: ('id * 'id) list;
                               getNodeData: ('id -> 'data) }

    let mk_graph (id,ord) nodeData edges =
        let nodemap = List.map (fun d -> id d, { nodeId = id d; nodeData=d; nodeNeighbours=[] }) nodeData
        let tab = Zmap.of_list ord nodemap 
        let getNode nodeId = Zmap.find nodeId tab
        let getNodeData nodeId = (getNode nodeId).nodeData
        let nodes = List.map snd nodemap
        List.iter (fun node -> node.nodeNeighbours <- List.map (snd >> getNode) (List.filter (fun (x,y) -> ord x node.nodeId = 0) edges)) nodes;
        {id=id; ord = ord; nodes=nodes;edges=edges;getNodeData=getNodeData}

    let iterate_cycles f g = 
        let rec trace path node = 
          if List.exists (g.id >> (=) node.nodeId) path then f (List.rev path)
          else List.iter (trace (node.nodeData::path)) node.nodeNeighbours
        List.iter (fun node -> trace [] node) g.nodes 

#if OLDCODE

let dfs g = 
    let grey = ref (Zset.empty g.ord)
    let time = ref 0
    let forest = ref []
    let backEdges = ref []
    let discoveryTimes = ref (Zmap.empty g.ord)
    let finishingTimes = ref (Zmap.empty g.ord)
    g.nodes |> List.iter (fun n ->  
      (* build a dfsTree for each node in turn *)
      let treeEdges = ref []
      let rec visit n1 = 
        incr time;
        grey := Zset.add n1.nodeId !grey;
        discoveryTimes := Zmap.add n1.nodeId !time !discoveryTimes;
        n1.nodeNeighbours |> List.iter (fun n2 ->
          if not (Zset.mem n2.nodeId !grey) then begin
            treeEdges := (n1.nodeId,n2.nodeId) :: !treeEdges;
            visit(n2)
          end else begin
            backEdges := (n1.nodeId,n2.nodeId) :: !backEdges
          end);
        incr time;
        finishingTimes := Zmap.add n1.nodeId !time !finishingTimes;
        ()
      if not (Zset.mem n.nodeId !grey) then begin 
        visit(n);
        forest := (n.nodeId,!treeEdges) :: !forest
      end);
    !forest, !backEdges,  (fun n -> Zmap.find n !discoveryTimes), (fun n -> Zmap.find n !finishingTimes)
 

(* Present strongly connected components, in dependency order *)
(* Each node is assumed to have a self-edge *)
let topsort_strongly_connected_components g = 
    let forest, backEdges, discoveryTimes, finishingTimes = dfs g
    let nodeIds = List.map (fun n -> n.nodeId) g.nodes
    let nodesInDecreasingFinishingOrder = 
      List.sort (fun n1 n2 -> -(compare (finishingTimes n1) (finishingTimes n2))) nodeIds
    let gT = NodeGraph.mk_graph (g.id,g.ord) (List.map g.getNodeData nodesInDecreasingFinishingOrder) (List.map (fun (x,y) -> (y,x)) g.edges)
    let forest, backEdges, discoveryTimes, finishingTimes = dfs gT
    let scc (root,tree) = Zset.add root (List.foldBack (fun (n1,n2) acc -> Zset.add n1 (Zset.add n2 acc)) tree (Zset.empty g.ord))
    let sccs = List.rev (List.map scc forest)
    List.map (Zset.elements >> List.map g.getNodeData) sccs
#endif


#if SELFTEST
let g1 = NodeGraph.mk_graph (=) [1;2;3] [(1,2);(2,3);(3,1)]
let g2 = NodeGraph.mk_graph (=) [1;2;3] [(1,2);(2,3)]
let g3 = NodeGraph.mk_graph (=) [1;2;3] [(1,1);(2,2)]
let g4 = NodeGraph.mk_graph (=) [1;2;3] [(1,1);(2,1)]
let g5 = NodeGraph.mk_graph (=) [1;2;3] [(3,2);(2,1)]
let g6 = NodeGraph.mk_graph (=) [1;2;3] []
let g7 = NodeGraph.mk_graph (=) [1;2;3] [(1,2);(2,1);(3,3)]
let g8 = NodeGraph.mk_graph (=) [1;2;3] [(3,2);(2,3);(1,1)]


let p sccs =  List.iter (fun l -> printf "scc: "; List.iter (fun i -> printf "%d;" i) l; printf "\n") sccs

do p (topsort_strongly_connected_components g1);;
do p (topsort_strongly_connected_components g2);;
do p (topsort_strongly_connected_components g3);;
do p (topsort_strongly_connected_components g4);;
do p (topsort_strongly_connected_components g5);;
do p (topsort_strongly_connected_components g6);;
do p (topsort_strongly_connected_components g7);;
do p (topsort_strongly_connected_components g8);;

#endif

(*---------------------------------------------------------------------------
!* In some cases we play games where we use 'null' as a more efficient representation
 * in F#. The functions below are used to give initial values to mutable fields.
 * This is an unsafe trick, as it relies on the fact that the type of values
 * being placed into the slot never utilizes "null" as a representation. To be used with
 * with care.
 *------------------------------------------------------------------------- *)

// The following DEBUG code does not compile.
//#if DEBUG
//type 'a nonnull_slot = 'a option 
//let nullable_slot_empty() = None 
//let nullable_slot_full(x) = Some x
//#else
type 'a nonnull_slot = 'a
let nullable_slot_empty() = Unchecked.defaultof<'a>
let nullable_slot_full(x) = x
//#endif    

(*---------------------------------------------------------------------------
!* Caches, mainly for free variables
 *------------------------------------------------------------------------- *)

type 'a cache = { mutable cacheVal: 'a nonnull_slot; }
let new_cache() = { cacheVal = nullable_slot_empty() }

let inline cached cache resf = 
  match box cache.cacheVal with 
  | null -> (let res = resf() in  cache.cacheVal <- nullable_slot_full res; res)
  | _ -> cache.cacheVal

let inline cacheOptRef cache f = 
    match !cache with 
    | Some v -> v
    | None -> 
       let res = f()
       cache := Some res;
       res 


(* There is a bug in .NET Framework v2.0.52727 DD#153959 that very occasionally hits F# code. *)
(* It is related to recursive class loading in multi-assembly NGEN scenarios. The bug has been fixed but *)
(* not yet deployed. *)
(* The bug manifests itself as an ExecutionEngine failure or fast-fail process exit which comes *)
(* and goes depending on whether components are NGEN'd or not, e.g. 'ngen install FSharp.COmpiler.dll' *)
(* One workaround for the bug is to break NGEN loading and fixups into smaller fragments. Roughly speaking, the NGEN *)
(* loading process works by doing delayed fixups of references in NGEN code. This happens on a per-method *)
(* basis. For example, one manifestation is that a "print" before calling a method like Lexfilter.create gets *)
(* displayed but the corresponding "print" in the body of that function doesn't get displayed. In between, the NGEN *)
(* image loader is performing a whole bunch of fixups of the NGEN code for the body of that method, and also for *)
(* bodies of methods referred to by that method. That second bit is very important: the fixup causing the crash may *)
(* be a couple of steps down the dependency chain. *)
(* *)
(* One way to break work into smaller chunks is to put delays in the call chains, i.e. insert extra stack frames. That's *)
(* what the function 'delayInsertedToWorkaroundKnownNgenBug' is for. If you get this problem, try inserting  *)
(*    delayInsertedToWorkaroundKnownNgenBug "Delay1" (fun () -> ...) *)
(* at the top of the function that doesn't seem to be being called correctly. This will help you isolate out the problem *)
(* and may make the problem go away altogher. Enable the 'print' commands in that function too. *)

let delayInsertedToWorkaroundKnownNgenBug s f = 
    (* Some random code to prevent inlining of this function *)
    let res = ref 10
    for i = 0 to 2 do 
       res := !res + String.length s;
    done;
    if verbose then Printf.printf "------------------------executing NGEN bug delay '%s', calling 'f' --------------\n" s;
    let res = f()
    if verbose then Printf.printf "------------------------exiting NGEN bug delay '%s' --------------\n" s;
    res
    

#if DUMPER
type Dumper(x:obj) =
     [<DebuggerBrowsable(DebuggerBrowsableState.Collapsed)>]
     member self.Dump = sprintf "%A" x 
#endif
