// <copyright file="MklOptimizationProvider.cs" company="Math.NET">
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

#if NATIVEMKL

using System;
using MathNet.Numerics.Optimization;

namespace MathNet.Numerics.Providers.Optimization.Mkl
{
    public class MklOptimizationProvider : IOptimizationProvider<double>
    {
        const int TR_SUCCESS = 1501;

        public NonLinearLeastSquaresResult NonLinearLeastSquaresUnboundedMinimize(int residualsLength, double[] initialGuess, LeastSquaresForwardModel function, out double[] parameters, Jacobian jacobianFunction = null, NonLinearLeastSquaresOptions options = null)
        {
            if (options == null) options = new NonLinearLeastSquaresOptions();
            bool analyticJacobian = jacobianFunction != null;
            double[] residuals = new double[residualsLength];
            double[] residualsMinus = new double[residualsLength];
            double[] residualsPlus = new double[residualsLength];
            double[] jacobian = new double[residualsLength * initialGuess.Length];
            parameters = new double[initialGuess.Length];

            double[] eps = new double[6]; // stop criteria
            int i;
            eps[0] = options.Criterion0; eps[1] = options.Criterion1; eps[2] = options.Criterion2;
            eps[3] = options.Criterion3; eps[4] = options.Criterion4; eps[5] = options.TrialStepPrecision;

            for (i = 0; i < initialGuess.Length; i++)
                parameters[i] = initialGuess[i];

            int successful;

            int maxIterations = options.MaximumIterations, maxTrialStepIterations = options.MaximumTrialStepIterations;

            IntPtr solverHandle = IntPtr.Zero;
            IntPtr jacobianHandle = IntPtr.Zero;

            int[] info = new int[6]; // for parameter checking

            double initialStepBound = 0.0;

            double jacobianPrecision = options.JacobianPrecision;

            // zero initial values:
            for (i = 0; i < residuals.Length; i++)
                residuals[i] = 0.0;
            for (i = 0; i < residuals.Length * parameters.Length; i++)
                jacobian[i] = 0.0;

            if (SafeNativeMethods.unbound_nonlinearleastsq_init(ref solverHandle, parameters.Length, residualsLength, parameters, eps, maxIterations, maxTrialStepIterations, initialStepBound) !=
                TR_SUCCESS)
            {
                SafeNativeMethods.FreeBuffers();
                return ErrorResult();
            }

            if (SafeNativeMethods.unbound_nonlinearleastsq_check(ref solverHandle, parameters.Length, residualsLength, jacobian, residuals, eps, info) != TR_SUCCESS)
            {
                SafeNativeMethods.FreeBuffers();
                return ErrorResult();
            }
            else
            {
                if (info[0] != 0 || // Handle invalid
                    info[1] != 0 || // Jacobian array not valid
                    info[2] != 0 || // Parameters array not valid
                    info[3] != 0)   // Eps array not valid
                {
                    SafeNativeMethods.FreeBuffers();
                    return ErrorResult();
                }
            }

            if (SafeNativeMethods.jacobi_init(ref jacobianHandle, parameters.Length, residuals.Length, parameters, jacobian, jacobianPrecision) != TR_SUCCESS)
            {
                SafeNativeMethods.FreeBuffers();
                return ErrorResult();
            }

            int rciRequest = 0;
            successful = 0;
            while (successful == 0)
            {
                if (SafeNativeMethods.unbound_nonlinearleastsq_solve(ref solverHandle, residuals, jacobian, ref rciRequest) != TR_SUCCESS)
                {
                    SafeNativeMethods.FreeBuffers();
                    return ErrorResult();
                }
                if (rciRequest == -1 || rciRequest == -2 || rciRequest == -3 ||
                    rciRequest == -4 || rciRequest == -5 || rciRequest == -6)
                    successful = 1;
                if (rciRequest == 1) // recalculate function to update parameters
                {
                    function(parameters, residuals);
                }
                if (rciRequest == 2)
                {
                    if (analyticJacobian)
                        jacobianFunction(parameters, jacobian);
                    else
                    {
                        // calculate by central differences:
                        int rciRequestJacobian = 0;
                        int jacobianSuccessful = 0;

                        // update Jacobian matrix:
                        while (jacobianSuccessful == 0)
                        {
                            if (SafeNativeMethods.jacobi_solve(ref jacobianHandle, residualsPlus, residualsMinus, ref rciRequestJacobian) != TR_SUCCESS)
                            {
                                SafeNativeMethods.FreeBuffers();
                                return ErrorResult();
                            }
                            if (rciRequestJacobian == 1)
                                function(parameters, residualsPlus);
                            else if (rciRequestJacobian == 2)
                                function(parameters, residualsMinus);
                            else if (rciRequestJacobian == 0)
                                jacobianSuccessful = 1;
                        }
                    }
                }
            }

            int stopCriterionNumber = 0, iterations = 0;
            double initialResidual = 0, finalResidual = 0;
            if (SafeNativeMethods.unbound_nonlinearleastsq_get(ref solverHandle, ref iterations, ref stopCriterionNumber, ref initialResidual, ref finalResidual) != TR_SUCCESS)
            {
                SafeNativeMethods.FreeBuffers();
                return ErrorResult();
            }

            if (SafeNativeMethods.unbound_nonlinearleastsq_delete(ref solverHandle) != TR_SUCCESS)
            {
                SafeNativeMethods.FreeBuffers();
                return ErrorResult();
            }

            if (SafeNativeMethods.jacobi_delete(ref jacobianHandle) != TR_SUCCESS)
            {
                SafeNativeMethods.FreeBuffers();
                return ErrorResult();
            }

            SafeNativeMethods.FreeBuffers();
            
            NonLinearLeastSquaresConvergenceType convergenceType = NonLinearLeastSquaresConvergenceType.Error;
            switch (rciRequest)
            {
                case -1:
                    convergenceType = NonLinearLeastSquaresConvergenceType.MaxIterationsExceeded; break;
                case -2:
                    convergenceType = NonLinearLeastSquaresConvergenceType.Criterion0; break;
                case -3:
                    convergenceType = NonLinearLeastSquaresConvergenceType.Criterion1; break;
                case -4:
                    convergenceType = NonLinearLeastSquaresConvergenceType.Criterion2; break;
                case -5:
                    convergenceType = NonLinearLeastSquaresConvergenceType.Criterion3; break;
                case -6:
                    convergenceType = NonLinearLeastSquaresConvergenceType.Criterion4; break;
            }

            // no errors, find reason for stopping;
            return new NonLinearLeastSquaresResult() { ConvergenceType = convergenceType, NumberOfIterations = iterations };
        }

        public static NonLinearLeastSquaresResult ErrorResult()
        {
            return new NonLinearLeastSquaresResult() { ConvergenceType = NonLinearLeastSquaresConvergenceType.Error };
        }
    }
}

#endif
