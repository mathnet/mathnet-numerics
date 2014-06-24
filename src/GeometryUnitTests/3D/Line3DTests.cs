namespace MathNet.GeometryUnitTests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class Line3DTests
    {
        [Test]
        public void Ctor()
        {
            Assert.Throws<ArgumentException>(() => new Line3D(Point3D.Origin, Point3D.Origin));
        }

        [TestCase("0, 0, 0", "1, -1, 1", "1, -1, 1")]
        public void DirectionsTest(string p1s, string p2s, string evs)
        {
            Line3D l = Line3D.Parse(p1s, p2s);
            var excpected = UnitVector3D.Parse(evs);
            AssertGemoetry.AreEqual(excpected, l.Direction);
        }

        [TestCase("0, 0, 0", "1, -1, 1", "p:{0, 0, 0} v:{1, 0, 0}", "0, 0, 0", "0, -1, 1")]
        public void ProjectOn(string p1s, string p2s, string pls, string ep1s, string ep2s)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            var line = new Line3D(p1, p2);
            var plane = Plane.Parse(pls);
            var expected = new Line3D(Point3D.Parse(ep1s), Point3D.Parse(ep2s));
            AssertGemoetry.AreEqual(expected, line.ProjectOn(plane));
        }

        [TestCase("0, 0, 0", "1, -2, 3", 3.741657)]
        public void Length(string p1s, string p2s, double expected)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            var l = new Line3D(p1, p2);
            Assert.AreEqual(expected, l.Length, 1e-6);
        }

        [TestCase("0, 0, 0", "1, -1, 1", "0, 0, 0", "1, -1, 1", true)]
        [TestCase("0, 0, 2", "1, -1, 1", "0, 0, 0", "1, -1, 1", false)]
        [TestCase("0, 0, 0", "1, -1, 1", "0, 0, 0", "2, -1, 1", false)]
        public void Equals(string p1s, string p2s, string p3s, string p4s, bool expected)
        {
            var line1 = new Line3D(Point3D.Parse(p1s), Point3D.Parse(p2s));
            var line2 = new Line3D(Point3D.Parse(p3s), Point3D.Parse(p4s));
            Assert.AreEqual(expected, line1.Equals(line2));
            Assert.AreEqual(expected, line1 == line2);
            Assert.AreEqual(!expected, line1 != line2);
        }

        [TestCase("0, 0, 0", "1, 0, 0", "0.5, 1, 0", true, "0.5, 0, 0")]
        [TestCase("0, 0, 0", "1, 0, 0", "0.5, 1, 0", false, "0.5, 0, 0")]
        [TestCase("0, 0, 0", "1, 0, 0", "2, 1, 0", true, "1, 0, 0")]
        [TestCase("0, 0, 0", "1, 0, 0", "2, 1, 0", false, "2, 0, 0")]
        [TestCase("0, 0, 0", "1, 0, 0", "-2, 1, 0", true, "0, 0, 0")]
        [TestCase("0, 0, 0", "1, 0, 0", "-2, 1, 0", false, "-2, 0, 0")]
        public void LineToTest(string p1s, string p2s, string ps, bool mustStartFromLine, string sps)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            Line3D l = new Line3D(p1, p2);
            var p = Point3D.Parse(ps);
            var actual = l.LineTo(p, mustStartFromLine);
            AssertGemoetry.AreEqual(Point3D.Parse(sps), actual.StartPoint, 1e-6);
            AssertGemoetry.AreEqual(p, actual.EndPoint, 1e-6);
        }

        [TestCase("1, 2, 3", "4, 5, 6", @"<Line3D><StartPoint X=""1"" Y=""2"" Z=""3"" /><EndPoint X=""4"" Y=""5"" Z=""6"" /></Line3D>")]
        public void XmlTests(string p1s, string p2s, string xml)
        {
            Point3D p1 = Point3D.Parse(p1s);
            Point3D p2 = Point3D.Parse(p2s);
            var l = new Line3D(p1, p2);
            AssertXml.XmlRoundTrips(l, xml, (e, a) => AssertGemoetry.AreEqual(e, a));
        }

        [Test]
        public void BinaryRountrip()
        {
            var line = new Line3D(new Point3D(1, 2, 3), new Point3D(4, 5, 6));
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, line);
                ms.Flush();
                ms.Position = 0;
                var roundTrip = (Line3D)formatter.Deserialize(ms);
                AssertGemoetry.AreEqual(line, roundTrip);
            }
        }
    }
}
