namespace MathNet.GeometryUnitTests
{
    using System;
    using Geometry;
    using Geometry.Units;
    using NUnit.Framework;

    [TestFixture]
    public class Vector3DTests
    {
        const string X = "1; 0 ; 0";
        const string Y = "0; 1; 0";
        const string Z = "0; 0; 1";
        const string NegativeX = "-1; 0; 0";
        const string NegativeY = "0; -1; 0";
        const string NegativeZ = "0; 0; -1";

        [TestCase("1; 0 ; 0")]
        [TestCase("1; 1 ; 0")]
        [TestCase("1; -1 ; 0")]
        [TestCase("0; 0 ; 0", ExpectedException = typeof(ArgumentException))]
        public void Orthogonal(string vs)
        {
            Vector3D v = Vector3D.Parse(vs);
            UnitVector3D orthogonal = v.Orthogonal;
            Assert.IsTrue(orthogonal.DotProduct(v) < 1e-6);
        }

        [TestCase("-2, 0, 1e-4", null, "(-2, 0, 0.0001)", 1e-4)]
        [TestCase("-2, 0, 1e-4", "F2", "(-2.00, 0.00, 0.00)", 1e-4)]
        public void ToString(string vs, string format, string expected, double tolerance)
        {
            var v = Vector3D.Parse(vs);
            string actual = v.ToString(format);
            Assert.AreEqual(expected, actual);
            LinearAlgebraAssert.AreEqual(v, Vector3D.Parse(actual), tolerance);
        }

        [TestCase(X, Y, Z)]
        [TestCase(X, "1, 1, 0", Z)]
        [TestCase(X, NegativeY, NegativeZ)]
        [TestCase(Y, Z, X)]
        [TestCase(Y, "0.1, 0.1, 1", "1, 0, -0.1", Description = "Nästan Z")]
        [TestCase(Y, "-0.1, -0.1, 1", "1, 0, 0.1", Description = "Nästan Z men minus")]
        public void CrossProductTest(string v1s, string v2s, string ves)
        {
            var vector1 = Vector3D.Parse(v1s);
            var vector2 = Vector3D.Parse(v2s);
            var expected = Vector3D.Parse(ves);
            Vector3D crossProduct = vector1.CrossProduct(vector2);
            LinearAlgebraAssert.AreEqual(expected, crossProduct, 1E-6);
        }

        [TestCase(X, Y, Z, 90)]
        [TestCase(X, X, Z, 0)]
        [TestCase(X, NegativeY, Z, -90)]
        [TestCase(X, NegativeX, Z, 180)]
        public void SignedAngleToTest(string fromString, string toString, string axisString, double degreeAngle)
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
            Angle angle = Angle.FromDegrees(rotationInDegrees);
            Vector3D rotated = new Vector3D(Matrix3D.RotationAroundZAxis(angle).Multiply(vector.ToDenseVector()));
            var actual = vector.SignedAngleTo(rotated, Vector3D.Parse(Z).Normalize());
            Assert.AreEqual(rotationInDegrees, actual.Degrees, 1E-6);
        }

        [TestCase(X, Z, 90, Y)]
        public void RotateTest(string vs, string avs, double deg, string evs)
        {
            var v = Vector3D.Parse(vs);
            var aboutvector = Vector3D.Parse(avs);
            var rotated = v.Rotate(aboutvector, Angle.FromDegrees(deg));
            var expected = Vector3D.Parse(evs);
            LinearAlgebraAssert.AreEqual(expected, rotated, 1E-6);
        }

        [TestCase("X", X)]
        [TestCase("Y", Y)]
        [TestCase("Z", Z)]
        public void SignedAngleTo_Itself(string axisDummy, string aboutDoubles)
        {
            Vector3D vector = new Vector3D(1, 1, 1);
            Vector3D aboutVector = Vector3D.Parse(aboutDoubles);
            var angle = vector.SignedAngleTo(vector, aboutVector.Normalize());
            Assert.AreEqual(0, angle.Degrees, 1E-6);
        }

        [Test]
        public void SignedAngleToBug()
        {
            var ninetyDegAngle = new Vector3D(0, 1, 0);
        }

