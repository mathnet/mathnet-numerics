// (c) Microsoft Corporation 2005-2009.  

namespace Microsoft.FSharp.Compatibility

open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core.Operators

/// Simple operations to convert between .NET enuemration types and integers
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Enum = 

    let to_int<'a  when 'a :> System.Enum> (x:'a) = (unbox (box x) : int)
    let of_int<'a  when 'a :> System.Enum> (x:int) : 'a = (unbox (box x) : 'a)

    let rec combine_aux l  = 
      match l with
      | [] -> 0
      | h::t -> (to_int h) ||| (combine_aux t) 
    let combine<'a  when 'a :> System.Enum> (l : 'a list) = (of_int (combine_aux l) : 'a)
    let test<'a  when 'a :> System.Enum> (x : 'a) (y : 'a) = (to_int x) &&& (to_int y) <> 0
