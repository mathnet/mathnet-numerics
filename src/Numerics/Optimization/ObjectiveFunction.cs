using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.Optimization
{
    [Flags]
    public enum EvaluationStatus { None = 0, Value = 1, Gradient = 2, Hessian = 4 }

    public interface IEvaluation
    {
        Vector<double> Point { get; }
        EvaluationStatus Status { get; }
        double Value { get; }
        Vector<double> Gradient { get; }
        Matrix<double> Hessian { get; }
    }

    public interface IObjectiveFunction
    {
        bool GradientSupported { get; }
        bool HessianSupported { get; }
        IEvaluation Evaluate(Vector<double> point);
    }

    public abstract class BaseEvaluation : IEvaluation
    {
        protected EvaluationStatus _status;
        protected double? _value;
        protected Vector<double> _gradient;
        protected Matrix<double> _hessian;
        protected Vector<double> _point;
        
        public Vector<double> Point { get { return _point; } }

        protected BaseEvaluation()
        {
            _status = EvaluationStatus.None;
        }

        public EvaluationStatus Status { get { return _status; } }

        public double Value 
        { 
            get 
            {
                if (!_value.HasValue)
                {
                    setValue();
                    _status |= EvaluationStatus.Value;
                }
                return _value.Value; 
            } 
        }
        public Vector<double> Gradient 
        { 
            get 
            {
                if (_gradient == null)
                {
                    setGradient();
                    _status |= EvaluationStatus.Gradient;
                }
                return _gradient; 
            } 
        }
        public Matrix<double> Hessian 
        { 
            get
            {
                if (_hessian == null)
                {
                    setHessian();
                    _status |= EvaluationStatus.Hessian;
                }
                return _hessian; 
            } 
        }

        protected abstract void setValue();
        protected abstract void setGradient();
        protected abstract void setHessian();
    }


    public class NullEvaluation : BaseEvaluation
    {

        public NullEvaluation(Vector<double> point)
        {
            _point = point;
            _status = EvaluationStatus.None;
        }

        public NullEvaluation(double point)
        {
            _point = DenseVector.Create(1, point);
            _status = EvaluationStatus.None;
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

    public class OneDEvaluationExpander : IEvaluation
    {
        public IEvaluation1D InnerEval { get; private set; }
        public OneDEvaluationExpander(IEvaluation1D eval)
        {
            this.InnerEval = eval;
        }


        public Vector<double> Point
        {
            get { return DenseVector.Create(1,this.InnerEval.Point); }
        }

        public EvaluationStatus Status
        {
            get { return this.InnerEval.Status; }
        }

        public double Value
        {
            get { return this.InnerEval.Value; }
        }

        public Vector<double> Gradient
        {
            get { return DenseVector.Create(1,this.InnerEval.Derivative) ; }
        }

        public Matrix<double> Hessian
        {
            get { return DenseMatrix.Create(1,1,this.InnerEval.SecondDerivative); }
        }
    }

    public class CachedEvaluation : BaseEvaluation
    {
        private SimpleObjectiveFunction _objective_object;

        public CachedEvaluation(SimpleObjectiveFunction f, Vector<double> point)
        {
            _objective_object = f;
			_point = point;
        }

        protected override void setValue()
        {
            _value = _objective_object.Objective(_point);
        }

        protected override void setGradient()
        {
            _gradient = _objective_object.Gradient(_point);
        }

        protected override void setHessian()
        {
            _hessian = _objective_object.Hessian(_point);
        }
    }

    public class SimpleObjectiveFunction : IObjectiveFunction
    {
        public Func<Vector<double>,double> Objective { get; private set; }
        public Func<Vector<double>,Vector<double>> Gradient { get; private set; }
        public Func<Vector<double>, Matrix<double>> Hessian { get; private set; }

        public SimpleObjectiveFunction(Func<Vector<double>,double> objective)
        {
            this.Objective = objective;
            this.Gradient = null;
            this.Hessian = null;
        }

        public SimpleObjectiveFunction(Func<Vector<double>,double> objective, Func<Vector<double>,Vector<double>> gradient)
        {
            this.Objective = objective;
            this.Gradient = gradient;
            this.Hessian = null;
        }

        public SimpleObjectiveFunction(Func<Vector<double>,double> objective, Func<Vector<double>,Vector<double>> gradient, Func<Vector<double>, Matrix<double>> hessian)
        {
            this.Objective = objective;
            this.Gradient = gradient;
            this.Hessian = hessian;
        }

        public bool GradientSupported
        {
            get { return this.Gradient != null; }
        }

        public bool HessianSupported
        {
            get { return this.Hessian != null; }
        }

        public IEvaluation Evaluate(Vector<double> point)
        {
            return new CachedEvaluation(this, point);
        }
    }
}
