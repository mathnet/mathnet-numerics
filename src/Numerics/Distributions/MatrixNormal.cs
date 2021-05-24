// <copyright file="MatrixNormal.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Multivariate Matrix-valued Normal distributions. The distribution
    /// is parameterized by a mean matrix (M), a covariance matrix for the rows (V) and a covariance matrix
    /// for the columns (K). If the dimension of M is d-by-m then V is d-by-d and K is m-by-m.
    /// <a href="http://en.wikipedia.org/wiki/Matrix_normal_distribution">Wikipedia - MatrixNormal distribution</a>.
    /// </summary>
    public class MatrixNormal : IDistribution
    {
        System.Random _random;

        /// <summary>
        /// The mean of the matrix normal distribution.
        /// </summary>
        readonly Matrix<double> _m;

        /// <summary>
        /// The covariance matrix for the rows.
        /// </summary>
        readonly Matrix<double> _v;

        /// <summary>
        /// The covariance matrix for the columns.
        /// </summary>
        readonly Matrix<double> _k;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixNormal"/> class.
        /// </summary>
        /// <param name="m">The mean of the matrix normal.</param>
        /// <param name="v">The covariance matrix for the rows.</param>
        /// <param name="k">The covariance matrix for the columns.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the dimensions of the mean and two covariance matrices don't match.</exception>
        public MatrixNormal(Matrix<double> m, Matrix<double> v, Matrix<double> k)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(m, v, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _m = m;
            _v = v;
            _k = k;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixNormal"/> class.
        /// </summary>
        /// <param name="m">The mean of the matrix normal.</param>
        /// <param name="v">The covariance matrix for the rows.</param>
        /// <param name="k">The covariance matrix for the columns.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the dimensions of the mean and two covariance matrices don't match.</exception>
        public MatrixNormal(Matrix<double> m, Matrix<double> v, Matrix<double> k, System.Random randomSource)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(m, v, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _m = m;
            _v = v;
            _k = k;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"MatrixNormal(Rows = {_m.RowCount}, Columns = {_m.ColumnCount})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="m">The mean of the matrix normal.</param>
        /// <param name="v">The covariance matrix for the rows.</param>
        /// <param name="k">The covariance matrix for the columns.</param>
        public static bool IsValidParameterSet(Matrix<double> m, Matrix<double> v, Matrix<double> k)
        {
            var n = m.RowCount;
            var p = m.ColumnCount;
            if (v.ColumnCount != n || v.RowCount != n)
            {
                return false;
            }

            if (k.ColumnCount != p || k.RowCount != p)
            {
                return false;
            }

            for (var i = 0; i < v.RowCount; i++)
            {
                if (v.At(i, i) <= 0)
                {
                    return false;
                }
            }

            for (var i = 0; i < k.RowCount; i++)
            {
                if (k.At(i, i) <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the mean. (M)
        /// </summary>
        /// <value>The mean of the distribution.</value>
        public Matrix<double> Mean => _m;

        /// <summary>
        /// Gets the row covariance. (V)
        /// </summary>
        /// <value>The row covariance.</value>
        public Matrix<double> RowCovariance => _v;

        /// <summary>
        /// Gets the column covariance. (K)
        /// </summary>
        /// <value>The column covariance.</value>
        public Matrix<double> ColumnCovariance => _k;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Evaluates the probability density function for the matrix normal distribution.
        /// </summary>
        /// <param name="x">The matrix at which to evaluate the density at.</param>
        /// <returns>the density at <paramref name="x"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">If the argument does not have the correct dimensions.</exception>
        public double Density(Matrix<double> x)
        {
            if (x.RowCount != _m.RowCount || x.ColumnCount != _m.ColumnCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentOutOfRangeException>(x, _m, "x");
            }

            var a = x - _m;
            var cholV = _v.Cholesky();
            var cholK = _k.Cholesky();

            return Math.Exp(-0.5*cholK.Solve(a.Transpose()*cholV.Solve(a)).Trace())
                   /Math.Pow(Constants.Pi2, x.RowCount*x.ColumnCount/2.0)
                   /Math.Pow(cholK.Determinant, x.RowCount/2.0)
                   /Math.Pow(cholV.Determinant, x.ColumnCount/2.0);
        }

        /// <summary>
        /// Samples a matrix normal distributed random variable.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public Matrix<double> Sample()
        {
            return Sample(_random, _m, _v, _k);
        }

        /// <summary>
        /// Samples a matrix normal distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="m">The mean of the matrix normal.</param>
        /// <param name="v">The covariance matrix for the rows.</param>
        /// <param name="k">The covariance matrix for the columns.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the dimensions of the mean and two covariance matrices don't match.</exception>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static Matrix<double> Sample(System.Random rnd, Matrix<double> m, Matrix<double> v, Matrix<double> k)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(m, v, k))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var n = m.RowCount;
            var p = m.ColumnCount;

            // Compute the Kronecker product of V and K, this is the covariance matrix for the stacked matrix.
            var vki = v.KroneckerProduct(k.Inverse());

            // Sample a vector valued random variable with VKi as the covariance.
            var vector = SampleVectorNormal(rnd, new DenseVector(n*p), vki);

            // Unstack the vector v and add the mean.
            var r = m.Clone();
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < p; j++)
                {
                    r.At(i, j, r.At(i, j) + vector[(j*n) + i]);
                }
            }

            return r;
        }

        /// <summary>
        /// Samples a vector normal distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mean">The mean of the vector normal distribution.</param>
        /// <param name="covariance">The covariance matrix of the vector normal distribution.</param>
        /// <returns>a sequence of samples from defined distribution.</returns>
        static Vector<double> SampleVectorNormal(System.Random rnd, Vector<double> mean, Matrix<double> covariance)
        {
            var chol = covariance.Cholesky();

            // Sample a standard normal variable.
            var v = Vector<double>.Build.Random(mean.Count, new Normal(rnd));

            // Return the transformed variable.
            return mean + (chol.Factor*v);
        }
    }
}
