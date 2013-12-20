// <copyright file="LinearSpline.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Interpolation
{
    /// <summary>
    /// Linear Spline Interpolation.
    /// </summary>
    /// <remarks>Supports both differentiation and integration.</remarks>
    public class LinearSpline : IInterpolation
    {
        readonly double[] _x;
        readonly double[] _c0;
        readonly double[] _c1;

        /// <param name="x">Sample points (N+1), sorted ascending</param>
        /// <param name="c0">Sample values (N or N+1) at the corresponding points; intercept, zero order coefficients</param>
        /// <param name="c1">Slopes (N) at the sample points (first order coefficients): N</param>
        public LinearSpline(double[] x, double[] c0, double[] c1)
        {
            _x = x;
            _c0 = c0;
            _c1 = c1;
        }

        /// <summary>
        /// Create a linear spline interpolation from a set of (x,y) value pairs.
        /// </summary>
        /// <remarks>
        /// The value pairs do not have to be sorted, but if they are not sorted ascendingly
        /// and the passed x and y arguments are arrays, they will be sorted inplace and thus modified.
        /// </remarks>
        public static LinearSpline Interpolate(IEnumerable<double> x, IEnumerable<double> y)
        {
            var xx = (x as double[]) ?? x.ToArray();
            var yy = (y as double[]) ?? y.ToArray();

            if (xx.Length != yy.Length)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            Sorting.Sort(xx, yy);

            var c1 = new double[xx.Length - 1];
            for (int i = 0; i < c1.Length; i++)
            {
                c1[i] = (yy[i + 1] - yy[i])/(xx[i + 1] - xx[i]);
            }

            return new LinearSpline(xx, yy, c1);
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        public bool SupportsDifferentiation
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        public bool SupportsIntegration
        {
            get { return true; }
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            int k = LeftBracketIndex(t);
            return _c0[k] + (t - _x[k])*_c1[k];
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        public double Differentiate(double t)
        {
            int k = LeftBracketIndex(t);
            return _c1[k];
        }

        /// <summary>
        /// Differentiate twice at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        public double Differentiate2(double t)
        {
            return 0d;
        }

        /// <summary>
        /// Integrate up to point t.
        /// </summary>
        /// <param name="t">Right bound of the integration interval [a,t].</param>
        /// <returns>Interpolated definite integral over the interval [a,t].</returns>
        public double Integrate(double t)
        {
            int k = LeftBracketIndex(t);
            var x = (t - _x[k]);
            return x*(_c0[k] + x*0.5*_c1[k]);
        }

        /// <summary>
        /// Interpolate, differentiate and 2nd differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        public Tuple<double, double, double> DifferentiateAll(double t)
        {
            int k = LeftBracketIndex(t);
            var x = (t - _x[k]);
            return new Tuple<double, double, double>(_c0[k] + x*_c1[k], _c1[k], 0d);
        }

        /// <summary>
        /// Find the index of the greatest sample point smaller than t.
        /// </summary>
        int LeftBracketIndex(double t)
        {
            // Binary search in the [ t[0], ..., t[n-2] ] (t[n-1] is not included)
            int low = 0;
            int high = _x.Length - 1;
            while (low != high - 1)
            {
                int middle = (low + high)/2;
                if (_x[middle] > t)
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
