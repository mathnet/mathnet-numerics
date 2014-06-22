namespace GeometryUnitTests.Unts
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Geometry.Units;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;

    public class AngleTests
    {
        private const double Tolerance = 1e-6;
        private const double degToRad = Math.PI / 180;
        [Test]
        public void Ctor()
        {
            const double deg = 90;
            const double rad = deg * degToRad;
            var angles = new Angle[]
            {
                new Angle(deg, AngleUnit.Degrees),
                deg * AngleUnit.Degrees,
                rad * AngleUnit.Radians,
                Angle.FromDegrees(deg),
                Angle.FromRadians(rad),
                Angle.From(deg, AngleUnit.Degrees),
                Angle.From(rad, AngleUnit.Radians),
                new Angle(rad, AngleUnit.Radians),
            };
            foreach (var angle in angles)
            {
                Assert.AreEqual(rad, angle.Value, Tolerance);
            }
        }

        [TestCase("5 °", 5 * degToRad)]
        [TestCase("5°", 5 * degToRad)]
        [TestCase("-5.34 rad", -5.34)]
        [TestCase("-5,34 rad", -5.34)]
        [TestCase("1e-4 rad", 0.0001)]
        [TestCase("1e-4 °", 0.0001 * degToRad)]
        public void ParseTest(string s, double expected)
        {
            var angle = Angle.Parse(s);
            Assert.AreEqual(expected, angle.Value, Tolerance);
            Assert.IsInstanceOf<Angle>(angle);
        }

        [TestCase("90 °", 90, Math.PI / 2)]
        [TestCase("1.5707963267949 rad", 90, Math.PI / 2)]
        public void Convert(string vs, double degv, double radv)
        {
            var angles = new Angle[]
            {
                Angle.Parse(vs),
                degv * AngleUnit.Degrees,
                radv * AngleUnit.Radians
            };
            foreach (var angle in angles)
            {
                Assert.AreEqual(degv, angle.Degrees, Tolerance);
                Assert.AreEqual(radv, angle, Tolerance);
                Assert.AreEqual(radv, angle.Value, Tolerance);
            }
        }

        [TestCase("90 °", 90, Math.PI / 2, true)]
        [TestCase("1 rad", 1 * 180 / Math.PI, 1, true)]
        [TestCase("1.1 rad", 1 * 180 / Math.PI, Math.PI / 2, false)]
        public void Equals(string s, double degv, double radv, bool expected)
        {
            var a = Angle.Parse(s);
            var deg = Angle.From(degv, AngleUnit.Degrees);
            Assert.AreEqual(expected, deg.Equals(a));
            Assert.AreEqual(expected, deg.Equals(a, Tolerance));
            Assert.AreEqual(expected, deg == a);
            Assert.AreEqual(!expected, deg != a);

            var rad = Angle.From(radv, AngleUnit.Radians);
            Assert.AreEqual(expected, rad.Equals(a));
            Assert.AreEqual(expected, rad.Equals(a, Tolerance));
            Assert.AreEqual(expected, rad == a);
            Assert.AreEqual(!expected, rad != a);
        }

        [TestCase("1.5707 rad", "1.5707 rad", 1.5707 + 1.5707)]
        [TestCase("1.5707 rad", "2 °", 1.5707 + 2 * degToRad)]
        public void Addition(string lvs, string rvs, double ev)
        {
            var lv = Angle.Parse(lvs);
            var rv = Angle.Parse(rvs);
            var sum = lv + rv;
            Assert.AreEqual(ev, sum.Value, Tolerance);
            Assert.IsInstanceOf<Angle>(sum);
        }

        [TestCase("1.5707 rad", "1.5706 rad", 1.5707 - 1.5706)]
        [TestCase("1.5707 rad", "2 °", 1.5707 - 2 * degToRad)]
        public void Subtraction(string lvs, string rvs, double ev)
        {
            var lv = Angle.Parse(lvs);
            var rv = Angle.Parse(rvs);
            var diff = lv - rv;
            Assert.AreEqual(ev, diff.Value, Tolerance);
            Assert.IsInstanceOf<Angle>(diff);
        }

        [TestCase("15 °", 5, 15 * 5 * degToRad)]
        [TestCase("-10 °", 0, 0)]
        [TestCase("-10 °", 2, -10 * 2 * degToRad)]
        [TestCase("1 rad", 2, 2)]
        public void Multiplication(string lvs, double rv, double ev)
        {
            var lv = Angle.Parse(lvs);
            var prods = new[] { lv * rv, rv * lv };
            foreach (var prod in prods)
            {
                Assert.AreEqual(ev, prod.Value, 1e-3);
                Assert.IsInstanceOf<Angle>(prod);
            }
        }

        [TestCase("3.141596 rad", 2, 1.570797999)]
        public void DivisionTest(string s, double rv, double expected)
        {
            var angle = Angle.Parse(s);
            var actual = angle / rv;
            Assert.AreEqual(expected, actual.Value, Tolerance);
            Assert.IsInstanceOf<Angle>(actual);
        }

        [Test]
        public void Compare()
        {
            Angle small = Angle.FromDegrees(1);
            Angle big = Angle.FromDegrees(2);
            Assert.IsTrue(small < big);
            Assert.IsTrue(small <= big);
            Assert.IsFalse(small > big);
            Assert.IsFalse(small >= big);
            Assert.AreEqual(-1, small.CompareTo(big));
            Assert.AreEqual(0, small.CompareTo(small));
            Assert.AreEqual(1, big.CompareTo(small));
        }

        [TestCase("15 °", "0.261799387799149rad")]
        public void ToString(string s, string expected)
        {
            Angle angle = Angle.Parse(s);
            string toString = angle.ToString(CultureInfo.InvariantCulture);
            string toStringComma = angle.ToString(CultureInfo.GetCultureInfo("sv"));
            Assert.AreEqual(expected, toString);
            Assert.AreEqual(expected.Replace('.', ','), toStringComma);
            Assert.IsTrue(angle.Equals(Angle.Parse(toString), Tolerance));
            Assert.IsTrue(angle.Equals(Angle.Parse(toStringComma), Tolerance));
        }

        [TestCase("15 °", "F2", "15.00°")]
        public void ToString(string s, string format, string expected)
        {
            Angle angle = Angle.Parse(s);
            string toString = angle.ToString(format, CultureInfo.InvariantCulture, AngleUnit.Degrees);
            string toStringComma = angle.ToString(format, CultureInfo.GetCultureInfo("sv"), AngleUnit.Degrees);
            Assert.AreEqual(expected, toString);
            Assert.AreEqual(expected.Replace('.', ','), toStringComma);
            Assert.IsTrue(angle.Equals(Angle.Parse(toString), Tolerance));
            Assert.IsTrue(angle.Equals(Angle.Parse(toStringComma), Tolerance));
        }

        [TestCase("15 °", false, @"<Angle Value=""0.26179938779914941"" />")]
        [TestCase("15 °", true, @"<Angle><Value>0.261799387799149</Value></Angle>")]
        public void XmlTest(string vs, bool asElements, string xml)
        {
            var angle = Angle.Parse(vs);
            angle.SerializeAsElements = asElements;
            AssertXml.XmlRoundTrips(angle, xml, (e, a) =>
            {
                Assert.AreEqual(e.Value, a.Value, Tolerance);
                Assert.IsInstanceOf<Angle>(a);
            });
        }
    }
}
