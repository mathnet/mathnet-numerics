// (c) Microsoft Corporation. All rights reserved

#light 

module (* internal *) Microsoft.FSharp.Compiler.Ast

open Microsoft.FSharp.Text
open Internal.Utilities
open Internal.Utilities.Text.Lexing
open Internal.Utilities.Text.Parsing
open Internal.Utilities.Compatibility.OCaml.Lexing
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.UnicodeLexing 
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.PrettyNaming

module Ilpars = Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiParser 
module Illex = Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiLexer 

open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Range


/// The prefix of the names used for the fake namespace path added to all dynamic code entries in FSI.EXE
let DynamicModulePrefix = "FSI_"
let public  lib_MF_name                    = "Microsoft.FSharp"
let public lib_MF_path                     = IL.split_namespace lib_MF_name 
let public lib_MFCore_name                 = lib_MF_name ^ ".Core"
let public lib_MFCore_path                 = IL.split_namespace lib_MFCore_name 


//------------------------------------------------------------------------
// XML doc pre-processing
//-----------------------------------------------------------------------

let findFirstIndexWhereTrue (arr: _ array) p = 
    let rec look lo hi = 
        assert ((lo >= 0) && (hi >= 0))
        assert ((lo <= arr.Length) && (hi <= arr.Length))
        if lo = hi then lo
        else
            let i = (lo+hi)/2
            if p arr.[i] then 
                if i = 0 then i 
                else
                    if p arr.[i-1] then 
                        look lo i
                    else 
                        i
            else
                // not true here, look after
                look (i+1) hi
    look 0 arr.Length
  
    
(*
findFirstIndexWhereTrue [| 2 |] (fun i -> i > 2) = 1
findFirstIndexWhereTrue [| 2 |] (fun i -> i > 1) = 0

findFirstIndexWhereTrue [| 2;3 |] (fun i -> i > 1) = 0
findFirstIndexWhereTrue [| 2;3 |] (fun i -> i > 2) = 1
findFirstIndexWhereTrue [| 2;3 |] (fun i -> i > 3) = 2

findFirstIndexWhereTrue [| 1;2;3 |] (fun i -> i > 0) = 0
findFirstIndexWhereTrue [| 1;2;3 |] (fun i -> i > 1) = 1
findFirstIndexWhereTrue [| 1;2;3 |] (fun i -> i > 2) = 2
findFirstIndexWhereTrue [| 1;2;3 |] (fun i -> i > 3) = 3

findFirstIndexWhereTrue [| 1;2;3;4 |] (fun i -> i > 0) = 0
findFirstIndexWhereTrue [| 1;2;3;4 |] (fun i -> i > 1) = 1
findFirstIndexWhereTrue [| 1;2;3;4 |] (fun i -> i > 2) = 2
findFirstIndexWhereTrue [| 1;2;3;4 |] (fun i -> i > 3) = 3
findFirstIndexWhereTrue [| 1;2;3;4 |] (fun i -> i > 4) = 4

findFirstIndexWhereTrue [| 1;2;3;4;5 |] (fun i -> i > 0) = 0
findFirstIndexWhereTrue [| 1;2;3;4;5 |] (fun i -> i > 1) = 1
findFirstIndexWhereTrue [| 1;2;3;4;5 |] (fun i -> i > 2) = 2
findFirstIndexWhereTrue [| 1;2;3;4;5 |] (fun i -> i > 3) = 3
findFirstIndexWhereTrue [| 1;2;3;4;5 |] (fun i -> i > 4) = 4
findFirstIndexWhereTrue [| 1;2;3;4;5 |] (fun i -> i > 5) = 5
*)

type XmlDocCollector() =
    let mutable savedLines = new ResizeArray<(string * pos)>()
    let mutable savedGrabPoints = new ResizeArray<pos>()
    let pos_compare p1 p2 = if pos_geq p1 p2 then 1 else if pos_eq p1 p2 then 0 else -1
    let savedGrabPointsAsArray = 
        lazy (savedGrabPoints.ToArray() |> Array.sortWith pos_compare)

    let savedLinesAsArray = 
        lazy (savedLines.ToArray() |> Array.sortWith (fun (_,p1) (_,p2) -> pos_compare p1 p2))

    let check() = 
        assert (not savedLinesAsArray.IsForced && "can't add more XmlDoc elements to XmlDocCOllector after extracting first XmlDoc from the overall results" <> "")

    member x.AddGrabPoint(pos) = 
        check()
        savedGrabPoints.Add pos 

    member x.AddXmlDocLine(line,pos) = 
        check()
        savedLines.Add(line,pos)

    member x.LinesBefore(grabPointPos) = 
                        
        let lines = savedLinesAsArray.Force()
        let grabPoints = savedGrabPointsAsArray.Force()
        let firstLineIndexAfterGrabPoint = findFirstIndexWhereTrue lines (fun (_,pos) -> pos_geq pos grabPointPos) 
        let grabPointIndex = findFirstIndexWhereTrue grabPoints (fun pos -> pos_geq pos grabPointPos) 
        assert (pos_eq grabPoints.[grabPointIndex] grabPointPos)
        let firstLineIndexAfterPrevGrabPoint = 
            if grabPointIndex = 0 then 
                0 
            else
                let prevGrabPointPos = grabPoints.[grabPointIndex-1]
                findFirstIndexWhereTrue lines (fun (_,pos) -> pos_geq pos prevGrabPointPos) 
        //printfn "#lines = %d, firstLineIndexAfterPrevGrabPoint = %d, firstLineIndexAfterGrabPoint = %d" lines.Length firstLineIndexAfterPrevGrabPoint  firstLineIndexAfterGrabPoint
        lines.[firstLineIndexAfterPrevGrabPoint..firstLineIndexAfterGrabPoint-1] |> Array.map fst

    
type XmlDoc = XmlDoc of string[]

let emptyXmlDoc = XmlDoc[| |]
let MergeXmlDoc (XmlDoc lines) (XmlDoc lines') = XmlDoc (Array.append lines lines')

type PreXmlDoc = 
    | PreXmlMerge of PreXmlDoc * PreXmlDoc
    | PreXmlDoc of pos * XmlDocCollector
    | PreXmlDocEmpty 

    member x.ToXmlDoc() = 
        match x with 
        | PreXmlMerge(a,b) -> MergeXmlDoc (a.ToXmlDoc()) (b.ToXmlDoc())
        | PreXmlDocEmpty -> emptyXmlDoc
        | PreXmlDoc (pos,collector) -> 
            let lines = collector.LinesBefore pos
            if lines.Length = 0 then emptyXmlDoc
            else XmlDoc lines

    static member CreateFromGrabPoint(collector:XmlDocCollector,grabPointPos) = 
        collector.AddGrabPoint grabPointPos
        PreXmlDoc(grabPointPos,collector)

let emptyPreXmlDoc = PreXmlDocEmpty 
let MergePreXmlDoc a b = PreXmlMerge (a,b)

  

let ProcessXmlDoc (XmlDoc lines) = 
    // chop leading spaces (well, this isn't very efficient, is it?) 
    let rec trimSpaces str = if String.hasPrefix str " " then trimSpaces (String.dropPrefix str " ") else str
         
    let rec processLines lines =
        match lines with 
        | [] -> []
        | (lineA::rest) as lines ->
            let lineAT = trimSpaces lineA
            if lineAT = "" then processLines rest
            else if String.hasPrefix lineAT "<" then lines
            else ["<summary>"] @ lines @ ["</summary>"] 

    let lines = processLines (Array.to_list lines)
    if lines.Length = 0 then emptyXmlDoc 
    else XmlDoc (Array.of_list lines)


//------------------------------------------------------------------------
//  AST: main ast definitions
//-----------------------------------------------------------------------


// PERFORMANCE: consider making this a struct.
[<System.Diagnostics.DebuggerDisplay("{idText}")>]
[<Sealed>]
type ident (text,range) = 
     member x.idText = text
     member x.idRange = range
     override x.ToString() = text

type ValueId = ident 
type UnionCaseId = ident 
type RecdFieldId = ident 

type LongIdent = ident list
type RecdFieldPath = LongIdent * RecdFieldId 
type access = | Access of int  (* 0 = public, 1 = assembly, 2 = outer module etc. *)

let accessPublic = Access 0
let accessInternal = Access 1
let accessPrivate = Access System.Int32.MaxValue

type 
    [<StructuralEquality(false); StructuralComparison(false)>]
    SynConst = 
    | Const_unit
    | Const_bool of bool
    | Const_int8 of sbyte
    | Const_uint8 of byte
    | Const_int16 of int16
    | Const_uint16 of uint16
    | Const_int32 of int32
    | Const_uint32 of uint32
    | Const_int64 of int64
    | Const_uint64 of uint64
    | Const_nativeint of int64
    | Const_unativeint of uint64
    | Const_float32 of single
    | Const_float of double
    | Const_char of char
    | Const_decimal of System.Decimal
    | Const_bignum of ( (* value: *) string * (* suffix: *) string)
    | Const_string of string * range 
    | Const_bytearray of byte[] * range 
    | Const_uint16array of uint16[] 
    | Const_measure of SynConst * SynMeasure (* we never iterate, so the const here is not another Const_measure *)

