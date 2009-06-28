// (c) Microsoft Corporation 2005-2009. 

namespace Microsoft.FSharp.Compatibility

    open System

    module Seq = 


        val generate   : opener:(unit -> 'b) -> generator:('b -> 'T option) -> closer:('b -> unit) -> seq<'T>

        [<Obsolete("This function will be removed in a future release. If necessary, take a copy of its implementation from the F# PowerPack and copy it into your application")>]
        val generate_using   : opener:(unit -> ('T :> IDisposable)) -> generator:('T -> 'b option) -> seq<'b>

        /// Return an IEnumerable that when iterated yields
        /// the given item followed by the items in the given sequence
        [<Obsolete("Use 'seq { yield x; yield! seq }' instead")>]
        val cons: 'T -> seq<'T> -> seq<'T>




        /// A synonym for Seq.zip
        [<Obsolete("The F# name for this operator is now 'zip'")>]
        val combine: seq<'T1> -> seq<'T2> -> seq<'T1 * 'T2>

        /// Return true if the IEnumerable is not empty.
        [<Obsolete("This function will be removed. Use 'not List.isEmpty' instead")>]
        val nonempty: seq<'T> -> bool
