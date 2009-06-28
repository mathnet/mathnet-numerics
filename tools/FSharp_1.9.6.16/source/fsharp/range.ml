// (c) Microsoft Corporation. All rights reserved
#light

/// Anything to do with special names of identifiers and other lexical rules 
module (* internal *) Microsoft.FSharp.Compiler.Range

open System.IO
open System.Collections.Generic
open Microsoft.FSharp.Text.Printf

open Internal.Utilities
open Internal.Utilities.Pervasives

open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler  
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Lib.Bits



type file_idx = int32 
type pos = int32 
type range = int64 
(*    { rangeFile: string;
      rangeBegin: pos;
      rangeEnd: pos }  *) 
    (* { posLine: int; posCol: int }  *)

let col_nbits  = 9
let line_nbits  = 16

let pos_nbits = line_nbits + col_nbits
let _ = assert (pos_nbits <= 32)
let pos_col_mask  = mask32 0         col_nbits
let line_col_mask = mask32 col_nbits line_nbits

let mk_pos l c = 
    let l = max 0 l in
    let c = max 0 c in
        ( c &&& pos_col_mask)
    ||| ((l <<< col_nbits) &&& line_col_mask)

let inline (lsr)  (x:int) (y:int)  = int32 (uint32 x >>> y)

let line_of_pos p =  (p lsr col_nbits)
let col_of_pos p =  (p &&& pos_col_mask)

let bits_of_pos (x:pos) :int32 = x
let pos_of_bits (x:int32) : pos = x

let file_idx_nbits = 14
let start_line_nbits = line_nbits
let start_col_nbits = col_nbits
let end_line_nbits = line_nbits
let end_col_nbits = col_nbits
let _ = assert (file_idx_nbits + start_line_nbits + start_col_nbits + end_line_nbits + end_col_nbits = 64)

let file_idx_mask   = mask64 0 file_idx_nbits
let start_line_mask = mask64 (file_idx_nbits) start_line_nbits
let start_col_mask  = mask64 (file_idx_nbits + start_line_nbits) start_col_nbits
let end_line_mask   = mask64 (file_idx_nbits + start_line_nbits + start_col_nbits) end_line_nbits
let end_col_mask    = mask64 (file_idx_nbits + start_line_nbits + start_col_nbits + end_line_nbits) end_col_nbits

let mk_file_idx_range fidx b e = 
        int64(fidx)
    ||| (int64(line_of_pos b) <<< file_idx_nbits) 
    ||| (int64(col_of_pos b)  <<< (file_idx_nbits + start_line_nbits))
    ||| (int64(line_of_pos e) <<< (file_idx_nbits + start_line_nbits + start_col_nbits))
    ||| (int64(col_of_pos e)  <<< (file_idx_nbits + start_line_nbits + start_col_nbits + end_line_nbits))
let file_idx_of_range r   = int32(r &&& file_idx_mask)
let start_line_of_range r = int32((r &&& start_line_mask) >>> file_idx_nbits)
let start_col_of_range r  = int32((r &&& start_col_mask)  >>> (file_idx_nbits + start_line_nbits)) 
let end_line_of_range r   = int32((r &&& end_line_mask)   >>> (file_idx_nbits + start_line_nbits + start_col_nbits)) 
let end_col_of_range r    = int32((r &&& end_col_mask)    >>> (file_idx_nbits + start_line_nbits + start_col_nbits + end_line_nbits)) 


// This is just a standard unique-index table
type FileIndexTable() = 
    class
        let indexToFileTable = new ResizeArray<_>(11)
        let fileToIndexTable = new Dictionary<string,int>(11)
        member t.FileToIndex f = 
            let mutable res = 0 in
            let ok = fileToIndexTable.TryGetValue(f,&res) in
            if ok then res 
            else
                lock fileToIndexTable (fun () -> 
                    let mutable res = 0 in
                    let ok = fileToIndexTable.TryGetValue(f,&res) in
                    if ok then res 
                    else
                        let n = indexToFileTable.Count in
                        indexToFileTable.Add(f);
                        fileToIndexTable.[f] <- n;
                        n)

        member t.IndexToFile n = 
            (if n < 0 then failwithf "file_of_file_idx: negative argument: n = %d\n" n);
            (if n >= indexToFileTable.Count then failwithf "file_of_file_idx: invalid argument: n = %d\n" n);
            indexToFileTable.[n]
    end
    
let maxFileIndex = pown32 file_idx_nbits

// WARNING: Global Mutable State, holding a mapping between integers and filenames
let fileIndexTable = new FileIndexTable()
// Note if we exceed the maximum number of files we'll start to report incorrect file names
let file_idx_of_file f = fileIndexTable.FileToIndex(f) % maxFileIndex 
let file_of_file_idx n = fileIndexTable.IndexToFile(n)

