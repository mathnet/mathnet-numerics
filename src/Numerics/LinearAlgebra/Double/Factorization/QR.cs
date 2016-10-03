﻿// <copyright file="QR.cs" company="Math.NET">
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

using System;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra.Double.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition.</para>
    /// <para>Any real square matrix A (m x n) may be decomposed as A = QR where Q is an orthogonal matrix
    /// (its columns are orthogonal unit vectors meaning QTQ = I) and R is an upper triangular matrix 
    /// (also called right triangular matrix).</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by Householder transformation.
    /// If a <seealso cref="QRMethod.Full"/> factorization is performed, the resulting Q matrix is an m x m matrix 
    /// and the R matrix is an m x n matrix. If a <seealso cref="QRMethod.Thin"/> factorization is performed, the 
    /// resulting Q matrix is an m x n matrix and the R matrix is an n x n matrix.     
    /// </remarks>
    internal abstract class QR : QR<double>
    {
        protected QR(Matrix<double> q, Matrix<double> rFull, QRMethod method)
            : base(q, rFull, method)
        {
        }

        /// <summary>
        /// Gets the absolute determinant value of the matrix for which the QR matrix was computed.
        /// </summary>
        public override double Determinant
        {
            get
            {
                if (FullR.RowCount != FullR.ColumnCount)
                {
                    throw new ArgumentException(Resources.ArgumentMatrixSquare);
                }

                var det = 1.0;
                for (var i = 0; i < FullR.ColumnCount; i++)
                {
                    det *= FullR.At(i, i);
                    if (Math.Abs(FullR.At(i, i)).AlmostEqual(0.0))
                    {
                        return 0;
                    }
                }

                return Math.Abs(det);
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
                for (var i = 0; i < FullR.ColumnCount; i++)
                {
                    if (Math.Abs(FullR.At(i, i)).AlmostEqual(0.0))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
