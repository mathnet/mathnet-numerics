// <copyright file="CubicSpline.cs" company="Math.NET">
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
    /// Cubic Spline Interpolation.
    /// </summary>
    /// <remarks>Supports both differentiation and integration.</remarks>
    public class CubicSpline : IInterpolation
    {
        readonly double[] _x;
        readonly double[] _c0;
        readonly double[] _c1;
        readonly double[] _c2;
        readonly double[] _c3;
        readonly Lazy<double[]> _indefiniteIntegral;

        /// <param name="x">sample points (N+1), sorted ascending</param>
        /// <param name="c0">Zero order spline coefficients (N)</param>
        /// <param name="c1">First order spline coefficients (N)</param>
        /// <param name="c2">second order spline coefficients (N)</param>
        /// <param name="c3">third order spline coefficients (N)</param>
        public CubicSpline(double[] x, double[] c0, double[] c1, double[] c2, double[] c3)
        {
            if (x.Length != c0.Length + 1 || x.Length != c1.Length + 1 || x.Length != c2.Length + 1 || x.Length != c3.Length + 1)
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
            _c2 = c2;
            _c3 = c3;
            _indefiniteIntegral = new Lazy<double[]>(ComputeIndefiniteIntegral);
        }

        /// <summary>
        /// Create a Hermite cubic spline interpolation from a set of (x,y) value pairs and their slope (first derivative), sorted ascendingly by x.
        /// </summary>
        public static CubicSpline InterpolateHermiteSorted(double[] x, double[] y, double[] firstDerivatives)
        {
            if (x.Length != y.Length || x.Length != firstDerivatives.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("The given array is too small. It must be at least 2 long.", nameof(x));
            }

            var c0 = new double[x.Length - 1];
            var c1 = new double[x.Length - 1];
            var c2 = new double[x.Length - 1];
            var c3 = new double[x.Length - 1];
            for (int i = 0; i < c1.Length; i++)
            {
                double w = x[i + 1] - x[i];
                double w2 = w*w;
                c0[i] = y[i];
                c1[i] = firstDerivatives[i];
                c2[i] = (3*(y[i + 1] - y[i])/w - 2*firstDerivatives[i] - firstDerivatives[i + 1])/w;
                c3[i] = (2*(y[i] - y[i + 1])/w + firstDerivatives[i] + firstDerivatives[i + 1])/w2;
            }

            return new CubicSpline(x, c0, c1, c2, c3);
        }

        /// <summary>
        /// Create a Hermite cubic spline interpolation from an unsorted set of (x,y) value pairs and their slope (first derivative).
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static CubicSpline InterpolateHermiteInplace(double[] x, double[] y, double[] firstDerivatives)
        {
            if (x.Length != y.Length || x.Length != firstDerivatives.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("The given array is too small. It must be at least 2 long.", nameof(x));
            }

            Sorting.Sort(x, y, firstDerivatives);
            return InterpolateHermiteSorted(x, y, firstDerivatives);
        }

        /// <summary>
        /// Create a Hermite cubic spline interpolation from an unsorted set of (x,y) value pairs and their slope (first derivative).
        /// </summary>
        public static CubicSpline InterpolateHermite(IEnumerable<double> x, IEnumerable<double> y, IEnumerable<double> firstDerivatives)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolateHermiteInplace(x.ToArray(), y.ToArray(), firstDerivatives.ToArray());
        }

        /// <summary>
        /// Create an Akima cubic spline interpolation from a set of (x,y) value pairs, sorted ascendingly by x.
        /// Akima splines are robust to outliers.
        /// </summary>
        public static CubicSpline InterpolateAkimaSorted(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 5)
            {
                throw new ArgumentException("The given array is too small. It must be at least 5 long.", nameof(x));
            }

            /* Prepare divided differences (diff) and weights (w) */

            var diff = new double[x.Length - 1];
            var weights = new double[x.Length - 1];

            for (int i = 0; i < diff.Length; i++)
            {
                diff[i] = (y[i + 1] - y[i])/(x[i + 1] - x[i]);
            }

            for (int i = 1; i < weights.Length; i++)
            {
                weights[i] = Math.Abs(diff[i] - diff[i - 1]);
            }

            /* Prepare Hermite interpolation scheme */

            var dd = new double[x.Length];

            for (int i = 2; i < dd.Length - 2; i++)
            {
                dd[i] = weights[i - 1].AlmostEqual(0.0) && weights[i + 1].AlmostEqual(0.0)
                    ? (((x[i + 1] - x[i])*diff[i - 1]) + ((x[i] - x[i - 1])*diff[i]))/(x[i + 1] - x[i - 1])
                    : ((weights[i + 1]*diff[i - 1]) + (weights[i - 1]*diff[i]))/(weights[i + 1] + weights[i - 1]);
            }

            dd[0] = DifferentiateThreePoint(x, y, 0, 0, 1, 2);
            dd[1] = DifferentiateThreePoint(x, y, 1, 0, 1, 2);
            dd[x.Length - 2] = DifferentiateThreePoint(x, y, x.Length - 2, x.Length - 3, x.Length - 2, x.Length - 1);
            dd[x.Length - 1] = DifferentiateThreePoint(x, y, x.Length - 1, x.Length - 3, x.Length - 2, x.Length - 1);

            /* Build Akima spline using Hermite interpolation scheme */

            return InterpolateHermiteSorted(x, y, dd);
        }

        /// <summary>
        /// Create an Akima cubic spline interpolation from an unsorted set of (x,y) value pairs.
        /// Akima splines are robust to outliers.
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static CubicSpline InterpolateAkimaInplace(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            Sorting.Sort(x, y);
            return InterpolateAkimaSorted(x, y);
        }

        /// <summary>
        /// Create an Akima cubic spline interpolation from an unsorted set of (x,y) value pairs.
        /// Akima splines are robust to outliers.
        /// </summary>
        public static CubicSpline InterpolateAkima(IEnumerable<double> x, IEnumerable<double> y)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolateAkimaInplace(x.ToArray(), y.ToArray());
        }

        /// <summary>
        /// Creates a modified Akima (makima) cubic spline interpolation from a set of (x,y) value pairs,
        /// sorted ascendingly by x.
        /// </summary>
        /// <param name="x">Sample points (must be sorted ascendingly)</param>
        /// <param name="y">Sample values corresponding to x</param>
        /// <returns>A CubicSpline instance using the makima method</returns>
        public static CubicSpline InterpolateMakimaSorted(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }
            if (x.Length < 5)
            {
                throw new ArgumentException("The given array is too small. It must be at least 5 long.", nameof(x));
            }

            var n = x.Length;
            var dx = new double[n - 1];
            for (var i = 0; i < n - 1; i++)
            {
                dx[i] = x[i + 1] - x[i];
            }

            // Allocate m with padding: length = n + 3
            var mLen = n + 3;
            var m = new double[mLen];
            // m[2] .. m[n+1] : slopes between adjacent points.
            for (var i = 0; i < n - 1; i++)
            {
                m[i + 2] = (y[i + 1] - y[i]) / dx[i];
            }

            // Extrapolate two additional points on the left.
            m[1] = 2 * m[2] - m[3];
            m[0] = 2 * m[1] - m[2];

            // Extrapolate two additional points on the right.
            m[n + 1] = 2 * m[n] - m[n - 1];
            m[n + 2] = 2 * m[n + 1] - m[n];

            // Compute differences (dm) and sums (pm) for makima weights.
            var dmLen = mLen - 1;
            var dm = new double[dmLen];
            var pm = new double[dmLen];
            for (var i = 0; i < dmLen; i++)
            {
                dm[i] = Math.Abs(m[i + 1] - m[i]);
                pm[i] = Math.Abs(m[i + 1] + m[i]);
            }

            // Compute modified weights: f1 and f2 for indices 0 .. n-1.
            var f1 = new double[n];
            var f2 = new double[n];
            for (var i = 0; i < n; i++)
            {
                f1[i] = dm[i + 2] + 0.5 * pm[i + 2];
                f2[i] = dm[i] + 0.5 * pm[i];
            }
            var f12 = new double[n];
            for (var i = 0; i < n; i++)
            {
                f12[i] = f1[i] + f2[i];
            }

            // Initial derivative estimates: t[i] = 0.5*(m[i+3] + m[i])
            var t = new double[n];
            for (var i = 0; i < n; i++)
            {
                t[i] = 0.5 * (m[i + 3] + m[i]);
            }

            // Determine threshold from maximum of f12.
            var maxF12 = 0.0;
            for (var i = 0; i < n; i++)
            {
                if (f12[i] > maxF12)
                    maxF12 = f12[i];
            }
            var threshold = 1e-9 * maxF12;

            // For indices where f12 is significant, update t[i] using weighted average.
            for (var i = 0; i < n; i++)
            {
                if (f12[i] > threshold)
                {
                    t[i] = (f1[i] * m[i + 1] + f2[i] * m[i + 2]) / f12[i];
                }
            }

            // Construct the Hermite cubic spline using the computed derivatives.
            return InterpolateHermiteSorted(x, y, t);
        }

        /// <summary>
        /// Creates a modified Akima (makima) cubic spline interpolation from an unsorted set of (x,y) value pairs.
        /// WARNING: This method works in-place and will sort the input arrays.
        /// </summary>
        /// <param name="x">Sample points</param>
        /// <param name="y">Sample values corresponding to x</param>
        /// <returns>A CubicSpline instance using the makima method</returns>
        public static CubicSpline InterpolateMakimaInplace(double[] x, double[] y)
        {
            // Sorting.Sort is assumed to sort x and apply the same permutation to y.
            Sorting.Sort(x, y);
            return InterpolateMakimaSorted(x, y);
        }

        /// <summary>
        /// Creates a modified Akima (makima) cubic spline interpolation from an unsorted set of (x,y) value pairs.
        /// </summary>
        /// <param name="x">Sample points as an IEnumerable</param>
        /// <param name="y">Sample values corresponding to x as an IEnumerable</param>
        /// <returns>A CubicSpline instance using the makima method</returns>
        public static CubicSpline InterpolateMakima(IEnumerable<double> x, IEnumerable<double> y)
        {
            return InterpolateMakimaInplace(x.ToArray(), y.ToArray());
        }

        /// <summary>
        /// Create a piecewise cubic Hermite interpolating polynomial from an unsorted set of (x,y) value pairs.
        /// Monotone-preserving interpolation with continuous first derivative.
        /// </summary>
        public static CubicSpline InterpolatePchipSorted(double[] x, double[] y)
        {
            // Implementation based on "Numerical Computing with Matlab" (Moler, 2004).

            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 3)
            {
                throw new ArgumentException("The given array is too small. It must be at least 3 long.", nameof(x));
            }

            var m = new double[x.Length - 1];

            for (int i = 0; i < m.Length; i++)
            {
                m[i] = (y[i + 1] - y[i])/(x[i + 1] - x[i]);
            }

            var dd = new double[x.Length];
            var hPrev = x[1] - x[0];
            var mPrevIs0 = m[0].AlmostEqual(0.0);

            for (var i = 1; i < x.Length - 1; ++i)
            {
                var h = x[i + 1] - x[i];
                var mIs0 = m[i].AlmostEqual(0.0);

                if (mIs0 || mPrevIs0 || Math.Sign(m[i]) != Math.Sign(m[i - 1]))
                {
                    dd[i] = 0;
                }
                else
                {
                    // Weighted harmonic mean of each slope.
                    var w1 = 2 * h + hPrev;
                    var w2 = h + 2 * hPrev;
                    dd[i] = (w1 + w2) / (w1 / m[i - 1] + w2 / m[i]);
                }

                hPrev = h;
                mPrevIs0 = mIs0;
            }

            // Special case end-points.
            dd[0] = PchipEndPoints(x[1] - x[0], x[2] - x[1], m[0], m[1]);
            dd[dd.Length - 1] = PchipEndPoints(
                x[x.Length - 1] - x[x.Length - 2], x[x.Length - 2] - x[x.Length - 3],
                m[m.Length - 1], m[m.Length - 2]);

            return InterpolateHermiteSorted(x, y, dd);
        }

        static double PchipEndPoints(double h0, double h1, double m0, double m1)
        {
            // One-sided, shape-preserving, three-point estimate for the derivative.
            var d = ((2 * h0 + h1) * m0 - h0 * m1) / (h0 + h1);

            if (Math.Sign(d) != Math.Sign(m0))
            {
                return 0.0;
            }

            if (Math.Sign(m0) != Math.Sign(m1) && (Math.Abs(d) > 3 * Math.Abs(m0)))
            {
                return 3 * m0;
            }

            return d;
        }

        /// <summary>
        /// Create a piecewise cubic Hermite interpolating polynomial from an unsorted set of (x,y) value pairs.
        /// Monotone-preserving interpolation with continuous first derivative.
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static CubicSpline InterpolatePchipInplace(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            Sorting.Sort(x, y);
            return InterpolatePchipSorted(x, y);
        }

        /// <summary>
        /// Create a piecewise cubic Hermite interpolating polynomial from an unsorted set of (x,y) value pairs.
        /// Monotone-preserving interpolation with continuous first derivative.
        /// </summary>
        public static CubicSpline InterpolatePchip(IEnumerable<double> x, IEnumerable<double> y)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolatePchipInplace(x.ToArray(), y.ToArray());
        }

        /// <summary>
        /// Create a cubic spline interpolation from a set of (x,y) value pairs, sorted ascendingly by x,
        /// and custom boundary/termination conditions.
        /// </summary>
        public static CubicSpline InterpolateBoundariesSorted(double[] x, double[] y,
            SplineBoundaryCondition leftBoundaryCondition, double leftBoundary,
            SplineBoundaryCondition rightBoundaryCondition, double rightBoundary)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            if (x.Length < 2)
            {
                throw new ArgumentException("The given array is too small. It must be at least 2 long.", nameof(x));
            }

            int n = x.Length;

            // normalize special cases
            if ((n == 2)
                && (leftBoundaryCondition == SplineBoundaryCondition.ParabolicallyTerminated)
                && (rightBoundaryCondition == SplineBoundaryCondition.ParabolicallyTerminated))
            {
                leftBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                leftBoundary = 0d;
                rightBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                rightBoundary = 0d;
            }

            if (leftBoundaryCondition == SplineBoundaryCondition.Natural)
            {
                leftBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                leftBoundary = 0d;
            }

            if (rightBoundaryCondition == SplineBoundaryCondition.Natural)
            {
                rightBoundaryCondition = SplineBoundaryCondition.SecondDerivative;
                rightBoundary = 0d;
            }

            var a1 = new double[n];
            var a2 = new double[n];
            var a3 = new double[n];
            var b = new double[n];

            // Left Boundary
            switch (leftBoundaryCondition)
            {
                case SplineBoundaryCondition.ParabolicallyTerminated:
                    a1[0] = 0;
                    a2[0] = 1;
                    a3[0] = 1;
                    b[0] = 2*(y[1] - y[0])/(x[1] - x[0]);
                    break;
                case SplineBoundaryCondition.FirstDerivative:
                    a1[0] = 0;
                    a2[0] = 1;
                    a3[0] = 0;
                    b[0] = leftBoundary;
                    break;
                case SplineBoundaryCondition.SecondDerivative:
                    a1[0] = 0;
                    a2[0] = 2;
                    a3[0] = 1;
                    b[0] = (3*((y[1] - y[0])/(x[1] - x[0]))) - (0.5*leftBoundary*(x[1] - x[0]));
                    break;
                default:
                    throw new NotSupportedException("Invalid Left Boundary Condition.");
            }

            // Central Conditions
            for (int i = 1; i < x.Length - 1; i++)
            {
                a1[i] = x[i + 1] - x[i];
                a2[i] = 2*(x[i + 1] - x[i - 1]);
                a3[i] = x[i] - x[i - 1];
                b[i] = (3*(y[i] - y[i - 1])/(x[i] - x[i - 1])*(x[i + 1] - x[i])) + (3*(y[i + 1] - y[i])/(x[i + 1] - x[i])*(x[i] - x[i - 1]));
            }

            // Right Boundary
            switch (rightBoundaryCondition)
            {
                case SplineBoundaryCondition.ParabolicallyTerminated:
                    a1[n - 1] = 1;
                    a2[n - 1] = 1;
                    a3[n - 1] = 0;
                    b[n - 1] = 2*(y[n - 1] - y[n - 2])/(x[n - 1] - x[n - 2]);
                    break;
                case SplineBoundaryCondition.FirstDerivative:
                    a1[n - 1] = 0;
                    a2[n - 1] = 1;
                    a3[n - 1] = 0;
                    b[n - 1] = rightBoundary;
                    break;
                case SplineBoundaryCondition.SecondDerivative:
                    a1[n - 1] = 1;
                    a2[n - 1] = 2;
                    a3[n - 1] = 0;
                    b[n - 1] = (3*(y[n - 1] - y[n - 2])/(x[n - 1] - x[n - 2])) + (0.5*rightBoundary*(x[n - 1] - x[n - 2]));
                    break;
                default:
                    throw new NotSupportedException("Invalid Right Boundary Condition.");
            }

            // Build Spline
            double[] dd = SolveTridiagonal(a1, a2, a3, b);
            return InterpolateHermiteSorted(x, y, dd);
        }

        /// <summary>
        /// Create a cubic spline interpolation from an unsorted set of (x,y) value pairs and custom boundary/termination conditions.
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static CubicSpline InterpolateBoundariesInplace(double[] x, double[] y,
            SplineBoundaryCondition leftBoundaryCondition, double leftBoundary,
            SplineBoundaryCondition rightBoundaryCondition, double rightBoundary)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException("All vectors must have the same dimensionality.");
            }

            Sorting.Sort(x, y);
            return InterpolateBoundariesSorted(x, y, leftBoundaryCondition, leftBoundary, rightBoundaryCondition, rightBoundary);
        }

        /// <summary>
        /// Create a cubic spline interpolation from an unsorted set of (x,y) value pairs and custom boundary/termination conditions.
        /// </summary>
        public static CubicSpline InterpolateBoundaries(IEnumerable<double> x, IEnumerable<double> y,
            SplineBoundaryCondition leftBoundaryCondition, double leftBoundary,
            SplineBoundaryCondition rightBoundaryCondition, double rightBoundary)
        {
            // note: we must make a copy, even if the input was arrays already
            return InterpolateBoundariesInplace(x.ToArray(), y.ToArray(), leftBoundaryCondition, leftBoundary, rightBoundaryCondition, rightBoundary);
        }

        /// <summary>
        /// Create a natural cubic spline interpolation from a set of (x,y) value pairs
        /// and zero second derivatives at the two boundaries, sorted ascendingly by x.
        /// </summary>
        public static CubicSpline InterpolateNaturalSorted(double[] x, double[] y)
        {
            return InterpolateBoundariesSorted(x, y, SplineBoundaryCondition.SecondDerivative, 0.0, SplineBoundaryCondition.SecondDerivative, 0.0);
        }

        /// <summary>
        /// Create a natural cubic spline interpolation from an unsorted set of (x,y) value pairs
        /// and zero second derivatives at the two boundaries.
        /// WARNING: Works in-place and can thus causes the data array to be reordered.
        /// </summary>
        public static CubicSpline InterpolateNaturalInplace(double[] x, double[] y)
        {
            return InterpolateBoundariesInplace(x, y, SplineBoundaryCondition.SecondDerivative, 0.0, SplineBoundaryCondition.SecondDerivative, 0.0);
        }

        /// <summary>
        /// Create a natural cubic spline interpolation from an unsorted set of (x,y) value pairs
        /// and zero second derivatives at the two boundaries.
        /// </summary>
        public static CubicSpline InterpolateNatural(IEnumerable<double> x, IEnumerable<double> y)
        {
            return InterpolateBoundaries(x, y, SplineBoundaryCondition.SecondDerivative, 0.0, SplineBoundaryCondition.SecondDerivative, 0.0);
        }

        /// <summary>
        /// Three-Point Differentiation Helper.
        /// </summary>
        /// <param name="xx">Sample Points t.</param>
        /// <param name="yy">Sample Values x(t).</param>
        /// <param name="indexT">Index of the point of the differentiation.</param>
        /// <param name="index0">Index of the first sample.</param>
        /// <param name="index1">Index of the second sample.</param>
        /// <param name="index2">Index of the third sample.</param>
        /// <returns>The derivative approximation.</returns>
        static double DifferentiateThreePoint(double[] xx, double[] yy, int indexT, int index0, int index1, int index2)
        {
            double x0 = yy[index0];
            double x1 = yy[index1];
            double x2 = yy[index2];

            double t = xx[indexT] - xx[index0];
            double t1 = xx[index1] - xx[index0];
            double t2 = xx[index2] - xx[index0];

            double a = (x2 - x0 - (t2/t1*(x1 - x0)))/(t2*(t2 - t1));
            double b = (x1 - x0 - a*t1*t1)/t1;
            return (2*a*t) + b;
        }

        /// <summary>
        /// Tridiagonal Solve Helper.
        /// </summary>
        /// <param name="a">The a-vector[n].</param>
        /// <param name="b">The b-vector[n], will be modified by this function.</param>
        /// <param name="c">The c-vector[n].</param>
        /// <param name="d">The d-vector[n], will be modified by this function.</param>
        /// <returns>The x-vector[n]</returns>
        static double[] SolveTridiagonal(double[] a, double[] b, double[] c, double[] d)
        {
            for (int k = 1; k < a.Length; k++)
            {
                double t = a[k]/b[k - 1];
                b[k] = b[k] - (t*c[k - 1]);
                d[k] = d[k] - (t*d[k - 1]);
            }

            var x = new double[a.Length];
            x[x.Length - 1] = d[d.Length - 1]/b[b.Length - 1];
            for (int k = x.Length - 2; k >= 0; k--)
            {
                x[k] = (d[k] - (c[k]*x[k + 1]))/b[k];
            }

            return x;
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
            var x = t - _x[k];
            return _c0[k] + x*(_c1[k] + x*(_c2[k] + x*_c3[k]));
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        public double Differentiate(double t)
        {
            int k = LeftSegmentIndex(t);
            var x = t - _x[k];
            return _c1[k] + x*(2*_c2[k] + x*3*_c3[k]);
        }

        /// <summary>
        /// Differentiate twice at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated second derivative at point t.</returns>
        public double Differentiate2(double t)
        {
            int k = LeftSegmentIndex(t);
            var x = t - _x[k];
            return 2*_c2[k] + x*6*_c3[k];
        }

        /// <summary>
        /// Differentiate three times at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated third derivative at point t.</returns>
        public double Differentiate3(double t)
        {
            int k = LeftSegmentIndex(t);
            return 6*_c3[k];
        }

        /// <summary>
        /// Indefinite integral at point t.
        /// </summary>
        /// <param name="t">Point t to integrate at.</param>
        public double Integrate(double t)
        {
            int k = LeftSegmentIndex(t);
            var x = t - _x[k];
            return _indefiniteIntegral.Value[k] + x*(_c0[k] + x*(_c1[k]/2 + x*(_c2[k]/3 + x*_c3[k]/4)));
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
                integral[i + 1] = integral[i] + w*(_c0[i] + w*(_c1[i]/2 + w*(_c2[i]/3 + w*_c3[i]/4)));
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

        /// <summary>
        /// Gets all the t values where the derivative is 0
        /// see: https://mathworld.wolfram.com/StationaryPoint.html
        /// </summary>
        /// <returns>An array of t values (in the domain of the function) where the derivative of the spline is 0</returns>
        public double[] StationaryPoints()
        {
            List<double> points = new List<double>();
            for (int index = 0; index < _x.Length - 1; index++)
            {
                double a = 6 * _c3[index]; //derive ax^3 and multiply by 2
                double b = 2 * _c2[index]; //derive bx^2
                double c = _c1[index];//derive cx
                double d = b * b - 2 * a * c;
                //first check if a is 0, if so its a linear function, this happens with quadratic condition
                if (a.AlmostEqual(0))
                {
                    double x = _x[index] - c / b;
                    //check if the result is in the domain
                    if (_x[index] <= x && x <= _x[index + 1]) points.Add(x);
                }
                else if (d.AlmostEqual(0))//its a quadratic with a single solution
                {
                    double x = _x[index] - b / a;
                    if (_x[index] <= x && x <= _x[index + 1]) points.Add(x);
                }
                else if (d > 0)//only has a solution if d is greater than 0
                {
                    d = (double)System.Math.Sqrt(d);
                    //apply quadratic equation
                    double x1 = _x[index] + (-b + d) / a;
                    double x2 = _x[index] + (-b - d) / a;
                    //Add any solution points that fall within the domain to the list
                    if ((_x[index] <= x1) && (x1 <= _x[index + 1])) points.Add(x1);
                    if ((_x[index] <= x2) && (x2 <= _x[index + 1])) points.Add(x2);
                }
            }
            return points.ToArray();
        }

        /// <summary>
        /// Returns the t values in the domain of the spline for which it takes the minimum and maximum value.
        /// </summary>
        /// <returns>A tuple containing the t value for which the spline is minimum in the first component and maximum in the second component </returns>
        public Tuple<double, double> Extrema()
        {
            //Check the edges of the domain
            //set the initial values to the leftmost domain point
            double t = _x[0];
            double max = Interpolate(t);
            double min = max;
            double minT = t;
            double maxT = t;
            //check the rightmost domain point
            t = _x[_x.Length-1];
            var ty = Interpolate(t);
            if (ty > max)
            {
                max = ty;
                maxT = t;
            }
            if (ty < min)
            {
                min = ty;
                minT = t;
            }
            //check the the inflexion, local minimums and local maximums
            double[] pointsToCheck = StationaryPoints();
            foreach (double p in pointsToCheck)
            {
                double y = Interpolate(p);
                if (y > max)
                {
                    max = y;
                    maxT = p;
                }
                if (y < min)
                {
                    min = y;
                    minT = p;
                }
            }
            return new Tuple<double, double>(minT, maxT);
        }
    }
}
