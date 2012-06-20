// <copyright file="CubicHermiteSplineInterpolation.cs" company="Math.NET">
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
    using Properties;

    /// <summary>
    /// Cubic Hermite Spline Interpolation Algorithm.
    /// </summary>
    /// <remarks>
    /// This algorithm supports both differentiation and integration.
    /// </remarks>
    public class CubicHermiteSplineInterpolation : IInterpolation
    {
        /// <summary>
        /// Internal Spline Interpolation
        /// </summary>
        private readonly SplineInterpolation _spline;

        /// <summary>
        /// Initializes a new instance of the CubicHermiteSplineInterpolation class.
        /// </summary>
        public CubicHermiteSplineInterpolation()
        {
            _spline = new SplineInterpolation();
        }

        /// <summary>
        /// Initializes a new instance of the CubicHermiteSplineInterpolation class.
        /// </summary>
        /// <param name="samplePoints">Sample Points t, sorted ascending.</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <param name="sampleDerivatives">Sample Derivatives x'(t)</param>
        public CubicHermiteSplineInterpolation(
            IList<double> samplePoints,
            IList<double> sampleValues,
            IList<double> sampleDerivatives)
        {
            _spline = new SplineInterpolation();
            Initialize(samplePoints, sampleValues, sampleDerivatives);
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        /// <seealso cref="Differentiate(double)"/>
        /// <seealso cref="Differentiate(double, out double, out double)"/>
        bool IInterpolation.SupportsDifferentiation
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        /// <seealso cref="Integrate"/>
        bool IInterpolation.SupportsIntegration
        {
            get { return true; }
        }

        /// <summary>
        /// Initialize the interpolation method with the given spline coefficients (sorted by the sample points t).
        /// </summary>
        /// <param name="samplePoints">Sample Points t, sorted ascending.</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <param name="sampleDerivatives">Sample Derivatives x'(t)</param>
        public void Initialize(
            IList<double> samplePoints,
            IList<double> sampleValues,
            IList<double> sampleDerivatives)
        {
            double[] coefficients = EvaluateSplineCoefficients(
                samplePoints,
                sampleValues,
                sampleDerivatives);

            _spline.Initialize(samplePoints, coefficients);
        }

        /// <summary>
        /// Evaluate the spline coefficients as used
        /// internally by this interpolation algorithm.
        /// </summary>
        /// <param name="samplePoints">Sample Points t, sorted ascending.</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <param name="sampleDerivatives">Sample Derivatives x'(t)</param>
        /// <returns>Spline Coefficient Vector</returns>
        public static double[] EvaluateSplineCoefficients(
            IList<double> samplePoints,
            IList<double> sampleValues,
            IList<double> sampleDerivatives)
        {
            if (null == samplePoints)
            {
                throw new ArgumentNullException("samplePoints");
            }

            if (null == sampleValues)
            {
                throw new ArgumentNullException("sampleValues");
            }

            if (null == sampleDerivatives)
            {
                throw new ArgumentNullException("sampleDerivatives");
            }

            if (samplePoints.Count < 2)
            {
                throw new ArgumentOutOfRangeException("samplePoints");
            }

            if (samplePoints.Count != sampleValues.Count
                || samplePoints.Count != sampleDerivatives.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            for (var i = 1; i < samplePoints.Count; ++i)
                if (samplePoints[i] <= samplePoints[i - 1])
                    throw new ArgumentException(Resources.Interpolation_Initialize_SamplePointsNotStrictlyAscendingOrder, "samplePoints");

            double[] coefficients = new double[4 * (samplePoints.Count - 1)];

            for (int i = 0, j = 0; i < samplePoints.Count - 1; i++, j += 4)
            {
                double delta = samplePoints[i + 1] - samplePoints[i];
                double delta2 = delta * delta;
                double delta3 = delta * delta2;
                coefficients[j] = sampleValues[i];
                coefficients[j + 1] = sampleDerivatives[i];
                coefficients[j + 2] = ((3 * (sampleValues[i + 1] - sampleValues[i])) - (2 * sampleDerivatives[i] * delta) - (sampleDerivatives[i + 1] * delta)) / delta2;
                coefficients[j + 3] = ((2 * (sampleValues[i] - sampleValues[i + 1])) + (sampleDerivatives[i] * delta) + (sampleDerivatives[i + 1] * delta)) / delta3;
            }

            return coefficients;
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            return _spline.Interpolate(t);
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        /// <seealso cref="IInterpolation.SupportsDifferentiation"/>
        /// <seealso cref="Differentiate(double, out double, out double)"/>
        public double Differentiate(double t)
        {
            return _spline.Differentiate(t);
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <param name="interpolatedValue">Interpolated value x(t)</param>
        /// <param name="secondDerivative">Interpolated second derivative at point t.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        /// <seealso cref="IInterpolation.SupportsDifferentiation"/>
        /// <seealso cref="Differentiate(double)"/>
        public double Differentiate(
            double t,
            out double interpolatedValue,
            out double secondDerivative)
        {
            return _spline.Differentiate(t, out interpolatedValue, out secondDerivative);
        }

        /// <summary>
        /// Integrate up to point t.
        /// </summary>
        /// <param name="t">Right bound of the integration interval [a,t].</param>
        /// <returns>Interpolated definite integral over the interval [a,t].</returns>
        /// <seealso cref="IInterpolation.SupportsIntegration"/>
        public double Integrate(double t)
        {
            return _spline.Integrate(t);
        }
    }
}