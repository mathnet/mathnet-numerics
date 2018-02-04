// <copyright file="MeyerFunction.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class MeyerFunction : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new MeyerFunction(),
                    InitialGuess = new double[] { 0.02, 4000, 250 },
                    MinimalValue = 87.9458,
                    MinimizingPoint = null,
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new MeyerFunction(),
                    InitialGuess = new double[] { 0.02, 4000, 250 },
                    MinimalValue = 87.9458,
                    MinimizingPoint = null,
                    LowerBound = new double[] { -1e6, -1e6, -1e6 },
                    UpperBound = new double[] { 1e6, 1e6, 1e6 },
                    CaseName = "loose bounds"
                };
            }
        }

        public MeyerFunction() { }

        public override string Description
        {
            get
            {
                return "Meyer fun (MGH #10)";
            }
        }

        public override int ItemDimension
        {
            get
            {
                return 16;
            }
        }

        public override int ParameterDimension
        {
            get
            {
                return 3;
            }
        }

        public override void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output)
        {
            int ii = itemIndex + 1;
            output[0] = Math.Exp(x[1] / (45.0 + 5 * ii + x[2]));
            output[1] = (Math.Exp(x[1] / (45.0 + 5 * ii + x[2])) * x[0]) / (45 + 5 * ii + x[2]);
            output[2] = -(Math.Exp(x[1] / (45.0 + 5 * ii + x[2])) * x[0] * x[1]) / Math.Pow(45 + 5 * ii + x[2], 2);

        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            var ii = itemIndex + 1;

            var t0 = (45.0 + 5 * ii + x[2]);
            var t1 = Math.Exp(x[1] / t0);

            output[0, 0] = 0;
            output[0, 1] = t1 / t0;
            output[0, 2] = -t1 * x[1] / Math.Pow(t0, 2);
            output[1, 0] = t1 / t0;
            output[1, 1] = t1 * x[0] / Math.Pow(t0, 2);
            output[1, 2] = -t1 * x[0] * (t0 + x[1]) / Math.Pow(t0, 3);
            output[2, 0] = -t1 * x[1] / Math.Pow(t0, 2);
            output[2, 1] = -t1 * x[0] * (t0 + x[1]) / Math.Pow(t0, 3);
            output[2, 2] = t1 * x[0] * x[1] * (2*t0 + x[1]) / Math.Pow(t0, 4);
        }

        private static readonly double[] y = { 34780, 28610, 23650, 19630, 16370, 13720, 11540, 9744, 8261, 7030, 6005, 5147, 4427, 3820, 3307, 2872 };

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            var ii = itemIndex + 1;
            var t = 45.0 + 5 * ii;
            return x[0] * Math.Exp(x[1] / (t + x[2])) - y[itemIndex];
        }
    }
}
