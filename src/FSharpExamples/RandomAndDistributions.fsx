(**

Math.NET Numerics: Random Numbers and Probability Distributions
===============================================================

The .Net base class library provides a pseudo-random number generator
for non-cryptography use in the form of the `System.Numerics` class.
Math.NET Numerics provides a few alternatives with different characteristics
in randomness, bias, sequence length and performance. All these classes
inherit from System.Random so you can use them as a replacement for
System.Random even in third-party code.

All random number generators generate numbers in a more-or-less uniform
distribution. Often you need to sample random numbers with a different
distribution, e.g. a Gaussian. You can do that with one of the distribution
classes, or in F# also using the `Sample` module. The distribution classes
also provide a lot of other functionality around probability distributions,
like parameter estimation or evaluating the commulative distribution function.

Initializtation
---------------

We need to reference Math.NET Numerics and the F# modules, and open
the namespaces for random numbers and probability distributions:

*)

#r "../../out/lib/Net40/MathNet.Numerics.dll"
#r "../../out/lib/Net40/MathNet.Numerics.FSharp.dll"

open MathNet.Numerics.Random
open MathNet.Numerics.Distributions

(**

Random Number Generators
------------------------

Other than for cryptographic random numbers where you'd never want to provide
a seed, all pseudo-random numbers can be initialized with a custom seed.
The same seed causes the same number sequence to be generated, which can be
very useful if you need results to be reproducible, e.g. in testing/verification.

If no seed is provided, System.Random uses a time based seed equivalent to the
one below. This means that all instances created within a short timeframe
(which typically spans around a thousand CPU clock cycles) will generate
exactly the same sequence. This can happen easily e.g. in parallel computing
and is often unwanted. That's why all number generators created using
Math.NET Numerics routines are by default initialized with a seed that combines
the time with a Guid (which are supposed to be generated uniquely, worldwide).

*)

let someTimeSeed = RandomSeed.Time()
let someGuidSeed = RandomSeed.Guid()

(**

Random number generators can be created using the `Random` module. Most functions
optionally accept a manual seed when using the variant with the Seed-suffix.
Some of them, like xor-shift, also have a variant with a Custom-suffix that
allow to pass additional parameters specific to that generator.

Note that the generators should be reused when generating multiple numbers.
If you'd create a new generator each time, the numbers it generates would be
exactly as random as your seed - and thus not very random at all.
However, generators are not automatically thread-safe. They *are* thread-safe
in Math.NET Numerics by default, but that can be disabled either using a
boolean argument at creation, or by setting `Control.ThreadSafeRandomNumberGenerators`.

*)

let a = Random.system ()
let b = Random.systemSeed (RandomSeed.Time())
let b2 = Random.systemSeed someGuidSeed
let c = Random.crypto ()
let d = Random.mersenneTwister ()
let e = Random.mersenneTwisterWith 1000 true (* thread-safe *)
let f = Random.xorshift ()
let g = Random.xorshiftCustom someTimeSeed false 916905990L 13579L 362436069L 77465321L
let h = Random.wh2006 ()
let i = Random.palf ()

// Generate some uniform random values
let values = (
        a.Next(),
        b.NextFullRangeInt32(),
        c.NextFullRangeInt64(),
        d.NextInt64(),
        e.NextDouble(),
        f.NextDecimal()
    )

(**

Probability Distributions
-------------------------

Non-uniform probability distributions can be created using their normal constructor,
some also offer static functions if there are multiple ways to parametrize them.

Since probability distributions can also be sampled to generate random numbers
with the configured distribution, all constructors optionally accept a random generator
as last argument.

*)

// some probability distributions
let normal = Normal.WithMeanVariance(3.0, 1.5, g)
let exponential = Exponential(2.4)
let gamma = Gamma(2.0, 1.5, Random.crypto())
let cauchy = Cauchy(0.0, 1.0, Random.mrg32k3aWith 10 false)
let poisson = Poisson(3.0)
let geometric = Geometric(0.8, Random.system())

// sample some random rumbers from these distributions
let continuous = [
        yield normal.Sample()
        yield exponential.Sample()
        yield! gamma.Samples() |> Seq.take 10
    ]
let discrete = [
        poisson.Sample()
        poisson.Sample()
        geometric.Sample()
    ]


// direct sampling (without creating a distribution object)
let u = Normal.Sample(Random.system(), 2.0, 4.0)
let v = Laplace.Samples(Random.mersenneTwister(), 1.0, 3.0) |> Seq.take 100 |> List.ofSeq
let w = Rayleigh.Sample(c, 1.5)
let x = Hypergeometric.Sample(h, 100, 20, 5)

(**

Specifically for F# there is also a `Sample` module that allow a somewhat
more functional view on the distributions by allowing them to be curried such that
the random source is passed in as last arguments. This way distributions can
be combined and transformed arbitrarily:

*)

/// Transform a sample distribution
let s1 rng = tanh (Sample.normal 2.0 0.5 rng)

/// Alternative way where we transform the function instead of its result
let s1alt rng = Sample.transform tanh (Sample.normal 2.0 0.5) rng

/// Alternative way that works exactly the same but operates on functions generating sequences
let s1seq rng = Sample.transformSeq tanh (Sample.normalSeq 2.0 0.5) rng

/// The same with multiple distributions:
let s2 rng = (Sample.normal 2.0 1.5 rng) * (Sample.cauchy 2.0 0.5 rng)
let s2alt rng = Sample.transform2 (*) (Sample.normal 2.0 1.5) (Sample.cauchy 2.0 0.5) rng
let s2seq rng = Sample.transformSeq2 (*) (Sample.normalSeq 2.0 1.5) (Sample.cauchySeq 2.0 0.5) rng

Seq.take 10 (s2seq (Random.system())) |> Seq.toArray

(**

Let's do some random walks, using distributions and random sources defined above:

*)

Seq.scan (+) 0.0 (normal.Samples()) |> Seq.take 10 |> Seq.toArray
Seq.scan (+) 0.0 (Sample.normalSeq 0.0 0.5 a) |> Seq.take 10 |> Seq.toArray
Seq.scan (+) 0.0 (s1seq a) |> Seq.take 10 |> Seq.toArray

(**

Distributions can not just be used to generate random samples.
You can use them to evaluate distribution properties or functions
with the given parametrization.

*)

// distribution properties of the gamma dist we configured above
let gammaStats = (
        gamma.Mean,
        gamma.Variance,
        gamma.StdDev,
        gamma.Entropy,
        gamma.Skewness,
        gamma.Mode
    )

// probability distribution functions of the normal dist we configured above
let nd = normal.Density(4.0) (* pdf *)
let ndLn = normal.DensityLn(4.0) (* ln(pdf) *)
let nc = normal.CumulativeDistribution(4.0) (* cdf *)
let nic = normal.InverseCumulativeDistribution(0.7) (* invcdf *)

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
