%{
// (c) Microsoft Corporation. All rights reserved

open Internal.Utilities
open Internal.Utilities.Pervasives

open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler
open Internal.Utilities.Text.Parsing

open System
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.PrettyNaming
open Microsoft.FSharp.Compiler.ErrorLogger

let mk_optional m xopt = 
    match xopt with 
    | None -> mksyn_lid_get m Ast.lib_MFCore_path "None"
    | Some x  -> Expr_app(ExprAtomicFlag.NonAtomic, mksyn_lid_get m Ast.lib_MFCore_path "Some",x,m)

let mk_Do (vis,strict,expr,m) = 
    if isSome vis then errorR(Error("Visibility declarations are not permitted on 'do' bindings",m));
    Binding (None,
             (if strict then DoBinding else StandaloneExpression),
             false,false,[],emptyPreXmlDoc,SynInfo.emptyValSynData,
             (if strict then Pat_const(Const_unit,m) else Pat_wild m),
             BindingRhs([],None,expr),m,NoSequencePointAtDoBinding)

let mk_Def_expr (e,m) = 
    let spExpr = if IsControlFlowExpression e then NoSequencePointAtDoBinding else SequencePointAtBinding (range_of_synexpr e) in
    Def_expr(spExpr,e,m)

let addAttribs attrs p =  Pat_attrib(p,attrs,range_of_synpat p)

let computeOverloadQualifier attrs = 
  let attrs = 
      attrs |> List.choose (fun attr -> 
          match attr with 
          | (Attr(lid,(Expr_const(Const_string (s,_),_) | Expr_paren(Expr_const(Const_string (s,_),_),_)),_,_)) ->
              begin match List.frontAndBack lid with 
              | (_,id) when id.idText = "OverloadID" or id.idText = "OverloadIDAttribute" -> Some(s)
              | _ -> None
              end
          | _ -> None) in
  match attrs with 
  | [x] -> Some x
  | [] -> None
  | _ -> failwith "Multiple OverloadID attributes"

(* error recovery*)
let arbExpr(parseState) = Expr_arb(lhs(parseState))

let mksyn_anon_constraint ty m = Type_anon_constraint(ty,m) 

(* This function is called by the generated parser code. Returning initiates error recovery *)
let parse_error_rich = Some (fun (ctxt: ParseErrorContext<_>) -> 
    errorR(SyntaxError(box(ctxt), GetLexerRange (GetParserLexbuf ctxt.ParseState))))

let reportParseErrorAt m s = errorR(Error(s,m))

let reportParseWarningAt m s = warning(Error(s,m))

let raiseParseErrorAt m s = 
    reportParseErrorAt m s; 
    // This initiates error recovery
    raise RecoverableParseError 

let checkEndOfFileError t = 
  match t with 
  | AT_ifdef_skip(_,_,m) -> reportParseErrorAt m "end of file in #if section begun at or after here"
  | AT_string (_,m) ->  reportParseErrorAt m "end of file in string begun at or before here"
  | AT_vstring (_,m) ->  reportParseErrorAt m "end of file in verbatim string begun at or before here"
  | AT_comment (_,_,m) ->  reportParseErrorAt m "end of file in comment begun at or before here"
  | AT_tokenized_comment (_,_,m) ->  reportParseErrorAt m "end of file in comment begun at or before here"
  | AT_comment_string (_,_,m) -> reportParseErrorAt m "end of file in string embedded in comment begun at or before here"
  | AT_comment_vstring (_,_,m) -> reportParseErrorAt m "end of file in verbatim string embedded in comment begun at or before here"
  | AT_camlonly (_,m) -> reportParseErrorAt m "end of file in IF-OCAML section begun at or before here" 
  | AT_endline(ENDL_skip(_,_,m)) -> reportParseErrorAt m "end of file in directive begun at or before here"
  | AT_endline(ENDL_token(stack))
  | AT_token(stack) -> 
      match stack with 
      | [] -> ()
      | (_,m) :: _  -> reportParseErrorAt m "no #endif found for #if or #else" 

type BindingSet = BindingSetPreAttrs of range * bool * bool * (SynAttributes -> access option -> SynAttributes * SynBinding list)

let mkClassMemberLocalBindings(isStatic,wholem,attrs,vis,BindingSetPreAttrs(_,isRec,isUse,declsPreAttrs)) = 
   let ignoredFreeAttrs,decls = declsPreAttrs attrs vis in
   if nonNil ignoredFreeAttrs then warning(Error("attributes have been ignored in this construct",wholem));
   if isUse then errorR(Error("'use' bindings are not permitted in implicit class constructors",wholem));
   ClassMemberDefn_let_bindings (decls,isStatic,isRec,wholem)

let mkLocalBindings (wholem,BindingSetPreAttrs(_,isRec,isUse,declsPreAttrs),body) = 
   let ignoredFreeAttrs,decls = declsPreAttrs [] None in 
   if nonNil ignoredFreeAttrs then warning(Error("attributes have been ignored in this construct",wholem));
   Expr_let (isRec,isUse,decls,body,wholem) 

let mkDefnBindings (wholem,BindingSetPreAttrs(_,isRec,isUse,declsPreAttrs),attrs,vis,attrsm) = 
    if isUse then errorR(Error("'use' bindings are not permitted in modules",wholem));
    let freeAttrs,decls = declsPreAttrs attrs vis in 
    let letDecls = [ Def_let (isRec,decls,wholem) ] in 
    let attrDecls = if nonNil freeAttrs then [ Def_attributes (freeAttrs,attrsm) ] else [] in 
    attrDecls @ letDecls

let id_of_pat m p = 
    match p with 
    | Pat_as (Pat_wild _,id,false,_,_) -> id 
    | _ -> raiseParseErrorAt m "an integer for loop must use a simple identifier"

let checkForMultipleAugmentations m a1 a2 = 
    if nonNil a1 && nonNil a2 then raiseParseErrorAt m "at most one 'with' augmentation is permitted";
    a1 @ a2

let grabXmlDoc(parseState,elemIdx) = 
    LexbufLocalXmlDocStore.GrabXML(GetParserLexbuf parseState,rhs parseState elemIdx)


%} 

%token <byte[]> BYTEARRAY
%token <string> STRING 
%token <string> IDENT 
%token <string> INFIX_STAR_STAR_OP 
%token <string> INFIX_COMPARE_OP 
%token <string> INFIX_AT_HAT_OP 
%token <string> INFIX_BAR_OP 
%token <string> PREFIX_OP
%token <string> INFIX_STAR_DIV_MOD_OP 
%token <string> INFIX_AMP_OP 
%token <string> PLUS_MINUS_OP 
%token <string> ADJACENT_PREFIX_PLUS_MINUS_OP 
%token <string> FUNKY_OPERATOR_NAME

/* bool indicates if INT8 was 'bad' max_int+1, e.g. '128'  */
%token <sbyte * bool> INT8 
%token <int16 * bool> INT16
%token <int32 * bool> INT32 INT32_DOT_DOT
%token <int64 * bool> INT64

%token <byte> UINT8
%token <uint16> UINT16
%token <uint32> UINT32
%token <uint64> UINT64
%token <uint64> UNATIVEINT
%token <int64> NATIVEINT
%token <single> IEEE32
%token <double> IEEE64
%token <char> CHAR
%token <System.Decimal> DECIMAL 
%token <(string * string)> BIGNUM
%token <bool> LET YIELD YIELD_BANG
%token <string> SPLICE_SYMBOL PERCENT_OP BINDER 
%token <string * bool> LQUOTE RQUOTE  RQUOTE_DOT 
%token BAR_BAR LESS GREATER UPCAST DOWNCAST NULL RESERVED MODULE NAMESPACE DELEGATE CONSTRAINT BASE
%token AND AS ASSERT ASR BEGIN DO DONE DOWNTO ELSE ELIF END DOT_DOT
%token EXCEPTION FALSE FOR FUN FUNCTION IF IN FINALLY DO_BANG 
%token LAZY  MATCH METHOD MUTABLE NEW OF 
%token OPEN OR REC THEN TO TRUE TRY TYPE VAL INLINE INTERFACE INSTANCE
%token WHEN WHILE WITH HASH AMP AMP_AMP QUOTE LPAREN RPAREN STAR COMMA RARROW RARROW2 GREATER_DOT GREATER_BAR_RBRACK LPAREN_STAR_RPAREN
%token QMARK QMARK_QMARK DOT COLON COLON_COLON COLON_GREATER  COLON_QMARK_GREATER COLON_QMARK COLON_EQUALS SEMICOLON 
%token SEMICOLON_SEMICOLON LARROW EQUALS  LBRACK  LBRACK_BAR  LBRACK_LESS LBRACE
%token LBRACE_LESS BAR_RBRACK GREATER_RBRACE UNDERSCORE
%token BAR RBRACK RBRACE MINUS DOLLAR
%token GREATER_RBRACK STRUCT SIG 
%token STATIC MEMBER CLASS VIRTUAL ABSTRACT OVERRIDE DEFAULT CONSTRUCTOR INHERIT 
%token EXTERN VOID PUBLIC PRIVATE INTERNAL 

/* for high-precedence tyapps and apps */
%token HIGH_PRECEDENCE_APP   /* inserted for f(x), but not f (x) */
%token HIGH_PRECEDENCE_TYAPP /* inserted for x<y>, but not x<y */

/* for offside rule */
%token <bool> OLET      /* LexFilter #light converts 'LET' tokens to 'OLET' when starting (CtxtLetDecl(blockLet=true)) */
%token <string> OBINDER /* LexFilter #light converts 'BINDER' tokens to 'OBINDER' when starting (CtxtLetDecl(blockLet=true)) */
%token ODO              /* LexFilter #light converts 'DO' tokens to 'ODO' */
%token ODO_BANG         /* LexFilter #light converts 'DO_BANG' tokens to 'ODO_BANG' */
%token OTHEN            /* LexFilter #light converts 'THEN' tokens to 'OTHEN' */
%token OELSE            /* LexFilter #light converts 'ELSE' tokens to 'OELSE' except if immeditely followed by 'if', when they become 'ELIF' */
%token OWITH            /* LexFilter #light converts SOME (but not all) 'WITH' tokens to 'OWITH' */ 
%token OFUNCTION        /* LexFilter #light converts 'FUNCTION' tokens to 'OFUNCTION' */ 
%token OFUN             /* LexFilter #light converts 'FUN' tokens to 'OFUN' */


%token ORESET           /* LexFilter uses internally to force a complete reset on a ';;' */

%token OBLOCKBEGIN      /* LexFilter #light inserts for:
                                  - just after first '=' or ':' when in 'CtxtModuleHead', i.e. after 'module' and sequence of dot/identifier/access tokens
                                  - just after first '=' when in 'CtxtMemberHead'
                                  - just after first '=' when in 'CtxtType' 
                                  - just after 'do' in any context (when opening CtxtDo)
                                  - just after 'finally' in any context 
                                  - just after 'with' (when opening CtxtWithAsAugment)
                                  - just after 'else' (when opening CtxtElse)
                                  - just after 'then' (when opening CtxtThen)
                                  - just after 'interface' (when pushing CtxtParen(INTERFACE), i.e. next token is DEFAULT | OVERRIDE | INTERFACE | NEW | TYPE | STATIC | END | MEMBER | ABSTRACT  | INHERIT | LBRACK_LESS)
                                  - just after 'class' (when pushing CtxtParen(CLASS)
                                  - just after 'class' 
                           But not when opening these CtxtSeqBlocks:
                                  - just after first non-dot/identifier token past 'namespace' 
                                  - just after first '=' when in 'CtxtLetDecl' or 'CtxtWithAsLet' 
                                  - just after 'lazy' in any context
                                  - just after '->' in any context                                  
                                  - when opening CtxtNamespaceHead, CtxtModuleHead 
                        */
%token OBLOCKSEP        /* LexFilter #light inserts when transforming CtxtSeqBlock(NotFirstInSeqBlock,_,AddBlockEnd) to CtxtSeqBlock(FirstInSeqBlock,_,AddBlockEnd) on exact alignment */

/*    REVIEW: merge OEND, ODECLEND, OBLOCKEND and ORIGHT_BLOCK_END into one token */
%token OEND             /* LexFilter #light inserts when closing CtxtFun, CtxtMatchClauses, CtxtWithAsLet _        */
%token ODECLEND         /* LexFilter #light inserts when closing CtxtDo and CtxtLetDecl(block) */
%token ORIGHT_BLOCK_END /* LexFilter #light inserts when closing CtxtSeqBlock(_,_,AddOneSidedBlockEnd) */
%token OBLOCKEND        /* LexFilter #light inserts when closing CtxtSeqBlock(_,_,AddBlockEnd) */

%token OINTERFACE_MEMBER /* inserted for non-paranthetical use of 'INTERFACE', i.e. not INTERFACE/END */
%token <token> ODUMMY

/* These are artificial */
%token <string> LEX_FAILURE
%token <Ast.lexcont> COMMENT WHITESPACE HASH_LINE HASH_LIGHT INACTIVECODE LINE_COMMENT STRING_TEXT EOF
%token <range * string * Ast.lexcont> HASH_IF HASH_ELSE HASH_ENDIF 

%start signatureFile implementationFile interaction
%type <Ast.ParsedImplFile> implementationFile
%type <Ast.ParsedSigFile> signatureFile
%type <Ast.interaction> interaction
%type <Ast.ident> ident
%type <Ast.SynType> typ
%type <Ast.SynTyconSpfn list> tyconSpfns
%type <Ast.SynExpr> declExpr
%type <Ast.SynPat> headBindingPattern


/* About precedence rules: 
 * 
 * Tokens and dummy-terminals are given precedence below (lowest first).
 * A rule has precedence of the first token or the dummy terminal given after %prec.
 * The precedence resolve shift/reduce conflicts:
 *   (a) If either rule has no precedence:
 *       S/R: shift over reduce, and
 *       R/R: reduce earlier rule over later rule.
 *   (b) If both rules have precedence:
 *       S/R: choose highest precedence action (precedence of reduce rule vs shift token)
 *            if same precedence: leftassoc gives reduce, rightassoc gives shift, nonassoc error.
 *       R/R: reduce the rule that comes first (textually first in the yacc file)
 *
 * Advice from: http://dinosaur.compilertools.net/yacc/
 *
 *   'Conflicts resolved by precedence are not counted in the number of S/R and R/R
 *    conflicts reported by Yacc. This means that mistakes in the moduleSpfn of
 *    precedences may disguise errors in the input grammar; it is a good idea to be
 *    sparing with precedences, and use them in an essentially ``cookbook'' fashion,
 *    until some experience has been gained'
 *
 * Observation:
 *   It is possible to eliminate conflicts by giving precedence to rules and tokens.
 *   Dummy tokens can be used for the rule and the tokens also need precedence.
 *   The danger is that giving precedence to the tokens may twist the grammar elsewhere.
 *   Maybe it would be good to assign precedence at given locations, e.g.
 *
 *   order: precShort precLong
 *
 *   rule: TokA TokB %@precShort        {action1}     -- assign prec to rule.
 *       | TokA TokB TokC@precLong TokD {action2}     -- assign prec to TokC at this point.
 *
 * Observation: reduce/reduce
 *   If there is a common prefix with a reduce/reduce conflict,
 *   e.g "OPEN path" for topopens and moduleDefns then can factor
 *   opendef = "OPEN path" which can be on both paths.
 *
 * Debugging and checking precedence rules.
 *   - comment out a rule's %prec and see what conflicts are introduced.
 *
 * Dummy terminals (like prec_type_prefix) can assign precedence to a rule.
 * Doc says rule and (shift) token precedence resolves shift/reduce conflict.
 * It seems like dummy terminals can not assign precedence to the shift,
 * but including the tokens in the precedences below will order them.
 * e.g. prec_type_prefix lower precedence than RARROW, LBRACK, IDENT, LAZY, STAR (all extend types).
 */

/* start with lowest */

%nonassoc prec_args_error             /* less than RPAREN */
%nonassoc prec_atomexpr_lparen_error  /* less than RPAREN */

%right AS

/* prec_wheretyp_prefix = "where typ" lower than extensions, i.e. "WHEN" */
%nonassoc prec_wheretyp_prefix        /* lower than WHEN and RPAREN */
%nonassoc RPAREN

%right WHEN

/* prec_pat_pat_action = "pattern when expr -> expr"
 * Lower than match extensions - i.e. BAR.
 */
%nonassoc prec_pat_pat_action          /* lower than BAR */

/* "a then b" as an object constructor is very low precedence */
/* Lower than "if a then b" */
%left prec_then_before
%nonassoc prec_then_if
%left  BAR

%right SEMICOLON  prec_semiexpr_sep OBLOCKSEP
%right prec_defn_sep

/* prec_atompat_pathop = precedence of at atomic pattern, e.g "Constructor".
 * Lower than possible pattern extensions, so "pathop . extension" does shift not reduce.
 * possible extensions are:
 *  - constant terminals.
 *  - null
 *  - LBRACK = [
 *  - TRUE,FALSE
 */
%nonassoc prec_atompat_pathop
%nonassoc INT8 UINT8 INT16 UINT16 INT32 UINT32 INT64 UINT64 NATIVEINT UNATIVEINT IEEE32 IEEE64 CHAR STRING BYTEARRAY BIGNUM DECIMAL
%nonassoc LPAREN LBRACE LBRACK_BAR 
%nonassoc TRUE FALSE UNDERSCORE NULL


/* prec_typ_prefix        lower than "T  -> T  -> T" extensions.
 * prec_tuptyp_prefix     lower than "T * T * T * T" extensions.
 * prec_tuptyptail_prefix lower than "T * T * T * T" extensions.
 * Lower than possible extensions:
 *  - STAR, LAZY, IDENT, RARROW
 *  - LBRACK = [ - for "base[]" types              
 * Shifts not reduces.
 */
%nonassoc prec_typ_prefix             /* lower than STAR, LAZY, IDENT, RARROW etc */
%nonassoc prec_tuptyp_prefix          /* ditto */
%nonassoc prec_tuptyptail_prefix      /* ditto */
%nonassoc prec_toptuptyptail_prefix      /* ditto */
        
%right    RARROW
%nonassoc IDENT LAZY LBRACK

/* prec_opt_attributes_none = precedence of no attributes
 * These can prefix LET-moduleDefns.
 * Committing to an opt_attribute (reduce) forces the decision that a following LET is a moduleDefn.
 * At the top-level, it could turn out to be an expr, so prefer to shift and find out...
 */
%nonassoc prec_opt_attributes_none    /* lower than LET,NEW */

/* LET,NEW higher than SEMICOLON so shift
 *   "seqExpr = seqExpr; . let x = y in z"
 *   "seqExpr = seqExpr; . new...."
 */
%nonassoc LET NEW

       
/* Redundant dummies: expr_let, expr_function, expr_fun, expr_match */
/* Resolves conflict: expr_try, expr_if */
%nonassoc expr_let
%nonassoc decl_let
%nonassoc expr_function expr_fun expr_match expr_try expr_do
%nonassoc decl_match decl_do
%nonassoc expr_if                     /* lower than ELSE to disambiguate "if _ then if _ then _ else _" */
%nonassoc ELSE   

/* prec_atomtyp_path = precedence of atomType "path"
 * Lower than possible extension "path<T1,T2>" to allow "path . <" shift.
 * Extensions: LESS
 */
%nonassoc prec_atomtyp_path           /* lower than LESS */
%nonassoc prec_atomtyp_get_path       /* lower than LESS */

/* prec_no_more_attr_bindings = precedence of "more_localBindings = ."
 * Lower precedence than AND so further bindings are shifted.
 */
%nonassoc prec_no_more_attr_bindings  /* lower than AND */
%nonassoc OPEN

/* prec_interfaces_prefix - lower than extensions, i.e. INTERFACE */
%nonassoc prec_interfaces_prefix      /* lower than INTERFACE */
%nonassoc INTERFACE

%right LARROW 
%right COLON_EQUALS 
%nonassoc pat_tuple expr_tuple
%left COMMA
%nonassoc slice_comma  /* for matrix.[1..2,3..4] the ".." has higher precedence than "2,3" */
%nonassoc DOT_DOT /* for matrix.[1..2,3..4] the ".." has higher precedence than "2,3" */
%nonassoc paren_pat_colon
%nonassoc paren_pat_attribs
%left OR BAR_BAR
%left AND   /* check */
%left  AMP AMP_AMP 
%nonassoc pat_conj
%nonassoc expr_not
%left INFIX_COMPARE_OP DOLLAR LESS GREATER EQUALS  INFIX_BAR_OP INFIX_AMP_OP
%right INFIX_AT_HAT_OP
%right COLON_COLON
%nonassoc pat_isinst expr_isinst COLON_GREATER  
%left PLUS_MINUS_OP MINUS expr_prefix_plus_minus ADJACENT_PREFIX_PLUS_MINUS_OP
%left  INFIX_STAR_DIV_MOD_OP STAR PERCENT_OP
%right INFIX_STAR_STAR_OP
%left  QMARK_QMARK
%left head_expr_adjacent_minus
%left expr_app expr_assert expr_lazy
%left arg_expr_adjacent_minus
%left expr_args
%right matching_bar
%left pat_app
%left pat_args
%left PREFIX_OP
%left DOT QMARK
%left HIGH_PRECEDENCE_APP
%left HIGH_PRECEDENCE_TYAPP


