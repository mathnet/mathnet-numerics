// <copyright file="Extensions.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra.Generic

// Module that contains implementation of useful F#-specific
// extension members for generic Matrix and Vector types
[<AutoOpen>]
module FSharpExtensions = 

  // A type extension for the generic vector type that 
  // adds the 'GetSlice' method to allow vec.[a .. b] syntax
  type MathNet.Numerics.LinearAlgebra.Generic.
      Vector<'T when 'T : struct and 'T : (new : unit -> 'T) 
                 and 'T :> System.IEquatable<'T> and 'T :> System.IFormattable 
                 and 'T :> System.ValueType> with

    /// Gets a slice of a vector starting at a specified index
    /// and ending at a specified index (both indices are optional)
    /// This method can be used via the x.[start .. finish] syntax
    member x.GetSlice(start, finish) = 
      let start = defaultArg start 0
      let finish = defaultArg finish (x.Count - 1)
      x.SubVector(start, finish - start + 1)

  // A type extension for the generic matrix type that
  // adds the 'GetSlice' method to allow m.[r1 .. r2, c1 .. c2] syntax
  type MathNet.Numerics.LinearAlgebra.Generic.
      Matrix<'T when 'T : struct and 'T : (new : unit -> 'T) 
                 and 'T :> System.IEquatable<'T> and 'T :> System.IFormattable 
                 and 'T :> System.ValueType> with

    /// Gets a submatrix using a specified column range and 
    /// row range (all indices are optional)
    /// This method can be used via the x.[r1 .. r2, c1 .. c2 ] syntax
    member x.GetSlice(rstart, rfinish, cstart, cfinish) = 
      let cstart = defaultArg cstart 0
      let rstart = defaultArg rstart 0
      let cfinish = defaultArg cfinish (x.ColumnCount - 1)
      let rfinish = defaultArg rfinish (x.RowCount - 1)
      x.SubMatrix(rstart, rfinish - rstart + 1, cstart, cfinish - cstart + 1)