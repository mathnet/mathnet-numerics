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

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Double
{
    using System;
    using System.Collections.Generic;
    using MbUnit.Framework;
    using LinearAlgebra.Double;

    public class SparseVectorTest : VectorTests
    {
        protected override Vector CreateVector(int size)
        {
            return new SparseVector(size);
        }

        protected override Vector CreateVector(IList<double> data)
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
            var data = new double[_data.Length];
            System.Array.Copy(_data, data, _data.Length);
            var vector = new SparseVector(data);

            //Assert.AreSame(data, vector.ToArray()); There is no way to cast SparseVector to double[], so "vector.ToArray()" and "array" have different references
            for (var i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], vector[i]);
            }

            // vector and data are different instances actually
            //vector[0] = 100.0;
            //Assert.AreEqual(100.0, data[0]);
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateSparseVectorFromAnotherSparseVector()
        {
            var vector = new SparseVector(_data);
            var other = new SparseVector(vector);


            Assert.AreNotSame(vector, other);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateSparseVectorFromAnotherVector()
        {
            var vector = (Vector)new SparseVector(_data);
            var other = new SparseVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanCreateSparseVectorFromUserDefinedVector()
        {
            var vector = new UserDefinedVector(_data);
            var other = new SparseVector(vector);

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        public void CanCreateSparseVectorWithConstantValues()
        {
            var vector = new SparseVector(5, 5);
            Assert.ForAll(vector, value => value == 5);
        }

        // TODO: Remove [Ignore] when SparseMatrix developed
        [Test]
        [MultipleAsserts]
        [Ignore]
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
            var vector = new SparseVector(_data);
            var array = vector.ToArray();
            Assert.IsInstanceOfType(typeof(double[]), array);
            //Assert.AreSame(vector.ToArray(), array); There is no way to cast SparseVector to double[], so "vector.ToArray()" and "array" have different references
            Assert.AreElementsEqual(vector, array);
        }

        [Test]
        [MultipleAsserts]
        public void CanConvertArrayToSparseVector()
        {
            var array = new[] { 0.0, 1.0, 2.0, 3.0, 4.0 };
            var vector = new SparseVector(array);
            Assert.IsInstanceOfType(typeof(SparseVector), vector);
            Assert.AreElementsEqual(array, array);
        }

        [Test]
        public void CanCallUnaryPlusOperatorOnSparseVector()
        {
            var vector = new SparseVector(_data);
            var other = +vector;
            Assert.AreSame(vector, other, "Should be the same vector");
        }

        [Test]
        [MultipleAsserts]
        public void CanAddTwoSparseVectorsUsingOperator()
        {
            var vector = new SparseVector(_data);
            var other = new SparseVector(_data);
            var result = vector + other;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i] * 2.0, result[i]);
            }
        }

        [Test]
        public void CanCallUnaryNegationOperatorOnDenseVector()
        {
            var vector = new SparseVector(_data);
            var other = -vector;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(-_data[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanSubtractTwoSparseVectorsUsingOperator()
        {
            var vector = new SparseVector(_data);
            var other = new SparseVector(_data);
            var result = vector - other;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i], vector[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(_data[i], other[i], "Making sure the original vector wasn't modified.");
                Assert.AreEqual(0.0, result[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanMultiplySparseVectorByScalarUsingOperators()
        {
            var vector = new SparseVector(_data);
            vector = vector * 2.0;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector = vector * 1.0;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector = new SparseVector(_data);
            vector = 2.0 * vector;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }

            vector = 1.0 * vector;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] * 2.0, vector[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CanDivideSparseVectorByScalarUsingOperators()
        {
            var vector = new SparseVector(_data);
            vector = vector / 2.0;

            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] / 2.0, vector[i]);
            }

            vector = vector / 1.0;
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(_data[i] / 2.0, vector[i]);
            }
        }
        #endregion

        [Test]
        [MultipleAsserts]
        public void CanCreateSparseVectorFromDenseVector()
        {
            var vector = (Vector)new DenseVector(_data);
            var other = new SparseVector(vector);

            Assert.AreNotSame(vector, other);
            for (var i = 0; i < _data.Length; i++)
            {
                Assert.AreEqual(vector[i], other[i]);
            }
        }

        [Test]
        [MultipleAsserts]
        public void CheckSparseMechanismBySettingValues()
        {
            var vector = new SparseVector(10000);
            
            //Add non-zero elements
            vector[200] = 1.5;
            Assert.AreEqual(1.5, vector[200]);
            Assert.AreEqual(1, vector.NonZerosCount);

            vector[500] = 3.5;
            Assert.AreEqual(3.5, vector[500]);
            Assert.AreEqual(2, vector.NonZerosCount);

            vector[800] = 5.5;
            Assert.AreEqual(5.5, vector[800]);
            Assert.AreEqual(3, vector.NonZerosCount);

            vector[0] = 7.5;
            Assert.AreEqual(7.5, vector[0]);
            Assert.AreEqual(4, vector.NonZerosCount);

            //Remove non-zero elements
            vector[200] = 0;
            Assert.AreEqual(0, vector[200]);
            Assert.AreEqual(3, vector.NonZerosCount);

            vector[500] = 0;
            Assert.AreEqual(0, vector[500]);
            Assert.AreEqual(2, vector.NonZerosCount);

            vector[800] = 0;
            Assert.AreEqual(0, vector[800]);
            Assert.AreEqual(1, vector.NonZerosCount);

            vector[0] = 0;
            Assert.AreEqual(0, vector[0]);
            Assert.AreEqual(0, vector.NonZerosCount);
        }

        [Test]
        [MultipleAsserts]
        public void CheckSparseMechanismByZeroMultiply()
        {
            var vector = new SparseVector(10000);

            //Add non-zero elements
            vector[200] = 1.5;
            vector[500] = 3.5;
            vector[800] = 5.5;
            vector[0] = 7.5;
            
            //Multiply by 0
            vector *= 0; 
            Assert.AreEqual(0, vector[200]);
            Assert.AreEqual(0, vector[500]);
            Assert.AreEqual(0, vector[800]);
            Assert.AreEqual(0, vector[0]);
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


            Assert.AreEqual(50.0, vectorA.DotProduct(vectorB));
        }

        [Test]
        public void CreateHugeSparseVector()
        {
            var data = new double[1000000];
            var rnd = new Random();
            for (int i = 0; i < 1000000; i++)
                data[i] = rnd.Next();

            var vector = new SparseVector(data);
        }
    }
}
