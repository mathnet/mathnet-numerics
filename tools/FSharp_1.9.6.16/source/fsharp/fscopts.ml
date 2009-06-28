// (c) Microsoft Corporation. All rights reserved

#light

module (* internal *) Microsoft.FSharp.Compiler.Fscopts

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
open Microsoft.FSharp.Compiler 
open System

module Ilsupp = Microsoft.FSharp.Compiler.AbstractIL.Internal.Support 
module Unilex = Microsoft.FSharp.Compiler.UnicodeLexing 

module Attributes = 
    open System.Runtime.CompilerServices

    //[<assembly: System.Security.SecurityTransparent>]
    [<Dependency("FSharp.Core",LoadHint.Always)>] 
    do()

open Microsoft.FSharp.Compiler.Build
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.TypeChecker
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops 
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Lexhelp
open Microsoft.FSharp.Compiler.Ilxgen
open Ilxerase


let lexFilterVerbose = false
let mutable enableConsoleColoring = true // global state

let setFlag r n = 
    match n with 
    | 0 -> r false
    | 1 -> r true
    | _ -> raise (Failure "expected 0/1")

let SetOptimizeOff(tcConfigB : TcConfigBuilder) = 
    tcConfigB.optSettings <- { tcConfigB.optSettings with jitOptUser = Some false }
    tcConfigB.optSettings <- { tcConfigB.optSettings with localOptUser = Some false }
    tcConfigB.optSettings <- { tcConfigB.optSettings with crossModuleOptUser = Some false }
    tcConfigB.optSettings <- { tcConfigB.optSettings with lambdaInlineThreshold = 0 }
    tcConfigB.ignoreSymbolStoreSequencePoints <- false;
    tcConfigB.doDetuple <- false; 
    tcConfigB.doTLR <- false;

let SetOptimizeOn(tcConfigB : TcConfigBuilder) =    
    tcConfigB.optSettings <- { tcConfigB.optSettings with jitOptUser = Some true }
    tcConfigB.optSettings <- { tcConfigB.optSettings with localOptUser = Some true }
    tcConfigB.optSettings <- { tcConfigB.optSettings with crossModuleOptUser = Some true }
    tcConfigB.optSettings <- { tcConfigB.optSettings with lambdaInlineThreshold = 6 }

    tcConfigB.ignoreSymbolStoreSequencePoints <- true;
    tcConfigB.doDetuple <- true;  
    tcConfigB.doTLR <- true;

let SetOptimizeSwitch (tcConfigB : TcConfigBuilder) switch = 
    if (switch = On) then SetOptimizeOn(tcConfigB) else SetOptimizeOff(tcConfigB)
        
let SetTailcallSwitch switch =
    if (switch = On) then
        SetTailCalls()
    else
        SetNoTailCalls()
        
let jitoptimize_switch (tcConfigB : TcConfigBuilder) switch =
    tcConfigB.optSettings <- { tcConfigB.optSettings with jitOptUser = Some (switch = On) }
    
let localoptimize_switch (tcConfigB : TcConfigBuilder) switch =
    tcConfigB.optSettings <- { tcConfigB.optSettings with localOptUser = Some (switch = On) }
    
let crossOptimizeSwitch (tcConfigB : TcConfigBuilder) switch =
    tcConfigB.optSettings <- { tcConfigB.optSettings with crossModuleOptUser = Some (switch = On) }

let splitting_switch (tcConfigB : TcConfigBuilder) switch =
    tcConfigB.optSettings <- { tcConfigB.optSettings with abstractBigTargets = switch = On }

let (++) x s = x @ [s]

let SetTarget (tcConfigB : TcConfigBuilder)(s : string) =
    match s.ToLowerInvariant() with
    | "exe"     ->  tcConfigB.target <- ConsoleExe
    | "winexe"  ->  tcConfigB.target <- WinExe
    | "library" ->  tcConfigB.target <- Dll
    | "module"  ->  tcConfigB.target <- Module
    | _         ->  error(Error("unrecognized target '"^s^"', expected 'exe', 'winexe', 'library' or 'module'",rangeCmdArgs))

let SetDebugSwitch (tcConfigB : TcConfigBuilder) (dtype : string option) (s : OptionSwitch) =
    match dtype with
    | Some(s) ->
       match s with 
       | "pdbonly" -> tcConfigB.jitTracking <- false
       | "full" -> tcConfigB.jitTracking <- true 
       | _ -> error(Error("unrecognized debug type '"^s^"', expected 'pdbonly' or 'full'", rangeCmdArgs))
    | None -> tcConfigB.jitTracking <- s = On 
    tcConfigB.debuginfo <- s = On ;

let setOutFileName tcConfigB s = 
    tcConfigB.outputFile <- Some s

let setSignatureFile tcConfigB s = 
    tcConfigB.printSignature <- true ; 
    tcConfigB.printSignatureFile <- s

// option tags
let tagString = "<string>"
let tagExe = "exe"
let tagWinExe = "winexe"
let tagLibrary = "library"
let tagModule = "module"
let tagFile = "<file>"
let tagFileList = "<file;...>"
let tagDirList = "<dir;...>"
let tagPathList = "<path;...>"
let tagResInfo = "<resinfo>"
let tagFullPDBOnly = "{full|pdbonly}"
let tagWarnList = "<warn;...>"
let tagSymbolList = "<symbol;...>"
let tagAddress = "<address>"
let tagN = "<n>"
let tagNone = ""


// PrintOptionInfo
//----------------

/// Print internal "option state" information for diagnostics and regression tests.  
let PrintOptionInfo (tcConfigB:TcConfigBuilder) =
    printfn "  jitOptUser . . . . . . : %+A" tcConfigB.optSettings.jitOptUser
    printfn "  localOptUser . . . . . : %+A" tcConfigB.optSettings.localOptUser
    printfn "  crossModuleOptUser . . : %+A" tcConfigB.optSettings.crossModuleOptUser
    printfn "  lambdaInlineThreshold  : %+A" tcConfigB.optSettings.lambdaInlineThreshold
    printfn "  ignoreSymStoreSeqPts . : %+A" tcConfigB.ignoreSymbolStoreSequencePoints
    printfn "  doDetuple  . . . . . . : %+A" tcConfigB.doDetuple
    printfn "  doTLR  . . . . . . . . : %+A" tcConfigB.doTLR
    printfn "  jitTracking  . . . . . : %+A" tcConfigB.jitTracking
    printfn "  debuginfo  . . . . . . : %+A" tcConfigB.debuginfo
    printfn "  resolutionEnvironment  : %+A" tcConfigB.resolutionEnvironment
    printfn "  product  . . . . . . . : %+A" tcConfigB.product
    printfn "  useFsiAuxLib . . . . . : %+A" tcConfigB.useFsiAuxLib
    tcConfigB.includes |> List.sort
                       |> List.iter (printfn "  include  . . . . . . . : %A")
  

// OptionBlock: Input files
//-------------------------

let inputFileFlagsBoth (tcConfigB : TcConfigBuilder) =
    [   CompilerOption("reference", tagFile, OptionString (fun s -> tcConfigB.AddReferencedAssemblyByPath (rangeStartup,s)), None,
                           [ "Reference an assembly (Short form: -r)" ]);
    ]

let referenceFlagAbbrev (tcConfigB : TcConfigBuilder) = 
        CompilerOption("r", tagFile, OptionString (fun s -> tcConfigB.AddReferencedAssemblyByPath (rangeStartup,s)), None,
                           [ "Short form of --reference" ])
      
let inputFileFlagsFsi tcConfigB = inputFileFlagsBoth tcConfigB
let inputFileFlagsFsc tcConfigB = inputFileFlagsBoth tcConfigB


