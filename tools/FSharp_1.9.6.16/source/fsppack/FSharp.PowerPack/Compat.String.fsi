// (c) Microsoft Corporation 2005-2009.

#if INTERNALIZED_POWER_PACK
namespace (* internal *) Internal.Utilities
#else
namespace Microsoft.FSharp.Compatibility
#endif

/// Compatibility module for string processing.  Richer string operations
/// are available via the member functions on strings and other functionality in
/// the <c>System.String</c> type
/// and the <c>System.Text.RegularExpressions</c> namespace.
module String = 

    /// Return a string with the first character converted to uppercase.
    val capitalize: string -> string

    /// Return a string with the first character converted to lowercase.
    val uncapitalize: string -> string

#if FX_NO_STRING_SPLIT_OPTIONS
#else
    /// Split the string using the given list of separator characters.
    /// Trimming is also performed at both ends of the string and any empty
    /// strings that result from the split are discarded.
    val split: char list -> (string -> string list)
#endif

    /// Removes all occurrences of a set of characters specified in a
    /// list from the beginning and end of this instance.
    val trim: char list -> (string -> string)


    /// Compare the given strings using ordinal comparison
    [<OCamlCompatibility("Consider using 'Operators.compare' instead")>]
    val compare: string -> string -> int

    /// Returns the character at the specified position in the string
    [<OCamlCompatibility("Consider using 'str.[i]' instead")>]
    val get: string -> int -> char

    /// Return a substring of length 'length' starting index 'start'.
    [<OCamlCompatibility("Consider using 'str.[i]' instead")>]
    val sub: string -> start:int -> length:int -> string

    /// Return a new string with all characters converted to lowercase
    [<OCamlCompatibility("Consider using 'str.ToLower()' instead")>]
    val lowercase: string -> string

    /// Return a string of the given length containing repetitions of the given character
    [<OCamlCompatibility("Consider using 'String.replicate' instead")>]
    val make: int -> char -> string

    /// Return s string of length 1 containing the given character
    [<OCamlCompatibility("Consider using the overloaded 'string' operator instead")>]
    val of_char: char -> string

    /// Return true is the given string contains the given character
    [<OCamlCompatibility("Consider using 'str.Contains' instead")>]
    val contains: string -> char -> bool

    /// Return true is the given string contains the given character in the
    /// range specified by the given start index and the given length
    [<OCamlCompatibility("Consider using 'str.IndexOf' instead")>]
    val contains_between: string -> start:int -> length:int -> char -> bool

    /// Return true is the given string contains the given character in the
    /// range from the given start index to the end of the string.
    [<OCamlCompatibility("Consider using 'str.IndexOf' instead")>]
    val contains_from: string -> int -> char -> bool

    /// Return the first index of the given character in the
    /// string.  Raise <c>KeyNotFoundException</c> if
    /// the string does not contain the given character.
    [<OCamlCompatibility("Consider using 'str.IndexOf' instead")>]
    val index: string -> char -> int

    /// Return the first index of the given character in the
    /// range from the given start position to the end of the string.  
    /// Raise <c>KeyNotFoundException</c> if
    /// the string does not contain the given character.
    [<OCamlCompatibility("Consider using 'str.IndexOf' instead")>]
    val index_from: string -> start:int -> char -> int

    /// Return true if the string contains the given character prior to the given index
    [<OCamlCompatibility("Consider using 'str.IndexOf' instead")>]
    val rcontains_from: string -> start:int -> char -> bool

    /// Return the index of the first occurrence of the given character 
    /// from the end of the string proceeding backwards
    [<OCamlCompatibility("Consider using 'str.IndexOf' instead")>]
    val rindex: string -> char -> int

    /// Return the index of the first occurrence of the given character 
    /// starting from the given index proceeding backwards.
    [<OCamlCompatibility("Consider using 'str.IndexOf' instead")>]
    val rindex_from: string -> start:int -> char -> int

    /// Return a string with all characters converted to uppercase.
    [<OCamlCompatibility("Consider using 'str.ToUpper' instead")>]
    val uppercase: string -> string

