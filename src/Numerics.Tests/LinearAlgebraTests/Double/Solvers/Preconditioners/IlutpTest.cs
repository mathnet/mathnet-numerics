// <copyright file="IlutpTest.cs" company="Math.NET">
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
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Double.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.Preconditioners
{
    /// <summary>
    /// Incomplete LU with tpPreconditioner test with drop tolerance and partial pivoting.
    /// </summary>
    [TestFixture, Category("LASolver")]
    public sealed class IlutpPreconditionerTest : PreconditionerTest
    {
        /// <summary>
        /// The drop tolerance.
        /// </summary>
        double _dropTolerance = 0.1;

        /// <summary>
        /// The fill level.
        /// </summary>
        double _fillLevel = 1.0;

        /// <summary>
        /// The pivot tolerance.
        /// </summary>
        double _pivotTolerance = 1.0;

        /// <summary>
        /// Setup default parameters.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _dropTolerance = 0.1;
            _fillLevel = 1.0;
            _pivotTolerance = 1.0;
        }

        /// <summary>
        /// Invoke method from Ilutp class.
        /// </summary>
        /// <typeparam name="T">Type of the return value.</typeparam>
        /// <param name="ilutp">Ilutp instance.</param>
        /// <param name="methodName">Method name.</param>
        /// <returns>Result of the method invocation.</returns>
        static T GetMethod<T>(ILUTPPreconditioner ilutp, string methodName)
        {
            var type = ilutp.GetType();
            var methodInfo = type.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                null,
                CallingConventions.Standard,
                new Type[0],
                null);
            var obj = methodInfo.Invoke(ilutp, null);
            return (T) obj;
        }

        /// <summary>
        /// Get upper triangle.
        /// </summary>
        /// <param name="ilutp">Ilutp instance.</param>
        /// <returns>Upper triangle.</returns>
        static SparseMatrix GetUpperTriangle(ILUTPPreconditioner ilutp)
        {
            return GetMethod<SparseMatrix>(ilutp, "UpperTriangle");
        }

        /// <summary>
        /// Get lower triangle.
        /// </summary>
        /// <param name="ilutp">Ilutp instance.</param>
        /// <returns>Lower triangle.</returns>
        static SparseMatrix GetLowerTriangle(ILUTPPreconditioner ilutp)
        {
            return GetMethod<SparseMatrix>(ilutp, "LowerTriangle");
        }

        /// <summary>
        /// Get pivots.
        /// </summary>
        /// <param name="ilutp">Ilutp instance.</param>
        /// <returns>Pivots array.</returns>
        static int[] GetPivots(ILUTPPreconditioner ilutp)
        {
            return GetMethod<int[]>(ilutp, "Pivots");
        }

        /// <summary>
        /// Create reverse unit matrix.
        /// </summary>
        /// <param name="size">Matrix order.</param>
        /// <returns>Reverse Unit matrix.</returns>
        static SparseMatrix CreateReverseUnitMatrix(int size)
        {
            var matrix = new SparseMatrix(size);
            for (var i = 0; i < size; i++)
            {
                matrix[i, size - 1 - i] = 2;
            }

            return matrix;
        }

        /// <summary>
        /// Create preconditioner (internal)
        /// </summary>
        /// <returns>Ilutp instance.</returns>
        ILUTPPreconditioner InternalCreatePreconditioner()
        {
            var result = new ILUTPPreconditioner
            {
                DropTolerance = _dropTolerance,
                FillLevel = _fillLevel,
                PivotTolerance = _pivotTolerance
            };
            return result;
        }

        /// <summary>
        /// Create preconditioner.
        /// </summary>
        /// <returns>New preconditioner instance.</returns>
        internal override IPreconditioner<double> CreatePreconditioner()
        {
            _pivotTolerance = 0;
            _dropTolerance = 0.0;
            _fillLevel = 100;
            return InternalCreatePreconditioner();
        }

        /// <summary>
        /// Check the result.
        /// </summary>
        /// <param name="preconditioner">Specific preconditioner.</param>
        /// <param name="matrix">Source matrix.</param>
        /// <param name="vector">Initial vector.</param>
        /// <param name="result">Result vector.</param>
        protected override void CheckResult(IPreconditioner<double> preconditioner, SparseMatrix matrix, Vector<double> vector, Vector<double> result)
        {
            Assert.AreEqual(typeof (ILUTPPreconditioner), preconditioner.GetType(), "#01");

            // Compute M * result = product
            // compare vector and product. Should be equal
            var product = new DenseVector(result.Count);
            matrix.Multiply(result, product);
            for (var i = 0; i < product.Count; i++)
            {
                Assert.IsTrue(vector[i].AlmostEqualNumbersBetween(product[i], -Epsilon.Magnitude()), "#02-" + i);
            }
        }

        /// <summary>
        /// Solve returning old vector without pivoting.
        /// </summary>
        [Test]
        public void SolveReturningOldVectorWithoutPivoting()
        {
            const int Size = 10;

            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);

            // set the pivot tolerance to zero so we don't pivot
            _pivotTolerance = 0.0;
            _dropTolerance = 0.0;
            _fillLevel = 100;
            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);
            var result = new DenseVector(vector.Count);
            preconditioner.Approximate(vector, result);
            CheckResult(preconditioner, newMatrix, vector, result);
        }

        /// <summary>
        /// Solve returning old vector with pivoting.
        /// </summary>
        [Test]
        public void SolveReturningOldVectorWithPivoting()
        {
            const int Size = 10;
            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);

            // Set the pivot tolerance to 1 so we always pivot (if necessary)
            _pivotTolerance = 1.0;
            _dropTolerance = 0.0;
            _fillLevel = 100;
            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);
            var result = new DenseVector(vector.Count);
            preconditioner.Approximate(vector, result);
            CheckResult(preconditioner, newMatrix, vector, result);
        }

        /// <summary>
        /// Compare with original dense matrix without pivoting.
        /// </summary>
        [Test]
        public void CompareWithOriginalDenseMatrixWithoutPivoting()
        {
            var sparseMatrix = new SparseMatrix(3);
            sparseMatrix[0, 0] = -1;
            sparseMatrix[0, 1] = 5;
            sparseMatrix[0, 2] = 6;
            sparseMatrix[1, 0] = 3;
            sparseMatrix[1, 1] = -6;
            sparseMatrix[1, 2] = 1;
            sparseMatrix[2, 0] = 6;
            sparseMatrix[2, 1] = 8;
            sparseMatrix[2, 2] = 9;
            var ilu = new ILUTPPreconditioner
            {
                PivotTolerance = 0.0,
                DropTolerance = 0,
                FillLevel = 10
            };
            ilu.Initialize(sparseMatrix);
            var l = GetLowerTriangle(ilu);

            // Assert l is lower triagonal
            for (var i = 0; i < l.RowCount; i++)
            {
                for (var j = i + 1; j < l.RowCount; j++)
                {
                    Assert.IsTrue(0.0.AlmostEqualNumbersBetween(l[i, j], -Epsilon.Magnitude()), "#01-" + i + "-" + j);
                }
            }

            var u = GetUpperTriangle(ilu);

            // Assert u is upper triagonal
            for (var i = 0; i < u.RowCount; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    Assert.IsTrue(0.0.AlmostEqualNumbersBetween(u[i, j], -Epsilon.Magnitude()), "#02-" + i + "-" + j);
                }
            }

            var original = l.Multiply(u);
            for (var i = 0; i < sparseMatrix.RowCount; i++)
            {
                for (var j = 0; j < sparseMatrix.ColumnCount; j++)
                {
                    Assert.IsTrue(sparseMatrix[i, j].AlmostEqualNumbersBetween(original[i, j], -Epsilon.Magnitude()), "#03-" + i + "-" + j);
                }
            }
        }

        /// <summary>
        /// Compare with original dense matrix with pivoting.
        /// </summary>
        [Test]
        public void CompareWithOriginalDenseMatrixWithPivoting()
        {
            var sparseMatrix = new SparseMatrix(3);
            sparseMatrix[0, 0] = -1;
            sparseMatrix[0, 1] = 5;
            sparseMatrix[0, 2] = 6;
            sparseMatrix[1, 0] = 3;
            sparseMatrix[1, 1] = -6;
            sparseMatrix[1, 2] = 1;
            sparseMatrix[2, 0] = 6;
            sparseMatrix[2, 1] = 8;
            sparseMatrix[2, 2] = 9;
            var ilu = new ILUTPPreconditioner
            {
                PivotTolerance = 1.0,
                DropTolerance = 0,
                FillLevel = 10
            };
            ilu.Initialize(sparseMatrix);
            var l = GetLowerTriangle(ilu);
            var u = GetUpperTriangle(ilu);
            var pivots = GetPivots(ilu);
            var p = new SparseMatrix(l.RowCount);
            for (var i = 0; i < p.RowCount; i++)
            {
                p[i, pivots[i]] = 1.0;
            }

            var temp = l.Multiply(u);
            var original = temp.Multiply(p);
            for (var i = 0; i < sparseMatrix.RowCount; i++)
            {
                for (var j = 0; j < sparseMatrix.ColumnCount; j++)
                {
                    Assert.IsTrue(sparseMatrix[i, j].AlmostEqualNumbersBetween(original[i, j], -Epsilon.Magnitude()), "#01-" + i + "-" + j);
                }
            }
        }

        /// <summary>
        /// Solve with pivoting.
        /// </summary>
        [Test]
        public void SolveWithPivoting()
        {
            const int Size = 10;
            var newMatrix = CreateReverseUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);
            var preconditioner = new ILUTPPreconditioner
            {
                PivotTolerance = 1.0,
                DropTolerance = 0,
                FillLevel = 10
            };
            preconditioner.Initialize(newMatrix);
            var result = new DenseVector(vector.Count);
            preconditioner.Approximate(vector, result);
            CheckResult(preconditioner, newMatrix, vector, result);
        }
    }
}
