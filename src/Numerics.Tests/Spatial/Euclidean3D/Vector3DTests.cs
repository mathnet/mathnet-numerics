// ReSharper disable InconsistentNaming

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using MathNet.Numerics.Spatial;
using MathNet.Numerics.Spatial.Euclidean3D;
using NUnit.Framework;

namespace MathNet.Numerics.Tests.Spatial.Euclidean3D
{
    [TestFixture]
    public class Vector3DTests
    {
        private const string X = "1; 0 ; 0";
        private const string Y = "0; 1; 0";
        private const string Z = "0; 0; 1";
        private const string NegativeX = "-1; 0; 0";
        private const string NegativeY = "0; -1; 0";
        private const string NegativeZ = "0; 0; -1";

        [Test]
        public void Ctor()
        {
            var v = new Vector3D(1, 2, 3);
            Assert.AreEqual(1, v.X);
            Assert.AreEqual(2, v.Y);
            Assert.AreEqual(3, v.Z);
        }

        [TestCase("1,2,-3", 3, "3,6,-9")]
        public void OperatorMultiply(string vectorAsString, double multiplier, string expected)
        {
            var vector = Vector3D.Parse(vectorAsString);
            AssertGeometry.AreEqual(Vector3D.Parse(expected), multiplier * vector, 1e-6);
        }

        [TestCase("-1,1,-1", -1, 1, -1)]
        [TestCase("1, 2, 3", 1, 2, 3)]
        [TestCase("1.2; 3.4; 5.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2;3.4;5.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2 ; 3.4 ; 5.6", 1.2, 3.4, 5.6)]
        [TestCase("1,2; 3,4; 5,6", 1.2, 3.4, 5.6)]
        [TestCase("1.2, 3.4, 5.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2 3.4 5.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2,\u00A03.4\u00A05.6", 1.2, 3.4, 5.6)]
        [TestCase("1.2\u00A03.4\u00A05.6", 1.2, 3.4, 5.6)]
        [TestCase("(1.2, 3.4 5.6)", 1.2, 3.4, 5.6)]
        [TestCase("1,2\u00A03,4\u00A05,6", 1.2, 3.4, 5.6)]
        [TestCase("(.1, 2.3e-4,1)", 0.1, 0.00023000000000000001, 1)]
        [TestCase("1.0 , 2.5,3.3", 1, 2.5, 3.3)]
        [TestCase("1,0 ; 2,5;3,3", 1, 2.5, 3.3)]
        [TestCase("1.0 ; 2.5;3.3", 1, 2.5, 3.3)]
        [TestCase("1.0,2.5,-3.3", 1, 2.5, -3.3)]
        [TestCase("1;2;3", 1, 2, 3)]
        public void Parse(string text, double expectedX, double expectedY, double expectedZ)
        {
            Assert.AreEqual(true, Vector3D.TryParse(text, out var p));
            Assert.AreEqual(expectedX, p.X);
            Assert.AreEqual(expectedY, p.Y);
            Assert.AreEqual(expectedZ, p.Z);

            p = Vector3D.Parse(text);
            Assert.AreEqual(expectedX, p.X);
            Assert.AreEqual(expectedY, p.Y);
            Assert.AreEqual(expectedZ, p.Z);

            p = Vector3D.Parse(p.ToString());
            Assert.AreEqual(expectedX, p.X);
            Assert.AreEqual(expectedY, p.Y);
            Assert.AreEqual(expectedZ, p.Z);
        }

        [TestCase("1.2")]
        [TestCase("1,2; 2.3; 3")]
        [TestCase("1; 2; 3; 4")]
        public void ParseFails(string text)
        {
            Assert.AreEqual(false, Vector3D.TryParse(text, out _));
            Assert.Throws<FormatException>(() => Vector3D.Parse(text));
        }

        [TestCase("<Vector3D X=\"1\" Y=\"-2\" Z=\"3\" />")]
        [TestCase("<Vector3D Y=\"-2\" Z=\"3\"  X=\"1\"/>")]
        [TestCase("<Vector3D Z=\"3\" X=\"1\" Y=\"-2\" />")]
        [TestCase("<Vector3D><X>1</X><Y>-2</Y><Z>3</Z></Vector3D>")]
        [TestCase("<Vector3D><Y>-2</Y><Z>3</Z><X>1</X></Vector3D>")]
        [TestCase("<Vector3D><Z>3</Z><X>1</X><Y>-2</Y></Vector3D>")]
        public void ReadFrom(string xml)
        {
            using (var reader = new StringReader(xml))
            {
                var actual = Vector3D.ReadFrom(XmlReader.Create(reader));
                Assert.AreEqual(new Vector3D(1, -2, 3), actual);
            }
        }