%nonassoc prec_interaction_empty

%%

/* F# TopLevel */
/* NOTE: interactions */
/* A SEMICOLON_SEMICOLON (or EOF) will mark the end of all interaction blocks. */
/* The end of interaction blocks must be determined without needing to lookahead one more token. */
/* A lookahead token would be dropped between parser calls. See bug 1027. */

interaction:
  | interactiveItemsTerminator
     { IDefns ($1,lhs(parseState)) }
  | SEMICOLON 
     { warning(Error("A semicolon is not expected at this point",rhs parseState 1));
       IDefns ([],lhs(parseState)) }
  | OBLOCKSEP
     { IDefns ([],lhs(parseState)) }
  
hashDirective:
  | HASH IDENT hashDirectiveArgs                            
     { HashDirective ($2,$3,lhs(parseState)) }

hashDirectiveArg: 
  | STRING 
     { $1 } 

hashDirectiveArgs: 
  |    
     { [] } 
  | hashDirectiveArgs hashDirectiveArg 
     { $1 @ [$2] }

interactiveTerminator: 
  | SEMICOLON_SEMICOLON {}
  | EOF     {}

/* Represents the sequence of items swallowed in one gulp by F# Interactive */
/* It is important to make this as large as possible given the chunk of input */
/* text. More or less identical to 'moduleDefns' but where SEMICOLON_SEMICOLON is */
/* not part of the grammar of topSeps and HASH interactions are not part of */
/* the swalloed blob, since things like #use must be processed separately. */
/* REVIEW: limiting the input chunks until the next # directive can lead to */ 
/* discrepencies between whole-file type checking in FSI and FSC. */

interactiveItemsTerminator:
  /* Always ends on interactiveTerminator */
  | interactiveTerminator  { [] }
  | interactiveModuleDefns interactiveTerminator { $1 }
  | interactiveExpr        interactiveTerminator { $1 }
  | interactiveHash        interactiveTerminator { $1 }
  | interactiveModuleDefns itop_seps interactiveItemsTerminator { $1 @ $3 }
  | interactiveExpr        itop_seps interactiveItemsTerminator { $1 @ $3 }
  | interactiveHash        itop_seps interactiveItemsTerminator { $1 @ $3 }

interactiveModuleDefns:
  /* One or more moduleDefn. REVIEW: "moduleDefns" logical name, but that is used already */
  | moduleDefn                        { $1 }
  | moduleDefn interactiveModuleDefns { $1 @ $2 }

interactiveExpr:
  | opt_attributes opt_decl_visibility declExpr
      { if isSome $2 then errorR(Error("Visibility declarations are not permitted here",rhs parseState 3));
        let attrDecls = if nonNil $1 then [ Def_attributes ($1,rhs parseState 1) ] else [] in 
        attrDecls @ [ mk_Def_expr($3,rhs parseState 3)] }

interactiveHash:      
  | hashDirective { [Def_hash($1,rhs parseState 1)] }
      
/* F# Language Proper */

signatureFile: 
  | fileNamespaceSpecs EOF 
     { checkEndOfFileError $2; $1 }
  | fileNamespaceSpecs error EOF 
     { $1 }

  /* If this rule fires it is kind of catastrophic: error recovery yields no results! */
  /* This will result in NO intellisense for the file! Ideally we wouldn't need this rule */
  /* Note: the compiler assumes there is at least one "fragment", so an empty one is used (see 4488) */
  | error EOF 
     { let emptySigFileFrag = AnonTopModuleSpec([],rhs parseState 1) in 
       ParsedSigFile([],[emptySigFileFrag]) }     

implementationFile: 
  | fileNamespaceImpls EOF 
     { checkEndOfFileError $2; $1 }
  | fileNamespaceImpls error EOF 
     { $1 }

  /* If this rule fires it is kind of catastrophic: error recovery yields no results! */
  /* This will result in NO intellisense for the file! Ideally we wouldn't need this rule */
  /* Note: the compiler assumes there is at least one "fragment", so an empty one is used (see 4488) */
  | error EOF 
     { let emptyImplFileFrag = AnonTopModuleImpl([],rhs parseState 1) in 
       ParsedImplFile([],[emptyImplFileFrag]) }

moduleIntro: 
  | MODULE opt_access path { $3,true,grabXmlDoc(parseState,1),$2 }

namespaceIntro: 
  | NAMESPACE path { $2,false,grabXmlDoc(parseState,1)  }

fileNamespaceSpecs: 
  | fileModuleSpec  
      { ParsedSigFile([],[ ($1 ([],emptyPreXmlDoc)) ]) }
  | fileModuleSpec  fileNamespaceSpecList 
      { (* If there are namespaces, the first fileModuleImpl may only contain # directives *)
        let decls = 
            match ($1 ([],emptyPreXmlDoc)) with 
            | AnonTopModuleSpec(decls,m) -> decls  
            | AnonNamespaceFragmentSpec(_,_, decls, _,_,_) -> decls 
            | NamedTopModuleSpec(ModuleOrNamespaceSpec(_,_,_,_,_,_,m)) ->
                raiseParseErrorAt m "only '#' compiler directives may occur prior to the first 'namespace' declaration" in
        let decls = 
            decls |> List.collect (function 
                | (Spec_hash (hd,_)) -> [hd]
                | d ->  
                     reportParseErrorAt (range_of_synspec d) "only '#' compiler directives may occur prior to the first 'namespace' declaration";
                     []) in
        ParsedSigFile(decls, $2) } 

fileNamespaceSpecList: 
  | fileNamespaceSpec fileNamespaceSpecList { $1 :: $2 }
  | fileNamespaceSpec { [$1] }

fileNamespaceSpec: 
  | opt_attributes namespaceIntro deprecated_opt_equals fileModuleSpec 
     { let path,_,xml = $2 in ($4 (path,xml)) }

fileNamespaceImpls: 
  | fileModuleImpl  
      { ParsedImplFile([], [ ($1 ([],emptyPreXmlDoc)) ]) }
  | fileModuleImpl fileNamespaceImplList 
      { (* If there are namespaces, the first fileModuleImpl may only contain # directives *)
        let decls = 
            match ($1 ([],emptyPreXmlDoc)) with 
            | AnonTopModuleImpl(decls,m) -> decls  
            | AnonNamespaceFragmentImpl(_,_, decls, _,_,_) -> decls 
            | NamedTopModuleImpl(ModuleOrNamespaceImpl(_,_,_,_,_,_,m)) ->
                raiseParseErrorAt m "only '#' compiler directives may occur prior to the first 'namespace' declaration" in
        let decls = 
            decls |> List.collect (function 
                | (Def_hash (hd,_)) -> [hd]
                | d ->  
                     reportParseErrorAt (range_of_syndecl d) "only '#' compiler directives may occur prior to the first 'namespace' declaration";
                     []) in
        ParsedImplFile(decls, $2) } 


fileNamespaceImplList: 
  | fileNamespaceImpl fileNamespaceImplList { $1 :: $2 }
  | fileNamespaceImpl { [$1] }

fileNamespaceImpl: 
  | opt_attributes namespaceIntro deprecated_opt_equals fileModuleImpl 
     { let path,_,xml = $2 in ($4 (path,xml)) }

fileModuleSpec: 
  | opt_attributes opt_decl_visibility  moduleIntro moduleSpfnsPossiblyEmpty 
    { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
      let m = (rhs2 parseState 3 4) in
      (fun (path,_) -> 
        let path2,_,xml,vis = $3 in 
        let lid = path@path2 in 
        NamedTopModuleSpec(ModuleOrNamespaceSpec(lid,true, $4, xml,$1,vis,m)))  }
  | moduleSpfnsPossiblyEmpty 
    { let m = (rhs parseState 1) in 
      (fun (path,xml) -> 
        match path with 
        | [] -> AnonTopModuleSpec($1, m)  
        | _ -> AnonNamespaceFragmentSpec(path,false, $1, xml,[],m))  } 

fileModuleImpl: 
  | opt_attributes opt_decl_visibility moduleIntro moduleDefnsOrExprPossiblyEmpty
    { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
      let m = (rhs2 parseState 3 4) in 
      (fun (path,_) -> 
        let path2,isModule,xml,vis = $3 in 
        let lid = path@path2 in 
        NamedTopModuleImpl(ModuleOrNamespaceImpl(lid,isModule, $4, xml,$1,vis,m))) }
  | moduleDefnsOrExprPossiblyEmpty 
    { let m = (rhs parseState 1) in 
      (fun (path,xml) -> 
        match path with 
        | [] -> AnonTopModuleImpl($1,m)  
        | _ -> AnonNamespaceFragmentImpl(path,false, $1, xml,[],m)) } 

moduleSpfnsPossiblyEmpty: 
  | moduleSpfns
      { $1 }
  | error
      { [] }
  | 
      { [] }
      
moduleSpfns: 
  | moduleSpfn  opt_top_seps moduleSpfns 
      { $1 :: $3 } 
  | error top_seps moduleSpfns 
      { (* silent recovery *) $3 }
  | moduleSpfn  opt_top_seps 
      { [$1] } 


moduleDefnsOrExprPossiblyEmpty:
  | moduleDefnsOrExpr
     { $1 }
  | 
     { [] }

/* A naked expression is only allowed at the start of a module/file, or straight after a top_seps */
moduleDefnsOrExpr:
  | opt_attributes opt_decl_visibility declExpr top_seps moduleDefnsOrExpr 
      { if isSome $2 then errorR(Error("Visibility declarations are not permitted here",rhs parseState 3));
        let attrDecls = if nonNil $1 then [ Def_attributes ($1,rhs parseState 1) ] else [] in 
        attrDecls @ mk_Def_expr ($3,rhs parseState 3) :: $5 }
  | opt_attributes opt_decl_visibility declExpr top_seps
      { if isSome $2 then errorR(Error("Visibility declarations are not permitted here",rhs parseState 3));
        let attrDecls = if nonNil $1 then [ Def_attributes ($1,rhs parseState 1) ] else [] in 
        attrDecls @ [ mk_Def_expr($3,rhs parseState 3) ] }
  | opt_attributes opt_decl_visibility declExpr
      { if isSome $2 then errorR(Error("Visibility declarations are not permitted here",rhs parseState 3));
        let attrDecls = if nonNil $1 then [ Def_attributes ($1,rhs parseState 1) ] else [] in 
        attrDecls @ [ mk_Def_expr($3,rhs parseState 3) ] }
  | moduleDefns 
      { $1 } 
  | error
     { [] }

moduleDefns:
  | moduleDefnOrDirective moduleDefns 
      {  $1 @ $2 } 
  | moduleDefnOrDirective top_seps moduleDefnsOrExpr 
      {  $1 @ $3 } 
  | moduleDefnOrDirective
      { $1 }
  | moduleDefnOrDirective top_seps
      { $1 }
  | error top_seps moduleDefnsOrExpr 
      {  $3 } 

moduleDefnOrDirective:
  | moduleDefn 
      {  $1  } 
  | hashDirective 
      { [ Def_hash ($1,rhs2 parseState 1 1) ] } 
  /* Recover whenever an error occurs in a moduleDefn */



/* This is used by both "fsi" interactions and "source file" fragments defined by moduleDefns */
moduleDefn:

  | opt_attributes opt_decl_visibility defnBindings                   %prec decl_let 
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        new_arg_uniq_ref := 0;
        mkDefnBindings (rhs parseState 3,$3,$1,$2,rhs parseState 3)  }

  | opt_attributes opt_decl_visibility hardwhiteLetBindings          %prec decl_let 
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        new_arg_uniq_ref := 0;
        mkDefnBindings (rhs parseState 3,$3,$1,$2,rhs parseState 3)  }

  | opt_attributes opt_decl_visibility doBinding %prec decl_let 
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        let letm = rhs parseState 3 in 
        mkDefnBindings (letm,$3,$1,$2,rhs parseState 3) }
  
  | opt_attributes opt_decl_visibility TYPE tyconDefn tyconDefnList
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        let      (TyconDefn(ComponentInfo(cas   ,kind     ,a,cs,b,c,d,d2,d3),e,f,g)) = $4 in
        let tc = (TyconDefn(ComponentInfo($1@cas,TMK_Tycon,a,cs,b,c,d,d2,d3),e,f,g)) in
        [ Def_tycons(tc :: $5,rhs2 parseState 3 5) ] }

  | opt_attributes opt_decl_visibility TYPE typeNameInfo tyconDefnAugmentation
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        if nonNil $1 then warning(Error("Attributes on augmentations are ignored, they must be placed on the original declaration",rhs parseState 1));
        [ Def_partial_tycon($4,$5,rhs2 parseState 3 5) ] }

  | opt_attributes opt_decl_visibility exconDefn
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        let (ExconDefn(ExconCore(cas,a,b,c,d,d2),e,f)) = $3 in 
        let ec = (ExconDefn(ExconCore($1@cas,a,b,c,d,d2),e,f)) in 
        [ Def_exn(ec, rhs2 parseState 3 3) ] }

  | opt_attributes opt_decl_visibility moduleIntro opt_signature EQUALS  namedModuleDefnBlock

      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        let attribs,(path,isModule,xml,vis),mty = $1,$3,$4 in 
        if not isModule          then raiseParseErrorAt (rhs parseState 3) "namespaces must be declared at the head of a file";
        match $6 with 
        | Choice1Of2 eqn -> 
            if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
            if isSome mty            then raiseParseErrorAt (rhs parseState 3) "a module abbreviation may not be given a constraint";
            if List.length path <> 1 then raiseParseErrorAt (rhs parseState 3) "a module abbreviation must be a simple name, not a path";
            if List.length $1 <> 0   then raiseParseErrorAt (rhs parseState 1) "ignorning attributes on module abbreviation";
            if isSome vis            then raiseParseErrorAt (rhs parseState 1) "ignorning accessibility attribute on module abbreviation. Module abbreviations are always private";
            [ Def_module_abbrev(List.hd path,eqn,rhs2 parseState 3 6) ]
        | Choice2Of2 def -> 
            if List.length path <> 1 then raiseParseErrorAt (rhs parseState 3) "a module definition must be a simple name, not a path";
            let info = ComponentInfo(attribs,TMK_Module,[],[],path,xml,false,vis,rhs parseState 3) in
            [ Def_module(info,def,mty,rhs2 parseState 3 6) ] }

  | openDecl 
      { [Def_open($1,rhs parseState 1)] }

/* this occurs on the right of a module abbreviation (#light encloses the r.h.s. with OBLOCKBEGIN/OBLOCKEND) */
/* We don't use it in signature files */
namedModuleAbbrevBlock:
  | OBLOCKBEGIN path OBLOCKEND 
       { $2 }
  | path 
       { $1 }
       
namedModuleDefnBlock:
  | OBLOCKBEGIN wrappedNamedModuleDefn OBLOCKEND 
       { 
         Choice2Of2 $2 
       }
  | OBLOCKBEGIN moduleDefnsOrExpr OBLOCKEND 
       { // BUG 2644 FSharp 1.0: 
         // There is an ambiguity here 
         // In particular, consider the following two:

         // module M2 = 
         //    System.DateTime.Now
         // module M2 = 
         //    Microsoft.FSharp.Core.List
         // The second is a module abbreviation , the first a module containing a single expression.
         // This is a bit unfortunate. For F# v1 the resolution is in favour of 
         // the module abbreviation, i.e. anything of the form 
         //    module M2 = ID.ID.ID.ID
         // will be taken as a module abbreviation, regardles of the identifiers themselves.
         // In a later version (Dev11) we could actually try resolving the names 
         // to both expressions and module identifiers and base the resolution of that semantic lookup
         //
         // This is similar to the ambiguitty between 
         //    type X = int
         // and 
         //    type X = OneValue
         // However in that case we do use type name lookup to make the resolution.

         match $2 with 
         | [ Def_expr (_,Expr_lid_or_id_get(false,path,_),_) ] -> 
             Choice1Of2  path
         | _ -> 
             Choice2Of2 $2 
       }
  | OBLOCKBEGIN moduleDefnsOrExpr recover 
       { reportParseErrorAt (rhs parseState 1) "unclosed block in #light syntax";   
         Choice2Of2 $2 
       }
  | OBLOCKBEGIN error OBLOCKEND                
       { 
         Choice2Of2 [] 
       }
  | wrappedNamedModuleDefn 
       {
         Choice2Of2 $1 
        }
  | path 
        {
         Choice1Of2 $1 
        }

wrappedNamedModuleDefn:
  | structOrBegin moduleDefnsOrExprPossiblyEmpty END 
       { $2 }
  | structOrBegin moduleDefnsOrExprPossiblyEmpty recover 
       { reportParseErrorAt (rhs parseState 1) "unmatched 'begin' or 'struct'";  
         $2 }
  | structOrBegin error END                      
       { [] }

opt_signature :
  | 
       { None }
  | COLON moduleSpecBlock
       { deprecatedWithError "Signature types must be given in a .fsi or .mli file" (lhs(parseState));
         Some(Sign_explicit($2)) }
  | COLON path
       { deprecatedWithError "Signature types must be given in a .fsi or .mli file" (lhs(parseState));
         Some(Sign_named($2)) }

tyconDefnAugmentation: 
  | WITH classDefnBlock decl_end
     { $2 }
/* opt_sig: { None } | COLON sigOrBegin moduleSpfns END { $3 } */

moduleSpfn: 
  | hashDirective 
      { Spec_hash ($1,rhs2 parseState 1 1)  } 
  | valSpfn 
      { $1 }

  | opt_attributes opt_decl_visibility moduleIntro colonOrEquals namedModuleAbbrevBlock 
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        let path,isModule,xml,vis = $3 in 
        if not isModule          then raiseParseErrorAt (rhs parseState 3) "namespaces must be declared at the head of a file";
        if List.length path <> 1 then raiseParseErrorAt (rhs parseState 3) "a module abbreviation must be a simple name, not a path";
        if List.length $1 <> 0   then raiseParseErrorAt (rhs parseState 1) "ignorning attributes on module abbreviation";
        if isSome(vis)           then raiseParseErrorAt (rhs parseState 1) "ignorning visibility attribute on module abbreviation. Module abbreviations are always private";
        Spec_module_abbrev(List.hd path,$5,rhs2 parseState 3 5) } 

  | opt_attributes opt_decl_visibility  moduleIntro colonOrEquals moduleSpecBlock
      { let path,isModule,xml,vis = $3 in 
        if not isModule          then raiseParseErrorAt (rhs parseState 3) "namespaces must be declared at the head of a file";
        if List.length path <> 1 then raiseParseErrorAt (rhs parseState 3) "a module moduleDefn must be a simple name, not a path";
        let info = ComponentInfo($1,TMK_Module,[],[],path,xml,false,vis,rhs parseState 3) in
        if isSome($2) then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        Spec_module(info,$5,rhs2 parseState 3 5) }

  | opt_attributes opt_decl_visibility  tyconSpfns 
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        let (TyconSpfn(ComponentInfo(cas,k,a,cs,b,c,d,d2,d3),e,f,g)),rest = 
           match $3 with
           | [] -> raiseParseErrorAt (rhs parseState 3) "Unexpected empty type moduleDefn list"
           | h::t -> h,t in 
        let tc = (TyconSpfn(ComponentInfo($1@cas,k,a,cs,b,c,d,d2,d3),e,f,g))in 
        Spec_tycon (tc::rest,rhs parseState 3) } 

  | opt_attributes opt_decl_visibility exconSpfn
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        let (ExconSpfn(ExconCore(cas,a,b,c,d,d2),e,f)) = $3 in 
        let ec = (ExconSpfn(ExconCore($1@cas,a,b,c,d,d2),e,f)) in 
        Spec_exn(ec, rhs parseState 3) }

  | OPEN path { Spec_open ($2, rhs2 parseState 1 2) }

valSpfn: 
  | opt_attributes opt_decl_visibility VAL opt_attributes opt_inline opt_mutable opt_access nameop opt_explicitValTyparDecls COLON topTypeWithTypeConstraints opt_literalValue
      { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        (let attr1,attr2,inlineFlag,mutableFlag,vis2,id,doc,explicitValTyparDecls,(ty,arity),konst = ($1),($4),($5),($6),($7),($8),grabXmlDoc(parseState,3),($9),($11),($12) in 
        if nonNil attr2 then errorR(Deprecated("Attributes should be placed before 'val'",rhs parseState 4));
        let m = rhs2 parseState 3 11 in 
        let valSpfn = ValSpfn((attr1@attr2),id,explicitValTyparDecls,ty,arity,inlineFlag,mutableFlag,doc, vis2,konst,m) in 
        Spec_val(valSpfn,m))
      }

opt_literalValue: 
  | { None }
  | EQUALS declExpr { Some($2) }
  
  
