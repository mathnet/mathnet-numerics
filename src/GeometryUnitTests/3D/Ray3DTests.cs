namespace GeometryUnitTests
{
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Geometry;
    using NUnit.Framework;

    [TestFixture]
    public class Ray3DTests
    {
        //[TestCase(new double[]{0,0,0}, new double[]{0,0,1},new double[]{1,0,0})]
        [Test]
        public void LineToTest()
        {
            var ray = new Ray3D(new Point3D(0, 0, 0), UnitVector3D.ZAxis);
            var point3D = new Point3D(1, 0, 0);
            Line3D line3DTo = ray.LineTo(point3D);
            LinearAlgebraAssert.AreEqual(new Point3D(0, 0, 0), line3DTo.StartPoint, float.Epsilon);
            LinearAlgebraAssert.AreEqual(point3D, line3DTo.EndPoint, float.Epsilon);
        }

        [TestCase("1, 2, 3", "-1, 2, 3", false, @"<Ray3D><ThroughPoint X=""1"" Y=""2"" Z=""3"" /><Direction X=""1"" Y=""0"" Z=""0"" /></Ray3D>")]
        public void XmlTests(string ps, string vs, bool asElements, string xml)
        {
            var ray = new Ray3D(Point3D.Parse(ps), UnitVector3D.Parse(vs));
            AssertXml.XmlRoundTrips(ray, xml, (e, a) => LinearAlgebraAssert.AreEqual(e, a));
        }

        [Test]
        public void BinaryRountrip()
        {
            var v = new Ray3D(new Point3D(1, 2, -3), new UnitVector3D(1, 2, 3));
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, v);
                ms.Flush();
                ms.Position = 0;
                var roundTrip = (Ray3D)formatter.Deserialize(ms);
                LinearAlgebraAssert.AreEqual(v, roundTrip);
            }
        }
    }
}
