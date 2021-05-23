// <copyright file="LazyObjectiveFunction.cs" company="Math.NET">
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

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    internal class LazyObjectiveFunction : IObjectiveFunction
    {
        readonly Func<Vector<double>, double> _function;
        readonly Func<Vector<double>, Vector<double>> _gradient;
        readonly Func<Vector<double>, Matrix<double>> _hessian;

        Vector<double> _point;

        bool _hasFunctionValue;
        double _functionValue;

        bool _hasGradientValue;
        Vector<double> _gradientValue;

        bool _hasHessianValue;
        Matrix<double> _hessianValue;

        public LazyObjectiveFunction(Func<Vector<double>, double> function, Func<Vector<double>, Vector<double>> gradient = null, Func<Vector<double>, Matrix<double>> hessian = null)
        {
            _function = function;
            _gradient = gradient;
            _hessian = hessian;

            IsGradientSupported = gradient != null;
            IsHessianSupported = hessian != null;
        }

        public IObjectiveFunction CreateNew()
        {
            return new LazyObjectiveFunction(_function, _gradient, _hessian);
        }

        public IObjectiveFunction Fork()
        {
            // no need to deep-clone values since they are replaced on evaluation
            return new LazyObjectiveFunction(_function, _gradient, _hessian)
            {
                _point = _point,
                _hasFunctionValue = _hasFunctionValue,
                _functionValue = _functionValue,
                _hasGradientValue = _hasGradientValue,
                _gradientValue = _gradientValue,
                _hasHessianValue = _hasHessianValue,
                _hessianValue = _hessianValue
            };
        }

        public bool IsGradientSupported { get; }
        public bool IsHessianSupported { get; }

        public void EvaluateAt(Vector<double> point)
        {
            _point = point;
            _hasFunctionValue = false;
            _hasGradientValue = false;
            _hasHessianValue = false;

            // don't keep references unnecessarily
            _gradientValue = null;
            _hessianValue = null;
        }

        public Vector<double> Point => _point;

        public double Value
        {
            get
            {
                if (!_hasFunctionValue)
                {
                    _functionValue = _function(_point);
                    _hasFunctionValue = true;
                }
                return _functionValue;
            }
        }

        public Vector<double> Gradient
        {
            get
            {
                if (!_hasGradientValue)
                {
                    _gradientValue = _gradient(_point);
                    _hasGradientValue = true;
                }
                return _gradientValue;
            }
        }

        public Matrix<double> Hessian
        {
            get
            {
                if (!_hasHessianValue)
                {
                    _hessianValue = _hessian(_point);
                    _hasHessianValue = true;
                }
                return _hessianValue;
            }
        }
    }
}
