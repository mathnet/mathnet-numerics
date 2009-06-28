// (c) Microsoft Corporation. All rights reserved
#light

namespace Internal.Utilities.Collections
open System
open System.Collections.Generic

#nowarn "44" // This construct is deprecated. This F# library function has been renamed. Use 'isSome' instead

type ValueStrength<'T> =
   | Strong of 'T
   | Weak of WeakReference

type internal AgedLookup<'TKey,'TValue>(keepStrongly:int, ?areSame) =
    /// The list of items stored. Youngest is at the end of the list.
    /// The choice of order is somewhat aribtrary. If the other way then adding
    /// items would be O(1) and removing O(N).
    let mutable refs:('TKey*ValueStrength<'TValue>) list = [] 
    
    // The 75 here determines how long the list should be passed the end of strongly held
    // references. Some operations are O(N) and we don't want to let things get out of
    // hand.
    let keepTotal : int = max keepStrongly 75 
    
    /// Default behavior of areSame function is structural equality (=)
    let AreSame(key,key') = 
        match areSame with 
        | Some(f)->f(key,key') 
        | None-> key = key'    
    
    /// Look up a the given key, return None if not found.
    let TryPeekKeyValueImpl(data,key) = 
        let rec Lookup key = function 
            // Treat a list of key-value pairs as a lookup collection.
            // This function returns true if two keys are the same according to the predicate
            // function passed in.
            | []->None
            | (key',value)::t->
                if AreSame(key,key') then Some(key',value) 
                else Lookup key t      
        Lookup key data    
        
    /// Determines whether a particular key exists.
    let Exists(data,key) = 
        match TryPeekKeyValueImpl(data,key) with
        | Some(_)->true
        | None->false    
        
    /// Set a particular key's value.
    let Add(data,key,value) = 
        data @ [key,value]   
        
    /// Remove a particular key value 
    let RemoveImpl(data, key) = 
        data |> List.filter (fun (key',_)->not (AreSame(key,key')))          
        
    /// Remove the stalest item from the list
    let RemoveStalest(data) = 
       match data with 
       | (_::t)->t 
       | _->data   
       
    let TryGetKeyValueImpl(data,key) = 
        match TryPeekKeyValueImpl(data,key) with 
        | Some(_, value) as result ->
            // If the result existed, move it to the top of the list.
            let data = RemoveImpl(data,key)
            let data = Add(data,key,value)
            result,data
        | None -> None,data          
       
    let FilterAndHold() =
        [ for (key,value) in refs do
            match value with
            | Strong(value) -> yield (key,value)
            | Weak(weakReference) ->
                match weakReference.Target with 
                | null -> ()
                | value -> yield key,(value:?>'TValue) ]

        
    let AssignWithStrength(newdata) = 
        let actualLength = List.length newdata
        let tossThreshold = max 0 (actualLength - keepTotal) // Delete everything less than this threshold
        let weakThreshhold = max 0 (actualLength - keepStrongly) // Weaken everything less than this threshhold
        
        refs<-
            newdata  
            |> List.mapi( fun n kv -> n,kv ) // Place the index.
            |> List.filter (fun (n:int,kv) -> n >= tossThreshold) // Delete everything below the toss threshhold
            |> List.map( fun (n:int,(k,v)) -> k,if n<weakThreshhold then Weak(WeakReference(v)) else Strong(v) )
        
    member al.TryPeekKeyValue(key) = 
        // Returns the original key value as well since it may be different depending on equality test.
        let data = FilterAndHold()
        TryPeekKeyValueImpl(data,key)
        
    member al.TryGetKeyValue(key) = 
        let data = FilterAndHold()
        let result,newdata = TryGetKeyValueImpl(data,key)
        AssignWithStrength(newdata)
        result
    member al.TryGet(key) = 
        let data = FilterAndHold()
        let result,newdata = TryGetKeyValueImpl(data,key)
        AssignWithStrength(newdata)
        match result with
        | Some(key',value) -> Some(value)
        | None -> None
    member al.Put(key,value) = 
        let data = FilterAndHold()
        let data = if Exists(data,key) then RemoveImpl(data,key) else data
        let data = Add(data,key,value)
        AssignWithStrength(data) // This will remove extras 
    member al.Remove(key) = 
        let data = FilterAndHold()
        let newdata = RemoveImpl(data,key)
        AssignWithStrength(newdata)
    member al.MostRecent : ('TKey*'TValue) option=  
        let data = FilterAndHold()
        if not data.IsEmpty then 
           // Non-optimal reverse list to get most recent. Consider an array of option for the data structure.
           Some(data |> List.rev |> List.hd)
        else None        
    member al.Clear() =
       refs <- []
    member al.ToSequence() =
        FilterAndHold()
        |> List.to_seq
           
        

type internal MruCache<'TKey,'TValue>(n,compute, ?areSame, ?isStillValid : 'TKey*'TValue->bool, ?areSameForSubsumption, ?logComputedNewValue, ?logUsedCachedValue) =

    /// Default behavior of areSame function is structural equality (=)
    let AreSame(key,key') = 
        match areSame with 
        | Some(f)->f(key,key') 
        | None-> key = key'
        
    /// Default behavior of areSame function is structural equality (=)
    let AreSameForSubsumption(key,key') : bool = 
        match areSameForSubsumption with 
        | Some(f)->f(key,key')
        | None->AreSame(key,key')   
        
    /// The list of items in the cache. Youngest is at the end of the list.
    /// The choice of order is somewhat aribtrary. If the other way then adding
    /// items would be O(1) and removing O(N).
    let cache = AgedLookup<'TKey,'TValue>(n,AreSameForSubsumption)
        
    /// Whether or not this result value is still valid.
    let IsStillValid(key,value) = 
        match isStillValid with 
        | Some(f)->f(key,value) 
        | None-> true        
        
    let Log = function
        | Some(f) -> f
        | None-> fun x->()

    /// Log a message when a new value is computed.        
    let LogComputedNewValue = Log logComputedNewValue
        
    /// Log a message when an existing value was retrieved from cache.
    let LogUsedCachedValue =  Log logUsedCachedValue
                
    member bc.GetAvailable(key) = 
        match cache.TryPeekKeyValue(key) with
        | Some(key', value)->
            if AreSame(key',key) then Some(value)
            else None
        | None -> None
       
    member bc.Get(key) = 
        let Compute() = 
            let value = compute key
            cache.Put(key, value)
            LogComputedNewValue(key)
            value        
        match cache.TryGetKeyValue(key) with
        | Some(key', value) as result -> 
            if AreSame(key', key) && IsStillValid(key,value) then
                LogUsedCachedValue(key)
                value
            else Compute()
        | None -> Compute()
           
    member bc.MostRecent = 
        cache.MostRecent
       
    member bc.SetAlternate(key:'TKey,value:'TValue) = 
        cache.Put(key,value)
       
    member bc.Remove(key) = 
        cache.Remove(key)
       
    member bc.Clear() =
        cache.Clear()

