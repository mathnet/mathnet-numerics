namespace MathNet.GeometryUnitTests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using NUnit.Framework;

    public static class AssertXml
    {
        public static void AreEqual(string first, string other)
        {
            var x1 = CleanupXml(first);
            var x2 = CleanupXml(other);
            Assert.AreEqual(x1, x2);
        }

        /// <summary>
        /// Serializes using XmlSerializer & DataContractSerializer
        /// Compares the generated xml
        /// Then asserts that the deserialized is the same as input (item)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="expectedXml"></param>
        /// <param name="assert"></param>
        public static void XmlRoundTrips<T>(T item, string expectedXml, Action<T, T> assert)
        {
            var roundtrips = new[]
            {
                XmlSerializerRoundTrip(item, expectedXml),
                DataContractRoundTrip(item, expectedXml)
            };
            foreach (var roundtrip in roundtrips)
            {
                assert(item, roundtrip);
            }
        }

        public static T XmlSerializerRoundTrip<T>(T item, string expected)
        {
            var serializer = new XmlSerializer(item.GetType());
            string xml;

            using (var sw = new StringWriter())
            using (var writer = XmlWriter.Create(sw, Settings))
            {
                serializer.Serialize(writer, item);
                xml = sw.ToString();
                Console.WriteLine("XmlSerializer");
                Console.Write(xml);
                Console.WriteLine();
                AreEqual(expected, xml);
            }
            using (var reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        public static T DataContractRoundTrip<T>(T item, string expected)
        {
            var serializer = new DataContractSerializer(item.GetType());
            string xml;
            using (var sw = new StringWriter())
            using (var writer = XmlWriter.Create(sw, Settings))
            {
                serializer.WriteObject(writer, item);
                writer.Flush();
                xml = sw.ToString();
                Console.WriteLine("DataContractSerializer");
                Console.Write(xml);
                Console.WriteLine();
                AreEqual(expected, xml);
            }
            using (var stringReader = new StringReader(xml))
            using (var reader = XmlReader.Create(stringReader))
            {
                return (T)serializer.ReadObject(reader);
            }
        }
        private static XmlWriterSettings Settings
        {
            get
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    NewLineHandling = NewLineHandling.Entitize,
                    OmitXmlDeclaration = true,
                    ////NamespaceHandling = NamespaceHandling.Default
                };
                return settings;
            }
        }
        private static string Normalize(string xml)
        {
            var e = XElement.Parse(xml);
            return Normalize(e);
        }
        private static string Normalize(XElement e)
        {
            using (var sw = new StringWriter())
            using (var writer = XmlWriter.Create(sw, Settings))
            {
                e.WriteTo(writer);
                writer.Flush();
                return sw.ToString();
            }
        }
        private static string CleanupXml(string xml)
        {
            var e = XElement.Parse(xml);
            XElement clean = RemoveAllNamespaces(e);
            return Normalize(clean);
        }

        /// <summary>
        /// Core recursion function
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static XElement RemoveAllNamespaces(XElement e)
        {
            var ne = new XElement(e.Name.LocalName, e.HasElements ? null : e.Value);
            ne.Add(e.Attributes().Where(a => !a.IsNamespaceDeclaration));
            ne.Add(e.Elements().Select(RemoveAllNamespaces));
            return ne;
        }
    }
}
