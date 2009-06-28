// (c) Microsoft Corporation. All rights reserved
  
#light


#if STANDALONE_METADATA

module (* internal *) FSharp.PowerPack.Metadata.Reader.Internal.Tast

open System.Collections.Generic 
open FSharp.PowerPack.Metadata.Reader.Internal.AbstractIL.IL
open FSharp.PowerPack.Metadata.Reader.Internal.PrettyNaming
open FSharp.PowerPack.Metadata.Reader.Internal.Prelude


#else

module (* internal *) Microsoft.FSharp.Compiler.Tast 

open System.Collections.Generic 
open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types

open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.PrettyNaming
open Microsoft.FSharp.Compiler.Sreflect

open Microsoft.FSharp.Text.Printf
#endif

///verboseStamps: print #stamp on each id -- very verbose - but sometimes useful. Turn on using '--stamps'
let verboseStamps = ref false


/// Unique name generator for stamps attached to lambdas and object expressions
type uniq = int64
let new_uniq = let i = ref 0L in fun () -> i := !i + 1L; !i
type stamp = int64
/// Unique name generator for stamps attached to to val_specs, tycon_specs etc.
let new_stamp = let i = ref 0L in fun () -> i := !i + 1L; !i

//-------------------------------------------------------------------------
// Flags

type ValInlineInfo =
    /// Indicates the value must always be inlined 
    | PseudoValue 
    /// Indictes the value is inlined but the code for the function still exists, e.g. to satisfy interfaces on objects, but that it is also always inlined 
    | AlwaysInline 
    | OptionalInline
    | NeverInline

let mustinline = function PseudoValue | AlwaysInline -> true | OptionalInline | NeverInline -> false

type ValRecursiveScopeInfo =
    /// Set while the value is within its recursive scope. The flag indicates if the value has been eagerly generalized and accepts generic-recursive calls 
    | ValInRecScope of bool
    /// The normal value for this flag when the value is not within its recursive scope 
    | ValNotInRecScope

type ValMutability   = 
    | Immutable 
    | Mutable

type TyparDynamicReq = 
    /// Indicates the type parameter is not needed at runtime and may be eliminated
    | NoDynamicReq 
    /// Indicates the type parameter is needed at runtime and may not be eliminated
    | DynamicReq

type ValBaseOrThisInfo = 
    | CtorThisVal 
    | BaseVal 
    | NormalVal 
    | MemberThisVal

//---------------------------------------------------------------------------
// Flags on values
//---------------------------------------------------------------------------

module ValFlags = begin

    let base_of_vflags x =
                                  match (x           &&&       0b0000000000000110L) with 
                                                             | 0b0000000000000000L -> BaseVal
                                                             | 0b0000000000000010L -> CtorThisVal
                                                             | 0b0000000000000100L -> NormalVal
                                                             | 0b0000000000000110L -> MemberThisVal
                                                             | _          -> failwith "base_of_vflags"

    let encode_base_of_vflags x val_flags =
                     (val_flags &&&                         ~~~0b0000000000000110L)
                                ||| (match x with
                                        | BaseVal ->           0b0000000000000000L
                                        | CtorThisVal ->       0b0000000000000010L
                                        | NormalVal ->         0b0000000000000100L
                                        | MemberThisVal ->     0b0000000000000110L)



    let is_compgen_of_vflags x =           (x           &&&       0b0000000000001000L) <> 0x0L
    let encode_compgen_of_vflags b val_flags = 
                         if b then      (  val_flags |||       0b0000000000001000L) 
                         else           (  val_flags &&&   ~~~ 0b0000000000001000L)
    let inline_info_of_vflags x =
                                  match (x           &&&       0b0000000000110000L) with 
                                                             | 0b0000000000000000L -> PseudoValue
                                                             | 0b0000000000010000L -> AlwaysInline
                                                             | 0b0000000000100000L -> OptionalInline
                                                             | 0b0000000000110000L -> NeverInline
                                                             | _          -> failwith "inline_info_of_vflags"
    let encode_mustinline_of_vflags x val_flags =
                     (val_flags &&&                         ~~~0b0000000000110000L)
                                ||| (match x with
                                        | PseudoValue ->       0b0000000000000000L
                                        | AlwaysInline ->      0b0000000000010000L
                                        | OptionalInline ->    0b0000000000100000L
                                        | NeverInline ->       0b0000000000110000L)

    let mutability_of_vflags x =
                                  match (x           &&&       0b0000000001000000L) with 
                                                             | 0b0000000000000000L -> Immutable
                                                             | 0b0000000001000000L -> Mutable
                                                             | _          -> failwith "mutability_of_vflags"

    let encode_mutability_of_vflags x val_flags =
                     (val_flags &&&                         ~~~0b0000000001000000L)
                                ||| (match x with
                                        | Immutable ->         0b0000000000000000L
                                        | Mutable   ->         0b0000000001000000L)

    let is_topbind_of_vflags x =
                                  match (x           &&&       0b0000000010000000L) with 
                                                             | 0b0000000000000000L -> false
                                                             | 0b0000000010000000L -> true
                                                             | _          -> failwith "is_topbind_of_vflags"

    let encode_is_topbind_of_vflags x val_flags =
                     (val_flags &&&                         ~~~0b0000000010000000L)
                                ||| (match x with
                                        | false     ->         0b0000000000000000L
                                        | true      ->         0b0000000010000000L)

    let is_extension_member_of_vflags x =
                                  match (x           &&&       0b0000000100000000L) with 
                                                             | 0b0000000000000000L -> false
                                                             | 0b0000000100000000L -> true
                                                             | _          -> failwith "is_extension_member_of_vflags"

    let encode_isext_of_vflags x val_flags =
                     (val_flags &&&                         ~~~0b0000000100000000L)
                                ||| (match x with
                                        | false     ->         0b0000000000000000L
                                        | true      ->         0b0000000100000000L)
    let is_incr_class_of_vflags x =
                                  match (x           &&&       0b0000001000000000L) with 
                                                             | 0b0000000000000000L -> false
                                                             | 0b0000001000000000L -> true
                                                             | _          -> failwith "is_incr_class_of_vflags"

    let encode_is_incr_class_of_vflags x val_flags =
                     (val_flags &&&                         ~~~0b0000001000000000L)
                                ||| (match x with
                                        | false     ->         0b0000000000000000L
                                        | true      ->         0b0000001000000000L)

    let is_tyfunc_of_vflags x =
                                  match (x           &&&       0b0000010000000000L) with 
                                                             | 0b0000000000000000L -> false
                                                             | 0b0000010000000000L -> true
                                                             | _          -> failwith "is_incr_class_of_vflags"

    let encode_is_tyfunc_of_vflags x val_flags =
                     (val_flags &&&                         ~~~0b0000010000000000L)
                                ||| (match x with
                                        | false     ->         0b0000000000000000L
                                        | true      ->         0b0000010000000000L)

    let vrec_of_vflags x =       match  (x           &&&       0b0001100000000000L) with 
                                                             | 0b0000000000000000L -> ValNotInRecScope
                                                             | 0b0000100000000000L -> ValInRecScope(true)
                                                             | 0b0001000000000000L -> ValInRecScope(false)
                                                             | _                   -> failwith "vrec_of_vflags"
    let encode_vrec_of_vflags x val_flags =
                    (val_flags &&&                          ~~~0b0001100000000000L)
                               |||  (match x with
                                     | ValNotInRecScope     -> 0b0000000000000000L
                                     | ValInRecScope(true)  -> 0b0000100000000000L
                                     | ValInRecScope(false) -> 0b0001000000000000L)

    let is_notailcall_hint_of_vflags x =
                                  match (x           &&&       0b0010000000000000L) with 
                                                             | 0b0000000000000000L -> false
                                                             | 0b0010000000000000L -> true
                                                             | _          -> failwith "is_notailcall_hint_of_vflags"

    let encode_notailcall_hint_of_vflags x val_flags =
                     (val_flags &&&                         ~~~0b0010000000000000L)
                                ||| (match x with
                                        | false     ->         0b0000000000000000L
                                        | true      ->         0b0010000000000000L)

    let encode (vrec,baseOrThis,isCompGen,mustinline,isMutable,isTopBinding,isExtensionMember,isImplicitCtor,isTyFunc) =
        0L |> encode_vrec_of_vflags      vrec 
           |> encode_base_of_vflags       baseOrThis
           |> encode_compgen_of_vflags    isCompGen
           |> encode_mustinline_of_vflags mustinline
           |> encode_mutability_of_vflags isMutable
           |> encode_is_topbind_of_vflags    isTopBinding
           |> encode_isext_of_vflags      isExtensionMember
           |> encode_is_incr_class_of_vflags      isImplicitCtor
           |> encode_is_tyfunc_of_vflags      isTyFunc
end



type TyparKind = 
    | KindType 
    | KindMeasure
    member x.AttrName =
      match x with
      | KindType -> None
      | KindMeasure -> Some "Measure"
    override x.ToString() = 
      match x with
      | KindType -> "type"
      | KindMeasure -> "measure"


type TyparRigidity = 
    /// Indicates the type parameter can't be solved
    | TyparRigid 
    /// Indicates we give a warning if the type parameter is ever solved
    | TyparWarnIfNotRigid 
    /// Indicates the type parameter is an inference variable may be solved
    | TyparFlexible
    /// Indicates the type parameter derives from an '_' anonymous type
    /// For units-of-measure, we give a warning if this gets solved to '1'
    | TyparAnon


module TyparFlags = begin

    (* encode typar flags into a bit field  *)

    let from_error_of_tpflags x = (x       &&&     0b00000000010) <> 0x0
    let encode_from_error_of_tpflags b typar_flags = 
             if b then      (  typar_flags |||     0b00000000010) 
             else           (  typar_flags &&& ~~~ 0b00000000010)

    let compgen_of_tpflags x = (x          &&&     0b00000000100) <> 0x0
    let encode_compgen_of_tpflags b typar_flags = 
             if b then      (  typar_flags |||     0b00000000100) 
             else           (  typar_flags &&& ~~~ 0b00000000100)

    let static_req_of_tpflags x =
                      match (x             &&&     0b00000001000) with 
                                                 | 0b00000000000 -> NoStaticReq
                                                 | 0b00000001000 -> HeadTypeStaticReq
                                                 | _             -> failwith "static_req_of_tpflags"

    let encode_static_req_of_tpflags x typar_flags =
                     (typar_flags &&&           ~~~0b00000001000)
                      ||| (match x with
                            | NoStaticReq ->       0b00000000000
                            | HeadTypeStaticReq -> 0b00000001000)


    let rigid_of_tpflags x =            
                      match (x             &&&     0b00001100000) with 
                                                 | 0b00000000000 -> TyparRigid
                                                 | 0b00000100000 -> TyparWarnIfNotRigid
                                                 | 0b00001000000 -> TyparFlexible
                                                 | 0b00001100000 -> TyparAnon
                                                 | _          -> failwith "rigid_of_tpflags"
    let encode_rigid_of_tpflags x typar_flags =
         (typar_flags &&&                       ~~~0b00001100000)
                    ||| (match x with
                          | TyparRigid          -> 0b00000000000
                          | TyparWarnIfNotRigid -> 0b00000100000
                          | TyparFlexible       -> 0b00001000000
                          | TyparAnon           -> 0b00001100000)

    let kind_of_tpflags x = 
                      match (x             &&&     0b00010000000) with 
                                                 | 0b00000000000 -> KindType
                                                 | 0b00010000000 -> KindMeasure
                                                 | _          -> failwith "kind_of_tpflags"

    let encode_kind_of_tpflags x typar_flags =
         (typar_flags &&&                       ~~~0b00010000000)
                      ||| (match x with
                            | KindType          -> 0b00000000000
                            | KindMeasure       -> 0b00010000000)

    let dynamic_req_of_tpflags x =
                      match (x             &&&     0b01000000000) with 
                                                 | 0b00000000000 -> NoDynamicReq
                                                 | 0b01000000000 -> DynamicReq
                                                 | _             -> failwith "dynamic_req_of_tpflags"

    let encode_dynamic_req_of_tpflags x typar_flags =
                     (typar_flags &&&           ~~~0b01000000000)
                      ||| (match x with
                            | NoDynamicReq ->      0b00000000000
                            | DynamicReq ->        0b01000000000)


    let encode (kind,rigid,isFromError,isCompGen,staticReq,dynamicReq) =
        0 |> encode_kind_of_tpflags kind
          |> encode_rigid_of_tpflags      rigid 
          |> encode_from_error_of_tpflags isFromError
          |> encode_compgen_of_tpflags    isCompGen
          |> encode_static_req_of_tpflags staticReq
          |> encode_dynamic_req_of_tpflags dynamicReq

end

let unassignedTyparName = "?"

exception UndefinedName of int * string * ident * string list
exception InternalUndefinedItemRef of string * string * string * string


