using System;
using System.Diagnostics.Contracts;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean2D
{
    /// <summary>
    /// This structure represents a line between two points in 2-space.  It allows for operations such as
    /// computing the length, direction, comparisons, and shifting by a vector.
    /// </summary>
    public struct LineSegment2D : IEquatable<LineSegment2D>
    {
        /// <summary>
        /// The starting point of the line segment
        /// </summary>
        public readonly Point2D StartPoint;

        /// <summary>
        /// The end point of the line segment
        /// </summary>
        public readonly Point2D EndPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineSegment2D"/> struct.
        /// Throws an ArgumentException if the <paramref name="startPoint"/> is equal to the <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="startPoint">the starting point of the line segment.</param>
        /// <param name="endPoint">the ending point of the line segment</param>
        public LineSegment2D(Point2D startPoint, Point2D endPoint)
        {
            if (startPoint == endPoint)
            {
                throw new ArgumentException("The segment starting and ending points cannot be identical");
            }

            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
        }

        /// <summary>
        /// Gets the distance from <see cref="StartPoint"/> to <see cref="EndPoint"/>
        /// </summary>
        [Pure]
        public double Length => this.StartPoint.DistanceTo(this.EndPoint);

        /// <summary>
        /// Gets a normalized vector in the direction from <see cref="StartPoint"/> to <see cref="EndPoint"/>
        /// </summary>
        [Pure]
        public Vector2D Direction => this.StartPoint.VectorTo(this.EndPoint).Normalize();

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified lines is equal.
        /// </summary>
        /// <param name="left">The first line to compare</param>
        /// <param name="right">The second line to compare</param>
        /// <returns>True if the lines are the same; otherwise false.</returns>
        public static bool operator ==(LineSegment2D left, LineSegment2D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified lines is not equal.
        /// </summary>
        /// <param name="left">The first line to compare</param>
        /// <param name="right">The second line to compare</param>
        /// <returns>True if the lines are different; otherwise false.</returns>
        public static bool operator !=(LineSegment2D left, LineSegment2D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a new <see cref="Line2D"/> from a pair of strings which represent points.
        /// See <see cref="Point2D.Parse(string, IFormatProvider)" /> for details on acceptable formats.
        /// </summary>
        /// <param name="startPointString">The string representation of the first point.</param>
        /// <param name="endPointString">The string representation of the second point.</param>
        /// <returns>A line segment from the first point to the second point.</returns>
        public static LineSegment2D Parse(string startPointString, string endPointString)
        {
            return new LineSegment2D(Point2D.Parse(startPointString), Point2D.Parse(endPointString));
        }

        /// <summary>
        /// Translates a line according to a provided vector
        /// </summary>
        /// <param name="vector">A vector to apply</param>
        /// <returns>A new translated line segment</returns>
        public LineSegment2D TranslateBy(Vector2D vector)
        {
            var startVector = this.StartPoint.ToVector2D().Add(vector);
            var endVector = this.EndPoint.ToVector2D().Add(vector);
            return new LineSegment2D(new Point2D(startVector.X, startVector.Y), new Point2D(endVector.X, endVector.Y));
        }

        /// <summary>
        /// Returns a new line segment between the closest point on this line segment and a point.
        /// </summary>
        /// <param name="p">the point to create a line to</param>
        /// <returns>A line segment between the point and the nearest point on this segment.</returns>
        [Pure]
        public LineSegment2D LineTo(Point2D p)
        {
            return new LineSegment2D(this.ClosestPointTo(p), p);
        }

        /// <summary>
        /// Returns the closest point on the line to the given point.
        /// </summary>
        /// <param name="p">The point that the returned point is the closest point on the line to</param>
        /// <returns>The closest point on the line to the provided point</returns>
        [Pure]
        public Point2D ClosestPointTo(Point2D p)
        {
            var v = this.StartPoint.VectorTo(p);
            var dotProduct = v.DotProduct(this.Direction);

            if (dotProduct < 0)
            {
                dotProduct = 0;
            }

            var l = this.Length;
            if (dotProduct > l)
            {
                dotProduct = l;
            }

            var alongVector = dotProduct * this.Direction;
            return this.StartPoint + alongVector;
        }

        /// <summary>
        /// Compute the intersection between two lines if the angle between them is greater than a specified
        /// angle tolerance.
        /// </summary>
        /// <param name="other">The other line to compute the intersection with</param>
        /// <param name="intersection">When this method returns, contains the intersection point, if the conversion succeeded, or the default point if the conversion failed.</param>
        /// <param name="tolerance">The tolerance used when checking if the lines are parallel</param>
        /// <returns>True if an intersection exists; otherwise false</returns>
        [Pure]
        public bool TryIntersect(LineSegment2D other, out Point2D intersection, Angle tolerance)
        {
            if (this.IsParallelTo(other, tolerance))
            {
                intersection = default(Point2D);
                return false;
            }

            // http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
            var p = this.StartPoint;
            var q = other.StartPoint;
            var r = this.StartPoint.VectorTo(this.EndPoint);
            var s = other.StartPoint.VectorTo(other.EndPoint);

            var t = (q - p).CrossProduct(s) / r.CrossProduct(s);

            intersection = p + (t * r);
            return true;
        }

        /// <summary>
        /// Checks to determine whether or not two line segments are parallel to each other within a specified angle tolerance
        /// </summary>
        /// <param name="other">The other line to check this one against</param>
        /// <param name="tolerance">If the angle between line directions is less than this value, the method returns true</param>
        /// <returns>True if the lines are parallel within the angle tolerance, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(LineSegment2D other, Angle tolerance)
        {
            return this.Direction.IsParallelTo(other.Direction, tolerance);
        }

        /// <inheritdoc/>
        [Pure]
        public override string ToString()
        {
            return $"StartPoint: {this.StartPoint}, EndPoint: {this.EndPoint}";
        }

        /// <summary>
        /// Returns a value to indicate if a pair of line segments are equal
        /// </summary>
        /// <param name="other">The line segment to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>True if the line segments are equal; otherwise false</returns>
        [Pure]
        public bool Equals(LineSegment2D other, double tolerance)
        {
            return this.StartPoint.Equals(other.StartPoint, tolerance) && this.EndPoint.Equals(other.EndPoint, tolerance);
        }

        /// <inheritdoc/>
        [Pure]
        public bool Equals(LineSegment2D l) => this.StartPoint.Equals(l.StartPoint) && this.EndPoint.Equals(l.EndPoint);

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj) => obj is LineSegment2D l && this.Equals(l);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.Combine(this.StartPoint, this.EndPoint);
    }
}
