// <copyright file="RosenbrockFunction.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public static class RosenbrockFunction
    {
        public static double Value(Vector<double> input)
        {
            return Math.Pow((1 - input[0]), 2) + 100 * Math.Pow((input[1] - input[0] * input[0]), 2);
        }

        public static Vector<double> Gradient(Vector<double> input)
        {
            Vector<double> output = new DenseVector(2);
            output[0] = -2 * (1 - input[0]) + 200 * (input[1] - input[0] * input[0]) * (-2 * input[0]);
            output[1] = 2 * 100 * (input[1] - input[0] * input[0]);
            return output;
        }

        public static Matrix<double> Hessian(Vector<double> input)
        {
            Matrix<double> output = new DenseMatrix(2, 2);
            output[0, 0] = 2 - 400 * input[1] + 1200 * input[0] * input[0];
            output[1, 1] = 200;
            output[0, 1] = -400 * input[0];
            output[1, 0] = output[0, 1];
            return output;
        }

        public static Vector<double> Minimum
        {
            get
            {
                return new DenseVector(new double[] { 1, 1 });
            }
        }
    }

    public static class BigRosenbrockFunction
    {
        public static double Value(Vector<double> input)
        {
            return 1000.0 + 100.0 * RosenbrockFunction.Value(input / 100.0);
        }

        public static Vector<double> Gradient(Vector<double> input)
        {
            return 100.0 * RosenbrockFunction.Gradient(input / 100.0);
        }

        public static Matrix<double> Hessian(Vector<double> input)
        {
            return 100.0 * RosenbrockFunction.Hessian(input / 100.0);
        }

        public static Vector<double> Minimum
        {
            get
            {
                return new DenseVector(new double[] { 100, 100 });
            }
        }
    }
}
