// <copyright file="MatrixNormal.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.Distributions
{
    using System;
    using LinearAlgebra.Double;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Factorization;
    using Properties;

    /// <summary>
    /// This class implements functionality for matrix valued normal distributions. The distribution
    /// is parameterized by a mean matrix (M), a covariance matrix for the rows (V) and a covariance matrix
    /// for the columns (K). If the dimension of M is d-by-m then V is d-by-d and K is m-by-m.
    /// <a href="http://en.wikipedia.org/wiki/Matrix_normal_distribution">Wikipedia - MatrixNormal distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class MatrixNormal
    {
        /// <summary>
        /// The mean of the matrix normal distribution.        
        /// </summary>
        private Matrix<double> _m;

        /// <summary>
        /// The covariance matrix for the rows.
        /// </summary>
        private Matrix<double> _v;

        /// <summary>
        /// The covariance matrix for the columns.
        /// </summary>
        private Matrix<double> _k;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixNormal"/> class. 
        /// </summary>
        /// <param name="m">
        /// The mean of the matrix normal.
        /// </param>
        /// <param name="v">
        /// The covariance matrix for the rows.
        /// </param>
        /// <param name="k">
        /// The covariance matrix for the columns.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If the dimensions of the mean and two covariance matrices don't match.
        /// </exception>
        public MatrixNormal(Matrix<double> m, Matrix<double> v, Matrix<double> k)
        {
            SetParameters(m, v, k);
            RandomSource = new Random();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "MatrixNormal(Rows = " + _m.RowCount + ", Columns = " + _m.ColumnCount + ")";
        }

        /// <summary>
        /// Gets or sets the mean. (M)
        /// </summary>
        /// <value>The mean of the distribution.</value>
        public Matrix<double> Mean
        {
            get
            {
                return _m;
            }

            set
            {
                SetParameters(value, _v, _k);
            }
        }

        /// <summary>
        /// Gets or sets the row covariance. (V)
        /// </summary>
        /// <value>The row covariance.</value>
        public Matrix<double> RowCovariance
        {
            get
            {
                return _v;
            }

            set
            {
                SetParameters(_m, value, _k);
            }
        }

        /// <summary>
        /// Gets or sets the column covariance. (K)
        /// </summary>
        /// <value>The column covariance.</value>
        public Matrix<double> ColumnCovariance
        {
            get
            {
                return _k;
            }

            set
            {
                SetParameters(_m, _v, value);
            }
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="m">The mean of the matrix normal.</param>
        /// <param name="v">The covariance matrix for the rows.</param>
        /// <param name="k">The covariance matrix for the columns.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        private void SetParameters(Matrix<double> m, Matrix<double> v, Matrix<double> k)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(m, v, k))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _m = m;
            _v = v;
            _k = k;
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="m">The mean of the matrix normal.</param>
        /// <param name="v">The covariance matrix for the rows.</param>
        /// <param name="k">The covariance matrix for the columns.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        private static bool IsValidParameterSet(Matrix<double> m, Matrix<double> v, Matrix<double> k)
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
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public Random RandomSource
        {
            get
            {
                return _random;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                _random = value;
            }
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
            var cholV = Cholesky<double>.Create(_v);
            var cholK = Cholesky<double>.Create(_k);

            return Math.Exp(-0.5 * cholV.Solve(a.Transpose() * cholK.Solve(a)).Trace())
                   / Math.Pow(2.0 * Constants.Pi, x.RowCount * x.ColumnCount / 2.0)
                   / Math.Pow(cholV.Determinant, x.RowCount / 2.0)
                   / Math.Pow(cholK.Determinant, x.ColumnCount / 2.0);
        }

        /// <summary>
        /// Samples a matrix normal distributed random variable.
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public Matrix<double> Sample()
        {
            return Sample(RandomSource, _m, _v, _k);
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
        public static Matrix<double> Sample(Random rnd, Matrix<double> m, Matrix<double> v, Matrix<double> k)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(m, v, k))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            var n = m.RowCount;
            var p = m.ColumnCount;

            // Compute the Kronecker product of V and K, this is the covariance matrix for the stacked matrix.
            var vki = v.KroneckerProduct(k.Inverse());

            // Sample a vector valued random variable with VKi as the covariance.
            var vector = SampleVectorNormal(rnd, new DenseVector(n * p), vki);

            // Unstack the vector v and add the mean.
            var r = m.Clone();
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < p; j++)
                {
                    r.At(i, j, r.At(i, j) + vector[(j * n) + i]);
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
        private static Vector<double> SampleVectorNormal(Random rnd, Vector<double> mean, Matrix<double> covariance)
        {
            var chol = Cholesky<double>.Create(covariance);
            return SampleVectorNormal(rnd, mean, chol);
        }

        /// <summary>
        /// Samples a vector normal distributed random variable.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="mean">The mean of the vector normal distribution.</param>
        /// <param name="cholesky">The Cholesky factorization of the covariance matrix.</param>
        /// <returns>a sequence of samples from defined distribution.</returns>
        private static Vector<double> SampleVectorNormal(Random rnd, Vector<double> mean, Cholesky<double> cholesky)
        {
            var count = mean.Count;

            // Sample a standard normal variable.
            var v = new DenseVector(count);
            for (var d = 0; d < count; d += 2)
            {
                var sample = Normal.SampleUncheckedBoxMuller(rnd);
                v[d] = sample.Item1;
                if (d + 1 < count)
                {
                    v[d + 1] = sample.Item2;
                }
            }

            // Return the transformed variable.
            return mean + (cholesky.Factor * v);
        }
    }
}
