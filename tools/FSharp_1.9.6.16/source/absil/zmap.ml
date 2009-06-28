(* (c) Microsoft Corporation. All rights reserved  *)

module Microsoft.FSharp.Compiler.AbstractIL.Internal.Zmap 

open Internal.Utilities.Collections.Tagged
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

type 'a order = 'a -> 'a -> int

type map<'key,'a> = Internal.Utilities.Collections.Tagged.Map<'key,'a>
type ('key,'a) t = map<'key,'a>

let empty (ord : 'a order) = Internal.Utilities.Collections.Tagged.Map<_,_,_>.Empty(ComparisonIdentity.FromFunction(ord))
let add k v (m:map<_,_>) = m.Add(k,v)
let find k (m:map<_,_>) = m.[k]
let tryfind k (m:map<_,_>) = m.TryFind(k)
let remove k (m:map<_,_>) = m.Remove(k)
let mem k (m:map<_,_>) = m.ContainsKey(k)
let iter f (m:map<_,_>) = m.Iterate(f)
let first f (m:map<_,_>) = m.First(fun k v -> if f k v then Some (k,v) else None)
let exists f (m:map<_,_>) = m.Exists(f)
let forall f (m:map<_,_>) = m.ForAll(f)
let map f (m:map<_,_>) = m.MapRange(f)
let mapi f (m:map<_,_>) = m.Map(f)
let fold f (m:map<_,_>) x = m.Fold f x
let to_list (m:map<_,_>) = m.ToList()
let fold_section lo hi f (m:map<_,_>) x = m.FoldSection lo hi f x

let is_empty (m:map<_,_>) = m.IsEmpty

let fmap f z (m:map<_,_>) =
  let m,z = m.FoldAndMap (fun k v z -> let z,v' = f z k v in v',z) z in
  z,m

let choose f  (m:map<_,_>) = m.First(f)
  
let chooseL f  (m:map<_,_>) =
  m.Fold (fun k v s -> match f k v with None -> s | Some x -> x::s) []
    
let of_list m xs = List.fold (fun m (k,v) -> add k v m) (empty m) xs
let of_FlatList m xs = FlatList.fold (fun m (k,v) -> add k v m) (empty m) xs

let keys   m = chooseL (fun k v -> Some k) m 
let values m = chooseL (fun k v -> Some v) m

let mem_of m k = mem k m
