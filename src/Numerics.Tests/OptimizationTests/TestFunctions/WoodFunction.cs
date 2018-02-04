// <copyright file="WoodFunction.cs" company="Math.NET">
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
    public class WoodFunction : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new WoodFunction(),
                    InitialGuess = new double[] { -3, -1, -3, -1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 1, 1, 1, 1 },
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new WoodFunction(),
                    InitialGuess = new double[] { -3, -1, -3, -1 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 1, 1, 1, 1 },
                    LowerBound = new double[] { -1000, -1000, -1000, -1000 },
                    UpperBound = new double[] { 1000, 1000, 1000, 1000 },
                    CaseName = "loose bounds"
                };
                yield return new TestCase()
                {
                    Function = new WoodFunction(),
                    InitialGuess = new double[] { -3, -1, -3, -1 },
                    MinimalValue = 1.5567008,
                    MinimizingPoint = null,
                    LowerBound = new double[] { -100, -100, -100, -100 },
                    UpperBound = new double[] { 0, 10, 100, 100 },
                    CaseName = "tight bounds"
                };
            }
        }

        public WoodFunction() { }

        public override string Description
        {
            get
            {
                return "Wood fun (MGH #14)";
            }
        }

        public override int ItemDimension
        {
            get
            {
                return 6;
            }
        }

        public override int ParameterDimension
        {
            get
            {
                return 4;
            }
        }

        public override void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output)
        {
            switch (itemIndex)
            {
                case 0:
                    output[0] = -20 * x[0];
                    output[1] = 10;
                    output[2] = 0;
                    output[3] = 0;
                    break;
                case 1:
                    output[0] = -1;
                    output[1] = 0;
                    output[2] = 0;
                    output[3] = 0;
                    break;
                case 2:
                    output[0] = 0;
                    output[1] = 0;
                    output[2] = -6 * Math.Sqrt(10) * x[2];
                    output[3] = 3 * Math.Sqrt(10);
                    break;
                case 3:
                    output[0] = 0;
                    output[1] = 0;
                    output[2] = -1;
                    output[3] = 0;
                    break;
                case 4:
                    output[0] = 0;
                    output[1] = Math.Sqrt(10);
                    output[2] = 0;
                    output[3] = Math.Sqrt(10);
                    break;
                case 5:
                    output[0] = 0;
                    output[1] = 1.0 / Math.Sqrt(10);
                    output[2] = 0;
                    output[3] = -1.0 / Math.Sqrt(10);
                    break;
                default:
                    throw new ArgumentException("itemIndex must be <= 5");
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            for (int ii = 0; ii < 4; ++ii)
                for (int jj = 0; jj < 4; ++jj)
                    output[ii, jj] = 0;
            switch (itemIndex)
            {
                case 0:
                    output[0, 0] = -20;
                    break;
                case 1:
                    break;
                case 2:
                    output[2, 2] = -6 * Math.Sqrt(10);
                    break;
                case 3:
                case 4:
                case 5:
                    break;
                default:
                    throw new ArgumentException("itemIndex must be <= 5");

            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            switch (itemIndex)
            {
                case 0:
                    return 10 * (x[1] - x[0] * x[0]);
                case 1:
                    return 1 - x[0];
                case 2:
                    return Math.Sqrt(90) * (x[3] - x[2] * x[2]);
                case 3:
                    return 1 - x[2];
                case 4:
                    return Math.Sqrt(10) * (x[1] + x[3] - 2);
                case 5:
                    return (x[1] - x[3]) / Math.Sqrt(10);
                default:
                    throw new ArgumentException("itemIndex must be <= 5");
            }
        }
    }
}
