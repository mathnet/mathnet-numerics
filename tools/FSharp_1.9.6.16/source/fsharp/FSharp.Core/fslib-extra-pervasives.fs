//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

module Microsoft.FSharp.Core.ExtraTopLevelOperators

open System
open System.Collections.Generic
open System.IO
open System.Diagnostics
open Microsoft.FSharp
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.Operators
open Microsoft.FSharp.Text
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Control
open Microsoft.FSharp.Primitives.Basics

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let seq (x : seq<_>) = (x :> seq<_>)

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let set l = Collections.Set.of_seq l

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let dict l = 
    // Use a dictionary (this requires hashing and equality on the key type)
    // Wrap keys in an Some(_) option in case they are null (when System.Collections.Generic.Dictionary fails). Sad but true.
    let t = new Dictionary<Option<_>,_>(HashIdentity.Structural)
    for (k,v) in l do 
        t.[Some(k)] <- v
    let d = (t :> IDictionary<_,_>)
    let c = (t :> ICollection<_>)
    let ieg = (t :> IEnumerable<_>)
    let ie = (t :> System.Collections.IEnumerable)
    // Give a read-only view of the dictionary
    { new IDictionary<'key, 'a> with 
            member s.Item 
                with get x = d.[Some(x)]            
                and  set (x,v) = raise (NotSupportedException("This value may not be mutated"))
            member s.Keys = 
                let keys = d.Keys
                { new ICollection<'key> with 
                      member s.Add(x) = raise (NotSupportedException("This value may not be mutated"));
                      member s.Clear() = raise (NotSupportedException("This value may not be mutated"));
                      member s.Remove(x) = raise (NotSupportedException("This value may not be mutated"));
                      member s.Contains(x) = keys.Contains(Some(x))
                      member s.CopyTo(arr,i) = 
                          let mutable n = 0 
                          for k in keys do 
                              arr.[i+n] <- k.Value
                              n <- n + 1
                      member s.IsReadOnly = true
                      member s.Count = keys.Count
                  interface IEnumerable<'key> with
                        member s.GetEnumerator() = (keys |> Seq.map (fun v -> v.Value)).GetEnumerator()
                  interface System.Collections.IEnumerable with
                        member s.GetEnumerator() = ((keys |> Seq.map (fun v -> v.Value)) :> System.Collections.IEnumerable).GetEnumerator() }
                
            member s.Values = d.Values
            member s.Add(k,v) = raise (NotSupportedException("This value may not be mutated"))
            member s.ContainsKey(k) = d.ContainsKey(Some(k))
            member s.TryGetValue(k,r) = 
                let key = Some(k)
                if d.ContainsKey(key) then (r <- d.[key]; true) else false
            member s.Remove(k : 'key) = (raise (NotSupportedException("This value may not be mutated")) : bool) 
      interface ICollection<KeyValuePair<'key, 'a>> with 
            member s.Add(x) = raise (NotSupportedException("This value may not be mutated"));
            member s.Clear() = raise (NotSupportedException("This value may not be mutated"));
            member s.Remove(x) = raise (NotSupportedException("This value may not be mutated"));
            member s.Contains(KeyValue(k,v)) = c.Contains(KeyValuePair<_,_>(Some(k),v))
            member s.CopyTo(arr,i) = 
                let mutable n = 0 
                for (KeyValue(k,v)) in c do 
                    arr.[i+n] <- KeyValuePair<_,_>(k.Value,v)
                    n <- n + 1
            member s.IsReadOnly = true
            member s.Count = c.Count
      interface IEnumerable<KeyValuePair<'key, 'a>> with
            member s.GetEnumerator() = 
                (c |> Seq.map (fun (KeyValue(k,v)) -> KeyValuePair<_,_>(k.Value,v))).GetEnumerator()
      interface System.Collections.IEnumerable with
            member s.GetEnumerator() = 
                ((c |> Seq.map (fun (KeyValue(k,v)) -> KeyValuePair<_,_>(k.Value,v))) :> System.Collections.IEnumerable).GetEnumerator() }

// --------------------------------------------------------------------
// Printf
// -------------------------------------------------------------------- 

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let sprintf     fp = Printf.sprintf     fp

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let failwithf   fp = Printf.failwithf   fp

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let fprintf (os:TextWriter)  fp = Printf.fprintf os  fp 

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let printf      fp = Printf.printf      fp 

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let eprintf     fp = Printf.eprintf     fp 

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let fprintfn (os:TextWriter) fp = Printf.fprintfn os fp 

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let printfn     fp = Printf.printfn     fp 

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let eprintfn    fp = Printf.eprintfn    fp 


[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let failwith s = raise (Failure s)

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
let output_any (oc: System.IO.TextWriter) x = Printf.fprintf oc "%A" x

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
let any_to_string x = Printf.sprintf "%A" x

[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
let print_any x = Printf.printf "%A" x


[<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly");  CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
let async = new Microsoft.FSharp.Control.AsyncBuilder()

#if FX_MINIMAL_REFLECTION // not on Compact Framework 
#else
let (~%) (x:Microsoft.FSharp.Quotations.Expr<'a>) : 'a = failwith "first class uses of '%' are not permitted"

let (~%%) (x: Microsoft.FSharp.Quotations.Expr) : 'a = failwith "first class uses of '%' are not permitted"
#endif

[<assembly: AutoOpen("Microsoft.FSharp")>]
[<assembly: AutoOpen("Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators")>]
[<assembly: AutoOpen("Microsoft.FSharp.Core.Operators")>]
[<assembly: AutoOpen("Microsoft.FSharp.Core")>]
[<assembly: AutoOpen("Microsoft.FSharp.Collections")>]
[<assembly: AutoOpen("Microsoft.FSharp.Control")>]
[<assembly: AutoOpen("Microsoft.FSharp.Text")>]
[<assembly: AutoOpen("Microsoft.FSharp.Core.ExtraTopLevelOperators")>]
do()

let Array2 = 1
let Array3 = 1
let IEvent = 1

let (|Lazy|) (x:Lazy<_>) = x.SynchronizedForce()
