// <copyright file="Exceptions.cs" company="Math.NET">
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

namespace MathNet.Numerics.Optimization
{
    public class OptimizationException : Exception
    {
        public OptimizationException(string message)
            : base(message) { }

        public OptimizationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class MaximumIterationsException : OptimizationException
    {
        public MaximumIterationsException(string message)
            : base(message) { }
    }

    public class EvaluationException : OptimizationException
    {
        public IObjectiveFunctionEvaluation ObjectiveFunction { get; }

        public EvaluationException(string message, IObjectiveFunctionEvaluation eval)
            : base(message)
        {
            ObjectiveFunction = eval;
        }

        public EvaluationException(string message, IObjectiveFunctionEvaluation eval, Exception innerException)
            : base(message, innerException)
        {
            ObjectiveFunction = eval;
        }

    }

    public class InnerOptimizationException : OptimizationException
    {
        public InnerOptimizationException(string message)
            : base(message) { }

        public InnerOptimizationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class IncompatibleObjectiveException : OptimizationException
    {
        public IncompatibleObjectiveException(string message)
            : base(message) { }
    }
}
