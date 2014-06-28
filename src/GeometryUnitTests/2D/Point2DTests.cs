namespace MathNet.GeometryUnitTests
{
    using Geometry;
    using Geometry.Units;
    using NUnit.Framework;

    public class Point2DTests
    {
        [TestCase("0, 0", "1, 0", 1.0)]
        [TestCase("2, 0", "1, 0", 1.0)]
        [TestCase("2, 0", "-1, 0", 3.0)]
        [TestCase("0, 2", "0, -1", 3.0)]
        public void DistanceTo(string p1s, string p2s, double expected)
        {
            var p1 = Point2D.Parse(p1s);
            var p2 = Point2D.Parse(p2s);
            Assert.AreEqual(expected, p1.DistanceTo(p2));
            Assert.AreEqual(expected, p2.DistanceTo(p1));
        }

        [TestCase(5, "90 °", "0, 5")]
        [TestCase(3, "-90 °", "0, -3")]
        [TestCase(1, "45 °", "0.71, 0.71")]
        [TestCase(1, "-45 °", "0.71, -0.71")]
        [TestCase(1, "0 °", "1, 0")]
        [TestCase(1, "180 °", "-1, 0")]
        public void PolarCtorTest(int r, string avs, string eps)
        {
            var av = Angle.Parse(avs);
            var p = new Point2D(r, av);
            var ep = Point2D.Parse(eps);
            AssertGemoetry.AreEqual(ep, p, 1e-2);
        }

        [TestCase("1, 0", "90 °", "0, 1")]
        [TestCase("1, 0", "-90 °", "0, -1")]
        [TestCase("1, 0", "45 °", "0.71, 0.71")]
        [TestCase("1, 0", "-45 °", "0.71, -0.71")]
        [TestCase("1, 0", "30 °", "0,87, 0.5")]
        [TestCase("-5, 0", "30 °", "-4.33, -2.5")]
        public void RotateTest(string ps, string avs, string eps)
        {
            var p = Point2D.Parse(ps);
            var av = Angle.Parse(avs);
            var expected = Point2D.Parse(eps);
            var rm = Matrix2D.Rotation(av);
            var actual = p.TransformBy(rm);
            AssertGemoetry.AreEqual(expected, actual, 1e-2);
        }

        [TestCase("1,2", false, @"<Point2D X=""1"" Y=""2"" />")]
        [TestCase("1,2", true, @"<Point2D><X>1</X><Y>2</Y></Point2D>")]
        public void XmlRountrip(string vs, bool asElements, string xml)
        {
            var p = Point2D.Parse(vs);
            p.SerializeAsElements = asElements;
            AssertXml.XmlRoundTrips(p, xml, (e, a) => AssertGemoetry.AreEqual(e, a));
        }
    }
}
