using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.Implementation
{
    public class NullEvaluation : BaseEvaluation
    {
        public NullEvaluation(Vector<double> point) 
            : base()
        {
            this.Point = point;
        }
        protected override void setValue()
        {
            throw new NotImplementedException();
        }

        protected override void setGradient()
        {
            throw new NotImplementedException();
        }

        protected override void setHessian()
        {
            throw new NotImplementedException();
        }
    }
}
