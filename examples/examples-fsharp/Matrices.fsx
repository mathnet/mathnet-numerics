// <copyright file="Matrices.fsx" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2013 Math.NET
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

#r "../../out/lib/Net40/MathNet.Numerics.dll"
#r "../../out/lib/Net40/MathNet.Numerics.FSharp.dll"

open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.Distributions

// Create a dense matrix directly from an array (by reference, without copy)
// The array must be in column major order (column-by-column)
let a1 = DenseMatrix(2, 3, [| 1.0; 2.0; 10.0; 20.0; 100.0; 300.0 |])
let a2 = DenseMatrix.raw 2 3 [| 1.0; 2.0; 10.0; 20.0; 100.0; 300.0 |]

// Create a matrix of size 3x4 (3 rows, 4 columns) with a given number for each value
let b1 : float Matrix = DenseMatrix.zero 3 4
let b2 = DenseMatrix.create 3 4 20.5
let b3 : float Matrix = SparseMatrix.zero 3 4

// Create a matrix of size 3x4 with random values sampled from a distribution
let c : float Matrix = DenseMatrix.random 3 4 (Normal.WithMeanStdDev(2.0, 0.5))

// Create a matrix of size 3x4 with each value initialized by a lambda function
let d1 = DenseMatrix.init 3 4 (fun i j -> float i / 100.0 + float j)
let d2 = SparseMatrix.init 3 4 (fun i j -> if i=j then float i / 100.0 + float j else 0.0)

// Matrices can also be constructed from sequences of rows or of columns
let e1 = DenseMatrix.ofRowSeq (seq { for i in 1 .. 20 do yield Array.init 10 (fun j -> float j + 100.0 * float i) })
let e2 = SparseMatrix.ofRowSeq (seq { for i in 1 .. 20 do yield Array.init 10 (fun j -> if i%5 = 0 then float j + 100.0 * float i else 0.0) })
let e3 = DenseMatrix.ofColumnSeq (seq { for j in 1 .. 10 do yield Array.init 20 (fun i -> float j + 100.0 * float i) })
let e4 = SparseMatrix.ofColumnSeq (seq { for j in 1 .. 10 do yield Array.init 20 (fun i -> if i%5 = 0 then float j + 100.0 * float i else 0.0) })

// Or from F# lists
let f1 = DenseMatrix.ofRowList [ for i in 1 .. 20 -> List.init 10 (fun j -> float j + 100.0 * float i) ]
let f2 = SparseMatrix.ofRowList [ for i in 1 .. 20 -> List.init 10 (fun j -> if i%5 = 0 then float j + 100.0 * float i else 0.0) ]
let f3 = DenseMatrix.ofColumnList [ for j in 1 .. 10 -> List.init 20 (fun i -> float j + 100.0 * float i) ]
let f4 = SparseMatrix.ofColumnList [ for j in 1 .. 10 -> List.init 20 (fun i -> if i%5 = 0 then float j + 100.0 * float i else 0.0) ]

// Or from row or column vectors
let m1 = DenseMatrix.ofRows [DenseVector.create 3 1.0; DenseVector.create 3 2.0]
let m2 = DenseMatrix.ofColumns [DenseVector.create 3 1.0; DenseVector.create 3 2.0]

// Or from row or column arrays
let n1 = DenseMatrix.ofRowArrays [| Array.create 3 1.0; Array.create 3 2.0 |]
let n2 = DenseMatrix.ofColumnArrays [| Array.create 3 1.0; Array.create 3 2.0 |]

// Or from indexed lists or sequences where all other values are zero (useful mostly for sparse data)
let g1 = DenseMatrix.ofListi 20 10 [(4,3,20.0); (18,9,3.0); (2,1,100.0)]
let g2 = SparseMatrix.ofListi 20 10 [(4,3,20.0); (18,9,3.0); (2,1,100.0)]
let g3 = SparseMatrix.ofSeqi 20 10 (Seq.ofList [(4,3,20.0); (18,9,3.0); (2,1,100.0)])

// Another way to create dense matrix is using the matrix function.
let h = matrix [[1.0;  2.0;  3.0]
                [10.0; 11.0; 12.0]]

// Or create it from a multi-dimensional array
let k = DenseMatrix.ofArray2 (array2D [[1.0;  2.0;  3.0]; [10.0; 11.0; 12.0]])

// We can now add two matrices together ...
let z = a1 + a2

// ... or scale them in the process.
let x = e1 + 3.0 * e4 - g2

// "pretty" printing (configurable with the Control class):
printfn "A: %A" e3
printfn "B: %s" (e3.ToString())
printfn "C: %s" (e3.ToTypeString())
printfn "D:\n%s" (e3.ToMatrixString(20, 20))
printfn "E:\n%s" (e3.ToMatrixString(4, 10))
