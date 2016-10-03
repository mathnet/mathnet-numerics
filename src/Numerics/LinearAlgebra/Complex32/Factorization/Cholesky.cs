// <copyright file="Cholesky.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2013 Math.NET
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

using MathNet.Numerics.LinearAlgebra.Factorization;

namespace MathNet.Numerics.LinearAlgebra.Complex32.Factorization
{
    using Numerics;

    /// <summary>
    /// <para>A class which encapsulates the functionality of a Cholesky factorization.</para>
    /// <para>For a symmetric, positive definite matrix A, the Cholesky factorization
    /// is an lower triangular matrix L so that A = L*L'.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the Cholesky factorization is done at construction time. If the matrix is not symmetric
    /// or positive definite, the constructor will throw an exception.
    /// </remarks>
    internal abstract class Cholesky : Cholesky<Complex32>
    {
        protected Cholesky(Matrix<Complex32> factor)
            : base(factor)
        {
        }

        /// <summary>
        /// Gets the determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public override Complex32 Determinant
        {
            get
            {
                var det = Complex32.One;
                for (var j = 0; j < Factor.RowCount; j++)
                {
                    var d = Factor.At(j, j);
                    det *= d*d;
                }

                return det;
            }
        }

        /// <summary>
        /// Gets the log determinant of the matrix for which the Cholesky matrix was computed.
        /// </summary>
        public override Complex32 DeterminantLn
        {
            get
            {
                var det = Complex32.Zero;
                for (var j = 0; j < Factor.RowCount; j++)
                {
                    det += 2.0f*Factor.At(j, j).NaturalLogarithm();
                }

                return det;
            }
        }
    }
}
