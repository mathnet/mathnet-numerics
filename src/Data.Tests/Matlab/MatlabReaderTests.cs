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

using System.Collections.Generic;
using System.IO;
using System.Numerics;
using MathNet.Numerics.Data.Matlab;
using NUnit.Framework;

namespace MathNet.Numerics.Data.Tests.Matlab
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

        /// <summary>                                                                                                
        /// Can read structures with nested structures, cell matrices and normal matrices
        /// </summary>                                                                                               
        [Test]
        public void CanReadNestedStructure()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.struct-nested.mat"))
            {
                var fileEntries = MatlabReader.List(stream);

                var result = MatlabReader.NonNumeric(fileEntries.Find(o => o.Name == "s"));

                // we can use the methods provided by OneOf to work on the returned NestedObject
                result.Switch(
                    s => Assert.IsTrue(s.ContainsKey("a")),
                    _ => Assert.Fail("Wrong type, Expected MatlabStructure but got CellMatrix"),
                    _ => Assert.Fail("Wrong type, Expected MatlabStructure but got CharMatrix"),
                    _ => Assert.Fail("Wrong type, Expected MatlabStructure but got Matrix<double>"));


                // or we can just use it direclty as a MatlabStructure
                var structure = result.AsT0;

                Assert.IsTrue(structure.ContainsKey("a"));// numeric matrix
                Assert.IsTrue(structure.ContainsKey("b"));// complex numeric matrix
                Assert.IsTrue(structure.ContainsKey("c"));// char matrix
                Assert.IsTrue(structure.ContainsKey("d"));// cell matrix
                Assert.IsTrue(structure.ContainsKey("e"));// structure

                // numeric matrices need compile time type information so they still need to be unpacked
                var numeric = structure["a"].AsT3;
                var numericUnpacked = MatlabReader.Unpack<double>(numeric);
                Assert.AreEqual(3, numericUnpacked.ColumnCount);
                Assert.AreEqual(1, numericUnpacked.RowCount);

                Assert.AreEqual(10, numericUnpacked[0, 0]);
                Assert.AreEqual(20, numericUnpacked[0, 1]);
                Assert.AreEqual(30, numericUnpacked[0, 2]);

                var complex = structure["b"].AsT3;
                var complexUnpacked = MatlabReader.Unpack<Complex>(complex);
                Assert.AreEqual(3, complexUnpacked.ColumnCount);
                Assert.AreEqual(1, complexUnpacked.RowCount);

                Assert.AreEqual(new Complex(1,2), complexUnpacked[0, 0]);
                Assert.AreEqual(new Complex(2,3), complexUnpacked[0, 1]);
                Assert.AreEqual(new Complex(3,4), complexUnpacked[0, 2]);

                // contains chars a,b and c in a single row (null terminated)
                var chars = structure["c"].AsT2;
                //convenient for use as string
                Assert.AreEqual("abc\0", chars.ConcatRows()[0]);

                var cells = structure["d"].AsT1;
                Assert.AreEqual(2, cells.Data.Length);

                var cell1 = MatlabReader.Unpack<double>(cells.Data[0, 0].AsT3);
                Assert.AreEqual(1, cell1.ColumnCount);
                Assert.AreEqual(1, cell1.RowCount);
                Assert.AreEqual(13, cell1[0, 0]);

                var cell2 = MatlabReader.Unpack<double>(cells.Data[0, 1].AsT3);
                Assert.AreEqual(1, cell2.ColumnCount);
                Assert.AreEqual(1, cell2.RowCount);
                Assert.AreEqual(12, cell2[0, 0]);

                var nestedStructure = structure["e"].AsT0;

                var aa = nestedStructure["aa"].AsT3;
                var aaUnpacked = MatlabReader.Unpack<double>(aa);
                Assert.AreEqual(1, aaUnpacked.ColumnCount);
                Assert.AreEqual(1, aaUnpacked.RowCount);
                Assert.AreEqual(23, aaUnpacked[0, 0]);

                var b = nestedStructure["b"].AsT3;
                var bUnpacked = MatlabReader.Unpack<double>(b);
                Assert.AreEqual(1, bUnpacked.ColumnCount);
                Assert.AreEqual(1, bUnpacked.RowCount);
                Assert.AreEqual(34, bUnpacked[0, 0]);
            }
        }

        [Test]
        public void CanReadNestedCells()
        {
            using (var stream = TestData.Data.ReadStream("Matlab.cell-array-nested.mat"))
            {
                var fileEntries = MatlabReader.List(stream);

                var result = MatlabReader.NonNumeric(fileEntries.Find(o => o.Name == "c"));

                // this cell matrix contains one row with in each cell:
                // 1x3 cell matrix
                // 1x4 numeric matrix
                // 3x3 numeric matrix
                // char matrix 'abcd'
                // 1x3 complex matrix
                // structure
                var cells = result.AsT1;

                // 1x3 cell matrix
                // each cell is char array, contents a,b,c
                var nestedCells = cells.Data[0, 0].AsT1;
                Assert.AreEqual(3, nestedCells.Data.Length);

                string[] expectedString = new string[] { "a\0", "b\0", "c\0" };
                for(int i = 0; i<3; i++)
                {
                    var singleChar = nestedCells.Data[0,i].AsT2;
                    Assert.AreEqual(1, singleChar.Data.Length);
                    Assert.AreEqual(expectedString[i], singleChar.Data[0, 0]);
                }

                // 1x4 numeric matrix
                var fourNumeric = cells.Data[0, 1].AsT3;
                var fourNumericUnpacked = MatlabReader.Unpack<double>(fourNumeric);
                Assert.AreEqual(1, fourNumericUnpacked.RowCount);
                Assert.AreEqual(4, fourNumericUnpacked.ColumnCount);

                for(int i = 0; i<4; i++)
                {
                    Assert.AreEqual(i + 1, fourNumericUnpacked[0, i]);
                }

                // 3x3 numeric matrix
                var threeNumeric = cells.Data[0, 2].AsT3;
                var threeNumericUnpacked = MatlabReader.Unpack<double>(threeNumeric);
                Assert.AreEqual(3, threeNumericUnpacked.RowCount);
                Assert.AreEqual(3, threeNumericUnpacked.ColumnCount);

                double[,] expected = new double[3, 3]
                {
                    { 0.0960473, 0.102643, 0.592588 },
                    { 0.796906, 0.477299, 0.147533 },
                    { 0.13894, 0.0817299, 0.906656 }
                };

                for(int row = 0; row<3; row++)
                {
                    for(int col = 0; col<3; col++)
                    {
                        Assert.AreEqual(expected[row, col], threeNumericUnpacked[row, col], 0.000001);
                    }
                }

                // char matrix
                var nestedChars = cells.Data[0, 3].AsT2;
                Assert.AreEqual(4, nestedChars.Data.Length);
                Assert.AreEqual("abcd", nestedChars.ConcatRows()[0]);

                // 1x3 complex matrix
                var complex = cells.Data[0, 4].AsT3;
                var complexUnpacked = MatlabReader.Unpack<Complex>(complex);
                Assert.AreEqual(1, complexUnpacked.RowCount);
                Assert.AreEqual(3, complexUnpacked.ColumnCount);

                for(int i = 0; i<3; i++)
                {
                    Assert.AreEqual(new Complex(i + 1, i + 2), complexUnpacked[0, i]);
                }

                // structure
                var structure = cells.Data[0, 5].AsT0;

                Assert.AreEqual(2, structure.Count);

                var fieldA = structure["a"].AsT3;
                var fieldAUnpacked = MatlabReader.Unpack<double>(fieldA);
                Assert.AreEqual(1, fieldAUnpacked.RowCount);
                Assert.AreEqual(1, fieldAUnpacked.ColumnCount);
                Assert.AreEqual(1, fieldAUnpacked[0, 0]);

                var fieldAbg = structure["abg"].AsT3;
                var fieldAbgUnpacked = MatlabReader.Unpack<double>(fieldAbg);
                Assert.AreEqual(1, fieldAbgUnpacked.RowCount);
                Assert.AreEqual(1, fieldAbgUnpacked.ColumnCount);
                Assert.AreEqual(2, fieldAbgUnpacked[0, 0]);
            }
        }
    }
}
