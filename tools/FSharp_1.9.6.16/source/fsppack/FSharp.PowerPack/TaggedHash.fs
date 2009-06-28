// (c) Microsoft Corporation 2005-2009. 

#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities.Collections.Tagged
#else
namespace Microsoft.FSharp.Collections.Tagged
#endif

    #nowarn "51"


    open System
    open System.Collections.Generic
#if INTERNALIZED_POWER_PACK
    open Internal.Utilities
    open Internal.Utilities.Collections
    type UntaggedHashMultiMap<'key,'v> = Internal.Utilities.Collections.HashMultiMap<'key,'v>
#else
    open Microsoft.FSharp.Collections
    type UntaggedHashMultiMap<'key,'v> = Microsoft.FSharp.Collections.HashMultiMap<'key,'v>
#endif

    type HashMultiMap<'key,'v,'hashTag>
         when 'hashTag :> IEqualityComparer<'key> =
        { t : UntaggedHashMultiMap<'key,'v> }

        static member Create(hasheq: 'hashTag,n:int)  : HashMultiMap<'key,'v,'hashTag> = 
            { t = UntaggedHashMultiMap<_,_>.Create(hasheq,n) }

        member x.Add(y,z) = x.t.Add(y,z)
        member x.Clear() = x.t.Clear()
        member x.Copy() : HashMultiMap<'key,'v,'hashTag>  = { t = x.t.Copy() }
        member x.Item with get(y) = x.t.[y]
                      and  set y z = x.t.[y] <- z
        member x.FindAll(y) = x.t.FindAll(y) 
        member x.Fold f acc =  x.t.Fold f acc
        member x.Iterate(f) =  x.t.Iterate(f)
        member x.Contains(y) = x.t.ContainsKey(y)
        member x.ContainsKey(y) = x.t.ContainsKey(y)
        member x.Remove(y) = x.t.Remove(y)
        member x.Replace(y,z) = x.t.Replace(y,z)
        member x.TryFind(y) = x.t.TryFind(y)
        member x.Count = x.t.Count

    [<Sealed>]
    type HashSet<'a,'hashTag> 
         when 'hashTag :> IEqualityComparer<'a>(t:  HashSet<'a>) =

        static member Create(hasheq: ('hashTag :> IEqualityComparer<'a>),size:int) : HashSet<'a,'hashTag> = 
            new HashSet<'a,'hashTag>(HashSet<_>(size,hasheq))

        member x.Add(y)    = t.Add(y)
        member x.Clear() = t.Clear()
        member x.Copy() = new HashSet<'a,'hashTag>(t.Copy())
        member x.Fold f acc = t.Fold f acc
        member x.Iterate(f) =  t.Iterate(f)
        member x.Contains(y) = t.Contains(y)
        member x.Remove(y) = t.Remove(y)
        member x.Count = t.Count

        interface IEnumerable<'a> with
            member x.GetEnumerator() = (t :> seq<_>).GetEnumerator() 

        interface System.Collections.IEnumerable with 
            member x.GetEnumerator() = (t :> System.Collections.IEnumerable).GetEnumerator()  

    type HashSet<'a> = HashSet<'a, IEqualityComparer<'a>>    
    type HashMultiMap<'key,'a> = HashMultiMap<'key,'a, IEqualityComparer<'key>>    
