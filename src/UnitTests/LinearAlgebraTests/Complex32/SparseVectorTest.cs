// <copyright file="SparseVectorTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex32
{
    using System;
    using System.Collections.Generic;
    using Numerics;
    using LinearAlgebra.Generic;
    using MbUnit.Framework;
    using LinearAlgebra.Complex32;

    public class SparseVectorTest : VectorTests
    {
        protected override Vector<Complex32> CreateVector(int size)
        {
            return new SparseVector(size);
        }

        protected override Vector<Complex32> CreateVector(IList<Complex32> data)
        {
            var vector = new SparseVector(data.Count);
            for (var index = 0; index < data.Count; index++)
            {
                vector[index] = data[index];
            }
            return vector;
        }

        #region Test similar to DenseVector
        [Test]
        [MultipleAsserts]
        public void CanCreateSparseVectorFromArray()
        {
            var data = new Complex32[Data.Length];
            Array.Copy(Data, data, Data.Length);
            var vector = new SparseVector(data);

            for (var i = 0; i < data.Length; i++)
            {
                AssertHelpers.AreEqual(data[i], vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateSparseVectorFromAnotherSparseVector()
        {
            var vector = new SparseVector(Data);
            var other = new SparseVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateSparseVectorFromAnotherVector()
        {
            var vector = (Vector<Complex32>)new SparseVector(Data);
            var other = new SparseVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateSparseVectorFromUserDefinedVector()
        {
            var vector = new UserDefinedVector(Data);
            var other = new SparseVector(vector);

            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        public void CanCreateSparseVectorWithConstantValues()
        {
            var vector = new SparseVector(5, new Complex32(5, 1));
            Assert.ForAll(vector, value => value == new Complex32(5,1));
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateSparseMatrix()
        {
            var vector = new SparseVector(3);
            var matrix = vector.CreateMatrix(2, 3);
            Assert.AreEqual(2, matrix.RowCount);
            Assert.AreEqual(3, matrix.ColumnCount);
        }


        [Test]
        [MultipleAsserts]
        public void CanConvertSparseVectorToArray()
        {
            var vector = new SparseVector(Data);
            var array = vector.ToArray();
            Assert.IsInstanceOfType(typeof(Complex32[]), array);
            Assert.AreElementsEqual(vector, array);
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertArrayToSparseVector()
        {
            var array = new[] { Complex32.Zero, new Complex32(1.0f, 1), new Complex32(2.0f, 1), new Complex32(3.0f, 1), new Complex32(4.0f, 1) };
            var vector = new SparseVector(array);
            Assert.IsInstanceOfType(typeof(SparseVector), vector);
            Assert.AreElementsEqual(array, array);
        }

        [Test]
        public void CanCallUnaryPlusOperatorOnSparseVector()
        {
            var vector = new SparseVector(Data);
            var other = +vector;
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoSparseVectorsUsingOperator()
        {
            var vector = new SparseVector(Data);
            var other = new SparseVector(Data);
            var result = vector + other;

            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(Data[i], vector[i]);
                AssertHelpers.AreEqual(Data[i], other[i]);
                AssertHelpers.AreEqual(Data[i] * 2.0f, result[i]);
            }
        }

        [Test]
        public void CanCallUnaryNegationOperatorOnSparseVector()
        {
            var vector = new SparseVector(Data);
            var other = -vector;
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(-Data[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoSparseVectorsUsingOperator()
        {
            var vector = new SparseVector(Data);
            var other = new SparseVector(Data);
            var result = vector - other;

            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(Data[i], vector[i]);
                AssertHelpers.AreEqual(Data[i], other[i]);
                AssertHelpers.AreEqual(Complex32.Zero, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplySparseVectorByComplexUsingOperators()
        {
            var vector = new SparseVector(Data);
            vector = vector * new Complex32(2.0f, 1);

            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(Data[i] * new Complex32(2.0f, 1), vector[i]);
            }

            vector = vector * 1.0f;
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(Data[i] * new Complex32(2.0f, 1), vector[i]);
            }

            vector = new SparseVector(Data);
            vector = new Complex32(2.0f, 1) * vector;

            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(Data[i] * new Complex32(2.0f, 1), vector[i]);
            }

            vector = 1.0f * vector;
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(Data[i] * new Complex32(2.0f, 1), vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideSparseVectorByComplexUsingOperators()
        {
            var vector = new SparseVector(Data);
            vector = vector / new Complex32(2.0f, 1);

            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(Data[i] / new Complex32(2.0f, 1), vector[i]);
            }

            vector = vector / 1.0f;
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(Data[i] / new Complex32(2.0f, 1), vector[i]);
            }
        }

        [Test]
        public void CanCalculateOuterProductForSparseVector()
        {
            var vector1 = CreateVector(Data);
            var vector2 = CreateVector(Data);
            Matrix<Complex32> m = Vector<Complex32>.OuterProduct(vector1, vector2);
            for (var i = 0; i < vector1.Count; i++)
            {
                for (var j = 0; j < vector2.Count; j++)
                {
                    AssertHelpers.AreEqual(m[i, j], vector1[i] * vector2[j]);
                }
            }
        }

        [Test]
        [ExpectedArgumentNullException]
        public void OuterProducForSparseVectortWithFirstParameterNullShouldThrowException()
        {
            SparseVector vector1 = null;
            var vector2 = CreateVector(Data);
            Vector<Complex32>.OuterProduct(vector1, vector2);
        }

        [Test]
        [ExpectedArgumentNullException]
        public void OuterProductForSparseVectorWithSecondParameterNullShouldThrowException()
        {
            var vector1 = CreateVector(Data);
            SparseVector vector2 = null;
            Vector<Complex32>.OuterProduct(vector1, vector2);
        }
        #endregion

        [Test]
        [MultipleAsserts]
        public void CanCreateSparseVectorFromDenseVector()
        {
            var vector = (Vector<Complex32>)new DenseVector(Data);
            var other = new SparseVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < Data.Length; i++)
            {
                AssertHelpers.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CheckSparseMechanismBySettingValues()
        {
            var vector = new SparseVector(10000);
            
            //Add non-zero elements
            vector[200] = 1.5f;
            AssertHelpers.AreEqual(1.5f, vector[200]);
            Assert.AreEqual(1, vector.NonZerosCount);

            vector[500] = 3.5f;
            AssertHelpers.AreEqual(3.5f, vector[500]);
            Assert.AreEqual(2, vector.NonZerosCount);

            vector[800] = 5.5f;
            AssertHelpers.AreEqual(5.5f, vector[800]);
            Assert.AreEqual(3, vector.NonZerosCount);

            vector[0] = 7.5f;
            AssertHelpers.AreEqual(7.5f, vector[0]);
            Assert.AreEqual(4, vector.NonZerosCount);

            //Remove non-zero elements
            vector[200] = 0;
            AssertHelpers.AreEqual(0, vector[200]);
            Assert.AreEqual(3, vector.NonZerosCount);

            vector[500] = 0;
            AssertHelpers.AreEqual(0, vector[500]);
            Assert.AreEqual(2, vector.NonZerosCount);

            vector[800] = 0;
            AssertHelpers.AreEqual(0, vector[800]);
            Assert.AreEqual(1, vector.NonZerosCount);

            vector[0] = 0;
            AssertHelpers.AreEqual(0, vector[0]);
            Assert.AreEqual(0, vector.NonZerosCount);
        }

        [Test]
        [MultipleAsserts]
        public void CheckSparseMechanismByZeroMultiply()
        {
            var vector = new SparseVector(10000);

            //Add non-zero elements
            vector[200] = 1.5f;
            vector[500] = 3.5f;
            vector[800] = 5.5f;
            vector[0] = 7.5f;
            
            //Multiply by 0
            vector *= 0; 
            AssertHelpers.AreEqual(0, vector[200]);
            AssertHelpers.AreEqual(0, vector[500]);
            AssertHelpers.AreEqual(0, vector[800]);
            AssertHelpers.AreEqual(0, vector[0]);
            Assert.AreEqual(0, vector.NonZerosCount);
        }


        [Test]
        public void CanDotProductOfTwoSparseVectors()
        {
            var vectorA = new SparseVector(10000);
            vectorA[200] = 1;
            vectorA[500] = 3;
            vectorA[800] = 5;
            vectorA[100] = 7;
            vectorA[900] = 9;

            var vectorB = new SparseVector(10000);
            vectorB[300] = 3;
            vectorB[500] = 5;
            vectorB[800] = 7;


            AssertHelpers.AreEqual(50.0f, vectorA.DotProduct(vectorB));
        }

        [Test]
        public void CreateHugeSparseVector()
        {
            var data = new Complex32[1000000];
            var rnd = new Random();
            for (var i = 0; i < 1000000; i++)
                data[i] = new Complex32(rnd.Next(), rnd.Next());

            var vector = new SparseVector(data);
        }

        [Test]
        public void PointwiseMultiplySparseVector()
        {
            var zeroArray = new[] { Complex32.Zero, new Complex32(1.0f, 1), Complex32.Zero, new Complex32(1.0f, 1), Complex32.Zero };
            var vector1 = new SparseVector(Data);
            var vector2 = new SparseVector(zeroArray);
            var result = new SparseVector(vector1.Count);

            vector1.PointwiseMultiply(vector2, result);

            for (var i = 0; i < vector1.Count; i++)
            {
                AssertHelpers.AreEqual(Data[i] * zeroArray[i], result[i]);
            }
            Assert.AreEqual(2, result.NonZerosCount);
        }
    }
}
