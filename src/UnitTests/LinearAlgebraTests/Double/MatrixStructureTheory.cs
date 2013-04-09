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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using Distributions;
    using LinearAlgebra.Double;
    using LinearAlgebra.Generic;
    using Numerics.Random;
    using NUnit.Framework;
    using System.Linq;

    [TestFixture]
    public class MatrixStructureTheory : MatrixStructureTheory<double>
    {
        public MatrixStructureTheory()
            : base(0d, typeof (DenseMatrix), typeof (SparseMatrix), typeof (DiagonalMatrix), typeof (DenseVector), typeof (SparseVector))
        {
        }

        [Datapoints]
        Matrix<double>[] _matrices = new Matrix<double>[]
            {
                DenseMatrix.OfArray(new[,] {{1d, 1d, 2d}, {1d, 1d, 2d}, {1d, 1d, 2d}}),
                DenseMatrix.OfArray(new[,] {{-1.1d, -2.2d, -3.3d}, {0d, 1.1d, 2.2d}, {-4.4d, 5.5d, 6.6d}}),
                DenseMatrix.OfArray(new[,] {{-1.1d, -2.2d, -3.3d, -4.4d}, {0d, 1.1d, 2.2d, 3.3d}, {1d, 2.1d, 6.2d, 4.3d}, {-4.4d, 5.5d, 6.6d, -7.7d}}),
                DenseMatrix.OfArray(new[,] {{-1.1d, -2.2d, -3.3d, -4.4d}, {-1.1d, -2.2d, -3.3d, -4.4d}, {-1.1d, -2.2d, -3.3d, -4.4d}, {-1.1d, -2.2d, -3.3d, -4.4d}}),
                DenseMatrix.OfArray(new[,] {{-1.1d, -2.2d}, {0d, 1.1d}, {-4.4d, 5.5d}}),
                DenseMatrix.OfArray(new[,] {{-1.1d, -2.2d, -3.3d}, {0d, 1.1d, 2.2d}}),
                DenseMatrix.OfArray(new[,] {{1d, 2d, 3d}, {2d, 2d, 0d}, {3d, 0d, 3d}}),

                SparseMatrix.OfArray(new[,] {{7d, 1d, 2d}, {1d, 1d, 2d}, {1d, 1d, 2d}}),
                SparseMatrix.OfArray(new[,] {{7d, 1d, 2d}, {1d, 0d, 0d}, {-2d, 0d, 0d}}),
                SparseMatrix.OfArray(new[,] {{-1.1d, 0d, 0d}, {0d, 1.1d, 2.2d}}),

                new DiagonalMatrix(3, 3, new[] {1d, -2d, 1.5d}),
                new DiagonalMatrix(3, 3, new[] {1d, 0d, -1.5d}),

                new UserDefinedMatrix(new[,] {{0d, 1d, 2d}, {-1d, 7.7d, 0d}, {-2d, 0d, 0d}})
            };

        [Datapoints]
        double[] _scalars = new[] {2d, -1.5d, 0d};

        protected override Matrix<double> CreateDenseZero(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        protected override Matrix<double> CreateDenseRandom(int rows, int columns, int seed)
        {
            var dist = new Normal {RandomSource = new MersenneTwister(seed)};
            return new DenseMatrix(rows, columns, dist.Samples().Take(rows*columns).ToArray());
        }

        protected override Matrix<double> CreateSparseZero(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }

        protected override Vector<double> CreateVectorZero(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector<double> CreateVectorRandom(int size, int seed)
        {
            var dist = new Normal {RandomSource = new MersenneTwister(seed)};
            return new DenseVector(dist.Samples().Take(size).ToArray());
        }
    }
}
