namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32.Solvers.Preconditioners
{
    using Numerics;
    using LinearAlgebra.Complex32;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Preconditioners;
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

        protected Vector<Complex32> CreateStandardBcVector(int size)
        {
            Vector<Complex32> vector = new DenseVector(size);
            for (var i = 0; i < size; i++)
            {
                vector[i] = i + 1;
            }

            return vector;
        }

        internal abstract IPreConditioner<Complex32> CreatePreconditioner();

        protected abstract void CheckResult(IPreConditioner<Complex32> preconditioner, SparseMatrix matrix, Vector<Complex32> vector, Vector<Complex32> result);

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

            Vector<Complex32> result = new DenseVector(vector.Count);
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

            Vector<Complex32> result = new DenseVector(vector.Count + 10);
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

            Vector<Complex32> result = new DenseVector(vector.Count + 10);
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

            Vector<Complex32> result = null;
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