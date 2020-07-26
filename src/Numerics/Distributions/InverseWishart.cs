// <copyright file="InverseWishart.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Multivariate Inverse Wishart distribution. This distribution is
    /// parameterized by the degrees of freedom nu and the scale matrix S. The inverse Wishart distribution
    /// is the conjugate prior for the covariance matrix of a multivariate normal distribution.
    /// <a href="http://en.wikipedia.org/wiki/Inverse-Wishart_distribution">Wikipedia - Inverse-Wishart distribution</a>.
    /// </summary>
    public class InverseWishart : IDistribution
    {
        System.Random _random;

        readonly double _freedom;
        readonly Matrix<double> _scale;

        /// <summary>
        /// Caches the Cholesky factorization of the scale matrix.
        /// </summary>
        readonly Cholesky<double> _chol;

        /// <summary>
        /// Initializes a new instance of the <see cref="InverseWishart"/> class.
        /// </summary>
        /// <param name="degreesOfFreedom">The degree of freedom (ν) for the inverse Wishart distribution.</param>
        /// <param name="scale">The scale matrix (Ψ) for the inverse Wishart distribution.</param>
        public InverseWishart(double degreesOfFreedom, Matrix<double> scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(degreesOfFreedom, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _freedom = degreesOfFreedom;
            _scale = scale;
            _chol = _scale.Cholesky();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InverseWishart"/> class.
        /// </summary>
        /// <param name="degreesOfFreedom">The degree of freedom (ν) for the inverse Wishart distribution.</param>
        /// <param name="scale">The scale matrix (Ψ) for the inverse Wishart distribution.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public InverseWishart(double degreesOfFreedom, Matrix<double> scale, System.Random randomSource)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(degreesOfFreedom, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _freedom = degreesOfFreedom;
            _scale = scale;
            _chol = _scale.Cholesky();
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"InverseWishart(ν = {_freedom}, Rows = {_scale.RowCount}, Columns = {_scale.ColumnCount})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="degreesOfFreedom">The degree of freedom (ν) for the inverse Wishart distribution.</param>
        /// <param name="scale">The scale matrix (Ψ) for the inverse Wishart distribution.</param>
        public static bool IsValidParameterSet(double degreesOfFreedom, Matrix<double> scale)
        {
            if (scale.RowCount != scale.ColumnCount)
            {
                return false;
            }

            for (var i = 0; i < scale.RowCount; i++)
            {
                if (scale.At(i, i) <= 0.0)
                {
                    return false;
                }
            }

            return degreesOfFreedom > 0.0;
        }

        /// <summary>
        /// Gets or sets the degree of freedom (ν) for the inverse Wishart distribution.
        /// </summary>
        public double DegreesOfFreedom => _freedom;

        /// <summary>
        /// Gets or sets the scale matrix (Ψ) for the inverse Wishart distribution.
        /// </summary>
        public Matrix<double> Scale => _scale;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean.
        /// </summary>
        /// <value>The mean of the distribution.</value>
        public Matrix<double> Mean => _scale*(1.0/(_freedom - _scale.RowCount - 1.0));

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        /// <value>The mode of the distribution.</value>
        /// <remarks>A. O'Hagan, and J. J. Forster (2004). Kendall's Advanced Theory of Statistics: Bayesian Inference. 2B (2 ed.). Arnold. ISBN 0-340-80752-0.</remarks>
        public Matrix<double> Mode => _scale*(1.0/(_freedom + _scale.RowCount + 1.0));

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        /// <value>The variance  of the distribution.</value>
        /// <remarks>Kanti V. Mardia, J. T. Kent and J. M. Bibby (1979). Multivariate Analysis.</remarks>
        public Matrix<double> Variance
        {
            get
            {
                return Matrix<double>.Build.Dense(_scale.RowCount, _scale.ColumnCount, (i, j) =>
                {
                    var num1 = ((_freedom - _scale.RowCount + 1)*_scale.At(i, j)*_scale.At(i, j)) + ((_freedom - _scale.RowCount - 1)*_scale.At(i, i)*_scale.At(j, j));
                    var num2 = (_freedom - _scale.RowCount)*(_freedom - _scale.RowCount - 1)*(_freedom - _scale.RowCount - 1)*(_freedom - _scale.RowCount - 3);
                    return num1/num2;
                });
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
            var p = _scale.RowCount;

            if (x.RowCount != p || x.ColumnCount != p)
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Matrix dimensions must agree.");
            }

            var chol = x.Cholesky();
            var dX = chol.Determinant;
            var sXi = chol.Solve(Scale);

            // Compute the multivariate Gamma function.
            var gp = Math.Pow(Constants.Pi, p*(p - 1.0)/4.0);
            for (var j = 1; j <= p; j++)
            {
                gp *= SpecialFunctions.Gamma((_freedom + 1.0 - j)/2.0);
            }

            return Math.Pow(dX, -(_freedom + p + 1.0)/2.0)
                   *Math.Exp(-0.5*sXi.Trace())
                   *Math.Pow(_chol.Determinant, _freedom/2.0)
                   /Math.Pow(2.0, _freedom*p/2.0)
                   /gp;
        }

        /// <summary>
        /// Samples an inverse Wishart distributed random variable by sampling
        /// a Wishart random variable and inverting the matrix.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public Matrix<double> Sample()
        {
            return Sample(_random, _freedom, _scale);
        }

        /// <summary>
        /// Samples an inverse Wishart distributed random variable by sampling
        /// a Wishart random variable and inverting the matrix.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="degreesOfFreedom">The degree of freedom (ν) for the inverse Wishart distribution.</param>
        /// <param name="scale">The scale matrix (Ψ) for the inverse Wishart distribution.</param>
        /// <returns>a sample from the distribution.</returns>
        public static Matrix<double> Sample(System.Random rnd, double degreesOfFreedom, Matrix<double> scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(degreesOfFreedom, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var r = Wishart.Sample(rnd, degreesOfFreedom, scale.Inverse());
            return r.PseudoInverse();
        }
    }
}
