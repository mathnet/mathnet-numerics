// <copyright file="CommonParallel.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2011 Math.NET
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

#if !SILVERLIGHT
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
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
        public static void For(int fromInclusive, int toExclusive, Action<int> body)
        {
            var parallel = true;
            if (Control.DisableParallelization || Control.NumberOfParallelWorkerThreads < 2)
            {
                parallel = false;
            }

            For(fromInclusive, toExclusive, body, parallel);
        }

        /// <summary>
        /// Executes a for loop in which iterations may run in parallel. 
        /// </summary>
        /// <param name="fromInclusive">The start index, inclusive.</param>
        /// <param name="toExclusive">The end index, exclusive.</param>
        /// <param name="body">The body to be invoked for each iteration.</param>
        /// <param name="parallel">Use multiple threads.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="body"/> argument is <c>null</c>.</exception>
        /// <exception cref="AggregateException">At least one invocation of the body threw an exception.</exception>
        public static void For(int fromInclusive, int toExclusive, Action<int> body, bool parallel)
        {
            if (parallel)
            {
#if SILVERLIGHT
                Parallel.For(fromInclusive, toExclusive, body);
#else
                Parallel.ForEach(
                    Partitioner.Create(fromInclusive, toExclusive),
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Control.NumberOfParallelWorkerThreads
                    },
                    (range, loopState) =>
                    {
                        for (var i = range.Item1; i < range.Item2; i++)
                        {
                            body(i);
                        }
                    });
#endif
            }
            else
            {
                for (var index = fromInclusive; index < toExclusive; index++)
                {
                    body(index);
                }
            }
        }

/*        /// <summary>
        /// Aggregates a function over a loop.
        /// </summary>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop</param>
        /// <param name="body">The function to aggregate.</param>
        /// <returns>The sum of the function over the loop.</returns>
        public static double Aggregate(int fromInclusive, int toExclusive, Func<int, double> body)
        {
            var sync = new object();
            var sum = 0.0;

#if SILVERLIGHT
            Parallel.For(
                fromInclusive, 
                toExclusive, 
                () => 0.0, 
                (i, localData) => localData += body(i), 
                localResult =>
                {
                    lock (sync)
                    {
                        sum += localResult;
                    }
                });
#else

            if (Control.DisableParallelization || Control.NumberOfParallelWorkerThreads < 2)
            {
                for (var index = fromInclusive; index < toExclusive; index++)
                {
                    sum += body(index);
                }
            }
            else
            {
                Parallel.ForEach(
                    Partitioner.Create(fromInclusive, toExclusive),
                    new ParallelOptions { MaxDegreeOfParallelism = Control.NumberOfParallelWorkerThreads },
                    () => 0.0,
                    (range, loopState, localData) =>
                    {
                        for (var i = range.Item1; i < range.Item2; i++)
                        {
                            localData += body(i);
                        }

                        return localData;
                    },
                    localResult =>
                    {
                        lock (sync)
                        {
                            sum += localResult;
                        }
                    });
            }
#endif
            return sum;
        }

        /// <summary>
        /// Aggregates a function over a loop.
        /// </summary>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop</param>
        /// <param name="body">The function to aggregate.</param>
        /// <returns>The sum of the function over the loop.</returns>
        public static float Aggregate(int fromInclusive, int toExclusive, Func<int, float> body)
        {
            var sync = new object();
            var sum = 0.0f;

#if SILVERLIGHT
            Parallel.For(
                fromInclusive, 
                toExclusive, 
                () => 0.0f, 
                (i, localData) => localData += body(i), 
                localResult =>
                {
                    lock (sync)
                    {
                        sum += localResult;
                    }
                });
#else

            if (Control.DisableParallelization || Control.NumberOfParallelWorkerThreads < 2)
            {
                for (var index = fromInclusive; index < toExclusive; index++)
                {
                    sum += body(index);
                }
            }
            else
            {
                Parallel.ForEach(
                    Partitioner.Create(fromInclusive, toExclusive),
                    new ParallelOptions { MaxDegreeOfParallelism = Control.NumberOfParallelWorkerThreads },
                    () => 0.0f,
                    (range, loopState, localData) =>
                    {
                        for (var i = range.Item1; i < range.Item2; i++)
                        {
                            localData += body(i);
                        }

                        return localData;
                    },
                    localResult =>
                    {
                        lock (sync)
                        {
                            sum += localResult;
                        }
                    });
            }
#endif
            return sum;
        }

        /// <summary>
        /// Aggregates a function over a loop for Complex data type.
        /// </summary>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop</param>
        /// <param name="body">The function to aggregate.</param>
        /// <returns>The sum of the function over the loop.</returns>
        public static Complex Aggregate(int fromInclusive, int toExclusive, Func<int, Complex> body)
        {
            var sync = new object();
            var sum = Complex.Zero;

#if SILVERLIGHT
            Parallel.For(
                fromInclusive, 
                toExclusive, 
                () => Complex.Zero, 
                (i, localData) => localData += body(i), 
                localResult =>
                {
                    lock (sync)
                    {
                        sum += localResult;
                    }
                });
#else

            if (Control.DisableParallelization || Control.NumberOfParallelWorkerThreads < 2)
            {
                for (var index = fromInclusive; index < toExclusive; index++)
                {
                    sum += body(index);
                }
            }
            else
            {
                Parallel.ForEach(
                    Partitioner.Create(fromInclusive, toExclusive),
                    new ParallelOptions { MaxDegreeOfParallelism = Control.NumberOfParallelWorkerThreads },
                    () => Complex.Zero,
                    (range, loopState, localData) =>
                    {
                        for (var i = range.Item1; i < range.Item2; i++)
                        {
                            localData += body(i);
                        }

                        return localData;
                    },
                    localResult =>
                    {
                        lock (sync)
                        {
                            sum += localResult;
                        }
                    });
            }
#endif
            return sum;
        }

        /// <summary>
        /// Aggregates a function over a loop for Complex32 data type.
        /// </summary>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop</param>
        /// <param name="body">The function to aggregate.</param>
        /// <returns>The sum of the function over the loop.</returns>
        public static Complex32 Aggregate(int fromInclusive, int toExclusive, Func<int, Complex32> body)
        {
            var sync = new object();
            var sum = Complex32.Zero;

#if SILVERLIGHT
            Parallel.For(
                fromInclusive, 
                toExclusive, 
                () => Complex32.Zero, 
                (i, localData) => localData += body(i), 
                localResult =>
                {
                    lock (sync)
                    {
                        sum += localResult;
                    }
                });
#else

            if (Control.DisableParallelization || Control.NumberOfParallelWorkerThreads < 2)
            {
                for (var index = fromInclusive; index < toExclusive; index++)
                {
                    sum += body(index);
                }
            }
            else
            {
                Parallel.ForEach(
                    Partitioner.Create(fromInclusive, toExclusive),
                    new ParallelOptions { MaxDegreeOfParallelism = Control.NumberOfParallelWorkerThreads },
                    () => Complex32.Zero,
                    (range, loopState, localData) =>
                    {
                        for (var i = range.Item1; i < range.Item2; i++)
                        {
                            localData += body(i);
                        }

                        return localData;
                    },
                    localResult =>
                    {
                        lock (sync)
                        {
                            sum += localResult;
                        }
                    });
            }
#endif
            return sum;
        }*/

        /// <summary>
        /// Executes each of the provided actions inside a discrete, asynchronous task. 
        /// </summary>
        /// <param name="actions">An array of actions to execute.</param>
        /// <exception cref="ArgumentException">The actions array contains a <c>null</c> element.</exception>
        /// <exception cref="AggregateException">An action threw an exception.</exception>
        public static void Invoke(params Action[] actions)
        {
            var maxThreads = Control.DisableParallelization ? 1 : Control.NumberOfParallelWorkerThreads;
#if SILVERLIGHT
            Parallel.Invoke(actions);
#else
            Parallel.Invoke(
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxThreads
                },
                actions);
