// First version copied from the F# compiler sources
// https://raw.github.com/fsharp/fsharp/master/src/fsharp/FSharp.Core/math/n.fsi

//----------------------------------------------------------------------------
// Copyright (c) 2002-2012 Microsoft Corporation. 
//
// This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
// copy of the license can be found in the License.html file at the root of this distribution. 
// By using this source code in any fashion, you are agreeing to be bound 
// by the terms of the Apache License, Version 2.0.
//
// You must not remove this notice, or any other, from this software.
//----------------------------------------------------------------------------

namespace Microsoft.FSharp.Math

#if FX_NO_BIGINT
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core

/// Abstract internal type
[<NoEquality; NoComparison>]
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
    val toFloat   : BigNat -> float
    val ofInt32     : int    -> BigNat
    val ofInt64   : int64  -> BigNat
    val toString  : BigNat -> string
    val ofString  : string -> BigNat

    val toUInt32  : BigNat -> uint32
    val toUInt64  : BigNat -> uint64
      
    val factorial  : BigNat -> BigNat
    // val randomBits : int -> BigNat    
    val bits       : BigNat -> int
    val isSmall   : BigNat -> bool   (* will fit in int32 (but not nec all int32) *)
    val getSmall  : BigNat -> int32 (* get the value, if it satisfies isSmall *)

#endif
