// (c) Microsoft Corporation 2005-2009.  

#if INTERNALIZED_POWER_PACK
module (* internal *) Internal.Utilities.Lazy
#else
module Microsoft.FSharp.Compatibility.Lazy
#endif

open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Core.Operators
open Microsoft.FSharp.Control
open Microsoft.FSharp.Collections


type 'a t = 'a Lazy

exception Undefined = Microsoft.FSharp.Control.Undefined

let force_with_lock (x: Lazy<'T>) = x.SynchronizedForce()
let force_without_lock (x: Lazy<'T>) = x.UnsynchronizedForce()
let force (x: Lazy<'T>) = x.Force()
let force_val (x: Lazy<'T>) = x.Force()
let lazy_from_fun f = Lazy.Create(f)
let create f = Lazy.Create(f)
let lazy_from_val v = Lazy.CreateFromValue(v)
let lazy_is_val (x: Lazy<'T>) = x.IsForced
