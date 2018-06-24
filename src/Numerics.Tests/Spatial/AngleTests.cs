using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MathNet.Numerics.Spatial;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial
{
    [TestFixture]
    public class AngleTests
    {
        private const double Tolerance = 1e-6;
        private const double DegToRad = Math.PI / 180;

        [Test]
        public void OperatorCompare()
        {
            var one = Angle.FromRadians(1);
            var two = Angle.FromRadians(2);
            Assert.AreEqual(true, one < two);
            Assert.AreEqual(true, one <= two);
            Assert.AreEqual(true, one <= Angle.FromRadians(1));
            Assert.AreEqual(false, one < Angle.FromRadians(1));
            Assert.AreEqual(false, one > Angle.FromRadians(1));
            Assert.AreEqual(true, one >= Angle.FromRadians(1));
        }

        [TestCase("1.5707 rad", "1.5707 rad", 1.5707 + 1.5707)]
        [TestCase("1.5707 rad", "2 °", 1.5707 + (2 * DegToRad))]
        public void OperatorAdd(string lvs, string rvs, double ev)
        {
            var lv = Angle.Parse(lvs);
            var rv = Angle.Parse(rvs);
            var sum = lv + rv;
            Assert.AreEqual(ev, sum.Radians, Tolerance);
            Assert.IsInstanceOf<Angle>(sum);
        }

        [TestCase("1.5707 rad", "1.5706 rad", 1.5707 - 1.5706)]
        [TestCase("1.5707 rad", "2 °", 1.5707 - (2 * DegToRad))]
        public void OperatorSubtract(string lvs, string rvs, double ev)
        {
            var lv = Angle.Parse(lvs);
            var rv = Angle.Parse(rvs);
            var diff = lv - rv;
            Assert.AreEqual(ev, diff.Radians, Tolerance);
            Assert.IsInstanceOf<Angle>(diff);
        }

        [TestCase("15 °", 5, 15 * 5 * DegToRad)]
        [TestCase("-10 °", 0, 0)]
        [TestCase("-10 °", 2, -10 * 2 * DegToRad)]
        [TestCase("1 rad", 2, 2)]
        public void OperatorMultiply(string lvs, double rv, double ev)
        {
            var lv = Angle.Parse(lvs);
            var prods = new[] { lv * rv, rv * lv };
            foreach (var prod in prods)
            {
                Assert.AreEqual(ev, prod.Radians, 1e-3);
                Assert.IsInstanceOf<Angle>(prod);
            }
        }

        [Test]
        public void OperatorNegate()
        {
            Assert.AreEqual(-1, (-Angle.FromRadians(1)).Radians);
        }

        [TestCase("3.141596 rad", 2, 1.570797999)]
        public void DivisionTest(string s, double rv, double expected)
        {
            var angle = Angle.Parse(s);
            var actual = angle / rv;
            Assert.AreEqual(expected, actual.Radians, Tolerance);
            Assert.IsInstanceOf<Angle>(actual);
        }

        [TestCase("90 °", 90, Math.PI / 2, true)]
        [TestCase("1 rad", 1 * 180 / Math.PI, 1, true)]
        [TestCase("1.1 rad", 1 * 180 / Math.PI, Math.PI / 2, false)]
        public void Equals(string s, double degrees, double radians, bool expected)
        {
            var a = Angle.Parse(s);
            var deg = Angle.FromDegrees(degrees);
            Assert.AreEqual(expected, deg.Equals(a));
            Assert.AreEqual(expected, deg.Equals(a, Tolerance));
            Assert.AreEqual(expected, deg == a);
            Assert.AreEqual(!expected, deg != a);

            var rad = Angle.FromRadians(radians);
            Assert.AreEqual(expected, rad.Equals(a));
            Assert.AreEqual(expected, rad.Equals(a, Tolerance));
            Assert.AreEqual(expected, rad == a);
            Assert.AreEqual(!expected, rad != a);
        }

        [Test]
        public void EqualsWithTolerance()
        {
            var one = Angle.FromRadians(1);
            var two = Angle.FromRadians(2);
            Assert.AreEqual(true, one.Equals(two, 2));
            Assert.AreEqual(false, one.Equals(two, 0.1));
            Assert.AreEqual(true, one.Equals(two, Angle.FromRadians(2)));
            Assert.AreEqual(false, one.Equals(two, Angle.FromRadians(0.1)));
        }

        [TestCase(90, 1.5707963267948966)]
        public void FromDegrees(double degrees, double expected)
        {
            Assert.AreEqual(expected, Angle.FromDegrees(degrees).Radians);
            Assert.AreEqual(degrees, Angle.FromDegrees(degrees).Degrees, 1E-6);
        }

        [TestCase(1, 1)]
        public void FromRadians(double radians, double expected)
        {
            Assert.AreEqual(expected, Angle.FromRadians(radians).Radians);
        }

        [TestCase(20, 33, 49, 0.35890271998857842)]
        public void FromSexagesimal(int degrees, int minutes, double seconds, double expected)
        {
            Assert.AreEqual(expected, Angle.FromSexagesimal(degrees, minutes, seconds).Radians, 1E-6);
        }

        [TestCase("5 °", 5 * DegToRad)]
        [TestCase("5°", 5 * DegToRad)]
        [TestCase("-5.34 rad", -5.34)]
        [TestCase("-5,34 rad", -5.34)]
        [TestCase("1e-4 rad", 0.0001)]
        [TestCase("1e-4 °", 0.0001 * DegToRad)]
        public void Parse(string s, double expected)
        {
            Assert.AreEqual(true, Angle.TryParse(s, out var angle));
            Assert.AreEqual(expected, angle.Radians, Tolerance);
            angle = Angle.Parse(s);
            Assert.AreEqual(expected, angle.Radians, Tolerance);
            Assert.IsInstanceOf<Angle>(angle);

            // ReSharper disable once RedundantToStringCall
            angle = Angle.Parse(s.ToString());
            Assert.AreEqual(expected, angle.Radians, Tolerance);
            Assert.IsInstanceOf<Angle>(angle);
        }

        [Test]
        public void FailParse()
        {
            bool result = Angle.TryParse("test", out var angle);
            Assert.AreEqual(default(Angle), angle);
            Assert.IsFalse(result);
        }

        [Test]
        public void FailParseDirect()
        {
            Assert.Throws<FormatException>(() => Angle.Parse("Test"), "Expected FormatException", null);
        }

        [TestCase(".1 rad", 0.1)]
        [TestCase("1.2 rad", 1.2)]
        [TestCase("1.2\u00A0rad", 1.2)]
        [TestCase("1.2radians", 1.2)]
        [TestCase("1.2 radians", 1.2)]
        [TestCase("1.2\u00A0radians", 1.2)]
        [TestCase("1.2\u00A0Radians", 1.2)]
        public void ParseRadians(string text, double expected)
        {
            Assert.AreEqual(true, Angle.TryParse(text, out var angle));
            Assert.AreEqual(expected, angle.Radians);
            Assert.AreEqual(expected, Angle.Parse(text).Radians);
        }

        [TestCase("1°", 1)]
        [TestCase("1 °", 1)]
        [TestCase("1deg", 1)]
        [TestCase("1 deg", 1)]
        [TestCase("1\u00A0deg", 1)]
        [TestCase("1\u00A0DEG", 1)]
        [TestCase("1degrees", 1)]
        [TestCase("1 degrees", 1)]
        [TestCase("1\u00A0degrees", 1)]
        [TestCase("1\u00A0Degrees", 1)]
        public void ParseDegrees(string text, double expected)
        {
            Assert.AreEqual(true, Angle.TryParse(text, out var angle));
            Assert.AreEqual(expected, angle.Degrees);
            Assert.AreEqual(expected, Angle.Parse(text).Degrees);
        }

        [TestCase(@"<Angle Value=""1"" />")]
        [TestCase(@"<Angle><Value>1</Value></Angle>")]
        public void ReadFrom(string xml)
        {
            var v = Angle.FromRadians(1);
            Assert.AreEqual(v, Angle.ReadFrom(XmlReader.Create(new StringReader(xml))));
        }

        [Test]
        public void Compare()
        {
            var small = Angle.FromDegrees(1);
            var big = Angle.FromDegrees(2);
            Assert.IsTrue(small < big);
            Assert.IsTrue(small <= big);
            Assert.IsFalse(small > big);
            Assert.IsFalse(small >= big);
            Assert.AreEqual(-1, small.CompareTo(big));
            Assert.AreEqual(0, small.CompareTo(small));
            Assert.AreEqual(1, big.CompareTo(small));
        }

        [TestCase("15 °", "0.261799387799149\u00A0rad")]
        public void ToString(string s, string expected)
        {
            var angle = Angle.Parse(s);
            var toString = angle.ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(expected, toString);
            Assert.IsTrue(angle.Equals(Angle.Parse(toString), Tolerance));
            Assert.IsTrue(angle.Equals(Angle.Parse(toString), Angle.FromRadians(Tolerance)));
        }

        //[TestCase("15°", "F2", "15.00°")]
        //public void ToString(string s, string format, string expected)
        //{
        //    var angle = Angle.Parse(s);
        //    var toString = angle.ToString(format, CultureInfo.InvariantCulture, AngleUnit.Degrees);
        //    Assert.AreEqual(expected, toString);
        //    Assert.AreEqual(angle.Radians, Angle.Parse(angle.ToString(format)).Radians, 1E-2);
        //    Assert.IsTrue(angle.Equals(Angle.Parse(toString), Tolerance));
        //}

        [TestCase("15°", @"<Angle Value=""0.26179938779914941"" />")]
        public void XmlRoundTrips(string vs, string xml)
        {
            var angle = Angle.Parse(vs);
            AssertXml.XmlRoundTrips(angle, xml, (e, a) =>
            {
                Assert.AreEqual(e.Radians, a.Radians, Tolerance);
            });
        }

        [Test]
        public void XmlContainerRoundtrip()
        {
            var container = new AssertXml.Container<Angle>
            {
                Value1 = Angle.FromRadians(1),
                Value2 = Angle.FromRadians(2),
            };
            var expected = "<ContainerOfAngle>\r\n" +
                           "  <Value1 Value=\"1\"></Value1>\r\n" +
                           "  <Value2 Value=\"2\"></Value2>\r\n" +
                           "</ContainerOfAngle>";
            var roundTrip = AssertXml.XmlSerializerRoundTrip(container, expected);
            Assert.AreEqual(container.Value1, roundTrip.Value1);
            Assert.AreEqual(container.Value2, roundTrip.Value2);
        }

        [Test]
        public void ReadXmlContainerElementValues()
        {
            var container = new AssertXml.Container<Angle>
            {
                Value1 = Angle.FromRadians(1),
                Value2 = Angle.FromRadians(2),
            };
            var xml = "<ContainerOfAngle>\r\n" +
                           "  <Value1>1</Value1>\r\n" +
                           "  <Value2>2</Value2>\r\n" +
                           "</ContainerOfAngle>";
            var serializer = new XmlSerializer(typeof(AssertXml.Container<Angle>));
            using (var reader = new StringReader(xml))
            {
                var deserialized = (AssertXml.Container<Angle>)serializer.Deserialize(reader);
                Assert.AreEqual(container.Value1, deserialized.Value1);
                Assert.AreEqual(container.Value2, deserialized.Value2);
            }
        }

        [TestCase("15°", @"<Angle><Value>0.261799387799149</Value></Angle>")]
        [TestCase("15°", @"<Angle><Radians>0.261799387799149</Radians></Angle>")]
        [TestCase("15°", @"<Angle><Degrees>15</Degrees></Angle>")]
        [TestCase("15°", @"<Angle Radians=""0.26179938779914941"" />")]
        [TestCase("180°", @"<Angle Degrees=""180"" />")]
        public void XmlElement(string vs, string xml)
        {
            var angle = Angle.Parse(vs);
            var serializer = new XmlSerializer(typeof(Angle));
            using (var reader = new StringReader(xml))
            {
                var fromElements = (Angle)serializer.Deserialize(reader);
                Assert.AreEqual(angle.Radians, fromElements.Radians, 1e-6);
            }
        }

        [Test]
        public void ToStringTest()
        {
            int number = 1;
            var angle = Angle.FromRadians(number);
            string expected = number + " rad";
            Assert.AreEqual(expected, angle.ToString());
        }

        [Test]
        public void ObjectEqualsTest()
        {
            var angle = Angle.FromRadians(1);
            Assert.IsTrue(angle.Equals((object)angle));
        }

        [Test]
        public void ObjectNullTest()
        {
            var angle = Angle.FromRadians(1);
            Assert.IsFalse(angle.Equals(null));
        }

        [Test]
        public void HashCodeTest()
        {
            string test = "test";
            var angle = Angle.FromRadians(1);
            var lookup = new System.Collections.Generic.Dictionary<Angle, string>
            {
                { angle, test }
            };

            Assert.AreEqual(test, lookup[angle]);
        }
    }
}
