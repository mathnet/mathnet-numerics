using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using MathNet.Numerics.Optimization;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    public class RosenbrockEvaluation : BaseEvaluation
    {
        public RosenbrockEvaluation()
            : base(true, true) { }

        protected override void SetValue()
        {
            this.ValueRaw = RosenbrockFunction.Value(this.Point);
        }

        protected override void SetGradient()
        {
            this.GradientRaw = RosenbrockFunction.Gradient(this.Point);
        }

        protected override void SetHessian()
        {
            this.HessianRaw = RosenbrockFunction.Hessian(this.Point);
        }

        public override IEvaluation CreateNew()
        {
            return new RosenbrockEvaluation();
        }
    }

    [TestFixture]
    public class TestNewtonMinimizer
    {

        [Test]
        public void FindMinimum_Rosenbrock_Easy()
        {
            var obj = new RosenbrockEvaluation();

            var solver = new NewtonMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 1.2, 1.2 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Hard()
        {
            var obj = new RosenbrockEvaluation();
            var solver = new NewtonMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { -1.2, 1.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Overton()
        {
            var obj = new RosenbrockEvaluation();
            var solver = new NewtonMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { -0.9, -0.5 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Linesearch_Rosenbrock_Easy()
        {
            var obj = new RosenbrockEvaluation();
            var solver = new NewtonMinimizer(1e-5, 1000, true);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 1.2, 1.2 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Linesearch_Rosenbrock_Hard()
        {
            var obj = new RosenbrockEvaluation();
            var solver = new NewtonMinimizer(1e-5, 1000, true);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { -1.2, 1.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Linesearch_Rosenbrock_Overton()
        {
            var obj = new RosenbrockEvaluation();
            var solver = new NewtonMinimizer(1e-5, 1000, true);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { -0.9, -0.5 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }
    }
}
