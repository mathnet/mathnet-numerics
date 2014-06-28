namespace MathNet.GeometryUnitTests
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class PlaneTest
    {
        const string X = "1; 0 ; 0";
        const string Z = "0; 0; 1";
        const string NegativeZ = "0; 0; -1";
        const string ZeroPoint = "0; 0; 0";

        [Test]
        public void CtorTest()
        {
            var plane1 = new Plane(new Point3D(0, 0, 3), UnitVector3D.ZAxis);
            var plane2 = new Plane(0, 0, 3, -3);
            var plane3 = new Plane(UnitVector3D.ZAxis, 3);
            AssertGemoetry.AreEqual(plane1, plane2);
            AssertGemoetry.AreEqual(plane1, plane3);
        }

        [TestCase("p:{0, 0, 0} v:{1, 0, 0}", new double[] { 0, 0, 0 }, new double[] { 1, 0, 0 })]
        [TestCase("1, 0, 0, 0", new double[] { 0, 0, 0 }, new double[] { 1, 0, 0 })]
        public void ParseTest(string s, double[] pds, double[] vds)
        {
            var plane = Plane.Parse(s);
            AssertGemoetry.AreEqual(new Point3D(pds), plane.RootPoint);
            AssertGemoetry.AreEqual(new Vector3D(vds), plane.Normal);
        }

        [TestCase(ZeroPoint, "p:{0, 0, 0} v:{0, 0, 1}", ZeroPoint)]
        [TestCase(ZeroPoint, "p:{0, 0, -1} v:{0, 0, 1}", "0; 0;-1")]
        [TestCase(ZeroPoint, "p:{0, 0, 1} v:{0, 0, -1}", "0; 0; 1")]
        [TestCase("1; 2; 3", "p:{0, 0, 0} v:{0, 0, 1}", "1; 2; 0")]
        public void ProjectPointOnTest(string ps, string pls, string eps)
        {
            var plane = Plane.Parse(pls);
            var projectedPoint = plane.Project(Point3D.Parse(ps));
            var expected = Point3D.Parse(eps);
            AssertGemoetry.AreEqual(expected, projectedPoint, float.Epsilon);
        }

        [Test]
        private void ProjectPoint(Point3D pointToProject, Point3D planeRootPoint, UnitVector3D planeNormal, Point3D projectedresult)
        {
            var plane = new Plane(planeNormal, planeRootPoint);
            var projectOn = plane.Project(pointToProject);
            AssertGemoetry.AreEqual(projectedresult, projectOn, float.Epsilon);
        }

        [TestCase(ZeroPoint, Z, ZeroPoint, 0)]
        [TestCase(ZeroPoint, Z, "1; 2; 0", 0)]
        [TestCase(ZeroPoint, Z, "1; -2; 0", 0)]
        [TestCase(ZeroPoint, Z, "1; 2; 3", 3)]
        [TestCase(ZeroPoint, Z, "-1; 2; -3", -3)]
        [TestCase(ZeroPoint, NegativeZ, ZeroPoint, 0)]
        [TestCase(ZeroPoint, NegativeZ, "1; 2; 1", -1)]
        [TestCase(ZeroPoint, NegativeZ, "1; 2; -1", 1)]
        [TestCase("0; 0; -1", NegativeZ, ZeroPoint, -1)]
        [TestCase("0; 0; 1", NegativeZ, ZeroPoint, 1)]
        [TestCase(ZeroPoint, X, "1; 0; 0", 1)]
        [TestCase("188,6578; 147,0620; 66,0170", Z, "118,6578; 147,0620; 126,1170", 60.1)]
        public void SignedDistanceToPoint(string prps, string pns, string ps, double expected)
        {
            var plane = new Plane(UnitVector3D.Parse(pns), Point3D.Parse(prps));
            var p = Point3D.Parse(ps);
            Assert.AreEqual(expected, plane.SignedDistanceTo(p), 1E-6);
        }

        [TestCase(ZeroPoint, Z, ZeroPoint, Z, 0)]
        [TestCase(ZeroPoint, Z, "0;0;1", Z, 1)]
        [TestCase(ZeroPoint, Z, "0;0;-1", Z, -1)]
        [TestCase(ZeroPoint, NegativeZ, "0;0;-1", Z, 1)]
        public void SignedDistanceToOtherPlane(string prps, string pns, string otherPlaneRootPointString, string otherPlaneNormalString, double expectedValue)
        {
            var plane = new Plane(UnitVector3D.Parse(pns), Point3D.Parse(prps));
            var otherPlane = new Plane(UnitVector3D.Parse(otherPlaneNormalString), Point3D.Parse(otherPlaneRootPointString));
            Assert.AreEqual(expectedValue, plane.SignedDistanceTo(otherPlane), 1E-6);
        }

        [TestCase(ZeroPoint, Z, ZeroPoint, Z, 0)]
        [TestCase(ZeroPoint, Z, ZeroPoint, X, 0)]
        [TestCase(ZeroPoint, Z, "0;0;1", X, 1)]
        public void SignedDistanceToRay(string prps, string pns, string rayThroughPointString, string rayDirectionString, double expectedValue)
        {
            var plane = new Plane(UnitVector3D.Parse(pns), Point3D.Parse(prps));
            var otherPlane = new Ray3D(Point3D.Parse(rayThroughPointString), UnitVector3D.Parse(rayDirectionString));
            Assert.AreEqual(expectedValue, plane.SignedDistanceTo(otherPlane), 1E-6);
        }

        [Test]
        public void ProjectLineOnTest()
        {
            var unitVector = UnitVector3D.ZAxis;
            var rootPoint = new Point3D(0, 0, 1);
            var plane = new Plane(unitVector, rootPoint);

            var line = new Line3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            var projectOn = plane.Project(line);
            AssertGemoetry.AreEqual(new Line3D(new Point3D(0, 0, 1), new Point3D(1, 0, 1)), projectOn, float.Epsilon);
        }

        [Test]
        public void ProjectVectorOnTest()
        {
            var unitVector = UnitVector3D.ZAxis;
            var rootPoint = new Point3D(0, 0, 1);
            var plane = new Plane(unitVector, rootPoint);
            var vector = new Vector3D(1, 0, 0);
            var projectOn = plane.Project(vector);
            AssertGemoetry.AreEqual(new Vector3D(1, 0, 0), projectOn.Direction, float.Epsilon);
            AssertGemoetry.AreEqual(new Point3D(0, 0, 1), projectOn.ThroughPoint, float.Epsilon);
        }

        [TestCase("p:{0, 0, 0} v:{0, 0, 1}", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, 0", "0, 0, 0", ExpectedException = typeof(ArgumentException))]
        [TestCase("p:{0, 0, 0} v:{0, 0, 1}", "p:{0, 0, 0} v:{0, 1, 0}", "0, 0, 0", "-1, 0, 0")]
        [TestCase("p:{0, 0, 2} v:{0, 0, 1}", "p:{0, 0, 0} v:{0, 1, 0}", "0, 0, 2", "-1, 0, 0")]
        public void InterSectionWithPlaneTest(string pl1s, string pl2s, string eps, string evs)
        {
            var plane1 = Plane.Parse(pl1s);
            var plane2 = Plane.Parse(pl2s);
            var intersections = new[]
            {
                plane1.IntersectionWith(plane2),
                plane2.IntersectionWith(plane1)
            };
            foreach (var intersection in intersections)
            {
                AssertGemoetry.AreEqual(Point3D.Parse(eps), intersection.ThroughPoint);
                AssertGemoetry.AreEqual(UnitVector3D.Parse(evs), intersection.Direction);
            }
        }

        [Test]
        public void MirrorPointTest()
        {
            var plane = new Plane(UnitVector3D.ZAxis, new Point3D(0, 0, 0));
            var point3D = new Point3D(1, 2, 3);
            Point3D mirrorAbout = plane.MirrorAbout(point3D);
            AssertGemoetry.AreEqual(new Point3D(1, 2, -3), mirrorAbout, float.Epsilon);
        }

        [Test]
        public void SignOfD()
        {
            var plane1 = new Plane(UnitVector3D.ZAxis, new Point3D(0, 0, 100));
            Assert.AreEqual(-100, plane1.D);
        }

        [Test]
        public void InterSectionPointDifferentOrderTest()
        {
            var plane1 = new Plane(new UnitVector3D(0.8, 0.3, 0.01), new Point3D(20, 0, 0));
            var plane2 = new Plane(new UnitVector3D(0.002, 1, 0.1), new Point3D(0, 0, 0));
            var plane3 = new Plane(new UnitVector3D(0.5, 0.5, 1), new Point3D(0, 0, -30));
            var pointFromPlanes1 = Plane.PointFromPlanes(plane1, plane2, plane3);
            var pointFromPlanes2 = Plane.PointFromPlanes(plane2, plane1, plane3);
            var pointFromPlanes3 = Plane.PointFromPlanes(plane3, plane1, plane2);
            AssertGemoetry.AreEqual(pointFromPlanes1, pointFromPlanes2, 1E-10);
            AssertGemoetry.AreEqual(pointFromPlanes3, pointFromPlanes2, 1E-10);
        }

        [TestCase("p:{0, 0, 0} v:{1, 0, 0}", "p:{0, 0, 0} v:{0, 1, 0}", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, 0")]
        [TestCase("p:{0, 0, 0} v:{-1, 0, 0}", "p:{0, 0, 0} v:{0, 1, 0}", "p:{0, 0, 0} v:{0, 0, 1}", "0, 0, 0")]
        [TestCase("p:{20, 0, 0} v:{1, 0, 0}", "p:{0, 0, 0} v:{0, 1, 0}", "p:{0, 0, -30} v:{0, 0, 1}", "20, 0, -30")]
        [TestCase("1, 1, 0, -12", "-1, 1, 0, -12", "0, 0, 1, -5", "0, 16.970563, 5")]
        public void PointFromPlanes(string pl1s, string pl2s, string pl3s, string eps)
        {
            var plane1 = Plane.Parse(pl1s);
            var plane2 = Plane.Parse(pl2s);
            var plane3 = Plane.Parse(pl3s);
            var points = new[]
            {
                Plane.PointFromPlanes(plane1, plane2, plane3),
                Plane.PointFromPlanes(plane2, plane1, plane3),
                Plane.PointFromPlanes(plane1, plane3, plane2),
                Plane.PointFromPlanes(plane2, plane3, plane1),
                Plane.PointFromPlanes(plane3, plane2, plane1),
                Plane.PointFromPlanes(plane3, plane1, plane2),
            };
            var expected = Point3D.Parse(eps);
            foreach (var point in points)
            {
                AssertGemoetry.AreEqual(expected, point);
            }
        }

        [TestCase("p:{0, 0, 0} v:{0, 0, 1}", @"<Plane><RootPoint X=""0"" Y=""0"" Z=""0"" /><Normal X=""0"" Y=""0"" Z=""1"" /></Plane>")]
        public void XmlRoundTrips(string p1s, string xml)
        {
            var plane = Plane.Parse(p1s);
            AssertXml.XmlRoundTrips(plane, xml, (e, a) => AssertGemoetry.AreEqual(e, a));
        }

        [TestCase("p:{0, 0, 0} v:{0, 0, 1}")]
        public void BinaryRountrip(string pls)
        {
            var plane = Plane.Parse(pls);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, plane);
                ms.Flush();
                ms.Position = 0;
                var roundTrip = (Plane)formatter.Deserialize(ms);
                AssertGemoetry.AreEqual(plane, roundTrip);
            }
        }
    }
}
