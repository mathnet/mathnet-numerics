// <copyright file="AdamsBashforth.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

namespace MathNet.Numerics.OdeSolvers
{
    public static class AdamsBashforth
    {
        /// <summary>
        /// First Order AB method(same as Forward Euler)
        /// </summary>
        /// <param name="y0">Initial value</param>
        /// <param name="start">Start Time</param>
        /// <param name="end">End Time</param>
        /// <param name="N">Number of subintervals</param>
        /// <param name="f">ode model</param>
        /// <returns></returns>
        public static double[] FirstOrder(double y0, double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double t = start;
            double[] y = new double[N];
            y[0] = y0;
            for (int i = 1; i < N; i++)
            {
                y[i] = y0 + dt * f(t, y0);
                t += dt;
                y0 = y[i];
            }
            return y;
        }
        /// <summary>
        /// Second Order AB Method(Require two initial guesses)
        /// </summary>
        /// <param name="y0">Initial value 1</param>
        /// <param name="y1">Initial value 2</param>
        /// <param name="start">Start Time</param>
        /// <param name="end">End Time</param>
        /// <param name="N">Number of subintervals</param>
        /// <param name="f">ode model</param>
        /// <returns></returns>
        public static double[] SecondOrder(double y0,  double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double t = start;
            double[] y = new double[N];
            double k1 = f(t, y0);
            double k2 = f(t + dt, y0 + dt * k1);
            double y1 = y0 + 0.5 * dt * (k1 + k2);
            y[0] = y0;
            t += dt;
            y[1] = y1;
            for (int i = 2; i < N; i++)
            {
                y[i] = y1 + dt * (1.5 * f(t + dt, y1) - 0.5 * f(t, y0));
                t += dt;
                y0 = y[i-1];
                y1 = y[i];
            }
            return y;
        }

        public static double[] ThirdOrder()
        {
            return null;
        }

        public static double[] FourthOrder()
        {
            return null;
        }
    }
}