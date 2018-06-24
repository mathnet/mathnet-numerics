using System;
using MathNet.Numerics.Spatial;
using MathNet.Numerics.Spatial.Euclidean3D;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial.Euclidean3D
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class CoordinateSystem3DTests
    {
        private const string X = "1; 0 ; 0";
        private const string Y = "0; 1; 0";
        private const string Z = "0; 0; 1";
        private const string NegativeX = "-1; 0; 0";
        private const string NegativeY = "0; -1; 0";
        private const string NegativeZ = "0; 0; -1";

        [TestCase("1, 2, 3", "4, 5, 6", "7, 8, 9", "-1, -2, -3")]
        public void ConstructorTest(string ps, string xs, string ys, string zs)
        {
            var origin = Point3D.Parse(ps);
            var xAxis = Vector3D.Parse(xs);
            var yAxis = Vector3D.Parse(ys);
            var zAxis = Vector3D.Parse(zs);
            var css = new[]
            {
                new CoordinateSystem3D(origin, xAxis, yAxis, zAxis),
                new CoordinateSystem3D(xAxis, yAxis, zAxis, origin)
            };
            foreach (var cs in css)
            {
                AssertGeometry.AreEqual(origin, cs.Origin);
                AssertGeometry.AreEqual(xAxis, cs.XAxis);
                AssertGeometry.AreEqual(yAxis, cs.YAxis);
                AssertGeometry.AreEqual(zAxis, cs.ZAxis);
            }
        }

        [TestCase("o:{1, 2e-6, -3} x:{1, 2, 3} y:{3, 3, 3} z:{4, 4, 4}", "1, 2e-6, -3", "1, 2, 3", "3, 3, 3", "4, 4, 4")]
        public void ParseTests(string s, string ops, string xs, string ys, string zs)
        {
            var cs = CoordinateSystem3D.Parse(s);
            AssertGeometry.AreEqual(Point3D.Parse(ops), cs.Origin);
            AssertGeometry.AreEqual(Vector3D.Parse(xs), cs.XAxis);
            AssertGeometry.AreEqual(Vector3D.Parse(ys), cs.YAxis);
            AssertGeometry.AreEqual(Vector3D.Parse(zs), cs.ZAxis);
        }

        [TestCase("1, 2, 3", "90°", "0, 0, 1", "-2, 1, 3")]
        [TestCase("1, 2, 3", "90°", "0, 0, -1", "2, -1, 3")]
        [TestCase("1, 2, 3", "-90°", "0, 0, 1", "2, -1, 3")]
        [TestCase("1, 2, 3", "180°", "0, 0, 1", "-1, -2, 3")]
        [TestCase("1, 2, 3", "270°", "0, 0, 1", "2, -1, 3")]
        [TestCase("1, 2, 3", "90°", "1, 0, 0", "1, -3, 2")]
        [TestCase("1, 2, 3", "-90°", "1, 0, 0", "1, 3, -2")]
        [TestCase("1, 2, 3", "90°", "-1, 0, 0", "1, 3, -2")]
        [TestCase("1, 2, 3", "90°", "0, 1, 0", "3, 2, -1")]
        [TestCase("1, 2, 3", "-90°", "0, 1, 0", "-3, 2, 1")]
        public void RotationAroundVector(string ps, string @as, string vs, string eps)
        {
            var p = Point3D.Parse(ps);
            var angle = Angle.Parse(@as);
            var coordinateSystems = new[]
                {
                    CoordinateSystem3D.Rotation(angle, UnitVector3D.Parse(vs)),
                    CoordinateSystem3D.Rotation(angle, Vector3D.Parse(vs)),
                };
            var expected = Point3D.Parse(eps);
            foreach (var coordinateSystem in coordinateSystems)
            {
                var rotatedPoint = coordinateSystem.Transform(p);
                AssertGeometry.AreEqual(expected, rotatedPoint);
            }
        }

        [TestCase("0°", "0°", "0°", "1, 2, 3", "1, 2, 3")]
        [TestCase("90°", "0°", "0°", "1, 2, 3", "-2, 1, 3")]
        [TestCase("-90°", "0°", "0°", "1, 2, 3", "2, -1, 3")]
        [TestCase("0°", "90°", "0°", "1, 2, 3", "3, 2, -1")]
        [TestCase("0°", "-90°", "0°", "1, 2, 3", "-3, 2, 1")]
        [TestCase("0°", "0°", "90°", "1, 2, 3", "1, -3, 2")]
        [TestCase("0°", "0°", "-90°", "1, 2, 3", "1, 3, -2")]
        public void RotationYawPitchRoll(string yaws, string pitchs, string rolls, string ps, string eps)
        {
            var p = Point3D.Parse(ps);
            var yaw = Angle.Parse(yaws);
            var pitch = Angle.Parse(pitchs);
            var roll = Angle.Parse(rolls);
            var coordinateSystems = new[]
                {
                    CoordinateSystem3D.Rotation(yaw, pitch, roll),
                };
            var expected = Point3D.Parse(eps);
            foreach (var coordinateSystem in coordinateSystems)
            {
                var rotatedPoint = coordinateSystem.Transform(p);
                AssertGeometry.AreEqual(expected, rotatedPoint);
            }
        }

        [TestCase("1, 2, 3", "0, 0, 1", "1, 2, 4")]
        [TestCase("1, 2, 3", "0, 0, -1", "1, 2, 2")]
        [TestCase("1, 2, 3", "0, 0, 0", "1, 2, 3")]
        [TestCase("1, 2, 3", "0, 1, 0", "1, 3, 3")]
        [TestCase("1, 2, 3", "0, -1, 0", "1, 1, 3")]
        [TestCase("1, 2, 3", "1, 0, 0", "2, 2, 3")]
        [TestCase("1, 2, 3", "-1, 0, 0", "0, 2, 3")]
        public void Translation(string ps, string vs, string eps)
        {
            var p = Point3D.Parse(ps);
            var cs = CoordinateSystem3D.Translation(Vector3D.Parse(vs));
            var tp = cs.Transform(p);
            Console.WriteLine(cs.ToString());
            AssertGeometry.AreEqual(Point3D.Parse(eps), tp);
        }

        [TestCase(X, X, null)]
        [TestCase(X, X, X)]
        [TestCase(X, X, Y)]
        [TestCase(X, X, Z)]
        [TestCase(X, NegativeX, null)]
        [TestCase(X, NegativeX, Z)]
        [TestCase(X, NegativeX, Y)]
        [TestCase(X, Y, null)]
        [TestCase(X, Z, null)]
        [TestCase(Y, Y, null)]
        [TestCase(Y, Y, X)]
        [TestCase(Y, NegativeY, null)]
        [TestCase(Y, NegativeY, X)]
        [TestCase(Y, NegativeY, Z)]
        [TestCase(Z, NegativeZ, null)]
        [TestCase(Z, NegativeZ, X)]
        [TestCase(Z, NegativeZ, Y)]
        [TestCase("1, 2, 3", "-1, 0, -1", null)]
        public void RotateToTest(string v1s, string v2s, string @as)
        {
            var axis = string.IsNullOrEmpty(@as) ? (UnitVector3D?)null : Vector3D.Parse(@as).Normalize();
            var v1 = Vector3D.Parse(v1s).Normalize();
            var v2 = Vector3D.Parse(v2s).Normalize();
            var actual = CoordinateSystem3D.RotateTo(v1, v2, axis);
            Console.WriteLine(actual);
            var rv = actual.Transform(v1);
            AssertGeometry.AreEqual(v2, rv);
            actual = CoordinateSystem3D.RotateTo(v2, v1, axis);
            rv = actual.Transform(v2);
            AssertGeometry.AreEqual(v1, rv);
        }

        [Test]
        public void InvertTest()
        {
            Assert.Inconclusive("Test this?");
        }

        [Test]
        public void EqualityNullOperator()
        {
            string test = "o:{1, 2e-6, -3} x:{1, 2, 3} y:{3, 3, 3} z:{4, 4, 4}";
            var cs = CoordinateSystem3D.Parse(test);

            Assert.IsFalse(cs == null);
        }

        [Test]
        public void EqualityNullOperatorTrue()
        {
            CoordinateSystem3D cs = null;

            Assert.IsTrue(cs == null);
        }

        [Test]
        public void EqualityNotNullOperator()
        {
            string test = "o:{1, 2e-6, -3} x:{1, 2, 3} y:{3, 3, 3} z:{4, 4, 4}";
            var cs = CoordinateSystem3D.Parse(test);

            Assert.IsTrue(cs != null);
        }

        [Test]
        public void EqualityNotNullOperatorFalse()
        {
            CoordinateSystem3D cs = null;

            Assert.IsFalse(cs != null);
        }

        [Test]
        public void EqualityNull()
        {
            string test = "o:{1, 2e-6, -3} x:{1, 2, 3} y:{3, 3, 3} z:{4, 4, 4}";
            var cs = CoordinateSystem3D.Parse(test);

            Assert.IsFalse(cs.Equals(null));
        }

        [TestCase("1; -5; 3", "1; -5; 3", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        public void TransformPoint(string ps, string eps, string css)
        {
            var p = Point3D.Parse(ps);
            var cs = CoordinateSystem3D.Parse(css);
            var actual = p.TransformBy(cs);
            var expected = Point3D.Parse(eps);
            AssertGeometry.AreEqual(expected, actual, float.Epsilon);
        }

        [TestCase("1; 2; 3", "1; 2; 3", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        [TestCase("1; 2; 3", "1; 2; 3", "o:{3, 4, 5} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        public void TransformVector(string vs, string evs, string css)
        {
            var v = Vector3D.Parse(vs);
            var cs = CoordinateSystem3D.Parse(css);
            var actual = cs.Transform(v);
            var expected = Vector3D.Parse(evs);
            AssertGeometry.AreEqual(expected, actual);
        }

        [Test]
        public void TransformUnitVector()
        {
            var cs = CoordinateSystem3D.Rotation(Angle.FromDegrees(90), UnitVector3D.ZAxis);
            var uv = UnitVector3D.XAxis;
            var actual = cs.Transform(uv);
            AssertGeometry.AreEqual(UnitVector3D.YAxis, actual);
        }

        [TestCase("o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        [TestCase("o:{0, 0, 0} x:{10, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{1, 0, 0} x:{0.1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        [TestCase("o:{0, 0, 0} x:{10, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{1, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        [TestCase("o:{1, 2, -7} x:{10, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        [TestCase("o:{1, 2, -7} x:{10, 0.1, 0} y:{0, 1.2, 0.1} z:{0.1, 0, 1}", "o:{2, 5, 1} x:{0.1, 2, 0} y:{0.2, -1, 0} z:{0, 0.4, 1}")]
        public void SetToAlignCoordinateSystemsTest(string fcss, string tcss)
        {
            var fcs = CoordinateSystem3D.Parse(fcss);
            var tcs = CoordinateSystem3D.Parse(tcss);

            var css = new[]
            {
                CoordinateSystem3D.SetToAlignCoordinateSystems(fcs.Origin, fcs.XAxis, fcs.YAxis, fcs.ZAxis, tcs.Origin, tcs.XAxis, tcs.YAxis, tcs.ZAxis),
                CoordinateSystem3D.CreateMappingCoordinateSystem(fcs, tcs)
            };
            foreach (var cs in css)
            {
                var aligned = cs.Transform(fcs);
                AssertGeometry.AreEqual(tcs.Origin, aligned.Origin);

                AssertGeometry.AreEqual(tcs.XAxis, aligned.XAxis);

                AssertGeometry.AreEqual(tcs.YAxis, aligned.YAxis);

                AssertGeometry.AreEqual(tcs.ZAxis, aligned.ZAxis);
            }
        }

        [TestCase(X, Y, Z)]
        [TestCase(NegativeX, Y, Z)]
        [TestCase(NegativeX, Y, null)]
        [TestCase(X, Y, null)]
        [TestCase(X, Y, "0,0,1")]
        [TestCase("1,-1, 1", "0, 1, 1", null)]
        [TestCase(X, Z, Y)]
        public void SetToRotateToTest(string vs, string vts, string axisString)
        {
            var v = UnitVector3D.Parse(vs, tolerance: 1);
            var vt = UnitVector3D.Parse(vts, tolerance: 1);
            UnitVector3D? axis = null;
            if (axisString != null)
            {
                axis = UnitVector3D.Parse(axisString);
            }

            var cs = CoordinateSystem3D.RotateTo(v, vt, axis);
            var rv = cs.Transform(v);
            AssertGeometry.AreEqual(vt, rv);

            var invert = cs.Invert();
            var rotateBack = invert.Transform(rv);
            AssertGeometry.AreEqual(v, rotateBack);

            cs = CoordinateSystem3D.RotateTo(vt, v, axis);
            rotateBack = cs.Transform(rv);
            AssertGeometry.AreEqual(v, rotateBack);
        }

        [TestCase("o:{1, 2, -7} x:{10, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        public void Transform(string cs1s, string cs2s)
        {
            var cs1 = CoordinateSystem3D.Parse(cs1s);
            var cs2 = CoordinateSystem3D.Parse(cs2s);
            var actual = cs1.Transform(cs2);
            var expected = new CoordinateSystem3D(cs1.Multiply(cs2));
            AssertGeometry.AreEqual(expected, actual);
        }

        [Test]
        public void XmlRoundTrips()
        {
            var cs = new CoordinateSystem3D(new Point3D(1, -2, 3), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1), new Vector3D(1, 0, 0));
            string expected = @"
<CoordinateSystem3D>
    <Origin X=""1"" Y=""-2"" Z=""3"" />
    <XAxis X=""0"" Y=""1"" Z=""0"" />
    <YAxis X=""0"" Y=""0"" Z=""1"" />
    <ZAxis X=""1"" Y=""0"" Z=""0"" />
</CoordinateSystem3D>";
            AssertXml.XmlRoundTrips(cs, expected, (e, a) => AssertGeometry.AreEqual(e, a));
        }
    }
}
