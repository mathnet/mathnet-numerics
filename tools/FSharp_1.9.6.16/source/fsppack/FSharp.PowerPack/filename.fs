// (c) Microsoft Corporation 2005-2009. 

#if INTERNALIZED_POWER_PACK
module (* internal *) Internal.Utilities.Filename
#else
module Microsoft.FSharp.Compatibility.OCaml.Filename
#endif

open System.IO

let current_dir_name = "."
let parent_dir_name =  ".."
let concat (x:string) (y:string) = Path.Combine (x,y)
let is_relative (s:string) = not (Path.IsPathRooted(s))

// Case sensitive (original behaviour preserved).
let check_suffix (x:string) (y:string) = x.EndsWith(y,System.StringComparison.Ordinal) 

let chop_suffix (x:string) (y:string) =
    if not (check_suffix x y) then 
        raise (System.ArgumentException("chop_suffix")) // message has to be precisely this, for OCaml compatibility, and no argument name can be set
    x.[0..x.Length-y.Length-1]

let has_extension (s:string) = 
    (s.Length >= 1 && s.[s.Length - 1] = '.' && s <> ".." && s <> ".") 
    || Path.HasExtension(s)

let chop_extension (s:string) =
    if s = "." then "" else // for OCaml compatibility
    if not (has_extension s) then 
        raise (System.ArgumentException("chop_extension")) // message has to be precisely this, for OCaml compatibility, and no argument name can be set
    Path.Combine (Path.GetDirectoryName s,Path.GetFileNameWithoutExtension(s))


let basename (s:string) = 
    Path.GetFileName(s)

let dirname (s:string) = 
    if s = "" then "."
    else 
      match Path.GetDirectoryName(s) with 
      | null -> if Path.IsPathRooted(s) then s else "."
      | res -> if res = "" then "." else res

let is_implicit (s:string) = 
    is_relative s &&
      match Path.GetDirectoryName(s) with 
      | null -> true
      | res -> (res <> current_dir_name && res <> parent_dir_name)


let temp_file (p:string) (s:string) = Path.GetTempFileName()

let quote s = "\'" ^ s ^ "\'"




#if INTERNALIZED_POWER_PACK
// Suppress FxCop uncalled code warnings when compiled into the Compiler
open System.Diagnostics.CodeAnalysis  
[<assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Internal.Utilities.Filename.#chop_suffix(System.String,System.String)")>]
[<assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Internal.Utilities.Filename.#is_implicit(System.String)")>]
[<assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Internal.Utilities.Filename.#get_parent_dir_name()")>]
[<assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Internal.Utilities.Filename.#quote(System.String)")>]
[<assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Internal.Utilities.Filename.#concat(System.String,System.String)")>]
[<assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Internal.Utilities.Filename.#temp_file(System.String,System.String)")>]
[<assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Internal.Utilities.Filename.#basename(System.String)")>]
[<assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Internal.Utilities.Filename.#get_current_dir_name()")>]
[<assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="Internal.Utilities.Filename.#is_relative(System.String)")>]
do()
#else
#endif
