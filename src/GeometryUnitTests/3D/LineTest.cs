namespace GeometryUnitTests
{
    using Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class LineTest
    {
        [TestCase("0, 0, 0", "1, -1, 1", "1, -1, 1")]
        public void DirectionsTest(string p1s, string p2s, string evs)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            Line3D l = new Line3D(p1, p2);
            var excpected = UnitVector3D.Parse(evs);
            LinearAlgebraAssert.AreEqual(excpected, l.Direction, 1e-6);
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
            LinearAlgebraAssert.AreEqual(Point3D.Parse(sps), actual.StartPoint, 1e-6);
            LinearAlgebraAssert.AreEqual(p, actual.EndPoint, 1e-6);
        }
        [TestCase("1, 2, 3", "4, 5, 6", null, @"<Line3D><StartPoint X=""1"" Y=""2"" Z=""3"" /><EndPoint X=""4"" Y=""5"" Z=""6"" /></Line3D>")]
        [TestCase("1, 2, 3", "4, 5, 6", "meh", @"<Line3D><StartPoint X=""1"" Y=""2"" Z=""3"" /><EndPoint X=""4"" Y=""5"" Z=""6"" /></Line3D>")]
        public void XmlTests(string p1s, string p2s, bool asElements, string xml)
        {
            Point3D p1 = Point3D.Parse(p1s);
            Point3D p2 = Point3D.Parse(p2s);
            var l = new Line3D(p1, p2);
            AssertXml.XmlRoundTrips(l, xml, (e, a) => LinearAlgebraAssert.AreEqual(e, a));
        }
    }
}
