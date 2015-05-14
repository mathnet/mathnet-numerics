using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.Implementation
{
    public class NullEvaluation : BaseEvaluation
    {
        public NullEvaluation(Vector<double> point)
            : base(false, false)
        {
            Point = point;
        }
        protected override void SetValue()
        {
            throw new NotImplementedException();
        }

        protected override void SetGradient()
        {
            throw new NotImplementedException();
        }

        protected override void SetHessian()
        {
            throw new NotImplementedException();
        }

        public override IEvaluation CreateNew()
        {
            throw new NotImplementedException();
        }
    }
}
