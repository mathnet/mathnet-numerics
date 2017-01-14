#if PORTABLE
namespace MathNet.Numerics
{
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class SerializableAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class SpecialNameAttribute : Attribute
    {
    }

    internal static class Partitioner
    {
        public static IEnumerable<Tuple<int, int>> Create(int fromInclusive, int toExclusive)
        {
            var rangeSize = Math.Max(1, (toExclusive - fromInclusive) / Control.MaxDegreeOfParallelism);
            return Create(fromInclusive, toExclusive, rangeSize);
        }

        public static IEnumerable<Tuple<int, int>> Create(int fromInclusive, int toExclusive, int rangeSize)
        {
            if (toExclusive <= fromInclusive) throw new ArgumentOutOfRangeException("toExclusive");
            if (rangeSize <= 0) throw new ArgumentOutOfRangeException("rangeSize");
            return CreateRanges(fromInclusive, toExclusive, rangeSize);
        }

        private static IEnumerable<Tuple<int, int>> CreateRanges(int fromInclusive, int toExclusive, int rangeSize)
        {
            bool flag = false;
            int num = fromInclusive;
            while (num < toExclusive && !flag)
            {
                int item = num;
                int num2;
                try
                {
                    num2 = checked(num + rangeSize);
                }
                catch (OverflowException)
                {
                    num2 = toExclusive;
                    flag = true;
                }
                if (num2 > toExclusive)
                {
                    num2 = toExclusive;
                }
                yield return new Tuple<int, int>(item, num2);
                num += rangeSize;
            }
        }
    }

    internal class ParallelOptions
    {
        public TaskScheduler TaskScheduler { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
        public CancellationToken CancellationToken { get; set; }

        public ParallelOptions()
        {
            TaskScheduler = TaskScheduler.Default;
            MaxDegreeOfParallelism = -1;
            CancellationToken = CancellationToken.None;
        }
    }

    internal class ParallelLoopState
    {
    }

    internal static class Parallel
    {
        public static void ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions parallelOptions, Action<TSource> body)
        {
            var chunks = source.ToArray();
            var tasks = new Task[chunks.Length];

            for (var i = 0; i < tasks.Length; i++)
            {
                var chunk = chunks[i];
                tasks[i] = Task.Factory.StartNew(() => body(chunk), parallelOptions.CancellationToken, TaskCreationOptions.None, parallelOptions.TaskScheduler);
            }

            Task.WaitAll(tasks, parallelOptions.CancellationToken);
        }

        public static void Invoke(ParallelOptions parallelOptions, params Action[] actions)
        {
            var tasks = new Task[actions.Length];

            for (var i = 0; i < tasks.Length; i++)
            {
                var action = actions[i];
                if (action == null)
                {
                    throw new ArgumentException(String.Format(Properties.Resources.ArgumentItemNull, "actions"), "actions");
                }

                tasks[i] = Task.Factory.StartNew(action, parallelOptions.CancellationToken, TaskCreationOptions.None, parallelOptions.TaskScheduler);
            }

            Task.WaitAll(tasks, parallelOptions.CancellationToken);
        }

        public static void ForEach<TSource, TLocal>(
            IEnumerable<TSource> source,
            ParallelOptions parallelOptions,
            Func<TLocal> localInit,
            Func<TSource, ParallelLoopState, TLocal, TLocal> body,
            Action<TLocal> localFinally)
        {
            var chunks = source.ToArray();
            var tasks = new Task[chunks.Length];
            var loopState = new ParallelLoopState();

            for (var i = 0; i < tasks.Length; i++)
            {
                var chunk = chunks[i];
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    var local = localInit();
                    local = body(chunk, loopState, local);
                    localFinally(local);
                }, parallelOptions.CancellationToken, TaskCreationOptions.None, parallelOptions.TaskScheduler);
            }

            Task.WaitAll(tasks, parallelOptions.CancellationToken);
        }
    }
}
#endif

#if (PORTABLE || NET35)
namespace MathNet.Numerics
{
    using System;

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class TargetedPatchingOptOutAttribute : Attribute
    {
        public string Reason { get; private set; }

        public TargetedPatchingOptOutAttribute(string reason)
        {
            Reason = reason;
        }
    }
}
#endif

