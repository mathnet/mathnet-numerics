namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex.Solvers.Preconditioners
{
    using System;
    using System.Numerics;
    using System.Reflection;
    using LinearAlgebra.Complex;
    using LinearAlgebra.Complex.Solvers.Preconditioners;
    using LinearAlgebra.Generic;
    using LinearAlgebra.Generic.Solvers.Preconditioners;
    using MbUnit.Framework;

    [TestFixture]
    public sealed class IlutpPreconditionerTest : PreconditionerTest
    {
        private double _dropTolerance = 0.1;
        private double _fillLevel = 1.0;
        private double _pivotTolerance = 1.0;

        [SetUp]
        public void Setup()
        {
            _dropTolerance = 0.1;
            _fillLevel = 1.0;
            _pivotTolerance = 1.0;
        }

        private static T GetMethod<T>(Ilutp ilutp, string methodName)
        {
            var type = ilutp.GetType();
            var methodInfo = type.GetMethod(methodName,
                                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
                                            null,
                                            CallingConventions.Standard,
                                            new Type[0],
                                            null);
            var obj = methodInfo.Invoke(ilutp, null);
            return (T)obj;
        }

        private static SparseMatrix GetUpperTriangle(Ilutp ilutp)
        {
            return GetMethod<SparseMatrix>(ilutp, "UpperTriangle");
        }

        private static SparseMatrix GetLowerTriangle(Ilutp ilutp)
        {
            return GetMethod<SparseMatrix>(ilutp, "LowerTriangle");
        }

        private static int[] GetPivots(Ilutp ilutp)
        {
            return GetMethod<int[]>(ilutp, "Pivots");
        }

        private static SparseMatrix CreateReverseUnitMatrix(int size)
        {
            var matrix = new SparseMatrix(size);
            for (var i = 0; i < size; i++)
            {
                matrix[i, size - 1 - i] = 2;
            }

            return matrix;
        }

        private Ilutp InternalCreatePreconditioner()
        {
            var result = new Ilutp
                         {
                             DropTolerance = _dropTolerance,
                             FillLevel = _fillLevel,
                             PivotTolerance = _pivotTolerance
                         };
            return result;
        }

        internal override IPreConditioner<Complex> CreatePreconditioner()
        {
            _pivotTolerance = 0;
            _dropTolerance = 0.0;
            _fillLevel = 100;
            return InternalCreatePreconditioner();
        }

        protected override void CheckResult(IPreConditioner<Complex> preconditioner, SparseMatrix matrix, Vector<Complex> vector, Vector<Complex> result)
        {
            Assert.AreEqual(typeof(Ilutp), preconditioner.GetType(), "#01");

            // Compute M * result = product
            // compare vector and product. Should be equal
            Vector<Complex> product = new DenseVector(result.Count);
            matrix.Multiply(result, product);
            for (var i = 0; i < product.Count; i++)
            {
                Assert.IsTrue(vector[i].Real.AlmostEqual(product[i].Real, -Epsilon.Magnitude()), "#02-" + i);
                Assert.IsTrue(vector[i].Imaginary.AlmostEqual(product[i].Imaginary, -Epsilon.Magnitude()), "#03-" + i);
            }
        }

        [Test]
        [MultipleAsserts]
        public void SolveReturningOldVectorWithoutPivoting()
        {
            const int Size = 10;
            
            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);
            
            // set the pivot tolerance to zero so we don't pivot
            _pivotTolerance = 0.0;
            _dropTolerance = 0.0;
            _fillLevel = 100;
            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);
            Vector<Complex> result = new DenseVector(vector.Count);
            preconditioner.Approximate(vector, result);
            CheckResult(preconditioner, newMatrix, vector, result);
        }

        [Test]
        [MultipleAsserts]
        public void SolveReturningOldVectorWithPivoting()
        {
            const int Size = 10;
            var newMatrix = CreateUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);
            
            // Set the pivot tolerance to 1 so we always pivot (if necessary)
            _pivotTolerance = 1.0;
            _dropTolerance = 0.0;
            _fillLevel = 100;
            var preconditioner = CreatePreconditioner();
            preconditioner.Initialize(newMatrix);
            Vector<Complex> result = new DenseVector(vector.Count);
            preconditioner.Approximate(vector, result);
            CheckResult(preconditioner, newMatrix, vector, result);
        }

        [Test]
        [MultipleAsserts]
        public void CompareWithOriginalDenseMatrixWithoutPivoting()
        {
            var sparseMatrix = new SparseMatrix(3);
            sparseMatrix[0, 0] = -1;
            sparseMatrix[0, 1] = 5;
            sparseMatrix[0, 2] = 6;
            sparseMatrix[1, 0] = 3;
            sparseMatrix[1, 1] = -6;
            sparseMatrix[1, 2] = 1;
            sparseMatrix[2, 0] = 6;
            sparseMatrix[2, 1] = 8;
            sparseMatrix[2, 2] = 9;
            var ilu = new Ilutp
                      {
                          PivotTolerance = 0.0,
                          DropTolerance = 0,
                          FillLevel = 10
                      };
            ilu.Initialize(sparseMatrix);
            var l = GetLowerTriangle(ilu);
            
            // Assert l is lower triagonal
            for (var i = 0; i < l.RowCount; i++)
            {
                for (var j = i + 1; j < l.RowCount; j++)
                {
                    Assert.IsTrue(0.0.AlmostEqual(l[i,j].Magnitude, -Epsilon.Magnitude()), "#01-" + i + "-" + j);
                }
            }

            var u = GetUpperTriangle(ilu);
            
            // Assert u is upper triagonal
            for (var i = 0; i < u.RowCount; i++)
            {
                for (var j = 0; j < i; j++)
                {
                    Assert.IsTrue(0.0.AlmostEqual(u[i,j].Magnitude, -Epsilon.Magnitude()), "#02-" + i + "-" + j);
                }
            }

            var original = l.Multiply(u);
            for (var i = 0; i < sparseMatrix.RowCount; i++)
            {
                for (var j = 0; j < sparseMatrix.ColumnCount; j++)
                {
                    Assert.IsTrue(sparseMatrix[i,j].Real.AlmostEqual(original[i, j].Real, -Epsilon.Magnitude()), "#03-" + i + "-" + j);
                    Assert.IsTrue(sparseMatrix[i, j].Imaginary.AlmostEqual(original[i, j].Imaginary, -Epsilon.Magnitude()), "#04-" + i + "-" + j);
                }
            }
        }

        [Test]
        [MultipleAsserts]
        public void CompareWithOriginalDenseMatrixWithPivoting()
        {
            var sparseMatrix = new SparseMatrix(3);
            sparseMatrix[0, 0] = -1;
            sparseMatrix[0, 1] = 5;
            sparseMatrix[0, 2] = 6;
            sparseMatrix[1, 0] = 3;
            sparseMatrix[1, 1] = -6;
            sparseMatrix[1, 2] = 1;
            sparseMatrix[2, 0] = 6;
            sparseMatrix[2, 1] = 8;
            sparseMatrix[2, 2] = 9;
            var ilu = new Ilutp
                      {
                          PivotTolerance = 1.0,
                          DropTolerance = 0,
                          FillLevel = 10
                      };
            ilu.Initialize(sparseMatrix);
            var l = GetLowerTriangle(ilu);
            var u = GetUpperTriangle(ilu);
            var pivots = GetPivots(ilu);
            var p = new SparseMatrix(l.RowCount);
            for (var i = 0; i < p.RowCount; i++)
            {
                p[i, pivots[i]] = 1.0;
            }

            var temp = l.Multiply(u);
            var original = temp.Multiply(p);
            for (var i = 0; i < sparseMatrix.RowCount; i++)
            {
                for (var j = 0; j < sparseMatrix.ColumnCount; j++)
                {
                    Assert.IsTrue(sparseMatrix[i, j].Real.AlmostEqual(original[i, j].Real, -Epsilon.Magnitude()), "#01-" + i + "-" + j);
                    Assert.IsTrue(sparseMatrix[i, j].Imaginary.AlmostEqual(original[i, j].Imaginary, -Epsilon.Magnitude()), "#02-" + i + "-" + j);
                }
            }
        }

        [Test]
        [MultipleAsserts]
        public void SolveWithPivoting()
        {
            const int Size = 10;
            var newMatrix = CreateReverseUnitMatrix(Size);
            var vector = CreateStandardBcVector(Size);
            var preconditioner = new Ilutp
                                 {
                                     PivotTolerance = 1.0,
                                     DropTolerance = 0,
                                     FillLevel = 10
                                 };
            preconditioner.Initialize(newMatrix);
            Vector<Complex> result = new DenseVector(vector.Count);
            preconditioner.Approximate(vector, result);
            CheckResult(preconditioner, newMatrix, vector, result);
        }
    }
}
