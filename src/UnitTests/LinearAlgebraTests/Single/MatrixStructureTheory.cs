// <copyright file="MatrixStructureTheory.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    [TestFixture, Category("LA")]
    public class MatrixStructureTheory : MatrixStructureTheory<float>
    {
        protected override Matrix<float> Get(TestMatrix matrix)
        {
            return TestData.Matrix(matrix);
        }

        [Datapoints]
        TestMatrix[] _matrices =
        {
            TestMatrix.DenseSquare3x3,
            TestMatrix.DenseSquare3x3b,
            TestMatrix.DenseSquare4x4,
            TestMatrix.DenseSquare4x4b,
            TestMatrix.DenseTall3x2,
            TestMatrix.DenseWide2x3,
            TestMatrix.DenseSquare3x3c,

            TestMatrix.SparseSquare3x3,
            TestMatrix.SparseSquare3x3b,
            TestMatrix.SparseWide2x3,

            TestMatrix.DiagonalSquare3x3,
            TestMatrix.DiagonalSquare3x3b,

            TestMatrix.UserSquare3x3
        };

        [Datapoints]
        float[] _scalars = { 2f, -1.5f, 0f };
    }
}
