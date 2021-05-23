// <copyright file="LinearSpline.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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
using System.Linq;

namespace MathNet.Numerics.Interpolation
{
    /// <summary>
    /// Piece-wise Linear Interpolation.
    /// </summary>
    /// <remarks>Supports both differentiation and integration.</remarks>
    public class LinearSpline : IInterpolation
    {
        readonly double[] _x;
        readonly double[] _c0;
        readonly double[] _c1;
        readonly Lazy<double[]> _indefiniteIntegral;

        /// <param name="x">Sample points (N+1), sorted ascending</param>
        /// <param name="c0">Sample values (N or N+1) at the corresponding points; intercept, zero order coefficients</param>
        /// <param name="c1">Slopes (N) at the sample points (first order coefficients): N</param>
        public LinearSpline(double[] x, double[] c0, double[] c1)
        {
            if ((x.Length != c0.Length + 1 && x.Length != c0.Length) || x.Length != c1.Length + 1)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("The given array is too small. It must be at least 2 long.", nameof(x));
            }

            _x = x;
            _c0 = c0;
            _c1 = c1;
            _indefiniteIntegral = new Lazy<double[]>(ComputeIndefiniteIntegral);
        }

        /// <summary>
        /// Create a linear spline interpolation from a set of (x,y) value pairs, sorted ascendingly by x.
        /// </summary>
        public static LinearSpline InterpolateSorted(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("The given array is too small. It must be at least 2 long.", nameof(x));
            }

            var c1 = new double[x.Length - 1];
            for (int i = 0; i < c1.Length; i++)
            {
                c1[i] = (y[i + 1] - y[i])/(x[i + 1] - x[i]);
            }

            return new LinearSpline(x, y, c1);
        }

        /// <summary>
        /// Create a linear spline interpolation from an unsorted set of (x,y) value pairs.
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static LinearSpline InterpolateInplace(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            Sorting.Sort(x, y);
            return InterpolateSorted(x, y);
        }

        /// <summary>
        /// Create a linear spline interpolation from an unsorted set of (x,y) value pairs.
        /// </summary>
        public static LinearSpline Interpolate(IEnumerable<double> x, IEnumerable<double> y)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolateInplace(x.ToArray(), y.ToArray());
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        bool IInterpolation.SupportsDifferentiation => true;

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        bool IInterpolation.SupportsIntegration => true;

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            int k = LeftSegmentIndex(t);
            return _c0[k] + (t - _x[k])*_c1[k];
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        public double Differentiate(double t)
        {
            int k = LeftSegmentIndex(t);
            return _c1[k];
        }

        /// <summary>
        /// Differentiate twice at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        public double Differentiate2(double t) => 0d;

        /// <summary>
        /// Indefinite integral at point t.
        /// </summary>
        /// <param name="t">Point t to integrate at.</param>
        public double Integrate(double t)
        {
            int k = LeftSegmentIndex(t);
            var x = t - _x[k];
            return _indefiniteIntegral.Value[k] + x*(_c0[k] + x*_c1[k]/2);
        }

        /// <summary>
        /// Definite integral between points a and b.
        /// </summary>
        /// <param name="a">Left bound of the integration interval [a,b].</param>
        /// <param name="b">Right bound of the integration interval [a,b].</param>
        public double Integrate(double a, double b) => Integrate(b) - Integrate(a);

        double[] ComputeIndefiniteIntegral()
        {
            var integral = new double[_c1.Length];
            for (int i = 0; i < integral.Length - 1; i++)
            {
                double w = _x[i + 1] - _x[i];
                integral[i + 1] = integral[i] + w*(_c0[i] + w*_c1[i]/2);
            }

            return integral;
        }

        /// <summary>
        /// Find the index of the greatest sample point smaller than t,
        /// or the left index of the closest segment for extrapolation.
        /// </summary>
        int LeftSegmentIndex(double t)
        {
            int index = Array.BinarySearch(_x, t);
            if (index < 0)
            {
                index = ~index - 1;
            }

            return Math.Min(Math.Max(index, 0), _x.Length - 2);
        }
    }
}
