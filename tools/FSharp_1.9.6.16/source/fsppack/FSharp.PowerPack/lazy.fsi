// (c) Microsoft Corporation 2005-2009.  

#if INTERNALIZED_POWER_PACK
module (* internal *) Internal.Utilities.Lazy
#else
module Microsoft.FSharp.Compatibility.Lazy
#endif

open Microsoft.FSharp.Core
open Microsoft.FSharp.Control
open Microsoft.FSharp.Collections
open System

type 'T t = Lazy<'T>

exception Undefined = Microsoft.FSharp.Control.Undefined

/// See Lazy.Force
val force: Lazy<'T> -> 'T

/// See Lazy.Force.
[<OCamlCompatibility("Consider using 'v.Force()' instead")>]
val force_val: Lazy<'T> -> 'T

/// Build a lazy (delayed) value from the given computation
[<OCamlCompatibility("Consider using 'lazy' instead")>]
val lazy_from_fun: (unit -> 'T) -> Lazy<'T>

/// Build a lazy (delayed) value from the given pre-computed value.
[<OCamlCompatibility("Consider using 'Lazy.CreateFromValue' instead")>]
val lazy_from_val: 'T -> Lazy<'T>

/// Check if a lazy (delayed) value has already been computed
[<OCamlCompatibility("Consider using 'Lazy.IsForced' instead")>]
val lazy_is_val: Lazy<'T> -> bool

/// See Lazy.SynchronizedForce.
[<Obsolete("Consider using 'v.Force()' instead")>]
val force_with_lock: Lazy<'T> -> 'T

/// See Lazy.UnsynchronizedForce
[<Obsolete("Consider using 'v.UnsynchronizedForce()' instead")>]
val force_without_lock: Lazy<'T> -> 'T

/// Build a lazy (delayed) value from the given computation
[<OCamlCompatibility("Consider using Lazy.Create instead")>]
val create : (unit -> 'T) -> Lazy<'T>
