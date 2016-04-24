// <copyright file="DenseVectorTests.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using Numerics;

    /// <summary>
    /// Dense vector tests.
    /// </summary>
    public class DenseVectorTests : VectorTests
    {
        /// <summary>
        /// Creates a new instance of the Vector class.
        /// </summary>
        /// <param name="size">The size of the <strong>Vector</strong> to construct.</param>
        /// <returns>The new <c>Vector</c>.</returns>
        protected override Vector<Complex32> CreateVector(int size)
        {
            return new DenseVector(size);
        }

        /// <summary>
        /// Creates a new instance of the Vector class.
        /// </summary>
        /// <param name="data">The array to create this vector from.</param>
        /// <returns>The new <c>Vector</c>.</returns>
        protected override Vector<Complex32> CreateVector(IList<Complex32> data)
        {
            var vector = new DenseVector(data.Count);
            for (var index = 0; index < data.Count; index++)
            {
                vector[index] = data[index];
            }

            return vector;
        }

        /// <summary>
        /// Can create a dense vector form array.
        /// </summary>
        [Test]
        public void CanCreateDenseVectorFromArray()
        {
            var data = new Complex32[Data.Length];
            Array.Copy(Data, data, Data.Length);
            var vector = new DenseVector(data);

            for (var i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], vector[i]);
            }

            vector[0] = new Complex32(10.0f, 1);
            Assert.AreEqual(new Complex32(10.0f, 1), data[0]);
        }

        /// <summary>
        /// Can create a dense vector from another dense vector.
        /// </summary>
        [Test]
        public void CanCreateDenseVectorFromAnotherDenseVector()
        {
            var vector = new DenseVector(Data);
            var other = DenseVector.OfVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        /// <summary>
        /// Can create a dense vector from another vector.
        /// </summary>
        [Test]
        public void CanCreateDenseVectorFromAnotherVector()
        {
            var vector = (Vector<Complex32>) new DenseVector(Data);
            var other = DenseVector.OfVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        /// <summary>
        /// Can create a dense vector from user defined vector.
        /// </summary>
        [Test]
        public void CanCreateDenseVectorFromUserDefinedVector()
        {
            var vector = new UserDefinedVector(Data);
            var other = DenseVector.OfVector(vector);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        /// <summary>
        /// Can create a dense vector with constant values.
        /// </summary>
        [Test]
        public void CanCreateDenseVectorWithConstantValues()
        {
            var vector = DenseVector.Create(5, 5);
            foreach (var t in vector)
            {
                Assert.AreEqual(t, new Complex32(5.0f, 0));
            }
        }

        /// <summary>
        /// Can create a dense matrix.
        /// </summary>
        [Test]
        public void CanCreateDenseMatrix()
        {
            var vector = new DenseVector(3);
            var matrix = Matrix<Complex32>.Build.SameAs(vector, 2, 3);
            Assert.IsInstanceOf<DenseMatrix>(matrix);
            Assert.AreEqual(2, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
        }

        /// <summary>
        /// Can convert a dense vector to an array.
        /// </summary>
        [Test]
        public void CanConvertDenseVectorToArray()
        {
            var vector = new DenseVector(Data);
            var array = (Complex32[]) vector;
            Assert.IsInstanceOf(typeof (Complex32[]), array);
            CollectionAssert.AreEqual(vector, array);
        }

        /// <summary>
        /// Can convert an array to a dense vector.
        /// </summary>
        [Test]
        public void CanConvertArrayToDenseVector()
        {
            var array = new[] {new Complex32(1, 1), new Complex32(2, 1), new Complex32(3, 1), new Complex32(4, 1)};
            var vector = (DenseVector) array;
            Assert.IsInstanceOf(typeof (DenseVector), vector);
            CollectionAssert.AreEqual(array, array);
        }

        /// <summary>
        /// Can call unary plus operator on a vector.
        /// </summary>
        [Test]
        public void CanCallUnaryPlusOperatorOnDenseVector()
        {
            var vector = new DenseVector(Data);
            var other = +vector;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        /// <summary>
        /// Can add two dense vectors using "+" operator.
        /// </summary>
        [Test]
        public void CanAddTwoDenseVectorsUsingOperator()
        {
            var vector = new DenseVector(Data);
            var other = new DenseVector(Data);
            var result = vector + other;
            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            CollectionAssert.AreEqual(Data, other, "Making sure the original vector wasn't modified.");

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i]*2.0f, result[i]);
            }
        }

        /// <summary>
        /// Can call unary negate operator on a dense vector.
        /// </summary>
        [Test]
        public void CanCallUnaryNegationOperatorOnDenseVector()
        {
            var vector = new DenseVector(Data);
            var other = -vector;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(-Data[i], other[i]);
            }
        }

        /// <summary>
        /// Can subtract two dense vectors using "-" operator.
        /// </summary>
        [Test]
        public void CanSubtractTwoDenseVectorsUsingOperator()
        {
            var vector = new DenseVector(Data);
            var other = new DenseVector(Data);
            var result = vector - other;
            CollectionAssert.AreEqual(Data, vector, "Making sure the original vector wasn't modified.");
            CollectionAssert.AreEqual(Data, other, "Making sure the original vector wasn't modified.");

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Complex32.Zero, result[i]);
            }
        }

        /// <summary>
        /// Can multiply a dense vector by a Complex using "*" operator.
        /// </summary>
        [Test]
        public void CanMultiplyDenseVectorByComplexUsingOperators()
        {
            var vector = new DenseVector(Data);
            vector = vector*new Complex32(2.0f, 1);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i]*new Complex32(2.0f, 1), vector[i]);
            }

            vector = vector*1.0f;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i]*new Complex32(2.0f, 1), vector[i]);
            }

            vector = new DenseVector(Data);
            vector = new Complex32(2.0f, 1)*vector;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i]*new Complex32(2.0f, 1), vector[i]);
            }

            vector = 1.0f*vector;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i]*new Complex32(2.0f, 1), vector[i]);
            }
        }

        /// <summary>
        /// Can divide a dense vector by a Complex using "/" operator.
        /// </summary>
        [Test]
        public void CanDivideDenseVectorByComplexUsingOperators()
        {
            var vector = new DenseVector(Data);
            vector = vector/new Complex32(2.0f, 1);

            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AlmostEqualRelative(Data[i]/new Complex32(2.0f, 1), vector[i], 6);
            }

            vector = vector/1.0f;
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AlmostEqualRelative(Data[i]/new Complex32(2.0f, 1), vector[i], 6);
            }
        }

        /// <summary>
        /// Can calculate an outer product for a dense vector.
        /// </summary>
        [Test]
        public void CanCalculateOuterProductForDenseVector()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            var m = Vector<Complex32>.OuterProduct(vector1, vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i]*vector2[j]);
                }
            }
        }
    }
}
