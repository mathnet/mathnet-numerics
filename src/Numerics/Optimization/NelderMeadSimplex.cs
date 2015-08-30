// <copyright file="NelderMeadSimplex.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// 
// Copyright (c) 2009-2015 Math.NET
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

// Converted from code relased with a MIT liscense available at https://code.google.com/p/nelder-mead-simplex/

using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.Optimization
{
    public sealed class NelderMeadSimplex
    {
        private static readonly double JITTER = 1e-10d;           // a small value used to protect against floating point noise

        public double ConvergenceTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public NelderMeadSimplex(double convergenceTolerance, int maximumIterations)
        {
            ConvergenceTolerance = convergenceTolerance;
            MaximumIterations = maximumIterations;
        }

        /// <summary>
        /// Finds the minimum of the objective function
        /// </summary>
        /// <param name="objectiveFunction">The objective function, no gradient or hessian needed</param>
        /// <param name="initialGuess">The intial guess</param>
        /// <returns>The minimum point</returns>
        public MinimizationResult FindMinimum(IObjectiveFunction objectiveFunction, Vector<double> initialGuess)
        {
            // confirm that we are in a position to commence
            if (objectiveFunction == null)
                throw new ArgumentNullException("objectiveFunction","ObjectiveFunction must be set to a valid ObjectiveFunctionDelegate");

            if (initialGuess == null)
                throw new ArgumentNullException("initialGuess", "initialGuess must be initialized");

            SimplexConstant[] simplexConstants = SimplexConstant.CreateFromVector(initialGuess);

            // create the initial simplex
            int numDimensions = simplexConstants.Length;
            int numVertices = numDimensions + 1;
            Vector<double>[] vertices = _initializeVertices(simplexConstants);
            double[] errorValues = new double[numVertices];

            int evaluationCount = 0;
            MinimizationResult.ExitCondition exitCondition = MinimizationResult.ExitCondition.None;
            ErrorProfile errorProfile;

            errorValues = _initializeErrorValues(vertices, objectiveFunction);

            // iterate until we converge, or complete our permitted number of iterations
            while (true)
            {
                errorProfile = _evaluateSimplex(errorValues);

                // see if the range in point heights is small enough to exit
                if (_hasConverged(ConvergenceTolerance, errorProfile, errorValues))
                {
                    exitCondition = MinimizationResult.ExitCondition.Converged;
                    break;
                }

                // attempt a reflection of the simplex
                double reflectionPointValue = _tryToScaleSimplex(-1.0, ref errorProfile, vertices, errorValues, objectiveFunction);
                ++evaluationCount;
                if (reflectionPointValue <= errorValues[errorProfile.LowestIndex])
                {
                    // it's better than the best point, so attempt an expansion of the simplex
                    double expansionPointValue = _tryToScaleSimplex(2.0, ref errorProfile, vertices, errorValues, objectiveFunction);
                    ++evaluationCount;
                }
                else if (reflectionPointValue >= errorValues[errorProfile.NextHighestIndex])
                {
                    // it would be worse than the second best point, so attempt a contraction to look
                    // for an intermediate point
                    double currentWorst = errorValues[errorProfile.HighestIndex];
                    double contractionPointValue = _tryToScaleSimplex(0.5, ref errorProfile, vertices, errorValues, objectiveFunction);
                    ++evaluationCount;
                    if (contractionPointValue >= currentWorst)
                    {
                        // that would be even worse, so let's try to contract uniformly towards the low point; 
                        // don't bother to update the error profile, we'll do it at the start of the
                        // next iteration
                        _shrinkSimplex(errorProfile, vertices, errorValues, objectiveFunction);
                        evaluationCount += numVertices; // that required one function evaluation for each vertex; keep track
                    }
                }
                // check to see if we have exceeded our alloted number of evaluations
                if (evaluationCount >= MaximumIterations)
                {
                    exitCondition = MinimizationResult.ExitCondition.LackOfProgress;
                    break;
                }
            }
            var regressionResult = new MinimizationResult(objectiveFunction, evaluationCount, exitCondition);
            return regressionResult;
        }

        /// <summary>
        /// Evaluate the objective function at each vertex to create a corresponding
        /// list of error values for each vertex
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        private static double[] _initializeErrorValues(Vector<double>[] vertices, IObjectiveFunction objectiveFunction)
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
        /// <param name="errorProfile"></param>
        /// <param name="errorValues"></param>
        /// <returns></returns>
        private static bool _hasConverged(double convergenceTolerance, ErrorProfile errorProfile, double[] errorValues)
        {
            double range = 2 * Math.Abs(errorValues[errorProfile.HighestIndex] - errorValues[errorProfile.LowestIndex]) /
                (Math.Abs(errorValues[errorProfile.HighestIndex]) + Math.Abs(errorValues[errorProfile.LowestIndex]) + JITTER);

            if (range < convergenceTolerance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Examine all error values to determine the ErrorProfile
        /// </summary>
        /// <param name="errorValues"></param>
        /// <returns></returns>
        private static ErrorProfile _evaluateSimplex(double[] errorValues)
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
        private static Vector<double>[] _initializeVertices(SimplexConstant[] simplexConstants)
        {
            int numDimensions = simplexConstants.Length;
            Vector<double>[] vertices = new Vector<double>[numDimensions + 1];

            // define one point of the simplex as the given initial guesses
            var p0 = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(numDimensions);
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
                Vector<double> unitVector = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(numDimensions);
                unitVector[i] = 1;
                vertices[i + 1] = p0.Add(unitVector.Multiply(scale));
            }
            return vertices;
        }

        /// <summary>
        /// Test a scaling operation of the high point, and replace it if it is an improvement
        /// </summary>
        /// <param name="scaleFactor"></param>
        /// <param name="errorProfile"></param>
        /// <param name="vertices"></param>
        /// <param name="errorValues"></param>
        /// <returns></returns>
        private static double _tryToScaleSimplex(double scaleFactor, ref ErrorProfile errorProfile, Vector<double>[] vertices,
                                          double[] errorValues, IObjectiveFunction objectiveFunction)
        {
            // find the centroid through which we will reflect
            Vector<double> centroid = _computeCentroid(vertices, errorProfile);

            // define the vector from the centroid to the high point
            Vector<double> centroidToHighPoint = vertices[errorProfile.HighestIndex].Subtract(centroid);

            // scale and position the vector to determine the new trial point
            Vector<double> newPoint = centroidToHighPoint.Multiply(scaleFactor).Add(centroid);

            // evaluate the new point
            objectiveFunction.EvaluateAt(newPoint);
            double newErrorValue = objectiveFunction.Value;

            // if it's better, replace the old high point
            if (newErrorValue < errorValues[errorProfile.HighestIndex])
            {
                vertices[errorProfile.HighestIndex] = newPoint;
                errorValues[errorProfile.HighestIndex] = newErrorValue;
            }

            return newErrorValue;
        }

        /// <summary>
        /// Contract the simplex uniformly around the lowest point
        /// </summary>
        /// <param name="errorProfile"></param>
        /// <param name="vertices"></param>
        /// <param name="errorValues"></param>
        private static void _shrinkSimplex(ErrorProfile errorProfile, Vector<double>[] vertices, double[] errorValues,
                                      IObjectiveFunction objectiveFunction)
        {
            Vector<double> lowestVertex = vertices[errorProfile.LowestIndex];
            for (int i = 0; i < vertices.Length; i++)
            {
                if (i != errorProfile.LowestIndex)
                {
                    vertices[i] = (vertices[i].Add(lowestVertex)).Multiply(0.5);
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
        private static Vector<double> _computeCentroid(Vector<double>[] vertices, ErrorProfile errorProfile)
        {
            int numVertices = vertices.Length;
            // find the centroid of all points except the worst one
            Vector<double> centroid = new MathNet.Numerics.LinearAlgebra.Double.DenseVector(numVertices - 1);
            for (int i = 0; i < numVertices; i++)
            {
                if (i != errorProfile.HighestIndex)
                {
                    centroid = centroid.Add(vertices[i]);
                }
            }
            return centroid.Multiply(1.0d / (numVertices - 1));
        }

        private sealed class SimplexConstant
        {
            private double _value;
            private double _initialPerturbation;

            public SimplexConstant(double value, double initialPerturbation)
            {
                _value = value;
                _initialPerturbation = initialPerturbation;
            }

            /// <summary>
            /// The value of the constant
            /// </summary>
            public double Value
            {
                get { return _value; }
                set { _value = value; }
            }

            // The size of the initial perturbation
            public double InitialPerturbation
            {
                get { return _initialPerturbation; }
                set { _initialPerturbation = value; }
            }

            public static SimplexConstant[] CreateFromVector(Vector<double> initialGuess)
            {
                var constants = new SimplexConstant[initialGuess.Count];

                for (int i = 0; i < constants.Length;i++ )
                {
                    double pertubation = initialGuess[i]==0.0 ? 1e-5 : initialGuess[i]*1e-5;
                    constants[i] = new SimplexConstant(initialGuess[i], pertubation);
                }
                return constants;
            }
        }

        private sealed class ErrorProfile
        {
            private int _highestIndex;
            private int _nextHighestIndex;
            private int _lowestIndex;

            public int HighestIndex
            {
                get { return _highestIndex; }
                set { _highestIndex = value; }
            }

            public int NextHighestIndex
            {
                get { return _nextHighestIndex; }
                set { _nextHighestIndex = value; }
            }

            public int LowestIndex
            {
                get { return _lowestIndex; }
                set { _lowestIndex = value; }
            }
        }
    }

    

}
