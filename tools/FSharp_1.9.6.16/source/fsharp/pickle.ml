// (c) Microsoft Corporation. All rights reserved

#light

#if STANDALONE_METADATA

module (* internal *) FSharp.PowerPack.Metadata.Reader.Internal.Pickle

open System.Collections.Generic 
open FSharp.PowerPack.Metadata.Reader.Internal.AbstractIL.IL
open FSharp.PowerPack.Metadata.Reader.Internal.Tast
open FSharp.PowerPack.Metadata.Reader.Internal.Prelude
#else
module Microsoft.FSharp.Compiler.Pickle 

open System.Collections.Generic
open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler 

module Ilprint = Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 
module Ilx    = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Lib.Bits
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types


#endif

let verbose = false

let ffailwith fileName str = failwithf "Error reading/writing metadata for the F# compiled DLL %s. Was the DLL compiled with an earlier version of the F# compiler? (error: %s)" fileName str 

// Fixup pickled data w.r.t. a set of CCU thunks indexed by name
type PickledDataWithReferences<'rawData> = 
    { /// The data that uses a collection of CcuThunks internally
      RawData: 'rawData; 
      /// The assumptions that need to be fixed up
      FixupThunks: list<CcuThunk> } 

    member x.Fixup loader =
        x.FixupThunks |> List.iter (fun reqd -> reqd.Fixup(loader reqd.AssemblyName)) ;
        x.RawData

    /// Like Fixup but loader may return None, in which case there is no fixup.
    member x.OptionalFixup loader =
        x.FixupThunks 
        |> List.iter(fun reqd->
            match loader reqd.AssemblyName with 
            | Some(loaded) -> reqd.Fixup(loaded)
            | None -> reqd.FixupOrphaned() );
        x.RawData

(*
    /// Like Fixup but loader may return None, in which case there is no fixup.
    member x.OptionalFixup loader =
        x.FixupThunks 
        |> List.iter(fun reqd->
            match loader reqd with 
            | Some(loaded) -> fixer (reqd,loaded)
            | None -> reqd.FixupOrphaned() );
        x.RawData
*)
    

(*---------------------------------------------------------------------------
 * Basic pickle/unpickle state
 *------------------------------------------------------------------------- *)

type 'a tbl = 
    { name: string;
      tbl: Dictionary<'a, int>;
      mutable rows: ResizeArray<'a>;
      mutable count: int }

let inline new_tbl n = 
  { name = n;
    tbl = new System.Collections.Generic.Dictionary<_,_>(1000, HashIdentity.Structural);
    rows= new ResizeArray<_>(1000);
    count=0; }

let get_tbl tbl = Seq.to_array tbl.rows
let tbl_size tbl = tbl.rows.Count

let add_entry tbl x =
    let n = tbl.count
    tbl.count <- tbl.count + 1;
    tbl.tbl.[x] <- n;
    tbl.rows.Add(x);
    n

let find_or_add_entry tbl x =
    let mutable res = Unchecked.defaultof<_>
    let ok = tbl.tbl.TryGetValue(x,&res)
    if ok then res else add_entry tbl x

type 'a itbl = 
    { itbl_name: string;
      itbl_rows: 'a array }

let new_itbl n r = { itbl_name=n; itbl_rows=r }

#if INCLUDE_METADATA_WRITER
type osgn_outmap<'data,'osgn> = 
    | ObservableNodeOutMap of 
        ('osgn -> stamp) * 
        ('osgn -> string) * 
        ('osgn -> range) * 
        ('osgn -> 'data) * 
        string * 
        stamp tbl 
(* inline this to get known-type-information through to the Hashtbl.create *)
let inline new_osgn_outmap f g rangeF h nm = ObservableNodeOutMap (f, g,rangeF,h,nm, new_tbl nm)
let osgn_outmap_size (ObservableNodeOutMap(_,_,_,_,_,x)) = tbl_size x

type WriterState = 
  { os: Bytes.Bytebuf.t; 
    oscope: ccu;
    occus: CcuReference tbl; 
    otycons: osgn_outmap<EntityData,Tycon>; 
    otypars: osgn_outmap<TyparData,Typar>; 
    ovals: osgn_outmap<ValData,Val>;
    ostrings: string tbl; 
    opubpaths: (int[] * int) tbl; 
    onlpaths: (int * int[]) tbl; 
    osimpletyps: (int * int) tbl;
    oglobals : Env.TcGlobals;
    ofile : string;
  }
let pfailwith st str = ffailwith st.ofile str

#endif
    
type osgn_inmap<'data,'osgn> = 
    | ObservableNodeInMap of ('osgn -> 'data -> unit) * ('osgn -> bool) * string * 'osgn array 
    member x.Get(n:int) = let (ObservableNodeInMap(_,_,_,arr)) = x in arr.[n]
let new_osgn_inmap mk lnk isLinked nm n = ObservableNodeInMap (lnk,isLinked,nm, Array.init n (fun i -> mk() ))

type ReaderState = 
  { is: Bytes.Bytestream.t; 
    iilscope: ILScopeRef;
    iccus: CcuThunk itbl; 
    itycons: osgn_inmap<EntityData,Tycon>;  
    itypars: osgn_inmap<TyparData,Typar>; 
    ivals: osgn_inmap<ValData,Val>;
    istrings: string itbl;
    ipubpaths: PublicPath itbl; 
    inlpaths: NonLocalPath itbl; 
    isimpletyps: typ itbl;
    ifile: string;
  }

let ufailwith st str = ffailwith st.ifile str

(*---------------------------------------------------------------------------
 * Basic pickle/unpickle operations
 *------------------------------------------------------------------------- *)
 
#if INCLUDE_METADATA_WRITER

type 'a pickler = 'a -> WriterState -> unit

let p_byte b st = Bytes.Bytebuf.emit_int_as_byte st.os b
let p_bool b st = p_byte (if b then 1 else 0) st
let p_void (os: WriterState) = ()
let p_unit () (os: WriterState) = ()
let prim_p_int32 i st = 
    p_byte (b0 i) st;
    p_byte (b1 i) st;
    p_byte (b2 i) st;
    p_byte (b3 i) st

/// Compress integers according to the same scheme used by CLR metadata 
/// This halves the size of pickled data 
let p_int32 n st = 
    if n >= 0 &  n <= 0x7F then 
        p_byte (b0 n) st
    else if n >= 0x80 & n <= 0x3FFF then  
        p_byte ( (0x80 ||| (n >>> 8))) st; 
        p_byte ( (n &&& 0xFF)) st 
    else 
        p_byte 0xFF st;
        prim_p_int32 n st

let p_bytes s st = 
    let len = Bytes.length s
    p_int32 (len) st;
    Bytes.Bytebuf.emit_bytes st.os s

let p_prim_string s st = 
    let bytes = Bytes.string_as_utf8_bytes s
    let len = Bytes.length bytes
    p_int32 (len) st;
    Bytes.Bytebuf.emit_bytes st.os bytes

let p_int c st = p_int32 c st
let p_int8 (i:sbyte) st = p_int32 (int32 i) st
let p_uint8 (i:byte) st = p_byte (int i) st
let p_int16 (i:int16) st = p_int32 (int32 i) st
let p_uint16 (x:uint16) st = p_int32 (int32 x) st
let p_uint32 (x:uint32) st = p_int32 (int32 x) st
let p_int64 (i:int64) st = 
    p_int32 (int32 (i &&& 0xFFFFFFFFL)) st;
    p_int32 (int32 (i >>> 32)) st

let p_uint64 (x:uint64) st = p_int64 (int64 x) st

let bits_of_float32 (x:float32) = System.BitConverter.ToInt32(System.BitConverter.GetBytes(x),0)
let bits_of_float (x:float) = System.BitConverter.DoubleToInt64Bits(x)

let p_single i st = p_int32 (bits_of_float32 i) st
let p_double i st = p_int64 (bits_of_float i) st
let p_ieee64 i st = p_int64 (bits_of_float i) st
let p_char i st = p_uint16 (uint16 (int32 i)) st
let inline p_tup2 p1 p2 (a,b) (st:WriterState) = (p1 a st : unit); (p2 b st : unit)
let inline p_tup3 p1 p2 p3 (a,b,c) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit)
let inline  p_tup4 p1 p2 p3 p4 (a,b,c,d) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit)
let inline  p_tup5 p1 p2 p3 p4 p5 (a,b,c,d,e) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit)
let inline  p_tup6 p1 p2 p3 p4 p5 p6 (a,b,c,d,e,f) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit)
let inline  ptup7 p1 p2 p3 p4 p5 p6 p7 (a,b,c,d,e,f,x7) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit); (p7 x7 st : unit)
let inline  ptup8 p1 p2 p3 p4 p5 p6 p7 p8 (a,b,c,d,e,f,x7,x8) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit); (p7 x7 st : unit); (p8 x8 st : unit)
let inline  ptup9 p1 p2 p3 p4 p5 p6 p7 p8 p9 (a,b,c,d,e,f,x7,x8,x9) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit); (p7 x7 st : unit); (p8 x8 st : unit); (p9 x9 st : unit)
let inline  ptup10 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 (a,b,c,d,e,f,x7,x8,x9,x10) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit); (p7 x7 st : unit); (p8 x8 st : unit); (p9 x9 st : unit); (p10 x10 st : unit)
let inline  ptup11 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 (a,b,c,d,e,f,x7,x8,x9,x10,x11) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit); (p7 x7 st : unit); (p8 x8 st : unit); (p9 x9 st : unit); (p10 x10 st : unit); (p11 x11 st : unit)
let inline  ptup12 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 p12 (a,b,c,d,e,f,x7,x8,x9,x10,x11,x12) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit); (p7 x7 st : unit); (p8 x8 st : unit); (p9 x9 st : unit); (p10 x10 st : unit); (p11 x11 st : unit); (p12 x12 st : unit)
let inline  ptup13 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 p12 p13 (a,b,c,d,e,f,x7,x8,x9,x10,x11,x12,x13) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit); (p7 x7 st : unit); (p8 x8 st : unit); (p9 x9 st : unit); (p10 x10 st : unit); (p11 x11 st : unit); (p12 x12 st : unit); (p13 x13 st : unit)
let inline  ptup14 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 p12 p13 p14 (a,b,c,d,e,f,x7,x8,x9,x10,x11,x12,x13,x14) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit); (p7 x7 st : unit); (p8 x8 st : unit); (p9 x9 st : unit); (p10 x10 st : unit); (p11 x11 st : unit); (p12 x12 st : unit); (p13 x13 st : unit) ; (p14 x14 st : unit)
let inline  ptup15 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 p12 p13 p14 p15 (a,b,c,d,e,f,x7,x8,x9,x10,x11,x12,x13,x14,x15) (st:WriterState) = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit); (p6 f st : unit); (p7 x7 st : unit); (p8 x8 st : unit); (p9 x9 st : unit); (p10 x10 st : unit); (p11 x11 st : unit); (p12 x12 st : unit); (p13 x13 st : unit) ; (p14 x14 st : unit); (p15 x15 st : unit)

#endif

let u_byte st = Bytes.Bytestream.read_byte st.is

type 'a unpickler = ReaderState -> 'a

let u_bool st = let b = u_byte st in (b = 1) 

let u_void (is: ReaderState) = ()

let u_unit (is: ReaderState) = ()

let prim_u_int32 st = 
    let b0 =  (u_byte st)
    let b1 =  (u_byte st)
    let b2 =  (u_byte st)
    let b3 =  (u_byte st)
    b0 ||| (b1 <<< 8) ||| (b2 <<< 16) ||| (b3 <<< 24)

let u_int32 st = 
    let b0 = u_byte st
    if b0 <= 0x7F then b0 
    else if b0 <= 0xbf then 
        let b0 = b0 &&& 0x7F
        let b1 = (u_byte st)
        (b0 <<< 8) ||| b1
    else  
        assert(b0 = 0xFF);
        prim_u_int32 st

let u_bytes st = 
    let n =  (u_int32 st)
    Bytes.Bytestream.read_bytes st.is n

let u_prim_string st = 
    let len =  (u_int32 st)
    Bytes.Bytestream.read_utf8_bytes_as_string st.is len

let u_int st = u_int32 st
let u_int8 st = sbyte (u_int32 st)
let u_uint8 st = byte (u_byte st)
let u_int16 st = int16 (u_int32 st)
let u_uint16 st = uint16 (u_int32 st)
let u_uint32 st = uint32 (u_int32 st)
let u_int64 st = 
    let b1 = (int64 (u_int32 st)) &&& 0xFFFFFFFFL
    let b2 = int64 (u_int32 st)
    b1 ||| (b2 <<< 32)

let u_uint64 st = uint64 (u_int64 st)
let float32_of_bits (x:int32) = System.BitConverter.ToSingle(System.BitConverter.GetBytes(x),0)
let float_of_bits (x:int64) = System.BitConverter.Int64BitsToDouble(x)

let u_single st = float32_of_bits (u_int32 st)
let u_double st = float_of_bits (u_int64 st)

let u_ieee64 st = float_of_bits (u_int64 st)

let u_char st = char (int32 (u_uint16 st))


let inline  u_tup2 p1 p2 (st:ReaderState) = let a = p1 st in let b = p2 st in (a,b)
let inline  u_tup3 p1 p2 p3 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in (a,b,c)
let inline u_tup4 p1 p2 p3 p4 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in (a,b,c,d)
let inline u_tup5 p1 p2 p3 p4 p5 (st:ReaderState) =
  let a = p1 st 
  let b = p2 st 
  let c = p3 st 
  let d = p4 st 
  let e = p5 st 
  (a,b,c,d,e)
let inline u_tup6 p1 p2 p3 p4 p5 p6 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in let e = p5 st in let f = p6 st in (a,b,c,d,e,f)
let inline utup7 p1 p2 p3 p4 p5 p6 p7 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in let e = p5 st in let f = p6 st in let x7 = p7 st in (a,b,c,d,e,f,x7)
let inline utup8 p1 p2 p3 p4 p5 p6 p7 p8 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in let e = p5 st in let f = p6 st in let x7 = p7 st in let x8 = p8 st in  (a,b,c,d,e,f,x7,x8)
let inline utup9 p1 p2 p3 p4 p5 p6 p7 p8 p9 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in let e = p5 st in let f = p6 st in let x7 = p7 st in let x8 = p8 st in let x9 = p9 st in (a,b,c,d,e,f,x7,x8,x9)
let inline utup10 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in
  let e = p5 st in let f = p6 st in let x7 = p7 st in let x8 = p8 st in
  let x9 = p9 st in let x10 = p10 st in (a,b,c,d,e,f,x7,x8,x9,x10)
let inline utup11 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in
  let e = p5 st in let f = p6 st in let x7 = p7 st in let x8 = p8 st in
  let x9 = p9 st in let x10 = p10 st in let x11 = p11 st in (a,b,c,d,e,f,x7,x8,x9,x10,x11)
let inline utup12 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 p12 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in
  let e = p5 st in let f = p6 st in let x7 = p7 st in let x8 = p8 st in
  let x9 = p9 st in let x10 = p10 st in let x11 = p11 st in let x12 = p12 st in
  (a,b,c,d,e,f,x7,x8,x9,x10,x11,x12)
let inline utup13 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 p12 p13 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in
  let e = p5 st in let f = p6 st in let x7 = p7 st in let x8 = p8 st in
  let x9 = p9 st in let x10 = p10 st in let x11 = p11 st in let x12 = p12 st in let x13 = p13 st in
  (a,b,c,d,e,f,x7,x8,x9,x10,x11,x12,x13)