// Type definitions, exception definitions, module definitions and
// namespace definitions are all 'entities'. These have too much in common to make it 
// worth factoring them out as separate types.
//
// Tycons, exncs and moduls are all modelled via tycon_specs, 
// they have different name-resolution logic. 
// For example, an excon ABC really correspond to a type called 
// ABCException with a union case ABC. At the moment they are 
// simply indexed in the excon table as the discriminator constructor ABC. 
type Entity = 
    { mutable Data: EntityData; }
    member x.MangledName = x.Data.entity_name
    member x.DisplayName = DemangleGenericTypeName x.Data.entity_name
    member x.DisplayNameWithUnderscoreTypars = 
        let nm = x.DisplayName 
        match x.Typars(x.Range) with 
        | [] -> x.DisplayName
        | tps -> x.DisplayName + "<" + String.concat "," (Array.create tps.Length "_") + ">"
    
    member x.Range = x.Data.entity_range
    member x.Stamp = x.Data.entity_stamp
    member x.Attribs = x.Data.entity_attribs
    member x.XmlDoc = x.Data.entity_xmldoc
    member x.ModuleOrNamespaceType = x.Data.entity_modul_contents.Force()
    
    member x.TypeContents = x.Data.entity_tycon_tcaug
    member x.TypeOrMeasureKind = x.Data.entity_kind
    member x.Id = ident(x.MangledName, x.Range)
    member x.TypeReprInfo = x.Data.entity_tycon_repr
    member x.ExceptionInfo = x.Data.entity_exn_info
    member x.IsExceptionDecl = match x.ExceptionInfo with TExnNone -> false | _ -> true
    member x.DemangledExceptionName =  
        let nm = x.MangledName
        if x.IsExceptionDecl then DemangleExceptionName nm else nm 
    
    member x.Typars(m) = x.Data.entity_typars.Force(m) // lazy because it may read metadata, must provide a context "range" in case error occurs reading metadata
    member x.TyparsNoRange = x.Typars(x.Range)
    member x.TypeAbbrev = x.Data.entity_tycon_abbrev
    member x.IsTypeAbbrev = x.TypeAbbrev.IsSome
    member x.TypeReprAccessibility = x.Data.entity_tycon_repr_accessibility
    member x.CompiledReprCache = x.Data.entity_il_repr_cache
    member x.PublicPath = x.Data.entity_pubpath
    member x.Accessibility = x.Data.entity_accessiblity
    member x.IsPrefixDisplay = x.Data.entity_uses_prefix_display
    member x.IsModuleOrNamespace = x.Data.entity_is_modul_or_namespace
    member x.IsNamespace = x.IsModuleOrNamespace && (match x.ModuleOrNamespaceType.ModuleOrNamespaceKind with Namespace -> true | _ -> false)
    member x.IsModule = x.IsModuleOrNamespace && (match x.ModuleOrNamespaceType.ModuleOrNamespaceKind with Namespace -> false | _ -> true)
    member x.CompilationPathOpt = x.Data.entity_cpath 
    member x.CompilationPath = match x.CompilationPathOpt with Some cpath -> cpath | None -> error(Error("type/module "^x.MangledName^" is not a concrete module or type",x.Range))
    
    member x.AllFieldTable = 
        match x.TypeReprInfo with 
        | Some (TRecdRepr x | TFsObjModelRepr {fsobjmodel_rfields=x}) -> x
        |  _ -> 
        match x.ExceptionInfo with 
        | TExnFresh x -> x
        | _ -> 
        { rfields_by_index = [| |]; 
          rfields_by_name = NameMap.empty }

    member x.AllFieldsArray = x.AllFieldTable.rfields_by_index
    member x.AllFieldsAsList = x.AllFieldsArray |> Array.to_list

    // NOTE: This method is over-used...
    member x.AllInstanceFieldsAsList = x.AllFieldsAsList |> List.filter (fun f -> not f.IsStatic)
    member x.TrueFieldsAsList = x.AllFieldsAsList |> List.filter (fun f -> not f.IsCompilerGenerated)
    member x.TrueInstanceFieldsAsList = x.AllFieldsAsList |> List.filter (fun f -> not f.IsStatic && not f.IsCompilerGenerated)

    member x.GetFieldByIndex(n) = x.AllFieldTable.FieldByIndex(n)
    member x.GetFieldByName(n) = x.AllFieldTable.FieldByName(n)

    member x.UnionTypeInfo = 
        match x.Data.entity_tycon_repr with 
        | Some (TFiniteUnionRepr x) -> Some x 
        |  _ -> None

    member x.UnionCasesArray = 
        match x.UnionTypeInfo with 
        | Some x -> x.funion_ucases.ucases_by_index 
        | None -> [| |] 

    member x.UnionCasesAsList = x.UnionCasesArray |> Array.to_list

    member x.GetUnionCaseByName(n) =
        match x.UnionTypeInfo with 
        | Some x  -> NameMap.tryfind n x.funion_ucases.ucases_by_name
        | None -> None

    
    // OSGN support
    static member NewUnlinked() : Entity = { Data = nullable_slot_empty() }
    static member New reason data : Entity  = 
        if !verboseStamps then 
            dprintf "entity %s#%d (%s)\n" data.entity_name data.entity_stamp reason;
        { Data = data }
    member x.Link(tg) = x.Data <- nullable_slot_full(tg)
    member x.IsLinked = match box x.Data with null -> false | _ -> true 

    override x.ToString() = x.MangledName

    member x.FSharpObjectModelTypeInfo = 
         match x.Data.entity_tycon_repr with 
         | Some (TFsObjModelRepr x) -> x 
         |  _ -> failwith "not an F# object model type definition"

    member x.IsILTycon = match x.TypeReprInfo with | Some (TILObjModelRepr _) -> true |  _ -> false
    member x.ILTyconInfo = match x.TypeReprInfo with | Some (TILObjModelRepr (a,b,c)) -> (a,b,c) |  _ -> failwith "not a .NET type definition"
    member x.ILTyconRawMetadata = let _,_,td = x.ILTyconInfo in td

    member x.IsUnionTycon = match x.TypeReprInfo with | Some (TFiniteUnionRepr _) -> true |  _ -> false
    member x.UnionInfo = match x.TypeReprInfo with | Some (TFiniteUnionRepr x) -> Some x |  _ -> None

    member x.IsRecordTycon = match x.TypeReprInfo with | Some (TRecdRepr _) -> true |  _ -> false
    member x.IsFSharpObjectModelTycon = match x.TypeReprInfo with | Some (TFsObjModelRepr _) -> true |  _ -> false
    member x.IsAsmReprTycon = match x.TypeReprInfo with | Some (TAsmRepr _) -> true |  _ -> false
    member x.IsMeasureableReprTycon = match x.TypeReprInfo with | Some (TMeasureableRepr _) -> true |  _ -> false
    member x.IsHiddenReprTycon = match x.TypeAbbrev,x.TypeReprInfo with | None,None -> true |  _ -> false

    member x.IsFSharpInterfaceTycon =  x.IsFSharpObjectModelTycon && match x.FSharpObjectModelTypeInfo.fsobjmodel_kind with TTyconInterface -> true | _ -> false
    member x.IsFSharpDelegateTycon =  x.IsFSharpObjectModelTycon && match x.FSharpObjectModelTypeInfo.fsobjmodel_kind with TTyconDelegate _ -> true | _ -> false
    member x.IsFSharpEnumTycon =  x.IsFSharpObjectModelTycon && match x.FSharpObjectModelTypeInfo.fsobjmodel_kind with TTyconEnum -> true | _ -> false

    member x.IsFSharpStructTycon =
        x.IsFSharpObjectModelTycon &&
        match x.FSharpObjectModelTypeInfo.fsobjmodel_kind with 
        | TTyconClass | TTyconInterface   | TTyconDelegate _ -> false
        | TTyconStruct | TTyconEnum -> true

    member x.IsILStructTycon =
        x.IsILTycon && 
        let tdef = x.ILTyconRawMetadata
        match tdef.tdKind with
        | TypeDef_valuetype | TypeDef_enum -> true
        | _ -> false

    member x.IsStructTycon = 
        x.IsILStructTycon || x.IsFSharpStructTycon

    /// From TAST TyconRef to IL ILTypeRef
    member x.CompiledRepresentation =

        let il_tref_for_cpath (CompPath(sref,p)) item = 
            let rec top racc  p = 
                match p with 
                | [] -> ILTypeRef.Create(sref,[],text_of_path  (List.rev (item::racc)))
                | (h,istype)::t -> 
                    match istype with 
                    | FSharpModuleWithSuffix | FSharpModule -> 
                        let outerTypeName = (text_of_path (List.rev (h::racc)))
                        ILTypeRef.Create(sref, (outerTypeName :: List.map (fun (nm,_) -> nm) t),item)
                    | _ -> 
                      top (h::racc) t
            top [] p 


        assert(not x.IsTypeAbbrev);
        cached x.CompiledReprCache (fun () -> 
            match x.ExceptionInfo with 
            | TExnAbbrevRepr ecref2 -> ecref2.CompiledRepresentation
            | TExnAsmRepr tref -> TyrepNamed(tref,AsObject)
            | _ -> 
            match x.TypeReprInfo with 
            | Some (TAsmRepr typ) -> TyrepOpen typ
            | _ -> 
                let boxity = if x.IsStructTycon then AsValue else AsObject
                TyrepNamed (il_tref_for_cpath x.CompilationPath x.MangledName,boxity))


    member x.CompiledRepresentationForTyrepNamed =
        match x.CompiledRepresentation with 
        | TyrepNamed(tref,_) -> tref
        | TyrepOpen _ -> invalidOp (sprintf "the type %s has an assembly code representation" x.DisplayNameWithUnderscoreTypars)


and 
 [<StructuralEquality(false); StructuralComparison(false)>]
 EntityData =
    { /// The declared type parameters of the type  
      // MUTABILITY; used only during creation and remapping  of tycons 
      mutable entity_typars: LazyWithContext<typars,range>;        

      // MUTABILITY; used only when establishing tycons. 
      // REVIEW: remove this use of mutabilty 
      mutable entity_kind : TyparKind;
      
      /// The unique stamp of the "tycon blob". Note the same tycon in signature and implementation get different stamps 
      entity_stamp: stamp;

      /// The name of the type, possibly with `n mangling 
      entity_name: string;

      /// The declaration location for the type constructor 
      entity_range: range;
      
      /// Indicates the type prefers the "tycon<a,b>" syntax for display etc. 
      entity_uses_prefix_display: bool;                   
      
      /// Indicates the "tycon blob" is actually a module 
      entity_is_modul_or_namespace : bool; 

      /// The declared accessibility of the representation, not taking signatures into account 
      entity_tycon_repr_accessibility: Accessibility;
      
      /// The declared attributes for the type 
      (* MUTABILITY; used only during creation and remapping of tycons *)
      mutable entity_attribs: Attribs;     
                
      /// The declared representation of the type, i.e. record, union, class etc. 
      //
      // REVIEW: the 'None' value here has two meanings
      //     - it indicates 'not yet known' during the first 2 phases of establishing type definitions
      //     - it indicated 'no representation' at all other times, i.e. 
      //           type X
      //       in signatures 
      //   It would be better to separate these two cases out, by just adding two cases
      //   to TyconRepresentation and removing the use of 'option'
      //
      // MUTABILITY; used only during creation and remapping of tycons 
      mutable entity_tycon_repr: TyconRepresentation option;   

      /// If non-None, indicates the type is an abbreviation for another type. 
      mutable entity_tycon_abbrev: typ option;             (* MUTABILITY; used only during creation and remapping of tycons *)
      
      /// The methods and properties of the type 
      mutable entity_tycon_tcaug: TyconAugmentation;      (* MUTABILITY; used only during creation and remapping of tycons *)
      
      /// Field used when the 'tycon' is really an exception definition 
      (* MUTABILITY; used only during creation and remapping of tycons *)
      mutable entity_exn_info: ExceptionInfo;     
      
      /// This field is used when the 'tycon' is really a module definition. It holds statically nested type definitions and nested modules 
      (* MUTABILITY: only used during creation and remapping  of tycons and *)
      (* when compiling fslib to fixup compiler forward references to internal items *)
      mutable entity_modul_contents: Lazy<ModuleOrNamespaceType>;     

      /// The declared documentation for the type or module 
      entity_xmldoc : XmlDoc;

      /// The stable path to the type, e.g. Microsoft.FSharp.Core.FastFunc`2 
      (* REVIEW: it looks like entity_cpath subsumes this *)
      entity_pubpath : PublicPath option; (*   where does this live? *)

      mutable entity_accessiblity: Accessibility; (*   how visible is this? *)  (* MUTABILITY; used only during creation and remapping  of tycons *)
 
      /// The stable path to the type, e.g. Microsoft.FSharp.Core.FastFunc`2 
      entity_cpath : CompilationPath option; 

      /// Used during codegen to hold the ILX representation indicating how to access the type 
      entity_il_repr_cache : CompiledTypeRepr cache;  (* MUTABILITY; *)

    }

and ParentRef = 
    | Parent of TyconRef
    | ParentNone
    
and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  TyconAugmentation = 
    { /// This is the value implementing the auto-generated comparison 
      /// semantics if any. It is not present if the type defines its own implementation 
      /// of IComparable or if the type doesn't implement IComparable implicitly. 
      mutable tcaug_compare        : (ValRef * ValRef) option;
      
      /// This is the value implementing the auto-generated comparison
      /// semantics if any. It is not present if the type defines its own implementation
      /// of IStructuralComparable or if the type doesn't implement IComparable implicitly.
      mutable tcaug_compare_withc : ValRef option;                      

      /// This is the value implementing the auto-generated equality 
      /// semantics if any. It is not present if the type defines its own implementation 
      /// of Object.Equals or if the type doesn't override Object.Equals implicitly. 
      mutable tcaug_equals        : (ValRef * ValRef) option;

      /// This is the value implementing the auto-generated comparison
      /// semantics if any. It is not present if the type defines its own implementation
      /// of IStructuralEquatable or if the type doesn't implement IComparable implicitly.
      mutable tcaug_hash_and_equals_withc : (ValRef * ValRef) option;                                    

      /// True if the type defined an Object.GetHashCode method. In this 
      /// case we give a warning if we auto-generate a hash method since the semantics may not match up
      mutable tcaug_hasObjectGetHashCode : bool;             
      
      /// Likewise IStructuralHash::GetHashCode 
      mutable tcaug_structural_hash: ValRef option;             
      
      /// Properties, methods etc. 
      mutable tcaug_adhoc          : (ValRef list) NameMap;
      
      /// Interface implementations - boolean indicates compiler-generated 
      mutable tcaug_implements     : (typ * bool * range) list;  
      
      /// Super type, if any 
      mutable tcaug_super          : typ option;                 
      
      /// Set to true at the end of the scope where proper augmentations are allowed 
      mutable tcaug_closed         : bool;                       

      /// Set to true if the type is determined to be abstract 
      mutable tcaug_abstract : bool;                       
    }
   
and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  TyconRepresentation = 
    /// Indicates the type is a class, struct, enum, delegate or interface 
    | TFsObjModelRepr    of TyconObjModelData
    /// Indicates the type is a record 
    | TRecdRepr          of TyconRecdFields
    /// Indicates the type is a discriminated union 
    | TFiniteUnionRepr   of TyconUnionData 
    /// Indicates the type is a .NET type 
    | TILObjModelRepr    of 
          // scope: 
          ILScopeRef * 
          // nesting:   
          ILTypeDef list * 
          // definition: 
          ILTypeDef 
    /// Indicates the type is implemented as IL assembly code using the given closed Abstract IL type 
    | TAsmRepr           of ILType
    /// Indicates the type is parameterized on a measure (e.g. float<_>) but erases to some other type (e.g. float)
    | TMeasureableRepr   of typ


and 
  TyconObjModelKind = 
    /// Indicates the type is a class (also used for units-of-measure)
    | TTyconClass 
    /// Indicates the type is an interface 
    | TTyconInterface 
    /// Indicates the type is a struct 
    | TTyconStruct 
    /// Indicates the type is a delegate with the given Invoke signature 
    | TTyconDelegate of SlotSig 
    /// Indicates the type is an enumeration 
    | TTyconEnum
    
and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  TyconObjModelData = 
    { /// Indicates whether the type declaration is a class, interface, enum, delegate or struct 
      fsobjmodel_kind: TyconObjModelKind;
      /// The declared abstract slots of the class, interface or struct 
      fsobjmodel_vslots: ValRef list; 
      /// The fields of the class, struct or enum 
      fsobjmodel_rfields: TyconRecdFields }

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  TyconRecdFields = 
    { /// The fields of the record, in declaration order. 
      rfields_by_index: RecdField array;
      
      /// The fields of the record, indexed by name. 
      rfields_by_name : RecdField NameMap  }

    member x.FieldByIndex(n) = 
        if n >= 0 && n < Array.length x.rfields_by_index then x.rfields_by_index.[n] 
        else failwith "FieldByIndex"

    member x.FieldByName(n) = x.rfields_by_name.TryFind(n)

    member x.AllFieldsAsList = x.rfields_by_index |> Array.to_list
    member x.TrueFieldsAsList = x.AllFieldsAsList |> List.filter (fun f -> not f.IsCompilerGenerated)   
    member x.TrueInstanceFieldsAsList = x.AllFieldsAsList |> List.filter (fun f -> not f.IsStatic && not f.IsCompilerGenerated)   

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  TyconUnionCases = 
    { /// The cases of the discriminated union, in declaration order. 
      ucases_by_index: UnionCase array;
      /// The cases of the discriminated union, indexed by name. 
      ucases_by_name : UnionCase NameMap 
    }
    member x.GetUnionCaseByIndex(n) = 
        if n >= 0 && n < x.ucases_by_index.Length then x.ucases_by_index.[n] 
        else invalidArg "n" "GetUnionCaseByIndex"

    member x.UnionCasesAsList = x.ucases_by_index |> Array.to_list

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  TyconUnionData =
    { /// The cases contained in the discriminated union. 
      funion_ucases: TyconUnionCases;
      /// The ILX data structure representing the discriminated union. 
#if STANDALONE_METADATA
#else
      funion_ilx_repr: IlxUnionRef cache; 
#endif
    }
    member x.UnionCasesAsList = x.funion_ucases.ucases_by_index |> Array.to_list

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  UnionCase =
    { /// Data carried by the case. 
      ucase_rfields: TyconRecdFields;
      /// Return type constructed by the case. Normally exactly the type of the enclosing type, sometimes an abbreviation of it 
      ucase_rty: typ;
      /// Name of the case in generated IL code 
      ucase_il_name: string;
      /// Documentation for the case 
      ucase_xmldoc : XmlDoc;
      /// Name/range of the case 
      ucase_id: ident; 
      ///  Indicates the declared visibility of the union constructor, not taking signatures into account 
      ucase_access: Accessibility; 
      /// Attributes, attached to the generated static method to make instances of the case 
      ucase_attribs: Attribs; }

    member uc.Attribs = uc.ucase_attribs
    member uc.Range = uc.ucase_id.idRange
    member uc.Id = uc.ucase_id
    member uc.Accessibility = uc.ucase_access
    member uc.DisplayName = uc.Id.idText
    member uc.RecdFieldsArray = uc.ucase_rfields.rfields_by_index 
    member uc.RecdFields = uc.ucase_rfields.rfields_by_index |> Array.to_list
    member uc.GetFieldByName nm = uc.ucase_rfields.FieldByName nm
    member uc.IsNullary = (uc.ucase_rfields.rfields_by_index.Length = 0)

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  RecdField =
    { /// Is the field declared mutable in F#? 
      rfield_mutable: bool;
      /// Documentation for the field 
      rfield_xmldoc : XmlDoc;
      /// The type of the field, w.r.t. the generic parameters of the enclosing type constructor 
      rfield_type: typ;
      /// Indicates a static field 
      rfield_static: bool;
      /// Indicates a compiler generated field, not visible to Intellisense or name resolution 
      rfield_secret: bool;
      /// The default initialization info, for static literals 
      rfield_const: Constant option; 
      ///  Indicates the declared visibility of the field, not taking signatures into account 
      rfield_access: Accessibility; 
      /// Attributes attached to generated property 
      rfield_pattribs: Attribs; 
      /// Attributes attached to generated field 
      rfield_fattribs: Attribs; 
      /// Name/declaration-location of the field 
      rfield_id: ident; }
    member v.Accessibility = v.rfield_access
    member v.PropertyAttribs = v.rfield_pattribs
    member v.FieldAttribs = v.rfield_fattribs
    member v.Range = v.rfield_id.idRange
    member v.Id = v.rfield_id
    member v.Name = v.rfield_id.idText
    member v.IsCompilerGenerated = v.rfield_secret
    member v.IsMutable = v.rfield_mutable
    member v.IsStatic = v.rfield_static
    member v.FormalType = v.rfield_type
    member v.LiteralValue = 
        match v.rfield_const  with 
        | None -> None
        | Some(TConst_zero) -> None
        | Some(k) -> Some(k)

    member v.IsZeroInit = 
        match v.rfield_const  with 
        | None -> false 
        | Some(TConst_zero) -> true 
        | _ -> false

and ExceptionInfo =
    /// Indicates that an exception is an abbreviation for the given exception 
    | TExnAbbrevRepr of TyconRef 
    /// Indicates that an exception is shorthand for the given .NET exception type 
    | TExnAsmRepr of ILTypeRef
    /// Indicates that an exception carries the given record of values 
    | TExnFresh of TyconRecdFields
    /// Indicates that an exception is abstract, i.e. is in a signature file, and we do not know the representation 
    | TExnNone

and ModuleOrNamespaceKind = 
    /// Indicates that a module is compiled to a class with the "Module" suffix added. 
    | FSharpModuleWithSuffix 
    /// Indicates that a module is compiled to a class with the same name as the original module 
    | FSharpModule 
    /// Indicates that a 'module' is really a namespace 
    | Namespace

