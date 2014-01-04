// <copyright file="ObjectiveChecker.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

using System;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    public class CheckedEvaluation : IEvaluation
    {
        readonly ObjectiveChecker Checker;
        public IEvaluation InnerEvaluation { get; private set; }
        bool ValueChecked;
        bool GradientChecked;
        bool HessianChecked;

        public CheckedEvaluation(ObjectiveChecker checker, IEvaluation evaluation)
        {
            Checker = checker;
            InnerEvaluation = evaluation;
        }

        public Vector<double> Point
        {
            get { return InnerEvaluation.Point; }
        }

        public EvaluationStatus Status
        {
            get { return InnerEvaluation.Status; }
        }

        public double Value
        {
            get
            {
                if (!ValueChecked)
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
                    Checker.ValueChecker(InnerEvaluation);
                }
                return InnerEvaluation.Value;
            }
        }

        public Vector<double> Gradient
        {
            get
            {
                if (!GradientChecked)
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
                    Checker.GradientChecker(InnerEvaluation);
                }
                return InnerEvaluation.Gradient;
            }
        }

        public Matrix<double> Hessian
        {
            get
            {
                if (!HessianChecked)
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
                    Checker.HessianChecker(InnerEvaluation);
                }
                return InnerEvaluation.Hessian;
            }
        }
    }

    public class ObjectiveChecker : IObjectiveFunction
    {
        public IObjectiveFunction InnerObjective { get; private set; }
        public Action<IEvaluation> ValueChecker { get; private set; }
        public Action<IEvaluation> GradientChecker { get; private set; }
        public Action<IEvaluation> HessianChecker { get; private set; }

        public ObjectiveChecker(IObjectiveFunction objective, Action<IEvaluation> valueChecker, Action<IEvaluation> gradientChecker, Action<IEvaluation> hessianChecker)
        {
            InnerObjective = objective;
            ValueChecker = valueChecker;
            GradientChecker = gradientChecker;
            HessianChecker = hessianChecker;
        }

        public bool GradientSupported
        {
            get { return InnerObjective.GradientSupported; }
        }

        public bool HessianSupported
        {
            get { return InnerObjective.HessianSupported; }
        }

        public IEvaluation Evaluate(Vector<double> point)
        {
            try
            {
                return new CheckedEvaluation(this, InnerObjective.Evaluate(point));
            }
            catch (Exception e)
            {
                throw new EvaluationException("Objective evaluation failed.", new NullEvaluation(point), e);
            }
        }
    }
}
