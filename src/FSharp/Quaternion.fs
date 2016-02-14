// <copyright file="Quaternion.fs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2016 Math.NET
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

namespace MathNet.Numerics

type Quaternion =
    {
        w:float
        x:float
        y:float
        z:float
    } with

    static member (+) (r: Quaternion, q: Quaternion) =
        { w=r.w+q.w; x=r.x+q.x; y=r.y+q.y; z=r.z+q.z }

    static member (-) (r: Quaternion, q: Quaternion) =
        { w=r.w-q.w; x=r.x-q.x; y=r.y-q.y; z=r.z-q.z }

    static member (*) (r: Quaternion, q: Quaternion) =
        let w = r.w*q.w - r.x*q.x - r.y*q.y - r.z*q.z
        let x = r.w*q.x + r.x*q.w - r.y*q.z + r.z*q.y
        let y = r.w*q.y + r.x*q.z + r.y*q.w - r.z*q.x
        let z = r.w*q.z - r.x*q.y + r.y*q.x + r.z*q.w
        { w=w; x=x; y=y; z=z }

    static member (/) (r: Quaternion, q: Quaternion) =
        let d = (r.w**2.0 + r.x**2.0 + r.y**2.0 + r.z**2.0)
        let w = (r.w*q.w + r.x*q.x + r.y*q.y + r.z*q.z) / d
        let x = (r.w*q.x - r.x*q.w - r.y*q.z + r.z*q.y) / d
        let y = (r.w*q.y + r.x*q.z - r.y*q.w - r.z*q.x) / d
        let z = (r.w*q.z - r.x*q.y + r.y*q.x - r.z*q.w) / d
        { w=w; x=x; y=y; z=z }

    static member (/) (q:Quaternion, a) =
        { w=q.w/a; x=q.x/a; y=q.y/a; z=q.z/a }


[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Quaternion =

    //Reference:
    //http://www.astro.rug.nl/software/kapteyn/_downloads/attitude.pdf
    //http://www.mathworks.com/help/aeroblks/quaternionmultiplication.html
    //http://www.mathworks.com/help/aeroblks/quaterniondivision.html
    //https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation#The_conjugation_operation

    let normSquared q =
        q.w*q.w + q.x*q.x + q.y*q.y + q.z*q.z

    /// Euclidean Norm
    let norm q =
        normSquared q |> sqrt

    /// Normalize a quaternion to unit quaternion with norm 1.
    let normalize q =
        let invNorm = 1.0 / (norm q)
        { w=q.w*invNorm; x=q.x*invNorm; y=q.y*invNorm; z=q.z*invNorm }

    let conjugate q =
        { w=q.w; x= -q.x; y= -q.y; z= -q.z }

    let inverse q =
        conjugate q / normSquared q

    /// Dot product
    let dot q1 q2 =
        q1.w*q2.w + q1.x*q2.x + q1.y*q2.y + q1.z*q2.z

    //create a new quaternion
    //angle in radians
    //http://www.astro.rug.nl/software/kapteyn/_downloads/attitude.pdf
    //6.12 Unit Quaternion ⇐ Axis-Angle
    //              _           _
    // qa (α, n) := | cos α/2   |
    //              | n sin α/2 |
    //
    [<System.Obsolete("Semantic Version Opt-Out: this routine has not been finalized yet and may change in breaking ways within minor versions.")>]
    let create (angle:float) (x:float) (y:float) (z:float) =
        //axis needs to be unit vector
        let vNorm = x*x + y*y + z*z
        let invNorm = 1.0 / (sqrt vNorm)
        let x' = x*invNorm
        let y' = y*invNorm
        let z' = z*invNorm

        let halfAngle = angle * 0.5
        let s = sin halfAngle
        let c = cos halfAngle
        { w=c; x=x'*s; y=y'*s; z=z'*s }

    /// rotate a vector(x,y,z) by a quaternion
    /// p is a pure quaternion(i.e w=0.0)
    /// p' = qpq**-1.0
    /// https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation#The_conjugation_operation
    [<System.Obsolete("Semantic Version Opt-Out: this routine has not been finalized yet and may change in breaking ways within minor versions.")>]
    let rotate q1 x y z =
        let q = normalize q1 //ensure unit quaternion
        let p = {w=0.0; x=x; y=y; z=z}
        q * p * inverse q

    /// <summary>
    /// Concatenates two Quaternions; the result represents the value1 rotation followed by the value2 rotation.
    /// </summary>
    /// <param name="value1">The first Quaternion rotation in the series.</param>
    /// <param name="value2">The second Quaternion rotation in the series.</param>
    /// <returns>A new Quaternion representing the concatenation of the value1 rotation followed by the value2 rotation.</returns>
    [<System.Obsolete("Semantic Version Opt-Out: this routine has not been finalized yet and may change in breaking ways within minor versions.")>]
    let concat (q:Quaternion) (q':Quaternion) =
         //concat rotation is q' * q instead of q * q'.
        q' * q
