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
