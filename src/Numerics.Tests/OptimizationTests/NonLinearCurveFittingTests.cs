using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.Optimization.TrustRegion;
using NUnit.Framework;
using System;

namespace MathNet.Numerics.Tests.OptimizationTests
{
    [TestFixture]
    public class NonLinearCurveFittingTests
    {
        #region Rosenbrock

        // model: Rosenbrock
        //       f(x; a, b) = (1 - a)^2 + 100*(b - a^2)^2
        // derivatives:
        //       df/da = 400*a^3 - 400*a*b + 2*a - 2
        //       df/db = 200*(b - a^2)
        // best fitted parameters:
        //       a = 1
        //       b = 1
        
        private Vector<double> RosenbrockResidual(Vector<double> p)
        {
            // Rosenbrock function: f(a,b) = (1-a)² + 100(b-a²)²
            // residual Vector: r = [r₁, r₂]
            // r₁ = (1-a)
            // r₂ = 10(b-a²)
            // f(a, b) = r₁² + r₂² = (1-a)² + 100(b-a²)²

            var residuals = Vector<double>.Build.Dense(2);
            residuals[0] = 1.0 - p[0];
            residuals[1] = 10.0 * (p[1] - p[0] * p[0]);
            return residuals;
        }

        private Matrix<double> RosenbrockJacobian(Vector<double> p)
        {
            // Jacobian Matrix:
            // J = [∂r₁/∂a, ∂r₁/∂b]
            //     [∂r₂/∂a, ∂r₂/∂b]
            // 
            // ∂r₁/∂a = -1
            // ∂r₁/∂b = 0
            // ∂r₂/∂a = -20a
            // ∂r₂/∂b = 10

            var jacobian = Matrix<double>.Build.Dense(2, 2);
            jacobian[0, 0] = -1.0;
            jacobian[0, 1] = 0.0;
            jacobian[1, 0] = -20.0 * p[0];
            jacobian[1, 1] = 10.0;
            return jacobian;
        }

        private readonly Vector<double> RosenbrockPbest = new DenseVector(new double[] { 1.0, 1.0 });

        private readonly Vector<double> RosenbrockStart1 = new DenseVector(new double[] { -1.2, 1.0 });
        private readonly Vector<double> RosebbrockLowerBound = new DenseVector(new double[] { -5.0, -5.0 });
        private readonly Vector<double> RosenbrockUpperBound = new DenseVector(new double[] { 5.0, 5.0 });

        [Test]
        public void Rosenbrock_LM_Residual_Der()
        {
            var obj = ObjectiveFunction.NonlinearModel(
                RosenbrockResidual,
                RosenbrockJacobian);

            var solver = new LevenbergMarquardtMinimizer();

            // unconstrained
            var result = solver.FindMinimum(obj, RosenbrockStart1);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(RosenbrockPbest[i], result.MinimizingPoint[i], 2);
            }

