namespace MathNet.Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using MathNet.Numerics.LinearAlgebra;
    using MathNet.Numerics.LinearAlgebra.Double;
    using Units;

    [Serializable]
    public struct Point3D : IXmlSerializable, IEquatable<Point3D>, IFormattable
    {
        #region Common

        /// <summary>
        /// Default is serializing as attributes, set to true for elements
        /// </summary>
        public bool SerializeAsElements;

        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double X;

        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double Z;

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            SerializeAsElements = false;
        }

        public Point3D(IEnumerable<double> data)
            : this(data.ToArray())
        {
        }

        public Point3D(double[] data)
            : this(data[0], data[1], data[2])
        {
            if (data.Length != 3)
            {
                throw new ArgumentException("Size must be 3");
            }
        }

        /// <summary>
        /// Returns a copy of the inner dense vector
        /// </summary>
        public DenseVector ToDenseVector()
        {
            return new DenseVector(new[] { X, Y, Z });
        }

        public static Point3D Parse(string value)
        {
            var doubles = Parser.ParseItem3D(value);
            return new Point3D(doubles);
        }

        public static bool operator ==(Point3D left, Point3D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point3D left, Point3D right)
        {
            return !left.Equals(right);
        }

        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Matrix<double> left, Point3D right)
        {
            return left * right.ToDenseVector();
        }

        [Obsolete("Not sure this is nice")]
        public static Vector<double> operator *(Point3D left, Matrix<double> right)
        {
            return left.ToDenseVector() * right;
        }

        public override string ToString()
        {
            return ToString(null, CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString(null, provider);
        }

        public string ToString(string format, IFormatProvider provider = null)
        {
            var numberFormatInfo = provider != null ? NumberFormatInfo.GetInstance(provider) : CultureInfo.InvariantCulture.NumberFormat;
            string separator = numberFormatInfo.NumberDecimalSeparator == "," ? ";" : ",";
            return string.Format("({0}{1} {2}{1} {3})",  X.ToString(format, numberFormatInfo), separator, Y.ToString(format, numberFormatInfo), Z.ToString(format, numberFormatInfo));
        }

        public bool Equals(Point3D other)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return X == other.X && Y == other.Y && Z == other.Z;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool Equals(Point3D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.X - X) < tolerance &&
                   Math.Abs(other.Y - Y) < tolerance &&
                   Math.Abs(other.Z - Z) < tolerance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Point3D && Equals((Point3D)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
        /// </returns>
        public XmlSchema GetSchema() { return null; }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);
            // Hacking set readonly fields here, can't think of a cleaner workaround
            double x = XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("X"));
            double y = XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("Y"));
            double z = XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("Z"));

            XmlExt.SetReadonlyFields(ref this, new[] { "X", "Y", "Z" }, new[] { x, y, z });
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
        public void WriteXml(XmlWriter writer)
        {
            if (SerializeAsElements)
            {
                writer.WriteElementString("X", X.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Y", Y.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Z", Z.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteAttribute("X", X);
                writer.WriteAttribute("Y", Y);
                writer.WriteAttribute("Z", Z);
            }
        }

        public static Point3D ReadFrom(XmlReader reader)
        {
            var p = new Point3D();
            p.ReadXml(reader);
            return p;
        }

        #endregion Common

        public static Point3D Origin
        {
            get
            {
                return new Point3D(0, 0, 0);
            }
        }

        public static Point3D NaN
        {
            get
            {
                return new Point3D(double.NaN, double.NaN, double.NaN);
            }
        }

        public static Point3D Centroid(IEnumerable<Point3D> points)
        {
            return Centroid(points.ToArray());
        }

        public static Point3D Centroid(params Point3D[] points)
        {
            return new Point3D(points.Average(point => point.X),
                              points.Average(point => point.Y),
                              points.Average(point => point.Z));
        }

        public static Point3D MidPoint(Point3D p1, Point3D p2)
        {
            return Centroid(p1, p2);
        }

        public static Point3D ItersectionOf(Plane plane1, Plane plane2, Plane plane3)
        {
            var ray = plane1.IntersectionWith(plane2);
            return plane3.IntersectionWith(ray);
        }

        public static Point3D ItersectionOf(Plane plane, Ray3D ray)
        {
            return plane.IntersectionWith(ray);
        }

        public static Point3D operator +(Point3D p, Vector3D v)
        {
            return new Point3D(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
        }

        public static Point3D operator +(Point3D p, UnitVector3D v)
        {
            return new Point3D(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
        }

        public static Point3D operator -(Point3D p, Vector3D v)
        {
            return new Point3D(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
        }

        public static Point3D operator -(Point3D p, UnitVector3D v)
        {
            return new Point3D(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
        }

        public static Vector3D operator -(Point3D lhs, Point3D rhs)
        {
            return new Vector3D(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        }

        // Not sure a ref to System.Windows.Media.Media3D is nice
        ////public static explicit operator Point3D(System.Windows.Media.Media3D.Point3D p)
        ////{
        ////    return new Point3D(p.X, p.Y, p.Z);
        ////}

        ////public static explicit operator System.Windows.Media.Media3D.Point3D(Point3D p)
        ////{
        ////    return new System.Windows.Media.Media3D.Point3D(p.X, p.Y, p.Z);
        ////}

        public Point3D MirrorAbout(Plane plane)
        {
            return plane.MirrorAbout(this);
        }

        public Point3D ProjectOn(Plane plane)
        {
            return plane.Project(this);
        }

        public Point3D Rotate(Vector3D aboutVector, Angle angle)
        {
            return Rotate(aboutVector.Normalize(), angle);
        }

        public Point3D Rotate(UnitVector3D aboutVector, Angle angle)
        {
            var cs = CoordinateSystem.Rotation(angle, aboutVector);
            return cs.Transform(this);
        }

        [Pure]
        public Vector3D VectorTo(Point3D p)
        {
            return p - this;
        }

        public double DistanceTo(Point3D p)
        {
            var vector = VectorTo(p);
            return vector.Length;
        }

        public Vector3D ToVector()
        {
            return new Vector3D(X, Y, Z);
        }

        public Point3D TransformBy(CoordinateSystem cs)
        {
            return cs.Transform(this);
        }

        public Point3D TransformBy(Matrix<double> m)
        {
            return new Point3D(m.Multiply(this.ToDenseVector()));
        }
    }
}