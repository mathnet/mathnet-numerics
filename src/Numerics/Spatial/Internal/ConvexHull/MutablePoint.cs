using System;

namespace MathNet.Numerics.Spatial.Internal.ConvexHull
{
    /// <summary>
    /// Provides a working surface for points for ConvextHull - do not use otherwise
    /// </summary>
    internal struct MutablePoint : IEquatable<MutablePoint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MutablePoint"/> struct.
        /// </summary>
        /// <param name="x">The x value</param>
        /// <param name="y">The y value</param>
        internal MutablePoint(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Gets or sets the X value
        /// </summary>
        internal double X { get; set; }

        /// <summary>
        /// Gets or sets the Y Value
        /// </summary>
        internal double Y { get; set; }

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified points is equal.
        /// </summary>
        /// <param name="p1">The first point to compare</param>
        /// <param name="p2">The second point to compare</param>
        /// <returns>True if the points are the same; otherwise false.</returns>
        public static bool operator ==(MutablePoint p1, MutablePoint p2) => p1.Equals(p2);

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified points is not equal.
        /// </summary>
        /// <param name="p1">The first point to compare</param>
        /// <param name="p2">The second point to compare</param>
        /// <returns>True if the points are different; otherwise false.</returns>
        public static bool operator !=(MutablePoint p1, MutablePoint p2) => !p1.Equals(p2);

        /// <inheritdoc/>
        public bool Equals(MutablePoint other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
