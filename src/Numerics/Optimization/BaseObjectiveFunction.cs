using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public abstract class BaseObjectiveFunction : IObjectiveFunction
    {
        [Flags]
        enum EvaluationStatus
        {
            None = 0,
            Value = 1,
            Gradient = 2,
            Hessian = 4
        }

        EvaluationStatus Status { get; set; }

        protected Vector<double> PointRaw { get; set; }
        protected double ValueRaw { get; set; }
        protected Vector<double> GradientRaw { get; set; }
        protected Matrix<double> HessianRaw { get; set; }

        public bool IsGradientSupported { get; private set; }
        public bool IsHessianSupported { get; private set; }

        protected BaseObjectiveFunction(bool gradientSupported, bool hessianSupported)
        {
            Status = EvaluationStatus.None;
            IsGradientSupported = gradientSupported;
            IsHessianSupported = hessianSupported;
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

        public void EvaluateAt(Vector<double> point)
        {
            PointRaw = point;
            Status = EvaluationStatus.None;
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

        public abstract IObjectiveFunction CreateNew();

        public virtual IObjectiveFunction Fork()
        {
            BaseObjectiveFunction fork = (BaseObjectiveFunction)CreateNew();
            fork.PointRaw = PointRaw;
            fork.ValueRaw = ValueRaw;
            fork.GradientRaw = GradientRaw;
            fork.HessianRaw = HessianRaw;
            fork.Status = Status;
            return fork;
        }

        protected abstract void SetValue();
        protected abstract void SetGradient();
        protected abstract void SetHessian();
    }
}
