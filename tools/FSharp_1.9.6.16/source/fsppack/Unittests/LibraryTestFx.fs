
module FSharp.PowerPack.LibraryTestFx

open System
open System.Collections.Generic

open NUnit.Framework

// Workaround for bug 3601, we are issuing an unnecessary warning
#nowarn "0004"

/// Check that the lamda throws an exception of the given type. Otherwise
/// calls Assert.Fail()
let private CheckThrowsExn<'a when 'a :> exn> (f : unit -> unit) =
    let funcThrowsAsExpected =
        try
            let _ = f ()
            false // Did not throw!
        with
        | :? 'a
            -> true   // Thew null ref, OK
        | _ -> false  // Did now throw a null ref exception!
    if funcThrowsAsExpected
    then ()
    else Assert.Fail()

// Illegitimate exceptions. Once we've scrubbed the library, we should add an
// attribute to flag these exception's usage as a bug.
//[<Obsolete("If a library routine throws a Failure exception that is a bug.")>]
let CheckThrowsFailureException      f = CheckThrowsExn<FailureException>         f
let CheckThrowsNullRefException      f = CheckThrowsExn<NullReferenceException>   f
let CheckThrowsIndexOutRangException f = CheckThrowsExn<IndexOutOfRangeException> f

// Legit exceptions
let CheckThrowsNotSupportedException f = CheckThrowsExn<NotSupportedException>    f
let CheckThrowsArgumentException     f = CheckThrowsExn<ArgumentException>        f
let CheckThrowsArgumentNullException f = CheckThrowsExn<ArgumentNullException>    f
let CheckThrowsKeyNotFoundException  f = CheckThrowsExn<KeyNotFoundException>     f
let CheckThrowsDivideByZeroException f = CheckThrowsExn<DivideByZeroException>    f
let CheckThrowsInvalidOperationExn   f = CheckThrowsExn<InvalidOperationException> f

// Verifies two sequences are equal (same length, equiv elements)
let VerifySeqsEqual seq1 seq2 =
    if Seq.length seq1 <> Seq.length seq2 then Assert.Fail()
    
    let zippedElements = Seq.zip seq1 seq2
    if zippedElements |> Seq.forall (fun (a, b) -> a = b) 
    then ()
    else Assert.Fail()