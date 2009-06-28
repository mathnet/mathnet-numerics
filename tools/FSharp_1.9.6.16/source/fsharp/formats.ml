// (c) Microsoft Corporation. All rights reserved

module internal Microsoft.FSharp.Compiler.Formats

#light

open Internal.Utilities
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 

open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.ConstraintSolver

type format_item = Simple of typ | FuncAndVal 

let copy_and_fixup_format_typar m tp = 
    let _,_,tinst = freshen_and_fixup_typars m TyparFlexible [] [] [tp]
    List.hd tinst

let lowestDefaultPriority = 0 (* See comment on TTyparDefaultsToType *)

let flexible_format_typar g m tys dflt = 
    let tp = NewTypar (KindType,TyparRigid,Typar(mksyn_id m "fmt",HeadTypeStaticReq,true),false,DynamicReq,[])
    fixup_typar_constraints tp [ TTyparSimpleChoice (tys,m); TTyparDefaultsToType (lowestDefaultPriority,dflt,m)];
    copy_and_fixup_format_typar m tp

let flexible_int_format_typar g m = 
    flexible_format_typar g m [ g.byte_ty; g.int16_ty; g.int32_ty; g.int64_ty;  g.sbyte_ty; g.uint16_ty; g.uint32_ty; g.uint64_ty;g.nativeint_ty;g.unativeint_ty; ] g.int_ty
    
    
let flexible_float_format_typar g m = 
    flexible_format_typar g m [ g.float_ty; g.float32_ty; g.decimal_ty ] g.float_ty

let is_digit c = ('0' <= c && c <= '9')

type info = 
  { mutable leftJustify    : bool; 
    mutable numPrefixIfPos : char option;
    mutable addZeros       : bool;
    mutable precision      : bool}

let newInfo ()= 
  { leftJustify    = false;
    numPrefixIfPos = None;
    addZeros       = false;
    precision      = false}

let ParseFormatString m g fmt bty cty dty = 
  let len = String.length fmt

  let rec go acc i = 
   if i >= len then
       let argtys =
           if acc |> List.forall (fun (p, _) -> p = None) then // without positional specifiers
               acc |> List.map snd |> List.rev
           //elif acc |> List.exists (fun (p, _) -> p = None) then
           //    failwith "You must use positional specifiers for all arguments"
           else  // with positional specifiers
               failwith "Positional specifiers are not permitted in format strings";
