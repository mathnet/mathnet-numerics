// (c) Microsoft Corporation. All rights reserved
// -------------------------------------------------------------------- 
// Erase discriminated unions.
// -------------------------------------------------------------------- 

#light

module Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.EraseIlxClassunions

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
open Microsoft.FSharp.Compiler.AbstractIL.Morphs 

module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 
module Ilx = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types
module Ilprint = Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 
module Illib = Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

open Illib
open Il
open Ilx
open Ilprint


// This functor helps us make representation decisions for F# union type compilation
type UnionReprDecisions<'Union,'Alt,'Type>
          (getAlternatives: 'Union->'Alt[],
           nullPermitted:'Union->bool,
           isGeneric:'Union -> bool,
           isNullary:'Alt->bool,
           nameOfAlt : 'Alt -> string,
           makeRootType: 'Union -> 'Type,
           makeNestedType: 'Union * string -> 'Type) =

    static let TaggingThresholdFixedConstant = 8 // note: permanently and forever fixed

    member repr.OptimizeAllAlternativesToConstantFieldsInRootClass cu = 
        let alts = getAlternatives cu
        (alts.Length > 1 && Array.forall isNullary alts) && 
        // can't use static fields for generic nullary constructors - though  Whidbey supports this, if we knew we were targetting generics... 
        not (isGeneric  cu)

    member repr.UseRuntimeTypes cu = 
        let alts = getAlternatives cu
        (alts.Length < TaggingThresholdFixedConstant) &&
        (not (repr.OptimizeAllAlternativesToConstantFieldsInRootClass cu))

    // WARNING: this must match IsUnionTypeWithNullAsTrueValue in the F# compiler *
    // REVIEW: make this attribute controlled 
    member repr.OptimizeAlternativeToNull (cu,alt) = 
        let alts = getAlternatives cu
        nullPermitted cu &&
        repr.UseRuntimeTypes cu && (* tags won't work if we're using "null" *)
        Array.existsOne isNullary alts  &&
        Array.exists (isNullary >> not) alts  &&
        isNullary alt  (* is this the one? *)

    member repr.OptimizingOneAlternativeToNull cu = 
        let alts = getAlternatives cu
        alts |> Array.existsOne (fun alt -> repr.OptimizeAlternativeToNull (cu,alt))

    member repr.OptimizeSingleNonNullaryAlternativeToRootClass (cu,alt) = 
        // Check all nullary constructors are being represented without using sub-classes 
        let alts = getAlternatives cu
        alts |> Array.forall (fun alt2 -> 
            not (isNullary alt2) || 
            repr.OptimizeAlternativeToNull (cu,alt2))   &&

        // Check there is only one non-nullary constructor 
        Array.existsOne (isNullary >> not) alts  &&
        not (isNullary alt)

    member repr.OptimizeAlternativeToConstantFieldInTaggedRootClass (cu,alt) = 
        let alts = getAlternatives cu
        isNullary alt &&
        not (isGeneric cu) &&
        not (repr.OptimizeAlternativeToNull (cu,alt))  &&
        not (repr.UseRuntimeTypes cu)

    member repr.OptimizeAlternativeToRootClass (cu,alt) = 
        repr.OptimizeAllAlternativesToConstantFieldsInRootClass cu ||
        repr.OptimizeAlternativeToConstantFieldInTaggedRootClass (cu,alt) ||
        repr.OptimizeSingleNonNullaryAlternativeToRootClass(cu,alt)
      
    member repr.MaintainPossiblyUniqueConstantFieldForAlternative(cu,alt) = 
        not (repr.OptimizeAlternativeToNull (cu,alt)) &&
        isNullary alt


    member repr.TypeForAlterntative (cuspec,alt) =
        if repr.OptimizeAlternativeToRootClass (cuspec,alt) || repr.OptimizeAlternativeToNull (cuspec,alt) then makeRootType cuspec
        else makeNestedType (cuspec,"_"^nameOfAlt alt)


let backend_derived_tref tref nm = mk_tref_in_tref (tref,"_"^nm)


let cuspecRepr = 
    UnionReprDecisions
        (altsarray_of_cuspec,
         nullPermitted_of_cuspec, 
         (fun cuspec -> nonNil (inst_of_cuspec cuspec)),
         alt_is_nullary,
         name_of_alt,
         (fun cuspec -> mk_boxed_typ (tref_of_cuspec cuspec) (inst_of_cuspec cuspec)),
         (fun (cuspec,nm) -> mk_boxed_typ (mk_tref_in_tref (tref_of_cuspec cuspec,nm)) (inst_of_cuspec cuspec)))

type NoTypesGeneratedViaThisReprDecider = NoTypesGeneratedViaThisReprDecider
let cudefRepr = 
    UnionReprDecisions
        ((fun (enc,td,cud) -> cud.cudAlternatives),
         (fun (enc,td,cud) -> cud.cudNullPermitted), 
         (fun (enc,td,cud) -> nonNil td.tdGenericParams),
         alt_is_nullary,
         name_of_alt,
         (fun (enc,td,cud) -> NoTypesGeneratedViaThisReprDecider),
         (fun ((enc,td,cud),nm) -> NoTypesGeneratedViaThisReprDecider))


type cenv  = 
    { ilg: ILGlobals } 

let mk_bblock2 (a,b) = 
    mk_bblock { bblockLabel=a; bblockInstrs= Array.of_list b}


let getter_prop_name (fields: ILFieldDef[]) i = fields.[i].fdName

let tester_name nm = "Is"^nm
let tagPropertyName = "Tag"

(* nb. not currently recording translation assumptions in the default external reference naming scheme *)
let tag_member_name = "tag"
let tag_member_ty ilg = ilg.typ_int32
let virt_tag_field_id ilg = tag_member_name,tag_member_ty ilg
let backend_field_id fdef = 
    // Use the lower case name of a field or constructor as the field/parameter name if it differs from the uppercase name
    let lowerName = String.uncapitalize fdef.fdName in 
    let nm = (if lowerName = fdef.fdName then "_" ^ fdef.fdName else lowerName)
    nm, fdef.fdType

