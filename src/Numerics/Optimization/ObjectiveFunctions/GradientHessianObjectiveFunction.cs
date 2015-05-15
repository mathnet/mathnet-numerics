using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    internal class GradientHessianObjectiveFunction : IObjectiveFunction
    {
        readonly Func<Vector<double>, Tuple<double, Vector<double>, Matrix<double>>> _function;

        public GradientHessianObjectiveFunction(Func<Vector<double>, Tuple<double, Vector<double>, Matrix<double>>> function)
        {
            _function = function;
        }

        public IObjectiveFunction CreateNew()
        {
            return new GradientHessianObjectiveFunction(_function);
        }

        public IObjectiveFunction Fork()
        {
            // no need to deep-clone values since they are replaced on evaluation
            return new GradientHessianObjectiveFunction(_function)
            {
                Point = Point,
                Value = Value,
                Gradient = Gradient,
                Hessian = Hessian
            };
        }

        public bool IsGradientSupported
        {
            get { return true; }
        }

        public bool IsHessianSupported
        {
            get { return true; }
        }

        public void EvaluateAt(Vector<double> point)
        {
            Point = point;

            var result = _function(point);
            Value = result.Item1;
            Gradient = result.Item2;
            Hessian = result.Item3;
        }

        public Vector<double> Point { get; private set; }
        public double Value { get; private set; }
        public Vector<double> Gradient { get; private set; }
        public Matrix<double> Hessian { get; private set; }
    }
}