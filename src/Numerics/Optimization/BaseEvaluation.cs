using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public abstract class BaseEvaluation : IEvaluation
    {
        public EvaluationStatus Status { get; set; }
        public Vector<double> Point { get; set; }
        public double ValueRaw { get; set; }
        public Vector<double> GradientRaw { get; set; }
        public Matrix<double> HessianRaw { get; set; }
                
        protected BaseEvaluation()
        {
            Status = EvaluationStatus.None;
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

        public void Reset(Vector<double> new_point)
        {
            this.Point = new_point;
            this.Status = EvaluationStatus.None;
        }

        protected abstract void setValue();
        protected abstract void setGradient();
        protected abstract void setHessian();
    }
}
