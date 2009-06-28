(* (c) Microsoft Corporation. All rights reserved  *)

module Microsoft.FSharp.Compiler.AbstractIL.Internal.Zset 

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Internal.Utilities.Collections.Tagged

type 'a order = 'a -> 'a -> int

type set<'a> = Internal.Utilities.Collections.Tagged.Set<'a >
type 'a t = set<'a>

let empty (ord : 'a order) = Internal.Utilities.Collections.Tagged.Set<_,_>.Empty(ComparisonIdentity.FromFunction ord)

let is_empty (s:set<_>) = s.IsEmpty

let mem x (s:set<_>) = s.Contains(x)
let add x (s:set<_>) = s.Add(x)
let addList xs a = List.fold (fun a x -> add x a) a xs
let addFlatList xs a = FlatList.fold (fun a x -> add x a) a xs
    
let singleton ord x = add x (empty ord)
let remove x (s:set<_>) = s.Remove(x)

let fold (f : 'a -> 'b -> 'b) (s:set<_>) b = s.Fold f b
let iter f (s:set<_>) = s.Iterate f 
let for_all p (s:set<_>) = s.ForAll p 
let count  (s:set<_>) = s.Count
let forall  p (s:set<_>) = s.ForAll p 
let exists  p (s:set<_>) = s.Exists p 
let subset (s1:set<_>) (s2:set<_>)  = s1.IsSubsetOf s2
let equal (s1:set<_>) (s2:set<_>)  = Internal.Utilities.Collections.Tagged.Set<_,_>.Equality(s1,s2)
let elements (s:set<_>) = s.ToList()
let filter p (s:set<_>) = s.Filter p

let union (s1:set<_>) (s2:set<_>)  = Internal.Utilities.Collections.Tagged.Set<_,_>.Union(s1,s2)
let inter (s1:set<_>) (s2:set<_>)  = Internal.Utilities.Collections.Tagged.Set<_,_>.Intersection(s1,s2)
let diff (s1:set<_>) (s2:set<_>)  = Internal.Utilities.Collections.Tagged.Set<_,_>.Difference(s1,s2)

let mem_of m k = mem k m
