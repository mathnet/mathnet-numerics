using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class CheckedEvaluation : IEvaluation
    {
        private ObjectiveChecker Checker;
        private IEvaluation InnerEvaluation;
        private bool ValueChecked;
        private bool GradientChecked;
        private bool HessianChecked;

        public CheckedEvaluation(ObjectiveChecker checker, IEvaluation evaluation)
        {
            this.Checker = checker;
            this.InnerEvaluation = evaluation;
        }

        public Vector<double> Point
        {
            get { return this.InnerEvaluation.Point; }
        }

        public double Value
        {
            get
            {

                if (!this.ValueChecked)
                {
                    double tmp;
                    try
                    {
                        tmp = this.InnerEvaluation.Value;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective function evaluation failed.", e);
                    }
                    this.Checker.ValueChecker(tmp,this.InnerEvaluation.Point);
                }
                return this.InnerEvaluation.Value;
            }
        }

        public Vector<double> Gradient
        {
            get
            {

                if (!this.GradientChecked)
                {
                    Vector<double> tmp;
                    try
                    {
                        tmp = this.InnerEvaluation.Gradient;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective gradient evaluation failed.", e);
                    }
                    this.Checker.GradientChecker(tmp, this.InnerEvaluation.Point);
                }
                return this.InnerEvaluation.Gradient;
            }
        }

        public Matrix<double> Hessian
        {
            get
            {

                if (!this.HessianChecked)
                {
                    Matrix<double> tmp;
                    try
                    {
                        tmp = this.InnerEvaluation.Hessian;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective hessian evaluation failed.", e);
                    }
                    this.Checker.HessianChecker(tmp, this.InnerEvaluation.Point);
                }
                return this.InnerEvaluation.Hessian;
            }
        }
    }

    public class ObjectiveChecker : IObjectiveFunction
    {
        public IObjectiveFunction InnerObjective { get; private set; }
        public Action<double, Vector<double>> ValueChecker { get; private set; }
        public Action<Vector<double>, Vector<double>> GradientChecker { get; private set; }
        public Action<Matrix<double>, Vector<double>> HessianChecker { get; private set; }

        public ObjectiveChecker(IObjectiveFunction objective, Action<double, Vector<double>> value_checker, Action<Vector<double>, Vector<double>> gradient_checker, Action<Matrix<double>, Vector<double>> hessian_checker)
        {
            this.InnerObjective = objective;
            this.ValueChecker = value_checker;
            this.GradientChecker = gradient_checker;
            this.HessianChecker = hessian_checker;
        }

        public bool GradientSupported
        {
            get { return this.InnerObjective.GradientSupported; }
        }

        public bool HessianSupported
        {
            get { return this.InnerObjective.HessianSupported; }
        }

        public IEvaluation Evaluate(Vector<double> point)
        {
            try
            {
                return new CheckedEvaluation(this, this.InnerObjective.Evaluate(point));
            }
            catch (Exception e)
            {
                throw new EvaluationException("Objective evaluation failed.", e);
            }
        }
    }
}
