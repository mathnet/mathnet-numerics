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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    using System;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    [TestFixture]
    public abstract class VectorArithmeticTheory<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        protected abstract T Minus(T value);
        protected abstract T Add(T first, T second);
        private T Subtract(T first, T second) { return Add(first, Minus(second)); }

        [Theory, Timeout(200)]
        public void CanEqualVector(Vector<T> vector, T scalar)
        {
            Assert.That(vector.Equals(vector));

            var a = vector.Clone();
            Assert.That(a, Is.Not.SameAs(vector));
            Assert.That(a.Equals(vector), Is.True);

            var a0 = a.At(0);
            var a0Equals = a0.Equals(scalar);
            a.At(0, scalar);
            Assert.That(a.Equals(vector), Is.EqualTo(a0Equals));
            Assert.That(vector.Equals(a), Is.EqualTo(a0Equals));

            var b = vector.Clone();
            var b1 = b.At(1);
            var b1Equals = b1.Equals(scalar);
            b.At(1, scalar);
            Assert.That(b.Equals(vector), Is.EqualTo(b1Equals));
            Assert.That(vector.Equals(b), Is.EqualTo(b1Equals));

            Assert.That(a.Equals(b), Is.EqualTo(a0Equals && b1Equals));

            a.At(0, a0);
            Assert.That(a.Equals(vector), Is.True);
            Assert.That(vector.Equals(a), Is.True);

            var c = vector.Clone();
            c.Subtract(vector, c);
            Assert.That(c.Equals(vector.CreateVector(vector.Count)));
        }

        [Theory, Timeout(200)]
        public void CanNegateVector(Vector<T> vector)
        {
            var hash = vector.GetHashCode();

            var result1 = -vector;
            var result2 = vector.Negate();

            Assert.That(vector.GetHashCode(), Is.EqualTo(hash));
            Assert.That(result1, Is.Not.SameAs(vector));
            Assert.That((-result1).Equals(vector));
            Assert.That(result2, Is.Not.SameAs(vector));
            Assert.That((-result2).Equals(vector));
            Assert.That(result1.Equals(result2));

            for (var i = 0; i < Math.Min(vector.Count, 20); i++)
            {
                Assert.That(result1[i], Is.EqualTo(Minus(vector[i])), i.ToString());
                Assert.That(result2[i], Is.EqualTo(Minus(vector[i])), i.ToString());
            }
        }

        [Theory, Timeout(200)]
        public void CanAddTwoVectors(Vector<T> a, Vector<T> b)
        {
            Assume.That(a.Count, Is.EqualTo(b.Count));

            var hasha = a.GetHashCode();
            var hashb = b.GetHashCode();

            var result1 = a + b;
            var result2 = a.Add(b);
            var result3 = a.Clone();
            result3.Add(b, result3);

            Assert.That(a.GetHashCode(), Is.EqualTo(hasha));
            Assert.That(b.GetHashCode(), Is.EqualTo(hashb));
            Assert.That(result1, Is.Not.SameAs(a));
            Assert.That(result1, Is.Not.SameAs(b));
            Assert.That(result2, Is.Not.SameAs(a));
            Assert.That(result2, Is.Not.SameAs(b));
            Assert.That(result3, Is.Not.SameAs(a));
            Assert.That(result3, Is.Not.SameAs(b));
            Assert.That(result1.Equals(result2));
            Assert.That(result1.Equals(result3));

            for (var i = 0; i < Math.Min(a.Count, 20); i++)
            {
                Assert.That(result1[i], Is.EqualTo(Add(a[i], b[i])), i.ToString());
                Assert.That(result2[i], Is.EqualTo(Add(a[i], b[i])), i.ToString());
                Assert.That(result3[i], Is.EqualTo(Add(a[i], b[i])), i.ToString());
            }
        }

        [Theory, Timeout(200)]
        public void CanAddScalarToVector(Vector<T> vector, T scalar)
        {
            Assume.That(vector.Count, Is.LessThan(100));

            var hash = vector.GetHashCode();

            var result1 = vector.Add(scalar);
            var result2 = vector.Clone();
            result2.Add(scalar, result2);

            Assert.That(vector.GetHashCode(), Is.EqualTo(hash));
            Assert.That(result1, Is.Not.SameAs(vector));
            Assert.That(result2, Is.Not.SameAs(vector));
            Assert.That(result1.Equals(result2));

            for (var i = 0; i < Math.Min(vector.Count, 20); i++)
            {
                Assert.That(result1[i], Is.EqualTo(Add(vector[i], scalar)), i.ToString());
                Assert.That(result2[i], Is.EqualTo(Add(vector[i], scalar)), i.ToString());
            }
        }

        [Theory, Timeout(200)]
        public void CanSubtractTwoVectors(Vector<T> a, Vector<T> b)
        {
            Assume.That(a.Count, Is.EqualTo(b.Count));

            var hasha = a.GetHashCode();
            var hashb = b.GetHashCode();

            var result1 = a - b;
            var result2 = a.Subtract(b);
            var result3 = a.Clone();
            result3.Subtract(b, result3);

            Assert.That(a.GetHashCode(), Is.EqualTo(hasha));
            Assert.That(b.GetHashCode(), Is.EqualTo(hashb));
            Assert.That(result1, Is.Not.SameAs(a));
            Assert.That(result1, Is.Not.SameAs(b));
            Assert.That(result2, Is.Not.SameAs(a));
            Assert.That(result2, Is.Not.SameAs(b));
            Assert.That(result3, Is.Not.SameAs(a));
            Assert.That(result3, Is.Not.SameAs(b));
            Assert.That(result1.Equals(result2));
            // Assert.That(result1.Equals(result3)); BUG GH-21

            for (var i = 0; i < Math.Min(a.Count, 20); i++)
            {
                Assert.That(result1[i], Is.EqualTo(Subtract(a[i], b[i])), i.ToString());
                Assert.That(result2[i], Is.EqualTo(Subtract(a[i], b[i])), i.ToString());
                Assert.That(result3[i], Is.EqualTo(Subtract(a[i], b[i])), i.ToString());
            }
        }

        [Theory, Timeout(200)]
        public void CanSubtractScalarFromVector(Vector<T> vector, T scalar)
        {
            Assume.That(vector.Count, Is.LessThan(100));

            var hash = vector.GetHashCode();

            var result1 = vector.Subtract(scalar);
            var result2 = vector.Clone();
            result2.Subtract(scalar, result2);

            Assert.That(vector.GetHashCode(), Is.EqualTo(hash));
            Assert.That(result1, Is.Not.SameAs(vector));
            Assert.That(result2, Is.Not.SameAs(vector));
            Assert.That(result1.Equals(result2));

            for (var i = 0; i < Math.Min(vector.Count, 20); i++)
            {
                Assert.That(result1[i], Is.EqualTo(Subtract(vector[i], scalar)), i.ToString());
                Assert.That(result2[i], Is.EqualTo(Subtract(vector[i], scalar)), i.ToString());
            }
        }
    }
}
