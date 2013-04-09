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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    using Distributions;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Single;
    using Numerics.Random;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class MatrixStructureTheory : MatrixStructureTheory<float>
    {
        public MatrixStructureTheory()
            : base(0f, typeof(DenseMatrix), typeof(SparseMatrix), typeof(DiagonalMatrix), typeof(DenseVector), typeof(SparseVector))
        {
        }

        [Datapoints]
        Matrix<float>[] _matrices = new Matrix<float>[]
            {
                DenseMatrix.OfArray(new[,] {{1f, 1f, 2f}, {1f, 1f, 2f}, {1f, 1f, 2f}}),
                DenseMatrix.OfArray(new[,] {{-1.1f, -2.2f, -3.3f}, {0f, 1.1f, 2.2f}, {-4.4f, 5.5f, 6.6f}}),
                DenseMatrix.OfArray(new[,] {{-1.1f, -2.2f, -3.3f, -4.4f}, {0f, 1.1f, 2.2f, 3.3f}, {1f, 2.1f, 6.2f, 4.3f}, {-4.4f, 5.5f, 6.6f, -7.7f}}),
                DenseMatrix.OfArray(new[,] {{-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}}),
                DenseMatrix.OfArray(new[,] {{-1.1f, -2.2f}, {0f, 1.1f}, {-4.4f, 5.5f}}),
                DenseMatrix.OfArray(new[,] {{-1.1f, -2.2f, -3.3f}, {0f, 1.1f, 2.2f}}),
                DenseMatrix.OfArray(new[,] {{1f, 2f, 3f}, {2f, 2f, 0f}, {3f, 0f, 3f}}),

                SparseMatrix.OfArray(new[,] {{7f, 1f, 2f}, {1f, 1f, 2f}, {1f, 1f, 2f}}),
                SparseMatrix.OfArray(new[,] {{7f, 1f, 2f}, {1f, 0f, 0f}, {-2f, 0f, 0f}}),
                SparseMatrix.OfArray(new[,] {{-1.1f, 0f, 0f}, {0f, 1.1f, 2.2f}}),

                new DiagonalMatrix(3, 3, new[] {1f, -2f, 1.5f}),
                new DiagonalMatrix(3, 3, new[] {1f, 0f, -1.5f}),

                new UserDefinedMatrix(new[,] {{0f, 1f, 2f}, {-1f, 7.7f, 0f}, {-2f, 0f, 0f}})
            };

        [Datapoints]
        float[] _scalars = new[] {2f, -1.5f, 0f};

        protected override Matrix<float> CreateDenseZero(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        protected override Matrix<float> CreateDenseRandom(int rows, int columns, int seed)
        {
            var dist = new Normal {RandomSource = new MersenneTwister(seed)};
            return new DenseMatrix(rows, columns, dist.Samples().Select(d => (float) d).Take(rows*columns).ToArray());
        }

        protected override Matrix<float> CreateSparseZero(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }

        protected override Vector<float> CreateVectorZero(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector<float> CreateVectorRandom(int size, int seed)
        {
            var dist = new Normal {RandomSource = new MersenneTwister(seed)};
            return new DenseVector(dist.Samples().Select(d => (float) d).Take(size).ToArray());
        }
    }
}
