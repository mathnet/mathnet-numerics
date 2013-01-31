using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using MathNet.Numerics.Optimization;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class TestConjugateGradientMinimizer
    {

        [Test]
        public void FindMinimum_Rosenbrock_Easy()
        {
            var obj = new SimpleObjectiveFunction(RosenbrockFunction.Value, RosenbrockFunction.Gradient);
            var solver = new ConjugateGradientMinimizer(1e-5, 100);
            var result = solver.FindMinimum(obj, new MathNet.Numerics.LinearAlgebra.Double.DenseVector(new double[]{1.2,1.2}));
            Assert.That(result.MinimizingPoint[0], Is.EqualTo(1.0));
            Assert.That(result.MinimizingPoint[1], Is.EqualTo(1.0));
        }

        [Test]
        public void FindMinimum_Rosenbrock_Hard()
        {

        }
    }
}