// OptionBlock: Errors and warnings
//---------------------------------

let errorsAndWarningsFlags (tcConfigB : TcConfigBuilder) = 
    [
        CompilerOption("warnaserror", tagNone, OptionSwitch(fun switch   -> tcConfigB.globalWarnAsError <- switch <> Off), None,
                           [ "Report all warnings as errors" ]); 

        CompilerOption("warnaserror", tagWarnList, OptionIntListSwitch (fun n switch -> 
                                                                tcConfigB.specificWarnAsError <- 
                                                                    if switch = Off then 
                                                                        ListSet.remove (=) n tcConfigB.specificWarnAsError
                                                                    else 
                                                                        ListSet.insert (=) n tcConfigB.specificWarnAsError), None,
                           [ "Report specific warnings as errors" ]);
           
        CompilerOption("warn", tagN, OptionInt (fun n -> 
                                                     tcConfigB.globalWarnLevel <- 
                                                     if (n >= 0 && n <= 4) then n 
                                                     else error(Error("Invalid warning level '" ^ (string n) ^ "'",rangeCmdArgs))), None,
                           [ "Set a warning level (0-4)" ]);
           
        CompilerOption("nowarn", tagWarnList, OptionStringList (fun n -> tcConfigB.TurnWarningOff(rangeCmdArgs,n)), None,
                           [ "Disable specific warning messages" ]); 
    ]


// OptionBlock: Output files
//--------------------------
          
let outputFileFlagsFsi (tcConfigB : TcConfigBuilder) = []
let outputFileFlagsFsc (tcConfigB : TcConfigBuilder) =
    [
        CompilerOption("out", tagFile, OptionString (setOutFileName tcConfigB), None,
                           [ "Name of the output file (Short form: -o)"]); 

        CompilerOption("target",  tagExe, OptionString (SetTarget tcConfigB), None,
                           [ "Build a console executable"]);
                           
        CompilerOption("target", tagWinExe, OptionString (SetTarget tcConfigB), None,
                           [ "Build a Windows executable"]);

        CompilerOption("target", tagLibrary, OptionString (SetTarget tcConfigB), None,
                           [ "Build a library (Short form: -a)"]);

        CompilerOption("target", tagModule, OptionString (SetTarget tcConfigB), None,
                           [ "Build a module that can be added to another assembly" ]);

        CompilerOption("delaysign", tagNone, OptionSwitch (fun s -> tcConfigB.delaysign <- (s = On)), None,
                           [ "Delay-sign the assembly using only the public"
                             "portion of the strong name key" ]);

        CompilerOption("doc", tagFile, OptionString (fun s -> tcConfigB.xmlDocOutputFile <- Some s), None,
                           [ "Write the xmldoc of the assembly to the given"
                             "file" ]);

        CompilerOption("keyfile", tagFile, OptionString (fun s -> tcConfigB.signer <- Some(s)),  None,
                           [ "Specify a strong name key file" ]);
        CompilerOption("keycontainer", tagString, OptionString(fun s -> tcConfigB.container <- Some(s)),None,
                           [ "Specify a strong name key container" ]);

        CompilerOption("platform", tagString, OptionString (fun s -> tcConfigB.platform <- match s with | "x86" -> Some X86 | "x64" -> Some AMD64 | "Itanium" -> Some IA64 | "anycpu" -> None | _ -> error(Error("unrecognized platform '"^s^"'",rangeCmdArgs))), None,
                           [ "Limit which platforms this code can run on:"
                             "x86, Itanium, x64 or anycpu. The default is"
                             "anycpu"]) ;

        CompilerOption("nooptimizationdata", tagNone, OptionUnit (fun () -> tcConfigB.onlyEssentialOptimizationData <- true), None,
                           [ "Only include optimization information essential"
                             "for implementing inlined constructs. Inhibits"
                             "cross-module inlining but improves binary"
                             "compatibility"]);

        CompilerOption("nointerfacedata", tagNone, OptionUnit (fun () -> tcConfigB.noSignatureData <- true), None,
                           [ "Don't add a resource to the generated assembly"
                             "containing F#-specific metadata"]);

        CompilerOption("sig", tagFile, OptionString (setSignatureFile tcConfigB), None,
                           [ "Print the inferred interface of the assembly"
                             "to a file"]);    
    ]


// OptionBlock: Resources
//-----------------------

let resourcesFlagsFsi (tcConfigB : TcConfigBuilder) = []
let resourcesFlagsFsc (tcConfigB : TcConfigBuilder) =
    [
        CompilerOption("win32res", tagFile, OptionString (fun s -> tcConfigB.win32res <- s), None,
                           [ "Specify a Win32 resource file (.res)" ]);
        
        CompilerOption("win32manifest", tagFile, OptionString (fun s -> tcConfigB.win32manifest <- s), None,
                           [ "Specify a Win32 manifest file" ]);
        
        CompilerOption("nowin32manifest", tagNone, OptionUnit (fun () -> tcConfigB.includewin32manifest <- false), None,
                           ["Do not include the default Win32 manifest"]);

        CompilerOption("resource", tagResInfo, OptionString (fun s -> tcConfigB.AddEmbeddedResource s), None,
                           [ "Embed the specified managed resource" ]);

        CompilerOption("linkresource", tagResInfo, OptionString (fun s -> tcConfigB.linkResources <- tcConfigB.linkResources ++ s), None,
                           [ "Link the specified resource to this assembly"
                             "where the resinfo format is"
                             "    <file>[,<string name>[,public|private]]"]);
    ]


// OptionBlock: Code generation
//-----------------------------
      
let codeGenerationFlags (tcConfigB : TcConfigBuilder) =
    [
        CompilerOption("debug", tagNone, OptionSwitch (SetDebugSwitch tcConfigB None), None,
                           [ "Emit debug information (Short form: -g)" ]);
        
        CompilerOption("debug", tagFullPDBOnly, OptionString (fun s -> SetDebugSwitch tcConfigB (Some(s)) On), None,
                           [ "Specify debugging type: full, pdbonly."
                             "('full' is the default and enables attaching a"
                             "debugger to a running program)" ]);

        CompilerOption("optimize", tagNone, OptionSwitch (SetOptimizeSwitch tcConfigB) , None,
                           [ "Enable optimizations (Short form: -O)" ]);

        CompilerOption("tailcalls", tagNone, OptionSwitch SetTailcallSwitch, None,
                           [ "Enable or disable tailcalls"]);
                           
        CompilerOption("crossoptimize", tagNone, OptionSwitch (crossOptimizeSwitch tcConfigB), None,
                           [ "Enable or disable cross-module optimizations"]);
        
    ]
 

// OptionBlock: Language
//----------------------

let defineSymbol tcConfigB s = tcConfigB.conditionalCompilationDefines <- s :: tcConfigB.conditionalCompilationDefines
      
let mlCompatibilityFlag (tcConfigB : TcConfigBuilder) = 
        CompilerOption("mlcompatibility", tagNone, OptionUnit   (fun () -> tcConfigB.mlCompatibility<-true; tcConfigB.TurnWarningOff(rangeCmdArgs,"62")),  None,
                           [ "Ignore OCaml-compatibility warnings." ])
let languageFlags tcConfigB =
    [
        CompilerOption("checked", tagNone, OptionSwitch (fun switch -> tcConfigB.Build.checkOverflow <- (switch = On)),  None,
                           [ "Generate overflow checks" ]);
        CompilerOption("define", tagString, OptionString (defineSymbol tcConfigB),  None,
                           [ "Define conditional compilation symbols (Short"
                             "form: -d)" ]);
        mlCompatibilityFlag tcConfigB
    ]
    

