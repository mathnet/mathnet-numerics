using System;
using System.Linq;

namespace MathNet.Numerics.LinearAlgebra
{
    /*------------------------------------------------------------------------------------------------------
     * This partial class file(Matrix.Indexers.cs) is used for add some indexer for Matrix<T> class.
     * I implement these functions mainly through int, int Array, ValueTuple struct, Range struct(.NET5).
     * They can change the usage of indexer function of MathNet.Numerics.LinearAlgebra.Matrix.
     * I have simply indicated the use method on each indexer function.
     * So everyone can use these indexers to Get or Set SubMatrix or SubVector more easily.
     * Enjoy it!
     * 
     * If you have any questions, suggestions, comments or ideas, please contact the author.
     * Thanks for your feedbacks.
     *                              Author Email: zhouxutao@163.com
     *                              Date: 2021.08.30
     -------------------------------------------------------------------------------------------------------*/
    public abstract partial class Matrix<T>
    {
        /// <summary>
        /// Get or Set the SubMatrix by Row ValueTuple (start, end) struct Indexes and Column ValueTuple (start, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTuple (start, end) struct Indexes.
        /// The End Must be Larger than the Start.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, end) struct Indexes.
        /// The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>The SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[(1, 2), (2, 3)];
        ///     etc.
        ///             
        /// Set:
        ///     oneMatrix[(1, 2), (2, 3)] = otherMatrix[(2, 3), (5, 6)];
        ///     etc. 
        /// </example>
        public Matrix<T> this[ValueTuple<int, int> rows, ValueTuple<int, int> columns]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;

                int columnIndex = columns.Item1;
                int columnCount = columns.Item2 - columns.Item1 + 1;

                var result = Build.SameAs(this, rowCount, columnCount);
                Storage.CopySubMatrixTo(result.Storage, rowIndex, 0, rowCount, columnIndex, 0, columnCount, ExistingData.AssumeZeros);
                return result;
            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;

                int columnIndex = columns.Item1;
                int columnCount = columns.Item2 - columns.Item1 + 1;

