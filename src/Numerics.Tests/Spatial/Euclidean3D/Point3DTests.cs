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
    public class Point3DTests
    {
        [Test]
        public void Ctor()
        {
            var actual = new Point3D(1, 2, 3);
            Assert.AreEqual(1, actual.X, 1e-6);
            Assert.AreEqual(2, actual.Y, 1e-6);
            Assert.AreEqual(3, actual.Z, 1e-6);
        }

        [TestCase("-1,1,-1", -1, 1, -1)]
        [TestCase("1, 2, 3", 1, 2, 3)]
        [TestCase("1.2; 3.4; 5.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2;3.4;5.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2 ; 3.4 ; 5.6", 1.2, 3.4, 5.6)]
        [TestCase("1,2; 3,4; 5,6", 1.2, 3.4, 5.6)]
        [TestCase("1.2, 3.4, 5.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2 3.4 5.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2,\u00A03.4\u00A05.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2\u00A03.4\u00A05.6", 1.2, 3.4, 5.6)]
        [TestCase("(1.2, 3.4 5.6)", 1.2, 3.4, 5.6)]
        [TestCase("1,2\u00A03,4\u00A05,6", 1.2, 3.4, 5.6)]
        [TestCase("(.1, 2.3e-4,1)", 0.1, 0.00023000000000000001, 1)]
        [TestCase("1.0 , 2.5,3.3", 1, 2.5, 3.3)]
        [TestCase("1,0 ; 2,5;3,3", 1, 2.5, 3.3)]
        [TestCase("1.0 ; 2.5;3.3", 1, 2.5, 3.3)]
        [TestCase("1.0,2.5,-3.3", 1, 2.5, -3.3)]
        [TestCase("1;2;3", 1, 2, 3)]
        public void Parse(string text, double expectedX, double expectedY, double expectedZ)
        {
            Assert.AreEqual(true, Point3D.TryParse(text, out var p));
            Assert.AreEqual(expectedX, p.X);
            Assert.AreEqual(expectedY, p.Y);
            Assert.AreEqual(expectedZ, p.Z);

            p = Point3D.Parse(text);
            Assert.AreEqual(expectedX, p.X);
            Assert.AreEqual(expectedY, p.Y);
            Assert.AreEqual(expectedZ, p.Z);

            p = Point3D.Parse(p.ToString());
            Assert.AreEqual(expectedX, p.X);
            Assert.AreEqual(expectedY, p.Y);
            Assert.AreEqual(expectedZ, p.Z);
        }

        [TestCase("1.2")]
        [TestCase("1,2; 2.3; 3")]
        [TestCase("1; 2; 3; 4")]
        public void ParseFails(string text)
        {
            Assert.AreEqual(false, Point3D.TryParse(text, out _));
            Assert.Throws<FormatException>(() => Point3D.Parse(text));
        }

        [TestCase("<Point3D X=\"1\" Y=\"-2\" Z=\"3\" />")]
        [TestCase("<Point3D Y=\"-2\" Z=\"3\"  X=\"1\"/>")]
        [TestCase("<Point3D Z=\"3\" X=\"1\" Y=\"-2\" />")]
        [TestCase("<Point3D><X>1</X><Y>-2</Y><Z>3</Z></Point3D>")]
        [TestCase("<Point3D><Y>-2</Y><Z>3</Z><X>1</X></Point3D>")]
        [TestCase("<Point3D><Z>3</Z><X>1</X><Y>-2</Y></Point3D>")]
        public void ReadFrom(string xml)
        {
            using (var reader = new StringReader(xml))
            {
                var actual = Point3D.ReadFrom(XmlReader.Create(reader));
                Assert.AreEqual(new Point3D(1, -2, 3), actual);
            }
        }

        [Test]
        public void ToDenseVector()
        {
            var p = new Point3D(1, 2, 3);
            var vector = p.ToVector();
            Assert.AreEqual(3, vector.Count);
            Assert.AreEqual(1, vector[0], 1e-6);
            Assert.AreEqual(2, vector[1], 1e-6);
            Assert.AreEqual(3, vector[2], 1e-6);

            var roundtripped = Point3D.OfVector(vector);
            Assert.AreEqual(1, roundtripped.X, 1e-6);
            Assert.AreEqual(2, roundtripped.Y, 1e-6);
            Assert.AreEqual(3, roundtripped.Z, 1e-6);
        }

        [TestCase("1, 2, 3", "1, 2, 3", 1e-4, true)]
        [TestCase("1, 2, 3", "4, 5, 6", 1e-4, false)]
        public void Equals(string p1s, string p2s, double tol, bool expected)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            Assert.AreEqual(expected, p1 == p2);
            Assert.AreEqual(expected, p1.Equals(p2));
            Assert.AreEqual(expected, p1.Equals((object)p2));
            Assert.AreEqual(expected, Equals(p1, p2));
            Assert.AreEqual(expected, p1.Equals(p2, tol));
            Assert.AreNotEqual(expected, p1 != p2);
        }

        [TestCase("0, 0, 0", "0, 0, 1", "0, 0, 0.5")]
        [TestCase("0, 0, 1", "0, 0, 0", "0, 0, 0.5")]
        [TestCase("0, 0, 0", "0, 0, 0", "0, 0, 0")]
        [TestCase("1, 1, 1", "3, 3, 3", "2, 2, 2")]
        [TestCase("-3, -3, -3", "3, 3, 3", "0, 0, 0")]
        public void MidPoint(string p1s, string p2s, string eps)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            var ep = Point3D.Parse(eps);
            var mp = Point3D.MidPoint(p1, p2);
            AssertGeometry.AreEqual(ep, mp, 1e-9);
            var centroid = Point3D.Centroid(p1, p2);
            AssertGeometry.AreEqual(ep, centroid, 1e-9);
        }

        [TestCase("0, 0, 0", "0, 0, 1", "0, 0, 0", "0, 1, 0", "0, 0, 0", "1, 0, 0", "0, 0, 0")]
        [TestCase("0, 0, 5", "0, 0, 1", "0, 4, 0", "0, 1, 0", "3, 0, 0", "1, 0, 0", "3, 4, 5")]
        public void FromPlanes(string rootPoint1, string unitVector1, string rootPoint2, string unitVector2, string rootPoint3, string unitVector3, string eps)
        {
            var plane1 = new Plane3D(Point3D.Parse(rootPoint1), UnitVector3D.Parse(unitVector1));
            var plane2 = new Plane3D(Point3D.Parse(rootPoint2), UnitVector3D.Parse(unitVector2));
            var plane3 = new Plane3D(Point3D.Parse(rootPoint3), UnitVector3D.Parse(unitVector3));
            var p1 = Point3D.IntersectionOf(plane1, plane2, plane3);
            var p2 = Point3D.IntersectionOf(plane2, plane1, plane3);
            var p3 = Point3D.IntersectionOf(plane2, plane3, plane1);
            var p4 = Point3D.IntersectionOf(plane3, plane1, plane2);
            var p5 = Point3D.IntersectionOf(plane3, plane2, plane1);
            var ep = Point3D.Parse(eps);
            foreach (var p in new[] { p1, p2, p3, p4, p5 })
            {
                AssertGeometry.AreEqual(ep, p);
            }
        }

        [TestCase("0, 0, 0", "0, 0, 0", "0, 0, 1", "0, 0, 0")]
        [TestCase("0, 0, 1", "0, 0, 0", "0, 0, 1", "0, 0, -1")]
        public void MirrorAbout(string ps, string rootPoint, string unitVector, string eps)
        {
            var p = Point3D.Parse(ps);
            var p2 = new Plane3D(Point3D.Parse(rootPoint), UnitVector3D.Parse(unitVector));
            var actual = p.MirrorAbout(p2);

            var ep = Point3D.Parse(eps);
            AssertGeometry.AreEqual(ep, actual);
        }

        [TestCase("0, 0, 0", "0, 0, 0", "0, 0, 1", "0, 0, 0")]
        [TestCase("0, 0, 1", "0, 0, 0", "0, 0, 1", "0, 0, 0")]
        [TestCase("0, 0, 1", "0, 10, 0", "0, 1, 0", "0, 10, 1")]
        public void ProjectOnTests(string ps, string rootPoint, string unitVector, string eps)
        {
            var p = Point3D.Parse(ps);
            var p2 = new Plane3D(Point3D.Parse(rootPoint), UnitVector3D.Parse(unitVector));
            var actual = p.ProjectOn(p2);

            var ep = Point3D.Parse(eps);
            AssertGeometry.AreEqual(ep, actual);
        }

        [TestCase("1, 2, 3", "1, 0, 0", "2, 2, 3")]
        [TestCase("1, 2, 3", "0, 1, 0", "1, 3, 3")]
        [TestCase("1, 2, 3", "0, 0, 1", "1, 2, 4")]
        public void AddVector(string ps, string vs, string eps)
        {
            var p = Point3D.Parse(ps);
            var actuals = new[]
                          {
                              p + Vector3D.Parse(vs),
                              p + UnitVector3D.Parse(vs)
                          };
            var expected = Point3D.Parse(eps);
            foreach (var actual in actuals)
            {
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("1, 2, 3", "1, 0, 0", "0, 2, 3")]
        [TestCase("1, 2, 3", "0, 1, 0", "1, 1, 3")]
        [TestCase("1, 2, 3", "0, 0, 1", "1, 2, 2")]
        public void SubtractVector(string ps, string vs, string eps)
        {
            var p = Point3D.Parse(ps);
            var actuals = new[]
                          {
                              p - Vector3D.Parse(vs),
                              p - UnitVector3D.Parse(vs)
                          };
            var expected = Point3D.Parse(eps);
            foreach (var actual in actuals)
            {
                Assert.AreEqual(expected, actual);
            }
        }

        [TestCase("1, 2, 3", "4, 8, 16", "-3, -6, -13")]
        public void SubtractPoint(string p1s, string p2s, string evs)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);

            var expected = Vector3D.Parse(evs);
            Assert.AreEqual(expected, p1 - p2);
        }

        [TestCase("0,0,0", "1,0,0", 1)]
        [TestCase("1,1,1", "2,1,1", 1)]
        public void DistanceTo(string p1s, string p2s, double d)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);

            Assert.AreEqual(d, p1.DistanceTo(p2), 1e-6);
            Assert.AreEqual(d, p2.DistanceTo(p1), 1e-6);
        }

        [TestCase("-1 ; 2;-3")]
        public void ToVectorAndBack(string ps)
        {
            var p = Point3D.Parse(ps);
            AssertGeometry.AreEqual(p, p.ToVector3D().ToPoint3D(), 1e-9);
        }

        [TestCase("-2, 0, 1e-4", null, "(-2, 0, 0.0001)", 1e-4)]
        [TestCase("-2, 0, 1e-4", "F2", "(-2.00, 0.00, 0.00)", 1e-4)]
        public void ToString(string vs, string format, string expected, double tolerance)
        {
            var p = Point3D.Parse(vs);
            var actual = p.ToString(format);
            Assert.AreEqual(expected, actual);
            AssertGeometry.AreEqual(p, Point3D.Parse(actual), tolerance);
        }

        [Test]
        public void XmlRoundtrip()
        {
            var p = new Point3D(1, -2, 3);
            var xml = @"<Point3D X=""1"" Y=""-2"" Z=""3"" />";
            AssertXml.XmlRoundTrips(p, xml, (expected, actual) => AssertGeometry.AreEqual(expected, actual));
        }

        [Test]
        public void XmlContainerRoundtrip()
        {
            var container = new AssertXml.Container<Point3D>
            {
                Value1 = new Point3D(1, 2, 3),
                Value2 = new Point3D(4, 5, 6)
            };
            var expected = "<ContainerOfPoint3D>\r\n" +
                           "  <Value1 X=\"1\" Y=\"2\" Z=\"3\"></Value1>\r\n" +
                           "  <Value2 X=\"4\" Y=\"5\" Z=\"6\"></Value2>\r\n" +
                           "</ContainerOfPoint3D>";
            var roundTrip = AssertXml.XmlSerializerRoundTrip(container, expected);
            AssertGeometry.AreEqual(container.Value1, roundTrip.Value1);
            AssertGeometry.AreEqual(container.Value2, roundTrip.Value2);
        }

        [Test]
        public void XmlElements()
        {
            var v = new Point3D(1, 2, 3);
            var serializer = new XmlSerializer(typeof(Point3D));
            AssertGeometry.AreEqual(v, (Point3D)serializer.Deserialize(new StringReader(@"<Point3D><X>1</X><Y>2</Y><Z>3</Z></Point3D>")));
        }

        [Test]
        public void XmlContainerElements()
        {
            var xml = "<ContainerOfPoint3D>\r\n" +
                      "  <Value1><X>1</X><Y>2</Y><Z>3</Z></Value1>\r\n" +
                      "  <Value2><X>4</X><Y>5</Y><Z>6</Z></Value2>\r\n" +
                      "</ContainerOfPoint3D>";
            var serializer = new XmlSerializer(typeof(AssertXml.Container<Point3D>));
            var deserialized = (AssertXml.Container<Point3D>)serializer.Deserialize(new StringReader(xml));
            AssertGeometry.AreEqual(new Point3D(1, 2, 3), deserialized.Value1);
            AssertGeometry.AreEqual(new Point3D(4, 5, 6), deserialized.Value2);
        }
    }
}
