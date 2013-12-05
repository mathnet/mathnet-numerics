#if PORTABLE
namespace MathNet.Numerics
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SerializableAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SpecialNameAttribute : Attribute
    {
    }
}
#endif

#if (PORTABLE || NET35)
namespace MathNet.Numerics
{
    using System;

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TargetedPatchingOptOutAttribute : Attribute
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
    using System.Collections.Generic;

    public class Tuple<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Tuple(T1 Item1, T2 Item2)
        {
            this.Item1 = Item1;
            this.Item2 = Item2;
        }
    }

    public class Tuple<T1, T2, T3>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }
        public T3 Item3 { get; set; }

        public Tuple(T1 Item1, T2 Item2, T3 Item3)
        {
            this.Item1 = Item1;
            this.Item2 = Item2;
            this.Item3 = Item3;
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
}
#endif