//                let acc = acc |> List.map (fun (p, t) -> Option.get p, t)
//                let nbArgs : int = acc |> List.max_by fst |> fst
//                let arr = Array.create nbArgs None
//                acc |> List.iter (fun (i,t) ->
//                    match arr.[i-1] with
//                      | None -> arr.[i-1] <- Some t
//                      | Some ty -> if ty <> t then failwithf "Incompatible types for argument %d" i);
//                arr |> Array.map (function None -> new_inference_typ () | Some t -> t)
//                    |> Array.to_list
      
       let aty = List.foldBack (-->) argtys dty
       let ety = mk_tupled_ty g argtys
       aty,ety
   elif System.Char.IsSurrogatePair(fmt,i) then 
      go acc (i+2)
   else 
    let c = fmt.[i]
    match c with
    | '%' ->
        let i = i+1 
        if i >= len then failwith "missing format specifier";
        let info = newInfo()

        let rec flags i =
          if i >= len then failwith "missing format specifier";
          match fmt.[i] with
          | '-' -> 
              if info.leftJustify then failwith "'-' flag set twice";
              info.leftJustify <- true;
              flags(i+1)
          | '+' -> 
              if info.numPrefixIfPos <> None then failwith "prefix flag (' ' or '+') set twice";
              info.numPrefixIfPos <- Some '+';
              flags(i+1)
          | '0' -> 
              if info.addZeros then failwith "'0' flag set twice";
              info.addZeros <- true;
              flags(i+1)
          | ' ' -> 
              if info.numPrefixIfPos <> None then failwith "prefix flag (' ' or '+') set twice";
              info.numPrefixIfPos <- Some ' ';
              flags(i+1)
          | '#' -> failwith "The # formatting modifier is invalid in F#."; 
          | _ -> i

        let rec digits_precision i = 
          if i >= len then failwith "bad precision in format specifier";
          match fmt.[i] with
          | c when is_digit c -> digits_precision (i+1)
          | _ -> i 

        let precision i = 
          if i >= len then failwith "bad width in format specifier";
          match fmt.[i] with
          | c when is_digit c -> info.precision <- true; false,digits_precision (i+1)
          | '*' -> info.precision <- true; true,(i+1)
          | _ -> failwith "precision missing after the '.'"

        let optional_dot_and_precision i = 
          if i >= len then failwith "bad width in format specifier";
          match fmt.[i] with
          | '.' -> precision (i+1)
          | _ -> false,i

        let rec digits_width_and_precision i = 
          if i >= len then failwith "bad width in format specifier";
          match fmt.[i] with
          | c when is_digit c -> digits_width_and_precision (i+1)
          | _ -> optional_dot_and_precision i

        let width_and_precision i = 
          if i >= len then failwith "bad width in format specifier";
          match fmt.[i] with
          | c when is_digit c -> false,digits_width_and_precision i
          | '*' -> true,optional_dot_and_precision (i+1)
          | _ -> false,optional_dot_and_precision i

        let rec digits_position n i =
            if i >= len then failwith "bad width in format specifier";
            match fmt.[i] with
            | c when is_digit c -> digits_position (n*10 + int c - int '0') (i+1)
            | '$' -> Some n, i+1
            | _ -> None, i

        let position i =
            match fmt.[i] with
            | c when c >= '1' && c <= '9' ->
                let p, i' = digits_position (int c - int '0') (i+1)
                if p = None then None, i else p, i'
            | _ -> None, i

        let posi, i = position i

        let i = flags i 

        let widthArg,(precisionArg,i) = width_and_precision i 

        if i >= len then failwith "bad precision in format specifier";

        let acc = if precisionArg then (Option.map ((+)1) posi, g.int_ty) :: acc else acc 

        let acc = if widthArg then (Option.map ((+)1) posi, g.int_ty) :: acc else acc 

        let checkNoPrecision     c = if info.precision then failwithf "'%c' format does not support precision" c
        let checkNoZeroFlag      c = if info.addZeros then failwithf "'%c' format does not support '0' flag" c
        let checkNoNumericPrefix c = if info.numPrefixIfPos <> None then
                                        failwithf "'%c' does not support prefix '%c' flag" c (Option.get info.numPrefixIfPos)

        let checkOtherFlags c = 
            checkNoPrecision c; 
            checkNoZeroFlag c; 
            checkNoNumericPrefix c

        let ch = fmt.[i]
        match ch with
        | '%' -> go acc (i+1) 

        | ('d' | 'i' | 'o' | 'u' | 'x' | 'X') ->
            if info.precision then failwithf "'%c' format does not support precision" ch;
            go ((posi, flexible_int_format_typar g m) :: acc) (i+1)

        | ('l' | 'L' as  c) ->
            if info.precision then failwithf "'%c' format does not support precision" ch;
            let i = i+1
            if i >= len then failwith "bad format specifier (after l or L): Expected ld,li,lo,lu,lx or lX. These format specifiers support code cross-compiled with OCaml. In F# code you can use %d, %x, %o or %u instead, which are overloaded to work with all basic integer types"
            // Always warn for %l and %Lx
            warning(OCamlCompatibility("The 'l' or 'L' in this format specifier is unnecessary except in code cross-compiled with OCaml. In F# code you can use %d, %x, %o or %u instead, which are overloaded to work with all basic integer types",m))
            match fmt.[i] with
            | ('d' | 'i' | 'o' | 'u' | 'x' | 'X') -> 
                go ((posi, flexible_int_format_typar g m) :: acc)  (i+1)
            | _ -> failwith "bad format specifier (after l or L). Expected ld,li,lo,lu,lx or lX. These format specifiers support code cross-compiled with OCaml. In F# code you can use %d, %x, %o or %u instead, which are overloaded to work with all basic integer types"

        | ('h' | 'H' as  c) ->
            failwith "The 'h' or 'H' in this format specifier is unnecessary. You can use %d, %x, %o or %u instead, which are overloaded to work with all basic integer types"

        | 'M' -> 
            go ((posi, g.decimal_ty) :: acc) (i+1)

        | 'n' ->
            failwith "%n format patterns are deprecated. You can use %d instead, which is overloaded to work with all basic integer types"

        | 'U' ->
            failwith "%U format patterns are deprecated. You can use %u instead, which is overloaded to work with all basic integer types"

        | ('f' | 'F' | 'e' | 'E' | 'g' | 'G') ->  
            go ((posi, flexible_float_format_typar g m) :: acc) (i+1)

        | 'b' ->
            checkOtherFlags ch;
            go ((posi, g.bool_ty)  :: acc) (i+1)

        | 'c' ->
            checkOtherFlags ch;
            go ((posi, g.char_ty)  :: acc) (i+1)

        | 's' ->
            checkOtherFlags ch;
            go ((posi, g.string_ty)  :: acc) (i+1)

        | 'O' ->
            checkOtherFlags ch;
            go ((posi, new_inference_typ ()) :: acc)  (i+1)

        | 'A' ->
            match info.numPrefixIfPos with
            | None     // %A has BindingFlags=Public, %+A has BindingFlags=Public | NonPublic
            | Some '+' -> go ((posi, new_inference_typ ()) :: acc)  (i+1)
            | Some _   -> failwithf "'%c' does not support prefix '%c' flag" ch (Option.get info.numPrefixIfPos)

        | 'a' ->
            checkOtherFlags ch;
            let xty = new_inference_typ () 
            let fty = bty --> (xty --> cty)
            go ((Option.map ((+)1) posi, xty) ::  (posi, fty) :: acc) (i+1)

        | 't' ->
            checkOtherFlags ch;
            go ((posi, bty --> cty) :: acc)  (i+1)

        | c -> failwith ("bad format specifier: '%"^(String.make 1 c)^"'") 
        
    | _ -> go acc (i+1) 
  go [] 0

