// <copyright file="MatrixTests.Arithmetic.cs" company="Math.NET">
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
	using System;
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
            var matrix = testMatrices["Singular3x3"];
            var clone = matrix.Clone();
            clone.Multiply(scalar);

            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j] * scalar, clone[i, j]);
                }
            }
        }

        [Test]
        public void CanMultiplyWithVector()
        {
            var A = testMatrices["Singular3x3"];
            var x = new DenseVector(new double[] { 1.0, 2.0, 3.0 });
            var y = A * x;

            Assert.AreEqual(A.RowCount, y.Count);

            for (int i = 0; i < A.RowCount; i++)
            {
                var ar = A.GetRow(i);
                var dot = ar * x;
                Assert.AreEqual(dot, y[i]);
            }
        }

        [Test]
        public void CanMultiplyWithVectorIntoResult()
        {
            var A = testMatrices["Singular3x3"];
            var x = new DenseVector(new double[] { 1.0, 2.0, 3.0 });
            var y = new DenseVector(3);
            A.Multiply(x, y);

            for (int i = 0; i < A.RowCount; i++)
            {
                var ar = A.GetRow(i);
                var dot = ar * x;
                Assert.AreEqual(dot, y[i]);
            }
        }

        [Test]
        public void CanMultiplyWithVectorIntoResultWhenUpdatingInputArgument()
        {
            var A = testMatrices["Singular3x3"];
            var x = new DenseVector(new double[] { 1.0, 2.0, 3.0 });
            var y = x;
            A.Multiply(x, x);

            Assert.AreSame(y, x);

            y = new DenseVector(new double[] { 1.0, 2.0, 3.0 });
            for (int i = 0; i < A.RowCount; i++)
            {
                var ar = A.GetRow(i);
                var dot = ar * y;
                Assert.AreEqual(dot, x[i]);
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void MultiplyWithVectorIntoResultFailsWhenResultIsNull()
        {
            var A = testMatrices["Singular3x3"];
            var x = new DenseVector(new double[] { 1.0, 2.0, 3.0 });
            Vector y = null;
            A.Multiply(x, y);
        }

        [Test]
        [ExpectedArgumentException]
        public void MultiplyWithVectorIntoResultFailsWhenResultIsTooLarge()
        {
            var A = testMatrices["Singular3x3"];
            var x = new DenseVector(new double[] { 1.0, 2.0, 3.0 });
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
            var matrix = testMatrices["Singular3x3"];
            var clone = matrix * scalar;

            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
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
            var matrix = testMatrices["Singular3x3"];
            var clone = matrix * scalar;

            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
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
            var matrix = testMatrices["Singular3x3"];
            var result = matrix.Clone();
            matrix.Multiply(scalar, result);

            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j] * scalar, result[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyWithScalarIntoResultFailsWhenResultIsNull()
        {
            var matrix = testMatrices["Singular3x3"];
            Matrix result = null;
            matrix.Multiply(2.3, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void MultiplyWithScalarFailsWhenResultHasMoreRows()
        {
            var matrix = testMatrices["Singular3x3"];
            Matrix result = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.Multiply(2.3, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void MultiplyWithScalarFailsWhenResultHasMoreColumns()
        {
            var matrix = testMatrices["Singular3x3"];
            Matrix result = CreateMatrix(matrix.RowCount, matrix.ColumnCount + 1);
            matrix.Multiply(2.3, result);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OperatorLeftMultiplyWithScalarFailsWhenMatrixIsNull()
        {
            Matrix matrix = null;
            Matrix result = 2.3 * matrix;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OperatorRightMultiplyWithScalarFailsWhenMatrixIsNull()
        {
            Matrix matrix = null;
            Matrix result = matrix * 2.3;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void CanAddMatrix(string mtxA, string mtxB)
        {
            var A = testMatrices[mtxA];
            var B = testMatrices[mtxB];

            Matrix matrix = A.Clone();
            matrix.Add(B);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], A[i, j] + B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddMatrixThrowsExceptionWhenArgumentIsNull()
        {
            Matrix matrix = testMatrices["Singular4x4"];
            Matrix other = null;
            matrix.Add(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddMatrixThrowsExceptionArgumentHasTooFewColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Tall3x2"];
            matrix.Add(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddMatrixThrowsExceptionArgumentHasTooFewRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            matrix.Add(other);
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void AddOperator(string mtxA, string mtxB)
        {
            var A = testMatrices[mtxA];
            var B = testMatrices[mtxB];
            
            Matrix result = A + B;
            for (int i = 0; i < A.RowCount; i++)
            {
                for (int j = 0; j < A.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i,j], A[i, j] + B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOperatorThrowsExceptionWhenLeftsideIsNull()
        {
            Matrix matrix = null;
            Matrix other = testMatrices["Singular3x3"];
            Matrix result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddOperatorThrowsExceptionWhenRightsideIsNull()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = null;
            Matrix result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOperatorThrowsExceptionWhenRightsideHasTooFewColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Tall3x2"];
            Matrix result = matrix + other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void AddOperatorThrowsExceptionWhenRightsideHasTooFewRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            Matrix result = matrix + other;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void CanSubtractMatrix(string mtxA, string mtxB)
        {
            var A = testMatrices[mtxA];
            var B = testMatrices[mtxB];

            Matrix matrix = A.Clone();
            matrix.Subtract(B);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], A[i, j] - B[i, j]);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideIsNull()
        {
            Matrix matrix = testMatrices["Singular4x4"];
            Matrix other = null;
            matrix.Subtract(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideHasTooFewColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Tall3x2"];
            matrix.Subtract(other);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractMatrixThrowsExceptionWhenRightSideHasTooFewRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            matrix.Subtract(other);
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        public void SubtractOperator(string mtxA, string mtxB)
        {
            var A = testMatrices[mtxA];
            var B = testMatrices[mtxB];

            Matrix result = A - B;
            for (int i = 0; i < A.RowCount; i++)
            {
                for (int j = 0; j < A.ColumnCount; j++)
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
            Matrix other = testMatrices["Singular3x3"];
            Matrix result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideIsNull()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = null;
            Matrix result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideHasTooFewColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Tall3x2"];
            Matrix result = matrix - other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SubtractOperatorThrowsExceptionWhenRightsideHasTooFewRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            Matrix result = matrix - other;
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
            Matrix A = testMatrices[nameA];
            Matrix B = testMatrices[nameB];
            Matrix C = A * B;

            Assert.AreEqual(C.RowCount, A.RowCount);
            Assert.AreEqual(C.ColumnCount, B.ColumnCount);

            for (int i = 0; i < C.RowCount; i++)
            {
                for (int j = 0; j < C.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(A.GetRow(i) * B.GetColumn(j), C[i, j], 15);
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MultiplyMatrixMatrixFailsWhenSizesAreIncompatible()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix other = testMatrices["Wide2x3"];
            Matrix result = matrix * other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyMatrixMatrixFailsWhenLeftArgumentIsNull()
        {
            Matrix matrix = null;
            Matrix other = testMatrices["Wide2x3"];
            Matrix result = matrix * other;
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MultiplyMatrixMatrixFailsWhenRightArgumentIsNull()
        {
            Matrix matrix = testMatrices["Wide2x3"];
            Matrix other = null;
            Matrix result = matrix * other;
        }

        [Test]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        [Row("Wide2x3", "Square3x3")]
        [Row("Wide2x3", "Tall3x2")]
        [Row("Tall3x2", "Wide2x3")]
        [MultipleAsserts]
        public void CanMultiplyMatrixWithMatrixIntoResult(string nameA, string nameB)
        {
            Matrix A = testMatrices[nameA];
            Matrix B = testMatrices[nameB];
            Matrix C = CreateMatrix(A.RowCount, B.ColumnCount);
            A.Multiply(B, C);

            Assert.AreEqual(C.RowCount, A.RowCount);
            Assert.AreEqual(C.ColumnCount, B.ColumnCount);

            for (int i = 0; i < C.RowCount; i++)
            {
                for (int j = 0; j < C.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(A.GetRow(i) * B.GetColumn(j), C[i, j], 15);
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
            var matrix = testMatrices[name];
            var copy = matrix.Clone();

            copy.Negate();

            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
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
            var matrix = testMatrices[name];
            var copy = matrix.Clone();

            matrix.Negate(copy);

            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(-matrix[i, j], copy[i, j]);
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void NegateIntoResultFailsWhenResultIsNull()
        {
            var matrix = testMatrices["Singular3x3"];
            Matrix copy = null;
            matrix.Negate(copy);
        }

        [Test]
        [ExpectedArgumentException]
        public void NegateIntoResultFailsWhenResultHasMoreRows()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.Negate(target);
        }

        [Test]
        [ExpectedArgumentException]
        public void NegateIntoResultFailsWhenResultHasMoreColumns()
        {
            Matrix matrix = testMatrices["Singular3x3"];
            Matrix target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            matrix.Negate(target);
        }

        [Test]
        public void Append()
        {
            Matrix left = testMatrices["Singular3x3"];
            Matrix right = testMatrices["Tall3x2"];
            Matrix result = left.Append(right);
            Assert.AreEqual(left.ColumnCount + right.ColumnCount, result.ColumnCount);
            Assert.AreEqual(left.RowCount, right.RowCount);

            for (int i = 0; i < result.RowCount; i++)
            {
                for (int j = 0; j < result.ColumnCount; j++)
                {
                    if (j < left.ColumnCount)
                    {
                        Assert.AreEqual(left[i, j], result[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(right[i, j - left.ColumnCount], result[i, j]);
                    }
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void AppendWithRightParameterNullShouldThrowException()
        {
            Matrix left = testMatrices["Square3x3"];
            Matrix right = null;
            left.Append(right);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void AppendWithResultParameterNullShouldThrowException()
        {
            Matrix left = testMatrices["Square3x3"];
            Matrix right = testMatrices["Tall3x2"];
            Matrix result = null;
            left.Append(right, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void AppendingTwoMatricesWithDifferentRowCountShouldThrowException()
        {
            Matrix left = testMatrices["Square3x3"];
            Matrix right = testMatrices["Wide2x3"];
            Matrix result = left.Append(right);
        }

        [Test]
        [ExpectedArgumentException]
        public void AppendingWithInvalidResultMatrixColumnsShouldThrowException()
        {
            Matrix left = testMatrices["Square3x3"];
            Matrix right = testMatrices["Tall3x2"];
            Matrix result = CreateMatrix(3, 2);
            left.Append(right, result);
        }

        [Test]
        public void Stack()
        {
            Matrix top = testMatrices["Square3x3"];
            Matrix bottom = testMatrices["Wide2x3"];
            Matrix result = top.Stack(bottom);
            Assert.AreEqual(top.RowCount + bottom.RowCount, result.RowCount);
            Assert.AreEqual(top.ColumnCount, result.ColumnCount);

            for (int i = 0; i < result.RowCount; i++)
            {
                for (int j = 0; j < result.ColumnCount; j++)
                {
                    if (i < top.RowCount)
                    {
                        Assert.AreEqual(result[i, j], top[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(result[i, j], bottom[i - top.RowCount, j]);
                    }
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void StackWithBottomParameterNullShouldThrowException()
        {
            Matrix top = testMatrices["Square3x3"];
            Matrix bottom = null;
            Matrix result = CreateMatrix(top.RowCount + top.RowCount, top.ColumnCount);
            top.Stack(bottom, result);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void StackWithResultParameterNullShouldThrowException()
        {
            Matrix top = testMatrices["Square3x3"];
            Matrix bottom = testMatrices["Square3x3"];
            Matrix result = null;
            top.Stack(bottom, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void StackTwoMatricesWithDifferentColumnsShouldThrowException()
        {
            Matrix top = testMatrices["Square3x3"];
            Matrix lower = testMatrices["Tall3x2"];
            Matrix result = CreateMatrix(top.RowCount + lower.RowCount, top.ColumnCount);
            top.Stack(lower, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void StackingWithInvalidResultMatrixRowsShouldThrowException()
        {
            Matrix top = testMatrices["Square3x3"];
            Matrix bottom = testMatrices["Wide2x3"];
            Matrix result = CreateMatrix(1, 3);
            top.Stack(bottom, result);
        }

        [Test]
        public void Trace()
        {
            Matrix matrix = testMatrices["Square3x3"];
            double trace = matrix.Trace();
            Assert.AreEqual(6.6, trace);
        }

        [Test]
        [ExpectedArgumentException]
        public void TraceOfNonSquareMatrixShouldThrowException()
        {
            Matrix matrix = testMatrices["Wide2x3"];
            double trace = matrix.Trace();
        }

        [Test]
        public void DiagonalStack()
        {
            Matrix top = testMatrices["Tall3x2"];
            Matrix bottom = testMatrices["Wide2x3"];
            Matrix result = top.DiagonalStack(bottom);
            Assert.AreEqual(top.RowCount + bottom.RowCount, result.RowCount);
            Assert.AreEqual(top.ColumnCount + bottom.ColumnCount, result.ColumnCount);

            for (int i = 0; i < result.RowCount; i++)
            {
                for (int j = 0; j < result.ColumnCount; j++)
                {
                    if (i < top.RowCount && j < top.ColumnCount)
                    {
                        Assert.AreEqual(top[i, j], result[i, j]);
                    }
                    else if (i >= top.RowCount && j >= top.ColumnCount)
                    {
                        Assert.AreEqual(bottom[i - top.RowCount, j - top.ColumnCount], result[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0, result[i, j]);
                    }
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DiagonalStackWithLowerNullShouldThrowException()
        {
            Matrix top = testMatrices["Square3x3"];
            Matrix lower = null;
            top.DiagonalStack(lower);
        }

        [Test]
        public void DiagonalStackWithPassingResult()
        {
            Matrix top = testMatrices["Tall3x2"];
            Matrix bottom = testMatrices["Wide2x3"];
            Matrix result = CreateMatrix(top.RowCount + bottom.RowCount, top.ColumnCount + bottom.ColumnCount);
            top.DiagonalStack(bottom, result);
            Assert.AreEqual(top.RowCount + bottom.RowCount, result.RowCount);
            Assert.AreEqual(top.ColumnCount + bottom.ColumnCount, result.ColumnCount);

            for (int i = 0; i < result.RowCount; i++)
            {
                for (int j = 0; j < result.ColumnCount; j++)
                {
                    if (i < top.RowCount && j < top.ColumnCount)
                    {
                        Assert.AreEqual(top[i, j], result[i, j]);
                    }
                    else if (i >= top.RowCount && j >= top.ColumnCount)
                    {
                        Assert.AreEqual(bottom[i - top.RowCount, j - top.ColumnCount], result[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0, result[i, j]);
                    }
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void DiagonalStackWithResultNullShouldThrowException()
        {
            Matrix top = testMatrices["Square3x3"];
            Matrix lower = testMatrices["Wide2x3"];
            Matrix result = null;
            top.DiagonalStack(lower,result);
        }

        [Test]
        [ExpectedArgumentException]
        public void DiagonalStackWithInvalidResultMatrixShouldThrowException()
        {
            Matrix top = testMatrices["Square3x3"];
            Matrix lower = testMatrices["Wide2x3"];
            Matrix result = CreateMatrix(top.RowCount + lower.RowCount + 2, top.ColumnCount + lower.ColumnCount);
            top.DiagonalStack(lower, result);

        }
    }
}