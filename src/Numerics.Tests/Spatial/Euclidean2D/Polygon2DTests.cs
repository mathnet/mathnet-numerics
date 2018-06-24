using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Spatial;
using MathNet.Numerics.Spatial.Euclidean2D;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial.Euclidean2D
{
    [TestFixture]
    public class Polygon2DTests
    {
        [Test]
        public void ConstructorTest()
        {
            var polygon = TestPolygon1();
            var checkList = new List<Point2D> { new Point2D(0, 0), new Point2D(0.25, 0.5), new Point2D(1, 1), new Point2D(-1, 1), new Point2D(0.5, -0.5) };
            CollectionAssert.AreEqual(checkList, polygon.Vertices);
        }

        [TestCase("0,0;2,2;3,1;2,0", "1,1", "1,1;3,3;4,2;3,1")]
        [TestCase("0,0;2,2;3,1;2,0", "-1,-1", "-1,-1;1,1;2,0;1,-1")]
        public void TranslatePolygon(string points, string vectorString, string expectedPolygon)
        {
            var testElement = new Polygon2D(from x in points.Split(';') select Point2D.Parse(x));
            var expected = new Polygon2D(from x in expectedPolygon.Split(';') select Point2D.Parse(x));
            Vector2D vector = Vector2D.Parse(vectorString);
            var result = testElement.TranslateBy(vector);
            Assert.AreEqual(expected, result);
        }

        [TestCase("0,0;1,2;-1,2", System.Math.PI, "0,0;-1,-2;1,-2")]
        public void RotatePolygon(string points, double angle, string expectedPolygon)
        {
            var testElement = new Polygon2D(from x in points.Split(';') select Point2D.Parse(x));
            var expected = new Polygon2D(from x in expectedPolygon.Split(';') select Point2D.Parse(x));
            Angle a = Angle.FromRadians(angle);
            var result = testElement.Rotate(a);
            Assert.IsTrue(expected.Equals(result, 0.001));
        }

        [TestCase("0,0;1,2;-1,2", "0,0:1,2;1,2:-1,2;-1,2:0,0")]
        public void PolygonEdges(string stringpoints, string lines)
        {
            List<Point2D> points = (from x in stringpoints.Split(';') select Point2D.Parse(x)).ToList();
            var lineset = lines.Split(';').Select(t => LineSegment2D.Parse(t.Split(':').First(), t.Split(':').Last())).ToList();

            var poly = new Polygon2D(points);
            CollectionAssert.AreEquivalent(lineset, poly.Edges);
        }

        [Test]
        public void ConstructorTest_ClipsStartOnDuplicate()
        {
            // Test to make sure that if the constructor point list is given to the polygon constructor with the first and last points
            // being duplicates, the point at the beginning of the list is removed
            var polygon = TestPolygon2();
            var checkList = new List<Point2D> { new Point2D(0.25, 0.5), new Point2D(1, 1), new Point2D(-1, 1), new Point2D(0.5, -0.5), new Point2D(0, 0) };
            CollectionAssert.AreEqual(checkList, polygon.Vertices);
        }

        [TestCase(0.5, 0, true)]
        [TestCase(0.35, 0, true)]
        [TestCase(0.5, 0.5, true)]
        [TestCase(0.75, 0.1, false)]
        [TestCase(0.75, -0.1, true)]
        [TestCase(0.5, -0.5, false)]
        [TestCase(0.25, 0.5, false)]
        [TestCase(0.25, -0.5, false)]
        [TestCase(0.0, 0, false)]
        [TestCase(1.5, 0, false)]
        public void IsPointInPolygonTest1(double x, double y, bool outcome)
        {
            var testPoint = new Point2D(x, y);
            var testPoly = TestPolygon3();

            Assert.AreEqual(outcome, testPoly.EnclosesPoint(testPoint));
        }

        [TestCase(0.5, 0, true)]
        [TestCase(0.35, 0, true)]
        [TestCase(0.5, 0.5, true)]
        [TestCase(0.75, 0.1, false)]
        [TestCase(0.75, -0.1, true)]
        [TestCase(0.5, -0.5, false)]
        [TestCase(0.25, 0.5, false)]
        [TestCase(0.25, -0.5, false)]
        [TestCase(0.0, 0, false)]
        [TestCase(1.5, 0, false)]
        public void IsPointInPolygonTest2(double x, double y, bool outcome)
        {
            var testPoint = new Point2D(x, y);
            var testPoly = TestPolygon4();

            Assert.AreEqual(outcome, testPoly.EnclosesPoint(testPoint));
        }

        // These test cases were generated using scipy.spatial's ConvexHull method
        [TestCase("0.27,0.41;0.87,0.67;0.7,0.33;0.5,0.61;0.04,0.23;0.73,0.14;0.84,0.02;0.25,0.23;0.12,0.2;0.37,0.78", "0.87,0.67;0.37,0.78;0.04,0.23;0.12,0.2;0.84,0.02")]
        [TestCase("0.81,0.25;0.77,0.15;0.17,0.48;0.4,0.58;0.29,0.92;0.37,0.26;0.7,0.91;0.04,0.1;0.39,0.73;0.7,0.12", "0.7,0.91;0.29,0.92;0.04,0.1;0.7,0.12;0.77,0.15;0.81,0.25")]
        [TestCase("0.87,0.39;0.83,0.42;0.75,0.62;0.91,0.49;0.18,0.63;0.17,0.95;0.22,0.5;0.93,0.41;0.66,0.79;0.32,0.42", "0.66,0.79;0.17,0.95;0.18,0.63;0.22,0.5;0.32,0.42;0.87,0.39;0.93,0.41;0.91,0.49")]
        [TestCase("0.18,0.39;0.91,0.3;0.35,0.53;0.91,0.38;0.49,0.28;0.61,0.22;0.27,0.18;0.44,0.06;0.5,0.79;0.78,0.22", "0.91,0.38;0.5,0.79;0.18,0.39;0.27,0.18;0.44,0.06;0.78,0.22;0.91,0.3")]
        [TestCase("0.89,0.55;0.98,0.24;0.03,0.2;0.51,0.99;0.72,0.32;0.56,0.87;0.1,0.75;0.64,0.16;0.82,0.73;0.17,0.46", "0.89,0.55;0.82,0.73;0.51,0.99;0.1,0.75;0.03,0.2;0.64,0.16;0.98,0.24")]
        [TestCase("-201.573,100.940;197.083,21.031;161.021,-29.414;114.223,-23.998;230.290,-68.246;-32.272,182.239;-173.345,72.736;-175.435,-176.273;90.810,-97.350;-196.942,216.594;67.759,-162.464;67.454,-174.844;-89.116,171.982;-18.421,11.935;73.816,-180.169;-103.560,-36.297;-233.800,194.296;-64.463,166.811;-17.182,83.403;-72.010,219.944", "-72.01,219.944;-196.942,216.594;-233.8,194.296;-175.435,-176.273;73.816,-180.169;230.29,-68.246;197.083,21.031")]
        public void ConvexHullTest(string points, string expected)
        {
            var testPoints = (from x in points.Split(';') select Point2D.Parse(x)).ToList();
            var expectedPoints = (from x in expected.Split(';') select Point2D.Parse(x)).ToList();

            var hullClockwise = Polygon2D.GetConvexHullFromPoints(testPoints, true);

            var clockwiseVertices = hullClockwise.Vertices;
            CollectionAssert.AreEqual(expectedPoints, clockwiseVertices);
            /*
            for (var i = 0; i < hullClockwise.VertexCount; i++)
            {
                Assert.That(ClockwiseVerticies[i], Is.EqualTo(expectedPoints[i]));
            }
            */

            var hullCounterClockwise = Polygon2D.GetConvexHullFromPoints(testPoints, false);
            var counterClockwiseVertices = hullCounterClockwise.Vertices;
            expectedPoints.Reverse();
            CollectionAssert.AreEqual(expectedPoints, counterClockwiseVertices);
            /*
            for (var i = 0; i < hullCounterClockwise.VertexCount; i++)
            {
                Assert.That(counterClockwiseVerticies[i], Is.EqualTo(expectedPoints[hullCounterClockwise.VertexCount - 1 - i]));
            }
            */

            var pointsNotOnConvexHull = testPoints.Except(hullCounterClockwise.Vertices);
            foreach (var pointNotOnConvexHull in pointsNotOnConvexHull)
            {
                var pointIsInsideConvexHull = hullCounterClockwise.EnclosesPoint(pointNotOnConvexHull);
                Assert.That(pointIsInsideConvexHull);
            }

            // second check: if we remove any point from the convex hull and build a new convex hull
            // then that point should be outside the new convex hull; if it's inside then our new
            // convex hull is the actual convex hull, which means the original one wasn't!
            foreach (var pointToRemove in counterClockwiseVertices)
            {
                var convexHullWithPointRemoved = new Polygon2D(hullCounterClockwise.Vertices.Except(new[] { pointToRemove }));
                var pointIsInsideConvexHull =
                    convexHullWithPointRemoved.EnclosesPoint(pointToRemove);
                Assert.That(pointIsInsideConvexHull, Is.Not.True);
            }
        }

        [TestCase("0,0;0.4,0;0.5,0;0.6,0;1,0;1,.25;1,.75;1,1;0,1;0,0.5", "1,0;1,1;0,1;0,0")]
        public void ReduceComplexity(string points, string reduced)
        {
            var testPoints = from x in points.Split(';') select Point2D.Parse(x);
            var expectedPoints = from x in reduced.Split(';') select Point2D.Parse(x);
            var poly = new Polygon2D(testPoints);
            var expected = new Polygon2D(expectedPoints);
            var thinned = poly.ReduceComplexity(0.00001);

            CollectionAssert.AreEqual(expected.Vertices, thinned.Vertices);
        }

        private static Polygon2D TestPolygon1()
        {
            var points = from x in new[] { "0,0", "0.25,0.5", "1,1", "-1,1", "0.5,-0.5" } select Point2D.Parse(x);
            return new Polygon2D(points);
        }

        private static Polygon2D TestPolygon2()
        {
            var points = from x in new[] { "0,0", "0.25,0.5", "1,1", "-1,1", "0.5,-0.5", "0,0" } select Point2D.Parse(x);
            return new Polygon2D(points);
        }

        private static Polygon2D TestPolygon3()
        {
            var points = from x in new[] { "0.25,0", "0.5,1", "1,-1" } select Point2D.Parse(x);
            return new Polygon2D(points);
        }

        private static Polygon2D TestPolygon4()
        {
            var points = from x in new[] { "0.5,1", "1,-1", "0.25,0" } select Point2D.Parse(x);
            return new Polygon2D(points);
        }
    }
}
