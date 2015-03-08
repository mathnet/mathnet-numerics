// <copyright file="MatrixStructureTheory.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Integer
{
    [TestFixture, Category("LA")]
    public class MatrixStructureTheory : MatrixStructureTheory<int>
    {
        [Datapoints]
        Matrix<int>[] _matrices =
        {
            Matrix<int>.Build.DenseOfArray(new[,] { { 1, 1, 2 }, { 1, 1, 2 }, { 1, 1, 2 } }),
            Matrix<int>.Build.DenseOfArray(new[,] { { -11, -22, -33 }, { 0, 11, 22 }, { -44, 55, 66 } }),
            Matrix<int>.Build.DenseOfArray(new[,] { { -11, -22, -33, -44 }, { 0, 11, 22, 33 }, { 1, 21, 62, 43 }, { -44, 55, 66, -77 } }),
            Matrix<int>.Build.DenseOfArray(new[,] { { -11, -22, -33, -44 }, { -11, -22, -33, -44 }, { -11, -22, -33, -44 }, { -11, -22, -33, -44 } }),
            Matrix<int>.Build.DenseOfArray(new[,] { { -11, -22 }, { 0, 11 }, { -44, 55 } }),
            Matrix<int>.Build.DenseOfArray(new[,] { { -11, -22, -33 }, { 0, 11, 22 } }),
            Matrix<int>.Build.DenseOfArray(new[,] { { 1, 2, 3 }, { 2, 2, 0 }, { 3, 0, 3 } }),

            Matrix<int>.Build.SparseOfArray(new[,] { { 7, 1, 2 }, { 1, 1, 2 }, { 1, 1, 2 } }),
            Matrix<int>.Build.SparseOfArray(new[,] { { 7, 1, 2 }, { 1, 0, 0 }, { -2, 0, 0 } }),
            Matrix<int>.Build.SparseOfArray(new[,] { { -11, 0, 0 }, { 0, 11, 22 } }),

            Matrix<int>.Build.Diagonal(3, 3, new[] { 1, -2, 15 }),
            Matrix<int>.Build.Diagonal(3, 3, new[] { 1, 0, -15 }),

            new UserDefinedMatrix(new[,] { { 0, 1, 2 }, { -1, 77, 0 }, { -2, 0, 0 } })
        };

        [Datapoints]
        int[] _scalars = { 2, -15, 0 };
    }
}