        [Test]
        public void ToDenseVector()
        {
            var v = new Vector3D(1, 2, 3);
            var vector = v.ToVector();
            Assert.AreEqual(3, vector.Count);
            Assert.AreEqual(1, vector[0]);
            Assert.AreEqual(2, vector[1]);
            Assert.AreEqual(3, vector[2]);

            var roundtripped = Vector3D.OfVector(vector);
            Assert.AreEqual(1, roundtripped.X);
            Assert.AreEqual(2, roundtripped.Y);
            Assert.AreEqual(3, roundtripped.Z);
        }

        [TestCase("1; 0 ; 0")]
        [TestCase("1; 1 ; 0")]
        [TestCase("1; -1 ; 0")]
        public void Orthogonal(string vs)
        {
            var v = Vector3D.Parse(vs);
            var orthogonal = v.Orthogonal;
            Assert.IsTrue(orthogonal.DotProduct(v) < 1e-6);
        }

        [TestCase("0; 0 ; 0")]
        public void Orthogonal_BadArgument(string vs)
        {
            var v = Vector3D.Parse(vs);
#pragma warning disable SA1312 // Variable names must begin with lower-case letter
            Assert.Throws<InvalidOperationException>(() => { var _ = v.Orthogonal; });
#pragma warning restore SA1312 // Variable names must begin with lower-case letter
        }

        [TestCase(X, Y, Z)]
        [TestCase(X, "1, 1, 0", Z)]
        [TestCase(X, NegativeY, NegativeZ)]
        [TestCase(Y, Z, X)]
        [TestCase(Y, "0.1, 0.1, 1", "1, 0, -0.1", Description = "Almost Z")]
        [TestCase(Y, "-0.1, -0.1, 1", "1, 0, 0.1", Description = "Almost Z men minus")]
        public void CrossProduct(string v1s, string v2s, string ves)
        {
            var vector1 = Vector3D.Parse(v1s);
            var vector2 = Vector3D.Parse(v2s);
            var expected = Vector3D.Parse(ves);
            var crossProduct = vector1.CrossProduct(vector2);
            AssertGeometry.AreEqual(expected, crossProduct, 1E-6);
        }

        [TestCase(X, Y, Z, 90)]
        [TestCase(X, X, Z, 0)]
        [TestCase(X, NegativeY, Z, -90)]
        [TestCase(X, NegativeX, Z, 180)]
        public void SignedAngleTo(string fromString, string toString, string axisString, double degreeAngle)
        {
            var fromVector = Vector3D.Parse(fromString);
            var toVector = Vector3D.Parse(toString);
            var aboutVector = Vector3D.Parse(axisString);
            Assert.AreEqual(degreeAngle, fromVector.SignedAngleTo(toVector, aboutVector.Normalize()).Degrees, 1E-6);
        }

        [TestCase("1; 0; 1", Y, "-1; 0; 1", "90°")]
        public void SignedAngleToArbitraryVector(string fromString, string toString, string axisString, string @as)
        {
            var fromVector = Vector3D.Parse(fromString);
            var toVector = Vector3D.Parse(toString);
            var aboutVector = Vector3D.Parse(axisString);
            var angle = Angle.Parse(@as);
            Assert.AreEqual(angle.Degrees, fromVector.SignedAngleTo(toVector.Normalize(), aboutVector.Normalize()).Degrees, 1E-6);
        }

