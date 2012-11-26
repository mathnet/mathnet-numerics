// <copyright file="RandomAndDistributions.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
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

module RandomAndDistributions

open MathNet.Numerics.Random
open MathNet.Numerics.Distributions

// generate some seeds for random values
let someGuidSeed = Random.seed ()
let someTimeSeed = Random.timeSeed ()

// generate some pseudo random number generators (listing incomplete; all of them are cast to the common base type, System.Random)
let a = Random.system ()
let b = Random.systemWith (Random.timeSeed())
let c = Random.crypto ()
let d = Random.mersenneTwister ()
let e = Random.mersenneTwisterWith 1000 true (* thread-safe *)
let f = Random.xorshift ()
let g = Random.xorshiftCustom someTimeSeed false 916905990L 13579L 362436069L 77465321L
let h = Random.wh2006 ()
let i = Random.palf ()

// generate some uniform random values
let values = (
        a.Next(),
        b.NextFullRangeInt32(),
        c.NextFullRangeInt64(),
        d.NextInt64(),
        e.NextDouble(),
        f.NextDecimal()
    )

// generate some probability distributions
let normal = Normal.WithMeanVariance(3.0, 1.5) |> withRandom g
let exponential = new Exponential(2.4)
let gamma = new Gamma(2.0, 1.5) |> withCryptoRandom
let cauchy = new Cauchy() |> withRandom (Random.mrg32k3aWith 10 false)
let poisson = new Poisson(3.0)
let geometric = new Geometric(1.2) |> withSystemRandom

// generate some random samples from these distributions
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

// direct sampling (without creating a configurable distribution object)
let u = Normal.Sample(Random.system(), 2.0, 4.0)
let v = Laplace.Samples(Random.mersenneTwister(), 1.0, 3.0) |> Seq.take 100 |> List.ofSeq
let w = Rayleigh.Sample(c, 1.5)
let x = Hypergeometric.Sample(h, 100, 20, 5)

// probability distribution functions of the normal dist we configured above
let nd = normal.Density(4.0) (* pdf *)
let ndLn = normal.DensityLn(4.0) (* ln(pdf) *)
let nc = normal.CumulativeDistribution(4.0) (* cdf *)
let nic = normal.InverseCumulativeDistribution(0.7) (* invcdf *)

// distribution properties of the gamma dist we configured above
let gammaStats = (
        gamma.Mean,
        gamma.Variance,
        gamma.StdDev,
        gamma.Entropy,
        gamma.Skewness,
        gamma.Mode
    )
