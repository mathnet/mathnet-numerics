    [hide]
    #I "../../out/lib/net40"
    #r "MathNet.Numerics.dll"
    #r "MathNet.Numerics.FSharp.dll"
    open System.Numerics
    open MathNet.Numerics
    open MathNet.Numerics.Random
    open MathNet.Numerics.Distributions

Probability Distributions
=========================

Math.NET Numerics provides a wide range of probability distributions. Given the
distribution parameters they can be used to investigate their statistical properties
or to sample non-uniform random numbers.

All the distributions implement a common set of operations such as
evaluating the density (PDF) and the cumulative distribution (CDF)
at a given point, or to compute the mean, standard deviation and other properties.
Because it is often numerically more stable and faster to compute such statistical quantities
in the logarithmic domain, we also provide a selection of them in the log domain with the "Ln" suffix,
e.g. DensityLn for the logarithmic density.

    [lang=csharp]
    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.Random;

    // create a parametrized distribution instance
    var gamma = new Gamma(2.0, 1.5);

    // distribution properties
    double mean = gamma.Mean;
    double variance = gamma.Variance;
    double entropy = gamma.Entropy;

    // distribution functions
    double a = gamma.Density(2.3); // PDF
    double b = gamma.DensityLn(2.3); // ln(PDF)
    double c = gamma.CumulativeDistribution(0.7); // CDF

    // non-uniform number sampling
    double randomSample = gamma.Sample();

Both probability functions and sampling are also available as static functions
for simpler usage scenarios:

    [lang=csharp]
    // distribution parameters must be passed as arguments
    double a2 = Gamma.PDF(2.0, 1.5, 2.3);
    double randomSample2 = Gamma.Sample(2.0, 1.5);

<div style="overflow:auto;">
<div style="float: left; width: 50%;">

### Continuous Distributions

