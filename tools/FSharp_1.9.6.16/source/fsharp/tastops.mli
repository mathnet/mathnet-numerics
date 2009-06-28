// (c) Microsoft Corporation 2005-2009. 

#light

/// Derived expression manipulation and construction functions.
module (* internal *) Microsoft.FSharp.Compiler.Tastops 

open Internal.Utilities
open Internal.Utilities.Pervasives
open System.Text
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Layout
open Microsoft.FSharp.Compiler.Lib

//-------------------------------------------------------------------------
// Build common types
//------------------------------------------------------------------------- 

val mk_fun_ty : typ -> typ -> typ
val ( --> ) : typ -> typ -> typ
val try_mk_forall_ty : typars -> typ -> typ
val ( +-> ) : typars -> typ -> typ
val mk_tuple_ty : typ list -> typ
val mk_iterated_fun_ty : typ list -> typ -> typ
val type_of_lambda_arg : range -> Val list -> typ
val mk_multi_lambda_ty : range -> Val list -> typ -> typ
val mk_lambda_ty : typars -> typ list -> typ -> typ

//-------------------------------------------------------------------------
// Module publication, used while compiling fslib.
//------------------------------------------------------------------------- 

val ensure_fslib_has_submodul_at : ccu -> ident list -> Tast.CompilationPath -> XmlDoc -> unit 

//-------------------------------------------------------------------------
// Miscellaneous accessors on terms
//------------------------------------------------------------------------- 

val strip_expr : expr -> expr

val discrim_of_case : DecisionTreeCase -> DecisionTreeDiscriminator
val dest_of_case : DecisionTreeCase -> DecisionTree

val var_of_bind : Binding -> Val
val vars_of_Bindings : Bindings -> FlatVals 
val vars_of_binds : Binding list -> Val list
val rhs_of_bind : Binding -> expr

//-------------------------------------------------------------------------
// Build decision trees imperatively
//------------------------------------------------------------------------- 

type MatchBuilder =
    new : SequencePointInfoForBinding * range -> MatchBuilder
    member AddTarget : DecisionTreeTarget -> int
    member AddResultTarget : expr * SequencePointInfoForTarget -> DecisionTree
    member CloseTargets : unit -> DecisionTreeTarget list
    member Close : DecisionTree * range * typ -> expr

//-------------------------------------------------------------------------
// Make some special decision graphs
//------------------------------------------------------------------------- 

val mk_bool_switch : range -> expr -> DecisionTree -> DecisionTree -> DecisionTree
val mk_cond : SequencePointInfoForBinding -> SequencePointInfoForTarget -> range -> typ -> expr -> expr -> expr -> expr
val mk_nonnull_cond : TcGlobals -> range -> typ -> expr -> expr -> expr -> expr

//-------------------------------------------------------------------------
// Make normalized references
//------------------------------------------------------------------------- 

val mk_local_vref   : Val -> ValRef
val mk_local_modref : ModuleOrNamespace -> ModuleOrNamespaceRef
val mk_local_tcref  : Tycon -> TyconRef
val mk_local_ecref  : Tycon -> TyconRef

val mk_nonlocal_ccu_top_tcref  : ccu (* viewed *) -> Tycon -> TyconRef

val mk_vref_in_modref   : ModuleOrNamespaceRef -> Val -> ValRef
val MakeNestedTcref  : ModuleOrNamespaceRef -> Tycon -> TyconRef
val mk_rfref_in_tcref   : ModuleOrNamespaceRef -> Tycon -> ident -> RecdFieldRef

//-------------------------------------------------------------------------
// Generate new locals
//------------------------------------------------------------------------- 

val expr_for_vref : range -> ValRef -> expr

(* NOTE: try to use expr_for_vref or the expression returned from mk_local instead of this. *)
val expr_for_val : range -> Val -> expr
val gen_mk_local : range -> string -> typ -> ValMutability -> bool -> Val * expr
val mk_local : range -> string -> typ -> Val * expr
val mk_compgen_local : range -> string -> typ -> Val * expr
val mk_mut_compgen_local : range -> string -> typ -> Val * expr
val mk_compgen_local_and_invisible_bind : TcGlobals -> string -> range -> expr -> Val * expr * Binding 

//-------------------------------------------------------------------------
// Make lambdas
//------------------------------------------------------------------------- 

val mk_multi_lambda : range -> Val list -> expr * typ -> expr
val mk_basev_multi_lambda : range -> Val option -> Val list -> expr * typ -> expr
val mk_lambda : range -> Val -> expr * typ -> expr
val mk_tlambda : range -> typars -> expr * typ -> expr
val mk_obj_expr : typ * Val option * expr * ObjExprMethod list * (typ * ObjExprMethod list) list * Range.range -> expr
val mk_tchoose : range -> typars -> expr -> expr
val mk_lambdas : range -> typars -> Val list -> expr * typ -> expr
val mk_multi_lambdas_core : range -> Val list list -> expr * typ -> expr * typ
val mk_multi_lambdas : range -> typars -> Val list list -> expr * typ -> expr
val mk_basev_multi_lambdas : range -> typars -> Val option -> Val list list -> expr * typ -> expr

val mk_while      : TcGlobals -> SequencePointInfoForWhileLoop * expr * expr * range                          -> expr
val mk_for        : TcGlobals -> SequencePointInfoForForLoop * Val * expr * ForLoopStyle * expr * expr * range -> expr
val mk_try_catch  : TcGlobals -> expr * Val * expr * Val * expr * range * typ * SequencePointInfoForTry * SequencePointInfoForWith -> expr
val mk_try_finally: TcGlobals -> expr * expr * range * typ * SequencePointInfoForTry * SequencePointInfoForFinally -> expr

//-------------------------------------------------------------------------
// Make let/letrec
//------------------------------------------------------------------------- 
 

// Generate a user-level let-bindings
val mk_bind : SequencePointInfoForBinding -> Val -> expr -> Binding
val mk_let_bind : range -> Binding -> expr -> expr
val mk_lets_bind : range -> Binding list -> expr -> expr
val mk_lets_from_Bindings : range -> Bindings -> expr -> expr
val mk_let : SequencePointInfoForBinding -> range -> Val -> expr -> expr -> expr
val mk_multi_lambda_bind : Val -> SequencePointInfoForBinding -> range -> typars -> Val list list -> expr * typ -> Binding

// Compiler generated bindings may involve a user variable.
// Compiler generated bindings may give rise to a sequence point if they are part of
// an SPAlways expression. Compiler generated bindings can arise from for example, inlining.
val mk_compgen_bind : Val -> expr -> Binding
val mk_compgen_binds : Val list -> expr list -> Bindings
val mk_compgen_let : range -> Val -> expr -> expr -> expr

// Invisible bindings are never given a sequence point and should never have side effects
val mk_invisible_let : range -> Val -> expr -> expr -> expr
val mk_invisible_bind : Val -> expr -> Binding
val mk_invisible_binds : Val list -> expr list -> Binding list
val mk_invisible_FlatBindings : FlatVals -> FlatExprs -> Bindings

val mk_letrec_binds : range -> Bindings -> expr -> expr
val mk_letrec_binds_typed : range -> Bindings -> expr * 'a -> expr * 'a

 
//-------------------------------------------------------------------------
// Type schemes
//------------------------------------------------------------------------- 
 
type TypeScheme   = 
    TypeScheme of 
        typars (* the truly generalized type parameters *) 
      * typars (* free choice type parameters from a recursive block where this value only generalizes a subsest of the overall set of type parameters generalized *)
      * typ    (* the 'tau' type forming the body of the generalized type *)

val mk_poly_bind_rhs : range -> TypeScheme -> expr -> expr
val is_being_generalized : Typar -> TypeScheme -> bool

//-------------------------------------------------------------------------
// Make lazy and/or
//------------------------------------------------------------------------- 

val mk_lazy_and  : TcGlobals -> range -> expr -> expr -> expr
val mk_lazy_or   : TcGlobals -> range -> expr -> expr -> expr
val mk_byref_typ : TcGlobals -> typ -> typ
val mk_nativeint_typ : TcGlobals -> typ
val mk_unit_typ : TcGlobals -> typ

//-------------------------------------------------------------------------
// Make construction operations
//------------------------------------------------------------------------- 

val mk_ucase : UnionCaseRef * tinst * expr list * range -> expr
val mk_exnconstr : TyconRef * expr list * range -> expr
val mk_asm : ILInstr list * tinst * expr list * typ list * range -> expr
val mk_coerce : expr * typ * range * typ -> expr
val mk_rethrow : range -> typ -> expr
val mk_rethrow_library_call : TcGlobals -> typ -> range -> expr


