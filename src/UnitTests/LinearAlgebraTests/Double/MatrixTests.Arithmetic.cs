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
            var matrix = this.TestMatrices["Singular3x3"];
            var clone = matrix.Clone();
            clone.Multiply(scalar);

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
            var A = this.TestMatrices["Singular3x3"];
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
            var A = this.TestMatrices["Singular3x3"];
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
            var A = this.TestMatrices["Singular3x3"];
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
            var A = this.TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { 1.0, 2.0, 3.0 });
            Vector y = null;
            A.Multiply(x, y);
        }

        [Test]
        [ExpectedArgumentException]
        public void MultiplyWithVectorIntoResultFailsWhenResultIsTooLarge()
        {
            var A = this.TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { 1.0, 2.0, 3.0 });
            Vector y = new DenseVector(4);
            A.Multiply(x, y);
        }

        [Test]
        [Row(0)]
        [Row(1)]
        [Row(2.2)]
        [MultipleAsserts]
        public void CanOperatorLeftMultiplyWithScalar(double scalar)
        {
            var matrix = this.TestMatrices["Singular3x3"];
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
            var matrix = this.TestMatrices["Singular3x3"];
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
            var matrix = this.TestMatrices["Singular3x3"];
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
            var matrix = this.TestMatrices["Singular3x3"];
            Matrix result = null;
            matrix.Multiply(2.3, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void MultiplyWithScalarFailsWhenResultHasMoreRows()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var result = this.CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.Multiply(2.3, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void MultiplyWithScalarFailsWhenResultHasMoreColumns()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var result = this.CreateMatrix(matrix.RowCount, matrix.ColumnCount + 1);
            matrix.Multiply(2.3, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OperatorLeftMultiplyWithScalarFailsWhenMatrixIsNull()
        {
            Matrix matrix = null;
            var result = 2.3 * matrix;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OperatorRightMultiplyWithScalarFailsWhenMatrixIsNull()
        {
            Matrix matrix = null;
            var result = matrix * 2.3;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void CanAddMatrix(string mtxA, string mtxB)
        {
            var A = this.TestMatrices[mtxA];
            var B = this.TestMatrices[mtxB];

            var matrix = A.Clone();
            matrix.Add(B);
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
            var matrix = this.TestMatrices["Singular4x4"];
            Matrix other = null;
            matrix.Add(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddMatrixThrowsExceptionArgumentHasTooFewColumns()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Tall3x2"];
            matrix.Add(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddMatrixThrowsExceptionArgumentHasTooFewRows()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Wide2x3"];
            matrix.Add(other);
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void AddOperator(string mtxA, string mtxB)
        {
            var A = this.TestMatrices[mtxA];
            var B = this.TestMatrices[mtxB];

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
            Matrix matrix = null;
            var other = this.TestMatrices["Singular3x3"];
            var result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOperatorThrowsExceptionWhenRightsideIsNull()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            Matrix other = null;
            var result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOperatorThrowsExceptionWhenRightsideHasTooFewColumns()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Tall3x2"];
            var result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOperatorThrowsExceptionWhenRightsideHasTooFewRows()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Wide2x3"];
            var result = matrix + other;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void CanSubtractMatrix(string mtxA, string mtxB)
        {
            var A = this.TestMatrices[mtxA];
            var B = this.TestMatrices[mtxB];

            var matrix = A.Clone();
            matrix.Subtract(B);
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
            var matrix = this.TestMatrices["Singular4x4"];
            Matrix other = null;
            matrix.Subtract(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideHasTooFewColumns()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Tall3x2"];
            matrix.Subtract(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideHasTooFewRows()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Wide2x3"];
            matrix.Subtract(other);
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void SubtractOperator(string mtxA, string mtxB)
        {
            var A = this.TestMatrices[mtxA];
            var B = this.TestMatrices[mtxB];

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
            Matrix matrix = null;
            var other = this.TestMatrices["Singular3x3"];
            var result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideIsNull()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            Matrix other = null;
            var result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideHasTooFewColumns()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Tall3x2"];
            var result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideHasTooFewRows()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Wide2x3"];
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
            var A = this.TestMatrices[nameA];
            var B = this.TestMatrices[nameB];
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
            var A = this.TestMatrices[nameA];
            var B = this.TestMatrices[nameA];
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
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Tall3x2"];
            var result = matrix.TransposeAndMultiply(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TransposeAndMultiplyMatrixMatrixFailsWhenRightArgumentIsNull()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            Matrix other = null;
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
            var A = this.TestMatrices[nameA];
            var B = this.TestMatrices[nameA];
            var C = this.CreateMatrix(A.RowCount, B.RowCount);
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
            var matrix = this.TestMatrices["Singular3x3"];
            var other = this.TestMatrices["Wide2x3"];
            var result = matrix * other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyMatrixMatrixFailsWhenLeftArgumentIsNull()
        {
            Matrix matrix = null;
            var other = this.TestMatrices["Wide2x3"];
            var result = matrix * other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyMatrixMatrixFailsWhenRightArgumentIsNull()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            Matrix other = null;
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
            var A = this.TestMatrices[nameA];
            var B = this.TestMatrices[nameB];
            var C = this.CreateMatrix(A.RowCount, B.ColumnCount);
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
            var matrix = this.TestMatrices[name];
            var copy = matrix.Clone();

            copy.Negate();

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
            var matrix = this.TestMatrices[name];
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
            var matrix = this.TestMatrices["Singular3x3"];
            Matrix copy = null;
            matrix.Negate(copy);
        }

        [Test]
        [ExpectedArgumentException]
        public void NegateIntoResultFailsWhenResultHasMoreRows()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var target = this.CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.Negate(target);
        }

        [Test]
        [ExpectedArgumentException]
        public void NegateIntoResultFailsWhenResultHasMoreColumns()
        {
            var matrix = this.TestMatrices["Singular3x3"];
            var target = this.CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.Negate(target);
        }


        [Test]
        public void KroneckerProduct()
        {
            var A = this.TestMatrices["Wide2x3"];
            var B = this.TestMatrices["Square3x3"];
            var result = this.CreateMatrix(A.RowCount * B.RowCount, A.ColumnCount * B.ColumnCount);
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
            var A = this.TestMatrices["Wide2x3"];
            var B = this.TestMatrices["Square3x3"];
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
            var matrix = this.TestMatrices["Square4x4"];
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
            var matrix = this.TestMatrices["Square4x4"].NormalizeRows(pValue);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                var row = matrix.Row(i);
                Assert.AreApproximatelyEqual(1.0, row.Norm(pValue), 10e-12);
            }
        }

        [Test]
        public void PointwiseMultiplyResult()
        {
            foreach (var  data in this.TestMatrices.Values)
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
            var matrix = this.TestMatrices["Wide2x3"];
            Matrix other = null;
            var result = matrix.Clone();
            matrix.PointwiseMultiply(other, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PointwiseMultiplyWithResultNullShouldThrowException()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            matrix.PointwiseMultiply(other, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PointwiseMultiplyWithInvalidOtherMatrixDimensionsShouldThrowException()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            var other = this.CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            var result = matrix.Clone();
            matrix.PointwiseMultiply(other, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PointwiseMultiplyWithInvalidResultMatrixDimensionsShouldThrowException()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            var result = this.CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.PointwiseMultiply(other, result);
        }

        [Test]
        public virtual void PointwiseDivideResult()
        {
            foreach (var  data in this.TestMatrices.Values)
            {
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
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PointwiseDivideWithNullOtherShouldThrowException()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            Matrix other = null;
            var result = matrix.Clone();
            matrix.PointwiseDivide(other, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PointwiseDivideWithResultNullShouldThrowException()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            matrix.PointwiseDivide(other, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PointwiseDivideWithInvalidOtherMatrixDimensionsShouldThrowException()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            var other = this.CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            var result = matrix.Clone();
            matrix.PointwiseDivide(other, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void PointwiseDivideWithInvalidResultMatrixDimensionsShouldThrowException()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            var result = this.CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.PointwiseDivide(other, result);
        }

        [Test]
        [Row(0, ExpectedException = typeof(ArgumentException))]
        [Row(-2, ExpectedException = typeof(ArgumentException))]
        public void RandomWithNonPositiveNumberOfRowsShouldThrowException(int numberOfRows)
        {
            var matrix = this.CreateMatrix(2, 3);
            matrix = matrix.Random(numberOfRows, 4, new ContinuousUniform());
        }

        [Test]
        [Row(0, ExpectedException = typeof(ArgumentException))]
        [Row(-2, ExpectedException = typeof(ArgumentException))]
        public void RandomWithNonPositiveNumberOfRowsShouldThrowException2(int numberOfRows)
        {
            var matrix = this.CreateMatrix(2, 3);
            matrix = matrix.Random(numberOfRows, 4, new DiscreteUniform(0, 2));
        }

        [Test]
        public void Trace()
        {
            var matrix = this.TestMatrices["Square3x3"];
            var trace = matrix.Trace();
            Assert.AreEqual(6.6, trace);
        }

        [Test]
        [ExpectedArgumentException]
        public void TraceOfNonSquareMatrixShouldThrowException()
        {
            var matrix = this.TestMatrices["Wide2x3"];
            var trace = matrix.Trace();
        }
    }
}