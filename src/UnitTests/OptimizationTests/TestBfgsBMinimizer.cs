using System;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
	[TestFixture]
	public class TestBfgsBMinimizer
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

			Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
			Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
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

			Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
			Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
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

			Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
			Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
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

			Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
			Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
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

			Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
			Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
		}
	}
}