        [TestCase(X, Y, 90)]
        [TestCase(Y, X, 90)]
        [TestCase(X, Z, 90)]
        [TestCase(Z, X, 90)]
        [TestCase(Y, Z, 90)]
        [TestCase(Z, Y, 90)]
        [TestCase(X, X, 0)]
        [TestCase(Y, Y, 0)]
        [TestCase(Z, Z, 0)]
        [TestCase(X, NegativeY, 90)]
        [TestCase(Y, NegativeY, 180)]
        [TestCase(Z, NegativeZ, 180)]
        [TestCase("1; 1; 0", X, 45)]
        [TestCase("1; 1; 0", Y, 45)]
        [TestCase("1; 1; 0", Z, 90)]
        [TestCase("2; 2; 0", "0; 0; 2", 90)]
        [TestCase("1; 1; 1", X, 54.74)]
        [TestCase("1; 1; 1", Y, 54.74)]
        [TestCase("1; 1; 1", Z, 54.74)]
        public void AngleToTest(string v1s, string v2s, double expected)
        {
            var v1 = Vector3D.Parse(v1s);
            var v2 = Vector3D.Parse(v2s);
            var angle = v1.AngleTo(v2);
            Assert.AreEqual(expected, angle.Degrees, 1E-2);
        }

        [TestCase("5; 0; 0", "1; 0 ; 0")]
        [TestCase("-5; 0; 0", "-1; 0 ; 0")]
        [TestCase("0; 5; 0", "0; 1 ; 0")]
        [TestCase("0; -5; 0", "0; -1 ; 0")]
        [TestCase("0; 0; 5", "0; 0 ; 1")]
        [TestCase("0; 0; -5", "0; 0 ; -1")]
        [TestCase("2; 2; 2", "0,577350269189626; 0,577350269189626; 0,577350269189626")]
        [TestCase("-2; 15; 2", "-0,131024356416084; 0,982682673120628; 0,131024356416084")]
        [TestCase("0; 0; 0", "0; 0 ; 0", ExpectedException = typeof(ArgumentException))]
        public void Normalize(string vs, string evs)
        {
            var vector = Vector3D.Parse(vs);
            var uv = vector.Normalize();
            var expected = UnitVector3D.Parse(evs);
            LinearAlgebraAssert.AreEqual(expected, uv, 1E-6);
        }

        [TestCase("1, -1, 10", 5, "5, -5, 50")]
        public void Scale(string vs, double s, string evs)
        {
            var v = Vector3D.Parse(vs);
            Vector3D actual = v.ScaleBy(s);
            LinearAlgebraAssert.AreEqual(Vector3D.Parse(evs), actual, 1e-6);
        }

        [TestCase("5;0;0", 5)]
        [TestCase("-5;0;0", 5)]
        [TestCase("-3;0;4", 5)]
        public void Length(string vectorString, double length)
        {
            var vector = Vector3D.Parse(vectorString);
            Assert.AreEqual(length, vector.Length);
        }

        [TestCase("1.0 , 2.5,3.3", new double[] { 1, 2.5, 3.3 })]
        [TestCase("1,0 ; 2,5;3,3", new double[] { 1, 2.5, 3.3 })]
        [TestCase("1.0 ; 2.5;3.3", new double[] { 1, 2.5, 3.3 })]
        [TestCase("1.0,2.5,-3.3", new double[] { 1, 2.5, -3.3 })]
        [TestCase("1;2;3", new double[] { 1, 2, 3 })]
        public void ParseTest(string vs, double[] ep)
        {
            Vector3D point3D = Vector3D.Parse(vs);
            Vector3D expected = new Vector3D(ep);
            LinearAlgebraAssert.AreEqual(point3D, expected, 1e-9);
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
        public void IsParallelToTest(string vector1, string vector2, bool isParalell)
        {
            var firstVector = Vector3D.Parse(vector1);
            var secondVector = Vector3D.Parse(vector2);
            Assert.AreEqual(isParalell, firstVector.IsParallelTo(secondVector, 1E-6));
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
        public void IsPerpendicilarToTest(string v1s, string v2s, bool expected)
        {
            var v1 = Vector3D.Parse(v1s);
            var v2 = Vector3D.Parse(v2s);
            Assert.AreEqual(expected, v1.IsPerpendicularTo(v2));
        }

        [TestCase("1,2,-3", 3, "3,6,-9")]
        public void Multiply(string vectorAsString, double mulitplier, string expected)
        {
            var vector = Vector3D.Parse(vectorAsString);
            LinearAlgebraAssert.AreEqual(Vector3D.Parse(expected), mulitplier * vector, 1e-6);
            LinearAlgebraAssert.AreEqual(Vector3D.Parse(expected), mulitplier * vector, 1e-6);
        }

        [Test]
        public void SerializeDeserialize()
        {
            var v = new Vector3D(1, 2, 3);
            var roundTrip = AssertXml.XmlSerializerRoundTrip(v, @"<Vector3D X=""1"" Y=""2"" Z=""3"" />");
            LinearAlgebraAssert.AreEqual(v, roundTrip);
        }
    }
}