moduleSpecBlock: 
  | OBLOCKBEGIN            moduleSpfns     OBLOCKEND { $2 }
  | OBLOCKBEGIN sigOrBegin moduleSpfnsPossiblyEmpty END OBLOCKEND { $3 }
  |             sigOrBegin moduleSpfnsPossiblyEmpty END { $2 }

opt_attributes:
  | attributes                                { $1 }
  |            %prec prec_opt_attributes_none { [] }

attributes: 
  | attributeList                     
     { $1 }
  | attributeList attributes
     { $1 @ $2 }

attributeList: 
  | LBRACK_LESS  attributeListElements opt_seps GREATER_RBRACK opt_OBLOCKSEP {  MatchPair parseState 1 4; $2 }

attributeListElements: 
  | attribute                     
     { [$1] }
  | attributeListElements seps attribute 
     { $1 @ [$3] }

attribute:
  | path opt_HIGH_PRECEDENCE_APP opt_atomicExprAfterType 
     { let arg = match $3 with None -> mksyn_unit (range_of_lid $1) | Some e -> e in 
       Attr($1,arg,None,range_of_lid $1) }
  | attributeTarget COLON path opt_HIGH_PRECEDENCE_APP opt_atomicExprAfterType 
     { let arg = match $5 with None -> mksyn_unit (range_of_lid $3) | Some e -> e in 
       Attr($3,arg,Some $1,range_of_lid $3) }

attributeTarget: 
  | MODULE { ident("module",lhs(parseState)) } 
  | TYPE { ident("type",lhs(parseState)) } 
  | ident { $1 } 
  | YIELD /* return */ { if $1 then reportParseErrorAt (rhs parseState 1) "syntax error";
                         ident("return",lhs(parseState)) } 


tyconSpfns:      
  | TYPE tyconSpfn_list 
     { $2 }

tyconSpfn_list:  
  | tyconSpfn AND tyconSpfn_list 
     { $1 :: $3 } 
  | tyconSpfn 
     { [$1] }

tyconSpfn: 
  | typeNameInfo  EQUALS tyconSpfnRhsBlock 
      { $3 $1 }
  | typeNameInfo  opt_classSpfn       
      { TyconSpfn($1,TyconSpfnRepr_simple (TyconCore_no_repr (lhs(parseState)),lhs(parseState)),$2,lhs(parseState)) }

tyconSpfnRhsBlock: 
  /* This rule allows members to be given for record and union types in the #light syntax */
  /* without the use of 'with' ... 'end'. For example: */
  /*     type R = */
  /*         { a : int } */
  /*         member r.A = a */
  /* It also takes into account that any existing 'with' */
  /* block still needs to be considered and may occur indented or undented from the core type */
  /* representation. */
  | OBLOCKBEGIN  tyconSpfnRhs opt_OBLOCKSEP classSpfnMembers opt_classSpfn OBLOCKEND opt_classSpfn  
     { let m = lhs(parseState) in 
       (fun nameInfo -> 
           $2 nameInfo (checkForMultipleAugmentations m ($4 @ $5) $7)) }
  | tyconSpfnRhs opt_classSpfn
     { let m = lhs(parseState) in 
       (fun nameInfo -> 
           $1 nameInfo $2) }

tyconSpfnRhs: 
  | tyconDefnOrSpfnSimpleRepr 
     { let m = lhs(parseState) in 
       (fun nameInfo augmentation -> 
           TyconSpfn(nameInfo,TyconSpfnRepr_simple ($1,m),augmentation,m)) }
  | tyconClassSpfn 
     { let m = lhs(parseState) in 
       (fun nameInfo augmentation -> 
           TyconSpfn(nameInfo,TyconSpfnRepr_class (fst $1,snd $1,m),augmentation,m)) }
  | DELEGATE OF topType
     { let m = lhs(parseState) in 
       let ty,arity = $3 in
       let invoke = ClassMemberSpfn_binding(ValSpfn([],mksyn_id m "Invoke",inferredTyparDecls,ty,arity,false,false,emptyPreXmlDoc,None,None,m),AbstractMemberFlags None MemberKindMember,m) in 
       (fun nameInfo augmentation -> 
           if nonNil augmentation then raiseParseErrorAt m "augmentations are not permitted on delegate type moduleDefns";
           TyconSpfn(nameInfo,TyconSpfnRepr_class (TyconDelegate (ty,arity),[invoke],m),[],m)) }

tyconClassSpfn: 
  | classSpfnBlockKindUnspecified
     { (TyconUnspecified, $1) }
  | classOrInterfaceOrStruct classSpfnBlock END
     { ($1,$2) }
  | classOrInterfaceOrStruct classSpfnBlock recover 
     { reportParseErrorAt (rhs parseState 1) "unmatched 'class', 'interface' or 'struct'";
       ($1,$2) }
  | classOrInterfaceOrStruct error END
     { (* silent recovery *) ($1,[]) }

classSpfnBlockKindUnspecified:
  | OBLOCKBEGIN  classSpfnMembers OBLOCKEND 
     { $2 }
  | OBLOCKBEGIN  classSpfnMembers recover
     { $2 }
/* NOTE: these rules enable a 'heavy' syntax to omit the kind of a type. */
  | BEGIN  classSpfnBlock END 
     { $2 }
  | BEGIN  classSpfnBlock recover
     { $2 }



classSpfnBlock:
  | OBLOCKBEGIN  classSpfnMembers OBLOCKEND { $2 }
  | OBLOCKBEGIN  classSpfnMembers recover { $2 }
  | classSpfnMembers { $1 }

classSpfnMembers:  
  | classMemberSpfn opt_seps classSpfnMembers 
     { $1 :: $3 } 
  |  
     { []  }

memberFlags: 
  /* | STATIC          { StaticMemberFlags } */
  | STATIC MEMBER   { StaticMemberFlags }  
  | MEMBER          { NonVirtualMemberFlags }
  | METHOD         { raiseParseErrorAt (rhs parseState 1) "use 'member x.MyMethod(arg) = ...' to declare a new method" }
  | VIRTUAL         { raiseParseErrorAt (rhs parseState 1) "use 'abstract' to declare a new virtual method slot, and 'default' or 'override' to specify the default implemenation for that slot" }
  | OVERRIDE        { OverrideMemberFlags }
  | DEFAULT        { OverrideMemberFlags }

memberSpecFlags: 
  | memberFlags { $1 }  
  | ABSTRACT        { AbstractMemberFlags }
  | ABSTRACT MEMBER { AbstractMemberFlags }

classMemberSpfnGetSet:
  | /* EMPTY */ 
    { (fun arity -> (match arity with ValSynInfo([],_) -> MemberKindPropertyGet | _ -> MemberKindMember)) }
  | WITH classMemberSpfnGetSetElements 
    { (fun arity -> $2) }
  | OWITH classMemberSpfnGetSetElements OEND
    { (fun arity -> $2) }
  | OWITH classMemberSpfnGetSetElements error
    {  reportParseErrorAt (rhs parseState 1) "unmatched 'with' or badly formatted 'with' block";
       (fun arity -> $2) }


classMemberSpfnGetSetElements:
  | nameop 
    { (let (id:ident) = $1 in 
       if id.idText = "get" then MemberKindPropertyGet 
       else if id.idText = "set" then MemberKindPropertySet 
       else raiseParseErrorAt (rhs parseState 1) "'get', 'set' or 'get,set' required") }
  | nameop COMMA nameop
    { let (id:ident) = $1 in 
      if not ((id.idText = "get" && $3.idText = "set") or 
              (id.idText = "set" && $3.idText = "get")) then 
         raiseParseErrorAt (rhs2 parseState 1 3) "'get', 'set' or 'get,set' required";
      MemberKindPropertyGetSet }

classMemberSpfn:
  | opt_attributes opt_decl_visibility memberSpecFlags opt_inline opt_access nameop opt_explicitValTyparDecls COLON topTypeWithTypeConstraints classMemberSpfnGetSet opt_literalValue
     { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
       let inlineFlag,doc,vis2,id,explicitValTyparDecls,(ty,arity),optLiteralValue = $4,grabXmlDoc(parseState,3),$5,$6,$7,$9,$11 in
       let m = rhs2 parseState 3 10 in 
       let valSpfn = ValSpfn($1,id,explicitValTyparDecls,ty,arity, inlineFlag,false,doc, vis2,optLiteralValue,m) in 
       ClassMemberSpfn_binding(valSpfn, $3 (computeOverloadQualifier $1) ($10 arity),m) }
  | opt_attributes opt_decl_visibility interfaceMember appType  
     { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
       ClassMemberSpfn_interface ($4,union_ranges (rhs parseState 3) (range_of_syntype $4)) }
  | opt_attributes opt_decl_visibility INHERIT appType 
     { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
       ClassMemberSpfn_inherit ($4,union_ranges (rhs parseState 3) (range_of_syntype $4)) }
  | opt_attributes opt_decl_visibility VAL fieldDecl 
     { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
       let fld = $4 $1 false  in
       ClassMemberSpfn_field(fld,rhs2 parseState 3 4) }
  | opt_attributes opt_decl_visibility STATIC VAL fieldDecl 
     { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
       ClassMemberSpfn_field($5 $1 true,rhs2 parseState 3 5) }
  | opt_attributes  opt_decl_visibility STATIC TYPE tyconSpfn 
     { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
       ClassMemberSpfn_tycon($5,rhs2 parseState 3 5) }
  | opt_attributes opt_decl_visibility NEW COLON topTypeWithTypeConstraints  
     { let vis,doc,(ty,arity) = $2,grabXmlDoc(parseState,3),$5 in 
       let m = union_ranges (rhs parseState 3) (range_of_syntype ty) in
       let inlineFlag = false in
       let valSpfn = ValSpfn($1,mksyn_id (rhs parseState 3) "new",noInferredTypars,ty,arity,inlineFlag,false, doc, vis,None,m) in
       ClassMemberSpfn_binding(valSpfn, CtorMemberFlags  (computeOverloadQualifier $1),m) }

typeNameInfo: 
  | opt_attributes tyconNameAndTyparDecls opt_typeConstraints
     { let typars,lid,fixity,tpcs1,vis,xmlDoc = $2 in 
       let tpcs2 = $3 in 
       ComponentInfo($1,TMK_Tycon,typars,(tpcs1 @ tpcs2),lid,xmlDoc,fixity,vis,range_of_lid lid)  }

tyconDefnList:  
  | AND tyconDefn tyconDefnList 
     { $2 :: $3 } 
  |                             
     { [] }

tyconDefn: 
  | typeNameInfo 
     { TyconDefn($1,TyconDefnRepr_simple(TyconCore_no_repr(lhs(parseState)),lhs(parseState)),[],lhs(parseState)) }
  | typeNameInfo EQUALS tyconDefnRhsBlock
     { let tcDefRepr,members = $3 in
       TyconDefn($1,tcDefRepr,members,lhs(parseState)) }
  | typeNameInfo opt_attributes opt_decl_visibility opt_HIGH_PRECEDENCE_APP  simplePatterns optAsSpec EQUALS tyconDefnRhsBlock
     { let vis,spats, az,(tcDefRepr,members) = $3,$5,$6,$8 in
       let (ComponentInfo(_,_,_,_,lid,_,_,_,_)) = $1 in 
       let memberCtorPattern = ClassMemberDefn_implicit_ctor (vis,$2,spats,az,range_of_lid lid) in
       let tcDefRepr = 
         match tcDefRepr with
         | TyconDefnRepr_class (k,cspec,m) -> TyconDefnRepr_class (k,memberCtorPattern::cspec,m)
         | _ -> reportParseErrorAt (rhs2 parseState 1 5) "Only class types may take value arguments"; tcDefRepr
       in
       TyconDefn($1,tcDefRepr,members,lhs(parseState))  }

tyconDefnRhsBlock: 
  /* This rule allows members to be given for record and union types in the #light syntax */
  /* without the use of 'with' ... 'end'. For example: */
  /*     type R = */
  /*         { a : int } */
  /*         member r.A = a */
  /* It also takes into account that any existing 'with' */
  /* block still needs to be considered and may occur indented or undented from the core type */
  /* representation. */
  | OBLOCKBEGIN  tyconDefnRhs opt_OBLOCKSEP classDefnMembers opt_classDefn OBLOCKEND opt_classDefn  
     { let m = lhs(parseState) in 
       $2 (checkForMultipleAugmentations m ($4 @ $5) $7) }
  | tyconDefnRhs opt_classDefn
     { let m = lhs(parseState) in 
       $1 $2 }

tyconDefnRhs: 
  | tyconDefnOrSpfnSimpleRepr 
     { let m = lhs(parseState) in (fun augmentation -> TyconDefnRepr_simple ($1,m),augmentation) }
  | tyconClassDefn 
     { let m = lhs(parseState) in (fun augmentation -> TyconDefnRepr_class (fst $1,snd $1,m),augmentation) }
  | DELEGATE OF topType
     { let m = lhs(parseState) in 
       let ty,arity = $3 in
       (fun augmentation -> 
           let valSpfn = ValSpfn([],mksyn_id m "Invoke",inferredTyparDecls,ty,arity,false,false,emptyPreXmlDoc,None,None,m) in 
           let invoke = ClassMemberDefn_slotsig(valSpfn,AbstractMemberFlags None MemberKindMember,m) in 
           if nonNil augmentation then raiseParseErrorAt m "augmentations are not permitted on delegate type moduleDefns";
           TyconDefnRepr_class (TyconDelegate (ty,arity),[invoke],m),[]) }

tyconClassDefn: 
  | classDefnBlockKindUnspecified
     { (TyconUnspecified, $1) }
  | classOrInterfaceOrStruct classDefnBlock END 
     { ($1,$2) }
  | classOrInterfaceOrStruct classDefnBlock recover 
     { reportParseErrorAt (rhs parseState 1) "unmatched 'class', 'interface' or 'struct'";
       ($1,$2) }
  | classOrInterfaceOrStruct error END
     { (* silent recovery *) ($1,[]) }

classDefnBlockKindUnspecified:
  | OBLOCKBEGIN  classDefnMembers recover
     { (* silent recovery *) $2 }
  | OBLOCKBEGIN  classDefnMembers OBLOCKEND 
     { $2 }
/* NOTE: these rules enable a 'heavy' syntax to omit the kind of a type. However this doesn't seem necessary to support.
  NOTE: that is 'type kind inference' is only supported for #light 
  | BEGIN  classDefnBlock END 
     { $2 }
  | BEGIN  classDefnBlock recover 
     { reportParseErrorAt (rhs parseState 1) "unmatched 'begin'";
       $2 }
  | BEGIN  error END 
     { (* silent recovery *) [] }
*/

classDefnBlock:
  | OBLOCKBEGIN  classDefnMembers recover { (* silent recovery *) $2 }
  | OBLOCKBEGIN  classDefnMembers OBLOCKEND { $2 }
  | classDefnMembers { $1 }
  
classDefnMembers:  
  | classDefnMember opt_seps classDefnMembers 
     { $1 @  $3 }
  /* REVIEW: Error recovery rules that are followed by potentially empty productions are suspicious! */
  | error classDefnMembers 
     { $2 }
  | 
     { [] }

classDefnMemberGetSet: 
  | WITH classDefnMemberGetSetElements
     { $2  }
  | OWITH classDefnMemberGetSetElements OEND
     { $2  }
  | OWITH classDefnMemberGetSetElements error
     { reportParseErrorAt (rhs parseState 1) "unmatched 'with' or badly formatted 'with' block";
       $2  }

classDefnMemberGetSetElements: 
  | classDefnMemberGetSetElement 
     { [$1]  }
  | classDefnMemberGetSetElement AND classDefnMemberGetSetElement
     { [$1;$3] }

classDefnMemberGetSetElement: 
  | opt_inline bindingPattern opt_topReturnTypeWithTypeConstraints EQUALS typedSeqExprBlock 
     { let rhsm = (range_of_synexpr $5) in 
       ($1,$2,$3,$5,rhsm) }

memberCore:  
 /* methods and simple getter properties */
  | opt_inline bindingPattern  opt_topReturnTypeWithTypeConstraints EQUALS typedSeqExprBlock  
     {  let rhsm = (range_of_synexpr $5) in 
        let wholem = union_ranges (rhs2 parseState 3 4) rhsm in 
        let mpat = rhs parseState 2 in 
        let optReturnType = $3 in 
        let bindingBuilder,bindm = $2 in 
        (fun vis memFlagsBuilder attrs -> 
             [ ClassMemberDefn_member_binding (bindingBuilder (vis,$1,false,bindm,NoSequencePointAtInvisibleBinding,wholem,optReturnType,$5,rhsm,[],attrs,Some(memFlagsBuilder (computeOverloadQualifier attrs) MemberKindMember)),bindm) ]) }

 /* properties with explicit get/set, also indexer properties */
  | opt_inline bindingPattern  opt_topReturnTypeWithTypeConstraints classDefnMemberGetSet  
     { let wholem = rhs2 parseState 2 4 in 
       let propertyNameBindingBuilder,_ = $2 in 
       let optPropertyType = $3 in 
       let mutableFlag = false in
       (fun visNoLongerUsed memFlagsBuilder attrs -> 
             $4 |> List.map (fun (optInline,(bindingBuilder,bindm),optReturnType,expr,exprm) ->
                   let optInline = $1 || optInline in 
                   let overloadQualifier =  (computeOverloadQualifier attrs) in 
                   
                   let binding = bindingBuilder (visNoLongerUsed,optInline,mutableFlag,bindm,NoSequencePointAtInvisibleBinding,wholem,optReturnType,expr,exprm,[],attrs,Some (memFlagsBuilder overloadQualifier MemberKindMember)) in
                   let (Binding (vis,_,pseudo,_,attrs,doc,valSynData,pv,_,bindm,spBind)) = binding in 
                   let memberKind = 
                         let getset = 
                               let rec go p = 
                                   match p with 
                                   | Pat_lid ([id],_,_,_,_) ->  id.idText
                                   | Pat_as (_,nm,_,_,_) ->  nm.idText
                                   | Pat_typed (p,_,_) ->  go p
                                   | Pat_attrib (p,_,_) ->  go p
                                  | _ -> raiseParseErrorAt bindm "invalid declaration syntax"  in
                               go pv in 
                         if getset = "get" then MemberKindPropertyGet 
                         else if getset = "set" then MemberKindPropertySet 
                         else raiseParseErrorAt bindm "get and/or set required" in


                   // REVIEW: It's hard not to ignore the optPropertyType type annotation for 'set' properties. To apply it, 
                   // we should apply it to the last argument, but at this point we've already pushed the patterns that 
                   // make up the arguments onto the RHS. So we just always give a warning. 

                   begin match optPropertyType with 
                   | Some _ -> errorR(Error("type annotations on property getters and setters must be given after the 'get()' or 'set(v)', e.g. 'with get() : string = ...'",bindm))
                   | None -> ()
                   end;
                   
                   let optReturnType = 
                       match (memberKind, optReturnType) with 
                       | MemberKindPropertySet,_ -> optReturnType
                       | _, None -> optPropertyType
                       | _ -> optReturnType in 

                   (* REDO with the correct member kind *)
                   let binding = bindingBuilder(vis,pseudo,mutableFlag,bindm,NoSequencePointAtInvisibleBinding,wholem,optReturnType,expr,exprm,[],attrs,Some(memFlagsBuilder overloadQualifier memberKind)) in 
                   let (Binding (vis,_,pseudo,_,attrs,doc,valSynData,pv,rhsAfterPats,bindm,spBind)) = binding in 
                
                   let (ValSynData(_,valSynInfo,_)) = valSynData  in

                   // Setters have all arguments tupled in their internal TAST form, though they don't appear to be tupled from the syntax
                   let memFlags = memFlagsBuilder overloadQualifier memberKind in
                   let valSynInfo = 
                       match memberKind, valSynInfo with 
                       | MemberKindPropertyGet,ValSynInfo ([],_)          when not memFlags.MemberIsInstance  -> raiseParseErrorAt bindm  "A getter property must at least have one argument, e.g. 'with get() = ...'"
                       | MemberKindPropertyGet,ValSynInfo ([thisArg],_)   when memFlags.MemberIsInstance      -> raiseParseErrorAt bindm  "A getter property must at least have one argument, e.g. 'with get() = ...'"

                       | MemberKindPropertySet,ValSynInfo (thisArg::indexAndValueArgs,ret) when     memFlags.MemberIsInstance -> ValSynInfo ([thisArg;List.concat indexAndValueArgs],ret)
                       | MemberKindPropertySet,ValSynInfo (indexAndValueArgs,ret)          when not memFlags.MemberIsInstance -> ValSynInfo ([List.concat indexAndValueArgs],ret)
                       | _ -> valSynInfo in

                   let valSynData = ValSynData(Some(memFlags), valSynInfo,None) in 

                   // Create the binding from the first lambda pattern in order to extract out the pattern of the 
                   // 'this' variable and put it into the pattern for the get/set binding, replacing the get/set part 
                   // A little gross. 
                   let pv',doc' = 
                       let bindingOuter = propertyNameBindingBuilder(vis,optInline,mutableFlag,bindm,spBind,bindm,optReturnType,expr,exprm,[],attrs,Some(memFlagsBuilder overloadQualifier MemberKindMember)) in
                       let (Binding (_,_,_,_,_,doc2,_,pvOuter,_,_,_)) = bindingOuter in 
                       
                   
                       let lidOuter,lidVisOuter = 
                           match pvOuter with 
                           | Pat_lid (lid,None,[],lidVisOuter,m) ->  lid,lidVisOuter
                           | p -> raiseParseErrorAt bindm "invalid declaration syntax"  in

                       // Merge the visibility from the outer point with the inner point, e.g.
                       //    member <VIS1>  this.Size with <VIS2> get ()      = m_size
                       
                       let mergeLidVisOuter lidVisInner =
                           match lidVisInner,lidVisOuter with 
                           | None,None -> None
                           | Some lidVisInner,None | None,Some lidVisInner -> Some lidVisInner
                           | Some _, Some _ ->  
                               errorR(Error("multiple accessibilities given for property getter or setter",bindm));
                               lidVisInner in
                   
                       // Replace the "get" or the "set" with the right name
                       let rec go p = 
                           match p with 
                           | Pat_lid ([id],tyargs,args,lidVisInner,m) ->  
                               // Setters have all arguments tupled in their internal form, though they don't 
                               // appear to be tupled from the syntax. Somewhat unfortunate
                               let args = 
                                   if id.idText = "set" then 
                                       match args with 
                                       | [Pat_paren(Pat_tuple (indexPats,_),indexPatRange);valuePat] when id.idText = "set" -> 
                                           [Pat_tuple(indexPats@[valuePat],union_ranges indexPatRange (range_of_synpat valuePat))] 
                                       | [indexPat;valuePat] -> 
                                           [Pat_tuple(args,union_ranges (range_of_synpat indexPat) (range_of_synpat valuePat))] 
                                       | [valuePat] -> 
                                           [valuePat] 
                                       | _ -> 
                                           raiseParseErrorAt m "property setters must be defined using 'set value = ', 'set idx value = ' or 'set (idx1,...,idxN) value = ... '" 
                                   else 
                                       args in
                               Pat_lid (lidOuter,tyargs,args,mergeLidVisOuter lidVisInner,m)
                           | Pat_as (_,nm,_,lidVisInner,m) ->  Pat_lid (lidOuter,None,[],mergeLidVisOuter lidVisInner,m)
                           | Pat_typed (p,ty,m) ->  Pat_typed(go p,ty,m)
                           | Pat_attrib (p,attribs,m) ->  Pat_attrib(go p,attribs,m)
                           | Pat_wild(m) ->  Pat_wild(m)
                           | _ -> raiseParseErrorAt bindm "invalid declaration syntax"  in

                       go pv,MergePreXmlDoc doc2 doc in

                   ClassMemberDefn_member_binding (Binding (vis,NormalBinding,pseudo,mutableFlag,attrs,doc',valSynData,pv',rhsAfterPats,bindm,spBind),bindm)))
       }

