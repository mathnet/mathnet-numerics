// ReSharper disable InconsistentNaming

using System;
using MathNet.Numerics.Spatial.Euclidean3D;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial.Euclidean3D
{
    [TestFixture]
    public class Circle3DTests
    {
        [TestCase("0, 0, 0", 2.5)]
        [TestCase("2, -4, 0", 4.7)]
        public void CircleCenterRadius(string p1s, double radius)
        {
            var center = Point3D.Parse(p1s);
            var circle = new Circle3D(center, UnitVector3D.ZAxis, radius);
            Assert.AreEqual(2 * radius, circle.Diameter, double.Epsilon);
            Assert.AreEqual(2 * Math.PI * radius, circle.Circumference, double.Epsilon);
            Assert.AreEqual(Math.PI * radius * radius, circle.Area, double.Epsilon);
        }

        [TestCase("0,0,0", "5,0,0", "2.5,0,0", 2.5)]
        [TestCase("23.56,15.241,0", "62.15,-12.984,0", "42.8550,1.1285,0", 23.90522289)]
        public void Circle2Points(string p1s, string p2s, string centers, double radius)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            var circle3D = Circle3D.FromPointsAndAxis(p1, p2, UnitVector3D.ZAxis);
            AssertGeometry.AreEqual(circle3D.CenterPoint, Point3D.Parse(centers));
            Assert.AreEqual(circle3D.Radius, radius, 1e-6);
        }

        // Test cases randomly generated from GOM Inspect Professional v8
        [TestCase("-1,0,0", "0,1,0", "1,0,0", "0,0,0", 1)]
        [TestCase("3.5184432,0.3072766,1.5733767", "2.6311562,3.8537550,4.1648900", "1.7918408,3.3085796,0.1773063", "2.7223188,2.4699561,2.2155024", 2.3923465)]
        [TestCase("3.9940300,4.7087055,3.0356712", "1.7448842,3.5524049,2.9346235", "3.3923391,0.6820635,2.8792665", "3.6015409,2.7091562,2.9554706", 2.0392835)]
        [TestCase("1.1572695,1.7427232,3.8991802", "3.8499789,2.6866347,1.1215183", "4.4666442,3.1640346,2.6081508", "2.6534515,2.3079965,2.6873057", 2.0066725)]
        [TestCase("2.8983684,2.6350014,3.6266714", "3.9347082,1.2248094,3.9586336", "1.3540560,4.4935097,4.4614937", "2.5949041,2.2684548,7.7958547", 4.1962526)]
        [TestCase("3.2522525,3.4579030,2.5739476", "2.5922429,2.5575788,4.4536494", "0.9654999,3.9766348,1.5029361", "1.4925488,3.2060788,3.1067942", 1.8557743)]
        [TestCase("1.4907420,4.4495954,1.2951978", "2.3796522,3.6326081,0.4917802", "3.6332795,0.3779172,3.4856655", "2.3105721,2.5275616,2.8479118", 2.6033163)]
        [TestCase("4.0537333,0.7137006,1.8633552", "0.7591453,2.7025442,4.5177595", "2.4149884,2.4450949,0.7413665", "2.3306759,1.8283553,3.0064357", 2.3490455)]
        [TestCase("2.3488295,3.4356098,2.3024682", "2.0004679,2.0740716,4.8011107", "4.1253165,3.8784045,1.5045145", "4.3383477,2.6362129,3.7888115", 2.6089145)]
        [TestCase("2.7730017,4.7353519,1.6764519", "4.4640538,3.8917330,1.7939499", "2.9657457,0.7421344,0.7106418", "2.8705245,2.7387444,1.1937714", 2.0564369)]
        [TestCase("0.3309697,1.4510555,4.3876068", "3.3591227,3.2904666,1.6724443", "0.6636883,0.8606021,3.9895073", "1.7839226,2.6021881,3.1186384", 2.2464326)]
        public void CircleFromThreePoints(string p1s, string p2s, string p3s, string centers, double radius)
        {
            var p1 = Point3D.Parse(p1s);
            var p2 = Point3D.Parse(p2s);
            var p3 = Point3D.Parse(p3s);
            var center = Point3D.Parse(centers);

            var circle = Circle3D.FromPoints(p1, p2, p3);

            AssertGeometry.AreEqual(center, circle.CenterPoint);
            Assert.AreEqual(radius, circle.Radius, 1e-6);
        }

        [Test]
        public void CircleFromThreePointsArgumentException()
        {
            var p1 = new Point3D(0, 0, 0);
            var p2 = new Point3D(-1, 0, 0);
            var p3 = new Point3D(1, 0, 0);

            Assert.Throws<InvalidOperationException>(() => Circle3D.FromPoints(p1, p2, p3));
        }
    }
}
