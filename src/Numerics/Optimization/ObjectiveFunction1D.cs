using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public interface IEvaluation1D
    {
        double Point { get; }
        EvaluationStatus Status { get; }
        double Value { get; }
        double Derivative { get; }
        double SecondDerivative { get; }
    }

    public interface IObjectiveFunction1D
    {
        bool DerivativeSupported { get; }
        bool SecondDerivativeSupported { get; }
        IEvaluation1D Evaluate(double point);
    }

    public abstract class BaseEvaluation1D : IEvaluation1D
    {
        protected double _point;
        protected EvaluationStatus _status;
        protected double? _value;
        protected double? _derivative;
        protected double? _second_derivative;

        public double Point { get { return _point; } }

        protected BaseEvaluation1D()
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
        public double Derivative
        {
            get
            {
                if (_derivative == null)
                {
                    setDerivative();
                    _status |= EvaluationStatus.Gradient;
                }
                return _derivative.Value;
            }
        }
        public double SecondDerivative
        {
            get
            {
                if (_second_derivative == null)
                {
                    setSecondDerivative();
                    _status |= EvaluationStatus.Hessian;
                }
                return _second_derivative.Value;
            }
        }

        protected abstract void setValue();
        protected abstract void setDerivative();
        protected abstract void setSecondDerivative();
    }

    public class CachedEvaluation1D : BaseEvaluation1D
    {
        private SimpleObjectiveFunction1D _objective_object;

        public CachedEvaluation1D(SimpleObjectiveFunction1D f, double point)
        {
            _objective_object = f;
            _point = point;
        }
        protected override void setValue()
        {
            _value = _objective_object.Objective(_point);
        }
        protected override void setDerivative()
        {
            _derivative = _objective_object.Derivative(_point);
        }
        protected override void setSecondDerivative()
        {
            _second_derivative = _objective_object.SecondDerivative(_point);
        }
    }

    public class SimpleObjectiveFunction1D : IObjectiveFunction1D
    {
        public Func<double, double> Objective { get; private set; }
        public Func<double, double> Derivative { get; private set; }
        public Func<double, double> SecondDerivative { get; private set; }

        public SimpleObjectiveFunction1D(Func<double, double> objective)
        {
            this.Objective = objective;
            this.Derivative = null;
            this.SecondDerivative = null;
        }

        public SimpleObjectiveFunction1D(Func<double, double> objective, Func<double, double> derivative)
        {
            this.Objective = objective;
            this.Derivative = derivative;
            this.SecondDerivative = null;
        }

        public SimpleObjectiveFunction1D(Func<double, double> objective, Func<double, double> derivative, Func<double,double> second_derivative)
        {
            this.Objective = objective;
            this.Derivative = derivative;
            this.SecondDerivative = second_derivative;
        }

        public bool DerivativeSupported
        {
            get { return this.Derivative != null; }
        }

        public bool SecondDerivativeSupported
        {
            get { return this.SecondDerivative != null; }
        }

        public IEvaluation1D Evaluate(double point)
        {
            return new CachedEvaluation1D(this, point);
        }
    }
}