#if NET35
namespace MathNet.Numerics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    internal static class ObjectComparer
    {
        internal static int Compare<T>(T a, T b)
        {
            if (ReferenceEquals(a, null)) return -1;
            if (ReferenceEquals(b, null)) return 1;
            if (Equals(a, b)) return 0;
            return Comparer<T>.Default.Compare(a, b);
        }
    }

    public class Tuple<T1, T2> : IComparable, IComparable<Tuple<T1, T2>>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            var other = obj as Tuple<T1, T2>;
            if (other == null) throw new ArgumentException();
            return CompareTo(other);
        }

        public int CompareTo(Tuple<T1, T2> other)
        {
            if (other == null) return 1;
            int a = ObjectComparer.Compare(Item1, other.Item1);
            return a != 0 ? a : ObjectComparer.Compare(Item2, other.Item2);
        }
    }

    public class Tuple<T1, T2, T3> : IComparable, IComparable<Tuple<T1, T2, T3>>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }

        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            var other = obj as Tuple<T1, T2, T3>;
            if (other == null) throw new ArgumentException();
            return CompareTo(other);
        }

        public int CompareTo(Tuple<T1, T2, T3> other)
        {
            if (other == null) return 1;
            int a = ObjectComparer.Compare(Item1, other.Item1);
            if (a != 0) return a;
            int b = ObjectComparer.Compare(Item2, other.Item2);
            return b != 0 ? b : ObjectComparer.Compare(Item3, other.Item3);
        }
    }

    public class Tuple<T1, T2, T3, T4> : IComparable, IComparable<Tuple<T1, T2, T3, T4>>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }
        public T4 Item4 { get; set; }

        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            var other = obj as Tuple<T1, T2, T3, T4>;
            if (other == null) throw new ArgumentException();
            return CompareTo(other);
        }

        public int CompareTo(Tuple<T1, T2, T3, T4> other)
        {
            if (other == null) return 1;
            int a = ObjectComparer.Compare(Item1, other.Item1);
            if (a != 0) return a;
            int b = ObjectComparer.Compare(Item2, other.Item2);
            if (b != 0) return b;
            int c = ObjectComparer.Compare(Item3, other.Item3);
            return c != 0 ? c : ObjectComparer.Compare(Item4, other.Item4);
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Zip<TA, TB, T>(this IEnumerable<TA> seqA, IEnumerable<TB> seqB, Func<TA, TB, T> func)
        {
            if (seqA == null) throw new ArgumentNullException("seqA");
            if (seqB == null) throw new ArgumentNullException("seqB");

            return Zip35Deferred(seqA, seqB, func);
        }

        private static IEnumerable<T> Zip35Deferred<A, B, T>(IEnumerable<A> seqA, IEnumerable<B> seqB, Func<A, B, T> func)
        {
            using (var iteratorA = seqA.GetEnumerator())
            using (var iteratorB = seqB.GetEnumerator())
            {
                while (iteratorA.MoveNext() && iteratorB.MoveNext())
                {
                    yield return func(iteratorA.Current, iteratorB.Current);
                }
            }
        }
    }

    internal static class Partitioner
    {
        public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive, int toExclusive)
        {
            var rangeSize = Math.Max(1, (toExclusive - fromInclusive) / Control.MaxDegreeOfParallelism);
            return Create(fromInclusive, toExclusive, rangeSize);
        }

        public static OrderablePartitioner<Tuple<int, int>> Create(int fromInclusive, int toExclusive, int rangeSize)
        {
            if (toExclusive <= fromInclusive) throw new ArgumentOutOfRangeException("toExclusive");
            if (rangeSize <= 0) throw new ArgumentOutOfRangeException("rangeSize");
            return System.Collections.Concurrent.Partitioner.Create(CreateRanges(fromInclusive, toExclusive, rangeSize));
        }

        private static IEnumerable<Tuple<int, int>> CreateRanges(int fromInclusive, int toExclusive, int rangeSize)
        {
            bool flag = false;
            int num = fromInclusive;
            while (num < toExclusive && !flag)
            {
                int item = num;
                int num2;
                try
                {
                    num2 = checked(num + rangeSize);
                }
                catch (OverflowException)
                {
                    num2 = toExclusive;
                    flag = true;
                }
                if (num2 > toExclusive)
                {
                    num2 = toExclusive;
                }
                yield return new Tuple<int, int>(item, num2);
                num += rangeSize;
            }
        }
    }
}
#endif