and  SimplePat =
    | SPat_as of  
         ValueId * 
         bool * (* true if a compiler generated name *) 
         bool * (* true if 'this' variable in member *) 
         bool * (* true if an optional parm. *) 
         range
    | SPat_typed of  SimplePat * SynType * range
    | SPat_attrib of  SimplePat * SynAttributes * range

and  SimplePats =
    | SPats of SimplePat list * range
    | SPats_typed of  SimplePats * SynType * range

and  
    [<StructuralEquality(false); StructuralComparison(false)>]
    SynPat =
    | Pat_const of SynConst * range
    | Pat_wild of range
    | Pat_as of  SynPat * ValueId * bool (* true if 'this' variable *)  * access option * range
    | Pat_instance_member of  ValueId * ValueId * access option * range (* adhoc overloaded method/property *)
    | Pat_typed of  SynPat * SynType * range
    | Pat_attrib of  SynPat * SynAttributes * range
    | Pat_disj of  SynPat * SynPat * range
    | Pat_conjs of  SynPat list * range
    | Pat_lid of LongIdent * SynValTyparDecls option (* usually None: temporary used to parse "f<'a> x = x"*) * SynPat list  * access option * range
    | Pat_tuple of  SynPat list * range
    | Pat_paren of  SynPat * range
    | Pat_array_or_list of  bool * SynPat list * range
    | Pat_recd of (RecdFieldPath * SynPat) list * range
    | Pat_range of char * char * range
    | Pat_null of range
    | Pat_opt_var of ident * range
    | Pat_isinst of SynType * range
    | Pat_expr of SynExpr * range

and 
    [<StructuralEquality(false); StructuralComparison(false)>]
    SynType =
    | Type_lid of LongIdent * range
    | Type_app of SynType * SynType list * bool * range // the bool is true if this is a postfix type application e.g. "int list" or "(int,string) dict"
    | Type_proj_then_app of SynType * LongIdent * SynType list * range
    | Type_tuple of (bool*SynType) list * range    // the bool is true if / rather than * follows the type
    | Type_arr of  int * SynType * range
    | Type_lazy of  SynType * range
    | Type_fun of  SynType * SynType * range
    | Type_forall of  SynTyparDecl * SynType * range
    | Type_var of SynTypar * range
    | Type_anon of range
    | Type_with_global_constraints of SynType * SynTypeConstraint list * range
    | Type_anon_constraint of SynType * range
    | Type_quotient of SynType * SynType * range       (* For units of measure e.g. m / s *)
    | Type_power of SynType * int * range      (* For units of measure e.g. m^3 *)
    | Type_dimensionless of range          (* For the dimensionless units i.e. 1 *)

and SeqExprOnly = SeqExprOnly of bool

and ExprAtomicFlag =
    | Atomic = 0
    | NonAtomic = 1
    
and  
  [<StructuralEquality(false); StructuralComparison(false)>]
  SynExpr =
    | Expr_paren of SynExpr * range  (* parenthesized expressions kept in AST to distinguish A.M((x,y)) from A.M(x,y) *)
    | Expr_quote of SynExpr * bool * SynExpr * range 
    | Expr_const of SynConst * range
    | Expr_typed of  SynExpr * SynType * range
    | Expr_tuple of  SynExpr list * range
    | Expr_array_or_list of  bool * SynExpr list * range 
    | Expr_recd of (SynType * SynExpr * range) option * SynExpr option * (RecdFieldPath * SynExpr) list * range
    | Expr_new of bool * SynType * SynExpr * range (* bool true if known to be 'family' ('proected') scope *)
    | Expr_impl of SynType * (SynExpr * ident option) option * SynBinding list * SynInterfaceImpl list * range
    | Expr_while of SequencePointInfoForWhileLoop * SynExpr * SynExpr * range
    | Expr_for of SequencePointInfoForForLoop * ident * SynExpr * bool * SynExpr * SynExpr * range
    | Expr_foreach of SequencePointInfoForForLoop * SeqExprOnly * SynPat * SynExpr * SynExpr * range
    | Expr_array_or_list_of_seq of bool * SynExpr * range
    | Expr_comprehension of bool * bool ref * SynExpr * range
    /// first bool indicates if lambda originates from a method. Patterns here are always "simple" 
    /// second bool indicates if this is a "later" part of an iterated sequence of lambdas
    | Expr_lambda of  bool * bool * SimplePats * SynExpr * range 
    | Expr_match of  SequencePointInfoForBinding * SynExpr * SynMatchClause list * bool * range (* bool indicates if this is an exception match in a computation expression which throws unmatched exceptions *)
    | Expr_do of  SynExpr * range
    | Expr_assert of SynExpr * range
    | Expr_app of ExprAtomicFlag * SynExpr * SynExpr * range
    | Expr_tyapp of SynExpr * SynType list * range
    | Expr_let of bool * bool * SynBinding list * SynExpr * range
    | Expr_try_catch of SynExpr * range * SynMatchClause list * range * range * SequencePointInfoForTry * SequencePointInfoForWith
    | Expr_try_finally of SynExpr * SynExpr * range * SequencePointInfoForTry * SequencePointInfoForFinally
    | Expr_seq of SequencePointInfoForSeq * bool * SynExpr * SynExpr * range (* false for first flag indicates "do a then b then return a" *)
    | Expr_arb  of range  // for error recovery
    | Expr_throwaway  of SynExpr * range  // for error recovery
    | Expr_cond of SynExpr * SynExpr * SynExpr option * SequencePointInfoForBinding * range * range
    | Expr_lid_get of bool * LongIdent * range  (* bool true if preceded by a '?' for an optional named parameter *) 
    | Expr_id_get of ident (* = Expr_lid_get(false,[id],id.idRange) *)
    | Expr_lid_set of LongIdent * SynExpr * range
    | Expr_lid_indexed_set of LongIdent * SynExpr * SynExpr * range
    | Expr_lvalue_get of SynExpr * LongIdent * range
    | Expr_lvalue_set of SynExpr * LongIdent * SynExpr * range
    | Expr_lvalue_indexed_set of SynExpr * LongIdent * SynExpr * SynExpr * range
    | Expr_constr_field_get of SynExpr * LongIdent * int * range
    | Expr_constr_field_set of SynExpr * LongIdent * int * SynExpr * range
    | Expr_asm of ILInstr array *  SynType list * SynExpr list * SynType list * range (* Embedded IL assembly code *)
    | Expr_static_optimization of StaticOptimizationConstraint list * SynExpr * SynExpr * range
    | Expr_isinst of  SynExpr * SynType * range
    | Expr_upcast of  SynExpr * SynType * range
    | Expr_addrof of  bool * SynExpr * range * range
    | Expr_downcast of  SynExpr * SynType * range
    | Expr_inferred_upcast of  SynExpr * range
    | Expr_inferred_downcast of  SynExpr * range
    | Expr_null of range
    | Expr_lazy of SynExpr * range
    | Expr_ifnull of SynExpr * SynExpr * range
    | Expr_trait_call of SynTypar list * SynClassMemberSpfn * SynExpr * range
    | Expr_typeof of SynType * range
    | Expr_lbrack_get of SynExpr * SynExpr list * range * range
    | Expr_lbrack_set of SynExpr * SynExpr list * SynExpr * range * range

    | Comp_zero of range 
    | Comp_yield   of (bool * bool) * SynExpr * range
    | Comp_yieldm  of (bool * bool) * SynExpr * range
    | Comp_bind    of SequencePointInfoForBinding * bool * SynPat * SynExpr * SynExpr * range
    | Comp_do_bind      of SynExpr * range
  

and SynInterfaceImpl = 
    | InterfaceImpl of SynType * SynBinding list * range

and SynMatchClause = 
    | Clause of SynPat * SynExpr option *  SynExpr * range * SequencePointInfoForTarget

and SynAttributes = SynAttribute list

and SynAttribute = 
    (* ident option are target specifiers, e.g. "assembly","module",etc. *)
    | Attr of LongIdent * SynExpr * ident option * range 

and ValSynData = 
    | ValSynData of MemberFlags option * ValSynInfo * ident option

and SynBindingKind = 
    | StandaloneExpression
    | NormalBinding
    | DoBinding
  
and SynBinding = 
    | Binding of 
        access option *
        SynBindingKind *  
        bool (* mustinline: *) *  
        bool (* mutable: *) *  
        SynAttributes * 
        PreXmlDoc *
        ValSynData * 
        SynPat * 
        BindingRhs *
        range *
        SequencePointInfoForBinding

and SequencePointInfoForTarget = 
    | SequencePointAtTarget
    | SuppressSequencePointAtTarget

and SequencePointInfoForSeq = 
    | SequencePointsAtSeq
    // This means "suppress a in 'a;b'" and "suppress b in 'a before b'"
    | SuppressSequencePointOnExprOfSequential
    // This means "suppress b in 'a;b'" and "suppress a in 'a before b'"
    | SuppressSequencePointOnStmtOfSequential

and SequencePointInfoForTry = 
    | SequencePointAtTry of range
    | NoSequencePointAtTry

