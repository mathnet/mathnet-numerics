// <copyright file="MatrixTests.Arithmetic.cs" company="Math.NET">
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
    using Distributions;
    using LinearAlgebra.Double;
    using LinearAlgebra.Generic;
    using MbUnit.Framework;

    public abstract partial class MatrixTests
    {
        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2.2)]
        [MultipleAsserts]
        public void CanMultiplyWithScalar(double scalar)
        {
            var matrix = TestMatrices["Singular3x3"];
            var clone = matrix.Clone();
            clone = clone.Multiply(scalar);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j] * scalar, clone[i, j]);
                }
            }
        }

        [Test]
        public void CanMultiplyWithVector()
        {
            var A = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { 1.0, 2.0, 3.0 });
            var y = A * x;

            Assert.AreEqual(A.RowCount, y.Count);

            for (var i = 0; i < A.RowCount; i++)
            {
                var ar = A.Row(i);
                var dot = ar * x;
                Assert.AreEqual(dot, y[i]);
            }
        }

        [Test]
        public void CanMultiplyWithVectorIntoResult()
        {
            var A = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { 1.0, 2.0, 3.0 });
            var y = new DenseVector(3);
            A.Multiply(x, y);

            for (var i = 0; i < A.RowCount; i++)
            {
                var ar = A.Row(i);
                var dot = ar * x;
                Assert.AreEqual(dot, y[i]);
            }
        }

        [Test]
        public void CanMultiplyWithVectorIntoResultWhenUpdatingInputArgument()
        {
            var A = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { 1.0, 2.0, 3.0 });
            var y = x;
            A.Multiply(x, x);

            Assert.AreSame(y, x);

            y = new DenseVector(new[] { 1.0, 2.0, 3.0 });
            for (var i = 0; i < A.RowCount; i++)
            {
                var ar = A.Row(i);
                var dot = ar * y;
                Assert.AreEqual(dot, x[i]);
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void MultiplyWithVectorIntoResultFailsWhenResultIsNull()
        {
            var A = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { 1.0, 2.0, 3.0 });
            Vector<double> y = null;
            A.Multiply(x, y);
        }

        [Test]
        [ExpectedArgumentException]
        public void MultiplyWithVectorIntoResultFailsWhenResultIsTooLarge()
        {
            var A = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { 1.0, 2.0, 3.0 });
            Vector<double> y = new DenseVector(4);
            A.Multiply(x, y);
        }

        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2.2)]
        [MultipleAsserts]
        public void CanOperatorLeftMultiplyWithScalar(double scalar)
        {
            var matrix = TestMatrices["Singular3x3"];
            var clone = matrix * scalar;

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j] * scalar, clone[i, j]);
                }
            }
        }

        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2.2)]
        [MultipleAsserts]
        public void CanOperatorRightMultiplyWithScalar(double scalar)
        {
            var matrix = TestMatrices["Singular3x3"];
            var clone = matrix * scalar;

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j] * scalar, clone[i, j]);
                }
            }
        }

        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2.2)]
        [MultipleAsserts]
        public void CanMultiplyWithScalarIntoResult(double scalar)
        {
            var matrix = TestMatrices["Singular3x3"];
            var result = matrix.Clone();
            matrix.Multiply(scalar, result);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j] * scalar, result[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyWithScalarIntoResultFailsWhenResultIsNull()
        {
            var matrix = TestMatrices["Singular3x3"];
            Matrix<double> result = null;
            matrix.Multiply(2.3, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void MultiplyWithScalarFailsWhenResultHasMoreRows()
        {
            var matrix = TestMatrices["Singular3x3"];
            var result = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.Multiply(2.3, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void MultiplyWithScalarFailsWhenResultHasMoreColumns()
        {
            var matrix = TestMatrices["Singular3x3"];
            var result = CreateMatrix(matrix.RowCount, matrix.ColumnCount + 1);
            matrix.Multiply(2.3, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OperatorLeftMultiplyWithScalarFailsWhenMatrixIsNull()
        {
            Matrix<double> matrix = null;
            var result = 2.3 * matrix;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OperatorRightMultiplyWithScalarFailsWhenMatrixIsNull()
        {
            Matrix<double> matrix = null;
            var result = matrix * 2.3;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void CanAddMatrix(string mtxA, string mtxB)
        {
            var A = TestMatrices[mtxA];
            var B = TestMatrices[mtxB];

            var matrix = A.Clone();
            matrix = matrix.Add(B);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], A[i, j] + B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddMatrixThrowsExceptionWhenArgumentIsNull()
        {
            var matrix = TestMatrices["Singular4x4"];
            Matrix<double> other = null;
            matrix.Add(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddMatrixThrowsExceptionArgumentHasTooFewColumns()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            matrix.Add(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddMatrixThrowsExceptionArgumentHasTooFewRows()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            matrix.Add(other);
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void AddOperator(string mtxA, string mtxB)
        {
            var A = TestMatrices[mtxA];
            var B = TestMatrices[mtxB];

            var result = A + B;
            for (var i = 0; i < A.RowCount; i++)
            {
                for (var j = 0; j < A.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i, j], A[i, j] + B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOperatorThrowsExceptionWhenLeftsideIsNull()
        {
            Matrix<double> matrix = null;
            var other = TestMatrices["Singular3x3"];
            var result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOperatorThrowsExceptionWhenRightsideIsNull()
        {
            var matrix = TestMatrices["Singular3x3"];
            Matrix<double> other = null;
            var result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOperatorThrowsExceptionWhenRightsideHasTooFewColumns()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            var result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOperatorThrowsExceptionWhenRightsideHasTooFewRows()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            var result = matrix + other;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void CanSubtractMatrix(string mtxA, string mtxB)
        {
            var A = TestMatrices[mtxA];
            var B = TestMatrices[mtxB];

            var matrix = A.Clone();
            matrix =  matrix.Subtract(B);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], A[i, j] - B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideIsNull()
        {
            var matrix = TestMatrices["Singular4x4"];
            Matrix<double> other = null;
            matrix.Subtract(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideHasTooFewColumns()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            matrix.Subtract(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideHasTooFewRows()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            matrix.Subtract(other);
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void SubtractOperator(string mtxA, string mtxB)
        {
            var A = TestMatrices[mtxA];
            var B = TestMatrices[mtxB];

            var result = A - B;
            for (var i = 0; i < A.RowCount; i++)
            {
                for (var j = 0; j < A.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i, j], A[i, j] - B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubtractOperatorThrowsExceptionWhenLeftsideIsNull()
        {
            Matrix<double> matrix = null;
            var other = TestMatrices["Singular3x3"];
            var result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideIsNull()
        {
            var matrix = TestMatrices["Singular3x3"];
            Matrix<double> other = null;
            var result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideHasTooFewColumns()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            var result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideHasTooFewRows()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            var result = matrix - other;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        [Row("Wide2x3", "Square3x3")]
        [Row("Wide2x3", "Tall3x2")]
        [Row("Tall3x2", "Wide2x3")]
        [MultipleAsserts]
        public void CanMultiplyMatrixWithMatrix(string nameA, string nameB)
        {
            var A = TestMatrices[nameA];
            var B = TestMatrices[nameB];
            var C = A * B;

            Assert.AreEqual(C.RowCount, A.RowCount);
            Assert.AreEqual(C.ColumnCount, B.ColumnCount);

            for (var i = 0; i < C.RowCount; i++)
            {
                for (var j = 0; j < C.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(A.Row(i) * B.Column(j), C[i, j], 15);
                }
            }
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Singular4x4")]
        [Row("Wide2x3")]
        [Row("Tall3x2")]
        [MultipleAsserts]
        public void CanTransposeAndMultiplyMatrixWithMatrix(string nameA)
        {
            var A = TestMatrices[nameA];
            var B = TestMatrices[nameA];
            var C = A.TransposeAndMultiply(B);

            Assert.AreEqual(C.RowCount, A.RowCount);
            Assert.AreEqual(C.ColumnCount, B.RowCount);

            for (var i = 0; i < C.RowCount; i++)
            {
                for (var j = 0; j < C.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(A.Row(i) * B.Row(j), C[i, j], 15);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TransposeAndMultiplyMatrixMatrixFailsWhenSizesAreIncompatible()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            var result = matrix.TransposeAndMultiply(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransposeAndMultiplyMatrixMatrixFailsWhenRightArgumentIsNull()
        {
            var matrix = TestMatrices["Wide2x3"];
            Matrix<double> other = null;
            var result = matrix.TransposeAndMultiply(other);
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Singular4x4")]
        [Row("Wide2x3")]
        [Row("Wide2x3")]
        [Row("Tall3x2")]
        [MultipleAsserts]
        public void CanTransposeAndMultiplyMatrixWithMatrixIntoResult(string nameA)
        {
            var A = TestMatrices[nameA];
            var B = TestMatrices[nameA];
            var C = CreateMatrix(A.RowCount, B.RowCount);
            A.TransposeAndMultiply(B, C);

            Assert.AreEqual(C.RowCount, A.RowCount);
            Assert.AreEqual(C.ColumnCount, B.RowCount);

            for (var i = 0; i < C.RowCount; i++)
            {
                for (var j = 0; j < C.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(A.Row(i) * B.Row(j), C[i, j], 15);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MultiplyMatrixMatrixFailsWhenSizesAreIncompatible()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            var result = matrix * other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyMatrixMatrixFailsWhenLeftArgumentIsNull()
        {
            Matrix<double> matrix = null;
            var other = TestMatrices["Wide2x3"];
            var result = matrix * other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyMatrixMatrixFailsWhenRightArgumentIsNull()
        {
            var matrix = TestMatrices["Wide2x3"];
            Matrix<double> other = null;
            var result = matrix * other;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        [Row("Wide2x3", "Square3x3")]
        [Row("Wide2x3", "Tall3x2")]
        [Row("Tall3x2", "Wide2x3")]
        [MultipleAsserts]
        public virtual void CanMultiplyMatrixWithMatrixIntoResult(string nameA, string nameB)
        {
            var A = TestMatrices[nameA];
            var B = TestMatrices[nameB];
            var C = CreateMatrix(A.RowCount, B.ColumnCount);
            A.Multiply(B, C);

            Assert.AreEqual(C.RowCount, A.RowCount);
            Assert.AreEqual(C.ColumnCount, B.ColumnCount);

            for (var i = 0; i < C.RowCount; i++)
            {
                for (var j = 0; j < C.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(A.Row(i) * B.Column(j), C[i, j], 15);
                }
            }
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        [MultipleAsserts]
        public void CanNegate(string name)
        {
            var matrix = TestMatrices[name];
            var copy = matrix.Clone();

            copy = copy.Negate();

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(-matrix[i, j], copy[i, j]);
                }
            }
        }

        [Test]
        [Row("Singular3x3")]
        [Row("Square3x3")]
        [Row("Square4x4")]
        [Row("Tall3x2")]
        [Row("Wide2x3")]
        [MultipleAsserts]
        public void CanNegateIntoResult(string name)
        {
            var matrix = TestMatrices[name];
            var copy = matrix.Clone();

            matrix.Negate(copy);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(-matrix[i, j], copy[i, j]);
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void NegateIntoResultFailsWhenResultIsNull()
        {
            var matrix = TestMatrices["Singular3x3"];
            Matrix<double> copy = null;
            matrix.Negate(copy);
        }

        [Test]
        [ExpectedArgumentException]
        public void NegateIntoResultFailsWhenResultHasMoreRows()
        {
            var matrix = TestMatrices["Singular3x3"];
            var target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.Negate(target);
        }

        [Test]
        [ExpectedArgumentException]
        public void NegateIntoResultFailsWhenResultHasMoreColumns()
        {
            var matrix = TestMatrices["Singular3x3"];
            var target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.Negate(target);
        }


        [Test]
        public void KroneckerProduct()
        {
            var A = TestMatrices["Wide2x3"];
            var B = TestMatrices["Square3x3"];
            var result = CreateMatrix(A.RowCount * B.RowCount, A.ColumnCount * B.ColumnCount);
            A.KroneckerProduct(B, result);
            for (var i = 0; i < A.RowCount; i++)
            {
                for (var j = 0; j < A.ColumnCount; j++)
                {
                    for (var ii = 0; ii < B.RowCount; ii++)
                    {
                        for (var jj = 0; jj < B.ColumnCount; jj++)
                        {
                            Assert.AreEqual(result[i * B.RowCount + ii, j * B.ColumnCount + jj], A[i, j] * B[ii, jj]);
                        }
                    }
                }
            }
        }

        [Test]
        public void KroneckerProductResult()
        {
            var A = TestMatrices["Wide2x3"];
            var B = TestMatrices["Square3x3"];
            var result = A.KroneckerProduct(B);
            for (var i = 0; i < A.RowCount; i++)
            {
                for (var j = 0; j < A.ColumnCount; j++)
                {
                    for (var ii = 0; ii < B.RowCount; ii++)
                    {
                        for (var jj = 0; jj < B.ColumnCount; jj++)
                        {
                            Assert.AreEqual(result[i * B.RowCount + ii, j * B.ColumnCount + jj], A[i, j] * B[ii, jj]);
                        }
                    }
                }
            }
        }

        [Test]
        [Row(1)]
        [Row(2)]
        [Row(-4, ExpectedException = typeof(ArgumentOutOfRangeException))]
        public void NormalizeColumns(int pValue)
        {
            var matrix = TestMatrices["Square4x4"];
            var result = matrix.NormalizeColumns(pValue);
            for (var j = 0; j < result.ColumnCount; j++)
            {
                var col = result.Column(j);
                Assert.AreApproximatelyEqual(1.0, col.Norm(pValue), 10e-12);
            }
        }

        [Test]
        [Row(1)]
        [Row(2)]
        [Row(-3, ExpectedException = typeof(ArgumentOutOfRangeException))]
        public void NormalizeRows(int pValue)
        {
            var matrix = TestMatrices["Square4x4"].NormalizeRows(pValue);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                var row = matrix.Row(i);
                Assert.AreApproximatelyEqual(1.0, row.Norm(pValue), 10e-12);
            }
        }

        [Test]
        public void PointwiseMultiplyResult()
        {
            foreach (var  data in TestMatrices.Values)
            {
                var other = data.Clone();
                var result = data.Clone();
                data.PointwiseMultiply(other, result);
                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        Assert.AreEqual(data[i, j] * other[i, j], result[i, j]);
                    }
                }

                result = data.PointwiseMultiply(other);
                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        Assert.AreEqual(data[i, j] * other[i, j], result[i, j]);
                    }
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PointwiseMultiplyWithNullOtherShouldThrowException()
        {
            var matrix = TestMatrices["Wide2x3"];
            Matrix<double> other = null;
            var result = matrix.Clone();
            matrix.PointwiseMultiply(other, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PointwiseMultiplyWithResultNullShouldThrowException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            matrix.PointwiseMultiply(other, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PointwiseMultiplyWithInvalidOtherMatrixDimensionsShouldThrowException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            var result = matrix.Clone();
            matrix.PointwiseMultiply(other, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PointwiseMultiplyWithInvalidResultMatrixDimensionsShouldThrowException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            var result = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.PointwiseMultiply(other, result);
        }

        [Test]
        public virtual void PointwiseDivideResult()
        {
            var data = TestMatrices["Singular3x3"];
            var other = data.Clone();
            var result = data.Clone();
            data.PointwiseDivide(other, result);
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(data[i, j] / other[i, j], result[i, j]);
                }
            }

            result = data.PointwiseDivide(other);
            for (var i = 0; i < data.RowCount; i++)
            {
                for (var j = 0; j < data.ColumnCount; j++)
                {
                    Assert.AreEqual(data[i, j] / other[i, j], result[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PointwiseDivideWithNullOtherShouldThrowException()
        {
            var matrix = TestMatrices["Wide2x3"];
            Matrix<double> other = null;
            var result = matrix.Clone();
            matrix.PointwiseDivide(other, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PointwiseDivideWithResultNullShouldThrowException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            matrix.PointwiseDivide(other, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PointwiseDivideWithInvalidOtherMatrixDimensionsShouldThrowException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            var result = matrix.Clone();
            matrix.PointwiseDivide(other, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PointwiseDivideWithInvalidResultMatrixDimensionsShouldThrowException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            var result = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.PointwiseDivide(other, result);
        }

        [Test]
        [Row(0, ExpectedException = typeof(ArgumentException))]
        [Row(-2, ExpectedException = typeof(ArgumentException))]
        public void RandomWithNonPositiveNumberOfRowsShouldThrowException(int numberOfRows)
        {
            var matrix = CreateMatrix(2, 3);
            matrix = matrix.Random(numberOfRows, 4, new ContinuousUniform());
        }

        [Test]
        [Row(0, ExpectedException = typeof(ArgumentException))]
        [Row(-2, ExpectedException = typeof(ArgumentException))]
        public void RandomWithNonPositiveNumberOfRowsShouldThrowException2(int numberOfRows)
        {
            var matrix = CreateMatrix(2, 3);
            matrix = matrix.Random(numberOfRows, 4, new DiscreteUniform(0, 2));
        }

        [Test]
        public void Trace()
        {
            var matrix = TestMatrices["Square3x3"];
            var trace = matrix.Trace();
            Assert.AreEqual(6.6, trace);
        }

        [Test]
        [ExpectedArgumentException]
        public void TraceOfNonSquareMatrixShouldThrowException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var trace = matrix.Trace();
        }
    }
}