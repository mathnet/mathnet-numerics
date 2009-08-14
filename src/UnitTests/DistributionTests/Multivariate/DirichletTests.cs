// <copyright file="DirichletTests.cs" company="Math.NET">
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

namespace dnAnalytics.Tests.Statistics.Distributions
{
    using dnAnalytics.Statistics.Distributions;
    using NUnit.Framework;

    [TestFixture]
    public class DirichletTests
    {
        private const double mAcceptableError = 1e-12;

        [Test]
        public void SymmetricDirichlet()
        {
            Dirichlet d = new Dirichlet(0.3, 5);

            for (int i = 0; i < 5; i++)
            {
                Assert.AreEqual(0.3, d.Mean[i], mAcceptableError);
                Assert.AreEqual(0.3 * (1.5 - 0.3) / (1.5 * 1.5 * 2.5), d.Variance[i], mAcceptableError);
            }
        }

        [Test]
        public void GetSetRNG()
        {
            Dirichlet d = new Dirichlet(0.3, 5);

            // Try getting the random number generator.
            System.Random rnd = d.RandomNumberGenerator;
            // Try setting the random number generator.
            d.RandomNumberGenerator = new System.Random();
        }
    }
}