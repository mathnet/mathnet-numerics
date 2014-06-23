namespace MathNet.Geometry.Units
{
    using System;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable]
    public struct Angle : IComparable<Angle>, IEquatable<Angle>, IFormattable, IXmlSerializable
    {
        public readonly double Radians;
      
        /// <summary>
        /// Default is serializing as attributes, set to true for elements
        /// </summary>
        public bool SerializeAsElements;

        private Angle(double radians)
        {
            Radians = radians;
            SerializeAsElements = false;
        }

        public Angle(double radians, Radians unit)
        {
            Radians = radians;
            SerializeAsElements = false;
        }

        public Angle(double radians, Degrees unit)
        {
            Radians = UnitConverter.ConvertFrom(radians, unit);
            SerializeAsElements = false;
        }

        [Obsolete("This boxes, use Angle.From() instead")]
        public Angle(double radians, IAngleUnit unit)
        {
            Radians = UnitConverter.ConvertFrom(radians, unit);
            SerializeAsElements = false;
        }

        public double Degrees
        {
            get
            {
                return UnitConverter.ConvertTo(Radians, AngleUnit.Degrees);
            }
        }

        /// <summary>
        /// Creates an Angle from it's string representation
        /// </summary>
        /// <param name="s">The string representation of the angle</param>
        /// <returns></returns>
        public static Angle Parse(string s)
        {
            return UnitParser.Parse(s, From);
        }

        public static Angle From<T>(double value, T unit) where T : IAngleUnit
        {
            return new Angle(UnitConverter.ConvertFrom(value, unit));
        }

        public static Angle FromDegrees(double value)
        {
            return new Angle(UnitConverter.ConvertFrom(value, AngleUnit.Degrees));
        }

        public static Angle FromRadians(double value)
        {
            return new Angle(value);
        }

        public static bool operator ==(Angle left, Angle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Angle left, Angle right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(Angle left, Angle right)
        {
            return left.Radians < right.Radians;
        }

        public static bool operator >(Angle left, Angle right)
        {
            return left.Radians > right.Radians;
        }

        public static bool operator <=(Angle left, Angle right)
        {
            return left.Radians <= right.Radians;
        }

        public static bool operator >=(Angle left, Angle right)
        {
            return left.Radians >= right.Radians;
        }

        public static Angle operator *(double left, Angle right)
        {
            return new Angle(left * right.Radians);
        }

        [Obsolete("Not sure this is nice")]
        public static Angle operator *(Angle left, double right)
        {
            return new Angle(left.Radians * right);
        }

        public static Angle operator /(Angle left, double right)
        {
            return new Angle(left.Radians / right);
        }

        public static Angle operator +(Angle left, Angle right)
        {
            return new Angle(left.Radians + right.Radians);
        }

        public static Angle operator -(Angle left, Angle right)
        {
            return new Angle(left.Radians - right.Radians);
        }

        ////[Obsolete("Will sweet when doing Math.Cos(angle) but opens the door for 1 + angle, prolly remove")]

        ////public static implicit operator double(Angle a)
        ////{
        ////    return a.Value;
        ////}

        public override string ToString()
        {
            return ToString((string)null, (IFormatProvider)NumberFormatInfo.CurrentInfo);
        }

        public string ToString(string format)
        {
            return ToString(format, (IFormatProvider)NumberFormatInfo.CurrentInfo);
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString((string)null, (IFormatProvider)NumberFormatInfo.GetInstance(provider));
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString(format, formatProvider, AngleUnit.Radians);
        }

        public string ToString<T>(string format, IFormatProvider formatProvider, T unit) where T : IAngleUnit
        {
            var value = UnitConverter.ConvertTo(Radians, unit);
            return string.Format("{0}{1}", value.ToString(format, formatProvider), unit.ShortName);
        }

        public int CompareTo(Angle other)
        {
            return Radians.CompareTo(other.Radians);
        }

        public bool Equals(Angle other)
        {
            return Radians.Equals(other.Radians);
        }

        public bool Equals(Angle other, double tolerance)
        {
            return Math.Abs(Radians - other.Radians) < tolerance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is Angle && this.Equals((Angle)obj);
        }

        public override int GetHashCode()
        {
            return Radians.GetHashCode();
        }

        /// <summary>
        /// This method is reserved and should not be used. When implementing the IXmlSerializable interface, 
        /// you should return null (Nothing in Visual Basic) from this method, and instead, 
        /// if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the
        ///  <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> 
        /// method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
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
            XmlExt.SetReadonlyField(ref this, x => x.Radians, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("Value")));
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
        public void WriteXml(XmlWriter writer)
        {
            if (SerializeAsElements)
            {
                writer.WriteElementString("Value", Radians.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteAttribute("Value", Radians);
            }
        }

        public static Angle ReadFrom(XmlReader reader)
        {
            var v = new Angle();
            v.ReadXml(reader);
            return v;
        }
    }
}