namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnit
open FsUnitTyped

open System
open MathNet.Numerics

module FitTests =

    [<Test>]
    let ``When fitting to an exact line should return exact parameters``() =
        let f z = 4.0 - 1.5*z
        let x = Array.append [| 1.0 .. 2.0 .. 10.0 |] [| -1.0 .. -1.0 .. -5.0  |]
        let y = x |> Array.map f

        // LeastSquares.FitToLine(x,y)
        let a, b = Fit.line x y
        a |> should (equalWithin 1.0e-12) 4.0
        b |> should (equalWithin 1.0e-12) -1.5

        let fres = Fit.lineFunc x y
        in x |> Array.iter (fun x -> fres x |> should (equalWithin 1.0e-12) (f x))

    [<Test>]
    let ``Can fit to arbitrary linear combination``() =

        // Mathematica: Fit[{{1,4.986},{2,2.347},{3,2.061},{4,-2.995},{5,-2.352},{6,-5.782}}, {1, sin(x), cos(x)}, x]
        // -> 4.02159 sin(x) - 1.46962 cos(x) - 0.287476

        let x = [| 1.0 .. 6.0 |]
        let y = [| 4.986; 2.347; 2.061; -2.995; -2.352; -5.782 |]

        // LeastSquares.FitToLinearCombination(x, y, (fun z -> 1.0), (fun z -> Math.Sin(z)), (fun z -> Math.Cos(z)))
        let [a;b;c] = (x,y) ||> Fit.linear [(fun _ -> 1.0); (Math.Sin); (Math.Cos)]
        a |> should (equalWithin 1.0e-4) -0.287476
        b |> should (equalWithin 1.0e-4) 4.02159
        c |> should (equalWithin 1.0e-4) -1.46962

        let fres = Fit.linearFunc [(fun z -> 1.0); (fun z -> Math.Sin(z)); (fun z -> Math.Cos(z))] x y
        in x |> Array.iter (fun x -> fres x |> should (equalWithin 1.0e-4) (4.02159*Math.Sin(x) - 1.46962*Math.Cos(x) - 0.287476))
