module MCMC

open MathNet.Numerics
open MathNet.Numerics.Random
open MathNet.Numerics.Statistics
open MathNet.Numerics.Distributions
open MathNet.Numerics.Statistics.Mcmc

/// The number of samples to gather for each sampler.
let N = 10000
/// The random number generator we use for the examples.
let rnd = new MersenneTwister()

//
// Example 1: Sampling a Beta distributed variable through rejection sampling.
//
// Target Distribution: Beta(2.7, 6.3)
//
// -----------------------------------------------------------------------------
do
    printfn "Rejection Sampling Example"

    /// The target distribution.
    let beta = new Beta(2.7, 6.3)

    /// Samples uniform distributed variables.
    let uniform = new ContinuousUniform(0.0, 1.0, RandomSource = rnd)

    /// Implements the rejection sampling procedure.
    let rs = new RejectionSampler<float>( ( fun x -> x**(beta.A-1.0) * (1.0 - x)**(beta.B-1.0) ),
                                          ( fun x -> 0.021 ),
                                          ( fun () -> uniform.Sample()) )

    /// An array of samples from the rejection sampler.
    let arr = rs.Sample(N)

    /// The true distribution.
    printfn "\tEmpirical Mean = %f (should be %f)" (Statistics.Mean(arr)) beta.Mean
    printfn "\tEmpirical StdDev = %f (should be %f)" (Statistics.StandardDeviation(arr)) beta.StdDev
    printfn "\tAcceptance rate = %f" rs.AcceptanceRate
    printfn ""



//
// Example 2: Sampling a normal distributed variable through Metropolis sampling.
//
// Target Distribution: Normal(1.0, 3.5)
//
// -----------------------------------------------------------------------------
do
    printfn "Metropolis Sampling Example"

    let mean, stddev = 1.0, 3.5
    let normal = new Normal(mean, stddev)

    /// Implements the rejection sampling procedure.
    let ms = new MetropolisSampler<float>( 0.1, (fun x -> log(normal.Density(x))),
                                                (fun x -> Normal.Sample(rnd, x, 0.3)), 20,
                                                RandomSource = rnd )

    /// An array of samples from the rejection sampler.
    let arr = ms.Sample(N)

    /// The true distribution.
    printfn "\tEmpirical Mean = %f (should be %f)" (Statistics.Mean(arr)) normal.Mean
    printfn "\tEmpirical StdDev = %f (should be %f)" (Statistics.StandardDeviation(arr)) normal.StdDev
    printfn "\tAcceptance rate = %f" ms.AcceptanceRate
    printfn ""



//
// Example 3: Sampling a normal distributed variable through Metropolis-Hastings sampling
//              with a symmetric proposal distribution.
//
// Target Distribution: Normal(1.0, 3.5)
//
// -----------------------------------------------------------------------------------------
do
    printfn "Metropolis Hastings Sampling Example (Symmetric Proposal)"
    let mean, stddev = 1.0, 3.5
    let normal = new Normal(mean, stddev)

    /// Evaluates the log normal distribution.
    let npdf x m s = -0.5*(x-m)*(x-m)/(s*s) - 0.5 * log(Constants.Pi2 * s * s)

    /// Implements the rejection sampling procedure.
    let ms = new MetropolisHastingsSampler<float>( 0.1, (fun x -> log(normal.Density(x))),
                    (fun x y -> npdf x y 0.3), (fun x -> Normal.Sample(rnd, x, 0.3)), 10,
                                                RandomSource = rnd )

    /// An array of samples from the rejection sampler.
    let arr = ms.Sample(N)

    /// The true distribution.
    printfn "\tEmpirical Mean = %f (should be %f)" (Statistics.Mean(arr)) normal.Mean
    printfn "\tEmpirical StdDev = %f (should be %f)" (Statistics.StandardDeviation(arr)) normal.StdDev
    printfn "\tAcceptance rate = %f" ms.AcceptanceRate
    printfn ""



//
// Example 4: Sampling a normal distributed variable through Metropolis-Hastings sampling
//              with a asymmetric proposal distribution.
//
// Target Distribution: Normal(1.0, 3.5)
//
// -----------------------------------------------------------------------------------------
do
    printfn "Metropolis Hastings Sampling Example (Assymetric Proposal)"
    let mean, stddev = 1.0, 3.5
    let normal = new Normal(mean, stddev)

    /// Evaluates the logarithm of the normal distribution function.
    let npdf x m s = -0.5*(x-m)*(x-m)/(s*s) - 0.5 * log(Constants.Pi2 * s * s)

    /// Samples from a mixture that is biased towards samples larger than x.
    let mixSample x =
        if Bernoulli.Sample(rnd, 0.5) = 1 then
            Normal.Sample(rnd, x, 0.3)
        else
            Normal.Sample(rnd, x + 0.1, 0.3)

    /// The transition kernel for the proposal above.
    let krnl xnew x = log (0.5 * exp(npdf xnew x 0.3) + 0.5 * exp(npdf xnew (x+0.1) 0.3))

    /// Implements the rejection sampling procedure.
    let ms = new MetropolisHastingsSampler<float>( 0.1, (fun x -> log(normal.Density(x))),
                    (fun xnew x -> krnl xnew x), (fun x -> mixSample x), 10,
                                                RandomSource = rnd )

    /// An array of samples from the rejection sampler.
    let arr = ms.Sample(N)

    /// The true distribution.
    printfn "\tEmpirical Mean = %f (should be %f)" (Statistics.Mean(arr)) normal.Mean
    printfn "\tEmpirical StdDev = %f (should be %f)" (Statistics.StandardDeviation(arr)) normal.StdDev
    printfn "\tAcceptance rate = %f" ms.AcceptanceRate
    printfn ""



//
// Example 5: Slice sampling a normal distributed random variable.
//
// Target Distribution: Normal(1.0, 3.5)
//
// -----------------------------------------------------------------------------------------
do
    printfn "Slice Sampling Example"
    let mean, stddev = 1.0, 3.5
    let normal = new Normal(mean, stddev)

    /// Evaluates the unnormalized logarithm of the normal distribution function.
    let npdf x m s = -0.5*(x-m)*(x-m)/(s*s)

    /// Implements the rejection sampling procedure.
    let ms = new UnivariateSliceSampler( 0.1, (fun x -> npdf x mean stddev), 5, 1.0, RandomSource = rnd )

    /// An array of samples from the rejection sampler.
    let arr = ms.Sample(N)

    /// The true distribution.
    printfn "\tEmpirical Mean = %f (should be %f)" (Statistics.Mean(arr)) normal.Mean
    printfn "\tEmpirical StdDev = %f (should be %f)" (Statistics.StandardDeviation(arr)) normal.StdDev
    printfn ""
