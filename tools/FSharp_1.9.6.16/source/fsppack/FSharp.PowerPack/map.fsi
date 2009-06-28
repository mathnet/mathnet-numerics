// (c) Microsoft Corporation 2005-2009.

#if INTERNALIZED_POWER_PACK
namespace Internal.Utilities
#else
namespace Microsoft.FSharp.Compatibility
#endif

#nowarn "62" // ocaml compat

open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open System
open System.Collections.Generic

/// Extension functionality for maps using structural comparison
module Map = 

    /// For use when not opening the Map module, e.g. Map.t
    [<Obsolete("This abbreviation should no longer be used in F# code")>]
    type ('Key,'Value) t = Microsoft.FSharp.Collections.Map<'Key,'Value>  

    /// Fold over the bindings in the map 
    [<OCamlCompatibility>]
    val fold: ('Key -> 'Value -> 'State -> 'State) -> Map<'Key,'Value> -> 'State -> 'State


#if INTERNALIZED_POWER_PACK
#else

    open Microsoft.FSharp.Collections

    //--------------------------------------------------------------------------
    // Map.Make functor
    //
    // Immutable maps using user-defined comparison.

    /// A collection of operations for creating and using maps based on a particular comparison function.
    /// The 'Tag type parameter is used to track information about the comparison function.
    [<OCamlCompatibility>]
    type Provider<'Key,'T,'Tag> 
     when 'Tag :> IComparer<'Key> =
        interface
          abstract empty: Tagged.Map<'Key,'T,'Tag>;
          abstract add: 'Key -> 'T -> Tagged.Map<'Key,'T,'Tag> -> Tagged.Map<'Key,'T,'Tag>;
          abstract find: 'Key -> Tagged.Map<'Key,'T,'Tag> -> 'T;
          abstract first: ('Key -> 'T -> 'U option) -> Tagged.Map<'Key,'T,'Tag> -> 'U option;
          abstract tryfind: 'Key -> Tagged.Map<'Key,'T,'Tag> -> 'T option;
          abstract remove: 'Key -> Tagged.Map<'Key,'T,'Tag> -> Tagged.Map<'Key,'T,'Tag>;
          abstract mem: 'Key -> Tagged.Map<'Key,'T,'Tag> -> bool;
          abstract iter: ('Key -> 'T -> unit) -> Tagged.Map<'Key,'T,'Tag> -> unit;
          abstract map:  ('T -> 'U) -> Tagged.Map<'Key,'T,'Tag> -> Tagged.Map<'Key,'U,'Tag>;
          abstract mapi: ('Key -> 'T -> 'U) -> Tagged.Map<'Key,'T,'Tag> -> Tagged.Map<'Key,'U,'Tag>;
          abstract fold: ('Key -> 'T -> 'State -> 'State) -> Tagged.Map<'Key,'T,'Tag> -> 'State -> 'State
        end

    [<OCamlCompatibility>]
    type Provider<'Key,'T> = Provider<'Key,'T,IComparer<'Key>>
    
    [<OCamlCompatibility>]
    val Make: ('Key -> 'Key -> int) -> Provider<'Key,'T>

    /// A functor to build a collection of operations for creating and using 
    /// maps based on the given comparison function. This returns a record that 
    /// contains the functions you use to create and manipulate maps of
    /// this kind.  The returned value is much like an ML module. 
    ///
    /// Language restrictions related to polymorphism may mean you
    /// have to create a new instantiation of for each toplevel
    /// key/value type pair.
    ///
    /// To use this function you need to define a new named class that implements IComparer and
    /// pass an instance of that class as the first argument. For example:
    ///      type MyComparer = 
    ///          new() = { }
    ///          interface IComparer&lt;string&gt; with 
    ///            member self.Compare(x,y) = ...
    ///
    /// let MyStringMapProvider : Map.Provider &lt; string,int &gt; = Map.MakeTagged(new MyComparer())
    [<OCamlCompatibility>]
    val MakeTagged: ('Tag :> IComparer<'Key>) -> Provider<'Key,'T,'Tag>

#endif