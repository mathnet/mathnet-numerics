// <copyright file="TestFunctions.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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
using System.Linq;

// ReSharper disable once CheckNamespace
namespace MathNet.Numerics
{
    public static class TestFunctions
    {
        /// <summary>
        /// Valley-shaped Rosenbrock function for 2 dimensions: (x,y) -> (1-x)^2 + 100*(y-x^2)^2.
        /// This function has a global minimum at (1,1) with f(1,1) = 0.
        /// Common range: [-5,10] or [-2.048,2.048].
        /// </summary>
        /// <remarks>
        /// https://en.wikipedia.org/wiki/Rosenbrock_function
        /// http://www.sfu.ca/~ssurjano/rosen.html
        /// </remarks>
        public static double Rosenbrock(double x, double y)
        {
            double a = 1.0 - x;
            double b = y - x*x;
            return a*a + 100*b*b;
        }

        /// <summary>
        /// Valley-shaped Rosenbrock function for 2 or more dimensions.
        /// This function have a global minimum of all ones and, for 8 > N > 3, a local minimum at (-1,1,...,1).
        /// </summary>
        /// <remarks>
        /// https://en.wikipedia.org/wiki/Rosenbrock_function
        /// http://www.sfu.ca/~ssurjano/rosen.html
        /// </remarks>
        public static double Rosenbrock(params double[] x)
        {
            double sum = 0;
            for (int i = 1; i < x.Length; i++)
            {
                sum += Rosenbrock(x[i - 1], x[i]);
            }

            return sum;
        }

        /// <summary>
        /// Himmelblau, a multi-modal function: (x,y) -> (x^2+y-11)^2 + (x+y^2-7)^2
        /// This function has 4 global minima with f(x,y) = 0.
        /// Common range: [-6,6].
        /// Named after David Mautner Himmelblau
        /// </summary>
        /// <remarks>
        /// https://en.wikipedia.org/wiki/Himmelblau%27s_function
        /// </remarks>
        public static double Himmelblau(double x, double y)
        {
            double a = x*x + y - 11.0;
            double b = x + y*y - 7.0;
            return a*a + b*b;
        }

        /// <summary>
        /// Rastrigin, a highly multi-modal function with many local minima.
        /// Global minimum of all zeros with f(0) = 0.
        /// Common range: [-5.12,5.12].
        /// </summary>
        /// <remarks>
        /// https://en.wikipedia.org/wiki/Rastrigin_function
        /// http://www.sfu.ca/~ssurjano/rastr.html
        /// </remarks>
        public static double Rastrigin(params double[] x)
        {
            return x.Sum(xi => xi*xi - 10.0*Math.Cos(Constants.Pi2*xi)) + 10.0*x.Length;
        }

        /// <summary>
        /// Drop-Wave, a multi-modal and highly complex function with many local minima.
        /// Global minimum of all zeros with f(0) = -1.
        /// Common range: [-5.12,5.12].
        /// </summary>
        /// <remarks>
        /// http://www.sfu.ca/~ssurjano/drop.html
        /// </remarks>
        public static double DropWave(double x, double y)
        {
            double t = x*x + y*y;
            return -(1.0 + Math.Cos(12.0*Math.Sqrt(t)))/(0.5*t + 2.0);
        }

        /// <summary>
        /// Ackley, a function with many local minima. It is nearly flat in outer regions but has a large hole at the center.
        /// Global minimum of all zeros with f(0) = 0.
        /// Common range: [-32.768, 32.768].
        /// </summary>
        /// <remarks>
        /// http://www.sfu.ca/~ssurjano/ackley.html
        /// </remarks>
        public static double Ackley(params double[] x)
        {
            double u = x.Sum(xi => xi*xi)/x.Length;
            double v = x.Sum(xi => Math.Cos(Constants.Pi2*xi))/x.Length;
            return -20*Math.Exp(-0.2*Math.Sqrt(u)) - Math.Exp(v) + 20 + Math.E;
        }

        /// <summary>
        /// Bowl-shaped first Bohachevsky function.
        /// Global minimum of all zeros with f(0,0) = 0.
        /// Common range: [-100, 100]
        /// </summary>
        /// <remarks>
        /// http://www.sfu.ca/~ssurjano/boha.html
        /// </remarks>
        public static double Bohachevsky1(double x, double y)
        {
            return x*x + 2*y*y - 0.3*Math.Cos(3*Math.PI*x) - 0.4*Math.Cos(4*Math.PI*y);
        }

        /// <summary>
        /// Plate-shaped Matyas function.
        /// Global minimum of all zeros with f(0,0) = 0.
        /// Common range: [-10, 10].
        /// </summary>
        /// <remarks>
        /// http://www.sfu.ca/~ssurjano/matya.html
        /// </remarks>
        public static double Matyas(double x, double y)
        {
            return 0.26*(x*x + y*y) - 0.48*x*y;
        }

        /// <summary>
        /// Valley-shaped six-hump camel back function.
        /// Two global minima and four local minima. Global minima with f(x) ) -1.0316 at (0.0898,-0.7126) and (-0.0898,0.7126).
        /// Common range: x in [-3,3], y in [-2,2].
        /// </summary>
        /// <remarks>
        /// http://www.sfu.ca/~ssurjano/camel6.html
        /// </remarks>
        public static double SixHumpCamel(double x, double y)
        {
            double x2 = x*x;
            double y2 = y*y;
            return (4 - 2.1*x2 + x2*x2/3)*x2 + x*y + (-4 + 4*y2)*y2;
        }
    }
}
