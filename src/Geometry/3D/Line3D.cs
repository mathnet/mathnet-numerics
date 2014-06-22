namespace Geometry
{
    using System;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable]
    public class Line3D : IEquatable<Line3D>, IXmlSerializable
    {
        private readonly Point3D _startPoint = new Point3D(double.NaN, double.NaN, double.NaN);
        private readonly Point3D _endPoint = new Point3D(double.NaN, double.NaN, double.NaN);
        public Line3D() { }
        public Line3D(Point3D startPoint, Point3D endPoint, string name = null)
        {
            Name = name;
            _startPoint = startPoint;
            _endPoint = endPoint;
        }
        public string Name { get; set; }
        public Point3D StartPoint
        {
            get
            {
                return _startPoint;
            }
        }
        public Point3D EndPoint
        {
            get
            {
                return _endPoint;
            }
        }
        public double Length
        {
            get
            {
                return StartPoint.DistanceTo(EndPoint);
            }
        }
        [XmlIgnore]
        public virtual UnitVector3D Direction
        {
            get
            {
                return StartPoint.VectorTo(EndPoint).Normalize();
            }
        }
        public static bool operator ==(Line3D left, Line3D right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(Line3D left, Line3D right)
        {
            return !Equals(left, right);
        }
        /// <summary>
        /// Returns the shortes line from a point to the ray
        /// </summary>
        /// <param name="p"></param>
        /// <param name="mustStartBetweenStartAndEnd">If false the startpoint can be on the line extending beyond the start and endpoint of the line</param>
        /// <returns></returns>
        public Line3D LineTo(Point3D p, bool mustStartBetweenStartAndEnd)
        {
            Vector3D v = StartPoint.VectorTo(p);
            double dotProduct = v.DotProduct(Direction);
            if (mustStartBetweenStartAndEnd)
            {
                if (dotProduct < 0)
                    dotProduct = 0;
                var l = Length;
                if (dotProduct > l)
                    dotProduct = l;
            }
            var alongVector = dotProduct * Direction;
            return new Line3D(StartPoint + alongVector, p);
        }
        public Line3D ProjectOn(Plane plane)
        {
            return plane.Project(this);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Line3D other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.StartPoint, StartPoint) && Equals(other.EndPoint, EndPoint);
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
            if (obj.GetType() != typeof(Line3D)) return false;
            return Equals((Line3D)obj);
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
                return ((StartPoint != null ? StartPoint.GetHashCode() : 0) * 397) ^ (EndPoint != null ? EndPoint.GetHashCode() : 0);
            }
        }
        public XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(XmlReader reader)
        {
            var e = (XElement)XNode.ReadFrom(reader);
            Name = e.ReadAttributeOrDefault("Name");
            StartPoint.ReadXml(e.Descendants("StartPoint").Single().CreateReader());
            EndPoint.ReadXml(e.Descendants("EndPoint").Single().CreateReader());
        }
        public void WriteXml(XmlWriter writer)
        {
            if (Name != null)
                writer.WriteAttributeString("Name", Name);
            writer.WriteElement("StartPoint", StartPoint);
            writer.WriteElement("EndPoint", EndPoint);
        }
    }
}
