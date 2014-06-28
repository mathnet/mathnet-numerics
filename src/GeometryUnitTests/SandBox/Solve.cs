namespace MathNet.GeometryUnitTests.SandBox
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Geometry;
    using Numerics.LinearAlgebra;
    using Numerics.LinearAlgebra.Double;
    using NUnit.Framework;
    [Explicit]
    public class Solve
    {
        DenseMatrix _a;
        DenseMatrix _x;
        [SetUp]
        public void SetUp()
        {
        //[TestCase(, )]

            _a = CoordinateSystem.Parse("o:{0, 0, 0} x:{10, 0, 0} y:{0, 1, 0} z:{0, 0, 1}");

            _x = new DenseMatrix(2, 2);
            _x[0, 0] = 2;
            _x[0, 1] = 1;
            _x[1, 0] = 4;
            _x[1, 1] = 1;
        }
        [Test]
        public void Qr()
        {
            var fcs = CoordinateSystem.Parse("o:{1, 0, 0} x:{0.1, 0, 0} y:{0, 1, 0} z:{0, 0, 1}");
            var tcs = CoordinateSystem.Parse("o:{0, 0, 0} x:{9, 0, 0} y:{0, 1, 0} z:{0, 0, 1}");
            var m = tcs.Multiply(fcs.Inverse());
            var multiply = m.Multiply(fcs);
            Dump(multiply,tcs);
        }

        private void Dump(params Matrix<double>[] ms)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < ms.First().RowCount; i++)
            {
                foreach (var m in ms)
                {
                    sb.Append("  ");
                    for (int j = 0; j < m.ColumnCount; j++)
                    {
                        sb.Append(m[i, j].ToString("F2") + " ");
                    }
                }

                sb.AppendLine();
            }
            Console.Write(sb);
            Console.WriteLine();
        }
    }
}
