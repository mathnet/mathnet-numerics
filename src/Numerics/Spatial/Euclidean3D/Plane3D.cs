using System;
using System.Diagnostics.Contracts;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Spatial.Internal;

namespace MathNet.Numerics.Spatial.Euclidean3D
{
    /// <summary>
    /// A geometric plane
    /// </summary>
    [Serializable]
    public struct Plane3D : IEquatable<Plane3D>, IXmlSerializable
    {
        /// <summary>
        /// The normal vector of the Plane.
        /// </summary>
        public readonly UnitVector3D Normal;

        /// <summary>
        /// The distance to the Plane along its normal from the origin.
        /// </summary>
        public readonly double D;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane3D"/> struct.
        /// Constructs a Plane from the X, Y, and Z components of its normal, and its distance from the origin on that normal.
        /// </summary>
        /// <param name="x">The X-component of the normal.</param>
        /// <param name="y">The Y-component of the normal.</param>
        /// <param name="z">The Z-component of the normal.</param>
        /// <param name="d">The distance of the Plane along its normal from the origin.</param>
        public Plane3D(double x, double y, double z, double d)
            : this(UnitVector3D.Create(x, y, z), -d)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane3D"/> struct.
        /// Constructs a Plane from the given normal and distance along the normal from the origin.
        /// </summary>
        /// <param name="normal">The Plane's normal vector.</param>
        /// <param name="offset">The Plane's distance from the origin along its normal vector.</param>
        public Plane3D(UnitVector3D normal, double offset = 0)
        {
            this.Normal = normal;
            this.D = -offset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane3D"/> struct.
        /// Constructs a Plane from the given normal and distance along the normal from the origin.
        /// </summary>
        /// <param name="normal">The Plane's normal vector.</param>
        /// <param name="rootPoint">A point in the plane.</param>
        public Plane3D(UnitVector3D normal, Point3D rootPoint)
            : this(normal, normal.DotProduct(rootPoint))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane3D"/> struct.
        /// Constructs a Plane from the given normal and distance along the normal from the origin.
        /// </summary>
        /// <param name="normal">The Plane's normal vector.</param>
        /// <param name="rootPoint">A point in the plane.</param>
        public Plane3D(Point3D rootPoint, UnitVector3D normal)
            : this(normal, normal.DotProduct(rootPoint))
        {
        }

        /// <summary>
        /// Gets the <see cref="Normal"/> x component.
        /// </summary>
        public double A => this.Normal.X;

        /// <summary>
        /// Gets the <see cref="Normal"/> y component.
        /// </summary>
        public double B => this.Normal.Y;

        /// <summary>
        /// Gets the <see cref="Normal"/> y component.
        /// </summary>
        public double C => this.Normal.Z;

        /// <summary>
        /// Gets the point on the plane closest to origin.
        /// </summary>
        public Point3D RootPoint => (-this.D * this.Normal).ToPoint3D();

        /// <summary>
        /// Returns a value that indicates whether each pair of elements in two specified geometric planes is equal.
        /// </summary>
        /// <param name="left">The first plane to compare.</param>
        /// <param name="right">The second plane to compare.</param>
        /// <returns>True if the geometric planes are the same; otherwise false.</returns>
        public static bool operator ==(Plane3D left, Plane3D right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value that indicates whether any pair of elements in two specified geometric planes is not equal.
        /// </summary>
        /// <param name="left">The first plane to compare.</param>
        /// <param name="right">The second plane to compare.</param>
        /// <returns>True if the geometric planes are different; otherwise false.</returns>
        public static bool operator !=(Plane3D left, Plane3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane3D"/> struct.
        /// Creates a plane that contains the three given points.
        /// </summary>
        /// <param name="p1">The first point on the plane.</param>
        /// <param name="p2">The second point on the plane.</param>
        /// <param name="p3">The third point on the plane.</param>
        /// <returns>The plane containing the three points.</returns>
        public static Plane3D FromPoints(Point3D p1, Point3D p2, Point3D p3)
        {
            // http://www.had2know.com/academics/equation-plane-through-3-points.html
            if (p1 == p2 || p1 == p3 || p2 == p3)
            {
                throw new ArgumentException("Must use three different points");
            }

            var v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            var v2 = new Vector3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);
            var cross = v1.CrossProduct(v2);

            if (cross.Length <= float.Epsilon)
            {
                throw new ArgumentException("The 3 points should not be on the same line");
            }

            return new Plane3D(cross.Normalize(), p1);
        }

        /// <summary>
        /// Returns a point of intersection between three planes
        /// </summary>
        /// <param name="plane1">The first plane</param>
        /// <param name="plane2">The second plane</param>
        /// <param name="plane3">The third plane</param>
        /// <returns>The intersection point</returns>
        public static Point3D PointFromPlanes(Plane3D plane1, Plane3D plane2, Plane3D plane3)
        {
            return Point3D.IntersectionOf(plane1, plane2, plane3);
        }

        /// <summary>
        /// Get the distance to the point along the <see cref="Normal"/>
        /// </summary>
        /// <param name="point">The <see cref="Point3D"/></param>
        /// <returns>The distance.</returns>
        [Pure]
        public double SignedDistanceTo(Point3D point)
        {
            var p = this.Project(point);
            var v = p.VectorTo(point);
            return v.DotProduct(this.Normal);
        }

        /// <summary>
        /// Get the distance to the plane along the <see cref="Normal"/>
        /// This assumes the planes are parallel
        /// </summary>
        /// <param name="other">The <see cref="Point3D"/></param>
        /// <returns>The distance.</returns>
        [Pure]
        public double SignedDistanceTo(Plane3D other)
        {
            if (!this.Normal.IsParallelTo(other.Normal, tolerance: 1E-15))
            {
                throw new ArgumentException("Planes are not parallel");
            }

            return this.SignedDistanceTo(other.RootPoint);
        }

        /// <summary>
        /// Get the distance to the ThroughPoint of <paramref name="ray"/>  along the <see cref="Normal"/>
        /// This assumes the ray is parallel to the plane.
        /// </summary>
        /// <param name="ray">The <see cref="Point3D"/></param>
        /// <returns>The distance.</returns>
        [Pure]
        public double SignedDistanceTo(Ray3D ray)
        {
            if (Math.Abs(ray.Direction.DotProduct(this.Normal) - 0) < 1E-15)
            {
                return this.SignedDistanceTo(ray.ThroughPoint);
            }

            return 0;
        }

        /// <summary>
        /// Get the distance to the point.
        /// </summary>
        /// <param name="point">The <see cref="Point3D"/></param>
        /// <returns>The distance.</returns>
        [Pure]
        public double AbsoluteDistanceTo(Point3D point)
        {
            return Math.Abs(this.SignedDistanceTo(point));
        }

        /// <summary>
        /// Projects a point onto the plane
        /// </summary>
        /// <param name="p">A point</param>
        /// <param name="projectionDirection">The direction of projection</param>
        /// <returns>a projected point</returns>
        [Pure]
        public Point3D Project(Point3D p, UnitVector3D? projectionDirection = null)
        {
            var dotProduct = this.Normal.DotProduct(p.ToVector3D());
            var projectiononNormal = projectionDirection == null ? this.Normal : projectionDirection.Value;
            var projectionVector = (dotProduct + this.D) * projectiononNormal;
            return p - projectionVector;
        }

        /// <summary>
        /// Projects a line onto the plane
        /// </summary>
        /// <param name="line3DToProject">The line to project</param>
        /// <returns>A projected line</returns>
        public Line3D Project(Line3D line3DToProject)
        {
            var projectedStartPoint = this.Project(line3DToProject.StartPoint);
            var projectedEndPoint = this.Project(line3DToProject.EndPoint);
            return new Line3D(projectedStartPoint, projectedEndPoint);
        }

        /// <summary>
        /// Projects a line onto the plane
        /// </summary>
        /// <param name="line3DToProject">The line to project</param>
        /// <returns>A projected line</returns>
        [Pure]
        public LineSegment3D Project(LineSegment3D line3DToProject)
        {
            var projectedStartPoint = this.Project(line3DToProject.StartPoint);
            var projectedEndPoint = this.Project(line3DToProject.EndPoint);
            return new LineSegment3D(projectedStartPoint, projectedEndPoint);
        }

        /// <summary>
        /// Projects a ray onto the plane
        /// </summary>
        /// <param name="rayToProject">The ray to project</param>
        /// <returns>A projected ray</returns>
        [Pure]
        public Ray3D Project(Ray3D rayToProject)
        {
            var projectedThroughPoint = this.Project(rayToProject.ThroughPoint);
            var projectedDirection = this.Project(rayToProject.Direction.ToVector3D());
            return new Ray3D(projectedThroughPoint, projectedDirection.Direction);
        }

        /// <summary>
        /// Project Vector3D onto this plane
        /// </summary>
        /// <param name="vector3DToProject">The Vector3D to project</param>
        /// <returns>The projected Vector3D</returns>
        [Pure]
        public Ray3D Project(Vector3D vector3DToProject)
        {
            var projectedEndPoint = this.Project(vector3DToProject.ToPoint3D());
            var projectedZero = this.Project(new Point3D(0, 0, 0));
            return new Ray3D(projectedZero, projectedZero.VectorTo(projectedEndPoint).Normalize());
        }

        /// <summary>
        /// Project Vector3D onto this plane
        /// </summary>
        /// <param name="vector3DToProject">The Vector3D to project</param>
        /// <returns>The projected Vector3D</returns>
        [Pure]
        public Ray3D Project(UnitVector3D vector3DToProject)
        {
            return this.Project(vector3DToProject.ToVector3D());
        }

        /// <summary>
        /// Finds the intersection of the two planes, throws if they are parallel
        /// http://mathworld.wolfram.com/Plane-PlaneIntersection.html
        /// </summary>
        /// <param name="intersectingPlane">a plane which intersects</param>
        /// <param name="tolerance">A tolerance (epsilon) to account for floating point error.</param>
        /// <returns>A ray at the intersection.</returns>
        [Pure]
        public Ray3D IntersectionWith(Plane3D intersectingPlane, double tolerance = float.Epsilon)
        {
            var a = new DenseMatrix(2, 3);
            a.SetRow(0, this.Normal.ToVector());
            a.SetRow(1, intersectingPlane.Normal.ToVector());
            var svd = a.Svd(true);
            if (svd.S[1] < tolerance)
            {
                throw new ArgumentException("Planes are parallel");
            }

            var y = new DenseMatrix(2, 1)
            {
                [0, 0] = -1 * this.D,
                [1, 0] = -1 * intersectingPlane.D
            };

            var pointOnIntersectionLine = svd.Solve(y);
            var throughPoint = Point3D.OfVector(pointOnIntersectionLine.Column(0));
            var direction = UnitVector3D.OfVector(svd.VT.Row(2));
            return new Ray3D(throughPoint, direction);
        }

        /// <summary>
        /// Find intersection between Line3D and Plane
        /// http://geomalgorithms.com/a05-_intersect-1.html
        /// </summary>
        /// <param name="line">A line segment</param>
        /// <param name="tolerance">A tolerance (epsilon) to account for floating point error.</param>
        /// <returns>Intersection Point or null</returns>
        public Point3D? IntersectionWith(Line3D line, double tolerance = float.Epsilon)
        {
            if (line.Direction.IsPerpendicularTo(this.Normal, tolerance))
            {
                // either parallel or lies in the plane
                var projectedPoint = this.Project(line.StartPoint, line.Direction);
                if (projectedPoint == line.StartPoint)
                {
                    throw new InvalidOperationException("Line lies in the plane");
                }

                // Line and plane are parallel
                return null;
            }

            var d = this.SignedDistanceTo(line.StartPoint);
            var u = line.StartPoint.VectorTo(line.EndPoint);
            var t = -1 * d / u.DotProduct(this.Normal);
            if (t > 1 || t < 0)
            {
                // They are not intersected
                return null;
            }

            return line.StartPoint + (t * u);
        }

        /// <summary>
        /// Find intersection between LineSegment3D and Plane
        /// http://geomalgorithms.com/a05-_intersect-1.html
        /// </summary>
        /// <param name="line">A line segment</param>
        /// <param name="tolerance">A tolerance (epsilon) to account for floating point error.</param>
        /// <returns>Intersection Point or null</returns>
        [Pure]
        public Point3D? IntersectionWith(LineSegment3D line, double tolerance = float.Epsilon)
        {
            if (line.Direction.IsPerpendicularTo(this.Normal, tolerance))
            {
                // either parallel or lies in the plane
                var projectedPoint = this.Project(line.StartPoint, line.Direction);
                if (projectedPoint == line.StartPoint)
                {
                    throw new InvalidOperationException("Line lies in the plane");
                }

                // Line and plane are parallel
                return null;
            }

            var d = this.SignedDistanceTo(line.StartPoint);
            var u = line.StartPoint.VectorTo(line.EndPoint);
            var t = -1 * d / u.DotProduct(this.Normal);
            if (t > 1 || t < 0)
            {
                // They are not intersected
                return null;
            }

            return line.StartPoint + (t * u);
        }

        /// <summary>
        /// http://www.cs.princeton.edu/courses/archive/fall00/cs426/lectures/raycast/sld017.htm
        /// </summary>
        /// <param name="ray">A ray</param>
        /// <param name="tolerance">A tolerance (epsilon) to account for floating point error.</param>
        /// <returns>The point of intersection.</returns>
        [Pure]
        public Point3D IntersectionWith(Ray3D ray, double tolerance = float.Epsilon)
        {
            if (this.Normal.IsPerpendicularTo(ray.Direction, tolerance))
            {
                throw new InvalidOperationException("Ray is parallel to the plane.");
            }

            var d = this.SignedDistanceTo(ray.ThroughPoint);
            var t = -1 * d / ray.Direction.DotProduct(this.Normal);
            return ray.ThroughPoint + (t * ray.Direction);
        }

        /// <summary>
        /// Returns <paramref name="p"/> mirrored about the plane.
        /// </summary>
        /// <param name="p">The <see cref="Point3D"/></param>
        /// <returns>The mirrored point.</returns>
        [Pure]
        public Point3D MirrorAbout(Point3D p)
        {
            var p2 = this.Project(p);
            var d = this.SignedDistanceTo(p);
            return p2 - (1 * d * this.Normal);
        }

        /// <summary>
        /// Rotates a plane
        /// </summary>
        /// <param name="aboutVector">The vector about which to rotate</param>
        /// <param name="angle">An angle to rotate</param>
        /// <returns>A rotated plane</returns>
        [Pure]
        public Plane3D Rotate(UnitVector3D aboutVector, Angle angle)
        {
            var rootPoint = this.RootPoint;
            var rotatedPoint = rootPoint.Rotate(aboutVector, angle);
            var rotatedPlaneVector = this.Normal.Rotate(aboutVector, angle);
            return new Plane3D(rotatedPlaneVector, rotatedPoint);
        }

        /// <summary>
        /// Returns a value to indicate if a pair of geometric planes are equal
        /// </summary>
        /// <param name="other">The geometric plane to compare against.</param>
        /// <param name="tolerance">A tolerance (epsilon) to adjust for floating point error</param>
        /// <returns>true if the geometric planes are equal; otherwise false</returns>
        [Pure]
        public bool Equals(Plane3D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.D - this.D) < tolerance && this.Normal.Equals(other.Normal, tolerance);
        }

        /// <inheritdoc />
        [Pure]
        public bool Equals(Plane3D p) => this.D.Equals(p.D) && this.Normal.Equals(p.Normal);

        /// <inheritdoc />
        [Pure]
        public override bool Equals(object obj) => obj is Plane3D p && this.Equals(p);

        /// <inheritdoc />
        [Pure]
        public override int GetHashCode() => HashCode.Combine(this.Normal, this.D);

        /// <inheritdoc />
        [Pure]
        public override string ToString()
        {
            return $"A:{Math.Round(this.A, 4)} B:{Math.Round(this.B, 4)} C:{Math.Round(this.C, 4)} D:{Math.Round(this.D, 4)}";
        }

        /// <inheritdoc />
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <inheritdoc />
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);
            this = new Plane3D(
                UnitVector3D.ReadFrom(e.SingleElement("Normal").CreateReader()),
                Point3D.ReadFrom(e.SingleElement("RootPoint").CreateReader()));
        }

        /// <inheritdoc/>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteElement("RootPoint", this.RootPoint);
            writer.WriteElement("Normal", this.Normal);
        }
    }
}
