// (c) Microsoft Corporation 2005-2009. 

#nowarn "9"
   
namespace Microsoft.FSharp.Compatibility

open System.Collections.Generic

module Seq = 
    let combine     ie1 ie2  = Seq.zip ie1 ie2
    let nonempty (ie : seq<'T>)  = use e = ie.GetEnumerator() in e.MoveNext()

    let generate openf compute closef = 
        seq { let r = openf() 
              try 
                let x = ref None
                while (x := compute r; (!x).IsSome) do
                    yield (!x).Value
              finally
                 closef r }
    
    let generate_using (openf : unit -> ('b :> System.IDisposable)) compute = 
        generate openf compute (fun (s:'b) -> s.Dispose())

    let cons (x:'T) (s: seq<'T>) = 
        seq { yield x
              yield! s }

