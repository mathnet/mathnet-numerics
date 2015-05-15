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

        public bool IsGradientSupported { get; private set; }
        public bool IsHessianSupported { get; private set; }

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

        public Vector<double> Point
        {
            get { return _point; }
        }

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