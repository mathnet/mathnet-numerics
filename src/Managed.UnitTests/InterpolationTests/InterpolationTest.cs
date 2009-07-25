// <copyright file="InterpolationTest.cs" company="Math.NET">
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
    using Interpolation;
    using Interpolation.Algorithms;
    using MbUnit.Framework;
    using MbUnit.Framework.ContractVerifiers;

    [TestFixture]
    public class InterpolationTest
    {
        [VerifyContract]
        public readonly IContract LinearSplineContractTests = new InterpolationContract<LinearSplineInterpolation>()
        {
            Factory = Interpolate.LinearBetweenPoints,
            Order = new[] { 2, 3, 6 },
            LinearBehavior = true,
            PolynomialBehavior = false,
            RationalBehavior = false
        };

        [VerifyContract]
        public readonly IContract FloaterHormannRationalContractTests = new InterpolationContract<FloaterHormannRationalInterpolation>()
        {
            Factory = Interpolate.RationalWithoutPoles,
            Order = new[] { 1, 2, 6 },
            LinearBehavior = true,
            PolynomialBehavior = true,
            RationalBehavior = true
        };

        [VerifyContract]
        public readonly IContract NevillePolynomialContractTests = new InterpolationContract<NevillePolynomialInterpolation>()
        {
            Factory = (t, x) => new NevillePolynomialInterpolation(t, x),
            Order = new[] { 1, 2, 6 },
            LinearBehavior = true,
            PolynomialBehavior = true,
            RationalBehavior = false
        };

        [VerifyContract]
        public readonly IContract BulirschStoerRationalContractTests = new InterpolationContract<BulirschStoerRationalInterpolation>()
        {
            Factory = Interpolate.RationalWithPoles,
            Order = new[] { 1, 2, 6 },
            LinearBehavior = false,
            PolynomialBehavior = true,
            RationalBehavior = true
        };
    }
}
