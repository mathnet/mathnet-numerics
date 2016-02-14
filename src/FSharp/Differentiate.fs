// <copyright file="Differentiate.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
open MathNet.Numerics.Differentiation

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Differentiate =

    let private tobcl (f:'a->'b) = Func<'a,'b>(f)
    let private tofs (f:Func<_,_>) = fun a -> f.Invoke(a)
    let private tofs2 (f:Func<_,_>) = fun a b -> f.Invoke([|a;b|])

    let derivative order x (f:float->float) = Differentiate.Derivative(tobcl f, x, order)
    let derivativeFunc order (f:float->float) = Differentiate.DerivativeFunc(tobcl f, order) |> tofs

    let firstDerivative x (f:float->float) = Differentiate.FirstDerivative(tobcl f, x)
    let firstDerivativeFunc (f:float->float) = Differentiate.FirstDerivativeFunc(tobcl f) |> tofs

    let secondDerivative x (f:float->float) = Differentiate.SecondDerivative(tobcl f, x)
    let secondDerivativeFunc (f:float->float) = Differentiate.SecondDerivativeFunc(tobcl f) |> tofs

    let partialDerivative order parameterIndex x (f:float[]->float) = Differentiate.PartialDerivative(tobcl f, x, parameterIndex, order)
    let partialDerivativeFunc order parameterIndex (f:float[]->float) = Differentiate.PartialDerivativeFunc(tobcl f, parameterIndex, order) |> tofs

    let firstPartialDerivative parameterIndex x (f:float[]->float) = Differentiate.FirstPartialDerivative(tobcl f, x, parameterIndex)
    let firstPartialDerivativeFunc parameterIndex (f:float[]->float) = Differentiate.FirstPartialDerivativeFunc(tobcl f, parameterIndex) |> tofs

    let partialDerivative2 order parameterIndex x (f:float->float->float) = Differentiate.PartialDerivative((fun x -> f x.[0] x.[1]), x, parameterIndex, order)
    let partialDerivative2Func order parameterIndex (f:float->float->float) = Differentiate.PartialDerivativeFunc((fun x -> f x.[0] x.[1]), parameterIndex, order) |> tofs2

    let firstPartialDerivative2 parameterIndex x (f:float->float->float) = Differentiate.FirstPartialDerivative((fun x -> f x.[0] x.[1]), x, parameterIndex)
    let firstPartialDerivative2Func parameterIndex (f:float->float->float) = Differentiate.FirstPartialDerivativeFunc((fun x -> f x.[0] x.[1]), parameterIndex) |> tofs2