//-------------------------------------------------------------------------
// Make projection operations
//------------------------------------------------------------------------- 
 
val mk_tuple_field_get                : expr                  * tinst * int         * range -> expr
val mk_recd_field_get_via_expra       : expr * RecdFieldRef   * tinst               * range -> expr
val mk_recd_field_get_addr_via_expra  : expr * RecdFieldRef   * tinst               * range -> expr
val mk_static_rfield_get              :        RecdFieldRef   * tinst               * range -> expr
val mk_static_rfield_set              :        RecdFieldRef   * tinst * expr        * range -> expr
val mk_static_rfield_get_addr         :        RecdFieldRef   * tinst               * range -> expr
val mk_recd_field_set_via_expra       : expr * RecdFieldRef   * tinst * expr        * range -> expr
val mk_ucase_tag_get                  : expr * TyconRef       * tinst               * range -> expr
val mk_ucase_proof                    : expr * UnionCaseRef   * tinst               * range -> expr
val mk_ucase_field_get_proven         : expr * UnionCaseRef   * tinst * int         * range -> expr
val mk_ucase_field_get_unproven       : expr * UnionCaseRef   * tinst * int         * range -> expr
val mk_exnconstr_field_get            : expr * TyconRef               * int         * range -> expr
val mk_ucase_field_set                : expr * UnionCaseRef   * tinst * int  * expr * range -> expr
val mk_exnconstr_field_set            : expr * TyconRef               * int  * expr * range -> expr

//-------------------------------------------------------------------------
// Compiled view of tuples
//------------------------------------------------------------------------- 
 
val maxTuple : int
val goodTupleFields : int
val compiled_tuple_tcref : TcGlobals -> 'a list -> TyconRef
val compiled_tuple_ty : TcGlobals -> typ list -> typ
val compiled_mk_tuple : TcGlobals -> typ list * expr list * range -> TyconRef * typ list * expr list * range

val mk_call_Tuple_ItemN : TcGlobals -> range -> int -> ILType -> expr -> typ -> expr
val split_after : int -> 'a list -> 'a list * 'a list
val get_rfref_of_tcref : TyconRef * int -> RecdFieldRef

//-------------------------------------------------------------------------
// Take the address of an expression, or force it into a mutable local. Any allocated
// mutable local may need to be kept alive over a larger expression, hence we return
// a wrapping function that wraps "let mutable loc = expr in ..." around a larger
// expression.
//------------------------------------------------------------------------- 

exception DefensiveCopyWarning of string * range 
type mutates = DefinitelyMutates | PossiblyMutates | NeverMutates
val mk_expra_of_expr : TcGlobals -> bool -> mutates -> expr -> range -> (expr -> expr) * expr

//-------------------------------------------------------------------------
// Tables keyed on values and/or type parameters
//------------------------------------------------------------------------- 

[<Struct>]
type ValMap<'a> = 
    struct
        val imap: I64map.t<'a>
        new : I64map.t<'a> -> ValMap<'a>
    end

