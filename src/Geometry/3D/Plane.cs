namespace Geometry
{
    using System;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MathNet.Numerics.LinearAlgebra.Factorization;
    using Units;

    [Serializable]
    public class Plane : IEquatable<Plane>, IXmlSerializable
    {
        private readonly UnitVector3D _normal;
        private readonly Point3D _rootPoint;
        private static string _item3DPattern = Parser.Vector3DPattern.Trim('^', '$');
        public static readonly string PlanePattern = string.Format(@"^ *p: *{{(?<p>{0})}} *v: *{{(?<v>{0})}} *$", _item3DPattern);
        public Plane()
        {
            _rootPoint = new Point3D(double.NaN, double.NaN, double.NaN);
            _normal = new UnitVector3D(double.NaN, double.NaN, double.NaN);
        }
        public Plane(double a, double b, double c, double d)
            : this(new UnitVector3D(a, b, c), -d)
        {
        }
        public Plane(UnitVector3D normal, double offset = 0)
        {
            _normal = normal;
            _rootPoint = (offset * _normal).ToPoint3D();
        }
        public Plane(UnitVector3D normal, Point3D rootPoint)
            : this(rootPoint, normal)
        {
        }
        public Plane(Point3D rootPoint, UnitVector3D normal)
        {
            _rootPoint = rootPoint;
            _normal = normal;
        }
        public static Plane Parse(string s)
        {
            var match = Regex.Match(s, PlanePattern);
            var p = Point3D.Parse(match.Groups["p"].Value);
            var uv = UnitVector3D.Parse(match.Groups["v"].Value);
            return new Plane(p, uv);
        }

        public double A
        {
            get
            {
                return Normal.X;
            }
        }

        public double B
        {
            get
            {
                return Normal.Y;
            }
        }

        public double C
        {
            get
            {
                return Normal.Z;
            }
        }

        public double D
        {
            get
            {
                var vector = RootPoint.ToVector();
                return -vector.DotProduct(Normal);
            }
            //private set { RootPoint = new Point3D(Normal.Multiply(-value)); }
        }

        public UnitVector3D Normal
        {
            get
            {
                return _normal;
            }
        }

        public Point3D RootPoint
        {
            get
            {
                return _rootPoint;
            }
        }

        public static bool operator ==(Plane left, Plane right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Plane left, Plane right)
        {
            return !Equals(left, right);
        }

        public double SignedDistanceTo(Point3D point)
        {
            var point3D = Project(point);
            var vectorTo = point3D.VectorTo(point);
            return vectorTo.DotProduct(Normal);
        }

        public double SignedDistanceTo(Plane otherPlane)
        {
            if (!Normal.IsParallelTo(otherPlane.Normal, tolerance: 1E-15))
                throw new ArgumentException("Planes are not paralell");
            return SignedDistanceTo(otherPlane.RootPoint);
        }

        public double SignedDistanceTo(Ray3D ray)
        {
            if (Math.Abs(ray.Direction.DotProduct(Normal) - 0) < 1E-15)
                return SignedDistanceTo(ray.ThroughPoint);
            return 0;
        }

        public double AbsoluteDistanceTo(Point3D point)
        {
            return Math.Abs(SignedDistanceTo(point));
        }

        public Point3D Project(Point3D p, UnitVector3D? projectionDirection = null)
        {
            double dotProduct = Normal.DotProduct(p.ToVector());
            var projectiononNormal = projectionDirection == null ? Normal : projectionDirection.Value;
            var projectionVector = (dotProduct + D) * projectiononNormal;
            return p - projectionVector;
        }

        public Line3D Project(Line3D line3DToProject)
        {
            var projectedStartPoint = Project(line3DToProject.StartPoint);
            var projectedEndPoint = Project(line3DToProject.EndPoint);
            return new Line3D(projectedStartPoint, projectedEndPoint);
        }

        public Ray3D Project(Ray3D rayToProject)
        {
            var projectedThroughPoint = Project(rayToProject.ThroughPoint);
            var projectedDirection = Project(rayToProject.Direction.ToVector3D());
            return new Ray3D(projectedThroughPoint, projectedDirection.Direction);
        }

        /// <summary>
        /// Project Vector3D onto this plane
        /// </summary>
        /// <param name="vector3DToProject">The Vector3D to project</param>
        /// <returns>The projected Vector3D</returns>
        public Ray3D Project(Vector3D vector3DToProject)
        {
            var projectedEndPoint = Project(vector3DToProject.ToPoint3D());
            var projectedZero = Project(new Point3D(0, 0, 0));
            return new Ray3D(projectedZero, projectedZero.VectorTo(projectedEndPoint).Normalize());
        }

        /// <summary>
        /// Finds the intersection of the two planes, throws if they are paralell
        /// </summary>
        /// <param name="intersectingPlane"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public Ray3D IntersectionWith(Plane intersectingPlane, double tolerance = float.Epsilon)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// http://www.cs.princeton.edu/courses/archive/fall00/cs426/lectures/raycast/sld017.htm
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public Point3D IntersectionWith(Ray3D ray, double tolerance = float.Epsilon)
        {
            var d = SignedDistanceTo(ray.ThroughPoint);
            var t = -1 * d / ray.Direction.DotProduct(Normal);
            return ray.ThroughPoint + t * ray.Direction;
        }

        /// <summary>
        /// Calculate the point of intersection of two lines projected onto this plane.
        /// </summary>
        /// <returns>Intersection point</returns>
        public Point3D IntersectionOf(Line3D first, Line3D second)
        {
            first = first.ProjectOn(this);
            second = second.ProjectOn(this);
            Point3D[] p = new Point3D[]
          {
            first.StartPoint,
            first.EndPoint,
            second.StartPoint,
            second.EndPoint,
          };

            // http://mathworld.wolfram.com/Line3D-LineIntersection.html
            double a = p[0].X * p[1].Y - p[0].Y * p[1].X;
            double b = p[2].X * p[3].Y - p[2].Y * p[3].X;
            double c = (p[0].X - p[1].X) * (p[2].Y - p[3].Y) - (p[0].Y - p[1].Y) * (p[2].X - p[3].X);
            if (c == 0)
                throw new Exception("Lines do not intersect");
            double x = (a * (p[2].X - p[3].X) - b * (p[0].X - p[1].X)) / c;
            double y = (a * (p[2].Y - p[3].Y) - b * (p[0].Y - p[1].Y)) / c;

            double z = -(A * x + B * y + D) / C;
            return new Point3D(x, y, z);
        }

        public Point3D MirrorAbout(Point3D p)
        {
            Point3D p2 = Project(p);
            double d = SignedDistanceTo(p);
            return p2 - 1 * d * Normal;
        }

        public Plane Rotate(UnitVector3D aboutVector, Angle angle)
        {
            var rootPoint = RootPoint;
            var rotatedPoint = rootPoint.Rotate(aboutVector, angle);
            var rotatedPlaneVector = Normal.Rotate(aboutVector, angle);
            return new Plane(rotatedPlaneVector, rotatedPoint);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Plane other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Math.Abs(other.A - A) >= float.Epsilon) return false;
            if (Math.Abs(other.C - C) >= float.Epsilon) return false;
            if (Math.Abs(other.B - B) >= float.Epsilon) return false;
            return Math.Abs(other.D - D) < float.Epsilon;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Plane)) return false;
            return Equals((Plane)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = A.GetHashCode();
                result = (result * 397) ^ C.GetHashCode();
                result = (result * 397) ^ B.GetHashCode();
                result = (result * 397) ^ D.GetHashCode();
                return result;
            }
        }

        public static Point3D PointFromPlanes(Plane plane1, Plane plane2, Plane plane3)
        {
            return Point3D.ItersectionOf(plane1, plane2, plane3);
        }

        public override string ToString()
        {
            return string.Format("A:{0} B:{1} C:{2} D:{3}", Math.Round(A, 4), Math.Round(B, 4), Math.Round(C, 4), Math.Round(D, 4));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var e = (XElement)XNode.ReadFrom(reader);
            RootPoint.ReadXml(e.SingleElementReader("RootPoint"));
            Normal.ReadXml(e.SingleElementReader("Normal"));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElement("RootPoint", RootPoint);
            writer.WriteElement("Normal", Normal);
        }
    }
}
