using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
    using LinearAlgebra.Complex;
    using LinearAlgebra.Generic;
    using NUnit.Framework;
    using System.Numerics;

    [TestFixture]
    public class MatrixStructureTheory : MatrixStructureTheory<Complex>
    {
        [Datapoints]
        Matrix<Complex>[] _matrices = new Matrix<Complex>[]
            {
                new DenseMatrix(new[,] {{1d, new Complex(1.1d, -4d), 2d}, {1d, 1d, 2d}, {1d, new Complex(1d, 2d), 2d}}),
                new DenseMatrix(new[,] {{-1.1d, -2.2d, -3.3d}, {0d, 1.1d, new Complex(2.2d, -1.2d)}, {-4.4d, 5.5d, 6.6d}}),
                new DenseMatrix(new[,] {{new Complex(-1.1d, -2d), -2.2d, -3.3d, -4.4d}, {0d, 1.1d, 2.2d, 3.3d}, {1d, 2.1d, 6.2d, 4.3d}, {-4.4d, 5.5d, 6.6d, -7.7d}}),
                new DenseMatrix(new[,] {{-1.1d, new Complex(-2.2d, 3.4d), -3.3d, -4.4d}, {-1.1d, -2.2d, -3.3d, -4.4d}, {-1.1d, -2.2d, -3.3d, -4.4d}, {-1.1d, -2.2d, -3.3d, -4.4d}}),
                new DenseMatrix(new[,] {{-1.1d, -2.2d}, {Complex.Zero, 1.1d}, {-4.4d, 5.5d}}),
                new DenseMatrix(new[,] {{-1.1d, -2.2d, -3.3d}, {0d, new Complex(1.1d, 0.1d), 2.2d}}),
                new DenseMatrix(new[,] {{1d, 2d, 3d}, {2d, new Complex(2d, 2d), 0d}, {3d, Complex.Zero, 3d}}),

                new SparseMatrix(new[,] {{7d, 1d, 2d}, {1d, 1d, 2d}, {1d, 1d + Complex.ImaginaryOne, 2d}}),
                new SparseMatrix(new[,] {{7d, 1d, 2d}, {new Complex(1d, 2d), 0d, Complex.Zero}, {-2d, 0d, 0d}}),
                new SparseMatrix(new[,] {{-1.1d, 0d, 0d}, {0d, new Complex(1.1d, 2d), 2.2d}}),

                new DiagonalMatrix(3, 3, new[] {new Complex(1d, 1d), -2d, 1.5d}),
                new DiagonalMatrix(3, 3, new[] {new Complex(1d, 2d), 0d, -1.5d}),

                new UserDefinedMatrix(new[,] {{0d, 1d, 2d}, {-1d, 7.7d, 0d}, {-2d, Complex.Zero, 0d}})
            };

        [Datapoints]
        Complex[] scalars = new[] {new Complex(2d, 0d), new Complex(-1.5d, 3.5d), Complex.Zero};

        protected override Matrix<Complex> CreateDenseZero(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }

        protected override Matrix<Complex> CreateDenseRandom(int rows, int columns, int seed)
        {
            var dist = new Normal {RandomSource = new MersenneTwister(seed)};
            return new DenseMatrix(rows, columns, Enumerable.Range(0, rows*columns).Select(k => new Complex(dist.Sample(), dist.Sample())).ToArray());
        }

        protected override Matrix<Complex> CreateSparseZero(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }

        protected override Vector<Complex> CreateVectorZero(int size)
        {
            return new DenseVector(size);
        }

        protected override Vector<Complex> CreateVectorRandom(int size, int seed)
        {
            var dist = new Normal {RandomSource = new MersenneTwister(seed)};
            return new DenseVector(Enumerable.Range(0, size).Select(k => new Complex(dist.Sample(), dist.Sample())).ToArray());
        }

        protected override Complex Zero
        {
            get { return Complex.Zero; }
        }
    }
}