// OptionBlock: HTML doc generation
//---------------------------------

let htmlFlagsFsc tcConfigB = (* FSC only *)
    [
        CompilerOption("generatehtml", tagNone, OptionUnit (fun () ->  tcConfigB.generateHtmlDocs <- true), None,
                           [ "Generate HTML documentation" ]);

        CompilerOption("htmloutputdir", tagFile, OptionString (fun s ->  tcConfigB.htmlDocDirectory <- Some s) , None,
                           [ "Output directory for HTML documentation" ]);

        CompilerOption("htmlcss", tagString, OptionString (fun s ->  tcConfigB.htmlDocCssFile <- Some s), None,
                           [ "Set the name of the Cascading Style Sheet" ]);

        CompilerOption("htmlnamespacefile", tagString, OptionString (fun s ->  tcConfigB.htmlDocNamespaceFile <- Some s), None,
                           [ "Set the name of the master namespaces.html"
                             "file assumed to be in the output directory" ]);

        CompilerOption("htmlnamespacefileappend", tagNone, OptionUnit (fun () -> tcConfigB.htmlDocAppendFlag <- true), None,
                           [ "Append to the master namespace file when"
                             "generating HTML documentation" ]);
    ]


// OptionBlock: Advanced user options
//-----------------------------------

let libFlag (tcConfigB : TcConfigBuilder) = 
        CompilerOption("lib", tagDirList, OptionStringList (fun s -> tcConfigB.AddIncludePath (rangeStartup,s)), None,
                           [ "Specify a directory for the include path which"
                             "is used to resolve source files and assemblies"
                             "(Short form: -I)" ])

let libFlagAbbrev (tcConfigB : TcConfigBuilder) = 
        CompilerOption("I", tagDirList, OptionStringList (fun s -> tcConfigB.AddIncludePath (rangeStartup,s)), None,
                           [ "Short form of --lib" ])
      
let codePageFlag (tcConfigB : TcConfigBuilder) = 
        CompilerOption("codepage", tagN, OptionInt (fun n -> 
                     let encoding = 
                         try System.Text.Encoding.GetEncoding(n)
                         with :? System.ArgumentException as err -> error(Error(err.Message,rangeCmdArgs))
                     tcConfigB.inputCodePage <- Some(n)), None,
                           [ "Specify the codepage used to read source files" ])

let utf8OutputFlag (tcConfigB: TcConfigBuilder) = 
        CompilerOption("utf8output", tagNone, OptionUnit (fun () -> tcConfigB.utf8output <- true), None,
                           [ "Output messages in UTF-8 encoding" ])

let fullPathsFlag  (tcConfigB : TcConfigBuilder)  = 
        CompilerOption("fullpaths", tagNone, OptionUnit (fun () -> tcConfigB.showFullPaths <- true), None,
                           [ "Output messages with fully qualified paths" ])

let cliRootFlag (tcConfigB : TcConfigBuilder) = 
        CompilerOption("cliroot", tagString, OptionString (fun s  -> ()), Some(DeprecatedCommandLineOption("--cliroot", "Use an explicit reference to a specific copy of mscorlib.dll instead", rangeCmdArgs)),
                           [ "Use to override where the compiler looks for"
                             "mscorlib.dll and framework components" ])
          
let advancedFlagsBoth tcConfigB =
    [
        codePageFlag tcConfigB;
        utf8OutputFlag tcConfigB;
        fullPathsFlag tcConfigB;
        libFlag tcConfigB;
    ]

let advancedFlagsFsi tcConfigB = advancedFlagsBoth tcConfigB      
let advancedFlagsFsc tcConfigB =
    advancedFlagsBoth tcConfigB @
    [
        CompilerOption("baseaddress", tagAddress, OptionString (fun s -> tcConfigB.baseAddress <- Some(int32 s)), None,
                           [ "Base address for the library to be built" ]);
        CompilerOption("noframework", tagNone, OptionUnit (fun () -> 
                                               tcConfigB.framework <- false; 
                                               tcConfigB.implicitlyResolveAssemblies <- false), None,
                           [ "Do not reference the .NET Framework assemblies"
                             "by default" ]);

        CompilerOption("standalone", tagNone, OptionUnit (fun s -> 
                                             tcConfigB.openDebugInformationForLaterStaticLinking <- true; 
                                             tcConfigB.standalone <- true;
                                             tcConfigB.implicitlyResolveAssemblies <- true), None,
                           [ "Statically link the F# library and all"
                             "referenced DLLs that depend on it into the"
                             "assembly being generated." ]);

        CompilerOption("staticlink", tagFile, OptionString (fun s -> tcConfigB.extraStaticLinkRoots <- tcConfigB.extraStaticLinkRoots @ [s]), None,
                           [ "Statically link the given assembly and all"
                             "referenced DLLs that depend on this assembly."
                             "Use an assembly name e.g. mylib, not a DLL name" ]);

        CompilerOption("pdb", tagString, OptionString (fun s -> tcConfigB.debugSymbolFile <- Some s), None,
                           [ "Name the output debug file" ]);
    ]

// OptionBlock: Internal options (internal use only)
//--------------------------------------------------

let testFlag tcConfigB = 
        CompilerOption("test", tagString, OptionString (fun s -> 
                                            match s with
                                            | "ErrorRanges"      -> tcConfigB.errorStyle <- ErrorStyle.TestErrors
                                            | "MemberBodyRanges" -> PostTypecheckSemanticChecks.testFlagMemberBody := true
                                            | "Tracking"         -> Lib.tracking := true (* general purpose on/off diagnostics flag *)
                                            | "NoNeedToTailcall" -> tcConfigB.optSettings <- { tcConfigB.optSettings with reportNoNeedToTailcall = true }
                                            | "FunctionSizes"    -> tcConfigB.optSettings <- { tcConfigB.optSettings with reportFunctionSizes = true }
                                            | "TotalSizes"       -> tcConfigB.optSettings <- { tcConfigB.optSettings with reportTotalSizes = true }
                                            | "HasEffect"        -> tcConfigB.optSettings <- { tcConfigB.optSettings with reportHasEffect = true }
                                            | str                -> warning(Error("Unknown --test argument: " ^ str,rangeCmdArgs))), None,
                           [ ])

let useIncrementalBuildFlag (tcConfigB : TcConfigBuilder) = 
        CompilerOption("use-incremental-build", tagNone, OptionUnit (fun () -> tcConfigB.useIncrementalBuilder <- true), None,
                           [ ])

let vsStyleErrorsFlag tcConfigB = 
        CompilerOption("vserrors", tagNone, OptionUnit (fun () -> tcConfigB.errorStyle <- ErrorStyle.VSErrors), None,
                           [ ])
          
