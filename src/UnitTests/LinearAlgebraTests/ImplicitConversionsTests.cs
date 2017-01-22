// <copyright file="ImplicitConversionsTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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

using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests
{
#if NOSYSNUMERICS
    using Complex64 = Numerics.Complex;
#else
    using Complex64 = System.Numerics.Complex;
#endif
    [TestFixture]
    public class ImplicitConversionsTests
    {
        private const int Size = 2;

        [Test]
        public void DoubleMatrixMultipliedByComplexMatrixIsComplexMatrix()
        {
            var realMatrix = Matrix<double>.Build.Dense(Size, Size);
            var complexMatrix = Matrix<Complex64>.Build.Dense(Size, Size);

            var result = realMatrix*complexMatrix;

            Assert.IsInstanceOf<Matrix<Complex64>>(result);
        }

        [Test]
        public void FloatMatrixMultipliedByComplexMatrixIsComplexMatrix()
        {
            var floatMatrix = Matrix<float>.Build.Dense(Size, Size);
            var complexMatrix = Matrix<Complex64>.Build.Dense(Size, Size);

            var result = floatMatrix*complexMatrix;

            Assert.IsInstanceOf<Matrix<Complex64>>(result);
        }

        [Test]
        public void Complex32MatrixMultipliedByComplexMatrixIsComplexMatrix()
        {
            var complex32Matrix = Matrix<Numerics.Complex32>.Build.Dense(Size, Size);
            var complexMatrix = Matrix<Complex64>.Build.Dense(Size, Size);

            var result = complex32Matrix*complexMatrix;

            Assert.IsInstanceOf<Matrix<Complex64>>(result);
        }

        [Test]
        [Ignore("Doesn't work and doesn't compile because of missing implicit conversion from Matrix<float> to Matrix<double>")]
        public void FloatMatrixMultipliedByDoubleMatrixIsDoubleMatrix()
        {
            var floatMatrix = Matrix<float>.Build.Dense(Size, Size);
            var doubleMatrix = Matrix<double>.Build.Dense(Size, Size);

            var result = 1;//floatMatrix*doubleMatrix;

            Assert.IsInstanceOf<Matrix<double>>(result);
        }

        [Test]
        public void DoubleVectorPlusComplexVectorIsComplexVector()
        {
            var realVector = Vector<double>.Build.Dense(Size, Size);
            var complexVector = Vector<Complex64>.Build.Dense(Size, Size);

            var result = realVector + complexVector;

            Assert.IsInstanceOf<Vector<Complex64>>(result);
        }

        [Test]
        public void FloatVectorPlusComplexVectorIsComplexVector()
        {
            var floatVector = Vector<float>.Build.Dense(Size, Size);
            var complexVector = Vector<Complex64>.Build.Dense(Size, Size);

            var result = floatVector + complexVector;

            Assert.IsInstanceOf<Vector<Complex64>>(result);
        }

        [Test]
        public void Complex32VectorPlusComplexVectorIsComplexVector()
        {
            var complex32Vector = Vector<Numerics.Complex32>.Build.Dense(Size, Size);
            var complexVector = Vector<Complex64>.Build.Dense(Size, Size);

            var result = complex32Vector + complexVector;

            Assert.IsInstanceOf<Vector<Complex64>>(result);
        }

        [Test]
        [Ignore("Doesn't work and doesn't compile because of missing implicit conversion from Vector<float> to Vector<double>")]
        public void FloatVectorPlusDoubleVectorIsDoubleVector()
        {
            var floatVector = Vector<float>.Build.Dense(Size, Size);
            var doubleVector = Vector<double>.Build.Dense(Size, Size);

            var result = 1;//floatVector+doubleVector;

            Assert.IsInstanceOf<Vector<double>>(result);
        }
    }
}