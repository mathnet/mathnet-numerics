// <copyright file="IntegrationTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using System;
using MathNet.Numerics.Integration;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.IntegrationTests
{
    /// <summary>
    /// Integration tests.
    /// </summary>
    [TestFixture, Category("Integration")]
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
        /// Test Function: f(x,y) = exp(-x/5) (2 + sin(x * y))
        /// </summary>
        /// <param name="x">First input value.</param>
        /// <param name="y">Second input value.</param>
        /// <returns>Function result.</returns>
        private static double TargetFunctionB(double x, double y)
        {
            return Math.Exp(-x / 5) * (2 + Math.Sin(2 * y));
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
        /// Test Function Start point.
        /// </summary>
        private const double StartB = 0;

        /// <summary>
        /// Test Function Stop point.
        /// </summary>
        private const double StopB = 1;

        /// <summary>
        /// Target area square.
        /// </summary>
        private const double TargetAreaA = 9.1082396073229965070;

        /// <summary>
        /// Target area.
        /// </summary>
        private const double TargetAreaB = 11.7078776759298776163;

        /// <summary>
        /// Test Integrate facade for simple use cases.
        /// </summary>
        [Test]
        public void TestIntegrateFacade()
        {
            Assert.AreEqual(
                TargetAreaA,
                Integrate.OnClosedInterval(TargetFunctionA, StartA, StopA),
                1e-5,
                "Interval");

            Assert.AreEqual(
                TargetAreaA,
                Integrate.OnClosedInterval(TargetFunctionA, StartA, StopA, 1e-10),
                1e-10,
                "Interval, Target 1e-10");

            Assert.AreEqual(
                Integrate.OnRectangle(TargetFunctionB, StartA, StopA, StartB, StopB),
                TargetAreaB,
                1e-12,
                "Rectangle");

            Assert.AreEqual(
                Integrate.OnRectangle(TargetFunctionB, StartA, StopA, StartB, StopB, 22),
                TargetAreaB,
                1e-10,
                "Rectangle, Gauss-Legendre Order 22");
        }

        /// <summary>
        /// Test double exponential transformation algorithm.
        /// </summary>
        /// <param name="targetRelativeError">Relative error.</param>
        [TestCase(1e-5)]
        [TestCase(1e-13)]
        public void TestDoubleExponentialTransformationAlgorithm(double targetRelativeError)
        {
            Assert.AreEqual(
                TargetAreaA,
                DoubleExponentialTransformation.Integrate(TargetFunctionA, StartA, StopA, targetRelativeError),
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
        [TestCase(1, 3.5e-1)]
        [TestCase(5, 1e-1)]
        [TestCase(10, 2e-2)]
        [TestCase(50, 6e-4)]
        [TestCase(1000, 1.5e-6)]
        public void TrapeziumRuleSupportsCompositeIntegration(int partitions, double maxRelativeError)
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
        [TestCase(1e-1)]
        [TestCase(1e-5)]
        [TestCase(1e-10)]
        public void TrapeziumRuleSupportsAdaptiveIntegration(double targetRelativeError)
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
        [TestCase(2, 1.7e-1)]
        [TestCase(6, 1.2e-1)]
        [TestCase(10, 8e-3)]
        [TestCase(50, 8e-6)]
        [TestCase(1000, 5e-11)]
        public void SimpsonRuleSupportsCompositeIntegration(int partitions, double maxRelativeError)
        {
            Assert.AreEqual(
                TargetAreaA,
                SimpsonRule.IntegrateComposite(TargetFunctionA, StartA, StopA, partitions),
                maxRelativeError * TargetAreaA,
                "Composite {0} Partitions",
                partitions);
        }

        /// <summary>
        /// Gauss-Legendre rule supports integration.
        /// </summary>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule.</param>
        [TestCase(19)]
        [TestCase(20)]
        [TestCase(21)]
        [TestCase(22)]
        public void TestGaussLegendreRuleIntegration(int order)
        {
            double appoximateArea = GaussLegendreRule.Integrate(TargetFunctionA, StartA, StopA, order);
            double relativeError = Math.Abs(TargetAreaA - appoximateArea) / TargetAreaA;
            Assert.Less(relativeError, 5e-16);
        }

        /// <summary>
        /// Gauss-Legendre rule supports 2-dimensional integration over the rectangle.
        /// </summary>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule.</param>
        [TestCase(19)]
        [TestCase(20)]
        [TestCase(21)]
        [TestCase(22)]
        public void TestGaussLegendreRuleIntegrate2D(int order)
        {
            double appoximateArea = GaussLegendreRule.Integrate(TargetFunctionB, StartA, StopA, StartB, StopB, order);
            double relativeError = Math.Abs(TargetAreaB - appoximateArea) / TargetAreaB;
            Assert.Less(relativeError, 1e-15);
        }

        /// <summary>
        /// Gauss-Legendre rule supports obtaining the ith abscissa/weight. In this case, they're used for integration.
        /// </summary>
        /// <param name="order">Defines an Nth order Gauss-Legendre rule. The order also defines the number of abscissas and weights for the rule.</param>
        [TestCase(19)]
        [TestCase(20)]
        [TestCase(21)]
        [TestCase(22)]
        public void TestGaussLegendreRuleGetAbscissaGetWeightOrderViaIntegration(int order)
        {
            GaussLegendreRule gaussLegendre = new GaussLegendreRule(StartA, StopA, order);

            double appoximateArea = 0;
            for (int i = 0; i < gaussLegendre.Order; i++)
            {
                appoximateArea += gaussLegendre.GetWeight(i) * TargetFunctionA(gaussLegendre.GetAbscissa(i));
            }

            double relativeError = Math.Abs(TargetAreaA - appoximateArea) / TargetAreaA;
            Assert.Less(relativeError, 5e-16);
        }

        /// <summary>
        /// Gauss-Legendre rule supports obtaining array of abscissas/weights.
        /// </summary>
        [Test]
        public void TestGaussLegendreRuleAbscissasWeightsViaIntegration()
        {
            const int order = 19;
            GaussLegendreRule gaussLegendre = new GaussLegendreRule(StartA, StopA, order);
            double[] abscissa = gaussLegendre.Abscissas;
            double[] weight = gaussLegendre.Weights;

            for (int i = 0; i < gaussLegendre.Order; i++)
            {
                Assert.AreEqual(gaussLegendre.GetAbscissa(i),abscissa[i]);
                Assert.AreEqual(gaussLegendre.GetWeight(i), weight[i]);
            }
        }

        /// <summary>
        /// Gauss-Legendre rule supports obtaining IntervalBegin.
        /// </summary>
        [Test]
        public void TestGetGaussLegendreRuleIntervalBegin()
        {
            const int order = 19;
            GaussLegendreRule gaussLegendre = new GaussLegendreRule(StartA, StopA, order);
            Assert.AreEqual(gaussLegendre.IntervalBegin, StartA);
        }

        /// <summary>
        /// Gauss-Legendre rule supports obtaining IntervalEnd.
        /// </summary>
        [Test]
        public void TestGaussLegendreRuleIntervalEnd()
        {
            const int order = 19;
            GaussLegendreRule gaussLegendre = new GaussLegendreRule(StartA, StopA, order);
            Assert.AreEqual(gaussLegendre.IntervalEnd, StopA);
        }
    }
}
