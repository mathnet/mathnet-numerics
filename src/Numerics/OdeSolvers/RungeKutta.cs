// <copyright file="OdeSolvers.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.OdeSolvers
{
    /// <summary>
    /// ODE Solver Algorithms
    /// </summary>
    public static class RungeKutta
    {
        /// <summary>
        /// Second Order Runge-Kutta method
        /// </summary>
        /// <param name="y0">initial value</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode function</param>
        /// <returns>approximations</returns>
        public static double[] SecondOrder(double y0, double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double k1;
            double k2;
            double t = start;
            double[] y = new double[N];
            y[0] = y0;
            for (int i = 1; i < N; i++)
            {
                k1 = f(t, y0);
                k2 = f(t + dt, y0 + k1 * dt);
                y[i] = y0 + dt * 0.5 * (k1 + k2);
                t += dt;
                y0 = y[i];
            }
            return y;
        }

        /// <summary>
        /// Fourth Order Runge-Kutta method
        /// </summary>
        /// <param name="y0">initial value</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode function</param>
        /// <returns>approximations</returns>
        public static double[] FourthOrder(double y0, double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double k1;
            double k2;
            double k3;
            double k4;
            double t = start;
            double[] y = new double[N];
            y[0] = y0;
            for (int i = 1; i < N; i++)
            {
                k1 = f(t, y0);
                k2 = f(t + dt / 2, y0 + k1 * dt / 2);
                k3 = f(t + dt / 2, y0 + k2 * dt / 2);
                k4 = f(t + dt, y0 + k3 * dt);
                y[i] = y0 + dt / 6 * (k1 + 2 * k2 + 2 * k3 + k4);
                t += dt;
                y0 = y[i];
            }
            return y;
        }

        /// <summary>
        /// Second Order Runge-Kutta to solve ODE SYSTEM
        /// </summary>
        /// <param name="y0">initial vector</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode function</param>
        /// <returns>approximations</returns>
        public static Vector<double>[] SecondOrder(Vector<double> y0, double start, double end, int N, Func<double, Vector<double>, Vector<double>> f)
        {
            double dt = (end - start) / (N - 1);
            Vector<double> k1, k2;
            Vector<double>[] y = new Vector<double>[N];
            double t = start;
            y[0] = y0;
            for (int i = 1; i < N; i++)
            {
                k1 = f(t, y0);
                k2 = f(t, y0 + k1 * dt);
                y[i] = y0 + dt * 0.5 * (k1 + k2);
                t += dt;
                y0 = y[i];
            }
            return y;
        }

        /// <summary>
        /// Fourth Order Runge-Kutta to solve ODE SYSTEM
        /// </summary>
        /// <param name="y0">initial vector</param>
        /// <param name="start">start time</param>
        /// <param name="end">end time</param>
        /// <param name="N">Size of output array(the larger, the finer)</param>
        /// <param name="f">ode function</param>
        /// <returns>approximations</returns>
        public static Vector<double>[] FourthOrder(Vector<double> y0, double start, double end, int N, Func<double, Vector<double>, Vector<double>> f)
        {
            double dt = (end - start) / (N - 1);
            Vector<double> k1, k2, k3, k4;
            Vector<double>[] y = new Vector<double>[N];
            double t = start;
            y[0] = y0;
            for (int i = 1; i < N; i++)
            {
                k1 = f(t, y0);
                k2 = f(t + dt / 2, y0 + k1 * dt / 2);
                k3 = f(t + dt / 2, y0 + k2 * dt / 2);
                k4 = f(t + dt, y0 + k3 * dt);
                y[i] = y0 + dt / 6 * (k1 + 2 * k2 + 2 * k3 + k4);
                t += dt;
                y0 = y[i];
            }
            return y;
        }
    }
}
