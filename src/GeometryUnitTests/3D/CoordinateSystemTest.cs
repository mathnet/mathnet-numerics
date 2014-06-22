namespace GeometryUnitTests
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
        readonly UnitVector3D _xAxis = UnitVector3D.XAxis;
        readonly UnitVector3D _yAxis = UnitVector3D.YAxis;
        readonly UnitVector3D _zAxis = UnitVector3D.ZAxis;
        readonly Point3D _origin = new Point3D(0, 0, 0);

        [TestCase("o:{1, 2e-6, -3} x:{1, 2, 3} y:{3, 3, 3} z:{4, 4, 4}", new double[] { 1, 2e-6, -3 }, new double[] { 1, 2, 3 }, new double[] { 3, 3, 3 }, new double[] { 4, 4, 4 })]
        public void ParseTests(string s, double[] ops, double[] xs, double[] ys, double[] zs)
        {
            var cs = CoordinateSystem.Parse(s);
            LinearAlgebraAssert.AreEqual(new Point3D(ops), cs.Origin);
            LinearAlgebraAssert.AreEqual(new Vector3D(xs), cs.XAxis);
            LinearAlgebraAssert.AreEqual(new Vector3D(ys), cs.YAxis);
            LinearAlgebraAssert.AreEqual(new Vector3D(zs), cs.ZAxis);
        }

        [Test]
        public void ConstructorTest()
        {
            var cs = new CoordinateSystem(_origin, _xAxis, _yAxis, _zAxis);
            LinearAlgebraAssert.AreEqual(cs, _origin, _xAxis.ToVector3D(), _yAxis.ToVector3D(), _zAxis.ToVector3D(), 1e-6);
        }

        //[Test]
        //public void SetCoordinateSystemTest()
        //{
        //    var coordinateSystem = new CoordinateSystem();
        //    coordinateSystem.SetCoordinateSystem(_origin, _xAxis, _yAxis, _zAxis);
        //    LinearAlgebraAssert.AreEqual(coordinateSystem, _origin, _xAxis, _yAxis, _zAxis, float.Epsilon);
        //}

        [Test]
        public void XmlTest()
        {
            var cs = new CoordinateSystem(_origin, _xAxis, _yAxis, _zAxis);
            string expectedXml =
                @"
<CoordinateSystem Name=""CordSysName"">
    <Origin X=""0"" Y=""0"" Z=""0"" />
    <XAxis X=""1"" Y=""0"" Z=""0"" />
    <YAxis X=""0"" Y=""1"" Z=""0"" />
    <ZAxis X=""0"" Y=""0"" Z=""1"" />
</CoordinateSystem>";
            var roundTrip = AssertXml.XmlSerializerRoundTrip(cs, expectedXml);
            LinearAlgebraAssert.AreEqual(cs, roundTrip);
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
            LinearAlgebraAssert.AreEqual(Point3D.Parse(evs), rp);
        }

        [TestCase("2, 0, 0", "0, 0, 1", "2, 0, 1")]
        [TestCase("2, 0, 0", "0, 0, -1", "2, 0, -1")]
        [TestCase("2, 0, 0", "0, 0, 0", "2, 0, 0")]
        [TestCase("2, 0, 0", "0, 1, 0", "2, 1, 0")]
        [TestCase("2, 0, 0", "0, -1, 0", "2, -1, 0")]
        [TestCase("2, 0, 0", "1, 0, 0", "3, 0, 0")]
        [TestCase("2, 0, 0", "-1, 0, 0", "1, 0, 0")]
        public void TranslationTests(string ps, string vs, string eps)
        {
            var p = Point3D.Parse(ps);
            var rcs = CoordinateSystem.Translation(Vector3D.Parse(vs));
            var tp = rcs.Transform(p);
            Console.WriteLine(rcs.ToString());
            LinearAlgebraAssert.AreEqual(Point3D.Parse(eps), tp);
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
            LinearAlgebraAssert.AreEqual(v2, rv);
            actual = CoordinateSystem.RotateTo(v2, v1, axis);
            rv = actual.Transform(v2);
            LinearAlgebraAssert.AreEqual(v1, rv);
        }

        [TestCase(0, 0, 0, X, Y, Z, ZeroPoint, TestName = "No rotation")]
        [TestCase(90, 0, 0, Y, NegativeX, Z, ZeroPoint, TestName = "yaw = +90")]
        [TestCase(-90, 0, 0, NegativeY, X, Z, ZeroPoint, TestName = "yaw = -90")]
        [TestCase(0, 90, 0, NegativeZ, Y, X, ZeroPoint, TestName = "pitch = +90")]
        [TestCase(0, -90, 0, Z, Y, NegativeX, ZeroPoint, TestName = "pitch = -90")]
        [TestCase(0, 0, 90, X, Z, NegativeY, ZeroPoint, TestName = "roll = +90")]
        [TestCase(0, 0, -90, X, NegativeZ, Y, ZeroPoint, TestName = "roll = -90")]
        public void RotateCoordSystemWithAnglesYawPitchRoll(double yaw, double pitch, double roll, string exs, string eys, string ezs, string eps)
        {
            CoordinateSystem actual = CoordinateSystem.Rotation(yaw, pitch, roll, AngleUnit.Degrees);
            var expected = Parse(eps, exs, eys, ezs);
            LinearAlgebraAssert.AreEqual(expected, actual);
        }

        public void InvertTest()
        {
            Assert.Inconclusive("Test this?");
        }

        [TestCase("1; -5; 3", "1; -5; 3", ZeroPoint, X, Y, Z)]
        public void TransformPoint(string ps, string eps, string ops, string xs, string ys, string zs)
        {
            var p = Point3D.Parse(ps);
            CoordinateSystem cs = Parse(ops, xs, ys, zs);
            Point3D actual = p.TransformBy(cs);
            var expected = Point3D.Parse(eps);
            LinearAlgebraAssert.AreEqual(expected, actual, float.Epsilon);
        }

        [TestCase("1; -5; 3", "1; -5; 3", ZeroPoint, X, Y, Z)]
        public void TransformVector(string vs, string evs, string pos, string xs, string ys, string zs)
        {
            var v = Vector3D.Parse(vs);
            CoordinateSystem cs = Parse(pos, xs, ys, zs);
            Vector3D actual = cs.Transform(v);
            var expected = Vector3D.Parse(evs);
            LinearAlgebraAssert.AreEqual(expected, actual);
        }

        [TestCase(ZeroPoint, "10;0;0", Y, Z, "1;0;0", X, Y, Z, "1;0;0", "0,1;0;0", Y, Z)]
        [TestCase(ZeroPoint, "10;0;0", Y, Z, ZeroPoint, X, Y, Z, ZeroPoint, "0,1;0;0", Y, Z)]
        [TestCase("7;-4;3", X, Y, Z, "1;2;3", X, Y, Z, "-6;6;0", X, Y, Z)]
        [TestCase("1;2;-7", "10;0;0", Y, Z, ZeroPoint, X, Y, Z, "-0,1;-2;7", "0,1;0;0", Y, Z)]
        public void SetToAlignCoordinateSystemsTest(string fos, string fxs, string fys, string fzs,
            string tops, string toxs, string toys, string tozs,
            string eps, string exs, string eys, string ezs)
        {
            var coordinateSystem = new CoordinateSystem();

            var p = Point3D.Parse(fos);
            var x = Vector3D.Parse(fxs);
            var y = Vector3D.Parse(fys);
            var z = Vector3D.Parse(fzs);

            var toP = Point3D.Parse(tops);
            var toX = Vector3D.Parse(toxs);
            var toY = Vector3D.Parse(toys);
            var toZ = Vector3D.Parse(tozs);

            CoordinateSystem mcs = CoordinateSystem.SetToAlignCoordinateSystems(p, x, y, z, toP, toX, toY, toZ);
            Point3D actual = p.TransformBy(mcs);
            LinearAlgebraAssert.AreEqual(actual, toP, 1e-6);
            var expected = Parse(eps, exs, eys, ezs);
            LinearAlgebraAssert.AreEqual(expected, mcs, 1E-6);
        }

        [TestCase(X, Y, Z, ZeroPoint, Y, NegativeX, Z)]
        [TestCase(NegativeX, Y, Z, ZeroPoint, NegativeY, X, Z)]
        [TestCase(NegativeX, Y, null, ZeroPoint, NegativeY, X, Z)]
        [TestCase(X, Y, null, ZeroPoint, Y, NegativeX, Z)]
        [TestCase(X, Y, Z, ZeroPoint, Y, NegativeX, Z)]
        [TestCase(X, Z, Y, ZeroPoint, Z, Y, NegativeX)]
        public void SetToRotateToTest(string vs, string vts, string axisString,
            string eps, string exs, string eys, string ezs)
        {
            var v = UnitVector3D.Parse(vs);
            var vt = UnitVector3D.Parse(vts);
            UnitVector3D? axis = null;
            if (axisString != null)
            {
                axis = UnitVector3D.Parse(axisString);
            }
            CoordinateSystem actual = CoordinateSystem.RotateTo(v, vt, axis);
            var rv = actual.Transform(v);
            LinearAlgebraAssert.AreEqual(vt, rv);
            CoordinateSystem expected = Parse(eps, exs, eys, ezs);
            LinearAlgebraAssert.AreEqual(expected, actual);
        }

        [Test]
        public void TransformUnitVector()
        {
            var cs = CoordinateSystem.Rotation(90, AngleUnit.Degrees, UnitVector3D.ZAxis);
            var uv = UnitVector3D.XAxis;
            var actual = cs.Transform(uv);
            LinearAlgebraAssert.AreEqual(UnitVector3D.YAxis, actual);
        }

        [Test]
        public void TransformCoordinateSystem()
        {
            var rcs = CoordinateSystem.Rotation(90, AngleUnit.Degrees, UnitVector3D.ZAxis);
            var cs = new CoordinateSystem(new Point3D(1, 0, 0), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
            var actual = rcs.Transform(cs);
            var expected = new CoordinateSystem(new Point3D(0, 1, 0), UnitVector3D.YAxis, UnitVector3D.XAxis.Negate(), UnitVector3D.ZAxis);
            LinearAlgebraAssert.AreEqual(expected, actual);

        }

        protected CoordinateSystem Parse(string pos, string xs, string ys, string zs)
        {
            return new CoordinateSystem(Vector3D.Parse(xs), Vector3D.Parse(ys), Vector3D.Parse(zs), Point3D.Parse(pos));
        }
    }
}
