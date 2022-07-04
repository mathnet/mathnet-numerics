using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MathNet.Numerics.Optimization
{
    internal class ProjectableParameters
    {
        private readonly IEnumerable<ProjectableParameter> _parameters;

        internal ProjectableParameters(IEnumerable<double> values, IList<double> lowerBounds, IList<double> upperBounds,
            IList<double> scales)
        {
            _parameters = values.Select((value, index) =>
                new ProjectableParameter(value, lowerBounds?[index], upperBounds?[index], scales?[index]));
        }

        internal Vector<double> ToInternal() =>
            DenseVector.OfEnumerable(_parameters.Select(p => p.ToInternal()));

        internal Vector<double> ToExternal() =>
            DenseVector.OfEnumerable(_parameters.Select(p => p.ToExternal()));

        internal Vector<double> JacobianScaleFactors() =>
            DenseVector.OfEnumerable(_parameters.Select(p => p.JacobianScaleFactor()));
    }


    internal class ProjectableParameter
    {
        private readonly double _value;
        private readonly double _lowerBound;
        private readonly double _upperBound;
        private readonly double _scaleFactor;

        internal ProjectableParameter(double value, double? lowerBound, double? upperBound, double? scaleFactor)
        {
            _value = value;
            _lowerBound = lowerBound ?? double.NegativeInfinity;
            _upperBound = upperBound ?? double.PositiveInfinity;
            _scaleFactor = scaleFactor ?? 1;
        }

        internal double ToInternal()
        {
            if (_lowerBound.IsFinite() && _upperBound.IsFinite())
                return Math.Asin(2 * (_value - _lowerBound) / (_upperBound - _lowerBound) - 1);

            if (_lowerBound.IsFinite())
                return Math.Sqrt(Math.Pow((_value - _lowerBound) / _scaleFactor + 1, 2) - 1);

            if (_upperBound.IsFinite())
                return Math.Sqrt(Math.Pow((_upperBound - _value) / _scaleFactor + 1, 2) - 1);

            return _value / _scaleFactor;
        }

        internal double ToExternal()
        {
            if (_lowerBound.IsFinite() && _upperBound.IsFinite())
                return _lowerBound + (_upperBound / 2 - _lowerBound / 2) * (Math.Sin(_value) + 1);

            if (_lowerBound.IsFinite())
                return _lowerBound + _scaleFactor * (Math.Sqrt(_value * _value + 1) - 1);

            if (_upperBound.IsFinite())
                return _upperBound - _scaleFactor * (Math.Sqrt(_value * _value + 1) - 1);

            return _value * _scaleFactor;
        }

        internal double JacobianScaleFactor()
        {
            if (_lowerBound.IsFinite() && _upperBound.IsFinite())
                return (_upperBound - _lowerBound) / 2 * Math.Cos(_value);

            if (_lowerBound.IsFinite())
                return _scaleFactor * _value / Math.Sqrt(_value * _value + 1);

            if (_upperBound.IsFinite())
                return -_scaleFactor * _value / Math.Sqrt(_value * _value + 1);

            return _scaleFactor;
        }
    }
}
