// <copyright file="IntegrationTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
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
    using MbUnit.Framework;

    [TestFixture]
    public class IntegrationTest
    {
        /// <summary>
        /// Test Function: f(x) = exp(-x/5) (2 + sin(2 * x))
        /// </summary>
        private static double TargetFunctionA(double x)
        {
            return Math.Exp(-x / 5) * (2 + Math.Sin(2 * x));
        }

        private const double StartA = 0;
        private const double StopA = 10;
        private const double TargetAreaA = 9.1082396073229965070;

        [Test]
        public void TestIntegratePortal()
        {
            Assert.AreApproximatelyEqual(
                TargetAreaA,
                Integrate.OnClosedInterval(TargetFunctionA, StartA, StopA),
                1e-5,
                "Basic");

            Assert.AreApproximatelyEqual(
                TargetAreaA,
                Integrate.OnClosedInterval(TargetFunctionA, StartA, StopA, 1e-10),
                1e-10,
                "Basic Target 1e-10");
        }

        [Test]
        [Row(1e-5)]
        [Row(1e-13)]
        public void TestDoubleExponentialTransformationAlgorithm(double targetRelativeError)
        {
            var algorithm = new DoubleExponentialTransformation();

            Assert.AreApproximatelyEqual(
                TargetAreaA,
                algorithm.Integrate(TargetFunctionA, StartA, StopA, targetRelativeError),
                targetRelativeError * TargetAreaA,
                "DET Adaptive {0}", targetRelativeError);
        }

        [Test]
        public void TrapeziumRuleSupportsTwoPointIntegration()
        {
            Assert.AreApproximatelyEqual(
                TargetAreaA,
                NewtonCotesTrapeziumRule.IntegrateTwoPoint(TargetFunctionA, StartA, StopA),
                0.4 * TargetAreaA,
                "Direct (1 Partition)");
        }

        [Test]
        [Row(1, 3.5e-1)]
        [Row(5, 1e-1)]
        [Row(10, 2e-2)]
        [Row(50, 6e-4)]
        [Row(1000, 1.5e-6)]
        public void TrapeziumRuleSupportsCompositeIntegration(int partitions, double maxRelativeError)
        {
            Assert.AreApproximatelyEqual(
                TargetAreaA,
                NewtonCotesTrapeziumRule.IntegrateComposite(TargetFunctionA, StartA, StopA, partitions),
                maxRelativeError * TargetAreaA,
                "Composite {0} Partitions", partitions);
        }

        [Test]
        [Row(1e-1)]
        [Row(1e-5)]
        [Row(1e-10)]
        public void TrapeziumRuleSupportsAdaptiveIntegration(double targetRelativeError)
        {
            Assert.AreApproximatelyEqual(
                TargetAreaA,
                NewtonCotesTrapeziumRule.IntegrateAdaptive(TargetFunctionA, StartA, StopA, targetRelativeError),
                targetRelativeError * TargetAreaA,
                "Adaptive {0}", targetRelativeError);
        }

        [Test]
        public void SimpsonRuleSupportsThreePointIntegration()
        {
            Assert.AreApproximatelyEqual(
                TargetAreaA,
                SimpsonRule.IntegrateThreePoint(TargetFunctionA, StartA, StopA),
                0.2 * TargetAreaA,
                "Direct (2 Partitions)");
        }

        [Test]
        [Row(2, 1.7e-1)]
        [Row(6, 1.2e-1)]
        [Row(10, 8e-3)]
        [Row(50, 8e-6)]
        [Row(1000, 5e-11)]
        public void SimpsonRuleSupportsCompositeIntegration(int partitions, double maxRelativeError)
        {
            Assert.AreApproximatelyEqual(
                TargetAreaA,
                SimpsonRule.IntegrateComposite(TargetFunctionA, StartA, StopA, partitions),
                maxRelativeError * TargetAreaA,
                "Composite {0} Partitions", partitions);
        }
    }
}
