namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using LinearAlgebra.Double;

    internal class UserDefinedMatrix : Matrix
    {
        private readonly double[,] _data;

        public UserDefinedMatrix(int rows, int columns) : base(rows, columns)
        {
            _data = new double[rows, columns];
        }

        public UserDefinedMatrix(double[,] data) : base(data.GetLength(0), data.GetLength(1))
        {
            _data = data;
        }

        public override double At(int row, int column)
        {
            return _data[row, column];
        }

        public override void At(int row, int column, double value)
        {
            _data[row, column] = value;
        }

        public override Matrix CreateMatrix(int numberOfRows, int numberOfColumns)
        {
            return new UserDefinedMatrix(numberOfRows, numberOfColumns);
        }

        public override Vector CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }
    }

    public class UserDefinedMatrixTests : MatrixTests
    {
        protected override Matrix CreateMatrix(int rows, int columns)
        {
            return new UserDefinedMatrix(rows, columns);
        }

        protected override Matrix CreateMatrix(double[,] data)
        {
            return new UserDefinedMatrix(data);
        }

        protected override Vector CreateVector(int size)
        {
            return new UserDefinedVector(size);
        }

        protected override Vector CreateVector(double[] data)
        {
            return new UserDefinedVector(data);
        }
    }
}