and SequencePointInfoForWith = 
    | SequencePointAtWith of range
    | NoSequencePointAtWith

and SequencePointInfoForFinally = 
    | SequencePointAtFinally of range
    | NoSequencePointAtFinally

and SequencePointInfoForForLoop = 
    | SequencePointAtForLoop of range
    | NoSequencePointAtForLoop
    
and SequencePointInfoForWhileLoop = 
    | SequencePointAtWhileLoop of range
    | NoSequencePointAtWhileLoop
    
and SequencePointInfoForBinding = 
    | SequencePointAtBinding of range
    // Indicates the ommission of a sequence point for a binding for a 'do expr' 
    | NoSequencePointAtDoBinding
    // Indicates the ommission of a sequence point for a binding for a 'let e = expr' where 'expr' has immediate control flow
    | NoSequencePointAtLetBinding
    // Indicates the ommission of a sequence point for a compiler generated binding
    // where wev'e done a local expansion of some construct into something that involves
    // a 'let'. e.g. we've inlined a function and bound its arguments using 'let'
    // The let bindings are 'sticky' in that the inversion of the inlining would involve
    // replacing the entire expression with the original and not just the let bindings alone.
    | NoSequencePointAtStickyBinding
    // Given 'let v = e1 in e2', where this is a compiler generated binding, 
    // we are sometimes forced to generate a sequence point for the expression anyway based on its
    // overall range. If the let binding is given the flag below then it is basically asserting that
    // the binding has no interesting side effects and can be totally ignored and the range
    // of the inner expression is used instead
    | NoSequencePointAtInvisibleBinding
    
    // Don't drop sequence points when combining sequence points
    member x.Combine(y:SequencePointInfoForBinding) = 
        match x,y with 
        | SequencePointAtBinding _ as g, _  -> g
        | _, (SequencePointAtBinding _ as g)  -> g
        | _ -> x

// BindingRhs records the r.h.s. of a binding after some munging in the parser. 
// NOTE: This is a bit of a mess.  In the early implementation of F# we decided 
// to have the parser convert "let f x = e" into 
// "let f = fun x -> e".  This is called "pushing" a pattern across to the right hand side. Complex 
// patterns (e.g. non-tuple patterns) result in a computation on the right. 
// However, this approach really isn't that great - especially since 
// the language is now considerably more complex, e.g. we use 
// type information from the first (but not the second) form in 
// type inference for recursive bindings, and the first form 
// may specify .NET attributes for arguments. There are still many 
// relics of this approach around, e.g. the expression in BindingRhs 
// below is of the second form. However, to extract relevant information 
// we keep a record of the pats and optional explicit return type already pushed 
// into expression so we can use any user-given type information from these 
and BindingRhs = 
    | BindingRhs of 
        SimplePats list * 
        (SynType * range * SynAttributes) option * 
        SynExpr 

and MemberFlags =
  { OverloadQualifier: string option; 
    MemberIsInstance: bool;
    MemberIsVirtual: bool;
    MemberIsDispatchSlot: bool;
    MemberIsOverrideOrExplicitImpl: bool;
    MemberIsFinal: bool;
    MemberKind: MemberKind }

/// Note the member kind is actually computed partially by a syntax tree transformation "norm_pat" in tc.ml 
and MemberKind = 
    | MemberKindClassConstructor
    | MemberKindConstructor
    | MemberKindMember 
    | MemberKindPropertyGet 
    | MemberKindPropertySet    
    | MemberKindPropertyGetSet    

and SynSignature =
    | Sign_named of LongIdent 
    | Sign_explicit of SynModuleSpecDecls


and 
    [<StructuralEquality(false); StructuralComparison(false)>]
    SynModuleImplDecl =
    | Def_module_abbrev of ident * LongIdent * range
    | Def_module of SynComponentInfo * SynModuleImplDecls * SynSignature option * range
    | Def_let of bool * SynBinding list * range (* first flag recursion, second flag must-inline *)
    | Def_expr of SequencePointInfoForBinding * SynExpr * range 
    | Def_tycons of SynTyconDefn list * range
    | Def_partial_tycon of SynComponentInfo * SynClassMemberDefns * range
    | Def_exn of SynExceptionDefn * range
    | Def_open of LongIdent * range
    | Def_attributes of SynAttributes * range
    | Def_hash of hashDirective * range

and SynExceptionCore = 
    | ExconCore of SynAttributes * SynUnionCaseDecl * LongIdent option * PreXmlDoc * access option * range

and SynExceptionDefn = 
    | ExconDefn of SynExceptionCore * SynClassMemberDefns * range

and SynTyconKind = 
    | TyconUnspecified 
    | TyconClass 
    | TyconInterface 
    | TyconStruct 
    | TyconRecord
    | TyconUnion
    | TyconAbbrev
    | TyconHiddenRepr
    | TyconILAssemblyCode
    /// REVIEW: this should be a different representation, rather than a SynTyconKind
    | TyconDelegate of SynType * ValSynInfo

and SynTyconDefnRepr =
    | TyconDefnRepr_class  of SynTyconKind * SynClassMemberDefns * range
    | TyconDefnRepr_simple of SynTyconSpfnOrDefnSimpleRepr * range

and SynTyconDefn =
    | TyconDefn of SynComponentInfo * SynTyconDefnRepr * SynClassMemberDefns * range

and SynClassMemberDefns = SynClassMemberDefn list

and 
    [<StructuralEquality(false); StructuralComparison(false)>]
    SynClassMemberDefn = 
    | ClassMemberDefn_open of LongIdent * range
    | ClassMemberDefn_member_binding of SynBinding * range                          
    /// implicit ctor args as a defn line, 'as' specification 
    | ClassMemberDefn_implicit_ctor of access option * SynAttributes * SimplePat list * ident option * range    
    /// inherit <typ>(args...) as base 
    | ClassMemberDefn_implicit_inherit of SynType * SynExpr * ident option * range   
    /// localDefns 
    | ClassMemberDefn_let_bindings of SynBinding list * (* static: *) bool * (* recursive: *) bool * range                    
    | ClassMemberDefn_slotsig of SynValSpfn * MemberFlags * range 
    | ClassMemberDefn_interface of SynType * SynClassMemberDefns option  * range
    | ClassMemberDefn_inherit of SynType  * ident option * range
    | ClassMemberDefn_field of SynFieldDecl  * range
    | ClassMemberDefn_tycon of SynTyconDefn * access option * range
      
and SynExnSpfn = 
    | ExconSpfn of SynExceptionCore * SynClassSpfn * range

and SynClassSpfn = SynClassMemberSpfn list

and 
    [<StructuralEquality(false); StructuralComparison(false)>]
    SynClassMemberSpfn = 
    | ClassMemberSpfn_binding of SynValSpfn  * MemberFlags * range 
    | ClassMemberSpfn_interface of SynType  * range
    | ClassMemberSpfn_inherit of SynType * range
    | ClassMemberSpfn_field of SynFieldDecl  * range
    | ClassMemberSpfn_tycon  of SynTyconSpfn * range

and SynValSpfn = 
    | ValSpfn of 
        SynAttributes * 
        ValueId * 
        SynValTyparDecls * 
        SynType * 
        ValSynInfo * 
        bool * 
        bool *  (* mutable? *) 
        PreXmlDoc * 
        access option *
        SynExpr option *
        range 

and ValSynInfo = 
    | ValSynInfo of (*args:*) ArgSynInfo list list * (*return:*) ArgSynInfo 
    member x.ArgInfos = (let (ValSynInfo(args,_)) = x in args)

and ArgSynInfo = 
    | ArgSynInfo of SynAttributes * (*optional:*) bool *  ident option

and SynValTyparDecls = 
    | SynValTyparDecls of SynTyparDecl list * bool * SynTypeConstraint list

and SynTyconSpfnRepr =
    | TyconSpfnRepr_class of SynTyconKind * SynClassSpfn * range
    | TyconSpfnRepr_simple of SynTyconSpfnOrDefnSimpleRepr * range 

and SynComponentInfo = 
    | ComponentInfo of SynAttributes * ComponentKind * SynTyparDecl list * SynTypeConstraint list * LongIdent * PreXmlDoc * (* preferPostfix: *) bool * access option * range

and ComponentKind = 
    | TMK_Namespace 
    | TMK_Module 
    | TMK_Tycon 

and SynTyconSpfn =
    | TyconSpfn of SynComponentInfo * SynTyconSpfnRepr * SynClassSpfn * range

and 
    [<StructuralEquality(false); StructuralComparison(false)>]
    SynModuleSpecDecl =
    | Spec_module_abbrev of ident * LongIdent * range
    | Spec_module   of SynComponentInfo * SynModuleSpecDecls * range
    | Spec_val      of SynValSpfn * range
    | Spec_tycon    of SynTyconSpfn list * range
    | Spec_exn      of SynExnSpfn * range
    | Spec_open     of LongIdent * range
    | Spec_hash     of hashDirective * range

