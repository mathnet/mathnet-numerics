// <copyright file="DenseVectorTests.cs" company="Math.NET">
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
    using System.Collections.Generic;
    using LinearAlgebra.Double;
    using LinearAlgebra.Generic;
    using MbUnit.Framework;

    public class DenseVectorTests : VectorTests
    {
        protected override Vector<double> CreateVector(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector<double> CreateVector(IList<double> data)
        {
            var vector = new DenseVector(data.Count);
            for (var index = 0; index < data.Count; index++)
            {
                vector[index] = data[index];
            }

            return vector;
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromArray()
        {
            var data = new double[Data.Length];
            Array.Copy(Data, data, Data.Length);
            var vector = new DenseVector(data);

            Assert.AreSame(data, vector.Data);
            for (var i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], vector[i]);
            }

            vector[0] = 100.0;
            Assert.AreEqual(100.0, data[0]);
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromAnotherDenseVector()
        {
            var vector = new DenseVector(Data);
            var other = new DenseVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromAnotherVector()
        {
            var vector = (Vector<double>)new DenseVector(Data);
            var other = new DenseVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromUserDefinedVector()
        {
            var vector = new UserDefinedVector(Data);
            var other = new DenseVector(vector);

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        public void CanCreateDenseVectorWithConstantValues()
        {
            var vector = new DenseVector(5, 5);
            Assert.ForAll(vector, value => value == 5);
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseMatrix()
        {
            var vector = new DenseVector(3);
            var matrix = vector.CreateMatrix(2, 3);
            Assert.AreEqual(2, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
        }


        [Test]
        [MultipleAsserts]
        public void CanConvertDenseVectorToArray()
        {
            var vector = new DenseVector(Data);
            var array = (double[])vector;
            Assert.IsInstanceOfType(typeof(double[]), array);
            Assert.AreSame(vector.Data, array);
            Assert.AreElementsEqual(vector, array);
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertArrayToDenseVector()
        {
            var array = new[] { 0.0, 1.0, 2.0, 3.0, 4.0 };
            var vector = (DenseVector)array;
            Assert.IsInstanceOfType(typeof(DenseVector), vector);
            Assert.AreElementsEqual(array, array);
        }

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

        [Test]
        [MultipleAsserts]
        public void CanAddTwoDenseVectorsUsingOperator()
        {
            var vector = new DenseVector(Data);
            var other = new DenseVector(Data);
            var result = vector + other;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i] * 2.0, result[i]);
            }
        }

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

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoDenseVectorsUsingOperator()
        {
            var vector = new DenseVector(Data);
            var other = new DenseVector(Data);
            var result = vector - other;

            for (var i = 0; i < Data.Length; i++)
            {
                Assert.AreEqual(Data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(Data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplyDenseVectorByScalarUsingOperators()
        {
            var vector = new DenseVector(Data);
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

            vector = new DenseVector(Data);
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

        [Test]
        [MultipleAsserts]
        public void CanDivideDenseVectorByScalarUsingOperators()
        {
            var vector = new DenseVector(Data);
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

        [Test]
        public void CanCalculateOuterProductForDenseVector()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            Matrix<double> m = Vector<double>.OuterProduct(vector1, vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void OuterProducForDenseVectortWithFirstParameterNullShouldThrowException()
        {
            DenseVector vector1 = null;
            var vector2 = CreateVector(Data);
            Vector<double>.OuterProduct(vector1, vector2);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void OuterProductForDenseVectorWithSecondParameterNullShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            DenseVector vector2 = null;
            Vector<double>.OuterProduct(vector1, vector2);
        }
    }
}