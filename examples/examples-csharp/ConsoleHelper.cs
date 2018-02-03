// <copyright file="ConsoleHelper.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
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
using MathNet.Numerics.Statistics;

namespace Examples
{
    /// <summary>
    /// Helper functions to output into Console window
    /// </summary>
    public static class ConsoleHelper
    {
        /// <summary>
        /// Display histogram from the array
        /// </summary>
        /// <param name="data">Source array</param>
        public static void DisplayHistogram(IEnumerable<double> data)
        {
            var blockSymbol = Convert.ToChar(9608);

            var rowMaxLength = Console.WindowWidth - 1;
            rowMaxLength = (rowMaxLength / 10) * 10;
            var rowCount = rowMaxLength / 3;

            var histogram = new Histogram(data, rowMaxLength);

            // Find the absolute peak
            var maxBucketCount = 0.0;
            for (var i = 0; i < histogram.BucketCount; i++)
            {
                if (histogram[i].Count > maxBucketCount)
                {
                    maxBucketCount = histogram[i].Count;
                }
            }

            // Number of bucket counts between rows
            var rowStep = maxBucketCount / rowCount;

            // Draw histogram line-by-line
            Console.WriteLine();

            for (var row = 0; row < rowCount; row++)
            {
                for (var col = 0; col < histogram.BucketCount; col++)
                {
                    if (histogram[col].Count >= maxBucketCount)
                    {
                        Console.Write(blockSymbol);
                    }
                    else
                    {
                        Console.Write(@" ");
                    }
                }

                Console.SetCursorPosition(0, Console.CursorTop + 1);
                maxBucketCount -= rowStep;
            }

            // Calculate distanse between label in X axis
            var axisStep = histogram.BucketCount / 2;

            var leftLabel = histogram.LowerBound.ToString("N");
            var middleLabel = ((histogram.UpperBound + histogram.LowerBound) / 2.0).ToString("N");
            var rightLabel = histogram.UpperBound.ToString("N");

            Console.Write(leftLabel);
            for (var j = 0; j < axisStep - leftLabel.Length; j++)
            {
                Console.Write(@" ");
            }

            Console.Write(middleLabel);
            for (var j = 0; j < axisStep - middleLabel.Length; j++)
            {
                Console.Write(@" ");
            }

            Console.Write(rightLabel);

            Console.WriteLine();
        }

        /// <summary>
        /// Display histogram from the array
        /// </summary>
        /// <param name="data">Source array</param>
        public static void DisplayHistogram(IEnumerable<int> data)
        {
            DisplayHistogram(data.Select(x => (double)x));
        }
    }
}
