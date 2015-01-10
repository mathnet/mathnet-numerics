// <copyright file="FindRoots.fs" company="Math.NET">
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

namespace MathNet.Numerics

open System
open MathNet.Numerics.RootFinding

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FindRoots =

    let private tobcl (f:'a->'b) = Func<'a,'b>(f)
    let private (|>|) option orElse = match option with | Some x -> Some x | None -> orElse()

    // direct algorithms

    let bisection maxIterations accuracy lowerBound upperBound (f:float->float) =
        match Bisection.TryFindRoot(tobcl f, lowerBound, upperBound, accuracy, maxIterations) with
        | true, root -> Some root
        | false, _ -> None

    let brent maxIterations accuracy lowerBound upperBound (f:float->float) =
        match Brent.TryFindRoot(tobcl f, lowerBound, upperBound, accuracy, maxIterations) with
        | true, root -> Some root
        | false, _ -> None

    let newtonRaphson maxIterations accuracy lowerBound upperBound (f:float->float) (df:float->float) =
        match NewtonRaphson.TryFindRoot(tobcl f, tobcl df, 0.5 * (lowerBound + upperBound), lowerBound, upperBound, accuracy, maxIterations) with
        | true, root -> Some root
        | false, _ -> None

    let newtonRaphsonGuess maxIterations accuracy guess (f:float->float) (df:float->float) =
        match NewtonRaphson.TryFindRoot(tobcl f, tobcl df, guess, Double.MinValue, Double.MaxValue, accuracy, maxIterations) with
        | true, root -> Some root
        | false, _ -> None

    let newtonRaphsonRobust maxIterations subdivision accuracy lowerBound upperBound (f:float->float) (df:float->float) =
        match RobustNewtonRaphson.TryFindRoot(tobcl f, tobcl df, lowerBound, upperBound, accuracy, maxIterations, subdivision) with
        | true, root -> Some root
        | false, _ -> None

    let broyden maxIterations accuracy guess (f:float[]->float[]) =
        match Broyden.TryFindRoot(tobcl f, guess, accuracy, maxIterations) with
        | true, root -> Some root
        | false, _ -> None

    // simple usage

    let ofFunction lowerBound upperBound (f:float->float) =
        brent 100 1e-8 lowerBound upperBound f
        |>| fun () -> bisection 100 1e-8 lowerBound upperBound f

    let ofFunctionDerivative lowerBound upperBound (f:float->float) (df:float->float) =
        newtonRaphsonRobust 100 20 1e-8 lowerBound upperBound f df
        |>| fun () -> bisection 100 1e-8 lowerBound upperBound f
