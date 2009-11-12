// <copyright file="ManagedLinearAlgebraProvider.cs" company="Math.NET">
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
namespace MathNet.Numerics.Algorithms.LinearAlgebra
{
    using System;
    using Properties;
    using Threading;

    /// <summary>
    /// The managed linear algebra provider.
    /// </summary>
    public class ManagedLinearAlgebraProvider : ILinearAlgebraProvider
    {
        #region Workspace information Members

        public int QueryWorkspaceBlockSize(string methodName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ILinearAlgebraProvider<double> Members

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
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
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
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (alpha == 1.0)
            {
                return;
            }

            Parallel.For(0, x.Length, i => x[i] = alpha * x[i]);
        }

        /// <summary>
        /// Computes the dot product of x and y.
        /// </summary>
        /// <param name="x">The vector x.</param>
        /// <param name="y">The vector y.</param>
        /// <returns>The dot product of x and y.</returns>
        /// <remarks>This is equivalent to the DOT BLAS routine.</remarks>
        public double DotProduct(double[] x, double[] y)
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
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            double d = 0.0;

            for (int i = 0; i < y.Length; i++)
            {
                d += y[i] * x[i];
            }

            return d;
        }

        /// <summary>
        /// Does a point wise add of two arrays <c>z = x + y</c>. This can be used 
        /// to add vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the addition.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void AddArrays(double[] x, double[] y, double[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }
            
