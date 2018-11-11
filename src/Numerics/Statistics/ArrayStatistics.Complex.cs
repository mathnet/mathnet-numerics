// <copyright file="ArrayStatistics.Complex.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2015 Math.NET
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

using Complex = System.Numerics.Complex;

namespace MathNet.Numerics.Statistics
{
    public static partial class ArrayStatistics
    {
        /// <summary>
        /// Returns the smallest absolute value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static Complex MinimumMagnitudePhase(Complex[] data)
        {
            if (data.Length == 0)
            {
                return new Complex(double.NaN, double.NaN);
            }

            double minMagnitude = double.PositiveInfinity;
            Complex min = new Complex(double.PositiveInfinity, double.PositiveInfinity);
            for (int i = 0; i < data.Length; i++)
            {
                double magnitude = data[i].Magnitude;
                if (double.IsNaN(magnitude))
                {
                    return new Complex(double.NaN, double.NaN);
                }
                if (magnitude < minMagnitude || magnitude == minMagnitude && data[i].Phase < min.Phase)
                {
                    minMagnitude = magnitude;
                    min = data[i];
                }
            }

            return min;
        }

        /// <summary>
        /// Returns the smallest absolute value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static Complex32 MinimumMagnitudePhase(Complex32[] data)
        {
            if (data.Length == 0)
            {
                return new Complex32(float.NaN, float.NaN);
            }

            float minMagnitude = float.PositiveInfinity;
            Complex32 min = new Complex32(float.PositiveInfinity, float.PositiveInfinity);
            for (int i = 0; i < data.Length; i++)
            {
                float magnitude = data[i].Magnitude;
                if (float.IsNaN(magnitude))
                {
                    return new Complex32(float.NaN, float.NaN);
                }
                if (magnitude < minMagnitude || magnitude == minMagnitude && data[i].Phase < min.Phase)
                {
                    minMagnitude = magnitude;
                    min = data[i];
                }
            }

            return min;
        }

        /// <summary>
        /// Returns the largest absolute value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static Complex MaximumMagnitudePhase(Complex[] data)
        {
            if (data.Length == 0)
            {
                return new Complex(double.NaN, double.NaN);
            }

            double maxMagnitude = 0.0d;
            Complex max = Complex.Zero;
            for (int i = 0; i < data.Length; i++)
            {
                double magnitude = data[i].Magnitude;
                if (double.IsNaN(magnitude))
                {
                    return new Complex(double.NaN, double.NaN);
                }
                if (magnitude > maxMagnitude || magnitude == maxMagnitude && data[i].Phase > max.Phase)
                {
                    maxMagnitude = magnitude;
                    max = data[i];
                }
            }

            return max;
        }

        /// <summary>
        /// Returns the largest absolute value from the unsorted data array.
        /// Returns NaN if data is empty or any entry is NaN.
        /// </summary>
        /// <param name="data">Sample array, no sorting is assumed.</param>
        public static Complex32 MaximumMagnitudePhase(Complex32[] data)
        {
            if (data.Length == 0)
            {
                return new Complex32(float.NaN, float.NaN);
            }

            float maxMagnitude = 0.0f;
            Complex32 max = Complex32.Zero;
            for (int i = 0; i < data.Length; i++)
            {
                float magnitude = data[i].Magnitude;
                if (float.IsNaN(magnitude))
                {
                    return new Complex32(float.NaN, float.NaN);
                }
                if (magnitude > maxMagnitude || magnitude == maxMagnitude && data[i].Phase > max.Phase)
                {
                    maxMagnitude = magnitude;
                    max = data[i];
                }
            }

            return max;
        }
    }
}
