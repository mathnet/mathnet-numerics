namespace MathNet.GeometryUnitTests
{
    using System.IO;
    using System.Xml;
    using Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class UnitVector3DTests
    {
        const string X = "1; 0 ; 0";
        const string Y = "0; 1; 0";
        const string Z = "0; 0; 1";
        const string NegativeX = "-1; 0; 0";
        const string NegativeY = "0; -1; 0";
        const string NegativeZ = "0; 0; -1";
        const string ZeroPoint = "0; 0; 0";
        [TestCase("1; 0; 0", 5, "5; 0; 0")]
        [TestCase("1; 0; 0", -5, "-5; 0; 0")]
        [TestCase("-1; 0; 0", 5, "-5; 0; 0")]
        [TestCase("-1; 0; 0", -5, "5; 0; 0")]
        [TestCase("0; 1; 0", 5, "0; 5; 0")]
        [TestCase("0; 0; 1", 5, "0; 0; 5")]
        public void Scale(string ivs, double s, string exs)
        {
            var uv = UnitVector3D.Parse(ivs);
            var v = uv.ScaleBy(s);
            AssertGemoetry.AreEqual(Vector3D.Parse(exs), v, float.Epsilon);
        }

        [TestCase("-1, 0, 0", null, "(-1, 0, 0)", 1e-4)]
        [TestCase("-1, 0, 1e-4", "F2", "(-1.00, 0.00, 0.00)", 1e-3)]
        public void ToString(string vs, string format, string expected, double tolerance)
        {
            var v = UnitVector3D.Parse(vs);
            string actual = v.ToString(format);
            Assert.AreEqual(expected, actual);
            AssertGemoetry.AreEqual(v, UnitVector3D.Parse(actual), tolerance);
        }

        [TestCase("1, -2, 3", false, @"<UnitVector3D X=""0.2672612419124244"" Y=""-0.53452248382484879"" Z=""0.80178372573727319"" />")]
        [TestCase("1, -2, 3", true, @"<UnitVector3D><X>0.267261241912424</X><Y>-0.534522483824849</Y><Z>0.801783725737273</Z></UnitVector3D>")]
        public void XmlRoundTrips(string uvs, bool asElements, string xml)
        {
            var uv = UnitVector3D.Parse(uvs);
            uv.SerializeAsElements = asElements;
            AssertXml.XmlRoundTrips(uv, xml, (e, a) => AssertGemoetry.AreEqual(e, a));
            var actual = UnitVector3D.ReadFrom(XmlReader.Create(new StringReader(xml)));
            AssertGemoetry.AreEqual(uv, actual);
        }

        [Test]
        public void CastTest()
        {
            Assert.Inconclusive("Is it possible to implement this in a good way?");
            //Vector<double> vector = new DenseVector(new[] { 1.0, 1.0, 0.0 });
            //UnitVector3D casted = (UnitVector3D)vector;
            //Assert.AreEqual(1, casted.Length, 0.00001);
            //Vector<double> castback = casted;

            //Vector<double> vectorTooLong = new DenseVector(new[] { 1.0, 0.0, 0.0, 0.0 });
            //UnitVector3D vectorFail;
            //Assert.Throws<InvalidCastException>(() => vectorFail = (UnitVector3D)vectorTooLong);
        }

        [TestCase("1,0,0", 3, "3,0,0")]
        public void MultiplyTest(string unitVectorAsString, double multiplier, string expected)
        {
            UnitVector3D unitVector3D = UnitVector3D.Parse(unitVectorAsString);
            Assert.AreEqual(Vector3D.Parse(expected), multiplier * unitVector3D);
        }
    }
}
