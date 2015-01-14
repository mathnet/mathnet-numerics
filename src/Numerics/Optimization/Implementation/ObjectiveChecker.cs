using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.Implementation
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
            set { this.InnerEvaluation.Point = value; }
        }

        public EvaluationStatus Status
        {
            get
            {
                return this.InnerEvaluation.Status;
            }
            set
            {
                this.InnerEvaluation.Status = value;
            }
        }

        public double ValueRaw
        {
            get
            {
                return this.InnerEvaluation.Value;
            }
            set
            {
                this.InnerEvaluation.ValueRaw = value;
            }
        }

        public Vector<double> GradientRaw
        {
            get { return this.InnerEvaluation.GradientRaw;  }
            set { this.InnerEvaluation.GradientRaw = value; }
        }

        public Matrix<double> HessianRaw
        {
            get { return this.InnerEvaluation.HessianRaw; }
            set { this.InnerEvaluation.HessianRaw = value; }
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
                        throw new EvaluationException("Objective function evaluation failed.", this.InnerEvaluation, e);
                    }
                    this.Checker.ValueChecker(this.InnerEvaluation);
                    this.ValueChecked = true;
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
                    this.GradientChecked = true;
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
                    this.HessianChecked = true;
                }
                return this.InnerEvaluation.Hessian;
            }
        }

        public void Reset(Vector<double> new_point)
        {
            this.InnerEvaluation.Reset(new_point);
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

        public void Evaluate(Vector<double> point, IEvaluation output)
        {
            try
            {
                this.InnerObjective.Evaluate(point, output);
            }
            catch (Exception e)
            {
                throw new EvaluationException("Objective evaluation failed.", new NullEvaluation(point), e);
            }
        }


        public IEvaluation CreateEvaluationObject()
        {
            return this.InnerObjective.CreateEvaluationObject();
        }
    }
}