        [TestCase(X, 5)]
        [TestCase(Y, 5)]
        [TestCase("1; 1; 0", 5)]
        [TestCase("1; 0; 1", 5)]
        [TestCase("0; 1; 1", 5)]
        [TestCase("1; 1; 1", 5)]
        [TestCase(X, 90)]
        [TestCase(Y, 90)]
        [TestCase("1; 1; 0", 90)]
        [TestCase("1; 0; 1", 90)]
        [TestCase("0; 1; 1", 90)]
        [TestCase("1; 1; 1", 90)]
        [TestCase("1; 0; 1", -90)]
        [TestCase("1; 0; 1", 180)]
        [TestCase("1; 0; 1", 0)]
        public void SignedAngleTo_RotationAroundZ(string vectorDoubles, double rotationInDegrees)
        {
            var vector = Vector3D.Parse(vectorDoubles);
            var angle = Angle.FromDegrees(rotationInDegrees);
            var rotated = Vector3D.OfVector(Matrix3D.RotationAroundZAxis(angle).Multiply(vector.ToVector()));
            var actual = vector.SignedAngleTo(rotated, Vector3D.Parse(Z).Normalize());
            Assert.AreEqual(rotationInDegrees, actual.Degrees, 1E-6);
        }

        [TestCase(X, Z, 90, Y)]
        public void Rotate(string vs, string avs, double deg, string evs)
        {
            var v = Vector3D.Parse(vs);
            var about = Vector3D.Parse(avs);
            var expected = Vector3D.Parse(evs);
            var rotated = v.Rotate(about, Angle.FromDegrees(deg));
            AssertGeometry.AreEqual(expected, rotated, 1E-6);

            rotated = v.Rotate(about.Normalize(), Angle.FromDegrees(deg));
            AssertGeometry.AreEqual(expected, rotated, 1E-6);
        }

        [TestCase("X", X)]
        [TestCase("Y", Y)]
        [TestCase("Z", Z)]
        public void SignedAngleTo_Itself(string axisDummy, string aboutDoubles)
        {
            var vector = new Vector3D(1, 1, 1);
            var aboutVector = Vector3D.Parse(aboutDoubles);
            var angle = vector.SignedAngleTo(vector, aboutVector.Normalize());
            Assert.AreEqual(0, angle.Degrees, 1E-6);
        }

        [TestCase(X, Y, "90°")]
        [TestCase(Y, X, "90°")]
        [TestCase(X, Z, "90°")]
        [TestCase(Z, X, "90°")]
        [TestCase(Y, Z, "90°")]
        [TestCase(Z, Y, "90°")]
        [TestCase(X, X, "0°")]
        [TestCase(Y, Y, "0°")]
        [TestCase(Z, Z, "0°")]
        [TestCase(X, NegativeY, "90°")]
        [TestCase(Y, NegativeY, "180°")]
        [TestCase(Z, NegativeZ, "180°")]
        [TestCase("1; 1; 0", X, "45°")]
        [TestCase("1; 1; 0", Y, "45°")]
        [TestCase("1; 1; 0", Z, "90°")]
        [TestCase("2; 2; 0", "0; 0; 2", "90°")]
        [TestCase("1; 1; 1", X, "54.74°")]
        [TestCase("1; 1; 1", Y, "54.74°")]
        [TestCase("1; 1; 1", Z, "54.74°")]
        [TestCase("1; 0; 0", "1; 0; 0", "0°")]
        [TestCase("-1; -1; 1", "-1; -1; 1", "0°")]
        [TestCase("1; 1; 1", "-1; -1; -1", "180°")]
        public void AngleTo(string v1s, string v2s, string ea)
        {
            var v1 = Vector3D.Parse(v1s);
            var v2 = Vector3D.Parse(v2s);
            var angles = new[]
                {
                    v1.AngleTo(v2),
                    v2.AngleTo(v1)
                };
            var expected = Angle.Parse(ea);
            foreach (var angle in angles)
            {
                Assert.AreEqual(expected.Radians, angle.Radians, 1E-2);
            }
        }

        [TestCase("5; 0; 0", "1; 0 ; 0")]
        [TestCase("-5; 0; 0", "-1; 0 ; 0")]
        [TestCase("0; 5; 0", "0; 1 ; 0")]
        [TestCase("0; -5; 0", "0; -1 ; 0")]
        [TestCase("0; 0; 5", "0; 0 ; 1")]
        [TestCase("0; 0; -5", "0; 0 ; -1")]
        [TestCase("2; 2; 2", "0,577350269189626; 0,577350269189626; 0,577350269189626")]
        [TestCase("-2; 15; 2", "-0,131024356416084; 0,982682673120628; 0,131024356416084")]
        public void Normalize(string vs, string evs)
        {
            var vector = Vector3D.Parse(vs);
            var uv = vector.Normalize();
            var expected = UnitVector3D.Parse(evs);
            AssertGeometry.AreEqual(expected, uv, 1E-6);
        }

