// (c) Microsoft Corporation 2005-2009.  

namespace Microsoft.FSharp.Compatibility

open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Core.Operators

/// ML-like operations on 32-bit System.Single floating point numbers.
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Float32 = 

    let add (x:float32) (y:float32) = x + y
    let sub (x:float32) (y:float32) = x - y
    let mul (x:float32) (y:float32) = x * y
    let div (x:float32) (y:float32) = x / y
    let neg (x:float32) =  -x

    let compare (x:float32) y = compare x y

    let of_int (n:int) =  float32 n
    let to_int (x:float32) = int x

    let of_int64 (x:int64) = float32 x
    let to_int64 (x:float32) = int64 x

    let of_int32 (x:int32) = float32 x
    let to_int32 (x:float32) = int32 x

    let of_float (x:float) = float32 x
    let to_float (x:float32) = float x

    let to_string (x:float32) = string x
    let of_string (s:string) = 
      (* Note System.Single.Parse doesn't handle -0.0 correctly (it returns +0.0) *)
      let s = s.Trim()  
      let l = s.Length 
      let p = 0 
      let p,sign = if (l >= p + 1 && s.[p] = '-') then 1,false else 0,true 
      let n = 
        try 
          if p >= l then raise (new System.FormatException()) 
          System.Single.Parse(s.[p..],System.Globalization.CultureInfo.InvariantCulture)
        with :? System.FormatException -> failwith "Float32.of_string"
      if sign then n else -n

    let to_bits (x:float32) = System.BitConverter.ToInt32(System.BitConverter.GetBytes(x),0)
    let of_bits (x:int32) = System.BitConverter.ToSingle(System.BitConverter.GetBytes(x),0)


