using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public interface IEvaluation1D
    {
        double Point { get; }
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

    public class CachedEvaluation1D : IEvaluation1D
    {
        private double? _value;
        private double? _derivative;
        private double? _second_derivative;
        private SimpleObjectiveFunction1D _objective_object;
        private double _point;

        public CachedEvaluation1D(SimpleObjectiveFunction1D f, double point)
        {
            _objective_object = f;
            _point = point;
        }
        private double setValue()
        {
            _value = _objective_object.Objective(_point);
            return _value.Value;
        }
        private double setDerivative()
        {
            _derivative = _objective_object.Derivative(_point);
            return _derivative.Value;
        }
        private double setSecondDerivative()
        {
            _second_derivative = _objective_object.SecondDerivative(_point);
            return _second_derivative.Value;
        }

        public double Point { get { return _point; } }
        public double Value { get { return _value ?? setValue(); } }
        public double Derivative { get { return _derivative ?? setDerivative(); } }
        public double SecondDerivative { get { return _second_derivative ?? setSecondDerivative(); } }

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
