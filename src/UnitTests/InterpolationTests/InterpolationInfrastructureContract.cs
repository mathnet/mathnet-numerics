// <copyright file="InterpolationInfrastructureContract.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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
    using System.Linq.Expressions;
    using Interpolation;
    using MbUnit.Framework;
    using MbUnit.Framework.ContractVerifiers;

    internal class InterpolationInfrastructureContract<TInterpolation> : AbstractContract
        where TInterpolation : IInterpolation
    {
        public Func<IInterpolation>[] UninitializedFactories { get; set; }
        public Expression<Func<IInterpolation>>[] InitializedFactories { get; set; }

        public int MinimumSampleCount { get; set; }

        protected override IEnumerable<Test> GetContractVerificationTests()
        {
            yield return CreateLoadUninitializedFactoryTest();
            yield return CreateLoadInitializedFactoryTest();

            yield return CreateInitializedFactoryChecksForNullTest();
            yield return CreateInitializedFactoryChecksForToFewSamplesTest();
            yield return CreateInitializedFactoryChecksForCountMismatchTest();

            yield return CreateConsistentCapabilityBehaviorTest();
        }

        private Test CreateLoadUninitializedFactoryTest()
        {
            return new TestCase(
                "LoadUninitializedFactory",
                () =>
                {
                    Assert.IsNotNull(UninitializedFactories);
                    Assert.LessThan(0, UninitializedFactories.Length);

                    foreach (var factory in UninitializedFactories)
                    {
                        var interpolation = factory();

                        Assert.IsNotNull(interpolation);
                        Assert.IsInstanceOfType(typeof(TInterpolation), interpolation);
                    }
                });
        }

        private Test CreateLoadInitializedFactoryTest()
        {
            return new TestCase(
                "LoadInitializedFactory",
                () =>
                {
                    Assert.IsNotNull(InitializedFactories);
                    Assert.LessThan(0, InitializedFactories.Length);

                    foreach (var factory in InitializedFactories)
                    {
                        var interpolation = factory.Compile()();

                        Assert.IsNotNull(interpolation);
                        Assert.IsInstanceOfType(typeof(TInterpolation), interpolation);
                        Assert.DoesNotThrow(() => interpolation.Interpolate(0.0));
                    }
                });
        }

        private Test CreateInitializedFactoryChecksForNullTest()
        {
            return new TestCase(
                "InitializedFactoryChecksForNull",
                () =>
                {
                    foreach (var factory in InitializedFactories)
                    {
                        // we only support method calls and constructors for now.
                        if (factory.Body.NodeType != ExpressionType.New
                            && factory.Body.NodeType != ExpressionType.Call)
                        {
                            Assert.Fail(
                                "Factory '{0}' is neither a constructor or a method call.",
                                factory.ToString());
                            continue;
                        }

                        var modifiedFactories = factory.ApplySingleMapEachArgument<IInterpolation>(
                            t => !t.IsValueType,
                            e => Expression.Constant(null, e.Type));

                        foreach (var modifiedFactory in modifiedFactories)
                        {
                            var closureFactoryReference = modifiedFactory;

                            Assert.Throws(
                                typeof(ArgumentNullException),
                                () => closureFactoryReference.Compile()(),
                                "Factory must check for null arguments ({0})",
                                closureFactoryReference.ToString());
                        }
                    }
                });
        }

        private Test CreateInitializedFactoryChecksForToFewSamplesTest()
        {
            return new TestCase(
                "InitializedFactoryChecksForToFewSamples",
                () =>
                {
                    foreach (var factory in InitializedFactories)
                    {
                        // we only support method calls and constructors for now.
                        if (factory.Body.NodeType != ExpressionType.New
                            && factory.Body.NodeType != ExpressionType.Call)
                        {
                            Assert.Fail(
                                "Factory '{0}' is neither a constructor or a method call.",
                                factory.ToString());
                            continue;
                        }

                        var modifiedFactories = factory.ApplySingleMapEachArgument<IInterpolation>(
                            t => !t.IsValueType,
                            e => Expression.Constant(new double[MinimumSampleCount - 1], typeof(double[])));

                        foreach (var modifiedFactory in modifiedFactories)
                        {
                            var closureFactoryReference = modifiedFactory;

                            Assert.Throws(
                                typeof(ArgumentException),
                                () => closureFactoryReference.Compile()(),
                                "Factory must check to ensure there are enough samples ({0})",
                                closureFactoryReference.ToString());
                        }
                    }
                });
        }

        private Test CreateInitializedFactoryChecksForCountMismatchTest()
        {
            return new TestCase(
                "InitializedFactoryChecksForCountMismatch",
                () =>
                {
                    foreach (var factory in InitializedFactories)
                    {
                        // we only support method calls and constructors for now.
                        if (factory.Body.NodeType != ExpressionType.New
                            && factory.Body.NodeType != ExpressionType.Call)
                        {
                            Assert.Fail(
                                "Factory '{0}' is neither a constructor or a method call.",
                                factory.ToString());
                            continue;
                        }

                        // mismatch doesn't make sense when there are less than two list arguments.
                        int listCount = factory.ApplyReduceArgument(
                            (e, count) => typeof(IList<double>).IsAssignableFrom(e.Type) ? count + 1 : count,
                            0);

                        if (listCount < 2)
                        {
                            continue;
                        }

                        var modifiedFactories = factory.ApplySingleMapEachArgument<IInterpolation>(
                            t => !t.IsValueType,
                            e =>
                            {
                                // add a single entry to the end of the list
                                var originalList = Expression.Lambda<Func<double[]>>(e).Compile()();
                                var newList = new double[originalList.Length + 1];
                                originalList.CopyTo(newList, 0);
                                newList[newList.Length - 1] = -1;
                                return Expression.Constant(newList, e.Type);
                            });

                        foreach (var modifiedFactory in modifiedFactories)
                        {
                            var closureFactoryReference = modifiedFactory;

                            Assert.Throws(
                                typeof(ArgumentException),
                                () => closureFactoryReference.Compile()(),
                                "Factory must check for matching sample lengths ({0})",
                                closureFactoryReference.ToString());
                        }
                    }
                });
        }

        private Test CreateConsistentCapabilityBehaviorTest()
        {
            return new TestCase(
                "ConsistentCapabilityBehavior",
                () =>
                {
                    var interpolation = InitializedFactories[0].Compile()();

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
    }
}