let inline utup14 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 p12 p13 p14 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in
  let e = p5 st in let f = p6 st in let x7 = p7 st in let x8 = p8 st in
  let x9 = p9 st in let x10 = p10 st in let x11 = p11 st in let x12 = p12 st in let x13 = p13 st in
  let x14 = p14 st in
  (a,b,c,d,e,f,x7,x8,x9,x10,x11,x12,x13,x14)
let inline utup15 p1 p2 p3 p4 p5 p6 p7 p8 p9 p10 p11 p12 p13 p14 p15 (st:ReaderState) =
  let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in
  let e = p5 st in let f = p6 st in let x7 = p7 st in let x8 = p8 st in
  let x9 = p9 st in let x10 = p10 st in let x11 = p11 st in let x12 = p12 st in let x13 = p13 st in
  let x14 = p14 st in let x15 = p15 st in
  (a,b,c,d,e,f,x7,x8,x9,x10,x11,x12,x13,x14,x15)


(*---------------------------------------------------------------------------
 * Pickle/unpickle operations for observably shared graph nodes
 *------------------------------------------------------------------------- *)

(* exception Nope *)

(* ctxt is for debugging *)
#if INCLUDE_METADATA_WRITER
let p_osgn_ref (ctxt:string) (ObservableNodeOutMap(stampF,nameF,rangeF,derefF,nm,keyTable)) x st = 
    let idx = find_or_add_entry keyTable (stampF x)
    //if ((idx >= 1387 && idx <= 1388) && nm = "ovals") then 
    //    dprintf "idx %d#%d in table %s has name '%s', was defined at '%s' and is referenced from context %s\n" idx (stampF x) nm (nameF x) (string_of_range (rangeF x)) ctxt; 
    //    System.Diagnostics.Debug.Assert(false)
    p_int idx st

let p_osgn_decl (ObservableNodeOutMap(stampF,nameF,rangeF,derefF,nm,keyTable)) p x st = 
    let stamp = stampF x
    let idx = find_or_add_entry keyTable stamp
    (* dprintf "decl %d#%d in table %s has name %s\n" idx (stampF x) nm (nameF x);  *)
    p_tup2 p_int p (idx,derefF x) st
#endif

let u_osgn_ref (ObservableNodeInMap(lnk,isLinked,nm,arr)) st = 
    let n = u_int st
    if n < 0 or n >= Array.length arr then ufailwith st ("u_osgn_ref: out of range, table = "^nm^", n = "^string n); 
    arr.[n]

let u_osgn_decl (ObservableNodeInMap(lnk,isLinked,nm,arr)) u st = 
    let idx,data = u_tup2 u_int u st
  (*   dprintf "unpickling osgn %d in table %s\n" idx nm; *)
    let res = arr.[idx]
    lnk res data;
    res

(*---------------------------------------------------------------------------
 * Pickle/unpickle operations for interned nodes in the term DAG
 *------------------------------------------------------------------------- *)

let encode_uniq tbl key = find_or_add_entry tbl key
let lookup_uniq st tbl n = 
  let arr = tbl.itbl_rows
  if n < 0 or n >= Array.length arr then ufailwith st ("lookup_uniq in table "^tbl.itbl_name^" out of range, n = "^string n^ ", sizeof(tab) = " ^ string (Array.length arr)); 
  arr.[n]

//---------------------------------------------------------------------------
// Pickle/unpickle arrays and lists. For lists use the same binary format as arrays so we can switch
// between internal representations relatively easily
//------------------------------------------------------------------------- 
 
