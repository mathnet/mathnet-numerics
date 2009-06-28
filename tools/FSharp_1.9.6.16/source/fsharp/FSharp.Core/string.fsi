//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Core

    open System
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Collections

    /// Functional programming operators for string processing.  Further string operations
    /// are available via the member functions on strings and other functionality in
    ///  <a href="http://msdn2.microsoft.com/en-us/library/system.string.aspx">System.String</a> 
    /// and <a href="http://msdn2.microsoft.com/library/system.text.regularexpressions.aspx">System.Text.RegularExpressions</a> types.
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module String = 

        /// Return a new string made by concatenating the given strings
        /// with separator 'sep', i.e. 'a1 + sep + ... + sep + aN'
        val concat: sep:string -> strings: seq<string> -> string

        /// Apply the function <c>action</c> to each character in the string.
        val iter: action:(char -> unit) -> str:string -> unit

        /// Apply the function <c>action</c> to the index of each character in the string and the character itself.
        val iteri: action:(int -> char -> unit) -> str:string -> unit

        /// Build a new string whose characters are the results of applying the function <c>mapping</c>
        /// to each of the characters of the input string.
        val map: mapping:(char -> char) -> str:string -> string

        /// Build a new string whose characters are the results of applying the function <c>mapping</c>
        /// to each character in the string and the character itself.
        val mapi: mapping:(int -> char -> char) -> str:string -> string

        /// Build a new string whose characters are the results of applying the function <c>mapping</c>
        /// to each of the characters of the input string and concatenating the resulting
        /// strings.
        val collect: mapping:(char -> string) -> str:string -> string

        /// Build a new string whose characters are the results of applying the function <c>mapping</c>
        /// to each index from <c>0</c> to <c>count-1</c> and concatenating the resulting
        /// strings.
        val init: count:int -> initializer:(int -> string) -> string

        /// Test if all characters in the string satisfy the given predicate.
        val forall: predicate:(char -> bool) -> str:string -> bool

        /// Test if any character of the string satisfies the given predicate.
        val exists: predicate:(char -> bool) -> str:string -> bool

        /// Return a string by concatenating <c>count</c> instances of <c>str</c>.
        val replicate: count:int -> str: string -> string

        /// Return the length of the string.
        val length: str:string -> int

#if DONT_INCLUDE_DEPRECATED
#else
        [<Obsolete("This F# library function has been renamed. Use 'collect' instead")>]
        val map_concat: mapping:(char -> string) -> str:string -> string

        [<Obsolete("This F# library function has been renamed. Use 'forall' instead")>]
        val for_all: predicate:(char -> bool) -> str:string -> bool
#endif
