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

using System;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    /// <summary>
    /// Abstract class with the common set of matrix tests
    /// </summary>
    public abstract partial class MatrixTests
    {
        /// <summary>
        /// Can multiply with a complex number.
        /// </summary>
        /// <param name="real">Complex real part value.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2.2)]
        public void CanMultiplyWithComplex(double real)
        {
            var value = new Complex(real, 1.0);
            var matrix = TestMatrices["Singular3x3"];
            var clone = matrix.Clone();
            clone = clone.Multiply(value);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j] * value, clone[i, j]);
                }
            }
        }

        /// <summary>
        /// Can multiply with a vector.
        /// </summary>
        [Test]
        public void CanMultiplyWithVector()
        {
            var matrix = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            var y = matrix * x;

            Assert.AreEqual(matrix.RowCount, y.Count);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                var ar = matrix.Row(i);
                var dot = ar * x;
                Assert.AreEqual(dot, y[i]);
            }
        }

        /// <summary>
        /// Can multiply with a vector into a result.
        /// </summary>
        [Test]
        public void CanMultiplyWithVectorIntoResult()
        {
            var matrix = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            var y = new DenseVector(3);
            matrix.Multiply(x, y);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                var ar = matrix.Row(i);
                var dot = ar * x;
                Assert.AreEqual(dot, y[i]);
            }
        }

        /// <summary>
        /// Can multiply with a vector into result when updating input argument.
        /// </summary>
        [Test]
        public void CanMultiplyWithVectorIntoResultWhenUpdatingInputArgument()
        {
            var matrix = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            var y = x;
            matrix.Multiply(x, x);

            Assert.AreSame(y, x);

            y = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            for (var i = 0; i < matrix.RowCount; i++)
            {
                var ar = matrix.Row(i);
                var dot = ar * y;
                Assert.AreEqual(dot, x[i]);
            }
        }

        /// <summary>
        /// Multiply with a vector into too large result throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void MultiplyWithVectorIntoLargerResultThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            Vector<Complex> y = new DenseVector(4);
            Assert.That(() => matrix.Multiply(x, y), Throws.ArgumentException);
        }

        /// <summary>
        /// Can left multiply with a complex.
        /// </summary>
        /// <param name="real">Complex real value.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2.2)]
        public void CanOperatorLeftMultiplyWithComplex(double real)
        {
            var value = new Complex(real, 1.0);
            var matrix = TestMatrices["Singular3x3"];
            var clone = value * matrix;

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(value * matrix[i, j], clone[i, j]);
                }
            }
        }

        /// <summary>
        /// Can right multiply with a Complex.
        /// </summary>
        /// <param name="real">Complex real value.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2.2)]
        public void CanOperatorRightMultiplyWithComplex(double real)
        {
            var value = new Complex(real, 1.0);
            var matrix = TestMatrices["Singular3x3"];
            var clone = matrix * value;

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j] * value, clone[i, j]);
                }
            }
        }

        /// <summary>
        /// Can multiply with a Complex into result.
        /// </summary>
        /// <param name="real">Complex real value.</param>
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2.2)]
        public void CanMultiplyWithComplexIntoResult(double real)
        {
            var value = new Complex(real, 1.0);
            var matrix = TestMatrices["Singular3x3"];
            var result = matrix.Clone();
            matrix.Multiply(value, result);

            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j] * value, result[i, j]);
                }
            }
        }

        /// <summary>
        /// Multiply with a scalar when result has more rows throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void MultiplyWithScalarWhenResultHasMoreRowsThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var result = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            Assert.That(() => matrix.Multiply(2.3, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Multiply with a scalar when result has more columns throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void MultiplyWithScalarWhenResultHasMoreColumnsThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var result = CreateMatrix(matrix.RowCount, matrix.ColumnCount + 1);
            Assert.That(() => matrix.Multiply(2.3, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Can add a matrix.
        /// </summary>
        /// <param name="mtxA">Matrix A name.</param>
        /// <param name="mtxB">Matrix B name.</param>
        [TestCase("Singular3x3", "Square3x3")]
        [TestCase("Singular4x4", "Square4x4")]
        public void CanAddMatrix(string mtxA, string mtxB)
        {
            var matrixA = TestMatrices[mtxA];
            var matrixB = TestMatrices[mtxB];

            var matrix = matrixA.Clone();
            matrix = matrix.Add(matrixB);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], matrixA[i, j] + matrixB[i, j]);
                }
            }
        }

        /// <summary>
        /// Can add a matrix.
        /// </summary>
        /// <param name="mtx">Matrix name.</param>
        [TestCase("Square3x3")]
        [TestCase("Tall3x2")]
        public void CanAddMatrixToSelf(string mtx)
        {
            var matrix = TestMatrices[mtx].Clone();

            var result = matrix.Add(matrix);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i, j], 2*matrix[i, j]);
                }
            }
        }

        /// <summary>
        /// Can subtract a matrix.
        /// </summary>
        /// <param name="mtx">Matrix name.</param>
        [TestCase("Square3x3")]
        [TestCase("Tall3x2")]
        public void CanSubtractMatrixFromSelf(string mtx)
        {
            var matrix = TestMatrices[mtx].Clone();

            var result = matrix.Subtract(matrix);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i, j], Complex.Zero);
                }
            }
        }

        /// <summary>
        /// Adding a matrix with fewer columns throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void AddMatrixWithFewerColumnsThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            Assert.That(() => matrix.Add(other), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Adding a matrix with fewer rows throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void AddMatrixWithFewerRowsThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            Assert.That(() => matrix.Add(other), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Add matrices using "+" operator.
        /// </summary>
        /// <param name="mtxA">Matrix A name.</param>
        /// <param name="mtxB">Matrix B name.</param>
        [TestCase("Singular3x3", "Square3x3")]
        [TestCase("Singular4x4", "Square4x4")]
        public void CanAddUsingOperator(string mtxA, string mtxB)
        {
            var matrixA = TestMatrices[mtxA];
            var matrixB = TestMatrices[mtxB];

            var result = matrixA + matrixB;
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i, j], matrixA[i, j] + matrixB[i, j]);
                }
            }
        }

        /// <summary>
        /// Add operator when right side has fewer columns throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void AddOperatorWhenRightSideHasFewerColumnsThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = matrix + other; });
        }

        /// <summary>
        /// Add operator when right side has fewer rows throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void AddOperatorWhenRightSideHasFewerRowsThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = matrix + other; });
        }

        /// <summary>
        /// Can subtract a matrix.
        /// </summary>
        /// <param name="mtxA">Matrix A name.</param>
        /// <param name="mtxB">Matrix B name.</param>
        [TestCase("Singular3x3", "Square3x3")]
        [TestCase("Singular4x4", "Square4x4")]
        public void CanSubtractMatrix(string mtxA, string mtxB)
        {
            var matrixA = TestMatrices[mtxA];
            var matrixB = TestMatrices[mtxB];

            var matrix = matrixA.Clone();
            matrix = matrix.Subtract(matrixB);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                for (var j = 0; j < matrix.ColumnCount; j++)
                {
                    Assert.AreEqual(matrix[i, j], matrixA[i, j] - matrixB[i, j]);
                }
            }
        }

        /// <summary>
        /// Subtract a matrix when right side has fewer columns throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SubtractMatrixWithFewerColumnsThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            Assert.That(() => matrix.Subtract(other), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Subtract a matrix when right side has fewer rows throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void SubtractMatrixWithFewerRowsThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            Assert.That(() => matrix.Subtract(other), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can subtract a matrix using "-" operator.
        /// </summary>
        /// <param name="mtxA">Matrix A name.</param>
        /// <param name="mtxB">Matrix B name.</param>
        [TestCase("Singular3x3", "Square3x3")]
        [TestCase("Singular4x4", "Square4x4")]
        public void CanSubtractUsingOperator(string mtxA, string mtxB)
        {
            var matrixA = TestMatrices[mtxA];
            var matrixB = TestMatrices[mtxB];

            var result = matrixA - matrixB;
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    Assert.AreEqual(result[i, j], matrixA[i, j] - matrixB[i, j]);
                }
            }
        }

        /// <summary>
        /// Subtract operator when right side has fewer columns throws <c>ArgumentOutOfRangeException</c>
        /// </summary>
        [Test]
        public void SubtractOperatorWhenRightSideHasFewerColumnsThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = matrix - other; });
        }

        /// <summary>
        /// Subtract operator when right side has fewer rows <c>ArgumentOutOfRangeException</c>
        /// </summary>
        [Test]
        public void SubtractOperatorWhenRightSideHasFewerRowsThrowsArgumentOutOfRangeException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            Assert.Throws<ArgumentOutOfRangeException>(() => { var result = matrix - other; });
        }

        /// <summary>
        /// Can multiply a matrix with matrix.
        /// </summary>
        /// <param name="nameA">Matrix A name.</param>
        /// <param name="nameB">Matrix B name.</param>
        [TestCase("Singular3x3", "Square3x3")]
        [TestCase("Singular4x4", "Square4x4")]
        [TestCase("Wide2x3", "Square3x3")]
        [TestCase("Wide2x3", "Tall3x2")]
        [TestCase("Tall3x2", "Wide2x3")]
        public void CanMultiplyMatrixWithMatrix(string nameA, string nameB)
        {
            var matrixA = TestMatrices[nameA];
            var matrixB = TestMatrices[nameB];
            var matrixC = matrixA * matrixB;

            Assert.AreEqual(matrixC.RowCount, matrixA.RowCount);
            Assert.AreEqual(matrixC.ColumnCount, matrixB.ColumnCount);

            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixA.Row(i) * matrixB.Column(j), matrixC[i, j], 12);
                }
            }
        }

        /// <summary>
        /// Can transpose and multiply a matrix with matrix.
        /// </summary>
        /// <param name="nameA">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Singular4x4")]
        [TestCase("Wide2x3")]
        [TestCase("Tall3x2")]
        public void CanTransposeAndMultiplyMatrixWithMatrix(string nameA)
        {
            var matrixA = TestMatrices[nameA];
            var matrixB = TestMatrices[nameA];
            var matrixC = matrixA.TransposeAndMultiply(matrixB);

            Assert.AreEqual(matrixC.RowCount, matrixA.RowCount);
            Assert.AreEqual(matrixC.ColumnCount, matrixB.RowCount);

            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixA.Row(i) * matrixB.Row(j), matrixC[i, j], 12);
                }
            }
        }

        /// <summary>
        /// Can transpose and multiply a matrix with differing dimensions.
        /// </summary>
        [Test]
        public void CanTransposeAndMultiplyWithDifferingDimensions()
        {
            var matrixA = TestMatrices["Tall3x2"];
            var matrixB = CreateMatrix(5, 2);
            var count = 1;
            for (var row = 0; row < matrixB.RowCount; row++)
            {
                for (var col = 0; col < matrixB.ColumnCount; col++)
                {
                    if (row == col)
                    {
                        matrixB[row, col] = count++;
                    }
                }
            }

            var matrixC = matrixA.TransposeAndMultiply(matrixB);

            Assert.AreEqual(matrixC.RowCount, matrixA.RowCount);
            Assert.AreEqual(matrixC.ColumnCount, matrixB.RowCount);

            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixA.Row(i) * matrixB.Row(j), matrixC[i, j], 14);
                }
            }
        }

        /// <summary>
        /// Transpose and multiply a matrix with matrix of incompatible size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void TransposeAndMultiplyMatrixMatrixWithIncompatibleSizesThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Tall3x2"];
            Assert.That(() => matrix.TransposeAndMultiply(other), Throws.ArgumentException);
        }

        /// <summary>
        /// Can transpose and multiply a matrix with matrix into a result matrix.
        /// </summary>
        /// <param name="nameA">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Singular4x4")]
        [TestCase("Wide2x3")]
        [TestCase("Tall3x2")]
        public void CanTransposeAndMultiplyMatrixWithMatrixIntoResult(string nameA)
        {
            var matrixA = TestMatrices[nameA];
            var matrixB = TestMatrices[nameA];
            var matrixC = CreateMatrix(matrixA.RowCount, matrixB.RowCount);
            matrixA.TransposeAndMultiply(matrixB, matrixC);

            Assert.AreEqual(matrixC.RowCount, matrixA.RowCount);
            Assert.AreEqual(matrixC.ColumnCount, matrixB.RowCount);

            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixA.Row(i) * matrixB.Row(j), matrixC[i, j], 12);
                }
            }
        }

        /// <summary>
        /// Multiply a matrix with incompatible size matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void MultiplyMatrixMatrixWithIncompatibleSizesThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var other = TestMatrices["Wide2x3"];
            Assert.Throws<ArgumentException>(() => { var result = matrix * other; });
        }

        /// <summary>
        /// Can multiply a matrix with matrix into a result matrix.
        /// </summary>
        /// <param name="nameA">Matrix A name.</param>
        /// <param name="nameB">Matrix B name.</param>
        [TestCase("Singular3x3", "Square3x3")]
        [TestCase("Singular4x4", "Square4x4")]
        [TestCase("Wide2x3", "Square3x3")]
        [TestCase("Wide2x3", "Tall3x2")]
        [TestCase("Tall3x2", "Wide2x3")]
        public virtual void CanMultiplyMatrixWithMatrixIntoResult(string nameA, string nameB)
        {
            var matrixA = TestMatrices[nameA];
            var matrixB = TestMatrices[nameB];
            var matrixC = CreateMatrix(matrixA.RowCount, matrixB.ColumnCount);
            matrixA.Multiply(matrixB, matrixC);

            Assert.AreEqual(matrixC.RowCount, matrixA.RowCount);
            Assert.AreEqual(matrixC.ColumnCount, matrixB.ColumnCount);

            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixA.Row(i) * matrixB.Column(j), matrixC[i, j], 12);
                }
            }
        }

        /// <summary>
        /// Can multiply transposed matrix with a vector.
        /// </summary>
        [Test]
        public void CanTransposeThisAndMultiplyWithVector()
        {
            var matrix = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            var y = matrix.TransposeThisAndMultiply(x);

            Assert.AreEqual(matrix.ColumnCount, y.Count);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                var ar = matrix.Column(j);
                var dot = ar * x;
                Assert.AreEqual(dot, y[j]);
            }
        }

        /// <summary>
        /// Can multiply transposed matrix with a vector into a result.
        /// </summary>
        [Test]
        public void CanTransposeThisAndMultiplyWithVectorIntoResult()
        {
            var matrix = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            var y = new DenseVector(3);
            matrix.TransposeThisAndMultiply(x, y);

            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                var ar = matrix.Column(j);
                var dot = ar * x;
                Assert.AreEqual(dot, y[j]);
            }
        }

        /// <summary>
        /// Can multiply transposed matrix with a vector into result when updating input argument.
        /// </summary>
        [Test]
        public void CanTransposeThisAndMultiplyWithVectorIntoResultWhenUpdatingInputArgument()
        {
            var matrix = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            var y = x;
            matrix.TransposeThisAndMultiply(x, x);

            Assert.AreSame(y, x);

            y = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            for (var j = 0; j < matrix.ColumnCount; j++)
            {
                var ar = matrix.Column(j);
                var dot = ar * y;
                Assert.AreEqual(dot, x[j]);
            }
        }

        /// <summary>
        /// Multiply transposed matrix with a vector into too large result throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void TransposeThisAndMultiplyWithVectorIntoLargerResultThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var x = new DenseVector(new[] { new Complex(1, 1), new Complex(2, 1), new Complex(3, 1) });
            Vector<Complex> y = new DenseVector(4);
            Assert.That(() => matrix.TransposeThisAndMultiply(x, y), Throws.ArgumentException);
        }

        /// <summary>
        /// Can multiply transposed matrix with another matrix.
        /// </summary>
        /// <param name="nameA">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Singular4x4")]
        [TestCase("Wide2x3")]
        [TestCase("Tall3x2")]
        public void CanTransposeThisAndMultiplyMatrixWithMatrix(string nameA)
        {
            var matrixA = TestMatrices[nameA];
            var matrixB = TestMatrices[nameA];
            var matrixC = matrixA.TransposeThisAndMultiply(matrixB);

            Assert.AreEqual(matrixC.RowCount, matrixA.ColumnCount);
            Assert.AreEqual(matrixC.ColumnCount, matrixB.ColumnCount);

            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixA.Column(i) * matrixB.Column(j), matrixC[i, j], 12);
                }
            }
        }

        /// <summary>
        /// Multiply the transpose of matrix with another matrix of incompatible size throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void TransposeThisAndMultiplyMatrixMatrixWithIncompatibleSizesThrowsArgumentException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = TestMatrices["Singular3x3"];
            Assert.That(() => matrix.TransposeThisAndMultiply(other), Throws.ArgumentException);
        }

        /// <summary>
        /// Multiply transpose of this matrix with another matrix into a result matrix.
        /// </summary>
        /// <param name="nameA">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Singular4x4")]
        [TestCase("Wide2x3")]
        [TestCase("Tall3x2")]
        public void CanTransposeThisAndMultiplyMatrixWithMatrixIntoResult(string nameA)
        {
            var matrixA = TestMatrices[nameA];
            var matrixB = TestMatrices[nameA];
            var matrixC = CreateMatrix(matrixA.ColumnCount, matrixB.ColumnCount);
            matrixA.TransposeThisAndMultiply(matrixB, matrixC);

            Assert.AreEqual(matrixC.RowCount, matrixA.ColumnCount);
            Assert.AreEqual(matrixC.ColumnCount, matrixB.ColumnCount);

            for (var i = 0; i < matrixC.RowCount; i++)
            {
                for (var j = 0; j < matrixC.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(matrixA.Column(i) * matrixB.Column(j), matrixC[i, j], 12);
                }
            }
        }

        /// <summary>
        /// Can negate a matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Singular4x4")]
        [TestCase("Wide2x3")]
        [TestCase("Wide2x3")]
        [TestCase("Tall3x2")]
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

        /// <summary>
        /// Can negate a matrix into a result matrix.
        /// </summary>
        /// <param name="name">Matrix name.</param>
        [TestCase("Singular3x3")]
        [TestCase("Singular4x4")]
        [TestCase("Wide2x3")]
        [TestCase("Wide2x3")]
        [TestCase("Tall3x2")]
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

        /// <summary>
        /// Negate into a result matrix with more rows throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void NegateIntoResultWithMoreRowsThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            Assert.That(() => matrix.Negate(target), Throws.ArgumentException);
        }

        /// <summary>
        /// Negate into a result matrix with more rows throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void NegateIntoResultWithMoreColumnsThrowsArgumentException()
        {
            var matrix = TestMatrices["Singular3x3"];
            var target = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            Assert.That(() => matrix.Negate(target), Throws.ArgumentException);
        }

        /// <summary>
        /// Can calculate Kronecker product.
        /// </summary>
        [Test]
        public void CanKroneckerProduct()
        {
            var matrixA = TestMatrices["Wide2x3"];
            var matrixB = TestMatrices["Square3x3"];
            var result = CreateMatrix(matrixA.RowCount * matrixB.RowCount, matrixA.ColumnCount * matrixB.ColumnCount);
            matrixA.KroneckerProduct(matrixB, result);
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    for (var ii = 0; ii < matrixB.RowCount; ii++)
                    {
                        for (var jj = 0; jj < matrixB.ColumnCount; jj++)
                        {
                            Assert.AreEqual(result[(i * matrixB.RowCount) + ii, (j * matrixB.ColumnCount) + jj], matrixA[i, j] * matrixB[ii, jj]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Can calculate Kronecker product into a result matrix.
        /// </summary>
        [Test]
        public void CanKroneckerProductIntoResult()
        {
            var matrixA = TestMatrices["Wide2x3"];
            var matrixB = TestMatrices["Square3x3"];
            var result = matrixA.KroneckerProduct(matrixB);
            for (var i = 0; i < matrixA.RowCount; i++)
            {
                for (var j = 0; j < matrixA.ColumnCount; j++)
                {
                    for (var ii = 0; ii < matrixB.RowCount; ii++)
                    {
                        for (var jj = 0; jj < matrixB.ColumnCount; jj++)
                        {
                            Assert.AreEqual(result[(i * matrixB.RowCount) + ii, (j * matrixB.ColumnCount) + jj], matrixA[i, j] * matrixB[ii, jj]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Can normalize columns of a matrix.
        /// </summary>
        /// <param name="p">The norm under which to normalize the columns under.</param>
        [TestCase(1)]
        [TestCase(2)]
        public void CanNormalizeColumns(int p)
        {
            var matrix = TestMatrices["Square4x4"];
            var result = matrix.NormalizeColumns(p);
            for (var j = 0; j < result.ColumnCount; j++)
            {
                var col = result.Column(j);
                AssertHelpers.AlmostEqual(Complex.One, col.Norm(p), 12);
            }
        }

        /// <summary>
        /// Normalize columns with wrong parameter throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void NormalizeColumnsWithWrongParameterThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => TestMatrices["Square4x4"].NormalizeColumns(-4), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can normalize rows of a matrix.
        /// </summary>
        /// <param name="p">The norm under which to normalize the rows under.</param>
        [TestCase(1)]
        [TestCase(2)]
        public void CanNormalizeRows(int p)
        {
            var matrix = TestMatrices["Square4x4"].NormalizeRows(p);
            for (var i = 0; i < matrix.RowCount; i++)
            {
                var row = matrix.Row(i);
                AssertHelpers.AlmostEqual(Complex.One, row.Norm(p), 12);
            }
        }

        /// <summary>
        /// Normalize rows with wrong parameter throws <c>ArgumentOutOfRangeException</c>.
        /// </summary>
        [Test]
        public void NormalizeRowsWithWrongParameterThrowsArgumentOutOfRangeException()
        {
            Assert.That(() => TestMatrices["Square4x4"].NormalizeRows(-4), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can pointwise multiply matrices into a result matrix.
        /// </summary>
        [Test]
        public void CanPointwiseMultiplyIntoResult()
        {
            foreach (var data in TestMatrices.Values)
            {
                var other = data.Clone();
                var result = data.Clone();
                data.PointwiseMultiply(other, result);
                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        var value = data[i, j]*other[i, j];
                        Assert.AreEqual(value.Real, result[i, j].Real, 1e-12);
                        Assert.AreEqual(value.Imaginary, result[i, j].Imaginary, 1e-12);
                    }
                }

                result = data.PointwiseMultiply(other);
                for (var i = 0; i < data.RowCount; i++)
                {
                    for (var j = 0; j < data.ColumnCount; j++)
                    {
                        var value = data[i, j] * other[i, j];
                        Assert.AreEqual(value.Real, result[i, j].Real, 1e-12);
                        Assert.AreEqual(value.Imaginary, result[i, j].Imaginary, 1e-12);
                    }
                }
            }
        }

        /// <summary>
        /// Pointwise multiply matrices with invalid dimensions into a result throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void PointwiseMultiplyWithInvalidDimensionsIntoResultThrowsArgumentException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            var result = matrix.Clone();
            Assert.That(() => matrix.PointwiseMultiply(other, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Pointwise multiply matrices with invalid result dimensions throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void PointwiseMultiplyWithInvalidResultDimensionsThrowsArgumentException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            var result = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            Assert.That(() => matrix.PointwiseMultiply(other, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Can pointwise divide matrices into a result matrix.
        /// </summary>
        [Test]
        public virtual void CanPointwiseDivideIntoResult()
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

        /// <summary>
        /// Pointwise divide matrices with invalid dimensions into a result throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void PointwiseDivideWithInvalidDimensionsIntoResultThrowsArgumentException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            var result = matrix.Clone();
            Assert.That(() => matrix.PointwiseDivide(other, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Pointwise divide matrices with invalid result dimensions throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void PointwiseDivideWithInvalidResultDimensionsThrowsArgumentException()
        {
            var matrix = TestMatrices["Wide2x3"];
            var other = matrix.Clone();
            var result = CreateMatrix(matrix.RowCount + 1, matrix.ColumnCount);
            Assert.That(() => matrix.PointwiseDivide(other, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Create random matrix with non-positive number of rows throw <c>ArgumentException</c>.
        /// </summary>
        /// <param name="numberOfRows">Number of rows.</param>
        [TestCase(0)]
        [TestCase(-2)]
        public void RandomWithNonPositiveNumberOfRowsThrowsArgumentException(int numberOfRows)
        {
            Assert.That(() => DenseMatrix.CreateRandom(numberOfRows, 4, new ContinuousUniform()), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        /// <summary>
        /// Can trace.
        /// </summary>
        [Test]
        public void CanTrace()
        {
            var matrix = TestMatrices["Square3x3"];
            var trace = matrix.Trace();
            Assert.AreEqual(new Complex(6.6, 3), trace);
        }

        /// <summary>
        /// Trace of non-square matrix throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void TraceOfNonSquareMatrixThrowsArgumentException()
        {
            var matrix = TestMatrices["Wide2x3"];
            Assert.That(() => matrix.Trace(), Throws.ArgumentException);
        }
    }
}
