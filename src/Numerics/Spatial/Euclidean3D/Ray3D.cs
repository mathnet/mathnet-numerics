using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean3D
{
    /// <summary>
    /// A ray in 3D space
    /// </summary>
    [Serializable]
    public struct Ray3D : IEquatable<Ray3D>, IXmlSerializable, IFormattable
    {
        /// <summary>
        /// The start point of the ray
        /// </summary>
        public readonly Point3D ThroughPoint;

        /// <summary>
        /// The direction of the ray
        /// </summary>
        public readonly UnitVector3D Direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ray3D"/> struct.
        /// </summary>
        /// <param name="throughPoint">The start point of the ray.</param>
        /// <param name="direction">The direction of the ray.</param>
        public Ray3D(Point3D throughPoint, UnitVector3D direction)
        {
            this.ThroughPoint = throughPoint;
            this.Direction = direction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ray3D"/> struct.
        /// </summary>
        /// <param name="throughPoint">The start point of the ray.</param>
        /// <param name="direction">A vector indicating the direction of the ray.</param>
        public Ray3D(Point3D throughPoint, Vector3D direction)
            : this(throughPoint, direction.Normalize())
        {
        }

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified rays is equal.
        /// </summary>
        /// <param name="left">The first ray to compare</param>
        /// <param name="right">The second ray to compare</param>
        /// <returns>True if the rays are the same; otherwise false.</returns>
        public static bool operator ==(Ray3D left, Ray3D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified rays is not equal.
        /// </summary>
        /// <param name="left">The first ray to compare</param>
        /// <param name="right">The second ray to compare</param>
        /// <returns>True if the rays are different; otherwise false.</returns>
        public static bool operator !=(Ray3D left, Ray3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// The intersection of the two planes
        /// </summary>
        /// <param name="plane1">The first plane</param>
        /// <param name="plane2">The second plane</param>
        /// <returns>A ray at the intersection of two planes</returns>
        public static Ray3D IntersectionOf(Plane3D plane1, Plane3D plane2)
        {
            return plane1.IntersectionWith(plane2);
        }

        /// <summary>
        /// Parses string representation of throughpoint and direction
        /// See <see cref="Point3D.Parse(string, IFormatProvider)" /> and  <see cref="UnitVector3D.Parse(string, IFormatProvider, double)" /> for details on acceptable formats.
        /// This is mainly meant for tests
        /// </summary>
        /// <param name="point">a string representing a start point for the ray.</param>
        /// <param name="direction">a string representing a direction for the ray.</param>
        /// <returns>A ray.</returns>
        public static Ray3D Parse(string point, string direction)
        {
            return new Ray3D(Point3D.Parse(point), UnitVector3D.Parse(direction));
        }

        /// <summary>
        /// Parses a string in the format: 'p:{1, 2, 3} v:{0, 0, 1}' to a Ray3D
        /// This is mainly meant for tests
        /// </summary>
        /// <param name="s">a string representing the ray</param>
        /// <returns>a ray</returns>
        [Obsolete("Should not have been made public. Mode Obsolete 2017-12-09")]
        public static Ray3D Parse(string s)
        {
            return Parser.ParseRay3D(s);
        }

        /// <summary>
        /// Returns the shortest line from a point to the ray
        /// </summary>
        /// <param name="point3D">A point.</param>
        /// <returns>A line segment from the point to the closest point on the ray</returns>
        [Pure]
        [Obsolete("Use ShortestLineTo, Obsolete from 2017-12-11")]
        public Line3D LineTo(Point3D point3D)
        {
            var v = this.ThroughPoint.VectorTo(point3D);
            var alongVector = v.ProjectOn(this.Direction);
            return new Line3D(this.ThroughPoint + alongVector, point3D);
        }

        /// <summary>
        /// Returns the shortest line from a point to the ray
        /// </summary>
        /// <param name="point3D">A point.</param>
        /// <returns>A line segment from the point to the closest point on the ray</returns>
        [Pure]
        public LineSegment3D ShortestLineTo(Point3D point3D)
        {
            var v = this.ThroughPoint.VectorTo(point3D);
            var alongVector = v.ProjectOn(this.Direction);
            return new LineSegment3D(this.ThroughPoint + alongVector, point3D);
        }

        /// <summary>
        /// Returns the point at which this ray intersects with the plane
        /// </summary>
        /// <param name="plane">A geometric plane.</param>
        /// <returns>A point of intersection if such an intersection exists; otherwise null.</returns>
        [Pure]
        public Point3D? IntersectionWith(Plane3D plane)
        {
            return plane.IntersectionWith(this);
        }

        /// <summary>
        /// Returns a value to indicate if a pair of rays are collinear
        /// </summary>
        /// <param name="otherRay">The ray to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>True if the rays are collinear; otherwise false.</returns>
        [Pure]
        public bool IsCollinear(Ray3D otherRay, double tolerance = float.Epsilon)
        {
            return this.Direction.IsParallelTo(otherRay.Direction, tolerance);
        }

        /// <summary>
        /// Returns a value to indicate if a pair of rays are equal
        /// </summary>
        /// <param name="other">The ray to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>True if the rays are equal; otherwise false</returns>
        [Pure]
        public bool Equals(Ray3D other, double tolerance)
        {
            return this.Direction.Equals(other.Direction, tolerance) &&
                   this.ThroughPoint.Equals(other.ThroughPoint, tolerance);
        }

        /// <inheritdoc/>
        [Pure]
        public bool Equals(Ray3D r) => this.Direction.Equals(r.Direction) && this.ThroughPoint.Equals(r.ThroughPoint);

        /// <inheritdoc/>
        [Pure]
        public override bool Equals(object obj) => obj is Ray3D r && this.Equals(r);

        /// <inheritdoc/>
        [Pure]
        public override int GetHashCode() => HashCode.Combine(this.ThroughPoint, this.Direction);

        /// <inheritdoc/>
        [Pure]
        public override string ToString()
        {
            return this.ToString(null, CultureInfo.InvariantCulture);
        }

        /// <inheritdoc/>
        [Pure]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(
                "ThroughPoint: {0}, Direction: {1}",
                this.ThroughPoint.ToString(format, formatProvider),
                this.Direction.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <inheritdoc/>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);
            this = new Ray3D(
                Point3D.ReadFrom(e.SingleElement("ThroughPoint").CreateReader()),
                UnitVector3D.ReadFrom(e.SingleElement("Direction").CreateReader()));
        }

        /// <inheritdoc/>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteElement("ThroughPoint", this.ThroughPoint);
            writer.WriteElement("Direction", this.Direction);
        }
    }
}