let internalFlags (tcConfigB:TcConfigBuilder) =
  [
    CompilerOption("stamps", tagNone, OptionSet Tast.verboseStamps, None, []); 
    CompilerOption("ranges", tagNone, OptionSet Tastops.DebugPrint.layout_ranges, None, []);   
    CompilerOption("terms" , tagNone, OptionUnit (fun () -> tcConfigB.showTerms <- true), None, []);
    CompilerOption("termsfile" , tagNone, OptionUnit (fun () -> tcConfigB.writeTermsToFiles <- true), None, []);
#if DEBUG
    CompilerOption("ilfiles", tagNone, OptionUnit (fun () -> tcConfigB.writeGeneratedILFiles <- true), None, []);
#endif
    CompilerOption("pause", tagNone, OptionUnit (fun () -> tcConfigB.pause <- true), None, []);
    CompilerOption("detuple", tagNone, OptionInt (setFlag (fun v -> tcConfigB.doDetuple <- v)), None, []);
    CompilerOption("simulateException", tagNone, OptionString (fun s -> tcConfigB.simulateException <- Some(s)), None, [ "Simulate an exception from some part of the compiler" ]);    
    CompilerOption("tlr", tagN, OptionInt (setFlag (fun v -> tcConfigB.doTLR <- v)), None, []);
    CompilerOption("tlrlift", tagNone, OptionInt (setFlag  (fun v -> Tlr.liftTLR := v)), None, []);
    CompilerOption("parseonly", tagNone, OptionUnit (fun () -> tcConfigB.parseOnly <- true), None, []);
    CompilerOption("typecheckonly", tagNone, OptionUnit (fun () -> tcConfigB.typeCheckOnly <- true), None, []);
    CompilerOption("ast", tagNone, OptionUnit (fun () -> tcConfigB.printAst <- true), None, []);
    CompilerOption("tokenize", tagNone, OptionUnit (fun () -> tcConfigB.tokenizeOnly <- true), None, []);
    CompilerOption("testInteractionParser", tagNone, OptionUnit (fun () -> tcConfigB.testInteractionParser <- true), None, []);
    CompilerOption("testparsererrorrecovery", tagNone, OptionUnit (fun () -> tcConfigB.reportNumDecls <- true), None, []);
    CompilerOption("inlinethreshold", tagN, OptionInt (fun n -> tcConfigB.optSettings <- { tcConfigB.optSettings with lambdaInlineThreshold = n }), None, []);
    CompilerOption("extraoptimizationloops", tagNone, OptionInt (fun n -> tcConfigB.extraOptimizationIterations <- n), None, []);
    CompilerOption("maxerrors", tagN, OptionInt (fun n -> tcConfigB.maxErrors <- n), None, []);
    CompilerOption("abortonerror", tagNone, OptionUnit (fun () -> tcConfigB.abortOnError <- true), None, []);
    CompilerOption("htmllocallinks", tagNone, OptionUnit (fun () -> tcConfigB.htmlDocLocalLinks <- true), None, []);
    CompilerOption("publicasinternal", tagNone, OptionSet Ilxgen.generatePublicAsInternal, None, []);    
    CompilerOption("implicitresolution", tagNone, OptionUnit (fun s -> tcConfigB.implicitlyResolveAssemblies <- true), None, []);

    CompilerOption("resolutions", tagNone, OptionUnit (fun () -> tcConfigB.showReferenceResolutions <- true), None,
                          [ "Display assembly reference resolution information" ]) ;
    CompilerOption("resolutionframeworkregistrybase", tagString, OptionString (fun s -> tcConfigB.resolutionFrameworkRegistryBase<-s), None,
                          [ "The base registry key to use for assembly resolution. This part in brackets here: HKEY_LOCAL_MACHINE\[SOFTWARE\Microsoft\.NETFramework]\v2.0.50727\AssemblyFoldersEx" ]);
    CompilerOption("resolutionassemblyfoldersuffix", tagString, OptionString (fun s -> tcConfigB.resolutionAssemblyFoldersSuffix<-s), None,
                          [ "The base registry key to use for assembly resolution. This part in brackets here: HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\v2.0.50727\[AssemblyFoldersEx]" ]);
    CompilerOption("resolutionassemblyfoldersconditions", tagString, OptionString (fun s -> tcConfigB.resolutionAssemblyFoldersConditions <- ","^s), None,
                          [ "Additional reference resolution conditions. For example \"OSVersion=5.1.2600.0,PlatformID=id" ]);
    CompilerOption("simpleresolution", tagNone, OptionUnit (fun () -> tcConfigB.useMonoResolution<-true), None,
                          [ "Resolve assembly references using directory-based mono rules rather than MSBuild resolution (Default=false except when running fsc.exe under mono)" ]);
    CompilerOption("msbuildresolution", tagNone, OptionUnit (fun () -> tcConfigB.useMonoResolution<-false), None,
                          [ "Resolve assembly references using MSBuild resolution rules rather than directory based (Default=true except when running fsc.exe under mono)" ]);
    testFlag tcConfigB ;
    useIncrementalBuildFlag tcConfigB;
    vsStyleErrorsFlag tcConfigB;
    CompilerOption("flaterrors", tagNone, OptionUnit (fun () -> tcConfigB.flatErrors <- true), None, []);
    CompilerOption("jit", tagNone, OptionSwitch (jitoptimize_switch tcConfigB), None, []);
    CompilerOption("localoptimize", tagNone, OptionSwitch(localoptimize_switch tcConfigB),None, []);
    CompilerOption("splitting", tagNone, OptionSwitch(splitting_switch tcConfigB),None, []);
    CompilerOption("versionfile", tagString, OptionString (fun s -> tcConfigB.version <- VersionFile s), None, []);
    CompilerOption("times" , tagNone, OptionUnit  (fun () -> tcConfigB.showTimes <- true), None,
                          [ "Display timing profiles for compilation" ]);
    (* BEGIN: Consider as public Retail option? *)
    // Some System.Console do not have operational colors, make this available in Retail?
    CompilerOption("consolecolors", tagNone, OptionSwitch (fun switch -> enableConsoleColoring <- switch=On), None,
                           [ "Output messages with Console colors" ])
  ]

  
// OptionBlock: Deprecated flags (fsc, service only)
//--------------------------------------------------
    
let compilingFsLibFlag (tcConfigB : TcConfigBuilder) = 
        CompilerOption("compiling-fslib", tagNone, OptionUnit (fun () -> tcConfigB.compilingFslib <- true; tcConfigB.TurnWarningOff(rangeStartup,"42"); Msilxlib.compiling_msilxlib_ref := true), (* Not deprecated, just undocumented *) None, [])
let mlKeywordsFlag = 
        CompilerOption("ml-keywords", tagNone, OptionUnit (fun () -> Lexhelp.Keywords.permitFsharpKeywords := false), Some(DeprecatedCommandLineOption("--ml-keywords", "", rangeCmdArgs)), [])

let gnuStyleErrorsFlag tcConfigB = 
        CompilerOption("gnu-style-errors", tagNone, OptionUnit (fun () -> tcConfigB.errorStyle <- ErrorStyle.EmacsErrors), Some(DeprecatedCommandLineOption("--gnu-style-errors", "", rangeCmdArgs)), [])

let deprecatedFlagsBoth tcConfigB =
    [ 
      CompilerOption("light", tagNone, OptionUnit (fun () -> tcConfigB.light <- Some(true)), Some(DeprecatedCommandLineOption("--light", "", rangeCmdArgs)), []);
      CompilerOption("indentation-syntax", tagNone, OptionUnit (fun () -> tcConfigB.light <- Some(true)), Some(DeprecatedCommandLineOption("--indentation-syntax", "", rangeCmdArgs)), []);
      CompilerOption("no-indentation-syntax", tagNone, OptionUnit (fun () -> tcConfigB.light <- Some(false)), Some(DeprecatedCommandLineOption("--no-indentation-syntax", "", rangeCmdArgs)), []); 
    ]
          
