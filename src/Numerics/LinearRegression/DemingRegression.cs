// <copyright file="DemingRegression.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2023 Math.NET
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

namespace MathNet.Numerics.LinearRegression
{
    public static class DemingRegression
    {
        /// <summary>
        /// Deming/Orthogonal regression, least-squares fitting the points in
        /// the 2D dataset (x,y) to a line
        /// <code>
        ///    a*x + b*y + c = 0
        /// </code>
        /// For <paramref name="delta"/> equal 1 (the default value), this is
        /// performing orthogonal regression, minimizing the sum of squared
        /// perpendicular distances from the data points to the regression line.
        /// <para>
        /// Orthogonal regression is a special case of Deming regression,
        /// and is assuming equal error variances on the x and y data,
        /// and applied by the argument <paramref name="delta"/> default value of 1.0.
        /// </para>
        /// <para>
        /// The parameters (a,b,c) are scaled such that a and b
        /// in absolute values are always less than one.
        /// </para>
        /// </summary>
        /// <param name="x">X data</param>
        /// <param name="y">Y data</param>
        /// <param name="delta">Ratio of variances of x and y data, var(y)/var(x). Default value is 1</param>
        /// <returns> returning its best fitting parameters as (a, b, c) tuple.</returns>
        /// <remarks>
        /// Solution algorithm as described from:
        ///     "Deming regression MethComp package", May 2007,
        ///     Anders Christian Jensen, Steno Diabetes Center, Gentofte, Denmark
        ///     https://en.wikipedia.org/wiki/Deming_regression
        ///     https://docplayer.gr/85660531-Deming-regression-methcomp-package-may.html
        /// Computations are offset to be centered around (mean(x), mean(y)),
        /// for improved numerical stability.
        /// </remarks>
        public static (double A, double B, double C) Fit(double[] x, double[] y, double delta = 1.0)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException($"All sample vectors must have the same length. However, vectors with disagreeing length {x.Length} and {y.Length} have been provided. A sample with index i is given by the value at index i of each provided vector.");
            }

            if (x.Length <= 1)
            {
                throw new ArgumentException($"A regression of the requested order requires at least 2 samples. Only {x.Length} samples have been provided.");
            }

            // First Pass: Mean and abs-max values
            double mx = 0.0;
            double my = 0.0;
            double maxax = 0;
            double maxay = 0;
            for (int i = 0; i < x.Length; i++)
            {
                mx += x[i];
                my += y[i];

                double ax = Math.Abs(x[i]);
                double ay = Math.Abs(y[i]);
                if (ax > maxax)
                    maxax = ax;
                if (ay > maxay)
                    maxay = ay;
            }

            // Mean values
            mx /= x.Length;
            my /= y.Length;

            // Second Pass: Second-degree central sample moments
            double sxx = 0.0, sxy = 0.0, syy = 0;
            for (int i = 0; i < x.Length; i++)
            {
                double xdiff = x[i] - mx;
                double ydiff = y[i] - my;
                sxx += xdiff * xdiff;
                sxy += xdiff * ydiff;
                syy += ydiff * ydiff;
            }

            sxx /= x.Length;
            sxy /= x.Length;
            syy /= x.Length;

            // Need to either solve for a for y = -a*x:
            //    −sxy * a^2 + (syy-delta*sxx) * a + delta*sxy = 0
            // or solve for b for x = -b*y:
            //    −sxy * b^2 + (sxx-syy/delta) * b + sxy/delta = 0
            // Which to choose: Depends on whether the slope is larger than one

            // If sxy is zero, either:
            //   - data is all symmetric around the mid-point
            //   - x or y is constant
            // Then either a or b, or both, are zero
            if (Math.Abs(sxy) <= (delta * sxx + syy) * 1e-10)
            {
                // When sxx has a value, it is a vertical line
                if (delta * sxx > syy && sxx > maxax * 1e-10)
                    return (0, 1, -my);
                // when syy has a value, it is a horizontal line
                else if (syy > maxay * 1e-10)
                    return (1, 0, -mx);
                // No line could be calculated
                else
                    return (double.NaN, double.NaN, double.NaN);
            }

            // Pick a or b solution such that both a and b are always <= 1
            if (syy <= delta*sxx)
            {
                double sxydiff = syy - delta*sxx;
                double a = -(sxydiff + Math.Sqrt(sxydiff * sxydiff + 4 * delta * sxy * sxy)) / (2 * sxy);
                double b = 1;
                double c = -my - a * mx;
                return (a, b, c);
            }
            else
            {
                double syxdiff = sxx - syy/delta;
                double a = 1;
                double b = -(syxdiff + Math.Sqrt(syxdiff * syxdiff + 4 / delta * sxy * sxy)) / (2 * sxy);
                double c = -b * my - mx;
                return (a, b, c);
            }

        }

        /// <summary>
        /// Deming/Orthogonal regression, least-Squares fitting the points in
        /// the 2D dataset (x,y) to a line
        /// <code>
        ///    a*x + b*y + c = 0
        /// </code>
        /// For <paramref name="delta"/> equal 1 (the default value), this is
        /// performing orthogonal regression, minimizing the sum of squared
        /// perpendicular distances from the data points to the regression line.
        /// <para>
        /// See <see cref="Fit(double[],double[], double)"/> for more details.
        /// </para>
        /// </summary>
        /// <param name="samples">(x,y) data as tuples</param>
        /// <param name="delta">Ratio of variances of x and y data, var(y)/var(x). Default value is 1</param>
        /// <returns> returning its best fitting parameters as (a, b, c) tuple.</returns>
        public static (double A, double B, double C) Fit(IEnumerable<Tuple<double, double>> samples, double delta = 1.0)
        {
            var (u, v) = samples.UnpackSinglePass();
            return Fit(u, v);
        }

        /// <summary>
        /// Deming/Orthogonal regression, least-Squares fitting the points in
        /// the 2D dataset (x,y) to a line
        /// <code>
        ///    a*x + b*y + c = 0
        /// </code>
        /// For <paramref name="delta"/> equal 1 (the default value), this is
        /// performing orthogonal regression, minimizing the sum of squared
        /// perpendicular distances from the data points to the regression line.
        /// <para>
        /// See <see cref="Fit(double[],double[], double)"/> for more details.
        /// </para>
        /// </summary>
        /// <param name="samples">(x,y) data as tuples</param>
        /// <param name="delta">Ratio of variances of x and y data, var(y)/var(x). Default value is 1</param>
        /// <returns> returning its best fitting parameters as (a, b, c) tuple.</returns>
        public static (double A, double B, double C) Fit(IEnumerable<(double, double)> samples, double delta = 1.0)
        {
            var (u, v) = samples.UnpackSinglePass();
            return Fit(u, v);
        }
    }
}
