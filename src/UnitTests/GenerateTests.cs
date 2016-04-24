// <copyright file="GenerateTests.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

using System;
using System.Linq;
using NUnit.Framework;

namespace MathNet.Numerics.UnitTests
{
#if !NOSYSNUMERICS
    using System.Numerics;
#endif

    [TestFixture]
    public class GenerateTests
    {
        [Test]
        public void LinearSpaced()
        {
            Assert.That(Generate.LinearSpaced(0, 0d, 2d), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LinearSpaced(1, 0d, 2d), Is.EqualTo(new[] { 2d }).AsCollection);
            Assert.That(Generate.LinearSpaced(2, 0d, 2d), Is.EqualTo(new[] { 0d, 2d }).AsCollection);
            Assert.That(Generate.LinearSpaced(3, 0d, 2d), Is.EqualTo(new[] { 0d, 1d, 2d }).AsCollection);
            Assert.That(Generate.LinearSpaced(4, 0d, 2d), Is.EqualTo(new[] { 0d, 2d/3d, 4d/3d, 2d }).Within(1e-12).AsCollection);

            Assert.That(Generate.LinearSpaced(0, 2d, 0d), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LinearSpaced(1, 2d, 0d), Is.EqualTo(new[] { 0d }).AsCollection);
            Assert.That(Generate.LinearSpaced(2, 2d, 0d), Is.EqualTo(new[] { 2d, 0d }).AsCollection);
            Assert.That(Generate.LinearSpaced(3, 2d, 0d), Is.EqualTo(new[] { 2d, 1d, 0d }).AsCollection);
            Assert.That(Generate.LinearSpaced(4, 2d, 0d), Is.EqualTo(new[] { 2d, 4d/3d, 2d/3d, 0d }).Within(1e-12).AsCollection);
        }

        [Test]
        public void LogSpaced()
        {
            Assert.That(Generate.LogSpaced(0, 0d, 2d), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LogSpaced(1, 0d, 2d), Is.EqualTo(new[] { 100.0 }).AsCollection);
            Assert.That(Generate.LogSpaced(2, 0d, 2d), Is.EqualTo(new[] { 1.0, 100.0 }).AsCollection);
            Assert.That(Generate.LogSpaced(3, 0d, 2d), Is.EqualTo(new[] { 1.0, 10.0, 100.0 }).AsCollection);
            Assert.That(Generate.LogSpaced(4, 0d, 2d), Is.EqualTo(new[] { 1.0, Math.Pow(10.0, 2.0/3.0), Math.Pow(10.0, 4.0/3.0), 100.0 }).Within(1e-12).AsCollection);

            Assert.That(Generate.LogSpaced(0, 2d, 0d), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LogSpaced(1, 2d, 0d), Is.EqualTo(new[] { 1.0 }).AsCollection);
            Assert.That(Generate.LogSpaced(2, 2d, 0d), Is.EqualTo(new[] { 100.0, 1.0 }).AsCollection);
            Assert.That(Generate.LogSpaced(3, 2d, 0d), Is.EqualTo(new[] { 100.0, 10.0, 1.0 }).AsCollection);
            Assert.That(Generate.LogSpaced(4, 2d, 0d), Is.EqualTo(new[] { 100.0, Math.Pow(10.0, 4.0/3.0), Math.Pow(10, 2.0/3.0), 1.0 }).Within(1e-12).AsCollection);

            Assert.That(Generate.LogSpaced(5, -2d, 2d), Is.EqualTo(new[] { 0.01, 0.1, 1.0, 10.0, 100.0 }).AsCollection);
            Assert.That(Generate.LogSpaced(5, 2d, -2d), Is.EqualTo(new[] { 100.0, 10.0, 1.0, 0.1, 0.01 }).AsCollection);
        }

