// <copyright file="Fit.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Factorization

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Fit =

    let private tofs (f:Func<_,_>) = fun a -> f.Invoke(a)

    /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
    /// returning its best fitting parameters as (a, b) tuple.
    let line x y = Fit.Line(x,y) |> properTuple2

    /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
    /// returning a function y' for the best fitting line.
    let lineFunc x y = Fit.LineFunc(x,y) |> tofs

    /// Least-Squares fitting the points ((x0,x1,...,xk),y) to a linear surface y : X -> p0*x0 + p1*x1 + ... + pk*xk,
    /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array.
    let multiDim intercept x y = Fit.MultiDim(x,y,intercept)

    /// Least-Squares fitting the points ((x0,x1,...,xk),y) to a linear surface y : X -> p0*x0 + p1*x1 + ... + pk*xk,
    /// returning a function y' for the best fitting surface.
    let multiDimFunc intercept x y = Fit.MultiDimFunc(x,y,intercept) |> tofs

    /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k,
    /// returning its best fitting parameters as [p0, p1, p2, ..., pk] array, compatible with Evaluate.Polynomial.
    let polynomial order x y = Fit.Polynomial(x,y,order)

    /// Least-Squares fitting the points (x,y) to a k-order polynomial y : x -> p0 + p1*x + p2*x^2 + ... + pk*x^k,
    /// returning a function y' for the best fitting polynomial.
    let polynomialFunc order x y = Fit.PolynomialFunc(x,y,order) |> tofs

    /// Least-Squares fitting the points (x,y) to an arbitrary linear combination y : x -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
    /// returning its best fitting parameters as [p0, p1, p2, ..., pk] list.
    let linear functions (x:_[]) (y:float[]) =
        functions
        |> List.map (fun f -> List.init (Array.length x) (fun i -> f x.[i]))
        |> DenseMatrix.ofColumnList
        |> fun m -> m.QR(QRMethod.Thin).Solve(DenseVector.raw y).ToArray()
        |> List.ofArray

    /// Least-Squares fitting the points (x,y) to an arbitrary linear combination y : x -> p0*f0(x) + p1*f1(x) + ... + pk*fk(x),
    /// returning a function y' for the best fitting combination.
    let linearFunc functions x y =
        let parts = linear functions x y |> List.zip functions
        in fun z -> parts |> List.fold (fun s (f,p) -> s+p*(f z)) 0.0
