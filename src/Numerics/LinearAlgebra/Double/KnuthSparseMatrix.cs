using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.LinearAlgebra.Double
{

    /// <summary>
    /// A Matrix class with sparse storage. The underlying storage scheme is pointers to lefter and upper elements Format.
    /// Algorithm was posted in "The Art of Computer Programming" - Knuth - 1968
    /// </summary>
    /// 
    class KnuthSparseMatrix
    {
        /// <summary>
        /// list to begin rows elements
        /// </summary>
        private List<KnuthNode> Rows;
        /// <summary>
        /// list to begin cols elements
        /// </summary>
        private List<KnuthNode> Cols;

        /// <summary>
        /// Gets the number of non zero elements in the matrix.
        /// </summary>
        /// <value>The number of non zero elements.</value>
        public int NonZerosCount()
        {
            int result = 0;
            for (int i = 0; i < Rows.Count; ++i)
            {
                KnuthNode iter = Rows[i];
                while (iter.Left != Rows[i])
                {
                    ++result;
                    iter = iter.Left;
                }
            }
            return result;
        }

        /// <summary>
        /// Insert new value to current possition
        /// </summary>
        /// <param name="x">number of row</param>
        /// <param name="y">number of column</param>
        /// <param name="val">value to insert</param>
        private void Insert(int x, int y, double val)
        {
            if (val == 0)
            {
                Delete(x, y);
                return;
            }
            KnuthNode rw = Rows[x];
            KnuthNode cl = Cols[y];
            KnuthNode toInsert = new KnuthNode(x, y, val);
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
            KnuthNode rw = Rows[x].Left, prewRw = Rows[x];
            KnuthNode cl = Cols[y].Up, prewCl = Cols[y];
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
            Rows = new List<KnuthNode>(row);
            for (int i = 0; i < row; ++i)
            {
                Rows.Add(new KnuthNode());
            }
            Cols = new List<KnuthNode>(col);
            for (int i = 0; i < col; ++i)
            {
                Cols.Add(new KnuthNode());
            }
        }
        /// <summary>
        /// Create new instanse of sparse matrix 
        /// </summary>
        /// <param name="row">number of rows</param>
        /// <param name="col">number of columns</param>
        public KnuthSparseMatrix(int row, int col)
        {
            SetBounds(row, col);
        }

        /// <summary>
        /// Create new instanse of sparse matrix 
        /// </summary>
        /// <param name="mtx">matrix in standart presentation to set as sparse</param>
        public KnuthSparseMatrix(double[][] mtx)
        {
            SetBounds(mtx.Length, mtx[0].Length);
            for (int i = 0; i < mtx.Length; ++i)
            {
                for (int j = 0; j < mtx[i].Length; ++j)
                {
                    if (mtx[i][j] != 0)
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
        public KnuthSparseMatrix(List<List<double>> mtx)
        {
            SetBounds(mtx.Count, mtx[0].Count);
            for (int i = 0; i < mtx.Count; ++i)
            {
                for (int j = 0; j < mtx[i].Count; ++j)
                {
                    if (mtx[i][j] != 0)
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
        public KnuthSparseMatrix(KnuthSparseMatrix mtx)
        {
            KnuthNode rw;
            SetBounds(mtx.Rows.Count, mtx.Cols.Count);
            for (int i = 0; i < Rows.Count; ++i)
            {
                rw = Rows[i].Left;
                while (rw != Rows[i])
                {
                    Insert(rw.Row, rw.Col, rw.Val);
                    rw = rw.Left;
                }
            }
        }

        /// <summary>
        /// Change signs of all values to opposite
        /// </summary>
        public void Negate()
        {
            KnuthNode rw;
            for (int i = 0; i < Rows.Count; ++i)
            {
                rw = Rows[i];
                while (rw.Left != Rows[i])
                {
                    rw.Val *= -1;
                    rw = rw.Left;
                }
            }
        }

        /// <summary>
        /// Multiply two sparse matrix
        /// </summary>
        /// <param name="mtx1">first matrix</param>
        /// <param name="mtx2">second matrix</param>
        /// <returns>result of multiplying this matrix</returns>
        public static KnuthSparseMatrix operator *(KnuthSparseMatrix mtx1, KnuthSparseMatrix mtx2)
        {
            if (mtx1.Cols.Count != mtx2.Rows.Count)
                throw new Exception("Wrong size of matrix to multiple");
            KnuthSparseMatrix Res = new KnuthSparseMatrix(mtx1.Rows.Count, mtx2.Cols.Count);
            double tmp;
            KnuthNode Iter1, Iter2;
            for (int i = 0; i < mtx1.Rows.Count; ++i)
            {
                for (int j = 0; j < mtx2.Cols.Count; ++j)
                {
                    tmp = 0; //initalize mult of i-th row and j-th column
                    Iter1 = mtx1.Rows[i].Left;
                    Iter2 = mtx2.Cols[j].Up;
                    while (Iter1 != mtx1.Rows[i] && Iter2 != mtx2.Cols[j])
                    {
                        if (Iter1.Col == Iter2.Row)
                        {
                            tmp += Iter1.Val * Iter2.Val;
                            Iter1 = Iter1.Left;
                            Iter2 = Iter2.Up;
                        }
                        else if (Iter1.Col < Iter2.Row)
                        {
                            Iter2 = Iter2.Up;
                        }
                        else //(Iter2.Col < Iter1.Col)
                        {
                            Iter1 = Iter1.Left;
                        }
                    }
                    if (tmp != 0) // paste not 0 element
                    {
                        Res.Insert(i, j, tmp);
                    }

                }
            }
            return Res;
        }

        /// <summary>
        /// Adds two matrixes
        /// </summary>
        /// <param name="mtx1">first matrix</param>
        /// <param name="mtx2">second matrix</param>
        /// <returns>result of summing this matrixes</returns>
        public static KnuthSparseMatrix operator +(KnuthSparseMatrix mtx1, KnuthSparseMatrix mtx2)
        {
            if (mtx1.Rows.Count != mtx2.Rows.Count || mtx1.Cols.Count != mtx2.Cols.Count)
                throw new Exception("Wrong size of matrix to summ");
            KnuthSparseMatrix Res = new KnuthSparseMatrix(mtx1.Rows.Count, mtx1.Cols.Count);
            KnuthNode Iter1, Iter2;
            for (int i = 0; i < mtx1.Rows.Count; ++i)
            {
                Iter1 = mtx1.Rows[i].Left;
                Iter2 = mtx2.Rows[i].Left;
                while (Iter1 != mtx1.Rows[i] || Iter2 != mtx2.Rows[i])
                {
                    if (Iter1 == mtx1.Rows[i])
                    {
                        Res.Insert(Iter2.Row, Iter2.Col, Iter2.Val);
                        Iter2 = Iter2.Left;
                    }
                    else if (Iter2 == mtx2.Rows[i])
                    {
                        Res.Insert(Iter1.Row, Iter1.Col, Iter1.Val);
                        Iter1 = Iter1.Left;
                    }
                    else if (Iter1.Col == Iter2.Col)
                    {
                        Res.Insert(Iter1.Row, Iter1.Col, Iter1.Val + Iter2.Val);
                        Iter1 = Iter1.Left;
                        Iter2 = Iter2.Left;
                    }
                    else if (Iter1.Col < Iter2.Col)
                    {
                        Res.Insert(Iter2.Row, Iter2.Col, Iter2.Val);
                        Iter2 = Iter2.Left;
                    }
                    else //(Iter2.Col < Iter1.Col)
                    {
                        Res.Insert(Iter1.Row, Iter1.Col, Iter1.Val);
                        Iter1 = Iter1.Left;
                    }
                }
            }
            return Res;
        }
        /// <summary>
        /// Substacts two matrixes
        /// </summary>
        /// <param name="mtx1">first matrix</param>
        /// <param name="mtx2">second matrix</param>
        /// <returns>result of substruction of this matrixes</returns>
        public static KnuthSparseMatrix operator -(KnuthSparseMatrix mtx1, KnuthSparseMatrix mtx2)
        {
            if (mtx1.Rows.Count != mtx2.Rows.Count || mtx1.Cols.Count != mtx2.Cols.Count)
                throw new Exception("Wrong size of matrix to summ");
            return mtx1 + (-mtx2);
        }

        /// <summary>
        /// Change signs of all values to opposite
        /// </summary>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        public static KnuthSparseMatrix operator -(KnuthSparseMatrix rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }
            KnuthSparseMatrix res = new KnuthSparseMatrix(rightSide);
            res.Negate();
            return res;
        }

        /// <summary>
        /// Check if two matrixes is equals
        /// </summary>
        /// <param name="mtx1">first matrix</param>
        /// <param name="mtx2">second matrix</param>
        /// <returns>true if matrixes are equal, else false</returns>
        public static bool operator ==(KnuthSparseMatrix mtx1, KnuthSparseMatrix mtx2)
        {
            if (mtx1.Rows.Count != mtx2.Rows.Count || mtx1.Cols.Count != mtx2.Cols.Count)
                return false;
            KnuthNode Iter1, Iter2;
            //check rows 
            for (int i = 0; i < mtx1.Rows.Count; ++i)
            {
                Iter1 = mtx1.Rows[i];
                Iter2 = mtx2.Rows[i];
                //empty and not empty row
                if (Iter1.Left == Iter1 && Iter2.Left != Iter2)
                    return false;
                if (Iter2.Left == Iter2 && Iter1.Left != Iter1)
                    return false;
                while (Iter1.Left != mtx1.Rows[i])
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
            for (int i = 0; i < mtx1.Cols.Count; ++i)
            {
                Iter1 = mtx1.Cols[i];
                Iter2 = mtx2.Cols[i];
                //empty and not empty coll
                if (Iter1.Up == Iter1 && Iter2.Up != Iter2)
                    return false;
                if (Iter2.Up == Iter2 && Iter1.Up != Iter1)
                    return false;
                while (Iter1.Up != mtx1.Cols[i])
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
        /// <summary>
        /// Check if two matrixes aren`t equals
        /// </summary>
        /// <param name="mtx1">first matrix</param>
        /// <param name="mtx2">second matrix</param>
        /// <returns>true if matrixes are not equals, else - false</returns>
        public static bool operator !=(KnuthSparseMatrix mtx1, KnuthSparseMatrix mtx2)
        {
            return !(mtx1 == mtx2);
        }

        /// <summary>
        /// Get current matrix in full standart form
        /// </summary>
        /// <returns>matrix in full form</returns>
        public double[][] getFullMatrix()
        {
            double[][] res = new double[Rows.Count][];
            for (int i = 0; i < Rows.Count; ++i)
            {
                res[i] = new double[Cols.Count];
            }
            //initialize
            KnuthNode iterator;
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
    }
}
