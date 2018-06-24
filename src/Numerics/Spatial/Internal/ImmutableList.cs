using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Spatial.Internal
{
    /// <summary>
    /// Internal implementation of an immutable list
    /// </summary>
    internal static class ImmutableList
    {
        /// <summary>
        /// Factory Construction
        /// </summary>
        /// <typeparam name="T">The list type</typeparam>
        /// <param name="data">A list of items to initialize with</param>
        /// <returns>An immutable list</returns>
        internal static ImmutableList<T> Create<T>(IEnumerable<T> data)
        {
            return ImmutableList<T>.Empty.AddRange(data.AsCollection());
        }

        /// <summary>
        /// Gets the passed source as a collection
        /// </summary>
        /// <typeparam name="T">the list type</typeparam>
        /// <param name="source">the list source</param>
        /// <returns>A collection</returns>
        private static ICollection<T> AsCollection<T>(this IEnumerable<T> source)
        {
            if (source is ICollection<T> collection)
            {
                return collection;
            }

            if (source is ImmutableList<T> list)
            {
                return list.GetRawData();
            }

            return source.ToList();
        }
    }
}
