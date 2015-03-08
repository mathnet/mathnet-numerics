using System;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra
{
    /// <summary>
    /// This is a collection of Extension methods to allow for indexing by <c>Vector&lt;int&gt;</c>
    /// </summary>
    /// <remarks>These could be moved into the <c>Matrix&lt;T&gt;</c> and <c>Vector&lt;T&gt;</c> classes 
    /// and possibly into the Storage implementation classes for optimization.
    /// WARNING: The indexing methods are not thread safe. Use "lock" with them and be sure to avoid deadlocks.</remarks>
    public static class IndexingExtensionMethods
    {
        // Unchecked
        
        /// <summary>
        /// This returns a newly constructed matrix based on the rows and columns specified by <paramref name="indexRows"/> 
        /// and <paramref name="indexColumns"/> without range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in construction of the result</param>
        /// <returns>A newly constructed <c>Matrix&lt;T&gt;</c>. 
        /// This will have dimensions <c><paramref name="indexRows"/>.Count</c> x <c><paramref name="indexColumns"/>.Count</c>.</returns>
        public static Matrix<T> At<T>(this Matrix<T> m, Vector<int> indexRows, Vector<int> indexColumns) where T : struct, IEquatable<T>, IFormattable
        {
            var source = m.Storage;
            int rows = indexRows.Count;
            int cols = indexColumns.Count;
            var result = Matrix<T>.Build.SameAs(m, rows, cols);
            var dest = result.Storage;
            for (int r = 0; r < rows; r++)
            {
                int rx = indexRows.At(r);
                for (int c = 0; c < cols; c++)
                {
                    dest.At(r, c, source.At(rx, indexColumns.At(c)));
                }
            }
            return result;
        }

        /// <summary>
        /// Set a subset of the elements of <paramref name="m"/> based on the rows and columns specified by <paramref name="indexRows"/> 
        /// and <paramref name="indexColumns"/> to the values in <paramref name="newValues"/> without range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> to be updated with the values in <paramref name="newValues"/></param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="newValues">The values to be set into <paramref name="m"/>.
        /// This should have dimensions <c><paramref name="indexRows"/>.Count</c> x <c><paramref name="indexColumns"/>.Count</c>.</param>
        public static void At<T>(this Matrix<T> m, Vector<int> indexRows, Vector<int> indexColumns, Matrix<T> newValues) where T : struct, IEquatable<T>, IFormattable
        {
            var source = newValues.Storage;
            int rows = indexRows.Count;
            int cols = indexColumns.Count;
            var dest = m.Storage;
            for (int r = 0; r < rows; r++)
            {
                int rx = indexRows.At(r);
                for (int c = 0; c < cols; c++)
                {
                    dest.At(rx, indexColumns.At(c), source.At(r, c));
                }
            }
        }

        /// <summary>
        /// This returns a newly constructed row-matrix based on the single <paramref name="row"/> 
        /// and columns specified by <paramref name="indexColumns"/> without range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="row">The index of the row within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in construction of the result</param>
        /// <returns>A newly constructed <c>Matrix&lt;T&gt;</c> 
        /// This will have dimensions <c>1</c> x <c><paramref name="indexColumns"/>.Count</c>.</returns>
        public static Matrix<T> At<T>(this Matrix<T> m, int row, Vector<int> indexColumns) where T : struct, IEquatable<T>, IFormattable
        {
            int cols = indexColumns.Count;
            var source = m.Storage;
            var result = Matrix<T>.Build.SameAs(m, 1, cols);
            var dest = result.Storage;
            for (int c = 0; c < cols; c++)
            {
                dest.At(0, c, source.At(row, indexColumns.At(c)));
            }
            return result;
        }

        /// <summary>
        /// Set a subset of the elements of <paramref name="m"/> based on the single <paramref name="row"/>
        /// and columns specified by <paramref name="indexColumns"/> to the values in <paramref name="newValues"/> without range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> to be updated with the values in <paramref name="newValues"/></param>
        /// <param name="row">The index of the row within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="newValues">The vector of values to be set into <paramref name="m"/>.
        /// This should have length <c><paramref name="indexColumns"/>.Count</c>.</param>
        public static void At<T>(this Matrix<T> m, int row, Vector<int> indexColumns, Vector<T> newValues) where T : struct, IEquatable<T>, IFormattable
        {
            int cols = indexColumns.Count;
            var source = newValues.Storage;
            var dest = m.Storage;
            for (int c = 0; c < cols; c++)
            {
                dest.At(row, indexColumns.At(c), source.At(c));
            }
        }

        /// <summary>
        /// This returns a newly constructed vector based on the single <paramref name="row"/> 
        /// and columns specified by <paramref name="indexColumns"/> without range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="row">The index of the row within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in construction of the result</param>
        /// <returns>A newly constructed <c>Vector&lt;T&gt;</c> 
        /// This will have length <c><paramref name="indexColumns"/>.Count</c>.</returns>
        public static Vector<T> AtV<T>(this Matrix<T> m, int row, Vector<int> indexColumns) where T : struct, IEquatable<T>, IFormattable
        {
            int cols = indexColumns.Count;
            var source = m.Storage;
            var result = Vector<T>.Build.SameAs(m, cols);
            var dest = result.Storage;
            for (int c = 0; c < cols; c++)
            {
                dest.At(c, source.At(row, indexColumns.At(c)));
            }
            return result;
        }

        /// <summary>
        /// This returns a newly constructed column-matrix based on the single <paramref name="column"/> 
        /// and columns specified by <paramref name="indexRows"/> without range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="column">The index of the column within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in construction of the result</param>
        /// <returns>A newly constructed <c>Matrix&lt;T&gt;</c> 
        /// This will have dimensions <c><paramref name="indexRows"/>.Count</c> x <c>1</c>.</returns>
        public static Matrix<T> At<T>(this Matrix<T> m, Vector<int> indexRows, int column) where T : struct, IEquatable<T>, IFormattable
        {
            int rows = indexRows.Count;
            var source = m.Storage;
            var result = Matrix<T>.Build.SameAs(m, rows, 1);
            var dest = result.Storage;
            for (int r = 0; r < rows; r++)
            {
                dest.At(r, 0, source.At(indexRows.At(r), column));
            }
            return result;
        }

        /// <summary>
        /// Set a subset of the elements of <paramref name="m"/> based on the single <paramref name="column"/>
        /// and rows specified by <paramref name="indexRows"/> to the values in <paramref name="newValues"/> without range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> to be updated with the values in <paramref name="newValues"/></param>
        /// <param name="column">The index of the column within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="newValues">The vector of values to be set into <paramref name="m"/>.
        /// This should have length <c><paramref name="indexRows"/>.Count</c>.</param>
        public static void At<T>(this Matrix<T> m, Vector<int> indexRows, int column, Vector<T> newValues) where T : struct, IEquatable<T>, IFormattable
        {
            int rows = indexRows.Count;
            var source = newValues.Storage;
            var dest = m.Storage;
            for (int r = 0; r < rows; r++)
            {
                dest.At(indexRows.At(r), column, source.At(r));
            }
        }

        /// <summary>
        /// This returns a newly constructed vector based on the single <paramref name="column"/> 
        /// and columns specified by <paramref name="indexRows"/> without range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="column">The index of the column within <paramref name="m"/> to use in construction of the result</param>
        /// <returns>A newly constructed <c>Vector&lt;T&gt;</c> 
        /// This will have length <c><paramref name="indexRows"/>.Count</c>.</returns>
        public static Vector<T> AtV<T>(this Matrix<T> m, Vector<int> indexRows, int column) where T : struct, IEquatable<T>, IFormattable
        {
            int rows = indexRows.Count;
            var source = m.Storage;
            var result = Vector<T>.Build.SameAs(m, rows);
            var dest = result.Storage;
            for (int r = 0; r < rows; r++)
            {
                dest.At(r, source.At(indexRows.At(r), column));
            }
            return result;
        }

        /// <summary>
        /// This returns a newly constructed vector based on the positions specified by <paramref name="indexes"/> without range checking.
        /// </summary>
        /// <param name="v">The <c>Vector&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="indexes">The indexes of the positions within <paramref name="v"/> to use in construction of the result</param>
        /// <returns>A newly constructed <c>Vector&lt;T&gt;</c> 
        /// This will have length <c><paramref name="indexes"/>.Count</c>.</returns>
        public static Vector<T> At<T>(this Vector<T> v, Vector<int> indexes) where T : struct, IEquatable<T>, IFormattable
        {
            int num = indexes.Count;
            var source = v.Storage;
            var result = Vector<T>.Build.SameAs(v, num);
            var dest = result.Storage;
            for (int n = 0; n < num; n++)
            {
                dest.At(n, source.At(indexes.At(n)));
            }
            return result;
        }

        /// <summary>
        /// Set a subset of the elements of <paramref name="v"/> based on the positions specified by <paramref name="indexes"/>
        /// to the values in <paramref name="newValues"/> without range checking.
        /// </summary>
        /// <param name="v">The <c>Vector&lt;T&gt;</c> to be updated with the values in <paramref name="newValues"/></param>
        /// <param name="indexes">The indexes of the positions within <paramref name="v"/> to update</param>
        /// <param name="newValues">The vector of values to be set into <paramref name="v"/>.
        /// This should have length <c><paramref name="indexes"/>.Count</c>.</param>
        public static void At<T>(this Vector<T> v, Vector<int> indexes, Vector<T> newValues) where T : struct, IEquatable<T>, IFormattable
        {
            int num = indexes.Count;
            var source = newValues.Storage;
            var dest = v.Storage;
            for (int n = 0; n < num; n++)
            {
                dest.At(indexes.At(n), source.At(n));
            }
        }

        // Checked

        /// <summary>
        /// This returns a newly constructed matrix based on the rows and columns specified by <paramref name="indexRows"/> 
        /// and <paramref name="indexColumns"/> with range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in construction of the result</param>
        /// <returns>A newly constructed <c>Matrix&lt;T&gt;</c>. 
        /// This will have dimensions <c><paramref name="indexRows"/>.Count</c> x <c><paramref name="indexColumns"/>.Count</c>.</returns>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static Matrix<T> SubMatrix<T>(this Matrix<T> m, Vector<int> indexRows, Vector<int> indexColumns) where T : struct, IEquatable<T>, IFormattable
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }

            if (indexRows == null)
            {
                throw new ArgumentNullException("indexRows");
            }

            if (indexColumns == null)
            {
                throw new ArgumentNullException("indexColumns");
            }

            int rows = indexRows.Count;
            int cols = indexColumns.Count;
            var source = m.Storage;
            var result = Matrix<T>.Build.SameAs(m, rows, cols);
            var dest = result.Storage;
            for (int r = 0; r < rows; r++)
            {
                int rx = indexRows.At(r);
                for (int c = 0; c < cols; c++)
                {
                    dest.At(r, c, source[rx, indexColumns.At(c)]);
                }
            }
            return result;
        }

        /// <summary>
        /// Set a subset of the elements of <paramref name="m"/> based on the rows and columns specified by <paramref name="indexRows"/> 
        /// and <paramref name="indexColumns"/> to the values in <paramref name="newValues"/> with range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> to be updated with the values in <paramref name="newValues"/></param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="newValues">The values to be set into <paramref name="m"/>.</param>
        /// <exception cref="ArgumentException">If <c><paramref name="indexRows"/>.Count</c> is not equal to <c><paramref name="newValues"/>.RowCount</c> or
        /// <c><paramref name="indexColumns"/>.Count</c> is not equal to <c><paramref name="newValues"/>.ColumnCount</c></exception>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static void SetSubMatrix<T>(this Matrix<T> m, Vector<int> indexRows, Vector<int> indexColumns, Matrix<T> newValues) where T : struct, IEquatable<T>, IFormattable
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }

            if (indexRows == null)
            {
                throw new ArgumentNullException("indexRows");
            }

            if (indexColumns == null)
            {
                throw new ArgumentNullException("indexColumns");
            }

            if (newValues == null)
            {
                throw new ArgumentNullException("newValues");
            }

            int rows = indexRows.Count;
            int cols = indexColumns.Count;
            //CONSIDER: (MattHeffron) Are these restrictions necessary? 
            // indexRows and/or indexColumns could have duplicate values and the assignments could be redundant 
            // (and possibly indeterminate if the for loops were parallelized!)
            // But both of the above could happen just as well if cols <= m.ColumnCount and rows <= m.RowCount
            // Matlab allows this to happen.
            ////if (rows > m.RowCount)
            ////{
            ////    throw new ArgumentException(string.Format(Resources.VectorLengthMustNotExceedMatrixDimension, string.Empty), "indexRows");
            ////}

            ////if (cols > m.ColumnCount)
            ////{
            ////    throw new ArgumentException(string.Format(Resources.VectorLengthMustNotExceedMatrixDimension, string.Empty), "indexColumns");
            ////}

            if (rows != newValues.RowCount)
            {
                throw new ArgumentException(string.Format(Resources.VectorLengthMustEqualMatrixDimension, "newValues "), "indexRows");
            }

            if (cols != newValues.ColumnCount)
            {
                throw new ArgumentException(string.Format(Resources.VectorLengthMustEqualMatrixDimension, "newValues "), "indexColumns");
            }

            var source = newValues.Storage;
            var dest = m.Storage;
            for (int r = 0; r < rows; r++)
            {
                int rx = indexRows.At(r);
                for (int c = 0; c < cols; c++)
                {
                    dest[rx, indexColumns.At(c)] = source[r, c];
                }
            }
        }

        /// <summary>
        /// This returns a newly constructed vector based on the single <paramref name="row"/> 
        /// and columns specified by <paramref name="indexColumns"/> with range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="row">The index of the row within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in construction of the result</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="row"/> is not a valid row index of <paramref name="m"/></exception>
        /// <returns>A newly constructed <c>Matrix&lt;T&gt;</c> 
        /// This will have length <c><paramref name="indexColumns"/>.Count</c>.</returns>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static Vector<T> SubRow<T>(this Matrix<T> m, int row, Vector<int> indexColumns) where T : struct, IEquatable<T>, IFormattable
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }

            if (indexColumns == null)
            {
                throw new ArgumentNullException("indexColumns");
            }

            if (!row.IsValidRowIndex(m))
            {
                throw new ArgumentOutOfRangeException("row");
            }

            int cols = indexColumns.Count;
            var source = m.Storage;
            var result = Vector<T>.Build.SameAs(m, cols);
            var dest = result.Storage;
            for (int c = 0; c < cols; c++)
            {
                dest.At(c, source[row, indexColumns.At(c)]);
            }
            return result;
        }

        /// <summary>
        /// This returns a newly constructed row-matrix based on the single <paramref name="row"/> 
        /// and columns specified by <paramref name="indexColumns"/> with range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="row">The index of the row within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in construction of the result</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="row"/> is not a valid row index of <paramref name="m"/></exception>
        /// <returns>A newly constructed <c>Matrix&lt;T&gt;</c> 
        /// This will have dimensions <c>1</c> x <c><paramref name="indexColumns"/>.Count</c>.</returns>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static Matrix<T> SubMatrix<T>(this Matrix<T> m, int row, Vector<int> indexColumns) where T : struct, IEquatable<T>, IFormattable
        {
            return SubRow(m, row, indexColumns).ToRowMatrix();
        }

        /// <summary>
        /// Set a subset of the elements of <paramref name="m"/> based on the single <paramref name="row"/>
        /// and columns specified by <paramref name="indexColumns"/> to the values in <paramref name="newValues"/> with range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> to be updated with the values in <paramref name="newValues"/></param>
        /// <param name="row">The index of the row within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="indexColumns">The indexes of the columns within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="newValues">The vector of values to be set into <paramref name="m"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="row"/> is not a valid row index of <paramref name="m"/></exception>
        /// <exception cref="ArgumentException">If <c><paramref name="indexColumns"/>.Count</c> is not equal to <c><paramref name="newValues"/>.Count</c></exception>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static void SetSubRow<T>(this Matrix<T> m, int row, Vector<int> indexColumns, Vector<T> newValues) where T : struct, IEquatable<T>, IFormattable
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }

           if (indexColumns == null)
            {
                throw new ArgumentNullException("indexColumns");
            }

            if (newValues == null)
            {
                throw new ArgumentNullException("newValues");
            }

            if (!row.IsValidRowIndex(m))
            {
                throw new ArgumentOutOfRangeException("row");
            }

            int cols = indexColumns.Count;
            //CONSIDER: (MattHeffron) Is this restriction necessary? 
            // indexColumns could have duplicate values and the assignments could be redundant 
            // (and possibly indeterminate if the for loop were parallelized!)
            // But both of the above could happen just as well if cols <= m.ColumnCount
            // Matlab allows this to happen.
            ////if (cols > m.ColumnCount)
            ////{
            ////    throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexColumns");
            ////}

            if (cols != newValues.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            var source = newValues.Storage;
            var dest = m.Storage;
            for (int c = 0; c < cols; c++)
            {
                dest[row, indexColumns.At(c)] = source[c];
            }
        }

        /// <summary>
        /// This returns a newly constructed vector based on the single <paramref name="column"/> 
        /// and columns specified by <paramref name="indexRows"/> with range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="column">The index of the column within <paramref name="m"/> to use in construction of the result</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="column"/> is not a valid column index of <paramref name="m"/></exception>
        /// <returns>A newly constructed <c>Vector&lt;T&gt;</c> 
        /// This will have length <c><paramref name="indexRows"/>.Count</c>.</returns>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static Vector<T> SubColumn<T>(this Matrix<T> m, Vector<int> indexRows, int column) where T : struct, IEquatable<T>, IFormattable
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }

            if (indexRows == null)
            {
                throw new ArgumentNullException("indexRows");
            }

            if (!column.IsValidColumnIndex(m))
            {
                throw new ArgumentOutOfRangeException("column");
            }

            int rows = indexRows.Count;
            var source = m.Storage;
            var result = Vector<T>.Build.SameAs(m, rows);
            var dest = result.Storage;
            for (int r = 0; r < rows; r++)
            {
                dest.At(r, source[indexRows.At(r), column]);
            }
            return result;
        }

        /// <summary>
        /// This returns a newly constructed column-matrix based on the single <paramref name="column"/> 
        /// and columns specified by <paramref name="indexRows"/> with range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="column">The index of the column within <paramref name="m"/> to use in construction of the result</param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in construction of the result</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="column"/> is not a valid column index of <paramref name="m"/></exception>
        /// <returns>A newly constructed <c>Matrix&lt;T&gt;</c> 
        /// This will have dimensions <c><paramref name="indexRows"/>.Count</c> x <c>1</c>.</returns>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static Matrix<T> SubMatrix<T>(this Matrix<T> m, Vector<int> indexRows, int column) where T : struct, IEquatable<T>, IFormattable
        {
            return SubColumn(m, indexRows, column).ToColumnMatrix();
        }

        /// <summary>
        /// Set a subset of the elements of <paramref name="m"/> based on the single <paramref name="column"/>
        /// and rows specified by <paramref name="indexRows"/> to the values in <paramref name="newValues"/> with range checking.
        /// </summary>
        /// <param name="m">The <c>Matrix&lt;T&gt;</c> to be updated with the values in <paramref name="newValues"/></param>
        /// <param name="column">The index of the column within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="indexRows">The indexes of the rows within <paramref name="m"/> to use in determining the positions to update</param>
        /// <param name="newValues">The vector of values to be set into <paramref name="m"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="column"/> is not a valid column index of <paramref name="m"/></exception>
        /// <exception cref="ArgumentException">If <c><paramref name="indexRows"/>.Count</c> is not equal to <c><paramref name="newValues"/>.Count</c></exception>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static void SetSubColumn<T>(this Matrix<T> m, Vector<int> indexRows, int column, Vector<T> newValues) where T : struct, IEquatable<T>, IFormattable
        {
            if (m == null)
            {
                throw new ArgumentNullException("m");
            }

            if (indexRows == null)
            {
                throw new ArgumentNullException("indexRows");
            }

            if (newValues == null)
            {
                throw new ArgumentNullException("newValues");
            }

            int rows = indexRows.Count;
            //CONSIDER: (MattHeffron) Is this restriction necessary? 
            // indexRows could have duplicate values and the assignments could be redundant 
            // (and possibly indeterminate if the for loop were parallelized!)
            // But both of the above could happen just as well if rows <= m.RowCount
            // Matlab allows this to happen.
            ////if (rows > m.RowCount)
            ////{
            ////    throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexRows");
            ////}

            if (rows != newValues.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            if (!column.IsValidColumnIndex(m))
            {
                throw new ArgumentOutOfRangeException("column");
            }

            var source = newValues.Storage;
            var dest = m.Storage;
            for (int r = 0; r < rows; r++)
            {
                dest[indexRows.At(r), column] = source[r];
            }
        }

        /// <summary>
        /// This returns a newly constructed vector based on the positions specified by <paramref name="indexes"/> with range checking.
        /// </summary>
        /// <param name="v">The <c>Vector&lt;T&gt;</c> from which to take the values for the constructed result</param>
        /// <param name="indexes">The indexes of the positions within <paramref name="v"/> to use in construction of the result</param>
        /// <returns>A newly constructed <c>Vector&lt;T&gt;</c> 
        /// This will have length <c><paramref name="indexes"/>.Count</c>.</returns>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static Vector<T> SubVector<T>(this Vector<T> v, Vector<int> indexes) where T : struct, IEquatable<T>, IFormattable
        {
            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            if (indexes == null)
            {
                throw new ArgumentNullException("indexes");
            }

            int num = indexes.Count;
            var source = v.Storage;
            var result = Vector<T>.Build.SameAs(v, num);
            var dest = result.Storage;
            for (int r = 0; r < num; r++)
            {
                dest.At(r, source[indexes.At(r)]);
            }
            return result;
        }

        /// <summary>
        /// Set a subset of the elements of <paramref name="v"/> based on the positions specified by <paramref name="indexes"/>
        /// to the values in <paramref name="newValues"/> without range checking.
        /// </summary>
        /// <param name="v">The <c>Vector&lt;T&gt;</c> to be updated with the values in <paramref name="newValues"/></param>
        /// <param name="indexes">The indexes of the positions within <paramref name="v"/> to update</param>
        /// <param name="newValues">The vector of values to be set into <paramref name="v"/>.</param>
        /// <exception cref="ArgumentException">If <c><paramref name="indexes"/>.Count</c> is not equal to <c><paramref name="newValues"/>.Count</c></exception>
        /// <remarks>This depends on range checking being performed at the MatrixStorage level.</remarks>
        public static void SetSubVector<T>(this Vector<T> v, Vector<int> indexes, Vector<T> newValues) where T : struct, IEquatable<T>, IFormattable
        {
            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            if (indexes == null)
            {
                throw new ArgumentNullException("indexes");
            }

            if (newValues == null)
            {
                throw new ArgumentNullException("newValues");
            }

            int num = indexes.Count;
            //CONSIDER: (MattHeffron) Is this restriction necessary? 
            // indexes could have duplicate values and the assignments could be redundant 
            // (and possibly indeterminate if the for loop were parallelized!)
            // But both of the above could happen just as well if num <= v.Count
            // Matlab allows this to happen.
            ////if (num > v.Count)
            ////{
            ////    throw new ArgumentException("Index vector length must not exceed Vector length", "indexes");
            ////}

            if (num != newValues.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            var source = newValues.Storage;
            var dest = v.Storage;
            for (int r = 0; r < num; r++)
            {
                dest[indexes.At(r)] = source[r];
            }
        }

        // Simple index bound checking

        /// <summary>
        /// Checks if an integer value is a valid row index into a <c>Matrix&lt;T&gt;</c>
        /// <example>
        /// Allows for "fluent" coding:
        /// <code>
        ///   Matrix&lt;T&gt; mat = ...;
        ///   if (3.IsValidRowIndex(mat)) ...
        /// </code></example>
        /// </summary>
        /// <param name="index">integer value to check</param>
        /// <param name="m">The matrix against which to check the <paramref name="index"/></param>
        /// <returns><c>true</c> if <paramref name="m"/> is not <c>null</c> and <paramref name="index"/> is &gt;= 0 and &lt; <paramref name="m"/>.RowCount</returns>
        public static bool IsValidRowIndex<T>(this int index, Matrix<T> m) where T : struct, IEquatable<T>, IFormattable
        {
            return m != null && index >= 0 && index < m.RowCount;
        }

        /// <summary>
        /// Checks if an integer value is a valid column index into a <c>Matrix&lt;T&gt;</c>
        /// <example>
        /// Allows for "fluent" coding:
        /// <code>
        ///   Matrix&lt;T&gt; mat = ...;
        ///   if (3.IsValidColumnIndex(mat)) ...
        /// </code></example>
        /// </summary>
        /// <param name="index">integer value to check</param>
        /// <param name="m">The matrix against which to check the <paramref name="index"/></param>
        /// <returns><c>true</c> if <paramref name="m"/> is not <c>null</c> and <paramref name="index"/> is &gt;= 0 and &lt; <paramref name="m"/>.ColumnCount</returns>
        public static bool IsValidColumnIndex<T>(this int index, Matrix<T> m) where T : struct, IEquatable<T>, IFormattable
        {
            return m != null && index >= 0 && index < m.ColumnCount;
        }

        /// <summary>
        /// Checks if an integer value is a valid index into a <c>Vector&lt;T&gt;</c>
        /// <example>
        /// Allows for "fluent" coding:
        /// <code>
        ///   Vector&lt;T&gt; vec = ...;
        ///   if (3.IsValidIndex(vec)) ...
        /// </code></example>
        /// </summary>
        /// <param name="index">integer value to check</param>
        /// <param name="v">The vector against which to check the <paramref name="index"/></param>
        /// <returns><c>true</c> if <paramref name="v"/> is not <c>null</c> and <paramref name="index"/> is &gt;= 0 and &lt; <paramref name="v"/>.Count</returns>
        public static bool IsValidIndex<T>(this int index, Vector<T> v) where T : struct, IEquatable<T>, IFormattable
        {
            return v != null && index >= 0 && index < v.Count;
        }

    }
}
