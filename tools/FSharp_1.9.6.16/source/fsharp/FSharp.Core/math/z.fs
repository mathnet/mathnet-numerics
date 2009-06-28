// (c) Microsoft Corporation. All rights reserved 

#nowarn "44" // This construct is deprecated. This function is for use by compiled F# code and should not be used directly

namespace Microsoft.FSharp.Math

#if FX_ATLEAST_40
    type BigInt = System.Numerics.BigInteger
    type bigint = System.Numerics.BigInteger
#else
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Primitives.Basics
    open Microsoft.FSharp.Math
    open System
    open System.Globalization


    // INVARIANT: signInt = 1 or -1
    //            value(z) = signInt * v
    // NOTE: 0 has two repns (+1,0) or (-1,0).
    [<Struct>]
    [<StructuralEquality(false); StructuralComparison(false)>]
    [<StructuredFormatDisplay("{StructuredDisplayString}I")>]
    type BigInt(signInt:int, v : BigNat) =

        //new() = new BigInt(0,0)

        static let smallLim =  4096
        static let smallPosTab = Array.init smallLim BigNatModule.of_int
        static let one = BigInt(1)
        static let zero = BigInt(0)

        static member internal nat n = 
            if BigNatModule.is_small n && BigNatModule.get_small n < smallLim 
            then smallPosTab.[BigNatModule.get_small n] 
            else n
        static member internal create (s,n) = BigInt(s,BigInt.nat n)
        static member internal posn n = BigInt(1,BigInt.nat n)
        static member internal negn n = BigInt(-1,BigInt.nat n)


        member x.Sign = if x.IsZero then 0 else signInt
        member x.SignInt = signInt
        member internal x.V = v

        static member op_Equality (x:BigInt, y:BigInt) =
            //System.Console.WriteLine("x = {0}",box x)
            //System.Console.WriteLine("y = {0}",box y)
            match x.SignInt,y.SignInt with
            |  1, 1 -> BigNatModule.equal x.V y.V                    // +1.xv = +1.yv iff xv = yv 
            | -1, -1 -> BigNatModule.equal x.V y.V                     // -1.xv = -1.yv iff xv = yv 
            |  1,-1 -> BigNatModule.isZero x.V && BigNatModule.isZero y.V       //  1.xv = -1.yv iff xv=0 and yv=0 
            | -1, 1 -> BigNatModule.isZero x.V && BigNatModule.isZero y.V       // -1.xv =  1.yv iff xv=0 and yv=0 
            | _ -> invalidArg "x" "signs should be +/- 1"

        static member op_Inequality (x:BigInt, y:BigInt) = not (BigInt.op_Equality(x,y)) // CA2226: OperatorsShouldHaveSymmetricalOverloads
                
        static member op_LessThan (x:BigInt, y:BigInt) =
            match x.SignInt,y.SignInt with
            |  1, 1 -> BigNatModule.lt x.V y.V                       //  1.xv <  1.yv iff xv < yv 
            | -1,-1 -> BigNatModule.lt y.V x.V                       // -1.xv < -1.yv iff yv < xv 
            |  1,-1 -> false                              //  1.xv < -1.yv iff 0 <= 1.xv < -1.yv <= 0 iff false 
            | -1, 1 -> not (BigNatModule.isZero x.V) || not (BigNatModule.isZero y.V)
                                                          // -1.xv <  1.yv
                                   // (a) xv=0 and yv=0,  then false
                                   // (b) xv<>0,          -1.xv <  0 <= 1.yv, so true
                                   // (c) yv<>0,          -1.xv <= 0 <  1.yv, so true
            | _ -> invalidArg "x" "signs should be +/- 1"
                
        static member op_GreaterThan (x:BigInt, y:BigInt) = // Follow lt by +/- symmetry 
            match x.SignInt,y.SignInt with
            | 1, 1 -> BigNatModule.gt x.V y.V 
            | -1,-1 -> BigNatModule.gt y.V x.V
            |  1,-1 -> not (BigNatModule.isZero x.V) || not (BigNatModule.isZero y.V)
            | -1, 1 -> false
            | _ -> invalidArg "x" "signs should be +/- 1"

        static member internal compare(n,nn) = if BigInt.op_LessThan(n,nn) then -1 elif BigInt.op_Equality(n,nn) then 0 else 1
        static member internal hash (z:BigInt) = z.SignInt + BigNatModule.hash(z.V)

        override x.ToString() =
            match x.SignInt with
            |  1 -> BigNatModule.to_string x.V                       // positive 
            | -1 -> 
                if BigNatModule.isZero x.V             
                then "0"                    // not negative infact, but zero. 
                else "-" + BigNatModule.to_string x.V  // negative 
            | _ -> invalidArg "x" "signs should be +/- 1"
               
        member x.StructuredDisplayString = x.ToString()

        interface System.IComparable with 
            member this.CompareTo(obj:obj) = 
                match obj with 
                | :? BigInt as that -> BigInt.compare(this,that)
                | _ -> invalidArg "obj" "the objects are not comparable"

        override this.Equals(obj) = 
            match obj with 
            | :? BigInt as that -> BigInt.op_Equality(this, that)
            | _ -> false
  
        override x.GetHashCode() = BigInt.hash(x)


        [<OverloadID("new_int")>]
        new (n:int) = 
            if n>=0 
            then BigInt (1,BigInt.nat(BigNatModule.of_int   n))
            elif (n = System.Int32.MinValue) 
            then BigInt(-1,BigInt.nat(BigNatModule.of_int64 (-(int64 n))))
            else BigInt(-1,BigInt.nat(BigNatModule.of_int (-n)))

        static member ToInt32(z:BigInt) =
            let u = BigNatModule.to_uint32 z.V
            if u <= uint32 System.Int32.MaxValue then
                (* Handle range [-MaxValue,MaxValue] *)
                z.SignInt * int32 u     
            elif z.SignInt = -1 &&       u = uint32 (System.Int32.MaxValue + 1) then
                //assert(System.Int32.MinValue = 0 - System.Int32.MaxValue - 1)       
                (* Handle MinValue = -(MaxValue+1) special case not covered by the above *)
                System.Int32.MinValue
            else
                raise (System.OverflowException())

        [<OverloadID("new_int64")>]
        new (n:int64) = 
            if n>=0L 
            then BigInt(1,BigInt.nat (BigNatModule.of_int64   n))
            elif (n = System.Int64.MinValue) 
            then BigInt(-1,BigInt.nat (BigNatModule.add (BigNatModule.of_int64 System.Int64.MaxValue) BigNatModule.one) )
            else BigInt(-1,BigInt.nat (BigNatModule.of_int64 (-n)))

        static member ToInt64(z:BigInt) = 
            let u = BigNatModule.to_uint64 z.V
            if u <= uint64 System.Int64.MaxValue then
                (* Handle range [-MaxValue,MaxValue] *)
                int64 z.SignInt * int64 u
            elif z.SignInt = -1 &&       u = uint64 (System.Int64.MaxValue + 1L) then    
                //assert(System.Int64.MinValue = 0 - System.Int64.MaxValue - 1L)      
                (* Handle MinValue = -(MaxValue+1) special case not covered by the above *)
                System.Int64.MinValue
            else
                raise (System.OverflowException())    

        static member One = one
        static member Zero = zero
        static member (~-) (z:BigInt)  = BigInt.create(-1 * z.SignInt,z.V)
        static member Scale(k,z:BigInt) =
            if k<0
            then BigInt.create(-z.SignInt, (BigNatModule.scale (-k) z.V))  // k.zsign.zv = -zsign.(-k.zv) 
            else BigInt.create(z.SignInt, (BigNatModule.scale k z.V))     // k.zsign.zv =  zsign.k.zv 

        // Result: 1.nx - 1.ny  (integer subtraction) 
        static member internal subnn (nx,ny) =                         
            if BigNatModule.gte nx ny 
            then BigInt.posn (BigNatModule.sub nx ny)          // nx >= ny, result +ve,  +1.(nx - ny) 
            else BigInt.negn (BigNatModule.sub ny nx)          // nx < ny,  result -ve,  -1.(ny - nx) 

        static member internal addnn (nx,ny) = 
            BigInt.posn (BigNatModule.add nx ny)              // Compute "nx + ny" to be integer 
            
        member x.IsZero = BigNatModule.isZero x.V                   // signx.xv = 0 iff xv=0, since signx is +1,-1 
        member x.IsOne = (x.SignInt = 1) && BigNatModule.isOne x.V       // signx.xv = 1 iff signx = +1 and xv = 1 
        static member (+) (x:BigInt,y:BigInt) =
            if y.IsZero then x else
            if x.IsZero then y else
            match x.SignInt,y.SignInt with
            |  1, 1 -> BigInt.addnn(x.V,y.V)                //  1.xv +  1.yv =  (xv + yv) 
            | -1,-1 -> -(BigInt.addnn(x.V,y.V))          // -1.xv + -1.yv = -(xv + yv) 
            |  1,-1 -> BigInt.subnn (x.V,y.V)                //  1.xv + -1.yv =  (xv - yv) 
            | -1, 1 -> BigInt.subnn(y.V,x.V)                // -1.xv +  1.yv =  (yv - xv) 
            | _ -> invalidArg "x" "signs should be +/- 1"
                
        static member (-) (x:BigInt,y:BigInt) =
            if y.IsZero then x else
            match x.SignInt,y.SignInt with
            |  1, 1 -> BigInt.subnn(x.V,y.V)                //  1.xv -  1.yv =  (xv - yv) 
            | -1,-1 -> BigInt.subnn(y.V,x.V)                // -1.xv - -1.yv =  (yv - xv) 
            |  1,-1 -> BigInt.addnn(x.V,y.V)                //  1.xv - -1.yv =  (xv + yv) 
            | -1, 1 -> -(BigInt.addnn(x.V,y.V))          // -1.xv -  1.yv = -(xv + yv) 
            | _ -> invalidArg "x" "signs should be +/- 1"
                
        static member ( * ) (x:BigInt,y:BigInt) =
            if x.IsZero then x
            elif y.IsZero then y
            elif x.IsOne then y
            elif y.IsOne then x
            else 
                let m = (BigNatModule.mul x.V y.V)
