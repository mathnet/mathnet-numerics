// <copyright file="CommonParallel.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

namespace MathNet.Numerics.Threading
{
    using System;
    using System.Threading.Tasks;

#if !PORTABLE
    using System.Collections.Concurrent;
    using System.Collections.Generic;
#else
    using System.Linq;
    using Properties;
#endif

    /// <summary>
    /// Used to simplify parallel code, particularly between the .NET 4.0 and Silverlight Code.
    /// </summary>
    public static class CommonParallel
    {
        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The body to be invoked for each iteration.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="body"/> argument is <c>null</c>.</exception>
        /// <exception cref="AggregateException">At least one invocation of the body threw an exception.</exception>
        [Obsolete("Use a more efficient overload instead. Scheduled for removal in v3.0.")]
        public static void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            if (body == null) throw new ArgumentNullException("body");
            if (toExclusive <= fromInclusive) throw new ArgumentOutOfRangeException("toExclusive");

            int rangeSize = (toExclusive - fromInclusive)/(Control.NumberOfParallelWorkerThreads*2);
            rangeSize = Math.Max(rangeSize, 1);

            For(fromInclusive,
                toExclusive,
                rangeSize,
                (start, stop) =>
                    {
                        for (var i = start; i < stop; i++)
                        {
                            body(i);
                        }
                    });
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The body to be invoked for each iteration range.</param>
        public static void For(int fromInclusive, int toExclusive, Action<int, int> body)
        {
            For(fromInclusive, toExclusive, Math.Max(1, (toExclusive - fromInclusive)/Control.NumberOfParallelWorkerThreads), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The body to be invoked for each iteration range.</param>
        public static void For(int fromInclusive, int toExclusive, int rangeSize, Action<int, int> body)
        {
            if (body == null) throw new ArgumentNullException("body");
            if (fromInclusive < 0) throw new ArgumentOutOfRangeException("fromInclusive");
            if (fromInclusive > toExclusive) throw new ArgumentOutOfRangeException("toExclusive");
            if (rangeSize < 1) throw new ArgumentOutOfRangeException("rangeSize");

            var length = toExclusive - fromInclusive;

            // Special case: nothing to do
            if (length <= 0)
            {
                return;
            }

            var maxDegreeOfParallelism = Control.NumberOfParallelWorkerThreads;

            // Special case: not worth to parallelize, inline
            if (Control.DisableParallelization || maxDegreeOfParallelism < 2 || (rangeSize*2) > length)
            {
                body(fromInclusive, toExclusive);
                return;
            }

#if PORTABLE
            var tasks = new Task[Math.Min(maxDegreeOfParallelism, length/rangeSize)];
            rangeSize = (toExclusive - fromInclusive)/tasks.Length;

            // partition the jobs into separate sets for each but the last worked thread
            for (var i = 0; i < tasks.Length - 1; i++)
            {
                var start = fromInclusive + (i*rangeSize);
                var stop = fromInclusive + ((i + 1)*rangeSize);

                tasks[i] = Task.Factory.StartNew(() => body(start, stop));
            }

            // add another set for last worker thread
            tasks[tasks.Length - 1] =
                Task.Factory.StartNew(() => body(fromInclusive + ((tasks.Length - 1)*rangeSize), toExclusive));

            Task.WaitAll(tasks);
#else
            Parallel.ForEach(
                Partitioner.Create(fromInclusive, toExclusive, rangeSize),
                new ParallelOptions {MaxDegreeOfParallelism = maxDegreeOfParallelism},
                (range, loopState) => body(range.Item1, range.Item2));
#endif
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="array">The array to iterate over.</param>
        /// <param name="body">The body to be invoked for each iteration.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="body"/> argument is <c>null</c>.</exception>
        /// <exception cref="AggregateException">At least one invocation of the body threw an exception.</exception>
        [Obsolete("Use a more efficient overload instead. Scheduled for removal in v3.0.")]
        public static void For<T>(T[] array, Action<int, T> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            // Special case: no action
            if (array == null || array.Length == 0)
            {
                return;
            }

            // Special case: single action, inline
            if (array.Length == 0)
            {
                body(0, array[0]);
                return;
            }

            // Special case: straight execution without parallelism
            if (Control.DisableParallelization || Control.NumberOfParallelWorkerThreads < 2)
            {
                // efficient since the compiler can drop the range checks
                for (int i = 0; i < array.Length; i++)
                {
                    body(i, array[i]);
                }
                return;
            }

            // Common case
#if PORTABLE
            var tasks = new Task[Control.NumberOfParallelWorkerThreads];
            var size = array.Length / tasks.Length;

            // partition the jobs into separate sets for each but the last worked thread
            for (var i1 = 0; i1 < tasks.Length - 1; i1++)
            {
                var start = (i1 * size);
                var stop = ((i1 + 1) * size);

                tasks[i1] = Task.Factory.StartNew(() =>
                    {
                        for (int j = start; j < stop; j++)
                        {
                            body(j, array[j]);
                        }
                    });
            }

            // add another set for last worker thread
            tasks[tasks.Length - 1] = Task.Factory.StartNew(() =>
                {
                    for (int j = ((tasks.Length - 1) * size); j < array.Length; j++)
                    {
                        body(j, array[j]);
                    }
                });

            Task.WaitAll(tasks);
#else
            Parallel.ForEach(
                Partitioner.Create(0, array.Length),
                new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Control.NumberOfParallelWorkerThreads
                    },
                (range, loopState) =>
                    {
                        for (var i = range.Item1; i < range.Item2; i++)
                        {
                            body(i, array[i]);
                        }
                    });
#endif
        }

        /// <summary>
        /// Executes each of the provided actions inside a discrete, asynchronous task. 
        /// </summary>
        /// <param name="actions">An array of actions to execute.</param>
        /// <exception cref="ArgumentException">The actions array contains a <c>null</c> element.</exception>
        /// <exception cref="AggregateException">At least one invocation of the actions threw an exception.</exception>
        public static void Invoke(params Action[] actions)
        {
            // Special case: no action
            if (actions.Length == 0)
            {
                return;
            }

            // Special case: single action, inline
            if (actions.Length == 1)
            {
                actions[0]();
                return;
            }

            // Special case: straight execution without parallelism
            if (Control.DisableParallelization || Control.NumberOfParallelWorkerThreads < 2)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i]();
                }
                return;
            }

