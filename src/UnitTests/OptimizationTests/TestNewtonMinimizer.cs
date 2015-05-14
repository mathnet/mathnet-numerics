﻿using System;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    public class RosenbrockObjectiveFunction : BaseObjectiveFunction
    {
        public RosenbrockObjectiveFunction()
            : base(true, true) { }

        protected override void SetValue()
        {
            ValueRaw = RosenbrockFunction.Value(Point);
        }

        protected override void SetGradient()
        {
            GradientRaw = RosenbrockFunction.Gradient(Point);
        }

        protected override void SetHessian()
        {
            HessianRaw = RosenbrockFunction.Hessian(Point);
        }

        public override IObjectiveFunction Fork()
        {
            return new RosenbrockObjectiveFunction();
        }
    }

    [TestFixture]
    public class TestNewtonMinimizer
    {

        [Test]
        public void FindMinimum_Rosenbrock_Easy()
        {
            var obj = new RosenbrockObjectiveFunction();

            var solver = new NewtonMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { 1.2, 1.2 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Hard()
        {
            var obj = new RosenbrockObjectiveFunction();
            var solver = new NewtonMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -1.2, 1.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Overton()
        {
            var obj = new RosenbrockObjectiveFunction();
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
            var obj = new RosenbrockObjectiveFunction();
            var solver = new NewtonMinimizer(1e-5, 1000, true);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -1.2, 1.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Linesearch_Rosenbrock_Overton()
        {
            var obj = new RosenbrockObjectiveFunction();
            var solver = new NewtonMinimizer(1e-5, 1000, true);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -0.9, -0.5 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }
    }
}