            // box constrained
            result = solver.FindMinimum(obj, RosenbrockStart1, RosebbrockLowerBound, RosenbrockUpperBound);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(RosenbrockPbest[i], result.MinimizingPoint[i], 2);
            }
        }

        [Test]
        public void Rosenbrock_LM_Residual_Dif()
        {
            var obj = ObjectiveFunction.NonlinearModel(
               RosenbrockResidual,
               observationCount: 2,
               accuracyOrder: 2);

            var solver = new LevenbergMarquardtMinimizer();

            // unconstrained 
            var result = solver.FindMinimum(obj, RosenbrockStart1);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(RosenbrockPbest[i], result.MinimizingPoint[i], 2);
            }

            // box constrained 
            result = solver.FindMinimum(obj, RosenbrockStart1, RosebbrockLowerBound, RosenbrockUpperBound);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(RosenbrockPbest[i], result.MinimizingPoint[i], 2);
            }
        }

        [Test]
        public void Rosenbrock_Bfgs_Dif()
        {
            var obj = ObjectiveFunction.NonlinearFunction(RosenbrockResidual);
            var solver = new BfgsMinimizer(1e-8, 1e-8, 1e-8, 1000);
            var result = solver.FindMinimum(obj, RosenbrockStart1);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(RosenbrockPbest[i], result.MinimizingPoint[i], 2);
            }
        }

        [Test]
        public void Rosenbrock_LBfgs_Dif()
        {
            var obj = ObjectiveFunction.NonlinearFunction(RosenbrockResidual);
            var solver = new LimitedMemoryBfgsMinimizer(1e-8, 1e-8, 1e-8, 1000);
            var result = solver.FindMinimum(obj, RosenbrockStart1);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(RosenbrockPbest[i], result.MinimizingPoint[i], 2);
            }
        }

        #endregion Rosenbrock

        #region Rat43

        // model: Rat43 (https://www.itl.nist.gov/div898/strd/nls/data/ratkowsky3.shtml)
        //       f(x; a, b, c, d) =  a / ((1 + exp(b - c * x))^(1 / d))
        // best fitted parameters:
        //       a = 6.9964151270E+02 +/- 1.6302297817E+01
        //       b = 5.2771253025E+00 +/- 2.0828735829E+00
        //       c = 7.5962938329E-01 +/- 1.9566123451E-01
        //       d = 1.2792483859E+00 +/- 6.8761936385E-01
        private Vector<double> Rat43Model(Vector<double> p, Vector<double> x)
        {
            var y = CreateVector.Dense<double>(x.Count);
            for (var i = 0; i < x.Count; i++)
            {
                y[i] = p[0] / Math.Pow(1.0 + Math.Exp(p[1] - p[2] * x[i]), 1.0 / p[3]);
            }
            return y;
        }
        private readonly Vector<double> Rat43X = new DenseVector(new double[] {
            1.00,   2.00,   3.00,   4.00,   5.00,   6.00,   7.00,   8.00,   9.00,   10.00,
            11.00,  12.00,  13.00,  14.00,  15.00
        });
        private readonly Vector<double> Rat43Y = new DenseVector(new double[] {
             16.08, 33.83,  65.80,  97.20,  191.55, 326.20, 386.87, 520.53, 590.03, 651.92,
            724.93, 699.56, 689.96, 637.56, 717.41
        });
        private readonly Vector<double> Rat43Pbest = new DenseVector(new double[] {
            6.9964151270E+02, 5.2771253025E+00, 7.5962938329E-01, 1.2792483859E+00
        });
        private readonly Vector<double> Rat43Pstd = new DenseVector(new double[]{
            1.6302297817E+01, 2.0828735829E+00, 1.9566123451E-01, 6.8761936385E-01
        });

        private readonly Vector<double> Rat43Start1 = new DenseVector(new double[] { 100, 10, 1, 1 });
        private readonly Vector<double> Rat43Start2 = new DenseVector(new double[] { 700, 5, 0.75, 1.3 });

        [Test]
        public void Rat43_LM_Dif()
        {
            var obj = ObjectiveFunction.NonlinearModel(Rat43Model, Rat43X, Rat43Y, accuracyOrder: 6);
            var solver = new LevenbergMarquardtMinimizer(initialMu: 0.0001);
            var result = solver.FindMinimum(obj, Rat43Start1);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(Rat43Pbest[i], result.MinimizingPoint[i], 4);
                AssertHelpers.AlmostEqualRelative(Rat43Pstd[i], result.StandardErrors[i], 4);
            }
        }

        [Test]
        public void Rat43_TRDL_Dif()
        {
            var obj = ObjectiveFunction.NonlinearModel(Rat43Model, Rat43X, Rat43Y, accuracyOrder: 6);
            var solver = new TrustRegionDogLegMinimizer();
            var result = solver.FindMinimum(obj, Rat43Start2);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(Rat43Pbest[i], result.MinimizingPoint[i], 2);
                AssertHelpers.AlmostEqualRelative(Rat43Pstd[i], result.StandardErrors[i], 2);
            }
        }

        [Test]
        public void Rat43_TRNCG_Dif()
        {
            var obj = ObjectiveFunction.NonlinearModel(Rat43Model, Rat43X, Rat43Y, accuracyOrder: 6);
            var solver = new TrustRegionNewtonCGMinimizer();
            var result = solver.FindMinimum(obj, Rat43Start2);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(Rat43Pbest[i], result.MinimizingPoint[i], 2);
                AssertHelpers.AlmostEqualRelative(Rat43Pstd[i], result.StandardErrors[i], 2);
            }
        }

        [Test]
        public void Rat43_Bfgs_Dif()
        {
            var obj = ObjectiveFunction.NonlinearFunction(Rat43Model, Rat43X, Rat43Y, accuracyOrder: 6);
            var solver = new BfgsMinimizer(1e-10, 1e-10, 1e-10, 1000);
            var result = solver.FindMinimum(obj, Rat43Start2);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(Rat43Pbest[i], result.MinimizingPoint[i], 2);
            }
        }

        [Test]
        public void Rat43_LBfgs_Dif()
        {
            var obj = ObjectiveFunction.NonlinearFunction(Rat43Model, Rat43X, Rat43Y, accuracyOrder: 6);
            var solver = new LimitedMemoryBfgsMinimizer(1e-10, 1e-10, 1e-10, 1000);
            var result = solver.FindMinimum(obj, Rat43Start2);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(Rat43Pbest[i], result.MinimizingPoint[i], 2);
            }
        }

        #endregion Rat43

        #region BoxBod

        // model: BoxBod (https://www.itl.nist.gov/div898/strd/nls/data/boxbod.shtml)
        //       f(x; a, b) = a*(1 - exp(-b*x))
        // derivatives:
        //       df/da = 1 - exp(-b*x)
        //       df/db = a*x*exp(-b*x)
        // best fitted parameters:
        //       a = 2.1380940889E+02 +/- 1.2354515176E+01
        //       b = 5.4723748542E-01 +/- 1.0455993237E-01

        private Vector<double> BoxBodModel(Vector<double> p, Vector<double> x)
        {
            var y = CreateVector.Dense<double>(x.Count);
            for (var i = 0; i < x.Count; i++)
            {
                y[i] = p[0] * (1.0 - Math.Exp(-p[1] * x[i]));
            }
            return y;
        }
        private Matrix<double> BoxBodPrime(Vector<double> p, Vector<double> x)
        {
            var prime = Matrix<double>.Build.Dense(x.Count, p.Count);
            for (var i = 0; i < x.Count; i++)
            {
                prime[i, 0] = 1.0 - Math.Exp(-p[1] * x[i]);
                prime[i, 1] = p[0] * x[i] * Math.Exp(-p[1] * x[i]);
            }
            return prime;
        }
        private readonly Vector<double> BoxBodX = new DenseVector(new double[] { 1, 2, 3, 5, 7, 10 });
        private readonly Vector<double> BoxBodY = new DenseVector(new double[] { 109, 149, 149, 191, 213, 224 });
        private readonly Vector<double> BoxBodPbest = new DenseVector(new double[] { 2.1380940889E+02, 5.4723748542E-01 });
        private readonly Vector<double> BoxBodPstd = new DenseVector(new double[] { 1.2354515176E+01, 1.0455993237E-01 });

        private readonly Vector<double> BoxBodStart1 = new DenseVector(new double[] { 1.0, 1.0 });
        private readonly Vector<double> BoxBodStart2 = new DenseVector(new double[] { 100.0, 0.75 });
        private readonly Vector<double> BoxBodLowerBound = new DenseVector(new double[] { -1000, -100 });
        private readonly Vector<double> BoxBodUpperBound = new DenseVector(new double[] { 1000.0, 100 });
        private readonly Vector<double> BoxBodScales = new DenseVector(new double[] { 100.0, 0.1 });

        [Test]
        public void BoxBod_LM_Der()
        {
            var obj = ObjectiveFunction.NonlinearModel(BoxBodModel, BoxBodPrime, BoxBodX, BoxBodY);
            var solver = new LevenbergMarquardtMinimizer();

            // unconstrained
            var result = solver.FindMinimum(obj, BoxBodStart1);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 6);
            }

            // lower < parameters < upper
            // Note that in this case, scales have no effect.
            result = solver.FindMinimum(obj, BoxBodStart1, lowerBound: BoxBodLowerBound, upperBound: BoxBodUpperBound);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 6);
            }

            // lower < parameters, no scales
            result = solver.FindMinimum(obj, BoxBodStart1, lowerBound: BoxBodLowerBound);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 6);
            }

            // lower < parameters, scales
            result = solver.FindMinimum(obj, BoxBodStart1, lowerBound: BoxBodLowerBound, scales: BoxBodScales);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 6);
            }

            // parameters < upper, no scales
            result = solver.FindMinimum(obj, BoxBodStart1, upperBound: BoxBodUpperBound);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 6);
            }

            // parameters < upper, scales
            result = solver.FindMinimum(obj, BoxBodStart1, upperBound: BoxBodUpperBound, scales: BoxBodScales);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 6);
            }

            // only scales
            result = solver.FindMinimum(obj, BoxBodStart1, scales: BoxBodScales);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 6);
            }
        }

        [Test]
        public void BoxBod_LM_Dif()
        {
            var obj = ObjectiveFunction.NonlinearModel(BoxBodModel, BoxBodX, BoxBodY);
            var solver = new LevenbergMarquardtMinimizer();

            // unconstrained
            var result = solver.FindMinimum(obj, BoxBodStart1);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 6);
            }

            // box constrained
            result = solver.FindMinimum(obj, BoxBodStart1, lowerBound: BoxBodLowerBound, upperBound: BoxBodUpperBound);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 6);
            }
        }

        [Test]
        public void BoxBod_TRDL_Dif()
        {
            var obj = ObjectiveFunction.NonlinearModel(BoxBodModel, BoxBodX, BoxBodY);
            var solver = new TrustRegionDogLegMinimizer();
            var result = solver.FindMinimum(obj, BoxBodStart1);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 3);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 3);
            }
        }

        [Test]
        public void BoxBod_TRNCG_Dif()
        {
            var obj = ObjectiveFunction.NonlinearModel(BoxBodModel, BoxBodX, BoxBodY);
            var solver = new TrustRegionNewtonCGMinimizer();
            var result = solver.FindMinimum(obj, BoxBodStart2);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 3);
                AssertHelpers.AlmostEqualRelative(BoxBodPstd[i], result.StandardErrors[i], 3);
            }
        }

        [Test]
        public void BoxBod_Bfgs_Der()
        {
            var obj = ObjectiveFunction.NonlinearFunction(BoxBodModel, BoxBodPrime, BoxBodX, BoxBodY);
            var solver = new BfgsMinimizer(1e-10, 1e-10, 1e-10, 100);
            var result = solver.FindMinimum(obj, BoxBodStart2);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
            }
        }

        [Test]
        public void BoxBod_Newton_Der()
        {
            var obj = ObjectiveFunction.NonlinearFunction(BoxBodModel, BoxBodPrime, BoxBodX, BoxBodY);
            var solver = new NewtonMinimizer(1e-10, 100);
            var result = solver.FindMinimum(obj, BoxBodStart2);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(BoxBodPbest[i], result.MinimizingPoint[i], 6);
            }
        }

        #endregion BoxBod

        #region Thurber

        // model : Thurber (https://www.itl.nist.gov/div898/strd/nls/data/thurber.shtml)
        //       f(x; b1 ... b7) = (b1 + b2*x + b3*x^2 + b4*x^3) / (1 + b5*x + b6*x^2 + b7*x^3)
        // derivatives:
        //       df/db1 = 1/(b5*x + b6*x^2 + b7*x^3 + 1)
        //       df/db2 = x/(b5*x + b6*x^2 + b7*x^3 + 1)
        //       df/db3 = x^2/(b5*x + b6*x^2 + b7*x^3 + 1)
        //       df/db4 = x^3/(b5*x + b6*x^2 + b7*x^3 + 1)
        //       df/db5 = -(x*(b1 + x*(b2 + x*(b3 + b4*x))))/(b5*x + b6*x^2 + b7*x^3 + 1)^2
        //       df/db6 = -(x^2*(b1 + x*(b2 + x*(b3 + b4*x))))/(b5*x + b6*x^2 + b7*x^3 + 1)^2
        //       df/db7 = -(x^3*(b1 + x*(b2 + x*(b3 + b4*x))))/(b5*x + b6*x^2 + b7*x^3 + 1)^2
        // best fitted parameters:
        //       b1 = 1.2881396800E+03 +/- 4.6647963344E+00
        //       b2 = 1.4910792535E+03 +/- 3.9571156086E+01
        //       b3 = 5.8323836877E+02 +/- 2.8698696102E+01
        //       b4 = 7.5416644291E+01 +/- 5.5675370270E+00
        //       b5 = 9.6629502864E-01 +/- 3.1333340687E-02
        //       b6 = 3.9797285797E-01 +/- 1.4984928198E-02
        //       b7 = 4.9727297349E-02 +/- 6.5842344623E-03
        private Vector<double> ThurberModel(Vector<double> p, Vector<double> x)
        {
            var y = CreateVector.Dense<double>(x.Count);
            for (var i = 0; i < x.Count; i++)
            {
                var xSq = x[i] * x[i];
                var xCb = xSq * x[i];

                y[i] = (p[0] + p[1] * x[i] + p[2] * xSq + p[3] * xCb)
                        / (1 + p[4] * x[i] + p[5] * xSq + p[6] * xCb);
            }
            return y;
        }
        private Matrix<double> ThurberPrime(Vector<double> p, Vector<double> x)
        {
            var prime = Matrix<double>.Build.Dense(x.Count, p.Count);
            for (var i = 0; i < x.Count; i++)
            {
                var xSq = x[i] * x[i];
                var xCb = xSq * x[i];
                var num = p[0] + x[i] * (p[1] + x[i] * (p[2] + p[3] * x[i]));
                var den = p[4] * x[i] + p[5] * xSq + p[6] * xCb + 1.0;
                var denSq = den * den;

                prime[i, 0] = 1.0 / den;
                prime[i, 1] = x[i] / den;
                prime[i, 2] = xSq / den;
                prime[i, 3] = xCb / den;
                prime[i, 4] = -(x[i] * num) / denSq;
                prime[i, 5] = -(xSq * num) / denSq;
                prime[i, 6] = -(xCb * num) / denSq;
            }
            return prime;
        }
        private readonly Vector<double> ThurberX = new DenseVector(new double[] {
            -3.067, -2.981, -2.921, -2.912, -2.84,
            -2.797, -2.702, -2.699, -2.633, -2.481,
            -2.363, -2.322, -1.501, -1.460, -1.274,
            -1.212, -1.100, -1.046, -0.915, -0.714,
            -0.566, -0.545, -0.400, -0.309, -0.109,
            -0.103, 0.01,   0.119,  0.377,  0.79,
            0.963,  1.006,  1.115,  1.572,  1.841,
            2.047,  2.2});
        private readonly Vector<double> ThurberY = new DenseVector(new double[] {
             80.574,    084.248,    087.264,    087.195,    089.076,
             089.608,   089.868,    090.101,    092.405,    095.854,
             100.696,   101.060,    401.672,    390.724,    567.534,
             635.316,   733.054,    759.087,    894.206,    990.785,
            1090.109,   1080.914,   1122.643,   1178.351,   1260.531,
            1273.514,   1288.339,   1327.543,   1353.863,   1414.509,
            1425.208,   1421.384,   1442.962,   1464.350,   1468.705,
            1447.894,   1457.628});
        private readonly Vector<double> ThurberPbest = new DenseVector(new double[] {
            1.2881396800E+03, 1.4910792535E+03, 5.8323836877E+02, 7.5416644291E+01, 9.6629502864E-01,
            3.9797285797E-01, 4.9727297349E-02 });
        private readonly Vector<double> ThurberPstd = new DenseVector(new double[] {
            4.6647963344E+00, 3.9571156086E+01, 2.8698696102E+01, 5.5675370270E+00, 3.1333340687E-02,
            1.4984928198E-02, 6.5842344623E-03 });
        private readonly Vector<double> ThurberStart = new DenseVector(new double[] { 1000.0, 1000.0, 400.0, 40.0, 0.7, 0.3, 0.03 });
        private readonly Vector<double> ThurberLowerBound = new DenseVector(new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });
        private readonly Vector<double> ThurberUpperBound = new DenseVector(new double[] { 1E6, 1E6, 1E6, 1E6, 1E6, 1E6, 1E6 });
        private readonly Vector<double> ThurberScales = new DenseVector(new double[7] { 1000, 1000, 400, 40, 0.7, 0.3, 0.03 });

        // NOTE: Higher accuracyOrder (e.g., 6) may lead to failure due to numerical instability
        // (round-off errors) for this model.

        [Test]
        public void Thurber_LM_Der()
        {
            var obj = ObjectiveFunction.NonlinearModel(ThurberModel, ThurberPrime, ThurberX, ThurberY);
            var solver = new LevenbergMarquardtMinimizer();
            var result = solver.FindMinimum(obj, ThurberStart);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.MinimizingPoint[i], 4);
                AssertHelpers.AlmostEqualRelative(ThurberPstd[i], result.StandardErrors[i], 4);
            }
        }

        [Test]
        public void Thurber_LM_Dif()
        {
            // NOTE: Higher accuracyOrder (e.g., 6) may lead to failure due to numerical instability
            // (round-off errors) for this model.

            var obj = ObjectiveFunction.NonlinearModel(ThurberModel, ThurberX, ThurberY);
            var solver = new LevenbergMarquardtMinimizer();
            var result = solver.FindMinimum(obj, ThurberStart);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.MinimizingPoint[i], 1);
                AssertHelpers.AlmostEqualRelative(ThurberPstd[i], result.StandardErrors[i], 1);
            }
        }

