
//==========================================================================
// (c) Microsoft Corporation 2005-2008.  The interface to the module 
// is similar to that found in versions of other ML implementations, 
// but is not an exact match.  The type signatures in this interface
// are an edited version of those generated automatically by running 
// "bin\fsc.exe -i" on the implementation file.
//===========================================================================

#if INTERNALIZED_POWER_PACK
module internal Internal.Utilities.Obj
#else
module Microsoft.FSharp.Compatibility.OCaml.Obj
#endif

[<OCamlCompatibility("Consider using 'obj' instead")>]
type t = obj

[<OCamlCompatibility("Consider using 'box' and/or 'unbox' instead")>]
val magic: 'a -> 'b

[<OCamlCompatibility("Consider using 'null' instead")>]
val nullobj: obj

/// See Microsoft.FSharp.Core.Operators.unbox
[<OCamlCompatibility("Consider using 'unbox' instead")>]
val obj: obj -> 'a

/// See Microsoft.FSharp.Core.Operators.box
[<OCamlCompatibility("Consider using 'box' instead")>]
val repr: 'a -> obj

/// See Microsoft.FSharp.Core.LanguagePrimitives.PhysicalEquality
[<OCamlCompatibility("Consider using 'Microsoft.FSharp.Core.LanguagePrimitives.PhysicalEquality' instead")>]
val eq: 'a -> 'a -> bool

[<OCamlCompatibility("Consider using 'not(Microsoft.FSharp.Core.LanguagePrimitives.PhysicalEquality(...))' instead")>]
/// Negation of Obj.eq (i.e. reference/physical inequality)
val not_eq: 'a -> 'a -> bool

