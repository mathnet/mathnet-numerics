// (c) Microsoft Corporation. All rights reserved
module (* internal *) Microsoft.FSharp.Compiler.Range

open Internal.Utilities
open Internal.Utilities.Pervasives
open System.Text
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler  

  
(* we keep a global tables of filenames that we can reference by integers *)
type file_idx = int32 
val file_idx_of_file : string -> file_idx
val file_of_file_idx : file_idx -> string

val col_nbits: int (* number of bits needed to store the largest possible column number we care about *)
val line_nbits: int (* number of bits needed to store the largest possible line number we care about *)

type pos = int32 
val line_of_pos : pos -> int
val col_of_pos : pos -> int
val dest_pos : pos -> int * int
/// Create a position for the given line and column
val mk_pos : line:int -> column:int -> pos

(* the raw bits for the pos *)
val pos_nbits: int (* maximum number of bits needed to store an encoded position *)
val bits_of_pos : pos -> int32
val pos_of_bits : int32 -> pos 

val pos_ord : pos -> pos -> int

type range = int64 

(* this view of range marks uses file indexes explicitly *)
val mk_file_idx_range : file_idx -> pos -> pos -> range
val dest_file_idx_range : range -> file_idx * pos * pos
val file_idx_of_range : range -> file_idx
val start_of_range : range -> pos
val end_of_range : range -> pos

(* this view hides the use of file indexes and just returns the filenames *)
val file_of_range : range -> string
val dest_range : range -> string * pos * pos
val mk_range : string -> pos -> pos -> range

(* derived accessors *)
val start_line_of_range : range -> int
val start_col_of_range : range -> int
val end_line_of_range : range -> int
val end_col_of_range : range -> int

val trim_range_right : range -> int -> range

(* range_ord: not a total order, but enough to sort on ranges *)      
val range_ord : range -> range -> int

val output_pos : out_channel -> pos -> unit
val output_range : out_channel -> range -> unit
val boutput_pos : StringBuilder -> pos -> unit
val boutput_range : StringBuilder -> range -> unit
    
val start_range_of_range : range -> range
val end_range_of_range : range -> range
val pos_gt : pos -> pos -> bool
val pos_eq : pos -> pos -> bool
val pos_geq : pos -> pos -> bool

val union_ranges : range -> range -> range
val range_contains_range : range -> range -> bool
val range_contains_pos : range -> pos -> bool
val range_before_pos : range -> pos -> bool

val rangeN : string -> int -> range
val pos0 : pos
val range0 : range
val rangeStartup : range
val rangeCmdArgs : range
 
// Store a file_idx in the pos_fname field, so we don't have to look up the 
// file_idx hash table to map back from pos_fname to a file_idx during lexing 
//
// Because we are using file indexes we actually encode the corresponding 
// file index within the bits of the string, which allows us to recover the file index 
// efficiently while creating positions during lexing. 
val decode_file_idx : string -> file_idx
val encode_file : string -> string 

(* For diagnostics *)  
val string_of_pos   : pos   -> string
val string_of_range : range -> string
