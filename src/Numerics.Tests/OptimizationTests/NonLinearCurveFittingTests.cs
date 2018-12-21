using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using NUnit.Framework;
using System;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    [TestFixture]
    public class NonLinearCurveFittingTests
    {
        // model: Rosenbrock
        //       f(x; a, b) = (1 - a)^2 + 100*(b - a^2)^2
        // derivatives:
        //       df/da = 400*a^3 - 400*a*b + 2*a - 2
        //       df/db = 200*(b - a^2)
        // best fitted parameters:
        //       a = 1
        //       b = 1
        private double RosenbrockModel(Vector<double> p, double x)
        {
            var y = Math.Pow(1.0 - p[0], 2) + 100.0 * Math.Pow(p[1] - p[0] * p[0], 2);
            return y;
        }
        private Vector<double> RosenbrockPrime(Vector<double> p, double x)
        {
            var prime = Vector<double>.Build.Dense(p.Count);
            prime[0] = 400.0 * p[0] * p[0] * p[0] - 400.0 * p[0] * p[1] + 2.0 * p[0] - 2.0;
            prime[1] = 200.0 * (p[1] - p[0] * p[0]);
            return prime;
        }
        private Vector<double> RosenbrockX = Vector<double>.Build.Dense(2);
        private Vector<double> RosenbrockY = Vector<double>.Build.Dense(2);
        private Vector<double> RosenbrockPbest = new DenseVector(new double[] { 1.0, 1.0 });

        private Vector<double> RosenbrockStart1 = new DenseVector(new double[] { -1.2, 1.0 });
        private Vector<double> RosebbrockLowerBound = new DenseVector(new double[] { -5.0, -5.0 });
        private Vector<double> RosenbrockUpperBound = new DenseVector(new double[] { 5.0, 5.0 });

        [Test]
        public void LMDER_FindMinimum_Rosenbrock_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(RosenbrockModel, RosenbrockPrime, RosenbrockX, RosenbrockY);
            var solver = new LevenbergMarquardtMinimizer(maximumIterations: 10000);

            var result = solver.FindMinimum(obj, RosenbrockStart1);

            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[0], result.BestFitParameters[0], 3);
            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[1], result.BestFitParameters[1], 3);
        }

        [Test]
        public void LMDIF_FindMinimum_Rosenbrock_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(RosenbrockModel, RosenbrockX, RosenbrockY, accuracyOrder:2);
            var solver = new LevenbergMarquardtMinimizer(maximumIterations: 10000);

            var result = solver.FindMinimum(obj, RosenbrockStart1);
            
            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[0], result.BestFitParameters[0], 3);
            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[1], result.BestFitParameters[1], 3);
        }

        [Test]
        public void LMDER_FindMinimum_Rosenbrock_BoxConstrained()
        {
            var obj = ObjectiveModel.FittingModel(RosenbrockModel, RosenbrockPrime, RosenbrockX, RosenbrockY,
                lowerBound : RosebbrockLowerBound, upperBound : RosenbrockUpperBound);
            var solver = new LevenbergMarquardtMinimizer(maximumIterations: 10000);

            var result = solver.FindMinimum(obj, RosenbrockStart1);

            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[0], result.BestFitParameters[0], 3);
            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[1], result.BestFitParameters[1], 3);
        }

        [Test]
        public void LMDIF_FindMinimum_Rosenbrock_BoxConstrained()
        {
            var obj = ObjectiveModel.FittingModel(RosenbrockModel, RosenbrockX, RosenbrockY,
                lowerBound: RosebbrockLowerBound, upperBound: RosenbrockUpperBound,
                accuracyOrder: 2);
            var solver = new LevenbergMarquardtMinimizer(maximumIterations: 10000);

            var result = solver.FindMinimum(obj, RosenbrockStart1);

            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[0], result.BestFitParameters[0], 3);
            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[1], result.BestFitParameters[1], 3);
        }

        [Test]
        public void TRLMDER_FindMinimum_Rosenbrock_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(RosenbrockModel, RosenbrockPrime, RosenbrockX, RosenbrockY);
            var solver = new TrustRegionDogLegMinimizer(maximumIterations: 10000);

            var result = solver.FindMinimum(obj, RosenbrockStart1);

            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[0], result.BestFitParameters[0], 1);
            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[1], result.BestFitParameters[1], 1);
        }

        [Test]
        public void TRLMDIF_FindMinimum_Rosenbrock_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(RosenbrockModel, RosenbrockPrime, RosenbrockX, RosenbrockY);
            var solver = new TrustRegionDogLegMinimizer(maximumIterations: 10000);

            var result = solver.FindMinimum(obj, RosenbrockStart1);

            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[0], result.BestFitParameters[0], 1);
            AssertHelpers.AlmostEqualRelative(RosenbrockPbest[1], result.BestFitParameters[1], 1);
        }

        // model: Rat43 (https://www.itl.nist.gov/div898/strd/nls/data/ratkowsky3.shtml)
        //       f(x; a, b, c, d) =  a / ((1 + exp(b - c * x))^(1 / d))
        // best fitted parameters:
        //       a = 6.9964151270E+02 +/- 1.6302297817E+01
        //       b = 5.2771253025E+00 +/- 2.0828735829E+00
        //       c = 7.5962938329E-01 +/- 1.9566123451E-01
        //       d = 1.2792483859E+00 +/- 6.8761936385E-01
        private double Rat43Model(Vector<double> p, double x)
        {
            var y = p[0] / Math.Pow(1.0 + Math.Exp(p[1] - p[2] * x), 1.0 / p[3]);
            return y;
        }
        private Vector<double> Rat43X = new DenseVector(new double[] {
            1.00,   2.00,   3.00,   4.00,   5.00,   6.00,   7.00,   8.00,   9.00,   10.00,
            11.00,  12.00,  13.00,  14.00,  15.00
        });
        private Vector<double> Rat43Y = new DenseVector(new double[] {
             16.08, 33.83,  65.80,  97.20,  191.55, 326.20, 386.87, 520.53, 590.03, 651.92,
            724.93, 699.56, 689.96, 637.56, 717.41
        });
        private Vector<double> Rat43Pbest = new DenseVector(new double[] {
            6.9964151270E+02, 5.2771253025E+00, 7.5962938329E-01, 1.2792483859E+00
        });
        private Vector<double> Rat43Pstd = new DenseVector(new double[]{
            1.6302297817E+01, 2.0828735829E+00, 1.9566123451E-01, 6.8761936385E-01
        });

        private Vector<double> Rat43Start1 = new DenseVector(new double[] { 100, 10, 1, 1 });
        private Vector<double> Rat43Start2 = new DenseVector(new double[] { 700, 5, 0.75, 1.3 });

        [Test]
        public void LMDIF_FindMinimum_Rat43_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(Rat43Model, Rat43X, Rat43Y, accuracyOrder: 6);
            var solver = new LevenbergMarquardtMinimizer();

            var result = solver.FindMinimum(obj, Rat43Start1);

            for (int i = 0; i < result.BestFitParameters.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(Rat43Pbest[i], result.BestFitParameters[i], 6);
                AssertHelpers.AlmostEqualRelative(Rat43Pstd[i], result.StandardErrors[i], 6);
            }
        }

        [Test]
        public void TRLMDIF_FindMinimum_Rat43_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(Rat43Model, Rat43X, Rat43Y, accuracyOrder: 6);
            var solver = new TrustRegionDogLegMinimizer();

            var result = solver.FindMinimum(obj, Rat43Start2);

            for (int i = 0; i < result.BestFitParameters.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(Rat43Pbest[i], result.BestFitParameters[i], 2);
                AssertHelpers.AlmostEqualRelative(Rat43Pstd[i], result.StandardErrors[i], 2);
            }
        }

        // model: BoxBod (https://www.itl.nist.gov/div898/strd/nls/data/boxbod.shtml)
        //       f(x; a, b) = a*(1 - exp(-b*x))
        // derivatives:
        //       df/da = 1 - exp(-b*x)
        //       df/db = a*x*exp(-b*x)
        // best fitted parameters:
        //       a = 2.1380940889E+02 +/- 1.2354515176E+01
        //       b = 5.4723748542E-01 +/- 1.0455993237E-01
        private double BoxBodModel(Vector<double> p, double x)
        {
            var y = p[0] * (1.0 - Math.Exp(-p[1] * x));
            return y;
        }
        private Vector<double> BoxBodPrime(Vector<double> p, double x)
        {
            var prime = Vector<double>.Build.Dense(p.Count);
            prime[0] = 1.0 - Math.Exp(-p[1] * x);
            prime[1] = p[0] * x * Math.Exp(-p[1] * x);
            return prime;
        }
        private Vector<double> BoxBodX = new DenseVector(new double[] { 1, 2, 3, 5, 7, 10 });
        private Vector<double> BoxBodY = new DenseVector(new double[] { 109, 149, 149, 191, 213, 224 });
        private Vector<double> BoxBodPbest = new DenseVector(new double[] { 2.1380940889E+02, 5.4723748542E-01 });
        private Vector<double> BoxBodPstd = new DenseVector(new double[] { 1.2354515176E+01, 1.0455993237E-01 });

        private Vector<double> BoxBodStart1 = new DenseVector(new double[] { 1.0, 1.0 });
        private Vector<double> BoxBodStart2 = new DenseVector(new double[] { 100.0, 0.75 });
        private Vector<double> BoxBodLowerBound = new DenseVector(new double[] { 0, 0 });
        private Vector<double> BoxBodUpperBound = new DenseVector(new double[] { 500.0, 10 });
        private Vector<double> BoxBodScales = new DenseVector(new double[] { 100.0, 1 });

        [Test]
        public void LMDER_FindMinimum_BoxBod_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(BoxBodModel, BoxBodPrime, BoxBodX, BoxBodY);
            var solver = new LevenbergMarquardtMinimizer();

            var result = solver.FindMinimum(obj, BoxBodStart1);

            AssertHelpers.AlmostEqualRelative(BoxBodPbest[0], result.BestFitParameters[0], 6);
            AssertHelpers.AlmostEqualRelative(BoxBodPbest[1], result.BestFitParameters[1], 6);

            AssertHelpers.AlmostEqualRelative(BoxBodPstd[0], result.StandardErrors[0], 6);
            AssertHelpers.AlmostEqualRelative(BoxBodPstd[1], result.StandardErrors[1], 6);
        }

        [Test]
        public void LMDIF_FindMinimum_BoxBod_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(BoxBodModel, BoxBodX, BoxBodY, accuracyOrder:6);
            var solver = new LevenbergMarquardtMinimizer();

            var result = solver.FindMinimum(obj, BoxBodStart1);

            AssertHelpers.AlmostEqualRelative(BoxBodPbest[0], result.BestFitParameters[0], 6);
            AssertHelpers.AlmostEqualRelative(BoxBodPbest[1], result.BestFitParameters[1], 6);

            AssertHelpers.AlmostEqualRelative(BoxBodPstd[0], result.StandardErrors[0], 6);
            AssertHelpers.AlmostEqualRelative(BoxBodPstd[1], result.StandardErrors[1], 6);
        }

        [Test]
        public void LMDER_FindMinimum_BoxBod_BoxConstrained()
        {
            var obj = ObjectiveModel.FittingModel(BoxBodModel, BoxBodPrime, BoxBodX, BoxBodY,
                lowerBound: BoxBodLowerBound, upperBound: BoxBodUpperBound);
            var solver = new LevenbergMarquardtMinimizer();

            var result = solver.FindMinimum(obj, BoxBodStart1);

            AssertHelpers.AlmostEqualRelative(BoxBodPbest[0], result.BestFitParameters[0], 6);
            AssertHelpers.AlmostEqualRelative(BoxBodPbest[1], result.BestFitParameters[1], 6);

            AssertHelpers.AlmostEqualRelative(BoxBodPstd[0], result.StandardErrors[0], 6);
            AssertHelpers.AlmostEqualRelative(BoxBodPstd[1], result.StandardErrors[1], 6);
        }

        [Test]
        public void LMDIF_FindMinimum_BoxBod_BoxConstrained()
        {
            var obj = ObjectiveModel.FittingModel(BoxBodModel, BoxBodX, BoxBodY,
                lowerBound: BoxBodLowerBound, upperBound: BoxBodUpperBound,
                accuracyOrder: 6);
            var solver = new LevenbergMarquardtMinimizer();

            var result = solver.FindMinimum(obj, BoxBodStart1);

            AssertHelpers.AlmostEqualRelative(BoxBodPbest[0], result.BestFitParameters[0], 6);
            AssertHelpers.AlmostEqualRelative(BoxBodPbest[1], result.BestFitParameters[1], 6);

            AssertHelpers.AlmostEqualRelative(BoxBodPstd[0], result.StandardErrors[0], 6);
            AssertHelpers.AlmostEqualRelative(BoxBodPstd[1], result.StandardErrors[1], 6);
        }

        [Test]
        public void TRLMDIF_FindMinimum_BoxBod_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(BoxBodModel, BoxBodX, BoxBodY, accuracyOrder: 6);
            var solver = new TrustRegionDogLegMinimizer();

            var result = solver.FindMinimum(obj, BoxBodStart1);

            AssertHelpers.AlmostEqualRelative(BoxBodPbest[0], result.BestFitParameters[0], 3);
            AssertHelpers.AlmostEqualRelative(BoxBodPbest[1], result.BestFitParameters[1], 3);

            AssertHelpers.AlmostEqualRelative(BoxBodPstd[0], result.StandardErrors[0], 3);
            AssertHelpers.AlmostEqualRelative(BoxBodPstd[1], result.StandardErrors[1], 3);
        }

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
        private double ThurberModel(Vector<double> p, double x)
        {
            var xSq = x * x;
            var xCb = xSq * x;

            var y = (p[0] + p[1] * x + p[2] * xSq + p[3] * xCb)
                    / (1 + p[4] * x + p[5] * xSq + p[6] * xCb);
            return y;
        }
        private Vector<double> ThurberPrime(Vector<double> p, double x)
        {
            var prime = Vector<double>.Build.Dense(p.Count);

            var xSq = x * x;
            var xCb = xSq * x;
            var num = (p[0] + x * (p[1] + x * (p[2] + p[3] * x)));
            var den = (p[4] * x + p[5] * xSq + p[6] * xCb + 1.0);
            var denSq = den * den;

            prime[0] = 1.0 / den;
            prime[1] = x / den;
            prime[2] = xSq / den;
            prime[3] = xCb / den;
            prime[4] = -(x * num) / denSq;
            prime[5] = -(xSq * num) / denSq;
            prime[6] = -(xCb * num) / denSq;
            return prime;
        }
        private Vector<double> ThurberX = new DenseVector(new double[] {
            -3.067, -2.981, -2.921, -2.912, -2.84,
            -2.797, -2.702, -2.699, -2.633, -2.481,
            -2.363, -2.322, -1.501, -1.460, -1.274,
            -1.212, -1.100, -1.046, -0.915, -0.714,
            -0.566, -0.545, -0.400, -0.309, -0.109,
            -0.103, 0.01,   0.119,  0.377,  0.79,
            0.963,  1.006,  1.115,  1.572,  1.841,
            2.047,  2.2});
        private Vector<double> ThurberY = new DenseVector(new double[] {
             80.574,    084.248,    087.264,    087.195,    089.076,
             089.608,   089.868,    090.101,    092.405,    095.854,
             100.696,   101.060,    401.672,    390.724,    567.534,
             635.316,   733.054,    759.087,    894.206,    990.785,
            1090.109,   1080.914,   1122.643,   1178.351,   1260.531,
            1273.514,   1288.339,   1327.543,   1353.863,   1414.509,
            1425.208,   1421.384,   1442.962,   1464.350,   1468.705,
            1447.894,   1457.628});
        private Vector<double> ThurberPbest = new DenseVector(new double[] {
            1.2881396800E+03, 1.4910792535E+03, 5.8323836877E+02, 7.5416644291E+01, 9.6629502864E-01,
            3.9797285797E-01, 4.9727297349E-02 });
        private Vector<double> ThurberPstd = new DenseVector(new double[] {
            4.6647963344E+00, 3.9571156086E+01, 2.8698696102E+01, 5.5675370270E+00, 3.1333340687E-02,
            1.4984928198E-02, 6.5842344623E-03 });
        private Vector<double> ThurberInitialGuess = new DenseVector(new double[] { 1000.0, 1000.0, 400.0, 40.0, 0.7, 0.3, 0.03 });
        private Vector<double> ThurberScales = new DenseVector(new double[7] { 1000, 1000, 400, 40, 0.7, 0.3, 0.03 });

        [Test]
        public void LMDER_FindMinimum_Thurber_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(ThurberModel, ThurberPrime, ThurberX, ThurberY);
            var solver = new LevenbergMarquardtMinimizer();

            var result = solver.FindMinimum(obj, ThurberInitialGuess);

            for (int i = 0; i < result.BestFitParameters.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.BestFitParameters[i], 6);
                AssertHelpers.AlmostEqualRelative(ThurberPstd[i], result.StandardErrors[i], 6);
            }
        }

        [Test]
        public void LMDIF_FindMinimum_Thurber_Unconstrained()
        {
            var obj = ObjectiveModel.FittingModel(ThurberModel, ThurberX, ThurberY, accuracyOrder: 6);
            var solver = new LevenbergMarquardtMinimizer();

            var result = solver.FindMinimum(obj, ThurberInitialGuess);

            for (int i = 0; i < result.BestFitParameters.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.BestFitParameters[i], 6);
                AssertHelpers.AlmostEqualRelative(ThurberPstd[i], result.StandardErrors[i], 6);
            }
        }
        
        [Test]
        public void TRLMDIF_FindMinimum_Thurber_Scaled()
        {
            var obj = ObjectiveModel.FittingModel(ThurberModel, ThurberX, ThurberY,
                scales: ThurberScales,
                accuracyOrder: 6);
            var solver = new TrustRegionDogLegMinimizer();

            var result = solver.FindMinimum(obj, ThurberInitialGuess);

            for (int i = 0; i < result.BestFitParameters.Count; i++)
            {
                AssertHelpers.AlmostEqualRelative(ThurberPbest[i], result.BestFitParameters[i], 3);
                AssertHelpers.AlmostEqualRelative(ThurberPstd[i], result.StandardErrors[i], 3);
            }
        }
    }
}
