namespace MathNet.GeometryUnitTests
{
    using System;
    using Geometry;
    using Geometry.Units;
    using NUnit.Framework;

    [TestFixture]
    public class CoordinateSystemTest
    {
        const string X = "1; 0 ; 0";
        const string Y = "0; 1; 0";
        const string Z = "0; 0; 1";
        const string NegativeX = "-1; 0; 0";
        const string NegativeY = "0; -1; 0";
        const string NegativeZ = "0; 0; -1";
        private const string ZeroPoint = "0; 0; 0";

        [TestCase("1, 2, 3", "4, 5, 6", "7, 8, 9", "-1, -2, -3")]
        public void ConstructorTest(string ps, string xs, string ys, string zs)
        {
            var origin = Point3D.Parse(ps);
            var xAxis = Vector3D.Parse(xs);
            var yAxis = Vector3D.Parse(ys);
            var zAxis = Vector3D.Parse(zs);
            var css = new[]
            {
                new CoordinateSystem(origin, xAxis, yAxis, zAxis),
                new CoordinateSystem(xAxis, yAxis, zAxis, origin)
            };
            foreach (var cs in css)
            {
                AssertGemoetry.AreEqual(origin, cs.Origin);
                AssertGemoetry.AreEqual(xAxis, cs.XAxis);
                AssertGemoetry.AreEqual(yAxis, cs.YAxis);
                AssertGemoetry.AreEqual(zAxis, cs.ZAxis);
            }
        }

        [TestCase("o:{1, 2e-6, -3} x:{1, 2, 3} y:{3, 3, 3} z:{4, 4, 4}", new double[] { 1, 2e-6, -3 }, new double[] { 1, 2, 3 }, new double[] { 3, 3, 3 }, new double[] { 4, 4, 4 })]
        public void ParseTests(string s, double[] ops, double[] xs, double[] ys, double[] zs)
        {
            var cs = CoordinateSystem.Parse(s);
            AssertGemoetry.AreEqual(new Point3D(ops), cs.Origin);
            AssertGemoetry.AreEqual(new Vector3D(xs), cs.XAxis);
            AssertGemoetry.AreEqual(new Vector3D(ys), cs.YAxis);
            AssertGemoetry.AreEqual(new Vector3D(zs), cs.ZAxis);
        }


        [TestCase("2, 0, 0", 90, "0, 0, 1", "0, 2, 0")]
        [TestCase("2, 0, 0", -90, "0, 0, 1", "0, -2, 0")]
        [TestCase("2, 0, 0", 180, "0, 0, 1", "-2, 0, 0")]
        [TestCase("2, 0, 0", 270, "0, 0, 1", "0, -2, 0")]
        [TestCase("2, 0, 0", 90, "1, 0, 0", "2, 0, 0")]
        [TestCase("0, 2, 0", 90, "1, 0, 0", "0, 0, 2")]
        [TestCase("0, 2, 0", -90, "1, 0, 0", "0, 0, -2")]
        [TestCase("2, 0, 0", 90, "0, 1, 0", "0, 0, -2")]
        [TestCase("2, 0, 0", -90, "0, 1, 0", "0, 0, 2")]
        public void RotationTests(string ps, double a, string uvs, string evs)
        {
            var p = Point3D.Parse(ps);
            var rcs = CoordinateSystem.Rotation(a, AngleUnit.Degrees, UnitVector3D.Parse(uvs));
            var rp = rcs.Transform(p);
            Console.WriteLine(rcs.ToString());
            AssertGemoetry.AreEqual(Point3D.Parse(evs), rp);
        }

