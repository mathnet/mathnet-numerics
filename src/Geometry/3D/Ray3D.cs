namespace MathNet.Geometry
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

        public static Ray3D Parse(string point, string direction)
        {
            return new Ray3D(Point3D.Parse(point), UnitVector3D.Parse(direction));
        }

        public static Ray3D Parse(string s)
        {
            return Parser.ParseRay3D(s);
        }

        public static bool operator ==(Ray3D left, Ray3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Ray3D left, Ray3D right)
        {
            return !left.Equals(right);
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

        public bool IsCollinear(Ray3D otherRay, double tolerance = float.Epsilon)
        {
            return Direction.IsParallelTo(otherRay.Direction, tolerance);
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
                ThroughPoint.ToString(format, formatProvider),
                Direction.ToString(format, formatProvider));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);
            XmlExt.SetReadonlyField(ref this, l => l.ThroughPoint, Point3D.ReadFrom(e.SingleElement("ThroughPoint").CreateReader()));
            XmlExt.SetReadonlyField(ref this, l => l.Direction, UnitVector3D.ReadFrom(e.SingleElement("Direction").CreateReader()));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElement("ThroughPoint", ThroughPoint);
            writer.WriteElement("Direction", Direction);
        }
    }
}