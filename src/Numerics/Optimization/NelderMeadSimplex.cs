// <copyright file="NelderMeadSimplex.cs" company="Math.NET">
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

// Converted from code released with a MIT license available at https://code.google.com/p/nelder-mead-simplex/

using System;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;

namespace MathNet.Numerics.Optimization
{
    /// <summary>
    /// Class implementing the Nelder-Mead simplex algorithm, used to find a minima when no gradient is available.
    /// Called fminsearch() in Matlab. A description of the algorithm can be found at
    /// http://se.mathworks.com/help/matlab/math/optimizing-nonlinear-functions.html#bsgpq6p-11
    /// or
    /// https://en.wikipedia.org/wiki/Nelder%E2%80%93Mead_method
    /// </summary>
    public sealed class NelderMeadSimplex : IUnconstrainedMinimizer
    {
        static readonly double JITTER = 1e-10d;           // a small value used to protect against floating point noise

        public double ConvergenceTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public NelderMeadSimplex(double convergenceTolerance, int maximumIterations)
        {
            ConvergenceTolerance = convergenceTolerance;
            MaximumIterations = maximumIterations;
        }

        /// <summary>
        /// Finds the minimum of the objective function without an initial perturbation, the default values used
        /// by fminsearch() in Matlab are used instead
        /// http://se.mathworks.com/help/matlab/math/optimizing-nonlinear-functions.html#bsgpq6p-11
        /// </summary>
        /// <param name="objectiveFunction">The objective function, no gradient or hessian needed</param>
        /// <param name="initialGuess">The initial guess</param>
        /// <returns>The minimum point</returns>
        public MinimizationResult FindMinimum(IObjectiveFunction objectiveFunction, Vector<double> initialGuess)
        {
            return Minimum(objectiveFunction, initialGuess, ConvergenceTolerance, MaximumIterations);
        }

        /// <summary>
        /// Finds the minimum of the objective function with an initial perturbation
        /// </summary>
        /// <param name="objectiveFunction">The objective function, no gradient or hessian needed</param>
        /// <param name="initialGuess">The initial guess</param>
        /// <param name="initalPertubation">The initial perturbation</param>
        /// <returns>The minimum point</returns>
        public MinimizationResult FindMinimum(IObjectiveFunction objectiveFunction, Vector<double> initialGuess, Vector<double> initalPertubation)
        {
            return Minimum(objectiveFunction, initialGuess, initalPertubation, ConvergenceTolerance, MaximumIterations);
        }

        /// <summary>
        /// Finds the minimum of the objective function without an initial perturbation, the default values used
        /// by fminsearch() in Matlab are used instead
        /// http://se.mathworks.com/help/matlab/math/optimizing-nonlinear-functions.html#bsgpq6p-11
        /// </summary>
        /// <param name="objectiveFunction">The objective function, no gradient or hessian needed</param>
        /// <param name="initialGuess">The initial guess</param>
        /// <returns>The minimum point</returns>
        public static MinimizationResult Minimum(IObjectiveFunction objectiveFunction, Vector<double> initialGuess, double convergenceTolerance=1e-8, int maximumIterations=1000)
        {
            var initalPertubation = new LinearAlgebra.Double.DenseVector(initialGuess.Count);
            for (int i = 0; i < initialGuess.Count; i++)
            {
                initalPertubation[i] = initialGuess[i] == 0.0 ? 0.00025 : initialGuess[i] * 0.05;
            }
            return Minimum(objectiveFunction, initialGuess, initalPertubation, convergenceTolerance, maximumIterations);
        }

