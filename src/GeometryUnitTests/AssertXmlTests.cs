namespace MathNet.GeometryUnitTests
{
    using NUnit.Framework;

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
        [Test]
        public void DataContractRoundTripTest()
        {
            var dummy = new XmlSerializableDummy("Meh", 14);
            var roundTrip = AssertXml.DataContractRoundTrip(dummy, @"<XmlSerializableDummy Age=""14""><Name>Meh</Name></XmlSerializableDummy>");
            Assert.AreEqual(dummy.Name, roundTrip.Name);
            Assert.AreEqual(dummy.Age, roundTrip.Age);
        }
    }
}