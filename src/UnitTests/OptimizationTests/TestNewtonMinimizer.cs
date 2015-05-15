using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.Optimization.ObjectiveFunctions;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    public class RosenbrockObjectiveFunction : BaseObjectiveFunction
    {
        public RosenbrockObjectiveFunction() : base(true, true) { }

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

        public override IObjectiveFunction CreateNew()
        {
            return new RosenbrockObjectiveFunction();
        }
    }

    public class InplaceRosenbrockObjectiveFunction : InplaceObjectiveFunction
    {
        public InplaceRosenbrockObjectiveFunction() : base(true, true) { }

        public override IObjectiveFunction CreateNew()
        {
            return new InplaceRosenbrockObjectiveFunction();
        }

        protected override void EvaluateAt(Vector<double> point, ref double value, ref Vector<double> gradient, ref Matrix<double> hessian)
        {
            // here we could directly overwrite the existing matrices instead.
            // note: values must then be initialized manually here first, if null.
            value = RosenbrockFunction.Value(point);
            gradient = RosenbrockFunction.Gradient(point);
            hessian = RosenbrockFunction.Hessian(point);
        }
    }

    [TestFixture]
    public class TestNewtonMinimizer
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
            var obj = new RosenbrockObjectiveFunction();
            var solver = new NewtonMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new DenseVector(new[] { -0.9, -0.5 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Linesearch_Rosenbrock_Easy()
        {
            var obj = new InplaceRosenbrockObjectiveFunction();
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
