// <copyright file="SystemRandomExtensionTests.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.RandomTests
{
    using System;
    using System.Threading;
    using MbUnit.Framework;

	public abstract class RandomTests
    {
        private const int _n = 10000;
        private readonly Type _randomType;

        protected RandomTests(Type randomType)
        {
            _randomType = randomType;
        }

        [Test]
        public void Sample()
        {
            System.Random random = (System.Random) Activator.CreateInstance(_randomType, new object[] {false});
            double sum = 0;
            for (int i = 0; i < _n; i++)
            {
                double next = random.NextDouble();
                sum += next;
                Assert.IsTrue(next >= 0);
                Assert.IsTrue(next <= 1);
            }
            //make sure are within 10% of the expected sum.
            Assert.IsTrue(sum >= _n / 2.0 - .05 * _n);
            Assert.IsTrue(sum <= _n / 2.0 + .05 * _n);
            if (random is IDisposable)
            {
                ((IDisposable) random).Dispose();
            }
        }

        [Test]
        public void ThreadSafeSample()
        {
            System.Random random = (System.Random) Activator.CreateInstance(_randomType, new object[] {true});

            Thread t1 = new Thread(runTest);
            Thread t2 = new Thread(runTest);
            t1.Start(random);
            t2.Start(random);
            t1.Join();
            t2.Join();
        }

        public void runTest(object random)
        {
            System.Random rng = (System.Random) random;
            for (int i = 0; i < _n; i++)
            {
                double next = rng.NextDouble();
                Assert.IsTrue(next >= 0);
                Assert.IsTrue(next <= 1);
            }
        }
    }
}