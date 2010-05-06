// <copyright file="Interpolate.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.Interpolation
{
    using System.Collections.Generic;
    using Algorithms;

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
        public static IInterpolation Common(
            IList<double> points,
            IList<double> values)
        {
            return RationalWithoutPoles(points, values);
        }

        /// <summary>
        /// Create a linear spline interpolation based on arbitrary points (sorted ascending).
        /// </summary>
        /// <param name="points">The sample points t, sorted ascending. Supports both lists and arrays.</param>
        /// <param name="values">The sample point values x(t). Supports both lists and arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static IInterpolation LinearBetweenPoints(
            IList<double> points,
            IList<double> values)
        {
            LinearSplineInterpolation method = new LinearSplineInterpolation();
            method.Initialize(points, values);
            return method;
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
        public static IInterpolation RationalWithoutPoles(
            IList<double> points,
            IList<double> values)
        {
            FloaterHormannRationalInterpolation method = new FloaterHormannRationalInterpolation();
            method.Initialize(points, values);
            return method;
        }

        /// <summary>
        /// Create a burlish stoer rational interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t. Supports both lists and arrays.</param>
        /// <param name="values">The sample point values x(t). Supports both lists and arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static IInterpolation RationalWithPoles(
            IList<double> points,
            IList<double> values)
        {
            BulirschStoerRationalInterpolation method = new BulirschStoerRationalInterpolation();
            method.Initialize(points, values);
            return method;
        }
    }
}