using System.Globalization;

#if NETSTANDARD1_3
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
            if (toExclusive <= fromInclusive) throw new ArgumentOutOfRangeException(nameof(toExclusive));
            if (rangeSize <= 0) throw new ArgumentOutOfRangeException(nameof(rangeSize));
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
                    throw new ArgumentException($"At least one item of {nameof(actions)} is a null reference (Nothing in Visual Basic).", nameof(actions));
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

#if NETSTANDARD1_3
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

#if NET40
namespace System.Runtime.CompilerServices
{
    internal class FormattableStringFactory
    {
        public static FormattableString Create(string format, params object[] args)
        {
            return new FormattableString(format, args);
        }
    }
}

namespace System
{
    internal class FormattableString
    {
        private readonly string format;
        private readonly object[] args;

        public FormattableString(string format, object[] args)
        {
            this.format = format;
            this.args = args;
        }

        public static string Invariant(FormattableString messageFormat)
        {
            return messageFormat.ToString(CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, format, args);
        }

        public override string ToString()
        {
            return string.Format(format, args);
        }
    }
}
#endif
