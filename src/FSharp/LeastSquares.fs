// <copyright file="LeastSquares.fs" company="Math.NET">
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
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic.Factorization

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Fit =

    let private tofs (f:Func<_,_>) = fun a -> f.Invoke(a)

    let line x y = let p = LeastSquares.FitToLine(x,y) in (p.[0],p.[1])
    let linef x y = LeastSquares.FitToLineFunc(x,y) |> tofs

    let polynomial order x y = LeastSquares.FitToPolynomial(x,y,order)
    let polynomialf order x y = LeastSquares.FitToPolynomialFunc(x,y,order) |> tofs

    let linear functions (x:float[]) (y:float[]) =
        functions
        |> List.map (fun f -> List.init (Array.length x) (fun i -> f x.[i]))
        |> DenseMatrix.ofColumnsList (Array.length x) (List.length functions)
        |> fun m -> m.QR(QRMethod.Thin).Solve(DenseVector(y)).ToArray()
        |> List.ofArray
    let linearf functions x y =
        let parts = linear functions x y |> List.zip functions
        in fun z -> parts |> List.fold (fun s (f,p) -> s+p*(f z)) 0.0
