(**
Random Numbers and Probability Distributions
============================================

The .Net Framework base class library includes a pseudo-random number generator
for non-cryptography use in the form of the `System.Random` class.
Math.NET Numerics provides a few alternatives with different characteristics
in randomness, bias, sequence length and performance. All these classes
inherit from `System.Random` so you can use them as a drop-in replacement
even in third-party code.

All random number generators (RNG) generate numbers in a more-or-less uniform
distribution. In practice you often need to sample random numbers with a different
distribution, like a Gaussian or Poisson. You can do that with one of our probability
distribution classes, or in F# also using the `Sample` module. Once parametrized,
the distribution classes also provide a variety of other functionality around probability
distributions, like evaluating statistical distribution properties or functions.

Initialization
--------------

We need to reference Math.NET Numerics and open the namespaces for
random numbers and probability distributions:

    using MathNet.Numerics.Random;
    using MathNet.Numerics.Distributions;

Or in F#:
*)

// Only needed in scripts/interactive
#r "../../out/lib/Net40/MathNet.Numerics.dll"
#r "../../out/lib/Net40/MathNet.Numerics.FSharp.dll"

open MathNet.Numerics.Random
open MathNet.Numerics.Distributions

(**
Random Number Generators
------------------------

Let's sample a few uniform random values using Mersenne Twister in C#:

    var rng = new MersenneTwister(42);
    int randomInt = rng.Next();
    double randomDouble = rng.NextDouble();

In F# you can use the constructor as well, or alternatively use the `Random` module.
In case of the latter, all objects will be cast to their common base type `System.Random`:
*)

let rng = MersenneTwister(42)
let rngEx = Random.mersenneTwisterSeed 42
let randomInt = rng.Next()

(**
If you have used `System.Random` before, you may remember that it only offers `Next` methods
to sample integers, and `NextDouble` for floating point numbers in the [0,1) interval.
Did you ever have a need to generate numbers of the full integer range including negative numbers,
or a `System.Decimal`? Extending discrete random numbers to different ranges or types is non-trivial
if the distribution should still be uniform over the chosen range. That's why we've added a few extensions
methods which are available on all RNGs (including `System.Random` itself):
*)

let values =
  ( rng.Next(), // built-in: int32 in the range [0, Int.MaxValue)
    rng.NextInt64(), // int64 in the range [0, Long.MaxValue)
    rng.NextFullRangeInt32(), // int32 in the range [Int.MinValue, Int.MaxValue]
    rng.NextFullRangeInt64(), // int64 in the range [Long.MinValue, Long.MaxValue]
    rng.NextDouble(), //  built-in: double in the range [0.0, 1.0)
    rng.NextDecimal() ) // decimal in then range [0.0, 1.0)

(**
The following custom RNGs are currently available in Math.NET Numerics:

* `MersenneTwister`: Mersenne Twister 19937 generator
* `Xorshift`: Multiply-with-carry XOR-shift generator
* `Mcg31m1`: Multiplicative congruental generator using a modulus of 2^31-1 and a multiplier of 1132489760
* `Mcg59`: Multiplicative congruental generator using a modulus of 2^59 and a multiplier of 13^13
* `WH1982`: Wichmann-Hill's 1982 combined multiplicative congruental generator
* `WH2006`: Wichmann-Hill's 2006 combined multiplicative congruental generator
* `Mrg32k3a`: 32-bit combined multiple recursive generator with 2 components of order 3
* `Palf`: Parallel Additive Lagged Fibonacci generator
* `SystemCryptoRandomNumberGenerator`: Using the RNGCryptoServiceProvider of the .Net Framework. *Not available in portable builds.*

Seeds and Thread Safety
-----------------------

Other than for cryptographic random numbers where you'd never want to provide
a seed, all other RNGs can be initialized with a custom seed. In the code sample
above we've used `42` as seed. The same seed causes the same number sequence
to be generated, which can be very useful if you need results to be reproducible,
e.g. in testing/verification.

If no seed is provided, `System.Random` uses a time based seed equivalent to the
one below. This means that all instances created within a short timeframe
(which typically spans about a thousand CPU clock cycles) will generate
exactly the same sequence. This can happen easily e.g. in parallel computing
and is often unwanted. That's why all number generators created using
Math.NET Numerics routines are by default initialized with a seed that combines
the time with a Guid (which are supposed to be generated uniquely, worldwide).
*)

let someTimeSeed = RandomSeed.Time()
let someGuidSeed = RandomSeed.Guid()

(**
Note that the generators should be reused when generating multiple numbers.
If you'd create a new generator each time, the numbers it generates would be
exactly as random as your seed - and thus not very random at all.
However, generators are not automatically thread-safe in .Net. They *are* thread-safe
when created using Math.NET Numerics by default, but that can be controlled either by a
boolean argument at creation or by setting `Control.ThreadSafeRandomNumberGenerators`.
*)

