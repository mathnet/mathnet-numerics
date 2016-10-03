namespace MathNet.Numerics.Tests

open NUnit.Framework
open FsUnitTyped

open System
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
            shouldEqual {w = 0.92387953251128674; x = 0.38268343236508978; y = 0.0; z = 0.0;}

    [<Test>]
    let ``Quaternion.create normalizes input vector`` () =
        let fourtyFiveDegreesInRadians = 45.0 * Math.PI / 180.0
        Quaternion.create fourtyFiveDegreesInRadians 100.0 0.0 0.0 |>
            shouldEqual <|
                Quaternion.create fourtyFiveDegreesInRadians 1.0 0.0 0.0

    [<Test>]
    let ``Quaternion.+`` () =
        n + n' |> shouldEqual {w= 1.5;x= -0.5;y= 1.5;z= 0.0}

    [<Test>]
    let ``Quaternion.-`` () =
        n - n' |> shouldEqual {w= 0.5;x= 0.5;y= 0.5;z= 0.0}

    [<Test>]
    let ``Quaternion.* r*q'`` () =
        r * q |> shouldEqual  {w = -8.0; x = -16.0; y = -24.0; z = -2.0;}

    [<Test>]
    let ``Quaternion.* q*r`` () =
        q * r |> shouldEqual {w = -8.0; x = 6.0; y = -4.0; z = -28.0;}

    //http://www.mathworks.com/help/aerotbx/ug/quatnorm.html
    [<Test>]
    let ``Quaternion.NormSquared`` () =
        Quaternion.normSquared n' |> shouldEqual 0.75

    //http://www.mathworks.com/help/aerotbx/ug/quatnormalize.html
    [<Test>]
    let ``Quaternion.Normalize`` () =
        Quaternion.normalize n |> shouldEqual {w= 0.70710678118654746; x= 0.0; y= 0.70710678118654746; z= 0.0}

    //http://www.mathworks.com/help/aerotbx/ug/quatinv.html
    [<Test>]
    let ``Quaternion.Inverse`` () =
        Quaternion.inverse n |> shouldEqual {w= 0.5;x= 0.0;y= -0.5;z= 0.0}

    //http://www.mathworks.com/help/aerotbx/ug/quatconj.html
    [<Test>]
    let ``Quaternion.Conjugate`` () =
        Quaternion.conjugate n |> shouldEqual {w= 1.0;x= 0.0;y= -1.0;z= 0.0}

    //http://www.mathworks.com/help/aerotbx/ug/quatrotate.html
    [<Test>]
    let ``Quaternion.Rotate`` () =
        Quaternion.rotate n 1.0 1.0 1.0 |> shouldEqual {w= 0.0;x= -1.0;y= 1.0;z= 1.0}
