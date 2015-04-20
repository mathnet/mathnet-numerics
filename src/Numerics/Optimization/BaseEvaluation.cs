using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public abstract class BaseEvaluation : IEvaluation
    {
        public EvaluationStatus Status { get; protected set; }

        protected Vector<double> PointRaw { get; set; }
        protected double ValueRaw { get; set; }
        protected Vector<double> GradientRaw { get; set; }
        protected Matrix<double> HessianRaw { get; set; }

        public bool GradientSupported { get; private set; }
        public bool HessianSupported { get; private set; }

        protected BaseEvaluation(bool gradient_supported, bool hessian_supported)
        {
            Status = EvaluationStatus.None;
            this.GradientSupported = gradient_supported;
            this.HessianSupported = hessian_supported;
        }

        public Vector<double> Point
        {
            get
            {
                return this.PointRaw;
            }
            set
            {
                this.PointRaw = value;
                this.Status = EvaluationStatus.None;
            }
        }

        public double Value
        {
            get
            {
                if (!Status.HasFlag(EvaluationStatus.Value))
                {
                    setValue();
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
                    setGradient();
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
                    setHessian();
                    Status |= EvaluationStatus.Hessian;
                }
                return HessianRaw;
            }
        }

        protected abstract void setValue();
        protected abstract void setGradient();
        protected abstract void setHessian();
        public abstract IEvaluation CreateNew();
    }
}
