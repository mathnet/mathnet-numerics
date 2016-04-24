// <copyright file="SparseVectorTest.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Storage;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using Numerics;

    /// <summary>
    /// Sparse vector tests.
    /// </summary>
    public class SparseVectorTest : VectorTests
    {
        /// <summary>
        /// Creates a new instance of the Vector class.
        /// </summary>
        /// <param name="size">The size of the <strong>Vector</strong> to construct.</param>
        /// <returns>The new <c>Vector</c>.</returns>
        protected override Vector<Complex32> CreateVector(int size)
        {
            return new SparseVector(size);
        }

        /// <summary>
        /// Creates a new instance of the Vector class.
        /// </summary>
        /// <param name="data">The array to create this vector from.</param>
        /// <returns>The new <c>Vector</c>.</returns>
        protected override Vector<Complex32> CreateVector(IList<Complex32> data)
        {
            var vector = new SparseVector(data.Count);
            for (var index = 0; index < data.Count; index++)
            {
                vector[index] = data[index];
            }

            return vector;
        }

        /// <summary>
        /// Can create a sparse vector form array.
        /// </summary>
        [Test]
        public void CanCreateSparseVectorFromArray()
        {
            var data = new Complex32[Data.Length];
            Array.Copy(Data, data, Data.Length);
            var vector = SparseVector.OfEnumerable(data);

            for (var i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], vector[i]);
            }
        }

        /// <summary>
        /// Can create a sparse vector from another sparse vector.
        /// </summary>
        [Test]
        public void CanCreateSparseVectorFromAnotherSparseVector()
        {
            var vector = SparseVector.OfEnumerable(Data);
            var other = SparseVector.OfVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        /// <summary>
        /// Can create a sparse vector from another vector.
        /// </summary>
        [Test]
        public void CanCreateSparseVectorFromAnotherVector()
        {
            var vector = (Vector<Complex32>) SparseVector.OfEnumerable(Data);
            var other = SparseVector.OfVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        /// <summary>
        /// Can create a sparse vector from user defined vector.
        /// </summary>
        [Test]
        public void CanCreateSparseVectorFromUserDefinedVector()
        {
            var vector = new UserDefinedVector(Data);
            var other = SparseVector.OfVector(vector);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        /// <summary>
        /// Can create a sparse matrix.
        /// </summary>
        [Test]
        public void CanCreateSparseMatrix()
        {
            var vector = new SparseVector(3);
            var matrix = Matrix<Complex32>.Build.SameAs(vector, 2, 3);
            Assert.IsInstanceOf<SparseMatrix>(matrix);
            Assert.AreEqual(2, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
        }

        /// <summary>
        /// Can convert a sparse vector to an array.
        /// </summary>
        [Test]
        public void CanConvertSparseVectorToArray()
        {
            var vector = SparseVector.OfEnumerable(Data);
            var array = vector.ToArray();
            Assert.IsInstanceOf(typeof (Complex32[]), array);
            CollectionAssert.AreEqual(vector, array);
        }

        /// <summary>
        /// Can convert an array to a sparse vector.
        /// </summary>
        [Test]
        public void CanConvertArrayToSparseVector()
        {
            var array = new[] {new Complex32(1, 1), new Complex32(2, 1), new Complex32(3, 1), new Complex32(4, 1)};
            var vector = SparseVector.OfEnumerable(array);
            Assert.IsInstanceOf(typeof (SparseVector), vector);
            CollectionAssert.AreEqual(array, array);
        }

        /// <summary>
        /// Can multiply a sparse vector by a Complex using "*" operator.
        /// </summary>
        [Test]
        public void CanMultiplySparseVectorByComplexUsingOperators()
        {
            var vector = SparseVector.OfEnumerable(Data);
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

            vector = SparseVector.OfEnumerable(Data);
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
        /// Can divide a sparse vector by a Complex using "/" operator.
        /// </summary>
        [Test]
        public void CanDivideSparseVectorByComplexUsingOperators()
        {
            var vector = SparseVector.OfEnumerable(Data);
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
        /// Can calculate an outer product for a sparse vector.
        /// </summary>
        [Test]
        public void CanCalculateOuterProductForSparseVector()
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

        /// <summary>
        /// Check sparse mechanism by setting values.
        /// </summary>
        [Test]
        public void CheckSparseMechanismBySettingValues()
        {
            var vector = new SparseVector(10000);
            var storage = (SparseVectorStorage<Complex32>) vector.Storage;

            // Add non-zero elements
            vector[200] = new Complex32(1.5f, 1);
            Assert.AreEqual(new Complex32(1.5f, 1), vector[200]);
            Assert.AreEqual(1, storage.ValueCount);

            vector[500] = new Complex32(3.5f, 1);
            Assert.AreEqual(new Complex32(3.5f, 1), vector[500]);
            Assert.AreEqual(2, storage.ValueCount);

            vector[800] = new Complex32(5.5f, 1);
            Assert.AreEqual(new Complex32(5.5f, 1), vector[800]);
            Assert.AreEqual(3, storage.ValueCount);

            vector[0] = new Complex32(7.5f, 1);
            Assert.AreEqual(new Complex32(7.5f, 1), vector[0]);
            Assert.AreEqual(4, storage.ValueCount);

            // Remove non-zero elements
            vector[200] = Complex32.Zero;
            Assert.AreEqual(Complex32.Zero, vector[200]);
            Assert.AreEqual(3, storage.ValueCount);

            vector[500] = Complex32.Zero;
            Assert.AreEqual(Complex32.Zero, vector[500]);
            Assert.AreEqual(2, storage.ValueCount);

            vector[800] = Complex32.Zero;
            Assert.AreEqual(Complex32.Zero, vector[800]);
            Assert.AreEqual(1, storage.ValueCount);

            vector[0] = Complex32.Zero;
            Assert.AreEqual(Complex32.Zero, vector[0]);
            Assert.AreEqual(0, storage.ValueCount);
        }

        /// <summary>
        /// Check sparse mechanism by zero multiply.
        /// </summary>
        [Test]
        public void CheckSparseMechanismByZeroMultiply()
        {
            var vector = new SparseVector(10000);

            // Add non-zero elements
            vector[200] = new Complex32(1.5f, 1);
            vector[500] = new Complex32(3.5f, 1);
            vector[800] = new Complex32(5.5f, 1);
            vector[0] = new Complex32(7.5f, 1);

            // Multiply by 0
            vector *= 0;

            var storage = (SparseVectorStorage<Complex32>) vector.Storage;
            Assert.AreEqual(Complex32.Zero, vector[200]);
            Assert.AreEqual(Complex32.Zero, vector[500]);
            Assert.AreEqual(Complex32.Zero, vector[800]);
            Assert.AreEqual(Complex32.Zero, vector[0]);
            Assert.AreEqual(0, storage.ValueCount);
        }

        /// <summary>
        /// Can calculate a dot product of two sparse vectors.
        /// </summary>
        [Test]
        public void CanDotProductOfTwoSparseVectors()
        {
            var vectorA = new SparseVector(10000);
            vectorA[200] = 1;
            vectorA[500] = 3;
            vectorA[800] = 5;
            vectorA[100] = 7;
            vectorA[900] = 9;

            var vectorB = new SparseVector(10000);
            vectorB[300] = 3;
            vectorB[500] = 5;
            vectorB[800] = 7;

            Assert.AreEqual(new Complex32(50.0f, 0), vectorA.DotProduct(vectorB));
        }

        /// <summary>
        /// Can pointwise multiple a sparse vector.
        /// </summary>
        [Test]
        public void CanPointwiseMultiplySparseVector()
        {
            var zeroArray = new[] {Complex32.Zero, new Complex32(1.0f, 1), Complex32.Zero, new Complex32(1.0f, 1), Complex32.Zero};
            var vector1 = SparseVector.OfEnumerable(Data);
            var vector2 = SparseVector.OfEnumerable(zeroArray);
            var result = new SparseVector(vector1.Count);

            vector1.PointwiseMultiply(vector2, result);

            for (var i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(Data[i]*zeroArray[i], result[i]);
            }

            var resultStorage = (SparseVectorStorage<Complex32>) result.Storage;
            Assert.AreEqual(2, resultStorage.ValueCount);
        }

        /// <summary>
        /// Test for issues #52. When setting previous non-zero values to zero,
        /// DoMultiply would copy non-zero values to the result, but use the
        /// length of nonzerovalues instead of NonZerosCount.
        /// </summary>
        [Test]
        public void CanScaleAVectorWhenSettingPreviousNonzeroElementsToZero()
        {
            var vector = new SparseVector(20);
            vector[10] = 1.0f;
            vector[11] = 2.0f;
            vector[11] = 0.0f;

            var scaled = new SparseVector(20);
            vector.Multiply(3.0f, scaled);

            Assert.AreEqual(3.0f, scaled[10].Real);
            Assert.AreEqual(0.0f, scaled[11].Real);
        }
    }
}
