namespace MathNet.Numerics.Tests

open NUnit.Framework
open NUnitLite
open System.Reflection

type T = { Dummy : string }

module Program =

    [<EntryPoint>]
    let main args =
        (new AutoRun(typeof<T>.GetTypeInfo().Assembly)).Execute(args)