        [TestCase("0; 0; 0", "0; 0 ; 0")]
        public void Normalize_BadArgument(string vs, string evs)
        {
            var vector = Vector3D.Parse(vs);
            //// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Assert.Throws<InvalidOperationException>(() => vector.Normalize());
        }

        [TestCase("1, -1, 10", 5, "5, -5, 50")]
        public void Scale(string vs, double s, string evs)
        {
            var v = Vector3D.Parse(vs);
            var actual = v.ScaleBy(s);
            AssertGeometry.AreEqual(Vector3D.Parse(evs), actual, 1e-6);
        }

        [TestCase("5;0;0", 5)]
        [TestCase("-5;0;0", 5)]
        [TestCase("-3;0;4", 5)]
        public void Length(string vectorString, double length)
        {
            var vector = Vector3D.Parse(vectorString);
            Assert.AreEqual(length, vector.Length);
        }

        [TestCase(X, X, true)]
        [TestCase(X, NegativeX, true)]
        [TestCase(Y, Y, true)]
        [TestCase(Y, NegativeY, true)]
        [TestCase(Z, NegativeZ, true)]
        [TestCase(Z, Z, true)]
        [TestCase("1;-8;7", "1;-8;7", true)]
        [TestCase(X, "1;-8;7", false)]
        [TestCase("1;-1.2;0", Z, false)]
        public void IsParallelTo(string vector1, string vector2, bool expected)
        {
            var v1 = Vector3D.Parse(vector1);
            var v2 = Vector3D.Parse(vector2);
            Assert.AreEqual(true, v1.IsParallelTo(v1, 1E-6));
            Assert.AreEqual(true, v2.IsParallelTo(v2, 1E-6));
            Assert.AreEqual(expected, v1.IsParallelTo(v2, 1E-6));
            Assert.AreEqual(expected, v2.IsParallelTo(v1, 1E-6));
        }

        [TestCase("0,1,0", "0,1, 0", 1e-10, true)]
        [TestCase("0,1,0", "0,-1, 0", 1e-10, true)]
        [TestCase("0,1,0", "0,1, 1", 1e-10, false)]
        [TestCase("0,1,1", "0,1, 1", 1e-10, true)]
        [TestCase("0,1,-1", "0,-1, 1", 1e-10, true)]
        [TestCase("0,1,0", "0,1, 0.001", 1e-10, false)]
        [TestCase("0,1,0", "0,1, -0.001", 1e-10, false)]
        [TestCase("0,-1,0", "0,1, 0.001", 1e-10, false)]
        [TestCase("0,-1,0", "0,1, -0.001", 1e-10, false)]
        [TestCase("0,1,0", "0,1, 0.001", 1e-6, true, Description = "These test cases demonstrate the effect of the tolerance")]
        [TestCase("0,1,0", "0,1, -0.001", 1e-6, true, Description = "These test cases demonstrate the effect of the tolerance")]
        [TestCase("0,-1,0", "0,1, 0.001", 1e-6, true, Description = "These test cases demonstrate the effect of the tolerance")]
        [TestCase("0,-1,0", "0,1, -0.001", 1e-6, true, Description = "These test cases demonstrate the effect of the tolerance")]
        [TestCase("0,1,0.5", "0,-1, -0.5", 1e-10, true)]
        public void IsParallelToByDoubleTolerance(string v1s, string v2s, double tolerance, bool expected)
        {
            var v1 = Vector3D.Parse(v1s);
            var v2 = Vector3D.Parse(v2s);
            Assert.AreEqual(expected, v1.IsParallelTo(v2, tolerance));
            Assert.AreEqual(expected, v2.IsParallelTo(v1, tolerance));
        }

