namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Single.Solvers.Preconditioners
{
    using LinearAlgebra.Single;
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

        protected Vector<float> CreateStandardBcVector(int size)
        {
            Vector<float> vector = new DenseVector(size);
            for (var i = 0; i < size; i++)
            {
                vector[i] = i + 1;
            }

            return vector;
        }

        internal abstract IPreConditioner<float> CreatePreconditioner();

        protected abstract void CheckResult(IPreConditioner<float> preconditioner, SparseMatrix matrix, Vector<float> vector, Vector<float> result);

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

            Vector<float> result = new DenseVector(vector.Count);
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

            Vector<float> result = new DenseVector(vector.Count + 10);
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

            Vector<float> result = new DenseVector(vector.Count + 10);
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

            Vector<float> result = null;
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