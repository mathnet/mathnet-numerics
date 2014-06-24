namespace MathNet.GeometryUnitTests
{
    using System.Globalization;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using Geometry;

    public class XmlSerializableDummy : IXmlSerializable
    {
        private readonly string _name;
        private XmlSerializableDummy()
        {

        }
        public XmlSerializableDummy(string name, int age)
        {
            Age = age;
            _name = name;
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public int Age { get; set; }

        public XmlSchema GetSchema() { return null; }
        public void ReadXml(XmlReader reader)
        {
            var e = (XElement)XNode.ReadFrom(reader);
            Age = XmlConvert.ToInt32(e.Attribute("Age").Value);
            var name = e.ReadAttributeOrElement("Name");
            XmlExt.WriteValueToReadonlyField(this, name, () => _name);
        }
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Age", Age.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Name", Name);
        }
    }
}