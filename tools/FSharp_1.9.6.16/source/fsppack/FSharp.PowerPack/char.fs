//==========================================================================
// (c) Microsoft Corporation 2005-2009.  
//==========================================================================


#if INTERNALIZED_POWER_PACK
namespace Internal.Utilities.OCaml
#else
namespace Microsoft.FSharp.Compatibility
#endif

open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core.Operators

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
#if INTERNALIZED_POWER_PACK
module internal Char = 
#else
module Char = 
#endif

    let compare (x:char) y = Operators.compare x y

    let code (c:char) = int c 
    let chr (n:int) =  char n 

#if FX_NO_TO_LOWER_INVARIANT
    let lowercase (c:char) = System.Char.ToLower(c, System.Globalization.CultureInfo.InvariantCulture)
    let uppercase (c:char) = System.Char.ToUpper(c, System.Globalization.CultureInfo.InvariantCulture)
#else
    let lowercase (c:char) = System.Char.ToLowerInvariant(c)
    let uppercase (c:char) = System.Char.ToUpperInvariant(c)
#endif
