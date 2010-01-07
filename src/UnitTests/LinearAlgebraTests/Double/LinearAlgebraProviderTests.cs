namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using Algorithms.LinearAlgebra;
    using MbUnit.Framework;

    [TestFixture]
    public abstract class LinearAlgebraProviderTests
    {
        protected ILinearAlgebraProvider<double> Provider{ get; set;}

        [Test, Ignore]
        public void CanAddVectorToScaledVector(double[] y, double alpha, double[] x)
        {
            
        }

        [Test, Ignore]
        public void CanScaleArray(double alpha, double[] x)
        {

        }

        [Test, Ignore]
        public void CanComputeDotProduct(double[] x, double[] y)
        {

        }

        [Test, Ignore]
        public void CanAddArrays(double[] x, double[] y, double[] result)
        {

        }

        [Test, Ignore]
        public void CanSubtractArrays(double[] x, double[] y, double[] result)
        {

        }
        
        [Test, Ignore]
        public void CanPointWiseMultiplyArrays(double[] x, double[] y, double[] result)
        {

        }

        [Test, Ignore]
        public void CanComputeMatrixNorm(Norm norm, double[] matrix){}

        [Test, Ignore]
        public void CanComputeMatrixNorm(Norm norm, double[] matrix, double[] work)
        {

        }

        [Test, Ignore]
        public void CanMatrixMultiply(double[] x, int xRows, int xColumns, double[] y, int yRows, int yColumns, double[] result)
        {

        }

        [Test, Ignore]
        public void CanMatrixMultiplyWithUpdate(Transpose transposeA, Transpose transposeB, double alpha, double[] a,
            int aRows, int aColumns, double[] b, int bRows, int bColumns, double beta, double[] c)
        {

        }

        [Test, Ignore]
        public void CanComputeLUFactor(double[] a, int[] ipiv)
        {

        }

        [Test, Ignore]
        public void CanComputeLUInverse(double[] a)
        {

        }

        public void CanComputeLUInverseFactored(double[] a, int[] ipiv)
        {

        }

        [Test, Ignore]
        public void CanComputeLUInverse(double[] a, double[] work)
        {

        }
        
        [Test, Ignore]
        public void CanComputeLUInverseFactored(double[] a, int[] ipiv, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeLUSolve(int columnsOfB, double[] a, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeLUSolveFactored(int columnsOfB, double[] a, int ipiv, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeLUSolve(Transpose transposeA, int columnsOfB, double[] a, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeLUSolveFactored(Transpose transposeA, int columnsOfB, double[] a, int ipiv, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeCholeskyFactor(double[] a, int order)
        {

        }

        [Test, Ignore]
        public void CanComputeCholeskySolve(int columnsOfB, double[] a, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeCholeskySolveFactored(int columnsOfB, double[] a, double[] b)
        {

        }

        [Test, Ignore]
        public void CanComputeQRFactor(double[] r, double[] q)
        {

        }

        [Test, Ignore]
        public void CanComputeQRFactor(double[] r, double[] q, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeQRSolve(int columnsOfB, double[] r, double[] q, double[] b, double[] x)
        {

        }

        [Test, Ignore]
        public void CanComputeQRSolve(int columnsOfB, double[] r, double[] q, double[] b, double[] x, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeQRSolveFactored(int columnsOfB, double[] q, double[] r, double[] b, double[] x)
        {

        }

        [Test, Ignore]
        public void CanComputeSinguarValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt)
        {

        }

        [Test, Ignore]
        public void CanComputeSingularValueDecomposition(bool computeVectors, double[] a, double[] s, double[] u, double[] vt, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeSvdSolve(double[] a, double[] s, double[] u, double[] vt, double[] b, double[] x)
        {

        }

        [Test, Ignore]
        public void CanComputeSvdSolve(double[] a, double[] s, double[] u, double[] vt, double[] b, double[] x, double[] work)
        {

        }

        [Test, Ignore]
        public void CanComputeSvdSolveFactored(int columnsOfB, double[] s, double[] u, double[] vt, double[] b, double[] x)
        {

        }
    }
}