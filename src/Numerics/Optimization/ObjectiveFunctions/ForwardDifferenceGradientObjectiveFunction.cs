using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    /// <summary>
    /// Adapts an objective function with only value implemented 
    /// to provide a gradient as well. Gradient calculation is 
    /// done using the finite difference method, specifically 
    /// forward differences.
    /// 
    /// For each gradient computed, the algorithm requires an 
    /// additional number of function evaluations equal to the 
    /// functions's number of input parameters.
    /// </summary>
    public class ForwardDifferenceGradientObjectiveFunction : IObjectiveFunction
    {
        public IObjectiveFunction InnerObjectiveFunction { get; protected set; }
        protected Vector<double> _lower_bound;
        protected Vector<double> _upper_bound;

        protected Vector<double> _point;
        protected bool _value_evaluated = false;
        protected bool _gradient_evaluated = false;
        protected Vector<double> _gradient;

        public double MinimumIncrement;
        public double RelativeIncrement;
        
        public ForwardDifferenceGradientObjectiveFunction(IObjectiveFunction valueOnlyObj, Vector<double> lowerBound, Vector<double> upperBound, double relativeIncrement=1e-5, double minimumIncrement=1e-8)
        {
            this.InnerObjectiveFunction = valueOnlyObj;
            _lower_bound = lowerBound;
            _upper_bound = upperBound;
            _gradient = new LinearAlgebra.Double.DenseVector(_lower_bound.Count);
            this.RelativeIncrement = relativeIncrement;
            this.MinimumIncrement = minimumIncrement;
        }

        protected void EvaluateValue()
        {
            _value_evaluated = true;
        }

        protected void EvaluateGradient()
        {
            if (!_value_evaluated)
                this.EvaluateValue();

            var tmp_point = _point.Clone();
            var tmp_obj = this.InnerObjectiveFunction.CreateNew();
            for (int ii = 0; ii < _gradient.Count; ++ii)
            {
                var orig_point = tmp_point[ii];
                var rel_incr = orig_point * this.RelativeIncrement;
                var h = Math.Max(rel_incr, this.MinimumIncrement);
                var mult = 1;
                if (orig_point + h > _upper_bound[ii])
                    mult = -1;

                tmp_point[ii] = orig_point + mult*h;
                tmp_obj.EvaluateAt(tmp_point);
                double bumped_value = tmp_obj.Value;
                _gradient[ii] = (mult * bumped_value - mult * this.InnerObjectiveFunction.Value) / h;

                tmp_point[ii] = orig_point;
            }
            _gradient_evaluated = true;
        }

        public Vector<double> Gradient
        {
            get
            {
                if (!_gradient_evaluated)
                    this.EvaluateGradient();
                return _gradient;
            }
        }

        public Matrix<double> Hessian
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsGradientSupported
        {
            get
            {
                return true;
            }
        }

        public bool IsHessianSupported
        {
            get
            {
                return false;
            }
        }

        public Vector<double> Point
        {
            get
            {
                return _point;
            }
        }

        public double Value
        {
            get
            {
                if (!_value_evaluated)
                    this.EvaluateValue();
                return this.InnerObjectiveFunction.Value;
            }
        }

        public IObjectiveFunction CreateNew()
        {
            var tmp = new ForwardDifferenceGradientObjectiveFunction(this.InnerObjectiveFunction.CreateNew(), _lower_bound, _upper_bound, this.RelativeIncrement, this.MinimumIncrement);
            return tmp;
        }

        public void EvaluateAt(Vector<double> point)
        {
            _point = point;
            _value_evaluated = false;
            _gradient_evaluated = false;
            this.InnerObjectiveFunction.EvaluateAt(point);
        }

        public IObjectiveFunction Fork()
        {
            var tmp = new ForwardDifferenceGradientObjectiveFunction(this.InnerObjectiveFunction.Fork(), _lower_bound, _upper_bound, this.RelativeIncrement, this.MinimumIncrement);
            tmp._point = _point?.Clone();
            tmp._gradient_evaluated = _gradient_evaluated;
            tmp._value_evaluated = _value_evaluated;
            tmp._gradient = _gradient?.Clone();

            return tmp;
        }
    }
}
