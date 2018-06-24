using System;
using System.Linq;
using MathNet.Numerics.Spatial.Euclidean2D;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial.Euclidean2D
{
    [TestFixture]
    public class PolyLine2DTests
    {
        [TestCase("0,0;1,1;2,2;3,3", 1, "1,1")]
        [TestCase("0,0;1,1;2,2;3,3", 0, "0,0")]
        [TestCase("0,0;1,1;2,2;3,3", 3, "3,3")]
        public void IndexAccessorTest(string points, int index, string expected)
        {
            var testElement = new PolyLine2D(from x in points.Split(';') select Point2D.Parse(x));
            var checkElement = Point2D.Parse(expected);
            AssertGeometry.AreEqual(checkElement, testElement.Vertices.Skip(index).First());
        }

        [TestCase("0,0;0,1", 1.0)]
        [TestCase("0,0;0,1;1,1", 2.0)]
        [TestCase("0,-1.5;0,1;1,1", 3.5)]
        public void GetPolyLineLengthTests(string points, double expected)
        {
            var testElement = new PolyLine2D(from x in points.Split(';') select Point2D.Parse(x));

            Assert.AreEqual(expected, testElement.Length);
        }

        [TestCase("0,-1.5;0,1;1,1", 1.0, "1,1")]
        [TestCase("0,-1.5;0,1;1,1", 0.0, "0,-1.5")]
        [TestCase("0,0;0,1;1,1", 0.25, "0,0.5")]
        [TestCase("0,0;0,1;1,1", 0.5, "0,1")]
        [TestCase("0,0;0,1;1,1", 0.75, "0.5,1")]
        public void GetPointAtFractionAlongCurve(string points, double fraction, string expected)
        {
            // Note that this method also tests GetPointAtLengthFromStart(...)
            var testElement = new PolyLine2D(from x in points.Split(';') select Point2D.Parse(x));
            var checkElement = Point2D.Parse(expected);

            Assert.AreEqual(checkElement, testElement.GetPointAtFractionAlongCurve(fraction));
        }

        [TestCase("0,-1.5;0,1;1,1", 2.0, "1,1")]
        [TestCase("0,-1.5;0,1;1,1", -5,  "0,-1.5")]
        public void GetPointAtFractionAlongCurveThrowsArgumentException(string points, double fraction, string expected)
        {
            var testElement = new PolyLine2D(from x in points.Split(';') select Point2D.Parse(x));
            Assert.Throws<ArgumentException>(() => { testElement.GetPointAtFractionAlongCurve(fraction); });
        }

        [TestCase("0,0;0,1;1,1", "0,-1", "0,0")] // Off Endpoint
        [TestCase("0,0;0,1;1,1", "2,1", "1,1")] // Off Endpoint
        [TestCase("0,0;0,1;1,1", "-1,2", "0,1")] // Off Corner
        [TestCase("0,0;0,1;1,1", "0,0", "0,0")] // On Endpoint
        [TestCase("0,0;0,1;1,1", "1,1", "1,1")] // On Endpoint
        [TestCase("0,0;0,1;1,1", "0,1", "0,1")] // On Corner
        [TestCase("0,0;0,1;1,1", "0,0.5", "0,0.5")] // On Curve
        [TestCase("0,0;0,1;1,1", "-1,0.5", "0,0.5")] // Off curve
        [TestCase("0,0;0,1;1,1", "0.5,1", "0.5,1")] // On Curve
        [TestCase("0,0;0,1;1,1", "0.5,1.5", "0.5,1")] // Off curve
        public void ClosestPointToTest(string points, string testPoint, string expectedPoint)
        {
            var testCurve = new PolyLine2D(from x in points.Split(';') select Point2D.Parse(x));
            var test = Point2D.Parse(testPoint);
            var expected = Point2D.Parse(expectedPoint);

            Assert.AreEqual(expected, testCurve.ClosestPointTo(test));
        }
    }
}
