using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.ObjectiveFunctions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Optimization.ObjectiveModels
{
    internal class FittingObjectiveModel : IObjectiveModel
    {
        #region Private Variables

        readonly Func<Vector<double>, double, double> userFunction; // (p, x) => f(x; p)
        readonly Func<Vector<double>, double, Vector<double>> userDerivatives; // (p, x) => df(x; p)/dp
        readonly int accuracyOrder; // the desired accuracy order to evaluate the jacobian by numerical approximaiton.

        Vector<double> coefficients;
        Vector<double> Pint; // internal(unbounded) coefficients
        public Vector<double> Pext; // external(bounded) coefficients

        bool hasFunctionValue;
        double functionValue; // the residual sum of squares. Residuals * Residuals
        Vector<double> residuals; // the error values

        bool hasJacobianValue;
        Matrix<double> jacobianValue; // the Jacobian matrix.
        Vector<double> gradientValue; // the Gradient vector.
        Matrix<double> hessianValue; // the Hessian matrix.
        
        bool isBounded;

        #endregion Private Variables

        #region Public Variables - Observed Data

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
        private Vector<double> L; // Weights = LL'

        /// <summary>
        /// Get the number of observations.
        /// </summary>
        public int NumberOfObservations { get { return (ObservedY == null) ? 0 : ObservedY.Count; } }

        #endregion Public Variables - Observed Data

        #region Public Variables - Bounds of Parameter

        /// <summary>
        /// Get the values of the parameters.
        /// </summary>
        public List<bool> IsFixed { get; private set; }

        /// <summary>
        /// Get the values of the parameters.
        /// </summary>
        public Vector<double> LowerBound { get; private set; }

        /// <summary>
        /// Get the values of the parameters.
        /// </summary>
        public Vector<double> UpperBound { get; private set; }

        /// <summary>
        /// Get the scale factor of the parameters.
        /// </summary>
        public Vector<double> Scales { get; private set; }

        /// <summary>
        /// Get the number of unknown parameters.
        /// </summary>
        public int NumberOfParameters { get { return (Point == null) ? 0 : Point.Count; } }

        #endregion Public Variables - Bounds of Parameter

        #region Public Variables - Others

        /// <summary>
        /// Get the number of calls to function.
        /// </summary>
        public int FunctionEvaluations { get; set; }
        /// <summary>
        /// Get the number of calls to jacobian.
        /// </summary>
        public int JacobianEvaluations { get; set; }

        #endregion Public Variables - Others

        public FittingObjectiveModel(Func<Vector<double>, double, double>function, Func<Vector<double>, double, Vector<double>> derivatives = null, int accuracyOrder = 2)
        {
            this.userFunction = function;
            this.userDerivatives = derivatives;
            this.accuracyOrder = Math.Min(6, Math.Max(1, accuracyOrder));

            IsFinished = false;
        }

        public IObjectiveModel Fork()
        {
            return new FittingObjectiveModel(userFunction, userDerivatives, accuracyOrder)
            {
                ObservedX = ObservedX,
                ObservedY = ObservedY,
                Weights = Weights,

                coefficients = coefficients,
                Pint = Pint,
                Pext = Pext,

                hasFunctionValue = hasFunctionValue,
                functionValue = functionValue,

                hasJacobianValue = hasJacobianValue,
                jacobianValue = jacobianValue,
                gradientValue = gradientValue,
                hessianValue = hessianValue
            };
        }

        public IObjectiveModel CreateNew()
        {
            return new FittingObjectiveModel(userFunction, userDerivatives, accuracyOrder);
        }

        /// <summary>
        /// Set or get the values of the parameters.
        /// </summary>
        public Vector<double> Point { get { return coefficients; } }

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
                if (!hasFunctionValue)
                {
                    EvaluateFunction();
                    hasFunctionValue = true;
                }
                return functionValue;
            }
        }

        /// <summary>
        /// Get the Gradient vector of x and p.
        /// </summary>
        public Vector<double> Gradient
        {
            get
            {
                if (!hasJacobianValue)
                {
                    EvaluateJacobian();
                    hasJacobianValue = true;
                }
                return gradientValue;
            }
        }

        /// <summary>
        /// Get the Hessian matrix of x and p, J'WJ
        /// </summary>
        public Matrix<double> Hessian
        {
            get
            {
                if (!hasJacobianValue)
                {
                    EvaluateJacobian();
                    hasJacobianValue = true;
                }
                return hessianValue;
            }
        }

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
                    df = df + IsFixed.Count(p => p == true);
                }
                return df;
            }
        }

        public bool IsGradientSupported { get { return true; } }
        public bool IsHessianSupported { get { return true; } }

        public bool IsFinished { get; set; }

        public IObjectiveFunction ToObjectiveFunction()
        {
            Tuple<double, Vector<double>, Matrix<double>> function(Vector<double> point)
            {
                EvaluateAt(point);

                return new Tuple<double, Vector<double>, Matrix<double>>(Value, Gradient, Hessian);
            }

            var objective = new GradientHessianObjectiveFunction(function);
            return objective;
        }

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
        /// <param name="lowerBound">The lower bounds of parameters.</param>
        /// <param name="upperBound">The upper bounds of parameters.</param>
        /// <param name="scales">The scaling constants of parameters</param>
        /// <param name="isFixed">The list to the parameters fix or free.</param>
        public void SetParameters(Vector<double> initialGuess, Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null, List<bool> isFixed = null)
        {
            if (initialGuess == null)
            {
                throw new ArgumentNullException("initialGuess");
            }
            coefficients = initialGuess;

            if (lowerBound != null && lowerBound.Count(x => double.IsInfinity(x) || double.IsNaN(x)) > 0)
            {
                throw new ArgumentException("The lower bounds must be finite.");
            }
            if (lowerBound != null && lowerBound.Count != initialGuess.Count)
            {
                throw new ArgumentException("The upper bounds can't have different elements from the initial guess.");
            }
            LowerBound = lowerBound;

            if (upperBound != null && upperBound.Count(x => double.IsInfinity(x) || double.IsNaN(x)) > 0)
            {
                throw new ArgumentException("The upper bounds must be finite.");
            }
            if (upperBound != null && upperBound.Count != initialGuess.Count)
            {
                throw new ArgumentException("The upper bounds can't have different elements from the initial guess.");
            }
            UpperBound = upperBound;

            if (scales != null && scales.Count(x => double.IsInfinity(x) || double.IsNaN(x) || x == 0) > 0)
            {
                throw new ArgumentException("The scales must be finite.");
            }
            if (scales != null && scales.Count != initialGuess.Count)
            {
                throw new ArgumentException("The upper bounds can't have different elements from the initial guess.");
            }
            if (scales != null && scales.Count(x => x < 0) > 0)
            {
                scales.PointwiseAbs();
            }
            Scales = scales;

            if (isFixed != null && isFixed.Count != initialGuess.Count)
            {
                throw new ArgumentException("The isFixed can't have different elements from the initial guess.");
            }
            if (isFixed != null && isFixed.Count(p => p == true) == isFixed.Count)
            {
                throw new ArgumentException("All the parameters can't be fixed.");
            }
            IsFixed = isFixed;

            isBounded = LowerBound != null || UpperBound != null || Scales != null;
        }

        public void EvaluateAt(Vector<double> parameters)
        {
            ValidateParameters(parameters);

            // To handle the box constrained minimization as the unconstrained minimization,
            // the parameters are mapping by the following rules,
            // which are modified the rules shown in the ref[1] in order to introduce scales.
            //
            // 1. lower < Pext < upper
            //    Pint = asin(2 * (Pext - lower) / (upper - lower) - 1)
            //    Pext = lower + (sin(Pint) + 1) * (upper - lower) / 2
            //    dPext/dPint = (upper - lower) / 2 * cos(Pint)
            //
            // 2. lower < Pext
            //    Pint = sqrt((Pext/scale - lower/scale + 1)^2 - 1)
            //    Pext = lower + scale * (sqrt(Pint^2 + 1) - 1)
            //    dPext/dPint = scale * Pint / sqrt(Pint^2 + 1)
            //
            // 3. Pext < upper
            //    Pint = sqrt((upper / scale - Pext / scale + 1)^2 - 1)
            //    Pext = upper + scale - scale * sqrt(Pint^2 + 1)
            //    dPext/dPint = - scale * Pint / sqrt(Pint^2 + 1)
            //
            // 4. no bounds, but scales
            //    Pint = Pext / scale
            //    Pext = Pint * scale
            //    dPext/dPint = scale
            //
            // The rules are applied in ProjectParametersToInternal, ProjectParametersToExternal, and ScaleFactorsOfJacobian methods.
            //
            // References:
            // [1] https://lmfit.github.io/lmfit-py/bounds.html
            // [2] MINUIT User's Guide, https://root.cern.ch/download/minuit.pdf
            //
            // Except when it is initial guess, the parameters argument is always internal parameter.
            // So, first map the parameters argument to the external parameters in order to calculate function values.

            Pext = (FunctionEvaluations > 0 && isBounded)
                ? ProjectParametersToExternal(parameters)
                : parameters.Clone();
            Pint = (isBounded)
                ? ProjectParametersToInternal(Pext)
                : Pext;

            this.coefficients = Pint;

            if (IsFinished)
            {
                this.coefficients = Pext;
            }

            hasFunctionValue = false;
            hasJacobianValue = false;

            // don't keep references unnecessarily
            jacobianValue = null;
            gradientValue = null;
            hessianValue = null;
        }

        #region Private Methods

        private void EvaluateFunction()
        {
            // Calculates the residuals, (y[i] - f(x[i]; p)) * L[i]
            if (ModelValues == null)
            {
                ModelValues = Vector<double>.Build.Dense(NumberOfObservations);
            }
            for (int i = 0; i < NumberOfObservations; i++)
            {
                ModelValues[i] = userFunction(Pext, ObservedX[i]);
            }
            FunctionEvaluations++;

            // calculate the weighted residuals
            residuals = (Weights == null)
                ? ObservedY - ModelValues
                : (ObservedY - ModelValues).PointwiseMultiply(L);

            // Calculate the residual sum of squares
            functionValue = residuals.DotProduct(residuals);

            return;
        }

        private void EvaluateJacobian()
        {
            // Calculates the jacobian of x and p.
            if (userDerivatives != null)
            {
                // analytical jacobian
                if (jacobianValue == null)
                {
                    jacobianValue = Matrix<double>.Build.Dense(NumberOfObservations, NumberOfParameters);
                }
                for (int i = 0; i < NumberOfObservations; i++)
                {
                    jacobianValue.SetRow(i, userDerivatives(Pext, ObservedX[i]));
                }
                JacobianEvaluations++;
            }
            else
            {
                // numerical jacobian
                jacobianValue = NumericalJacobian(Pext, ModelValues, accuracyOrder);
                FunctionEvaluations += accuracyOrder;
            }

            var scaleFactors = (isBounded && !IsFinished)
               ? ScaleFactorsOfJacobian(Pint)
               : Vector<double>.Build.Dense(Pint.Count, 1.0);

            // project jacobian: Jint(x; Pint) = Jext(x; Pext) * scale where scale = dPext/dPint
            for (int i = 0; i < NumberOfObservations; i++)
            {
                for (int j = 0; j < NumberOfParameters; j++)
                {
                    if (IsFixed != null && IsFixed[j])
                    {
                        // if j-th parameter is fixed, set J[i, j] = 0
                        jacobianValue[i, j] = 0.0;
                    }
                    else
                    {
                        jacobianValue[i, j] = jacobianValue[i, j] * scaleFactors[j];
                    }
                }
            }

            // Gradient, g = -J'W(y − f(x; p)) = -J'L(L'E) = -J'LR
            gradientValue = (Weights == null)
                ? -jacobianValue.Transpose() * (ObservedY - ModelValues)
                : -jacobianValue.Transpose() * Weights * (ObservedY - ModelValues);

            // approximated Hessian, H = J'WJ + ∑LRiHi ~ J'WJ near the minimum
            hessianValue = (Weights == null)
                ? jacobianValue.Transpose() * jacobianValue
                : jacobianValue.Transpose() * Weights * jacobianValue;
        }

        private void ValidateParameters(Vector<double> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }
            else if (parameters.Count(p => double.IsNaN(p) || double.IsInfinity(p)) > 0)
            {
                throw new ArgumentException("the parameters must be finite.");
            }
            if (LowerBound != null && parameters.Count != LowerBound.Count)
            {
                throw new ArgumentException("The parameters can't have different size from the lower bounds.");
            }
            if (UpperBound != null && parameters.Count != UpperBound.Count)
            {
                throw new ArgumentException("The parameters can't have different size from the upper bounds.");
            }
            if (Scales != null && parameters.Count != Scales.Count)
            {
                throw new ArgumentException("The parameters can't have different size from the scales.");
            }
            if (IsFixed != null && parameters.Count != IsFixed.Count)
            {
                throw new ArgumentException("The parameters can't have different size from the IsFixed list.");
            }
        }

        private Matrix<double> NumericalJacobian(Vector<double> Pext, Vector<double> currentValues, int accuracyOrder = 2)
        {   
            const double sqrtEpsilon = 1.4901161193847656250E-8; // sqrt(machineEpsilon)
            
            Matrix<double> derivertives = Matrix<double>.Build.Dense(NumberOfObservations, NumberOfParameters);

            var d = 0.000003 * Pext.PointwiseAbs().PointwiseMaximum(sqrtEpsilon);

            var h = Vector<double>.Build.Dense(NumberOfParameters);
            for (int i = 0; i < NumberOfObservations; i++)
            {
                var x = ObservedX[i];
                for (int j = 0; j < NumberOfParameters; j++)
                {
                    h[j] = d[j];

                    if (accuracyOrder >= 6)
                    {
                        // f'(x) = {- f(x - 3h) + 9f(x - 2h) - 45f(x - h) + 45f(x + h) - 9f(x + 2h) + f(x + 3h)} / 60h + O(h^6)
                        var f1 = userFunction(Pext - 3 * h, x);
                        var f2 = userFunction(Pext - 2 * h, x);
                        var f3 = userFunction(Pext - h, x);
                        var f4 = userFunction(Pext + h, x);
                        var f5 = userFunction(Pext + 2 * h, x);
                        var f6 = userFunction(Pext + 3 * h, x);

                        var prime = (-f1 + 9 * f2 - 45 * f3 + 45 * f4 - 9 * f5 + f6) / (60 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else if (accuracyOrder == 5)
                    {
                        // f'(x) = {-137f(x) + 300f(x + h) - 300f(x + 2h) + 200f(x + 3h) - 75f(x + 4h) + 12f(x + 5h)} / 60h + O(h^5)
                        var f1 = currentValues[i];
                        var f2 = userFunction(Pext + h, x);
                        var f3 = userFunction(Pext + 2 * h, x);
                        var f4 = userFunction(Pext + 3 * h, x);
                        var f5 = userFunction(Pext + 4 * h, x);
                        var f6 = userFunction(Pext + 5 * h, x);

                        var prime = (-137 * f1 + 300 * f2 - 300 * f3 + 200 * f4 - 75 * f5 + 12 * f6) / (60 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else if (accuracyOrder == 4)
                    {
                        // f'(x) = {f(x - 2h) - 8f(x - h) + 8f(x + h) - f(x + 2h)} / 12h + O(h^4)
                        var f1 = userFunction(Pext - 2 * h, x);
                        var f2 = userFunction(Pext - h, x);
                        var f3 = userFunction(Pext + h, x);
                        var f4 = userFunction(Pext + 2 * h, x);

                        var prime = (f1 - 8 * f2 + 8 * f3 - f4) / (12 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else if (accuracyOrder == 3)
                    {
                        // f'(x) = {-11f(x) + 18f(x + h) - 9f(x + 2h) + 2f(x + 3h)} / 6h + O(h^3)
                        var f1 = currentValues[i];
                        var f2 = userFunction(Pext + h, x);
                        var f3 = userFunction(Pext + 2 * h, x);
                        var f4 = userFunction(Pext + 3 * h, x);

                        var prime = (-11 * f1 + 18 * f2 - 9 * f3 + 2 * f4) / (6 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else if (accuracyOrder == 2)
                    {
                        // f'(x) = {f(x + h) - f(x - h)} / 2h + O(h^2)
                        var f1 = userFunction(Pext + h, x);
                        var f2 = userFunction(Pext - h, x);

                        var prime = (f1 - f2) / (2 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else
                    {
                        // f'(x) = {- f(x) + f(x + h)} / h + O(h)
                        var f1 = currentValues[i];
                        var f2 = userFunction(Pext + h, x);

                        var prime = (-f1 + f2) / h[j];
                        derivertives[i, j] = prime;
                    }

                    h[j] = 0;
                }
            }

            return derivertives;
        }
        
        private Vector<double> ProjectParametersToInternal(Vector<double> Pext)
        {
            var Pint = Pext.Clone();

            if (LowerBound != null && UpperBound != null)
            {
                for (int i = 0; i < Pext.Count; i++)
                {
                    Pint[i] = Math.Asin((2.0 * (Pext[i] - LowerBound[i]) / (UpperBound[i] - LowerBound[i])) - 1.0);
                }

                return Pint;
            }
            else if (LowerBound != null && UpperBound == null)
            {
                for (int i = 0; i < Pext.Count; i++)
                {
                    Pint[i] = (Scales == null)
                        ? Math.Sqrt(Math.Pow(Pext[i] - LowerBound[i] + 1.0, 2) - 1.0)
                        : Math.Sqrt(Math.Pow((Pext[i] - LowerBound[i]) / Scales[i] + 1.0, 2) - 1.0);
                }

                return Pint;
            }
            else if (LowerBound == null && UpperBound != null)
            {
                for (int i = 0; i < Pext.Count; i++)
                {
                    Pint[i] = (Scales == null)
                        ? Math.Sqrt(Math.Pow(UpperBound[i] - Pext[i] + 1.0, 2) - 1.0)
                        : Math.Sqrt(Math.Pow((UpperBound[i] - Pext[i]) / Scales[i] + 1.0, 2) - 1.0);
                }

                return Pint;
            }
            else if (Scales != null)
            {
                for (int i = 0; i < Pext.Count; i++)
                {
                    Pint[i] = Pext[i] / Scales[i];
                }

                return Pint;
            }

            return Pint;
        }

        private Vector<double> ProjectParametersToExternal(Vector<double> Pint)
        {
            var Pext = Pint.Clone();

            if (LowerBound != null && UpperBound != null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    Pext[i] = LowerBound[i] + (UpperBound[i] / 2.0 - LowerBound[i] / 2.0) * (Math.Sin(Pint[i]) + 1.0);
                }

                return Pext;
            }
            else if (LowerBound != null && UpperBound == null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    Pext[i] = (Scales == null)
                        ? LowerBound[i] + Math.Sqrt(Pint[i] * Pint[i] + 1.0) - 1.0
                        : LowerBound[i] + Scales[i] * (Math.Sqrt(Pint[i] * Pint[i] + 1.0) - 1.0);
                }

                return Pext;
            }
            else if (LowerBound == null && UpperBound != null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    Pext[i] = (Scales == null)
                        ? UpperBound[i] - Math.Sqrt(Pint[i] * Pint[i] + 1.0) + 1.0
                        : UpperBound[i] - Scales[i] * (Math.Sqrt(Pint[i] * Pint[i] + 1.0) - 1.0);
                }

                return Pext;
            }
            else if (Scales != null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    Pext[i] = Pint[i] * Scales[i];
                }

                return Pext;
            }

            return Pext;
        }

        private Vector<double> ScaleFactorsOfJacobian(Vector<double> Pint)
        {
            var scale = Vector<double>.Build.Dense(Pint.Count, 1.0);

            if (LowerBound != null && UpperBound != null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    scale[i] = (UpperBound[i] - LowerBound[i]) / 2.0 * Math.Cos(Pint[i]);
                }
                return scale;
            }
            else if (LowerBound != null && UpperBound == null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    scale[i] = (Scales == null)
                        ? Pint[i] / Math.Sqrt(Pint[i] * Pint[i] + 1.0)
                        : Scales[i] * Pint[i] / Math.Sqrt(Pint[i] * Pint[i] + 1.0);
                }
                return scale;
            }
            else if (LowerBound == null && UpperBound != null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    scale[i] = (Scales == null)
                        ? -Pint[i] / Math.Sqrt(Pint[i] * Pint[i] + 1.0)
                        : -Scales[i] * Pint[i] / Math.Sqrt(Pint[i] * Pint[i] + 1.0);
                }
                return scale;
            }
            else if (Scales != null)
            {
                return Scales;
            }

            return scale;
        }

        #endregion Private Methods
    }
}
