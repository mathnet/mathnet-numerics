// <copyright file="JennrichAndSampsonFunction.cs" company="Math.NET">
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
    public class JennrichAndSampsonFunction : BaseTestFunction
    {
        private readonly int _m;

        public JennrichAndSampsonFunction(int itemDimension)
        {
            if (itemDimension < 2)
                throw new ArgumentException("itemDimension must be at least 2.");
            _m = itemDimension;
        }

        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                yield return new TestCase()
                {
                    Function = new JennrichAndSampsonFunction(10),
                    InitialGuess = new double[] { 0.3, 0.4 },
                    MinimalValue = 124.362,
                    MinimizingPoint = new double[] { 0.2578, 0.2578 },
                    CaseName = "unbounded"
                };
                //yield return new TestCase()
                //{
                //    Function = new JennrichAndSampsonFunction(10),
                //    LowerBound = new double[] { 0.6, 0.5 },
                //    UpperBound = new double[] { 10, 50 },
                //    StartPoint = new double[] { 1.0, 1.0 },
                //    MinimizingInput = null,
                //    MinimizingValue = 0,
                //    CaseName = "tight bounds"
                //};
                yield return new TestCase()
                {
                    Function = new JennrichAndSampsonFunction(10),
                    LowerBound = new double[] { -50, -50 },
                    UpperBound = new double[] { 50, 50 },
                    InitialGuess = new double[] { 0.3, 0.4 },
                    MinimizingPoint = null,
                    MinimalValue = 0,
                    CaseName = "loose bounds"
                };
            }
        }

        public override string Description
        {
            get
            {
                return $"Jennrich & Sampson fun (MGH #6) (n={this.ItemDimension})";
            }
        }

        public override int ItemDimension
        {
            get
            {
                return _m;
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
            int ii = itemIndex + 1;
            output[0] = -(Math.Exp(ii * x[0]) * ii);
            output[1] = -(Math.Exp(ii * x[1]) * ii);
        }

        public override void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output)
        {
            int ii = itemIndex + 1;
            output[0, 0] = -(Math.Exp(ii * x[0]) * ii*ii);
            output[0, 1] = 0;
            output[1, 0] = 0;
            output[1, 1] = -(Math.Exp(ii * x[1]) * ii*ii);
        }

        public override double ItemValue(Vector<double> x, int itemIndex)
        {
            int ii = itemIndex + 1;
            return 2 + 2 * ii - (Math.Exp(ii * x[0]) + Math.Exp(ii * x[1]));
        }
    }
}
