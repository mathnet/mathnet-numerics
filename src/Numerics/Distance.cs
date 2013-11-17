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
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Properties;
using MathNet.Numerics.Statistics;

namespace MathNet.Numerics
{
    public static class Distance
    {
        /// <summary>
        /// Sum of Absolute Difference (SAD), i.e. the L1-norm (Manhattan) of the difference.
        /// </summary>
        public static double SAD<T>(Vector<T> a, Vector<T> b) where T : struct, IEquatable<T>, IFormattable
        {
            return (a - b).L1Norm();
        }

        /// <summary>
        /// Sum of Absolute Difference (SAD), i.e. the L1-norm (Manhattan) of the difference.
        /// </summary>
        public static double SAD(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException(Resources.ArgumentVectorsSameLength);

            double sum = 0d;
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

            float sum = 0f;
            for (var i = 0; i < a.Length; i++)
            {
                sum += Math.Abs(a[i] - b[i]);
            }
            return sum;
        }

        /// <summary>
        /// Mean-Absolute Error (MAE), i.e. the normalized L1-norm (Manhattan) of the difference.
        /// </summary>
        public static double MAE<T>(Vector<T> a, Vector<T> b) where T : struct, IEquatable<T>, IFormattable
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
        public static double SSD<T>(Vector<T> a, Vector<T> b) where T : struct, IEquatable<T>, IFormattable
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
        public static double MSE<T>(Vector<T> a, Vector<T> b) where T : struct, IEquatable<T>, IFormattable
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

        /// <summary>
        /// Euclidean Distance, i.e. the L2-norm of the difference.
        /// </summary>
        public static double Euclidean<T>(Vector<T> a, Vector<T> b) where T : struct, IEquatable<T>, IFormattable
        {
            return (a - b).L2Norm();
        }

        /// <summary>
        /// Euclidean Distance, i.e. the L2-norm of the difference.
        /// </summary>
        public static double Euclidean(double[] a, double[] b)
        {
            return Math.Sqrt(SSD(a, b));
        }

        /// <summary>
        /// Euclidean Distance, i.e. the L2-norm of the difference.
        /// </summary>
        public static float Euclidean(float[] a, float[] b)
        {
            return (float) Math.Sqrt(SSD(a, b));
        }

        /// <summary>
        /// Manhattan Distance, i.e. the L1-norm of the difference.
        /// </summary>
        public static double Manhattan<T>(Vector<T> a, Vector<T> b) where T : struct, IEquatable<T>, IFormattable
        {
            return (a - b).L1Norm();
        }

        /// <summary>
        /// Manhattan Distance, i.e. the L1-norm of the difference.
        /// </summary>
        public static double Manhattan(double[] a, double[] b)
        {
            return SAD(a, b);
        }

        /// <summary>
        /// Manhattan Distance, i.e. the L1-norm of the difference.
        /// </summary>
        public static float Manhattan(float[] a, float[] b)
        {
            return SAD(a, b);
        }

        /// <summary>
        /// Chebyshev Distance, i.e. the Infinity-norm of the difference.
        /// </summary>
        public static double Chebyshev<T>(Vector<T> a, Vector<T> b) where T : struct, IEquatable<T>, IFormattable
        {
            return (a - b).InfinityNorm();
        }

        /// <summary>
        /// Chebyshev Distance, i.e. the Infinity-norm of the difference.
        /// </summary>
        public static double Chebyshev(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new ArgumentOutOfRangeException("b");
            double max = Math.Abs(a[0] - b[0]);
            for (int i = 1; i < a.Length; i++)
            {
                var next = Math.Abs(a[i] - b[i]);
                if (next > max)
                {
                    max = next;
                }
            }
            return max;
        }

        /// <summary>
        /// Chebyshev Distance, i.e. the Infinity-norm of the difference.
        /// </summary>
        public static float Chebyshev(float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new ArgumentOutOfRangeException("b");
            float max = Math.Abs(a[0] - b[0]);
            for (int i = 1; i < a.Length; i++)
            {
                var next = Math.Abs(a[i] - b[i]);
                if (next > max)
                {
                    max = next;
                }
            }
            return max;
        }

        /// <summary>
        /// Minkowski Distance, i.e. the generalized p-norm of the difference.
        /// </summary>
        public static double Minkowski<T>(double p, Vector<T> a, Vector<T> b) where T : struct, IEquatable<T>, IFormattable
        {
            return (a - b).Norm(p);
        }

        /// <summary>
        /// Minkowski Distance, i.e. the generalized p-norm of the difference.
        /// </summary>
        public static double Minkowski(double p, double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            if (p < 0d) throw new ArgumentOutOfRangeException("p");
            if (p == 1d) return Manhattan(a, b);
            if (p == 2d) return Euclidean(a, b);
            if (double.IsPositiveInfinity(p)) return Chebyshev(a, b);

            double sum = 0d;
            for (var i = 0; i < a.Length; i++)
            {
                sum += Math.Pow(Math.Abs(a[i] - b[i]), p);
            }
            return Math.Pow(sum, 1.0 / p);
        }

        /// <summary>
        /// Minkowski Distance, i.e. the generalized p-norm of the difference.
        /// </summary>
        public static float Minkowski(double p, float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            if (p < 0d) throw new ArgumentOutOfRangeException("p");
            if (p == 1d) return Manhattan(a, b);
            if (p == 2d) return Euclidean(a, b);
            if (double.IsPositiveInfinity(p)) return Chebyshev(a, b);

            double sum = 0d;
            for (var i = 0; i < a.Length; i++)
            {
                sum += Math.Pow(Math.Abs(a[i] - b[i]), p);
            }
            return (float) Math.Pow(sum, 1.0/p);
        }

        /// <summary>
        /// Canberra Distance, a weighted version of the L1-norm of the difference.
        /// </summary>
        public static double Canberra(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException(Resources.ArgumentVectorsSameLength);

            double sum = 0d;
            for (var i = 0; i < a.Length; i++)
            {
                sum += Math.Abs(a[i] - b[i]) / (Math.Abs(a[i]) + Math.Abs(b[i]));
            }
            return sum;
        }

        /// <summary>
        /// Canberra Distance, a weighted version of the L1-norm of the difference.
        /// </summary>
        public static float Canberra(float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new ArgumentException(Resources.ArgumentVectorsSameLength);

            float sum = 0f;
            for (var i = 0; i < a.Length; i++)
            {
                sum += Math.Abs(a[i] - b[i]) / (Math.Abs(a[i]) + Math.Abs(b[i]));
            }
            return sum;
        }

        /// <summary>
        /// Hamming Distance, i.e. the number of positions that have different values in the vectors.
        /// </summary>
        public static double Hamming(double[] a, double[] b)
        {
            if (a.Length != b.Length) throw new ArgumentOutOfRangeException("b");
            int count = 0;
            for (int i = 1; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Hamming Distance, i.e. the number of positions that have different values in the vectors.
        /// </summary>
        public static float Hamming(float[] a, float[] b)
        {
            if (a.Length != b.Length) throw new ArgumentOutOfRangeException("b");
            int count = 0;
            for (int i = 1; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Pearson's distance, i.e. 1 - the person correlation coefficient.
        /// </summary>
        public static double Pearson(IEnumerable<double> a, IEnumerable<double> b)
        {
            return 1.0 - Correlation.Pearson(a, b);
        }
    }
}
