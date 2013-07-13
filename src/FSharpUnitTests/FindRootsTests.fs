namespace MathNet.Numerics.Tests

open System
open MathNet.Numerics
open NUnit.Framework
open FsUnit

module FindRootsTests =

    let f x = (x - 3.0)*(x - 4.0)
    let df x = 2.0*x - 7.0

    [<Test>]
    let ``Bisection should find both roots of (x - 3) * (x - 4)``() =
        f |> FindRoots.bisection 100 1e-14 -5.0 3.5 |> should equal (Some 3.0)
        f |> FindRoots.bisection 100 1e-14 3.2 5.0 |> should equal (Some 4.0)

    [<Test>]
    let ``Brent should find both roots of (x - 3) * (x - 4)``() =
        f |> FindRoots.brent 100 1e-14 -5.0 3.5 |> should equal (Some 3.0)
        f |> FindRoots.brent 100 1e-14 3.2 5.0 |> should equal (Some 4.0)

    [<Test>]
    let ``Newton-Raphson should find both roots of (x - 3) * (x - 4)``() =
        (f, df) ||> FindRoots.newtonRaphson 100 1e-14 -5.0 3.5 |> should equal (Some 3.0)
        (f, df) ||> FindRoots.newtonRaphson 100 1e-14 3.2 5.0 |> should equal (Some 4.0)

    [<Test>]
    let ``Newton-Raphson by Guess should find both roots of (x - 3) * (x - 4)``() =
        (f, df) ||> FindRoots.newtonRaphsonGuess 100 1e-14 2.8 |> should equal (Some 3.0)
        (f, df) ||> FindRoots.newtonRaphsonGuess 100 1e-14 3.7 |> should equal (Some 4.0)
        
    [<Test>]
    let ``Robust Newton-Raphson should find both roots of (x - 3) * (x - 4)``() =
        (f, df) ||> FindRoots.newtonRaphsonRobust 100 20 1e-14 -5.0 3.5 |> should equal (Some 3.0)
        (f, df) ||> FindRoots.newtonRaphsonRobust 100 20 1e-14 3.2 5.0 |> should equal (Some 4.0)

    [<Test>]
    let ``Simple method should find both roots of (x - 3) * (x - 4)``() =
        f |> FindRoots.ofFunction -5.0 3.5 |> Option.get |> should (equalWithin 1e-8) 3.0
        f |> FindRoots.ofFunction 3.2 5.0 |> Option.get |> should (equalWithin 1e-8) 4.0
        (f, df) ||> FindRoots.ofFunctionAndDerivative -5.0 3.5 |> Option.get |> should (equalWithin 1e-8) 3.0
        (f, df) ||> FindRoots.ofFunctionAndDerivative 3.2 5.0 |> Option.get |> should (equalWithin 1e-8) 4.0
