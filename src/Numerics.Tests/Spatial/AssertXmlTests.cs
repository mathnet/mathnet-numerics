using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial
{

#if !NETCOREAPP1_1

    public class AssertXmlTests
    {
        [Test]
        public void XmlSerializerRoundTripTest()
        {
            var dummy = new XmlSerializableDummy("Meh", 14);
            var roundTrip = AssertXml.XmlSerializerRoundTrip(dummy, @"<XmlSerializableDummy Age=""14""><Name>Meh</Name></XmlSerializableDummy>");
            Assert.AreEqual(dummy.Name, roundTrip.Name);
            Assert.AreEqual(dummy.Age, roundTrip.Age);
        }

        public class XmlSerializableDummy : IXmlSerializable
        {
            private readonly string name;

            public XmlSerializableDummy(string name, int age)
            {
                this.Age = age;
                this.name = name;
            }

            // ReSharper disable once UnusedMember.Local
            private XmlSerializableDummy()
            {
            }

            public string Name => this.name;

            public int Age { get; set; }

            public XmlSchema GetSchema() => null;

            public void ReadXml(XmlReader reader)
            {
                var e = (XElement)XNode.ReadFrom(reader);
                this.Age = XmlConvert.ToInt32(e.Attribute("Age").Value);
                var name = ReadAttributeOrElement(e, "Name");
                WriteValueToReadonlyField(this, name, () => this.name);
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteAttributeString("Age", this.Age.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Name", this.Name);
            }

            private static string ReadAttributeOrElement(XElement e, string localName)
            {
                XAttribute xattribute = e.Attributes()
                    .SingleOrDefault(x => x.Name.LocalName == localName);
                if (xattribute != null)
                {
                    return xattribute.Value;
                }

                XElement xelement = e.Elements()
                    .SingleOrDefault(x => x.Name.LocalName == localName);
                if (xelement != null)
                {
                    return xelement.Value;
                }

                throw new XmlException($"Attribute or element {localName} not found");
            }

            private static void WriteValueToReadonlyField<TClass, TProperty>(
                TClass item,
                TProperty value,
                Expression<Func<TProperty>> fieldExpression)
            {
                string name = ((MemberExpression)fieldExpression.Body).Member.Name;
                GetAllFields(item.GetType())
                    .Single(x => x.Name == name)
                    .SetValue(item, value);
            }

            private static IEnumerable<FieldInfo> GetAllFields(Type t)
            {
                if (t == null)
                {
                    return Enumerable.Empty<FieldInfo>();
                }

                BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static |
                                           BindingFlags.Public | BindingFlags.NonPublic;
                return t.GetFields(bindingAttr)
                    .Concat(GetAllFields(t.BaseType));
            }
        }
    }

#endif

}
