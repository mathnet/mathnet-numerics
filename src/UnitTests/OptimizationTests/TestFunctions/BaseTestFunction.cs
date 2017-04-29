// <copyright file="BaseTestFunction.cs" company="Math.NET">
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

using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public abstract class BaseTestFunction : ITestFunction
    {
        public abstract string Description { get; }
        public abstract int ParameterDimension { get; }
        public abstract int ItemDimension { get; }

        public abstract double ItemValue(Vector<double> x, int itemIndex);
        public abstract void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output);
        public abstract void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output);

        public virtual Vector<double> ItemGradient(Vector<double> x, int itemIndex)
        {
            var output = new LinearAlgebra.Double.DenseVector(this.ParameterDimension);
            this.ItemGradientByRef(x, itemIndex, output);
            return output;
        }

        public virtual Matrix<double> ItemHessian(Vector<double> x, int itemIndex)
        {
            var output = new LinearAlgebra.Double.DenseMatrix(this.ParameterDimension, this.ParameterDimension);
            this.ItemHessianByRef(x, itemIndex, output);
            return output;
        }


        public virtual void JacobianbyRef(Vector<double> x, Matrix<double> output)
        {
            for (int ii = 0; ii < this.ItemDimension; ++ii)
            {
                var grad = this.ItemGradient(x, ii);
                output.SetRow(ii, grad);
            }
        }

        public virtual Matrix<double> Jacobian(Vector<double> x)
        {
            var output = new LinearAlgebra.Double.DenseMatrix(this.ItemDimension, this.ParameterDimension);
            this.JacobianbyRef(x, output);
            return output;
        }

        public virtual void SsqGradientByRef(Vector<double> x, Vector<double> output)
        {
            if (output.Count != this.ParameterDimension)
                throw new ArgumentException($"Output vector must match parameter dimension of function; expected {this.ParameterDimension}, got {output.Count}.");

            for (int jj = 0; jj < this.ParameterDimension; ++jj)
                output[jj] = 0.0;
            var tmp_grad = new LinearAlgebra.Double.DenseVector(this.ParameterDimension);
            double tmp_value = 0.0;

            for (int ii = 0; ii < this.ItemDimension; ++ii)
            {
                tmp_value = this.ItemValue(x, ii);
                this.ItemGradientByRef(x, ii, tmp_grad);
                for (int jj = 0; jj < this.ParameterDimension; ++jj)
                    output[jj] += 2 * tmp_value * tmp_grad[jj];
            }
        }

        public virtual Vector<double> SsqGradient(Vector<double> x)
        {
            var output = new LinearAlgebra.Double.DenseVector(this.ParameterDimension);
            this.SsqGradientByRef(x, output);
            return output;
        }

        public virtual void SsqHessianByRef(Vector<double> x, Matrix<double> output)
        {
            if (output.RowCount != this.ParameterDimension || output.ColumnCount != this.ParameterDimension)
                throw new ArgumentException($"Output matrix must match parameter dimension of function; expected {this.ParameterDimension}x{this.ParameterDimension}, got {output.RowCount}x{output.ColumnCount}.");

            for (int ii = 0; ii < this.ParameterDimension; ++ii)
                for (int jj = 0; jj < this.ParameterDimension; ++jj)
                    output[ii,jj] = 0.0;

            var tmp_grad = new LinearAlgebra.Double.DenseVector(this.ParameterDimension);
            var tmp_hess = new LinearAlgebra.Double.DenseMatrix(this.ParameterDimension, this.ParameterDimension);
            double tmp_value = 0.0;

            for (int ii = 0; ii < this.ItemDimension; ++ii)
            {
                tmp_value = this.ItemValue(x, ii);
                this.ItemGradientByRef(x, ii, tmp_grad);
                this.ItemHessianByRef(x, ii, tmp_hess);
                for (int jj = 0; jj < this.ParameterDimension; ++jj)
                {
                    for (int kk = 0; kk < this.ParameterDimension; ++kk)
                    {
                        var increment = 2 * (tmp_value * tmp_hess[jj, kk] + tmp_grad[jj] * tmp_grad[kk]);
                        output[jj, kk] += increment;
                    }
                }
            }
        }

        public virtual Matrix<double> SsqHessian(Vector<double> x)
        {
            var output = new LinearAlgebra.Double.DenseMatrix(this.ParameterDimension, this.ParameterDimension);
            this.SsqHessianByRef(x, output);
            return output;
        }

        public virtual double SsqValue(Vector<double> x)
        {
            double ssq = 0.0;
            for (int ii = 0; ii < this.ItemDimension; ++ii)
            {
                var tmp = this.ItemValue(x, ii);
                ssq += tmp * tmp;
            }
            return ssq;
        }

    }
}
