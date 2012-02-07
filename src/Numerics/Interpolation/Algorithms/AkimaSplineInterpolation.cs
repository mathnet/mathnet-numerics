// <copyright file="AkimaSplineInterpolation.cs" company="Math.NET">
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
    /// Akima Spline Interpolation Algorithm.
    /// </summary>
    /// <remarks>
    /// This algorithm supports both differentiation and integration.
    /// </remarks>
    public class AkimaSplineInterpolation : IInterpolation
    {
        /// <summary>
        /// Internal Spline Interpolation
        /// </summary>
        private readonly CubicHermiteSplineInterpolation _spline;

        /// <summary>
        /// Initializes a new instance of the AkimaSplineInterpolation class.
        /// </summary>
        public AkimaSplineInterpolation()
        {
            _spline = new CubicHermiteSplineInterpolation();
        }

        /// <summary>
        /// Initializes a new instance of the AkimaSplineInterpolation class.
        /// </summary>
        /// <param name="samplePoints">Sample Points t, sorted ascending.</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        public AkimaSplineInterpolation(
            IList<double> samplePoints,
            IList<double> sampleValues)
        {
            _spline = new CubicHermiteSplineInterpolation();
            Initialize(samplePoints, sampleValues);
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
        public void Initialize(
            IList<double> samplePoints,
            IList<double> sampleValues)
        {
            double[] derivatives = EvaluateSplineDerivatives(
                samplePoints,
                sampleValues);

            _spline.Initialize(samplePoints, sampleValues, derivatives);
        }

        /// <summary>
        /// Evaluate the spline derivatives as used
        /// internally by this interpolation algorithm.
        /// </summary>
        /// <param name="samplePoints">Sample Points t, sorted ascending.</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <returns>Spline Derivative Vector</returns>
        public static double[] EvaluateSplineDerivatives(
            IList<double> samplePoints,
            IList<double> sampleValues)
        {
            if (null == samplePoints)
            {
                throw new ArgumentNullException("samplePoints");
            }

            if (null == sampleValues)
            {
                throw new ArgumentNullException("sampleValues");
            }

            if (samplePoints.Count < 5)
            {
                throw new ArgumentOutOfRangeException("samplePoints");
            }

            if (samplePoints.Count != sampleValues.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            for (var i = 1; i < samplePoints.Count; ++i)
                if (samplePoints[i] <= samplePoints[i - 1])
                    throw new ArgumentException(Resources.Interpolation_Initialize_SamplePointsNotStrictlyAscendingOrder, "samplePoints");

            int n = samplePoints.Count;

            /* Prepare divided differences (diff) and weights (w) */

            var differences = new double[n - 1];
            var weights = new double[n - 1];

            for (int i = 0; i < differences.Length; i++)
            {
                differences[i] = (sampleValues[i + 1] - sampleValues[i]) / (samplePoints[i + 1] - samplePoints[i]);
            }

            for (int i = 1; i < weights.Length; i++)
            {
                weights[i] = Math.Abs(differences[i] - differences[i - 1]);
            }

            /* Prepare Hermite interpolation scheme */

            var derivatives = new double[n];

            for (int i = 2; i < derivatives.Length - 2; i++)
            {
                derivatives[i] =
                    weights[i - 1].AlmostEqual(0.0) && weights[i + 1].AlmostEqual(0.0)
                        ? (((samplePoints[i + 1] - samplePoints[i]) * differences[i - 1])
                           + ((samplePoints[i] - samplePoints[i - 1]) * differences[i]))
                          / (samplePoints[i + 1] - samplePoints[i - 1])
                        : ((weights[i + 1] * differences[i - 1])
                           + (weights[i - 1] * differences[i]))
                          / (weights[i + 1] + weights[i - 1]);
            }

            derivatives[0] = DifferentiateThreePoint(samplePoints, sampleValues, 0, 0, 1, 2);
            derivatives[1] = DifferentiateThreePoint(samplePoints, sampleValues, 1, 0, 1, 2); 
            derivatives[n - 2] = DifferentiateThreePoint(samplePoints, sampleValues, n - 2, n - 3, n - 2, n - 1);
            derivatives[n - 1] = DifferentiateThreePoint(samplePoints, sampleValues, n - 1, n - 3, n - 2, n - 1);

            /* Build Akima spline using Hermite interpolation scheme */

            return derivatives;
        }

        /// <summary>
        /// Evaluate the spline coefficients as used
        /// internally by this interpolation algorithm.
        /// </summary>
        /// <param name="samplePoints">Sample Points t, sorted ascending.</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <returns>Spline Coefficient Vector</returns>
        public static double[] EvaluateSplineCoefficients(
            IList<double> samplePoints,
            IList<double> sampleValues)
        {
            double[] derivatives = EvaluateSplineDerivatives(
                samplePoints,
                sampleValues);

            return CubicHermiteSplineInterpolation.EvaluateSplineCoefficients(
                samplePoints,
                sampleValues,
                derivatives);
        }

        /// <summary>
        /// Three-Point Differentiation Helper.
        /// </summary>
        /// <param name="samplePoints">Sample Points t.</param>
        /// <param name="sampleValues">Sample Values x(t).</param>
        /// <param name="indexT">Index of the point of the differentiation.</param>
        /// <param name="index0">Index of the first sample.</param>
        /// <param name="index1">Index of the second sample.</param>
        /// <param name="index2">Index of the third sample.</param>
        /// <returns>The derivative approximation.</returns>
        private static double DifferentiateThreePoint(
            IList<double> samplePoints,
            IList<double> sampleValues,
            int indexT,
            int index0,
            int index1,
            int index2)
        {
            double x0 = sampleValues[index0];
            double x1 = sampleValues[index1];
            double x2 = sampleValues[index2];

            double t = samplePoints[indexT] - samplePoints[index0];
            double t1 = samplePoints[index1] - samplePoints[index0];
            double t2 = samplePoints[index2] - samplePoints[index0];

            double a = (x2 - x0 - (t2 / t1 * (x1 - x0))) / ((t2 * t2) - (t1 * t2));
            double b = (x1 - x0 - (a * t1 * t1)) / t1;
            return (2 * a * t) + b;
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