using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Spatial.Euclidean2D;
using MathNet.Numerics.Spatial.Euclidean3D;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial
{
    public static class AssertGeometry
    {
        public static void AreEqual(CoordinateSystem3D coordinateSystem, Point3D origin, Vector3D xAxis, Vector3D yAxis, Vector3D zAxis, double tolerance = 1e-6)
        {
            AreEqual(xAxis, coordinateSystem.XAxis, tolerance);
            AreEqual(yAxis, coordinateSystem.YAxis, tolerance);
            AreEqual(zAxis, coordinateSystem.ZAxis, tolerance);
            AreEqual(origin, coordinateSystem.Origin, tolerance);

            AreEqual(new double[] { xAxis.X, xAxis.Y, xAxis.Z, 0 }, coordinateSystem.Column(0).ToArray(), tolerance);
            AreEqual(new double[] { yAxis.X, yAxis.Y, yAxis.Z, 0 }, coordinateSystem.Column(1).ToArray(), tolerance);
            AreEqual(new double[] { zAxis.X, zAxis.Y, zAxis.Z, 0 }, coordinateSystem.Column(2).ToArray(), tolerance);
            AreEqual(new double[] { origin.X, origin.Y, origin.Z, 1 }, coordinateSystem.Column(3).ToArray(), tolerance);
        }

        public static void AreEqual(UnitVector3D expected, UnitVector3D actual, double tolerance = 1e-6, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("Expected {0} but was {1}", expected, actual);
            }

            Assert.AreEqual(expected.X, actual.X, tolerance, message);
            Assert.AreEqual(expected.Y, actual.Y, tolerance, message);
            Assert.AreEqual(expected.Z, actual.Z, tolerance, message);
        }

        public static void AreEqual(Vector3D expected, Vector3D actual, double tolerance = 1e-6, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("Expected {0} but was {1}", expected, actual);
            }

            Assert.AreEqual(expected.X, actual.X, tolerance, message);
            Assert.AreEqual(expected.Y, actual.Y, tolerance, message);
            Assert.AreEqual(expected.Z, actual.Z, tolerance, message);
        }

        public static void AreEqual(UnitVector3D expected, Vector3D actual, double tolerance = 1e-6, string message = "")
        {
            AreEqual(expected.ToVector3D(), actual, tolerance, message);
        }

        public static void AreEqual(Vector3D expected, UnitVector3D actual, double tolerance = 1e-6, string message = "")
        {
            AreEqual(expected, actual.ToVector3D(), tolerance, message);
        }

        public static void AreEqual(Vector2D expected, Vector2D actual, double tolerance = 1e-6, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("Expected {0} but was {1}", expected, actual);
            }

            Assert.AreEqual(expected.X, actual.X, tolerance, message);
            Assert.AreEqual(expected.Y, actual.Y, tolerance, message);
        }

        public static void AreEqual(Point3D expected, Point3D actual, double tolerance = 1e-6, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("Expected {0} but was {1}", expected, actual);
            }

            Assert.AreEqual(expected.X, actual.X, tolerance, message);
            Assert.AreEqual(expected.Y, actual.Y, tolerance, message);
            Assert.AreEqual(expected.Z, actual.Z, tolerance, message);
        }

        public static void AreEqual(CoordinateSystem3D expected, CoordinateSystem3D actual, double tolerance = 1e-6, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("Expected {0} but was {1}", expected, actual);
            }

            if (expected.Values.Length != actual.Values.Length)
            {
                Assert.Fail();
            }

            for (var i = 0; i < expected.Values.Length; i++)
            {
                Assert.AreEqual(expected.Values[i], actual.Values[i], tolerance);
            }
        }

        public static void AreEqual(double[] expected, double[] actual, double tolerance = 1e-6, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("Expected {0} but was {1}", "{" + string.Join(",", expected) + "}", "{" + string.Join(",", actual) + "}");
            }

            if (expected.Length != actual.Length)
            {
                Assert.Fail();
            }

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], actual[i], tolerance);
            }
        }

        public static void AreEqual(Line3D expected, Line3D actual, double tolerance = 1e-6)
        {
            AreEqual(expected.StartPoint, actual.StartPoint, tolerance);
            AreEqual(expected.EndPoint, actual.EndPoint, tolerance);
        }

        public static void AreEqual(LineSegment3D expected, LineSegment3D actual, double tolerance = 1e-6)
        {
            AreEqual(expected.StartPoint, actual.StartPoint, tolerance);
            AreEqual(expected.EndPoint, actual.EndPoint, tolerance);
        }

        public static void AreEqual(Ray3D expected, Ray3D actual, double tolerance = 1e-6, string message = "")
        {
            AreEqual(expected.ThroughPoint, actual.ThroughPoint, tolerance, message);
            AreEqual(expected.Direction, actual.Direction, tolerance, message);
        }

        public static void AreEqual(Plane3D expected, Plane3D actual, double tolerance = 1e-6, string message = "")
        {
            AreEqual(expected.Normal, actual.Normal, tolerance, message);
            AreEqual(expected.RootPoint, actual.RootPoint, tolerance, message);
            Assert.AreEqual(expected.D, actual.D, tolerance, message);
        }

        public static void AreEqual(Matrix<double> expected, Matrix<double> actual, double tolerance = 1e-6)
        {
            Assert.AreEqual(expected.RowCount, actual.RowCount);
            Assert.AreEqual(expected.ColumnCount, actual.ColumnCount);
            var expectedRowWiseArray = expected.ToRowMajorArray();
            var actualRowWiseArray = actual.ToRowMajorArray();
            for (var i = 0; i < expectedRowWiseArray.Length; i++)
            {
                Assert.AreEqual(expectedRowWiseArray[i], actualRowWiseArray[i], tolerance);
            }
        }

        public static void AreEqual(Point2D expected, Point2D actual, double tolerance = 1e-6, string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Format("Expected {0} but was {1}", expected, actual);
            }

            Assert.AreEqual(expected.X, actual.X, tolerance, message);
            Assert.AreEqual(expected.Y, actual.Y, tolerance, message);
        }
    }
}
