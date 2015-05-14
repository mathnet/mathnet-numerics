using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.Implementation
{
    public class CheckedEvaluation : IEvaluation
    {
        private bool _valueChecked;
        private bool _gradientChecked;
        private bool _hessianChecked;

        public IEvaluation InnerEvaluation { get; private set; }
        public Action<IEvaluation> ValueChecker { get; private set; }
        public Action<IEvaluation> GradientChecker { get; private set; }
        public Action<IEvaluation> HessianChecker { get; private set; }

        public CheckedEvaluation(IEvaluation objective, Action<IEvaluation> valueChecker, Action<IEvaluation> gradientChecker, Action<IEvaluation> hessianChecker)
        {
            InnerEvaluation = objective;
            ValueChecker = valueChecker;
            GradientChecker = gradientChecker;
            HessianChecker = hessianChecker;
        }

        public Vector<double> Point
        {
            get { return InnerEvaluation.Point; }
            set { InnerEvaluation.Point = value; }
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
                        tmp = InnerEvaluation.Value;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective function evaluation failed.", InnerEvaluation, e);
                    }
                    ValueChecker(InnerEvaluation);
                    _valueChecked = true;
                }
                return InnerEvaluation.Value;
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
                        tmp = InnerEvaluation.Gradient;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective gradient evaluation failed.", InnerEvaluation, e);
                    }
                    GradientChecker(InnerEvaluation);
                    _gradientChecked = true;
                }
                return InnerEvaluation.Gradient;
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
                        tmp = InnerEvaluation.Hessian;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective hessian evaluation failed.", InnerEvaluation, e);
                    }
                    HessianChecker(InnerEvaluation);
                    _hessianChecked = true;
                }
                return InnerEvaluation.Hessian;
            }
        }

        public IEvaluation CreateNew()
        {
            return new CheckedEvaluation(InnerEvaluation, ValueChecker, GradientChecker, HessianChecker);
        }

        public bool GradientSupported
        {
            get { return InnerEvaluation.GradientSupported; }
        }

        public bool HessianSupported
        {
            get { return InnerEvaluation.HessianSupported; }
        }
    }
}
