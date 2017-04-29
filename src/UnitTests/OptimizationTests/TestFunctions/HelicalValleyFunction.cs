// <copyright file="HelicalValleyFunction.cs" company="Math.NET">
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
    public class HelicalValleyFunction : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new HelicalValleyFunction(),
                    InitialGuess = new double[] { -1, 0, 0 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 1, 0, 0 },
                    CaseName = "unbounded"
                };
                yield return new TestCase()
                {
                    Function = new HelicalValleyFunction(),
                    InitialGuess = new double[] { -1, 0, 0 },
                    MinimalValue = 0,
                    MinimizingPoint = new double[] { 1, 0, 0 },
                    LowerBound = new double[] { -1000, -1000, -1000 },
                    UpperBound = new double[] { 1000, 1000, 1000 },
                    CaseName = "loose bounds"
                };
                yield return new TestCase()
                {
                    Function = new HelicalValleyFunction(),
                    InitialGuess = new double[] { -1, 0, 0 },
                    MinimalValue = 0.99042212,
                    LowerBound = new double[] { -100, -1, -1 },
                    UpperBound = new double[] { 0.8, 1, 1 },
                    CaseName = "tight bounds"
                };
            }
        }

        public override string Description
        {
            get
            {
                return "Helical valley fun (MGH #7)";
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
                return 3;
            }
        }

        public override void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output)
        {
            switch (itemIndex)
            {
                case 0:
                    output[0] = -100 * theta10(x[0], x[1]);
                    output[1] = -100 * theta01(x[0], x[1]);
                    output[2] = 10;
                    break;
                case 1:
                    output[0] = (10 * x[0]) / Math.Sqrt(x[0]*x[0] + x[1]*x[1]);
                    output[1] = (10 * x[1]) / Math.Sqrt(x[0]*x[0] + x[1]*x[1]);
                    output[2] = 0;
                    break;
                case 2:
                    output[0] = 0;
                    output[1] = 0;
                    output[2] = 1;
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
                    output[0, 0] = -100 * theta20(x[0], x[1]);
                    output[0, 1] = -100 * theta11(x[0], x[1]);
                    output[0, 2] = 0;
                    output[1, 0] = -100 * theta11(x[0], x[1]);
                    output[1, 1] = -100 * theta02(x[0], x[1]);
                    output[1, 2] = 0;
                    output[2, 0] = 0;
                    output[2, 1] = 0;
                    output[2, 2] = 0;
                    break;
                case 1:
                    output[0, 0] = (10 * x[1]*x[1]) / Math.Pow(x[0]*x[0] + x[1]*x[1],1.5);
                    output[0, 1] = (-10 * x[0] * x[1]) / Math.Pow(x[0]*x[0] + x[1]*x[1],1.5);
                    output[0, 2] = 0;
                    output[1, 0] = (-10 * x[0] * x[1]) / Math.Pow(x[0] * x[0] + x[1] * x[1], 1.5);
                    output[1, 1] = (10 * x[0]*x[0]) / Math.Pow(x[0] * x[0] + x[1] * x[1], 1.5);
                    output[1, 2] = 0;
                    output[2, 0] = 0;
                    output[2, 1] = 0;
                    output[2, 2] = 0;
                    break;
                case 2:
                    for (int ii = 0; ii < 2; ++ii)
                        for (int jj = 0; jj < 2; ++jj)
                            output[ii, jj] = 0;
                    break;
                default:
                    throw new ArgumentException("itemIndex must be <= 2");
            }
        }

        private static double theta(double x1, double x2)
        {
            if (x1 >= 0)
                return 0.5 * Math.Atan(x2 / x1) / Math.PI;
            else
                return 0.5 * Math.Atan(x2 / x1) / Math.PI + 0.5;
        }

        private static double theta10(double x1, double x2)
        {
            return -(x2 / (2 * Math.PI * Math.Pow(x1,2) + 2 * Math.PI * Math.Pow(x2,2)));
        }

        private static double theta01(double x1, double x2)
        {
            return x1 / (2 * Math.PI * x1*x1 + 2 * Math.PI * x2*x2);
        }

        private static double theta20(double x1,double x2)
        {
            return (x1 * x2) / (Math.PI * Math.Pow(x1 * x1 + x2 * x2, 2));
        }

        private static double theta11(double x1, double x2)
        {
            return (-x1 * x1 + x2 * x2) / (2 * Math.PI * Math.Pow(x1 * x1 + x2 * x2, 2));
        }

        private static double theta02(double x1, double x2)
        {
            return -((x1 * x2) / (Math.PI * Math.Pow(x1*x1 + x2*x2, 2)));
        }
        
        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            switch (itemIndex)
            {
                case 0:
                    return 10 * (x[2] - 10 * theta(x[0], x[1]));
                case 1:
                    return 10 * (Math.Sqrt(x[0] * x[0] + x[1] * x[1]) - 1);
                case 2:
                    return x[2];
                default:
                    throw new ArgumentException("itemIndex must be <= 2");
            }
        }
    }
}
