// (c) Microsoft Corporation. All rights reserved
(*----------------------------------------------------------------------------
 * API to the compiler as an incremental service for lexing, parsing,
 * type checking and intellisense-like environment-reporting.
 *--------------------------------------------------------------------------*)

#light 
namespace Microsoft.FSharp.Compiler.SourceCodeServices

open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.Range
open System.Collections.Generic

/// Represents encoded information for the end-of-line continutation of lexing
type LexState = int64

/// A line/column pair
type Position = int * int
/// A start-position/end-position pair
type Range = Position * Position

type TokenColorKind =
    | Default = 0
    | Text = 0
    | Keyword = 1
    | Comment = 2
    | Identifier = 3
    | String = 4
    | UpperIdentifier = 5
    | InactiveCode = 7
    | PreprocessorKeyword = 8
    | Number = 9
    
type TriggerClass =
    | None         = 0x00000000
    | MemberSelect = 0x00000001
    | MatchBraces  = 0x00000002
    | ChoiceSelect = 0x00000004
    | MethodTip    = 0x000000F0
    | ParamStart   = 0x00000010
    | ParamNext    = 0x00000020
    | ParamEnd     = 0x00000040    
    
type TokenCharKind = 
    | Default     = 0x00000000
    | Text        = 0x00000000
    | Keyword     = 0x00000001
    | Identifier  = 0x00000002
    | String      = 0x00000003
    | Literal     = 0x00000004
    | Operator    = 0x00000005
    | Delimiter   = 0x00000006
    | WhiteSpace  = 0x00000008
    | LineComment = 0x00000009
    | Comment     = 0x0000000A    
    
/// Information about a particular token from the tokenizer
type TokenInformation = {
    /// Left column of the token.
    LeftColumn:int;
    /// Right column of the token.
    RightColumn:int;
    ColorClass:TokenColorKind;
    CharClass:TokenCharKind;
    TriggerClass:TriggerClass;
    /// The tag is an integer identifier for the token
    Tag:int
    /// Provides additional information about the token
    TokenName:string }

type Severity = Warning | Error

type ErrorInfo = {
    StartLine:int
    EndLine:int
    StartColumn:int
    EndColumn:int
    Severity:Severity
    Message:string }
    
[<Sealed>]
type TextResult =
    /// The text to be displayed
    member Text: string
    /// The file related to the construct for which the DataTipText is being displayed and the XmlDoc string for the construct
    member XMLFileAndSig: (string * string) option    
    
[<Sealed>]
type Declaration =
    member Name : string
    member DescriptionText : TextResult
    member Glyph : int
    
[<Sealed>]
type DeclarationSet =
    member Items : Declaration array
    
type Param = 
    { Name: string;
      Display: string;
      Description: string }

type Method = 
    { Description : TextResult;
      Type: string;
      Parameters: Param array }

[<Sealed>]
type MethodOverloads = 
    member Name: string;
    member Methods: Method array 




/// Represents an item to be displayed in the navigation bar
[<Sealed>]
type DeclarationItem = 
    member Name : string
    member UniqueName : string
    member Glyph : int
    member Range : Range
    member BodyRange : Range
    member IsSingleTopLevel : bool

/// Represents top-level declarations (that should be in the type drop-down)
/// with nested declarations (that can be shown in the member drop-down)
type TopLevelDeclaration = 
    { Declaration : DeclarationItem
      Nested : DeclarationItem[] }
      
/// Represents result of 'GetNavigationItems' operation - this contains
/// all the members and currently selected indices. First level correspond to
/// types & modules and second level are methods etc.
[<Sealed>]
type NavigationItems =
    member Declarations : TopLevelDeclaration[]
    
type FindDeclResult = 
    ///  no identifier at this locn 
    | IdNotFound    
    /// no decl info in this buffer at the moment 
    | NoDeclInfo   
    /// found declaration; return (position-in-file, name-of-file, names-of-referenced-assemblies)
    | DeclFound      of Position * string * (string list)
    /// found declaration but source file doesn't exist; try to generate an .fsi
    | NeedToGenerate of (*filename of .dll*) string * (*name-fixing function*)(string -> string) * (*fully-qualified identifier to goto*)(string list)
     