abstractMemberFlags: 
  | ABSTRACT {} 
  | ABSTRACT MEMBER {} 

classDefnMember:
  | opt_attributes opt_decl_visibility classDefnBindings
     { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
       [mkClassMemberLocalBindings(false,rhs2 parseState 3 3,$1,$2,$3)] }
       
  | opt_attributes opt_decl_visibility STATIC classDefnBindings  
     { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
       [mkClassMemberLocalBindings(true,rhs2 parseState 3 4,$1,$2,$4)] }
       
       
/*
  | openDecl 
      { [ClassMemberDefn_open($1,rhs parseState 1)] }
*/

  | opt_attributes opt_decl_visibility memberFlags memberCore  opt_ODECLEND
     { if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
       $4 $2 $3 $1 }
       
  | opt_attributes opt_decl_visibility interfaceMember appType opt_interfaceImplDefn  
     {  if nonNil $1 then errorR(Error("attributes are not permitted on interface implementations",rhs parseState 1));
        if isSome $2 then errorR(Error("interfaces always have the same visibility as the enclosing type",rhs parseState 3));
        [ ClassMemberDefn_interface ($4, $5,rhs2 parseState 3 5) ] }
        
  | opt_attributes opt_decl_visibility abstractMemberFlags opt_inline nameop opt_explicitValTyparDecls COLON topTypeWithTypeConstraints classMemberSpfnGetSet  opt_ODECLEND
     { let ty,arity = $8 in
       let inlineFlag,doc,id,explicitValTyparDecls = $4,grabXmlDoc(parseState,3),$5,$6 in
       let m = rhs2 parseState 3 9 in
       if isSome $2 then errorR(Error("Accessibility modifiers are not allowed on this member. Abstract slots always have the same visibility as the enclosing type",m));
       let valSpfn = ValSpfn($1,id,explicitValTyparDecls,ty,arity, inlineFlag,false,doc, None,None,m) in
       [ ClassMemberDefn_slotsig(valSpfn,AbstractMemberFlags (computeOverloadQualifier $1) ($9 arity), m) ] }
       
  | opt_attributes opt_decl_visibility inheritsDefn
     {  if nonNil $1 then errorR(Error("attributes are not permitted on 'inherit' declarations",rhs parseState 1));
        if isSome $2 then errorR(Error("Visibility declarations are not permitted on an 'inherits' declaration",rhs parseState 1));
        [ $3 ] }
        
  | opt_attributes opt_decl_visibility VAL fieldDecl 
     {  if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        [ ClassMemberDefn_field($4 $1 false,rhs2 parseState 3 4) ] }
        
  | opt_attributes opt_decl_visibility STATIC VAL fieldDecl 
     {  if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        [ ClassMemberDefn_field($5 $1 true,rhs2 parseState 3 5) ] }
        
  | opt_attributes opt_decl_visibility NEW  atomicPattern optAsSpec EQUALS typedSeqExprBlock opt_ODECLEND
     {  let m = union_ranges (rhs2 parseState 3 6) (range_of_synexpr $7) in 
        let expr = $7 in
        let valSynData = ValSynData (Some(CtorMemberFlags (computeOverloadQualifier $1)), ValSynInfo([SynInfo.InferArgSynInfoFromPat $4],SynInfo.unnamedRetVal), $5) in 
        let vis = $2 in 
        [ ClassMemberDefn_member_binding(Binding (None,NormalBinding,false,false,$1,grabXmlDoc(parseState,3),valSynData, Pat_lid ([mksyn_id (rhs parseState 3) "new"],Some noInferredTypars,[$4],vis,rhs parseState 3),BindingRhs([],None,expr),m,NoSequencePointAtInvisibleBinding),m) ] }
        
  | opt_attributes opt_decl_visibility STATIC TYPE tyconDefn 
     {  if isSome $2 then errorR(Error("Visibility declarations should come immediately prior to the identifier naming a construct",rhs parseState 2));
        [ ClassMemberDefn_tycon($5,None,rhs2 parseState 3 5) ] }
   
atomicPatternLongIdent:
  | pathop { (None,$1) }
  | access pathop { (Some($1), $2) }

opt_access:
  |        { None }
  | access { Some($1) } 

access:
  | PRIVATE  { accessPrivate }
  | PUBLIC   { accessPublic }
  | INTERNAL { accessInternal }

/* only valid on 'NEW' */
opt_decl_visibility:
  | access { Some($1) } 
  |  { None }
  
opt_interfaceImplDefn: 
  | WITH objectImplementationBlock decl_end { Some($2) } 
  |                            { None }

opt_classDefn: 
  | WITH classDefnBlock decl_end { $2 } 
  |                           { [] }

opt_classSpfn: 
  | WITH classSpfnBlock decl_end { $2 } 
  |                    { [] }


inheritsDefn: 
  | INHERIT appType optBaseSpec
     { let mDecl = union_ranges (rhs parseState 1) (range_of_syntype $2) in
       ClassMemberDefn_inherit($2,$3,mDecl) }
  | INHERIT appType opt_HIGH_PRECEDENCE_APP atomicExprAfterType optBaseSpec
     { let mDecl = union_ranges (rhs parseState 1) (range_of_synexpr $4) in
       ClassMemberDefn_implicit_inherit($2,$4,$5,mDecl) }

optAsSpec: 
  | asSpec { Some($1) } 
  |        { None }

asSpec: 
  | AS ident { $2 } 

optBaseSpec: 
  | baseSpec { Some($1) } 
  |        { None }

baseSpec: 
  | AS ident 
      { if ($2).idText <> "base" then 
             errorR(Error("'inherit' declarations may not have 'as'  bindings. The keyword 'base' may be used instead. Remove this 'as' binding",rhs2 parseState 1 2)); 
        ident("base",rhs parseState 2) } 
        
  | AS BASE 
      { errorR(Error("'inherit' declarations may not have 'as' bindings. The keyword 'base' may be used instead. Remove this 'as' binding",rhs2 parseState 1 2)); 
        ident("base",rhs parseState 2) } 


objectImplementationBlock:
  | OBLOCKBEGIN objectImplementationMembers OBLOCKEND { $2 }
  | OBLOCKBEGIN objectImplementationMembers recover { $2 }
  | objectImplementationMembers { $1 }

objectImplementationMembers:  
  | objectImplementationMember opt_seps objectImplementationMembers { $1 @  $3 }
  | objectImplementationMember opt_seps { $1 }

objectImplementationMember: 
  | opt_attributes memberOrOverride memberCore opt_ODECLEND
     { $3 None OverrideMemberFlags $1 }
  | opt_attributes memberOrOverride error { [] } 

memberOrOverride: 
  | MEMBER {} 
  | OVERRIDE {}


tyconDefnOrSpfnSimpleRepr: 
  | opt_attributes opt_decl_visibility typ
     { if nonNil $1 then errorR(Error("Attributes are not allowed here",rhs parseState 1));
       if isSome $2 then errorR(Error("Visibility declarations are not permitted on type abbreviations",rhs parseState 2));
       TyconCore_abbrev ($3, lhs(parseState)) }
  | opt_attributes opt_decl_visibility unionRepr
     { if nonNil $1 then errorR(Error("Attributes are not allowed here",rhs parseState 1));
       if $3 |> List.exists (function Choice1Of2 _ -> true | _ -> false) then (
           if isSome $2 then errorR(Error("Visibility declarations are not permitted on enum types",rhs parseState 2));
           TyconCore_enum ($3 |> List.choose (function 
                                              | Choice1Of2 data ->  
                                                Some(data) 
                                              | Choice2Of2(UnionCase(_,_,_,_,_,m)) -> 
                                                errorR(Error("All enum fields must be given values",m)); None),
                           lhs(parseState))
       ) else 
           TyconCore_union ($2, 
                            $3 |> List.choose (function Choice2Of2 data -> Some(data) | Choice1Of2 _ -> failwith "huh?"),
                            lhs(parseState)) }
  | opt_attributes opt_decl_visibility braceFieldDeclList
     { if nonNil $1 then errorR(Error("Attributes are not allowed here",rhs parseState 1));
       TyconCore_recd ($2,$3,lhs(parseState)) }
  | opt_attributes opt_decl_visibility LPAREN inlineAssemblyTyconRepr RPAREN
     { if nonNil $1 then errorR(Error("Attributes are not allowed here",rhs parseState 1));
       libraryOnlyWarning (lhs(parseState));
       if isSome $2 then errorR(Error("Visibility declarations are not permitted on inline assembly code types",rhs parseState 2));
       MatchPair parseState 3 5;  
       $4 }


braceFieldDeclList:
  | LBRACE  recdFieldDeclList RBRACE
     { MatchPair parseState 1 3;   $2 }
  | LBRACE  recdFieldDeclList recover
     { $2 }
  | LBRACE  error RBRACE
     { MatchPair parseState 1 3;   [] }

inlineAssemblyTyconRepr:
  | HASH STRING opt_HASH 
     { libraryOnlyWarning (lhs(parseState));
       let lhsm = lhs(parseState) in 
       TyconCore_asm (ParseAssemblyCodeType $2 (rhs parseState 2),lhsm) }

classOrInterfaceOrStruct: 
  | CLASS     { TyconClass } 
  | INTERFACE { TyconInterface } 
  | STRUCT    { TyconStruct }

interfaceMember: 
  | INTERFACE { } 
  | OINTERFACE_MEMBER    { }

tyconNameAndTyparDecls:  
  | opt_access path 
      { [], $2,false,[],$1,grabXmlDoc(parseState,2) }
  | opt_access prefixTyparDecls  path
      { $2, $3,false,[],$1,grabXmlDoc(parseState,2) }
  | opt_access path postfixTyparDecls 
      { let tps,tpcs = $3 in 
        tps, $2,true,tpcs,$1,grabXmlDoc(parseState,2) }

prefixTyparDecls:
  | typar { [ TyparDecl([],$1) ] }
  | LPAREN prefixTyparDeclList RPAREN {  MatchPair parseState 1 3; List.rev $2 }

prefixTyparDeclList: 
  | prefixTyparDeclList COMMA typarDecl { $3 :: $1 } 
  | typarDecl { [$1] }

typarDecl : 
  | opt_attributes typar { TyparDecl($1,$2) }

postfixTyparDecls: 
  | opt_HIGH_PRECEDENCE_TYAPP LESS prefixTyparDeclList opt_typeConstraints GREATER { List.rev $3, $4 }

explicitValTyparDeclsCore: 
  | prefixTyparDeclList COMMA DOT_DOT 
      { (List.rev $1,true) }
  | DOT_DOT 
      { deprecatedWithError "Either specify all relevant type parameters or none" (lhs(parseState));
        ([],true) }
  | prefixTyparDeclList 
      { (List.rev $1,false) }
  | 
      { ([],false) }

explicitValTyparDecls: 
  | opt_HIGH_PRECEDENCE_TYAPP LESS explicitValTyparDeclsCore opt_typeConstraints GREATER 
      { let tps,flex = $3 in 
         SynValTyparDecls(tps,flex,$4) }

opt_explicitValTyparDecls: 
  | explicitValTyparDecls 
      { $1 } 
  |       
      { SynValTyparDecls([],true,[]) }

opt_explicitValTyparDecls2: 
  | explicitValTyparDecls 
      { Some $1 } 
  |       
      { None }

opt_typeConstraints:
  |                      
     { [] }
  | WHEN typeConstraints 
     { List.rev $2 }

typeConstraints: 
  | typeConstraints AND typeConstraint { $3 :: $1 } 
  | typeConstraint { [$1] }

typeConstraint: 
  | DEFAULT typar COLON typ 
      { libraryOnlyWarning (lhs(parseState)); WhereTyparDefaultsToType($2,$4,lhs(parseState)) }
  | typar COLON_GREATER typ 
      { WhereTyparSubtypeOfType($1,$3,lhs(parseState)) }
  | typar COLON STRUCT 
      { WhereTyparIsValueType($1,lhs(parseState)) }
  | typar COLON IDENT STRUCT 
      { if $3 <> "not" then reportParseErrorAt (rhs parseState 3) ("Unexpected identifier: '"^ $3 ^"'");  
        WhereTyparIsReferenceType($1,lhs(parseState)) }
  | typar COLON NULL 
      { WhereTyparSupportsNull($1,lhs(parseState)) }
  | typar COLON LPAREN classMemberSpfn RPAREN 
      { WhereTyparSupportsMember([ $1 ],$4,lhs(parseState)) }
  | LPAREN typar OR typar RPAREN COLON LPAREN classMemberSpfn RPAREN 
      { WhereTyparSupportsMember([ $2 ; $4 ],$8,lhs(parseState)) }
  | typar COLON DELEGATE typeArgs 
      { WhereTyparIsDelegate($1,$4,lhs(parseState)) }
  | typar COLON IDENT typeArgs 
      { match $3 with 
        | "enum" -> WhereTyparIsEnum($1,$4,lhs(parseState))
        | nm -> raiseParseErrorAt (rhs parseState 3) ("Unexpected identifier: '"^ nm ^"'") }

unionRepr:
  /* Note the next three rules are required to disambiguate this from type x = y */
  /* Attributes can only appear on a single constructor if you've used a | */
  | barAndgrabXmlDoc attrUnionCaseDecls  
     { $2 $1 }
  | firstUnionCaseDeclOfMany barAndgrabXmlDoc attrUnionCaseDecls  
     { $1 :: $3 $2 }
  | firstUnionCaseDecl 
     { [$1] } 

barAndgrabXmlDoc : 
  | BAR { grabXmlDoc(parseState,1) }

attrUnionCaseDecls: 
  | attrUnionCaseDecl barAndgrabXmlDoc attrUnionCaseDecls  { (fun xmlDoc -> $1 xmlDoc  :: $3 $2) } 
  | attrUnionCaseDecl { (fun xmlDoc -> [ $1 xmlDoc ]) }

attrUnionCaseDecl: 
  | opt_attributes opt_access unionCaseName opt_OBLOCKSEP
      { if isSome $2 then errorR(Error("Visibility declarations are not permitted on union cases. Use 'type U = internal ...' or 'type U = private ...' to give an accessibility to the whole representation",rhs parseState 2));
        let mDecl = rhs parseState 3 in
        (fun xmlDoc -> Choice2Of2 (UnionCase ( $1, $3,UnionCaseFields [],xmlDoc,None,mDecl))) 
      } 
  | opt_attributes opt_access unionCaseName OF unionCaseRepr  opt_OBLOCKSEP
      { if isSome $2 then errorR(Error("Visibility declarations are not permitted on union cases. Use 'type U = internal ...' or 'type U = private ...' to give an accessibility to the whole representation",rhs parseState 2));
        let mDecl = rhs2 parseState 3 5 in
        (fun xmlDoc -> Choice2Of2 (UnionCase ( $1, $3,UnionCaseFields $5,xmlDoc,None,mDecl))) 
      } 
  | opt_attributes opt_access unionCaseName COLON topType opt_OBLOCKSEP
      { if isSome $2 then errorR(Error("Visibility declarations are not permitted on union cases. Use 'type U = internal ...' or 'type U = private ...' to give an accessibility to the whole representation",rhs parseState 2));
        libraryOnlyWarning(lhs(parseState));
        let mDecl = rhs2 parseState 3 5 in
        (fun xmlDoc -> Choice2Of2 (UnionCase ( $1, $3,UnionCaseFullType $5,xmlDoc,None,mDecl))) 
      }
  | opt_attributes opt_access unionCaseName EQUALS constant opt_OBLOCKSEP
      { if isSome $2 then errorR(Error("Visibility declarations are not permitted on enumeration fields",rhs parseState 2));
        let mDecl = rhs2 parseState 3 5 in
        (fun xmlDoc -> Choice1Of2 (EnumCase ( $1, $3,$5,xmlDoc,mDecl))) 
      } 

/* REVIEW: unify this with operatorName! */
unionCaseName: 
  | nameop  
      { $1 } 
  | LPAREN COLON_COLON RPAREN  
      {  MatchPair parseState 1 3; ident(opname_Cons,rhs parseState 2) }  
  | LPAREN LBRACK RBRACK  RPAREN  
      {  MatchPair parseState 1 4; ident(opname_Nil,rhs2 parseState 2 3) }  

firstUnionCaseDeclOfMany: 
  | ident opt_OBLOCKSEP
      { 
        Choice2Of2 (UnionCase ( [], $1,UnionCaseFields [],emptyPreXmlDoc,None,rhs parseState 1)) 
      } 
  | ident EQUALS constant opt_OBLOCKSEP
      { 
        Choice1Of2 (EnumCase ([],$1,$3,emptyPreXmlDoc,rhs2 parseState 1 3)) 
      }
  | firstUnionCaseDecl opt_OBLOCKSEP
      { $1 }

firstUnionCaseDecl: 
  | ident OF unionCaseRepr  
     { 
       Choice2Of2 (UnionCase ( [],$1,UnionCaseFields $3,emptyPreXmlDoc,None,rhs2 parseState 1 3)) 
    } 
  | ident EQUALS constant opt_OBLOCKSEP
      { 
        Choice1Of2 (EnumCase ([],$1,$3,emptyPreXmlDoc,rhs2 parseState 1 3)) 
      }

unionCaseRepr:
  | braceFieldDeclList
     { errorR(Deprecated("Consider using a separate record type instead",lhs(parseState))); 
       $1 }
  | appType STAR tupleTypeElements 
     { List.map anon_field_of_typ ($1 :: $3) }
  | appType 
     { [anon_field_of_typ $1] }

recdFieldDeclList: 
  | recdFieldDecl seps recdFieldDeclList 
     { $1 :: $3 } 
  | recdFieldDecl opt_seps           
     { [$1] }

recdFieldDecl: 
  | opt_attributes  fieldDecl
     { let fld = $2 $1 false in 
       let (Field(a,b,c,d,e,f,vis,g)) = fld in 
       if isSome vis then errorR(Error("Visibility declarations are not permitted on record fields. Use 'type R = internal ...' or 'type R = private ...' to give an accessibility to the whole representation",rhs parseState 2));
       Field(a,b,c,d,e,f,None,g)  }

