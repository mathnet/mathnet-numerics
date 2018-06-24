using System;
using System.Linq;
using MathNet.Numerics.Spatial.Euclidean3D;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial.Euclidean3D
{
    [TestFixture]
    public class PolyLine3DTests
    {
        [TestCase("0,0,1;1,1,0;2,2,1;3,3,0", 1, "1,1,0")]
        [TestCase("0,0,1;1,1,0;2,2,1;3,3,0", 0, "0,0,1")]
        [TestCase("0,0,1;1,1,0;2,2,1;3,3,0", 3, "3,3,0")]
        public void IndexAccessorTest(string points, int index, string expected)
        {
            var testElement = new PolyLine3D(from x in points.Split(';') select Point3D.Parse(x));
            var checkElement = Point3D.Parse(expected);
            AssertGeometry.AreEqual(checkElement, testElement.Vertices.Skip(index).First());
        }

        [TestCase("0,0,0;0,1,0", 1.0)]
        [TestCase("0,0,0;0,1,0;1,1,0", 2.0)]
        [TestCase("0,-1.5,0;0,1,0;1,1,0", 3.5)]
        public void GetPolyLineLengthTests(string points, double expected)
        {
            var testElement = new PolyLine3D(from x in points.Split(';') select Point3D.Parse(x));

            Assert.AreEqual(expected, testElement.Length, 1e-10);
        }

        [TestCase("0,-1.5,0;0,1,0;1,1,0", 1.0, "1,1,0")]
        [TestCase("0,-1.5,0;0,1,0;1,1,0", 0.0, "0,-1.5,0")]
        [TestCase("0,0,0;0,1,0;1,1,0", 0.25, "0,0.5,0")]
        [TestCase("0,0,0;0,1,0;1,1,0", 0.5, "0,1,0")]
        [TestCase("0,0,0;0,1,0;1,1,0", 0.75, "0.5,1,0")]
        public void GetPointAtFractionAlongCurve(string points, double fraction, string expected)
        {
            // Note that this method also tests GetPointAtLengthFromStart(...)
            var testElement = new PolyLine3D(from x in points.Split(';') select Point3D.Parse(x));
            var checkElement = Point3D.Parse(expected);

            AssertGeometry.AreEqual(checkElement, testElement.GetPointAtFractionAlongCurve(fraction));
        }

        [TestCase("0,-1.5,0;0,1,0;1,1,0", 2.0, "1,1,0")]
        [TestCase("0,-1.5,0;0,1,0;1,1,0", -5, "0,-1.5,0")]
        public void GetPointAtFractionAlongCurveThrowsArgumentException(string points, double fraction, string expected)
        {
            var testElement = new PolyLine3D(from x in points.Split(';') select Point3D.Parse(x));
            Assert.Throws<ArgumentException>(() => { testElement.GetPointAtFractionAlongCurve(fraction); });
        }

        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "0,-1,0", "0,0,0")] // Off Endpoint
        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "2,1,2", "1,1,2")] // Off Endpoint
        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "-1,2,1", "0,1,1")] // Off Corner
        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "0,0,0", "0,0,0")] // On Endpoint
        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "1,1,2", "1,1,2")] // On Endpoint
        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "0,1,1", "0,1,1")] // On Corner
        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "0,0.5,0.5", "0,0.5,0.5")] // On Curve
        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "-1,0.5,0.5", "0,0.5,0.5")] // Off curve
        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "0.5,1,1.5", "0.5,1,1.5")] // On Curve
        [TestCase("0,0,0 ; 0,1,1 ; 1,1,2", "0.5,1.5,1.5", "0.5,1,1.5")] // Off curve
        public void ClosestPointToTest(string points, string testPoint, string expectedPoint)
        {
            var testCurve = new PolyLine3D(from x in points.Split(';') select Point3D.Parse(x));
            var test = Point3D.Parse(testPoint);
            var expected = Point3D.Parse(expectedPoint);

            AssertGeometry.AreEqual(expected, testCurve.ClosestPointTo(test), 1e-06);
        }
    }
}
