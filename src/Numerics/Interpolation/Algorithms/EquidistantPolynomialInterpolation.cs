// <copyright file="EquidistantPolynomialInterpolation.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.Interpolation.Algorithms
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Barycentric Polynomial Interpolation where the given sample points are equidistant.
    /// </summary>
    /// <remarks>
    /// This algorithm neither supports differentiation nor integration.
    /// </remarks>
    public class EquidistantPolynomialInterpolation : IInterpolation
    {
        /// <summary>
        /// Internal Barycentric Interpolation
        /// </summary>
        private readonly BarycentricInterpolation _barycentric;

        /// <summary>
        /// Initializes a new instance of the EquidistantPolynomialInterpolation class.
        /// </summary>
        public EquidistantPolynomialInterpolation()
        {
            _barycentric = new BarycentricInterpolation();
        }

        /// <summary>
        /// Initializes a new instance of the EquidistantPolynomialInterpolation class.
        /// </summary>
        /// <param name="leftBound">Left bound of the sample point interval.</param>
        /// <param name="rightBound">Right bound of the sample point interval.</param>
        /// <param name="sampleValues">Sample Values x(t) where t is equidistant over [a,b], i.e. x[i] = x(a+(b-a)*i/(n-1))</param>
        public EquidistantPolynomialInterpolation(
            double leftBound,
            double rightBound,
            IList<double> sampleValues)
        {
            _barycentric = new BarycentricInterpolation();
            Initialize(leftBound, rightBound, sampleValues);
        }

        /// <summary>
        /// Initializes a new instance of the EquidistantPolynomialInterpolation class.
        /// </summary>
        /// <param name="samplePoints">Equidistant Sample Points t = a+(b-a)*i/(n-1)</param>
        /// <param name="sampleValues">Sample Values x(t) where t are equidistant over [a,b], i.e. x[i] = x(a+(b-a)*i/(n-1))</param>
        public EquidistantPolynomialInterpolation(
            IList<double> samplePoints,
            IList<double> sampleValues)
        {
            _barycentric = new BarycentricInterpolation();
            Initialize(samplePoints, sampleValues);
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        /// <seealso cref="IInterpolation.Differentiate(double)"/>
        /// <seealso cref="IInterpolation.Differentiate(double, out double, out double)"/>
        bool IInterpolation.SupportsDifferentiation
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        /// <seealso cref="IInterpolation.Integrate"/>
        bool IInterpolation.SupportsIntegration
        {
            get { return false; }
        }

        /// <summary>
        /// Initialize the interpolation method with the given sampls in the interval [leftBound,rightBound].
        /// </summary>
        /// <param name="leftBound">Left bound of the sample point interval.</param>
        /// <param name="rightBound">Right bound of the sample point interval.</param>
        /// <param name="sampleValues">Sample Values x(t) where t are equidistant over [a,b], i.e. x[i] = x(a+(b-a)*i/(n-1))</param>
        public void Initialize(
            double leftBound,
            double rightBound,
            IList<double> sampleValues)
        {
            if (null == sampleValues)
            {
                throw new ArgumentNullException("sampleValues");
            }

            if (sampleValues.Count < 1)
            {
                throw new ArgumentOutOfRangeException("sampleValues");
            }

            var samplePoints = new double[sampleValues.Count];
            samplePoints[0] = leftBound;
            double step = (rightBound - leftBound) / (samplePoints.Length - 1);
            for (int i = 1; i < samplePoints.Length; i++)
            {
                samplePoints[i] = samplePoints[i - 1] + step;
            }

            var weights = EvaluateBarycentricWeights(sampleValues.Count);

            _barycentric.Initialize(samplePoints, sampleValues, weights);
        }

        /// <summary>
        /// Initialize the interpolation method with the given sample set (no sorting assumed).
        /// </summary>
        /// <param name="samplePoints">Equidistant Sample Points t = a+(b-a)*i/(n-1)</param>
        /// <param name="sampleValues">Sample Values x(t) where t are equidistant over [a,b], i.e. x[i] = x(a+(b-a)*i/(n-1))</param>
        public void Initialize(
            IList<double> samplePoints,
            IList<double> sampleValues)
        {
            if (null == sampleValues)
            {
                throw new ArgumentNullException("sampleValues");
            }

            var weights = EvaluateBarycentricWeights(sampleValues.Count);

            _barycentric.Initialize(samplePoints, sampleValues, weights);
        }

        /// <summary>
        /// Evaluate the barycentric weights as used
        /// internally by this interpolation algorithm.
        /// </summary>
        /// <param name="sampleCount">Count of Sample Values x(t).</param>
        /// <returns>Barycentric Weight Vector</returns>
        public static double[] EvaluateBarycentricWeights(
            int sampleCount)
        {
            if (sampleCount < 1)
            {
                throw new ArgumentOutOfRangeException("sampleCount");
            }

            var weights = new double[sampleCount];
            weights[0] = 1.0;
            for (int i = 1; i < weights.Length; i++)
            {
                weights[i] = -(weights[i - 1] * (weights.Length - i)) / i;
            }

            return weights;
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            return _barycentric.Interpolate(t);
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        /// <seealso cref="IInterpolation.SupportsDifferentiation"/>
        /// <seealso cref="IInterpolation.Differentiate(double, out double, out double)"/>
        double IInterpolation.Differentiate(double t)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <param name="interpolatedValue">Interpolated value x(t)</param>
        /// <param name="secondDerivative">Interpolated second derivative at point t.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        /// <seealso cref="IInterpolation.SupportsDifferentiation"/>
        /// <seealso cref="IInterpolation.Differentiate(double)"/>
        double IInterpolation.Differentiate(
            double t,
            out double interpolatedValue,
            out double secondDerivative)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Integrate up to point t.
        /// </summary>
        /// <param name="t">Right bound of the integration interval [a,t].</param>
        /// <returns>Interpolated definite integral over the interval [a,t].</returns>
        /// <seealso cref="IInterpolation.SupportsIntegration"/>
        double IInterpolation.Integrate(double t)
        {
            throw new NotSupportedException();
        }
    }
}
