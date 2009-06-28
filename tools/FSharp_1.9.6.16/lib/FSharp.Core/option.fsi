//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Core

    open System
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Collections

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    /// Basic operations on options.  
    module Option =

        /// Returns true if the option is not None
        val isSome: option:'T option -> bool

        /// Returns true if the option is None
        val isNone: option:'T option -> bool

        /// Gets the value associated with the option.  If the option is None then
        /// raises ArgumentException
        val get: option:'T option -> 'T

        /// <c>length inp</c> evaluates to <c>match inp with None -> 0 | Some _ -> 1</c>
        val count: option:'T option -> int

        /// <c>fold_left f s inp</c> evaluates to <c>match inp with None -> s | Some x -> f s x</c>
        val fold: folder:('State -> 'T -> 'State) -> state:'State -> option:'T option -> 'State

        /// <c>fold_right f inp s</c> evaluates to "match inp with None -> s | Some x -> f x s"
        val foldBack: folder:('T -> 'State -> 'State) -> option:'T option -> state:'State -> 'State

        /// <c>exists p inp</c> evaluates to <c>match inp with None -> false | Some x -> p x</c>
        val exists: predicate:('T -> bool) -> option:'T option -> bool

        /// <c>forall p inp" evaluates to "match inp with None -> true | Some x -> p x</c>
        val forall: predicate:('T -> bool) -> option:'T option -> bool

        /// <c>iter f inp</c> executes <c>match inp with None -> () | Some x -> f x</c>
        val iter: action:('T -> unit) -> option:'T option -> unit

        /// <c>map f inp</c> evaluates to <c>match inp with None -> None | Some x -> Some (f x)</c>
        val map: mapping:('T -> 'U) -> option:'T option -> 'U option

        /// <c>bind f inp</c> evaluates to <c>match inp with None -> None | Some x -> f x</c>
        val bind: binder:('T -> 'U option) -> option:'T option -> 'U option

        /// Convert the option to an array of length 0 or 1
        val to_array: option:'T option -> 'T array

        /// Convert the option to a list of length 0 or 1
        val to_list: option:'T option -> 'T list

#if DONT_INCLUDE_DEPRECATED
#else
        [<Obsolete("This F# library function has been renamed. Use 'isSome' instead")>]
        val is_some: option:'T option -> bool

        [<Obsolete("This F# library function has been renamed. Use 'isNone' instead")>]
        val is_none: option:'T option -> bool

        [<Obsolete("This F# library function has been renamed. Use 'fold' instead")>]
        val fold_left: folder:('State -> 'T -> 'State) -> state:'State -> option:'T option -> 'State

        [<Obsolete("This F# library function has been renamed. Use 'foldBack' instead")>]
        val fold_right: folder:('T -> 'State -> 'State) -> option:'T option -> state:'State -> 'State

        [<Obsolete("This F# library function has been renamed. Use 'forall' instead")>]
        val for_all: predicate:('T -> bool) -> option:'T option -> bool

        /// <c>filter p inp</c> evaluates to <c>match inp with None -> None | Some x -> if p x then inp else None</c>
        [<Obsolete("This function will be removed from the F# library in a future release, as it is rarely used")>]
        val filter: predicate:('T -> bool) -> option:'T option -> 'T option

        /// <c>filter p inp</c> evaluates to <c>match inp with None -> None | Some x -> if p x then inp else None</c>
        [<Obsolete("This function will be removed from the F# library in a future release, as it is rarely used")>]
        val length: option:'T option -> int

        /// <c>partition p inp</c> evaluates to 
        /// <c>match inp with None -> None,None | Some x -> if p x then inp,None else None,inp</c>
        [<Obsolete("This function will be removed from the F# library in a future release, as it is rarely used")>]
        val partition: predicate:('T -> bool) -> option:'T option -> ('T option * 'T option)

#endif
