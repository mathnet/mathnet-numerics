//==========================================================================
// (c) Microsoft Corporation 2005-2008.   The interface to the module 
// is similar to that found in versions of other ML implementations, 
// but is not an exact match.  The type signatures in this interface
// are an edited version of those generated automatically by running 
// "bin\fsc.exe -i" on the implementation file.
//===========================================================================

/// Big_int compatability module for arbitrary sized integers.
[<OCamlCompatibility("The Big_int module is a thin wrapper over the type Microsoft.FSharp.Math.BigInt, and the corresponding module of operations Microsoft.FSharp.Math.BigInt. We recommend using these directly")>]
module Microsoft.FSharp.Compatibility.OCaml.Big_int

open Microsoft.FSharp.Compatibility.OCaml
open Microsoft.FSharp.Compatibility.OCaml.Pervasives
open Microsoft.FSharp.Math

type big_int = bigint

val zero_big_int                    : big_int
val unit_big_int                    : big_int
val minus_big_int                   : big_int -> big_int -> big_int
val add_big_int                     : big_int -> big_int -> big_int
val succ_big_int                    : big_int -> big_int
val add_int_big_int                 : int     -> big_int -> big_int
val sub_big_int                     : big_int -> big_int -> big_int
val pred_big_int                    : big_int -> big_int
val mult_big_int                    : big_int -> big_int -> big_int
val mult_int_big_int                : int     -> big_int -> big_int
val square_big_int                  : big_int -> big_int
val quomod_big_int                  : big_int -> big_int -> big_int * big_int
val div_big_int                     : big_int -> big_int -> big_int
val mod_big_int                     : big_int -> big_int -> big_int
val gcd_big_int                     : big_int -> big_int -> big_int
#if FX_ATLEAST_40
#else
val power_int_positive_int          : int     -> int -> big_int
val power_big_int_positive_int      : big_int -> int -> big_int
val power_int_positive_big_int      : int     -> big_int -> big_int
val power_big_int_positive_big_int  : big_int -> big_int -> big_int
val sign_big_int                    : big_int -> int
val compare_big_int                 : big_int -> big_int -> int
#endif
val eq_big_int                      : big_int -> big_int -> bool
val le_big_int                      : big_int -> big_int -> bool
val ge_big_int                      : big_int -> big_int -> bool
val lt_big_int                      : big_int -> big_int -> bool
val gt_big_int                      : big_int -> big_int -> bool
val max_big_int                     : big_int -> big_int -> big_int
val min_big_int                     : big_int -> big_int -> big_int
val string_of_big_int               : big_int -> string
val big_int_of_string               : string  -> big_int
val int_of_big_int                  : big_int -> int    
val big_int_of_int                  : int     -> big_int
val float_of_big_int                : big_int -> float
