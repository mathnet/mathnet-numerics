// <copyright file="BulirschStoerRationalInterpolation.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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

namespace MathNet.Numerics.Interpolation
{
    /// <summary>
    /// Rational Interpolation (with poles) using Roland Bulirsch and Josef Stoer's Algorithm.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This algorithm supports neither differentiation nor integration.
    /// </para>
    /// </remarks>
    public class BulirschStoerRationalInterpolation : IInterpolation
    {
        /// <summary>
        /// Sample Points t.
        /// </summary>
        IList<double> _points;

        /// <summary>
        /// Spline Values x(t).
        /// </summary>
        IList<double> _values;

        /// <summary>
        /// Initializes a new instance of the BulirschStoerRationalInterpolation class.
        /// </summary>
        public BulirschStoerRationalInterpolation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BulirschStoerRationalInterpolation class.
        /// </summary>
        /// <param name="samplePoints">Sample Points t</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        public BulirschStoerRationalInterpolation(IList<double> samplePoints, IList<double> sampleValues)
        {
            Initialize(samplePoints, sampleValues);
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        bool IInterpolation.SupportsDifferentiation
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        bool IInterpolation.SupportsIntegration
        {
            get { return false; }
        }

        /// <summary>
        /// Initialize the interpolation method with the given sample pairs.
        /// </summary>
        /// <param name="samplePoints">Sample Points t</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        public void Initialize(IList<double> samplePoints, IList<double> sampleValues)
        {
            if (null == samplePoints)
            {
                throw new ArgumentNullException("samplePoints");
            }

            if (null == sampleValues)
            {
                throw new ArgumentNullException("sampleValues");
            }

            if (samplePoints.Count < 1)
            {
                throw new ArgumentOutOfRangeException("samplePoints");
            }

            if (samplePoints.Count != sampleValues.Count)
            {
                throw new ArgumentException(Properties.Resources.ArgumentVectorsSameLength);
            }

            _points = samplePoints;
            _values = sampleValues;
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            const double tiny = 1.0e-25;
            int n = _points.Count;

            var c = new double[n];
            var d = new double[n];

            int nearestIndex = 0;
            double nearestDistance = Math.Abs(t - _points[0]);

            for (int i = 0; i < n; i++)
            {
                double distance = Math.Abs(t - _points[i]);
                if (distance.AlmostEqual(0.0))
                {
                    return _values[i];
                }

                if (distance < nearestDistance)
                {
                    nearestIndex = i;
                    nearestDistance = distance;
                }

                c[i] = _values[i];
                d[i] = _values[i] + tiny;
            }

            double x = _values[nearestIndex];

            for (int level = 1; level < n; level++)
            {
                for (int i = 0; i < n - level; i++)
                {
                    double hp = _points[i + level] - t;
                    double ho = (_points[i] - t)*d[i]/hp;

                    double den = ho - c[i + 1];
                    if (den.AlmostEqual(0.0))
                    {
                        return double.NaN; // zero-div, singularity
                    }

                    den = (c[i + 1] - d[i])/den;
                    d[i] = c[i + 1]*den;
                    c[i] = ho*den;
                }

                x += (2*nearestIndex) < (n - level)
                    ? c[nearestIndex]
                    : d[--nearestIndex];
            }

            return x;
        }

        /// <summary>
        /// Differentiate at point t. NOT SUPPORTED.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        double IInterpolation.Differentiate(double t)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Differentiate twice at point t. NOT SUPPORTED.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        double IInterpolation.Differentiate2(double t)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Indefinite integral at point t. NOT SUPPORTED.
        /// </summary>
        /// <param name="t">Point t to integrate at.</param>
        double IInterpolation.Integrate(double t)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Definite integral between points a and b. NOT SUPPORTED.
        /// </summary>
        /// <param name="a">Left bound of the integration interval [a,b].</param>
        /// <param name="b">Right bound of the integration interval [a,b].</param>
        double IInterpolation.Integrate(double a, double b)
        {
            throw new NotSupportedException();
        }
    }
}