                value.Storage.CopySubMatrixTo(Storage, 0, rowIndex, rowCount, 0, columnIndex, columnCount);
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by Row ValueTuple (start, end) struct Indexes and Column int Single Index.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTuple (start, end) struct Indexes.The End Must be Larger than the Start.
        /// </param>
        /// <param name="columnIndex">
        /// Column int Single Index.
        /// </param>
        /// 
        /// <returns>A SubVector</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneMatrix[(1, 2), 5];
        ///     etc.
        ///             
        /// Set:
        ///     oneMatrix[(1, 2), 6] = otherVector[(2, 3)];
        ///     etc.
        /// </example>
        public Vector<T> this[ValueTuple<int, int> rows, int columnIndex]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                VerifySingleIndex(columnIndex, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;

                var result = Vector<T>.Build.SameAs(this, rowCount);
                Storage.CopySubColumnTo(result.Storage, columnIndex, rowIndex, 0, rowCount);
                return result;
            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                VerifySingleIndex(columnIndex, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;

                value.Storage.CopyToSubColumn(Storage, columnIndex, 0, rowIndex, rowCount);
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by Row int Single Index and Column ValueTuple (start, end) struct Indexes. 
        /// </summary>
        /// 
        /// <param name="rowIndex">
        /// Row int Single Index.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, end) struct Indexes.
        /// The End Must be Larger than the Start.
        /// </param>
        /// <returns>A SubVector</returns>
        /// 
        /// <example>
        ///  Get:
        ///     var vec = oneMatrix[5, (2, 4)];
        ///     etc.
        ///  Set:
        ///     oneMatrix[6, (2, 4)] = otherVector[(2, 4)];
        ///     oneMatrix[4, (2, 4)] = otherMatrix[9, (2, 4)];
        ///     etc.
        ///</example>
        public Vector<T> this[int rowIndex, ValueTuple<int, int> columns]
        {
            get
            {
                VerifySingleIndex(rowIndex, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int columnIndex = columns.Item1;
                int columnCount = columns.Item2 - columns.Item1 + 1;

                var result = Vector<T>.Build.SameAs(this, columnCount);
                Storage.CopySubRowTo(result.Storage, rowIndex, columnIndex, 0, columnCount);
                return result;
            }

            set
            {
                VerifySingleIndex(rowIndex, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int columnIndex = columns.Item1;
                int columnCount = columns.Item2 - columns.Item1 + 1;

                value.Storage.CopyToSubRow(Storage, rowIndex, 0, columnIndex, columnCount);
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by Row int Single Index and Column ValueTuple (start, step, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="rowIndex">
        /// Row int Single Index.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by (start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>A SubVector</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneMatrix[5, (1, 3, 5)];
        ///     etc.
        /// Set:
        ///     oneMatrix[5, (1, 3, 5)] = otherVector[(1, 3, 5)];
        ///     etc.
        /// </example>
        public Vector<T> this[int rowIndex, ValueTuple<int, int, int> columns]
        {
            get
            {
                VerifySingleIndex(rowIndex, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                var result = Vector<T>.Build.SameAs(this, columnsIndex.Length);
                for (int i = 0; i < columnsIndex.Length; i++)
                {
                    result.Storage[i] = Storage[rowIndex, columnsIndex[i]];
                }
                return result;
            }

            set
            {
                VerifySingleIndex(rowIndex, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                for (int i = 0; i < columnsIndex.Length; i++)
                {
                    Storage[rowIndex, columnsIndex[i]] = value.Storage[i];
                }
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by Row ValueTuple (start, step, end) struct Indexes and Column int Single Index.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by (start, step, end). The End Must be Larger than the start.
        /// </param>
        /// <param name="columnIndex">
        /// Column int Single Index.
        /// </param>
        /// 
        /// <returns>A SubVector</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneMatrix[(1, 3, 5), 5];
        ///     etc.
        /// Set:
        ///     oneMatrix[(1, 3, 5), 5] = otherVector[(1, 3, 5)];
        ///     oneMatrix[(1, 3, 5), 5] = otherMatrix[(1, 3, 5), 6];
        ///     etc.
        /// </example>
        public Vector<T> this[ValueTuple<int, int, int> rows, int columnIndex]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                VerifySingleIndex(columnIndex, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);

                var result = Vector<T>.Build.SameAs(this, rowsIndex.Length);
                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    result.Storage[i] = Storage[rowsIndex[i], columnIndex];
                }
                return result;
            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                VerifySingleIndex(columnIndex, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);

                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    Storage[rowsIndex[i], columnIndex] = value.Storage[i];
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row ValueTuple (start, step, end) struct Indexes and Column ValueTuple (start, step, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by (start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by (start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[(1, 3, 5), (2, 4, 6)];
        ///     etc.         
        /// Set:
        ///     oneMatrix[(1, 3, 5), (2, 4, 6)] = otherMatrix[(1, 3, 5), (2, 4, 6)];
        ///     etc.
        /// </example>
        public Matrix<T> this[ValueTuple<int, int, int> rows, ValueTuple<int, int, int> columns]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);
                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                var result = Build.SameAs(this, rowsIndex.Length, columnsIndex.Length);
                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    for (int j = 0; j < columnsIndex.Length; j++)
                    {
                        result.Storage[i, j] = Storage[rowsIndex[i], columnsIndex[j]];
                    }
                }
                return result;
            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);
                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    for (int j = 0; j < columnsIndex.Length; j++)
                    {
                        Storage[rowsIndex[i], columnsIndex[j]] = value.Storage[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row ValueTuple (start, end) struct Indexes and Column ValueTuple (start, step, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTuple (start, end) struct Indexes.
        /// The End Must be Larger than the Start.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by (start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[(1, 5), (2, 4, 6)];
        ///     etc.
        /// Set:
        ///     oneMatrix[(1, 5), (2, 4, 6)] = otherMatrix[(1, 5), (2, 4, 6)];
        ///     etc.
        /// </example>
        public Matrix<T> this[ValueTuple<int, int> rows, ValueTuple<int, int, int> columns]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;
                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                var result = Build.SameAs(this, rowCount, columnsIndex.Length);
                for (int i = 0; i < columnsIndex.Length; i++)
                {
                    result.SetColumn(i, Column(columnsIndex[i], rowIndex, rowCount));
                }
                return result;
            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;
                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                for (int i = 0; i < columnsIndex.Length; i++)
                {
                    SetColumn(columnsIndex[i], rowIndex, rowCount, value.Column(i));
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row ValueTuple (start, step, end) struct Indexes and Column ValueTuple (start, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by (start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, end) struct Indexes.
        /// The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>The SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[(2, 4, 6), (1, 5)];
        ///     etc.
        /// Set:
        ///     oneMatrix[(2, 4, 6), (1, 5)] = otherMatrix[(2, 4, 6), (1, 5)];
        ///     etc.
        /// </example>
        public Matrix<T> this[ValueTuple<int, int, int> rows, ValueTuple<int, int> columns]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);
                int columnIndex = columns.Item1;
                int columnCount = columns.Item2 - columns.Item1 + 1;

                var result = Build.SameAs(this, rowsIndex.Length, columnCount);
                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    result.SetRow(i, Row(rowsIndex[i], columnIndex, columnCount));
                }
                return result;
            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);
                int columnIndex = columns.Item1;
                int columnCount = columns.Item2 - columns.Item1 + 1;

                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    SetRow(rowsIndex[i], columnIndex, columnCount, value.Row(i));
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row int Array Indexes and Column int Array Indexes. 
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// <param name="columns">
        /// Column int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// 
        /// <returns>The SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[new int[] { 1, 2, 3 }, new int[] { 2, 5, 6 }];
        ///     etc.
        /// Set:
        ///     oneMatrix[new int[] { 1, 2, 3 }, new int[] { 2, 5, 6 }] = otherMatrix[new int[] { 1, 2, 3 }, new int[] { 3, 6, 3 }]; 
        ///     etc.
        /// </example>
        public Matrix<T> this[int[] rows, int[] columns]
        {
            get
            {
                VerifyArray(rows, RowCount);
                VerifyArray(columns, ColumnCount);

                var result = Build.SameAs(this, rows.Length, columns.Length);
                for (int i = 0; i < rows.Length; i++)
                {
                    for (int j = 0; j < columns.Length; j++)
                    {
                        result.Storage[i, j] = Storage[rows[i], columns[j]];
                    }
                }
                return result;
            }

            set
            {
                VerifyArray(rows, RowCount);
                VerifyArray(columns, ColumnCount);

                for (int i = 0; i < rows.Length; i++)
                {
                    for (int j = 0; j < columns.Length; j++)
                    {
                        Storage[rows[i], columns[j]] = value.Storage[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by Row int Array Indexes and Column int Single Index.
        /// </summary>
        /// 
        /// <param name="rows">
        /// The Row int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// <param name="columnIndex">
        /// The Column int Single Index
        /// </param>
        /// 
        /// <returns>The SubVector</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneMatrix[new int[] { 1, 2, 3 }, 3];
        ///     etc.        
        /// Set:
        ///     oneMatrix[new int[] { 1, 2, 3 }, 3] = otherMatrix[new int[] { 5, 6, 4 }, 6];
        ///     oneMatrix[new int[] { 1, 2, 3 }, 3] = otherVector[new int[] { 1, 2, 3 }];
        ///     etc.
        /// </example>
        public Vector<T> this[int[] rows, int columnIndex]
        {
            get
            {
                VerifyArray(rows, RowCount);
                VerifySingleIndex(columnIndex, ColumnCount);

                var result = Vector<T>.Build.SameAs(this, rows.Length);
                for (int i = 0; i < rows.Length; i++)
                {
                    result.Storage[i] = Storage[rows[i], columnIndex];
                }
                return result;
            }

            set
            {
                VerifyArray(rows, RowCount);
                VerifySingleIndex(columnIndex, ColumnCount);

                for (int i = 0; i < rows.Length; i++)
                {
                    Storage[rows[i], columnIndex] = value.Storage[i];
                }
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by Row int Single Index and Column int Array Indexes.
        /// </summary>
        /// 
        /// <param name="rowIndex">
        /// Row int Single Index.
        /// </param>
        /// <param name="columns">
        /// Column int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// 
        /// <returns>The SubVector</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneMatrix[ 3, new int[] { 1, 2, 3 }];
        ///     etc.
        /// Set:
        ///     oneMatrix[ 3, new int[] { 1, 2, 3 }] = otherMatrix[ 6, new int[] { 2, 5, 4 }];
        ///     oneMatrix[ 3, new int[] { 1, 2, 3 }]  = otherVector[new int[] { 2, 5, 4 }];
        ///     etc.
        /// </example>
        public Vector<T> this[int rowIndex, int[] columns]
        {
            get
            {
                VerifySingleIndex(rowIndex, RowCount);
                VerifyArray(columns, ColumnCount);

                var result = Vector<T>.Build.SameAs(this, columns.Length);
                for (int i = 0; i < columns.Length; i++)
                {
                    result.Storage[i] = Storage[rowIndex, columns[i]];
                }
                return result;
            }

            set
            {
                VerifySingleIndex(rowIndex, RowCount);
                VerifyArray(columns, ColumnCount);

                for (int i = 0; i < columns.Length; i++)
                {
                    Storage[columns[i], rowIndex] = value.Storage[i];
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row int Array Indexes and Column ValueTuple (start, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, end) struct Indexes.
        /// The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>The SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[ new int[] { 1, 2, 3 }, (1, 2)];
        ///     etc.
        /// Set:
        ///     oneMatrix[ new int[] { 1, 2, 3 }, (1, 2)] = otherMatrix[ new int[] { 4, 3, 5 }, (1, 2)];
        ///     etc.
        /// </example>
        public Matrix<T> this[int[] rows, ValueTuple<int, int> columns]
        {
            get
            {
                VerifyArray(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2);

                var result = Build.SameAs(this, rows.Length, columnsIndex.Length);
                for (int i = 0; i < rows.Length; i++)
                {
                    for (int j = 0; j < columnsIndex.Length; j++)
                    {
                        result.Storage[i, j] = Storage[rows[i], columnsIndex[j]];
                    }
                }
                return result;
            }

            set
            {
                VerifyArray(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2);

                for (int i = 0; i < rows.Length; i++)
                {
                    for (int j = 0; j < columnsIndex.Length; j++)
                    {
                        Storage[rows[i], columnsIndex[j]] = value.Storage[i, j];
                    }
                }

            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row int Array Indexes and Column ValueTuple (start, step, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row int Array Indexes.
        /// The Length of it count Must be Larger than One.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by(start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[new int[] { 1, 2, 3 }, (1, 2, 5)];
        ///     etc.            
        /// Set:
        ///     oneMatrix[new int[] { 1, 2, 3 }, (1, 2, 5)] = otherMatrix[new int[] { 4, 3, 5 }, (1, 2, 5)]; 
        ///     etc.
        /// </example>
        public Matrix<T> this[int[] rows, ValueTuple<int, int, int> columns]
        {
            get
            {
                VerifyArray(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                var result = Build.SameAs(this, rows.Length, columnsIndex.Length);
                for (int i = 0; i < rows.Length; i++)
                {
                    for (int j = 0; j < columnsIndex.Length; j++)
                    {
                        result.Storage[i, j] = Storage[rows[i], columnsIndex[j]];
                    }
                }
                return result;
            }

            set
            {
                VerifyArray(rows, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                for (int i = 0; i < rows.Length; i++)
                {
                    for (int j = 0; j < columnsIndex.Length; j++)
                    {
                        Storage[rows[i], columnsIndex[j]] = value.Storage[i, j];
                    }
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row ValueTupe (start, end) struct Indexes and Column int Array Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTupe (start, end) struct Indexes.
        /// The End Must be Larger than the Start.
        /// </param>
        /// <param name="columns">
        /// Column int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// 
        /// <returns></returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[ (1, 3), new int[] { 1, 2, 3 }];
        ///     etc.            
        /// Set:
        ///     oneMatrix[ (1, 3), new int[] { 1, 2, 3 }] = otherMatrix[ (1, 3), new int[] { 3, 6, 1 }];
        ///     etc.
        /// </example>
        public Matrix<T> this[ValueTuple<int, int> rows, int[] columns]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                VerifyArray(columns, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;

                var result = Build.SameAs(this, rowCount, columns.Length);
                for (int i = 0; i < columns.Length; i++)
                {
                    result.SetColumn(i, Column(columns[i], rowIndex, rowCount));
                }
                return result;
            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                VerifyArray(columns, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;

                for (int i = 0; i < columns.Length; i++)
                {
                    SetColumn(columns[i], rowIndex, rowCount, value.Column(i));
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row ValueTupe (start, step, end) struct Indexes and Column int Array Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTupe (start, step, end) struct Indexes.
        /// It will be a LinearRange by(start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// <param name="columns">
        /// Column int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[ (1, 2, 5), new int[] { 1, 2, 3 }];
        ///     etc.            
        /// Set:
        ///     oneMatrix[ (1, 2, 5), new int[] { 1, 2, 3 }] = otherMatrix[ (1, 2, 5), new int[] { 3, 6, 1 }]; 
        ///     etc.
        /// </example>
        public Matrix<T> this[ValueTuple<int, int, int> rows, int[] columns]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                VerifyArray(columns, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);

                var result = Build.SameAs(this, rowsIndex.Length, columns.Length);
                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    for (int j = 0; j < columns.Length; j++)
                    {
                        result.Storage[i, j] = Storage[rowsIndex[i], columns[j]];
                    }
                }
                return result;

            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                VerifyArray(columns, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);

                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    for (int j = 0; j < columns.Length; j++)
                    {
                        Storage[rowsIndex[i], columns[j]] = value.Storage[i, j];
                    }
                }

            }
        }

#if NET5_0_OR_GREATER

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row Range struct Indexes and Column Range struct Indexes.
        /// </summary>
        /// 
        /// <param name="rowRange">
        /// Row Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// <param name="columnRange">
        /// Column Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[1..3, 3..5];
        ///     var mat = oneMatrix[1..3, ..5];
        ///     var mat = oneMatrix[1..3, 3..];
        ///     var mat = oneMatrix[1..3, ^5..^1];
        ///     var mat = oneMatrix[1..3, ..];
        ///     etc.
        /// Set:
        ///     oneMatrix[1..3, 3..5] = otherMatrix[2..4, 3..5];
        ///     oneMatrix[1..3, ^5..^1] = otherMatrix[1..3, ^5..^1];
        ///     etc.
        /// </example>
        public Matrix<T> this[Range rowRange, Range columnRange]
        {
            get
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;
                int columnCount = columnEndIndex - columnStartIndex + 1;

                var result = Build.SameAs(this, rowCount, columnCount);
                Storage.CopySubMatrixTo(result.Storage, rowStartIndex, 0, rowCount, columnStartIndex, 0, columnCount, ExistingData.AssumeZeros);
                return result;
            }

            set
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;
                int columnCount = columnEndIndex - columnStartIndex + 1;

                value.Storage.CopySubMatrixTo(Storage, 0, rowStartIndex, rowCount, 0, columnStartIndex, columnCount);
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by Row Range struct Indexes and Column int Single Index.
        /// </summary>
        /// 
        /// <param name="rowRange">
        /// Row Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// <param name="columnIndex">
        /// Column int Single Index.
        /// </param>
        /// 
        /// <returns>A SubVector</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneMatrix[1..3, 1];
        ///     var vec = oneMatrix[..3, 1];
        ///     var vec = oneMatrix[1.., 1];
        ///     var vec = oneMatrix[^3..^1, 1];
        ///     var vec = oneMatrix[.., 1];
        ///     etc.
        /// Set:
        ///     oneMatrix[1..3, 1] = otherMatrix[1..3, 1];
        ///     oneMatrix[^3..^1, 1] = otherMatrix[^3..^1, 1];
        ///     oneMatrix[1..3, 1] = otherVector[1..3];
        ///     etc. 
        /// </example>
        public Vector<T> this[Range rowRange, int columnIndex]
        {
            get
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                VerifySingleIndex(columnIndex, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;

                var result = Vector<T>.Build.SameAs(this, rowCount);
                Storage.CopySubColumnTo(result.Storage, columnIndex, rowStartIndex, 0, rowCount);
                return result;
            }

            set
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                VerifySingleIndex(columnIndex, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;

                value.Storage.CopyToSubColumn(Storage, columnIndex, 0, rowStartIndex, rowCount);
            }
        }

        /// <summary>
        /// Get the SubVector or Set the SubElement by Row int Single Index and Column Range struct Indexes.
        /// </summary>
        /// 
        /// <param name="rowIndex">
        /// Row int Single Index.
        /// </param>
        /// <param name="columnRange">
        /// Column Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// 
        /// <returns>A SubVector</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneMatrix[2, 1..3];
        ///     var vec = oneMatrix[2, ..3];
        ///     var vec = oneMatrix[2, 1..];
        ///     var vec = oneMatrix[2, ^3..^1];
        ///     var vec = oneMatrix[2, ..];
        ///     etc.             
        /// Set:
        ///     oneMatrix[2, 1..3] = otherMatrix[2, 1..3];
        ///     oneMatrix[2, ^3..^1] = otherMatrix[2, ^3..^1];
        ///     oneMatrix[2, 1..3] = otherVector[1..3]; 
        ///     etc.
        /// </example>
        public Vector<T> this[int rowIndex, Range columnRange]
        {
            get
            {
                VerifySingleIndex(rowIndex, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int columnCount = columnEndIndex - columnStartIndex + 1;

                var result = Vector<T>.Build.SameAs(this, columnCount);
                Storage.CopySubRowTo(result.Storage, rowIndex, columnStartIndex, 0, columnCount);
                return result;
            }

            set
            {
                VerifySingleIndex(rowIndex, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int columnCount = columnEndIndex - columnStartIndex + 1;

                value.Storage.CopyToSubRow(Storage, rowIndex, 0, columnStartIndex, columnCount);
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row Range struct Indexes and Column int Array Indexes.
        /// </summary>
        /// 
        /// <param name="rowRange">
        /// Row Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// <param name="columns">
        /// Column int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var vec = oneMatrix[1..3, new int[]{ 1, 2, 3 }];
        ///     var vec = oneMatrix[1.., new int[]{ 1, 2, 3 }];
        ///     var vec = oneMatrix[..3, new int[]{ 1, 2, 3 }];
        ///     var vec = oneMatrix[^3..^1, new int[]{ 1, 2, 3 }];
        ///     var vec = oneMatrix[.., new int[]{ 1, 2, 3 }];
        ///     etc.
        /// Set:
        ///     oneMatrix[1..3, new int[]{ 1, 2, 3 }] = otherMatrix[1..3, new int[]{ 4, 3, 7 }];
        ///     oneMatrix[^3..^1, new int[]{ 1, 2, 3 }] = otherMatrix[^3..^1, new int[]{ 5, 2, 3 }];
        /// etc.
        /// </example>
        public Matrix<T> this[Range rowRange, int[] columns]
        {
            get
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                VerifyArray(columns, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;

                var result = Build.SameAs(this, rowCount, columns.Length);
                for (int i = 0; i < columns.Length; i++)
                {
                    result.SetColumn(i, Column(columns[i], rowStartIndex, rowCount));
                }
                return result;
            }

            set
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                VerifyArray(columns, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;

                for (int i = 0; i < columns.Length; i++)
                {
                    SetColumn(columns[i], rowStartIndex, rowCount, value.Column(i));
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row Range struct Indexes and Column ValueTuple (start, end) struct Indexes.
        /// </summary>
        /// 
        /// <param name="rowRange">
        /// Row Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, end) struct Indexes.
        /// The End Must be Larger than the Start of it.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[1..3, (1, 4)];
        ///     var mat = oneMatrix[1.., (1, 4)];
        ///     var mat = oneMatrix[..3, (1, 4)];
        ///     var mat = oneMatrix[^3..^1, (1, 4)];
        ///     var mat = oneMatrix[.., (1, 4)];
        ///     etc.
        /// Set:
        ///     oneMatrix[1..3, (1, 4)] = otherMatrix[1..3, (2, 5)];
        ///     oneMatrix[^3..^1, (1, 4)] = otherMatrix[^3..^1, (2, 5)];
        ///     etc. 
        /// </example>
        public Matrix<T> this[Range rowRange, ValueTuple<int, int> columns]
        {
            get
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;
                int columnIndex = columns.Item1;
                int columnCount = columns.Item2 - columns.Item1 + 1;

                var result = Build.SameAs(this, rowCount, columnCount);
                Storage.CopySubMatrixTo(result.Storage, rowStartIndex, 0, rowCount, columnIndex, 0, columnCount, ExistingData.AssumeZeros);
                return result;
            }

            set
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;
                int columnIndex = columns.Item1;
                int columnCount = columns.Item2 - columns.Item1 + 1;

                value.Storage.CopySubMatrixTo(Storage, 0, rowStartIndex, rowCount, 0, columnIndex, columnCount);
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row Range struct Indexes and Column ValueTuple (start, step, end) struct Indexes. 
        /// </summary>
        /// 
        /// <param name="rowRange">
        /// Row Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// <param name="columns">
        /// Column ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by(start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[1..3, (1, 2, 5)];
        ///     var mat = oneMatrix[1.., (1, 2, 5)];
        ///     var mat = oneMatrix[..3, (1, 2, 5)];
        ///     var mat = oneMatrix[^3..^1, (1, 2, 5)];
        ///     var mat = oneMatrix[.., (1, 2, 5)];
        ///     etc.
        /// Set:
        ///     oneMatrix[1..3, (1, 2, 5)] = otherMatrix[1..3, (1, 2, 5)];
        ///     oneMatrix[^3..^1, (1, 2, 5)] = otherMatrix[^3..^1, (1, 2, 5)];
        ///     etc.  
        /// </example>
        public Matrix<T> this[Range rowRange, ValueTuple<int, int, int> columns]
        {
            get
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;
                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                var result = Build.SameAs(this, rowCount, columnsIndex.Length);
                for (int i = 0; i < columnsIndex.Length; i++)
                {
                    result.SetColumn(i, Column(columnsIndex[i], rowStartIndex, rowCount));
                }
                return result;
            }

            set
            {
                var (rowStartIndex, rowEndIndex) = VerifyRange(rowRange, RowCount);
                VerifyValueTuple(columns, ColumnCount);

                int rowCount = rowEndIndex - rowStartIndex + 1;
                int[] columnsIndex = Generate.LinearRangeInt32(columns.Item1, columns.Item2, columns.Item3);

                for (int i = 0; i < columnsIndex.Length; i++)
                {
                    SetColumn(columnsIndex[i], rowStartIndex, rowCount, value.Column(i));
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row int Array Indexes and Column Range struct Indexes. 
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row int Array Indexes.
        /// The Length of it Must be Larger than One.
        /// </param>
        /// <param name="columnRange">
        /// Column Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[new int[]{ 1, 2, 3}, 1..5];
        ///     var mat = oneMatrix[new int[]{ 1, 2, 3}, 1..];
        ///     var mat = oneMatrix[new int[]{ 1, 2, 3}, ..5];
        ///     var mat = oneMatrix[new int[]{ 1, 2, 3}, ^5..^1];
        ///     var mat = oneMatrix[new int[]{ 1, 2, 3}, ..];
        ///     etc.
        /// Set:
        ///     oneMatrix[new int[]{ 1, 2, 3}, 1..5] = otherMatrix[new int[]{ 1, 2, 3}, 1..5];
        ///     oneMatrix[new int[]{ 1, 2, 3}, ^5..^1] = otherMatrix[new int[]{ 1, 2, 3}, ^5..^1];
        ///     etc.    
        /// </example>
        public Matrix<T> this[int[] rows, Range columnRange]
        {
            get
            {
                VerifyArray(rows, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int columnCount = columnEndIndex - columnStartIndex + 1;

                var result = Build.SameAs(this, rows.Length, columnCount);
                for (int i = 0; i < rows.Length; i++)
                {
                    result.SetRow(i, Row(rows[i], columnStartIndex, columnCount));
                }
                return result;
            }

            set
            {
                VerifyArray(rows, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int columnCount = columnEndIndex - columnStartIndex + 1;

                for (int i = 0; i < rows.Length; i++)
                {
                    SetRow(rows[i], columnStartIndex, columnCount, value.Row(i));
                }
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row ValueTuple (start, end) struct Indexes and Column Range struct Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTuple (start, end) struct Indexes.
        /// The End Must be Larger than the Start of it.
        /// </param>
        /// <param name="columnRange">
        /// Column Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[(1, 3), 1..5];
        ///     var mat = oneMatrix[(1, 3), 1..];
        ///     var mat = oneMatrix[(1, 3), ..5];
        ///     var mat = oneMatrix[(1, 3), ^5..^1];
        ///     var mat = oneMatrix[(1, 3), ..];
        ///     etc.
        /// Set:
        ///     oneMatrix[(1, 3), 1..5] = otherMatrix[(1, 3), 1..5];
        ///     oneMatrix[(1, 3), ^5..^1] = otherMatrix[(1, 3), ^5..^1];
        ///     etc.  
        /// </example>
        public Matrix<T> this[ValueTuple<int, int> rows, Range columnRange]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;
                int columnCount = columnEndIndex - columnStartIndex + 1;

                var result = Build.SameAs(this, rowCount, columnCount);
                Storage.CopySubMatrixTo(result.Storage, rowIndex, 0, rowCount, columnStartIndex, 0, columnCount, ExistingData.AssumeZeros);
                return result;
            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int rowIndex = rows.Item1;
                int rowCount = rows.Item2 - rows.Item1 + 1;
                int columnCount = columnEndIndex - columnStartIndex + 1;

                value.Storage.CopySubMatrixTo(Storage, 0, rowIndex, rowCount, 0, columnStartIndex, columnCount);
            }
        }

        /// <summary>
        /// Get the SubMatrix or Set the SubElement by Row ValueTuple (start, step, end) struct Indexes and Column Range struct Indexes.
        /// </summary>
        /// 
        /// <param name="rows">
        /// Row ValueTuple (start, step, end) struct Indexes.
        /// It will be a LinearRange by(start, step, end). The End Must be Larger than the Start.
        /// </param>
        /// <param name="columnRange">
        /// Column Range struct Indexes.
        /// The End Must be Larger than the Start of it. The End Range Index will be contain in the Indexes.
        /// </param>
        /// 
        /// <returns>A SubMatrix</returns>
        /// 
        /// <example>
        /// Get:
        ///     var mat = oneMatrix[(1, 2, 5), 1..5];
        ///     var mat = oneMatrix[(1, 2, 5), 1..];
        ///     var mat = oneMatrix[(1, 2, 5), ..5];
        ///     var mat = oneMatrix[(1, 2, 5), ^5..^1];
        ///     var mat = oneMatrix[(1, 2, 5), ..];
        ///     etc.
        /// Set:
        ///     oneMatrix[(1, 2, 5), 1..5] = otherMatrix[(1, 2, 5), 1..5];
        ///     oneMatrix[(1, 2, 5), ^5..^1] = otherMatrix[(1, 2, 5), ^5..^1];
        ///     etc. 
        /// </example>
        public Matrix<T> this[ValueTuple<int, int, int> rows, Range columnRange]
        {
            get
            {
                VerifyValueTuple(rows, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);
                int columnCount = columnEndIndex - columnStartIndex + 1;

                var result = Build.SameAs(this, rowsIndex.Length, columnCount);
                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    result.SetRow(i, Row(rowsIndex[i], columnStartIndex, columnCount));
                }
                return result;
            }

            set
            {
                VerifyValueTuple(rows, RowCount);
                var (columnStartIndex, columnEndIndex) = VerifyRange(columnRange, ColumnCount);

                int[] rowsIndex = Generate.LinearRangeInt32(rows.Item1, rows.Item2, rows.Item3);
                int columnCount = columnEndIndex - columnStartIndex + 1;

                for (int i = 0; i < rowsIndex.Length; i++)
                {
                    SetRow(rowsIndex[i], columnStartIndex, columnCount, value.Row(i));
                }
            }
        }
#endif

        #region Used for Verifying the availability of Indexers

#if NET5_0_OR_GREATER

        /// <summary>
        /// Used for Verifying the availability of Row or Column Range struct Indexes. 
        /// </summary>
        /// <param name="range">Row or Column Range struct Indexes</param>
        /// <param name="count">RowCount or ColumnCount of the Matrix</param>
        /// <returns name="startIndex">The Start of the Range.</returns>
        /// <returns name="endIndex">The End of the Range.</returns>
        private (int startIndex, int endIndex) VerifyRange(Range range, int count)
        {
            int startIndex = range.Start.IsFromEnd ? count - range.Start.Value - 1 : range.Start.Value;
            int endIndex = range.End.IsFromEnd ? count - range.End.Value - 1 : range.End.Value;

            if (startIndex < 0 || endIndex >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(range));
            }

            return (startIndex, endIndex);
        }
#endif

        /// <summary>
        /// Used for Verifying the availability of Row or Column int Array Indexes.
        /// </summary>
        /// <param name="array">Row or Column int Array Indexes</param>
        /// <param name="count">RowCount or ColumnCount of the Matrix</param>
        private void VerifyArray(int[] array, int count)
        {
            if (array.Length <= 1)
            {
                throw new ArgumentException("The Length of int array must Larger than One!");
            }

            if (array.Min() < 0 || array.Max() >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(array));
            }
        }

        /// <summary>
        /// Used for Verifying the availability of Row or Column ValueTuple (start, end) struct Indexes.
        /// </summary>
        /// <param name="vt">Row or Column ValueTuple (strat, end) struct Indexes</param>
        /// <param name="count">RowCount or ColumnCount of the Matrix</param>
        private void VerifyValueTuple(ValueTuple<int, int> vt, int count)
        {
            if (vt.Item1 < 0 || vt.Item2 >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(vt));
            }

            if (vt.Item2 <= vt.Item1)
            {
                throw new ArgumentException("The Length of ValueTuple must Larger than One!");
            }
        }

        /// <summary>
        /// Used for Verifying the availability of Row or Column ValueTuple (start, step, end) struct Indexes.
        /// </summary>
        /// <param name="vt">Row or Column ValueTuple (start, step, end) struct Indexes</param>
        /// <param name="count">RowCount or ColumnCount of the Matrix</param>
        private void VerifyValueTuple(ValueTuple<int, int, int> vt, int count)
        {
            if (vt.Item1 < 0 || vt.Item3 >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(vt));
            }

            if (vt.Item3 <= vt.Item1)
            {
                throw new ArgumentException("The Length of ValueTuple must Larger than One!");
            }
        }

        /// <summary>
        /// Used for Verifying the availability of Row or Column int Single Index.
        /// </summary>
        /// <param name="index">Row or Column int Single Index</param>
        /// <param name="count">RowCount or ColumnCount of the Matrix</param>
        private void VerifySingleIndex(int index, int count)
        {
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        #endregion

    }
}
