(*** hide ***)
#I "../../out/lib/net40"
#r "MathNet.Numerics.dll"
#r "MathNet.Numerics.FSharp.dll"
open MathNet.Numerics.Random
open MathNet.Numerics.Distributions

(**
Random Numbers and Probability Distributions
============================================

The .Net Framework base class library (BCL) includes a pseudo-random number generator
for non-cryptography use in the form of the `System.Random` class.
Math.NET Numerics provides a few alternatives with different characteristics
in randomness, bias, sequence length, performance and thread-safety. All these classes
inherit from `System.Random` so you can use them as a drop-in replacement
even in third-party code.

All random number generators (RNG) generate numbers in a uniform
distribution. In practice you often need to sample random numbers with a different
distribution, like a Gaussian or Poisson. You can do that with one of our probability
distribution classes, or in F# also using the `Sample` module. Once parametrized,
the distribution classes also provide a variety of other functionality around probability
distributions, like evaluating statistical distribution properties or functions.


Initialization
--------------

We need to reference Math.NET Numerics and open the namespaces for
random numbers and probability distributions:

    [lang=csharp]
    using MathNet.Numerics.Random;
    using MathNet.Numerics.Distributions;


Generating Random Numbers
-------------------------

Let's sample a few random values from a uniform distributed variable
$X\sim\mathcal{U}(0,1)$, such that $0 \le x < 1$:

    [lang=csharp]
    // create an array with 1000 random values
    double[] samples = SystemRandomSource.Doubles(1000);

    // now overwrite the existing array with new random values
    SystemRandomSource.Doubles(samples);

    // we can also create an infinite sequence of random values:
    IEnumerable<double> sampleSeq = SystemRandomSource.DoubleSequence();

    // take a single random value
    System.Random rng = SystemRandomSource.Default;
    double sample = rng.NextDouble();
    decimal sampled = rng.NextDecimal();

In F# we can do exactly the same, or alternatively use the `Random` module:
*)

let samples = Random.doubles 1000

// overwrite the whole array with new random values
Random.doubleFill samples

// create an infinite sequence:
let sampleSeq = Random.doubleSeq ()

// take a single random value
let rng = Random.shared
let sample = rng.NextDouble()
let sampled = rng.NextDecimal()

(**
If you have used the .Net BCL random number generators before, you have likely
noticed a few differences: we used special routines to create a full array or
sequence in one go, we were able to sample a decimal number, an we used static functions
and a shared default instance instead of creating our own instance.

Math.NET Numerics provides a few alternative random number generators in their own types.
For example, `MersenneTwister` implements the very popular mersenne twister algorithm. All these types
inherit from `System.Random`, are fully compatible to it and can also be used exactly the same way:

    [lang=csharp]
    System.Random random = new SystemRandomSource();
    var sample = random.NextDouble();

However, unlike System.Random they can be made thread safe, use much more reasonable
default seeds and have some convenient extra routines. The `SystemRandomSource` class that
was used above uses System.Random to generate random numbers internally - but with all the extras.


Full Range Integers and Decimal
-------------------------------

Out of the box, `System.Random` only provides `Next` methods to sample integers
in the [0, Int.MaxValue) range and `NextDouble` for floating point numbers in the [0,1) interval.
Did you ever have a need to generate numbers of the full integer range including negative numbers,
or a `System.Decimal`? Extending discrete random numbers to different ranges or types is non-trivial
if the distribution should still be uniform over the chosen range. That's why we've added a few extensions
methods which are available on all RNGs (including System.Random itself):

* **NextInt64** generates a 64 bit integer, uniform in the range [0, Long.MaxValue)
* **NextDecimal** generates a `System.Decimal`, uniform in the range [0.0, 1.0)
* **NextFullRangeInt32** generates a 32 bit integer, uniform in the range [Int.MinValue, Int.MaxValue]
* **NextFullRangeInt64** generates a 64 bit integer, uniform in the range [Long.MinValue, Long.MaxValue]


Seeds
-----

All RNGs can be initialized with a custom seed number. The same seed causes
the same number sequence to be generated, which can be very useful if you need results
to be reproducible, e.g. in testing/verification. The exception is cryptography,
where reproducible random number sequences would be a fatal security flaw,
so our crypto random source does not accept a seed.

In the code samples above we did not provide a seed, so a default seed was used.
If no seed is provided, `System.Random` uses a time based seed equivalent to the
one below. This means that all instances created within a short time-frame
(which typically spans about a thousand CPU clock cycles) will generate
exactly the same sequence. This can happen easily e.g. in parallel computing
and is often unwanted. That's why all Math.NET Numerics RNGs are by default
initialized with a robust seed taken from the `CryptoRandomSource` if available,
or else a combination of a random number from a shared RNG, the time and a Guid
(which are supposed to be generated uniquely, worldwide).
*)

let someTimeSeed = RandomSeed.Time() // not recommended
let someGuidSeed = RandomSeed.Guid()
let someRobustSeed = RandomSeed.Robust() // recommended, used by default

