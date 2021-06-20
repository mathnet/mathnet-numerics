using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    internal class NonlinearObjectiveFunction : IObjectiveModel
    {
        #region Private Variables

        readonly Func<Vector<double>, Vector<double>, Vector<double>> _userFunction; // (p, x) => f(x; p)
        readonly Func<Vector<double>, Vector<double>, Matrix<double>> _userDerivative; // (p, x) => df(x; p)/dp
        readonly int _accuracyOrder; // the desired accuracy order to evaluate the jacobian by numerical approximaiton.

        Vector<double> _coefficients;

        bool _hasFunctionValue;
        double _functionValue; // the residual sum of squares, residuals * residuals.
        Vector<double> _residuals; // the weighted error values

        bool _hasJacobianValue;
        Matrix<double> _jacobianValue; // the Jacobian matrix.
        Vector<double> _gradientValue; // the Gradient vector.
        Matrix<double> _hessianValue; // the Hessian matrix.

        #endregion Private Variables

        #region Public Variables

        /// <summary>
        /// Set or get the values of the independent variable.
        /// </summary>
        public Vector<double> ObservedX { get; private set; }

        /// <summary>
        /// Set or get the values of the observations.
        /// </summary>
        public Vector<double> ObservedY { get; private set; }

        /// <summary>
        /// Set or get the values of the weights for the observations.
        /// </summary>
        public Matrix<double> Weights { get; private set; }
        Vector<double> L; // Weights = LL'

        /// <summary>
        /// Get whether parameters are fixed or free.
        /// </summary>
        public List<bool> IsFixed { get; private set; }

        /// <summary>
        /// Get the number of observations.
        /// </summary>
        public int NumberOfObservations => ObservedY?.Count ?? 0;

        /// <summary>
        /// Get the number of unknown parameters.
        /// </summary>
        public int NumberOfParameters => Point?.Count ?? 0;

        /// <summary>
        /// Get the degree of freedom
        /// </summary>
        public int DegreeOfFreedom
        {
            get
            {
                var df = NumberOfObservations - NumberOfParameters;
                if (IsFixed != null)
                {
                    df = df + IsFixed.Count(p => p);
                }
                return df;
            }
        }

        /// <summary>
        /// Get the number of calls to function.
        /// </summary>
        public int FunctionEvaluations { get; set; }

        /// <summary>
        /// Get the number of calls to jacobian.
        /// </summary>
        public int JacobianEvaluations { get; set; }

        #endregion Public Variables

        public NonlinearObjectiveFunction(Func<Vector<double>, Vector<double>, Vector<double>> function,
            Func<Vector<double>, Vector<double>, Matrix<double>> derivative = null, int accuracyOrder = 2)
        {
            _userFunction = function;
            _userDerivative = derivative;
            _accuracyOrder = Math.Min(6, Math.Max(1, accuracyOrder));
        }

        public IObjectiveModel Fork()
        {
            return new NonlinearObjectiveFunction(_userFunction, _userDerivative, _accuracyOrder)
            {
                ObservedX = ObservedX,
                ObservedY = ObservedY,
                Weights = Weights,

                _coefficients = _coefficients,

                _hasFunctionValue = _hasFunctionValue,
                _functionValue = _functionValue,

                _hasJacobianValue = _hasJacobianValue,
                _jacobianValue = _jacobianValue,
                _gradientValue = _gradientValue,
                _hessianValue = _hessianValue
            };
        }

        public IObjectiveModel CreateNew()
        {
            return new NonlinearObjectiveFunction(_userFunction, _userDerivative, _accuracyOrder);
        }

        /// <summary>
        /// Set or get the values of the parameters.
        /// </summary>
        public Vector<double> Point => _coefficients;

        /// <summary>
        /// Get the y-values of the fitted model that correspond to the independent values.
        /// </summary>
        public Vector<double> ModelValues { get; private set; }

        /// <summary>
        /// Get the residual sum of squares.
        /// </summary>
        public double Value
        {
            get
            {
                if (!_hasFunctionValue)
                {
                    EvaluateFunction();
                    _hasFunctionValue = true;
                }
                return _functionValue;
            }
        }

        /// <summary>
        /// Get the Gradient vector of x and p.
        /// </summary>
        public Vector<double> Gradient
        {
            get
            {
                if (!_hasJacobianValue)
                {
                    EvaluateJacobian();
                    _hasJacobianValue = true;
                }
                return _gradientValue;
            }
        }

        /// <summary>
        /// Get the Hessian matrix of x and p, J'WJ
        /// </summary>
        public Matrix<double> Hessian
        {
            get
            {
                if (!_hasJacobianValue)
                {
                    EvaluateJacobian();
                    _hasJacobianValue = true;
                }
                return _hessianValue;
            }
        }

        public bool IsGradientSupported => true;
        public bool IsHessianSupported => true;

        /// <summary>
        /// Set observed data to fit.
        /// </summary>
        public void SetObserved(Vector<double> observedX, Vector<double> observedY, Vector<double> weights = null)
        {
            if (observedX == null || observedY == null)
            {
                throw new ArgumentNullException("The data set can't be null.");
            }
            if (observedX.Count != observedY.Count)
            {
                throw new ArgumentException("The observed x data can't have different from observed y data.");
            }
            ObservedX = observedX;
            ObservedY = observedY;

            if (weights != null && weights.Count != observedY.Count)
            {
                throw new ArgumentException("The weightings can't have different from observations.");
            }
            if (weights != null && weights.Count(x => double.IsInfinity(x) || double.IsNaN(x)) > 0)
            {
                throw new ArgumentException("The weightings are not well-defined.");
            }
            if (weights != null && weights.Count(x => x == 0) == weights.Count)
            {
                throw new ArgumentException("All the weightings can't be zero.");
            }
            if (weights != null && weights.Count(x => x < 0) > 0)
            {
                weights = weights.PointwiseAbs();
            }

            Weights = (weights == null)
                    ? null
                    : Matrix<double>.Build.DenseOfDiagonalVector(weights);

            L = (weights == null)
                ? null
                : Weights.Diagonal().PointwiseSqrt();
        }

        /// <summary>
        /// Set parameters and bounds.
        /// </summary>
        /// <param name="initialGuess">The initial values of parameters.</param>
        /// <param name="isFixed">The list to the parameters fix or free.</param>
        public void SetParameters(Vector<double> initialGuess, List<bool> isFixed = null)
        {
            _coefficients = initialGuess ?? throw new ArgumentNullException(nameof(initialGuess));

            if (isFixed != null && isFixed.Count != initialGuess.Count)
            {
                throw new ArgumentException("The isFixed can't have different size from the initial guess.");
            }
            if (isFixed != null && isFixed.Count(p => p) == isFixed.Count)
            {
                throw new ArgumentException("All the parameters can't be fixed.");
            }
            IsFixed = isFixed;
        }

        public void EvaluateAt(Vector<double> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (parameters.Count(p => double.IsNaN(p) || double.IsInfinity(p)) > 0)
            {
                throw new ArgumentException("The parameters must be finite.");
            }

            _coefficients = parameters;
            _hasFunctionValue = false;
            _hasJacobianValue = false;

            _jacobianValue = null;
            _gradientValue = null;
            _hessianValue = null;
        }

        public IObjectiveFunction ToObjectiveFunction()
        {
            (double, Vector<double>, Matrix<double>) Function(Vector<double> point)
            {
                EvaluateAt(point);
                return (Value, Gradient, Hessian);
            }

            var objective = new GradientHessianObjectiveFunction(Function);
            return objective;
        }

        #region Private Methods

        void EvaluateFunction()
        {
            // Calculates the residuals, (y[i] - f(x[i]; p)) * L[i]
            if (ModelValues == null)
            {
                ModelValues = Vector<double>.Build.Dense(NumberOfObservations);
            }
            ModelValues = _userFunction(Point, ObservedX);
            FunctionEvaluations++;

            // calculate the weighted residuals
            _residuals = (Weights == null)
                ? ObservedY - ModelValues
                : (ObservedY - ModelValues).PointwiseMultiply(L);

            // Calculate the residual sum of squares
            _functionValue = _residuals.DotProduct(_residuals);
        }

        void EvaluateJacobian()
        {
            // Calculates the jacobian of x and p.
            if (_userDerivative != null)
            {
                // analytical jacobian
                _jacobianValue = _userDerivative(Point, ObservedX);
                JacobianEvaluations++;
            }
            else
            {
                // numerical jacobian
                _jacobianValue = NumericalJacobian(Point, ModelValues, _accuracyOrder);
                FunctionEvaluations += _accuracyOrder;
            }

            // weighted jacobian
            for (int i = 0; i < NumberOfObservations; i++)
            {
                for (int j = 0; j < NumberOfParameters; j++)
                {
                    if (IsFixed != null && IsFixed[j])
                    {
                        // if j-th parameter is fixed, set J[i, j] = 0
                        _jacobianValue[i, j] = 0.0;
                    }
                    else if (Weights != null)
                    {
                        _jacobianValue[i, j] = _jacobianValue[i, j] * L[i];
                    }
                }
            }

            // Gradient, g = -J'W(y − f(x; p)) = -J'L(L'E) = -J'LR
            _gradientValue = -_jacobianValue.Transpose() * _residuals;

            // approximated Hessian, H = J'WJ + ∑LRiHi ~ J'WJ near the minimum
            _hessianValue = _jacobianValue.Transpose() * _jacobianValue;
        }

        Matrix<double> NumericalJacobian(Vector<double> parameters, Vector<double> currentValues, int accuracyOrder = 2)
        {
            const double sqrtEpsilon = 1.4901161193847656250E-8; // sqrt(machineEpsilon)

            Matrix<double> derivertives = Matrix<double>.Build.Dense(NumberOfObservations, NumberOfParameters);

            var d = 0.000003 * parameters.PointwiseAbs().PointwiseMaximum(sqrtEpsilon);

            var h = Vector<double>.Build.Dense(NumberOfParameters);
            for (int j = 0; j < NumberOfParameters; j++)
            {
                h[j] = d[j];

                if (accuracyOrder >= 6)
                {
                    // f'(x) = {- f(x - 3h) + 9f(x - 2h) - 45f(x - h) + 45f(x + h) - 9f(x + 2h) + f(x + 3h)} / 60h + O(h^6)
                    var f1 = _userFunction(parameters - 3 * h, ObservedX);
                    var f2 = _userFunction(parameters - 2 * h, ObservedX);
                    var f3 = _userFunction(parameters - h, ObservedX);
                    var f4 = _userFunction(parameters + h, ObservedX);
                    var f5 = _userFunction(parameters + 2 * h, ObservedX);
                    var f6 = _userFunction(parameters + 3 * h, ObservedX);

                    var prime = (-f1 + 9 * f2 - 45 * f3 + 45 * f4 - 9 * f5 + f6) / (60 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else if (accuracyOrder == 5)
                {
                    // f'(x) = {-137f(x) + 300f(x + h) - 300f(x + 2h) + 200f(x + 3h) - 75f(x + 4h) + 12f(x + 5h)} / 60h + O(h^5)
                    var f1 = currentValues;
                    var f2 = _userFunction(parameters + h, ObservedX);
                    var f3 = _userFunction(parameters + 2 * h, ObservedX);
                    var f4 = _userFunction(parameters + 3 * h, ObservedX);
                    var f5 = _userFunction(parameters + 4 * h, ObservedX);
                    var f6 = _userFunction(parameters + 5 * h, ObservedX);

                    var prime = (-137 * f1 + 300 * f2 - 300 * f3 + 200 * f4 - 75 * f5 + 12 * f6) / (60 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else if (accuracyOrder == 4)
                {
                    // f'(x) = {f(x - 2h) - 8f(x - h) + 8f(x + h) - f(x + 2h)} / 12h + O(h^4)
                    var f1 = _userFunction(parameters - 2 * h, ObservedX);
                    var f2 = _userFunction(parameters - h, ObservedX);
                    var f3 = _userFunction(parameters + h, ObservedX);
                    var f4 = _userFunction(parameters + 2 * h, ObservedX);

                    var prime = (f1 - 8 * f2 + 8 * f3 - f4) / (12 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else if (accuracyOrder == 3)
                {
                    // f'(x) = {-11f(x) + 18f(x + h) - 9f(x + 2h) + 2f(x + 3h)} / 6h + O(h^3)
                    var f1 = currentValues;
                    var f2 = _userFunction(parameters + h, ObservedX);
                    var f3 = _userFunction(parameters + 2 * h, ObservedX);
                    var f4 = _userFunction(parameters + 3 * h, ObservedX);

                    var prime = (-11 * f1 + 18 * f2 - 9 * f3 + 2 * f4) / (6 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else if (accuracyOrder == 2)
                {
                    // f'(x) = {f(x + h) - f(x - h)} / 2h + O(h^2)
                    var f1 = _userFunction(parameters + h, ObservedX);
                    var f2 = _userFunction(parameters - h, ObservedX);

                    var prime = (f1 - f2) / (2 * h[j]);
                    derivertives.SetColumn(j, prime);
                }
                else
                {
                    // f'(x) = {- f(x) + f(x + h)} / h + O(h)
                    var f1 = currentValues;
                    var f2 = _userFunction(parameters + h, ObservedX);

                    var prime = (-f1 + f2) / h[j];
                    derivertives.SetColumn(j, prime);
                }

                h[j] = 0;
            }

            return derivertives;
        }

        #endregion Private Methods
    }
}
