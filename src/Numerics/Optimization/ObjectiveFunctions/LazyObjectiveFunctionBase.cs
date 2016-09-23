using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    public abstract class LazyObjectiveFunctionBase : IObjectiveFunction
    {
        Vector<double> _point;

        protected bool _hasFunctionValue;
        protected double _functionValue;

        protected bool _hasGradientValue;
        protected Vector<double> _gradientValue;

        protected bool _hasHessianValue;
        protected Matrix<double> _hessianValue;

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
            fork._point = _point == null ? null : _point.Clone();
            fork._hasFunctionValue = _hasFunctionValue;
            fork._functionValue = _functionValue;
            fork._hasGradientValue = _hasGradientValue;
            fork._gradientValue = _gradientValue == null ? null : _gradientValue.Clone(); ;
            fork._hasHessianValue = _hasHessianValue;
            fork._hessianValue = _hessianValue == null ? null : _hessianValue.Clone();
            return fork;
        }

        public bool IsGradientSupported { get; private set; }
        public bool IsHessianSupported { get; private set; }

        public void EvaluateAt(Vector<double> point)
        {
            _point = point;
            _hasFunctionValue = false;
            _hasGradientValue = false;
            _hasHessianValue = false;
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
                    EvaluateValue();
                }
                return _functionValue;
            }
            protected set
            {
                _functionValue = value;
                _hasFunctionValue = true;
            }
        }

        public Vector<double> Gradient
        {
            get
            {
                if (!_hasGradientValue)
                {
                    EvaluateGradient();
                }
                return _gradientValue;
            }
            protected set
            {
                _gradientValue = value;
                _hasGradientValue = true;
            }
        }

        public Matrix<double> Hessian
        {
            get
            {
                if (!_hasHessianValue)
                {
                    EvaluateHessian();
                }
                return _hessianValue;
            }
            protected set
            {
                _hessianValue = value;
                _hasHessianValue = true;
            }
        }
    }
}
