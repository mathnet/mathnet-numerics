// <copyright file="PreConditionerTest.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.Preconditioners
{
    /// <summary>
    /// Abstract class for preconditioners tests.
    /// </summary>
    public abstract class PreconditionerTest
    {
        /// <summary>
        /// Epsilon value.
        /// </summary>
        protected const double Epsilon = 1e-10;

        /// <summary>
        /// Create unit matrix.
        /// </summary>
        /// <param name="size">Matrix size.</param>
        /// <returns>New unit matrix.</returns>
        internal SparseMatrix CreateUnitMatrix(int size)
        {
            var matrix = new SparseMatrix(size);
            for (var i = 0; i < size; i++)
            {
                matrix[i, i] = 2;
            }
            return matrix;
        }

        /// <summary>
        /// Create standard vector.
        /// </summary>
        /// <param name="size">Size of the vector.</param>
        /// <returns>New vector.</returns>
        protected DenseVector CreateStandardBcVector(int size)
        {
            var vector = new DenseVector(size);
            for (var i = 0; i < size; i++)
            {
                vector[i] = i + 1;
            }
            return vector;
        }

        /// <summary>
        /// Create preconditioner.
        /// </summary>
        /// <returns>New preconditioner instance.</returns>
        internal abstract IPreconditioner<double> CreatePreconditioner();

        /// <summary>
        /// Check the result.
        /// </summary>
        /// <param name="preconditioner">Specific preconditioner.</param>
        /// <param name="matrix">Source matrix.</param>
        /// <param name="vector">Initial vector.</param>
        /// <param name="result">Result vector.</param>
        protected abstract void CheckResult(IPreconditioner<double> preconditioner, SparseMatrix matrix, Vector<double> vector, Vector<double> result);

        /// <summary>
        /// Approximate returning old vector.
        /// </summary>
        [Test]
        public void ApproximateReturningOldVector()
        {
            const int Size = 10;
            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);

            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);

            var result = new DenseVector(vector.Count);
            preconditioner.Approximate(vector, result);

            CheckResult(preconditioner, newMatrix, vector, result);
        }

        /// <summary>
        /// Approximate with a vector with incorrect length throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void ApproximateWithVectorWithIncorrectLengthThrowsArgumentException()
        {
            const int Size = 10;
            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);

            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);

            var result = new DenseVector(vector.Count + 10);
            Assert.That(() => preconditioner.Approximate(vector, result), Throws.ArgumentException);
        }

        /// <summary>
        /// Approximate with non initialized preconditioner throws <c>ArgumentException</c>.
        /// </summary>
        [Test]
        public void ApproximateWithNonInitializedPreconditionerThrowsArgumentException()
        {
            const int Size = 10;
            var vector = CreateStandardBcVector(Size);
            var preconditioner = CreatePreconditioner();
            var result = new DenseVector(vector.Count);
            Assert.That(() => preconditioner.Approximate(vector, result), Throws.ArgumentException);
        }
    }
}