        [TestCase("0,1,0", "0,1, 0", 1e-10, true)]
        [TestCase("0,1,0", "0,-1, 0", 1e-10, true)]
        [TestCase("0,1,0", "0,1, 1", 1e-10, false)]
        [TestCase("0,1,1", "0,1, 1", 1e-10, true)]
        [TestCase("0,1,-1", "0,-1, 1", 1e-10, true)]
        [TestCase("0,1,0", "0,1, 0.001", 1e-10, false)]
        [TestCase("0,1,0", "0,1, -0.001", 1e-10, false)]
        [TestCase("0,-1,0", "0,1, 0.001", 1e-10, false)]
        [TestCase("0,-1,0", "0,1, -0.001", 1e-10, false)]
        [TestCase("0,1,0", "0,1, 0.001", 1e-6, true, Description = "These test cases demonstrate the effect of the tolerance")]
        [TestCase("0,1,0", "0,1, -0.001", 1e-6, true, Description = "These test cases demonstrate the effect of the tolerance")]
        [TestCase("0,-1,0", "0,1, 0.001", 1e-6, true, Description = "These test cases demonstrate the effect of the tolerance")]
        [TestCase("0,-1,0", "0,1, -0.001", 1e-6, true, Description = "These test cases demonstrate the effect of the tolerance")]
        [TestCase("0,1,0.5", "0,-1, -0.5", 1e-10, true)]
        public void IsParallelToUnitVectorByDoubleTolerance(string v1s, string v2s, double tolerance, bool expected)
        {
            var v1 = Vector3D.Parse(v1s);
            var v2 = Vector3D.Parse(v2s).Normalize();
            Assert.AreEqual(expected, v1.IsParallelTo(v2, tolerance));
            Assert.AreEqual(expected, v2.IsParallelTo(v1, tolerance));
        }

        [TestCase("0,1,0", "0,1, 0", 1e-4, true)]
        [TestCase("0,1,0", "0,-1, 0", 1e-4, true)]
        [TestCase("0,1,0", "0,1, 1", 1e-4, false)]
        [TestCase("0,1,1", "0,1, 1", 1e-4, true)]
        [TestCase("0,1,-1", "0,-1, 1", 1e-4, true)]
        [TestCase("0,1,0", "0,1, 0.001", 0.06, true)]
        [TestCase("0,1,0", "0,1, -0.001", 0.06, true)]
        [TestCase("0,-1,0", "0,1, 0.001", 0.06, true)]
        [TestCase("0,-1,0", "0,1, -0.001", 0.06, true)]
        [TestCase("0,1,0", "0,1, 0.001", 0.05, false)]
        [TestCase("0,1,0", "0,1, -0.001", 0.05, false)]
        [TestCase("0,-1,0", "0,1, 0.001", 0.05, false)]
        [TestCase("0,-1,0", "0,1, -0.001", 0.05, false)]
        [TestCase("0,1,0.5", "0,-1, -0.5", 1e-4, true)]
        public void IsParallelToByAngleTolerance(string v1s, string v2s, double degreesTolerance, bool expected)
        {
            var v1 = Vector3D.Parse(v1s);
            var v2 = Vector3D.Parse(v2s);
            Assert.AreEqual(expected, v1.IsParallelTo(v2, Angle.FromDegrees(degreesTolerance)));
            Assert.AreEqual(expected, v2.IsParallelTo(v1, Angle.FromDegrees(degreesTolerance)));
        }

        [TestCase("0,1,0", "0,1, 0", 1e-4, true)]
        [TestCase("0,1,0", "0,-1, 0", 1e-4, true)]
        [TestCase("0,1,0", "0,1, 1", 1e-4, false)]
        [TestCase("0,1,1", "0,1, 1", 1e-4, true)]
        [TestCase("0,1,-1", "0,-1, 1", 1e-4, true)]
        [TestCase("0,1,0", "0,1, 0.001", 0.06, true)]
        [TestCase("0,1,0", "0,1, -0.001", 0.06, true)]
        [TestCase("0,-1,0", "0,1, 0.001", 0.06, true)]
        [TestCase("0,-1,0", "0,1, -0.001", 0.06, true)]
        [TestCase("0,1,0", "0,1, 0.001", 0.05, false)]
        [TestCase("0,1,0", "0,1, -0.001", 0.05, false)]
        [TestCase("0,-1,0", "0,1, 0.001", 0.05, false)]
        [TestCase("0,-1,0", "0,1, -0.001", 0.05, false)]
        [TestCase("0,1,0.5", "0,-1, -0.5", 1e-4, true)]
        public void IsParallelToUnitVectorByAngleTolerance(string v1s, string v2s, double degreesTolerance, bool expected)
        {
            var v1 = Vector3D.Parse(v1s);
            var v2 = Vector3D.Parse(v2s).Normalize();
            Assert.AreEqual(expected, v1.IsParallelTo(v2, Angle.FromDegrees(degreesTolerance)));
            Assert.AreEqual(expected, v2.IsParallelTo(v1, Angle.FromDegrees(degreesTolerance)));
        }

