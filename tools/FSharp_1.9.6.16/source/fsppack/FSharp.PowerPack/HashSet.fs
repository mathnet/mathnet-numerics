// (c) Microsoft Corporation 2005-2009. 


#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities
#else
namespace Microsoft.FSharp.Collections
#endif

open System
open System.Collections
open System.Collections.Generic

// HashSets are currently implemented using the .NET Dictionary type. 
[<Sealed>]
type HashSet<'a>(t: Dictionary<'a,int>) = 

    new (size:int,hasheq: IEqualityComparer<'a>) = 
        new HashSet<_>(new Dictionary<_,_>(size,hasheq))

    [<OverloadID("new_size")>]
    new (size:int) = 
        new HashSet<'a>(size,HashIdentity.Structural)

    new () = 
        new HashSet<'a>(11)

    [<OverloadID("new_seq")>]
    new (seq:seq<'a>) as t = 
        new HashSet<'a>(1)
        then seq |> Seq.iter t.Add
        
    static member Create(size:int,hasheq: IEqualityComparer<'a>) = new HashSet<_>(size,hasheq)

    [<OverloadID("Create_size")>]
    static member Create(size:int) = new HashSet<'a>(size)

    static member Create() =  new HashSet<'a>()

    [<OverloadID("Create_seq")>]
    static member Create(seq:seq<'a>) = new HashSet<'a>(seq)
        
    member x.Add(y)    = t.[y] <- 0

    member x.Clear() = t.Clear()

    member x.Copy() : HashSet<'a>  = 
        let t2 = new Dictionary<'a,int>(t.Count,t.Comparer) in 
        t |> Seq.iter (fun kvp -> t2.[kvp.Key] <- 0); 
        new HashSet<'a>(t2)

    member x.Fold f acc = 
        let mutable res = acc
        for kvp in t do
            res <- f kvp.Key res
        res

    member x.Iterate(f) =  t |> Seq.iter (fun kvp -> f kvp.Key)

    member x.Contains(y) = t.ContainsKey(y)
    member x.Remove(y) = t.Remove(y) |> ignore
    member x.Count = t.Count
    interface IEnumerable<'a> with
        member x.GetEnumerator() = t.Keys.GetEnumerator() :> IEnumerator<_>
    interface System.Collections.IEnumerable with
        member x.GetEnumerator() = t.Keys.GetEnumerator()  :> IEnumerator 