and 
    [<StructuralEquality(false); StructuralComparison(false)>]
    SynTyconSpfnOrDefnSimpleRepr =
    | TyconCore_union of access option * SynUnionCaseDecls * range
    | TyconCore_enum of SynEnumCaseDecls * range
    | TyconCore_recd of access option * SynFieldDecls * range
    | TyconCore_general of SynTyconKind * (SynType * range * ident option) list * (SynValSpfn * MemberFlags) list * SynFieldDecls * bool * bool * range 
    | TyconCore_asm of ILType * range
    | TyconCore_abbrev of SynType * range
    | TyconCore_no_repr of range

and SynUnionCaseDecls = SynUnionCaseDecl list

and SynEnumCaseDecls = SynEnumCaseDecl list

and SynFieldDecls = SynFieldDecl list

and SynFieldDecl = 
    | Field of SynAttributes * (* static: *) bool * RecdFieldId option * SynType * bool * PreXmlDoc * access option * range

and SynUnionCaseDecl = 
    | UnionCase of SynAttributes * UnionCaseId * SynUnionConstrTypeDecl * PreXmlDoc * access option * range

and SynEnumCaseDecl =
    | EnumCase of SynAttributes * UnionCaseId * SynConst * PreXmlDoc * range

and SynUnionConstrTypeDecl = 
    /// Normal ML-style declaration 
    | UnionCaseFields of SynFieldDecl list      
    /// Full type spec given by 'UnionCase : ty1 * tyN -> rty' 
    | UnionCaseFullType of (SynType * ValSynInfo) 

and SynMeasure = 
    | Measure_Con of LongIdent * range
    | Measure_Prod of SynMeasure * SynMeasure * range
    | Measure_Seq of SynMeasure list * range
    | Measure_Quot of SynMeasure * SynMeasure * range
    | Measure_Power of SynMeasure * int * range
    | Measure_One 
    | Measure_Anon of range
    | Measure_Var of SynTypar * range

and SynTypar = 
    | Typar of ident * TyparStaticReq * (* compgen: *) bool 

and SynTyparDecl = 
    | TyparDecl of SynAttributes * SynTypar

and TyparStaticReq = 
    | NoStaticReq 
    | HeadTypeStaticReq 

and StaticOptimizationConstraint =
    | WhenTyparTyconEqualsTycon of SynTypar *  SynType * range
    | WhenInlined of range

and 
    [<StructuralEquality(false); StructuralComparison(false)>]
    SynTypeConstraint =
    | WhereTyparIsValueType of SynTypar * range
    (* | WhereTyparSupportsDefaultConstructor of SynTypar * range *)
    | WhereTyparIsReferenceType of SynTypar * range
    | WhereTyparSupportsNull of SynTypar * range
    | WhereTyparDefaultsToType of SynTypar * SynType * range
    | WhereTyparEqualsType of SynTypar *  SynType * range
    | WhereTyparSubtypeOfType of SynTypar *  SynType * range
    | WhereTyparSupportsMember of SynTypar list * SynClassMemberSpfn * range
    | WhereTyparIsEnum of SynTypar * SynType list * range
    | WhereTyparIsDelegate of SynTypar * SynType list * range

and SynModuleSpecDecls = SynModuleSpecDecl list
and SynModuleImplDecls = SynModuleImplDecl list

/// QualifiedNameOfFile acts to fully-qualify module specifications and implementations, 
/// most importantly the ones that simply contribute fragments to a namespace (i.e. the AnonNamespaceFragmentSpec case) 
/// There may be multiple such fragments in a single assembly, a major difference between traditional 
/// ML and F#.  There may thus also be multiple matching pairs of these in an assembly, all contributing types to the same 
/// namespace. These are matched up by the filename-rule. 
and QualifiedNameOfFile = 
    | QualifiedNameOfFile of ident 
    member x.Text = (let (QualifiedNameOfFile(t)) = x in t.idText)
    member x.Id = (let (QualifiedNameOfFile(t)) = x in t)
    member x.Range = (let (QualifiedNameOfFile(t)) = x in t.idRange)

/// ModuleOrNamespaceImpl(lid,isModule,decls,xmlDoc,attribs,access,m)
and moduleImpl = 
    | ModuleOrNamespaceImpl of LongIdent * (*isModule:*) bool * SynModuleImplDecls * PreXmlDoc * SynAttributes * access option * range 

and moduleSpec = 
    | ModuleOrNamespaceSpec of LongIdent * (*isModule:*) bool * SynModuleSpecDecls * PreXmlDoc * SynAttributes * access option * range 

and ParsedSigFileFragment = 
    | AnonTopModuleSpec of SynModuleSpecDecls * range
    | NamedTopModuleSpec of moduleSpec
    | AnonNamespaceFragmentSpec of LongIdent * bool * SynModuleSpecDecls * PreXmlDoc * SynAttributes * range

and ParsedImplFileFragment = 
    | AnonTopModuleImpl of SynModuleImplDecls * range
    | NamedTopModuleImpl of moduleImpl
    | AnonNamespaceFragmentImpl of LongIdent * bool * SynModuleImplDecls * PreXmlDoc * SynAttributes * range

and interaction =
    | IDefns of SynModuleImplDecl list * range
    | IHash  of hashDirective * range

and hashDirective = 
    | HashDirective of string * string list * range

and ParsedImplFile = 
    | ParsedImplFile of hashDirective list * ParsedImplFileFragment list

and ParsedSigFile = 
    | ParsedSigFile of hashDirective list * ParsedSigFileFragment list

//----------------------------------------------------------------------
// AST and parsing utilities.
//----------------------------------------------------------------------

type path = string list 
let ident (s,r) = new ident(s,r)
let text_of_id (id:ident) = id.idText
let path_of_lid lid = List.map text_of_id lid
let arr_path_of_lid lid = Array.of_list (List.map text_of_id lid)
let text_of_path path = String.concat "." path
let text_of_arr_path path = 
    String.concat "." (List.of_array path)
let text_of_lid lid = text_of_path (path_of_lid lid)

let range_of_lid (lid: ident list) = 
    match lid with 
    | [] -> failwith "range_of_lid"
    | [id] -> id.idRange
    | h::t -> union_ranges h.idRange (List.last t).idRange 


type ScopedPragma = 
   | WarningOff of range * int
   // Note: this type may be extended in the future with optimization on/off switches etc.

// These are the results of parsing + folding in the implicit file name
/// ImplFile(modname,isScript,qualName,hashDirectives,modules,canContainEntryPoint)
type implFile = ImplFile of string * (*isScript: *) bool * QualifiedNameOfFile * ScopedPragma list * hashDirective list * moduleImpl list * bool
type sigFile = SigFile of string * QualifiedNameOfFile * ScopedPragma list * hashDirective list * moduleSpec list

type input = 
  | ImplFileInput of implFile
  | SigFileInput of sigFile

let range_of_input inp = 
  match inp with
  | ImplFileInput (ImplFile(_,_,_,_,_,(ModuleOrNamespaceImpl(_,_,_,_,_,_,m) :: _),_))
  | SigFileInput (SigFile(_,_,_,_,(ModuleOrNamespaceSpec(_,_,_,_,_,_,m) :: _))) -> m
  | ImplFileInput (ImplFile(filename,_,_,_,_,[],_))
  | SigFileInput (SigFile(filename,_,_,_,[])) ->
#if DEBUG      
      assert("" = "compiler expects ImplFileInput and SigFileInput to have at least one fragment, 4488")
#endif    
      rangeN filename 0 (* There are no implementations, e.g. due to errors, so return a default range for the file *)


//----------------------------------------------------------------------
// Construct syntactic AST nodes
//-----------------------------------------------------------------------

let mksyn_id m s = ident(s,m)
let path_to_lid m p = List.map (mksyn_id m) p
let text_to_id0 n = mksyn_id range0 n

// REVIEW: get rid of this name generator, which is used for the type inference 
// variables implicit in the #C syntax 
let mksyn_new_uniq = let i = ref 0 in fun () -> incr i; !i
let mksyn_item m n = Expr_id_get(mksyn_id m n)

// REVIEW: get rid of this state 
let new_arg_uniq_ref = ref 0 
let mksyn_new_arg_uniq () = incr new_arg_uniq_ref; !new_arg_uniq_ref
let mksyn_spat_var isOpt id = SPat_as (id,false,false,isOpt,id.idRange)

let range_of_synpat p = 
  match p with 
  | Pat_const(_,m) | Pat_wild m | Pat_as (_,_,_,_,m) | Pat_disj (_,_,m) | Pat_conjs (_,m) 
  | Pat_lid (_,_,_,_,m) | Pat_array_or_list(_,_,m) | Pat_tuple (_,m) |Pat_typed(_,_,m) |Pat_attrib(_,_,m) 
  | Pat_recd (_,m) | Pat_range (_,_,m) | Pat_null m | Pat_isinst (_,m) | Pat_expr (_,m)
  | Pat_instance_member(_,_,_,m) | Pat_opt_var(_,m) | Pat_paren(_,m) -> m 

let range_of_syntype ty = 
  match ty with 
  | Type_lid(_,m) | Type_app(_,_,_,m) | Type_proj_then_app(_,_,_,m) | Type_tuple(_,m) | Type_lazy(_,m) | Type_arr(_,_,m) | Type_fun(_,_,m)
  | Type_forall(_,_,m) | Type_var(_,m) | Type_anon m | Type_with_global_constraints(_,_,m)
  | Type_anon_constraint(_,m) | Type_quotient(_,_,m) | Type_power(_,_,m) | Type_dimensionless m -> m

