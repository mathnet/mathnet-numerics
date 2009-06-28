#light
namespace Microsoft.FSharp.Compiler

open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library

/// Generalized Incremental Builder. This is exposed only for unittesting purposes.
module IncrementalBuild =
  // A build scalar.
  type Scalar<'T> = interface
    end
  /// A build vector.        
  type Vector<'T> = interface
    end
  type Build
  /// Declares a vector build input.
  val InputScalar : string -> Scalar<'T>
  /// Declares a scalar build input.
  val InputVector : string -> Vector<'T>
  /// Methods for acting on build Scalars
  module Scalar = 
      /// Apply a function to one scalar to produce another.
      val Map : string -> ('I -> 'O) -> Scalar<'I> -> Scalar<'O>
      /// Apply a function to scalar value to produce a vector.
      val Multiplex : string -> ('I -> 'O array)->Scalar<'I> -> Vector<'O>

  /// Methods for acting on build Vectors
  module Vector = 
      /// Maps one vector to another using the given function.    
      val Map : string -> ('I -> 'O) -> Vector<'I> -> Vector<'O>
      /// Updates the creates a new vector with the same items but with 
      /// timestamp specified by the passed-in function.  
      val Stamp : string -> ('I -> System.DateTime) -> Vector<'I> -> Vector<'I>
      /// Apply a function to each element of the vector, threading an accumulator argument
      /// through the computation. Returns intermediate results in a vector.
      val ScanLeft : string -> ('A -> 'I -> Eventually<'A>) -> Scalar<'A> -> Vector<'I> -> Vector<'A>
      /// Apply a function to a vector to get a scalar value.
      val Demultiplex : string -> ('I array -> 'O)->Vector<'I> -> Scalar<'O>
      /// Convert a Vector into a Scalar.
      val AsScalar: string -> Vector<'I> -> Scalar<'I array> 

  /// Evaluate a build.
  val Eval : (string -> Build -> Build)
  /// Evaluate a single slot.
  val EvalSlot : (string * 'a * Build -> Build)
  /// Do one step in the build.
  val Step : (string -> Build -> Build option)
  /// Get a scalar vector. Result must be available
  val GetScalarResult<'T> : string * Build -> ('T * System.DateTime) option
  /// Get a result vector. All results must be available or thrown an exception.
  val GetVectorResult<'T> : string * Build -> 'T array
  /// Get an element of vector result or None if there were no results.
  val GetVectorResultBySlot<'T> : string*int*Build -> ('T * System.DateTime) option
  
  /// Declare build outputs and bind them to real values.
  type BuildScope = 
       new : unit -> BuildScope
       /// Declare a named scalar output.
       member DeclareScalarOutput : name:string * output:Scalar<'T> -> unit
       /// Declare a named vector output.
       member DeclareVectorOutput : name:string * output:Vector<'T> -> unit
       /// Set the conrete inputs for this build
       member GetConcreteBuild : vectorinputs:(string * int * obj list) list * scalarinputs:(string*obj) list -> Build

/// Incremental builder for F# parsing and type checking.  
module FsiGeneration =
  /// Result of generating an interface file (probably for GotoDefinition); this
  /// is (name-of-generated-file,
  ///     map-from-fully-qualified-name-to-position-in-file,
  ///     referenced-assemblies-needed-to-resolve-names-in-file)
  type FsiGenerationResult = (string * System.Collections.Generic.Dictionary<string list, int * int> * string list) option

  /// Given a .dll name, predictably generate a name for the corresponding .fsi file that we generate.
  val GeneratedFsiNameGenerator                   : string -> string
  /// The directory where generated .fsi files can be placed (and deleted) without interfering with user files.
  val PathForGeneratedVisualStudioFSharpTempFiles : string

/// Incremental builder for F# parsing and type checking.  
module IncrementalFSharpBuild =

  /// Callbacks for things that happen in the build.                  
  type BuildEvents = 
    { BeforeTypeCheckFile: string -> unit }
    static member Default : BuildEvents
    
  type FileDependency = {
        // Name of the file
        Filename : string
        // If true, then deletion or creation of this file should trigger an entirely fresh build
        ExistenceDependency : bool
        // If true, then changing this file should trigger an incremental rebuild
        IncrementalBuildDependency : bool
      }    
    
                        
  val Create :
    tcConfig : Build.TcConfig *
    projectDirectory : string *
    assemblyName : string *
    niceNameGen : Microsoft.FSharp.Compiler.Ast.NiceNameGenerator *
    resourceManager : Microsoft.FSharp.Compiler.Lexhelp.LexResourceManager *
    sourceFiles : string list *
    ensureReactive : bool *
    buildEvents : BuildEvents *
    errorLogger : ErrorLogger * 
    errorRecovery: (exn -> Range.range -> unit)  
      -> IncrementalBuild.Build * FileDependency list

  /// Perform one step in the F# build.
  val Step : IncrementalBuild.Build -> IncrementalBuild.Build option

  /// Ensure that the given file has been typechecked.
  val EvalTypeCheckSlot :
    'a * IncrementalBuild.Build -> IncrementalBuild.Build

  /// Get the preceding typecheck state of a slot, allow stale results.
  val GetAntecedentTypeCheckResultsBySlot :
    int * IncrementalBuild.Build ->
    (Build.tcState * Build.TcImports * Microsoft.FSharp.Compiler.Env.TcGlobals * Build.TcConfig * System.DateTime) option

  /// Get the final typecheck result.
  val TypeCheck :
    IncrementalBuild.Build ->
    IncrementalBuild.Build * Build.tcState * TypeChecker.topAttribs * Tast.TypedAssembly * TypeChecker.tcEnv * Build.TcImports * Env.TcGlobals * Build.TcConfig

  /// Attempts to find the slot of the given input file name. Throws an exception if it couldn't find it.    
  val GetSlotOfFileName :
    string * IncrementalBuild.Build -> int

  /// Returns a function that tries to generate an interface (.fsi) file given the name of an assembly's .dll.
  val GetFsiGenerators                            : IncrementalBuild.Build -> ((string -> string) -> string -> FsiGeneration.FsiGenerationResult) * IncrementalBuild.Build

