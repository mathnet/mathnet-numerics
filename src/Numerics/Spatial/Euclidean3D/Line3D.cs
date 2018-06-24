using System;
using System.Diagnostics.Contracts;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean3D
{
    /// <summary>
    /// A line between two points
    /// </summary>
    [Serializable]
    public struct Line3D : IEquatable<Line3D>, IXmlSerializable
    {
        /// <summary>
        /// The start point of the line
        /// </summary>
        public readonly Point3D StartPoint;

        /// <summary>
        /// The end point of the line
        /// </summary>
        public readonly Point3D EndPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="Line3D"/> struct.
        /// Throws an ArgumentException if the <paramref name="startPoint"/> is equal to the <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="startPoint">The starting point of the line segment.</param>
        /// <param name="endPoint">The ending point of the line segment.</param>
        public Line3D(Point3D startPoint, Point3D endPoint)
        {
            if (startPoint == endPoint)
            {
                throw new ArgumentException("StartPoint == EndPoint");
            }

            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
        }

        /// <summary>
        /// Gets distance from <see cref="StartPoint"/> to <see cref="EndPoint"/>, the length of the line
        /// </summary>
        [Pure]
        public double Length => this.StartPoint.DistanceTo(this.EndPoint);

        /// <summary>
        /// Gets the direction from the <see cref="StartPoint"/> to <see cref="EndPoint"/>
        /// </summary>
        [Pure]
        public UnitVector3D Direction => this.StartPoint.VectorTo(this.EndPoint).Normalize();

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified lines is equal.
        /// </summary>
        /// <param name="left">The first line to compare</param>
        /// <param name="right">The second line to compare</param>
        /// <returns>True if the lines are the same; otherwise false.</returns>
        public static bool operator ==(Line3D left, Line3D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified lines is not equal.
        /// </summary>
        /// <param name="left">The first line to compare</param>
        /// <param name="right">The second line to compare</param>
        /// <returns>True if the lines are different; otherwise false.</returns>
        public static bool operator !=(Line3D left, Line3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns a new <see cref="Line3D"/> from a pair of strings which represent points.
        /// See <see cref="Point3D.Parse(string, IFormatProvider)" /> for details on acceptable formats.
        /// </summary>
        /// <param name="startPoint">The string representation of the first point.</param>
        /// <param name="endPoint">The string representation of the second point.</param>
        /// <returns>A line segment from the first point to the second point.</returns>
        public static Line3D Parse(string startPoint, string endPoint)
        {
            return new Line3D(Point3D.Parse(startPoint), Point3D.Parse(endPoint));
        }

        /// <summary>
        /// Returns the shortest line between this line and a point.
        /// </summary>
        /// <param name="p">the point to create a line to</param>
        /// <param name="mustStartBetweenStartAndEnd">If false the start point can extend beyond the start and endpoint of the line</param>
        /// <returns>The shortest line between the line and the point</returns>
        [Pure]
        public Line3D LineTo(Point3D p, bool mustStartBetweenStartAndEnd)
        {
            return new Line3D(this.ClosestPointTo(p, mustStartBetweenStartAndEnd), p);
        }

        /// <summary>
        /// Returns the closest point on the line to the given point.
        /// </summary>
        /// <param name="p">The point that the returned point is the closest point on the line to</param>
        /// <param name="mustBeOnSegment">If true the returned point is contained by the segment ends, otherwise it can be anywhere on the projected line</param>
        /// <returns>The closest point on the line to the provided point</returns>
        [Pure]
        public Point3D ClosestPointTo(Point3D p, bool mustBeOnSegment)
        {
            var v = p - this.StartPoint;
            var dotProduct = v.DotProduct(this.Direction);
            if (mustBeOnSegment)
            {
                if (dotProduct < 0)
                {
                    dotProduct = 0;
                }

                if (dotProduct > this.Length)
                {
                    dotProduct = this.Length;
                }
            }

            var alongVector = dotProduct * this.Direction;
            return this.StartPoint + alongVector;
        }

        /// <summary>
        /// The line projected on a plane
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <returns>A projected line.</returns>
        [Pure]
        public Line3D ProjectOn(Plane3D plane)
        {
            return plane.Project(this);
        }

        /// <summary>
        /// Find the intersection between the line and a plane
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <param name="tolerance">A tolerance (epsilon) to compensate for floating point error</param>
        /// <returns>A point where the line and plane intersect; null if no such point exists</returns>
        [Pure]
        public Point3D? IntersectionWith(Plane3D plane, double tolerance = double.Epsilon)
        {
            return plane.IntersectionWith(this, tolerance);
        }

        /// <summary>
        /// Checks to determine whether or not two lines are parallel to each other, using the dot product within
        /// the double precision specified in the MathNet.Numerics package.
        /// </summary>
        /// <param name="other">The other line to check this one against</param>
        /// <returns>True if the lines are parallel, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(Line3D other)
        {
            return this.Direction.IsParallelTo(other.Direction, Precision.DoublePrecision * 2);
        }

        /// <summary>
        /// Checks to determine whether or not two lines are parallel to each other within a specified angle tolerance
        /// </summary>
        /// <param name="other">The other line to check this one against</param>
        /// <param name="angleTolerance">If the angle between line directions is less than this value, the method returns true</param>
        /// <returns>True if the lines are parallel within the angle tolerance, false if they are not</returns>
        [Pure]
        public bool IsParallelTo(Line3D other, Angle angleTolerance)
        {
            return this.Direction.IsParallelTo(other.Direction, angleTolerance);
        }

        /// <summary>
        /// Computes the pair of points which represent the closest distance between this Line3D and another Line3D, with the first
        /// point being the point on this Line3D, and the second point being the corresponding point on the other Line3D.  If the lines
        /// intersect the points will be identical, if the lines are parallel the first point will be the start point of this line.
        /// </summary>
        /// <param name="other">line to compute the closest points with</param>
        /// <returns>A tuple of two points representing the endpoints of the shortest distance between the two lines</returns>
        [Pure]
        public Tuple<Point3D, Point3D> ClosestPointsBetween(Line3D other)
        {
            if (this.IsParallelTo(other))
            {
                return Tuple.Create(this.StartPoint, other.ClosestPointTo(this.StartPoint, false));
            }

            // http://geomalgorithms.com/a07-_distance.html
            var point0 = this.StartPoint;
            var u = this.Direction;
            var point1 = other.StartPoint;
            var v = other.Direction;

            var w0 = point0 - point1;
            var a = u.DotProduct(u);
            var b = u.DotProduct(v);
            var c = v.DotProduct(v);
            var d = u.DotProduct(w0);
            var e = v.DotProduct(w0);

            var sc = ((b * e) - (c * d)) / ((a * c) - (b * b));
            var tc = ((a * e) - (b * d)) / ((a * c) - (b * b));

            return Tuple.Create(point0 + (sc * u), point1 + (tc * v));
        }

        /// <summary>
        /// Computes the pair of points which represents the closest distance between this Line3D and another Line3D, with the option
        /// of treating the lines as segments bounded by their start and end points.
        /// </summary>
        /// <param name="other">line to compute the closest points with</param>
        /// <param name="mustBeOnSegments">if true, the lines are treated as segments bounded by the start and end point</param>
        /// <returns>A tuple of two points representing the endpoints of the shortest distance between the two lines or segments</returns>
        [Pure]
        public Tuple<Point3D, Point3D> ClosestPointsBetween(Line3D other, bool mustBeOnSegments)
        {
            // If the segments are parallel and the answer must be on the segments, we can skip directly to the ending
            // algorithm where the endpoints are projected onto the opposite segment and the smallest distance is
            // taken.  Otherwise we must first check if the infinite length line solution is valid.
            // If the lines aren't parallel OR it doesn't have to be constrained to the segments
            if (!this.IsParallelTo(other) || !mustBeOnSegments)
            {
                // Compute the unbounded result, and if mustBeOnSegments is false we can directly return the results
                // since this is the same as calling the other method.
                var result = this.ClosestPointsBetween(other);
                if (!mustBeOnSegments)
                {
                    return result;
                }

                // A point that is known to be collinear with the line start and end points is on the segment if
                // its distance to both endpoints is less than the segment length.  If both projected points lie
                // within their segment, we can directly return the result.
                if (result.Item1.DistanceTo(this.StartPoint) <= this.Length &&
                    result.Item1.DistanceTo(this.EndPoint) <= this.Length &&
                    result.Item2.DistanceTo(other.StartPoint) <= other.Length &&
                    result.Item2.DistanceTo(other.EndPoint) <= other.Length)
                {
                    return result;
                }
            }

            //// If we got here, we know that either we're doing a bounded distance on two parallel segments or one
            //// of the two closest span points is outside of the segment of the line it was projected on.  In either
            //// case we project each of the four endpoints onto the opposite segments and select the one with the
            //// smallest projected distance.

            var checkPoint = other.ClosestPointTo(this.StartPoint, true);
            var distance = checkPoint.DistanceTo(this.StartPoint);
            var closestPair = Tuple.Create(this.StartPoint, checkPoint);
            var minDistance = distance;

            checkPoint = other.ClosestPointTo(this.EndPoint, true);
            distance = checkPoint.DistanceTo(this.EndPoint);
            if (distance < minDistance)
            {
                closestPair = Tuple.Create(this.EndPoint, checkPoint);
                minDistance = distance;
            }

            checkPoint = this.ClosestPointTo(other.StartPoint, true);
            distance = checkPoint.DistanceTo(other.StartPoint);
            if (distance < minDistance)
            {
                closestPair = Tuple.Create(checkPoint, other.StartPoint);
                minDistance = distance;
            }

            checkPoint = this.ClosestPointTo(other.EndPoint, true);
            distance = checkPoint.DistanceTo(other.EndPoint);
            if (distance < minDistance)
            {
                closestPair = Tuple.Create(checkPoint, other.EndPoint);
            }

            return closestPair;
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(Line3D other)
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

            return obj is Line3D d && this.Equals(d);
        }

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.StartPoint.GetHashCode();
                hashCode = (hashCode * 397) ^ this.EndPoint.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        [Pure]
        public override string ToString()
        {
            return $"StartPoint: {this.StartPoint}, EndPoint: {this.EndPoint}";
        }

        /// <inheritdoc />
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <inheritdoc/>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);
            this = new Line3D(
                Point3D.ReadFrom(e.SingleElement("StartPoint").CreateReader()),
                Point3D.ReadFrom(e.SingleElement("EndPoint").CreateReader()));
        }

        /// <inheritdoc />
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteElement("StartPoint", this.StartPoint);
            writer.WriteElement("EndPoint", this.EndPoint);
        }
    }
}
