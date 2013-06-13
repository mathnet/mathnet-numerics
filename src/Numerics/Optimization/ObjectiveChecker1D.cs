using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.Optimization
{
    public class CheckedEvaluation1D : IEvaluation1D
    {
        private ObjectiveChecker1D Checker;
        private IEvaluation1D InnerEvaluation;
        private bool ValueChecked;
        private bool DerivativeChecked;
        private bool SecondDerivativeChecked;

        public CheckedEvaluation1D(ObjectiveChecker1D checker, IEvaluation1D evaluation)
        {
            this.Checker = checker;
            this.InnerEvaluation = evaluation;
        }

        public double Point
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
                        throw new EvaluationException("Objective function evaluation failed.", e, new DenseVector(1, this.Point));
                    }
                    this.Checker.ValueChecker(tmp, this.InnerEvaluation.Point);
                }
                return this.InnerEvaluation.Value;
            }
        }

        public double Derivative
        {
            get
            {

                if (!this.DerivativeChecked)
                {
                    double tmp;
                    try
                    {
                        tmp = this.InnerEvaluation.Derivative;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective derivative evaluation failed.", e, new DenseVector(1, this.Point));
                    }
                    this.Checker.DerivativeChecker(tmp, this.InnerEvaluation.Point);
                }
                return this.InnerEvaluation.Derivative;
            }
        }

        public double SecondDerivative
        {
            get
            {

                if (!this.SecondDerivativeChecked)
                {
                    double tmp;
                    try
                    {
                        tmp = this.InnerEvaluation.SecondDerivative;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective second derivative evaluation failed.", e, new DenseVector(1,this.Point));
                    }
                    this.Checker.SecondDerivativeChecker(tmp, this.InnerEvaluation.Point);
                }
                return this.InnerEvaluation.SecondDerivative;
            }
        }
    }

    public class ObjectiveChecker1D : IObjectiveFunction1D
    {
        public IObjectiveFunction1D InnerObjective { get; private set; }
        public Action<double, double> ValueChecker { get; private set; }
        public Action<double, double> DerivativeChecker { get; private set; }
        public Action<double, double> SecondDerivativeChecker { get; private set; }

        public ObjectiveChecker1D(IObjectiveFunction1D objective, Action<double, double> value_checker, Action<double, double> gradient_checker, Action<double, double> hessian_checker)
        {
            this.InnerObjective = objective;
            this.ValueChecker = value_checker;
            this.DerivativeChecker = gradient_checker;
            this.SecondDerivativeChecker = hessian_checker;
        }

        public bool DerivativeSupported
        {
            get { return this.InnerObjective.DerivativeSupported; }
        }

        public bool SecondDerivativeSupported
        {
            get { return this.InnerObjective.SecondDerivativeSupported; }
        }

        public IEvaluation1D Evaluate(double point)
        {
            try
            {
                return new CheckedEvaluation1D(this, this.InnerObjective.Evaluate(point));
            }
            catch (Exception e)
            {
                throw new EvaluationException("Objective evaluation failed.", e, new DenseVector(1,point));
            }
        }
    }
}
