using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    public abstract class InplaceObjectiveFunction : IObjectiveFunction
    {
        Vector<double> _point;
        double _functionValue;
        Vector<double> _gradientValue;
        Matrix<double> _hessianValue;

        protected InplaceObjectiveFunction(bool isGradientSupported, bool isHessianSupported)
        {
            IsGradientSupported = isGradientSupported;
            IsHessianSupported = isHessianSupported;
        }

        public abstract IObjectiveFunction CreateNew();

        public virtual IObjectiveFunction Fork()
        {
            // no need to deep-clone values since they are replaced on evaluation
            InplaceObjectiveFunction objective = (InplaceObjectiveFunction)CreateNew();
            objective._point = _point == null ? null : _point.Clone();
            objective._functionValue = _functionValue;
            objective._gradientValue = _gradientValue == null ? null : _gradientValue.Clone();
            objective._hessianValue = _hessianValue == null ? null : _hessianValue.Clone();
            return objective;
        }

        public bool IsGradientSupported { get; private set; }
        public bool IsHessianSupported { get; private set; }

        public void EvaluateAt(Vector<double> point)
        {
            _point = point;
            EvaluateAt(_point, ref _functionValue, ref _gradientValue, ref _hessianValue);
        }

        protected abstract void EvaluateAt(Vector<double> point, ref double value, ref Vector<double> gradient, ref Matrix<double> hessian);

        public Vector<double> Point
        {
            get { return _point; }
        }

        public double Value
        {
            get { return _functionValue; }
        }

        public Vector<double> Gradient
        {
            get { return _gradientValue; }
        }

        public Matrix<double> Hessian
        {
            get { return _hessianValue; }
        }
    }
}
