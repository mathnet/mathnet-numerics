namespace MathNet.GeometryUnitTests
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Geometry;
    using Geometry.Units;
    using NUnit.Framework;

    public class Vector2DTests
    {
        [TestCase("1, 0", "1, 0", 1e-4, true)]
        [TestCase("-1, 1", "-1, 1", 1e-4, true)]
        [TestCase("1, 0", "1, 1", 1e-4, false)]
        public void Equals(string v1s, string v2s, double tol, bool expected)
        {
            var v1 = Vector2D.Parse(v1s);
            var v2 = Vector2D.Parse(v2s);
            Assert.AreEqual(expected, v1 == v2);
            Assert.AreEqual(expected, v1.Equals(v2));
            Assert.AreEqual(expected, Equals(v1, v2));
            Assert.AreEqual(expected, v1.Equals(v2, tol));
            Assert.AreNotEqual(expected, v1 != v2);
        }

        [TestCase("2, 0", 2)]
        [TestCase("-2, 0", 2)]
        [TestCase("0, 2", 2)]
        public void Length(string vs, double expected)
        {
            var v = Vector2D.Parse(vs);
            Assert.AreEqual(expected, v.Length, 1e-6);
        }

        [TestCase("-2, 0", null, "(-2, 0)")]
        [TestCase("-2, 0", "N2", "(-2.00, 0.00)")]
        public void ToString(string vs, string format, string expected)
        {
            var v = Vector2D.Parse(vs);
            string actual = v.ToString(format);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(v, Vector2D.Parse(actual));
        }

        [TestCase("1, 0", "1, 0", 1e-4, false)]
        [TestCase("1, 0", "0, -1", 1e-4, true)]
        [TestCase("1, 0", "0, 1", 1e-4, true)]
        [TestCase("0, 1", "1, 0", 1e-4, true)]
        [TestCase("0, 1", "0, 1", 1e-4, false)]
        public void IsPerpendicularTo(string v1s, string v2s, double tol, bool expected)
        {
            var v1 = Vector2D.Parse(v1s);
            var v2 = Vector2D.Parse(v2s);
            Assert.AreEqual(expected, v1.IsPerpendicularTo(v2, tol));
            Assert.AreEqual(expected, v2.IsPerpendicularTo(v1, tol));
        }

        [TestCase("1, 0", "1, 0", 1e-4, true)]
        [TestCase("1, 0", "-1, 0", 1e-4, true)]
        [TestCase("1, 0", "1, 1", 1e-4, false)]
        [TestCase("1, 1", "1, 1", 1e-4, true)]
        [TestCase("1, -1", "-1, 1", 1e-4, true)]
        public void IsParallelTo(string v1s, string v2s, double tol, bool expected)
        {
            var v1 = Vector2D.Parse(v1s);
            var v2 = Vector2D.Parse(v2s);
            Assert.AreEqual(expected, v1.IsParallelTo(v2, tol));
            Assert.AreEqual(expected, v2.IsParallelTo(v1, tol));
        }

        [TestCase("1, 0", "0, 1", false, 90)]
        [TestCase("1, 0", "0, 1", true, 270)]
        [TestCase("1, 0", "0, -1", true, 90)]
        [TestCase("1, 0", "0, -1", false, 270)]
        [TestCase("1, 0", "-1, 0", true, 180)]
        [TestCase("1, 0", "-1, 0", false, 180)]
        [TestCase("1, 0", "1, 0", false, 0)]
        [TestCase("0, 1", "1, 0", true, 90)]
        public void AngleToTest(string v1s, string v2s, bool clockWise, float expected)
        {
            var v1 = Vector2D.Parse(v1s);
            var v2 = Vector2D.Parse(v2s);
            Angle av = v1.SignedAngleTo(v2, clockWise);
            Assert.AreEqual(expected, av.Degrees, 0.1);
        }

        [TestCase("1, 0", "90°", "0, 1")]
        [TestCase("1, 0", "-270°", "0, 1")]
        [TestCase("1, 0", "-90°", "0, -1")]
        [TestCase("1, 0", "270°", "0, -1")]
        [TestCase("1, 0", "180°", "-1, 0")]
        [TestCase("1, 0", "180°", "-1, 0")]
        [TestCase("1, 0", "0°", "1, 0")]
        [TestCase("0, 1", "-90°", "1, 0")]
        public void RotateTest(string vs, string @as, string evs)
        {
            var v = Vector2D.Parse(vs);
            var angle = Angle.Parse(@as);
            var rv = v.Rotate(angle);
            var expected = Vector2D.Parse(evs);
            LinearAlgebraAssert.AreEqual(expected, rv, 0.1);
        }

        [TestCase("1,0", "0,1", "270°", "-90°")]
        [TestCase("0,1", "1,0", "90°", "90°")]
        [TestCase("0,1", "0,-1", "180°", "180°")]
        public void SignedAngleTo(string v1s, string v2s, string expectedClockWise, string expectedNegative)
        {
            var v1 = Vector2D.Parse(v1s);
            var v2 = Vector2D.Parse(v2s);
            var cw = v1.SignedAngleTo(v2, true);
            var expected = Angle.Parse(expectedClockWise);
            Assert.AreEqual(expected.Degrees, cw.Degrees, 1e-4);
            var cwNeg = v1.SignedAngleTo(v2, true, true);
            Assert.AreEqual(Angle.Parse(expectedNegative).Degrees, cwNeg.Degrees, 1e-4);

            var ccw = v1.SignedAngleTo(v2, false);
            Assert.AreEqual(360 - expected.Degrees, ccw.Degrees, 1e-4);
        }

        [TestCase("1, 2", false, @"<Vector2D X=""1"" Y=""2"" />")]
        [TestCase("1, 2", true, @"<Vector2D><X>1</X><Y>2</Y></Vector2D>")]
        public void XmlRoundtrip(string vs, bool asElements, string xml)
        {
            var p = Vector2D.Parse(vs);
            p.SerializeAsElements = asElements;
            AssertXml.XmlRoundTrips(p, xml, (e, a) => LinearAlgebraAssert.AreEqual(e, a));
        }

        [Test]
        public void BinaryRountrip()
        {
            var v = new Vector2D(1, 2);
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, v);
                ms.Flush();
                ms.Position = 0;
                var roundTrip = (Vector2D)formatter.Deserialize(ms);
                LinearAlgebraAssert.AreEqual(v, roundTrip);
            }
        }
    }
}