#endif
        }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop</param>
        /// <param name="body">The function to select items over a subset.</param>
        /// <param name="localFinally">The function to select the item of selection from the subsets.</param>
        /// <returns>The selected value.</returns>
        public static double Select(int fromInclusive, int toExclusive, Func<int, double, double> body, Func<double, double, double> localFinally)
       {
            double ret = 0;
            var syncLock = new object();

#if SILVERLIGHT
             Parallel.For(
                fromInclusive, 
                toExclusive, 
                () => 0.0, 
                (i, localData) => localData = body(i, localData), 
                localResult =>
                {
                    lock (syncLock)
                    {
                        ret = localFinally(ret, localResult);
                    }
                });
#else
            var maxThreads = Control.DisableParallelization ? 1 : Control.NumberOfParallelWorkerThreads;
            Parallel.ForEach(
                Partitioner.Create(fromInclusive, toExclusive),
                new ParallelOptions { MaxDegreeOfParallelism = maxThreads },
                () => 0.0, 
                (range, loop, localData) =>
                {
                    for (var i = range.Item1; i < range.Item2; i++)
                    {
                        localData = body(i, localData);
                    }

                    return localData;
                }, 
                localResult =>
                {
                    lock (syncLock)
                    {
                        ret = localFinally(ret, localResult);
                    }
                });
#endif

            return ret;
       }

        /// <summary>
        /// Selects an item (such as Max or Min).
        /// </summary>
        /// <param name="fromInclusive">Starting index of the loop.</param>
        /// <param name="toExclusive">Ending index of the loop</param>
        /// <param name="body">The function to select items over a subset.</param>
        /// <param name="localFinally">The function to select the item of selection from the subsets.</param>
        /// <returns>The selected value.</returns>
        public static float Select(int fromInclusive, int toExclusive, Func<int, float, float> body, Func<float, float, float> localFinally)
        {
            float ret = 0;
            var syncLock = new object();

#if SILVERLIGHT
             Parallel.For(
                fromInclusive, 
                toExclusive, 
                () => 0.0f, 
                (i, localData) => localData = body(i, localData), 
                localResult =>
                {
                    lock (syncLock)
                    {
                        ret = localFinally(ret, localResult);
                    }
                });
#else
            var maxThreads = Control.DisableParallelization ? 1 : Control.NumberOfParallelWorkerThreads;
            Parallel.ForEach(
                Partitioner.Create(fromInclusive, toExclusive),
                new ParallelOptions { MaxDegreeOfParallelism = maxThreads },
                () => 0.0f,
                (range, loop, localData) =>
                {
                    for (var i = range.Item1; i < range.Item2; i++)
                    {
                        localData = body(i, localData);
                    }

                    return localData;
                },
                localResult =>
                {
                    lock (syncLock)
                    {
                        ret = localFinally(ret, localResult);
                    }
                });
#endif

            return ret;
        }
    }
}