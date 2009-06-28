// (c) Microsoft Corporation 2005-2009. 

namespace Microsoft.FSharp.Math

module GlobalAssociations =

    open Microsoft.FSharp.Math
    open Microsoft.FSharp.Math.Instances
    open System

    let ComplexNumerics = 
      { new IFractional<_> with 
          member ops.Zero = Complex.Zero
          member ops.One = Complex.One
          member ops.Add(a,b) = a + b
          member ops.Subtract(a,b) = a - b
          member ops.Multiply(a,b) = a * b
          member ops.Divide(a,b) = a / b
          member ops.Negate(a) = -a
          member ops.Abs(a)  = a // not signed
          member ops.Sign(a) = 1 // not signed
          member ops.Reciprocal(a) =  Complex.One / a 
          member ops.ToString((x:complex),fmt,fmtprovider) = x.ToString(fmt,fmtprovider)
          member ops.Parse(s,numstyle,fmtprovider) = Complex.mkRect (System.Double.Parse(s,numstyle,fmtprovider),0.0) }

    let ht = 
        let ht = new System.Collections.Generic.Dictionary<Type,obj>() 
        let optab =
            [ typeof<float>,   (Some(FloatNumerics    :> INumeric<float>) :> obj);
              typeof<int32>,   (Some(Int32Numerics    :> INumeric<int32>) :> obj);
              typeof<int64>,   (Some(Int64Numerics    :> INumeric<int64>) :> obj);
              typeof<bigint>,  (Some(BigIntNumerics   :> INumeric<bigint>) :> obj);
              typeof<float32>, (Some(Float32Numerics  :> INumeric<float32>) :> obj);
              typeof<Complex>, (Some(ComplexNumerics :> INumeric<Complex>) :> obj);
              typeof<bignum>,  (Some(BigNumNumerics   :> INumeric<bignum>) :> obj); ]
           
        List.iter (fun (ty,ops) -> ht.Add(ty,ops)) optab;
        ht
        
    let Put (ty: System.Type, d : obj)  =
        lock ht (fun () -> 
            if ht.ContainsKey(ty) then invalidArg "ty" ("the type "+ty.Name+" already has a registered numeric association");
            ht.Add(ty, d))
      
    let TryGetNumericAssociation<'a>() = 
        lock ht (fun () -> 
            let ty = typeof<'a>  
            if ht.ContainsKey(ty) then
                match ht.[ty] with
                | :? (INumeric<'a> option) as r -> r
                | _ -> invalidArg "ty" ("The type "+ty.Name+" has a numeric association but it was not of the correct type")
            else
                None)

    let GetNumericAssociation() = (TryGetNumericAssociation()).Value
    let RegisterNumericAssociation (d : INumeric<'a>)  = Put(typeof<'a>, box(Some d))