let ref_to_field_in_tspec tspec (nm,ty) = mk_fspec_in_boxed_tspec (tspec,nm,ty)

let const_fname nm = nm^"_uniq"
let const_formal_field_ty nm (baseTypeSpec:ILTypeSpec) = 
    Type_boxed (mk_tspec(baseTypeSpec.TypeRef, List.mapi (fun i _ -> mk_tyvar_ty (uint16 i)) baseTypeSpec.GenericArgs))

let const_fspec nm (baseTypeSpec:ILTypeSpec) = 
    let const_fid = (const_fname nm,const_formal_field_ty nm baseTypeSpec)
    ref_to_field_in_tspec baseTypeSpec const_fid

let baseTypeSpec_of_cuspec cuspec = mk_tspec (tref_of_cuspec cuspec, inst_of_cuspec cuspec)

let tspec_of_alt cuspec alt = cuspecRepr.TypeForAlterntative(cuspec,alt) |> tspec_of_typ
let typ_of_alt cuspec (alt:int) = cuspecRepr.TypeForAlterntative(cuspec,(alt_of_cuspec cuspec alt)) 


let altinfo_of_cuspec cuspec cidx =
    let alt = 
      try Ilx.alt_of_cuspec cuspec cidx 
      with _ -> failwith ("alternative "^string cidx^" not found") 
    let tspec = tspec_of_alt cuspec alt
    name_of_alt alt,tspec,alt

let rtt_discriminate derived_tspec = 
    [ I_isinst (Type_boxed derived_tspec); I_arith AI_ldnull; I_arith AI_cgt_un ]

let ceq_then after = 
    match after with 
    | I_brcmp (BI_brfalse,a,b) -> [I_brcmp (BI_bne_un,a,b)]
    | I_brcmp (BI_brtrue,a,b) ->  [I_brcmp (BI_beq,a,b)]
    | _ -> [I_arith AI_ceq; after]

let rtt_discriminate_then derived_tspec after = 
    match after with 
    | I_brcmp (BI_brfalse,_,_) 
    | I_brcmp (BI_brtrue,_,_) -> 
        [ I_isinst (Type_boxed derived_tspec); after ]
    | _ -> rtt_discriminate derived_tspec @ [ after ]

let get_tag cenv cuspec baseTypeSpec = 
    [ mk_normal_ldfld (ref_to_field_in_tspec baseTypeSpec (virt_tag_field_id cenv.ilg)) ]

let tag_discriminate cenv cuspec baseTypeSpec cidx = 
    get_tag cenv cuspec baseTypeSpec 
    @ [ mk_ldc_i32 (cidx); 
        I_arith AI_ceq ]

let tag_discriminate_then cenv cuspec baseTypeSpec cidx after = 
    get_tag cenv cuspec baseTypeSpec 
    @ [ mk_ldc_i32 (cidx) ] 
    @ ceq_then after

