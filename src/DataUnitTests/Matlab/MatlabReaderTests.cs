// <copyright file="MatlabReaderTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System.Numerics;
using MathNet.Numerics.Data.Matlab;
using NUnit.Framework;

namespace MathNet.Numerics.Data.UnitTests.Matlab
{
    /// <summary>
    /// Matlab matrix reader test.
    /// </summary>
    [TestFixture]
    public class MatlabReaderTests
    {
        /// <summary>
        /// Can read all matrices.
        /// </summary>
        [Test]
        public void CanReadAllMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<double>(stream);
                Assert.AreEqual(30, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Double.DenseMatrix), matrix.Value.GetType());
                }
            }
        }

        /// <summary>
        /// Can read first matrix.
        /// </summary>
        [Test]
        public void CanReadFirstMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.A.mat"))
            {
                var matrix = MatlabReader.Read<double>(stream);
                Assert.AreEqual(100, matrix.RowCount);
                Assert.AreEqual(100, matrix.ColumnCount);
                Assert.AreEqual(typeof (LinearAlgebra.Double.DenseMatrix), matrix.GetType());
                AssertHelpers.AlmostEqual(100.108979553704, matrix.FrobeniusNorm(), 5);
            }
        }

        /// <summary>
        /// Can read named matrices.
        /// </summary>
        [Test]
        public void CanReadNamedMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<double>(stream, "Ad", "Au64");
                Assert.AreEqual(2, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Double.DenseMatrix), matrix.Value.GetType());
                }
            }
        }

        /// <summary>
        /// Can read named matrix.
        /// </summary>
        [Test]
        public void CanReadNamedMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<double>(stream, "Ad");
                Assert.AreEqual(1, matrices.Count);
                var ad = matrices["Ad"];
                Assert.AreEqual(100, ad.RowCount);
                Assert.AreEqual(100, ad.ColumnCount);
                AssertHelpers.AlmostEqual(100.431635988639, ad.FrobeniusNorm(), 5);
                Assert.AreEqual(typeof (LinearAlgebra.Double.DenseMatrix), ad.GetType());
            }
        }

        /// <summary>
        /// Can read named sparse matrix.
        /// </summary>
        [Test]
        public void CanReadNamedSparseMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.sparse-small.mat"))
            {
                var matrix = MatlabReader.Read<double>(stream, "S");
                Assert.AreEqual(100, matrix.RowCount);
                Assert.AreEqual(100, matrix.ColumnCount);
                Assert.AreEqual(typeof (LinearAlgebra.Double.SparseMatrix), matrix.GetType());
                AssertHelpers.AlmostEqual(17.6385090630805, matrix.FrobeniusNorm(), 12);
            }
        }

        /// <summary>
        /// Can read all complex matrices.
        /// </summary>
        [Test]
        public void CanReadComplexAllMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.complex.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex>(stream);
                Assert.AreEqual(3, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Complex.DenseMatrix), matrix.Value.GetType());
                }

                var a = matrices["a"];

                Assert.AreEqual(100, a.RowCount);
                Assert.AreEqual(100, a.ColumnCount);
                AssertHelpers.AlmostEqual(27.232498979698409, a.L2Norm(), 13);
            }
        }

        /// <summary>
        /// Can read sparse complex matrices.
        /// </summary>
        [Test]
        public void CanReadSparseComplexAllMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.sparse_complex.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex>(stream);
                Assert.AreEqual(3, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Complex.SparseMatrix), matrix.Value.GetType());
                }

                var a = matrices["sa"];

                Assert.AreEqual(100, a.RowCount);
                Assert.AreEqual(100, a.ColumnCount);
                AssertHelpers.AlmostEqual(13.223654390985379, a.L2Norm(), 13);
            }
        }

        /// <summary>
        /// Can read non-complex matrices.
        /// </summary>
        [Test]
        public void CanReadNonComplexAllMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex>(stream);
                Assert.AreEqual(30, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Complex.DenseMatrix), matrix.Value.GetType());
                }
            }
        }

        /// <summary>
        /// Can read non-complex first matrix.
        /// </summary>
        [Test]
        public void CanReadNonComplexFirstMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.A.mat"))
            {
                var matrix = MatlabReader.Read<Complex>(stream);
                Assert.AreEqual(100, matrix.RowCount);
                Assert.AreEqual(100, matrix.ColumnCount);
                Assert.AreEqual(typeof (LinearAlgebra.Complex.DenseMatrix), matrix.GetType());
                AssertHelpers.AlmostEqual(100.108979553704, matrix.FrobeniusNorm(), 13);
            }
        }

        /// <summary>
        /// Can read non-complex named matrices.
        /// </summary>
        [Test]
        public void CanReadNonComplexNamedMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex>(stream, "Ad", "Au64");
                Assert.AreEqual(2, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Complex.DenseMatrix), matrix.Value.GetType());
                }
            }
        }

        /// <summary>
        /// Can read non-complex named matrix.
        /// </summary>
        [Test]
        public void CanReadNonComplexNamedMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex>(stream, "Ad");
                Assert.AreEqual(1, matrices.Count);
                var ad = matrices["Ad"];
                Assert.AreEqual(100, ad.RowCount);
                Assert.AreEqual(100, ad.ColumnCount);
                AssertHelpers.AlmostEqual(100.431635988639, ad.FrobeniusNorm(), 13);
                Assert.AreEqual(typeof (LinearAlgebra.Complex.DenseMatrix), ad.GetType());
            }
        }

        /// <summary>
        /// Can read non-complex named sparse matrix.
        /// </summary>
        [Test]
        public void CanReadNonComplexNamedSparseMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.sparse-small.mat"))
            {
                var matrix = MatlabReader.Read<Complex>(stream, "S");
                Assert.AreEqual(100, matrix.RowCount);
                Assert.AreEqual(100, matrix.ColumnCount);
                Assert.AreEqual(typeof (LinearAlgebra.Complex.SparseMatrix), matrix.GetType());
                AssertHelpers.AlmostEqual(17.6385090630805, matrix.FrobeniusNorm(), 12);
            }
        }

        /// <summary>
        /// Can read all complex matrices.
        /// </summary>
        [Test]
        public void CanReadComplex32AllMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.complex.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex32>(stream);
                Assert.AreEqual(3, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Complex32.DenseMatrix), matrix.Value.GetType());
                }

                var a = matrices["a"];

                Assert.AreEqual(100, a.RowCount);
                Assert.AreEqual(100, a.ColumnCount);
                AssertHelpers.AlmostEqual(27.232498979698409, a.L2Norm(), 5);
            }
        }

        /// <summary>
        /// Can read sparse complex matrices.
        /// </summary>
        [Test]
        public void CanReadSparseComplex32AllMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.sparse_complex.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex32>(stream);
                Assert.AreEqual(3, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Complex32.SparseMatrix), matrix.Value.GetType());
                }

                var a = matrices["sa"];

                Assert.AreEqual(100, a.RowCount);
                Assert.AreEqual(100, a.ColumnCount);
                AssertHelpers.AlmostEqual(13.223654390985379, a.L2Norm(), 5);
            }
        }

        /// <summary>
        /// Can read non-complex matrices.
        /// </summary>
        [Test]
        public void CanReadNonComplex32AllMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex32>(stream);
                Assert.AreEqual(30, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Complex32.DenseMatrix), matrix.Value.GetType());
                }
            }
        }

        /// <summary>
        /// Can read non-complex first matrix.
        /// </summary>
        [Test]
        public void CanReadNonComplex32FirstMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.A.mat"))
            {
                var matrix = MatlabReader.Read<Complex32>(stream);
                Assert.AreEqual(100, matrix.RowCount);
                Assert.AreEqual(100, matrix.ColumnCount);
                Assert.AreEqual(typeof (LinearAlgebra.Complex32.DenseMatrix), matrix.GetType());
                AssertHelpers.AlmostEqual(100.108979553704, matrix.FrobeniusNorm(), 6);
            }
        }

        /// <summary>
        /// Can read non-complex named matrices.
        /// </summary>
        [Test]
        public void CanReadNonComplex32NamedMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex32>(stream, "Ad", "Au64");
                Assert.AreEqual(2, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Complex32.DenseMatrix), matrix.Value.GetType());
                }
            }
        }

        /// <summary>
        /// Can read non-complex named matrix.
        /// </summary>
        [Test]
        public void CanReadNonComplex32NamedMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<Complex32>(stream, "Ad");
                Assert.AreEqual(1, matrices.Count);
                var ad = matrices["Ad"];
                Assert.AreEqual(100, ad.RowCount);
                Assert.AreEqual(100, ad.ColumnCount);
                AssertHelpers.AlmostEqual(100.431635988639, ad.FrobeniusNorm(), 6);
                Assert.AreEqual(typeof (LinearAlgebra.Complex32.DenseMatrix), ad.GetType());
            }
        }

        /// <summary>
        /// Can read non-complex named sparse matrix.
        /// </summary>
        [Test]
        public void CanReadNonComplex32NamedSparseMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.sparse-small.mat"))
            {
                var matrix = MatlabReader.Read<Complex32>(stream, "S");
                Assert.AreEqual(100, matrix.RowCount);
                Assert.AreEqual(100, matrix.ColumnCount);
                Assert.AreEqual(typeof (LinearAlgebra.Complex32.SparseMatrix), matrix.GetType());
                AssertHelpers.AlmostEqual(17.6385090630805, matrix.FrobeniusNorm(), 6);
            }
        }

        /// <summary>
        /// Can read all matrices.
        /// </summary>
        [Test]
        public void CanReadFloatAllMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<float>(stream);
                Assert.AreEqual(30, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Single.DenseMatrix), matrix.Value.GetType());
                }
            }
        }

        /// <summary>
        /// Can read first matrix.
        /// </summary>
        [Test]
        public void CanReadFloatFirstMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.A.mat"))
            {
                var matrix = MatlabReader.Read<float>(stream);
                Assert.AreEqual(100, matrix.RowCount);
                Assert.AreEqual(100, matrix.ColumnCount);
                Assert.AreEqual(typeof (LinearAlgebra.Single.DenseMatrix), matrix.GetType());
                AssertHelpers.AlmostEqual(100.108979553704f, matrix.FrobeniusNorm(), 6);
            }
        }

        /// <summary>
        /// Can read named matrices.
        /// </summary>
        [Test]
        public void CanReadFloatNamedMatrices()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<float>(stream, "Ad", "Au64");
                Assert.AreEqual(2, matrices.Count);
                foreach (var matrix in matrices)
                {
                    Assert.AreEqual(typeof (LinearAlgebra.Single.DenseMatrix), matrix.Value.GetType());
                }
            }
        }

        /// <summary>
        /// Can read named matrix.
        /// </summary>
        [Test]
        public void CanReadFloatNamedMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.collection.mat"))
            {
                var matrices = MatlabReader.ReadAll<float>(stream, "Ad");
                Assert.AreEqual(1, matrices.Count);
                var ad = matrices["Ad"];
                Assert.AreEqual(100, ad.RowCount);
                Assert.AreEqual(100, ad.ColumnCount);
                AssertHelpers.AlmostEqual(100.431635988639f, ad.FrobeniusNorm(), 6);
                Assert.AreEqual(typeof (LinearAlgebra.Single.DenseMatrix), ad.GetType());
            }
        }

        /// <summary>
        /// Can read named sparse matrix.
        /// </summary>
        [Test]
        public void CanReadFloatNamedSparseMatrix()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.sparse-small.mat"))
            {
                var matrix = MatlabReader.Read<float>(stream, "S");
                Assert.AreEqual(100, matrix.RowCount);
                Assert.AreEqual(100, matrix.ColumnCount);
                Assert.AreEqual(typeof (LinearAlgebra.Single.SparseMatrix), matrix.GetType());
                AssertHelpers.AlmostEqual(17.6385090630805f, matrix.FrobeniusNorm(), 6);
            }
        }
    }
}
