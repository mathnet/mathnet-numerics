// <copyright file="BealeFunction2D.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
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

namespace MathNet.Numerics.Tests.OptimizationTests.TestFunctions
{
    public static  class BealeFunction2D
    {
        public static double Value(Vector<double> input)
        {
            double x = input[0];
            double y = input[1];

            double a = 1.5 - x + x * y;
            double b = 2.25 - x + x * y * y;
            double c = 2.625 - x + x * Math.Pow(y, 3);

            return Math.Pow(a, 2) + Math.Pow(b, 2) + Math.Pow(c, 2);
        }

        public static Vector<double> Gradient(Vector<double> input)
        {
            double x = input[0];
            double y = input[1];

            double a = 1.5 - x + x * y;
            double b = 2.25 - x + x * y * y;
            double c = 2.625 - x + x * Math.Pow(y, 3);

            Vector<double> output = new DenseVector(2);

            output[0] = 2 * (y - 1) * a + 2 * (y * y - 1) * b + 2 * (Math.Pow(y, 3) - 1) * c;
            output[1] = 2 * x * a + 4 * x * y * b + 6 * x * y * y * c;
            return output;
        }

        public static Matrix<double> Hessian(Vector<double> input)
        {
            double x = input[0];
            double y = input[1];

            double a = 1.5 - x + x * y;
            double axprime = y - 1;

            double b = 2.25 - x + x * y * y;
            double bxprime = y * y - 1;

            double c = 2.625 - x + x * Math.Pow(y, 3);
            double cxprime = Math.Pow(y, 3) - 1;


            Matrix<double> output = new DenseMatrix(2, 2);
            output[0, 0] = 2 * (Math.Pow(axprime, 2) + Math.Pow(bxprime, 2) + Math.Pow(cxprime, 2));
            output[1, 1] = 2 * Math.Pow(x, 2) + 8 * Math.Pow(x, 2) * Math.Pow(y, 2) + 18 * Math.Pow(x, 2) * Math.Pow(y, 4) + 4 * x * b + 12 * x * y * c;
            output[0, 1] = 2 * x * axprime + 2 * a + 4 * x * y * bxprime + 4 * y * b + 6 * x * Math.Pow(y, 2) * cxprime + 6 * Math.Pow(y, 2) * c;
            output[1, 0] = output[0, 1];
            return output;
        }

        public static Vector<double> Minimum
        {
            get
            {
                return new DenseVector(new double[] { 3, 0.5 });
            }
        }
    }
}

