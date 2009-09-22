// <copyright file="VectorNormalTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests
{
    using System;
    using System.Linq;
    using MbUnit.Framework;
    using MathNet.Numerics.Distributions;

    [TestFixture]
    public class VectorNormalTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        //[Test]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        //public void NormalConstructorFail()
        //{
        //    Matrix cov = new DenseMatrix(new double[,] { { 1.0, 1.0 }, { -1.0, 2.0 } });
        //    Vector mean = new DenseVector(new double[] { 5.0, 5.0 });

        //    // Build a new vector normal distribution.
        //    VectorNormal normal = new VectorNormal(mean, cov);
        //}

        [Test]
        public void StandardNormal()
        {
            VectorNormal normal = new VectorNormal(5);

            // Test the mean.
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(0.0, normal.Mean[i]);
            }

            // Test the covariance.
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (i == j)
                    {
                        Assert.AreEqual(1.0, normal.Covariance[i, j]);
                    }
                    else
                    {
                        Assert.AreEqual(0.0, normal.Covariance[i, j]);
                    }
                }
            }

            // Test the pdf.
            Assert.AreEqual(0.010105326013812, normal.Density(new DenseVector(5, 0.0)), mAcceptableError);
            Assert.AreEqual(8.294956719377678e-004, normal.Density(new DenseVector(5, 1.0)), mAcceptableError);

            // Test the mode.
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(0.0, normal.Mode[i]);
            }

            // Test the median.
            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(0.0, normal.Median[i]);
            }

            // Test the entropy.
            Assert.AreEqual(7.094692666023364, normal.Entropy, mAcceptableError);
        }

        [Test]
        public void NormalFromCovariance()
        {
            Matrix cov = new DenseMatrix(new double[,] { { 1.0, 0.9 }, { 0.9, 1.0 } });
            Vector mean = new DenseVector(new double[] { 5.0, 5.0 });

            // Check that these are valid mean and covariances.
            Assert.DoesNotThrow(() => VectorNormal.CheckParameters(mean, cov));

            // Build a new vector normal distribution.
            VectorNormal normal = new VectorNormal(mean, cov);

            // Test the mean.
            Assert.AreEqual(5.0, normal.Mean[0]);
            Assert.AreEqual(5.0, normal.Mean[1]);

            // Test the covariance.
            Assert.AreEqual(1.0, normal.Covariance[0, 0]);
            Assert.AreEqual(0.9, normal.Covariance[0, 1]);
            Assert.AreEqual(0.9, normal.Covariance[1, 0]);
            Assert.AreEqual(1.0, normal.Covariance[1, 1]);

            // Test the mode.
            Assert.AreEqual(5.0, normal.Mode[0]);
            Assert.AreEqual(5.0, normal.Mode[1]);

            // Test the median.
            Assert.AreEqual(5.0, normal.Median[0]);
            Assert.AreEqual(5.0, normal.Median[1]);

            // Test the entropy.
            Assert.AreEqual(2.007511462998520, normal.Entropy, mAcceptableError);

            // Get the RNG.
            System.Random rnd = normal.RandomNumberGenerator;
        }








        [Test]
        public void HasRandomSource(int i)
        {
            VectorNormal d = new VectorNormal(0.3, 5);
            Assert.IsNotNull(d.RandomSource);
        }

        [Test]
        public void CanSetRandomSource(int i)
        {
            VectorNormal d = new VectorNormal(0.3, 5);
            d.RandomSource = new Random();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FailSetRandomSourceWithNullReference(int i)
        {
            VectorNormal d = new VectorNormal(0.3, 5);
            d.RandomSource = null;
        }

        [Test]
        public void CanGetDimension()
        {
            VectorNormal d = new VectorNormal(0.3, 10);
            Assert.AreEqual(10, d.Dimension);
        }

        [Test]
        public void ValidateMean()
        {
            VectorNormal d = new VectorNormal(0.3, 5);

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

            VectorNormal d = new VectorNormal(alpha);

            for (int i = 0; i < 10; i++)
            {
                AssertHelpers.AlmostEqual(i * (sum - i) / (sum * sum * (sum + 1.0)), d.Variance[i], 15);
            }
        }

        [Test]
        public void CanSampleVectorNormal()
        {
            VectorNormal d = new VectorNormal(1.0, 5);
            double[] s = d.Sample();
        }
    }
}