let deprecatedFlagsFsi tcConfigB = deprecatedFlagsBoth tcConfigB
let deprecatedFlagsFsc tcConfigB =
    deprecatedFlagsBoth tcConfigB @
    [
    cliRootFlag tcConfigB;
    CompilerOption("jit-optimize", tagNone, OptionUnit (fun _ -> tcConfigB.optSettings <- { tcConfigB.optSettings with jitOptUser = Some true }), Some(DeprecatedCommandLineOption("--jit-optimize", "", rangeCmdArgs)), []);
    CompilerOption("no-jit-optimize", tagNone, OptionUnit (fun _ -> tcConfigB.optSettings <- { tcConfigB.optSettings with jitOptUser = Some false }), Some(DeprecatedCommandLineOption("--no-jit-optimize", "", rangeCmdArgs)), []);
    CompilerOption("jit-tracking", tagNone, OptionUnit (fun _ -> tcConfigB.jitTracking <- true), Some(DeprecatedCommandLineOption("--jit-tracking", "", rangeCmdArgs)), []);
    CompilerOption("no-jit-tracking", tagNone, OptionUnit (fun _ -> tcConfigB.jitTracking <- false), Some(DeprecatedCommandLineOption("--no-jit-tracking", "", rangeCmdArgs)), []);
    CompilerOption("progress", tagNone, OptionUnit (fun () -> progress := true), Some(DeprecatedCommandLineOption("--progress", "", rangeCmdArgs)), []);
    (compilingFsLibFlag tcConfigB) ;
    CompilerOption("version", tagString, OptionString (fun s -> tcConfigB.version <- VersionString s), Some(DeprecatedCommandLineOption("--version", "", rangeCmdArgs)), []);
//  "--clr-mscorlib", OptionString (fun s -> warning(Some(DeprecatedCommandLineOption("--clr-mscorlib", "", rangeCmdArgs))) ;   tcConfigB.Build.mscorlib_assembly_name <- s), "\n\tThe name of mscorlib on the target CLR"; 
    CompilerOption("generate-config-file", tagNone, OptionUnit (fun () -> tcConfigB.generateConfigFile <- true), Some(DeprecatedCommandLineOption("--generate-config-file", "", rangeCmdArgs)), []);
    CompilerOption("local-optimize", tagNone, OptionUnit (fun _ -> tcConfigB.optSettings <- { tcConfigB.optSettings with localOptUser = Some true }), Some(DeprecatedCommandLineOption("--local-optimize", "", rangeCmdArgs)), []);
    CompilerOption("no-local-optimize", tagNone, OptionUnit (fun _ -> tcConfigB.optSettings <- { tcConfigB.optSettings with localOptUser = Some false }), Some(DeprecatedCommandLineOption("--no-local-optimize", "", rangeCmdArgs)), []);
    CompilerOption("cross-optimize", tagNone, OptionUnit (fun _ -> tcConfigB.optSettings <- { tcConfigB.optSettings with crossModuleOptUser = Some true }), Some(DeprecatedCommandLineOption("--cross-optimize", "", rangeCmdArgs)), []);
    CompilerOption("no-cross-optimize", tagNone, OptionUnit (fun _ -> tcConfigB.optSettings <- { tcConfigB.optSettings with crossModuleOptUser = Some false }), Some(DeprecatedCommandLineOption("--no-cross-optimize", "", rangeCmdArgs)), []);
    CompilerOption("no-string-interning", tagNone, OptionUnit (fun () -> tcConfigB.internConstantStrings <- false), Some(DeprecatedCommandLineOption("--no-string-interning", "", rangeCmdArgs)), []);
    CompilerOption("statistics", tagNone, OptionUnit (fun () -> tcConfigB.stats <- true), Some(DeprecatedCommandLineOption("--statistics", "", rangeCmdArgs)), []);
    CompilerOption("generate-filter-blocks", tagNone, OptionUnit (fun () -> tcConfigB.generateFilterBlocks <- true), Some(DeprecatedCommandLineOption("--generate-filter-blocks", "", rangeCmdArgs)), []); 
    CompilerOption("max-errors", tagN, OptionInt (fun n -> tcConfigB.maxErrors <- n), Some(DeprecatedCommandLineOption("--max-errors", "Use '--maxerrors' instead", rangeCmdArgs)), []);
    CompilerOption("debug-file", tagNone, OptionString (fun s -> tcConfigB.debugSymbolFile <- Some s), Some(DeprecatedCommandLineOption("--debug-file", "Use '--pdb' instead", rangeCmdArgs)), []);
    CompilerOption("no-debug-file", tagNone,  OptionUnit (fun () -> tcConfigB.debuginfo <- false), Some(DeprecatedCommandLineOption("--no-debug-file", "Use '--debug-' instead", rangeCmdArgs)), []);
    CompilerOption("Ooff", tagNone, OptionUnit (fun () -> SetOptimizeOff(tcConfigB)), Some(DeprecatedCommandLineOption("-Ooff", "Use '--optimize-' instead", rangeCmdArgs)), []);
    mlKeywordsFlag ;
    gnuStyleErrorsFlag tcConfigB;
    ]


// OptionBlock: Miscellaneous options
//-----------------------------------

let DisplayBannerText tcConfigB =
    if tcConfigB.showBanner then (
        printfn "%s, (c) Microsoft Corporation, All Rights Reserved" tcConfigB.product
        printfn "F# Version %s, compiling for .NET Framework Version %s"  Ilxconfig.version (Ilsupp.clrVersion())
    )

/// FSC only help. (FSI has it's own help function).
let displayHelpFsc tcConfigB (blocks:CompilerOptionBlock list) =
    DisplayBannerText tcConfigB;
    printCompilerOptionBlocks blocks
    exit 0
      
let miscFlagsBoth tcConfigB = 
    [   CompilerOption("nologo", tagNone, OptionUnit (fun () -> tcConfigB.showBanner <- false), None, [ "Suppress compiler copyright message" ]);
    ]
      
let miscFlagsFsc tcConfigB =
    miscFlagsBoth tcConfigB @
    [   CompilerOption("help", tagNone, OptionHelp (fun blocks -> displayHelpFsc tcConfigB blocks), None, [ "Display this usage message (Short form: -?)" ])
    ]
let miscFlagsFsi tcConfigB = miscFlagsBoth tcConfigB


// OptionBlock: Abbreviations of existing options
//-----------------------------------------------
      
let abbreviatedFlagsBoth tcConfigB =
    [
        CompilerOption("d", tagString, OptionString (defineSymbol tcConfigB), None, [ "Short form of --define"]);
        CompilerOption("O", tagNone, OptionSwitch (SetOptimizeSwitch tcConfigB) , None, [ "Short form of --optimize[+|-]"]);
        CompilerOption("g", tagNone, OptionSwitch (SetDebugSwitch tcConfigB None), None, [ "Short form of --debug"]);
        CompilerOption("i", tagString, OptionUnit (fun () -> tcConfigB.printSignature <- true), None, [ "Short form of --sig"]);
        referenceFlagAbbrev tcConfigB; (* -r <dll> *)
        libFlagAbbrev tcConfigB;       (* -I <dir> *)
    ]

