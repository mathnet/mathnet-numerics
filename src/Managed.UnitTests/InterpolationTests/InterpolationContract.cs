// <copyright file="InterpolationContracts.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.InterpolationTests
{
    using System;
    using System.Collections.Generic;
    using Gallio.Framework.Assertions;
    using Interpolation;
    using MbUnit.Framework;
    using MbUnit.Framework.ContractVerifiers;

    internal class InterpolationContract<TInterpolation> : AbstractContract
        where TInterpolation : IInterpolation
    {
        public Func<IList<double>, IList<double>, IInterpolation> Factory { get; set; }
        public int[] Order { get; set; }
        public bool PolynomialBehavior { get; set; }
        public bool RationalBehavior { get; set; }

        public InterpolationContract()
        {
            Order = new[] { 4 };
        }

        protected override IEnumerable<Test> GetContractVerificationTests()
        {
            yield return CreateFactoryReturnsCorrectTypeTest("FactoryReturnsCorrectType");
            yield return CreateInitChecksForNullTest("InitChecksForNull");
            yield return CreateInitChecksForMatchingCountTest("InitChecksForMatchingCount");
            yield return CreateConstructorInitShortcutTest("ConstructorInitShortcut");
            yield return CreateConsistentCapabilityBehaviorTest("ConsistentCapabilityBehavior");
            yield return CreateInterpolationMatchesNodePointsTest("InterpolationMatchesNodePoints");
            yield return CreateLinearBehaviorTest("LinearBehavior");

            if (PolynomialBehavior)
            {
                yield return CreatePolynomialBehaviorTest("PolynomialBehavior");
            }

            if (RationalBehavior)
            {
                yield return CreateRationalBehaviorTest("RationalBehavior");
            }
        }

        private Test CreateFactoryReturnsCorrectTypeTest(string name)
        {
            return new TestCase(name, () =>
            {
                var interpolation = Factory(new List<double> { 1, 2, 3 }, new List<double> { 10, 20, 30 });

                AssertionHelper.Verify(() =>
                {
                    // verify returned interpolation has the expected type
                    if (interpolation.GetType() == typeof(TInterpolation))
                    {
                        return null;
                    }

                    return new AssertionFailureBuilder(
                        "Expected the factory to return the correct type.")
                        .AddRawLabeledValue("Interpolation Type", typeof(TInterpolation))
                        .SetStackTrace(Context.GetStackTraceData())
                        .ToAssertionFailure();
                });
            });
        }

        private Test CreateInitChecksForNullTest(string name)
        {
            return new TestCase(name, () =>
            {
                var points = new List<double> { 1, 2, 3, 4, 5 };
                var values = new List<double> { 10, 20, 30, 40, 50 };

                Assert.DoesNotThrow(() => Factory(points, values));

                Assert.Throws(typeof(ArgumentNullException), () => Factory(points, null));
                Assert.Throws(typeof(ArgumentNullException), () => Factory(null, values));
                Assert.Throws(typeof(ArgumentNullException), () => Factory(null, null));
            });
        }

        private Test CreateInitChecksForMatchingCountTest(string name)
        {
            return new TestCase(name, () =>
            {
                var points = new List<double> { 1, 2, 3, 4, 5 };
                var valuesOk = new List<double> { 10, 20, 30, 40, 50 };
                var valuesFail1 = new List<double> { 10, 20, 30, 40 };
                var valuesFail2 = new List<double> { 10, 20, 30, 40, 50, 60 };

                Assert.DoesNotThrow(() => Factory(points, valuesOk));

                Assert.Throws(typeof(ArgumentException), () => Factory(points, valuesFail1));
                Assert.Throws(typeof(ArgumentException), () => Factory(points, valuesFail2));
            });
        }

        private Test CreateConstructorInitShortcutTest(string name)
        {
            return new TestCase(name, () =>
            {
                var points = new List<double> { 1, 2, 3, 4, 5 };
                var values = new List<double> { 10, 20, 30, 40, 50 };

                var ctor = typeof(TInterpolation).GetConstructor(
                    new[] { typeof (IList<double>), typeof (IList<double>) }
                    );

                var interpolation = (IInterpolation)ctor.Invoke(
                    new[] { points, values }
                    );

                Assert.AreApproximatelyEqual(20, interpolation.Interpolate(2), 1e-12);
            });
        }

        private Test CreateConsistentCapabilityBehaviorTest(string name)
        {
            return new TestCase(name, () =>
            {
                var interpolation = Factory(new List<double> { 1, 2, 3 }, new List<double> { 10, 20, 30 });

                // verify consistent differentiation capability
                if (interpolation.SupportsDifferentiation)
                {
                    double a, b;
                    Assert.DoesNotThrow(() => interpolation.Differentiate(1.2));
                    Assert.DoesNotThrow(() => interpolation.Differentiate(1.2, out a, out b));
                }
                else
                {
                    double a, b;

                    Assert.Throws(
                        typeof(NotSupportedException),
                        () => interpolation.Differentiate(1.2));

                    Assert.Throws(
                        typeof(NotSupportedException),
                        () => interpolation.Differentiate(1.2, out a, out b));
                }

                // verify consistent integration capability
                if (interpolation.SupportsIntegration)
                {
                    Assert.DoesNotThrow(() => interpolation.Integrate(1.2));
                }
                else
                {
                    Assert.Throws(
                        typeof(NotSupportedException),
                        () => interpolation.Integrate(1.2));
                }
            });
        }

        private Test CreateInterpolationMatchesNodePointsTest(string name)
        {
            return new TestCase(name, () =>
            {
                var points = new List<double> { 1, 2, 2.3, 3, 8 };
                var values = new List<double> { 50, 20, 30, 10, -20 };
                var interpolation = Factory(points, values);

                for (int i = 0; i < points.Count; i++)
                {
                    Assert.AreApproximatelyEqual(
                        values[i],
                        interpolation.Interpolate(points[i]),
                        1e-12);
                }
            });
        }

        private Test CreateLinearBehaviorTest(string name)
        {
            return new TestCase(name, () =>
            {
                const double yOffset = 2.0;
                const double xOffset = 4.0;
                Random random = new Random();

                for (int k = 0; k < Order.Length; k++)
                {
                    int order = Order[k];

                    // build linear samples
                    double[] points = new double[order];
                    double[] values = new double[order];
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i] = xOffset + i;
                        values[i] = yOffset + i;
                    }

                    var interpolation = Factory(points, values);

                    // build linear test vectors randomly between the sample points
                    double[] testPoints = new double[order + 1];
                    double[] testValues = new double[order + 1];
                    if (order == 1)
                    {
                        testPoints[0] = xOffset - random.NextDouble();
                        testPoints[1] = xOffset + random.NextDouble();
                        testValues[0] = testValues[1] = yOffset;
                    }
                    else
                    {
                        for (int i = 0; i < testPoints.Length; i++)
                        {
                            double z = (i - 1) + random.NextDouble();
                            testPoints[i] = xOffset + z;
                            testValues[i] = yOffset + z;
                        }
                    }

                    // verify interpolation with test samples
                    for (int i = 0; i < testPoints.Length; i++)
                    {
                        Assert.AreApproximatelyEqual(
                            testValues[i],
                            interpolation.Interpolate(testPoints[i]),
                            1e-12);
                    }
                }
            });
        }

        private Test CreatePolynomialBehaviorTest(string name)
        {
            return new TestCase(name, () =>
            {
                var points = new List<double> { -2.0, -1.0, 0.0, 1.0, 2.0 };
                var values = new List<double> { 1.0, 2.0, -1.0, 0.0, 1.0 };
                var interpolation = Factory(points, values);

                // Maple: "with(CurveFitting);"
                // Maple: "PolynomialInterpolation([[-2,1],[-1,2],[0,-1],[1,0],[2,1]], x);"
                Assert.AreApproximatelyEqual(-4.5968, interpolation.Interpolate(-2.4), 1e-6, "A -2.4");
                Assert.AreApproximatelyEqual(1.65395, interpolation.Interpolate(-0.9), 1e-6, "A -0.9");
                Assert.AreApproximatelyEqual(0.21875, interpolation.Interpolate(-0.5), 1e-6, "A -0.5");
                Assert.AreApproximatelyEqual(-0.84205, interpolation.Interpolate(-0.1), 1e-6, "A -0.1");
                Assert.AreApproximatelyEqual(-1.10805, interpolation.Interpolate(0.1), 1e-6, "A 0.1");
                Assert.AreApproximatelyEqual(-1.1248, interpolation.Interpolate(0.4), 1e-6, "A 0.4");
                Assert.AreApproximatelyEqual(0.5392, interpolation.Interpolate(1.2), 1e-6, "A 1.2");
                Assert.AreApproximatelyEqual(-4431.0, interpolation.Interpolate(10.0), 1e-6, "A 10.0");
                Assert.AreApproximatelyEqual(-5071.0, interpolation.Interpolate(-10.0), 1e-6, "A -10.0");
            });
        }

        private Test CreateRationalBehaviorTest(string name)
        {
            return new TestCase(name, () =>
            {
                double[] points, values;
                SampleFunctionEquidistant(t => 1 / (1 + (t * t)), -5.0, 5.0, 41, out points, out values);
                var interpolation = Factory(points, values);

                for (int i = 0; i < points.Length; i++)
                {
                    Assert.AreApproximatelyEqual(
                        values[i],
                        interpolation.Interpolate(points[i]),
                        1e-12,
                        "Match on knots");
                }

                double[] testPoints, testValues;
                SampleFunctionEquidistant(t => 1 / (1 + (t * t)), -5.0, 5.0, 81, out testPoints, out testValues);

                for (int i = 0; i < testPoints.Length; i++)
                {
                    Assert.AreApproximatelyEqual(
                        testValues[i],
                        interpolation.Interpolate(testPoints[i]),
                        1e-5,
                        "Match between knots");
                }
            });
        }

        private static void SampleFunctionEquidistant(
            Func<double, double> f,
            double start,
            double stop,
            int samples,
            out double[] points,
            out double[] values)
        {
            points = new double[samples];
            values = new double[samples];

            double step = (stop - start) / (samples - 1);
            for (int i = 0; i < points.Length; i++)
            {
                double t = start + (i * step);
                points[i] = t;
                values[i] = f(t);
            }
        }
    }
}