and 
    [<Sealed>]
    ModuleOrNamespaceType(kind: ModuleOrNamespaceKind, vals: Val NameMap, entities: Entity NameMap) = 

      let mutable entities = entities 
      
      /// Lookup tables keyed the way various clients expect them to be keyed.
      /// We attach them here so we don't need to store lookup tables via any other technique 
      let apref_cache : ActivePatternElemRef NameMap option ref = ref None
      let modulesByDemangledName_cache       : ModuleOrNamespace NameMap option ref = ref None
      let exconsByDemangledName_cache        : Tycon NameMap option ref = ref None
      let tyconsByDemangledNameAndArity_cache: (Map<NameArityPair, Tycon>) option ref= ref None
      let tyconsByAccessNames_cache          : NameMultiMap<Tycon> option ref = ref None
      let tyconsByMangledName_cache          : Tycon NameMap option ref = ref None
  
  
      /// Namespace or module-compiled-as-type? 
      member mtyp.ModuleOrNamespaceKind = kind 
              
      /// Values, including members in F# types in this module-or-namespace-fragment. 
      member mtyp.AllValuesAndMembers = vals
      
      /// Type, mapping mangled name to Tycon, e.g. 
      ////     "Dictionary`2" --> Tycon 
      ////     "ListModule" --> Tycon with module info
      ////     "FooException" --> Tycon with exception info
      member mtyp.AllEntities = entities

      /// Mutation used during compilation of FSharp.Core.dll
      member mtyp.AddModuleOrNamespaceByMutation(modul:ModuleOrNamespace) =
          entities <- Map.add modul.MangledName modul entities;
          modulesByDemangledName_cache := None          
          
      member mtyp.AddEntity(tycon:Tycon) = 
          new ModuleOrNamespaceType(mtyp.ModuleOrNamespaceKind, mtyp.AllValuesAndMembers, Map.add tycon.MangledName tycon mtyp.AllEntities)
          
      member mtyp.AddVal(vspec:Val) = 
          new ModuleOrNamespaceType(mtyp.ModuleOrNamespaceKind, Map.add vspec.MangledName vspec mtyp.AllValuesAndMembers, mtyp.AllEntities)
          
      /// Lookup tables keyed the way various clients expect them to be keyed.
      /// We attach them here so we don't need to store lookup tables via any other technique 
      member mtyp.ActivePatternsLookupTable               = apref_cache
      member mtyp.ModulesAndNamespacesLookupTable         = modulesByDemangledName_cache
      member mtyp.FSharpExceptionsLookupTable             = exconsByDemangledName_cache
      member mtyp.TypesByDemangledNameAndArityLookupTable = tyconsByDemangledNameAndArity_cache
      member mtyp.TypesByAccessNamesLookupTable           = tyconsByAccessNames_cache
      member mtyp.TypesByMangledNameLookupTable           = tyconsByMangledName_cache
  

and ModuleOrNamespace = Entity 
and Tycon = Entity 

and Accessibility = 
    /// TAccess(...,path,...) indicates the construct  can only be accessed from any code in the given type constructor, module or assembly. [] indicates global scope. 
    | TAccess of CompilationPath list
    
