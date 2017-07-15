// <copyright file="IObjectiveFunction.cs" company="Math.NET">
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

using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Objective function with a frozen evaluation that must not be changed from the outside.
    /// </summary>
    public interface IObjectiveFunctionEvaluation
    {
        /// <summary>Create a new unevaluated and independent copy of this objective function</summary>
        IObjectiveFunction CreateNew();

        Vector<double> Point { get; }
        double Value { get; }

        bool IsGradientSupported { get; }
        Vector<double> Gradient { get; }

        bool IsHessianSupported { get; }
        Matrix<double> Hessian { get; }
    }

    /// <summary>
    /// Objective function with a mutable evaluation.
    /// </summary>
    public interface IObjectiveFunction : IObjectiveFunctionEvaluation
    {
        void EvaluateAt(Vector<double> point);

        /// <summary>Create a new independent copy of this objective function, evaluated at the same point.</summary>
        IObjectiveFunction Fork();
    }

    public interface IScalarObjectiveFunctionEvaluation
    {
        double Point { get; }
        double Value { get; }
        double Derivative { get; }
        double SecondDerivative { get; }
    }

    public interface IScalarObjectiveFunction
    {
        bool IsDerivativeSupported { get; }
        bool IsSecondDerivativeSupported { get; }
        IScalarObjectiveFunctionEvaluation Evaluate(double point);
    }
}
