using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace MathNet.Numerics.Spatial.Internal
{
    /// <summary>
    /// An extension class for XmlWriter
    /// </summary>
    internal static class XmlWriterExt
    {
        /// <summary>
        /// Writes an element
        /// </summary>
        /// <param name="writer">An Xml Writer</param>
        /// <param name="name">The element name</param>
        /// <param name="value">The value</param>
        internal static void WriteElement(this XmlWriter writer, string name, IXmlSerializable value)
        {
            writer.WriteStartElement(name);
            value.WriteXml(writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes an element
        /// </summary>
        /// <param name="writer">An Xml Writer</param>
        /// <param name="name">The element name</param>
        /// <param name="value">The value</param>
        internal static void WriteElement(this XmlWriter writer, string name, double value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes an element
        /// </summary>
        /// <param name="writer">An Xml Writer</param>
        /// <param name="name">The element name</param>
        /// <param name="value">The value</param>
        /// <param name="format">a format to apply to the value</param>
        internal static void WriteElement(this XmlWriter writer, string name, double value, string format)
        {
            writer.WriteElementString(name, value.ToString(format, CultureInfo.InvariantCulture));
        }

        internal static XmlWriter WriteAttribute<T>(this XmlWriter writer, string name, T value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(value);
            writer.WriteEndAttribute();
            return writer;
        }
    }
}
