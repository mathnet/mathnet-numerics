// <copyright file="InterpolationInfrastuctureTest.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.UnitTests.InterpolationTests
{
    using System;
    using System.Linq.Expressions;
    using Interpolation;
    using Interpolation.Algorithms;
    using MbUnit.Framework;
    using MbUnit.Framework.ContractVerifiers;
    using Sampling;

    [TestFixture]
    public class InterpolationInfrastuctureTest
    {
        /**** Direct Algorithms (without precomputations) ****/

        [VerifyContract]
        public readonly IContract NevillePolynomialInfrastructureTests =
            new InterpolationInfrastructureContract<NevillePolynomialInterpolation>
            {
                MinimumSampleCount = 1,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new NevillePolynomialInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new NevillePolynomialInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10))
                    }
            };

        [VerifyContract]
        public readonly IContract BulirschStoerRationalInfrastructureTests =
            new InterpolationInfrastructureContract<BulirschStoerRationalInterpolation>
            {
                MinimumSampleCount = 1,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new BulirschStoerRationalInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new BulirschStoerRationalInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10)),
                        () => Interpolate.RationalWithPoles(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10))
                    }
            };

        /**** Barycentric Algorithms ****/

        [VerifyContract]
        public readonly IContract BarycentricInfrastructureTests =
            new InterpolationInfrastructureContract<BarycentricInterpolation>
            {
                MinimumSampleCount = 3,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new BarycentricInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new BarycentricInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10),
                                  Sample.EquidistantStartingAt(t => t, 1, 0.1, 10)),
                        () => new BarycentricInterpolation(
                                  Sample.EquidistantStartingAt(t => t, 5, -1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10),
                                  Sample.EquidistantStartingAt(t => t, 1, 0.1, 10))
                    }
            };

        [VerifyContract]
        public readonly IContract FloaterHormannRationalInfrastructureTests =
            new InterpolationInfrastructureContract<FloaterHormannRationalInterpolation>
            {
                MinimumSampleCount = 1,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new FloaterHormannRationalInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new FloaterHormannRationalInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10)),
                        () => new FloaterHormannRationalInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10),
                                  5),
                        () => Interpolate.RationalWithoutPoles(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10)),
                        () => Interpolate.Common(
                                  Sample.EquidistantStartingAt(t => t, 5, -1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10))
                    }
            };

        [VerifyContract]
        public readonly IContract EquidistantPolynomialInfrastructureTests =
            new InterpolationInfrastructureContract<EquidistantPolynomialInterpolation>
            {
                MinimumSampleCount = 1,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new EquidistantPolynomialInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new EquidistantPolynomialInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10)),
                        () => new EquidistantPolynomialInterpolation(
                                  Sample.EquidistantStartingAt(t => t, 5, -1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10)),
                        () => new EquidistantPolynomialInterpolation(
                                  -5,
                                  4,
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10))
                    }
            };

        /**** Spline Algorithms ****/

        [VerifyContract]
        public readonly IContract SplineInfrastructureTests =
            new InterpolationInfrastructureContract<SplineInterpolation>
            {
                MinimumSampleCount = 2,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new SplineInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new SplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 4 * (10 - 1))),
                        () => new SplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  AkimaSplineInterpolation.EvaluateSplineCoefficients(
                                      Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                      Sample.EquidistantStartingAt(t => t, -2, 0.5, 10))),
                        () => new SplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  CubicSplineInterpolation.EvaluateSplineCoefficients(
                                      Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                      Sample.EquidistantStartingAt(t => t, -2, 0.5, 10),
                                      SplineBoundaryCondition.Natural,
                                      1.0,
                                      SplineBoundaryCondition.Natural,
                                      -1.0))
                    }
            };

        [VerifyContract]
        public readonly IContract CubicHermiteSplineInfrastructureTests =
            new InterpolationInfrastructureContract<CubicHermiteSplineInterpolation>
            {
                MinimumSampleCount = 2,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new CubicHermiteSplineInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new CubicHermiteSplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10),
                                  Sample.EquidistantStartingAt(t => t, 1, 0.1, 10))
                    }
            };

        [VerifyContract]
        public readonly IContract LinearSplineInfrastructureTests =
            new InterpolationInfrastructureContract<LinearSplineInterpolation>
            {
                MinimumSampleCount = 2,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new LinearSplineInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new LinearSplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10)),
                        () => Interpolate.LinearBetweenPoints(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10))
                    }
            };

        [VerifyContract]
        public readonly IContract CubicSplineInfrastructureTests =
            new InterpolationInfrastructureContract<CubicSplineInterpolation>
            {
                MinimumSampleCount = 2,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new CubicSplineInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new CubicSplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10)),
                        () => new CubicSplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10),
                                  SplineBoundaryCondition.FirstDerivative,
                                  1.0,
                                  SplineBoundaryCondition.FirstDerivative,
                                  -1.0),
                        () => new CubicSplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10),
                                  SplineBoundaryCondition.Natural,
                                  1.0,
                                  SplineBoundaryCondition.Natural,
                                  -1.0),
                        () => new CubicSplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10),
                                  SplineBoundaryCondition.ParabolicallyTerminated,
                                  1.0,
                                  SplineBoundaryCondition.ParabolicallyTerminated,
                                  -1.0),
                        () => new CubicSplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 2),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 2),
                                  SplineBoundaryCondition.ParabolicallyTerminated,
                                  1.0,
                                  SplineBoundaryCondition.ParabolicallyTerminated,
                                  -1.0)
                    }
            };

        [VerifyContract]
        public readonly IContract AkimaSplineInfrastructureTests =
            new InterpolationInfrastructureContract<AkimaSplineInterpolation>
            {
                MinimumSampleCount = 5,
                UninitializedFactories =
                    new Func<IInterpolation>[]
                    {
                        () => new AkimaSplineInterpolation()
                    },
                InitializedFactories =
                    new Expression<Func<IInterpolation>>[]
                    {
                        () => new AkimaSplineInterpolation(
                                  Sample.EquidistantStartingAt(t => t, -5, 1, 10),
                                  Sample.EquidistantStartingAt(t => t, -2, 0.5, 10))
                    }
            };

        [Test]
        public void FloaterHormannRationalThrowsOnBadOrder()
        {
            Assert.Throws(
                typeof(ArgumentOutOfRangeException),
                () => new FloaterHormannRationalInterpolation(
                          new double[5],
                          new double[5],
                          10));
        }

        [Test]
        public void CubicSplineThrowsOnBadBoundaryCondition()
        {
            Assert.Throws(
                typeof(NotSupportedException),
                () => new CubicSplineInterpolation(
                          new double[5],
                          new double[5],
                          (SplineBoundaryCondition)(-1),
                          0,
                          SplineBoundaryCondition.Natural,
                          0));

            Assert.Throws(
                typeof(NotSupportedException),
                () => new CubicSplineInterpolation(
                          new double[5],
                          new double[5],
                          SplineBoundaryCondition.Natural,
                          0,
                          (SplineBoundaryCondition)(-1),
                          0));
        }
    }
}