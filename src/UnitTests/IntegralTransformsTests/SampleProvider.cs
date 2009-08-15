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

namespace MathNet.Numerics.UnitTests.IntegralTransformsTests
{
    using System;

    internal static class SampleProvider
    {
        private static readonly Random _random = new Random();

        internal static Complex[] ProvideComplexSamples(int count)
        {
            var samples = new Complex[count];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = Complex.WithRealImaginary(
                    1 - (2 * _random.NextDouble()),
                    1 - (2 * _random.NextDouble()));
            }

            return samples;
        }

        internal static double[] ProvideRealSamples(int count)
        {
            var samples = new double[count];
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = 1 - (2 * _random.NextDouble());
            }

            return samples;
        }
    }
}