//                sample "smallMulResult" (BigNatModule.is_small m) smallMulResult
                BigInt.create (x.SignInt * y.SignInt,m)  // xsign.xv * ysign.yv = (xsign.ysign).(xv.yv) 
            
        static member DivRem (x:BigInt,y:BigInt) =
            let d,r = BigNatModule.divmod x.V y.V
            // HAVE: |x| = d.|y| + r and 0 <= r < |y| 
            // HAVE: xv  = d.yv  + r and 0 <= r < yv  
            match x.SignInt,y.SignInt with
            |  1, 1 -> BigInt.posn d,BigInt.posn r                //  1.xv =  1.d.( 1.yv) + ( 1.r) 
            | -1,-1 -> BigInt.posn d,BigInt.negn r                // -1.xv =  1.d.(-1.yv) + (-1.r) 
            |  1,-1 -> BigInt.negn d,BigInt.posn r                //  1.xv = -1.d.(-1.yv) + ( 1.r) 
            | -1, 1 -> BigInt.negn d,BigInt.negn r                // -1.xv = -1.d.( 1.yv) + (-1.r) 
            | _ -> invalidArg "x" "signs should be +/- 1"
                
        static member (/) (x:BigInt,y:BigInt) = fst (BigInt.DivRem(x,y))
        static member (%) (x:BigInt,y:BigInt) = snd (BigInt.DivRem(x,y))
        static member Gcd (x:BigInt,y:BigInt) = BigInt.posn (BigNatModule.hcf x.V y.V) // hcf (xsign.xv,ysign.yv) = hcf (xv,yv) 
            
