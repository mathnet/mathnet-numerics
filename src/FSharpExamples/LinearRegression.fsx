// <copyright file="LinearRegression.fsx" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

#r "../../out/lib/Net40/MathNet.Numerics.dll"
#r "../../out/lib/Net40/MathNet.Numerics.FSharp.dll"

open System
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.Distributions

// Simple Least Squares Linear Regression, from:
// http://christoph.ruegg.name/blog/2012/9/9/linear-regression-mathnet-numerics.html

let ``Fitting to a line`` =
    printfn "Fitting to a line"

    let X = DenseMatrix.ofColumnsList 3 2 [ List.init 3 (fun i -> 1.0); [ 10.0; 20.0; 30.0 ] ]
    let y = DenseVector [| 15.0; 20.0; 25.0 |]
    let p = X.QR().Solve(y)

    printfn "X: %A" X
    printfn "y: %s" (y.ToString())
    printfn "p: %s" (p.ToString())

    (p.[0], p.[1])


let ``Fitting to an arbitrary linear function from noisy data`` =
    printfn "Fitting to an arbitrary linear function from noisy data"

    // define our target functions
    let f1 x = Math.Sqrt(Math.Exp(x))
    let f2 x = SpecialFunctions.DiGamma(x*x)

    // sample points
    let xdata = [ 1.0 .. 1.0 .. 10.0 ]

    // create data samples, with chosen parameters and with gaussian noise added
    let fy (noise:IContinuousDistribution) x = 2.5*f1(x) - 4.0*f2(x) + noise.Sample()
    let ydata = xdata |> List.map (fy (Normal.WithMeanVariance(0.0,2.0)))

    // build matrix form
    let X =
        [
            xdata |> List.map f1
            xdata |> List.map f2
        ] |> DenseMatrix.ofColumnsList 10 2
    let y = DenseVector.ofList ydata

    // solve
    let p = X.QR().Solve(y)

    printfn "X: %A" X
    printfn "y: %s" (y.ToString())
    printfn "p: %s" (p.ToString())

    (p.[0], p.[1])


let ``Fitting to an sine from noisy data`` =
    printfn "Fitting to an sine from noisy data"

    // sample points
    let omega = 1.0
    let xdata = [| -1.0; 0.0; 0.1; 0.2; 0.3; 0.4; 0.65; 1.0; 1.2; 2.1; 4.5; 5.0; 6.0; |]

    // generate noisy data for sample points
    let rnd = Random(1)
    let ydata = xdata |> Array.map (fun x -> 5.0 + 2.0*Math.Sin(omega*x + 0.2) + 2.0*(rnd.NextDouble()-0.5))

    let X = [
                Array.create xdata.Length 1.0
                xdata |> Array.map (fun x -> Math.Sin(omega*x))
                xdata |> Array.map (fun x -> Math.Cos(omega*x))
            ] |> DenseMatrix.ofColumns xdata.Length 3
    let y = DenseVector ydata

    let p = X.QR().Solve(y)
    let a = p.[0]
    let b = SpecialFunctions.Hypotenuse(p.[1], p.[2])
    let c = Math.Atan2(p.[2], p.[1])

    printfn "X: %A" X
    printfn "y: %s" (y.ToString())
    printfn "p: %s" (p.ToString())
    printfn "a: %f, b: %f, c: %f" a b c

    (a,b,c)
