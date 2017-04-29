// <copyright file="BfgsBMinimizerTests.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions;
using System.Collections;
using MathNet.Numerics.Optimization.ObjectiveFunctions;
using NUnit.Framework.Interfaces;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class BfgsBMinimizerTests
    {
        [Test]
        public void FindMinimum_Rosenbrock_Easy()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsBMinimizer (1e-5, 1e-5, 1e-5, maximumIterations: 1000);
            var lowerBound = new DenseVector(new[]{ -5.0, -5.0 });
            var upperBound = new DenseVector(new[]{ 5.0, 5.0 });
            var initialGuess = new DenseVector(new[] { 1.2, 1.2 });

            var result = solver.FindMinimum(obj, lowerBound, upperBound, initialGuess);

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Hard()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsBMinimizer (1e-5, 1e-5, 1e-5, maximumIterations: 1000);

            var lowerBound = new DenseVector(new[]{ -5.0, -5.0 });
            var upperBound = new DenseVector(new[]{ 5.0, 5.0 });

            var initialGuess = new DenseVector (new[]{ -1.2, 1.0 });

            var result = solver.FindMinimum(obj, lowerBound, upperBound, initialGuess);

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Overton()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsBMinimizer (1e-5, 1e-5, 1e-5, maximumIterations: 1000);

            var lowerBound = new DenseVector(new[]{ -5.0, -5.0 });
            var upperBound = new DenseVector(new[]{ 5.0, 5.0 });
            var initialGuess = new DenseVector (new[]{ -0.9, -0.5 });

            var result = solver.FindMinimum (obj, lowerBound, upperBound, initialGuess);

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Easy_OneBoundary()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsBMinimizer (1e-5, 1e-5, 1e-5, maximumIterations: 1000);
            var lowerBound = new DenseVector(new[]{ 1.0, -5.0 });
            var upperBound = new DenseVector(new[]{ 5.0, 5.0 });
            var initialGuess = new DenseVector(new[] { 1.2, 1.2 });

            var result = solver.FindMinimum(obj, lowerBound, upperBound, initialGuess);

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Easy_TwoBoundaries()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsBMinimizer (1e-5, 1e-5, 1e-5, maximumIterations: 1000);
            var lowerBound = new DenseVector(new[]{ 1.0, 1.0 });
            var upperBound = new DenseVector(new[]{ 5.0, 5.0 });
            var initialGuess = new DenseVector(new[] { 1.2, 1.2 });

            var result = solver.FindMinimum(obj, lowerBound, upperBound, initialGuess);

            Assert.That(Math.Abs(result.MinimizingPoint[0] - RosenbrockFunction.Minimum[0]), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - RosenbrockFunction.Minimum[1]), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_MinimumGreateerOrEqualToLowerBoundary()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsBMinimizer(1e-5, 1e-5, 1e-5, maximumIterations: 1000);

            var lowerBound = new DenseVector(new[] { 2, 2.0 });
            var upperBound = new DenseVector(new[] { 5.0, 5.0 });

            var initialGuess = new DenseVector(new[] { 2.5, 2.5 });

            var result = solver.FindMinimum(obj, lowerBound, upperBound, initialGuess);

            Assert.GreaterOrEqual(result.MinimizingPoint[0],lowerBound[0]);
            Assert.GreaterOrEqual(result.MinimizingPoint[1], lowerBound[1]);
        }

        [Test]
        public void FindMinimum_Rosenbrock_MinimumLesserOrEqualToUpperBoundary()
        {
            var obj = ObjectiveFunction.Gradient(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsBMinimizer(1e-5, 1e-5, 1e-5, maximumIterations: 1000);

            var lowerBound = new DenseVector(new[] { -2.0, -2.0 });
            var upperBound = new DenseVector(new[] { 0.5, 0.5 });

            var initialGuess = new DenseVector(new[] { -0.9, -0.5 });

            var result = solver.FindMinimum(obj, lowerBound, upperBound, initialGuess);

            Assert.LessOrEqual(result.MinimizingPoint[0],upperBound[0]);
            Assert.LessOrEqual(result.MinimizingPoint[1],upperBound[1]);
        }

        [Test]
        [TestCaseSource(typeof(MghTestCaseEnumerator))]
        public void Mgh_Tests(TestFunctions.TestCase test_case)
        {
            var obj = new MghObjectiveFunction(test_case.Function, true, true);
            var solver = new BfgsBMinimizer(1e-8, 1e-8, 1e-8, 1000);

            var result = solver.FindMinimum(obj, test_case.LowerBound, test_case.UpperBound, test_case.InitialGuess);

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
        [TestCaseSource(typeof(FdMghTestCaseEnumerator))]
        public void Mgh_FiniteDifference_Tests(TestFunctions.TestCase test_case)
        {
            var obj1 = new MghObjectiveFunction(test_case.Function, true, true);
            var obj = new ForwardDifferenceGradientObjectiveFunction(obj1, test_case.LowerBound, test_case.UpperBound, 1e-10, 1e-10);
            var solver = new BfgsBMinimizer(1e-8, 1e-8, 1e-8, 1000);

            var result = solver.FindMinimum(obj, test_case.LowerBound, test_case.UpperBound, test_case.InitialGuess);

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


        private class BaseMghTestCaseEnumerator : IEnumerable<ITestCaseData>
        {
            private string _prefix = "";

            public BaseMghTestCaseEnumerator(string prefix)
            {
                if (prefix.EndsWith(" "))
                    _prefix = prefix;
                else
                    _prefix = prefix + " ";
            }
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
                    .Where(x => x.IsBounded)
                    .Select<TestCase, ITestCaseData>(x => new TestCaseData(x)
                        .SetName(_prefix + x.FullName)
                    )
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private class MghTestCaseEnumerator : BaseMghTestCaseEnumerator
        {
            public MghTestCaseEnumerator() : base("") { }
        }

        private class FdMghTestCaseEnumerator : BaseMghTestCaseEnumerator
        {
            public FdMghTestCaseEnumerator() : base("FD") { }
        }
    }
}
