// (c) Microsoft Corporation 2005-2009.

#if INTERNALIZED_POWER_PACK
namespace Internal.Utilities
#else
namespace Microsoft.FSharp.Compatibility
#endif

#nowarn "62" // ocaml compat

open System
open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open System.Collections.Generic

/// Immutable sets implemented via binary trees
module Set = 
    /// For use when not opening the Set module, e.g. Set.t
    [<Obsolete("This abbreviation should no longer be used in F# code")>]
    type 'T t = Microsoft.FSharp.Collections.Set<'T>  

    [<OCamlCompatibility>]
    val cardinal: Set<'T> -> int

    ///The elements of the set as a list.
    [<OCamlCompatibility>]
    val elements: Set<'T> -> 'T list

    // ///Apply the given accumulating function to all the elements of the set
    // [<OCamlCompatibility>]
    // val fold: ('T -> 'State -> 'State) -> Set<'T> -> 'State -> 'State

    ///Compute the intersection of the two sets.
    [<OCamlCompatibility>]
    val inter: Set<'T> -> Set<'T> -> Set<'T>



#if INTERNALIZED_POWER_PACK
#else
    open Microsoft.FSharp.Collections

    ///A collection of operations for creating and using sets based on a particular comparison function.
    ///The 'Tag' type parameter is used to track information about the comparison function.
    [<OCamlCompatibility>]
    type Provider<'T,'Tag> when 'Tag :> IComparer<'T> =
       interface
         abstract empty: Tagged.Set<'T,'Tag>;
         abstract is_empty: Tagged.Set<'T,'Tag> -> bool;
         abstract mem: 'T -> Tagged.Set<'T,'Tag> -> bool;
         abstract add: 'T -> Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag>;
         abstract singleton: 'T -> Tagged.Set<'T,'Tag>;
         abstract remove: 'T -> Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag>;
         abstract union: Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag>;
         abstract inter: Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag>;
         abstract diff: Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag>;
         abstract iter: ('T -> unit) -> Tagged.Set<'T,'Tag> -> unit;
         abstract elements: Tagged.Set<'T,'Tag> -> 'T list;
         abstract equal: Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag> -> bool;
         abstract subset: Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag> -> bool;
         abstract compare: Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag> -> int;
         abstract for_all: ('T -> bool) -> Tagged.Set<'T,'Tag> -> bool;
         abstract exists: ('T -> bool) -> Tagged.Set<'T,'Tag> -> bool;
         abstract filter: ('T -> bool) -> Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag>;
         abstract partition: ('T -> bool) -> Tagged.Set<'T,'Tag> -> Tagged.Set<'T,'Tag> * Tagged.Set<'T,'Tag>;
         abstract fold:  ('T -> 'State -> 'State) -> Tagged.Set<'T,'Tag> -> 'State -> 'State;
         abstract cardinal: Tagged.Set<'T,'Tag> -> int;
         abstract min_elt: Tagged.Set<'T,'Tag> -> 'T;
         abstract max_elt: Tagged.Set<'T,'Tag> -> 'T;
         abstract choose: Tagged.Set<'T,'Tag> -> 'T 
       end
         

    [<OCamlCompatibility>]
    type Provider<'T> = Provider<'T, IComparer<'T>>

    /// Build a collection of operations for creating and using 
    /// maps based on a single consistent comparison function. This returns a record
    /// that contains the functions you use to create and manipulate maps all of which 
    /// use this comparison function.  The returned value is much like an ML module. 
    ///
    /// Use MakeTagged if you want additional type safety that guarantees that two sets
    /// based on different comparison functions can never be combined in inconsistent ways.
    
    [<OCamlCompatibility>]
    val Make: ('T -> 'T -> int) -> Provider<'T>

    ///A functor to build a collection of operations for creating and using 
    /// sets based on the given comparison function. This returns a record that 
    /// contains the functions you use to create and manipulate maps of
    /// this kind.  The returned value is much like an ML module. 
    ///
    /// To use this function you need to define a new named class that implements IComparer and
    /// pass an instance of that class as the first argument. For example:
    ///      type MyComparer() = 
    ///          interface IComparer&lt;string&gt; with 
    ///            member self.Compare(x,y) = ...
    ///
    /// let MyStringSetProvider = Set.MakeTagged(new MyComparer())
    [<OCamlCompatibility>]
    val MakeTagged: ('Tag :> IComparer<'T>) -> Provider<'T,'Tag>

#endif

