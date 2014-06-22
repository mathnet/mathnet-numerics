namespace GeometryUnitTests
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class Point3DTests
    {
        [TestCase("0, 0, 0", "0, 0, 1", "0, 0, 0.5")]
        [TestCase("0, 0, 1", "0, 0, 0", "0, 0, 0.5")]
        [TestCase("0, 0, 0", "0, 0, 0", "0, 0, 0")]
        [TestCase("1, 1, 1", "3, 3, 3", "2, 2, 2")]
        [TestCase("-3, -3, -3", "3, 3, 3", "0, 0, 0")]
        public void MidPointTest(string p1s, string p2s, string eps)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            var ep = Point3D.Parse(eps);
            Point3D mp = Point3D.MidPoint(p1, p2);
            LinearAlgebraAssert.AreEqual(ep, mp, 1e-9);
            var centroid = Point3D.Centroid(p1, p2);
            LinearAlgebraAssert.AreEqual(ep, centroid, 1e-9);
        }

        [TestCase("p:{0, 0, 0} v:{0, 0, 1}", "p:{0, 0, 0} v:{0, 1, 0}", "p:{0, 0, 0} v:{1, 0, 0}", "0, 0, 0")]
        [TestCase("p:{0, 0, 5} v:{0, 0, 1}", "p:{0, 4, 0} v:{0, 1, 0}", "p:{3, 0, 0} v:{1, 0, 0}", "3, 4, 5")]
        public void FromPlanesTests(string pl1s, string pl2s, string pl3s, string eps)
        {
            var plane1 = Plane.Parse(pl1s);
            var plane2 = Plane.Parse(pl2s);
            var plane3 = Plane.Parse(pl3s);
            var p1 = Point3D.ItersectionOf(plane1, plane2, plane3);
            var p2 = Point3D.ItersectionOf(plane2, plane1, plane3);
            var p3 = Point3D.ItersectionOf(plane2, plane3, plane1);
            var p4 = Point3D.ItersectionOf(plane3, plane1, plane2);
            var p5 = Point3D.ItersectionOf(plane3, plane2, plane1);
            var ep = Point3D.Parse(eps);
            foreach (var p in new[] { p1, p2, p3, p4, p5 })
            {
                LinearAlgebraAssert.AreEqual(ep, p);
            }
        }

        [TestCase("0, 0, 0", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, 0")]
        [TestCase("0, 0, 1", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, -1")]
        public void MirrorAboutTests(string ps, string pls, string eps)
        {
            var p = Point3D.Parse(ps);
            var p2 = Plane.Parse(pls);
            var actual = p.MirrorAbout(p2);

            var ep = Point3D.Parse(eps);
            LinearAlgebraAssert.AreEqual(ep, actual);
        }

        [TestCase("0, 0, 0", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, 0")]
        [TestCase("0, 0, 1", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, 0")]
        [TestCase("0, 0, 1", "p:{0, 10, 0} v:{0, 1, 0}", "0, 10, 1")]
        public void ProjectOnTests(string ps, string pls, string eps)
        {
            var p = Point3D.Parse(ps);
            var p2 = Plane.Parse(pls);
            var actual = p.ProjectOn(p2);

            var ep = Point3D.Parse(eps);
            LinearAlgebraAssert.AreEqual(ep, actual);
        }

        [TestCase("0,0,0", "1,0,0", "1,0,0")]
        public void AddVectorTest(string ps, string vs, string expected)
        {
            Point3D point3D = Point3D.Parse(ps);
            Vector3D vector3D = Vector3D.Parse(vs);
            Assert.AreEqual(Point3D.Parse(expected), point3D + vector3D);
        }

        [TestCase("0,0,0", "1,0,0", 1)]
        [TestCase("1,1,1", "2,1,1", 1)]
        public void DistanceToTest(string p1s, string p2s, double d)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);

            Assert.AreEqual(d, p1.DistanceTo(p2), 1e-6);
            Assert.AreEqual(d, p2.DistanceTo(p1), 1e-6);
        }

        [TestCase("1.0 , 2.5,3.3", new double[] { 1, 2.5, 3.3 })]
        [TestCase("1,0 ; 2,5;3,3", new double[] { 1, 2.5, 3.3 })]
        [TestCase("-1.0 ; 2.5;3.3", new double[] { -1, 2.5, 3.3 })]
        [TestCase("-1 ; -2;-3", new double[] { -1, -2, -3 })]
        public void ParseTest(string pointAsString, double[] expectedPoint)
        {
            Point3D point3D = Point3D.Parse(pointAsString);
            Point3D expected = new Point3D(expectedPoint);
            LinearAlgebraAssert.AreEqual(expected, point3D, 1e-9);
        }

        [TestCase("-1 ; 2;-3")]
        public void ToVectorAndBack(string ps)
        {
            Point3D p = Point3D.Parse(ps);
            LinearAlgebraAssert.AreEqual(p, p.ToVector().ToPoint3D(), 1e-9);
        }

        [TestCase("1, -2, 3", false, @"<Point3D X=""1"" Y=""-2"" Z=""3"" />")]
        [TestCase("1, -2, 3", true, "<Point3D><X>1</X><Y>-2</Y><Z>3</Z></Point3D>")]
        public void XmlRoundtrip(string vs, bool asElements, string xml)
        {
            var p = Point3D.Parse(vs);
            p.SerializeAsElements = asElements;
            AssertXml.XmlRoundTrips(p, xml,(expected, actual) => LinearAlgebraAssert.AreEqual(expected,actual));
        }

        [Test]
        public void BinaryRountrip()
        {
            var v = new Point3D(1, 2, 3);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, v);
                ms.Flush();
                ms.Position = 0;
                var roundTrip = (Point3D)formatter.Deserialize(ms);
                LinearAlgebraAssert.AreEqual(v, roundTrip);
            }
        }
    }
}
