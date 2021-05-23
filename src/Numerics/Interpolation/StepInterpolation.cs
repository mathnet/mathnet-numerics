// <copyright file="StepInterpolation.cs" company="Math.NET">
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
    /// A step function where the start of each segment is included, and the last segment is open-ended.
    /// Segment i is [x_i, x_i+1) for i &lt; N, or [x_i, infinity] for i = N.
    /// The domain of the function is all real numbers, such that y = 0 where x &lt;.
    /// </summary>
    /// <remarks>Supports both differentiation and integration.</remarks>
    public class StepInterpolation : IInterpolation
    {
        readonly double[] _x;
        readonly double[] _y;
        readonly Lazy<double[]> _indefiniteIntegral;

        /// <param name="x">Sample points (N), sorted ascending</param>
        /// <param name="sy">Samples values (N) of each segment starting at the corresponding sample point.</param>
        public StepInterpolation(double[] x, double[] sy)
        {
            if (x.Length != sy.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 1)
            {
                throw new ArgumentException("The given array is too small. It must be at least 1 long.", nameof(x));
            }

            _x = x;
            _y = sy;
            _indefiniteIntegral = new Lazy<double[]>(ComputeIndefiniteIntegral);
        }

        /// <summary>
        /// Create a linear spline interpolation from a set of (x,y) value pairs, sorted ascendingly by x.
        /// </summary>
        public static StepInterpolation InterpolateSorted(double[] x, double[] y)
        {
            return new StepInterpolation(x, y);
        }

        /// <summary>
        /// Create a linear spline interpolation from an unsorted set of (x,y) value pairs.
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static StepInterpolation InterpolateInplace(double[] x, double[] y)
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
        public static StepInterpolation Interpolate(IEnumerable<double> x, IEnumerable<double> y)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolateInplace(x.ToArray(), y.ToArray());
        }

        bool IInterpolation.SupportsDifferentiation => true;

        bool IInterpolation.SupportsIntegration => true;

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            if (t < _x[0])
            {
                return 0.0;
            }

            int k = LeftBracketIndex(t);
            return _y[k];
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        public double Differentiate(double t)
        {
            int index = Array.BinarySearch(_x, t);
            if (index >= 0)
            {
                return double.NaN;
            }

            return 0d;
        }

        /// <summary>
        /// Differentiate twice at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        public double Differentiate2(double t) => Differentiate(t);

        /// <summary>
        /// Indefinite integral at point t.
        /// </summary>
        /// <param name="t">Point t to integrate at.</param>
        public double Integrate(double t)
        {
            if (t <= _x[0])
            {
                return 0.0;
            }

            int k = LeftBracketIndex(t);
            var x = t - _x[k];
            return _indefiniteIntegral.Value[k] + x*_y[k];
        }

        /// <summary>
        /// Definite integral between points a and b.
        /// </summary>
        /// <param name="a">Left bound of the integration interval [a,b].</param>
        /// <param name="b">Right bound of the integration interval [a,b].</param>
        public double Integrate(double a, double b) => Integrate(b) - Integrate(a);

        double[] ComputeIndefiniteIntegral()
        {
            var integral = new double[_x.Length];
            for (int i = 0; i < integral.Length - 1; i++)
            {
                integral[i + 1] = integral[i] + (_x[i + 1] - _x[i])*_y[i];
            }

            return integral;
        }

        /// <summary>
        /// Find the index of the greatest sample point smaller than t.
        /// </summary>
        int LeftBracketIndex(double t)
        {
            int index = Array.BinarySearch(_x, t);
            return index >= 0 ? index : ~index - 1;
        }
    }
}
