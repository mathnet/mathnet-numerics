using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.Implementation
{
    public class CheckedObjectiveFunction : IObjectiveFunction
    {
        private bool _valueChecked;
        private bool _gradientChecked;
        private bool _hessianChecked;

        public IObjectiveFunction InnerObjectiveFunction { get; private set; }
        public Action<IObjectiveFunction> ValueChecker { get; private set; }
        public Action<IObjectiveFunction> GradientChecker { get; private set; }
        public Action<IObjectiveFunction> HessianChecker { get; private set; }

        public CheckedObjectiveFunction(IObjectiveFunction objective, Action<IObjectiveFunction> valueChecker, Action<IObjectiveFunction> gradientChecker, Action<IObjectiveFunction> hessianChecker)
        {
            InnerObjectiveFunction = objective;
            ValueChecker = valueChecker;
            GradientChecker = gradientChecker;
            HessianChecker = hessianChecker;
        }

        public Vector<double> Point
        {
            get { return InnerObjectiveFunction.Point; }
        }

        public void EvaluateAt(Vector<double> point)
        {
            InnerObjectiveFunction.EvaluateAt(point);
        }

        public double Value
        {
            get
            {

                if (!_valueChecked)
                {
                    double tmp;
                    try
                    {
                        tmp = InnerObjectiveFunction.Value;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective function evaluation failed.", InnerObjectiveFunction, e);
                    }
                    ValueChecker(InnerObjectiveFunction);
                    _valueChecked = true;
                }
                return InnerObjectiveFunction.Value;
            }
        }

        public Vector<double> Gradient
        {
            get
            {

                if (!_gradientChecked)
                {
                    Vector<double> tmp;
                    try
                    {
                        tmp = InnerObjectiveFunction.Gradient;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective gradient evaluation failed.", InnerObjectiveFunction, e);
                    }
                    GradientChecker(InnerObjectiveFunction);
                    _gradientChecked = true;
                }
                return InnerObjectiveFunction.Gradient;
            }
        }

        public Matrix<double> Hessian
        {
            get
            {

                if (!_hessianChecked)
                {
                    Matrix<double> tmp;
                    try
                    {
                        tmp = InnerObjectiveFunction.Hessian;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective hessian evaluation failed.", InnerObjectiveFunction, e);
                    }
                    HessianChecker(InnerObjectiveFunction);
                    _hessianChecked = true;
                }
                return InnerObjectiveFunction.Hessian;
            }
        }

        public IObjectiveFunction Fork()
        {
            return new CheckedObjectiveFunction(InnerObjectiveFunction, ValueChecker, GradientChecker, HessianChecker);
        }

        public bool IsGradientSupported
        {
            get { return InnerObjectiveFunction.IsGradientSupported; }
        }

        public bool IsHessianSupported
        {
            get { return InnerObjectiveFunction.IsHessianSupported; }
        }
    }
}
