// (c) Microsoft Corporation. All rights reserved
(* --------------------------------------------------------------------	
 * Outcomes.  These are used to describe steps of a machine that
 * may raise errors.  The errors can be trapped.
 * --------------------------------------------------------------------	*)

module (* internal *) Microsoft.FSharp.Compiler.Outcome

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler

type 'a outcome = ResultOrException<'a>

let success a = Result a
let raze (b:exn) = Exception b

let trappable_outcome_cases f1 f2 = function
  | Result x -> f1 x
  | Exception err -> f2 err
let trappable_option (exn:exn) res = 
  match res with 
  | Some x -> success(x) 
  | None -> raze exn
let trappable_trywith res f = trappable_outcome_cases success f res
let rec trappable_outcome_first (e:exn) f l1 =
  match l1 with 
  | [] -> raze e 
  | (h1::t1) -> 
      trappable_outcome_cases success 
	(fun x -> trappable_outcome_first e f t1) 
	(f h1)
let (||?>) res f = 
  match res with 
  | Result x -> f x 
  | Exception err -> Exception err

let (|?>) res f = 
  match res with 
  | Result x -> Result(f x )
  | Exception err -> Exception err
  
let razewith s = raze(Failure(s))

let ForceRaise = function
  | Result x -> x
  | Exception err -> raise err

let rec trappable_map f l1 =
  match l1 with 
    [] -> success []
  | (h1::t1) -> 
      f h1 ||?> (fun h -> trappable_map f t1 ||?> (fun t -> success (h::t)))

let rec trappable_map2 f l1 l2 =
  match l1,l2 with 
  | [],[] -> success []
  | [],_ -> razewith "trappable_map2"
  | _,[] -> razewith "trappable_map2"
  | (h1::t1),(h2::t2) -> 
      f h1 h2 ||?> (fun h -> trappable_map2 f t1 t2 ||?> (fun t -> success(h::t)))

 (* x |> otherwise (fun () -> result) *)
let otherwise f x =
   trappable_outcome_cases (fun x -> success(x)) (fun err -> f()) x

	    
