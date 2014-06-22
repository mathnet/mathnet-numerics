namespace Geometry.Units
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
        public readonly double Value;
        /// <summary>
        /// Default is serializing as attributes, set to true for elements
        /// </summary>
        public bool SerializeAsElements;

        private Angle(double value)
        {
            Value = value;
            SerializeAsElements = false;
        }

        public Angle(double value, Radians unit)
        {
            Value = value;
            SerializeAsElements = false;
        }

        public Angle(double value, Degrees unit)
        {
            Value = UnitConverter.ConvertFrom(value, unit);
            SerializeAsElements = false;
        }

        [Obsolete("This boxes, use Angle.From() instead")]
        public Angle(double value, IAngleUnit unit)
        {
            Value = UnitConverter.ConvertFrom(value, unit);
            SerializeAsElements = false;
        }

        public double Degrees
        {
            get
            {
                return UnitConverter.ConvertTo(Value, AngleUnit.Degrees);
            }
        }

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
            return left.Value < right.Value;
        }

        public static bool operator >(Angle left, Angle right)
        {
            return left.Value > right.Value;
        }

        public static bool operator <=(Angle left, Angle right)
        {
            return left.Value <= right.Value;
        }

        public static bool operator >=(Angle left, Angle right)
        {
            return left.Value >= right.Value;
        }

        public static Angle operator *(double left, Angle right)
        {
            return new Angle(left * right.Value);
        }

        [Obsolete("Not sure this is nice")]
        public static Angle operator *(Angle left, double right)
        {
            return new Angle(left.Value * right);
        }

        public static Angle operator /(Angle left, double right)
        {
            return new Angle(left.Value / right);
        }

        public static Angle operator +(Angle left, Angle right)
        {
            return new Angle(left.Value + right.Value);
        }

        public static Angle operator -(Angle left, Angle right)
        {
            return new Angle(left.Value - right.Value);
        }

        public static implicit operator double(Angle a)
        {
            return a.Value;
        }

        public override string ToString()
        {
            return ToString(null, (IFormatProvider)NumberFormatInfo.CurrentInfo);
        }
        public string ToString<T>(T unit) where T : IAngleUnit
        {
            return ToString(null, NumberFormatInfo.CurrentInfo, unit);
        }
        public string ToString(string format)
        {
            return ToString(format, (IFormatProvider)NumberFormatInfo.CurrentInfo);
        }
        public string ToString<T>(string format, T unit) where T : IAngleUnit
        {
            return ToString(format, NumberFormatInfo.CurrentInfo, unit);
        }

        public string ToString(IFormatProvider provider)
        {
            return ToString((string)null, (IFormatProvider)NumberFormatInfo.GetInstance(provider));
        }

        public string ToString<T>(IFormatProvider provider, T unit) where T : IAngleUnit
        {
            return ToString(null, NumberFormatInfo.GetInstance(provider), unit);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return string.Format("{0}{1}", Value.ToString(format, formatProvider), AngleUnit.Radians.ShortName);
        }

        public string ToString<T>(string format, IFormatProvider formatProvider, T unit) where T : IAngleUnit
        {
            return string.Format("{0}{1}", Value.ToString(format, formatProvider), unit.ShortName);
        }

        public int CompareTo(Angle other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(Angle other)
        {
            return Value.Equals(other.Value);
        }

        public bool Equals(Angle other, double tolerance)
        {
            return Math.Abs(Value - other.Value) < tolerance;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Angle && Equals((Angle)obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
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
            XmlExt.SetReadonlyField(ref this, x => x.Value, XmlConvert.ToDouble(e.ReadAttributeOrElementOrDefault("Value")));
        }

        /// <summary>
        /// Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized. </param>
        public void WriteXml(XmlWriter writer)
        {
            if (SerializeAsElements)
            {
                writer.WriteElementString("Value", Value.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                writer.WriteAttribute("Value", Value);
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