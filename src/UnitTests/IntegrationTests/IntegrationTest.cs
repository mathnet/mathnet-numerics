// <copyright file="IntegrationTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests.IntegrationTests
{
    using System;
    using Integration;
    using Integration.Algorithms;
    using NUnit.Framework;

    /// <summary>
    /// Integration tests.
    /// </summary>
    [TestFixture]
    public class IntegrationTest
    {
        /// <summary>
        /// Test Function: f(x) = exp(-x/5) (2 + sin(2 * x))
        /// </summary>
        /// <param name="x">Input value.</param>
        /// <returns>Function result.</returns>
        private static double TargetFunctionA(double x)
        {
            return Math.Exp(-x / 5) * (2 + Math.Sin(2 * x));
        }

        /// <summary>
        /// Test Function Start point.
        /// </summary>
        private const double StartA = 0;

        /// <summary>
        /// Test Function Stop point.
        /// </summary>
        private const double StopA = 10;

        /// <summary>
        /// Target area square.
        /// </summary>
        private const double TargetAreaA = 9.1082396073229965070;

        /// <summary>
        /// Test integrate portal.
        /// </summary>
        [Test]
        public void TestIntegratePortal()
        {
            Assert.AreEqual(
                TargetAreaA, 
                Integrate.OnClosedInterval(TargetFunctionA, StartA, StopA), 
                1e-5, 
                "Basic");

            Assert.AreEqual(
                TargetAreaA, 
                Integrate.OnClosedInterval(TargetFunctionA, StartA, StopA, 1e-10), 
                1e-10, 
                "Basic Target 1e-10");
        }

        /// <summary>
        /// Test double exponential transformation algorithm.
        /// </summary>
        /// <param name="targetRelativeError">Relative error.</param>
        [Test]
        public void TestDoubleExponentialTransformationAlgorithm([Values(1e-5, 1e-13)] double targetRelativeError)
        {
            var algorithm = new DoubleExponentialTransformation();

            Assert.AreEqual(
                TargetAreaA, 
                algorithm.Integrate(TargetFunctionA, StartA, StopA, targetRelativeError), 
                targetRelativeError * TargetAreaA, 
                "DET Adaptive {0}", 
                targetRelativeError);
        }

        /// <summary>
        /// Trapezium rule supports two point integration.
        /// </summary>
        [Test]
        public void TrapeziumRuleSupportsTwoPointIntegration()
        {
            Assert.AreEqual(
                TargetAreaA, 
                NewtonCotesTrapeziumRule.IntegrateTwoPoint(TargetFunctionA, StartA, StopA), 
                0.4 * TargetAreaA, 
                "Direct (1 Partition)");
        }

        /// <summary>
        /// Trapezium rule supports composite integration.
        /// </summary>
        /// <param name="partitions">Partitions count.</param>
        /// <param name="maxRelativeError">Maximum relative error.</param>
        [Test, Sequential]
        public void TrapeziumRuleSupportsCompositeIntegration([Values(1, 5, 10, 50, 1000)] int partitions, [Values(3.5e-1, 1e-1, 2e-2, 6e-4, 1.5e-6)] double maxRelativeError)
        {
            Assert.AreEqual(
                TargetAreaA, 
                NewtonCotesTrapeziumRule.IntegrateComposite(TargetFunctionA, StartA, StopA, partitions), 
                maxRelativeError * TargetAreaA, 
                "Composite {0} Partitions", 
                partitions);
        }

        /// <summary>
        /// Trapezium rule supports adaptive integration.
        /// </summary>
        /// <param name="targetRelativeError">Relative error</param>
        [Test]
        public void TrapeziumRuleSupportsAdaptiveIntegration([Values(1e-1, 1e-5, 1e-10)] double targetRelativeError)
        {
            Assert.AreEqual(
                TargetAreaA, 
                NewtonCotesTrapeziumRule.IntegrateAdaptive(TargetFunctionA, StartA, StopA, targetRelativeError), 
                targetRelativeError * TargetAreaA, 
                "Adaptive {0}", 
                targetRelativeError);
        }

        /// <summary>
        /// Simpson rule supports three point integration.
        /// </summary>
        [Test]
        public void SimpsonRuleSupportsThreePointIntegration()
        {
            Assert.AreEqual(
                TargetAreaA, 
                SimpsonRule.IntegrateThreePoint(TargetFunctionA, StartA, StopA), 
                0.2 * TargetAreaA, 
                "Direct (2 Partitions)");
        }

        /// <summary>
        /// Simpson rule supports composite integration.
        /// </summary>
        /// <param name="partitions">Partitions count.</param>
        /// <param name="maxRelativeError">Maximum relative error.</param>
        [Test, Sequential]
        public void SimpsonRuleSupportsCompositeIntegration([Values(2, 6, 10, 50, 1000)] int partitions, [Values(1.7e-1, 1.2e-1, 8e-3, 8e-6, 5e-11)] double maxRelativeError)
        {
            Assert.AreEqual(
                TargetAreaA, 
                SimpsonRule.IntegrateComposite(TargetFunctionA, StartA, StopA, partitions), 
                maxRelativeError * TargetAreaA, 
                "Composite {0} Partitions", 
                partitions);
        }
    }
}
