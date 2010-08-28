// <copyright file="VectorTests.Norm.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    using System;
	using MbUnit.Framework;

    public abstract partial class VectorTests
    {
        [Test]
        public void CanComputeNorm()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(7.416198487095663f, (float)vector.Norm(2), 7);
        }

        [Test]
        public void CanComputeNorm1()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(15.0f, (float)vector.Norm(1), 7);
        }

        [Test]
        public void CanComputeSquareNorm()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(55.0f, (float)vector.Norm(2) * (float)vector.Norm(2), 7);
        }

        [Test]
        [Row(1, 15.0)]
        [Row(2, 7.416198487095663)]
        [Row(3, 6.0822019955734001)]
        [Row(10, 5.0540557845353753)]
        public void CanComputeNormP(int p, float expected)
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(expected, (float)vector.Norm(p), 7);
        }
        
        [Test]
        public void CanComputeNormInfinity()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(5.0f, (float)vector.Norm(Double.PositiveInfinity), 7);
        }

        [Test]
        [MultipleAsserts]
        public void CanNormalizeVector()
        {
            var vector = CreateVector(Data);
            var result = vector.Normalize(2);
            AssertHelpers.AlmostEqual(0.134839967f, result[0], 7);
            AssertHelpers.AlmostEqual(0.269679934f, result[1], 7);
            AssertHelpers.AlmostEqual(0.404519916f, result[2], 7);
            AssertHelpers.AlmostEqual(0.539359868f, result[3], 7);
            AssertHelpers.AlmostEqual(0.6741998f, result[4], 7);
        }
    }
}