        [Test]
        public void LinearRange()
        {
            Assert.That(Generate.LinearRange(1, 1), Is.EqualTo(new[] { 1d }).AsCollection);

            Assert.That(Generate.LinearRange(1, 3), Is.EqualTo(new[] { 1d, 2d, 3d }).AsCollection);
            Assert.That(Generate.LinearRange(-1, -3), Is.EqualTo(new[] { -1d, -2d, -3d }).AsCollection);
            Assert.That(Generate.LinearRange(-3, -1), Is.EqualTo(new[] { -3d, -2d, -1d }).AsCollection);
            Assert.That(Generate.LinearRange(1, -2), Is.EqualTo(new[] { 1d, 0d, -1d, -2d }).AsCollection);
        }

        [Test]
        public void LinearRangeStep()
        {
            Assert.That(Generate.LinearRange(1, 1, 1), Is.EqualTo(new[] { 1d }).AsCollection);

            Assert.That(Generate.LinearRange(1, -1, 2), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LinearRange(2, 1, 1), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LinearRange(1, 0, 2), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LinearRange(2, 0, 1), Is.EqualTo(new double[0]).AsCollection);

            Assert.That(Generate.LinearRange(1, 1, 5), Is.EqualTo(new[] { 1d, 2d, 3d, 4d, 5d }).AsCollection);
            Assert.That(Generate.LinearRange(1, 2, 5), Is.EqualTo(new[] { 1d, 3d, 5d }).AsCollection);
            Assert.That(Generate.LinearRange(1, 2, 6), Is.EqualTo(new[] { 1d, 3d, 5d }).AsCollection);
            Assert.That(Generate.LinearRange(1, 2, 4), Is.EqualTo(new[] { 1d, 3d }).AsCollection);

            Assert.That(Generate.LinearRange(1, -1, -3), Is.EqualTo(new[] { 1d, 0d, -1d, -2d, -3d }).AsCollection);
            Assert.That(Generate.LinearRange(1, -2, -3), Is.EqualTo(new[] { 1d, -1d, -3d }).AsCollection);
            Assert.That(Generate.LinearRange(1, -2, -4), Is.EqualTo(new[] { 1d, -1d, -3d }).AsCollection);
            Assert.That(Generate.LinearRange(1, -2, -2), Is.EqualTo(new[] { 1d, -1d }).AsCollection);
        }

