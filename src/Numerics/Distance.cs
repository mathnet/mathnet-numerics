// <copyright file="Distance.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics
{
    public static class Distance
    {
        /// <summary>
        /// Sum of Absolute Difference (SAD), i.e. the L1-norm (Manhattan) of the difference.
        /// </summary>
        public static double SAD(Vector<double> a, Vector<double> b)
        {
            return (a - b).L1Norm();
        }

        /// <summary>
        /// Sum of Absolute Difference (SAD), i.e. the L1-norm (Manhattan) of the difference.
        /// </summary>
        public static float SAD(Vector<float> a, Vector<float> b)
        {
            return (a - b).L1Norm();
        }

        /// <summary>
        /// Sum of Absolute Difference (SAD), i.e. the L1-norm (Manhattan) of the difference.
        /// </summary>
        public static double SAD(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException(Resources.ArgumentVectorsSameLength);

            var sum = 0d;
            for (var i = 0; i < a.Length; i++)
            {
                sum += Math.Abs(a[i] - b[i]);
            }
            return sum;
        }

        /// <summary>
        /// Sum of Absolute Difference (SAD), i.e. the L1-norm (Manhattan) of the difference.
        /// </summary>
        public static float SAD(float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException(Resources.ArgumentVectorsSameLength);

            var sum = 0f;
            for (var i = 0; i < a.Length; i++)
            {
                sum += Math.Abs(a[i] - b[i]);
            }
            return sum;
        }

        /// <summary>
        /// Mean-Absolute Error (MAE), i.e. the normalized L1-norm (Manhattan) of the difference.
        /// </summary>
        public static double MAE(Vector<double> a, Vector<double> b)
        {
            return (a - b).L1Norm()/a.Count;
        }

        /// <summary>
        /// Mean-Absolute Error (MAE), i.e. the normalized L1-norm (Manhattan) of the difference.
        /// </summary>
        public static float MAE(Vector<float> a, Vector<float> b)
        {
            return (a - b).L1Norm()/a.Count;
        }

        /// <summary>
        /// Mean-Absolute Error (MAE), i.e. the normalized L1-norm (Manhattan) of the difference.
        /// </summary>
        public static double MAE(double[] a, double[] b)
        {
            return SAD(a, b)/a.Length;
        }

        /// <summary>
        /// Mean-Absolute Error (MAE), i.e. the normalized L1-norm (Manhattan) of the difference.
        /// </summary>
        public static float MAE(float[] a, float[] b)
        {
            return SAD(a, b)/a.Length;
        }

        /// <summary>
        /// Sum of Squared Difference (SSD), i.e. the squared L2-norm (Euclidean) of the difference.
        /// </summary>
        public static double SSD(Vector<double> a, Vector<double> b)
        {
            var norm = (a - b).L2Norm();
            return norm*norm;
        }

        /// <summary>
        /// Sum of Squared Difference (SSD), i.e. the squared L2-norm (Euclidean) of the difference.
        /// </summary>
        public static float SSD(Vector<float> a, Vector<float> b)
        {
            var norm = (a - b).L2Norm();
            return norm*norm;
        }

        /// <summary>
        /// Sum of Squared Difference (SSD), i.e. the squared L2-norm (Euclidean) of the difference.
        /// </summary>
        public static double SSD(double[] a, double[] b)
        {
            var diff = new double[a.Length];
            Control.LinearAlgebraProvider.SubtractArrays(a, b, diff);
            return Control.LinearAlgebraProvider.DotProduct(diff, diff);
        }

        /// <summary>
        /// Sum of Squared Difference (SSD), i.e. the squared L2-norm (Euclidean) of the difference.
        /// </summary>
        public static float SSD(float[] a, float[] b)
        {
            var diff = new float[a.Length];
            Control.LinearAlgebraProvider.SubtractArrays(a, b, diff);
            return Control.LinearAlgebraProvider.DotProduct(diff, diff);
        }

        /// <summary>
        /// Mean-Squared Error (MSE), i.e. the normalized squared L2-norm (Euclidean) of the difference.
        /// </summary>
        public static double MSE(Vector<double> a, Vector<double> b)
        {
            var norm = (a - b).L2Norm();
            return norm*norm/a.Count;
        }

        /// <summary>
        /// Mean-Squared Error (MSE), i.e. the normalized squared L2-norm (Euclidean) of the difference.
        /// </summary>
        public static float MSE(Vector<float> a, Vector<float> b)
        {
            var norm = (a - b).L2Norm();
            return norm*norm/a.Count;
        }

        /// <summary>
        /// Mean-Squared Error (MSE), i.e. the normalized squared L2-norm (Euclidean) of the difference.
        /// </summary>
        public static double MSE(double[] a, double[] b)
        {
            return SSD(a, b)/a.Length;
        }

        /// <summary>
        /// Mean-Squared Error (MSE), i.e. the normalized squared L2-norm (Euclidean) of the difference.
        /// </summary>
        public static float MSE(float[] a, float[] b)
        {
            return SSD(a, b)/a.Length;
        }
    }
}
