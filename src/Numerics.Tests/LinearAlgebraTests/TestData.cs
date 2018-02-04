// <copyright file="TestData.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
    public enum TestMatrix
    {
        DenseSquare3x3,
        DenseSquare3x3b,
        DenseSquare4x4,
        DenseSquare4x4b,
        DenseTall3x2,
        DenseWide2x3,
        DenseSquare3x3c,

        SparseSquare3x3,
        SparseSquare3x3b,
        SparseWide2x3,

        DiagonalSquare3x3,
        DiagonalSquare3x3b,

        UserSquare3x3
    }

    public enum TestMatrixStorage
    {
        DenseMatrix = 1,
        SparseMatrix = 2,
        DiagonalMatrix = 3
    }

    public enum TestVector
    {
        Dense5,
        Dense5WithZeros,

        Sparse5,
        Sparse5WithZeros,
        Sparse5AllZeros,
        SparseMaxLengthAllZeros
    }

    public enum TestVectorStorage
    {
        DenseVector = 1,
        SparseVector = 2
    }

    public static class TestData
    {
        public static VectorStorage<T> VectorStorage<T>(TestVectorStorage type, IEnumerable<T> data)
            where T : struct, IEquatable<T>, IFormattable
        {
            switch (type)
            {
                case TestVectorStorage.DenseVector:
                    return DenseVectorStorage<T>.OfEnumerable(data);
                case TestVectorStorage.SparseVector:
                    return SparseVectorStorage<T>.OfEnumerable(data);
                default:
                    throw new NotSupportedException();
            }
        }

        public static VectorStorage<T> VectorStorage<T>(TestVectorStorage type, int length)
            where T : struct, IEquatable<T>, IFormattable
        {
            switch (type)
            {
                case TestVectorStorage.DenseVector:
                    return new DenseVectorStorage<T>(length);
                case TestVectorStorage.SparseVector:
                    return new SparseVectorStorage<T>(length);
                default:
                    throw new NotSupportedException();
            }
        }

        public static MatrixStorage<T> MatrixStorage<T>(TestMatrixStorage type, T[,] data)
            where T : struct, IEquatable<T>, IFormattable
        {
            switch (type)
            {
                case TestMatrixStorage.DenseMatrix:
                    return DenseColumnMajorMatrixStorage<T>.OfArray(data);
                case TestMatrixStorage.SparseMatrix:
                    return SparseCompressedRowMatrixStorage<T>.OfArray(data);
                case TestMatrixStorage.DiagonalMatrix:
                    return DiagonalMatrixStorage<T>.OfArray(data);
                default:
                    throw new NotSupportedException();
            }
        }

        public static MatrixStorage<T> MatrixStorage<T>(TestMatrixStorage type, int rows, int columns)
            where T : struct, IEquatable<T>, IFormattable
        {
            switch (type)
            {
                case TestMatrixStorage.DenseMatrix:
                    return new DenseColumnMajorMatrixStorage<T>(rows, columns);
                case TestMatrixStorage.SparseMatrix:
                    return new SparseCompressedRowMatrixStorage<T>(rows, columns);
                case TestMatrixStorage.DiagonalMatrix:
                    return new DiagonalMatrixStorage<T>(rows, columns);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}