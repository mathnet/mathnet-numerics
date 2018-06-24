using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace MathNet.Numerics.Spatial.Internal
{
    /// <summary>
    /// An extension class for XmlReader
    /// </summary>
    internal static class XmlReaderExt
    {
        /// <summary>
        /// Creates a default(T) and calls ReadXml(reader) on it.
        /// </summary>
        /// <typeparam name="T">The type of the instance to read from the current position of the reader.</typeparam>
        /// <param name="reader">A <see cref="XmlReader"/></param>
        /// <returns> A new instance of {T} with values from <paramref name="reader"/></returns>
        internal static T ReadElementAs<T>(this XmlReader reader)
            where T : struct, IXmlSerializable
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var instance = default(T);
            instance.ReadXml(reader);
            return instance;
        }

        /// <summary>
        /// Reads the attribute named <paramref name="localName"/> if it exists on the current element.
        /// This is not a proper try method as it checks if <paramref name="reader"/> is null and throws.
        /// </summary>
        /// <param name="reader">A <see cref="XmlReader"/></param>
        /// <param name="localName">The name of the attribute to read the value of.</param>
        /// <param name="value">The value read from <paramref name="reader"/></param>
        /// <returns>True if the attribute was found.</returns>
        internal static bool TryReadAttributeAsDouble(this XmlReader reader, string localName, out double value)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.MoveToContent() == XmlNodeType.Element &&
                reader.HasAttributes &&
                reader.MoveToAttribute(localName))
            {
                value = XmlConvert.ToDouble(reader.Value);
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// Reads the values of the elements named <paramref name="localName"/> and <paramref name="localName"/> if they exist on the current element.
        /// This is not a proper try method as it checks if <paramref name="reader"/> is null and throws.
        /// </summary>
        /// <remarks>
        /// Calling this method has side effects as it changes the position of the reader.
        /// </remarks>
        /// <param name="reader">A <see cref="XmlReader"/></param>
        /// <param name="localName">The local name of the element to read value from.</param>
        /// <param name="value">The value read from <paramref name="reader"/></param>
        /// <returns>True if both elements were found.</returns>
        internal static bool TryReadElementContentAsDouble(this XmlReader reader, string localName, out double value)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.MoveToContent() == XmlNodeType.Element &&
                !reader.IsEmptyElement &&
                reader.LocalName == localName)
            {
                value = reader.ReadElementContentAsDouble();
                return true;
            }

            value = 0;
            return false;
        }

        /// <summary>
        /// Reads the values of the elements named <paramref name="xName"/> and <paramref name="xName"/> if they exist on the current element.
        /// This is not a proper try method as it checks if <paramref name="reader"/> is null and throws.
        /// </summary>
        /// <remarks>
        /// Calling this method has side effects as it changes the position of the reader.
        /// </remarks>
        /// <param name="reader">A <see cref="XmlReader"/></param>
        /// <param name="xName">The local name of the x element.</param>
        /// <param name="yName">The local name of the y element.</param>
        /// <param name="x">The x value read from <paramref name="reader"/></param>
        /// <param name="y">The y value read from <paramref name="reader"/></param>
        /// <returns>True if both elements were found.</returns>
        internal static bool TryReadChildElementsAsDoubles(this XmlReader reader, string xName, string yName, out double x, out double y)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            x = 0;
            y = 0;
            if (reader.MoveToContent() == XmlNodeType.Element &&
                !reader.IsEmptyElement &&
                !reader.HasValue)
            {
                var subtree = reader.ReadSubtree();
                if (subtree.ReadToFirstDescendant())
                {
                    if (subtree.TryReadSiblingElementsAsDoubles(xName, yName, out x, out y))
                    {
                        subtree.Skip();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Reads the values of the elements named <paramref name="xName"/> and <paramref name="xName"/> if they exist on the current element.
        /// This is not a proper try method as it checks if <paramref name="reader"/> is null and throws.
        /// </summary>
        /// <remarks>
        /// Calling this method has side effects as it changes the position of the reader.
        /// </remarks>
        /// <param name="reader">A <see cref="XmlReader"/></param>
        /// <param name="xName">The local name of the x element.</param>
        /// <param name="yName">The local name of the y element.</param>
        /// <param name="zName">The local name of the z element.</param>
        /// <param name="x">The x value read from <paramref name="reader"/></param>
        /// <param name="y">The y value read from <paramref name="reader"/></param>
        /// <param name="z">The z value read from <paramref name="reader"/></param>
        /// <returns>True if both elements were found.</returns>
        internal static bool TryReadChildElementsAsDoubles(this XmlReader reader, string xName, string yName, string zName, out double x, out double y, out double z)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            x = 0;
            y = 0;
            z = 0;
            if (reader.MoveToContent() == XmlNodeType.Element &&
                !reader.IsEmptyElement &&
                !reader.HasValue)
            {
                var subtree = reader.ReadSubtree();
                if (subtree.ReadToFirstDescendant())
                {
                    if (subtree.TryReadElementContentAsDouble(xName, out x))
                    {
                        if (subtree.TryReadSiblingElementsAsDoubles(yName, zName, out y, out z))
                        {
                            subtree.Skip();
                            return true;
                        }

                        return false;
                    }

                    if (subtree.TryReadElementContentAsDouble(yName, out y))
                    {
                        if (subtree.TryReadSiblingElementsAsDoubles(xName, zName, out x, out z))
                        {
                            subtree.Skip();
                            return true;
                        }

                        return false;
                    }

                    if (subtree.TryReadElementContentAsDouble(zName, out z))
                    {
                        if (subtree.TryReadSiblingElementsAsDoubles(xName, yName, out x, out y))
                        {
                            subtree.Skip();
                            return true;
                        }

                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Reads until first descendant
        /// </summary>
        /// <param name="reader">An xml reader</param>
        /// <returns>True if successful; otherwise false</returns>
        internal static bool ReadToFirstDescendant(this XmlReader reader)
        {
            var depth = reader.Depth;
            if (reader.MoveToContent() != XmlNodeType.Element)
            {
                return reader.Depth > depth &&
                       reader.NodeType == XmlNodeType.Element;
            }

            while (reader.Read() &&
                   reader.Depth <= depth)
            {
            }

            return reader.Depth > depth &&
                   reader.NodeType == XmlNodeType.Element;
        }

        /// <summary>
        /// Attempts to read sibling elements as a pair of doubles
        /// </summary>
        /// <param name="subtree">an xml reader</param>
        /// <param name="xName">The name of the x element</param>
        /// <param name="yName">The name of the y element</param>
        /// <param name="x">The x value</param>
        /// <param name="y">The y value</param>
        /// <returns>True if successful; otherwise false</returns>
        private static bool TryReadSiblingElementsAsDoubles(this XmlReader subtree, string xName, string yName, out double x, out double y)
        {
            if (subtree.TryReadElementContentAsDouble(xName, out x) &&
                subtree.TryReadElementContentAsDouble(yName, out y))
            {
                return true;
            }

            if (subtree.TryReadElementContentAsDouble(yName, out y) &&
                subtree.TryReadElementContentAsDouble(xName, out x))
            {
                return true;
            }

            return false;
        }

        public static XElement SingleElement(this XElement e, string localName)
        {
            return e.Elements()
                .Single(x => x.Name.LocalName == localName);
        }

        public static XmlReader SingleElementReader(this XElement e, string localName)
        {
            return e.SingleElement(localName)
                .CreateReader();
        }
    }
}
