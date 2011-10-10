// <copyright file="VectorArithmeticTheory.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2011 Math.NET
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    [TestFixture]
    public abstract class VectorArithmeticTheory
    {
        [Theory, Timeout(100)]
        public void CanCallUnaryPlusOperatorOnVector(Vector<double> vector)
        {
            var hash = vector.GetHashCode();

            var result = +vector;

            Assert.That(vector.GetHashCode(), Is.EqualTo(hash));
            Assert.That(result, Is.Not.SameAs(vector));
            Assert.That(result, Is.EqualTo(vector));
        }

        [Theory, Timeout(100)]
        public void CanCallUnaryMinusOperatorOnVector(Vector<double> vector)
        {
            var hash = vector.GetHashCode();

            var result = -vector;

            Assert.That(vector.GetHashCode(), Is.EqualTo(hash));
            Assert.That(result, Is.Not.SameAs(vector));
            Assert.That(-result, Is.EqualTo(vector));

            for (var i = 0; i < Math.Min(vector.Count, 20); i++)
            {
                Assert.That(result[i], Is.EqualTo(-vector[i]));
            }
        }

        [Theory, Pairwise, Timeout(100)]
        public void CanAddTwoVectorsUsingOperator(Vector<double> a, Vector<double> b)
        {
            Assume.That(a.Count, Is.EqualTo(b.Count));

            var hasha = a.GetHashCode();
            var hashb = b.GetHashCode();

            var result = a + b;

            Assert.That(a.GetHashCode(), Is.EqualTo(hasha));
            Assert.That(b.GetHashCode(), Is.EqualTo(hashb));
            Assert.That(result, Is.Not.SameAs(a));
            Assert.That(result, Is.Not.SameAs(b));

            for (var i = 0; i < Math.Min(a.Count, 20); i++)
            {
                Assert.That(result[i], Is.EqualTo(a[i] + b[i]));
            }
        }

        [Theory, Pairwise, Timeout(100)]
        public void CanSubtractTwoVectorsUsingOperator(Vector<double> a, Vector<double> b)
        {
            Assume.That(a.Count, Is.EqualTo(b.Count));

            var hasha = a.GetHashCode();
            var hashb = b.GetHashCode();

            var result = a - b;

            Assert.That(a.GetHashCode(), Is.EqualTo(hasha));
            Assert.That(b.GetHashCode(), Is.EqualTo(hashb));
            Assert.That(result, Is.Not.SameAs(a));
            Assert.That(result, Is.Not.SameAs(b));

            for (var i = 0; i < Math.Min(a.Count, 20); i++)
            {
                Assert.That(result[i], Is.EqualTo(a[i] - b[i]));
            }
        }
    }
}