fieldDecl: 
  | opt_mutable opt_access ident COLON  polyType 
     { let rhsm = rhs2 parseState 3 5 in 
       (fun attrs stat -> Field(attrs, stat,Some $3,$5,$1,grabXmlDoc(parseState,3),$2,rhsm)) }


exconDefn: 
  | exconCore opt_classDefn 
     { ExconDefn($1,$2, lhs(parseState)) }

exconSpfn: 
  | exconCore opt_classSpfn 
     { ExconSpfn($1,$2,lhs(parseState)) }
  
exceptionAndGrabDoc:
  | EXCEPTION { grabXmlDoc(parseState,1) }
  
exconCore: 
  | exceptionAndGrabDoc opt_attributes opt_access exconIntro exconRepr 
     { ExconCore($2,$4, $5,$1,$3,lhs(parseState)) }
  
exconIntro: 
  | ident 
      { UnionCase ( [], $1,UnionCaseFields [],emptyPreXmlDoc,None,lhs(parseState)) }
  | ident OF unionCaseRepr 
      { UnionCase ( [], $1,UnionCaseFields $3,emptyPreXmlDoc,None,lhs(parseState)) }

exconRepr: 
  |             { None }
  | EQUALS path { Some ($2) }

openDecl:  
  |  OPEN path { $2 }

defnBindings: 
  | LET opt_rec localBindings 
      { let letm = rhs parseState 1 in 
        let isUse,isRec,bindingsPreAttrs = $1,$2,$3 in 
        (* the first binding swallow any attributes prior to the 'let' *)
        BindingSetPreAttrs(rhs parseState 1,isRec,isUse,(fun attrs vis -> 
            let binds = bindingsPreAttrs attrs vis letm in 
                        if not isRec && List.length binds > 1 then 
                                ocamlCompatWarning "The declaration form 'let ... and ...' for non-recursive bindings is deprecated in F# code. In F# code simply use multiple 'let' bindings" letm; 
            [],binds)) }
  | cPrototype
      { BindingSetPreAttrs(lhs(parseState), false,false,$1)  }

doBinding:
  | DO typedSeqExprBlock 
      { let letm = rhs parseState 1 in 
        let wholem = rhs2 parseState 1 2 in 
        // any attributes prior to the 'let' are left free, e.g. become top-level attributes 
        // associated with the module, 'main' function or assembly depending on their target 
        BindingSetPreAttrs(letm,false,false,(fun attrs vis -> attrs,[mk_Do (vis,true,$2,wholem)])) }


hardwhiteLetBindings: 
  | OLET opt_rec localBindings hardwhiteDefnBindingsTerminator
      { $4 (rhs parseState 1);  (* report unterminated error *)
        let letm = rhs parseState 1 in 
        let isUse,isRec,bindingsPreAttrs = $1,$2,$3 in 
           
        (* the first binding swallow any attributes prior to the 'let' *)
        BindingSetPreAttrs(letm,isRec,isUse,(fun attrs vis -> 
            let binds = bindingsPreAttrs attrs vis letm in 
                        if not isRec && List.length binds > 1 then 
                                ocamlCompatWarning "The declaration form 'let ... and ...' for non-recursive bindings is deprecated in F# code. In F# code simply use multiple 'let' bindings" letm; 
            [],bindingsPreAttrs attrs vis letm)) }

hardwhiteDoBinding: 
  | ODO typedSeqExprBlock hardwhiteDefnBindingsTerminator          
      { $3 (rhs parseState 1);  (* report unterminated error *)
        let letm = rhs parseState 1 in 
        let wholem = union_ranges letm (range_of_synexpr $2) in 
        let seqPt = NoSequencePointAtDoBinding in 
        // any attributes prior to the 'let' are left free, e.g. become top-level attributes 
        // associated with the module, 'main' function or assembly depending on their target 
        BindingSetPreAttrs(letm,false,false,(fun attrs vis -> attrs,[mk_Do (vis,true,$2,wholem)])), $2 }

classDefnBindings: 
  | defnBindings { $1 }
  | doBinding { $1 }
  | hardwhiteLetBindings { $1 } 
  | hardwhiteDoBinding  { fst $1 }


hardwhiteDefnBindingsTerminator:
  |  ODECLEND
     { (fun m -> ()) }
  |  recover 
     { (fun m -> reportParseErrorAt m "unmatched 'let' or 'do'") }

cPrototype: 
  | EXTERN cRetType opt_access ident opt_HIGH_PRECEDENCE_APP LPAREN cArgs RPAREN 
      { let rty,vis,nm,args  = $2,$3,$4,$7 in 
        let xmlDoc = grabXmlDoc(parseState,1) in 
        let nmm = rhs parseState 3 in 
        let argsm = rhs parseState 6 in 
        let bindm = lhs(parseState) in
        let wholem = lhs(parseState) in
        let rhsm = lhs(parseState) in 
        let rhsExpr = Expr_app(ExprAtomicFlag.NonAtomic, Expr_id_get(ident("failwith",rhsm)),Expr_const(Const_string("extern was not given a DllImport attribute",rhsm),rhsm),rhsm) in
        (fun attrs vis -> 
            let binding = mksyn_binding (xmlDoc,Pat_lid ([nm],Some(noInferredTypars),[Pat_tuple(args,argsm)],vis,nmm)) (vis,false,false,bindm,NoSequencePointAtInvisibleBinding,wholem,Some(rty),rhsExpr,rhsm,[],attrs,None) in
            [], [binding]) }

cArgs: 
  | cMoreArgs 
     { List.rev $1 }
  | cArg 
     { [$1] }
  |       
     { [] }
  
cMoreArgs: 
  | cMoreArgs COMMA cArg 
     { $3 :: $1 }
  | cArg COMMA cArg 
     { [$3; $1] }

cArg: 
  | opt_attributes cType       
     { let m = lhs(parseState) in Pat_typed(Pat_wild m,$2,m) |> addAttribs $1 }
  | opt_attributes cType ident 
     { let m = lhs(parseState) in Pat_typed(Pat_as (Pat_wild m,$3,false,None,m),$2,m) |> addAttribs $1 }

cType: 
  | path      
     { let m = lhs(parseState) in 
       Type_app(Type_lid($1,m),[],false,m) } 

  | cType opt_HIGH_PRECEDENCE_APP LBRACK RBRACK 
     { let m = lhs(parseState) in 
       Type_app(Type_lid([ident("[]",m)],m),[$1],true,m) } 

  | cType STAR 
     { let m = lhs(parseState) in 
       Type_app(Type_lid([ident("nativeptr",m)],m),[$1],true,m) } 

  | cType AMP  
     { let m = lhs(parseState) in 
       Type_app(Type_lid([ident("byref",m)],m),[$1],true,m) } 

  | VOID STAR 
     { let m = lhs(parseState) in 
       Type_app(Type_lid([ident("nativeint",m)],m),[],true,m) } 

cRetType: 
  | opt_attributes cType 
     { ($2,ArgSynInfo($1,false,None)),rhs parseState 2 }
  | opt_attributes VOID  
     { let m = rhs parseState 2 in 
       (Type_app(Type_lid([ident("unit",m)],m),[],false,m),ArgSynInfo($1,false,None)),m } 


localBindings: 
  | attr_localBinding more_localBindings 
      { (fun attrs vis letm -> 
           match $1 with 
           | Some f -> (f attrs vis letm true ::  $2) 
           | None -> $2) }

more_localBindings: 
  | AND attr_localBinding more_localBindings 
      { let letm = (rhs parseState 1) in
        (match $2 with 
         | Some f -> f [] None letm false :: $3 
         | None -> $3) }
  | %prec prec_no_more_attr_bindings 
      { [] }

attr_localBinding: 
  | DO typedSeqExprBlock 
      { let m = union_ranges (rhs parseState 1) (range_of_synexpr $2) in 
        Some(fun attrs vis _ isFirst -> 
          deprecatedWithError "The declaration form 'let do ...' and 'and do ...' has been removed from the F# language" m;
          mk_Do (vis,true,$2,m)) }
  | opt_attributes localBinding 
      { Some(fun attrs vis letm _ -> 
          $2 (attrs@$1) vis letm) }
  | error 
      { None }

localBinding: 
  | opt_inline opt_mutable bindingPattern  opt_topReturnTypeWithTypeConstraints EQUALS  typedExprWithStaticOptimizationsBlock 
      { let expr,opts = $6 in
        let eqm = rhs parseState 5 in 
        let rhsm = range_of_synexpr expr in 
        let optReturnType = $4 in 
        let bindingBuilder,bindm = $3 in 
        (fun attrs vis letm -> 
            let wholem = union_ranges letm rhsm in
            let spBind = if IsControlFlowExpression expr then NoSequencePointAtLetBinding else SequencePointAtBinding(wholem) in
            bindingBuilder (vis,$1,$2,bindm,spBind,wholem,optReturnType,expr,rhsm,opts,attrs,None)) }
  | opt_inline opt_mutable bindingPattern  opt_topReturnTypeWithTypeConstraints EQUALS  error
      { let wholem = rhs2 parseState 3 5 in 
        let rhsm = rhs parseState 5 in
        let optReturnType = $4 in 
        let bindingBuilder,bindm = $3 in 
        (fun attrs vis letm -> 
            let spBind = SequencePointAtBinding(union_ranges letm rhsm) in
            bindingBuilder (vis,$1,$2,bindm,spBind,wholem,optReturnType,arbExpr(parseState),rhsm,[],attrs,None))  }

/* REVIEW: this should probably be an expression form rather than tied to this particular part of the grammar */
typedExprWithStaticOptimizationsBlock: 
  | OBLOCKBEGIN typedExprWithStaticOptimizations OBLOCKEND { $2 }
  | OBLOCKBEGIN typedExprWithStaticOptimizations recover { $2 (* silent recovery *) }
  | typedExprWithStaticOptimizations { $1 }

typedExprWithStaticOptimizations : 
  | typedSeqExpr opt_staticOptimizations { $1, List.rev $2 }

opt_staticOptimizations: 
  | opt_staticOptimizations staticOptimization { $2 :: $1 } 
  | { [] }

staticOptimization: 
  | WHEN staticOptimizationConditions EQUALS typedSeqExprBlock { ($2,$4) }

staticOptimizationConditions: 
  | staticOptimizationConditions AND staticOptimizationCondition { $3 :: $1 } 
  | staticOptimizationCondition { [$1 ] }

staticOptimizationCondition: 
  | typar COLON typ { WhenTyparTyconEqualsTycon($1,$3,lhs(parseState)) }
  | TRUE  {WhenInlined(rhs parseState 1) }

rawconstant: 
  | INT8 { if snd $1 then errorR(Error("This number is outside the allowable range for 8-bit signed integers", lhs(parseState)));
           Const_int8 (fst $1) } 
  | UINT8 { Const_uint8 $1 } 
  | INT16 { if snd $1 then errorR(Error("This number is outside the allowable range for 16-bit signed integers", lhs(parseState)));
            Const_int16 (fst $1) } 
  | UINT16 { Const_uint16 $1 } 
  | INT32 { if snd $1 then errorR(Error("This number is outside the allowable range for 32-bit signed integers", lhs(parseState)));
            Const_int32 (fst $1) } 
  | UINT32 { Const_uint32 $1 } 
  | INT64 { if snd $1 then errorR(Error("This number is outside the allowable range for 64-bit signed integers", lhs(parseState)));
            Const_int64 (fst $1) } 
  | UINT64 { Const_uint64 $1 } 
  | NATIVEINT { Const_nativeint $1 } 
  | UNATIVEINT { Const_unativeint $1 } 
  | IEEE32 { Const_float32 $1 } 
  | IEEE64 { Const_float $1 } 
  | CHAR { Const_char $1 } 
  | DECIMAL { Const_decimal $1 } 
  | BIGNUM { Const_bignum $1 } 
  | STRING { Const_string ($1,lhs(parseState)) } 
  | BYTEARRAY { Const_bytearray ($1,lhs(parseState)) }

constant: 
  | rawconstant { $1 }
  | rawconstant HIGH_PRECEDENCE_TYAPP measurearg { Const_measure($1, $3) }

bindingPattern:
  | headBindingPattern   
      {  let xmlDoc = grabXmlDoc(parseState,1) in
         mksyn_binding (xmlDoc,$1), rhs parseState 1 }

/* sp = v | sp:typ | attrs sp */
simplePattern:
  | ident 
      { SPat_as ($1,false,false,false,rhs parseState 1) }
  | QMARK ident 
      { SPat_as ($2,false,false,true,rhs parseState 2) }
  | simplePattern COLON typeWithTypeConstraints
      { let lhsm = lhs(parseState) in 
        SPat_typed($1,$3,lhsm) }
  | attributes simplePattern %prec paren_pat_attribs
      { let lhsm = lhs(parseState)  in
        SPat_attrib($2,$1,lhsm) }

simplePatternCommaList:
  | simplePattern { [$1] }
  | simplePattern COMMA simplePatternCommaList { $1 :: $3 }

simplePatterns:
  | LPAREN simplePatternCommaList RPAREN { $2 }
  | LPAREN RPAREN { [] }
  | LPAREN simplePatternCommaList recover { reportParseErrorAt (rhs parseState 1) "unmatched '('"; [] }
  | LPAREN error RPAREN { (* silent recovery *) [] }
  | LPAREN recover {  reportParseErrorAt (rhs parseState 1) "unmatched '('"; [] }  


headBindingPattern:
  | headBindingPattern AS ident 
      { Pat_as ($1,$3,false,None,rhs2 parseState 1 3) }
  | headBindingPattern BAR headBindingPattern  
      { Pat_disj($1,$3,rhs2 parseState 1 3) }
  | headBindingPattern COLON_COLON  headBindingPattern 
      { Pat_lid (mksyn_constr (rhs parseState 2) opname_Cons, None,[Pat_tuple ([$1;$3],rhs2 parseState 1 3)],None,lhs(parseState)) }
  | tuplePatternElements  %prec pat_tuple 
      { Pat_tuple(List.rev $1, lhs(parseState)) }
  | conjPatternElements   %prec pat_conj
      { Pat_conjs(List.rev $1, lhs(parseState)) }
  | constrPattern 
      { $1 }

tuplePatternElements: 
  | tuplePatternElements COMMA headBindingPattern { $3 :: $1 }
  | headBindingPattern COMMA headBindingPattern { $3 :: $1 :: [] }

conjPatternElements: 
  | conjPatternElements AMP headBindingPattern { $3 :: $1 }
  | headBindingPattern AMP headBindingPattern { $3 :: $1 :: [] }

constrPattern:
  | atomicPatternLongIdent explicitValTyparDecls                                                          { let vis,lid = $1 in Pat_lid (lid,Some $2,[],vis,lhs(parseState)) }
  | atomicPatternLongIdent opt_explicitValTyparDecls2                     atomicPatterns    %prec pat_app { let vis,lid = $1 in Pat_lid (lid,$2,$3,vis,lhs(parseState)) }
  | atomicPatternLongIdent opt_explicitValTyparDecls2 HIGH_PRECEDENCE_APP atomicPatterns                  { let vis,lid = $1 in Pat_lid (lid,$2,$4,vis,lhs(parseState)) }
  | COLON_QMARK atomType  %prec pat_isinst { Pat_isinst($2,lhs(parseState)) }
  | atomicPattern { $1 }

atomicPatterns: 
  | atomicPattern atomicPatterns %prec pat_args { $1 :: $2 } 
  | atomicPattern HIGH_PRECEDENCE_APP atomicPatterns 
      { reportParseErrorAt (rhs parseState 1) "Successive patterns should be separated by spaces or tupled";
        $1 :: $3 } 
  | atomicPattern { [$1] }


atomicPattern:
  | quoteExpr 
      { Pat_expr($1,lhs(parseState)) } 
  | CHAR DOT_DOT CHAR { Pat_range ($1,$3,rhs2 parseState 1 3) }
  | LBRACE recordPatternElements RBRACE
      { $2 }
  | LBRACK listPatternElements RBRACK
      { MatchPair parseState 1 3; Pat_array_or_list(false,$2,lhs(parseState)) }
  | LBRACK_BAR listPatternElements  BAR_RBRACK
      { MatchPair parseState 1 3; Pat_array_or_list(true,$2, lhs(parseState)) }
  | UNDERSCORE { Pat_wild (lhs(parseState)) }
  | QMARK ident { Pat_opt_var($2,lhs(parseState)) } 
  | atomicPatternLongIdent %prec prec_atompat_pathop 
      { let vis,lid = $1 in 
        if List.length lid > 1 || (let c = (List.hd lid).idText.[0] in Char.IsUpper(c) && not (Char.IsLower c)) 
        then mksyn_pat_maybe_var lid vis (lhs(parseState))
        else mksyn_pat_var vis (List.hd lid) }
  | constant { Pat_const ($1,range_of_synconst $1 (lhs(parseState))) }
  | FALSE  { Pat_const(Const_bool false,lhs(parseState)) } 
  | TRUE  { Pat_const(Const_bool true,lhs(parseState)) } 
  | NULL { Pat_null(lhs(parseState)) }
  | LPAREN parenPatternBody RPAREN {  MatchPair parseState 1 3; let m = (lhs(parseState)) in Pat_paren($2 m,m) } 
  | LPAREN parenPatternBody recover { reportParseErrorAt (rhs parseState 1) "unmatched '('"; $2 (rhs2 parseState 1 2) }
  | LPAREN error RPAREN { (* silent recovery *) Pat_wild (lhs(parseState)) }
  | LPAREN recover {  reportParseErrorAt (rhs parseState 1) "unmatched '('"; Pat_wild (lhs(parseState))}  

  
      
parenPatternBody: 
  | parenPattern 
      { (fun m -> $1) } 
  |      
      { (fun m -> Pat_const(Const_unit,m)) } 

/* This duplicates out 'patterns' in order to give type annotations */
/* the desired precedence w.r.t. patterns, tuple patterns in particular. */
/* Duplication requried to minimize the disturbance to the grammar, */
/* in particular the expected property that "pat" parses the same as */
/* "(pat)"!  Here are some examples: */
/*    a,b                  parses as (a,b) */
/*    (a,b)           also parses as (a,b) */
/*    (a,b : t)            parses as (a, (b:t)) */
/*    a,b as t             parses as ((a,b) as t) */
/*    (a,b as t)      also parses as ((a,b) as t) */
/*    a,b | c,d            parses as ((a,b) | (c,d)) */
/*    (a,b | c,d)     also parses as ((a,b) | (c,d)) */
/*    (a : t,b)            parses as ((a:t),b) */
/*    (a : t1,b : t2)      parses as ((a:t),(b:t2)) */
/*    (a,b as nm : t)      parses as (((a,b) as nm) : t) */
/*    (a,b :: c : t)       parses as (((a,b) :: c) : t) */
/* */
/* Probably the most unexpected thing here is that 'as nm' binds the */
/* whole pattern to the left, whereas ': t' binds only the pattern */
/* immediately preceding in the tuple. */
/* */
/* Also, it is unexpected that '(a,b : t)' in a pattern binds differently to */
/* '(a,b : t)' in an expression. It's not that easy to solve that without */
/* duplicating the entire expression grammar, or making a fairly severe breaking change */
/* to the language. */
parenPattern:
  | parenPattern AS ident 
      { Pat_as ($1,$3,false,None,rhs2 parseState 1 3) }
  | parenPattern BAR parenPattern  
      { Pat_disj($1,$3,rhs2 parseState 1 3) }
  | tupleParenPatternElements 
      { Pat_tuple(List.rev $1,lhs(parseState)) }
  | conjParenPatternElements
      { Pat_conjs(List.rev $1,rhs2 parseState 1 3) }
  | parenPattern COLON  typeWithTypeConstraints %prec paren_pat_colon
      { let lhsm = lhs(parseState) in 
        Pat_typed($1,$3,lhsm) } 
  | attributes parenPattern  %prec paren_pat_attribs
      { let lhsm = lhs(parseState)  in
        Pat_attrib($2,$1,lhsm) } 
  | parenPattern COLON_COLON  parenPattern 
      { Pat_lid (mksyn_constr (rhs parseState 2) opname_Cons, None, [ Pat_tuple ([$1;$3],rhs2 parseState 1 3) ],None,lhs(parseState)) }
  | parenPattern COLON_GREATER typ  
      { let lhsm = lhs(parseState) in 
        deprecatedWithError "Patterns of the form 'pat :> type' have been removed from the F# language. Consider using just 'pat : type' instead" lhsm;
        Pat_typed($1, mksyn_anon_constraint $3 lhsm,lhsm) } 
  | constrPattern { $1 }

tupleParenPatternElements:
  | tupleParenPatternElements COMMA parenPattern  { $3 :: $1 }
  | parenPattern COMMA parenPattern  { $3 :: $1 :: [] }
  
conjParenPatternElements: 
  | conjParenPatternElements AMP parenPattern { $3 :: $1 }
  | parenPattern AMP parenPattern { $3 :: $1 :: [] }

