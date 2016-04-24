// <copyright file="ReturnTypeTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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
using MathNet.Numerics.LinearAlgebra.Complex;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests.LinearAlgebraTests.Complex
{
#if NOSYSNUMERICS
    using Complex = Numerics.Complex;
#else
    using Complex = System.Numerics.Complex;
#endif

    [TestFixture, Category("LA")]
    public class ReturnTypeTests
    {
        readonly Vector<Complex> _vectorDense = Vector<Complex>.Build.Dense(3);
        readonly Vector<Complex> _vectorSparse = Vector<Complex>.Build.Sparse(3);
        readonly Matrix<Complex> _matrixDense = Matrix<Complex>.Build.Dense(3, 3);
        readonly Matrix<Complex> _matrixSparse = Matrix<Complex>.Build.Sparse(3, 3);
        readonly Matrix<Complex> _matrixDiagonal = Matrix<Complex>.Build.Diagonal(3, 3);

        [Test]
        public void VerifyExamples()
        {
            Assert.That(_vectorDense, Is.TypeOf<DenseVector>());
            Assert.That(_vectorSparse, Is.TypeOf<SparseVector>());
            Assert.That(_matrixDense, Is.TypeOf<DenseMatrix>());
            Assert.That(_matrixSparse, Is.TypeOf<SparseMatrix>());
            Assert.That(_matrixDiagonal, Is.TypeOf<DiagonalMatrix>());
        }

        [Test]
        public void Negate()
        {
            Assert.That(_vectorDense.Negate(), Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse.Negate(), Is.InstanceOf<SparseVector>());
            Assert.That(_matrixDense.Negate(), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.Negate(), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.Negate(), Is.InstanceOf<DiagonalMatrix>());
        }

        [Test]
        public void Conjugate()
        {
            Assert.That(_vectorDense.Conjugate(), Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse.Conjugate(), Is.InstanceOf<SparseVector>());
            Assert.That(_matrixDense.Conjugate(), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.Conjugate(), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.Conjugate(), Is.InstanceOf<DiagonalMatrix>());
        }

        [Test]
        public void Transpose()
        {
            Assert.That(_matrixDense.Transpose(), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.Transpose(), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.Transpose(), Is.InstanceOf<DiagonalMatrix>());
        }

        [Test]
        public void ConjugateTranspose()
        {
            Assert.That(_matrixDense.ConjugateTranspose(), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.ConjugateTranspose(), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.ConjugateTranspose(), Is.InstanceOf<DiagonalMatrix>());
        }

        [Test]
        public void Add()
        {
            Assert.That(_vectorDense + _vectorDense, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorDense + _vectorSparse, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse + _vectorDense, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse + _vectorSparse, Is.InstanceOf<SparseVector>());
            Assert.That(_matrixDense + _matrixDense, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense + _matrixSparse, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense + _matrixDiagonal, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse + _matrixDense, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse + _matrixSparse, Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixSparse + _matrixDiagonal, Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal + _matrixDense, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDiagonal + _matrixSparse, Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal + _matrixDiagonal, Is.InstanceOf<DiagonalMatrix>());
        }

        [Test]
        public void Subtract()
        {
            Assert.That(_vectorDense - _vectorDense, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorDense - _vectorSparse, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse - _vectorDense, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse - _vectorSparse, Is.InstanceOf<SparseVector>());
            Assert.That(_matrixDense - _matrixDense, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense - _matrixSparse, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense - _matrixDiagonal, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse - _matrixDense, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse - _matrixSparse, Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixSparse - _matrixDiagonal, Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal - _matrixDense, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDiagonal - _matrixSparse, Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal - _matrixDiagonal, Is.InstanceOf<DiagonalMatrix>());
        }

        [Test]
        public void PointwiseMultiply()
        {
            Assert.That(_vectorDense.PointwiseMultiply(_vectorDense), Is.InstanceOf<DenseVector>());
            Assert.That(_vectorDense.PointwiseMultiply(_vectorSparse), Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse.PointwiseMultiply(_vectorDense), Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse.PointwiseMultiply(_vectorSparse), Is.InstanceOf<SparseVector>());
            Assert.That(_matrixDense.PointwiseMultiply(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.PointwiseMultiply(_matrixSparse), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.PointwiseMultiply(_matrixDiagonal), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.PointwiseMultiply(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.PointwiseMultiply(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixSparse.PointwiseMultiply(_matrixDiagonal), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.PointwiseMultiply(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDiagonal.PointwiseMultiply(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.PointwiseMultiply(_matrixDiagonal), Is.InstanceOf<DiagonalMatrix>());
        }

        [Test]
        public void MatrixMultiply()
        {
            Assert.That(_matrixDense*_matrixDense, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense*_matrixSparse, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense*_matrixDiagonal, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse*_matrixDense, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse*_matrixSparse, Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixSparse*_matrixDiagonal, Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal*_matrixDense, Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDiagonal*_matrixSparse, Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal*_matrixDiagonal, Is.InstanceOf<DiagonalMatrix>());

            Assert.That(_matrixDense.TransposeThisAndMultiply(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.TransposeThisAndMultiply(_matrixSparse), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.TransposeThisAndMultiply(_matrixDiagonal), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.TransposeThisAndMultiply(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.TransposeThisAndMultiply(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixSparse.TransposeThisAndMultiply(_matrixDiagonal), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.TransposeThisAndMultiply(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDiagonal.TransposeThisAndMultiply(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.TransposeThisAndMultiply(_matrixDiagonal), Is.InstanceOf<DiagonalMatrix>());

            Assert.That(_matrixDense.TransposeAndMultiply(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.TransposeAndMultiply(_matrixSparse), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.TransposeAndMultiply(_matrixDiagonal), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.TransposeAndMultiply(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.TransposeAndMultiply(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixSparse.TransposeAndMultiply(_matrixDiagonal), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.TransposeAndMultiply(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDiagonal.TransposeAndMultiply(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.TransposeAndMultiply(_matrixDiagonal), Is.InstanceOf<DiagonalMatrix>());
        }

        [Test]
        public void MatrixVectorMultiply()
        {
            Assert.That(_matrixDense*_vectorDense, Is.InstanceOf<DenseVector>());
            Assert.That(_matrixDense*_vectorSparse, Is.InstanceOf<DenseVector>());
            Assert.That(_matrixSparse*_vectorDense, Is.InstanceOf<DenseVector>());
            Assert.That(_matrixSparse*_vectorSparse, Is.InstanceOf<SparseVector>());
            Assert.That(_matrixDiagonal*_vectorDense, Is.InstanceOf<DenseVector>());
            Assert.That(_matrixDiagonal*_vectorSparse, Is.InstanceOf<SparseVector>());

            Assert.That(_vectorDense*_matrixDense, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse*_matrixDense, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorDense*_matrixSparse, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse*_matrixSparse, Is.InstanceOf<SparseVector>());
            Assert.That(_vectorDense*_matrixDiagonal, Is.InstanceOf<DenseVector>());
            Assert.That(_vectorSparse*_matrixDiagonal, Is.InstanceOf<SparseVector>());

            Assert.That(_matrixDense.TransposeThisAndMultiply(_vectorDense), Is.InstanceOf<DenseVector>());
            Assert.That(_matrixDense.TransposeThisAndMultiply(_vectorSparse), Is.InstanceOf<DenseVector>());
            Assert.That(_matrixSparse.TransposeThisAndMultiply(_vectorDense), Is.InstanceOf<DenseVector>());
            Assert.That(_matrixSparse.TransposeThisAndMultiply(_vectorSparse), Is.InstanceOf<SparseVector>());
            Assert.That(_matrixDiagonal.TransposeThisAndMultiply(_vectorDense), Is.InstanceOf<DenseVector>());
            Assert.That(_matrixDiagonal.TransposeThisAndMultiply(_vectorSparse), Is.InstanceOf<SparseVector>());
        }

        [Test]
        public void Append()
        {
            Assert.That(_matrixDense.Append(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.Append(_matrixSparse), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.Append(_matrixDiagonal), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.Append(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.Append(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixSparse.Append(_matrixDiagonal), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.Append(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDiagonal.Append(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.Append(_matrixDiagonal), Is.InstanceOf<SparseMatrix>());
        }

        [Test]
        public void Stack()
        {
            Assert.That(_matrixDense.Stack(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.Stack(_matrixSparse), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.Stack(_matrixDiagonal), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.Stack(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.Stack(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixSparse.Stack(_matrixDiagonal), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.Stack(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDiagonal.Stack(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.Stack(_matrixDiagonal), Is.InstanceOf<SparseMatrix>());
        }

        [Test]
        public void DiagonalStack()
        {
            Assert.That(_matrixDense.DiagonalStack(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.DiagonalStack(_matrixSparse), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDense.DiagonalStack(_matrixDiagonal), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.DiagonalStack(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixSparse.DiagonalStack(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixSparse.DiagonalStack(_matrixDiagonal), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.DiagonalStack(_matrixDense), Is.InstanceOf<DenseMatrix>());
            Assert.That(_matrixDiagonal.DiagonalStack(_matrixSparse), Is.InstanceOf<SparseMatrix>());
            Assert.That(_matrixDiagonal.DiagonalStack(_matrixDiagonal), Is.InstanceOf<DiagonalMatrix>());

            // Special Case
            Assert.That(Matrix<Complex>.Build.DiagonalIdentity(2, 4).DiagonalStack(_matrixDiagonal), Is.InstanceOf<SparseMatrix>());
        }
    }
}
