// (c) Microsoft Corporation 2005-2009. 


namespace Microsoft.FSharp.Math

#nowarn "42"

module Measure =

    let infinity<[<Measure>] 'u> : float<'u> = (# "" System.Double.PositiveInfinity : float<'u> #)
    let nan<[<Measure>] 'u> : float<'u> = (# "" System.Double.NaN : float<'u> #)

    let infinityf<[<Measure>] 'u> : float32<'u> = (# "" System.Double.NegativeInfinity : float32<'u> #)
    let nanf<[<Measure>] 'u> : float32<'u> = (# "" System.Single.NaN : float32<'u> #)

