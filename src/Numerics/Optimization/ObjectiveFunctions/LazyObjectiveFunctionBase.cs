// <copyright file="LazyObjectiveFunctionBase.cs" company="Math.NET">
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

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    public abstract class LazyObjectiveFunctionBase : IObjectiveFunction
    {
        Vector<double> _point;

        protected bool HasFunctionValue { get; set; }
        protected double FunctionValue { get; set; }

        protected bool HasGradientValue { get; set; }
        protected Vector<double> GradientValue { get; set; }

        protected bool HasHessianValue { get; set; }
        protected Matrix<double> HessianValue { get; set; }

        protected LazyObjectiveFunctionBase(bool gradientSupported, bool hessianSupported)
        {
            IsGradientSupported = gradientSupported;
            IsHessianSupported = hessianSupported;
        }

        public abstract IObjectiveFunction CreateNew();

        public virtual IObjectiveFunction Fork()
        {
            // we need to deep-clone values since they may be updated inplace on evaluation
            LazyObjectiveFunctionBase fork = (LazyObjectiveFunctionBase)CreateNew();
            fork._point = _point?.Clone();
            fork.HasFunctionValue = HasFunctionValue;
            fork.FunctionValue = FunctionValue;
            fork.HasGradientValue = HasGradientValue;
            fork.GradientValue = GradientValue?.Clone();
            fork.HasHessianValue = HasHessianValue;
            fork.HessianValue = HessianValue?.Clone();
            return fork;
        }

        public bool IsGradientSupported { get; }
        public bool IsHessianSupported { get; }

        public void EvaluateAt(Vector<double> point)
        {
            _point = point;
            HasFunctionValue = false;
            HasGradientValue = false;
            HasHessianValue = false;
        }

        protected abstract void EvaluateValue();

        protected virtual void EvaluateGradient()
        {
            Gradient = null;
        }

        protected virtual void EvaluateHessian()
        {
            Hessian = null;
        }

        public Vector<double> Point => _point;

        public double Value
        {
            get
            {
                if (!HasFunctionValue)
                {
                    EvaluateValue();
                }
                return FunctionValue;
            }
            protected set
            {
                FunctionValue = value;
                HasFunctionValue = true;
            }
        }

        public Vector<double> Gradient
        {
            get
            {
                if (!HasGradientValue)
                {
                    EvaluateGradient();
                }
                return GradientValue;
            }
            protected set
            {
                GradientValue = value;
                HasGradientValue = true;
            }
        }

        public Matrix<double> Hessian
        {
            get
            {
                if (!HasHessianValue)
                {
                    EvaluateHessian();
                }
                return HessianValue;
            }
            protected set
            {
                HessianValue = value;
                HasHessianValue = true;
            }
        }
    }
}
