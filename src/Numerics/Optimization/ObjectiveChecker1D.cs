// <copyright file="ObjectiveChecker1D.cs" company="Math.NET">
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

namespace MathNet.Numerics.Optimization
{
    public class CheckedEvaluation1D : IEvaluation1D
    {
        readonly ObjectiveChecker1D Checker;
        readonly IEvaluation1D InnerEvaluation;
        bool ValueChecked;
        bool DerivativeChecked;
        bool SecondDerivativeChecked;

        public CheckedEvaluation1D(ObjectiveChecker1D checker, IEvaluation1D evaluation)
        {
            Checker = checker;
            InnerEvaluation = evaluation;
        }

        public double Point
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

        public double Derivative
        {
            get
            {
                if (!DerivativeChecked)
                {
                    double tmp;
                    try
                    {
                        tmp = InnerEvaluation.Derivative;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective derivative evaluation failed.", InnerEvaluation, e);
                    }
                    Checker.DerivativeChecker(InnerEvaluation);
                }
                return InnerEvaluation.Derivative;
            }
        }

        public double SecondDerivative
        {
            get
            {
                if (!SecondDerivativeChecked)
                {
                    double tmp;
                    try
                    {
                        tmp = InnerEvaluation.SecondDerivative;
                    }
                    catch (Exception e)
                    {
                        throw new EvaluationException("Objective second derivative evaluation failed.", InnerEvaluation, e);
                    }
                    Checker.SecondDerivativeChecker(InnerEvaluation);
                }
                return InnerEvaluation.SecondDerivative;
            }
        }
    }

    public class ObjectiveChecker1D : IObjectiveFunction1D
    {
        public IObjectiveFunction1D InnerObjective { get; private set; }
        public Action<IEvaluation1D> ValueChecker { get; private set; }
        public Action<IEvaluation1D> DerivativeChecker { get; private set; }
        public Action<IEvaluation1D> SecondDerivativeChecker { get; private set; }

        public ObjectiveChecker1D(IObjectiveFunction1D objective, Action<IEvaluation1D> valueChecker, Action<IEvaluation1D> gradientChecker, Action<IEvaluation1D> hessianChecker)
        {
            InnerObjective = objective;
            ValueChecker = valueChecker;
            DerivativeChecker = gradientChecker;
            SecondDerivativeChecker = hessianChecker;
        }

        public bool DerivativeSupported
        {
            get { return InnerObjective.DerivativeSupported; }
        }

        public bool SecondDerivativeSupported
        {
            get { return InnerObjective.SecondDerivativeSupported; }
        }

        public IEvaluation1D Evaluate(double point)
        {
            try
            {
                return new CheckedEvaluation1D(this, InnerObjective.Evaluate(point));
            }
            catch (Exception e)
            {
                throw new EvaluationException("Objective evaluation failed.", new NullEvaluation(point), e);
            }
        }
    }
}
