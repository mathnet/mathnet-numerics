using System;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.Optimization.ObjectiveFunctions;
using NUnit.Framework;
using MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    public class LazyRosenbrockObjectiveFunction : LazyObjectiveFunctionBase
    {
        public LazyRosenbrockObjectiveFunction() : base(true, true) { }

        public override IObjectiveFunction CreateNew()
        {
            return new LazyRosenbrockObjectiveFunction();
        }

        protected override void EvaluateValue()
        {
            Value = RosenbrockFunction.Value(Point);
        }

        protected override void EvaluateGradient()
        {
            Gradient = RosenbrockFunction.Gradient(Point);
        }

        protected override void EvaluateHessian()
        {
            Hessian = RosenbrockFunction.Hessian(Point);
        }
    }

    public class RosenbrockObjectiveFunction : ObjectiveFunctionBase
    {
        public RosenbrockObjectiveFunction() : base(true, true) { }

        public override IObjectiveFunction CreateNew()
        {
            return new RosenbrockObjectiveFunction();
        }

        protected override void Evaluate()
        {
            // here we could directly overwrite the existing matrix cells instead.
            // note: values must then be initialized manually first, if null.
            Value = RosenbrockFunction.Value(Point);
            Gradient = RosenbrockFunction.Gradient(Point);
            Hessian = RosenbrockFunction.Hessian(Point);
        }
    }

    [TestFixture]
    public class NewtonMinimizerTests
    {
        [Test]
        public void FindMinimum_Rosenbrock_Easy()
        {
            var obj = ObjectiveFunction.GradientHessian(RosenbrockFunction.Value, RosenbrockFunction.Gradient, RosenbrockFunction.Hessian);
            var solver = new NewtonMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { 1.2, 1.2 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Hard()
        {
            var obj = ObjectiveFunction.GradientHessian(point => Tuple.Create(RosenbrockFunction.Value(point), RosenbrockFunction.Gradient(point), RosenbrockFunction.Hessian(point)));
            var solver = new NewtonMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -1.2, 1.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Overton()
        {
            var obj = new LazyRosenbrockObjectiveFunction();
            var solver = new NewtonMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -0.9, -0.5 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Linesearch_Rosenbrock_Easy()
        {
            var obj = new RosenbrockObjectiveFunction();
            var solver = new NewtonMinimizer(1e-5, 1000, true);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { 1.2, 1.2 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Linesearch_Rosenbrock_Hard()
        {
            var obj = new LazyRosenbrockObjectiveFunction();
            var solver = new NewtonMinimizer(1e-5, 1000, true);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -1.2, 1.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Linesearch_Rosenbrock_Overton()
        {
            var obj = new LazyRosenbrockObjectiveFunction();
            var solver = new NewtonMinimizer(1e-5, 1000, true);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -0.9, -0.5 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        private class MghTestCaseEnumerator : IEnumerable<ITestCaseData>
        {
            private static readonly string[] _ignore_list =
            {
                "Beale fun (MGH #5) unbounded",
                "Meyer fun (MGH #10) unbounded",
                "Wood fun (MGH #14) unbounded",
            };

            private static bool in_ignore_list(string test_name)
            {
                return _ignore_list.Contains(test_name);
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
                    .Where(x => x.IsUnbounded)
                    .Select(x => new TestCaseData(x)
                        .SetName(x.FullName)
                        .IgnoreIf(in_ignore_list(x.FullName),"Algo error, not implementation error")
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
            var solver = new NewtonMinimizer(1e-8, 1000, useLineSearch: false);

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
    }
}
