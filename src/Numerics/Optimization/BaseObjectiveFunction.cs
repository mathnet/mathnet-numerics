using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public class BaseObjectiveFunction<T> : IObjectiveFunction where T : IEvaluation
    {
        public BaseObjectiveFunction(bool gradient_supported, bool hessian_supported)
        {
            _gradient_supported = gradient_supported;
            _hessian_supported = hessian_supported;
        }

        private bool _gradient_supported;
        private bool _hessian_supported;

        public bool GradientSupported
        {
            get { return _gradient_supported; }
        }

        public bool HessianSupported
        {
            get { return _hessian_supported; }
        }

        public void Evaluate(LinearAlgebra.Vector<double> point, IEvaluation output)
        {
            output.Reset(point);
        }

        public virtual IEvaluation CreateEvaluationObject()
        {
            return default(T);
        }
    }
}
