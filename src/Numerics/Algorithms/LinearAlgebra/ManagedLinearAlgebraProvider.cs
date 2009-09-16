// <copyright file="ManagedLinearAlgebra.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
// Copyright (c) 2009 Math.NET
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
using System;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Algorithms.LinearAlgebra
{
    /// <summary>
    /// The managed linear algebra provider.
    /// </summary>
    public class ManagedLinearAlgebraProvider : ILinearAlgebraProvider
    {
        /// <summary>
        /// Adds a scaled vector to another: <c>y += alpha*x</c>.
        /// </summary>
        /// <param name="y">The vector to update.</param>
        /// <param name="alpha">The value to scale <paramref name="x"/> by.</param>
        /// <param name="x">The vector to add to <paramref name="y"/>.</param>
        /// <remarks>This equivalent to the AXPY BLAS routine.</remarks>
        public void AddVectorToScaledVector(double[] y, double alpha, double[] x)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (y.Length != x.Length)
            {
                throw new ArgumentException(Properties.Resources.ArgumentVectorsSameLength);
            }

            if (alpha == 0.0)
            {
                return;
            }

            if (alpha == 1.0)
            {
                Parallel.For(0, y.Length, i => y[i] += x[i]);
            }
            else
            {
                Parallel.For(0, y.Length, i => y[i] += alpha * x[i]);
            }
        }

        /// <summary>
        /// Scales an array. Can be used to scale a vector and a matrix.
        /// </summary>
        /// <param name="alpha">The scalar.</param>
        /// <param name="x">The values to scale.</param>
        /// <remarks>This is equivalent to the SCAL BLAS routine.</remarks>
        public void ScaleArray(double alpha, double[] x)
        {
            if (alpha == 1.0)
            {
                return;
            }

            Parallel.For(0, x.Length, i => x[i] = alpha * x[i]);
        }

        #region ILinearAlgebraProvider Members

        public int QueryWorkspaceBlockSize(string methodName)
        {
            throw new NotImplementedException();
        }

        public double DotProduct(double[] x, double[] y)
        {
            throw new NotImplementedException();
        }

        public void AddArrays(double[] x, double[] y, double[] result)
        {
            throw new NotImplementedException();
        }

        public void SubtractArrays(double[] x, double[] y, double[] result)
        {
            throw new NotImplementedException();
        }

        public void PointWiseMultiplyArrays(double[] x, double[] y, double[] result)
        {
            throw new NotImplementedException();
        }

        public double MatrixNorm(Norm norm, double[] matrix)
        {
            throw new NotImplementedException();
        }

        public double MatrixNorm(Norm norm, double[] matrix, double[] work)
        {
            throw new NotImplementedException();
        }

        public void MatrixMultiply(double[] x, double[] y, double[] result)
        {
            throw new NotImplementedException();
        }

        public void MatrixMultiplyWithUpdate(Transpose transposeA, Transpose transposeB, double alpha, double[] a, double[] b, double beta, double[] c)
        {
            throw new NotImplementedException();
        }

        public void LUFactor(double[] a, int[] ipiv)
        {
            throw new NotImplementedException();
        }

        public void LUInverse(double[] a)
        {
            throw new NotImplementedException();
        }

        public void LUInverseFactored(double[] a, int[] ipiv)
        {
            throw new NotImplementedException();
        }

        public void LUInverse(double[] a, double[] work)
        {
            throw new NotImplementedException();
        }

        public void LUInverseFactored(double[] a, int[] ipiv, double[] work)
        {
            throw new NotImplementedException();
        }

        public void LUSolve(int columnsOfB, double[] a, double[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolveFactored(int columnsOfB, double[] a, int ipiv, double[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolve(Transpose transposeA, int columnsOfB, double[] a, double[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolveFactored(Transpose transposeA, int columnsOfB, double[] a, int ipiv, double[] b)
        {
            throw new NotImplementedException();
        }

        public void CholeskyFactor(double[] a)
        {
            throw new NotImplementedException();
        }

        public void CholeskySolve(int columnsOfB, double[] a, double[] b)
        {
            throw new NotImplementedException();
        }

        public void CholeskySolveFactored(int columnsOfB, double[] a, double[] b)
        {
            throw new NotImplementedException();
        }

        public void QRFactor(double[] a, double[] q, double[] tau)
        {
            throw new NotImplementedException();
        }

        public void QRFactor(double[] a, double[] q, double[] tau, double[] work)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, double[] a, double[] tau, double[] b, double[] x)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, double[] a, double[] tau, double[] b, double[] x, double[] work)
        {
            throw new NotImplementedException();
        }

        public void SinguarValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt)
        {
            throw new NotImplementedException();
        }

        public void SinguarValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt, double[] work)
        {
            throw new NotImplementedException();
        }

        public void SvdSolve(double[] s, double[] u, double[] vt, double[] b, double[] x)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}