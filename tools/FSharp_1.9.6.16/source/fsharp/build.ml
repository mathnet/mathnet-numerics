// (c) Microsoft Corporation. All rights reserved
#light

/// Loading initial context, reporting errors etc.
module (* internal *) Microsoft.FSharp.Compiler.Build
open System
open System.IO
open Internal.Utilities
open Internal.Utilities.Text
open Microsoft.FSharp.Text
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
open Microsoft.FSharp.Compiler.Pickle
open Microsoft.FSharp.Compiler.Range

open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.TypeChecker

open Microsoft.FSharp.Compiler.SR
open Microsoft.FSharp.Compiler.DiagnosticMessage

module Ilsupp = Microsoft.FSharp.Compiler.AbstractIL.Internal.Support 
module Ilread = Microsoft.FSharp.Compiler.AbstractIL.BinaryReader 

module Tc = Microsoft.FSharp.Compiler.TypeChecker
module SR = Microsoft.FSharp.Compiler.SR
module DM = Microsoft.FSharp.Compiler.DiagnosticMessage

open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Lexhelp
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Infos
open Microsoft.FSharp.Compiler.ConstraintSolver
open Microsoft.FSharp.Compiler.MSBuildResolver
open Microsoft.FSharp.Compiler.Typrelns
open Internal.Utilities.Debug
open Microsoft.FSharp.Compiler.Nameres
open Microsoft.FSharp.Compiler.PrettyNaming

#if DEBUG
let showAssertForUnexpectedException = ref true
#endif

//----------------------------------------------------------------------------
// Some Globals
//--------------------------------------------------------------------------

let sigSuffixes = [".mli";".fsi"]
let implSuffixes = [".ml";".fs";".fsscript";".fsx"]
let resSuffixes = [".resx"]
let scriptSuffixes = [".fsscript";".fsx"]
let lightSyntaxDefaultExtensions : string list = [ ".fs";".fsscript";".fsx";".fsi" ]
let syntaxFlagRequiredExtensions : string list = [] // lightSyntaxDefaultExtensions


//----------------------------------------------------------------------------
// ERROR REPORTING
//--------------------------------------------------------------------------

exception HashIncludeNotAllowedInNonScript of range
exception HashReferenceNotAllowedInNonScript of range
exception HashReferenceCopyAfterCompileNotAllowedInNonScript of range
exception HashDirectiveNotAllowedInNonScript of range
exception FileNameNotResolved of (*filename*) string * (*description of searched locations*) string * range
exception AssemblyNotResolved of (*originalName*) string * range
exception LoadedSourceNotFoundIgnoring of (*filename*) string * range
exception MSBuildReferenceResolutionWarning of (*MSBuild warning code*)string * (*Message*)string * range
exception MSBuildReferenceResolutionError of (*MSBuild warning code*)string * (*Message*)string * range
exception DeprecatedCommandLineOption of string * string * range
exception HashLoadedSourceHasIssues of (*warnings*) exn list * (*errors*) exn list * range
exception HashLoadedScriptConsideredSource of range
exception InvalidInternalsVisibleToAssemblyName of (*badName*)string * (*fileName option*) string option


let rec RangeOfError err = 
  match err with 
  | ErrorFromAddingConstraint(_,err2,_) -> RangeOfError err2 
  | ReservedKeyword(_,m)
  | IndentationProblem(_,m)
  | ErrorFromAddingTypeEquation(_,_,_,_,_,m) 
  | ErrorFromApplyingDefault(_,_,_,_,_,m) 
  | ErrorsFromAddingSubsumptionConstraint(_,_,_,_,_,m) 
  | FunctionExpected(_,_,m)
  | BakedInMemberConstraintName(_,m)
  | IndexOutOfRangeExceptionWarning(m)
  | StandardOperatorRedefinitionWarning(_,m)
  | BadEventTransformation(m)
  | ParameterlessStructCtor(m)
  | FieldNotMutable (_,_,m) 
  | Recursion (_,_,_,_,m) 
  | InvalidRuntimeCoercion(_,_,_,m) 
  | IndeterminateRuntimeCoercion(_,_,_,m)
  | IndeterminateStaticCoercion (_,_,_,m)
  | StaticCoercionShouldUseBox (_,_,_,m)
  | CoercionTargetSealed(_,_,m)
  | UpcastUnnecessary(m)
  | Creflect.IgnoringPartOfQuotedTermWarning (_,m) 
  
  | TypeTestUnnecessary(m)
  | RuntimeCoercionSourceSealed(_,_,m)
  | OverrideDoesntOverride(_,_,_,_,_,m)
  | UnionPatternsBindDifferentNames m 
  | UnionCaseWrongArguments (_,_,_,m) 
  | TypeIsImplicitlyAbstract m 
  | RequiredButNotSpecified (_,_,_,_,m) 
  | FunctionValueUnexpected (_,_,m)
  | UnitTypeExpected (_,_,_,m )
  | UseOfAddressOfOperator m 
  | DeprecatedThreadStaticBindingWarning(m) 
  | NonUniqueInferredAbstractSlot (_,_,_,_,_,m) 
  | DefensiveCopyWarning (_,m)
  | DeprecatedClassFieldInference(m)
  | LetRecCheckedAtRuntime m 
  | UpperCaseIdentifierInPattern m
  | NotUpperCaseConstructor m
  | RecursiveUseCheckedAtRuntime (_,_,m) 
  | LetRecEvaluatedOutOfOrder (_,_,_,m) 
  | Error (_,m)
  | SyntaxError (_,m) 
  | InternalError (_,m)
  | FullAbstraction(_,m)
  | InterfaceNotRevealed(_,_,m) 
  | WrappedError (_,m)
  | Patcompile.MatchIncomplete (_,_,m)
  | Patcompile.RuleNeverMatched m 
  | ValNotMutable(_,_,m)
  | ValNotLocal(_,_,m) 
  | MissingFields(_,m) 
  | OverrideInIntrinsicAugmentation(m)
  | IntfImplInIntrinsicAugmentation(m) 
  | OverrideInExtrinsicAugmentation(m)
  | IntfImplInExtrinsicAugmentation(m) 
  | ValueRestriction(_,_,_,_,m) 
  | LetRecUnsound (_,_,m) 
  | Obsolete (_,m) 
  | Experimental (_,m) 
  | PossibleUnverifiableCode m
  | OCamlCompatibility (_,m) 
  | Deprecated(_,m) 
  | LibraryUseOnly(m) 
  | FieldsFromDifferentTypes (_,_,_,m) 
  | IndeterminateType(m)
  | TyconBadArgs(_,_,_,m) -> Some m
  | FieldNotContained(_,arf,frf,_) -> Some arf.Range
  | ValueNotContained(_,_,aval,_,_) -> Some aval.Range
  | ConstrNotContained(_,aval,_,_) -> Some aval.ucase_id.idRange
  | ExnconstrNotContained(_,aexnc,_,_) -> Some aexnc.Range
  | VarBoundTwice(id) 
  | UndefinedName(_,_,id,_) -> Some id.idRange 
  | Duplicate(_,_,m) 
  | NameClash(_,_,_,m,_,_,_) 
  | UnresolvedOverloading(_,_,_,_,_,m) 
  | PossibleOverload(_,_,m) 
  //| PossibleBestOverload(_,_,m) 
  | VirtualAugmentationOnNullValuedType(m)
  | NonVirtualAugmentationOnNullValuedType(m)
  | NonRigidTypar(_,_,_,_,_,m)
  | ConstraintSolverTupleDiffLengths(_,_,_,m,_) 
  | ConstraintSolverInfiniteTypes(_,_,_,m,_) 
  | ConstraintSolverMissingConstraint(_,_,_,m,_) 
  | ConstraintSolverTypesNotInEqualityRelation(_,_,_,m,_) 
  | ConstraintSolverError(_,m,_) 
  | ConstraintSolverTypesNotInSubsumptionRelation(_,_,_,m,_) 
  | ConstraintSolverRelatedInformation(_,m,_) 
  | SelfRefObjCtor(_,m) -> Some m
  | NotAFunction(_,_,mfun,marg) -> Some mfun
  | UnresolvedReferenceError(_,m) ->Some m
  | UnresolvedPathReference(_,_,m) ->Some m
  | DeprecatedCommandLineOption(_,_,m) ->Some m
  | HashIncludeNotAllowedInNonScript(m)
  | HashReferenceNotAllowedInNonScript(m) 
  | HashDirectiveNotAllowedInNonScript(m)  
  | HashReferenceCopyAfterCompileNotAllowedInNonScript(m) -> Some m  
  | FileNameNotResolved(_,_,m) -> Some m
  | LoadedSourceNotFoundIgnoring(_,m) -> Some m
  | MSBuildReferenceResolutionWarning(_,_,m) -> Some m
  | MSBuildReferenceResolutionError(_,_,m) -> Some m
  | AssemblyNotResolved(_,m) -> Some m
  | HashLoadedSourceHasIssues(_,_,m) -> Some m
  | HashLoadedScriptConsideredSource(m) -> Some m

  // Strip TargetInvocationException wrappers
  | :? System.Reflection.TargetInvocationException as e -> 
      RangeOfError e.InnerException
  
  | _ -> None

let rec GetErrorNumber err = 
  match err with 
  (* DO NOT CHANGE THESE NUMBERS *)
  | ErrorFromAddingTypeEquation _ -> 1
  | FunctionExpected _ -> 2
  | NotAFunction _  -> 3
  | IndexOutOfRangeExceptionWarning _ -> 4
  | FieldNotMutable  _ -> 5
  | Recursion _ -> 6
  | InvalidRuntimeCoercion _ -> 7
  | IndeterminateRuntimeCoercion _ -> 8
  | PossibleUnverifiableCode _ -> 9
  | SyntaxError _ -> 10
  | IndeterminateStaticCoercion  _ -> 13
  | StaticCoercionShouldUseBox  _ -> 14
  | RuntimeCoercionSourceSealed _ -> 16 
  | OverrideDoesntOverride _ -> 17
  | UnionPatternsBindDifferentNames _  -> 18
  | UnionCaseWrongArguments  _ -> 19
  | UnitTypeExpected _  -> 20
  | RecursiveUseCheckedAtRuntime  _ -> 21
  | LetRecEvaluatedOutOfOrder  _ -> 22
  | NameClash _ -> 23
  | Patcompile.MatchIncomplete _ -> 25
  | Patcompile.RuleNeverMatched _ -> 26
  | ValNotMutable _ -> 27
  | ValNotLocal _ -> 28
  | MissingFields _ -> 29
  | ValueRestriction _ -> 30
  | LetRecUnsound  _ -> 31
  | FieldsFromDifferentTypes  _ -> 32
  | TyconBadArgs _ -> 33
  | ValueNotContained _ -> 34
  | Deprecated  _ -> 35
  | ConstrNotContained _ -> 36
  | Duplicate _ -> 37
  | VarBoundTwice _  -> 38
  | UndefinedName _ -> 39
  | LetRecCheckedAtRuntime _ -> 40
  | UnresolvedOverloading _ -> 41
  | LibraryUseOnly _ -> 42
  | ErrorFromAddingConstraint _ -> 43
  | Obsolete _ -> 44
  | FullAbstraction _ -> 45
  | ReservedKeyword _ -> 46
  | SelfRefObjCtor _ -> 47
  | VirtualAugmentationOnNullValuedType _ -> 48
  | UpperCaseIdentifierInPattern _ -> 49
  | InterfaceNotRevealed _ -> 50
  | UseOfAddressOfOperator _ -> 51
  | DefensiveCopyWarning _ -> 52
  | NotUpperCaseConstructor _ -> 53
  | TypeIsImplicitlyAbstract _ -> 54
  | DeprecatedClassFieldInference _ -> 55
  | DeprecatedThreadStaticBindingWarning _ -> 56
  | Experimental _ -> 57
  | IndentationProblem _ -> 58
  | CoercionTargetSealed _ -> 59 
  | OverrideInIntrinsicAugmentation _ -> 60
  | NonVirtualAugmentationOnNullValuedType _ -> 61
  | OCamlCompatibility _ -> 62
  | ExnconstrNotContained _ -> 63
  | NonRigidTypar _ -> 64
  | UpcastUnnecessary _ -> 66
  | TypeTestUnnecessary _ -> 67
  | Creflect.IgnoringPartOfQuotedTermWarning _ -> 68
  | IntfImplInIntrinsicAugmentation _ -> 69
  | NonUniqueInferredAbstractSlot _ -> 70
  | ErrorFromApplyingDefault _ -> 71
  | IndeterminateType _ -> 72
  | InternalError _ -> 73
  | UnresolvedReferenceNoRange _
  | UnresolvedReferenceError _ 
  | UnresolvedPathReferenceNoRange _ 
  | UnresolvedPathReference _ -> 74
  | DeprecatedCommandLineOption _ -> 75
  | HashIncludeNotAllowedInNonScript _ 
  | HashReferenceNotAllowedInNonScript _ 
  | HashDirectiveNotAllowedInNonScript _
  | HashReferenceCopyAfterCompileNotAllowedInNonScript _ -> 76
  | BakedInMemberConstraintName _ -> 77
  | FileNameNotResolved _ -> 78  
  | LoadedSourceNotFoundIgnoring _ -> 79
  | ParameterlessStructCtor _ -> 81
  | MSBuildReferenceResolutionWarning _ -> 82
  | MSBuildReferenceResolutionError _ -> 83
  | AssemblyNotResolved _ -> 84
  | HashLoadedSourceHasIssues _ -> 85
  | StandardOperatorRedefinitionWarning _ -> 86
  | InvalidInternalsVisibleToAssemblyName _ -> 87
  | OverrideInExtrinsicAugmentation _ -> 89
  | IntfImplInExtrinsicAugmentation _ -> 90
  | BadEventTransformation _ -> 91
  | HashLoadedScriptConsideredSource _ -> 92

   (* DO NOT CHANGE THE NUMBERS *)

  // Strip TargetInvocationException wrappers
  | :? System.Reflection.TargetInvocationException as e -> 
      GetErrorNumber e.InnerException
  
  | WrappedError(e,_) -> GetErrorNumber e   

 (* These do not have good error numbers yet *)
  | Error  _ -> 191
  | Failure _ -> 192
  | _ -> 193

let warningOn err level = 
  match err with 
  // Level 4 warnings
  | RecursiveUseCheckedAtRuntime _
  | LetRecEvaluatedOutOfOrder  _
  | DefensiveCopyWarning _
  | FullAbstraction _ -> level > 3
  // Level 1 - 3 warnings
  | _ -> level > 0

let rec SplitRelatedErrors err = 
  match err with 
  | UnresolvedOverloading(a,overloads,bestOverloads,errors,b,c) -> 
       UnresolvedOverloading(a,[],[],[],b,c), (overloads@bestOverloads@errors)
  | ConstraintSolverRelatedInformation(fopt,m2,e) -> 
      let e,related = SplitRelatedErrors e
      ConstraintSolverRelatedInformation(fopt,m2,e), related
  | ErrorFromAddingTypeEquation(g,denv,t1,t2,e,m) ->
      let e,related = SplitRelatedErrors e
      ErrorFromAddingTypeEquation(g,denv,t1,t2,e,m) , related
  | ErrorFromApplyingDefault(g,denv,tp,defaultType,e,m) ->  
      let e,related = SplitRelatedErrors e
      ErrorFromApplyingDefault(g,denv,tp,defaultType,e,m) , related
  | ErrorsFromAddingSubsumptionConstraint(g,denv,t1,t2,e,m) ->  
      let e,related = SplitRelatedErrors e
      ErrorsFromAddingSubsumptionConstraint(g,denv,t1,t2,e,m), related
  | ErrorFromAddingConstraint(x,e,m) ->  
      let e,related = SplitRelatedErrors e
      ErrorFromAddingConstraint(x,e,m), related
  | WrappedError (e,m) -> 
      let e,related = SplitRelatedErrors e
      WrappedError(e,m), related
  // Strip TargetInvocationException wrappers
  | :? System.Reflection.TargetInvocationException as e -> 
      SplitRelatedErrors e.InnerException
  | _ -> 
       err, []
   


let stringsOfTypes denv ts = 
    let _,ts,tpcs = PrettyTypes.PrettifyTypesN denv.g ts
    let denvMin = { denv with showImperativeTyparAnnotations=false; showConstraintTyparAnnotations=false  }
    List.map (NicePrint.string_of_typ denvMin) ts 

// If the output text is different without showing constraints and/or imperative type variable 
// annotations and/or fully qualifying paths then don't show them! 
let minimalStringsOfTwoTypes denv t1 t2= 
    let _,(t1,t2),tpcs = PrettyTypes.PrettifyTypes2 denv.g (t1,t2)
    // try denv + no type annotations 
    let denvMin = { denv with showImperativeTyparAnnotations=false; showConstraintTyparAnnotations=false  }
    let min1 = NicePrint.string_of_typ denvMin t1
    let min2 = NicePrint.string_of_typ denvMin t2
    if min1 <> min2 then (min1,min2,"") else
    // try denv + no type annotations + show full paths
    let denvMinWithAllPaths = { denvMin with openTopPaths=[] }.Normalize()
    let min1 = NicePrint.string_of_typ denvMinWithAllPaths t1
    let min2 = NicePrint.string_of_typ denvMinWithAllPaths t2
    // try denv 
    if min1 <> min2 then (min1,min2,"") else
    let min1 = NicePrint.string_of_typ denv t1
    let min2 = NicePrint.string_of_typ denv t2
    if min1 <> min2 then (min1,min2,NicePrint.string_of_typar_constraints denv tpcs)  else
    // try denv + show full paths
    let denvWithAllPaths = { denv with openTopPaths=[] }.Normalize()
    let min1 = NicePrint.string_of_typ denv t1
    let min2 = NicePrint.string_of_typ denv t2
    (min1,min2,NicePrint.string_of_typar_constraints denv tpcs)  
    

// Note: Always show imperative annotations when comparing value signatures 
let minimalStringsOfTwoValues denv v1 v2= 
    let denvMin = { denv with showImperativeTyparAnnotations=true; showConstraintTyparAnnotations=false  }
    let min1 = bufs (fun buf -> NicePrint.output_qualified_val_spec denvMin buf v1)
    let min2 = bufs (fun buf -> NicePrint.output_qualified_val_spec denvMin buf v2) 
    if min1 <> min2 then (min1,min2) else
    let denvMax = { denv with showImperativeTyparAnnotations=true; showConstraintTyparAnnotations=true  }
    let max1 = bufs (fun buf -> NicePrint.output_qualified_val_spec denvMax buf v1)
    let max2 = bufs (fun buf -> NicePrint.output_qualified_val_spec denvMax buf v2) 
    max1,max2
    
let DeclareMesssage = DM.DeclareResourceString

let SeeAlsoE = DeclareResourceString("SeeAlso","%s")
let ConstraintSolverTupleDiffLengthsE = DeclareResourceString("ConstraintSolverTupleDiffLengths","%d%d")
let ConstraintSolverInfiniteTypesE = DeclareResourceString("ConstraintSolverInfiniteTypes", "%s%s")
let ConstraintSolverMissingConstraintE = DeclareResourceString("ConstraintSolverMissingConstraint","%s")
let ConstraintSolverTypesNotInEqualityRelation1E = DeclareResourceString("ConstraintSolverTypesNotInEqualityRelation1","%s%s")
let ConstraintSolverTypesNotInEqualityRelation2E = DeclareResourceString("ConstraintSolverTypesNotInEqualityRelation2", "%s%s")
let ConstraintSolverTypesNotInSubsumptionRelationE = DeclareResourceString("ConstraintSolverTypesNotInSubsumptionRelation","%s%s%s")
let ConstraintSolverErrorE = DeclareResourceString("ConstraintSolverError","%s")
let ErrorFromAddingTypeEquation1E = DeclareResourceString("ErrorFromAddingTypeEquation1","%s%s%s")
let ErrorFromAddingTypeEquation2E = DeclareResourceString("ErrorFromAddingTypeEquation2","%s%s%s")
let ErrorFromApplyingDefault1E = DeclareResourceString("ErrorFromApplyingDefault1","%s")
let ErrorFromApplyingDefault2E = DeclareResourceString("ErrorFromApplyingDefault2","")
let ErrorsFromAddingSubsumptionConstraintE = DeclareResourceString("ErrorsFromAddingSubsumptionConstraint","%s%s%s")
let UpperCaseIdentifierInPatternE = DeclareResourceString("UpperCaseIdentifierInPattern","")
let NotUpperCaseConstructorE = DeclareResourceString("NotUpperCaseConstructor","")
let UnresolvedOverloadingE = DeclareResourceString("UnresolvedOverloading","%s")
let PossibleOverloadE = DeclareResourceString("PossibleOverload","%s")
let FunctionExpectedE = DeclareResourceString("FunctionExpected","")
let BakedInMemberConstraintNameE = DeclareResourceString("BakedInMemberConstraintName","%s")
let IndexOutOfRangeExceptionWarningE = DeclareResourceString("IndexOutOfRangeExceptionWarning","")
let StandardOperatorRedefinitionWarningE = DeclareResourceString("StandardOperatorRedefinitionWarning","%s")
let BadEventTransformationE = DeclareResourceString("BadEventTransformation","")
let ParameterlessStructCtorE = DeclareResourceString("ParameterlessStructCtor","")
let InterfaceNotRevealedE = DeclareResourceString("InterfaceNotRevealed","%s")
let NotAFunction1E = DeclareResourceString("NotAFunction1","")
let NotAFunction2E = DeclareResourceString("NotAFunction2","")
let TyconBadArgsE = DeclareResourceString("TyconBadArgs","%s%d%d")
let IndeterminateTypeE = DeclareResourceString("IndeterminateType","")
let NameClash1E = DeclareResourceString("NameClash1","%s%s")
let NameClash2E = DeclareResourceString("NameClash2","%s%s%s%s%s")
let Duplicate1E = DeclareResourceString("Duplicate1","%s")
let Duplicate2E = DeclareResourceString("Duplicate2","%s%s")
let UndefinedName1E = DeclareResourceString("UndefinedName1","%s%s")
let UndefinedName2E = DeclareResourceString("UndefinedName2","")
let InternalUndefinedTyconItemE = DeclareResourceString("InternalUndefinedTyconItem","%s%s%s")
let InternalUndefinedItemRefE = DeclareResourceString("InternalUndefinedItemRef","%s%s%s%s")
let FieldNotMutableE = DeclareResourceString("FieldNotMutable","")
let FieldsFromDifferentTypesE = DeclareResourceString("FieldsFromDifferentTypes","%s%s")
let VarBoundTwiceE = DeclareResourceString("VarBoundTwice","%s")
let RecursionE = DeclareResourceString("Recursion","%s%s%s%s")
let InvalidRuntimeCoercionE = DeclareResourceString("InvalidRuntimeCoercion","%s%s%s")
let IndeterminateRuntimeCoercionE = DeclareResourceString("IndeterminateRuntimeCoercion","%s%s")
let IndeterminateStaticCoercionE = DeclareResourceString("IndeterminateStaticCoercion","%s%s")
let StaticCoercionShouldUseBoxE = DeclareResourceString("StaticCoercionShouldUseBox","%s%s")
let TypeIsImplicitlyAbstractE = DeclareResourceString("TypeIsImplicitlyAbstract","")
let NonRigidTypar1E = DeclareResourceString("NonRigidTypar1","%s%s")
let NonRigidTypar2E = DeclareResourceString("NonRigidTypar2","%s%s")
let NonRigidTypar3E = DeclareResourceString("NonRigidTypar3","%s%s")
let OBlockEndE = DeclareResourceString("Parser.TOKEN.OBLOCKEND","")
let UnexpectedEndOfInputE = DeclareResourceString("UnexpectedEndOfInput","")
let UnexpectedE = DeclareResourceString("Unexpected","%s")
let NONTERM_interactionE = DeclareResourceString("NONTERM.interaction","")
let NONTERM_hashDirectiveE = DeclareResourceString("NONTERM.hashDirective","")
let NONTERM_fieldDeclE = DeclareResourceString("NONTERM.fieldDecl","")
let NONTERM_unionCaseReprE = DeclareResourceString("NONTERM.unionCaseRepr","")
let NONTERM_localBindingE = DeclareResourceString("NONTERM.localBinding","")
let NONTERM_hardwhiteLetBindingsE = DeclareResourceString("NONTERM.hardwhiteLetBindings","")
let NONTERM_classDefnMemberE = DeclareResourceString("NONTERM.classDefnMember","")
let NONTERM_defnBindingsE = DeclareResourceString("NONTERM.defnBindings","")
let NONTERM_classMemberSpfnE = DeclareResourceString("NONTERM.classMemberSpfn","")
let NONTERM_valSpfnE = DeclareResourceString("NONTERM.valSpfn","")
let NONTERM_tyconSpfnE = DeclareResourceString("NONTERM.tyconSpfn","")
let NONTERM_anonLambdaExprE = DeclareResourceString("NONTERM.anonLambdaExpr","")
let NONTERM_attrUnionCaseDeclE = DeclareResourceString("NONTERM.attrUnionCaseDecl","")
let NONTERM_cPrototypeE = DeclareResourceString("NONTERM.cPrototype","")
let NONTERM_objectImplementationMembersE = DeclareResourceString("NONTERM.objectImplementationMembers","")
let NONTERM_ifExprCasesE = DeclareResourceString("NONTERM.ifExprCases","")
let NONTERM_openDeclE = DeclareResourceString("NONTERM.openDecl","")
let NONTERM_fileModuleSpecE = DeclareResourceString("NONTERM.fileModuleSpec","")
let NONTERM_patternClausesE = DeclareResourceString("NONTERM.patternClauses","")
let NONTERM_beginEndExprE = DeclareResourceString("NONTERM.beginEndExpr","")
let NONTERM_recdExprE = DeclareResourceString("NONTERM.recdExpr","")
let NONTERM_tyconDefnE = DeclareResourceString("NONTERM.tyconDefn","")
let NONTERM_exconCoreE = DeclareResourceString("NONTERM.exconCore","")
let NONTERM_typeNameInfoE = DeclareResourceString("NONTERM.typeNameInfo","")
let NONTERM_attributeListE = DeclareResourceString("NONTERM.attributeList","")
let NONTERM_quoteExprE = DeclareResourceString("NONTERM.quoteExpr","")
let NONTERM_typeConstraintE = DeclareResourceString("NONTERM.typeConstraint","")
let NONTERM_Category_ImplementationFileE = DeclareResourceString("NONTERM.Category.ImplementationFile","")
let NONTERM_Category_DefinitionE = DeclareResourceString("NONTERM.Category.Definition","")
let NONTERM_Category_SignatureFileE = DeclareResourceString("NONTERM.Category.SignatureFile","")
let NONTERM_Category_PatternE = DeclareResourceString("NONTERM.Category.Pattern","")
let NONTERM_Category_ExprE = DeclareResourceString("NONTERM.Category.Expr","")
let NONTERM_Category_TypeE = DeclareResourceString("NONTERM.Category.Type","")
let NONTERM_typeArgsActualE = DeclareResourceString("NONTERM.typeArgsActual","")
let TokenName1E = DeclareResourceString("TokenName1","%s")
let TokenName1TokenName2E = DeclareResourceString("TokenName1TokenName2","%s%s")
let TokenName1TokenName2TokenName3E = DeclareResourceString("TokenName1TokenName2TokenName3","%s%s%s")
let RuntimeCoercionSourceSealed1E = DeclareResourceString("RuntimeCoercionSourceSealed1","%s")
let RuntimeCoercionSourceSealed2E = DeclareResourceString("RuntimeCoercionSourceSealed2","%s")
let CoercionTargetSealedE = DeclareResourceString("CoercionTargetSealed","%s")
let UpcastUnnecessaryE = DeclareResourceString("UpcastUnnecessary","")
let TypeTestUnnecessaryE = DeclareResourceString("TypeTestUnnecessary","")
let IgnoringPartOfQuotedTermWarningE = DeclareResourceString("IgnoringPartOfQuotedTermWarning","%s")
let OverrideDoesntOverride1E = DeclareResourceString("OverrideDoesntOverride1","%s")
let OverrideDoesntOverride2E = DeclareResourceString("OverrideDoesntOverride2","%s")
let OverrideDoesntOverride3E = DeclareResourceString("OverrideDoesntOverride3","%s")
let UnionCaseWrongArgumentsE = DeclareResourceString("UnionCaseWrongArguments","%d%d")
let UnionPatternsBindDifferentNamesE = DeclareResourceString("UnionPatternsBindDifferentNames","")
let ValueNotContainedE = DeclareResourceString("ValueNotContained","%s%s%s%s")
let ConstrNotContainedE = DeclareResourceString("ConstrNotContained","%s%s%s")
let ExnconstrNotContainedE = DeclareResourceString("ExnconstrNotContained","%s%s%s")
let FieldNotContainedE = DeclareResourceString("FieldNotContained","%s%s%s")
let RequiredButNotSpecifiedE = DeclareResourceString("RequiredButNotSpecified","%s%s%s")
let UseOfAddressOfOperatorE = DeclareResourceString("UseOfAddressOfOperator","")
let DefensiveCopyWarningE = DeclareResourceString("DefensiveCopyWarning","%s")
let DeprecatedThreadStaticBindingWarningE = DeclareResourceString("DeprecatedThreadStaticBindingWarning","")
let DeprecatedClassFieldInferenceE = DeclareResourceString("DeprecatedClassFieldInference","")
let FunctionValueUnexpectedE = DeclareResourceString("FunctionValueUnexpected","%s")
let UnitTypeExpected1E = DeclareResourceString("UnitTypeExpected1","%s")
let UnitTypeExpected2E = DeclareResourceString("UnitTypeExpected2","")
let RecursiveUseCheckedAtRuntimeE = DeclareResourceString("RecursiveUseCheckedAtRuntime","")
let LetRecUnsound1E = DeclareResourceString("LetRecUnsound1","%s")
let LetRecUnsound2E = DeclareResourceString("LetRecUnsound2","%s%s")
let LetRecUnsoundInnerE = DeclareResourceString("LetRecUnsoundInner","%s")
let LetRecEvaluatedOutOfOrderE = DeclareResourceString("LetRecEvaluatedOutOfOrder","")
let LetRecCheckedAtRuntimeE = DeclareResourceString("LetRecCheckedAtRuntime","")
let SelfRefObjCtor1E = DeclareResourceString("SelfRefObjCtor1","")
let SelfRefObjCtor2E = DeclareResourceString("SelfRefObjCtor2","")
let VirtualAugmentationOnNullValuedTypeE = DeclareResourceString("VirtualAugmentationOnNullValuedType","")
let NonVirtualAugmentationOnNullValuedTypeE = DeclareResourceString("NonVirtualAugmentationOnNullValuedType","")
let NonUniqueInferredAbstractSlot1E = DeclareResourceString("NonUniqueInferredAbstractSlot1","%s")
let NonUniqueInferredAbstractSlot2E = DeclareResourceString("NonUniqueInferredAbstractSlot2","")
let NonUniqueInferredAbstractSlot3E = DeclareResourceString("NonUniqueInferredAbstractSlot3","%s%s")
let NonUniqueInferredAbstractSlot4E = DeclareResourceString("NonUniqueInferredAbstractSlot4","")
let ErrorE = DeclareResourceString("Error","%s")
let Failure3E = DeclareResourceString("Failure3","%s")
let Failure4E = DeclareResourceString("Failure4","%s")
let FullAbstractionE = DeclareResourceString("FullAbstraction","%s")
let MatchIncomplete1E = DeclareResourceString("MatchIncomplete1","")
let MatchIncomplete2E = DeclareResourceString("MatchIncomplete2","%s")
let MatchIncomplete3E = DeclareResourceString("MatchIncomplete3","%s")
let MatchIncomplete4E = DeclareResourceString("MatchIncomplete4","")
let RuleNeverMatchedE = DeclareResourceString("RuleNeverMatched","")
let ValNotMutableE = DeclareResourceString("ValNotMutable","")
let ValNotLocalE = DeclareResourceString("ValNotLocal","")
let Obsolete1E = DeclareResourceString("Obsolete1","")
let Obsolete2E = DeclareResourceString("Obsolete2","%s")
let ExperimentalE = DeclareResourceString("Experimental","%s")
let PossibleUnverifiableCodeE = DeclareResourceString("PossibleUnverifiableCode","")
let OCamlCompatibilityE = DeclareResourceString("OCamlCompatibility","%s")
let DeprecatedE = DeclareResourceString("Deprecated","%s")
let LibraryUseOnlyE = DeclareResourceString("LibraryUseOnly","")
let MissingFieldsE = DeclareResourceString("MissingFields","%s")
let ValueRestriction1E = DeclareResourceString("ValueRestriction1","%s%s%s")
let ValueRestriction2E = DeclareResourceString("ValueRestriction2","%s%s%s")
let ValueRestriction3E = DeclareResourceString("ValueRestriction3","%s")
let ValueRestriction4E = DeclareResourceString("ValueRestriction4","%s%s%s")
let ValueRestriction5E = DeclareResourceString("ValueRestriction5","%s%s%s")
let RecoverableParseErrorE = DeclareResourceString("RecoverableParseError","")
let ReservedKeywordE = DeclareResourceString("ReservedKeyword","%s")
let IndentationProblemE = DeclareResourceString("IndentationProblem","%s")
let OverrideInIntrinsicAugmentationE = DeclareResourceString("OverrideInIntrinsicAugmentation","")
let OverrideInExtrinsicAugmentationE = DeclareResourceString("OverrideInExtrinsicAugmentation","")
let IntfImplInIntrinsicAugmentationE = DeclareResourceString("IntfImplInIntrinsicAugmentation","")
let IntfImplInExtrinsicAugmentationE = DeclareResourceString("IntfImplInExtrinsicAugmentation","")
let UnresolvedReferenceNoRangeE = DeclareResourceString("UnresolvedReferenceNoRange","%s")
let UnresolvedPathReferenceNoRangeE = DeclareResourceString("UnresolvedPathReferenceNoRange","%s%s")
let DeprecatedCommandLineOptionE = DeclareResourceString("DeprecatedCommandLineOption","%s%s")
let HashIncludeNotAllowedInNonScriptE = DeclareResourceString("HashIncludeNotAllowedInNonScript","")
let HashReferenceNotAllowedInNonScriptE = DeclareResourceString("HashReferenceNotAllowedInNonScript","")
let HashReferenceCopyAfterCompileNotAllowedInNonScriptE = DeclareResourceString("HashReferenceCopyAfterCompileNotAllowedInNonScript","")
let HashDirectiveNotAllowedInNonScriptE = DeclareResourceString("HashDirectiveNotAllowedInNonScript","")
let FileNameNotResolvedE = DeclareResourceString("FileNameNotResolved","%s%s")
let AssemblyNotResolvedE = DeclareResourceString("AssemblyNotResolved","%s")
let HashLoadedSourceHasIssues1E = DeclareResourceString("HashLoadedSourceHasIssues1","")
let HashLoadedSourceHasIssues2E = DeclareResourceString("HashLoadedSourceHasIssues2","")
let HashLoadedScriptConsideredSourceE = DeclareResourceString("HashLoadedScriptConsideredSource","")  
let InvalidInternalsVisibleToAssemblyName1E = DeclareResourceString("InvalidInternalsVisibleToAssemblyName1","%s%s")
let InvalidInternalsVisibleToAssemblyName2E = DeclareResourceString("InvalidInternalsVisibleToAssemblyName2","%s")
let LoadedSourceNotFoundIgnoringE = DeclareResourceString("LoadedSourceNotFoundIgnoring","%s")
let MSBuildReferenceResolutionErrorE = DeclareResourceString("MSBuildReferenceResolutionError","%s%s")
let TargetInvocationExceptionWrapperE = DeclareResourceString("TargetInvocationExceptionWrapper","%s")