* [Continuous Uniform](http://en.wikipedia.org/wiki/Uniform_distribution_%28continuous%29)
* [Normal](http://en.wikipedia.org/wiki/Normal_distribution)
* [Log Normal](http://en.wikipedia.org/wiki/Log-normal_distribution)
* [Beta](http://en.wikipedia.org/wiki/Beta_distribution)
* [Cauchy](http://en.wikipedia.org/wiki/cauchy_distribution) (Cauchy-Lorentz)
* [Chi](http://en.wikipedia.org/wiki/Chi_distribution)
* [Chi Squared](http://en.wikipedia.org/wiki/Chi-square_distribution)
* [Erlang](http://en.wikipedia.org/wiki/Erlang_distribution)
* [Exponential](http://en.wikipedia.org/wiki/exponential_distribution)
* [Fisher-Snedecor](http://en.wikipedia.org/wiki/F-distribution) (F-Distribution)
* [Gamma](http://en.wikipedia.org/wiki/Gamma_distribution)
* [Inverse Gamma](http://en.wikipedia.org/wiki/inverse-gamma_distribution)
* [Laplace](http://en.wikipedia.org/wiki/Laplace_distribution)
* [Pareto](http://en.wikipedia.org/wiki/Pareto_distribution)
* [Rayleigh](http://en.wikipedia.org/wiki/Rayleigh_distribution)
* [Stable](http://en.wikipedia.org/wiki/Stable_distribution)
* [Stundent-T](http://en.wikipedia.org/wiki/Student%27s_t-distribution)
* [Weibull](http://en.wikipedia.org/wiki/Weibull_distribution)
* [Triangular](https://en.wikipedia.org/wiki/Triangular_distribution)

</div>
<div style="float: right; width: 50%;">

### Discrete Distributions

* [Discrete Uniform](http://en.wikipedia.org/wiki/Uniform_distribution_%28discrete%29)
* [Bernoulli](http://en.wikipedia.org/wiki/Bernoulli_distribution)
* [Binomial](http://en.wikipedia.org/wiki/Binomial_distribution)
* [Negative Binomial](http://en.wikipedia.org/wiki/Negative_binomial_distribution)
* [Geometric](http://en.wikipedia.org/wiki/geometric_distribution)
* [Hypergeometric](http://en.wikipedia.org/wiki/Hypergeometric_distribution)
* [Poisson](http://en.wikipedia.org/wiki/Poisson_distribution)
* [Categorical](http://en.wikipedia.org/wiki/Categorical_distribution)
* [Conway-Maxwell-Poisson](http://en.wikipedia.org/wiki/Conway%E2%80%93Maxwell%E2%80%93Poisson_distribution)
* [Zipf](http://en.wikipedia.org/wiki/Zipf%27s_law)

### Multivariate Distributions

* [Dirichlet](http://en.wikipedia.org/wiki/Dirichlet_distribution)
* [Inverse Wishart](http://en.wikipedia.org/wiki/Inverse-Wishart_distribution)
* [Matrix Normal](http://en.wikipedia.org/wiki/Matrix_normal_distribution)
* [Multinomial](http://en.wikipedia.org/wiki/Multinomial_distribution)
* [Normal Gamma](http://en.wikipedia.org/wiki/Normal-gamma_distribution)
* [Wishart](http://en.wikipedia.org/wiki/Wishart_distribution)

</div>
</div>


Distribution Parameters
-----------------------

There are many ways to parametrize a distribution in the literature. When using the
default constructor, read carefully which parameters it requires. For distributions where
multiple ways are common there are also static methods, so you can use the one that fits best.
For example, a normal distribution is usually parametrized with mean and standard deviation,
but if you'd rather use mean and precision:

    [lang=csharp]
    var normal = Normal.WithMeanPrecision(0.0, 0.5);

Since probability distributions can also be sampled to generate random numbers
with the configured distribution, all constructors optionally accept a random generator
as last argument.

    [lang=csharp]
    var gamma2 = new Gamma(2.0, 1.5, new MersenneTwister());

    // the random generator can also be replaced on an existing instance
    gamma2.RandomSource = new Mrg32k3a();

A few more examples, this time in F#:

    [lang=fsharp]
    // some probability distributions
    let normal = Normal.WithMeanVariance(3.0, 1.5, a)
    let exponential = Exponential(2.4)
    let gamma = Gamma(2.0, 1.5, Random.crypto())
    let cauchy = Cauchy(0.0, 1.0, Random.mrg32k3aWith 10 false)
    let poisson = Poisson(3.0)
    let geometric = Geometric(0.8, Random.system())

Some of the distributions also have routines for maximum-likelihood parameter
estimation from a set of samples:

    [lang=fsharp]
    let estimation = LogNormal.Estimate([| 2.0; 1.5; 2.1; 1.2; 3.0; 2.4; 1.8 |])
    let mean, variance = estimation.Mean, estimation.Variance
    let moreSamples = estimation.Samples() |> Seq.take 10 |> Seq.toArray

or in C#:

    [lang=csharp]
    LogNormal estimation = LogNormal.Estimate(new [] {2.0, 1.5, 2.1, 1.2, 3.0, 2.4, 1.8});
    double mean = estimation.Mean, variance = estimation.Variance;
    double[] moreSamples = estimation.Samples().Take(10).ToArray();


Sampling a Probability Distribution
-----------------------------------

Each distribution provides methods to generate random numbers from that distribution.
These random variate generators work by accessing the distribution's member RandomSource
to provide uniform random numbers. By default, this member is an instance of System.Random
but one can easily replace this with more sophisticated random number generators from
`MathNet.Numerics.Random` (see [Random Numbers](Random.html) for details).

    [lang=fsharp]
    // sample some random numbers from these distributions
    // continuous distributions sample to floating-point numbers:
    let continuous =
      [ yield normal.Sample()
        yield exponential.Sample()
        yield! gamma.Samples() |> Seq.take 10 ]

    // discrete distributions on the other hand sample to integers:
    let discrete =
      [ poisson.Sample()
        poisson.Sample()
        geometric.Sample() ]

Instead of creating a distribution object we can also sample directly with static functions.
Note that no intermediate value caching is possible this way and parameters must be validated on each call.

    [lang=fsharp]
    // using the default number generator (SystemRandomSource.Default)
    let w = Rayleigh.Sample(1.5)
    let x = Hypergeometric.Sample(100, 20, 5)

    // or by manually providing the uniform random number generator
    let u = Normal.Sample(Random.system(), 2.0, 4.0)
    let v = Laplace.Samples(Random.mersenneTwister(), 1.0, 3.0) |> Seq.take 100 |> List.ofSeq

If you need to sample not just one or two values but a large number of them,
there are routines that either fill an existing array or return an enumerable.
The variant that fills an array is generally the fastest. Routines to sample
more than one value use the plural form `Samples` instead of `Sample`.

Let's sample 100'000 values from a laplace distribution with mean 1.0 and scale 2.0 in C#:

    [lang=csharp]
    var samples = new double[100000];
    Laplace.Samples(samples, 1.0, 2.0);

Let's do some random walks in F# (TODO: Graph):

    [lang=fsharp]
    Seq.scan (+) 0.0 (Normal.Samples(0.0, 1.0)) |> Seq.take 10 |> Seq.toArray
    Seq.scan (+) 0.0 (Cauchy.Samples(0.0, 1.0)) |> Seq.take 10 |> Seq.toArray


Distribution Functions and Properties
-------------------------------------

Distributions can not just be used to generate non-uniform random samples.
Once parametrized they can compute a variety of distribution properties
or evaluate distribution functions. Because it is often numerically more stable
and faster to compute and work with such quantities in the logarithmic domain,
some of them are also available with the `Ln`-suffix.

    [lang=fsharp]
    // distribution properties of the gamma we've configured above
    let gammaStats =
      ( gamma.Mean,
        gamma.Variance,
        gamma.StdDev,
        gamma.Entropy,
        gamma.Skewness,
        gamma.Mode )

    // probability distribution functions of the normal we've configured above.
    let nd = normal.Density(4.0)  (* PDF *)
    let ndLn = normal.DensityLn(4.0)  (* ln(PDF) *)
    let nc = normal.CumulativeDistribution(4.0)  (* CDF *)
    let nic = normal.InverseCumulativeDistribution(0.7)  (* CDF^(-1) *)

    // Distribution functions can also be evaluated without creating an object,
    // but then you have to pass in the distribution parameters as first arguments:
    let nd2 = Normal.PDF(3.0, sqrt 1.5, 4.0)
    let ndLn2 = Normal.PDFLn(3.0, sqrt 1.5, 4.0)
    let nc2 = Normal.CDF(3.0, sqrt 1.5, 4.0)
    let nic2 = Normal.InvCDF(3.0, sqrt 1.5, 0.7)


Composing Distributions
-----------------------

Specifically for F# there is also a `Sample` module that allows a somewhat more functional
view on distribution sampling functions by having the random source passed in as last argument.
This way they can be composed and transformed arbitrarily if curried:

    [lang=fsharp]
    /// Transform a sample from a distribution
    let s1 rng = tanh (Sample.normal 2.0 0.5 rng)

    /// But we really want to transform the function, not the resulting sample:
    let s1f rng = Sample.map tanh (Sample.normal 2.0 0.5) rng

    /// Exactly the same also works with functions generating full sequences
    let s1s rng = Sample.mapSeq tanh (Sample.normalSeq 2.0 0.5) rng

    /// Now with multiple distributions, e.g. their product:
    let s2 rng = (Sample.normal 2.0 1.5 rng) * (Sample.cauchy 2.0 0.5 rng)
    let s2f rng = Sample.map2 (*) (Sample.normal 2.0 1.5) (Sample.cauchy 2.0 0.5) rng
    let s2s rng = Sample.mapSeq2 (*) (Sample.normalSeq 2.0 1.5) (Sample.cauchySeq 2.0 0.5) rng

    // Taking some samples from the composed function
    Seq.take 10 (s2s (Random.system())) |> Seq.toArray
