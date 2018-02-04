// <copyright file="DirichletTests.cs" company="Math.NET">
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
using System.Linq;
using MathNet.Numerics.Distributions;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.DistributionTests.Multivariate
{
    using Random = System.Random;

    /// <summary>
    /// Dirichlet distribution tests
    /// </summary>
    [TestFixture, Category("Distributions")]
    public class DirichletTests
    {
        /// <summary>
        /// Set-up test parameters.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        /// <summary>
        /// Can create symmetric Dirichlet.
        /// </summary>
        [Test]
        public void CanCreateSymmetricDirichlet()
        {
            var d = new Dirichlet(0.3, 5);

            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(0.3, d.Alpha[i]);
            }
        }

        /// <summary>
        /// Can create dirichlet.
        /// </summary>
        [Test]
        public void CanCreateDirichlet()
        {
            var alpha = new double[10];
            for (var i = 0; i < 10; i++)
            {
                alpha[i] = i;
            }

            var d = new Dirichlet(alpha);

            for (var i = 0; i < 5; i++)
            {
                Assert.AreEqual(i, d.Alpha[i]);
            }
        }

        /// <summary>
        /// Fail create dirichlet with bad parameters.
        /// </summary>
        [Test]
        public void FailCreateDirichlet()
        {
            Assert.That(() => new Dirichlet(0.0, 5), Throws.ArgumentException);
            Assert.That(() => new Dirichlet(-0.1, 5), Throws.ArgumentException);
        }

        /// <summary>
        /// Has random source.
        /// </summary>
        [Test]
        public void HasRandomSource()
        {
            var d = new Dirichlet(0.3, 5);
            Assert.IsNotNull(d.RandomSource);
        }

        /// <summary>
        /// Can set random source.
        /// </summary>
        [Test]
        public void CanSetRandomSource()
        {
            GC.KeepAlive(new Dirichlet(0.3, 5)
            {
                RandomSource = new Random(0)
            });
        }

        [Test]
        public void HasRandomSourceEvenAfterSetToNull()
        {
            var d = new Dirichlet(0.3, 5);
            Assert.DoesNotThrow(() => d.RandomSource = null);
            Assert.IsNotNull(d.RandomSource);
        }

        /// <summary>
        /// Can get dimension.
        /// </summary>
        [Test]
        public void CanGetDimension()
        {
            var d = new Dirichlet(0.3, 10);
            Assert.AreEqual(10, d.Dimension);
        }

        /// <summary>
        /// Can get alpha.
        /// </summary>
        [Test]
        public void CanGetAlpha()
        {
            var d = new Dirichlet(0.3, 10);
            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(0.3, d.Alpha[i]);
            }
        }

        /// <summary>
        /// Validate mean.
        /// </summary>
        [Test]
        public void ValidateMean()
        {
            var d = new Dirichlet(0.3, 5);

            for (var i = 0; i < 5; i++)
            {
                AssertHelpers.AlmostEqualRelative(0.3 / 1.5, d.Mean[i], 15);
            }
        }

        /// <summary>
        /// Validate variance.
        /// </summary>
        [Test]
        public void ValidateVariance()
        {
            var alpha = new double[10];
            var sum = 0.0;
            for (var i = 0; i < 10; i++)
            {
                alpha[i] = i;
                sum += i;
            }

            var d = new Dirichlet(alpha);
            for (var i = 0; i < 10; i++)
            {
                AssertHelpers.AlmostEqualRelative(i * (sum - i) / (sum * sum * (sum + 1.0)), d.Variance[i], 15);
            }
        }

        /// <summary>
        /// Validate density.
        /// </summary>
        /// <param name="x">Alphas array.</param>
        /// <param name="res">Expected value.</param>
        /// <remarks>
        /// Mathematica: InputForm[PDF[DirichletDistribution[{0.1, 0.3, 0.5, 0.8}], {0.01, 0.03, 0.5}]]
        /// </remarks>
        [TestCase(new[] { 0.01, 0.03, 0.5 }, 18.77225681167061)]
        [TestCase(new[] { 0.1, 0.2, 0.3, 0.4 }, 0.8314656481199253)]
        public void ValidateDensity(double[] x, double res)
        {
            var d = new Dirichlet(new[] { 0.1, 0.3, 0.5, 0.8 });
            AssertHelpers.AlmostEqualRelative(res, d.Density(x), 12);
        }

        /// <summary>
        /// Validate density log.
        /// </summary>
        /// <param name="x">Alpha array.</param>
        [TestCase(new[] { 0.01, 0.03, 0.5, 0.5 })]
        [TestCase(new[] { 0.1, 0.2, 0.3, 0.4 })]
        public void ValidateDensityLn(double[] x)
        {
            var d = new Dirichlet(new[] { 0.1, 0.3, 0.5, 0.8 });
            AssertHelpers.AlmostEqualRelative(d.DensityLn(x), Math.Log(d.Density(x)), 12);
        }

        /// <summary>
        /// Validate density log matches Beta for 2-dimension cases
        /// </summary>
        /// <param name="x">Alpha array.</param>
        [TestCase(0.01)]
        [TestCase(0.1)]
        [TestCase(0.4)]
        [TestCase(0.71)]
        public void ValidateBetaSpecialCaseDensityLn(double x)
        {
            var d = new Dirichlet(new[] { 0.1, 0.3 });
            var beta = new Beta(0.1, 0.3);
            AssertHelpers.AlmostEqualRelative(d.DensityLn(new[] { x }), beta.DensityLn(x), 10);
        }

        /// <summary>
        /// Validate entropy.
        /// </summary>
        /// <param name="x">Alpha array.</param>
        [TestCase(new[] { 0.1, 0.3, 0.5, 0.8 })]
        [TestCase(new[] { 0.1, 0.2, 0.3, 0.4 })]
        public void ValidateEntropy(double[] x)
        {
            var d = new Dirichlet(x);

            var sum = x.Sum(t => (t - 1) * SpecialFunctions.DiGamma(t));
            var res = SpecialFunctions.GammaLn(x.Sum()) + ((x.Sum() - x.Length) * SpecialFunctions.DiGamma(x.Sum())) - sum;
            AssertHelpers.AlmostEqualRelative(res, d.Entropy, 12);
        }

        /// <summary>
        /// Can sample symmetric dirichlet.
        /// </summary>
        [Test]
        public void CanSampleSymmetricDirichlet()
        {
            var d = new Dirichlet(1.0, 5);
            d.Sample();
        }

        /// <summary>
        /// Can sample singular dirichlet.
        /// </summary>
        [Test]
        public void CanSampleSingularDirichlet()
        {
            var d = new Dirichlet(new[] { 2.0, 1.0, 0.0, 3.0 });
            d.Sample();
        }
    }
}
