// (c) Microsoft Corporation 2005-2009. 

(* Rebind z,q,nums to implement the ocaml library APIs *)

module Microsoft.FSharp.Compatibility.OCaml.Num
open Microsoft.FSharp.Compatibility.OCaml
open Microsoft.FSharp.Compatibility.OCaml.Pervasives
open Microsoft.FSharp.Core
open Microsoft.FSharp.Math

type num = bignum

let Big_int(x) = BigNum.FromBigInt x
let Int(x) = BigNum.FromInt x

let minus_num(x:bignum) = (x)
  
let add_num (x:bignum) y = x + y 
let sub_num (x:bignum) y = x - y 
let mult_num (x:bignum) y = x *  y 
let div_num  (x:bignum) y = x / y 
let abs_num (x:bignum) = abs x
let succ_num (x:bignum) = x + BigNum.One
let pred_num (x:bignum) = x - BigNum.One
let pow_num (x:bignum) n = BigNum.PowN(x,n)
let incr_num r = r := succ_num !r
let decr_num r = (r := pred_num !r)

let sign_num(n:bignum) = sign n

let ( +/ ) x y = add_num x y 
let ( -/ ) x y = sub_num x y 
let ( */ ) x y = mult_num x y 
let ( >/ ) (x:bignum) y = x > y 
let ( </ ) (x:bignum) y = x < y 
let ( <=/ ) (x:bignum) y = x <= y 
let ( >=/ ) (x:bignum) y = x >= y 


let compare_num (x:bignum) y = compare x y 
let max_num  (x:bignum) y = max x y 
let min_num  (x:bignum) y = min x y 
let to_float (n:bignum) = float n

let ( <>/ ) x y = not ((compare_num x y) = 0)
let ( =/ ) x y = ((compare_num x y) = 0)

let big_int_of_num(n) = BigNum.ToBigInt n
let num_of_big_int(x) = Big_int x 
let float_of_num(x:bignum) = float x
let int_of_num(x:bignum) = int x

let neg (x:bignum) = - x
let add (x:bignum) y = x + y
let sub (x:bignum) y = x - y 
let mul (x:bignum) y = x * y 
let div (x:bignum) y = x / y 
let pow (x:bignum) y = BigNum.PowN(x,y)

let string_of_num (x:bignum) = x.ToString()
let num_of_string (s:string) = BigNum.Parse(s)
