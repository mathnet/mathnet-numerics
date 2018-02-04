// <copyright file="FreudensteinAndRothFunction.cs" company="Math.NET">
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

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class FreudensteinAndRothFunction : BaseTestFunction
    {

        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new FreudensteinAndRothFunction(),
                    InitialGuess = new double[] { 0.5, -2 },
                    MinimizingPoint = new double[] { 5, 4 },
                    MinimalValue = 0,
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new FreudensteinAndRothFunction(),
                    InitialGuess = new double[] { 0.5, -2 },
                    MinimizingPoint = new double[] {5, 4},
                    MinimalValue = 0,
                    LowerBound = new double[] { -1000, -1000 },
                    UpperBound = new double[] { 1000, 1000},
                    CaseName = "loose bounds"
                };
            }
        }

        public override string Description {  get { return "Freudenstein & Roth fun (MGH #2)"; } }

        public override int ParameterDimension
        {
            get
            {
                return 2;
            }
        }

        public override int ItemDimension
        {
            get
            {
                return 2;
            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            if (itemIndex == 0)
                return -13 + x[0] + ((5 - x[1]) * x[1] - 2) * x[1];
            else 
                return -29 + x[0] + ((x[1] + 1) * x[1] - 14) * x[1];
        }

        public override void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output)
        {
            if (itemIndex == 0)
            {
                output[0] = 1;
                output[1] = -2 + (5 - 2 * x[1]) * x[1] + (5 - x[1]) * x[1];
            }
            else
            {
                output[0] = 1;
                output[1] = -14 + x[1] * (1 + x[1]) + x[1] * (1 + 2 * x[1]);
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            if (itemIndex == 0)
            {
                output[0, 0] = 0;
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1, 1] = 10 - 6 * x[1];
            }
            else
            {
                output[0, 0] = 0;
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1, 1] = 2 + 6 * x[1];
            }
        }
    }
}
