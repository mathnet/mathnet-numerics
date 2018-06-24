using System;
using System.Diagnostics.Contracts;

namespace MathNet.Numerics.Spatial.Euclidean2D
{
    /// <summary>
    /// This structure represents a line between two points in 2-space.  It allows for operations such as
    /// computing the length, direction, projections to, comparisons, and shifting by a vector.
    /// </summary>
    public struct Line2D : IEquatable<Line2D>
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
        /// Initializes a new instance of the <see cref="Line2D"/> struct.
        /// Throws an ArgumentException if the <paramref name="startPoint"/> is equal to the <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="startPoint">the starting point of the line segment.</param>
        /// <param name="endPoint">the ending point of the line segment</param>
        public Line2D(Point2D startPoint, Point2D endPoint)
        {
            if (startPoint == endPoint)
            {
                throw new ArgumentException("The Line2D starting and ending points cannot be identical");
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
        public static bool operator ==(Line2D left, Line2D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified lines is not equal.
        /// </summary>
        /// <param name="left">The first line to compare</param>
        /// <param name="right">The second line to compare</param>
        /// <returns>True if the lines are different; otherwise false.</returns>
        public static bool operator !=(Line2D left, Line2D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Adds a vector to the start point and end point of the line
        /// </summary>
        /// <param name="offset">The vector to add</param>
        /// <param name="line">The line</param>
        /// <returns>A new <see cref="Line2D"/> at the adjusted points</returns>
        public static Line2D operator +(Vector2D offset, Line2D line)
        {
            return new Line2D(line.StartPoint + offset, line.EndPoint + offset);
        }

        /// <summary>
        /// Adds a vector to the start point and end point of the line
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="offset">The vector to add</param>
        /// <returns>A new line at the adjusted points</returns>
        public static Line2D operator +(Line2D line, Vector2D offset)
        {
            return offset + line;
        }

        /// <summary>
        /// Subtracts a vector from the start point and end point of the line
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="offset">The vector to subtract</param>
        /// <returns>A new line at the adjusted points</returns>
        public static Line2D operator -(Line2D line, Vector2D offset)
        {
            return line + (-offset);
        }

        /// <summary>
        /// Returns a new <see cref="Line2D"/> from a pair of strings which represent points.
        /// See <see cref="Point2D.Parse(string, IFormatProvider)" /> for details on acceptable formats.
        /// </summary>
        /// <param name="startPointString">The string representation of the first point.</param>
        /// <param name="endPointString">The string representation of the second point.</param>
        /// <returns>A line segment from the first point to the second point.</returns>
        public static Line2D Parse(string startPointString, string endPointString)
        {
            return new Line2D(Point2D.Parse(startPointString), Point2D.Parse(endPointString));
        }

        /// <summary>
        /// Returns the shortest line between this line and a point.
        /// </summary>
        /// <param name="p">the point to create a line to</param>
        /// <param name="mustStartBetweenAndEnd">If false the start point can extend beyond the start and endpoint of the line</param>
        /// <returns>The shortest line between the line and the point</returns>
        [Pure]
        public Line2D LineTo(Point2D p, bool mustStartBetweenAndEnd)
        {
            return new Line2D(this.ClosestPointTo(p, mustStartBetweenAndEnd), p);
        }

        /// <summary>
        /// Returns the closest point on the line to the given point.
        /// </summary>
        /// <param name="p">The point that the returned point is the closest point on the line to</param>
        /// <param name="mustBeOnSegment">If true the returned point is contained by the segment ends, otherwise it can be anywhere on the projected line</param>
        /// <returns>The closest point on the line to the provided point</returns>
        [Pure]
        public Point2D ClosestPointTo(Point2D p, bool mustBeOnSegment)
        {
            var v = this.StartPoint.VectorTo(p);
            var dotProduct = v.DotProduct(this.Direction);
            if (mustBeOnSegment)
            {
                if (dotProduct < 0)
                {
                    dotProduct = 0;
                }

                var l = this.Length;
                if (dotProduct > l)
                {
                    dotProduct = l;
                }
            }

            var alongVector = dotProduct * this.Direction;
            return this.StartPoint + alongVector;
        }

        /// <summary>
        /// Compute the intersection between two lines with parallelism considered by the double floating point precision
        /// on the cross product of the two directions.
        /// </summary>
        /// <param name="other">The other line to compute the intersection with</param>
        /// <returns>The point at the intersection of two lines, or null if the lines are parallel.</returns>
        [Pure]
        public Point2D? IntersectWith(Line2D other)
        {
            if (this.IsParallelTo(other))
            {
                return null;
            }

            // http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
            var p = this.StartPoint;
            var q = other.StartPoint;
            var r = this.StartPoint.VectorTo(this.EndPoint);
            var s = other.StartPoint.VectorTo(other.EndPoint);

            var t = (q - p).CrossProduct(s) / r.CrossProduct(s);

            return p + (t * r);
        }

        /// <summary>
        /// Compute the intersection between two lines if the angle between them is greater than a specified
        /// angle tolerance.
        /// </summary>
        /// <param name="other">The other line to compute the intersection with</param>
        /// <param name="tolerance">The tolerance used when checking if the lines are parallel</param>
        /// <returns>The point at the intersection of two lines, or null if the lines are parallel.</returns>
        [Pure]
        public Point2D? IntersectWith(Line2D other, Angle tolerance)
        {
            if (this.IsParallelTo(other, tolerance))
            {
                return null;
            }

            // http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
            var p = this.StartPoint;
            var q = other.StartPoint;
            var r = this.StartPoint.VectorTo(this.EndPoint);
            var s = other.StartPoint.VectorTo(other.EndPoint);

            var t = (q - p).CrossProduct(s) / r.CrossProduct(s);

            return p + (t * r);
        }

        /// <summary>
        /// Checks to determine whether or not two lines are parallel to each other, using the dot product within
        /// the double precision specified in the MathNet.Numerics package.
        /// </summary>
        /// <param name="other">The other line to check this one against</param>
        /// <returns>True if the lines are parallel, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(Line2D other)
        {
            return this.Direction.IsParallelTo(other.Direction, Precision.DoublePrecision * 2);
        }

        /// <summary>
        /// Checks to determine whether or not two lines are parallel to each other within a specified angle tolerance
        /// </summary>
        /// <param name="other">The other line to check this one against</param>
        /// <param name="tolerance">If the angle between line directions is less than this value, the method returns true</param>
        /// <returns>True if the lines are parallel within the angle tolerance, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(Line2D other, Angle tolerance)
        {
            return this.Direction.IsParallelTo(other.Direction, tolerance);
        }

        /// <inheritdoc/>
        [Pure]
        public override string ToString()
        {
            return $"StartPoint: {this.StartPoint}, EndPoint: {this.EndPoint}";
        }

        /// <inheritdoc/>
        [Pure]
        public bool Equals(Line2D other)
        {
            return this.StartPoint.Equals(other.StartPoint) && this.EndPoint.Equals(other.EndPoint);
        }

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Line2D d && this.Equals(d);
        }

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.StartPoint.GetHashCode() * 397) ^ this.EndPoint.GetHashCode();
            }
        }
    }
}
