// <copyright file="DiagonalTest.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Complex32;
using MathNet.Numerics.LinearAlgebra.Complex32.Solvers;
using MathNet.Numerics.LinearAlgebra.Solvers;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.Preconditioners
{
    using Numerics;

    /// <summary>
    /// Diagonal preconditioner test.
    /// </summary>
    [TestFixture, Category("LASolver")]
    public sealed class DiagonalTest : PreconditionerTest
    {
        /// <summary>
        /// Create preconditioner.
        /// </summary>
        /// <returns>New preconditioner instance.</returns>
        internal override IPreconditioner<Complex32> CreatePreconditioner()
        {
            return new DiagonalPreconditioner();
        }

        /// <summary>
        /// Check the result.
        /// </summary>
        /// <param name="preconditioner">Specific preconditioner.</param>
        /// <param name="matrix">Source matrix.</param>
        /// <param name="vector">Initial vector.</param>
        /// <param name="result">Result vector.</param>
        protected override void CheckResult(IPreconditioner<Complex32> preconditioner, SparseMatrix matrix, Vector<Complex32> vector, Vector<Complex32> result)
        {
            Assert.AreEqual(typeof (DiagonalPreconditioner), preconditioner.GetType(), "#01");

            // Compute M * result = product
            // compare vector and product. Should be equal
            var product = new DenseVector(result.Count);
            matrix.Multiply(result, product);
            for (var i = 0; i < product.Count; i++)
            {
                Assert.IsTrue(vector[i].Real.AlmostEqualNumbersBetween(product[i].Real, -Epsilon.Magnitude()), "#02-" + i);
                Assert.IsTrue(vector[i].Imaginary.AlmostEqualNumbersBetween(product[i].Imaginary, -Epsilon.Magnitude()), "#03-" + i);
            }
        }
    }
}
