namespace MathNet.Geometry
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
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            if (this.StartPoint == this.EndPoint)
            {
                throw new ArgumentException("StartPoint == EndPoint");
            }

            this._length = -1.0;
            this._direction = new UnitVector3D();
        }

        public double Length
        {
            get
            {
                if (this._length < 0)
                {
                    var vectorTo = this.StartPoint.VectorTo(this.EndPoint);
                    this._length = vectorTo.Length;
                    if (this._length > 0)
                    {
                        this._direction = vectorTo.Normalize();
                    }
                }

                return this._length;
            }
        }

        public UnitVector3D Direction
        {
            get
            {
                if (this._length < 0)
                {
                    this._length = this.Length; // Side effect hack
                }

                return this._direction;
            }
        }

        public static Line3D Parse(string startPoint, string endPoint)
        {
            return new Line3D(Point3D.Parse(startPoint), Point3D.Parse(endPoint));
        }

        public static bool operator ==(Line3D left, Line3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Line3D left, Line3D right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the shortes line to a point
        /// </summary>
        /// <param name="p"></param>
        /// <param name="mustStartBetweenStartAndEnd">If false the startpoint can be on the line extending beyond the start and endpoint of the line</param>
        /// <returns></returns>
        public Line3D LineTo(Point3D p, bool mustStartBetweenStartAndEnd)
        {
            Vector3D v = this.StartPoint.VectorTo(p);
            double dotProduct = v.DotProduct(this.Direction);
            if (mustStartBetweenStartAndEnd)
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
            return new Line3D(this.StartPoint + alongVector, p);
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
            return this.StartPoint.Equals(other.StartPoint) && this.EndPoint.Equals(other.EndPoint);
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
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Line3D && this.Equals((Line3D)obj);
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
                var hashCode = this.StartPoint.GetHashCode();
                hashCode = (hashCode * 397) ^ this.EndPoint.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("StartPoint: {0}, EndPoint: {1}", this.StartPoint, this.EndPoint);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
        
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);
            XmlExt.SetReadonlyField(ref this, l => l.StartPoint, Point3D.ReadFrom(e.SingleElement("StartPoint").CreateReader()));
            XmlExt.SetReadonlyField(ref this, l => l.EndPoint, Point3D.ReadFrom(e.SingleElement("EndPoint").CreateReader()));
        }
        
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElement("StartPoint", this.StartPoint);
            writer.WriteElement("EndPoint", this.EndPoint);
        }
    }
}
