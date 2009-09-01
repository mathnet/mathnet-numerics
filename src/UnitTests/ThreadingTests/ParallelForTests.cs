// <copyright file="ParallelTest.cs" company="Math.NET">
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

namespace MathNet.Numerics.UnitTests.ThreadingTests
{
    using System;
    using System.Threading;
    using MbUnit.Framework;
    using Threading;

    [TestFixture]
    public class ParallelForTests
    {
        [Test, ApartmentState(ApartmentState.MTA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 101)]
        public void ParallelForInvokesEveryItemOnceMTAOnePerCore(int count)
        {
            var items = new int[count];

            // ensure One-Per-Core
            ThreadQueue.Start(Environment.ProcessorCount);

            Parallel.For(0, count, i => items[i]++);
            Parallel.For(0, count, i => items[i] += 1000);

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1001, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.STA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 101)]
        public void ParallelForInvokesEveryItemOnceSTAOnePerCore(int count)
        {
            var items = new int[count];

            // ensure One-Per-Core
            ThreadQueue.Start(Environment.ProcessorCount);

            Parallel.For(0, count, i => items[i]++);
            Parallel.For(0, count, i => items[i] += 1000);

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1001, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.MTA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 101)]
        public void ParallelForInvokesEveryItemOnceMTATwoPerCore(int count)
        {
            var items = new int[count];

            // ensure Two-Per-Core
            ThreadQueue.Start(2 * Environment.ProcessorCount);

            Parallel.For(0, count, i => items[i]++);
            Parallel.For(0, count, i => items[i] += 1000);

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1001, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.STA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 101)]
        public void ParallelForInvokesEveryItemOnceSTATwoPerCore(int count)
        {
            var items = new int[count];

            // ensure Two-Per-Core
            ThreadQueue.Start(2 * Environment.ProcessorCount);

            Parallel.For(0, count, i => items[i]++);
            Parallel.For(0, count, i => items[i] += 1000);

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1001, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.MTA)]
        public void DoesNotGetConfusedByMultipleStartShutdown()
        {
            ThreadQueue.Shutdown();
            ThreadQueue.Shutdown();

            ThreadQueue.Start(2);
            Assert.AreEqual(2, ThreadQueue.ThreadCount);

            Control.NumberOfParallelWorkerThreads = 2;
            Assert.AreEqual(2, ThreadQueue.ThreadCount);

            ThreadQueue.Start(4);
            Assert.AreEqual(4, ThreadQueue.ThreadCount);
            Assert.AreEqual(4, Control.NumberOfParallelWorkerThreads);

            ThreadQueue.Shutdown();
            ThreadQueue.Start();
            Assert.AreEqual(4, ThreadQueue.ThreadCount);

            ThreadQueue.Start(2);
            Assert.AreEqual(2, ThreadQueue.ThreadCount);

            var items = new int[50];

            Parallel.For(0, items.Length, i => items[i]++);
            Parallel.For(0, items.Length, i => items[i] += 1000);

            ThreadQueue.Shutdown();

            for(int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1001, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.MTA)]
        public void DoesDetectAndResolveRecursiveParallelization()
        {
            int countSharedBetweenClosures = 0;

            Assert.DoesNotThrow(
                () =>
                Parallel.For(
                    0,
                    10,
                    j => Interlocked.Increment(ref countSharedBetweenClosures)));

            Assert.AreEqual(10, countSharedBetweenClosures);
            countSharedBetweenClosures = 0;

            Parallel.For(
                0,
                10,
                i =>
                Parallel.For(
                    0,
                    10,
                    j => Interlocked.Increment(ref countSharedBetweenClosures)));

            Assert.AreEqual(100, countSharedBetweenClosures);
            countSharedBetweenClosures = 0;

            Parallel.For(
                0,
                10,
                i =>
                Parallel.For(
                    0,
                    10,
                    j =>
                    Parallel.For(
                        0,
                        10,
                        k => Interlocked.Increment(ref countSharedBetweenClosures))));

            Assert.AreEqual(1000, countSharedBetweenClosures);
        }




        [Test, ApartmentState(ApartmentState.MTA)]
       // [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 101)]
            [Column(10)]
        public void ParallelForInvokesEveryItemOnceMTAOnePerCoreWithIntialAndFinally(int count)
        {
            var items = new double[count];

            // ensure One-Per-Core
            ThreadQueue.Start(Environment.ProcessorCount);

            var sum = 0.0;
            var sync = new object();
            Parallel.For(
                0,
                count,
                () => 0.0,
                (i, localData) =>
                {
                    localData += 1;
                    Console.WriteLine(localData);
                    items[i] = localData;
                    return localData;
                },
                localResult =>
                {
                    lock (sync)
                    {
                        sum += localResult;
                    }
                }
                );

            for (var i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(i+1, items[i]);
            }
        }

     /*   [Test, ApartmentState(ApartmentState.STA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 101)]
        public void ParallelForInvokesEveryItemOnceSTAOnePerCore(int count)
        {
            var items = new int[count];

            // ensure One-Per-Core
            ThreadQueue.Start(Environment.ProcessorCount);

            Parallel.For(0, count, i => items[i]++);
            Parallel.For(0, count, i => items[i] += 1000);

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1001, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.MTA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 101)]
        public void ParallelForInvokesEveryItemOnceMTATwoPerCore(int count)
        {
            var items = new int[count];

            // ensure Two-Per-Core
            ThreadQueue.Start(2 * Environment.ProcessorCount);

            Parallel.For(0, count, i => items[i]++);
            Parallel.For(0, count, i => items[i] += 1000);

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1001, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.STA)]
        [Column(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 100, 101)]
        public void ParallelForInvokesEveryItemOnceSTATwoPerCore(int count)
        {
            var items = new int[count];

            // ensure Two-Per-Core
            ThreadQueue.Start(2 * Environment.ProcessorCount);

            Parallel.For(0, count, i => items[i]++);
            Parallel.For(0, count, i => items[i] += 1000);

            for (int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1001, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.MTA)]
        public void DoesNotGetConfusedByMultipleStartShutdown()
        {
            ThreadQueue.Shutdown();
            ThreadQueue.Shutdown();

            ThreadQueue.Start(2);
            Assert.AreEqual(2, ThreadQueue.ThreadCount);

            Control.NumberOfParallelWorkerThreads = 2;
            Assert.AreEqual(2, ThreadQueue.ThreadCount);

            ThreadQueue.Start(4);
            Assert.AreEqual(4, ThreadQueue.ThreadCount);
            Assert.AreEqual(4, Control.NumberOfParallelWorkerThreads);

            ThreadQueue.Shutdown();
            ThreadQueue.Start();
            Assert.AreEqual(4, ThreadQueue.ThreadCount);

            ThreadQueue.Start(2);
            Assert.AreEqual(2, ThreadQueue.ThreadCount);

            var items = new int[50];

            Parallel.For(0, items.Length, i => items[i]++);
            Parallel.For(0, items.Length, i => items[i] += 1000);

            ThreadQueue.Shutdown();

            for(int i = 0; i < items.Length; i++)
            {
                Assert.AreEqual(1001, items[i], i.ToString());
            }
        }

        [Test, ApartmentState(ApartmentState.MTA)]
        public void DoesDetectAndResolveRecursiveParallelization()
        {
            int countSharedBetweenClosures = 0;

            Assert.DoesNotThrow(
                () =>
                Parallel.For(
                    0,
                    10,
                    j => Interlocked.Increment(ref countSharedBetweenClosures)));

            Assert.AreEqual(10, countSharedBetweenClosures);
            countSharedBetweenClosures = 0;

            Parallel.For(
                0,
                10,
                i =>
                Parallel.For(
                    0,
                    10,
                    j => Interlocked.Increment(ref countSharedBetweenClosures)));

            Assert.AreEqual(100, countSharedBetweenClosures);
            countSharedBetweenClosures = 0;

            Parallel.For(
                0,
                10,
                i =>
                Parallel.For(
                    0,
                    10,
                    j =>
                    Parallel.For(
                        0,
                        10,
                        k => Interlocked.Increment(ref countSharedBetweenClosures))));

            Assert.AreEqual(1000, countSharedBetweenClosures);
        }*/
    }
}