let rec conv_instr cenv tmps inplab outlab instr = 
    match instr with 
    | I_other e when is_ilx_ext_instr e -> 
        match (dest_ilx_ext_instr e) with 
        |  (EI_newdata (cuspec, cidx)) ->
            let nm,derived_tspec,alt = altinfo_of_cuspec cuspec cidx
            if cuspecRepr.OptimizeAlternativeToNull (cuspec,alt) then 
                Choice1Of2 [ I_arith AI_ldnull ]
            elif cuspecRepr.MaintainPossiblyUniqueConstantFieldForAlternative (cuspec,alt) then 
                let baseTypeSpec = baseTypeSpec_of_cuspec cuspec
                Choice1Of2 [ mk_normal_call (mk_static_nongeneric_mspec_in_boxed_tspec(baseTypeSpec,"get_uniq_"^nm,[],const_formal_field_ty nm baseTypeSpec)) ]
            elif cuspecRepr.OptimizeSingleNonNullaryAlternativeToRootClass (cuspec,alt) then 
                let baseTypeSpec = baseTypeSpec_of_cuspec cuspec
                Choice1Of2 [ mk_normal_newobj(mk_ctor_mspec_for_boxed_tspec (baseTypeSpec,List.map (fun fd -> fd.fdType) (Array.to_list (fdefs_of_alt alt)))) ]
            else 
                Choice1Of2 [ mk_normal_newobj(mk_ctor_mspec_for_boxed_tspec (derived_tspec,List.map (fun fd -> fd.fdType)  (Array.to_list (fdefs_of_alt alt)))) ]

        |  (EI_stdata (cuspec, cidx,fidx)) ->
            let nm,derived_tspec,alt = altinfo_of_cuspec cuspec cidx
            let (nm,ty) = backend_field_id (fdef_of_alt alt fidx)
            Choice1Of2 [ mk_normal_call (mk_nongeneric_instance_mspec_in_boxed_tspec(derived_tspec,"set_"^nm,[ty],Type_void)) ]
              
        |  (EI_lddata (cuspec,cidx,fidx)) ->
            let nm,derived_tspec,alt = altinfo_of_cuspec cuspec cidx
            let field_id = backend_field_id (fdef_of_alt alt fidx) 

            // The only union type to use a mutable field outside its constructor
            // is Microsoft.FSharp.Collections.List`1. The mutation is only used inside FSharp.Core.dll.
            // This type must hide the field behind a getter/setter property pair.
            match derived_tspec.Enclosing with 
            | ["Microsoft.FSharp.Collections.FSharpList`1"] -> 
                let (nm,ty) = field_id
                Choice1Of2 [ mk_normal_call (mk_nongeneric_instance_mspec_in_boxed_tspec(derived_tspec,"get_"^nm,[],ty)) ]
            | _ -> 
                let ILFieldSpec = ref_to_field_in_tspec derived_tspec field_id
                Choice1Of2 [ I_ldfld (Aligned,Nonvolatile, ILFieldSpec) ]
              

        |  (EI_lddatatag cuspec) ->
            let alts = alts_of_cuspec cuspec
            if  not (cuspecRepr.UseRuntimeTypes cuspec) then 
                let baseTypeSpec = baseTypeSpec_of_cuspec cuspec
                Choice1Of2 (get_tag cenv cuspec baseTypeSpec)
            elif List.length alts = 1 then 
                Choice1Of2 [ I_arith AI_pop; I_arith (AI_ldc(DT_I4,NUM_I4(0))) ] 
            else 
                let baseTypeSpec = baseTypeSpec_of_cuspec cuspec
                let locn = alloc_tmp tmps (mk_local (mk_typ AsObject baseTypeSpec))

                let mk_case last inplab cidx fail_lab = 
                    let nm,derived_tspec,alt = altinfo_of_cuspec cuspec cidx
                    let internal_lab = generate_code_label ()
                    let cmp_null = cuspecRepr.OptimizeAlternativeToNull (cuspec,alt)
                    if last then 
                      mk_bblock2 (inplab,[ I_arith (AI_ldc(DT_I4,NUM_I4(cidx))); 
                                          I_br outlab ])   
                    else 
                      let test = I_brcmp ((if cmp_null then BI_brtrue else BI_brfalse),fail_lab,internal_lab)
                      let test_block = 
                        if cmp_null || cuspecRepr.OptimizeSingleNonNullaryAlternativeToRootClass (cuspec,alt) then 
                          [ test ]
                        elif cuspecRepr.UseRuntimeTypes (cuspec) then 
                          rtt_discriminate_then derived_tspec test
                        else 
                          tag_discriminate_then cenv cuspec baseTypeSpec cidx test 
                      mk_group_block 
                        ([internal_lab],
                        [ mk_bblock2 (inplab, I_ldloc locn ::test_block);
                          mk_bblock2 (internal_lab,[I_arith (AI_ldc(DT_I4,NUM_I4(cidx))); I_br outlab ]) ]) 

                // Make the block for the last test. 
                let last_inplab = generate_code_label ()
                let last_block = mk_case true last_inplab 0 outlab

                // Make the blocks for the remaining tests. 
                let _,first_inplab,overall_block = 
                  List.foldBack
                    (fun _ (n,continue_inplab, continue_block) -> 
                      let new_inplab = generate_code_label ()
                      n+1,
                      new_inplab,
                      mk_group_block 
                        ([continue_inplab],
                        [ mk_case false new_inplab n continue_inplab;
                          continue_block ]))
                    (List.tl alts)
                    (1,last_inplab, last_block)

                // Add on a branch to the first input label.  This gets optimized away by the printer/emitter. 
                Choice2Of2 
                  (mk_group_block
                     ([first_inplab],
                     [ mk_bblock2 (inplab, [ I_stloc locn; I_br first_inplab ]);
                   overall_block ]))
                
        |  (EI_castdata (canfail,cuspec,cidx)) ->
            let nm,derived_tspec,alt = altinfo_of_cuspec cuspec cidx
            if cuspecRepr.OptimizeAlternativeToNull (cuspec,alt) then 
              if canfail then 
                  let baseTypeSpec = baseTypeSpec_of_cuspec cuspec
                  let internal1 = generate_code_label ()
                  Choice2Of2 
                    (mk_group_block  
                       ([internal1],
                       [ mk_bblock2 (inplab,
                                    [ I_arith AI_dup;
                                      I_brcmp (BI_brfalse,outlab, internal1) ]);
                         mk_bblock2 (internal1,
                                    [ mk_mscorlib_exn_newobj cenv.ilg "System.InvalidCastException";
                                      I_throw ]);
                       ] ))
              else 
                  // If it can't fail, it's still verifiable just to leave the value on the stack unchecked 
                  Choice1Of2 [] 
                  
            elif cuspecRepr.OptimizeSingleNonNullaryAlternativeToRootClass (cuspec,alt) then 
                Choice1Of2 []

            else Choice1Of2 [ I_castclass (Type_boxed derived_tspec) ] 
              
        |  (EI_brisdata (cuspec,cidx,tg,fail_lab)) ->
            let nm,derived_tspec,alt = altinfo_of_cuspec cuspec cidx
            if cuspecRepr.OptimizeAlternativeToNull (cuspec,alt) then 
                Choice1Of2 [ I_brcmp (BI_brtrue,fail_lab,tg) ] 
            elif cuspecRepr.OptimizeSingleNonNullaryAlternativeToRootClass (cuspec,alt) then 
                Choice1Of2 [ I_brcmp (BI_brfalse,fail_lab,tg) ] 
            elif cuspecRepr.UseRuntimeTypes (cuspec) then 
                Choice1Of2 (rtt_discriminate_then derived_tspec (I_brcmp (BI_brfalse,fail_lab,tg)))
            else 
                let baseTypeSpec = baseTypeSpec_of_cuspec cuspec
                Choice1Of2 (tag_discriminate_then cenv cuspec baseTypeSpec cidx (I_brcmp (BI_brfalse,fail_lab,tg)))

        |  (EI_isdata (cuspec,cidx)) ->
            let nm,derived_tspec,alt = altinfo_of_cuspec cuspec cidx
            if cuspecRepr.OptimizeAlternativeToNull (cuspec,alt) then 
                Choice1Of2 [ I_arith AI_ldnull; I_arith AI_ceq ] 
            elif cuspecRepr.OptimizeSingleNonNullaryAlternativeToRootClass (cuspec,alt) then 
                Choice1Of2 [ I_arith AI_ldnull; I_arith AI_cgt_un ] 
            elif cuspecRepr.UseRuntimeTypes (cuspec) then 
                Choice1Of2 (rtt_discriminate derived_tspec)
            else 
                let baseTypeSpec = baseTypeSpec_of_cuspec cuspec
                Choice1Of2 (tag_discriminate cenv cuspec baseTypeSpec cidx)
              
        |  (EI_datacase (leave_on_stack,cuspec,cases,cont)) ->
            let baseTypeSpec = baseTypeSpec_of_cuspec cuspec
        
            // REVIEW: tag discriminate should map to a switch even for verifiable code 
            // when we leave the result on the stack - this will need more casts inserted 
            if (leave_on_stack) || cuspecRepr.UseRuntimeTypes (cuspec) then 
              let locn = alloc_tmp tmps (mk_local (mk_typ AsObject baseTypeSpec))
              let mk_case last inplab (cidx,tg) fail_lab = 
                let nm,derived_tspec,alt = altinfo_of_cuspec cuspec cidx
                let internal_lab = generate_code_label ()
                let cmp_null = cuspecRepr.OptimizeAlternativeToNull (cuspec,alt)
                let use_one_block = not leave_on_stack
 
                let test = 
                    let test_instr = 
                        if use_one_block then I_brcmp ((if cmp_null then BI_brfalse else BI_brtrue),tg,fail_lab) 
                        else I_brcmp ((if cmp_null then BI_brtrue else BI_brfalse),fail_lab,internal_lab)

                    [ I_ldloc locn ] @
                    (if cmp_null || cuspecRepr.OptimizeSingleNonNullaryAlternativeToRootClass (cuspec,alt) then 
                        [ test_instr ]
                          
                     elif cuspecRepr.UseRuntimeTypes (cuspec) then 
                          rtt_discriminate_then derived_tspec test_instr
                     else 
                          tag_discriminate_then cenv cuspec baseTypeSpec cidx test_instr)

                if use_one_block then mk_bblock2 (inplab, test) 
                else
                  mk_group_block 
                    ([internal_lab],
                    [ mk_bblock2 (inplab, test);
                      mk_bblock2 
                        (internal_lab,
                         (if leave_on_stack then 
                           if not cmp_null then 
                             [ I_ldloc locn; I_castclass (Type_boxed derived_tspec) ]
                           else [ I_ldloc locn ]
                          else []) @
                         [ I_br tg ]) ])
              
              // Make the block for the last test. 
              let last_inplab = generate_code_label ()
              let last_case,first_cases = 
                  let l2 = List.rev cases in 
                  List.hd l2, List.rev (List.tl l2)
              
              let last_block = mk_case true last_inplab last_case cont
              
              // Make the blocks for the remaining tests. 
              let first_inplab,overall_block = 
                List.foldBack
                  (fun case_info (continue_inplab, continue_block) -> 
                    let new_inplab = generate_code_label ()
                    new_inplab,
                    mk_group_block 
                      ([continue_inplab],
                      [ mk_case false new_inplab case_info continue_inplab;
                        continue_block ]))
                  first_cases
                  (last_inplab, last_block)

              // Add on a branch to the first input label.  This gets optimized 
              // away by the printer/emitter. 
              Choice2Of2 
                (mk_group_block
                   ([first_inplab],
                   [ mk_bblock2 (inplab, [ I_stloc locn; I_br first_inplab ]);
                     overall_block ]))
            else 
                let mk_case i _ = 
                    if List.mem_assoc (i) cases then 
                        List.assoc (i) cases 
                    else cont

                let dests = List.mapi mk_case (alts_of_cuspec cuspec)
                Choice1Of2 ((if leave_on_stack then [I_arith AI_dup] else []) @
                            get_tag cenv cuspec baseTypeSpec @ 
                            [ I_switch (dests,cont) ])
                
        | _ -> Choice1Of2 [instr] 

    | _ -> Choice1Of2 [instr] 