#if INCLUDE_METADATA_WRITER
let p_array f (x: 'a[]) st =
    p_int x.Length st;
    for i = 0 to x.Length-1 do
        f x.[i] st

 
let p_list f x st = p_array f (Array.of_list x) st


#if FLAT_LIST_AS_LIST
#else
let p_FlatList f (x: FlatList<'a>) st = p_list f x st // p_array f (match x.array with null -> [| |] | _ -> x.array) st
#endif
#if FLAT_LIST_AS_ARRAY_STRUCT
let p_FlatList f (x: FlatList<'a>) st = p_array f (match x.array with null -> [| |] | _ -> x.array) st
#endif
#if FLAT_LIST_AS_ARRAY
let p_FlatList f (x: FlatList<'a>) st = p_array f x st
#endif

let p_wrap (f: 'a -> 'b) (p : 'b pickler) : 'a pickler = (fun x st -> p (f x) st)
let p_option f x st =
    match x with 
    | None -> p_byte 0 st
    | Some h -> p_byte 1 st; f h st

(*
let p_lazy p x st = 
    p (Lazy.force x) st
*)
// This is an attempt to pickle lazy values in such a way that they can be read back
// lazily. However, it doesn't work because the value may contain the definitions of some
// OSGN nodes. We could record these nodes as we find them and somehow mark them as "lazy tripwire" 
// OSGN node s when they are reconstituted, i.e. ones where the lazy value is forced before the 
// OSGN node is dereferenced.
//
//
let p_lazy p x st = 
    let v = Lazy.force x
    let fixupPos1 = Bytes.Bytebuf.position st.os
    //// We fix these up after
    prim_p_int32 0 st;
    let fixupPos2 = Bytes.Bytebuf.position st.os
    prim_p_int32 0 st;
    let fixupPos3 = Bytes.Bytebuf.position st.os
    prim_p_int32 0 st;
    let fixupPos4 = Bytes.Bytebuf.position st.os
    prim_p_int32 0 st;
    let fixupPos5 = Bytes.Bytebuf.position st.os
    prim_p_int32 0 st;
    let fixupPos6 = Bytes.Bytebuf.position st.os
    prim_p_int32 0 st;
    let fixupPos7 = Bytes.Bytebuf.position st.os
    prim_p_int32 0 st;
    let idx1 = Bytes.Bytebuf.position st.os
    let otyconsIdx1 = osgn_outmap_size st.otycons
    let otyparsIdx1 = osgn_outmap_size st.otypars
    let ovalsIdx1 = osgn_outmap_size st.ovals
    // Run the pickler
    p v st;
    // Determine and fixup the length of the pickled data
    let idx2 = Bytes.Bytebuf.position st.os
    Bytes.Bytebuf.fixup_i32 st.os fixupPos1 (idx2-idx1);
    // Determine and fixup the ranges of OSGN nodes defined within the lazy portion
    let otyconsIdx2 = osgn_outmap_size st.otycons
    let otyparsIdx2 = osgn_outmap_size st.otypars
    let ovalsIdx2 = osgn_outmap_size st.ovals
    Bytes.Bytebuf.fixup_i32 st.os fixupPos2 otyconsIdx1;
    Bytes.Bytebuf.fixup_i32 st.os fixupPos3 otyconsIdx2;
    Bytes.Bytebuf.fixup_i32 st.os fixupPos4 otyparsIdx1;
    Bytes.Bytebuf.fixup_i32 st.os fixupPos5 otyparsIdx2;
    Bytes.Bytebuf.fixup_i32 st.os fixupPos6 ovalsIdx1;
    Bytes.Bytebuf.fixup_i32 st.os fixupPos7 ovalsIdx2

let p_hole () = 
    let h = ref (None : 'a pickler option)
    (fun f -> h := Some f),(fun x st -> match !h with Some f -> f x st | None -> pfailwith st "p_hole: unfilled hole")


#endif

let u_array f st =
    let n = u_int st
    let res = Array.zeroCreate n
    for i = 0 to n-1 do
        res.[i] <- f st
    res

let u_list f st = Array.to_list (u_array f st)

#if FLAT_LIST_AS_LIST
#else
let u_FlatList f st = u_list f st // new FlatList<_> (u_array f st)
#endif
#if FLAT_LIST_AS_ARRAY_STRUCT
let u_FlatList f st = FlatList(u_array f st)
#endif
#if FLAT_LIST_AS_ARRAY
let u_FlatList f st = u_array f st
#endif

let u_array_revi f st =
    let n = u_int st
    let res = Array.zeroCreate n
    for i = 0 to n-1 do
        res.[i] <- f st (n-1-i) 
    res

(* Mark up default constraints with a priority in reverse order: last gets 0 etc. See comment on TTyparDefaultsToType *)
let u_list_revi f st = Array.to_list (u_array_revi f st)
 
 
let u_wrap (f: 'b -> 'a) (u : 'b unpickler) : 'a unpickler = (fun st -> f (u st))


let u_option f st = 
    let tag = u_byte st
    match tag with
    | 0 -> None
    | 1 -> Some (f st)
    | n -> ufailwith st ("u_option: found number " ^ string n)

(*

let u_lazy u st = 
    Lazy.CreateFromValue (u st)
*)


// Boobytrap an OSGN node with a force of a lazy load of a bunch of pickled data
#if LAZY_UNPICKLE
let wire (x:osgn<_>) (res:Lazy<_>) = 
    x.osgnTripWire <- Some(fun () -> res.Force() |> ignore)
#endif

let u_lazy u st = 

    // Read the number of bytes in the record
    let len         = prim_u_int32 st // fixupPos1
    // These are the ranges of OSGN nodes defined within the lazily read portion of the graph
    let otyconsIdx1 = prim_u_int32 st // fixupPos2
    let otyconsIdx2 = prim_u_int32 st // fixupPos3
    let otyparsIdx1 = prim_u_int32 st // fixupPos4
    let otyparsIdx2 = prim_u_int32 st // fixupPos5
    let ovalsIdx1   = prim_u_int32 st // fixupPos6
    let ovalsIdx2   = prim_u_int32 st // fixupPos7

#if LAZY_UNPICKLE
    // Record the position in the bytestream to use when forcing the read of the data
    let idx1 = Bytes.Bytestream.position st.is
    // Skip the length of data
    Bytes.Bytestream.skip st.is len;
    // This is the lazy computation that wil force the unpickling of the term.
    // This term must contain OSGN definitions of the given nodes.
    let res = 
        lazy (let st = { st with is = Bytes.Bytestream.clone_and_seek st.is idx1 }
              u st)
    /// Force the reading of the data as a "tripwire" for each of the OSGN thunks 
    for i = otyconsIdx1 to otyconsIdx2-1 do wire (st.itycons.Get(i)) res done;
    for i = ovalsIdx1   to ovalsIdx2-1   do wire (st.ivals.Get(i))   res done;
    for i = otyparsIdx1 to otyparsIdx2-1 do wire (st.itypars.Get(i)) res done;
    res
#else
    Lazy.CreateFromValue(u st)
#endif    


let u_hole () = 
    let h = ref (None : 'a unpickler option)
    (fun f -> h := Some f),(fun st -> match !h with Some f -> f st | None -> ufailwith st "u_hole: unfilled hole")

(*---------------------------------------------------------------------------
 * Pickle/unpickle F# interface data 
 *------------------------------------------------------------------------- *)

// Strings 
// A huge number of these occur in pickled F# data, so make them unique 
let encode_string stringTab x = encode_uniq stringTab x
let decode_string x = x
let lookup_string st stringTab x = lookup_uniq st stringTab x
let u_encoded_string = u_prim_string
let u_string st   = lookup_uniq st st.istrings (u_int st)
let u_strings = u_list u_string
let u_ints = u_list u_int


#if INCLUDE_METADATA_WRITER
let p_encoded_string = p_prim_string
let p_string s st = p_int (encode_string st.ostrings s) st
let p_strings = p_list p_string
let p_ints = p_list p_int
#endif

// CCU References 
// A huge number of these occur in pickled F# data, so make them unique 
let encode_ccuref ccuTab (x:CcuThunk) = encode_uniq ccuTab x.AssemblyName 
let decode_ccuref x = x
let lookup_ccuref st ccuTab x = lookup_uniq st ccuTab x
let u_encoded_ccuref = u_prim_string
let u_ccuref st   = lookup_uniq st st.iccus (u_int st)

#if INCLUDE_METADATA_WRITER
let p_encoded_ccuref = p_prim_string
let p_ccuref s st = p_int (encode_ccuref st.occus s) st
#endif

// References to public items in this module 
// A huge number of these occur in pickled F# data, so make them unique 
let decode_pubpath st stringTab (a,b) = PubPath(Array.map (lookup_string st stringTab) a, lookup_string st stringTab b)
let lookup_pubpath st pubpathTab x = lookup_uniq st pubpathTab x
let u_encoded_pubpath = u_tup2 (u_array u_int) u_int
let u_pubpath st = lookup_uniq st st.ipubpaths (u_int st)

#if INCLUDE_METADATA_WRITER
let encode_pubpath stringTab pubpathTab (PubPath(a,b)) = encode_uniq pubpathTab (Array.map (encode_string stringTab) a, encode_string stringTab b)
let p_encoded_pubpath = p_tup2 (p_array p_int) p_int
let p_pubpath x st = p_int (encode_pubpath st.ostrings st.opubpaths x) st
#endif

// References to other modules 
// A huge number of these occur in pickled F# data, so make them unique 
let decode_nlpath st ccuTab stringTab (a,b) = NLPath(lookup_ccuref st ccuTab a, Array.map (lookup_string st stringTab) b)
let lookup_nlpath st nlpathTab x = lookup_uniq st nlpathTab x
let u_encoded_nlpath = u_tup2 u_int (u_array u_int)
let u_nlpath st = lookup_uniq st st.inlpaths (u_int st)

#if INCLUDE_METADATA_WRITER
let encode_nlpath ccuTab stringTab nlpathTab (NLPath(a,b)) = encode_uniq nlpathTab (encode_ccuref ccuTab a, Array.map (encode_string stringTab) b)
let p_encoded_nlpath = p_tup2 p_int (p_array p_int)
let p_nlpath x st = p_int (encode_nlpath st.occus st.ostrings st.onlpaths x) st
#endif

// Simple types are types like "int", represented as TType(Ref_nonlocal(...,"int"),[]). 
// A huge number of these occur in pickled F# data, so make them unique. 
let decode_simpletyp st ccuTab stringTab nlpathTab (a,b) = TType_app(mk_nonlocal_tcref (lookup_nlpath st nlpathTab a) (lookup_string st stringTab b),[])
let lookup_simpletyp st simpletypTab x = lookup_uniq st simpletypTab x
let u_encoded_simpletyp = u_tup2 u_int u_int
let u_simpletyp st = lookup_uniq st st.isimpletyps (u_int st)
#if INCLUDE_METADATA_WRITER
let encode_simpletyp ccuTab stringTab nlpathTab simpletypTab (a,b) = encode_uniq simpletypTab (encode_nlpath ccuTab stringTab nlpathTab a, encode_string stringTab b)
let p_encoded_simpletyp = p_tup2 p_int p_int
let p_simpletyp x st = p_int (encode_simpletyp st.occus st.ostrings st.onlpaths st.osimpletyps x) st
#endif

type sizes = int * int * int 
#if INCLUDE_METADATA_WRITER
let pickle_obj_with_dangling_ccus file g scope p x =
  let ccuNameTab,(sizes: sizes),stringTab,pubpathTab,nlpathTab,simpletypTab,phase1bytes =
    let st1 = 
      { os = Bytes.Bytebuf.create 100000; 
        oscope=scope;
        occus= new_tbl "occus"; 
        otycons=new_osgn_outmap (fun (tc:Tycon) -> tc.Stamp) (fun tc -> tc.MangledName) (fun tc -> tc.Range) (fun osgn -> osgn.Data) "otycons"; 
        otypars=new_osgn_outmap (fun (tp:Typar) -> tp.Stamp) (fun tp -> tp.DisplayName) (fun tp -> tp.Range) (fun osgn -> osgn.Data) "otypars"; 
        ovals=new_osgn_outmap (fun (v:Val) -> v.Stamp) (fun v -> v.MangledName) (fun v -> v.Range) (fun osgn -> osgn.Data)  "ovals";
        ostrings=new_tbl "ostrings";
        onlpaths=new_tbl "onlpaths";  
        opubpaths=new_tbl "opubpaths";  
        osimpletyps=new_tbl "osimpletyps";  
        oglobals=g;
        ofile=file;
        (* REINSTATE: odecomps=new_osgn_outmap stamp_of_decomp name_of_decomp "odecomps"; *) }
    p x st1;
    let sizes = 
      osgn_outmap_size st1.otycons,
      osgn_outmap_size st1.otypars,
      osgn_outmap_size st1.ovals 
    st1.occus, sizes, st1.ostrings, st1.opubpaths,st1.onlpaths, st1.osimpletyps, Bytes.Bytebuf.close st1.os
  let phase2data = (get_tbl ccuNameTab,sizes,get_tbl stringTab,get_tbl pubpathTab,get_tbl nlpathTab,get_tbl simpletypTab,phase1bytes)
  let phase2bytes = 
    let st2 = 
     { os = Bytes.Bytebuf.create 100000; 
       oscope=scope;
       occus= new_tbl "occus (fake)"; 
       otycons=new_osgn_outmap (fun (tc:Tycon) -> tc.Stamp) (fun tc -> tc.MangledName) (fun tc -> tc.Range) (fun osgn -> osgn.Data) "otycons"; 
       otypars=new_osgn_outmap (fun (tp:Typar) -> tp.Stamp) (fun tp -> tp.DisplayName) (fun tp -> tp.Range) (fun osgn -> osgn.Data) "otypars"; 
       ovals=new_osgn_outmap (fun (v:Val) -> v.Stamp) (fun v -> v.MangledName) (fun v -> v.Range) (fun osgn -> osgn.Data)  "ovals";
       ostrings=new_tbl "ostrings (fake)";
       opubpaths=new_tbl "opubpaths (fake)";
       onlpaths=new_tbl "onlpaths (fake)";
       osimpletyps=new_tbl "osimpletyps (fake)";
       oglobals=g;
       ofile=file; }
    ptup7
      (p_array p_encoded_ccuref) 
      (p_tup3 p_int p_int p_int) 
      (p_array p_encoded_string) 
      (p_array p_encoded_pubpath) 
      (p_array p_encoded_nlpath) 
      (p_array p_encoded_simpletyp) 
      p_bytes 
      phase2data st2;
    Bytes.Bytebuf.close st2.os
  phase2bytes
  
#endif
    
#if CHECKED
let check (ilscope:ILScopeRef) (ObservableNodeInMap(lnk,isLinked,nm,arr)) =
    for i = 0 to arr.Length-1 do
      let n = arr.[i]
      if not (isLinked n) then 
        printf "*** unpickle: osgn %d in table %s with IL scope %s had no matching declaration (was not fixed up)\nPlease report this warning. (Note for compiler developers: to get information about which item this index relates to, enable the conditional in Pickle.p_osgn_ref to refer to the given index number and recompile an identical copy of the source for the DLL containing the data being unpickled.  A message will then be printed indicating the name of the item.\n" i nm ilscope.QualifiedName
#endif

let unpickle_obj_with_dangling_ccus file ilscope u (phase2bytes:byte[]) =
    let st2 = 
       { is = Bytes.Bytestream.of_bytes phase2bytes 0 phase2bytes.Length; 
         iilscope= ilscope;
         iccus= new_itbl "iccus (fake)" [| |]; 
         itycons= new_osgn_inmap Tycon.NewUnlinked (fun osgn tg -> osgn.Link(tg)) (fun osgn -> osgn.IsLinked) "itycons" 0; 
         itypars= new_osgn_inmap Typar.NewUnlinked (fun osgn tg -> osgn.Link(tg)) (fun osgn -> osgn.IsLinked) "itypars" 0; 
         ivals  = new_osgn_inmap Val.NewUnlinked   (fun osgn tg -> osgn.Link(tg)) (fun osgn -> osgn.IsLinked) "ivals" 0;
         istrings = new_itbl "istrings (fake)" [| |]; 
         inlpaths = new_itbl "inlpaths (fake)" [| |]; 
         ipubpaths = new_itbl "ipubpaths (fake)" [| |]; 
         isimpletyps = new_itbl "isimpletyps (fake)" [| |]; 
         ifile=file }
    let phase2data = 
        utup7
           (u_array u_encoded_ccuref) 
           (u_tup3 u_int u_int u_int) 
           (u_array u_encoded_string) 
           (u_array u_encoded_pubpath) 
           (u_array u_encoded_nlpath) 
           (u_array u_encoded_simpletyp) 
           u_bytes st2
    let ccuNameTab,sizes,stringTab,pubpathTab,nlpathTab,simpletypTab,phase1bytes = phase2data
    let ccuTab       = new_itbl "iccus"       (Array.map (CcuThunk.CreateDelayed) ccuNameTab)
    let stringTab    = new_itbl "istrings"    (Array.map decode_string stringTab)
    let pubpathTab   = new_itbl "ipubpaths"   (Array.map (decode_pubpath st2 stringTab) pubpathTab)
    let nlpathTab    = new_itbl "inlpaths"    (Array.map (decode_nlpath st2 ccuTab stringTab) nlpathTab)
    let simpletypTab = new_itbl "isimpletyps" (Array.map (decode_simpletyp st2 ccuTab stringTab nlpathTab) simpletypTab)
    let ((ntycons,ntypars,nvals) : sizes) = sizes
    let data = 
        let st1 = 
           { is = Bytes.Bytestream.of_bytes phase1bytes 0 phase1bytes.Length; 
             iccus=  ccuTab; 
             iilscope= ilscope;
             itycons= new_osgn_inmap Tycon.NewUnlinked (fun osgn tg -> osgn.Link(tg)) (fun osgn -> osgn.IsLinked)  "itycons" ntycons; 
             itypars= new_osgn_inmap Typar.NewUnlinked (fun osgn tg -> osgn.Link(tg)) (fun osgn -> osgn.IsLinked) "itypars" ntypars; 
             ivals=   new_osgn_inmap Val.NewUnlinked   (fun osgn tg -> osgn.Link(tg)) (fun osgn -> osgn.IsLinked) "ivals" nvals;
             istrings = stringTab;
             ipubpaths = pubpathTab;
             inlpaths = nlpathTab;
             isimpletyps = simpletypTab;
             ifile=file }
        let res = u st1
#if CHECKED
#if LAZY_UNPICKLE
#else
        check ilscope st1.itycons;
        check ilscope st1.ivals;
        check ilscope st1.itypars;
#endif
#endif
        res

    {RawData=data; FixupThunks=Array.to_list ccuTab.itbl_rows }
    

(*=========================================================================*)
(* PART II *)
(*=========================================================================*)

(*---------------------------------------------------------------------------
 * Pickle/unpickle for Abstract IL data, up to IL instructions 
 *------------------------------------------------------------------------- *)

#if INCLUDE_METADATA_WRITER
let p_pubkey x st = 
    match x with 
    | PublicKey b      -> p_byte 0 st; p_bytes b st
    | PublicKeyToken b -> p_byte 1 st; p_bytes b st
let p_version x st = p_tup4 p_uint16 p_uint16 p_uint16 p_uint16 x st
let p_modref (x:ILModuleRef) st = 
    p_tup3 p_string p_bool (p_option p_bytes) (x.Name,x.HasMetadata,x.Hash) st
let p_assref (x:ILAssemblyRef) st =
  p_tup6 p_string (p_option p_bytes) (p_option p_pubkey) p_bool (p_option p_version) (p_option p_string)
    ( x.Name,x.Hash,x.PublicKey,x.Retargetable,x.Version,x.Locale) st
let p_scoref x st = 
  match x with 
  | ScopeRef_local         -> p_byte 0 st; p_void st
  | ScopeRef_module mref   -> p_byte 1 st; p_modref mref st
  | ScopeRef_assembly aref -> p_byte 2 st; p_assref aref st
#endif

let u_pubkey st = 
    let tag = u_byte st
    match tag with
    | 0 -> u_bytes st |> (fun b -> PublicKey b) 
    | 1 -> u_bytes st |> (fun b -> PublicKeyToken b) 
    | _ -> ufailwith st "u_pubkey"

let u_version st = u_tup4 u_uint16 u_uint16 u_uint16 u_uint16 st

let u_modref st = 
    let (a,b,c) = u_tup3 u_string u_bool (u_option u_bytes) st
    ILModuleRef.Create(a, b, c)

let u_assref st =
  let a,b,c,d,e,f = u_tup6 u_string (u_option u_bytes) (u_option u_pubkey) u_bool (u_option u_version) (u_option u_string) st
  ILAssemblyRef.Create(a, b, c, d, e, f)

// IL scope references are rescoped as they are unpickled.  This means 
// the pickler accepts IL fragments containing ScopeRef_local, as we adjust 
// these to be absolute scope references during unpickling.  
let u_scoref st = 
  let res = 
    let tag = u_byte st
    match tag with
    | 0 -> u_void   st |> (fun () -> ScopeRef_local)
    | 1 -> u_modref st |> (fun mref -> ScopeRef_module mref)
    | 2 -> u_assref st |> (fun aref -> ScopeRef_assembly aref)
    | _ -> ufailwith st "u_scoref"  
  let res = rescope_scoref st.iilscope res 
  res

#if INCLUDE_METADATA_WRITER
let p_hasthis x st = 
  p_byte (match x with 
          | CC_instance -> 0
          | CC_instance_explicit -> 1
          | CC_static -> 2) st
let p_array_shape = p_wrap (fun (ILArrayShape x) -> x) (p_list (p_tup2 (p_option p_int32) (p_option p_int32)))
let fill_p_iltyp,p_iltyp = p_hole()
let p_iltyps = (p_list p_iltyp)
let p_basic_callconv x st = 
  p_byte (match x with 
          | CC_default -> 0
          | CC_cdecl  -> 1
          | CC_stdcall -> 2
          | CC_thiscall -> 3
          | CC_fastcall -> 4
          | CC_vararg -> 5) st
let p_callconv (Callconv(x,y)) st = p_tup2 p_hasthis p_basic_callconv (x,y) st
let p_callsig = p_wrap (fun x -> (x.callsigCallconv,x.callsigArgs,x.callsigReturn)) (p_tup3 p_callconv p_iltyps p_iltyp)
let p_iltref (x:ILTypeRef) st = p_tup3 p_scoref p_strings p_string (x.Scope,x.Enclosing,x.Name) st
let p_iltspec (a:ILTypeSpec) st = p_tup2 p_iltref p_iltyps (a.TypeRef,a.GenericArgs) st
let _ = fill_p_iltyp (fun ty st ->
  match ty with 
  | Type_void             -> p_byte 0 st; p_void st
  | Type_array (shape,ty) -> p_byte 1 st; p_tup2 p_array_shape p_iltyp (shape,ty) st
  | Type_value tspec      -> p_byte 2 st; p_iltspec tspec st
  | Type_boxed tspec      -> p_byte 3 st; p_iltspec tspec st
  | Type_ptr ty           -> p_byte 4 st; p_iltyp ty st
  | Type_byref ty         -> p_byte 5 st; p_iltyp ty st
  | Type_fptr csig        -> p_byte 6 st; p_callsig csig st
  | Type_tyvar n          -> p_byte 7 st; p_uint16 n st
  | Type_modified (req,tref,ty) -> p_byte 8 st; p_tup3 p_bool p_iltref p_iltyp (req,tref,ty) st)
#endif
let u_basic_callconv st = 
  match u_byte st with 
  | 0 -> CC_default 
  | 1 -> CC_cdecl  
  | 2 -> CC_stdcall 
  | 3 -> CC_thiscall 
  | 4 -> CC_fastcall 
  | 5 -> CC_vararg
  | _ -> ufailwith st "u_basic_callconv"

let u_hasthis st = 
  match u_byte st with 
  | 0 -> CC_instance 
  | 1 -> CC_instance_explicit 
  | 2 -> CC_static 
  | _ -> ufailwith st "u_hasthis"

let u_callconv st = let a,b = u_tup2 u_hasthis u_basic_callconv st in Callconv(a,b)
let u_iltref st = let a,b,c = u_tup3 u_scoref u_strings u_string st in ILTypeRef.Create(a, b, c) 
let u_array_shape = u_wrap (fun x -> ILArrayShape x) (u_list (u_tup2 (u_option u_int32) (u_option u_int32)))


let fill_u_iltyp,u_iltyp = u_hole()
let u_iltyps = (u_list u_iltyp)
let u_callsig = u_wrap (fun (a,b,c) -> {callsigCallconv=a; callsigArgs=b; callsigReturn=c}) (u_tup3 u_callconv u_iltyps u_iltyp)
let u_iltspec st = let a,b = u_tup2 u_iltref u_iltyps st in ILTypeSpec.Create(a,b)

let _ = fill_u_iltyp (fun st ->
  let tag = u_byte st
  match tag with
  | 0 -> u_void st |> (fun () -> Type_void)
  | 1 -> u_tup2 u_array_shape u_iltyp  st |> (fun (arr,ty) -> Type_array (arr,ty))
  | 2 -> u_iltspec st |> (fun x -> Type_value x)
  | 3 -> u_iltspec st |> (fun x -> Type_boxed x)
  | 4 -> u_iltyp st |> (fun x -> Type_ptr x)
  | 5 -> u_iltyp st |> (fun x -> Type_byref x)
  | 6 -> u_callsig st |> (fun x -> Type_fptr x)
  | 7 -> u_uint16 st |> (fun x -> Type_tyvar x)
  | 8 -> u_tup3 u_bool u_iltref u_iltyp  st |> (fun (req,tref,ty) -> Type_modified (req,tref,ty))
  | 9 -> u_tup2 u_array_shape u_iltyp  st |> (fun (shape,ty) -> Type_array (shape,ty))
  | _ -> ufailwith st "u_iltyp")


#if INCLUDE_METADATA_WRITER
let p_ilmref (x:ILMethodRef) st = p_tup6 p_iltref p_callconv p_int p_string p_iltyps p_iltyp (x.EnclosingTypeRef,x.CallingConv,x.GenericArity,x.Name,x.ArgTypes,x.ReturnType) st
let p_ilfref x st = p_tup3 p_iltref p_string p_iltyp (x.frefParent,x.frefName,x.frefType) st
let pilmspec x st = p_tup3 p_ilmref p_iltyp p_iltyps (dest_mspec x) st
let pilfspec x st = p_tup2 p_ilfref p_iltyp (x.fspecFieldRef,x.fspecEnclosingType) st
let pbasic_type x st = p_int (match x with DT_R -> 0 | DT_I1 -> 1 | DT_U1 -> 2 | DT_I2 -> 3 | DT_U2 -> 4 | DT_I4 -> 5 | DT_U4 -> 6 | DT_I8 -> 7 | DT_U8 -> 8 | DT_R4 -> 9 | DT_R8 -> 10 | DT_I -> 11 | DT_U -> 12 | DT_REF -> 13) st
let p_ldtoken_info x st = 
  match x with 
  | Token_type ty -> p_byte 0 st; p_iltyp ty st
  | Token_method x -> p_byte 1 st; pilmspec x st
  | Token_field x -> p_byte 2 st; pilfspec x st
let p_alignment x st = p_int (match x with Aligned -> 0 | Unaligned_1 -> 1 | Unaligned_2 -> 2 | Unaligned_4 -> 3) st
let p_volatility x st = p_int (match x with Volatile -> 0 | Nonvolatile -> 1) st
let p_readonly x st = p_int (match x with ReadonlyAddress -> 0 | NormalAddress -> 1) st
let p_tailness x st = p_int (match x with Tailcall -> 0 | Normalcall -> 1) st
let p_varargs = p_option p_iltyps
let p_ldc_info x st = 
  match x with 
  | NUM_I4 x -> p_byte 0 st; p_int32 x st
  | NUM_I8 x -> p_byte 1 st; p_int64 x st
  | NUM_R4 x -> p_byte 2 st; p_single x st
  | NUM_R8 x -> p_byte 3 st; p_ieee64 x st

#endif

let u_ilmref st = 
    let x1,x2,x3,x4,x5,x6 = u_tup6 u_iltref u_callconv u_int u_string u_iltyps u_iltyp st
    ILMethodRef.Create(x1,x2,x4,x3,x5,x6)

let u_ilfref st = let x1,x2,x3 = u_tup3 u_iltref u_string u_iltyp st in {frefParent=x1;frefName=x2;frefType=x3}

let uilmspec st = let x1,x2,x3 = u_tup3 u_ilmref u_iltyp u_iltyps st in ILMethodSpec.Create(x2,x1,x3)
let uilfspec st = let x1,x2 = u_tup2 u_ilfref u_iltyp st in {fspecFieldRef=x1;fspecEnclosingType=x2}

let ubasic_type st = (match u_int st with  0 -> DT_R | 1 -> DT_I1 | 2 -> DT_U1 | 3 -> DT_I2 | 4 -> DT_U2 | 5 -> DT_I4 | 6 -> DT_U4 | 7 -> DT_I8 | 8 -> DT_U8 | 9 -> DT_R4 | 10 -> DT_R8 | 11 -> DT_I | 12 -> DT_U | 13 -> DT_REF | _ -> ufailwith st "ubasic_type" )
  
let u_ldtoken_info st = 
  let tag = u_byte st
  match tag with
  | 0 -> u_iltyp st |> (fun x -> Token_type x)
  | 1 -> uilmspec st |> (fun x -> Token_method x)
  | 2 -> uilfspec st |> (fun x -> Token_field x)
  | _ -> ufailwith st "u_ldtoken_info"
  
let u_ldc_info st = 
  let tag = u_byte st
  match tag with
  | 0 -> u_int32 st |> (fun x -> NUM_I4 x)
  | 1 -> u_int64 st |> (fun x -> NUM_I8 x)
  | 2 -> u_single st |> (fun x -> NUM_R4 x)
  | 3 -> u_ieee64 st |> (fun x -> NUM_R8 x)
  | _ -> ufailwith st "u_ldtoken_info"
  
let u_alignment st = (match u_int st with  0 -> Aligned | 1 -> Unaligned_1 | 2 -> Unaligned_2 | 3 -> Unaligned_4 | _ -> ufailwith st "u_alignment" )
let u_volatility st = (match u_int st with  0 -> Volatile | 1 -> Nonvolatile | _ -> ufailwith st "u_volatility" )
let u_readonly st = (match u_int st with  0 -> ReadonlyAddress | 1 -> NormalAddress | _ -> ufailwith st "u_readonly" )
let u_tailness st = (match u_int st with  0 -> Tailcall | 1 -> Normalcall | _ -> ufailwith st "u_tailness" )
let u_varargs = u_option u_iltyps
  
let itag_nop         = 0 
let itag_break       = 1 
let itag_ldarg       = 2 
let itag_ldloc       = 3
let itag_stloc       = 4 
let itag_ldnull      = 5 
let itag_ldc         = 6 
let itag_dup           = 7 
let itag_pop           = 8 
let itag_jmp           = 9 
let itag_call          = 10 
let itag_calli         = 11 
let itag_ret           = 12 
let itag_br            = 13 
let itag_brfalse       = 14 
let itag_brtrue        = 15 
let itag_beq           = 16 
let itag_bge           = 17 
let itag_bgt           = 18 
let itag_ble           = 19 
let itag_blt           = 20 
let itag_bne_un        = 21 
let itag_bge_un        = 22 
let itag_bgt_un        = 23 
let itag_ble_un        = 24 
let itag_blt_un        = 25 
let itag_switch        = 26 
let itag_ldind         = 27
let itag_stind         = 28
let itag_add           = 29
let itag_sub           = 30 
let itag_mul           = 31
let itag_div           = 32 
let itag_div_un        = 33 
let itag_rem           = 34 
let itag_rem_un        = 35 
let itag_and           = 36 
let itag_or            = 37 
let itag_xor           = 38 
let itag_shl           = 39 
let itag_shr           = 40 
let itag_shr_un        = 41 
let itag_neg           = 42 
let itag_not           = 43 
let itag_conv       = 44
let itag_conv_un     = 45 
let itag_conv_ovf   = 46
let itag_conv_ovf_un   = 47
let itag_callvirt      = 48 
let itag_cpobj         = 49 
let itag_ldobj         = 50 
let itag_ldstr         = 51 
let itag_newobj        = 52 
let itag_castclass     = 53 
let itag_isinst        = 54 
let itag_unbox         = 55 
let itag_throw         = 56 
let itag_ldfld         = 57 
let itag_ldflda        = 58 
let itag_stfld         = 59 
let itag_ldsfld        = 60 
let itag_ldsflda       = 61 
let itag_stsfld        = 62 
let itag_stobj         = 63 
let itag_box           = 64 
let itag_newarr        = 65 
let itag_ldlen         = 66 
let itag_ldelema       = 67 
let itag_ldelem     = 68
let itag_stelem      = 69 
let itag_refanyval     = 70 
let itag_ckfinite      = 71 
let itag_mkrefany      = 72 
let itag_ldtoken       = 73 
let itag_add_ovf       = 74 
let itag_add_ovf_un    = 75 
let itag_mul_ovf       = 76 
let itag_mul_ovf_un    = 77 
let itag_sub_ovf       = 78 
let itag_sub_ovf_un    = 79 
let itag_endfinally    = 80 
let itag_leave         = 81 
let itag_arglist        = 82
let itag_ceq        = 83
let itag_cgt        = 84
let itag_cgt_un        = 85
let itag_clt        = 86
let itag_clt_un        = 87
let itag_ldftn        = 88 
let itag_ldvirtftn    = 89 
let itag_ldarga      = 90 
let itag_starg       = 91 
let itag_ldloca      = 92 
let itag_localloc     = 93 
let itag_endfilter    = 94 
let itag_unaligned   = 95 
let itag_volatile    = 96 
let itag_constrained    = 97
let itag_readonly    = 98
let itag_tail        = 99 
let itag_initobj             = 100
let itag_cpblk       = 101
let itag_initblk             = 102
let itag_rethrow             = 103 
let itag_sizeof      = 104
let itag_refanytype   = 105
let itag_ldelem_any = 106
let itag_stelem_any = 107
let itag_unbox_any = 108
let itag_ldunit = 109
let itag_ldlen_multi = 113
let itag_callconstrained      = 114 
let itag_ilzero = 115

let simple_instrs = 
 [
  itag_ret,              (I_ret);
  itag_add,              (I_arith AI_add);
  itag_add_ovf,        (I_arith AI_add_ovf);
  itag_add_ovf_un,   (I_arith AI_add_ovf_un);
  itag_and,              (I_arith AI_and);  
  itag_div,              (I_arith AI_div); 
  itag_div_un,         (I_arith AI_div_un);
  itag_ceq,              (I_arith AI_ceq);  
  itag_cgt,              (I_arith AI_cgt );
  itag_cgt_un,         (I_arith AI_cgt_un);
  itag_clt,              (I_arith AI_clt);
  itag_clt_un,         (I_arith AI_clt_un);
  itag_mul,   (I_arith AI_mul  );
  itag_mul_ovf,   (I_arith AI_mul_ovf);
  itag_mul_ovf_un,   (I_arith AI_mul_ovf_un);
  itag_rem,   (I_arith AI_rem  );
  itag_rem_un,   (I_arith AI_rem_un ); 
  itag_shl,   (I_arith AI_shl ); 
  itag_shr,   (I_arith AI_shr ); 
  itag_shr_un,   (I_arith AI_shr_un);
  itag_sub,   (I_arith AI_sub  );
  itag_sub_ovf,   (I_arith AI_sub_ovf);
  itag_sub_ovf_un,   (I_arith AI_sub_ovf_un); 
  itag_xor,   (I_arith AI_xor);  
  itag_or,   (I_arith AI_or);     
  itag_neg,   (I_arith AI_neg);     
  itag_not,   (I_arith AI_not);     
  itag_ldnull,   (I_arith AI_ldnull);   
  itag_dup,   (I_arith AI_dup);   
  itag_pop,   (I_arith AI_pop);
  itag_ckfinite,   (I_arith AI_ckfinite);
  itag_nop,   (I_arith AI_nop);
  itag_break,   (I_break);
  itag_arglist,   (I_arglist);
  itag_endfilter,   (I_endfilter);
  itag_endfinally,   I_endfinally;
  itag_refanytype,   (I_refanytype);
  itag_localloc,   (I_localloc);
  itag_throw,   (I_throw);
  itag_ldlen,   (I_ldlen);
  itag_rethrow,       (I_rethrow);
];;

let encode_table = Dictionary<_,_>(300);;
let _ = List.iter (fun (icode,i) -> encode_table.[i] <- icode) simple_instrs;;
let encode_instr si = encode_table.[si]
let is_noarg_instr s = encode_table.ContainsKey s

let decoders = 
 [ itag_ldarg, (u_uint16 >>  (fun x -> I_ldarg x));
   itag_starg, (u_uint16 >>  (fun x -> I_starg x));
   itag_ldarga, (u_uint16 >>  (fun x -> I_ldarga x));
   itag_stloc, (u_uint16 >>  (fun x -> I_stloc x));
   itag_ldloc, (u_uint16 >>  (fun x -> I_ldloc x));
   itag_ldloca, (u_uint16 >>  (fun x -> I_ldloca x)); 
   itag_stind, (u_tup3 u_alignment u_volatility ubasic_type) >> (fun (a,b,c) -> I_stind (a,b,c));
   itag_ldind, (u_tup3 u_alignment u_volatility ubasic_type) >> (fun (a,b,c) -> I_ldind (a,b,c));
   itag_cpblk, (u_tup2 u_alignment u_volatility) >> (fun (a,b) -> I_cpblk (a,b));
   itag_initblk, (u_tup2 u_alignment u_volatility) >> (fun (a,b) -> I_initblk (a,b));
   itag_call, (u_tup3 u_tailness uilmspec u_varargs) >> (fun (a,b,c) -> I_call (a,b,c));
   itag_callvirt, (u_tup3 u_tailness uilmspec u_varargs) >> (fun (a,b,c) -> I_callvirt (a,b,c));
   itag_callconstrained, (u_tup4 u_tailness u_iltyp uilmspec u_varargs) >> (fun (a,b,c,d) -> I_callconstraint (a,b,c,d));
   itag_newobj, (u_tup2 uilmspec u_varargs) >> (fun (a,b) -> I_newobj (a,b));
   itag_ldftn, uilmspec >> (fun a -> I_ldftn (a));
   itag_ldvirtftn, uilmspec >> (fun a -> I_ldvirtftn (a));
   itag_calli, (u_tup3 u_tailness u_callsig u_varargs) >> (fun (a,b,c) -> I_calli (a,b,c));
   itag_ldc, (u_tup2 ubasic_type u_ldc_info) >> (fun (a,b) -> I_arith (AI_ldc (a,b)));
   itag_conv, ubasic_type >> (fun a -> I_arith (AI_conv a));
   itag_conv_ovf, ubasic_type >> (fun a -> I_arith (AI_conv_ovf a));
   itag_conv_ovf_un, ubasic_type >> (fun a -> I_arith (AI_conv_ovf_un a));
   itag_stelem, ubasic_type >> (fun a -> I_stelem a);
   itag_ldelem, ubasic_type >> (fun a -> I_ldelem a);
   itag_ldfld, (u_tup3 u_alignment u_volatility uilfspec) >> (fun (a,b,c) -> I_ldfld (a,b,c));
   itag_ldflda, uilfspec >> (fun a -> I_ldflda a);
   itag_ldsfld, (u_tup2 u_volatility uilfspec) >> (fun (a,b) -> I_ldsfld (a,b));
   itag_ldsflda, uilfspec >> (fun a -> I_ldsflda a);
   itag_stfld, (u_tup3 u_alignment u_volatility uilfspec) >> (fun (a,b,c) -> I_stfld (a,b,c));
   itag_stsfld, (u_tup2 u_volatility uilfspec) >> (fun (a,b) -> I_stsfld (a,b));
   itag_ldtoken, u_ldtoken_info >> (fun a -> I_ldtoken a);
   itag_ldstr, u_string >> (fun a -> I_ldstr a);
   itag_box, u_iltyp >> (fun a -> I_box a);
   itag_unbox, u_iltyp >> (fun a -> I_unbox a);
   itag_unbox_any, u_iltyp >> (fun a -> I_unbox_any a);
   itag_newarr, u_tup2 u_array_shape u_iltyp >> (fun (a,b) -> I_newarr(a,b));
   itag_stelem_any, u_tup2 u_array_shape u_iltyp >> (fun (a,b) -> I_stelem_any(a,b));
   itag_ldelem_any, u_tup2 u_array_shape u_iltyp >> (fun (a,b) -> I_ldelem_any(a,b));
   itag_ldelema, u_tup3 u_readonly u_array_shape u_iltyp >> (fun (a,b,c) -> I_ldelema(a,b,c));
   itag_castclass, u_iltyp >> (fun a -> I_castclass a);
   itag_isinst, u_iltyp >> (fun a -> I_isinst a);
   itag_refanyval, u_iltyp >> (fun a -> I_refanyval a);
   itag_mkrefany, u_iltyp >> (fun a -> I_mkrefany a);
   itag_initobj, u_iltyp >> (fun a -> I_initobj a);
   itag_initobj, u_iltyp >> (fun a -> I_initobj a);
   itag_ldobj, (u_tup3 u_alignment u_volatility u_iltyp) >> (fun (a,b,c) -> I_ldobj (a,b,c));
   itag_stobj, (u_tup3 u_alignment u_volatility u_iltyp) >> (fun (a,b,c) -> I_stobj (a,b,c));
   itag_cpobj, u_iltyp >> (fun a -> I_cpobj a);
   itag_sizeof, u_iltyp >> (fun a -> I_sizeof a);
   itag_ilzero, u_iltyp >> (fun ty -> EI_ilzero ty);
   itag_ldlen_multi, u_tup2 u_int32 u_int32 >> (fun (a,b) -> EI_ldlen_multi (a,b));
   ] 

let decode_tab = 
  let tab = Array.init 256 (fun n -> (fun st -> ufailwith st ("no decoder for instruction "^string n)))
  let add_instr (icode,f) =  tab.[icode] <- f
  List.iter add_instr decoders;
  List.iter (fun (icode,mk) -> add_instr (icode,(fun _ -> mk))) simple_instrs;
  tab

#if INCLUDE_METADATA_WRITER
let p_instr n p x st = 
  p_int n st; p x st

let rec p_ilinstr x st =
  match x with
  | si when is_noarg_instr si -> p_instr (encode_instr si) p_unit () st
  | I_leave _ | I_brcmp _ | I_br _ | I_switch _ -> pfailwith st "p_ilinstr: cannot encode branches"
  | I_seqpoint s ->   ()
  | I_call      (tl,mspec,varargs) -> p_instr itag_call (p_tup3 p_tailness pilmspec p_varargs) (tl,mspec,varargs) st;
  | I_callvirt  (tl,mspec,varargs) -> p_instr itag_callvirt (p_tup3 p_tailness pilmspec p_varargs) (tl,mspec,varargs) st;
  | I_callconstraint    (tl,ty,mspec,varargs)   -> p_instr itag_callconstrained (p_tup4 p_tailness p_iltyp pilmspec p_varargs) (tl,ty,mspec,varargs) st;
  | I_newobj    (mspec,varargs) -> p_instr itag_newobj (p_tup2 pilmspec p_varargs) (mspec,varargs) st;
  | I_ldftn     mspec   ->  p_instr itag_ldftn pilmspec mspec st;
  | I_ldvirtftn mspec   -> p_instr itag_ldvirtftn pilmspec mspec st;
  | I_calli (a,b,c)     ->  p_instr itag_calli (p_tup3 p_tailness p_callsig p_varargs) (a,b,c) st;
  | I_ldarg x ->  p_instr itag_ldarg p_uint16 x st
  | I_starg x ->  p_instr itag_starg p_uint16 x st
  | I_ldarga x ->  p_instr itag_ldarga p_uint16 x st
  | I_ldloc x ->  p_instr itag_ldloc p_uint16 x st
  | I_stloc x ->  p_instr itag_stloc p_uint16 x st
  | I_ldloca x ->  p_instr itag_ldloca p_uint16 x st
  | I_cpblk     (al,vol) -> p_instr itag_cpblk (p_tup2 p_alignment p_volatility) (al,vol) st
  | I_initblk   (al,vol) -> p_instr itag_initblk (p_tup2 p_alignment p_volatility) (al,vol) st
  | I_arith (AI_ldc (a,b)) -> p_instr itag_ldc (p_tup2 pbasic_type p_ldc_info) (a,b) st
  | I_arith (AI_conv a) -> p_instr itag_conv pbasic_type a st
  | I_arith (AI_conv_ovf a) -> p_instr itag_conv_ovf pbasic_type a st
  | I_arith (AI_conv_ovf_un a) -> p_instr itag_conv_ovf_un pbasic_type a st
  | I_ldind (a,b,c) -> p_instr itag_ldind (p_tup3 p_alignment p_volatility pbasic_type) (a,b,c) st
  | I_stind (a,b,c) -> p_instr itag_stind (p_tup3 p_alignment p_volatility pbasic_type) (a,b,c) st
  | I_stelem a  -> p_instr itag_stelem pbasic_type a st 
  | I_ldelem a  -> p_instr itag_ldelem pbasic_type a st 
  | I_ldfld(a,b,c) -> p_instr itag_ldfld (p_tup3 p_alignment p_volatility pilfspec) (a,b,c) st
  | I_ldflda(c) -> p_instr itag_ldflda pilfspec c st
  | I_ldsfld(a,b) -> p_instr itag_ldsfld (p_tup2 p_volatility pilfspec) (a,b) st
  | I_ldsflda(a) -> p_instr itag_ldsflda pilfspec a st
  | I_stfld(a,b,c) -> p_instr itag_stfld (p_tup3 p_alignment p_volatility pilfspec) (a,b,c) st
  | I_stsfld(a,b) -> p_instr itag_stsfld (p_tup2 p_volatility pilfspec) (a,b) st
  | I_ldtoken  tok -> p_instr itag_ldtoken p_ldtoken_info tok st
  | I_ldstr     s       -> p_instr itag_ldstr p_string s st
  | I_box  ty   -> p_instr itag_box p_iltyp ty st
  | I_unbox  ty -> p_instr itag_unbox p_iltyp ty st
  | I_unbox_any  ty     -> p_instr itag_unbox_any p_iltyp ty st
  | I_newarr(a,b)       -> p_instr itag_newarr (p_tup2 p_array_shape p_iltyp) (a,b) st
  | I_stelem_any(a,b)   -> p_instr itag_stelem_any (p_tup2 p_array_shape p_iltyp) (a,b) st
  | I_ldelem_any(a,b)   -> p_instr itag_ldelem_any (p_tup2 p_array_shape p_iltyp) (a,b) st
  | I_ldelema(a,b,c)    -> p_instr itag_ldelema (p_tup3 p_readonly p_array_shape p_iltyp) (a,b,c) st
  | I_castclass  ty     -> p_instr itag_castclass p_iltyp ty st
  | I_isinst  ty        -> p_instr itag_isinst p_iltyp ty st
  | I_refanyval  ty     -> p_instr itag_refanyval p_iltyp ty st
  | I_mkrefany  ty      -> p_instr itag_mkrefany p_iltyp ty st
  | I_initobj  ty       -> p_instr itag_initobj p_iltyp ty st
  | I_ldobj(a,b,c)      -> p_instr itag_ldobj (p_tup3 p_alignment p_volatility p_iltyp) (a,b,c) st
  | I_stobj(a,b,c)      -> p_instr itag_stobj (p_tup3 p_alignment p_volatility p_iltyp) (a,b,c) st
  | I_cpobj  ty         -> p_instr itag_cpobj p_iltyp ty st
  | I_sizeof  ty        -> p_instr itag_sizeof p_iltyp ty st
  | EI_ilzero (a)        -> p_instr itag_ilzero p_iltyp a st
  | EI_ldlen_multi (n,m) -> p_instr itag_ldlen_multi (p_tup2 p_int32 p_int32) (n,m) st
  | I_other e when Ilx.is_ilx_ext_instr e -> pfailwith st "an ILX instruction cannot be emitted"
  |  _ -> pfailwith st "an IL instruction cannot be emitted"
#endif

let u_ilinstr st = 
  let n = u_int st
  decode_tab.[n] st

  

(*---------------------------------------------------------------------------
 * Pickle/unpickle for F# types and module signatures
 *------------------------------------------------------------------------- *)

#if INCLUDE_METADATA_WRITER
let p_Map pk pv = p_wrap Map.to_list (p_list (p_tup2 pk pv))
let p_namemap p = p_Map p_string p
let p_immutable_ref p = p_wrap (!) p
#endif

let u_Map uk uv = u_wrap Map.of_list (u_list (u_tup2 uk uv))
let u_namemap u = u_Map u_string u
let u_immutable_ref u = u_wrap (ref) u

#if INCLUDE_METADATA_WRITER
let p_pos (x: pos) st = p_tup2 p_int p_int (dest_pos x) st
let p_range (x: range) st = p_tup3 p_string p_pos p_pos (dest_range x) st
let p_dummy_range : range pickler   = fun x st -> ()
let p_ident (x: ident) st = p_tup2 p_string p_range (x.idText,x.idRange) st
let p_xmldoc (XmlDoc x) st = p_array p_string x st
#endif

let u_pos st = let a = u_int st in let b = u_int st in mk_pos a b
let u_range st = let a = u_string st in let b = u_pos st in let c = u_pos st in mk_range a b c

// Most ranges (e.g. on optimization expressions) can be elided from stored data 
let u_dummy_range : range unpickler = fun st -> range0
let u_ident st = let a = u_string st in let b = u_range st in ident(a,b)
let u_xmldoc st = XmlDoc (u_array u_string st)


#if INCLUDE_METADATA_WRITER
let p_nonlocal_item_ref () {nlr_nlpath=a;nlr_item=b} st =
    p_nlpath a st; p_string b st
let p_local_item_ref ctxt tab st = p_osgn_ref ctxt tab st

let rec p_vref ctxt x st = 
    match x with 
    | VRef_private(x) -> p_byte 0 st; p_local_item_ref ctxt st.ovals x st
    | VRef_nonlocal(x) -> p_byte 1 st; p_nonlocal_item_ref () x st
let rec p_tcref ctxt x st = 
    match x with 
    | ERef_private(x) -> p_byte 0 st; p_local_item_ref ctxt st.otycons x st
    | ERef_nonlocal(x) -> p_byte 1 st; p_nonlocal_item_ref () x st

let p_ucref (UCRef(a,b)) st = p_tup2 (p_tcref "ucref") p_string (a,b) st
let p_rfref (RFRef(a,b)) st = p_tup2 (p_tcref "rfref") p_string (a,b) st
let p_vrefs ctxt = p_list (p_vref ctxt) 
let p_tpref x st = p_local_item_ref "typar" st.otypars  x st

#endif

let u_nonlocal_item_ref () st = 
    let a = u_nlpath st in let b = u_string st
    mk_nlr a b
  
let u_local_item_ref tab st = u_osgn_ref tab st

let u_vref st = 
    let tag = u_byte st
    match tag with
    | 0 -> u_local_item_ref st.ivals st |> (fun x -> VRef_private x)
    | 1 -> u_nonlocal_item_ref () st |> (fun x -> VRef_nonlocal x)
    | _ -> ufailwith st "u_item_ref"
    
let u_tcref st = 
    let tag = u_byte st
    match tag with
    | 0 -> u_local_item_ref st.itycons  st |> (fun x -> ERef_private x)
    | 1 -> u_nonlocal_item_ref () st |> (fun x -> ERef_nonlocal x)
    | _ -> ufailwith st "u_item_ref"
    
let u_ucref st  = let a,b = u_tup2 u_tcref u_string st in UCRef(a,b)

let u_rfref st = let a,b = u_tup2 u_tcref u_string st in RFRef(a,b)

let u_vrefs = u_list u_vref 

let u_tpref st = u_local_item_ref st.itypars st


#if INCLUDE_METADATA_WRITER
let fill_p_typ,p_typ = p_hole()
let p_typs = (p_list p_typ);;

let fill_p_attribs,p_attribs = p_hole()
#endif

let fill_u_typ,u_typ = u_hole()
let u_typs = (u_list u_typ);;
let fill_u_attribs,u_attribs = u_hole()


#if INCLUDE_METADATA_WRITER
let p_kind x st =
  p_byte (match x with
          | KindType -> 0
          | KindMeasure -> 1) st

let p_member_kind x st = 
    p_byte (match x with 
            | MemberKindMember -> 0
            | MemberKindPropertyGet  -> 1
            | MemberKindPropertySet -> 2
            | MemberKindConstructor -> 3
            | MemberKindClassConstructor -> 4
            | MemberKindPropertyGetSet -> pfailwith st "pickling: MemberKindPropertyGetSet only expected in parse trees") st
#endif


let u_kind st =
  match u_byte st with
  | 0 -> KindType
  | 1 -> KindMeasure
  | _ -> ufailwith st "u_kind"

let u_member_kind st = 
    match u_byte st with 
    | 0 -> MemberKindMember 
    | 1 -> MemberKindPropertyGet  
    | 2 -> MemberKindPropertySet 
    | 3 -> MemberKindConstructor
    | 4 -> MemberKindClassConstructor
    | _ -> ufailwith st "u_member_kind"

#if INCLUDE_METADATA_WRITER
let p_MemberFlags x st = 
  ptup7 (p_option p_string) p_bool p_bool p_bool p_bool p_bool p_member_kind 
      (x.OverloadQualifier, 
       x.MemberIsInstance, 
       x.MemberIsVirtual, 
       x.MemberIsDispatchSlot, 
       x.MemberIsOverrideOrExplicitImpl, 
       x.MemberIsFinal, 
       x.MemberKind) st
#endif
let u_MemberFlags st = 
  let x1,x2,x3,x4,x5,x6,x7 = utup7 (u_option u_string) u_bool u_bool u_bool u_bool u_bool u_member_kind st
  { OverloadQualifier=x1;
    MemberIsInstance=x2;
    MemberIsVirtual=x3;
    MemberIsDispatchSlot=x4;
    MemberIsOverrideOrExplicitImpl=x5;
    MemberIsFinal=x6;
    MemberKind=x7}

#if INCLUDE_METADATA_WRITER
let p_trait_sln sln st = 
    match sln with 
    | ILMethSln(a,b,c,d) ->
         p_byte 0 st; p_tup4 p_typ (p_option p_iltref) p_ilmref p_typs (a,b,c,d) st
    | FSMethSln(a,b,c) ->
         p_byte 1 st; p_tup3 p_typ (p_vref "trait") p_typs (a,b,c) st
    | BuiltInSln -> 
         p_byte 2 st

let p_trait (TTrait(a,b,c,d,e,f)) st  = 
    p_tup6 p_typs p_string p_MemberFlags p_typs (p_option p_typ) (p_option p_trait_sln) (a,b,c,d,e,!f) st
#endif

let u_trait_sln st = 
    let tag = u_byte st
    match tag with 
    | 0 -> 
        let (a,b,c,d) = u_tup4 u_typ (u_option u_iltref) u_ilmref u_typs st
        ILMethSln(a,b,c,d) 
    | 1 -> 
        let (a,b,c) = u_tup3 u_typ u_vref u_typs st
        FSMethSln(a,b,c)
    | 2 -> 
        BuiltInSln
    | _ -> ufailwith st "u_trait_sln" 

let u_trait st = 
    let a,b,c,d,e,f = u_tup6 u_typs u_string u_MemberFlags u_typs (u_option u_typ) (u_option u_trait_sln) st
    TTrait (a,b,c,d,e,ref f)

#if INCLUDE_METADATA_WRITER
let rec p_measure_expr measure st =
    let measure = strip_upeqnsA false measure 
    match measure with 
    | MeasureCon tcref   -> p_byte 0 st; p_tcref "measure" tcref st
    | MeasureInv x       -> p_byte 1 st; p_measure_expr x st
    | MeasureProd(x1,x2) -> p_byte 2 st; p_measure_expr x1 st; p_measure_expr x2 st
    | MeasureVar(v)      -> p_byte 3 st; p_tpref v st
    | MeasureOne      -> p_byte 4 st
#endif

let rec u_measure_expr st =
    let tag = u_byte st
    match tag with
    | 0 -> let a = u_tcref st in MeasureCon a
    | 1 -> let a = u_measure_expr st in MeasureInv a
    | 2 -> let a,b = u_tup2 u_measure_expr u_measure_expr st in MeasureProd (a,b)
    | 3 -> let a = u_tpref st in MeasureVar a
    | 4 -> MeasureOne
    | _ -> ufailwith st "u_measure_expr"

#if INCLUDE_METADATA_WRITER
let p_typar_constraint x st = 
    match x with 
    | TTyparCoercesToType (a,m)                 -> p_byte 0 st; p_tup2 p_typ p_range (a,m) st
    | TTyparMayResolveMemberConstraint(traitInfo,m) -> p_byte 1 st; p_tup2 p_trait p_range (traitInfo,m) st
    | TTyparDefaultsToType(_,rty,m)               -> p_byte 2 st; p_tup2 p_typ p_range (rty,m) st
    | TTyparSupportsNull(m)                     -> p_byte 3 st; p_range m st
    | TTyparIsNotNullableValueType(m)           -> p_byte 4 st; p_range m st
    | TTyparIsReferenceType(m)                  -> p_byte 5 st; p_range m st
    | TTyparRequiresDefaultConstructor(m)       -> p_byte 6 st; p_range m st
    | TTyparSimpleChoice(tys,m)                 -> p_byte 7 st; p_tup2 p_typs p_range (tys,m) st
    | TTyparIsEnum(ty,m)                        -> p_byte 8 st; p_typ ty st; p_range m st
    | TTyparIsDelegate(aty,bty,m)               -> p_byte 9 st; p_typ aty st; p_typ bty st; p_range m st
let p_typar_constraints = (p_list p_typar_constraint)
#endif

let u_typar_constraint st = 
    let tag = u_byte st
    match tag with
    | 0 -> u_tup2 u_typ u_range st |> (fun (a,b)   -> (fun ridx -> TTyparCoercesToType (a,b) ))
    | 1 -> u_tup2 u_trait u_range st |> (fun (traitInfo,f) -> (fun ridx -> TTyparMayResolveMemberConstraint(traitInfo,f)))
    | 2 -> u_tup2 u_typ u_range                  st |> (fun (a,b)   -> (fun ridx -> TTyparDefaultsToType(ridx,a,b)))
    | 3 -> u_range                             st |> (fun (a)     -> (fun ridx -> TTyparSupportsNull(a)))
    | 4 -> u_range                             st |> (fun (a)     -> (fun ridx -> TTyparIsNotNullableValueType(a)))
    | 5 -> u_range                             st |> (fun (a)     -> (fun ridx -> TTyparIsReferenceType(a)))
    | 6 -> u_range                             st |> (fun (a)     -> (fun ridx -> TTyparRequiresDefaultConstructor(a)))
    | 7 -> u_tup2 u_typs u_range                st |> (fun (a,b)   -> (fun ridx -> TTyparSimpleChoice(a,b)))
    | 8 -> u_tup2 u_typ u_range                st |> (fun (a,b)   -> (fun ridx -> TTyparIsEnum(a,b)))
    | 9 -> u_tup3 u_typ u_typ u_range          st |> (fun (a,b,c) -> (fun ridx -> TTyparIsDelegate(a,b,c)))
    | _ -> ufailwith st "u_typar_constraint" 


let u_typar_constraints = (u_list_revi u_typar_constraint)


#if INCLUDE_METADATA_WRITER
let p_typar_spec_data (x:TyparData) st = 
    p_tup5
      p_ident 
      p_attribs
      p_int32
      p_typar_constraints
      p_xmldoc

      (x.typar_id,x.typar_attribs,x.typar_flags,x.typar_constraints,x.typar_xmldoc) st

let p_typar_spec (x:Typar) st = 
    //Disabled, workaround for bug 2721: if x.Rigidity <> TyparRigid then warning(Error(sprintf "p_typar_spec: typar#%d is not rigid" x.Stamp, x.Range));
    if x.IsFromError then warning(Error("p_typar_spec: from error", x.Range));
    p_osgn_decl st.otypars p_typar_spec_data x st

let p_typar_specs = (p_list p_typar_spec)
#endif

let u_typar_spec_data st = 
    let a,c,d,e,g = u_tup5 u_ident u_attribs u_int32 u_typar_constraints u_xmldoc st
    { typar_id=a; 
      typar_stamp=new_stamp();
      typar_attribs=c;
      typar_flags=d;
      typar_constraints=e;
      typar_solution=None;
      typar_xmldoc=g }

let u_typar_spec st = 
    u_osgn_decl st.itypars u_typar_spec_data st 

let u_typar_specs = (u_list u_typar_spec)


#if INCLUDE_METADATA_WRITER
let _ = fill_p_typ (fun ty st ->
   let ty = strip_tpeqns ty
   match ty with 
   | TType_tuple l                                                 -> p_byte 0 st; p_typs l st
   | TType_app(ERef_nonlocal { nlr_nlpath=nlpath; nlr_item=item },[]) -> p_byte 1 st; p_simpletyp (nlpath,item) st
   | TType_app (tc,tinst)                                          -> p_byte 2 st; p_tup2 (p_tcref "typ") p_typs (tc,tinst) st
   | TType_fun (d,r)                                               -> p_byte 3 st; p_tup2 p_typ p_typ (d,r) st
   | TType_var r                                                   -> p_byte 4 st; p_tpref r st
   | TType_forall (tps,r)                                          -> p_byte 5 st; p_tup2 p_typar_specs p_typ (tps,r) st
   | TType_modul_bindings                                          -> p_byte 6 st; p_void st
   | TType_measure measure                                         -> p_byte 7 st; p_measure_expr measure st
   | TType_ucase (uc,tinst)                                        -> p_byte 8 st; p_tup2 p_ucref p_typs (uc,tinst) st)

#endif

let _ = fill_u_typ (fun st ->
    let tag = u_byte st
    match tag with
    | 0 -> let l = u_typs st                              in TType_tuple l
    | 1 -> u_simpletyp st 
    | 2 -> let tc = u_tcref st in let tinst = u_typs st    in TType_app (tc,tinst)
    | 3 -> let d = u_typ st    in let r = u_typ st         in TType_fun (d,r)
    | 4 -> let r = u_tpref st                             in  r.AsType
    | 5 -> let tps = u_typar_specs st in let r = u_typ st  in TType_forall (tps,r)
    | 6 ->                                                  TType_modul_bindings
    | 7 -> let measure = u_measure_expr st                in TType_measure measure
    | 8 -> let uc = u_ucref st in let tinst = u_typs st    in TType_ucase (uc,tinst)
    | _ -> ufailwith st "u_typ")
  

#if INCLUDE_METADATA_WRITER
let fill_p_binds,p_binds = p_hole()
let fill_p_targets,p_targets = p_hole()
let fill_p_Exprs,p_Exprs = p_hole()
let fill_p_FlatExprs,p_FlatExprs = p_hole()
let fill_p_constraints,p_constraints = p_hole()
let fill_p_Vals,p_Vals = p_hole()
let fill_p_FlatVals,p_FlatVals = p_hole()
#endif

let fill_u_binds,u_binds = u_hole()
let fill_u_targets,u_targets = u_hole()
let fill_u_Exprs,u_Exprs = u_hole()
let fill_u_FlatExprs,u_FlatExprs = u_hole()
let fill_u_constraints,u_constraints = u_hole()
let fill_u_Vals,u_Vals = u_hole()
let fill_u_FlatVals,u_FlatVals = u_hole()

#if INCLUDE_METADATA_WRITER
let p_TopArgInfo (TopArgInfo(a,b)) st = p_attribs a st; p_option p_ident b st
let p_TopTyparInfo (TopTyparInfo(a,b)) st = p_tup2 p_ident p_kind (a,b) st
let p_ValTopReprInfo (TopValInfo (a,args,ret)) st = 
    p_list p_TopTyparInfo a st; 
    p_list (p_list p_TopArgInfo) args st; 
    p_TopArgInfo ret st
#endif


let u_TopArgInfo st = let a = u_attribs st in let b = u_option u_ident st in match a,b with [],None -> TopValInfo.unnamedTopArg1 | _ -> TopArgInfo(a,b)
let u_TopTyparInfo st = let a,b = u_tup2 u_ident u_kind st in TopTyparInfo(a,b)
let u_ValTopReprInfo st = 
    let a = u_list u_TopTyparInfo st
    let b = u_list (u_list u_TopArgInfo) st
    let c = u_TopArgInfo st
    TopValInfo (a,b,c)

#if INCLUDE_METADATA_WRITER
let p_ranges = (p_option (p_tup2 p_range p_range)) 
let p_istype x st = 
  match x with 
  | FSharpModuleWithSuffix -> p_byte 0 st
  | FSharpModule            -> p_byte 1 st
  | Namespace              -> p_byte 2 st
let p_cpath (CompPath(a,b)) st = p_tup2 p_scoref (p_list (p_tup2 p_string p_istype)) (a,b) st

#endif

let u_ranges = (u_option (u_tup2 u_range u_range))

let u_istype st = 
  let tag = u_byte st
  match tag with
  | 0 -> FSharpModuleWithSuffix 
  | 1 -> FSharpModule  
  | 2 -> Namespace 
  | _ -> ufailwith st "u_istype"

let u_cpath  st = let a,b = u_tup2 u_scoref (u_list (u_tup2 u_string u_istype)) st in (CompPath(a,b))


let rec dummy x = x
#if INCLUDE_METADATA_WRITER
and p_tycon_repr x st = 
    match x with 
    | TRecdRepr fs          -> p_byte 0 st; p_rfield_table fs st
    | TFiniteUnionRepr x    -> p_byte 2 st; p_list p_unioncase_spec (Array.to_list x.funion_ucases.ucases_by_index) st
    | TILObjModelRepr (_,_,td) -> error (Failure("Unexpected IL type "^td.Name))
    | TAsmRepr ilty         -> p_byte 4 st; p_iltyp ilty st
    | TFsObjModelRepr r     -> p_byte 5 st; p_tycon_objmodel_data r st
    | TMeasureableRepr ty       -> p_byte 6 st; p_typ ty st
and p_tycon_objmodel_data x st = 
  p_tup3 p_tycon_objmodel_kind (p_vrefs "vslots") p_rfield_table 
    (x.fsobjmodel_kind, x.fsobjmodel_vslots, x.fsobjmodel_rfields) st
and p_unioncase_spec x st =                     
    ptup7 
        p_rfield_table p_typ p_string p_ident p_attribs p_xmldoc p_access
        (x.ucase_rfields,x.ucase_rty,x.ucase_il_name,x.ucase_id,x.ucase_attribs,x.ucase_xmldoc,x.ucase_access) st
and p_exnc_spec_data x st = p_entity_spec_data x st
and p_exnc_repr x st =
  match x with 
  | TExnAbbrevRepr x -> p_byte 0 st; (p_tcref "exn abbrev") x st
  | TExnAsmRepr x    -> p_byte 1 st; p_iltref x st
  | TExnFresh x      -> p_byte 2 st; p_rfield_table x st
  | TExnNone         -> p_byte 3 st
and p_exnc_spec x st = p_tycon_spec x st
and p_access (TAccess n) st = p_list p_cpath n st
and p_recdfield_spec x st = 
    ptup10
      p_bool p_typ p_bool p_bool (p_option p_const) p_ident p_attribs p_attribs p_xmldoc p_access 
      (x.rfield_mutable,x.rfield_type,x.rfield_static,x.rfield_secret,x.rfield_const,x.rfield_id,x.rfield_pattribs,x.rfield_fattribs,x.rfield_xmldoc,x.rfield_access) st
and p_rfield_table x st = 
  p_list p_recdfield_spec (Array.to_list x.rfields_by_index) st

and p_entity_spec_data x st = 
  ptup15
    p_typar_specs
    p_ident 
    (p_option p_pubpath)
    (p_tup2 p_access p_access)
    p_attribs
    (p_option p_tycon_repr)
    (p_option p_typ)
    p_tcaug
    p_xmldoc
    p_kind
    p_bool 
    p_bool 
    (p_option p_cpath)
    (p_lazy p_modul_typ)
    p_exnc_repr 
    (x.entity_typars.Force(x.entity_range),
     ident (x.entity_name, x.entity_range),
     x.entity_pubpath,
     (x.entity_accessiblity, x.entity_tycon_repr_accessibility),
     x.entity_attribs,
     x.entity_tycon_repr,
     x.entity_tycon_abbrev,
     x.entity_tycon_tcaug,
     x.entity_xmldoc,
     x.entity_kind,
     x.entity_uses_prefix_display,
     x.entity_is_modul_or_namespace,
     x.entity_cpath,
     x.entity_modul_contents,
     x.entity_exn_info) st
and p_tcaug p st = 
  ptup7
    (p_option (p_tup2 (p_vref "compare_obj") (p_vref "compare")))
    (p_option (p_vref "hash"))
    (p_option (p_tup2 (p_vref "hash") (p_vref "equals")))
    (p_namemap (p_vrefs "adhoc")) 
    (p_list (p_tup3 p_typ p_bool p_range))
    (p_option p_typ)
    p_bool
    (p.tcaug_compare, p.tcaug_structural_hash, p.tcaug_equals, p.tcaug_adhoc, p.tcaug_implements,p.tcaug_super,p.tcaug_abstract) st

and p_tycon_spec x st = p_osgn_decl st.otycons p_entity_spec_data x st

and p_parentref x st = 
    match x with 
    | ParentNone -> p_byte 0 st
    | Parent x -> p_byte 1 st; p_tcref "parent tycon" x st

and p_attribkind x st = 
    match x with 
    | ILAttrib x -> p_byte 0 st; p_ilmref x st
    | FSAttrib x -> p_byte 1 st; p_vref "attrib" x st

and p_attrib (Attrib (a,b,c,d,e)) st = 
    p_tup5 (p_tcref "attrib") p_attribkind (p_list p_attrib_expr) (p_list p_attrib_arg) p_range (a,b,c,d,e) st

and p_attrib_expr (AttribExpr(e1,e2)) st = 
    p_tup2 p_expr p_expr (e1,e2) st

and p_attrib_arg (AttribNamedArg(a,b,c,d)) st = 
    p_tup4 p_string p_typ p_bool p_attrib_expr (a,b,c,d) st

and p_member_info x st = 
    p_tup5 p_string 
        (p_tcref "member_info")  p_MemberFlags (p_list p_slotsig) p_bool 
        (x.CompiledName, x.ApparentParent,x.MemberFlags,x.ImplementedSlotSigs,x.IsImplemented) st

and p_tycon_objmodel_kind x st = 
    match x with 
    | TTyconClass       -> p_byte 0 st; p_void st
    | TTyconInterface   -> p_byte 1 st; p_void st
    | TTyconStruct      -> p_byte 2 st; p_void st
    | TTyconDelegate ss -> p_byte 3 st; p_slotsig ss st
    | TTyconEnum        -> p_byte 4 st; p_void st

and p_mustinline x st = 
    p_byte (match x with 
            | PseudoValue -> 0
            | AlwaysInline  -> 1
            | OptionalInline -> 2
            | NeverInline -> 3) st

and p_basethis x st = 
    p_byte (match x with 
            | BaseVal -> 0
            | CtorThisVal  -> 1
            | NormalVal -> 2
            | MemberThisVal -> 3) st

and p_vrefFlags x st = 
    p_byte (match x with 
            | NormalValUse -> 0
            | CtorValUsedAsSuperInit  -> 1
            | CtorValUsedAsSelfInit  -> 2
            | VSlotDirectCall -> 3) st

and p_ValData x st =
    if verbose then dprintf "p_ValData, nm = %s, stamp #%d, ty = %s\n" x.val_name x.val_stamp (DebugPrint.showType x.val_type);
    ptup12
      p_string
      p_ranges
      p_typ 
      p_int64 
      (p_option p_pubpath) 
      (p_option p_member_info) 
      p_attribs 
      (p_option p_ValTopReprInfo)
      p_xmldoc
      p_access
      p_parentref
      (p_option p_const)
      ( x.val_name,
        (* only keep range information on published values, not on optimization data *)
        (match x.val_pubpath with None -> None | Some _ -> Some(x.val_range, x.val_defn_range)),
        x.val_type,
        x.val_flags,
        x.val_pubpath,
        x.val_member_info,
        x.val_attribs,
        x.val_top_repr_info,
        x.val_xmldoc,
        x.val_access,
        x.val_actual_parent,
        x.val_const) st
      
and p_Val x st = p_osgn_decl st.ovals p_ValData x st
and p_modul_typ (x: ModuleOrNamespaceType) st = 
    p_tup3
      p_istype
      (p_namemap p_Val)
      (p_namemap p_tycon_spec)
      (x.ModuleOrNamespaceKind,x.AllValuesAndMembers,x.AllEntities)
      st

#endif


and u_tycon_repr st = 
    let tag = u_byte st
    match tag with
    | 0 -> u_rfield_table            st |> (fun x -> TRecdRepr x)
    | 2 -> u_list u_unioncase_spec   st |> (fun x -> MakeUnionRepr x)
    | 4 -> u_iltyp                   st |> (fun x -> TAsmRepr x)
    | 5 -> u_tycon_objmodel_data     st |> (fun x -> TFsObjModelRepr x)
    | 6 -> u_typ st |> (fun ty -> TMeasureableRepr ty)
    | _ -> ufailwith st "u_tycon_repr"
  
and u_tycon_objmodel_data st = 
  let x1,x2,x3 = u_tup3 u_tycon_objmodel_kind u_vrefs u_rfield_table st
  {fsobjmodel_kind=x1; fsobjmodel_vslots=x2; fsobjmodel_rfields=x3 }
  
and u_unioncase_spec st = 
    let a,b,c,d,e,f,i = utup7 u_rfield_table u_typ u_string u_ident u_attribs u_xmldoc u_access st
    {ucase_rfields=a; ucase_rty=b; ucase_il_name=c; ucase_id=d; ucase_attribs=e;ucase_xmldoc=f;ucase_access=i }
    
and u_exnc_spec_data st = u_entity_spec_data st 

and u_exnc_repr st =
  let tag = u_byte st
  match tag with
  | 0 -> u_tcref           st |> (fun x -> TExnAbbrevRepr x)
  | 1 -> u_iltref          st |> (fun x -> TExnAsmRepr x)
  | 2 -> u_rfield_table st |> (fun x -> TExnFresh x)
  | 3 -> TExnNone
  | _ -> ufailwith st "u_exnc_repr"
  
and u_exnc_spec st = u_tycon_spec st

and u_access st = 
    match u_list u_cpath st with 
    | [] -> taccessPublic (* save unnecessary allocations *)
    | res -> TAccess res

and u_recdfield_spec st = 
    let a,c1,c2,c2b,c3,d,e1,e2,f,g = utup10 u_bool u_typ u_bool u_bool (u_option u_const) u_ident u_attribs u_attribs u_xmldoc u_access st
    { rfield_mutable=a;  rfield_type=c1; rfield_static=c2; rfield_secret=c2b; rfield_const=c3; rfield_id=d; rfield_pattribs=e1;rfield_fattribs=e2;rfield_xmldoc=f; rfield_access=g }

and u_rfield_table st = MakeRecdFieldsTable (u_list u_recdfield_spec st)

and u_entity_spec_data st = 
    let x1,x2,x3,(x4a,x4b),x6,x7,x8,x9,x10,x10b,x11,x11b,x12,x13,x14 = 
       utup15
          u_typar_specs
          u_ident 
          (u_option u_pubpath)
          (u_tup2 u_access u_access)
          u_attribs
          (u_option u_tycon_repr)
          (u_option u_typ) 
          u_tcaug 
          u_xmldoc 
          u_kind
          u_bool 
          u_bool 
          (u_option u_cpath )
          (u_lazy u_modul_typ) 
          u_exnc_repr 
          st
    { entity_typars=LazyWithContext<_,_>.NotLazy x1;
      entity_stamp=new_stamp();
      entity_name=x2.idText;
      entity_range=x2.idRange;
      entity_pubpath=x3;
      entity_accessiblity=x4a;
      entity_tycon_repr_accessibility=x4b;
      entity_attribs=x6;
      entity_tycon_repr=x7;
      entity_tycon_abbrev=x8;
      entity_tycon_tcaug=x9;
      entity_xmldoc=x10;
      entity_kind=x10b;
      entity_uses_prefix_display=x11;
      entity_is_modul_or_namespace=x11b;
      entity_cpath=x12;
      entity_modul_contents= x13;
      entity_exn_info=x14;
      entity_il_repr_cache=new_cache();  } 

and u_tcaug st = 
  let a,b1,b2,c,d,e,g = 
    utup7
      (u_option (u_tup2 u_vref u_vref))
      (u_option u_vref)
      (u_option (u_tup2 u_vref u_vref))
      (u_namemap u_vrefs)
      (u_list (u_tup3 u_typ u_bool u_range)) 
      (u_option u_typ)
      u_bool 
      st 
  {tcaug_compare=a; 
   tcaug_structural_hash=b1;
   tcaug_compare_withc=None; 
   tcaug_hash_and_equals_withc=None; 
   tcaug_equals=b2; 
   // only used for code generation and checking - hence don't care about the values when reading back in
   tcaug_hasObjectGetHashCode=false; 
   tcaug_adhoc=c; 
   tcaug_implements=d;
   tcaug_super=e;
   tcaug_closed=true; 
   tcaug_abstract=g}
 
and u_tycon_spec st = u_osgn_decl st.itycons u_entity_spec_data st 

and u_parentref st = 
    let tag = u_byte st
    match tag with
    | 0 -> ParentNone
    | 1 -> u_tcref st |> (fun x -> Parent x)
    | _ -> ufailwith st "u_attribkind" 

and u_attribkind st = 
    let tag = u_byte st
    match tag with
    | 0 -> u_ilmref st |> (fun x -> ILAttrib x) 
    | 1 -> u_vref st |> (fun x -> FSAttrib x)
    | _ -> ufailwith st "u_attribkind" 

and u_attrib st : Attrib = 
    let a,b,c,d,e = u_tup5 u_tcref u_attribkind (u_list u_attrib_expr) (u_list u_attrib_arg) u_range st
    Attrib(a,b,c,d,e)

and u_attrib_expr st = 
    let a,b = u_tup2 u_expr u_expr st 
    AttribExpr(a,b)

and u_attrib_arg st  = 
    let a,b,c,d = u_tup4 u_string u_typ u_bool u_attrib_expr st 
    AttribNamedArg(a,b,c,d)

and u_member_info st = 
    let x1,x2,x3,x4,x5 = u_tup5 u_string u_tcref u_MemberFlags (u_list u_slotsig) u_bool st
    { CompiledName=x1;
      ApparentParent=x2;
      MemberFlags=x3;
      ImplementedSlotSigs=x4;
      IsImplemented=x5  }

and u_tycon_objmodel_kind st = 
    let tag = u_byte st
    match tag with
    | 0 -> u_void    st |> (fun () -> TTyconClass )
    | 1 -> u_void    st |> (fun () -> TTyconInterface  )
    | 2 -> u_void    st |> (fun () -> TTyconStruct )
    | 3 -> u_slotsig st |> (fun x  -> TTyconDelegate x)
    | 4 -> u_void    st |> (fun () -> TTyconEnum )
    | _ -> ufailwith st "u_tycon_objmodel_kind"

and u_mustinline st = 
    match u_byte st with 
    | 0 -> PseudoValue 
    | 1 -> AlwaysInline  
    | 2 -> OptionalInline 
    | 3 -> NeverInline 
    | _ -> ufailwith st "u_mustinline"

and u_basethis st = 
    match u_byte st with 
    | 0 -> BaseVal 
    | 1 -> CtorThisVal  
    | 2 -> NormalVal 
    | 3 -> MemberThisVal
    | _ -> ufailwith st "u_basethis"

and u_vrefFlags st = 
    match u_byte st with 
    | 0 -> NormalValUse 
    | 1 -> CtorValUsedAsSuperInit
    | 2 -> CtorValUsedAsSelfInit
    | 3 -> VSlotDirectCall
    | _ -> ufailwith st "u_vrefFlags"

and u_ValData st =
  let x1,x1a,x2,x4,x7,x8,x9,x10,x12,x13,x13b,x14 = 
    utup12
      u_string
      u_ranges
      u_typ 
      u_int64
      (u_option u_pubpath)
      (u_option u_member_info) 
      u_attribs 
      (u_option u_ValTopReprInfo)
      u_xmldoc 
      u_access
      u_parentref
      (u_option u_const) st
  { val_name=x1;
    val_range=(match x1a with None -> range0 | Some(a,_) -> a);
    val_defn_range=(match x1a with None -> range0 | Some(_,b) -> b);
    val_type=x2;
    val_stamp=new_stamp();
    val_flags=x4;
    val_pubpath=x7;
    val_defn = None;
    val_member_info=x8;
    val_attribs=x9;
    val_top_repr_info=x10;
    val_xmldoc=x12;
    val_access=x13;
    val_actual_parent=x13b;
    val_const=x14;
  }

and u_Val st = u_osgn_decl st.ivals u_ValData st 


and u_modul_typ st = 
  let x1,x3,x5 = 
    u_tup3
      u_istype
      (u_namemap u_Val)
      (u_namemap u_tycon_spec) st
  new ModuleOrNamespaceType(x1,x3,x5)


(*---------------------------------------------------------------------------
 * Pickle/unpickle for F# expressions (for optimization data)
 *------------------------------------------------------------------------- *)

#if INCLUDE_METADATA_WRITER
and p_const x st = 
  match x with 
  | TConst_bool x       -> p_byte 0  st; p_bool x st
  | TConst_sbyte x       -> p_byte 1  st; p_int8 x st
  | TConst_byte x      -> p_byte 2  st; p_uint8 x st
  | TConst_int16 x      -> p_byte 3  st; p_int16 x st
  | TConst_uint16 x     -> p_byte 4  st; p_uint16 x st
  | TConst_int32 x      -> p_byte 5  st; p_int32 x st
  | TConst_uint32 x     -> p_byte 6  st; p_uint32 x st
  | TConst_int64 x      -> p_byte 7  st; p_int64 x st
  | TConst_uint64 x     -> p_byte 8  st; p_uint64 x st
  | TConst_nativeint x  -> p_byte 9  st; p_int64 x st
  | TConst_unativeint x -> p_byte 10 st; p_uint64 x st
  | TConst_float32 x    -> p_byte 11 st; p_single x st
  | TConst_float x      -> p_byte 12 st; p_int64 (bits_of_float x) st
  | TConst_char c       -> p_byte 13 st; p_char c st
  | TConst_string s     -> p_byte 14 st; p_string s st
  | TConst_unit         -> p_byte 17 st; p_void st
  | TConst_zero         -> p_byte 18 st; p_void st
  | TConst_decimal s    -> p_byte 19 st; p_array p_int32 (System.Decimal.GetBits(s)) st
#endif

and u_const st = 
  let tag = u_byte st
  match tag with
  | 0 -> u_bool st |> (fun x -> TConst_bool x) 
  | 1 -> u_int8 st |> (fun x -> TConst_sbyte x)
  | 2 -> u_uint8 st |> (fun x -> TConst_byte x)
  | 3 -> u_int16 st |> (fun x -> TConst_int16 x)
  | 4 -> u_uint16 st |> (fun x -> TConst_uint16 x)
  | 5 -> u_int32 st |> (fun x -> TConst_int32 x)
  | 6 -> u_uint32 st |> (fun x -> TConst_uint32 x)
  | 7 -> u_int64 st |> (fun x -> TConst_int64 x)
  | 8 -> u_uint64 st |> (fun x -> TConst_uint64 x)
  | 9 -> u_int64 st |> (fun x -> TConst_nativeint x)
  | 10 -> u_uint64 st |> (fun x -> TConst_unativeint x)
  | 11 -> u_single st |> (fun x -> TConst_float32 x)
  | 12 -> u_int64 st |> (fun x -> TConst_float (float_of_bits x))
  | 13 -> u_char st |> (fun x -> TConst_char x)
  | 14 -> u_string st |> (fun x -> TConst_string x)
  | 17 -> u_void st |> (fun () -> TConst_unit)
  | 18 -> u_void st |> (fun () -> TConst_zero)
  | 19 -> u_array u_int32 st |> (fun bits -> TConst_decimal (new System.Decimal(bits)))
  | _ -> ufailwith st "u_const" 


#if INCLUDE_METADATA_WRITER
and p_dtree x st = 
  match x with 
  | TDSwitch (a,b,c,d) -> p_byte 0 st; p_tup4 p_expr (p_list p_dtree_case) (p_option p_dtree) p_dummy_range (a,b,c,d) st
  | TDSuccess (a,b)    -> p_byte 1 st; p_tup2 p_FlatExprs p_int (a,b) st
  | TDBind (a,b)       -> p_byte 2 st; p_tup2 p_bind p_dtree (a,b) st

and p_dtree_case (TCase(a,b)) st = p_tup2 p_dtree_discrim p_dtree (a,b) st

and p_dtree_discrim x st = 
  match x with 
  | TTest_unionconstr (ucref,tinst) -> p_byte 1 st; p_tup2 p_ucref p_typs (ucref,tinst) st
  | TTest_const c                   -> p_byte 2 st; p_const c st
  | TTest_isnull                    -> p_byte 3 st; p_void st
  | TTest_isinst (srcty,tgty)       -> p_byte 4 st; p_typ srcty st; p_typ tgty st
  | TTest_query _                   -> pfailwith st "TTest_query: only used during pattern match compilation"
  | TTest_array_length (n,ty)       -> p_byte 6 st; p_tup2 p_int p_typ (n,ty) st

and p_target (TTarget(a,b,_)) st = p_tup2 p_FlatVals p_expr (a,b) st
and p_bind (TBind(a,b,_)) st = p_tup2 p_Val p_val_repr (a,b) st
and p_val_repr x st = p_expr x st

and p_lval_op_kind x st =
  p_int (match x with LGetAddr -> 0 | LByrefGet -> 1 | LSet -> 2 | LByrefSet -> 3) st

and p_recdInfo x st = 
    match x with 
    | RecdExprIsObjInit -> p_byte 0 st
    | RecdExpr -> p_byte 1 st


#endif
and u_dtree st = 
  let tag = u_byte st
  match tag with
  | 0 -> u_tup4 u_expr (u_list u_dtree_case) (u_option u_dtree) u_dummy_range st |> (fun (e,cases,dflt,m) -> TDSwitch (e,cases,dflt,m) ) 
  | 1 -> u_tup2 u_FlatExprs u_int st |> (fun (es,n) -> TDSuccess (es,n) )
  | 2 -> u_tup2 u_bind u_dtree st |> (fun (b,t) -> TDBind(b,t) )
  | _ -> ufailwith st "u_dtree" 

and u_dtree_case st = let a,b = u_tup2 u_dtree_discrim u_dtree st in (TCase(a,b)) 

and u_dtree_discrim st = 
  let tag = u_byte st
  match tag with
  | 1 -> u_tup2 u_ucref u_typs st |> (fun (a,b) -> TTest_unionconstr (a,b) ) 
  | 2 -> u_const st             |> (fun c -> TTest_const c ) 
  | 3 -> u_void st              |> (fun () -> TTest_isnull ) 
  | 4 -> u_tup2 u_typ u_typ st    |> (fun (srcty,tgty) -> TTest_isinst (srcty,tgty) )
  | 6 -> u_tup2 u_int u_typ st    |> (fun (n,ty)       -> TTest_array_length (n,ty) )
  | _ -> ufailwith st "u_dtree_discrim" 

and u_target st = let a,b = u_tup2 u_FlatVals u_expr st in (TTarget(a,b,SuppressSequencePointAtTarget)) 

and u_bind st = let a = u_Val st in let b = u_val_repr st in TBind(a,b,NoSequencePointAtStickyBinding)

and u_val_repr st = u_expr st

and u_lval_op_kind st =
  match (u_int st) with 0 -> LGetAddr | 1 -> LByrefGet | 2 -> LSet | 3 -> LByrefSet | _ -> ufailwith st "uval_op_kind"

and u_recdInfo st = 
    let tag = u_byte st
    match tag with 
    | 0 -> RecdExprIsObjInit 
    | 1 -> RecdExpr 
    | _ -> ufailwith st "u_recdInfo" 
  
#if INCLUDE_METADATA_WRITER
and p_op x st = 
  match x with 
  | TOp_ucase c                   -> p_byte 0 st; p_ucref c st
  | TOp_exnconstr c               -> p_byte 1 st; p_tcref "op"  c st
  | TOp_tuple                     -> p_byte 2 st
  | TOp_recd (a,b)                -> p_byte 3 st; p_tup2 p_recdInfo (p_tcref "recd op") (a,b) st
  | TOp_rfield_set (a)            -> p_byte 4 st; p_rfref a st
  | TOp_rfield_get (a)            -> p_byte 5 st; p_rfref a st
  | TOp_ucase_tag_get (a)         -> p_byte 6 st; p_tcref "cnstr op" a st
  | TOp_ucase_field_get (a,b)     -> p_byte 7 st; p_tup2 p_ucref p_int (a,b) st
  | TOp_ucase_field_set (a,b)     -> p_byte 8 st; p_tup2 p_ucref p_int (a,b) st
  | TOp_exnconstr_field_get (a,b) -> p_byte 9 st; p_tup2 (p_tcref "exn op") p_int (a,b) st
  | TOp_exnconstr_field_set (a,b) -> p_byte 10 st; p_tup2 (p_tcref "exn op")  p_int (a,b) st
  | TOp_tuple_field_get (a)       -> p_byte 11 st; p_int a st
  | TOp_asm (a,b)                 -> p_byte 12 st; p_tup2 (p_list p_ilinstr) p_typs (a,b) st
  | TOp_get_ref_lval              -> p_byte 13 st
  | TOp_ucase_proof (a)     -> p_byte 14 st; p_ucref a st
  | TOp_coerce                    -> p_byte 15 st
  | TOp_trait_call (b)            -> p_byte 16 st; p_trait b st
  | TOp_lval_op (a,b)             -> p_byte 17 st; p_tup2 p_lval_op_kind (p_vref "lval") (a,b) st
  | TOp_ilcall (a,b,c,d)          -> p_byte 18 st; p_tup4 (ptup9 p_bool p_bool p_bool p_bool p_vrefFlags p_bool p_bool (p_option (p_tup2 p_typ p_typ)) p_ilmref) p_typs p_typs p_typs (a,b,c,d) st
  | TOp_array                     -> p_byte 19 st
  | TOp_while _                   -> p_byte 20 st
  | TOp_for(_,dir)                -> p_byte 21 st; p_int (match dir with FSharpForLoopUp -> 0 | CSharpForLoopUp -> 1 | FSharpForLoopDown -> 2) st
  | TOp_bytes bytes               -> p_byte 22 st; p_bytes bytes st
  | TOp_try_catch _                -> p_byte 23 st
  | TOp_try_finally _               -> p_byte 24 st
  | TOp_field_get_addr (a)   -> p_byte 25 st; p_rfref a st
  | TOp_uint16s arr          -> p_byte 26 st; p_array p_uint16 arr st
  | TOp_rethrow              -> p_byte 27 st
  | TOp_goto _ | TOp_label _ | TOp_return -> failwith "unexpected backend construct in pickled TAST"
#endif

and u_op st = 
  let tag = u_byte st
  match tag with
  | 0 -> let a = u_ucref st
         TOp_ucase (a) 
  | 1 -> let a = u_tcref st
         TOp_exnconstr (a) 
  | 2 -> TOp_tuple 
  | 3 -> let a = u_recdInfo st
         let b = u_tcref st
         TOp_recd (a,b) 
  | 4 -> let a = u_rfref st
         TOp_rfield_set (a) 
  | 5 -> let a = u_rfref st
         TOp_rfield_get (a) 
  | 6 -> let a = u_tcref st
         TOp_ucase_tag_get (a) 
  | 7 -> let a = u_ucref st
         let b = u_int st
         TOp_ucase_field_get (a,b) 
  | 8 -> let a = u_ucref st
         let b = u_int st
         TOp_ucase_field_set (a,b) 
  | 9 -> let a = u_tcref st
         let b = u_int st
         TOp_exnconstr_field_get (a,b) 
  | 10 -> let a = u_tcref st
          let b = u_int st
          TOp_exnconstr_field_set (a,b) 
  | 11 -> let a = u_int st
          TOp_tuple_field_get (a) 
  | 12 -> let a = (u_list u_ilinstr) st
          let b = u_typs st
          TOp_asm (a,b) 
  | 13 -> TOp_get_ref_lval 
  | 14 -> let a = u_ucref st
          TOp_ucase_proof (a) 
  | 15 -> TOp_coerce
  | 16 -> let a = u_trait st
          TOp_trait_call a
  | 17 -> let a = u_lval_op_kind st
          let b = u_vref st
          TOp_lval_op (a,b) 
  | 18 -> let (a1,a2,a3,a4,a5,a6,a7,a8,a9) = (utup9 u_bool u_bool u_bool u_bool u_vrefFlags u_bool u_bool (u_option (u_tup2 u_typ u_typ)) u_ilmref) st
          let b = u_typs st
          let c = u_typs st
          let d = u_typs st
          TOp_ilcall ((a1,a2,a3,a4,a5,a6,a7,a8,a9),b,c,d) 
  | 19 -> TOp_array
  | 20 -> TOp_while NoSequencePointAtWhileLoop
  | 21 -> let dir = match u_int st with 0 -> FSharpForLoopUp | 1 -> CSharpForLoopUp | 2 -> FSharpForLoopDown | _ -> failwith "unknown for loop"
          TOp_for (NoSequencePointAtForLoop, dir)
  | 22 -> TOp_bytes (u_bytes st)
  | 23 -> TOp_try_catch(NoSequencePointAtTry,NoSequencePointAtWith)
  | 24 -> TOp_try_finally(NoSequencePointAtTry,NoSequencePointAtFinally)
  | 25 -> let a = u_rfref st
          TOp_field_get_addr (a) 
  | 26 -> TOp_uint16s (u_array u_uint16 st)
  | 27 -> TOp_rethrow
  | _ -> ufailwith st "u_op" 

#if INCLUDE_METADATA_WRITER
and p_expr expr st = 
(* try *)
  match expr with 
  | TExpr_link e -> p_expr !e st
  | TExpr_const (x,m,ty)                -> p_byte 0 st; p_tup3 p_const p_dummy_range p_typ (x,m,ty) st
  | TExpr_val (a,b,m)                   -> p_byte 1 st; p_tup3 (p_vref "val") p_vrefFlags p_dummy_range (a,b,m) st
  | TExpr_op(a,b,c,d)                   -> p_byte 2 st; p_tup4 p_op  p_typs p_Exprs p_dummy_range (a,b,c,d) st
  | TExpr_seq (a,b,c,_,d)                 -> p_byte 6 st; p_tup4 p_expr p_expr p_int p_dummy_range (a,b,(match c with NormalSeq -> 0 | ThenDoSeq -> 1),d) st
  | TExpr_lambda (a,b0,b1,c,d,e,_)      -> p_byte 9 st; p_tup5 (p_option p_Val) p_Vals p_expr p_dummy_range p_typ (b0,b1,c,d,e) st
  | TExpr_tlambda (a,b,c,d,e,_)         -> p_byte 10 st; p_tup4 p_typar_specs p_expr p_dummy_range p_typ (b,c,d,e) st
  | TExpr_app (a1,a2,b,c,d)             -> p_byte 11 st; p_tup5 p_expr p_typ p_typs p_Exprs p_dummy_range (a1,a2,b,c,d) st
  | TExpr_letrec (a,b,c,_)              -> p_byte 12 st; p_tup3 p_binds p_expr p_dummy_range (a,b,c) st
  | TExpr_let (a,b,c,_)                 -> p_byte 13 st; p_tup3 p_bind p_expr p_dummy_range (a,b,c) st
  | TExpr_match (_,a,b,c,d,e,_)           -> p_byte 14 st; p_tup5 p_dummy_range p_dtree p_targets p_dummy_range p_typ (a,b,c,d,e) st
  | TExpr_obj(a,b,c,d,e,f,g,_)          -> p_byte 21 st; p_tup6 p_typ (p_option p_Val) p_expr p_methods p_intfs p_dummy_range (b,c,d,e,f,g) st
  | TExpr_static_optimization(a,b,c,d)  -> p_byte 22 st; p_tup4 p_constraints p_expr p_expr p_dummy_range (a,b,c,d) st
  | TExpr_tchoose (a,b,c)               -> p_byte 25 st; p_tup3 p_typar_specs p_expr p_dummy_range (a,b,c) st
  | TExpr_quote(ast,_,m,ty)             -> p_byte 26 st; p_tup3 p_expr p_dummy_range p_typ (ast,m,ty) st
#endif

(*
with Nope -> 
   dprintf "\nloc: %a\nexpr: %s\n\n" output_range (Tastops.range_of_expr expr) (Layout.showL (Tastops.ExprL expr));
   stdout.Flush();
   raise Nope
*)
and u_expr st = 
  let tag = u_byte st
  match tag with
  | 0 -> let a = u_const st
         let b = u_dummy_range st
         let c = u_typ st
         TExpr_const (a,b,c) 
  | 1 -> let a = u_vref st
         let b = u_vrefFlags st
         let c = u_dummy_range st
         TExpr_val (a,b,c) 
  | 2 -> let a = u_op st
         let b = u_typs st
         let c = u_Exprs st
         let d = u_dummy_range st
         TExpr_op (a,b,c,d)
  | 6 -> let a = u_expr st
         let b = u_expr st
         let c = u_int st
         let d = u_dummy_range  st
         TExpr_seq (a,b,(match c with 0 -> NormalSeq | 1 -> ThenDoSeq | _ -> ufailwith st "specialSeqFlag"),SuppressSequencePointOnExprOfSequential,d) 
  | 9 -> let b0 = u_option u_Val st
         let b1 = u_Vals st
         let c = u_expr st
         let d = u_dummy_range st
         let e = u_typ st
         TExpr_lambda (new_uniq(),b0,b1,c,d,e,SkipFreeVarsCache()) 
  | 10 -> let b = u_typar_specs st
          let c = u_expr st
          let d = u_dummy_range st
          let e = u_typ st
          TExpr_tlambda (new_uniq(),b,c,d,e,SkipFreeVarsCache()) 
  | 11 -> let a1 = u_expr st
          let a2 = u_typ st
          let b = u_typs st
          let c = u_Exprs st
          let d = u_dummy_range st
          TExpr_app (a1,a2,b,c,d) 
  | 12 -> let a = u_binds st
          let b = u_expr st
          let c = u_dummy_range st
          TExpr_letrec (a,b,c,NewFreeVarsCache()) 
  | 13 -> let a = u_bind st
          let b = u_expr st
          let c = u_dummy_range st
          TExpr_let (a,b,c,NewFreeVarsCache()) 
  | 14 -> let a = u_dummy_range st
          let b = u_dtree st
          let c = u_targets st
          let d = u_dummy_range st
          let e = u_typ st
          TExpr_match (NoSequencePointAtStickyBinding,a,b,c,d,e,SkipFreeVarsCache()) 
  | 21 -> let b = u_typ st
          let c = (u_option u_Val) st
          let d = u_expr st
          let e = u_methods st
          let f = u_intfs st
          let g = u_dummy_range st
          TExpr_obj (new_uniq(),b,c,d,e,f,g,SkipFreeVarsCache())
  | 22 -> let a = u_constraints st
          let b = u_expr st
          let c = u_expr st
          let d = u_dummy_range st
          TExpr_static_optimization (a,b,c,d)
  | 25 -> let a = u_typar_specs st
          let b = u_expr st
          let c = u_dummy_range st
          TExpr_tchoose (a,b,c)
  | 26 -> let b = u_expr st
          let c = u_dummy_range st
          let d = u_typ st
          TExpr_quote (b,ref None,c,d)
  | _ -> ufailwith st "u_expr" 

#if INCLUDE_METADATA_WRITER
and p_arg x st = p_tup2 p_expr p_typ x st
and p_constraint x st = 
  match x with
  | TTyconEqualsTycon (a,b) -> p_byte 0 st; p_tup2 p_typ p_typ (a,b) st

and p_slotparam (TSlotParam (a,b,c,d,e,f)) st = p_tup6 (p_option p_string) p_typ p_bool p_bool p_bool p_attribs (a,b,c,d,e,f) st
and p_slotsig (TSlotSig (a,b,c,d,e,f)) st = p_tup6 p_string p_typ p_typar_specs p_typar_specs (p_list (p_list p_slotparam)) (p_option p_typ) (a,b,c,d,e,f) st
and p_method (TObjExprMethod (a,b,c,d,e)) st = p_tup5 p_slotsig p_typar_specs (p_list p_Vals) p_expr p_range (a,b,c,d,e) st 
and p_methods x st = p_list p_method x st
and p_intf x st = p_tup2 p_typ p_methods x st
and p_intfs x st = p_list p_intf x st
#endif

and u_arg st = u_tup2 u_expr u_typ st

and u_constraint st = 
  let tag = u_byte st
  match tag with
  | 0 -> u_tup2 u_typ u_typ st |> (fun (a,b) -> TTyconEqualsTycon(a,b) ) 
  | _ -> ufailwith st "u_constraint" 

and u_slotparam st = 
    let a,b,c,d,e,f = u_tup6 (u_option u_string) u_typ u_bool u_bool u_bool u_attribs st 
    TSlotParam(a,b,c,d,e,f)

and u_slotsig st = 
    let a,b,c,d,e,f = u_tup6 u_string u_typ u_typar_specs u_typar_specs (u_list (u_list u_slotparam)) (u_option u_typ) st
    TSlotSig(a,b,c,d,e,f)

and u_method st = 
    let a,b,c,d,e = u_tup5 u_slotsig u_typar_specs (u_list u_Vals) u_expr u_range st 
    TObjExprMethod(a,b,c,d,e)

and u_methods st = u_list u_method st

and u_intf st = u_tup2 u_typ u_methods st

and u_intfs st = u_list u_intf st

#if INCLUDE_METADATA_WRITER
let _ = fill_p_binds (p_FlatList p_bind);;
let _ = fill_p_targets (p_array p_target);;
let _ = fill_p_constraints (p_list p_constraint);;
let _ = fill_p_Exprs (p_list p_expr);;
let _ = fill_p_FlatExprs (p_FlatList p_expr);;
let _ = fill_p_attribs (p_list p_attrib);;
let _ = fill_p_Vals (p_list p_Val);;
let _ = fill_p_FlatVals (p_FlatList p_Val);;
#endif

let _ = fill_u_binds (u_FlatList u_bind);;
let _ = fill_u_targets (u_array u_target);;
let _ = fill_u_constraints (u_list u_constraint);;
let _ = fill_u_Exprs (u_list u_expr);;
let _ = fill_u_FlatExprs (u_FlatList u_expr);;
let _ = fill_u_attribs (u_list u_attrib);;
let _ = fill_u_Vals (u_list u_Val);;
let _ = fill_u_FlatVals (u_FlatList u_Val);;

(*---------------------------------------------------------------------------
 * Pickle/unpickle F# interface data 
 *------------------------------------------------------------------------- *)

#if INCLUDE_METADATA_WRITER
let pickle_modul_spec mspec st = p_tycon_spec mspec st
let PickleModuleInfo minfo st = p_tup3 pickle_modul_spec p_string p_bool (minfo.mspec, minfo.compile_time_working_dir, minfo.usesQuotations) st
#endif

let unpickle_modul_spec st = u_tycon_spec st 
  
let UnpickleModuleInfo st = let a,b,c = u_tup3 unpickle_modul_spec u_string u_bool st in { mspec=a; compile_time_working_dir=b; usesQuotations=c }
