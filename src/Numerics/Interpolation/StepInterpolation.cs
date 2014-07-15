// <copyright file="StepInterpolation.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.Interpolation
{
    /// <summary>
    /// A step function where the start of each segment is included, and last is excluded.  Segment i is [x_i, x_i+1).
    /// The domain of the function is all real numbers, such that y = 0 where x &lt; x_0 or x gt; x_n
    /// </summary>
    public class StepInterpolation : IInterpolation
    {
        readonly double[] _x;
        readonly double[] _y;
        readonly Lazy<double[]> _indefiniteIntegral;

        /// <param name="x">Sample points (N+1) sorted in ascending order</param>
        /// <param name="y">Functional value (N) of each segment.</param>
        public StepInterpolation(IEnumerable<double> x, IEnumerable<double> y)
        {
            var xx = (x as double[]) ?? x.ToArray();
            var yy = (y as double[]) ?? y.ToArray();

            if (xx.Length != yy.Length + 1)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            _x = xx;
            _y = yy;
            _indefiniteIntegral = new Lazy<double[]>(ComputeIndefiniteIntegral);
        }

        double[] ComputeIndefiniteIntegral()
        {
            var integral = new double[_x.Length];
            for (int i = 0; i < integral.Length - 1; i++)
            {
                integral[i + 1] = integral[i] + (_x[i + 1] - _x[i])*_y[i];
            }
            return integral;
        }

        public bool SupportsDifferentiation
        {
            get { return true; }
        }

        public bool SupportsIntegration
        {
            get { return true; }
        }

        public double Differentiate(double t)
        {
            int index = Array.BinarySearch(_x, t);
            if (index >= 0)
                return double.NaN;
            return 0d;
        }

        public double Differentiate2(double t)
        {
            return Differentiate(t);
        }

        /// <summary>
        /// Indefinite integral at point t.
        /// </summary>
        /// <param name="t">Point t to integrate at.</param>
        public double Integrate(double t)
        {
            if (t <= _x[0])
                return 0.0;
            int last = _x.Length - 1;
            if (t >= _x[last])
                return _indefiniteIntegral.Value[last];

            int k = LeftBracketIndex(t);
            var x = (t - _x[k]);
            return _indefiniteIntegral.Value[k] + x*_y[k];
        }

        /// <summary>
        /// Definite integral between points a and b.
        /// </summary>
        /// <param name="a">Left bound of the integration interval [a,b].</param>
        /// <param name="b">Right bound of the integration interval [a,b].</param>
        public double Integrate(double a, double b)
        {
            return Integrate(b) - Integrate(a);
        }

        public double Interpolate(double t)
        {
            if (t < _x[0] || t >= _x[_x.Length - 1])
                return 0.0;

            int k = LeftBracketIndex(t);
            return _y[k];
        }

        /// <summary>
        /// Find the index of the greatest sample point smaller than t.
        /// </summary>
        int LeftBracketIndex(double t)
        {
            int index = Array.BinarySearch(_x, t);
            if (index >= 0)
                return index;
            index = ~index;
            return index - 1;
        }
    }
}