        /// <summary>
        /// Finds the minimum of the objective function with an initial perturbation
        /// </summary>
        /// <param name="objectiveFunction">The objective function, no gradient or hessian needed</param>
        /// <param name="initialGuess">The initial guess</param>
        /// <param name="initalPertubation">The initial perturbation</param>
        /// <returns>The minimum point</returns>
        public static MinimizationResult Minimum(IObjectiveFunction objectiveFunction, Vector<double> initialGuess, Vector<double> initalPertubation, double convergenceTolerance=1e-8, int maximumIterations=1000)
        {
            // confirm that we are in a position to commence
            if (objectiveFunction == null)
                throw new ArgumentNullException(nameof(objectiveFunction),"ObjectiveFunction must be set to a valid ObjectiveFunctionDelegate");

            if (initialGuess == null)
                throw new ArgumentNullException(nameof(initialGuess), "initialGuess must be initialized");

            if (initalPertubation == null)
                throw new ArgumentNullException(nameof(initalPertubation), "initalPertubation must be initialized, if unknown use overloaded version of FindMinimum()");

            SimplexConstant[] simplexConstants = SimplexConstant.CreateSimplexConstantsFromVectors(initialGuess,initalPertubation);

            // create the initial simplex
            int numDimensions = simplexConstants.Length;
            int numVertices = numDimensions + 1;
            Vector<double>[] vertices = InitializeVertices(simplexConstants);

            int evaluationCount = 0;
            ExitCondition exitCondition;
            ErrorProfile errorProfile;

            double[] errorValues = InitializeErrorValues(vertices, objectiveFunction);
            int numTimesHasConverged = 0;

            // iterate until we converge, or complete our permitted number of iterations
            while (true)
            {
                errorProfile = EvaluateSimplex(errorValues);

                // see if the range in point heights is small enough to exit
                // to handle the case when the function is symmetrical and extra iteration is performed
                if (HasConverged(convergenceTolerance, errorProfile, errorValues))
                {
                    numTimesHasConverged++;
                }
                else
                {
                    numTimesHasConverged = 0;
                }
                if (numTimesHasConverged == 2)
                {
                    exitCondition = ExitCondition.Converged;
                    break;
                }

                // This algorithm follows https://www.scilab.org/sites/default/files/neldermead.pdf we give the
                // lines from Figure 4.1. to better follow along. Note that the values we use for
                // ρ (rho) = 1, χ (chi) =2, γ (gamma) = 0.5 and σ (sigma) = 0.5 are the default values given in the paper and
                // match the values used here https://se.mathworks.com/help/matlab/math/optimizing-nonlinear-functions.html#bsgpq6p-11

                // calculate the centroid
                // x ← x(n + 1)
                Vector<double> centroid = ComputeCentroid(vertices, errorProfile);

                // attempt a reflection of the simplex - using our default for rho
                // x_r ← x(ρ, n + 1) {Reflect}
                // f_r ← f(x_r)
                (Vector<double> reflectionPoint, double reflectionPointValue) = ScaleSimplex(1.0, ref errorProfile, centroid, vertices, objectiveFunction);
                ++evaluationCount;

                // if f_r < f_1 then
                if (reflectionPointValue < errorValues[errorProfile.LowestIndex])
                {
                    // it's better than the best point, but we attempt to improve even that by expanding the simplex
                    // x_e ← x(ρχ, n + 1) {Expand}
                    // f_e ← f(x_e)
                    (Vector<double> expansionPoint, double expansionPointValue)  = ScaleSimplex(2.0, ref errorProfile, centroid, vertices, objectiveFunction);
                    ++evaluationCount;

                    // if f_e < f_r then
                    if (expansionPointValue < reflectionPointValue)
                    {
                        // Accept x_e
                        AcceptNewVertex(expansionPoint, expansionPointValue, ref errorProfile, vertices, errorValues);
                    }
                    else
                    {
                        // Accept x_r
                        AcceptNewVertex(reflectionPoint, reflectionPointValue, ref errorProfile, vertices, errorValues);
                    }
                }
                // else if f_1 ≤ f_r < f_n then
                else if (reflectionPointValue < errorValues[errorProfile.NextHighestIndex])
                {
                    // Accept x_r
                    AcceptNewVertex(reflectionPoint, reflectionPointValue, ref errorProfile, vertices, errorValues);
                }
                // else if f_n ≤ f_r < f_n+1 then
                else if (reflectionPointValue < errorValues[errorProfile.HighestIndex])
                {
                    // x_c ← x(ργ, n + 1) {Outside contraction}
                    // f_c ← f(x_c)
                    (Vector<double> contractionPoint, double contractionPointValue)  = ScaleSimplex(0.5, ref errorProfile, centroid, vertices, objectiveFunction);
                    // if f_c < f_r then
                    if (contractionPointValue < reflectionPointValue)
                    {
                        // Accept x_c
                        AcceptNewVertex(contractionPoint, contractionPointValue, ref errorProfile, vertices, errorValues);
                    }
                    // else
                    else
                    {
                        // Compute the points x_i = x_1 + σ(x_i − x_1), i = 2, n + 1 {Shrink}
                        // Compute f_i = f(v_i) for i = 2, n + 1
                        ShrinkSimplex(errorProfile, vertices, errorValues, objectiveFunction);
                        evaluationCount += numVertices; // that required one function evaluation for each vertex; keep track
                    }
                }
                // else
                else
                {
                    // The reflected value is worse than even the worst vertex of the current simplex
                    // x_c ← x(−γ, n + 1) {Inside contraction}
                    // f_c ← f(x_c)
                    (Vector<double> contractionPoint, double contractionPointValue)  = ScaleSimplex(-0.5, ref errorProfile, centroid, vertices, objectiveFunction);
                    ++evaluationCount;

                    // if fc < fn+1 then
                    if (contractionPointValue < errorValues[errorProfile.HighestIndex])
                    {
                        // Accept x_c
                        AcceptNewVertex(contractionPoint, contractionPointValue, ref errorProfile, vertices, errorValues);
                    }
                    // else
                    else
                    {
                        // Compute the points xi = x_1 + σ(x_i − x_1), i = 2, n + 1 {Shrink}
                        // Compute fi = f(vi) for i = 2, n + 1
                        ShrinkSimplex(errorProfile, vertices, errorValues, objectiveFunction);
                        evaluationCount += numVertices; // that required one function evaluation for each vertex; keep track
                    }
                }
                // check to see if we have exceeded our allotted number of evaluations
                if (evaluationCount >= maximumIterations)
                {
                    throw new MaximumIterationsException(FormattableString.Invariant($"Maximum iterations ({maximumIterations}) reached."));
                }
            }

            objectiveFunction.EvaluateAt(vertices[errorProfile.LowestIndex]);
            var regressionResult = new MinimizationResult(objectiveFunction, evaluationCount, exitCondition);
            return regressionResult;
        }