(*
        let min (x:BigInt) (y:BigInt) =
            match x.SignInt,y.SignInt with
            |  1, 1 -> if BigNatModule.lte(x.V,y.V) then x else y   // if xv <= yv then  1.xv <=  1.yv 
            | -1,-1 -> if BigNatModule.lte(x.V,y.V) then y else x   // if xv <= yv then -1.yv <= -1.xv 
            |  1,-1 -> y                                // -1.yv <= 1.xv 
            | -1, 1 -> x                                // -1.xv <= 1.yv 
            | _ -> invalidArg "x" "signs should be +/- 1"
                
        let max (x:BigInt) (y:BigInt) =
            match x.SignInt,y.SignInt with
            |  1, 1 -> if BigNatModule.gte(x.V,y.V) then x else y   // if xv >= yv then  1.xv >=  1.yv 
            | -1,-1 -> if BigNatModule.gte(x.V,y.V) then y else x   // if xv >= yv then -1.yv >= -1.xv 
            |  1,-1 -> x                                //  1.xv >= -1.yv 
            | -1, 1 -> y                                //  1.yv >= -1.xv 
            | _ -> invalidArg "x" "signs should be +/- 1"
*)                
        member x.IsNegative = x.SignInt = -1 && not (x.IsZero)  // signx.xv < 0 iff signx = -1 and xv<>0 
        member x.IsPositive = x.SignInt =  1 && not (x.IsZero)  // signx.xv > 0 iff signx = +1 and xv<>0 
        static member Abs (x:BigInt)  = if x.SignInt = -1 then -x else x

        static member op_LessThanOrEqual (x:BigInt,y:BigInt) =
            match x.SignInt,y.SignInt with
            |  1, 1 -> BigNatModule.lte x.V y.V                      //  1.xv <=  1.yv iff xv <= yv 
            | -1,-1 -> BigNatModule.lte y.V x.V                      // -1.xv <= -1.yv iff yv <= xv 
            |  1,-1 -> BigNatModule.isZero x.V && BigNatModule.isZero y.V       //  1.xv <= -1.yv,
                                                          // (a) if xv=0 and yv=0 then true
                                                          // (b) otherwise false, only meet at zero.
                                                           
            | -1, 1 -> true                               // -1.xv <= 1.yv, true 
            | _ -> invalidArg "x" "signs should be +/- 1"
                
        static member op_GreaterThanOrEqual (x:BigInt,y:BigInt) = // Follow lte by +/- symmetry 
            match x.SignInt,y.SignInt with
            |  1, 1 -> BigNatModule.gte x.V y.V
            | -1,-1 -> BigNatModule.gte y.V x.V
            |  1,-1 -> true
            | -1, 1 -> BigNatModule.isZero x.V && BigNatModule.isZero y.V
            | _ -> invalidArg "x" "signs should be +/- 1"
                
        static member internal powi (z:BigInt,i) =
            if i>=0 then
             // (signz.zv)^i = (signz^i)        .(zv^i)
             //              = (signz^(i mod 2)).(zv^i) since signInt is 1,-1
             //              = either     1.zv^i        when i mod 2 = 0 (even power kills signInt)
             //                    or signz.zv^i        when i mod 2 = 1 (odd  power keeps signInt)
                 
                BigInt.create ((if i % 2 = 0 then 1 else z.SignInt), BigNatModule.powi z.V i)
            else
                invalidArg "i" "unexpected negative exponent"
              
        static member Pow (z:BigInt,i:BigInt) =
            if not i.IsNegative then
                BigInt.create ((if BigNatModule.isZero (BigNatModule.rem i.V BigNatModule.two) then 1 else z.SignInt), BigNatModule.pow z.V i.V)
            else
                invalidArg "i" "unexpected negative exponent"
              
        static member ToDouble(x:BigInt) =
            match x.SignInt with
            |  1 ->    BigNatModule.to_float x.V                     // float (1.xv)  =   float (xv) 
            | -1 -> - (BigNatModule.to_float x.V)                    // float (-1.xv) = - float (xv) 
            | _ -> invalidArg "x" "signs should be +/- 1"
                
        static member Parse(str:string) =
            let len = str.Length 
            if len = 0 then invalidArg "str" "empty string";
            if str.[0..0] = "-" then
                BigInt.negn (BigNatModule.of_string str.[1..len-1])
            else
                BigInt.posn (BigNatModule.of_string str)
              
        member internal x.IsSmall = BigNatModule.is_small (x.V)
        static member Factorial (x:BigInt) =
            if x.IsNegative then invalidArg "x" "the input was negative" 
            elif x.IsPositive then BigInt.posn (BigNatModule.factorial x.V)
            else BigInt.One 

        static member ( ~+ )(n1:BigInt) = n1
        static member (..) (n1:BigInt,n2:BigInt) = OperatorIntrinsics.RangeStepGeneric(BigInt.Zero,(+),n1,BigInt.One,n2)
        static member (.. ..) (n1:BigInt,step:BigInt,n2:BigInt) = OperatorIntrinsics.RangeStepGeneric(BigInt.Zero,(+),n1,step,n2)
  
        static member FromInt64(x:int64) = new BigInt(x)
        static member FromInt32(x:int32) = new BigInt(x)

    type bigint = BigInt

