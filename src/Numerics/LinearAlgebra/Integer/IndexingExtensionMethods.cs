using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.Properties;

namespace MathNet.Numerics.LinearAlgebra
{
    public static class IndexingExtensionMethods
    {
        //TODO: These are ALL ripe for optimizing based on the underlying storage types
        // That would move them into the Storage implementation classes.

        // Unchecked
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

        // Checked
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
            if (rows > m.RowCount)
            {
                throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexRows");
            }

            if (cols > m.ColumnCount)
            {
                throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexColumns");
            }

            var source = m.Storage;
            var result = Matrix<T>.Build.SameAs(m, rows, cols);
            var dest = result.Storage;
            for (int r = 0; r < rows; r++)
            {
                int rx = indexRows.At(r);
                for (int c = 0; c < cols; c++)
                {
                    dest[r, c] = source[rx, indexColumns.At(c)];
                }
            }
            return result;
        }

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
            if (rows > m.RowCount)
            {
                throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexRows");
            }

            if (cols > m.ColumnCount)
            {
                throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexColumns");
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
            if (cols > m.ColumnCount)
            {
                throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexColumns");
            }

            var source = m.Storage;
            var result = Vector<T>.Build.SameAs(m, cols);
            var dest = result.Storage;
            for (int c = 0; c < cols; c++)
            {
                dest[c] = source[row, indexColumns.At(c)];
            }
            return result;
        }

        public static Matrix<T> SubMatrix<T>(this Matrix<T> m, int row, Vector<int> indexColumns) where T : struct, IEquatable<T>, IFormattable
        {
            return SubRow(m, row, indexColumns).ToRowMatrix();
        }

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
            if (cols > m.ColumnCount)
            {
                throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexColumns");
            }

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

            int rows = indexRows.Count;
            if (rows > m.RowCount)
            {
                throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexRows");
            }

            if (!column.IsValidColumnIndex(m))
            {
                throw new ArgumentOutOfRangeException("column");
            }

            var source = m.Storage;
            var result = Vector<T>.Build.SameAs(m, rows);
            var dest = result.Storage;
            for (int r = 0; r < rows; r++)
            {
                dest[r] = source[indexRows.At(r), column];
            }
            return result;
        }

        public static Matrix<T> SubMatrix<T>(this Matrix<T> m, Vector<int> indexRows, int column) where T : struct, IEquatable<T>, IFormattable
        {
            return SubColumn(m, indexRows, column).ToColumnMatrix();
        }

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
            if (rows > m.RowCount)
            {
                throw new ArgumentException("Vector length must not exceed corresponding Matrix dimension", "indexRows");
            }

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

        // Simple index bound checking
        public static bool IsValidRowIndex<T>(this int index, Matrix<T> m) where T : struct, IEquatable<T>, IFormattable
        {
            return m != null && index >= 0 && index < m.RowCount;
        }

        public static bool IsValidColumnIndex<T>(this int index, Matrix<T> m) where T : struct, IEquatable<T>, IFormattable
        {
            return m != null && index >= 0 && index < m.ColumnCount;
        }

        public static bool IsValidIndex<T>(this int index, Vector<T> v) where T : struct, IEquatable<T>, IFormattable
        {
            return v != null && index >= 0 && index < v.Count;
        }

    }
}
