// <copyright file="DenseVectorTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
	using System.Collections.Generic;
	using MbUnit.Framework;
    using LinearAlgebra.Double;
    using System;

    public class DenseVectorTests : VectorTests
    {
        protected override Vector CreateVector(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector CreateVector(IList<double> data)
        {
            var vector = new DenseVector(data.Count);
            for(var index = 0; index < data.Count; index++)
            {
                vector[index] = data[index];
            }

            return vector;
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromArray()
        {
            var data = new double[_data.Length];
            System.Array.Copy(_data, data, _data.Length);
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
            var vector = new DenseVector(_data);
            var other = new DenseVector(vector);
            
            
            Assert.AreNotSame(vector, other);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromAnotherVector()
        {
            var vector = (Vector)new DenseVector(_data);
            var other = new DenseVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateDenseVectorFromUserDefinedVector()
        {
            var vector = new UserDefinedVector(_data);
            var other = new DenseVector(vector);

            for (var i = 0; i < _data.Length; i++)
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
            var vector = new DenseVector(_data);
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
            var vector = new DenseVector(_data);
            var other = +vector;
            Assert.AreSame(vector, other, "Should be the same vector");
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoDenseVectorsUsingOperator()
        {
            var vector = new DenseVector(_data);
            var other = new DenseVector(_data);
            var result = vector + other;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] * 2.0, result[i]);
            }
        }

        [Test]
        public void CanCallUnaryNegationOperatorOnDenseVector()
        {
            var vector = new DenseVector(_data);
            var other = -vector;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(-_data[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoDenseVectorsUsingOperator()
        {
            var vector = new DenseVector(_data);
            var other = new DenseVector(_data);
            var result = vector - other;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplyDenseVectorByScalarUsingOperators()
        {
            var vector = new DenseVector(_data);
            vector = vector * 2.0;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector = vector * 1.0;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector = new DenseVector(_data);
            vector = 2.0 * vector;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector = 1.0 * vector;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideDenseVectorByScalarUsingOperators()
        {
            var vector = new DenseVector(_data);
            vector = vector / 2.0;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] / 2.0, vector[i]);
            }

            vector = vector / 1.0;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] / 2.0, vector[i]);
            }
        }

        [Test]
        public void CanFindAbsoluteMinimumIndexInDenseVector()
        {
            DenseVector source = new DenseVector(_data);
            int expected = 0;
            int actual = source.AbsoluteMinimumIndex();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindAbsoluteMinimumInDenseVector()
        {
            DenseVector source = new DenseVector(_data);
            double expected = 1;
            double actual = source.AbsoluteMinimum();
            Assert.AreEqual(expected, actual);

        }

        [Test]
        [Row(0, 5)]
        [Row(2, 2)]
        [Row(1, 4)]
        [Row(6, 10, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row(1, 10, ExpectedException = typeof(ArgumentOutOfRangeException))]
        [Row(1, -10, ExpectedException = typeof(ArgumentOutOfRangeException))]
        public void CanCalculateSubVector(int index, int length)
        {
            DenseVector vector = new DenseVector(_data);
            Vector sub = vector.SubVector(index, length);
            Assert.AreEqual(length, sub.Count);
            for (int i = 0; i < length; i++)
            {
                Assert.AreEqual(vector[i + index], sub[i]);
            }
        }

        [Test]
        public void CanFindMaximumIndexInDenseVector()
        {
            DenseVector vector = new DenseVector(_data);

            int expected = 4;
            int actual = vector.MaximumIndex();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindMaximumInDenseVector()
        {
            DenseVector vector = new DenseVector(_data);
            double expected = 5;
            double actual = vector.Maximum();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindMinimumIndexOfDenseVector()
        {
            DenseVector vector = new DenseVector(_data);
            int expected = 0;
            int actual = vector.MinimumIndex();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanFindMinimumOfDenseVector()
        {
            DenseVector vector = new DenseVector(_data);
            double expected = 1;
            double actual = vector.Minimum();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SumOfDenseVector()
        {
            double[] testData = { -20, -10, 10, 20, 30, };
            DenseVector vector = new DenseVector(testData);
            double actual = vector.Sum();
            double expected = 30;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SumMagnitudesOfDenseVector()
        {
            double[] testData = { -20, -10, 10, 20, 30, };
            DenseVector vector = new DenseVector(testData);
            double actual = vector.SumMagnitudes();
            double expected = 90;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void SetValuesWithNullParameterShouldThrowException()
        {
            DenseVector vector = new DenseVector(_data);
            vector.SetValues(null);
        }

        [Test]
        [ExpectedArgumentException]
        public new void SetValuesWithNonEqualDataLengthShouldThrowException()
        {
            DenseVector vector = new DenseVector(_data.Length + 2);
            vector.SetValues(_data);
        }

        [Test]
        public new void PointWiseMultiply()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = new DenseVector(vector1.Count);
            vector1.PointWiseMultiply(vector2, result);
            for (int i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(_data[i] * _data[i], result[i]);
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void PointWiseMultiplyWithOtherNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = null;
            DenseVector result = new DenseVector(vector1.Count);
            vector1.PointWiseMultiply(vector2, result);
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void PointWiseMultiplyWithResultNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = null;
            vector1.PointWiseMultiply(vector2, result);
        }

        [Test]
        [ExpectedArgumentException]
        public new void PointWiseMultiplyWithInvalidResultLengthShouldThrowException()
        {
            Vector vector1 = new DenseVector(_data);
            Vector vector2 = new DenseVector(_data);
            Vector result = CreateVector(vector1.Count + 1);
            vector1.PointWiseMultiply(vector2, result);
        }

        [Test]
        public new void PointWiseMultiplyWithResult()
        {
            Vector vector1 = new DenseVector(_data);
            Vector vector2 = new DenseVector(_data);
            Vector result = vector1.PointWiseMultiply(vector2);
            for (int i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(_data[i] * _data[i], result[i]);
            }
        }

        [Test]
        public new void PointWiseAdd()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = new DenseVector(vector1.Count);
            vector1.PointWiseAdd(vector2, result);
            for (int i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(_data[i] + _data[i], result[i]);
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void PointWiseAddWithOtherNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = null;
            DenseVector result = new DenseVector(vector1.Count);
            vector1.PointWiseAdd(vector2, result);
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void PointWiseAddWithResultNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = null;
            vector1.PointWiseAdd(vector2, result);
        }

        [Test]
        [ExpectedArgumentException]
        public new void PointWiseAddWithInvalidResultLengthShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = new DenseVector(vector1.Count + 1);
            vector1.PointWiseAdd(vector2, result);
        }

        [Test]
        public new void PointWiseAddWithResult()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = vector1.PointWiseAdd(vector2);
            for (int i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(_data[i] + _data[i], result[i]);
            }
        }

        [Test]
        public new void PointWiseSubtract()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = new DenseVector(vector1.Count);
            vector1.PointWiseSubtract(vector2, result);
            for (int i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(_data[i] - _data[i], result[i]);
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void PointWiseSubtractWithOtherNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = null;
            DenseVector result = new DenseVector(vector1.Count);
            vector1.PointWiseSubtract(vector2, result);
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void PointWiseSubtractWithResultNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            Vector result = null;
            vector1.PointWiseSubtract(vector2, result);
        }

        [Test]
        [ExpectedArgumentException]
        public new void PointWiseSubtractWithInvalidResultLengthShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = new DenseVector(vector1.Count + 1);
            vector1.PointWiseSubtract(vector2, result);
        }

        [Test]
        public new void PointWiseSubtractWithResult()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = vector1.PointWiseSubtract(vector2);
            for (int i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(_data[i] - _data[i], result[i]);
            }
        }

        [Test]
        public new void PointWiseDivide()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = new DenseVector(vector1.Count);
            vector1.PointWiseDivide(vector2, result);
            for (int i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(_data[i] / _data[i], result[i]);
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void PointWiseDivideWithOtherNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = null;
            DenseVector result = new DenseVector(vector1.Count);
            vector1.PointWiseDivide(vector2, result);
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void PointWiseDivideWithResultNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = null;
            vector1.PointWiseDivide(vector2, result);
        }

        [Test]
        [ExpectedArgumentException]
        public new void PointWiseDivideWithInvalidResultLengthShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = new DenseVector(vector1.Count + 1);
            vector1.PointWiseDivide(vector2, result);
        }

        [Test]
        public new void PointWiseDivideWithResult()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            DenseVector result = vector1.PointWiseDivide(vector2);
            for (int i = 0; i < vector1.Count; i++)
            {
                Assert.AreEqual(_data[i] / _data[i], result[i]);
            }
        }

        [Test]
        public new void CanCalculateDyadicProduct()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            Matrix m = DenseVector.DyadicProduct(vector1, vector2);
            for (int i = 0; i < vector1.Count; i++)
            {
                for (int j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void DyadicProductWithFirstParameterNullShouldThrowException()
        {
            DenseVector vector1 = null;
            DenseVector vector2 = new DenseVector(_data);
            DenseVector.DyadicProduct(vector1, vector2);
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void DyadicProductWithSecondParameterNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = null;
            Vector.DyadicProduct(vector1, vector2);
        }

        [Test]
        public new void CanCalculateTensorMultiply()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = new DenseVector(_data);
            Matrix m = vector1.TensorMultiply(vector2);
            for (int i = 0; i < vector1.Count; i++)
            {
                for (int j = 0; j < vector2.Count; j++)
                {
                    Assert.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public new void TensorMultiplyWithNullParameterNullShouldThrowException()
        {
            DenseVector vector1 = new DenseVector(_data);
            DenseVector vector2 = null;
            vector1.TensorMultiply(vector2);
        }

        [Test]
        [ExpectedArgumentException]
        public new void RandomWithNumberOfElementsLessThanZeroShouldThrowException()
        {

            DenseVector vector = new DenseVector(4);
            vector = vector.Random(-3);
        }
    }
}
