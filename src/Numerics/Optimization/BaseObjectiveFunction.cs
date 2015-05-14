using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public abstract class BaseObjectiveFunction : IObjectiveFunction
    {
        public EvaluationStatus Status { get; protected set; }

        protected Vector<double> PointRaw { get; set; }
        protected double ValueRaw { get; set; }
        protected Vector<double> GradientRaw { get; set; }
        protected Matrix<double> HessianRaw { get; set; }

        public bool GradientSupported { get; private set; }
        public bool HessianSupported { get; private set; }

        protected BaseObjectiveFunction(bool gradientSupported, bool hessianSupported)
        {
            Status = EvaluationStatus.None;
            GradientSupported = gradientSupported;
            HessianSupported = hessianSupported;
        }

        public Vector<double> Point
        {
            get
            {
                return PointRaw;
            }
            set
            {
                PointRaw = value;
                Status = EvaluationStatus.None;
            }
        }

        public double Value
        {
            get
            {
                if (!Status.HasFlag(EvaluationStatus.Value))
                {
                    SetValue();
                    Status |= EvaluationStatus.Value;
                }
                return ValueRaw;
            }
        }
        public Vector<double> Gradient
        {
            get
            {
                if (!Status.HasFlag(EvaluationStatus.Gradient))
                {
                    SetGradient();
                    Status |= EvaluationStatus.Gradient;
                }
                return GradientRaw;
            }
        }
        public Matrix<double> Hessian
        {
            get
            {
                if (!Status.HasFlag(EvaluationStatus.Hessian))
                {
                    SetHessian();
                    Status |= EvaluationStatus.Hessian;
                }
                return HessianRaw;
            }
        }

        protected abstract void SetValue();
        protected abstract void SetGradient();
        protected abstract void SetHessian();
        public abstract IObjectiveFunction CreateNew();
    }
}
