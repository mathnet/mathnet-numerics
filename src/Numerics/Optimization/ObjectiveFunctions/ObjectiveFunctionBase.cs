using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    public abstract class ObjectiveFunctionBase : IObjectiveFunction
    {
        protected ObjectiveFunctionBase(bool isGradientSupported, bool isHessianSupported)
        {
            IsGradientSupported = isGradientSupported;
            IsHessianSupported = isHessianSupported;
        }

        public abstract IObjectiveFunction CreateNew();

        public virtual IObjectiveFunction Fork()
        {
            // we need to deep-clone values since they may be updated inplace on evaluation
            ObjectiveFunctionBase objective = (ObjectiveFunctionBase)CreateNew();
            objective.Point = Point == null ? null : Point.Clone();
            objective.Value = Value;
            objective.Gradient = Gradient == null ? null : Gradient.Clone();
            objective.Hessian = Hessian == null ? null : Hessian.Clone();
            return objective;
        }

        public bool IsGradientSupported { get; private set; }
        public bool IsHessianSupported { get; private set; }

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;
            Evaluate();
        }

        protected abstract void Evaluate();

        public Vector<double> Point { get; private set; }
        public double Value { get; protected set; }
        public Vector<double> Gradient { get; protected set; }
        public Matrix<double> Hessian { get; protected set; }
    }
}
