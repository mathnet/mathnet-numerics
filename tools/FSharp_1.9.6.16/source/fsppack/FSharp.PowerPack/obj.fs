//==========================================================================
// (c) Microsoft Corporation 2005-2008.  
//===========================================================================

#if INTERNALIZED_POWER_PACK
module internal Internal.Utilities.Obj
#else
module Microsoft.FSharp.Compatibility.OCaml.Obj
#endif

type t = obj
let repr x = box x
let obj (x:obj) = unbox x
let magic x = obj (repr x)
let nullobj = (null : obj)


let eq (x: 'a)  (y: 'a) = LanguagePrimitives.PhysicalEquality x y
let not_eq (x:'a) (y:'a) = not (LanguagePrimitives.PhysicalEquality x y)
