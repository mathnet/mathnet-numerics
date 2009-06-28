// (c) Microsoft Corporation. All rights reserved

//----------------------------------------------------------------------------
// Write Abstract IL structures at runtime using Reflection.Emit
//----------------------------------------------------------------------------

#light

module Microsoft.FSharp.Compiler.AbstractIL.RuntimeWriter    
  
open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types
open Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 
open Microsoft.FSharp.Compiler.AbstractIL.IL

open Microsoft.FSharp.Text.Printf

open System
open System.Reflection
open System.Reflection.Emit
open System.Runtime.InteropServices
open System.Collections.Generic

let codeLabelOrder = compare : ILCodeLabel -> ILCodeLabel -> int

let verbose = false

//----------------------------------------------------------------------------
// misc
//----------------------------------------------------------------------------

let inline flagsIf  b x  = if b then x else enum 0

module Zmap = 
    let force x m str = match Zmap.tryfind x m with Some y -> y | None -> failwithf "Zmap.force: %s: x = %A" str x

let equalTypes (s:Type) (t:Type) = s.Equals(t)
let equalTypeLists ss tt =  List.lengthsEqAndForall2 equalTypes ss tt

let getGenericArgumentsOfType (typT : Type) = 
    if typT .IsGenericType   then typT .GetGenericArguments() else [| |]
let getGenericArgumentsOfMethod (methI : MethodInfo) = 
    if methI.IsGenericMethod then methI.GetGenericArguments() else [| |] 

let getTypeConstructor (ty: Type) = 
    if ty.IsGenericType then ty.GetGenericTypeDefinition() else ty

//----------------------------------------------------------------------------
// convAssemblyRef
//----------------------------------------------------------------------------

let convAssemblyRef (aref:ILAssemblyRef) = 
    let asmName = new System.Reflection.AssemblyName()
    asmName.Name    <- aref.Name;
    (match aref.PublicKey with 
     | None -> ()
     | Some (PublicKey      bytes) -> asmName.SetPublicKey(bytes)
     | Some (PublicKeyToken bytes) -> asmName.SetPublicKeyToken(bytes));
    let setVersion (major,minor,build,rev) = 
       asmName.Version <- System.Version (int32 major,int32 minor,int32 build, int32 rev)
    Option.iter setVersion aref.Version;
    //  asmName.ProcessorArchitecture <- System.Reflection.ProcessorArchitecture.MSIL;
    //Option.iter (fun name -> asmName.CultureInfo <- System.Globalization.CultureInfo.CreateSpecificCulture(name)) aref.Locale;
    asmName.CultureInfo <- System.Globalization.CultureInfo.InvariantCulture;
    asmName


/// Convert an Abstract IL type reference to Reflection.Emit System.Type value
// REVIEW: This ought to be an adequate substitute for this whole function, but it needs 
// to be thoroughly tested.
//    Type.GetType(tref.QualifiedName) 
// []              ,name -> name
// [ns]            ,name -> ns+name
// [ns;typeA;typeB],name -> ns+typeA+typeB+name
let getTRefType (tref:ILTypeRef) = 
    if verbose then dprintf "- getTRefType: %s\n" tref.Name;

    let prefix = 
        match tref.Enclosing with
        | []             -> ""
        | enclosingNames -> String.concat "+" enclosingNames ^ "+"
    let qualifiedName = prefix ^ tref.Name (* e.g. Name.Space.Class+NestedClass, assembly *)
    if verbose then dprintf "- - qualifiedName = %s\n" qualifiedName;    
    match tref.Scope with
    | ScopeRef_assembly asmref ->
        let asmName    = convAssemblyRef asmref
        let currentDom = System.AppDomain.CurrentDomain
        let assembly   = currentDom.Load(asmName)
        if verbose then dprintf "- - assembly = %s\n" (assembly.ToString());
        let nm = prefix ^ tref.Name 
        let typT       = assembly.GetType(nm)
        typT |> nonNull "GetTRefType" // (Printf.sprintf "GetTRefType (assembly %O, ty = %s)" assembly nm)
    | ScopeRef_module _ 
    | ScopeRef_local _ ->
        if verbose then dprintf "- - module/local\n";
        let typT = Type.GetType(qualifiedName,true) 
        if verbose then dprintf "- getTRefType: Got %d\n" (if isNonNull typT then 1 else 0);
        typT |> nonNull "GetTRefType" // (Printf.sprintf "GetTRefType (local %s)" qualifiedName)

/// The global environment
type cenv = 
    { ilg: ILGlobals; generate_pdb: bool }

/// The (local) emitter env (state). Some of these fields are effectively global accumulators
/// and could be placed as hash tables in the global environment.
[<AutoSerializable(false)>]
type emEnv =
    { emTypMap   : Zmap.map<ILTypeRef,Type * TypeBuilder * ILTypeDef * Type option (*the created type*) > ;
      emConsMap  : Zmap.map<ILMethodRef,ConstructorBuilder>;    
      emMethMap  : Zmap.map<ILMethodRef,MethodBuilder>;
      emFieldMap : Zmap.map<ILFieldRef,FieldBuilder>;
      emPropMap  : Zmap.map<ILPropertyRef,PropertyBuilder>;
      emLocals   : LocalBuilder array;
      emLabels   : Zmap.map<IL.ILCodeLabel,Label>;
      emTyvars   : Type array list; // stack
      emEntryPts : (TypeBuilder * string) list }
  
let type_ref_order      = compare : ILTypeRef      -> ILTypeRef      -> int
let method_ref_order    = compare : ILMethodRef    -> ILMethodRef    -> int
let field_ref_order     = compare : ILFieldRef     -> ILFieldRef     -> int
let property_ref_order  = compare : ILPropertyRef  -> ILPropertyRef  -> int

let emEnv0 = 
    { emTypMap   = Zmap.empty type_ref_order;
      emConsMap  = Zmap.empty method_ref_order;
      emMethMap  = Zmap.empty method_ref_order;
      emFieldMap = Zmap.empty field_ref_order;
      emPropMap = Zmap.empty property_ref_order;
      emLocals   = [| |];
      emLabels   = Zmap.empty codeLabelOrder;
      emTyvars   = [];
      emEntryPts = []; }

let envBindTypeRef emEnv (tref:ILTypeRef) (typT,typB,typeDef)= 
    if verbose then dprintf "- envBindTypeRef: %s\n" tref.Name;
    match typT with 
    | null -> failwithf "binding null type in envBindTypeRef: %s\n" tref.Name;
    | _ -> {emEnv with emTypMap = Zmap.add tref (typT,typB,typeDef,None) emEnv.emTypMap}

let envUpdateCreatedTypeRef emEnv (tref:ILTypeRef) =
    // The tref's TypeBuilder has been created, so we have a Type proper.
    // Update the tables to include this created type (the typT held prior to this is (i think) actually (TypeBuilder :> Type).
    // The (TypeBuilder :> Type) does not implement all the methods that a Type proper does.
    if verbose then dprintf "- envBindTypeRef: %s\n" tref.Name;
    let typT,typB,typeDef,createdTypOpt = Zmap.force tref emEnv.emTypMap "envGetTypeDef: failed"
    if typB.IsCreated() then
        {emEnv with emTypMap = Zmap.add tref (typT,typB,typeDef,Some (typB.CreateType())) emEnv.emTypMap}
    else
#if DEBUG
        printf "envUpdateCreatedTypeRef: expected type to be created\n";
#endif
        emEnv

let envGetTypT emEnv preferCreated (tref:ILTypeRef) = 
    if verbose then dprintf "- envGetTypT: %s\n" tref.Name;
    match Zmap.tryfind tref emEnv.emTypMap with
    | Some (typT,typB,typeDef,Some createdTyp) when preferCreated -> createdTyp |> nonNull "envGetTypT: null create type table?"
    | Some (typT,typB,typeDef,_)                                  -> typT       |> nonNull "envGetTypT: null type table?"
    | None                                                        -> getTRefType tref

let envBindConsRef emEnv (mref:ILMethodRef) consB = 
    if verbose then dprintf "- envBindConsRef: %s\n" mref.Name;
    {emEnv with emConsMap = Zmap.add mref consB emEnv.emConsMap}

let envGetConsB emEnv (mref:ILMethodRef) = 
    if verbose then dprintf "- envGetConsB: %s\n" mref.Name;
    Zmap.force mref emEnv.emConsMap "envGetConsB: failed"

let envBindMethodRef emEnv (mref:ILMethodRef) methB = 
    if verbose then dprintf "- envBindMethodRef: %s\n" mref.Name;
    {emEnv with emMethMap = Zmap.add mref methB emEnv.emMethMap}

let envGetMethB emEnv (mref:ILMethodRef) = 
    if verbose then dprintf "- envGetMethB: %s\n" mref.Name;
    Zmap.force mref emEnv.emMethMap "envGetMethB: failed"

let envBindFieldRef emEnv fref fieldB = 
    if verbose then dprintf "- envBindFieldRef: %s\n" fref.frefName;
    {emEnv with emFieldMap = Zmap.add fref fieldB emEnv.emFieldMap}

let envGetFieldB emEnv fref =
    Zmap.force fref emEnv.emFieldMap "- envGetMethB: failed"
      
let envBindPropRef emEnv (pref:ILPropertyRef) propB = 
    if verbose then dprintf "- envBindPropRef: %s\n" pref.Name;
    {emEnv with emPropMap = Zmap.add pref propB emEnv.emPropMap}

let envGetPropB emEnv pref =
    Zmap.force pref emEnv.emPropMap "- envGetPropB: failed"
      
let envGetTypB emEnv (tref:ILTypeRef) = 
    if verbose then dprintf "- envGetTypB: %s\n" tref.Name; 
    Zmap.force tref emEnv.emTypMap "envGetTypB: failed"
    |> (fun (typT,typB,typeDef,createdTypOpt) -> typB)
                 
let envGetTypeDef emEnv (tref:ILTypeRef) = 
    if verbose then dprintf "- envGetTypeDef: %s\n" tref.Name; 
    Zmap.force tref emEnv.emTypMap "envGetTypeDef: failed"
    |> (fun (typT,typB,typeDef,createdTypOpt) -> typeDef)
                 
let envSetLocals emEnv locs = assert (emEnv.emLocals.Length = 0); // check "locals" is not yet set (scopes once only)
                              {emEnv with emLocals = locs}
let envGetLocal  emEnv i    = emEnv.emLocals.[i] // implicit bounds checking

let envSetLabel emEnv name lab =
    assert (not (Zmap.mem name emEnv.emLabels));
    {emEnv with emLabels = Zmap.add name lab emEnv.emLabels}
    
let envGetLabel emEnv name = 
    //if verbose then dprintf "- envGetLabel: %s\n" name;
    Zmap.find name emEnv.emLabels

let envPushTyvars emEnv typs =  {emEnv with emTyvars = typs :: emEnv.emTyvars}
let envPopTyvars  emEnv      =  {emEnv with emTyvars = List.tl emEnv.emTyvars}
let envGetTyvar   emEnv u16  =  
    match emEnv.emTyvars with
    | []     -> failwith "envGetTyvar: not scope of type vars"
    | tvs::_ -> let i = int32 u16 
                if i<0 || i>= Array.length tvs then
                    failwith (Printf.sprintf "want tyvar #%d, but only had %d tyvars" i (Array.length tvs))
                else
                    (if verbose then dprintf "envGetTyvar %d\n" i;
                     tvs.[i])

let isEmittedTypeRef emEnv tref = Zmap.mem tref emEnv.emTypMap

let envAddEntryPt  emEnv mref = {emEnv with emEntryPts = mref::emEnv.emEntryPts}
let envPopEntryPts emEnv      = {emEnv with emEntryPts = []},emEnv.emEntryPts

//----------------------------------------------------------------------------
// convCallConv
//----------------------------------------------------------------------------

let convCallConv (Callconv (hasThis,basic)) =
    if verbose then dprintf "- convCallconv\n";  
    let ccA = match hasThis with CC_static            -> CallingConventions.Standard
                               | CC_instance_explicit -> CallingConventions.ExplicitThis
                               | CC_instance          -> CallingConventions.HasThis
    let ccB = match basic with   CC_default  -> enum 0
                               | CC_cdecl    -> enum 0
                               | CC_stdcall  -> enum 0
                               | CC_thiscall -> enum 0 // XXX: check all these
                               | CC_fastcall -> enum 0
                               | CC_vararg   -> CallingConventions.VarArgs
    ccA ||| ccB


//----------------------------------------------------------------------------
// convType
//----------------------------------------------------------------------------

