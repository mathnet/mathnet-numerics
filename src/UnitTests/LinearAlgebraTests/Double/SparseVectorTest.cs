// <copyright file="SparseVectorTest.cs" company="Math.NET">
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
    using System.Collections.Generic;
    using LinearAlgebra.Double;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Storage;
    using NUnit.Framework;

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
        protected override Vector<double> CreateVector(int size)
        {
            return new SparseVector(size);
        }

        /// <summary>
        /// Creates a new instance of the Vector class.
        /// </summary>
        /// <param name="data">The array to create this vector from.</param>
        /// <returns>The new <c>Vector</c>.</returns>
        protected override Vector<double> CreateVector(IList<double> data)
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
            var data = new double[Data.Length];
            Array.Copy(Data, data, Data.Length);
            var vector = new SparseVector(data);

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
            var vector = new SparseVector(Data);
            var other = new SparseVector(vector);

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
            var vector = (Vector<double>)new SparseVector(Data);
            var other = new SparseVector(vector);

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
            var other = new SparseVector(vector);

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
            var matrix = vector.CreateMatrix(2, 3);
            Assert.AreEqual(2, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
        }

        /// <summary>
        /// Can convert a sparse vector to an array.
        /// </summary>
        [Test]
        public void CanConvertSparseVectorToArray()
        {
            var vector = new SparseVector(Data);
            var array = vector.ToArray();
            Assert.IsInstanceOf(typeof(double[]), array);
            CollectionAssert.AreEqual(vector, array);
        }

        /// <summary>
        /// Can convert an array to a sparse vector.
        /// </summary>
        [Test]
        public void CanConvertArrayToSparseVector()
        {
            var array = new[] { 0.0, 1.0, 2.0, 3.0, 4.0 };
            var vector = new SparseVector(array);
            Assert.IsInstanceOf(typeof(SparseVector), vector);
            CollectionAssert.AreEqual(array, array);
        }

        /// <summary>
        /// Can multiply a sparse vector by a scalar using "*" operator.
        /// </summary>
        [Test]
        public void CanMultiplySparseVectorByScalarUsingOperators()
        {
            var vector = new SparseVector(Data);
            vector = vector * 2.0;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector = vector * 1.0;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector = new SparseVector(Data);
            vector = 2.0 * vector;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }

            vector = 1.0 * vector;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] * 2.0, vector[i]);
            }
        }

        /// <summary>
        /// Can divide a sparse vector by a scalar using "/" operator.
        /// </summary>
        [Test]
        public void CanDivideSparseVectorByScalarUsingOperators()
        {
            var vector = new SparseVector(Data);
            vector = vector / 2.0;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
            }

            vector = vector / 1.0;
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i] / 2.0, vector[i]);
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
            var m = Vector<double>.OuterProduct(vector1, vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        /// <summary>
        /// Outer product for <c>null</c> sparse vectors throws <c>ArgumentNullException</c>.
        /// </summary>
        [Test]
        public void OuterProductForNullSparseVectorsThrowsArgumentNullException()
        {
            SparseVector vector1 = null;
            var vector2 = CreateVector(Data);
            Assert.Throws<ArgumentNullException>(() => Vector<double>.OuterProduct(vector1, vector2));
            Assert.Throws<ArgumentNullException>(() => Vector<double>.OuterProduct(vector2, vector1));
        }

        /// <summary>
        /// Check sparse mechanism by setting values.
        /// </summary>
        [Test]
        public void CheckSparseMechanismBySettingValues()
        {
            var vector = new SparseVector(10000);
            var storage = (SparseVectorStorage<double>)vector.Storage;

            // Add non-zero elements
            vector[200] = 1.5;
            Assert.AreEqual(1.5, vector[200]);
            Assert.AreEqual(1, storage.ValueCount);

            vector[500] = 3.5;
            Assert.AreEqual(3.5, vector[500]);
            Assert.AreEqual(2, storage.ValueCount);

            vector[800] = 5.5;
            Assert.AreEqual(5.5, vector[800]);
            Assert.AreEqual(3, storage.ValueCount);

            vector[0] = 7.5;
            Assert.AreEqual(7.5, vector[0]);
            Assert.AreEqual(4, storage.ValueCount);

            // Remove non-zero elements
            vector[200] = 0;
            Assert.AreEqual(0, vector[200]);
            Assert.AreEqual(3, storage.ValueCount);

            vector[500] = 0;
            Assert.AreEqual(0, vector[500]);
            Assert.AreEqual(2, storage.ValueCount);

            vector[800] = 0;
            Assert.AreEqual(0, vector[800]);
            Assert.AreEqual(1, storage.ValueCount);

            vector[0] = 0;
            Assert.AreEqual(0, vector[0]);
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
            vector[200] = 1.5;
            vector[500] = 3.5;
            vector[800] = 5.5;
            vector[0] = 7.5;

            // Multiply by 0
            vector *= 0;

            var storage = (SparseVectorStorage<double>)vector.Storage;
            Assert.AreEqual(0, vector[200]);
            Assert.AreEqual(0, vector[500]);
            Assert.AreEqual(0, vector[800]);
            Assert.AreEqual(0, vector[0]);
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

            Assert.AreEqual(50.0, vectorA.DotProduct(vectorB));
        }

        /// <summary>
        /// Can pointwise multiple a sparse vector.
        /// </summary>
        [Test]
        public void CanPointwiseMultiplySparseVector()
        {
            var zeroArray = new[] { 0.0, 1.0, 0.0, 1.0, 0.0 };
            var vector1 = new SparseVector(Data);
            var vector2 = new SparseVector(zeroArray);
            var result = new SparseVector(vector1.Count);

            vector1.PointwiseMultiply(vector2, result);

            for (var i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(Data[i] * zeroArray[i], result[i]);
            }

            var resultStorage = (SparseVectorStorage<double>)result.Storage;
            Assert.AreEqual(2, resultStorage.ValueCount);
        }

        /// <summary>
        /// Can outer multiple two sparse vectors. Checking fix for workitem 5696.
        /// </summary>
        [Test]
        public void CanOuterMultiplySparseVectors()
        {
            var vector1 = new SparseVector(new[] { 2.0, 2.0, 0.0, 0.0 });
            var vector2 = new SparseVector(new[] { 2.0, 2.0, 0.0, 0.0 });
            var result = vector1.OuterProduct(vector2);

            Assert.AreEqual(4.0, result[0, 0]);
            Assert.AreEqual(4.0, result[0, 1]);
            Assert.AreEqual(4.0, result[1, 0]);
            Assert.AreEqual(4.0, result[1, 1]);

            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    if (i > 1 || j > 1)
                    {
                        Assert.AreEqual(0.0, result[i, j]);
                    }
                }
            }
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
            vector[10] = 1.0;
            vector[11] = 2.0;
            vector[11] = 0.0;

            var scaled = new SparseVector(20);
            vector.Multiply(3.0, scaled);

            Assert.AreEqual(3.0, scaled[10]);
            Assert.AreEqual(0.0, scaled[11]);
        }
    }
}
