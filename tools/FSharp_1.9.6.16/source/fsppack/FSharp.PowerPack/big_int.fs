// (c) Microsoft Corporation 2005-2009. 
module Microsoft.FSharp.Compatibility.OCaml.Big_int

open Microsoft.FSharp.Math
open Microsoft.FSharp.Compatibility
open Microsoft.FSharp.Compatibility.OCaml
open Microsoft.FSharp.Compatibility.OCaml.Pervasives

type big_int = bigint
let zero_big_int = BigInt.Zero
let unit_big_int = BigInt.One
let minus_big_int    (a:BigInt) b  = a - b
let add_big_int      (a:BigInt) b  = a + b
let succ_big_int     n    = n + BigInt.One
let add_int_big_int  (a:int) b  = new BigInt(a) + b
let sub_big_int      (a:BigInt) b  = a - b
let pred_big_int     (a:BigInt)    = a - BigInt.One
let mult_big_int     (a:BigInt) b  = a * b
let mult_int_big_int (a:int) b  = new BigInt(a) * b
let square_big_int   (a:BigInt)= a  * a
let quomod_big_int   a b  = BigInt.DivRem (a,b)
let div_big_int      (a:BigInt) b  =  a / b    
let mod_big_int      (a:BigInt) b  = a % b    
let gcd_big_int      a b  = 
#if FX_ATLEAST_40
    BigInt.GreatestCommonDivisor (a,b)
#else
    BigInt.Gcd (a,b)
#endif
#if FX_ATLEAST_40
#else
let power_int_positive_int         (x:int) (n:int) = BigInt.Pow (BigInt x, BigInt n)
let power_big_int_positive_int     x (n:int) = BigInt.Pow  (x, BigInt n)
let power_int_positive_big_int     (x:int) n = BigInt.Pow  (BigInt x,n)
let power_big_int_positive_big_int x n = BigInt.Pow   (x,n)
let sign_big_int    (a:BigInt)   = sign a
let compare_big_int (x:BigInt) y = (x - y).Sign
#endif
let eq_big_int  (x:bigint) (y:bigint) = BigInt.(=) (x,y)
let le_big_int  (x:bigint) (y:bigint) = BigInt.(<=)   (x,y)
let ge_big_int  (x:bigint) (y:bigint) = BigInt.(>=)   (x,y)
let lt_big_int  (x:bigint) (y:bigint) = BigInt.(<)    (x,y)
let gt_big_int  (x:bigint) (y:bigint) = BigInt.(>)    (x,y)
let max_big_int (x:BigInt) y = max x y
let min_big_int (x:BigInt) y = min x y
let string_of_big_int (x:BigInt) = x.ToString()
let big_int_of_string (x:string) = BigInt.Parse x
let int_of_big_int  (x:BigInt)    = 
#if FX_ATLEAST_40
    (BigInt.op_Explicit x : int)
#else
    int x
#endif
let big_int_of_int (x:int)    = new BigInt(x)
let float_of_big_int (x:BigInt)  = 
#if FX_ATLEAST_40
    (BigInt.op_Explicit x : float)
#else
    float x
#endif

