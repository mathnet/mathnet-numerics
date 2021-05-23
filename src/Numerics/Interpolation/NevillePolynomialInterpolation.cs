// <copyright file="NevillePolynomialInterpolation.cs" company="Math.NET">
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
    /// Lagrange Polynomial Interpolation using Neville's Algorithm.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This algorithm supports differentiation, but doesn't support integration.
    /// </para>
    /// <para>
    /// When working with equidistant or Chebyshev sample points it is
    /// recommended to use the barycentric algorithms specialized for
    /// these cases instead of this arbitrary Neville algorithm.
    /// </para>
    /// </remarks>
    public class NevillePolynomialInterpolation : IInterpolation
    {
        readonly double[] _x;
        readonly double[] _y;

        /// <param name="x">Sample Points t, sorted ascendingly.</param>
        /// <param name="y">Sample Values x(t), sorted ascendingly by x.</param>
        public NevillePolynomialInterpolation(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 1)
            {
                throw new ArgumentException("The given array is too small. It must be at least 1 long.", nameof(x));
            }

            for (var i = 1; i < x.Length; ++i)
            {
                if (x[i] == x[i - 1])
                {
                    throw new ArgumentException("All sample points should be unique.", nameof(x));
                }
            }

            _x = x;
            _y = y;
        }

        /// <summary>
        /// Create a Neville polynomial interpolation from a set of (x,y) value pairs, sorted ascendingly by x.
        /// </summary>
        public static NevillePolynomialInterpolation InterpolateSorted(double[] x, double[] y)
        {
            return new NevillePolynomialInterpolation(x, y);
        }

        /// <summary>
        /// Create a Neville polynomial interpolation from an unsorted set of (x,y) value pairs.
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static NevillePolynomialInterpolation InterpolateInplace(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            Sorting.Sort(x, y);
            return InterpolateSorted(x, y);
        }

        /// <summary>
        /// Create a Neville polynomial interpolation from an unsorted set of (x,y) value pairs.
        /// </summary>
        public static NevillePolynomialInterpolation Interpolate(IEnumerable<double> x, IEnumerable<double> y)
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
        bool IInterpolation.SupportsIntegration => false;

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            var x = new double[_y.Length];
            _y.CopyTo(x, 0);

            for (int level = 1; level < x.Length; level++)
            {
                for (int i = 0; i < x.Length - level; i++)
                {
                    double hp = t - _x[i + level];
                    double ho = _x[i] - t;
                    double den = _x[i] - _x[i + level];
                    x[i] = ((hp*x[i]) + (ho*x[i + 1]))/den;
                }
            }

            return x[0];
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        public double Differentiate(double t)
        {
            var x = new double[_y.Length];
            var dx = new double[_y.Length];
            _y.CopyTo(x, 0);

            for (int level = 1; level < x.Length; level++)
            {
                for (int i = 0; i < x.Length - level; i++)
                {
                    double hp = t - _x[i + level];
                    double ho = _x[i] - t;
                    double den = _x[i] - _x[i + level];
                    dx[i] = ((hp*dx[i]) + x[i] + (ho*dx[i + 1]) - x[i + 1])/den;
                    x[i] = ((hp*x[i]) + (ho*x[i + 1]))/den;
                }
            }

            return dx[0];
        }

        /// <summary>
        /// Differentiate twice at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        public double Differentiate2(double t)
        {
            var x = new double[_y.Length];
            var dx = new double[_y.Length];
            var ddx = new double[_y.Length];
            _y.CopyTo(x, 0);

            for (int level = 1; level < x.Length; level++)
            {
                for (int i = 0; i < x.Length - level; i++)
                {
                    double hp = t - _x[i + level];
                    double ho = _x[i] - t;
                    double den = _x[i] - _x[i + level];
                    ddx[i] = ((hp*ddx[i]) + (ho*ddx[i + 1]) + (2*dx[i]) - (2*dx[i + 1]))/den;
                    dx[i] = ((hp*dx[i]) + x[i] + (ho*dx[i + 1]) - x[i + 1])/den;
                    x[i] = ((hp*x[i]) + (ho*x[i + 1]))/den;
                }
            }

            return ddx[0];
        }

        /// <summary>
        /// Indefinite integral at point t. NOT SUPPORTED.
        /// </summary>
        /// <param name="t">Point t to integrate at.</param>
        double IInterpolation.Integrate(double t) => throw new NotSupportedException();

        /// <summary>
        /// Definite integral between points a and b. NOT SUPPORTED.
        /// </summary>
        /// <param name="a">Left bound of the integration interval [a,b].</param>
        /// <param name="b">Right bound of the integration interval [a,b].</param>
        double IInterpolation.Integrate(double a, double b) => throw new NotSupportedException();
    }
}