type Names = string list 
type NamesWithResidue = Names * string 

[<Sealed>]
/// A handle to type information gleaned from typechecking the file. 
type TypeCheckInfo  =
    /// Resolve the names at the given location to a set of declarations
    member GetDeclarations                : Position * string * NamesWithResidue * (*tokentag:*)int -> DeclarationSet
    /// Resolve the names at the given location to give a data tip 
    member GetDataTipText                 : Position * string * Names * (*tokentag:*)int -> TextResult
    /// Resolve the names at the given location to give F1 keyword
    member GetF1Keyword                   : Position * string * Names -> string option
    // Resolve the names at the given location to a set of methods
    member GetMethods                     : Position * string * Names option * (*tokentag:*)int -> MethodOverloads
    /// Resolve the names at the given location to the declaration location of the corresponding construct
    member GetDeclarationLocation         : Position * string * Names * (*tokentag:*)int * bool -> FindDeclResult
    /// A version of `GetDeclarationLocation` augmented with the option (via the `bool`) parameter to force .fsi generation (even if source exists); this is primarily for testing
    member GetDeclarationLocationInternal : bool -> Position * string * Names * (*tokentag:*)int * bool -> FindDeclResult

[<Sealed>]
/// A handle to the results of TypeCheckSource
type TypeCheckResults =
    /// The errors returned by parsing a source file
    member Errors : ErrorInfo array
    /// A handle to type information gleaned from typechecking the file. 
    member TypeCheckInfo: TypeCheckInfo option

[<Sealed>]
type UntypedParseInfo = 
    /// Name of the file for which this information were created
    member FileName                       : string
    /// Get declaraed items and the selected item at the specified location
    member GetNavigationItems             : unit -> NavigationItems
    /// Return the inner-most range associated with a possible breakpoint location
    member ValidateBreakpointLocation : Position -> Range option
    /// When these files change then the build is invalid
    member DependencyFiles : unit -> string list
    
/// This type represents results obtained from parsing, before the type checking is performed
/// It can be used for populating navigation information and for running the 
/// 'TypeCheckSource' method to get the full information.
type UntypedParseResults

type ParseOptions =
    { 
      // The FileName is relevant to the ParseOptions because some aspects of the
      // default environment depend on whether the file is a .fsx or .fs
      FileName: string;
      ProjectFileName: string;
      ProjectFileNames: string array;
      ProjectOptions: string array;
      /// When true, the typechecking environment is known a priori to be incomplete. 
      /// This can happen, for example, when a .fs file is opened outside of a project.
      /// It may be appropriate, then, to not show error messages related to type checking
      /// since they will just be noise.
      IsIncompleteTypeCheckEnvironment : bool;
    }
    static member Defaults : ParseOptions
         
          
/// Object to tokenize a line of F# source code, starting with the given lexState.  The lexState should be 0 for
/// the first line of text. Returns an array of ranges of the text and two enumerations categorizing the
/// tokens and characters covered by that range, i.e. TokenColorKind and TokenCharKind.  The enumerations
/// are somewhat adhoc but useful enough to give good colorization options to the user in an IDE.
///
/// A new lexState is also returned.  An IDE-plugin should in general cache the lexState 
/// values for each line of the edited code.
[<Sealed>]
type Tokenizer =
    new : string * string list * string -> Tokenizer
    member StartNewLine : unit -> unit 
    member ScanToken : LexState -> Option<TokenInformation> * LexState
    
/// Information about the compilation environment    
module CompilerEnvironment =
    /// These are the names of assemblies that should be referenced for scripting (.fsx)
    val DefaultReferencesForScripting : string list
    /// These are the names of assemblies that should be referenced for .fs, .ml, .fsi, .mli files that
    /// are not asscociated with a project.
    val DefaultReferencesForOrphanSources : string list
    /// Return the compilation defines that should be used when editing the given file.
    val GetCompilationDefinesForEditing : filename : string * compilerFlags : string list -> string list

