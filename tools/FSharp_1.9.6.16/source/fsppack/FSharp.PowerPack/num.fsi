//==========================================================================
// (c) Microsoft Corporation 2005-2008.   The interface to the module 
// is similar to that found in versions of other ML implementations, 
// but is not an exact match.  The type signatures in this interface
// are an edited version of those generated automatically by running 
// "bin\fsc.exe -i" on the implementation file.
//===========================================================================

[<System.Obsolete("This module will be removed in a future release. It is a thin wrapper over the type Microsoft.FSharp.Math.BigRational. Consider using that type directly")>]
module Microsoft.FSharp.Compatibility.OCaml.Num
open Microsoft.FSharp.Compatibility.OCaml
open Microsoft.FSharp.Compatibility.OCaml.Pervasives
open Microsoft.FSharp.Core
open Microsoft.FSharp.Math

// Note: the BigNum type support overloaded operators in F#
type num = bignum

val Big_int: bigint -> num
val Int: int -> num

val minus_num   : num -> num
  
val add_num  : num -> num -> num 
val sub_num  : num -> num -> num 
val mult_num  : num -> num -> num 
val div_num  : num -> num -> num
val abs_num   : num -> num
val succ_num  : num -> num
val pred_num  : num -> num
val pow_num   : num -> int -> num
val incr_num  : num ref -> unit
val decr_num  : num ref -> unit
val sign_num: num -> int

val string_of_num: num -> string
val num_of_string: string -> num 

val ( +/ )      : num -> num -> num 
val ( -/ )      : num -> num -> num 
val ( */ )      : num -> num -> num 

val ( >/ )      : num -> num -> bool
val ( </ )      : num -> num -> bool
val ( <=/ )      : num -> num -> bool
val ( >=/ )      : num -> num -> bool
val ( <>/ )      : num -> num -> bool
val ( =/ )      : num -> num -> bool

val compare_num : num -> num -> int
val max_num  : num -> num -> num 
val min_num  : num -> num -> num
val float_of_num: num -> float 
val int_of_num: num -> int
val big_int_of_num: num -> bigint
val num_of_big_int: bigint -> num

