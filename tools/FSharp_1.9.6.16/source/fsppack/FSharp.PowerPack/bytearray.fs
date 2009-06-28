// (c) Microsoft Corporation 2005-2009. 

#if INTERNALIZED_POWER_PACK
module internal Internal.Utilities.Bytearray
#else
module Microsoft.FSharp.Compatibility.Bytearray
#endif

let compare (x:byte[]) y = compare x y

type bytearray = byte[]
let length (arr: byte[]) =  Array.length arr
let get (arr: byte[]) (n:int) =  Array.get arr n
let set (arr: byte[]) (n:int) (x:byte) =  Array.set arr n x
let zero_create (n:int) : byte[]= Array.zeroCreate n
let make  (n:int) = (zero_create n : byte[]) 
let create (n:int)  = make n
let init (n:int) (f: int -> byte) =  Array.init n f
let concat (arrs:byte[] list) = Array.concat arrs
let append arr1 arr2 = concat [arr1; arr2]
let sub (arr:byte[]) (start:int) (len:int) = Array.sub arr start len
let fill (arr:byte[]) (start:int) (len:int) (x:byte) = Array.fill arr start len x
let copy (arr:byte[]) = Array.copy arr
let blit (arr1:byte[]) (start1:int) (arr2: byte[]) (start2:int) (len:int) = Array.blit arr1 start1 arr2 start2 len
let to_list (arr:byte[]) = Array.to_list arr  
let of_list (l:byte list) = Array.of_list l
let iter (f : byte -> unit) (arr:byte[]) = Array.iter f arr
let map (f: byte -> byte) (arr:byte[]) = Array.map f arr
let iteri (f : int -> byte -> unit) (arr:byte[]) = Array.iteri f arr
let mapi (f: int -> byte -> byte) (arr:byte[]) = Array.mapi f arr
let fold_left (f : 'State -> byte -> 'State) (acc: 'State) (arr:byte[]) = Array.fold f acc arr
let fold_right (f : byte -> 'State -> 'State) (arr:byte[]) (acc: 'State) = Array.foldBack f arr acc

type encoding = System.Text.Encoding

#if FX_NO_ASCII_ENCODING
#else
let ascii_to_string (b:byte[]) = System.Text.Encoding.ASCII.GetString(b,0,b.Length)
  
let string_to_ascii (s:string) = System.Text.Encoding.ASCII.GetBytes(s)
#endif
