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
        public IEvaluation InnerEvaluation { get; private set; }
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
        public EvaluationStatus Status { get { return this.InnerEvaluation.Status; } }

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
                        throw new EvaluationException("Objective function evaluation failed.", this.InnerEvaluation, e);
                    }
                    this.Checker.ValueChecker(this.InnerEvaluation);
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
                        throw new EvaluationException("Objective gradient evaluation failed.", this.InnerEvaluation, e);
                    }
                    this.Checker.GradientChecker(this.InnerEvaluation);
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
                        throw new EvaluationException("Objective hessian evaluation failed.", this.InnerEvaluation, e);
                    }
                    this.Checker.HessianChecker(InnerEvaluation);
                }
                return this.InnerEvaluation.Hessian;
            }
        }
    }

    public class ObjectiveChecker : IObjectiveFunction
    {
        public IObjectiveFunction InnerObjective { get; private set; }
        public Action<IEvaluation> ValueChecker { get; private set; }
        public Action<IEvaluation> GradientChecker { get; private set; }
        public Action<IEvaluation> HessianChecker { get; private set; }

        public ObjectiveChecker(IObjectiveFunction objective, Action<IEvaluation> value_checker, Action<IEvaluation> gradient_checker, Action<IEvaluation> hessian_checker)
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
                throw new EvaluationException("Objective evaluation failed.", new NullEvaluation(point), e);
            }
        }
    }
}
