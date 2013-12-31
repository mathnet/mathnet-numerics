// <copyright file="Interpolate.cs" company="Math.NET">
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

using System.Collections.Generic;
using MathNet.Numerics.Interpolation;

namespace MathNet.Numerics
{
    /// <summary>
    /// Interpolation Factory.
    /// </summary>
    public static class Interpolate
    {
        /// <summary>
        /// Creates an interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t. Supports both lists and arrays.</param>
        /// <param name="values">The sample point values x(t). Supports both lists and arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static IInterpolation Common(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.Barycentric.InterpolateRationalFloaterHormann(points, values);
        }

        /// <summary>
        /// Create a floater hormann rational pole-free interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t. Supports both lists and arrays.</param>
        /// <param name="values">The sample point values x(t). Supports both lists and arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static IInterpolation RationalWithoutPoles(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.Barycentric.InterpolateRationalFloaterHormann(points, values);
        }

        /// <summary>
        /// Create a burlisch stoer rational interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t.  Optimized for arrays..</param>
        /// <param name="values">The sample point values x(t).  Optimized for arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static IInterpolation RationalWithPoles(IEnumerable<double> points, IEnumerable<double> values)
        {
            return new BulirschStoerRationalInterpolation(points, values);
        }

        /// <summary>
        /// Create a barycentric polynomial interpolation where the given sample points are equidistant.
        /// </summary>
        /// <param name="points">The sample points t, must be equidistant. Optimized for arrays.</param>
        /// <param name="values">The sample point values x(t). Optimized for arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// The value pairs do not have to be sorted, but if they are not sorted ascendingly
        /// and the passed x and y arguments are arrays, they will be sorted inplace and thus modified.
        /// </remarks>
        public static IInterpolation PolynomialEquidistant(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.Barycentric.InterpolatePolynomialEquidistant(points, values);
        }

        /// <summary>
        /// Create a neville polynomial interpolation based on arbitrary points.
        /// If the points happen to be equidistant, consider to use the much more robust PolynomialEquidistant instead.
        /// Otherwise, consider whether RationalWithoutPoles would not be a more robust alternative.
        /// </summary>
        /// <param name="points">The sample points t.  Optimized for arrays.</param>
        /// <param name="values">The sample point values x(t).  Optimized for arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static IInterpolation Polynomial(IEnumerable<double> points, IEnumerable<double> values)
        {
            return new NevillePolynomialInterpolation(points, values);
        }

        /// <summary>
        /// Create a piecewise linear spline interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t. Optimized for arrays.</param>
        /// <param name="values">The sample point values x(t). Optimized for arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// The value pairs do not have to be sorted, but if they are not sorted ascendingly
        /// and the passed x and y arguments are arrays, they will be sorted inplace and thus modified.
        /// </remarks>
        public static IInterpolation LinearSpline(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.LinearSpline.Interpolate(points, values);
        }

        /// <summary>
        /// Create an piecewise natural cubic spline interpolation based on arbitrary points, with zero secondary derivatives at the boundaries.
        /// </summary>
        /// <param name="points">The sample points t. Optimized for arrays.</param>
        /// <param name="values">The sample point values x(t). Optimized for arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// The value pairs do not have to be sorted, but if they are not sorted ascendingly
        /// and the passed x and y arguments are arrays, they will be sorted inplace and thus modified.
        /// </remarks>
        public static IInterpolation CubicSpline(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.CubicSpline.InterpolateNatural(points, values);
        }

        /// <summary>
        /// Create an piecewise cubic Akima spline interpolation based on arbitrary points. Akima splines are robust to outliers.
        /// </summary>
        /// <param name="points">The sample points t. Optimized for arrays.</param>
        /// <param name="values">The sample point values x(t). Optimized for arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// The value pairs do not have to be sorted, but if they are not sorted ascendingly
        /// and the passed x and y arguments are arrays, they will be sorted inplace and thus modified.
        /// </remarks>
        public static IInterpolation CubicSplineRobust(IEnumerable<double> points, IEnumerable<double> values)
        {
            return Interpolation.CubicSpline.InterpolateAkima(points, values);
        }

        /// <summary>
        /// Create a piecewise cubic Hermite spline interpolation based on arbitrary points and their slopes/first derivative.
        /// </summary>
        /// <param name="points">The sample points t. Optimized for arrays.</param>
        /// <param name="values">The sample point values x(t). Optimized for arrays.</param>
        /// <param name="firstDerivatives">The slope at the sample points. Optimized for arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        /// <remarks>
        /// The value pairs do not have to be sorted, but if they are not sorted ascendingly
        /// and the passed x and y arguments are arrays, they will be sorted inplace and thus modified.
        /// </remarks>
        public static IInterpolation CubicSplineWithDerivatives(IEnumerable<double> points, IEnumerable<double> values, IEnumerable<double> firstDerivatives)
        {
            return Interpolation.CubicSpline.InterpolateHermite(points, values, firstDerivatives);
        }
    }
}
