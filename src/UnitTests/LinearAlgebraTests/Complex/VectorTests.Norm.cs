// <copyright file="VectorTests.Norm.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

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
            AssertHelpers.AlmostEqualRelative(7.74596669241483, vector.L2Norm(), 14);
            AssertHelpers.AlmostEqualRelative(7.74596669241483, vector.Norm(2), 14);
        }

        /// <summary>
        /// Can compute 1st norm.
        /// </summary>
        [Test]
        public void CanComputeNorm1()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqualRelative(16.0346843392517, vector.L1Norm(), 14);
            AssertHelpers.AlmostEqualRelative(16.0346843392517, vector.Norm(1), 14);
        }

        /// <summary>
        /// Can compute square norm.
        /// </summary>
        [Test]
        public void CanComputeSquareNorm()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqualRelative(60.0, vector.L2Norm() * vector.L2Norm(), 14);
            AssertHelpers.AlmostEqualRelative(60.0, vector.Norm(2) * vector.Norm(2), 14);
        }

        /// <summary>
        /// Can compute a norm.
        /// </summary>
        /// <param name="p">The norm index.</param>
        /// <param name="expected">The expected <c>P</c>-norm value.</param>
        [TestCase(1, 16.0346843392517)]
        [TestCase(2, 7.74596669241483)]
        [TestCase(3, 6.28528392332871)]
        [TestCase(10, 5.1608912235454)]
        public void CanComputeNormP(int p, double expected)
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqualRelative(expected, vector.Norm(p), 14);
        }

        /// <summary>
        /// Can compute infinity norm.
        /// </summary>
        [Test]
        public void CanComputeNormInfinity()
        {
            var vector = CreateVector(Data);
            AssertHelpers.AlmostEqualRelative(5.09901951359279, vector.InfinityNorm(), 13);
            AssertHelpers.AlmostEqualRelative(5.09901951359279, vector.Norm(double.PositiveInfinity), 13);
        }

        /// <summary>
        /// Can normalize a vector.
        /// </summary>
        [Test]
        public void CanNormalizeVector()
        {
            var vector = CreateVector(Data);
            var result = vector.Normalize(2);
            AssertHelpers.AlmostEqualRelative(new Complex(0.129099444873581, 0.129099444873581), result[0], 14);
            AssertHelpers.AlmostEqualRelative(new Complex(0.258198889747161, 0.129099444873581), result[1], 14);
            AssertHelpers.AlmostEqualRelative(new Complex(0.387298334620742, 0.129099444873581), result[2], 14);
            AssertHelpers.AlmostEqualRelative(new Complex(0.516397779494322, 0.129099444873581), result[3], 14);
            AssertHelpers.AlmostEqualRelative(new Complex(0.645497224367903, 0.129099444873581), result[4], 14);
        }
    }
}
