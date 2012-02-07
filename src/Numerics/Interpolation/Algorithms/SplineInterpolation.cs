// <copyright file="SplineInterpolation.cs" company="Math.NET">
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
    /// Third-Degree Spline Interpolation Algorithm.
    /// </summary>
    /// <remarks>
    /// This algorithm supports both differentiation and integration.
    /// </remarks>
    public class SplineInterpolation : IInterpolation
    {
        /// <summary>
        /// Sample Points t.
        /// </summary>
        private IList<double> _points;

        /// <summary>
        /// Spline Coefficients c(t).
        /// </summary>
        private IList<double> _coefficients;

        /// <summary>
        /// Number of samples.
        /// </summary>
        private int _sampleCount;

        /// <summary>
        /// Initializes a new instance of the SplineInterpolation class.
        /// </summary>
        public SplineInterpolation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SplineInterpolation class.
        /// </summary>
        /// <param name="samplePoints">Sample Points t (length: N), sorted ascending.</param>
        /// <param name="splineCoefficients">Spline Coefficients (length: 4*(N-1)).</param>
        public SplineInterpolation(
            IList<double> samplePoints,
            IList<double> splineCoefficients)
        {
            Initialize(samplePoints, splineCoefficients);
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
        /// <param name="samplePoints">Sample Points t (length: N), sorted ascending.</param>
        /// <param name="splineCoefficients">Spline Coefficients (length: 4*(N-1)).</param>
        public void Initialize(
            IList<double> samplePoints,
            IList<double> splineCoefficients)
        {
            if (null == samplePoints)
            {
                throw new ArgumentNullException("samplePoints");
            }

            if (null == splineCoefficients)
            {
                throw new ArgumentNullException("splineCoefficients");
            }

            if (samplePoints.Count < 2)
            {
                throw new ArgumentOutOfRangeException("samplePoints");
            }

            if (splineCoefficients.Count != 4 * (samplePoints.Count - 1))
            {
                throw new ArgumentOutOfRangeException("splineCoefficients");
            }

            for (var i = 1; i < samplePoints.Count; ++i)
                if (samplePoints[i] <= samplePoints[i - 1])
                    throw new ArgumentException(Resources.Interpolation_Initialize_SamplePointsNotStrictlyAscendingOrder, "samplePoints");

            _points = samplePoints;
            _coefficients = splineCoefficients;
            _sampleCount = samplePoints.Count;
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            int closestLeftIndex = IndexOfClosestPointLeftOf(t);

            // Interpolation
            double offset = t - _points[closestLeftIndex];
            int k = closestLeftIndex << 2;

            return _coefficients[k]
                   + (offset * (_coefficients[k + 1]
                                + (offset * (_coefficients[k + 2]
                                             + (offset * _coefficients[k + 3])))));
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
            int closestLeftIndex = IndexOfClosestPointLeftOf(t);

            // Differentiation
            double offset = t - _points[closestLeftIndex];
            int k = closestLeftIndex << 2;

            return _coefficients[k + 1]
                   + (2 * offset * _coefficients[k + 2])
                   + (3 * offset * offset * _coefficients[k + 3]);
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
            int closestLeftIndex = IndexOfClosestPointLeftOf(t);

            // Differentiation
            double offset = t - _points[closestLeftIndex];
            int k = closestLeftIndex << 2;

            interpolatedValue = _coefficients[k]
                                + (offset * (_coefficients[k + 1]
                                             + (offset * (_coefficients[k + 2]
                                                          + (offset * _coefficients[k + 3])))));

            secondDerivative = (2 * _coefficients[k + 2])
                               + (6 * offset * _coefficients[k + 3]);

            return _coefficients[k + 1]
                   + (2 * offset * _coefficients[k + 2])
                   + (3 * offset * offset * _coefficients[k + 3]);
        }

        /// <summary>
        /// Integrate up to point t.
        /// </summary>
        /// <param name="t">Right bound of the integration interval [a,t].</param>
        /// <returns>Interpolated definite integral over the interval [a,t].</returns>
        /// <seealso cref="IInterpolation.SupportsIntegration"/>
        public double Integrate(double t)
        {
            int closestLeftIndex = IndexOfClosestPointLeftOf(t);

            // Integration
            double result = 0;
            for (int i = 0, j = 0; i < closestLeftIndex; i++, j += 4)
            {
                double w = _points[i + 1] - _points[i];
                result += w * (_coefficients[j]
                               + ((w * _coefficients[j + 1] * 0.5)
                                  + (w * ((_coefficients[j + 2] / 3)
                                          + (w * _coefficients[j + 3] * 0.25)))));
            }

            double offset = t - _points[closestLeftIndex];
            int k = closestLeftIndex << 2;

            return result + (offset * (_coefficients[k]
                                       + (offset * _coefficients[k + 1] * 0.5)
                                       + (offset * _coefficients[k + 2] / 3)
                                       + (offset * _coefficients[k + 3] * 0.25)));
        }

        /// <summary>
        /// Find the index of the greatest sample point smaller than t.
        /// </summary>
        /// <param name="t">The value to look for.</param>
        /// <returns>The sample point index.</returns>
        private int IndexOfClosestPointLeftOf(double t)
        {
            // Binary search in the [ t[0], ..., t[n-2] ] (t[n-1] is not included)
            int low = 0;
            int high = _sampleCount - 1;
            while (low != high - 1)
            {
                int middle = (low + high) / 2;
                if (_points[middle] > t)
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            return low;
        }
    }
}