recordPatternElements:
  | recordPatternElementsAux { let rs,m = $1 in Pat_recd (rs,m) }

recordPatternElementsAux: /* Fix 1190 */
  | recordPatternElement opt_seps                      { [$1],lhs(parseState) }
  | recordPatternElement seps recordPatternElementsAux { let r = $1 in let (rs,dropMark) = $3 in (r :: rs),lhs(parseState) }

recordPatternElement:  
  | path EQUALS parenPattern { (List.frontAndBack $1,$3) }

listPatternElements: /* Fix 3569 */
  |                                       { [] }
  | parenPattern opt_seps                 { [$1] }
  | parenPattern seps listPatternElements { $1 :: $3 }

/* The lexfilter likes to insert OBLOCKBEGIN/OBLOCKEND pairs */
typedSeqExprBlock: 
  | OBLOCKBEGIN typedSeqExpr OBLOCKEND { $2 }
  | OBLOCKBEGIN typedSeqExpr recover { $2 }
  | typedSeqExpr { $1 }

/* The lexfilter likes to insert OBLOCKBEGIN/OBLOCKEND pairs */
declExprBlock: 
  | OBLOCKBEGIN typedSeqExpr OBLOCKEND { $2 }
  | declExpr { $1 }

/* For some constructs the lex filter can't be sure to insert a matching OBLOCKEND, e.g. "function a -> b | c -> d" all in one line */
/* for these it only inserts a trailing ORIGHT_BLOCK_END */
typedSeqExprBlockR: 
  | typedSeqExpr ORIGHT_BLOCK_END { $1 }
  | typedSeqExpr { $1 }

typedSeqExpr: 
  | seqExpr COLON               typeWithTypeConstraints { Expr_typed ($1,$3, union_ranges (range_of_synexpr $1) (range_of_syntype $3)) }
  | seqExpr COLON_QMARK         typ  %prec expr_isinst  { Expr_isinst($1,$3, union_ranges (range_of_synexpr $1) (range_of_syntype $3)) }
  | seqExpr COLON_GREATER       typ                     { Expr_upcast($1,$3, union_ranges (range_of_synexpr $1) (range_of_syntype $3)) } 
  | seqExpr COLON_QMARK_GREATER typ                     { Expr_downcast($1,$3, union_ranges (range_of_synexpr $1) (range_of_syntype $3)) }
  | seqExpr { $1 }

seqExpr:
  | declExpr seps seqExpr                 { Expr_seq(SequencePointsAtSeq,true,$1,$3,union_ranges (range_of_synexpr $1) (range_of_synexpr $3)) } 
  | declExpr seps                         { $1 }  
  | declExpr             %prec SEMICOLON { $1 } 
  | declExpr THEN seqExpr %prec prec_then_before { Expr_seq(SequencePointsAtSeq,false,$1,$3,union_ranges (range_of_synexpr $1) (range_of_synexpr $3) ) }
  | declExpr OTHEN OBLOCKBEGIN typedSeqExpr OBLOCKEND %prec prec_then_before { Expr_seq(SequencePointsAtSeq,false,$1,$4,union_ranges (range_of_synexpr $1) (range_of_synexpr $4)) }

/* use this as the last terminal when performing silent error recovery */
/* in situations where a syntax error has definitely occurred. This allows */
/* the EOF token to be swallowed to help initiate error recovery. */
recover: 
   | error { }  
   | EOF { }

declExpr:
  | defnBindings IN typedSeqExpr  %prec expr_let 
     { mkLocalBindings (rhs2 parseState 1 3,$1,$3) }
  | defnBindings IN error        %prec expr_let
     { mkLocalBindings (rhs2 parseState 1 2,$1,arbExpr(parseState)) }
  | defnBindings error        %prec expr_let
    { reportParseErrorAt (match $1 with (BindingSetPreAttrs(m,_,_,_))  -> m) "no matching 'in' found for this 'let'";
      mkLocalBindings (rhs parseState 1,$1,arbExpr(parseState)) }

  | hardwhiteLetBindings typedSeqExprBlock  %prec expr_let 
     { mkLocalBindings (union_ranges (rhs parseState 1) (range_of_synexpr $2),$1,$2) }
  | hardwhiteLetBindings error        %prec expr_let
     { reportParseErrorAt (match $1 with (BindingSetPreAttrs(m,_,_,_))  -> m) "error in the return expression for this 'let'. Possible incorrect indentation";
       mkLocalBindings (rhs2 parseState 1 2,$1,arbExpr(parseState)) }
  | hardwhiteLetBindings OBLOCKSEP typedSeqExprBlock  %prec expr_let 
     { mkLocalBindings (union_ranges (rhs2 parseState 1 2) (range_of_synexpr $3) ,$1,$3) }
  | hardwhiteLetBindings OBLOCKSEP error        %prec expr_let
     { //reportParseErrorAt (match $1 with (BindingSetPreAttrs(m,_,_,_))  -> m) "error in the return expression for this 'let'. Possible incorrect indentation";
       mkLocalBindings (rhs2 parseState 1 2,$1,arbExpr(parseState)) }

  | hardwhiteDoBinding  %prec expr_let
     { let e = snd $1 in
       Expr_do(e,range_of_synexpr e) }
  
  | anonMatchingExpr %prec expr_function
      { $1 }
  | anonLambdaExpr  %prec expr_fun { $1 }

  | MATCH typedSeqExpr     withClauses              %prec expr_match 
      { let mMatch = (rhs parseState 1) in
        let mWith,(clauses,mLast) = $3 in 
        let spBind = SequencePointAtBinding(union_ranges mMatch mWith) in 
        Expr_match(spBind, $2,clauses,false,union_ranges mMatch mLast) }

  | MATCH typedSeqExpr     recover               %prec expr_match 
      { (* Produce approximate expression during error recovery *)
        Expr_throwaway($2,rhs2 parseState 1 2) }
      
  | TRY typedSeqExprBlockR withClauses              %prec expr_try 
      { let mTry = (rhs parseState 1) in
        let spTry = SequencePointAtTry(mTry) in 
        let mWith,(clauses,mLast) = $3 in 
        let spWith = SequencePointAtWith(mWith) in 
        let mTryToWith = union_ranges mTry mWith in 
        let mWithToLast = union_ranges mWith mLast in 
        let mTryToLast = union_ranges mTry mLast in
        Expr_try_catch($2, mTryToWith, clauses,mWithToLast, mTryToLast,spTry,spWith) }

  | TRY typedSeqExprBlockR recover              %prec expr_try 
      { (* Produce approximate expression during error recovery *)
        (* Include any expressions to make sure they gets type checked in case that generates useful results for intellisense *)
        $2 }

  | TRY typedSeqExprBlockR FINALLY typedSeqExprBlock %prec expr_try 
      { let mTry = rhs parseState 1 in 
        let spTry = SequencePointAtTry(mTry) in 
        let spFinally = SequencePointAtFinally(rhs parseState 3) in 
        let mTryToLast = union_ranges mTry (range_of_synexpr $4) in 
        Expr_try_finally($2, $4,mTryToLast,spTry,spFinally) }

  | IF declExpr ifExprCases %prec expr_if 
      { let mIf = (rhs parseState 1) in
        $3 $2 mIf }

  | IF declExpr recover %prec expr_if 
      { reportParseErrorAt (rhs parseState 1) "unmatched 'if'"; 
        (* Produce an approximate expression during error recovery. *)
        (* Include expressions to make sure they get type checked in case that generates useful results for intellisense. *)
        (* Generate a throwAway for the expression so it isn't forced to have a type 'bool' *)
        (* from the context it is used in. *)
        Expr_throwaway($2, rhs2 parseState 1 2) }

  | IF recover %prec expr_if 
      { arbExpr parseState }

  | LAZY declExpr %prec expr_lazy 
      { Expr_lazy($2,union_ranges (rhs parseState 1) (range_of_synexpr $2)) }

  | ASSERT declExpr %prec expr_assert { Expr_assert($2, union_ranges (rhs parseState 1) (range_of_synexpr $2)) }
  | ASSERT %prec expr_assert { raiseParseErrorAt (rhs parseState 1) "'assert' may no longer be used as a first class value. Use 'assert <expr>' instead" }

  | WHILE declExpr do_or_odo typedSeqExprBlock done_term 
      { let mWhile = union_ranges (rhs parseState 1) (range_of_synexpr $2) in
        let spWhile = SequencePointAtWhileLoop(mWhile) in 
        Expr_while(spWhile,$2,$4,union_ranges (rhs parseState 1) (range_of_synexpr $4)) }
      
  | WHILE declExpr do_or_odo typedSeqExprBlock recover { reportParseErrorAt (rhs parseState 4) "'done' expected after this expression" ;  arbExpr(parseState) }  
  | WHILE declExpr do_or_odo error done_term { (* silent recovery *) arbExpr(parseState) }  
  | WHILE declExpr recover { reportParseErrorAt (rhs parseState 2) "'do' expected after this expression" ; arbExpr(parseState) }  
  | WHILE error done_term { (* silent recovery *) arbExpr(parseState)  } 

  | FOR forLoopBinder do_or_odo typedSeqExprBlock done_term 
      { let spBind = SequencePointAtForLoop(rhs2 parseState 1 3) in
        let (a,b) = $2 in Expr_foreach(spBind,SeqExprOnly(false),a,b,$4,union_ranges (rhs parseState 1) (range_of_synexpr $4)) }

  | FOR forLoopBinder recover
      { let spBind = SequencePointAtForLoop(rhs2 parseState 1 2) in
        let (a,b) = $2 in Expr_foreach(spBind,SeqExprOnly(false),a,b,arbExpr(parseState),lhs(parseState)) }

  | FOR forLoopBinder do_or_odo error done_term 
      { let spBind = SequencePointAtForLoop(rhs2 parseState 1 3) in
        let (a,b) = $2 in Expr_foreach(spBind,SeqExprOnly(false),a,b,arbExpr(parseState),rhs2 parseState 1 3) }

  | FOR forLoopRange  do_or_odo typedSeqExprBlock done_term 
      { let spBind = SequencePointAtForLoop(rhs2 parseState 1 3) in
        let (a,b,c,d) = $2 in Expr_for(spBind,a,b,c,d,$4,union_ranges (rhs parseState 1) (range_of_synexpr $4)) }

  | FOR forLoopRange  do_or_odo typedSeqExprBlock recover 
      { reportParseErrorAt (rhs parseState 1) "unclosed 'for', e.g. no 'done' found to match this 'for'" ; arbExpr(parseState) }

  | FOR forLoopRange  do_or_odo error done_term 
      { (* silent recovery *) arbExpr(parseState) }

  | FOR error do_or_odo typedSeqExprBlock done_term 
      { (* silent recovery *) $4 }

/* do not include this one - though for fairly bizarre reasons!
   If the user has simply typed 'for'as the 
   start of a variable name, and intellisense parsing 
   kicks in, then we can't be sure we're parsing a for-loop. The general rule is that you shoudn't
   commit to aggressive look-for-a-matching-construct error recovery until
   you're sure you're parsing a particular construct.

  This probably affects 'and' as well, but it's hard to change that.
  'for' is a particularly common prefix of identifiers.

  | FOR error done_term {  reportParseErrorAt (rhs parseState 2)  "identifier expected"; arbExpr(parseState) }
*/

  | FOR parenPattern error done_term {  reportParseErrorAt (rhs parseState 3) "'=' expected"; arbExpr(parseState) }

  /* START MONADIC SYNTAX ONLY */
  | YIELD declExpr
     { Comp_yield(($1,not $1),$2, union_ranges (rhs parseState 1) (range_of_synexpr $2)) } 
  | YIELD_BANG declExpr
     { Comp_yieldm(($1,not $1), $2, union_ranges (rhs parseState 1) (range_of_synexpr $2)) } 

  | BINDER headBindingPattern EQUALS typedSeqExprBlock IN opt_OBLOCKSEP typedSeqExprBlock %prec expr_let 
     { let spBind = SequencePointAtBinding(rhs2 parseState 1 5) in
       let m = rhs parseState 1 in
       Comp_bind(spBind,($1 = "use"),$2,$4,$7, lhs parseState) }

  | OBINDER headBindingPattern EQUALS typedSeqExprBlock hardwhiteDefnBindingsTerminator opt_OBLOCKSEP typedSeqExprBlock %prec expr_let 
     { let spBind = SequencePointAtBinding(union_ranges (rhs parseState 1) (range_of_synexpr $4)) in
       let m = rhs parseState 1 in
       Comp_bind(spBind,($1 = "use"),$2,$4,$7, lhs parseState) }

  | OBINDER headBindingPattern EQUALS typedSeqExprBlock hardwhiteDefnBindingsTerminator opt_OBLOCKSEP error %prec expr_let 
     { (* error recovery that allows intellisense when writing incomplete computation expressions *)
                         let spBind = SequencePointAtBinding(union_ranges (rhs parseState 1) (range_of_synexpr $4)) in
       let m = rhs parseState 1 in
       Comp_bind(spBind,($1 = "use"),$2,$4, (Comp_zero(m)) , lhs parseState) }

  | DO_BANG typedSeqExpr IN opt_OBLOCKSEP typedSeqExprBlock %prec expr_let 
     { let spBind = NoSequencePointAtDoBinding in
       Comp_bind(spBind,false,Pat_const(Const_unit,range_of_synexpr $2),$2,$5, union_ranges (rhs parseState 1) (range_of_synexpr $5)) }

  | ODO_BANG typedSeqExprBlock hardwhiteDefnBindingsTerminator %prec expr_let 
     { Comp_do_bind($2, union_ranges (rhs parseState 1) (range_of_synexpr $2)) }

  | FOR forLoopBinder opt_OBLOCKSEP monadicSingleLineQualifiersThenArrowThenExprR %prec expr_let 
     { let spBind = SequencePointAtForLoop(rhs2 parseState 1 2) in
       let a,b= $2 in Expr_foreach(spBind,SeqExprOnly(true),a,b,$4 (rhs parseState 3),rhs2 parseState 1 3) } 

  | RARROW typedSeqExprBlockR 
     { errorR(Error("The use of '->' in sequence and computation expressions is limited to the form 'for pat in expr -> expr'. Use the syntax 'for ... in ... do ... yield...' to generate elements in more complex sequence expressions",lhs parseState));
       Comp_yield((true,true),$2, lhs parseState) } 
     
  /* END MONADIC SYNTAX ONLY */


  | declExpr COLON_EQUALS           declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 ":=" $3 }
  | minusExpr LARROW                declExprBlock { mksyn_assign (union_ranges (range_of_synexpr $1) (range_of_synexpr $3)) $1 $3 }
  | tupleExpr  %prec expr_tuple  { Expr_tuple( List.rev $1,lhs(parseState)) }
  | declExpr  BAR_BAR               declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 "||" $3 }
  | declExpr  INFIX_BAR_OP          declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 $2 $3 }
  | declExpr  OR                    declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 "or" $3 }
  | declExpr  AMP                   declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 "&" $3 }
  | declExpr  AMP_AMP               declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 "&&" $3 }
  | declExpr  INFIX_AMP_OP          declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 $2 $3 }
  | declExpr  EQUALS                declExpr { mksyn_infix (rhs parseState 2) (union_ranges (range_of_synexpr $1) (range_of_synexpr $3)) $1 "=" $3 }
  | declExpr  INFIX_COMPARE_OP      declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 $2 $3 }
  | declExpr  DOLLAR                declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 "$" $3 }
  | declExpr  LESS                  declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 "<" $3 }
  | declExpr  GREATER               declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 ">" $3 }
  | declExpr  INFIX_AT_HAT_OP       declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 $2 $3 }
  | declExpr  PERCENT_OP            declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 $2 $3 }
  | declExpr  COLON_COLON           declExpr { Expr_app (ExprAtomicFlag.NonAtomic, mksyn_item (rhs parseState 2) opname_Cons,Expr_tuple ([$1;$3],lhs(parseState)),lhs(parseState)) }
  | declExpr  PLUS_MINUS_OP         declExpr { mksyn_infix (rhs parseState 2)  (lhs(parseState)) $1 $2 $3 }
  | declExpr  MINUS                 declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 "-" $3 }
  | declExpr  STAR                  declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 "*" $3 }
  | declExpr  INFIX_STAR_DIV_MOD_OP declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 $2 $3 }
  | declExpr  INFIX_STAR_STAR_OP    declExpr { mksyn_infix (rhs parseState 2) (lhs(parseState)) $1 $2 $3 }
  | minusExpr %prec expr_prefix_plus_minus { $1 }

dynamicArg:
  | IDENT
      { let con = Const_string ($1,rhs parseState 1) in
        let arg2 = Expr_const (con,range_of_synconst con (rhs parseState 1))  in
        arg2 }
  | LPAREN typedSeqExpr RPAREN
      { $2 }

monadicWhenCondition:
  | WHEN declExpr 
     { $2 }
  
withClauses:
  | WITH withPatternClauses       { rhs parseState 1, $2 }
  | OWITH withPatternClauses OEND { rhs parseState 1, $2 }
  | OWITH withPatternClauses recover { rhs parseState 1, $2 }

withPatternClauses:
  | patternClauses 
      { $1 }
  | BAR patternClauses 
      {  $2 }
  | BAR error 
      {  (* silent recovery *)  
         let mLast = rhs parseState 1 in
         [], mLast }
  | error  
      {  (* silent recovery *)  
         let mLast = rhs parseState 1 in
         [], mLast }


patternAndGuard: 
  | parenPattern patternGuard 
      { $1, $2, rhs parseState 1 }
      
patternClauses: 
  | patternAndGuard patternResult %prec prec_pat_pat_action
     { let pat,guard,patm = $1 in 
       let mLast = range_of_synexpr $2 in 
       [Clause(pat,guard,$2,patm,SequencePointAtTarget)], mLast  }
  | patternAndGuard patternResult BAR patternClauses 
     { let pat,guard,patm = $1 in 
       let clauses,mLast = $4 in 
       (Clause(pat,guard,$2,patm,SequencePointAtTarget) :: clauses), mLast }
  | patternAndGuard patternResult BAR error 
     { let pat,guard,patm = $1 in 
       let mLast = rhs parseState 3 in 
       (* silent recovery *)
       [Clause(pat,guard,$2,patm,SequencePointAtTarget)], mLast  }
  | patternAndGuard patternResult error 
     { let pat,guard,patm = $1 in 
       let mLast = range_of_synexpr $2 in 
       (* silent recovery *)
       [Clause(pat,guard,$2,patm,SequencePointAtTarget)], mLast }
 
patternGuard: 
  | WHEN declExpr 
     { Some $2 }
  | 
     { None }

patternResult: 
  | RARROW typedSeqExprBlockR  
     { $2 }

ifExprCases: 
  | ifExprThen ifExprElifs 
      { let exprThen,mThen = $1 in 
        (fun exprGuard mIf -> 
            let mIfToThen = union_ranges mIf mThen in
            let mIfToEndOfElseBranch = union_ranges mIf (range_of_synexpr (match $2 with None -> exprThen | Some e -> e)) in
            let spIfToThen = SequencePointAtBinding(mIfToThen) in
            Expr_cond(exprGuard,exprThen,$2,spIfToThen,mIfToThen,mIfToEndOfElseBranch)) }

ifExprThen: 
  | THEN  declExpr %prec prec_then_if { $2, rhs parseState 1 }
  | OTHEN  OBLOCKBEGIN typedSeqExpr OBLOCKEND %prec prec_then_if { $3,rhs parseState 1 }
  | OTHEN  OBLOCKBEGIN typedSeqExpr recover %prec prec_then_if { $3,rhs parseState 1 }

ifExprElifs: 
  | 
      { None }
  | ELSE declExpr 
      { Some $2 }
  | OELSE  OBLOCKBEGIN typedSeqExpr OBLOCKEND 
      { Some $3 }
  | OELSE  OBLOCKBEGIN typedSeqExpr recover 
      { Some $3 }
  | ELIF declExpr ifExprCases 
      { let mElif = rhs parseState 1 in 
        Some ($3 $2 mElif) }

tupleExpr: 
  | tupleExpr COMMA declExpr   
      { $3 :: $1 }
  | declExpr COMMA declExpr  
      { $3 :: $1 :: [] }

