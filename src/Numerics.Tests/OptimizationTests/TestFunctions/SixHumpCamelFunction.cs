// <copyright file="SixHumpCamelFunction.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.Tests.OptimizationTests.TestFunctions
{
    /// <summary>
    /// Six-Hump Camel Function, see http://www.sfu.ca/~ssurjano/camel6.html for formula and global minimum locations
    /// </summary>
    public static class SixHumpCamelFunction
    {
        public static double Value(Vector<double> input)
        {
            double x = input[0];
            double y = input[1];

            double x2 = x * x;
            double x4 = x * x * x * x;
            double y2 = y * y;

            return x2 * (4 - 2.1 * x2 + x4 / 3) + x * y + 4 * y2 * (y2 - 1);
        }

        public static Vector<double> Gradient(Vector<double> input)
        {
            double x = input[0];
            double y = input[1];

            double x3 = x * x * x;
            double x5 = x * x * x * x * x;
            double y2 = y * y;
            double y3 = y * y * y;           

            Vector<double> output = new DenseVector(2);

            output[0] = 2 * (4 * x - 4.2 * x3 + x5 + 0.5 * y);
            output[1] = x - 8 * y + 16 * y3;
            return output;
        }

        public static Matrix<double> Hessian(Vector<double> input)
        {
            double x = input[0];
            double y = input[1];

            double x2 = x * x;
            double x4 = x * x * x * x;
            double y2 = y * y;


            Matrix<double> output = new DenseMatrix(2, 2);
            output[0, 0] = 10 * (0.8 - 2.52 * x2 + x4);
            output[1, 1] = 48 * y2 - 8;
            output[0, 1] = 1;
            output[1, 0] = output[0, 1];
            return output;
        }

        public static Vector<double> Minimum
        {
            get
            {
                return new DenseVector(new double[] { 0.0898, -0.7126 });
            }
        }
    }
}
