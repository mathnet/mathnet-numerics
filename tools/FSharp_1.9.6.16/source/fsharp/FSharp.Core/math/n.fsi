//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Math

#if FX_ATLEAST_40
#else
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core

/// Abstract internal type
type internal BigNat 

module internal BigNatModule =

    val zero  : BigNat
    val one   : BigNat
    val two   : BigNat

    val add        : BigNat -> BigNat -> BigNat
    val sub        : BigNat -> BigNat -> BigNat
    val mul        : BigNat -> BigNat -> BigNat
    val divmod     : BigNat -> BigNat -> BigNat * BigNat    
    val div        : BigNat -> BigNat -> BigNat
    val rem        : BigNat -> BigNat -> BigNat
    val hcf        : BigNat -> BigNat -> BigNat

    val min        : BigNat -> BigNat -> BigNat
    val max        : BigNat -> BigNat -> BigNat
    val scale      : int -> BigNat -> BigNat    
    val powi       : BigNat -> int -> BigNat
    val pow        : BigNat -> BigNat -> BigNat

    val IsZero     : BigNat -> bool
    val isZero     : BigNat -> bool
    val isOne      : BigNat -> bool
    val equal      : BigNat -> BigNat -> bool
    val compare    : BigNat -> BigNat -> int
    val lt         : BigNat -> BigNat -> bool
    val gt         : BigNat -> BigNat -> bool
    val lte        : BigNat -> BigNat -> bool
    val gte        : BigNat -> BigNat -> bool

    val hash       : BigNat -> int
    val to_float   : BigNat -> float
    val of_int     : int    -> BigNat
    val of_int64   : int64  -> BigNat
    val to_string  : BigNat -> string
    val of_string  : string -> BigNat

    val to_uint32  : BigNat -> uint32
    val to_uint64  : BigNat -> uint64
      
    val factorial  : BigNat -> BigNat
    // val randomBits : int -> BigNat    
    val bits       : BigNat -> int
    val is_small   : BigNat -> bool   (* will fit in int32 (but not nec all int32) *)
    val get_small  : BigNat -> int32 (* get the value, if it satisfies is_small *)

#endif