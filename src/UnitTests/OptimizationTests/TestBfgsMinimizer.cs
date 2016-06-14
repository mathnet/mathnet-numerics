using System;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class TestBfgsMinimizer
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
    }
}
