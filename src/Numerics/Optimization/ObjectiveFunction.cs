// <copyright file="ObjectiveFunction.cs" company="Math.NET">
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
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.Optimization
{
    [Flags]
    public enum EvaluationStatus
    {
        None = 0,
        Value = 1,
        Gradient = 2,
        Hessian = 4
    }

    public interface IEvaluation
    {
        Vector<double> Point { get; }
        EvaluationStatus Status { get; }
        double Value { get; }
        Vector<double> Gradient { get; }
        Matrix<double> Hessian { get; }
    }

    public interface IObjectiveFunction
    {
        bool GradientSupported { get; }
        bool HessianSupported { get; }
        IEvaluation Evaluate(Vector<double> point);
    }

    public abstract class BaseEvaluation : IEvaluation
    {
        protected EvaluationStatus _status;
        protected double? _value;
        protected Vector<double> _gradient;
        protected Matrix<double> _hessian;
        protected Vector<double> _point;

        public Vector<double> Point
        {
            get { return _point; }
        }

        protected BaseEvaluation()
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

        public Vector<double> Gradient
        {
            get
            {
                if (_gradient == null)
                {
                    SetGradient();
                    _status |= EvaluationStatus.Gradient;
                }
                return _gradient;
            }
        }

        public Matrix<double> Hessian
        {
            get
            {
                if (_hessian == null)
                {
                    SetHessian();
                    _status |= EvaluationStatus.Hessian;
                }
                return _hessian;
            }
        }

        protected abstract void SetValue();
        protected abstract void SetGradient();
        protected abstract void SetHessian();
    }


    public class NullEvaluation : BaseEvaluation
    {

        public NullEvaluation(Vector<double> point)
        {
            _point = point;
            _status = EvaluationStatus.None;
        }

        public NullEvaluation(double point)
        {
            _point = DenseVector.Create(1, point);
            _status = EvaluationStatus.None;
        }

        protected override void SetValue()
        {
            throw new NotImplementedException();
        }

        protected override void SetGradient()
        {
            throw new NotImplementedException();
        }

        protected override void SetHessian()
        {
            throw new NotImplementedException();
        }
    }

    public class OneDEvaluationExpander : IEvaluation
    {
        public IEvaluation1D InnerEval { get; private set; }

        public OneDEvaluationExpander(IEvaluation1D eval)
        {
            InnerEval = eval;
        }


        public Vector<double> Point
        {
            get { return DenseVector.Create(1, InnerEval.Point); }
        }

        public EvaluationStatus Status
        {
            get { return InnerEval.Status; }
        }

        public double Value
        {
            get { return InnerEval.Value; }
        }

        public Vector<double> Gradient
        {
            get { return DenseVector.Create(1, InnerEval.Derivative); }
        }

        public Matrix<double> Hessian
        {
            get { return DenseMatrix.Create(1, 1, InnerEval.SecondDerivative); }
        }
    }

    public class CachedEvaluation : BaseEvaluation
    {
        readonly SimpleObjectiveFunction _objectiveObject;

        public CachedEvaluation(SimpleObjectiveFunction f, Vector<double> point)
        {
            _objectiveObject = f;
            _point = point;
        }

        protected override void SetValue()
        {
            _value = _objectiveObject.Objective(_point);
        }

        protected override void SetGradient()
        {
            _gradient = _objectiveObject.Gradient(_point);
        }

        protected override void SetHessian()
        {
            _hessian = _objectiveObject.Hessian(_point);
        }
    }

    public class SimpleObjectiveFunction : IObjectiveFunction
    {
        public Func<Vector<double>, double> Objective { get; private set; }
        public Func<Vector<double>, Vector<double>> Gradient { get; private set; }
        public Func<Vector<double>, Matrix<double>> Hessian { get; private set; }

        public SimpleObjectiveFunction(Func<Vector<double>, double> objective)
        {
            Objective = objective;
            Gradient = null;
            Hessian = null;
        }

        public SimpleObjectiveFunction(Func<Vector<double>, double> objective, Func<Vector<double>, Vector<double>> gradient)
        {
            Objective = objective;
            Gradient = gradient;
            Hessian = null;
        }

        public SimpleObjectiveFunction(Func<Vector<double>, double> objective, Func<Vector<double>, Vector<double>> gradient, Func<Vector<double>, Matrix<double>> hessian)
        {
            Objective = objective;
            Gradient = gradient;
            Hessian = hessian;
        }

        public bool GradientSupported
        {
            get { return Gradient != null; }
        }

        public bool HessianSupported
        {
            get { return Hessian != null; }
        }

        public IEvaluation Evaluate(Vector<double> point)
        {
            return new CachedEvaluation(this, point);
        }
    }
}
