// <copyright file="OdeSolverTest.cs" company="Math.NET">
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

extern alias NUnitFramework;

using System;
using MathNet.Numerics.OdeSolvers;
using NUnitFramework.NUnit.Framework;
using System.Linq;

namespace MathNet.Numerics.UnitTests.OdeSolvers
{
    /// <summary>
    /// ODE Solver tests.
    /// </summary>
    [TestFixture, Category("OdeSolver")]
    public class OdeSolverTest
    {
        /// <summary>
        /// Runge-Kutta second order method for first order ODE.
        /// </summary>
        [Test]
        public void RK2Test()
        {
            Func<double, double, double> ode = (t, y) => t + 2 * y * t;
            Func<double, double> sol = (t) => 0.5 * (Math.Exp(t * t) - 1);
            for (int k = 0; k < 4; k++)
            {
                double y0 = 0;
                double[] y_t = RungeKutta.SecondOrder(y0, 0, 2, Convert.ToInt32(Math.Pow(2, k + 6)), ode);
                Console.WriteLine(Math.Abs(sol(2) - y_t.Last()));
            }
        }

        /// <summary>
        /// Runge-Kutta second order method for first order ODE.
        /// </summary>
        [Test]
        public void RK4Test()
        {
            Func<double, double, double> ode = (t, y) => t + 2 * y * t;
            Func<double, double> sol = (t) => 0.5 * (Math.Exp(t * t) - 1);
            for (int k = 0; k < 4; k++)
            {
                double y0 = 0;
                double[] y_t = RungeKutta.FourthOrder(y0, 0, 2, Convert.ToInt32(Math.Pow(2, k + 6)), ode);
                Console.WriteLine(Math.Abs(sol(2) - y_t.Last()));
            }
        }
    }
}