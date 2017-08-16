﻿// <copyright file="ObjectiveFunction1D.cs" company="Math.NET">
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
    internal class ScalarValueObjectiveFunctionEvaluation : IScalarObjectiveFunctionEvaluation
    {
        public ScalarValueObjectiveFunctionEvaluation(double point, double value)
        {
            Point = point;
            Value = value;
        }

        public double Point { get; }
        public double Value { get; }

        public double Derivative
        {
            get { throw new NotSupportedException(); }
        }

        public double SecondDerivative
        {
            get { throw new NotSupportedException(); }
        }
    }

    internal class ScalarValueObjectiveFunction : IScalarObjectiveFunction
    {
        public Func<double, double> Objective { get; private set; }

        public ScalarValueObjectiveFunction(Func<double, double> objective)
        {
            Objective = objective;
        }

        public bool IsDerivativeSupported
        {
            get { return false; }
        }

        public bool IsSecondDerivativeSupported
        {
            get { return false; }
        }

        public IScalarObjectiveFunctionEvaluation Evaluate(double point)
        {
            return new ScalarValueObjectiveFunctionEvaluation(point, Objective(point));
        }
    }
}
