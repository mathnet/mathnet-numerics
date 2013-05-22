// <copyright file="Ilutp.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.LinearAlgebra.Complex32.Solvers.Preconditioners
{
    using System;
    using System.Collections.Generic;
    using Numerics;
    using Properties;

    /// <summary>
    /// This class performs an Incomplete LU factorization with drop tolerance
    /// and partial pivoting. The drop tolerance indicates which additional entries
    /// will be dropped from the factorized LU matrices.
    /// </summary>
    /// <remarks>
    /// The ILUTP-Mem algorithm was taken from: <br/>
    /// ILUTP_Mem: a Space-Efficient Incomplete LU Preconditioner
    /// <br/>
    /// Tzu-Yi Chen, Department of Mathematics and Computer Science, <br/>
    /// Pomona College, Claremont CA 91711, USA <br/>
    /// Published in: <br/>
    /// Lecture Notes in Computer Science <br/>
    /// Volume 3046 / 2004 <br/>
    /// pp. 20 - 28 <br/>
    /// Algorithm is described in Section 2, page 22
    /// </remarks>
    public sealed class Ilutp : IPreConditioner
    {
        /// <summary>
        /// The default fill level.
        /// </summary>
        public const double DefaultFillLevel = 200.0;

        /// <summary>
        /// The default drop tolerance.
        /// </summary>
        public const double DefaultDropTolerance = 0.0001;
        
        /// <summary>
        /// The decomposed upper triangular matrix.
        /// </summary>
        private SparseMatrix _upper;

        /// <summary>
        /// The decomposed lower triangular matrix.
        /// </summary>
        private SparseMatrix _lower;
        
        /// <summary>
        /// The array containing the pivot values.
        /// </summary>
        private int[] _pivots;

        /// <summary>
        /// The fill level.
        /// </summary>
        private double _fillLevel = DefaultFillLevel;
        
        /// <summary>
        /// The drop tolerance.
        /// </summary>
        private double _dropTolerance = DefaultDropTolerance;
        
        /// <summary>
        /// The pivot tolerance.
        /// </summary>
        private double _pivotTolerance;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ilutp"/> class with the default settings.
        /// </summary>
        public Ilutp()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Ilutp"/> class with the specified settings.
        /// </summary>
        /// <param name="fillLevel">
        /// The amount of fill that is allowed in the matrix. The value is a fraction of 
        /// the number of non-zero entries in the original matrix. Values should be positive.
        /// </param>
        /// <param name="dropTolerance">
        /// The absolute drop tolerance which indicates below what absolute value an entry 
        /// will be dropped from the matrix. A drop tolerance of 0.0 means that no values
        /// will be dropped. Values should always be positive.
        /// </param>
        /// <param name="pivotTolerance">
        /// The pivot tolerance which indicates at what level pivoting will take place. A
        /// value of 0.0 means that no pivoting will take place.
        /// </param>
        public Ilutp(double fillLevel, double dropTolerance, double pivotTolerance)
        {
            if (fillLevel < 0)
            {
                throw new ArgumentOutOfRangeException("fillLevel");
            }

            if (dropTolerance < 0)
            {
                throw new ArgumentOutOfRangeException("dropTolerance");
            }

            if (pivotTolerance < 0)
            {
                throw new ArgumentOutOfRangeException("pivotTolerance");
            }

            _fillLevel = fillLevel;
            _dropTolerance = dropTolerance;
            _pivotTolerance = pivotTolerance;
        }
                        
        /// <summary>
        /// Gets or sets the amount of fill that is allowed in the matrix. The
        /// value is a fraction of the number of non-zero entries in the original
        /// matrix. The standard value is 200.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Values should always be positive and can be higher than 1.0. A value lower
        /// than 1.0 means that the eventual preconditioner matrix will have fewer
        /// non-zero entries as the original matrix. A value higher than 1.0 means that
        /// the eventual preconditioner can have more non-zero values than the original 
        /// matrix.
        /// </para>
        /// <para>
        /// Note that any changes to the <b>FillLevel</b> after creating the preconditioner
        /// will invalidate the created preconditioner and will require a re-initialization of
        /// the preconditioner.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a negative value is provided.</exception>
        public double FillLevel
        {
            get
            {
                return _fillLevel;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _fillLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the absolute drop tolerance which indicates below what absolute value
        /// an entry will be dropped from the matrix. The standard value is 0.0001.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The values should always be positive and can be larger than 1.0. A low value will
        /// keep more small numbers in the preconditioner matrix. A high value will remove 
        /// more small numbers from the preconditioner matrix.
        /// </para>
        /// <para>
        /// Note that any changes to the <b>DropTolerance</b> after creating the preconditioner
        /// will invalidate the created preconditioner and will require a re-initialization of
        /// the preconditioner.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a negative value is provided.</exception>
        public double DropTolerance
        {
            get
            {
                return _dropTolerance;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _dropTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets the pivot tolerance which indicates at what level pivoting will
        /// take place. The standard value is 0.0 which means pivoting will never take place.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The pivot tolerance is used to calculate if pivoting is necessary. Pivoting
        /// will take place if any of the values in a row is bigger than the 
        /// diagonal value of that row divided by the pivot tolerance, i.e. pivoting
        /// will take place if <b>row(i,j) > row(i,i) / PivotTolerance</b> for
        /// any <b>j</b> that is not equal to <b>i</b>.
        /// </para>
        /// <para>
        /// Note that any changes to the <b>PivotTolerance</b> after creating the preconditioner
        /// will invalidate the created preconditioner and will require a re-initialization of
        /// the preconditioner.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if a negative value is provided.</exception>
        public double PivotTolerance
        {
            get
            {
                return _pivotTolerance;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                _pivotTolerance = value;
            }
        }
        
        /// <summary>
        /// Returns the upper triagonal matrix that was created during the LU decomposition.
        /// </summary>
        /// <remarks>
        /// This method is used for debugging purposes only and should normally not be used.
        /// </remarks>
        /// <returns>A new matrix containing the upper triagonal elements.</returns>
        internal Matrix UpperTriangle()
        {
            return (Matrix)_upper.Clone();
        }

        /// <summary>
        /// Returns the lower triagonal matrix that was created during the LU decomposition.
        /// </summary>
        /// <remarks>
        /// This method is used for debugging purposes only and should normally not be used.
        /// </remarks>
        /// <returns>A new matrix containing the lower triagonal elements.</returns>
        internal Matrix LowerTriangle()
        {
            return (Matrix)_lower.Clone();
        }

        /// <summary>
        /// Returns the pivot array. This array is not needed for normal use because
        /// the preconditioner will return the solution vector values in the proper order.
        /// </summary>
        /// <remarks>
        /// This method is used for debugging purposes only and should normally not be used.
        /// </remarks>
        /// <returns>The pivot array.</returns>
        internal int[] Pivots()
        {
            var result = new int[_pivots.Length];
            for (var i = 0; i < _pivots.Length; i++)
            {
                result[i] = _pivots[i];
            }

            return result;
        }

        /// <summary>
        /// Initializes the preconditioner and loads the internal data structures.
        /// </summary>
        /// <param name="matrix">
        /// The <see cref="Matrix"/> upon which this preconditioner is based. Note that the 
        /// method takes a general matrix type. However internally the data is stored 
        /// as a sparse matrix. Therefore it is not recommended to pass a dense matrix.
        /// </param>
        /// <exception cref="ArgumentNullException"> If <paramref name="matrix"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="matrix"/> is not a square matrix.</exception>
        public void Initialize(Matrix matrix)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, "matrix");
            }

            var sparseMatrix = (matrix is SparseMatrix) ? matrix as SparseMatrix : SparseMatrix.OfMatrix(matrix);

            // The creation of the preconditioner follows the following algorithm.
            // spaceLeft = lfilNnz * nnz(A)
            // for i = 1, .. , n
            // {
            //    w = a(i,*)
            //    for j = 1, .. , i - 1
            //    {
            //        if (w(j) != 0)
            //        {
            //            w(j) = w(j) / a(j,j)
            //            if (w(j) < dropTol)
            //            {
            //                w(j) = 0;
            //            }
            //            if (w(j) != 0)
            //            {
            //                w = w - w(j) * U(j,*)
            //            }
            //        }
            //    }
            //
            //    for j = i, .. ,n
            //    {
            //        if w(j) <= dropTol * ||A(i,*)||
            //        {
            //            w(j) = 0
            //        }
            //    }
            //
            //    spaceRow = spaceLeft / (n - i + 1) // Determine the space for this row
            //    lfil = spaceRow / 2  // space for this row of L
            //    l(i,j) = w(j) for j = 1, .. , i -1 // only the largest lfil elements
            //
            //    lfil = spaceRow - nnz(L(i,:))  // space for this row of U
            //    u(i,j) = w(j) for j = i, .. , n // only the largest lfil - 1 elements
            //    w = 0
            //    
            //    if max(U(i,i + 1: n)) > U(i,i) / pivTol then // pivot if necessary
            //    {
            //        pivot by swapping the max and the diagonal entries
            //        Update L, U
            //        Update P
            //    }
            //    spaceLeft = spaceLeft - nnz(L(i,:)) - nnz(U(i,:))
            // }
            // Create the lower triangular matrix
            _lower = new SparseMatrix(sparseMatrix.RowCount);

            // Create the upper triangular matrix and copy the values
            _upper = new SparseMatrix(sparseMatrix.RowCount);
            
            // Create the pivot array
            _pivots = new int[sparseMatrix.RowCount];
            for (var i = 0; i < _pivots.Length; i++)
            {
                _pivots[i] = i;
            }

            Vector workVector = new DenseVector(sparseMatrix.RowCount);
            Vector rowVector = new DenseVector(sparseMatrix.ColumnCount);
            var indexSorting = new int[sparseMatrix.RowCount];
            
            // spaceLeft = lfilNnz * nnz(A)
            var spaceLeft = (int)_fillLevel * sparseMatrix.NonZerosCount;
            
            // for i = 1, .. , n
            for (var i = 0; i < sparseMatrix.RowCount; i++)
            {
                // w = a(i,*)
                sparseMatrix.Row(i, workVector);
                
                // pivot the row
                PivotRow(workVector);
                var vectorNorm = workVector.Norm(Double.PositiveInfinity);
                
                // for j = 1, .. , i - 1)
                for (var j = 0; j < i; j++)
                {
                    // if (w(j) != 0)
                    // {
                    //     w(j) = w(j) / a(j,j)
                    //     if (w(j) < dropTol)
                    //     {
                    //         w(j) = 0;
                    //     }
                    //     if (w(j) != 0)
                    //     {
                    //         w = w - w(j) * U(j,*)
                    //     }
                    if (workVector[j] != 0.0f)
                    {
                        // Calculate the multiplication factors that go into the L matrix
                        workVector[j] = workVector[j] / _upper[j, j];
                        if (workVector[j].Magnitude < _dropTolerance)
                        {
                            workVector[j] = 0.0f;
                        }

                        // Calculate the addition factor
                        if (workVector[j] != 0.0f)
                        {
                            // vector update all in one go
                            _upper.Row(j, rowVector);

                            // zero out columnVector[k] because we don't need that
                            // one anymore for k = 0 to k = j
                            for (var k = 0; k <= j; k++)
                            {
                                rowVector[k] = 0.0f;
                            }

                            rowVector.Multiply(workVector[j], rowVector);
                            workVector.Subtract(rowVector, workVector);
                        }
                    }
                }

                // for j = i, .. ,n
                for (var j = i; j < sparseMatrix.RowCount; j++)
                {
                    // if w(j) <= dropTol * ||A(i,*)||
                    // {
                    //     w(j) = 0
                    // }
                    if (workVector[j].Magnitude <= _dropTolerance * vectorNorm.Real)
                    {
                        workVector[j] = 0.0f;
                    }
                }

                // spaceRow = spaceLeft / (n - i + 1) // Determine the space for this row
                var spaceRow = spaceLeft / (sparseMatrix.RowCount - i + 1);

                // lfil = spaceRow / 2  // space for this row of L
                var fillLevel = spaceRow / 2;
                FindLargestItems(0, i - 1, indexSorting, workVector);
                
                // l(i,j) = w(j) for j = 1, .. , i -1 // only the largest lfil elements
                var lowerNonZeroCount = 0;
                var count = 0;
                for (var j = 0; j < i; j++)
                {
                    if ((count > fillLevel) || (indexSorting[j] == -1))
                    {
                        break;
                    }

                    _lower[i, indexSorting[j]] = workVector[indexSorting[j]];
                    count += 1;
                    lowerNonZeroCount += 1;
                }

                FindLargestItems(i + 1, sparseMatrix.RowCount - 1, indexSorting, workVector);
                
                // lfil = spaceRow - nnz(L(i,:))  // space for this row of U
                fillLevel = spaceRow - lowerNonZeroCount;
                
                // u(i,j) = w(j) for j = i + 1, .. , n // only the largest lfil - 1 elements
                var upperNonZeroCount = 0;
                count = 0;
                for (var j = 0; j < sparseMatrix.RowCount - i; j++)
                {
                    if ((count > fillLevel - 1) || (indexSorting[j] == -1))
                    {
                        break;
                    }

                    _upper[i, indexSorting[j]] = workVector[indexSorting[j]];
                    count += 1;
                    upperNonZeroCount += 1;
                }

                // Simply copy the diagonal element. Next step is to see if we pivot
                _upper[i, i] = workVector[i];

                // if max(U(i,i + 1: n)) > U(i,i) / pivTol then // pivot if necessary
                // {
                //     pivot by swapping the max and the diagonal entries
                //     Update L, U
                //     Update P
                // }

                // Check if we really need to pivot. If (i+1) >=(mCoefficientMatrix.Rows -1) then
                // we are working on the last row. That means that there is only one number
                // And pivoting is useless. Also the indexSorting array will only contain
                // -1 values.
                if ((i + 1) < (sparseMatrix.RowCount - 1))
                {
                    if (workVector[i].Magnitude < _pivotTolerance * workVector[indexSorting[0]].Magnitude)
                    {
                        // swap columns of u (which holds the values of A in the
                        // sections that haven't been partitioned yet.
                        SwapColumns(_upper, i, indexSorting[0]);
                        
                        // Update P
                        var temp = _pivots[i];
                        _pivots[i] = _pivots[indexSorting[0]];
                        _pivots[indexSorting[0]] = temp;
                    }
                }

                // spaceLeft = spaceLeft - nnz(L(i,:)) - nnz(U(i,:))
                spaceLeft -= lowerNonZeroCount + upperNonZeroCount;
            }

            for (var i = 0; i < _lower.RowCount; i++)
            {
                _lower[i, i] = 1.0f;
            }
        }

        /// <summary>
        /// Pivot elements in the <paramref name="row"/> according to internal pivot array
        /// </summary>
        /// <param name="row">Row <see cref="Vector"/> to pivot in</param>
        private void PivotRow(Vector row)
        {
            var knownPivots = new Dictionary<int, int>();
            
            // pivot the row
            for (var i = 0; i < row.Count; i++)
            {
                if ((_pivots[i] != i) && (!PivotMapFound(knownPivots, i)))
                {
                    // store the pivots in the hashtable
                    knownPivots.Add(_pivots[i], i);

                    var t = row[i];
                    row[i] = row[_pivots[i]];
                    row[_pivots[i]] = t;
                }
            }
        }

        /// <summary>
        /// Was pivoting already performed
        /// </summary>
        /// <param name="knownPivots">Pivots already done</param>
        /// <param name="currentItem">Current item to pivot</param>
        /// <returns><c>true</c> if performed, otherwise <c>false</c></returns>
        private bool PivotMapFound(Dictionary<int, int> knownPivots, int currentItem)
        {
            if (knownPivots.ContainsKey(_pivots[currentItem]))
            {
                if (knownPivots[_pivots[currentItem]].Equals(currentItem))
                {
                    return true;
                }
            }

            if (knownPivots.ContainsKey(currentItem))
            {
                if (knownPivots[currentItem].Equals(_pivots[currentItem]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Swap columns in the <see cref="Matrix"/>
        /// </summary>
        /// <param name="matrix">Source <see cref="Matrix"/>.</param>
        /// <param name="firstColumn">First column index to swap</param>
        /// <param name="secondColumn">Second column index to swap</param>
        private static void SwapColumns(Matrix matrix, int firstColumn, int secondColumn)
        {
            for (var i = 0; i < matrix.RowCount; i++)
            {
                var temp = matrix[i, firstColumn];
                matrix[i, firstColumn] = matrix[i, secondColumn];
                matrix[i, secondColumn] = temp;
            }
        }

        /// <summary>
        /// Sort vector descending, not changing vector but placing sorted indicies to <paramref name="sortedIndices"/>
        /// </summary>
        /// <param name="lowerBound">Start sort form</param>
        /// <param name="upperBound">Sort till upper bound</param>
        /// <param name="sortedIndices">Array with sorted vector indicies</param>
        /// <param name="values">Source <see cref="Vector"/></param>
        private static void FindLargestItems(int lowerBound, int upperBound, int[] sortedIndices, Vector values)
        {
            // Copy the indices for the values into the array
            for (var i = 0; i < upperBound + 1 - lowerBound; i++)
            {
                sortedIndices[i] = lowerBound + i;
            }

            for (var i = upperBound + 1 - lowerBound; i < sortedIndices.Length; i++)
            {
                sortedIndices[i] = -1;
            }

            // Sort the first set of items.
            // Sorting starts at index 0 because the index array
            // starts at zero
            // and ends at index upperBound - lowerBound
            IlutpElementSorter.SortDoubleIndicesDecreasing(0, upperBound - lowerBound, sortedIndices, values);
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <returns>The left hand side vector.</returns>
        public Vector Approximate(Vector rhs)
        {
            if (rhs == null)
            {
                throw new ArgumentNullException("rhs");
            }

            if (_upper == null)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDoesNotExist);
            }

            if (rhs.Count != _upper.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rhs");
            }

            Vector result = new DenseVector(rhs.Count);
            Approximate(rhs, result);
            return result;
        }

        /// <summary>
        /// Approximates the solution to the matrix equation <b>Ax = b</b>.
        /// </summary>
        /// <param name="rhs">The right hand side vector.</param>
        /// <param name="lhs">The left hand side vector. Also known as the result vector.</param>
        public void Approximate(Vector rhs, Vector lhs)
        {
            if (rhs == null)
            {
                throw new ArgumentNullException("rhs");
            }

            if (lhs == null)
            {
                throw new ArgumentNullException("lhs");
            }

            if (_upper == null)
            {
                throw new ArgumentException(Resources.ArgumentMatrixDoesNotExist);
            }

            if ((lhs.Count != rhs.Count) || (lhs.Count != _upper.RowCount))
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rhs");
            }
            
            // Solve equation here
            // Pivot(vector, result);
            // Solve L*Y = B(piv,:)
            Vector rowValues = new DenseVector(_lower.RowCount);
            for (var i = 0; i < _lower.RowCount; i++)
            {
                _lower.Row(i, rowValues);

                var sum = Complex32.Zero;
                for (var j = 0; j < i; j++)
                {
                    sum += rowValues[j] * lhs[j];
                }

                lhs[i] = rhs[i] - sum;
            }

            // Solve U*X = Y;
            for (var i = _upper.RowCount - 1; i > -1; i--)
            {
                _upper.Row(i, rowValues);

                var sum = Complex32.Zero;
                for (var j = _upper.RowCount - 1; j > i; j--)
                {
                    sum += rowValues[j] * lhs[j];
                }

                lhs[i] = 1 / rowValues[i] * (lhs[i] - sum);
            }

            // We have a column pivot so we only need to pivot the
            // end result not the incoming right hand side vector
            var temp = (Vector)lhs.Clone();
            
            Pivot(temp, lhs);
        }

        /// <summary>
        /// Pivot elements in <see cref="Vector"/> according to internal pivot array
        /// </summary>
        /// <param name="vector">Source <see cref="Vector"/>.</param>
        /// <param name="result">Result <see cref="Vector"/> after pivoting.</param>
        private void Pivot(Vector vector, Vector result)
        {
            for (var i = 0; i < _pivots.Length; i++)
            {
                result[i] = vector[_pivots[i]];
            }
        }
    }
}
