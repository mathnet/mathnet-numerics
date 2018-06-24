using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean3D
{
    /// <summary>
    /// A PolyLine is an ordered series of line segments in space represented as list of connected Point3Ds.
    /// </summary>
    public class PolyLine3D : IEnumerable<Point3D>, IEquatable<PolyLine3D>
    {
        /// <summary>
        /// An internal list of points
        /// </summary>
        private readonly List<Point3D> points;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolyLine3D"/> class.
        /// Creates a PolyLine3D from a pre-existing IEnumerable of Point3Ds
        /// </summary>
        /// <param name="points">A list of points.</param>
        public PolyLine3D(IEnumerable<Point3D> points)
        {
            this.points = new List<Point3D>(points);
        }

        /// <summary>
        /// Gets an integer representing the number of Point3D objects in the polyline
        /// </summary>
        [Obsolete("Use VertexCount instead, obsolete since 2018-01-12")]
        public int Count => this.points.Count;

        /// <summary>
        /// Gets the number of vertices in the polyline.
        /// </summary>
        public int VertexCount => this.points.Count;

        /// <summary>
        /// Gets the length of the polyline, computed as the sum of the lengths of every segment
        /// </summary>
        public double Length => this.GetPolyLineLength();

        /// <summary>
        /// Gets a list of vertices
        /// </summary>
        public IEnumerable<Point3D> Vertices
        {
            get
            {
                foreach (var point in this.points)
                {
                    yield return point;
                }
            }
        }

        /// <summary>
        /// Returns a point in the polyline by index number
        /// </summary>
        /// <param name="key">The index of a point</param>
        /// <returns>The indexed point</returns>
        [Obsolete("Use Vertices instead, obsolete since 2018-01-12")]
        public Point3D this[int key] => this.points[key];

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified lines is equal.
        /// </summary>
        /// <param name="left">The first line to compare</param>
        /// <param name="right">The second line to compare</param>
        /// <returns>True if the lines are the same; otherwise false.</returns>
        public static bool operator ==(PolyLine3D left, PolyLine3D right)
        {
            return left?.Equals(right) == true;
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified lines is not equal.
        /// </summary>
        /// <param name="left">The first line to compare</param>
        /// <param name="right">The second line to compare</param>
        /// <returns>True if the lines are different; otherwise false.</returns>
        public static bool operator !=(PolyLine3D left, PolyLine3D right)
        {
            return left?.Equals(right) != true;
        }

        /// <summary>
        /// Get the point at a fractional distance along the curve.  For instance, fraction=0.5 will return
        /// the point halfway along the length of the polyline.
        /// </summary>
        /// <param name="fraction">The fractional length at which to compute the point</param>
        /// <returns>A point a fraction of the way along the line.</returns>
        public Point3D GetPointAtFractionAlongCurve(double fraction)
        {
            if (fraction > 1 || fraction < 0)
            {
                throw new ArgumentException("fraction must be between 0 and 1");
            }

            return this.GetPointAtLengthFromStart(fraction * this.Length);
        }

        /// <summary>
        /// Get the point at a specified distance along the curve.  A negative argument will return the first point,
        /// an argument greater than the length of the curve will return the last point.
        /// </summary>
        /// <param name="lengthFromStart">The distance from the first point along the curve at which to return a point</param>
        /// <returns>A point which is the specified distance along the line</returns>
        public Point3D GetPointAtLengthFromStart(double lengthFromStart)
        {
            var length = this.Length;
            if (lengthFromStart >= length)
            {
                return this.points.Last();
            }

            if (lengthFromStart <= 0)
            {
                return this.points.First();
            }

            double cumulativeLength = 0;
            var i = 0;
            while (true)
            {
                var nextLength = cumulativeLength + this.points[i].DistanceTo(this.points[i + 1]);
                if (cumulativeLength <= lengthFromStart && nextLength > lengthFromStart)
                {
                    var leftover = lengthFromStart - cumulativeLength;
                    var direction = this.points[i].VectorTo(this.points[i + 1]).Normalize();
                    return this.points[i] + (leftover * direction);
                }

                cumulativeLength = nextLength;
                i++;
            }
        }

        /// <summary>
        /// Returns the closest point on the polyline to the given point.
        /// </summary>
        /// <param name="p">A point</param>
        /// <returns>A point which is the closest to the given point but still on the line.</returns>
        public Point3D ClosestPointTo(Point3D p)
        {
            var minError = double.MaxValue;
            var closest = default(Point3D);

            for (var i = 0; i < this.VertexCount - 1; i++)
            {
                var segment = new LineSegment3D(this.points[i], this.points[i + 1]);
                var projected = segment.ClosestPointTo(p);
                var error = p.DistanceTo(projected);
                if (error < minError)
                {
                    minError = error;
                    closest = projected;
                }
            }

            return closest;
        }

        /// <summary>
        /// Returns a value to indicate if a pair of polylines are equal
        /// </summary>
        /// <param name="other">The polyline to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>true if the polylines are equal; otherwise false</returns>
        [Pure]
        public bool Equals(PolyLine3D other, double tolerance)
        {
            if (this.VertexCount != other?.VertexCount)
            {
                return false;
            }

            for (var i = 0; i < this.points.Count; i++)
            {
                if (!this.points[i].Equals(other.points[i], tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(PolyLine3D other)
        {
            if (this.VertexCount != other?.VertexCount)
            {
                return false;
            }

            for (var i = 0; i < this.points.Count; i++)
            {
                if (!this.points[i].Equals(other.points[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj)
        {
            return obj is PolyLine3D polyLine3D &&
                   this.Equals(polyLine3D);
        }

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode()
        {
            return HashCode.CombineMany(this.points);
        }

        /// <inheritdoc />
        [Obsolete("Use Vertices instead, obsolete since 2018-01-12")]
        public IEnumerator<Point3D> GetEnumerator()
        {
            return this.points.GetEnumerator();
        }

        /// <inheritdoc />
        [Obsolete("Use Vertices instead, obsolete since 2018-01-12")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns the length of the polyline by summing the lengths of the individual segments
        /// </summary>
        /// <returns>The length of the line.</returns>
        private double GetPolyLineLength()
        {
            double length = 0;
            for (var i = 0; i < this.points.Count - 1; ++i)
            {
                length += this.points[i].DistanceTo(this.points[i + 1]);
            }

            return length;
        }
    }
}