/// Information about the debugging environment
module DebuggerEnvironment =
    /// Return the language ID, which is the expression evaluator id that the
    /// debugger will use.
    val GetLanguageID : unit -> System.Guid
    
/// Callbacks to notify of background changes.
/// These will be called with a Post to the given SynchronizationContext
[<AutoSerializable(false)>]
type BackgroundCompilerEvents = 
    {   /// This file has become eligible to be re-typechecked.
        FileTypeCheckStateIsDirty: string -> unit }
        
// Identical to _VSFILECHANGEFLAGS in vsshell.idl
type DependencyChangeCode =
    | FileChanged = 0x00000001
    | TimeChanged = 0x00000002
    | Deleted = 0x00000008
    | Added = 0x00000010        

[<Sealed>]
[<AutoSerializable(false)>]      
type InteractiveChecker =
    /// Create an instance of an InteractiveChecker.  Currently resources are not reclaimed.
    static member Create : BackgroundCompilerEvents -> InteractiveChecker
    /// Parse a source code file, returning information about brace matching in the file
    /// Return an enumeration of the matching parethetical tokens in the file
    member MatchBraces : source: string * options: ParseOptions -> (Range * Range) array
    /// Parse a source code file, returning a handle that can be used for obtaining navigation bar information
    /// To get the full information, call 'TypeCheckSource' method on the result
    member UntypedParse : source: string * options: ParseOptions -> UntypedParseInfo

    /// Typecheck a source code file, returning a handle to the results of the parse including
    /// the reconstructed types in the file.
    ///
    /// Return None if the background builder is not yet done prepring the type check results for the antecedent to the 
    /// file.
    member TypeCheckSource : parsed: UntypedParseInfo * source: string * options: ParseOptions -> TypeCheckResults option

    /// Try to get recent type check results for a file. This may arbitrarily refuse to return any
    /// results if the InteractiveChecker would like a chance to recheck the file, in which case
    /// UntypedParse and TypeCheckSource should be called. If the source of the file
    /// has changed the results returned by this function may be out of date, though may
    /// still be usable for generating intellsense menus and information.
    member TryGetRecentTypeCheckResultsForFile : options: ParseOptions -> (UntypedParseInfo * TypeCheckResults) option
        
    /// This function is called when the configuration is known to have changed for reasons not encoded in the ParseOptions.
    /// For example, dependent references may have been deleted or created.
    member InvalidateConfiguration : options : ParseOptions * (string * DependencyChangeCode) list -> unit    

    /// Begin background parsing the given project.
    member StartBackgroundCompile : ParseOptions -> unit
    /// Stop the background compile.
    member StopBackgroundCompile : unit -> unit
    /// Block until the background compile finishes.
    member WaitForBackgroundCompile : unit -> unit
    
    /// Report a statistic for testability
    static member GlobalForegroundParseCountStatistic : int

    /// Report a statistic for testability
    static member GlobalForegroundTypeCheckCountStatistic : int


// These functions determine all declarations, called by fsi.fs for fsi-server requests.
module FsiIntelisense =
    val getDeclarations : Build.TcConfig * Env.TcGlobals * Build.TcImports * Build.tcState -> string -> string array -> (string * string * string * int) array

module TestHooks =
    val HookScope                                    : string * ((unit->unit)->unit) -> System.IDisposable
    val EnableFsiGenerationHook                      : unit -> System.IDisposable
    
module TestExpose =     
    val TokenInfo                                    : Parser.token -> (TokenColorKind * TokenCharKind * TriggerClass) 

module PrettyNaming =
    val IsIdentifierPartCharacter     : (char -> bool)
    val IsLongIdentifierPartCharacter : (char -> bool)
    val GetLongNameFromString         : (string -> Names)

/// Information about F# source file names
module SourceFile =
   /// Whether or not this file is compilable
   val IsCompilable : string -> bool
   /// Whether or not this file should be a single-file project
   val MustBeSingleFileProject : string -> bool
   /// Is a file generated by the language service?
   val IsFSharpLanguageServiceGenerated : string -> bool
