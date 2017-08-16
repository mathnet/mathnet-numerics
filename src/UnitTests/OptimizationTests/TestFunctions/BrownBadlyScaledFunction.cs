// <copyright file="BrownBadlyScaledFunction.cs" company="Math.NET">
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
    public class BrownBadlyScaledFunction : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new BrownBadlyScaledFunction(),
                    InitialGuess = new double[] { 1, 1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 1e6, 2e-6 },
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new BrownBadlyScaledFunction(),
                    InitialGuess = new double[] { 1, 1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 1e6, 2e-6 },
                    LowerBound = new double[] { -1e8, -1e8 },
                    UpperBound = new double[] { 1e8, 1e8 },
                    CaseName = "loose bounds"
                };
                yield return new TestCase()
                {
                    Function = new BrownBadlyScaledFunction(),
                    InitialGuess = new double[] { 1, 1 },
                    MinimalValue = 0.784e3,
                    MinimizingPoint = new double[] { 1e6, 2e-6 },
                    LowerBound = new double[] { 0, 3e-5 },
                    UpperBound = new double[] { 1e6, 100 },
                    CaseName = "tight bounds"                 
                };
            }
        }

        public BrownBadlyScaledFunction() { }

        public override string Description
        {
            get
            {
                return "Brown badly scaled fun (MGH #4)";
            }
        }

        public override int ItemDimension
        {
            get
            {
                return 3;
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
            switch (itemIndex)
            {
                case 0:
                    output[0] = 1;
                    output[1] = 0;
                    break;
                case 1:
                    output[0] = 0;
                    output[1] = 1;
                    break;
                case 2:
                    output[0] = x[1];
                    output[1] = x[0];
                    break;
                default:
                    throw new ArgumentException("itemIndex must be <= 2");
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            switch (itemIndex)
            {
                case 0:
                case 1:
                    output[0, 0] = 0;
                    output[0, 1] = 0;
                    output[1, 0] = 0;
                    output[1, 1] = 0;
                    break;
                case 2:
                    output[0, 0] = 0;
                    output[0, 1] = 1;
                    output[1, 0] = 1;
                    output[1, 1] = 0;
                    break;
                default:
                    throw new ArgumentException("itemIndex must be <= 2");
            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            switch (itemIndex)
            {
                case 0:
                    return x[0] - 1e6;
                case 1:
                    return x[1] - 2e-6;
                case 2:
                    return x[0] * x[1] - 2;
                default:
                    throw new ArgumentException("itemIndex must be <= 2");
            }
        }
    }
}
