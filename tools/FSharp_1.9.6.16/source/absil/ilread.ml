// (c) Microsoft Corporation 2005-2009. 
#light

//---------------------------------------------------------------------
// The big binary reader
//
//---------------------------------------------------------------------

module Microsoft.FSharp.Compiler.AbstractIL.BinaryReader 

open System.IO
open System.Collections.Generic
open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Support 
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
open Microsoft.FSharp.Compiler.AbstractIL.Internal.BinaryConstants 
open Microsoft.FSharp.Compiler.AbstractIL.IL  
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Range

type readerOptions =
    { pdbPath: string option;
      ilGlobals: ILGlobals;
      optimizeForMemory: bool }

let report_ref = ref (fun oc -> ()) 
let add_report f = let old = !report_ref in report_ref := (fun oc -> old oc; f oc) 
let report (oc:TextWriter) = !report_ref oc

let logging = false
let checking = false  
let _ = if checking then dprintn "warning : Ilread.checking is on"

let empty_custom_attrs = mk_custom_attrs []

/// Read file from memory mapped files
module MMap = 

    open System
    open System.IO
    open System.Runtime.InteropServices
    open Microsoft.FSharp.NativeInterop

    type HANDLE = nativeint
    type ADDR   = nativeint
    type SIZE_T = nativeint

    [<DllImport("kernel32", SetLastError=true)>]
    extern bool CloseHandle (HANDLE handler)

    [<DllImport("kernel32", SetLastError=true, CharSet=CharSet.Unicode)>]
    extern HANDLE CreateFile (string lpFileName, 
                              int dwDesiredAccess, 
                              int dwShareMode,
                              HANDLE lpSecurityAttributes, 
                              int dwCreationDisposition,
                              int dwFlagsAndAttributes, 
                              HANDLE hTemplateFile)
             
    [<DllImport("kernel32", SetLastError=true, CharSet=CharSet.Unicode)>]
    extern HANDLE CreateFileMapping (HANDLE hFile, 
                                     HANDLE lpAttributes, 
                                     int flProtect, 
                                     int dwMaximumSizeLow, 
                                     int dwMaximumSizeHigh,
                                     string lpName) 

    [<DllImport("kernel32", SetLastError=true)>]
    extern ADDR MapViewOfFile (HANDLE hFileMappingObject, 
                               int    dwDesiredAccess, 
                               int    dwFileOffsetHigh,
                               int    dwFileOffsetLow, 
                               SIZE_T dwNumBytesToMap)

    [<DllImport("kernel32", SetLastError=true)>]
    extern bool UnmapViewOfFile (ADDR lpBaseAddress)

    let INVALID_HANDLE = new IntPtr(-1)
    let MAP_READ    = 0x0004
    let GENERIC_READ = 0x80000000
    let NULL_HANDLE = IntPtr.Zero
    let FILE_SHARE_NONE = 0x0000
    let FILE_SHARE_READ = 0x0001
    let FILE_SHARE_WRITE = 0x0002
    let FILE_SHARE_READ_WRITE = 0x0003
    let CREATE_ALWAYS  = 0x0002
    let OPEN_EXISTING   = 0x0003
    let OPEN_ALWAYS  = 0x0004

    type mmap = { hMap: HANDLE; start:nativeint }

    let create fileName  =
        //printf "fileName = %s\n" fileName;
        let hFile = CreateFile (fileName, GENERIC_READ, FILE_SHARE_READ_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero  )
        //printf "hFile = %Lx\n" (hFile.ToInt64());
        if ( hFile.Equals(INVALID_HANDLE) ) then
            failwithf "CreateFile(0x%08x)" ( Marshal.GetHRForLastWin32Error() );
        let protection = 0x00000002 (* ReadOnly *)
        //printf "OK! hFile = %Lx\n" (hFile.ToInt64());
        let hMap = CreateFileMapping (hFile, IntPtr.Zero, protection, 0,0, null )
        ignore(CloseHandle(hFile));
        if hMap.Equals(NULL_HANDLE) then
            failwithf "CreateFileMapping(0x%08x)" ( Marshal.GetHRForLastWin32Error() );

        let start = MapViewOfFile (hMap, MAP_READ,0,0,0n)

        if ( start.Equals(IntPtr.Zero) ) then
           failwithf "MapViewOfFile(0x%08x)" ( Marshal.GetHRForLastWin32Error() );
        { hMap = hMap; start = start; }

    let addr m (i:int) = 
        m.start + nativeint i

    let deref_byte (p:nativeint) = 
        NativePtr.read (NativePtr.of_nativeint<byte> p) |> int32

    let read_byte m i = 
        deref_byte (addr m i)

    let read_bytes m i len = 
        let res = Bytes.zero_create len
        Marshal.Copy(addr m i, res, 0,len);
        res
      
    let read_i32 m i = 
        NativePtr.read (NativePtr.of_nativeint<int32> (addr m i)) 

    let read_u16_as_i32 m i = 
        NativePtr.read (NativePtr.of_nativeint<uint16> (addr m i)) |> int32

    let close m = 
        ignore(UnmapViewOfFile ( m.start ));
        ignore(CloseHandle ( m.hMap ))

    let count_utf8_string m i = 
        let start = addr m i  
        let mutable p = start 
        while deref_byte p <> 0 do
            p <- p+1n
        int (p-start)

    let read_utf8_string m i = 
        let n = count_utf8_string m i
        new System.String(NativePtr.of_nativeint (addr m i), 0, n, System.Text.Encoding.UTF8)


module MMapChannel = 

    type t = 
      { mutable mmPos: int;
        mmMap: MMap.mmap }

    let open_in f = 
      let mmap = MMap.create f
      { mmPos = 0; mmMap = mmap }

    let input_byte mc = 
      let res = MMap.read_byte mc.mmMap mc.mmPos
      mc.mmPos <- mc.mmPos + 1;
      res

    let input_i32 mc = 
      let res = MMap.read_i32 mc.mmMap mc.mmPos
      mc.mmPos <- mc.mmPos + 4;
      res

    let input_u16_as_i32 mc = 
      let res = MMap.read_u16_as_i32 mc.mmMap mc.mmPos
      mc.mmPos <- mc.mmPos + 2;
      res

    let input_bytes mc len = 
      let res = MMap.read_bytes mc.mmMap mc.mmPos len
      mc.mmPos <- mc.mmPos + len;
      res

    let peek_utf8_string mc = 
      MMap.read_utf8_string mc.mmMap mc.mmPos 

    let seek mc addr = mc.mmPos <- addr
    let close mc = MMap.close mc.mmMap

/// Read file into cached memory blocks to avoid taking any kind of a
/// lock on the file, and avoid needing any cleanup of files.
module MemChannel = 

    type mem_in_channel = 
        { mutable mcPos: int;
          mcBlocks: byte[] }

    let open_in f = 
        let mcBlocks = System.IO.File.ReadAllBytes f
        { mcPos = 0; mcBlocks = mcBlocks }

    let input_byte mc = 
        let p = mc.mcPos
        let res = int mc.mcBlocks.[p]
        mc.mcPos <- p + 1;
        res

    let input_bytes mc len = 
        let p = mc.mcPos
        let res = Array.sub mc.mcBlocks p len
        mc.mcPos <- p + len;
        res

    let seek mc addr = 
        mc.mcPos <- addr


(*---------------------------------------------------------------------
 * Read file from cached memory blocks or via 'seek'
 *---------------------------------------------------------------------*)

type input = 
    /// REVIEW: use a BinaryReader directly 
    | Chan of string * in_channel option ref
    | Mem of MemChannel.mem_in_channel
    | MMap of MMapChannel.t 

let input_byte is = 
    match is with 
    | Chan (_,{contents=Some ic}) -> Pervasives.input_byte ic
    | Chan (s,_) -> failwith ("input_byte: input channel "^s^" was closed" )
    | Mem mc -> MemChannel.input_byte mc
    | MMap mc -> MMapChannel.input_byte mc 

let seek is addr = 
    match is with 
    | Chan (_,{contents=Some ic}) -> Pervasives.seek_in ic ( addr)
    | Chan (s,_) -> failwith ("seek: input channel "^s^" was closed" )
    | Mem mc -> MemChannel.seek mc ( addr)
    | MMap mc -> MMapChannel.seek mc ( addr) 

let read_bytes is len = 
    match is with 
    | Chan (_,{contents=Some ic}) -> Bytes.really_input ic ( len)
    | Chan (s,_) -> failwith ("read_bytes: input channel "^s^" was closed" )
    | Mem mc -> MemChannel.input_bytes mc ( len)
    | MMap mc -> MMapChannel.input_bytes mc ( len) 

let read_i64 is = 
    let b0 = input_byte is
    let b1 = input_byte is
    let b2 = input_byte is
    let b3 = input_byte is
    let b4 = input_byte is
    let b5 = input_byte is
    let b6 = input_byte is
    let b7 = input_byte is
    int64 b0 ||| (int64 b1 <<< 8) ||| (int64 b2 <<< 16) ||| (int64 b3 <<< 24) |||
    (int64 b4 <<< 32) ||| (int64 b5 <<< 40) ||| (int64 b6 <<< 48) ||| (int64 b7 <<< 56)

let read_i32 is = 
    match is with 
    | Chan (_,{contents=Some ic}) -> Pervasives.input_binary_int ic
    | Chan (s,_) -> failwith ("read_bytes: input channel "^s^" was closed" )
    | MMap mm -> MMapChannel.input_i32 mm 
    | Mem _ ->
       let b0 = input_byte is
       let b1 = input_byte is
       let b2 = input_byte is
       let b3 = input_byte is
       b0 ||| (b1 <<< 8) ||| (b2 <<< 16) ||| (b3 <<< 24)


(*---------------------------------------------------------------------
 * Derived reading
 *---------------------------------------------------------------------*)

let read_u16_as_i32 is = 
    match is with 
    | MMap mm -> MMapChannel.input_u16_as_i32 mm 
    | _ ->
        let b0 = input_byte is
        let b1 = input_byte is
        b0 ||| (b1 <<< 8) 
    
    
let read_u16 is = uint16 (read_u16_as_i32 is)
    
let read_u8_as_i32 is = 
    let b0 = input_byte is
    b0

let read_u8_as_u16 is = uint16 (read_u8_as_i32 is)
    
let read_i8 is = 
    let b0 = input_byte is
    sbyte (byte b0)
  

let read_i8_as_i32 is = int32 (read_i8 is)
    
let float32_of_bits (x:int32) = System.BitConverter.ToSingle(System.BitConverter.GetBytes(x),0)
let float_of_bits (x:int64) = System.BitConverter.Int64BitsToDouble(x)

let read_ieee32 is = float32_of_bits (read_i32 is)
let read_ieee64 is = float_of_bits (read_i64 is)
    
let read_z_unsigned_int32 is = 
    let b0 = read_u8_as_i32 is
    if b0 <= 0x7F then b0 
    elif b0 <= 0xbf then 
        let b0 = b0 &&& 0x7f
        let b1 = read_u8_as_i32 is
        (b0 <<< 8) ||| b1
    else 
        let b0 = b0 &&& 0x3f
        let b1 = read_u8_as_i32 is
        let b2 = read_u8_as_i32 is
        let b3 = read_u8_as_i32 is
        (b0 <<< 24) ||| (b1 <<< 16) ||| (b2 <<< 8) ||| b3

let SeekReadInt32         is addr = seek is addr; read_i32 is
let SeekReadInt64         is addr = seek is addr; read_i64 is
let SeekReadUInt16AsInt32 is addr = seek is addr; read_u16_as_i32 is
let SeekReadUInt16        is addr = seek is addr; read_u16 is
let SeekReadByteAsInt32   is addr = seek is addr; read_u8_as_i32 is 
let SeekReadByteAsUInt16  is addr = seek is addr; read_u8_as_u16 is
let SeekReadSByte         is addr = seek is addr; read_i8 is 
let SeekReadSByteAsInt32  is addr = seek is addr; read_i8_as_i32 is
let SeekReadSingle        is addr = seek is addr; read_ieee32 is
let SeekReadDouble        is addr = seek is addr; read_ieee64 is
let SeekReadBytes         is addr len = seek is addr; read_bytes is len
    
let rec count_utf8_string is n = 
    let c = input_byte is
    if c = 0 then n 
    else count_utf8_string is (n+1)

let SeekReadUTF8String is addr = 
    seek is addr;
    match is with 
    | MMap mc -> 
      // optimized implementation 
      MMapChannel.peek_utf8_string mc
    | _ -> 
      let n = count_utf8_string is 0
      let bytes = SeekReadBytes is addr (n)
      Bytes.utf8_bytes_as_string bytes

let ReadBlob is = 
    let len = read_z_unsigned_int32 is
    read_bytes is len
    
let SeekReadBlob is addr = 
    seek is addr;
    ReadBlob is
    
let ReadUserString is = 
    let len = read_z_unsigned_int32 is
    Bytes.unicode_bytes_as_string (read_bytes is (len - 1))
    
let SeekReadUserString is addr = 
    seek is addr;
    ReadUserString is
    
let ReadGuid is = 
    read_bytes is 0x10
    
let SeakReadGuid is addr = 
    seek is addr;
    ReadGuid is
    
(*---------------------------------------------------------------------
 * Utilities.  
 *---------------------------------------------------------------------*)

let align alignment n = ((n + alignment - 0x1) / alignment) * alignment

let uncoded_token tab idx = (((tag_of_table tab) <<< 24) ||| idx)

let i32_to_uncoded_token tok  = 
    let idx = tok &&& 0xffffff
    let tab = tok lsr 24
    (Table ( tab),  idx)

let read_uncoded_token is  = i32_to_uncoded_token (read_i32 is)

#nowarn "65"

type TaggedIndex<'a> = 
    struct
        val tag: 'a
        val index : int32
        new(tag,index) = { tag=tag; index=index }
    end


let uncoded_token_to_tdor (tab,tok) = 
    let tag =
      if tab = tab_TypeDef then tdor_TypeDef 
      elif tab = tab_TypeRef then tdor_TypeRef
      elif tab = tab_TypeSpec then tdor_TypeSpec
      else failwith "bad table in uncoded_token_to_tdor" 
    TaggedIndex(tag,tok)

let uncoded_token_to_mdor (tab,tok) = 
    let tag =
      if tab = tab_Method then mdor_MethodDef 
      elif tab = tab_MemberRef then mdor_MemberRef
      else failwith "bad table in uncoded_token_to_mdor" 
    TaggedIndex(tag,tok)

let SeekReaduncoded_token is addr  = 
    seek is addr;
    read_uncoded_token is

let (|TaggedIndex|) (x:TaggedIndex<'a>) = x.tag, x.index    
let read_z_tagged_idx f nbits big is = 
    let tok = if big then read_i32 is else read_u16_as_i32 is
    let tagmask = 
      if nbits = 1 then 1 
      elif nbits = 2 then 3 
      elif nbits = 3 then 7 
      elif nbits = 4 then 15 
         elif nbits = 5 then 31 
         else failwith "too many nbits"
    let tag = tok &&& tagmask
    let idx = tok lsr nbits
    TaggedIndex(f tag, idx) 
       
(*---------------------------------------------------------------------
 * Primitives to help read signatures.  These do not use the file cursor, but
 * pass ar
 *---------------------------------------------------------------------*)

let sigptr_check (bytes:byte[]) sigptr = 
    if checking && sigptr >= bytes.Length then failwith "read past end of sig. "

let sigptr_get_byte (bytes:byte[]) sigptr = 
    sigptr_check bytes sigptr;
    int(bytes.[sigptr]), sigptr + 1

let sigptr_get_bool bytes sigptr = 
    let b0,sigptr = sigptr_get_byte bytes sigptr
    (b0 = 0x01) ,sigptr

let sigptr_get_u8 bytes sigptr = 
    let b0,sigptr = sigptr_get_byte bytes sigptr
    byte b0,sigptr

let sigptr_get_i8 bytes sigptr = 
    let i,sigptr = sigptr_get_u8 bytes sigptr
    sbyte i,sigptr

let sigptr_get_u16 bytes sigptr = 
    let b0,sigptr = sigptr_get_byte bytes sigptr
    let b1,sigptr = sigptr_get_byte bytes sigptr
    uint16 (b0 ||| (b1 <<< 8)),sigptr

let sigptr_get_i16 bytes sigptr = 
    let u,sigptr = sigptr_get_u16 bytes sigptr
    int16 u,sigptr

let sigptr_get_i32 bytes sigptr = 
    sigptr_check bytes sigptr;
    let b0 = int bytes.[sigptr]
    let b1 = int bytes.[sigptr+1]
    let b2 = int bytes.[sigptr+2]
    let b3 = int bytes.[sigptr+3]
    let res = b0 ||| (b1 <<< 8) ||| (b2 <<< 16) ||| (b3 <<< 24)
    res, sigptr + 4

let sigptr_get_u32 bytes sigptr = 
    let u,sigptr = sigptr_get_i32 bytes sigptr
    uint32 u,sigptr

let sigptr_get_u64 bytes sigptr = 
    let u0,sigptr = sigptr_get_u32 bytes sigptr
    let u1,sigptr = sigptr_get_u32 bytes sigptr
    (uint64 u0 ||| (uint64 u1 <<< 32)),sigptr

let sigptr_get_i64 bytes sigptr = 
    let u,sigptr = sigptr_get_u64 bytes sigptr
    int64 u,sigptr

let sigptr_get_ieee32 bytes sigptr = 
    let u,sigptr = sigptr_get_i32 bytes sigptr
    float32_of_bits u,sigptr

let sigptr_get_ieee64 bytes sigptr = 
    let u,sigptr = sigptr_get_i64 bytes sigptr
    float_of_bits u,sigptr

let sigptr_get_z_i32 bytes sigptr = 
  let b0,sigptr = sigptr_get_byte bytes sigptr
  if b0 <= 0x7F then b0, sigptr
  elif b0 <= 0xbf then 
      let b0 = b0 &&& 0x7f
      let b1,sigptr = sigptr_get_byte bytes sigptr
      (b0 <<< 8) ||| b1, sigptr
  else 
      let b0 = b0 &&& 0x3f
      let b1,sigptr = sigptr_get_byte bytes sigptr
      let b2,sigptr = sigptr_get_byte bytes sigptr
      let b3,sigptr = sigptr_get_byte bytes sigptr
      (b0 <<< 24) ||| (b1 <<< 16) ||| (b2 <<< 8) ||| b3, sigptr
         

let rec sigptr_foldi_acc f n (bytes:byte[]) (sigptr:int) i acc = 
  if i < n then 
    let x,sp = f bytes sigptr
    sigptr_foldi_acc f n bytes sp (i+1) (x::acc)
  else 
    List.rev acc, sigptr

let sigptr_foldi f n (bytes:byte[]) (sigptr:int) = 
  sigptr_foldi_acc f n bytes sigptr 0 []


let sigptr_get_bytes n bytes sigptr = 
  if checking && sigptr + n >= Bytes.length bytes then 
      dprintn "read past end of sig. in sigptr_get_string"; 
      Bytes.zero_create 0, sigptr
  else 
      let res = Bytes.zero_create n
      for i = 0 to (n - 1) do 
          Bytes.set res i (Bytes.get bytes (sigptr + i))
      res, sigptr + n

let sigptr_get_string n bytes sigptr = 
    let bytearray,sigptr = sigptr_get_bytes n bytes sigptr
    Bytes.utf8_bytes_as_string bytearray,sigptr
   

(* -------------------------------------------------------------------- 
 * Now the tables of instructions
 * -------------------------------------------------------------------- *)

[<StructuralEquality(false); StructuralComparison(false)>]
type prefixes = 
 { mutable al:Alignment; 
   mutable tl:Tailcall;
   mutable vol:Volatility;
   mutable ro:ReadonlySpec;
   mutable constrained: ILType option}
 
let no_prefixes mk prefixes = 
    if prefixes.al <> Aligned then failwith "an unaligned prefix is not allowed here";
    if prefixes.vol <> Nonvolatile then failwith "a volatile prefix is not allowed here";
    if prefixes.tl <> Normalcall then failwith "a tailcall prefix is not allowed here";
    if prefixes.ro <> NormalAddress then failwith "a readonly prefix is not allowed here";
    if prefixes.constrained <> None then failwith "a constrained prefix is not allowed here";
    mk 

let volatile_unaligned_prefix mk prefixes = 
    if prefixes.tl <> Normalcall then failwith "a tailcall prefix is not allowed here";
    if prefixes.constrained <> None then failwith "a constrained prefix is not allowed here";
    if prefixes.ro <> NormalAddress then failwith "a readonly prefix is not allowed here";
    mk (prefixes.al,prefixes.vol) 

let volatile_prefix mk prefixes = 
    if prefixes.al <> Aligned then failwith "an unaligned prefix is not allowed here";
    if prefixes.tl <> Normalcall then failwith "a tailcall prefix is not allowed here";
    if prefixes.constrained <> None then failwith "a constrained prefix is not allowed here";
    if prefixes.ro <> NormalAddress then failwith "a readonly prefix is not allowed here";
    mk prefixes.vol

let tail_prefix mk prefixes = 
    if prefixes.al <> Aligned then failwith "an unaligned prefix is not allowed here";
    if prefixes.vol <> Nonvolatile then failwith "a volatile prefix is not allowed here";
    if prefixes.constrained <> None then failwith "a constrained prefix is not allowed here";
    if prefixes.ro <> NormalAddress then failwith "a readonly prefix is not allowed here";
    mk prefixes.tl 

let constraint_tail_prefix mk prefixes = 
    if prefixes.al <> Aligned then failwith "an unaligned prefix is not allowed here";
    if prefixes.vol <> Nonvolatile then failwith "a volatile prefix is not allowed here";
    if prefixes.ro <> NormalAddress then failwith "a readonly prefix is not allowed here";
    mk (prefixes.constrained,prefixes.tl )

let readonly_prefix mk prefixes = 
    if prefixes.al <> Aligned then failwith "an unaligned prefix is not allowed here";
    if prefixes.vol <> Nonvolatile then failwith "a volatile prefix is not allowed here";
    if prefixes.tl <> Normalcall then failwith "a tailcall prefix is not allowed here";
    if prefixes.constrained <> None then failwith "a constrained prefix is not allowed here";
    mk prefixes.ro


[<StructuralEquality(false); StructuralComparison(false)>]
type instr_decoder = 
    | I_u16_u8_instr of (prefixes -> uint16 -> ILInstr)
    | I_u16_u16_instr of (prefixes -> uint16 -> ILInstr)
    | I_none_instr of (prefixes -> ILInstr)
    | I_i64_instr of (prefixes -> int64 -> ILInstr)
    | I_i32_i32_instr of (prefixes -> int32 -> ILInstr)
    | I_i32_i8_instr of (prefixes -> int32 -> ILInstr)
    | I_r4_instr of (prefixes -> single -> ILInstr)
    | I_r8_instr of (prefixes -> double -> ILInstr)
    | I_field_instr of (prefixes -> ILFieldSpec -> ILInstr)
    | I_method_instr of (prefixes -> ILMethodSpec * varargs -> ILInstr)
    | I_unconditional_i32_instr of (prefixes -> ILCodeLabel  -> ILInstr)
    | I_unconditional_i8_instr of (prefixes -> ILCodeLabel  -> ILInstr)
    | I_conditional_i32_instr of (prefixes -> ILCodeLabel * ILCodeLabel -> ILInstr)
    | I_conditional_i8_instr of (prefixes -> ILCodeLabel * ILCodeLabel -> ILInstr)
    | I_string_instr of (prefixes -> string -> ILInstr)
    | I_switch_instr of (prefixes -> ILCodeLabel list * ILCodeLabel -> ILInstr)
    | I_tok_instr of (prefixes -> ILTokenSpec -> ILInstr)
    | I_sig_instr of (prefixes -> ILCallingSignature * varargs -> ILInstr)
    | I_type_instr of (prefixes -> ILType -> ILInstr)
    | I_invalid_instr

let mk_stind dt = volatile_unaligned_prefix (fun (x,y) -> I_stind(x,y,dt))
let mk_ldind dt = volatile_unaligned_prefix (fun (x,y) -> I_ldind(x,y,dt))

let instrs () = 
 [ i_ldarg_s, I_u16_u8_instr (no_prefixes (fun x -> I_ldarg x));
   i_starg_s, I_u16_u8_instr (no_prefixes (fun x -> I_starg x));
   i_ldarga_s, I_u16_u8_instr (no_prefixes (fun x -> I_ldarga x));
   i_stloc_s, I_u16_u8_instr (no_prefixes (fun x -> I_stloc x));
   i_ldloc_s, I_u16_u8_instr (no_prefixes (fun x -> I_ldloc x));
   i_ldloca_s, I_u16_u8_instr (no_prefixes (fun x -> I_ldloca x));
   i_ldarg, I_u16_u16_instr (no_prefixes (fun x -> I_ldarg x));
   i_starg, I_u16_u16_instr (no_prefixes (fun x -> I_starg x));
   i_ldarga, I_u16_u16_instr (no_prefixes (fun x -> I_ldarga x));
   i_stloc, I_u16_u16_instr (no_prefixes (fun x -> I_stloc x));
   i_ldloc, I_u16_u16_instr (no_prefixes (fun x -> I_ldloc x));
   i_ldloca, I_u16_u16_instr (no_prefixes (fun x -> I_ldloca x)); 
   i_stind_i, I_none_instr (mk_stind            DT_I);
   i_stind_i1, I_none_instr (mk_stind           DT_I1);
   i_stind_i2, I_none_instr (mk_stind           DT_I2);
   i_stind_i4, I_none_instr (mk_stind           DT_I4);
   i_stind_i8, I_none_instr (mk_stind           DT_I8);
   i_stind_r4, I_none_instr (mk_stind           DT_R4);
   i_stind_r8, I_none_instr (mk_stind           DT_R8);
   i_stind_ref, I_none_instr (mk_stind          DT_REF);
   i_ldind_i, I_none_instr (mk_ldind            DT_I);
   i_ldind_i1, I_none_instr (mk_ldind           DT_I1);
   i_ldind_i2, I_none_instr (mk_ldind           DT_I2);
   i_ldind_i4, I_none_instr (mk_ldind           DT_I4);
   i_ldind_i8, I_none_instr (mk_ldind           DT_I8);
   i_ldind_u1, I_none_instr (mk_ldind           DT_U1);
   i_ldind_u2, I_none_instr (mk_ldind           DT_U2);
   i_ldind_u4, I_none_instr (mk_ldind           DT_U4);
   i_ldind_r4, I_none_instr (mk_ldind           DT_R4);
   i_ldind_r8, I_none_instr (mk_ldind           DT_R8);
   i_ldind_ref, I_none_instr (mk_ldind          DT_REF);
   i_cpblk, I_none_instr (volatile_unaligned_prefix (fun (x,y) -> I_cpblk(x,y)));
   i_initblk, I_none_instr (volatile_unaligned_prefix (fun (x,y) -> I_initblk(x,y))); 
   i_ldc_i8, I_i64_instr (no_prefixes (fun x ->I_arith (AI_ldc (DT_I8, NUM_I8 x)))); 
   i_ldc_i4, I_i32_i32_instr (no_prefixes (fun x -> ((mk_ldc_i32 x))));
   i_ldc_i4_s, I_i32_i8_instr (no_prefixes (fun x -> ((mk_ldc_i32 x))));
   i_ldc_r4, I_r4_instr (no_prefixes (fun x -> I_arith (AI_ldc (DT_R4, NUM_R4 x)))); 
   i_ldc_r8, I_r8_instr (no_prefixes (fun x -> I_arith (AI_ldc (DT_R8, NUM_R8 x))));
   i_ldfld, I_field_instr (volatile_unaligned_prefix(fun (x,y) fspec -> I_ldfld(x,y,fspec)));
   i_stfld, I_field_instr (volatile_unaligned_prefix(fun  (x,y) fspec -> I_stfld(x,y,fspec)));
   i_ldsfld, I_field_instr (volatile_prefix (fun x fspec -> I_ldsfld (x, fspec)));
   i_stsfld, I_field_instr (volatile_prefix (fun x fspec -> I_stsfld (x, fspec)));
   i_ldflda, I_field_instr (no_prefixes (fun fspec -> I_ldflda fspec));
   i_ldsflda, I_field_instr (no_prefixes (fun fspec -> I_ldsflda fspec)); 
   i_call, I_method_instr (tail_prefix (fun tl (mspec,y) -> I_call (tl,mspec,y)));
   i_ldftn, I_method_instr (no_prefixes (fun (mspec,y) -> I_ldftn mspec));
   i_ldvirtftn, I_method_instr (no_prefixes (fun (mspec,y) -> I_ldvirtftn mspec));
   i_newobj, I_method_instr (no_prefixes (fun (mspec,y) -> I_newobj (mspec,y)));
   i_callvirt, I_method_instr (constraint_tail_prefix (fun (c,tl) (mspec,y) -> match c with Some ty -> I_callconstraint(tl,ty,mspec,y) | None -> I_callvirt (tl,mspec,y))); 
   i_leave_s, I_unconditional_i8_instr (no_prefixes (fun x -> I_leave x));
   i_br_s, I_unconditional_i8_instr (no_prefixes (fun x -> I_br x)); 
   i_leave, I_unconditional_i32_instr (no_prefixes (fun x -> I_leave x));
   i_br, I_unconditional_i32_instr (no_prefixes (fun x -> I_br x)); 
   i_brtrue_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_brtrue,x,y)));
   i_brfalse_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_brfalse,x,y)));
   i_beq_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_beq,x,y)));
   i_blt_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_blt,x,y)));
   i_blt_un_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_blt_un,x,y)));
   i_ble_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_ble,x,y)));
   i_ble_un_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_ble_un,x,y)));
   i_bgt_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bgt,x,y)));
   i_bgt_un_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bgt_un,x,y)));
   i_bge_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bge,x,y)));
   i_bge_un_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bge_un,x,y)));
   i_bne_un_s, I_conditional_i8_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bne_un,x,y)));   
   i_brtrue, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_brtrue,x,y)));
   i_brfalse, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_brfalse,x,y)));
   i_beq, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_beq,x,y)));
   i_blt, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_blt,x,y)));
   i_blt_un, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_blt_un,x,y)));
   i_ble, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_ble,x,y)));
   i_ble_un, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_ble_un,x,y)));
   i_bgt, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bgt,x,y)));
   i_bgt_un, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bgt_un,x,y)));
   i_bge, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bge,x,y)));
   i_bge_un, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bge_un,x,y)));
   i_bne_un, I_conditional_i32_instr (no_prefixes (fun (x,y) -> I_brcmp (BI_bne_un,x,y))); 
   i_ldstr, I_string_instr (no_prefixes (fun x -> I_ldstr x)); 
   i_switch, I_switch_instr (no_prefixes (fun x -> I_switch x));
   i_ldtoken, I_tok_instr (no_prefixes (fun x -> I_ldtoken x));
   i_calli, I_sig_instr (tail_prefix (fun tl (x,y) -> I_calli (tl, x, y)));
   i_mkrefany, I_type_instr (no_prefixes (fun x -> I_mkrefany x));
   i_refanyval, I_type_instr (no_prefixes (fun x -> I_refanyval x));
   i_ldelema, I_type_instr (readonly_prefix (fun ro x -> I_ldelema (ro,Rank1ArrayShape,x)));
   i_ldelem_any, I_type_instr (no_prefixes (fun x -> I_ldelem_any (Rank1ArrayShape,x)));
   i_stelem_any, I_type_instr (no_prefixes (fun x -> I_stelem_any (Rank1ArrayShape,x)));
   i_newarr, I_type_instr (no_prefixes (fun x -> I_newarr (Rank1ArrayShape,x)));  
   i_castclass, I_type_instr (no_prefixes (fun x -> I_castclass x));
   i_isinst, I_type_instr (no_prefixes (fun x -> I_isinst x));
   i_unbox_any, I_type_instr (no_prefixes (fun x -> I_unbox_any x));
   i_cpobj, I_type_instr (no_prefixes (fun x -> I_cpobj x));
   i_initobj, I_type_instr (no_prefixes (fun x -> I_initobj x));
   i_ldobj, I_type_instr (volatile_unaligned_prefix (fun (x,y) z -> I_ldobj (x,y,z)));
   i_stobj, I_type_instr (volatile_unaligned_prefix (fun (x,y) z -> I_stobj (x,y,z)));
   i_sizeof, I_type_instr (no_prefixes (fun x -> I_sizeof x));
   i_box, I_type_instr (no_prefixes (fun x -> I_box x));
   i_unbox, I_type_instr (no_prefixes (fun x -> I_unbox x)); ] 

