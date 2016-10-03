// <copyright file="SimpleRegression.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearRegression
{
    public static class SimpleRegression
    {
        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning its best fitting parameters as (a, b) tuple,
        /// where a is the intercept and b the slope.
        /// </summary>
        /// <param name="x">Predictor (independent)</param>
        /// <param name="y">Response (dependent)</param>
        public static Tuple<double, double> Fit(double[] x, double[] y)
        {
            if (x.Length != y.Length)
            {
                throw new ArgumentException(string.Format(Resources.SampleVectorsSameLength, x.Length, y.Length));
            }

            if (x.Length <= 1)
            {
                throw new ArgumentException(string.Format(Resources.RegressionNotEnoughSamples, 2, x.Length));
            }

            // First Pass: Mean (Less robust but faster than ArrayStatistics.Mean)
            double mx = 0.0;
            double my = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                mx += x[i];
                my += y[i];
            }

            mx /= x.Length;
            my /= y.Length;

            // Second Pass: Covariance/Variance
            double covariance = 0.0;
            double variance = 0.0;
            for (int i = 0; i < x.Length; i++)
            {
                double diff = x[i] - mx;
                covariance += diff*(y[i] - my);
                variance += diff*diff;
            }

            var b = covariance/variance;
            return new Tuple<double, double>(my - b*mx, b);
        }

        /// <summary>
        /// Least-Squares fitting the points (x,y) to a line y : x -> a+b*x,
        /// returning its best fitting parameters as (a, b) tuple,
        /// where a is the intercept and b the slope.
        /// </summary>
        /// <param name="samples">Predictor-Response samples as tuples</param>
        public static Tuple<double, double> Fit(IEnumerable<Tuple<double, double>> samples)
        {
            var xy = samples.UnpackSinglePass();
            return Fit(xy.Item1, xy.Item2);
        }
    }
}
