// <copyright file="RosenbrockFunction2.cs" company="Math.NET">
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
    public class RosenbrockFunction2 : BaseTestFunction
    {
        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { -1.2, 1 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { -1000, -1000 },
                    UpperBound = new double[] { 1000, 1000 },
                    CaseName = "hard start",
                    IsUnboundedOverride = true
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { 1.2, 1.2 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { -5, -5 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "easy start"
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { -0.9, -0.5 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { -5, -5 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "Overton start",
                    IsUnboundedOverride = true
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { 1.2, 1.2 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { 1, -5 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "easy one active bound"
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { 1.2, 1.2 },
                    MinimizingPoint = new double[] { 1, 1 },
                    MinimalValue = 0,
                    LowerBound = new double[] { 1, 1 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "easy two active bounds"
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { 2.5, 2.5 },
                    MinimizingPoint = new double[] { 2, 4 },
                    MinimalValue = 1,
                    LowerBound = new double[] { 2, 2 },
                    UpperBound = new double[] { 5, 5 },
                    CaseName = "min on lower bound, not local"
                };
                yield return new TestCase()
                {
                    Function = new RosenbrockFunction2(),
                    InitialGuess = new double[] { -0.9, -0.5 },
                    MinimizingPoint = new double[] { 0.5, 0.25 },
                    MinimalValue = 0.25,
                    LowerBound = new double[] { -2, -2 },
                    UpperBound = new double[] { 0.5, 0.5 },
                    CaseName = "min on upper bound, not local"
                };


            }
        }
        public RosenbrockFunction2() { }

        public override string Description
        {
            get
            {
                return "Rosenbrock fun (MGH #1)";
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
                output[0] = -20 * x[0];
                output[1] = 10;
            } else
            {
                output[0] = -1;
                output[1] = 0;
            }
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            if (itemIndex == 0)
            {
                output[0, 0] = -20;
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1, 1] = 0;
            } else
            {
                output[0, 0] = 0;
                output[0, 1] = 0;
                output[1, 0] = 0;
                output[1, 1] = 0;
            }
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            if (itemIndex == 0)
                return 10 * (x[1] - x[0] * x[0]);
            else
                return 1 - x[0];
        }
    }
}
