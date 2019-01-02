using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization.ObjectiveFunctions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathNet.Numerics.Optimization.ObjectiveModels
{
    internal class FittingObjectiveModel : IObjectiveModel
    {
        readonly Func<Vector<double>, double, double> userFunction; // (p, x) => f(x; p)
        readonly Func<Vector<double>, double, Vector<double>> userDerivatives; // (p, x) => df(x; p)/dp

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
        /// inverse of the standard measurement errors
        /// If null, unity weighting is used.
        /// </summary>
        public Matrix<double> Weights { get; private set; }
        // W = LL'
        private Vector<double> L;

        /// <summary>
        /// Set or get the values of the parameters.
        /// </summary>
        public Vector<double> Parameters { get; private set; }

        /// <summary>
        /// Set or get the values of the parameters.
        /// </summary>
        public List<bool> IsFixed { get; set; }

        /// <summary>
        /// Set or get the values of the parameters.
        /// </summary>
        public Vector<double> LowerBound { get; set; }

        /// <summary>
        /// Set or get the values of the parameters.
        /// </summary>
        public Vector<double> UpperBound { get; set; }

        /// <summary>
        /// Set or get the scale factor of the parameters.
        /// </summary>
        public Vector<double> Scales { get; set; }

        /// <summary>
        /// Set of get whether or not the parameters are bounded. 
        /// </summary>
        public bool IsBounded { get; set; }

        /// <summary>
        /// Get the y-values of the fitted model that correspond to the independent values.
        /// </summary>
        public Vector<double> Values { get; private set; }

        /// <summary>
        /// Get the error values, R(x; p) = L * (y - f(x; p)) where L = sqrt(W)
        /// </summary>
        private Vector<double> Residuals;

        /// <summary>
        /// Get the residual sum of squares, R.DotProduct(R)
        /// </summary>
        public double Residue { get; private set; }

        /// <summary>
        /// Get the Jacobian matrix of x and p, J(x; p).
        /// </summary>
        public Matrix<double> Jacobian { get; private set; }
        
        /// <summary>
        /// Get the Gradient vector of x and p, J'WR
        /// </summary>
        public Vector<double> Gradient { get; private set; }

        /// <summary>
        /// Get the Hessian matrix of x and p, J'WJ
        /// </summary>
        public Matrix<double> Hessian { get; private set; }

        /// <summary>
        /// Get the number of observations.
        /// </summary>
        public int NumberOfObservations { get { return (ObservedY == null) ? 0 : ObservedY.Count; } }

        /// <summary>
        /// Get the number of unknown parameters.
        /// </summary>
        public int NumberOfParameters { get { return (Parameters == null) ? 0 : Parameters.Count; } }

        /// <summary>
        /// Get the degree of freedom
        /// </summary>
        public int DegreeOfFreedom
        {
            get
            {
                var dof = NumberOfObservations - NumberOfParameters;
                if (IsFixed != null)
                {
                    dof = dof + IsFixed.Count(p => p == true);
                }
                return dof;
            }
        }

        /// <summary>
        /// Get the covariance matrix.
        /// </summary>
        public Matrix<double> Covariance { get; private set; }

        /// <summary>
        /// Get the number of calls to function.
        /// </summary>
        public int FunctionEvaluations { get; set; }
        /// <summary>
        /// Get the number of calls to jacobian.
        /// </summary>
        public int JacobianEvaluations { get; set; }

        /// <summary>
        /// Set or get the desired accuracy order of the numerical jacobian.
        /// </summary>
        public int AccuracyOrder { get; set; }

        /// <summary>
        /// Get whether or not the analytical jacobian is supported.
        /// </summary>
        public bool IsJacobianSupported {  get { return userDerivatives != null; } }

        #endregion Public Variables

        public FittingObjectiveModel(Func<Vector<double>, double, double>function, Func<Vector<double>, double, Vector<double>> derivatives, int accuracyOrder = 2)
        {
            userFunction = function;
            userDerivatives = derivatives;
            AccuracyOrder = Math.Min(6, Math.Max(1, accuracyOrder));
        }

        public IObjectiveModel Fork()
        {
            return new FittingObjectiveModel(userFunction, userDerivatives, AccuracyOrder)
            {
                ObservedX = ObservedX,
                ObservedY = ObservedY,
                Weights = Weights,

                Parameters = Parameters,
                LowerBound = LowerBound,
                UpperBound = UpperBound,
                IsFixed = IsFixed,
                Scales = Scales,
                IsBounded = IsBounded,

                Residue = Residue,
                Jacobian = Jacobian
            };
        }

        public IObjectiveModel CreateNew()
        {
            return new FittingObjectiveModel(userFunction, userDerivatives, AccuracyOrder);
        }

        public IObjectiveFunction ToObjectiveFunction()
        {
            Tuple<double, Vector<double>, Matrix<double>> function(Vector<double> point)
            {
                EvaluateFunction(point);
                EvaluateJacobian(point);

                return new Tuple<double, Vector<double>, Matrix<double>>(Residue, -Gradient, Hessian);
            }

            LowerBound = null;
            UpperBound = null;
            Scales = null;
            IsFixed = null;
            IsBounded = false;

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
        /// Set observed data to fit.
        /// </summary>
        public void SetObserved(double[] observedX, double[] observedY, double[] weights = null)
        {
            if (observedX == null || observedY == null)
            {
                throw new ArgumentNullException("The data set can't be null.");
            }
            if (observedX.Length != observedY.Length)
            {
                throw new ArgumentException("The observed x data can't have different from observed y data.");
            }

            var wVector = (weights == null)
                ? null
                : Vector<double>.Build.DenseOfArray(weights);
            SetObserved(Vector<double>.Build.DenseOfArray(observedX), Vector<double>.Build.DenseOfArray(observedY), wVector);
        }

        /// <summary>
        /// Set parameters.
        /// <para/>
        /// If bounded, the paramneters will be projected to unconstrained range by the mapping rule from the MINPACK.
        /// If the projection is not needed, set IsBounded = false befre calling the Minimization method.
        /// </summary>
        /// <param name="lowerBound">The lower bounds of parameters.</param>
        /// <param name="upperBound">The upper bounds of parameters.</param>
        /// /// <param name="scales">The scaling constants of parameters</param>
        /// <param name="isFixed">The list to the parameters fix or free.</param>
        public void SetParameters(Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null, List<bool> isFixed = null)
        {
            if (lowerBound != null && lowerBound.Count(x => double.IsInfinity(x) || double.IsNaN(x)) > 0)
            {
                throw new ArgumentException("The lower bounds must be finite.");
            }            
            LowerBound = lowerBound;

            if (upperBound != null && upperBound.Count(x => double.IsInfinity(x) || double.IsNaN(x)) > 0)
            {
                throw new ArgumentException("The upper bounds must be finite.");
            }
            if (upperBound != null && lowerBound != null && upperBound.Count != lowerBound.Count)
            {
                throw new ArgumentException("The upper bounds can't have different elements from the lower bounds.");
            }
            UpperBound = upperBound;

            if (scales != null && scales.Count(x => double.IsInfinity(x) || double.IsNaN(x) || x == 0) > 0)
            {
                throw new ArgumentException("The scales must be finite.");
            }
            if (scales != null && lowerBound != null && scales.Count != lowerBound.Count)
            {
                throw new ArgumentException("The upper bounds can't have different elements from the lower bounds.");
            }
            if (scales != null && upperBound != null && scales.Count != upperBound.Count)
            {
                throw new ArgumentException("The upper bounds can't have different elements from the upper bounds.");
            }
            if (scales != null && scales.Count(x => x < 0) > 0)
            {
                scales.PointwiseAbs();
            }
            Scales = scales;

            IsBounded = (LowerBound != null || UpperBound != null || Scales != null);

            if (isFixed != null && lowerBound != null && isFixed.Count != lowerBound.Count)
            {
                throw new ArgumentException("The initial guess can't have different elements from the lower bounds.");
            }
            if (isFixed != null && upperBound != null && isFixed.Count != upperBound.Count)
            {
                throw new ArgumentException("The initial guess can't have different elements from the upper bounds.");
            }
            if (isFixed != null && scales != null && isFixed.Count != scales.Count)
            {
                throw new ArgumentException("The initial guess can't have different elements from the scales.");
            }
            if (isFixed != null && isFixed.Count(p => p == true) == isFixed.Count)
            {
                throw new ArgumentException("All the parameters can't be fixed.");
            }
            IsFixed = isFixed;
        }

        /// <summary>
        /// Set parameters.
        /// <para/>
        /// If bounded, the paramneters will be projected to unconstrained range by the mapping rule.
        /// If the projection is not needed, set IsBounded = false befre calling the Minimization method.
        /// </summary>
        /// <param name="lowerBound">The lower bounds of parameters.</param>
        /// <param name="upperBound">The upper bounds of parameters.</param>
        /// <param name="scales">The scaling constants of parameters</param>
        /// <param name="isFixed">The list to the parameters fix or free.</param>
        public void SetParameters(double[] lowerBound = null, double[] upperBound = null, double[] scales = null, bool[] isFixed = null)
        {
            var lb = (lowerBound == null) ? null : Vector<double>.Build.DenseOfArray(lowerBound);
            var ub = (upperBound == null) ? null : Vector<double>.Build.DenseOfArray(upperBound);
            var sc = (scales == null) ? null : Vector<double>.Build.DenseOfArray(scales);
            var fp = (isFixed == null) ? null : isFixed.ToList();

            SetParameters(lb, ub, sc, fp);
        }

        public void EvaluateFunction(Vector<double> parameters)
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
            var Pext = (FunctionEvaluations > 0 && this.IsBounded)
                    ? ProjectParametersToExternal(parameters)
                    : parameters.Clone();

            // Project parameters, now this.Parameters are the internal parameters.
            Parameters = (this.IsBounded)
                ? ProjectParametersToInternal(Pext)
                : Pext;

            // Calculates the residuals, (y[i] - f(x[i]; p)) * L[i]
            if (Values == null)
            {
                Values = Vector<double>.Build.Dense(NumberOfObservations);
            }
            for (int i = 0; i < NumberOfObservations; i++)
            {
                Values[i] = userFunction(Pext, ObservedX[i]);
            }
            FunctionEvaluations++;

            // calculate the weighted residuals
            Residuals = (Weights == null)
                ? ObservedY - Values
                : (ObservedY - Values).PointwiseMultiply(L);

            // Calculate the residual sum of squares
            Residue = Residuals.DotProduct(Residuals);            

            return;
        }

        public void EvaluateJacobian(Vector<double> parameters)
        {
            var Pext = (IsBounded)
                ? ProjectParametersToExternal(parameters)
                : parameters.Clone();

            // Calculates the jacobian of x and p.
            if (userDerivatives != null)
            {
                // analytical jacobian
                if (Jacobian == null)
                {
                    Jacobian = Matrix<double>.Build.Dense(NumberOfObservations, NumberOfParameters);
                }
                for (int i = 0; i < NumberOfObservations; i++)
                {
                    Jacobian.SetRow(i, userDerivatives(Pext, ObservedX[i]));
                }
                JacobianEvaluations++;
            }
            else
            {
                // numerical jacobian
                Jacobian = NumericalJacobian(Pext, Values, AccuracyOrder);
                FunctionEvaluations += AccuracyOrder;
            }

            var scaleFactors = (this.IsBounded)
               ? ScaleFactorsOfJacobian(Parameters)
               : Vector<double>.Build.Dense(Parameters.Count, 1.0);

            // Jint(x; Pint) = Jext(x; Pext) * scale where scale = dPext/dPint
            for (int i = 0; i < NumberOfObservations; i++)
            {
                for (int j = 0; j < NumberOfParameters; j++)
                {
                    if (IsFixed != null && IsFixed[j])
                    {
                        // if j-th parameter is fixed, set J[i, j] = 0
                        Jacobian[i, j] = 0.0;
                    }
                    else
                    {
                        Jacobian[i, j] = Jacobian[i, j] * scaleFactors[j];
                    }
                }
            }

            // Gradient, g = J'W(y − f(x; p)) = J'L(L'E) = J'LR
            Gradient = (Weights == null)
                ? Jacobian.Transpose() * (ObservedY - Values)
                : Jacobian.Transpose() * Weights * (ObservedY - Values);

            // approximated Hessian, H = J'WJ + ∑LRiHi ~ J'WJ near the minimum
            Hessian = (Weights == null)
                ? Jacobian.Transpose() * Jacobian
                : Jacobian.Transpose() * Weights * Jacobian;
        }

        public void EvaluateCovariance(Vector<double> parameters)
        {
            // convert to bounded(external) parameters
            var Pext = (IsBounded)
                    ? ProjectParametersToExternal(parameters)
                    : parameters.Clone();

            // set IsBounded = false to get external Parameters and covariance matrix
            this.IsBounded = false;

            EvaluateFunction(Pext);
            EvaluateJacobian(Pext);

            if (Hessian == null || Residuals == null || DegreeOfFreedom < 1)
            {
                Covariance = null;
                return;
            }

            var covariance = Hessian.PseudoInverse() * Residuals.DotProduct(Residuals) / DegreeOfFreedom;

            Covariance = covariance;

            // restore isBounded
            this.IsBounded = (LowerBound != null || UpperBound != null);

            return;
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

        #region Numerical Derivatives

        // Numerical derivatives by using the central or forward finite difference 
        private Matrix<double> NumericalJacobian(Vector<double> parameters, Vector<double> currentValues, int accuracyOrder = 2)
        {   
            const double sqrtEpsilon = 1.4901161193847656250E-8; // sqrt(machineEpsilon)
            
            Matrix<double> derivertives = Matrix<double>.Build.Dense(NumberOfObservations, NumberOfParameters);

            var d = 0.000003 * parameters.PointwiseAbs().PointwiseMaximum(sqrtEpsilon);

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
                        var f1 = userFunction(parameters - 3 * h, x);
                        var f2 = userFunction(parameters - 2 * h, x);
                        var f3 = userFunction(parameters - h, x);
                        var f4 = userFunction(parameters + h, x);
                        var f5 = userFunction(parameters + 2 * h, x);
                        var f6 = userFunction(parameters + 3 * h, x);

                        var prime = (-f1 + 9 * f2 - 45 * f3 + 45 * f4 - 9 * f5 + f6) / (60 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else if (accuracyOrder == 5)
                    {
                        // f'(x) = {-137f(x) + 300f(x + h) - 300f(x + 2h) + 200f(x + 3h) - 75f(x + 4h) + 12f(x + 5h)} / 60h + O(h^5)
                        var f1 = currentValues[i];
                        var f2 = userFunction(parameters + h, x);
                        var f3 = userFunction(parameters + 2 * h, x);
                        var f4 = userFunction(parameters + 3 * h, x);
                        var f5 = userFunction(parameters + 4 * h, x);
                        var f6 = userFunction(parameters + 5 * h, x);

                        var prime = (-137 * f1 + 300 * f2 - 300 * f3 + 200 * f4 - 75 * f5 + 12 * f6) / (60 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else if (accuracyOrder == 4)
                    {
                        // f'(x) = {f(x - 2h) - 8f(x - h) + 8f(x + h) - f(x + 2h)} / 12h + O(h^4)
                        var f1 = userFunction(parameters - 2 * h, x);
                        var f2 = userFunction(parameters - h, x);
                        var f3 = userFunction(parameters + h, x);
                        var f4 = userFunction(parameters + 2 * h, x);

                        var prime = (f1 - 8 * f2 + 8 * f3 - f4) / (12 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else if (accuracyOrder == 3)
                    {
                        // f'(x) = {-11f(x) + 18f(x + h) - 9f(x + 2h) + 2f(x + 3h)} / 6h + O(h^3)
                        var f1 = currentValues[i];
                        var f2 = userFunction(parameters + h, x);
                        var f3 = userFunction(parameters + 2 * h, x);
                        var f4 = userFunction(parameters + 3 * h, x);

                        var prime = (-11 * f1 + 18 * f2 - 9 * f3 + 2 * f4) / (6 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else if (accuracyOrder == 2)
                    {
                        // f'(x) = {f(x + h) - f(x - h)} / 2h + O(h^2)
                        var f1 = userFunction(parameters + h, x);
                        var f2 = userFunction(parameters - h, x);

                        var prime = (f1 - f2) / (2 * h[j]);
                        derivertives[i, j] = prime;
                    }
                    else
                    {
                        // f'(x) = {- f(x) + f(x + h)} / h + O(h)
                        var f1 = currentValues[i];
                        var f2 = userFunction(parameters + h, x);

                        var prime = (-f1 + f2) / h[j];
                        derivertives[i, j] = prime;
                    }

                    h[j] = 0;
                }
            }

            return derivertives;
        }
        
        #endregion Numerical Derivatives

        #region Projection

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

        #endregion Projection
    }
}
