namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.Preconditioners
{
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Complex32.Solvers.Preconditioners;
    using MbUnit.Framework;

    public abstract class PreconditionerTest
    {
        protected const double Epsilon = 1e-5;

        internal SparseMatrix CreateUnitMatrix(int size)
        {
            var matrix = new SparseMatrix(size);
            for (var i = 0; i < size; i++)
            {
                matrix[i, i] = 2;
            }

            return matrix;
        }

        protected Vector CreateStandardBcVector(int size)
        {
            Vector vector = new DenseVector(size);
            for (var i = 0; i < size; i++)
            {
                vector[i] = i + 1;
            }

            return vector;
        }

        internal abstract IPreConditioner CreatePreconditioner();

        protected abstract void CheckResult(IPreConditioner preconditioner, SparseMatrix matrix, Vector vector, Vector result);

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

            Vector result = new DenseVector(vector.Count);
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

            Vector result = new DenseVector(vector.Count + 10);
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

            Vector result = new DenseVector(vector.Count + 10);
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

            Vector result = null;
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