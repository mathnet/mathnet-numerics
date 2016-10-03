﻿// <copyright file="Svd.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Single.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of the singular value decomposition (SVD).</para>
    /// <para>Suppose M is an m-by-n matrix whose entries are real numbers.
    /// Then there exists a factorization of the form M = UΣVT where:
    /// - U is an m-by-m unitary matrix;
    /// - Σ is m-by-n diagonal matrix with nonnegative real numbers on the diagonal;
    /// - VT denotes transpose of V, an n-by-n unitary matrix;
    /// Such a factorization is called a singular-value decomposition of M. A common convention is to order the diagonal
    /// entries Σ(i,i) in descending order. In this case, the diagonal matrix Σ is uniquely determined
    /// by M (though the matrices U and V are not). The diagonal entries of Σ are known as the singular values of M.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the singular value decomposition is done at construction time.
    /// </remarks>
    internal abstract class Svd : Svd<float>
    {
        protected Svd(Vector<float> s, Matrix<float> u, Matrix<float> vt, bool vectorsComputed)
            : base(s, u, vt, vectorsComputed)
        {
        }

        /// <summary>
        /// Gets the effective numerical matrix rank.
        /// </summary>
        /// <value>The number of non-negligible singular values.</value>
        public override int Rank
        {
            get
            {
                double tolerance = Precision.EpsilonOf(S.Maximum())*Math.Max(U.RowCount, VT.RowCount);
                return S.Count(t => Math.Abs(t) > tolerance);
            }
        }

        /// <summary>
        /// Gets the two norm of the <see cref="Matrix{T}"/>.
        /// </summary>
        /// <returns>The 2-norm of the <see cref="Matrix{T}"/>.</returns>
        public override double L2Norm
        {
            get
            {
                return Math.Abs(S[0]);
            }
        }

        /// <summary>
        /// Gets the condition number <b>max(S) / min(S)</b>
        /// </summary>
        /// <returns>The condition number.</returns>
        public override float ConditionNumber
        {
            get
            {
                var tmp = Math.Min(U.RowCount, VT.ColumnCount) - 1;
                return Math.Abs(S[0]) / Math.Abs(S[tmp]);
            }
        }

        /// <summary>
        /// Gets the determinant of the square matrix for which the SVD was computed.
        /// </summary>
        public override float Determinant
        {
            get
            {
                if (U.RowCount != VT.ColumnCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixSquare);
                }

                var det = 1.0;
                foreach (var value in S)
                {
                    det *= value;
                    if (Math.Abs(value).AlmostEqual(0.0f))
                    {
                        return 0;
                    }
                }

                return Convert.ToSingle(Math.Abs(det));
            }
        }
    }
}