let rec convTypeSpec emEnv preferCreated (tspec:ILTypeSpec) =
    if verbose then dprintf "- convTypeSpec: %s\n" tspec.Name;
    let typT   = envGetTypT emEnv preferCreated tspec.TypeRef
    let tyargs = List.map (convTypeAux emEnv preferCreated) tspec.GenericArgs
    match tyargs,typT.IsGenericType with
    | tyargs,true  -> typT.MakeGenericType(Array.of_list tyargs)    |> nonNull "convTypeSpec: generic" 
    | []    ,false -> typT                                          |> nonNull "convTypeSpec: non generic" 
    | h::_  ,false -> failwithf "- convTypeSpec: non-generic type '%O' has type instance of length %d and head %O?" typT (List.length tyargs) h
      
and convTypeAux emEnv preferCreated typ =
    match typ with
    | Type_void               -> Type.GetType("System.Void",true)
    | Type_array (shape,eltType) -> 
        let baseT = convTypeAux emEnv preferCreated eltType |> nonNull "convType: array base"
        let nDims = shape.Rank
        // MakeArrayType()  returns "eltType[]"
        // MakeArrayType(1) returns "eltType[*]"
        // MakeArrayType(2) returns "eltType[,]"
        // MakeArrayType(3) returns "eltType[,,]"
        // All non-equal.
        if nDims=1
        then baseT.MakeArrayType() 
        else baseT.MakeArrayType shape.Rank
    | Type_value tspec        -> convTypeSpec emEnv preferCreated tspec              |> nonNull "convType: value"
    | Type_boxed tspec        -> convTypeSpec emEnv preferCreated tspec              |> nonNull "convType: boxed"
    | Type_ptr eltType        -> let baseT = convTypeAux emEnv preferCreated eltType |> nonNull "convType: ptr eltType"
                                 baseT.MakePointerType()                             |> nonNull "convType: ptr" 
    | Type_byref eltType      -> let baseT = convTypeAux emEnv preferCreated eltType |> nonNull "convType: byref eltType"
                                 baseT.MakeByRefType()                               |> nonNull "convType: byref" 
    | Type_tyvar tv           -> envGetTyvar emEnv tv                                |> nonNull "convType: tyvar" 
  // XXX: REVIEW: complete the following cases.                                                        
    | Type_fptr callsig -> failwith "convType: fptr"
    | Type_modified _   -> failwith "convType: modified"

// [Bug 4063].
// The convType functions convert AbsIL types into concrete Type values.
// The emitted types have (TypeBuilder:>Type) and (TypeBuilderInstantiation:>Type).
// These can be used to construct the concrete Type for a given AbsIL type.
// This is the convType function.
// Certain functions here, e.g. convMethodRef, convConstructorSpec assume they get the "Builders" for emitted types.
//
// The "lookupType" function (see end of file) provides AbsIL to Type lookup (post emit).
// The external use (reflection and pretty printing) requires the created Type (rather than the builder).
// convCreatedType ensures created types are used where possible.
// Note: typeBuilder.CreateType() freezes the type and makes a proper Type for the collected information.
//------  
// REVIEW: "convType becomes convCreatedType", the functions could be combined.
// If convCreatedType replaced convType functions like convMethodRef, convConstructorSpec, ... (and more?)
// will need to be fixed for emitted types to handle both TypeBuilder and later Type proper.
  
/// Uses TypeBuilder/TypeBuilderInstantiation for emitted types
let convType        emEnv typ = convTypeAux emEnv false typ

/// Uses the .CreateType() for emitted type (if available)
let convCreatedType emEnv typ = convTypeAux emEnv true  typ 
  

//----------------------------------------------------------------------------
// buildGenParams
//----------------------------------------------------------------------------

let buildGenParamsPass1 emEnv defineGenericParameters (gps : ILGenericParameterDefs) = 
    if verbose then dprintf "buildGenParamsPass1\n"; 
    match gps with 
    | [] -> () 
    | gps ->
        let gpsNames = List.map (fun gp -> gp.gpName) gps
        defineGenericParameters (Array.of_list gpsNames)  |> ignore


let buildGenParamsPass1b emEnv (genArgs : Type array) (gps : ILGenericParameterDefs) = 
    if verbose then dprintf "buildGenParamsPass1b\n"; 
    let genpBs =  genArgs |>  Array.map (fun x -> (x :?> GenericTypeParameterBuilder)) 
    gps |> List.iteri (fun i (gp:ILGenericParameterDef) ->
        let gpB = genpBs.[i]
        // the Constraints are either the parent (base) type or interfaces.
        let constraintTs = List.map (convType emEnv) gp.Constraints
        let interfaceTs,baseTs = List.partition (fun (typ:System.Type) -> typ.IsInterface) constraintTs
        // set base type constraint
        (match baseTs with
            []      -> () // Q: should a baseType be set? It is in some samples. Should this be a failure case?
          | [baseT] -> gpB.SetBaseTypeConstraint(baseT)
          | _       -> failwith "buildGenParam: multiple base types"
        );
        // set interface contraints (interfaces that instances of gp must meet)
        gpB.SetInterfaceConstraints(Array.of_list interfaceTs);

        let flags = GenericParameterAttributes.None 
        let flags =
           match gp.gpVariance with
           | NonVariant    -> flags
           | CoVariant     -> flags ||| GenericParameterAttributes.Covariant
           | ContraVariant -> flags ||| GenericParameterAttributes.Contravariant
       
        let flags = if gp.gpReferenceTypeConstraint        then flags ||| GenericParameterAttributes.ReferenceTypeConstraint        else flags 
        let flags = if gp.gpNotNullableValueTypeConstraint then flags ||| GenericParameterAttributes.NotNullableValueTypeConstraint else flags
        let flags = if gp.gpDefaultConstructorConstraint   then flags ||| GenericParameterAttributes.DefaultConstructorConstraint   else flags
        
        gpB.SetGenericParameterAttributes(flags)
    )

//----------------------------------------------------------------------------
// convFieldInit
//----------------------------------------------------------------------------

let convFieldInit x = 
    match x with 
    | FieldInit_string s       -> box s
    | FieldInit_bool bool      -> box bool   
    | FieldInit_char u16       -> box (char (int u16))  
    | FieldInit_int8 i8        -> box i8     
    | FieldInit_int16 i16      -> box i16    
    | FieldInit_int32 i32      -> box i32    
    | FieldInit_int64 i64      -> box i64    
    | FieldInit_uint8 u8       -> box u8     
    | FieldInit_uint16 u16     -> box u16    
    | FieldInit_uint32 u32     -> box u32    
    | FieldInit_uint64 u64     -> box u64    
    | FieldInit_single ieee32 -> box ieee32 
    | FieldInit_double ieee64 -> box ieee64 
    | FieldInit_ref            -> (null :> Object)

//----------------------------------------------------------------------------
// Some types require hard work...
//----------------------------------------------------------------------------

// This is gross. TypeBuilderInstantiation should really be a public type, since we
// have to use alternative means for various Method/Field/Constructor lookups.  However since 
// it isn't we resort to this technique...
let TypeBuilderInstantiationT = Type.GetType("System.Reflection.Emit.TypeBuilderInstantiation" )

let typeIsNotQueryable (typ : Type) =
    (typ :? TypeBuilder) || ((typ.GetType()).Equals(TypeBuilderInstantiationT))

//----------------------------------------------------------------------------
// convFieldSpec
//----------------------------------------------------------------------------

let queryableTypeGetField emEnv (parentT:Type) fref  =
    let tyargTs  = getGenericArgumentsOfType parentT
    parentT.GetField(fref.frefName, BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance ||| BindingFlags.Static )  
        |> nonNull (sprintf "queryableTypeGetField: %O::%s" parentT fref.frefName)
    
let nonQueryableTypeGetField (parentTI:Type) (fieldInfo : FieldInfo) : FieldInfo = 
    if parentTI.IsGenericType then TypeBuilder.GetField(parentTI,fieldInfo) else fieldInfo


let convFieldSpec emEnv fspec =
    let fref = fspec.fspecFieldRef
    let tref = fref.frefParent 
    let parentTI = convType emEnv fspec.fspecEnclosingType
    if isEmittedTypeRef emEnv tref then
        // NOTE: if "convType becomes convCreatedType", then handle queryable types here too. [bug 4063] (necessary? what repro?)
        let fieldB = envGetFieldB emEnv fref
        nonQueryableTypeGetField parentTI fieldB
    else
        // Prior type.
        if typeIsNotQueryable parentTI then 
            let parentT = getTypeConstructor parentTI
            let fieldInfo = queryableTypeGetField emEnv parentT  fref 
            nonQueryableTypeGetField parentTI fieldInfo
        else 
            queryableTypeGetField emEnv parentTI fspec.fspecFieldRef

//----------------------------------------------------------------------------
// convMethodRef
//----------------------------------------------------------------------------

