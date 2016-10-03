// First version copied from the F# Power Pack
// https://raw.github.com/fsharp/powerpack/master/src/FSharp.PowerPack.Unittests/Utilities.fs

namespace MathNet.Numerics.Tests
open NUnit.Framework
open System
open System.Collections.Generic

[<AutoOpen>]
module Utilities =
    let test msg (b:bool) = Assert.IsTrue(b, "MiniTest '" + msg + "'")
    let logMessage msg = ()
//        System.Console.WriteLine("LOG:" + msg)
//        System.Diagnostics.Trace.WriteLine("LOG:" + msg)
    let check msg v1 v2 = test msg (v1 = v2)
    let reportFailure msg = Assert.Fail msg
    let numActiveEnumerators = ref 0
    let throws f = try f() |> ignore; false with e -> true

    let countEnumeratorsAndCheckedDisposedAtMostOnceAtEnd (seq: seq<'a>) =
       let enumerator() =
                 numActiveEnumerators := !numActiveEnumerators + 1;
                 let disposed = ref false in
                 let endReached = ref false in
                 let ie = seq.GetEnumerator() in
                 { new System.Collections.Generic.IEnumerator<'a> with
                      member x.Current =
                          test "rvlrve0" (not !endReached);
                          test "rvlrve1" (not !disposed);
                          ie.Current
                      member x.Dispose() =
                          test "rvlrve2" !endReached;
                          test "rvlrve4" (not !disposed);
                          numActiveEnumerators := !numActiveEnumerators - 1;
                          disposed := true;
                          ie.Dispose()
                   interface System.Collections.IEnumerator with
                      member x.MoveNext() =
                          test "rvlrve0" (not !endReached);
                          test "rvlrve3" (not !disposed);
                          endReached := not (ie.MoveNext());
                          not !endReached
                      member x.Current =
                          test "qrvlrve0" (not !endReached);
                          test "qrvlrve1" (not !disposed);
                          box ie.Current
                      member x.Reset() =
                          ie.Reset()
                   } in

       { new seq<'a> with
             member x.GetEnumerator() =  enumerator()
         interface System.Collections.IEnumerable with
             member x.GetEnumerator() =  (enumerator() :> _) }

    let countEnumeratorsAndCheckedDisposedAtMostOnce (seq: seq<'a>) =
       let enumerator() =
                 let disposed = ref false in
                 let endReached = ref false in
                 let ie = seq.GetEnumerator() in
                 numActiveEnumerators := !numActiveEnumerators + 1;
                 { new System.Collections.Generic.IEnumerator<'a> with
                      member x.Current =
                          test "qrvlrve0" (not !endReached);
                          test "qrvlrve1" (not !disposed);
                          ie.Current
                      member x.Dispose() =
                          test "qrvlrve4" (not !disposed);
                          numActiveEnumerators := !numActiveEnumerators - 1;
                          disposed := true;
                          ie.Dispose()
                   interface System.Collections.IEnumerator with
                      member x.MoveNext() =
                          test "qrvlrve0" (not !endReached);
                          test "qrvlrve3" (not !disposed);
                          endReached := not (ie.MoveNext());
                          not !endReached
                      member x.Current =
                          test "qrvlrve0" (not !endReached);
                          test "qrvlrve1" (not !disposed);
                          box ie.Current
                      member x.Reset() =
                          ie.Reset()
                   } in

       { new seq<'a> with
             member x.GetEnumerator() =  enumerator()
         interface System.Collections.IEnumerable with
             member x.GetEnumerator() =  (enumerator() :> _) }

    // Verifies two sequences are equal (same length, equiv elements)
    let verifySeqsEqual seq1 seq2 =
        if Seq.length seq1 <> Seq.length seq2 then Assert.Fail()

        let zippedElements = Seq.zip seq1 seq2
        if zippedElements |> Seq.forall (fun (a, b) -> a = b)
        then ()
        else Assert.Fail()

    /// Check that the lamda throws an exception of the given type. Otherwise
    /// calls Assert.Fail()
    let private checkThrowsExn<'a when 'a :> exn> (f : unit -> unit) =
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
    let checkThrowsNullRefException      f = checkThrowsExn<NullReferenceException>   f
    let checkThrowsIndexOutRangException f = checkThrowsExn<IndexOutOfRangeException> f

    // Legit exceptions
    let checkThrowsNotSupportedException f = checkThrowsExn<NotSupportedException>    f
    let checkThrowsArgumentException     f = checkThrowsExn<ArgumentException>        f
    let checkThrowsArgumentNullException f = checkThrowsExn<ArgumentNullException>    f
    let checkThrowsKeyNotFoundException  f = checkThrowsExn<KeyNotFoundException>     f
    let checkThrowsDivideByZeroException f = checkThrowsExn<DivideByZeroException>    f
    let checkThrowsInvalidOperationExn   f = checkThrowsExn<InvalidOperationException> f
