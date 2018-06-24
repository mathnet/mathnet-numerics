// ReSharper disable InconsistentNaming

using System;
using System.Linq;
using MathNet.Numerics.Spatial.Euclidean3D;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial.Euclidean3D
{
    [TestFixture]
    public class Plane3DTests
    {
        private const string X = "1; 0 ; 0";
        private const string Z = "0; 0; 1";
        private const string NegativeZ = "0; 0; -1";
        private const string ZeroPoint = "0; 0; 0";

        [Test]
        public void Ctor()
        {
            var plane1 = new Plane3D(new Point3D(0, 0, 3), UnitVector3D.ZAxis);
            var plane2 = new Plane3D(0, 0, 3, -3);
            var plane3 = new Plane3D(UnitVector3D.ZAxis, 3);
            var plane4 = Plane3D.FromPoints(new Point3D(0, 0, 3), new Point3D(5, 3, 3), new Point3D(-2, 1, 3));
            AssertGeometry.AreEqual(plane1, plane2);
            AssertGeometry.AreEqual(plane1, plane3);
            AssertGeometry.AreEqual(plane1, plane4);
        }

        [TestCase("0, 0, 0", "1, 0, 0", "0, 0, 0", "1, 0, 0")]
        public void Parse(string rootPoint, string unitVector, string pds, string vds)
        {
            var plane = new Plane3D(Point3D.Parse(rootPoint), UnitVector3D.Parse(unitVector));
            AssertGeometry.AreEqual(Point3D.Parse(pds), plane.RootPoint);
            AssertGeometry.AreEqual(Vector3D.Parse(vds), plane.Normal);
        }

        [TestCase("1, 0, 0, 0", "0, 0, 0", "1, 0, 0")]
        public void Parse2(string s, string pds, string vds)
        {
            var plane = this.GetPlaneFrom4Doubles(s);
            AssertGeometry.AreEqual(Point3D.Parse(pds), plane.RootPoint);
            AssertGeometry.AreEqual(Vector3D.Parse(vds), plane.Normal);
        }

        [TestCase(ZeroPoint, "0, 0, 0", "0, 0, 1", ZeroPoint)]
        [TestCase(ZeroPoint, "0, 0, -1", "0, 0, 1", "0; 0;-1")]
        [TestCase(ZeroPoint, "0, 0, 1", "0, 0, -1", "0; 0; 1")]
        [TestCase("1; 2; 3", "0, 0, 0", "0, 0, 1", "1; 2; 0")]
        public void ProjectPointOn(string ps, string rootPoint, string unitVector, string eps)
        {
            var plane = new Plane3D(Point3D.Parse(rootPoint), UnitVector3D.Parse(unitVector));
            var projectedPoint = plane.Project(Point3D.Parse(ps));
            var expected = Point3D.Parse(eps);
            AssertGeometry.AreEqual(expected, projectedPoint, float.Epsilon);
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
            var plane = new Plane3D(UnitVector3D.Parse(pns), Point3D.Parse(prps));
            var p = Point3D.Parse(ps);
            Assert.AreEqual(expected, plane.SignedDistanceTo(p), 1E-6);
        }

        [TestCase(ZeroPoint, Z, ZeroPoint, Z, 0)]
        [TestCase(ZeroPoint, Z, "0;0;1", Z, 1)]
        [TestCase(ZeroPoint, Z, "0;0;-1", Z, -1)]
        [TestCase(ZeroPoint, NegativeZ, "0;0;-1", Z, 1)]
        public void SignedDistanceToOtherPlane(string prps, string pns, string otherPlaneRootPointString, string otherPlaneNormalString, double expectedValue)
        {
            var plane = new Plane3D(UnitVector3D.Parse(pns), Point3D.Parse(prps));
            var otherPlane = new Plane3D(UnitVector3D.Parse(otherPlaneNormalString), Point3D.Parse(otherPlaneRootPointString));
            Assert.AreEqual(expectedValue, plane.SignedDistanceTo(otherPlane), 1E-6);
        }

        [TestCase(ZeroPoint, Z, ZeroPoint, Z, 0)]
        [TestCase(ZeroPoint, Z, ZeroPoint, X, 0)]
        [TestCase(ZeroPoint, Z, "0;0;1", X, 1)]
        public void SignedDistanceToRay(string prps, string pns, string rayThroughPointString, string rayDirectionString, double expectedValue)
        {
            var plane = new Plane3D(UnitVector3D.Parse(pns), Point3D.Parse(prps));
            var otherPlane = new Ray3D(Point3D.Parse(rayThroughPointString), UnitVector3D.Parse(rayDirectionString));
            Assert.AreEqual(expectedValue, plane.SignedDistanceTo(otherPlane), 1E-6);
        }

        [Test]
        public void ProjectLineOn()
        {
            var unitVector = UnitVector3D.ZAxis;
            var rootPoint = new Point3D(0, 0, 1);
            var plane = new Plane3D(unitVector, rootPoint);

            var line = new LineSegment3D(new Point3D(0, 0, 0), new Point3D(1, 0, 0));
            var projectOn = plane.Project(line);
            AssertGeometry.AreEqual(new LineSegment3D(new Point3D(0, 0, 1), new Point3D(1, 0, 1)), projectOn, float.Epsilon);
        }

        [Test]
        public void ProjectVectorOn()
        {
            var unitVector = UnitVector3D.ZAxis;
            var rootPoint = new Point3D(0, 0, 1);
            var plane = new Plane3D(unitVector, rootPoint);
            var vector = new Vector3D(1, 0, 0);
            var projectOn = plane.Project(vector);
            AssertGeometry.AreEqual(new Vector3D(1, 0, 0), projectOn.Direction, float.Epsilon);
            AssertGeometry.AreEqual(new Point3D(0, 0, 1), projectOn.ThroughPoint, float.Epsilon);
        }

        [TestCase("0, 0, 0", "0, 0, 1", "0, 0, 0", "0, 1, 0", "0, 0, 0", "-1, 0, 0")]
        [TestCase("0, 0, 2", "0, 0, 1", "0, 0, 0", "0, 1, 0", "0, 0, 2", "-1, 0, 0")]
        public void InterSectionWithPlane(string rootPoint1, string unitVector1, string rootPoint2, string unitVector2, string eps, string evs)
        {
            var plane1 = new Plane3D(Point3D.Parse(rootPoint1), UnitVector3D.Parse(unitVector1));
            var plane2 = new Plane3D(Point3D.Parse(rootPoint2), UnitVector3D.Parse(unitVector2));
            var intersections = new[]
            {
                plane1.IntersectionWith(plane2),
                plane2.IntersectionWith(plane1)
            };
            foreach (var intersection in intersections)
            {
                AssertGeometry.AreEqual(Point3D.Parse(eps), intersection.ThroughPoint);
                AssertGeometry.AreEqual(UnitVector3D.Parse(evs), intersection.Direction);
            }
        }

        [TestCase("0, 0, 0", "0, 0, 1", "0, 0, 0", "0, 0, 1", "0, 0, 0", "0, 0, 0")]
        public void InterSectionWithPlaneTest_BadArgument(string rootPoint1, string unitVector1, string rootPoint2, string unitVector2, string eps, string evs)
        {
            var plane1 = new Plane3D(Point3D.Parse(rootPoint1), UnitVector3D.Parse(unitVector1));
            var plane2 = new Plane3D(Point3D.Parse(rootPoint2), UnitVector3D.Parse(unitVector2));

            Assert.Throws<ArgumentException>(() => plane1.IntersectionWith(plane2));
            Assert.Throws<ArgumentException>(() => plane2.IntersectionWith(plane1));
        }

        [Test]
        public void MirrorPoint()
        {
            var plane = new Plane3D(UnitVector3D.ZAxis, new Point3D(0, 0, 0));
            var point3D = new Point3D(1, 2, 3);
            var mirrorAbout = plane.MirrorAbout(point3D);
            AssertGeometry.AreEqual(new Point3D(1, 2, -3), mirrorAbout, float.Epsilon);
        }

        [Test]
        public void SignOfD()
        {
            var plane1 = new Plane3D(UnitVector3D.ZAxis, new Point3D(0, 0, 100));
            Assert.AreEqual(-100, plane1.D);
        }

        [Test]
        public void InterSectionPointDifferentOrder()
        {
            var plane1 = new Plane3D(UnitVector3D.Create(0.8, 0.3, 0.01), new Point3D(20, 0, 0));
            var plane2 = new Plane3D(UnitVector3D.Create(0.002, 1, 0.1), new Point3D(0, 0, 0));
            var plane3 = new Plane3D(UnitVector3D.Create(0.5, 0.5, 1), new Point3D(0, 0, -30));
            var pointFromPlanes1 = Plane3D.PointFromPlanes(plane1, plane2, plane3);
            var pointFromPlanes2 = Plane3D.PointFromPlanes(plane2, plane1, plane3);
            var pointFromPlanes3 = Plane3D.PointFromPlanes(plane3, plane1, plane2);
            AssertGeometry.AreEqual(pointFromPlanes1, pointFromPlanes2, 1E-10);
            AssertGeometry.AreEqual(pointFromPlanes3, pointFromPlanes2, 1E-10);
        }

        [TestCase("0, 0, 0", "1, 0, 0", "0, 0, 0", "0, 1, 0", "0, 0, 0", "0, 0, 1", "0, 0, 0")]
        [TestCase("0, 0, 0", "-1, 0, 0", "0, 0, 0", "0, 1, 0", "0, 0, 0", "0, 0, 1", "0, 0, 0")]
        [TestCase("20, 0, 0", "1, 0, 0", "0, 0, 0", "0, 1, 0", "0, 0, -30", "0, 0, 1", "20, 0, -30")]
        public void PointFromPlanes(string rootPoint1, string unitVector1, string rootPoint2, string unitVector2, string rootPoint3, string unitVector3, string eps)
        {
            var plane1 = new Plane3D(Point3D.Parse(rootPoint1), UnitVector3D.Parse(unitVector1));
            var plane2 = new Plane3D(Point3D.Parse(rootPoint2), UnitVector3D.Parse(unitVector2));
            var plane3 = new Plane3D(Point3D.Parse(rootPoint3), UnitVector3D.Parse(unitVector3));
            var points = new[]
            {
                Plane3D.PointFromPlanes(plane1, plane2, plane3),
                Plane3D.PointFromPlanes(plane2, plane1, plane3),
                Plane3D.PointFromPlanes(plane1, plane3, plane2),
                Plane3D.PointFromPlanes(plane2, plane3, plane1),
                Plane3D.PointFromPlanes(plane3, plane2, plane1),
                Plane3D.PointFromPlanes(plane3, plane1, plane2),
            };
            var expected = Point3D.Parse(eps);
            foreach (var point in points)
            {
                AssertGeometry.AreEqual(expected, point);
            }
        }

        [TestCase("1, 1, 0, -12", "-1, 1, 0, -12", "0, 0, 1, -5", "0, 16.970563, 5")]
        public void PointFromPlanes2(string planeString1, string planeString2, string planeString3, string eps)
        {
            var plane1 = this.GetPlaneFrom4Doubles(planeString1);
            var plane2 = this.GetPlaneFrom4Doubles(planeString2);
            var plane3 = this.GetPlaneFrom4Doubles(planeString3);
            var points = new[]
            {
                Plane3D.PointFromPlanes(plane1, plane2, plane3),
                Plane3D.PointFromPlanes(plane2, plane1, plane3),
                Plane3D.PointFromPlanes(plane1, plane3, plane2),
                Plane3D.PointFromPlanes(plane2, plane3, plane1),
                Plane3D.PointFromPlanes(plane3, plane2, plane1),
                Plane3D.PointFromPlanes(plane3, plane1, plane2),
            };
            var expected = Point3D.Parse(eps);
            foreach (var point in points)
            {
                AssertGeometry.AreEqual(expected, point);
            }
        }

        [TestCase("0, 0, 0", "0, 0, 1", @"<Plane3D><RootPoint X=""0"" Y=""0"" Z=""0"" /><Normal X=""0"" Y=""0"" Z=""1"" /></Plane3D>")]
        public void XmlRoundTrips(string rootPoint, string unitVector, string xml)
        {
            var plane = new Plane3D(Point3D.Parse(rootPoint), UnitVector3D.Parse(unitVector));
            AssertXml.XmlRoundTrips(plane, xml, (e, a) => AssertGeometry.AreEqual(e, a));
        }

        private Plane3D GetPlaneFrom4Doubles(string inputstring)
        {
            var numbers = inputstring.Split(',').Select(t => double.Parse(t)).ToArray();
            return new Plane3D(numbers[0], numbers[1], numbers[2], numbers[3]);
        }
    }
}
