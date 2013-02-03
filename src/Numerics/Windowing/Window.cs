using System.Collections.Generic;

namespace MathNet.Numerics.Windowing
{
    public static class Window
    {
        public static IEnumerable<VolatileWindow<T>> SlidingWindow<T>(this IEnumerable<T> source, int size, int slide=1, int buffer=0, bool delayed=true)
        {
            var window = new VolatileWindow<T>(size, buffer <= 0 ? size : buffer);
            int slideCounter = delayed ? slide - size : 0;

            using (var enumerator = source.GetEnumerator())
            while (enumerator.MoveNext())
            {
                window.Next(enumerator.Current);
                if (++slideCounter == slide)
                {
                    slideCounter = 0;
                    yield return window;
                }
            }
        }

        public static IEnumerable<PermanentWindow<T>> PermanentSlidingWindow<T>(this IEnumerable<T> source, int size, int slide=1, int buffer=0, bool delayed=true)
        {
            var window = new PermanentWindow<T>(size, buffer <= 0 ? size : buffer);
            int slideCounter = delayed ? slide - size : 0;

            using (var enumerator = source.GetEnumerator())
            while (enumerator.MoveNext())
            {
                window = window.Next(enumerator.Current);
                if (++slideCounter == slide)
                {
                    slideCounter = 0;
                    yield return window;
                }
            }
        }

        public static IEnumerable<VolatileWindow<T>> TumblingWindow<T>(this IEnumerable<T> source, int size)
        {
            return source.SlidingWindow(size, size, size, true);
        }

        public static IEnumerable<PermanentWindow<T>> PermanentTumblingWindow<T>(this IEnumerable<T> source, int size)
        {
            return source.PermanentSlidingWindow(size, size, size, true);
        }

        public static IEnumerable<VolatileWindow<T>> FilterWindow<T>(this IEnumerable<T> source, int size)
        {
            return source.SlidingWindow(size, 1, 2*size, false);
        }

        public static IEnumerable<PermanentWindow<T>> PermanentFilterWindow<T>(this IEnumerable<T> source, int size)
        {
            return source.PermanentSlidingWindow(size, 1, 2*size, false);
        }
    }
}
