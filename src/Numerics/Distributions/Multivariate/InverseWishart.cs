// <copyright file="InverseWishart.cs" company="Math.NET">
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
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Factorization;
    using Properties;

    /// <summary>
    /// This class implements functionality for the inverse Wishart distribution. This distribution is
    /// parameterized by the degrees of freedom nu and the scale matrix S. The inverse Wishart distribution
    /// is the conjugate prior for the covariance matrix of a multivariate normal distribution.
    /// <a href="http://en.wikipedia.org/wiki/Inverse-Wishart_distribution">Wikipedia - Inverse-Wishart distribution</a>.
    /// </summary>
    /// <remarks><para>The distribution will use the <see cref="System.Random"/> by default. 
    /// Users can set the random number generator by using the <see cref="RandomSource"/> property.</para>
    /// <para>The statistics classes will check all the incoming parameters whether they are in the allowed
    /// range. This might involve heavy computation. Optionally, by setting Control.CheckDistributionParameters
    /// to <c>false</c>, all parameter checks can be turned off.</para></remarks>
    public class InverseWishart
    {
        /// <summary>
        /// The degrees of freedom for the inverse Wishart distribution.
        /// </summary>
        private double _nu;

        /// <summary>
        /// The scale matrix for the inverse Wishart distribution.
        /// </summary>
        private Matrix<double> _s;

        /// <summary>
        /// Caches the Cholesky factorization of the scale matrix.
        /// </summary>
        private Cholesky<double> _chol;

        /// <summary>
        /// The distribution's random number generator.
        /// </summary>
        private Random _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="InverseWishart"/> class. 
        /// </summary>
        /// <param name="nu">
        /// The degrees of freedom for the inverse Wishart distribution.
        /// </param>
        /// <param name="s">
        /// The scale matrix for the inverse Wishart distribution.
        /// </param>
        public InverseWishart(double nu, Matrix<double> s)
        {
            SetParameters(nu, s);
            RandomSource = new Random();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return "InverseWishart(Nu = " + _nu + ", Rows = " + _s.RowCount + ", Columns = " + _s.ColumnCount + ")";
        }

        /// <summary>
        /// Sets the parameters of the distribution after checking their validity.
        /// </summary>
        /// <param name="nu">The degrees of freedom for the Wishart distribution.</param>
        /// <param name="s">The scale matrix for the Wishart distribution.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the parameters don't pass the <see cref="IsValidParameterSet"/> function.</exception>
        private void SetParameters(double nu, Matrix<double> s)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(nu, s))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            _nu = nu;
            _s = s;
            _chol = Cholesky<double>.Create(_s);
        }

        /// <summary>
        /// Checks whether the parameters of the distribution are valid. 
        /// </summary>
        /// <param name="nu">The degrees of freedom for the Wishart distribution.</param>
        /// <param name="s">The scale matrix for the Wishart distribution.</param>
        /// <returns><c>true</c> when the parameters are valid, <c>false</c> otherwise.</returns>
        private static bool IsValidParameterSet(double nu, Matrix<double> s)
        {
            if (s.RowCount != s.ColumnCount)
            {
                return false;
            }

            for (var i = 0; i < s.RowCount; i++)
            {
                if (s.At(i, i) <= 0.0)
                {
                    return false;
                }
            }

            if (nu <= 0.0 || Double.IsNaN(nu))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the degrees of freedom for the inverse Wishart distribution.
        /// </summary>
        public double Nu
        {
            get
            {
                return _nu;
            }

            set
            {
                SetParameters(value, _s);
            }
        }

        /// <summary>
        /// Gets or sets the scale matrix for the inverse Wishart distribution.
        /// </summary>
        public Matrix<double> S
        {
            get
            {
                return _s;
            }

            set
            {
                SetParameters(_nu, value);
            }
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
        /// Gets the mean.
        /// </summary>
        /// <value>The mean of the distribution.</value>
        public Matrix<double> Mean
        {
            get
            {
                return _s * (1.0 / (_nu - _s.RowCount - 1.0));
            }
        }

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        /// <value>The mode of the distribution.</value>
        /// <remarks>A. O'Hagan, and J. J. Forster (2004). Kendall's Advanced Theory of Statistics: Bayesian Inference. 2B (2 ed.). Arnold. ISBN 0-340-80752-0.</remarks>
        public Matrix<double> Mode
        {
            get
            {
                return _s * (1.0 / (_nu + _s.RowCount + 1.0));
            }
        }

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        /// <value>The variance  of the distribution.</value>
        /// <remarks>Kanti V. Mardia, J. T. Kent and J. M. Bibby (1979). Multivariate Analysis.</remarks>
        public Matrix<double> Variance
        {
            get
            {
                var res = _s.CreateMatrix(_s.RowCount, _s.ColumnCount);
                for (var i = 0; i < res.RowCount; i++)
                {
                    for (var j = 0; j < res.ColumnCount; j++)
                    {
                        var num1 = ((_nu - _s.RowCount + 1) * _s.At(i, j) * _s.At(i, j)) + ((_nu - _s.RowCount - 1) * _s.At(i, i) * _s.At(j, j));
                        var num2 = (_nu - _s.RowCount) * (_nu - _s.RowCount - 1) * (_nu - _s.RowCount - 1) * (_nu - _s.RowCount - 3);
                        res.At(i, j, num1 / num2);
                    }
                }

                return res;
            }
        }

        /// <summary>
        /// Evaluates the probability density function for the inverse Wishart distribution.
        /// </summary>
        /// <param name="x">The matrix at which to evaluate the density at.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the argument does not have the same dimensions as the scale matrix.</exception>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(Matrix<double> x)
        {
            var p = _s.RowCount;

            if (x.RowCount != p || x.ColumnCount != p)
            {
                throw new ArgumentOutOfRangeException("x", Resources.ArgumentMatrixDimensions);
            }

            var chol = Cholesky<double>.Create(x);
            var dX = chol.Determinant;
            var sXi = chol.Solve(S);

            // Compute the multivariate Gamma function.
            var gp = Math.Pow(Constants.Pi, p * (p - 1.0) / 4.0);
            for (var j = 1; j <= p; j++)
            {
                gp *= SpecialFunctions.Gamma((_nu + 1.0 - j) / 2.0);
            }

            return Math.Pow(dX, -(_nu + p + 1.0) / 2.0)
                   * Math.Exp(-0.5 * sXi.Trace())
                   * Math.Pow(_chol.Determinant, _nu / 2.0)
                   / Math.Pow(2.0, _nu * p / 2.0)
                   / gp;
        }

        /// <summary>
        /// Samples an inverse Wishart distributed random variable by sampling
        /// a Wishart random variable and inverting the matrix.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public Matrix<double> Sample()
        {
            return Sample(RandomSource, _nu, _s);
        }

        /// <summary>
        /// Samples an inverse Wishart distributed random variable by sampling
        /// a Wishart random variable and inverting the matrix.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="nu">The degrees of freedom.</param>
        /// <param name="s">The scale matrix.</param>
        /// <returns>a sample from the distribution.</returns>
        public static Matrix<double> Sample(Random rnd, double nu, Matrix<double> s)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(nu, s))
            {
                throw new ArgumentOutOfRangeException(Resources.InvalidDistributionParameters);
            }

            var r = Wishart.Sample(rnd, nu, s.Inverse());
            return r.Inverse();
        }
    }
}
