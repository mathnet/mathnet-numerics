using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    class TestRosenbrockFunction
    {
        [Test]
        public void TestGradient()
        {
            var input = new LinearAlgebra.Double.DenseVector(new double[]{ -0.9, -0.5 } );

            var v1 = RosenbrockFunction.Value(input);
            var g = RosenbrockFunction.Gradient(input);

            var eps = 1e-5;
            var eps0 = (new LinearAlgebra.Double.DenseVector(new double[] { 1.0, 0.0 })) * eps;
            var eps1 = (new LinearAlgebra.Double.DenseVector(new double[] { 0.0, 1.0 })) * eps;

            var g0 = (RosenbrockFunction.Value(input + eps0) - RosenbrockFunction.Value(input - eps0)) / (2 * eps);
            var g1 = (RosenbrockFunction.Value(input + eps1) - RosenbrockFunction.Value(input - eps1)) / (2 * eps);

            Assert.That(Math.Abs(g0 - g[0]) < 1e-3);
            Assert.That(Math.Abs(g1 - g[1]) < 1e-3);
        }

        [Test]
        public void TestHessian()
        {
            var input = new LinearAlgebra.Double.DenseVector(new double[] { -0.9, -0.5 });

            var v1 = RosenbrockFunction.Value(input);
            var h = RosenbrockFunction.Hessian(input);

            var eps = 1e-5;

            var eps0 = (new LinearAlgebra.Double.DenseVector(new double[] { 1.0, 0.0 })) * eps;
            var eps1 = (new LinearAlgebra.Double.DenseVector(new double[] { 0.0, 1.0 })) * eps;

            var epsuu = (new LinearAlgebra.Double.DenseVector(new double[] { 1.0, 1.0 })) * eps;
            var epsud = (new LinearAlgebra.Double.DenseVector(new double[] { 1.0, -1.0 })) * eps;
            
            

            var h00 = (RosenbrockFunction.Value(input + eps0) - 2*RosenbrockFunction.Value(input) + RosenbrockFunction.Value(input - eps0)) / (eps*eps);
            var h11 = (RosenbrockFunction.Value(input + eps1) - 2 * RosenbrockFunction.Value(input) + RosenbrockFunction.Value(input - eps1)) / (eps * eps);
            var h01 = (RosenbrockFunction.Value(input + epsuu) - RosenbrockFunction.Value(input + epsud) - RosenbrockFunction.Value(input - epsud) + RosenbrockFunction.Value(input - epsuu)) / (4*eps * eps);


            Assert.That(Math.Abs(h00 - h[0,0]) < 1e-3);
            Assert.That(Math.Abs(h11 - h[1,1]) < 1e-3);
            Assert.That(Math.Abs(h01 - h[0, 1]) < 1e-3);
            Assert.That(Math.Abs(h01 - h[1, 0]) < 1e-3);
        }
    }
}
