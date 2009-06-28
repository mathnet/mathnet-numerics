// (c) Microsoft Corporation 2005-2009. 


/// Generate the hash/compare functions we add to user-defined types by default.
module internal Microsoft.FSharp.Compiler.Augment 

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler 

open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Env

val CheckAugmentationAttribs : TcGlobals -> Tycon -> unit
val TyconIsAugmentedWithCompare  : TcGlobals -> Tycon -> bool
val TyconIsAugmentedWithEquals   : TcGlobals -> Tycon -> bool
val TyconIsAugmentedWithHash     : TcGlobals -> Tycon -> bool

val MakeValsForCompareAugmentation   : TcGlobals -> TyconRef -> Val * Val
val MakeValsForCompareWithComparerAugmentation : TcGlobals -> TyconRef -> Val
val MakeValsForEqualsAugmentation    : TcGlobals -> TyconRef -> Val * Val
val MakeValsForEqualityWithComparerAugmentation   : TcGlobals -> TyconRef -> Val * Val

val MakeBindingsForCompareAugmentation : TcGlobals -> Tycon -> Binding list
val MakeBindingsForCompareWithComparerAugmentation : TcGlobals -> Tycon -> Binding list
val MakeBindingsForEqualsAugmentation  : TcGlobals -> Tycon -> Binding list
val MakeBindingsForEqualityWithComparerAugmentation  : TcGlobals -> Tycon -> Binding list
