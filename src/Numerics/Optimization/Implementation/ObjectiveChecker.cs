using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization.Implementation
{
    public class CheckedEvaluation : IEvaluation
    {
        public IEvaluation InnerEvaluation { get; private set; }
        private bool ValueChecked;
        private bool GradientChecked;
        private bool HessianChecked;
        public Action<IEvaluation> ValueChecker { get; private set; }
        public Action<IEvaluation> GradientChecker { get; private set; }
        public Action<IEvaluation> HessianChecker { get; private set; }


        public CheckedEvaluation(IEvaluation objective, Action<IEvaluation> value_checker, Action<IEvaluation> gradient_checker, Action<IEvaluation> hessian_checker)
        {
            this.InnerEvaluation = objective;
            this.ValueChecker = value_checker;
            this.GradientChecker = gradient_checker;
            this.HessianChecker = hessian_checker;
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
                    this.ValueChecker(this.InnerEvaluation);
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
                    this.GradientChecker(this.InnerEvaluation);
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
                    this.HessianChecker(InnerEvaluation);
                    this.HessianChecked = true;
                }
                return this.InnerEvaluation.Hessian;
            }
        }

        public IEvaluation CreateNew()
        {
            return new CheckedEvaluation(this.InnerEvaluation, this.ValueChecker, this.GradientChecker, this.HessianChecker);
        }

        public bool GradientSupported
        {
            get { return this.InnerEvaluation.GradientSupported; }
        }

        public bool HessianSupported
        {
            get { return this.InnerEvaluation.HessianSupported; }
        }
    }
}