let queryableTypeGetMethodBySearch emEnv parentT (mref:ILMethodRef) =
    assert(not (typeIsNotQueryable(parentT)));
    let cconv = (if mref.CallingConv.IsStatic then BindingFlags.Static else BindingFlags.Instance)
    let methInfos = parentT.GetMethods(cconv ||| BindingFlags.Public ||| BindingFlags.NonPublic) |> Array.to_list
      (* First, filter on name, if unique, then binding "done" *)
    let tyargTs = getGenericArgumentsOfType parentT      
    let methInfos = methInfos |> List.filter (fun methInfo -> methInfo.Name = mref.Name)
    match methInfos with 
    | [methInfo] -> 
        if verbose then dprintf "Got '%O::%s' by singluar name match\n" parentT mref.Name;
        methInfo
    | _ ->
      (* Second, type match. Note type erased (non-generic) F# code would not type match but they have unique names *)
        let select (methInfo:MethodInfo) =
            let tyargTIs = tyargTs
            (* mref implied Types *)
            let mtyargTIs = getGenericArgumentsOfMethod methInfo 
            if Array.length mtyargTIs  <> mref.GenericArity then false (* method generic arity mismatch *) else
            let argTs,resT = 
                let emEnv = envPushTyvars emEnv (Array.append tyargTs mtyargTIs)
                let argTs = List.map (convType emEnv) mref.ArgTypes
                let resT  = convType emEnv mref.ReturnType
                argTs,resT 
          
          (* methInfo implied Types *)
            let haveArgTs = 
              let parameters = Array.to_list (methInfo.GetParameters())
              parameters |> List.map (fun param -> param.ParameterType) 
         
            let haveResT  = methInfo.ReturnType
          (* check for match *)
            if List.length argTs <> List.length haveArgTs then false (* method argument length mismatch *) else
            let res = equalTypeLists (resT::argTs) (haveResT::haveArgTs) 
            res
       
        match List.tryfind select methInfos with
        | None          -> failwith "convMethodRef: could not bind to method"
        | Some methInfo -> methInfo (* return MethodInfo for (generic) type's (generic) method *)
                           |> nonNull "convMethodRef"
          
let queryableTypeGetMethod emEnv parentT (mref:ILMethodRef) =
    assert(not (typeIsNotQueryable(parentT)));
    if mref.GenericArity = 0 then 
        let tyargTs = getGenericArgumentsOfType parentT      
        let argTs,resT = 
            let emEnv = envPushTyvars emEnv tyargTs
            let argTs = Array.of_list (List.map (convType emEnv) mref.ArgTypes)
            let resT  = convType emEnv mref.ReturnType
            argTs,resT 
        let stat = mref.CallingConv.IsStatic
        if verbose then dprintf "Using GetMethod to get '%O::%s', static = %b, parentT = %s\n" parentT mref.Name stat parentT.AssemblyQualifiedName;
        argTs |> Array.iteri (fun i ty -> if verbose then dprintf "arg %d = %O\n" i ty);
        let cconv = (if stat then BindingFlags.Static else BindingFlags.Instance)
        let methInfo = 
            try 
              parentT.GetMethod(mref.Name,cconv ||| BindingFlags.Public ||| BindingFlags.NonPublic,
                                null,
                                argTs,
                                (null:ParameterModifier[])) 
            // This can fail if there is an ambiguity w.r.t. return type 
            with _ -> null
        if (isNonNull methInfo && equalTypes resT methInfo.ReturnType) then 
            (if verbose then dprintf "Got method '%O' using GetMethod, resT = %O, haveResT = %O\n" methInfo resT methInfo.ReturnType;
             methInfo)
        else
            (if verbose then dprintf "**** Failed lookup or Incorrect return type for '%O' using GetMethod\n" methInfo;
             queryableTypeGetMethodBySearch emEnv parentT mref)
    else 
        if verbose then dprintf "Using queryableTypeGetMethodBySearch to get '%O::%s'\n" parentT mref.Name;
        queryableTypeGetMethodBySearch emEnv parentT mref

let nonQueryableTypeGetMethod (parentTI:Type) (methInfo : MethodInfo) : MethodInfo = 
    if (parentTI.IsGenericType &&
        not (equalTypes parentTI (getTypeConstructor parentTI))) 
    then TypeBuilder.GetMethod(parentTI,methInfo )
    else methInfo 

let convMethodRef emEnv (parentTI:Type) (mref:ILMethodRef) =
    if verbose then dprintf "- convMethodRef %s %s\n" mref.EnclosingTypeRef.Name mref.Name;
    let parent = mref.EnclosingTypeRef
    if isEmittedTypeRef emEnv parent then
        // NOTE: if "convType becomes convCreatedType", then handle queryable types here too. [bug 4063]      
        // Emitted type, can get fully generic MethodBuilder from env.
        let methB = envGetMethB emEnv mref
        if verbose then dprintf "- convMethodRef, isEmitted = true\n";
        nonQueryableTypeGetMethod parentTI methB
        |> nonNull "convMethodRef (emitted)"
    else
        // Prior type.
        if typeIsNotQueryable parentTI then 
            let parentT = getTypeConstructor parentTI
            let methInfo = queryableTypeGetMethod emEnv parentT mref 
            nonQueryableTypeGetMethod parentTI methInfo
        else 
            queryableTypeGetMethod emEnv parentTI mref 

//----------------------------------------------------------------------------
// convMethodSpec
//----------------------------------------------------------------------------
      
let convMethodSpec emEnv (mspec:ILMethodSpec) =
    if verbose then dprintf "- convMethodSpec %s with inst=%d\n" mspec.Name mspec.GenericArgs.Length;
    let typT     = convType emEnv mspec.EnclosingType       (* (instanced) parent Type *)
    let methInfo = convMethodRef emEnv typT mspec.MethodRef (* (generic)   method of (generic) parent *)
    if verbose then dprintf "- convMethodSpec, methInfo = %A\n" methInfo;
    let methInfo =
        if mspec.GenericArgs = [] then 
            methInfo // non generic 
        else 
            let minstTs  = Array.of_list (List.map (convType emEnv) mspec.GenericArgs)
            let methInfo = methInfo.MakeGenericMethod minstTs // instantiate method 
            if verbose then dprintf "- convMethodSpec (after MakeGenericMethod), methInfo = %A\n" methInfo;
            methInfo
    methInfo |> nonNull "convMethodSpec"

//----------------------------------------------------------------------------
// - QueryableTypeGetConstructors: get a constructor on a non-TypeBuilder type
//----------------------------------------------------------------------------

let queryableTypeGetConstructor emEnv (parentT:Type) (mref:ILMethodRef)  =
    let tyargTs  = getGenericArgumentsOfType parentT
    let reqArgTs  = 
        let emEnv = envPushTyvars emEnv tyargTs
        Array.of_list (List.map (convType emEnv) mref.ArgTypes)
    parentT.GetConstructor(BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance,
                            null, 
                            reqArgTs,
                            null)  

let nonQueryableTypeGetConstructor (parentTI:Type) (consInfo : ConstructorInfo) : ConstructorInfo = 
    if parentTI.IsGenericType then TypeBuilder.GetConstructor(parentTI,consInfo) else consInfo

//----------------------------------------------------------------------------
// convConstructorSpec (like convMethodSpec) 
//----------------------------------------------------------------------------

let convConstructorSpec emEnv (mspec:ILMethodSpec) =
    if verbose then dprintf "- convConstructorSpec %s %s with inst=%d\n" (mspec.MethodRef.EnclosingTypeRef.Name) mspec.Name (List.length mspec.GenericArgs);
    let mref   = mspec.MethodRef
    let parentTI = convType emEnv mspec.EnclosingType
    if verbose then dprintf "- convConstructorSpec: prior type, parentTI = %O\n" parentTI;
    if isEmittedTypeRef emEnv mref.EnclosingTypeRef then
        // NOTE: if "convType becomes convCreatedType", then handle queryable types here too. [bug 4063]
        let consB = envGetConsB emEnv mref
        nonQueryableTypeGetConstructor parentTI consB |> nonNull "convConstructorSpec: (emitted)"
    else
        // Prior type.
        if typeIsNotQueryable parentTI then 
            let parentT  = getTypeConstructor parentTI       
            let ctorG = queryableTypeGetConstructor emEnv parentT mref 
            nonQueryableTypeGetConstructor parentTI ctorG
        else
            queryableTypeGetConstructor emEnv parentTI mref 

//----------------------------------------------------------------------------
// emitLabelMark, defineLabel
//----------------------------------------------------------------------------

let emitLabelMark emEnv (ilG:ILGenerator) (label:ILCodeLabel) =
    //if verbose then dprintf "- emitLabelMark %s\n" (label:string);
    let lab = envGetLabel emEnv label
    ilG.MarkLabel(lab)

let defineLabel (ilG:ILGenerator) emEnv (label:ILCodeLabel) =
    //if verbose then dprintf "- defineLabel %s\n" (label:string);  
    let lab = ilG.DefineLabel()
    envSetLabel emEnv label lab

//----------------------------------------------------------------------------
// emitCustomAttrs
//----------------------------------------------------------------------------

let convCustomAttr emEnv cattr =
    let methInfo = 
       match convConstructorSpec emEnv cattr.customMethod with 
       | null -> failwithf "convCustomAttr: %A" cattr.customMethod
       | res -> res
    let data = cattr.customData 
    (methInfo,data)

let emitCustomAttr  emEnv add cattr  = add (convCustomAttr emEnv cattr)
let emitCustomAttrs emEnv add cattrs = List.iter (emitCustomAttr emEnv add) (dest_custom_attrs cattrs)

//----------------------------------------------------------------------------
// emitInstr cenv - I_arith
//----------------------------------------------------------------------------
 
let rec emitInstrI_arith emEnv (ilG:ILGenerator) x = 
    match x with
    | AI_add                      -> ilG.Emit(OpCodes.Add) 
    | AI_add_ovf                  -> ilG.Emit(OpCodes.Add_Ovf) 
    | AI_add_ovf_un               -> ilG.Emit(OpCodes.Add_Ovf_Un)
    | AI_and                      -> ilG.Emit(OpCodes.And)
    | AI_div                      -> ilG.Emit(OpCodes.Div)
    | AI_div_un                   -> ilG.Emit(OpCodes.Div_Un)
    | AI_ceq                      -> ilG.Emit(OpCodes.Ceq)
    | AI_cgt                      -> ilG.Emit(OpCodes.Cgt)
    | AI_cgt_un                   -> ilG.Emit(OpCodes.Cgt_Un)
    | AI_clt                      -> ilG.Emit(OpCodes.Clt)
    | AI_clt_un                   -> ilG.Emit(OpCodes.Clt_Un)
    (* conversion *)
    | AI_conv dt                  -> (match dt with
                                        | DT_I   -> ilG.Emit(OpCodes.Conv_I)
                                        | DT_I1  -> ilG.Emit(OpCodes.Conv_I1)
                                        | DT_I2  -> ilG.Emit(OpCodes.Conv_I2)
                                        | DT_I4  -> ilG.Emit(OpCodes.Conv_I4)
                                        | DT_I8  -> ilG.Emit(OpCodes.Conv_I8)
                                        | DT_U   -> ilG.Emit(OpCodes.Conv_U)      
                                        | DT_U1  -> ilG.Emit(OpCodes.Conv_U1)      
                                        | DT_U2  -> ilG.Emit(OpCodes.Conv_U2)      
                                        | DT_U4  -> ilG.Emit(OpCodes.Conv_U4)      
                                        | DT_U8  -> ilG.Emit(OpCodes.Conv_U8)
                                        | DT_R   -> ilG.Emit(OpCodes.Conv_R_Un)
                                        | DT_R4  -> ilG.Emit(OpCodes.Conv_R4)
                                        | DT_R8  -> ilG.Emit(OpCodes.Conv_R8)
                                        | DT_REF -> failwith "AI_conv DT_REF?" // XXX - check
                     )
    (* conversion - ovf checks *)
    | AI_conv_ovf dt              -> (match dt with
                                        | DT_I   -> ilG.Emit(OpCodes.Conv_Ovf_I)
                                        | DT_I1  -> ilG.Emit(OpCodes.Conv_Ovf_I1)
                                        | DT_I2  -> ilG.Emit(OpCodes.Conv_Ovf_I2)
                                        | DT_I4  -> ilG.Emit(OpCodes.Conv_Ovf_I4)
                                        | DT_I8  -> ilG.Emit(OpCodes.Conv_Ovf_I8)
                                        | DT_U   -> ilG.Emit(OpCodes.Conv_Ovf_U)      
                                        | DT_U1  -> ilG.Emit(OpCodes.Conv_Ovf_U1)      
                                        | DT_U2  -> ilG.Emit(OpCodes.Conv_Ovf_U2)      
                                        | DT_U4  -> ilG.Emit(OpCodes.Conv_Ovf_U4)
                                        | DT_U8  -> ilG.Emit(OpCodes.Conv_Ovf_U8)
                                        | DT_R   -> failwith "AI_conv_ovf DT_R?" // XXX - check       
                                        | DT_R4  -> failwith "AI_conv_ovf DT_R4?" // XXX - check       
                                        | DT_R8  -> failwith "AI_conv_ovf DT_R8?" // XXX - check       
                                        | DT_REF -> failwith "AI_conv_ovf DT_REF?" // XXX - check
                     )
    (* conversion - ovf checks and unsigned *)
    | AI_conv_ovf_un dt           -> (match dt with
                                        | DT_I   -> ilG.Emit(OpCodes.Conv_Ovf_I_Un)
                                        | DT_I1  -> ilG.Emit(OpCodes.Conv_Ovf_I1_Un)
                                        | DT_I2  -> ilG.Emit(OpCodes.Conv_Ovf_I2_Un)
                                        | DT_I4  -> ilG.Emit(OpCodes.Conv_Ovf_I4_Un)
                                        | DT_I8  -> ilG.Emit(OpCodes.Conv_Ovf_I8_Un)
                                        | DT_U   -> ilG.Emit(OpCodes.Conv_Ovf_U_Un)            
                                        | DT_U1  -> ilG.Emit(OpCodes.Conv_Ovf_U1_Un)      
                                        | DT_U2  -> ilG.Emit(OpCodes.Conv_Ovf_U2_Un)      
                                        | DT_U4  -> ilG.Emit(OpCodes.Conv_Ovf_U4_Un)      
                                        | DT_U8  -> ilG.Emit(OpCodes.Conv_Ovf_U8_Un)
                                        | DT_R   -> failwith "AI_conv_ovf_un DT_R?" // XXX - check       
                                        | DT_R4  -> failwith "AI_conv_ovf_un DT_R4?" // XXX - check       
                                        | DT_R8  -> failwith "AI_conv_ovf_un DT_R8?" // XXX - check       
                                        | DT_REF -> failwith "AI_conv_ovf_un DT_REF?" // XXX - check
                     )
    | AI_mul                      -> ilG.Emit(OpCodes.Mul)
    | AI_mul_ovf                  -> ilG.Emit(OpCodes.Mul_Ovf)
    | AI_mul_ovf_un               -> ilG.Emit(OpCodes.Mul_Ovf_Un)
    | AI_rem                      -> ilG.Emit(OpCodes.Rem)
    | AI_rem_un                   -> ilG.Emit(OpCodes.Rem_Un)
    | AI_shl                      -> ilG.Emit(OpCodes.Shl)
    | AI_shr                      -> ilG.Emit(OpCodes.Shr)
    | AI_shr_un                   -> ilG.Emit(OpCodes.Shr_Un)
    | AI_sub                      -> ilG.Emit(OpCodes.Sub)
    | AI_sub_ovf                  -> ilG.Emit(OpCodes.Sub_Ovf)
    | AI_sub_ovf_un               -> ilG.Emit(OpCodes.Sub_Ovf_Un)
    | AI_xor                      -> ilG.Emit(OpCodes.Xor)
    | AI_or                       -> ilG.Emit(OpCodes.Or)
    | AI_neg                      -> ilG.Emit(OpCodes.Neg)
    | AI_not                      -> ilG.Emit(OpCodes.Not)
    | AI_ldnull                   -> ilG.Emit(OpCodes.Ldnull)
    | AI_dup                      -> ilG.Emit(OpCodes.Dup)
    | AI_pop                      -> ilG.Emit(OpCodes.Pop)
    | AI_ckfinite                 -> ilG.Emit(OpCodes.Ckfinite)
    | AI_nop                      -> ilG.Emit(OpCodes.Nop)
    | AI_ldc (DT_I4,NUM_I4 i32)   -> ilG.Emit(OpCodes.Ldc_I4,i32)
    | AI_ldc (DT_I8,NUM_I8 i64)   -> ilG.Emit(OpCodes.Ldc_I8,i64)
    | AI_ldc (DT_R4,NUM_R4 r32)   -> ilG.Emit(OpCodes.Ldc_R4,r32)
    | AI_ldc (DT_R8,NUM_R8 r64)   -> ilG.Emit(OpCodes.Ldc_R8,r64)
    | AI_ldc (_    ,_         )   -> failwith "emitInstrI_arith (AI_ldc (typ,const)) iltyped"


///Emit comparison instructions
let emitInstrCompare emEnv (ilG:ILGenerator) comp targ = 
    match comp with
    | BI_beq     -> ilG.Emit(OpCodes.Beq    ,envGetLabel emEnv targ)
    | BI_bge     -> ilG.Emit(OpCodes.Bge    ,envGetLabel emEnv targ)
    | BI_bge_un  -> ilG.Emit(OpCodes.Bge_Un ,envGetLabel emEnv targ)
    | BI_bgt     -> ilG.Emit(OpCodes.Bgt    ,envGetLabel emEnv targ)
    | BI_bgt_un  -> ilG.Emit(OpCodes.Bgt_Un ,envGetLabel emEnv targ)
    | BI_ble     -> ilG.Emit(OpCodes.Ble    ,envGetLabel emEnv targ)
    | BI_ble_un  -> ilG.Emit(OpCodes.Ble_Un ,envGetLabel emEnv targ)
    | BI_blt     -> ilG.Emit(OpCodes.Blt    ,envGetLabel emEnv targ)
    | BI_blt_un  -> ilG.Emit(OpCodes.Blt_Un ,envGetLabel emEnv targ)
    | BI_bne_un  -> ilG.Emit(OpCodes.Bne_Un ,envGetLabel emEnv targ)
    | BI_brfalse -> ilG.Emit(OpCodes.Brfalse,envGetLabel emEnv targ)
    | BI_brtrue  -> ilG.Emit(OpCodes.Brtrue ,envGetLabel emEnv targ)


/// Emit the volatile. prefix
let emitInstrVolatile (ilG:ILGenerator) = function
    | Volatile    -> ilG.Emit(OpCodes.Volatile)
    | Nonvolatile -> ()

/// Emit the align. prefix
let emitInstrAlign (ilG:ILGenerator) = function      
    | Aligned     -> ()
    | Unaligned_1 -> ilG.Emit(OpCodes.Unaligned,1L) // note: doc says use "long" overload!
    | Unaligned_2 -> ilG.Emit(OpCodes.Unaligned,2L)
    | Unaligned_4 -> ilG.Emit(OpCodes.Unaligned,3L)

/// Emit the tail. prefix if necessary
let emitInstrTail (ilG:ILGenerator) tail emitTheCall = 
    match tail with
    | Tailcall   -> ilG.Emit(OpCodes.Tailcall); emitTheCall(); ilG.Emit(OpCodes.Ret)
    | Normalcall -> emitTheCall()

let emitInstrNewobj emEnv (ilG:ILGenerator) mspec varargs =
    match varargs with
    | None         -> ilG.Emit(OpCodes.Newobj,convConstructorSpec emEnv mspec)
    | Some vartyps -> failwith "emit: pending new varargs" // XXX - gap

let emitInstrCall emEnv (ilG:ILGenerator) opCall tail (mspec:ILMethodSpec) varargs =
    if verbose then dprintf "emitInstrCall\n";
    emitInstrTail ilG tail (fun () ->
        if mspec.MethodRef.Name = ".ctor" || mspec.MethodRef.Name = ".cctor" then
            let cinfo = convConstructorSpec emEnv mspec
            match varargs with
            | None         -> ilG.Emit     (opCall,cinfo)
            | Some vartyps -> failwith "emitInstrCall: .ctor and varargs"
        else
            let minfo = convMethodSpec emEnv mspec
            match varargs with
            | None         -> ilG.Emit     (opCall,minfo)
            | Some vartyps -> ilG.EmitCall (opCall,minfo,Array.of_list (List.map (convType emEnv) vartyps))
    )


//----------------------------------------------------------------------------
// emitInstr cenv
//----------------------------------------------------------------------------

let rec emitInstr cenv (modB : ModuleBuilder) emEnv (ilG:ILGenerator) instr = 
    match instr with 
    | I_arith ainstr              -> emitInstrI_arith emEnv ilG ainstr
    | I_ldarg  u16                -> ilG.Emit(OpCodes.Ldarg ,int16 u16)
    | I_ldarga u16                -> ilG.Emit(OpCodes.Ldarga,int16 u16)
    | I_ldind (align,vol,dt)      -> emitInstrAlign ilG align;
                                     emitInstrVolatile ilG vol;
                                     (match dt with
                                      | DT_I   -> ilG.Emit(OpCodes.Ldind_I)
                                      | DT_I1  -> ilG.Emit(OpCodes.Ldind_I1)
                                      | DT_I2  -> ilG.Emit(OpCodes.Ldind_I2)
                                      | DT_I4  -> ilG.Emit(OpCodes.Ldind_I4)
                                      | DT_I8  -> ilG.Emit(OpCodes.Ldind_I8)
                                      | DT_R   -> failwith "emitInstr cenv: ldind R"
                                      | DT_R4  -> ilG.Emit(OpCodes.Ldind_R4)
                                      | DT_R8  -> ilG.Emit(OpCodes.Ldind_R8)
                                      | DT_U   -> failwith "emitInstr cenv: ldind U"
                                      | DT_U1  -> ilG.Emit(OpCodes.Ldind_U1)
                                      | DT_U2  -> ilG.Emit(OpCodes.Ldind_U2)
                                      | DT_U4  -> ilG.Emit(OpCodes.Ldind_U4)
                                      | DT_U8  -> failwith "emitInstr cenv: ldind U8"
                                      | DT_REF -> ilG.Emit(OpCodes.Ldind_Ref))
    | I_ldloc  u16                -> ilG.Emit(OpCodes.Ldloc ,int16 u16)
    | I_ldloca u16                -> ilG.Emit(OpCodes.Ldloca,int16 u16)
    | I_starg  u16                -> ilG.Emit(OpCodes.Starg ,int16 u16)
    | I_stind (align,vol,dt)      -> emitInstrAlign ilG align;
                                     emitInstrVolatile ilG vol;
                                     (match dt with
                                      | DT_I   -> ilG.Emit(OpCodes.Stind_I)
                                      | DT_I1  -> ilG.Emit(OpCodes.Stind_I1)
                                      | DT_I2  -> ilG.Emit(OpCodes.Stind_I2)
                                      | DT_I4  -> ilG.Emit(OpCodes.Stind_I4)
                                      | DT_I8  -> ilG.Emit(OpCodes.Stind_I8)
                                      | DT_R   -> failwith "emitInstr cenv: stind R"
                                      | DT_R4  -> ilG.Emit(OpCodes.Stind_R4)
                                      | DT_R8  -> ilG.Emit(OpCodes.Stind_R8)
                                      | DT_U   -> ilG.Emit(OpCodes.Stind_I)    // NOTE: unsigned -> int conversion
                                      | DT_U1  -> ilG.Emit(OpCodes.Stind_I1)   // NOTE: follows code ilwrite.ml
                                      | DT_U2  -> ilG.Emit(OpCodes.Stind_I2)   // NOTE: is it ok?
                                      | DT_U4  -> ilG.Emit(OpCodes.Stind_I4)   // NOTE: it is generated by bytearray tests
                                      | DT_U8  -> ilG.Emit(OpCodes.Stind_I8)   // NOTE: unsigned -> int conversion
                                      | DT_REF -> ilG.Emit(OpCodes.Stind_Ref)) 
    | I_stloc  u16                -> ilG.Emit(OpCodes.Stloc,int16 u16)
    | I_br  label                 -> ilG.Emit(OpCodes.Br,envGetLabel emEnv label)
    | I_jmp mspec                 -> let methInfo = convMethodSpec emEnv mspec
                                     ilG.Emit(OpCodes.Jmp,methInfo)
    | I_brcmp (comp,targ,fall)    -> emitInstrCompare emEnv ilG comp targ;
                                     ilG.Emit(OpCodes.Br,envGetLabel emEnv fall)  // XXX - very likely to be the next instruction...
    | I_switch (labels,next)      -> ilG.Emit(OpCodes.Switch,Array.of_list (List.map (envGetLabel emEnv) labels));
                                     ilG.Emit(OpCodes.Br,envGetLabel emEnv next)  // XXX - very likely to be the next instruction...
    | I_ret                       -> ilG.Emit(OpCodes.Ret)
    | I_call           (tail,mspec,varargs)   -> emitInstrCall emEnv ilG OpCodes.Call     tail mspec varargs
    | I_callvirt       (tail,mspec,varargs)   -> emitInstrCall emEnv ilG OpCodes.Callvirt tail mspec varargs
    | I_callconstraint (tail,typ,mspec,varargs) -> ilG.Emit(OpCodes.Constrained,convType emEnv typ); 
                                                   emitInstrCall emEnv ilG OpCodes.Callvirt tail mspec varargs   
    | I_calli (tail,callsig,None)             -> emitInstrTail ilG tail (fun () ->
                                                   ilG.EmitCalli(OpCodes.Calli,
                                                                 convCallConv callsig.callsigCallconv,
                                                                 convType emEnv callsig.callsigReturn,
                                                                 Array.of_list (List.map (convType emEnv) callsig.callsigArgs),
                                                                 Array.of_list []))
    | I_calli (tail,callsig,Some vartyps)     -> emitInstrTail ilG tail (fun () ->
                                                   ilG.EmitCalli(OpCodes.Calli,
                                                                 convCallConv callsig.callsigCallconv,
                                                                 convType emEnv callsig.callsigReturn,
                                                                 Array.of_list (List.map (convType emEnv) callsig.callsigArgs),
                                                                 Array.of_list (List.map (convType emEnv) vartyps)))
    | I_ldftn mspec                           -> ilG.Emit(OpCodes.Ldftn,convMethodSpec emEnv mspec)
    | I_newobj (mspec,varargs)                -> emitInstrNewobj emEnv ilG mspec varargs
    | I_throw                        -> ilG.Emit(OpCodes.Throw)
    | I_endfinally                   -> ilG.Emit(OpCodes.Endfinally) (* capitalization! *)
    | I_endfilter                    -> () (* ilG.Emit(OpCodes.Endfilter) *)
    | I_leave label                  -> ilG.Emit(OpCodes.Leave,envGetLabel emEnv label)
    | I_ldsfld (vol,fspec)           ->                           emitInstrVolatile ilG vol; ilG.Emit(OpCodes.Ldsfld ,convFieldSpec emEnv fspec)
    | I_ldfld (align,vol,fspec)      -> emitInstrAlign ilG align; emitInstrVolatile ilG vol; ilG.Emit(OpCodes.Ldfld  ,convFieldSpec emEnv fspec)
    | I_ldsflda fspec                ->                                                      ilG.Emit(OpCodes.Ldsflda,convFieldSpec emEnv fspec)
    | I_ldflda fspec                 ->                                                      ilG.Emit(OpCodes.Ldflda ,convFieldSpec emEnv fspec)
    | I_stsfld (vol,fspec)           ->                           emitInstrVolatile ilG vol; ilG.Emit(OpCodes.Stsfld ,convFieldSpec emEnv fspec)
    | I_stfld (align,vol,fspec)      -> emitInstrAlign ilG align; emitInstrVolatile ilG vol; ilG.Emit(OpCodes.Stfld  ,convFieldSpec emEnv fspec)
    | I_ldstr     s                  -> ilG.Emit(OpCodes.Ldstr    ,s)
    | I_isinst    typ                -> ilG.Emit(OpCodes.Isinst   ,convType emEnv typ)
    | I_castclass typ                -> ilG.Emit(OpCodes.Castclass,convType emEnv typ)
    | I_ldtoken (Token_type typ)     -> ilG.Emit(OpCodes.Ldtoken  ,convType emEnv typ)
    | I_ldtoken (Token_method mspec) -> ilG.Emit(OpCodes.Ldtoken  ,convMethodSpec emEnv mspec)
    | I_ldtoken (Token_field fspec)  -> ilG.Emit(OpCodes.Ldtoken  ,convFieldSpec  emEnv fspec)
    | I_ldvirtftn mspec              -> ilG.Emit(OpCodes.Ldvirtftn,convMethodSpec emEnv mspec)
    (* Value type instructions *)
    | I_cpobj     typ             -> ilG.Emit(OpCodes.Cpobj    ,convType emEnv typ)
    | I_initobj   typ             -> ilG.Emit(OpCodes.Initobj  ,convType emEnv typ)
    | I_ldobj (align,vol,typ)     -> emitInstrAlign ilG align; emitInstrVolatile ilG vol; ilG.Emit(OpCodes.Ldobj ,convType emEnv typ)
    | I_stobj (align,vol,typ)     -> emitInstrAlign ilG align; emitInstrVolatile ilG vol; ilG.Emit(OpCodes.Stobj ,convType emEnv typ)
    | I_box       typ             -> ilG.Emit(OpCodes.Box      ,convType emEnv typ)
    | I_unbox     typ             -> ilG.Emit(OpCodes.Unbox    ,convType emEnv typ)
    | I_unbox_any typ             -> ilG.Emit(OpCodes.Unbox_Any,convType emEnv typ)
    | I_sizeof    typ             -> ilG.Emit(OpCodes.Sizeof   ,convType emEnv typ)
    // Generalized array instructions. 
    // In AbsIL these instructions include 
    // both the single-dimensional variants (with ILArrayShape == Rank1ArrayShape) 
    // and calls to the "special" multi-dimensional "methods" such as 
    //   newobj void string[,]::.ctor(int32, int32) 
    //   call string string[,]::Get(int32, int32) 
    //   call string& string[,]::Address(int32, int32) 
    //   call void string[,]::Set(int32, int32,string) 
    // The IL reader transforms calls of this form to the corresponding 
    // generalized instruction with the corresponding ILArrayShape 
    // argument. This is done to simplify the IL and make it more uniform. 
    // The IL writer then reverses this when emitting the binary. 
    | I_ldelem dt                 -> (match dt with
                                      | DT_I   -> ilG.Emit(OpCodes.Ldelem_I)
                                      | DT_I1  -> ilG.Emit(OpCodes.Ldelem_I1)
                                      | DT_I2  -> ilG.Emit(OpCodes.Ldelem_I2)
                                      | DT_I4  -> ilG.Emit(OpCodes.Ldelem_I4)
                                      | DT_I8  -> ilG.Emit(OpCodes.Ldelem_I8)
                                      | DT_R   -> failwith "emitInstr cenv: ldelem R"
                                      | DT_R4  -> ilG.Emit(OpCodes.Ldelem_R4)
                                      | DT_R8  -> ilG.Emit(OpCodes.Ldelem_R8)
                                      | DT_U   -> failwith "emitInstr cenv: ldelem U"
                                      | DT_U1  -> ilG.Emit(OpCodes.Ldelem_U1)
                                      | DT_U2  -> ilG.Emit(OpCodes.Ldelem_U2)
                                      | DT_U4  -> ilG.Emit(OpCodes.Ldelem_U4)
                                      | DT_U8  -> failwith "emitInstr cenv: ldelem U8"
                                      | DT_REF -> ilG.Emit(OpCodes.Ldelem_Ref))
    | I_stelem dt                 -> (match dt with
                                      | DT_I   -> ilG.Emit(OpCodes.Stelem_I)
                                      | DT_I1  -> ilG.Emit(OpCodes.Stelem_I1)
                                      | DT_I2  -> ilG.Emit(OpCodes.Stelem_I2)
                                      | DT_I4  -> ilG.Emit(OpCodes.Stelem_I4)
                                      | DT_I8  -> ilG.Emit(OpCodes.Stelem_I8)
                                      | DT_R   -> failwith "emitInstr cenv: stelem R"
                                      | DT_R4  -> ilG.Emit(OpCodes.Stelem_R4)
                                      | DT_R8  -> ilG.Emit(OpCodes.Stelem_R8)
                                      | DT_U   -> failwith "emitInstr cenv: stelem U"
                                      | DT_U1  -> failwith "emitInstr cenv: stelem U1"
                                      | DT_U2  -> failwith "emitInstr cenv: stelem U2"
                                      | DT_U4  -> failwith "emitInstr cenv: stelem U4"
                                      | DT_U8  -> failwith "emitInstr cenv: stelem U8"
                                      | DT_REF -> ilG.Emit(OpCodes.Stelem_Ref))
    | I_ldelema (ro,shape,typ)     -> 
        if (ro = ReadonlyAddress) then ilG.Emit(OpCodes.Readonly);
        if (shape = Rank1ArrayShape) 
        then ilG.Emit(OpCodes.Ldelema,convType emEnv typ)
        else 
            let aty = convType emEnv (Type_array(shape,typ)) 
            let ety = aty.GetElementType()
            let rty = ety.MakeByRefType() 
            let meth = modB.GetArrayMethod(aty,"Address",System.Reflection.CallingConventions.HasThis,rty,Array.create shape.Rank (typeof<int>) )
            ilG.Emit(OpCodes.Call,meth)
    | I_ldelem_any (shape,typ)     -> 
        if (shape = Rank1ArrayShape)      then ilG.Emit(OpCodes.Ldelem,convType emEnv typ)
        else 
            let aty = convType emEnv (Type_array(shape,typ)) 
            let ety = aty.GetElementType()
            let meth = modB.GetArrayMethod(aty,"Get",System.Reflection.CallingConventions.HasThis,ety,Array.create shape.Rank (typeof<int>) )
            ilG.Emit(OpCodes.Call,meth)
    | I_stelem_any (shape,typ)     -> 
        if (shape = Rank1ArrayShape)      then ilG.Emit(OpCodes.Stelem,convType emEnv typ)
        else 
            let aty = convType emEnv (Type_array(shape,typ)) 
            let ety = aty.GetElementType()
            let meth = modB.GetArrayMethod(aty,"Set",System.Reflection.CallingConventions.HasThis,(null:Type),Array.append (Array.create shape.Rank (typeof<int>)) (Array.of_list [ ety ])) 
            ilG.Emit(OpCodes.Call,meth)
    | I_newarr (shape,typ)         -> 
        if (shape = Rank1ArrayShape)
        then ilG.Emit(OpCodes.Newarr,convType emEnv typ)
        else 
            let aty = convType emEnv (Type_array(shape,typ)) 
            let ety = aty.GetElementType()
            let meth = modB.GetArrayMethod(aty,".ctor",System.Reflection.CallingConventions.HasThis,(null:Type),Array.create shape.Rank (typeof<int>))
            ilG.Emit(OpCodes.Newobj,meth)
    | I_ldlen                      -> ilG.Emit(OpCodes.Ldlen)
    | I_mkrefany   typ             -> ilG.Emit(OpCodes.Mkrefany,convType emEnv typ)
    | I_refanytype                 -> ilG.Emit(OpCodes.Refanytype)
    | I_refanyval typ              -> ilG.Emit(OpCodes.Refanyval,convType emEnv typ)
    | I_rethrow                    -> ilG.Emit(OpCodes.Rethrow)
    | I_break                      -> ilG.Emit(OpCodes.Break)
    | I_seqpoint src               -> 
        if cenv.generate_pdb && not (src.Document.File.EndsWith("stdin",StringComparison.Ordinal)) then
            let guid x = match x with None -> Guid.Empty | Some g -> Guid(g:byte[]) in
            let symDoc = modB.DefineDocument(src.Document.File, guid src.Document.Language, guid src.Document.Vendor, guid src.Document.DocumentType)
            ilG.MarkSequencePoint(symDoc, src.Line, src.Column, src.EndLine, src.EndColumn)
    | I_arglist                    -> ilG.Emit(OpCodes.Arglist)
    | I_localloc                   -> ilG.Emit(OpCodes.Localloc)
    | I_cpblk (align,vol)          -> emitInstrAlign ilG align;
                                      emitInstrVolatile ilG vol;
                                      ilG.Emit(OpCodes.Cpblk)
    | I_initblk (align,vol)        -> emitInstrAlign ilG align;
                                      emitInstrVolatile ilG vol;
                                      ilG.Emit(OpCodes.Initblk)
    | EI_ldlen_multi (n,m) -> 
        emitInstr cenv modB emEnv ilG (mk_ldc_i32 m);
        emitInstr cenv modB emEnv ilG (mk_normal_call(mk_nongeneric_mspec_in_typ(cenv.ilg.typ_Array, ILCallingConv.Instance, "GetLength", [cenv.ilg.typ_int32], cenv.ilg.typ_int32)))
    | I_other e when is_ilx_ext_instr e -> Printf.failwithf "the ILX instruction %s cannot be emitted" (e.ToString())
    |  i -> Printf.failwithf "the IL instruction %s cannot be emitted" (i.ToString())

//----------------------------------------------------------------------------
// emitCode 
//----------------------------------------------------------------------------

let emitBasicBlock cenv  modB emEnv (ilG:ILGenerator) bblock =
    if verbose then dprintf "emitBasicBlock cenv\n";    
    emitLabelMark emEnv ilG bblock.bblockLabel;
    Array.iter (emitInstr cenv modB emEnv ilG) bblock.bblockInstrs;
    ()

let emitCode cenv modB emEnv (ilG:ILGenerator) code =
    if verbose then dprintf "emitCode cenv\n";  
    // pre define labels pending determining their actual marks
    let labels = labels_of_code code
    //List.iter (fun lab -> if verbose then dprintf "Label %s \n" lab) labels;
    let emEnv  = List.fold (defineLabel ilG) emEnv labels
    let rec emitter = function
        | ILBasicBlock bblock                  -> emitBasicBlock cenv modB emEnv ilG bblock
        | GroupBlock (localDebugInfos,codes) -> if verbose then dprintf "emitGroupBlock\n";
                                                List.iter emitter codes
        | RestrictBlock (labels,code)        -> if verbose then dprintf "emitRestrictBlock\n";
                                                emitter code (* restrictions ignorable: code_labels unique *)
        | TryBlock (code,seh)                -> 
            if verbose then dprintf "emitTryBlock: start\n";
            let endExBlockL = ilG.BeginExceptionBlock()
            emitter code;
            //ilG.MarkLabel endExBlockL;
            emitHandler seh;
            ilG.EndExceptionBlock();
            if verbose then dprintf "emitTryBlock: done\n"
    and emitHandler seh =
        if verbose then dprintf "emitHandler\n";    
        match seh with      
        | FaultBlock code         -> ilG.BeginFaultBlock();   emitter code
        | FinallyBlock code       -> ilG.BeginFinallyBlock(); emitter code
        | FilterCatchBlock fcodes -> 
            let emitFilter (filter,code) =
                match filter with
                | TypeFilter typ  -> 
                    ilG.BeginCatchBlock (convType emEnv typ); 
                    emitter code
                | CodeFilter test -> 
                    ilG.BeginExceptFilterBlock(); 
                    emitter test; 
                    ilG.BeginCatchBlock null; 
                    emitter code
            fcodes |> List.iter emitFilter 
    emitter code

//----------------------------------------------------------------------------
// emitILMethodBody 
//----------------------------------------------------------------------------

let emitLocal emEnv (ilG : ILGenerator) local =
    ilG.DeclareLocal(convType emEnv local.localType,local.localPinned)

let emitILMethodBody cenv modB emEnv (ilG:ILGenerator) ilmbody =
    // XXX - REVIEW:
    //      ilNoInlining: bool;
    //      ilSource: source option }
    if verbose then dprintf "emitILMethodBody cenv: start\n";  
    // emit locals and record emEnv
    let localBs = List.map (emitLocal emEnv ilG) ilmbody.ilLocals
    let emEnv = envSetLocals emEnv (Array.of_list localBs)
    emitCode cenv modB emEnv ilG ilmbody.ilCode;
    if verbose then dprintf "emitILMethodBody cenv: end\n"

//----------------------------------------------------------------------------
// emitMethodBody 
//----------------------------------------------------------------------------

let emitMethodBody cenv modB emEnv ilG name mbody =
    match dest_mbody mbody with
    | MethodBody_il ilmbody       -> emitILMethodBody cenv modB emEnv (ilG()) ilmbody
    | MethodBody_pinvoke  pinvoke -> () (* Printf.printf "EMIT: pinvoke method %s\n" name *) (* XXX - check *)
    | MethodBody_abstract         -> () (* Printf.printf "EMIT: abstract method %s\n" name *) (* XXX - check *)
    | MethodBody_native           -> failwith "emitMethodBody cenv: native"               (* XXX - gap *)

//----------------------------------------------------------------------------
// emitParameter
//----------------------------------------------------------------------------

let emitParameter emEnv (defineParameter : int * ParameterAttributes * string -> ParameterBuilder) i param =
    //  -paramType: typ;
    //  -paramDefault: ILFieldInit option;  
    //  -paramMarshal: NativeType option; (* Marshalling map for parameters. COM Interop only. *)
    if verbose then dprintf "emitParameter %s\n" (match param.paramName with Some n -> n | None -> "anon");
    let attrs = flagsIf param.paramIn       ParameterAttributes.In ||| 
                flagsIf param.paramOut      ParameterAttributes.Out |||
                flagsIf param.paramOptional ParameterAttributes.Optional
    let name = match param.paramName with
               | Some name -> name
               | None      -> "X"^string(i+1)
   
    let parB = defineParameter(i,attrs,name)
    emitCustomAttrs emEnv (fun (x,y) -> parB.SetCustomAttribute(x,y)) param.paramCustomAttrs

//----------------------------------------------------------------------------
// convMethodAttributes
//----------------------------------------------------------------------------

let convMethodAttributes mdef =    
    if verbose then dprintf "- convMethodAttributes %s\n" mdef.mdName;
    let attrKind = 
        match mdef.mdKind with 
        | MethodKind_static        -> MethodAttributes.Static
        | MethodKind_cctor         -> MethodAttributes.Static
        | MethodKind_ctor          -> enum 0                 
        | MethodKind_nonvirtual    -> enum 0
        | MethodKind_virtual vinfo -> MethodAttributes.Virtual |||
                                      flagsIf vinfo.virtNewslot   MethodAttributes.NewSlot |||
                                      flagsIf vinfo.virtFinal     MethodAttributes.Final |||
                                      flagsIf vinfo.virtStrict    MethodAttributes.CheckAccessOnOverride |||
                                      flagsIf vinfo.virtAbstract  MethodAttributes.Abstract
   
    let attrAccess = 
        match mdef.mdAccess with
        | MemAccess_assembly -> MethodAttributes.Assembly
        | MemAccess_compilercontrolled -> failwith "Method access compiler controled."
        | MemAccess_famandassem        -> MethodAttributes.FamANDAssem
        | MemAccess_famorassem         -> MethodAttributes.FamORAssem
        | MemAccess_family             -> MethodAttributes.Family
        | MemAccess_private            -> MethodAttributes.Private
        | MemAccess_public             -> MethodAttributes.Public
   
    let attrOthers = flagsIf mdef.mdHasSecurity MethodAttributes.HasSecurity |||
                     flagsIf mdef.mdSpecialName MethodAttributes.SpecialName |||
                     flagsIf mdef.mdHideBySig   MethodAttributes.HideBySig |||
                     flagsIf mdef.mdReqSecObj   MethodAttributes.RequireSecObject 
   
    attrKind ||| attrAccess ||| attrOthers

let convMethodImplFlags mdef =    
    if verbose then dprintf "- convMethodImplFlags %s\n" mdef.mdName;
    (match  mdef.mdCodeKind with 
     | MethodCodeKind_native -> MethodImplAttributes.Native
     | MethodCodeKind_runtime -> MethodImplAttributes.Runtime
     | MethodCodeKind_il  -> MethodImplAttributes.IL) 
    ||| flagsIf mdef.mdInternalCall MethodImplAttributes.InternalCall
    ||| (if mdef.mdManaged then MethodImplAttributes.Managed else MethodImplAttributes.Unmanaged)
    ||| flagsIf mdef.mdForwardRef MethodImplAttributes.ForwardRef
    ||| flagsIf mdef.mdPreserveSig MethodImplAttributes.PreserveSig
    ||| flagsIf mdef.mdSynchronized MethodImplAttributes.Synchronized
    ||| flagsIf (match dest_mbody mdef.mdBody with MethodBody_il b -> b.ilNoInlining | _ -> false) MethodImplAttributes.NoInlining

//----------------------------------------------------------------------------
// buildMethodPass2
//----------------------------------------------------------------------------
  
let rec buildMethodPass2 tref (typB:TypeBuilder) emEnv (mdef : ILMethodDef) =
   // remaining REVIEW:
   // mdCodeKind: MethodCodeKind;   
   // mdInternalCall: bool;
   // mdManaged: bool;
   // mdForwardRef: bool;
   // mdSecurityDecls: Permissions;
   // mdUnmanagedExport: bool; (* -- The method is exported to unmanaged code using COM interop. *)
   // mdSynchronized: bool;
   // mdPreserveSig: bool;
   // mdMustRun: bool; (* Whidbey feature: SafeHandle finalizer must be run *)
   // mdExport: (i32 * string option) option; 
   // mdVtableEntry: (i32 * i32) option;
    if verbose then dprintf "buildMethodPass2 %s\n" mdef.mdName;
    let attrs = convMethodAttributes mdef
    let implflags = convMethodImplFlags mdef
    let cconv = convCallConv mdef.mdCallconv
    let mref = mk_mref_to_mdef (tref,mdef)   
    let emEnv = if mdef.mdEntrypoint && mdef.ParameterTypes=[] then 
                    (* Bug 2209:
                        Here, we collect the entry points generated by ilxgen corresponding to the top-level effects.
                        Users can (now) annotate their own functions with EntryPoint attributes.
                        However, these user entry points functions must take string[] argument.
                        By only adding entry points with no arguments, we only collect the top-level effects.
                     *)
                    envAddEntryPt emEnv (typB,mdef.mdName)
                else
                    emEnv
    match dest_mbody mdef.mdBody with
    | MethodBody_pinvoke  p -> 
        let argtys = Array.of_list (List.map (convType emEnv) mdef.ParameterTypes) 
        let rty = convType emEnv mdef.Return.Type
        
        let pcc = 
            match p.pinvokeCallconv with 
            | PInvokeCallConvCdecl -> CallingConvention.Cdecl
            | PInvokeCallConvStdcall -> CallingConvention.StdCall
            | PInvokeCallConvThiscall -> CallingConvention.ThisCall
            | PInvokeCallConvFastcall -> CallingConvention.FastCall
            | PInvokeCallConvNone 
            | PInvokeCallConvWinapi -> CallingConvention.Winapi 
        
        let pcs = 
            match p.PInvokeCharEncoding with 
            | PInvokeEncodingNone -> CharSet.None
            | PInvokeEncodingAnsi -> CharSet.Ansi
            | PInvokeEncodingUnicode -> CharSet.Unicode
            | PInvokeEncodingAuto -> CharSet.Auto 
      
(* p.PInvokeThrowOnUnmappableChar *)
(* p.PInvokeCharBestFit *)
(* p.pinvokeNoMangle *)

        let methB = typB.DefinePInvokeMethod(mdef.mdName, 
                                             p.pinvokeWhere.Name, 
                                             p.pinvokeName, 
                                             attrs, 
                                             cconv, 
                                             rty, 
                                             null, null, 
                                             argtys, 
                                             null, null, 
                                             pcc, 
                                             pcs) 
        methB.SetImplementationFlags(implflags);
        envBindMethodRef emEnv mref methB

    | _ -> 
      match mdef.mdName with
      | ".cctor" 
      | ".ctor" ->
          let consB = typB.DefineConstructor(attrs,
                                             cconv,
                                             Array.of_list (List.map (convType emEnv) mdef.ParameterTypes))
          consB.SetImplementationFlags(implflags);
          envBindConsRef emEnv mref consB
      | name    ->
          // Note the return/argument types may involve the generic parameters
          let methB = typB.DefineMethod(mdef.mdName,attrs,cconv) 
        
          // Method generic type parameters         
          buildGenParamsPass1 emEnv (fun x -> methB.DefineGenericParameters(x)) mdef.GenericParams;
          let genArgs = getGenericArgumentsOfMethod methB 
          let emEnv = envPushTyvars emEnv (Array.append (getGenericArgumentsOfType typB) genArgs)
          buildGenParamsPass1b emEnv genArgs mdef.GenericParams;
          // set parameter and return types (may depend on generic args)
          methB.SetParameters(Array.of_list (List.map (convType emEnv) mdef.ParameterTypes));
          methB.SetReturnType(convType emEnv mdef.Return.Type);
          let emEnv = envPopTyvars emEnv
          methB.SetImplementationFlags(implflags);
          envBindMethodRef emEnv mref methB


//----------------------------------------------------------------------------
// buildMethodPass3 cenv
//----------------------------------------------------------------------------
    
let rec buildMethodPass3 cenv tref modB (typB:TypeBuilder) emEnv (mdef : ILMethodDef) =
    if verbose then dprintf "buildMethodPass3 cenv %s\n" mdef.mdName;
    let mref  = mk_mref_to_mdef (tref,mdef)
    match dest_mbody mdef.mdBody with
    | MethodBody_pinvoke  p -> ()
    | _ -> 
         match mdef.mdName with
         | ".cctor" | ".ctor" ->
              let consB = envGetConsB emEnv mref
              // Constructors can not have generic parameters
              assert (mdef.GenericParams=[]);
              // Value parameters       
              let defineParameter (i,attr,name) = consB.DefineParameter(i+1,attr,name)
              mdef.mdParams |> List.iteri (emitParameter emEnv defineParameter);
              // Body
              emitMethodBody cenv modB emEnv (fun () -> consB.GetILGenerator()) mdef.mdName mdef.mdBody;
              emitCustomAttrs emEnv (fun (x,y) -> consB.SetCustomAttribute(x,y)) mdef.mdCustomAttrs;
              ()
         | name ->
              let methB = envGetMethB emEnv mref
              let emEnv = envPushTyvars emEnv (Array.append
                                                 (getGenericArgumentsOfType typB)
                                                 (getGenericArgumentsOfMethod methB))

              begin match (dest_custom_attrs mdef.mdReturn.returnCustomAttrs) with
              | [] -> ()
              | _ ->
                  let retB = methB.DefineParameter(0,System.Reflection.ParameterAttributes.Retval,null) 
                  emitCustomAttrs emEnv (fun (x,y) -> retB.SetCustomAttribute(x,y)) mdef.mdReturn.returnCustomAttrs
              end;

              // Value parameters
              let defineParameter (i,attr,name) = methB.DefineParameter(i+1,attr,name) 
              mdef.mdParams |> List.iteri (emitParameter emEnv defineParameter);
              // Body
              emitMethodBody cenv modB emEnv (fun () -> methB.GetILGenerator()) mdef.mdName mdef.mdBody;
              let emEnv = envPopTyvars emEnv // case fold later...
              emitCustomAttrs emEnv (fun (x,y) -> methB.SetCustomAttribute(x,y)) mdef.mdCustomAttrs;
              ()
      
//----------------------------------------------------------------------------
// buildFieldPass2
//----------------------------------------------------------------------------
  
let buildFieldPass2 tref (typB:TypeBuilder) emEnv (fdef : ILFieldDef) =
    (*{ -fdData:    bytes option;       
        -fdMarshal: NativeType option;  *)
    if verbose then dprintf "buildFieldPass2 %s\n" fdef.fdName;
    let attrsAccess = match fdef.fdAccess with
                      | MemAccess_assembly           -> FieldAttributes.Assembly
                      | MemAccess_compilercontrolled -> failwith "Field access compiler controled."
                      | MemAccess_famandassem        -> FieldAttributes.FamANDAssem
                      | MemAccess_famorassem         -> FieldAttributes.FamORAssem
                      | MemAccess_family             -> FieldAttributes.Family
                      | MemAccess_private            -> FieldAttributes.Private
                      | MemAccess_public             -> FieldAttributes.Public
    let attrsOther = flagsIf fdef.fdStatic        FieldAttributes.Static |||
                     flagsIf fdef.fdSpecialName   FieldAttributes.SpecialName |||
                     flagsIf fdef.fdLiteral       FieldAttributes.Literal |||
                     flagsIf fdef.fdInitOnly      FieldAttributes.InitOnly |||
                     flagsIf fdef.fdNotSerialized FieldAttributes.NotSerialized 
    let attrs = attrsAccess ||| attrsOther
    let fieldT = convType emEnv fdef.fdType
    let fieldB = 
        match fdef.fdData with 
        | Some d -> typB.DefineInitializedData(fdef.fdName, d, attrs)
        | None -> typB.DefineField(fdef.fdName,
                                   fieldT,
                                   attrs)
     
    // set default value
    fdef.fdInit   |> Option.iter (fun initial -> fieldB.SetConstant(convFieldInit initial));
    fdef.fdOffset |> Option.iter (fun offset ->  fieldB.SetOffset(offset));
    // assert unsupported:    
    assert (fdef.fdMarshal=None);
    // custom attributes: done on pass 3 as they may reference attribute constructors generated on
    // pass 2.
    let fref = mk_fref_in_tref (tref,fdef.fdName,fdef.fdType)    
    envBindFieldRef emEnv fref fieldB

let buildFieldPass3 tref (typB:TypeBuilder) emEnv (fdef : ILFieldDef) =
    if verbose then dprintf "buildFieldPass3 %s\n" fdef.fdName;
    let fref = mk_fref_in_tref (tref,fdef.fdName,fdef.fdType)    
    let fieldB = envGetFieldB emEnv fref
    emitCustomAttrs emEnv (fun (x,y) -> fieldB.SetCustomAttribute(x,y)) fdef.fdCustomAttrs

//----------------------------------------------------------------------------
// buildPropertyPass2,3
//----------------------------------------------------------------------------
  
let buildPropertyPass2 tref (typB:TypeBuilder) emEnv (prop : ILPropertyDef) =
    (*{ -propCallconv: hasthis; } *)
    if verbose then dprintf "buildPropertyPass2 %s\n" prop.propName;
    let attrs = flagsIf prop.propRTSpecialName PropertyAttributes.RTSpecialName |||
                flagsIf prop.propSpecialName   PropertyAttributes.SpecialName
    let propB = typB.DefineProperty(prop.propName,
                                    attrs,
                                    convType emEnv prop.propType,
                                    Array.of_list (List.map (convType emEnv) prop.propArgs)) 
   
    // install get/set methods
    let installOp descr setOp opMRef =
        if verbose then dprintf "buildPropertyPass2: installing %s" descr;      
        setOp(envGetMethB emEnv opMRef)
   
    prop.propSet |> Option.iter (installOp "set" (fun methB -> propB.SetSetMethod(methB)));
    prop.propGet |> Option.iter (installOp "get" (fun methB -> propB.SetGetMethod(methB)));
    // set default value
    prop.propInit |> Option.iter (fun initial -> propB.SetConstant(convFieldInit initial));
    // custom attributes
    let pref = mk_pref (tref,prop.propName)    
    envBindPropRef emEnv pref propB
    // XXX - propCallconv ???

let buildPropertyPass3 tref (typB:TypeBuilder) emEnv (prop : ILPropertyDef) = 
  let pref = mk_pref (tref,prop.propName)    
  let propB = envGetPropB emEnv pref
  emitCustomAttrs emEnv (fun (x,y) -> propB.SetCustomAttribute(x,y)) prop.propCustomAttrs

//----------------------------------------------------------------------------
// buildMethodImplsPass3
//----------------------------------------------------------------------------
  
let buildMethodImplsPass3 tref (typB:TypeBuilder) emEnv (mimpl : IL.ILMethodImplDef) =
    if verbose then dprintf "buildMethodImplsPass3 %s\n" mimpl.mimplOverrideBy.MethodRef.Name;    
    let bodyMethInfo = convMethodRef emEnv (typB :> Type) mimpl.mimplOverrideBy.MethodRef // doc: must be MethodBuilder
    let (OverridesSpec (mref,dtyp)) = mimpl.mimplOverrides
    let declMethTI = convType emEnv dtyp 
    let declMethInfo = convMethodRef  emEnv declMethTI mref
    typB.DefineMethodOverride(bodyMethInfo,declMethInfo);
    emEnv

//----------------------------------------------------------------------------
// typeAttributesOf*
//----------------------------------------------------------------------------

let typeAttrbutesOfTypeDefKind x = 
    match x with 
    // required for a TypeBuilder
    | TypeDef_class           -> TypeAttributes.Class
    | TypeDef_valuetype       -> TypeAttributes.Class
    | TypeDef_interface       -> TypeAttributes.Interface
    | TypeDef_enum            -> TypeAttributes.Class
    | TypeDef_delegate        -> TypeAttributes.Class
    | TypeDef_other xtdk      -> failwith "typeAttributes of other external"

let typeAttrbutesOfTypeAccess x =
    match x with 
    | TypeAccess_public       -> TypeAttributes.Public
    | TypeAccess_private      -> TypeAttributes.NotPublic
    | TypeAccess_nested macc  -> match macc with
                                 | MemAccess_assembly           -> TypeAttributes.NestedAssembly
                                 | MemAccess_compilercontrolled -> failwith "Nested compiler controled."
                                 | MemAccess_famandassem        -> TypeAttributes.NestedFamANDAssem
                                 | MemAccess_famorassem         -> TypeAttributes.NestedFamORAssem
                                 | MemAccess_family             -> TypeAttributes.NestedFamily
                                 | MemAccess_private            -> TypeAttributes.NestedPrivate
                                 | MemAccess_public             -> TypeAttributes.NestedPublic
                        
let typeAttributesOfTypeEncoding x = 
    match x with 
    | TypeEncoding_ansi     -> TypeAttributes.AnsiClass    
    | TypeEncoding_autochar -> TypeAttributes.AutoClass
    | TypeEncoding_unicode  -> TypeAttributes.UnicodeClass


let typeAttributesOfTypeLayout cenv emEnv x = 
    let attr p = 
      if p.typeSize =None && p.typePack = None then None
      else 
        Some(convCustomAttr emEnv 
               (IL.mk_custom_attribute cenv.ilg
                  (mk_tref (cenv.ilg.mscorlib_scoref,"System.Runtime.InteropServices.StructLayoutAttribute"), 
                   [mk_nongeneric_value_typ (mk_tref (cenv.ilg.mscorlib_scoref,"System.Runtime.InteropServices.LayoutKind")) ],
                   [ CustomElem_int32 0x02 ],
                   (p.typePack |> Option.to_list |> List.map (fun x -> ("Pack", cenv.ilg.typ_int32, false, CustomElem_int32 (int32 x))))  @
                   (p.typeSize |> Option.to_list |> List.map (fun x -> ("Size", cenv.ilg.typ_int32, false, CustomElem_int32 x)))))) in
    match x with 
    | TypeLayout_auto         -> TypeAttributes.AutoLayout,None
    | TypeLayout_explicit p   -> TypeAttributes.ExplicitLayout,(attr p)
    | TypeLayout_sequential p -> TypeAttributes.SequentialLayout, (attr p)


//----------------------------------------------------------------------------
// buildTypeDefPass1 cenv
//----------------------------------------------------------------------------
    
let rec buildTypeDefPass1 cenv emEnv (modB:ModuleBuilder) rootTypeBuilder nesting (tdef : ILTypeDef) =
    // -tdComInterop: bool; (* Class or interface generated for COM interop *) 
    // -tdSecurityDecls: Permissions;
    // -tdInitSemantics: ILTypeDefInitSemantics;
    // -tdEvents: events;
    if verbose then dprintf "buildTypeDefPass1 cenv %s\n" tdef.Name;
    // TypeAttributes
    let attrsKind   = typeAttrbutesOfTypeDefKind tdef.tdKind 
    let attrsAccess = typeAttrbutesOfTypeAccess  tdef.tdAccess
    let attrsLayout,cattrsLayout = typeAttributesOfTypeLayout cenv emEnv tdef.tdLayout
    let attrsEnc    = typeAttributesOfTypeEncoding tdef.tdEncoding
    let attrsOther  = flagsIf tdef.IsAbstract     TypeAttributes.Abstract |||
                      flagsIf tdef.IsSealed       TypeAttributes.Sealed |||
                      flagsIf tdef.IsSerializable TypeAttributes.Serializable |||
                      flagsIf tdef.tdSpecialName  TypeAttributes.SpecialName |||
                      flagsIf tdef.tdHasSecurity  TypeAttributes.HasSecurity
     
    let attrsType = attrsKind ||| attrsAccess ||| attrsLayout ||| attrsEnc ||| attrsOther

    // TypeBuilder from TypeAttributes.
    if verbose then dprintf "buildTypeDefPass1 cenv: build name = %s\n" tdef.Name;
    let typB : TypeBuilder = rootTypeBuilder  (tdef.Name,attrsType)
    let typB = typB |> nonNull "buildTypeDefPass1 cenv: typB is null!"
    cattrsLayout |> Option.iter (fun (x,y) -> typB.SetCustomAttribute(x,y));

    buildGenParamsPass1 emEnv (fun x -> typB.DefineGenericParameters(x)) tdef.GenericParams; 
    // bind tref -> (typT,typB)
    let tref = tref_for_nested_tdef ScopeRef_local (nesting,tdef)    
    let typT =
        // Q: would it be ok to use typB :> Type ?
        // Maybe not, recall TypeBuilder maybe subtype of Type, but it is not THE Type.
        let nameInModule = tref.QualifiedName
        if verbose then dprintf "buildTypeDefPass1 cenv: nameInModule= %s\n" nameInModule;
        modB.GetType(nameInModule,false,false)
   
    if verbose then dprintf "buildTypeDefPass1 cenv: null? %d\n" (if typT=null then 0 else 1);
    let emEnv = envBindTypeRef emEnv tref (typT,typB,tdef)
    // recurse on nested types
    let nesting = nesting @ [tdef]     
    let buildNestedType emEnv tdef = buildTypeTypeDef cenv emEnv modB typB nesting tdef
    let emEnv = List.fold buildNestedType emEnv (dest_tdefs  tdef.NestedTypes)
    emEnv

and buildTypeTypeDef cenv emEnv modB (typB : TypeBuilder) nesting tdef =
    if verbose then dprintf "buildTypeTypeDef cenv\n";
    let rootTypB  (name,attrs)        = typB.DefineNestedType(name,attrs)       
    buildTypeDefPass1 cenv emEnv modB rootTypB nesting tdef

//----------------------------------------------------------------------------
// buildTypeDefPass1b
//----------------------------------------------------------------------------
    
let rec buildTypeDefPass1b nesting emEnv (tdef : ILTypeDef) = 
    if verbose then dprintf "buildTypeDefPass1b %s\n" tdef.Name; 
    let tref = tref_for_nested_tdef ScopeRef_local (nesting,tdef)
    let typB  = envGetTypB emEnv tref
    let genArgs = (getGenericArgumentsOfType typB) 
    let emEnv = envPushTyvars emEnv genArgs
    // Parent may reference types being defined, so has to come after it's Pass1 creation 
    tdef.Extends |> Option.iter (fun typ -> typB.SetParent(convType emEnv typ));
    // build constraints on ILGenericParameterDefs.  Constraints may reference types being defined, 
    // so have to come after all types are created
    buildGenParamsPass1b emEnv genArgs tdef.GenericParams; 
    let emEnv = envPopTyvars emEnv     
    let nesting = nesting @ [tdef]     
    List.iter (buildTypeDefPass1b nesting emEnv) (dest_tdefs tdef.NestedTypes)

//----------------------------------------------------------------------------
// buildTypeDefPass2
//----------------------------------------------------------------------------

let rec buildTypeDefPass2 nesting emEnv (tdef : ILTypeDef) = 
    if verbose then dprintf "buildTypeDefPass2 %s\n" tdef.Name; 
    let tref = tref_for_nested_tdef ScopeRef_local (nesting,tdef)
    let typB  = envGetTypB emEnv tref
    let emEnv = envPushTyvars emEnv (getGenericArgumentsOfType typB)
    // add interface impls
    tdef.Implements |> List.map (convType emEnv) |> List.iter (fun implT -> typB.AddInterfaceImplementation(implT));
    // add methods, properties
    let emEnv = List.fold (buildMethodPass2      tref typB) emEnv (dest_mdefs tdef.Methods) 
    let emEnv = List.fold (buildFieldPass2       tref typB) emEnv (dest_fdefs tdef.Fields)  
    let emEnv = List.fold (buildPropertyPass2    tref typB) emEnv (dest_pdefs tdef.Properties) 
    let emEnv = envPopTyvars emEnv
    // nested types
    let nesting = nesting @ [tdef]
    let emEnv = List.fold (buildTypeDefPass2 nesting) emEnv (dest_tdefs tdef.NestedTypes)
    emEnv

//----------------------------------------------------------------------------
// buildTypeDefPass3 cenv
//----------------------------------------------------------------------------
    
let rec buildTypeDefPass3 cenv nesting modB emEnv (tdef : ILTypeDef) =
    if verbose then dprintf "buildTypeDefPass3 cenv %s\n" tdef.Name; 
    let tref = tref_for_nested_tdef ScopeRef_local (nesting,tdef)
    let typB = envGetTypB emEnv tref
    let emEnv = envPushTyvars emEnv (getGenericArgumentsOfType typB)
    // add method bodies, properties
    tdef.Methods |> dest_mdefs |> List.iter (buildMethodPass3 cenv tref modB typB emEnv);
    tdef.Properties |> dest_pdefs |> List.iter (buildPropertyPass3 tref typB emEnv);
    tdef.Fields  |> dest_fdefs |> List.iter (buildFieldPass3 tref typB emEnv);
    let emEnv = List.fold (buildMethodImplsPass3 tref typB) emEnv (dest_mimpls     tdef.tdMethodImpls)
    tdef.tdCustomAttrs |> emitCustomAttrs emEnv (fun (x,y) -> typB.SetCustomAttribute(x,y)) ;
    // custom attributes
    let emEnv = envPopTyvars emEnv
    // nested types
    let nesting = nesting @ [tdef]
    let emEnv = List.fold (buildTypeDefPass3 cenv nesting modB) emEnv (dest_tdefs tdef.NestedTypes)
    emEnv

//----------------------------------------------------------------------------
// buildTypeDefPass4 - Create the Types
// MSDN says: If this type is a nested type, the CreateType method must 
// be called on the enclosing type before it is called on the nested type.
// If the current type derives from an incomplete type or implements 
// incomplete interfaces, call the CreateType method on the parent 
// type and the interface types before calling it on the current type.
// If the enclosing type contains a field that is a value type 
// defined as a nested type (for example, a field that is an 
// enumeration defined as a nested type), calling the CreateType method 
// on the enclosing type will generate a AppDomain.TypeResolve event. 
// This is because the loader cannot determine the size of the enclosing 
// type until the nested type has been completed. The caller should define 
// a handler for the TypeResolve event to complete the definition of the 
// nested type by calling CreateType on the TypeBuilder object that represents 
// the nested type. The code example for this topic shows how to define such 
// an event handler.
//----------------------------------------------------------------------------

let enclosing_trefs_of_tref (tref:ILTypeRef) = 
   match tref.Enclosing with 
   | [] -> []
   | h :: t -> List.scan_left (fun tr nm -> mk_tref_in_tref (tr,nm)) (mk_tref(tref.Scope, h)) t

let rec trefs_of_typ valueTypesOnly typ acc = 
    match typ with
    | Type_void | Type_tyvar _                              -> acc
    | Type_ptr eltType | Type_byref eltType -> acc
    | Type_array (_,eltType) -> if valueTypesOnly then acc else trefs_of_typ valueTypesOnly eltType acc
    | Type_value tspec -> tspec.TypeRef :: List.foldBack (trefs_of_typ valueTypesOnly) tspec.GenericArgs acc
    | Type_boxed tspec -> if valueTypesOnly then acc else tspec.TypeRef :: List.foldBack (trefs_of_typ valueTypesOnly) tspec.GenericArgs acc
    | Type_fptr callsig -> failwith "trefs_of_typ: fptr"
    | Type_modified _   -> failwith "trefs_of_typ: modified"

let verbose2 = false
    
let createTypeRef (visited : Dictionary<_,_>, created : Dictionary<_,_>) emEnv tref = 
    let rec traverseTypeDef priority tref (tdef:ILTypeDef) =
        // WORKAROUND (ProductStudio FSharp 1.0 bug 615): the constraints on generic method parameters 
        // are resolved overly eagerly by reflection emit's CreateType. The priority drops down to 1 here
        // because we absolutely have to create these types before attempting to create the enclosing type.
        if priority >= 1 then 
            if verbose2 then dprintf "buildTypeDefPass4: Doing method constraints of %s\n" tdef.Name; 
            tdef.Methods |> dest_mdefs |> List.iter   (fun md -> md.GenericParams |> List.iter (fun gp -> gp.Constraints |> List.iter (traverseType false 1)));
        // We have to define all struct types in all methods before a class is defined. This only has any effect when there is a struct type
        // being defined simultaneously with this type.
        if priority >= 1 then 
            if verbose2 then dprintf "buildTypeDefPass4: Doing value types in method signautres of %s, #mdefs = %d\n" tdef.Name (List.length (tdef.Methods |> dest_mdefs)); 
            tdef.Methods |> dest_mdefs |> List.iter   (fun md -> md.Parameters |> List.iter (fun p -> p.Type |> (traverseType true 1))
                                                                 md.Return.Type |> traverseType true 1);
        // We absolutely need the parent type...
        if priority >= 1 then 
            if verbose2 then dprintf "buildTypeDefPass4: Creating Super Class Chain of %s\n" tdef.Name; 
            tdef.Extends    |> Option.iter (traverseType false priority);
        
        // We absolutely need the interface types...
        if priority >= 1 then 
            if verbose2 then dprintf "buildTypeDefPass4: Creating Interface Chain of %s\n" tdef.Name; 
            tdef.Implements |> List.iter (traverseType false priority);
        
        if priority >= 1 then 
            if verbose2 then dprintf "buildTypeDefPass4: Do value types in fields of %s\n" tdef.Name; 
            tdef.Fields |> dest_fdefs |> List.iter (fun fd -> traverseType true 1 fd.Type);
        
        (* There seem to be some types we can create without creating the enclosing types *)
        (* Hence only attempt to create the enclosing types when the priority is >= 2 *)
        if priority >= 2 then 
            if verbose2 then dprintf "buildTypeDefPass4: Creating Enclosing Types of %s\n" tdef.Name; 
            tref |> enclosing_trefs_of_tref |> List.iter (traverseTypeRef priority);

        if verbose2 then dprintf "buildTypeDefPass4: Done with dependencies of %s\n" tdef.Name
    and traverseType valueTypesOnly priority typ = 
        if verbose2 then dprintf "- traverseType %A\n" typ;
        trefs_of_typ valueTypesOnly typ []
        |> List.filter (isEmittedTypeRef emEnv)
        |> List.iter (traverseTypeRef priority)

    and traverseTypeRef priority  tref = 
        let typB = envGetTypB emEnv tref
        if verbose2 then dprintf "- considering reference to type %s\n" typB.FullName;
        if not (visited.ContainsKey(tref)) or visited.[tref] > priority then 
            visited.[tref] <- priority;
            let tdef = envGetTypeDef emEnv tref
            if verbose2 then dprintf "- traversing type %s\n" typB.FullName;
            traverseTypeDef priority tref tdef;
            if not (created.ContainsKey(tref)) then 
                created.[tref] <- true;
                if verbose2 then dprintf "- creating type %s\n" typB.FullName;
                typB.CreateType()  |> ignore

    traverseTypeRef 2 tref 

let rec buildTypeDefPass4 (visited,created) nesting emEnv (tdef : ILTypeDef) =
    if verbose2 then dprintf "buildTypeDefPass4 %s\n" tdef.Name; 
    let tref = tref_for_nested_tdef ScopeRef_local (nesting,tdef)
    let typB = envGetTypB emEnv tref
    createTypeRef (visited,created) emEnv tref;
    // nested types
    let nesting = nesting @ [tdef]
    tdef.NestedTypes |> dest_tdefs |> List.iter (buildTypeDefPass4 (visited,created) nesting emEnv)

//----------------------------------------------------------------------------
// buildModuleType
//----------------------------------------------------------------------------
     
let buildModuleTypePass1 cenv (modB:ModuleBuilder) emEnv (tdef:ILTypeDef) =
    if verbose then dprintf "buildModuleTypePass1 cenv, tdef.Name = %s\n" tdef.Name;
    let rootTypB  (name,attrs)        = modB.DefineType(name,attrs)       
    buildTypeDefPass1 cenv emEnv modB rootTypB [] tdef

let buildModuleTypePass1b          emEnv tdef = buildTypeDefPass1b [] emEnv tdef
let buildModuleTypePass2           emEnv tdef = buildTypeDefPass2 [] emEnv tdef
let buildModuleTypePass3 cenv modB emEnv tdef = buildTypeDefPass3 cenv [] modB emEnv tdef
let buildModuleTypePass4 visited   emEnv tdef = buildTypeDefPass4 visited [] emEnv tdef

//----------------------------------------------------------------------------
// buildModuleFragment - only the types the fragment get written
//----------------------------------------------------------------------------
    
let buildModuleFragment cenv emEnv (asmB : AssemblyBuilder) (modB : ModuleBuilder) (m: ILModuleDef) =
    let tdefs = dest_tdefs m.modulTypeDefs in

    let emEnv = List.fold (buildModuleTypePass1 cenv modB) emEnv tdefs
    Array.iter (fun (tyT:Type) -> if verbose then dprintf "fqn = %s\n" tyT.FullName) (modB.GetTypes());
    List.iter (buildModuleTypePass1b emEnv) tdefs
    let emEnv = List.fold buildModuleTypePass2   emEnv  tdefs
    let emEnv = List.fold (buildModuleTypePass3 cenv modB)   emEnv  tdefs
    let visited = new Dictionary<_,_>(10) 
    let created = new Dictionary<_,_>(10) 
    List.iter (buildModuleTypePass4  (visited,created) emEnv) tdefs
    let emEnv = Seq.fold envUpdateCreatedTypeRef emEnv created.Keys // update typT with the created typT
    emitCustomAttrs emEnv (fun (x,y) -> modB.SetCustomAttribute(x,y)) m.modulCustomAttrs;    
    m.modulResources |> dest_resources |> List.iter (fun r -> 
        let attribs = (match r.resourceAccess with Resource_public -> ResourceAttributes.Public | Resource_private -> ResourceAttributes.Private) 
        match r.resourceWhere with 
        | Resource_local bf -> 
            modB.DefineManifestResource(r.resourceName, new IO.MemoryStream(bf()), attribs)
        | Resource_file (mr,n) -> 
           asmB.AddResourceFile(r.resourceName, mr.Name, attribs)
        | Resource_assembly _ -> 
           failwith "references to resources other assemblies may not be emitted using System.Reflection");
    emEnv

//----------------------------------------------------------------------------
// test hook
//----------------------------------------------------------------------------

let mkDynamicAssemblyAndModule assemblyName debugInfo =
    let filename = assemblyName ^ ".dll"
    let currentDom  = System.AppDomain.CurrentDomain
    let asmDir  = "."
    let asmName = new AssemblyName()
    asmName.Name <- assemblyName;
    let asmB = currentDom.DefineDynamicAssembly(asmName,AssemblyBuilderAccess.RunAndSave,asmDir) 
    let modB = asmB.DefineDynamicModule(assemblyName,filename,debugInfo)
    asmB,modB

let emitModuleFragment ilg emEnv (asmB : AssemblyBuilder) (modB : ModuleBuilder) (modul : IL.ILModuleDef) (debugInfo : bool) =
    let cenv = { ilg = ilg ; generate_pdb = debugInfo }

    let emEnv = buildModuleFragment cenv emEnv asmB modB modul
    begin match modul.modulManifest with 
    | None -> ()
    | Some mani ->  
       // REVIEW: remainder of manifest
       emitCustomAttrs emEnv (fun (x,y) -> asmB.SetCustomAttribute(x,y)) mani.manifestCustomAttrs;    
    end;
    // invoke entry point methods
    let execEntryPtFun ((typB : TypeBuilder),methodName) () =
      try 
        ignore (typB.InvokeMember(methodName,
                                  BindingFlags.InvokeMethod ||| BindingFlags.Public ||| BindingFlags.Static,
                                  null,
                                  null,
                                  Array.of_list [ ],
                                  Globalization.CultureInfo.InvariantCulture));
        None
      with 
         | :? System.Reflection.TargetInvocationException as e ->
             Some(e.InnerException)
   
    let emEnv,entryPts = envPopEntryPts emEnv
    let execs = List.map execEntryPtFun entryPts
    emEnv,execs


//----------------------------------------------------------------------------
// lookup* allow conversion from AbsIL to their emitted representations
//----------------------------------------------------------------------------

// TypeBuilder is a subtype of Type.
// However, casting TypeBuilder to Type is not the same as getting Type proper.
// The builder version does not implement all methods on the parent.
// 
// The emEnv stores (typT:Type) for each tref.
// Once the emitted type is created this typT is updated to ensure it is the Type proper.
// So Type lookup will return the proper Type not TypeBuilder.
let lookupTypeRef   emEnv tref = Zmap.tryfind tref emEnv.emTypMap   |> Option.map (function (typ,_,_,Some createdTyp) -> createdTyp | (typ,_,_,None) -> typ)
let lookupType      emEnv typ  = convCreatedType emEnv typ

// Lookups of ILFieldRef and MethodRef may require a similar non-Builder-fixup post Type-creation.
let lookupFieldRef  emEnv fref = Zmap.tryfind fref emEnv.emFieldMap |> Option.map (fun fieldBuilder  -> fieldBuilder  :> FieldInfo)
let lookupMethodRef emEnv mref = Zmap.tryfind mref emEnv.emMethMap  |> Option.map (fun methodBuilder -> methodBuilder :> MethodInfo)


//----------------------------------------------------------------------------
// REVIEW:
//  [ ] events todo
//  [ ] check cust attributes get through, e.g STA.
//  [ ] XXX notes (highlight gaps)
//----------------------------------------------------------------------------
