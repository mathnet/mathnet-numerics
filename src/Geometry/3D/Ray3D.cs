namespace Geometry
{
    using System;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable]
    public struct Ray3D : IEquatable<Ray3D>, IXmlSerializable, IFormattable
    {
        public readonly Point3D ThroughPoint;

        public readonly UnitVector3D Direction;

        public Ray3D(Point3D throughPoint, UnitVector3D direction)
        {
            ThroughPoint = throughPoint;
            Direction = direction;
        }

        public Ray3D(Point3D throughPoint, Vector3D direction)
            : this(throughPoint, direction.Normalize())
        {
        }

        public static Ray3D IntersectionOf(Plane plane1, Plane plane2)
        {
            return plane1.IntersectionWith(plane2);
        }

        /// <summary>
        /// Returns the shortes line from a point to the ray
        /// </summary>
        /// <param name="point3D"></param>
        /// <returns></returns>
        public Line3D LineTo(Point3D point3D)
        {
            Vector3D v = ThroughPoint.VectorTo(point3D);
            Vector3D alongVector = v.ProjectOn(Direction);
            return new Line3D(ThroughPoint + alongVector, point3D);
        }

        public bool IsCollinear(Ray3D otherRay, double epsilon = float.Epsilon)
        {
            if (!Direction.IsParallelTo(otherRay.Direction, epsilon))
            {
                return false;
            }
            Vector3D vectorTo = ThroughPoint.VectorTo(otherRay.ThroughPoint);
            if (Math.Abs(vectorTo.Length - 0) < epsilon)
            {
                return true;
            }
            if (!vectorTo.IsParallelTo(Direction, epsilon))
            {
                return false;
            }
            return true;
        }

        public bool Equals(Ray3D other)
        {
            return Direction.Equals(other.Direction) &&
                   ThroughPoint.Equals(other.ThroughPoint);
        }

        public bool Equals(Ray3D other, double tolerance)
        {
            return Direction.Equals(other.Direction, tolerance) &&
                   ThroughPoint.Equals(other.ThroughPoint, tolerance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Ray3D && Equals((Ray3D)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ThroughPoint.GetHashCode();
                hashCode = (hashCode * 397) ^ Direction.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Ray3D left, Ray3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Ray3D left, Ray3D right)
        {
            return !left.Equals(right);
        }

        public Point3D GetPointAtSpecifiedZ(double specifiedZ)
        {
            //ThroughPoint.Z + vectorZ * t = specatZ
            //t = (specatZ - Through.z )/vector z
            //punkten blir through + vectorn*t

            var t = (specifiedZ - ThroughPoint.Z) / Direction.Z;

            var scaledDirection = new Vector3D(Direction.X, Direction.Y, Direction.Z);
            var v = scaledDirection.ScaleBy(t);

            var retPoint = ThroughPoint + v;
            return retPoint;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format(
                "ThroughPoint: {0}, Direction: {1}",
                Direction.ToString(format, formatProvider),
                ThroughPoint.ToString(format, formatProvider));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            var e = (XElement)XNode.ReadFrom(reader);
            ThroughPoint.ReadXml(e.SingleElementReader("ThroughPoint"));
            Direction.ReadXml(e.SingleElementReader("Direction"));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElement("ThroughPoint", ThroughPoint);
            writer.WriteElement("Direction", Direction);
        }
    }
}