let range_of_synconst c dflt = 
  match c with 
  | Const_string (_,m0) | Const_bytearray (_,m0) -> m0 
  | _ -> dflt
  
let range_of_synexpr = function
    | Expr_paren(_,m) 
    | Expr_quote(_,_,_,m) 
    | Expr_const(_,m) 
    | Expr_typed (_,_,m)
    | Expr_tuple (_,m)
    | Expr_array_or_list (_,_,m)
    | Expr_recd (_,_,_,m)
    | Expr_new (_,_,_,m)
    | Expr_impl (_,_,_,_,m)
    | Expr_while (_,_,_,m)
    | Expr_for (_,_,_,_,_,_,m)
    | Expr_foreach (_,_,_,_,_,m)
    | Expr_comprehension (_,_,_,m)
    | Expr_array_or_list_of_seq (_,_,m)
    | Expr_lambda (_,_,_,_,m)
    | Expr_match (_,_,_,_,m)
    | Expr_do (_,m)
    | Expr_assert (_,m)
    | Expr_app (_,_,_,m)
    | Expr_tyapp (_,_,m)
    | Expr_let (_,_,_,_,m)
    | Expr_try_catch (_,_,_,_,m,_,_)
    | Expr_try_finally (_,_,m,_,_)
    | Expr_seq (_,_,_,_,m)
    | Expr_arb m
    | Expr_throwaway (_,m) 
    | Expr_cond (_,_,_,_,_,m)
    | Expr_lid_get (_,_,m)
    | Expr_lid_set (_,_,m)
    | Expr_lid_indexed_set (_,_,_,m)
    | Expr_lbrack_get (_,_,_,m)
    | Expr_lbrack_set (_,_,_,_,m)
    | Expr_lvalue_get (_,_,m)
    | Expr_lvalue_set (_,_,_,m)
    | Expr_lvalue_indexed_set (_,_,_,_,m)
    | Expr_constr_field_get (_,_,_,m)
    | Expr_constr_field_set (_,_,_,_,m)
    | Expr_asm (_,_,_,_,m)
    | Expr_static_optimization (_,_,_,m)
    | Expr_isinst (_,_,m)
    | Expr_upcast (_,_,m)
    | Expr_addrof (_,_,_,m)
    | Expr_downcast (_,_,m)
    | Expr_inferred_upcast (_,m)
    | Expr_inferred_downcast (_,m)
    | Expr_null m
    | Expr_lazy (_, m)
    | Expr_trait_call(_,_,_,m)
    | Expr_typeof(_,m)
    | Comp_zero (m)
    | Comp_yield (_,_,m)
    | Comp_yieldm (_,_,m)
    | Comp_bind  (_,_,_,_,_,m)
    | Comp_do_bind  (_,m)
    | Expr_ifnull (_,_,m) -> m
    | Expr_id_get id -> id.idRange

let range_of_syndecl d = 
    match d with 
    | Def_module_abbrev(_,_,m) 
    | Def_module(_,_,_,m)
    | Def_let(_,_,m) 
    | Def_expr(_,_,m) 
    | Def_tycons(_,m)
    | Def_partial_tycon(_,_,m) 
    | Def_exn(_,m)
    | Def_open (_,m)
    | Def_hash (_,m)
    | Def_attributes(_,m) -> m

let range_of_synspec d = 
    match d with 
    | Spec_module_abbrev (_,_,m)
    | Spec_module   (_,_,m)
    | Spec_val      (_,m)
    | Spec_tycon    (_,m)
    | Spec_exn      (_,m)
    | Spec_open     (_,m)
    | Spec_hash     (_,m) -> m

let range_of_classmember d =
    match d with
    | ClassMemberDefn_member_binding(_, m)
    | ClassMemberDefn_interface(_, _, m)
    | ClassMemberDefn_open(_, m)
    | ClassMemberDefn_let_bindings(_,_,_,m) 
    | ClassMemberDefn_implicit_ctor(_,_,_,_,m)
    | ClassMemberDefn_implicit_inherit(_,_,_,m) 
    | ClassMemberDefn_slotsig(_,_,m)
    | ClassMemberDefn_inherit(_,_,m)
    | ClassMemberDefn_field(_,m)
    | ClassMemberDefn_tycon(_,_,m) -> m
  
  
let rec IsControlFlowExpression e = 
    match e with 
    | Expr_impl _ 
    | Expr_lambda _ 
    | Expr_let _ 
    | Expr_seq _ 
    | Expr_cond _ 
    | Comp_bind _
    | Expr_match _  
    | Expr_try_catch _ 
    | Expr_try_finally _ 
    | Expr_for _ 
    | Expr_foreach _ 
    | Expr_while _ -> true
    | Expr_typed(e,_,_) -> IsControlFlowExpression e
    | _ -> false

let anon_field_of_typ ty = Field([],false,None,ty,false,emptyPreXmlDoc,None,range_of_syntype ty)

let mksyn_pat_var vis (id:ident) = Pat_as (Pat_wild id.idRange,id,false,vis,id.idRange)
let mksyn_this_pat_var (id:ident) = Pat_as (Pat_wild id.idRange,id,true,None,id.idRange)
let mksyn_pat_maybe_var lid vis m =  Pat_lid (lid,None,[],vis,m) 

let generatedArgNamePrefix = "_arg"

let new_arg_name() = (generatedArgNamePrefix^string (mksyn_new_arg_uniq())) 

let mksyn_new_arg_var m  =
    let nm = new_arg_name()
    let id = mksyn_id m nm
    mksyn_pat_var None id,mksyn_item m nm

/// Push non-simple parts of a patten match over onto the r.h.s. of a lambda.
/// Return a simple pattern and a function to build a match on the r.h.s. if the pattern is complex
let rec SimplePatOfPat p =
    match p with 
    | Pat_typed(p',ty,m) -> 
        let p2,laterf = SimplePatOfPat p'
        SPat_typed(p2,ty,m), 
        laterf
    | Pat_attrib(p',attribs,m) -> 
        let p2,laterf = SimplePatOfPat p'
        SPat_attrib(p2,attribs,m), 
        laterf
    | Pat_as (Pat_wild _, v,thisv,_,m) -> 
        SPat_as (v,false,thisv,false,m), 
        None
    | Pat_opt_var (v,m) -> 
        SPat_as (v,false,false,true,m), 
        None
    | Pat_paren (p,m) -> SimplePatOfPat p 
    | _ -> 
        let m = range_of_synpat p
        (* 'nm' may be a real variable. Maintain its name. *)
        let compgen,nm = (match p with Pat_lid([id],None,[],None,_) -> false,id.idText | _ -> true,new_arg_name())
        let id = mksyn_id m nm
        let item = mksyn_item m nm
        SPat_as (id,compgen,false,false,id.idRange),
        Some (fun e -> Expr_match(NoSequencePointAtInvisibleBinding, item,[Clause(p,None,e,m,SuppressSequencePointAtTarget)],false,m)) 

let appFunOpt funOpt x = match funOpt with None -> x | Some f -> f x
let composeFunOpt funOpt1 funOpt2 = match funOpt2 with None -> funOpt1 | Some f -> Some (fun x -> appFunOpt funOpt1 (f x))
let rec SimplePatsOfPat p =
      match p with 
      | Pat_typed(p',ty,m) -> 
          let p2,laterf = SimplePatsOfPat p'
          SPats_typed(p2,ty,m), 
          laterf
  //    | Pat_paren (p,m) -> SimplePatsOfPat p 
      | Pat_tuple (ps,m) 
      | Pat_paren(Pat_tuple (ps,m),_) -> 
          let ps2,laterf = 
            List.foldBack 
              (fun (p',rhsf) (ps',rhsf') -> 
                p'::ps', 
                (composeFunOpt rhsf rhsf'))
              (List.map SimplePatOfPat ps) 
              ([], None)
          SPats (ps2,m),
          laterf
      | Pat_paren(Pat_const (Const_unit,m),_) 
      | Pat_const (Const_unit,m) -> 
          SPats ([],m),
          None
      | _ -> 
          let m = range_of_synpat p
          let sp,laterf = SimplePatOfPat p
          SPats ([sp],m),laterf

let PushPatternToExpr isMember pat rhs =
    let nowpats,laterf = SimplePatsOfPat pat
    nowpats, Expr_lambda (isMember,false,nowpats, appFunOpt laterf rhs,range_of_synexpr rhs)

let IsSimplePattern pat =
    let nowpats,laterf = SimplePatsOfPat pat
    isNone laterf
  
/// "fun (UnionCase x) (UnionCase y) -> body" 
///       ==> 
///   "fun tmp1 tmp2 -> 
///        let (UnionCase x) = tmp1 in 
///        let (UnionCase y) = tmp2 in 
///        body" 
let PushCurriedPatternsToExpr wholem isMember pats rhs =
    // Two phases
    // First phase: Fold back, from right to left, pushing patterns into r.h.s. expr
    let spatsl,rhs = 
        (pats, ([],rhs)) 
           ||> List.foldBack (fun arg (spatsl,body) -> 
              let spats,bodyf = SimplePatsOfPat arg
              // accumulate the body. This builds "let (UnionCase y) = tmp2 in body"
              let body = appFunOpt bodyf body
              // accumulate the patterns
              let spatsl = spats::spatsl
              (spatsl,body))
    // Second phase: build lambdas. Mark subsequent ones with "true" indicating they are part of an iterated sequence of lambdas
    let expr = 
        match spatsl with
        | [] -> rhs
        | h::t -> 
            let expr = List.foldBack (fun spats e -> Expr_lambda (isMember,true,spats, e,wholem)) t rhs
            let expr = Expr_lambda (isMember,false,h, expr,wholem)
            expr
    spatsl,expr

let new_unit_uniq_ref = ref 0
let new_unit_uniq () = incr new_unit_uniq_ref; !new_unit_uniq_ref


/// Helper for parsing the inline IL fragments. 
let ParseAssemblyCodeInstructions s m = 
    try Ilpars.top_instrs Illex.token (UnicodeLexing.StringAsLexbuf s)
    with RecoverableParseError -> 
      errorR(Error("error while parsing embedded IL",m)); [| |]

/// Helper for parsing the inline IL fragments. 
let ParseAssemblyCodeType s m = 
    try Ilpars.top_typ Illex.token (UnicodeLexing.StringAsLexbuf s)
    with RecoverableParseError -> 
      errorR(Error("error while parsing embedded IL type",m)); IL.ecmaILGlobals.IL.typ_Object

//------------------------------------------------------------------------
// AST constructors
//------------------------------------------------------------------------

let lparen_set_opname  = (CompileOpName lparen_set) 
let lparen_get_opname  = (CompileOpName lparen_get) 
let qmark_opname = (CompileOpName qmark)
let mksyn_lid_get m path n = Expr_lid_get(false,path_to_lid m path @ [mksyn_id m n],m)
let mksyn_mod_item m modul n = mksyn_lid_get m [modul] n
let mk_oper opm oper = mksyn_item opm (CompileOpName oper)

// 'false' in Expr_app means that operators are never high-precedence applications
let mksyn_infix opm m l oper r = Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, mk_oper opm oper,l,m), r,m)
let mksyn_bifix m oper l r = Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, mk_oper m oper,l,m), r,m)
let mksyn_trifix m oper  x1 x2 x3 = Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, mk_oper m oper,x1,m), x2,m), x3,m)
let mksyn_quadfix m oper  x1 x2 x3 x4 = Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, mk_oper m oper,x1,m), x2,m), x3,m),x4,m)
let mksyn_quinfix m oper  x1 x2 x3 x4 x5 = Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, Expr_app (ExprAtomicFlag.NonAtomic, mk_oper m oper,x1,m), x2,m), x3,m),x4,m),x5,m)
let mksyn_prefix opm m oper x = Expr_app (ExprAtomicFlag.NonAtomic, mk_oper opm oper, x,m)
let mksyn_constr m n = [mksyn_id m (CompileOpName n)]