            // Common case
#if PORTABLE
            var tasks = new Task[actions.Length];
            for (var i = 0; i < tasks.Length; i++)
            {
                Action action = actions[i];
                if (action == null)
                {
                    throw new ArgumentException(String.Format(Resources.ArgumentItemNull, "actions"), "actions");
                }

                tasks[i] = Task.Factory.StartNew(action);
            }
            Task.WaitAll(tasks);
#else
            Parallel.Invoke(
                new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Control.NumberOfParallelWorkerThreads
                    },
                actions);
#endif
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop</param>
        /// <param name="select">The function to select items over a subset.</param>
        /// <param name="reduce">The function to select the item of selection from the subsets.</param>
        /// <returns>The selected value.</returns>
        public static T Aggregate<T>(int fromInclusive, int toExclusive, Func<int, T> select, Func<T[], T> reduce)
        {
            if (select == null)
            {
                throw new ArgumentNullException("select");
            }
            if (reduce == null)
            {
                throw new ArgumentNullException("reduce");
            }

            // Special case: no action
            if (fromInclusive >= toExclusive)
            {
                return reduce(new T[0]);
            }

            // Special case: single action, inline
            if (fromInclusive == (toExclusive - 1))
            {
                return reduce(new[] {select(fromInclusive)});
            }

            // Special case: straight execution without parallelism
            if (Control.DisableParallelization || Control.NumberOfParallelWorkerThreads < 2)
            {
                var mapped = new T[toExclusive - fromInclusive];
                for (int k = 0; k < mapped.Length; k++)
                {
                    mapped[k] = select(k + fromInclusive);
                }
                return reduce(mapped);
            }

#if PORTABLE
            var tasks = new Task<T>[Control.NumberOfParallelWorkerThreads];
            var size = (toExclusive - fromInclusive) / tasks.Length;

            // partition the jobs into separate sets for each but the last worked thread
            for (var i = 0; i < tasks.Length - 1; i++)
            {
                var start = fromInclusive + (i * size);
                var stop = fromInclusive + ((i + 1) * size);

                tasks[i] = Task.Factory.StartNew(() =>
                    {
                        var mapped = new T[stop - start];
                        for (int k = 0; k < mapped.Length; k++)
                        {
                            mapped[k] = select(k + start);
                        }
                        return reduce(mapped);
                    });
            }

            // add another set for last worker thread
            tasks[tasks.Length - 1] = Task.Factory.StartNew(() =>
                {
                    var start = fromInclusive + ((tasks.Length - 1) * size);
                    var mapped = new T[toExclusive - start];
                    for (int k = 0; k < mapped.Length; k++)
                    {
                        mapped[k] = select(k + start);
                    }
                    return reduce(mapped);
                });

            return Task.Factory
                .ContinueWhenAll(tasks, tsk => reduce(tsk.Select(t => t.Result).ToArray()))
                .Result;
#else
            var intermediateResults = new List<T>();
            var syncLock = new object();
            var maxThreads = Control.DisableParallelization ? 1 : Control.NumberOfParallelWorkerThreads;
            Parallel.ForEach(
                Partitioner.Create(fromInclusive, toExclusive),
                new ParallelOptions {MaxDegreeOfParallelism = maxThreads},
                () => new List<T>(),
                (range, loop, localData) =>
                    {
                        var mapped = new T[range.Item2 - range.Item1];
                        for (int k = 0; k < mapped.Length; k++)
                        {
                            mapped[k] = select(k + range.Item1);
                        }
                        localData.Add(reduce(mapped));
                        return localData;
                    },
                localResult =>
                    {
                        lock (syncLock)
                        {
                            intermediateResults.Add(reduce(localResult.ToArray()));
                        }
                    });
            return reduce(intermediateResults.ToArray());
#endif
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <param name="array">The array to iterate over.</param>
        /// <param name="select">The function to select items over a subset.</param>
        /// <param name="reduce">The function to select the item of selection from the subsets.</param>
        /// <returns>The selected value.</returns>
        public static U Aggregate<T, U>(T[] array, Func<int, T, U> select, Func<U[], U> reduce)
        {
            if (select == null)
            {
                throw new ArgumentNullException("select");
            }
            if (reduce == null)
            {
                throw new ArgumentNullException("reduce");
            }

            // Special case: no action
            if (array == null || array.Length == 0)
            {
                return reduce(new U[0]);
            }

            // Special case: single action, inline
            if (array.Length == 1)
            {
                return reduce(new[] {select(0, array[0])});
            }

            // Special case: straight execution without parallelism
            if (Control.DisableParallelization || Control.NumberOfParallelWorkerThreads < 2)
            {
                var mapped = new U[array.Length];
                for (int k = 0; k < mapped.Length; k++)
                {
                    mapped[k] = select(k, array[k]);
                }
                return reduce(mapped);
            }

#if PORTABLE
            var tasks = new Task<U>[Control.NumberOfParallelWorkerThreads];
            var size = array.Length / tasks.Length;

            // partition the jobs into separate sets for each but the last worked thread
            for (var i = 0; i < tasks.Length - 1; i++)
            {
                var start = (i * size);
                var stop = ((i + 1) * size);

                tasks[i] = Task.Factory.StartNew(() =>
                    {
                        var mapped = new U[stop - start];
                        for (int k = 0; k < mapped.Length; k++)
                        {
                            mapped[k] = select(k + start, array[k + start]);
                        }
                        return reduce(mapped);
                    });
            }

            // add another set for last worker thread
            tasks[tasks.Length - 1] = Task.Factory.StartNew(() =>
                {
                    var start = ((tasks.Length - 1) * size);
                    var mapped = new U[array.Length - start];
                    for (int k = 0; k < mapped.Length; k++)
                    {
                        mapped[k] = select(k + start, array[k + start]);
                    }
                    return reduce(mapped);
                });

            return Task.Factory
                .ContinueWhenAll(tasks, tsk => reduce(tsk.Select(t => t.Result).ToArray()))
                .Result;
#else
            var intermediateResults = new List<U>();
            var syncLock = new object();
            var maxThreads = Control.DisableParallelization ? 1 : Control.NumberOfParallelWorkerThreads;
            Parallel.ForEach(
                Partitioner.Create(0, array.Length),
                new ParallelOptions {MaxDegreeOfParallelism = maxThreads},
                () => new List<U>(),
                (range, loop, localData) =>
                    {
                        var mapped = new U[range.Item2 - range.Item1];
                        for (int k = 0; k < mapped.Length; k++)
                        {
                            mapped[k] = select(k + range.Item1, array[k + range.Item1]);
                        }
                        localData.Add(reduce(mapped));
                        return localData;
                    },
                localResult =>
                    {
                        lock (syncLock)
                        {
                            intermediateResults.Add(reduce(localResult.ToArray()));
                        }
                    });
            return reduce(intermediateResults.ToArray());
#endif
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop</param>
        /// <param name="select">The function to select items over a subset.</param>
        /// <param name="reducePair">The function to select the item of selection from the subsets.</param>
        /// <param name="reduceDefault">Default result of the reduce function on an empty set.</param>
        /// <returns>The selected value.</returns>
        public static T Aggregate<T>(int fromInclusive, int toExclusive, Func<int, T> select, Func<T, T, T> reducePair, T reduceDefault)
        {
            return Aggregate(fromInclusive, toExclusive, select, results =>
                {
                    if (results == null || results.Length == 0)
                    {
                        return reduceDefault;
                    }

                    if (results.Length == 1)
                    {
                        return results[0];
                    }

                    T result = results[0];
                    for (int i = 1; i < results.Length; i++)
                    {
                        result = reducePair(result, results[i]);
                    }
                    return result;
                });
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <param name="array">The array to iterate over.</param>
        /// <param name="select">The function to select items over a subset.</param>
        /// <param name="reducePair">The function to select the item of selection from the subsets.</param>
        /// <param name="reduceDefault">Default result of the reduce function on an empty set.</param>
        /// <returns>The selected value.</returns>
        public static U Aggregate<T, U>(T[] array, Func<int, T, U> select, Func<U, U, U> reducePair, U reduceDefault)
        {
            return Aggregate(array, select, results =>
                {
                    if (results == null || results.Length == 0)
                    {
                        return reduceDefault;
                    }

                    if (results.Length == 1)
                    {
                        return results[0];
                    }

                    U result = results[0];
                    for (int i = 1; i < results.Length; i++)
                    {
                        result = reducePair(result, results[i]);
                    }
                    return result;
                });
        }
    }
}