// <copyright file="BfgsMinimizerTests.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using NUnit.Framework;
using MathNet.Numerics.Tests.OptimizationTests.TestFunctions;
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework.Interfaces;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.ObjectiveFunctions;

namespace MathNet.Numerics.Tests.OptimizationTests
{
    [TestFixture]
    public class BfgsMinimizerTests
    {
        [Test]
        public void FindMinimum_Rosenbrock_Easy()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { 1.2, 1.2 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Hard()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -1.2, 1.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Overton()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -0.9, -0.5 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_BigRosenbrock_Easy()
        {
            var obj = ObjectiveFunction.Gradient(BigRosenbrockFunction.Value, BigRosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-10, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { 1.2*100.0, 1.2*100.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - BigRosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - BigRosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_BigRosenbrock_Hard()
        {
            var obj = ObjectiveFunction.Gradient(BigRosenbrockFunction.Value, BigRosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -1.2*100.0, 1.0*100.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - BigRosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - BigRosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_BigRosenbrock_Overton()
        {
            var obj = ObjectiveFunction.Gradient(BigRosenbrockFunction.Value, BigRosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1e-5, 1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -0.9*100.0, -0.5*100.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - BigRosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - BigRosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        private class MghTestCaseEnumerator : IEnumerable<ITestCaseData>
        {
            public IEnumerator<ITestCaseData> GetEnumerator()
            {
                return
                    RosenbrockFunction2.TestCases
                        .Concat(BealeFunction.TestCases)
                        .Concat(HelicalValleyFunction.TestCases)
                        .Concat(MeyerFunction.TestCases)
                        .Concat(PowellSingularFunction.TestCases)
                        .Concat(WoodFunction.TestCases)
                        .Concat(BrownAndDennisFunction.TestCases)
                    .Where(x => x.IsUnbounded)
                    .Select<TestCase, ITestCaseData>(x => new TestCaseData(x)
                        .SetName(x.FullName)
                    )
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        [Test]
        [TestCaseSource(typeof(MghTestCaseEnumerator))]
        public void Mgh_Tests(TestFunctions.TestCase test_case)
        {
            var obj = new MghObjectiveFunction(test_case.Function, true, true);
            var solver = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 1000);

            var result = solver.FindMinimum(obj, test_case.InitialGuess);

            if (test_case.MinimizingPoint != null)
            {
                Assert.That((result.MinimizingPoint - test_case.MinimizingPoint).L2Norm(), Is.LessThan(1e-3));
            }

            var val1 = result.FunctionInfoAtMinimum.Value;
            var val2 = test_case.MinimalValue;
            var abs_min = Math.Min(Math.Abs(val1), Math.Abs(val2));
            var abs_err = Math.Abs(val1 - val2);
            var rel_err = abs_err / abs_min;
            var success = (abs_min <= 1 && abs_err < 1e-3) || (abs_min > 1 && rel_err < 1e-3);
            Assert.That(success, "Minimal function value is not as expected.");
        }


        [Test]
        public void BfgsMinimizer_MinimizesProperlyWhenConstrainedInOneVariable()
        {
            // Test comes from github issue 510
            // https://github.com/mathnet/mathnet-numerics/issues/510
            Func<Vector<double>, double> function = (Vector<double> vectorArg) =>
            {
                double x = vectorArg[0];
                double y = -1.0 * Math.Exp(-x * x);
                return y;
            };

            double gradientTolerance = 1e-10;
            double parameterTolerance = 1e-10;
            double functionProgressTolerance = 1e-10;
            int maxIterations = 1000;

            var initialGuess = new DenseVector(new[] { -1.0 });

            // Bad Bound
            var lowerBound = new DenseVector(new[] { -2.0 });
            var upperBound = new DenseVector(new[] { 2.0 });


            var objective = ObjectiveFunction.Value(function);
            var objectiveWithGradient = new ForwardDifferenceGradientObjectiveFunction(objective, lowerBound, upperBound);
            var algorithm = new BfgsBMinimizer(gradientTolerance, parameterTolerance, functionProgressTolerance, maxIterations);
            var result = algorithm.FindMinimum(objectiveWithGradient, lowerBound, upperBound, initialGuess);

            var resultVector = result.MinimizingPoint;
            double xResult = resultVector[0];

            // (actual minimum at zero)
            var abs_err = Math.Abs(xResult);
            var success = abs_err < 1e-6;
            Assert.That(success, "Minimal function value is not as expected.");
        }
    }
}
