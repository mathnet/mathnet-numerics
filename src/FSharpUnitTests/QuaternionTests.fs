namespace MathNet.Numerics.Tests

open System
open NUnit.Framework
open FsUnit
open MathNet.Numerics

module QuaternionTests =

    //simple quaternions
    let n = {w=1.0;x=0.0;y=1.0;z=0.0}
    let n' = {w= 0.5;x= -0.5;y= 0.5;z= 0.0}
    //less simple quaternions
    let r = {w=1.0;x=2.0;y=3.0;z=4.0}
    let q = {w= -4.0;x= 3.0; y= -2.0; z= 1.0}

    [<Test>]
    let ``Quaternion.create`` () =
        let fourtyFiveDegreesInRadians = 45.0 * Math.PI / 180.0
        Quaternion.create fourtyFiveDegreesInRadians 1.0 0.0 0.0 |>
            should equal {w = 0.92387953251128674; x = 0.38268343236508978; y = 0.0; z = 0.0;}

    [<Test>]
    let ``Quaternion.create normalizes input vector`` () =
        let fourtyFiveDegreesInRadians = 45.0 * Math.PI / 180.0
        Quaternion.create fourtyFiveDegreesInRadians 100.0 0.0 0.0 |>
            should equal <|
                Quaternion.create fourtyFiveDegreesInRadians 1.0 0.0 0.0

    [<Test>]
    let ``Quaternion.+`` () =
        n + n' |> should equal {w= 1.5;x= -0.5;y= 1.5;z= 0.0}

    [<Test>]
    let ``Quaternion.-`` () =
        n - n' |> should equal {w= 0.5;x= 0.5;y= 0.5;z= 0.0}

    [<Test>]
    let ``Quaternion.* r*q'`` () =
        r * q |> should equal  {w = -8.0; x = -16.0; y = -24.0; z = -2.0;}

    [<Test>]
    let ``Quaternion.* q*r`` () =
        q * r |> should equal {w = -8.0; x = 6.0; y = -4.0; z = -28.0;}

    //http://www.mathworks.com/help/aerotbx/ug/quatnorm.html
    [<Test>]
    let ``Quaternion.Norm`` () =
        Quaternion.norm n' |> should equal 0.75

    //http://www.mathworks.com/help/aerotbx/ug/quatnormalize.html
    [<Test>]
    let ``Quaternion.Normalize`` () =
        Quaternion.normalize n |> should equal {w= 0.70710678118654746; x= 0.0; y= 0.70710678118654746; z= 0.0}

    //http://www.mathworks.com/help/aerotbx/ug/quatinv.html
    [<Test>]
    let ``Quaternion.Inverse`` () =
        Quaternion.inverse n |> should equal {w= 0.5;x= 0.0;y= -0.5;z= 0.0}

    //http://www.mathworks.com/help/aerotbx/ug/quatconj.html
    [<Test>]
    let ``Quaternion.Conjugate`` () =
        Quaternion.conjugate n |> should equal {w= 1.0;x= 0.0;y= -1.0;z= 0.0}

    //http://www.mathworks.com/help/aerotbx/ug/quatrotate.html
    [<Test>]
    let ``Quaternion.Rotate`` () =
        Quaternion.rotate n 1.0 1.0 1.0 |> should equal {w= 0.0;x= -1.0;y= 1.0;z= 1.0}
