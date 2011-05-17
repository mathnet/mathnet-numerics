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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using NUnit.Framework;

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
            AssertHelpers.AlmostEqual(7.416198487095663, vector.Norm(2), 15);
        }

        /// <summary>
        /// Can compute 1st norm.
        /// </summary>
        [Test]
        public void CanComputeNorm1()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(15.0, vector.Norm(1), 15);
        }

        /// <summary>
        /// Can compute square norm.
        /// </summary>
        [Test]
        public void CanComputeSquareNorm()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(55.0, vector.Norm(2) * vector.Norm(2), 15);
        }

        /// <summary>
        /// Can compute a norm.
        /// </summary>
        /// <param name="p">The norm index.</param>
        /// <param name="expected">The expected <c>P</c>-norm value.</param>
        [TestCase(1, 15.0)]
        [TestCase(2, 7.416198487095663)]
        [TestCase(3, 6.0822019955734001)]
        [TestCase(10, 5.0540557845353753)]
        public void CanComputeNormP(int p, double expected)
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(expected, vector.Norm(p), 15);
        }

        /// <summary>
        /// Can compute infinity norm.
        /// </summary>
        [Test]
        public void CanComputeNormInfinity()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqual(5.0, vector.Norm(Double.PositiveInfinity), 15);
        }

        /// <summary>
        /// Can normalize a vector.
        /// </summary>
        [Test]
        public void CanNormalizeVector()
        {
            var vector = CreateVector(Data);
            var result = vector.Normalize(2);
            AssertHelpers.AlmostEqual(0.134839972492648, result[0], 14);
            AssertHelpers.AlmostEqual(0.269679944985297, result[1], 14);
            AssertHelpers.AlmostEqual(0.404519917477945, result[2], 14);
            AssertHelpers.AlmostEqual(0.539359889970594, result[3], 14);
            AssertHelpers.AlmostEqual(0.674199862463242, result[4], 14);
        }
    }
}
