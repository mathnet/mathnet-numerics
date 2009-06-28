//==========================================================================
// Basic enum-related operations 
//
// (c) Microsoft Corporation 2005-2009.  
//=========================================================================

namespace Microsoft.FSharp.Compatibility

open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections

/// Simple operations to convert between .NET enuemration types and integers
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Enum= 

    /// Convert an enumeration value to an integer.  The argument type is inferred from context.
    [<System.Obsolete("Use the overloaded conversion function 'int' as a replacement")>]
    val to_int: 'T -> int             when 'T :> System.Enum
    /// Convert an integer to an enumeration value.  The result type is inferred from context.
    [<System.Obsolete("Use the overloaded conversion function 'enum' as a replacement")>]
    val of_int: int -> 'T             when 'T :> System.Enum

    /// Combine enum values using 'logical or'. The relevant enumeration type is inferred from context.
    [<System.Obsolete("Use the infix operator '|||' as a replacement")>]
    val combine: 'T list -> 'T  when 'T :> System.Enum

    /// Test if an enumeration value has a particular flag set, using 'logical and'. 
    /// The relevant enumeration type is inferred from context.
    [<System.Obsolete("Use the expression form '(v1 &&& v2) <> enum 0' as a replacement")>]
    val test: 'T -> 'T -> bool   when 'T :> System.Enum