let mk_range f b e = mk_file_idx_range (file_idx_of_file f) b e
let file_of_range r = file_of_file_idx (file_idx_of_range r)


(* end representation, start derived ops *)

let start_of_range r = mk_pos (start_line_of_range r) (start_col_of_range r)
let end_of_range r = mk_pos (end_line_of_range r) (end_col_of_range r)
let dest_file_idx_range r = file_idx_of_range r,start_of_range r,end_of_range r
let dest_range r = file_of_range r,start_of_range r,end_of_range r
let dest_pos p = line_of_pos p,col_of_pos p

let trim_range_right r n = 
    let fidx,p1,p2 = dest_file_idx_range r in 
    let l2,c2 = dest_pos p2 in 
    mk_file_idx_range fidx p1 (mk_pos l2 (max 0 (c2 - n)))
                 
let pos_ord   p1 p2 = Pair.order (Int32.order   ,Int32.order) (dest_pos p1) (dest_pos p2)
(* range_ord: not a total order, but enough to sort on ranges *)      
let range_ord r1 r2 = Pair.order (String.order,pos_ord) (file_of_range r1,start_of_range r1) (file_of_range r2,start_of_range r2)

let output_pos (os:TextWriter) m = fprintf os "(%d,%d)" (line_of_pos m) (col_of_pos m)
let output_range (os:TextWriter) m = fprintf os "%s%a-%a" (file_of_range m) output_pos (start_of_range m) output_pos (end_of_range m)
let boutput_pos os m = bprintf os "(%d,%d)" (line_of_pos m) (col_of_pos m)
let boutput_range os m = bprintf os "%s%a-%a" (file_of_range m) boutput_pos (start_of_range m) boutput_pos (end_of_range m)
    
let start_range_of_range m =    let f,s,e = dest_file_idx_range m in mk_file_idx_range f s s
let end_range_of_range m =   let f,s,e = dest_file_idx_range m in mk_file_idx_range f e e
let pos_gt p1 p2 =
   (line_of_pos p1 > line_of_pos p2 or
      (line_of_pos p1 = line_of_pos p2 &&
       col_of_pos p1 > col_of_pos p2))

let pos_eq p1 p2 = (line_of_pos p1 = line_of_pos p2 &&  col_of_pos p1 = col_of_pos p2)
let pos_geq p1 p2 = pos_eq p1 p2 or pos_gt p1 p2

let union_ranges m1 m2 = 
    if file_idx_of_range m1 <> file_idx_of_range m2 then m2 else
    let b = 
      if pos_geq (start_of_range m1) (start_of_range m2) then (start_of_range m2)
      else (start_of_range m1) in 
    let e = 
      if pos_geq (end_of_range m1) (end_of_range m2) then (end_of_range m1)
      else (end_of_range m2) in 
    mk_file_idx_range (file_idx_of_range m1) b e

let range_contains_range m1 m2 =
    (file_of_range m1) = (file_of_range m2) &&
    pos_geq (start_of_range m2) (start_of_range m1) &&
    pos_geq (end_of_range m1) (end_of_range m2)

let range_contains_pos m1 p =
    pos_geq p (start_of_range m1) &&
    pos_geq (end_of_range m1) p

let range_before_pos m1 p =
    pos_geq p (end_of_range m1)

let rangeN filename line =  mk_range filename (mk_pos line 0) (mk_pos line 80)
let pos0 = mk_pos 1 0
let range0 =  rangeN "unknown" 1
let rangeStartup = rangeN "startup" 1
let rangeCmdArgs = rangeN "commandLineArgs" 0

// Store a file_idx in the pos_fname field, so we don't have to look up the 
// file_idx hash table to map back from pos_fname to a file_idx during lexing 
let encode_file_idx idx = 
   Bytes.utf8_bytes_as_string (Bytes.of_intarray [|  (idx &&& 0x7F); 
                                                     ((idx lsr 7) &&& 0x7F)  |])

let encode_file file = file |> file_idx_of_file |> encode_file_idx

let _ = assert (file_idx_nbits <= 14) (* this encoding is size limited *)
let decode_file_idx (s:string) = 
    if String.length s = 0 then 0 else 
    let idx =   (int32 s.[0]) 
             ||| ((int32 s.[1]) <<< 7) in 
    idx
           
(* For Diagnostics *)
let string_of_pos   pos = let line,col = line_of_pos pos,col_of_pos pos in sprintf "(%d,%d)" line col
let string_of_range r   = sprintf "%s%s-%s" (file_of_range r) (string_of_pos (start_of_range r)) (string_of_pos (end_of_range r))