        /// <summary>
        /// Evaluate the objective function at each vertex to create a corresponding
        /// list of error values for each vertex
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="objectiveFunction"></param>
        /// <returns></returns>
        static double[] InitializeErrorValues(Vector<double>[] vertices, IObjectiveFunction objectiveFunction)
        {
            double[] errorValues = new double[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                objectiveFunction.EvaluateAt(vertices[i]);
                errorValues[i] = objectiveFunction.Value;
            }
            return errorValues;
        }

        /// <summary>
        /// Check whether the points in the error profile have so little range that we
        /// consider ourselves to have converged
        /// </summary>
        /// <param name="convergenceTolerance"></param>
        /// <param name="errorProfile"></param>
        /// <param name="errorValues"></param>
        /// <returns></returns>
        static bool HasConverged(double convergenceTolerance, ErrorProfile errorProfile, double[] errorValues)
        {
            double range = 2 * Math.Abs(errorValues[errorProfile.HighestIndex] - errorValues[errorProfile.LowestIndex]) /
                (Math.Abs(errorValues[errorProfile.HighestIndex]) + Math.Abs(errorValues[errorProfile.LowestIndex]) + JITTER);

            return range < convergenceTolerance;
        }

        /// <summary>
        /// Examine all error values to determine the ErrorProfile
        /// </summary>
        /// <param name="errorValues"></param>
        /// <returns></returns>
        static ErrorProfile EvaluateSimplex(double[] errorValues)
        {
            ErrorProfile errorProfile = new ErrorProfile();
            if (errorValues[0] > errorValues[1])
            {
                errorProfile.HighestIndex = 0;
                errorProfile.NextHighestIndex = 1;
            }
            else
            {
                errorProfile.HighestIndex = 1;
                errorProfile.NextHighestIndex = 0;
            }

            for (int index = 0; index < errorValues.Length; index++)
            {
                double errorValue = errorValues[index];
                if (errorValue <= errorValues[errorProfile.LowestIndex])
                {
                    errorProfile.LowestIndex = index;
                }
                if (errorValue > errorValues[errorProfile.HighestIndex])
                {
                    errorProfile.NextHighestIndex = errorProfile.HighestIndex; // downgrade the current highest to next highest
                    errorProfile.HighestIndex = index;
                }
                else if (errorValue > errorValues[errorProfile.NextHighestIndex] && index != errorProfile.HighestIndex)
                {
                    errorProfile.NextHighestIndex = index;
                }
            }

            return errorProfile;
        }

        /// <summary>
        /// Construct an initial simplex, given starting guesses for the constants, and
        /// initial step sizes for each dimension
        /// </summary>
        /// <param name="simplexConstants"></param>
        /// <returns></returns>
        static Vector<double>[] InitializeVertices(SimplexConstant[] simplexConstants)
        {
            int numDimensions = simplexConstants.Length;
            Vector<double>[] vertices = new Vector<double>[numDimensions + 1];

            // define one point of the simplex as the given initial guesses
            var p0 = new LinearAlgebra.Double.DenseVector(numDimensions);
            for (int i = 0; i < numDimensions; i++)
            {
                p0[i] = simplexConstants[i].Value;
            }

            // now fill in the vertices, creating the additional points as:
            // P(i) = P(0) + Scale(i) * UnitVector(i)
            vertices[0] = p0;
            for (int i = 0; i < numDimensions; i++)
            {
                double scale = simplexConstants[i].InitialPerturbation;
                Vector<double> unitVector = new LinearAlgebra.Double.DenseVector(numDimensions);
                unitVector[i] = 1;
                vertices[i + 1] = p0.Add(unitVector.Multiply(scale));
            }
            return vertices;
        }

