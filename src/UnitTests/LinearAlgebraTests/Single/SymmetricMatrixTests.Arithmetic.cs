// <copyright file="SymmetricMatrixTests.Arithmetic.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    using System.Collections.Generic;
    using LinearAlgebra.Single;
    using NUnit.Framework;

    /// <summary>
    /// Abstract class with the common set of matrix tests for symmetric matrices
    /// </summary>
    public abstract partial class SymmetricMatrixTests
    {
        /// <summary>
        /// Setup test matrices. 
        /// Singular and Square matrices are overridden here with symmetric ones so that calls to base methods work as intended. 
        /// Additional NonSymmetric matrices are defined for some tests. 
        /// </summary>
        [SetUp]
        public override void SetupMatrices()
        {
            TestData2D = new Dictionary<string, float[,]>
                         {
                             { "Singular3x3", new[,] { { 1.0f, 2.0f, 3.0f }, { 2.0f, 0.0f, 0.0f }, { 3.0f, 0.0f, 0.0f } } }, 
                             { "Square3x3", new[,] { { -1.1f, 2.0f, 3.0f }, { 2.0f, 1.1f, 0.0f }, { 3.0f, 0.0f, 6.6f } } }, 
                             { "Square4x4", new[,] { { 1.1f, 2.0f, -3.0f, 4.4f }, { 2.0f, 5.0f, -6.0f, 7.0f }, { -3.0f, -6.0f, 8.0f, 9.0f }, { 4.4f, 7.0f, 9.0f, 10.0f } } }, 
                             { "Singular4x4", new[,] { { 1.0f, 2.0f, 0.0f, 4.0f }, { 2.0f, 5.0f, 0.0f, 7.0f }, { 0.0f, 0.0f, 0.0f, 0.0f }, { 4.0f, 7.0f, 0.0f, 10.0f } } }, 
                             { "Tall3x2", new[,] { { -1.1f, -2.2f }, { 0.0f, 1.1f }, { -4.4f, 5.5f } } }, 
                             { "Wide2x3", new[,] { { -1.1f, -2.2f, -3.3f }, { 0.0f, 1.1f, 2.2f } } }, 
                             { "Symmetric3x3", new[,] { { 1.0f, 2.0f, 3.0f }, { 2.0f, 2.0f, 0.0f }, { 3.0f, 0.0f, 3.0f } } },
                             { "NonSymmetric3x3", new[,] { { -1.1f, -2.2f, -3.3f }, { 0.0f, 1.1f, 2.2f }, { -4.4f, 5.5f, 6.6f } } },
                             { "NonSymmetric4x4", new[,] { { -1.1f, -2.2f, -3.3f, -4.4f }, { 0.0f, 1.1f, 2.2f, 3.3f }, { 1.0f, 2.1f, 6.2f, 4.3f }, { -4.4f, 5.5f, 6.6f, -7.7f } } },
                             { "IndexTester4x4", new [,] { { 0f, 1f, 3f, 6f }, { 1f, 2f, 4f, 7f }, { 3f, 4f, 5f, 8f }, { 6f, 7f, 8f, 9f } } }
                         };

            TestMatrices = new Dictionary<string, Matrix>();

            foreach (var name in TestData2D.Keys)
            {
                TestMatrices.Add(name, CreateMatrix(TestData2D[name]));
            }
        }

        /// <summary>
        /// Can add a non-symmetric matrix to this symmetric matrix.
        /// </summary>
        /// <param name="mtxA">Matrix A name.</param>
        /// <param name="mtxB">Matrix B name.</param>
        [Test, Sequential]
        public void CanAddNonSymmetricMatrix([Values("Square3x3", "Square4x4")] string mtxA, [Values("NonSymmetric3x3", "NonSymmetric4x4")] string mtxB)
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
        /// Can subtract a non-symmetric matrix from this symmetric matrix.
        /// </summary>
        /// <param name="mtxA">Matrix A name.</param>
        /// <param name="mtxB">Matrix B name.</param>
        [Test, Sequential]
        public void CanSubtractNonSymmetricMatrix([Values("Square3x3", "Square4x4")] string mtxA, [Values("NonSymmetric3x3", "NonSymmetric4x4")] string mtxB)
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
        /// Can compute Frobenius norm.
        /// </summary>
        public override void CanComputeFrobeniusNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 14);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 14);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual(denseMatrix.FrobeniusNorm(), matrix.FrobeniusNorm(), 14);
        }



        /// <summary>
        /// Can compute Infinity norm.
        /// </summary>
        public override void CanComputeInfinityNorm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 14);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 14);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual(denseMatrix.InfinityNorm(), matrix.InfinityNorm(), 14);
        }



        /// <summary>
        /// Can compute L1 norm.
        /// </summary>
        public override void CanComputeL1Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.L1Norm(), matrix.L1Norm(), 14);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.L1Norm(), matrix.L1Norm(), 14);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual(denseMatrix.L1Norm(), matrix.L1Norm(), 14);
        }



        /// <summary>
        /// Can compute L2 norm.
        /// </summary>
        public override void CanComputeL2Norm()
        {
            var matrix = TestMatrices["Square3x3"];
            var denseMatrix = new DenseMatrix(TestData2D["Square3x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.L2Norm(), matrix.L2Norm(), 14);

            matrix = TestMatrices["Wide2x3"];
            denseMatrix = new DenseMatrix(TestData2D["Wide2x3"]);
            AssertHelpers.AlmostEqual(denseMatrix.L2Norm(), matrix.L2Norm(), 14);

            matrix = TestMatrices["Tall3x2"];
            denseMatrix = new DenseMatrix(TestData2D["Tall3x2"]);
            AssertHelpers.AlmostEqual(denseMatrix.L2Norm(), matrix.L2Norm(), 14);
        }
    }
}