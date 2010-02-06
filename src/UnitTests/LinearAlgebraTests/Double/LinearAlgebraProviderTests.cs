namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using Algorithms.LinearAlgebra;
    using MbUnit.Framework;

    [TestFixture]
    public abstract class LinearAlgebraProviderTests
    {
        protected ILinearAlgebraProvider<double> Provider{ get; set;}

        private double[] y = new [] { 1.1, 2.2, 3.3, 4.4, 5.5 };
        private double[] x = new[] { 6.6, 7.7, 8.8, 9.9, 10.1 };

        [Test, MultipleAsserts]
        public void CanAddVectorToScaledVector()
        {
            var result = new double[y.Length];
            Array.Copy(y, result, y.Length);

            Provider.AddVectorToScaledVector(result, 0, x);
            for (var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i], result[i]);
            }

            Array.Copy(y, result, y.Length);
            Provider.AddVectorToScaledVector(result, 1, x);
            for (var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i] + x[i], result[i]);
            }

            Array.Copy(y, result, y.Length);
            Provider.AddVectorToScaledVector(result, Math.PI, x);
            for( var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i] + Math.PI * x[i], result[i]);
            }
        }

        [Test, MultipleAsserts]
        public void CanScaleArray()
        {
            var result = new double[y.Length];

            Array.Copy(y, result, y.Length);
            Provider.ScaleArray(1, result);
            for (var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i], result[i]);
            }

            Array.Copy(y, result, y.Length);
            Provider.ScaleArray(Math.PI, result);
            for (var i = 0; i < y.Length; i++)
            {
                Assert.AreEqual(y[i] * Math.PI, result[i]);
            }
        }

        [Test]
        public void CanComputeDotProduct()
        {
            var result = Provider.DotProduct(x, y);
            Console.WriteLine(result);
            AssertHelpers.AlmostEqual(152.35, result, 15);

        }

        [Test]
        public void CanAddArrays()
        {
            var result = new double[y.Length];
            Provider.AddArrays(x, y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(x[i] + y[i], result[i]);
            }
        }

        [Test]
        public void CanSubtractArrays()
        {
            var result = new double[y.Length];
            Provider.SubtractArrays(x, y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(x[i] - y[i], result[i]);
            }
        }
        
        [Test]
        public void CanPointWiseMultiplyArrays()
        {
            var result = new double[y.Length];
            Provider.PointWiseMultiplyArrays(x, y, result);
            for (var i = 0; i < result.Length; i++)
            {
                Assert.AreEqual(x[i] * y[i], result[i]);
            }
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