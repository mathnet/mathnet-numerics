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

using MathNet.Numerics.Integration;
using NUnit.Framework;
using System;
using System.Numerics;

namespace MathNet.Numerics.Tests.IntegrationTests
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
        /// Test Function: f(x,y) = exp(-x/5) (2 + sin(2 * y))
        /// </summary>
        /// <param name="x">First input value.</param>
        /// <param name="y">Second input value.</param>
        /// <returns>Function result.</returns>
        private static double TargetFunctionB(double x, double y)
        {
            return Math.Exp(-x / 5) * (2 + Math.Sin(2 * y));
        }

        /// <summary>
        /// Test Function: f(x) = 1 / (1 + x^2)
        /// </summary>
        /// <param name="x">First input value.</param>
        /// <returns>Function result.</returns>
        private static double TargetFunctionC(double x)
        {
            return 1 / (1 + x * x);
        }

        /// <summary>
        /// Test Function: f(x) = log(x)
        /// </summary>
        /// <param name="x">First input value.</param>
        /// <returns>Function result.</returns>
        private static double TargetFunctionD(double x)
        {
            return Math.Log(x);
        }

        /// <summary>
        /// Test Function: f(x) = log^2(x)
        /// </summary>
        /// <param name="x">First input value.</param>
        /// <returns>Function result.</returns>
        private static double TargetFunctionE(double x)
        {
            return Math.Log(x) * Math.Log(x);
        }

        /// <summary>
        /// Test Function: f(x) = e^(-x) cos(x)
        /// </summary>
        /// <param name="x">First input value.</param>
        /// <returns>Function result.</returns>
        private static double TargetFunctionF(double x)
        {
            return Math.Exp(-x) * Math.Cos(x);
        }

        /// <summary>
        /// Test Function: f(x) = sqrt(x)/sqrt(1-x^2)
        /// </summary>
        /// <param name="x">First input value.</param>
        /// <returns>Function result.</returns>
        private static double TargetFunctionG(double x)
        {
            return Math.Sqrt(x) / Math.Sqrt(1 - x * x);
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
        /// Test Function Start point.
        /// </summary>
        private const double StartC = double.NegativeInfinity;

        /// <summary>
        /// Test Function Stop point.
        /// </summary>
        private const double StopC = double.PositiveInfinity;

        /// <summary>
        /// Test Function Start point.
        /// </summary>
        private const double StartD = 0;

        /// <summary>
        /// Test Function Stop point.
        /// </summary>
        private const double StopD = 1;

        /// <summary>
        /// Test Function Start point.
        /// </summary>
        private const double StartE = 0;

        /// <summary>
        /// Test Function Stop point.
        /// </summary>
        private const double StopE = 1;

        /// <summary>
        /// Test Function Start point.
        /// </summary>
        private const double StartF = 0;

        /// <summary>
        /// Test Function Stop point.
        /// </summary>
        private const double StopF = double.PositiveInfinity;

        /// <summary>
        /// Test Function Start point.
        /// </summary>
        private const double StartG = 0;

        /// <summary>
        /// Test Function Stop point.
        /// </summary>
        private const double StopG = 1;

        /// <summary>
        /// Target area square.
        /// </summary>
        private const double TargetAreaA = 9.1082396073229965070;

        /// <summary>
        /// Target area.
        /// </summary>
        private const double TargetAreaB = 11.7078776759298776163;

        /// <summary>
        /// Target area.
        /// </summary>
        private const double TargetAreaC = Constants.Pi;

        /// <summary>
        /// Target area.
        /// </summary>
        private const double TargetAreaD = -1;

        /// <summary>
        /// Target area.
        /// </summary>
        private const double TargetAreaE = 2;

        /// <summary>
        /// Target area.
        /// </summary>
        private const double TargetAreaF = 0.5;

        /// <summary>
        /// Target area.
        /// </summary>
        private const double TargetAreaG = 1.1981402347355922074;

        /// <summary>
        /// Test Integrate facade for simple use cases.
        /// </summary>
        [Test]
        public void TestIntegrateFacade()
        {
            // TargetFunctionA
            // integral_(0)^(10) exp(-x/5) (2 + sin(2 x)) dx = 9.1082 

            Assert.AreEqual(
                TargetAreaA,
                Integrate.OnClosedInterval(TargetFunctionA, StartA, StopA),
                1e-5,
                "Interval, Target 1e-08");

            Assert.AreEqual(
                TargetAreaA,
                Integrate.OnClosedInterval(TargetFunctionA, StartA, StopA, 1e-10),
                1e-10,
                "Interval, Target 1e-10");

            // TargetFunctionB
            // integral_(0)^(1) integral_(0)^(10) exp(-x/5) (2 + sin(2 y)) dx dy = 11.7079 

            Assert.AreEqual(
                Integrate.OnRectangle(TargetFunctionB, StartA, StopA, StartB, StopB),
                TargetAreaB,
                1e-12,
                "Rectangle, order 32");

            Assert.AreEqual(
                Integrate.OnRectangle(TargetFunctionB, StartA, StopA, StartB, StopB, 22),
                TargetAreaB,
                1e-10,
                "Rectangle, Order 22");

            // TargetFunctionC
            // integral_(-oo)^(oo) 1/(1 + x^2) dx = pi

            Assert.AreEqual(
                TargetAreaC,
                Integrate.DoubleExponential(TargetFunctionC, StartC, StopC),
                1e-5,
                "DoubleExponential, 1/(1 + x^2)");

            Assert.AreEqual(
                TargetAreaC,
                Integrate.DoubleExponential(TargetFunctionC, StartC, StopC, 1e-10),
                1e-10,
                "DoubleExponential, 1/(1 + x^2)");

            // TargetFunctionD
            // integral_(0)^(1) log(x) dx = -1

            Assert.AreEqual(
                TargetAreaD,
                Integrate.DoubleExponential(TargetFunctionD, StartD, StopD),
                1e-10,
                "DoubleExponential, log(x)");
            
            Assert.AreEqual(
                TargetAreaD,
                Integrate.GaussLegendre(TargetFunctionD, StartD, StopD, order: 1024),
                1e-10,
                "GaussLegendre, log(x), order 1024");

            Assert.AreEqual(
                TargetAreaD,
                Integrate.GaussKronrod(TargetFunctionD, StartD, StopD, 1e-10, order: 15),
                1e-10,
                "GaussKronrod, log(x), order 15");
            Assert.AreEqual(
                TargetAreaD,
                Integrate.GaussKronrod(TargetFunctionD, StartD, StopD, 1e-10, order: 21),
                1e-10,
                "GaussKronrod, log(x), order 21");
            Assert.AreEqual(
                TargetAreaD,
                Integrate.GaussKronrod(TargetFunctionD, StartD, StopD, 1e-10, order: 31),
                1e-10,
                "GaussKronrod, log(x), order 31");
            Assert.AreEqual(
                TargetAreaD,
                Integrate.GaussKronrod(TargetFunctionD, StartD, StopD, 1e-10, order: 41),
                1e-10,
                "GaussKronrod, log(x), order 41");
            Assert.AreEqual(
                TargetAreaD,
                Integrate.GaussKronrod(TargetFunctionD, StartD, StopD, 1e-10, order: 51),
                1e-10,
                "GaussKronrod, log(x), order 51");
            Assert.AreEqual(
                TargetAreaD,
                Integrate.GaussKronrod(TargetFunctionD, StartD, StopD, 1e-10, order: 61),
                1e-10,
                "GaussKronrod, log(x), order 61");

            double error, L1;
            var Q = Integrate.GaussKronrod(TargetFunctionD, StartD, StopD, out error, out L1, 1e-10, order: 15);
            Assert.AreEqual(
                Math.Abs(TargetAreaD),
                Math.Abs(L1),
                1e-10,
                "GaussKronrod, L1");

            // TargetFunctionE
            // integral_(0)^(1) log^2(x) dx = 2

            Assert.AreEqual(
                TargetAreaE,
                Integrate.DoubleExponential(TargetFunctionE, StartE, StopE),
                1e-10,
                "DoubleExponential, log^2(x)");
            
            Assert.AreEqual(
                TargetAreaE,
                Integrate.GaussLegendre(TargetFunctionE, StartE, StopE, order: 128),
                1e-5,
                "GaussLegendre, log^2(x), order 128");

            Assert.AreEqual(
                TargetAreaE,
                Integrate.GaussKronrod(TargetFunctionE, StartE, StopE, 1e-10, order: 15),
                1e-10,
                "GaussKronrod, log^2(x), order 15");

            // TargetFunctionF
            // integral_(0)^(oo) exp(-x) cos(x) dx = 1/2

            Assert.AreEqual(
               TargetAreaF,
               Integrate.DoubleExponential(TargetFunctionF, StartF, StopF),
               1e-10,
               "DoubleExponential, e^(-x) cos(x)");

            Assert.AreEqual(
                TargetAreaF,
                Integrate.GaussLegendre(TargetFunctionF, StartF, StopF, order: 128),
                1e-10,
                "GaussLegendre, e^(-x) cos(x), order 128");

            Assert.AreEqual(
                TargetAreaF,
                Integrate.GaussKronrod(TargetFunctionF, StartF, StopF, 1e-10, order: 15),
                1e-10,
                "GaussKronrod, e^(-x) cos(x), order 15");

            // TargetFunctionG
            // integral_(0)^(1) sqrt(x)/sqrt(1 - x^2) dx = 1.19814

            Assert.AreEqual(
               TargetAreaG,
               Integrate.DoubleExponential(TargetFunctionG, StartG, StopG),
               1e-5,
               "DoubleExponential, sqrt(x)/sqrt(1 - x^2)");

            Assert.AreEqual(
                TargetAreaG,
                Integrate.GaussLegendre(TargetFunctionG, StartG, StopG, order: 128),
                1e-10,
                "GaussLegendre, sqrt(x)/sqrt(1 - x^2), order 128");

            Assert.AreEqual(
                TargetAreaG,
                Integrate.GaussKronrod(TargetFunctionG, StartG, StopG, 1e-10, order: 15),
                1e-10,
                "GaussKronrod, sqrt(x)/sqrt(1 - x^2), order 15");
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
                Assert.AreEqual(gaussLegendre.GetAbscissa(i), abscissa[i]);
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

        /// <summary>
        /// Gauss-Kronrod rule supports integration.
        /// </summary>
        /// <param name="order">Defines an Nth order Gauss-Kronrod rule. The order also defines the number of abscissas and weights for the rule.</param>
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(101)]
        [TestCase(201)]
        public void TestGaussKronrodRuleIntegration(int order)
        {
            double appoximateArea = GaussKronrodRule.Integrate(TargetFunctionA, StartA, StopA, out _, out _, order: order);
            double relativeError = Math.Abs(TargetAreaA - appoximateArea) / TargetAreaA;
            Assert.Less(relativeError, 5e-16);
        }

        // integral_(-oo)^(oo) exp(-x^2/2) dx = sqrt(2 ¥ð)
        // integral_(-oo)^(0) exp(-x^2/2) dx = sqrt(¥ð/2)
        // integral_(0)^(oo exp(-x^2/2) dx = sqrt(¥ð/2)
        // integral_(-1)^(1) exp(-x^2/2) dx = sqrt(2 ¥ð) erf(1/sqrt(2))
        // integral_(1)^(0) exp(-x^2/2) dx = -sqrt(¥ð/2) erf(1/sqrt(2))
        [TestCase(double.NegativeInfinity, double.PositiveInfinity, Constants.Sqrt2Pi)]        
        [TestCase(double.NegativeInfinity, 0, Constants.SqrtPiOver2)]        
        [TestCase(0, double.PositiveInfinity, Constants.SqrtPiOver2)]        
        [TestCase(-1, 1, 1.7112487837842976063)]        
        [TestCase(1, 0, -0.85562439189214880317)]
        public void TestIntegralOfGaussian(double a, double b, double expected)
        {
            Assert.AreEqual(
                expected,
                Integrate.DoubleExponential((x) => Math.Exp(-x * x / 2), a, b),
                1e-10,
                "DET Integral of e^(-x^2 /2) from {0} to {1}", a, b);

            Assert.AreEqual(
                expected,
                Integrate.GaussKronrod((x) => Math.Exp(-x * x / 2), a, b),
                1e-10,
                "GK Integral of e^(-x^2 /2) from {0} to {1}", a, b);

            Assert.AreEqual(
                expected,
                Integrate.GaussLegendre((x) => Math.Exp(-x * x / 2), a, b, order: 128),
                1e-10,
                "GL Integral of e^(-x^2 /2) from {0} to {1}", a, b);
        }

        // integral_(-oo)^(oo) sin(pi x) / (pi x) dx = 1 / pi integral_(-oo)^(oo) sin(x) / x dx
        //                                           = 1 / pi integral_(oo)^(oo) 1 / (1 + t^2) dt
        //                                           = 1
        //                                        or = 2 / pi integral_(0)^(oo) 1 / (1 + t^2) dt
        //                                        or = 2 / pi integral_(-oo)^(0) 1 / (1 + t^2) dt
        [TestCase(double.NegativeInfinity, double.PositiveInfinity, 1, Constants.InvPi)]
        [TestCase(0, double.PositiveInfinity, 1, Constants.TwoInvPi)]
        [TestCase(double.NegativeInfinity, 0, 1, Constants.TwoInvPi)]
        public void TestIntegralOfSinc(double a, double b, double expected, double factor)
        {
            Assert.AreEqual(
                expected,
                factor * Integrate.DoubleExponential((x) => 1 / (1 + x * x), a, b),
                1e-10,
                "DET Integral of sin(pi*x)/(pi*x) from {0} to {1}", a, b);

            Assert.AreEqual(
                expected,
                factor * Integrate.GaussKronrod((x) => 1 / (1 + x * x), a, b),
                1e-10,
                "GK Integral of sin(pi*x)/(pi*x) from {0} to {1}", a, b);

            Assert.AreEqual(
                expected,
                factor * Integrate.GaussLegendre((x) => 1 / (1 + x * x), a, b, order: 128),
                1e-10,
                "GL Integral of sin(pi*x)/(pi*x) from {0} to {1}", a, b);
        }

        // integral_(-oo)^(oo) 1/(1 + j x^2) dx = -(-1)^(3/4) ¥ð
        // integral_(0)^(oo) 1/(1 + j x^2) dx = -1/2 (-1)^(3/4) ¥ð
        // integral_(-oo)^(0) 1/(1 + j x^2) dx = -1/2 (-1)^(3/4) ¥ð
        [TestCase(double.NegativeInfinity, double.PositiveInfinity, 2.2214414690791831235, -2.2214414690791831235)]        
        [TestCase(0, double.PositiveInfinity, 1.1107207345395915618, -1.1107207345395915618)]        
        [TestCase(double.NegativeInfinity, 0, 1.1107207345395915618, -1.1107207345395915618)]
        public void TestContourIntegral(double a, double b, double r, double i)
        {
            var expected = new Complex(r, i);            
            var actualDET = ContourIntegrate.DoubleExponential((x) => 1 / new Complex(1, x * x), a, b);
            var actualGK = ContourIntegrate.GaussKronrod((x) => 1 / new Complex(1, x * x), a, b);
            var actualGL = ContourIntegrate.GaussLegendre((x) => 1 / new Complex(1, x * x), a, b, order: 128);

            Assert.AreEqual(
               expected.Real,
               actualDET.Real,
               1e-10,
               "DET Integral of Re[e^(-x^2 /2) / (1 + j e^x)] from {0} to {1}", a, b);

            Assert.AreEqual(
               expected.Imaginary,
               actualDET.Imaginary,
               1e-10,
               "DET Integral of Im[e^(-x^2 /2) / (1 + j e^x)] from {0} to {1}", a, b);

            Assert.AreEqual(
               expected.Real,
               actualGK.Real,
               1e-10,
               "GK Integral of Re[e^(-x^2 /2) / (1 + j e^x)] from {0} to {1}", a, b);

            Assert.AreEqual(
               expected.Imaginary,
               actualGK.Imaginary,
               1e-10,
               "GK Integral of Im[e^(-x^2 /2) / (1 + j e^x)] from {0} to {1}", a, b);

            Assert.AreEqual(
              expected.Real,
              actualGL.Real,
              1e-10,
              "GL Integral of Re[e^(-x^2 /2) / (1 + j e^x)] from {0} to {1}", a, b);

            Assert.AreEqual(
               expected.Imaginary,
               actualGL.Imaginary,
               1e-10,
               "GL Integral of Im[e^(-x^2 /2) / (1 + j e^x)] from {0} to {1}", a, b);
        }
    }
}