val vspec_map_find    : Val -> ValMap<'a> -> 'a
val vspec_map_tryfind : Val -> ValMap<'a> -> 'a option
val vspec_map_mem     : Val -> ValMap<'a> -> bool
val vspec_map_add     : Val -> 'a -> ValMap<'a> -> ValMap<'a>
val vspec_map_remove  : Val -> ValMap<'a> -> ValMap<'a>
val vspec_map_empty   : unit -> ValMap<'a>
val vspec_map_is_empty   : ValMap<'a> -> bool
val vspec_map_of_list  : (Val * 'a) list -> ValMap<'a>

type ValHash<'a> 
val vspec_hash_find    : ValHash<'a> -> Val -> 'a
val vspec_hash_tryfind : ValHash<'a> -> Val -> 'a option
val vspec_hash_mem     : ValHash<'a> -> Val -> bool
val vspec_hash_add     : ValHash<'a> -> Val -> 'a -> unit
val vspec_hash_remove  : ValHash<'a> -> Val -> unit
val vspec_hash_create  : unit -> ValHash<'a>


type ValMultiMap<'a> = ValMap<'a list>
val vspec_mmap_find : Val -> ValMultiMap<'a> -> 'a list
val vspec_mmap_add  : Val -> 'a -> ValMultiMap<'a> -> ValMultiMap<'a>
val vspec_mmap_empty : unit -> ValMultiMap<'a>

type TyparMap<'a> 
val tpmap_find   : Typar -> TyparMap<'a> -> 'a
val tpmap_mem   : Typar -> TyparMap<'a> -> bool
val tpmap_add   : Typar -> 'a -> TyparMap<'a> -> TyparMap<'a> 
val tpmap_empty : unit -> TyparMap<'a> 

type TcrefMap<'a> = TCRefMap of 'a Lib.I64map.t
val tcref_map_tryfind : TyconRef -> TcrefMap<'a> -> 'a option
val tcref_map_find    : TyconRef -> TcrefMap<'a> -> 'a
val tcref_map_mem     : TyconRef -> TcrefMap<'a> -> bool
val tcref_map_add     : TyconRef -> 'a -> TcrefMap<'a> -> TcrefMap<'a>
val tcref_map_empty   : unit -> TcrefMap<'a>
val tcref_map_of_list : (TyconRef * 'a) list -> TcrefMap<'a>

type TcrefMultiMap<'a> = 'a list TcrefMap
val tcref_mmap_find : TyconRef -> TcrefMultiMap<'a> -> 'a list
val tcref_mmap_add  : TyconRef -> 'a -> TcrefMultiMap<'a> -> TcrefMultiMap<'a>
val tcref_mmap_empty : unit -> TcrefMultiMap<'a>

val val_spec_order : Val -> Val -> int
val tycon_spec_order : Tycon -> Tycon -> int
val rfref_order : RecdFieldRef -> RecdFieldRef -> int
val typar_spec_order : Typar -> Typar -> int
val bind_order : Binding -> Binding -> int

val tcref_eq : TcGlobals -> TyconRef -> TyconRef -> bool
val tycon_eq : Tycon -> Tycon -> bool

//-------------------------------------------------------------------------
// Operations on types: substitution
//------------------------------------------------------------------------- 

type TyparInst = (Typar * typ) list

type TyconRefRemap = TyconRef TcrefMap
type ValRemap = ValMap<ValRef>
type Remap =
    { tpinst : TyparInst;
      vspec_remap: ValRemap;
      tcref_remap : TyconRefRemap }
type tyenv = Remap      
val empty_tcref_remap : TyconRefRemap
val empty_tyenv : tyenv

val mk_typar_inst : typars -> typ list -> TyparInst
val mk_tcref_inst : TyconRef -> tinst -> TyparInst
val empty_tpinst : TyparInst

val InstType               : TyparInst -> typ -> typ
val inst_types              : TyparInst -> tinst -> tinst
val inst_typar_constraints  : TyparInst -> TyparConstraint list -> TyparConstraint list 
val inst_trait              : TyparInst -> TraitConstraintInfo -> TraitConstraintInfo 

/// These are just substitutions (REVIEW: remove)
type tpenv = (Typar * typ) list
val empty_tpenv : TyparInst

//-------------------------------------------------------------------------
// From typars to types 
//------------------------------------------------------------------------- 

val generalize_typars : typars -> tinst
val generalize_tcref : TyconRef -> typ list * typ
val mk_typar_to_typar_renaming : typars -> typars -> TyparInst * typ list

//-------------------------------------------------------------------------
// See through typar equations from inference and/or type abbreviation equations.
//------------------------------------------------------------------------- 

val reduce_tcref_abbrev : TyconRef -> typ list -> typ
val reduce_tcref_measureable : TyconRef -> typ list -> typ
val reduce_tcref_abbrev_measure : TyconRef -> measure

/// set bool to 'true' to allow shortcutting of type parameter equation chains during stripping 
val strip_tpeqns_and_tcabbrevsA : TcGlobals -> bool -> typ -> typ 
val strip_tpeqns_and_tcabbrevs : TcGlobals -> typ -> typ
val strip_tpeqns_and_tcabbrevs_and_measureable : TcGlobals -> typ -> typ

//-------------------------------------------------------------------------
// See through exception abbreviations
//------------------------------------------------------------------------- 

val strip_eqns_from_ecref : TyconRef -> Tycon

//-------------------------------------------------------------------------
// Analyze types.  These all look through type abbreviations and 
// inference equations, i.e. are "stripped"
//------------------------------------------------------------------------- 

val dest_forall_typ     : TcGlobals -> typ -> typars * typ
val dest_fun_typ        : TcGlobals -> typ -> typ * typ
val dest_tuple_typ      : TcGlobals -> typ -> typ list
val dest_typar_typ      : TcGlobals -> typ -> Typar
val dest_anypar_typ     : TcGlobals -> typ -> Typar
val dest_measure_typ    : TcGlobals -> typ -> measure
val try_dest_forall_typ : TcGlobals -> typ -> typars * typ

val is_fun_typ          : TcGlobals -> typ -> bool
val is_forall_typ       : TcGlobals -> typ -> bool
val is_tuple_typ        : TcGlobals -> typ -> bool
val is_tuple_struct_typ : TcGlobals -> typ -> bool
val is_union_typ        : TcGlobals -> typ -> bool
val is_repr_hidden_typ  : TcGlobals -> typ -> bool
val is_fsobjmodel_typ   : TcGlobals -> typ -> bool
val is_recd_typ         : TcGlobals -> typ -> bool
val is_typar_typ        : TcGlobals -> typ -> bool
val is_anypar_typ       : TcGlobals -> typ -> bool
val is_measure_typ      : TcGlobals -> typ -> bool

val contains_measures_typ : TcGlobals -> typ -> bool

val mk_tyapp_ty : TyconRef -> tinst -> typ

val mk_proven_ucase_typ : UnionCaseRef -> tinst -> typ
val is_proven_ucase_typ : typ -> bool

val is_stripped_tyapp_typ     : TcGlobals -> typ -> bool
val dest_stripped_tyapp_typ   : TcGlobals -> typ -> TyconRef * tinst
val tcref_of_stripped_typ     : TcGlobals -> typ -> TyconRef
val try_tcref_of_stripped_typ : TcGlobals -> typ -> TyconRef option
val tinst_of_stripped_typ     : TcGlobals -> typ -> tinst
val mk_inst_for_stripped_typ  : TcGlobals -> typ -> TyparInst

val domain_of_fun_typ  : TcGlobals -> typ -> typ
val range_of_fun_typ   : TcGlobals -> typ -> typ
val strip_fun_typ      : TcGlobals -> typ -> typ list * typ
val strip_fun_typ_upto : TcGlobals -> int -> typ -> typ list * typ

val reduce_forall_typ : TcGlobals -> typ -> tinst -> typ

val try_dest_tuple_typ : TcGlobals -> typ -> typ list

(* union data constructors *)
val rty_of_ucref               : UnionCaseRef -> typ
val rfields_of_ucref           : UnionCaseRef -> RecdField list
val rty_of_uctyp : UnionCaseRef -> tinst -> typ
val ucref_index: UnionCaseRef -> int
val ucrefs_of_tcref: TyconRef -> UnionCaseRef list

(* fields of union data constructors *)
val typ_of_ucref_rfield_by_idx : UnionCaseRef -> tinst -> int -> typ
val typs_of_ucref_rfields : TyparInst -> UnionCaseRef -> typ list

(* fields of records *)
val typ_of_rfield           : TyparInst -> RecdField -> typ

val actual_rtyp_of_rfref   : RecdFieldRef -> tinst -> typ
val rfref_index            : RecdFieldRef -> int

val typs_of_instance_rfields_of_tcref    : TyparInst -> TyconRef -> typ list
val actual_typ_of_rfield     : Tycon -> tinst -> RecdField -> typ
val instance_rfrefs_of_tcref : TyconRef -> RecdFieldRef list
val all_rfrefs_of_tcref      : TyconRef -> RecdFieldRef list

(* fields of exception constructors *)
val rfields_of_ecref : TyconRef -> RecdField list
val typs_of_ecref_rfields : TyconRef -> typ list

val IterType : (typ -> unit) * (Typar -> unit) * (TraitConstraintSln -> unit) -> typ -> unit

val is_typeof_vref : TcGlobals -> ValRef -> bool
val is_typedefof_vref : TcGlobals -> ValRef -> bool

//-------------------------------------------------------------------------
// Top types: guaranteed to be compiled to .NET methods, and must be able to 
// have user-specified argument names (for stability w.r.t. reflection)
// and user-specified argument and return attributes.
//------------------------------------------------------------------------- 

type UncurriedArgInfos = (typ * TopArgInfo) list 
type CurriedArgInfos = UncurriedArgInfos list

val GetTopTauTypeInFSharpForm : TcGlobals -> TopArgInfo list list -> typ -> range -> CurriedArgInfos * typ
val GetTopValTypeInFSharpForm     : TcGlobals -> ValTopReprInfo -> typ -> range -> typars * CurriedArgInfos * typ * TopArgInfo
val IsCompiledAsStaticValue       : TcGlobals -> Val -> bool
val GetTopValTypeInCompiledForm   : TcGlobals -> ValTopReprInfo -> typ -> range -> typars * CurriedArgInfos * typ option * TopArgInfo
val GetFSharpViewOfReturnType : TcGlobals -> typ option -> typ

val NormalizeDeclaredTyparsForEquiRecursiveInference : TcGlobals -> typars -> typars

//-------------------------------------------------------------------------
// Compute the return type after an application
//------------------------------------------------------------------------- 
 
val apply_types : TcGlobals -> typ -> typ list * 'a list -> typ

//-------------------------------------------------------------------------
// Compute free variables in types
//------------------------------------------------------------------------- 
 
val empty_free_loctypars : FreeTypars
val union_free_loctypars : FreeTypars -> FreeTypars -> FreeTypars

val empty_free_loctycons : FreeTycons
val union_free_loctycons : FreeTycons -> FreeTycons -> FreeTycons

val empty_free_tyvars : FreeTyvars
val union_free_tyvars : FreeTyvars -> FreeTyvars -> FreeTyvars

val empty_free_locvals : FreeLocals
val union_free_locvals : FreeLocals -> FreeLocals -> FreeLocals

type FreeVarOptions 

val CollectLocalsNoCaching : FreeVarOptions
val CollectTyparsNoCaching : FreeVarOptions
val CollectTyparsAndLocalsNoCaching : FreeVarOptions
val CollectTyparsAndLocals : FreeVarOptions
val CollectLocals : FreeVarOptions
val CollectTypars : FreeVarOptions
val CollectAllNoCaching : FreeVarOptions
val CollectAll : FreeVarOptions

val acc_free_in_types : FreeVarOptions -> typ list -> FreeTyvars -> FreeTyvars
val acc_free_in_type : FreeVarOptions -> typ -> FreeTyvars -> FreeTyvars
val acc_free_tprefs : FreeVarOptions -> typars -> FreeTyvars -> FreeTyvars

val free_in_type  : FreeVarOptions -> typ      -> FreeTyvars
val free_in_types : FreeVarOptions -> typ list -> FreeTyvars
val free_in_val   : FreeVarOptions -> Val -> FreeTyvars

(* This one puts free variables in canonical left-to-right order. *)
val free_in_type_lr : TcGlobals -> bool -> typ -> typars
val free_in_types_lr : TcGlobals -> bool -> typ list -> typars
val free_in_types_lr_no_cxs : TcGlobals -> typ list -> typars


//-------------------------------------------------------------------------
// Equivalence of types (up to substitution of type variables in the left-hand type)
//------------------------------------------------------------------------- 

type TypeEquivEnv = 
    { ae_typars: TyparMap<typ>;
      ae_tcrefs: TyconRefRemap }

val tyeq_env_empty : TypeEquivEnv

val bind_tyeq_env_typars : typars -> typars   -> TypeEquivEnv -> TypeEquivEnv
val bind_tyeq_env_types  : typars -> typ list -> TypeEquivEnv -> TypeEquivEnv
val bind_tyeq_env_tpinst : TyparInst         -> TypeEquivEnv -> TypeEquivEnv
val mk_tyeq_env          : typars -> typars                     -> TypeEquivEnv

type Erasure = EraseAll | EraseMeasures | EraseNone
val traits_aequiv_aux           : Erasure -> TcGlobals -> TypeEquivEnv -> TraitConstraintInfo  -> TraitConstraintInfo  -> bool
val traits_aequiv               :            TcGlobals -> TypeEquivEnv -> TraitConstraintInfo  -> TraitConstraintInfo  -> bool
val typarConstraints_aequiv_aux : Erasure -> TcGlobals -> TypeEquivEnv -> TyparConstraint      -> TyparConstraint      -> bool
val typarConstraints_aequiv     :            TcGlobals -> TypeEquivEnv -> TyparConstraint      -> TyparConstraint      -> bool
val typar_decls_aequiv          :            TcGlobals -> TypeEquivEnv -> typars               -> typars               -> bool
val type_aequiv_aux             : Erasure -> TcGlobals -> TypeEquivEnv -> typ                  -> typ                  -> bool
val type_aequiv                 :            TcGlobals -> TypeEquivEnv -> typ                  -> typ                  -> bool
val return_types_aequiv_aux     : Erasure -> TcGlobals -> TypeEquivEnv -> typ option           -> typ option           -> bool
val return_types_aequiv         :            TcGlobals -> TypeEquivEnv -> typ option           -> typ option           -> bool
val tcref_aequiv                :            TcGlobals -> TypeEquivEnv -> TyconRef             -> TyconRef             -> bool
val type_equiv_aux              : Erasure -> TcGlobals                   -> typ                  -> typ                  -> bool
val type_equiv                  :            TcGlobals                   -> typ                  -> typ                  -> bool
val measure_equiv               :            TcGlobals                   -> measure              -> measure              -> bool

//-------------------------------------------------------------------------
// Unit operations
//------------------------------------------------------------------------- 
val MeasurePower : measure -> int -> measure
val ListMeasureVarOccsWithNonZeroExponents : measure -> (Typar * int) list
val ListMeasureConOccsWithNonZeroExponents : TcGlobals -> bool -> measure -> (TyconRef * int) list
val ProdMeasures : measure list -> measure
val MeasureVarExponent : Typar -> measure -> int
val MeasureConExponent : TcGlobals -> bool -> TyconRef -> measure -> int

//-------------------------------------------------------------------------
// VSPR values (i.e. values that are really members in the object model)
//------------------------------------------------------------------------- 

val GetNumObjArgsOfVal: Val -> int
val GetNumObjArgsOfValRef: ValRef -> int

val GetTypeOfMemberInMemberForm : TcGlobals -> ValRef -> typars * CurriedArgInfos * typ option * TopArgInfo
val GetTypeOfIntrinsicMemberInCompiledForm : TcGlobals -> ValRef -> typars * CurriedArgInfos * typ option * TopArgInfo
val GetMemberTypeInMemberForm : TcGlobals -> MemberFlags -> ValTopReprInfo -> typ -> range -> typars * CurriedArgInfos * typ option * TopArgInfo

val PartitionValTypars : TcGlobals -> Val -> (typars * typars * typars * TyparInst * typ list) option
val PartitionValRefTypars : TcGlobals -> ValRef -> (typars * typars * typars * TyparInst * typ list) option

val ReturnTypeOfPropertyVal : TcGlobals -> Val -> typ
val ArgInfosOfPropertyVal : TcGlobals -> Val -> UncurriedArgInfos 
val ArgInfosOfMember: TcGlobals -> ValRef -> CurriedArgInfos 

val GetMemberCallInfo : TcGlobals -> ValRef * ValUseFlag -> int * bool * bool * bool * bool * bool * bool * bool

//-------------------------------------------------------------------------
// Printing
//------------------------------------------------------------------------- 
 
module PrettyTypes =
    val NeedsPrettyTyparName : Typar -> bool
    val PrettyTyparNames : (Typar -> bool) -> string list -> typars -> string list
    val PrettifyTypes1 : TcGlobals -> typ -> TyparInst * typ * (Typar * TyparConstraint) list
    val PrettifyTypes2 : TcGlobals -> typ * typ -> TyparInst * (typ * typ) * (Typar * TyparConstraint) list
    val PrettifyTypesN : TcGlobals -> typ list -> TyparInst * typ list * (Typar * TyparConstraint) list

type DisplayEnv = {html: bool;
                   htmlHideRedundantKeywords: bool;
                   htmlAssemMap: string Lib.NameMap;
                   openTopPaths: (string list) list;
                   showObsoleteMembers: bool; 
                   showTyparBinding: bool;
                   showImperativeTyparAnnotations: bool;
                   suppressInlineKeyword:bool;
                   showMemberContainers: bool;
                   shortConstraints:bool;
                   showAttributes: bool;
                   showOverrides:bool;
                   showConstraintTyparAnnotations:bool;
                   abbreviateAdditionalConstraints: bool;
                   showTyparDefaultConstraints: bool;
                   g: TcGlobals ;
                   contextAccessibility: Accessibility;
                   generatedValueLayout:(Val -> layout option);                     
                   }
      with 
          member Normalize: unit -> DisplayEnv
      end

val empty_denv : TcGlobals -> DisplayEnv
val denv_add_open_path   : path   -> DisplayEnv -> DisplayEnv
val denv_add_open_modref : ModuleOrNamespaceRef   -> DisplayEnv -> DisplayEnv

val full_name_of_nlpath : NonLocalPath -> string

/// Return the full text for an item as we want it displayed to the user as a fully qualified entity
val full_display_text_of_modref : ModuleOrNamespaceRef -> string
val full_display_text_of_parent_of_modref : ModuleOrNamespaceRef -> string option
val full_display_text_of_vref   : ValRef -> string
val full_display_text_of_tcref  : TyconRef -> string
val full_display_text_of_ecref  : TyconRef -> string
val full_display_text_of_ucref  : UnionCaseRef -> string
val full_display_text_of_rfref  : RecdFieldRef -> string

val ticks_and_argcount_text_of_tcref : TyconRef -> string

/// Return the full path to the item using mangled names (well, sort of), to act as a unique key into the FSI generation lookaside table.
/// The mangled names have to precisely match the path names implicitly embedded as attributes into layout objects by the NicePrint code below.
/// This is a very fragile technique and the mangled names are not really guaranteed to be unique (e.g. the mangled names
/// don't cope with overloading of types by generic arity), and this whole business of using mangled paths is not a 
/// good technique in general. Hence these functions should not be used outside the FSI generation code.
val approx_full_mangled_name_of_modref : ModuleOrNamespaceRef -> string
val approx_full_mangled_name_of_vref : ValRef -> string
val approx_full_mangled_name_of_tcref  : TyconRef -> string
val approx_full_mangled_name_of_ecref  : TyconRef -> string
val approx_full_mangled_name_of_ucref  : UnionCaseRef -> string
val approx_full_mangled_name_of_rfref  : RecdFieldRef -> string

/// A unique qualified name for each type definition, used to qualify the names of interface implementation methods
val qualified_mangled_name_of_tcref : TyconRef -> string -> string

module NicePrint =
    val constL : Constant -> layout
    val typeL                       : DisplayEnv -> typ -> layout
    val constraintL                 : DisplayEnv -> (Typar * TyparConstraint) -> layout 
    val topTypAndConstraintsL       : DisplayEnv -> (typ * TopArgInfo) list -> typ -> layout
    val typesAndConstraintsL        : DisplayEnv -> typ list -> layout list * layout
    val tyconL                      : DisplayEnv -> layout -> Tycon -> layout
    val memberSigL                  : DisplayEnv -> TyparInst * string * typars * CurriedArgInfos * typ -> layout
    val measureL                    : DisplayEnv -> measure -> layout
    val valL                        : DisplayEnv -> Val -> layout
    val exnDefnL                    : DisplayEnv -> Tycon -> layout
    val exnDefnReprL                : DisplayEnv -> ExceptionInfo -> layout
    val dataExprL                   : DisplayEnv -> expr -> layout

    val InferredSigOfModuleExprL    : bool -> DisplayEnv -> ModuleOrNamespaceExprWithSig -> layout
    val ModuleOrNamespaceTypeL      : DisplayEnv -> ModuleOrNamespaceType -> layout
    val ModuleOrNamespaceL          : DisplayEnv -> ModuleOrNamespace -> layout
    val AssemblyL                   : DisplayEnv -> ModuleOrNamespace -> layout
    
    val string_of_typ               : DisplayEnv -> typ -> string
    val pretty_string_of_typ        : DisplayEnv -> typ -> string
    val pretty_string_of_unit       : DisplayEnv -> measure -> string
    val string_of_typar_constraints : DisplayEnv -> (Typar * TyparConstraint) list  -> string
    val string_of_typar_constraint  : DisplayEnv -> Typar * TyparConstraint -> string

    val output_tref                 : DisplayEnv -> StringBuilder -> ILTypeRef -> unit
    val output_tcref                : DisplayEnv -> StringBuilder -> TyconRef -> unit
    val output_typ                  : DisplayEnv -> StringBuilder -> typ -> unit
    val output_typars               : DisplayEnv -> string -> StringBuilder -> typars -> unit
    val output_typar_constraint     : DisplayEnv -> StringBuilder -> Typar * TyparConstraint -> unit
    val output_typar_constraints    : DisplayEnv -> StringBuilder -> (Typar * TyparConstraint) list -> unit
    val output_qualified_val_spec   : DisplayEnv -> StringBuilder -> Val -> unit
    
    val string_of_qualified_val_spec : DisplayEnv -> Val -> string

    val output_tycon                : DisplayEnv -> StringBuilder -> Tycon -> unit
    val output_ucase                : DisplayEnv -> StringBuilder -> UnionCase -> unit
    val output_rfield               : DisplayEnv -> StringBuilder -> RecdField -> unit
    val output_exnc                 : DisplayEnv -> StringBuilder -> Tycon -> unit
    
    val string_of_tycon             : DisplayEnv -> Tycon -> string
    val string_of_ucase             : DisplayEnv -> UnionCase -> string
    val string_of_rfield            : DisplayEnv -> RecdField -> string
    val string_of_exnc              : DisplayEnv -> Tycon -> string

val adhoc_of_tycon : Tycon -> ValRef list
val super_of_tycon : TcGlobals -> Tycon -> Tast.typ
val implements_of_tycon : TcGlobals -> Tycon -> Tast.typ list
val vslot_vals_of_tycons : Tycon list -> Val list

//-------------------------------------------------------------------------
// Free variables in expressions etc.
//------------------------------------------------------------------------- 

val empty_freevars : FreeVars
val union_freevars : FreeVars -> FreeVars -> FreeVars

val acc_free_in_targets      : FreeVarOptions -> DecisionTreeTarget array -> FreeVars -> FreeVars
val acc_free_in_exprs        : FreeVarOptions -> expr list -> FreeVars -> FreeVars
val acc_free_in_switch_cases : FreeVarOptions -> DecisionTreeCase list -> DecisionTree option -> FreeVars -> FreeVars
val acc_free_in_dtree        : FreeVarOptions -> DecisionTree -> FreeVars -> FreeVars

/// Get the free variables in a module definition.
val free_in_mdef : FreeVarOptions -> ModuleOrNamespaceExpr -> FreeVars

/// Get the free variables in an expression.
val free_in_expr  : FreeVarOptions -> expr  -> FreeVars

/// Get the free variables in the right hand side of a binding.
val free_in_rhs   : FreeVarOptions -> Binding  -> FreeVars

val free_tyvars_all_public  : FreeTyvars -> bool
val freevars_all_public     : FreeVars -> bool

//-------------------------------------------------------------------------
// Mark/range/position information from expressions
//------------------------------------------------------------------------- 

val range_of_expr : expr -> range

//-------------------------------------------------------------------------
// Top expressions to implement top types
//------------------------------------------------------------------------- 

val dest_top_lambda : expr * typ -> typars * Val list list * expr * typ
val InferArityOfExpr : TcGlobals -> typ -> Tast.Attribs list list -> Tast.Attribs -> expr -> ValTopReprInfo
val InferArityOfExprBinding : TcGlobals -> Val -> expr -> ValTopReprInfo
val chosen_arity_of_bind : Binding -> ValTopReprInfo option

//-------------------------------------------------------------------------
//  Copy expressions and types
//------------------------------------------------------------------------- 
                   
val empty_expr_remap : Remap

(* REVIEW: this mutation should not be needed *)
val set_val_has_no_arity : Val -> Val

type ValCopyFlag = 
    | CloneAll
    | CloneAllAndMarkExprValsAsCompilerGenerated
    // OnlyCloneExprVals is a nasty setting to reuse the cloning logic in a mode where all 
    // Tycon and "module/member" Val objects keep their identity, but the Val objects for all expr bindings
    // are cloned. This is used to 'fixup' the TAST created by tlr.ml. 
    //
    // This is a fragile mode of use. It's not really clear why TLR needs to create a "bad" expression tree that
    // reuses Val objects as multiple value bindings, and its been the cause of several subtle bugs.
    | OnlyCloneExprVals

val remap_tcref : TyconRefRemap -> TyconRef -> TyconRef
val remap_ucref : TyconRefRemap -> UnionCaseRef -> UnionCaseRef
val remap_rfref : TyconRefRemap -> RecdFieldRef -> RecdFieldRef
val remap_vref : Remap -> ValRef -> ValRef
val remap_expr : TcGlobals -> ValCopyFlag -> Remap -> expr -> expr
val remap_possible_forall_typ : TcGlobals -> Remap -> typ -> typ
val copy_mtyp : TcGlobals -> ValCopyFlag -> ModuleOrNamespaceType -> ModuleOrNamespaceType
val copy_expr : TcGlobals -> ValCopyFlag -> expr -> expr
val copy_ImplFile : TcGlobals -> ValCopyFlag -> TypedImplFile -> TypedImplFile
val copy_assembly : TcGlobals -> ValCopyFlag -> TypedAssembly -> TypedAssembly
val copy_slotsig : SlotSig -> SlotSig
val inst_slotsig : TyparInst -> SlotSig -> SlotSig
val inst_expr : TcGlobals -> TyparInst -> expr -> expr

//-------------------------------------------------------------------------
// Build the remapping that corresponds to a module meeting its signature
// and also report the set of tycons, tycon representations and values hidden in the process.
//------------------------------------------------------------------------- 

type SignatureRepackageInfo = 
    { mrpiVals: (ValRef * ValRef) list;
      mrpiTycons: (TyconRef * TyconRef) list  }
      
type SignatureHidingInfo = 
    { mhiTycons  : Tycon Zset.t; 
      mhiTyconReprs : Tycon Zset.t;  
      mhiVals       : Val Zset.t; 
      mhiRecdFields : RecdFieldRef Zset.t;
      mhiUnionCases : UnionCaseRef Zset.t }

val empty_mrpi : SignatureRepackageInfo
val empty_mhi : SignatureHidingInfo
val union_mhi : SignatureHidingInfo -> SignatureHidingInfo -> SignatureHidingInfo

val mk_mtyp_to_mtyp_remapping : ModuleOrNamespaceType -> ModuleOrNamespaceType -> SignatureRepackageInfo * SignatureHidingInfo
val mk_mdef_to_mtyp_remapping : ModuleOrNamespaceExpr -> ModuleOrNamespaceType -> SignatureRepackageInfo * SignatureHidingInfo
val mk_assembly_boundary_mhi : ModuleOrNamespaceType -> SignatureHidingInfo
val mtyp_of_mexpr : ModuleOrNamespaceExprWithSig -> ModuleOrNamespaceType
val mk_repackage_remapping : SignatureRepackageInfo -> Remap 

val wrap_modul_in_namespace : ident -> ModuleOrNamespace -> ModuleOrNamespace
val wrap_mbind_in_namespace : ident -> ModuleOrNamespaceBinding -> ModuleOrNamespaceBinding
val wrap_modul_as_mtyp_in_namespace : ModuleOrNamespace -> ModuleOrNamespaceType 
val wrap_mtyp_as_mspec : ident -> CompilationPath -> ModuleOrNamespaceType -> ModuleOrNamespace

val SigTypeOfImplFile : TypedImplFile -> ModuleOrNamespaceType

//-------------------------------------------------------------------------
// Given a list of top-most signatures that together constrain the public compilation units
// of an assembly, compute a remapping that converts local references to non-local references.
// This remapping must be applied to all pickled expressions and types 
// exported from the assembly.
//------------------------------------------------------------------------- 

val MakeExportRemapping : ccu -> (ModuleOrNamespace -> Remap)
val ApplyExportRemappingToEntity :  TcGlobals -> Remap -> ModuleOrNamespace -> ModuleOrNamespace 

/// Query SignatureRepackageInfo
val IsHiddenTycon     : (Remap * SignatureHidingInfo) list -> Tycon -> bool
val IsHiddenTyconRepr : (Remap * SignatureHidingInfo) list -> Tycon -> bool
val IsHiddenVal       : (Remap * SignatureHidingInfo) list -> Val -> bool
val IsHiddenRecdField : (Remap * SignatureHidingInfo) list -> RecdFieldRef -> bool

//-------------------------------------------------------------------------
//  Adjust marks in expressions
//------------------------------------------------------------------------- 

val RemarkExpr : range -> expr -> expr

//-------------------------------------------------------------------------
// Make applications
//------------------------------------------------------------------------- 
 
val prim_mk_app : (expr * typ) -> tinst -> expr list -> range -> expr
val mk_appl : TcGlobals -> (expr * typ) * typ list list * expr list * range -> expr
val mk_tyapp : range -> expr * typ -> typ list -> expr

val mk_val_set   : range -> ValRef -> expr -> expr
val mk_lval_set  : range -> ValRef -> expr -> expr
val mk_lval_get  : range -> ValRef -> expr
val mk_val_addr  : range -> ValRef -> expr

//-------------------------------------------------------------------------
// Note these take the address of the record expression if it is a struct, and
// apply a type instantiation if it is a first-class polymorphic record field.
//------------------------------------------------------------------------- 

val mk_recd_field_get : TcGlobals -> expr * RecdFieldRef * tinst * tinst * range -> expr
val mk_recd_field_set : TcGlobals -> expr * RecdFieldRef * tinst * expr * range -> expr

//-------------------------------------------------------------------------
//  Get the targets used in a decision graph (for reporting warnings)
//------------------------------------------------------------------------- 

val acc_targets_of_dtree : DecisionTree -> int list -> int list

//-------------------------------------------------------------------------
//  Optimizations on decision graphs
//------------------------------------------------------------------------- 

val mk_and_optimize_match : SequencePointInfoForBinding  -> range -> range -> typ -> DecisionTree -> DecisionTreeTarget list -> expr

val prim_mk_match : SequencePointInfoForBinding * range * DecisionTree * DecisionTreeTarget array * range * typ -> expr

//-------------------------------------------------------------------------
//  Work out what things on the r.h.s. of a letrec need to be fixed up
//------------------------------------------------------------------------- 

val iter_letrec_fixups : 
   TcGlobals -> Val option  -> 
   (Val option -> expr -> (expr -> expr) -> expr -> unit) -> 
   expr * (expr -> expr) -> expr -> unit

//-------------------------------------------------------------------------
// From lambdas taking multiple variables to lambdas taking a single variable
// of tuple type. 
//------------------------------------------------------------------------- 

val multi_lambda_to_tupled_lambda: Val list -> expr -> Val * expr
val AdjustArityOfLambdaBody          : TcGlobals -> int -> Val list -> expr -> Val list * expr

//-------------------------------------------------------------------------
// Make applications, doing beta reduction by introducing let-bindings
//------------------------------------------------------------------------- 

val MakeApplicationAndBetaReduce : TcGlobals -> expr * typ * tinst list * expr list * range -> expr

val JoinTyparStaticReq : TyparStaticReq -> TyparStaticReq -> TyparStaticReq

//-------------------------------------------------------------------------
// More layout - this is for debugging
//------------------------------------------------------------------------- 
module DebugPrint =

    val layout_ranges : bool ref
    val showType : typ -> string
    val showExpr : expr -> string

val ValRefL : ValRef -> layout
val UnionCaseRefL : UnionCaseRef -> layout
val vspecAtBindL : Val -> layout
val intL : int -> layout
val valL : Val -> layout
val TyparDeclL : Typar -> layout
val traitL : TraitConstraintInfo -> layout
val TyparL : Typar -> layout
val TyparsL : typars -> layout
val typeL : typ -> layout
val SlotSigL : SlotSig -> layout
val EntityTypeL : ModuleOrNamespaceType -> layout
val EntityL : ModuleOrNamespace -> layout
val TypeOfvalL : Val -> layout
val MemberL : ValMemberInfo -> layout
val BindingL : Binding -> layout
val ExprL : expr -> layout
val tyconL : Tycon -> layout
val DecisionTreeL : DecisionTree -> layout
val ImplFileL : TypedImplFile -> layout
val AssemblyL : TypedAssembly -> layout
val recdFieldRefL : RecdFieldRef -> layout

//-------------------------------------------------------------------------
// Fold on expressions
//------------------------------------------------------------------------- 

type ExprFolder<'a> =
    { exprIntercept    : ('a -> expr -> 'a) -> 'a -> expr -> 'a option;
      valBindingSiteIntercept          : 'a -> bool * Val -> 'a;
      nonRecBindingsIntercept         : 'a -> Binding -> 'a;         
      recBindingsIntercept        : 'a -> Bindings -> 'a;         
      dtreeAcc           : 'a -> DecisionTree -> 'a;
      targetIntercept    : ('a -> expr -> 'a) -> 'a -> DecisionTreeTarget -> 'a option;
      tmethodIntercept   : ('a -> expr -> 'a) -> 'a -> ObjExprMethod -> 'a option;}
val ExprFolder0 : ExprFolder<'a>
val FoldImplFile: ExprFolder<'a> -> ('a -> TypedImplFile -> 'a) 
val FoldExpr : ExprFolder<'a> -> ('a -> expr -> 'a) 

val ExprStats : expr -> string

//-------------------------------------------------------------------------
// Make some common types
//------------------------------------------------------------------------- 

val mk_nativeptr_typ  : TcGlobals -> typ -> typ
val mk_array_typ      : TcGlobals -> typ -> typ
val is_option_ty     : TcGlobals -> typ -> bool
val dest_option_ty   : TcGlobals -> typ -> typ
val try_dest_option_ty : TcGlobals -> typ -> typ option

//-------------------------------------------------------------------------
// Primitives associated with compiling the IEvent idiom to .NET events
//------------------------------------------------------------------------- 

val is_fslib_IDelegateEvent_ty   : TcGlobals -> typ -> bool
val dest_fslib_IDelegateEvent_ty : TcGlobals -> typ -> typ 
val mk_fslib_IEvent2_ty   : TcGlobals -> typ -> typ -> typ

//-------------------------------------------------------------------------
// Primitives associated with printf format string parsing
//------------------------------------------------------------------------- 

val mk_lazy_ty : TcGlobals -> typ -> typ
val mk_format_ty : TcGlobals -> typ -> typ -> typ -> typ -> typ -> typ

//-------------------------------------------------------------------------
// Classify types
//------------------------------------------------------------------------- 

val is_il_named_typ      : TcGlobals -> typ -> bool
val mk_multi_dim_array_typ         : TcGlobals -> int -> typ -> typ
val is_il_arr_tcref      : TcGlobals -> TyconRef -> bool
val rank_of_il_arr_tcref : TcGlobals -> TyconRef -> int
val is_il_arr_typ        : TcGlobals -> typ -> bool
val is_il_arr1_typ       : TcGlobals -> typ -> bool
val dest_il_arr1_typ     : TcGlobals -> typ -> typ
val is_compat_array_typ  : TcGlobals -> typ -> bool
val is_unit_typ          : TcGlobals -> typ -> bool
val is_obj_typ           : TcGlobals -> typ -> bool
val is_void_typ          : TcGlobals -> typ -> bool
val is_il_ref_typ        : TcGlobals -> typ -> bool

val is_any_array_typ      : TcGlobals -> typ -> bool
val dest_any_array_typ    : TcGlobals -> typ -> typ
val rank_of_any_array_typ : TcGlobals -> typ -> int

val is_fsobjmodel_ref_typ         : TcGlobals -> typ -> bool
val is_struct_tcref               : TyconRef -> bool
val is_enum_tycon                 : Tycon -> bool
val is_enum_tcref                 : TyconRef -> bool
val is_interface_tycon                 : Tycon -> bool
val is_interface_tcref                 : TyconRef -> bool
val is_delegate_typ  : TcGlobals -> typ -> bool
val is_interface_typ : TcGlobals -> typ -> bool
val is_ref_typ    : TcGlobals -> typ -> bool
val is_sealed_typ : TcGlobals -> typ -> bool
val IsComInteropType : TcGlobals -> typ -> bool
val GetUnderlyingTypeOfEnumType : TcGlobals -> typ -> typ
val is_struct_typ : TcGlobals -> typ -> bool
val is_class_typ  : TcGlobals -> typ -> bool
val is_enum_typ   : TcGlobals -> typ -> bool
val is_flag_enum_typ   : TcGlobals -> typ -> bool

//-------------------------------------------------------------------------
// Special semantic constraints
//------------------------------------------------------------------------- 

val IsUnionTypeWithNullAsTrueValue: TcGlobals -> Tycon -> bool
val MemberIsCompiledAsInstance : TcGlobals -> TyconRef -> bool -> ValMemberInfo -> Attribs -> bool
val ValSpecIsCompiledAsInstance : TcGlobals -> Val -> bool
val ValRefIsCompiledAsInstanceMember : TcGlobals -> ValRef -> bool
val ModuleNameIsMangled : TcGlobals -> Attribs -> bool

val CompileAsEvent : TcGlobals -> Attribs -> bool

val TypeNullIsExtraValue : TcGlobals -> typ -> bool
val TypeNullIsTrueValue : TcGlobals -> typ -> bool
val TypeNullNotLiked : TcGlobals -> typ -> bool
val TypeNullNever : TcGlobals -> typ -> bool

val TypeSatisfiesNullConstraint : TcGlobals -> typ -> bool
val TypeHasDefaultValue : TcGlobals -> typ -> bool

val is_partially_implemented_tycon : Tycon -> bool

val ucref_alloc_observable : UnionCaseRef -> bool
val tycon_alloc_observable : Tycon -> bool
val tcref_alloc_observable : TyconRef -> bool
val ecref_alloc_observable : TyconRef -> bool 
val ucref_rfield_mutable : TcGlobals -> UnionCaseRef -> int -> bool
val ecref_rfield_mutable : TyconRef -> int -> bool

val use_genuine_field : Tycon -> RecdField -> bool 
val gen_field_name : Tycon -> RecdField -> string

//-------------------------------------------------------------------------
// Destruct slotsigs etc.
//------------------------------------------------------------------------- 

val slotsig_has_void_rty     : SlotSig -> bool
val actual_rty_of_slotsig    : tinst -> tinst -> SlotSig -> typ option

val rty_of_tmethod : TcGlobals -> ObjExprMethod -> typ option

//-------------------------------------------------------------------------
// Primitives associated with initialization graphs
//------------------------------------------------------------------------- 

val mk_refcell              : TcGlobals -> range -> typ -> expr -> expr
val mk_refcell_get          : TcGlobals -> range -> typ -> expr -> expr
val mk_refcell_set          : TcGlobals -> range -> typ -> expr -> expr -> expr
val mk_lazy_delayed         : TcGlobals -> range -> typ -> expr -> expr
val mk_lazy_force           : TcGlobals -> range -> typ -> expr -> expr

val mk_refcell_contents_rfref : TcGlobals -> RecdFieldRef
val is_refcell_ty   : TcGlobals -> typ -> bool
val dest_refcell_ty : TcGlobals -> typ -> typ
val mk_refcell_ty   : TcGlobals -> typ -> typ

val mk_seq_ty          : TcGlobals -> typ -> typ
val mk_IEnumerator_ty  : TcGlobals -> typ -> typ
val mk_list_ty         : TcGlobals -> typ -> typ
val mk_option_ty       : TcGlobals -> typ -> typ
val mk_none_ucref      : TcGlobals -> UnionCaseRef
val mk_some_ucref      : TcGlobals -> UnionCaseRef

val mk_nil  : TcGlobals -> range -> typ -> expr
val mk_cons : TcGlobals -> typ -> expr -> expr -> expr

//-------------------------------------------------------------------------
// Make a few more expressions
//------------------------------------------------------------------------- 

val mk_seq  : SequencePointInfoForSeq -> range -> expr -> expr -> expr
val mk_compgen_seq  : range -> expr -> expr -> expr
val mk_seqs : SequencePointInfoForSeq -> TcGlobals -> range -> expr list -> expr   
val mk_recd : TcGlobals -> RecordConstructionInfo * TyconRef * tinst * RecdFieldRef list * expr list * range -> expr
val mk_unbox : typ -> expr -> range -> expr
val mk_isinst : typ -> expr -> range -> expr
val mk_null : range -> typ -> expr
val mk_nonnull_test : TcGlobals -> range -> expr -> expr
val mk_nonnull_poke : TcGlobals -> range -> expr -> expr
val mk_isinst_cond : TcGlobals -> range -> typ -> expr -> Val -> expr -> expr -> expr
val mk_throw   : range -> typ -> expr -> expr
val mk_ldarg0 : range -> typ -> expr

val mk_ilzero : range * typ -> expr

val mk_string    : TcGlobals -> range -> string -> expr
val mk_int64     : TcGlobals -> range -> int64 -> expr
val mk_bool      : TcGlobals -> range -> bool -> expr
val mk_byte      : TcGlobals -> range -> byte -> expr
val mk_uint16    : TcGlobals -> range -> uint16 -> expr
val mk_true      : TcGlobals -> range -> expr
val mk_false     : TcGlobals -> range -> expr
val mk_unit      : TcGlobals -> range -> expr
val mk_int32     : TcGlobals -> range -> int32 -> expr
val mk_int       : TcGlobals -> range -> int -> expr
val mk_zero      : TcGlobals -> range -> expr
val mk_one       : TcGlobals -> range -> expr
val mk_two       : TcGlobals -> range -> expr
val mk_minus_one : TcGlobals -> range -> expr
val dest_int32 : expr -> int32 option

//-------------------------------------------------------------------------
// Primitives associated with quotations
//------------------------------------------------------------------------- 
 
val mk_expr_ty : TcGlobals -> typ -> typ
val mk_raw_expr_ty : TcGlobals -> typ
val mspec_Type_GetTypeFromHandle : ILGlobals ->  ILMethodSpec
val fspec_Missing_Value : ILGlobals ->  ILFieldSpec
val mk_bytearray_ty : TcGlobals -> typ

//-------------------------------------------------------------------------
// Construct calls to some intrinsic functions
//------------------------------------------------------------------------- 

val mk_call_string_compare           : TcGlobals -> range -> expr -> expr -> expr
val mk_call_new_format              : TcGlobals -> range -> typ -> typ -> typ -> typ -> typ -> expr -> expr

val mk_call_unbox       : TcGlobals -> range -> typ -> expr -> expr
val mk_call_get_generic_comparer : TcGlobals -> range -> expr
val mk_call_get_generic_equality_comparer : TcGlobals -> range -> expr

val mk_call_unbox_fast  : TcGlobals -> range -> typ -> expr -> expr
val can_use_unbox_fast  : TcGlobals -> typ -> bool

val mk_call_dispose     : TcGlobals -> range -> typ -> expr -> expr
val mk_call_seq         : TcGlobals -> range -> typ -> expr -> expr

val mk_call_istype      : TcGlobals -> range -> typ -> expr -> expr
val can_use_istype_fast : TcGlobals -> typ -> bool

val mk_call_typeof      : TcGlobals -> range -> typ -> expr 

val mk_call_create_instance          : TcGlobals -> range -> typ -> expr
val mk_call_array_get                : TcGlobals -> range -> typ -> expr -> expr -> expr
val mk_call_raise                    : TcGlobals -> range -> typ -> expr -> expr

val mk_call_generic_comparison_outer       : TcGlobals -> range -> typ -> expr -> expr -> expr
val mk_call_generic_comparison_withc_outer : TcGlobals -> range -> typ -> expr -> expr -> expr -> expr
val mk_call_generic_equality_outer         : TcGlobals -> range -> typ -> expr -> expr -> expr
val mk_call_generic_equality_withc_outer   : TcGlobals -> range -> typ -> expr -> expr -> expr -> expr
//val mk_call_generic_hash_outer             : TcGlobals -> range -> typ -> expr -> expr
val mk_call_generic_hash_withc_outer       : TcGlobals -> range -> typ -> expr -> expr -> expr

val TryEliminateDesugaredConstants : TcGlobals -> range -> Constant -> expr option

val mk_call_unpickle_quotation     : TcGlobals -> range -> expr -> expr -> expr -> expr -> expr
val mk_call_cast_quotation     : TcGlobals -> range -> typ -> expr -> expr 
val mk_call_lift_value : TcGlobals -> range -> typ -> expr -> expr
val mk_call_seq_map_concat      : TcGlobals -> range -> typ  -> typ -> expr -> expr -> expr
val mk_call_seq_using           : TcGlobals -> range -> typ  -> typ -> expr -> expr -> expr
val mk_call_seq_delay           : TcGlobals -> range -> typ  -> expr -> expr
val mk_call_seq_append          : TcGlobals -> range -> typ -> expr -> expr -> expr
val mk_call_seq_finally         : TcGlobals -> range -> typ -> expr -> expr -> expr
val mk_call_seq_generated       : TcGlobals -> range -> typ -> expr -> expr -> expr
val mk_call_seq_of_functions    : TcGlobals -> range -> typ  -> typ -> expr -> expr -> expr -> expr
val mk_call_seq_to_array        : TcGlobals -> range -> typ  -> expr -> expr 
val mk_call_seq_to_list         : TcGlobals -> range -> typ  -> expr -> expr 
val mk_call_seq_map             : TcGlobals -> range -> typ  -> typ -> expr -> expr -> expr
val mk_call_seq_singleton       : TcGlobals -> range -> typ  -> expr -> expr
val mk_call_seq_empty           : TcGlobals -> range -> typ  -> expr
val mk_ceq                      : TcGlobals -> range -> expr -> expr -> expr

val mk_case : DecisionTreeDiscriminator * DecisionTree -> DecisionTreeCase

//-------------------------------------------------------------------------
// operations primarily associated with the optimization to fix
// up loops to generate .NET code that does not include array bound checks
//------------------------------------------------------------------------- 

val dest_incr : expr -> expr option
val mk_decr   : TcGlobals -> range -> expr -> expr
val mk_incr   : TcGlobals -> range -> expr -> expr
val mk_ldlen  : TcGlobals -> range -> expr -> expr

//-------------------------------------------------------------------------
// type-of operations on the expression tree
//------------------------------------------------------------------------- 

val type_of_expr : TcGlobals -> expr -> typ 

//-------------------------------------------------------------------------
// Analyze attribute sets 
//------------------------------------------------------------------------- 

val ILThingHasILAttrib : ILTypeRef -> ILAttributes -> bool
val ILThingDecodeILAttrib   : TcGlobals -> ILTypeRef -> ILAttributes -> (ILAttributeElement list * ILAttributeNamedArg list) option
val ILThingHasAttrib : Env.BuiltinAttribInfo -> ILAttributes -> bool

val IsMatchingAttrib      : TcGlobals -> Env.BuiltinAttribInfo -> Attrib -> bool
val HasAttrib             : TcGlobals -> Env.BuiltinAttribInfo -> Attribs -> bool
val TryFindAttrib         : TcGlobals -> Env.BuiltinAttribInfo -> Attribs -> Attrib option
val TryFindUnitAttrib     : TcGlobals -> Env.BuiltinAttribInfo -> Attribs -> unit option
val TryFindBoolAttrib     : TcGlobals -> Env.BuiltinAttribInfo -> Attribs -> bool option
val TryFindInt32Attrib    : TcGlobals -> Env.BuiltinAttribInfo -> Attribs -> int32 option

val TyconRefTryBindAttrib  : TcGlobals -> Env.BuiltinAttribInfo -> TyconRef -> ((ILAttributeElement list * ILAttributeNamedArg list) -> 'a option) -> (Attrib -> 'a option) -> 'a option
val TyconRefHasAttrib      : TcGlobals -> Env.BuiltinAttribInfo -> TyconRef -> bool

val IsCompilationMappingAttr    : ILAttribute -> bool
val IsSignatureDataVersionAttr  : ILAttribute -> bool
val ILThingHasExtensionAttribute : ILAttributes -> bool
val TryFindAutoOpenAttr           : ILAttribute -> string option 
val TryFindInternalsVisibleToAttr : ILAttribute -> string option 
val IsMatchingSignatureDataVersionAttr : ILVersionInfo -> ILAttribute -> bool


val mk_CompilationMappingAttr                         : TcGlobals -> int -> ILAttribute
val mk_CompilationMappingAttrWithSeqNum               : TcGlobals -> int -> int -> ILAttribute
val mk_CompilationMappingAttrWithVariantNumAndSeqNum  : TcGlobals -> int -> int -> int             -> ILAttribute
val mk_SignatureDataVersionAttr                       : TcGlobals -> ILVersionInfo -> ILAttribute
val mk_CompilerGeneratedAttr                          : TcGlobals -> int -> ILAttribute

val is_definitely_not_serializable : TcGlobals -> typ -> bool

//-------------------------------------------------------------------------
// More common type construction
//------------------------------------------------------------------------- 

val is_byref_typ : TcGlobals -> typ -> bool
val dest_byref_typ : TcGlobals -> typ -> typ

//-------------------------------------------------------------------------
// Tuple constructors/destructors
//------------------------------------------------------------------------- 

val typed_expr_for_val : range -> Val -> expr * typ

val mutable use_40_System_Types : bool

val is_tuple : expr -> bool
val try_dest_tuple : expr -> expr list
val mk_tupled : TcGlobals -> range -> expr list -> typ list -> expr 
val mk_tupled_notypes : TcGlobals -> range -> expr list -> expr 
val mk_tupled_ty : TcGlobals -> typ list -> typ
val mk_tupled_vars_ty : TcGlobals -> Val list -> typ
val mk_tupled_vars : TcGlobals -> range -> Val list -> expr 
val mk_meth_ty : TcGlobals -> typ list list -> typ -> typ

//-------------------------------------------------------------------------
// 
//------------------------------------------------------------------------- 

val AdjustValForExpectedArity : TcGlobals -> range -> ValRef -> ValUseFlag -> ValTopReprInfo -> expr * typ
val AdjustValToTopVal : Val -> ParentRef -> ValTopReprInfo -> unit
val LinearizeTopMatch : TcGlobals -> ParentRef -> expr -> expr
val AdjustPossibleSubsumptionExpr : TcGlobals -> expr -> expr list -> (expr * expr list) option
val NormalizeAndAdjustPossibleSubsumptionExprs : TcGlobals -> expr -> expr

//-------------------------------------------------------------------------
// XmlDoc signatures, used by both VS mode and XML-help emit
//------------------------------------------------------------------------- 

val XmlDocArgsEnc : TcGlobals -> typars -> typ list -> string
val XmlDocSigOfVal : TcGlobals -> string -> Val -> string
val XmlDocSigOfTycon : TcGlobals -> string -> Tycon -> string
val XmlDocSigOfSubModul : TcGlobals -> string -> string


//---------------------------------------------------------------------------
// Resolve static optimizations
//------------------------------------------------------------------------- 

val DecideStaticOptimizations : Env.TcGlobals -> Tast.StaticOptimization list -> int
val mk_static_optimization_expr     : Env.TcGlobals -> StaticOptimization list * expr * expr * range -> expr

//---------------------------------------------------------------------------
// Build for loops
//------------------------------------------------------------------------- 

val mk_fast_for_loop : Env.TcGlobals -> SequencePointInfoForForLoop * range * Val * expr * bool * expr * expr -> expr

//---------------------------------------------------------------------------
// Active pattern helpers
//------------------------------------------------------------------------- 

val is_ap_name : string -> bool
val name_of_apref : ActivePatternElemRef -> string
val apinfo_of_vname : string * range -> ActivePatternInfo option
val apinfo_of_vref  : ValRef -> ActivePatternInfo option
val mk_choices_ucref : Env.TcGlobals -> range -> int -> int -> Tast.UnionCaseRef

val names_of_apinfo      : ActivePatternInfo -> string list 
val total_of_apinfo      : ActivePatternInfo -> bool
val mk_apinfo_result_typ : Env.TcGlobals -> range -> ActivePatternInfo -> Tast.typ list-> Tast.typ
val mk_apinfo_typ        : Env.TcGlobals -> range -> ActivePatternInfo -> Tast.typ -> Tast.typ list-> Tast.typ

//---------------------------------------------------------------------------
// Structural rewrites
//------------------------------------------------------------------------- 

type ExprRewritingEnv = 
    {pre_intercept: ((expr -> expr) -> expr -> expr option) option;
     post_transform: expr -> expr option;
     under_quotations: bool }    

val RewriteExpr : ExprRewritingEnv -> expr -> expr
val RewriteImplFile : ExprRewritingEnv -> TypedImplFile -> TypedImplFile

val IsGenericValWithGenericContraints: TcGlobals -> Val -> bool
val tcaug_has_interface : TcGlobals -> TyconAugmentation -> Tast.typ -> bool
val tcaug_has_override : TcGlobals -> TyconAugmentation -> string -> Tast.typ list -> bool

val (|BitwiseOr|_|) : TcGlobals -> expr -> (expr * expr) option

val EvalConstantExpr : TcGlobals -> expr -> expr
val EvalAttribArg: TcGlobals -> expr -> expr

val (|SpecificAttribNamedArg|_|) : string -> AttribNamedArg -> expr option 
val (|AttribInt32Arg|_|) : AttribExpr -> int32 option
val (|AttribBoolArg|_|) : AttribExpr -> bool option
val (|AttribStringArg|_|) : AttribExpr -> string option
