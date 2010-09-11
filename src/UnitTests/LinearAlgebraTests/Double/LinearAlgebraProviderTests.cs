// <copyright file="LinearAlgebraProviderTests.cs" company="Math.NET">
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
    using Algorithms.LinearAlgebra;
    using LinearAlgebra.Double;
    using LinearAlgebra.Generic;
    using MbUnit.Framework;

    [TestFixture]
    public abstract class LinearAlgebraProviderTests : MatrixLoader
    {
        protected ILinearAlgebraProvider<double> Provider{ get; set;}

        private double[] y = new [] { 1.1, 2.2, 3.3, 4.4, 5.5 };
        private double[] x = new[] { 6.6, 7.7, 8.8, 9.9, 10.1 };

        [Test, MultipleAsserts]
        public void CanAddVectorToScaledVector()
        {
            var result = new double[y.Length];
            Array.Copy(y, result, y.Length);

            Provider.AddVectorToScaledVector(result, 0, x);
            for (var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i], result[i]);
            }

            Array.Copy(y, result, y.Length);
            Provider.AddVectorToScaledVector(result, 1, x);
            for (var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i] + x[i], result[i]);
            }

            Array.Copy(y, result, y.Length);
            Provider.AddVectorToScaledVector(result, Math.PI, x);
            for( var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i] + Math.PI * x[i], result[i]);
            }
        }

        [Test, MultipleAsserts]
        public void CanScaleArray()
        {
            var result = new double[y.Length];

            Array.Copy(y, result, y.Length);
            Provider.ScaleArray(1, result);
            for (var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i], result[i]);
            }

            Array.Copy(y, result, y.Length);
            Provider.ScaleArray(Math.PI, result);
            for (var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i] * Math.PI, result[i]);
            }
        }

        [Test]
        public void CanComputeDotProduct()
        {
            var result = Provider.DotProduct(x, y);
            AssertHelpers.AlmostEqual(152.35, result, 15);
        }

        [Test]
        public void CanAddArrays()
        {
            var result = new double[y.Length];
            Provider.AddArrays(x, y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(x[i] + y[i], result[i]);
            }
        }

        [Test]
        public void CanSubtractArrays()
        {
            var result = new double[y.Length];
            Provider.SubtractArrays(x, y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(x[i] - y[i], result[i]);
            }
        }
        
        [Test]
        public void CanPointWiseMultiplyArrays()
        {
            var result = new double[y.Length];
            Provider.PointWiseMultiplyArrays(x, y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(x[i] * y[i], result[i]);
            }
        }

        [Test, Ignore]
        public void CanComputeMatrixNorm(Norm norm, double[] matrix){}

        [Test, Ignore]
        public void CanComputeMatrixNorm(Norm norm, double[] matrix, double[] work)
        {

        }

        [Test, MultipleAsserts]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        [Row("Wide2x3", "Square3x3")]
        [Row("Wide2x3", "Tall3x2")]
        [Row("Tall3x2", "Wide2x3")]
        public void CanMatrixMultiply(string nameX, string nameY)
        {
            var x = (DenseMatrix)TestMatrices[nameX];
            var y = (DenseMatrix)TestMatrices[nameY];
            var c = (DenseMatrix)CreateMatrix(x.RowCount, y.ColumnCount);

            Provider.MatrixMultiply(x.Data, x.RowCount, x.ColumnCount, y.Data, y.RowCount, y.ColumnCount, c.Data);

            for (int i = 0; i < c.RowCount; i++)
            {
                for (int j = 0; j < c.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(x.Row(i) * y.Column(j), c[i, j], 15);
                }
            }
        }

        [Test, MultipleAsserts]
        [Row("Singular3x3", "Square3x3")]
        [Row("Singular4x4", "Square4x4")]
        [Row("Wide2x3", "Square3x3")]
        [Row("Wide2x3", "Tall3x2")]
        [Row("Tall3x2", "Wide2x3")]
        public void CanMatrixMultiplyWithUpdate(string nameX, string nameY)
        {
            var x = (DenseMatrix)TestMatrices[nameX];
            var y = (DenseMatrix)TestMatrices[nameY];
            var c = (DenseMatrix)CreateMatrix(x.RowCount, y.ColumnCount);

            Provider.MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 2.0, x.Data, x.RowCount, x.ColumnCount, y.Data, y.RowCount, y.ColumnCount, 1.0, c.Data);

            for (int i = 0; i < c.RowCount; i++)
            {
                for (int j = 0; j < c.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(2 * (x.Row(i) * y.Column(j)), c[i, j], 15);
                }
            }

            Provider.MatrixMultiplyWithUpdate(Transpose.DontTranspose, Transpose.DontTranspose, 2.0, x.Data, x.RowCount, x.ColumnCount, y.Data, y.RowCount, y.ColumnCount, 1.0, c.Data);

            for (int i = 0; i < c.RowCount; i++)
            {
                for (int j = 0; j < c.ColumnCount; j++)
                {
                    AssertHelpers.AlmostEqual(4 * (x.Row(i) * y.Column(j)), c[i, j], 15);
                }
            }
        }

        [Test, Ignore]
        public void CanComputeLUFactor(double[] a, int[] ipiv)
        {

        }

        [Test, Ignore]
        public void CanComputeLUInverse(double[] a)
        {

        }

        public void CanComputeLUInverseFactored(double[] a, int[] ipiv)
        {

        }

        [Test, Ignore]
        public void CanComputeLUInverse(double[] a, double[] work)
        {

        }
        
        [Test, Ignore]
        public void CanComputeLUInverseFactored(double[] a, int[] ipiv, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeLUSolve(int columnsOfB, double[] a, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeLUSolveFactored(int columnsOfB, double[] a, int ipiv, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeLUSolve(Transpose transposeA, int columnsOfB, double[] a, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeLUSolveFactored(Transpose transposeA, int columnsOfB, double[] a, int ipiv, double[] b)
        {

        }

        [Test]
        public void CanComputeCholeskyFactor()
        {
            var matrix = new double[] { 1, 1, 1, 1, 1, 5, 5, 5, 1, 5, 14, 14, 1, 5, 14, 15 };
            Provider.CholeskyFactor(matrix, 4);
            Assert.AreEqual(matrix[0], 1);
            Assert.AreEqual(matrix[1], 1);
            Assert.AreEqual(matrix[2], 1);
            Assert.AreEqual(matrix[3], 1);
            Assert.AreEqual(matrix[4], 0);
            Assert.AreEqual(matrix[5], 2);
            Assert.AreEqual(matrix[6], 2);
            Assert.AreEqual(matrix[7], 2);
            Assert.AreEqual(matrix[8], 0);
            Assert.AreEqual(matrix[9], 0);
            Assert.AreEqual(matrix[10], 3);
            Assert.AreEqual(matrix[11], 3);
            Assert.AreEqual(matrix[12], 0);
            Assert.AreEqual(matrix[13], 0);
            Assert.AreEqual(matrix[14], 0);
            Assert.AreEqual(matrix[15], 1);
        }

        [Test, Ignore]
        public void CanComputeCholeskySolve(int columnsOfB, double[] a, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeCholeskySolveFactored(int columnsOfB, double[] a, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeQRFactor(double[] r, double[] q)
        {

        }

        [Test, Ignore]
        public void CanComputeQRFactor(double[] r, double[] q, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeQRSolve(int columnsOfB, double[] r, double[] q, double[] b, double[] x)
        {

        }

        [Test, Ignore]
        public void CanComputeQRSolve(int columnsOfB, double[] r, double[] q, double[] b, double[] x, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeQRSolveFactored(int columnsOfB, double[] q, double[] r, double[] b, double[] x)
        {

        }

        [Test, Ignore]
        public void CanComputeSinguarValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt)
        {

        }

        [Test, Ignore]
        public void CanComputeSingularValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeSvdSolve(double[] a, double[] s, double[] u, double[] vt, double[] b, double[] x)
        {

        }

        [Test, Ignore]
        public void CanComputeSvdSolve(double[] a, double[] s, double[] u, double[] vt, double[] b, double[] x, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeSvdSolveFactored(int columnsOfB, double[] s, double[] u, double[] vt, double[] b, double[] x)
        {

        }

        protected override Matrix<double> CreateMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        protected override Matrix<double> CreateMatrix(double[,] data)
        {
            return new DenseMatrix(data);
        }

        protected override Vector<double> CreateVector(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector<double> CreateVector(double[] data)
        {
            return new DenseVector(data);
        }
    }
}