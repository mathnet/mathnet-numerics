// <copyright file="BasinHoppingTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// https://numerics.mathdotnet.com
// https://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-$CURRENT_YEAR$ Math.NET
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.Optimization.ObjectiveFunctions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathNet.Numerics.Tests.OptimizationTests
{
    using Random = System.Random;

    [TestFixture]
    public class BasinHoppingTests
    {
        #region Test Objective Functions

        /// <summary>
        /// Base class for test functions implementing IObjectiveFunction
        /// </summary>
        private abstract class TestFunctionBase : IObjectiveFunction
        {
            protected int dimensions;
            protected Vector<double> currentPoint;
            protected double functionValue;
            protected Vector<double> gradient;
            protected Matrix<double> hessian;

            protected TestFunctionBase(int dimensions)
            {
                this.dimensions = dimensions;
                currentPoint = Vector<double>.Build.Dense(dimensions);
                gradient = Vector<double>.Build.Dense(dimensions);
                hessian = Matrix<double>.Build.Dense(dimensions, dimensions);
            }

            // Abstract method to be implemented by derived classes
            protected abstract void CalculateValues();

            public void EvaluateAt(Vector<double> point)
            {
                currentPoint = point;
                CalculateValues();
            }

            public abstract IObjectiveFunction Fork();
            public abstract IObjectiveFunction CreateNew();

            public Vector<double> Point => currentPoint;
            public double Value => functionValue;
            public Vector<double> Gradient => gradient;
            public Matrix<double> Hessian => hessian;

            // Support flags
            public bool IsGradientSupported => true;
            public bool IsHessianSupported => false; // Set to false as we don't implement full Hessian in all cases
        }

        /// <summary>
        /// Rastrigin function implementation
        /// f(x) = 10n + Σ[x_i² - 10cos(2πx_i)]
        /// Global minimum at (0,0,...,0) with f(x) = 0
        /// </summary>
        private class RastriginFunction : TestFunctionBase
        {
            public RastriginFunction(int dimensions) : base(dimensions) { }

            protected override void CalculateValues()
            {
                // Calculate function value
                functionValue = 10 * dimensions;
                for (var i = 0; i < dimensions; i++)
                {
                    functionValue += Math.Pow(currentPoint[i], 2) - 10 * Math.Cos(2 * Math.PI * currentPoint[i]);
                }

                // Calculate gradient
                for (var i = 0; i < dimensions; i++)
                {
                    gradient[i] = 2 * currentPoint[i] + 20 * Math.PI * Math.Sin(2 * Math.PI * currentPoint[i]);
                }
            }

            public override IObjectiveFunction Fork()
            {
                var fork = new RastriginFunction(dimensions);
                fork.EvaluateAt(currentPoint);
                return fork;
            }

            public override IObjectiveFunction CreateNew()
            {
                return new RastriginFunction(dimensions);
            }
        }

        /// <summary>
        /// Rosenbrock function implementation
        /// f(x) = Σ[100(x_{i+1} - x_i²)² + (x_i - 1)²]
        /// Global minimum at (1,1,...,1) with f(x) = 0
        /// </summary>
        private class RosenbrockFunction : TestFunctionBase
        {
            public RosenbrockFunction(int dimensions) : base(dimensions)
            {
                if (dimensions < 2)
                    throw new ArgumentException("Rosenbrock function requires at least 2 dimensions");
            }

            protected override void CalculateValues()
            {
                // Calculate function value
                functionValue = 0;
                for (var i = 0; i < dimensions - 1; i++)
                {
                    var a = currentPoint[i + 1] - Math.Pow(currentPoint[i], 2);
                    var b = currentPoint[i] - 1;
                    functionValue += 100 * Math.Pow(a, 2) + Math.Pow(b, 2);
                }

                // Calculate gradient
                for (var i = 0; i < dimensions; i++)
                {
                    if (i == 0)
                    {
                        gradient[i] = -400 * currentPoint[i] * (currentPoint[i + 1] - Math.Pow(currentPoint[i], 2)) - 2 * (1 - currentPoint[i]);
                    }
                    else if (i == dimensions - 1)
                    {
                        gradient[i] = 200 * (currentPoint[i] - Math.Pow(currentPoint[i - 1], 2));
                    }
                    else
                    {
                        gradient[i] = 200 * (currentPoint[i] - Math.Pow(currentPoint[i - 1], 2))
                                    - 400 * currentPoint[i] * (currentPoint[i + 1] - Math.Pow(currentPoint[i], 2))
                                    - 2 * (1 - currentPoint[i]);
                    }
                }
            }

            public override IObjectiveFunction Fork()
            {
                var fork = new RosenbrockFunction(dimensions);
                fork.EvaluateAt(currentPoint);
                return fork;
            }

            public override IObjectiveFunction CreateNew()
            {
                return new RosenbrockFunction(dimensions);
            }
        }

        /// <summary>
        /// Himmelblau function implementation
        /// f(x,y) = (x² + y - 11)² + (x + y² - 7)²
        /// Has 4 local minima
        /// </summary>
        private class HimmelblauFunction : TestFunctionBase
        {
            public HimmelblauFunction() : base(2) { }

            protected override void CalculateValues()
            {
                var x = currentPoint[0];
                var y = currentPoint[1];

                // Calculate function value
                var term1 = Math.Pow(x, 2) + y - 11;
                var term2 = x + Math.Pow(y, 2) - 7;
                functionValue = Math.Pow(term1, 2) + Math.Pow(term2, 2);

                // Calculate gradient
                gradient[0] = 4 * x * term1 + 2 * term2;
                gradient[1] = 2 * term1 + 4 * y * term2;
            }

            public override IObjectiveFunction Fork()
            {
                var fork = new HimmelblauFunction();
                fork.EvaluateAt(currentPoint);
                return fork;
            }

            public override IObjectiveFunction CreateNew()
            {
                return new HimmelblauFunction();
            }
        }

        /// <summary>
        /// Ackley function implementation
        /// f(x) = -20*exp(-0.2*sqrt(0.5*(x₁² + x₂²))) - exp(0.5*(cos(2πx₁) + cos(2πx₂))) + 20 + e
        /// Global minimum at (0,0) with f(x) = 0
        /// </summary>
        private class AckleyFunction : TestFunctionBase
        {
            private const double A = 20;
            private const double B = 0.2;
            private const double C = 2 * Math.PI;

            public AckleyFunction(int dimensions) : base(dimensions) { }

            protected override void CalculateValues()
            {
                double sumSquares = 0;
                double sumCos = 0;

                for (var i = 0; i < dimensions; i++)
                {
                    sumSquares += Math.Pow(currentPoint[i], 2);
                    sumCos += Math.Cos(C * currentPoint[i]);
                }

                var term1 = -A * Math.Exp(-B * Math.Sqrt(sumSquares / dimensions));
                var term2 = -Math.Exp(sumCos / dimensions);

                functionValue = term1 + term2 + A + Math.E;

                // Calculate gradient
                var sqrtTerm = Math.Sqrt(sumSquares / dimensions);
                var expTerm1 = Math.Exp(-B * sqrtTerm);
                var expTerm2 = Math.Exp(sumCos / dimensions);

                for (var i = 0; i < dimensions; i++)
                {
                    var gradTerm1 = A * B * expTerm1 * currentPoint[i] / (dimensions * sqrtTerm);
                    var gradTerm2 = C * expTerm2 * Math.Sin(C * currentPoint[i]) / dimensions;

                    gradient[i] = gradTerm1 + gradTerm2;
                }
            }

            public override IObjectiveFunction Fork()
            {
                var fork = new AckleyFunction(dimensions);
                fork.EvaluateAt(currentPoint);
                return fork;
            }

            public override IObjectiveFunction CreateNew()
            {
                return new AckleyFunction(dimensions);
            }
        }

        #endregion

        // Note: NelderMeadSimplex is not used as it doesn't work properly with BasinHopping in this implementation

        [Test]
        public void BasinHopping_RastriginFunction_FindsGlobalMinimum()
        {
            var dimensions = 2;
            var initialGuess = Vector<double>.Build.Dense(dimensions, 3.0);
            var objective = new RastriginFunction(dimensions);
            objective.EvaluateAt(initialGuess); // Initial evaluation

            var localMinimizer = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 500);
            var basinHopping = new BasinHopping(
                localMinimizer: localMinimizer,
                temperature: 1.0,
                stepSize: 1.0,
                maxIterations: 50);

            // Track progress
            var functionValues = new List<double>();
            var acceptedSteps = new List<bool>();

            basinHopping.SetCallback((x, f, accepted) =>
            {
                functionValues.Add(f);
                acceptedSteps.Add(accepted);
                Debug.WriteLine($"Iteration: {functionValues.Count}, Value: {f}, Accepted: {accepted}");
                return false; // Continue optimization
            });

            var result = basinHopping.FindMinimum(objective, initialGuess);

            Debug.WriteLine($"Final solution: [{string.Join(", ", result.MinimizingPoint)}]");

            // Evaluate objective at final point to get function value
            var finalObjective = objective.CreateNew();
            finalObjective.EvaluateAt(result.MinimizingPoint);
            var finalValue = finalObjective.Value;

            Debug.WriteLine($"Function value: {finalValue}");
            Debug.WriteLine($"Iterations: {result.Iterations}");
            Debug.WriteLine($"Acceptance rate: {acceptedSteps.Count(x => x) / (double)acceptedSteps.Count:P2}");

            // Global minimum should be close to (0,0)
            for (var i = 0; i < dimensions; i++)
            {
                Assert.IsTrue(Math.Abs(result.MinimizingPoint[i]) < 0.1,
                    $"Parameter {i} should be close to 0, got {result.MinimizingPoint[i]}");
            }

            Assert.IsTrue(finalValue < 0.1,
                $"Function value should be close to 0, got {finalValue}");
        }

        [Test]
        public void BasinHopping_RosenbrockFunction_FindsGlobalMinimum()
        {
            var dimensions = 4;
            var initialGuess = Vector<double>.Build.Dense(dimensions, 0.0); // Start at origin
            var objective = new RosenbrockFunction(dimensions);
            objective.EvaluateAt(initialGuess);

            var localMinimizer = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 500);
            var basinHopping = new BasinHopping(
                localMinimizer: localMinimizer,
                temperature: 1.0,
                stepSize: 0.5,
                maxIterations: 100);

            var result = basinHopping.FindMinimum(objective, initialGuess);

            Debug.WriteLine($"Final solution: [{string.Join(", ", result.MinimizingPoint)}]");

            // Evaluate at final point
            var finalObjective = objective.CreateNew();
            finalObjective.EvaluateAt(result.MinimizingPoint);
            var finalValue = finalObjective.Value;

            Debug.WriteLine($"Function value: {finalValue}");

            // Global minimum is at (1,1,...,1)
            for (var i = 0; i < dimensions; i++)
            {
                Assert.IsTrue(Math.Abs(result.MinimizingPoint[i] - 1.0) < 1E-5,
                    $"Parameter {i} should be close to 1, got {result.MinimizingPoint[i]}");
            }

            Assert.IsTrue(finalValue < 1.0,
                $"Function value should be close to 0, got {finalValue}");
        }

        [Test]
        public void BasinHopping_HimmelblauFunction_FindsLocalMinimum()
        {
            var initialGuess = Vector<double>.Build.DenseOfArray(new double[] { 0.0, 0.0 });
            var objective = new HimmelblauFunction();
            objective.EvaluateAt(initialGuess);

            var localMinimizer = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 500);
            var basinHopping = new BasinHopping(
                localMinimizer: localMinimizer,
                temperature: 2.0,
                stepSize: 2.0,
                maxIterations: 30);

            var result = basinHopping.FindMinimum(objective, initialGuess);

            Debug.WriteLine($"Found minimum at ({result.MinimizingPoint[0]}, {result.MinimizingPoint[1]})");

            // Evaluate at final point
            var finalObjective = objective.CreateNew();
            finalObjective.EvaluateAt(result.MinimizingPoint);
            var finalValue = finalObjective.Value;

            Debug.WriteLine($"Function value: {finalValue}");

            // Known local minima for Himmelblau function
            var knownMinima = new[]
            {
                new[] { 3.0, 2.0 },
                new[] { -2.805118, 3.131312 },
                new[] { -3.779310, -3.283186 },
                new[] { 3.584428, -1.848126 }
            };

            // Check if result is close to one of the known minima
            var closeToMinimum = false;
            foreach (var minima in knownMinima)
            {
                var distance = Math.Sqrt(
                    Math.Pow(result.MinimizingPoint[0] - minima[0], 2) +
                    Math.Pow(result.MinimizingPoint[1] - minima[1], 2));

                if (distance < 0.1)
                {
                    closeToMinimum = true;
                    Debug.WriteLine($"Result is close to known minimum ({minima[0]}, {minima[1]})");
                    break;
                }
            }

            Assert.IsTrue(closeToMinimum, "Result should be close to one of the known minima");
            Assert.IsTrue(finalValue < 0.1,
                $"Function value should be close to 0, got {finalValue}");
        }

        [Test]
        public void BasinHopping_AckleyFunction_FindsGlobalMinimum()
        {
            var dimensions = 2;
            var initialGuess = Vector<double>.Build.Dense(dimensions, 2.0);
            var objective = new AckleyFunction(dimensions);
            objective.EvaluateAt(initialGuess);

            var localMinimizer = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 200);
            var basinHopping = new BasinHopping(
                localMinimizer: localMinimizer,
                temperature: 5.0,  // Higher temperature for better exploration
                stepSize: 2.0,     // Larger step size to jump between basins
                maxIterations: 50);

            var result = basinHopping.FindMinimum(objective, initialGuess);

            // Global minimum is at (0,0,...,0)
            var distanceToOrigin = result.MinimizingPoint.L2Norm();
            Assert.IsTrue(distanceToOrigin < 0.5,
                $"Solution should be close to origin, distance was {distanceToOrigin}");
        }

        [Test]
        public void BasinHopping_CompareTemperatures_RastriginFunction()
        {
            var dimensions = 3;
            var initialGuess = Vector<double>.Build.Dense(dimensions, 2.0);
            var objective = new RastriginFunction(dimensions);
            objective.EvaluateAt(initialGuess);

            // Create two optimizers with different temperatures
            var localMinimizer = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 200);

            var lowTempOptimizer = new BasinHopping(
                localMinimizer: localMinimizer,
                temperature: 0.1,
                stepSize: 1.0,
                maxIterations: 30);

            var highTempOptimizer = new BasinHopping(
                localMinimizer: localMinimizer,
                temperature: 3.0,
                stepSize: 1.0,
                maxIterations: 30);

            // Track acceptance rates
            int lowTempAccepted = 0, lowTempTotal = 0;
            int highTempAccepted = 0, highTempTotal = 0;

            lowTempOptimizer.SetCallback((x, f, accepted) =>
            {
                lowTempTotal++;
                if (accepted) lowTempAccepted++;
                return false;
            });

            highTempOptimizer.SetCallback((x, f, accepted) =>
            {
                highTempTotal++;
                if (accepted) highTempAccepted++;
                return false;
            });

            var lowTempResult = lowTempOptimizer.FindMinimum(objective.CreateNew(), initialGuess);
            var highTempResult = highTempOptimizer.FindMinimum(objective.CreateNew(), initialGuess);

            var lowTempRate = lowTempTotal > 0 ? (double)lowTempAccepted / lowTempTotal : 0;
            var highTempRate = highTempTotal > 0 ? (double)highTempAccepted / highTempTotal : 0;

            Debug.WriteLine($"Low temperature acceptance rate: {lowTempRate:P2}");
            Debug.WriteLine($"High temperature acceptance rate: {highTempRate:P2}");

            // Higher temperature should lead to higher acceptance rate
            Assert.IsTrue(highTempRate > lowTempRate,
                "Higher temperature should result in higher acceptance rate");

            // Evaluate both results
            var lowTempObjective = objective.CreateNew();
            lowTempObjective.EvaluateAt(lowTempResult.MinimizingPoint);

            var highTempObjective = objective.CreateNew();
            highTempObjective.EvaluateAt(highTempResult.MinimizingPoint);

            Debug.WriteLine($"Low temperature result value: {lowTempObjective.Value}");
            Debug.WriteLine($"High temperature result value: {highTempObjective.Value}");
        }

        [Test]
        public void BasinHopping_CustomTakeStep_RastriginFunction()
        {
            var dimensions = 2;
            var initialGuess = Vector<double>.Build.Dense(dimensions, 3.0);
            var objective = new RastriginFunction(dimensions);
            objective.EvaluateAt(initialGuess);

            var localMinimizer = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 200);
            var basinHopping = new BasinHopping(
                localMinimizer: localMinimizer,
                temperature: 1.0,
                maxIterations: 50);

            // Custom step-taking function with decreasing step size
            var random = new Random(42); // Fixed seed for reproducibility
            var currentStepSize = 2.0;

            basinHopping.SetTakeStep(currentPoint =>
            {
                var newPoint = currentPoint.Clone();

                // Take random step with decreasing step size
                for (var i = 0; i < dimensions; i++)
                {
                    newPoint[i] += (random.NextDouble() * 2 - 1) * currentStepSize;
                }

                // Decrease step size for next iteration
                currentStepSize *= 0.95;

                return newPoint;
            });

            var result = basinHopping.FindMinimum(objective, initialGuess);

            Debug.WriteLine($"Custom step result: [{string.Join(", ", result.MinimizingPoint)}]");

            // Evaluate at final point
            var finalObjective = objective.CreateNew();
            finalObjective.EvaluateAt(result.MinimizingPoint);

            Debug.WriteLine($"Function value: {finalObjective.Value}");
            Debug.WriteLine($"Final step size: {currentStepSize}");

            // Should find global minimum
            Assert.IsTrue(finalObjective.Value < 1.0,
                $"Function value should be close to 0, got {finalObjective.Value}");
        }

        [Test]
        public void BasinHopping_CustomAcceptTest_RastriginFunction()
        {
            var dimensions = 2;
            var initialGuess = Vector<double>.Build.Dense(dimensions, 3.0);
            var objective = new RastriginFunction(dimensions);
            objective.EvaluateAt(initialGuess);

            var localMinimizer = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 200);
            var basinHopping = new BasinHopping(
                localMinimizer: localMinimizer,
                temperature: 1.0,
                stepSize: 1.0,
                maxIterations: 50);

            // Custom acceptance test with simulated annealing cooling schedule
            var random = new Random(42);
            var temperature = 5.0; // Starting temperature
            var coolingRate = 0.95; // Cooling rate

            basinHopping.SetAcceptTest((newValue, oldValue) =>
            {
                // Always accept if new value is better
                if (newValue <= oldValue)
                {
                    return AcceptanceStatus.Accept;
                }

                // Metropolis criterion with decreasing temperature
                var deltaE = newValue - oldValue;
                var acceptanceProbability = Math.Exp(-deltaE / temperature);

                var accepted = random.NextDouble() < acceptanceProbability;

                // Cool the temperature
                temperature *= coolingRate;

                return accepted ? AcceptanceStatus.Accept : AcceptanceStatus.Reject;
            });

            var result = basinHopping.FindMinimum(objective, initialGuess);

            Debug.WriteLine($"Custom accept test result: [{string.Join(", ", result.MinimizingPoint)}]");

            // Evaluate at final point
            var finalObjective = objective.CreateNew();
            finalObjective.EvaluateAt(result.MinimizingPoint);

            Debug.WriteLine($"Function value: {finalObjective.Value}");
            Debug.WriteLine($"Final temperature: {temperature}");

            // Should find global minimum
            Assert.IsTrue(finalObjective.Value < 1.0,
                $"Function value should be close to 0, got {finalObjective.Value}");
        }

        [Test]
        public void BasinHopping_CompareLocalMinimizers_RastriginFunction()
        {
            var dimensions = 2;
            var initialGuess = Vector<double>.Build.Dense(dimensions, 3.0);
            var objective = new RastriginFunction(dimensions);
            objective.EvaluateAt(initialGuess);

            // Compare different local minimizers
            var nelderMead = new NelderMeadSimplex(1e-8, 100);
            var bfgs = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 100);
            var lbfgs = new LimitedMemoryBfgsMinimizer(1e-8, 1e-8, 1e-8, 100);

            var basinHoppingNM = new BasinHopping(nelderMead, temperature: 1.0, maxIterations: 30);
            var basinHoppingBFGS = new BasinHopping(bfgs, temperature: 1.0, maxIterations: 30);
            var basinHoppingLBFGS = new BasinHopping(lbfgs, temperature: 1.0, maxIterations: 30);

            var resultNM = basinHoppingNM.FindMinimum(objective.CreateNew(), initialGuess);
            var resultBFGS = basinHoppingBFGS.FindMinimum(objective.CreateNew(), initialGuess);
            var resultLBFGS = basinHoppingLBFGS.FindMinimum(objective.CreateNew(), initialGuess);

            // Evaluate all results
            var evalNM = objective.CreateNew();
            evalNM.EvaluateAt(resultNM.MinimizingPoint);

            var evalBFGS = objective.CreateNew();
            evalBFGS.EvaluateAt(resultBFGS.MinimizingPoint);

            var evalLBFGS = objective.CreateNew();
            evalLBFGS.EvaluateAt(resultLBFGS.MinimizingPoint);

            Debug.WriteLine($"Nelder-Mead result: {evalNM.Value}");
            Debug.WriteLine($"BFGS result: {evalBFGS.Value}");
            Debug.WriteLine($"L-BFGS result: {evalLBFGS.Value}");

            // At least one minimizer should find a good solution
            Assert.IsTrue(evalNM.Value < 1.0 || evalBFGS.Value < 1.0 || evalLBFGS.Value < 1.0,
                "At least one minimizer should find a good solution");
        }

        [Test]
        public void BasinHopping_HighDimensional_RastriginFunction()
        {
            var dimensions = 10; // Higher dimensional problem
            var initialGuess = Vector<double>.Build.Dense(dimensions, 2.0);
            var objective = new RastriginFunction(dimensions);
            objective.EvaluateAt(initialGuess);

            var localMinimizer = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 200);
            var basinHopping = new BasinHopping(
                localMinimizer: localMinimizer,
                temperature: 2.0,
                stepSize: 1.0,
                maxIterations: 100);

            var result = basinHopping.FindMinimum(objective, initialGuess);

            // Evaluate at final point
            var finalObjective = objective.CreateNew();
            finalObjective.EvaluateAt(result.MinimizingPoint);
            var functionValue = finalObjective.Value;
            var distanceToOrigin = result.MinimizingPoint.L2Norm();

            Debug.WriteLine($"High-dimensional result function value: {functionValue}");
            Debug.WriteLine($"Distance to origin: {distanceToOrigin}");

            // For high dimensions, we use more relaxed tolerances
            Assert.IsTrue(functionValue < dimensions * 2,
                $"Function value should be relatively low, got {functionValue}");
        }
    }
}
