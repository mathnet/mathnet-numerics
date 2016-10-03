﻿// <copyright file="Evd.cs" company="Math.NET">
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

namespace MathNet.Numerics.LinearAlgebra.Complex.Factorization
{

#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    /// <summary>
    /// Eigenvalues and eigenvectors of a real matrix.
    /// </summary>
    /// <remarks>
    /// If A is symmetric, then A = V*D*V' where the eigenvalue matrix D is
    /// diagonal and the eigenvector matrix V is orthogonal.
    /// I.e. A = V*D*V' and V*VT=I.
    /// If A is not symmetric, then the eigenvalue matrix D is block diagonal
    /// with the real eigenvalues in 1-by-1 blocks and any complex eigenvalues,
    /// lambda + i*mu, in 2-by-2 blocks, [lambda, mu; -mu, lambda].  The
    /// columns of V represent the eigenvectors in the sense that A*V = V*D,
    /// i.e. A.Multiply(V) equals V.Multiply(D).  The matrix V may be badly
    /// conditioned, or even singular, so the validity of the equation
    /// A = V*D*Inverse(V) depends upon V.Condition().
    /// </remarks>
    internal abstract class Evd : Evd<Complex>
    {
        protected Evd(Matrix<Complex> eigenVectors, Vector<Complex> eigenValues, Matrix<Complex> blockDiagonal, bool isSymmetric)
            : base(eigenVectors, eigenValues, blockDiagonal, isSymmetric)
        {
        }

        /// <summary>
        /// Gets the absolute value of determinant of the square matrix for which the EVD was computed.
        /// </summary>
        public override Complex Determinant
        {
            get
            {
                var det = Complex.One;
                for (var i = 0; i < EigenValues.Count; i++)
                {
                    det *= EigenValues[i];

                    if (EigenValues[i].AlmostEqual(Complex.Zero))
                    {
                        return 0;
                    }
                }

                return det.Magnitude;
            }
        }

        /// <summary>
        /// Gets the effective numerical matrix rank.
        /// </summary>
        /// <value>The number of non-negligible singular values.</value>
        public override int Rank
        {
            get
            {
                var rank = 0;
                for (var i = 0; i < EigenValues.Count; i++)
                {
                    if (EigenValues[i].AlmostEqual(Complex.Zero))
                    {
                        continue;
                    }

                    rank++;
                }

                return rank;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix is full rank or not.
        /// </summary>
        /// <value><c>true</c> if the matrix is full rank; otherwise <c>false</c>.</value>
        public override bool IsFullRank
        {
            get
            {
                for (var i = 0; i < EigenValues.Count; i++)
                {
                    if (EigenValues[i].AlmostEqual(Complex.Zero))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