            Parallel.For(0, y.Length, i => result[i] = x[i] + y[i]);
        }

        /// <summary>
        /// Does a point wise subtraction of two arrays <c>z = x - y</c>. This can be used 
        /// to subtract vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the subtraction.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void SubtractArrays(double[] x, double[] y, double[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] - y[i]);
        }

        /// <summary>
        /// Does a point wise multiplication of two arrays <c>z = x * y</c>. This can be used
        /// to multiple elements of vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the point wise multiplication.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void PointWiseMultiplyArrays(double[] x, double[] y, double[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] * y[i]);
        }

        public double MatrixNorm(Norm norm, double[] matrix)
        {
            throw new NotImplementedException();
        }

        public double MatrixNorm(Norm norm, double[] matrix, double[] work)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Multiples two matrices. <c>result = x * y</c>
        /// </summary>
        /// <param name="x">The x matrix.</param>
        /// <param name="xRows">The number of rows in the x matrix.</param>
        /// <param name="xColumns">The number of columns in the x matrix.</param>
        /// <param name="y">The y matrix.</param>
        /// <param name="yRows">The number of rows in the y matrix.</param>
        /// <param name="yColumns">The number of columns in the y matrix.</param>
        /// <param name="result">Where to store the result of the multiplication.</param>
        /// <remarks>This is a simplified version of the BLAS GEMM routine with alpha
        /// set to 1.0 and beta set to 0.0, and x and y are not transposed.</remarks>
        public void MatrixMultiply(double[] x, int xRows, int xColumns, double[] y, int yRows, int yColumns, double[] result)
        {
            // First check some basic requirement on the parameters of the matrix multiplication.
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (xRows * xColumns != x.Length)
            {
                throw new ArgumentException("x.Length != xRows * xColumns");
            }

            if (yRows * yColumns != y.Length)
            {
                throw new ArgumentException("y.Length != yRows * yColumns");
            }

            if (xColumns != yRows)
            {
                throw new ArgumentException("xColumns != yRows");
            }

            if (xRows * yColumns != result.Length)
            {
                throw new ArgumentException("xRows * yColumns != result.Length");
            }

            // Check whether we will be overwriting any of our inputs and make copies if necessary.
            // TODO - we can don't have to allocate a completely new matrix when x or y point to the same memory
            // as result, we can do it on a row wise basis. We should investigate this.
            double[] xdata;
            if (ReferenceEquals(x, result))
            {
                xdata = (double[]) x.Clone();
            }
            else
            {
                xdata = x;
            }

            double[] ydata;
            if (ReferenceEquals(y, result))
            {
                ydata = (double[]) y.Clone();
            }
            else
            {
                ydata = y;
            }

            // Start the actual matrix multiplication.
            // TODO - For small matrices we should get rid of the parallelism because of startup costs.
            // Perhaps the following implementations would be a good one
            // http://blog.feradz.com/2009/01/cache-efficient-matrix-multiplication/
            Parallel.For(0, xRows, i =>
            {
                for (int j = 0; j < yColumns; j++)
                {
                    for (int k = 0; k < xColumns; k++)
                    {
                        result[j + yColumns * i] += xdata[k + xColumns * i] * ydata[j + yColumns * k];
                    }
                }
            });
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

        public void QRFactor(double[] r, double[] q)
        {
            throw new NotImplementedException();
        }

        public void QRFactor(double[] r, double[] q, double[] work)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, double[] r, double[] q, double[] b, double[] x)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, double[] r, double[] q, double[] b, double[] x, double[] work)
        {
            throw new NotImplementedException();
        }

        public void QRSolveFactored(int columnsOfB, double[] q, double[] r, double[] b, double[] x)
        {
            throw new NotImplementedException();
        }

        public void SinguarValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt)
        {
            throw new NotImplementedException();
        }

        public void SingularValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt, double[] work)
        {
            throw new NotImplementedException();
        }

        public void SvdSolve(double[] a, double[] s, double[] u, double[] vt, double[] b, double[] x)
        {
            throw new NotImplementedException();
        }

        public void SvdSolve(double[] a, double[] s, double[] u, double[] vt, double[] b, double[] x, double[] work)
        {
            throw new NotImplementedException();
        }

        public void SvdSolveFactored(int columnsOfB, double[] s, double[] u, double[] vt, double[] b, double[] x)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ILinearAlgebraProvider<float> Members

        /// <summary>
        /// Adds a scaled vector to another: <c>y += alpha*x</c>.
        /// </summary>
        /// <param name="y">The vector to update.</param>
        /// <param name="alpha">The value to scale <paramref name="x"/> by.</param>
        /// <param name="x">The vector to add to <paramref name="y"/>.</param>
        /// <remarks>This equivalent to the AXPY BLAS routine.</remarks>
        public void AddVectorToScaledVector(float[] y, float alpha, float[] x)
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
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
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
        public void ScaleArray(float alpha, float[] x)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (alpha == 1.0)
            {
                return;
            }

            Parallel.For(0, x.Length, i => x[i] = alpha * x[i]);
        }

        /// <summary>
        /// Computes the dot product of x and y.
        /// </summary>
        /// <param name="x">The vector x.</param>
        /// <param name="y">The vector y.</param>
        /// <returns>The dot product of x and y.</returns>
        /// <remarks>This is equivalent to the DOT BLAS routine.</remarks>
        public float DotProduct(float[] x, float[] y)
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
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            float d = 0.0F;

            for (int i = 0; i < y.Length; i++)
            {
                d += y[i] * x[i];
            }

            return d;
        }

        /// <summary>
        /// Does a point wise add of two arrays <c>z = x + y</c>. This can be used 
        /// to add vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the addition.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void AddArrays(float[] x, float[] y, float[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] + y[i]);
        }

        /// <summary>
        /// Does a point wise subtraction of two arrays <c>z = x - y</c>. This can be used 
        /// to subtract vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the subtraction.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void SubtractArrays(float[] x, float[] y, float[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] - y[i]);
        }

        /// <summary>
        /// Does a point wise multiplication of two arrays <c>z = x * y</c>. This can be used
        /// to multiple elements of vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the point wise multiplication.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void PointWiseMultiplyArrays(float[] x, float[] y, float[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] * y[i]);
        }

        public float MatrixNorm(Norm norm, float[] matrix)
        {
            throw new NotImplementedException();
        }

        public float MatrixNorm(Norm norm, float[] matrix, float[] work)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Multiples two matrices. <c>result = x * y</c>
        /// </summary>
        /// <param name="x">The x matrix.</param>
        /// <param name="xRows">The number of rows in the x matrix.</param>
        /// <param name="xColumns">The number of columns in the x matrix.</param>
        /// <param name="y">The y matrix.</param>
        /// <param name="yRows">The number of rows in the y matrix.</param>
        /// <param name="yColumns">The number of columns in the y matrix.</param>
        /// <param name="result">Where to store the result of the multiplication.</param>
        /// <remarks>This is a simplified version of the BLAS GEMM routine with alpha
        /// set to 1.0 and beta set to 0.0, and x and y are not transposed.</remarks>
        public void MatrixMultiply(float[] x, int xRows, int xColumns, float[] y, int yRows, int yColumns, float[] result)
        {
            // First check some basic requirement on the parameters of the matrix multiplication.
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (xRows * xColumns != x.Length)
            {
                throw new ArgumentException("x.Length != xRows * xColumns");
            }

            if (yRows * yColumns != y.Length)
            {
                throw new ArgumentException("y.Length != yRows * yColumns");
            }

            if (xColumns != yRows)
            {
                throw new ArgumentException("xColumns != yRows");
            }

            if (xRows * yColumns != result.Length)
            {
                throw new ArgumentException("xRows * yColumns != result.Length");
            }

            // Check whether we will be overwriting any of our inputs and make copies if necessary.
            // TODO - we can don't have to allocate a completely new matrix when x or y point to the same memory
            // as result, we can do it on a row wise basis. We should investigate this.
            float[] xdata;
            if (ReferenceEquals(x, result))
            {
                xdata = (float[]) x.Clone();
            }
            else
            {
                xdata = x;
            }

            float[] ydata;
            if (ReferenceEquals(y, result))
            {
                ydata = (float[]) y.Clone();
            }
            else
            {
                ydata = y;
            }

            // Start the actual matrix multiplication.
            // TODO - For small matrices we should get rid of the parallelism because of startup costs.
            // Perhaps the following implementations would be a good one
            // http://blog.feradz.com/2009/01/cache-efficient-matrix-multiplication/
            Parallel.For(0, xRows, i =>
            {
                for (int j = 0; j < yColumns; j++)
                {
                    for (int k = 0; k < xColumns; k++)
                    {
                        result[j + yColumns * i] += xdata[k + xColumns * i] * ydata[j + yColumns * k];
                    }
                }
            });
        }

        public void MatrixMultiplyWithUpdate(Transpose transposeA, Transpose transposeB, float alpha, float[] a, float[] b, float beta, float[] c)
        {
            throw new NotImplementedException();
        }

        public void LUFactor(float[] a, int[] ipiv)
        {
            throw new NotImplementedException();
        }

        public void LUInverse(float[] a)
        {
            throw new NotImplementedException();
        }

        public void LUInverseFactored(float[] a, int[] ipiv)
        {
            throw new NotImplementedException();
        }

        public void LUInverse(float[] a, float[] work)
        {
            throw new NotImplementedException();
        }

        public void LUInverseFactored(float[] a, int[] ipiv, float[] work)
        {
            throw new NotImplementedException();
        }

        public void LUSolve(int columnsOfB, float[] a, float[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolveFactored(int columnsOfB, float[] a, int ipiv, float[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolve(Transpose transposeA, int columnsOfB, float[] a, float[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolveFactored(Transpose transposeA, int columnsOfB, float[] a, int ipiv, float[] b)
        {
            throw new NotImplementedException();
        }

        public void CholeskyFactor(float[] a)
        {
            throw new NotImplementedException();
        }

        public void CholeskySolve(int columnsOfB, float[] a, float[] b)
        {
            throw new NotImplementedException();
        }

        public void CholeskySolveFactored(int columnsOfB, float[] a, float[] b)
        {
            throw new NotImplementedException();
        }

        public void QRFactor(float[] r, float[] q)
        {
            throw new NotImplementedException();
        }

        public void QRFactor(float[] r, float[] q, float[] work)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, float[] r, float[] q, float[] b, float[] x)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, float[] r, float[] q, float[] b, float[] x, float[] work)
        {
            throw new NotImplementedException();
        }

        public void QRSolveFactored(int columnsOfB, float[] q, float[] r, float[] b, float[] x)
        {
            throw new NotImplementedException();
        }

        public void SinguarValueDecomposition(bool computeVectors, float[] a, float[] s, float[] u, float[] vt)
        {
            throw new NotImplementedException();
        }

        public void SingularValueDecomposition(bool computeVectors, float[] a, float[] s, float[] u, float[] vt, float[] work)
        {
            throw new NotImplementedException();
        }

        public void SvdSolve(float[] a, float[] s, float[] u, float[] vt, float[] b, float[] x)
        {
            throw new NotImplementedException();
        }

        public void SvdSolve(float[] a, float[] s, float[] u, float[] vt, float[] b, float[] x, float[] work)
        {
            throw new NotImplementedException();
        }

        public void SvdSolveFactored(int columnsOfB, float[] s, float[] u, float[] vt, float[] b, float[] x)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ILinearAlgebraProvider<Complex> Members

        /// <summary>
        /// Adds a scaled vector to another: <c>y += alpha*x</c>.
        /// </summary>
        /// <param name="y">The vector to update.</param>
        /// <param name="alpha">The value to scale <paramref name="x"/> by.</param>
        /// <param name="x">The vector to add to <paramref name="y"/>.</param>
        /// <remarks>This equivalent to the AXPY BLAS routine.</remarks>
        public void AddVectorToScaledVector(Complex[] y, Complex alpha, Complex[] x)
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
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
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
        public void ScaleArray(Complex alpha, Complex[] x)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (alpha == 1.0)
            {
                return;
            }

            Parallel.For(0, x.Length, i => x[i] = alpha * x[i]);
        }

        /// <summary>
        /// Computes the dot product of x and y.
        /// </summary>
        /// <param name="x">The vector x.</param>
        /// <param name="y">The vector y.</param>
        /// <returns>The dot product of x and y.</returns>
        /// <remarks>This is equivalent to the DOT BLAS routine.</remarks>
        public Complex DotProduct(Complex[] x, Complex[] y)
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
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Complex d = new Complex(0.0, 0.0);

            for (int i = 0; i < y.Length; i++)
            {
                d += y[i] * x[i];
            }

            return d;
        }

        /// <summary>
        /// Does a point wise add of two arrays <c>z = x + y</c>. This can be used 
        /// to add vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the addition.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void AddArrays(Complex[] x, Complex[] y, Complex[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] + y[i]);
        }

        /// <summary>
        /// Does a point wise subtraction of two arrays <c>z = x - y</c>. This can be used 
        /// to subtract vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the subtraction.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void SubtractArrays(Complex[] x, Complex[] y, Complex[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] - y[i]);
        }

        /// <summary>
        /// Does a point wise multiplication of two arrays <c>z = x * y</c>. This can be used
        /// to multiple elements of vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the point wise multiplication.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void PointWiseMultiplyArrays(Complex[] x, Complex[] y, Complex[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] * y[i]);
        }

        public Complex MatrixNorm(Norm norm, Complex[] matrix)
        {
            throw new NotImplementedException();
        }

        public Complex MatrixNorm(Norm norm, Complex[] matrix, Complex[] work)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Multiples two matrices. <c>result = x * y</c>
        /// </summary>
        /// <param name="x">The x matrix.</param>
        /// <param name="xRows">The number of rows in the x matrix.</param>
        /// <param name="xColumns">The number of columns in the x matrix.</param>
        /// <param name="y">The y matrix.</param>
        /// <param name="yRows">The number of rows in the y matrix.</param>
        /// <param name="yColumns">The number of columns in the y matrix.</param>
        /// <param name="result">Where to store the result of the multiplication.</param>
        /// <remarks>This is a simplified version of the BLAS GEMM routine with alpha
        /// set to 1.0 and beta set to 0.0, and x and y are not transposed.</remarks>
        public void MatrixMultiply(Complex[] x, int xRows, int xColumns, Complex[] y, int yRows, int yColumns, Complex[] result)
        {
            // First check some basic requirement on the parameters of the matrix multiplication.
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (xRows * xColumns != x.Length)
            {
                throw new ArgumentException("x.Length != xRows * xColumns");
            }

            if (yRows * yColumns != y.Length)
            {
                throw new ArgumentException("y.Length != yRows * yColumns");
            }

            if (xColumns != yRows)
            {
                throw new ArgumentException("xColumns != yRows");
            }

            if (xRows * yColumns != result.Length)
            {
                throw new ArgumentException("xRows * yColumns != result.Length");
            }

            // Check whether we will be overwriting any of our inputs and make copies if necessary.
            // TODO - we can don't have to allocate a completely new matrix when x or y point to the same memory
            // as result, we can do it on a row wise basis. We should investigate this.
            double[] xdata;
            if (ReferenceEquals(x, result))
            {
                xdata = (Complex[]) x.Clone();
            }
            else
            {
                xdata = x;
            }

            double[] ydata;
            if (ReferenceEquals(y, result))
            {
                ydata = (Complex[]) y.Clone();
            }
            else
            {
                ydata = y;
            }

            // Start the actual matrix multiplication.
            // TODO - For small matrices we should get rid of the parallelism because of startup costs.
            // Perhaps the following implementations would be a good one
            // http://blog.feradz.com/2009/01/cache-efficient-matrix-multiplication/
            Parallel.For(0, xRows, i =>
                {
                    for (int j = 0; j < yColumns; j++)
                    {
                        for (int k = 0; k < xColumns; k++)
                        {
                            result[j + yColumns * i] += xdata[k + xColumns * i] * ydata[j + yColumns * k];
                        }
                    }
                });
        }

        public void MatrixMultiplyWithUpdate(Transpose transposeA, Transpose transposeB, Complex alpha, Complex[] a, Complex[] b, Complex beta, Complex[] c)
        {
            throw new NotImplementedException();
        }

        public void LUFactor(Complex[] a, int[] ipiv)
        {
            throw new NotImplementedException();
        }

        public void LUInverse(Complex[] a)
        {
            throw new NotImplementedException();
        }

        public void LUInverseFactored(Complex[] a, int[] ipiv)
        {
            throw new NotImplementedException();
        }

        public void LUInverse(Complex[] a, Complex[] work)
        {
            throw new NotImplementedException();
        }

        public void LUInverseFactored(Complex[] a, int[] ipiv, Complex[] work)
        {
            throw new NotImplementedException();
        }

        public void LUSolve(int columnsOfB, Complex[] a, Complex[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolveFactored(int columnsOfB, Complex[] a, int ipiv, Complex[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolve(Transpose transposeA, int columnsOfB, Complex[] a, Complex[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolveFactored(Transpose transposeA, int columnsOfB, Complex[] a, int ipiv, Complex[] b)
        {
            throw new NotImplementedException();
        }

        public void CholeskyFactor(Complex[] a)
        {
            throw new NotImplementedException();
        }

        public void CholeskySolve(int columnsOfB, Complex[] a, Complex[] b)
        {
            throw new NotImplementedException();
        }

        public void CholeskySolveFactored(int columnsOfB, Complex[] a, Complex[] b)
        {
            throw new NotImplementedException();
        }

        public void QRFactor(Complex[] r, Complex[] q)
        {
            throw new NotImplementedException();
        }

        public void QRFactor(Complex[] r, Complex[] q, Complex[] work)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, Complex[] r, Complex[] q, Complex[] b, Complex[] x)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, Complex[] r, Complex[] q, Complex[] b, Complex[] x, Complex[] work)
        {
            throw new NotImplementedException();
        }

        public void QRSolveFactored(int columnsOfB, Complex[] q, Complex[] r, Complex[] b, Complex[] x)
        {
            throw new NotImplementedException();
        }

        public void SinguarValueDecomposition(bool computeVectors, Complex[] a, Complex[] s, Complex[] u, Complex[] vt)
        {
            throw new NotImplementedException();
        }

        public void SingularValueDecomposition(bool computeVectors, Complex[] a, Complex[] s, Complex[] u, Complex[] vt, Complex[] work)
        {
            throw new NotImplementedException();
        }

        public void SvdSolve(Complex[] a, Complex[] s, Complex[] u, Complex[] vt, Complex[] b, Complex[] x)
        {
            throw new NotImplementedException();
        }

        public void SvdSolve(Complex[] a, Complex[] s, Complex[] u, Complex[] vt, Complex[] b, Complex[] x, Complex[] work)
        {
            throw new NotImplementedException();
        }

        public void SvdSolveFactored(int columnsOfB, Complex[] s, Complex[] u, Complex[] vt, Complex[] b, Complex[] x)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ILinearAlgebraProvider<Complex32> Members

        /// <summary>
        /// Adds a scaled vector to another: <c>y += alpha*x</c>.
        /// </summary>
        /// <param name="y">The vector to update.</param>
        /// <param name="alpha">The value to scale <paramref name="x"/> by.</param>
        /// <param name="x">The vector to add to <paramref name="y"/>.</param>
        /// <remarks>This equivalent to the AXPY BLAS routine.</remarks>
        public void AddVectorToScaledVector(Complex32[] y, Complex32 alpha, Complex32[] x)
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
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            if (alpha == 0.0F)
            {
                return;
            }

            if (alpha == 1.0F)
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
        public void ScaleArray(Complex32 alpha, Complex32[] x)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (alpha.IsOne)
            {
                return;
            }

            Parallel.For(0, x.Length, i => x[i] = alpha * x[i]);
        }

        /// <summary>
        /// Computes the dot product of x and y.
        /// </summary>
        /// <param name="x">The vector x.</param>
        /// <param name="y">The vector y.</param>
        /// <returns>The dot product of x and y.</returns>
        /// <remarks>This is equivalent to the DOT BLAS routine.</remarks>
        public Complex32 DotProduct(Complex32[] x, Complex32[] y)
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
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Complex32 d = new Complex32(0.0F, 0.0F);

            for (int i = 0; i < y.Length; i++)
            {
                d += y[i] * x[i];
            }

            return d;
        }

        /// <summary>
        /// Does a point wise add of two arrays <c>z = x + y</c>. This can be used 
        /// to add vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the addition.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void AddArrays(Complex32[] x, Complex32[] y, Complex32[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] + y[i]);
        }

        /// <summary>
        /// Does a point wise subtraction of two arrays <c>z = x - y</c>. This can be used 
        /// to subtract vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the subtraction.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void SubtractArrays(Complex32[] x, Complex32[] y, Complex32[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] - y[i]);
        }

        /// <summary>
        /// Does a point wise multiplication of two arrays <c>z = x * y</c>. This can be used
        /// to multiple elements of vectors or matrices.
        /// </summary>
        /// <param name="x">The array x.</param>
        /// <param name="y">The array y.</param>
        /// <param name="result">The result of the point wise multiplication.</param>
        /// <remarks>There is no equivalent BLAS routine, but many libraries
        /// provide optimized (parallel and/or vectorized) versions of this
        /// routine.</remarks>
        public void PointWiseMultiplyArrays(Complex32[] x, Complex32[] y, Complex32[] result)
        {
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (x == null)
            {
                throw new ArgumentNullException("x");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (y.Length != x.Length || y.Length != result.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Parallel.For(0, y.Length, i => result[i] = x[i] * y[i]);
        }

        public Complex32 MatrixNorm(Norm norm, Complex32[] matrix)
        {
            throw new NotImplementedException();
        }

        public Complex32 MatrixNorm(Norm norm, Complex32[] matrix, Complex32[] work)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Multiples two matrices. <c>result = x * y</c>
        /// </summary>
        /// <param name="x">The x matrix.</param>
        /// <param name="xRows">The number of rows in the x matrix.</param>
        /// <param name="xColumns">The number of columns in the x matrix.</param>
        /// <param name="y">The y matrix.</param>
        /// <param name="yRows">The number of rows in the y matrix.</param>
        /// <param name="yColumns">The number of columns in the y matrix.</param>
        /// <param name="result">Where to store the result of the multiplication.</param>
        /// <remarks>This is a simplified version of the BLAS GEMM routine with alpha
        /// set to 1.0 and beta set to 0.0, and x and y are not transposed.</remarks>
        public void MatrixMultiply(Complex32[] x, int xRows, int xColumns, Complex32[] y, int yRows, int yColumns, Complex32[] result)
        {
            // First check some basic requirement on the parameters of the matrix multiplication.
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (xRows * xColumns != x.Length)
            {
                throw new ArgumentException("x.Length != xRows * xColumns");
            }

            if (yRows * yColumns != y.Length)
            {
                throw new ArgumentException("y.Length != yRows * yColumns");
            }

            if (xColumns != yRows)
            {
                throw new ArgumentException("xColumns != yRows");
            }

            if (xRows * yColumns != result.Length)
            {
                throw new ArgumentException("xRows * yColumns != result.Length");
            }

            // Check whether we will be overwriting any of our inputs and make copies if necessary.
            // TODO - we can don't have to allocate a completely new matrix when x or y point to the same memory
            // as result, we can do it on a row wise basis. We should investigate this.
            Complex32[] xdata;
            if (ReferenceEquals(x, result))
            {
                xdata = (Complex32[]) x.Clone();
            }
            else
            {
                xdata = x;
            }

            Complex32[] ydata;
            if (ReferenceEquals(y, result))
            {
                ydata = (Complex32[]) y.Clone();
            }
            else
            {
                ydata = y;
            }

            // Start the actual matrix multiplication.
            // TODO - For small matrices we should get rid of the parallelism because of startup costs.
            // Perhaps the following implementations would be a good one
            // http://blog.feradz.com/2009/01/cache-efficient-matrix-multiplication/
            Parallel.For(0, xRows, i =>
            {
                for (int j = 0; j < yColumns; j++)
                {
                    for (int k = 0; k < xColumns; k++)
                    {
                        result[j + yColumns * i] += xdata[k + xColumns * i] * ydata[j + yColumns * k];
                    }
                }
            });
        }

        public void MatrixMultiplyWithUpdate(Transpose transposeA, Transpose transposeB, Complex32 alpha, Complex32[] a, Complex32[] b, Complex32 beta, Complex32[] c)
        {
            throw new NotImplementedException();
        }

        public void LUFactor(Complex32[] a, int[] ipiv)
        {
            throw new NotImplementedException();
        }

        public void LUInverse(Complex32[] a)
        {
            throw new NotImplementedException();
        }

        public void LUInverseFactored(Complex32[] a, int[] ipiv)
        {
            throw new NotImplementedException();
        }

        public void LUInverse(Complex32[] a, Complex32[] work)
        {
            throw new NotImplementedException();
        }

        public void LUInverseFactored(Complex32[] a, int[] ipiv, Complex32[] work)
        {
            throw new NotImplementedException();
        }

        public void LUSolve(int columnsOfB, Complex32[] a, Complex32[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolveFactored(int columnsOfB, Complex32[] a, int ipiv, Complex32[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolve(Transpose transposeA, int columnsOfB, Complex32[] a, Complex32[] b)
        {
            throw new NotImplementedException();
        }

        public void LUSolveFactored(Transpose transposeA, int columnsOfB, Complex32[] a, int ipiv, Complex32[] b)
        {
            throw new NotImplementedException();
        }

        public void CholeskyFactor(Complex32[] a)
        {
            throw new NotImplementedException();
        }

        public void CholeskySolve(int columnsOfB, Complex32[] a, Complex32[] b)
        {
            throw new NotImplementedException();
        }

        public void CholeskySolveFactored(int columnsOfB, Complex32[] a, Complex32[] b)
        {
            throw new NotImplementedException();
        }

        public void QRFactor(Complex32[] r, Complex32[] q)
        {
            throw new NotImplementedException();
        }

        public void QRFactor(Complex32[] r, Complex32[] q, Complex32[] work)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, Complex32[] r, Complex32[] q, Complex32[] b, Complex32[] x)
        {
            throw new NotImplementedException();
        }

        public void QRSolve(int columnsOfB, Complex32[] r, Complex32[] q, Complex32[] b, Complex32[] x, Complex32[] work)
        {
            throw new NotImplementedException();
        }

        public void QRSolveFactored(int columnsOfB, Complex32[] q, Complex32[] r, Complex32[] b, Complex32[] x)
        {
            throw new NotImplementedException();
        }

        public void SinguarValueDecomposition(bool computeVectors, Complex32[] a, Complex32[] s, Complex32[] u, Complex32[] vt)
        {
            throw new NotImplementedException();
        }

        public void SingularValueDecomposition(bool computeVectors, Complex32[] a, Complex32[] s, Complex32[] u, Complex32[] vt, Complex32[] work)
        {
            throw new NotImplementedException();
        }

        public void SvdSolve(Complex32[] a, Complex32[] s, Complex32[] u, Complex32[] vt, Complex32[] b, Complex32[] x)
        {
            throw new NotImplementedException();
        }

        public void SvdSolve(Complex32[] a, Complex32[] s, Complex32[] u, Complex32[] vt, Complex32[] b, Complex32[] x, Complex32[] work)
        {
            throw new NotImplementedException();
        }

        public void SvdSolveFactored(int columnsOfB, Complex32[] s, Complex32[] u, Complex32[] vt, Complex32[] b, Complex32[] x)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}