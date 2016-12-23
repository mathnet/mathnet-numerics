// <copyright file="RandomTests.cs" company="Math.NET">
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
using System.Threading;
using MathNet.Numerics.Random;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.Random
{
    /// <summary>
    /// Abstract class fro RNG tests.
    /// </summary>
    public abstract class RandomTests
    {
        /// <summary>
        /// Number of samples.
        /// </summary>
        const int N = 10000;

        /// <summary>
        /// Random generator type.
        /// </summary>
        readonly Type _randomType;

        /// <summary>
        /// Initializes a new instance of the RandomTests class.
        /// </summary>
        /// <param name="randomType">Random generator type</param>
        protected RandomTests(Type randomType)
        {
            _randomType = randomType;
        }

        /// <summary>
        /// Can sample.
        /// </summary>
        [Test]
        public void Sample()
        {
            var random = (System.Random)Activator.CreateInstance(_randomType, new object[] { false });
            double sum = 0;
            for (var i = 0; i < N; i++)
            {
                var next = random.NextDouble();
                sum += next;
                Assert.IsTrue(next >= 0);
                Assert.IsTrue(next <= 1);
            }

            // make sure are within 10% of the expected sum.
            Assert.IsTrue(sum >= (N/2.0) - (.05*N));
            Assert.IsTrue(sum <= (N/2.0) + (.05*N));

            var disposable = random as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
		
		/// <summary>
        ///     Next() result is in boundaries.
        /// </summary>
        [Test]
        public void Boundaries()
        {
            var random = (System.Random)Activator.CreateInstance(_randomType, new object[] { false });

            for (var i = 1; i < N; i++)
            {
                var j = N;
                var next = random.Next(i, j);
                Assert.IsTrue(next >= i, string.Format("Value {0} is smaller than lower bound {1}", next, i));
                Assert.IsTrue(next < j, string.Format("Value {0} is larger or equal to upper bound {1}", next, j));
            }

            var disposable = random as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        [Test]
        public void Reproducible()
        {
#if !PORTABLE
            if (_randomType == typeof (CryptoRandomSource))
            {
                Assert.Ignore("CryptoRandomSource does not support seeds and is not reproducible by design.");
            }
#endif

            Assert.That(
                ((RandomSource)Activator.CreateInstance(_randomType, new object[] { 5, false })).NextDoubles(1000),
                Is.EqualTo(((RandomSource)Activator.CreateInstance(_randomType, new object[] { 5, false })).NextDoubles(1000)).Within(1e-12).AsCollection);
        }

        /// <summary>
        /// Can thread-safe sample
        /// </summary>
        [Test]
        public void ThreadSafeSample()
        {
            var random = (System.Random)Activator.CreateInstance(_randomType, new object[] { true });

            var t1 = new Thread(RunTest);
            var t2 = new Thread(RunTest);
            t1.Start(random);
            t2.Start(random);
            t1.Join();
            t2.Join();
        }

        /// <summary>
        /// Test runner function.
        /// </summary>
        /// <param name="random">RNG object.</param>
        public void RunTest(object random)
        {
            var rng = (System.Random)random;
            for (var i = 0; i < N; i++)
            {
                var next = rng.NextDouble();
                Assert.IsTrue(next >= 0);
                Assert.IsTrue(next <= 1);
            }
        }
    }
}
