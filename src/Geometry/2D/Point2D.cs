namespace MathNet.Geometry
{
    using System;
    using System.Collections.Generic;
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
    public struct Point2D : IXmlSerializable, IEquatable<Point2D>, IFormattable
    {
        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double X;

        /// <summary>
        /// Using public fields cos: http://blogs.msdn.com/b/ricom/archive/2006/08/31/performance-quiz-11-ten-questions-on-value-based-programming.aspx
        /// </summary>
        public readonly double Y;

        /// <summary>
        /// Default is serializing as attributes, set to true for elements
        /// </summary>
        public bool SerializeAsElements;

        public Point2D(double x, double y)
        {
            this.X = x;
            this.Y = y;
            this.SerializeAsElements = false;
        }

        /// <summary>
        /// Creates a point r from origin rotated a counterclockwise from X-Axis
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        public Point2D(double r, Angle a)
            : this(r * Math.Cos(a.Radians), r * Math.Sin(a.Radians))
        {
        }

        public Point2D(IEnumerable<double> data)
            : this(data.ToArray())
        {
        }

        public Point2D(double[] data)
            : this(data[0], data[1])
        {
            if (data.Length != 2)
            {
                throw new ArgumentException("data.Length != 2!");
            }
        }

        public DenseVector ToDenseVector()
        {
            return new DenseVector(new[] { this.X, this.Y });
        }

        public static Point2D Parse(string value)
        {
            var doubles = Parser.ParseItem2D(value);
            return new Point2D(doubles);
        }

        public static bool operator ==(Point2D left, Point2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Point2D left, Point2D right)
        {
            return !left.Equals(right);
        }

        public Point2D TransformBy(Matrix<double> m)
        {
            var transformed = m.Multiply(this.ToDenseVector());
            return new Point2D(transformed);
        }

        public override string ToString()
        {
            return this.ToString(null, CultureInfo.InvariantCulture);
        }

        public string ToString(IFormatProvider provider)
        {
            return this.ToString(null, provider);
        }

        public string ToString(string format, IFormatProvider provider = null)
        {
            var numberFormatInfo = provider != null ? NumberFormatInfo.GetInstance(provider) : CultureInfo.InvariantCulture.NumberFormat;
            string separator = numberFormatInfo.NumberDecimalSeparator == "," ? ";" : ",";
            return string.Format("({0}{1} {2})", this.X.ToString(format, numberFormatInfo), separator, this.Y.ToString(format, numberFormatInfo));
        }

        public bool Equals(Point2D other)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return this.X == other.X && this.Y == other.Y;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool Equals(Point2D other, double tolerance)
        {
            if (tolerance < 0)
            {
                throw new ArgumentException("epsilon < 0");
            }

            return Math.Abs(other.X - this.X) < tolerance &&
                   Math.Abs(other.Y - this.Y) < tolerance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Point2D && this.Equals((Point2D)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.X.GetHashCode() * 397) ^ this.Y.GetHashCode();
            }
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized. </param>
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var e = (XElement)XNode.ReadFrom(reader);

            // Hacking set readonly fields here, can't think of a cleaner workaround
            XmlExt.SetReadonlyField(ref this, x => x.X, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("X")));
            XmlExt.SetReadonlyField(ref this, x => x.Y, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("Y")));
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
        public void WriteXml(XmlWriter writer)
        {
            if (this.SerializeAsElements)
            {
                writer.WriteElementString("X", this.X.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Y", this.Y.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteAttribute("X", this.X);
                writer.WriteAttribute("Y", this.Y);
            }
        }

        public static Point2D ReadFrom(XmlReader reader)
        {
            var v = new Point2D();
            v.ReadXml(reader);
            return v;
        }

        public static Point2D Origin
        {
            get
            {
                return new Point2D(0, 0);
            }
        }

        public static Point2D Centroid(IEnumerable<Point2D> points)
        {
            return Centroid(points.ToArray());
        }

        public static Point2D Centroid(params Point2D[] points)
        {
            return new Point2D(
                points.Average(point => point.X),
                points.Average(point => point.Y));
        }

        public static Point2D MidPoint(Point2D point1, Point2D point2)
        {
            return Centroid(point1, point2);
        }

        public static Point2D operator +(Point2D point, Vector3D vector)
        {
            return new Point2D(point.X + vector.X, point.Y + vector.Y);
        }

        public static Point2D operator -(Point2D point, Vector3D vector)
        {
            return new Point2D(point.X - vector.X, point.Y - vector.Y);
        }

        public static Vector2D operator -(Point2D lhs, Point2D rhs)
        {
            return new Vector2D(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public Vector2D VectorTo(Point2D otherPoint)
        {
            return otherPoint - this;
        }

        public double DistanceTo(Point2D otherPoint)
        {
            var vector = this.VectorTo(otherPoint);
            return vector.Length;
        }

        public Vector2D ToVector()
        {
            return new Vector2D(this.X, this.Y);
        }
       
        /// <summary>
        /// return new Point3D(X, Y, 0);
        /// </summary>
        /// <returns>return new Point3D(X, Y, 0);</returns>
        public Point3D ToPoint3D()
        {
            return new Point3D(this.X, this.Y, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <returns>return cs.Transform(this.ToPoint3D());</returns>
        public Point3D TransformBy(CoordinateSystem cs)
        {
            return cs.Transform(this.ToPoint3D());
        }
    }
}