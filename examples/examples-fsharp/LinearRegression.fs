module LinearRegression

open System
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.Distributions

// Simple Least Squares Linear Regression. For the general principle see
// http://christoph.ruegg.name/blog/2012/9/9/linear-regression-mathnet-numerics.html

let ``Fitting to a line`` =
    printfn "Fitting to a line "

    let offset, slope = Fit.line [| 10.0; 20.0; 30.0 |] [| 15.0; 20.0; 25.0 |]
    offset, slope


let ``Fitting to an arbitrary linear function from noisy data`` =
    printfn "Fitting to an arbitrary linear function from noisy data"

    // define our target function as linear combination of the following two arbitrary functions
    let f1 x = Math.Sqrt(Math.Exp(x))
    let f2 x = SpecialFunctions.DiGamma(x*x)

    // sample points
    let xdata = [| 1.0 .. 1.0 .. 10.0 |]

    // generate data samples with chosen parameters and with gaussian noise added
    let fy (noise:IContinuousDistribution) x = 2.5*f1(x) - 4.0*f2(x) + noise.Sample()
    let ydata = xdata |> Array.map (fy (Normal.WithMeanVariance(0.0,2.0)))

    let p = Fit.linear [f1; f2] xdata ydata
    p.[0], p.[1]


let ``Fitting to an sine from noisy data`` =
    printfn "Fitting to an sine from noisy data"

    // sample points
    let omega = 1.0
    let xdata = [| -1.0; 0.0; 0.1; 0.2; 0.3; 0.4; 0.65; 1.0; 1.2; 2.1; 4.5; 5.0; 6.0; |]

    // generate noisy data for sample points
    let rnd = Random(1)
    let ydata = xdata |> Array.map (fun x -> 5.0 + 2.0*Math.Sin(omega*x + 0.2) + 2.0*(rnd.NextDouble()-0.5))

    let p = (xdata, ydata) ||> Fit.linear [(fun _ -> 1.0); (fun z -> Math.Sin(omega*z)); (fun z -> Math.Cos(omega*z))]

    let a = p.[0]
    let b = SpecialFunctions.Hypotenuse(p.[1], p.[2])
    let c = Math.Atan2(p.[2], p.[1])

    (a,b,c)
