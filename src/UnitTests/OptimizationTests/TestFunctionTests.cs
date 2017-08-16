// <copyright file="TestFunctionTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions;
using NUnit.Framework;
using MathNet.Numerics.LinearAlgebra;
using System.Collections;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class TestFunctionTests
    {
        private static IEnumerable<TestFunctions.TestCase> MghCases
        {
            get
            {
                return Enumerable.Empty<TestFunctions.TestCase>()
                    .Concat(RosenbrockFunction2.TestCases)
                    .Concat(BealeFunction.TestCases)
                    .Concat(HelicalValleyFunction.TestCases)
                    .Concat(MeyerFunction.TestCases)
                    .Concat(PowellSingularFunction.TestCases)
                    .Concat(WoodFunction.TestCases)
                    .Concat(BrownAndDennisFunction.TestCases);
            }
        }

        private class MghCaseEnumerator : IEnumerable<TestCaseData>
        {
            public string CategoryName { get; protected set; }

            public MghCaseEnumerator(string category_name)
            {
                this.CategoryName = category_name;
            }

            public virtual IEnumerator<TestCaseData> GetEnumerator()
            {
                return MghCases
                    .Select(x =>
                        new TestCaseData(x)
                            .SetName($"{x.FullName} {this.CategoryName}")
                    ).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        [Test]
        public void Smoke_Construction()
        {
            var c = new TestCase()
            {
                InitialGuess = new double[] { 1, 2, 3 },
                MinimizingPoint = new double[] { 1, 1, 1 },
                MinimalValue = 0
            };
        }

        private class ValueAtMinimumSource : MghCaseEnumerator
        {
            public ValueAtMinimumSource() : base("ValueAtMinimum") { }

            public override IEnumerator<TestCaseData> GetEnumerator()
            {
                return MghCases
                    .Where(x => x.MinimizingPoint != null)
                    .Select(x =>
                        new TestCaseData(x)
                            .SetName($"{x.FullName} {this.CategoryName}")
                    )
                    .GetEnumerator();
            }
        }

        [Test]
        [TestCaseSource(typeof(ValueAtMinimumSource))]
        public void ValueAtMinimum(TestFunctions.TestCase test_case)
        {
            if (test_case.MinimizingPoint != null)
            {
                var value_at_minimum = test_case.Function.SsqValue(test_case.MinimizingPoint);
                Assert.That(
                    Math.Abs(value_at_minimum - test_case.MinimalValue) < 1e-3,
                    $"Function value at minimum not as expected."
                );
            }
        }

        private class GradientAtStartSource : MghCaseEnumerator
        {
            public GradientAtStartSource() : base("GradientAtStart") { }
        }

        [Test]
        [TestCaseSource(typeof(GradientAtStartSource))]
        public void GradientAtStart(TestFunctions.TestCase test_case)
        {
            var a_grad = test_case.Function.SsqGradient(test_case.InitialGuess);
            var fd_grad = Vector<double>.Build.Dense(test_case.Function.ParameterDimension, 0.0);

            for (int ii = 0; ii < test_case.Function.ParameterDimension; ++ii)
            {
                var h = 1e-6;

                var bump_up = test_case.InitialGuess.Clone();
                bump_up[ii] += h;
                var bump_down = test_case.InitialGuess.Clone();
                bump_down[ii] -= h;

                var up_val = test_case.Function.SsqValue(bump_up);
                var down_val = test_case.Function.SsqValue(bump_down);

                fd_grad[ii] = 0.5 * (up_val - down_val) / h;
            }

            for (int ii = 0; ii < test_case.Function.ParameterDimension; ++ii)
            {
                var val1 = a_grad[ii];
                var val2 = fd_grad[ii];
                var min_abs_val = Math.Min(Math.Abs(val1), Math.Abs(val2));
                if (min_abs_val <= 1)
                    Assert.That(Math.Abs(val1 - val2) < 1e-3, $"Problem with gradient value at start point.");
                else
                    Assert.That(Math.Abs(val1 - val2) / min_abs_val < 1e-3, $"Problem with gradient value at start point.");
            }
        }

        private class HessianAtStartSource : MghCaseEnumerator
        {
            public HessianAtStartSource() : base("HessianAtStart") { }

            public override IEnumerator<TestCaseData> GetEnumerator()
            {
                return MghCases
                    .Where(x => x.MinimizingPoint != null)
                    .Select(x =>
                        new TestCaseData(x)
                            .SetName($"{x.FullName} {this.CategoryName}")
                    )
                    .GetEnumerator();
            }
        }
        [Test]
        [TestCaseSource(typeof(HessianAtStartSource))]
        public void HessianAtStart(TestFunctions.TestCase test_case)
        {
            var a_hess = test_case.Function.SsqHessian(test_case.InitialGuess);
            var fd_hess = Matrix<double>.Build.Dense(test_case.Function.ParameterDimension, test_case.Function.ParameterDimension);
            for (int ii = 0; ii < test_case.Function.ParameterDimension; ++ii)
            {
                for (int jj = 0; jj < test_case.Function.ParameterDimension; ++jj)
                {
                    var h1 = 1e-3 * Math.Max(1.0, Math.Abs(test_case.InitialGuess[ii]));
                    var h2 = 1e-3 * Math.Max(1.0, Math.Abs(test_case.InitialGuess[jj]));

                    var bump_uu = test_case.InitialGuess.Clone();
                    bump_uu[ii] += h1;
                    bump_uu[jj] += h2;

                    var bump_dd = test_case.InitialGuess.Clone();
                    bump_dd[ii] -= h1;
                    bump_dd[jj] -= h2;

                    var bump_ud = test_case.InitialGuess.Clone();
                    bump_ud[ii] += h1;
                    bump_ud[jj] -= h2;

                    var bump_du = test_case.InitialGuess.Clone();
                    bump_du[ii] -= h1;
                    bump_du[jj] += h2;

                    var val_uu = test_case.Function.SsqValue(bump_uu);
                    var val_dd = test_case.Function.SsqValue(bump_dd);
                    var val_ud = test_case.Function.SsqValue(bump_ud);
                    var val_du = test_case.Function.SsqValue(bump_du);

                    fd_hess[ii, jj] = (val_uu - val_ud + val_dd - val_du) / (4 * h1 * h2);
                }
            }

            for (int ii = 0; ii < test_case.Function.ParameterDimension; ++ii)
            {
                for (int jj = 0; jj < test_case.Function.ParameterDimension; ++jj)
                {
                    var val1 = fd_hess[ii, jj];
                    var val2 = a_hess[ii, jj];

                    var abs_min = Math.Min(Math.Abs(val1), Math.Abs(val2));
                    if (abs_min <= 1)
                    {
                        Assert.That(Math.Abs(val1 - val2) < 1e-3, $"Problem with hessian at start point.");
                    }
                    else
                    {
                        Assert.That(Math.Abs(val1 - val2) / abs_min < 0.05, $"Problem with hessian at start point.");
                    }
                }
            }
        }

        private class ItemGradientAtStartSource : MghCaseEnumerator
        {
            public ItemGradientAtStartSource() : base("ItemGradientAtStart") { }
        }

        [Test]
        [TestCaseSource(typeof(ItemGradientAtStartSource))]
        public void ItemGradientAtStart(TestFunctions.TestCase test_case)
        {
            for (var item_index = 0; item_index < test_case.Function.ItemDimension; ++item_index)
            {

                var a_grad = test_case.Function.ItemGradient(test_case.InitialGuess, item_index);
                var h = 1e-4;
                var fd_grad = Vector<double>.Build.Dense(test_case.Function.ParameterDimension, 0.0);

                for (int ii = 0; ii < test_case.Function.ParameterDimension; ++ii)
                {
                    var bump_up = test_case.InitialGuess.Clone();
                    bump_up[ii] += h;
                    var bump_down = test_case.InitialGuess.Clone();
                    bump_down[ii] -= h;

                    var up_val = test_case.Function.ItemValue(bump_up, item_index);
                    var down_val = test_case.Function.ItemValue(bump_down, item_index);

                    fd_grad[ii] = 0.5 * (up_val - down_val) / h;
                }

                for (int ii = 0; ii < test_case.Function.ParameterDimension; ++ii)
                {
                    Assert.That(Math.Abs(fd_grad[ii] - a_grad[ii]) < 1e-3, $"Failed for parameter {ii}");
                }
            }
        }

        private class ItemHessianAtStartSource : MghCaseEnumerator
        {
            public ItemHessianAtStartSource() : base("ItemHessianAtStart") { }
        }

        [Test]
        [TestCaseSource(typeof(ItemHessianAtStartSource))]
        public void ItemHessianAtStart(TestFunctions.TestCase test_case)
        {
            for (var item_index = 0; item_index < test_case.Function.ItemDimension; ++item_index)
            {
                var a_hess = test_case.Function.ItemHessian(test_case.InitialGuess, item_index);
                var h = 1e-4;
                var fd_hess = Matrix<double>.Build.Dense(test_case.Function.ParameterDimension, test_case.Function.ParameterDimension);
                for (int ii = 0; ii < test_case.Function.ParameterDimension; ++ii)
                {
                    for (int jj = 0; jj < test_case.Function.ParameterDimension; ++jj)
                    {
                        var bump_uu = test_case.InitialGuess.Clone();
                        bump_uu[ii] += h;
                        bump_uu[jj] += h;

                        var bump_dd = test_case.InitialGuess.Clone();
                        bump_dd[ii] -= h;
                        bump_dd[jj] -= h;

                        var bump_ud = test_case.InitialGuess.Clone();
                        bump_ud[ii] += h;
                        bump_ud[jj] -= h;

                        var bump_du = test_case.InitialGuess.Clone();
                        bump_du[ii] -= h;
                        bump_du[jj] += h;

                        var val_uu = test_case.Function.ItemValue(bump_uu, item_index);
                        var val_dd = test_case.Function.ItemValue(bump_dd, item_index);
                        var val_ud = test_case.Function.ItemValue(bump_ud, item_index);
                        var val_du = test_case.Function.ItemValue(bump_du, item_index);

                        fd_hess[ii, jj] = (val_uu - val_ud + val_dd - val_du) / (4 * h * h);
                    }
                }

                for (int ii = 0; ii < test_case.Function.ParameterDimension; ++ii)
                {
                    for (int jj = 0; jj < test_case.Function.ParameterDimension; ++jj)
                    {
                        var val1 = fd_hess[ii, jj];
                        var val2 = a_hess[ii, jj];

                        var abs_min = Math.Min(Math.Abs(val1), Math.Abs(val2));
                        if (abs_min <= 1)
                        {
                            Assert.That(Math.Abs(val1 - val2) < 1e-3, $"Problem with hessian at start point.");
                        }
                        else
                        {
                            Assert.That(Math.Abs(val1 - val2) / abs_min < 0.05, $"Problem with hessian at start point.");
                        }
                    }
                }
            }
        }

    }
}