        [TestCase("2, 0, 0", "0, 0, 1", "2, 0, 1")]
        [TestCase("2, 0, 0", "0, 0, -1", "2, 0, -1")]
        [TestCase("2, 0, 0", "0, 0, 0", "2, 0, 0")]
        [TestCase("2, 0, 0", "0, 1, 0", "2, 1, 0")]
        [TestCase("2, 0, 0", "0, -1, 0", "2, -1, 0")]
        [TestCase("2, 0, 0", "1, 0, 0", "3, 0, 0")]
        [TestCase("2, 0, 0", "-1, 0, 0", "1, 0, 0")]
        public void Translation(string ps, string vs, string eps)
        {
            var p = Point3D.Parse(ps);
            var cs = CoordinateSystem.Translation(Vector3D.Parse(vs));
            var tp = cs.Transform(p);
            Console.WriteLine(cs.ToString());
            AssertGemoetry.AreEqual(Point3D.Parse(eps), tp);
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
            UnitVector3D? axis = string.IsNullOrEmpty(@as) ? (UnitVector3D?)null : UnitVector3D.Parse(@as);
            UnitVector3D v1 = UnitVector3D.Parse(v1s);
            UnitVector3D v2 = UnitVector3D.Parse(v2s);
            CoordinateSystem actual = CoordinateSystem.RotateTo(v1, v2, axis);
            Console.WriteLine(actual);
            var rv = actual.Transform(v1);
            AssertGemoetry.AreEqual(v2, rv);
            actual = CoordinateSystem.RotateTo(v2, v1, axis);
            rv = actual.Transform(v2);
            AssertGemoetry.AreEqual(v1, rv);
        }

        [TestCase(0, 0, 0, "1, 1, 1", "1, 1, 1", TestName = "No rotation")]
        [TestCase(90, 0, 0, "2, 0, 0", "0, 2, 0", TestName = "yaw = +90")]
        [TestCase(-90, 0, 0, "1, 0, 0", "0, -1, 0", TestName = "yaw = -90")]
        [TestCase(0, 90, 0, "1, 0, 0", "0, 0, -1", TestName = "pitch = +90")]
        [TestCase(0, -90, 0, "1, 0, 0", "0, 0, 1", TestName = "pitch = -90")]
        [TestCase(0, 0, 90, "1, 0, 0", "1, 0, 0", TestName = "roll = +90")]
        [TestCase(0, 0, 90, "0, 1, 0", "0, 0, 1", TestName = "roll = +90")]
        [TestCase(0, 0, -90, "1, 0, 0", "1, 0, 0", TestName = "roll = +90")]
        public void RotationYawPitchRoll(double yaw, double pitch, double roll, string vs, string evs)
        {
            var cs = CoordinateSystem.Rotation(yaw, pitch, roll, AngleUnit.Degrees);
            var v = Vector3D.Parse(vs);
            var actual = cs.Transform(v);
            var expected = Vector3D.Parse(evs);
            AssertGemoetry.AreEqual(expected, actual);
        }

        [Test]
        public void InvertTest()
        {
            Assert.Inconclusive("Test this?");
        }

