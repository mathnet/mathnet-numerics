// <copyright file="Wishart.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// Multivariate Wishart distribution. This distribution is
    /// parameterized by the degrees of freedom nu and the scale matrix S. The Wishart distribution
    /// is the conjugate prior for the precision (inverse covariance) matrix of the multivariate
    /// normal distribution.
    /// <a href="http://en.wikipedia.org/wiki/Wishart_distribution">Wikipedia - Wishart distribution</a>.
    /// </summary>
    public class Wishart : IDistribution
    {
        System.Random _random;

        /// <summary>
        /// The degrees of freedom for the Wishart distribution.
        /// </summary>
        readonly double _degreesOfFreedom;

        /// <summary>
        /// The scale matrix for the Wishart distribution.
        /// </summary>
        readonly Matrix<double> _scale;

        /// <summary>
        /// Caches the Cholesky factorization of the scale matrix.
        /// </summary>
        readonly Cholesky<double> _chol;

        /// <summary>
        /// Initializes a new instance of the <see cref="Wishart"/> class.
        /// </summary>
        /// <param name="degreesOfFreedom">The degrees of freedom (n) for the Wishart distribution.</param>
        /// <param name="scale">The scale matrix (V) for the Wishart distribution.</param>
        public Wishart(double degreesOfFreedom, Matrix<double> scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(degreesOfFreedom, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _degreesOfFreedom = degreesOfFreedom;
            _scale = scale;
            _chol = _scale.Cholesky();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Wishart"/> class.
        /// </summary>
        /// <param name="degreesOfFreedom">The degrees of freedom (n) for the Wishart distribution.</param>
        /// <param name="scale">The scale matrix (V) for the Wishart distribution.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public Wishart(double degreesOfFreedom, Matrix<double> scale, System.Random randomSource)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(degreesOfFreedom, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _degreesOfFreedom = degreesOfFreedom;
            _scale = scale;
            _chol = _scale.Cholesky();
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="degreesOfFreedom">The degrees of freedom (n) for the Wishart distribution.</param>
        /// <param name="scale">The scale matrix (V) for the Wishart distribution.</param>
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

            if (degreesOfFreedom <= 0.0 || double.IsNaN(degreesOfFreedom))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the degrees of freedom (n) for the Wishart distribution.
        /// </summary>
        public double DegreesOfFreedom => _degreesOfFreedom;

        /// <summary>
        /// Gets or sets the scale matrix (V) for the Wishart distribution.
        /// </summary>
        public Matrix<double> Scale => _scale;

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"Wishart(DegreesOfFreedom = {_degreesOfFreedom}, Rows = {_scale.RowCount}, Columns = {_scale.ColumnCount})";
        }

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        /// <value>The mean of the distribution.</value>
        public Matrix<double> Mean => _degreesOfFreedom*_scale;

        /// <summary>
        /// Gets the mode of the distribution.
        /// </summary>
        /// <value>The mode of the distribution.</value>
        public Matrix<double> Mode => (_degreesOfFreedom - _scale.RowCount - 1.0)*_scale;

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        /// <value>The variance  of the distribution.</value>
        public Matrix<double> Variance
        {
            get
            {
                return Matrix<double>.Build.Dense(_scale.RowCount, _scale.ColumnCount,
                    (i, j) => _degreesOfFreedom*((_scale.At(i, j)*_scale.At(i, j)) + (_scale.At(i, i)*_scale.At(j, j))));
            }
        }

        /// <summary>
        /// Evaluates the probability density function for the Wishart distribution.
        /// </summary>
        /// <param name="x">The matrix at which to evaluate the density at.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the argument does not have the same dimensions as the scale matrix.</exception>
        /// <returns>the density at <paramref name="x"/>.</returns>
        public double Density(Matrix<double> x)
        {
            var p = _scale.RowCount;

            if (x.RowCount != p || x.ColumnCount != p)
            {
                throw Matrix.DimensionsDontMatch<ArgumentOutOfRangeException>(x, _scale, "x");
            }

            var dX = x.Determinant();
            var siX = _chol.Solve(x);

            // Compute the multivariate Gamma function.
            var gp = Math.Pow(Constants.Pi, p*(p - 1.0)/4.0);
            for (var j = 1; j <= p; j++)
            {
                gp *= SpecialFunctions.Gamma((_degreesOfFreedom + 1.0 - j)/2.0);
            }

            return Math.Pow(dX, (_degreesOfFreedom - p - 1.0)/2.0)
                   *Math.Exp(-0.5*siX.Trace())
                   /Math.Pow(2.0, _degreesOfFreedom*p/2.0)
                   /Math.Pow(_chol.Determinant, _degreesOfFreedom/2.0)
                   /gp;
        }

        /// <summary>
        /// Samples a Wishart distributed random variable using the method
        ///     Algorithm AS 53: Wishart Variate Generator
        ///     W. B. Smith and R. R. Hocking
        ///     Applied Statistics, Vol. 21, No. 3 (1972), pp. 341-345
        /// </summary>
        /// <returns>A random number from this distribution.</returns>
        public Matrix<double> Sample()
        {
            return DoSample(RandomSource, _degreesOfFreedom, _scale, _chol);
        }

        /// <summary>
        /// Samples a Wishart distributed random variable using the method
        ///     Algorithm AS 53: Wishart Variate Generator
        ///     W. B. Smith and R. R. Hocking
        ///     Applied Statistics, Vol. 21, No. 3 (1972), pp. 341-345
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom (n) for the Wishart distribution.</param>
        /// <param name="scale">The scale matrix (V) for the Wishart distribution.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static Matrix<double> Sample(System.Random rnd, double degreesOfFreedom, Matrix<double> scale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(degreesOfFreedom, scale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            return DoSample(rnd, degreesOfFreedom, scale, scale.Cholesky());
        }

        /// <summary>
        /// Samples the distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="degreesOfFreedom">The degrees of freedom (n) for the Wishart distribution.</param>
        /// <param name="scale">The scale matrix (V) for the Wishart distribution.</param>
        /// <param name="chol">The cholesky decomposition to use.</param>
        /// <returns>a random number from the distribution.</returns>
        static Matrix<double> DoSample(System.Random rnd, double degreesOfFreedom, Matrix<double> scale, Cholesky<double> chol)
        {
            var count = scale.RowCount;

            // First generate a lower triangular matrix with Sqrt(Chi-Squares) on the diagonal
            // and normal distributed variables in the lower triangle.
            var a = new DenseMatrix(count, count);
            for (var d = 0; d < count; d++)
            {
                a.At(d, d, Math.Sqrt(Gamma.Sample(rnd, (degreesOfFreedom - d)/2.0, 0.5)));
            }

            for (var i = 1; i < count; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    a.At(i, j, Normal.Sample(rnd, 0.0, 1.0));
                }
            }

            var factor = chol.Factor;
            return factor*a*a.Transpose()*factor.Transpose();
        }
    }
}