minusExpr: 
  | MINUS minusExpr   %prec expr_prefix_plus_minus
      { mksyn_prefix (rhs parseState 1) (union_ranges (rhs parseState 1) (range_of_synexpr $2)) "~-" $2 }
  | PLUS_MINUS_OP minusExpr  
      { mksyn_prefix (rhs parseState 1) (union_ranges (rhs parseState 1) (range_of_synexpr $2)) ("~"^($1)) $2 } 
  | ADJACENT_PREFIX_PLUS_MINUS_OP minusExpr 
      { mksyn_prefix (rhs parseState 1) (union_ranges (rhs parseState 1) (range_of_synexpr $2)) ("~"^($1)) $2 }
  | SPLICE_SYMBOL minusExpr
      { mksyn_prefix (rhs parseState 1) (union_ranges (rhs parseState 1) (range_of_synexpr $2)) $1 $2 }
  | PERCENT_OP minusExpr
      { mksyn_prefix (rhs parseState 1) (union_ranges (rhs parseState 1) (range_of_synexpr $2)) ("~"^($1)) $2 }
  | AMP  minusExpr    
      { Expr_addrof(true,$2,rhs parseState 1,union_ranges (rhs parseState 1) (range_of_synexpr $2)) } 
  | AMP_AMP  minusExpr   
      { Expr_addrof(false,$2,rhs parseState 1, union_ranges (rhs parseState 1) (range_of_synexpr $2)) } 
  | NEW appType  opt_HIGH_PRECEDENCE_APP atomicExprAfterType 
      { Expr_new(false,$2,$4,union_ranges (rhs parseState 1) (range_of_synexpr $4)) }
  | NEW appType opt_HIGH_PRECEDENCE_APP error   
      { Expr_new(false,$2,arbExpr(parseState),union_ranges (rhs parseState 1) (range_of_syntype $2)) }
  | UPCAST  minusExpr 
      { Expr_inferred_upcast($2,union_ranges (rhs parseState 1) (range_of_synexpr $2)) }   
  | DOWNCAST  minusExpr 
      { Expr_inferred_downcast($2,union_ranges (rhs parseState 1) (range_of_synexpr $2))}   
  | appExpr 
      { $1 }

appExpr:
  | appExpr argExpr %prec expr_app
      { Expr_app (ExprAtomicFlag.NonAtomic, $1,$2,union_ranges (range_of_synexpr $1) (range_of_synexpr $2))  }
  | atomicExpr 
      { let arg,_ = $1 in 
        arg }

argExpr:
  | ADJACENT_PREFIX_PLUS_MINUS_OP atomicExpr 
      { let arg2,hpa2 = $2 in 
        if hpa2 then reportParseErrorAt (rhs parseState 1) "Successive arguments should be separated by spaces or tupled, and arguments involving function or method applications should be parenthesized";
        mksyn_prefix (rhs parseState 1) (union_ranges (rhs parseState 1) (range_of_synexpr arg2)) ("~"^($1)) arg2 }
   | atomicExpr 
      { let arg,hpa = $1 in 
        if hpa then reportParseErrorAt (range_of_synexpr arg) "Successive arguments should be separated by spaces or tupled, and arguments involving function or method applications should be parenthesized";
        arg }
    
    
atomicExpr:
  | atomicExpr HIGH_PRECEDENCE_APP atomicExpr
      { let arg1,_ = $1 in 
        let arg2,_ = $3 in 
        Expr_app (ExprAtomicFlag.Atomic, arg1,arg2,union_ranges (range_of_synexpr arg1) (range_of_synexpr arg2)),true  }

  | atomicExpr HIGH_PRECEDENCE_TYAPP typeArgsActual
      { let arg1,_ = $1 in 
        Expr_tyapp (arg1,$3,lhs(parseState)),false }

  | PREFIX_OP  atomicExpr  
      { let arg2,hpa2 = $2 in 
        mksyn_prefix (rhs parseState 1) (union_ranges (rhs parseState 1) (range_of_synexpr arg2)) $1 arg2,hpa2 }
  | atomicExpr DOT atomicExprQualification 
      { let arg1,hpa1 = $1 in 
        $3 arg1 (lhs(parseState)) (rhs parseState 2),hpa1 }
  | BASE DOT atomicExprQualification 
      { let arg1 = Expr_id_get(ident("base",rhs parseState 1)) in
        $3 arg1 (lhs(parseState)) (rhs parseState 2),false }
  | QMARK nameop 
      { Expr_lid_get (true,[$2],rhs parseState 2),false }
  | atomicExpr QMARK dynamicArg
      { let arg1,hpa1 = $1 in
        mksyn_infix (rhs parseState 2) (lhs(parseState)) arg1 "?" $3, hpa1 }
  | nameop
      { Expr_id_get ($1),false }
  | LBRACK listExprElements RBRACK 
      { MatchPair parseState 1 3; 
        $2 (lhs(parseState)) false,false }
  | LBRACK listExprElements recover 
      { reportParseErrorAt (rhs parseState 1) "unmatched '['"; 
        $2 (rhs2 parseState 1 2) false, false }
  | LBRACK error RBRACK 
      { MatchPair parseState 1 3; 
        (* silent recovery *) 
        Expr_array_or_list(false,[ ], lhs(parseState)),false  } 
  | atomicExprAfterType 
      { $1,false }

atomicExprQualification:
  |    identop 
      { let idm = rhs parseState 1 in 
        (fun e lhsm dotm -> mksyn_dot lhsm e $1) }
  |  recover 
      { (fun e lhsm dotm -> 
            reportParseErrorAt dotm "missing qualification after '.'"; 
            // Include 'e' in the returned expression but throw it away
            Expr_throwaway(e,lhsm)) }
  |   INT32 
      { (fun e lhsm dotm -> 
            libraryOnlyWarning (lhs(parseState));
            mksyn_dotn lhsm e (fst $1)) }
  |   LPAREN COLON_COLON RPAREN DOT INT32  
      { (fun e lhsm dotm -> 
            libraryOnlyWarning(lhs(parseState));
            Expr_constr_field_get (e,mksyn_constr lhsm opname_Cons,(fst $5),lhsm)) }
  |   LPAREN  typedSeqExpr RPAREN  
      { MatchPair parseState 1 3; 
        (fun e lhsm dotm -> 
            ocamlCompatWarning "The expression form 'expr.(expr)' is for use when OCaml compatibility is enabled. In F# code you may use 'expr.[expr]'. A type annotation may be required to indicate the first expression is an array" (lhs(parseState)); 
            mksyn_dot_lparen_get lhsm e $2) }
  |   LBRACK  typedSeqExpr RBRACK  
      { MatchPair parseState 1 3; 
        (fun e lhsm dotm -> mksyn_dot_lbrack_get lhsm dotm e $2) }

  |   LBRACK  optRange RBRACK  
      { MatchPair parseState 1 3; 
        (fun e lhsm dotm -> mksyn_dot_lbrack_slice_get lhsm dotm e $2) }
  |   LBRACK  optRange COMMA optRange RBRACK  %prec slice_comma
      { MatchPair parseState 1 5; 
        (fun e lhsm dotm -> mksyn_dot_lbrack_slice2_get lhsm dotm e $2 $4) }
  |   LBRACK  optRange COMMA optRange COMMA optRange RBRACK  %prec slice_comma
      { MatchPair parseState 1 7; 
        (fun e lhsm dotm -> mksyn_dot_lbrack_slice3_get lhsm dotm e $2 $4 $6) }
  |   LBRACK  optRange COMMA optRange COMMA optRange COMMA optRange RBRACK  %prec slice_comma
      { MatchPair parseState 1 9; 
        (fun e lhsm dotm -> mksyn_dot_lbrack_slice4_get lhsm dotm e $2 $4 $6 $8) }

optRange:
  | declExpr DOT_DOT declExpr { mk_optional (rhs parseState 1) (Some $1), mk_optional (rhs parseState 3) (Some $3) }
  | declExpr DOT_DOT { mk_optional (rhs parseState 1) (Some $1), mk_optional (rhs parseState 2) None }
  | DOT_DOT declExpr { mk_optional (rhs parseState 1) None, mk_optional (rhs parseState 2) (Some $2) }
  | STAR { mk_optional (rhs parseState 1) None, mk_optional (rhs parseState 1) None }
  

/* the start et of atomicExprAfterType must not overlap with the valid postfix tokens of the type syntax, e.g. new List<T>(...) */
atomicExprAfterType:
  | constant 
      { Expr_const ($1,range_of_synconst $1 (lhs(parseState))) }
  | parenExpr 
      { $1 }
  | braceExpr 
      { $1 }
  | NULL 
      { Expr_null(lhs(parseState)) } 
  | FALSE  
      { Expr_const(Const_bool false,lhs(parseState)) } 
  | TRUE  
      { Expr_const(Const_bool true,lhs(parseState)) } 
  | quoteExpr
      { $1 }
  | arrayExpr
      { $1 }
  | beginEndExpr
      { $1 }
  
beginEndExpr:
  | BEGIN typedSeqExpr END 
      { Expr_paren($2,rhs2 parseState 1 3) } 
  | BEGIN typedSeqExpr recover 
      { reportParseErrorAt (rhs parseState 1) "unmatched 'begin'"; $2 } 
  | BEGIN error END 
      { (* silent recovery *) arbExpr(parseState)  } 
  | BEGIN END 
      { mksyn_unit (lhs(parseState)) } 

quoteExpr:
  | LQUOTE typedSeqExpr RQUOTE 
      { MatchPair parseState 1 3; 
        if $1 <> $3 then reportParseErrorAt (rhs parseState 1) ("mismatched quotation, beginning with '"^ fst $1 ^ "'");  
        (Expr_quote(mksyn_item (lhs(parseState)) (CompileOpName (fst $1)), snd $1, $2,lhs(parseState))) } 
  | LQUOTE typedSeqExpr recover 
      { reportParseErrorAt (rhs parseState 1) ("unmatched '"^fst $1^"'");  
        // Note: deliberately use this smaller range for the expression: see FSHarp 1.0 bug 3225
        let mExpr = rhs2 parseState 1 1 in
        Expr_quote(mksyn_item (lhs(parseState)) (CompileOpName (fst $1)),snd $1, $2,mExpr)  } 
  | LQUOTE error RQUOTE 
      { MatchPair parseState 1 3; (* silent recovery *) Expr_quote(mksyn_item (lhs(parseState)) (CompileOpName (fst $1)),snd $1, arbExpr(parseState),lhs(parseState))  }  

arrayExpr:
  | LBRACK_BAR listExprElements BAR_RBRACK 
      {  MatchPair parseState 1 3; $2 (lhs(parseState)) true } 
  | LBRACK_BAR listExprElements recover 
      { reportParseErrorAt (rhs parseState 1) "unmatched '[|'"; 
        $2 (rhs2 parseState 1 2) true}
  | LBRACK_BAR error BAR_RBRACK 
      {  MatchPair parseState 1 3; (* silent recovery *) Expr_array_or_list(true,[ ], lhs(parseState)) }  

parenExpr:
  | LPAREN parenExprBody RPAREN 
      { MatchPair parseState 1 3; $2 (rhs2 parseState 1 3) }
  | LPAREN parenExprBody recover 
      { reportParseErrorAt (rhs parseState 1) "unmatched '('"; let lhsm = rhs2 parseState 1 2 in Expr_paren($2 lhsm,lhsm) }
  | LPAREN error RPAREN 
      { MatchPair parseState 1 3; (* silent recovery *) arbExpr(parseState) }
  | LPAREN recover %prec prec_atomexpr_lparen_error 
      { reportParseErrorAt (rhs parseState 1) "unmatched '('"; arbExpr(parseState)  }  

parenExprBody:
  |   
      {  (fun m -> Expr_const(Const_unit,m)) } 
  | TYPE typ  
      {  (fun  m -> Expr_typeof($2,m)) }
  | staticallyKnownHeadTypars COLON LPAREN classMemberSpfn RPAREN  typedSeqExpr 
      {  MatchPair parseState 3 5;  
         MatchPair parseState 6 8; 
         (fun m -> Expr_trait_call($1,$4,$6,m)) } /* disambiguate: x $a.id(x) */
  | typedSeqExpr
      { (fun m -> Expr_paren($1,m)) } 
  | inlineAssemblyExpr 
      { $1 }

staticallyKnownHeadTypars:
  | staticallyKnownHeadTypar { [$1] }
  | LPAREN staticallyKnownHeadTypar OR staticallyKnownHeadTypar RPAREN { [$2 ; $4 ] }

braceExpr:
  | LBRACE braceExprBody RBRACE 
     {  MatchPair parseState 1 3; $2 (lhs(parseState)) }
  | LBRACE braceExprBody recover 
     { reportParseErrorAt (rhs parseState 1) "unmatched '{'" ; $2 (lhs(parseState)) }  
  | LBRACE error RBRACE 
     { MatchPair parseState 1 3; (* silent recovery *) arbExpr(parseState)  }  

braceExprBody:
  | recdExpr 
     {  (fun m -> let a,b,c = $1 in Expr_recd(a,b,c,m)) }
  | objExpr 
     { $1 }
  | monadicExprInitial 
     { $1 false }

listExprElements: 
  | monadicExprInitial
     { (fun lhsm isArray -> Expr_array_or_list_of_seq(isArray, $1 true lhsm,lhsm)) }
  | 
     { (fun lhsm isArray -> Expr_array_or_list(isArray,[ ], lhsm)) }

monadicExprInitial: 
  | seqExpr
     { (fun isArrayOrList lhsm -> Expr_comprehension(isArrayOrList,ref(isArrayOrList),$1,lhsm)) }
  | rangeSequenceExpr 
     { $1 }
  
rangeSequenceExpr: 
  | declExpr TO       declExpr  %prec expr_let
     { deprecatedWithError "use 'expr .. expr' instead" (lhs(parseState)); (fun _ m -> mksyn_infix m m $1 ".." $3) }
  | declExpr DOT_DOT  declExpr  
     { (fun _ m -> mksyn_infix m m $1 ".." $3) }
  | declExpr DOT_DOT  declExpr DOT_DOT declExpr  
     { (fun _ m -> mksyn_trifix m ".. .." $1 $3 $5) }


/* Allow a naked yield (no "yield" or "return" or "->") immediately after a "->" */
/* Allow a naked yield (no "yield!" or "return!" or "->>") immediately after a "->>" */
/* In both cases multiple 'for' and 'when' bindings can precede */
monadicSingleLineQualifiersThenArrowThenExprR:
  | RARROW typedSeqExprBlockR 
     { (fun m -> Comp_yield((true,false),$2,m)) } 
  | RARROW2 typedSeqExprBlockR 
     { let mAll = union_ranges (rhs parseState 1) (range_of_synexpr $2) in
       deprecatedWithError "The expression form '->>' in sequence expressions has been removed from the F# language. Use the syntax 'yield! ...' to generate multiple elements in sequence expressions" mAll;
       (fun m -> Comp_yieldm((true,false),$2,m)) } 

  | FOR forLoopBinder opt_OBLOCKSEP monadicSingleLineQualifiersThenArrowThenExprR %prec decl_let 
     { deprecatedWithError "Nested 'for' loops in sequence expressions should be written 'for x in <collection1> do for y in <collection2> do ...yield <result>" (rhs2 parseState 1 2);
       let spBind = SequencePointAtForLoop(rhs2 parseState 1 2) in
       let a2,b2= $2 in 
       (fun m -> 
           Expr_foreach(spBind,SeqExprOnly(true),a2,b2,$4 m,m)) } 

  | monadicWhenCondition opt_OBLOCKSEP monadicSingleLineQualifiersThenArrowThenExprR %prec decl_let 
     { let mWhenAndGuard = range_of_synexpr $1 in
       deprecatedWithError "'when' conditions in sequence expressions have been removed from the F# language. Use 'for x in <collection> do if <condition> then ...yield <result>" mWhenAndGuard;
       let spWhenAndGuard = SequencePointAtBinding(mWhenAndGuard) in
       (fun m -> 
           Expr_cond($1,$3 m,None,spWhenAndGuard,mWhenAndGuard,m)) } 


forLoopBinder: 
  | parenPattern IN declExpr 
     { ($1, $3) }
  | parenPattern IN rangeSequenceExpr 
     { ($1, $3 false (rhs parseState 3)) }
  | parenPattern IN recover
     { ($1, arbExpr(parseState)) }

forLoopRange: 
  | parenPattern EQUALS declExpr  direction  declExpr { id_of_pat (rhs parseState 1) $1,$3,$4,$5 }

inlineAssemblyExpr:
  |  HASH STRING opt_inlineAssemblyTypeArg opt_curriedArgExprs  opt_inlineAssemblyReturnTypes opt_HASH 
      { libraryOnlyWarning (lhs(parseState));
        let s,sm = $2,rhs parseState 2 in
        (fun m -> Expr_asm (ParseAssemblyCodeInstructions s sm,$3,List.rev $4,$5,m)) }
  
opt_curriedArgExprs: 
  | opt_curriedArgExprs argExpr  %prec expr_args { $2 :: $1 } 
  |  { [] }

opt_atomicExprAfterType: 
  |  { None }
  |  atomicExprAfterType { Some($1) }

opt_inlineAssemblyTypeArg:
  |  { [] }
  | TYPE LPAREN typ RPAREN  {  MatchPair parseState 2 4; [$3] }

opt_inlineAssemblyReturnTypes:
  |  
     { [] }
  | COLON typ 
     { [$2] }
  | COLON LPAREN RPAREN  
     {  MatchPair parseState 2 3; [] }

recdExpr:
  | 
     { (None,None, []) }
  | INHERIT appType opt_HIGH_PRECEDENCE_APP opt_atomicExprAfterType recdExprBindings opt_seps
     { let arg = match $4 with None -> mksyn_unit (lhs(parseState)) | Some e -> e in 
       (Some($2,arg,rhs2 parseState 2 4),None, $5) }
/* REVIEW: we shouldn't really be permitting appExpr here if we want to minimize the number of "entries" into the expression */
/* syntax. OCaml only permits atomicExpr here */
  | appExpr EQUALS declExpr recdExprBindings opt_seps
     { match $1 with 
       | Expr_lid_or_id_get(false,v,m) -> (None,None, (List.frontAndBack v,$3) :: List.rev $4) 
       | _ -> raiseParseErrorAt (rhs parseState 2) "field bindings must have the form 'id = expr;'" }
  | appExpr WITH path EQUALS  declExpr recdExprBindings  opt_seps
     {  (None,Some $1,(List.frontAndBack $3,$5):: List.rev $6) }
  | appExpr OWITH path EQUALS  declExpr recdExprBindings  opt_seps OEND
     {  (None,Some $1,(List.frontAndBack $3,$5):: List.rev $6) }

recdExprBindings: 
  | recdExprBindings seps path EQUALS declExpr { (List.frontAndBack $3,$5) :: $1 } 
  |                                            { [] }

/* There is a minor conflict between
       seq { new ty() }  // sequence expression with one very odd 'action' expression
  and 
       { new ty() }   // object expression with no interfaces and no overrides
Hence we make sure the latter is not permitted by the grammar
*/
objExpr:
  | objExprBaseCall objExprBindings opt_OBLOCKSEP opt_objExprInterfaces
     { (fun m -> let (a,b) = $1 in Expr_impl(a,b,$2,$4, m)) }
  | objExprBaseCall opt_OBLOCKSEP objExprInterfaces
     { (fun m -> let (a,b) = $1 in Expr_impl(a,b,[],$3, m)) }
  | NEW appType
     { (fun m -> let (a,b) = $2,None in Expr_impl(a,b,[],[], m)) }

objExprBaseCall:
  | NEW appType  opt_HIGH_PRECEDENCE_APP atomicExprAfterType baseSpec
     { ($2, Some($4,Some($5))) }
  | NEW appType  opt_HIGH_PRECEDENCE_APP atomicExprAfterType 
     { ($2, Some($4,None)) }
  | NEW appType
     { $2,None }
 


opt_objExprBindings: 
  | objExprBindings { $1 }
  |                 { [] }

objExprBindings: 
  | WITH localBindings 
      { let letm = (rhs parseState 1) in
        ($2 [] None letm) }
  | OWITH localBindings OEND
      { let letm = (rhs parseState 1) in
        ($2 [] None letm) }
  | WITH objectImplementationBlock opt_decl_end
      { $2 |> 
        (List.choose (function ClassMemberDefn_member_binding(b,m) -> Some b
                          | ClassMemberDefn_implicit_inherit (_, _, _, m)
                          | ClassMemberDefn_implicit_ctor (_,_,_, _, m)
                          | ClassMemberDefn_let_bindings(_,_,_,m)                    
                          | ClassMemberDefn_slotsig(_,_,m) 
                          | ClassMemberDefn_interface(_,_,m) 
                          | ClassMemberDefn_inherit(_,_,m)
                          | ClassMemberDefn_field(_,m)
                          | ClassMemberDefn_open(_,m)
                          | ClassMemberDefn_tycon(_,_,m) -> errorR(Error("This member is not permitted in an object implementation",m)); None)) }

objExprInterfaces:
  | objExprInterface opt_objExprInterfaces { $1 :: $2 }

opt_objExprInterfaces:
  | %prec prec_interfaces_prefix { [] }
  | objExprInterface opt_objExprInterfaces { $1 :: $2 }
  | error opt_objExprInterfaces { (* silent recovery *) $2 }

objExprInterface:
  |  interfaceMember appType opt_objExprBindings opt_decl_end opt_OBLOCKSEP
    { InterfaceImpl($2, $3, lhs(parseState)) }

direction: 
  | TO     { true } 
  | DOWNTO { false }


