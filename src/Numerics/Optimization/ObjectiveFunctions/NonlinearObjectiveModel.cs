using MathNet.Numerics.Differentiation;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Optimization.ObjectiveFunctions
{
    /// <summary>
    /// Nonlinear objective model for optimization problems.
    /// Can be initialized in two ways:
    /// 1. With a model modelFunction f(x;p) and observed data (x,y) for curve fitting
    /// 2. With a direct residual modelFunction R(p) for general minimization problems
    /// </summary>
    internal class NonlinearObjectiveModel : IObjectiveModel
    {
        #region Private Variables

        /// <summary>
        /// The model modelFunction: f(x; p) that maps x to y given parameters p
        /// Null if using direct residual modelFunction mode
        /// </summary>
        readonly Func<Vector<double>, Vector<double>, Vector<double>> _modelFunction; // (p, x) => f(x; p)

        /// <summary>
        /// The derivative of model modelFunction with respect to parameters
        /// Null if using direct residual modelFunction mode or if derivative not provided
        /// </summary>
        readonly Func<Vector<double>, Vector<double>, Matrix<double>> _modelDerivative; // (p, x) => df(x; p)/dp

        /// <summary>
        /// The direct residual modelFunction: R(p) that calculates residuals directly from parameters
        /// Null if using model modelFunction mode
        /// </summary>
        readonly Func<Vector<double>, Vector<double>> _residualFunction; // p => R(p)

        /// <summary>
        /// The Jacobian of the direct residual modelFunction
        /// Null if using model modelFunction mode or if Jacobian not provided
        /// </summary>
        readonly Func<Vector<double>, Matrix<double>> _residualJacobian; // p => dR(p)/dp

        /// <summary>
        /// Flag indicating whether we're using direct residual modelFunction mode
        /// </summary>
        readonly bool _useDirectResiduals;

        readonly int _accuracyOrder; // the desired accuracy order to evaluate the jacobian by numerical approximaiton.

        Vector<double> _coefficients;

        bool _hasFunctionValue;
        double _functionValue; // the residual sum of squares, residuals * residuals.
        Vector<double> _residuals; // the weighted error values

        bool _hasJacobianValue;
        Matrix<double> _jacobianValue; // the Jacobian matrix.
        Vector<double> _gradientValue; // the Gradient vector.
        Matrix<double> _hessianValue; // the Hessian matrix.

        /// <summary>
        /// Number of observations for direct residual mode
        /// </summary>
        readonly int? _observationCount;

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
        /// For direct residual mode, returns the explicitly provided observation count.
        /// If not provided and residuals are available, uses the residual vector length.
        /// </summary>
        public int NumberOfObservations
        {
            get
            {
                if (_useDirectResiduals)
                {
                    // If observation count was explicitly provided, use it
                    if (_observationCount.HasValue)
                        return _observationCount.Value;

                    // Otherwise, if we have calculated residuals, use their count
                    if (_residuals != null)
                        return _residuals.Count;

                    // If neither is available, return 0
                    return 0;
                }
                else
                {
                    return ObservedY?.Count ?? 0;
                }
            }
        }

        /// <summary>
        /// Get the number of unknown parameters.
        /// </summary>
        public int NumberOfParameters => Point?.Count ?? 0;

        /// <inheritdoc/>
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
        /// Get the number of calls to modelFunction.
        /// </summary>
        public int FunctionEvaluations { get; set; }

        /// <summary>
        /// Get the number of calls to jacobian.
        /// </summary>
        public int JacobianEvaluations { get; set; }

        #endregion Public Variables

        /// <summary>
        /// Initializes a new instance using a model modelFunction f(x;p) for curve fitting
        /// </summary>
        /// <param name="modelFunction">The model modelFunction f(x;p) that predicts y values</param>
        /// <param name="derivative">Optional derivative modelFunction of the model</param>
        /// <param name="accuracyOrder">Accuracy order for numerical differentiation (1-6)</param>
        public NonlinearObjectiveModel(
            Func<Vector<double>, Vector<double>, Vector<double>> modelFunction,
            Func<Vector<double>, Vector<double>, Matrix<double>> derivative = null,
            int accuracyOrder = 2)
        {
            _modelFunction = modelFunction;
            _modelDerivative = derivative;
            _useDirectResiduals = false;
            _accuracyOrder = Math.Min(6, Math.Max(1, accuracyOrder));
        }

        /// <summary>
        /// Initializes a new instance using a direct residual function R(p)
        /// </summary>
        /// <param name="residualFunction">Function that directly calculates residuals from parameters</param>
        /// <param name="jacobian">Optional Jacobian of residual function</param>
        /// <param name="observationCount">Number of observations for degree of freedom calculation. If not provided, 
        /// will use the length of residual vector, which may not be appropriate for all statistical calculations.</param>
        /// <param name="accuracyOrder">Accuracy order for numerical differentiation (1-6)</param>
        public NonlinearObjectiveModel(
            Func<Vector<double>, Vector<double>> residualFunction,
            Func<Vector<double>, Matrix<double>> jacobian = null,
            int? observationCount = null,
            int accuracyOrder = 2)
        {
            _residualFunction = residualFunction ?? throw new ArgumentNullException(nameof(residualFunction));
            _residualJacobian = jacobian;
            _useDirectResiduals = true;
            _accuracyOrder = Math.Min(6, Math.Max(1, accuracyOrder));
            _observationCount = observationCount;
        }

        /// <inheritdoc/>
        public IObjectiveModel Fork()
        {
            if (_useDirectResiduals)
            {
                return new NonlinearObjectiveModel(_residualFunction, _residualJacobian, _observationCount, _accuracyOrder)
                {
                    _coefficients = _coefficients,
                    _hasFunctionValue = _hasFunctionValue,
                    _functionValue = _functionValue,
                    _residuals = _residuals,
                    _hasJacobianValue = _hasJacobianValue,
                    _jacobianValue = _jacobianValue,
                    _gradientValue = _gradientValue,
                    _hessianValue = _hessianValue,
                    IsFixed = IsFixed
                };
            }
            else
            {
                return new NonlinearObjectiveModel(_modelFunction, _modelDerivative, _accuracyOrder)
                {
                    ObservedX = ObservedX,
                    ObservedY = ObservedY,
                    Weights = Weights,
                    L = L,
                    _coefficients = _coefficients,
                    _hasFunctionValue = _hasFunctionValue,
                    _functionValue = _functionValue,
                    _residuals = _residuals,
                    _hasJacobianValue = _hasJacobianValue,
                    _jacobianValue = _jacobianValue,
                    _gradientValue = _gradientValue,
                    _hessianValue = _hessianValue,
                    IsFixed = IsFixed
                };
            }
        }

        /// <inheritdoc/>
        public IObjectiveModel CreateNew()
        {
            if (_useDirectResiduals)
            {
                return new NonlinearObjectiveModel(_residualFunction, _residualJacobian, _observationCount, _accuracyOrder)
                {
                    IsFixed = IsFixed
                };
            }
            else
            {
                return new NonlinearObjectiveModel(_modelFunction, _modelDerivative, _accuracyOrder)
                {
                    ObservedX = ObservedX,
                    ObservedY = ObservedY,
                    Weights = Weights,
                    L = L,                                        
                    IsFixed = IsFixed
                };
            }
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
        /// Get the residual values at the current parameters.
        /// </summary>
        public Vector<double> Residuals
        {
            get
            {
                if (!_hasFunctionValue)
                {
                    EvaluateFunction();
                    _hasFunctionValue = true;
                }
                return _residuals;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool IsGradientSupported => true;

        /// <inheritdoc/>
        public bool IsHessianSupported => true;

        /// <summary>
        /// Set observed data to fit.
        /// Only applicable when using model function mode.
        /// </summary>
        public void SetObserved(Vector<double> observedX, Vector<double> observedY, Vector<double> weights = null)
        {
            if (_useDirectResiduals)
            {
                throw new InvalidOperationException("Cannot set observed data when using direct residual function mode.");
            }

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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void EvaluateAt(Vector<double> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (parameters.Any(p => double.IsNaN(p) || double.IsInfinity(p)))
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

        /// <inheritdoc/>
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

        /// <summary>
        /// Evaluates the objective function at the current parameter values.
        /// </summary>
        void EvaluateFunction()
        {
            if (_coefficients == null)
            {
                throw new InvalidOperationException("Cannot evaluate function: current parameters is not set.");
            }

            if (_useDirectResiduals)
            {
                // Direct residual mode: calculate residuals directly from parameters
                _residuals = _residualFunction(Point);
                FunctionEvaluations++;
            }
            else
            {
                // Model function mode: calculate residuals from model predictions and observed data
                if (ModelValues == null)
                {
                    ModelValues = Vector<double>.Build.Dense(NumberOfObservations);
                }

                ModelValues = _modelFunction(Point, ObservedX);
                FunctionEvaluations++;

                // calculate the weighted residuals
                _residuals = (Weights == null)
                    ? ObservedY - ModelValues
                    : (ObservedY - ModelValues).PointwiseMultiply(L);
            }

            // Calculate the residual sum of squares with 1/2 factor
            // F(p) = 1/2 * ∑(residuals²)
            _functionValue = 0.5 * _residuals.DotProduct(_residuals);
        }

        /// <summary>
        /// Evaluates the Jacobian matrix, gradient vector, and approximated Hessian matrix at the current parameters.
        /// For direct residual mode, gradient is J'R where J is the Jacobian and R is the residual vector.
        /// For model function mode, gradient is -J'R since residuals are defined as (observed - predicted).
        /// </summary>
        void EvaluateJacobian()
        {
            var evaluations = 0;

            if (_coefficients == null)
            {
                throw new InvalidOperationException("Cannot evaluate Jacobian: current parameters is not set.");
            }

            if (_useDirectResiduals)
            {
                // Direct residual mode: use provided Jacobian or calculate numerically
                if (_residualJacobian != null)
                {
                    _jacobianValue = _residualJacobian(Point);
                    JacobianEvaluations++;
                }
                else
                {
                    // Calculate Jacobian numerically for residual function
                    _jacobianValue = NumericalJacobianForResidual(Point, out evaluations);
                    FunctionEvaluations += evaluations;
                }
            }
            else
            {
                // Model function mode: use provided derivative or calculate numerically
                if (_modelDerivative != null)
                {
                    // analytical jacobian
                    _jacobianValue = _modelDerivative(Point, ObservedX);
                    JacobianEvaluations++;
                }
                else
                {
                    // numerical jacobian
                    _jacobianValue = NumericalJacobian(Point, out evaluations);
                    FunctionEvaluations += evaluations;
                }

                // Apply weights to jacobian in model function mode
                if (Weights != null)
                {
                    for (var i = 0; i < NumberOfObservations; i++)
                    {
                        for (var j = 0; j < NumberOfParameters; j++)
                        {
                            _jacobianValue[i, j] = _jacobianValue[i, j] * L[i];
                        }
                    }
                }
            }

            // Apply fixed parameters to jacobian
            if (IsFixed != null)
            {
                for (var j = 0; j < NumberOfParameters; j++)
                {
                    if (IsFixed[j])
                    {
                        // if j-th parameter is fixed, set J[i, j] = 0
                        for (var i = 0; i < _jacobianValue.RowCount; i++)
                        {
                            _jacobianValue[i, j] = 0.0;
                        }
                    }
                }
            }

            // Gradient calculation with sign dependent on mode
            if (_useDirectResiduals)
            {
                // For direct residual mode: g = J'R
                // When using a direct residual function R(p), the gradient is J'R
                _gradientValue = _jacobianValue.Transpose() * _residuals;
            }
            else
            {
                // For model function mode: g = -J'R
                // When using a model function with residuals defined as (observed - predicted),
                // the gradient includes a negative sign
                _gradientValue = -_jacobianValue.Transpose() * _residuals;
            }

            // approximated Hessian, H = J'J + ∑Ri∇²Ri ~ J'J near the minimum
            _hessianValue = _jacobianValue.Transpose() * _jacobianValue;
        }

        /// <summary>
        /// Calculates the Jacobian matrix using numerical differentiation with finite differences.
        /// The accuracy order determines which finite difference formula to use.
        /// </summary>
        /// <param name="parameters">The current parameter values</param>
        /// <param name="evaluationCount">Returns the number of function evaluations performed</param>
        /// <returns>The Jacobian matrix of partial derivatives df(x;p)/dp</returns>
        Matrix<double> NumericalJacobian(Vector<double> parameters, out int evaluationCount)
        {
            // Get appropriate finite difference configuration based on _accuracyOrder
            var (points, center) = GetFiniteDifferenceConfiguration(_accuracyOrder);

            // Initialize NumericalJacobian with appropriate configuration
            var jacobianCalculator = new NumericalJacobian(points, center);
            var derivatives = Matrix<double>.Build.Dense(NumberOfObservations, NumberOfParameters);
            evaluationCount = 0;

            // Process each observation point separately
            for (var i = 0; i < NumberOfObservations; i++)
            {
                var obsIndex = i; // Capture observation index for the lambda

                // Create adapter function that returns the model value for current observation
                // when given parameters array
                double funcAdapter(double[] p)
                {
                    var paramsVector = Vector<double>.Build.DenseOfArray(p);
                    var modelValues = _modelFunction(paramsVector, ObservedX);
                    return modelValues[obsIndex];
                }

                // Calculate gradient (which is the row of Jacobian for this observation)
                var jacobianRow = jacobianCalculator.Evaluate(funcAdapter, parameters.ToArray());

                // Store results in derivatives matrix
                for (var j = 0; j < NumberOfParameters; j++)
                {
                    derivatives[i, j] = jacobianRow[j];
                }
            }

            // Get total function evaluation count
            evaluationCount = jacobianCalculator.FunctionEvaluations;

            return derivatives;
        }

        /// <summary>
        /// Calculate numerical Jacobian for direct residual function R(p) using finite differences.
        /// The accuracy order determines which finite difference formula to use.
        /// </summary>
        /// <param name="parameters">Current parameter values</param>
        /// <param name="evaluationCount">Returns the number of function evaluations performed</param>
        /// <returns>Jacobian matrix of partial derivatives dR(p)/dp</returns>
        Matrix<double> NumericalJacobianForResidual(Vector<double> parameters, out int evaluationCount)
        {
            // Get current residuals
            var residuals = _residualFunction(parameters);
            var residualSize = residuals.Count;

            // Get appropriate finite difference configuration based on _accuracyOrder
            var (points, center) = GetFiniteDifferenceConfiguration(_accuracyOrder);

            var derivatives = Matrix<double>.Build.Dense(residualSize, NumberOfParameters);
            evaluationCount = 0;
            var totalEvaluations = 0;

            // Process each residual component separately
            for (var i = 0; i < residualSize; i++)
            {
                var resIndex = i; // Capture residual index for the lambda

                // Create adapter function that returns the residual component for the current index
                // when given parameters array
                double funcAdapter(double[] p)
                {
                    var paramsVector = Vector<double>.Build.DenseOfArray(p);
                    var resValues = _residualFunction(paramsVector);
                    return resValues[resIndex];
                }

                // Calculate gradient (which is the row of Jacobian for this residual component)
                var jacobianCalculator = new NumericalJacobian(points, center);
                var jacobianRow = jacobianCalculator.Evaluate(funcAdapter, parameters.ToArray());
                totalEvaluations += jacobianCalculator.FunctionEvaluations;

                // Store results in derivatives matrix
                for (var j = 0; j < NumberOfParameters; j++)
                {
                    derivatives[i, j] = jacobianRow[j];
                }
            }

            // Set the total evaluation count
            evaluationCount = totalEvaluations;

            return derivatives;
        }

        /// <summary>
        /// Returns appropriate finite difference configuration based on accuracy order.
        /// </summary>
        /// <param name="accuracyOrder">Accuracy order (1-6)</param>
        /// <returns>Tuple of (points count, center position)</returns>
        private static (int points, int center) GetFiniteDifferenceConfiguration(int accuracyOrder)
        {
            switch (accuracyOrder)
            {
                case 1:
                    // 1st order accuracy: 2-point forward difference
                    return (2, 0);
                case 2:
                    // 2nd order accuracy: 3-point central difference
                    return (3, 1);
                case 3:
                    // 3rd order accuracy: 4-point difference
                    return (4, 1);  // Asymmetric central difference
                case 4:
                    // 4th order accuracy: 5-point central difference
                    return (5, 2);
                case 5:
                    // 5th order accuracy: 6-point difference
                    return (6, 2);  // Asymmetric central difference
                default:
                case 6:
                    // 6th order accuracy: 7-point central difference
                    return (7, 3);
            }
        }

        #endregion Private Methods
    }
}