(** Let's generate random numbers like before, but this time with custom seed 42: *)

let samplesSeeded = Random.doublesSeed 42 1000
Random.doubleFillSeed 42 samplesSeeded
let samplesSeqSeeded = Random.doubleSeqSeed 42

(**
Or without the F# Random module, e.g. in C#:

    [lang=csharp]
    double[] samplesSeeded = SystemRandomSource.Doubles(1000, 42);
    SystemRandomSource.Doubles(samplesSeeded, 42);
    IEnumerable<double> sampleSeqSeeded = SystemRandomSource.DoubleSequence(42);


Uniform Random Number Generators
--------------------------------

Up to now we've used only `SystemRandomSource`, but there's much more:

* **SystemRandomSource**: Wraps the .NET System.Random to provide thread-safety
* **CryptoRandomSource**: Wraps the .NET RNGCryptoServiceProvider. *Not available in portable builds.*
* **MersenneTwister**: Mersenne Twister 19937 generator
* **Xorshift**: Multiply-with-carry XOR-shift generator
* **Mcg31m1**: Multiplicative congruential generator using a modulus of 2^31-1 and a multiplier of 1132489760
* **Mcg59**: Multiplicative congruential generator using a modulus of 2^59 and a multiplier of 13^13
* **WH1982**: Wichmann-Hill's 1982 combined multiplicative congruential generator
* **WH2006**: Wichmann-Hill's 2006 combined multiplicative congruential generator
* **Mrg32k3a**: 32-bit combined multiple recursive generator with 2 components of order 3
* **Palf**: Parallel Additive Lagged Fibonacci generator

Let's sample a few uniform random values using Mersenne Twister in C#:

    [lang=csharp]
    // Typical way with an instance:
    var random = new MersenneTwister(42); // seed 42
    int sampleInt = random.Next();
    double sampleDouble = random.NextDouble();
    decimal sampleDecimal = random.NextDecimal();
    double[] samples = random.NextDoubles(1000);
    IEnumerable<double> sampleSeq = random.NextDoubleSequence();

    // Simpler and faster if you need a large sequence, only once:
    double[] samples = MersenneTwister.Doubles(1000, 42) // 1000 numbers, seed 42
    IEnumerable<double> sampleSeq = MersenneTwister.DoubleSequence(42); // seed 42

In F# you can use the constructor as well, or alternatively use the `Random` module.
In case of the latter, all objects will be cast to their common base type `System.Random`:
*)

// By using the normal constructor (random1 has type MersenneTwister) 
let random1 = MersenneTwister()
let random1b = MersenneTwister(42) // with seed

// By using the Random module (random2 has type System.Random)
let random2 = Random.mersenneTwister ()
let random2b = Random.mersenneTwisterSeed 42 // with seed
let random2c = Random.mersenneTwisterWith 42 false // opt-out of thread-safety

// Using some other algorithms:
let random3 = Random.crypto ()
let random4 = Random.xorshift ()
let random5 = Random.wh2006 ()

(**
Shared Instances and Thread Safety
----------------------------------

Generators make certain claims about how many random numbers they can generate
until the whole sequence repeats itself. However, this only applies if you
continue to sample from the same instance and its internal state.
The generator instances should therefore be reused within an application if long
random sequences are needed. If you'd create a new instance each time, the numbers
it generates would be exactly as random as your seed - and thus not very random at all.

Another reason to share instances: most generators run an initialization routine before
they can start generating numbers which can be expensive. Some of them also maintain
their internal state in large memory blocks, which can quickly add up when creating multiple
instances.

Unfortunately the two generators provided by .NET are not thread-safe and thus cannot be
shared between threads without manual locking. But all the RNGs provided by Math.NET Numerics,
including the `SystemRandomSource` and `CryptoRandomSource` wrappers, *are* thread-safe by default,
unless explicitly disabled by a constructor argument or by setting `Control.ThreadSafeRandomNumberGenerators`
(which is used if the constructor argument is omitted).

For convenience a few generators provide a thread-safe shared instance

    [lang=csharp]
    var a = SystemRandomSource.Default;
    var b = MersenneTwister.Default;

Or with the F# module:
*)

let a = Random.systemShared
let b = Random.mersenneTwisterShared

// or if you don't care, simply
let c = Random.shared;

(**
Probability Distributions
-------------------------

For non-uniform random number generation you can use the wide range of probability
distributions in the `MathNet.Numerics.Distributions` namespace.

There are many ways to parametrize a distribution in the literature. When using the
default constructor, read carefully which parameters it requires. For distributions where
multiple ways are common there are also static methods, so you can use the one that fits best.
For example, a normal distribution is usually parametrized with mean and standard deviation,
but if you'd rather use mean and precision:

    [lang=csharp]
    var normal = Normal.WithMeanPrecision(0.0, 0.5);

Since probability distributions can also be sampled to generate random numbers
with the configured distribution, all constructors optionally accept a random generator
as last argument. A few more examples, this time in F#:
*)

// some probability distributions
let normal = Normal.WithMeanVariance(3.0, 1.5, a)
let exponential = Exponential(2.4)
let gamma = Gamma(2.0, 1.5, Random.crypto())
let cauchy = Cauchy(0.0, 1.0, Random.mrg32k3aWith 10 false)
let poisson = Poisson(3.0)
let geometric = Geometric(0.8, Random.system())

// sample some random numbers from these distributions
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
let x = Hypergeometric.Sample(b, 100, 20, 5)

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

    [lang=csharp]
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
