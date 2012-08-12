using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Generic;
    using NUnit.Framework;
    using Complex32 = Numerics.Complex32;

    [TestFixture]
    public class MatrixStructureTheory : MatrixStructureTheory<Complex32>
    {
        [Datapoints]
        Matrix<Complex32>[] _matrices = new Matrix<Complex32>[]
            {
                new DenseMatrix(new[,] {{1f, new Complex32(1.1f, -4f), 2f}, {1f, 1f, 2f}, {1f, new Complex32(1f, 2f), 2f}}),
                new DenseMatrix(new[,] {{-1.1f, -2.2f, -3.3f}, {0f, 1.1f, new Complex32(2.2f, -1.2f)}, {-4.4f, 5.5f, 6.6f}}),
                new DenseMatrix(new[,] {{new Complex32(-1.1f, -2f), -2.2f, -3.3f, -4.4f}, {0f, 1.1f, 2.2f, 3.3f}, {1f, 2.1f, 6.2f, 4.3f}, {-4.4f, 5.5f, 6.6f, -7.7f}}),
                new DenseMatrix(new[,] {{-1.1f, new Complex32(-2.2f, 3.4f), -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}}),
                new DenseMatrix(new[,] {{-1.1f, -2.2f}, {Complex32.Zero, 1.1f}, {-4.4f, 5.5f}}),
                new DenseMatrix(new[,] {{-1.1f, -2.2f, -3.3f}, {0f, new Complex32(1.1f, 0.1f), 2.2f}}),
                new DenseMatrix(new[,] {{1f, 2f, 3f}, {2f, new Complex32(2f, 2f), 0f}, {3f, Complex32.Zero, 3f}}),

                new SparseMatrix(new[,] {{7f, 1f, 2f}, {1f, 1f, 2f}, {1f, 1f + Complex32.ImaginaryOne, 2f}}),
                new SparseMatrix(new[,] {{7f, 1f, 2f}, {new Complex32(1f, 2f), 0f, Complex32.Zero}, {-2f, 0f, 0f}}),
                new SparseMatrix(new[,] {{-1.1f, 0f, 0f}, {0f, new Complex32(1.1f, 2f), 2.2f}}),

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

        protected override Complex32 Zero
        {
            get { return Complex32.Zero; }
        }
    }
}