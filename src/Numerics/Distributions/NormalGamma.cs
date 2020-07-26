// <copyright file="NormalGamma.cs" company="Math.NET">
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
using System.Collections.Generic;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.Distributions
{
    /// <summary>
    /// This structure represents the type over which the <see cref="NormalGamma"/> distribution
    /// is defined.
    /// </summary>
    public struct MeanPrecisionPair
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeanPrecisionPair"/> struct.
        /// </summary>
        /// <param name="m">The mean of the pair.</param>
        /// <param name="p">The precision of the pair.</param>
        public MeanPrecisionPair(double m, double p)
        {
            Mean = m;
            Precision = p;
        }

        /// <summary>
        /// Gets or sets the mean of the pair.
        /// </summary>
        public double Mean { get; set; }

        /// <summary>
        /// Gets or sets the precision of the pair.
        /// </summary>
        public double Precision { get; set; }
    }

    /// <summary>
    /// Multivariate Normal-Gamma Distribution.
    /// <para>The <see cref="NormalGamma"/> distribution is the conjugate prior distribution for the <see cref="Normal"/>
    /// distribution. It specifies a prior over the mean and precision of the <see cref="Normal"/> distribution.</para>
    /// <para>It is parameterized by four numbers: the mean location, the mean scale, the precision shape and the
    /// precision inverse scale.</para>
    /// <para>The distribution NG(mu, tau | mloc,mscale,psscale,pinvscale) = Normal(mu | mloc, 1/(mscale*tau)) * Gamma(tau | psscale,pinvscale).</para>
    /// <para>The following degenerate cases are special: when the precision is known,
    /// the precision shape will encode the value of the precision while the precision inverse scale is positive
    /// infinity. When the mean is known, the mean location will encode the value of the mean while the scale
    /// will be positive infinity. A completely degenerate NormalGamma distribution with known mean and precision is possible as well.</para>
    /// <a href="http://en.wikipedia.org/wiki/Normal-gamma_distribution">Wikipedia - Normal-Gamma distribution</a>.
    /// </summary>
    public class NormalGamma : IDistribution
    {
        System.Random _random;

        readonly double _meanLocation;
        readonly double _meanScale;
        readonly double _precisionShape;
        readonly double _precisionInvScale;

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalGamma"/> class.
        /// </summary>
        /// <param name="meanLocation">The location of the mean.</param>
        /// <param name="meanScale">The scale of the mean.</param>
        /// <param name="precisionShape">The shape of the precision.</param>
        /// <param name="precisionInverseScale">The inverse scale of the precision.</param>
        public NormalGamma(double meanLocation, double meanScale, double precisionShape, double precisionInverseScale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(meanLocation, meanScale, precisionShape, precisionInverseScale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = SystemRandomSource.Default;
            _meanLocation = meanLocation;
            _meanScale = meanScale;
            _precisionShape = precisionShape;
            _precisionInvScale = precisionInverseScale;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalGamma"/> class.
        /// </summary>
        /// <param name="meanLocation">The location of the mean.</param>
        /// <param name="meanScale">The scale of the mean.</param>
        /// <param name="precisionShape">The shape of the precision.</param>
        /// <param name="precisionInverseScale">The inverse scale of the precision.</param>
        /// <param name="randomSource">The random number generator which is used to draw random samples.</param>
        public NormalGamma(double meanLocation, double meanScale, double precisionShape, double precisionInverseScale, System.Random randomSource)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(meanLocation, meanScale, precisionShape, precisionInverseScale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            _random = randomSource ?? SystemRandomSource.Default;
            _meanLocation = meanLocation;
            _meanScale = meanScale;
            _precisionShape = precisionShape;
            _precisionInvScale = precisionInverseScale;
        }

        /// <summary>
        /// A string representation of the distribution.
        /// </summary>
        /// <returns>a string representation of the distribution.</returns>
        public override string ToString()
        {
            return $"NormalGamma(Mean Location = {_meanLocation}, Mean Scale = {_meanScale}, Precision Shape = {_precisionShape}, Precision Inverse Scale = {_precisionInvScale})";
        }

        /// <summary>
        /// Tests whether the provided values are valid parameters for this distribution.
        /// </summary>
        /// <param name="meanLocation">The location of the mean.</param>
        /// <param name="meanScale">The scale of the mean.</param>
        /// <param name="precShape">The shape of the precision.</param>
        /// <param name="precInvScale">The inverse scale of the precision.</param>
        public static bool IsValidParameterSet(double meanLocation, double meanScale, double precShape, double precInvScale)
        {
            return meanScale > 0.0 && precShape > 0.0 && precInvScale > 0.0 && !double.IsNaN(meanLocation);
        }

        /// <summary>
        /// Gets the location of the mean.
        /// </summary>
        public double MeanLocation => _meanLocation;

        /// <summary>
        /// Gets the scale of the mean.
        /// </summary>
        public double MeanScale => _meanScale;

        /// <summary>
        /// Gets the shape of the precision.
        /// </summary>
        public double PrecisionShape => _precisionShape;

        /// <summary>
        /// Gets the inverse scale of the precision.
        /// </summary>
        public double PrecisionInverseScale => _precisionInvScale;

        /// <summary>
        /// Gets or sets the random number generator which is used to draw random samples.
        /// </summary>
        public System.Random RandomSource
        {
            get => _random;
            set => _random = value ?? SystemRandomSource.Default;
        }

        /// <summary>
        /// Returns the marginal distribution for the mean of the <c>NormalGamma</c> distribution.
        /// </summary>
        /// <returns>the marginal distribution for the mean of the <c>NormalGamma</c> distribution.</returns>
        public StudentT MeanMarginal()
        {
            if (double.IsPositiveInfinity(_precisionInvScale))
            {
                return new StudentT(_meanLocation, 1.0/(_meanScale*_precisionShape), double.PositiveInfinity);
            }

            return new StudentT(_meanLocation, Math.Sqrt(_precisionInvScale/(_meanScale*_precisionShape)), 2.0*_precisionShape);
        }

        /// <summary>
        /// Returns the marginal distribution for the precision of the <see cref="NormalGamma"/> distribution.
        /// </summary>
        /// <returns>The marginal distribution for the precision of the <see cref="NormalGamma"/> distribution/</returns>
        public Gamma PrecisionMarginal()
        {
            return new Gamma(_precisionShape, _precisionInvScale);
        }

        /// <summary>
        /// Gets the mean of the distribution.
        /// </summary>
        /// <value>The mean of the distribution.</value>
        public MeanPrecisionPair Mean => double.IsPositiveInfinity(_precisionInvScale) ? new MeanPrecisionPair(_meanLocation, _precisionShape) : new MeanPrecisionPair(_meanLocation, _precisionShape/_precisionInvScale);

        /// <summary>
        /// Gets the variance of the distribution.
        /// </summary>
        /// <value>The mean of the distribution.</value>
        public MeanPrecisionPair Variance => new MeanPrecisionPair(_precisionInvScale/(_meanScale*(_precisionShape - 1)), _precisionShape/Math.Sqrt(_precisionInvScale));

        /// <summary>
        /// Evaluates the probability density function for a NormalGamma distribution.
        /// </summary>
        /// <param name="mp">The mean/precision pair of the distribution</param>
        /// <returns>Density value</returns>
        public double Density(MeanPrecisionPair mp)
        {
            return Density(mp.Mean, mp.Precision);
        }

        /// <summary>
        /// Evaluates the probability density function for a NormalGamma distribution.
        /// </summary>
        /// <param name="mean">The mean of the distribution</param>
        /// <param name="prec">The precision of the distribution</param>
        /// <returns>Density value</returns>
        public double Density(double mean, double prec)
        {
            if (double.IsPositiveInfinity(_precisionInvScale) && _meanScale == 0.0)
            {
                throw new NotSupportedException();
            }

            if (double.IsPositiveInfinity(_precisionInvScale))
            {
                throw new NotSupportedException();
            }

            if (_meanScale <= 0.0)
            {
                throw new NotSupportedException();
            }

            if (_precisionShape > 160.0)
            {
                return Math.Exp(DensityLn(mean, prec));
            }

            // double e = -0.5 * prec * (mean - _meanLocation) * (mean - _meanLocation) - prec * _precisionInvScale;
            // return Math.Pow(prec * _precisionInvScale, _precisionShape) * Math.Exp(e) / (Constants.Sqrt2Pi * Math.Sqrt(prec) * SpecialFunctions.Gamma(_precisionShape));
            double e = -(0.5*prec*_meanScale*(mean - _meanLocation)*(mean - _meanLocation)) - (prec*_precisionInvScale);
            return Math.Pow(prec*_precisionInvScale, _precisionShape)*Math.Exp(e)*Math.Sqrt(_meanScale)
                   /(Constants.Sqrt2Pi*Math.Sqrt(prec)*SpecialFunctions.Gamma(_precisionShape));
        }

        /// <summary>
        /// Evaluates the log probability density function for a NormalGamma distribution.
        /// </summary>
        /// <param name="mp">The mean/precision pair of the distribution</param>
        /// <returns>The log of the density value</returns>
        public double DensityLn(MeanPrecisionPair mp)
        {
            return DensityLn(mp.Mean, mp.Precision);
        }

        /// <summary>
        /// Evaluates the log probability density function for a NormalGamma distribution.
        /// </summary>
        /// <param name="mean">The mean of the distribution</param>
        /// <param name="prec">The precision of the distribution</param>
        /// <returns>The log of the density value</returns>
        public double DensityLn(double mean, double prec)
        {
            if (double.IsPositiveInfinity(_precisionInvScale) && _meanScale == 0.0)
            {
                throw new NotSupportedException();
            }

            if (double.IsPositiveInfinity(_precisionInvScale))
            {
                throw new NotSupportedException();
            }

            if (_meanScale <= 0.0)
            {
                throw new NotSupportedException();
            }

            // double e = -0.5 * prec * (mean - _meanLocation) * (mean - _meanLocation) - prec * _precisionInvScale;
            // return (_precisionShape - 0.5) * Math.Log(prec) + _precisionShape * Math.Log(_precisionInvScale) + e - Constants.LogSqrt2Pi - SpecialFunctions.GammaLn(_precisionShape);
            double e = -(0.5*prec*_meanScale*(mean - _meanLocation)*(mean - _meanLocation)) - (prec*_precisionInvScale);
            return ((_precisionShape - 0.5)*Math.Log(prec)) + (_precisionShape*Math.Log(_precisionInvScale)) - (0.5*Math.Log(_meanScale)) + e - Constants.LogSqrt2Pi - SpecialFunctions.GammaLn(_precisionShape);
        }

        /// <summary>
        /// Generates a sample from the <c>NormalGamma</c> distribution.
        /// </summary>
        /// <returns>a sample from the distribution.</returns>
        public MeanPrecisionPair Sample()
        {
            return Sample(_random, _meanLocation, _meanScale, _precisionShape, _precisionInvScale);
        }

        /// <summary>
        /// Generates a sequence of samples from the <c>NormalGamma</c> distribution
        /// </summary>
        /// <returns>a sequence of samples from the distribution.</returns>
        public IEnumerable<MeanPrecisionPair> Samples()
        {
            while (true)
            {
                yield return Sample(_random, _meanLocation, _meanScale, _precisionShape, _precisionInvScale);
            }
        }

        /// <summary>
        /// Generates a sample from the <c>NormalGamma</c> distribution.
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="meanLocation">The location of the mean.</param>
        /// <param name="meanScale">The scale of the mean.</param>
        /// <param name="precisionShape">The shape of the precision.</param>
        /// <param name="precisionInverseScale">The inverse scale of the precision.</param>
        /// <returns>a sample from the distribution.</returns>
        public static MeanPrecisionPair Sample(System.Random rnd, double meanLocation, double meanScale, double precisionShape, double precisionInverseScale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(meanLocation, meanScale, precisionShape, precisionInverseScale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            var mp = new MeanPrecisionPair();

            // Sample the precision.
            mp.Precision = double.IsPositiveInfinity(precisionInverseScale) ? precisionShape : Gamma.Sample(rnd, precisionShape, precisionInverseScale);

            // Sample the mean.
            mp.Mean = meanScale == 0.0 ? meanLocation : Normal.Sample(rnd, meanLocation, Math.Sqrt(1.0/(meanScale*mp.Precision)));

            return mp;
        }

        /// <summary>
        /// Generates a sequence of samples from the NormalGamma distribution
        /// </summary>
        /// <param name="rnd">The random number generator to use.</param>
        /// <param name="meanLocation">The location of the mean.</param>
        /// <param name="meanScale">The scale of the mean.</param>
        /// <param name="precisionShape">The shape of the precision.</param>
        /// <param name="precisionInvScale">The inverse scale of the precision.</param>
        /// <returns>a sequence of samples from the distribution.</returns>
        public static IEnumerable<MeanPrecisionPair> Samples(System.Random rnd, double meanLocation, double meanScale, double precisionShape, double precisionInvScale)
        {
            if (Control.CheckDistributionParameters && !IsValidParameterSet(meanLocation, meanScale, precisionShape, precisionInvScale))
            {
                throw new ArgumentException("Invalid parametrization for the distribution.");
            }

            while (true)
            {
                var mp = new MeanPrecisionPair();

                // Sample the precision.
                mp.Precision = double.IsPositiveInfinity(precisionInvScale) ? precisionShape : Gamma.Sample(rnd, precisionShape, precisionInvScale);

                // Sample the mean.
                mp.Mean = meanScale == 0.0 ? meanLocation : Normal.Sample(rnd, meanLocation, Math.Sqrt(1.0/(meanScale*mp.Precision)));

                yield return mp;
            }
        }
    }
}
