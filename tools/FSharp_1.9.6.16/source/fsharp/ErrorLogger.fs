// (c) Microsoft Corporation. All rights reserved

#light 

module (* internal *) Microsoft.FSharp.Compiler.ErrorLogger

open Internal.Utilities
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Range

(*------------------------------------------------------------------------
 * General error recovery mechanism
 *-----------------------------------------------------------------------*)

/// Thrown when want to add some range information to some .NET exception
exception WrappedError of exn * range

/// Thrown when immediate, local error recovery is not possible. This indicates
/// we've reported an error but need to make a non-local transfer of control.
/// Error recovery may catch this and continue (see 'errorRecovery')
exception ReportedError

/// Thrown when we stop processing the F# Interactive interactive entry or #load.
exception StopProcessing


(* common error kinds *)
exception Error of string * range
exception InternalError of string * range
exception OCamlCompatibility of string * range
exception LibraryUseOnly of range
exception Deprecated of string * range
exception Experimental of string * range
exception PossibleUnverifiableCode of range


// Range\NoRange Duals
exception UnresolvedReferenceNoRange of (*assemblyname*) string 
exception UnresolvedReferenceError of (*assemblyname*) string * range
exception UnresolvedPathReferenceNoRange of (*assemblyname*) string * (*path*) string
exception UnresolvedPathReference of (*assemblyname*) string * (*path*) string * range

// Attach a range if this is a range dual exception.
let rec AttachRange m (exn:exn) = 
    if m = range0 then exn
    else 
        match exn with
        // Strip TargetInvocationException wrappers
        | :? System.Reflection.TargetInvocationException -> AttachRange m exn.InnerException
        | UnresolvedReferenceNoRange(a) -> UnresolvedReferenceError(a,m)
        | UnresolvedPathReferenceNoRange(a,p) -> UnresolvedPathReference(a,p,m)
        | Failure(msg) -> InternalError(msg^" (Failure)",m)
        | InvalidArgument(msg) -> InternalError(msg^" (InvalidArgument)",m)
        | notARangeDual -> notARangeDual

//----------------------------------------------------------------------------
// Error logger interface

type Exiter = 
    abstract Exit : int -> 'a

let QuitProcessExiter = 
    { new Exiter with 
        member x.Exit(n) = 
            try 
              System.Environment.Exit(n)
            with _ -> 
              ()
            failwith "System.Environment.Exit did not exit!" }

type ErrorLogger = 
    abstract ErrorCount: int;
    abstract WarnSink: exn->unit;
    abstract ErrorSink: exn->unit

