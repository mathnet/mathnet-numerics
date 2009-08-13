// <copyright file="SampleProvider.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.InterpolationTests
{
    using System;

    internal static class SampleProvider
    {
        internal static double[] LinearEquidistant(int count, double start, double step)
        {
            var samples = new double[count];
            var nextValue = start;

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = nextValue;
                nextValue += step;
            }

            return samples;
        }

        internal static void Equidistant(
            Func<double, double> f,
            double start,
            double stop,
            int samples,
            out double[] points,
            out double[] values)
        {
            points = new double[samples];
            values = new double[samples];

            if (samples == 0)
            {
                return;
            }

            if (samples == 1)
            {
                double t = points[0] = 0.5 * (start + stop);
                values[0] = f(t);
                return;
            }

            double step = (stop - start) / (samples - 1);
            for (int i = 0; i < points.Length; i++)
            {
                double t = start + (i * step);
                points[i] = t;
                values[i] = f(t);
            }
        }
    }
}
