module Histogram

open MathNet.Numerics.Statistics

/// The number of buckets to use in our histogram.
let B = 4

/// Create a small dataset.
let data = [| 0.5; 1.5; 2.5; 3.5; 4.5; 5.5; 6.5; 7.5; 8.5; 9.5 |]

/// A histogram with 4 buckets for this dataset.
let hist = new Histogram(data, B)

// Print some histogram information.
printfn "Histogram.ToString(): %O" hist
for i in 0 .. B-1 do
    printfn "Bucket %d contains %f datapoints." i hist.[i].Count