let abbreviatedFlagsFsi tcConfigB = abbreviatedFlagsBoth tcConfigB
let abbreviatedFlagsFsc tcConfigB =
    abbreviatedFlagsBoth tcConfigB @
    [   (* FSC only abbreviated options *)
        CompilerOption("o", tagString, OptionString (setOutFileName tcConfigB), None, [ "Short form of --out"]);
        CompilerOption("a", tagString, OptionUnit (fun () -> tcConfigB.target <- Dll), None, [ "Short form of --target library" ]);
        (* FSC help abbreviations. FSI has it's own help options... *)
        CompilerOption("?"        , tagNone, OptionHelp (fun blocks -> displayHelpFsc tcConfigB blocks), None, [ "Short form of --help" ]);
        CompilerOption("help"     , tagNone, OptionHelp (fun blocks -> displayHelpFsc tcConfigB blocks), None, [ "Short form of --help" ]);
        CompilerOption("full-help", tagNone, OptionHelp (fun blocks -> displayHelpFsc tcConfigB blocks), None, [ "Short form of --help" ])
    ]
    
let abbrevFlagSet tcConfigB isFsc =
    let mutable argList : string list = []
    for c in ((if isFsc then abbreviatedFlagsFsc else abbreviatedFlagsFsi) tcConfigB) do
        match c with
        | CompilerOption(arg,_,OptionString s,_,_)
        | CompilerOption(arg,_,OptionStringList s,_,_) -> argList <- argList @ ["-"^arg;"/"^arg]
        | _ -> ()
    Set argList
    
// check for abbreviated options that accept spaces instead of colons, and replace the spaces
// with colons when necessary
let PostProcessCompilerArgs (abbrevArgs : string Set) (args : string[]) =
    let mutable i = 0
    let mutable idx = 0
    let len = args.Length
    let mutable arga : string[] = Array.create len ""
    
    while i < len do
        if not(abbrevArgs.Contains(args.[i])) || i = (len - 1)  then
            arga.[idx] <- args.[i] ;
            i <- i+1
        else
            arga.[idx] <- args.[i] ^ ":" ^ args.[i+1]
            i <- i + 2
        idx <- idx + 1
    Array.to_list arga.[0 .. (idx - 1)]

// OptionBlock: QA options
//------------------------
      
let testingAndQAFlags tcConfigB =
  [
    CompilerOption("dumpAllCommandLineOptions", tagNone, OptionHelp(fun blocks -> dumpCompilerOptionBlocks blocks), None, ["Command line options"])
  ]


// Core compiler options, overview
//--------------------------------
      
(*  The "core" compiler options are "the ones defined here".
    Currently, fsi.exe has some additional options, defined in fsi.fs.
    
    The compiler options are put into blocks, named as <block>Flags.
    Some block options differ between fsc and fsi, in this case they split as <block>FlagsFsc and <block>FlagsFsi.
    
    The "service.ml" (language service) flags are the same as the fsc flags (except help options are removed).
    REVIEW: is this correct? what about fsx files in VS and fsi options?
  
    Block                      | notes
    ---------------------------|--------------------
    outputFileFlags            |
    inputFileFlags             |
    resourcesFlags             |
    codeGenerationFlags        |
    errorsAndWarningsFlags     |
    languageFlags              |
    htmlFlags                  |
    miscFlags                  |
    advancedFlags              |
    internalFlags              |
    abbreviatedFlags           |
    deprecatedFlags            | REVIEW: some of these may have been valid for fsi.exe?
    fsiSpecificFlags           | These are defined later, in fsi.fs
    ---------------------------|--------------------
*)

// Core compiler options exported to fsc.ml, service.ml and fsi.fs
//----------------------------------------------------------------

/// The core/common options used by fsc.exe. [not currently extended by fsc.ml].
let GetCoreFscCompilerOptions (tcConfigB: TcConfigBuilder) = 
  [ PublicOptions("- OUTPUT FILES -"        , outputFileFlagsFsc      tcConfigB); 
    PublicOptions("- INPUT FILES -"         , inputFileFlagsFsc       tcConfigB);
    PublicOptions("- RESOURCES -"           , resourcesFlagsFsc       tcConfigB);
    PublicOptions("- CODE GENERATION -"     , codeGenerationFlags     tcConfigB);
    PublicOptions("- ERRORS AND WARNINGS -" , errorsAndWarningsFlags  tcConfigB);
    PublicOptions("- LANGUAGE -"            , languageFlags           tcConfigB);
    PublicOptions("- HTML -"                , htmlFlagsFsc            tcConfigB);
    PublicOptions("- MISCELLANEOUS -"       , miscFlagsFsc            tcConfigB);
    PublicOptions("- ADVANCED -"            , advancedFlagsFsc        tcConfigB);
    PrivateOptions(List.concat              [ internalFlags           tcConfigB;
                                              abbreviatedFlagsFsc     tcConfigB;
                                              deprecatedFlagsFsc      tcConfigB;
                                              testingAndQAFlags       tcConfigB])
  ]

/// The core/common options used by the F# VS Language Service.
/// Filter out OptionHelp which does printing then exit. This is not wanted in the context of VS!!
let GetCoreServiceCompilerOptions (tcConfigB:TcConfigBuilder) =
  let isHelpOption = function CompilerOption(s,tag,OptionHelp _,_,descr) -> true | opt -> false
  List.map (filterCompilerOptionBlock (isHelpOption >> not)) (GetCoreFscCompilerOptions tcConfigB)

/// The core/common options used by fsi.exe. [note, some additional options are added in fsi.fs].
let GetCoreFsiCompilerOptions (tcConfigB: TcConfigBuilder) =
  [ PublicOptions("- OUTPUT FILES -"        , outputFileFlagsFsi      tcConfigB);
    PublicOptions("- INPUT FILES -"         , inputFileFlagsFsi       tcConfigB);
    PublicOptions("- RESOURCES -"           , resourcesFlagsFsi       tcConfigB);
    PublicOptions("- CODE GENERATION -"     , codeGenerationFlags     tcConfigB);
    PublicOptions("- ERRORS AND WARNINGS -" , errorsAndWarningsFlags  tcConfigB);
    PublicOptions("- LANGUAGE -"            , languageFlags           tcConfigB);
    // Note: no HTML block for fsi.exe
    PublicOptions("- MISCELLANEOUS -"       , miscFlagsFsi            tcConfigB);
    PublicOptions("- ADVANCED -"            , advancedFlagsFsi        tcConfigB);
    PrivateOptions(List.concat              [ internalFlags           tcConfigB;
                                              abbreviatedFlagsFsi     tcConfigB;
                                              deprecatedFlagsFsi      tcConfigB;
                                              testingAndQAFlags       tcConfigB])
  ]


(*----------------------------------------------------------------------------
!* parsing - ParseOneInputFile
 * Filename is either (ml/mli/fs/fsi source) or (.resx file).
 * For source file, parse it to AST. For .resx compile to .resource and
 * read.
 *--------------------------------------------------------------------------*)

let ParseOneInputFile (tcConfig:TcConfig,lexResourceManager,conditionalCompilationDefines,filename,canContainEntryPoint,errorLogger) =
    try 
       let lower = String.lowercase filename
       if List.exists (Filename.check_suffix lower) (sigSuffixes@implSuffixes)  then  
            if not(Internal.Utilities.FileSystem.File.SafeExists(filename)) then
                error(Error("Source file '"^filename^"' could not be found",rangeStartup))
            // bug 3155: if the file name is indirect, use a full path
            let shortFilename = SanitizeFileName filename tcConfig.implicitIncludeDir 
            let stream,reader,lexbuf = UnicodeLexing.UnicodeFileAsLexbuf(filename,tcConfig.inputCodePage) 
            use stream = stream
            use reader = reader
            let skip = true in (* don't report whitespace from lexer *)
            let lightSyntaxStatus = LightSyntaxStatus (tcConfig.ComputeLightSyntaxInitialStatus filename,true) 
            let syntaxFlagRequired = tcConfig.ComputeSyntaxFlagRequired(filename)
            let lexargs = mkLexargs ((fun () -> tcConfig.implicitIncludeDir),filename,conditionalCompilationDefines@tcConfig.conditionalCompilationDefines,lightSyntaxStatus,lexResourceManager, ref [],errorLogger)
            let input = 
                Lexhelp.usingLexbufForParsing (lexbuf,filename,None) (fun lexbuf ->
                    if verbose then dprintn ("Parsing... "^shortFilename);
                    let tokenizer = Lexfilter.create syntaxFlagRequired lightSyntaxStatus (Lexer.token lexargs skip) lexbuf

                    if tcConfig.tokenizeOnly then 
                        while true do 
                            Printf.printf "tokenize - getting one token from %s\n" shortFilename;
                            let t = tokenizer.lexer lexbuf
                            Printf.printf "tokenize - got %s @ %a\n" (Parser.token_to_string t) output_range (GetLexerRange lexbuf);
                            (match t with Parser.EOF _ -> exit 0 | _ -> ());
                            if lexbuf.IsPastEndOfStream then  Printf.printf "!!! at end of stream\n"

                    if tcConfig.testInteractionParser then 
                        while true do 
                            match (Parser.interaction tokenizer.lexer lexbuf) with
                            | IDefns(l,m) -> dprintf "Parsed OK, got %d defs @ %a\n" (List.length l) output_range m;
                            | IHash (_,m) -> dprintf "Parsed OK, got hash @ %a\n" output_range m;
                        exit 0;

                    let res = ParseInput(tokenizer.lexer,errorLogger,lexbuf,None,filename,canContainEntryPoint)

                    if tcConfig.reportNumDecls then 
                        let rec flattenSpecs specs = 
                              specs |> List.collect (function (Spec_module (_,subDecls,_)) -> flattenSpecs subDecls | spec -> [spec])
                        let rec flattenDefns specs = 
                              specs |> List.collect (function (Def_module (_,subDecls,_,_)) -> flattenDefns subDecls | defn -> [defn])

                        let flattenModSpec (ModuleOrNamespaceSpec(_,_,decls,_,_,_,_)) = flattenSpecs decls
                        let flattenModImpl (ModuleOrNamespaceImpl(_,_,decls,_,_,_,_)) = flattenDefns decls
                        match res with 
                        | SigFileInput(SigFile(_,_,_,_,specs)) -> 
                            dprintf "parsing yielded %d specs" (List.length (List.collect flattenModSpec specs))
                        | ImplFileInput(ImplFile(_,_,_,_,_,impls,_)) -> 
                            dprintf "parsing yielded %d definitions" (List.length (List.collect flattenModImpl impls))
                    res
                )
            if verbose then dprintn ("Parsed "^shortFilename);
            Some input 
       
       else error(Error("The file extension  of "^(SanitizeFileName filename tcConfig.implicitIncludeDir)^" is not recognized.  Source files must have extension .fs, .fsi, .fsx, .fsscript, .ml or .mli",rangeStartup))
    with e -> (* errorR(Failure("parse failed")); *) errorRecovery e rangeStartup; None 

(*----------------------------------------------------------------------------
!* PrintWholeAssemblyImplementation
 *--------------------------------------------------------------------------*)

let showTermFileCount = ref 0    
let PrintWholeAssemblyImplementation (tcConfig:TcConfig) outfile header expr =
    if tcConfig.showTerms then
        if tcConfig.writeTermsToFiles then 
            let filename = outfile ^ ".terms"
            let n = !showTermFileCount
            showTermFileCount := n+1;
            use f = open_out (filename ^ "-" ^ string n ^ "-" ^ header)
            Layout.outL f (Layout.squashTo 192 (AssemblyL expr));
        else 
            dprintf "\n------------------\nshowTerm: %s:\n" header;
            Layout.outL stderr (Layout.squashTo 192 (AssemblyL expr));
            dprintf "\n------------------\n";

(*----------------------------------------------------------------------------
!* ReportTime 
 *--------------------------------------------------------------------------*)

let tPrev = ref None
let nPrev = ref None
let ReportTime (tcConfig:TcConfig) descr =
    
    match !nPrev with
    | None -> ()
    | Some prevDescr ->
        if tcConfig.pause then 
            dprintf "[done '%s', entering '%s'] press any key... " prevDescr descr;
            System.Console.ReadLine() |> ignore;
        // Intentionally putting this right after the pause so a debugger can be attached.
        match tcConfig.simulateException with
        | Some("fsc-oom") -> raise(System.OutOfMemoryException())
        | Some("fsc-an") -> raise(System.ArgumentNullException("simulated"))
        | Some("fsc-invop") -> raise(System.InvalidOperationException())
        | Some("fsc-av") -> raise(System.AccessViolationException())
        | Some("fsc-aor") -> raise(System.ArgumentOutOfRangeException())
        | Some("fsc-dv0") -> raise(System.DivideByZeroException())
        | Some("fsc-nfn") -> raise(System.NotFiniteNumberException())
        | Some("fsc-oe") -> raise(System.OverflowException())
        | Some("fsc-atmm") -> raise(System.ArrayTypeMismatchException())
        | Some("fsc-bif") -> raise(System.BadImageFormatException())
        | Some("fsc-knf") -> raise(System.Collections.Generic.KeyNotFoundException())
        | Some("fsc-ior") -> raise(System.IndexOutOfRangeException())
        | Some("fsc-ic") -> raise(System.InvalidCastException())
        | Some("fsc-ip") -> raise(System.InvalidProgramException())
        | Some("fsc-ma") -> raise(System.MemberAccessException())
        | Some("fsc-ni") -> raise(System.NotImplementedException())
        | Some("fsc-nr") -> raise(System.NullReferenceException())
        | Some("fsc-oc") -> raise(System.OperationCanceledException())
        | Some("fsc-fail") -> failwith "simulated"
        | _ -> ()




    if (tcConfig.showTimes || verbose) then 
        // Note that Sys.time calls are relatively expensive on the startup path so we don't
        // make this call unless showTimes has been turned on.
        let timeNow = System.Diagnostics.Process.GetCurrentProcess().UserProcessorTime.TotalSeconds
        let maxGen = System.GC.MaxGeneration
        let gcNow = [| for i in 0 .. maxGen -> System.GC.CollectionCount(i) |]
        let ptime = System.Diagnostics.Process.GetCurrentProcess()
        let wsNow = ptime.WorkingSet/1000000

        match !tPrev, !nPrev with
        | Some (timePrev,gcPrev:int[]),Some prevDescr ->
            let spanGC = [| for i in 0 .. maxGen -> System.GC.CollectionCount(i) - gcPrev.[i] |]
            dprintf "TIME: %4.1f Delta: %4.1f Mem: %3d" 
                timeNow (timeNow - timePrev) 
                wsNow;
            dprintf " G0: %3d G1: %2d G2: %2d [%s]\n" 
                spanGC.[Operators.min 0 maxGen] spanGC.[Operators.min 1 maxGen] spanGC.[Operators.min 2 maxGen]
                prevDescr

        | _ -> ()
        tPrev := Some (timeNow,gcNow)

    nPrev := Some descr

(*----------------------------------------------------------------------------
!* OPTIMIZATION - support - addDllToOptEnv
 *--------------------------------------------------------------------------*)

let AddExternalCcuToOpimizationEnv optEnv ccuinfo =
    match ccuinfo.FSharpOptimizationData.Force() with 
    | None -> optEnv
    | Some(data) -> Opt.BindCcu ccuinfo.FSharpViewOfMetadata data optEnv 

(*----------------------------------------------------------------------------
!* OPTIMIZATION - support - optimize
 *--------------------------------------------------------------------------*)


let InitialOptimizationEnv (tcImports:TcImports) =
    let ccuinfos = tcImports.GetCcuInfos()
    let optEnv = Opt.empty_env
    let optEnv = List.fold AddExternalCcuToOpimizationEnv optEnv ccuinfos
    optEnv
     
let ApplyAllOptimizations (tcConfig:TcConfig,tcGlobals,outfile,importMap,isIncrementalFragment,optEnv,ccu:ccu,tassembly:TypedAssembly) =
    (* NOTE: optEnv - threads through *)
    (*---*) 
    (* Always optimize once - the results of this step give the x-module optimization *)
    (* info.  Subsequent optimization steps choose representations etc. which we don't *)
    (* want to save in the x-module info (i.e. x-module info is currently "high level"). *)
    PrintWholeAssemblyImplementation tcConfig outfile "pass-start" tassembly;
#if DEBUG
    if tcConfig.showOptimizationData then dprintf "Expression prior to optimization:\n%s\n" (Layout.showL (Layout.squashTo 192 (AssemblyL tassembly)));
    if tcConfig.showOptimizationData then dprintf "CCU prior to optimization:\n%s\n" (Layout.showL (Layout.squashTo 192 (EntityL ccu.Contents)));
#endif

    let optEnv0 = optEnv
    let (TAssembly(implFiles)) = tassembly
    ReportTime tcConfig ("Optimizations");
    let results,(optEnvFirstLoop,_,_) = 
        ((optEnv0,optEnv0,optEnv0),implFiles) ||> List.mapfold (fun (optEnvFirstLoop,optEnvExtraLoop,optEnvFinalSimplify) implFile -> 

            // Only do abstract_big_targets on the first pass!  Only do it when TLR is on!  
            let optSettings = tcConfig.optSettings 
            let optSettings = { optSettings with abstractBigTargets = tcConfig.doTLR }
            let optSettings = { optSettings with reportingPhase = true }
            
            //ReportTime tcConfig ("Initial simplify");
            let optEnvFirstLoop,implFile,implFileOptData = 
                Opt.OptimizeImplFile(optSettings,ccu,tcGlobals,importMap,optEnvFirstLoop,isIncrementalFragment,implFile)

            // Only do this on the first pass!
            let optSettings = { optSettings with abstractBigTargets = false }
            let optSettings = { optSettings with reportingPhase = false }
#if DEBUG
            if tcConfig.showOptimizationData then dprintf "Optimization implFileOptData:\n%s\n" (Layout.showL (Layout.squashTo 192 (Opt.moduleInfoL implFileOptData)));
#endif

            let implFile,optEnvExtraLoop = 
                if tcConfig.extraOptimizationIterations > 0 then 
                    //ReportTime tcConfig ("Extra simplification loop");
                    let optEnvExtraLoop,implFile, _ = Opt.OptimizeImplFile(optSettings,ccu,tcGlobals,importMap,optEnvExtraLoop,isIncrementalFragment,implFile)
                    //PrintWholeAssemblyImplementation tcConfig outfile (Printf.sprintf "extra-loop-%d" n) implFile;
                    implFile,optEnvExtraLoop
                else
                    implFile,optEnvExtraLoop

            let implFile = 
                if tcConfig.doDetuple then 
                    //ReportTime tcConfig ("Detupled optimization");
                    let implFile = implFile |> Detuple.DetupleImplFile ccu tcGlobals 
                    //PrintWholeAssemblyImplementation tcConfig outfile "post-detuple" implFile;
                    implFile 
                else implFile 

            let implFile = 
                if tcConfig.doTLR then 
                    implFile |> Tlr.MakeTLRDecisions ccu tcGlobals 
                else implFile 

            let implFile = 
                Lowertop.LowerImplFile tcGlobals implFile
              
            let implFile,optEnvFinalSimplify =
                if tcConfig.doTLR then 
                    //ReportTime tcConfig ("Final simplify pass");
                    let optEnvFinalSimplify,implFile, _ = Opt.OptimizeImplFile(optSettings,ccu,tcGlobals,importMap,optEnvFinalSimplify,isIncrementalFragment,implFile)
                    //PrintWholeAssemblyImplementation tcConfig outfile "post-rec-opt" implFile;
                    implFile,optEnvFinalSimplify 
                else 
                    implFile,optEnvFinalSimplify 
            (implFile,implFileOptData),(optEnvFirstLoop,optEnvExtraLoop,optEnvFinalSimplify))

    let implFiles,implFileOptDatas = List.unzip results
    let assemblyOptData = Opt.UnionModuleInfos implFileOptDatas
    let tassembly = TAssembly(implFiles)
    PrintWholeAssemblyImplementation tcConfig outfile "pass-end" tassembly;
    ReportTime tcConfig ("Ending Optimizations");

    tassembly, assemblyOptData,optEnvFirstLoop

(*----------------------------------------------------------------------------
!* ILX generation 
 *--------------------------------------------------------------------------*)

let IlxgenEnvInit (tcConfig:TcConfig,tcImports:TcImports,tcGlobals,generatedCcu) = 
    let ccus = tcImports.GetCcusInDeclOrder()
    let ilxGenEnv = Ilxgen.GetEmptyIlxGenEnv generatedCcu
    Ilxgen.AddExternalCcusToIlxGenEnv tcGlobals ilxGenEnv ccus


let GenerateIlxCode(isInteractive,isInteractiveOnMono, tcGlobals, tcConfig:TcConfig, importMap,topAttrs,optimizedImpls,generatedCcu,fragName,ilxGenEnv) =
    if !progress then dprintf "Generating ILX code...\n";
    let cenv = { g=tcGlobals;
                 viewCcu = generatedCcu;
                 generateFilterBlocks = tcConfig.generateFilterBlocks;
                 emitConstantArraysUsingStaticDataBlobs = not isInteractiveOnMono;
                 workAroundReflectionEmitBugs=isInteractive; (* REVIEW: is this still required? *)
                 debug= tcConfig.debuginfo;
                 fragName = fragName;
                 localOptimizationsAreOn= tcConfig.optSettings.localOpt ();
                 Ilxgen.amap=importMap;
                 mainMethodInfo= (if (tcConfig.target = Dll || tcConfig.target = Module) then None else Some topAttrs.mainMethodAttrs);
                 emptyProgramOk = isInteractive;
               }
    Ilxgen.GenerateCode cenv ilxGenEnv optimizedImpls (topAttrs.assemblyAttrs,topAttrs.netModuleAttrs) 


        
(*----------------------------------------------------------------------------
!* Assembly ref normalization: make sure all assemblies are referred to
 * by the same references.
 *--------------------------------------------------------------------------*)

let NormalizeAssemblyRefs (tcImports:TcImports) = 
    if verbose then dprintn "Normalizing assembly references in generated IL code...";
    let assemFinder nm = tcImports.FindDllInfo(Range.rangeStartup,nm)
    (fun scoref ->
       match scoref with 
       | ScopeRef_local 
       | ScopeRef_module _ -> scoref
       | ScopeRef_assembly aref -> (assemFinder aref.Name).ILScopeRef)

let fsharpModuleName (t:target) (s:string) = 
    // return the name of the file as a module name
    let ext = match t with | Dll -> "dll" | Module -> "netmodule" | _ -> "exe"
    s + "." + ext


let ignoreFailureOnMono1_1_16 f = try f() with _ -> ()

let DoWithErrorColor isWarn f =
    if not enableConsoleColoring then
        f()
    else
        let foreBackColor =
            try
                let c = Console.ForegroundColor // may fail, perhaps on Mac, and maybe ForegroundColor is Black
                let b = Console.BackgroundColor // may fail, perhaps on Mac, and maybe BackgroundColor is White
                Some (c,b)
            with
                e -> None
        match foreBackColor with
          | None -> f() (* could not get console colours, so no attempt to change colours, can not set them back *)
          | Some (c,b) ->
              try
                let warnColor  = if Console.BackgroundColor = ConsoleColor.White then ConsoleColor.DarkBlue else ConsoleColor.Cyan
                let errorColor = ConsoleColor.Red
                ignoreFailureOnMono1_1_16 (fun () -> Console.ForegroundColor <- (if isWarn then warnColor else errorColor));
                f();
              finally
                ignoreFailureOnMono1_1_16 (fun () -> Console.ForegroundColor <- c)


          

        