let mksyn_dot_lparen_set  m a b c = mksyn_trifix m lparen_set a b c
let mksyn_dot_lbrack_get  m mDot a b   = Expr_lbrack_get(a,[b],mDot,m)
let mksyn_qmark_set m a b c = mksyn_trifix m qmark_set a b c

let mksyn_dot_lbrack_slice_get  m mDot arr (x,y) = 
    Expr_lbrack_get(arr,[x;y],mDot,m)

let mksyn_dot_lbrack_slice2_get  m mDot arr (x1,y1) (x2,y2) = 
    Expr_lbrack_get(arr,[x1;y1;x2;y2],mDot,m)

let mksyn_dot_lbrack_slice3_get  m mDot arr (x1,y1) (x2,y2) (x3,y3) = 
    Expr_lbrack_get(arr,[x1;y1;x2;y2;x3;y3],mDot,m)

let mksyn_dot_lbrack_slice4_get  m mDot arr (x1,y1) (x2,y2) (x3,y3) (x4,y4) = 
    Expr_lbrack_get(arr,[x1;y1;x2;y2;x3;y3;x4;y4],mDot,m)

let mksyn_dot_lparen_get  m a b   = 
  match b with
  | Expr_tuple ([_;_],_)   -> error(Deprecated("This indexer notation has been removed from the F# language",m))
  | Expr_tuple ([_;_;_],_) -> error(Deprecated("This indexer notation has been removed from the F# language",m))
  | _ -> mksyn_infix m m a lparen_get b
let mksyn_unit m = Expr_const(Const_unit,m)
let mksyn_unit_pat m = Pat_const(Const_unit,m)
let mksyn_delay m e = Expr_lambda (false,false,SPats ([mksyn_spat_var false (mksyn_id m "unitVar")],m), e, m)

let (|Expr_lid_or_id_get|_|) inp = 
    match inp with
    | Expr_lid_get(isOpt,lid, m) -> Some (isOpt,lid,m)
    | Expr_id_get(id) -> Some (false,[id], id.idRange)
    | _ -> None

let (|Expr_single_id_get|_|) inp = 
    match inp with
    | Expr_lid_get(false,[id], _) -> Some id.idText
    | Expr_id_get(id) -> Some id.idText
    | _ -> None
    
let mksyn_assign m l r = 
    let m = union_ranges (range_of_synexpr l) (range_of_synexpr r)
    match l with 
    //| Expr_paren(l2,m2)  -> mksyn_assign m l2 r
    | Expr_lid_or_id_get(false,v,_)  -> Expr_lid_set (v,r,m)
    | Expr_lvalue_get(e,v,_)  -> Expr_lvalue_set (e,v,r,m)
    | Expr_lbrack_get(e1,e2,mDot,_)  -> Expr_lbrack_set (e1,e2,r,mDot,m)
    | Expr_constr_field_get (x,y,z,_) -> Expr_constr_field_set (x,y,z,r,m) 
    | Expr_app (_, Expr_app(_, Expr_single_id_get(nm), a, _),b,_) when nm = qmark_opname -> 
        mksyn_qmark_set m a b r
    | Expr_app (_, Expr_app(_, Expr_single_id_get(nm), a, _),b,_) when nm = lparen_get_opname -> 
        mksyn_dot_lparen_set m a b r
    | Expr_app (_, Expr_lid_get(false,v,_),x,_)  -> Expr_lid_indexed_set (v,x,r,m)
    | Expr_app (_, Expr_lvalue_get(e,v,_),x,_)  -> Expr_lvalue_indexed_set (e,v,x,r,m)
    |   _ -> errorR(Error("invalid expression on left of assignment",m));  Expr_const(Const_unit,m)

let rec mksyn_dot m l r = 
    match l with 
    //| Expr_paren(l2,m2)  -> mksyn_dot m l2 r
    | Expr_lid_get(isOpt,lid,_) -> Expr_lid_get(isOpt,lid@[r],m) // MEMORY PERFORMANCE: This is memory intensive (we create a lot of these list nodes) - an ImmutableArray would be better here
    | Expr_id_get(id) -> Expr_lid_get(false,[id;r],m)
    | Expr_lvalue_get(e,lid,_) -> Expr_lvalue_get(e,lid@[r],m)// MEMORY PERFORMANCE: This is memory intensive (we create a lot of these list nodes) - an ImmutableArray would be better here
    | expr -> Expr_lvalue_get(expr,[r],m)

let rec mksyn_dotn m l r = 
    match l with 
    //| Expr_paren(l2,m2)  -> mksyn_dotn m l2 r
    | Expr_app (_, Expr_app(_, Expr_single_id_get(nm), a, _),Expr_lid_get (false,cid,_),_) when nm = lparen_get_opname-> 
        Expr_constr_field_get (a,cid, r,m)
    |   _ -> errorR(Error("array access or constructor field access expected",m));  Expr_const(Const_unit,m)
        
let mksyn_match_lambda (isMember,isExnMatch,wholem,mtch,spBind) =
    let p,pe = mksyn_new_arg_var wholem
    let _,e = PushCurriedPatternsToExpr wholem isMember [p] (Expr_match(spBind,pe,mtch,isExnMatch,wholem))
    e

let mksyn_fun_match_lambdas isMember wholem ps e = 
    let _,e =  PushCurriedPatternsToExpr wholem isMember ps e 
    e

let mksyn_cons x y =
    let xm = range_of_synexpr x
    Expr_app(ExprAtomicFlag.NonAtomic, Expr_id_get(mksyn_id xm opname_Cons),Expr_tuple([x;y],xm),xm) 

let mksyn_list m l = 
    List.foldBack mksyn_cons l (Expr_id_get(mksyn_id m opname_Nil))

let mksyn_cons_pat x y =
    let xm = range_of_synpat x
    Pat_lid (mksyn_constr xm opname_Cons, None, [Pat_tuple ([x;y],xm)],None,xm)

let mksyn_list_pat m l =
    List.foldBack mksyn_cons_pat l (Pat_lid(mksyn_constr m opname_Nil, None, [], None,m))