let a = Random.system ()
let b = Random.systemSeed (RandomSeed.Guid())
let c = Random.crypto ()
let d = Random.mersenneTwister ()
let e = Random.mersenneTwisterWith 1000 true (* thread-safe *)
let f = Random.xorshift ()
let g = Random.xorshiftCustom someTimeSeed false 916905990L 13579L 362436069L 77465321L
let h = Random.wh2006 ()
let i = Random.palf ()

(**
Probability Distributions
-------------------------

For non-uniform random number generation you can use one the wide range of probability
distributions in the `MathNet.Numerics.Distributions` namespace.

There are many ways to parametrize a distribution in the literature. When using the
default constructor, read carefully which parameters it requires. For distributions where
multiple ways are common there are also static methods, so you can use the one that fits best.
For example, a normal distribution is usually parametrized with mean and standard deviation,
but if you'd rather use mean and precision:

    var normal = Normal.WithMeanPrecision(0.0, 0.5);

Since probability distributions can also be sampled to generate random numbers
with the configured distribution, all constructors optionally accept a random generator
as last argument. A few more examples, this time in F#:
*)

// some probability distributions
let normal = Normal.WithMeanVariance(3.0, 1.5, g)
let exponential = Exponential(2.4)
let gamma = Gamma(2.0, 1.5, Random.crypto())
let cauchy = Cauchy(0.0, 1.0, Random.mrg32k3aWith 10 false)
let poisson = Poisson(3.0)
let geometric = Geometric(0.8, Random.system())

// sample some random rumbers from these distributions
let continuous =
  [ yield normal.Sample()
    yield exponential.Sample()
    yield! gamma.Samples() |> Seq.take 10 ]

let discrete =
  [ poisson.Sample()
    poisson.Sample()
    geometric.Sample() ]

// direct sampling (without creating a distribution object)
let u = Normal.Sample(Random.system(), 2.0, 4.0)
let v = Laplace.Samples(Random.mersenneTwister(), 1.0, 3.0) |> Seq.take 100 |> List.ofSeq
let w = Rayleigh.Sample(c, 1.5)
let x = Hypergeometric.Sample(h, 100, 20, 5)

(**
Distribution Functions and Properties
-------------------------------------

Distributions can not just be used to generate non-uniform random samples.
Once parametrized they can compute a variety of distribution properties
or evaluate distribution functions. Because it is often numerically more stable
and faster to compute and work with such quantities in the logarithmic domain,
some of them are also available with the `Ln`-suffix.
*)

// distribution properties of the gamma we've configured above
let gammaStats =
  ( gamma.Mean,
    gamma.Variance,
    gamma.StdDev,
    gamma.Entropy,
    gamma.Skewness,
    gamma.Mode )

// probability distribution functions of the normal we've configured above.
let nd = normal.Density(4.0)  (* pdf *)
let ndLn = normal.DensityLn(4.0)  (* ln(pdf) *)
let nc = normal.CumulativeDistribution(4.0)  (* cdf *)
let nic = normal.InverseCumulativeDistribution(0.7)  (* invcdf *)

// Distribution functions can also be evaluated without creating an object,
// but then you have to pass in the distribution parameters as first arguments:
let nd2 = Normal.PDF(3.0, sqrt 1.5, 4.0)
let ndLn2 = Normal.PDFLn(3.0, sqrt 1.5, 4.0)
let nc2 = Normal.CDF(3.0, sqrt 1.5, 4.0)
let nic2 = Normal.InvCDF(3.0, sqrt 1.5, 0.7)

(**
Some of the distributions also have routines for maximum-likelihood parameter
estimation from a set of samples:
*)

let estimation = LogNormal.Estimate([| 2.0; 1.5; 2.1; 1.2; 3.0; 2.4; 1.8 |])
let mean, variance = estimation.Mean, estimation.Variance
let moreSamples = estimation.Samples() |> Seq.take 10 |> Seq.toArray

(**
or in C#:

    LogNormal estimation = LogNormal.Estimate(new [] {2.0, 1.5, 2.1, 1.2, 3.0, 2.4, 1.8});
    double mean = estimation.Mean, variance = estimation.Variance;
    double[] moreSamples = estimation.Samples().Take(10).ToArray();

Let's do some random walks, using distributions and random sources defined above (TODO: Graph):
*)

Seq.scan (+) 0.0 (normal.Samples()) |> Seq.take 10 |> Seq.toArray
Seq.scan (+) 0.0 (Sample.normalSeq 0.0 0.5 a) |> Seq.take 10 |> Seq.toArray

(**
Composing Distributions
-----------------------

Specifically for F# there is also a `Sample` module that allows a somewhat more functional
view on distribution sampling functions by having the random source passed in as last argument.
This way they can be composed and transformed arbitrarily if curried:
*)

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

// The random walk from above, but this time using the composition from above
Seq.scan (+) 0.0 (s1s a) |> Seq.take 10 |> Seq.toArray