let getErrorString = SR.GetString

let rec OutputExceptionR (os:System.Text.StringBuilder) exn =
  match exn with 
  | ConstraintSolverTupleDiffLengths(denv,tl1,tl2,m,m2) -> 
      os.Append(ConstraintSolverTupleDiffLengthsE.Format (List.length tl1) (List.length tl2)) |> ignore;
      (if start_line_of_range m <> start_line_of_range m2 then 
         os.Append(SeeAlsoE.Format (string_of_range m)) |> ignore);
  | ConstraintSolverInfiniteTypes(denv,t1,t2,m,m2) ->
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv t1 t2
      os.Append(ConstraintSolverInfiniteTypesE.Format t1 t2)  |> ignore;
      (if start_line_of_range m <> start_line_of_range m2 then 
         os.Append(SeeAlsoE.Format (string_of_range m)) |> ignore );
  | ConstraintSolverMissingConstraint(denv,tpr,tpc,m,m2) -> 
      os.Append(ConstraintSolverMissingConstraintE.Format (NicePrint.string_of_typar_constraint denv (tpr,tpc))) |> ignore;
      (if start_line_of_range m <> start_line_of_range m2 then 
         os.Append(SeeAlsoE.Format (string_of_range m)) |> ignore );
  | ConstraintSolverTypesNotInEqualityRelation(denv,(TType_measure _ as t1),(TType_measure _ as t2),m,m2) -> 
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv t1 t2
      os.Append(ConstraintSolverTypesNotInEqualityRelation1E.Format t1 t2)  |> ignore;
      (if start_line_of_range m <> start_line_of_range m2 then 
         os.Append(SeeAlsoE.Format (string_of_range m))  |> ignore);
  | ConstraintSolverTypesNotInEqualityRelation(denv,t1,t2,m,m2) -> 
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv t1 t2
      os.Append(ConstraintSolverTypesNotInEqualityRelation2E.Format t1 t2)  |> ignore;
      (if start_line_of_range m <> start_line_of_range m2 then 
         os.Append(SeeAlsoE.Format (string_of_range m)) |> ignore);
  | ConstraintSolverTypesNotInSubsumptionRelation(denv,t1,t2,m,m2) -> 
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv t1 t2
      os.Append(ConstraintSolverTypesNotInSubsumptionRelationE.Format t2 t1 tpcs) |> ignore;
      (if start_line_of_range m <> start_line_of_range m2 then 
         os.Append(SeeAlsoE.Format (string_of_range m2)) |> ignore);
  | ConstraintSolverError(msg,m,m2) -> 
     os.Append(ConstraintSolverErrorE.Format msg) |> ignore;
      (if start_line_of_range m <> start_line_of_range m2 then 
         os.Append(SeeAlsoE.Format (string_of_range m2)) |> ignore);
  | ConstraintSolverRelatedInformation(fopt,m2,e) -> 
      match e with 
      | ConstraintSolverError _ -> OutputExceptionR os e;
      | _ -> ()
      fopt |> Option.iter (Printf.bprintf os " %s")
  | ErrorFromAddingTypeEquation(g,denv,t1,t2,ConstraintSolverTypesNotInEqualityRelation(_,t1',t2',m,_),_) 
     when type_equiv g t1 t1'
     &&   type_equiv g t2 t2' ->  
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv t1 t2
      os.Append(ErrorFromAddingTypeEquation1E.Format t2 t1 tpcs) |> ignore
  | ErrorFromAddingTypeEquation(g,denv,_,_,((ConstraintSolverTypesNotInSubsumptionRelation _ | ConstraintSolverError _) as e),m)  ->  
      OutputExceptionR os e;
  | ErrorFromAddingTypeEquation(g,denv,t1,t2,e,m) ->
      if not (type_equiv g t1 t2) then (
          let t1,t2,tpcs = minimalStringsOfTwoTypes denv t1 t2
          if t1<>t2 ^ tpcs then os.Append(ErrorFromAddingTypeEquation2E.Format t1 t2 tpcs) |> ignore;
      );
      OutputExceptionR os e
  | ErrorFromApplyingDefault(g,denv,tp,defaultType,e,m) ->  
      let defaultType = List.hd (stringsOfTypes denv [defaultType])
      os.Append(ErrorFromApplyingDefault1E.Format defaultType) |> ignore
      OutputExceptionR os e
      os.Append(ErrorFromApplyingDefault2E.Format) |> ignore
  | ErrorsFromAddingSubsumptionConstraint(g,denv,t1,t2,e,m) ->  
      if not (type_equiv g t1 t2) then (
          let t1,t2,tpcs = minimalStringsOfTwoTypes denv t1 t2
          if t1 <> (t2 ^ tpcs) then 
              os.Append(ErrorsFromAddingSubsumptionConstraintE.Format t2 t1 tpcs) |> ignore
      );
      OutputExceptionR os e
  | UpperCaseIdentifierInPattern(m) -> 
      os.Append(UpperCaseIdentifierInPatternE.Format) |> ignore
  | NotUpperCaseConstructor(m) -> 
      os.Append(NotUpperCaseConstructorE.Format) |> ignore
  | ErrorFromAddingConstraint(_,e,_) ->  
      OutputExceptionR os e;
  | UnresolvedOverloading(_,_,_,_,mtext,m) -> 
      os.Append(UnresolvedOverloadingE.Format mtext) |> ignore
  | PossibleOverload(_,minfo,m) -> 
      os.Append(PossibleOverloadE.Format minfo) |> ignore
  //| PossibleBestOverload(_,minfo,m) -> 
  //    Printf.bprintf os "\n\nPossible best overload: '%s'." minfo
  | FunctionExpected(denv,t,m) ->
      os.Append(FunctionExpectedE.Format) |> ignore
  | BakedInMemberConstraintName(nm,m) ->
      os.Append(BakedInMemberConstraintNameE.Format nm) |> ignore
  | IndexOutOfRangeExceptionWarning(m) ->
      os.Append(IndexOutOfRangeExceptionWarningE.Format) |> ignore
  | StandardOperatorRedefinitionWarning(msg,_) -> 
      os.Append(StandardOperatorRedefinitionWarningE.Format msg) |> ignore
  | BadEventTransformation(m) ->
     os.Append(BadEventTransformationE.Format) |> ignore
  | ParameterlessStructCtor(m) ->
     os.Append(ParameterlessStructCtorE.Format) |> ignore
  | InterfaceNotRevealed(denv,ity,m) ->
      os.Append(InterfaceNotRevealedE.Format (NicePrint.pretty_string_of_typ denv ity)) |> ignore
  | NotAFunction(denv,t,mfun,marg) ->
      if start_col_of_range marg = 0 then 
        os.Append(NotAFunction1E.Format) |> ignore
      else
        os.Append(NotAFunction2E.Format) |> ignore
      
  | TyconBadArgs(denv,tcref,d,m) -> 
      let exp = tcref.TyparsNoRange.Length
      os.Append(TyconBadArgsE.Format (full_display_text_of_tcref tcref) exp d) |> ignore
  | IndeterminateType(m) -> 
      os.Append(IndeterminateTypeE.Format) |> ignore
  | NameClash(nm,k1,nm1,m1,k2,nm2,m2) -> 
      if nm = nm1 && nm1 = nm2 && k1 = k2 then 
          os.Append(NameClash1E.Format k1 nm1) |> ignore
      else
          os.Append(NameClash2E.Format k1 nm1 nm k2 nm2) |> ignore
  | Duplicate(k,s,m)  -> 
      if k = "member" then 
          os.Append(Duplicate1E.Format (DecompileOpName s)) |> ignore
      else 
          os.Append(Duplicate2E.Format k (DecompileOpName s)) |> ignore
  | UndefinedName(_,k,id,avail) -> 
      os.Append(UndefinedName1E.Format k (DecompileOpName id.idText)) |> ignore
      if List.mem id.idText 
          [ "open_out"; "pred"; "succ"; "min_int"; "max_int"; "End_of_file"; "Out_of_memory";
            "Division_by_zero"; "Stack_overflow"; "Not_found"; "Match_failure"; "Assert_failure";
            "Invalid_argument"; "!="; "=="; "+."; "-."; "*."; "/."; "abs_float"; "max_float"; "min_float";
            "epsilon_float"; "mod_float"; "modf"; "nonempty"; "neg_infinity"; "ldexp"; "FP_normal";
            "classify_float"; "bool_of_string"; "char_of_int"; "int_of_char"; "int_of_string";
            "int_of_float"; "string_of_bool"; "string_of_float"; "string_of_int"; "float_of_int";
            "float_of_string"; "in_channel"; "open_in"; "open_in_bin"; "open_in_gen"; 
            "close_in";  "in_channel_length"; "input_byte"; "input_char"; "input_line"; "input_value";
            "pos_in"; "really_input"; "seek_in"; "set_binary_mode_in"; "unsafe_really_input"; "out_channel";
            "open_out"; "open_out_bin"; "open_out_gen"; "close_out"; "out_channel_length";
            "output_byte"; "output_string"; "output_value"; "pos_out"; "seek_out"; "set_binary_mode_out";
            "flush"; "prerr_char"; "prerr_endline"; "prerr_float"; "prerr_int"; "prerr_newline";
            "prerr_string"; "print_char"; "print_endline"; "print_float"; "print_int"; "print_newline";
            "mem"; "assoc"; "try_assoc"; "mem_assoc"; "remove_assoc"; "assq";"try_assq"; "mem_assq";
            "remove_assq"; "memq"; "rev_map"; "rev_map2"; "rev_append"; "scan1_left"; "scan1_right";
            "print_string"; "read_float"; "read_int"; "read_line"; "Pervasives"; "Hashtbl"; "Buffer"; "Tagged"; "HashSet";
            "Parsing"; "Lexing"; "Char"; "Int32"; "Int64"; "UInt32"; "UInt64"; "Int16"; "UInt16"; "Byte";
            "SByte"; "Int8"; "UInt8"; "Obj"; "Num"; "Filename"; "ReadonlyArray"; "Roarray"; "LazyList"; "Big_int"; "Sys";
            "Printexc"; "Float"; "Float32"; "Matrix"; "Vector"; "Complex"; "generate_using"; "generate"; "complex"; "RowVector"; "matrix"; "vector"; "rowvec"; 
            // String.* functions
            "split"; "lowercase"; "contains"; "contains_between"; "contains_from"; "capitalize"; "uncapitalize"; "uppercase"; "trim" ] then 
          os.Append(UndefinedName2E.Format) |> ignore
            
  | InternalUndefinedTyconItem(k,tcref,s) ->
      os.Append(InternalUndefinedTyconItemE.Format (full_display_text_of_tcref tcref) k s) |> ignore
  | InternalUndefinedItemRef(k,smr,ccuName,s) ->
      os.Append(InternalUndefinedItemRefE.Format smr ccuName k s) |> ignore
  | FieldNotMutable (denv,fref,m) -> 
      os.Append(FieldNotMutableE.Format) |> ignore
  | FieldsFromDifferentTypes (denv,fref1,fref2,m) -> 
      os.Append(FieldsFromDifferentTypesE.Format fref1.FieldName fref2.FieldName) |> ignore
  | VarBoundTwice(id) ->  
      os.Append(VarBoundTwiceE.Format (DecompileOpName id.idText)) |> ignore
  | Recursion (denv,id,ty1,ty2,m) -> 
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv ty1 ty2
      os.Append(RecursionE.Format (DecompileOpName id.idText) t1 t2 tpcs) |> ignore
  | InvalidRuntimeCoercion(denv,ty1,ty2,m) -> 
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv ty1 ty2
      os.Append(InvalidRuntimeCoercionE.Format t1 t2 tpcs) |> ignore
  | IndeterminateRuntimeCoercion(denv,ty1,ty2,m) -> 
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv ty1 ty2
      os.Append(IndeterminateRuntimeCoercionE.Format t1 t2) |> ignore
  | IndeterminateStaticCoercion(denv,ty1,ty2,m) -> 
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv ty1 ty2
      os.Append(IndeterminateStaticCoercionE.Format t1 t2) |> ignore
  | StaticCoercionShouldUseBox(denv,ty1,ty2,m) ->
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv ty1 ty2
      os.Append(StaticCoercionShouldUseBoxE.Format t1 t2) |> ignore
  | TypeIsImplicitlyAbstract(m) -> 
      os.Append(TypeIsImplicitlyAbstractE.Format) |> ignore
  | NonRigidTypar(denv,tpnmOpt,typarRange,ty1,ty,m) -> 
      let _,(ty1,ty),tpcs = PrettyTypes.PrettifyTypes2 denv.g (ty1,ty)
      match tpnmOpt with 
      | None -> 
          os.Append(NonRigidTypar1E.Format (string_of_range typarRange) (NicePrint.string_of_typ denv ty)) |> ignore
      | Some tpnm -> 
          match ty1 with 
          | TType_measure _ -> 
            os.Append(NonRigidTypar2E.Format tpnm  (NicePrint.string_of_typ denv ty)) |> ignore
          | _ -> 
            os.Append(NonRigidTypar3E.Format tpnm  (NicePrint.string_of_typ denv ty)) |> ignore
  | SyntaxError (ctxt,m) -> 
      let ctxt = unbox<Parsing.ParseErrorContext<Parser.token>>(ctxt)
      let tokenIdToText tid = 
          match tid with 
          | Parser.TOKEN_IDENT -> getErrorString("Parser.TOKEN.IDENT")
          | Parser.TOKEN_BIGNUM 
          | Parser.TOKEN_INT8  
          | Parser.TOKEN_UINT8 
          | Parser.TOKEN_INT16  
          | Parser.TOKEN_UINT16 
          | Parser.TOKEN_INT32 
          | Parser.TOKEN_UINT32 
          | Parser.TOKEN_INT64 
          | Parser.TOKEN_UINT64 
          | Parser.TOKEN_UNATIVEINT 
          | Parser.TOKEN_NATIVEINT -> getErrorString("Parser.TOKEN.INT")
          | Parser.TOKEN_IEEE32 
          | Parser.TOKEN_IEEE64 -> getErrorString("Parser.TOKEN.FLOAT")
          | Parser.TOKEN_DECIMAL -> getErrorString("Parser.TOKEN.DECIMAL")
          | Parser.TOKEN_CHAR -> getErrorString("Parser.TOKEN.CHAR")
            
          | Parser.TOKEN_BASE -> getErrorString("Parser.TOKEN.BASE")
          | Parser.TOKEN_LPAREN_STAR_RPAREN -> getErrorString("Parser.TOKEN.LPAREN.STAR.RPAREN")
          | Parser.TOKEN_DOLLAR -> getErrorString("Parser.TOKEN.DOLLAR")
          | Parser.TOKEN_INFIX_STAR_STAR_OP -> getErrorString("Parser.TOKEN.INFIX.STAR.STAR.OP")
          | Parser.TOKEN_INFIX_COMPARE_OP -> getErrorString("Parser.TOKEN.INFIX.COMPARE.OP")
          | Parser.TOKEN_COLON_GREATER -> getErrorString("Parser.TOKEN.COLON.GREATER")  
          | Parser.TOKEN_COLON_COLON  ->getErrorString("Parser.TOKEN.COLON.COLON")
          | Parser.TOKEN_PERCENT_OP -> getErrorString("Parser.TOKEN.PERCENT.OP")
          | Parser.TOKEN_INFIX_AT_HAT_OP -> getErrorString("Parser.TOKEN.INFIX.AT.HAT.OP")
          | Parser.TOKEN_INFIX_BAR_OP -> getErrorString("Parser.TOKEN.INFIX.BAR.OP")
          | Parser.TOKEN_PLUS_MINUS_OP -> getErrorString("Parser.TOKEN.PLUS.MINUS.OP")
          | Parser.TOKEN_PREFIX_OP -> getErrorString("Parser.TOKEN.PREFIX.OP")
          | Parser.TOKEN_COLON_QMARK_GREATER   -> getErrorString("Parser.TOKEN.COLON.QMARK.GREATER")
          | Parser.TOKEN_INFIX_STAR_DIV_MOD_OP -> getErrorString("Parser.TOKEN.INFIX.STAR.DIV.MOD.OP")
          | Parser.TOKEN_INFIX_AMP_OP -> getErrorString("Parser.TOKEN.INFIX.AMP.OP")
          | Parser.TOKEN_AMP   -> getErrorString("Parser.TOKEN.AMP")
          | Parser.TOKEN_AMP_AMP  -> getErrorString("Parser.TOKEN.AMP.AMP")
          | Parser.TOKEN_BAR_BAR  -> getErrorString("Parser.TOKEN.BAR.BAR")
          | Parser.TOKEN_LESS   -> getErrorString("Parser.TOKEN.LESS")
          | Parser.TOKEN_GREATER  -> getErrorString("Parser.TOKEN.GREATER")
          | Parser.TOKEN_QMARK   -> getErrorString("Parser.TOKEN.QMARK")
          | Parser.TOKEN_QMARK_QMARK -> getErrorString("Parser.TOKEN.QMARK.QMARK")
          | Parser.TOKEN_COLON_QMARK-> getErrorString("Parser.TOKEN.COLON.QMARK")
          | Parser.TOKEN_INT32_DOT_DOT -> getErrorString("Parser.TOKEN.INT32.DOT.DOT")
          | Parser.TOKEN_DOT_DOT       -> getErrorString("Parser.TOKEN.DOT.DOT")
          | Parser.TOKEN_QUOTE   -> getErrorString("Parser.TOKEN.QUOTE")
          | Parser.TOKEN_STAR  -> getErrorString("Parser.TOKEN.STAR")
          | Parser.TOKEN_HIGH_PRECEDENCE_TYAPP  -> getErrorString("Parser.TOKEN.HIGH.PRECEDENCE.TYAPP")
          | Parser.TOKEN_COLON    -> getErrorString("Parser.TOKEN.COLON")
          | Parser.TOKEN_COLON_EQUALS   -> getErrorString("Parser.TOKEN.COLON.EQUALS")
          | Parser.TOKEN_LARROW   -> getErrorString("Parser.TOKEN.LARROW")
          | Parser.TOKEN_EQUALS -> getErrorString("Parser.TOKEN.EQUALS")
          | Parser.TOKEN_GREATER_DOT -> getErrorString("Parser.TOKEN.GREATER.DOT")
          | Parser.TOKEN_GREATER_BAR_RBRACK -> getErrorString("Parser.TOKEN.GREATER.BAR.RBRACK")
          | Parser.TOKEN_MINUS -> getErrorString("Parser.TOKEN.MINUS")
          | Parser.TOKEN_ADJACENT_PREFIX_PLUS_MINUS_OP    -> getErrorString("Parser.TOKEN.ADJACENT.PREFIX.PLUS.MINUS.OP")
          | Parser.TOKEN_FUNKY_OPERATOR_NAME -> getErrorString("Parser.TOKEN.FUNKY.OPERATOR.NAME") 
          | Parser.TOKEN_COMMA-> getErrorString("Parser.TOKEN.COMMA")
          | Parser.TOKEN_DOT -> getErrorString("Parser.TOKEN.DOT")
          | Parser.TOKEN_BAR-> getErrorString("Parser.TOKEN.BAR")
          | Parser.TOKEN_HASH -> getErrorString("Parser.TOKEN.HASH")
          | Parser.TOKEN_UNDERSCORE   -> getErrorString("Parser.TOKEN.UNDERSCORE")
          | Parser.TOKEN_SEMICOLON   -> getErrorString("Parser.TOKEN.SEMICOLON")
          | Parser.TOKEN_SEMICOLON_SEMICOLON-> getErrorString("Parser.TOKEN.SEMICOLON.SEMICOLON")
          | Parser.TOKEN_LPAREN-> getErrorString("Parser.TOKEN.LPAREN")
          | Parser.TOKEN_RPAREN -> getErrorString("Parser.TOKEN.RPAREN")
          | Parser.TOKEN_SPLICE_SYMBOL -> getErrorString("Parser.TOKEN.SPLICE.SYMBOL")
          | Parser.TOKEN_LQUOTE  -> getErrorString("Parser.TOKEN.LQUOTE")
          | Parser.TOKEN_LBRACK  -> getErrorString("Parser.TOKEN.LBRACK")
          | Parser.TOKEN_LBRACK_BAR  -> getErrorString("Parser.TOKEN.LBRACK.BAR")
          | Parser.TOKEN_LBRACK_LESS  -> getErrorString("Parser.TOKEN.LBRACK.LESS")
          | Parser.TOKEN_LBRACE   -> getErrorString("Parser.TOKEN.LBRACE")
          | Parser.TOKEN_LBRACE_LESS-> getErrorString("Parser.TOKEN.LBRACE.LESS")
          | Parser.TOKEN_BAR_RBRACK   -> getErrorString("Parser.TOKEN.BAR.RBRACK")
          | Parser.TOKEN_GREATER_RBRACE   -> getErrorString("Parser.TOKEN.GREATER.RBRACE")
          | Parser.TOKEN_GREATER_RBRACK  -> getErrorString("Parser.TOKEN.GREATER.RBRACK")
          | Parser.TOKEN_RQUOTE_DOT _ 
          | Parser.TOKEN_RQUOTE  -> getErrorString("Parser.TOKEN.RQUOTE")
          | Parser.TOKEN_RBRACK  -> getErrorString("Parser.TOKEN.RBRACK")
          | Parser.TOKEN_RBRACE  -> getErrorString("Parser.TOKEN.RBRACE")
          | Parser.TOKEN_PUBLIC -> getErrorString("Parser.TOKEN.PUBLIC")
          | Parser.TOKEN_PRIVATE -> getErrorString("Parser.TOKEN.PRIVATE")
          | Parser.TOKEN_INTERNAL -> getErrorString("Parser.TOKEN.INTERNAL")
          | Parser.TOKEN_CONSTRAINT -> getErrorString("Parser.TOKEN.CONSTRAINT")
          | Parser.TOKEN_INSTANCE -> getErrorString("Parser.TOKEN.INSTANCE")
          | Parser.TOKEN_DELEGATE -> getErrorString("Parser.TOKEN.DELEGATE")
          | Parser.TOKEN_INHERIT -> getErrorString("Parser.TOKEN.INHERIT")
          | Parser.TOKEN_CONSTRUCTOR-> getErrorString("Parser.TOKEN.CONSTRUCTOR")
          | Parser.TOKEN_DEFAULT -> getErrorString("Parser.TOKEN.DEFAULT")
          | Parser.TOKEN_OVERRIDE-> getErrorString("Parser.TOKEN.OVERRIDE")
          | Parser.TOKEN_ABSTRACT-> getErrorString("Parser.TOKEN.ABSTRACT")
          | Parser.TOKEN_VIRTUAL-> getErrorString("Parser.TOKEN.VIRTUAL")
          | Parser.TOKEN_CLASS-> getErrorString("Parser.TOKEN.CLASS")
          | Parser.TOKEN_MEMBER -> getErrorString("Parser.TOKEN.MEMBER")
          | Parser.TOKEN_STATIC -> getErrorString("Parser.TOKEN.STATIC")
          | Parser.TOKEN_NAMESPACE-> getErrorString("Parser.TOKEN.NAMESPACE")
          | Parser.TOKEN_OBLOCKBEGIN  -> getErrorString("Parser.TOKEN.OBLOCKBEGIN") 
          | Parser.TOKEN_ODECLEND 
          | Parser.TOKEN_OBLOCKSEP 
          | Parser.TOKEN_OEND 
          | Parser.TOKEN_ORIGHT_BLOCK_END 
          | Parser.TOKEN_OBLOCKEND -> getErrorString("Parser.TOKEN.OBLOCKEND") 
          | Parser.TOKEN_THEN  
          | Parser.TOKEN_OTHEN -> getErrorString("Parser.TOKEN.OTHEN")
          | Parser.TOKEN_ELSE
          | Parser.TOKEN_OELSE -> getErrorString("Parser.TOKEN.OELSE")
          | Parser.TOKEN_LET(_) 
          | Parser.TOKEN_OLET(_)  -> getErrorString("Parser.TOKEN.OLET")
          | Parser.TOKEN_OBINDER 
          | Parser.TOKEN_BINDER -> getErrorString("Parser.TOKEN.BINDER")
          | Parser.TOKEN_ODO -> getErrorString("Parser.TOKEN.ODO")
          | Parser.TOKEN_OWITH -> getErrorString("Parser.TOKEN.OWITH")
          | Parser.TOKEN_OFUNCTION -> getErrorString("Parser.TOKEN.OFUNCTION")
          | Parser.TOKEN_OFUN -> getErrorString("Parser.TOKEN.OFUN")
          | Parser.TOKEN_ORESET -> getErrorString("Parser.TOKEN.ORESET")
          | Parser.TOKEN_ODUMMY -> getErrorString("Parser.TOKEN.ODUMMY")
          | Parser.TOKEN_DO_BANG 
          | Parser.TOKEN_ODO_BANG -> getErrorString("Parser.TOKEN.ODO.BANG")
          | Parser.TOKEN_YIELD -> getErrorString("Parser.TOKEN.YIELD")
          | Parser.TOKEN_YIELD_BANG  -> getErrorString("Parser.TOKEN.YIELD.BANG")
          | Parser.TOKEN_OINTERFACE_MEMBER-> getErrorString("Parser.TOKEN.OINTERFACE.MEMBER")
          | Parser.TOKEN_ELIF -> getErrorString("Parser.TOKEN.ELIF")
          | Parser.TOKEN_RARROW -> getErrorString("Parser.TOKEN.RARROW")
          | Parser.TOKEN_RARROW2 -> getErrorString("Parser.TOKEN.RARROW2")
          | Parser.TOKEN_SIG -> getErrorString("Parser.TOKEN.SIG")
          | Parser.TOKEN_STRUCT  -> getErrorString("Parser.TOKEN.STRUCT")
          | Parser.TOKEN_UPCAST   -> getErrorString("Parser.TOKEN.UPCAST")
          | Parser.TOKEN_DOWNCAST   -> getErrorString("Parser.TOKEN.DOWNCAST")
          | Parser.TOKEN_NULL   -> getErrorString("Parser.TOKEN.NULL")
          | Parser.TOKEN_RESERVED    -> getErrorString("Parser.TOKEN.RESERVED")
          | Parser.TOKEN_MODULE    -> getErrorString("Parser.TOKEN.MODULE")
          | Parser.TOKEN_AND    -> getErrorString("Parser.TOKEN.AND")
          | Parser.TOKEN_AS   -> getErrorString("Parser.TOKEN.AS")
          | Parser.TOKEN_ASSERT   -> getErrorString("Parser.TOKEN.ASSERT")
          | Parser.TOKEN_ASR-> getErrorString("Parser.TOKEN.ASR")
          | Parser.TOKEN_DOWNTO   -> getErrorString("Parser.TOKEN.DOWNTO")
          | Parser.TOKEN_EXCEPTION   -> getErrorString("Parser.TOKEN.EXCEPTION")
          | Parser.TOKEN_FALSE   -> getErrorString("Parser.TOKEN.FALSE")
          | Parser.TOKEN_FOR   -> getErrorString("Parser.TOKEN.FOR")
          | Parser.TOKEN_FUN   -> getErrorString("Parser.TOKEN.FUN")
          | Parser.TOKEN_FUNCTION-> getErrorString("Parser.TOKEN.FUNCTION")
          | Parser.TOKEN_FINALLY   -> getErrorString("Parser.TOKEN.FINALLY")
          | Parser.TOKEN_LAZY   -> getErrorString("Parser.TOKEN.LAZY")
          | Parser.TOKEN_MATCH   -> getErrorString("Parser.TOKEN.MATCH")
          | Parser.TOKEN_METHOD   -> getErrorString("Parser.TOKEN.METHOD")
          | Parser.TOKEN_MUTABLE   -> getErrorString("Parser.TOKEN.MUTABLE")
          | Parser.TOKEN_NEW   -> getErrorString("Parser.TOKEN.NEW")
          | Parser.TOKEN_OF    -> getErrorString("Parser.TOKEN.OF")
          | Parser.TOKEN_OPEN   -> getErrorString("Parser.TOKEN.OPEN")
          | Parser.TOKEN_OR -> getErrorString("Parser.TOKEN.OR")
          | Parser.TOKEN_VOID -> getErrorString("Parser.TOKEN.VOID")
          | Parser.TOKEN_EXTERN-> getErrorString("Parser.TOKEN.EXTERN")
          | Parser.TOKEN_INTERFACE -> getErrorString("Parser.TOKEN.INTERFACE")
          | Parser.TOKEN_REC   -> getErrorString("Parser.TOKEN.REC")
          | Parser.TOKEN_TO   -> getErrorString("Parser.TOKEN.TO")
          | Parser.TOKEN_TRUE   -> getErrorString("Parser.TOKEN.TRUE")
          | Parser.TOKEN_TRY   -> getErrorString("Parser.TOKEN.TRY")
          | Parser.TOKEN_TYPE   -> getErrorString("Parser.TOKEN.TYPE")
          | Parser.TOKEN_VAL   -> getErrorString("Parser.TOKEN.VAL")
          | Parser.TOKEN_INLINE   -> getErrorString("Parser.TOKEN.INLINE")
          | Parser.TOKEN_WHEN  -> getErrorString("Parser.TOKEN.WHEN")
          | Parser.TOKEN_WHILE   -> getErrorString("Parser.TOKEN.WHILE")
          | Parser.TOKEN_WITH-> getErrorString("Parser.TOKEN.WITH")
          | Parser.TOKEN_IF -> getErrorString("Parser.TOKEN.IF")
          | Parser.TOKEN_DO -> getErrorString("Parser.TOKEN.DO")
          | Parser.TOKEN_DONE -> getErrorString("Parser.TOKEN.DONE")
          | Parser.TOKEN_IN -> getErrorString("Parser.TOKEN.IN")
          | Parser.TOKEN_HIGH_PRECEDENCE_APP-> getErrorString("Parser.TOKEN.HIGH.PRECEDENCE.APP")
          | Parser.TOKEN_BEGIN  -> getErrorString("Parser.TOKEN.BEGIN")
          | Parser.TOKEN_END -> getErrorString("Parser.TOKEN.END")
          | Parser.TOKEN_HASH_LIGHT
          | Parser.TOKEN_HASH_LINE 
          | Parser.TOKEN_HASH_IF 
          | Parser.TOKEN_HASH_ELSE 
          | Parser.TOKEN_HASH_ENDIF  -> getErrorString("Parser.TOKEN.HASH.ENDIF")
          | Parser.TOKEN_INACTIVECODE -> getErrorString("Parser.TOKEN.INACTIVECODE")
          | Parser.TOKEN_LEX_FAILURE-> getErrorString("Parser.TOKEN.LEX.FAILURE")
          | Parser.TOKEN_WHITESPACE -> getErrorString("Parser.TOKEN.WHITESPACE")
          | Parser.TOKEN_COMMENT -> getErrorString("Parser.TOKEN.COMMENT")
          | Parser.TOKEN_LINE_COMMENT -> getErrorString("Parser.TOKEN.LINE.COMMENT")
          | Parser.TOKEN_STRING_TEXT -> getErrorString("Parser.TOKEN.STRING.TEXT")
          | Parser.TOKEN_BYTEARRAY -> getErrorString("Parser.TOKEN.BYTEARRAY")
          | Parser.TOKEN_STRING -> getErrorString("Parser.TOKEN.STRING")
          | Parser.TOKEN_EOF -> getErrorString("Parser.TOKEN.EOF")
          | unknown -> sprintf "%A" unknown

      match ctxt.CurrentToken with 
      | None -> os.Append(UnexpectedEndOfInputE.Format) |> ignore
      | Some token -> 
          match (token |> Parser.tagOfToken |> Parser.tokenTagToTokenId), token with 
          | (Parser.TOKEN_ORIGHT_BLOCK_END | Parser.TOKEN_OBLOCKEND),_ -> os.Append(OBlockEndE.Format) |> ignore
          | Parser.TOKEN_LEX_FAILURE, Parser.LEX_FAILURE str -> Printf.bprintf os "%s" str (* Fix bug://2431 *)
          | token,_ -> os.Append(UnexpectedE.Format (token |> tokenIdToText)) |> ignore

      (* Search for a state producing a single recognized non-terminal in the states on the stack *)
      let foundInContext =
          
          (* Merge a bunch of expression non terminals *)
          let (|NONTERM_Category_Expr|_|) = function
                | Parser.NONTERM_argExpr|Parser.NONTERM_minusExpr|Parser.NONTERM_parenExpr|Parser.NONTERM_atomicExpr
                | Parser.NONTERM_appExpr|Parser.NONTERM_tupleExpr|Parser.NONTERM_declExpr|Parser.NONTERM_braceExpr
                | Parser.NONTERM_typedSeqExprBlock
                | Parser.NONTERM_interactiveExpr -> Some()
                | _ -> None
                
          (* Merge a bunch of pattern non terminals *)
          let (|NONTERM_Category_Pattern|_|) = function 
                | Parser.NONTERM_constrPattern|Parser.NONTERM_parenPattern|Parser.NONTERM_atomicPattern -> Some() 
                | _ -> None
          
          (* Merge a bunch of if/then/else non terminals *)
          let (|NONTERM_Category_IfThenElse|_|) = function
                | Parser.NONTERM_ifExprThen|Parser.NONTERM_ifExprElifs|Parser.NONTERM_ifExprCases -> Some()
                | _ -> None
                
          (* Merge a bunch of non terminals *)
          let (|NONTERM_Category_SignatureFile|_|) = function
                | Parser.NONTERM_signatureFile|Parser.NONTERM_moduleSpfn|Parser.NONTERM_moduleSpfns -> Some()
                | _ -> None
          let (|NONTERM_Category_ImplementationFile|_|) = function
                | Parser.NONTERM_implementationFile|Parser.NONTERM_fileNamespaceImpl|Parser.NONTERM_fileNamespaceImpls -> Some()
                | _ -> None
          let (|NONTERM_Category_Definition|_|) = function
                | Parser.NONTERM_fileModuleImpl|Parser.NONTERM_moduleDefn|Parser.NONTERM_interactiveModuleDefns
                |Parser.NONTERM_moduleDefns|Parser.NONTERM_moduleDefnsOrExpr -> Some()
                | _ -> None
          
          let (|NONTERM_Category_Type|_|) = function
                | Parser.NONTERM_typ|Parser.NONTERM_tupleType -> Some()
                | _ -> None

          let (|NONTERM_Category_Interaction|_|) = function
                | Parser.NONTERM_interactiveItemsTerminator|Parser.NONTERM_interaction|Parser.NONTERM__startinteraction -> Some()
                | _ -> None
 
          
          // Canonicalize the categories and check for a unique category
          ctxt.ReducibleProductions |> List.exists (fun prods -> 
              match prods 
                    |> List.map Parser.prodIdxToNonTerminal 
                    |> List.map (function 
                                 | NONTERM_Category_Type -> Parser.NONTERM_typ
                                 | NONTERM_Category_Expr -> Parser.NONTERM_declExpr 
                                 | NONTERM_Category_Pattern -> Parser.NONTERM_atomicPattern 
                                 | NONTERM_Category_IfThenElse -> Parser.NONTERM_ifExprThen
                                 | NONTERM_Category_SignatureFile -> Parser.NONTERM_signatureFile
                                 | NONTERM_Category_ImplementationFile -> Parser.NONTERM_implementationFile
                                 | NONTERM_Category_Definition -> Parser.NONTERM_moduleDefn
                                 | NONTERM_Category_Interaction -> Parser.NONTERM_interaction
                                 | nt -> nt)
                    |> Set.of_list 
                    |> Set.to_list with 
              | [Parser.NONTERM_interaction] -> os.Append(NONTERM_interactionE.Format) |> ignore; true
              | [Parser.NONTERM_hashDirective] -> os.Append(NONTERM_hashDirectiveE.Format) |> ignore; true
              | [Parser.NONTERM_fieldDecl] -> os.Append(NONTERM_fieldDeclE.Format) |> ignore; true
              | [Parser.NONTERM_unionCaseRepr] -> os.Append(NONTERM_unionCaseReprE.Format) |> ignore; true
              | [Parser.NONTERM_localBinding] -> os.Append(NONTERM_localBindingE.Format) |> ignore; true
              | [Parser.NONTERM_hardwhiteLetBindings] -> os.Append(NONTERM_hardwhiteLetBindingsE.Format) |> ignore; true
              | [Parser.NONTERM_classDefnMember] -> os.Append(NONTERM_classDefnMemberE.Format) |> ignore; true
              | [Parser.NONTERM_defnBindings] -> os.Append(NONTERM_defnBindingsE.Format) |> ignore; true
              | [Parser.NONTERM_classMemberSpfn] -> os.Append(NONTERM_classMemberSpfnE.Format) |> ignore; true
              | [Parser.NONTERM_valSpfn] -> os.Append(NONTERM_valSpfnE.Format) |> ignore; true
              | [Parser.NONTERM_tyconSpfn] -> os.Append(NONTERM_tyconSpfnE.Format) |> ignore; true
              | [Parser.NONTERM_anonLambdaExpr] -> os.Append(NONTERM_anonLambdaExprE.Format) |> ignore; true
              | [Parser.NONTERM_attrUnionCaseDecl] -> os.Append(NONTERM_attrUnionCaseDeclE.Format) |> ignore; true
              | [Parser.NONTERM_cPrototype] -> os.Append(NONTERM_cPrototypeE.Format) |> ignore; true
              | [Parser.NONTERM_objExpr|Parser.NONTERM_objectImplementationMembers] -> os.Append(NONTERM_objectImplementationMembersE.Format) |> ignore; true
              | [Parser.NONTERM_ifExprThen|Parser.NONTERM_ifExprElifs|Parser.NONTERM_ifExprCases] -> os.Append(NONTERM_ifExprCasesE.Format) |> ignore; true
              | [Parser.NONTERM_openDecl] -> os.Append(NONTERM_openDeclE.Format) |> ignore; true
              | [Parser.NONTERM_fileModuleSpec] -> os.Append(NONTERM_fileModuleSpecE.Format) |> ignore; true
              | [Parser.NONTERM_patternClauses] -> os.Append(NONTERM_patternClausesE.Format) |> ignore; true
              | [Parser.NONTERM_beginEndExpr] -> os.Append(NONTERM_beginEndExprE.Format) |> ignore; true
              | [Parser.NONTERM_recdExpr] -> os.Append(NONTERM_recdExprE.Format) |> ignore; true
              | [Parser.NONTERM_tyconDefn] -> os.Append(NONTERM_tyconDefnE.Format) |> ignore; true
              | [Parser.NONTERM_exconCore] -> os.Append(NONTERM_exconCoreE.Format) |> ignore; true
              | [Parser.NONTERM_typeNameInfo] -> os.Append(NONTERM_typeNameInfoE.Format) |> ignore; true
              | [Parser.NONTERM_attributeList] -> os.Append(NONTERM_attributeListE.Format) |> ignore; true
              | [Parser.NONTERM_quoteExpr] -> os.Append(NONTERM_quoteExprE.Format) |> ignore; true
              | [Parser.NONTERM_typeConstraint] -> os.Append(NONTERM_typeConstraintE.Format) |> ignore; true
              | [NONTERM_Category_ImplementationFile] -> os.Append(NONTERM_Category_ImplementationFileE.Format) |> ignore; true
              | [NONTERM_Category_Definition] -> os.Append(NONTERM_Category_DefinitionE.Format) |> ignore; true
              | [NONTERM_Category_SignatureFile] -> os.Append(NONTERM_Category_SignatureFileE.Format) |> ignore; true
              | [NONTERM_Category_Pattern] -> os.Append(NONTERM_Category_PatternE.Format) |> ignore; true
              | [NONTERM_Category_Expr] ->  os.Append(NONTERM_Category_ExprE.Format) |> ignore; true
              | [NONTERM_Category_Type] ->  os.Append(NONTERM_Category_TypeE.Format) |> ignore; true
              | [Parser.NONTERM_typeArgsActual] -> os.Append(NONTERM_typeArgsActualE.Format) |> ignore; true
              | _ -> 
                  false)
#if DEBUG
      if not foundInContext then
          Printf.bprintf os ". (Please report to fsbugs@microsoft.com: no 'in' context found: %+A)" (List.map (List.map Parser.prodIdxToNonTerminal) ctxt.ReducibleProductions);
#endif
      let fix (s:string) = s.Replace(SR.GetString("FixKeyword"),"").Replace(SR.GetString("FixSymbol"),"").Replace(SR.GetString("FixReplace"),"")
      match (ctxt.ShiftTokens 
                   |> List.map Parser.tokenTagToTokenId 
                   |> List.filter (function Parser.TOKEN_error | Parser.TOKEN_EOF -> false | _ -> true) 
                   |> List.map tokenIdToText 
                   |> Set.of_list 
                   |> Set.to_list) with 
      | [tokenName1]            -> os.Append(TokenName1E.Format (fix tokenName1)) |> ignore
      | [tokenName1;tokenName2] -> os.Append(TokenName1TokenName2E.Format (fix tokenName1) (fix tokenName2)) |> ignore
      | [tokenName1;tokenName2;tokenName3] -> os.Append(TokenName1TokenName2TokenName3E.Format (fix tokenName1) (fix tokenName2) (fix tokenName3)) |> ignore
      | _ -> ()
(*
      Printf.bprintf os ".\n\n    state = %A\n    token = %A\n    expect (shift) %A\n    expect (reduce) %A\n   prods=%A\n     non terminals: %A" 
          ctxt.StateStack
          ctxt.CurrentToken
          (List.map Parser.tokenTagToTokenId ctxt.ShiftTokens)
          (List.map Parser.tokenTagToTokenId ctxt.ReduceTokens)
          ctxt.ReducibleProductions
          (List.mapSquared Parser.prodIdxToNonTerminal ctxt.ReducibleProductions)
*)
  | RuntimeCoercionSourceSealed(denv,ty,m) -> 
      let _,ty,tpcs = PrettyTypes.PrettifyTypes1 denv.g ty
      if is_typar_typ denv.g ty 
      then os.Append(RuntimeCoercionSourceSealed1E.Format (NicePrint.string_of_typ denv ty)) |> ignore
      else os.Append(RuntimeCoercionSourceSealed2E.Format (NicePrint.string_of_typ denv ty)) |> ignore
  | CoercionTargetSealed(denv,ty,m) -> 
      let _,ty,tpcs = PrettyTypes.PrettifyTypes1 denv.g ty
      os.Append(CoercionTargetSealedE.Format (NicePrint.string_of_typ denv ty)) |> ignore
  | UpcastUnnecessary(m) -> 
      os.Append(UpcastUnnecessaryE.Format) |> ignore
  | TypeTestUnnecessary(m) -> 
      os.Append(TypeTestUnnecessaryE.Format) |> ignore
  | Creflect.IgnoringPartOfQuotedTermWarning (msg,_) -> 
      Printf.bprintf os "%s" msg
  | OverrideDoesntOverride(denv,impl,minfoVirtOpt,g,amap,m) ->
      let sig1 = DispatchSlotChecking.FormatOverride denv impl
      begin match minfoVirtOpt with 
      | None -> 
          os.Append(OverrideDoesntOverride1E.Format sig1) |> ignore
      | Some minfoVirt -> 
          os.Append(OverrideDoesntOverride2E.Format sig1) |> ignore
          let sig2 = DispatchSlotChecking.FormatMethInfoSig g amap m denv minfoVirt
          if sig1 <> sig2 then 
              os.Append(OverrideDoesntOverride3E.Format  sig2) |> ignore
      end
  | UnionCaseWrongArguments (denv,n1,n2,m) ->
      os.Append(UnionCaseWrongArgumentsE.Format n2 n1) |> ignore
  | UnionPatternsBindDifferentNames m -> 
      os.Append(UnionPatternsBindDifferentNamesE.Format) |> ignore
  | ValueNotContained (denv,mref,v1,v2,s) ->
      let text1,text2 = minimalStringsOfTwoValues denv v1 v2
      os.Append(ValueNotContainedE.Format 
         (full_display_text_of_modref mref) 
         text1 
         text2 
         s) |> ignore
  | ConstrNotContained (denv,v1,v2,msg) ->
      os.Append(ConstrNotContainedE.Format (NicePrint.string_of_ucase denv v1) (NicePrint.string_of_ucase denv v2) msg) |> ignore
  | ExnconstrNotContained (denv,v1,v2,s) ->
      os.Append(ExnconstrNotContainedE.Format s (NicePrint.string_of_exnc denv v1) (NicePrint.string_of_exnc denv v2)) |> ignore
  | FieldNotContained (denv,v1,v2,msg) ->
      os.Append(FieldNotContainedE.Format (NicePrint.string_of_rfield denv v1) (NicePrint.string_of_rfield denv v2) msg) |> ignore
  | RequiredButNotSpecified (denv,mref,k,name,m) ->
      let nsb = new System.Text.StringBuilder()
      name nsb;
      os.Append(RequiredButNotSpecifiedE.Format (full_display_text_of_modref mref) k (nsb.ToString())) |> ignore
  | UseOfAddressOfOperator _ -> 
      os.Append(UseOfAddressOfOperatorE.Format) |> ignore
  | DefensiveCopyWarning(s,m) -> os.Append(DefensiveCopyWarningE.Format s) |> ignore
  | DeprecatedThreadStaticBindingWarning(m) -> 
      os.Append(DeprecatedThreadStaticBindingWarningE.Format) |> ignore
  | DeprecatedClassFieldInference(m) -> 
      os.Append(DeprecatedClassFieldInferenceE.Format) |> ignore
  | FunctionValueUnexpected (denv,ty,m) ->
      let _,ty,tpcs = PrettyTypes.PrettifyTypes1 denv.g ty
      os.Append(FunctionValueUnexpectedE.Format (NicePrint.string_of_typ denv ty)) |> ignore
  | UnitTypeExpected (denv,ty,perhapsProp,m) ->
      let _,ty,tpcs = PrettyTypes.PrettifyTypes1 denv.g ty
      os.Append(UnitTypeExpected1E.Format (NicePrint.string_of_typ denv ty)) |> ignore
      if perhapsProp then os.Append(UnitTypeExpected2E.Format) |> ignore
  | RecursiveUseCheckedAtRuntime (denv,v,m) -> 
      os.Append(RecursiveUseCheckedAtRuntimeE.Format) |> ignore
  | LetRecUnsound (denv,[v],m) ->  
      os.Append(LetRecUnsound1E.Format v.DisplayName) |> ignore
  | LetRecUnsound (denv,path,m) -> 
      let bos = new System.Text.StringBuilder()
      let s = List.iter (fun (v:ValRef) -> bos.Append(LetRecUnsoundInnerE.Format v.DisplayName) |> ignore) (List.tl path @ [List.hd path])
      os.Append(LetRecUnsound2E.Format (List.hd path).DisplayName (bos.ToString())) |> ignore
  | LetRecEvaluatedOutOfOrder (denv,v1,v2,m) -> 
      os.Append(LetRecEvaluatedOutOfOrderE.Format) |> ignore
  | LetRecCheckedAtRuntime _ -> 
      os.Append(LetRecCheckedAtRuntimeE.Format) |> ignore
  | SelfRefObjCtor(false,m) -> 
      os.Append(SelfRefObjCtor1E.Format) |> ignore
  | SelfRefObjCtor(true,m) -> 
      os.Append(SelfRefObjCtor2E.Format) |> ignore
  | VirtualAugmentationOnNullValuedType(m) ->
      os.Append(VirtualAugmentationOnNullValuedTypeE.Format) |> ignore
  | NonVirtualAugmentationOnNullValuedType(m) ->
      os.Append(NonVirtualAugmentationOnNullValuedTypeE.Format) |> ignore
  | NonUniqueInferredAbstractSlot(g,denv,bindnm,bvirt1,bvirt2,m) ->
      os.Append(NonUniqueInferredAbstractSlot1E.Format bindnm) |> ignore
      let ty1 = bvirt1.EnclosingType
      let ty2 = bvirt2.EnclosingType
      let t1,t2,tpcs = minimalStringsOfTwoTypes denv ty1 ty2
      os.Append(NonUniqueInferredAbstractSlot2E.Format) |> ignore
      if t1 <> t2 then 
          os.Append(NonUniqueInferredAbstractSlot3E.Format t1 t2) |> ignore
      os.Append(NonUniqueInferredAbstractSlot4E.Format) |> ignore
  | Error (s,m) -> os.Append(ErrorE.Format s) |> ignore
  | InternalError (s,_) 
  | InvalidArgument s 
  | Failure s ->
      let f1 = SR.GetString("Failure1")
      let f2 = SR.GetString("Failure2") 
      match s with 
      | f when f = f1 -> os.Append(Failure3E.Format s) |> ignore
      | f when f = f2 -> os.Append(Failure3E.Format s) |> ignore
      | _ -> os.Append(Failure4E.Format s) |> ignore
#if DEBUG
      Printf.bprintf os "\nStack Trace\n%s\n" (exn.ToString())
      if !showAssertForUnexpectedException then 
          System.Diagnostics.Debug.Assert(false,sprintf "Bug seen in compiler: %s" (exn.ToString()))
#endif
  | FullAbstraction(s,m) -> os.Append(FullAbstractionE.Format s) |> ignore
  | WrappedError (exn,m) -> OutputExceptionR os exn
  | Patcompile.MatchIncomplete (isComp,cexOpt,m) -> 
      os.Append(MatchIncomplete1E.Format) |> ignore
      match cexOpt with 
      | None -> ()
      | Some (cex,false) ->  os.Append(MatchIncomplete2E.Format cex) |> ignore
      | Some (cex,true) ->  os.Append(MatchIncomplete3E.Format cex) |> ignore
      if isComp then 
          os.Append(MatchIncomplete4E.Format) |> ignore
  | Patcompile.RuleNeverMatched m -> os.Append(RuleNeverMatchedE.Format) |> ignore
  | ValNotMutable(denv,vr,m) -> os.Append(ValNotMutableE.Format) |> ignore
  | ValNotLocal(denv,vr,m) -> os.Append(ValNotLocalE.Format) |> ignore
  | Obsolete (s, _) -> 
        os.Append(Obsolete1E.Format) |> ignore
        if s <> "" then os.Append(Obsolete2E.Format s) |> ignore
  | Experimental (s, _) -> os.Append(ExperimentalE.Format s) |> ignore
  | PossibleUnverifiableCode m -> os.Append(PossibleUnverifiableCodeE.Format) |> ignore
  | OCamlCompatibility (s, _) -> os.Append(OCamlCompatibilityE.Format (if s = "" then "" else s^". ")) |> ignore
  | Deprecated(s, _) -> os.Append(DeprecatedE.Format s) |> ignore
  | LibraryUseOnly(_) -> os.Append(LibraryUseOnlyE.Format) |> ignore
  | MissingFields(sl,m) -> os.Append(MissingFieldsE.Format (String.concat "," sl ^".")) |> ignore
  | ValueRestriction(denv,hassig,v,tp,m) -> 
      let denv = { denv with showImperativeTyparAnnotations=true; }
      let tps,tau = v.TypeScheme
      if hassig then 
          if is_fun_typ denv.g tau && (arity_of_val v).HasNoArgs then 
            os.Append(ValueRestriction1E.Format
              v.DisplayName 
              (NicePrint.string_of_qualified_val_spec denv v)
              v.DisplayName) |> ignore
          else
            os.Append(ValueRestriction2E.Format
              v.DisplayName 
              (NicePrint.string_of_qualified_val_spec denv v)
              v.DisplayName) |> ignore
      else
          match v.MemberInfo with 
          | Some(membInfo) when 
              begin match membInfo.MemberFlags.MemberKind with 
              | MemberKindPropertyGet 
              | MemberKindPropertySet 
              | MemberKindConstructor -> true (* can't infer extra polymorphism *)
              | _ -> false                     (* can infer extra polymorphism *)
              end -> 
                  os.Append(ValueRestriction3E.Format (NicePrint.string_of_qualified_val_spec denv v)) |> ignore
          | _ -> 
            if is_fun_typ denv.g tau && (arity_of_val v).HasNoArgs then 
                os.Append(ValueRestriction4E.Format
                  v.DisplayName
                  (NicePrint.string_of_qualified_val_spec denv v)
                  v.DisplayName) |> ignore
            else
                os.Append(ValueRestriction5E.Format
                  v.DisplayName
                  (NicePrint.string_of_qualified_val_spec denv v)
                  v.DisplayName) |> ignore
            
  | Parsing.RecoverableParseError -> os.Append(RecoverableParseErrorE.Format) |> ignore
  | ReservedKeyword (s,m) -> os.Append(ReservedKeywordE.Format s) |> ignore
  | IndentationProblem (s,m) -> os.Append(IndentationProblemE.Format s) |> ignore
  | OverrideInIntrinsicAugmentation(m) -> os.Append(OverrideInIntrinsicAugmentationE.Format) |> ignore
  | OverrideInExtrinsicAugmentation(m) -> os.Append(OverrideInExtrinsicAugmentationE.Format) |> ignore
  | IntfImplInIntrinsicAugmentation(m) -> os.Append(IntfImplInIntrinsicAugmentationE.Format) |> ignore
  | IntfImplInExtrinsicAugmentation(m) -> os.Append(IntfImplInExtrinsicAugmentationE.Format) |> ignore
  | UnresolvedReferenceError(assemblyname,_)
  | UnresolvedReferenceNoRange(assemblyname) ->
    os.Append(UnresolvedReferenceNoRangeE.Format assemblyname) |> ignore
  | UnresolvedPathReference(assemblyname,pathname,_) 
  | UnresolvedPathReferenceNoRange(assemblyname,pathname) ->
    os.Append(UnresolvedPathReferenceNoRangeE.Format pathname assemblyname) |> ignore
  | DeprecatedCommandLineOption(optionName,altOption,_) -> os.Append(DeprecatedCommandLineOptionE.Format optionName altOption) |> ignore
  | HashIncludeNotAllowedInNonScript(_) ->
      os.Append(HashIncludeNotAllowedInNonScriptE.Format) |> ignore
  | HashReferenceNotAllowedInNonScript(_) ->
      os.Append(HashReferenceNotAllowedInNonScriptE.Format) |> ignore
  | HashReferenceCopyAfterCompileNotAllowedInNonScript(_) ->
      os.Append(HashReferenceCopyAfterCompileNotAllowedInNonScriptE.Format) |> ignore
  | HashDirectiveNotAllowedInNonScript(_) ->
      os.Append(HashDirectiveNotAllowedInNonScriptE.Format) |> ignore
  | FileNameNotResolved(filename,locations,_) -> 
      os.Append(FileNameNotResolvedE.Format filename locations) |> ignore
  | AssemblyNotResolved(originalName,_) ->
      os.Append(AssemblyNotResolvedE.Format originalName) |> ignore
  | HashLoadedSourceHasIssues(warnings,errors,_) -> 
    let Emit(l:exn list) =
        OutputExceptionR os (List.hd l)
    if errors=[] then 
        os.Append(HashLoadedSourceHasIssues1E.Format) |> ignore
        Emit(warnings)
    else
        os.Append(HashLoadedSourceHasIssues2E.Format) |> ignore
        Emit(errors)
  | HashLoadedScriptConsideredSource(_) ->
      os.Append(HashLoadedScriptConsideredSourceE.Format) |> ignore
  | InvalidInternalsVisibleToAssemblyName(badName,fileNameOption) ->      
      match fileNameOption with      
      | Some file -> os.Append(InvalidInternalsVisibleToAssemblyName1E.Format badName file) |> ignore
      | None      -> os.Append(InvalidInternalsVisibleToAssemblyName2E.Format badName) |> ignore
  | LoadedSourceNotFoundIgnoring(filename,_) ->
      os.Append(LoadedSourceNotFoundIgnoringE.Format filename) |> ignore
  | MSBuildReferenceResolutionWarning(code,message,_) 
  | MSBuildReferenceResolutionError(code,message,_) -> 
      os.Append(MSBuildReferenceResolutionErrorE.Format message code) |> ignore
  // Strip TargetInvocationException wrappers
  | :? System.Reflection.TargetInvocationException as e -> 
      OutputExceptionR os e.InnerException
  | :? FileNotFoundException as e -> Printf.bprintf os "%s" e.Message
  | :? DirectoryNotFoundException as e -> Printf.bprintf os "%s" e.Message
  | :? System.ArgumentException as e -> Printf.bprintf os "%s" e.Message
  | :? System.NotSupportedException as e -> Printf.bprintf os "%s" e.Message
  | :? IOException as e -> Printf.bprintf os "%s" e.Message
  | :? System.UnauthorizedAccessException as e -> Printf.bprintf os "%s" e.Message
  | e -> 
      os.Append(TargetInvocationExceptionWrapperE.Format e.Message) |> ignore
#if DEBUG
      Printf.bprintf os "\nStack Trace\n%s\n" (exn.ToString())
      if !showAssertForUnexpectedException then 
          System.Diagnostics.Debug.Assert(false,sprintf "Bug seen in compiler: %s" (exn.ToString()))
#endif


and output_plural os n = if n <> 1 then Printf.bprintf os "s"

// remove any newlines and tabs 
let OutputException (os:System.Text.StringBuilder) exn (flattenErrors:bool) = 
    let buf = new System.Text.StringBuilder()

    OutputExceptionR buf exn
    
    let s = if flattenErrors then buf.ToString().Replace('\n',' ').Replace('\t',' ') else buf.ToString()
    
    os.Append(s) |> ignore


type ErrorStyle = 
    | DefaultErrors 
    | EmacsErrors 
    | TestErrors 
    | VSErrors

let SanitizeFileName fileName implicitIncludeDir =
    // The assert below is almost ok, but it fires in two cases:
    //  - fsi.exe sometimes passes "stdin" as a dummy filename
    //  - if you have a #line directive, e.g. 
    //        # 1000 "Line01.fs"
    //    then it also asserts.  But these are edge cases that can be fixed later, e.g. in bug 4651.
    //System.Diagnostics.Debug.Assert(System.IO.Path.IsPathRooted(fileName), sprintf "filename should be absolute: '%s'" fileName)
    let fullPath = System.IO.Path.GetFullPath(fileName)
    let currentDir = implicitIncludeDir
    
    // if the file name is not rooted in the current directory, return the full path
    if not(fullPath.StartsWith(currentDir)) then
        fullPath
    // if the file name is rooted in the current directory, return the relative path
    else
        fullPath.Replace(currentDir^"\\","")

(* used by fsc.exe and fsi.exe, but not by VS *)
let rec OutputErrorOrWarning (implicitIncludeDir,showFullPaths,flattenErrors,errorStyle,warn) os (err:exn) = 
    let output_where (showFullPaths,errorStyle) exn os m = 
        if m = rangeStartup || m = rangeCmdArgs then () 
        else
            let file = file_of_range m
            let file = if showFullPaths then 
                            Filename.fullpath implicitIncludeDir file
                       else 
                            SanitizeFileName file implicitIncludeDir
            match errorStyle with
              | ErrorStyle.EmacsErrors   -> Printf.bprintf os "File \"%s\", line %d, characters %d-%d: " (file.Replace("\\","/")) (start_line_of_range m) (start_col_of_range m) (end_col_of_range m)
              // We're adjusting the columns here to be 1-based - both for parity with C# and for MSBuild, which assumes 1-based columns for error output
              | ErrorStyle.DefaultErrors -> Printf.bprintf os "%s(%d,%d): " (file.Replace("/","\\")) (start_line_of_range m) ((start_col_of_range m) + 1)
              // We may also want to change TestErrors to be 1-based
              | ErrorStyle.TestErrors    -> Printf.bprintf os "%s(%d,%d-%d,%d): " (file.Replace("/","\\")) (start_line_of_range m) ((start_col_of_range m) + 1) (end_line_of_range m) ((end_col_of_range m) + 1) 
              // Here, we want the complete range information so Project Systems can generate proper squiggles
              | ErrorStyle.VSErrors      -> Printf.bprintf os "%s(%d,%d,%d,%d): " (file.Replace("/","\\")) (start_line_of_range m) ((start_col_of_range m) + 1) (end_line_of_range m) ((end_col_of_range m) + 1)


    match err with 
    | ReportedError -> 
        dprintf "Unexpected ReportedError"  (* this should actually never happen *)
    | StopProcessing -> 
        dprintf "Unexpected StopProcessing"  (* this should actually never happen *)
    | _ -> 
        Printf.bprintf os "\n";
        match RangeOfError err with 
        | Some m -> output_where (showFullPaths,errorStyle) err os m 
        | None -> ()

        Printf.bprintf os "%s FS%04d: " (if warn then "warning" else "error") (GetErrorNumber err);
        let mainError,relatedErrors = SplitRelatedErrors err
        OutputException os mainError flattenErrors;
        List.iter (fun err -> Printf.bprintf os "\n"; OutputException os err flattenErrors) relatedErrors
      
let OutputErrorOrWarningContext prefix fileLineFn os (err:exn) =
    match RangeOfError err with
    | None   -> ()      
    | Some m -> 
        let filename = file_of_range m
        let lineA = start_line_of_range m
        let lineB = end_line_of_range m
        let line  = fileLineFn filename lineA
        if line<>"" then 
            let iA    = start_col_of_range m
            let iB    = end_col_of_range m
            let iLen  = if lineA = lineB then max (iB - iA) 1  else 1
            Printf.bprintf os "%s%s\n"   prefix line;
            Printf.bprintf os "%s%s%s\n" prefix (String.make iA '-') (String.make iLen '^')



//----------------------------------------------------------------------------

let coreFramework = 
  [ "System";
    "System.Xml" ]

let extendedFramework = 
    [ "System.Runtime.Remoting";
      "System.Runtime.Serialization.Formatters.Soap";
      "System.Data";
      "System.Drawing";
      "System.Web";
      "System.Web.Services";
      "System.Windows.Forms"; ]

let GetFSharpCoreLibraryName () = "FSharp.Core"
let GetFSharpPowerPackLibraryName () = "FSharp.PowerPack"
let fsiaux () = "FSharp.Compiler.Interactive.Settings"  
let fsiAuxSettingsModulePath = "Microsoft.FSharp.Compiler.Interactive.Settings"
let scriptingFramework = coreFramework @ extendedFramework @ ["FSharp.Compiler.Interactive.Settings"]

let (++) x s = x @ [s]

/// Determine the default "frameworkVersion" (which is passed into MSBuild resolve).
/// If this binary was built for v4, the return "v4.0"
/// If this binary was built for v2, the return "v3.5", "v3.5" or "v2.0" depending on what is installed.
///
/// See: Detecting which versions of the .NET framework are installed.
///      http://blogs.msdn.com/aaronru/archive/2007/11/26/net-framework-3-5-rtm-detection-logic.aspx
/// See: bug 4409.
open Microsoft.Win32
let highestInstalledNetFrameworkVersionMajorMinor() =
#if FX_ATLEAST_40  
    4,"v4.0"
#else
    try    
        let net35 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.5","Install",null) = box 1
        let net30 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v3.0","Install",null) = box 1
        let net20 = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727","Install",null) = box 1
        if   net35 then 2,"v3.5"
        elif net30 then 2,"v3.0"
        else 2,"v2.0" // version is 2.0 assumed since this code is running.
    // the above calls to Registry.GetValue could potentially fail - especially on platforms like Mono
    with e -> warning(Error("Could not determine highest installed .NET framework version from Registry keys, using version 2.0",rangeStartup)); 2,"v2.0"
#endif

//----------------------------------------------------------------------------
// General file name resolver
//--------------------------------------------------------------------------

let TryResolveFileUsingPaths(paths,m,name) =
    let () = 
        try Path.IsPathRooted(name)  |> ignore 
        with :? System.ArgumentException as e -> error(Error(e.Message,m))
    if Path.IsPathRooted(name) && Internal.Utilities.FileSystem.File.SafeExists name 
    then name 
    else
        let res = paths |> List.tryPick (fun path ->  
                    let n = Filename.concat path name
                    if Internal.Utilities.FileSystem.File.SafeExists n then  Some n 
                    else None)
        match res with 
        | Some f -> f
        | None -> 
            let filename = name
            let searchMessage = String.concat "\n " paths
            raise (FileNameNotResolved(name,searchMessage,m))
            
            

let GetWarningNumber(m,s:string) =
    try 
        Some (int32 s)
    with err -> 
        warning(Error("invalid warning number: '"^s^"'",m));
        None

let ComputeMakePathAbsolute implicitIncludeDir (path : string) = 
    try  
        // remove any quotation marks from the path first
        let path = path.Replace("\"","")
        if not (Path.IsPathRooted(path)) 
        then Filename.concat implicitIncludeDir path
        else path 
    with 
        :? System.ArgumentException -> path  


//----------------------------------------------------------------------------
// Configuration
//--------------------------------------------------------------------------
    

/// Determins whether a path is invalid or not.
let IsInvalidPath (path : string) = 
    String.IsNullOrEmpty(path) || 
    path.IndexOfAny(Path.GetInvalidPathChars()) <> -1

type target = 
    | WinExe 
    | ConsoleExe 
    | Dll 
    | Module
    member x.IsExe = (match x with ConsoleExe | WinExe -> true | _ -> false)

type ResolveLibFileMode = Speculative | ReportErrors

type VersionFlag = 
    | VersionString of string
    | VersionFile of string
    | VersionNone
    member x.GetVersionInfo(implicitIncludeDir) =
        let vstr = x.GetVersionString(implicitIncludeDir)
        try 
            IL.parse_version vstr
        with _ -> errorR(Error("Invalid version string '"^vstr^"'",rangeStartup)) ; IL.parse_version "0.0.0.0"
        
    member x.GetVersionString(implicitIncludeDir) = 
         match x with 
         | VersionString s -> s
         | VersionFile s ->
             let s = if Path.IsPathRooted(s) then s else Path.Combine(implicitIncludeDir,s)
             if not(Internal.Utilities.FileSystem.File.SafeExists(s)) then 
                 errorR(Error("Invalid version file '"^s^"'",rangeStartup)) ; "0.0.0.0"
             else
                 use is = System.IO.File.OpenText s
                 is.ReadLine()
         | VersionNone -> "0.0.0.0"
     

type AssemblyReference = 
    | AssemblyReference of range * string 
    member x.Range = (let (AssemblyReference(m,_)) = x in m)
    member x.Text = (let (AssemblyReference(_,text)) = x in text)
    member x.SimpleAssemblyNameIs(name) = 
        (String.Compare(Path.GetFileNameWithoutExtension(x.Text), name, StringComparison.OrdinalIgnoreCase) = 0) ||
        (let text = x.Text.ToLowerInvariant()
         not (text.Contains "/") && not (text.Contains "\\") && not (text.Contains ".dll") && not (text.Contains ".exe") &&
           try let aname = System.Reflection.AssemblyName(x.Text) in aname.Name = name 
           with _ -> false) 

type TcConfigBuilder =
    { mutable mscorlibAssemblyName : string;
      mutable autoResolveOpenDirectivesToDlls: bool;
      mutable noFeedback: bool;
      mutable implicitIncludeDir: string; (* normally "." *)
      mutable openBinariesInMemory: bool; (* false for command line, true for VS *)
      mutable openDebugInformationForLaterStaticLinking: bool; (* only for --standalone *)
      defaultFSharpBinariesDir: string;
      mutable compilingFslib: bool;
      mutable useIncrementalBuilder: bool;
      mutable includes: string list;
      mutable implicitOpens: string list;
      mutable useFsiAuxLib: bool;
      mutable framework: bool;
      mutable resolutionEnvironment : ResolutionEnvironment
      mutable implicitlyResolveAssemblies: bool;
      mutable light: bool option;
      mutable conditionalCompilationDefines: string list;
      mutable loadedSources: (range * string) list;
      mutable referencedDLLs : AssemblyReference list;
      optimizeForMemory: bool;
      mutable inputCodePage: int option;
      mutable embedResources : string list;
      mutable globalWarnAsError: bool;
      mutable globalWarnLevel: int;
      mutable specificWarnOff: int list; 
      mutable specificWarnAsError: int list 
      mutable mlCompatibility: bool;
      mutable checkOverflow: bool;
      mutable showReferenceResolutions:bool;
      mutable outputFile : string option;
      mutable resolutionFrameworkRegistryBase : string;
      mutable resolutionAssemblyFoldersSuffix : string; 
      mutable resolutionAssemblyFoldersConditions : string;    
      mutable platform : ILPlatform option;
      mutable useMonoResolution : bool
      mutable target : target
      mutable debuginfo : bool
      mutable debugSymbolFile : string option
      (* Backend configuration *)
      mutable typeCheckOnly : bool
      mutable parseOnly : bool
      mutable simulateException : string option
      mutable printAst : bool
      mutable tokenizeOnly : bool
      mutable testInteractionParser : bool
      mutable reportNumDecls : bool
      mutable printSignature : bool
      mutable printSignatureFile : string
      mutable xmlDocOutputFile : string option
      mutable generateHtmlDocs : bool
      mutable htmlDocDirectory : string option
      mutable htmlDocCssFile : string option
      mutable htmlDocNamespaceFile : string option
      mutable htmlDocAppendFlag : bool
      mutable htmlDocLocalLinks : bool  (* Do not do absolute links for fslib/mllib references *)
      mutable stats : bool
      mutable generateFilterBlocks : bool (* don't generate filter blocks due to bugs on Mono *)

      mutable signer : string option
      mutable container : string option

      mutable delaysign : bool
      mutable version : VersionFlag 
      mutable standalone : bool
      mutable extraStaticLinkRoots : string list 
      mutable noSignatureData : bool
      mutable onlyEssentialOptimizationData : bool
      mutable useOptimizationDataFile : bool
      mutable jitTracking : bool
      mutable ignoreSymbolStoreSequencePoints : bool
      mutable internConstantStrings : bool
      mutable generateConfigFile : bool
      mutable extraOptimizationIterations : int

      mutable win32res : string 
      mutable win32manifest : string
      mutable includewin32manifest : bool
      mutable linkResources : string list


      mutable showFullPaths : bool
      mutable errorStyle : ErrorStyle
      mutable utf8output : bool
      mutable flatErrors: bool

      mutable maxErrors : int
      mutable abortOnError : bool (* intended for fsi scripts that should exit on first error *)
      mutable baseAddress : int32 option
 #if DEBUG
      mutable writeGeneratedILFiles : bool (* write il files? *)  
      mutable showOptimizationData : bool
#endif
      mutable showTerms     : bool (* show terms between passes? *)
      mutable writeTermsToFiles : bool (* show terms to files? *)
      mutable doDetuple     : bool (* run detuple pass? *)
      mutable doTLR         : bool (* run TLR     pass? - not by default *)
      mutable optsOn        : bool (* optimizations are turned on *)
      mutable optSettings   : Opt.OptimizationSettings 

      mutable product : string
      /// show the MS (c) notice, e.g. with help or fsi? 
      mutable showBanner  : bool
        
      /// show times between passes? 
      mutable showTimes : bool

      /// pause between passes? 
      mutable pause : bool 
      }


    static member CreateNew (defaultFSharpBinariesDir,optimizeForMemory,implicitIncludeDir) =
        System.Diagnostics.Debug.Assert(Path.IsPathRooted(implicitIncludeDir), sprintf "implicitIncludeDir should be absolute: '%s'" implicitIncludeDir)
        if (String.IsNullOrEmpty(defaultFSharpBinariesDir)) then 
            failwith "Expected a valid defaultFSharpBinariesDir"
        { mscorlibAssemblyName = "mscorlib";
          light = None;
          noFeedback=false;
          conditionalCompilationDefines=[];
          implicitIncludeDir = implicitIncludeDir;
          autoResolveOpenDirectivesToDlls = false;
          openBinariesInMemory = false;
          openDebugInformationForLaterStaticLinking=false;
          defaultFSharpBinariesDir=defaultFSharpBinariesDir;
          compilingFslib=false;
          useIncrementalBuilder=false;
          useFsiAuxLib=false;
          implicitOpens=[];
          includes=[];
          resolutionEnvironment=MSBuildResolver.CompileTimeLike
          framework=true;
          implicitlyResolveAssemblies=true;
          referencedDLLs = [];
          loadedSources = [];
          globalWarnAsError=false;
          globalWarnLevel=3;
          specificWarnOff=[]; 
          specificWarnAsError=[] 
          embedResources = [];
          inputCodePage=None;
          optimizeForMemory=optimizeForMemory;
          mlCompatibility=false;
          checkOverflow=false;
          showReferenceResolutions=false;
          outputFile=None;
          resolutionFrameworkRegistryBase = "Software\Microsoft\.NetFramework";
          resolutionAssemblyFoldersSuffix = "AssemblyFoldersEx"; 
          resolutionAssemblyFoldersConditions = "";              
          platform = None;
          useMonoResolution = runningOnMono
          target = ConsoleExe
          debuginfo = false
          debugSymbolFile = None          

          (* Backend configuration *)
          typeCheckOnly = false
          parseOnly = false
          simulateException = None
          printAst = false
          tokenizeOnly = false
          testInteractionParser = false
          reportNumDecls = false
          printSignature = false
          printSignatureFile = ""
          xmlDocOutputFile = None
          generateHtmlDocs = false
          htmlDocDirectory = None
          htmlDocCssFile = None
          htmlDocNamespaceFile = None
          htmlDocAppendFlag = false
          htmlDocLocalLinks = false  (* Do not do absolute links for fslib/mllib references *)
          stats = false
          generateFilterBlocks = false (* don't generate filter blocks due to bugs on Mono *)

          signer = None
          container = None
          maxErrors = 100
          abortOnError = false
          baseAddress = None

          delaysign = false
          version = VersionNone
          standalone = false
          extraStaticLinkRoots = []
          noSignatureData = false
          onlyEssentialOptimizationData = false
          useOptimizationDataFile = false
          jitTracking = true
          ignoreSymbolStoreSequencePoints = false
          internConstantStrings = true
          generateConfigFile = false
          extraOptimizationIterations = 0

          win32res = ""
          win32manifest = ""
          includewin32manifest = true
          linkResources = []
          showFullPaths =false
          errorStyle = ErrorStyle.DefaultErrors
          utf8output = false
          flatErrors = false

 #if DEBUG
          writeGeneratedILFiles       = false (* write il files? *)  
          showOptimizationData = false
 #endif
          showTerms     = false 
          writeTermsToFiles = false 
          
          doDetuple     = false 
          doTLR         = false 
          optsOn        = false 
          optSettings   = Opt.OptimizationSettings.Defaults
          product = "Microsoft F# Compiler"
          showBanner  = true 
          showTimes = false 
          pause = false 
        } 
    /// Decide names of output file, pdb and assembly
    member tcConfigB.DecideNames sourceFiles =
        if sourceFiles = [] then errorR(Error("No inputs specified",rangeCmdArgs));
        let ext() = match tcConfigB.target with Dll -> ".dll" | Module -> ".netmodule" | ConsoleExe | WinExe -> ".exe"
        let implFiles = sourceFiles |> List.filter (fun lower -> List.exists (Filename.check_suffix (String.lowercase lower)) implSuffixes)
        let outfile = 
            match tcConfigB.outputFile, List.rev implFiles with 
            | None,[] -> "out" ^ ext()
            | None, h :: _  -> 
                let basic = Path.GetFileName h
                let modname = try Filename.chop_extension basic with _ -> basic
                modname^(ext())
            | Some f,_ -> f
        let assemblyName = 
            let baseName = Path.GetFileName outfile
            if not (Filename.check_suffix (String.lowercase baseName) (ext())) then
              errorR(Error("The output name extension doesn't match the flags used. If -a is used the output file name must end with .dll, if --target module is used the output extension must be .netmodule, otherwise .exe ",rangeCmdArgs));
            System.IO.Path.GetFileNameWithoutExtension baseName 

        let pdbfile = 
            if tcConfigB.debuginfo then
              Some (match tcConfigB.debugSymbolFile with None -> (Filename.chop_extension outfile)^"."^(Ilsupp.pdb_suffix_for_configuration (Ilsupp.current_configuration())) | Some f -> f)   
            elif (tcConfigB.debugSymbolFile <> None) && (not (tcConfigB.debuginfo)) then
              error(Error("The --pdb option requires the --debug option to be used",rangeStartup))  
            else None
        tcConfigB.outputFile <- Some(outfile)
        outfile,pdbfile,assemblyName
    member tcConfigB.TurnWarningOff(m,s:string) =
        match GetWarningNumber(m,s) with 
        | None -> ()
        | Some n -> 
            // nowarn 62 turns on mlCompatibility, e.g. shows ML compat items in intellisense menus
            if n = 62 then tcConfigB.mlCompatibility <- true;
            tcConfigB.specificWarnOff <- ListSet.insert (=) n tcConfigB.specificWarnOff

    member tcConfigB.AddIncludePath (m,path) = 
        let absolutePath = ComputeMakePathAbsolute tcConfigB.implicitIncludeDir path
        let ok = 
            let existsOpt = 
                try Some(Directory.Exists(absolutePath)) 
                with e -> warning(Error("The search directory '"^path^"' is invalid",m)); None
            match existsOpt with 
            | Some(exists) -> 
                if not exists then warning(Error("The search directory '"^absolutePath^"' could not be found",m));         
                exists
            | None -> false
        if ok && not (List.mem absolutePath tcConfigB.includes) then 
           tcConfigB.includes <- tcConfigB.includes ++ absolutePath
           
    member tcConfigB.AddLoadedSource(m,path) =
        if IsInvalidPath(path) then
            warning(Error(Printf.sprintf "'%s' is not a valid filename" path,m))    
        else 
            let path = ComputeMakePathAbsolute tcConfigB.implicitIncludeDir path
                
            if not (List.mem path (List.map snd tcConfigB.loadedSources)) then 
                tcConfigB.loadedSources <- tcConfigB.loadedSources ++ (m,path)
                

    member tcConfigB.AddEmbeddedResource filename =
        tcConfigB.embedResources <- tcConfigB.embedResources ++ filename

    member tcConfigB.AddReferencedAssemblyByPath (m,path) = 
        if IsInvalidPath(path) then
            warning(Error(Printf.sprintf "'%s' is not a valid assembly name" path,m))
        elif not (List.mem (AssemblyReference(m,path)) tcConfigB.referencedDLLs) then // NOTE: We keep same paths if range is different.
             tcConfigB.referencedDLLs <- tcConfigB.referencedDLLs ++ AssemblyReference(m,path)

    
    static member SplitCommandLineResourceInfo ri = 
        if String.contains ri ',' then 
            let p = String.index ri ',' 
            let file = String.sub ri 0 p 
            let rest = String.sub ri (p+1) (String.length ri - p - 1) 
            if String.contains rest ',' then 
                let p = String.index rest ',' 
                let name = String.sub rest 0 p^".resources" 
                let pubpri = String.sub rest (p+1) (rest.Length - p - 1) 
                if pubpri = "public" then file,name,Resource_public 
                elif pubpri = "private" then file,name,Resource_private
                else error(Error("unrecognized privacy setting "^pubpri^" for managed resource",rangeStartup))
            else 
                file,rest^".resources",Resource_public
        else 
            ri,System.IO.Path.GetFileName(ri),Resource_public 


let OpenILBinary(filename,optimizeForMemory,openBinariesInMemory,ilGlobalsOpt,pdbPathOption,mscorlibAssemblyName) = 
      let ilGlobals   = 
          match ilGlobalsOpt with 
          | None -> IL.mk_ILGlobals ScopeRef_local (Some mscorlibAssemblyName) 
          | Some ilGlobals -> ilGlobals
      let opts = { Ilread.defaults with 
                      Ilread.ilGlobals=ilGlobals;
                      // fsc.exe does not uses optimizeForMemory (hence keeps MORE caches in AbstractIL)
                      // fsi.exe does use optimizeForMemory (hence keeps FEWER caches in AbstractIL), because its long running
                      // Visual Studio does use optimizeForMemory (hence keeps FEWER caches in AbstractIL), because its long running
                      Ilread.optimizeForMemory=optimizeForMemory;
                      Ilread.pdbPath = pdbPathOption; } 
                      
      // Visual Studio uses OpenILModuleReaderAfterReadingAllBytes for all DLLs to avoid having to dispose of any readers explicitly
      if openBinariesInMemory // && not syslib 
      then Ilread.OpenILModuleReaderAfterReadingAllBytes filename opts
      else Ilread.OpenILModuleReader filename opts

#if DEBUG
[<System.Diagnostics.DebuggerDisplayAttribute("AssemblyResolution({ResolvedPath})")>]
#endif
type AssemblyResolution = 
    { originalReference : AssemblyReference
      resolvedPath : string    
      resolvedFrom : ResolvedFrom
      fusionName : string 
      fusionVersion : string 
      redist : string 
      sysdir : bool 
    }
    member private this.ResolvedPath = this.resolvedPath
    static member Default = 
         {originalReference = AssemblyReference(range0, null); resolvedPath = null; resolvedFrom = Unknown; fusionName = null; fusionVersion = null; redist = null; sysdir = false}    

type UnresolvedReference = UnresolvedReference of string * AssemblyReference list

let highestInstalledFrameworkVersion = highestInstalledNetFrameworkVersionMajorMinor()

[<Sealed>]
/// This type is immutable and must be kept as such. Do not extract or mutate the underlying data except by cloning it.
type TcConfig(data : TcConfigBuilder,validate:bool) =
    // Validate the inputs - this helps ensure errors in options are shown in visual studio rather than only when built
    // However we only validate a minimal number of options at the moment
    do if validate then try data.version.GetVersionInfo(data.implicitIncludeDir) |> ignore with e -> errorR(e) 

    // clone the input builder to ensure nobody messes with it.
    let data = { data with pause = data.pause }

    /// A closed set of assemblies where, for any subset S:
    ///    -  the TcImports object built for S (and thus the F# Compiler CCUs for the assemblies in S) 
    ///       is a resource that can be shared between any two IncrementalBuild objects that reference
    ///       precisely S
    let systemAssemblies = 
        [data.mscorlibAssemblyName] @ [GetFSharpCoreLibraryName()] @ coreFramework @ extendedFramework @ ["System.Core"] 

    let computeKnownDllReference(referencedDLLs:AssemblyReference list, libraryName, pathOpt) = 
        let defaultCoreLibraryReference = AssemblyReference(rangeStartup,(match pathOpt with Some p -> Filename.concat p libraryName  | None -> libraryName)^".dll")
        match data.referencedDLLs |> List.filter(fun assemblyReference -> assemblyReference.SimpleAssemblyNameIs libraryName) with
        | [AssemblyReference(m,f) as r] -> 
            let filename = ComputeMakePathAbsolute data.implicitIncludeDir f
            if Internal.Utilities.FileSystem.File.SafeExists(filename) then 
                r,true
            else   
                // If the file doesn't exist, let reference resolution logic report the error later...
                defaultCoreLibraryReference, false
        | [] -> 
            defaultCoreLibraryReference, false
        | _ -> error(Error(sprintf "Multiple references to %s.dll are not permitted" libraryName,rangeCmdArgs))

    // Look for an explicit reference to mscorlib and use that to compute clrRoot and targetFrameworkVersion
    let mscorlibReference,isMscorlibExplicit = computeKnownDllReference(data.referencedDLLs,data.mscorlibAssemblyName,None)
    let fslibReference,isFslibExplicit = computeKnownDllReference(data.referencedDLLs,GetFSharpCoreLibraryName(),Some(data.defaultFSharpBinariesDir))

    let clrRootValue,(mscorlibMajorVersion,targetFrameworkVersionValue) = 
        if isMscorlibExplicit then 
            let filename = ComputeMakePathAbsolute data.implicitIncludeDir mscorlibReference.Text
            try 
            
                let ilReader = OpenILBinary(filename,data.optimizeForMemory,data.openBinariesInMemory,None,None,data.mscorlibAssemblyName)
                try 
                   let ilModule = ilReader.ILModuleDef
                   match ilModule.ManifestOfAssembly.Version with 
                   | Some(v1,v2,_,_) -> 
                       if v1 = 1us then 
                           warning(Error(sprintf "The file '%s' is a CLI 1.x version of mscorlib. F# requires CLI version 2.0 or greater" filename,rangeStartup))
                       let clrRoot = Some(Path.GetDirectoryName(Path.GetFullPath(filename)))
                       clrRoot, (int v1, sprintf "v%d.%d" v1 v2)
                   | _ -> 
                       failwith "could not read version from mscorlib.dll"
                finally
                   Ilread.CloseILModuleReader ilReader
            with _ -> 
                error(Error(sprintf "Unable to read assembly '%s'" filename,rangeStartup))
        else
            None, highestInstalledFrameworkVersion 
    
    // Check that the referenced version of FSharp.COre.dll matches the referenced version of mscorlib.dll 
    let checkFSharpBinaryCompatWithMscorlib filename (ilAssemblyRefs: ILAssemblyRef list) m = 
        match ilAssemblyRefs |> List.tryFind (fun aref -> aref.Name = data.mscorlibAssemblyName) with 
        | Some aref when 
              (match aref.Version with 
               | Some(v1,_,_,_) -> ((v1 < 4us) <> (mscorlibMajorVersion < 4))
               | _ -> false) -> 
           warning(Error(sprintf "The referenced or default base CLI library 'mscorlib' is binary-incompatible with the referenced F# core library '%s'. Consider recompiling the library or making an explicit reference to a version of this library that matches the CLI version you are using" filename,m))
        | _ -> 
           ()

    // Look for an explicit reference to FSharp.Core and use that to compute fsharpBinariesDir
    let fsharpBinariesDirValue = 
        if isFslibExplicit then 
            let filename = ComputeMakePathAbsolute data.implicitIncludeDir fslibReference.Text
            try 
                let ilReader = OpenILBinary(filename,data.optimizeForMemory,data.openBinariesInMemory,None,None,data.mscorlibAssemblyName)
                try 
                   checkFSharpBinaryCompatWithMscorlib filename ilReader.ILAssemblyRefs rangeStartup;
                   let fslibRoot = Path.GetDirectoryName(Path.GetFullPath(filename))
                   fslibRoot (* , sprintf "v%d.%d" v1 v2 *)
                finally
                   Ilread.CloseILModuleReader ilReader
            with _ -> 
                error(Error(sprintf "Unable to read assembly '%s'" filename,rangeStartup))
        else
            data.defaultFSharpBinariesDir
            


    member x.mscorlibAssemblyName = data.mscorlibAssemblyName
    member x.autoResolveOpenDirectivesToDlls = data.autoResolveOpenDirectivesToDlls
    member x.noFeedback = data.noFeedback
    member x.implicitIncludeDir = data.implicitIncludeDir
    member x.openBinariesInMemory = data.openBinariesInMemory
    member x.openDebugInformationForLaterStaticLinking = data.openDebugInformationForLaterStaticLinking
    member x.fsharpBinariesDir = fsharpBinariesDirValue
    member x.compilingFslib = data.compilingFslib
    member x.useIncrementalBuilder = data.useIncrementalBuilder
    member x.includes = data.includes
    member x.implicitOpens = data.implicitOpens
    member x.useFsiAuxLib = data.useFsiAuxLib
    member x.framework = data.framework
    member x.implicitlyResolveAssemblies = data.implicitlyResolveAssemblies
    member x.resolutionEnvironment = data.resolutionEnvironment
    member x.light = data.light
    member x.conditionalCompilationDefines = data.conditionalCompilationDefines
    member x.loadedSources = data.loadedSources
    member x.referencedDLLs = data.referencedDLLs
    member x.clrRoot = clrRootValue
    member x.optimizeForMemory = data.optimizeForMemory
    member x.inputCodePage = data.inputCodePage
    member x.embedResources  = data.embedResources
    member x.globalWarnAsError = data.globalWarnAsError
    member x.globalWarnLevel = data.globalWarnLevel
    member x.specificWarnOff = data. specificWarnOff
    member x.specificWarnAsError = data.specificWarnAsError
    member x.mlCompatibility = data.mlCompatibility
    member x.checkOverflow = data.checkOverflow
    member x.showReferenceResolutions = data.showReferenceResolutions
    member x.outputFile  = data.outputFile
    member x.resolutionFrameworkRegistryBase  = data.resolutionFrameworkRegistryBase
    member x.resolutionAssemblyFoldersSuffix  = data. resolutionAssemblyFoldersSuffix
    member x.resolutionAssemblyFoldersConditions  = data.  resolutionAssemblyFoldersConditions  
    member x.platform  = data.platform
    member x.useMonoResolution  = data.useMonoResolution
    member x.target  = data.target
    member x.debuginfo  = data.debuginfo
    member x.debugSymbolFile  = data.debugSymbolFile
    member x.typeCheckOnly  = data.typeCheckOnly
    member x.parseOnly  = data.parseOnly
    member x.simulateException = data.simulateException
    member x.printAst  = data.printAst
    member x.targetFrameworkVersionMajorMinor = targetFrameworkVersionValue
    member x.tokenizeOnly  = data.tokenizeOnly
    member x.testInteractionParser  = data.testInteractionParser
    member x.reportNumDecls  = data.reportNumDecls
    member x.printSignature  = data.printSignature
    member x.printSignatureFile  = data.printSignatureFile
    member x.xmlDocOutputFile  = data.xmlDocOutputFile
    member x.generateHtmlDocs  = data.generateHtmlDocs
    member x.htmlDocDirectory  = match data.htmlDocDirectory with | None -> None | Some(x) -> Some(if System.IO.Path.IsPathRooted(x) then x else System.IO.Path.Combine(data.implicitIncludeDir, x))
    member x.htmlDocCssFile  = data.htmlDocCssFile
    member x.htmlDocNamespaceFile  = data.htmlDocNamespaceFile
    member x.htmlDocAppendFlag  = data.htmlDocAppendFlag
    member x.htmlDocLocalLinks  = data.htmlDocLocalLinks
    member x.stats  = data.stats
    member x.generateFilterBlocks  = data.generateFilterBlocks
    member x.signer  = data.signer
    member x.container = data.container
    member x.delaysign  = data.delaysign
    member x.version  = data.version
    member x.standalone  = data.standalone
    member x.extraStaticLinkRoots  = data.extraStaticLinkRoots
    member x.noSignatureData  = data.noSignatureData
    member x.onlyEssentialOptimizationData  = data.onlyEssentialOptimizationData
    member x.useOptimizationDataFile  = data.useOptimizationDataFile
    member x.jitTracking  = data.jitTracking
    member x.ignoreSymbolStoreSequencePoints  = data.ignoreSymbolStoreSequencePoints
    member x.internConstantStrings  = data.internConstantStrings
    member x.generateConfigFile  = data.generateConfigFile
    member x.extraOptimizationIterations  = data.extraOptimizationIterations
    member x.win32res  = data.win32res
    member x.win32manifest = data.win32manifest
    member x.includewin32manifest = data.includewin32manifest
    member x.linkResources  = data.linkResources
    member x.showFullPaths  = data.showFullPaths
    member x.errorStyle  = data.errorStyle
    member x.utf8output  = data.utf8output
    member x.flatErrors = data.flatErrors
    member x.maxErrors  = data.maxErrors
    member x.baseAddress  = data.baseAddress
 #if DEBUG
    member x.writeGeneratedILFiles  = data.writeGeneratedILFiles
    member x.showOptimizationData  = data.showOptimizationData
#endif
    member x.showTerms      = data.showTerms
    member x.writeTermsToFiles  = data.writeTermsToFiles
    member x.doDetuple      = data.doDetuple
    member x.doTLR          = data.doTLR
    member x.optSettings    = data.optSettings
    member x.optsOn         = data.optsOn
    member x.product  = data.product
    member x.showBanner   = data.showBanner
    member x.showTimes  = data.showTimes
    member x.pause  = data.pause

    static member Create(builder,validate) = TcConfig(builder,validate)

    member tcConfig.CloneOfOriginalBuilder = 
        { data with conditionalCompilationDefines=data.conditionalCompilationDefines }

    member tcConfig.ComputeCanContainEntryPoint(sourceFiles:string list) = 
        let n = sourceFiles.Length in 
        sourceFiles |> List.mapi (fun i nm -> (i=n-1) && tcConfig.target.IsExe)
            
    // This call can fail if no CLR is found (this is the path to mscorlib)
    member tcConfig.ClrRoot = 
        match tcConfig.clrRoot with 
        | Some x -> 
            [tcConfig.MakePathAbsolute x]
        | None -> 
            // When running on Mono we lead everyone to believe we're doing .NET 2.0 compilation 
            // by default. 
            if runningOnMono then 
                let mono10SysDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() 
                assert(mono10SysDir.EndsWith("1.0",StringComparison.Ordinal));
                let mono20SysDir = Path.Combine(Path.GetDirectoryName mono10SysDir, "2.0")
                let mono21SysDir = Path.Combine(Path.GetDirectoryName mono10SysDir, "2.1") 
                if Directory.Exists(mono20SysDir) then 
                  if Directory.Exists(mono21SysDir) then
                    [mono21SysDir;mono20SysDir]
                  else
                    [mono20SysDir]
                else [mono10SysDir]
            else 
                try [System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()] 
                with e -> errorRecovery e range0; [] 

    member tcConfig.ComputeLightSyntaxInitialStatus filename = 
        let lower = String.lowercase filename
        let lightOnByDefault = List.exists (Filename.check_suffix lower) lightSyntaxDefaultExtensions
        if lightOnByDefault then (tcConfig.light <> Some(false)) else (tcConfig.light = Some(true) )

    member tcConfig.ComputeSyntaxFlagRequired filename = 
        let lower = String.lowercase filename
        List.exists (Filename.check_suffix lower) syntaxFlagRequiredExtensions

    member tcConfig.GetAvailableLoadedSources() =
        let ResolveLoadedSource (m,path) =
            try
                let path = tcConfig.MakePathAbsolute path
                if not(Internal.Utilities.FileSystem.File.SafeExists(path)) then 
                    error(LoadedSourceNotFoundIgnoring(path,m))                         
                    None
                else Some(m,path)
            with e -> errorRecovery e m; None
            
        tcConfig.loadedSources |> List.map ResolveLoadedSource |> List.filter Option.is_some |> List.map Option.get                
        

    /// Return true if this is an installed system memory that is allowed to be locked or placed into the
    /// 'framework' reference set that is potentially shared across multiple compilations.
    member tcConfig.IsSystemAssembly (filename:string) =  
        try 
            Internal.Utilities.FileSystem.File.SafeExists(filename) && 
            (let baseFilename = Path.GetFileNameWithoutExtension(filename)
             systemAssemblies |> List.exists(fun s->s=baseFilename))
        with e ->
            false

        
    // This is not the complete set of search paths, it is just the set that is special to F# (as compared to MSBuild resolution)
    member tcConfig.SearchPathsForLibraryFiles = 
            (tcConfig.ClrRoot @ 
             List.map (tcConfig.MakePathAbsolute) tcConfig.includes ++
             tcConfig.implicitIncludeDir ++
             tcConfig.fsharpBinariesDir)

    member tcConfig.SearchPathsForSourceFiles  = 
        List.map (tcConfig.MakePathAbsolute) tcConfig.includes ++
        tcConfig.implicitIncludeDir 

    member tcConfig.MakePathAbsolute path = 
        let result = ComputeMakePathAbsolute tcConfig.implicitIncludeDir path
#if TRACK_DOWN_EXTRA_BACKSLASHES        
        System.Diagnostics.Debug.Assert(not(result.Contains(@"\\")), "tcConfig.MakePathAbsolute results in a non-canonical filename with extra backslashes: "^result)
#endif
        result
      
    member tcConfig.ResolveLibWithDirectories (AssemblyReference (m,nm) as r) = 
        let resolved = TryResolveFileUsingPaths(tcConfig.SearchPathsForLibraryFiles,m,nm)
        let sysdir = tcConfig.IsSystemAssembly resolved
        {AssemblyResolution.Default with originalReference=r;resolvedPath=resolved;resolvedFrom=Unknown;sysdir=sysdir}

    member tcConfig.ResolveSourceFile (m,nm) = 
        TryResolveFileUsingPaths(tcConfig.SearchPathsForSourceFiles,m,nm)

    member tcConfig.CheckFSharpBinary (filename,ilAssemblyRefs,m) = 
        checkFSharpBinaryCompatWithMscorlib filename ilAssemblyRefs m

    // NOTE!! if mode=Speculative then this method must not report ANY warnings or errors through 'warning' or 'error'. Instead
    // it must return warnings and errors as data
    //
    // NOTE!! if mode=ReportErrors then this method must not raise exceptions. It must just report the errors and recover
    static member TryResolveLibsUsingMSBuildRules (tcConfig:TcConfig,originalReferences:AssemblyReference list, errorAndWarningRange:range, mode:ResolveLibFileMode) : AssemblyResolution list * UnresolvedReference list =
        use t = Trace.Call("Build","TryResolveLibsUsingMSBuildRules", fun _->sprintf "Original references %A" originalReferences)
    
        if tcConfig.useMonoResolution then
            failwith "MSBuild resolution is not supported."
            
        if originalReferences=[] then [],[]
        else            
            // Group references by name with range values in the grouped value list.
            let groupedReferences = 
                originalReferences
                |> Seq.group_by(fun r -> r.Text)
                |> Seq.map(fun (name,s)->name,s |> List.of_seq)
                |> Array.of_seq
                
            // Check whether the given file is a .NET assembly.   
            // Related to Dev10 515444. GetAssemblyName called by MSBuild's ResolveAssemblyReference
            // will throw a ExecutionEngineException if things like notepad.exe. Attempt to prefilter
            // non->NET assemblies by cracking the PE header and looking at the section (RVA) that points
            // to managed code.
            let IsNetAssembly file = 
                try 
                    use fs =  new FileStream(file, FileMode.Open, FileAccess.Read)
                    use reader = new BinaryReader(fs)
                    fs.Position <- 060L // Jump to PE Header
                    let header = reader.ReadUInt32() // Read the header position
                    fs.Position <- int64 header
                    let magic = reader.ReadUInt16() // the magic number lets us know if we're dealing with a 32 or 64-bit header
                    fs.Position <- int64 (header + (if magic = 0x020Bus then 248u else 232u)) // Jump to RVA
                    let rva14 = reader.ReadUInt32()
                    (rva14<>0u)
                with 
                    | :? System.UnauthorizedAccessException -> false
                    | :? System.IO.EndOfStreamException -> false
                    | :? System.ArgumentOutOfRangeException -> false
                    | :? System.IO.IOException -> false                       
                        
            let IsNotFileOrIsAssembly file =
                let file = if Path.IsPathRooted(file) then file else Path.Combine(tcConfig.implicitIncludeDir, file)
                not(Internal.Utilities.FileSystem.File.SafeExists(file)) || IsNetAssembly(file)                   
                             
            let logmessage showMessages  = 
                if showMessages && tcConfig.showReferenceResolutions then (fun (message:string)->dprintf "%s\n" message)
                else (fun message->())
            let logwarning showMessages = 
                (fun code message->
                    if showMessages && mode = ReportErrors then 
                        match code with 
                        | "MSB3106"  ->
                            // These are warnings that mean 'not resolved' for some assembly.
                            // Note that we don't get to know the name of the assembly that couldn't be resolved.
                            // Ignore these and rely on the logic below to emit an error for each unresolved reference.
                            ()
                        | _ -> 
                            (if code = "MSB3245" then errorR else warning)
                                (MSBuildReferenceResolutionWarning(code,message,errorAndWarningRange)))
            let logerror showMessages = 
                (fun code message ->
                    if showMessages && mode = ReportErrors then 
                        errorR(MSBuildReferenceResolutionError(code,message,errorAndWarningRange)))

            let targetFrameworkMajorMinor = tcConfig.targetFrameworkVersionMajorMinor

#if DEBUG
            assert( Set.contains targetFrameworkMajorMinor (set ["v2.0";"v3.0";"v3.5";"v4.0"]) ) // Resolve is flexible, but pinning down targetFrameworkMajorMinor.
#endif

            let targetProcessorArchitecture = 
                    match tcConfig.platform with
                    | None -> "" // msil
                    | Some(X86) -> "x86"
                    | Some(AMD64) -> "amd64"
                    | Some(IA64) -> "ia64"
            let outputDirectory = 
                match tcConfig.outputFile with 
                | Some(outputFile) -> tcConfig.MakePathAbsolute outputFile
                | None -> tcConfig.implicitIncludeDir
            let targetFrameworkDirectories =
                match tcConfig.clrRoot with
                | Some(clrRoot) -> [tcConfig.MakePathAbsolute clrRoot]
                | None -> []
            
            let references = [|0..groupedReferences.Length-1|] 
                             |> Array.map(fun i->(fst groupedReferences.[i]),(string)i) 
                             |> Array.filter(fst >> IsNotFileOrIsAssembly)

            let Resolve(references,showMessages) = 
                try 
                    MSBuildResolver.Resolve
                       (tcConfig.resolutionEnvironment,
                        references,
                        targetFrameworkMajorMinor,   // TargetFrameworkVersionMajorMinor
                        targetFrameworkDirectories,  // TargetFrameworkDirectories 
                        targetProcessorArchitecture, // TargetProcessorArchitecture
                        Path.GetDirectoryName(outputDirectory), // Output directory
                        tcConfig.fsharpBinariesDir, // FSharp binaries directory
                        tcConfig.includes, // Explicit include directories
                        tcConfig.implicitIncludeDir, // Implicit include directory (likely the project directory)
                        tcConfig.resolutionFrameworkRegistryBase, 
                        tcConfig.resolutionAssemblyFoldersSuffix, 
                        tcConfig.resolutionAssemblyFoldersConditions, 
                        logmessage showMessages, logwarning showMessages, logerror showMessages)
                with 
                    MSBuildResolver.ResolutionFailure -> error(Error("Assembly resolution failure at or near this location",errorAndWarningRange))
            
            let resolutions = Resolve(references,(*showMessages*)true)  
                                                  
            let resultingResolutions =                                         
                resolutions.resolvedFiles
                    |> Array.map(fun resolvedFile -> 
                                    let i = (int)(resolvedFile.baggage)
                                    let original,ms = groupedReferences.[i]
                                    ms|>List.map(fun originalReference->
                                                    System.Diagnostics.Debug.Assert(System.IO.Path.IsPathRooted(resolvedFile.itemSpec), sprintf "msbuild-resolved path is not absolute: '%s'" resolvedFile.itemSpec)
                                                    let canonicalItemSpec = System.IO.Path.GetFullPath(resolvedFile.itemSpec)
                                                    {originalReference=originalReference; 
                                                     resolvedPath=canonicalItemSpec; 
                                                     resolvedFrom=resolvedFile.resolvedFrom;
                                                     fusionName=resolvedFile.fusionName;
                                                     fusionVersion=resolvedFile.version;
                                                     redist=resolvedFile.redist;
                                                     sysdir=tcConfig.IsSystemAssembly canonicalItemSpec}))
                    |> List.of_array   
                    |> List.flatten                                                 
                    
            // O(N^2) here over a small set of referenced assemblies.
            let IsResolved(originalName:string) =
                if resultingResolutions |> List.exists(fun resolution -> resolution.originalReference.Text = originalName) then true
                else 
                    // MSBuild resolution may have unified the result of two duplicate references. Try to re-resolve now.
                    // If re-resolution worked then this was a removed duplicate.
                    Resolve([|originalName,""|],(*showMessages*)false).resolvedFiles.Length<>0 
                    
            let unresolvedReferences =                     
                    groupedReferences 
                    //|> Array.filter(fst >> IsNotFileOrIsAssembly)
                    |> Array.filter(fst >> IsResolved >> not)   
                    |> List.of_array                 

            // Report that an assembly was not resolved.
            let ReportAssemblyNotResolved(file,originalReferences:AssemblyReference list) = 
                Trace.PrintLine("Build", fun () -> sprintf "Reporting error about assembly not found: %s" file)
                originalReferences |> List.iter(fun originalReference -> errorR(AssemblyNotResolved(file,originalReference.Range)))

            // Search for original references that are not in the resolved set and issue errors for them
            if mode = ReportErrors then 
                unresolvedReferences |> List.iter(ReportAssemblyNotResolved)
                
            // If mode=Speculative, then we haven't reported any errors.
            // We report the error condition by returning an empty list of resolutions
            if mode = Speculative && (originalReferences |> List.exists(fun r -> not (IsResolved r.Text))) then 
                [],(List.of_array groupedReferences) |> List.map UnresolvedReference
            else 
                resultingResolutions,unresolvedReferences |> List.map UnresolvedReference    


    member tcConfig.MscorlibDllReference() = mscorlibReference
        
    member tcConfig.CoreLibraryDllReference() = fslibReference
        

let warningMem n l = n <> 0 && List.mem n l

let ReportWarning (globalWarnLevel : int) (specificWarnOff : int list) (err:exn) = 
    let n = GetErrorNumber err
    warningOn err globalWarnLevel && not (warningMem n specificWarnOff)

let ReportWarningAsError (globalWarnLevel : int) (specificWarnOff : int list) (specificWarnAsError : int list) (globalWarnAsError : bool)  (err:exn) =
    (warningOn err globalWarnLevel) &&
    ((globalWarnAsError && not (warningMem (GetErrorNumber err) specificWarnOff)) ||
     warningMem (GetErrorNumber err) specificWarnAsError)

//----------------------------------------------------------------------------
// Scoped #nowarn pragmas


let GetScopedPragmasForHashDirective hd = 
    [ match hd with 
      | HashDirective("nowarn",[s],m) -> 
          match GetWarningNumber(m,s) with 
            | None -> ()
            | Some n -> yield WarningOff(m,n) 
      | _ -> () ]

let GetScopedPragmasForInput input = 

    match input with 
    | SigFileInput (SigFile(_,_,pragmas,_,_)) -> pragmas
    | ImplFileInput (ImplFile(_,_,_,pragmas,_,_,_)) ->pragmas



/// Build an ErrorLogger that delegates to another ErrorLogger but filters warnings turned off by the given pragma declarations
//
// NOTE: we allow a flag to turn of strict file checking. This is because file names sometimes don't match due to use of 
// #line directives, e.g. for pars.fs/pars.fsy. In this case we just test by line number - in most cases this is sufficent
// because we install a filtering error handler on a file-by-file basis for parsing and type-checking.
// However this is indicative of a more systematic problem where source-line 
// sensitive operations (lexfilter and warning filtering) do not always
// interact well with #line directives.
type ErrorLoggerFilteringByScopedPragmas (checkFile,scopedPragmas,errorLogger:ErrorLogger) =
   let mutable scopedPragmas = scopedPragmas
   member x.ScopedPragmas with set(v) = scopedPragmas <- v
   interface ErrorLogger with 
       member x.ErrorSink(e) = errorLogger.ErrorSink(e)
       member x.ErrorCount = errorLogger.ErrorCount
       member x.WarnSink(err:exn) = 
          let report = 
              let warningNum = GetErrorNumber err
              match RangeOfError err with 
              | Some m -> not (scopedPragmas |> List.exists (fun pragma ->
                                 match pragma with 
                                 | WarningOff(pragmaRange,warningNumFromPragma) -> 
                                    warningNum = warningNumFromPragma && 
                                    (not checkFile || Range.file_idx_of_range m = Range.file_idx_of_range pragmaRange) &&
                                    Range.pos_geq (Range.start_of_range m) (Range.start_of_range pragmaRange)))  
              | None -> true
          if report then errorLogger.WarnSink err; 

let GetErrorLoggerFilteringByScopedPragmas(checkFile,scopedPragmas,errorLogger) = 
    (ErrorLoggerFilteringByScopedPragmas(checkFile,scopedPragmas,errorLogger) :> ErrorLogger)

/// Build an ErrorLogger that delegates to another ErrorLogger but filters warnings turned off by the given pragma declarations
type DelayedErrorLogger(errorLogger:ErrorLogger) =
   let delayed = new ResizeArray<_>()
   interface ErrorLogger with 
       member x.ErrorSink(e) = delayed.Add (e,true)
       member x.ErrorCount = delayed |> Seq.filter snd |> Seq.length
       member x.WarnSink(e) = delayed.Add(e,false)
   member x.CommitDelayedErrorsAndWarnings() = 
       // Eagerly grab all the errors and warnings from the mutable collection
       let errors = delayed |> Seq.to_list
       // Now report them
       for (e,isError) in errors do
           if isError then errorLogger.ErrorSink(e) else errorLogger.WarnSink(e)

//----------------------------------------------------------------------------
// Parsing
//--------------------------------------------------------------------------


let CanonicalizeFilename filename = 
  let basic = Path.GetFileName filename
  String.capitalize (try Filename.chop_extension basic with _ -> basic)

let QualFileNameOfModuleName m modname = QualifiedNameOfFile(mksyn_id m (text_of_lid modname))
let QualFileNameOfFilename m filename = QualifiedNameOfFile(mksyn_id m (CanonicalizeFilename filename))
let QualFileNameOfUniquePath (m, p: string list) = QualifiedNameOfFile(mksyn_id m (String.concat "_" p))

let QualFileNameOfSpecs filename specs = 
    match specs with 
    | [ModuleOrNamespaceSpec(modname,true,_,_,_,_,m)] -> QualFileNameOfModuleName m modname
    | _ -> QualFileNameOfFilename (rangeN filename 1) filename

let QualFileNameOfImpls filename specs = 
    match specs with 
    | [ModuleOrNamespaceImpl(modname,true,_,_,_,_,m)] -> QualFileNameOfModuleName m modname
    | _ -> QualFileNameOfFilename (rangeN filename 1) filename

let PrepandPathToQualFileName x (QualifiedNameOfFile(q)) = QualFileNameOfUniquePath (q.idRange,path_of_lid x@[q.idText])
let PrepandPathToImpl x (ModuleOrNamespaceImpl(p,c,d,e,f,g,h)) = ModuleOrNamespaceImpl(x@p,c,d,e,f,g,h)
let PrepandPathToSpec x (ModuleOrNamespaceSpec(p,c,d,e,f,g,h)) = ModuleOrNamespaceSpec(x@p,c,d,e,f,g,h)

let PrependPathToInput x inp = 
    match inp with 
    | ImplFileInput (ImplFile(b,c,q,d,hd,impls,e)) -> ImplFileInput (ImplFile(b,c,PrepandPathToQualFileName x q,d,hd,List.map (PrepandPathToImpl x) impls,e))
    | SigFileInput (SigFile(b,q,d,hd,specs)) -> SigFileInput(SigFile(b,PrepandPathToQualFileName x q,d,hd,List.map (PrepandPathToSpec x) specs))

let ComputeAnonModuleName check defaultNamespace filename m = 
    let modname = CanonicalizeFilename filename
    if check && not (modname |> String.for_all (fun c -> System.Char.IsLetterOrDigit(c) || c = '_')) then
          if not (filename.EndsWith("fsx",StringComparison.OrdinalIgnoreCase) || filename.EndsWith("fsscript",StringComparison.OrdinalIgnoreCase)) then // bug://2893
              warning(Error(sprintf "The declarations in this file will be placed in an implicit module '%s' based on the file name '%s'. However this is not a valid F# identifier, so the contents will not be accessible from other files. Consider renaming the file or adding a 'module' or 'namespace' declaration at the top of the file" modname (Path.GetFileName filename),m));
    let combined = 
      match defaultNamespace with 
      | None -> modname
      | Some ns -> text_of_path [ns;modname]
    path_to_lid m (split_namespace combined)

let PostParseModuleImpl i defaultNamespace filename impl = 
    match impl with 
    | NamedTopModuleImpl(ModuleOrNamespaceImpl(lid,isModule,decls,xmlDoc,attribs,access,m)) -> 
      ModuleOrNamespaceImpl(lid,isModule,decls,xmlDoc,attribs,access,m)
    | AnonTopModuleImpl (defs,m)-> 
        let modname = ComputeAnonModuleName (nonNil defs) defaultNamespace filename m
        ModuleOrNamespaceImpl(modname,true,defs,emptyPreXmlDoc,[],None,m)
    | AnonNamespaceFragmentImpl (nsname,b,c,d,e,m)-> 
        ModuleOrNamespaceImpl(nsname,b,c,d,e,None,m)

let PostParseModuleSpec i  defaultNamespace filename intf = 
    match intf with 
    | NamedTopModuleSpec(x) -> x 
    | AnonTopModuleSpec (defs,m) -> 
        let modname = ComputeAnonModuleName (nonNil defs) defaultNamespace filename m
        ModuleOrNamespaceSpec(modname,true,defs,emptyPreXmlDoc,[],None,m)
    | AnonNamespaceFragmentSpec (nsname,b,c,d,e,m)-> 
        ModuleOrNamespaceSpec(nsname,b,c,d,e,None,m)



let IsScript filename = 
    let lower = String.lowercase filename 
    scriptSuffixes |> List.exists (Filename.check_suffix lower)
    
    
let PostParseModuleImpls (defaultNamespace,filename,canContainEntryPoint,ParsedImplFile(hashDirectives,impls)) = 
    let impls = impls |> List.mapi (fun i x -> PostParseModuleImpl i defaultNamespace filename x) 
    let qualName = QualFileNameOfImpls filename impls
    let isScript = IsScript filename

    let scopedPragmas = 
        [ for (ModuleOrNamespaceImpl(_,_,decls,_,_,_,_)) in impls do 
            for d in decls do
                match d with 
                | Def_hash (hd,_) -> yield! GetScopedPragmasForHashDirective hd
                | _ -> () 
          for hd in hashDirectives do 
              yield! GetScopedPragmasForHashDirective hd ]
    ImplFileInput(ImplFile(filename,isScript,qualName,scopedPragmas,hashDirectives,impls,canContainEntryPoint))
  
let PostParseModuleSpecs (defaultNamespace,filename,ParsedSigFile(hashDirectives,specs)) = 
    let specs = specs |> List.mapi (fun i x -> PostParseModuleSpec i defaultNamespace filename x) 
    let qualName = QualFileNameOfSpecs filename specs
    let scopedPragmas = 
        [ for (ModuleOrNamespaceSpec(_,_,decls,_,_,_,_)) in specs do 
            for d in decls do
                match d with 
                | Spec_hash(hd,_) -> yield! GetScopedPragmasForHashDirective hd
                | _ -> () 
          for hd in hashDirectives do 
              yield! GetScopedPragmasForHashDirective hd ]

    SigFileInput(SigFile(filename,qualName,scopedPragmas,hashDirectives,specs))

let ParseInput (lexer,errorLogger:ErrorLogger,lexbuf:UnicodeLexing.Lexbuf,defaultNamespace,filename,canContainEntryPoint) = 
    // The assert below is almost ok, but it fires in two cases:
    //  - fsi.exe sometimes passes "stdin" as a dummy filename
    //  - if you have a #line directive, e.g. 
    //        # 1000 "Line01.fs"
    //    then it also asserts.  But these are edge cases that can be fixed later, e.g. in bug 4651.
    //System.Diagnostics.Debug.Assert(System.IO.Path.IsPathRooted(filename), sprintf "should be absolute: '%s'" filename)
    let lower = String.lowercase filename 
    // Delay sending errors and warnings until after the file is parsed. This gives us a chance to scrape the
    // #nowarn declarations for the file
    let filteringErrorLogger = ErrorLoggerFilteringByScopedPragmas(false,[],errorLogger)
    let errorLogger = DelayedErrorLogger(filteringErrorLogger)
    use unwind = InstallGlobalErrorLogger (fun _ -> errorLogger)
    try     
        let input = 
            if implSuffixes |> List.exists (Filename.check_suffix lower)   then  
                let impl = Parser.implementationFile lexer lexbuf 
                PostParseModuleImpls (defaultNamespace,filename,canContainEntryPoint,impl)
            elif sigSuffixes |> List.exists (Filename.check_suffix lower)  then  
                let intfs = Parser.signatureFile lexer lexbuf 
                PostParseModuleSpecs (defaultNamespace,filename,intfs)
            else 
                errorLogger.Error(InternalError("ParseInput: unknown file suffix",Range.rangeStartup))
        filteringErrorLogger.ScopedPragmas <- GetScopedPragmasForInput input
        input
    finally
        // OK, now commit the errors, since the ScopedPragmas will (hopefully) have been scraped
        errorLogger.CommitDelayedErrorsAndWarnings()
    
      
     

[<Sealed>] 
type TcAssemblyResolutions(results : AssemblyResolution list, unresolved : UnresolvedReference list) = 

    let originalReferenceToResolution = results |> List.map (fun r -> r.originalReference.Text,r) |> Map.of_list
    let resolvedPathToResolution      = results |> List.map (fun r -> r.resolvedPath,r) |> Map.of_list

    /// Add some resolutions to the map of resolution results.                
    member tcResolutions.AddResolutionResults(newResults) = TcAssemblyResolutions(newResults @ results, unresolved)
    /// Add some unresolved results.
    member tcResolutions.AddUnresolvedReferences(newUnresolved) = TcAssemblyResolutions(results, newUnresolved @ unresolved)

    /// Get information about referenced DLLs
    member tcResolutions.GetAssemblyResolutions() = results
    member tcResolutions.GetUnresolvedReferences() = unresolved
    member tcResolutions.TryFindByOriginalReference(assemblyReference:AssemblyReference) = originalReferenceToResolution.TryFind assemblyReference.Text
    member tcResolutions.TryFindByResolvedName(nm) = resolvedPathToResolution.TryFind nm
        
    static member Resolve (tcConfig:TcConfig,assemblyList:AssemblyReference list) : TcAssemblyResolutions =
        let resolved,unresolved = 
            if tcConfig.useMonoResolution then 
                assemblyList |> List.map tcConfig.ResolveLibWithDirectories, []
            else
                TcConfig.TryResolveLibsUsingMSBuildRules (tcConfig,assemblyList,rangeStartup,ReportErrors)
        TcAssemblyResolutions(resolved,unresolved)                    


    static member GetAllDllReferences (tcConfig:TcConfig) =
        [ yield tcConfig.MscorlibDllReference()
          //yield tcConfig.SystemDllReference()
          if not tcConfig.compilingFslib then 
              yield tcConfig.CoreLibraryDllReference()

          if tcConfig.framework then 
              for s in coreFramework do yield AssemblyReference(rangeStartup,s^".dll")
              for s in extendedFramework do yield AssemblyReference(rangeStartup,s^".dll")
          if tcConfig.useFsiAuxLib then 
              let name = Filename.concat tcConfig.fsharpBinariesDir (fsiaux()^".dll")
              yield AssemblyReference(rangeStartup,name) 
          yield! tcConfig.referencedDLLs ]

    static member SplitNonFoundationalResolutions (tcConfig:TcConfig) =
        let assemblyList = TcAssemblyResolutions.GetAllDllReferences tcConfig
        let resolutions = TcAssemblyResolutions.Resolve(tcConfig,assemblyList)
        let frameworkDLLs,nonFrameworkReferences = resolutions.GetAssemblyResolutions() |> List.partition (fun r -> r.sysdir) 
        let unresolved = resolutions.GetUnresolvedReferences()
#if TRACK_DOWN_EXTRA_BACKSLASHES        
        frameworkDLLs |> List.iter(fun x ->
            let path = x.resolvedPath 
            System.Diagnostics.Debug.Assert(not(path.Contains(@"\\")), "SplitNonFoundationalResolutions results in a non-canonical filename with extra backslashes: "^path)
            )
        nonFrameworkReferences |> List.iter(fun x ->
            let path = x.resolvedPath 
            System.Diagnostics.Debug.Assert(not(path.Contains(@"\\")), "SplitNonFoundationalResolutions results in a non-canonical filename with extra backslashes: "^path)
            )
#endif       
#if DEBUG
        let itFailed = ref false
        let addedText = "\nIf you want to debug this right now, attach a debugger, and put a breakpoint in 'build.ml' near the text '!itFailed', and you can re-step through the assembly resolution logic."
        unresolved 
        |> List.iter (fun (UnresolvedReference(referenceText,ranges)) ->
            if referenceText.Contains("mscorlib") then
                System.Diagnostics.Debug.Assert(false, sprintf "whoops, did not resolve mscorlib: '%s'%s" referenceText addedText)
                itFailed := true)
        frameworkDLLs 
        |> List.iter (fun x -> 
            if not(System.IO.Path.IsPathRooted(x.resolvedPath)) then
                System.Diagnostics.Debug.Assert(false, sprintf "frameworkDLL should be absolute path: '%s'%s" x.resolvedPath addedText)
                itFailed := true)
        nonFrameworkReferences 
        |> List.iter (fun x -> 
            if not(System.IO.Path.IsPathRooted(x.resolvedPath)) then
                System.Diagnostics.Debug.Assert(false, sprintf "nonFrameworkReference should be absolute path: '%s'%s" x.resolvedPath addedText) 
                itFailed := true)
        if !itFailed then
            // idea is, put a breakpoint here and then step through
            let assemblyList = TcAssemblyResolutions.GetAllDllReferences tcConfig
            let resolutions = TcAssemblyResolutions.Resolve(tcConfig,assemblyList)
            let frameworkDLLs,nonFrameworkReferences = resolutions.GetAssemblyResolutions() |> List.partition (fun r -> r.sysdir) 
            ()
#endif
        frameworkDLLs,nonFrameworkReferences,unresolved

    static member BuildFromPriorResolutions (tcConfig:TcConfig,resolutions) =
        let references = resolutions |> List.map (fun r -> r.originalReference)
        TcAssemblyResolutions.Resolve(tcConfig,references)
            

//----------------------------------------------------------------------------
// Typecheck and optimization environments on disk
//--------------------------------------------------------------------------
open Pickle

let IsSignatureDataResource r = String.hasPrefix r.resourceName FSharpSignatureDataResourceName
let IsOptDataResource r = String.hasPrefix r.resourceName FSharpOptimizationDataResourceName
let GetSignatureDataResourceName r = String.dropPrefix (String.dropPrefix r.resourceName FSharpSignatureDataResourceName) "."
let GetOptDataResourceName r = String.dropPrefix (String.dropPrefix r.resourceName FSharpOptimizationDataResourceName) "."

let IsReflectedDefinitionsResource r = String.hasPrefix r.resourceName Sreflect.pickledDefinitionsResourceNameBase

let UnpickleFromResource file m u sref r = 
    match r.resourceWhere with 
    | Resource_local b -> unpickle_obj_with_dangling_ccus file sref u (b())
    | _-> error(InternalError("UnpickleFromResource",m))

let MakeILResource rname bytes = 
    { resourceName = rname;
      resourceWhere = Resource_local (fun () -> bytes);
      resourceAccess = Resource_public;
      resourceCustomAttrs = mk_custom_attrs [] }

let PickleToResource file g scope rname p x = 
    { resourceName = rname;
      resourceWhere = (let bytes = pickle_obj_with_dangling_ccus file g scope p x in Resource_local (fun () -> bytes));
      resourceAccess = Resource_public;
      resourceCustomAttrs = mk_custom_attrs [] }


let GetSignatureData file m sref r : PickledDataWithReferences<PickledModuleInfo> = 
    UnpickleFromResource file m UnpickleModuleInfo sref r

let WriteSignatureData (tcConfig:TcConfig,tcGlobals,exportRemapping,ccu:ccu,file) : ILResource = 
    let mspec = ccu.Contents
    if !verboseStamps then 
        dprintf "Signature data before remap:\n%s\n" (Layout.showL (Layout.squashTo 192 (EntityL mspec)));
        dprintf "---------------------- START OF APPLYING EXPORT REMAPPING TO SIGNATURE DATA------------\n";
    let mspec = ApplyExportRemappingToEntity tcGlobals exportRemapping mspec
    if !verboseStamps then 
        dprintf "---------------------- END OF APPLYING EXPORT REMAPPING TO SIGNATURE DATA------------\n";
        dprintf "Signature data after remap:\n%s\n" (Layout.showL (Layout.squashTo 192 (EntityL mspec)));
    PickleToResource file tcGlobals ccu (FSharpSignatureDataResourceName^"."^ccu.AssemblyName) PickleModuleInfo 
        { mspec=mspec; 
          compile_time_working_dir=tcConfig.implicitIncludeDir;
          usesQuotations = ccu.UsesQuotations }

let GetOptimizationData file m sref ca = 
    UnpickleFromResource file m Opt.u_lazy_modul_info sref ca

let WriteOptData tcGlobals file (ccu,modulInfo) = 
    if verbose then  dprintf "Optimization data after remap:\n%s\n" (Layout.showL (Layout.squashTo 192 (Opt.moduleInfoL modulInfo)));
    PickleToResource file tcGlobals ccu (FSharpOptimizationDataResourceName^"."^ccu.AssemblyName) Opt.p_lazy_modul_info modulInfo

//----------------------------------------------------------------------------
// Names to match up refs and defs for assemblies and modules
//--------------------------------------------------------------------------

let GetNameOfScopeRef sref = 
    match sref with 
    | ScopeRef_local -> "<local>"
    | ScopeRef_module mref -> mref.Name
    | ScopeRef_assembly aref -> aref.Name
  
let GetNameOfILModule m = if module_is_mainmod m then assname_of_mainmod m else m.modulName


let MakeScopeRefForIlModule tcConfig ilModule = 
    if module_is_mainmod ilModule then 
        ScopeRef_assembly (assref_for_mainmod  ilModule)
    else
        ScopeRef_module (modref_for_modul ilModule)

let GetCustomAttributesOfIlModule (ilModule:ILModuleDef) = 
    dest_custom_attrs (match ilModule.Manifest with Some m -> m.CustomAttrs | None -> ilModule.CustomAttrs) 

let GetAutoOpenAttributes(ilModule) = 
    ilModule |> GetCustomAttributesOfIlModule |> List.choose TryFindAutoOpenAttr

let GetInternalsVisibleToAttributes ilModule = 
    ilModule |> GetCustomAttributesOfIlModule |> List.choose TryFindInternalsVisibleToAttr

//----------------------------------------------------------------------------
// Relink blobs of saved data by fixing up ccus.
//--------------------------------------------------------------------------

type ImportedBinary = 
    { FileName: string;
      IsFSharpBinary: bool; 
      RawMetadata: ILModuleDef; 
      ILAssemblyRefs : ILAssemblyRef list;
      ILScopeRef: ILScopeRef }

type ImportedAssembly = 
    { ILScopeRef: ILScopeRef; 
      FSharpViewOfMetadata: ccu;
      AssemblyAutoOpenAttributes: string list;
      AssemblyInternalsVisibleToAttributes: string list;
      FSharpOptimizationData : Lazy<Option<Opt.LazyModuleInfo>> }

type AvailableImportedAssembly =
    | ResolvedImportedAssembly of ImportedAssembly
    | UnresolvedImportedAssembly of string

let availableToOptionalCcu = function
    | ResolvedCcu(ccu) -> Some(ccu)
    | UnresolvedCcu(assemblyName) -> None


//----------------------------------------------------------------------------
// TcConfigProvider
//--------------------------------------------------------------------------

type TcConfigProvider = 
    | TcConfigProvider of (unit -> TcConfig)
    member x.Get() = (let (TcConfigProvider(f)) = x in f())
    static member Constant(tcConfig) = TcConfigProvider(fun () -> tcConfig)
    static member BasedOnMutableBuilder(tcConfigB) = TcConfigProvider(fun () -> TcConfig.Create(tcConfigB,validate=false))
    
    
//----------------------------------------------------------------------------
// TcImports
//--------------------------------------------------------------------------

          
/// Tables of imported assemblies.      
[<Sealed>] 
type TcImports(tcConfigP:TcConfigProvider,initialResolutions:TcAssemblyResolutions, importsBase:TcImports option,ilGlobalsOpt) = 
    let mutable resolutions = initialResolutions

    let mutable importsBase : TcImports option = importsBase
    let mutable dllInfos: ImportedBinary list = []
    let mutable dllTable: ImportedBinary NameMap = NameMap.empty
    let mutable ccuInfos: ImportedAssembly list = []
    let mutable ccuTable: ImportedAssembly NameMap = NameMap.empty
    let mutable disposeActions = []
    let mutable originalReferenceToResolution : Map<string,AssemblyResolution> = Map.empty 
    let mutable resolvedPathToResolution : Map<string,AssemblyResolution> = Map.empty 
#if DEBUG
    let mutable disposed = false
#endif
    let mutable ilGlobalsOpt = ilGlobalsOpt
    let mutable tcGlobals = None
    
    let CheckDisposed() =
#if DEBUG
        if disposed then failwith "Use of Disposed TcConfig"
#else   
        ()        
#endif
  
    member tcImports.SetBase(baseTcImports) =
        CheckDisposed()
        importsBase <- Some(baseTcImports)

    member private tcImports.Base 
        with get() = 
            CheckDisposed()
            importsBase

    member tcImports.CcuTable 
        with get() = 
            CheckDisposed()
            ccuTable
        
    member private tcImports.DllTable 
        with get() = 
            CheckDisposed()
            dllTable        
        
    member tcImports.RegisterCcu(ccuInfo) =
        CheckDisposed()
        ccuInfos <- ccuInfos ++ ccuInfo;
        // Assembly Ref Resolution: remove this use of ccu.AssemblyName
        ccuTable <- NameMap.add (ccuInfo.FSharpViewOfMetadata.AssemblyName) ccuInfo ccuTable
    
    member tcImports.RegisterDll(dllInfo) =
        CheckDisposed()
        dllInfos <- dllInfos ++ dllInfo;
        dllTable <- NameMap.add (GetNameOfScopeRef dllInfo.ILScopeRef) dllInfo dllTable

    member tcImports.GetDllInfos() = 
        CheckDisposed()
        match importsBase with 
        | Some(importsBase)-> importsBase.GetDllInfos() @ dllInfos
        | None -> dllInfos
        
    member tcImports.FindDllInfo (m,assemblyName) =
        CheckDisposed()
        let rec look(t:TcImports) = 
            match NameMap.tryfind assemblyName t.DllTable with
            | Some res -> Some(res)
            | None -> 
                match t.Base with 
                | Some t2 -> look(t2)
                | None -> None
        match look(tcImports) with
        | Some res -> res
        | None ->
            tcImports.ImplicitLoadIfAllowed(m,assemblyName);
            match look(tcImports) with 
            | Some res -> res
            | None -> error(Error("could not resolve assembly "^assemblyName,m))
    

    member tcImports.FindDllInfoFromAssemblyRef(m,assref:ILAssemblyRef) = 
        CheckDisposed()
        tcImports.FindDllInfo(m,assref.Name) 
        
        
    member tcImports.GetCcuInfos() = 
        CheckDisposed()
        match importsBase with 
        | Some(importsBase)-> importsBase.GetCcuInfos() @ ccuInfos
        | None -> ccuInfos        
        
    member tcImports.GetCcusInDeclOrder() =         
        CheckDisposed()
        List.map (fun x -> x.FSharpViewOfMetadata) (tcImports.GetCcuInfos())  
        
    // This is the main "assembly reference --> assembly" resolution routine. 
    // We parameterize by a fallback resolution function that will go and look for DLLs matching the assembly name 
    // in the include search paths. 
    member tcImports.FindCcuInfo (m,assemblyName) = 
        CheckDisposed()
        let rec look (t:TcImports) = 
            match NameMap.tryfind assemblyName t.CcuTable with
            | Some res -> Some(res)
            | None -> 
                 match t.Base with 
                 | Some t2 -> look t2 
                 | None -> None

        match look(tcImports) with
        | Some res -> ResolvedImportedAssembly(res)
        | None ->
            tcImports.ImplicitLoadIfAllowed(m,assemblyName);
            match look(tcImports) with 
            | Some res -> ResolvedImportedAssembly(res)
            | None -> UnresolvedImportedAssembly(assemblyName)
        

    member tcImports.FindCcu(m,assemblyName) = 
        CheckDisposed()
        match tcImports.FindCcuInfo(m,assemblyName) with
        | ResolvedImportedAssembly(importedAssembly) -> ResolvedCcu(importedAssembly.FSharpViewOfMetadata)
        | UnresolvedImportedAssembly(assemblyName) -> UnresolvedCcu(assemblyName)

    member tcImports.FindCcuFromAssemblyRef(m,assref:ILAssemblyRef) = 
        CheckDisposed()
        match tcImports.FindCcuInfo(m,assref.Name) with
        | ResolvedImportedAssembly(importedAssembly) -> ResolvedCcu(importedAssembly.FSharpViewOfMetadata)
        | UnresolvedImportedAssembly(assemblyName) -> UnresolvedCcu(assref.QualifiedName)

    member tcImports.AssemblyLoader = 
        Import.AssemblyLoader (fun (m,assref) -> tcImports.FindCcuFromAssemblyRef(m,assref))

    member tcImports.AttachDisposeAction(action) =
        CheckDisposed()
        disposeActions <- action :: disposeActions
  
    override obj.ToString() = 
        sprintf "tcImports = \n    dllInfos=%A\n    dllTable=%A\n    ccuInfos=%A\n    ccuTable=%A\n    Base=%s\n"
            dllInfos
            dllTable
            ccuInfos
            ccuTable
            (match importsBase with None-> "None" | Some(importsBase) -> importsBase.ToString())
    
      
    // Note: the returned binary reader is associated with the tcImports, i.e. when the tcImports are closed 
    // then the reader is closed. 
    member tcImports.OpenIlBinaryModule(syslib,filename,m) = 
      try
        CheckDisposed()
        let tcConfig = tcConfigP.Get()
        let pdbPathOption = 
            // We open the pdb file if one exists parallel to the binary we 
            // are reading, so that --standalone will preserve debug information. 
            if tcConfig.openDebugInformationForLaterStaticLinking then 
                let pdbDir = (try Filename.dirname filename with _ -> ".") 
                let pdbFile = (try Filename.chop_extension filename with _ -> filename)^".pdb" 
                if Internal.Utilities.FileSystem.File.SafeExists pdbFile then 
                    if verbose then dprintf "reading PDB file %s from directory %s\n" pdbFile pdbDir;
                    Some pdbDir
                else 
                    None 
            else   
                None 


        let ilBinaryReader = OpenILBinary(filename,tcConfig.optimizeForMemory,tcConfig.openBinariesInMemory,ilGlobalsOpt,pdbPathOption,tcConfig.mscorlibAssemblyName)

        tcImports.AttachDisposeAction(fun _ -> Ilread.CloseILModuleReader ilBinaryReader);
        ilBinaryReader.ILModuleDef, ilBinaryReader.ILAssemblyRefs
      with e ->
        error(Error(sprintf "Error opening binary file '%s': %s" filename e.Message,m))



    (* auxModTable is used for multi-module assemblies *)
    member tcImports.MkLoaderForMultiModuleIlAssemblies m syslib =
        CheckDisposed()
        let auxModTable = Hashtbl.create 10
        fun viewedScopeRef ->
        
            let tcConfig = tcConfigP.Get()
            match viewedScopeRef with
            | ScopeRef_module modref -> 
                let key = modref.Name
                if not (auxModTable.ContainsKey(key)) then
                    let resolution = tcConfig.ResolveLibWithDirectories(AssemblyReference(m,key))
                    let ilModule,_ = tcImports.OpenIlBinaryModule(syslib,resolution.resolvedPath,m)
                    auxModTable.[key] <- ilModule
                auxModTable.[key] 

            | _ -> 
                error(InternalError("Unexpected ScopeRef_local or ScopeRef_assembly in exported type table",m))

    member tcImports.IsAlreadyRegistered nm =
        CheckDisposed()
        tcImports.GetDllInfos() |> List.exists (fun dll -> 
            match dll.ILScopeRef with 
            | ScopeRef_assembly a -> a.Name =  nm 
            | _ -> false)

    member tcImports.GetImportMap() = 
        CheckDisposed()
        new Import.ImportMap (tcImports.GetTcGlobals(),tcImports.AssemblyLoader)

    // Note the tcGlobals are only available once mscorlib and fslib have been established. For TcImports, 
    // they are logically only needed when converting AbsIL data structures into F# data structures, and
    // when converting AbsIL types in particular, since these types are normalized through the tables
    // in the tcGlobals (E.g. normalizing 'System.Int32' to 'int'). On the whole ImportIlAssembly doesn't
    // actually convert AbsIL types - it only converts the outer shell of type definitions - the vast majority of
    // types such as those in method signatures are currently converted on-demand. However ImportILAssembly does have to
    // convert the types that are constraints in generic parameters, which was the original motivation for making sure that
    // ImportILAssembly had a tcGlobals available when it really needs it.
    member tcImports.GetTcGlobals() =
        CheckDisposed()
        match tcGlobals with 
        | Some(g) -> g 
        | None -> 
            match importsBase with 
            | Some(b) -> b.GetTcGlobals() 
            | None -> failwith "unreachable: GetGlobals"

    member private tcImports.SetILGlobals(ilg) =
        CheckDisposed()
        ilGlobalsOpt <- Some(ilg)

    member private tcImports.SetTcGlobals(g) =
        CheckDisposed()
        tcGlobals <- Some(g)

    // Add a referenced assembly
    //
    // Retargetable assembly refs are required for binaries that must run 
    // against DLLs supported by multiple publishers. For example
    // Compact Framework binaries must use this. However it is not
    // clear when else it is required, e.g. for Mono.
    
    member tcImports.PrepareToImportReferencedIlDll syslib m filename (dllinfo:ImportedBinary) =
        CheckDisposed()
        let tcConfig = tcConfigP.Get()
        let ilModule = dllinfo.RawMetadata
        let sref = dllinfo.ILScopeRef
        let aref =   
            match sref with 
            | ScopeRef_assembly aref -> aref 
            | _ -> error(InternalError("PrepareToImportReferencedIlDll: cannot reference .NET netmodules directly, reference the containing assembly instead",m))

        let nm = aref.Name
        if verbose then dprintn ("Converting IL assembly to F# data structures "^nm);
        let auxModuleLoader = tcImports.MkLoaderForMultiModuleIlAssemblies m syslib
        let ccu = Import.ImportIlAssembly(tcImports.GetImportMap,m,auxModuleLoader,sref,tcConfig.implicitIncludeDir, Some filename,ilModule)
        let ccuinfo = 
            { FSharpViewOfMetadata=ccu; 
              ILScopeRef = sref; 
              AssemblyAutoOpenAttributes = GetAutoOpenAttributes(ilModule);
              AssemblyInternalsVisibleToAttributes = GetInternalsVisibleToAttributes(ilModule);
              FSharpOptimizationData = notlazy None }
        tcImports.RegisterCcu(ccuinfo);
        let phase2 () = [ResolvedImportedAssembly(ccuinfo)]
        phase2

    member tcImports.PrepareToImportReferencedFSharpDll syslib m filename (dllinfo:ImportedBinary) =
        CheckDisposed()
        let tcConfig = tcConfigP.Get()
        tcConfig.CheckFSharpBinary(filename,dllinfo.ILAssemblyRefs,m)
        let ilModule = dllinfo.RawMetadata 
        let sref = dllinfo.ILScopeRef 
        let modname = GetNameOfScopeRef sref 
        if verbose then dprintn ("Converting F# assembly to F# data structures "^(GetNameOfScopeRef sref));
        let attrs = GetCustomAttributesOfIlModule ilModule 
        assert (List.exists IsSignatureDataVersionAttr attrs);
        if verbose then dprintn ("Relinking interface info from F# assembly "^modname);
        let resources = dest_resources ilModule.modulResources 
        assert (List.exists IsSignatureDataResource resources);
        let optDataFromResource = 
            resources 
            |> List.choose (fun r -> if IsOptDataResource r then Some(GetOptDataResourceName r,r) else None)
        let ccuRawDataAndInfos = 
            resources 
            |> List.filter IsSignatureDataResource 
            |> List.map (fun iresource -> 
                let ccuName = GetSignatureDataResourceName iresource 
                let data = GetSignatureData filename m sref iresource 

                // Look for optimization data in a file 
                let optDataFromFile = 
                    let optDataFileName = (Filename.chop_extension filename)^".optdata" 
                    if Internal.Utilities.FileSystem.File.SafeExists optDataFileName then 
                       try Some(ccuName,MakeILResource optDataFileName (File.ReadAllBytes optDataFileName))
                       with _ -> None 
                    else None 
                     
                let optDatas =
                    // If F# optData is written to file, the DLL still contains "essential" optData - i.e. the must inline information.
                    // If we find optData in a file, we should use that one in preference to resource optData.
                    // We choose one or the other. 
                    match optDataFromFile,optDataFromResource with
                    | None        ,[]       -> warning(Error(Printf.sprintf "No optimization information found for compilation unit '%s'" ccuName,m)); Map.empty
                    | Some optData,_        -> Map.of_list [optData]    // prefer optData from file if available, since it implies DLL optData is limitted to "essential"
                    | None        ,optDatas -> Map.of_list optDatas     // Espect this route, optData from file is being disabled (but kept around).

                let minfo : PickledModuleInfo = data.RawData 
                let mspec = minfo.mspec 


                // Adjust where the code for known F# libraries live relative to the installation of F#
                let code_dir = 
                    let dir = minfo.compile_time_working_dir
                    let knownLibraryLocation = @"src\fsharp\" // Help highlighting... " 
                    let knownLibarySuffixes = 
                        [ @"FSharp.Core";
                          @"FSharp.PowerPack"; 
                          @"FSharp.PowerPack.Linq"; 
                          @"FSharp.PowerPack.Metadata"  ]
                    match knownLibarySuffixes |> List.tryfind (fun x -> dir.EndsWith(knownLibraryLocation + x,StringComparison.OrdinalIgnoreCase)) with
                    | None -> 
                        dir
                    | Some libSuffix -> 
                        // chop off 'FSharp.Core'
                        let dir = dir.[0..dir.Length-1-libSuffix.Length]
                        // chop off 'src\fsharp\'
                        let dir = dir.[0..dir.Length-1-knownLibraryLocation.Length]
                        // add "..\lib\FSharp.Core" to the F# binaries directory
                        let dir = Path.Combine(Path.Combine(tcConfig.fsharpBinariesDir,@"..\lib"),libSuffix)
                        dir

                let ccu = 
                   new_ccu ccuName
                    { ccu_scoref=sref;
                      ccu_stamp = new_stamp();
                      ccu_filename = Some filename; 
                      ccu_qname= Some(sref.QualifiedName);
                      ccu_code_dir = code_dir;  (* note: in some cases we fix up this information later *)
                      ccu_fsharp=true;
                      ccu_contents = mspec; 
                      ccu_usesQuotations = minfo.usesQuotations;
                      ccu_forwarders = lazy Map.empty }

                let optdata = 
                    lazy 
                        (match Map.tryfind ccuName optDatas  with 
                         | None -> 
                            if verbose then dprintf "*** no optimization data for CCU %s, was DLL compiled with --no-optimization-data??\n" ccuName; 
                            None
                         | Some info -> 
                            let data = GetOptimizationData filename m sref info 
                            let res = data.OptionalFixup(fun nm -> availableToOptionalCcu(tcImports.FindCcu(m,nm))) 
                            if verbose then dprintf "found optimization data for CCU %s\n" ccuName; 
                            Some res)  
                data,{ FSharpViewOfMetadata=ccu; 
                       AssemblyAutoOpenAttributes = GetAutoOpenAttributes(ilModule);
                       AssemblyInternalsVisibleToAttributes = GetInternalsVisibleToAttributes(ilModule);
                       FSharpOptimizationData=optdata; 
                       ILScopeRef = sref }  )
                     
        // Register all before relinking to cope with mutually-referential ccus 
        ccuRawDataAndInfos |> List.iter (snd >> tcImports.RegisterCcu);
        let phase2 () = 
            (* Relink *)
            (* dprintf "Phase2: %s\n" filename; REMOVE DIAGNOSTICS *)
            ccuRawDataAndInfos |> List.iter (fun (data,_) -> data.OptionalFixup(fun nm -> availableToOptionalCcu(tcImports.FindCcu(m,nm))) |> ignore);
            ccuRawDataAndInfos |> List.map snd |> List.map ResolvedImportedAssembly  
        phase2
         

    member tcImports.RegisterAndPrepareToImportReferencedDll warnIfAlreadyLoaded (r:AssemblyResolution) : _*(unit -> AvailableImportedAssembly list)=
        CheckDisposed()
        let tcConfig = tcConfigP.Get()
        let m = r.originalReference.Range
        let filename = r.resolvedPath
        let syslib = r.sysdir
        let ilModule,ilAssemblyRefs = tcImports.OpenIlBinaryModule(syslib,filename,m)

        let modname = GetNameOfILModule ilModule 
        if tcImports.IsAlreadyRegistered modname then 
            let dllinfo = tcImports.FindDllInfo(m,modname)
            let phase2() = [tcImports.FindCcuInfo(m,modname)]
            dllinfo,phase2
        else 
            let sref = MakeScopeRefForIlModule tcConfig ilModule
            let dllinfo = {RawMetadata=ilModule; 
                           FileName=filename;
                           IsFSharpBinary=true; 
                           ILScopeRef = sref;
                           ILAssemblyRefs = ilAssemblyRefs }
            tcImports.RegisterDll(dllinfo);
            let attrs = GetCustomAttributesOfIlModule ilModule
            let phase2 = 
                if (List.exists IsSignatureDataVersionAttr attrs) then 
                    if not (List.exists (IsMatchingSignatureDataVersionAttr (IL.parse_version Ilxconfig.version)) attrs) then 
                      errorR(Error("The F#-compiled DLL '"^filename^"' needs to be recompiled to be used with this version of F#",m));
                      tcImports.PrepareToImportReferencedIlDll syslib m filename dllinfo
                    else 
                      tcImports.PrepareToImportReferencedFSharpDll syslib m filename dllinfo
                else 
                    tcImports.PrepareToImportReferencedIlDll syslib m filename dllinfo
            dllinfo,phase2

    member tcImports.RegisterAndImportReferencedAssemblies  warnIfAlreadyLoaded (nms:AssemblyResolution list) =
        CheckDisposed()
        
        let dllinfos,phase2s = 
           nms |> List.map (tcImports.RegisterAndPrepareToImportReferencedDll warnIfAlreadyLoaded)
               |> List.unzip
        let ccuinfos = (List.collect (fun phase2 -> phase2()) phase2s) 
        dllinfos,ccuinfos
      
    member tcImports.DoRegisterAndImportReferencedAssemblies nms = 
        CheckDisposed()
        tcImports.RegisterAndImportReferencedAssemblies true nms |> ignore

    member tcImports.ImplicitLoadIfAllowed(m,assemblyName) = 
        CheckDisposed()
        // If the user is asking for the default framework then also try to resolve other implicit assemblies as they are discovered.
        // Using this flag to mean 'allow implicit discover of assemblies'.
        let tcConfig = tcConfigP.Get()
        if tcConfig.implicitlyResolveAssemblies then 
            let tryFile speculativeFileName = 
                let foundFile = 
                    try Some(tcImports.ResolveLibFile (AssemblyReference(m,speculativeFileName),ResolveLibFileMode.Speculative))
                    with 
                        // Don't re-report the load error
                        | AssemblyNotResolved _ 
                        | FileNameNotResolved _ -> None 
                match foundFile with 
                | None -> None
                | Some res -> 
                     //if not tcConfig.noFeedback then dprintf "Implicitly referencing '%s'...\n" fileName;
                     tcImports.DoRegisterAndImportReferencedAssemblies [res]
                     Some()

            match tryFile (assemblyName^".dll") with 
            | Some() -> ()
            | None -> tryFile (assemblyName^".exe") |> ignore
            

    member tcImports.TryResolveLibFile (assemblyReference:AssemblyReference,mode:ResolveLibFileMode): OperationResult<AssemblyResolution> = 
        let tcConfig = tcConfigP.Get()
        // First try the prior resolutions map.
        match resolutions.TryFindByOriginalReference assemblyReference with
        | Some(assemblyResolution) -> 
            ResultD(assemblyResolution)
        | None ->
            match resolutions.TryFindByResolvedName assemblyReference.Text with 
            | Some(assemblyResolution) -> 
                ResultD(assemblyResolution)
            | None ->       
                if tcConfigP.Get().useMonoResolution then
                   try 
                       ResultD(tcConfig.ResolveLibWithDirectories assemblyReference)
                   with e -> 
                       ErrorD(e)
                else 
                    // This is a previously unencounterd assembly. Resolve it and add it to the list.
                    // But don't cache resolution failures because the assembly may appear on the disk later.
                    let resolved,unresolved = TcConfig.TryResolveLibsUsingMSBuildRules(tcConfig,[ assemblyReference ],assemblyReference.Range,mode)
                    match resolved,unresolved with
                    | (assemblyResolution::_,_)  -> 
                        resolutions <- resolutions.AddResolutionResults(resolved)
                        ResultD(assemblyResolution)
                    | (_,_::_)  -> 
                        resolutions <- resolutions.AddUnresolvedReferences(unresolved)
                        ErrorD(AssemblyNotResolved(assemblyReference.Text,assemblyReference.Range))
                    | [],[] -> 
                        // Note, if mode=ResolveLibFileMode.Speculative and the resolution failed then TryResolveLibsUsingMSBuildRules returns
                        // the empty list and we convert the failure into an AssemblyNotResolved here.
                        ErrorD(AssemblyNotResolved(assemblyReference.Text,assemblyReference.Range))

    /// Do TryResolveLibFile and commit the result

    member tcImports.ResolveLibFile (assemblyReference,mode) = 
         let opResult = tcImports.TryResolveLibFile(assemblyReference,mode) 
         CommitOperationResult opResult
         

    static member BuildFrameworkTcImports (tcConfigP:TcConfigProvider,frameworkDLLs) =
        use t = Trace.Call("Build","BuildFrameworkTcImports", fun _->"")

        let tcConfig = tcConfigP.Get()
        let tcResolutions = TcAssemblyResolutions.BuildFromPriorResolutions(tcConfig,frameworkDLLs)

        // mscorlib gets loaded first.
        let mscorlibReference = tcConfig.MscorlibDllReference()

        let frameworkTcImports = new TcImports(tcConfigP,tcResolutions,None,None) 
        
        let sysCcu =
            let mscorlibResolution = tcConfig.ResolveLibWithDirectories(mscorlibReference)
            //printfn "mscorlibResolution= %s" mscorlibResolution.resolvedPath
            match frameworkTcImports.RegisterAndImportReferencedAssemblies false [mscorlibResolution]  with
            | (_, [ResolvedImportedAssembly(sysCcu)]) -> sysCcu
            | _        -> error(InternalError("BuildFoundationalTcImports: no sysCcu for "^mscorlibReference.Text,rangeStartup))
        let ilGlobals   = IL.mk_ILGlobals sysCcu.FSharpViewOfMetadata.ILScopeRef (Some tcConfig.mscorlibAssemblyName) 
        frameworkTcImports.SetILGlobals ilGlobals

        // Load the rest of the framework DLLs all at once (they may be mutually recursive)
        frameworkTcImports.DoRegisterAndImportReferencedAssemblies (tcResolutions.GetAssemblyResolutions())

        let fslibCcu = 
            if tcConfig.compilingFslib then 
                // When compiling FSharp.Core.dll, the fslibCcu reference to FSharp.Core.dll is a delayed ccu thunk fixed up during type checking
                let fslibCcu = CcuThunk.CreateDelayed(GetFSharpCoreLibraryName())
                fslibCcu 
            else
                let fslibCcuInfo =
                    let coreLibraryReference = tcConfig.CoreLibraryDllReference()
                    //printfn "coreLibraryReference = %A" coreLibraryReference
                    match tcResolutions.TryFindByOriginalReference(coreLibraryReference) with 
                    | Some coreLibraryResolution -> 
                        //printfn "coreLibraryResolution = '%s'" coreLibraryResolution.resolvedPath
                        match frameworkTcImports.RegisterAndImportReferencedAssemblies false [coreLibraryResolution] with
                        | (_, [ResolvedImportedAssembly(fslibCcuInfo) ]) -> fslibCcuInfo
                        | _ -> 
                            error(InternalError("BuildFrameworkTcImports: no successful import of "^coreLibraryResolution.resolvedPath,coreLibraryResolution.originalReference.Range))
                    | None -> 
                        error(InternalError(sprintf "BuildFrameworkTcImports: no resolution of '%s'" coreLibraryReference.Text,rangeStartup))
                Msilxlib.ilxLibraryAssemRef := 
                    (let scoref = fslibCcuInfo.ILScopeRef
                     match scoref with
                     | ScopeRef_assembly aref             -> Some aref
                     | ScopeRef_local | ScopeRef_module _ -> error(InternalError("fslib_assembly_ref: not ScopeRef_assembly",rangeStartup)));
                fslibCcuInfo.FSharpViewOfMetadata                  

        // can't access system tuples on frameworks < v4.0
        match ilGlobals.mscorlib_scoref.AssemblyRef.Version with 
        | Some(v1,_,_,_) when v1 < 4us -> 
            Microsoft.FSharp.Compiler.Tastops.use_40_System_Types <- false
        | _ -> ()

        // OK, now we have both mscorlib.dll and FSharp.Core.dll we can create TcGlobals
        let tcGlobals = mk_tcGlobals(tcConfig.compilingFslib,sysCcu.FSharpViewOfMetadata,ilGlobals,fslibCcu,tcConfig.implicitIncludeDir,tcConfig.mlCompatibility, Microsoft.FSharp.Compiler.Tastops.use_40_System_Types) 
#if DEBUG
        // the global_g reference cell is used only for debug printing
        global_g := Some tcGlobals;
#endif
        // do this prior to parsing, since parsing IL assembly code may refer to mscorlib
        Microsoft.FSharp.Compiler.AbstractIL.Internal.AsciiConstants.parse_ilGlobals := tcGlobals.ilg; 
        frameworkTcImports.SetTcGlobals(tcGlobals)
        tcGlobals,frameworkTcImports
        
    static member BuildNonFrameworkTcImports (tcConfigP:TcConfigProvider,tcGlobals:TcGlobals, baseTcImports,nonFrameworkReferences) = 
        let tcConfig = tcConfigP.Get()
        let tcResolutions = TcAssemblyResolutions.BuildFromPriorResolutions(tcConfig,nonFrameworkReferences)
        let references = tcResolutions.GetAssemblyResolutions()
        let tcImports = new TcImports(tcConfigP,tcResolutions,Some baseTcImports, Some tcGlobals.ilg)
        tcImports.DoRegisterAndImportReferencedAssemblies references;
        tcImports

      
    static member BuildTcImports(tcConfigP:TcConfigProvider) = 
        let tcConfig = tcConfigP.Get()
        //let foundationalTcImports,tcGlobals = TcImports.BuildFoundationalTcImports(tcConfigP)
        let frameworkDLLs,nonFrameworkReferences,unresolvedReferences = TcAssemblyResolutions.SplitNonFoundationalResolutions tcConfig
        let tcGlobals,frameworkTcImports = TcImports.BuildFrameworkTcImports (tcConfigP,frameworkDLLs)
        let tcImports = TcImports.BuildNonFrameworkTcImports(tcConfigP,tcGlobals,frameworkTcImports,nonFrameworkReferences)
        tcGlobals,tcImports
        
    interface System.IDisposable with 
        member tcImports.Dispose() = 
            CheckDisposed()
            // disposing deliberately only closes this tcImports, not the ones up the chain 
#if DEBUG
            disposed <- true        
#endif
            if verbose then 
                dprintf "disposing of TcImports, %d binaries\n" disposeActions.Length;
            List.iter (fun f -> f()) disposeActions          

//----------------------------------------------------------------------------
// Add "#r" and "#I" declarations to the tcConfig
//--------------------------------------------------------------------------

// Add the reference and add the ccu to the type checking environment . Only used by F# Interactive
let RequireDLL (tcImports:TcImports) tcEnv m file = 
    let RequireResolved = function
        | ResolvedImportedAssembly(ccuinfo) -> ccuinfo
        | UnresolvedImportedAssembly(assemblyName) -> error(Error("could not resolve assembly '"^assemblyName^"' required by '"^file,m))
    let resolution = tcImports.ResolveLibFile(AssemblyReference(m,file),ResolveLibFileMode.ReportErrors)
    let dllinfos,ccuinfos = tcImports.RegisterAndImportReferencedAssemblies false [resolution]
    let ccuinfos = ccuinfos |> List.map RequireResolved
    let g = tcImports.GetTcGlobals()
    let amap = tcImports.GetImportMap()
    let tcEnv = ccuinfos |> List.fold (fun tcEnv ccuinfo -> Tc.AddCcuToTcEnv(g,amap,m,tcEnv,ccuinfo.FSharpViewOfMetadata,ccuinfo.AssemblyAutoOpenAttributes,false)) tcEnv 
    tcEnv,(dllinfos,ccuinfos)

       
       
let ProcessMetaCommandsFromInput 
     (nowarnF: 'state -> range * string -> 'state,
      dllRequireF: 'state -> range * string -> 'state,
      loadSourceF: 'state -> range * string -> unit) 
     (tcConfig:TcConfigBuilder) 
     inp 
     state0 =

    let canHaveScriptMetaCommands = 
        match inp with 
        | SigFileInput(_) ->  false
        | ImplFileInput(ImplFile(filename,isScript,_,_,_,_,_)) -> isScript

    let ProcessMetaCommand state hash  =
        let mutable matchedm = range0
        try 
            match hash with 
            | HashDirective("I",args,m) ->
               if not canHaveScriptMetaCommands then 
                   errorR(HashIncludeNotAllowedInNonScript(m));
               match args with 
               | [path] -> 
                   matchedm<-m
                   tcConfig.AddIncludePath(m,path); 
                   state
               | _ -> 
                   errorR(Error("Invalid directive. Expected '#I \"<path>\"'",m))
                   state
            | HashDirective("nowarn",[d],m) ->
               nowarnF state (m,d)
            | HashDirective(("reference" | "r"),args,m) -> 
               if not canHaveScriptMetaCommands then 
                   errorR(HashReferenceNotAllowedInNonScript(m));
               match args with 
               | [path] -> 
                   matchedm<-m
                   dllRequireF state (m,path)
               | _ -> 
                   errorR(Error("Invalid directive. Expected '#r \"<file-or-assembly>\"'",m))
                   state
            | HashDirective(("Reference" | "R"),args,m) -> 
               errorR(HashReferenceCopyAfterCompileNotAllowedInNonScript(m));
               match args with 
               | [path] -> 
                   matchedm<-m
                   dllRequireF state (m,path)
               | _ -> state
            | HashDirective("load",args,m) -> 
               if not canHaveScriptMetaCommands then 
                   errorR(HashDirectiveNotAllowedInNonScript(m));
               match args with 
               | _ :: _ -> 
                  matchedm<-m
                  args |> List.iter (fun path -> loadSourceF state (m,path))
               | _ -> 
                  errorR(Error("Invalid directive. Expected '#load \"<file>\" ... \"<file>\"'",m))
               state
            | HashDirective("time",args,m) -> 
               if not canHaveScriptMetaCommands then 
                   errorR(HashDirectiveNotAllowedInNonScript(m));
               match args with 
               | [] -> 
                   ()
               | ["on" | "off"] -> 
                   ()
               | _ -> 
                   errorR(Error("Invalid directive. Expected '#time', '#time \"on\"' or '#time \"off\"'",m))
               state
               
            | _ -> 
               
            (* warning(Error("This meta-command has been ignored",m)); *) 
               state
        with e -> errorRecovery e matchedm; state

    let rec WarnOnIgnoredSpecDecls decls = 
        decls |> List.iter (fun d -> 
            match d with 
            | Spec_hash (h,m) -> warning(Error("Directives inside modules are ignored",m)); 
            | Spec_module (_,subDecls,_) -> WarnOnIgnoredSpecDecls subDecls
            | _ -> ())

    let rec WarnOnIgnoredImplDecls decls = 
        decls |> List.iter (fun d -> 
            match d with 
            | Def_hash (h,m) -> warning(Error("Directives inside modules are ignored",m)); 
            | Def_module (_,subDecls,_,_) -> WarnOnIgnoredImplDecls subDecls
            | _ -> ())

    let ProcessMetaCommandsFromModuleSpec state (ModuleOrNamespaceSpec(_,_,decls,_,_,_,_)) =
        List.fold (fun s d -> 
            match d with 
            | Spec_hash (h,m) -> ProcessMetaCommand s h
            | Spec_module (_,subDecls,_) -> WarnOnIgnoredSpecDecls subDecls; s
            | _ -> s)
         state
         decls 

    let ProcessMetaCommandsFromModuleImpl state (ModuleOrNamespaceImpl(_,_,decls,_,_,_,_)) =
        List.fold (fun s d -> 
            match d with 
            | Def_hash (h,m) -> ProcessMetaCommand s h
            | Def_module (_,subDecls,_,_) -> WarnOnIgnoredImplDecls subDecls; s
            | _ -> s)
         state
         decls

    match inp with 
    | SigFileInput(SigFile(_,_,_,hashDirectives,specs)) -> 
        let state = List.fold ProcessMetaCommand state0 hashDirectives
        let state = List.fold ProcessMetaCommandsFromModuleSpec state specs
        state
    | ImplFileInput(ImplFile(_,_,_,_,hashDirectives,impls,_)) -> 
        let state = List.fold ProcessMetaCommand state0 hashDirectives
        let state = List.fold ProcessMetaCommandsFromModuleImpl state impls
        state

let ApplyMetaCommandsFromInputToTcConfig (tcConfig:TcConfig) (inp:input) = 
    // Clone
    let tcConfigB = tcConfig.CloneOfOriginalBuilder 
    let getWarningNumber = fun () (m,s) -> () 
    let addReferencedAssemblyByPath = fun () (m,s) -> tcConfigB.AddReferencedAssemblyByPath(m,s)
    let addLoadedSource = fun () (m,s) -> tcConfigB.AddLoadedSource(m,s);
    ProcessMetaCommandsFromInput (getWarningNumber, addReferencedAssemblyByPath, addLoadedSource) tcConfigB inp ()
    TcConfig.Create(tcConfigB,validate=false)

let GetResolvedAssemblyInformation(tcConfig : TcConfig) : AssemblyResolution list =
    let assemblyList = TcAssemblyResolutions.GetAllDllReferences(tcConfig)
    let resolutions = TcAssemblyResolutions.Resolve(tcConfig,assemblyList)
    resolutions.GetAssemblyResolutions()

//----------------------------------------------------------------------------
// Build the initial type checking environment
//--------------------------------------------------------------------------

let implicitOpen tcGlobals amap m tcEnv p =
    if verbose then dprintf "opening %s\n" p ;
    Tc.TcOpenDecl tcGlobals amap m m tcEnv (path_to_lid m (split_namespace p))

let GetInitialTypecheckerEnv (assemblyName:string option) initm (tcConfig:TcConfig) (tcImports:TcImports) tcGlobals =    
    let initm = start_range_of_range initm
    if verbose then dprintf "--- building initial tcEnv\n";         
    let internalsAreVisibleHere (ccuinfo:ImportedAssembly) =
        match assemblyName with
        | None -> false
        | Some assemblyName ->
            let isTargetAssemblyName (visibleTo:string) =             
                try                    
                    System.Reflection.AssemblyName(visibleTo).Name = assemblyName                
                with e ->
                    warning(InvalidInternalsVisibleToAssemblyName(visibleTo,ccuinfo.FSharpViewOfMetadata.FileName))
                    false
            let internalsVisibleTos = ccuinfo.AssemblyInternalsVisibleToAttributes
            List.exists isTargetAssemblyName internalsVisibleTos
    let ccus = tcImports.GetCcuInfos() |> List.map (fun ccuinfo -> ccuinfo.FSharpViewOfMetadata,
                                                                   ccuinfo.AssemblyAutoOpenAttributes,
                                                                   ccuinfo |> internalsAreVisibleHere)    
    let amap = tcImports.GetImportMap()
    let tcEnv = Tc.CreateInitialTcEnv(tcGlobals,amap,initm,ccus) |> (fun tce ->
            if tcConfig.checkOverflow then
                List.fold (implicitOpen tcGlobals amap initm) tce [lib_MFOperatorsChecked_name]
            else
                tce)
    if verbose then dprintf "--- opening implicit paths\n"; 
    if verbose then dprintf "--- GetInitialTypecheckerEnv, top modules = %s\n" (String.concat ";" (NameMap.domainL (nenv_of_tenv tcEnv).eModulesAndNamespaces)); 
    if verbose then dprintf "<-- GetInitialTypecheckerEnv\n"; 
    tcEnv

//----------------------------------------------------------------------------
// TYPECHECK
//--------------------------------------------------------------------------

(* The incremental state of type checking files *)
(* REVIEW: clean this up  *)

type topRootedSigs =  Zmap.t<QualifiedNameOfFile, ModuleOrNamespaceType>
type topRootedImpls = QualifiedNameOfFile Zset.t
type TypecheckerSigsAndImpls = TopSigsAndImpls of topRootedSigs * topRootedImpls * ModuleOrNamespaceType * ModuleOrNamespaceType

let qname_ord (q1:QualifiedNameOfFile) (q2:QualifiedNameOfFile) = compare q1.Text q2.Text

type tcState = 
    { tcsCcu: ccu;
      tcsCcuType: ModuleOrNamespace;
      tcsNiceNameGen: NiceNameGenerator;
      tcsTcSigEnv: Tc.tcEnv;
      tcsTcImplEnv: Tc.tcEnv;
      (* The accumulated results of type checking for this assembly *)
      tcsTopSigsAndImpls : TypecheckerSigsAndImpls }
    member x.NiceNameGenerator = x.tcsNiceNameGen
    member x.TcEnvFromSignatures = x.tcsTcSigEnv
    member x.TcEnvFromImpls = x.tcsTcImplEnv
    member x.Ccu = x.tcsCcu
 
    member x.NextStateAfterIncrementalFragment(tcEnvAtEndOfLastInput) = 
        { x with tcsTcSigEnv = tcEnvAtEndOfLastInput;
                 tcsTcImplEnv = tcEnvAtEndOfLastInput } 

 
let TypecheckInitialState(m,ccuName,tcConfig:TcConfig,tcGlobals,niceNameGen,tcEnv0) =
  if verbose then dprintf "Typecheck (constructing initial state)....\n";
  (* Create a ccu to hold all the results of compilation *)
  let ccuType = NewCcuContents ScopeRef_local m ccuName (empty_mtype Namespace)
  let ccu = 
      new_ccu ccuName 
        {ccu_fsharp=true;
         ccu_usesQuotations=false;
         ccu_filename=None; 
         ccu_stamp = new_stamp();
         ccu_qname= None;
         ccu_code_dir = tcConfig.implicitIncludeDir; 
         ccu_scoref=ScopeRef_local;
         ccu_contents=ccuType;
         ccu_forwarders=lazy Map.empty }

  (* OK, is this is the F# library CCU then fix it up. *)
  if tcConfig.compilingFslib then 
      tcGlobals.fslibCcu.Fixup(ccu);
      

  { tcsCcu= ccu;
    tcsCcuType=ccuType;
    tcsNiceNameGen=niceNameGen;
    tcsTcSigEnv=tcEnv0;
    tcsTcImplEnv=tcEnv0;
    tcsTopSigsAndImpls = TopSigsAndImpls (Zmap.empty qname_ord,Zset.empty qname_ord, empty_mtype Namespace, empty_mtype Namespace ) }

let CheckSimulateException(tcConfig:TcConfig) = 
    match tcConfig.simulateException with
    | Some("tc-oom") -> raise(System.OutOfMemoryException())
    | Some("tc-an") -> raise(System.ArgumentNullException("simulated"))
    | Some("tc-invop") -> raise(System.InvalidOperationException())
    | Some("tc-av") -> raise(System.AccessViolationException())
    | Some("tc-aor") -> raise(System.ArgumentOutOfRangeException())
    | Some("tc-dv0") -> raise(System.DivideByZeroException())
    | Some("tc-nfn") -> raise(System.NotFiniteNumberException())
    | Some("tc-oe") -> raise(System.OverflowException())
    | Some("tc-atmm") -> raise(System.ArrayTypeMismatchException())
    | Some("tc-bif") -> raise(System.BadImageFormatException())
    | Some("tc-knf") -> raise(System.Collections.Generic.KeyNotFoundException())
    | Some("tc-ior") -> raise(System.IndexOutOfRangeException())
    | Some("tc-ic") -> raise(System.InvalidCastException())
    | Some("tc-ip") -> raise(System.InvalidProgramException())
    | Some("tc-ma") -> raise(System.MemberAccessException())
    | Some("tc-ni") -> raise(System.NotImplementedException())
    | Some("tc-nr") -> raise(System.NullReferenceException())
    | Some("tc-oc") -> raise(System.OperationCanceledException())
    | Some("tc-fail") -> failwith "simulated"
    | _ -> ()


(* Typecheck a single file or interactive entry into F# Interactive *)
let TypecheckOneInputEventually
      checkForNoErrors 
      (tcConfig:TcConfig)
      (tcImports:TcImports) 
      tcGlobals 
      prefixPathOpt  
      (tcState:tcState)
      inp =
  eventually {
   try 
      CheckSimulateException(tcConfig)
      let (TopSigsAndImpls(topRootedSigs,topRootedImpls,allSigModulTyp,allImplementedSigModulTyp)) = tcState.tcsTopSigsAndImpls
      let m = range_of_input inp
      let amap = tcImports.GetImportMap()
      let! (topAttrs, mimpls,tcEnvAtEnd,tcSigEnv,tcImplEnv,topSigsAndImpls,ccuType) = 
        eventually {
            match inp with 
            | SigFileInput (SigFile(filename,qualNameOfFile, _,_,_) as file) ->
                (* Check if we've seen this top module signature before. *)
                if Zmap.mem qualNameOfFile topRootedSigs then 
                    errorR(Error("A signature for the file or module "^qualNameOfFile.Text^" has already been specified",start_range_of_range m));

                (* Check if the implementation came first in compilation order *)
                if Zset.mem qualNameOfFile topRootedImpls then 
                    errorR(Error("An implementation of file or module "^qualNameOfFile.Text^" has already been given. Compilation order is significant in F# because of type inference. You may need to adjust the order of your files to place the signature file before the implementation. In Visual Studio files are type-checked in the order they appear in the project file, which can be edited manually or adjusted using the solution explorer",m));

                (* Typecheck the signature file *)
                if !verboseStamps then 
                    dprintf "---------------------- START CHECK %A ------------\n" filename;
                let! (tcEnvAtEnd,tcEnv,smodulTypeTopRooted) = 
                    Tc.TypecheckOneSigFile (tcGlobals,tcState.tcsNiceNameGen,amap,tcState.tcsCcu,checkForNoErrors,tcConfig.conditionalCompilationDefines) tcState.tcsTcSigEnv file

                if !verboseStamps then 
                    dprintf "Type-checked signature:\n%s\n" (Layout.showL (Layout.squashTo 192 (EntityTypeL smodulTypeTopRooted)));
                    dprintf "---------------------- END CHECK %A ------------\n" filename;

                let topRootedSigs = Zmap.add qualNameOfFile  smodulTypeTopRooted topRootedSigs

                // Open the prefixPath for fsi.exe 
                let tcEnv = 
                    match prefixPathOpt with 
                    | None -> tcEnv 
                    | Some prefixPath -> 
                        let m = qualNameOfFile.Range
                        TcOpenDecl tcGlobals amap m m tcEnv prefixPath

                (* Build the incremental results *)
                let allSigsModulTyp = combine_mtyps [] m [smodulTypeTopRooted;allSigModulTyp]

                let ccuType = 
                    NewCcuContents ScopeRef_local m tcState.tcsCcu.AssemblyName allSigsModulTyp

                if verbose then dprintf "SigFile, nm = %s, qname = %s\n" (demangled_name_of_modul tcState.tcsCcu.Contents) qualNameOfFile.Text;
                let res = (EmptyTopAttrs, [],tcEnvAtEnd,tcEnv,tcState.tcsTcImplEnv,TopSigsAndImpls(topRootedSigs,topRootedImpls, allSigModulTyp, allImplementedSigModulTyp  ),tcState.tcsCcuType)
                return res

            | ImplFileInput (ImplFile(filename,_,qualNameOfFile,_,_,_,_) as file) ->
            
                // Check if we've got an interface for this fragment 
                let topRootedSigOpt = topRootedSigs.TryFind(qualNameOfFile)

                if verbose then dprintf "ImplFileInput, nm = %s, qualNameOfFile = %s, ?topRootedSigOpt = %b\n" filename qualNameOfFile.Text (isSome topRootedSigOpt);

                // Check if we've already seen an implementation for this fragment 
                if Zset.mem qualNameOfFile topRootedImpls then 
                  errorR(Error("An implementation of the file or module "^qualNameOfFile.Text^" has already been given",m));

                let tcImplEnv = tcState.tcsTcImplEnv

                if !verboseStamps then 
                    dprintf "---------------------- START CHECK %A ------------\n" filename;
                // Typecheck the implementation file 
                let! topAttrs,implFile,tcEnvAtEnd = 
                    Tc.TypecheckOneImplFile  (tcGlobals,tcState.tcsNiceNameGen,amap,tcState.tcsCcu,checkForNoErrors,tcConfig.conditionalCompilationDefines) tcImplEnv topRootedSigOpt file
                let hadSig = isSome topRootedSigOpt
                let implFileSigType = SigTypeOfImplFile implFile

                if !verboseStamps then 
                    dprintf "Implementation signature:\n%s\n" (Layout.showL (Layout.squashTo 192 (EntityTypeL implFileSigType)));
                    dprintf "---------------------- END CHECK %A ------------\n" filename;

                if verbose then  dprintf "done TypecheckOneImplFile...\n";
                let topRootedImpls = Zset.add qualNameOfFile topRootedImpls
        
                // Only add it to the environment if it didn't have a signature 
                let m = qualNameOfFile.Range
                let tcImplEnv = Tc.AddLocalTopRootedModuleOrNamespace tcGlobals amap m tcImplEnv implFileSigType
                let tcSigEnv = 
                    if hadSig then tcState.tcsTcSigEnv 
                    else Tc.AddLocalTopRootedModuleOrNamespace tcGlobals amap m tcState.tcsTcSigEnv implFileSigType
                
                // Open the prefixPath for fsi.exe 
                let tcImplEnv = 
                    match prefixPathOpt with 
                    | None -> tcImplEnv 
                    | Some prefixPath -> 
                        TcOpenDecl tcGlobals amap m m tcImplEnv prefixPath

                let allImplementedSigModulTyp = combine_mtyps [] m [implFileSigType; allImplementedSigModulTyp]

                // Add it to the CCU 
                let ccuType = 
                    // The signature must be reestablished. 
                    //   [CHECK: Why? This seriously degraded performance] 
                    NewCcuContents ScopeRef_local m tcState.tcsCcu.AssemblyName allImplementedSigModulTyp

                if verbose then  dprintf "done TypecheckOneInputEventually...\n";

                let topSigsAndImpls = TopSigsAndImpls(topRootedSigs,topRootedImpls,allSigModulTyp,allImplementedSigModulTyp)
                let res = (topAttrs,[implFile], tcEnvAtEnd, tcSigEnv, tcImplEnv,topSigsAndImpls,ccuType)
                return res }
     
      return (tcEnvAtEnd,topAttrs,mimpls),
             { tcState with 
                  tcsCcuType=ccuType;
                  tcsTcSigEnv=tcSigEnv;
                  tcsTcImplEnv=tcImplEnv;
                  tcsTopSigsAndImpls = topSigsAndImpls }
   with e -> 
      errorRecovery e range0; 
      return (tcState.TcEnvFromSignatures,EmptyTopAttrs,[]),tcState
 }

let TypecheckOneInput checkForNoErrors tcConfig tcImports tcGlobals prefixPathOpt  tcState inp =
    // 'use' ensures that the warning handler is restored at the end
    use unwind = InstallGlobalErrorLogger(fun oldLogger -> GetErrorLoggerFilteringByScopedPragmas(false,GetScopedPragmasForInput(inp),oldLogger))
    TypecheckOneInputEventually checkForNoErrors tcConfig tcImports tcGlobals prefixPathOpt  tcState inp |> Eventually.force

let TypecheckMultipleInputsFinish(results,tcState:tcState) =
    let tcEnvsAtEndFile,topAttrs,mimpls = List.unzip3 results
    
    let topAttrs = List.foldBack CombineTopAttrs topAttrs EmptyTopAttrs
    let mimpls = List.concat mimpls
    // This is the environment required by fsi.exe when incrementally adding definitions 
    let tcEnvAtEndOfLastFile = (match tcEnvsAtEndFile with h :: _ -> h | _ -> tcState.TcEnvFromSignatures)
    if verbose then  dprintf "done TypecheckMultipleInputs...\n";
    
    (tcEnvAtEndOfLastFile,topAttrs,mimpls),tcState

let TypecheckMultipleInputs(checkForNoErrors,tcConfig:TcConfig,tcImports,tcGlobals,prefixPathOpt,tcState,inputs) =
    let results,tcState =  List.mapfold (TypecheckOneInput checkForNoErrors tcConfig tcImports tcGlobals prefixPathOpt) tcState inputs
    TypecheckMultipleInputsFinish(results,tcState)

let TypecheckClosedInputSetFinish(mimpls,tcState) =
    // Publish the latest contents to the CCU 
    tcState.tcsCcu.Deref.ccu_contents <- tcState.tcsCcuType;

    // Check all interfaces have implementations 
    let (TopSigsAndImpls(topRootedSigs,topRootedImpls,_,_)) = tcState.tcsTopSigsAndImpls
    topRootedSigs |> Zmap.iter (fun qualNameOfFile y ->  
      if not (Zset.mem qualNameOfFile topRootedImpls) then 
        errorR(Error("The signature file "^qualNameOfFile.Text^" does not have a corresponding implementation file. If an implementation file exists then check the 'module' and 'namespace' declarations in the signature and implementation files match", qualNameOfFile.Range)));
    if verbose then  dprintf "done TypecheckClosedInputSet...\n";
    let tassembly = TAssembly(mimpls)
    tcState, tassembly    
    
let TypecheckClosedInputSet(checkForNoErrors,tcConfig,tcImports,tcGlobals,prefixPathOpt,tcState,inputs) =
    // tcEnvAtEndOfLastFile is the environment required by fsi.exe when incrementally adding definitions 
    let (tcEnvAtEndOfLastFile,topAttrs,mimpls),tcState = TypecheckMultipleInputs (checkForNoErrors,tcConfig,tcImports,tcGlobals,prefixPathOpt,tcState,inputs)
    let tcState,tassembly = TypecheckClosedInputSetFinish (mimpls, tcState)
    tcState, topAttrs, tassembly, tcEnvAtEndOfLastFile

type OptionSwitch = 
    | On
    | Off

type OptionSpec = 
    | OptionClear of bool ref
    | OptionFloat of (float -> unit)
    | OptionInt of (int -> unit)
    | OptionSwitch of (OptionSwitch -> unit)
    | OptionIntList of (int -> unit)
    | OptionIntListSwitch of (int -> OptionSwitch -> unit)
    | OptionRest of (string -> unit)
    | OptionSet of bool ref
    | OptionString of (string -> unit)
    | OptionStringList of (string -> unit)
    | OptionStringListSwitch of (string -> OptionSwitch -> unit)
    | OptionUnit of (unit -> unit)
    | OptionHelp of (CompilerOptionBlock list -> unit)                      // like OptionUnit, but given the "options"
    | OptionGeneral of (string list -> bool) * (string list -> string list) // Applies? * (ApplyReturningResidualArgs)

and  CompilerOption      = CompilerOption of string * string * OptionSpec * Option<exn> * string list
and  CompilerOptionBlock = PublicOptions  of string * CompilerOption list | PrivateOptions of CompilerOption list
let blockOptions = function PublicOptions (heading,opts) -> opts | PrivateOptions opts -> opts

let filterCompilerOptionBlock pred block =
  match block with
    | PublicOptions(heading,opts) -> PublicOptions(heading,List.filter pred opts)
    | PrivateOptions(opts)        -> PrivateOptions(List.filter pred opts)

let compilerOptionUsage (CompilerOption(s,tag,spec,_,help)) =
  let s = if s="--" then "" else s (* s="flag" for "--flag" options. s="--" for "--" option. Adjust printing here for "--" case. *)
  match spec with
    | (OptionUnit _ | OptionSet _ | OptionClear _ | OptionHelp _) -> sprintf "--%s" s 
    | OptionStringList f -> sprintf "--%s:%s" s tag
    | OptionIntList f -> sprintf "--%s:%s" s tag
    | OptionSwitch f -> sprintf "--%s[+|-]" s 
    | OptionStringListSwitch f -> sprintf "--%s[+|-]:%s" s tag
    | OptionIntListSwitch f -> sprintf "--%s[+|-]:%s" s tag
    | OptionString f -> sprintf "--%s:%s" s tag
    | OptionInt f -> sprintf "--%s:%s" s tag
    | OptionFloat f ->  sprintf "--%s:%s" s tag         
    | OptionRest f -> sprintf "--%s ..." s
    | OptionGeneral (pred,exec) -> if tag="" then sprintf "%s" s else sprintf "%s:%s" s tag (* still being decided *)

let printCompilerOption (CompilerOption(s,tag,spec,_,help) as compilerOption) =
    let flagWidth = 30 // fixed width for printing of flags, e.g. --warnaserror:<warn;...>
    let lineWidth = try System.Console.BufferWidth with e -> 80
    // Lines have this form: <flagWidth><space><description>
    //   flagWidth chars - for flags description or padding on continuation lines.
    //   single space    - space.
    //   description     - words upto but excluding the final character of the line.
    assert(flagWidth = 30)
    printf "%-30s" (compilerOptionUsage compilerOption)
    let printWord column (word:string) =
        // Have printed upto column.
        // Now print the next word including any preceeding whitespace.
        // Returns the column printed to (suited to folding).
        if column + 1 (*space*) + word.Length >= lineWidth then // NOTE: "equality" ensures final character of the line is never printed          
          printfn "" (* newline *)
          assert(flagWidth = 30)
          printf  "%-30s %s" ""(*<--flags*) word
          flagWidth + 1 + word.Length
        else
          printf  " %s" word
          column + 1 + word.Length
    let words = (String.concat " " help).Split [| ' ' |]
    let finalColumn = Array.fold printWord flagWidth words
    printfn "" (* newline *)

let printPublicOptions (heading,opts) =
  if opts<>[] then
    printfn ""
    printfn ""      
    printfn "\t\t%s" heading
    List.iter printCompilerOption opts

let printCompilerOptionBlocks blocks =
  let equals x y = x=y
  let publicBlocks = List.choose (function PrivateOptions _ -> None | PublicOptions (heading,opts) -> Some (heading,opts)) blocks
  let consider doneHeadings (heading,opts) =
    if Set.mem heading doneHeadings then
      doneHeadings
    else
      let headingOptions = List.filter (fst >> equals heading) publicBlocks |> List.map snd |> List.concat
      printPublicOptions (heading,headingOptions)
      Set.add heading doneHeadings
  List.fold consider Set.empty publicBlocks |> ignore<Set<string>>

(* For QA *)
let dumpCompilerOption prefix (CompilerOption(str,tag,spec,_,help)) =
    printf "section='%-25s' ! option=%-30s kind=" prefix str
    match spec with
      | OptionUnit             _ -> printf "OptionUnit"
      | OptionSet              _ -> printf "OptionSet"
      | OptionClear            _ -> printf "OptionClear"
      | OptionHelp             _ -> printf "OptionHelp"
      | OptionStringList       _ -> printf "OptionStringList"
      | OptionIntList          _ -> printf "OptionIntList"
      | OptionSwitch           _ -> printf "OptionSwitch"
      | OptionStringListSwitch _ -> printf "OptionStringListSwitch"
      | OptionIntListSwitch    _ -> printf "OptionIntListSwitch"
      | OptionString           _ -> printf "OptionString"
      | OptionInt              _ -> printf "OptionInt"
      | OptionFloat            _ -> printf "OptionFloat"
      | OptionRest             _ -> printf "OptionRest"
      | OptionGeneral          _ -> printf "OptionGeneral"
    printf "\n"
let dumpCompilerOptionBlock = function
  | PublicOptions (heading,opts) -> List.iter (dumpCompilerOption heading)     opts
  | PrivateOptions opts          -> List.iter (dumpCompilerOption "NoSection") opts
let dumpCompilerOptionBlocks blocks = List.iter dumpCompilerOptionBlock blocks


//----------------------------------------------------------------------------
// The argument parser is used by both the VS plug-in and the fsc.exe to
// parse the include file path and other front-end arguments.
//
// The language service uses this function too. It's important to continue
// processing flags even if an error is seen in one so that the best possible
// intellisense can be show.
//--------------------------------------------------------------------------
let ParseCompilerOptions (collectOtherArgument : string -> unit) (blocks: CompilerOptionBlock list) args =
  let specs : CompilerOption list = List.collect blockOptions blocks
          
  // returns a tuple - the option token, the option argument string
  let parseOption (s : string) = 
    // grab the option token
    let opts = s.Split([|':'|])
    let mutable opt = opts.[0]
    // if it doesn't start with a '-' or '/', reject outright
    if opt.[0] <> '-' && opt.[0] <> '/' then
      opt <- ""
    elif opt <> "--" then
      // is it an abbreviated or MSFT-style option?
      // if so, strip the first character and move on with your life
      if opt.Length = 2 || opt.[0] = '/' then
        opt <- opt.[1 ..]
      // else, it should be a non-abbreviated option starting with "--"
      elif opt.Length > 3 && opt.StartsWith("--") then
        opt <- opt.[2 ..]
      else
        opt <- ""
(*
    // is it two characters?  If so, strip '-' or '/' from the start
    if opt <> "--" && opt.Length > 1 && (opt.[0] = '-' || opt.[0] = '/') then
      opt <- opt.[1 ..]
      // is it more than two characters?  If so, strip "--" or '/' from the start
      if opt.Length > 2 && opt.[0] = '-' then // abbreviated options should have only 1 '-'
        opt <- opt.[1 ..]
*)
    // get the argument string  
    let optArgs = if opts.Length > 1 then String.Join(":",opts.[1 ..]) else ""
    opt, optArgs
              
  let getOptionArg compilerOption (argString : string) =
    if argString = "" then
      let es = sprintf "option requires parameter: %s" (compilerOptionUsage compilerOption)
      errorR(Error(es,rangeCmdArgs)) ;
    argString
    
  let getOptionArgList compilerOption (argString : string) =
    if argString = "" then
      let es = sprintf "option requires parameter: %s" (compilerOptionUsage compilerOption)
      errorR(Error(es,rangeCmdArgs)) ;
      []
    else
      argString.Split([|',';';'|]) |> List.of_array
  
  let getSwitchOpt (opt : string) =
    // if opt is a switch, strip the  '+' or '-'
    if opt <> "--" && opt.Length > 1 && (opt.EndsWith("+",StringComparison.Ordinal) || opt.EndsWith("-",StringComparison.Ordinal)) then
      opt.[0 .. opt.Length - 2]
    else
      opt
      
  let getSwitch (s: string) = 
    let s = (s.Split([|':'|])).[0]
    if s <> "--" && s.EndsWith("-",StringComparison.Ordinal) then Off else On

  let rec process_arg args =    
    match args with 
    | [] -> ()
    | opt :: t ->  

        let optToken, argString = parseOption opt

        let report_deprecated_option deprecated =
          match deprecated with
          | Some(e) -> warning(e)
          | None -> ()

        let rec attempt l = 
          match l with 
          | (CompilerOption(s, _, OptionHelp f, d, _) :: _) when optToken = s  && argString = "" -> 
              report_deprecated_option d
              f blocks; t
          | (CompilerOption(s, _, OptionUnit f, d, _) :: _) when optToken = s  && argString = "" -> 
              report_deprecated_option d
              f (); t
          | (CompilerOption(s, _, OptionSwitch f, d, _) :: _) when getSwitchOpt(optToken) = s && argString = "" -> 
              report_deprecated_option d
              f (getSwitch opt); t
          | (CompilerOption(s, _, OptionSet f, d, _) :: _) when optToken = s && argString = "" -> 
              report_deprecated_option d
              f := true; t
          | (CompilerOption(s, _, OptionClear f, d, _) :: _) when optToken = s && argString = "" -> 
              report_deprecated_option d
              f := false; t
          | (CompilerOption(s, _, OptionString f, d, _) as compilerOption :: _) when optToken = s -> 
              report_deprecated_option d
              let oa = getOptionArg compilerOption argString
              if oa <> "" then
                  f (getOptionArg compilerOption oa)
              t 
          | (CompilerOption(s, _, OptionInt f, d, _) as compilerOption :: _) when optToken = s ->
              report_deprecated_option d
              let oa = getOptionArg compilerOption argString
              if oa <> "" then 
                  f (try int32 (oa) with _ -> 
                      errorR(Error("'"^(getOptionArg compilerOption argString)^"' is not a valid integer argument",rangeCmdArgs)); 0)
              t
          | (CompilerOption(s, _, OptionFloat f, d, _) as compilerOption :: _) when optToken = s -> 
              report_deprecated_option d
              let oa = getOptionArg compilerOption argString
              if oa <> "" then
                  f (try float (oa) with _ -> 
                      errorR(Error(("'"^getOptionArg compilerOption argString)^"' is not a valid floating point argument", rangeCmdArgs)); 0.0)
              t
          | (CompilerOption(s, _, OptionRest f, d, _) :: _) when optToken = s -> 
              report_deprecated_option d
              List.iter f t; []
          | (CompilerOption(s, _, OptionIntList f, d, _) as compilerOption :: _) when optToken = s ->
              report_deprecated_option d
              let al = getOptionArgList compilerOption argString
              if al <> [] then
                  List.iter (fun i -> f (try int32 i with _ -> errorR(Error(("'"^i^"' is not a valid integer argument"),rangeCmdArgs)); 0)) al ;
              t
          | (CompilerOption(s, _, OptionIntListSwitch f, d, _) as compilerOption :: _) when getSwitchOpt(optToken) = s -> 
              report_deprecated_option d
              let al = getOptionArgList compilerOption argString
              if al <> [] then
                  let switch = getSwitch(opt)
                  List.iter (fun i -> f (try int32 i with _ -> errorR(Error(("'"^i^"' is not a valid integer argument"),rangeCmdArgs)); 0) switch) al ; 
              t
              // here
          | (CompilerOption(s, _, OptionStringList f, d, _) as compilerOption :: _) when optToken = s -> 
              report_deprecated_option d
              let al = getOptionArgList compilerOption argString
              if al <> [] then
                  List.iter (fun s -> f s) (getOptionArgList compilerOption argString)
              t
          | (CompilerOption(s, _, OptionStringListSwitch f, d, _) as compilerOption :: _) when getSwitchOpt(optToken) = s -> 
              report_deprecated_option d
              let al = getOptionArgList compilerOption argString
              if al <> [] then
                  let switch = getSwitch(opt)
                  List.iter (fun s -> f s switch) (getOptionArgList compilerOption argString)
              t
          | (CompilerOption(s, _, OptionGeneral (pred,exec), d, _) :: more) when pred args -> 
              report_deprecated_option d
              let rest = exec args in rest // arguments taken, rest remaining
          | (_ :: more) -> attempt more 
          | [] -> 
              if (opt.[0] = '-' || opt.[0] = '/') then 
                  // want the whole opt token - delimeter and all
                  let unrecOpt = sprintf "'%s'" (opt.Split([|':'|]).[0])
                  errorR(Error("unrecognized option: "^ unrecOpt,rangeCmdArgs)) ;
                  t
              else 
                 (collectOtherArgument opt; t)
        let rest = attempt specs 
        process_arg rest
  
  let result = process_arg args
  result
