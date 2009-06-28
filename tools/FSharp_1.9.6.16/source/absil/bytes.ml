// (c) Microsoft Corporation 2005-2009.
#light

/// Byte arrays
#if STANDALONE_METADATA
module FSharp.PowerPack.Metadata.Reader.Bytes
#else
module Microsoft.FSharp.Compiler.AbstractIL.Internal.Bytes 
#endif

open System.IO
open Internal.Utilities
open Internal.Utilities.Pervasives

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 


let b0 n =  (n &&& 0xFF)
let b1 n =  ((n >>> 8) &&& 0xFF)
let b2 n =  ((n >>> 16) &&& 0xFF)
let b3 n =  ((n >>> 24) &&& 0xFF)

let dWw1 n = int32 ((n >>> 32) &&& 0xFFFFFFFFL)
let dWw0 n = int32 (n          &&& 0xFFFFFFFFL)

let length (b:byte[]) = Array.length b
let get (b:byte[]) n = int32 (Array.get b n)  
let make (f : _ -> int) n = Array.init n (fun i -> byte (f i))  
let zero_create n : byte[] = Array.zeroCreate n      

#if STANDALONE_METADATA
#else
let really_input (is:TextReader) n =
    let buff = Array.create n 0uy in 
     really_input is buff 0 n;
     buff

let maybe_input (is:TextReader) n =
    let buff = Array.create n 0uy in 
    let x = input is buff 0 n in 
    Array.sub buff 0 x

let output (os:TextWriter) b =
  Pervasives.output os b 0 (Array.length b) 
#endif
  
let sub ( b:byte[]) s l = Array.sub b s l   
let set bb n (b:int32) = Array.set bb n (byte b) 
let blit (a:byte[]) b c d e = Array.blit a b c d e 
let string_as_unicode_bytes (s:string) = System.Text.Encoding.Unicode.GetBytes s 
let utf8_bytes_as_string (b:byte[]) = System.Text.Encoding.UTF8.GetString b 
let unicode_bytes_as_string (b:byte[]) = System.Text.Encoding.Unicode.GetString b 
let compare (b1:byte[]) (b2:byte[]) = compare b1 b2

let to_intarray (b:byte[]) =  Array.init (length b) (get b)
let of_intarray (arr:int[]) = make (fun i -> arr.[i]) (Array.length arr)

let string_as_utf8_bytes (s:string) = System.Text.Encoding.UTF8.GetBytes s  

let append (b1: byte[]) (b2:byte[]) = Array.append b1 b2 

let string_as_utf8_bytes_null_terminated (s:string) = 
    append (string_as_utf8_bytes s) (of_intarray [| 0x0 |]) 

let string_as_unicode_bytes_null_terminated (s:string) = 
    append (string_as_unicode_bytes s) (of_intarray [| 0x0;0x0 |]) 


module Bytestream = 
    type t = { bytes: byte[]; mutable pos: int; max: int }

    let of_bytes b n len = 
        if n < 0 or (n+len) > length b then failwith "Bytestream.of_bytes";
        { bytes = b; pos = n; max = n+len }

    let read_byte b  = 
        if b.pos >= b.max then failwith "Bytestream.of_bytes.read_byte: end of stream";
        let res = get b.bytes b.pos in
        b.pos <- b.pos + 1;
        res 
      
    let read_bytes b n  = 
        if b.pos + n > b.max then failwith "Bytestream.read_bytes: end of stream";
        let res = sub b.bytes b.pos n in
        b.pos <- b.pos + n;
        res 

    let position b = b.pos 
    let clone_and_seek b pos = { bytes=b.bytes; pos=pos; max=b.max }
    let skip b n = b.pos <- b.pos + n

    let read_utf8_bytes_as_string (b:t) n = 
        let res = System.Text.Encoding.UTF8.GetString(b.bytes,b.pos,n) in  
        b.pos <- b.pos + n; res 


module Bytebuf = 
    type t = 
        { mutable bbArray: byte[]; 
          mutable bbCurrent: int }

    let create sz = 
        { bbArray=zero_create sz; 
          bbCurrent = 0; }
        
    let ensure_bytebuf buf new_size = 
        let old_buf_size = buf.bbArray.Length in 
        if new_size > old_buf_size then begin
          let old = buf.bbArray in 
          buf.bbArray <- zero_create (max new_size (old_buf_size * 2));
          blit old 0 buf.bbArray 0 buf.bbCurrent;
        end

    let close buf = sub buf.bbArray 0 buf.bbCurrent

    let emit_int_as_byte buf i = 
        let new_size = buf.bbCurrent + 1 in 
        ensure_bytebuf buf new_size;
        set buf.bbArray buf.bbCurrent i;
        buf.bbCurrent <- new_size 

    let emit_byte buf (b:byte) = emit_int_as_byte buf (int b)
    let emit_bool_as_byte buf (b:bool) = emit_int_as_byte buf (if b then 1 else 0)

    let emit_bytes buf i = 
        let n = length i in 
        let new_size = buf.bbCurrent + n in 
        ensure_bytebuf buf new_size;
        blit i 0 buf.bbArray buf.bbCurrent n;
        buf.bbCurrent <- new_size 

    let emit_i32_as_u16 buf n = 
        let new_size = buf.bbCurrent + 2 in 
        ensure_bytebuf buf new_size;
        set buf.bbArray buf.bbCurrent (b0 n);
        set buf.bbArray (buf.bbCurrent + 1) (b1 n);
        buf.bbCurrent <- new_size 
    
    let emit_u16 buf (x:uint16) = emit_i32_as_u16 buf (int32 x)

    let fixup_i32 bb pos n = 
        set bb.bbArray pos (b0 n);
        set bb.bbArray (pos + 1) (b1 n);
        set bb.bbArray (pos + 2) (b2 n);
        set bb.bbArray (pos + 3) (b3 n);

    let emit_i32 buf n = 
        let new_size = buf.bbCurrent + 4 in 
        ensure_bytebuf buf new_size;
        fixup_i32 buf buf.bbCurrent n;
        buf.bbCurrent <- new_size 

    let emit_i64 buf x = 
      emit_i32 buf (dWw0 x);
      emit_i32 buf (dWw1 x)

    let emit_intarray_as_bytes buf arr = 
        let n = Array.length arr in 
        let new_size = buf.bbCurrent + n in 
        ensure_bytebuf buf new_size;
        let bbarr = buf.bbArray in
        let bbbase = buf.bbCurrent in
        for i= 0 to n - 1 do set bbarr (bbbase + i) arr.[i] done;
        buf.bbCurrent <- new_size 

    let length bb = bb.bbCurrent
    let position bb = bb.bbCurrent



