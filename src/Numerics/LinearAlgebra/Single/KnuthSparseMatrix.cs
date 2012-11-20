using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.LinearAlgebra.Single
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MathNet.Numerics.LinearAlgebra.Storage;
    using Generic;
    using Properties;
    using Threading;

    /// <summary>
    /// A Matrix class with sparse storage. The underlying storage scheme is pointers to lefter and upper elements Format.
    /// Algorithm was posted in "The Art of Computer Programming" - Knuth - 1968
    /// </summary>
    public class KnuthSparseMatrix : Matrix
    {
        /// <summary>
        /// Storage element
        /// </summary>
        private KnuthSparseMatrixStorage<float> storage;

        /// <summary>
        /// Creates a <c>KnuthSparseMatrix</c> for the given number of rows and columns.
        /// </summary>
        /// <param name="numberOfRows">The number of rows.</param>
        /// <param name="numberOfColumns">The number of columns.</param>
        /// <param name="fullyMutable">True if all fields must be mutable (e.g. not a diagonal matrix).</param>
        /// <returns>
        /// A <c>SparseMatrix</c> with the given dimensions.
        /// </returns>
        public override Matrix<float> CreateMatrix(int numberOfRows, int numberOfColumns, bool fullyMutable = false)
        {
            return new KnuthSparseMatrix(numberOfRows, numberOfColumns);
        }

        /// <summary>
        /// Creates a <see cref="SparseVector"/> with a the given dimension.
        /// </summary>
        /// <param name="size">The size of the vector.</param>
        /// <param name="fullyMutable">True if all fields must be mutable.</param>
        /// <returns>
        /// A <see cref="SparseVector"/> with the given dimension.
        /// </returns>
        public override Vector<float> CreateVector(int size, bool fullyMutable = false)
        {
            return new SparseVector(size);
        }

        /// <summary>
        /// Gets the number of non zero elements in the matrix.
        /// </summary>
        /// <value>The number of non zero elements.</value>
        public int NonZerosCount()
        {
            int result = 0;
            for (int i = 0; i < storage.Rows.Count; ++i)
            {
                KnuthNode<float> iter = storage.Rows[i];
                while (iter.Left != storage.Rows[i])
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
        /// <param name="row">number of row</param>
        /// <param name="col">number of column</param>
        /// <param name="val">value to insert</param>
        public void At(int row, int col, float val)
        {
            storage.At(row, col, val);
        }

        /// <summary>
        /// Gets a value at current possition
        /// </summary>
        /// <param name="row">number of row</param>
        /// <param name="col">number of column</param>
        public float At(int row, int col)
        {
            return storage.At(row, col);
        }

        /// <summary>
        /// Check if elements are equals
        /// </summary>
        /// <param name="obj">opject to compare</param>
        /// <returns>if they are equals</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Gets hash code of element
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KnuthSparseMatrix"/> class.
        /// </summary>
        /// <param name="_storage">
        /// Saved at KnuthSparseMatrixStorage matrix.
        /// </param>
        internal KnuthSparseMatrix(KnuthSparseMatrixStorage<float> _storage)
            : base(_storage)
        {
            storage = _storage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KnuthSparseMatrix"/> class.
        /// </summary>
        /// <param name="_storage">
        /// Saved at KnuthSparseMatrixStorage matrix.
        /// </param>
        internal KnuthSparseMatrix(KnuthSparseMatrix mtx)
            : base(mtx.storage)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KnuthSparseMatrix"/> class.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        public KnuthSparseMatrix(int rows, int columns)
            : this(new KnuthSparseMatrixStorage<float>(rows, columns, 0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KnuthSparseMatrix"/> class. This matrix is square with a given size.
        /// </summary>
        /// <param name="order">the size of the square matrix.</param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="order"/> is less than one.
        /// </exception>
        public KnuthSparseMatrix(int order)
            : this(order, order)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SparseMatrix"/> class from a 2D array. 
        /// </summary>
        /// <param name="array">The 2D array to create this matrix from.</param>
        public KnuthSparseMatrix(float[,] array)
            : this(array.GetLength(0), array.GetLength(1))
        {
            for (var i = 0; i < storage.RowCount; i++)
            {
                for (var j = 0; j < storage.ColumnCount; j++)
                {
                    storage.At(i, j, array[i, j]);
                }
            }
        }

        /// <summary>
        /// Create new instanse of sparse matrix 
        /// </summary>
        /// <param name="mtx">matrix in list<list> presentation to set as sparse</param>
        public KnuthSparseMatrix(List<List<float>> array)
            : this(array.Count, array[0].Count)
        {
            for (var i = 0; i < storage.RowCount; i++)
            {
                for (var j = 0; j < storage.ColumnCount; j++)
                {
                    storage.At(i, j, array[i][j]);
                }
            }
        }
        /// <summary>
        /// Change signs of all values to opposite
        /// </summary>
        public void Negate()
        {
            KnuthNode<float> rw;
            for (int i = 0; i < storage.Rows.Count; ++i)
            {
                rw = storage.Rows[i];
                while (rw.Left != storage.Rows[i])
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
            if (mtx1.storage.Cols.Count != mtx2.storage.Rows.Count)
                throw new Exception("Wrong size of matrix to multiple");
            KnuthSparseMatrix Res = new KnuthSparseMatrix(mtx1.storage.Rows.Count, mtx2.storage.Cols.Count);
            float tmp;
            KnuthNode<float> Iter1, Iter2;
            for (int i = 0; i < mtx1.storage.Rows.Count; ++i)
            {
                for (int j = 0; j < mtx2.storage.Cols.Count; ++j)
                {
                    tmp = 0; //initalize mult of i-th row and j-th column
                    Iter1 = mtx1.storage.Rows[i].Left;
                    Iter2 = mtx2.storage.Cols[j].Up;
                    while (Iter1 != mtx1.storage.Rows[i] && Iter2 != mtx2.storage.Cols[j])
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
                        Res.At(i, j, tmp);
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
            if (mtx1.storage.Rows.Count != mtx2.storage.Rows.Count || mtx1.storage.Cols.Count != mtx2.storage.Cols.Count)
                throw new Exception("Wrong size of matrix to summ");
            KnuthSparseMatrix Res = new KnuthSparseMatrix(mtx1.storage.Rows.Count, mtx1.storage.Cols.Count);
            KnuthNode<float> Iter1, Iter2;
            for (int i = 0; i < mtx1.storage.Rows.Count; ++i)
            {
                Iter1 = mtx1.storage.Rows[i].Left;
                Iter2 = mtx2.storage.Rows[i].Left;
                while (Iter1 != mtx1.storage.Rows[i] || Iter2 != mtx2.storage.Rows[i])
                {
                    if (Iter1 == mtx1.storage.Rows[i])
                    {
                        Res.At(Iter2.Row, Iter2.Col, Iter2.Val);
                        Iter2 = Iter2.Left;
                    }
                    else if (Iter2 == mtx2.storage.Rows[i])
                    {
                        Res.At(Iter1.Row, Iter1.Col, Iter1.Val);
                        Iter1 = Iter1.Left;
                    }
                    else if (Iter1.Col == Iter2.Col)
                    {
                        Res.At(Iter1.Row, Iter1.Col, Iter1.Val + Iter2.Val);
                        Iter1 = Iter1.Left;
                        Iter2 = Iter2.Left;
                    }
                    else if (Iter1.Col < Iter2.Col)
                    {
                        Res.At(Iter2.Row, Iter2.Col, Iter2.Val);
                        Iter2 = Iter2.Left;
                    }
                    else //(Iter2.Col < Iter1.Col)
                    {
                        Res.At(Iter1.Row, Iter1.Col, Iter1.Val);
                        Iter1 = Iter1.Left;
                    }
                }
            }
            return Res;
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
            return mtx1.Equals(mtx2);
        }
        /// <summary>
        /// Check if two matrixes aren`t equals
        /// </summary>
        /// <param name="mtx1">first matrix</param>
        /// <param name="mtx2">second matrix</param>
        /// <returns>true if matrixes are not equals, else - false</returns>
        public static bool operator !=(KnuthSparseMatrix mtx1, KnuthSparseMatrix mtx2)
        {
            return !mtx1.Equals(mtx2);
        }

        /// <summary>
        /// Get current matrix in full standart form
        /// </summary>
        /// <returns>matrix in full form</returns>
        public float[][] getFullMatrix()
        {
            return storage.getFullMatrix();
        }
    }
}
