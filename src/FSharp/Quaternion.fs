module MathNet.Numerics.Quaternion

//Reference:
//http://www.astro.rug.nl/software/kapteyn/_downloads/attitude.pdf
//http://www.mathworks.com/help/aeroblks/quaternionmultiplication.html
//http://www.mathworks.com/help/aeroblks/quaterniondivision.html
//https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation#The_conjugation_operation

type Quaternion = 
    {
        w:float
        x:float
        y:float
        z:float
    } with
    static member (+) (r: Quaternion, q: Quaternion) =
        {w=r.w+q.w;x=r.x+q.x;y=r.y+q.y;z=r.z+q.z}
    static member (-) (r: Quaternion, q: Quaternion) =
        {w=r.w-q.w;x=r.x-q.x;y=r.y-q.y;z=r.z-q.z}
    static member (*) (r: Quaternion, q: Quaternion) =
        let w = r.w*q.w - r.x*q.x - r.y*q.y - r.z*q.z
        let x = r.w*q.x + r.x*q.w - r.y*q.z + r.z*q.y
        let y = r.w*q.y + r.x*q.z + r.y*q.w - r.z*q.x
        let z = r.w*q.z - r.x*q.y + r.y*q.x + r.z*q.w
        {w=w;x=x;y=y;z=z}
    static member (/) (r: Quaternion, q: Quaternion) =
        let d = (r.w**2.0 + r.x**2.0 + r.y**2.0 + r.z**2.0)

        let w = (r.w*q.w + r.x*q.x + r.y*q.y + r.z*q.z) / d
        let x = (r.w*q.x - r.x*q.w - r.y*q.z + r.z*q.y) / d
        let y = (r.w*q.y + r.x*q.z - r.y*q.w - r.z*q.x) / d
        let z = (r.w*q.z - r.x*q.y + r.y*q.x - r.z*q.w) / d
        {w=w;x=x;y=y;z=z}
    static member (/) (q:Quaternion, a) =
        {w=q.w/a; x=q.x/a;y=q.y/a;z=q.z/a}

let norm q = 
    q.w**2.0 + q.x**2.0 + q.y**2.0 + q.z**2.0

let normalize q =
    let invNorm = 1.0 / (norm q |> sqrt)
    {w=q.w*invNorm;x=q.x*invNorm;y=q.y*invNorm;z=q.z*invNorm}

let conjugate q = 
    {w=q.w; x= -q.x; y= -q.y; z= -q.z}

let inverse q = 
    conjugate q / norm q

//create a new quaternion
//angle in radians
//http://www.astro.rug.nl/software/kapteyn/_downloads/attitude.pdf
//6.12 Unit Quaternion ⇐ Axis-Angle
//              _           _
// qa (α, n) := | cos α/2   |
//              | n sin α/2 |
//              -           -
let create (angle:float) (x:float) (y:float) (z:float) =
        //axis needs to be unit vector
        let vNorm = x**2.0+y**2.0+z**2.0
        let invNorm = 1.0 / (vNorm |> sqrt)
        let x' = x*invNorm
        let y' = y*invNorm
        let z' = z*invNorm

        let halfAngle = angle * 0.5
        let s = halfAngle |> sin
        let c = halfAngle |> cos
        {w=c; x=x'*s; y=y'*s; z=z'*s}

//dot product
let dot q1 q2 =
    q1.w*q2.w + q1.x * q2.x + q1.y * q2.y + q1.z * q2.z

//rotate a vector(x,y,z) by a quaternion
// p is a pure quaternion(i.e w=0.0)
// p' = qpq**-1.0
// https://en.wikipedia.org/wiki/Quaternions_and_spatial_rotation#The_conjugation_operation
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
let concat (q:Quaternion) (q':Quaternion) = q' * q //concat rotation is actually q' * q instead of q * q'.