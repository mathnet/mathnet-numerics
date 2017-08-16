// <copyright file="MghObjectiveFunction.cs" company="Math.NET">
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

using MathNet.Numerics.Optimization;
using MathNet.Numerics.Optimization.ObjectiveFunctions;
using MathNet.Numerics.UnitTests.OptimizationTests.TestFunctions;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    public class MghObjectiveFunction : LazyObjectiveFunctionBase
    {
        private ITestFunction TestFunction;

        public MghObjectiveFunction(ITestFunction testFunction, bool use_gradient, bool use_hessian)
            : base(use_gradient, use_hessian)
        {
            this.TestFunction = testFunction;
        }

        public override IObjectiveFunction CreateNew()
        {
            return new MghObjectiveFunction(this.TestFunction, this.IsGradientSupported, this.IsHessianSupported);
        }

        protected override void EvaluateValue()
        {
            this.Value = this.TestFunction.SsqValue(this.Point);
        }

        protected override void EvaluateGradient()
        {
            if (this.IsGradientSupported)
            {
                if (this.GradientValue == null)
                    this.Gradient = new DenseVector(this.TestFunction.ParameterDimension);
                this.TestFunction.SsqGradientByRef(this.Point, GradientValue);
            }
        }

        protected override void EvaluateHessian()
        {
            if (this.IsHessianSupported)
            {
                if (this.HessianValue == null)
                    this.Hessian = new DenseMatrix(this.TestFunction.ParameterDimension, this.TestFunction.ParameterDimension);
                this.TestFunction.SsqHessianByRef(this.Point, HessianValue);
            }
        }
    }
}
