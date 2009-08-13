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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5))
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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5)),
                        () => Interpolate.RationalWithPoles(
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5))
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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5),
                                  SampleProvider.LinearEquidistant(10, 1, 0.1)),
                        () => new BarycentricInterpolation(
                                  SampleProvider.LinearEquidistant(10, 5, -1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5),
                                  SampleProvider.LinearEquidistant(10, 1, 0.1))
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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5)),
                        () => new FloaterHormannRationalInterpolation(
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5),
                                  5),
                        () => Interpolate.RationalWithoutPoles(
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5)),
                        () => Interpolate.Common(
                                  SampleProvider.LinearEquidistant(10, 5, -1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5))
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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5)),
                        () => new EquidistantPolynomialInterpolation(
                                  SampleProvider.LinearEquidistant(10, 5, -1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5)),
                        () => new EquidistantPolynomialInterpolation(
                                  -5,
                                  4,
                                  SampleProvider.LinearEquidistant(10, -2, 0.5))
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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(4 * (10 - 1), -2, 0.5)),
                        () => new SplineInterpolation(
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  AkimaSplineInterpolation.EvaluateSplineCoefficients(
                                      SampleProvider.LinearEquidistant(10, -5, 1),
                                      SampleProvider.LinearEquidistant(10, -2, 0.5))),
                        () => new SplineInterpolation(
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  CubicSplineInterpolation.EvaluateSplineCoefficients(
                                      SampleProvider.LinearEquidistant(10, -5, 1),
                                      SampleProvider.LinearEquidistant(10, -2, 0.5),
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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5),
                                  SampleProvider.LinearEquidistant(10, 1, 0.1))
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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5)),
                        () => Interpolate.LinearBetweenPoints(
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5))
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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5)),
                        () => new CubicSplineInterpolation(
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5),
                                  SplineBoundaryCondition.FirstDerivative,
                                  1.0,
                                  SplineBoundaryCondition.FirstDerivative,
                                  -1.0),
                        () => new CubicSplineInterpolation(
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5),
                                  SplineBoundaryCondition.Natural,
                                  1.0,
                                  SplineBoundaryCondition.Natural,
                                  -1.0),
                        () => new CubicSplineInterpolation(
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5),
                                  SplineBoundaryCondition.ParabolicallyTerminated,
                                  1.0,
                                  SplineBoundaryCondition.ParabolicallyTerminated,
                                  -1.0),
                        () => new CubicSplineInterpolation(
                                  SampleProvider.LinearEquidistant(2, -5, 1),
                                  SampleProvider.LinearEquidistant(2, -2, 0.5),
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
                                  SampleProvider.LinearEquidistant(10, -5, 1),
                                  SampleProvider.LinearEquidistant(10, -2, 0.5))
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
