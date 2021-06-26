using MathNet.Numerics.LinearAlgebra;
using System;
using System.Linq;

namespace MathNet.Numerics.Optimization
{
    public abstract class NonlinearMinimizerBase
    {
        /// <summary>
        /// The stopping threshold for the function value or L2 norm of the residuals.
        /// </summary>
        public double FunctionTolerance { get; set; }

        /// <summary>
        /// The stopping threshold for L2 norm of the change of the parameters.
        /// </summary>
        public double StepTolerance { get; set; }

        /// <summary>
        /// The stopping threshold for infinity norm of the gradient.
        /// </summary>
        public double GradientTolerance { get; set; }

        /// <summary>
        /// The maximum number of iterations.
        /// </summary>
        public int MaximumIterations { get; set; }

        /// <summary>
        /// The lower bound of the parameters.
        /// </summary>
        public Vector<double> LowerBound { get; private set; }

        /// <summary>
        /// The upper bound of the parameters.
        /// </summary>
        public Vector<double> UpperBound { get; private set; }

        /// <summary>
        /// The scale factors for the parameters.
        /// </summary>
        public Vector<double> Scales { get; private set; }

        bool IsBounded => LowerBound != null || UpperBound != null || Scales != null;

        protected NonlinearMinimizerBase(double gradientTolerance = 1E-18, double stepTolerance = 1E-18, double functionTolerance = 1E-18, int maximumIterations = -1)
        {
            GradientTolerance = gradientTolerance;
            StepTolerance = stepTolerance;
            FunctionTolerance = functionTolerance;
            MaximumIterations = maximumIterations;
        }

        protected void ValidateBounds(Vector<double> parameters, Vector<double> lowerBound = null, Vector<double> upperBound = null, Vector<double> scales = null)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (lowerBound != null && lowerBound.Count(x => double.IsInfinity(x) || double.IsNaN(x)) > 0)
            {
                throw new ArgumentException("The lower bounds must be finite.");
            }
            if (lowerBound != null && lowerBound.Count != parameters.Count)
            {
                throw new ArgumentException("The lower bounds can't have different size from the parameters.");
            }
            LowerBound = lowerBound;

            if (upperBound != null && upperBound.Count(x => double.IsInfinity(x) || double.IsNaN(x)) > 0)
            {
                throw new ArgumentException("The upper bounds must be finite.");
            }
            if (upperBound != null && upperBound.Count != parameters.Count)
            {
                throw new ArgumentException("The upper bounds can't have different size from the parameters.");
            }
            UpperBound = upperBound;

            if (scales != null && scales.Count(x => double.IsInfinity(x) || double.IsNaN(x) || x == 0) > 0)
            {
                throw new ArgumentException("The scales must be finite.");
            }
            if (scales != null && scales.Count != parameters.Count)
            {
                throw new ArgumentException("The scales can't have different size from the parameters.");
            }
            if (scales != null && scales.Count(x => x < 0) > 0)
            {
                scales.PointwiseAbs();
            }
            Scales = scales;
        }

        protected double EvaluateFunction(IObjectiveModel objective, Vector<double> Pint)
        {
            var Pext = ProjectToExternalParameters(Pint);
            objective.EvaluateAt(Pext);
            return objective.Value;
        }

        protected (Vector<double> Gradient, Matrix<double> Hessian) EvaluateJacobian(IObjectiveModel objective, Vector<double> Pint)
        {
            var gradient = objective.Gradient;
            var hessian = objective.Hessian;

            if (IsBounded)
            {
                var scaleFactors = ScaleFactorsOfJacobian(Pint); // the parameters argument is always internal.

                for (int i = 0; i < gradient.Count; i++)
                {
                    gradient[i] = gradient[i] * scaleFactors[i];
                }

                for (int i = 0; i < hessian.RowCount; i++)
                {
                    for (int j = 0; j < hessian.ColumnCount; j++)
                    {
                        hessian[i, j] = hessian[i, j] * scaleFactors[i] * scaleFactors[j];
                    }
                }
            }

            return (gradient, hessian);
        }

        #region Projection of Parameters

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

        protected Vector<double> ProjectToInternalParameters(Vector<double> Pext)
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

            if (LowerBound != null && UpperBound == null)
            {
                for (int i = 0; i < Pext.Count; i++)
                {
                    Pint[i] = (Scales == null)
                        ? Math.Sqrt(Math.Pow(Pext[i] - LowerBound[i] + 1.0, 2) - 1.0)
                        : Math.Sqrt(Math.Pow((Pext[i] - LowerBound[i]) / Scales[i] + 1.0, 2) - 1.0);
                }

                return Pint;
            }

            if (LowerBound == null && UpperBound != null)
            {
                for (int i = 0; i < Pext.Count; i++)
                {
                    Pint[i] = (Scales == null)
                        ? Math.Sqrt(Math.Pow(UpperBound[i] - Pext[i] + 1.0, 2) - 1.0)
                        : Math.Sqrt(Math.Pow((UpperBound[i] - Pext[i]) / Scales[i] + 1.0, 2) - 1.0);
                }

                return Pint;
            }

            if (Scales != null)
            {
                for (int i = 0; i < Pext.Count; i++)
                {
                    Pint[i] = Pext[i] / Scales[i];
                }

                return Pint;
            }

            return Pint;
        }

        protected Vector<double> ProjectToExternalParameters(Vector<double> Pint)
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

            if (LowerBound != null && UpperBound == null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    Pext[i] = (Scales == null)
                        ? LowerBound[i] + Math.Sqrt(Pint[i] * Pint[i] + 1.0) - 1.0
                        : LowerBound[i] + Scales[i] * (Math.Sqrt(Pint[i] * Pint[i] + 1.0) - 1.0);
                }

                return Pext;
            }

            if (LowerBound == null && UpperBound != null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    Pext[i] = (Scales == null)
                        ? UpperBound[i] - Math.Sqrt(Pint[i] * Pint[i] + 1.0) + 1.0
                        : UpperBound[i] - Scales[i] * (Math.Sqrt(Pint[i] * Pint[i] + 1.0) - 1.0);
                }

                return Pext;
            }

            if (Scales != null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    Pext[i] = Pint[i] * Scales[i];
                }

                return Pext;
            }

            return Pext;
        }

        protected Vector<double> ScaleFactorsOfJacobian(Vector<double> Pint)
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

            if (LowerBound != null && UpperBound == null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    scale[i] = (Scales == null)
                        ? Pint[i] / Math.Sqrt(Pint[i] * Pint[i] + 1.0)
                        : Scales[i] * Pint[i] / Math.Sqrt(Pint[i] * Pint[i] + 1.0);
                }
                return scale;
            }

            if (LowerBound == null && UpperBound != null)
            {
                for (int i = 0; i < Pint.Count; i++)
                {
                    scale[i] = (Scales == null)
                        ? -Pint[i] / Math.Sqrt(Pint[i] * Pint[i] + 1.0)
                        : -Scales[i] * Pint[i] / Math.Sqrt(Pint[i] * Pint[i] + 1.0);
                }
                return scale;
            }

            if (Scales != null)
            {
                return Scales;
            }

            return scale;
        }

        #endregion Projection of Parameters
    }
}
