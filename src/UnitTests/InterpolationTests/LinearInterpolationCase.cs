// <copyright file="LinearInterpolationCase.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.UnitTests.InterpolationTests
{
    /// <summary>
    /// LinearInterpolation test case.
    /// </summary>
    internal static class LinearInterpolationCase
    {
        /// <summary>
        /// Build linear samples.
        /// </summary>
        /// <param name="x">X sample values.</param>
        /// <param name="y">Y samples values.</param>
        /// <param name="xtest">X test values.</param>
        /// <param name="ytest">Y test values.</param>
        /// <param name="samples">Sample values.</param>
        /// <param name="sampleOffset">Sample offset.</param>
        /// <param name="slope">Slope number.</param>
        /// <param name="intercept">Intercept criteria.</param>
        public static void Build(out double[] x, out double[] y, out double[] xtest, out double[] ytest, int samples = 3, double sampleOffset = -0.5, double slope = 2.0, double intercept = -1.0)
        {
            // Fixed-seed "random" distribution to ensure we always test with the same data
            var uniform = new ContinuousUniform(0.0, 1.0, new SystemRandomSource(42));

            // build linear samples
            x = new double[samples];
            y = new double[samples];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = i + sampleOffset;
                y[i] = (x[i]*slope) + intercept;
            }

            // build linear test vectors randomly between the sample points
            xtest = new double[samples + 1];
            ytest = new double[samples + 1];
            if (samples == 1)
            {
                // y = const
                xtest[0] = sampleOffset - uniform.Sample();
                xtest[1] = sampleOffset + uniform.Sample();
                ytest[0] = ytest[1] = (sampleOffset*slope) + intercept;
            }
            else
            {
                for (int i = 0; i < xtest.Length; i++)
                {
                    xtest[i] = (i - 1) + sampleOffset + uniform.Sample();
                    ytest[i] = (xtest[i]*slope) + intercept;
                }
            }
        }
    }
}
