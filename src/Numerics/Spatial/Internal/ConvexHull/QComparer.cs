using System;
using System.Collections.Generic;

namespace MathNet.Numerics.Spatial.Internal.ConvexHull
{
    /// <summary>
    /// A comparer for convex hull's use of an Avl tree
    /// </summary>
    internal class QComparer : IComparer<MutablePoint>
    {
        /// <summary>
        /// A function to compare with
        /// </summary>
        private readonly Func<MutablePoint, MutablePoint, int> comparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="QComparer"/> class.
        /// </summary>
        /// <param name="comparer">a function to use for comparing</param>
        public QComparer(Func<MutablePoint, MutablePoint, int> comparer)
        {
            this.comparer = comparer;
        }

        /// <summary>
        /// Compares two points using the provided function
        /// </summary>
        /// <param name="pt1">the first point</param>
        /// <param name="pt2">the second point</param>
        /// <returns>A value of -1 if less than, a value of 1 is greater than; otherwise a value of 0</returns>
        public int Compare(MutablePoint pt1, MutablePoint pt2)
        {
            return this.comparer(pt1, pt2);
        }
    }
}