anonLambdaExpr: 
  | FUN atomicPatterns RARROW typedSeqExprBlock 
     { let mAll = union_ranges (rhs parseState 1) (range_of_synexpr $4) in
       mksyn_fun_match_lambdas false mAll $2 $4 }
  | FUN atomicPatterns RARROW error
     { let mAll = rhs2 parseState 1 3 in
       mksyn_fun_match_lambdas false mAll $2 (arbExpr(parseState)) }
  | OFUN atomicPatterns RARROW typedSeqExprBlockR OEND
     { let mAll = union_ranges (rhs parseState 1) (range_of_synexpr $4) in
       mksyn_fun_match_lambdas false mAll $2 $4 }
  | OFUN atomicPatterns RARROW ORIGHT_BLOCK_END OEND
     { reportParseErrorAt (rhs2 parseState 1 3) "missing function body" ;
       mksyn_fun_match_lambdas false (rhs2 parseState 1 3) $2 (arbExpr(parseState)) }

  | OFUN atomicPatterns RARROW recover
     { reportParseErrorAt (rhs2 parseState 1 3) "missing function body" ;
       mksyn_fun_match_lambdas false (rhs2 parseState 1 3) $2 (arbExpr(parseState)) }

anonMatchingExpr: 
  | FUNCTION opt_bar patternClauses  %prec expr_function
      { let clauses,mLast = $3 in
        let mAll = union_ranges (rhs parseState 1) mLast in
        mksyn_match_lambda(false,false,mAll,clauses,NoSequencePointAtInvisibleBinding) }
  | OFUNCTION opt_bar patternClauses  OEND %prec expr_function
      { let clauses,mLast = $3 in
        let mAll = union_ranges (rhs parseState 1) mLast in
        mksyn_match_lambda(false,false,mAll,clauses,NoSequencePointAtInvisibleBinding) }
  | OFUNCTION opt_bar patternClauses  error OEND %prec expr_function
      { reportParseErrorAt (rhs parseState 1) "error in 'function' block"; 
        let clauses,mLast = $3 in
        let mAll = union_ranges (rhs parseState 1) mLast in
        mksyn_match_lambda(false,false,mAll,clauses,NoSequencePointAtInvisibleBinding) }

/*--------------------------------------------------------------------------*/
/* TYPE ALGEBRA                                                             */

typeWithTypeConstraints:
  | typ %prec prec_wheretyp_prefix { $1 }
  | typ WHEN typeConstraints 
     { Type_with_global_constraints($1, List.rev $3,lhs(parseState)) }

topTypeWithTypeConstraints: 
  | topType 
     { $1 }
  | topType WHEN typeConstraints 
     { let ty,arity = $1 in 
        (* nb. it doesn't matter where the constraints go in the structure of the type. *)
        Type_with_global_constraints(ty,List.rev $3,lhs(parseState)), arity }

opt_topReturnTypeWithTypeConstraints: 
  |             
     { None } 
  | COLON topTypeWithTypeConstraints 
     { let ty,arity = $2 in 
       let arity = (match arity with ValSynInfo([],rmdata)-> rmdata | _ -> SynInfo.unnamedRetVal) in
       Some ((ty,arity),rhs parseState 2) }

topType: 
  | topTupleType RARROW topType 
     { let dty,dmdata= $1 in 
       let rty,(ValSynInfo(dmdatas,rmdata)) = $3 in 
       Type_fun(dty,rty,lhs(parseState)), (ValSynInfo(dmdata::dmdatas, rmdata)) }
  | topTupleType 
     { let ty,rmdata = $1 in ty, (ValSynInfo([],(match rmdata with [md] -> md | _ -> SynInfo.unnamedRetVal))) }

topTupleType:
  | topAppType STAR topTupleTypeElements 
     { let ty,mdata = $1 in let tys,mdatas = List.unzip $3 in (Type_tuple(List.map (fun ty -> (false,ty)) (ty ::tys), lhs(parseState))),(mdata :: mdatas) }
  | topAppType                 
     { let ty,mdata = $1 in ty,[mdata] }

topTupleTypeElements:
  | topAppType STAR topTupleTypeElements       { $1 :: $3 }
  | topAppType %prec prec_toptuptyptail_prefix { [$1] }

/* REVIEW: why can't we use opt_attributes here? */
topAppType:
  | attributes appType COLON appType 
     { match $2 with 
       | Type_lid([id],_) -> $4,ArgSynInfo($1,false,Some id)
       | _ -> raiseParseErrorAt (rhs parseState 2) "syntax error in labelled type argument"  }
  | attributes QMARK ident COLON appType 
     { $5,ArgSynInfo($1,true,Some $3) }
  | attributes appType 
     { ($2,ArgSynInfo($1,false,None)) }
  | appType COLON appType 
     { match $1 with 
       | Type_lid([id],_) -> $3,ArgSynInfo([],false,Some id)
       | _ -> raiseParseErrorAt (rhs parseState 2) "syntax error in labelled type argument"  }
  | QMARK ident COLON appType 
     { $4,ArgSynInfo([],true,Some $2) }
  | appType 
     { $1,ArgSynInfo([],false,None) }

polyType: 
  | typar DOT typ 
      { deprecatedWithError "OCaml-style polymorphic record fields are deprecated and will be removed in a future release of the language. Consider using an interface type with a generic method instead" (lhs(parseState));
        Type_forall(TyparDecl([],$1),$3,lhs(parseState)) }
  | typ { $1 }

typ:
  | tupleType RARROW typ  { Type_fun($1,$3,lhs(parseState)) }
  | tupleType %prec prec_typ_prefix { $1 }


tupleType:
  | appType STAR tupleOrQuotTypeElements { Type_tuple((false,$1) :: $3,lhs(parseState)) }

  | INFIX_STAR_DIV_MOD_OP tupleOrQuotTypeElements
    { if $1 <> "/" then reportParseErrorAt (rhs parseState 1) "Unexpected infix operator in type expression";
      Type_tuple((true, Type_dimensionless (lhs(parseState))):: $2, lhs(parseState)) }

  | appType INFIX_STAR_DIV_MOD_OP tupleOrQuotTypeElements
      { if $2 <> "/" then reportParseErrorAt (rhs parseState 1) "Unexpected infix operator in type expression";
        Type_tuple((true,$1) :: $3, lhs(parseState)) }
  | appType %prec prec_tuptyp_prefix { $1 }

tupleOrQuotTypeElements:
  | appType STAR tupleOrQuotTypeElements              { (false,$1) :: $3 }
  | appType INFIX_STAR_DIV_MOD_OP tupleOrQuotTypeElements 
      { if $2 <> "/" then reportParseErrorAt (rhs parseState 1) "Unexpected infix operator in type expression";
        (true,$1) :: $3 }
  | appType %prec prec_tuptyptail_prefix { [(false,$1)] }

tupleTypeElements:
  | appType STAR tupleTypeElements              { $1 :: $3 }
  | appType %prec prec_tuptyptail_prefix { [$1] }

appTypeCon:
  | path %prec prec_atomtyp_path 
      { Type_lid($1, lhs(parseState)) }

  | typar 
     { Type_var($1, lhs(parseState)) }

appTypeConPower:
  | appTypeCon INFIX_AT_HAT_OP INT32
     { if $2 = "^-" then Type_power($1, -(fst $3), lhs(parseState))
       else Type_power($1, fst $3, lhs(parseState)) }
  | appTypeCon 
    { $1 }

appType:
  | appType arrayTypeSuffix 
      {  Type_arr($2,$1,lhs(parseState)) }
  | appType HIGH_PRECEDENCE_APP arrayTypeSuffix 
      {  Type_arr($3,$1,lhs(parseState)) }
  | appType appTypeConPower  
      { Type_app($2,[$1],true,lhs(parseState)) }
  | appType LAZY  
      { deprecatedWithError "The use of 'typ lazy' as a type is deprecated. Use 'Lazy<typ>' instead" (rhs parseState 2);
        Type_lazy($1,lhs(parseState)) }
  | LPAREN appTypePrexifArguments RPAREN  appTypeConPower
      { ocamlCompatWarning "The syntax '(typ,...,typ) ident' for multi-argument generic type instantiations is only recommended if OCaml compatibility is enabled. Consider using 'ident<typ,...,typ>' instead" (lhs(parseState)); 
        MatchPair parseState 1 3; 
        Type_app($4,$2, true, lhs(parseState)) }
  | powerType 
      { $1 }
  | typar      COLON_GREATER typ                     
      {  let tp,typ = $1,$3 in 
         let m = lhs(parseState) in 
         Type_with_global_constraints(Type_var (tp, rhs parseState 1), [WhereTyparSubtypeOfType(tp,typ,m)],m)  }
  | UNDERSCORE COLON_GREATER typ %prec COLON_GREATER 
      {  MatchPair parseState 1 3; mksyn_anon_constraint $3 (lhs(parseState)) }

arrayTypeSuffix:
  | LBRACK RBRACK 
      {  MatchPair parseState 1 2; 1 }
  | LBRACK COMMA RBRACK 
      {  MatchPair parseState 1 3; 2 }
  | LBRACK COMMA COMMA RBRACK 
      {  MatchPair parseState 1 4; 3 }
  | LBRACK COMMA COMMA COMMA RBRACK 
      {  MatchPair parseState 1 5; 4 }

appTypePrexifArguments:
  | typ COMMA typ typeListElements { $1 :: $3 :: List.rev $4 }

typeListElements: 
  | typeListElements COMMA typ { $3 :: $1 } 
  |                      { [] }

powerType:
  | atomType
    { $1 }
  | atomType INFIX_AT_HAT_OP INT32
     { if $2 <> "^" && $2 <> "^-" then reportParseErrorAt (rhs parseState 2) "Unexpected infix operator in type expression";
       if $2 = "^-" then Type_power($1, - (fst $3), lhs(parseState))
       else Type_power($1, fst $3, lhs(parseState)) }
  | atomType INFIX_AT_HAT_OP MINUS INT32
     { if $2 <> "^" then reportParseErrorAt (rhs parseState 2) "Unexpected infix operator in type expression";
       Type_power($1, - (fst $4), lhs(parseState)) }

atomType:
  | HASH atomType 
     { mksyn_anon_constraint $2 (lhs(parseState)) }
  | appTypeConPower %prec prec_atomtyp_path 
     { $1 }
  | UNDERSCORE 
     { Type_anon (lhs(parseState)) }
  | LPAREN typ RPAREN 
     {  MatchPair parseState 1 3; $2 }
  | LPAREN typ recover      
     { reportParseErrorAt (rhs parseState 1) "unmatched '('" ; $2 }  
  | INT32
      { if fst $1 <> 1 then reportParseErrorAt (rhs parseState 1) "Unexpected integer literal in type expression";
        Type_dimensionless (lhs(parseState))
      }
  | LPAREN error RPAREN   
     { (* silent recovery *) Type_anon (lhs(parseState)) }  
  | appTypeCon typeArgs %prec prec_atomtyp_path 
     { Type_app($1,$2,false,lhs(parseState)) } 
  | atomType DOT path %prec prec_atomtyp_get_path 
     { Type_proj_then_app($1,$3,[],lhs(parseState)) } 
  | atomType DOT path typeArgs %prec prec_atomtyp_get_path 
     { Type_proj_then_app($1,$3,$4,lhs(parseState)) } 


typeArgs:
  | typeArgsActual
     { $1 } 
  | HIGH_PRECEDENCE_TYAPP typeArgsActual 
     { $2 } 

typeArgsActual:
  | LESS GREATER 
     { [] } 
  | LESS typ GREATER 
     { [$2] } 
  | LESS typ COMMA typ typeListElements GREATER 
     { $2 :: $4 :: List.rev $5 } 

measurearg:
  | LESS measure GREATER
     { $2 }
  | LESS UNDERSCORE GREATER
     { Measure_Anon (lhs(parseState)) }

measureatom:
  | path 
     { Measure_Con($1, lhs(parseState)) }

  | typar 
     { Measure_Var($1, lhs(parseState)) }

  | LPAREN measure RPAREN
     { $2 }

measurepower:
  | measureatom 
      { $1 }

  | measureatom INFIX_AT_HAT_OP INT32
     { if $2 <> "^" && $2 <> "^-" then reportParseErrorAt (rhs parseState 2) "Unexpected infix operator in unit-of-measure expression. Legal operators are '*', '/' and '^'";
       if $2 = "^-" then Measure_Power($1, - (fst $3), lhs(parseState))
       else Measure_Power($1, fst $3, lhs(parseState)) }

  | measureatom INFIX_AT_HAT_OP MINUS INT32
     { if $2 <> "^" then reportParseErrorAt (rhs parseState 2) "Unexpected infix operator in unit-of-measure expression. Legal operators are '*', '/' and '^'";
       Measure_Power($1, - (fst $4), lhs(parseState)) }

  | INT32
     { if fst $1 <> 1 then reportParseErrorAt (rhs parseState 1) "Unexpected integer literal in unit-of-measure expression";
       Measure_One }

measureseq:
  | measurepower
    { [$1] }
  | measurepower measureseq
    { $1 :: $2 }

measure:
  | measureseq
    { Measure_Seq($1, lhs(parseState)) }
  | measure STAR measure
    { Measure_Prod($1, $3, lhs(parseState)) }
  | measure INFIX_STAR_DIV_MOD_OP measure
    { if $2 <> "*" && $2 <> "/" then reportParseErrorAt (rhs parseState 2) "Unexpected infix operator in unit-of-measure expression. Legal operators are '*', '/' and '^'";
      if $2 = "*" then Measure_Prod($1, $3, lhs(parseState))
      else Measure_Quot($1, $3, lhs(parseState)) }
  | INFIX_STAR_DIV_MOD_OP measure
     { if $1 <> "/" then reportParseErrorAt (rhs parseState 1) "Unexpected operator in unit-of-measure expression. Legal operators are '*', '/' and '^'";
       Measure_Quot(Measure_One, $2, lhs(parseState)) }
   
typar: 
  | QUOTE ident 
     {  let id = mksyn_id (lhs(parseState)) ($2).idText in
        Typar(id ,NoStaticReq,false) }
/*
  | DOLLAR ident 
     {  libraryOnlyWarning (lhs(parseState)); 
        let id = mksyn_id (lhs(parseState)) ($2).idText in
        Typar(id,CompleteStaticReq,false) }
*/
  | staticallyKnownHeadTypar 
     { $1 }

staticallyKnownHeadTypar: 
  | INFIX_AT_HAT_OP ident 
    {  if $1 <> "^" then reportParseErrorAt (rhs parseState 1) "syntax error: unexpeced type paramter specification";
       Typar($2,HeadTypeStaticReq,false) }

  

ident: 
  | IDENT 
     { ident($1,rhs parseState 1) } 

path: 
  | ident  
     { [$1] }
  | path DOT ident  
     { (* silent recovery *) $1 @ [$3] } 
  | path DOT error  
     { (* silent recovery *) $1  } 

opname: 
  | LPAREN operatorName RPAREN  
     {  MatchPair parseState 1 3; 
        ident(CompileOpName $2,rhs parseState 2) }
  | LPAREN_STAR_RPAREN
     {  MatchPair parseState 1 1; 
        ident(CompileOpName "*",rhs parseState 1) }

/* active pattern value names */
  | LPAREN barNames BAR RPAREN 
     { let text = ("|"^String.concat "|" (List.rev $2) ^ "|") in
       ident(text,rhs2 parseState 2 3) }
                         
  | LPAREN barNames BAR UNDERSCORE BAR RPAREN 
     { let text = ("|"^String.concat "|" (List.rev $2) ^ "|_|" ) in
       ident(text,rhs2 parseState 2 5) }

operatorName: 
  | PREFIX_OP { $1 }
  | INFIX_STAR_STAR_OP  { $1 }
  | INFIX_COMPARE_OP { $1 }
  | INFIX_AT_HAT_OP  { $1 }
  | INFIX_BAR_OP  { $1 }
  | INFIX_AMP_OP { $1 }
  | PLUS_MINUS_OP  { $1 }
  | INFIX_STAR_DIV_MOD_OP { $1 }
  | DOLLAR { "$" }
  | ADJACENT_PREFIX_PLUS_MINUS_OP { $1 }
  | MINUS { "-" }
  | STAR { "*" }
  | EQUALS { "=" }
  | OR { "or" }
  | LESS { "<" }
  | GREATER { ">" }
  | QMARK { "?" }
  | AMP { "&" }
  | AMP_AMP { "&&" }
  | BAR_BAR { "||" }
  | COLON_EQUALS { ":=" }
  | FUNKY_OPERATOR_NAME 
      { if $1 <> ".[]" then 
             deprecatedOperator (lhs(parseState)); 
        $1 }
  | SPLICE_SYMBOL { $1 }
  | PERCENT_OP { $1 }
  | DOT_DOT { (* deprecatedOperator (lhs(parseState)); *) ".." }
  | DOT_DOT DOT_DOT { (* deprecatedOperator (lhs(parseState)); *) ".. .." }
  | LQUOTE RQUOTE 
      { if $1 <> $2 then reportParseErrorAt (rhs parseState 1) ("mismatched quotation operator name, beginning with '"^fst $1^"'");  
        fst $1 } 

barName: 
  | IDENT 
      { if not (String.isUpper $1) then reportParseErrorAt (rhs parseState 1) ("active pattern case identifiers must begin with an uppercase letter");  
        $1 }

barNames: 
  | BAR barName
      { [$2] }
  | barNames BAR barName
      { $3 :: $1 }

identop: 
  | ident  
     { $1 } 
  | opname 
     { $1 }

/* path ending in an op */
pathop: 
  | ident  
     { [$1] }
  | opname 
     { [$1] }
  | ident DOT pathop 
     { $1 :: $3 } 
  | ident DOT error  
     { (* silent recovery *) [$1] }  


/* nameop is identop not used as part of a path */
nameop: 
  | identop  { $1 } 

top_sep: 
  | SEMICOLON { } 
  | SEMICOLON_SEMICOLON { }
  | OBLOCKSEP { }  

top_seps: 
  | top_sep                     { } 
  | top_sep top_seps { }

itop_sep: 
  | SEMICOLON { } 
  | OBLOCKSEP { }  

itop_seps: 
  | itop_sep                     { } 
  | itop_sep itop_seps { }

opt_itop_seps: 
  | itop_sep opt_itop_seps { }
  |                        { } 

opt_top_seps: 
  | top_sep opt_top_seps { }
  |                      { } 

seps: 
  | OBLOCKSEP { } 
  | SEMICOLON { }
  | OBLOCKSEP SEMICOLON { }
  | SEMICOLON OBLOCKSEP { }

/* An 'end' that's optional only in #light, where an ODECLEND gets inserted, and explicit 'end's get converted to OEND */
decl_end: 
  | ODECLEND 
      { } 
  | OEND 
      { (* reportParseWarningAt (rhs parseState 2) "this 'end' token is not needed in #light syntax and should  be omitted. A future release of the language may require this";  *)  }
  | END 
      {} 

/* An 'end' that's optional in both #light and #heavy */
opt_decl_end: 
  | ODECLEND 
      {} 
  | OEND 
      { (* reportParseWarningAt (rhs parseState 2) "this 'end' token is not needed in #light syntax and should be omitted. A future release of the language may require this";   *)  } 
  | END 
      {} 
  |     
      {} 

opt_ODECLEND: 
  | ODECLEND { } 
  |          { }

deprecated_opt_equals: 
  | EQUALS    { deprecatedWithError "No '=' symbol should follow a 'namespace' declaration" (lhs(parseState)) } 
  |           {  }

opt_OBLOCKSEP: 
  | OBLOCKSEP { }
  |          { } 

opt_seps: 
  | seps { }
  |      { } 

opt_rec: 
  | REC { true }
  |     { false } 

opt_bar: 
  | BAR { } 
  |     { } 

opt_inline: 
  | INLINE { true } 
  |        { false }

opt_mutable: 
  | MUTABLE { true } 
  |         { false }

do_or_odo: 
  | DO  { }
  | ODO { }

done_term: 
  | DONE { }
  | ODECLEND { }  /* DONE gets thrown away by the lexfilter in favour of ODECLEND */

structOrBegin: 
  | STRUCT { ocamlCompatWarning "The syntax 'module ... = struct .. end' is deprecated unless OCaml compatibility is enabled. Consider using 'module ... = begin .. end'" (lhs(parseState)); }
  | BEGIN { } 

sigOrBegin: 
  | SIG { ocamlCompatWarning "The syntax 'module ... : sig .. end' is deprecated unless OCaml compatibility is enabled. Consider using 'module ... = begin .. end'" (lhs(parseState)); }
  | BEGIN { } 

colonOrEquals: 
  | COLON { ocamlCompatWarning "The syntax 'module ... : sig .. end' is deprecated unless OCaml compatibility is enabled. Consider using 'module ... = begin .. end'" (lhs(parseState)); }
  | EQUALS { } 

opt_HASH: 
  | HASH {} 

opt_HIGH_PRECEDENCE_APP:
  | HIGH_PRECEDENCE_APP { }
  |    { }

opt_HIGH_PRECEDENCE_TYAPP:
  | HIGH_PRECEDENCE_TYAPP { }
  |    { }