//------------------------------------------------------------------------
// Arities of members
// Members have strongly syntactically constrained arities.  We must infer
// the arity from the syntax in order to have any chance of handling recursive 
// cross references during type inference.
//
// So we record the arity for: 
// StaticProperty --> [1]               -- for unit arg
// this.StaticProperty --> [1;1]        -- for unit arg
// StaticMethod(args) --> map InferArgSynInfoFromSimplePat args
// this.InstanceMethod() --> 1 :: map InferArgSynInfoFromSimplePat args
// this.InstanceProperty with get(argpat) --> 1 :: [InferArgSynInfoFromSimplePat argpat]
// StaticProperty with get(argpat) --> [InferArgSynInfoFromSimplePat argpat]
// this.InstanceProperty with get() --> 1 :: [InferArgSynInfoFromSimplePat argpat]
// StaticProperty with get() --> [InferArgSynInfoFromSimplePat argpat]
// 
// this.InstanceProperty with set(argpat)(v) --> 1 :: [InferArgSynInfoFromSimplePat argpat; 1]
// StaticProperty with set(argpat)(v) --> [InferArgSynInfoFromSimplePat argpat; 1]
// this.InstanceProperty with set(v) --> 1 :: [1]
// StaticProperty with set(v) --> [1] 
//-----------------------------------------------------------------------

module SynInfo = begin
    let unnamedTopArg1 = ArgSynInfo([],false,None)
    let unnamedTopArg = [unnamedTopArg1]
    let unitArgData = unnamedTopArg
    let unnamedRetVal = ArgSynInfo([],false,None)
    let selfMetadata = unnamedTopArg

    let HasNoArgs (ValSynInfo(args,_)) = isNil args
    let HasOptionalArgs (ValSynInfo(args,_)) = List.exists (List.exists (fun (ArgSynInfo(_,isOptArg,_)) -> isOptArg)) args
    let IncorporateEmptyTupledArg (ValSynInfo(args,retInfo)) = ValSynInfo([]::args,retInfo)
    let IncorporateSelfArg (ValSynInfo(args,retInfo)) = ValSynInfo(selfMetadata::args,retInfo)
    let IncorporateSetterArg (ValSynInfo(args,retInfo)) = 
         let args = 
             match args with 
             [] -> [unnamedTopArg] 
             | [arg] -> [arg@[unnamedTopArg1]] 
             | _ -> failwith "invalid setter type" 
         ValSynInfo(args,retInfo)
    let NumCurriedArgs(ValSynInfo(args,_)) = List.length args
    let AritiesOfArgs (ValSynInfo(args,_)) = List.map List.length args
    let AttribsOfArgData (ArgSynInfo(attribs,_,_)) = attribs
    let IsOptionalArg (ArgSynInfo(_,isOpt,_)) = isOpt
    let rec InferArgSynInfoFromSimplePat attribs p = 
        match p with 
        | SPat_as(nm,compgen,_,isOpt,_) -> 
           (* if List.length attribs <> 0 then dprintf "List.length attribs = %d\n" (List.length attribs); *)
           ArgSynInfo(attribs, isOpt, (if compgen then None else Some nm))
        | SPat_typed(a,_,_) -> InferArgSynInfoFromSimplePat attribs a
        | SPat_attrib(a,attribs2,_) -> InferArgSynInfoFromSimplePat (attribs @ attribs2) a
      
    let rec InferArgSynInfoFromSimplePats x = 
        match x with 
        | SPats(ps,_) -> List.map (InferArgSynInfoFromSimplePat []) ps
        | SPats_typed(ps,_,_) -> InferArgSynInfoFromSimplePats ps

    let InferArgSynInfoFromPat p = 
        let sp,_ = SimplePatsOfPat p
        InferArgSynInfoFromSimplePats sp

    /// Make sure only a solitary unit argument has unit elimination
    let AdjustArgsForUnitElimination infosForArgs = 
        match infosForArgs with 
        | [[]] -> infosForArgs 
        | _ -> infosForArgs |> List.map (function [] -> unitArgData | x -> x)

    let AdjustMemberArgs memFlags infosForArgs = 
        match infosForArgs with 
        // Transform a property declared using '[static] member P = expr' to a method taking a "unit" argument 
        | [] when memFlags=MemberKindMember -> [] :: infosForArgs
        | _ -> infosForArgs

    let InferLambdaArgs origRhsExpr = 
        let rec loop e = 
            match e with 
            | Expr_lambda(false,_,spats,rest,_) -> 
                InferArgSynInfoFromSimplePats spats :: loop rest
            | _ -> []
        loop origRhsExpr

    let InferSynReturnData retInfo = 
        match retInfo with 
        | None -> unnamedRetVal 
        | Some((_,retInfo),_) -> retInfo

    let emptyValSynInfo = ValSynInfo([],unnamedRetVal)
    let emptyValSynData = ValSynData(None,emptyValSynInfo,None)

    let InferValSynData memberFlagsOpt pat retInfo origRhsExpr = 

        let infosForExplicitArgs = 
            match pat with 
            | Some(Pat_lid(_,_,curriedArgs,_,m)) -> List.map InferArgSynInfoFromPat curriedArgs
            | _ -> []

        let explicitArgsAreSimple = 
            match pat with 
            | Some(Pat_lid(_,_,curriedArgs,_,m)) -> List.forall IsSimplePattern curriedArgs
            | _ -> true

        let retInfo = InferSynReturnData retInfo

        match memberFlagsOpt with
        | None -> 
            let infosForLambdaArgs = InferLambdaArgs origRhsExpr
            let infosForArgs = infosForExplicitArgs @ (if explicitArgsAreSimple then infosForLambdaArgs else [])
            let infosForArgs = AdjustArgsForUnitElimination infosForArgs 
            ValSynData(None,ValSynInfo(infosForArgs,retInfo),None)
        | Some memFlags  -> 
            let infosForObjArgs = 
                if memFlags.MemberIsInstance then [ selfMetadata ] else []

            let infosForArgs = AdjustMemberArgs memFlags.MemberKind infosForExplicitArgs
            let infosForArgs = AdjustArgsForUnitElimination infosForArgs 
            
            let argInfos = infosForObjArgs @ infosForArgs
            ValSynData(Some(memFlags),ValSynInfo(argInfos,retInfo),None)
end


let mksyn_binding_rhs staticOptimizations rhsExpr rhsRange retInfo =
    let rhsExpr = List.foldBack (fun (c,e1) e2 -> Expr_static_optimization (c,e1,e2,rhsRange)) staticOptimizations rhsExpr
    let rhsExpr,retTyOpt = 
        match retInfo with 
        | Some ((ty,ArgSynInfo(rattribs,_,_)),tym) -> Expr_typed(rhsExpr,ty,range_of_synexpr rhsExpr), Some(ty,tym,rattribs) 
        | None -> rhsExpr,None 
    rhsExpr,retTyOpt

let mksyn_binding (xmlDoc,headPat) (vis,pseudo,mut,bindm,spBind,wholem,retInfo,origRhsExpr,rhsRange,staticOptimizations,attrs,memberFlagsOpt) =
    let info = SynInfo.InferValSynData memberFlagsOpt (Some headPat) retInfo origRhsExpr
    let rhsExpr,retTyOpt = mksyn_binding_rhs staticOptimizations origRhsExpr rhsRange retInfo
    // dprintfn "headPat = %A, info = %A" headPat info
    // PERFORMANCE: There are quite a lot of these nodes allocated. Perhaps not much we can do about that.
    Binding (vis,NormalBinding,pseudo,mut,attrs,xmlDoc,info,headPat,BindingRhs([],retTyOpt,rhsExpr),bindm,spBind) 

let NonVirtualMemberFlags q k = { MemberKind=k; OverloadQualifier=q;  MemberIsInstance=true;  MemberIsVirtual=false; MemberIsDispatchSlot=false; MemberIsOverrideOrExplicitImpl=false; MemberIsFinal=false }
let CtorMemberFlags q =      { OverloadQualifier=q;MemberKind=MemberKindConstructor; MemberIsInstance=false; MemberIsVirtual=false; MemberIsDispatchSlot=false; MemberIsOverrideOrExplicitImpl=false;  MemberIsFinal=false }
let ClassCtorMemberFlags =      { OverloadQualifier=None;MemberKind=MemberKindClassConstructor; MemberIsInstance=false; MemberIsVirtual=false; MemberIsDispatchSlot=false; MemberIsOverrideOrExplicitImpl=false;  MemberIsFinal=false }
let OverrideMemberFlags q k =   { MemberKind=k; OverloadQualifier=q;  MemberIsInstance=true;  MemberIsVirtual=false;  MemberIsDispatchSlot=false; MemberIsOverrideOrExplicitImpl=true;  MemberIsFinal=false }
let AbstractMemberFlags q k =   { MemberKind=k; OverloadQualifier=q;  MemberIsInstance=true;  MemberIsVirtual=false;  MemberIsDispatchSlot=true;  MemberIsOverrideOrExplicitImpl=false;  MemberIsFinal=false }
let StaticMemberFlags q k = { MemberKind=k; OverloadQualifier=q;  MemberIsInstance=false; MemberIsVirtual=false; MemberIsDispatchSlot=false; MemberIsOverrideOrExplicitImpl=false;  MemberIsFinal=false }

