using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using MathNet.Numerics.Optimization;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class TestBfgsMinimizer
    {

        [Test]
        public void FindMinimum_Rosenbrock_Easy()
        {
            var obj = new SimpleObjectiveFunction(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { 1.2, 1.2 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Hard()
        {
            var obj = new SimpleObjectiveFunction(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { -1.2, 1.0 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }
        
        [Test]
        public void FindMinimum_Rosenbrock_Overton()
        {
            var obj = new SimpleObjectiveFunction(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new BfgsMinimizer(1e-5, 1000);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[] { -0.9, -0.5 }));

            Assert.That(Math.Abs(result.MinimizingPoint[0] - 1.0), Is.LessThan(1e-3));
            Assert.That(Math.Abs(result.MinimizingPoint[1] - 1.0), Is.LessThan(1e-3));
        }

    }
}
