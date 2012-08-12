using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using LinearAlgebra.Double;
    using LinearAlgebra.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class MatrixStructureTheory : MatrixStructureTheory<double>
    {
        [Datapoints]
        Matrix<double>[] _matrices = new Matrix<double>[]
            {
                new DenseMatrix(new[,] {{1d, 1d, 2d}, {1d, 1d, 2d}, {1d, 1d, 2d}}),
                new DenseMatrix(new[,] {{-1.1d, -2.2d, -3.3d}, {0d, 1.1d, 2.2d}, {-4.4d, 5.5d, 6.6d}}),
                new DenseMatrix(new[,] {{-1.1d, -2.2d, -3.3d, -4.4d}, {0d, 1.1d, 2.2d, 3.3d}, {1d, 2.1d, 6.2d, 4.3d}, {-4.4d, 5.5d, 6.6d, -7.7d}}),
                new DenseMatrix(new[,] {{-1.1d, -2.2d, -3.3d, -4.4d}, {-1.1d, -2.2d, -3.3d, -4.4d}, {-1.1d, -2.2d, -3.3d, -4.4d}, {-1.1d, -2.2d, -3.3d, -4.4d}}),
                new DenseMatrix(new[,] {{-1.1d, -2.2d}, {0d, 1.1d}, {-4.4d, 5.5d}}),
                new DenseMatrix(new[,] {{-1.1d, -2.2d, -3.3d}, {0d, 1.1d, 2.2d}}),
                new DenseMatrix(new[,] {{1d, 2d, 3d}, {2d, 2d, 0d}, {3d, 0d, 3d}}),

                new SparseMatrix(new[,] {{7d, 1d, 2d}, {1d, 1d, 2d}, {1d, 1d, 2d}}),
                new SparseMatrix(new[,] {{7d, 1d, 2d}, {1d, 0d, 0d}, {-2d, 0d, 0d}}),
                new SparseMatrix(new[,] {{-1.1d, 0d, 0d}, {0d, 1.1d, 2.2d}}),

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

        protected override double Zero
        {
            get { return 0d; }
        }
    }
}