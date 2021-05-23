// <copyright file="TransformedInterpolation.cs" company="Math.NET">
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
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.Interpolation
{
    /// <summary>
    /// Wraps an interpolation with a transformation of the interpolated values.
    /// </summary>
    /// <remarks>Neither differentiation nor integration is supported.</remarks>
    public class TransformedInterpolation : IInterpolation
    {
        readonly IInterpolation _interpolation;
        readonly Func<double, double> _transform;

        public TransformedInterpolation(IInterpolation interpolation, Func<double, double> transform)
        {
            _interpolation = interpolation;
            _transform = transform;
        }

        /// <summary>
        /// Create a linear spline interpolation from a set of (x,y) value pairs, sorted ascendingly by x.
        /// </summary>
        public static TransformedInterpolation InterpolateSorted(
            Func<double, double> transform,
            Func<double, double> transformInverse,
            double[] x,
            double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            var yhat = new double[y.Length];
            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    yhat[i] = transformInverse(y[i]);
                }
            });

            return new TransformedInterpolation(LinearSpline.InterpolateSorted(x, yhat), transform);
        }

        /// <summary>
        /// Create a linear spline interpolation from an unsorted set of (x,y) value pairs.
        /// WARNING: Works in-place and can thus causes the data array to be reordered and modified.
        /// </summary>
        public static TransformedInterpolation InterpolateInplace(
            Func<double, double> transform,
            Func<double, double> transformInverse,
            double[] x,
            double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            Sorting.Sort(x, y);
            CommonParallel.For(0, y.Length, 4096, (a, b) =>
            {
                for (int i = a; i < b; i++)
                {
                    y[i] = transformInverse(y[i]);
                }
            });

            return new TransformedInterpolation(LinearSpline.InterpolateSorted(x, y), transform);
        }

        /// <summary>
        /// Create a linear spline interpolation from an unsorted set of (x,y) value pairs.
        /// </summary>
        public static TransformedInterpolation Interpolate(
            Func<double, double> transform,
            Func<double, double> transformInverse,
            IEnumerable<double> x,
            IEnumerable<double> y)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolateInplace(transform, transformInverse, x.ToArray(), y.ToArray());
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        bool IInterpolation.SupportsDifferentiation => false;

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        bool IInterpolation.SupportsIntegration => false;

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t) => _transform(_interpolation.Interpolate(t));

        /// <summary>
        /// Differentiate at point t. NOT SUPPORTED.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        double IInterpolation.Differentiate(double t) => throw new NotSupportedException();

        /// <summary>
        /// Differentiate twice at point t. NOT SUPPORTED.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        double IInterpolation.Differentiate2(double t) => throw new NotSupportedException();

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
