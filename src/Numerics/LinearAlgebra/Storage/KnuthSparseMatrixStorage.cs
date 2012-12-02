
namespace MathNet.Numerics.LinearAlgebra.Storage
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// class to storage matrix using algorithm of Knuth
    /// </summary>
    public class KnuthSparseMatrixStorage<T> : MatrixStorage<T>
        where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Define zero element for cuurnt type
        /// </summary>
        readonly T zero;

        /// <summary>
        /// list to begin rows elements
        /// </summary>
        public List<KnuthNode<T>> Rows;

        /// <summary>
        /// list to begin cols elements
        /// </summary>
        public List<KnuthNode<T>> Cols;

        /// <summary>
        /// Compare to elements if they equals
        /// </summary>
        /// <param name="mtx2">element to compare</param>
        /// <returns>if there quals</returns>
        public bool Equals(KnuthSparseMatrixStorage<T> mtx2)
        {
            if (this.Rows.Count != mtx2.Rows.Count || this.Cols.Count != mtx2.Cols.Count)
                return false;
            KnuthNode<T> Iter1, Iter2;
            //check rows 
            for (int i = 0; i < this.Rows.Count; ++i)
            {
                Iter1 = this.Rows[i];
                Iter2 = mtx2.Rows[i];
                //empty and not empty row
                if (Iter1.Left == Iter1 && Iter2.Left != Iter2)
                    return false;
                if (Iter2.Left == Iter2 && Iter1.Left != Iter1)
                    return false;
                while (Iter1.Left != this.Rows[i])
                {
                    //row of second mtx allready ends
                    if (Iter2.Left == mtx2.Rows[i])
                        return false;
                    //not equal elements
                    if (Iter1 != Iter2)
                        return false;
                    Iter1 = Iter1.Left;
                    Iter2 = Iter2.Left;
                }
                //row of second mtx not allready ends
                if (Iter2.Left != mtx2.Rows[i])
                    return false;
            }
            //check cols 
            for (int i = 0; i < this.Cols.Count; ++i)
            {
                Iter1 = this.Cols[i];
                Iter2 = mtx2.Cols[i];
                //empty and not empty coll
                if (Iter1.Up == Iter1 && Iter2.Up != Iter2)
                    return false;
                if (Iter2.Up == Iter2 && Iter1.Up != Iter1)
                    return false;
                while (Iter1.Up != this.Cols[i])
                {
                    //coll of second mtx allready ends
                    if (Iter2.Up == mtx2.Cols[i])
                        return false;
                    //not equal elements
                    if (Iter1 != Iter2)
                        return false;
                    Iter1 = Iter1.Up;
                    Iter2 = Iter2.Up;
                }
                //row of second mtx not allready ends
                if (Iter2.Up != mtx2.Cols[i])
                    return false;
            }
            //all elements are equals
            return true;
        }

        public override bool Equals(MatrixStorage<T> other)
        {
            return base.Equals(other);
        }

        /// <summary>
        /// Gets element at current position
        /// </summary>
        /// <param name="row">number of row</param>
        /// <param name="column">number of column</param>
        /// <returns>value on this position</returns>
        public override T At(int row, int column)
        {
            return Get(row, column);
        }

        /// <summary>
        /// Sets element to current position
        /// </summary>
        /// <param name="row"></param>
        /// <param name="row">number of row</param>
        /// <param name="column">number of column</param>
        public override void At(int row, int column, T val)
        {
            Insert(row, column, val);
        }

        /// <summary>
        /// Insert new value to current possition
        /// </summary>
        /// <param name="x">number of row</param>
        /// <param name="y">number of column</param>
        /// <param name="val">value to insert</param>
        private void Insert(int x, int y, T val)
        {
            if (val.Equals(zero)) //var == 0
            {
                Delete(x, y);
                return;
            }
            KnuthNode<T> rw = Rows[x];
            KnuthNode<T> cl = Cols[y];
            KnuthNode<T> toInsert = new KnuthNode<T>(x, y, val, zero);
            //search needed row
            while (rw.Left != Rows[x] && rw.Left.Col >= y)
            {
                rw = rw.Left;
            }
            //insert to row
            if (rw.Col == y) // element allready exist
            {
                rw.Val = val;
            }
            else
            {
                toInsert.Left = rw.Left;
                rw.Left = toInsert;
            }
            //search needed column
            while (cl.Up != Cols[y] && cl.Up.Row >= x)
            {
                cl = cl.Up;
            }
            //insert to column
            if (cl.Row == x) // element allready exist
            {
                cl.Val = val;
            }
            else
            {
                toInsert.Up = cl.Up;
                cl.Up = toInsert;
            }
        }

        /// <summary>
        /// Delete current element
        /// </summary>
        /// <param name="x">number of row</param>
        /// <param name="y">number of column</param>
        private void Delete(int x, int y)
        {
            KnuthNode<T> rw = Rows[x].Left, prewRw = Rows[x];
            KnuthNode<T> cl = Cols[y].Up, prewCl = Cols[y];
            bool haver = false, havec = false;
            for (int i = 0; i < Rows.Count; ++i)
            {
                if (rw.Col == y)
                {
                    haver = true;
                    break;
                }
                prewRw = rw;
                rw = rw.Left;
            }
            for (int i = 0; i < Cols.Count; ++i)
            {
                if (cl.Row == x)
                {
                    havec = true;
                    break;
                }
                prewCl = cl;
                cl = cl.Up;
            }
            if (havec && haver)
            {
                prewCl.Up = cl.Up;
                prewRw.Left = rw.Left;
            }
        }

        /// <summary>
        /// Set bounds of new sparse matrix
        /// </summary>
        /// <param name="row">count of rows</param>
        /// <param name="col">count of cols</param>
        private void SetBounds(int row, int col)
        {
            Rows = new List<KnuthNode<T>>(row);
            for (int i = 0; i < row; ++i)
            {
                Rows.Add(new KnuthNode<T>(zero));
            }
            Cols = new List<KnuthNode<T>>(col);
            for (int i = 0; i < col; ++i)
            {
                Cols.Add(new KnuthNode<T>(zero));
            }
        }

        /// <summary>
        /// Create new instanse of sparse matrix 
        /// </summary>
        /// <param name="row">number of rows</param>
        /// <param name="col">number of columns</param>
        internal KnuthSparseMatrixStorage(int rows, int columns, T _zero)
            : base(rows, columns)
        {
            zero = _zero;
            SetBounds(rows, columns);
        }

        /// <summary>
        /// Create new instanse of sparse matrix 
        /// </summary>
        /// <param name="mtx">matrix in standart presentation to set as sparse</param>
        internal KnuthSparseMatrixStorage(int rows, int columns, T[][] mtx, T _zero)
            : base(rows, columns)
        {
            zero = _zero;
            SetBounds(mtx.Length, mtx[0].Length);
            for (int i = 0; i < mtx.Length; ++i)
            {
                for (int j = 0; j < mtx[i].Length; ++j)
                {
                    if (!mtx[i][j].Equals(zero))
                    {
                        Insert(i, j, mtx[i][j]);
                    }
                }
            }
        }

        /// <summary>
        /// Create new instanse of sparse matrix 
        /// </summary>
        /// <param name="mtx">matrix in list<list> presentation to set as sparse</param>
         internal KnuthSparseMatrixStorage(int rows, int columns, List<List<T>> mtx, T _zero)
            : base(rows, columns)
        {
             zero = _zero;
            SetBounds(mtx.Count, mtx[0].Count);
            for (int i = 0; i < mtx.Count; ++i)
            {
                for (int j = 0; j < mtx[i].Count; ++j)
                {
                    if (!mtx[i][j].Equals(zero))
                    {
                        Insert(i, j, mtx[i][j]);
                    }
                }
            }
        }

        /// <summary>
        /// Copy existing instanse of matrix to new matrix
        /// </summary>
        /// <param name="mtx">matrix to copy</param>
        public KnuthSparseMatrixStorage<T> Copy()
        {
            KnuthSparseMatrixStorage<T> res = new KnuthSparseMatrixStorage<T>(this.Rows.Count, this.Cols.Count, zero);
            KnuthNode<T> rw;
            for (int i = 0; i < Rows.Count; ++i)
            {
                rw = Rows[i].Left;
                while (rw != Rows[i])
                {
                    res.Insert(rw.Row, rw.Col, rw.Val);
                    rw = rw.Left;
                }
            }
            return res;
        }

        /// <summary>
        /// Gets element at curent position
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public T Get(int row, int column)
        {
            KnuthNode<T> rw = Rows[row].Left, prewRw = Rows[row];
            KnuthNode<T> cl = Cols[column].Up, prewCl = Cols[column];
            bool haver = false, havec = false;
            for (int i = 0; i < Rows.Count; ++i)
            {
                if (rw.Col == column)
                {
                    haver = true;
                    break;
                }
                prewRw = rw;
                rw = rw.Left;
            }
            for (int i = 0; i < Cols.Count; ++i)
            {
                if (cl.Row == row)
                {
                    havec = true;
                    break;
                }
                prewCl = cl;
                cl = cl.Up;
            }
            if (havec && haver)
            {
                return cl.Val;
            }
            else
            {
                return zero;
            }
        }

        /// <summary>
        /// Gets or sets the value at the given row and column.
        /// </summary>
        /// <param name="row">
        /// The row of the element.
        /// </param>
        /// <param name="column">
        /// The column of the element.
        /// </param>
        /// <value>The value to get or set.</value>
        //public T this[int row, int column]
        //{
        //    get
        //    {
        //        if(row< 0 || row >= Rows.Count)
        //            throw new ArgumentOutOfRangeException("row");
        //        if (column < 0 || column >= Cols.Count)
        //            throw new ArgumentOutOfRangeException("column");
        //        return Get(row, column);
        //    }

        //    set
        //    {
        //        if (row < 0 || row >= Rows.Count)
        //            throw new ArgumentOutOfRangeException("row");
        //        if (column < 0 || column >= Cols.Count)
        //            throw new ArgumentOutOfRangeException("column");
        //        Insert(row, column, value);
        //    }
        //}

        public override void CopyTo(MatrixStorage<T> target, bool skipClearing = false)
        {
            base.CopyTo(target, skipClearing);
        }
        ///// <summary>
        ///// Check if two matrixes is equals
        ///// </summary>
        ///// <param name="mtx1">first matrix</param>
        ///// <param name="mtx2">second matrix</param>
        ///// <returns>true if matrixes are equal, else false</returns>
        //public static bool operator ==(KnuthSparseMatrixStorage<T> mtx1, KnuthSparseMatrixStorage<T> mtx2)
        //{
        //    return mtx1.Equals(mtx2);
        //}
        ///// <summary>
        ///// Check if two matrixes aren`t equals
        ///// </summary>
        ///// <param name="mtx1">first matrix</param>
        ///// <param name="mtx2">second matrix</param>
        ///// <returns>true if matrixes are not equals, else - false</returns>
        //public static bool operator !=(KnuthSparseMatrixStorage<T> mtx1, KnuthSparseMatrixStorage<T> mtx2)
        //{
        //    return !mtx1.Equals(mtx2);
        //}

        /// <summary>
        /// Get current matrix in full standart form
        /// </summary>
        /// <returns>matrix in full form</returns>
        public T[][] getFullMatrix()
        {
            T[][] res = new T[Rows.Count][];
            for (int i = 0; i < Rows.Count; ++i)
            {
                res[i] = new T[Cols.Count];
            }
            //initialize
            KnuthNode<T> iterator;
            for (int i = 0; i < Rows.Count; ++i)
            {
                iterator = Rows[i].Left;
                while (iterator != Rows[i])
                {
                    res[iterator.Row][iterator.Col] = iterator.Val;
                    iterator = iterator.Left;
                }
            }
            return res;
        }

        ///// <summary>
        ///// Compare to elements if they equals
        ///// </summary>
        ///// <param name="mtx2">element to compare</param>
        ///// <returns>if there quals</returns>
        //public override bool Equals(object obj)
        //{
        //    return base.Equals(obj);
        //}

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            int RowCount = Rows.Count;
            int ColumnCount = Cols.Count;
            var hashNum = Math.Min(RowCount * ColumnCount, 25);
            int hash = 17;
            unchecked
            {
                for (var i = 0; i < hashNum; i++)
                {
                    var col = i % ColumnCount;
                    var row = (i - col) / RowCount;
                    hash = hash * 31 + Get(row, col).GetHashCode();
                }
            }
            return hash;
        }
    }
}
