// <copyright file="HybridMCGeneric.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
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

namespace MathNet.Numerics.Statistics.Mcmc
{
    using System;
    using Distributions;
    using Properties;

    /// <summary>
    /// The Hybrid (also called Hamiltonian) Monte Carlo produces samples from distribition P using a set
    /// of Hamiltonian equations to guide the sampling process. It uses the negative of the log density as
    /// a potential energy, and a randomly generated momentum to set up a Hamiltonian system, which is then used
    /// to sample the distribution. This can result in a faster convergence than the random walk Metropolis sampler
    /// (<seealso cref="MetropolisSampler{T}"/>).
    /// </summary>
    /// <typeparam name="T">The type of samples this sampler produces.</typeparam>
    abstract public class HybridMCGeneric<T> : McmcSampler<T>
    {
        /// <summary>
        /// The delegate type that defines a derivative evaluated at a certain point.
        /// </summary>
        /// <param name="f">Function to be differentiated.</param>
        /// <param name="x">Value where the derivative is computed.</param>
        /// <returns></returns>
        public delegate T DiffMethod(DensityLn<T> f, T x);

        /// <summary>
        /// Evaluates the energy function of the target distribution.
        /// </summary>
        readonly DensityLn<T> _energy;

        /// <summary>
        /// The current location of the sampler.
        /// </summary>
        protected T Current;

        /// <summary>
        /// The number of burn iterations between two samples.
        /// </summary>
        int _burnInterval;

        /// <summary>
        /// The size of each step in the Hamiltonian equation.
        /// </summary>
        double _stepSize;

        /// <summary>
        /// The number of iterations in the Hamiltonian equation.
        /// </summary>
        int _frogLeapSteps;

        /// <summary>
        /// The algorithm used for differentiation.
        /// </summary>
        readonly DiffMethod _diff;

        /// <summary>
        /// Gets or sets the number of iterations in between returning samples.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When burn interval is negative.</exception>
        public int BurnInterval
        {
            get { return _burnInterval; }
            set
            {
                _burnInterval = SetNonNegative(value);
            }
        }

        /// <summary>
        /// Gets or sets the number of iterations in the Hamiltonian equation.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When frogleap steps is negative or zero.</exception>
        public int FrogLeapSteps
        {
            get { return _frogLeapSteps; }
            set
            {
                _frogLeapSteps = SetPositive(value);
            }
        }

        /// <summary>
        /// Gets or sets the size of each step in the Hamiltonian equation.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When step size is negative or zero.</exception>
        public double StepSize
        {
            get { return _stepSize; }
            set
            {
                _stepSize = SetPositive(value);
            }
        }

        /// <summary>
        /// Constructs a new Hybrid Monte Carlo sampler.
        /// </summary>
        /// <param name="x0">The initial sample.</param>
        /// <param name="pdfLnP">The log density of the distribution we want to sample from.</param>
        /// <param name="frogLeapSteps">Number frogleap simulation steps.</param>
        /// <param name="stepSize">Size of the frogleap simulation steps.</param>
        /// <param name="burnInterval">The number of iterations in between returning samples.</param>
        /// <param name="randomSource">Random number generator used for sampling the momentum.</param>
        /// <param name="diff">The method used for differentiation.</param>
        /// <exception cref="ArgumentOutOfRangeException">When the number of burnInterval iteration is negative.</exception>
        /// <exception cref="ArgumentNullException">When either x0, pdfLnP or diff is null.</exception>
        public HybridMCGeneric(T x0, DensityLn<T> pdfLnP, int frogLeapSteps, double stepSize, int burnInterval, Random randomSource, DiffMethod diff)
        {
            _energy = x => -pdfLnP(x);
            FrogLeapSteps = frogLeapSteps;
            StepSize = stepSize;
            BurnInterval = burnInterval;
            Current = x0;
            _diff = diff;
            RandomSource = randomSource;
        }

        /// <summary>
        /// Returns a sample from the distribution P.
        /// </summary>
        public override T Sample()
        {
            Burn(_burnInterval + 1);

            return Current;
        }

        /// <summary>
        /// This method runs the sampler for a number of iterations without returning a sample
        /// </summary>
        protected void Burn(int n)
        {
            T p = Create();
            double e = _energy(Current);
            T gradient = _diff(_energy, Current);
            for (int i = 0; i < n; i++)
            {
                RandomizeMomentum(ref p);
                double h = Hamiltonian(p, e);


                T mNew = Copy(Current);
                T gNew = Copy(gradient);

                for (int j = 0; j < _frogLeapSteps; j++)
                {
                    HamiltonianEquations(ref gNew, ref mNew, ref p);
                }

                double enew = _energy(mNew);
                double hnew = Hamiltonian(p, enew);

                double dh = hnew - h;

                Update(ref e, ref gradient, mNew, gNew, enew, dh);
                Samples++;
            }
        }

