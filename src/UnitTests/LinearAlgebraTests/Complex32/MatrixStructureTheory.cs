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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using Distributions;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Generic;
    using Numerics.Random;
    using NUnit.Framework;
    using System.Linq;
    using Complex32 = Numerics.Complex32;

    [TestFixture]
    public class MatrixStructureTheory : MatrixStructureTheory<Complex32>
    {
        public MatrixStructureTheory()
            : base(Complex32.Zero, typeof(DenseMatrix), typeof(SparseMatrix), typeof(DiagonalMatrix), typeof(DenseVector), typeof(SparseVector))
        {
        }

        [Datapoints]
        Matrix<Complex32>[] _matrices = new Matrix<Complex32>[]
            {
                DenseMatrix.OfArray(new[,] {{1f, new Complex32(1.1f, -4f), 2f}, {1f, 1f, 2f}, {1f, new Complex32(1f, 2f), 2f}}),
                DenseMatrix.OfArray(new[,] {{-1.1f, -2.2f, -3.3f}, {0f, 1.1f, new Complex32(2.2f, -1.2f)}, {-4.4f, 5.5f, 6.6f}}),
                DenseMatrix.OfArray(new[,] {{new Complex32(-1.1f, -2f), -2.2f, -3.3f, -4.4f}, {0f, 1.1f, 2.2f, 3.3f}, {1f, 2.1f, 6.2f, 4.3f}, {-4.4f, 5.5f, 6.6f, -7.7f}}),
                DenseMatrix.OfArray(new[,] {{-1.1f, new Complex32(-2.2f, 3.4f), -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}}),
                DenseMatrix.OfArray(new[,] {{-1.1f, -2.2f}, {Complex32.Zero, 1.1f}, {-4.4f, 5.5f}}),
                DenseMatrix.OfArray(new[,] {{-1.1f, -2.2f, -3.3f}, {0f, new Complex32(1.1f, 0.1f), 2.2f}}),
                DenseMatrix.OfArray(new[,] {{1f, 2f, 3f}, {2f, new Complex32(2f, 2f), 0f}, {3f, Complex32.Zero, 3f}}),

                SparseMatrix.OfArray(new[,] {{7f, 1f, 2f}, {1f, 1f, 2f}, {1f, 1f + Complex32.ImaginaryOne, 2f}}),
                SparseMatrix.OfArray(new[,] {{7f, 1f, 2f}, {new Complex32(1f, 2f), 0f, Complex32.Zero}, {-2f, 0f, 0f}}),
                SparseMatrix.OfArray(new[,] {{-1.1f, 0f, 0f}, {0f, new Complex32(1.1f, 2f), 2.2f}}),

                new DiagonalMatrix(3, 3, new[] {new Complex32(1f, 1f), -2f, 1.5f}),
                new DiagonalMatrix(3, 3, new[] {new Complex32(1f, 2f), 0f, -1.5f}),

                new UserDefinedMatrix(new[,] {{0f, 1f, 2f}, {-1f, 7.7f, 0f}, {-2f, Complex32.Zero, 0f}})
            };

        [Datapoints]
        Complex32[] scalars = new[] {new Complex32(2f, 0f), new Complex32(-1.5f, 3.5f), Complex32.Zero};

        protected override Matrix<Complex32> CreateDenseZero(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        protected override Matrix<Complex32> CreateDenseRandom(int rows, int columns, int seed)
        {
            var dist = new Normal {RandomSource = new MersenneTwister(seed)};
            return new DenseMatrix(rows, columns, Enumerable.Range(0, rows*columns).Select(k => new Complex32((float) dist.Sample(), (float) dist.Sample())).ToArray());
        }

        protected override Matrix<Complex32> CreateSparseZero(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }

        protected override Vector<Complex32> CreateVectorZero(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector<Complex32> CreateVectorRandom(int size, int seed)
        {
            var dist = new Normal {RandomSource = new MersenneTwister(seed)};
            return new DenseVector(Enumerable.Range(0, size).Select(k => new Complex32((float) dist.Sample(), (float) dist.Sample())).ToArray());
        }
    }
}