and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  TyparData = 
    { mutable typar_id: ident; (* MUTABILITY: we set the names of generalized inference type parameters to make the look nice for IL code generation *)
       
      mutable typar_flags: int32;
       (*
          FLAGS ARE LOGICALLY: 
              (* MUTABILITY CLEANUP: could create fresh rigid variables and equate these to them. *) 
              mutable typar_rigid: bool;                                 (* cannot unify: quantified.  Mutated when inference decides to generalize. *)
              typar_from_error: bool;                                    (* typar was generated as part of error recovery *)
              typar_compgen: bool;
              mutable typar_static_req: TyparStaticReq;                  (* true for $a types or any tyvars in types equated with $a types - these may not be generalized *)
       *)
       
       /// The unique stamp of the typar blob. 
      typar_stamp: stamp; 
       
       /// The documentation for the type parameter. Empty for type inference variables.
      typar_xmldoc : XmlDoc;
       
       /// The declared attributes of the type parameter. Empty for type inference variables. 
      mutable typar_attribs: Attribs;                      
       
       /// An inferred equivalence for a type inference variable. 
       (* Note: this is the most important mutable state in all of F#! *)
      mutable typar_solution: typ option;
       
       /// The inferred constraints for the type inference variable 
       (* Note: along with typar_solution, this is the most important mutable state in all of F#! *)
      mutable typar_constraints: TyparConstraint list; 
    } 

and 
  [<ReferenceEquality(true)>]
  Typar = 
    { mutable Data: TyparData;
      mutable AsType: typ }
    member x.Name                = x.Data.typar_id.idText
    member x.Range               = x.Data.typar_id.idRange
    member x.Id                  = x.Data.typar_id
    member x.Stamp               = x.Data.typar_stamp
    member x.Solution            = x.Data.typar_solution
    member x.Constraints         = x.Data.typar_constraints
    member x.IsCompilerGenerated = x.Data.typar_flags |> TyparFlags.compgen_of_tpflags
    member x.Rigidity            = x.Data.typar_flags |> TyparFlags.rigid_of_tpflags
    member x.DynamicReq          = x.Data.typar_flags |> TyparFlags.dynamic_req_of_tpflags
    member x.StaticReq           = x.Data.typar_flags |> TyparFlags.static_req_of_tpflags
    member x.IsFromError         = x.Data.typar_flags |> TyparFlags.from_error_of_tpflags
    member x.Kind                = x.Data.typar_flags |> TyparFlags.kind_of_tpflags
    member x.IsErased            = match x.Kind with KindType -> false | _ -> true
    member x.Attribs             = x.Data.typar_attribs
    member x.DisplayName = let nm = x.Name in if nm = "?" then "?"^string x.Stamp else nm

    // OSGN support
    static member NewUnlinked() : Typar  = 
        let res = { Data = nullable_slot_empty(); AsType=Unchecked.defaultof<_> }
        res.AsType <- TType_var res
        res
    static member New(data) : Typar = 
        let res = { Data = data; AsType=Unchecked.defaultof<_> }
        res.AsType <- TType_var res
        res
    member x.Link(tg) = x.Data <- nullable_slot_full(tg)
    member x.IsLinked = match box x.Data with null -> false | _ -> true 

    override x.ToString() = x.Name

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  TyparConstraint = 
    /// Indicates a constraint that a type is a subtype of the given type 
    | TTyparCoercesToType              of typ * range

    /// Indicates a default value for an inference type variable should it be netiher generalized nor solved 
    | TTyparDefaultsToType             of int * typ * range 
    
    /// Indicates a constraint that a type has a 'null' value 
    | TTyparSupportsNull               of range 
    
    /// Indicates a constraint that a type has a member with the given signature 
    | TTyparMayResolveMemberConstraint of TraitConstraintInfo * range 
    
    /// Indicates a constraint that a type is a non-Nullable value type 
    /// These are part of .NET's model of generic constraints, and in order to 
    /// generate verifiable code we must attach them to F# generalzied type variables as well. 
    | TTyparIsNotNullableValueType     of range 
    
    /// Indicates a constraint that a type is a reference type 
    | TTyparIsReferenceType            of range 

    /// Indicates a constraint that a type is a simple choice between one of the given ground types. See format.ml 
    | TTyparSimpleChoice               of typ list * range 

    /// Indicates a constraint that a type has a parameterless constructor 
    | TTyparRequiresDefaultConstructor of range 

    /// Indicates a constraint that a type is an enum with the given underlying 
    | TTyparIsEnum                     of typ * range 
    
    /// Indicates a constraint that a type is a delegate from the given tuple of args to the given return type *)
    | TTyparIsDelegate                 of typ * typ * range 
    
/// The specification of a member constraint that must be solved 
and TraitConstraintInfo = 
    /// Indicates the signature of a member constraint 
    | TTrait of typ list * string * MemberFlags * typ list * typ option * (* solution: *) TraitConstraintSln option ref 
    member x.MemberName = (let (TTrait(_,nm,_,_,_,_)) = x in nm)
    member x.ReturnType = (let (TTrait(_,_,_,_,ty,_)) = x in ty)
    member x.Solution 
        with get() = (let (TTrait(_,_,_,_,ty,sln)) = x in sln.Value)
        and set(v) = (let (TTrait(_,_,_,_,ty,sln)) = x in sln.Value <- v)
    
and TraitConstraintSln = 
    | FSMethSln of 
         typ * // the type and its instantiation
         ValRef  *   // the method
         tinst // the generic method instantiation 
    | ILMethSln of
         typ * 
         ILTypeRef option (* extension? *) * 
         ILMethodRef * 
         // typars * // the uninstantiated generic method args 
         tinst    // the generic method instantiation 
    | BuiltInSln
   //| DefaultStructCtorSln of typ

and Val = 
    { mutable Data: ValData; }
    /// The internal name the value. 
    member x.MangledName = x.Data.val_name
    /// The place where the value was defined. 
    member x.Range = x.Data.val_range
    /// A unique stamp within the context of this invocation of the compiler process 
    member x.Stamp = x.Data.val_stamp
    /// The type of the value. 
    /// May be a Type_forall for a generic value. 
    /// May be a type variable or type containing type variables during type inference. 

    // Mutability used in inference by adjustAllUsesOfRecValue.  
    // This replaces the recursively inferred type with a schema. 
    // MUTABILITY CLEANUP: find a way to do this using type unification alone. 
    member x.Type                       = x.Data.val_type
    member x.Accessibility              = x.Data.val_access
    /// Range of the definition (implementation) of the value, used by Visual Studio 
    /// Updated by mutation when the implementation is matched against the signature. 
    member x.DefinitionRange            = x.Data.val_defn_range
    /// The value of a value or member marked with [&lt;LiteralAttribute&gt;] 
    member x.LiteralValue               = x.Data.val_const
    member x.Id                         = ident(x.MangledName,x.Range)
    /// Is this represented as a "top level" static binding (i.e. a static field, static member,
    /// instance member), rather than an "inner" binding that may result in a closure.
    ///
    /// This is implied by IsMemberOrModuleBinding, however not vice versa, for two reasons.
    /// Some optimizations mutate this value when they decide to change the representation of a 
    /// binding to be IsCompiledAsTopLevel. Second, even immediately after type checking we expect
    /// some non-module, non-member bindings to be marked IsCompiledAsTopLevel, e.g. 'y' in 
    /// 'let x = let y = 1 in y + y' (NOTE: check this, don't take it as gospel)
    member x.IsCompiledAsTopLevel       = x.Data.val_top_repr_info.IsSome 

    member x.UniqueCompiledName =
#if STANDALONE_METADATA
#else
        // These cases must get stable unique names for their static field & static property. This name
        // must be stable across quotation generation and IL code generation (quotations can refer to the 
        // properties implicit in these)
        //
        //    Variable 'x' here, which is compiled as a top level static:
        //         do let x = expr in ...    // IsMemberOrModuleBinding = false, IsCompiledAsTopLevel = true, IsMember = false, CompilerGenerated=false
        //
        //    The implicit 'patternInput' variable here:
        //         let [x] = expr in ...    // IsMemberOrModuleBinding = true, IsCompiledAsTopLevel = true, IsMember = false, CompilerGenerated=true
        //    
        //    The implicit 'copyOfStruct' variables here:
        //         let dt = System.DateTime.Now - System.DateTime.Now // IsMemberOrModuleBinding = false, IsCompiledAsTopLevel = true, IsMember = false, CompilerGenerated=true
        //    
        // However we don't need this for CompilerGenerated members such as the imlpementations of IComparable
        if x.IsCompiledAsTopLevel  && not x.IsMember  && (x.IsCompilerGenerated || not x.IsMemberOrModuleBinding) then 
            globalStableNameGenerator.GetUniqueCompilerGeneratedName(x.MangledName,x.Range,x.Stamp) 
        else 
#endif
            x.MangledName

    /// What is the public path to the value, if any? Should be set if and only if
    /// IsMemberOrModuleBinding is set.
    //
    // Note: this is a somewhat strange field to be storing since the information
    // is imprecise (it doesn't indicate if the path is made of namespaces or types, hence
    // it's not enough to rebuild a compiled reference to the value. However it is enough to
    // build an F# cross-module reference to the value)
    //
    // Also, this is recoverable from the parent
    //
    // We use it here:
    //   - in opt.ml   : when compiling fslib, we bind an entry for the value in a global table (see bind_escaping_local_vspec)
    //   - in ilxgen.ml: when compiling fslib, we bind an entry for the value in a global table (see bind_escaping_local_vspec)
    //   - in opt.ml   : (full_display_text_of_vref) for error reporting of non-inlinable values
    //   - in service.ml (boutput_item_description): to display the full text of a value's binding location
    //   - in check.ml: as a boolean to detect public values for saving quotations 
    //   - in ilxgen.ml: as a boolean to detect public values for saving quotations 
    //   - in MakeExportRemapping, to build non-local references for values
    member x.PublicPath                 = x.Data.val_pubpath

    /// Is this a member definition or module definition?
    member x.IsMemberOrModuleBinding    = x.Data.val_flags |> ValFlags.is_topbind_of_vflags
    member x.IsExtensionMember          = x.Data.val_flags |> ValFlags.is_extension_member_of_vflags

    member x.ReflectedDefinition        = x.Data.val_defn

    /// Is this a member, if so some more data about the member.
    ///
    /// Note, the value may still be (a) an extension member or (b) and abtract slot without
    /// a true body.
    member x.MemberInfo                 = x.Data.val_member_info

    member x.IsMember                   = x.MemberInfo.IsSome
    member x.IsNonExtensionMember       = x.IsMember && not x.IsExtensionMember
    member x.IsModuleBinding            = x.IsMemberOrModuleBinding && not x.IsMember 
    member x.IsCompiledIntoModule       = x.IsExtensionMember || x.IsModuleBinding

    member x.IsInstanceMember = x.IsMember && x.MemberInfo.Value.MemberFlags.MemberIsInstance

    member x.IsConstructor              =
        match x.MemberInfo with 
        | Some(memberInfo) when not x.IsExtensionMember && (memberInfo.MemberFlags.MemberKind = MemberKindConstructor) -> true
        | _ -> false

    member x.IsOverride                 =
        match x.MemberInfo with 
        | Some(memberInfo) when memberInfo.MemberFlags.MemberIsOverrideOrExplicitImpl -> true
        | _ -> false
            
    member x.IsMutable                  = (match x.Data.val_flags |> ValFlags.mutability_of_vflags with Immutable -> false | Mutable -> true)

    /// Was the value inferred to be a method or function that definitely makes no critical tailcalls?
    member x.MakesNoCriticalTailcalls = x.Data.val_flags |> ValFlags.is_notailcall_hint_of_vflags

    member x.IsIncrClassGeneratedMember     = x.IsCompilerGenerated && x.Data.val_flags |> ValFlags.is_incr_class_of_vflags
    member x.IsIncrClassConstructor = x.IsConstructor && x.Data.val_flags |> ValFlags.is_incr_class_of_vflags
    member x.RecursiveValInfo           = x.Data.val_flags |> ValFlags.vrec_of_vflags
    member x.BaseOrThisInfo             = x.Data.val_flags |> ValFlags.base_of_vflags

    //  Was this value declared to be a type function, e.g. "let f<'a> = typeof<'a>"
    member x.IsTypeFunction             = x.Data.val_flags |> ValFlags.is_tyfunc_of_vflags
    member x.TopValInfo                  = x.Data.val_top_repr_info
    member x.InlineInfo                 = x.Data.val_flags |> ValFlags.inline_info_of_vflags
    member x.MustInline                 = mustinline(x.InlineInfo)
    member x.IsCompilerGenerated        = x.Data.val_flags |> ValFlags.is_compgen_of_vflags
    member x.Attribs                    = x.Data.val_attribs
    member x.XmlDoc                     = x.Data.val_xmldoc
    /// The parent type or module, if any (None for expression bindings and parameters)
    member x.ActualParent               = x.Data.val_actual_parent

    member x.MemberActualParent = 
        match x.ActualParent  with 
        | Parent tcref -> tcref
        | ParentNone -> error(InternalError("MemberActualParent: does not have a parent",x.Range))
            
    member x.MemberApparentParent = 
        match x.MemberInfo with 
        | Some membInfo -> membInfo.ApparentParent
        | None -> error(InternalError("MemberApparentParent",x.Range))

    member x.ApparentParent = 
        match x.MemberInfo with 
        | Some membInfo -> Parent(membInfo.ApparentParent)
        | None -> x.ActualParent

    member x.CoreDisplayName = 
        match x.MemberInfo with 
        | Some membInfo -> 
            match membInfo.MemberFlags.MemberKind with 
            | MemberKindClassConstructor 
            | MemberKindConstructor 
            | MemberKindMember -> membInfo.CompiledName
            | MemberKindPropertyGetSet 
            | MemberKindPropertySet
            | MemberKindPropertyGet -> membInfo.PropertyName
        | None -> x.MangledName 

    member x.DisplayName = 
        DemangleOperatorName x.CoreDisplayName

    member x.TypeScheme = 
        match x.Type with 
        | TType_forall(tps,tau) -> tps,tau
        | ty -> [],ty

    member x.TauType = 
        match x.Type with 
        | TType_forall(_,tau) -> tau
        | ty -> ty

    member x.Typars = 
        match x.Type with 
        | TType_forall(tps,_) -> tps
        | ty -> []

    member x.CompiledName = 
        match x.MemberInfo with 
        | Some membInfo -> membInfo.CompiledName
        | None -> x.MangledName

    // OSGN support
    static member NewUnlinked() : Val  = { Data = nullable_slot_empty() }
    static member New(data) : Val = { Data = data }
    member x.Link(tg) = x.Data <- nullable_slot_full(tg)
    member x.IsLinked = match box x.Data with null -> false | _ -> true 

    override x.ToString() = x.MangledName
    
    
and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  ValData =
    { val_name: string;
      val_range: range;
      mutable val_defn_range: range; 
      mutable val_type: typ;
      val_stamp: stamp; 
      /// See vflags section further below for encoding/decodings here 
      mutable val_flags: int64;
      mutable val_const: Constant option;
      val_pubpath : PublicPath option;

      /// What is the original, unoptimized, closed-term definition, if any? 
      /// Used to implement [<ReflectedDefinition>]
      mutable val_defn: expr option; 

      /// How visible is this? 
      val_access: Accessibility; 

      /// Is the value actually an instance method/property/event that augments 
      /// a type, and if so what name does it take in the IL?
      val_member_info: ValMemberInfo option;

      /// Custom attributes attached to the value. These contain references to other values (i.e. constructors in types). Mutable to fixup  
      /// these value references after copying a colelction of values. 
      mutable val_attribs: Attribs;

      /// Top level values have an arity inferred and/or specified
      /// signatures.  The arity records the number of arguments preferred 
      /// in each position for a curried functions. The currying is based 
      /// on the number of lambdas, and in each position the elements are 
      /// based on attempting to deconstruct the type of the argument as a 
      /// tuple-type.  The field is mutable because arities for recursive 
      /// values are only inferred after the r.h.s. is analyzed, but the 
      /// value itself is created before the r.h.s. is analyzed. 
      ///
      /// TLR also sets this for inner bindings that it wants to 
      /// represent as "top level" bindings.
     
      // MUTABILITY CLEANUP: mutability of this field is used by 
      //     -- adjustAllUsesOfRecValue 
      //     -- TLR optimizations
      //     -- LinearizeTopMatch
      //
      // For example, we use mutability to replace the empty arity initially assumed with an arity garnered from the 
      // type-checked expression.  
      mutable val_top_repr_info: ValTopReprInfo option;


      // MUTABILITY CLEANUP: mutability of this field is used by 
      //     -- LinearizeTopMatch
      //
      // The fresh temporary should just be created with the right parent
      mutable val_actual_parent: ParentRef;

      /// XML documentation attached to a value.
      val_xmldoc : XmlDoc; 
  } 

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  ValMemberInfo = 
    { /// The member name in compiled code
      CompiledName: string;
     
      /// The parent type. For an extension member this is the type being extended 
      ApparentParent: TyconRef;  

      /// Gets updated with full slotsig after interface implementation relation is checked 
      mutable ImplementedSlotSigs: SlotSig list; 

      /// Gets updated with 'true' if an abstract slot is implemented in the file being typechecked.  Internal only. 
      mutable IsImplemented: bool;                      

      MemberFlags: MemberFlags }

    member x.PropertyName = 
        let logicalName = 
            match x.ImplementedSlotSigs with 
            | (TSlotSig(nm,_,_,_,_,_)) :: _ -> nm
            | _ -> x.CompiledName 
        ChopPropertyName logicalName
    member x.LogicalName = 
        match x.ImplementedSlotSigs with 
        | slotsig :: _ -> slotsig.Name
        | _ -> x.CompiledName 



/// Non-local references indirect via a CCU
/// The lookup into the CCU is a NonLocalPath, which is a series of strings
/// We cache the result of dereferencing
and NonLocalItemRef = 
    { /// The path to an item referenced via a CCU
      nlr_nlpath : NonLocalPath; 
      /// The name of an item referenced via a CCU 
      nlr_item: string; }
      
/// A public path records where a construct lives within the global namespace
/// of a CCU.
and PublicPath      = 
    | PubPath of string[] * string

/// The information ILXGEN needs about the location of an item
and CompilationPath = 
    | CompPath of ILScopeRef * (string * ModuleOrNamespaceKind) list
    member x.ILScopeRef = (let (CompPath(scoref,_)) = x in scoref)
    member x.AccessPath = (let (CompPath(_,p)) = x in p)

/// Index into the namespace/module structure of a particular CCU 
and NonLocalPath    = 
    | NLPath of ccu * string[]
        
    member nlpath.TryDeref = 
        let (NLPath(ccu,p)) = nlpath 
        ccu.EnsureDerefable(p)
        let rec loop (entity:Entity)  i = 
            if i >= p.Length then Some entity
            else 
                let next = entity.ModuleOrNamespaceType.AllEntities.TryFind(p.[i])
                match next with 
                | Some res -> loop res (i+1)
                | None -> None

        match loop ccu.Contents 0 with
        | Some res as r -> r
        | None ->
            // OK, the lookup failed. Check if we can redirect through a type forwarder on this assembly.
            // Look for a forwarder for each prefix-path
            let rec tryForwardPrefixPath i = 
                if i < p.Length then 
                    match ccu.TryForward(p.[0..i-1],p.[i]) with
                    | Some tcref -> 
                       // OK, found a forwarder, now continue with the lookup to find the nested type
                       loop tcref.Deref (i+1)
                    | None -> tryForwardPrefixPath (i+1)
                else
                    None
            tryForwardPrefixPath 0
        
    member nlpath.DisplayName =
        let (NLPath(ccu,p)) = nlpath 
        String.concat "." p 

    member nlpath.AssemblyName =
        let (NLPath(ccu,p)) = nlpath 
        ccu.AssemblyName

    member nlpath.Deref = 
        match nlpath.TryDeref with 
        | Some res -> res
        | None -> 
              errorR (InternalUndefinedItemRef ("module/namespace",nlpath.DisplayName, nlpath.AssemblyName, "<some module on this path>")); 
              raise (KeyNotFoundException())
        
    member nlpath.TryModuleOrNamespaceType = 
        nlpath.TryDeref |> Option.map (fun v -> v.ModuleOrNamespaceType) 

    member nlpath.ModuleOrNamespaceType = 
        nlpath.Deref.ModuleOrNamespaceType

        

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  EntityRef = 
    { /// Indicates a reference to something bound in this CCU 
      mutable binding: Entity nonnull_slot
      /// Indicates a reference to something bound in another CCU 
      nlr: NonLocalItemRef }
    member x.IsLocalRef = match box x.nlr with null -> true | _ -> false
    member x.IsResolved = match box x.binding with null -> false | _ -> true
    member x.PrivateTarget = x.binding
    member x.ResolvedTarget = x.binding

    member private tcr.Resolve() = 
        let res = 
            match tcr.nlr.nlr_nlpath.TryModuleOrNamespaceType with
            | Some mtyp -> 
                Map.tryFind tcr.nlr.nlr_item mtyp.AllEntities

            | None -> None
        let res = 
            match res with 
            | Some _ -> res
            | None -> 
                // The lookup failed. See if we can go through a type forwarder
                let (NLPath(ccu,p)) = tcr.nlr.nlr_nlpath 
                ccu.EnsureDerefable(p)
                match ccu.TryForward(p,tcr.nlr.nlr_item) with 
                | Some forwardedTo -> 
                    forwardedTo.TryDeref // recurse
                | None -> 
                    None
        match res with 
        | Some r -> 
             tcr.binding <- nullable_slot_full r; 
        | None -> 
             ()

    // Dereference the TyconRef to a Tycon. Amortize the cost of doing this.
    // This path should not allocate in the amortized case
    member tcr.Deref = 
        match box tcr.binding with 
        | null ->
            tcr.Resolve()
            match box tcr.binding with 
            | null -> raise (InternalUndefinedItemRef ("namespace, module or type",tcr.nlr.nlr_nlpath.DisplayName,tcr.nlr.nlr_nlpath.AssemblyName, tcr.nlr.nlr_item))
            | _ -> tcr.binding
        | _ -> 
            tcr.binding

    // Dereference the TyconRef to a Tycon option.
    member tcr.TryDeref = 
        match box tcr.binding with 
        | null -> 
            tcr.Resolve()
            match box tcr.binding with 
            | null -> None
            | _ -> Some tcr.binding

        | _ -> 
            Some tcr.binding

    override x.ToString() = 
       if x.IsLocalRef then 
           x.ResolvedTarget.DisplayName 
       else 
           x.nlr.nlr_nlpath.DisplayName + "::" + x.nlr.nlr_item


    member x.CompiledRepresentation = x.Deref.CompiledRepresentation
    member x.CompiledRepresentationForTyrepNamed = x.Deref.CompiledRepresentationForTyrepNamed
    member x.MangledName = x.Deref.MangledName
    member x.DisplayName = x.Deref.DisplayName
    member x.DisplayNameWithUnderscoreTypars = x.Deref.DisplayNameWithUnderscoreTypars
    member x.Range = x.Deref.Range
    member x.Stamp = x.Deref.Stamp
    member x.Attribs = x.Deref.Attribs
    member x.XmlDoc = x.Deref.XmlDoc
    member x.ModuleOrNamespaceType = x.Deref.ModuleOrNamespaceType
    
    member x.TypeContents = x.Deref.TypeContents
    member x.TypeOrMeasureKind = x.Deref.TypeOrMeasureKind
    member x.Id = x.Deref.Id
    member x.TypeReprInfo = x.Deref.TypeReprInfo
    member x.ExceptionInfo = x.Deref.ExceptionInfo
    member x.IsExceptionDecl = x.Deref.IsExceptionDecl
    
    member x.DemangledExceptionName = x.Deref.DemangledExceptionName
        
    member x.Typars(m) = x.Deref.Typars(m)
    member x.TyparsNoRange = x.Deref.TyparsNoRange
    member x.TypeAbbrev = x.Deref.TypeAbbrev
    member x.IsTypeAbbrev = x.Deref.IsTypeAbbrev
    member x.TypeReprAccessibility = x.Deref.TypeReprAccessibility
    member x.CompiledReprCache = x.Deref.CompiledReprCache
    member x.PublicPath = x.Deref.PublicPath
    member x.Accessibility = x.Deref.Accessibility
    member x.IsPrefixDisplay = x.Deref.IsPrefixDisplay
    member x.IsModuleOrNamespace  = x.Deref.IsModuleOrNamespace
    member x.IsNamespace          = x.Deref.IsNamespace
    member x.IsModule             = x.Deref.IsModule
    member x.CompilationPathOpt   = x.Deref.CompilationPathOpt
    member x.CompilationPath      = x.Deref.CompilationPath
    member x.AllFieldTable        = x.Deref.AllFieldTable
    member x.AllFieldsArray       = x.Deref.AllFieldsArray
    member x.AllFieldsAsList = x.Deref.AllFieldsAsList
    member x.TrueFieldsAsList = x.Deref.TrueFieldsAsList
    member x.TrueInstanceFieldsAsList = x.Deref.TrueInstanceFieldsAsList
    member x.AllInstanceFieldsAsList = x.Deref.AllInstanceFieldsAsList
    member x.GetFieldByIndex(n)        = x.Deref.GetFieldByIndex(n)
    member x.GetFieldByName(n)         = x.Deref.GetFieldByName(n)
    member x.UnionTypeInfo             = x.Deref.UnionTypeInfo
    member x.UnionCasesArray           = x.Deref.UnionCasesArray
    member x.UnionCasesAsList     = x.Deref.UnionCasesAsList
    member x.GetUnionCaseByName(n)     = x.Deref.GetUnionCaseByName(n)
    member x.FSharpObjectModelTypeInfo = x.Deref.FSharpObjectModelTypeInfo
    member x.IsStructTycon             = x.Deref.IsStructTycon
    member x.IsAsmReprTycon            = x.Deref.IsAsmReprTycon
    member x.IsMeasureableReprTycon    = x.Deref.IsMeasureableReprTycon
    
    
    member x.IsILTycon                = x.Deref.IsILTycon
    member x.ILTyconInfo              = x.Deref.ILTyconInfo
    member x.ILTyconRawMetadata       = x.Deref.ILTyconRawMetadata
    member x.IsUnionTycon             = x.Deref.IsUnionTycon
    member x.UnionInfo                = x.Deref.UnionInfo
    member x.IsRecordTycon            = x.Deref.IsRecordTycon
    member x.IsFSharpObjectModelTycon = x.Deref.IsFSharpObjectModelTycon
    member x.IsHiddenReprTycon        = x.Deref.IsHiddenReprTycon

    member x.IsFSharpInterfaceTycon   = x.Deref.IsFSharpInterfaceTycon
    member x.IsFSharpDelegateTycon    = x.Deref.IsFSharpDelegateTycon
    member x.IsFSharpEnumTycon        = x.Deref.IsFSharpEnumTycon

    member x.IsFSharpStructTycon      = x.Deref.IsFSharpStructTycon

    member x.IsILStructTycon          = x.Deref.IsILStructTycon


/// note: ModuleOrNamespaceRef and TyconRef are type equivalent 
and ModuleOrNamespaceRef       = EntityRef
and TyconRef       = EntityRef

/// References are either local or nonlocal
and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  ValRef = 
    { /// Indicates a reference to something bound in this CCU 
      mutable binding: Val nonnull_slot
      /// Indicates a reference to something bound in another CCU 
      nlr: NonLocalItemRef }
    member x.IsLocalRef = match box x.nlr with null -> true | _ -> false
    member x.IsResolved = match box x.binding with null -> false | _ -> true
    member x.PrivateTarget = x.binding
    member x.ResolvedTarget = x.binding

    member vr.Deref = 
        match box vr.binding with 
        | null ->
            let res = 
                let nlr = vr.nlr 
                let mtyp = nlr.nlr_nlpath.ModuleOrNamespaceType
                try Map.find nlr.nlr_item mtyp.AllValuesAndMembers
                with :? KeyNotFoundException -> raise (InternalUndefinedItemRef ("val",nlr.nlr_nlpath.DisplayName, nlr.nlr_nlpath.AssemblyName, nlr.nlr_item))
            vr.binding <- nullable_slot_full res; 
            res 
        | x -> vr.binding

    member vr.TryDeref = 
        match box vr.binding with 
        | null -> 
            vr.nlr.nlr_nlpath.TryModuleOrNamespaceType |> Option.bind (fun mty -> Map.tryFind vr.nlr.nlr_item mty.AllValuesAndMembers)
        | _ -> Some vr.binding

    member x.Type                       = x.Deref.Type
    member x.TypeScheme                 = x.Deref.TypeScheme
    member x.TauType                    = x.Deref.TauType
    member x.Typars                     = x.Deref.Typars
    member x.MangledName                = x.Deref.MangledName
    member x.DisplayName                = x.Deref.DisplayName
    member x.CompiledName               = x.Deref.CompiledName
    member x.CoreDisplayName            = x.Deref.CoreDisplayName
    member x.Range                      = x.Deref.Range

    member x.Accessibility              = x.Deref.Accessibility
    member x.ActualParent               = x.Deref.ActualParent
    member x.ApparentParent             = x.Deref.ApparentParent
    member x.DefinitionRange            = x.Deref.DefinitionRange
    member x.LiteralValue               = x.Deref.LiteralValue
    member x.Id                         = x.Deref.Id
    member x.Stamp                      = x.Deref.Stamp
    member x.IsCompiledAsTopLevel       = x.Deref.IsCompiledAsTopLevel
    member x.UniqueCompiledName         = x.Deref.UniqueCompiledName

    member x.PublicPath                 = x.Deref.PublicPath
    member x.ReflectedDefinition        = x.Deref.ReflectedDefinition
    member x.IsConstructor              = x.Deref.IsConstructor
    member x.MemberInfo                 = x.Deref.MemberInfo
    member x.IsMember                   = x.Deref.IsMember
    member x.IsModuleBinding            = x.Deref.IsModuleBinding
    member x.IsInstanceMember           = x.Deref.IsInstanceMember

    member x.IsMutable                  = x.Deref.IsMutable
    member x.MakesNoCriticalTailcalls  = x.Deref.MakesNoCriticalTailcalls
    member x.IsMemberOrModuleBinding    = x.Deref.IsMemberOrModuleBinding
    member x.IsExtensionMember          = x.Deref.IsExtensionMember
    member x.IsIncrClassConstructor = x.Deref.IsIncrClassConstructor
    member x.IsIncrClassGeneratedMember = x.Deref.IsIncrClassGeneratedMember
    member x.RecursiveValInfo           = x.Deref.RecursiveValInfo
    member x.BaseOrThisInfo             = x.Deref.BaseOrThisInfo
    member x.IsTypeFunction             = x.Deref.IsTypeFunction
    member x.TopValInfo                  = x.Deref.TopValInfo
    member x.InlineInfo                 = x.Deref.InlineInfo
    member x.MustInline                 = x.Deref.MustInline
    member x.IsCompilerGenerated        = x.Deref.IsCompilerGenerated
    member x.Attribs                    = x.Deref.Attribs
    member x.XmlDoc                     = x.Deref.XmlDoc
    member x.MemberActualParent         = x.Deref.MemberActualParent
    member x.MemberApparentParent       = x.Deref.MemberApparentParent
    override x.ToString() = 
       if x.IsLocalRef then x.ResolvedTarget.DisplayName else x.nlr.nlr_nlpath.DisplayName + "::" + x.nlr.nlr_item

and UnionCaseRef = UCRef of TyconRef * string
and RecdFieldRef = RFRef of TyconRef * string

and 
  /// The algebra of types
  [<StructuralEquality(false); StructuralComparison(false)>]
// REMOVING because of possible stack overflow  [<System.Diagnostics.DebuggerTypeProxy(typedefof<Dumper>)>]
  typ =
    /// Indicates the type is a universal type, only used for types of values, members and record fields 
    | TType_forall of typars * typ
    /// Indicates the type is a type application 
    | TType_app of TyconRef * tinst
    /// Indicates the type is a tuple type 
    | TType_tuple of typ list
    /// Indicates the type is a function type 
    | TType_fun of  typ * typ
    /// Indicates the type is a non-F#-visible type representing a "proof" that a union value belongs to a particular union case
    /// These types are not user-visible and will never appear as an inferred type. They are the types given to
    /// the temporaries arising out of pattern matching on union values.
    | TType_ucase of  UnionCaseRef * tinst
    /// Indicates the type is a variable type, whether declared, generalized or an inference type parameter  
    | TType_var of Typar 
    /// A legacy fake type used in legacy code to indicate the "type" of a module when building a module expression 
    | TType_modul_bindings
    | TType_measure of measure

and tinst = typ list 

and measure = 
    | MeasureVar of Typar
    | MeasureCon of TyconRef
    | MeasureProd of measure*measure
    | MeasureInv of measure
    | MeasureOne

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  CcuData = 
    { /// Holds the filename for the DLL, if any 
      ccu_filename: string option; 
      
      /// Holds the data indicating how this assembly/module is referenced from the code being compiled. 
      ccu_scoref: ILScopeRef;
      
      /// A unique stamp for this DLL 
      ccu_stamp: stamp;
      
      /// The fully qualified assembly reference string to refer to this assembly. This is persisted in quotations 
      ccu_qname: string option; 
      
      /// A hint as to where does the code for the CCU live (e.g what was the tcConfig.implicitIncludeDir at compilation time for this DLL?) 
      ccu_code_dir: string; 
      
      /// Indicates that this DLL was compiled using the F# compiler 
      ccu_fsharp: bool; 
      
      /// Indicates that this DLL uses quotation literals somewhere. This is used to implement a restriction on static linking
      mutable ccu_usesQuotations : bool;
      
      /// A handle to the full specification of the contents of the module contained in this ccu *)
      (* NOTE: may contain transient state during typechecking *)
      mutable ccu_contents: ModuleOrNamespace;
      
      ccu_forwarders : CcuTypeForwarderTable }

and CcuTypeForwarderTable = Lazy<Map<string[] * string, EntityRef>>

and CcuReference =  string // ILAssemblyRef

and ccu = CcuThunk

// Compilation units and Cross-compilation-unit thunks.
//
// A compilation unit is, more or less, the new material created in one
// invocation of the compiler.  Due to static linking assemblies may hold more 
// than one compilation unit (i.e. when two assemblies are merged into a compilation
// the resulting assembly will contain 3 CUs).  Compilation units are also created for referenced
// .NET assemblies. 
// 
// References to items such as type constructors are via 
// cross-compilation-unit thunks, which directly reference the data structures that define
// these modules.  Thus, when saving out values to disk we only wish 
// to save out the "current" part of the term graph.  When reading values
// back in we "fixup" the links to previously referenced modules.
//
// All non-local accesses to the data structures are mediated
// by ccu-thunks.  Ultimately, a ccu-thunk is either a (named) element of
// the data structure, or it is a delayed fixup, i.e. an invalid dangling
// reference that has not had an appropriate fixup applied.  

/// A relinkable handle to the contents of a compilation unit. Relinking is performed by mutation.
and CcuThunk = 
    { mutable target: CcuData;
      mutable orphanfixup : bool;
      name: CcuReference  }
      
    member ccu.Deref = 
        if (ccu.target = Unchecked.defaultof<CcuData>) || ccu.orphanfixup then 
            raise(UnresolvedReferenceNoRange ccu.name)
        ccu.target
   
    member ccu.IsUnresolvedReference = (ccu.target = Unchecked.defaultof<CcuData> || ccu.orphanfixup)

    /// Ensure the ccu is derefable in advance. Supply a path to attach to any resulting error message.
    member ccu.EnsureDerefable(requiringPath:string[]) = 
        // ccu.orphanfixup is true when a reference is missing in the transitive closure of static references that
        // may potentially be required for the metadata of referenced DLLs. It is set to true if the "loader"
        // used in the F# metadata-deserializer or the .NET metadata reader returns a failing value (e.g. None).
        // Note: When used from Visual Studio, the loader will not automatically chase down transitively referenced DLLs - they
        // must be in the explicit references in the project.
        if ccu.IsUnresolvedReference then 
            let path = System.String.Join(".", requiringPath)
            raise(UnresolvedPathReferenceNoRange(ccu.name,path))
            
    member ccu.UsesQuotations with get() = ccu.Deref.ccu_usesQuotations and set(v) = ccu.Deref.ccu_usesQuotations <- v
    member ccu.AssemblyName = ccu.name
    member ccu.ILScopeRef = ccu.Deref.ccu_scoref
    member ccu.Stamp = ccu.Deref.ccu_stamp
    member ccu.FileName = ccu.Deref.ccu_filename
    member ccu.QualifiedName = ccu.Deref.ccu_qname
    member ccu.SourceCodeDirectory = ccu.Deref.ccu_code_dir
    member ccu.IsFSharp = ccu.Deref.ccu_fsharp
    member ccu.Contents = ccu.Deref.ccu_contents
    member ccu.TypeForwarders : Map<string[] * string, EntityRef>  = ccu.Deref.ccu_forwarders.Force()

    static member Create(nm,x) = 
        { target = x; 
          orphanfixup = false;
          name = nm;  }

    static member CreateDelayed(nm) = 
        { target = Unchecked.defaultof<_>; 
          orphanfixup = false;
          name = nm;  }

    member x.Fixup(avail:CcuThunk) = 
        match box x.target with
        | null -> 
            assert (avail.AssemblyName = x.AssemblyName)
            x.target <- 
               (match box avail.target with
                | null -> error(Failure("internal error: ccu thunk '"^avail.name^"' not fixed up!"))
                | _ -> avail.target)
        | _ -> errorR(Failure("internal error: the ccu thunk for assembly "^x.AssemblyName^" not delayed!"));
        
    member x.FixupOrphaned() = 
        match box x.target with
        | null -> x.orphanfixup<-true
        | _ -> errorR(Failure("internal error: the ccu thunk for assembly "^x.AssemblyName^" not delayed!"));
            
    member ccu.TryForward(nlpath:string[],item:string) : EntityRef option  = 
        ccu.EnsureDerefable(nlpath)
        ccu.TypeForwarders.TryFind(nlpath,item) 
        //printfn "trying to forward %A::%s from ccu '%s', res = '%A'" p n ccu.AssemblyName res.IsSome

    override ccu.ToString() = ccu.AssemblyName

/// The result of attempting to resolve an assembly name to a full ccu.
/// UnresolvedCcu will contain the name of the assembly that could not be resolved.
and CcuResolutionResult =
    | ResolvedCcu of ccu
    | UnresolvedCcu of string

and PickledModuleInfo =
  { mspec: ModuleOrNamespace;
    compile_time_working_dir: string;
    usesQuotations : bool }

//---------------------------------------------------------------------------
// Attributes
//---------------------------------------------------------------------------

and Attribs = Attrib list 

and AttribKind = 
  /// Indicates an attribute refers to a type defined in an imported .NET assembly *)
  | ILAttrib of ILMethodRef 
  /// Indicates an attribute refers to a type defined in an imported F# assembly *)
  | FSAttrib of ValRef

/// Attrib(kind,unnamedArgs,propVals)
and Attrib = 
  | Attrib of TyconRef * AttribKind * AttribExpr list * AttribNamedArg list * range

/// We keep both source expression and evaluated expression around to help intellisense and signature printing
and AttribExpr = AttribExpr of (* source *) expr * (* evaluated *) expr 

/// AttribNamedArg(name,type,isField,value)
and AttribNamedArg = AttribNamedArg of (string*typ*bool*AttribExpr)

/// Constants in expressions
and Constant = 
  | TConst_bool       of bool
  | TConst_sbyte       of sbyte
  | TConst_byte      of byte
  | TConst_int16      of int16
  | TConst_uint16     of uint16
  | TConst_int32      of int32
  | TConst_uint32     of uint32
  | TConst_int64      of int64
  | TConst_uint64     of uint64
  | TConst_nativeint  of int64
  | TConst_unativeint of uint64
  | TConst_float32    of single
  | TConst_float      of double
  | TConst_char       of char
  | TConst_string     of string (* in unicode *)
  | TConst_decimal    of System.Decimal (* in unicode *)
  | TConst_unit
  | TConst_zero (* null/zero-bit-pattern *)
  

/// Decision trees. Pattern matching has been compiled down to
/// a decision tree by this point.  The right-hand-sides (actions) of
/// the decision tree are labelled by integers that are unique for that
/// particular tree.
and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  DecisionTree = 

    /// Indicates a decision point in a decision tree. 
    | TDSwitch  of 
          (* input: *) expr * 
          (* cases: *) DecisionTreeCase list * 
          (* default: *) DecisionTree option * range

    /// Indicates the decision tree has terminated with success, calling the given target with the given parameters 
    | TDSuccess of 
          (* results: *) FlatExprs * 
          (* target: *) int  

    /// Bind the given value throught the remaining cases of the dtree. 
    | TDBind of 
          (* binding: *) Binding * 
          (* body: *) DecisionTree

and DecisionTreeCase = 
    | TCase of DecisionTreeDiscriminator * DecisionTree

and 
  [<StructuralEquality(false); StructuralComparison(false)>]
  DecisionTreeDiscriminator = 
    /// Test if the input to a decision tree matches the given constructor 
    | TTest_unionconstr of (UnionCaseRef * tinst) 

    /// Test if the input to a decision tree is an array of the given length 
    | TTest_array_length of int * typ  

    /// Test if the input to a decision tree is the given constant value 
    | TTest_const of Constant

    /// Test if the input to a decision tree is null 
    | TTest_isnull 

    /// Test if the input to a decision tree is an instance of the given type 
    | TTest_isinst of (* source: *) typ * (* target: *) typ

    /// Run the active pattern and bind a successful result to the (one) variable in the remaining tree 
    | TTest_query of expr * typ list * ValRef option * int * ActivePatternInfo


/// A target of a decision tree. Can be thought of as a little function, though is compiled as a local block. 
and DecisionTreeTarget = 
    | TTarget of FlatVals * expr * SequencePointInfoForTarget

and Bindings = FlatList<Binding>

and Binding = 
    | TBind of Val * expr * SequencePointInfoForBinding
    member x.Var               = (let (TBind(v,_,_)) = x in v)
    member x.Expr              = (let (TBind(_,e,_)) = x in e)
    member x.SequencePointInfo = (let (TBind(_,_,sp)) = x in sp)
    
// ActivePatternElemRef: active pattern element (deconstruction case), e.g. 'JNil' or 'JCons'. 
// Integer indicates which choice in the target set is being selected by this item. 
and ActivePatternElemRef = 
    | APElemRef of ActivePatternInfo * ValRef * int 

    member x.IsTotalActivePattern = (let (APElemRef(total,vref,n)) = x in total)
    member x.ActivePatternVal = (let (APElemRef(total,vref,n)) = x in vref)
    member x.CaseIndex = (let (APElemRef(total,vref,n)) = x in n)

and ActivePatternInfo = 
    | APInfo of bool * string list * range

and ValTopReprInfo  = 
    | TopValInfo  of (* numTypars: *) TopTyparInfo list * (* args: *) TopArgInfo list list * (* result: *) TopArgInfo 
    member x.ArgInfos       = (let (TopValInfo(_,args,_)) = x in args)
    member x.NumCurriedArgs = (let (TopValInfo(_,args,_)) = x in args.Length)
    member x.NumTypars      = (let (TopValInfo(n,_,_)) = x in n.Length)
    member x.HasNoArgs      = (let (TopValInfo(n,args,_)) = x in n.IsEmpty && args.IsEmpty)
    member x.AritiesOfArgs  = (let (TopValInfo(_,args,_)) = x in List.map List.length args)
    member x.KindsOfTypars  = (let (TopValInfo(n,_,_)) = x in n |> List.map (fun (TopTyparInfo(_,k)) -> k))

/// The extra metadata stored about typars for top-level definitions. Any information here is propagated from signature through
/// to the compiled code.
and TopArgInfo = TopArgInfo of (* attributes: *) Attribs * (* name: *) ident option 

/// The extra metadata stored about typars for top-level definitions. Any information here is propagated from signature through
/// to the compiled code.
and TopTyparInfo = TopTyparInfo of ident * TyparKind

and typars = Typar list
 
and Exprs = expr list
and FlatExprs = FlatList<expr>
and Vals = Val list
and FlatVals = FlatList<Val>

/// The big type of expressions.  
and expr =
    /// A constant expression. 
    | TExpr_const of Constant * range * typ

    /// Reference a value. The flag is only relevant if the value is an object model member 
    /// and indicates base calls and special uses of object constructors. 
    | TExpr_val of ValRef * ValUseFlag * range

    /// Sequence expressions, used for "a;b", "let a = e in b;a" and "a then b" (the last an OO constructor). 
    | TExpr_seq of expr * expr * SequentialOpKind * SequencePointInfoForSeq * range

    /// Lambda expressions. 
    
    // Why multiple vspecs? A TExpr_lambda taking multiple arguments really accepts a tuple. 
    // But it is in a convenient form to be compile accepting multiple 
    // arguments, e.g. if compiled as a toplevel static method. 

    // REVIEW: see if we can eliminate this and just use lambdas taking single arguments. 
    // though perhaps propagating metadata about preferred argument names. 

    // REVIEW: it would probably be better if the freevar cache cached those of the body rather than the 
    // whole expression.  

    // REVIEW: why not conjoin multiple lambdas into a single iterated lambda node? 
    | TExpr_lambda of uniq * Val option * Val list * expr * range * typ * SkipFreeVarsCache

    // Type lambdas.  These are used for the r.h.s. of polymorphic 'let' bindings and 
    // for expressions that implement first-class polymorphic values. 
    // REVIEW: it would probably be better if the freevar cache cached those of the body rather than the 
    // whole expression.  
    | TExpr_tlambda of uniq * typars * expr * range * typ  * SkipFreeVarsCache

    /// Applications.
    /// Applications combine type and term applications, and are normalized so 
    /// that sequential applications are combined, so "(f x y)" becomes "f [[x];[y]]". 
    /// The type attached to the function is the formal function type, used to ensure we don't build application 
    /// nodes that over-apply when instantiating at function types. 
    | TExpr_app of expr * typ * tinst * Exprs * range

    /// Bind a recursive set of values. 

    // REVIEW: it would probably be better if the freevar cache cached those of the body rather than the 
    // whole expression.  
    | TExpr_letrec of Bindings * expr * range * FreeVarsCache

    /// Bind a value. 
    
    // REVIEW: do we really need both TExpr_let AND TExpr_letrec AND TExpr_match AND TExpr_lambda!? 
    // Why not just TExpr_match the primitive? 
    
    // REVIEW: it would probably be better if the freevar cache cached those of the body rather than the 
    // whole expression.  
    | TExpr_let of Binding * expr * range * FreeVarsCache

    // Object expressions: A closure that implements an interface or a base type. 
    // The base object type might be a delegate type. 
    | TExpr_obj of 
         (* unique *)           uniq * 
         (* object typ *)       typ *                                         (* <-- NOTE: specifies type parameters for base type *)
         (* base val *)         Val option * 
         (* ctor call *)        expr * 
         (* overrides *)        ObjExprMethod list * 
         (* extra interfaces *) (typ * ObjExprMethod list) list *                   
                                range * 
                                SkipFreeVarsCache

    // Pattern matching. 

    /// Matches are a more complicated form of "let" with multiple possible destinations 
    /// and possibly multiple ways to get to each destination.  
    /// The first mark is that of the expression being matched, which is used 
    /// as the mark for all the decision making and binding that happens during the match. 
    | TExpr_match of SequencePointInfoForBinding * range * DecisionTree * DecisionTreeTarget array * range * typ * SkipFreeVarsCache

    /// If we statically know some infomation then in many cases we can use a more optimized expression 
    /// This is primarily used by terms in the standard library, particularly those implementing overloaded 
    /// operators. 
    | TExpr_static_optimization of StaticOptimization list * expr * expr * range

    /// An intrinsic applied to some (strictly evaluated) arguments 
    /// A few of intrinsics (TOp_try, TOp_while, TOp_for) expect arguments kept in a normal form involving lambdas 
    | TExpr_op of ExprOpSpec * tinst * Exprs * range

    // Indicates the expression is a quoted expression tree. 
    | TExpr_quote of expr * (typ list * Exprs * ExprData) option ref * range * typ  
    
    /// Typechecking residue: Indicates a free choice of typars that arises due to 
    /// minimization of polymorphism at let-rec bindings.  These are 
    /// resolved to a concrete instantiation on subsequent rewrites. 
    | TExpr_tchoose of typars * expr * range

    /// Typechecking residue: A TExpr_link occurs for every use of a recursively bound variable. While type-checking 
    /// the recursive bindings a dummy expression is stored in the mutable reference cell. 
    /// After type checking the bindings this is replaced by a use of the variable, perhaps at an 
    /// appropriate type instantiation. These are immediately eliminated on subsequent rewrites. 
    | TExpr_link of expr ref

/// A type for a module-or-namespace-fragment and the actual definition of the module-or-namespace-fragment
and ModuleOrNamespaceExprWithSig = 
    | TMTyped of 
         /// The module_typ is a binder. However it is not used in the ModuleOrNamespaceExpr: it is only referenced from the 'outside' 
         ModuleOrNamespaceType 
         * ModuleOrNamespaceExpr
         * range

/// The contents of a module-or-namespace-fragment definition 
and ModuleOrNamespaceExpr = 
    /// Indicates the module is a module with a signature 
    | TMAbstract of ModuleOrNamespaceExprWithSig
    /// Indicates the module fragment is made of several module fragments in succession 
    | TMDefs     of ModuleOrNamespaceExpr list  
    /// Indicates the module fragment is a 'let' definition 
    | TMDefLet   of Binding * range
    /// Indicates the module fragment is an evaluation of expression for side-effects
    | TMDefDo   of expr * range
    /// Indicates the module fragment is a 'rec' definition of types, values and modules
    | TMDefRec   of Tycon list * Bindings * ModuleOrNamespaceBinding list * range

/// A named module-or-namespace-fragment definition 
and ModuleOrNamespaceBinding = 
    | TMBind of 
         /// This ModuleOrNamespace that represents the compilation of a module as a class. 
         /// The same set of tycons etc. are bound in the ModuleOrNamespace as in the ModuleOrNamespaceExpr
         ModuleOrNamespace * 
         /// This is the body of the module/namespace 
         ModuleOrNamespaceExpr


#if STANDALONE_METADATA
#else
and TypedImplFile = TImplFile of QualifiedNameOfFile * ScopedPragma list * ModuleOrNamespaceExprWithSig

and TypedAssembly = TAssembly of TypedImplFile list

#endif
and RecordConstructionInfo = 
   /// We're in a constructor. The purpose of the record expression is to 
   /// fill in the fields of a pre-created but uninitialized object 
   | RecdExprIsObjInit
   /// Normal record construction 
   | RecdExpr
   
and 
    [<StructuralEquality(false); StructuralComparison(false)>]
    ExprOpSpec =
    /// An operation representing the creation of a union value of the particular union case
    | TOp_ucase of UnionCaseRef 
    /// An operation representing the creation of an exception value using an F# exception declaration
    | TOp_exnconstr of TyconRef
    /// An operation representing the creation of a tuple value
    | TOp_tuple 
    /// An operation representing the creation of an array value
    | TOp_array
    /// Constant bytes, but a new mutable blob is generated each time the construct is executed 
    | TOp_bytes of byte[] 
    | TOp_uint16s of uint16[] 
    // REVIEW: simplify these two to a more general concretization of inner letrec bindings
    /// An operation representing a lambda-encoded while loop
    | TOp_while of SequencePointInfoForWhileLoop
    /// An operation representing a lambda-encoded for loop
    | TOp_for of SequencePointInfoForForLoop * ForLoopStyle (* count up or down? *)
    /// An operation representing a lambda-encoded try/catch
    | TOp_try_catch of SequencePointInfoForTry * SequencePointInfoForWith
    /// An operation representing a lambda-encoded try/finally
    | TOp_try_finally of SequencePointInfoForTry * SequencePointInfoForFinally

    /// Construct a record or object-model value. The ValRef is for self-referential class constructors, otherwise 
    /// it indicates that we're in a constructor and the purpose of the expression is to 
    /// fill in the fields of a pre-created but uninitialized object, and to assign the initialized 
    /// version of the object into the optional mutable cell pointed to be the given value. 
    | TOp_recd of RecordConstructionInfo * TyconRef
    
    /// An operation representing setting a record field
    | TOp_rfield_set of RecdFieldRef 
    /// An operation representing getting a record field
    | TOp_rfield_get of RecdFieldRef 
    /// An operation representing getting the address of a record field
    | TOp_field_get_addr of RecdFieldRef       
    /// An operation representing getting an integer tag for a union value representing the union case number
    | TOp_ucase_tag_get of TyconRef 
    /// An operation representing a coercion that proves a union value is of a particular union case. THis is not a test, its
    /// simply added proof to enable us to generate verifiable code for field access on union types
    | TOp_ucase_proof of UnionCaseRef
    /// An operation representing a field-get from a union value, where that value has been proven to be of the corresponding union case.
    | TOp_ucase_field_get of UnionCaseRef * int 
    /// An operation representing a field-get from a union value. THe value is not assumed to have been proven to be of the corresponding union case.
    | TOp_ucase_field_set of  UnionCaseRef * int
    /// An operation representing a field-get from an F# exception value.
    | TOp_exnconstr_field_get of TyconRef * int 
    /// An operation representing a field-set on an F# exception value.
    | TOp_exnconstr_field_set of TyconRef * int 
    /// An operation representing a field-get from an F# tuple value.
    | TOp_tuple_field_get of int 
    /// IL assembly code - type list are the types pushed on the stack 
    | TOp_asm of ILInstr list * typ list 
    /// generate a ldflda on an 'a ref. REVIEW: generalize to a TOp_flda 
    | TOp_get_ref_lval 
    /// Conversion node, compiled via type-directed translation or to box/unbox 
    | TOp_coerce 
    /// Represents a "rethrow" operation. May not be rebound, or used outside of try-finally, expecting a unit argument 
    | TOp_rethrow 
    | TOp_return
#if STANDALONE_METADATA
#else
    | TOp_goto of ILCodeLabel
    | TOp_label of ILCodeLabel
#endif

    /// Pseudo method calls. This is used for overloaded operations like op_Addition. 
    | TOp_trait_call of TraitConstraintInfo  

    /// Operation nodes represnting C-style operations on byrefs and mutable vals (l-values) 
    | TOp_lval_op of LValueOperation * ValRef 

    /// IL method calls 
    | TOp_ilcall of 
        (bool * (* virtual call? *)
         bool * (* protected? *)
         bool * (* is the object a value type? *) 
         bool * (* newobj call? *) 
         ValUseFlag * (* isSuperInit call? *) 
         bool * (* property? used for reflection *)
         bool * (* DllImport? if so don't tailcall *)
         (typ * typ) option * (* coercion to box 'this' *)  
         ILMethodRef) * 
       typ list * (* tinst *) 
       typ list * (* minst *) 
       typ list   (* types of pushed values if any *) 

and ForLoopStyle = 
    /// Evaluate start and end once, loop up
    | FSharpForLoopUp 
    /// Evaluate start and end once, loop down
    | FSharpForLoopDown 
    /// Evaluate start once and end multiple times, loop up
    | CSharpForLoopUp

and LValueOperation = 
    /// In C syntax this is: &localv            
    | LGetAddr      
    /// In C syntax this is: *localv_ptr        
    | LByrefGet     
    /// In C syntax this is:  localv = e     , note == *(&localv) = e == LGetAddr; LByrefSet
    | LSet          
    /// In C syntax this is: *localv_ptr = e   
    | LByrefSet     

and SequentialOpKind = 
    /// a ; b 
    | NormalSeq 
    /// let res = a in b;res 
    | ThenDoSeq     

and ValUseFlag =
    | NormalValUse
    /// A call to a constructor, e.g. 'inherit C()'
    | CtorValUsedAsSuperInit
    /// A call to a constructor, e.g. 'new C() = new C(3)'
    | CtorValUsedAsSelfInit
    /// A call to a base method, e.g. 'base.OnPaint(args)'
    | VSlotDirectCall
  
and StaticOptimization = 
    | TTyconEqualsTycon of typ * typ
  
/// A representation of a method in an object expression. 
/// Note: Methods associated with types are represented as val declarations
/// Note: We should probably use val_specs for object expressions, as then the treatment of members 
/// in object expressions could be more unified with the treatment of members in types 
and ObjExprMethod = 
    | TObjExprMethod of SlotSig * typars * Val list list * expr * range
    member x.Id = let (TObjExprMethod(slotsig,methFormalTypars,_,_,m)) = x in mksyn_id m slotsig.Name

and SlotSig = 
    | TSlotSig of string * typ * typars * typars * SlotParam list list * typ option
    member ss.Name             = let (TSlotSig(nm,_,_,_,_,_)) = ss in nm
    member ss.ImplementedType  = let (TSlotSig(_,ty,_,_,_,_)) = ss in ty
    member ss.ClassTypars      = let (TSlotSig(_,_,ctps,_,_,_)) = ss in ctps
    member ss.MethodTypars     = let (TSlotSig(_,_,_,mtps,_,_)) = ss in mtps
    member ss.FormalParams     = let (TSlotSig(_,_,_,_,ps,_)) = ss in ps
    member ss.FormalReturnType = let (TSlotSig(_,_,_,_,_,rt)) = ss in rt

and SlotParam = 
    | TSlotParam of  string option * typ * bool (* in *) * bool (* out *) * bool (* optional *) * Attribs
    member x.Type = let (TSlotParam(_,ty,_,_,_,_)) = x in ty

//---------------------------------------------------------------------------
// Freevars.  Computed and cached by later phases (never computed type checking).  Cached in terms. Not pickled.
//---------------------------------------------------------------------------

#if STANDALONE_METADATA
#else
and FreeLocals = Val Zset.t
and FreeTypars = Typar Zset.t
and FreeTycons = Tycon Zset.t
and FreeRecdFields = RecdFieldRef Zset.t
and FreeUnionCases = UnionCaseRef Zset.t
and FreeTyvars = 
    { /// The summary of locally defined type definitions used in the expression. These may be made private by a signature 
      /// and we have to check various conditions associated with that. 
      FreeTycons: FreeTycons;

      /// The summary of values used as trait solutions
      FreeTraitSolutions: FreeLocals;
      
      /// The summary of type parameters used in the expression. These may not escape the enclosing generic construct 
      /// and we have to check various conditions associated with that. 
      FreeTypars: FreeTypars }


and SkipFreeVarsCache = unit
and FreeVarsCache = FreeVars cache

and FreeVars = 
    { /// The summary of locally defined variables used in the expression. These may be hidden at let bindings etc. 
      /// or made private by a signature or marked 'internal' or 'private', and we have to check various conditions associated with that. 
      FreeLocals: FreeLocals;
      
      /// Indicates if the expression contains a call to a protected member or a base call. 
      /// Calls to protected members and direct calls to super classes can't escape, also code can't be inlined 
      UsesMethodLocalConstructs: bool; 

      /// Indicates if the expression contains a call to rethrow that is not bound under a (try-)with branch. 
      /// Rethrow may only occur in such locations. 
      UsesUnboundRethrow: bool; 

      /// The summary of locally defined tycon representations used in the expression. These may be made private by a signature 
      /// or marked 'internal' or 'private' and we have to check various conditions associated with that. 
      FreeLocalTyconReprs: FreeTycons; 

      /// The summary of fields used in the expression. These may be made private by a signature 
      /// or marked 'internal' or 'private' and we have to check various conditions associated with that. 
      FreeRecdFields: FreeRecdFields;
      
      /// The summary of union constructors used in the expression. These may be
      /// marked 'internal' or 'private' and we have to check various conditions associated with that.
      FreeUnionCases: FreeUnionCases;
      
      /// See FreeTyvars above.
      FreeTyvars: FreeTyvars }
#endif

/// Specifies the compiled representations of type and exception definitions.  
/// Computed and cached by later phases (never computed type checking).  Cached at 
/// type and exception definitions. Not pickled.
and CompiledTypeRepr = 
    | TyrepNamed of ILTypeRef * ILBoxity
    | TyrepOpen of ILType  

//---------------------------------------------------------------------------
// Basic properties on type definitions
//---------------------------------------------------------------------------

let demangled_name_of_entity_name nm k = 
    match k with 
    | FSharpModuleWithSuffix -> String.dropSuffix nm FSharpModuleSuffix
    | _ -> nm

let demangled_name_of_modul (x:ModuleOrNamespace) = 
    demangled_name_of_entity_name x.MangledName x.ModuleOrNamespaceType.ModuleOrNamespaceKind

/// Metadata on values (names of arguments etc. 
module TopValInfo = 
    let unnamedTopArg1 = TopArgInfo([],None)
    let unnamedTopArg = [unnamedTopArg1]
    let unitArgData = [[]]
    let unnamedRetVal = TopArgInfo([],None)
    let selfMetadata = unnamedTopArg
    let emptyValData = TopValInfo([],[],unnamedRetVal)

    let InferTyparInfo (tps:Typar list) = tps |> List.map (fun tp -> TopTyparInfo(tp.Id, tp.Kind))
    let InferTopArgInfo (v:Val) = TopArgInfo ([], Some v.Id)
    let InferTopArgInfos (vs:Val list list) = TopValInfo([],List.mapSquared InferTopArgInfo vs,unnamedRetVal)
    let HasNoArgs (TopValInfo(n,args,_)) = n.IsEmpty && args.IsEmpty

//---------------------------------------------------------------------------
// Basic properties via functions (old style)
//---------------------------------------------------------------------------

let id_of_tycon             (tc:Tycon) = ident(tc.MangledName, tc.Range)
let stamp_of_tycon          (tc:Tycon) = tc.Stamp
let attribs_of_tycon        (tc:Tycon) = tc.Attribs
let pubpath_of_tycon        (tc:Tycon) = tc.PublicPath

let data_of_val          (v:Val) = v.Data 
let type_of_val          (v:Val) = v.Type
let types_of_vals        (v:Val list) = v |> List.map (fun v -> v.Type)
let name_of_val          (v:Val) = v.MangledName
let id_of_val            (v:Val) = ident(v.MangledName,v.Range)
let pubpath_of_val       (v:Val) = v.PublicPath
  
let arity_of_val (v:Val) = (match v.TopValInfo with None -> TopValInfo.emptyValData | Some arities -> arities)

let set_vrec_of_vflags       x b = x.val_flags <- ValFlags.encode_vrec_of_vflags       b x.val_flags
let set_is_topbind_of_vflags x b = x.val_flags <- ValFlags.encode_is_topbind_of_vflags b x.val_flags
let set_notailcall_hint_of_vflags x b = x.val_flags <- ValFlags.encode_notailcall_hint_of_vflags b x.val_flags

//-------------------------------------------------------------------------
// Managed cached type name lookup tables
//------------------------------------------------------------------------- 
 
let AddTyconsByDemangledNameAndArity nm (typars:Typar list) x tab = 
    let nm = DemangleGenericTypeName nm 
    Map.add (NameArityPair(nm, typars.Length)) x tab

let AddTyconsByAccessNames nm x tab = 
    if IsMangledGenericName nm then 
        let dnm = DemangleGenericTypeName nm 
        let res = NameMultiMap.add nm x tab 
        NameMultiMap.add dnm x res
    else
        NameMultiMap.add nm x tab 
       
type ModuleOrNamespaceType with  
    member mtyp.TypeDefinitions               = mtyp.AllEntities |> NameMap.range |> List.filter (fun x -> not x.IsExceptionDecl && not x.IsModuleOrNamespace)
    member mtyp.ExceptionDefinitions          = mtyp.AllEntities |> NameMap.range |> List.filter (fun x -> x.IsExceptionDecl)
    member mtyp.ModuleAndNamespaceDefinitions = mtyp.AllEntities |> NameMap.range |> List.filter (fun x -> x.IsModuleOrNamespace)
    member mtyp.TypeAndExceptionDefinitions   = mtyp.AllEntities |> NameMap.range |> List.filter (fun x -> not x.IsModuleOrNamespace)

    member mtyp.TypesByDemangledNameAndArity(m) = 
      cacheOptRef mtyp.TypesByDemangledNameAndArityLookupTable (fun () -> 
         List.foldBack (fun (tc:Tycon) acc -> AddTyconsByDemangledNameAndArity tc.MangledName (tc.Typars(m)) tc acc) mtyp.TypeAndExceptionDefinitions  Map.empty)

    member mtyp.TypesByAccessNames = 
        cacheOptRef mtyp.TypesByAccessNamesLookupTable (fun () -> 
           List.foldBack (fun (tc:Tycon) acc -> AddTyconsByAccessNames tc.MangledName tc acc) mtyp.TypeAndExceptionDefinitions  Map.empty)

    member mtyp.TypesByMangledName = 
        let add_tyconsByMangledName (x:Tycon) tab = NameMap.add x.MangledName x tab 
        cacheOptRef mtyp.TypesByMangledNameLookupTable (fun () -> 
           List.foldBack add_tyconsByMangledName mtyp.TypeAndExceptionDefinitions  Map.empty)

    member mtyp.ExceptionDefinitionsByDemangledName = 
        let add_exconsByDemangledName (tycon:Tycon) acc = NameMap.add tycon.DemangledExceptionName tycon acc
        cacheOptRef mtyp.FSharpExceptionsLookupTable (fun () -> 
           List.foldBack add_exconsByDemangledName mtyp.ExceptionDefinitions  Map.empty)

    member mtyp.ModulesAndNamespacesByDemangledName = 
        let add_moduleByDemangledName (tycon:Entity) acc = 
            if tycon.IsModuleOrNamespace then 
                NameMap.add (demangled_name_of_modul tycon) tycon acc
            else acc
        cacheOptRef mtyp.ModulesAndNamespacesLookupTable (fun () -> 
           NameMap.foldRange add_moduleByDemangledName mtyp.AllEntities  Map.empty)

type CcuThunk with 
    member ccu.TopModulesAndNamespaces = ccu.Contents.ModuleOrNamespaceType.ModulesAndNamespacesByDemangledName
    member ccu.TopTypeAndExceptionDefinitions = ccu.Contents.ModuleOrNamespaceType.TypeAndExceptionDefinitions

let name_of_rfield     v = v.rfield_id.idText
let set_rigid_of_tpdata       x b = x.typar_flags <- TyparFlags.encode_rigid_of_tpflags       b x.typar_flags
let set_from_error_of_tpdata  x b = x.typar_flags <- TyparFlags.encode_from_error_of_tpflags  b x.typar_flags
let set_static_req_of_tpdata  x b = x.typar_flags <- TyparFlags.encode_static_req_of_tpflags  b x.typar_flags
let set_dynamic_req_of_tpdata x b = x.typar_flags <- TyparFlags.encode_dynamic_req_of_tpflags b x.typar_flags
let set_compgen_of_tpdata x b     = x.typar_flags <- TyparFlags.encode_compgen_of_tpflags     b x.typar_flags
let set_kind_of_tpdata        x b = x.typar_flags <- TyparFlags.encode_kind_of_tpflags        b x.typar_flags


//---------------------------------------------------------------------------
// Aggregate operations to help transform the components that 
// make up the entire compilation unit
//---------------------------------------------------------------------------

#if STANDALONE_METADATA
#else
let mapTImplFile      f   (TImplFile(fragName,pragmas,moduleExpr)) = TImplFile(fragName, pragmas,f moduleExpr)
let fmapTImplFile     f z (TImplFile(fragName,pragmas,moduleExpr)) = let z,moduleExpr = f z moduleExpr in z,TImplFile(fragName,pragmas,moduleExpr)
let map_acc_TImplFile f z (TImplFile(fragName,pragmas,moduleExpr)) = let moduleExpr,z = f z moduleExpr in TImplFile(fragName,pragmas,moduleExpr), z
let foldTImplFile     f z (TImplFile(fragName,pragmas,moduleExpr)) = f z moduleExpr
#endif

//---------------------------------------------------------------------------
// Equality relations on locally defined things 
//---------------------------------------------------------------------------

let local_tcref_eq  (lv1:Tycon) (lv2:Tycon) = lv1.Stamp === lv2.Stamp
let typar_ref_eq    (lv1:Typar) (lv2:Typar) = lv1.Stamp === lv2.Stamp


/// Equality on value specs, implemented as reference equality
let vspec_eq (lv1: Val) (lv2: Val) = (lv1 === lv2)

/// Equality on CCUs, implemented as reference equality
let ccu_eq (mv1: ccu) (mv2: ccu) = (mv1 === mv2) || (mv1.Contents === mv2.Contents)

/// Equality on type varialbes, implemented as reference equality
let tpspec_eq (tp1: Typar) (tp2: Typar) = (tp1 === tp2)


let deref_tycon (tcr :TyconRef) = tcr.Deref
/// Identical to tcref.Deref and deref_tycon, just used to help distinguish what kind of entity we expect here
let deref_modul (tcr :ModuleOrNamespaceRef) = tcr.Deref


let deref_val (vr :ValRef) = vr.Deref
let (|ValDeref|) (vr :ValRef) = vr.Deref
let try_deref_val (vr :ValRef) = vr.TryDeref



//---------------------------------------------------------------------------
// Get information from refs
//---------------------------------------------------------------------------

exception InternalUndefinedTyconItem of string * TyconRef * string

type RecdFieldRef with 
    member x.TyconRef = let (RFRef(tcref,id)) = x in tcref
    member x.FieldName = let (RFRef(tcref,id)) = x in id
    member x.Tycon = x.TyconRef.Deref
    member x.RecdField = 
        let (RFRef(tcref,id)) = x
        match tcref.GetFieldByName id with 
        | Some res -> res
        | None -> raise (InternalUndefinedTyconItem ("field",tcref, id))
    member x.PropertyAttribs = x.RecdField.PropertyAttribs
    member x.Range = x.RecdField.Range

type UnionCaseRef with 
    member x.TyconRef = let (UCRef(tcref,_)) = x in tcref
    member x.CaseName = let (UCRef(_,nm)) = x in nm
    member x.Tycon = x.TyconRef.Deref
    member x.UnionCase = 
        let (UCRef(tcref,nm)) = x
        match tcref.GetUnionCaseByName nm with 
        | Some res -> res
        | None -> raise (InternalUndefinedTyconItem ("union case",tcref, nm))
    member x.Attribs = x.UnionCase.Attribs
    member x.Range = x.UnionCase.Range

//--------------------------------------------------------------------------
// Make references to TAST items
//--------------------------------------------------------------------------

let mk_rfref tcref f = RFRef(tcref, f)
let mk_ucref tcref c = UCRef(tcref, c)
let rfref_of_rfield tcref f = mk_rfref tcref f.rfield_id.idText
let ucref_of_ucase tcref c = mk_ucref tcref c.ucase_id.idText
let mk_nlpath (NLPath(mref,p)) n = NLPath(mref,Array.append p [| n |])
let mk_cpath (CompPath(scoref,p)) n modKind = CompPath(scoref,p@[(n,modKind)])



let path_of_nlpath (NLPath(a,b)) = b
let ccu_of_nlpath  (NLPath(a,b)) = a

let demangled_name_of_modref    (x:ModuleOrNamespaceRef) = (deref_modul x) |> demangled_name_of_modul 

let nlpath_of_nlref nlr = nlr.nlr_nlpath 
let item_of_nlref   nlr = nlr.nlr_item
let ccu_of_nlref    nlr = ccu_of_nlpath (nlpath_of_nlref nlr)

let VRef_private(x) : ValRef = { binding=x; nlr=Unchecked.defaultof<_> }      
let VRef_nonlocal(x) : ValRef = { binding=Unchecked.defaultof<_>; nlr=x }      
let VRef_nonlocal_preresolved x xref : ValRef = { binding=x; nlr=xref }      
let (|VRef_private|VRef_nonlocal|) (x: ValRef) = 
    match box x.nlr with 
    | null -> VRef_private(x.binding) 
    | _ -> VRef_nonlocal(x.nlr)

let ERef_private(x) : EntityRef = { binding=x; nlr=Unchecked.defaultof<_> }      
let ERef_nonlocal(x) : EntityRef = { binding=Unchecked.defaultof<_>; nlr=x }      
let ERef_nonlocal_preresolved x xref : EntityRef = { binding=x; nlr=xref }      
let (|ERef_private|ERef_nonlocal|) (x: EntityRef) = 
    match box x.nlr with 
    | null -> ERef_private(x.binding) 
    | _ -> ERef_nonlocal(x.nlr)

let ccu_of_vref iref =  
    match iref with 
    | VRef_private _ -> None
    | VRef_nonlocal(nlr) -> Some (ccu_of_nlref iref.nlr)

let ccu_of_tcref iref =  
    match iref with 
    | ERef_private _ -> None
    | ERef_nonlocal(nlr) -> Some (ccu_of_nlref iref.nlr)

//--------------------------------------------------------------------------
// Type parameters and inference unknowns
//-------------------------------------------------------------------------

let mk_typar_ty (tp:Typar) = 
    match tp.Kind with 
    | KindType -> tp.AsType 
    | KindMeasure -> TType_measure (MeasureVar tp)

let CopyTypar (tp: Typar) = let x = tp.Data in Typar.New { x with typar_stamp=new_stamp() }
let CopyTypars tps = List.map CopyTypar tps

let fixup_typar_constraints (tp:Typar) cs =
    tp.Data.typar_constraints <-  cs


//--------------------------------------------------------------------------
// Inference variables
//-------------------------------------------------------------------------- 

let tpref_is_solved (r:Typar) = 
    match r.Solution with 
    | None -> false
    | _ -> true

    
let try_shortcut_solved_upref canShortcut (r:Typar) = 
    if r.Kind = KindType then failwith "try_shortcut_solved_upref: kind=type";
    match r.Solution with
    | Some (TType_measure unt) -> 
        if canShortcut then 
            match unt with 
            | MeasureVar r2 -> 
               match r2.Solution with
               | None -> ()
               | Some res as soln -> 
                  r.Data.typar_solution <- soln;
            | _ -> () 
        unt
    | _ -> 
        failwith "try_shortcut_solved_upref: unsolved"
      

let rec strip_upeqnsA canShortcut measure = 
    match measure with 
    | MeasureVar r when tpref_is_solved r -> strip_upeqnsA canShortcut (try_shortcut_solved_upref canShortcut r)
    | _ -> measure

let rec strip_tpeqnsA canShortcut ty = 
    match ty with 
    | TType_var r -> 
        match r.Solution with
        | Some soln -> 
            if canShortcut then 
                match soln with 
                // We avoid shortcutting when there are additional constraints on the type variable we're trying to cut out
                // This is only because IterType likes to walk _all_ the constraints _everywhere_ in a type, including
                // those attached to _solved_ type variables. In an ideal world this would never be needed - see the notes
                // on IterType.
                | TType_var r2 when r2.Constraints.IsEmpty -> 
                   match r2.Solution with
                   | None -> ()
                   | Some res as soln2 -> 
                      r.Data.typar_solution <- soln2;
                | _ -> () 
            strip_tpeqnsA canShortcut soln
        | None -> 
            ty
    | TType_measure measure -> 
        TType_measure (strip_upeqnsA canShortcut measure)
    | _ -> ty

let strip_tpeqns ty = strip_tpeqnsA false ty
let strip_upeqns measure = strip_upeqnsA false measure

//--------------------------------------------------------------------------
// Construct local references
//-------------------------------------------------------------------------- 


let mk_nlr mp id = {nlr_nlpath = mp; nlr_item=id }
let mk_local_tcref x = ERef_private x
let mk_nonlocal_tcref mp id = ERef_nonlocal (mk_nlr mp id)
let mk_nonlocal_vref mp id = VRef_nonlocal (mk_nlr mp id)
let mk_nonlocal_tcref_preresolved x mp id = ERef_nonlocal_preresolved x (mk_nlr mp id)
let mk_nonlocal_vref_preresolved x mp id = VRef_nonlocal_preresolved x (mk_nlr mp id)

//--------------------------------------------------------------------------
// From Ref_private to Ref_nonlocal when exporting data.
//--------------------------------------------------------------------------

let enclosing_nlpath_of_pubpath viewedCcu (PubPath(p,nm)) = NLPath(viewedCcu, p)
let nlpath_of_pubpath viewedCcu (PubPath(p,nm)) = NLPath(viewedCcu,Array.append p [| nm |])
let nlpath_of_modul viewedCcu (v:ModuleOrNamespace) = v.PublicPath |> Option.map (nlpath_of_pubpath viewedCcu)

let nlref_of_pubpath viewedCcu (PubPath(p,nm) as pubpath) x = 
    mk_nlr (enclosing_nlpath_of_pubpath viewedCcu pubpath) nm

let rescope_val_pubpath viewedCcu pubpath x : ValRef = VRef_nonlocal (nlref_of_pubpath viewedCcu pubpath x)
let rescope_tycon_pubpath viewedCcu pubpath x : TyconRef = ERef_nonlocal (nlref_of_pubpath viewedCcu pubpath x)

//---------------------------------------------------------------------------
// Equality between TAST items.
//---------------------------------------------------------------------------

let vref_in_this_assembly compilingFslib (x: ValRef) = 
    match x with 
    | VRef_private _ -> true
    | VRef_nonlocal _ -> compilingFslib

let tcref_in_this_assembly compilingFslib (x: TyconRef) = 
    match x with 
    | ERef_private _ -> true
    | ERef_nonlocal _ -> compilingFslib

let array_path_eq (y1:string[]) (y2:string[]) =
    let len1 = y1.Length 
    let len2 = y2.Length 
    (len1 = len2) && 
    (let rec loop i = (i >= len1) || (y1.[i] = y2.[i] && loop (i+1)) 
     loop 0)

let nlpath_eq (NLPath(x1,y1) as smr1) (NLPath(x2,y2) as smr2) = 
    smr1 === smr2 || (ccu_eq x1 x2 && array_path_eq y1 y2)

/// This predicate tests if non-local resolution paths are definitely known to resolve
/// to different entities. All references with different named paths always resolve to 
/// different entities. Two references with the same named paths may resolve to the same 
/// entities even if they reference through different CCUs, because one reference
/// may be forwarded to another via a .NET TypeForwarder.
let nlpath_definitely_not_eq (NLPath(x1,y1)) (NLPath(x2,y2) as smr2) = 
    not (array_path_eq y1 y2)

let nlref_eq nlr1 nlr2 = 
    (nlr1 === nlr2 ) || 
    (nlpath_eq nlr1.nlr_nlpath nlr2.nlr_nlpath && 
     (nlr1.nlr_item === nlr2.nlr_item  || nlr1.nlr_item  = nlr2.nlr_item))

/// See nlpath_definitely_not_eq
let nlref_definitely_not_eq nlr1 nlr2 = 
    (nlpath_definitely_not_eq nlr1.nlr_nlpath nlr2.nlr_nlpath || nlr1.nlr_item <> nlr2.nlr_item)

/// Compiler-internal references to items in fslib are generated as Ref_nonlocal even when compiling fslib 
let fslib_nlpath_eq_pubpath nlr (PubPath(path,nm)) = 
    nlr.nlr_item = nm  &&
    let (NLPath(ccu,p)) = nlr.nlr_nlpath 
    if (array_path_eq p path) then true 
    else ( (* warning(Failure(sprintf "%s <> %s" (text_of_arr_path p) (text_of_arr_path path))); *) false)

let fslib_refs_eq ppF namef (|Ref_private|Ref_nonlocal|) fslibCcu x y  =
    match x,y with 
    | (Ref_nonlocal nlr, Ref_private x)
    | (Ref_private x, Ref_nonlocal nlr) ->
        ccu_eq (ccu_of_nlpath nlr.nlr_nlpath) fslibCcu &&
        let pubpathOpt = ppF x 
        isSome pubpathOpt && fslib_nlpath_eq_pubpath nlr (Option.get pubpathOpt)
    | (Ref_private x, Ref_private y) ->
        let pubpathOpt1 = ppF x 
        let pubpathOpt2 = ppF y 
        isSome pubpathOpt1 && isSome pubpathOpt2 && pubpathOpt1 = pubpathOpt2
    | _ -> false
  
let prim_tcref_eq compilingFslib fslibCcu (x : TyconRef) (y : TyconRef) = 
    x === y ||
    match x.IsResolved,y.IsResolved with 
    | true, true when not compilingFslib -> x.ResolvedTarget === y.ResolvedTarget 
    | _ -> 
    match x.IsLocalRef,y.IsLocalRef with 
    | false, false when 
        (// Two tcrefs with identical paths are always equal
         nlref_eq x.nlr y.nlr || 
         // The tcrefs may have forwarders. If they may possibly be equal then resolve them to get their canonical references
         // and compare those using pointer equality.
         (not (nlref_definitely_not_eq x.nlr y.nlr) && x.Deref === y.Deref)) -> 
        true
    | _ -> 
        compilingFslib && fslib_refs_eq pubpath_of_tycon (fun (tc:Tycon) -> tc.MangledName) (|ERef_private|ERef_nonlocal|) fslibCcu x y  

let prim_ucref_eq compilingFslib fslibCcu (UCRef(tcr1,c1) as uc1) (UCRef(tcr2,c2) as uc2) = 
    uc1 === uc2 || (prim_tcref_eq compilingFslib fslibCcu tcr1 tcr2 && c1 = c2)

let prim_vref_eq compilingFslib fslibCcu (x : ValRef) (y : ValRef) =
    x === y ||
    match x.IsResolved,y.IsResolved with 
    | true, true when x.ResolvedTarget === y.ResolvedTarget -> true
    | _ -> 
    match x.IsLocalRef,y.IsLocalRef with 
    | true,true when vspec_eq x.PrivateTarget y.PrivateTarget -> true
    | false,false when nlref_eq x.nlr y.nlr -> true
    | _ -> compilingFslib && fslib_refs_eq pubpath_of_val  name_of_val (|VRef_private|VRef_nonlocal|) fslibCcu x y
 
//---------------------------------------------------------------------------
// pubpath/cpath mess
//---------------------------------------------------------------------------

#if STANDALONE_METADATA
#else
let GetNameOfScopeRef sref = 
    match sref with 
    | ScopeRef_local -> "<local>"
    | ScopeRef_module mref -> mref.Name
    | ScopeRef_assembly aref -> aref.Name
let mangled_text_of_cpath (CompPath(scoref,path)) = GetNameOfScopeRef scoref ^"/"^ text_of_path (List.map fst path)
let string_of_access (TAccess paths) = String.concat ";" (List.map mangled_text_of_cpath paths)
#endif
  
let mangled_path_of_cpath (CompPath(scoref,path))  = List.map fst path
let pubpath_of_cpath (id:ident) cpath = PubPath(Array.of_list (mangled_path_of_cpath cpath),id.idText)

let demangled_path_of_cpath (CompPath(scoref,path)) = 
    path |> List.map (fun (nm,k) -> demangled_name_of_entity_name nm k)

let parent_cpath (CompPath(scoref,cpath)) = 
    let a,b = List.frontAndBack cpath 
    CompPath(scoref,a)

let full_cpath_of_modul (m:ModuleOrNamespace) = 
    let (CompPath(scoref,cpath))  = m.CompilationPath
    CompPath(scoref,cpath@[(m.MangledName, m.ModuleOrNamespaceType.ModuleOrNamespaceKind)])

// Can cpath2 be accessed given a right to access cpath1. That is, is cpath2 a nested type or namespace of cpath1. Note order of arguments.
let can_access_cpath_from (CompPath(scoref1,cpath1)) (CompPath(scoref2,cpath2)) =
    let rec loop p1 p2  = 
        match p1,p2 with 
        | (a1,k1)::rest1, (a2,k2)::rest2 -> (a1=a2) && (k1=k2) && loop rest1 rest2
        | [],_ -> true 
        | _ -> false // cpath1 is longer
    loop cpath1 cpath2 &&
    (scoref1 = scoref2)

let can_access_cpath_from_one_of cpaths cpathTest =
    List.exists (fun cpath -> can_access_cpath_from cpath cpathTest) cpaths

let can_access_from (TAccess x) cpath = 
    x |> List.forall (fun cpath1 -> can_access_cpath_from cpath1 cpath)

let can_access_from_everywhere (TAccess x) = x.IsEmpty
let can_access_from_somewhere (TAccess x) = true
let IsLessAccessible (TAccess aa) (TAccess bb)  = 
   (* not (ListSet.isSubsetOf (=) aa bb) *)
    not (aa |> List.forall(fun a -> bb |> List.exists (fun b -> can_access_cpath_from a b)))

/// Given (newPath,oldPath) replace oldPath by newPath in the TAccess.
let access_subst_paths (newPath,oldPath) (TAccess paths) =
    let subst cpath = if cpath=oldPath then newPath else cpath
    TAccess (List.map subst paths)

let cpath_of_ccu (ccu:ccu) = CompPath(ccu.ILScopeRef,[]) 
let nlpath_of_ccu ccu = NLPath(ccu,[| |]) 
let taccessPublic = TAccess []

//---------------------------------------------------------------------------
// Construct TAST nodes
//---------------------------------------------------------------------------

let SkipFreeVarsCache() = ()
let SkipCacheCompute (cache:SkipFreeVarsCache) f = f()

let NewFreeVarsCache() = new_cache ()

let MakeUnionCasesTable ucs = 
    { ucases_by_index = Array.of_list ucs; 
      ucases_by_name = NameMap.of_keyed_list (fun uc -> uc.DisplayName) ucs }
                                                                  
let MakeRecdFieldsTable ucs = 
    { rfields_by_index = Array.of_list ucs; 
      rfields_by_name = ucs  |> NameMap.of_keyed_list (fun rfld -> rfld.Name) }
                                                                  

let MakeUnionCases ucs = 
    { funion_ucases=MakeUnionCasesTable ucs; 
#if STANDALONE_METADATA
#else
      funion_ilx_repr=new_cache()
#endif
    }
let MakeUnionRepr ucs = TFiniteUnionRepr (MakeUnionCases ucs)


let new_ccu nm x  : ccu = CcuThunk.Create(nm,x)

let NewTypar (kind,rigid,Typar(id,staticReq,isCompGen),isFromError,dynamicReq,attribs) = 
    Typar.New
      { typar_id = id; 
        typar_stamp = new_stamp(); 
        typar_flags= TyparFlags.encode (kind,rigid,isFromError,isCompGen,staticReq,dynamicReq); 
        typar_attribs= attribs; 
        typar_solution = None;
        typar_constraints=[];
        typar_xmldoc = emptyXmlDoc; (* todo *) } 

let mk_rigid_typar nm m = NewTypar (KindType,TyparRigid,Typar(mksyn_id m nm,NoStaticReq,true),false,DynamicReq,[])
let new_tcaug () =  { tcaug_compare=None; 
                      tcaug_compare_withc=None; 
                      tcaug_equals=None; 
                      tcaug_structural_hash=None;
                      tcaug_hash_and_equals_withc=None; 
                      tcaug_hasObjectGetHashCode=false; 
                      tcaug_adhoc=NameMultiMap.empty; 
                      tcaug_super=None;
                      tcaug_implements=[]; 
                      tcaug_closed=false; 
                      tcaug_abstract=false; }

let combineAccess (TAccess a1) (TAccess a2) = TAccess(a1@a2)

let NewUnionCase id nm tys rty attribs docOption vis = 
    { ucase_id=id;
      ucase_il_name=nm;
      ucase_xmldoc=docOption;
      ucase_access=vis;
      ucase_rfields = MakeRecdFieldsTable tys;
      ucase_rty = rty;
      ucase_attribs=attribs } 

let set_tcaug_compare              tcaug x = tcaug.tcaug_compare              <- Some x
let set_tcaug_compare_withc   tcaug x = tcaug.tcaug_compare_withc   <- Some x
let set_tcaug_equals               tcaug x = tcaug.tcaug_equals               <- Some x
let set_tcaug_hash_and_equals_withc tcaug x = tcaug.tcaug_hash_and_equals_withc <- Some x
let set_tcaug_hasObjectGetHashCode tcaug b = tcaug.tcaug_hasObjectGetHashCode <- b


let NewModuleOrNamespaceType mkind tycons vals = 
    new ModuleOrNamespaceType(mkind, NameMap.of_keyed_list name_of_val vals, NameMap.of_keyed_list (fun (tc:Tycon) -> tc.MangledName) tycons)

let empty_mtype mkind = NewModuleOrNamespaceType mkind [] []


let NewExn cpath (id:ident) vis repr attribs doc = 
    let id = mksyn_id id.idRange (mangle_exception_name id.idText)
    Tycon.New "exnc"
      { entity_stamp=new_stamp();
        entity_attribs=attribs;
        entity_kind=KindType;
        entity_name=id.idText;
        entity_range=id.idRange;
        entity_exn_info= repr;
        entity_tycon_tcaug=new_tcaug();
        entity_xmldoc=doc;
        entity_pubpath=cpath |> Option.map (pubpath_of_cpath id);
        entity_accessiblity=vis;
        entity_tycon_repr_accessibility=vis;
        entity_modul_contents = notlazy (empty_mtype FSharpModule);
        entity_cpath= cpath;
        entity_typars=LazyWithContext<_,_>.NotLazy [];
        entity_tycon_abbrev = None;
        entity_tycon_repr = None;
        entity_uses_prefix_display=false; (* REVIEW, though note these are not generic anyway *)
        entity_is_modul_or_namespace = false;
        entity_il_repr_cache= new_cache() ;  } 

let NewRecdField  stat konst id ty isMutable pattribs fattribs docOption vis secret =
    { rfield_mutable=isMutable;
      rfield_pattribs=pattribs;
      rfield_fattribs=fattribs;
      rfield_type=ty;
      rfield_static=stat;
      rfield_const=konst;
      rfield_access = vis;
      rfield_secret = secret;
      rfield_xmldoc = docOption; 
      rfield_id=id; }

    
let NewTycon cpath (nm,m) vis repr_vis kind typars docOption preferPostfix mtyp =
    let stamp = new_stamp() 
    Tycon.New "tycon"
      { entity_stamp=stamp;
        entity_name=nm;
        entity_kind=kind;
        entity_range=m;
        entity_uses_prefix_display=preferPostfix;
        entity_attribs=[];
        entity_typars=typars;
        entity_tycon_abbrev = None;
        entity_tycon_repr = None;
        entity_tycon_repr_accessibility = repr_vis;
        entity_exn_info=TExnNone;
        entity_tycon_tcaug=new_tcaug();
        entity_modul_contents = mtyp;
        entity_accessiblity=vis;
        entity_xmldoc = docOption;
        entity_pubpath=cpath |> Option.map (pubpath_of_cpath (mksyn_id m nm));
        entity_cpath = cpath;
        entity_is_modul_or_namespace =false;
        entity_il_repr_cache = new_cache(); }

let NewILTycon nlpath id tps (scoref,enc,tdef) mtyp =
    let tycon = NewTycon nlpath id taccessPublic taccessPublic KindType tps emptyXmlDoc true mtyp 
    tycon.Data.entity_tycon_repr <- Some (TILObjModelRepr (scoref,enc,tdef));
    tycon.TypeContents.tcaug_closed <- true;
    tycon

exception Duplicate of string * string * range
exception NameClash of string * string * string * range * string * string * range
exception FullAbstraction of string * range

let mk_namemap s (idf: _ -> ident) items = 
    (items,NameMap.empty) ||> List.foldBack (fun item sofar -> 
        let id = idf item 
        if NameMap.mem id.idText sofar then raise (Duplicate(s,id.idText,id.idRange));
        NameMap.add id.idText item sofar) 

let mk_tycon_namemap     = mk_namemap "type"      id_of_tycon
let mk_exnconstr_namemap = mk_namemap "exception" id_of_tycon
let mk_val_namemap       = mk_namemap "value"     id_of_val

let NewModuleOrNamespace cpath vis (id:ident) xml attribs mtype = 
    let stamp = new_stamp() 
    // Put the module suffix on if needed 
    Tycon.New "mspec"
      { entity_name=id.idText;
        entity_range = id.idRange;
        entity_stamp=stamp;
        entity_kind=KindType;
        entity_modul_contents = mtype;
        entity_uses_prefix_display=false; 
        entity_is_modul_or_namespace =true;
        entity_typars=LazyWithContext<_,_>.NotLazy [];
        entity_tycon_abbrev = None;
        entity_tycon_repr = None;
        entity_tycon_repr_accessibility = vis;
        entity_exn_info=TExnNone;
        entity_tycon_tcaug=new_tcaug();
        entity_pubpath=cpath |> Option.map (pubpath_of_cpath id);
        entity_cpath=cpath;
        entity_accessiblity=vis;
        entity_attribs=attribs;
        entity_xmldoc=xml;
        entity_il_repr_cache = new_cache(); }

let NewVal (id:ident,ty,isMutable,isCompGen,arity,cpathOpt,vis,vrec,specialRepr,baseOrThis,attribs,mustinline,doc,isTopBinding,isExtensionMember,isImplicitCtor,isTyFunc,konst,actualParent) : Val = 
    let stamp = new_stamp() 
    if !verboseStamps then dprintf "NewVal, %s#%d\n" id.idText stamp;
    Val.New
        { val_stamp = stamp;
          val_name=id.idText;
          val_range=id.idRange;
          val_defn_range=id.idRange;
          val_defn=None;
          val_top_repr_info= arity;
          val_actual_parent= actualParent;
          val_flags = ValFlags.encode(vrec,baseOrThis,isCompGen,mustinline,isMutable,isTopBinding,isExtensionMember,isImplicitCtor,isTyFunc);
          val_pubpath= cpathOpt |> Option.map (pubpath_of_cpath id);
          val_const= konst;
          val_access=vis;
          val_member_info=specialRepr;
          val_attribs=attribs;
          val_type = ty;
          val_xmldoc = doc; } 


let NewCcuContents sref m nm mty =
    NewModuleOrNamespace (Some(CompPath(sref,[]))) taccessPublic (mksyn_id m nm) emptyXmlDoc [] (notlazy mty)
      

//--------------------------------------------------------------------------
// Cloning and adjusting
//--------------------------------------------------------------------------
 
/// Create a tycon based on an existing one using the function 'f'. 
/// We require that we be given the new parent for the new tycon. 
/// We pass the new tycon to 'f' in case it needs to reparent the 
/// contents of the tycon. 
let NewModifiedTycon f (orig:Tycon) = 
    let stamp = new_stamp() 
    let data = orig.Data 
    if !verboseStamps then dprintf "NewModifiedTycon, %s#%d, based on %s#%d\n" orig.MangledName stamp orig.MangledName data.entity_stamp;
    Tycon.New "NewModifiedTycon" (f { data with entity_stamp=stamp; }) 
    
/// Create a module Tycon based on an existing one using the function 'f'. 
/// We require that we be given the parent for the new module. 
/// We pass the new module to 'f' in case it needs to reparent the 
/// contents of the module. 
let NewModifiedModuleOrNamespace f orig = 
    orig |> NewModifiedTycon (fun d -> 
        { d with entity_modul_contents = notlazy (f (d.entity_modul_contents.Force())) }) 

/// Create a Val based on an existing one using the function 'f'. 
/// We require that we be given the parent for the new Val. 
let NewModifiedVal f (orig:Val) = 
    let data = orig.Data
    let stamp = new_stamp() 
    if !verboseStamps then dprintf "NewModifiedVal, stamp #%d, based on stamp #%d\n" stamp data.val_stamp;
    let data' = f { data with val_stamp=stamp }
    Val.New data'

let NewClonedModuleOrNamespace orig =  NewModifiedModuleOrNamespace (fun mty -> mty) orig
let NewClonedTycon orig =  NewModifiedTycon (fun d -> d) orig

//------------------------------------------------------------------------------

/// Combine module types when multiple namespace fragments contribute to the
/// same namespace, making new module specs as we go.
let private combine_maps f m1 m2 = 
    Map.foldBack (fun k v acc -> Map.add k (if Map.contains k m2 then f [v;Map.find k m2] else f [v]) acc) m1 
      (Map.foldBack (fun k v acc -> if Map.contains k m1 then acc else Map.add k (f [v]) acc) m2 Map.empty)


let rec private combine_msigtyps path m (mty1:ModuleOrNamespaceType)  (mty2:ModuleOrNamespaceType)  = 
    match mty1.ModuleOrNamespaceKind,mty2.ModuleOrNamespaceKind  with 
    | Namespace,Namespace -> 
        let kind = mty1.ModuleOrNamespaceKind
        let entities = combine_maps (combine_entitiesl path) mty1.AllEntities mty2.AllEntities 
        let vals = combine_maps (function [] -> failwith "??" | [v] -> v | (v:Val) :: _ -> errorR(Error( sprintf "two values named '%s' occur in namespace '%s' in two parts of this assembly" v.MangledName (text_of_path path),v.Range)); v) mty1.AllValuesAndMembers mty2.AllValuesAndMembers 
        new ModuleOrNamespaceType(kind, vals, entities)
    | Namespace, _ | _,Namespace -> error(Error(sprintf "a namespace and a module named '%s' both occur in two parts of this assembly" (text_of_path path),m))
    | _-> error(Error(sprintf "two modules named '%s' occur in two parts of this assembly" (text_of_path path),m))

and private combine_entitiesl path l = 
    match l with
    | h :: t -> List.fold (combine_entites path) h t
    | _ -> failwith "combine_entitiesl"

and private combine_entites path (tycon1:Entity) (tycon2:Entity) = 

    match tycon1.IsModuleOrNamespace, tycon2.IsModuleOrNamespace with
    | true,true -> 
        tycon1 |> NewModifiedTycon (fun data1 -> 
                    { data1 with 
                         entity_xmldoc = MergeXmlDoc tycon1.XmlDoc tycon2.XmlDoc;
                         entity_attribs = tycon1.Attribs @ tycon2.Attribs;
                         entity_modul_contents=lazy (combine_msigtyps (path@[demangled_name_of_modul tycon2]) tycon2.Range tycon1.ModuleOrNamespaceType tycon2.ModuleOrNamespaceType); }) 
    | false,false -> 
        error(Error( sprintf "two type definitions named '%s' occur in namespace '%s' in two parts of this assembly" tycon2.MangledName (text_of_path path),tycon2.Range))
    | _,_ -> 
        error(Error( sprintf "a module and a type definition named '%s' occur in namespace '%s' in two parts of this assembly" tycon2.MangledName (text_of_path path),tycon2.Range))
    
and combine_mtyps path m l = 
    match l with
    | h :: t -> List.fold (combine_msigtyps path m) h t
    | _ -> failwith "combine_mtyps"

//--------------------------------------------------------------------------
// Resource format for pickled data
//--------------------------------------------------------------------------

let FSharpOptimizationDataResourceName = "FSharpOptimizationData"
let FSharpSignatureDataResourceName = "FSharpSignatureData"


#if STANDALONE_METADATA

type TcGlobals = 
    { nativeptr_tcr:TyconRef;
      nativeint_tcr:TyconRef;
      byref_tcr:TyconRef;
      il_arr1_tcr:TyconRef;
      il_arr2_tcr:TyconRef;
      il_arr3_tcr:TyconRef;
      il_arr4_tcr:TyconRef;
      unit_tcr:TyconRef; }

let mk_nativeptr_typ g ty = TType_app (g.nativeptr_tcr, [ty])
let mk_byref_typ g ty = TType_app (g.byref_tcr, [ty])
let mk_unit_typ g = TType_app (g.unit_tcr, [])
let mk_nativeint_typ g = TType_app (g.nativeint_tcr, [])

let mk_multi_dim_array_typ g n ty = 
    if n = 1 then TType_app (g.il_arr1_tcr, [ty]) 
    elif n = 2 then TType_app (g.il_arr2_tcr, [ty]) 
    elif n = 3 then TType_app (g.il_arr3_tcr, [ty]) 
    elif n = 4 then TType_app (g.il_arr4_tcr, [ty]) 
    else failwith "F# supports a maxiumum .NET array dimension of 4"


#endif
