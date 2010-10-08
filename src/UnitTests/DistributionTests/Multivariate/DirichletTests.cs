// <copyright file="DirichletTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Multivariate
{
	using System;
	using System.Linq;
	using MbUnit.Framework;
	using Distributions;

	[TestFixture]
    public class DirichletTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test]
        public void CanCreateSymmetricDirichlet()
        {
            Dirichlet d = new Dirichlet(0.3, 5);

            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(0.3, d.Alpha[i]);
            }
        }

        [Test]
        public void CanCreateDirichlet()
        {
            double[] alpha = new double[10];
            for (int i = 0; i < 10; i++)
            {
                alpha[i] = i;
            }

            Dirichlet d = new Dirichlet(alpha);

            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(i, d.Alpha[i]);
            }
        }

        [Test]
        [Row(0.0)]
        [Row(-0.1)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailCreateDirichlet(double alpha)
        {
            Dirichlet d = new Dirichlet(alpha, 5);
        }

        [Test]
        public void HasRandomSource()
        {
            Dirichlet d = new Dirichlet(0.3, 5);
            Assert.IsNotNull(d.RandomSource);
        }

        [Test]
        public void CanSetRandomSource()
        {
            Dirichlet d = new Dirichlet(0.3, 5);
            d.RandomSource = new Random();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FailSetRandomSourceWithNullReference()
        {
            Dirichlet d = new Dirichlet(0.3, 5);
            d.RandomSource = null;
        }

        [Test]
        public void CanGetDimension()
        {
            Dirichlet d = new Dirichlet(0.3, 10);
            Assert.AreEqual(10, d.Dimension);
        }

        [Test]
        public void CanGetAlpha()
        {
            Dirichlet d = new Dirichlet(0.3, 10);

            double[] alpha = new double[10];
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(0.3, d.Alpha[i]);
            }
        }

        [Test]
        public void CanSetAlpha()
        {
            Dirichlet d = new Dirichlet(0.3, 10);

            double[] alpha = new double[10];
            for (int i = 0; i < 10; i++)
            {
                alpha[i] = i;
            }

            d.Alpha = alpha;
        }

        [Test]
        public void ValidateMean()
        {
            Dirichlet d = new Dirichlet(0.3, 5);

            for (int i = 0; i < 5; i++)
            {
                AssertHelpers.AlmostEqual(0.3/1.5, d.Mean[i], 15);
            }
        }

        [Test]
        public void ValidateVariance()
        {
            double[] alpha = new double[10];
            double sum = 0.0;
            for (int i = 0; i < 10; i++)
            {
                alpha[i] = i;
                sum += i;
            }

            Dirichlet d = new Dirichlet(alpha);

            for (int i = 0; i < 10; i++)
            {
                AssertHelpers.AlmostEqual(i * (sum - i) / (sum * sum * (sum + 1.0)), d.Variance[i], 15);
            }
        }

        [Test]
        [Row(new[] { 0.01, 0.03, 0.5 }, 1335.32600710379)]
        [Row(new[] { 0.1, 0.2, 0.3, 0.4 }, 59.1446044600076)]
        public void ValidateDensity(double[] x, double res)
        {
            var d = new Dirichlet(new[] { 0.1, 0.3, 0.5, 0.8 });
            AssertHelpers.AlmostEqual(res, d.Density(x), 12);
        }

        [Test]
        [Row(new[] { 0.01, 0.03, 0.5 })]
        [Row(new[] { 0.1, 0.2, 0.3, 0.4 })]
        public void ValidateDensityLn(double[] x)
        {
            var d = new Dirichlet(new[] { 0.1, 0.3, 0.5, 0.8 });
            AssertHelpers.AlmostEqual(d.DensityLn(x), Math.Log(d.Density(x)), 12);
        }

        [Test]
        [Row(new[] { 0.1, 0.3, 0.5, 0.8 })]
        [Row(new[] { 0.1, 0.2, 0.3, 0.4 })]
        public void ValidateEntropy(double[] x)
        {
            var d = new Dirichlet(x);

            var sum = x.Sum(t => (t - 1) * SpecialFunctions.DiGamma(t));
            var res = SpecialFunctions.GammaLn(x.Sum()) + (x.Sum() - x.Length) * SpecialFunctions.DiGamma(x.Sum()) - sum;
            AssertHelpers.AlmostEqual(res, d.Entropy, 12);
        }

        [Test]
        public void CanSampleSymmetricDirichlet()
        {
            Dirichlet d = new Dirichlet(1.0, 5);
            double[] s = d.Sample();
        }

        [Test]
        public void CanSampleSingularDirichlet()
        {
            Dirichlet d = new Dirichlet(new double[] {2.0, 1.0, 0.0, 3.0});
            double[] s = d.Sample();
        }
    }
}