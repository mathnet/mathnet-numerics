// ReSharper disable InconsistentNaming

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MathNet.Numerics.Spatial.Euclidean3D;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial.Euclidean3D
{
    [TestFixture]
    public class UnitVector3DTests
    {
        [Test]
        public void Create()
        {
            var actual = UnitVector3D.Create(1, -2, 3);
            Assert.AreEqual(0.2672612419124244, actual.X);
            Assert.AreEqual(-0.53452248382484879, actual.Y);
            Assert.AreEqual(0.80178372573727319, actual.Z);

            actual = UnitVector3D.Create(0.2672612419124244, -0.53452248382484879, 0.80178372573727319);
            Assert.AreEqual(0.2672612419124244, actual.X);
            Assert.AreEqual(-0.53452248382484879, actual.Y);
            Assert.AreEqual(0.80178372573727319, actual.Z);

            Assert.Throws<ArgumentOutOfRangeException>(() => UnitVector3D.Create(double.NaN, 2, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => UnitVector3D.Create(double.PositiveInfinity, 2, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => UnitVector3D.Create(double.NegativeInfinity, 2, 3));
        }

        [TestCase("1,0; 0; 0,0", 1, 0, 0)]
        [TestCase("0; 1,0; 0,0", 0, 1, 0)]
        [TestCase("0; 0,0; 1,0", 0, 0, 1)]
        [TestCase("1.0; 0; 0.0", 1, 0, 0)]
        [TestCase("0; 1.0; 0.0", 0, 1, 0)]
        [TestCase("0; 0.0; 1.0", 0, 0, 1)]
        public void Parse(string text, double expectedX, double expectedY, double expectedZ)
        {
            Assert.AreEqual(true, UnitVector3D.TryParse(text, out var p));
            Assert.AreEqual(expectedX, p.X);
            Assert.AreEqual(expectedY, p.Y);
            Assert.AreEqual(expectedZ, p.Z);

            p = UnitVector3D.Parse(text);
            Assert.AreEqual(expectedX, p.X);
            Assert.AreEqual(expectedY, p.Y);
            Assert.AreEqual(expectedZ, p.Z);

            p = UnitVector3D.Parse(p.ToString());
            Assert.AreEqual(expectedX, p.X);
            Assert.AreEqual(expectedY, p.Y);
            Assert.AreEqual(expectedZ, p.Z);
        }

        [TestCase("1; 2; 3")]
        [TestCase("1.2")]
        [TestCase("1,2; 2.3; 3")]
        [TestCase("1; 2; 3; 4")]
        public void ParseFails(string text)
        {
            Assert.AreEqual(false, UnitVector3D.TryParse(text, out _));
            Assert.Throws<FormatException>(() => UnitVector3D.Parse(text));
        }

        [Test]
        public void ToDenseVector()
        {
            var uv = UnitVector3D.Create(0.2672612419124244, -0.53452248382484879, 0.80178372573727319);
            var vector = uv.ToVector();
            Assert.AreEqual(3, vector.Count);
            Assert.AreEqual(0.2672612419124244, vector[0]);
            Assert.AreEqual(-0.53452248382484879, vector[1]);
            Assert.AreEqual(0.80178372573727319, vector[2]);

            var roundtripped = UnitVector3D.OfVector(vector);
            Assert.AreEqual(0.2672612419124244, roundtripped.X);
            Assert.AreEqual(-0.53452248382484879, roundtripped.Y);
            Assert.AreEqual(0.80178372573727319, roundtripped.Z);
        }

        [TestCase("1, 0, 0", "1, 0, 0", 1e-4, true)]
        [TestCase("0, 1, 0", "0, 1, 0", 1e-4, true)]
        [TestCase("0, 0, 1", "0, 0, 1", 1e-4, true)]
        [TestCase("1, 0, 0", "0, 1, 0", 1e-4, false)]
        [TestCase("0, 1, 0", "1, 0, 0", 1e-4, false)]
        [TestCase("0, 0, 1", "0, 1, 0", 1e-4, false)]
        public void Equals(string p1s, string p2s, double tol, bool expected)
        {
            var v1 = UnitVector3D.Parse(p1s);
            var v2 = UnitVector3D.Parse(p2s);
            var vector3D = v1.ToVector3D();
            Assert.AreEqual(expected, v1 == v2);
            Assert.IsTrue(v1 == vector3D);
            Assert.IsTrue(vector3D == v1);

            Assert.AreEqual(expected, v1.Equals(v2));
            Assert.IsTrue(v1.Equals(vector3D));
            Assert.IsTrue(vector3D.Equals(v1));
            Assert.AreEqual(expected, v1.Equals(v2.ToVector3D()));
            Assert.AreEqual(expected, v2.ToVector3D().Equals(v1));

            Assert.AreEqual(expected, v1.Equals((object)v2));
            Assert.AreEqual(expected, Equals(v1, v2));

            Assert.AreEqual(expected, v1.Equals(v2, tol));
            Assert.AreNotEqual(expected, v1 != v2);
            Assert.AreNotEqual(expected, v1 != v2.ToVector3D());
            Assert.AreNotEqual(expected, v2.ToVector3D() != v1);
        }

        [TestCase("1; 0; 0", 5, "5; 0; 0")]
        [TestCase("1; 0; 0", -5, "-5; 0; 0")]
        [TestCase("-1; 0; 0", 5, "-5; 0; 0")]
        [TestCase("-1; 0; 0", -5, "5; 0; 0")]
        [TestCase("0; 1; 0", 5, "0; 5; 0")]
        [TestCase("0; 0; 1", 5, "0; 0; 5")]
        public void Scale(string ivs, double s, string exs)
        {
            var uv = UnitVector3D.Parse(ivs);
            var v = uv.ScaleBy(s);
            AssertGeometry.AreEqual(Vector3D.Parse(exs), v, float.Epsilon);
        }

        [TestCase("1; 0; 0", "1; 0; 0", 1)]
        [TestCase("1; 0; 0", "-1; 0; 0", -1)]
        [TestCase("1; 0; 0", "0; -1; 0", 0)]
        public void DotProduct(string v1s, string v2s, double expected)
        {
            var uv1 = UnitVector3D.Parse(v1s);
            var uv2 = UnitVector3D.Parse(v2s);
            var dp = uv1.DotProduct(uv2);
            Assert.AreEqual(dp, expected, 1e-9);
            Assert.IsTrue(dp <= 1);
            Assert.IsTrue(dp >= -1);
        }

        [TestCase("-1, 0, 0", null, "(-1, 0, 0)", 1e-4)]
        [TestCase("-1, 0, 1e-4", "F2", "(-1.00, 0.00, 0.00)", 1e-3)]
        public void ToString(string vs, string format, string expected, double tolerance)
        {
            var v = UnitVector3D.Parse(vs);
            var actual = v.ToString(format);
            Assert.AreEqual(expected, actual);
            AssertGeometry.AreEqual(v, UnitVector3D.Parse(actual), tolerance);
        }

        [TestCase("1,0,0", 3, "3,0,0")]
        public void MultiplyTest(string unitVectorAsString, double multiplier, string expected)
        {
            var unitVector3D = UnitVector3D.Parse(unitVectorAsString);
            Assert.AreEqual(Vector3D.Parse(expected), multiplier * unitVector3D);
        }

        [TestCase("<UnitVector3D X=\"0.2672612419124244\" Y=\"-0.53452248382484879\" Z=\"0.80178372573727319\" />")]
        [TestCase("<UnitVector3D Y=\"-0.53452248382484879\" Z=\"0.80178372573727319\"  X=\"0.2672612419124244\"/>")]
        [TestCase("<UnitVector3D Z=\"0.80178372573727319\" X=\"0.2672612419124244\" Y=\"-0.53452248382484879\" />")]
        [TestCase("<UnitVector3D><X>0.2672612419124244</X><Y>-0.53452248382484879</Y><Z>0.80178372573727319</Z></UnitVector3D>")]
        [TestCase("<UnitVector3D><Y>-0.53452248382484879</Y><Z>0.80178372573727319</Z><X>0.2672612419124244</X></UnitVector3D>")]
        [TestCase("<UnitVector3D><Z>0.80178372573727319</Z><X>0.2672612419124244</X><Y>-0.53452248382484879</Y></UnitVector3D>")]
        public void ReadFrom(string xml)
        {
            using (var reader = new StringReader(xml))
            {
                var actual = UnitVector3D.ReadFrom(XmlReader.Create(reader));
                Assert.AreEqual(UnitVector3D.Create(0.2672612419124244, -0.53452248382484879, 0.80178372573727319), actual);
            }
        }

        [Test]
        public void XmlRoundtrip()
        {
            var uv = UnitVector3D.Create(0.2672612419124244, -0.53452248382484879, 0.80178372573727319);
            var xml = "<UnitVector3D X=\"0.2672612419124244\" Y=\"-0.53452248382484879\" Z=\"0.80178372573727319\" />";
            AssertXml.XmlRoundTrips(uv, xml, (expected, actual) => AssertGeometry.AreEqual(expected, actual));
        }

        [Test]
        public void XmlContainerRoundtrip()
        {
            var container = new AssertXml.Container<UnitVector3D>
            {
                Value1 = UnitVector3D.Create(0.2672612419124244, -0.53452248382484879, 0.80178372573727319),
                Value2 = UnitVector3D.Create(1, 0, 0)
            };
            var expected = "<ContainerOfUnitVector3D>\r\n" +
                           "  <Value1 X=\"0.2672612419124244\" Y=\"-0.53452248382484879\" Z=\"0.80178372573727319\"></Value1>\r\n" +
                           "  <Value2 X=\"1\" Y=\"0\" Z=\"0\"></Value2>\r\n" +
                           "</ContainerOfUnitVector3D>";
            var roundTrip = AssertXml.XmlSerializerRoundTrip(container, expected);
            AssertGeometry.AreEqual(container.Value1, roundTrip.Value1);
            AssertGeometry.AreEqual(container.Value2, roundTrip.Value2);
        }

        [Test]
        public void XmlElements()
        {
            var v = UnitVector3D.Create(0.2672612419124244, -0.53452248382484879, 0.80178372573727319);
            var serializer = new XmlSerializer(typeof(UnitVector3D));
            using (var reader = new StringReader("<UnitVector3D><X>0.2672612419124244</X><Y>-0.53452248382484879</Y><Z>0.80178372573727319</Z></UnitVector3D>"))
            {
                AssertGeometry.AreEqual(v, (UnitVector3D)serializer.Deserialize(reader));
            }
        }

        [Test]
        public void XmlContainerElements()
        {
            var xml = "<ContainerOfUnitVector3D>\r\n" +
                      "  <Value1><X>0.2672612419124244</X><Y>-0.53452248382484879</Y><Z>0.80178372573727319</Z></Value1>\r\n" +
                      "  <Value2><X>1</X><Y>0</Y><Z>0</Z></Value2>\r\n" +
                      "</ContainerOfUnitVector3D>";
            var serializer = new XmlSerializer(typeof(AssertXml.Container<UnitVector3D>));
            var deserialized = (AssertXml.Container<UnitVector3D>)serializer.Deserialize(new StringReader(xml));
            AssertGeometry.AreEqual(UnitVector3D.Create(0.2672612419124244, -0.53452248382484879, 0.80178372573727319), deserialized.Value1);
            AssertGeometry.AreEqual(UnitVector3D.Create(1, 0, 0), deserialized.Value2);
        }
    }
}
