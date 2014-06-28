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
    public struct Vector2D : IXmlSerializable, IEquatable<Vector2D>, IFormattable
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

        private static readonly Vector2D CachedXAxis = new Vector2D(1, 0);
        private static readonly Vector2D CachedYAxis = new Vector2D(0, 1);

        public Vector2D(double x, double y)
        {
            this.X = x;
            this.Y = y;
            this.SerializeAsElements = false;
        }

        /// <summary>
        /// Creates a vector with length r rotated a counterclockwise from X-Axis
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a"></param>
        /// <param name="name"></param>
        public Vector2D(double r, Angle a)
            : this(r * Math.Cos(a.Radians), r * Math.Sin(a.Radians))
        {
        }

        public Vector2D(IEnumerable<double> data)
            : this(data.ToArray())
        {
        }

        public Vector2D(double[] data)
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

        public static Vector2D Parse(string value)
        {
            var doubles = Parser.ParseItem2D(value);
            return new Vector2D(doubles);
        }

        public static bool operator ==(Vector2D left, Vector2D right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2D left, Vector2D right)
        {
            return !left.Equals(right);
        }

        public Vector2D TransformBy(Matrix<double> m)
        {
            var transformed = m.Multiply(this.ToDenseVector());
            return new Vector2D(transformed);
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

        public bool Equals(Vector2D other)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return this.X == other.X && this.Y == other.Y;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }

        public bool Equals(Vector2D other, double tolerance)
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

            return obj is Vector2D && this.Equals((Vector2D)obj);
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

        public static Vector2D ReadFrom(XmlReader reader)
        {
            var v = new Vector2D();
            v.ReadXml(reader);
            return v;
        }

        public static Vector2D XAxis
        {
            get
            {
                return CachedXAxis;
            }
        }

        public static Vector2D YAxis
        {
            get
            {
                return CachedYAxis;
            }
        }

        public static Vector2D operator +(Vector2D v1, Vector2D v2)
        {
            return v1.Add(v2);
        }

        public static Vector2D operator -(Vector2D v1, Vector2D v2)
        {
            return v1.Subtract(v2);
        }

        public static Vector2D operator -(Vector2D v)
        {
            return v.Negate();
        }

        public static Vector2D operator *(double d, Vector2D v)
        {
            return new Vector2D(d * v.X, d * v.Y);
        }

        public static Vector2D operator /(Vector2D v, double d)
        {
            return new Vector2D(v.X / d, v.Y / d);
        }

        public bool IsParallelTo(Vector2D othervector, double tolerance = 1.40129846432482E-45)
        {
            var @this = this.Normalize();
            var other = othervector.Normalize();
            var dp = Math.Abs(@this.DotProduct(other));
            return Math.Abs(1 - dp) < tolerance;
        }

        public bool IsPerpendicularTo(Vector2D othervector, double tolerance = 1.40129846432482E-45)
        {
            var @this = this.Normalize();
            var other = othervector.Normalize();
            return Math.Abs(@this.DotProduct(other)) < tolerance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v2"></param>
        /// <param name="clockWise">Positive in clockwisedirection</param>
        /// <param name="returnNegative">If angle is > 180° a negative value is returned</param>
        /// <returns></returns>
        public Angle SignedAngleTo(Vector2D v2, bool clockWise, bool returnNegative = false)
        {
            int sign = clockWise ? -1 : 1;
            double a1 = Math.Atan2(this.Y, this.X) * sign;
            double a2 = Math.Atan2(v2.Y, v2.X) * sign;
            double a = a2 - a1;
            if (a < 0 && !returnNegative)
            {
                a += 2 * Math.PI;
            }

            return new Angle(a, AngleUnit.Radians);
        }

        public Angle AngleTo(Vector2D toVector2D)
        {
            var @this = this.Normalize();
            var @other = toVector2D.Normalize();
            return new Angle(Math.Acos(@this.DotProduct(@other)), AngleUnit.Radians);
        }

        public Vector2D Rotate<T>(double angle, T angleUnit) where T : IAngleUnit
        {
            return this.Rotate(Angle.From(angle, angleUnit));
        }

        public Vector2D Rotate(Angle angle)
        {
            var cs = Math.Cos(angle.Radians);
            var sn = Math.Sin(angle.Radians);
            var x = (this.X * cs) - (this.Y * sn);
            var y = (this.X * sn) + (this.Y * cs);
            return new Vector2D(x, y);
        }

        public double Length
        {
            get
            {
                return Math.Sqrt((this.X * this.X) + (this.Y * this.Y));
            }
        }

        public double DotProduct(Vector2D other)
        {
            return (this.X * other.X) + (this.Y * other.Y);
        }

        public Vector2D Normalize()
        {
            var l = this.Length;
            return new Vector2D(this.X / l, this.Y / l);
        }

        public Vector2D ScaleBy(double d)
        {
            return new Vector2D(d * this.X, d * this.Y);
        }

        public Vector2D Negate()
        {
            return new Vector2D(-1 * this.X, -1 * this.Y);
        }

        public Vector2D Subtract(Vector2D v)
        {
            return new Vector2D(this.X - v.X, this.Y - v.Y);
        }

        public Vector2D Add(Vector2D v)
        {
            return new Vector2D(this.X + v.X, this.Y + v.Y);
        }
    }
}
