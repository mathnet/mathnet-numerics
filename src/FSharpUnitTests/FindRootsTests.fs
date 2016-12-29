namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open FsUnitTyped

open System
open MathNet.Numerics

module FindRootsTests =

    let f x = (x - 3.0)*(x - 4.0)
    let df x = 2.0*x - 7.0
    let g (xa:float[]) =
        let x = xa.[0];
        let T = xa.[1];
        let k = 0.12 * Math.Exp(12581.0 * (T - 298.0) / (298.0 * T));
        let fx = 120.0 * x - 75.0 * k * (1.0 - x);
        let fT = -x * (873.0 - T) + 11.0 * (T - 300.0);
        [|fx; fT|]

    [<Test>]
    let ``Bisection should find both roots of (x - 3) * (x - 4)``() =
        f |> FindRoots.bisection 100 1e-14 -5.0 3.5 |> (fun x ->
            match x with
            | Some x -> should (equalWithin 1e-14) 3.0
            | None ->   failwith "The element is not equal.") |> ignore
        f |> FindRoots.bisection 100 1e-14 3.2 5.0 |> (fun x ->
            match x with
            | Some x -> should (equalWithin 1e-14) 4.0
            | None ->   failwith "The element is not equal.") |> ignore

    [<Test>]
    let ``Brent should find both roots of (x - 3) * (x - 4)``() =
        f |> FindRoots.brent 100 1e-14 -5.0 3.5 |> shouldEqual (Some 3.0)
        f |> FindRoots.brent 100 1e-14 3.2 5.0 |> shouldEqual (Some 4.0)

    [<Test>]
    let ``Newton-Raphson should find both roots of (x - 3) * (x - 4)``() =
        (f, df) ||> FindRoots.newtonRaphson 100 1e-14 -5.0 3.5 |> shouldEqual (Some 3.0)
        (f, df) ||> FindRoots.newtonRaphson 100 1e-14 3.2 5.0 |> shouldEqual (Some 4.0)

    [<Test>]
    let ``Newton-Raphson by Guess should find both roots of (x - 3) * (x - 4)``() =
        (f, df) ||> FindRoots.newtonRaphsonGuess 100 1e-14 2.8 |> shouldEqual (Some 3.0)
        (f, df) ||> FindRoots.newtonRaphsonGuess 100 1e-14 3.7 |> shouldEqual (Some 4.0)

    [<Test>]
    let ``Robust Newton-Raphson should find both roots of (x - 3) * (x - 4)``() =
        (f, df) ||> FindRoots.newtonRaphsonRobust 100 20 1e-14 -5.0 3.5 |> shouldEqual (Some 3.0)
        (f, df) ||> FindRoots.newtonRaphsonRobust 100 20 1e-14 3.2 5.0 |> shouldEqual (Some 4.0)

    [<Test>]
    let ``Bryoden should find both roots of (x - 3) * (x - 4) and Twoeq2``() =
        f |> (fun g (x:float[]) -> [|g x.[0]|]) |> FindRoots.broyden 100 1e-14 [|1.0;|] |> shouldEqual (Some [|3.0|])
        f |> (fun g (x:float[]) -> [|g x.[0]|]) |> FindRoots.broyden 100 1e-14 [|9.0;|] |> shouldEqual (Some [|4.0|])
        g |> FindRoots.broyden 100 1e-6 [|1.0; 400.0;|] |> (fun x ->
            match x with
            | Some x -> should (equalWithin 1e-1) [|0.9638680512795; 346.16369814640|]
            | None ->   failwith "The element in array is not equal.") |> ignore

    [<Test>]
    let ``Simple method should find both roots of (x - 3) * (x - 4)``() =
        f |> FindRoots.ofFunction -5.0 3.5 |> Option.get |> should (equalWithin 1e-8) 3.0
        f |> FindRoots.ofFunction 3.2 5.0 |> Option.get |> should (equalWithin 1e-8) 4.0
        (f, df) ||> FindRoots.ofFunctionDerivative -5.0 3.5 |> Option.get |> should (equalWithin 1e-8) 3.0
        (f, df) ||> FindRoots.ofFunctionDerivative 3.2 5.0 |> Option.get |> should (equalWithin 1e-8) 4.0

