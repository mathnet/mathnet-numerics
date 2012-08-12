using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single
{
    using LinearAlgebra.Single;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class MatrixStructureTheory : MatrixStructureTheory<float>
    {
        [Datapoints]
        Matrix<float>[] _matrices = new Matrix<float>[]
            {
                new DenseMatrix(new[,] {{1f, 1f, 2f}, {1f, 1f, 2f}, {1f, 1f, 2f}}),
                new DenseMatrix(new[,] {{-1.1f, -2.2f, -3.3f}, {0f, 1.1f, 2.2f}, {-4.4f, 5.5f, 6.6f}}),
                new DenseMatrix(new[,] {{-1.1f, -2.2f, -3.3f, -4.4f}, {0f, 1.1f, 2.2f, 3.3f}, {1f, 2.1f, 6.2f, 4.3f}, {-4.4f, 5.5f, 6.6f, -7.7f}}),
                new DenseMatrix(new[,] {{-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}, {-1.1f, -2.2f, -3.3f, -4.4f}}),
                new DenseMatrix(new[,] {{-1.1f, -2.2f}, {0f, 1.1f}, {-4.4f, 5.5f}}),
                new DenseMatrix(new[,] {{-1.1f, -2.2f, -3.3f}, {0f, 1.1f, 2.2f}}),
                new DenseMatrix(new[,] {{1f, 2f, 3f}, {2f, 2f, 0f}, {3f, 0f, 3f}}),

                new SparseMatrix(new[,] {{7f, 1f, 2f}, {1f, 1f, 2f}, {1f, 1f, 2f}}),
                new SparseMatrix(new[,] {{7f, 1f, 2f}, {1f, 0f, 0f}, {-2f, 0f, 0f}}),
                new SparseMatrix(new[,] {{-1.1f, 0f, 0f}, {0f, 1.1f, 2.2f}}),

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

        protected override float Zero
        {
            get { return 0f; }
        }
    }
}
