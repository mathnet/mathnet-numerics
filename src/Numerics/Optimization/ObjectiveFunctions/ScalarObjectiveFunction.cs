// <copyright file="ObjectiveFunction1D.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2017 Math.NET
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

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    internal class LazyScalarObjectiveFunctionEvaluation : IScalarObjectiveFunctionEvaluation
    {
        double? _value;
        double? _derivative;
        double? _secondDerivative;
        readonly ScalarObjectiveFunction _objectiveObject;
        readonly double _point;

        public LazyScalarObjectiveFunctionEvaluation(ScalarObjectiveFunction f, double point)
        {
            _objectiveObject = f;
            _point = point;
        }

        double SetValue()
        {
            _value = _objectiveObject.Objective(_point);
            return _value.Value;
        }

        double SetDerivative()
        {
            _derivative = _objectiveObject.Derivative(_point);
            return _derivative.Value;
        }

        double SetSecondDerivative()
        {
            _secondDerivative = _objectiveObject.SecondDerivative(_point);
            return _secondDerivative.Value;
        }

        public double Point => _point;
        public double Value => _value ?? SetValue();
        public double Derivative => _derivative ?? SetDerivative();
        public double SecondDerivative => _secondDerivative ?? SetSecondDerivative();
    }

    internal class ScalarObjectiveFunction : IScalarObjectiveFunction
    {
        public Func<double, double> Objective { get; }
        public Func<double, double> Derivative { get; }
        public Func<double, double> SecondDerivative { get; }

        public ScalarObjectiveFunction(Func<double, double> objective)
        {
            Objective = objective;
            Derivative = null;
            SecondDerivative = null;
        }

        public ScalarObjectiveFunction(Func<double, double> objective, Func<double, double> derivative)
        {
            Objective = objective;
            Derivative = derivative;
            SecondDerivative = null;
        }

        public ScalarObjectiveFunction(Func<double, double> objective, Func<double, double> derivative, Func<double,double> secondDerivative)
        {
            Objective = objective;
            Derivative = derivative;
            SecondDerivative = secondDerivative;
        }

        public bool IsDerivativeSupported => Derivative != null;

        public bool IsSecondDerivativeSupported => SecondDerivative != null;

        public IScalarObjectiveFunctionEvaluation Evaluate(double point)
        {
            return new LazyScalarObjectiveFunctionEvaluation(this, point);
        }
    }
}
