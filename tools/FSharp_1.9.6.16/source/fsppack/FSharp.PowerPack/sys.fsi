//==========================================================================
// (c) Microsoft Corporation 2005-2008.  The interface to the module 
// is similar to that found in versions of other ML implementations, 
// but is not an exact match.  The type signatures in this interface
// are an edited version of those generated automatically by running 
// "bin\fsc.exe -i" on the implementation file.
//===========================================================================

/// Sys: Basic system operations (for ML compatibility)
///
/// This module is only included to make it possible to cross-compile 
/// code with other ML compilers.  It may be deprecated and/or removed in 
/// a future release. You may wish to use .NET functions directly instead. 
#if INTERNALIZED_POWER_PACK
module internal Internal.Utilities.Sys
#else
[<OCamlCompatibility>]
module Microsoft.FSharp.Compatibility.OCaml.Sys
#endif

#if FX_NO_COMMAND_LINE_ARGS
#else
/// The array of command line options. Gives the command line arguments
/// as returned by <c>System.Environment.GetCommandLineArgs</c>.
[<OCamlCompatibility("Consider using System.Environment.GetCommandLineArgs directly")>]
val argv: string array
#endif

/// Returns true if a file currently exists, using System.IO.File.Exists(s).
[<OCamlCompatibility("Consider using System.IO.File.Exists directly")>]
val file_exists: string -> bool

#if FX_NO_ENVIRONMENT
#else
/// Call System.Environment.GetEnvironmentVariable. Raise <c>KeyNotFoundException</c> if the variable is not defined.
[<OCamlCompatibility("Consider using System.Environment.GetEnvironmentVariable directly")>]
val getenv: string -> string
#endif

/// Deletes a file using <c>System.IO.File.Delete</c>.
[<OCamlCompatibility("Consider using System.IO.File.Delete directly")>]
val remove: string -> unit

/// Rename a file on disk using System.IO.File.Move  
[<OCamlCompatibility("Consider using System.IO.File.Move directly")>]
val rename: string -> string -> unit

/// Sets the current working directory for the process using <c>System.IO.Directory.SetCurrentDirectory</c> 
[<OCamlCompatibility("Consider using System.IO.Directory.SetCurrentDirectory directly")>]
val chdir: string -> unit

/// Returns the current working directory for the process using <c>System.IO.Directory.GetCurrentDirectory</c>
[<OCamlCompatibility("Consider using System.IO.Directory.GetCurrentDirectory directly")>]
val getcwd: unit -> string

#if FX_NO_PROCESS_START
#else
/// Run the command and return it's exit code.
///
/// Warning: 'command' currently attempts to execute the string using 
/// the 'cmd.exe' shell processor.  If it is not present on the system 
/// then the operation will fail.  Use System.Diagnostics.Process 
/// directly to run commands in a portable way, which involves specifying 
/// the program to run and the arguments independently.
[<OCamlCompatibility("Consider using System.Diagnostics.Process directly")>]
val command: string -> int
#endif

#if FX_NO_APP_DOMAINS
#else
/// Path of the current executable, using
/// <c>System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,System.AppDomain.CurrentDomain.FriendlyName)</c>
[<OCamlCompatibility("Consider using System.AppDomain.CurrentDomain.FriendlyName directly")>]
val executable_name: string
#endif

/// The number of bits in the "int" type.
[<OCamlCompatibility("Consider using sizeof<int> directly, where this returns a size in bytes")>]
val word_size: int

#if FX_NO_PROCESS_DIAGNOSTICS
#else
/// Time consumed by the main thread. (for approximate timings).
/// Generally returns only the processor time used by the main 
/// thread of the application.
[<OCamlCompatibility("Consider using System.Diagnostics.Process.GetCurrentProcess().UserProcessorTime.TotalSeconds directly")>]
val time: unit -> float
#endif
