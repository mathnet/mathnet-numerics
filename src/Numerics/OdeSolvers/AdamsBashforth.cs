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
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode model</param>
        /// <returns>approximation with size N</returns>
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
        /// Second Order AB Method
        /// </summary>
        /// <param name="y0">Initial value 1</param>
        /// <param name="start">Start Time</param>
        /// <param name="end">End Time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode model</param>
        /// <returns>approximation with size N</returns>
        public static double[] SecondOrder(double y0, double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double t = start;
            double[] y = new double[N];

            double k1 = f(t, y0);
            double k2 = f(t + dt, y0 + dt * k1);
            double y1 = y0 + 0.5 * dt * (k1 + k2);

            y[0] = y0;
            y[1] = y1;
            for (int i = 2; i < N; i++)
            {
                y[i] = y1 + dt * (1.5 * f(t + dt, y1) - 0.5 * f(t, y0));
                t += dt;
                y0 = y[i - 1];
                y1 = y[i];
            }
            return y;
        }

        /// <summary>
        /// Third Order AB Method
        /// </summary>
        /// <param name="y0">Initial value 1</param>
        /// <param name="start">Start Time</param>
        /// <param name="end">End Time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode model</param>
        /// <returns>approximation with size N</returns>
        public static double[] ThirdOrder(double y0, double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double t = start;
            double[] y = new double[N];

            double k1;
            double k2;
            double k3;
            double k4;
            y[0] = y0;
            for (int i = 1; i < 3; i++)
            {
                k1 = dt * f(t, y0);
                k2 = dt * f(t + dt / 2, y0 + k1 / 2);
                k3 = dt * f(t + dt / 2, y0 + k2 / 2);
                k4 = dt * f(t + dt, y0 + k3);
                y[i] = y0 + (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                t += dt;
                y0 = y[i];
            }
            for (int i = 3; i < N; i++)
            {
                y[i] = y[i - 1] + dt * (23 * f(t, y[i - 1]) - 16 * f(t - dt, y[i - 2]) + 5 * f(t - 2 * dt, y[i - 3])) / 12.0;
                t += dt;
            }
            return y;
        }

        /// <summary>
        /// Fourth Order AB Method
        /// </summary>
        /// <param name="y0">Initial value 1</param>
        /// <param name="start">Start Time</param>
        /// <param name="end">End Time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode model</param>
        /// <returns>approximation with size N</returns>
        public static double[] FourthOrder(double y0, double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double t = start;
            double[] y = new double[N];

            double k1;
            double k2;
            double k3;
            double k4;
            y[0] = y0;
            for (int i = 1; i < 4; i++)
            {
                k1 = dt * f(t, y0);
                k2 = dt * f(t + dt / 2, y0 + k1 / 2);
                k3 = dt * f(t + dt / 2, y0 + k2 / 2);
                k4 = dt * f(t + dt, y0 + k3);
                y[i] = y0 + (k1 + 2 * k2 + 2 * k3 + k4) / 6;
                t += dt;
                y0 = y[i];
            }
            for (int i = 4; i < N; i++)
            {
                y[i] = y[i - 1] + dt * (55 * f(t, y[i - 1]) - 59 * f(t - dt, y[i - 2]) + 37 * f(t - 2 * dt, y[i - 3]) - 9 * f(t - 3 * dt, y[i - 4])) / 24.0;
                t += dt;
            }
            return y;
        }
    }
}