        [TestCase(X, X, false)]
        [TestCase(NegativeX, X, false)]
        [TestCase("-11;0;0", X, false)]
        [TestCase("1;1;0", X, false)]
        [TestCase(X, Y, true)]
        [TestCase(X, Z, true)]
        [TestCase(Y, X, true)]
        [TestCase(Y, Z, true)]
        [TestCase(Z, Y, true)]
        [TestCase(Z, X, true)]
        public void IsPerpendicularTo(string v1s, string v2s, bool expected)
        {
            var v1 = Vector3D.Parse(v1s);
            var v2 = Vector3D.Parse(v2s);
            Assert.AreEqual(expected, v1.IsPerpendicularTo(v2));
        }

        [TestCase("1, 2, 3", "1, 2, 3", 1e-4, true)]
        [TestCase("1, 2, 3", "4, 5, 6", 1e-4, false)]
        public void Equals(string p1s, string p2s, double tol, bool expected)
        {
            var v1 = Vector3D.Parse(p1s);
            var v2 = Vector3D.Parse(p2s);
            Assert.AreEqual(expected, v1 == v2);
            Assert.AreEqual(expected, v1.Equals(v2));
            Assert.AreEqual(expected, v1.Equals((object)v2));
            Assert.AreEqual(expected, Equals(v1, v2));
            Assert.AreEqual(expected, v1.Equals(v2, tol));
            Assert.AreNotEqual(expected, v1 != v2);
        }

        [TestCase("-2, 0, 1e-4", null, "(-2, 0, 0.0001)", 1e-4)]
        [TestCase("-2, 0, 1e-4", "F2", "(-2.00, 0.00, 0.00)", 1e-4)]
        public void ToString(string vs, string format, string expected, double tolerance)
        {
            var v = Vector3D.Parse(vs);
            var actual = v.ToString(format);
            Assert.AreEqual(expected, actual);
            AssertGeometry.AreEqual(v, Vector3D.Parse(actual), tolerance);
        }

        [Test]
        public void XmlRoundtrip()
        {
            var p = new Vector3D(1, -2, 3);
            var xml = @"<Vector3D X=""1"" Y=""-2"" Z=""3"" />";
            AssertXml.XmlRoundTrips(p, xml, (expected, actual) => AssertGeometry.AreEqual(expected, actual));
        }

        [Test]
        public void XmlContainerRoundtrip()
        {
            var container = new AssertXml.Container<Vector3D>
            {
                Value1 = new Vector3D(1, 2, 3),
                Value2 = new Vector3D(4, 5, 6)
            };
            var expected = "<ContainerOfVector3D>\r\n" +
                           "  <Value1 X=\"1\" Y=\"2\" Z=\"3\"></Value1>\r\n" +
                           "  <Value2 X=\"4\" Y=\"5\" Z=\"6\"></Value2>\r\n" +
                           "</ContainerOfVector3D>";
            var roundTrip = AssertXml.XmlSerializerRoundTrip(container, expected);
            AssertGeometry.AreEqual(container.Value1, roundTrip.Value1);
            AssertGeometry.AreEqual(container.Value2, roundTrip.Value2);
        }

        [Test]
        public void XmlElements()
        {
            var v = new Vector3D(1, 2, 3);
            var serializer = new XmlSerializer(typeof(Vector3D));
            AssertGeometry.AreEqual(v, (Vector3D)serializer.Deserialize(new StringReader(@"<Vector3D><X>1</X><Y>2</Y><Z>3</Z></Vector3D>")));
        }

        [Test]
        public void XmlContainerElements()
        {
            var xml = "<ContainerOfVector3D>\r\n" +
                      "  <Value1><X>1</X><Y>2</Y><Z>3</Z></Value1>\r\n" +
                      "  <Value2><X>4</X><Y>5</Y><Z>6</Z></Value2>\r\n" +
                      "</ContainerOfVector3D>";
            var serializer = new XmlSerializer(typeof(AssertXml.Container<Vector3D>));
            var deserialized = (AssertXml.Container<Vector3D>)serializer.Deserialize(new StringReader(xml));
            AssertGeometry.AreEqual(new Vector3D(1, 2, 3), deserialized.Value1);
            AssertGeometry.AreEqual(new Vector3D(4, 5, 6), deserialized.Value2);
        }
    }
}
