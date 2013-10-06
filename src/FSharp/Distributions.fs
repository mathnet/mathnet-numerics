// <copyright file="Random.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2012 Math.NET
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

    let transform f dist : System.Random -> 'T = fun rng -> f (dist rng)
    let transform2 f dist1 dist2 : System.Random -> 'T = fun rng -> f (dist1 rng) (dist2 rng)
    let transform3 f dist1 dist2 dist3 : System.Random -> 'T = fun rng -> f (dist1 rng) (dist2 rng) (dist3 rng)

    let transformSeq f dist : System.Random -> 'T seq = fun rng -> dist rng |> Seq.map f
    let transformSeq2 f dist1 dist2 : System.Random -> 'T seq = fun rng -> Seq.zip (dist1 rng) (dist2 rng) |> Seq.map (fun (d1, d2) -> f d1 d2)
    let transformSeq3 f dist1 dist2 dist3 : System.Random -> 'T seq = fun rng -> Seq.zip3 (dist1 rng) (dist2 rng) (dist3 rng) |> Seq.map (fun (d1, d2, d3) -> f d1 d2 d3)

    /// Bernoulli with probability (p).
    let bernoulli p rng = Bernoulli.Sample(rng, p)
    let bernoulliSeq p rng = Bernoulli.Samples(rng, p)

    /// Beta with α and β shape parameters.
    let beta a b rng = Beta.Sample(rng, a, b)
    let betaSeq a b rng = Beta.Samples(rng, a, b)

    /// Binomial with success probability (p) in each trial and number of trials (n).
    let binomial p n rng = Binomial.Sample(rng, p, n)
    let binomialSeq p n rng = Binomial.Samples(rng, p, n)

    /// Categorical with an array of nonnegative ratios defining the relative probability mass (unnormalized).
    let categorical probabilityMass rng = Categorical.SampleWithProbabilityMass(rng, probabilityMass)
    let categoricalSeq probabilityMass rng = Categorical.SamplesWithProbabilityMass(rng, probabilityMass)

    /// Cauchy with location (x0) and scale (γ).
    let cauchy location scale rng = Cauchy.Sample(rng, location, scale)
    let cauchySeq location scale rng = Cauchy.Samples(rng, location, scale)

    /// Chi with degrees of freedom (k).
    let chi freedom rng = Chi.Sample(rng, freedom)
    let chiSeq freedom rng = Chi.Samples(rng, freedom)

    /// Chi-Squared with degrees of freedom (k).
    let chiSquared freedom rng = ChiSquared.Sample(rng, freedom)
    let chiSquaredSeq freedom rng = ChiSquared.Samples(rng, freedom)

    /// Continuous-Uniform with lower and upper bounds.
    let continuousUniform lower upper rng = ContinuousUniform.Sample(rng, lower, upper)
    let continuousUniformSeq lower upper rng = ContinuousUniform.Samples(rng, lower, upper)

    /// Conway-Maxwell-Poisson with lambda (λ) and rate of decay (ν).
    let conwayMaxwellPoisson lambda nu rng = ConwayMaxwellPoisson.Sample(rng, lambda, nu)
    let conwayMaxwellPoissonSeq lambda nu rng = ConwayMaxwellPoisson.Samples(rng, lambda, nu)

    /// Discrete-Uniform with lower and upper bounds (both inclusive).
    let discreteUniform lower upper rng = DiscreteUniform.Sample(rng, lower, upper)
    let discreteUniformSeq lower upper rng = DiscreteUniform.Samples(rng, lower, upper)

    /// Erlang with shape (k) and rate or inverse scale (λ).
    let erlang shape rate rng = Erlang.Sample(rng, shape, rate)
    let erlangSeq shape rate rng = Erlang.Samples(rng, shape, rate)

    /// Exponential with rate (λ).
    let exponential rate rng = Exponential.Sample(rng, rate)
    let exponentialSeq rate rng = Exponential.Samples(rng, rate)

    /// Fisher-Snedecor (F-Distribution) with first (d1) and second (d2) degree of freedom.
    let fisherSnedecor d1 d2 rng = FisherSnedecor.Sample(rng, d1, d2)
    let fisherSnedecorSeq d1 d2 rng = FisherSnedecor.Samples(rng, d1, d2)

    /// Gamma with shape (k, α) and rate or inverse scale (β).
    let gamma shape rate rng = Gamma.Sample(rng, shape, rate)
    let gammaSeq shape rate rng = Gamma.Sample(rng, shape, rate)

    /// Geometric with probability (p) of generating one.
    let geometric p rng = Geometric.Sample(rng, p)
    let geometricSeq p rng = Geometric.Samples(rng, p)

    /// Hypergeometric with size of the population (N), number successes within the population (K, M) and number of draws without replacement (n).
    let hypergeometric population success draws rng = Hypergeometric.Sample(rng, population, success, draws)
    let hypergeometricSeq population success draws rng = Hypergeometric.Samples(rng, population, success, draws)

    /// Inverse-Gamma with shape (α) and scale (β)
    let inverseGamma shape scale rng = InverseGamma.Sample(rng, shape, scale)
    let inverseGammaSeq shape scale rng = InverseGamma.Samples(rng, shape, scale)

    /// Laplace with location (μ) and scale (b).
    let laplace location scale rng = Laplace.Sample(rng, location, scale)
    let laplaceSeq location scale rng = Laplace.Samples(rng, location, scale)

    /// Log-Normal with log-scale (μ) and shape (σ).
    let logNormal mu sigma rng = LogNormal.Sample(rng, mu, sigma)
    let logNormalSeq mu sigma rng = LogNormal.Samples(rng, mu, sigma)

    /// Negative-Binomial with number of failures (r) until the experiment stoppedand probability (p) of a trial resulting in success.
    let negativeBinomial r p rng = NegativeBinomial.Sample(rng, r, p)
    let negativeBinomialSeq r p rng = NegativeBinomial.Samples(rng, r, p)

    /// Normal with mean (μ) and standard deviation (σ).
    let normal mean stddev rng = Normal.Sample(rng, mean, stddev)
    let normalSeq mean stddev rng = Normal.Samples(rng, mean, stddev)

    /// Standard Gaussian.
    let standard rng = Normal.Sample(rng, 0.0, 1.0)
    let standardSeq rng = Normal.Samples(rng, 0.0, 1.0)

    /// Pareto with scale (xm) and shape (α).
    let pareto scale shape rng = Pareto.Sample(rng, scale, shape)
    let paretoSeq scale shape rng = Pareto.Samples(rng, scale, shape)

    /// Poisson with lambda (λ).
    let poisson lambda rng = Poisson.Sample(rng, lambda)
    let poissonSeq lambda rng = Poisson.Samples(rng, lambda)

    /// Rayleigh with scale (σ).
    let rayleigh scale rng = Rayleigh.Sample(rng, scale)
    let rayleighSeq scale rng = Rayleigh.Sample(rng, scale)

    /// Stable with stability (α), skewness (β), scale (c) and location (μ).
    let stable alpha beta scale location rng = Stable.Sample(rng, alpha, beta, scale, location)
    let stableSeq alpha beta scale location rng = Stable.Samples(rng, alpha, beta, scale, location)

    /// Student-T with location (μ), scale (σ) and degrees of freedom (ν).
    let studentT location scale freedom rng = StudentT.Sample(rng, location, scale, freedom)
    let studentTSeq location scale freedom rng = StudentT.Samples(rng, location, scale, freedom)

    /// Weibull with shape (k) and scale (λ).
    let weibull shape scale rng = Weibull.Sample(rng, shape, scale)
    let weibullSeq shape scale rng = Weibull.Samples(rng, shape, scale)

    /// Zipf with s and n parameters.
    let zipf s n rng = Zipf.Sample(rng, s, n)
    let zipfSeq s n rng = Zipf.Samples(rng, s, n)
