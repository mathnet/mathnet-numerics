// <copyright file="OdeSolvers.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2013 Math.NET
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
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.OdeSolvers
{
    /// <summary>
    /// ODE Solver Algorithms
    /// </summary>
    public static class RungeKutta
    {
        public static double[] SecondOrder(double y0, double start, double end, int N, Func<double, double, double> f)
        {
            double dt = (end - start) / (N - 1);
            double k1 = 0;
            double k2 = 0;
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
    }
}