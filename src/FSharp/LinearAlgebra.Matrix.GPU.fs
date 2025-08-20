// <copyright file="LinearAlgebra.Matrix.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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

namespace MathNet.Numerics.LinearAlgebra.GPU

open System
open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra
open Brahma.FSharp
open FSharp.Quotations


/// A module which implements functional matrix operations.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Matrix =

    /// !!!
    let iter (action: Expr<'t -> unit>) (matrix: #Matrix<'t>) = 
        let device = ClDevice.GetFirstAppropriateDevice() 
        let context = RuntimeContext(device).ClContext

        let heigh = matrix.RowCount
        let width = matrix.ColumnCount
        let matrixClArray = context.CreateClArray(heigh * width, allocationMode = AllocationMode.Default)

        let kernel = 
            <@
                fun (range: Range2D) (matrix: ClArray<'t>) ->
                    let row = range.GlobalID0
                    let col = range.GlobalID1 
                    (%action) matrix.[row * width + col]
            @>

        let kernel = context.Compile kernel

        fun (commandQueue: MailboxProcessor<_>) ->
            let ndRange = Range2D(heigh, width, 16, 16)
            let kernel = kernel.GetKernel()
            
            commandQueue.Post(
                Msg.MsgSetArguments
                    (fun () -> kernel.KernelFunc ndRange matrixClArray)
            )
            commandQueue.Post(Msg.CreateRunMsg<_, _> kernel)


    /// !!!
    let map2 f (v: #Matrix<_>) (w: #Matrix<_>) = v

    /// !!!
    let mXm (v: #Matrix<_>) (w: #Matrix<_>) = v
