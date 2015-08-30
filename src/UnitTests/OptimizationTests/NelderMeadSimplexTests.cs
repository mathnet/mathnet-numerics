// <copyright file="NelderMeadSimplexTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2015 Math.NET
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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class NelderMeadSimplexTests
    {
        /// <summary>
        /// Test that finds the constants of a parable, function adds noise and return the mean square error
        /// Copied from the test in https://code.google.com/p/nelder-mead-simplex/
        /// </summary>
        [Test]
        public void FindParableConstantsThatMinimizesErrors()
        {
            var nms = new NelderMeadSimplex(1e-6, 1000);
            double a = 5;
            double b = 10;
            IObjectiveFunction objFun = ObjectiveFunction.Value((constants)=>
            {
                double ssq = 0;
                System.Random r = new System.Random();
                for (double x = -10; x < 10; x += .1)
                {
                    double yTrue = a * x * x + b * x + r.NextDouble();
                    double yRegress = constants[0] * x * x + constants[1] * x;
                    ssq += Math.Pow((yTrue - yRegress), 2);
                }
                return ssq;
            });
            var initialGuess = new DenseVector(2);
            initialGuess[0] = 3;
            initialGuess[1] = 5;
            var result = nms.FindMinimum(objFun, initialGuess);

            Assert.NotNull(result);
            Assert.NotNull(result.MinimizingPoint);
            Assert.NotNull(result.FunctionInfoAtMinimum);
            Assert.That(Math.Abs(result.MinimizingPoint[0] - a), Is.LessThan(1e-2));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - b), Is.LessThan(1e-2));
        }

        [Test]
        public void NMS_FindMinimum_Rosenbrock_Easy()
        {
            var obj = ObjectiveFunction.Value(RosenbrockFunction.Value);
            var solver = new NelderMeadSimplex(1e-5, maximumIterations: 1000);
            var initialGuess = new DenseVector(new[] { 1.2, 1.2 });

            var result = solver.FindMinimum(obj, initialGuess);

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }


        [Test]
        public void NMS_FindMinimum_Rosenbrock_Hard()
        {
            var obj = ObjectiveFunction.Value(RosenbrockFunction.Value);
            var solver = new NelderMeadSimplex(1e-5, maximumIterations: 1000);

            var initialGuess = new DenseVector(new[] { -1.2, 1.0 });

            var result = solver.FindMinimum(obj,initialGuess);

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }
    }
}
