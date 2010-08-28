namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double.Solvers.Preconditioners
{
    using LinearAlgebra.Double;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Preconditioners;
    using MbUnit.Framework;

    public abstract class PreconditionerTest
    {
        protected const double Epsilon = 1e-10;

        internal SparseMatrix CreateUnitMatrix(int size)
        {
            var matrix = new SparseMatrix(size);
            for (var i = 0; i < size; i++)
            {
                matrix[i, i] = 2;
            }

            return matrix;
        }

        protected Vector<double> CreateStandardBcVector(int size)
        {
            Vector<double> vector = new DenseVector(size);
            for (var i = 0; i < size; i++)
            {
                vector[i] = i + 1;
            }

            return vector;
        }

        internal abstract IPreConditioner<double> CreatePreconditioner();

        protected abstract void CheckResult(IPreConditioner<double> preconditioner, SparseMatrix matrix, Vector<double> vector, Vector<double> result);

        [Test]
        [MultipleAsserts]
        public void ApproximateWithUnitMatrixReturningNewVector()
        {
            const int Size = 10;

            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);

            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);

            var result = preconditioner.Approximate(vector);

            CheckResult(preconditioner, newMatrix, vector, result);
        }

        [Test]
        [MultipleAsserts]
        public void ApproximateReturningOldVector()
        {
            const int Size = 10;
            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);

            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);

            Vector<double> result = new DenseVector(vector.Count);
            preconditioner.Approximate(vector, result);

            CheckResult(preconditioner, newMatrix, vector, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void ApproximateWithVectorWithIncorrectLength()
        {
            const int Size = 10;
            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);

            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);

            Vector<double> result = new DenseVector(vector.Count + 10);
            preconditioner.Approximate(vector, result);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void ApproximateWithNullVector()
        {
            const int Size = 10;
            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);

            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);

            Vector<double> result = new DenseVector(vector.Count + 10);
            preconditioner.Approximate(null, result);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void ApproximateWithNullResultVector()
        {
            const int Size = 10;
            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);

            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);

            Vector<double> result = null;
            preconditioner.Approximate(vector, result);
        }

        [Test]
        [ExpectedArgumentException]
        public void ApproximateWithNonInitializedPreconditioner()
        {
            const int Size = 10;
            var vector = CreateStandardBcVector(Size);
            var preconditioner = CreatePreconditioner();
            preconditioner.Approximate(vector);
        }
    }
}