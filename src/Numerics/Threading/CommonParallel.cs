// <copyright file="CommonParallel.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MathNet.Numerics.Threading
{
    /// <summary>
    /// Used to simplify parallel code, particularly between the .NET 4.0 and Silverlight Code.
    /// </summary>
    internal static class CommonParallel
    {
        static ParallelOptions CreateParallelOptions()
        {
            return new ParallelOptions
            {
                MaxDegreeOfParallelism = Control.MaxDegreeOfParallelism,
                TaskScheduler = Control.TaskScheduler,
            };
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The body to be invoked for each iteration range.</param>
        public static void For(int fromInclusive, int toExclusive, Action<int, int> body)
        {
            For(fromInclusive, toExclusive, Math.Max(1, (toExclusive - fromInclusive)/Control.MaxDegreeOfParallelism), body);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel.
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="rangeSize">The partition size for splitting work into smaller pieces.</param>
        /// <param name="body">The body to be invoked for each iteration range.</param>
        public static void For(int fromInclusive, int toExclusive, int rangeSize, Action<int, int> body)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (fromInclusive < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fromInclusive));
            }

            if (fromInclusive > toExclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(toExclusive));
            }

            if (rangeSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rangeSize));
            }

            var length = toExclusive - fromInclusive;

            // Special case: nothing to do
            if (length <= 0)
            {
                return;
            }

            // Special case: not worth to parallelize, inline
            if (Control.MaxDegreeOfParallelism < 2 || (rangeSize*2) > length)
            {
                body(fromInclusive, toExclusive);
                return;
            }

            // Common case
            Parallel.ForEach(
                Partitioner.Create(fromInclusive, toExclusive, rangeSize),
                CreateParallelOptions(),
                range => body(range.Item1, range.Item2));
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
            if (Control.MaxDegreeOfParallelism < 2)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i]();
                }

                return;
            }

            // Common case
            Parallel.Invoke(
                CreateParallelOptions(),
                actions);
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
                throw new ArgumentNullException(nameof(select));
            }

            if (reduce == null)
            {
                throw new ArgumentNullException(nameof(reduce));
            }

            // Special case: no action
            if (fromInclusive >= toExclusive)
            {
                return reduce(Array.Empty<T>());
            }

            // Special case: single action, inline
            if (fromInclusive == (toExclusive - 1))
            {
                return reduce(new[] { select(fromInclusive) });
            }

            // Special case: straight execution without parallelism
            if (Control.MaxDegreeOfParallelism < 2)
            {
                var mapped = new T[toExclusive - fromInclusive];
                for (int k = 0; k < mapped.Length; k++)
                {
                    mapped[k] = select(k + fromInclusive);
                }

                return reduce(mapped);
            }

            // Common case
            var intermediateResults = new List<T>();
            var syncLock = new object();
            Parallel.ForEach(
                Partitioner.Create(fromInclusive, toExclusive),
                CreateParallelOptions(),
                () => new List<T>(),
                (range, _, localData) =>
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
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <param name="array">The array to iterate over.</param>
        /// <param name="select">The function to select items over a subset.</param>
        /// <param name="reduce">The function to select the item of selection from the subsets.</param>
        /// <returns>The selected value.</returns>
        public static TOut Aggregate<T, TOut>(T[] array, Func<int, T, TOut> select, Func<TOut[], TOut> reduce)
        {
            if (select == null)
            {
                throw new ArgumentNullException(nameof(select));
            }

            if (reduce == null)
            {
                throw new ArgumentNullException(nameof(reduce));
            }

            // Special case: no action
            if (array == null || array.Length == 0)
            {
                return reduce(Array.Empty<TOut>());
            }

            // Special case: single action, inline
            if (array.Length == 1)
            {
                return reduce(new[] { select(0, array[0]) });
            }

            // Special case: straight execution without parallelism
            if (Control.MaxDegreeOfParallelism < 2)
            {
                var mapped = new TOut[array.Length];
                for (int k = 0; k < mapped.Length; k++)
                {
                    mapped[k] = select(k, array[k]);
                }

                return reduce(mapped);
            }

            // Common case
            var intermediateResults = new List<TOut>();
            var syncLock = new object();
            Parallel.ForEach(
                Partitioner.Create(0, array.Length),
                CreateParallelOptions(),
                () => new List<TOut>(),
                (range, _, localData) =>
                {
                    var mapped = new TOut[range.Item2 - range.Item1];
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
        public static TOut Aggregate<T, TOut>(T[] array, Func<int, T, TOut> select, Func<TOut, TOut, TOut> reducePair, TOut reduceDefault)
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

                TOut result = results[0];
                for (int i = 1; i < results.Length; i++)
                {
                    result = reducePair(result, results[i]);
                }

                return result;
            });
        }
    }
}
