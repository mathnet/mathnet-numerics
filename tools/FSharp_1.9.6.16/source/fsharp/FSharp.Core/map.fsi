//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Collections

    open System
    open System.Collections.Generic
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Collections

    /// Immutable maps.  Keys are ordered by F# generic comparison.
    ///
    /// <performance> 
    ///   Maps based on generic comparison are efficient for small keys. They are not a suitable choice if keys are recursive data structures 
    ///   or if keys require bespoke comparison semantics.
    /// </performance>
    [<Sealed>]
    type Map<'Key,'Value> =
        /// Return a new map with the binding added to the given map.
        member Add: key:'Key * value:'Value -> Map<'Key,'Value>

        /// Return true if there are no bindings in the map.
        member IsEmpty: bool

        /// Build a map that contains the bindings of the given IEnumerable
        new : elements:seq<'Key * 'Value> -> Map<'Key,'Value>

        /// The empty map
        static member Empty: Map<'Key,'Value>

        /// Test if an element is in the domain of the map
        member ContainsKey: key:'Key -> bool

        /// The number of bindings in the map
        member Count: int

        /// Lookup an element in the map. Raise <c>KeyNotFoundException</c> if no binding
        /// exists in the map.
        member Item : key:'Key -> 'Value with get

        /// Remove an element from the domain of the map.  No exception is raised if the element is not present.
        member Remove: key:'Key -> Map<'Key,'Value>

        /// Lookup an element in the map, returning a <c>Some</c> value if the element is in the domain 
        /// of the map and <c>None</c> if not.
        member TryFind: key:'Key -> 'Value option

        interface IDictionary<'Key, 'Value>         
        interface ICollection<KeyValuePair<'Key, 'Value>> 
        interface IEnumerable<KeyValuePair<'Key, 'Value>>         
        interface System.IComparable
        interface System.Collections.IEnumerable 
        override Equals : obj -> bool

    /// Functional programming operators related to the <c>Map&lt;_,_&gt;</c> type.
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Map = 

        /// Return a new map with the binding added to the given map.
        val add: key:'Key -> value:'T -> table:Map<'Key,'T> -> Map<'Key,'T>

        /// Return a new map made from the given bindings
        val of_list: elements:('Key * 'T) list -> Map<'Key,'T>

        /// Return a new map made from the given bindings
        val of_array: elements:('Key * 'T) array -> Map<'Key,'T>

        /// Return a new map made from the given bindings
        val of_seq: elements:seq<'Key * 'T> -> Map<'Key,'T>

        /// View the collection as an enumerable sequence. This collection
        /// type is also directly compatible with 'seq&lt;KeyValuePair&lt;_,_&gt; &gt;'.
        ///
        /// Note this function returns a sequence of tuples, whereas the collection
        /// itself is compatible with the logically equivalent sequence of KeyValuePairs.
        /// Using sequences of tuples tends to be more convenient in F#, however the
        /// collection itself must enumerate KeyValuePairs to conform to the .NET
        /// design guidelines and the IDictionary interface.
        val to_seq: table:Map<'Key,'T> -> seq<'Key * 'T> 

        /// Returns a list of all key-value pairs in the mappinng
        val to_list: table:Map<'Key,'T> -> list<'Key * 'T> 

        /// Returns an array of all key-value pairs in the mappinng
        val to_array: table:Map<'Key,'T> -> ('Key * 'T) array

        /// Is the map empty?
        val isEmpty: table:Map<'Key,'T> -> bool

        [<OCamlCompatibility("This F# library function has been renamed. Use 'isEmpty' instead")>]
        val is_empty: table:Map<'Key,'T> -> bool

        /// The empty map
        [<GeneralizableValueAttribute>]
        val empty<'Key,'T> : Map<'Key,'T>

        /// Lookup an element in the map, raising <c>KeyNotFoundException</c> if no binding
        /// exists in the map.
        val find: key:'Key -> table:Map<'Key,'T> -> 'T

        /// Search the map looking for the first element where the given function returns a <c>Some</c> value
        val tryPick: chooser:('Key -> 'T -> 'U option) -> table:Map<'Key,'T> -> 'U option

        /// Search the map looking for the first element where the given function returns a <c>Some</c> value
        val pick: chooser:('Key -> 'T -> 'U option) -> table:Map<'Key,'T> -> 'U 

        /// Fold over the bindings in the map 
        val foldBack: folder:('Key -> 'T -> 'State -> 'State) -> table:Map<'Key,'T> -> state:'State -> 'State

        /// Fold over the bindings in the map 
        val fold: folder:('State -> 'Key -> 'T -> 'State) -> state:'State -> table:Map<'Key,'T> -> 'State

        /// Apply the given function to each binding in the dictionary
        val iter: action:('Key -> 'T -> unit) -> table:Map<'Key,'T> -> unit

        /// Return true if the given predicate returns true for one of the
        /// bindings in the map.
        val exists: predicate:('Key -> 'T -> bool) -> table:Map<'Key, 'T> -> bool

        /// Build a new map containing only the bindings for which the given predicate returns 'true'
        val filter: predicate:('Key -> 'T -> bool) -> table:Map<'Key, 'T> -> Map<'Key, 'T>

        /// Return true if the given predicate returns true for all of the
        /// bindings in the map.
        val forall: predicate:('Key -> 'T -> bool) -> table:Map<'Key, 'T> -> bool

        /// Build a new collection whose elements are the results of applying the given function
        /// to each of the elements of the collection. The index passed to the
        /// function indicates the index of element being transformed.
        val map: mapping:('Key -> 'T -> 'U) -> table:Map<'Key,'T> -> Map<'Key,'U>

        /// Test is an element is in the domain of the map
        val contains: key:'Key -> table:Map<'Key,'T> -> bool

        /// Build two new maps, one containing the bindings for which the given predicate returns 'true',
        /// and the other the remaining bindings.
        val partition: predicate:('Key -> 'T -> bool) -> table:Map<'Key, 'T> -> Map<'Key, 'T> * Map<'Key, 'T>

        /// Remove an element from the domain of the map.  No exception is raised if the element is not present.
        val remove: key:'Key -> table:Map<'Key,'T> -> Map<'Key,'T>

        /// Lookup an element in the map, returning a <c>Some</c> value if the element is in the domain 
        /// of the map and <c>None</c> if not.
        val tryFind: key:'Key -> table:Map<'Key,'T> -> 'T option

        /// Evaluates the function on each mapping in the collection. Returns the key for the first mapping
        /// where the function returns 'true'. Raise <c>KeyNotFoundException</c> if no such element exists.
        val findIndex: predicate:('Key -> 'T -> bool) -> table:Map<'Key,'T> -> 'Key

        /// Return the key of the first mapping in the collection that satisfies the given predicate. 
        /// Return 'None' if no such element exists.
        val tryFindIndex: predicate:('Key -> 'T -> bool) -> table:Map<'Key,'T> -> 'Key option

#if DONT_INCLUDE_DEPRECATED
#else
        /// Search the map looking for the first element where the given function returns a <c>Some</c> value
        [<Obsolete("This F# library function has been renamed. Use 'tryPick' instead")>]
        val first: chooser:('Key -> 'T -> 'U option) -> table:Map<'Key,'T> -> 'U option

        [<Obsolete("This F# library function has been renamed. Use 'foldBack' instead")>]
        val fold_right: folder:('Key -> 'T -> 'State -> 'State) -> table:Map<'Key,'T> -> state:'State -> 'State

        [<Obsolete("This F# library function has been renamed. Use 'fold' instead")>]
        val fold_left: folder:('State -> 'Key -> 'T -> 'State) -> state:'State -> table:Map<'Key,'T> -> 'State

        [<Obsolete("This F# library function has been renamed. Use 'forall' instead")>]
        val for_all: predicate:('Key -> 'T -> bool) -> table:Map<'Key, 'T> -> bool

        [<Obsolete("This has been renamed to 'map'")>]
        val mapi: mapping:('Key -> 'T -> 'U) -> table:Map<'Key,'T> -> Map<'Key,'U>

        [<OCamlCompatibility("This F# library function has been renamed. Use 'contains' instead")>]
        val mem: key:'Key -> table:Map<'Key,'T> -> bool

        [<Obsolete("This F# library function has been renamed. Use 'tryFind' instead")>]
        val tryfind: key:'Key -> table:Map<'Key,'T> -> 'T option

        [<Obsolete("This F# library function has been renamed. Use 'findIndex' instead")>]
        val find_index: predicate:('Key -> 'T -> bool) -> table:Map<'Key,'T> -> 'Key

        [<Obsolete("This F# library function has been renamed. Use 'tryFindIndex' instead")>]
        val tryfind_index: predicate:('Key -> 'T -> bool) -> table:Map<'Key,'T> -> 'Key option

#endif
