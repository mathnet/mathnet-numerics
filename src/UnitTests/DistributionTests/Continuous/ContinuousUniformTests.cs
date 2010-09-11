// <copyright file="ContinuousUniformTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.DistributionTests.Continuous
{
	using System;
	using System.Linq;
	using MbUnit.Framework;
	using Distributions;

	[TestFixture]
    public class ContinuousUniformTests
    {
        [SetUp]
        public void SetUp()
        {
            Control.CheckDistributionParameters = true;
        }

        [Test, MultipleAsserts]
        public void CanCreateContinuousUniform()
        {
            var n = new ContinuousUniform();
            Assert.AreEqual<double>(0.0, n.Lower);
            Assert.AreEqual<double>(1.0, n.Upper);
        }

        [Test, MultipleAsserts]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void CanCreateContinuousUniform(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual<double>(lower, n.Lower);
            Assert.AreEqual<double>(upper, n.Upper);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [Row(Double.NaN, 1.0)]
        [Row(1.0, Double.NaN)]
        [Row(Double.NaN, Double.NaN)]
        [Row(1.0, 0.0)]
        public void ContinuousUniformCreateFailsWithBadParameters(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
        }

        [Test]
        public void ValidateToString()
        {
            var n = new ContinuousUniform(1.0, 2.0);
            Assert.AreEqual<string>("ContinuousUniform(Lower = 1, Upper = 2)", n.ToString());
        }

        [Test]
        [Row(-10.0)]
        [Row(-0.0)]
        [Row(0.0)]
        [Row(0.1)]
        [Row(1.0)]
        public void CanSetLower(double lower)
        {
            var n = new ContinuousUniform();
            n.Lower = lower;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetLowerFail()
        {
            var n = new ContinuousUniform();
            n.Lower = 3.0;
        }

        [Test]
        [Row(1.0)]
        [Row(2.0)]
        [Row(12.0)]
        public void CanSetUpper(double upper)
        {
            var n = new ContinuousUniform();
            n.Upper = upper;
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetUpperFail()
        {
            var n = new ContinuousUniform();
            n.Upper = -1.0;
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateEntropy(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual<double>(Math.Log(upper - lower), n.Entropy);
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateSkewness(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual<double>(0.0, n.Skewness);
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateMode(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual<double>( (lower + upper) / 2.0 , n.Mode);
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateMedian(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual<double>((lower + upper) / 2.0, n.Median);
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateMinimum(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual<double>(lower, n.Minimum);
        }

        [Test]
        [Row(-0.0, 2.0)]
        [Row(0.0, 2.0)]
        [Row(0.1, 4.0)]
        [Row(1.0, 10.0)]
        [Row(10.0, 11.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateMaximum(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            Assert.AreEqual<double>(upper, n.Maximum);
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateDensity(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            for (int i = 0; i < 11; i++)
            {
                double x = i - 5.0;
                if(x >= lower && x <= upper)
                {
                    Assert.AreEqual<double>(1.0 / (upper - lower), n.Density(x));
                }
                else
                {
                    Assert.AreEqual<double>(0.0, n.Density(x));
                }
            }
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateDensityLn(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            for (int i = 0; i < 11; i++)
            {
                double x = i - 5.0;
                if (x >= lower && x <= upper)
                {
                    Assert.AreEqual<double>(-Math.Log(upper - lower), n.DensityLn(x));
                }
                else
                {
                    Assert.AreEqual<double>(double.NegativeInfinity, n.DensityLn(x));
                }
            }
        }

        [Test]
        public void CanSampleStatic()
        {
            var d = ContinuousUniform.Sample(new Random(), 0.0, 1.0);
        }

        [Test]
        public void CanSampleSequenceStatic()
        {
            var ied = ContinuousUniform.Samples(new Random(), 0.0, 1.0);
            var arr = ied.Take(5).ToArray();
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleStatic()
        {
            var d = ContinuousUniform.Sample(new Random(), 0.0, -1.0);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void FailSampleSequenceStatic()
        {
            var ied = ContinuousUniform.Samples(new Random(), 0.0, -1.0).First();
        }

        [Test]
        public void CanSample()
        {
            var n = new ContinuousUniform();
            var d = n.Sample();
        }

        [Test]
        public void CanSampleSequence()
        {
            var n = new ContinuousUniform();
            var ied = n.Samples();
            var e = ied.Take(5).ToArray();
        }

        [Test]
        [Row(0.0, 0.0)]
        [Row(0.0, 0.1)]
        [Row(0.0, 1.0)]
        [Row(0.0, 10.0)]
        [Row(-5.0, 100.0)]
        [Row(0.0, Double.PositiveInfinity)]
        public void ValidateCumulativeDistribution(double lower, double upper)
        {
            var n = new ContinuousUniform(lower, upper);
            for (int i = 0; i < 11; i++)
            {
                double x = i - 5.0;
                if (x <= lower)
                {
                    Assert.AreEqual<double>(0.0, n.CumulativeDistribution(x));
                }
                else if (x >= upper)
                {
                    Assert.AreEqual<double>(1.0, n.CumulativeDistribution(x));
                }
                else
                {
                    Assert.AreEqual<double>((x - lower) / (upper - lower), n.CumulativeDistribution(x));
                }
            }
        }
    }
}