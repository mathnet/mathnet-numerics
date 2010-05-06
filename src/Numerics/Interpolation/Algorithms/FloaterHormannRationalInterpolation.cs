// <copyright file="FloaterHormannRationalInterpolation.cs" company="Math.NET">
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

namespace MathNet.Numerics.Interpolation.Algorithms
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Barycentric Rational Interpolation without poles, using Mike Floater and Kai Hormann's Algorithm.
    /// </summary>
    /// <remarks>
    /// This algorithm neither supports differentiation nor integration.
    /// </remarks>
    public class FloaterHormannRationalInterpolation : IInterpolation
    {
        /// <summary>
        /// Internal Barycentric Interpolation
        /// </summary>
        private readonly BarycentricInterpolation _barycentric;

        /// <summary>
        /// Initializes a new instance of the FloaterHormannRationalInterpolation class.
        /// </summary>
        public FloaterHormannRationalInterpolation()
        {
            _barycentric = new BarycentricInterpolation();
        }

        /// <summary>
        /// Initializes a new instance of the FloaterHormannRationalInterpolation class.
        /// </summary>
        /// <param name="samplePoints">Sample Points t</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        public FloaterHormannRationalInterpolation(
            IList<double> samplePoints,
            IList<double> sampleValues)
        {
            _barycentric = new BarycentricInterpolation();
            Initialize(samplePoints, sampleValues);
        }

        /// <summary>
        /// Initializes a new instance of the FloaterHormannRationalInterpolation class.
        /// </summary>
        /// <param name="samplePoints">Sample Points t</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <param name="order">
        /// Order of the interpolation scheme, 0 &lt;= order &lt;= N.
        /// In most cases a value between 3 and 8 gives good results.
        /// </param>
        public FloaterHormannRationalInterpolation(
            IList<double> samplePoints,
            IList<double> sampleValues,
            int order)
        {
            _barycentric = new BarycentricInterpolation();
            Initialize(samplePoints, sampleValues, order);
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports differentiation (interpolated derivative).
        /// </summary>
        /// <seealso cref="IInterpolation.Differentiate(double)"/>
        /// <seealso cref="IInterpolation.Differentiate(double, out double, out double)"/>
        bool IInterpolation.SupportsDifferentiation
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the algorithm supports integration (interpolated quadrature).
        /// </summary>
        /// <seealso cref="IInterpolation.Integrate"/>
        bool IInterpolation.SupportsIntegration
        {
            get { return false; }
        }

        /// <summary>
        /// Initialize the interpolation method with the given sample set.
        /// </summary>
        /// <remarks>
        /// The interpolation scheme order will be set to 3.
        /// </remarks>
        /// <param name="samplePoints">Sample Points t (no sorting assumed)</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        public void Initialize(
            IList<double> samplePoints,
            IList<double> sampleValues)
        {
            if (null == samplePoints)
            {
                throw new ArgumentNullException("samplePoints");
            }

            double[] weights = EvaluateBarycentricWeights(
                samplePoints,
                sampleValues,
                Math.Min(3, samplePoints.Count - 1));

            _barycentric.Initialize(samplePoints, sampleValues, weights);
        }

        /// <summary>
        /// Initialize the interpolation method with the given sample set (no sorting assumed).
        /// </summary>
        /// <param name="samplePoints">Sample Points t</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <param name="order">
        /// Order of the interpolation scheme, 0 &lt;= order &lt;= N.
        /// In most cases a value between 3 and 8 gives good results.
        /// </param>
        public void Initialize(
            IList<double> samplePoints,
            IList<double> sampleValues,
            int order)
        {
            double[] weights = EvaluateBarycentricWeights(
                samplePoints,
                sampleValues,
                order);

            _barycentric.Initialize(samplePoints, sampleValues, weights);
        }

        /// <summary>
        /// Evaluate the barycentric weights as used
        /// internally by this interpolation algorithm.
        /// </summary>
        /// <param name="samplePoints">Sample Points t</param>
        /// <param name="sampleValues">Sample Values x(t)</param>
        /// <param name="order">
        /// Order of the interpolation scheme, 0 &lt;= order &lt;= N.
        /// In most cases a value between 3 and 8 gives good results.
        /// </param>
        /// <returns>Barycentric Weight Vector</returns>
        public static double[] EvaluateBarycentricWeights(
            IList<double> samplePoints,
            IList<double> sampleValues,
            int order)
        {
            if (null == samplePoints)
            {
                throw new ArgumentNullException("samplePoints");
            }

            if (null == sampleValues)
            {
                throw new ArgumentNullException("sampleValues");
            }

            if (samplePoints.Count < 1)
            {
                throw new ArgumentOutOfRangeException("samplePoints");
            }

            if (samplePoints.Count != sampleValues.Count)
            {
                throw new ArgumentException(Properties.Resources.ArgumentVectorsSameLength);
            }

            if (0 > order || samplePoints.Count <= order)
            {
                throw new ArgumentOutOfRangeException("order");
            }

            double[] sortedWeights = new double[sampleValues.Count];
            double[] sortedPoints = new double[samplePoints.Count];
            samplePoints.CopyTo(sortedPoints, 0);

            // order: odd -> negative, even -> positive
            double sign = ((order & 0x1) == 0x1) ? -1.0 : 1.0;

            // init permutation vector
            int[] perm = new int[sortedWeights.Length];
            for (int i = 0; i < perm.Length; i++)
            {
                perm[i] = i;
            }

            // sort and update permutation vector
            for (int i = 0; i < perm.Length - 1; i++)
            {
                for (int j = i + 1; j < perm.Length; j++)
                {
                    if (sortedPoints[j] < sortedPoints[i])
                    {
                        double s = sortedPoints[i];
                        sortedPoints[i] = sortedPoints[j];
                        sortedPoints[j] = s;
                        int k = perm[i];
                        perm[i] = perm[j];
                        perm[j] = k;
                    }
                }
            }

            // compute barycentric weights
            for (int k = 0; k < sortedWeights.Length; k++)
            {
                double s = 0;
                for (int i = Math.Max(k - order, 0); i <= Math.Min(k, sortedWeights.Length - 1 - order); i++)
                {
                    double v = 1;
                    for (int j = i; j <= i + order; j++)
                    {
                        if (j != k)
                        {
                            v = v / Math.Abs(sortedPoints[k] - sortedPoints[j]);
                        }
                    }

                    s = s + v;
                }

                sortedWeights[k] = sign * s;
                sign = -sign;
            }

            // reorder back to original order, based on the permutation vector.
            double[] weights = new double[sortedWeights.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[perm[i]] = sortedWeights[i];
            }

            return weights;
        }

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated value x(t).</returns>
        public double Interpolate(double t)
        {
            return _barycentric.Interpolate(t);
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        /// <seealso cref="IInterpolation.SupportsDifferentiation"/>
        /// <seealso cref="IInterpolation.Differentiate(double, out double, out double)"/>
        double IInterpolation.Differentiate(double t)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Differentiate at point t.
        /// </summary>
        /// <param name="t">Point t to interpolate at.</param>
        /// <param name="interpolatedValue">Interpolated value x(t)</param>
        /// <param name="secondDerivative">Interpolated second derivative at point t.</param>
        /// <returns>Interpolated first derivative at point t.</returns>
        /// <seealso cref="IInterpolation.SupportsDifferentiation"/>
        /// <seealso cref="IInterpolation.Differentiate(double)"/>
        double IInterpolation.Differentiate(
            double t,
            out double interpolatedValue,
            out double secondDerivative)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Integrate up to point t.
        /// </summary>
        /// <param name="t">Right bound of the integration interval [a,t].</param>
        /// <returns>Interpolated definite integral over the interval [a,t].</returns>
        /// <seealso cref="IInterpolation.SupportsIntegration"/>
        double IInterpolation.Integrate(double t)
        {
            throw new NotSupportedException();
        }
    }
}