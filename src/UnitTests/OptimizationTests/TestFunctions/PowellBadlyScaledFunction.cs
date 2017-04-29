// <copyright file="PowellBadlyScaledFunction.cs" company="Math.NET">
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
    public class PowellBadlyScaledFunction : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new PowellBadlyScaledFunction(),
                    InitialGuess = new double[] { 0, 1 },
                    MinimizingPoint = new double[] { 1.098e-5, 9.106 },
                    MinimalValue = 0,
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new PowellBadlyScaledFunction(),
                    InitialGuess = new double[] { 0, 1 },
                    MinimizingPoint = new double[] { 1.098e-5, 9.106 },
                    MinimalValue = 0,
                    LowerBound = new double[] { -1000, -1000 },
                    UpperBound = new double[] { 1000, 1000 },
                    CaseName = "loose bounds"
                };
                yield return new TestCase()
                {
                    Function = new PowellBadlyScaledFunction(),
                    LowerBound = new double[] { 0, 1 },
                    UpperBound = new double[] { 1, 9 },
                    InitialGuess = new double[] { 0, 1 },
                    MinimalValue = 0.15125900e-9,
                    CaseName = "tight bounds"
                };
            }
        }

        public override string Description
        {
            get
            {
                return "Powell badly scaled fun (MGH #3)";
            }
        }

        public override int ItemDimension
        {
            get
            {
                return 2;
            }
        }

        public override int ParameterDimension
        {
            get
            {
                return 2;
            }
        }

        public override void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output)
        {
            if (itemIndex == 0)
            {
                output[0] = 10000 * x[1];
                output[1] = 10000 * x[0];
            }
            else if (itemIndex == 1)
            {
                output[0] = -Math.Exp(-x[0]);
                output[1] = -Math.Exp(-x[1]);
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            if (itemIndex == 0)
            {
                output[0, 0] = 0;
                output[0, 1] = 10000;
                output[1, 0] = 10000;
                output[1, 1] = 0;
            }
            else
            {
                output[0, 0] = Math.Exp(-x[0]);
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1,1] = Math.Exp(-x[1]);
            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            if (itemIndex == 0)
                return 10000.0 * x[0] * x[1] - 1;
            else
                return Math.Exp(-x[0]) + Math.Exp(-x[1]) - 1.0001;
        }
    }
}