        [TestCase("1; -5; 3", "1; -5; 3", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        public void TransformPoint(string ps, string eps, string css)
        {
            var p = Point3D.Parse(ps);
            CoordinateSystem cs = CoordinateSystem.Parse(css);
            Point3D actual = p.TransformBy(cs);
            var expected = Point3D.Parse(eps);
            AssertGemoetry.AreEqual(expected, actual, float.Epsilon);
        }

        [TestCase("1; -5; 3", "1; -5; 3", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        public void TransformVector(string vs, string evs, string css)
        {
            var v = Vector3D.Parse(vs);
            CoordinateSystem cs = CoordinateSystem.Parse(css);
            Vector3D actual = cs.Transform(v);
            var expected = Vector3D.Parse(evs);
            AssertGemoetry.AreEqual(expected, actual);
        }

        [TestCase("o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        [TestCase("o:{0, 0, 0} x:{10, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{1, 0, 0} x:{0.1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        [TestCase("o:{0, 0, 0} x:{10, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{1, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        [TestCase("o:{1, 2, -7} x:{10, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        [TestCase("o:{1, 2, -7} x:{10, 0.1, 0} y:{0, 1.2, 0.1} z:{0.1, 0, 1}", "o:{2, 5, 1} x:{0.1, 2, 0} y:{0.2, -1, 0} z:{0, 0.4, 1}")]
        public void SetToAlignCoordinateSystemsTest(string fcss, string tcss)
        {
            var fcs = CoordinateSystem.Parse(fcss);
            var tcs = CoordinateSystem.Parse(tcss);

            var css = new[]
            {
                CoordinateSystem.SetToAlignCoordinateSystems(fcs.Origin, fcs.XAxis, fcs.YAxis, fcs.ZAxis, tcs.Origin, tcs.XAxis, tcs.YAxis, tcs.ZAxis),
                CoordinateSystem.CreateMappingCoordinateSystem(fcs, tcs)
            };
            foreach (var cs in css)
            {
                var aligned = cs.Transform(fcs);
                AssertGemoetry.AreEqual(tcs.Origin, aligned.Origin);

                AssertGemoetry.AreEqual(tcs.XAxis, aligned.XAxis);

                AssertGemoetry.AreEqual(tcs.YAxis, aligned.YAxis);

                AssertGemoetry.AreEqual(tcs.ZAxis, aligned.ZAxis);
            }
        }

        [TestCase(X, Y, Z)]
        [TestCase(NegativeX, Y, Z)]
        [TestCase(NegativeX, Y, null)]
        [TestCase(X, Y, null)]
        [TestCase(X, Y, "0,0,1")]
        [TestCase("1,-1, 1", "0, 1, 1", null)]
        [TestCase(X, Y, Z)]
        [TestCase(X, Z, Y)]
        public void SetToRotateToTest(string vs, string vts, string axisString)
        {
            var v = UnitVector3D.Parse(vs);
            var vt = UnitVector3D.Parse(vts);
            UnitVector3D? axis = null;
            if (axisString != null)
            {
                axis = UnitVector3D.Parse(axisString);
            }
            CoordinateSystem cs = CoordinateSystem.RotateTo(v, vt, axis);
            var rv = cs.Transform(v);
            AssertGemoetry.AreEqual(vt, rv);

            CoordinateSystem invert = cs.Invert();
            Vector3D rotateBack = invert.Transform(rv);
            AssertGemoetry.AreEqual(v, rotateBack);

            cs = CoordinateSystem.RotateTo(vt, v, axis);
            rotateBack = cs.Transform(rv);
            AssertGemoetry.AreEqual(v, rotateBack);
        }

        [Test]
        public void TransformUnitVector()
        {
            var cs = CoordinateSystem.Rotation(90, AngleUnit.Degrees, UnitVector3D.ZAxis);
            var uv = UnitVector3D.XAxis;
            var actual = cs.Transform(uv);
            AssertGemoetry.AreEqual(UnitVector3D.YAxis, actual);
        }

        [TestCase("o:{1, 2, -7} x:{10, 0, 0} y:{0, 1, 0} z:{0, 0, 1}", "o:{0, 0, 0} x:{1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}")]
        public void Transform(string cs1s, string cs2s)
        {
            var cs1 = CoordinateSystem.Parse(cs1s);
            var cs2 = CoordinateSystem.Parse(cs2s);
            var actual = cs1.Transform(cs2);
            var expected = new CoordinateSystem(cs1.Multiply(cs2));
            AssertGemoetry.AreEqual(expected, actual);
        }

        [Test]
        public void XmlRoundTrips()
        {
            var cs = new CoordinateSystem(new Point3D(1, -2, 3), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1), new Vector3D(1, 0, 0));
            const string expected = @"
<CoordinateSystem>
    <Origin X=""1"" Y=""-2"" Z=""3"" />
    <XAxis X=""0"" Y=""1"" Z=""0"" />
    <YAxis X=""0"" Y=""0"" Z=""1"" />
    <ZAxis X=""1"" Y=""0"" Z=""0"" />
</CoordinateSystem>";
            AssertXml.XmlRoundTrips(cs, expected, (e, a) => AssertGemoetry.AreEqual(e, a));
        }
    }
}