let inferredTyparDecls = SynValTyparDecls([],true,[])
let noInferredTypars = SynValTyparDecls([],false,[])

//------------------------------------------------------------------------
// Lexer args: status of #if/#endif processing.  
//------------------------------------------------------------------------

type ifdefStackEntry = IfDefIf | IfDefElse 
type ifdefStackEntries = (ifdefStackEntry * range) list
type ifdefStack = ifdefStackEntries ref

/// Specifies how the 'endline' function in the lexer should continue after
/// it reaches end of line or eof. The options are to continue with 'token' function
/// or to continue with 'ifdef_skip' function.
type endlinecont = 
    | ENDL_token of ifdefStackEntries
    | ENDL_skip of ifdefStackEntries * int * range
    member x.IfdefStack = 
      match x with | ENDL_token(ifd) | ENDL_skip(ifd, _, _) -> ifd
          
/// The parser defines a number of tokens for whitespace and
/// comments eliminated by the lexer.  These carry a specification of
/// a continuation for the lexer when used in scenarios where we don't
/// care about whitespace.
type lexcont = 
    | AT_token             of ifdefStackEntries
    | AT_ifdef_skip        of ifdefStackEntries * int * range
    | AT_string            of ifdefStackEntries *range
    | AT_vstring           of ifdefStackEntries * range
    | AT_comment           of ifdefStackEntries * int * range
    | AT_tokenized_comment of ifdefStackEntries * int * range
    | AT_comment_string    of ifdefStackEntries * int * range
    | AT_comment_vstring   of ifdefStackEntries * int * range
    | AT_camlonly          of ifdefStackEntries * range
    | AT_endline           of endlinecont
    
    member x.IfdefStack =
        match x with 
        | AT_token (ifd)
        | AT_ifdef_skip (ifd,_,_)
        | AT_string (ifd,_)
        | AT_vstring (ifd,_)
        | AT_comment (ifd,_,_)
        | AT_tokenized_comment (ifd,_,_)
        | AT_comment_string (ifd,_,_)
        | AT_comment_vstring (ifd,_,_)
        | AT_camlonly (ifd,_) -> ifd
        | AT_endline(endl) -> endl.IfdefStack


(*------------------------------------------------------------------------
 * Parser/Lexer state
 *-----------------------------------------------------------------------*)

exception SyntaxError of obj (* ParseErrorContext<_> *) * range

type ConcreteSyntaxSink = 
    { MatchPair: (range -> range -> unit) }

let pos_of_lexpos (p:Position) = 
    mk_pos p.Line p.Column

let mksyn_range (p1:Position) p2 = 
    mk_file_idx_range (decode_file_idx p1.FileName) (pos_of_lexpos p1) (pos_of_lexpos p2)

let GetLexerRange (lexbuf:UnicodeLexing.Lexbuf) = 
    mksyn_range lexbuf.StartPos lexbuf.EndPos

let GetParserLexbuf (parseState: IParseState) = 
    assert (parseState.ParserLocalStore.ContainsKey("LexBuffer"));
    assert (parseState.ParserLocalStore.["LexBuffer"] :? UnicodeLexing.Lexbuf);
    (parseState.ParserLocalStore.["LexBuffer"] :?> UnicodeLexing.Lexbuf)

// The key into the ParserLocalStore and BufferLocalStore used to hold the concreateSyntaxSink
let concreteSyntaxSinkKey = "ConcreteSyntaxSink" 

let GetConcreteSyntaxSink (parseState: IParseState) = 
    if parseState.ParserLocalStore.ContainsKey(concreteSyntaxSinkKey) then 
        (parseState.ParserLocalStore.[concreteSyntaxSinkKey] :?> ConcreteSyntaxSink option)
    else
        let lexbuf = GetParserLexbuf parseState
        let res = 
            if lexbuf.BufferLocalStore.ContainsKey(concreteSyntaxSinkKey) then 
                assert (lexbuf.BufferLocalStore.[concreteSyntaxSinkKey] :? ConcreteSyntaxSink);
                Some (lexbuf.BufferLocalStore.[concreteSyntaxSinkKey] :?> ConcreteSyntaxSink) 
            else 
                None
        parseState.ParserLocalStore.[concreteSyntaxSinkKey] <- res;
        res

let SetConcreteSyntaxSink (lexbuf:UnicodeLexing.Lexbuf) (concreteSyntaxSink: ConcreteSyntaxSink option) =
    match concreteSyntaxSink with 
    | None -> 
        ()
    | Some r -> 
        lexbuf.BufferLocalStore.[concreteSyntaxSinkKey] <- r

/// Get the range corresponding to the result of a grammar rule while it is being reduced
let lhs (parseState: IParseState) = 
    let p1,p2 = parseState.ResultRange
    mksyn_range p1 p2

/// Get the position corresponding to the start of one of the r.h.s. symbols of a grammar rule while it is being reduced
let rhspos (parseState: IParseState) n = 
    pos_of_lexpos (parseState.InputStartPosition(n))

/// Get the range covering two of the r.h.s. symbols of a grammar rule while it is being reduced
let rhs2 (parseState: IParseState) n m = 
    let p1 = parseState.InputStartPosition(n) 
    let p2 = parseState.InputEndPosition(m) 
    mksyn_range p1 p2

/// Get the range corresponding to one of the r.h.s. symbols of a grammar rule while it is being reduced
let rhs parseState n = rhs2 parseState n n 

let MatchPair parseState p1 p2 = 
    match GetConcreteSyntaxSink(parseState) with 
    | None -> ()
    | Some snk -> snk.MatchPair (rhs parseState p1) (rhs parseState p2)

//------------------------------------------------------------------------
// XmlDoc F# lexer/parser state (thread local)
//------------------------------------------------------------------------

// The key into the BufferLocalStore used to hold the current accumulated XmlDoc lines 
module LexbufLocalXmlDocStore = 
    let private xmlDocKey = "XmlDoc"

    let ClearXmlDoc (lexbuf:Lexbuf) = 
        lexbuf.BufferLocalStore.[xmlDocKey] <- box (XmlDocCollector())

    let SaveXmlDoc (lexbuf:Lexbuf) (line,pos) = 
        if not (lexbuf.BufferLocalStore.ContainsKey(xmlDocKey)) then 
            lexbuf.BufferLocalStore.[xmlDocKey] <- box (XmlDocCollector())
        let collector = unbox<XmlDocCollector>(lexbuf.BufferLocalStore.[xmlDocKey])
        collector.AddXmlDocLine(line,pos)

    let GrabXML (lexbuf:Lexbuf, markerRange)  = 
        if lexbuf.BufferLocalStore.ContainsKey(xmlDocKey) then 
            PreXmlDoc.CreateFromGrabPoint(unbox<XmlDocCollector>(lexbuf.BufferLocalStore.[xmlDocKey]),end_of_range markerRange)
        else
            emptyPreXmlDoc
#if DEBUG
    let DumpXmlDoc note (XmlDoc lines) = 
        printf "\nXmlDoc: %s\n" note; 
        Array.iter (printf "  %s\n") lines; 
        XmlDoc lines
#endif


   
/// Generates compiler-generated names marked up with a source code location
type NiceNameGenerator() = 

    let basicNameCounts = new System.Collections.Generic.Dictionary<string,_>(100)

    member x.FreshCompilerGeneratedName (name,m) =
        let basicName = GetBasicNameOfPossibleCompilerGeneratedName name
        let n = (if basicNameCounts.ContainsKey basicName then basicNameCounts.[basicName] else 0) 
        let nm = CompilerGeneratedNameSuffix basicName (string (start_line_of_range m) ^ (match n with 0 -> "" | n -> "-" ^ string n))
        basicNameCounts.[basicName] <- n+1
        nm

    member x.Reset () = basicNameCounts.Clear()

   

/// Generates compiler-generated names marked up with a source code location, but if given the same unique value then
/// return precisely the same name
type StableNiceNameGenerator() = 

    let names = new System.Collections.Generic.Dictionary<(string * int64),_>(100)
    let basicNameCounts = new System.Collections.Generic.Dictionary<string,_>(100)

    member x.GetUniqueCompilerGeneratedName (name,m,uniq) =
        let basicName = GetBasicNameOfPossibleCompilerGeneratedName name
        if names.ContainsKey (basicName,uniq) then
            names.[(basicName,uniq)]
        else 
            let n = (if basicNameCounts.ContainsKey basicName then basicNameCounts.[basicName] else 0) 
            let nm = CompilerGeneratedNameSuffix basicName (string (start_line_of_range m) ^ (match n with 0 -> "" | n -> "-" ^ string n))
            names.[(basicName,uniq)] <- nm
            basicNameCounts.[basicName] <- n+1
            nm

    member x.Reset () = 
        basicNameCounts.Clear()
        names.Clear()

/// A global generator of compiler generated names
let globalNng = NiceNameGenerator()
/// A global generator of stable compiler generated names
let globalStableNameGenerator = StableNiceNameGenerator ()
