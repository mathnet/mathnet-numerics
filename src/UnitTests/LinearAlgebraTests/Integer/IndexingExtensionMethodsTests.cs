// <copyright file="IndexingExtensionMethodsTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2015 Math.NET
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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Integer
{
    using System;
    using System.Collections.Generic;
    using MathNet.Numerics.LinearAlgebra;
    using NUnit.Framework;

    /// <summary>
    /// Class with the indexing extension methods tests.
    /// </summary>
    public class IndexingExtensionMethodsTests
    {
        /// <summary>
        /// Gets or sets test matrix instances to use.
        /// </summary>
        private Dictionary<string, Matrix<double>> TestMatrices { get; set; }

        /// <summary>
        /// Gets or sets test vector instances to use.
        /// </summary>
        private Dictionary<string, Vector<double>> TestVectors { get; set; }

        /// <summary>
        /// Gets or sets test index vectors instances to use.
        /// </summary>
        protected Dictionary<string, Vector<int>> IndexVectors { get; set; }
        
        /// <summary>
        /// Setup test matrices and vectors.
        /// </summary>
        [SetUp]
        public virtual void SetupMatricesAndVectors()
        {
            var testData2D = new Dictionary<string, double[,]>
            {
                { "Source4x4", new double[,] { { 100, 101, 102, 103 }, 
                                               { 110, 111, 112, 113 }, 
                                               { 120, 121, 122, 123 }, 
                                               { 130, 131, 132, 133 } } },
                { "SubMatrix3x3", new double[,] { { 100, 101, 103 }, 
                                                  { 120, 121, 123 }, 
                                                  { 130, 131, 133 } } },
                { "Oversize6x6", new double[,] { { 130, 130, 131, 131, 132, 132 },
                                                 { 120, 120, 121, 121, 122, 122 },
                                                 { 110, 110, 111, 111, 112, 112 },
                                                 { 130, 130, 131, 131, 132, 132 },
                                                 { 120, 120, 121, 121, 122, 122 },
                                                 { 110, 110, 111, 111, 112, 112 } } },
                { "NewValues3x3", new double[,] { { 200, 201, 203 }, 
                                                  { 220, 221, 223 }, 
                                                  { 230, 231, 233 } } },
                { "Updated4x4", new double[,] { { 200, 201, 102, 203 }, 
                                                { 110, 111, 112, 113 }, 
                                                { 220, 221, 122, 223 }, 
                                                { 230, 231, 132, 233 } } },
                { "SubVectorRow1x3", new double[,] { { 110, 111, 113 } } },     // row == 1, SubMatrixColumns
                { "UpdatedSubVectorRow4x4", new double[,] { { 100, 101, 102, 103 }, 
                                                            { 210, 211, 112, 213 }, 
                                                            { 120, 121, 122, 123 }, 
                                                            { 130, 131, 132, 133 } } },
                { "SubVectorColumn3x1", new double[,] { { 102 },
                                                        { 122 }, 
                                                        { 132 } } },            // SubMatrixRows, column == 2
                { "UpdatedSubVectorColumn4x4", new double[,] { { 100, 101, 202, 103 }, 
                                                               { 110, 111, 112, 113 }, 
                                                               { 120, 121, 222, 123 }, 
                                                               { 130, 131, 232, 133 } } },
            };

            TestMatrices = new Dictionary<string, Matrix<double>>();
            foreach (var name in testData2D.Keys)
            {
                TestMatrices.Add(name, Matrix<double>.Build.DenseOfArray(testData2D[name]));
            }

            var testDataV = new Dictionary<string, double[]>
            {
                { "Source4", new double[] { 100, 101, 102, 103 } },
                { "SubVector3", new double[] { 100, 101, 103 } },
                { "Oversize6", new double[] { 130, 130, 131, 131, 132, 132 } },
                { "Updated4", new double[] { 200, 201, 102, 203 } },
                { "SubVectorRow3", new double[] { 110, 111, 113 } },            // row == 1, SubMatrixColumns
                { "NewValuesSubVectorRow3", new double[] { 210, 211, 213 } },
                { "SubVectorColumn3", new double[] { 102, 122, 132 } },         // SubMatrixRows, column == 2
                { "NewValuesSubVectorColumn3", new double[] { 202, 222, 232 } },         // SubMatrixRows, column == 2
                { "NewValues3", new double[] { 200, 201, 203 } },
                { "NewValues7", new double[] { 200, 201, 202, 203, 204, 205, 206 } },
            };

            TestVectors = new Dictionary<string, Vector<double>>();
            foreach (var name in testDataV.Keys)
            {
                TestVectors.Add(name, Vector<double>.Build.DenseOfArray(testDataV[name]));
            }

            var testDataIndexes = new Dictionary<string, int[]> 
            {
                {"SubMatrixRows", new int[] { 0, 2, 3 }},
                {"SubMatrixColumns", new int[] { 0, 1, 3 }},
                {"OversizeRows", new int[] { 3, 2, 1, 3, 2, 1 }},
                {"OversizeColumns", new int[] { 0, 0, 1, 1, 2, 2 }},
                {"SubVector", new int[] { 0, 1, 3 }},
                {"Oversize", new int[] { 0, 0, 1, 1, 2, 2 }},
            };

            IndexVectors = new Dictionary<string, Vector<int>>();
            foreach (var name in testDataIndexes.Keys)
            {
                IndexVectors.Add(name, Vector<int>.Build.DenseOfArray(testDataIndexes[name]));
            }

        }

        // Unchecked

        /// <summary>
        /// Can get submatrix unchecked.
        /// </summary>
        [Test]
        public void CanGetSubMatrixUnchecked()
        {
            var expected = TestMatrices["SubMatrix3x3"];
            var source = TestMatrices["Source4x4"];
            var selectRows = IndexVectors["SubMatrixRows"];
            var selectColumns = IndexVectors["SubMatrixColumns"];
            var actual = source.At(selectRows, selectColumns);
            
            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can get oversize submatrix unchecked.
        /// </summary>
        [Test]
        public void CanGetOversizeSubMatrixUnchecked()
        {
            var expected = TestMatrices["Oversize6x6"];
            var source = TestMatrices["Source4x4"];
            var selectRows = IndexVectors["OversizeRows"];
            var selectColumns = IndexVectors["OversizeColumns"];
            var actual = source.At(selectRows, selectColumns);
            
            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can update submatrix unchecked.
        /// </summary>
        [Test]
        public void CanUpdateSubMatrixUnchecked()
        {
            var expected = TestMatrices["Updated4x4"];
            var matrix = TestMatrices["Source4x4"].Clone();
            var newValues = TestMatrices["NewValues3x3"];
            var selectRows = IndexVectors["SubMatrixRows"];
            var selectColumns = IndexVectors["SubMatrixColumns"];
            matrix.At(selectRows, selectColumns, newValues);
            
            AssertHelpers.AlmostEqualRelative(expected, matrix, 6);
        }

        /// <summary>
        /// Can get subvector of row as matrix unchecked.
        /// </summary>
        [Test]
        public void CanGetSubVectorOfRowAsMatrixUnchecked()
        {
            var expected = TestMatrices["SubVectorRow1x3"];
            var source = TestMatrices["Source4x4"];
            var selectRow = 1;
            var selectColumns = IndexVectors["SubMatrixColumns"];
            var actual = source.At(selectRow, selectColumns);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can update subvector of row as matrix unchecked.
        /// </summary>
        [Test]
        public void CanUpdateSubVectorOfRowAsMatrixUnchecked()
        {
            var expected = TestMatrices["UpdatedSubVectorRow4x4"];
            var matrix = TestMatrices["Source4x4"].Clone();
            var newValues = TestVectors["NewValuesSubVectorRow3"];
            var selectRow = 1;
            var selectColumns = IndexVectors["SubMatrixColumns"];
            matrix.At(selectRow, selectColumns, newValues);

            AssertHelpers.AlmostEqualRelative(expected, matrix, 6);
        }

        /// <summary>
        /// Can get subvector of row as vector unchecked.
        /// </summary>
        [Test]
        public void CanGetSubVectorOfRowAsVectorUnchecked()
        {
            var expected = TestVectors["SubVectorRow3"];
            var source = TestMatrices["Source4x4"];
            var selectRow = 1;
            var selectColumns = IndexVectors["SubMatrixColumns"];
            var actual = source.AtV(selectRow, selectColumns);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can get subvector of column as matrix unchecked.
        /// </summary>
        [Test]
        public void CanGetSubVectorOfColumnAsMatrixUnchecked()
        {
            var expected = TestMatrices["SubVectorColumn3x1"];
            var source = TestMatrices["Source4x4"];
            var selectColumn = 2;
            var selectRows = IndexVectors["SubMatrixRows"];
            var actual = source.At(selectRows, selectColumn);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can update subvector of column as matrix unchecked.
        /// </summary>
        [Test]
        public void CanUpdateSubVectorOfColumnAsMatrixUnchecked()
        {
            var expected = TestMatrices["UpdatedSubVectorColumn4x4"];
            var matrix = TestMatrices["Source4x4"].Clone();
            var newValues = TestVectors["NewValuesSubVectorColumn3"];
            var selectColumn = 2;
            var selectRows = IndexVectors["SubMatrixRows"];
            matrix.At(selectRows, selectColumn, newValues);

            AssertHelpers.AlmostEqualRelative(expected, matrix, 6);
        }

        /// <summary>
        /// Can get subvector of column as vector unchecked.
        /// </summary>
        [Test]
        public void CanGetSubVectorOfColumnAsVectorUnchecked()
        {
            var expected = TestVectors["SubVectorColumn3"];
            var source = TestMatrices["Source4x4"];
            var selectColumn = 2;
            var selectRows = IndexVectors["SubMatrixRows"];
            var actual = source.AtV(selectRows, selectColumn);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can get subvector of vector unchecked.
        /// </summary>
        [Test]
        public void CanGetSubVectorOfVectorUnchecked()
        {
            var expected = TestVectors["SubVector3"];
            var source = TestVectors["Source4"];
            var select = IndexVectors["SubVector"];
            var actual = source.At(select);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can update subvector unchecked.
        /// </summary>
        [Test]
        public void CanUpdateSubVectorOfVectorUnchecked()
        {
            var expected = TestVectors["Updated4"];
            var vector = TestVectors["Source4"].Clone();
            var newValues = TestVectors["NewValues3"];
            var select = IndexVectors["SubVector"];
            vector.At(select, newValues);

            AssertHelpers.AlmostEqualRelative(expected, vector, 6);
        }

        // Checked

        /// <summary>
        /// Can get submatrix checked.
        /// </summary>
        [Test]
        public void CanGetSubMatrixChecked()
        {
            var expected = TestMatrices["SubMatrix3x3"];
            var source = TestMatrices["Source4x4"];
            var selectRows = IndexVectors["SubMatrixRows"];
            var selectColumns = IndexVectors["SubMatrixColumns"];
            var actual = source.SubMatrix(selectRows, selectColumns);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can get oversize submatrix checked.
        /// </summary>
        [Test]
        public void CanGetOversizeSubMatrixChecked()
        {
            var expected = TestMatrices["Oversize6x6"];
            var source = TestMatrices["Source4x4"];
            var selectRows = IndexVectors["OversizeRows"];
            var selectColumns = IndexVectors["OversizeColumns"];
            var actual = source.SubMatrix(selectRows, selectColumns);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can update submatrix checked.
        /// </summary>
        [Test]
        public void CanUpdateSubMatrixChecked()
        {
            var expected = TestMatrices["Updated4x4"];
            var matrix = TestMatrices["Source4x4"].Clone();
            var newValues = TestMatrices["NewValues3x3"];
            var selectRows = IndexVectors["SubMatrixRows"];
            var selectColumns = IndexVectors["SubMatrixColumns"];
            matrix.SetSubMatrix(selectRows, selectColumns, newValues);

            AssertHelpers.AlmostEqualRelative(expected, matrix, 6);
        }

        /// <summary>
        /// Update submatrix with mismatched indexRows.Count and newValues.RowCount throws <code>ArgumentException</code>checked.
        /// </summary>
        [Test]
        public void UpdateSubMatrixMismatchedRowsCountsThrowsArgumentException()
        {
            var matrix = TestMatrices["Source4x4"].Clone();
            var newValues = TestMatrices["SubVectorRow1x3"];
            var selectRows = IndexVectors["SubMatrixRows"];
            var selectColumns = IndexVectors["SubMatrixColumns"];
            Assert.That(() => matrix.SetSubMatrix(selectRows, selectColumns, newValues), Throws.ArgumentException);
        }

        /// <summary>
        /// Update submatrix with mismatched indexColumns.Count and newValues.ColumnCount throws <code>ArgumentException</code>.
        /// </summary>
        [Test]
        public void UpdateSubMatrixMismatchedColumnsCountsThrowsArgumentException()
        {
            var matrix = TestMatrices["Source4x4"].Clone();
            var newValues = TestMatrices["SubVectorColumn3x1"];
            var selectRows = IndexVectors["SubMatrixRows"];
            var selectColumns = IndexVectors["SubMatrixColumns"];
            Assert.Throws<ArgumentException>(() => matrix.SetSubMatrix(selectRows, selectColumns, newValues));
        }

        /// <summary>
        /// Can get subvector of row as vector checked.
        /// </summary>
        [Test]
        public void CanGetSubVectorOfRowAsVectorChecked()
        {
            var expected = TestVectors["SubVectorRow3"];
            var source = TestMatrices["Source4x4"];
            var selectRow = 1;
            var selectColumns = IndexVectors["SubMatrixColumns"];
            var actual = source.SubRow(selectRow, selectColumns);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Get subvector of invalid row index throws <code>ArgumentOutOfRangeException</code>.
        /// </summary>
        [Test]
        public void GetSubVectorOfInvalidRowIndexThrowsArgumentOutOfRangeException()
        {
            var source = TestMatrices["Source4x4"];
            var selectColumns = IndexVectors["SubMatrixColumns"];
            Assert.Throws<ArgumentOutOfRangeException>(() => source.SubRow(-1, selectColumns));
            Assert.Throws<ArgumentOutOfRangeException>(() => source.SubRow(source.RowCount, selectColumns));
        }

        /// <summary>
        /// Can get subvector of row as matrix checked.
        /// </summary>
        /// <remarks>SubMatrix in this case uses SubRow so error checking will not be repeated.</remarks>
        [Test]
        public void CanGetSubVectorOfRowAsMatrixChecked()
        {
            var expected = TestMatrices["SubVectorRow1x3"];
            var source = TestMatrices["Source4x4"];
            var selectRow = 1;
            var selectColumns = IndexVectors["SubMatrixColumns"];
            var actual = source.SubMatrix(selectRow, selectColumns);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can update subvector of row as matrix checked.
        /// </summary>
        [Test]
        public void CanUpdateSubVectorOfRowAsMatrixChecked()
        {
            var expected = TestMatrices["UpdatedSubVectorRow4x4"];
            var matrix = TestMatrices["Source4x4"].Clone();
            var newValues = TestVectors["NewValuesSubVectorRow3"];
            var selectRow = 1;
            var selectColumns = IndexVectors["SubMatrixColumns"];
            matrix.SetSubRow(selectRow, selectColumns, newValues);

            AssertHelpers.AlmostEqualRelative(expected, matrix, 6);
        }

        /// <summary>
        /// Update subvector of invalid row index throws <code>ArgumentOutOfRangeException</code>.
        /// </summary>
        [Test]
        public void UpdateSubVectorOfInvalidRowIndexThrowsArgumentOutOfRangeException()
        {
            var source = TestMatrices["Source4x4"];
            var selectColumns = IndexVectors["SubMatrixColumns"];
            var newValues = TestVectors["NewValuesSubVectorRow3"];
            Assert.Throws<ArgumentOutOfRangeException>(() => source.SetSubRow(-1, selectColumns, newValues));
            Assert.Throws<ArgumentOutOfRangeException>(() => source.SetSubRow(source.RowCount, selectColumns, newValues));
        }

        /// <summary>
        /// Update subvector with mismatched indexColumns.Count and newValues.Count throws <code>ArgumentException</code>.
        /// </summary>
        [Test]
        public void UpdateSubVectorOfMismatchedColumnsCountThrowsArgumentException()
        {
            var source = TestMatrices["Source4x4"];
            var selectColumns = IndexVectors["SubMatrixColumns"];
            var newValues = TestVectors["NewValues7"];
            Assert.Throws<ArgumentException>(() => source.SetSubRow(1, selectColumns, newValues));
        }

        /// <summary>
        /// Can get subvector of column as vector checked.
        /// </summary>
        /// <remarks>SubMatrix in this case uses SubRow so error checking will not be repeated.</remarks>
        [Test]
        public void CanGetSubVectorOfColumnAsVectorChecked()
        {
            var expected = TestVectors["SubVectorColumn3"];
            var source = TestMatrices["Source4x4"];
            var selectColumn = 2;
            var selectRows = IndexVectors["SubMatrixRows"];
            var actual = source.SubColumn(selectRows, selectColumn);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can get subvector of column as matrix checked.
        /// </summary>
        [Test]
        public void CanGetSubVectorOfColumnAsMatrixChecked()
        {
            var expected = TestMatrices["SubVectorColumn3x1"];
            var source = TestMatrices["Source4x4"];
            var selectColumn = 2;
            var selectRows = IndexVectors["SubMatrixRows"];
            var actual = source.SubMatrix(selectRows, selectColumn);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Get subvector of invalid column index throws <code>ArgumentOutOfRangeException</code>.
        /// </summary>
        [Test]
        public void GetSubVectorOfInvalidColumnIndexThrowsArgumentOutOfRangeException()
        {
            var source = TestMatrices["Source4x4"];
            var selectRows = IndexVectors["SubMatrixRows"];
            Assert.Throws<ArgumentOutOfRangeException>(() => source.SubColumn(selectRows, - 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => source.SubColumn(selectRows, source.ColumnCount));
        }

        /// <summary>
        /// Can update subvector of column as matrix checked.
        /// </summary>
        [Test]
        public void CanUpdateSubVectorOfColumnAsMatrixChecked()
        {
            var expected = TestMatrices["UpdatedSubVectorColumn4x4"];
            var matrix = TestMatrices["Source4x4"].Clone();
            var newValues = TestVectors["NewValuesSubVectorColumn3"];
            var selectColumn = 2;
            var selectRows = IndexVectors["SubMatrixRows"];
            matrix.SetSubColumn(selectRows, selectColumn, newValues);

            AssertHelpers.AlmostEqualRelative(expected, matrix, 6);
        }

        /// <summary>
        /// Update subvector of invalid column index throws <code>ArgumentOutOfRangeException</code>.
        /// </summary>
        [Test]
        public void UpdateSubVectorOfInvalidColumnIndexThrowsArgumentOutOfRangeException()
        {
            var source = TestMatrices["Source4x4"];
            var selectRows = IndexVectors["SubMatrixRows"];
            var newValues = TestVectors["NewValuesSubVectorColumn3"];
            Assert.Throws<ArgumentOutOfRangeException>(() => source.SetSubColumn(selectRows, -1, newValues));
            Assert.Throws<ArgumentOutOfRangeException>(() => source.SetSubColumn(selectRows, source.ColumnCount, newValues));
        }

        /// <summary>
        /// Update subvector with mismatched indexRows.Count and newValues.Count throws <code>ArgumentException</code>.
        /// </summary>
        [Test]
        public void UpdateSubVectorOfMismatchedRowCountThrowsArgumentException()
        {
            var source = TestMatrices["Source4x4"];
            var selectRows = IndexVectors["SubMatrixRows"];
            var newValues = TestVectors["NewValues7"];
            Assert.Throws<ArgumentException>(() => source.SetSubColumn(selectRows, 1, newValues));
        }

        /// <summary>
        /// Can get subvector of vector checked.
        /// </summary>
        [Test]
        public void CanGetSubVectorOfVectorChecked()
        {
            var expected = TestVectors["SubVector3"];
            var source = TestVectors["Source4"];
            var select = IndexVectors["SubVector"];
            var actual = source.SubVector(select);

            AssertHelpers.AlmostEqualRelative(expected, actual, 6);
        }

        /// <summary>
        /// Can update subvector checked.
        /// </summary>
        [Test]
        public void CanUpdateSubVectorOfVectorChecked()
        {
            var expected = TestVectors["Updated4"];
            var vector = TestVectors["Source4"].Clone();
            var newValues = TestVectors["NewValues3"];
            var select = IndexVectors["SubVector"];
            vector.SetSubVector(select, newValues);

            AssertHelpers.AlmostEqualRelative(expected, vector, 6);
        }

        /// <summary>
        /// Update subvector with mismatched indexes.Count and newValues.Count throws <code>ArgumentException</code>
        /// </summary>
        [Test]
        public void UpdateSubVectorMismatchedIndexValuesThrowsArgumentException()
        {
            var vector = TestVectors["Source4"].Clone();
            var newValues = TestVectors["NewValues7"];
            var select = IndexVectors["SubMatrixRows"];

            Assert.Throws<ArgumentException>(() => vector.SetSubVector(select, newValues));
        }

        // Simple index bound checking

        /// <summary>
        /// Can determine validity of row indexes
        /// </summary>
        [Test]
        public void CanDetermineValidRowIndex()
        {
            var m = TestMatrices["Source4x4"];
            Assert.IsFalse((-1).IsValidRowIndex(m), "Failed to detect -1 as invalid row index.");
            Assert.IsTrue((2).IsValidRowIndex(m), "Failed to detect 2 as valid row index.");
            Assert.IsFalse((5).IsValidRowIndex(m), "Failed to detect 5 as invalid row index.");
        }

        /// <summary>
        /// Can determine validity of column indexes
        /// </summary>
        [Test]
        public void CanDetermineValidColumnIndex()
        {
            var m = TestMatrices["Source4x4"];
            Assert.IsFalse((-1).IsValidColumnIndex(m), "Failed to detect -1 as invalid column index.");
            Assert.IsTrue((2).IsValidColumnIndex(m), "Failed to detect 2 as valid column index.");
            Assert.IsFalse((5).IsValidColumnIndex(m), "Failed to detect 5 as invalid column index.");
        }

        /// <summary>
        /// Can determine validity of vector indexes
        /// </summary>
        [Test]
        public void CanDetermineValidIndex()
        {
            var m = TestVectors["Source4"];
            Assert.IsFalse((-1).IsValidIndex(m), "Failed to detect -1 as invalid index.");
            Assert.IsTrue((2).IsValidIndex(m), "Failed to detect 2 as valid index.");
            Assert.IsFalse((5).IsValidIndex(m), "Failed to detect 5 as invalid index.");
        }
    }
}