namespace Microsoft.FSharp.Core

#if FX_ATLEAST_40
    // The "bigint" type alias is not defined in the FSharp.Core namespace in Dev10. 
    type bigint = 
      | NotDefined
#else
    type bigint = Microsoft.FSharp.Math.BigInt
#endif
    open System
    open System.Diagnostics.CodeAnalysis
    open System.Globalization
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Math

#if FX_ATLEAST_40
    // No need for FxCop suppressions on Dev10
#else
    // FxCop suppressions
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_Addition(Microsoft.FSharp.Math.BigInt,Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_Division(Microsoft.FSharp.Math.BigInt,Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_GreaterThan(Microsoft.FSharp.Math.BigInt,Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_GreaterThanOrEqual(Microsoft.FSharp.Math.BigInt,Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_LessThan(Microsoft.FSharp.Math.BigInt,Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_LessThanOrEqual(Microsoft.FSharp.Math.BigInt,Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_Modulus(Microsoft.FSharp.Math.BigInt,Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_Multiply(Microsoft.FSharp.Math.BigInt,Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_Subtraction(Microsoft.FSharp.Math.BigInt,Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_UnaryNegation(Microsoft.FSharp.Math.BigInt)")>]
    [<assembly: SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="Microsoft.FSharp.Math.BigInt.#op_UnaryPlus(Microsoft.FSharp.Math.BigInt)")>]
    do()
#endif

    [<AutoOpen>]
    module NumericLiterals =

#if FX_ATLEAST_40
        module NumericLiteralI = 
            let numTy = System.Reflection.Assembly.Load("System").GetType("System.Numerics.BigInteger")
            let meth64 = numTy.GetConstructor([|typeof<int64>|])
            let methString = numTy.GetMethod("Parse",[|typeof<string>; typeof<System.Globalization.NumberStyles>; typeof<System.IFormatProvider> |])

            let tab64 = new System.Collections.Generic.Dictionary<int64,obj>()
            let tabParse = new System.Collections.Generic.Dictionary<string,obj>()
            let FromInt64Dynamic (x64:int64) : obj = 
                lock tab64 (fun () -> 
                    let mutable res = Unchecked.defaultof<_> 
                    let ok = tab64.TryGetValue(x64,&res)
                    if ok then res else 
                    res <- meth64.Invoke [| box x64 |] 
                    tab64.[x64] <- res
                    res)

            let inline get32 (x32:int32) =  FromInt64Dynamic (int64 x32)

            let inline isOX s = not (System.String.IsNullOrEmpty(s)) && s.Length > 2 && s.[0] = '0' && s.[1] = 'x'

            let FromStringDynamic (s:string) : obj = 
                lock tabParse (fun () -> 
                    let mutable res = Unchecked.defaultof<_> 
                    let ok = tabParse.TryGetValue(s,&res)
                    if ok then res else 
                    res <-  
                       if isOX s then 
                           methString.Invoke(null,[| box s; box NumberStyles.AllowHexSpecifier; box CultureInfo.InvariantCulture |] )
                       else
                           methString.Invoke(null,[| box s; box NumberStyles.AllowLeadingSign; box CultureInfo.InvariantCulture |] )
                           
                    tabParse.[s] <- res
                    res)

            let inline FromZero () : 'T = 
                (get32 0 :?> 'T)
                when 'T : bigint = BigInt.Zero 

            let inline FromOne () : 'T = 
                (get32 1 :?> 'T)
                when 'T : bigint = BigInt.One

            let inline FromInt32 (i:int32): 'T = 
                (get32 i :?> 'T)
                when 'T : bigint = new BigInt(i)
            
            let inline FromInt64 (i:int64): 'T = 
                (FromInt64Dynamic i :?> 'T)
                when 'T : bigint = new BigInt(i)
                
            let inline FromString (s:string) : 'T = 
                (FromStringDynamic s :?> 'T)
                when 'T : bigint = 
                   if  isOX s then 
                      BigInt.Parse (s.[2..],NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture)
                   else
                      BigInt.Parse (s,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture)

#else
        module NumericLiteralI = 
            let tab64 = new System.Collections.Generic.Dictionary<int64,bigint>()
            let get64 (x64:int64) = 
                lock tab64 (fun () -> 
                    let mutable res = Unchecked.defaultof<_> 
                    let ok = tab64.TryGetValue(x64,&res)
                    if ok then res else 
                    res <- BigInt.FromInt64 x64
                    tab64.[x64] <- res
                    res)

            let get32 (x32:int32) =  get64 (int64 x32)

            let tabParse = new System.Collections.Generic.Dictionary<string,bigint>()
            let getParse s = 
                lock tabParse (fun () -> 
                    let mutable res = Unchecked.defaultof<_> 
                    let ok = tabParse.TryGetValue(s,&res)
                    if ok then res else 
                    res <-  BigInt.Parse s
                    tabParse.[s] <- res
                    res)

            let FromZero () = BigInt.Zero 
            let FromOne () = BigInt.One 
            let FromInt32 i = get32 i
            let FromInt64 i = get64 i
            let FromString s = getParse s

        module NumericLiteralN = 
            let FromZero () = failwith "this code should not be reachable"
            let FromOne () = failwith "this code should not be reachable"
            let FromInt32 (i:int32) = failwith "this code should not be reachable"
            let FromInt64 (i64:int64) = failwith "this code should not be reachable"
            let FromString (s:string) = failwith "this code should not be reachable"
#endif // FX_ATLEAST_40

#endif
        