        /// <summary>
        /// Method used to update the sample location. Used in the end of the loop.
        /// </summary>
        /// <param name="e">The old energy.</param>
        /// <param name="gradient">The old gradient/derivative of the energy.</param>
        /// <param name="mNew">The new sample.</param>
        /// <param name="gNew">The new gradient/derivative of the energy.</param>
        /// <param name="enew">The new energy.</param>
        /// <param name="dh">The difference between the old Hamiltonian and new Hamiltonian. Use to determine
        /// if an update should take place. </param>
        protected void Update(ref double e, ref T gradient, T mNew, T gNew, double enew, double dh)
        {
            if (dh <= 0)
            {
                Current = mNew; gradient = gNew; e = enew; Accepts++;
            }
            else if (Bernoulli.Sample(RandomSource, Math.Exp(-dh)) == 1)
            {
                Current = mNew; gradient = gNew; e = enew; Accepts++;
            }
        }

        /// <summary>
        /// Use for creating temporary objects in the Burn method.
        /// </summary>
        /// <returns>An object of type T.</returns>
        abstract protected T Create();

        /// <summary>
        /// Use for copying objects in the Burn method.
        /// </summary>
        /// <param name="source">The source of copying.</param>
        /// <returns>A copy of the source object.</returns>
        abstract protected T Copy(T source);

        /// <summary>
        /// Method for doing dot product.
        /// </summary>
        /// <param name="first">First vector/scalar in the product.</param>
        /// <param name="second">Second vector/scalar in the product.</param>
        /// <returns></returns>
        abstract protected double DoProduct(T first, T second);

        /// <summary>
        /// Method for adding, multiply the second vector/scalar by factor and then
        /// add it to the first vector/scalar.
        /// </summary>
        /// <param name="first">First vector/scalar.</param>
        /// <param name="factor">Scalar factor multiplying by the second vector/scalar.</param>
        /// <param name="second">Second vector/scalar.</param>
        abstract protected void DoAdd(ref T first, double factor, T second);

        /// <summary>
        /// Multiplying the second vector/scalar by factor and then subtract it from
        /// the first vector/scalar.
        /// </summary>
        /// <param name="first">First vector/scalar.</param>
        /// <param name="factor">Scalar factor to be multiplied to the second vector/scalar.</param>
        /// <param name="second">Second vector/scalar.</param>
        abstract protected void DoSubtract(ref T first, double factor, T second);

        /// <summary>
        /// Method for sampling a random momentum.
        /// </summary>
        /// <param name="p">Momentum to be randomized.</param>
        abstract protected void RandomizeMomentum(ref T p);

        /// <summary>
        /// The Hamiltonian equations that is used to produce the new sample.
        /// </summary>
        protected void HamiltonianEquations(ref T gNew, ref T mNew, ref T p)
        {
            DoSubtract(ref p, _stepSize / 2, gNew);
            DoAdd(ref mNew, _stepSize, p);
            gNew = _diff(_energy, mNew);
            DoSubtract(ref p, _stepSize / 2, gNew);
        }

        /// <summary>
        /// Method to compute the Hamiltonian used in the method.
        /// </summary>
        /// <param name="momentum">The momentum.</param>
        /// <param name="e">The energy.</param>
        /// <returns>Hamiltonian=E+p.p/2</returns>
        protected double Hamiltonian(T momentum, double e)
        {
            return e + DoProduct(momentum, momentum) / 2;
        }

        /// <summary>
        /// Method to check and set a quantity to a non-negative value.
        /// </summary>
        /// <param name="value">Proposed value to be checked.</param>
        /// <returns>Returns value if it is greater than or equal to zero.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws when value is negative.</exception>
        protected int SetNonNegative(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentNotNegative);
            }
            return value;
        }

        /// <summary>
        /// Method to check and set a quantity to a non-negative value.
        /// </summary>
        /// <param name="value">Proposed value to be checked.</param>
        /// <returns>Returns value if it is greater than to zero.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws when value is negative or zero.</exception>
        protected int SetPositive(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentNotNegative);
            }
            return value;
        }

        /// <summary>
        /// Method to check and set a quantity to a non-negative value.
        /// </summary>
        /// <param name="value">Proposed value to be checked.</param>
        /// <returns>Returns value if it is greater than zero.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws when value is negative or zero.</exception>
        protected double SetPositive(double value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(Resources.ArgumentNotNegative);
            }
            return value;
        }
    }
}
