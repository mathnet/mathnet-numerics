// <copyright file="BarycentricInterpolation.cs" company="Math.NET">
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
    /// Barycentric Interpolation Algorithm.
    /// </summary>
    /// <remarks>
    /// This algorithm neither supports differentiation nor integration.
    /// </remarks>
    public class BarycentricInterpolation : IInterpolation
    {
        /// <summary>
        /// Sample Points t.
        /// </summary>
        private IList<double> _points;

        /// <summary>
        /// Sample Values x(t).
        /// </summary>
        private IList<double> _values;

        /// <summary>
        /// Barycentric Weights w(t).
        /// </summary>
        private IList<double> _weights;

        /// <summary>
        /// Initializes a new instance of the BarycentricInterpolation class.
        /// </summary>
        public BarycentricInterpolation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BarycentricInterpolation class.
        /// </summary>
        /// <param name="samplePoints">Sample Points t (no sorting assumed)</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <param name="barycentricWeights">Barycentric weights w(t)</param>
        public BarycentricInterpolation(
            IList<double> samplePoints,
            IList<double> sampleValues,
            IList<double> barycentricWeights)
        {
            Initialize(samplePoints, sampleValues, barycentricWeights);
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
        /// Initialize the interpolation method with the given sample set (no sorting assumed).
        /// </summary>
        /// <param name="samplePoints">Sample Points t</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <param name="barycentricWeights">Barycentric weights w(t)</param>
        public void Initialize(
            IList<double> samplePoints,
            IList<double> sampleValues,
            IList<double> barycentricWeights)
        {
            if (null == samplePoints)
            {
                throw new ArgumentNullException("samplePoints");
            }

            if (null == sampleValues)
            {
                throw new ArgumentNullException("sampleValues");
            }

            if (null == barycentricWeights)
            {
                throw new ArgumentNullException("barycentricWeights");
            }

            if (samplePoints.Count < 1)
            {
                throw new ArgumentOutOfRangeException("samplePoints");
            }

            if (samplePoints.Count != sampleValues.Count)
            {
                throw new ArgumentException(Properties.Resources.ArgumentVectorsSameLength);
            }

            if (samplePoints.Count != barycentricWeights.Count)
            {
                throw new ArgumentException(Properties.Resources.ArgumentVectorsSameLength);
            }

            _points = samplePoints;
            _values = sampleValues;
            _weights = barycentricWeights;
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            // trivial case: only one sample?
            if (_points.Count == 1)
            {
                return _values[0];
            }

            // evaluate closest point and offset from that point (no sorting assumed)
            int closestPoint = 0;
            double offset = t - _points[0];
            for (int i = 1; i < _points.Count; i++)
            {
                if (Math.Abs(t - _points[i]) < Math.Abs(offset))
                {
                    offset = t - _points[i];
                    closestPoint = i;
                }
            }

            // trivial case: on a known sample point?
            if (offset == 0.0)
            {
                // NOTE (cdrnet, 200908) not offset.AlmostZero() by design
                return _values[closestPoint];
            }

            if (Math.Abs(offset) > 1e-150)
            {
                // no need to guard against overflow, so use fast formula
                closestPoint = -1;
                offset = 1.0;
            }

            double s1 = 0.0;
            double s2 = 0.0;
            for (int i = 0; i < _points.Count; i++)
            {
                if (i != closestPoint)
                {
                    double v = offset * _weights[i] / (t - _points[i]);
                    s1 = s1 + (v * _values[i]);
                    s2 = s2 + v;
                }
                else
                {
                    double v = _weights[i];
                    s1 = s1 + (v * _values[i]);
                    s2 = s2 + v;
                }
            }

            return s1 / s2;
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