let conv_ilmbody cenv il = 
    let tmps = new_tmps (List.length il.ilLocals)
    let code= topcode_instr2code (conv_instr cenv tmps) il.ilCode
    {il with
          ilLocals = il.ilLocals @ get_tmps tmps;
          ilCode=code; 
          ilMaxStack=il.ilMaxStack+2 }

let conv_mdef cenv md  =
    {md with mdBody= mbody_ilmbody2ilmbody (conv_ilmbody cenv) md.mdBody }

let mk_hidden_generated_instance_fdef ilg (nm,ty,init,access) = 
     mk_instance_fdef (nm,ty,init,access)
            |> add_fdef_never_attrs ilg
            |> add_fdef_generated_attrs ilg

let mk_hidden_generated_static_fdef ilg (a,b,c,d,e) = 
     mk_static_fdef (a,b,c,d,e)
            |> add_fdef_never_attrs ilg
            |> add_fdef_generated_attrs ilg

let conv_alternative_def cenv num (td:ILTypeDef) cud info cuspec (baseTypeSpec:ILTypeSpec) alt =
    let attr = cud.cudWhere
    let nm = name_of_alt alt
    let fields = fdefs_of_alt alt
    let altTypeSpec = tspec_of_alt cuspec alt
    let repr = cudefRepr 

    // These attributes get splattered all over the place
    // REVIEW: rethink how and where we can attach custom attributes to discriminated unions  
    let addAltAttribs mdef = { mdef with mdCustomAttrs=alt.altCustomAttrs }

    // The stdata instruction is only ever used for the F# "List" type
    //
    // Microsoft.FSharp.Collections.List`1 is indeed immutable, but we use mutation on this type internally
    // within FSharp.Core.dll
    let isTotallyImmutable = (td.Name <> "Microsoft.FSharp.Collections.FSharpList`1")
    
    let uniqObjMeths = 

         if alt_is_nullary alt && repr.MaintainPossiblyUniqueConstantFieldForAlternative (info,alt) then 
             [ mk_static_nongeneric_mdef
                   ("get_uniq_"^nm,
                    cud.cudReprAccess,[],mk_return(Type_boxed baseTypeSpec),
                    mk_impl(true,[],fields.Length,
                            nonbranching_instrs_to_code 
                              (let fspec = const_fspec nm baseTypeSpec
                               [ I_ldsfld (Nonvolatile,fspec) ]), attr))
                 |> conv_mdef cenv
                 |> add_mdef_generated_attrs cenv.ilg ]
         else
            []

    let helperMeths,helperProps = 

        if cud.cudHelpers then 

            let makerMeths,makerProps = 

                if alt_is_nullary alt then 

                    let nullaryMeth = 
                        mk_static_nongeneric_mdef
                          ("get_"^nm,
                           cud.cudHelpersAccess,[],mk_return(Type_boxed baseTypeSpec),
                           mk_impl(true,[],fields.Length,
                                   nonbranching_instrs_to_code 
                                     ([ (mk_IlxInstr (EI_newdata (cuspec,  num)))]), attr))
                        |> conv_mdef cenv
                        |> add_mdef_generated_attrs cenv.ilg
                        |> addAltAttribs

                    let nullaryProp = 
                         
                        { propName=nm;
                          propRTSpecialName=false;
                          propSpecialName=false;
                          propSet=None;
                          propGet=Some(mk_mref(baseTypeSpec.TypeRef,ILCallingConv.Static,"get_"^nm,0,[],Type_boxed baseTypeSpec));
                          propCallconv=CC_static;
                          propType=Type_boxed baseTypeSpec;          
                          propInit=None;
                          propArgs=[];
                          propCustomAttrs=mk_custom_attrs []; }
                        |> add_pdef_generated_attrs cenv.ilg
                        |> add_pdef_never_attrs cenv.ilg

                    [nullaryMeth],[nullaryProp]
                  
                else
                    let mdef = 
                         mk_static_nongeneric_mdef
                           (nm,
                            cud.cudHelpersAccess,
                            fields |> Array.map (fun fd -> mk_named_param (fd.fdName, fd.fdType)) |> Array.to_list,
                            mk_return(Type_boxed baseTypeSpec),
                            mk_impl(true,[],fields.Length,
                                    nonbranching_instrs_to_code 
                                      (Array.to_list (Array.mapi (fun i _ -> I_ldarg (uint16 i)) fields) @
                                       [ (mk_IlxInstr (EI_newdata (cuspec,  num)))]), attr))
                         |> conv_mdef cenv
                         |> add_mdef_generated_attrs cenv.ilg
                         |> addAltAttribs

                    [mdef],[]

            let testerMeths = 
                if cud.cudAlternatives.Length <= 1 then [] 
                elif repr.OptimizingOneAlternativeToNull info then []
                else
                    [ mk_instance_mdef
                         (tester_name nm,
                          cud.cudHelpersAccess,[],
                          mk_return cenv.ilg.typ_bool,
                          mk_impl(true,[],2,nonbranching_instrs_to_code 
                                    [ ldarg_0;
                                      (mk_IlxInstr (EI_isdata (cuspec, num))) ], attr))
                      |> conv_mdef cenv
                      |> add_mdef_generated_attrs cenv.ilg ]

            let getterMeths = 
                fields 
                |> Array.mapi (fun i fdef -> 
                    mk_instance_mdef
                       ("get_"^getter_prop_name fields i,
                        cud.cudHelpersAccess,[],
                        mk_return fdef.fdType,
                        mk_impl(true,[],2,
                                nonbranching_instrs_to_code 
                                  [ ldarg_0;
                                    (mk_IlxInstr (EI_castdata (true,cuspec, num)));
                                    (mk_IlxInstr (EI_lddata (cuspec, num, i)))],attr))
                    |> conv_mdef cenv
                    |> add_mdef_generated_attrs cenv.ilg) 
                |> Array.to_list    

            let getterProps =
                fields 
                |> Array.mapi (fun i fdef -> 
                    { propName=getter_prop_name fields i;
                      propRTSpecialName=false;
                      propSpecialName=false;
                      propSet=None;
                      propGet=Some(mk_mref(baseTypeSpec.TypeRef,ILCallingConv.Instance,"get_"^getter_prop_name fields i,0,[],fdef.fdType));
                      propCallconv=CC_instance;
                      propType=fdef.fdType;          
                      propInit=None;
                      propArgs=[];
                      propCustomAttrs= fdef.fdCustomAttrs; }
                    |> add_pdef_generated_attrs cenv.ilg
                    //|> add_pdef_never_attrs cenv.ilg
                )
                |> Array.to_list

            testerMeths @ getterMeths @ makerMeths, getterProps @ makerProps

        else 
            [],[] 

    let typeDefs,debugTypeDefs,nullaryFields = 
      if repr.OptimizeAlternativeToNull (info,alt) then [], [], []
      elif repr.OptimizeSingleNonNullaryAlternativeToRootClass (info,alt) then [], [], []
      else 
          let nullaryFields = 
              if repr.MaintainPossiblyUniqueConstantFieldForAlternative(info,alt) then 
                  let basic = mk_hidden_generated_static_fdef cenv.ilg (const_fname nm, Type_boxed baseTypeSpec, None, None, MemAccess_assembly)
                  let uniqObjField = { basic with fdInitOnly=true }
                  let inRootClass = cuspecRepr.OptimizeAlternativeToRootClass (cuspec,alt)
            
                  [ (info,alt, altTypeSpec,num,uniqObjField,inRootClass) ] 
              else 
                  []

          let typeDefs,debugTypeDefs = 
              if repr.OptimizeAllAlternativesToConstantFieldsInRootClass info then [],[]
              elif repr.OptimizeAlternativeToConstantFieldInTaggedRootClass (info,alt) then [],[]
              else
                
                let debugTypeDefs, debugAttrs = 
                    if not cud.cudDebugProxies then  [],  []
                    else
                      
                      let debugProxyTypeName = altTypeSpec.Name^"@DebugTypeProxy"
                      let debugProxyTypeSpec = mk_tspec(mk_nested_tref(altTypeSpec.Scope,altTypeSpec.Enclosing, debugProxyTypeName),altTypeSpec.GenericArgs)
                      let debugProxyFieldName = "_obj"
                      
                      let debugProxyFields = 
                          [ mk_hidden_generated_instance_fdef cenv.ilg (debugProxyFieldName,Type_boxed altTypeSpec, None, MemAccess_private) ]

                      let debugProxyCtor = 
                          mk_ctor(MemAccess_public (* must always be public - see jared parson blog entry on implementing debugger type proxy *),
                                  [ mk_named_param ("obj",Type_boxed altTypeSpec) ],
                                  mk_impl
                                    (false,[],3,
                                     nonbranching_instrs_to_code
                                       [ yield ldarg_0 
                                         yield mk_normal_call (mk_ctor_mspec_for_boxed_tspec (cenv.ilg.tspec_Object,[]))  
                                         yield ldarg_0 
                                         yield I_ldarg 1us;
                                         yield mk_normal_stfld (mk_fspec_in_boxed_tspec (debugProxyTypeSpec,debugProxyFieldName,Type_boxed altTypeSpec)); ],None))

                          |> add_mdef_generated_attrs cenv.ilg

                      let debugProxyGetterMeths = 
                          fields 
                          |> Array.mapi (fun i field -> 
                              let nm,ty = backend_field_id field
                              mk_instance_mdef
                                 ("get_"^getter_prop_name fields i,
                                  MemAccess_public,[],
                                  mk_return field.fdType,
                                  mk_impl(true,[],2,
                                          nonbranching_instrs_to_code 
                                            [ ldarg_0;
                                              mk_normal_ldfld (mk_fspec_in_boxed_tspec (debugProxyTypeSpec,debugProxyFieldName,Type_boxed altTypeSpec)); 
                                              mk_normal_ldfld (mk_fspec_in_typ(Type_boxed altTypeSpec,nm,ty));],None))
                              |> conv_mdef cenv
                              |> add_mdef_generated_attrs cenv.ilg)
                          |> Array.to_list

                      let debugProxyGetterProps =
                          fields 
                          |> Array.mapi (fun i fdef -> 
                              { propName=getter_prop_name fields i;
                                propRTSpecialName=false;
                                propSpecialName=false;
                                propSet=None;
                                propGet=Some(mk_mref(debugProxyTypeSpec.TypeRef,ILCallingConv.Instance,"get_"^getter_prop_name fields i,0,[],fdef.fdType));
                                propCallconv=CC_instance;
                                propType=fdef.fdType;          
                                propInit=None;
                                propArgs=[];
                                propCustomAttrs= fdef.fdCustomAttrs; }
                              |> add_pdef_generated_attrs cenv.ilg)
                          |> Array.to_list
                      let debugProxyTypeDef = 
                          mk_generic_class (debugProxyTypeName, 
                                            TypeAccess_nested MemAccess_private, 
                                            td.tdGenericParams, 
                                            cenv.ilg.typ_Object, [], 
                                            mk_mdefs ([debugProxyCtor] @ debugProxyGetterMeths), 
                                            mk_fdefs debugProxyFields,
                                            mk_properties debugProxyGetterProps,
                                            mk_events [],
                                            mk_custom_attrs [])
                      [ { debugProxyTypeDef with tdSpecialName=true } ],
                      ( [mk_DebuggerTypeProxyAttribute cenv.ilg (Type_boxed debugProxyTypeSpec)] @ cud.cudDebugDisplayAttributes)
                                      
                let basicTypeDef = 
                    let basicFields = 
                        fields 
                        |> Array.map (fun field -> 
                            let nm,ty = backend_field_id field
                            let fdef = mk_hidden_generated_instance_fdef cenv.ilg (nm,ty, None, (if isTotallyImmutable then cud.cudHelpersAccess else MemAccess_assembly))
                            { fdef with fdInitOnly=isTotallyImmutable })
                        |> Array.to_list

                    let virtTagMeths = []

                    let setterMethods = 

                        // The stdata instruction is only ever used for the F# "List" type
                        if not isTotallyImmutable then 
                           [ for field in fields do 
                                let nm,ty = backend_field_id field
                                yield
                                    mk_instance_mdef
                                         ("set_"^nm,
                                          MemAccess_assembly,[mk_unnamed_param ty],mk_return(Type_void),
                                          mk_impl(true,[],2,
                                                  nonbranching_instrs_to_code 
                                                    (let fspec = mk_fspec_in_boxed_tspec(altTypeSpec,nm,ty)
                                                     [ I_ldarg 0us;
                                                       I_ldarg 1us;
                                                       mk_normal_stfld fspec ]), attr))
                                    |> conv_mdef cenv
                                    |> add_mdef_generated_attrs cenv.ilg
                                yield 
                                    mk_instance_mdef
                                       ("get_"^nm,
                                        cud.cudReprAccess,[],mk_return(ty),
                                        mk_impl(true,[],2,
                                                nonbranching_instrs_to_code 
                                                  (let fspec = mk_fspec_in_boxed_tspec(altTypeSpec,nm,ty)
                                                   [ I_ldarg 0us;
                                                     mk_normal_ldfld fspec ]), attr))
                                    |> conv_mdef cenv
                                    |> add_mdef_generated_attrs cenv.ilg ]
                        else
                           []

                    
                    let basicCtorMeth = 
                        mk_storage_ctor 
                           (attr  ,
                            [ yield ldarg_0 
                              if  not (repr.UseRuntimeTypes info) then 
                                  yield I_arith (AI_ldc(DT_I4,NUM_I4(num)))
                                  yield mk_normal_call (mk_ctor_mspec_for_boxed_tspec (baseTypeSpec,[cenv.ilg.typ_int32])) 
                              else 
                                  yield mk_normal_call (mk_ctor_mspec_for_boxed_tspec (baseTypeSpec,[])) ],
                            altTypeSpec,
                            (basicFields |> List.map (fun fdef -> fdef.fdName, fdef.fdType) ),
                            cud.cudReprAccess)
                        |> add_mdef_generated_attrs cenv.ilg

                    let tdef = 
                        mk_generic_class (altTypeSpec.Name, 
                                          TypeAccess_nested cud.cudReprAccess, 
                                          td.tdGenericParams, 
                                          Type_boxed baseTypeSpec, [], 
                                          mk_mdefs (virtTagMeths @ [basicCtorMeth] @ setterMethods), 
                                          mk_fdefs basicFields,
                                          mk_properties [],
                                          mk_events [],
                                          mk_custom_attrs debugAttrs)
                    { tdef with tdSerializable=td.tdSerializable; 
                                tdSpecialName=true }

                [ basicTypeDef ], debugTypeDefs 


          typeDefs,debugTypeDefs,nullaryFields

    (uniqObjMeths@helperMeths),helperProps,typeDefs,debugTypeDefs,nullaryFields
        
  
