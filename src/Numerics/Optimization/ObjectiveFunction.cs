using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public interface IEvaluation
    {
        Vector<double> Point { get; }
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

    public class CachedEvaluation : IEvaluation
    {
        private double? _value;
        private Vector<double> _gradient;
        private Matrix<double> _hessian;
        private SimpleObjectiveFunction _objective_object;
        private Vector<double> _point;

        public CachedEvaluation(SimpleObjectiveFunction f, Vector<double> point)
        {
            _objective_object = f;
			_point = point;
        }
        private double setValue()
        {
            _value = _objective_object.Objective(_point);
            return _value.Value;
        }
        private Vector<double> setGradient()
        {
            _gradient = _objective_object.Gradient(_point);
            return _gradient;
        }
        private Matrix<double> setHessian()
        {
            _hessian = _objective_object.Hessian(_point);
            return _hessian;
        }

        public Vector<double> Point { get { return _point; } }
        public double Value { get { return _value ?? setValue(); } }
        public Vector<double> Gradient { get { return _gradient ?? setGradient(); } }
        public Matrix<double> Hessian { get { return _hessian ?? setHessian(); } }

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