#if !MKL
        // TODO: Fails with MKL, to be investigated
        [Test]
#endif
        public void Thurber_TRDL_Dif()
        {
            var obj = ObjectiveFunction.NonlinearModel(ThurberModel, ThurberX, ThurberY, accuracyOrder: 1);
            var solver = new TrustRegionDogLegMinimizer();
            var result = solver.FindMinimum(obj, ThurberStart, scales: ThurberScales);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.MinimizingPoint[i], 3);
                AssertHelpers.AlmostEqualRelative(ThurberPstd[i], result.StandardErrors[i], 3);
            }
        }

        [Test]
        public void Thurber_TRNCG_Dif()
        {
            var obj = ObjectiveFunction.NonlinearModel(ThurberModel, ThurberX, ThurberY);
            var solver = new TrustRegionNewtonCGMinimizer();
            var result = solver.FindMinimum(obj, ThurberStart, scales: ThurberScales);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.MinimizingPoint[i], 3);
                AssertHelpers.AlmostEqualRelative(ThurberPstd[i], result.StandardErrors[i], 3);
            }
        }

        [Test]
        public void Thurber_Bfgs_Dif()
        {
            var obj = ObjectiveFunction.NonlinearFunction(ThurberModel, ThurberX, ThurberY);
            var solver = new BfgsMinimizer(1e-10, 1e-10, 1e-10, 1000);
            var result = solver.FindMinimum(obj, ThurberStart);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.MinimizingPoint[i], 6);
            }
        }

        [Test]
        public void Thurber_BfgsB_Dif()
        {
            var obj = ObjectiveFunction.NonlinearFunction(ThurberModel, ThurberX, ThurberY);
            var solver = new BfgsBMinimizer(1e-10, 1e-10, 1e-10, 1000);
            var result = solver.FindMinimum(obj, ThurberLowerBound, ThurberUpperBound, ThurberStart);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.MinimizingPoint[i], 6);
            }
        }

        [Test]
        public void Thurber_LBfgs_Dif()
        {
            var obj = ObjectiveFunction.NonlinearFunction(ThurberModel, ThurberX, ThurberY);
            var solver = new LimitedMemoryBfgsMinimizer(1e-10, 1e-10, 1e-10, 1000);
            var result = solver.FindMinimum(obj, ThurberStart);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.MinimizingPoint[i], 6);
            }
        }

        #endregion Thurber

        #region Weighted Nonlinear Regression

        // Data from https://www.mathworks.com/help/stats/examples/weighted-nonlinear-regression.html

        private Vector<double> PollutionModel(Vector<double> p, Vector<double> x)
        {
            var y = CreateVector.Dense<double>(x.Count);
            for (var i = 0; i < x.Count; i++)
            {
                y[i] = p[0] * (1.0 - Math.Exp(-p[1] * x[i]));
            }
            return y;
        }

        private readonly Vector<double> PollutionX = new DenseVector(new double[] { 1, 2, 3, 5, 7, 10 });
        private readonly Vector<double> PollutionY = new DenseVector(new double[] { 109, 149, 149, 191, 213, 224 });
        private readonly Vector<double> PollutionW = new DenseVector(new double[] { 1, 1, 5, 5, 5, 5 });
        private readonly Vector<double> PollutionStart = new DenseVector(new double[] { 240, 0.5 });
        private readonly Vector<double> PollutionBest = new DenseVector(new double[] { 225.17, 0.40078 });

        [Test]
        public void PollutionWithWeights()
        {
            var obj = ObjectiveFunction.NonlinearModel(PollutionModel, PollutionX, PollutionY, PollutionW, accuracyOrder: 2);
            var solver = new LevenbergMarquardtMinimizer();
            var result = solver.FindMinimum(obj, PollutionStart);

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(PollutionBest[i], result.MinimizingPoint[i], 4);
            }

            // Check statistics
            AssertHelpers.AlmostEqualRelative(0.908, result.RSquared, 2);
            AssertHelpers.AlmostEqualRelative(0.885, result.AdjustedRSquared, 2);
            AssertHelpers.AlmostEqualRelative(24.0096, result.StandardError, 2);

            // Check parameter statistics (using expected values)
            var expectedStdErrors = new double[] { 10.7, 0.064296 };
            var expectedTStats = new double[] { 21.045, 6.2333 };
            var expectedPValues = new double[] { 3.0134e-05, 0.0033745 };
            // Expected confidence interval bounds
            var expectedCI = new double[,]
                {
                    { 195.4650, 254.8788 },
                    { 0.2223, 0.5793 }
                };

            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(expectedStdErrors[i], result.StandardErrors[i], 3);
                AssertHelpers.AlmostEqualRelative(expectedTStats[i], result.TStatistics[i], 3);
                AssertHelpers.AlmostEqualRelative(expectedPValues[i], result.PValues[i], 3);

                // Calculate and check confidence interval bounds
                var lowerBound = result.MinimizingPoint[i] - result.ConfidenceIntervalHalfWidths[i];
                var upperBound = result.MinimizingPoint[i] + result.ConfidenceIntervalHalfWidths[i];

                AssertHelpers.AlmostEqualRelative(expectedCI[i, 0], lowerBound, 3);
                AssertHelpers.AlmostEqualRelative(expectedCI[i, 1], upperBound, 3);
            }
        }

        #endregion Weighted Nonlinear Regression

        #region Multivariate Nonlinear Regression (Direct Residual)

        [Test]
        public void Circle_LM_WithDirectResidual()
        {
            // Points on a circle in (x,y) coordinate format
            double[] x = { 5, 0, -5, 0, 3.5355, -3.5355, -3.5355, 3.5355 };
            double[] y = { 0, 5, 0, -5, 3.5355, 3.5355, -3.5355, -3.5355 };

            // Define direct residual function
            Vector<double> residualFunc(Vector<double> p)
            {
                var a = p[0];
                var b = p[1];
                var r = p[2];

                var residuals = Vector<double>.Build.Dense(x.Length);
                for (var i = 0; i < x.Length; i++)
                {
                    residuals[i] = Math.Pow(x[i] - a, 2) + Math.Pow(y[i] - b, 2) - r * r;
                }
                return residuals;
            }

            // Define Jacobian function
            Matrix<double> jacobianFunc(Vector<double> p)
            {
                var a = p[0];
                var b = p[1];
                var r = p[2];

                var jacobian = Matrix<double>.Build.Dense(x.Length, 3);
                for (var i = 0; i < x.Length; i++)
                {
                    jacobian[i, 0] = -2 * (x[i] - a);
                    jacobian[i, 1] = -2 * (y[i] - b);
                    jacobian[i, 2] = -2 * r;
                }
                return jacobian;
            }

            // Initial parameter guess [a, b, r] = [1, 1, 3]
            var initialGuess = Vector<double>.Build.Dense(new[] { 1.0, 1.0, 3.0 });

            // Expected result [a, b, r] = [0, 0, 5]
            var expectedResult = Vector<double>.Build.Dense(new[] { 0.0, 0.0, 5.0 });

            // Create NonlinearObjectiveModel
            var obj = ObjectiveFunction.NonlinearModel(residualFunc, jacobianFunc);

            var solver = new LevenbergMarquardtMinimizer();
            var result = solver.FindMinimum(obj, initialGuess);

            // Verify results
            for (var i = 0; i < result.MinimizingPoint.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(expectedResult[i], result.MinimizingPoint[i], 4);
            }
        }

        #endregion Multivariate Nonlinear Regression (Direct Residual)
    }
}
