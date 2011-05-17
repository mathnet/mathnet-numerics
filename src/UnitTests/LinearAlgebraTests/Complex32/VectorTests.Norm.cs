// <copyright file="VectorTests.Norm.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using System;
    using NUnit.Framework;
    using Complex32 = Numerics.Complex32;

    /// <summary>
    /// Abstract class with the norms set of vector tests.
    /// </summary>
    public abstract partial class VectorTests
    {
        /// <summary>
        /// Can compute 2nd norm.
        /// </summary>
        [Test]
        public void CanComputeNorm()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(7.7459666f, vector.Norm(2).Real, 7);
        }

        /// <summary>
        /// Can compute 1st norm.
        /// </summary>
        [Test]
        public void CanComputeNorm1()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(16.0346843f, vector.Norm(1).Real, 7);
        }

        /// <summary>
        /// Can compute square norm.
        /// </summary>
        [Test]
        public void CanComputeSquareNorm()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(60f, vector.Norm(2).Real * vector.Norm(2).Real, 6);
        }

        /// <summary>
        /// Can compute a norm.
        /// </summary>
        /// <param name="p">The norm index.</param>
        /// <param name="expected">The expected <c>P</c>-norm value.</param>
        [TestCase(1, 16.0346843392517f)]
        [TestCase(2, 7.74596669241483f)]
        [TestCase(3, 6.28528392332871f)]
        [TestCase(10, 5.1608912235454f)]
        public void CanComputeNormP(int p, float expected)
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(expected, vector.Norm(p).Real, 5);
        }

        /// <summary>
        /// Can compute infinity norm.
        /// </summary>
        [Test]
        public void CanComputeNormInfinity()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(5.0990195, vector.Norm(Single.PositiveInfinity).Real, 7);
        }

        /// <summary>
        /// Can normalize a vector.
        /// </summary>
        [Test]
        public void CanNormalizeVector()
        {
            var vector = CreateVector(Data);
            var result = vector.Normalize(2);
            AssertHelpers.AlmostEqual(new Complex32(0.12909944f, 0.12909944f), result[0], 7);
            AssertHelpers.AlmostEqual(new Complex32(0.25819888f, 0.12909944f), result[1], 7);
            AssertHelpers.AlmostEqual(new Complex32(0.38729833f, 0.12909944f), result[2], 7);
            AssertHelpers.AlmostEqual(new Complex32(0.51639777f, 0.12909944f), result[3], 7);
            AssertHelpers.AlmostEqual(new Complex32(0.64549722f, 0.12909944f), result[4], 7);
        }
    }
}
