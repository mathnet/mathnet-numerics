// <copyright file="Statistics.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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

namespace MathNet.Numerics.Statistics

open System

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Statistics =

    let private tofs (f:Func<_,_>) = fun a -> f.Invoke(a)

    let quantileFunc (data : float seq) = Statistics.QuantileFunc(data) |> tofs
    let quantileCustomFunc (data : float seq) definition = Statistics.QuantileCustomFunc(data, definition) |> tofs
    let percentileFunc (data : float seq) = Statistics.PercentileFunc(data) |> tofs
    let orderStatisticFunc (data : float seq) = Statistics.OrderStatisticFunc(data) |> tofs
    let quantileRankFunc (data : float seq) = Statistics.QuantileRankFunc(data) |> tofs
    let quantileRankCustomFunc (data : float seq) definition = Statistics.QuantileRankFunc(data, definition) |> tofs
    let empiricalCDFFunc (data : float seq) = Statistics.EmpiricalCDFFunc(data) |> tofs
    let empiricalInvCDFFunc (data : float seq) = Statistics.EmpiricalInvCDFFunc(data) |> tofs