(* The tables are delayed to avoid building them unnecessarily at startup *)
(* Many applications of AbsIL (e.g. a compiler) don't need to read instructions. *)
let one_byte_instrs = ref None
let two_byte_instrs = ref None
let fill_instrs () = 
    let one_byte_tab = Array.create 256 I_invalid_instr
    let two_byte_tab = Array.create 256 I_invalid_instr
    let add_instr (i,f) =  
        if i > 0xff then 
            assert (i lsr 8 = 0xfe); 
            let i =  (i &&& 0xff)
            if (two_byte_tab.[i] <> I_invalid_instr) then 
              dprintn ("warning: duplicate decode entries for "^string i);
            two_byte_tab.[i] <- f
        else 
            if (one_byte_tab.[i] <> I_invalid_instr) then
              dprintn ("warning: duplicate decode entries for "^string i);
            one_byte_tab.[i] <- f 
    List.iter add_instr (instrs());
    List.iter (fun (x,mk) -> add_instr (x,I_none_instr (no_prefixes mk))) (noarg_instrs.Force());
    one_byte_instrs := Some one_byte_tab;
    two_byte_instrs := Some two_byte_tab

let rec get_one_byte_instr i = 
    match !one_byte_instrs with 
    | None -> fill_instrs(); get_one_byte_instr i
    | Some t -> t.[i]

let rec get_two_byte_instr i = 
    match !two_byte_instrs with 
    | None -> fill_instrs(); get_two_byte_instr i
    | Some t -> t.[i]
  
(*---------------------------------------------------------------------
 * 
 *---------------------------------------------------------------------*)

type chunk = { size: int32; addr: int32 }

let chunk sz next = ({addr=next; size=sz},next + sz) 
let nochunk next = ({addr= 0x0;size= 0x0; } ,next)

type row_element_kind = 
  | UShort 
  | ULong 
  | Byte 
  | Data 
  | GGuid 
  | Blob 
  | SString 
  | SimpleIndex of table
  | TypeDefOrRefOrSpec
  | TypeOrMethodDef
  | HasConstant 
  | HasCustomAttribute
  | HasFieldMarshal 
  | HasDeclSecurity 
  | MemberRefParent 
  | HasSemantics 
  | MethodDefOrRef
  | MemberForwarded
  | Implementation 
  | CustomAttributeType
  | ResolutionScope
type row_kind = RowKind of row_element_kind list

let kind_AssemblyRef = RowKind [ UShort; UShort; UShort; UShort; ULong; Blob; SString; SString; Blob; ]
let kind_ModuleRef =   RowKind [ SString ]
let kind_FileRef = RowKind [ ULong; SString; Blob ]
let kind_TypeRef = RowKind [ ResolutionScope; SString; SString ]
let kind_TypeSpec = RowKind [ Blob ]
let kind_TypeDef = RowKind [ ULong; SString; SString; TypeDefOrRefOrSpec; SimpleIndex tab_Field; SimpleIndex tab_Method ]
let kind_PropertyMap = RowKind [ SimpleIndex tab_TypeDef; SimpleIndex tab_Property ]
let kind_EventMap = RowKind [ SimpleIndex tab_TypeDef; SimpleIndex tab_Event ]
let kind_InterfaceImpl = RowKind [ SimpleIndex tab_TypeDef; TypeDefOrRefOrSpec ]
let kind_Nested = RowKind [ SimpleIndex tab_TypeDef; SimpleIndex tab_TypeDef ]
let kind_CustomAttribute = RowKind [HasCustomAttribute; CustomAttributeType; Blob ]
let kind_DeclSecurity = RowKind [ UShort; HasDeclSecurity; Blob ]
let kind_MemberRef = RowKind [MemberRefParent; SString; Blob ]
let kind_StandAloneSig = RowKind [Blob ]
let kind_FieldDef = RowKind [UShort; SString; Blob ]
let kind_FieldRVA = RowKind [Data; SimpleIndex tab_Field ]
let kind_FieldMarshal = RowKind [HasFieldMarshal; Blob ]
let kind_Constant = RowKind [ UShort;HasConstant; Blob ]
let kind_FieldLayout = RowKind [ULong; SimpleIndex tab_Field ]
let kind_Param = RowKind [ UShort; UShort; SString ]
let kind_MethodDef = RowKind [ULong;  UShort; UShort; SString; Blob; SimpleIndex tab_Param ]
let kind_MethodImpl = RowKind [SimpleIndex tab_TypeDef; MethodDefOrRef; MethodDefOrRef ]
let kind_ImplMap = RowKind [UShort; MemberForwarded; SString; SimpleIndex tab_ModuleRef ]
let kind_MethodSemantics = RowKind [UShort; SimpleIndex tab_Method; HasSemantics ]
let kind_Property =RowKind [ UShort; SString; Blob ]
let kind_Event =RowKind [ UShort; SString; TypeDefOrRefOrSpec ]
let kind_ManifestResource =RowKind [ ULong; ULong; SString; Implementation ]
let kind_ClassLayout = RowKind [ UShort; ULong; SimpleIndex tab_TypeDef ]
let kind_ExportedType = RowKind [  ULong; ULong; SString; SString; Implementation ]
let kind_Assembly = RowKind [  ULong; UShort; UShort; UShort; UShort; ULong; Blob; SString; SString ]
let kind_GenericParam_v1_1 = RowKind [  UShort; UShort; TypeOrMethodDef; SString; TypeDefOrRefOrSpec ]
let kind_GenericParam_v2_0 = RowKind [  UShort; UShort; TypeOrMethodDef; SString ]
let kind_MethodSpec = RowKind [ MethodDefOrRef; Blob ]
let kind_GenericParamConstraint = RowKind [  SimpleIndex tab_GenericParam; TypeDefOrRefOrSpec ]
let kind_Module = RowKind [  UShort; SString; GGuid; GGuid; GGuid ]
let kind_Illegal = RowKind []

(*---------------------------------------------------------------------
 * Used for binary searches of sorted tables.  Each function that reads
 * a table row returns a tuple that contains the elements of the row.
 * One of these elements may be a key for a sorted table.  These
 * keys can be compared using the functions below depending on the
 * kind of element in that column.
 *---------------------------------------------------------------------*)

let hc_compare (TaggedIndex(HasConstantTag t1, (idx1:int))) (TaggedIndex(HasConstantTag t2, idx2)) = 
  if idx1 < idx2 then -1 elif idx1 > idx2 then 1 else compare t1 t2

let hs_compare (TaggedIndex(HasSemanticsTag t1, (idx1:int))) (TaggedIndex(HasSemanticsTag t2, idx2)) = 
  if idx1 < idx2 then -1 elif idx1 > idx2 then 1 else compare t1 t2

let hca_compare (TaggedIndex(HasCustomAttributeTag t1, (idx1:int))) (TaggedIndex(HasCustomAttributeTag t2, idx2)) = 
  if idx1 < idx2 then -1 elif idx1 > idx2 then 1 else compare t1 t2

let mf_compare (TaggedIndex(MemberForwardedTag t1, (idx1:int))) (TaggedIndex(MemberForwardedTag t2, idx2)) = 
  if idx1 < idx2 then -1 elif idx1 > idx2 then 1 else compare t1 t2

let hds_compare (TaggedIndex(HasDeclSecurityTag t1, (idx1:int))) (TaggedIndex(HasDeclSecurityTag t2, idx2)) = 
  if idx1 < idx2 then -1 elif idx1 > idx2 then 1 else compare t1 t2

let hfm_compare (TaggedIndex(HasFieldMarshalTag t1, idx1)) (TaggedIndex(HasFieldMarshalTag t2, idx2)) = 
  if idx1 < idx2 then -1 elif idx1 > idx2 then 1 else compare t1 t2

let tomd_compare (TaggedIndex(TypeOrMethodDefTag t1, idx1)) (TaggedIndex(TypeOrMethodDefTag t2, idx2)) = 
  if idx1 < idx2 then -1 elif idx1 > idx2 then 1 else compare t1 t2

let simpleindex_compare (idx1:int) (idx2:int) = 
  compare idx1 idx2

(*---------------------------------------------------------------------
 * The various keys for the various caches.  We avoid using polymorphic
 * types within keys becuase F# is not crash hot at hashing and compare 
 * them.  In other words, the types in the indexes below should not
 * use (real) tuples, lists, options etc.  We also add a couple of 
 * hashtables indexed by integers since these are also much faster
 * for F#.
 *---------------------------------------------------------------------*)

type typeDefAsTypIdx = TypeDefAsTypIdx of ILBoxity * ILGenericArgs * int
type typeRefAsTypIdx = TypeRefAsTypIdx of ILBoxity * ILGenericArgs * int
type blobAsMethodSigIdx = BlobAsMethodSigIdx of int * int32
type blobAsFieldSigIdx = BlobAsFieldSigIdx of int * int32
type blobAsPropSigIdx = BlobAsPropSigIdx of int * int32
type blobAsLocalSigIdx = BlobAsLocalSigIdx of int * int32
type memberRefAsMspecIdx =  MemberRefAsMspecIdx of int * int
type methodSpecAsMspecIdx =  MethodSpecAsMspecIdx of int * int
type memberRefAsFspecIdx = MemberRefAsFspecIdx of int * int
type customAttrIdx = CustomAttrIdx of customAttributeType_tag * int * int32
type securityDeclIdx   = SecurityDeclIdx of uint16 * int32
type genericParsIdx = GenericParamsIdx of int * typeOrMethodDef_tag * int

(*---------------------------------------------------------------------
 * Polymorphic caches for row and heap readers
 *---------------------------------------------------------------------*)

module I32hashtbl = 

   type 'a t = System.Collections.Generic.Dictionary<int32,'a> 
   let create (n:int) : 'a t = new System.Collections.Generic.Dictionary<_,_>(n)
   let add (t: 'a t) x y = t.Add(x,y)
   let find (t: 'a t) x = t.Item(x)
   let mem (t: 'a t) x = t.ContainsKey(x) 


let mk_cache_int32 lowMem inbase nm sz  =
    if lowMem then (fun f x -> f x) else
    let cache = ref null in // I32hashtbl.create 11 (* sz *)
    let count = ref 0
    add_report (fun oc -> if !count <> 0 then output_string oc (inbase^string !count ^ " "^nm^" cache hits"^"\n"));
    fun f (idx:int32) ->
        let cache = 
            match !cache with
            | null -> cache := I32hashtbl.create 11 (* sz  *)
            | _ -> ()
            !cache
        if I32hashtbl.mem cache idx then (incr count; I32hashtbl.find cache idx)
        else let res = f idx in I32hashtbl.add cache idx res; res 

let mk_cache_gen lowMem inbase nm sz  =
    if lowMem then (fun f x -> f x) else
    let cache = ref null in // I32hashtbl.create 11 (* sz *)
    //let cache = new Dictionary<_,_>(sz:int)
    let count = ref 0
    add_report (fun oc -> if !count <> 0 then output_string oc (inbase^string !count ^ " "^nm^" cache hits"^"\n"));
    fun f (idx :'a) ->
        let cache = 
            match !cache with
            | null -> cache := new Dictionary<_,_>(11 (* sz:int *) ) 
            | _ -> ()
            !cache
        if cache.ContainsKey idx then (incr count; cache.[idx])
        else let res = f idx in cache.[idx] <- res; res 

(*-----------------------------------------------------------------------
 * Polymorphic general helpers for searching for particular rows.
 * ----------------------------------------------------------------------*)

let SeekFindRow nrows rowChooser =
    let mutable i = 1
    while (i <= nrows &&  not (rowChooser i)) do 
        i <- i + 1;
    if i > nrows then dprintn "warning: SeekFindRow: row not found";
    i  

// search for rows satisfying predicate 
let SeekReadIndexedRows (nrows,rowReader,keyfunc,keycomparef,binchop, rowConverter) =
    if binchop then
        let mutable low = 0
        let mutable high = nrows + 1
        begin 
          let mutable fin = false
          while not fin do 
                   (* if logging then dprintn (infile ^ ": binary search of table "^string (tag_of_table tab)^", low = "^string !low^", high = "^string !high);*)
              if high - low <= 1  then 
                  fin <- true 
              else 
                  let mid = (low + high) / 2
                  let midrow = rowReader mid
                  let c = keycomparef (keyfunc midrow)
                  if c > 0 then 
                      low <- mid
                  elif c < 0 then 
                      high <- mid 
                  else 
                      fin <- true
        end;
             (* if logging then dprintn (infile ^ ": finished binary search of table "^string (tag_of_table tab));*)
        let mutable res = []
        if high - low > 1 then 
            // now read off rows, forward and backwards 
            let mid = (low + high) / 2
            // read forward 
            begin 
                let mutable fin = false
                let mutable curr = mid
                while not fin do 
                  if curr > nrows then 
                      fin <- true;
                  else 
                      let currrow = rowReader curr
                      if keycomparef (keyfunc currrow) = 0 then 
                          res <- rowConverter currrow :: res;
                      else 
                          fin <- true;
                      curr <- curr + 1;
                done;
            end;
            res <- List.rev res;
            // read backwards 
            begin 
                let mutable fin = false
                let mutable curr = mid - 1
                while not fin do 
                  if curr = 0 then 
                    fin <- true
                  else  
                    let currrow = rowReader curr
                    if keycomparef (keyfunc currrow) = 0 then 
                        res <- rowConverter currrow :: res;
                    else 
                        fin <- true;
                    curr <- curr - 1;
            end;
        // sanity check 
        if checking then 
            let res2 = 
                [ for i = 1 to nrows do
                    let rowinfo = rowReader i
                    if keycomparef (keyfunc rowinfo) = 0 then 
                      yield rowConverter rowinfo ]
            if (res2 <> res) then 
                failwith ("results of binary search did not match results of linear search: linear search produced "^string res2.Length^", binary search produced "^string res.Length)
        
        res
    else 
        let res = ref []
        for i = 1 to nrows do
            let rowinfo = rowReader i
            if keycomparef (keyfunc rowinfo) = 0 then 
              res := rowConverter rowinfo :: !res;
        List.rev !res  


let SeekReadOptionalIndexedRow ((nrows,rowReader,keyfunc,keycomparer,binchop,rowConverter) as info) =
    match SeekReadIndexedRows info with 
    | [k] -> Some k
    | [] -> None
    | h::t -> 
        dprintn ("multiple rows found when indexing table"); 
        Some h 
        
let SeekReadIndexedRow ((nrows,rowReader,keyfunc,keycomparer,binchop,rowConverter) as info) =
    match SeekReadOptionalIndexedRow info with 
    | Some row -> row
    | None -> failwith ("no row found for key when indexing table")

(*---------------------------------------------------------------------
 * The big fat reader.
 *---------------------------------------------------------------------*)

type ILModuleReader = 
    { modul: ILModuleDef; 
      ilAssemblyRefs: Lazy<ILAssemblyRef list>
      dispose: unit -> unit }
    member x.ILModuleDef = x.modul
    member x.ILAssemblyRefs = x.ilAssemblyRefs.Force()
    
 
type method_data = ILType * ILCallingConv * string * ILType list * ILType * ILType list 
type vararg_method_data = ILType * ILCallingConv * string * ILType list * ILType list option * ILType * ILType list 

[<StructuralEquality(false); StructuralComparison(false)>]
type ctxt = 
  { ilg: ILGlobals;
    data_end_points: int32 list Lazy.t;
    sorted: int64;
    pdb: (pdb_reader * (string -> ILSourceDocument)) option;
    eptoken: table * int;
    nrows: table -> int; 
    text_phys_loc : int32; 
    text_phys_size : int32;
    data_phys_loc : int32;
    data_phys_size : int32;
    anyV2P : (string * int32) -> int32;
    metadata_addr: int32;
    section_headers : (int32 * int32 * int32) list;
    native_resources_addr:int32;
    native_resources_size:int32;
    resources_addr:int32;
    strongname_addr:int32;
    vtable_fixups_addr:int32;
    is:input;
    infile:string;
    user_strings_stream_phys_loc: int32;
    strings_stream_phys_loc: int32;
    blobs_stream_phys_loc: int32;
    ReadUserStringHeap: (int32 -> string);
    MemoizeString: string -> string;
    ReadStringHeap: (int32 -> string);
    ReadBlobHeap: (int32 -> byte[]);
    guids_stream_phys_loc : int32;
    row_addr : (table -> int -> int32);
    table_bignesses : bool array;
    rs_bigness : bool;  
    tdor_bigness : bool;
    tomd_bigness : bool;   
    hc_bigness : bool;   
    hca_bigness : bool;   
    hfm_bigness : bool;   
    hds_bigness : bool;   
    mrp_bigness : bool;   
    hs_bigness : bool;   
    mdor_bigness : bool;   
    mf_bigness : bool;   
    i_bigness : bool;   
    cat_bigness : bool;   
    strings_big: bool;   
    guids_big: bool;   
    blobs_big: bool;   
    count_TypeRef : int ref;
    count_TypeDef : int ref;     
    count_Field : int ref;      
    count_Method : int ref;     
    count_Param : int ref;          
    count_InterfaceImpl : int ref;  
    count_MemberRef : int ref;        
    count_Constant : int ref;         
    count_CustomAttribute : int ref;  
    count_FieldMarshal: int ref;    
    count_Permission : int ref;      
    count_ClassLayout : int ref;     
    count_FieldLayout : int ref;       
    count_StandAloneSig : int ref;    
    count_EventMap : int ref;         
    count_Event : int ref;            
    count_PropertyMap : int ref;       
    count_Property : int ref;           
    count_MethodSemantics : int ref;    
    count_MethodImpl : int ref;  
    count_ModuleRef : int ref;       
    count_TypeSpec : int ref;         
    count_ImplMap : int ref;      
    count_FieldRVA : int ref;   
    count_Assembly : int ref;        
    count_AssemblyRef : int ref;    
    count_File : int ref;           
    count_ExportedType : int ref;  
   count_ManifestResource : int ref;
   count_Nested : int ref;         
   count_GenericParam : int ref;       
   count_GenericParamConstraint : int ref;     
   count_MethodSpec : int ref;        
   SeekReadNestedRow  : int -> int * int;
   SeekReadConstantRow  : int -> int32 * TaggedIndex<hasConstant_tag> * int32;
   SeekReadMethodSemanticsRow  : int -> int32 * int * TaggedIndex<hasSemantics_tag>;
   SeekReadTypeDefRow : int -> int32 * int32 * int32 * TaggedIndex<typeDefOrRef_tag> * int * int;
   SeekReadInterfaceImplRow  : int -> int * TaggedIndex<typeDefOrRef_tag>;
   SeekReadFieldMarshalRow  : int -> TaggedIndex<hasFieldMarshal_tag> * int32;
   SeekReadPropertyMapRow  : int -> int * int; 
   SeekReadAssemblyRef : int -> ILAssemblyRef;
   SeekReadMethodSpec_as_mdata : methodSpecAsMspecIdx -> vararg_method_data;
   SeekReadMemberRef_as_mdata : memberRefAsMspecIdx -> vararg_method_data;
   SeekReadMemberRef_as_fspec : memberRefAsFspecIdx -> ILFieldSpec;
   SeekReadCustomAttr : customAttrIdx -> ILAttribute;
   SeekReadSecurityDecl : securityDeclIdx -> ILPermission;
   SeekReadTypeRef : int ->ILTypeRef;
   SeekReadTypeRef_as_typ : typeRefAsTypIdx -> ILType;
   ReadBlobHeap_as_property_sig : blobAsPropSigIdx -> ILThisConvention * ILType * ILType list;
   ReadBlobHeap_as_field_sig : blobAsFieldSigIdx -> ILType;
   ReadBlobHeap_as_method_sig : blobAsMethodSigIdx -> bool * int32 * ILCallingConv * ILType * ILType list * ILType list option;

   ReadBlobHeap_as_locals_sig : blobAsLocalSigIdx -> Local list;
   SeekReadTypeDefAsType : typeDefAsTypIdx -> ILType;
   SeekReadMethodDef_as_mdata : int -> method_data;
   SeekReadGenericParams : genericParsIdx -> ILGenericParameterDef list;
   SeekReadFieldDef_as_fspec : int -> ILFieldSpec; }
  
let read_z_untagged_idx tab ctxt =  
    if ctxt.table_bignesses.[tag_of_table tab]
    then read_i32 ctxt.is 
    else read_u16_as_i32 ctxt.is

let read_rs_idx    ctxt = read_z_tagged_idx mkResolutionScopeTag     2 ctxt.rs_bigness ctxt.is   
let read_tdor_idx  ctxt = read_z_tagged_idx mkTypeDefOrRefOrSpecTag  2 ctxt.tdor_bigness ctxt.is   
let read_tomd_idx  ctxt = read_z_tagged_idx mkTypeOrMethodDefTag     1 ctxt.tomd_bigness ctxt.is   
let read_hc_idx    ctxt = read_z_tagged_idx mkHasConstantTag         2 ctxt.hc_bigness ctxt.is   
let read_hca_idx   ctxt = read_z_tagged_idx mkHasCustomAttributeTag  5 ctxt.hca_bigness ctxt.is   
let read_hfm_idx   ctxt = read_z_tagged_idx mkHasFieldMarshalTag     1 ctxt.hfm_bigness ctxt.is   
let read_hds_idx   ctxt = read_z_tagged_idx mkHasDeclSecurityTag     2 ctxt.hds_bigness ctxt.is   
let read_mrp_idx   ctxt = read_z_tagged_idx mkMemberRefParentTag     3 ctxt.mrp_bigness ctxt.is   
let read_hs_idx    ctxt = read_z_tagged_idx mkHasSemanticsTag        1 ctxt.hs_bigness ctxt.is   
let read_mdor_idx  ctxt = read_z_tagged_idx mkMethodDefOrRefTag      1 ctxt.mdor_bigness ctxt.is   
let read_mf_idx    ctxt = read_z_tagged_idx mkMemberForwardedTag     1 ctxt.mf_bigness ctxt.is   
let read_i_idx     ctxt = read_z_tagged_idx mkImplementationTag      2 ctxt.i_bigness ctxt.is   
let read_cat_idx   ctxt = read_z_tagged_idx mkCustomAttributeTypeTag 3 ctxt.cat_bigness ctxt.is   
let read_string_idx ctxt = if ctxt.strings_big then read_i32 ctxt.is else read_u16_as_i32 ctxt.is 
let read_guid_idx ctxt = if ctxt.guids_big then read_i32 ctxt.is else read_u16_as_i32 ctxt.is
let read_blob_idx ctxt = if ctxt.blobs_big then read_i32 ctxt.is else read_u16_as_i32 ctxt.is 

let SeekReadModuleRow ctxt idx =
     if idx = 0 then failwith "cannot read Module table row 0";
     let addr = ctxt.row_addr tab_Module idx
     if logging then dprintn (ctxt.infile ^ ": module row addr = " ^ string addr);

     seek ctxt.is addr;
     let generation = read_u16 ctxt.is
     let name_idx = read_string_idx ctxt
     let mvid_idx = read_guid_idx ctxt
     let encid_idx = read_guid_idx ctxt
     let encbaseid_idx = read_guid_idx ctxt
     (generation, name_idx, mvid_idx, encid_idx, encbaseid_idx) 

/// Read Table ILTypeRef 
let SeekReadTypeRefRow ctxt idx =
     incr ctxt.count_TypeRef;
     let addr = ctxt.row_addr tab_TypeRef idx
     seek ctxt.is addr;
     let scope_idx = read_rs_idx ctxt
     let name_idx = read_string_idx ctxt
     let namespace_idx = read_string_idx ctxt
     (scope_idx,name_idx,namespace_idx) 

/// Read Table ILTypeDef 
let SeekReadTypeDefRow ctxt idx = ctxt.SeekReadTypeDefRow idx
let SeekReadTypeDefRowUncached ctxtH idx =
    let ctxt = getHole ctxtH
    incr ctxt.count_TypeDef;
    let addr = ctxt.row_addr tab_TypeDef idx
    seek ctxt.is addr;
    let flags = read_i32 ctxt.is
    let name_idx = read_string_idx ctxt
    let namespace_idx = read_string_idx ctxt
    let extends_idx = read_tdor_idx ctxt
    let fields_idx = read_z_untagged_idx tab_Field ctxt
    let methods_idx = read_z_untagged_idx tab_Method ctxt
    (flags, name_idx, namespace_idx, extends_idx, fields_idx, methods_idx) 

/// Read Table Field 
let SeekReadFieldRow ctxt idx =
     incr ctxt.count_Field;
     let addr = ctxt.row_addr tab_Field idx
     seek ctxt.is addr;
     let flags = read_u16_as_i32 ctxt.is
     let name_idx = read_string_idx ctxt
     let type_idx = read_blob_idx ctxt
     (flags,name_idx,type_idx)  

/// Read Table Method 
let SeekReadMethodRow ctxt idx =
     incr ctxt.count_Method;
     let addr = ctxt.row_addr tab_Method idx
     seek ctxt.is addr;
     let code_rva = read_i32 ctxt.is
     let implflags = read_u16_as_i32 ctxt.is
     let flags = read_u16_as_i32 ctxt.is
     let name_idx = read_string_idx ctxt
     let type_idx = read_blob_idx ctxt
     let param_idx = read_z_untagged_idx tab_Param ctxt
     (code_rva, implflags, flags, name_idx, type_idx, param_idx) 

/// Read Table Param 
let SeekReadParamRow ctxt idx =
     incr ctxt.count_Param;
     let addr = ctxt.row_addr tab_Param idx
     seek ctxt.is addr;
     let flags = read_u16_as_i32 ctxt.is
     let seq =  (read_u16_as_i32 ctxt.is)
     let name_idx = read_string_idx ctxt
     (flags,seq,name_idx) 

/// Read Table InterfaceImpl 
let SeekReadInterfaceImplRow ctxt idx = ctxt.SeekReadInterfaceImplRow idx
let SeekReadInterfaceImplRowUncached ctxtH idx =
     let ctxt = getHole ctxtH
     incr ctxt.count_InterfaceImpl;
     let addr = ctxt.row_addr tab_InterfaceImpl idx
     seek ctxt.is addr;
     let tidx = read_z_untagged_idx tab_TypeDef ctxt
     let intf_idx = read_tdor_idx ctxt
     (tidx,intf_idx)

/// Read Table MemberRef 
let SeekReadMemberRefRow ctxt idx =
     incr ctxt.count_MemberRef;
     let addr = ctxt.row_addr tab_MemberRef idx
     seek ctxt.is addr;
     let mrp_idx = read_mrp_idx ctxt
     let name_idx = read_string_idx ctxt
     let type_idx = read_blob_idx ctxt
     (mrp_idx,name_idx,type_idx) 

/// Read Table Constant 
let SeekReadConstantRow ctxt idx = ctxt.SeekReadConstantRow idx
let SeekReadConstantRowUncached ctxtH idx =
     let ctxt = getHole ctxtH
     incr ctxt.count_Constant;
     let addr = ctxt.row_addr tab_Constant idx
     seek ctxt.is addr;
     let kind = read_u16_as_i32 ctxt.is
     let parent_idx = read_hc_idx ctxt
     let val_idx = read_blob_idx ctxt
     (kind, parent_idx, val_idx)

/// Read Table CustomAttribute 
let SeekReadCustomAttributeRow ctxt idx =
     incr ctxt.count_CustomAttribute;
     let addr = ctxt.row_addr tab_CustomAttribute idx
     seek ctxt.is addr;
     let parent_idx = read_hca_idx ctxt
     let type_idx = read_cat_idx ctxt
     let val_idx = read_blob_idx ctxt
     (parent_idx, type_idx, val_idx)  

/// Read Table FieldMarshal 
let SeekReadFieldMarshalRow ctxt idx = ctxt.SeekReadFieldMarshalRow idx
let SeekReadFieldMarshalRowUncached ctxtH idx =
     let ctxt = getHole ctxtH
     incr ctxt.count_FieldMarshal;
     let addr = ctxt.row_addr tab_FieldMarshal idx
     seek ctxt.is addr;
     let parent_idx = read_hfm_idx ctxt
     let type_idx = read_blob_idx ctxt
     (parent_idx, type_idx)

/// Read Table Permission 
let SeekReadPermissionRow ctxt idx =
     incr ctxt.count_Permission;
    (* if logging then dprintn (ctxt.infile ^ ": reading Permission row "^string idx); *)
     let addr = ctxt.row_addr tab_Permission idx
     seek ctxt.is addr;
     let action = read_u16 ctxt.is
     let parent_idx = read_hds_idx ctxt
     let type_idx = read_blob_idx ctxt
     (* if logging then dprintn "finished read of Permission row";*)
     (action,parent_idx, type_idx) 

/// Read Table ClassLayout 
let SeekReadClassLayoutRow ctxt idx =
     incr ctxt.count_ClassLayout;
     let addr = ctxt.row_addr tab_ClassLayout idx
     seek ctxt.is addr;
     let pack = read_u16 ctxt.is
     let size = read_i32 ctxt.is
     let tidx = read_z_untagged_idx tab_TypeDef ctxt
     (pack,size,tidx)  

/// Read Table FieldLayout 
let SeekReadFieldLayoutRow ctxt idx =
     incr ctxt.count_FieldLayout;
     let addr = ctxt.row_addr tab_FieldLayout idx
     seek ctxt.is addr;
     let offset = read_i32 ctxt.is
     let fidx = read_z_untagged_idx tab_Field ctxt
     (offset,fidx)  

//// Read Table StandAloneSig 
let SeekReadStandAloneSigRow ctxt idx =
     incr ctxt.count_StandAloneSig;
     let addr = ctxt.row_addr tab_StandAloneSig idx
     seek ctxt.is addr;
     let sig_idx = read_blob_idx ctxt
     (sig_idx)  

/// Read Table EventMap 
let SeekReadEventMapRow ctxt idx =
    incr ctxt.count_EventMap;
    let addr = ctxt.row_addr tab_EventMap idx
    seek ctxt.is addr;
    let tidx = read_z_untagged_idx tab_TypeDef ctxt
    let events_idx = read_z_untagged_idx tab_Event ctxt
    (tidx,events_idx) 

/// Read Table Event 
let SeekReadEventRow ctxt idx =
     incr ctxt.count_Event;
     let addr = ctxt.row_addr tab_Event idx
     seek ctxt.is addr;
     let flags = read_u16_as_i32 ctxt.is
     let name_idx = read_string_idx ctxt
     let typ_idx = read_tdor_idx ctxt
     (flags,name_idx,typ_idx) 
   
/// Read Table PropertyMap 
let SeekReadPropertyMapRow ctxt idx = ctxt.SeekReadPropertyMapRow idx
let SeekReadPropertyMapRowUncached ctxtH idx =
    let ctxt = getHole ctxtH
    incr ctxt.count_PropertyMap;
    let addr = ctxt.row_addr tab_PropertyMap idx
    seek ctxt.is addr;
    let tidx = read_z_untagged_idx tab_TypeDef ctxt
    let props_idx = read_z_untagged_idx tab_Property ctxt
    (tidx,props_idx)

/// Read Table Property 
let SeekReadPropertyRow ctxt idx =
     incr ctxt.count_Property;
     let addr = ctxt.row_addr tab_Property idx
     seek ctxt.is addr;
     let flags = read_u16_as_i32 ctxt.is
     let name_idx = read_string_idx ctxt
     let typ_idx = read_blob_idx ctxt
     (flags,name_idx,typ_idx) 

/// Read Table MethodSemantics 
let SeekReadMethodSemanticsRow ctxt idx = ctxt.SeekReadMethodSemanticsRow idx
let SeekReadMethodSemanticsRowUncached ctxtH idx =
    let ctxt = getHole ctxtH
    incr ctxt.count_MethodSemantics;
    let addr = ctxt.row_addr tab_MethodSemantics idx
    seek ctxt.is addr;
    let flags = read_u16_as_i32 ctxt.is
    let midx = read_z_untagged_idx tab_Method ctxt
    let assoc_idx = read_hs_idx ctxt
    (flags,midx,assoc_idx)

/// Read Table MethodImpl 
let SeekReadMethodImplRow ctxt idx =
    incr ctxt.count_MethodImpl;
    let addr = ctxt.row_addr tab_MethodImpl idx
    seek ctxt.is addr;
    let tidx = read_z_untagged_idx tab_TypeDef ctxt
    let mbody_idx = read_mdor_idx ctxt
    let mdecl_idx = read_mdor_idx ctxt
    (tidx,mbody_idx,mdecl_idx) 

/// Read Table ILModuleRef 
let SeekReadModuleRefRow ctxt idx =
    incr ctxt.count_ModuleRef;
    let addr = ctxt.row_addr tab_ModuleRef idx
    seek ctxt.is addr;
    let name_idx = read_string_idx ctxt
    name_idx  

/// Read Table ILTypeSpec 
let SeekReadTypeSpecRow ctxt idx =
    incr ctxt.count_TypeSpec;
    let addr = ctxt.row_addr tab_TypeSpec idx
    seek ctxt.is addr;
    let blob_idx = read_blob_idx ctxt
    blob_idx  

/// Read Table ImplMap 
let SeekReadImplMapRow ctxt idx =
    incr ctxt.count_ImplMap;
    let addr = ctxt.row_addr tab_ImplMap idx
    seek ctxt.is addr;
    let flags = read_u16_as_i32 ctxt.is
    let forwrded_idx = read_mf_idx ctxt
    let name_idx = read_string_idx ctxt
    let scope_idx = read_z_untagged_idx tab_ModuleRef ctxt
    (flags, forwrded_idx, name_idx, scope_idx) 

/// Read Table FieldRVA 
let SeekReadFieldRVARow ctxt idx =
    incr ctxt.count_FieldRVA;
    let addr = ctxt.row_addr tab_FieldRVA idx
    seek ctxt.is addr;
    let rva = read_i32 ctxt.is
    let fidx = read_z_untagged_idx tab_Field ctxt
    (rva,fidx) 

/// Read Table Assembly 
let SeekReadAssemblyRow ctxt idx =
    incr ctxt.count_Assembly;
    let addr = ctxt.row_addr tab_Assembly idx
    seek ctxt.is addr;
    let hash = read_i32 ctxt.is
    let v1 = read_u16 ctxt.is
    let v2 = read_u16 ctxt.is
    let v3 = read_u16 ctxt.is
    let v4 = read_u16 ctxt.is
    let flags = read_i32 ctxt.is
    let public_key_idx = read_blob_idx ctxt
    let name_idx = read_string_idx ctxt
    let locale_idx = read_string_idx ctxt
    (hash,v1,v2,v3,v4,flags,public_key_idx, name_idx, locale_idx)

/// Read Table ILAssemblyRef 
let SeekReadAssemblyRefRow ctxt idx =
    incr ctxt.count_AssemblyRef;
    let addr = ctxt.row_addr tab_AssemblyRef idx
    seek ctxt.is addr;
    let v1 = read_u16 ctxt.is
    let v2 = read_u16 ctxt.is
    let v3 = read_u16 ctxt.is
    let v4 = read_u16 ctxt.is
    let flags = read_i32 ctxt.is
    let public_key_or_token_idx = read_blob_idx ctxt
    let name_idx = read_string_idx ctxt
    let locale_idx = read_string_idx ctxt
    let hash_value_idx = read_blob_idx ctxt
    (v1,v2,v3,v4,flags,public_key_or_token_idx, name_idx, locale_idx,hash_value_idx) 

/// Read Table File 
let SeekReadFileRow ctxt idx =
    incr ctxt.count_File;
    let addr = ctxt.row_addr tab_File idx
    seek ctxt.is addr;
    let flags = read_i32 ctxt.is
    let name_idx = read_string_idx ctxt
    let hash_value_idx = read_blob_idx ctxt
    (flags, name_idx, hash_value_idx) 

/// Read Table ILExportedType 
let SeekReadExportedTypeRow ctxt idx =
    incr ctxt.count_ExportedType;
    let addr = ctxt.row_addr tab_ExportedType idx
    seek ctxt.is addr;
    let flags = read_i32 ctxt.is
    let tok = read_i32 ctxt.is
    let name_idx = read_string_idx ctxt
    let namespace_idx = read_string_idx ctxt
    let impl_idx = read_i_idx ctxt
    (flags,tok,name_idx,namespace_idx,impl_idx) 

/// Read Table ManifestResource 
let SeekReadManifestResourceRow ctxt idx =
    incr ctxt.count_ManifestResource;
    let addr = ctxt.row_addr tab_ManifestResource idx
    seek ctxt.is addr;
    let offset = read_i32 ctxt.is
    let flags = read_i32 ctxt.is
    let name_idx = read_string_idx ctxt
    let impl_idx = read_i_idx ctxt
    (offset,flags,name_idx,impl_idx) 

/// Read Table Nested 
let SeekReadNestedRow ctxt idx = ctxt.SeekReadNestedRow idx
let SeekReadNestedRowUncached ctxtH idx =
    let ctxt = getHole ctxtH
    incr ctxt.count_Nested;
    let addr = ctxt.row_addr tab_Nested idx
    seek ctxt.is addr;
    let nested_idx = read_z_untagged_idx tab_TypeDef ctxt
    let encl_idx = read_z_untagged_idx tab_TypeDef ctxt
    (nested_idx,encl_idx)

/// Read Table GenericParam 
let SeekReadGenericParamRow ctxt idx =
    incr ctxt.count_GenericParam;
    let addr = ctxt.row_addr tab_GenericParam idx
    seek ctxt.is addr;
    let seq = read_u16 ctxt.is
    let flags = read_u16 ctxt.is
    let owner_idx = read_tomd_idx ctxt
    let name_idx = read_string_idx ctxt
    (idx,seq,flags,owner_idx,name_idx) 

// Read Table GenericParamConstraint 
let SeekReadGenericParamConstraintRow ctxt idx =
     incr ctxt.count_GenericParamConstraint;
     let addr = ctxt.row_addr tab_GenericParamConstraint idx
     seek ctxt.is addr;
     let pidx = read_z_untagged_idx tab_GenericParam ctxt
     let constraint_idx = read_tdor_idx ctxt
     (pidx,constraint_idx) 

/// Read Table ILMethodSpec 
let SeekReadMethodSpecRow ctxt idx =
    incr ctxt.count_MethodSpec;
    let addr = ctxt.row_addr tab_MethodSpec idx
    seek ctxt.is addr;
    let mdor_idx = read_mdor_idx ctxt
    let inst_idx = read_blob_idx ctxt
    (mdor_idx,inst_idx) 

let ReadUserStringHeapUncached ctxtH idx = 
    let ctxt = getHole ctxtH
    if logging then dprintn (ctxt.infile ^ ": reading user string heap "^string idx);
    let res = SeekReadUserString ctxt.is (ctxt.user_strings_stream_phys_loc + idx)
    (* if logging then dprintn (ctxt.infile ^ ": read string '"^res^"'"); *)
    res 
let ReadUserStringHeap        ctxt idx = ctxt.ReadUserStringHeap  idx 

let ReadStringHeapUncached ctxtH idx = 
     let ctxt = getHole ctxtH
     SeekReadUTF8String ctxt.is (ctxt.strings_stream_phys_loc + idx) 
let ReadStringHeap          ctxt idx = ctxt.ReadStringHeap idx 
let ReadStringHeapOption   ctxt idx = if idx = 0 then None else Some (ReadStringHeap ctxt idx) 

let ReadBlobHeapUncached ctxtH idx = 
     let ctxt = getHole ctxtH
     if logging then dprintn (ctxt.infile ^ ": reading blob heap "^string idx);
     SeekReadBlob ctxt.is (ctxt.blobs_stream_phys_loc + idx) 
let ReadBlobHeap        ctxt idx = ctxt.ReadBlobHeap idx 
let ReadBlobHeapOption ctxt idx = if idx = 0 then None else Some (ReadBlobHeap ctxt idx) 

let ReadGuidHeap ctxt idx = SeakReadGuid ctxt.is (ctxt.guids_stream_phys_loc + idx) 

   (* read a single value out of a blob heap using the given function *)
let ReadBlobHeap_as_bool   ctxt vidx = fst (sigptr_get_bool   (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_i8     ctxt vidx = fst (sigptr_get_i8     (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_i16    ctxt vidx = fst (sigptr_get_i16    (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_i32    ctxt vidx = fst (sigptr_get_i32    (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_i64    ctxt vidx = fst (sigptr_get_i64    (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_u8     ctxt vidx = fst (sigptr_get_u8     (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_u16    ctxt vidx = fst (sigptr_get_u16    (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_u32    ctxt vidx = fst (sigptr_get_u32    (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_u64    ctxt vidx = fst (sigptr_get_u64    (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_ieee32 ctxt vidx = fst (sigptr_get_ieee32 (ReadBlobHeap ctxt vidx) 0) 
let ReadBlobHeap_as_ieee64 ctxt vidx = fst (sigptr_get_ieee64 (ReadBlobHeap ctxt vidx) 0) 
   


//-----------------------------------------------------------------------
// This is gross.  Some binaries have raw data embedded
// their text sections, e.g. mscorlib, for field inits.  And there is no 
// information that definitively tells us the extent of 
// the text section that may be interesting data.  
// But we certainly don't want to duplicate the entire 
// text section as data! 
//  
// So, we assume: 
//   1. no part of the metadata is double-used for raw data  
//   2. the data bits are all the bits of the text section 
//      that stretch from a Field or Resource RVA to one of 
//        (a) the next Field or resource RVA 
//        (b) a MethodRVA 
//        (c) the start of the metadata 
//        (d) the end of a section 
//        (e) the start of the native resources attached to the binary if any
// ----------------------------------------------------------------------*)


let read_native_resources ctxt = 

  let native_resources = 
      if logging then dprintn (ctxt.infile ^ ": native_resources_size = "^string ctxt.native_resources_size);
      if logging then dprintn (ctxt.infile ^ ": native_resources_addr = "^string ctxt.native_resources_addr);
      if ctxt.native_resources_size = 0x0 or ctxt.native_resources_addr = 0x0 then 
          []
      else
          [ (lazy (if logging then dprintn (ctxt.infile ^ ": reading linked resource...");
                   let linkedResource = SeekReadBytes ctxt.is (ctxt.anyV2P (ctxt.infile ^ ": native resources",ctxt.native_resources_addr)) ctxt.native_resources_size
                   if logging then dprintn (ctxt.infile ^ ": size = "^string (Bytes.length linkedResource));
                   if logging then dprintn (ctxt.infile ^ ": unlinking resource...");
                   unlinkResource ctxt.native_resources_addr linkedResource)) ]
  native_resources
   
let data_end_points ctxtH = 
    lazy
      begin 
        let ctxt = getHole ctxtH
        let dataStartPoints = 
            let res = ref []
            for i = 1 to ctxt.nrows (tab_FieldRVA) do
                let rva,fidx = SeekReadFieldRVARow ctxt i
                res := ("field",rva) :: !res;
            for i = 1 to ctxt.nrows (tab_ManifestResource) do
                let (offset,_,_,TaggedIndex(tag,idx)) = SeekReadManifestResourceRow ctxt i
                if idx = 0 then 
                  let rva = ctxt.resources_addr + offset
                  res := ("manifest resource", rva) :: !res;
            !res
        if isNil dataStartPoints then [] 
        else
          let methodRVAs = 
            let res = ref []
            for i = 1 to ctxt.nrows (tab_Method) do
                let (rva, _, _, name_idx, _, _) = SeekReadMethodRow ctxt i
                if rva <> 0 then 
                   let nm = ReadStringHeap ctxt name_idx
                   res := (nm,rva) :: !res;
            !res
          ([ ctxt.text_phys_loc + ctxt.text_phys_size; 
            ctxt.data_phys_loc + ctxt.data_phys_size; ] 
           @ 
           (List.map ctxt.anyV2P 
              (dataStartPoints 
                @ [for (virt_addr,virt_size,phys_loc) in ctxt.section_headers do yield ("section start",virt_addr) done]
                @ [("md",ctxt.metadata_addr)]
                @ (if ctxt.native_resources_addr = 0x0 then [] else [("native resources",ctxt.native_resources_addr); ])
                @ (if ctxt.resources_addr = 0x0 then [] else [("managed resources",ctxt.resources_addr); ])
                @ (if ctxt.strongname_addr = 0x0 then [] else [("managed strongname",ctxt.strongname_addr); ])
                @ (if ctxt.vtable_fixups_addr = 0x0 then [] else [("managed vtable_fixups",ctxt.vtable_fixups_addr); ])
                @ methodRVAs)))
           // Make distinct 
           |> Set.of_list
           |> Set.to_list
           |> List.sort 
      end 
      

let rec rva_to_data ctxt nm rva = 
    if rva = 0x0 then failwith "rva is zero";
    let start = ctxt.anyV2P (nm, rva)
    let endPoints = (Lazy.force ctxt.data_end_points)
    let rec look l = 
      match l with 
      | [] -> 
          failwithf "find_text_data_extent: none found for infile=%s, name=%s, rva=0x%08x, start=0x%08x" ctxt.infile nm rva start 
      | e::t -> 
         if start < e then 
           (SeekReadBytes ctxt.is start (e - start)) 
         else look t
    look endPoints


  
//-----------------------------------------------------------------------
// Read the AbsIL structure (lazily) by reading off the relevant rows.
// ----------------------------------------------------------------------

let is_sorted ctxt tab = ((ctxt.sorted &&& (int64 1 <<< tag_of_table tab)) <> int64 0x0) 

let rec SeekReadModule ctxt (subsys,ilonly,only32,only64,platform,is_dll, align_virt,align_phys,image_base_real) idx =
    let (generation, name_idx, mvid_idx, encid_idx, encbaseid_idx) = SeekReadModuleRow ctxt idx
    let mname = ReadStringHeap ctxt name_idx
    let native_resources = read_native_resources ctxt

    { modulManifest =      
         if ctxt.nrows (tab_Assembly) > 0 then Some (SeekReadAssemblyManifest ctxt 1) 
         else None;
      modulCustomAttrs = SeekReadCustomAttrs ctxt (TaggedIndex(hca_Module,idx));
      modulName = mname;
      modulNativeResources=native_resources;
      modulTypeDefs = mk_lazy_tdefs (lazy (SeekReadTopTypeDefs ctxt ()));
      modulSubSystem = int32 subsys;
      modulILonly = ilonly;
      modulPlatform = platform;
      modul32bit = only32;
      modul64bit = only64;
      modulDLL=is_dll;
      modulVirtAlignment = align_virt;
      modulPhysAlignment = align_phys;
      modulImageBase = image_base_real;
      modulResources = SeekReadManifestResources ctxt ();
      (* modulFixups = [] (* REVIEW:VIP *) *) }  

and SeekReadAssemblyManifest ctxt idx =
    let (hash,v1,v2,v3,v4,flags,public_key_idx, name_idx, locale_idx) = SeekReadAssemblyRow ctxt idx
    let name = ReadStringHeap ctxt name_idx
    let pubkey = ReadBlobHeapOption ctxt public_key_idx
    { manifestName= name; 
      manifestAuxModuleHashAlgorithm=hash;
      manifestSecurityDecls= SeekReadSecurityDecls ctxt (TaggedIndex(hds_Assembly,idx));
      manifestPublicKey= pubkey;  
      manifestVersion= Some (v1,v2,v3,v4);
      manifestLocale= ReadStringHeapOption ctxt locale_idx;
      manifestCustomAttrs = SeekReadCustomAttrs ctxt (TaggedIndex(hca_Assembly,idx));
      manifestLongevity= 
        begin let masked = flags &&& 0x000e
          if masked = 0x0000 then LongevityUnspecified
          elif masked = 0x0002 then LongevityLibrary
          elif masked = 0x0004 then LongevityPlatformAppDomain
          elif masked = 0x0006 then LongevityPlatformProcess
          elif masked = 0x0008 then LongevityPlatformSystem
          else LongevityUnspecified
        end;
      manifestExportedTypes= SeekReadTopExportedTypes ctxt ();
      manifestEntrypointElsewhere=(if fst ctxt.eptoken = tab_File then Some (SeekReadFile ctxt (snd ctxt.eptoken)) else None);
      manifestRetargetable = 0 <> (flags &&& 0xff);
      manifestDisableJitOptimizations = 0 <> (flags &&& 0x4000);
      manifestJitTracking = 0 <> (flags &&& 0x8000); } 
     
and SeekReadAssemblyRef ctxt idx = ctxt.SeekReadAssemblyRef idx
and SeekReadAssemblyRefUncached ctxtH idx = 
    let ctxt = getHole ctxtH
    let (v1,v2,v3,v4,flags,public_key_or_token_idx, name_idx, locale_idx,hash_value_idx) = SeekReadAssemblyRefRow ctxt idx
    let nm = ReadStringHeap ctxt name_idx
    let publicKey = 
        match ReadBlobHeapOption ctxt public_key_or_token_idx with 
          | None -> None
          | Some blob -> Some (if (flags &&& 0x0001) <> 0x0 then PublicKey blob else PublicKeyToken blob)
          
    ILAssemblyRef.Create
        (name=nm, 
         hash=ReadBlobHeapOption ctxt hash_value_idx, 
         publicKey=publicKey,
         retargetable=((flags &&& 0x0100) <> 0x0), 
         version=Some(v1,v2,v3,v4), 
         locale=ReadStringHeapOption ctxt locale_idx;)

and SeekReadModuleRef ctxt idx =
    let (name_idx) = SeekReadModuleRefRow ctxt idx
    ILModuleRef.Create(name =  ReadStringHeap ctxt name_idx,
                     hasMetadata=true,
                     hash=None)

and SeekReadFile ctxt idx =
    let (flags, name_idx, hash_value_idx) = SeekReadFileRow ctxt idx
    ILModuleRef.Create(name =  ReadStringHeap ctxt name_idx,
                     hasMetadata= ((flags &&& 0x0001) = 0x0),
                     hash= ReadBlobHeapOption ctxt hash_value_idx)

and SeekReadClassLayout ctxt idx =
    match SeekReadOptionalIndexedRow (ctxt.nrows tab_ClassLayout,SeekReadClassLayoutRow ctxt,(fun (_,_,tidx) -> tidx),simpleindex_compare idx,is_sorted ctxt tab_ClassLayout,(fun (pack,size,_) -> pack,size)) with 
    | None -> { typeSize = None; typePack = None }
    | Some (pack,size) -> { typeSize = Some size; 
                           typePack = Some pack; }

and member_access_of_flags flags =
    let f = (flags &&& 0x00000007)
    if f = 0x00000001 then  MemAccess_private 
    elif f = 0x00000006 then  MemAccess_public 
    elif f = 0x00000004 then  MemAccess_family 
    elif f = 0x00000002 then  MemAccess_famandassem 
    elif f = 0x00000005 then  MemAccess_famorassem 
    elif f = 0x00000003 then  MemAccess_assembly 
    else MemAccess_compilercontrolled

and type_access_of_flags flags =
    let f = (flags &&& 0x00000007)
    if f = 0x00000001 then TypeAccess_public 
    elif f = 0x00000002 then TypeAccess_nested MemAccess_public 
    elif f = 0x00000003 then TypeAccess_nested MemAccess_private 
    elif f = 0x00000004 then TypeAccess_nested MemAccess_family 
    elif f = 0x00000006 then TypeAccess_nested MemAccess_famandassem 
    elif f = 0x00000007 then TypeAccess_nested MemAccess_famorassem 
    elif f = 0x00000005 then TypeAccess_nested MemAccess_assembly 
    else TypeAccess_private

and type_layout_of_flags ctxt flags tidx = 
    let f = (flags &&& 0x00000018)
    if f = 0x00000008 then TypeLayout_sequential (SeekReadClassLayout ctxt tidx)
    elif f = 0x00000010 then  TypeLayout_explicit (SeekReadClassLayout ctxt tidx)
    else TypeLayout_auto

and type_kind_of_flags nm mdefs fdefs super flags =
    if (flags &&& 0x00000020) <> 0x0 then TypeDef_interface 
    else 
         let is_enum = (match super with None -> false | Some ty -> (tspec_of_typ ty).Name = "System.Enum")
         let is_delegate = (match super with None -> false | Some ty -> (tspec_of_typ ty).Name = "System.Delegate")
         let is_multicast_delegate = (match super with None -> false | Some ty -> (tspec_of_typ ty).Name = "System.MulticastDelegate")
         let self_is_multicast_delegate = nm = "System.MulticastDelegate"
         let is_valuetype = (match super with None -> false | Some ty -> (tspec_of_typ ty).Name = "System.ValueType" && nm <> "System.Enum")
         if is_enum then TypeDef_enum 
         elif  (is_delegate && not self_is_multicast_delegate) or is_multicast_delegate then TypeDef_delegate
         elif is_valuetype then TypeDef_valuetype 
         else TypeDef_class 

and type_encoding_of_flags flags = 
    let f = (flags &&& 0x00030000)
    if f = 0x00020000 then TypeEncoding_autochar 
    elif f = 0x00010000 then TypeEncoding_unicode 
    else TypeEncoding_ansi

and seek_is_wanted_TypeDef flags = true
       
and seek_is_top_TypeDef flags =
    (type_access_of_flags flags =  TypeAccess_private) ||
     type_access_of_flags flags =  TypeAccess_public
       
and seek_is_top_TypeDef_idx ctxt idx =
    let (flags,_,_, _, _,_) = SeekReadTypeDefRow ctxt idx
    seek_is_top_TypeDef flags
       
and ReadBlobHeap_as_split_type_name ctxt (name_idx,namespace_idx) = 
    let name = ReadStringHeap ctxt name_idx
    let nspace = ReadStringHeapOption ctxt namespace_idx
    match nspace with 
    | Some nspace -> split_namespace nspace,name  
    | None -> [],name

and ReadBlobHeapAsTypeName ctxt (name_idx,namespace_idx) = 
    let name = ReadStringHeap ctxt name_idx
    let nspace = ReadStringHeapOption ctxt namespace_idx
    match nspace with 
    | None -> name  
    | Some ns -> ctxt.MemoizeString (ns^"."^name)

and SeekReadTypeDefRow_extents ctxt ((_,_, _, _, fields_idx, methods_idx) as info ) (idx:int) =
    if idx >= ctxt.nrows (tab_TypeDef) then 
      ctxt.nrows (tab_Field) + 1,
      ctxt.nrows (tab_Method) + 1
    else
      let (_, _, _, _, fields_idx, methods_idx) = SeekReadTypeDefRow ctxt (idx + 1)
      fields_idx, methods_idx 

and SeekReadTypeDefRow_with_extents ctxt (idx:int) =
     let info= SeekReadTypeDefRow ctxt idx
     info,SeekReadTypeDefRow_extents ctxt info idx

and SeekReadTypeDef ctxt toponly acc (idx:int) =
    let (flags,name_idx,namespace_idx, _, _, _) = SeekReadTypeDefRow ctxt idx
    if toponly && not (seek_is_top_TypeDef flags) then acc
    elif not (seek_is_wanted_TypeDef flags) then acc
    else
     let ns,n = ReadBlobHeap_as_split_type_name ctxt (name_idx,namespace_idx)
     let cas = SeekReadCustomAttrs ctxt (TaggedIndex(hca_TypeDef,idx))

     let rest = 
       lazy
         begin 
           (* Re-read so as not to save all these in the lazy closure - this suspension ctxt.is the largest *)
           (* heavily allocated one in all of AbsIL*)
           let ((flags,name_idx,namespace_idx, extends_idx, fields_idx, methods_idx) as info) = SeekReadTypeDefRow ctxt idx
           let nm = ReadBlobHeapAsTypeName ctxt (name_idx,namespace_idx)
           let cas = SeekReadCustomAttrs ctxt (TaggedIndex(hca_TypeDef,idx))

           if logging then dprintn ("reading remainder of type "^nm);       
           let (end_fields_idx, end_methods_idx) = SeekReadTypeDefRow_extents ctxt info idx
           let typars = SeekReadGenericParams ctxt 0 (tomd_TypeDef,idx)
           let numtypars = List.length typars
           if logging then dprintn ("reading supertype of type "^nm);       
           let super = SeekReadOptionalTypeDefOrRef ctxt numtypars AsObject extends_idx
           let layout = type_layout_of_flags ctxt flags idx
           let has_layout = (match layout with TypeLayout_explicit _ -> true | _ -> false)
           if logging then dprintn ("setting up reading of methods and fields for type "^nm);       
           let mdefs = SeekReadMethods ctxt numtypars methods_idx end_methods_idx
           let fdefs = SeekReadFields ctxt (numtypars,has_layout) fields_idx end_fields_idx
           if logging then dprintn ("determining kind of type "^nm);        
           let kind = type_kind_of_flags nm mdefs fdefs super flags
           if logging then dprintn ("setting up read of nested types for type "^nm);        
           let nested = SeekReadNestedTypeDefs ctxt idx nm
           if logging then dprintn ("setting up read of interface impls for type "^nm);     
           let impls = SeekReadInterfaceImpls ctxt numtypars idx
           if logging then dprintn ("setting up read of security decls for type "^nm);   
           let sdecls =  SeekReadSecurityDecls ctxt (TaggedIndex(hds_TypeDef,idx))
           if logging then dprintn ("setting up read of mimpls for type "^nm);   
           let mimpls = SeekReadMethodImpls ctxt numtypars idx
           if logging then dprintn ("setting up read of properties for type "^nm);       
           let props = SeekReadProperties ctxt numtypars idx
           if logging then dprintn ("setting up read of custom attributes for type "^nm);        
           let events = SeekReadEvents ctxt numtypars idx
           if logging then dprintn ("preparing results for type "^nm);   
           let res = 
             { tdKind= kind;
               tdName=nm;
               tdGenericParams=typars; 
               tdAccess= type_access_of_flags flags;
               tdAbstract= (flags &&& 0x00000080) <> 0x0;
               tdSealed= (flags &&& 0x00000100) <> 0x0; 
               tdSerializable= (flags &&& 0x00002000) <> 0x0; 
               tdComInterop= (flags &&& 0x00001000) <> 0x0; 
               tdLayout = layout;
               tdSpecialName= (flags &&& 0x00000400) <> 0x0;
               tdEncoding=type_encoding_of_flags flags;
               tdNested= nested;
               tdImplements = impls;  
               tdExtends = super; 
               tdMethodDefs = mdefs; 
               tdSecurityDecls = sdecls;
               tdHasSecurity=(flags &&& 0x00040000) <> 0x0;
               tdFieldDefs=fdefs;
               tdMethodImpls=mimpls;
               tdInitSemantics=
                   if kind = TypeDef_interface then TypeInit_beforeany
                   elif (flags &&& 0x00100000) <> 0x0 then TypeInit_beforefield
                   else TypeInit_beforeany; 
               tdEvents= events;
               tdProperties=props;
               tdCustomAttrs=cas; }
           if logging then dprintn ("done reading remainder of type "^nm);          
           res
         end
     (ns,n,cas,rest) :: acc

and SeekReadTopTypeDefs ctxt () =
    let res = ref []
    for i = 1 to ctxt.nrows (tab_TypeDef) do
      res := SeekReadTypeDef ctxt true !res i;
    List.rev !res 

and SeekReadNestedTypeDefs ctxt tidx nm =
    mk_lazy_tdefs 
      (lazy 
         begin
           if logging then dprintn ("reading nested ILTypeDefs for type "^nm);        
           let nested_idxs = SeekReadIndexedRows (ctxt.nrows tab_Nested,SeekReadNestedRow ctxt,snd,simpleindex_compare tidx,false,fst)
           List.rev (List.fold (SeekReadTypeDef ctxt false) [] nested_idxs)
         end)

and SeekReadInterfaceImpls ctxt numtypars tidx =
    SeekReadIndexedRows (ctxt.nrows tab_InterfaceImpl,
                            SeekReadInterfaceImplRow ctxt,
                            fst,
                            simpleindex_compare tidx,
                            is_sorted ctxt tab_InterfaceImpl,
                            (snd >> SeekReadTypeDefOrRef ctxt numtypars AsObject (*ok*) [])) 

and SeekReadGenericParams ctxt numtypars (a,b) = ctxt.SeekReadGenericParams (GenericParamsIdx(numtypars,a,b))

and SeekReadGenericParamsUncached ctxtH (GenericParamsIdx(numtypars,a,b)) =
    let ctxt = getHole ctxtH
    let pars =
        SeekReadIndexedRows
            (ctxt.nrows tab_GenericParam,SeekReadGenericParamRow ctxt,
             (fun (_,_,_,tomd,_) -> tomd),
             tomd_compare (TaggedIndex(a,b)),
             is_sorted ctxt tab_GenericParam,
             (fun (gpidx,seq,flags,_,name_idx) -> 
                 let flags = int32 flags
                 let variance_flags = flags &&& 0x0003
                 let variance = 
                     if variance_flags = 0x0000 then NonVariant
                     elif variance_flags = 0x0001 then CoVariant
                     elif variance_flags = 0x0002 then ContraVariant 
                     else NonVariant
                 let constraints = SeekReadGenericParamConstraintsUncached ctxt numtypars gpidx
                 seq, {gpName=ReadStringHeap ctxt name_idx;
                       gpConstraints=constraints;
                       gpVariance=variance;  
                       gpReferenceTypeConstraint= (flags &&& 0x0004) <> 0;
                       gpNotNullableValueTypeConstraint= (flags &&& 0x0008) <> 0;
                       gpDefaultConstructorConstraint=(flags &&& 0x0010) <> 0; }))
    pars |> List.sortBy fst |> List.map snd 

and SeekReadGenericParamConstraintsUncached ctxt numtypars gpidx =
    SeekReadIndexedRows 
        (ctxt.nrows tab_GenericParamConstraint,
         SeekReadGenericParamConstraintRow ctxt,
         fst,
         simpleindex_compare gpidx,
         is_sorted ctxt tab_GenericParamConstraint,
         (snd >>  SeekReadTypeDefOrRef ctxt numtypars AsObject (*ok*) []))

and SeekReadTypeDefAsType ctxt boxity ginst idx =
      ctxt.SeekReadTypeDefAsType (TypeDefAsTypIdx (boxity,ginst,idx))

and SeekReadTypeDefAsTypeUncached ctxtH (TypeDefAsTypIdx (boxity,ginst,idx)) =
    let ctxt = getHole ctxtH
    mk_typ boxity (ILTypeSpec.Create(SeekReadTypeDefAsTypeRef ctxt idx, ginst))

and SeekReadTypeDefAsTypeRef ctxt idx =
     if logging then dprintn ("reading ILTypeDef "^string idx^" as type ref"); 
     let enc = 
       if seek_is_top_TypeDef_idx ctxt idx then [] 
       else 
         let encl_idx = SeekReadIndexedRow (ctxt.nrows tab_Nested,SeekReadNestedRow ctxt,fst,simpleindex_compare idx,is_sorted ctxt tab_Nested,snd)
         let tref = SeekReadTypeDefAsTypeRef ctxt encl_idx
         tref.Enclosing@[tref.Name]
     let (_, name_idx, namespace_idx, _, _, _) = SeekReadTypeDefRow ctxt idx
     let nm = ReadBlobHeapAsTypeName ctxt (name_idx,namespace_idx)
     ILTypeRef.Create(scope=ScopeRef_local, enclosing=enc, name = nm )

and SeekReadTypeRef ctxt idx = ctxt.SeekReadTypeRef idx
and SeekReadTypeRefUncached ctxtH idx =
     let ctxt = getHole ctxtH
     if logging then dprintn ("reading ILTypeRef "^string idx); 
     let scope_idx,name_idx,namespace_idx = SeekReadTypeRefRow ctxt idx
     let scope,enc = SeekReadTypeRefScope ctxt scope_idx
     let nm = ReadBlobHeapAsTypeName ctxt (name_idx,namespace_idx)
     ILTypeRef.Create(scope=scope, enclosing=enc, name = nm) 

and SeekReadTypeRef_as_typ ctxt boxity ginst idx = ctxt.SeekReadTypeRef_as_typ (TypeRefAsTypIdx (boxity,ginst,idx))
and SeekReadTypeRef_as_typUncached ctxtH (TypeRefAsTypIdx (boxity,ginst,idx)) =
     let ctxt = getHole ctxtH
     mk_typ boxity (ILTypeSpec.Create(SeekReadTypeRef ctxt idx, ginst))

and SeekReadTypeDefOrRef ctxt numtypars boxity ginst (TaggedIndex(tag,idx) ) =
    match tag with 
    | tag when tag = tdor_TypeDef -> SeekReadTypeDefAsType ctxt boxity ginst idx
    | tag when tag = tdor_TypeRef -> SeekReadTypeRef_as_typ ctxt boxity ginst idx
    | tag when tag = tdor_TypeSpec -> 
        if ginst <> [] then dprintn ("type spec used as type constructor for a generic instantiation: ignoring instantiation");
        ReadBlobHeap_as_typ ctxt numtypars (SeekReadTypeSpecRow ctxt idx)
    | _ -> failwith "SeekReadTypeDefOrRef ctxt"

and SeekReadTypeDefOrRefAsTypeRef ctxt (TaggedIndex(tag,idx) ) =
    match tag with 
    | tag when tag = tdor_TypeDef -> SeekReadTypeDefAsTypeRef ctxt idx
    | tag when tag = tdor_TypeRef -> SeekReadTypeRef ctxt idx
    | tag when tag = tdor_TypeSpec -> 
        dprintn ("type spec used where a type ref or def ctxt.is required");
        ctxt.ilg.tref_Object
    | _ -> failwith "SeekReadTypeDefOrRefAsTypeRef_read_tdor"

and SeekReadMethodRefParent ctxt numtypars (TaggedIndex(tag,idx)) =
    match tag with 
    | tag when tag = mrp_TypeRef -> SeekReadTypeRef_as_typ ctxt AsObject (* not ok - no way to tell if a member ref parent ctxt.is a value type or not *) [] idx
    | tag when tag = mrp_ModuleRef -> typ_for_toplevel (ScopeRef_module (SeekReadModuleRef ctxt idx))
    | tag when tag = mrp_MethodDef -> 
        let mspec = mk_mspec_in_typ(SeekReadMethodDef_as_mdata ctxt idx)
        mspec.EnclosingType
    | tag when tag = mrp_TypeSpec -> ReadBlobHeap_as_typ ctxt numtypars (SeekReadTypeSpecRow ctxt idx)
    | _ -> failwith "SeekReadMethodRefParent ctxt"

and SeekReadMethodDefOrRef ctxt numtypars (TaggedIndex(tag,idx)) =
    match tag with 
    | tag when tag = mdor_MethodDef -> 
        let (encl_typ, cc, nm, argtys, retty,minst) = SeekReadMethodDef_as_mdata ctxt idx
        (encl_typ, cc, nm, argtys, None,retty,minst)
    | tag when tag = mdor_MemberRef -> SeekReadMemberRef_as_mdata ctxt numtypars idx
    | _ -> failwith "SeekReadMethodDefOrRef ctxt"

and SeekReadMethodDefOrRefNoVarargs ctxt numtypars x =
     let (encl_typ, cc, nm, argtys, varargs,retty,minst)=     SeekReadMethodDefOrRef ctxt numtypars x 
     if varargs <> None then dprintf "ignoring sentinel and varargs in ILMethodDef token signature";
     (encl_typ, cc, nm, argtys, retty,minst)

and SeekReadCustomAttrType ctxt (TaggedIndex(tag,idx) ) =
    match tag with 
    | tag when tag = cat_MethodDef -> (mk_mspec_in_typ (SeekReadMethodDef_as_mdata ctxt idx))
    | tag when tag = cat_MemberRef -> (mk_mspec_in_typ (SeekReadMemberRef_as_mdata_no_varargs ctxt 0 idx))
    | _ -> failwith "SeekReadCustomAttrType ctxt"
    
and SeekReadImplAsScopeRef ctxt (TaggedIndex(tag,idx) ) =
     if idx = 0 then ScopeRef_local
     else 
       match tag with 
       | tag when tag = i_File -> ScopeRef_module (SeekReadFile ctxt idx)
       | tag when tag = i_AssemblyRef -> ScopeRef_assembly (SeekReadAssemblyRef ctxt idx)
       | tag when tag = i_ExportedType -> failwith "SeekReadImplAsScopeRef ctxt"
       | _ -> failwith "SeekReadImplAsScopeRef ctxt"

and SeekReadTypeRefScope ctxt (TaggedIndex(tag,idx) ) =
    match tag with 
    | tag when tag = rs_Module -> ScopeRef_local,[]
    | tag when tag = rs_ModuleRef -> ScopeRef_module (SeekReadModuleRef ctxt idx),[]
    | tag when tag = rs_AssemblyRef -> ScopeRef_assembly (SeekReadAssemblyRef ctxt idx),[]
    | tag when tag = rs_TypeRef -> 
        let tref = SeekReadTypeRef ctxt idx
        tref.Scope,(tref.Enclosing@[tref.Name])
    | _ -> failwith "SeekReadTypeRefScope ctxt"

and SeekReadOptionalTypeDefOrRef ctxt numtypars boxity idx = 
    if idx = TaggedIndex(tdor_TypeDef, 0) then None
    else Some (SeekReadTypeDefOrRef ctxt numtypars boxity [] idx)

and SeekReadField ctxt (numtypars, has_layout) (idx:int) =
     let (flags,name_idx,type_idx) = SeekReadFieldRow ctxt idx
     let nm = ReadStringHeap ctxt name_idx
     if logging then dprintn ("reading field "^nm);         
     let isStatic = (flags &&& 0x0010) <> 0
     let fd = 
       { fdName = nm;
         fdType= ReadBlobHeap_as_field_sig ctxt numtypars type_idx;
         fdAccess = member_access_of_flags flags;
         fdStatic = isStatic;
         fdInitOnly = (flags &&& 0x0020) <> 0;
         fdLiteral = (flags &&& 0x0040) <> 0;
         fdNotSerialized = (flags &&& 0x0080) <> 0;
         fdSpecialName = (flags &&& 0x0200) <> 0 or (flags &&& 0x0400) <> 0; (* REVIEW: RTSpecialName *)
         fdInit = if (flags &&& 0x8000) = 0 then None else Some (SeekReadConstant ctxt (TaggedIndex(hc_FieldDef,idx)));
         fdMarshal = 
             if (flags &&& 0x1000) = 0 then None else 
             Some (SeekReadIndexedRow (ctxt.nrows tab_FieldMarshal,
                                       SeekReadFieldMarshalRow ctxt,
                                       fst,
                                       hfm_compare (TaggedIndex(hfm_FieldDef,idx)),
                                       is_sorted ctxt tab_FieldMarshal,
                                       (snd >> ReadBlobHeapAsNativeType ctxt)));
         fdData = 
             begin 
               if (flags &&& 0x0100) = 0 then None 
               else 
                 let rva = SeekReadIndexedRow (ctxt.nrows tab_FieldRVA,
                                                  SeekReadFieldRVARow ctxt,
                                                  snd,
                                                  simpleindex_compare idx,
                                                  is_sorted ctxt tab_FieldRVA,
                                                  fst) 
                 Some (rva_to_data ctxt "field" rva)
             end;
         fdOffset = 
             if has_layout && not isStatic then Some (SeekReadIndexedRow (ctxt.nrows tab_FieldLayout,
                                                                             SeekReadFieldLayoutRow ctxt,
                                                                             snd,
                                                                             simpleindex_compare idx,
                                                                             is_sorted ctxt tab_FieldLayout,
                                                                             fst)) else None; 
         fdCustomAttrs=SeekReadCustomAttrs ctxt (TaggedIndex(hca_FieldDef,idx)); }
     if logging then dprintn ("done reading field "^nm);    
     fd
     
and SeekReadFields ctxt (numtypars, has_layout) fidx1 fidx2 =
    mk_lazy_fdefs 
       (lazy
           [ for i = fidx1 to fidx2 - 1 do
               yield SeekReadField ctxt (numtypars, has_layout) i ])

and SeekReadMethods ctxt numtypars midx1 midx2 =
    mk_lazy_mdefs 
       (lazy 
           [ for i = midx1 to midx2 - 1 do
               if seek_is_wanted_MethodDef i then 
                 yield SeekReadMethod ctxt numtypars i ])

and sigptr_get_tdor_idx bytes sigptr = 
    let n, sigptr = sigptr_get_z_i32 bytes sigptr
    if (n &&& 0x01) = 0x0 then (* Type Def *)
        TaggedIndex(tdor_TypeDef,  (n lsr 2)), sigptr
    else (* Type Ref *)
        TaggedIndex(tdor_TypeRef,  (n lsr 2)), sigptr
         

and sigptr_get_typ ctxt numtypars bytes sigptr = 
    let b0,sigptr = sigptr_get_byte bytes sigptr
    if logging then dprintn ("reading type from sig at "^string sigptr^", et = "^string b0); 
    if b0 = et_OBJECT then ctxt.ilg.typ_Object , sigptr
    elif b0 = et_STRING then ctxt.ilg.typ_String, sigptr
    elif b0 = et_I1 then ctxt.ilg.typ_int8, sigptr
    elif b0 = et_I2 then ctxt.ilg.typ_int16, sigptr
    elif b0 = et_I4 then ctxt.ilg.typ_int32, sigptr
    elif b0 = et_I8 then ctxt.ilg.typ_int64, sigptr
    elif b0 = et_I then ctxt.ilg.typ_IntPtr, sigptr
    elif b0 = et_U1 then ctxt.ilg.typ_uint8, sigptr
    elif b0 = et_U2 then ctxt.ilg.typ_uint16, sigptr
    elif b0 = et_U4 then ctxt.ilg.typ_uint32, sigptr
    elif b0 = et_U8 then ctxt.ilg.typ_uint64, sigptr
    elif b0 = et_U then ctxt.ilg.typ_UIntPtr, sigptr
    elif b0 = et_R4 then ctxt.ilg.typ_float32, sigptr
    elif b0 = et_R8 then ctxt.ilg.typ_float64, sigptr
    elif b0 = et_CHAR then ctxt.ilg.typ_char, sigptr
    elif b0 = et_BOOLEAN then ctxt.ilg.typ_bool, sigptr
    elif b0 = et_WITH then 
      let b0,sigptr = sigptr_get_byte bytes sigptr
      let tdor_idx, sigptr = sigptr_get_tdor_idx bytes sigptr
      let n, sigptr = sigptr_get_z_i32 bytes sigptr
      let argtys,sigptr = sigptr_foldi (sigptr_get_typ ctxt numtypars) ( n) bytes sigptr
      SeekReadTypeDefOrRef ctxt numtypars (if b0 = et_CLASS then AsObject else AsValue) argtys tdor_idx,
      sigptr
        
    elif b0 = et_CLASS then 
      let tdor_idx, sigptr = sigptr_get_tdor_idx bytes sigptr
      SeekReadTypeDefOrRef ctxt numtypars AsObject [] tdor_idx, sigptr
    elif b0 = et_VALUETYPE then 
      let tdor_idx, sigptr = sigptr_get_tdor_idx bytes sigptr
      SeekReadTypeDefOrRef ctxt numtypars AsValue [] tdor_idx, sigptr
    elif b0 = et_VAR then 
      let n, sigptr = sigptr_get_z_i32 bytes sigptr
      Type_tyvar (uint16 n),sigptr
    elif b0 = et_MVAR then 
      let n, sigptr = sigptr_get_z_i32 bytes sigptr
      Type_tyvar (uint16 (n + numtypars)), sigptr
    elif b0 = et_BYREF then 
      let typ, sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
      Type_byref typ, sigptr
    elif b0 = et_PTR then 
      let typ, sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
      Type_ptr typ, sigptr
    elif b0 = et_SZARRAY then 
      let typ, sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
      mk_sdarray_ty typ, sigptr
    elif b0 = et_ARRAY then
      let typ, sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
      let rank, sigptr = sigptr_get_z_i32 bytes sigptr
      let num_sized, sigptr = sigptr_get_z_i32 bytes sigptr
      let sizes, sigptr = sigptr_foldi sigptr_get_z_i32 ( num_sized) bytes sigptr
      let num_lobounded, sigptr = sigptr_get_z_i32 bytes sigptr
      let lobounds, sigptr = sigptr_foldi sigptr_get_z_i32 ( num_lobounded) bytes sigptr
      let shape = 
        let dim i =
          (if i <  num_lobounded then Some (List.nth lobounds i) else None),
          (if i <  num_sized then Some (List.nth sizes i) else None)
        ILArrayShape (Array.to_list (Array.init ( rank) dim))
      mk_array_ty (typ, shape), sigptr
        
    elif b0 = et_VOID then Type_void, sigptr
    elif b0 = et_TYPEDBYREF then ctxt.ilg.typ_TypedReference, sigptr
    elif b0 = et_CMOD_REQD or b0 = et_CMOD_OPT  then 
      let tdor_idx, sigptr = sigptr_get_tdor_idx bytes sigptr
      let typ, sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
      Type_modified((b0 = et_CMOD_REQD), SeekReadTypeDefOrRefAsTypeRef ctxt tdor_idx, typ), sigptr
    elif b0 = et_FNPTR then
      begin
        if logging then dprintn ("reading fptr sig "); 
        let cc_byte,sigptr = sigptr_get_byte bytes sigptr
        let generic,cc = byte_as_callconv cc_byte
        if generic then failwith "fptr sig may not be generic";
        let numparams,sigptr = sigptr_get_z_i32 bytes sigptr
        let retty,sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
        let argtys,sigptr = sigptr_foldi (sigptr_get_typ ctxt numtypars) ( numparams) bytes sigptr
        Type_fptr
          { callsigCallconv=cc;
            callsigArgs=argtys;
            callsigReturn=retty }
          ,sigptr
      end 
    elif b0 = et_SENTINEL then failwith "varargs NYI"
    else Type_void , sigptr
        
and sigptr_get_vararg_typs ctxt n numtypars bytes sigptr = 
    sigptr_foldi (sigptr_get_typ ctxt numtypars) n bytes sigptr 

and sigptr_get_arg_typs ctxt n numtypars bytes sigptr acc = 
    if n <= 0 then (List.rev acc,None),sigptr 
    else
      let b0,sigptr2 = sigptr_get_byte bytes sigptr
      if b0 = et_SENTINEL then 
        let varargs,sigptr = sigptr_get_vararg_typs ctxt n numtypars bytes sigptr2
        (List.rev acc,Some(varargs)),sigptr
      else
        let x,sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
        sigptr_get_arg_typs ctxt (n-1) numtypars bytes sigptr (x::acc)
         
and sigptr_get_local ctxt numtypars bytes sigptr = 
    let pinned,sigptr = 
      let b0, sigptr' = sigptr_get_byte bytes sigptr
      if b0 = et_PINNED then 
        true, sigptr'
      else 
        false, sigptr
    let typ, sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
    { localPinned = pinned;
      localType = typ }, sigptr
         
and ReadBlobHeap_as_method_sig ctxt numtypars blob_idx  =
    ctxt.ReadBlobHeap_as_method_sig (BlobAsMethodSigIdx (numtypars,blob_idx))

and ReadBlobHeap_as_method_sigUncached ctxtH (BlobAsMethodSigIdx (numtypars,blob_idx)) =
    let ctxt = getHole ctxtH
    if logging then dprintn ("reading method sig at "^string blob_idx); 
    let bytes = ReadBlobHeap ctxt blob_idx
    let sigptr = 0
    let cc_byte,sigptr = sigptr_get_byte bytes sigptr
    let generic,cc = byte_as_callconv cc_byte
    let genarity,sigptr = if generic then sigptr_get_z_i32 bytes sigptr else 0x0,sigptr
    let numparams,sigptr = sigptr_get_z_i32 bytes sigptr
    let retty,sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
    let (argtys,varargs),sigptr = sigptr_get_arg_typs ctxt  ( numparams) numtypars bytes sigptr []
    generic,genarity,cc,retty,argtys,varargs
      
and ReadBlobHeap_as_typ ctxt numtypars blob_idx = 
    let bytes = ReadBlobHeap ctxt blob_idx
    let ty,sigptr = sigptr_get_typ ctxt numtypars bytes 0
    ty

and ReadBlobHeap_as_field_sig ctxt numtypars blob_idx  =
    ctxt.ReadBlobHeap_as_field_sig (BlobAsFieldSigIdx (numtypars,blob_idx))
and ReadBlobHeap_as_field_sigUncached ctxtH (BlobAsFieldSigIdx (numtypars,blob_idx)) =
    let ctxt = getHole ctxtH
    if logging then dprintn ("reading field sig at "^string blob_idx); 
    let bytes = ReadBlobHeap ctxt blob_idx
    let sigptr = 0
    let cc_byte,sigptr = sigptr_get_byte bytes sigptr
    if cc_byte <> e_IMAGE_CEE_CS_CALLCONV_FIELD then dprintn "warning: field sig was not CC_FIELD";
    let retty,sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
    retty

      
and ReadBlobHeap_as_property_sig ctxt numtypars blob_idx  =
    ctxt.ReadBlobHeap_as_property_sig (BlobAsPropSigIdx (numtypars,blob_idx))
and ReadBlobHeap_as_property_sigUncached ctxtH (BlobAsPropSigIdx (numtypars,blob_idx))  =
    let ctxt = getHole ctxtH
    let bytes = ReadBlobHeap ctxt blob_idx
    let sigptr = 0
    let cc_byte,sigptr = sigptr_get_byte bytes sigptr
    let hasthis = byte_as_hasthis cc_byte
    let cc_masked = (cc_byte &&& 0x0f)
    if cc_masked <> e_IMAGE_CEE_CS_CALLCONV_PROPERTY then dprintn ("warning: property sig was "^string cc_masked^" instead of CC_PROPERTY");
    let numparams,sigptr = sigptr_get_z_i32 bytes sigptr
    let retty,sigptr = sigptr_get_typ ctxt numtypars bytes sigptr
    let argtys,sigptr = sigptr_foldi (sigptr_get_typ ctxt numtypars) ( numparams) bytes sigptr
    hasthis,retty,argtys
      
and ReadBlobHeap_as_locals_sig ctxt numtypars blob_idx  =
    ctxt.ReadBlobHeap_as_locals_sig (BlobAsLocalSigIdx (numtypars,blob_idx))

and ReadBlobHeap_as_locals_sigUncached ctxtH (BlobAsLocalSigIdx (numtypars,blob_idx)) =
    let ctxt = getHole ctxtH
    let bytes = ReadBlobHeap ctxt blob_idx
    let sigptr = 0
    let cc_byte,sigptr = sigptr_get_byte bytes sigptr
    if cc_byte <> e_IMAGE_CEE_CS_CALLCONV_LOCAL_SIG then dprintn "warning: local sig was not CC_LOCAL";
    let numlocals,sigptr = sigptr_get_z_i32 bytes sigptr
    let localtys,sigptr = sigptr_foldi (sigptr_get_local ctxt numtypars) ( numlocals) bytes sigptr
    localtys
      
and byte_as_hasthis b = 
    let hasthis_masked = (b &&& 0x60)
    if hasthis_masked = e_IMAGE_CEE_CS_CALLCONV_INSTANCE then CC_instance
    elif hasthis_masked = e_IMAGE_CEE_CS_CALLCONV_INSTANCE_EXPLICIT then CC_instance_explicit 
    else CC_static 

and byte_as_callconv b = 
    let cc = 
        let cc_masked = (b &&& 0x0f)
        if cc_masked =  e_IMAGE_CEE_CS_CALLCONV_FASTCALL then CC_fastcall 
        elif cc_masked = e_IMAGE_CEE_CS_CALLCONV_STDCALL then CC_stdcall 
        elif cc_masked = e_IMAGE_CEE_CS_CALLCONV_THISCALL then CC_thiscall 
        elif cc_masked = e_IMAGE_CEE_CS_CALLCONV_CDECL then CC_cdecl 
        elif cc_masked = e_IMAGE_CEE_CS_CALLCONV_VARARG then CC_vararg 
        else  CC_default
    let generic = (b &&& e_IMAGE_CEE_CS_CALLCONV_GENERIC) <> 0x0
    generic, Callconv (byte_as_hasthis b,cc) 
      
and SeekReadMemberRef_as_mdata ctxt numtypars idx = 
    ctxt.SeekReadMemberRef_as_mdata (MemberRefAsMspecIdx (numtypars,idx))
and SeekReadMemberRef_as_mdataUncached ctxtH (MemberRefAsMspecIdx (numtypars,idx)) = 
    let ctxt = getHole ctxtH
    let (mrp_idx,name_idx,type_idx) = SeekReadMemberRefRow ctxt idx
    let nm = ReadStringHeap ctxt name_idx
    let encl_typ = SeekReadMethodRefParent ctxt numtypars mrp_idx
    let generic,genarity,cc,retty,argtys,varargs = ReadBlobHeap_as_method_sig ctxt (List.length (inst_of_typ encl_typ)) type_idx
    let minst =  List.init genarity (fun n -> mk_tyvar_ty (uint16 (numtypars+n))) 
    (encl_typ, cc, nm, argtys, varargs,retty,minst)

and SeekReadMemberRef_as_mdata_no_varargs ctxt numtypars idx =
   let (encl_typ, cc, nm, argtys,varargs, retty,minst) =  SeekReadMemberRef_as_mdata ctxt numtypars idx
   if isSome varargs then dprintf "ignoring sentinel and varargs in ILMethodDef token signature";
   (encl_typ, cc, nm, argtys, retty,minst)

and SeekReadMethodSpec_as_mdata ctxt numtypars idx =  
    ctxt.SeekReadMethodSpec_as_mdata (MethodSpecAsMspecIdx (numtypars,idx))
and SeekReadMethodSpec_as_mdataUncached ctxtH (MethodSpecAsMspecIdx (numtypars,idx)) = 
    let ctxt = getHole ctxtH
    let (mdor_idx,inst_idx) = SeekReadMethodSpecRow ctxt idx
    let (encl_typ, cc, nm, argtys, varargs,retty,_) = SeekReadMethodDefOrRef ctxt numtypars mdor_idx
    let minst = 
        let bytes = ReadBlobHeap ctxt inst_idx
        let sigptr = 0
        let cc_byte,sigptr = sigptr_get_byte bytes sigptr
        if cc_byte <> e_IMAGE_CEE_CS_CALLCONV_GENERICINST then dprintn ("warning: method inst ILCallingConv was "^string cc_byte^" instead of CC_GENERICINST");
        let numgpars,sigptr = sigptr_get_z_i32 bytes sigptr
        let argtys,sigptr = sigptr_foldi (sigptr_get_typ ctxt numtypars) ( numgpars) bytes sigptr
        argtys
    (encl_typ, cc, nm, argtys, varargs,retty, minst)

and SeekReadMemberRef_as_fspec ctxt numtypars idx = 
   ctxt.SeekReadMemberRef_as_fspec (MemberRefAsFspecIdx (numtypars,idx))
and SeekReadMemberRef_as_fspecUncached ctxtH (MemberRefAsFspecIdx (numtypars,idx)) = 
   let ctxt = getHole ctxtH
   let (mrp_idx,name_idx,type_idx) = SeekReadMemberRefRow ctxt idx
   let nm = ReadStringHeap ctxt name_idx
   let encl_typ = SeekReadMethodRefParent ctxt numtypars mrp_idx
   let retty = ReadBlobHeap_as_field_sig ctxt numtypars type_idx
   mk_fspec_in_typ(encl_typ, nm, retty)

// One extremely annoying aspect of the MD format is that given a 
// ILMethodDef token it is non-trivial to find which ILTypeDef it belongs 
// to.  So we do a binary chop through the ILTypeDef table 
// looking for which ILTypeDef has the ILMethodDef within its range.  
// Although the ILTypeDef table is not "sorted", it is effectively sorted by 
// method-range and field-range start/finish indexes  
and SeekReadMethodDef_as_mdata ctxt idx =
   ctxt.SeekReadMethodDef_as_mdata idx
and SeekReadMethodDef_as_mdataUncached ctxtH idx =
   let ctxt = getHole ctxtH
   let (code_rva, implflags, flags, name_idx, type_idx, param_idx) = SeekReadMethodRow ctxt idx
   let nm = ReadStringHeap ctxt name_idx
   // Look for the method def parent. 
   let tidx = 
     SeekReadIndexedRow (ctxt.nrows tab_TypeDef,
                            (fun i -> i, SeekReadTypeDefRow_with_extents ctxt i),
                            (fun r -> r),
                            (fun (_,((_, _, _, _, _, methods_idx),
                                      (_, end_methods_idx)))  -> 
                                        if end_methods_idx <= idx then 1 
                                        elif methods_idx <= idx && idx < end_methods_idx then 0 
                                        else -1),
                            true,fst)
   // Read the method def signature. 
   let generic,genarity,cc,retty,argtys,varargs = ReadBlobHeap_as_method_sig ctxt 0 type_idx
   if varargs <> None then dprintf "ignoring sentinel and varargs in ILMethodDef token signature";
   // Create a formal instantiation if needed 
   let finst = generalize_gparams (SeekReadGenericParams ctxt 0 (tomd_TypeDef,tidx))
   let minst = generalize_gparams (SeekReadGenericParams ctxt (List.length finst) (tomd_MethodDef,idx))
   // Read the method def parent. 
   let encl_typ = SeekReadTypeDefAsType ctxt AsObject (* not ok: see note *) finst tidx
   // Return the constituent parts: put it together at the place where this is called. 
   (encl_typ, cc, nm, argtys, retty,minst)


 (* Similarly for fields. *)
and SeekReadFieldDef_as_fspec ctxt idx =
   ctxt.SeekReadFieldDef_as_fspec idx
and SeekReadFieldDef_as_fspecUncached ctxtH idx =
   let ctxt = getHole ctxtH
   let (flags, name_idx, type_idx) = SeekReadFieldRow ctxt idx
   let nm = ReadStringHeap ctxt name_idx
   (* Look for the field def parent. *)
   let tidx = 
     SeekReadIndexedRow (ctxt.nrows tab_TypeDef,
                            (fun i -> i, SeekReadTypeDefRow_with_extents ctxt i),
                            (fun r -> r),
                            (fun (_,((_, _, _, _, fields_idx, _),(end_fields_idx, _)))  -> 
                                if end_fields_idx <= idx then 1 
                                elif fields_idx <= idx && idx < end_fields_idx then 0 
                                else -1),
                            true,fst)
   // Read the field signature. 
   let retty = ReadBlobHeap_as_field_sig ctxt 0 type_idx
   // Create a formal instantiation if needed 
   let finst = generalize_gparams (SeekReadGenericParams ctxt 0 (tomd_TypeDef,tidx))
   // Read the field def parent. 
   let encl_typ = SeekReadTypeDefAsType ctxt AsObject (* not ok: see note *) finst tidx
   // Put it together. 
   mk_fspec_in_typ(encl_typ, nm, retty)

and seek_is_wanted_MethodDef idx = true

and SeekReadMethod ctxt numtypars (idx:int) =
   if logging then dprintn ("reading method "^string idx); 
   let (code_rva, implflags, flags, name_idx, type_idx, param_idx) = SeekReadMethodRow ctxt idx
   let nm = ReadStringHeap ctxt name_idx
   if logging then dprintn ("  method name = " ^ nm); 
   let isStatic = (flags &&& 0x0010) <> 0x0
   let final = (flags &&& 0x0020) <> 0x0
   let virt = (flags &&& 0x0040) <> 0x0
   let strict = (flags &&& 0x0200) <> 0x0
   let hidebysig = (flags &&& 0x0080) <> 0x0
   let newslot = (flags &&& 0x0100) <> 0x0
   let abstr = (flags &&& 0x0400) <> 0x0
   let specialname = (flags &&& 0x0800) <> 0x0
   let pinvoke = (flags &&& 0x2000) <> 0x0
   let export = (flags &&& 0x0008) <> 0x0
   let rtspecialname = (flags &&& 0x1000) <> 0x0
   let reqsecobj = (flags &&& 0x8000) <> 0x0
   let hassec = (flags &&& 0x4000) <> 0x0
   let codetype = implflags &&& 0x0003
   let unmanaged = (implflags &&& 0x0004) <> 0x0
   let forwardref = (implflags &&& 0x0010) <> 0x0
   let preservesig = (implflags &&& 0x0080) <> 0x0
   let internalcall = (implflags &&& 0x1000) <> 0x0
   let synchronized = (implflags &&& 0x0020) <> 0x0
   let noinline = (implflags &&& 0x0008) <> 0x0
   let mustrun = (implflags &&& 0x0040) <> 0x0
   let cctor = (nm = ".cctor")
   let ctor = (nm = ".ctor")
   let generic,genarity,cc,retty,argtys,varargs = ReadBlobHeap_as_method_sig ctxt numtypars type_idx
   if varargs <> None then dprintf "ignoring sentinel and varargs in ILMethodDef signature";
   
   if logging then dprintn ("finding end param idx"); 
   let end_param_idx =
     if idx >= ctxt.nrows (tab_Method) then 
       ctxt.nrows (tab_Param) + 1
     else
       let (_,_,_,_,_, param_idx) = SeekReadMethodRow ctxt (idx + 1)
       param_idx
   
   if logging then dprintn ("found param range: "^string param_idx^" - "^string end_param_idx); 
   let ret,ilParams = SeekReadParams ctxt (retty,argtys) param_idx end_param_idx
   if logging then dprintn ("read param range: "^string param_idx^" - "^string end_param_idx); 

   let res = 
     { mdName=nm;
       mdKind = 
           (if cctor then MethodKind_cctor 
            elif ctor then MethodKind_ctor 
            elif isStatic then MethodKind_static 
            elif virt then 
             MethodKind_virtual 
               { virtFinal=final; 
                 virtNewslot=newslot; 
                 virtStrict=strict;
                 virtAbstract=abstr; }
            else MethodKind_nonvirtual);
       mdAccess = member_access_of_flags flags;
       mdSecurityDecls=SeekReadSecurityDecls ctxt (TaggedIndex(hds_MethodDef,idx));
       mdHasSecurity=hassec;
       mdEntrypoint= (fst ctxt.eptoken = tab_Method && snd ctxt.eptoken = idx);
       mdReqSecObj=reqsecobj;
       mdHideBySig=hidebysig;
       mdSpecialName=specialname;
       mdUnmanagedExport=export;
       mdSynchronized=synchronized;
       mdMustRun=mustrun;
       mdPreserveSig=preservesig;
       mdManaged = not unmanaged;
       mdInternalCall = internalcall;
       mdForwardRef = forwardref;
       mdCodeKind = (if (codetype = 0x00) then MethodCodeKind_il elif (codetype = 0x01) then MethodCodeKind_native elif (codetype = 0x03) then MethodCodeKind_runtime else (dprintn  "unsupported code type"; MethodCodeKind_native));
       mdExport=None; (* REVIEW:VIP *)
       mdVtableEntry=None; (* REVIEW:VIP *)
       mdGenericParams=SeekReadGenericParams ctxt numtypars (tomd_MethodDef,idx);
       mdCustomAttrs=SeekReadCustomAttrs ctxt (TaggedIndex(hca_MethodDef,idx)); 
       mdParams= ilParams;
       mdCallconv=cc;
       mdReturn=ret;
       mdBody=
         if (codetype = 0x01) && pinvoke then 
           mk_lazy_mbody (notlazy MethodBody_native)
         elif pinvoke then 
           SeekReadImplMap ctxt nm  idx
         elif internalcall or abstr or unmanaged or (codetype <> 0x00) then 
           if code_rva <> 0x0 then dprintn "non-IL or abstract method with non-zero RVA";
           mk_lazy_mbody (notlazy MethodBody_abstract)  
         else 
           SeekReadMethodRVA ctxt (idx,nm,internalcall,noinline,numtypars) code_rva;   
     }
   if logging then dprintn ("  done method = " ^ nm); 
   res
     
     
and SeekReadParams ctxt (retty,argtys) pidx1 pidx2 =
  let ret_res = ref { returnMarshal=None;
                      returnType=retty;
                      returnCustomAttrs=empty_custom_attrs }
  let params_res = 
      argtys 
      |> Array.of_list 
      |> Array.map (fun ty ->  { paramName=None;
                                 paramDefault=None;
                                 paramMarshal=None;
                                 paramIn=false;
                                 paramOut=false;
                                 paramOptional=false;
                                 paramType=ty;
                                 paramCustomAttrs=empty_custom_attrs })
  for i = pidx1 to pidx2 - 1 do
      SeekReadParam_extras ctxt (ret_res,params_res) i
  !ret_res, Array.to_list params_res

and SeekReadParam_extras ctxt (ret_res,params_res) (idx:int) =
   let (flags,seq,name_idx) = SeekReadParamRow ctxt idx
   if logging then dprintn ("reading param "^string idx^", seq = "^string seq); 
   let inout_masked = (flags &&& 0x00FF)
   let has_marshal = (flags &&& 0x2000) <> 0x0
   let has_default = (flags &&& 0x1000) <> 0x0
   let fm_reader idx = SeekReadIndexedRow (ctxt.nrows tab_FieldMarshal,SeekReadFieldMarshalRow ctxt,fst,hfm_compare idx,is_sorted ctxt tab_FieldMarshal,(snd >> ReadBlobHeapAsNativeType ctxt))
   let cas = SeekReadCustomAttrs ctxt (TaggedIndex(hca_ParamDef,idx))
   if seq = 0 then

     ret_res := { !ret_res with 
                      returnMarshal=(if has_marshal then Some (fm_reader (TaggedIndex(hfm_ParamDef,idx))) else None);
                      returnCustomAttrs = cas }
   elif seq > Array.length params_res then dprintn "bad seq num. for param"
   else 
     params_res.[seq - 1] <- 
        { params_res.[seq - 1] with 
             paramMarshal=(if has_marshal then Some (fm_reader (TaggedIndex(hfm_ParamDef,idx))) else None);
             paramDefault = (if has_default then Some (SeekReadConstant ctxt (TaggedIndex(hc_ParamDef,idx))) else None);
             paramName = ReadStringHeapOption ctxt name_idx;
             paramIn = ((inout_masked &&& 0x0001) <> 0x0);
             paramOut = ((inout_masked &&& 0x0002) <> 0x0);
             paramOptional = ((inout_masked &&& 0x0010) <> 0x0);
             paramCustomAttrs =cas }
          
and SeekReadMethodImpls ctxt numtypars tidx =
   mk_lazy_mimpls 
      (lazy 
        begin 
          let mimpls = SeekReadIndexedRows (ctxt.nrows tab_MethodImpl,SeekReadMethodImplRow ctxt,(fun (a,_,_) -> a),simpleindex_compare tidx,is_sorted ctxt tab_MethodImpl,(fun (_,b,c) -> b,c))
          mimpls |> List.map (fun (b,c) -> 
              { mimplOverrideBy=(mk_mspec_in_typ (SeekReadMethodDefOrRefNoVarargs ctxt numtypars b));
                mimplOverrides=
                    (let mspec = (mk_mspec_in_typ (SeekReadMethodDefOrRefNoVarargs ctxt numtypars c))
                     OverridesSpec(mspec.MethodRef, mspec.EnclosingType)) })
        end)

and SeekReadMultipleMethodSemantics ctxt (flags,id) =
    SeekReadIndexedRows 
      (ctxt.nrows tab_MethodSemantics ,
       SeekReadMethodSemanticsRow ctxt,
       (fun (flags,_,c) -> c),
       hs_compare id,
       is_sorted ctxt tab_MethodSemantics,
       (fun (a,b,c) -> a, (mk_mspec_in_typ (SeekReadMethodDef_as_mdata ctxt b)).MethodRef))
    |> List.filter (fun (flags2,_) -> flags = flags2) 
    |> List.map snd 


and SeekReadoptional_MethodSemantics ctxt id =
  match SeekReadMultipleMethodSemantics ctxt id with 
    [] -> None
  | [h] -> Some h
  | h::t -> dprintn "multiple method semantics found"; Some h

and SeekReadMethodSemantics ctxt id =
   match SeekReadoptional_MethodSemantics ctxt id with 
   | None -> failwith "SeekReadMethodSemantics ctxt: no method found"
   | Some x -> x

and SeekReadEvent ctxt numtypars idx =
   let (flags,name_idx,typ_idx) = SeekReadEventRow ctxt idx
   { eventName = ReadStringHeap ctxt name_idx;
     eventType = SeekReadOptionalTypeDefOrRef ctxt numtypars AsObject typ_idx;
     eventSpecialName  = (flags &&& 0x0200) <> 0x0; 
     eventRTSpecialName = (flags &&& 0x0400) <> 0x0;
     eventAddOn= SeekReadMethodSemantics ctxt (0x0008,TaggedIndex(hs_Event, idx));
     eventRemoveOn=SeekReadMethodSemantics ctxt (0x0010,TaggedIndex(hs_Event,idx));
     eventFire=SeekReadoptional_MethodSemantics ctxt (0x0020,TaggedIndex(hs_Event,idx));
     eventOther = SeekReadMultipleMethodSemantics ctxt (0x0004, TaggedIndex(hs_Event, idx));
     eventCustomAttrs=SeekReadCustomAttrs ctxt (TaggedIndex(hca_Event,idx)) }
   
  (* REVIEW: can substantially reduce numbers of EventMap and PropertyMap reads by first checking if the whole table is sorted according to ILTypeDef tokens and then doing a binary chop *)
and SeekReadEvents ctxt numtypars tidx =
   mk_lazy_events 
      (lazy 
         begin 
           match SeekReadOptionalIndexedRow (ctxt.nrows tab_EventMap,(fun i -> i, SeekReadEventMapRow ctxt i),(fun (_,row) -> fst row),compare tidx,false,(fun (i,row) -> (i,snd row))) with 
           | None -> []
           | Some (row_num,begin_event_idx) ->
               let end_event_idx =
                   if row_num >= ctxt.nrows (tab_EventMap) then 
                       ctxt.nrows (tab_Event) + 1
                   else
                       let (_, end_event_idx) = SeekReadEventMapRow ctxt (row_num + 1)
                       end_event_idx

               [ for i in begin_event_idx .. end_event_idx - 1 do
                   yield SeekReadEvent ctxt numtypars i ]
         end)

and SeekReadProperty ctxt numtypars idx =
   let (flags,name_idx,typ_idx) = SeekReadPropertyRow ctxt idx
   let cc,retty,argtys = ReadBlobHeap_as_property_sig ctxt numtypars typ_idx
   let setter= SeekReadoptional_MethodSemantics ctxt (0x0001,TaggedIndex(hs_Property,idx))
   let getter = SeekReadoptional_MethodSemantics ctxt (0x0002,TaggedIndex(hs_Property,idx))
(* NOTE: the "hasthis" value on the property is not reliable: better to look on the getter/setter *)
(* NOTE: e.g. tlbimp on Office msword.olb seems to set this incorrectly *)
   let hasthis_of_callconv (Callconv (a,b)) = a
   let cc2 =
       match getter with 
       | Some mref -> hasthis_of_callconv mref.CallingConv 
       | None -> 
           match setter with 
           | Some mref ->  hasthis_of_callconv mref.CallingConv 
           | None -> cc
   { propName=ReadStringHeap ctxt name_idx;
     propCallconv = cc2;
     propRTSpecialName=(flags &&& 0x0400) <> 0x0; 
     propSpecialName= (flags &&& 0x0200) <> 0x0; 
     propSet=setter;
     propGet=getter;
     propType=retty;
     propInit= if (flags &&& 0x1000) = 0 then None else Some (SeekReadConstant ctxt (TaggedIndex(hc_Property,idx)));
     propArgs=argtys;
     propCustomAttrs=SeekReadCustomAttrs ctxt (TaggedIndex(hca_Property,idx)) }
   
and SeekReadProperties ctxt numtypars tidx =
   mk_lazy_properties
      (lazy 
         begin 
           match SeekReadOptionalIndexedRow (ctxt.nrows tab_PropertyMap,(fun i -> i, SeekReadPropertyMapRow ctxt i),(fun (_,row) -> fst row),compare tidx,false,(fun (i,row) -> (i,snd row))) with 
           | None -> []
           | Some (row_num,begin_prop_idx) ->
               let end_prop_idx =
                   if row_num >= ctxt.nrows (tab_PropertyMap) then 
                       ctxt.nrows (tab_Property) + 1
                   else
                       let (_, end_prop_idx) = SeekReadPropertyMapRow ctxt (row_num + 1)
                       end_prop_idx
               [ for i in begin_prop_idx .. end_prop_idx - 1 do
                   yield SeekReadProperty ctxt numtypars i ]
         end)


and SeekReadCustomAttrs ctxt idx = 
    mk_computed_custom_attrs
     (fun () ->
          SeekReadIndexedRows (ctxt.nrows tab_CustomAttribute,
                                  SeekReadCustomAttributeRow ctxt,(fun (a,_,_) -> a),
                                  hca_compare idx,
                                  is_sorted ctxt tab_CustomAttribute,
                                  (fun (_,b,c) -> SeekReadCustomAttr ctxt (b,c))))

and SeekReadCustomAttr ctxt (TaggedIndex(cat,idx),b) = 
    ctxt.SeekReadCustomAttr (CustomAttrIdx (cat,idx,b))

and SeekReadCustomAttrUncached ctxtH (CustomAttrIdx (cat,idx,val_idx)) = 
    let ctxt = getHole ctxtH
    { customMethod=SeekReadCustomAttrType ctxt (TaggedIndex(cat,idx));
      customData=
        match ReadBlobHeapOption ctxt val_idx with
        | Some bytes -> bytes
        | None -> Bytes.of_intarray [| |] }

and SeekReadSecurityDecls ctxt idx = 
   mk_lazy_security_decls
    (lazy
       begin
         SeekReadIndexedRows (ctxt.nrows tab_Permission,
                                 SeekReadPermissionRow ctxt,
                                 (fun (_,par,_) -> par),
                                 hds_compare idx,
                                 is_sorted ctxt tab_Permission,
                                 (fun (act,_,ty) -> SeekReadSecurityDecl ctxt (act,ty)))
       end)

and SeekReadSecurityDecl ctxt (a,b) = 
    ctxt.SeekReadSecurityDecl (SecurityDeclIdx (a,b))

and SeekReadSecurityDeclUncached ctxtH (SecurityDeclIdx (act,ty)) = 
    let ctxt = getHole ctxtH
    if logging then dprintn "reading SecurityDecl";
    PermissionSet ((if List.mem_assoc (int act) (Lazy.force secaction_rmap) then List.assoc (int act) (Lazy.force secaction_rmap) else failwith "unknown security action"),
                   ReadBlobHeap ctxt ty)


and SeekReadConstant ctxt idx =
  let kind,vidx = SeekReadIndexedRow (ctxt.nrows tab_Constant,
                                         SeekReadConstantRow ctxt,
                                         (fun (_,key,_) -> key), 
                                         hc_compare idx,is_sorted ctxt tab_Constant,(fun (kind,_,v) -> kind,v))
  match kind with 
  | x when x = et_STRING -> FieldInit_string (Bytes.unicode_bytes_as_string (ReadBlobHeap ctxt vidx))  | x when x = et_BOOLEAN -> FieldInit_bool (ReadBlobHeap_as_bool ctxt vidx) 
  | x when x = et_CHAR -> FieldInit_char (ReadBlobHeap_as_u16 ctxt vidx) 
  | x when x = et_I1 -> FieldInit_int8 (ReadBlobHeap_as_i8 ctxt vidx) 
  | x when x = et_I2 -> FieldInit_int16 (ReadBlobHeap_as_i16 ctxt vidx) 
  | x when x = et_I4 -> FieldInit_int32 (ReadBlobHeap_as_i32 ctxt vidx) 
  | x when x = et_I8 -> FieldInit_int64 (ReadBlobHeap_as_i64 ctxt vidx) 
  | x when x = et_U1 -> FieldInit_uint8 (ReadBlobHeap_as_u8 ctxt vidx) 
  | x when x = et_U2 -> FieldInit_uint16 (ReadBlobHeap_as_u16 ctxt vidx) 
  | x when x = et_U4 -> FieldInit_uint32 (ReadBlobHeap_as_u32 ctxt vidx) 
  | x when x = et_U8 -> FieldInit_uint64 (ReadBlobHeap_as_u64 ctxt vidx) 
  | x when x = et_R4 -> FieldInit_single (ReadBlobHeap_as_ieee32 ctxt vidx) 
  | x when x = et_R8 -> FieldInit_double (ReadBlobHeap_as_ieee64 ctxt vidx) 
  | x when x = et_CLASS or x = et_OBJECT ->  FieldInit_ref
  | _ -> FieldInit_ref

and SeekReadImplMap ctxt nm midx = 
   mk_lazy_mbody 
      (lazy 
          begin 
            if logging then dprintn ("reading pinvoke map for method "^string midx);       
            let (flags,name_idx, scope_idx) = SeekReadIndexedRow (ctxt.nrows tab_ImplMap,
                                                                     SeekReadImplMapRow ctxt,
                                                                     (fun (_,m,_,_) -> m),
                                                                     mf_compare (TaggedIndex(mf_MethodDef,midx)),
                                                                     is_sorted ctxt tab_ImplMap,
                                                                     (fun (a,_,c,d) -> a,c,d))
            let cc = 
              let masked = flags &&& 0x0700
              if masked = 0x0000 then PInvokeCallConvNone 
              elif masked = 0x0200 then PInvokeCallConvCdecl 
              elif masked = 0x0300 then PInvokeCallConvStdcall 
              elif masked = 0x0400 then PInvokeCallConvThiscall 
              elif masked = 0x0500 then PInvokeCallConvFastcall 
              elif masked = 0x0100 then PInvokeCallConvWinapi 
              else (dprintn "strange pinvokeCallconv"; PInvokeCallConvNone)
            let enc = 
              let masked = flags &&& 0x0006
              if masked = 0x0000 then PInvokeEncodingNone 
              elif masked = 0x0002 then PInvokeEncodingAnsi 
              elif masked = 0x0004 then PInvokeEncodingUnicode 
              elif masked = 0x0006 then PInvokeEncodingAuto 
              else (dprintn "strange PInvokeCharEncoding"; PInvokeEncodingNone)
            let bestfit = 
              let masked = flags &&& 0x0030
              if masked = 0x0000 then PInvokeBestFitUseAssem 
              elif masked = 0x0010 then PInvokeBestFitEnabled 
              elif masked = 0x0020 then PInvokeBestFitDisabled 
              else (dprintn "strange PInvokeCharBestFit"; PInvokeBestFitUseAssem)
            let unmap = 
              let masked = flags &&& 0x3000
              if masked = 0x0000 then PInvokeThrowOnUnmappableCharUseAssem 
              elif masked = 0x1000 then PInvokeThrowOnUnmappableCharEnabled 
              elif masked = 0x2000 then PInvokeThrowOnUnmappableCharDisabled 
              else (dprintn "strange PInvokeThrowOnUnmappableChar"; PInvokeThrowOnUnmappableCharUseAssem)

            MethodBody_pinvoke { pinvokeCallconv = cc; 
                                 PInvokeCharEncoding = enc;
                                 PInvokeCharBestFit=bestfit;
                                 PInvokeThrowOnUnmappableChar=unmap;
                                 pinvokeNoMangle = (flags &&& 0x0001) <> 0x0;
                                 pinvokeLastErr = (flags &&& 0x0040) <> 0x0;
                                 pinvokeName = 
                                     (match ReadStringHeapOption ctxt name_idx with 
                                      | None -> nm
                                      | Some nm2 -> nm2);
                                 pinvokeWhere = SeekReadModuleRef ctxt scope_idx }
          end)

and SeekReadTopCode ctxt nm numtypars (sz:int) start seqpoints = 
   let labels_of_raw_offsets = new Dictionary<_,_>(sz/2)
   let ilOffsetsOfLabels = new Dictionary<_,_>(sz/2)
   let try_raw2lab raw_offset = 
     if labels_of_raw_offsets.ContainsKey raw_offset then 
       Some(labels_of_raw_offsets.[raw_offset])
     else 
       None
   let raw2lab raw_offset = 
     match try_raw2lab raw_offset with 
     | Some l -> l
     | None -> 
       let lab = generate_code_label()
       labels_of_raw_offsets.[raw_offset] <- lab;
       lab
   let mark_as_instruction_start raw_offset il_offset = 
     let lab = raw2lab raw_offset
     ilOffsetsOfLabels.[lab] <- il_offset

   let ibuf = new ResizeArray<_>(sz/2)
   let curr = ref 0
   let prefixes = { al=Aligned; tl= Normalcall; vol= Nonvolatile;ro=NormalAddress;constrained=None }
   let lastb = ref 0x0
   let lastb2 = ref 0x0
   let b = ref 0x0
   let get () = 
       lastb := SeekReadByteAsInt32 ctxt.is (start + (!curr));
       incr curr;
       b := 
         if !lastb = 0xfe && !curr < sz then 
           lastb2 := SeekReadByteAsInt32 ctxt.is (start + (!curr));
           incr curr;
           !lastb2
         else 
           !lastb

   let seqpoints_remaining = ref seqpoints

   while !curr < sz do
     if logging then dprintn (ctxt.infile ^ ", "^nm^": registering "^string !curr^" as start of an instruction"); 
     mark_as_instruction_start !curr ibuf.Count;

     (* Insert any sequence points into the instruction sequence *)
       
     if logging then dprintn ("** #remaining sequence points @ "^string !curr ^ " = "^string (List.length !seqpoints_remaining));


     while 
         (match !seqpoints_remaining with 
          |  (i,tag) :: rest when i <= !curr -> true
          | _ -> false) 
        do
         if logging then dprintn ("** Emitting one sequence point ** ");
         let (_,tag) = List.hd !seqpoints_remaining
         seqpoints_remaining := List.tl !seqpoints_remaining;
         ibuf.Add (I_seqpoint tag)

     if logging then dprintn (ctxt.infile ^ ", "^nm^ ": instruction begins at "^string !curr); 
     (* Read the prefixes.  Leave lastb and lastb2 holding the instruction byte(s) *)
     begin 
       prefixes.al <- Aligned;
       prefixes.tl <- Normalcall;
       prefixes.vol <- Nonvolatile;
       prefixes.ro<-NormalAddress;
       prefixes.constrained<-None;
       get ();
       while !curr < sz && 
         !lastb = 0xfe &&
         (!b = (i_constrained &&& 0xff) or
          !b = (i_readonly &&& 0xff) or
          !b = (i_unaligned &&& 0xff) or
          !b = (i_volatile &&& 0xff) or
          !b = (i_tail &&& 0xff)) do
         begin
             if !b = (i_unaligned &&& 0xff) then
               let unal = SeekReadByteAsInt32 ctxt.is (start + (!curr))
               incr curr;
               prefixes.al <-
                  if unal = 0x1 then Unaligned_1 
                  elif unal = 0x2 then Unaligned_2
                  elif unal = 0x4 then Unaligned_4 
                  else (dprintn "bad alignment for unaligned";  Aligned)
             elif !b = (i_volatile &&& 0xff) then prefixes.vol <- Volatile
             elif !b = (i_readonly &&& 0xff) then prefixes.ro <- ReadonlyAddress
             elif !b = (i_constrained &&& 0xff) then 
                 let uncoded = SeekReaduncoded_token ctxt.is (start + (!curr))
                 curr := !curr + 4;
                 let typ = SeekReadTypeDefOrRef ctxt numtypars AsObject [] (uncoded_token_to_tdor uncoded)
                 prefixes.constrained <- Some typ
             else prefixes.tl <- Tailcall;
         end;
         get ();
       done;
     end;

     if logging then dprintn (ctxt.infile ^ ": data for instruction begins at "^string !curr); 
     (* Read and decode the instruction *)
     if (!curr <= sz) then 
       let idecoder = 
           if !lastb = 0xfe then get_two_byte_instr ( !lastb2)
           else get_one_byte_instr ( !lastb)
       let instr = 
         match idecoder with 
         | I_u16_u8_instr f -> 
             let x = SeekReadByteAsUInt16 ctxt.is (start + (!curr))
             curr := !curr + 1;
             f prefixes x
         | I_u16_u16_instr f -> 
             let x = SeekReadUInt16 ctxt.is (start + (!curr))
             curr := !curr + 2;
             f prefixes x
         | I_none_instr f -> 
             f prefixes 
         | I_i64_instr f ->
             let x = SeekReadInt64 ctxt.is (start + (!curr))
             curr := !curr + 8;
             f prefixes x
         | I_i32_i8_instr f ->
             let x = SeekReadSByteAsInt32 ctxt.is (start + (!curr))
             curr := !curr + 1;
             f prefixes x
         | I_i32_i32_instr f ->
             let x = SeekReadInt32 ctxt.is (start + (!curr))
             curr := !curr + 4;
             f prefixes x
         | I_r4_instr f ->
             let x = SeekReadSingle ctxt.is (start + (!curr))
             curr := !curr + 4;
             f prefixes x
         | I_r8_instr f ->
             let x = SeekReadDouble ctxt.is (start + (!curr))
             curr := !curr + 8;
             f prefixes x
         | I_field_instr f ->
             let (tab,tok) = SeekReaduncoded_token ctxt.is (start + (!curr))
             curr := !curr + 4;
             let fspec = 
               if tab = tab_Field then 
                 SeekReadFieldDef_as_fspec ctxt tok
               elif tab = tab_MemberRef then
                 SeekReadMemberRef_as_fspec ctxt numtypars tok
               else failwith "bad table in FieldDefOrRef"
             f prefixes fspec
         | I_method_instr f ->
             if logging then dprintn (ctxt.infile ^ ": method instruction, curr = "^string !curr); 
       
             let (tab,idx) = SeekReaduncoded_token ctxt.is (start + (!curr))
             curr := !curr + 4;
             let  (encl_typ, cc, nm, argtys,varargs, retty, minst) =
               if tab = tab_Method then 
                 SeekReadMethodDefOrRef ctxt numtypars (TaggedIndex(mdor_MethodDef, idx))
               elif tab = tab_MemberRef then 
                 SeekReadMethodDefOrRef ctxt numtypars (TaggedIndex(mdor_MemberRef, idx))
               elif tab = tab_MethodSpec then 
                 SeekReadMethodSpec_as_mdata ctxt numtypars idx  
               else failwith "bad table in MethodDefOrRefOrSpec" 
             if is_array_ty encl_typ then 
               let (shape,ty) = dest_array_ty encl_typ
               match nm with
               | "Get" -> I_ldelem_any(shape,ty)
               | "Set" ->  I_stelem_any(shape,ty)
               | "Address" ->  I_ldelema(prefixes.ro, shape,ty)
               | ".ctor" ->  I_newarr(shape,ty)
               | _ -> failwith "bad method on array type"
             else 
               let mspec = (mk_mspec_in_typ (encl_typ, cc, nm, argtys, retty, minst))
               f prefixes (mspec,varargs)
         | I_type_instr f ->
             let uncoded = SeekReaduncoded_token ctxt.is (start + (!curr))
             curr := !curr + 4;
             let typ = SeekReadTypeDefOrRef ctxt numtypars AsObject [] (uncoded_token_to_tdor uncoded)
             f prefixes typ
         | I_string_instr f ->
             let (tab,idx) = SeekReaduncoded_token ctxt.is (start + (!curr))
             curr := !curr + 4;
             if tab <> tab_UserStrings then dprintn "warning: bad table in user string for ldstr";
             f prefixes (ReadUserStringHeap ctxt (idx))

         | I_conditional_i32_instr f ->
             let offs_dest =  (SeekReadInt32 ctxt.is (start + (!curr)))
             curr := !curr + 4;
             let dest = !curr + offs_dest
             let next = !curr
             f prefixes (raw2lab dest, raw2lab next)
         | I_conditional_i8_instr f ->
             let offs_dest = int (SeekReadSByte ctxt.is (start + (!curr)))
             curr := !curr + 1;
             let dest = !curr + offs_dest
             let next = !curr
             f prefixes (raw2lab dest, raw2lab next)
         | I_unconditional_i32_instr f ->
             let offs_dest =  (SeekReadInt32 ctxt.is (start + (!curr)))
             curr := !curr + 4;
             let dest = !curr + offs_dest
             f prefixes (raw2lab dest)
         | I_unconditional_i8_instr f ->
             let offs_dest = int (SeekReadSByte ctxt.is (start + (!curr)))
             curr := !curr + 1;
             let dest = !curr + offs_dest
             f prefixes (raw2lab dest)
         | I_invalid_instr -> dprintn ("invalid instruction: "^string !lastb^ (if !lastb = 0xfe then ","^string !lastb2 else "")); I_ret
         | I_tok_instr f ->  
             let (tab,idx) = SeekReaduncoded_token ctxt.is (start + (!curr))
             curr := !curr + 4;
             (* REVIEW: this incorrectly labels all MemberRef tokens as Token_method's: we should go look at the MemberRef sig to determine if it ctxt.is a field or method *)        
             let token_info = 
               if tab = tab_Method or tab = tab_MemberRef (* REVIEW:generics or tab = tab_MethodSpec *) then 
                 Token_method ((mk_mspec_in_typ (SeekReadMethodDefOrRefNoVarargs ctxt numtypars (uncoded_token_to_mdor (tab,idx)))))
               elif tab = tab_Field then 
                 Token_field (SeekReadFieldDef_as_fspec ctxt idx)
               elif tab = tab_TypeDef or tab = tab_TypeRef or tab = tab_TypeSpec  then 
                 Token_type (SeekReadTypeDefOrRef ctxt numtypars AsObject [] (uncoded_token_to_tdor (tab,idx))) 
               else failwith "bad token for ldtoken" 
             f prefixes token_info
         | I_sig_instr f ->  
             let (tab,idx) = SeekReaduncoded_token ctxt.is (start + (!curr))
             curr := !curr + 4;
             if tab <> tab_StandAloneSig then dprintn "strange table for callsig token";
             let generic,genarity,cc,retty,argtys,varargs = ReadBlobHeap_as_method_sig ctxt numtypars (SeekReadStandAloneSigRow ctxt idx)
             if generic then failwith "bad image: a generic method signature ctxt.is begin used at a calli instruction";
             f prefixes (mk_callsig (cc,argtys,retty), varargs)
         | I_switch_instr f ->  
             let n =  (SeekReadInt32 ctxt.is (start + (!curr)))
             curr := !curr + 4;
             let offsets = 
               List.init n (fun _ -> 
                   let i =  (SeekReadInt32 ctxt.is (start + (!curr)))
                   curr := !curr + 4; 
                   i) 
             let dests = List.map (fun offs -> raw2lab (!curr + offs)) offsets
             let next = raw2lab !curr
             f prefixes (dests,next)
       ibuf.Add instr
   done;
   (* Finished reading instructions - mark the end of the instruction stream in case the PDB information refers to it. *)
   mark_as_instruction_start !curr ibuf.Count;
   (* Build the function that maps from raw labels (offsets into the bytecode stream) to indexes in the AbsIL instruction stream *)
   let lab2pc lab = 
       try
          ilOffsetsOfLabels.[lab]
       with Not_found -> failwith ("branch destination "^string_of_code_label lab^" not found in code")

   // Some offsets used in debug info refer to the end of an instruction, rather than the 
   // start of the subsequent instruction.  But all labels refer to instruction starts, 
   // apart from a final label which refers to the end of the method.  This function finds 
   // the start of the next instruction referred to by the raw offset. 
   let raw2nextLab raw_offset = 
       let isInstrStart x = 
         match try_raw2lab x with 
         | None -> false
         | Some lab -> ilOffsetsOfLabels.ContainsKey lab
       if  isInstrStart raw_offset then raw2lab raw_offset 
       elif  isInstrStart (raw_offset+1) then raw2lab (raw_offset+1)
       else failwith ("the bytecode raw offset "^string raw_offset^" did not refer either to the start or end of an instruction")
   let instrs = ibuf  |> ResizeArray.to_array
   instrs,raw2lab, lab2pc, raw2nextLab

and SeekReadMethodRVA ctxt (idx,nm,internalcall,noinline,numtypars) rva = 
  mk_lazy_mbody 
   (lazy
     begin 

       // Read any debug information for this method into temporary data structures 
       //    -- a list of locals, marked with the raw offsets (actually closures which accept the resolution function that maps raw offsets to labels) 
       //    -- an overall range for the method 
       //    -- the sequence points for the method 
       let local_pdb_infos, mrange_pdb_info, seqpoints = 
         match ctxt.pdb with 
         | None -> 
             [], None, []
         | Some (pdbr, get_doc) -> 
               try 

                 let pdbm = pdbReaderGetMethod pdbr (uncoded_token tab_Method idx)
                 let rootScope = pdbMethodGetRootScope pdbm 
                 let sps = pdbMethodGetSequencePoints pdbm
                 (*dprintf "#sps for 0x%lx = %d\n" (uncoded_token tab_Method idx) (Array.length sps);  *)
                 (* let roota,rootb = pdbScopeGetOffsets rootScope in  *)
                 let seqpoints =
                    let arr = 
                       sps |> Array.map (fun sp -> 
                           (* It is VERY annoying to have to call GetURL for the document for each sequence point.  This appears to be a short coming of the PDB reader API.  They should return an index into the array of documents for the reader *)
                           let sourcedoc = get_doc (pdbDocumentGetURL sp.pdbSeqPointDocument)
                           let source = 
                             ILSourceMarker.Create(document = sourcedoc,
                                                 line = sp.pdbSeqPointLine,
                                                 column = sp.pdbSeqPointColumn,
                                                 endLine = sp.pdbSeqPointEndLine,
                                                 endColumn = sp.pdbSeqPointEndColumn)
                           (sp.pdbSeqPointOffset,source))
                         
                    Array.sortInPlaceBy fst arr;
                    
                    Array.to_list arr
                 let rec scopes scp = 
                       let a,b = pdbScopeGetOffsets scp
                       let lvs =  pdbScopeGetLocals scp
                       let ilvs = 
                         lvs 
                         |> Array.to_list 
                         |> List.filter (fun l -> 
                             let k,idx = pdbVariableGetAddressAttributes l
                             k = 1 (* ADDR_IL_OFFSET *)) 
                       let ilinfos =
                         ilvs |> List.map (fun ilv -> 
                             let k,idx = pdbVariableGetAddressAttributes ilv
                             let n = pdbVariableGetName ilv
                             if logging then dprintn ("local variable debug info: name="^n^", kind = "^string k^", localNum = "^string idx); 
                             { localNum=  idx; 
                               localName=n})
                           
                       let this_one = 
                         (fun raw2nextLab ->
                           { locRange= (raw2nextLab a,raw2nextLab b); 
                             locInfos = ilinfos })
                       if logging then dprintn ("this scope covers IL range: "^string a^"-"^string b); 
                       let others = List.foldBack (scopes >> (@)) (Array.to_list (pdbScopeGetChildren scp)) []
                       this_one :: others
                 let local_pdb_infos = [] (* <REVIEW> scopes fail for mscorlib </REVIEW> scopes rootScope  *)
                 if logging then dprintn ("done local_pdb_infos"); 
                 // REVIEW: look through sps to get ranges?  Use GetRanges?? Change AbsIL?? 
                 (local_pdb_infos,None,seqpoints)
               with e -> 
                   if logging then dprintn ("* Warning: PDB info for method "^nm^" could not be read and will be ignored: "^e.Message);
                   [],None,[]
       
       
       let baseRVA = ctxt.anyV2P("method rva",rva)
       if logging then dprintn (ctxt.infile ^ ": reading body of method "^nm^" at rva "^string rva^", phys "^string baseRVA); 
       let b = SeekReadByteAsInt32 ctxt.is baseRVA
       if (b &&& e_CorILMethod_FormatMask) = e_CorILMethod_TinyFormat then 
         let code_base = baseRVA + 1
         let code_size =  (b lsr 2)
         if logging then dprintn (ctxt.infile ^ ": tiny format for "^nm^", code size = " ^ string code_size);
         let instrs,_,lab2pc,raw2nextLab = SeekReadTopCode ctxt nm numtypars code_size code_base seqpoints
         (* Convert the linear code format to the nested code format *)
         if logging then dprintn ("doing local_pdb_infos2 (tiny format)"); 
         let local_pdb_infos2 = List.map (fun f -> f raw2nextLab) local_pdb_infos
         if logging then dprintn ("done local_pdb_infos2 (tiny format), checking code..."); 
         let code = check_code (build_code nm lab2pc instrs [] local_pdb_infos2)
         if logging then dprintn ("done checking code (tiny format)."); 
         MethodBody_il
           { ilZeroInit=false;
             ilMaxStack= 8;
             ilNoInlining=noinline;
             ilLocals=[];
             ilSource=mrange_pdb_info; 
             ilCode=code }

       elif (b &&& e_CorILMethod_FormatMask) = e_CorILMethod_FatFormat then 
         let has_more_sects = (b &&& e_CorILMethod_MoreSects) <> 0x0
         let initlocals = (b &&& e_CorILMethod_InitLocals) <> 0x0
         let maxstack = SeekReadUInt16AsInt32 ctxt.is (baseRVA + 2)
         let code_size = SeekReadInt32 ctxt.is (baseRVA + 4)
         let locals_tab,localtoken = SeekReaduncoded_token ctxt.is (baseRVA + 8)
         let code_base = baseRVA + 12
         let locals = 
           if localtoken = 0x0 then [] 
           else 
             if locals_tab <> tab_StandAloneSig then dprintn "strange table for locals token";
             ReadBlobHeap_as_locals_sig ctxt numtypars (SeekReadStandAloneSigRow ctxt localtoken) 
           

         if logging then dprintn (ctxt.infile ^ ": fat format for "^nm^", code size = " ^ string code_size^", has_more_sects = "^(if has_more_sects then "true" else "false")^",b = "^string b);
         
         (* Read the method body *)
         let instrs,raw2lab,lab2pc,raw2nextLab = SeekReadTopCode ctxt nm numtypars ( code_size) code_base seqpoints

         (* Read all the sections that follow the method body. *)
         (* These contain the exception clauses. *)
         let next_sect_base = ref (align 4 (code_base + code_size))
         let more_sects = ref has_more_sects
         let seh = ref []
         while !more_sects do
           let sect_base = !next_sect_base
           let sect_flag = SeekReadByteAsInt32 ctxt.is sect_base
           if logging then dprintn (ctxt.infile ^ ": fat format for "^nm^", sect_flag = " ^ string sect_flag);
           let sect_size, clauses = 
             if (sect_flag &&& e_CorILMethod_Sect_FatFormat) <> 0x0 then 
                 let big_size = (SeekReadInt32 ctxt.is sect_base) lsr 8
                 if logging then dprintn (nm^": one more section");
                 if logging then dprintn (ctxt.infile ^ ": big_size = "^string big_size);
                 let clauses = 
                     if (sect_flag &&& e_CorILMethod_Sect_EHTable) <> 0x0 then 
                         // WORKAROUND: The ECMA spec says this should be  
                         // let num_clauses =  ((big_size - 4)  / 24) in  
                         // but the CCI IL generator generates multiples of 24
                         let num_clauses =  (big_size  / 24)
                         if logging then dprintn (nm^" has "^string num_clauses ^" fat seh clauses");
                         
                         List.init num_clauses (fun i -> 
                             let clause_base = sect_base + 4 + (i * 24)
                             let kind = SeekReadInt32 ctxt.is (clause_base + 0)
                             if logging then dprintn ("One fat SEH clause, kind = "^string kind);
                             let st1 = SeekReadInt32 ctxt.is (clause_base + 4)
                             let sz1 = SeekReadInt32 ctxt.is (clause_base + 8)
                             let st2 = SeekReadInt32 ctxt.is (clause_base + 12)
                             let sz2 = SeekReadInt32 ctxt.is (clause_base + 16)
                             let extra = SeekReadInt32 ctxt.is (clause_base + 20)
                             (kind,st1,sz1,st2,sz2,extra))
                     else []
                 big_size, clauses
             else 
               let small_size = SeekReadByteAsInt32 ctxt.is (sect_base + 0x01)
               let clauses = 
                 if (sect_flag &&& e_CorILMethod_Sect_EHTable) <> 0x0 then begin
                   if logging then dprintn (nm^": small_size = "^string small_size);
                   (* WORKAROUND: The ECMA spec says this should be  *)
                   (* let num_clauses =  ((small_size - 4)  / 12) in  *)
                   (* but the C# compiler (or some IL generator) generates multiples of 12 *)
                   let num_clauses =  (small_size  / 12)
                   if logging then dprintn (nm^" has "^string num_clauses ^" tiny seh clauses");
                   List.init num_clauses (fun i -> 
                       let clause_base = sect_base + 4 + (i * 12)
                       let kind = SeekReadUInt16AsInt32 ctxt.is (clause_base + 0)
                       if logging then dprintn ("One tiny SEH clause, kind = "^string kind);
                       let st1 = SeekReadUInt16AsInt32 ctxt.is (clause_base + 2)
                       let sz1 = SeekReadByteAsInt32 ctxt.is (clause_base + 4)
                       let st2 = SeekReadUInt16AsInt32 ctxt.is (clause_base + 5)
                       let sz2 = SeekReadByteAsInt32 ctxt.is (clause_base + 7)
                       let extra = SeekReadInt32 ctxt.is (clause_base + 8)
                       (kind,st1,sz1,st2,sz2,extra))
                 end else []
               small_size, clauses

           (* Morph together clauses that cover the same range *)
           let seh_clauses = 
              let seh_map = Dictionary.create (List.length clauses)
      
              List.iter
                (fun (kind,st1,sz1,st2,sz2,extra) ->
                  let try_start = raw2lab ( st1)
                  let try_finish = raw2lab ( (st1 + sz1))
                  let handler_start = raw2lab ( st2)
                  let handler_finish = raw2lab ( (st2 + sz2))
                  let clause = 
                    if kind = e_COR_ILEXCEPTION_CLAUSE_EXCEPTION then 
                      SEH_type_catch(SeekReadTypeDefOrRef ctxt numtypars AsObject [] (uncoded_token_to_tdor (i32_to_uncoded_token extra)), (handler_start, handler_finish) )
                    elif kind = e_COR_ILEXCEPTION_CLAUSE_FILTER then 
                      let filter_start = raw2lab ( extra)
                      let filter_finish = handler_start
                      SEH_filter_catch((filter_start, filter_finish), (handler_start, handler_finish))
                    elif kind = e_COR_ILEXCEPTION_CLAUSE_FINALLY then 
                      SEH_finally(handler_start, handler_finish)
                    elif kind = e_COR_ILEXCEPTION_CLAUSE_FAULT then 
                      SEH_fault(handler_start, handler_finish)
                    else begin
                      dprintn (ctxt.infile ^ ": unknown exception handler kind: "^string kind);
                      SEH_finally(handler_start, handler_finish)
                    end
                 
                  let key =  (try_start, try_finish)
                  if Dictionary.mem seh_map key then 
                    let prev = Dictionary.find seh_map key
                    Dictionary.replace seh_map key (prev @ [clause])
                  else 
                    Dictionary.add seh_map key [clause])
                clauses;
              Dictionary.fold  (fun key bs acc -> {exnRange=key; exnClauses=bs} :: acc) seh_map []
           seh := seh_clauses;
           more_sects := (sect_flag &&& e_CorILMethod_Sect_MoreSects) <> 0x0;
           next_sect_base := sect_base + sect_size;
         done; (* while *)

         (* Convert the linear code format to the nested code format *)
         if logging then dprintn ("doing local_pdb_infos2"); 
         let local_pdb_infos2 = List.map (fun f -> f raw2nextLab) local_pdb_infos
         if logging then dprintn ("done local_pdb_infos2, checking code..."); 
         let code = check_code (build_code nm lab2pc instrs !seh local_pdb_infos2)
         if logging then dprintn ("done checking code."); 
         MethodBody_il
           { ilZeroInit=initlocals;
             ilMaxStack= maxstack;
             ilNoInlining=noinline;
             ilLocals=locals;
             ilCode=code;
             ilSource=mrange_pdb_info}
       else 
         if logging then failwith "unknown format";
         MethodBody_abstract
     end)

and i32_as_variant_typ ctxt (n:int32) = 
    if List.mem_assoc n (Lazy.force variant_type_rmap) then 
      List.assoc n (Lazy.force variant_type_rmap)
    elif (n &&& vt_ARRAY) <> 0x0 then VariantType_array (i32_as_variant_typ ctxt (n &&& (~~~ vt_ARRAY)))
    elif (n &&& vt_VECTOR) <> 0x0 then VariantType_vector (i32_as_variant_typ ctxt (n &&& (~~~ vt_VECTOR)))
    elif (n &&& vt_BYREF) <> 0x0 then VariantType_byref (i32_as_variant_typ ctxt (n &&& (~~~ vt_BYREF)))
    else (dprintn (ctxt.infile ^ ": i32_as_variant_typ ctxt: unexpected variant type, n = "^string n) ; VariantType_empty)

and ReadBlobHeapAsNativeType ctxt blob_idx = 
    if logging then dprintn (ctxt.infile ^ ": reading native type blob "^string blob_idx); 
    let bytes = ReadBlobHeap ctxt blob_idx
    let res,_ = sigptr_get_native_typ ctxt bytes 0
    res

and sigptr_get_native_typ ctxt bytes sigptr = 
    if logging then dprintn (ctxt.infile ^ ": reading native type blob, sigptr= "^string sigptr); 
    let ntbyte,sigptr = sigptr_get_byte bytes sigptr
    if List.mem_assoc ntbyte (Lazy.force native_type_map) then 
        List.assoc ntbyte (Lazy.force native_type_map), sigptr
    elif ntbyte = 0x0 then NativeType_empty, sigptr
    elif ntbyte = nt_CUSTOMMARSHALER then  
        if logging then
          for i = 0 to Bytes.length bytes - 1 do
            if logging then dprintn (ctxt.infile ^ ": byte "^string i^" = "^string(Bytes.get bytes i));
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM1) , sigptr= "^string sigptr^ ", Bytes.length bytes = "^string(Bytes.length bytes)); 
        let guidLen,sigptr = sigptr_get_z_i32 bytes sigptr
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM2) , sigptr= "^string sigptr^", guidLen = "^string ( guidLen)); 
        let guid,sigptr = sigptr_get_bytes ( guidLen) bytes sigptr
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM3) , sigptr= "^string sigptr); 
        let nativeTypeNameLen,sigptr = sigptr_get_z_i32 bytes sigptr
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM4) , sigptr= "^string sigptr^", nativeTypeNameLen = "^string ( nativeTypeNameLen)); 
        let nativeTypeName,sigptr = sigptr_get_string ( nativeTypeNameLen) bytes sigptr
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM4) , sigptr= "^string sigptr^", nativeTypeName = "^nativeTypeName); 
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM5) , sigptr= "^string sigptr); 
        let custMarshallerNameLen,sigptr = sigptr_get_z_i32 bytes sigptr
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM6) , sigptr= "^string sigptr^", custMarshallerNameLen = "^string ( custMarshallerNameLen)); 
        let custMarshallerName,sigptr = sigptr_get_string ( custMarshallerNameLen) bytes sigptr
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM7) , sigptr= "^string sigptr^", custMarshallerName = "^custMarshallerName); 
        let cookieStringLen,sigptr = sigptr_get_z_i32 bytes sigptr
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM8) , sigptr= "^string sigptr^", cookieStringLen = "^string ( cookieStringLen)); 
        let cookieString,sigptr = sigptr_get_bytes ( cookieStringLen) bytes sigptr
        if logging then dprintn (ctxt.infile ^ ": reading native type blob (CM9) , sigptr= "^string sigptr); 
        NativeType_custom (guid,nativeTypeName,custMarshallerName,cookieString), sigptr
    elif ntbyte = nt_FIXEDSYSSTRING then 
      let i,sigptr = sigptr_get_z_i32 bytes sigptr
      NativeType_fixed_sysstring i, sigptr
    elif ntbyte = nt_FIXEDARRAY then 
      let i,sigptr = sigptr_get_z_i32 bytes sigptr
      NativeType_fixed_array i, sigptr
    elif ntbyte = nt_SAFEARRAY then 
      (if sigptr >= Bytes.length bytes then
         NativeType_safe_array(VariantType_empty, None),sigptr
       else 
         let i,sigptr = sigptr_get_z_i32 bytes sigptr
         if sigptr >= Bytes.length bytes then
           NativeType_safe_array (i32_as_variant_typ ctxt i, None), sigptr
         else 
           let len,sigptr = sigptr_get_z_i32 bytes sigptr
           let s,sigptr = sigptr_get_string ( len) bytes sigptr
           NativeType_safe_array (i32_as_variant_typ ctxt i, Some s), sigptr)
    elif ntbyte = nt_ARRAY then 
       if sigptr >= Bytes.length bytes then
         NativeType_array(None,None),sigptr
       else 
         let nt,sigptr = 
           let u,sigptr' = sigptr_get_z_i32 bytes sigptr
           if (u = nt_MAX) then 
             NativeType_empty, sigptr'
           else
           (* note: go back to start and read native type *)
             sigptr_get_native_typ ctxt bytes sigptr
         if sigptr >= Bytes.length bytes then
           NativeType_array (Some nt,None), sigptr
         else
           let pnum,sigptr = sigptr_get_z_i32 bytes sigptr
           if sigptr >= Bytes.length bytes then
             NativeType_array (Some nt,Some(pnum,None)), sigptr
           else 
             let additive,sigptr = 
               if sigptr >= Bytes.length bytes then 0, sigptr
               else sigptr_get_z_i32 bytes sigptr
             NativeType_array (Some nt,Some(pnum,Some(additive))), sigptr
    else (dprintn (ctxt.infile ^ ": unexpected native type, nt = "^string ntbyte); NativeType_empty, sigptr)
      
and SeekReadManifestResources ctxt () = 
    mk_lazy_resources 
      (lazy
         [ for i = 1 to ctxt.nrows (tab_ManifestResource) do
             let (offset,flags,name_idx,impl_idx) = SeekReadManifestResourceRow ctxt i
             let scoref = SeekReadImplAsScopeRef ctxt impl_idx
             let datalab = 
               match scoref with
               | ScopeRef_local -> 
                  let start = ctxt.anyV2P ("resource",offset + ctxt.resources_addr)
                  let len = SeekReadInt32 ctxt.is start
                  Resource_local (fun () -> SeekReadBytes ctxt.is (start + 4) len)
               | ScopeRef_module mref -> Resource_file (mref,offset)
               | ScopeRef_assembly aref -> Resource_assembly aref

             let r = 
               { resourceName= ReadStringHeap ctxt name_idx;
                 resourceWhere = datalab;
                 resourceAccess = (if (flags &&& 0x01) <> 0x0 then Resource_public else Resource_private);
                 resourceCustomAttrs =  SeekReadCustomAttrs ctxt (TaggedIndex(hca_ManifestResource, i)) }
             yield r ])


and SeekReadNestedExportedTypes ctxt parent_idx = 
    mk_lazy_nested_exported_types
      (lazy
         [ for i = 1 to ctxt.nrows tab_ExportedType do
               let (flags,tok,name_idx,namespace_idx,impl_idx) = SeekReadExportedTypeRow ctxt i
               if not (seek_is_top_TypeDef flags) then
                   let (TaggedIndex(tag,idx) ) = impl_idx
               //let isTopTypeDef =  (idx = 0 || tag <> i_ExportedType) 
               //if not isTopTypeDef then
                   match tag with 
                   | tag when tag = i_ExportedType && idx = parent_idx  ->
                       let nm = ReadBlobHeapAsTypeName ctxt (name_idx,namespace_idx)
                       yield 
                         { nestedExportedTypeName=nm;
                           nestedExportedTypeAccess=(match type_access_of_flags flags with TypeAccess_nested n -> n | _ -> failwith "non-nested access for a nested type described as being in an auxiliary module");
                           nestedExportedTypeNested=SeekReadNestedExportedTypes ctxt i;
                           nestedExportedTypeCustomAttrs=SeekReadCustomAttrs ctxt (TaggedIndex(hca_ExportedType, i)) } 
                   | _ -> () ])
      
and SeekReadTopExportedTypes ctxt () = 
    mk_lazy_exported_types 
      (lazy
         begin 
           let res = ref []
           for i = 1 to ctxt.nrows tab_ExportedType do
             let (flags,tok,name_idx,namespace_idx,impl_idx) = SeekReadExportedTypeRow ctxt i
             if seek_is_top_TypeDef flags then 
             //let (TaggedIndex(tag,idx) ) = impl_idx
             //let isTopTypeDef =  (idx = 0 || tag <> i_ExportedType) 

             //if isTopTypeDef then 
                 try 
                   let nm = ReadBlobHeapAsTypeName ctxt (name_idx,namespace_idx)
                   let scoref = SeekReadImplAsScopeRef ctxt impl_idx
                   let entry = 
                     { exportedTypeScope=scoref;
                       exportedTypeName=nm;
                       exportedTypeForwarder =   ((flags &&& 0x00200000) <> 0);
                       exportedTypeAccess=type_access_of_flags flags;
                       exportedTypeNested=SeekReadNestedExportedTypes ctxt i;
                       exportedTypeCustomAttrs=SeekReadCustomAttrs ctxt (TaggedIndex(hca_ExportedType, i)) } 
                   res := entry :: !res;
                 with _ -> // BUG 3794: Type forwarding is broken for nested types
                  ()
           done;
           List.rev !res
         end)
         
let getPDBReader opts infile =  
    match opts.pdbPath with 
    | None -> None
    | Some pdbpath ->
         try 
              let pdbr = pdbReadOpen infile pdbpath
              let pdbdocs = pdbReaderGetDocuments pdbr
  
              let tab = new Dictionary<_,_>(Array.length pdbdocs)
              pdbdocs |> Array.iter  (fun pdbdoc -> 
                  let url = pdbDocumentGetURL pdbdoc
                  tab.[url] <-
                      ILSourceDocument.Create(language=Some (pdbDocumentGetLanguage pdbdoc),
                                            vendor = Some (pdbDocumentGetLanguageVendor pdbdoc),
                                            documentType = Some (pdbDocumentGetType pdbdoc),
                                            file = url));

              let docfun url = if tab.ContainsKey url then tab.[url] else failwith ("Document with URL "^url^" not found in list of documents in the PDB file")
              Some (pdbr, docfun)
          with e -> dprintn ("* Warning: PDB file could not be read and will be ignored: "^e.Message); None         
      
(*-----------------------------------------------------------------------
 * Crack the binary headers, build a reader context and return the lazy
 * read of the AbsIL module.
 * ----------------------------------------------------------------------*)

let rec gen_open_binary_reader infile is opts = 

    (* MSDOS HEADER *)
    let pe_signature_phys_loc = SeekReadInt32 is 0x3c

    (* PE HEADER *)
    let pe_file_header_phys_loc = pe_signature_phys_loc + 0x04
    let pe_optional_header_phys_loc = pe_file_header_phys_loc + 0x14
    let pe_signature = SeekReadInt32 is (pe_signature_phys_loc + 0)
    if pe_signature <>  0x4550 then failwithf "not a PE file - bad magic PE number 0x%08x, is = %A" pe_signature is;


    (* PE SIGNATURE *)
    let machine = SeekReadUInt16AsInt32 is (pe_file_header_phys_loc + 0)
    let num_sections = SeekReadUInt16AsInt32 is (pe_file_header_phys_loc + 2)
    let opt_header_size = SeekReadUInt16AsInt32 is (pe_file_header_phys_loc + 16)
    if opt_header_size <>  0xe0 &&
       opt_header_size <> 0xf0 then failwith "not a PE file - bad optional header size";
    let x64adjust = opt_header_size - 0xe0
    let only64 = (opt_header_size = 0xf0)    (* May want to read in the optional header Magic number and check that as well... *)
    let platform = match machine with | 0x8664 -> Some(AMD64) | 0x200 -> Some(IA64) | _ -> Some(X86) 
    let section_headers_start_phys_loc = pe_optional_header_phys_loc + opt_header_size

    let flags = SeekReadUInt16AsInt32 is (pe_file_header_phys_loc + 18)
    let is_dll = (flags &&& 0x2000) <> 0x0

   (* OPTIONAL PE HEADER *)
    let text_phys_size = SeekReadInt32 is (pe_optional_header_phys_loc + 4)  (* Size of the code (text) section, or the sum of all code sections if there are multiple sections. *)
     (* x86: 000000a0 *) 
    let initdata_phys_size   = SeekReadInt32 is (pe_optional_header_phys_loc + 8) (* Size of the initialized data section, or the sum of all such sections if there are multiple data sections. *)
    let uninitdata_phys_size = SeekReadInt32 is (pe_optional_header_phys_loc + 12) (* Size of the uninitialized data section, or the sum of all such sections if there are multiple data sections. *)
    let entrypoint_addr      = SeekReadInt32 is (pe_optional_header_phys_loc + 16) (* RVA of entry point , needs to point to bytes 0xFF 0x25 followed by the RVA+!0x4000000 in a section marked execute/read for EXEs or 0 for DLLs e.g. 0x0000b57e *)
    let text_addr            = SeekReadInt32 is (pe_optional_header_phys_loc + 20) (* e.g. 0x0002000 *)
     (* x86: 000000b0 *) 
    let data_addr       = SeekReadInt32 is (pe_optional_header_phys_loc + 24) (* e.g. 0x0000c000 *)
    (*  REVIEW: For now, we'll use the DWORD at offset 24 for x64.  This currently ok since fsc doesn't support true 64-bit image bases,
        but we'll have to fix this up when such support is added. *)    
    let image_base_real = if only64 then data_addr else SeekReadInt32 is (pe_optional_header_phys_loc + 28)  (* Image Base Always 0x400000 (see Section 23.1). - QUERY : no it's not always 0x400000, e.g. 0x034f0000 *)
    let align_virt      = SeekReadInt32 is (pe_optional_header_phys_loc + 32)   (*  Section Alignment Always 0x2000 (see Section 23.1). *)
    let align_phys      = SeekReadInt32 is (pe_optional_header_phys_loc + 36)  (* File Alignment Either 0x200 or 0x1000. *)
     (* x86: 000000c0 *) 
    let os_major     = SeekReadUInt16 is (pe_optional_header_phys_loc + 40)   (*  OS Major Always 4 (see Section 23.1). *)
    let os_minor     = SeekReadUInt16 is (pe_optional_header_phys_loc + 42)   (* OS Minor Always 0 (see Section 23.1). *)
    let user_major   = SeekReadUInt16 is (pe_optional_header_phys_loc + 44)   (* User Major Always 0 (see Section 23.1). *)
    let user_minor   = SeekReadUInt16 is (pe_optional_header_phys_loc + 46)   (* User Minor Always 0 (see Section 23.1). *)
    let subsys_major = SeekReadUInt16 is (pe_optional_header_phys_loc + 48)   (* SubSys Major Always 4 (see Section 23.1). *)
    let subsys_minor = SeekReadUInt16 is (pe_optional_header_phys_loc + 50)   (* SubSys Minor Always 0 (see Section 23.1). *)
     (* x86: 000000d0 *) 
    let image_end_addr   = SeekReadInt32 is (pe_optional_header_phys_loc + 56)  (* Image Size: Size, in bytes, of image, including all headers and padding; shall be a multiple of Section Alignment. e.g. 0x0000e000 *)
    let header_phys_size = SeekReadInt32 is (pe_optional_header_phys_loc + 60)  (* Header Size Combined size of MS-DOS Header, PE Header, PE Optional Header and padding; shall be a multiple of the file alignment. *)
    let subsys           = SeekReadUInt16 is (pe_optional_header_phys_loc + 68)   (* SubSystem Subsystem required to run this image. Shall be either IMAGE_SUBSYSTEM_WINDOWS_CE_GUI (!0x3) or IMAGE_SUBSYSTEM_WINDOWS_GUI (!0x2). QUERY: Why is this 3 on the images ILASM produces??? *)
     (* x86: 000000e0 *) 

    (* WARNING: THESE ARE 64 bit ON x64/ia64 *)
    (*  REVIEW: If we ever decide that we need these values for x64, we'll have to read them in as 64bit and fix up the rest of the offsets.
        Then again, it should suffice to just use the defaults, and still not bother... *)
    (*  let stack_reserve = SeekReadInt32 is (pe_optional_header_phys_loc + 72) in *)  (* Stack Reserve Size Always 0x100000 (1Mb) (see Section 23.1). *)
    (*   let stack_commit = SeekReadInt32 is (pe_optional_header_phys_loc + 76) in  *) (* Stack Commit Size Always 0x1000 (4Kb) (see Section 23.1). *)
    (*   let heap_reserve = SeekReadInt32 is (pe_optional_header_phys_loc + 80) in *)  (* Heap Reserve Size Always 0x100000 (1Mb) (see Section 23.1). *)
    (*   let heap_commit = SeekReadInt32 is (pe_optional_header_phys_loc + 84) in *)  (* Heap Commit Size Always 0x1000 (4Kb) (see Section 23.1). *)

     (* x86: 000000f0, x64: 00000100 *) 
    let num_data_directories = SeekReadInt32 is (pe_optional_header_phys_loc + 92 + x64adjust)   (* Number of Data Directories: Always 0x10 (see Section 23.1). *)
     (* 00000100 - these addresses are for x86 - for the x64 location, add x64adjust (0x10) *) 
    let import_tab_addr = SeekReadInt32 is (pe_optional_header_phys_loc + 104 + x64adjust)   (* Import Table RVA of Import Table, (see clause 24.3.1). e.g. 0000b530 *) 
    let import_tab_size = SeekReadInt32 is (pe_optional_header_phys_loc + 108 + x64adjust)  (* Size of Import Table, (see clause 24.3.1).  *)
    let native_resources_addr = SeekReadInt32 is (pe_optional_header_phys_loc + 112 + x64adjust)
    let native_resources_size = SeekReadInt32 is (pe_optional_header_phys_loc + 116 + x64adjust)
     (* 00000110 *) 
     (* 00000120 *) 
  (*   let base_reloc_tab_addr = SeekReadInt32 is (pe_optional_header_phys_loc + 136)
    let base_reloc_tab_size = SeekReadInt32 is (pe_optional_header_phys_loc + 140) in  *)
     (* 00000130 *) 
     (* 00000140 *) 
     (* 00000150 *) 
    let import_addr_tab_addr = SeekReadInt32 is (pe_optional_header_phys_loc + 192 + x64adjust)   (* RVA of Import Addr Table, (see clause 24.3.1). e.g. 0x00002000 *) 
    let import_addr_tab_size = SeekReadInt32 is (pe_optional_header_phys_loc + 196 + x64adjust)  (* Size of Import Addr Table, (see clause 24.3.1). e.g. 0x00002000 *) 
     (* 00000160 *) 
    let cli_header_addr = SeekReadInt32 is (pe_optional_header_phys_loc + 208 + x64adjust)
    let cli_header_size = SeekReadInt32 is (pe_optional_header_phys_loc + 212 + x64adjust)
     (* 00000170 *) 


    (* Crack section headers *)

    let section_headers = 
      [ for i in 0 .. num_sections-1 do
          let pos = section_headers_start_phys_loc + i * 0x28
          let virt_size = SeekReadInt32 is (pos + 8)
          let virt_addr = SeekReadInt32 is (pos + 12)
          let phys_loc = SeekReadInt32 is (pos + 20)
          yield (virt_addr,virt_size,phys_loc) ]

    let find_section_header addr = 
      let rec look i pos = 
        if i >= num_sections then 0x0 
        else
          let virt_size = SeekReadInt32 is (pos + 8)
          let virt_addr = SeekReadInt32 is (pos + 12)
          if (addr >= virt_addr && addr < virt_addr + virt_size) then pos 
          else look (i+1) (pos + 0x28)
      look 0 section_headers_start_phys_loc
    
    let text_header_start = find_section_header cli_header_addr
    let data_header_start = find_section_header data_addr
  (*  let reloc_header_start = find_section_header base_reloc_tab_addr in  *)

    let text_size = if text_header_start = 0x0 then 0x0 else SeekReadInt32 is (text_header_start + 8)
    let text_addr = if text_header_start = 0x0 then 0x0 else SeekReadInt32 is (text_header_start + 12)
    let text_phys_size = if text_header_start = 0x0 then 0x0 else SeekReadInt32 is (text_header_start + 16)
    let text_phys_loc = if text_header_start = 0x0 then 0x0 else SeekReadInt32 is (text_header_start + 20)

  (*
    let reloc_size = if reloc_header_start = 0x0 then 0x0 else SeekReadInt32 is (reloc_header_start + 8)
    let reloc_addr = if reloc_header_start = 0x0 then 0x0 else SeekReadInt32 is (reloc_header_start + 12)
    let reloc_phys_size = if reloc_header_start = 0x0 then 0x0 else SeekReadInt32 is (reloc_header_start + 16)
    let reloc_phys_loc = if reloc_header_start = 0x0 then 0x0 else SeekReadInt32 is (reloc_header_start + 20)
  *)

    if logging then dprintn (infile ^ ": text_header_start = "^string text_header_start);
    if logging then dprintn (infile ^ ": data_header_start = "^string data_header_start);
    if logging then  dprintn (infile ^ ": data_addr (pre section crack) = "^string data_addr);

    let data_size = if data_header_start = 0x0 then 0x0 else SeekReadInt32 is (data_header_start + 8)
    let data_addr = if data_header_start = 0x0 then 0x0 else SeekReadInt32 is (data_header_start + 12)
    let data_phys_size = if data_header_start = 0x0 then 0x0 else SeekReadInt32 is (data_header_start + 16)
    let data_phys_loc = if data_header_start = 0x0 then 0x0 else SeekReadInt32 is (data_header_start + 20)

    if logging then dprintn (infile ^ ": data_addr (post section crack) = "^string data_addr);

    let anyV2P (n,v) = 
      let rec look i pos = 
        if i >= num_sections then (failwith (infile ^ ": bad "^n^", rva "^string v); 0x0)
        else
          let virt_size = SeekReadInt32 is (pos + 8)
          let virt_addr = SeekReadInt32 is (pos + 12)
          let phys_loc = SeekReadInt32 is (pos + 20)
          if (v >= virt_addr && (v < virt_addr + virt_size)) then (v - virt_addr) + phys_loc 
          else look (i+1) (pos + 0x28)
      look 0 section_headers_start_phys_loc

  (*  let relocV2P v = v - reloc_addr + reloc_phys_loc in  *)

    if logging then dprintn (infile ^ ": num_sections = "^string num_sections); 
    if logging then dprintn (infile ^ ": cli_header_addr = "^string cli_header_addr); 
    if logging then dprintn (infile ^ ": cli_header_phys = "^string (anyV2P ("cli header",cli_header_addr))); 
    if logging then dprintn (infile ^ ": data_size = "^string data_size); 
    if logging then dprintn (infile ^ ": data_addr = "^string data_addr); 

    let cli_header_phys_loc = anyV2P ("cli header",cli_header_addr)

    let major_runtime_version = SeekReadUInt16 is (cli_header_phys_loc + 4)
    let minor_runtime_version = SeekReadUInt16 is (cli_header_phys_loc + 6)
    let metadata_addr         = SeekReadInt32 is (cli_header_phys_loc + 8)
    let metadata_size         = SeekReadInt32 is (cli_header_phys_loc + 12)
    let cli_flags             = SeekReadInt32 is (cli_header_phys_loc + 16)
    
    let ilonly             = (cli_flags &&& 0x01) <> 0x00
    let only32             = (cli_flags &&& 0x02) <> 0x00
    let strongname_signed  = (cli_flags &&& 0x08) <> 0x00
    let trackdebugdata     = (cli_flags &&& 0x010000) <> 0x00
    
    let eptoken = SeekReaduncoded_token is (cli_header_phys_loc + 20)
    let resources_addr     = SeekReadInt32 is (cli_header_phys_loc + 24)
    let resources_size     = SeekReadInt32 is (cli_header_phys_loc + 28)
    let strongname_addr    = SeekReadInt32 is (cli_header_phys_loc + 32)
    let strongname_size    = SeekReadInt32 is (cli_header_phys_loc + 36)
    let vtable_fixups_addr = SeekReadInt32 is (cli_header_phys_loc + 40)
    let vtable_fixups_size = SeekReadInt32 is (cli_header_phys_loc + 44)

    if logging then dprintn (infile ^ ": metadata_addr = "^string metadata_addr); 
    if logging then dprintn (infile ^ ": resources_addr = "^string resources_addr); 
    if logging then dprintn (infile ^ ": resources_size = "^string resources_size); 
    if logging then dprintn (infile ^ ": native_resources_addr = "^string native_resources_addr); 
    if logging then dprintn (infile ^ ": native_resources_size = "^string native_resources_size); 

    let metadata_phys_loc = anyV2P ("metadata",metadata_addr)
    let magic = SeekReadUInt16AsInt32 is metadata_phys_loc
    if magic <> 0x5342 then failwith (infile ^ ": bad metadata magic number: " ^ string magic);
    let magic2 = SeekReadUInt16AsInt32 is (metadata_phys_loc + 2)
    if magic2 <> 0x424a then failwith "bad metadata magic number";
    let major_metadata_version = SeekReadUInt16 is (metadata_phys_loc + 4)
    let minor_metadata_version = SeekReadUInt16 is (metadata_phys_loc + 6)

    let version_length = SeekReadInt32 is (metadata_phys_loc + 12)
    let x = align 0x04 (16 + version_length)
    let num_streams = SeekReadUInt16AsInt32 is (metadata_phys_loc + x + 2)
    let stream_headers_start = (metadata_phys_loc + x + 4)

    if logging then dprintn (infile ^ ": num_streams = "^string num_streams); 
    if logging then dprintn (infile ^ ": stream_headers_start = "^string stream_headers_start); 

  (* Crack stream headers *)

    let try_find_stream name = 
      let rec look i pos = 
        if i >= num_streams then raise Not_found
        else
          let offset = SeekReadInt32 is (pos + 0)
          let length = SeekReadInt32 is (pos + 4)
          let res = ref true
          let fin = ref false
          let n = ref 0
          // read and compare the stream name byte by byte 
          while (not !fin) do 
              let c= SeekReadByteAsInt32 is (pos + 8 + (!n))
              if c = 0 then 
                  fin := true
              elif !n >= Array.length name or c <> name.[!n] then 
                  res := false;
              incr n
          if !res then (offset + metadata_phys_loc,length) 
          else look (i+1) (align 0x04 (pos + 8 + (!n)))
      look 0 stream_headers_start
    let find_stream name = try try_find_stream name with Not_found -> (0x0, 0x0)

    let (tables_stream_phys_loc, tables_stream_size) = 
      try try_find_stream [| 0x23; 0x7e |] (* #~ *) 
      with Not_found -> 
        try try_find_stream [| 0x23; 0x2d |] (* #-: at least one DLL I've seen uses this! *)  
        with Not_found -> 
         dprintf "no metadata tables found under stream names '#~' or '#-', please report this\n";
         let first_stream_offset = SeekReadInt32 is (stream_headers_start + 0)
         let first_stream_length = SeekReadInt32 is (stream_headers_start + 4)
         first_stream_offset,first_stream_length
    let (strings_stream_phys_loc, strings_stream_size) = find_stream [| 0x23; 0x53; 0x74; 0x72; 0x69; 0x6e; 0x67; 0x73; |] (* #Strings *)
    let (user_strings_stream_phys_loc, user_strings_stream_size) = find_stream [| 0x23; 0x55; 0x53; |] (* #US *)
    let (guids_stream_phys_loc, guids_stream_size) = find_stream [| 0x23; 0x47; 0x55; 0x49; 0x44; |] (* #GUID *)
    let (blobs_stream_phys_loc, blobs_stream_size) = find_stream [| 0x23; 0x42; 0x6c; 0x6f; 0x62; |] (* #Blob *)

    if logging then dprintn (infile ^ ": tables_addr = "^string tables_stream_phys_loc); 
    if logging then dprintn (infile ^ ": tables_size = "^string tables_stream_size); 
    if logging then dprintn (infile ^ ": strings_addr = "^string strings_stream_phys_loc);
    if logging then dprintn (infile ^ ": strings_size = "^string strings_stream_size); 
    if logging then dprintn (infile ^ ": user_strings_addr = "^string user_strings_stream_phys_loc); 
    if logging then dprintn (infile ^ ": guids_addr = "^string guids_stream_phys_loc); 
    if logging then dprintn (infile ^ ": blobs_addr = "^string blobs_stream_phys_loc); 

    let tables_stream_major_version = SeekReadByteAsInt32 is (tables_stream_phys_loc + 4)
    let tables_stream_minor_version = SeekReadByteAsInt32 is (tables_stream_phys_loc + 5)

    let usingWhidbeyBeta1TableSchemeForGenericParam = (tables_stream_major_version = 1) && (tables_stream_minor_version = 1)

    let table_kinds = 
        [|kind_Module               (* Table 0  *); 
          kind_TypeRef              (* Table 1  *);
          kind_TypeDef              (* Table 2  *);
          kind_Illegal (* kind_FieldPtr *)             (* Table 3  *);
          kind_FieldDef                (* Table 4  *);
          kind_Illegal (* kind_MethodPtr *)            (* Table 5  *);
          kind_MethodDef               (* Table 6  *);
          kind_Illegal (* kind_ParamPtr *)             (* Table 7  *);
          kind_Param                (* Table 8  *);
          kind_InterfaceImpl        (* Table 9  *);
          kind_MemberRef            (* Table 10 *);
          kind_Constant             (* Table 11 *);
          kind_CustomAttribute      (* Table 12 *);
          kind_FieldMarshal         (* Table 13 *);
          kind_DeclSecurity         (* Table 14 *);
          kind_ClassLayout          (* Table 15 *);
          kind_FieldLayout          (* Table 16 *);
          kind_StandAloneSig        (* Table 17 *);
          kind_EventMap             (* Table 18 *);
          kind_Illegal (* kind_EventPtr *)             (* Table 19 *);
          kind_Event                (* Table 20 *);
          kind_PropertyMap          (* Table 21 *);
          kind_Illegal (* kind_PropertyPtr *)          (* Table 22 *);
          kind_Property             (* Table 23 *);
          kind_MethodSemantics      (* Table 24 *);
          kind_MethodImpl           (* Table 25 *);
          kind_ModuleRef            (* Table 26 *);
          kind_TypeSpec             (* Table 27 *);
          kind_ImplMap              (* Table 28 *);
          kind_FieldRVA             (* Table 29 *);
          kind_Illegal (* kind_ENCLog *)               (* Table 30 *);
          kind_Illegal (* kind_ENCMap *)               (* Table 31 *);
          kind_Assembly             (* Table 32 *);
          kind_Illegal (* kind_AssemblyProcessor *)    (* Table 33 *);
          kind_Illegal (* kind_AssemblyOS *)           (* Table 34 *);
          kind_AssemblyRef          (* Table 35 *);
          kind_Illegal (* kind_AssemblyRefProcessor *) (* Table 36 *);
          kind_Illegal (* kind_AssemblyRefOS *)        (* Table 37 *);
          kind_FileRef                 (* Table 38 *);
          kind_ExportedType         (* Table 39 *);
          kind_ManifestResource     (* Table 40 *);
          kind_Nested               (* Table 41 *);
         (if usingWhidbeyBeta1TableSchemeForGenericParam then kind_GenericParam_v1_1 else  kind_GenericParam_v2_0);        (* Table 42 *)
          kind_MethodSpec         (* Table 43 *);
          kind_GenericParamConstraint         (* Table 44 *);
          kind_Illegal         (* Table 45 *);
          kind_Illegal         (* Table 46 *);
          kind_Illegal         (* Table 47 *);
          kind_Illegal         (* Table 48 *);
          kind_Illegal         (* Table 49 *);
          kind_Illegal         (* Table 50 *);
          kind_Illegal         (* Table 51 *);
          kind_Illegal         (* Table 52 *);
          kind_Illegal         (* Table 53 *);
          kind_Illegal         (* Table 54 *);
          kind_Illegal         (* Table 55 *);
          kind_Illegal         (* Table 56 *);
          kind_Illegal         (* Table 57 *);
          kind_Illegal         (* Table 58 *);
          kind_Illegal         (* Table 59 *);
          kind_Illegal         (* Table 60 *);
          kind_Illegal         (* Table 61 *);
          kind_Illegal         (* Table 62 *);
          kind_Illegal         (* Table 63 *);
        |]

    let heap_sizes = SeekReadByteAsInt32 is (tables_stream_phys_loc + 6)
    let valid = SeekReadInt64 is (tables_stream_phys_loc + 8)
    let sorted = SeekReadInt64 is (tables_stream_phys_loc + 16)
    let tables_present, table_num_rows, start_of_tables = 
        let present = ref []
        let num_rows = Array.create 64 0
        let prev_numrow_idx = ref (tables_stream_phys_loc + 24)
        for i = 0 to 63 do 
            if (valid &&& (int64 1 <<< i)) <> int64  0 then 
                present := i :: !present;
                num_rows.[i] <-  (SeekReadInt32 is !prev_numrow_idx);
                prev_numrow_idx := !prev_numrow_idx + 4
        List.rev !present, num_rows, !prev_numrow_idx

    let nrows t = table_num_rows.[tag_of_table t]
    let num_tables = List.length tables_present
    let strings_big = (heap_sizes &&& 1) <> 0
    let guids_big = (heap_sizes &&& 2) <> 0
    let blobs_big = (heap_sizes &&& 4) <> 0

    if logging then dprintn (infile ^ ": num_tables = "^string num_tables);
    if logging && strings_big then dprintn (infile ^ ": strings are big");
    if logging && blobs_big then dprintn (infile ^ ": blobs are big");

    let table_bignesses = Array.map (fun n -> n >= 0x10000) table_num_rows
      
    let coded_bigness nbits tab =
      let rows = nrows tab
      rows >= (0x10000 lsr nbits)
    
    let tdor_bigness = 
      coded_bigness 2 tab_TypeDef || 
      coded_bigness 2 tab_TypeRef || 
      coded_bigness 2 tab_TypeSpec
    
    let tomd_bigness = 
      coded_bigness 1 tab_TypeDef || 
      coded_bigness 1 tab_Method
    
    let hc_bigness = 
      coded_bigness 2 tab_Field ||
      coded_bigness 2 tab_Param ||
      coded_bigness 2 tab_Property
    
    let hca_bigness = 
      coded_bigness 5 tab_Method ||
      coded_bigness 5 tab_Field ||
      coded_bigness 5 tab_TypeRef  ||
      coded_bigness 5 tab_TypeDef ||
      coded_bigness 5 tab_Param ||
      coded_bigness 5 tab_InterfaceImpl ||
      coded_bigness 5 tab_MemberRef ||
      coded_bigness 5 tab_Module ||
      coded_bigness 5 tab_Permission ||
      coded_bigness 5 tab_Property ||
      coded_bigness 5 tab_Event ||
      coded_bigness 5 tab_StandAloneSig ||
      coded_bigness 5 tab_ModuleRef ||
      coded_bigness 5 tab_TypeSpec ||
      coded_bigness 5 tab_Assembly ||
      coded_bigness 5 tab_AssemblyRef ||
      coded_bigness 5 tab_File ||
      coded_bigness 5 tab_ExportedType ||
      coded_bigness 5 tab_ManifestResource ||
      coded_bigness 5 tab_GenericParam ||
      coded_bigness 5 tab_GenericParamConstraint ||
      coded_bigness 5 tab_MethodSpec

    
    let hfm_bigness = 
      coded_bigness 1 tab_Field || 
      coded_bigness 1 tab_Param
    
    let hds_bigness = 
      coded_bigness 2 tab_TypeDef || 
      coded_bigness 2 tab_Method ||
      coded_bigness 2 tab_Assembly
    
    let mrp_bigness = 
      coded_bigness 3 tab_TypeRef ||
      coded_bigness 3 tab_ModuleRef ||
      coded_bigness 3 tab_Method ||
      coded_bigness 3 tab_TypeSpec
    
    let hs_bigness = 
      coded_bigness 1 tab_Event || 
      coded_bigness 1 tab_Property 
    
    let mdor_bigness =
      coded_bigness 1 tab_Method ||    
      coded_bigness 1 tab_MemberRef 
    
    let mf_bigness =
      coded_bigness 1 tab_Field ||
      coded_bigness 1 tab_Method 
    
    let i_bigness =
      coded_bigness 2 tab_File || 
      coded_bigness 2 tab_AssemblyRef ||    
      coded_bigness 2 tab_ExportedType 
    
    let cat_bigness =  
      coded_bigness 3 tab_Method ||    
      coded_bigness 3 tab_MemberRef 
    
    let rs_bigness = 
      coded_bigness 2 tab_Module ||    
      coded_bigness 2 tab_ModuleRef || 
      coded_bigness 2 tab_AssemblyRef  ||
      coded_bigness 2 tab_TypeRef
      
    let row_kind_size (RowKind kinds) = 
      List.fold 
        (fun sofar x -> 
          sofar +
            match x with 
            | UShort -> 2
            | ULong -> 4
            | Byte -> 1
            | Data -> 4
            | GGuid -> (if guids_big then 4 else 2)
            | Blob  -> (if blobs_big then 4 else 2)
            | SString  -> (if strings_big then 4 else 2)
            | SimpleIndex (Table tab) -> (if table_bignesses.[tab] then 4 else 2)
            | TypeDefOrRefOrSpec -> (if tdor_bigness then 4 else 2)
            | TypeOrMethodDef -> (if tomd_bigness then 4 else 2)
            | HasConstant  -> (if hc_bigness then 4 else 2)
            | HasCustomAttribute -> (if hca_bigness then 4 else 2)
            | HasFieldMarshal  -> (if hfm_bigness then 4 else 2)
            | HasDeclSecurity  -> (if hds_bigness then 4 else 2)
            | MemberRefParent  -> (if mrp_bigness then 4 else 2)
            | HasSemantics  -> (if hs_bigness then 4 else 2)
            | MethodDefOrRef -> (if mdor_bigness then 4 else 2)
            | MemberForwarded -> (if mf_bigness then 4 else 2)
            | Implementation  -> (if i_bigness then 4 else 2)
            | CustomAttributeType -> (if cat_bigness then 4 else 2)
            | ResolutionScope -> (if rs_bigness then 4 else 2)) 0 kinds

    let table_row_sizes = 
         let res = Array.create 64 0x0
         for i = 0 to 63 do 
             res.[i] <- (row_kind_size (table_kinds.[i]));
             (* dprintf "table_row_sizes.[%d] = %ld\n" i res.[i]; *)
         res

    let table_phys_locs = 
         let res = Array.create 64 0x0
         let prev_table_phys_loc = ref start_of_tables
         for i = 0 to 63 do 
             res.[i] <- !prev_table_phys_loc;
             prev_table_phys_loc := !prev_table_phys_loc + ((table_num_rows.[i]) * table_row_sizes.[i]);
             if logging then dprintf "table_phys_locs.[%d] = %ld, offset from start_of_tables = 0x%08lx\n" i res.[i] (res.[i] -  start_of_tables);
         res
    
    let inbase = System.IO.Path.GetFileName infile^": "

    // All the caches.  The sizes are guesstimates for the rough sharing-density of the assembly 
    // We should also take a parameter that indicates how much of the assembly we actually 
    // expect to get read at all 
    let cache_AssemblyRef               = mk_cache_int32 opts.optimizeForMemory inbase "ILAssemblyRef"  (nrows (tab_AssemblyRef))
    let cache_MethodSpec_as_mdata       = mk_cache_gen opts.optimizeForMemory inbase "MethodSpec_as_mdata" (nrows (tab_MethodSpec) / 20 + 1)
    let cache_MemberRef_as_mdata        = mk_cache_gen opts.optimizeForMemory inbase "MemberRef_as_mdata" (nrows (tab_MemberRef) / 20 + 1)
     //let cache_MemberRef_as_fspec        = mk_cache_gen opts.optimizeForMemory inbase "MemberRef_as_fspec" (nrows (tab_MemberRef) / 40 + 1)
    let cache_CustomAttr                = mk_cache_gen opts.optimizeForMemory inbase "CustomAttr" (nrows (tab_CustomAttribute) / 50 + 1)
    //let cache_SecurityDecl              = mk_cache_gen opts.optimizeForMemory inbase "SecurityDecl" (nrows (tab_Permission) / 20 + 1)
    let cache_TypeRef                   = mk_cache_int32 opts.optimizeForMemory inbase "ILTypeRef" (nrows (tab_TypeRef) / 20 + 1)
    let cache_TypeRef_as_typ            = mk_cache_gen opts.optimizeForMemory inbase "TypeRef_as_typ" (nrows (tab_TypeRef) / 20 + 1)
    let cache_blob_heap_as_property_sig = mk_cache_gen opts.optimizeForMemory inbase "blob_heap_as_property_sig" (nrows (tab_Property) / 20 + 1)
    let cache_blob_heap_as_field_sig    = mk_cache_gen opts.optimizeForMemory inbase "blob_heap_as_field_sig" (nrows (tab_Field) / 20 + 1)
    let cache_blob_heap_as_method_sig   = mk_cache_gen opts.optimizeForMemory inbase "blob_heap_as_method_sig" (nrows (tab_Method) / 20 + 1)
    //let cache_blob_heap_as_locals_sig   = mk_cache_gen opts.optimizeForMemory inbase "blob_heap_as_locals_sig" (nrows (tab_Method) / 20 + 1)
    let cache_TypeDef_as_typ            = mk_cache_gen opts.optimizeForMemory inbase "TypeDef_as_typ" (nrows (tab_TypeDef) / 20 + 1)
    let cache_MethodDef_as_mdata        = mk_cache_int32 opts.optimizeForMemory inbase "MethodDef_as_mdata" (nrows (tab_Method) / 20 + 1)
    let cache_GenericParams             = mk_cache_gen opts.optimizeForMemory inbase "GenericParams" (nrows (tab_GenericParam) / 20 + 1)
    let cache_GenericParamConstraints   = mk_cache_gen opts.optimizeForMemory inbase "GenericParamConstraints" (nrows (tab_GenericParamConstraint) / 8 + 1)
    let cache_FieldDef_as_fspec         = mk_cache_int32 opts.optimizeForMemory inbase "FieldDef_as_fspec" (nrows (tab_Field) / 20 + 1)
    let cache_user_string_heap          = mk_cache_int32 opts.optimizeForMemory inbase "user_string heap" ( user_strings_stream_size / 20 + 1)
    (* nb. Lots and lots of cache hits on this cache, hence never optimize cache away *)
    let cache_string_heap               = mk_cache_int32 false inbase "string heap" ( strings_stream_size / 50 + 1)
    let cache_blob_heap                 = mk_cache_int32 opts.optimizeForMemory inbase "blob heap" ( blobs_stream_size / 50 + 1) 

     // These tables are not required to enforce sharing fo the final data 
     // structure, but are very useful as searching these tables gives rise to many reads 
     // in standard applications.  
     
    let cache_Nested_row          = mk_cache_int32 opts.optimizeForMemory inbase "Nested Table Rows" (nrows (tab_Nested) / 20 + 1)
    let cache_Constant_row        = mk_cache_int32 opts.optimizeForMemory inbase "Constant Rows" (nrows (tab_Constant) / 20 + 1)
    let cache_MethodSemantics_row = mk_cache_int32 opts.optimizeForMemory inbase "MethodSemantics Rows" (nrows (tab_MethodSemantics) / 20 + 1)
    let cache_TypeDef_row         = mk_cache_int32 opts.optimizeForMemory inbase "ILTypeDef Rows" (nrows (tab_TypeDef) / 20 + 1)
    let cache_InterfaceImpl_row   = mk_cache_int32 opts.optimizeForMemory inbase "InterfaceImpl Rows" (nrows (tab_InterfaceImpl) / 20 + 1)
    let cache_FieldMarshal_row    = mk_cache_int32 opts.optimizeForMemory inbase "FieldMarshal Rows" (nrows (tab_FieldMarshal) / 20 + 1)
    let cache_PropertyMap_row     = mk_cache_int32 opts.optimizeForMemory inbase "PropertyMap Rows" (nrows (tab_PropertyMap) / 20 + 1)

    let mk_row_counter nm  =
       let count = ref 0
       add_report (fun oc -> if !count <> 0 then output_string oc (inbase^string !count ^ " "^nm^" rows read"^"\n"));
       count

    let count_TypeRef                = mk_row_counter "ILTypeRef"
    let count_TypeDef                = mk_row_counter "ILTypeDef"
    let count_Field                  = mk_row_counter "Field"
    let count_Method                 = mk_row_counter "Method"
    let count_Param                  = mk_row_counter "Param"
    let count_InterfaceImpl          = mk_row_counter "InterfaceImpl"
    let count_MemberRef              = mk_row_counter "MemberRef"
    let count_Constant               = mk_row_counter "Constant"
    let count_CustomAttribute        = mk_row_counter "CustomAttribute"
    let count_FieldMarshal           = mk_row_counter "FieldMarshal"
    let count_Permission             = mk_row_counter "Permission"
    let count_ClassLayout            = mk_row_counter "ClassLayout"
    let count_FieldLayout            = mk_row_counter "FieldLayout"
    let count_StandAloneSig          = mk_row_counter "StandAloneSig"
    let count_EventMap               = mk_row_counter "EventMap"
    let count_Event                  = mk_row_counter "Event"
    let count_PropertyMap            = mk_row_counter "PropertyMap"
    let count_Property               = mk_row_counter "Property"
    let count_MethodSemantics        = mk_row_counter "MethodSemantics"
    let count_MethodImpl             = mk_row_counter "MethodImpl"
    let count_ModuleRef              = mk_row_counter "ILModuleRef"
    let count_TypeSpec               = mk_row_counter "ILTypeSpec"
    let count_ImplMap                = mk_row_counter "ImplMap"
    let count_FieldRVA               = mk_row_counter "FieldRVA"
    let count_Assembly               = mk_row_counter "Assembly"
    let count_AssemblyRef            = mk_row_counter "ILAssemblyRef"
    let count_File                   = mk_row_counter "File"
    let count_ExportedType           = mk_row_counter "ILExportedType"
    let count_ManifestResource       = mk_row_counter "ManifestResource"
    let count_Nested                 = mk_row_counter "Nested"
    let count_GenericParam           = mk_row_counter "GenericParam"
    let count_GenericParamConstraint = mk_row_counter "GenericParamConstraint"
    let count_MethodSpec             = mk_row_counter "ILMethodSpec"


   (*-----------------------------------------------------------------------
    * Set up the PDB reader so we can read debug info for methods.
    * ----------------------------------------------------------------------*)

    let pdb = if runningOnMono then None else getPDBReader opts infile

    let row_addr tab idx = table_phys_locs.[tag_of_table tab] + (idx - 1) * table_row_sizes.[tag_of_table tab]


    // Build the reader context
    // Use an initialization hole 
    let ctxtH = ref None
    let ctxt = { ilg=opts.ilGlobals; 
                 data_end_points = data_end_points ctxtH;
                 pdb=pdb;
                 sorted=sorted;
                 nrows=nrows; 
                 text_phys_loc=text_phys_loc; 
                 text_phys_size=text_phys_size;
                 data_phys_loc=data_phys_loc;
                 data_phys_size=data_phys_size;
                 anyV2P=anyV2P;
                 metadata_addr=metadata_addr;
                 section_headers=section_headers;
                 native_resources_addr=native_resources_addr;
                 native_resources_size=native_resources_size;
                 resources_addr=resources_addr;
                 strongname_addr=strongname_addr;
                 vtable_fixups_addr=vtable_fixups_addr;
                 is=is;
                 infile=infile;
                 user_strings_stream_phys_loc=user_strings_stream_phys_loc;
                 strings_stream_phys_loc=strings_stream_phys_loc;
                 blobs_stream_phys_loc=blobs_stream_phys_loc;
                 MemoizeString = memoize id;
                 ReadUserStringHeap = cache_user_string_heap (ReadUserStringHeapUncached ctxtH);
                 ReadStringHeap = cache_string_heap (ReadStringHeapUncached ctxtH);
                 ReadBlobHeap = cache_blob_heap (ReadBlobHeapUncached ctxtH);
                 SeekReadNestedRow  = cache_Nested_row  (SeekReadNestedRowUncached ctxtH);
                 SeekReadConstantRow  = cache_Constant_row  (SeekReadConstantRowUncached ctxtH);
                 SeekReadMethodSemanticsRow  = cache_MethodSemantics_row  (SeekReadMethodSemanticsRowUncached ctxtH);
                 SeekReadTypeDefRow  = cache_TypeDef_row  (SeekReadTypeDefRowUncached ctxtH);
                 SeekReadInterfaceImplRow  = cache_InterfaceImpl_row  (SeekReadInterfaceImplRowUncached ctxtH);
                 SeekReadFieldMarshalRow  = cache_FieldMarshal_row  (SeekReadFieldMarshalRowUncached ctxtH);
                 SeekReadPropertyMapRow = cache_PropertyMap_row  (SeekReadPropertyMapRowUncached ctxtH);
                 SeekReadAssemblyRef = cache_AssemblyRef  (SeekReadAssemblyRefUncached ctxtH);
                 SeekReadMethodSpec_as_mdata = cache_MethodSpec_as_mdata  (SeekReadMethodSpec_as_mdataUncached ctxtH);
                 SeekReadMemberRef_as_mdata = cache_MemberRef_as_mdata  (SeekReadMemberRef_as_mdataUncached ctxtH);
                 SeekReadMemberRef_as_fspec = (* cache_MemberRef_as_fspec  *) (SeekReadMemberRef_as_fspecUncached ctxtH);
                 SeekReadCustomAttr = cache_CustomAttr  (SeekReadCustomAttrUncached ctxtH);
                 SeekReadSecurityDecl = (* cache_SecurityDecl  *) (SeekReadSecurityDeclUncached ctxtH);
                 SeekReadTypeRef = cache_TypeRef (SeekReadTypeRefUncached ctxtH);
                 ReadBlobHeap_as_property_sig = cache_blob_heap_as_property_sig (ReadBlobHeap_as_property_sigUncached ctxtH);
                 ReadBlobHeap_as_field_sig = cache_blob_heap_as_field_sig (ReadBlobHeap_as_field_sigUncached ctxtH);
                 ReadBlobHeap_as_method_sig = cache_blob_heap_as_method_sig (ReadBlobHeap_as_method_sigUncached ctxtH);
                 ReadBlobHeap_as_locals_sig = (* cache_blob_heap_as_locals_sig *) (ReadBlobHeap_as_locals_sigUncached ctxtH);
                 SeekReadTypeDefAsType = cache_TypeDef_as_typ (SeekReadTypeDefAsTypeUncached ctxtH);
                 SeekReadTypeRef_as_typ = cache_TypeRef_as_typ (SeekReadTypeRef_as_typUncached ctxtH);
                 SeekReadMethodDef_as_mdata = cache_MethodDef_as_mdata (SeekReadMethodDef_as_mdataUncached ctxtH);
                 SeekReadGenericParams = cache_GenericParams (SeekReadGenericParamsUncached ctxtH);
                 SeekReadFieldDef_as_fspec = cache_FieldDef_as_fspec (SeekReadFieldDef_as_fspecUncached ctxtH);
                 guids_stream_phys_loc = guids_stream_phys_loc;
                 row_addr=row_addr;
                 eptoken=eptoken; 
                 rs_bigness =rs_bigness;
                 tdor_bigness =tdor_bigness;
                 tomd_bigness =tomd_bigness;   
                 hc_bigness =hc_bigness;   
                 hca_bigness =hca_bigness;   
                 hfm_bigness =hfm_bigness;   
                 hds_bigness =hds_bigness;
                 mrp_bigness =mrp_bigness;
                 hs_bigness =hs_bigness;
                 mdor_bigness =mdor_bigness;
                 mf_bigness =mf_bigness;
                 i_bigness =i_bigness;
                 cat_bigness =cat_bigness; 
                 strings_big=strings_big;
                 guids_big=guids_big;
                 blobs_big=blobs_big;
                 table_bignesses=table_bignesses;
                 count_TypeRef = count_TypeRef;             
                 count_TypeDef = count_TypeDef;             
                 count_Field = count_Field;               
                 count_Method = count_Method;              
                 count_Param = count_Param;               
                 count_InterfaceImpl = count_InterfaceImpl;       
                 count_MemberRef = count_MemberRef;           
                 count_Constant = count_Constant;            
                 count_CustomAttribute = count_CustomAttribute;     
                 count_FieldMarshal = count_FieldMarshal;        
                 count_Permission = count_Permission;         
                 count_ClassLayout = count_ClassLayout;        
                 count_FieldLayout = count_FieldLayout;         
                 count_StandAloneSig = count_StandAloneSig;       
                 count_EventMap = count_EventMap;            
                 count_Event = count_Event;               
                 count_PropertyMap = count_PropertyMap;         
                 count_Property = count_Property;            
                 count_MethodSemantics = count_MethodSemantics;     
                 count_MethodImpl = count_MethodImpl;          
                 count_ModuleRef = count_ModuleRef;           
                 count_TypeSpec = count_TypeSpec;            
                 count_ImplMap = count_ImplMap;             
                 count_FieldRVA = count_FieldRVA;            
                 count_Assembly = count_Assembly;            
                 count_AssemblyRef = count_AssemblyRef;         
                 count_File = count_File;                
                 count_ExportedType = count_ExportedType;        
                 count_ManifestResource = count_ManifestResource;    
                 count_Nested = count_Nested;              
                 count_GenericParam = count_GenericParam;              
                 count_GenericParamConstraint = count_GenericParamConstraint;              
                 count_MethodSpec = count_MethodSpec;  } 
    ctxtH := Some ctxt;
     
    let ilModule = SeekReadModule ctxt (subsys,ilonly,only32,only64,platform,is_dll, align_virt,align_phys,image_base_real) 1
    let ilAssemblyRefs = lazy [ for i in 1 .. nrows (tab_AssemblyRef) do yield SeekReadAssemblyRef ctxt i ]
    
    ilModule,ilAssemblyRefs,pdb
  
let CloseILModuleReader x = x.dispose()

let defaults = 
  { optimizeForMemory=false; 
    pdbPath= None; 
    ilGlobals=ecmaILGlobals } 


let OpenILModuleReader infile opts = 

 try 
      let mmap = MMapChannel.open_in infile
      let modul,ilAssemblyRefs,pdb = gen_open_binary_reader infile (MMap mmap) opts
      { modul = modul; 
        ilAssemblyRefs=ilAssemblyRefs;
        dispose = (fun () -> 
          MMapChannel.close mmap;
          match pdb with 
          | Some (pdbr,_) -> pdbReadClose pdbr
          | None -> ()) }
  with :? System.DllNotFoundException ->
      let is = open_in_bin infile
      let cell = ref (Some is)
      let modul,ilAssemblyRefs,pdb = gen_open_binary_reader infile (Chan (infile,cell)) opts
      { modul = modul; 
        ilAssemblyRefs = ilAssemblyRefs;
        dispose = (fun () -> 
          cell := None;
          close_in is;
          match pdb with 
          | Some (pdbr,_) -> pdbReadClose pdbr
          | None -> ()) }

let ilModuleReaderCache = new Internal.Utilities.Collections.AgedLookup<(string * System.DateTime),ILModuleReader>(0)
let OpenILModuleReaderAfterReadingAllBytes infile opts = 
    // Use GetDirectoryName and GetFileName to pseudo-normalize the paths.
    let key,succeeded = 
        try (Path.GetFullPath(infile), File.GetLastWriteTime(infile)), true
        with e -> 
            System.Diagnostics.Debug.Assert(false, "Failed to compute key in OpenILModuleReaderAfterReadingAllBytes cache. Falling back to uncached.") 
            ("",System.DateTime.Now), false
    let cacheResult = 
        if not succeeded then None // Fall back to uncached.
        else if opts.pdbPath.IsSome then None // can't used a cached entry when reading PDBs, since it makes the returned object IDisposable
        else ilModuleReaderCache.TryGet(key) 
    match cacheResult with 
    | Some(ilModuleReader) -> ilModuleReader
    | None -> 
        let mc = MemChannel.open_in infile
        let modul,ilAssemblyRefs,pdb = gen_open_binary_reader infile (Mem mc) opts
        let ilModuleReader = 
            { modul = modul; 
              ilAssemblyRefs = ilAssemblyRefs
              dispose = (fun () -> 
                match pdb with 
                | Some (pdbr,_) -> pdbReadClose pdbr
                | None -> ()) }
        if isNone pdb && succeeded then 
            ilModuleReaderCache.Put(key, ilModuleReader)
        ilModuleReader



