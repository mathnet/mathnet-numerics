// <copyright file="ObjectiveFunction1D.cs" company="Math.NET">
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
    public interface IEvaluation1D
    {
        double Point { get; }
        EvaluationStatus Status { get; }
        double Value { get; }
        double Derivative { get; }
        double SecondDerivative { get; }
    }

    public interface IObjectiveFunction1D
    {
        bool DerivativeSupported { get; }
        bool SecondDerivativeSupported { get; }
        IEvaluation1D Evaluate(double point);
    }

    public abstract class BaseEvaluation1D : IEvaluation1D
    {
        protected double _point;
        protected EvaluationStatus _status;
        protected double? _value;
        protected double? _derivative;
        protected double? _secondDerivative;

        public double Point
        {
            get { return _point; }
        }

        protected BaseEvaluation1D()
        {
            _status = EvaluationStatus.None;
        }

        public EvaluationStatus Status
        {
            get { return _status; }
        }

        public double Value
        {
            get
            {
                if (!_value.HasValue)
                {
                    SetValue();
                    _status |= EvaluationStatus.Value;
                }
                return _value.Value;
            }
        }

        public double Derivative
        {
            get
            {
                if (_derivative == null)
                {
                    SetDerivative();
                    _status |= EvaluationStatus.Gradient;
                }
                return _derivative.Value;
            }
        }

        public double SecondDerivative
        {
            get
            {
                if (_secondDerivative == null)
                {
                    SetSecondDerivative();
                    _status |= EvaluationStatus.Hessian;
                }
                return _secondDerivative.Value;
            }
        }

        protected abstract void SetValue();
        protected abstract void SetDerivative();
        protected abstract void SetSecondDerivative();
    }

    public class CachedEvaluation1D : BaseEvaluation1D
    {
        readonly SimpleObjectiveFunction1D _objectiveObject;

        public CachedEvaluation1D(SimpleObjectiveFunction1D f, double point)
        {
            _objectiveObject = f;
            _point = point;
        }

        protected override void SetValue()
        {
            _value = _objectiveObject.Objective(_point);
        }

        protected override void SetDerivative()
        {
            _derivative = _objectiveObject.Derivative(_point);
        }

        protected override void SetSecondDerivative()
        {
            _secondDerivative = _objectiveObject.SecondDerivative(_point);
        }
    }

    public class SimpleObjectiveFunction1D : IObjectiveFunction1D
    {
        public Func<double, double> Objective { get; private set; }
        public Func<double, double> Derivative { get; private set; }
        public Func<double, double> SecondDerivative { get; private set; }

        public SimpleObjectiveFunction1D(Func<double, double> objective)
        {
            Objective = objective;
            Derivative = null;
            SecondDerivative = null;
        }

        public SimpleObjectiveFunction1D(Func<double, double> objective, Func<double, double> derivative)
        {
            Objective = objective;
            Derivative = derivative;
            SecondDerivative = null;
        }

        public SimpleObjectiveFunction1D(Func<double, double> objective, Func<double, double> derivative, Func<double, double> secondDerivative)
        {
            Objective = objective;
            Derivative = derivative;
            SecondDerivative = secondDerivative;
        }

        public bool DerivativeSupported
        {
            get { return Derivative != null; }
        }

        public bool SecondDerivativeSupported
        {
            get { return SecondDerivative != null; }
        }

        public IEvaluation1D Evaluate(double point)
        {
            return new CachedEvaluation1D(this, point);
        }
    }
}