        [Test]
        public void LinearRangeFloatingPoint()
        {
            Assert.That(Generate.LinearRange(1d, 1d, 1d), Is.EqualTo(new[] { 1d }).AsCollection);

            Assert.That(Generate.LinearRange(1d, -1d, 2d), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LinearRange(2d, 1d, 1d), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LinearRange(1d, 0d, 2d), Is.EqualTo(new double[0]).AsCollection);
            Assert.That(Generate.LinearRange(2d, 0d, 1d), Is.EqualTo(new double[0]).AsCollection);

            Assert.That(Generate.LinearRange(1d, 1d, 5d), Is.EqualTo(new[] { 1d, 2d, 3d, 4d, 5d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, 2d, 5d), Is.EqualTo(new[] { 1d, 3d, 5d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, 2d, 6d), Is.EqualTo(new[] { 1d, 3d, 5d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, 2d, 4d), Is.EqualTo(new[] { 1d, 3d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, 1.5d, 5d), Is.EqualTo(new[] { 1d, 2.5d, 4d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, 1.5d, 6.5d), Is.EqualTo(new[] { 1d, 2.5d, 4d, 5.5d }).AsCollection);

            Assert.That(Generate.LinearRange(1d, -1d, -3d), Is.EqualTo(new[] { 1d, 0d, -1d, -2d, -3d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, -2d, -3d), Is.EqualTo(new[] { 1d, -1d, -3d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, -2d, -4d), Is.EqualTo(new[] { 1d, -1d, -3d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, -2d, -2d), Is.EqualTo(new[] { 1d, -1d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, -1.5d, -3d), Is.EqualTo(new[] { 1d, -0.5d, -2d }).AsCollection);
            Assert.That(Generate.LinearRange(1d, -1.5d, -3.5d), Is.EqualTo(new[] { 1d, -0.5d, -2d, -3.5 }).AsCollection);
            Assert.That(Generate.LinearRange(1d, -1.5d, -4d), Is.EqualTo(new[] { 1d, -0.5d, -2d, -3.5 }).AsCollection);
        }

        [Test]
        public void Periodic()
        {
            Assert.That(Generate.Periodic(8, 2.0, 0.5), Is.EqualTo(new[] { 0.0, 0.25, 0.5, 0.75, 0.0, 0.25, 0.5, 0.75 }).Within(1e-12).AsCollection);
            Assert.That(Generate.Periodic(8, 2.0, 0.5, 1.0, delay: 1), Is.EqualTo(new[] { 0.75, 0.0, 0.25, 0.5, 0.75, 0.0, 0.25, 0.5 }).Within(1e-12).AsCollection);
            Assert.That(Generate.Periodic(8, 2.0, 0.5, 1.0, delay: -1), Is.EqualTo(new[] { 0.25, 0.5, 0.75, 0.0, 0.25, 0.5, 0.75, 0.0 }).Within(1e-12).AsCollection);
            Assert.That(Generate.Periodic(8, 2.0, 0.5, 1.0, phase: 0.7), Is.EqualTo(new[] { 0.7, 0.95, 0.2, 0.45, 0.7, 0.95, 0.2, 0.45 }).Within(1e-12).AsCollection);
            Assert.That(Generate.Periodic(8, 2.0, 0.5, 1.0, phase: -0.3), Is.EqualTo(new[] { 0.7, 0.95, 0.2, 0.45, 0.7, 0.95, 0.2, 0.45 }).Within(1e-12).AsCollection);
            Assert.That(Generate.Periodic(8, 2.0, 0.5, 2.0), Is.EqualTo(new[] { 0.0, 0.5, 1.0, 1.5, 0.0, 0.5, 1.0, 1.5 }).Within(1e-12).AsCollection);
            Assert.That(Generate.Periodic(8, 2.0, 0.5, 2.0, phase: 1.9), Is.EqualTo(new[] { 1.9, 0.4, 0.9, 1.4, 1.9, 0.4, 0.9, 1.4 }).Within(1e-12).AsCollection);
            Assert.That(Generate.Periodic(8, 8.0, 1.0), Is.EqualTo(new[] { 0.0, 0.125, 0.25, 0.375, 0.5, 0.625, 0.75, 0.875 }).Within(1e-12).AsCollection);

            // Angular over a full circle:
            const double isq2 = Constants.Sqrt1Over2;
            Assert.That(Generate.Periodic(8, 2.0, 0.5, Constants.Pi2).Select(Math.Sin), Is.EqualTo(new[] { 0.0, 1.0, 0.0, -1.0, 0.0, 1.0, 0.0, -1.0 }).Within(1e-12).AsCollection);
            Assert.That(Generate.Periodic(8, 2.0, 0.25, Constants.Pi2).Select(Math.Sin), Is.EqualTo(new[] { 0.0, isq2, 1.0, isq2, 0.0, -isq2, -1.0, -isq2 }).Within(1e-12).AsCollection);
        }

        [Test]
        public void StandardWaves()
        {
            Assert.That(Generate.Square(12, 3, 7, -1.0, 1.0, delay: 1), Is.EqualTo(new[] { -1.0, 1, 1, 1, -1, -1, -1, -1, -1, -1, -1, 1 }).Within(1e-12).AsCollection);
            Assert.That(Generate.Triangle(12, 4, 7, -1.0, 1.0, delay: 1), Is.EqualTo(new[] { -0.714, -1, -0.5, 0, 0.5, 1, 0.714, 0.429, 0.143, -0.143, -0.429, -0.714 }).Within(1e-3).AsCollection);
            Assert.That(Generate.Sawtooth(12, 5, -1.0, 1.0, delay: 1), Is.EqualTo(new[] { 1.0, -1, -0.5, 0, 0.5, 1, -1, -0.5, 0, 0.5, 1, -1 }).Within(1e-12).AsCollection);
        }

        [Test]
        public void StandardWavesConsistentWithSequence()
        {
            Assert.That(
                Generate.SquareSequence(3, 7, -1.0, 1.0, delay: -2).Take(1000).ToArray(),
                Is.EqualTo(Generate.Square(1000, 3, 7, -1.0, 1.0, delay: -2)).Within(1e-12).AsCollection);
            Assert.That(
                Generate.TriangleSequence(4, 7, -1.0, 1.0, delay: -2).Take(1000).ToArray(),
                Is.EqualTo(Generate.Triangle(1000, 4, 7, -1.0, 1.0, delay: -2)).Within(1e-12).AsCollection);
            Assert.That(
                Generate.SawtoothSequence(5, -1.0, 1.0, delay: -2).Take(1000).ToArray(),
                Is.EqualTo(Generate.Sawtooth(1000, 5, -1.0, 1.0, delay: -2)).Within(1e-12).AsCollection);
        }

        [Test]
        public void PeriodicConsistentWithSinusoidal()
        {
            Assert.That(
                Generate.PeriodicMap(100, Math.Sin, 16.0, 2.0, Constants.Pi2),
                Is.EqualTo(Generate.Sinusoidal(100, 16.0, 2.0, 1.0)).Within(1e-12).AsCollection);

            Assert.That(
                Generate.PeriodicMapSequence(Math.Sin, 16.0, 2.0, Constants.Pi2).Take(100).ToArray(),
                Is.EqualTo(Generate.SinusoidalSequence(16.0, 2.0, 1.0).Take(100).ToArray()).Within(1e-12).AsCollection);
        }

        [Test]
        public void PreiodicConsistentWithSequence()
        {
            Assert.That(
                Generate.PeriodicSequence(16.0, 2.0, Constants.Pi2, Constants.PiOver4, 2).Take(1000).ToArray(),
                Is.EqualTo(Generate.Periodic(1000, 16.0, 2.0, Constants.Pi2, Constants.PiOver4, 2)).Within(1e-12).AsCollection);
        }

        [Test]
        public void SinusoidalConsistentWithSequence()
        {
            Assert.That(
                Generate.SinusoidalSequence(32, 2, 5, 1, 0.5, -6).Take(1000).ToArray(),
                Is.EqualTo(Generate.Sinusoidal(1000, 32, 2, 5, 1, 0.5, -6)).AsCollection);
        }

        [Test]
        public void StepConsistentWithSequence()
        {
            Assert.That(
                Generate.StepSequence(5, 40).Take(1000).ToArray(),
                Is.EqualTo(Generate.Step(1000, 5, 40)).AsCollection);
        }

        [Test]
        public void ImpulseConsistentWithSequence()
        {
            Assert.That(
                Generate.ImpulseSequence(5, 40).Take(1000).ToArray(),
                Is.EqualTo(Generate.Impulse(1000, 5, 40)).AsCollection);

            Assert.That(
                Generate.PeriodicImpulseSequence(100, 5, 40).Take(1000).ToArray(),
                Is.EqualTo(Generate.PeriodicImpulse(1000, 100, 5, 40)).AsCollection);
        }

        [Test]
        public void UnfoldConsistentWithSequence()
        {
            Assert.That(
                Generate.UnfoldSequence((s => new Tuple<int, int>(s + 1, s + 1)), 0).Take(250).ToArray(),
                Is.EqualTo(Generate.Unfold(250, (s => new Tuple<int, int>(s + 1, s + 1)), 0)).AsCollection);
        }

#if !NOSYSNUMERICS

        [Test]
        public void FibonacciConsistentWithSequence()
        {
            Assert.That(
                Generate.FibonacciSequence().Take(250).ToArray(),
                Is.EqualTo(Generate.Fibonacci(250)).AsCollection);
        }

        [Test]
        public void FibonacciConsistentWithUnfold()
        {
            Assert.That(
                Generate.FibonacciSequence().Take(250).ToArray(),
                Is.EqualTo(new[] { BigInteger.Zero, BigInteger.One }.Concat(Generate.Unfold(248, (s =>
                {
                    var z = s.Item1 + s.Item2;
                    return new Tuple<BigInteger, Tuple<BigInteger, BigInteger>>(z, new Tuple<BigInteger, BigInteger>(s.Item2, z));
                }), new Tuple<BigInteger, BigInteger>(BigInteger.Zero, BigInteger.One)))).AsCollection);
        }

#endif

    }
}
