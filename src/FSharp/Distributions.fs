// <copyright file="Random.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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

namespace MathNet.Numerics.Distributions

open MathNet.Numerics.Random

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Sample =

    let map f dist : System.Random -> 'T = fun rng -> f (dist rng)
    let map2 f dist1 dist2 : System.Random -> 'T = fun rng -> f (dist1 rng) (dist2 rng)
    let map3 f dist1 dist2 dist3 : System.Random -> 'T = fun rng -> f (dist1 rng) (dist2 rng) (dist3 rng)

    let mapSeq f dist : System.Random -> 'T seq = fun rng -> dist rng |> Seq.map f
    let mapSeq2 f dist1 dist2 : System.Random -> 'T seq = fun rng -> Seq.zip (dist1 rng) (dist2 rng) |> Seq.map (fun (d1, d2) -> f d1 d2)
    let mapSeq3 f dist1 dist2 dist3 : System.Random -> 'T seq = fun rng -> Seq.zip3 (dist1 rng) (dist2 rng) (dist3 rng) |> Seq.map (fun (d1, d2, d3) -> f d1 d2 d3)

    /// Bernoulli with probability (p).
    let bernoulli p (rng:System.Random) = Bernoulli.Sample(rng, p)
    let bernoulliSeq p (rng:System.Random) = Bernoulli.Samples(rng, p)

    /// Beta with α and β shape parameters.
    let beta a b (rng:System.Random) = Beta.Sample(rng, a, b)
    let betaSeq a b (rng:System.Random) = Beta.Samples(rng, a, b)

    /// Binomial with success probability (p) in each trial and number of trials (n).
    let binomial p n (rng:System.Random) = Binomial.Sample(rng, p, n)
    let binomialSeq p n (rng:System.Random) = Binomial.Samples(rng, p, n)

    /// Categorical with an array of nonnegative ratios defining the relative probability mass (unnormalized).
    let categorical probabilityMass (rng:System.Random) = Categorical.Sample(rng, probabilityMass)
    let categoricalSeq probabilityMass (rng:System.Random) = Categorical.Samples(rng, probabilityMass)

    /// Cauchy with location (x0) and scale (γ).
    let cauchy location scale (rng:System.Random) = Cauchy.Sample(rng, location, scale)
    let cauchySeq location scale (rng:System.Random) = Cauchy.Samples(rng, location, scale)

    /// Chi with degrees of freedom (k).
    let chi freedom (rng:System.Random) = Chi.Sample(rng, freedom)
    let chiSeq freedom (rng:System.Random) = Chi.Samples(rng, freedom)

    /// Chi-Squared with degrees of freedom (k).
    let chiSquared freedom (rng:System.Random) = ChiSquared.Sample(rng, freedom)
    let chiSquaredSeq freedom (rng:System.Random) = ChiSquared.Samples(rng, freedom)

    /// Continuous-Uniform with lower and upper bounds.
    let continuousUniform lower upper (rng:System.Random) = ContinuousUniform.Sample(rng, lower, upper)
    let continuousUniformSeq lower upper (rng:System.Random) = ContinuousUniform.Samples(rng, lower, upper)

    /// Conway-Maxwell-Poisson with lambda (λ) and rate of decay (ν).
    let conwayMaxwellPoisson lambda nu (rng:System.Random) = ConwayMaxwellPoisson.Sample(rng, lambda, nu)
    let conwayMaxwellPoissonSeq lambda nu (rng:System.Random) = ConwayMaxwellPoisson.Samples(rng, lambda, nu)

    /// Discrete-Uniform with lower and upper bounds (both inclusive).
    let discreteUniform lower upper (rng:System.Random) = DiscreteUniform.Sample(rng, lower, upper)
    let discreteUniformSeq lower upper (rng:System.Random) = DiscreteUniform.Samples(rng, lower, upper)

    /// Erlang with shape (k) and rate or inverse scale (λ).
    let erlang (shape:int) rate (rng:System.Random) = Erlang.Sample(rng, shape, rate)
    let erlangSeq (shape:int) rate (rng:System.Random) = Erlang.Samples(rng, shape, rate)

    /// Exponential with rate (λ).
    let exponential rate (rng:System.Random) = Exponential.Sample(rng, rate)
    let exponentialSeq rate (rng:System.Random) = Exponential.Samples(rng, rate)

    /// Fisher-Snedecor (F-Distribution) with first (d1) and second (d2) degree of freedom.
    let fisherSnedecor d1 d2 (rng:System.Random) = FisherSnedecor.Sample(rng, d1, d2)
    let fisherSnedecorSeq d1 d2 (rng:System.Random) = FisherSnedecor.Samples(rng, d1, d2)

    /// Gamma with shape (k, α) and rate or inverse scale (β).
    let gamma shape rate (rng:System.Random) = Gamma.Sample(rng, shape, rate)
    let gammaSeq shape rate (rng:System.Random) = Gamma.Sample(rng, shape, rate)

    /// Geometric with probability (p) of generating one.
    let geometric p (rng:System.Random) = Geometric.Sample(rng, p)
    let geometricSeq p (rng:System.Random) = Geometric.Samples(rng, p)

    /// Hypergeometric with size of the population (N), number successes within the population (K, M) and number of draws without replacement (n).
    let hypergeometric population success draws (rng:System.Random) = Hypergeometric.Sample(rng, population, success, draws)
    let hypergeometricSeq population success draws (rng:System.Random) = Hypergeometric.Samples(rng, population, success, draws)

    /// Inverse-Gamma with shape (α) and scale (β)
    let inverseGamma shape scale (rng:System.Random) = InverseGamma.Sample(rng, shape, scale)
    let inverseGammaSeq shape scale (rng:System.Random) = InverseGamma.Samples(rng, shape, scale)

    /// Laplace with location (μ) and scale (b).
    let laplace location scale (rng:System.Random) = Laplace.Sample(rng, location, scale)
    let laplaceSeq location scale (rng:System.Random) = Laplace.Samples(rng, location, scale)

    /// Log-Normal with log-scale (μ) and shape (σ).
    let logNormal mu sigma (rng:System.Random) = LogNormal.Sample(rng, mu, sigma)
    let logNormalSeq mu sigma (rng:System.Random) = LogNormal.Samples(rng, mu, sigma)

    /// Negative-Binomial with number of failures (r) until the experiment stopped and probability (p) of a trial resulting in success.
    let negativeBinomial r p (rng:System.Random) = NegativeBinomial.Sample(rng, r, p)
    let negativeBinomialSeq r p (rng:System.Random) = NegativeBinomial.Samples(rng, r, p)

    /// Normal with mean (μ) and standard deviation (σ).
    let normal mean stddev (rng:System.Random) = Normal.Sample(rng, mean, stddev)
    let normalSeq mean stddev (rng:System.Random) = Normal.Samples(rng, mean, stddev)

    /// Standard Gaussian.
    let standard (rng:System.Random) = Normal.Sample(rng, 0.0, 1.0)
    let standardSeq (rng:System.Random) = Normal.Samples(rng, 0.0, 1.0)

    /// Pareto with scale (xm) and shape (α).
    let pareto scale shape (rng:System.Random) = Pareto.Sample(rng, scale, shape)
    let paretoSeq scale shape (rng:System.Random) = Pareto.Samples(rng, scale, shape)

    /// Poisson with lambda (λ).
    let poisson lambda (rng:System.Random) = Poisson.Sample(rng, lambda)
    let poissonSeq lambda (rng:System.Random) = Poisson.Samples(rng, lambda)

    /// Rayleigh with scale (σ).
    let rayleigh scale (rng:System.Random) = Rayleigh.Sample(rng, scale)
    let rayleighSeq scale (rng:System.Random) = Rayleigh.Sample(rng, scale)

    /// Stable with stability (α), skewness (β), scale (c) and location (μ).
    let stable alpha beta scale location (rng:System.Random) = Stable.Sample(rng, alpha, beta, scale, location)
    let stableSeq alpha beta scale location (rng:System.Random) = Stable.Samples(rng, alpha, beta, scale, location)

    /// Student-T with location (μ), scale (σ) and degrees of freedom (ν).
    let studentT location scale freedom (rng:System.Random) = StudentT.Sample(rng, location, scale, freedom)
    let studentTSeq location scale freedom (rng:System.Random) = StudentT.Samples(rng, location, scale, freedom)

    /// Weibull with shape (k) and scale (λ).
    let weibull shape scale (rng:System.Random) = Weibull.Sample(rng, shape, scale)
    let weibullSeq shape scale (rng:System.Random) = Weibull.Samples(rng, shape, scale)

    /// Zipf with s and n parameters.
    let zipf s n (rng:System.Random) = Zipf.Sample(rng, s, n)
    let zipfSeq s n (rng:System.Random) = Zipf.Samples(rng, s, n)