let rec conv_cudef cenv enc td cud = 
    let baseTypeSpec = tspec_for_nested_tdef ScopeRef_local (enc,td)
    let cuspec = IlxUnionSpec(IlxUnionRef(baseTypeSpec.TypeRef,cud.cudAlternatives,cud.cudNullPermitted), baseTypeSpec.GenericArgs)
    let info = (enc,td,cud)
    let repr = cudefRepr 

    let _,aux_meths,aux_props,altTypeDefs,debugTypeDefs,nullaryFields = 
        Array.fold_left 
          (fun (num,msofar,psofar,csofar,dsofar,fsofar) alt -> 
            let ms,ps,cls,dcls,flds = conv_alternative_def cenv num td cud info cuspec baseTypeSpec alt
            (num+1,msofar@ms, psofar@ps,csofar@cls,dsofar@dcls,fsofar@flds)) 
          (0,[],[],[],[],[])
          cud.cudAlternatives
       
    let selfFields,selfCtorMeths,selfNum = 
        match  cud.cudAlternatives |> Array.to_list |> List.findi 0 (fun alt -> repr.OptimizeSingleNonNullaryAlternativeToRootClass (info,alt))  with 
        | Some (alt,altNum) ->
            let fields = alt |> fdefs_of_alt |> Array.to_list |> List.map backend_field_id 
            let ctor = 
                mk_simple_storage_ctor 
                   (cud.cudWhere,
                    (match td.tdExtends with None -> Some cenv.ilg.tspec_Object | Some typ -> Some (tspec_of_typ typ)),
                    baseTypeSpec,
                    fields,
                    cud.cudReprAccess)
                |> add_mdef_generated_attrs cenv.ilg
                
            fields,[ctor],altNum

        |  None ->
            [],[],0

    let virttagFields = 
        if repr.UseRuntimeTypes info then  []
        else [ virt_tag_field_id cenv.ilg ] 

    let selfAndVirtTagFields = 
        (selfFields @ virttagFields) 
        |> List.map (fun (nm,ty)-> 
            mk_hidden_generated_instance_fdef cenv.ilg (nm,ty, None, cud.cudReprAccess))

    let virtTagMeths = []

    let ctorMeths =
      if (isNil selfFields && isNil virttagFields && nonNil selfCtorMeths)
          ||  cud.cudAlternatives |> Array.forall (fun alt -> repr.OptimizeSingleNonNullaryAlternativeToRootClass (info,alt))  then 

          [] (* no need for a second ctor in these cases *)

      else 
          [ mk_simple_storage_ctor 
               (cud.cudWhere,
                (match td.tdExtends with None -> Some cenv.ilg.tspec_Object | Some typ -> Some (tspec_of_typ typ)),
                baseTypeSpec,
                virttagFields,
                cud.cudReprAccess)
            |> add_mdef_generated_attrs cenv.ilg ]

    // The following two are for the case where we're using virtual tags, and we 
    // need to generate a class for the nullary constructors we're optimizing to 
    // constant values.  These constants each carry a field giving their appropriate tag. 
    // i.e. we don't really use virtual dispatch to save space on these. 
    let nullary_tspec = baseTypeSpec
    let nullaryTypeDefs = []

    // Now initialize the constant fields wherever they are stored... 
    let add_const_field_init cd = 
      if isNil nullaryFields then 
         cd 
      else 
         prepend_instrs_to_cctor 
            [ for (info,alt,altTypeSpec,fidx,fd,inRootClass) in nullaryFields do 
                let const_fid = (fd.fdName,Type_boxed baseTypeSpec)
                let const_fspec = ref_to_field_in_tspec baseTypeSpec const_fid
                if repr.UseRuntimeTypes info then 
                    yield mk_normal_newobj (mk_ctor_mspec_for_boxed_tspec (altTypeSpec,[])); 
                elif inRootClass then
                    yield I_arith (AI_ldc(DT_I4,NUM_I4(fidx)));  
                    yield  mk_normal_newobj (mk_ctor_mspec_for_boxed_tspec (altTypeSpec,[cenv.ilg.typ_int32] ))
                else
                    yield mk_normal_newobj (mk_ctor_mspec_for_boxed_tspec (altTypeSpec,[])); 
                yield mk_normal_stsfld const_fspec ]
            cud.cudWhere
            cd

    let tagMeths, tagProps, tagFields = 
        let tagFields = 
           cud.cudAlternatives |> Array.mapi (fun num alt ->
               let fdef = mk_hidden_generated_static_fdef cenv.ilg ("tag_"^name_of_alt alt,cenv.ilg.typ_int32,Some(FieldInit_int32(num)),None,cud.cudHelpersAccess)
               {fdef with fdLiteral = true})
           |> Array.to_list

        let tagMeths,tagProps = 

          // If we are using NULL as a representation for an element of this type then we cannot 
          // use an instance method 
          if (repr.OptimizingOneAlternativeToNull info) then
              [ mk_static_nongeneric_mdef
                    ("Get"^tagPropertyName,
                     cud.cudHelpersAccess,
                     [mk_unnamed_param (Type_boxed baseTypeSpec)],
                     mk_return (cenv.ilg.typ_int32),
                     mk_impl(true,[],2,
                             nonbranching_instrs_to_code 
                                 [ ldarg_0;
                                   (mk_IlxInstr (EI_lddatatag cuspec)) ], 
                             cud.cudWhere))
                |> conv_mdef cenv 
                |> add_mdef_generated_attrs cenv.ilg ], 
              [] 

          else
              [ mk_instance_mdef
                    ("get_"^tagPropertyName,
                     cud.cudHelpersAccess,[],
                     mk_return cenv.ilg.typ_int32,
                     mk_impl(true,[],2,
                             nonbranching_instrs_to_code 
                                 [ ldarg_0;
                                   (mk_IlxInstr (EI_lddatatag cuspec)) ], 
                             cud.cudWhere)) 
                |> conv_mdef cenv
                |> add_mdef_generated_attrs cenv.ilg ], 

              [ { propName=tagPropertyName;
                  propRTSpecialName=false;
                  propSpecialName=false;
                  propSet=None;
                  propGet=Some(mk_mref(baseTypeSpec.TypeRef,ILCallingConv.Instance,"get_"^tagPropertyName,0,[],cenv.ilg.typ_int32));
                  propCallconv=CC_instance;
                  propType=cenv.ilg.typ_int32;          
                  propInit=None;
                  propArgs=[];
                  propCustomAttrs=mk_custom_attrs []; }
                |> add_pdef_generated_attrs cenv.ilg 
                |> add_pdef_never_attrs cenv.ilg  ]

        tagMeths,tagProps,tagFields

    // The class can be abstract if each alternative is represented by a derived type
    let isAbstract = (altTypeDefs.Length = cud.cudAlternatives.Length)        

    // If the class is abstract make the constructor used for the subclasses protected
    let ctorMeths = 
        if isAbstract then 
            ctorMeths |> List.map (fun mdef -> {mdef with mdAccess=MemAccess_famandassem })
        else   
            ctorMeths

    let baseTypeDef = 
        { tdName = td.tdName;
          tdNested = mk_tdefs (nullaryTypeDefs @ altTypeDefs @ debugTypeDefs @ dest_tdefs (conv_tdefs cenv (enc@[td]) td.tdNested));
          tdGenericParams= td.tdGenericParams;
          tdAccess = td.tdAccess;
          tdAbstract = isAbstract;
          tdSealed = false;
          tdSerializable=td.tdSerializable;
          tdComInterop=false;
          tdLayout=td.tdLayout; 
          tdSpecialName=td.tdSpecialName;
          tdEncoding=td.tdEncoding ;
          tdImplements = td.tdImplements;
          tdExtends= (match td.tdExtends with None -> Some cenv.ilg.typ_Object | _ -> td.tdExtends) ;
          tdMethodDefs= mk_mdefs (tagMeths @ virtTagMeths @ ctorMeths @ selfCtorMeths @ aux_meths @ List.map (conv_mdef cenv) (dest_mdefs td.tdMethodDefs));
          tdSecurityDecls=td.tdSecurityDecls;
          
          tdHasSecurity=td.tdHasSecurity; 
          tdFieldDefs=mk_fdefs (List.map (fun (_,_,_,_,fdef,_) -> fdef) nullaryFields @ 
                                selfAndVirtTagFields @ 
                                tagFields @ 
                                dest_fdefs td.tdFieldDefs);
          tdMethodImpls=td.tdMethodImpls;
          tdInitSemantics=TypeInit_beforefield;
          tdEvents=td.tdEvents;
          tdProperties=mk_properties (tagProps @ aux_props @ dest_pdefs td.tdProperties);
          tdCustomAttrs=td.tdCustomAttrs;
          tdKind = TypeDef_class; }

    let baseTypeDef' = add_const_field_init baseTypeDef
    baseTypeDef'


and conv_tdef cenv enc td = 
    match td.tdKind with 
    | TypeDef_other e when is_ilx_ext_type_def_kind e -> 
        begin match dest_ilx_ext_type_def_kind e with 
        | ETypeDef_closure cloinfo -> 
            {td with tdNested = conv_tdefs cenv (enc@[td]) td.tdNested;
                     tdMethodDefs=mdefs_mdef2mdef (conv_mdef cenv) td.tdMethodDefs;
                     tdKind= mk_IlxTypeDefKind(ETypeDef_closure (cloinfo_ilmbody2ilmbody (conv_ilmbody cenv) cloinfo)) }
        | ETypeDef_classunion cud -> conv_cudef cenv enc td cud
        end
    | _ -> 
      {td with tdNested = conv_tdefs cenv (enc@[td]) td.tdNested;
               tdMethodDefs=mdefs_mdef2mdef (conv_mdef cenv) td.tdMethodDefs; }

and conv_tdefs cenv enc tdefs = 
    tdefs_tdef2tdef (conv_tdef cenv enc) tdefs

let ConvModule ilg modul = 
    let cenv = { ilg=ilg; }
    module_tdefs2tdefs (conv_tdefs cenv []) modul


       
