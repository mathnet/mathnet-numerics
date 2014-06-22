namespace Geometry
{
    using System;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable]
    public struct Line3D : IEquatable<Line3D>, IXmlSerializable
    {
        public readonly Point3D StartPoint;
        public readonly Point3D EndPoint;
        private double _length;
        private UnitVector3D _direction;

        /// <summary>
        /// Throws if StartPoint == EndPoint
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        public Line3D(Point3D startPoint, Point3D endPoint)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            if (StartPoint == EndPoint)
            {
                throw new ArgumentException("StartPoint == EndPoint");
            }
            _length = -1.0;
            _direction = new UnitVector3D();
        }

        public double Length
        {
            get
            {
                if (_length < 0)
                {
                    var vectorTo = StartPoint.VectorTo(EndPoint);
                    _length = vectorTo.Length;
                    if (_length > 0)
                    {
                        _direction = vectorTo.Normalize();
                    }
                }
                return _length;
            }
        }

        public UnitVector3D Direction
        {
            get
            {
                if (_length < 0)
                {
                    _length = Length; // Side effect hack
                }
                return _direction;
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
            return obj is Line3D && Equals((Line3D)obj);
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
                var hashCode = StartPoint.GetHashCode();
                hashCode = (hashCode * 397) ^ EndPoint.GetHashCode();
                return hashCode;
            }
        }
        public XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(XmlReader reader)
        {
            var e = (XElement)XNode.ReadFrom(reader);
            var p = new Point3D(e.Descendants("StartPoint").Single().CreateReader());

            StartPoint.ReadXml(e.Descendants("StartPoint").Single().CreateReader());
            EndPoint.ReadXml(e.Descendants("EndPoint").Single().CreateReader());
        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElement("StartPoint", StartPoint);
            writer.WriteElement("EndPoint", EndPoint);
        }
    }
}