        /// <summary>
        /// Calculates a new simplex by moving the worst point along the line given by itself and the centroid.
        /// </summary>
        /// <remarks>This is called the x-function in the paper https://www.scilab.org/sites/default/files/neldermead.pdf (4.4)</remarks>
        /// <param name="scaleFactor">The factor to scale along the given line.</param>
        /// <param name="errorProfile">The error profile.</param>
        /// <param name="centroid">The centroid of the simplex.</param>
        /// <param name="vertices">The simplex.</param>
        /// <param name="objectiveFunction">The objective function.</param>
        /// <returns>The point that would replace the worst thus defining the scaled simplex.</returns>
        static (Vector<double> scaledPoint, double scaledValue) ScaleSimplex(double scaleFactor, ref ErrorProfile errorProfile,
            Vector<double> centroid, Vector<double>[] vertices, IObjectiveFunction objectiveFunction)
        {
            // define the vector from the high point to the centroid
            Vector<double> highPointToCentroid = centroid.Subtract(vertices[errorProfile.HighestIndex]);

            // scale and position the vector to determine the new trial point
            Vector<double> newPoint = highPointToCentroid.Multiply(scaleFactor).Add(centroid);

            // evaluate the new point
            objectiveFunction.EvaluateAt(newPoint);
            return (newPoint, objectiveFunction.Value);
        }

        /// <summary>
        /// Accept the new point as the new vertex of the simplex, replacing the worst point.
        /// </summary>
        /// <param name="newPoint">The new point.</param>
        /// <param name="newErrorValue">The error value at that point.</param>
        /// <param name="errorProfile">The error profile.</param>
        /// <param name="vertices">The vertices of the simplex.</param>
        /// <param name="errorValues">The error values of the simplex.</param>
        static void AcceptNewVertex(Vector<double> newPoint, double newErrorValue, ref ErrorProfile errorProfile, Vector<double>[] vertices,
            double[] errorValues)
        {
            vertices[errorProfile.HighestIndex] = newPoint;
            errorValues[errorProfile.HighestIndex] = newErrorValue;
        }

        /// <summary>
        /// Contract the simplex uniformly around the lowest point
        /// </summary>
        /// <param name="errorProfile"></param>
        /// <param name="vertices"></param>
        /// <param name="errorValues"></param>
        /// <param name="objectiveFunction"></param>
        static void ShrinkSimplex(ErrorProfile errorProfile, Vector<double>[] vertices, double[] errorValues,
                                      IObjectiveFunction objectiveFunction)
        {
            // Let's try to contract uniformly towards the low point;
            // don't bother to update the error profile, we'll do it at the start of the
            // next iteration
            // In the paper this is written as:
            // Compute the points x_i = x_1 + σ(x_i − x_1), i = 2, n + 1 {Shrink}
            // Compute f_i = f(v_i) for i = 2, n + 1

            Vector<double> lowestVertex = vertices[errorProfile.LowestIndex];
            for (int i = 0; i < vertices.Length; i++)
            {
                if (i != errorProfile.LowestIndex)
                {
                    // x_i = x_1 + σ(x_i − x_1) with σ = 1/2 is equal to
                    // x_i = (x_1 + x_i) / 2
                    vertices[i] = vertices[i].Add(lowestVertex).Multiply(0.5);
                    objectiveFunction.EvaluateAt(vertices[i]);
                    errorValues[i] = objectiveFunction.Value;
                }
            }
        }

        /// <summary>
        /// Compute the centroid of all points except the worst
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="errorProfile"></param>
        /// <returns></returns>
        static Vector<double> ComputeCentroid(Vector<double>[] vertices, ErrorProfile errorProfile)
        {
            int numVertices = vertices.Length;
            // find the centroid of all points except the worst one
            Vector<double> centroid = new LinearAlgebra.Double.DenseVector(numVertices - 1);
            for (int i = 0; i < numVertices; i++)
            {
                if (i != errorProfile.HighestIndex)
                {
                    centroid = centroid.Add(vertices[i]);
                }
            }
            return centroid.Multiply(1.0d / (numVertices - 1));
        }

        sealed class SimplexConstant
        {
            SimplexConstant(double value, double initialPerturbation)
            {
                Value = value;
                InitialPerturbation = initialPerturbation;
            }

            /// <summary>
            /// The value of the constant
            /// </summary>
            public double Value { get; }

            // The size of the initial perturbation
            public double InitialPerturbation { get; }

            public static SimplexConstant[] CreateSimplexConstantsFromVectors(Vector<double> initialGuess, Vector<double> initialPertubation)
            {
                var constants = new SimplexConstant[initialGuess.Count];
                for (int i = 0; i < constants.Length;i++ )
                {
                    constants[i] = new SimplexConstant(initialGuess[i], initialPertubation[i]);
                }
                return constants;
            }
        }

        sealed class ErrorProfile
        {
            public int HighestIndex { get; set; }
            public int NextHighestIndex { get; set; }
            public int LowestIndex { get; set; }
        }
    }
}
