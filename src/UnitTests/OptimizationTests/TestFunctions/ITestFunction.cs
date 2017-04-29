// <copyright file="ITestFunction.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions
{
    public class TestCase
    {
        public string CaseName;
        public ITestFunction Function;
        public DenseVector InitialGuess;
        public DenseVector LowerBound;
        public DenseVector UpperBound;
        public double MinimalValue;
        public DenseVector MinimizingPoint;

        public bool IsBounded
        {
            get
            {
                return this.LowerBound != null && this.UpperBound != null;
            }
        }

        public bool IsUnbounded
        {
            get
            {
                return this.IsUnboundedOverride ?? this.LowerBound == null || this.UpperBound == null;
            }
        }

        public bool? IsUnboundedOverride;

        public string FullName
        {
            get
            {
                return $"{this.Function.Description} {this.CaseName}";
            }
        }
    }

    public interface ITestFunction
    {
        string Description { get; }
        int ParameterDimension { get; }
        int ItemDimension { get; }

        double ItemValue(Vector<double> x, int itemIndex);
        Vector<double> ItemGradient(Vector<double> x, int itemIndex);
        void ItemGradientByRef(Vector<double> x, int itemIndex, Vector<double> output);
        Matrix<double> ItemHessian(Vector<double> x, int itemIndex);
        void ItemHessianByRef(Vector<double> x, int itemIndex, Matrix<double> output);

        Matrix<double> Jacobian(Vector<double> x);
        void JacobianbyRef(Vector<double> x, Matrix<double> output);

        double SsqValue(Vector<double> x);
        Vector<double> SsqGradient(Vector<double> x);
        void SsqGradientByRef(Vector<double> x, Vector<double> output);
        Matrix<double> SsqHessian(Vector<double> x);
        void SsqHessianByRef(Vector<double> x, Matrix<double> output);
    }
}
