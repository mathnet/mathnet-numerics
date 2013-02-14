using System;

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
        private double? _secondDerivative;
        private readonly SimpleObjectiveFunction1D _objectiveObject;
        private readonly double _point;

        public CachedEvaluation1D(SimpleObjectiveFunction1D f, double point)
        {
            _objectiveObject = f;
            _point = point;
        }
        private double SetValue()
        {
            _value = _objectiveObject.Objective(_point);
            return _value.Value;
        }
        private double SetDerivative()
        {
            _derivative = _objectiveObject.Derivative(_point);
            return _derivative.Value;
        }
        private double SetSecondDerivative()
        {
            _secondDerivative = _objectiveObject.SecondDerivative(_point);
            return _secondDerivative.Value;
        }

        public double Point { get { return _point; } }
        public double Value { get { return _value ?? SetValue(); } }
        public double Derivative { get { return _derivative ?? SetDerivative(); } }
        public double SecondDerivative { get { return _secondDerivative ?? SetSecondDerivative(); } }

    }

    public class SimpleObjectiveFunction1D : IObjectiveFunction1D
    {
        public Func<double, double> Objective { get; private set; }
        public Func<double, double> Derivative { get; private set; }
        public Func<double, double> SecondDerivative { get; private set; }

        public SimpleObjectiveFunction1D(Func<double, double> objective)
        {
            Objective = objective;
            Derivative = null;
            SecondDerivative = null;
        }

        public SimpleObjectiveFunction1D(Func<double, double> objective, Func<double, double> derivative)
        {
            Objective = objective;
            Derivative = derivative;
            SecondDerivative = null;
        }

        public SimpleObjectiveFunction1D(Func<double, double> objective, Func<double, double> derivative, Func<double,double> secondDerivative)
        {
            Objective = objective;
            Derivative = derivative;
            SecondDerivative = secondDerivative;
        }

        public bool DerivativeSupported
        {
            get { return Derivative != null; }
        }

        public bool SecondDerivativeSupported
        {
            get { return SecondDerivative != null; }
        }

        public IEvaluation1D Evaluate(double point)
        {
            return new CachedEvaluation1D(this, point);
        }
    }
}