[<AutoOpen>]
module ErrorLoggerExtensions = 
    open System.Reflection

    // Instruct the exception not to reset itself when thrown again.
    // Why don’t we just not catch these in the first place? Because we made the design choice to ask the user to send mail to fsbugs@microsoft.com. 
    // To achieve this, we need to catch the exception, report the email address and stack trace, and then rethrow. 
    let PreserveStackTrace(exn) =
        try 
            let preserveStackTrace = typeof<System.Exception>.GetMethod("InternalPreserveStackTrace", BindingFlags.Instance ||| BindingFlags.NonPublic)
            preserveStackTrace.Invoke(exn, null) |> ignore
        with e->
           // This is probably only the mono case.
           System.Diagnostics.Debug.Assert(false, "Could not preserve stack trace for watson exception.")
           ()


    // Reraise an exception if it is one we want to report to Watson.
    let ReraiseIfWatsonable(exn) =
        match box exn with 
        // These few SystemExceptions which we don't report to Watson are because we handle these in some way in Build.ml
        | :? System.Reflection.TargetInvocationException -> ()
        | :? System.NotSupportedException  -> ()
        | :? System.IO.IOException -> () // This covers FileNotFoundException and DirectoryNotFoundException
        | :? System.UnauthorizedAccessException -> ()
        | :? FailureException // This gives reports for compiler INTERNAL ERRORs
        | :? System.SystemException -> 
            PreserveStackTrace(exn)
            raise exn
        | _ -> ()

    type ErrorLogger with  
        member x.ErrorR  exn = match exn with StopProcessing | ReportedError -> raise exn | _ -> x.ErrorSink exn
        member x.Warning exn = match exn with StopProcessing | ReportedError -> raise exn | _ -> x.WarnSink exn
        member x.Error   exn = x.ErrorR exn; raise ReportedError
        member x.ErrorRecovery (exn:exn) (m:range) =
            // Never throws ReportedError.
            // Throws StopProcessing and exceptions raised by the ErrorSink(exn) handler.
            match exn with
            (* Don't send ThreadAbortException down the error channel *)
            | :? System.Threading.ThreadAbortException | WrappedError((:? System.Threading.ThreadAbortException),_) ->  ()
            | ReportedError  | WrappedError(ReportedError,_)  -> ()
            | StopProcessing | WrappedError(StopProcessing,_) -> raise exn
            | e ->
                try  
                    x.ErrorR (AttachRange m exn) // may raise exceptions, e.g. an fsi error sink raises StopProcessing.
                    ReraiseIfWatsonable(exn)
                with
                  | ReportedError  | WrappedError(ReportedError,_)  -> ()
        member x.StopProcessingRecovery (exn:exn) (m:range) =
            // Do standard error recovery.
            // Additionally ignore/catch StopProcessing. [This is the only catch handler for StopProcessing].
            // Additionally ignore/catch ReportedError.
            // Can throw other exceptions raised by the ErrorSink(exn) handler.         
            match exn with
            | StopProcessing | WrappedError(StopProcessing,_) -> () // suppress, so skip error recovery.
            | e ->
                try  x.ErrorRecovery exn m
                with
                  | StopProcessing | WrappedError(StopProcessing,_) -> () // catch, e.g. raised by ErrorSink.
                  | ReportedError  | WrappedError(ReportedError,_)  -> () // catch, but not expected unless ErrorRecovery is changed.
        member x.ErrorRecoveryNoRange (exn:exn) =
            x.ErrorRecovery exn range0

let  mutable private globalErrorLogger = 
     { new ErrorLogger with 
               member x.WarnSink (e:exn) = 
                   ()
                  // Ideally we would assert here and in ErrorSink, and explicitly install a GlobalErrorLogger. However 
                  // that would then mean that different threads are using the global error logger which makes 
                  // things tricky. So for the moment we jsut make the default global error logger discard errors.
                  
                  // use unwind = InstallGlobalErrorLogger (fun _ -> DiscardErrorsLogger)
                   //assert false
                   //dprintf "no warning handler installed\n" 
               member x.ErrorSink (e:exn) = 
                   ()
                   //assert false
                   //dprintf "no error handler installed\n" 
               member x.ErrorCount = 0 }

let DiscardErrorsLogger = 
     { new ErrorLogger with 
               member x.WarnSink (e:exn) =  ()
               member x.ErrorSink (e:exn) = ()
               member x.ErrorCount = 0 }
               
let InstallGlobalErrorLogger(errorLoggerTransformer) =
    let oldErrorLogger = globalErrorLogger
    globalErrorLogger <- errorLoggerTransformer oldErrorLogger
    { new System.IDisposable with 
         member x.Dispose() = globalErrorLogger <- oldErrorLogger }


// Global functions are still used by parser and TAST ops
let errorR  exn = globalErrorLogger.ErrorR exn
let warning exn = globalErrorLogger.Warning exn
let error   exn = globalErrorLogger.Error exn
let errorRecovery exn m = globalErrorLogger.ErrorRecovery exn m
let stopProcessingRecovery exn m = globalErrorLogger.StopProcessingRecovery exn m
let errorRecoveryNoRange exn = globalErrorLogger.ErrorRecoveryNoRange exn

let report f = 
    f() 

let deprecated s m = warning(Deprecated(s,m))
let deprecatedWithError s m = errorR(Deprecated(s,m))
let libraryOnlyWarning m = warning(LibraryUseOnly(m))
let deprecatedOperator m = deprecated "the treatment of this operator is now handled directly by the F# compiler and its meaning may not be redefined" m
let ocamlCompatWarning s m = warning(OCamlCompatibility(s,m))

//------------------------------------------------------------------------
// Errors as data: Sometimes we have to reify errors as data, e.g. if backtracking 
//
// REVIEW: consider using F# computation expressions here

type warning = exn
type error = exn
type OperationResult<'a> = 
    | OkResult of warning list * 'a
    | ErrorResult of warning list * error
    
type ImperativeOperationResult = OperationResult<unit>

let ReportWarnings warns = List.iter warning warns

let CommitOperationResult res = 
    match res with 
    | OkResult (warns,res) -> ReportWarnings warns; res
    | ErrorResult (warns,err) -> ReportWarnings warns; error err

let RaiseOperationResult res : unit = CommitOperationResult res

let ErrorD err = ErrorResult([],err)
let WarnD err = OkResult([err],())
let CompleteD = OkResult([],())
let ResultD x = OkResult([],x)
let CheckNoErrorsAndGetWarnings res = match res with OkResult (warns,_) -> Some warns | ErrorResult _ -> None

/// The bind in the monad. Stop on first error. Accumulate warnings and continue. 
let (++) res f = 
    match res with 
    | OkResult([],res) -> (* tailcall *) f res 
    | OkResult(warns,res) -> 
        begin match f res with 
        | OkResult(warns2,res2) -> OkResult(warns@warns2, res2)
        | ErrorResult(warns2,err) -> ErrorResult(warns@warns2, err)
        end
    | ErrorResult(warns,err) -> 
        ErrorResult(warns,err)
        
/// Stop on first error. Accumulate warnings and continue. 
let rec IterateD f xs = match xs with [] -> CompleteD | h :: t -> f h ++ (fun () -> IterateD f t)
let rec WhileD gd body = if gd() then body() ++ (fun () -> WhileD gd body) else CompleteD
let MapD f xs = let rec loop acc xs = match xs with [] -> ResultD (List.rev acc) | h :: t -> f h ++ (fun x -> loop (x::acc) t) in loop [] xs

type TrackErrorsBuilder() =
    member x.Bind(res,k) = res ++ k
    member x.Return(res) = ResultD(res)
    member x.For(seq,k) = IterateD k seq
    member x.While(gd,k) = WhileD gd k

let trackErrors = TrackErrorsBuilder()
    
/// Stop on first error. Accumulate warnings and continue. 
let OptionD f xs = match xs with None -> CompleteD | Some(h) -> f h 

/// Stop on first error. Report index 
let IterateIdxD f xs = 
    let rec loop xs i = match xs with [] -> CompleteD | h :: t -> f i h ++ (fun () -> loop t (i+1))
    loop xs 0

/// Stop on first error. Accumulate warnings and continue. 
let rec Iterate2D f xs ys = 
    match xs,ys with 
    | [],[] -> CompleteD 
    | h1 :: t1, h2::t2 -> f h1 h2 ++ (fun () -> Iterate2D f t1 t2) 
    | _ -> failwith "Iterate2D"

let TryD f g = 
    match f() with
    | ErrorResult(warns,err) ->  (OkResult(warns,())) ++ (fun () -> g err)
    | res -> res

let rec RepeatWhileD body = body() ++ (function true -> RepeatWhileD body | false -> CompleteD) 
let AtLeastOneD f l = MapD f l ++ (fun res -> ResultD (List.exists id res))

