// (c) Microsoft Corporation 2005-2009. 

#light

module Microsoft.FSharp.Compiler.AbstractIL.BinaryWriter 

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open System.Collections.Generic 
open Microsoft.FSharp.Compiler.DiagnosticMessage
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Range
open System.IO

module Ilx = Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Types  
module Ilsupp = Microsoft.FSharp.Compiler.AbstractIL.Internal.Support 
module Ildiag = Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
module Ilbinary = Microsoft.FSharp.Compiler.AbstractIL.Internal.BinaryConstants 
module Ilprint = Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 
module Illib = Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

open Illib
open Ildiag
open Il
open Ilbinary
open Ilsupp

module Bytebuf = Bytes.Bytebuf

(*---------------------------------------------------------------------
 * The big writer.
 *---------------------------------------------------------------------*)

let checking = false 
let _ = if checking then dprintn "warning : Ilwrite.checking is on"
let logging = false
let showTimes = ref false
#if DEBUG
let showEntryLookups = false
#endif

(*---------------------------------------------------------------------
 * Library
 *---------------------------------------------------------------------*)

let (@@) = Array.append  

let reportTime =
  let tFirst = ref None     
  let tPrev = ref None     
  fun descr ->
    if !showTimes then 
      let t = System.Diagnostics.Process.GetCurrentProcess().UserProcessorTime.TotalSeconds
      let prev = match !tPrev with None -> 0.0 | Some t -> t
      let first = match !tFirst with None -> (tFirst := Some t; t) | Some t -> t
      dprintf "ilwrite: TIME %10.3f (total)   %10.3f (delta) - %s\n" (t - first) (t - prev) descr;
      tPrev := Some t

(*---------------------------------------------------------------------
 * Byte, byte array fragments and other concrete representations
 * manipulations.
 *---------------------------------------------------------------------*)

(* Little-endian encoding of int32 *)
let b0 n =  (n &&& 0xFF)
let b1 n =  ((n >>> 8) &&& 0xFF)
let b2 n =  ((n >>> 16) &&& 0xFF)
let b3 n =  ((n >>> 24) &&& 0xFF)

(* Little-endian encoding of int64 *)
let dw7 n = int32 ((n >>> 56) &&& 0xFFL)
let dw6 n = int32 ((n >>> 48) &&& 0xFFL)
let dw5 n = int32 ((n >>> 40) &&& 0xFFL)
let dw4 n = int32 ((n >>> 32) &&& 0xFFL)
let dw3 n = int32 ((n >>> 24) &&& 0xFFL)
let dw2 n = int32 ((n >>> 16) &&& 0xFFL)
let dw1 n = int32 ((n >>> 8)  &&& 0xFFL)
let dw0 n = int32 (n &&& 0xFFL)

(* REVIEW: get rid of all of these in favour of writing directly to buffers *)
let u8_as_intarray (i:byte) = [| b0 (int i) |]
let u16_as_intarray (x:uint16) =  let n = (int x) in [| b0 n; b1 n |]
let i32_as_intarray (i:int32) = [| b0 i; b1 i; b2 i; b3 i |]
let i64_as_intarray (i:int64) = [| dw0 i; dw1 i; dw2 i; dw3 i; dw4 i; dw5 i; dw6 i; dw7 i |]

let i8_as_intarray (i:sbyte) = u8_as_intarray (byte i)
let i16_as_intarray (i:int16) = u16_as_intarray (uint16 i)
let u32_as_intarray (i:uint32) = i32_as_intarray (int32 i)
let u64_as_intarray (i:uint64) = i64_as_intarray (int64 i)

let bits_of_float32 (x:float32) = System.BitConverter.ToInt32(System.BitConverter.GetBytes(x),0)
let bits_of_float (x:float) = System.BitConverter.DoubleToInt64Bits(x)

let ieee32_as_intarray i = i32_as_intarray (bits_of_float32 i)
let ieee64_as_intarray i = i64_as_intarray (bits_of_float i)

let emit f = let bb = Bytebuf.create 10 in f bb; Bytebuf.close bb

/// Alignment and padding
let align alignment n = ((n + alignment - 1) / alignment) * alignment

(*---------------------------------------------------------------------
 * Concrete token representations etc. used in PE files
 *---------------------------------------------------------------------*)

let z_u32_size n = 
  if n <= 0x7F then 1
  elif n <= 0x3FFF then 2
  else 4

let emit_z_u32 bb n = 
    if n >= 0 &&  n <= 0x7F then 
        Bytebuf.emit_int_as_byte bb n  
    elif n >= 0x80 && n <= 0x3FFF then 
        Bytebuf.emit_int_as_byte bb (0x80 ||| (n >>> 8));
        Bytebuf.emit_int_as_byte bb (n &&& 0xFF) 
    else 
        Bytebuf.emit_int_as_byte bb (0xc0l ||| ((n >>> 24) &&& 0xFF));
        Bytebuf.emit_int_as_byte bb (           (n >>> 16) &&& 0xFF);
        Bytebuf.emit_int_as_byte bb (           (n >>> 8)  &&& 0xFF);
        Bytebuf.emit_int_as_byte bb (            n         &&& 0xFF)

let emit_pad buf n = 
    for i = 0 to n-1 do
        Bytebuf.emit_int_as_byte buf 0x0

let bytebuf_emit_z_untagged_index buf  big idx = 
    if big then Bytebuf.emit_i32 buf idx
    elif idx > 0xffff then failwith "z_untagged_index: too big for small address or simple index"
    else Bytebuf.emit_i32_as_u16 buf idx

let bytebuf_emit_z_tagged_index buf tag nbits big idx =
    let idx2 = (idx <<< nbits) ||| tag
    if big then Bytebuf.emit_i32 buf idx2
    else Bytebuf.emit_i32_as_u16 buf idx2

let GetUncodedToken tab idx = (((tag_of_table tab) <<< 24) ||| idx)

(* From ECMA for UserStrings:
This final byte holds the value 1 if and only if any UTF16 character within the string has any bit set in its top byte, or its low byte is any of the following:
0x01–0x08, 0x0E–0x1F, 0x27, 0x2D,
0x7F. Otherwise, it holds 0. The 1 signifies Unicode characters that require handling beyond that normally provided for 8-bit encoding sets.
*)

let marker_for_unicode_bytes (b:byte[]) = 
    let len = Bytes.length b
    let rec scan i = 
        i < len/2 && 
        (let b1 = Bytes.get b (i*2)
         let b2 = Bytes.get b (i*2+1)
         (b2 <> 0)
         || (b1 >= 0x01 && b1 <= 0x08) 
         || (b1 >= 0xE && b1 <= 0x1F)
         || (b1 = 0x27)
         || (b1 = 0x2D)
         || scan (i+1))
    let marker = if scan 0 then 0x01 else 0x00
    marker


(* -------------------------------------------------------------------- 
 * Fixups
 * -------------------------------------------------------------------- *)

/// Check that the data held at a fixup is some special magic value, as a sanity check
/// to ensure the fixup is being placed at a ood lcoation.
let CheckFixup32 data offset exp = 
    if Bytes.get data (offset + 3) <> b3 exp then failwith "fixup sanity check failed";
    if Bytes.get data (offset + 2) <> b2 exp then failwith "fixup sanity check failed";
    if Bytes.get data (offset + 1) <> b1 exp then failwith "fixup sanity check failed";
    if Bytes.get data (offset) <> b0 exp then failwith "fixup sanity check failed"

let ApplyFixup32 data offset v = 
    Bytes.set data (offset)   (b0 v);
    Bytes.set data (offset+1) (b1 v);
    Bytes.set data (offset+2) (b2 v);
    Bytes.set data (offset+3) (b3 v)

(* -------------------------------------------------------------------- 
 * PDB data
 * -------------------------------------------------------------------- *)
 
let NoPDBsOnMonoWarningE = DeclareResourceString("NoPDBsOnMonoWarning","")

type pdb_document = Il.ILSourceDocument
(* type pdb_namespace = string  (* todo: do we need more here? *)*)
type pdb_var = 
    { pdbVarName: string;
      pdbVarSig: byte[]; 
      pdbVarAttributes: int32 (* this is essentially the local index the name corresponds to *) }

type pdb_method_scope = 
    { Children: pdb_method_scope array;
      StartOffset: int;
      EndOffset: int;
      Locals: pdb_var array;
      (* REVIEW open_namespaces: pdb_namespace array; *) }

type PdbSourceLoc = 
    { Document: int;
      Line: int;
      Column: int; }
      
type PdbSequencePoint = 
    { Document: int;
      Offset: int;
      Line: int;
      Column: int;
      EndLine: int;
      EndColumn: int; }
    override x.ToString() = sprintf "(%d,%d)-(%d,%d)" x.Line x.Column x.EndLine x.EndColumn

type PdbMethodData = 
    { MethToken: int32;
      MethName:string;
      Params: pdb_var array;
      RootScope: pdb_method_scope;
      Range: (PdbSourceLoc * PdbSourceLoc) option;
      SequencePoints: PdbSequencePoint array; }

let compare_seqpoints_by_source sp1 sp2 = 
    let c1 = compare sp1.Document sp2.Document
    if c1 <> 0 then c1 else 
    let c1 = compare sp1.Line sp2.Line
    if c1 <> 0 then c1 else 
    compare sp1.Column sp2.Column 
    
let compare_seqpoints_by_offset sp1 sp2 = 
  compare sp1.Offset sp2.Offset 

let sizeof_IMAGE_DEBUG_DIRECTORY = 28 (* 28 is the size of the IMAGE_DEBUG_DIRECTORY in ntimage.h *)

type pdb_data = 
    { EntryPoint: int32 option;
      Documents: pdb_document array;
      Methods: PdbMethodData array }

//---------------------------------------------------------------------
// PDB Writer.  The function [WritePdbInfo] abstracts the 
// imperative calls to the Symbol Writer API.
//---------------------------------------------------------------------

let WritePdbInfo  fixupOverlappingSequencePoints f fpdb info = 
    (try System.IO.File.Delete(fpdb) with _ -> ());
    let pdbw = ref Unchecked.defaultof<pdb_writer>
    
    try
        pdbw := pdbInitialize f fpdb
    with _ -> error(Error((sprintf "Unexpected error creating debug information file '%s'" fpdb), rangeCmdArgs))

    match info.EntryPoint with 
    | None -> () 
    | Some x -> pdbSetUserEntryPoint !pdbw x 

    let docs = info.Documents |> Array.map (fun doc -> pdbDefineDocument !pdbw doc.File)
    let get_doc i = 
      if i < 0 or i >= Array.length docs then failwith "get_doc: bad doc number";
      docs.[i]
    reportTime (Printf.sprintf "PDB: Defined %d documents" info.Documents.Length);
    Array.sortInPlaceBy (fun x -> x.MethToken) info.Methods;

    reportTime (Printf.sprintf "PDB: Sorted %d methods" info.Methods.Length);

    (* This next bit is a workaround.  The sequence points we get *)
    (* from F# (which has nothing to do with this module) are actually expression *)
    (* marks, i.e. the source ranges they denote are typically *)
    (* nested, and each point indicates where the  *)
    (* code for an expression with a particular range begins.  *)
    (* This is in many ways a much more convenient form to emit. *)
    (* However, it is not the form that debug tools accept nicely. *)
    (* However, sequence points are really a non-overlapping, non-nested *)
    (* partition of the source code of a method.  So here we shorten the *)
    (* length of all sequence point marks so they do not go further than *)
    (* the next sequence point in the source. *)
    let spCounts =  info.Methods |> Array.map (fun x -> x.SequencePoints.Length)
    let allSps = Array.concat (Array.map (fun x -> x.SequencePoints) info.Methods |> Array.to_list)
    let allSps = Array.mapi (fun i sp -> (i,sp)) allSps
    if fixupOverlappingSequencePoints then 
        // sort the sequence points into source order 
        Array.sortInPlaceWith (fun (_,sp1) (_,sp2) -> compare_seqpoints_by_source sp1 sp2) allSps;
        // shorten the ranges of any that overlap with following sequence points 
        // sort the sequence points back into offset order 
        for i = 0 to Array.length allSps - 2 do
            let n,sp1 = allSps.[i]
            let _,sp2 = allSps.[i+1]
            if (sp1.Document = sp2.Document) && 
               (sp1.EndLine > sp2.Line || 
                (sp1.EndLine = sp2.Line &&
                 sp1.EndColumn >= sp2.Column)) then
              let adjustToPrevLine = (sp1.Line < sp2.Line)
              allSps.[i] <-  n,{sp1 with EndLine = (if adjustToPrevLine then sp2.Line-1 else sp2.Line);
                                         EndColumn = (if adjustToPrevLine then 80 else sp2.Column); }
        Array.sortInPlaceBy fst allSps;


    
    let spOffset = ref 0
    info.Methods |> Array.iteri (fun i minfo ->

          let sps = Array.sub allSps !spOffset spCounts.[i]
          spOffset := !spOffset + spCounts.[i];
          begin match minfo.Range with 
          | None -> () 
          | Some (a,b) ->
              pdbOpenMethod !pdbw minfo.MethToken;

              pdbSetMethodRange !pdbw 
                (get_doc a.Document) a.Line a.Column
                (get_doc b.Document) b.Line b.Column;

              (* Partition the sequence points by document *)
              let spsets =
                let res = (Map.empty : Map<int,PdbSequencePoint list ref>)
                let add res (_,sp) = 
                  let k = sp.Document
                  match Map.tryfind k res with
                      Some xsR -> xsR := sp :: !xsR; res
                    | None     -> Map.add k (ref [sp]) res
               
                let res = Array.fold_left add res sps
                let res = Map.to_list res in  (* ordering may bnot be stable *)
                List.map (fun (_,x) -> Array.of_list !x) res

              spsets |> List.iter (fun spset -> 
                  if spset.Length > 0 then 
                    Array.sortInPlaceWith compare_seqpoints_by_offset spset;
                    let sps = 
                      spset |> Array.map (fun sp -> 
                           (* Ildiag.dprintf "token 0x%08lx has an sp at offset 0x%08x\n" minfo.MethToken sp.Offset; *)
                           (sp.Offset, sp.Line, sp.Column,sp.EndLine, sp.EndColumn)) 
                  (* Use of alloca in implementation of pdbDefineSequencePoints can give stack overflow here *)
                    if sps.Length < 5000 then 
                      pdbDefineSequencePoints !pdbw (get_doc spset.[0].Document) sps;);

              (* Write the scopes *)
              let rec writePdbScope top sco = 
                  if top or sco.Locals.Length <> 0 or sco.Children.Length <> 0 then 
                      pdbOpenScope !pdbw sco.StartOffset;
                      sco.Locals |> Array.iter (fun v -> pdbDefineLocalVariable !pdbw v.pdbVarName v.pdbVarSig v.pdbVarAttributes);
                      sco.Children |> Array.iter (writePdbScope false);
                      pdbCloseScope !pdbw sco.EndOffset;
              writePdbScope true minfo.RootScope; 

              pdbCloseMethod !pdbw
          end);
    reportTime "PDB: Wrote methods";
    let res = pdbGetDebugInfo !pdbw
    pdbClose !pdbw;
    reportTime "PDB: Closed";
    res

//---------------------------------------------------------------------
// Strong name signing
//---------------------------------------------------------------------

type signer =  
  | PublicKeySigner of Ilsupp.pubkey
  | KeyPair of keyPair
  | KeyContainer of keyContainerName

let signerOpenPublicKeyFile s = 
  let pubkey = Ilsupp.signerOpenPublicKeyFile s
  PublicKeySigner(pubkey)
  
let signerOpenPublicKey pubkey = 
  PublicKeySigner(pubkey)

let signerOpenKeyPairFile s = 
  let keypair = Ilsupp.signerOpenKeyPairFile s
  KeyPair(keypair)

let signerOpenKeyContainer s = 
  KeyContainer(s)

let signerClose s = 
  match s with 
  | PublicKeySigner _
  | KeyPair _ -> ()
  | KeyContainer containerName -> Ilsupp.signerCloseKeyContainer(containerName)
  
let signerFullySigned s =
  match s with 
  | PublicKeySigner _ -> false
  | KeyPair _ | KeyContainer _ -> true

let signerPublicKey s = 
  match s with 
  | PublicKeySigner p -> p
  | KeyPair kp -> Ilsupp.signerGetPublicKeyForKeyPair kp
  | KeyContainer kn -> Ilsupp.signerGetPublicKeyForKeyContainer kn

let signerSignatureSize s = 
  try Ilsupp.signerSignatureSize(signerPublicKey s)
  with e -> 
    failwith ("A call to StrongNameSignatureSize failed ("^e.Message^")");
    0x80 

let signerSignFile file s = 
  match s with 
  | PublicKeySigner p -> ()
  | KeyPair kp -> Ilsupp.signerSignFileWithKeyPair file kp
  | KeyContainer kn -> Ilsupp.signerSignFileWithKeyContainer file kn


//---------------------------------------------------------------------
// TYPES FOR TABLES
//---------------------------------------------------------------------

type RowElement = 
  | UShort of uint16
  | ULong of int32
  | Data of int * bool (* Index into cenv.data or cenv.resources.  Will be adjusted later in writing once we fix an overall location for the data section.  flag indicates if offset is relative to cenv.resources. *)
  | Guid of int (* pos. in guid array *)
  | Blob of int (* pos. in blob array *)
  | String of int (* pos. in string array *)
  | SimpleIndex of table * int (* pos. in some table *)
  | TypeDefOrRefOrSpec of typeDefOrRef_tag * int
  | TypeOrMethodDef of typeOrMethodDef_tag * int
  | HasConstant of hasConstant_tag * int
  | HasCustomAttribute of hasCustomAttribute_tag * int
  | HasFieldMarshal of hasFieldMarshal_tag * int
  | HasDeclSecurity of hasDeclSecurity_tag * int
  | MemberRefParent of memberRefParent_tag * int
  | HasSemantics of hasSemantics_tag * int
  | MethodDefOrRef of methodDefOrRef_tag * int
  | MemberForwarded of  memberForwarded_tag * int
  | Implementation of implementation_tag * int
  | CustomAttributeType of customAttributeType_tag * int
  | ResolutionScope of resolutionScope_tag * int

type BlobIndex = int
type StringIndex = int

let BlobIndex (x:BlobIndex) : int = x
let StringIndex (x:StringIndex) : int = x

/// Abstract, general type of metadata table rows
type IGenericRow = 
    abstract GetGenericRow : unit -> array<RowElement>

/// Shared rows are used for the ILTypeRef, ILMethodRef, ILMethodSpec, etc. tables
/// where entries can be shared and need to be made unique through hash-cons'ing
type ISharedRow = 
    inherit IGenericRow
    
/// This is the representation of shared rows is used for most shared row types.
/// Rows ILAssemblyRef and ILMethodRef are very common and are given their own
/// representations.
type SimpleSharedRow(elems: array<RowElement>) =
    let hashCode = hash elems // precompute to give more efficient hashing and equality comparisons
    interface ISharedRow with 
        member x.GetGenericRow() = elems
    member x.GenericRow = elems
    override x.GetHashCode() = hashCode
    override x.Equals(obj:obj) = 
        match obj with 
        | :? SimpleSharedRow as y -> elems = y.GenericRow
        | _ -> false

/// Unshared rows are used for definitional tables where elements do not need to be made unique
/// e.g. ILMethodDef and ILTypeDef. Most tables are like this. We don't precompute a 
/// hash code for these rows, and indeed the GetHashCode and Equals should not be needed.
type UnsharedRow(elems: array<RowElement>) =
    interface IGenericRow with 
        member x.GetGenericRow() = elems
    member x.GenericRow = elems
    override x.GetHashCode() = hash elems
    override x.Equals(obj:obj) = 
        match obj with 
        | :? UnsharedRow as y -> elems = y.GenericRow
        | _ -> false
             
let inline combineHash x2 acc = 37 * acc + x2 // (acc <<< 6 + acc >>> 2 + x2 + 0x9e3779b9)

/// Special representation for ILAssemblyRef rows
type AssemblyRefRow(s1,s2,s3,s4,l1,b1,nameIdx,str2,b2) = 
    let hashCode = hash nameIdx
    let genericRow = [| UShort s1; UShort s2; UShort s3; UShort s4; ULong l1; Blob b1; String nameIdx; String str2; Blob b2 |]
    interface ISharedRow with 
        member x.GetGenericRow() =  genericRow            
    member x.GenericRow = genericRow
    override x.GetHashCode() = hashCode
    override x.Equals(obj:obj) = 
        match obj with 
        | :? AssemblyRefRow as y -> genericRow = y.GenericRow
        | _ -> false

/// Special representation of a very common kind of row
type MemberRefRow(mrp:RowElement,nmIdx:StringIndex,blobIdx:BlobIndex) = 
    let hash =  hash mrp |> combineHash (hash nmIdx) |> combineHash (hash blobIdx)
    let genericRow = [| mrp; String nmIdx; Blob blobIdx |]
    interface ISharedRow with 
        member x.GetGenericRow() = genericRow
    member x.GenericRow = genericRow
    override x.GetHashCode() = hash
    override x.Equals(obj:obj) = 
        match obj with 
        | :? MemberRefRow as y -> genericRow = y.GenericRow
        | _ -> false

(*=====================================================================
 *=====================================================================
 * IL --> TABLES+CODE
s *=====================================================================
 *=====================================================================*)

// This environment keeps track of how many generic parameters are in scope. 
// This lets us translate AbsIL type variable number to IL type variable numbering 
type env = 
    { envClassFormals: int }
let env_enter_tdef gparams ={envClassFormals=gparams }
let env_enter_msig tgparams mgparams =
    { envClassFormals=tgparams }
let env_enter_fspec (fspec:ILFieldSpec) =
    { envClassFormals=List.length (inst_of_typ fspec.EnclosingType) }
let env_enter_ospec (ospec:OverridesSpec) =
    { envClassFormals=List.length (inst_of_typ ospec.EnclosingType) }

let env_enter_mref mref =
    { envClassFormals=System.Int32.MaxValue }

let mk_env = { envClassFormals=0 }

(*---------------------------------------------------------------------
 * TABLES
 *---------------------------------------------------------------------*)

[<StructuralEquality(false); StructuralComparison(false)>]
type MetadataTable<'a> = 
    { name: string;
      dict: Dictionary<'a, int>; // given a row, find its entry number
#if DEBUG
      mutable lookups: int;
#endif
      mutable rows: ResizeArray<'a> ; }
    member x.Count = x.rows.Count

// inline justification: allow hash equality function to be specialized for type of key 
let NewTable(nm,hashEq) = 
  { name=nm;
#if DEBUG
    lookups=0;
#endif
    dict = new Dictionary<_,_>(100, hashEq);
    rows= new ResizeArray<_>(); }

let GetTableEntries (tbl:MetadataTable<'a>) = 
#if DEBUG
    if showEntryLookups then dprintf "--> table %s had %d entries and %d lookups\n" tbl.name tbl.Count tbl.lookups;
#endif
    tbl.rows |> ResizeArray.to_list

let AddSharedEntry tbl x =
    let n = tbl.rows.Count + 1
    tbl.dict.[x] <- n;
    tbl.rows.Add(x);
    n

let AddUnsharedEntry tbl x =
    let n = tbl.rows.Count + 1
    tbl.rows.Add(x);
    n

let FindOrAddSharedEntry tbl x =
#if DEBUG
    tbl.lookups <- tbl.lookups + 1; 
#endif
    let mutable res = Unchecked.defaultof<_>
    let ok = tbl.dict.TryGetValue(x,&res)
    if ok then res
    else AddSharedEntry tbl x


/// This is only used in one special place - see furthre below. 
let SetRowsOfTable tbl t = 
    tbl.rows <- ResizeArray.of_list t;  
    let h = tbl.dict
    h.Clear();
    t |> List.iteri (fun i x -> 
        h.[x] <- (i+1))

let AddUniqueEntry nm geterr tbl x =
    if tbl.dict.ContainsKey x then failwith ("duplicate entry '"^geterr x^"' in "^nm^" table")
    else AddSharedEntry tbl x

let GetTableEntry tbl x = tbl.dict.[x] 

//---------------------------------------------------------------------
// Keys into some of the tables
//---------------------------------------------------------------------

/// We use this key type to help find ILMethodDefs for MethodRefs 
type MethodDefKey(tidx:int,garity:int,nm:string,rty:ILType,argtys:ILType list) =
    // Precompute the hash. The hash doesn't include the return type or 
    // argument types (only argument type count). This is very important, since
    // hashing these is way too expensive
    let hashCode = 
       hash tidx 
       |> combineHash (hash garity) 
       |> combineHash (hash nm) 
       |> combineHash (hash argtys.Length)
    member key.TypeIdx = tidx
    member key.GenericArity = garity
    member key.Name = nm
    member key.ReturnType = rty
    member key.ArgTypes = argtys
    override x.GetHashCode() = hashCode
    override x.Equals(obj:obj) = 
        match obj with 
        | :? MethodDefKey as y -> 
            tidx = y.TypeIdx && 
            garity = y.GenericArity && 
            nm = y.Name && 
            // note: these next two use structural equality on AbstractIL ILType values
            rty = y.ReturnType && 
            argtys = y.ArgTypes
        | _ -> false

/// We use this key type to help find ILFieldDefs for FieldRefs
type FieldDefKey(tidx:int,nm:string,ty:ILType) = 
    // precompute the hash. hash doesn't include the type 
    let hashCode = hash tidx |> combineHash (hash nm) 
    member key.TypeIdx = tidx
    member key.Name = nm
    member key.Type = ty
    override x.GetHashCode() = hashCode
    override x.Equals(obj:obj) = 
        match obj with 
        | :? FieldDefKey as y -> 
            tidx = y.TypeIdx && 
            nm = y.Name && 
            ty = y.Type 
        | _ -> false

type propkey = PropKey of int (* type. def. idx. *) * string * ILType * ILType list
type eventkey = EventKey of int (* type. def. idx. *) * string
type tdkey = TdKey of string list (* enclosing *) * string (* type name *)

(*---------------------------------------------------------------------
 * The Writer Context
 *---------------------------------------------------------------------*)

[<StructuralEquality(false); StructuralComparison(false)>]
type cenv = 
    { mscorlib: ILScopeRef;
      ilg: ILGlobals;
      desiredMetadataVersion: ILVersionInfo;
      reqd_data_fixups: (int32 * (int * bool)) list ref;
      /// References to strings in codestreams: offset of code and a (fixup-location , string token) list) 
      mutable reqd_string_fixups: (int32 * (int * int) list) list; 
      code_chunks: Bytebuf.t; 
      mutable next_code_addr: int32;
      generate_pdb: bool;
      pdbinfo: ResizeArray<PdbMethodData>;
      documents: pdb_document MetadataTable;
      /// Raw data, to go into the data section 
      data: Bytebuf.t; 
      /// Raw resource data, to go into the data section 
      resources: Bytebuf.t; 
      mutable entrypoint: (bool * int) option; 

      /// Caches
      trefCache: Dictionary<ILTypeRef,int>;

      /// The following are all used to generate unique items in the output 
      tables: array<MetadataTable<IGenericRow>>;
      AssemblyRefs: MetadataTable<AssemblyRefRow>;
      fieldDefs: MetadataTable<FieldDefKey>;
      methodDefIdxsByKey:  MetadataTable<MethodDefKey>;
      methodDefIdxs:  Dictionary<ILMethodDef,int>;
      propertyDefs: MetadataTable<propkey>;
      eventDefs: MetadataTable<eventkey>;
      typeDefs: MetadataTable<tdkey>; 
      guids: MetadataTable<byte[]>; 
      blobs: MetadataTable<byte[]>; 
      strings: MetadataTable<string>; 
      userStrings: MetadataTable<string>;
  }

let table cenv (Table idx) = cenv.tables.[idx]

let FindOrAddRow cenv tbl (x:IGenericRow) = FindOrAddSharedEntry (table cenv tbl) x

// Shared rows must be hash-cons'd to be made unique (no duplicates according to contents)
let AddSharedRow cenv tbl (x:ISharedRow) = AddSharedEntry (table cenv tbl) (x :> IGenericRow)

// Unshared rows correspond to definition elements (e.g. a ILTypeDef or a ILMethodDef)
let AddUnsharedRow cenv tbl (x:UnsharedRow) = AddUnsharedEntry (table cenv tbl) (x :> IGenericRow)

let metadataSchemaVersionSupportedByCLRVersion v = 
    (* Whidbey Beta 1 version numbers are between 2.0.40520.0 and 2.0.40607.0 *)
    (* Later Whidbey versions are post 2.0.40607.0.. However we assume *)
    (* internal builds such as 2.0.x86chk are Whidbey Beta 2 or later *)
    if Il.version_compare v (parse_version ("2.0.40520.0")) >= 0 &&
       Il.version_compare  v (parse_version ("2.0.40608.0")) < 0 then 1,1
    elif Il.version_compare v (parse_version ("2.0.0.0")) >= 0 then 2,0
    else 1,0 

let headerVersionSupportedByCLRVersion v = 
   (* The COM20HEADER version number *)
   (* Whidbey version numbers are 2.5 *)
   (* Earlier are 2.0 *)
   (* From an email from jeffschw: "Be built with a compiler that marks the COM20HEADER with Major >=2 and Minor >= 5.  The V2.0 compilers produce images with 2.5, V1.x produces images with 2.0." *)
    if Il.version_compare v (parse_version ("2.0.0.0")) >= 0 then 2,5
    else 2,0 

let peOptionalHeaderByteByCLRVersion v = 
   (*  A flag in the PE file optional header seems to depend on CLI version *)
   (* Whidbey version numbers are 8 *)
   (* Earlier are 6 *)
   (* Tools are meant to ignore this, but the VS Profiler wants it to have the right value *)
    if Il.version_compare v (parse_version ("2.0.0.0")) >= 0 then 8
    else 6

(* returned by write_binary_internal *)
[<StructuralEquality(false); StructuralComparison(false)>]
type mappings =  
    { tdefMap: ILTypeDef list * ILTypeDef -> int32;
      fdefMap: ILTypeDef list * ILTypeDef -> ILFieldDef -> int32;
      mdefMap: ILTypeDef list * ILTypeDef -> ILMethodDef -> int32;
      propertyMap: ILTypeDef list * ILTypeDef -> ILPropertyDef -> int32;
      eventMap: ILTypeDef list * ILTypeDef -> ILEventDef -> int32 }

let record_reqd_data_fixup reqd_data_fixups buf pos lab =
    reqd_data_fixups :=  (pos,lab) :: !reqd_data_fixups;
    (* Write a special value in that we check later when applying the fixup *)
    Bytebuf.emit_i32 buf 0xdeaddddd


    
(*---------------------------------------------------------------------
 * Utilities Used During Writing
 *---------------------------------------------------------------------*)

let add_code cenv ((reqd_string_fixups_offset,reqd_string_fixups),code) = 
    if align 4 cenv.next_code_addr <> cenv.next_code_addr then dprintn "warning: code not 4-byte aligned";
    cenv.reqd_string_fixups <- (cenv.next_code_addr + reqd_string_fixups_offset, reqd_string_fixups) :: cenv.reqd_string_fixups;
    Bytebuf.emit_bytes cenv.code_chunks code;
    cenv.next_code_addr <- cenv.next_code_addr + (Bytes.length code)

let get_code cenv = Bytebuf.close cenv.code_chunks

(*---------------------------------------------------------------------
 * Split type names into namespace/name pairs and memoize.
 *---------------------------------------------------------------------*)

let split_name_at nm idx = 
  let last = (String.length nm) - 1
  (String.sub nm 0 idx),
  (if idx < last then String.sub nm (idx + 1) (last - idx) else "")

let split_namespace_aux nm = 
    if String.contains nm '.' then 
      let idx = String.rindex nm '.'
      let s1,s2 = split_name_at nm idx
      Some s1,s2 
    else None, nm

let memoize_namespace_tab = Dictionary.create 100

let split_namespace nm =
    if Dictionary.mem memoize_namespace_tab nm then
        Dictionary.find memoize_namespace_tab nm
    else
        let x = split_namespace_aux nm
        (Dictionary.add  memoize_namespace_tab nm x; x)

(*---------------------------------------------------------------------
 * The UserString, BlobHeap, GuidHeap tables
 *---------------------------------------------------------------------*)

let string_as_utf8_intarray (s:string) = 
    Bytes.to_intarray (Bytes.string_as_utf8_bytes s)
   
let GetUserStringHeapIdx cenv s = 
    FindOrAddSharedEntry cenv.userStrings s

let GetBytesAsBlobIdx cenv bytes = 
    if Bytes.length bytes = 0 then 0 
    else FindOrAddSharedEntry cenv.blobs bytes

let GetStingHeapIdx cenv s = 
    if s = "" then 0 
    else FindOrAddSharedEntry cenv.strings s

let GetGuidIdx cenv info = FindOrAddSharedEntry cenv.guids (Bytes.of_intarray info)

let GetIntArrayAsBlobIdx cenv blob = GetBytesAsBlobIdx cenv (Bytes.of_intarray blob)

let GetStringHeapIdxOption cenv sopt =
    match sopt with 
    | Some ns -> GetStingHeapIdx cenv ns
    | None -> 0

let name_as_elem_pair cenv n =
    let (n1,n2) = split_namespace n
    String (GetStringHeapIdxOption cenv n1),
    String (GetStingHeapIdx cenv n2)

(*=====================================================================
 * Pass 1 - allocate indexes for types 
 *=====================================================================*)

let name_of_tdkey (TdKey (enc,n)) = n

let rec tdef_pass1 enc cenv (td:ILTypeDef) = 
  ignore (AddUniqueEntry "type index" name_of_tdkey cenv.typeDefs (TdKey (enc,td.Name)));
  GenTypeDefsPass1 (enc@[td.Name]) cenv (dest_tdefs td.NestedTypes)

and GenTypeDefsPass1 enc cenv tds = List.iter (tdef_pass1 enc cenv) tds


(*=====================================================================
 * Pass 2 - allocate indexes for methods and fields and write rows for types 
 *=====================================================================*)

let rec GetIdxForTypeDef cenv key  = 
    try GetTableEntry cenv.typeDefs key
    with 
      :? KeyNotFoundException -> 
        let (TdKey (enc,n) ) = key
        failwith ("One of your modules expects the type '"^String.concat "." (enc@[n])^"' to be defined within the module being emitted.  You may be missing an input file")
    
(* -------------------------------------------------------------------- 
 * Assembly and module references
 * -------------------------------------------------------------------- *)

let rec GetAssemblyRefAsRow cenv (aref:ILAssemblyRef) =
    AssemblyRefRow 
        ((match aref.Version with None -> 0us | Some (x,y,z,w) -> x),
         (match aref.Version with None -> 0us | Some (x,y,z,w) -> y),
         (match aref.Version with None -> 0us | Some (x,y,z,w) -> z),
         (match aref.Version with None -> 0us | Some (x,y,z,w) -> w),
         ((match aref.PublicKey with Some (PublicKey _) -> 0x0001 | _ -> 0x0000)
          ||| (if aref.Retargetable then 0x0100 else 0x0000)),
         BlobIndex (match aref.PublicKey with 
                    | None ->  0 
                    | Some (PublicKey b | PublicKeyToken b) -> GetBytesAsBlobIdx cenv b),
         StringIndex (GetStingHeapIdx cenv aref.Name),
         StringIndex (match aref.Locale with None -> 0 | Some s -> GetStingHeapIdx cenv s),
         BlobIndex (match aref.Hash with None -> 0 | Some s -> GetBytesAsBlobIdx cenv s))
  
and GetAssemblyRefAsIdx cenv aref = 
    FindOrAddRow cenv tab_AssemblyRef (GetAssemblyRefAsRow cenv aref)

and GetModuleRefAsRow cenv (mref:ILModuleRef) =
    SimpleSharedRow 
        [| String (GetStingHeapIdx cenv mref.Name) |]

and GetModuleRefAsFileRow cenv (mref:ILModuleRef) =
    SimpleSharedRow 
        [|  ULong (if mref.HasMetadata then 0x0000 else 0x0001);
            String (GetStingHeapIdx cenv mref.Name);
            (match mref.Hash with None -> Blob 0 | Some s -> Blob (GetBytesAsBlobIdx cenv s)); |]

and GetModuleRefAsIdx cenv mref = 
    FindOrAddRow cenv tab_ModuleRef (GetModuleRefAsRow cenv mref)

and GetModuleRefAsFileIdx cenv mref = 
    FindOrAddRow cenv tab_File (GetModuleRefAsFileRow cenv mref)

(* -------------------------------------------------------------------- 
 * Does a ILScopeRef point to this module?
 * -------------------------------------------------------------------- *)

let scoref_is_local scoref = (scoref = ScopeRef_local) 

(* -------------------------------------------------------------------- 
 * Scopes to Implementation elements.
 * -------------------------------------------------------------------- *)

let scoref_as_Implementation_elem cenv scoref = 
    match scoref with 
    | ScopeRef_local ->  (i_AssemblyRef, 0)
    | ScopeRef_assembly aref -> (i_AssemblyRef, GetAssemblyRefAsIdx cenv aref)
    | ScopeRef_module mref -> (i_File, GetModuleRefAsFileIdx cenv mref)
 
(* -------------------------------------------------------------------- 
 * Type references, types etc.
 * -------------------------------------------------------------------- *)

let rec GetTypeRefAsTypeRefRow cenv (tref:ILTypeRef) = 
    let nselem,nelem = name_as_elem_pair cenv tref.Name
    let rs1,rs2 = rscope_as_ResolutionScope_elem cenv (tref.Scope,tref.Enclosing)
    SimpleSharedRow [| ResolutionScope (rs1,rs2); nelem; nselem |]

and GetTypeRefAsTypeRefIdx cenv tref = 
    let mutable res = 0
    if cenv.trefCache.TryGetValue(tref,&res) then res else 
    let res = FindOrAddRow cenv tab_TypeRef (GetTypeRefAsTypeRefRow cenv tref)
    cenv.trefCache.[tref] <- res;
    res

and GetTypeDescAsTypeRefIdx cenv (scoref,enc,n) =  
    GetTypeRefAsTypeRefIdx cenv (mk_nested_tref (scoref,enc,n))

and rscope_as_ResolutionScope_elem cenv (scoref,enc) = 
    if isNil enc then 
        match scoref with 
        | ScopeRef_local -> (rs_Module, 1) 
        | ScopeRef_assembly aref -> (rs_AssemblyRef, GetAssemblyRefAsIdx cenv aref)
        | ScopeRef_module mref -> (rs_ModuleRef, GetModuleRefAsIdx cenv mref)
    else
        let enc2,n2 = List.frontAndBack enc
        (rs_TypeRef, GetTypeDescAsTypeRefIdx cenv (scoref,enc2,n2))
 

let emit_tdesc_as_TypeDefOrRefEncoded cenv bb (scoref,enc,nm) = 
    if scoref_is_local scoref then 
        let idx = GetIdxForTypeDef cenv (TdKey(enc,nm))
        emit_z_u32 bb (idx <<< 2) (* ECMA 22.2.8 TypeDefOrRefEncoded - ILTypeDef *)
    else 
        let idx = GetTypeDescAsTypeRefIdx cenv (scoref,enc,nm)
        emit_z_u32 bb ((idx <<< 2) ||| 0x01) (* ECMA 22.2.8 TypeDefOrRefEncoded - ILTypeRef *)

let  tref_is_local cenv (tref:ILTypeRef) = scoref_is_local tref.Scope

let typ_is_local cenv typ = 
    is_tref_typ typ && isNil (inst_of_typ typ) && tref_is_local cenv (tref_of_typ typ)


let tdor_as_uncoded (tag,idx) =
    let tab = 
      if tag = tdor_TypeDef then tab_TypeDef 
      elif tag = tdor_TypeRef then tab_TypeRef  
      elif tag = tdor_TypeSpec then tab_TypeSpec
      else failwith "tdor_as_uncoded"
    GetUncodedToken tab idx

// REVIEW: write into an accumuating buffer
let EmitArrayShape bb (ILArrayShape shape) = 
    let sized = List.filter (function (_,Some _) -> true | _ -> false) shape
    let lobounded = List.filter (function (Some _,_) -> true | _ -> false) shape
    emit_z_u32 bb shape.Length;
    emit_z_u32 bb sized.Length;
    sized |> List.iter (function (_,Some sz) -> emit_z_u32 bb sz | _ -> failwith "?");
    emit_z_u32 bb lobounded.Length;
    lobounded |> List.iter (function (Some low,_) -> emit_z_u32 bb low | _ -> failwith "?") 
        
let hasthis_as_byte hasthis =
     match hasthis with 
     | CC_instance -> e_IMAGE_CEE_CS_CALLCONV_INSTANCE
     | CC_instance_explicit -> e_IMAGE_CEE_CS_CALLCONV_INSTANCE_EXPLICIT
     | CC_static -> 0x00

let callconv_as_byte ntypars (Callconv (hasthis,bcc)) = 
    hasthis_as_byte hasthis |||
    (if ntypars > 0 then e_IMAGE_CEE_CS_CALLCONV_GENERIC else 0x00) |||
    (match bcc with 
    | CC_fastcall -> e_IMAGE_CEE_CS_CALLCONV_FASTCALL
    | CC_stdcall -> e_IMAGE_CEE_CS_CALLCONV_STDCALL
    | CC_thiscall -> e_IMAGE_CEE_CS_CALLCONV_THISCALL
    | CC_cdecl -> e_IMAGE_CEE_CS_CALLCONV_CDECL
    | CC_default -> 0x00
    | CC_vararg -> e_IMAGE_CEE_CS_CALLCONV_VARARG)
  

// REVIEW: write into an accumuating buffer
let rec emit_tspec cenv env bb (et,tspec:ILTypeSpec) = 
    if isNil tspec.GenericArgs then 
        Bytebuf.emit_int_as_byte bb et;
        emit_tdesc_as_TypeDefOrRefEncoded cenv bb (tspec.Scope,tspec.Enclosing,tspec.Name)
    else  
        Bytebuf.emit_int_as_byte bb et_WITH;
        Bytebuf.emit_int_as_byte bb et;
        emit_tdesc_as_TypeDefOrRefEncoded cenv bb (tspec.Scope,tspec.Enclosing,tspec.Name);
        emit_z_u32 bb (List.length tspec.GenericArgs);
        EmitTypes cenv env bb tspec.GenericArgs

and GetTypeAsTypeDefOrRef cenv env ty = 
    if (typ_is_local cenv ty) then 
        let tref = tref_of_typ ty
        (tdor_TypeDef, GetIdxForTypeDef cenv (TdKey(tref.Enclosing,tref.Name)))
    elif is_tref_typ ty && isNil (inst_of_typ ty) then
        (tdor_TypeRef, GetTypeRefAsTypeRefIdx cenv (tref_of_typ ty))
    else 
        (tdor_TypeSpec, GetTypeAsTypeSpecIdx cenv env ty)

and GetTypeAsBytes cenv env ty = emit (fun bb -> EmitType cenv env bb ty)

and GetTypeAsBlobIdx cenv env (ty:ILType) = 
    GetBytesAsBlobIdx cenv (GetTypeAsBytes cenv env ty)

and GetTypeAsTypeSpecRow cenv env (ty:ILType) = 
    SimpleSharedRow [| Blob (GetTypeAsBlobIdx cenv env ty) |]

and GetTypeAsTypeSpecIdx cenv env ty = 
    FindOrAddRow cenv tab_TypeSpec (GetTypeAsTypeSpecRow cenv env ty)


and EmitType cenv env bb ty =
    let ilg = cenv.ilg
    match ty with 
  (* REVIEW: what are these doing here? *)
    | Type_value tspec when tspec.Name = "System.String" ->   Bytebuf.emit_int_as_byte bb et_STRING 
    | Type_value tspec when tspec.Name = "System.Object" ->   Bytebuf.emit_int_as_byte bb et_OBJECT 
    | typ when typ_is_SByte ilg typ ->   Bytebuf.emit_int_as_byte bb et_I1 
    | typ when typ_is_Int16 ilg typ ->   Bytebuf.emit_int_as_byte bb et_I2 
    | typ when typ_is_Int32 ilg typ ->    Bytebuf.emit_int_as_byte bb et_I4 
    | typ when typ_is_Int64 ilg typ ->     Bytebuf.emit_int_as_byte bb et_I8 
    | typ when typ_is_Byte ilg typ ->     Bytebuf.emit_int_as_byte bb et_U1 
    | typ when typ_is_UInt16 ilg typ ->     Bytebuf.emit_int_as_byte bb et_U2 
    | typ when typ_is_UInt32 ilg typ ->     Bytebuf.emit_int_as_byte bb et_U4 
    | typ when typ_is_UInt64 ilg typ ->     Bytebuf.emit_int_as_byte bb et_U8 
    | typ when typ_is_Double ilg typ ->     Bytebuf.emit_int_as_byte bb et_R8 
    | typ when typ_is_Single ilg typ ->     Bytebuf.emit_int_as_byte bb et_R4 
    | typ when typ_is_Bool ilg typ ->     Bytebuf.emit_int_as_byte bb et_BOOLEAN 
    | typ when typ_is_Char ilg typ ->     Bytebuf.emit_int_as_byte bb et_CHAR 
    | typ when typ_is_String ilg typ ->     Bytebuf.emit_int_as_byte bb et_STRING 
    | typ when typ_is_Object ilg typ ->     Bytebuf.emit_int_as_byte bb et_OBJECT 
    | typ when typ_is_IntPtr ilg typ ->     Bytebuf.emit_int_as_byte bb et_I 
    | typ when typ_is_UIntPtr ilg typ ->     Bytebuf.emit_int_as_byte bb et_U 
    | typ when typ_is_TypedReference ilg typ ->     Bytebuf.emit_int_as_byte bb et_TYPEDBYREF 

    | Type_boxed tspec ->  emit_tspec cenv env bb (et_CLASS,tspec)
    | Type_value tspec ->  emit_tspec cenv env bb (et_VALUETYPE,tspec)
    | Type_array (shape,ty) ->  
        if shape = Rank1ArrayShape then (Bytebuf.emit_int_as_byte bb et_SZARRAY ; EmitType cenv env bb ty)
        else (Bytebuf.emit_int_as_byte bb et_ARRAY; EmitType cenv env bb ty; EmitArrayShape bb shape)
    | Type_tyvar tv ->  
        let cgparams = env.envClassFormals
        if int32 tv <  cgparams then 
            Bytebuf.emit_int_as_byte bb et_VAR;
            emit_z_u32 bb (int32 tv)
        else
            Bytebuf.emit_int_as_byte bb et_MVAR;
            emit_z_u32 bb (int32 tv -  cgparams)

    | Type_byref typ -> 
        Bytebuf.emit_int_as_byte bb et_BYREF;
        EmitType cenv env bb typ
    | Type_ptr typ ->  
        Bytebuf.emit_int_as_byte bb et_PTR;
        EmitType cenv env bb typ
    | Type_void ->   
        Bytebuf.emit_int_as_byte bb et_VOID 
    | Type_fptr x ->
        Bytebuf.emit_int_as_byte bb et_FNPTR;
        EmitCallsig cenv env bb (x.CallingConv,x.ArgTypes,x.ReturnType,None,0)
    | Type_modified (req,tref,ty) ->
        Bytebuf.emit_int_as_byte bb (if req then et_CMOD_REQD else et_CMOD_OPT);
        emit_tdesc_as_TypeDefOrRefEncoded cenv bb (tref.Scope, tref.Enclosing,tref.Name);
        EmitType cenv env bb ty
     | _ -> failwith "EmitType"

// REVIEW: write into an accumuating buffer
and EmitCallsig cenv env bb (callconv,args,ret,(varargs:varargs),genarity) = 
      Bytebuf.emit_int_as_byte bb (callconv_as_byte genarity callconv);
      if genarity > 0 then emit_z_u32 bb (genarity);
      emit_z_u32 bb ((List.length args + (match varargs with None -> 0 | Some l -> List.length l)));
      EmitType cenv env bb ret;
      args |> List.iter (EmitType cenv env bb);
      match varargs with 
       | None | Some [] -> () (* no extra arg = no sentinel *)
       | Some tys -> 
           Bytebuf.emit_int_as_byte bb et_SENTINEL;
           List.iter (EmitType cenv env bb) tys

and GetCallsigAsBytes cenv env x = emit (fun bb -> EmitCallsig cenv env bb x)

// REVIEW: write into an accumuating buffer
and EmitTypes cenv env bb (inst: ILType list) = 
    inst |> List.iter (EmitType cenv env bb) 

let GetTypeAsMemberRefParent cenv env ty =
    match GetTypeAsTypeDefOrRef cenv env ty with 
    | (tag,tok) when tag = tdor_TypeDef -> dprintn "GetTypeAsMemberRefParent: mspec should have been encoded as mdtMethodDef?"; MemberRefParent (mrp_TypeRef, 1)
    | (tag,tok) when tag = tdor_TypeRef -> MemberRefParent (mrp_TypeRef, tok)
    | (tag,tok) when tag = tdor_TypeSpec -> MemberRefParent (mrp_TypeSpec, tok)
    | _ -> failwith "GetTypeAsMemberRefParent"


(* -------------------------------------------------------------------- 
 * Native types
 * -------------------------------------------------------------------- *)

let rec GetVariantTypeAsInt32 ty = 
    if List.mem_assoc ty (Lazy.force variant_type_map) then 
        (List.assoc ty (Lazy.force variant_type_map ))
    else 
        match ty with 
        | VariantType_array  vt -> vt_ARRAY  ||| GetVariantTypeAsInt32  vt
        | VariantType_vector vt -> vt_VECTOR ||| GetVariantTypeAsInt32  vt
        | VariantType_byref  vt -> vt_BYREF  ||| GetVariantTypeAsInt32  vt
        | _ -> failwith "Unexpected variant type"

// based on information in ECMA and asmparse.y in the CLR codebase 
let rec GetNativeTypeAsBlobIdx cenv (ty:ILNativeType) = 
    GetBytesAsBlobIdx cenv (GetNativeTypeAsBytes ty)

and GetNativeTypeAsBytes ty = emit (fun bb -> EmitNativeType bb ty)

// REVIEW: write into an accumuating buffer
and EmitNativeType bb ty = 
    if List.mem_assoc ty (Lazy.force native_type_rmap) then 
        Bytebuf.emit_int_as_byte bb (List.assoc ty (Lazy.force native_type_rmap))
    else 
      match ty with 
      | NativeType_empty -> ()
      | NativeType_custom (guid,nativeTypeName,custMarshallerName,cookieString) ->
          let guid = Bytes.to_intarray guid
          let u1 = string_as_utf8_intarray nativeTypeName
          let u2 = string_as_utf8_intarray custMarshallerName
          let u3 = Bytes.to_intarray cookieString
          Bytebuf.emit_int_as_byte bb nt_CUSTOMMARSHALER; 
          emit_z_u32 bb (Array.length guid);
          Bytebuf.emit_intarray_as_bytes bb guid;
          emit_z_u32 bb (Array.length u1); Bytebuf.emit_intarray_as_bytes bb u1;
          emit_z_u32 bb (Array.length u2); Bytebuf.emit_intarray_as_bytes bb u2;
          emit_z_u32 bb (Array.length u3); Bytebuf.emit_intarray_as_bytes bb u3
      | NativeType_fixed_sysstring i -> 
          Bytebuf.emit_int_as_byte bb nt_FIXEDSYSSTRING; 
          emit_z_u32 bb i

      | NativeType_fixed_array i -> 
          Bytebuf.emit_int_as_byte bb nt_FIXEDARRAY;
          emit_z_u32 bb i
      | (* COM interop *) NativeType_safe_array (vt,name) -> 
          Bytebuf.emit_int_as_byte bb nt_SAFEARRAY;
          emit_z_u32 bb (GetVariantTypeAsInt32 vt);
          match name with 
          | None -> () 
          | Some n -> 
               let u1 = Bytes.to_intarray (Bytes.string_as_utf8_bytes_null_terminated n)
               emit_z_u32 bb (Array.length u1) ; Bytebuf.emit_intarray_as_bytes bb  u1
      | NativeType_array (nt,sizeinfo) ->  (* REVIEW: check if this corresponds to the ECMA spec *)
          Bytebuf.emit_int_as_byte bb nt_ARRAY; 
          match nt with 
          | None -> ()
          | Some ntt ->
             (if ntt = NativeType_empty then 
               emit_z_u32 bb nt_MAX 
              else 
                EmitNativeType bb ntt); 
             match sizeinfo with 
             | None -> ()  (* chunk out with zeroes because some crappy tools (e.g. asmmeta) read these poorly and expect further elements. *)
             | Some (pnum,additive) ->
                 (* ParamNum *) 
                 emit_z_u32 bb pnum;
               (* ElemMul *) (* z_u32 0x1l *) 
                 match additive with 
                 |  None -> ()
                 |  Some n ->  (* NumElem *) emit_z_u32 bb n
      | _ -> failwith "Unexpected native type"

(* -------------------------------------------------------------------- 
 * Native types
 * -------------------------------------------------------------------- *)

let rec GetFieldInitAsBlobIdx cenv env (ty:ILFieldInit) = 
    GetIntArrayAsBlobIdx cenv (GetFieldInitAsIntArray cenv env ty)

// REVIEW: write into an accumuating buffer
and GetFieldInitAsIntArray cenv env i = 
    match i with 
    | FieldInit_string b -> Bytes.to_intarray (Bytes.string_as_unicode_bytes b)
    | FieldInit_bool b ->  [| if b then 0x01 else 0x00 |]
    | FieldInit_char x -> u16_as_intarray x
    | FieldInit_int8 x -> i8_as_intarray x
    | FieldInit_int16 x -> i16_as_intarray x
    | FieldInit_int32 x -> i32_as_intarray x
    | FieldInit_int64 x -> i64_as_intarray x
    | FieldInit_uint8 x -> u8_as_intarray x
    | FieldInit_uint16 x -> u16_as_intarray x
    | FieldInit_uint32 x -> u32_as_intarray x
    | FieldInit_uint64 x -> u64_as_intarray x
    | FieldInit_single x -> ieee32_as_intarray x
    | FieldInit_double x -> ieee64_as_intarray x
    | FieldInit_ref  -> [| 0x00; 0x00; 0x00; 0x00;  |]

and GetFieldInitFlags i = 
    UShort 
      (uint16
        (match i with 
         | FieldInit_string _ -> et_STRING
         | FieldInit_bool _ -> et_BOOLEAN
         | FieldInit_char _ -> et_CHAR
         | FieldInit_int8 _ -> et_I1
         | FieldInit_int16 _ -> et_I2
         | FieldInit_int32 _ -> et_I4
         | FieldInit_int64 _ -> et_I8
         | FieldInit_uint8 _ -> et_U1
         | FieldInit_uint16 _ -> et_U2
         | FieldInit_uint32 _ -> et_U4
         | FieldInit_uint64 _ -> et_U8
         | FieldInit_single _ -> et_R4
         | FieldInit_double _ -> et_R8
         | FieldInit_ref -> et_CLASS))
                  
(* -------------------------------------------------------------------- 
 * Type definitions
 * -------------------------------------------------------------------- *)

let GetMemberAccessFlags access = 
    match access with 
    | MemAccess_compilercontrolled -> 0x00000000
    | MemAccess_public -> 0x00000006
    | MemAccess_private  -> 0x00000001
    | MemAccess_family  -> 0x00000004
    | MemAccess_famandassem -> 0x00000002
    | MemAccess_famorassem -> 0x00000005
    | MemAccess_assembly -> 0x00000003

let GetTypeAccessFlags  access = 
    match access with 
    | TypeAccess_public -> 0x00000001
    | TypeAccess_private  -> 0x00000000
    | TypeAccess_nested MemAccess_public -> 0x00000002
    | TypeAccess_nested MemAccess_private  -> 0x00000003
    | TypeAccess_nested MemAccess_family  -> 0x00000004
    | TypeAccess_nested MemAccess_famandassem -> 0x00000006
    | TypeAccess_nested MemAccess_famorassem -> 0x00000007
    | TypeAccess_nested MemAccess_assembly -> 0x00000005
    | TypeAccess_nested MemAccess_compilercontrolled -> failwith "bad type acccess"

let rec GetTypeDefAsRow cenv env enc (td:ILTypeDef) = 
    let nselem,nelem = name_as_elem_pair cenv td.Name
    let flags = 
      if (is_toplevel_tname td.Name) then 0x00000000
      else
        
        GetTypeAccessFlags td.Access |||
        begin 
          match td.Layout with 
          | TypeLayout_auto ->  0x00000000
          | TypeLayout_sequential _  -> 0x00000008
          | TypeLayout_explicit _ -> 0x00000010
        end |||
        begin 
          match td.tdKind with
          | TypeDef_interface -> 0x00000020
          | _ -> 0x00000000
        end |||
        (if td.IsAbstract then 0x00000080l else 0x00000000) |||
        (if td.IsSealed then 0x00000100l else 0x00000000) ||| 
        (if td.tdComInterop then 0x00001000l else 0x00000000)  |||
        (if td.IsSerializable then 0x00002000l else 0x00000000) |||
        begin 
          match td.Encoding with 
          | TypeEncoding_ansi -> 0x00000000
          | TypeEncoding_autochar -> 0x00020000
          | TypeEncoding_unicode ->  0x00010000
        end |||
        begin 
          match td.InitSemantics with
          |  TypeInit_beforefield when not (match td.tdKind with TypeDef_interface -> true | _ -> false) -> 0x00100000 
          | _ -> 0x00000000
        end |||
        (if td.tdSpecialName then 0x00000400 else 0x00000000) |||
          (* @REVIEW    (if rtspecialname_of_tdef td then 0x00000800l else 0x00000000) ||| *)
        (if td.tdHasSecurity or dest_security_decls td.tdSecurityDecls <> [] then 0x00040000 else 0x00000000)

    let tdor_tag, tdor_row = GetTypeOptionAsTypeDefOrRef cenv env td.Extends
    UnsharedRow 
       [| ULong flags ; 
          nelem; 
          nselem; 
          TypeDefOrRefOrSpec (tdor_tag, tdor_row); 
          SimpleIndex (tab_Field, cenv.fieldDefs.Count + 1); 
          SimpleIndex (tab_Method,cenv.methodDefIdxsByKey.Count + 1) |]  

and GetTypeOptionAsTypeDefOrRef cenv env ty_opt = 
    match ty_opt with
    | None -> (tdor_TypeDef, 0)
    | Some ty -> (GetTypeAsTypeDefOrRef cenv env ty)

and GetTypeDefAsPropertyMapRow cenv tidx = 
    UnsharedRow
        [| SimpleIndex (tab_TypeDef,  tidx);
           SimpleIndex (tab_Property, cenv.propertyDefs.Count + 1) |]  

and GetTypeDefAsEventMapRow cenv tidx = 
    UnsharedRow
        [| SimpleIndex (tab_TypeDef,  tidx);
           SimpleIndex (tab_Event, cenv.eventDefs.Count + 1) |]  
    
and GetKeyForFieldDef tidx fd = 
    FieldDefKey (tidx,fd.fdName, fd.fdType)

and GenFieldDefPass2 cenv tidx fd = 
    ignore (AddUniqueEntry "field" (fun (fdkey:FieldDefKey) -> fdkey.Name) cenv.fieldDefs (GetKeyForFieldDef tidx fd))

and GetKeyForMethodDef tidx md = 
    MethodDefKey (tidx,md.mdGenericParams.Length, md.mdName, md.Return.Type, md.ParameterTypes)

and GenMethodDefPass2 cenv tidx md = 
    let idx = 
      AddUniqueEntry "method" 
         (fun (key:MethodDefKey) -> 
           dprintn "Duplicate in method table is:";
           dprintn ("  Type index: "^string key.TypeIdx);
           dprintn ("  Method name: "^key.Name);
           dprintn ("  Method arity (num generic params): "^string key.GenericArity);
           key.Name
         )
         cenv.methodDefIdxsByKey 
         (GetKeyForMethodDef tidx md) 
    
    cenv.methodDefIdxs.[md] <- idx

and GetKeyForPropertyDef tidx x = 
    PropKey (tidx, x.propName, x.propType, x.propArgs)

and GenPropertyDefPass2 cenv tidx x = 
    ignore (AddUniqueEntry "property" (fun (PropKey (_,n,_,_)) -> n) cenv.propertyDefs (GetKeyForPropertyDef tidx x))

and GetTypeAsImplementsRow cenv env tidx ty =
    let tdor_tag,tdor_row = GetTypeAsTypeDefOrRef cenv env ty
    UnsharedRow 
        [| SimpleIndex (tab_TypeDef, tidx); 
           TypeDefOrRefOrSpec (tdor_tag,tdor_row) |]

and GenImplementsPass2 cenv env tidx ty =
    AddUnsharedRow cenv tab_InterfaceImpl (GetTypeAsImplementsRow cenv env tidx ty) |> ignore
      
and GetKeyForEvent tidx x = 
    EventKey (tidx, x.eventName)

and GenEventDefPass2 cenv tidx x = 
    ignore (AddUniqueEntry "event" (fun (EventKey(a,b)) -> b) cenv.eventDefs (GetKeyForEvent tidx x))

and GenTypeDefPass2 pidx enc cenv (td:ILTypeDef) =
   try 
      let env = env_enter_tdef (List.length td.tdGenericParams)
      let tidx = GetIdxForTypeDef cenv (TdKey(enc,td.Name))
      let tidx2 = AddUnsharedRow cenv tab_TypeDef (GetTypeDefAsRow cenv env enc td)
      if tidx <> tidx2 then failwith "index of typedef on second pass does not match index on first pass";

      // Add entries to auxiliary mapping tables, e.g. Nested, PropertyMap etc. 
      // Note Nested is organised differntly to the others... 
      if nonNil enc then 
          AddUnsharedRow cenv tab_Nested 
              (UnsharedRow 
                  [| SimpleIndex (tab_TypeDef, tidx); 
                     SimpleIndex (tab_TypeDef, pidx) |]) |> ignore;
      let props = dest_pdefs td.Properties
      if nonNil props then 
          AddUnsharedRow cenv tab_PropertyMap 
              (GetTypeDefAsPropertyMapRow cenv tidx) |> ignore; 
      let events = (dest_edefs td.Events)
      if nonNil events then 
          AddUnsharedRow cenv tab_EventMap 
              (GetTypeDefAsEventMapRow cenv tidx) |> ignore;

      // Now generate or assign index numbers for tables referenced by the maps. 
      // Don't yet generate contents of these tables - leave that to pass3, as 
      // code may need to embed these entries. 
      td.Implements |> List.iter (GenImplementsPass2 cenv env tidx);
      props |> List.iter (GenPropertyDefPass2 cenv tidx);
      events |> List.iter (GenEventDefPass2 cenv tidx);
      td.Fields |> dest_fdefs |> List.iter (GenFieldDefPass2 cenv tidx);
      td.Methods |> dest_mdefs |> List.iter (GenMethodDefPass2 cenv tidx);
      td.NestedTypes |> dest_tdefs |> GenTypeDefsPass2 tidx (enc@[td.Name]) cenv
   with e ->
     failwith ("Error in pass2 for type "^td.tdName^", error: "^e.Message);
     rethrow(); 
     raise e

and GenTypeDefsPass2 pidx enc cenv tds =
    List.iter (GenTypeDefPass2 pidx enc cenv) tds

(*=====================================================================
 * Pass 3 - write details of methods, fields, IL code, custom attrs etc.
 *=====================================================================*)

exception MethodDefNotFound
let FindMethodDefIdx cenv mdkey = 
    try GetTableEntry cenv.methodDefIdxsByKey mdkey
    with :? KeyNotFoundException -> 
      let tname_of_tidx i = 
        match 
           (cenv.typeDefs.dict 
             |> Seq.fold (fun  sofar kvp -> 
                let tkey2 = kvp.Key 
                let tidx2 = kvp.Value 
                if i = tidx2 then 
                    if sofar = None then 
                        Some tkey2 
                    else failwith "mutiple type names map to index" 
                else sofar)  None) with 
          | Some x -> x
          | None -> raise MethodDefNotFound 
      let (TdKey (tenc,tname)) = tname_of_tidx mdkey.TypeIdx
      dprintn ("The local method '"^(String.concat "." (tenc@[tname]))^"'::'"^mdkey.Name^"' was referenced but not declared");
      dprintn ("generic arity: "^string mdkey.GenericArity);
      //dprint "return type: "; Ilprint.output_typ stderr rty; dprintn "";
      //List.iter (fun ty -> dprint "arg type: "; Ilprint.output_typ stderr ty; dprintn "") argtys;
      cenv.methodDefIdxsByKey.dict |> Seq.iter (fun (KeyValue(mdkey2,_)) -> 
          if mdkey2.TypeIdx = mdkey.TypeIdx && mdkey.Name = mdkey2.Name then 
              let (TdKey (tenc2,tname2)) = tname_of_tidx mdkey2.TypeIdx
              dprintn ("A method in '"^(String.concat "." (tenc2@[tname2]))^"' had the right name but the wrong signature:");
              dprintn ("generic arity: "^string mdkey2.GenericArity)) ;
      raise MethodDefNotFound


let rec GetMethodDefIdx cenv md = 
    cenv.methodDefIdxs.[md]

and FindFieldDefIdx cenv fdkey = 
    try GetTableEntry cenv.fieldDefs fdkey 
    with :? KeyNotFoundException -> 
      failwith ("The local field "^fdkey.Name^" was referenced but not declared");
      1

and GetFieldDefAsFieldDefIdx cenv tidx fd = 
    FindFieldDefIdx cenv (GetKeyForFieldDef tidx fd) 

(* -------------------------------------------------------------------- 
 * ILMethodRef --> ILMethodDef.  
 * 
 * Only successfuly converts ILMethodRef's referring to 
 * methods in the module being emitted.
 * -------------------------------------------------------------------- *)

let GetMethodRefAsMethodDefIdx cenv (mref:ILMethodRef) =
    try 
        let tref = mref.EnclosingTypeRef
        if not (tref_is_local cenv tref) then
             failwithf "method referred to by method impl, event or property is not in a type defined in this module, method ref is %A" mref;
        let tidx = GetIdxForTypeDef cenv (TdKey(tref.Enclosing,tref.Name))
        let mdkey = MethodDefKey (tidx,mref.GenericArity, mref.Name, mref.ReturnType, mref.ArgTypes)
        FindMethodDefIdx cenv mdkey
    with e ->
        failwithf "Error in GetMethodRefAsMethodDefIdx for mref = %A, error: %s" mref.Name  e.Message;
        rethrow(); 
        raise e

let rec MethodRefInfoAsMemberRefRow cenv env fenv (nm,typ,callconv,args,ret,varargs,genarity) =
    MemberRefRow(GetTypeAsMemberRefParent cenv env typ,
                 GetStingHeapIdx cenv nm,
                 GetMethodRefInfoAsBlobIdx cenv fenv (callconv,args,ret,varargs,genarity))

and GetMethodRefInfoAsBlobIdx cenv env info = 
    GetBytesAsBlobIdx cenv (GetCallsigAsBytes cenv env info)

let GetMethodRefInfoAsMemberRefIdx cenv env  ((nm,typ,cc,args,ret,varargs,genarity) as minfo) = 
    let fenv = env_enter_msig (if is_array_ty typ then env.envClassFormals else List.length (inst_of_typ typ)) genarity
    FindOrAddRow cenv tab_MemberRef 
      (MethodRefInfoAsMemberRefRow cenv env fenv  minfo)

let GetMethodRefInfoAsMethodRefOrDef always_mdef cenv env ((nm,typ,cc,args,ret,varargs,genarity) as minfo) =
    if isNone varargs && (always_mdef or typ_is_local cenv typ) then
        if not (is_tref_typ typ) then failwith "GetMethodRefInfoAsMethodRefOrDef: unexpected local tref-typ";
        try (mdor_MethodDef, GetMethodRefAsMethodDefIdx cenv (mk_mref(tref_of_typ typ, cc, nm, genarity, args,ret)))
        with MethodDefNotFound -> (mdor_MemberRef, GetMethodRefInfoAsMemberRefIdx cenv env minfo)
    else (mdor_MemberRef, GetMethodRefInfoAsMemberRefIdx cenv env minfo)


// -------------------------------------------------------------------- 
// ILMethodSpec --> ILMethodRef/ILMethodDef/ILMethodSpec
// -------------------------------------------------------------------- 

let rec GetMethodSpecInfoAsMethodSpecIdx cenv env (nm,typ,cc,args,ret,varargs,minst) = 
    let mdor_tag,mdor_row = GetMethodRefInfoAsMethodRefOrDef false cenv env (nm,typ,cc,args,ret,varargs,List.length minst)
    let blob = 
        emit (fun bb -> 
            Bytebuf.emit_int_as_byte bb e_IMAGE_CEE_CS_CALLCONV_GENERICINST;
            emit_z_u32 bb (List.length minst);
            minst |> List.iter (EmitType cenv env bb))
    FindOrAddRow cenv tab_MethodSpec 
      (SimpleSharedRow 
          [| MethodDefOrRef (mdor_tag,mdor_row);
             Blob (GetBytesAsBlobIdx cenv blob) |])

and GetMethodDefOrRefAsUncodedToken (tag,idx) =
    let tab = 
        if tag = mdor_MethodDef then tab_Method
        elif tag = mdor_MemberRef then tab_MemberRef  
        else failwith "GetMethodDefOrRefAsUncodedToken"
    GetUncodedToken tab idx

and GetMethodSpecInfoAsUncodedToken cenv env ((_,_,_,_,_,_,minst) as minfo) =
    if minst <> [] then 
      GetUncodedToken tab_MethodSpec (GetMethodSpecInfoAsMethodSpecIdx cenv env minfo)
    else 
      GetMethodDefOrRefAsUncodedToken (GetMethodRefInfoAsMethodRefOrDef false cenv env (mrefinfo_of_mspecinfo minfo))

and GetMethodSpecAsUncodedToken cenv env mspec = 
    GetMethodSpecInfoAsUncodedToken cenv env (InfoOfMethodSpec mspec)

and mrefinfo_of_mspecinfo (nm,typ,cc,args,ret,varargs,minst) = 
    (nm,typ,cc,args,ret,varargs,List.length minst)

and GetMethodSpecAsMethodDefOrRef cenv env (mspec,varargs) =
    GetMethodRefInfoAsMethodRefOrDef false cenv env (mrefinfo_of_mspecinfo (InfoOfMethodSpec (mspec,varargs)))

and GetMethodSpecAsMethodDef cenv env (mspec,varargs) =
    GetMethodRefInfoAsMethodRefOrDef true cenv env (mrefinfo_of_mspecinfo (InfoOfMethodSpec (mspec,varargs)))

and InfoOfMethodSpec (mspec:ILMethodSpec,varargs) = 
      (mspec.Name,
       mspec.EnclosingType,
       mspec.CallingConv,
       mspec.FormalArgTypes,
       mspec.FormalReturnType,
       varargs,
       mspec.GenericArgs)

(* -------------------------------------------------------------------- 
 * Il.method_in_parent --> ILMethodRef/ILMethodDef
 * 
 * Used for MethodImpls.
 * -------------------------------------------------------------------- *)

let rec GetOverridesSpecAsMemberRefIdx cenv env ospec = 
    let fenv = env_enter_ospec ospec
    let row = 
        MethodRefInfoAsMemberRefRow cenv env fenv  
            (ospec.MethodRef.Name,
             ospec.EnclosingType,
             ospec.MethodRef.CallingConv,
             ospec.MethodRef.ArgTypes,
             ospec.MethodRef.ReturnType,
             None,
             ospec.MethodRef.GenericArity)
    FindOrAddRow cenv tab_MemberRef  row
     
and GetOverridesSpecAsMethodDefOrRef cenv env (ospec:OverridesSpec) =
    let typ = ospec.EnclosingType
    if typ_is_local cenv typ then 
        if not (is_tref_typ typ) then failwith "GetOverridesSpecAsMethodDefOrRef: unexpected local tref-typ"; 
        try (mdor_MethodDef, GetMethodRefAsMethodDefIdx cenv ospec.MethodRef)
        with MethodDefNotFound ->  (mdor_MemberRef, GetOverridesSpecAsMemberRefIdx cenv env ospec) 
    else 
        (mdor_MemberRef, GetOverridesSpecAsMemberRefIdx cenv env ospec) 

(* -------------------------------------------------------------------- 
 * ILMethodRef --> ILMethodRef/ILMethodDef
 * 
 * Used for Custom Attrs.
 * -------------------------------------------------------------------- *)

let rec GetMethodRefAsMemberRefIdx cenv env fenv (mref:ILMethodRef) = 
    let row = 
        MethodRefInfoAsMemberRefRow cenv env fenv 
            (mref.Name,
             mk_nongeneric_boxed_typ mref.EnclosingTypeRef,
             mref.CallingConv,
             mref.ArgTypes,
             mref.ReturnType,
             None,
             mref.GenericArity)
    FindOrAddRow cenv tab_MemberRef row

and GetMethodRefAsCustomAttribType cenv (mref:ILMethodRef) =
    let fenv = env_enter_mref mref
    let tref = mref.EnclosingTypeRef
    if tref_is_local cenv tref then
        try (cat_MethodDef, GetMethodRefAsMethodDefIdx cenv mref)
        with MethodDefNotFound -> (cat_MemberRef, GetMethodRefAsMemberRefIdx cenv fenv fenv mref)
    else
        (cat_MemberRef, GetMethodRefAsMemberRefIdx cenv fenv fenv mref)

(* -------------------------------------------------------------------- 
 * ILAttributes --> CustomAttribute rows
 * -------------------------------------------------------------------- *)

let rec GetCustomAttrDataAsBlobIdx cenv data = 
    if Bytes.length data = 0 then 0 else GetBytesAsBlobIdx cenv data

and GetCustomAttrRow cenv hca attr = 
    let cat = GetMethodRefAsCustomAttribType cenv attr.customMethod.MethodRef
    UnsharedRow 
        [| HasCustomAttribute (fst hca, snd hca);
           CustomAttributeType (fst cat, snd cat); 
           Blob (GetCustomAttrDataAsBlobIdx cenv attr.customData); |]  

and GenCustomAttrPass3 cenv hca attr = 
    AddUnsharedRow cenv tab_CustomAttribute (GetCustomAttrRow cenv hca attr) |> ignore

and GenCustomAttrsPass3 cenv hca attrs = 
    attrs |> dest_custom_attrs |> List.iter (GenCustomAttrPass3 cenv hca) 

// -------------------------------------------------------------------- 
// ILPermissionSet --> DeclSecurity rows
// -------------------------------------------------------------------- *)

let rec GetSecurityDeclRow cenv hds (PermissionSet (action, s)) = 
    let bytes = Bytes.to_intarray s
    UnsharedRow 
        [| UShort (uint16 (List.assoc action (Lazy.force secaction_map)));
           HasDeclSecurity (fst hds, snd hds);
           Blob (GetIntArrayAsBlobIdx cenv bytes); |]  

and GenSecurityDeclPass3 cenv hds attr = 
    AddUnsharedRow cenv tab_Permission (GetSecurityDeclRow cenv hds attr) |> ignore

and GenSecurityDeclsPass3 cenv hds attrs = 
    List.iter (GenSecurityDeclPass3 cenv hds) attrs 

// -------------------------------------------------------------------- 
// ILFieldSpec --> FieldRef  or ILFieldDef row
// -------------------------------------------------------------------- 

let rec GetFieldSpecAsMemberRefRow cenv env fenv (fspec:ILFieldSpec) = 
    MemberRefRow (GetTypeAsMemberRefParent cenv env fspec.EnclosingType,
                  GetStingHeapIdx cenv fspec.Name,
                  GetFieldSpecSigAsBlobIdx cenv fenv fspec)

and GetFieldSpecAsMemberRefIdx cenv env fspec = 
    let fenv = env_enter_fspec fspec
    FindOrAddRow cenv tab_MemberRef (GetFieldSpecAsMemberRefRow cenv env fenv fspec)

// REVIEW: write into an accumuating buffer
and EmitFieldSpecSig cenv env bb (fspec:ILFieldSpec) = 
    Bytebuf.emit_int_as_byte bb e_IMAGE_CEE_CS_CALLCONV_FIELD;
    EmitType cenv env bb fspec.FormalType

and GetFieldSpecSigAsBytes cenv env x = 
    emit (fun bb -> EmitFieldSpecSig cenv env bb x) 

and GetFieldSpecSigAsBlobIdx cenv env x = 
    GetBytesAsBlobIdx cenv (GetFieldSpecSigAsBytes cenv env x)

and GetFieldSpecAsFieldDefOrRef cenv env (fspec:ILFieldSpec) =
    let typ = fspec.EnclosingType
    if typ_is_local cenv typ then
        if not (is_tref_typ typ) then failwith "GetFieldSpecAsFieldDefOrRef: unexpected local tref-typ";
        let tref = tref_of_typ typ
        let tidx = GetIdxForTypeDef cenv (TdKey(tref.Enclosing,tref.Name))
        let fdkey = FieldDefKey (tidx,fspec.Name, fspec.FormalType)
        (true, FindFieldDefIdx cenv fdkey)
    else 
        (false, GetFieldSpecAsMemberRefIdx cenv env fspec)

and GetFieldDefOrRefAsUncodedToken (tag,idx) =
    let tab = if tag then tab_Field else tab_MemberRef
    GetUncodedToken tab idx

(* -------------------------------------------------------------------- 
 * Il.callsig --> StandAloneSig
 * -------------------------------------------------------------------- *)

let GetCallsigAsBlobIdx cenv env (callsig:ILCallingSignature,varargs) = 
    GetBytesAsBlobIdx cenv 
      (GetCallsigAsBytes cenv env (callsig.CallingConv,
                                      callsig.ArgTypes,
                                      callsig.ReturnType,varargs,0))
    
let GetCallsigAsStandAloneSigRow cenv env x = 
    SimpleSharedRow [| Blob (GetCallsigAsBlobIdx cenv env x) |]

let GetCallsigAsStandAloneSigIdx cenv env info = 
    FindOrAddRow cenv tab_StandAloneSig (GetCallsigAsStandAloneSigRow cenv env info)

(* -------------------------------------------------------------------- 
 * local signatures --> BlobHeap idx
 * -------------------------------------------------------------------- *)

let EmitLocalSig cenv env bb locals = 
    Bytebuf.emit_int_as_byte bb e_IMAGE_CEE_CS_CALLCONV_LOCAL_SIG;
    emit_z_u32 bb (List.length locals);
    locals |> List.iter (typ_of_local >> EmitType cenv env bb) 

let GetLocalSigAsBlobHeapIdx cenv env locals = 
    GetBytesAsBlobIdx cenv (emit (fun bb -> EmitLocalSig cenv env bb locals))

let GetLocalSigAsStandAloneSigIdx cenv env locals = 
    SimpleSharedRow [| Blob ( GetLocalSigAsBlobHeapIdx cenv env locals ); |]



type ExceptionClauseKind = 
  | FinallyClause 
  | FaultClause 
  | TypeFilterClause of int32 
  | FilterClause of int

type ExceptionClauseSpec = (int * int * int * int * ExceptionClauseKind)

module Codebuf = begin

    (* -------------------------------------------------------------------- 
     * Buffer to write results of emitting code into.  Also record:
     *   - branch sources (where fixups will occur)
     *   - possible branch destinations
     *   - locations of embedded handles into the string table
     *   - the exception table
     * -------------------------------------------------------------------- *)
    type t = 
        { code: Bytebuf.t; 
          /// (instruction; optional short form); start of instr in code buffer; code loc for the end of the instruction the fixup resides in ; where is the destination of the fixup 
          mutable reqd_brfixups: ((int * int option) * int * ILCodeLabel list) list; 
          avail_brfixups: Dictionary.t<ILCodeLabel, int> ;
          /// code loc to fixup in code buffer 
          mutable reqd_string_fixups_in_method: (int * int) list; 
          /// data for exception handling clauses 
          mutable seh: ExceptionClauseSpec list; 
          seqpoints: ResizeArray<PdbSequencePoint>;
        }

    let CreateCodeBuffer nm = 
        { seh = [];
          code= Bytebuf.create 200;
          reqd_brfixups=[];
          reqd_string_fixups_in_method=[];
          avail_brfixups = Dictionary.create 0;
          seqpoints = new ResizeArray<_>(10)
        }

    let emit_seh_clause codebuf seh = codebuf.seh <- seh :: codebuf.seh

    let emit_seqpoint cenv codebuf (m:ILSourceMarker)  = 
        if cenv.generate_pdb then 
          // table indexes are 1-based, document array indexes are 0-based 
          let doc = (FindOrAddSharedEntry cenv.documents m.Document) - 1  
          codebuf.seqpoints.Add 
            { Document=doc;
              Offset= Bytebuf.length codebuf.code;
              Line=m.Line;
              Column=m.Column;
              EndLine=m.EndLine;
              EndColumn=m.EndColumn; }
              
    let emit_byte codebuf x = Bytebuf.emit_int_as_byte codebuf.code x
    let emit_intarray_as_bytes codebuf x = Bytebuf.emit_intarray_as_bytes codebuf.code x
    let emit_u16 codebuf x = Bytebuf.emit_u16 codebuf.code x
    let emit_i32 codebuf x = Bytebuf.emit_i32 codebuf.code x
    let emit_i64 codebuf x = Bytebuf.emit_i64 codebuf.code x

    let emit_uncoded codebuf u = emit_i32 codebuf u

    let record_reqd_stringfixup codebuf stringidx = 
        codebuf.reqd_string_fixups_in_method <- (Bytebuf.length codebuf.code, stringidx) :: codebuf.reqd_string_fixups_in_method;
        // Write a special value in that we check later when applying the fixup 
        emit_i32 codebuf 0xdeadbeef

    let record_reqd_brfixups codebuf i tgs = 
        codebuf.reqd_brfixups <- (i, Bytebuf.length codebuf.code, tgs) :: codebuf.reqd_brfixups;
        (* Write a special value in that we check later when applying the fixup *)
        (* Value is 0x11 {deadbbbb}* where 11 is for the instruction and deadbbbb is for each target *)
        emit_byte codebuf 0x11; (* for the instruction *)
        (if fst i = i_switch then 
          emit_i32 codebuf ((List.length tgs)));
        List.iter (fun _ -> emit_i32 codebuf 0xdeadbbbb) tgs

    let record_reqd_brfixup codebuf i tg = record_reqd_brfixups codebuf i [tg]
    let record_avail_brfixup codebuf tg = 
        Dictionary.add codebuf.avail_brfixups tg (Bytebuf.length codebuf.code)

    (* -------------------------------------------------------------------- 
     * Applying branch fixups.  Use short versions of instructions
     * wherever possible.  Sadly we can only determine if we can use a short
     * version after we've layed out the code for all other instructions.  
     * This in turn means that using a short version may change 
     * the various offsets into the code.
     * -------------------------------------------------------------------- *)

    let binchop p (arr: 'a[]) = 
      let rec go n m =
        if n > m then not_found()
        else 
          let i = (n+m)/2
          let c = p arr.[i] in if c = 0 then i elif c < 0 then go n (i-1) else go (i+1) m
      go 0 (Array.length arr)

    let apply_brfixups 
          orig_code 
          orig_seh 
          orig_reqd_string_fixups 
          orig_avail_brfixups 
          orig_reqd_brfixups 
          orig_seqpoints
          orig_scopes = 
      let ordered_orig_reqd_brfixups = orig_reqd_brfixups |> List.sortBy (fun (_,fixuploc,_) -> fixuploc)

      let new_code = Bytebuf.create (Bytes.length orig_code)

      (* Copy over all the code, working out whether the branches will be short *)
      (* or long and adjusting the branch destinations.  Record an adjust function to adjust all the other *)
      (* gumpf that refers to fixed offsets in the code stream. *)
      let new_code, new_reqd_brfixups,adjuster = 
          let remaining_reqd_brfixups = ref ordered_orig_reqd_brfixups
          let orig_where = ref 0
          let new_where = ref 0
          let done_last = ref false
          let new_reqd_brfixups = ref []

          let adjustments = ref []

          while (!remaining_reqd_brfixups <> [] or not !done_last) do
            let doing_last = isNil !remaining_reqd_brfixups  
            let orig_start_of_nobranch_block = !orig_where
            let new_start_of_nobranch_block = !new_where

            if logging then dprintn ("move chunk, doing_last = "^(if doing_last then "true" else "false"));

            let orig_end_of_nobranch_block = 
              if doing_last then Bytes.length orig_code 
              else 
                let (_,orig_start_of_instr,_) = List.hd !remaining_reqd_brfixups
                orig_start_of_instr

            (* Copy over a chunk of non-branching code *)
            let nobranch_len = orig_end_of_nobranch_block - orig_start_of_nobranch_block
            Bytebuf.emit_bytes new_code (Bytes.sub orig_code  orig_start_of_nobranch_block nobranch_len);
              
            (* Record how to adjust addresses in this range, including the branch instruction *)
            (* we write below, or the end of the method if we're doing the last bblock *)
            adjustments := (orig_start_of_nobranch_block,orig_end_of_nobranch_block,new_start_of_nobranch_block) :: !adjustments;
           
            (* Increment locations to the branch instruction we're really interested in  *)
            orig_where := orig_end_of_nobranch_block;
            new_where := !new_where + nobranch_len;
              
            (* Now do the branch instruction.  Decide whether the fixup will be short or long in the new code *)
            if doing_last then done_last := true
            else begin
              let (i,orig_start_of_instr,tgs) = List.hd !remaining_reqd_brfixups
              remaining_reqd_brfixups := List.tl !remaining_reqd_brfixups;
              if Bytes.get orig_code orig_start_of_instr <> 0x11 then failwith "br fixup sanity check failed (1)";
              let i_length = if fst i = i_switch then 5 else 1
              orig_where := !orig_where + i_length;

              let orig_end_of_instr = orig_start_of_instr + i_length + 4 * List.length tgs
              let new_end_of_instr_if_small = !new_where + i_length + 1
              let new_end_of_instr_if_big = !new_where + i_length + 4 * List.length tgs
              
              let short = 
                match i,tgs with 
                | (_,Some i_short),[tg] 
                    when
                      begin 
                        // Use the original offsets to compute if the branch is small or large.  This is 
                        // a safe approximation because code only gets smaller. 
                        if not (Dictionary.mem orig_avail_brfixups tg) then 
                          dprintn ("branch target "^string_of_code_label tg^" not found in code");
                        let orig_dest = 
                            match Dictionary.tryfind orig_avail_brfixups tg with 
                            | Some(v) -> v
                            | None -> 666666
                        let orig_rel_offset = orig_dest - orig_end_of_instr
                        -128 <= orig_rel_offset && orig_rel_offset <= 127
                      end 
                  ->
                    Bytebuf.emit_int_as_byte new_code i_short;
                    true
                | (i_long,_),_ ->
                    Bytebuf.emit_int_as_byte new_code i_long;
                    (if i_long = i_switch then 
                      Bytebuf.emit_i32 new_code ((List.length tgs)));
                    false
              
              new_where := !new_where + i_length;
              if !new_where <> (Bytebuf.length new_code) then dprintn "mismatch between new_where and new_code";

              tgs |> List.iter
                (fun tg ->
                    let orig_fixuploc = !orig_where
                    CheckFixup32 orig_code orig_fixuploc 0xdeadbbbb;
                    
                    if short then 
                        new_reqd_brfixups := (!new_where, new_end_of_instr_if_small, tg, true) :: !new_reqd_brfixups;
                        Bytebuf.emit_int_as_byte new_code 0x98; (* sanity check *)
                        new_where := !new_where + 1;
                    else 
                        new_reqd_brfixups := (!new_where, new_end_of_instr_if_big, tg, false) :: !new_reqd_brfixups;
                        Bytebuf.emit_i32 new_code 0xf00dd00fl; (* sanity check *)
                        new_where := !new_where + 4;
                    if !new_where <> Bytebuf.length new_code then dprintn "mismatch between new_where and new_code";
                    orig_where := !orig_where + 4);
              
              if !orig_where <> orig_end_of_instr then dprintn "mismatch between orig_where and orig_end_of_instr";
            end;
          done;

          let adjuster  = 
            let arr = Array.of_list (List.rev !adjustments)
            fun addr -> 
              let i = 
                  try binchop (fun (a1,a2,_) -> if addr < a1 then -1 elif addr > a2 then 1 else 0) arr 
                  with 
                     :? KeyNotFoundException -> 
                         failwith ("adjuster: address "^string addr^" is out of range")
              let (orig_start_of_nobranch_block,orig_end_of_nobranch_block,new_start_of_nobranch_block) = arr.[i]
              addr - (orig_start_of_nobranch_block - new_start_of_nobranch_block) 

          Bytebuf.close new_code, 
          !new_reqd_brfixups, 
          adjuster

      (* Now adjust everything *)
      let new_avail_brfixups = 
          let tab = Dictionary.create 10
          Dictionary.iter (fun tglab orig_brdest -> Dictionary.add tab tglab (adjuster orig_brdest)) orig_avail_brfixups;
          tab
      let new_reqd_string_fixups = List.map (fun (orig_fixuploc,stok) -> adjuster orig_fixuploc,stok) orig_reqd_string_fixups
      let new_seqpoints = Array.map (fun (sp:PdbSequencePoint) -> {sp with Offset=adjuster sp.Offset}) orig_seqpoints
      let new_seh = 
          orig_seh |> List.map (fun (st1,sz1,st2,sz2,kind) ->
              (adjuster st1,(adjuster (st1 + sz1) - adjuster st1),
               adjuster st2,(adjuster (st2 + sz2) - adjuster st2),
               (match kind with 
               | FinallyClause | FaultClause | TypeFilterClause _ -> kind
               | FilterClause n -> FilterClause (adjuster n))))
            
      let new_scopes =
        let rec remap scope =
          {scope with StartOffset = adjuster scope.StartOffset;
                      EndOffset = adjuster scope.EndOffset;
                      Children = Array.map remap scope.Children }
        List.map remap orig_scopes
      
      (* Now apply the adjusted fixups in the new code *)
      new_reqd_brfixups |> List.iter (fun (new_fixuploc,end_of_instr,tg, small) ->
            if not (Dictionary.mem new_avail_brfixups tg) then 
              failwith ("target "^string_of_code_label tg^" not found in new fixups");
            try 
                let n = Dictionary.find new_avail_brfixups tg
                let rel_offset = (n - end_of_instr)
                if small then 
                    if Bytes.get new_code new_fixuploc <> 0x98 then failwith "br fixupsanity check failed";
                    Bytes.set new_code new_fixuploc (b0 rel_offset);
                else 
                    CheckFixup32 new_code new_fixuploc 0xf00dd00fl;
                    ApplyFixup32 new_code new_fixuploc rel_offset
            with Not_found -> ());

      new_code, new_reqd_string_fixups, new_seh, new_seqpoints, new_scopes


    (* -------------------------------------------------------------------- 
     * Structured residue of emitting instructions: SEH exception handling
     * and scopes for local variables.
     * -------------------------------------------------------------------- *)

    (* Emitting instructions generates a tree of seh specifications *)
    (* We then emit the exception handling specs separately. *)
    (* nb. ECMA spec says the SEH blocks must be returned inside-out *)
    type seh_tree = 
      | Tip 
      | Node of (ExceptionClauseSpec option * seh_tree list) list
        
    (* Emitting instructions also generates a tree of locals-in-use specifications *)
    (* i.e. scopes suitable for use to generate debugging info *)
    type scope = pdb_method_scope


    (* -------------------------------------------------------------------- 
     * Table of encodings for instructions without arguments, also indexes
     * for all instructions.
     * -------------------------------------------------------------------- *)

    let encoding_of_noarg_instr_table = Dictionary.create 300
    let _ = 
      List.iter 
        (fun (x,mk) -> Dictionary.add encoding_of_noarg_instr_table mk x)
        (noarg_instrs.Force())
    let encoding_of_noarg_instr si = Dictionary.find encoding_of_noarg_instr_table si
    (* let is_noarg_instr s = Dictionary.mem encoding_of_noarg_instr_table s *)


    (* -------------------------------------------------------------------- 
     * Emit instructions
     * -------------------------------------------------------------------- *)

    let emit_instr_code codebuf i = 
        if i > 0xFF then 
            assert (i >>> 8 = 0xFE); 
            emit_byte codebuf ( ((i >>> 8)  &&& 0xFF)); 
            emit_byte codebuf ( (i &&& 0xFF)); 
        else 
            emit_byte codebuf i

    let emit_typ_instr cenv codebuf env i ty = 
        emit_instr_code codebuf i; 
        emit_uncoded codebuf (tdor_as_uncoded (GetTypeAsTypeDefOrRef cenv env ty))

    let emit_mspecinfo_instr cenv codebuf env i mspecinfo = 
        emit_instr_code codebuf i; 
        emit_uncoded codebuf (GetMethodSpecInfoAsUncodedToken cenv env mspecinfo)

    let emit_mspec_instr cenv codebuf env i mspec = 
        emit_instr_code codebuf i; 
        emit_uncoded codebuf (GetMethodSpecAsUncodedToken cenv env mspec)

    let emit_fspec_instr cenv codebuf env i fspec = 
        emit_instr_code codebuf i; 
        emit_uncoded codebuf (GetFieldDefOrRefAsUncodedToken (GetFieldSpecAsFieldDefOrRef cenv env fspec))

    let emit_short_u16_instr codebuf (i_short,i) x = 
        let n = int32 x
        if n <= 255 then 
            emit_instr_code codebuf i_short; 
            emit_byte codebuf n;
        else 
            emit_instr_code codebuf i; 
            emit_u16 codebuf x;

    let emit_short_i32_instr codebuf (i_short,i) x = 
        if x >= (-128) && x <= 127 then 
            emit_instr_code codebuf i_short; 
            emit_byte codebuf ( (if x < 0x0 then x + 256 else x));
        else 
            emit_instr_code codebuf i; 
            emit_i32 codebuf x;

    let emit_tailness codebuf tl = 
        if tl = Tailcall && !Ilprint.emit_tailcalls then emit_instr_code codebuf i_tail

    let emit_after_tailcall codebuf tl =
        if tl = Tailcall then emit_instr_code codebuf i_ret

    let emit_volatility codebuf tl = 
        if tl = Volatile then emit_instr_code codebuf i_volatile

    let emit_constrained cenv codebuf env ty = 
        emit_instr_code codebuf i_constrained;
        emit_uncoded codebuf (tdor_as_uncoded (GetTypeAsTypeDefOrRef cenv env ty))

    let emit_alignment codebuf tl = 
        match tl with 
        | Aligned -> ()
        | Unaligned_1 -> emit_instr_code codebuf i_unaligned; emit_byte codebuf 0x1
        | Unaligned_2 -> emit_instr_code codebuf i_unaligned; emit_byte codebuf 0x2
        | Unaligned_4 -> emit_instr_code codebuf i_unaligned; emit_byte codebuf 0x4

    let rec EmitInstr cenv codebuf env instr =
        match instr with
        | si when is_noarg_instr si ->
             emit_instr_code codebuf (encoding_of_noarg_instr si)
        | I_brcmp (cmp,tg1,tg2)  -> 
            record_reqd_brfixup codebuf ((Lazy.force brcmp_map).[cmp], Some (Lazy.force brcmp_smap).[cmp]) tg1
        | I_br tg -> ()
        | I_seqpoint s ->   emit_seqpoint cenv codebuf s
        | I_leave tg -> record_reqd_brfixup codebuf (i_leave,Some i_leave_s) tg
        | I_call  (tl,mspec,varargs)      -> 
            emit_tailness codebuf tl;
            emit_mspec_instr cenv codebuf env i_call (mspec,varargs);
            emit_after_tailcall codebuf tl
        | I_callvirt      (tl,mspec,varargs)      -> 
            emit_tailness codebuf tl;
            emit_mspec_instr cenv codebuf env i_callvirt (mspec,varargs);
            emit_after_tailcall codebuf tl
        | I_callconstraint        (tl,ty,mspec,varargs)   -> 
            emit_tailness codebuf tl;
            emit_constrained cenv codebuf env ty;
            emit_mspec_instr cenv codebuf env i_callvirt (mspec,varargs);
            emit_after_tailcall codebuf tl
        | I_newobj        (mspec,varargs) -> 
            emit_mspec_instr cenv codebuf env i_newobj (mspec,varargs)
        | I_ldftn mspec   -> 
            emit_mspec_instr cenv codebuf env i_ldftn (mspec,None)
        | I_ldvirtftn     mspec   -> 
            emit_mspec_instr cenv codebuf env i_ldvirtftn (mspec,None)

        | I_calli (tl,callsig,varargs)    -> 
            emit_tailness codebuf tl;
            emit_instr_code codebuf i_calli; 
            emit_uncoded codebuf (GetUncodedToken tab_StandAloneSig (GetCallsigAsStandAloneSigIdx cenv env (callsig,varargs)));
            emit_after_tailcall codebuf tl

        | I_ldarg u16 ->  emit_short_u16_instr codebuf (i_ldarg_s,i_ldarg) u16 
        | I_starg u16 ->  emit_short_u16_instr codebuf (i_starg_s,i_starg) u16 
        | I_ldarga u16 ->  emit_short_u16_instr codebuf (i_ldarga_s,i_ldarga) u16 
        | I_ldloc u16 ->  emit_short_u16_instr codebuf (i_ldloc_s,i_ldloc) u16 
        | I_stloc u16 ->  emit_short_u16_instr codebuf (i_stloc_s,i_stloc) u16 
        | I_ldloca u16 ->  emit_short_u16_instr codebuf (i_ldloca_s,i_ldloca) u16 

        | I_cpblk (al,vol)        -> 
            emit_alignment codebuf al; 
            emit_volatility codebuf vol;
            emit_instr_code codebuf i_cpblk
        | I_initblk       (al,vol)        -> 
            emit_alignment codebuf al; 
            emit_volatility codebuf vol;
            emit_instr_code codebuf i_initblk

        | I_arith (AI_ldc (DT_I4, NUM_I4 x)) -> 
            emit_short_i32_instr codebuf (i_ldc_i4_s,i_ldc_i4) x
        | I_arith (AI_ldc (DT_I8, NUM_I8 x)) -> 
            emit_instr_code codebuf i_ldc_i8; 
            emit_i64 codebuf x;
        | I_arith (AI_ldc (dt, NUM_R4 x)) -> 
            emit_instr_code codebuf i_ldc_r4; 
            emit_i32 codebuf (bits_of_float32 x)
        | I_arith (AI_ldc (dt, NUM_R8 x)) -> 
            emit_instr_code codebuf i_ldc_r8; 
            emit_i64 codebuf (bits_of_float x)

        | I_ldind (al,vol,dt)     -> 
            emit_alignment codebuf al; 
            emit_volatility codebuf vol;
            emit_instr_code codebuf 
              (match dt with 
              | DT_I -> i_ldind_i
              | DT_I1  -> i_ldind_i1     
              | DT_I2  -> i_ldind_i2     
              | DT_I4  -> i_ldind_i4     
              | DT_U1  -> i_ldind_u1     
              | DT_U2  -> i_ldind_u2     
              | DT_U4  -> i_ldind_u4     
              | DT_I8  -> i_ldind_i8     
              | DT_R4  -> i_ldind_r4     
              | DT_R8  -> i_ldind_r8     
              | DT_REF  -> i_ldind_ref
              | _ -> failwith "ldind")

        | I_stelem dt     -> 
            emit_instr_code codebuf 
              (match dt with 
              | DT_I | DT_U -> i_stelem_i
              | DT_U1 | DT_I1  -> i_stelem_i1     
              | DT_I2 | DT_U2  -> i_stelem_i2     
              | DT_I4 | DT_U4  -> i_stelem_i4     
              | DT_I8 | DT_U8  -> i_stelem_i8     
              | DT_R4  -> i_stelem_r4     
              | DT_R8  -> i_stelem_r8     
              | DT_REF  -> i_stelem_ref
              | _ -> failwith "stelem")

        | I_ldelem dt     -> 
            emit_instr_code codebuf 
              (match dt with 
              | DT_I -> i_ldelem_i
              | DT_I1  -> i_ldelem_i1     
              | DT_I2  -> i_ldelem_i2     
              | DT_I4  -> i_ldelem_i4     
              | DT_I8  -> i_ldelem_i8     
              | DT_U1  -> i_ldelem_u1     
              | DT_U2  -> i_ldelem_u2     
              | DT_U4  -> i_ldelem_u4     
              | DT_R4  -> i_ldelem_r4     
              | DT_R8  -> i_ldelem_r8     
              | DT_REF  -> i_ldelem_ref
              | _ -> failwith "ldelem")

        | I_stind (al,vol,dt)     -> 
            emit_alignment codebuf al; 
            emit_volatility codebuf vol;
            emit_instr_code codebuf 
              (match dt with 
              | DT_U | DT_I -> i_stind_i
              | DT_U1 | DT_I1  -> i_stind_i1     
              | DT_U2 | DT_I2  -> i_stind_i2     
              | DT_U4 | DT_I4  -> i_stind_i4     
              | DT_U8 | DT_I8  -> i_stind_i8     
              | DT_R4  -> i_stind_r4     
              | DT_R8  -> i_stind_r8     
              | DT_REF  -> i_stind_ref
              | _ -> failwith "stelem")

        | I_switch (labs,dflt)    ->  record_reqd_brfixups codebuf (i_switch,None) labs

        | I_ldfld (al,vol,fspec)  -> 
            emit_alignment codebuf al; 
            emit_volatility codebuf vol;
            emit_fspec_instr cenv codebuf env i_ldfld fspec
        | I_ldflda        fspec   -> 
            emit_fspec_instr cenv codebuf env i_ldflda fspec
        | I_ldsfld        (vol,fspec)     -> 
            emit_volatility codebuf vol;
            emit_fspec_instr cenv codebuf env i_ldsfld fspec
        | I_ldsflda       fspec   -> 
            emit_fspec_instr cenv codebuf env i_ldsflda fspec
        | I_stfld (al,vol,fspec)  -> 
            emit_alignment codebuf al; 
            emit_volatility codebuf vol;
            emit_fspec_instr cenv codebuf env i_stfld fspec
        | I_stsfld        (vol,fspec)     -> 
            emit_volatility codebuf vol;
            emit_fspec_instr cenv codebuf env i_stsfld fspec

        | I_ldtoken  tok  -> 
            emit_instr_code codebuf i_ldtoken;
            emit_uncoded codebuf 
              (match tok with 
              | Token_type typ -> 
                  match GetTypeAsTypeDefOrRef cenv env typ with 
                  | (tag,idx) when tag = tdor_TypeDef -> GetUncodedToken tab_TypeDef idx
                  | (tag,idx) when tag = tdor_TypeRef -> GetUncodedToken tab_TypeRef idx
                  | (tag,idx) when tag = tdor_TypeSpec -> GetUncodedToken tab_TypeSpec idx
                  | _ -> failwith "?"
              | Token_method mspec ->
                  match GetMethodSpecAsMethodDefOrRef cenv env (mspec,None) with 
                  | (tag,idx) when tag = mdor_MethodDef -> GetUncodedToken tab_Method idx
                  | (tag,idx) when tag = mdor_MemberRef -> GetUncodedToken tab_MemberRef idx
                  | _ -> failwith "?"

              | Token_field fspec ->
                  match GetFieldSpecAsFieldDefOrRef cenv env fspec with 
                  | (tag,idx) when tag -> GetUncodedToken tab_Field idx
                  | (tag,idx)  -> GetUncodedToken tab_MemberRef idx)
        | I_ldstr s       -> 
            emit_instr_code codebuf i_ldstr;
            record_reqd_stringfixup codebuf (GetUserStringHeapIdx cenv s)

        | I_box  ty       -> emit_typ_instr cenv codebuf env i_box ty
        | I_unbox  ty     -> emit_typ_instr cenv codebuf env i_unbox ty
        | I_unbox_any  ty -> emit_typ_instr cenv codebuf env i_unbox_any ty 

        | I_newarr (shape,ty) -> 
            if (shape = Rank1ArrayShape) then   
              emit_typ_instr cenv codebuf env i_newarr ty
            else
              let rank = shape.Rank
              let args = Array.to_list (Array.create ( rank) (cenv.ilg.typ_int32))
              emit_mspecinfo_instr cenv codebuf env i_newobj (".ctor",mk_array_ty(ty,shape),ILCallingConv.Instance,args,Type_void,None,[])

        | I_stelem_any (shape,ty) -> 
            if (shape = Rank1ArrayShape) then   
              emit_typ_instr cenv codebuf env i_stelem_any ty  
            else 
              let rank = shape.Rank
              let args = Array.to_list (Array.create ( rank) (cenv.ilg.typ_int32)) @ [ty]
              emit_mspecinfo_instr cenv codebuf env i_call ("Set",mk_array_ty(ty,shape),ILCallingConv.Instance,args,Type_void,None,[])

        | I_ldelem_any (shape,ty) -> 
            if (shape = Rank1ArrayShape) then   
              emit_typ_instr cenv codebuf env i_ldelem_any ty  
            else 
              let rank = shape.Rank
              let args = Array.to_list (Array.create ( rank) (cenv.ilg.typ_int32))
              emit_mspecinfo_instr cenv codebuf env i_call ("Get",mk_array_ty(ty,shape),ILCallingConv.Instance,args,ty,None,[])

        | I_ldelema  (ro,shape,ty) -> 
            if (ro = ReadonlyAddress) then
              emit_instr_code codebuf i_readonly;
            if (shape = Rank1ArrayShape) then   
              emit_typ_instr cenv codebuf env i_ldelema ty
            else 
              let rank = shape.Rank
              let args = Array.to_list (Array.create ( rank) (cenv.ilg.typ_int32))
              emit_mspecinfo_instr cenv codebuf env i_call ("Address",mk_array_ty(ty,shape),ILCallingConv.Instance,args,Type_byref ty,None,[])

        | I_castclass  ty -> emit_typ_instr cenv codebuf env i_castclass ty
        | I_isinst  ty -> emit_typ_instr cenv codebuf env i_isinst ty
        | I_refanyval  ty -> emit_typ_instr cenv codebuf env i_refanyval ty
        | I_mkrefany  ty -> emit_typ_instr cenv codebuf env i_mkrefany ty
        | I_initobj  ty -> emit_typ_instr cenv codebuf env i_initobj ty
        | I_ldobj  (al,vol,ty) -> 
            emit_alignment codebuf al; 
            emit_volatility codebuf vol;
            emit_typ_instr cenv codebuf env i_ldobj ty
        | I_stobj  (al,vol,ty) -> 
            emit_alignment codebuf al; 
            emit_volatility codebuf vol;
            emit_typ_instr cenv codebuf env i_stobj ty
        | I_cpobj  ty -> emit_typ_instr cenv codebuf env i_cpobj ty
        | I_sizeof  ty -> emit_typ_instr cenv codebuf env i_sizeof ty
        | EI_ldlen_multi (n,m)    -> 
            emit_short_i32_instr codebuf (i_ldc_i4_s,i_ldc_i4) m;
            EmitInstr cenv codebuf env (mk_normal_call(mk_nongeneric_mspec_in_typ(cenv.ilg.typ_Array, ILCallingConv.Instance, "GetLength", [(cenv.ilg.typ_int32)], (cenv.ilg.typ_int32))))

      (* ILX: REVIEW erase me earlier *)
        | I_other e when Ilx.is_ilx_ext_instr e -> 
            match (Ilx.dest_ilx_ext_instr e) with 
            |  (Ilx.EI_ldftn_then_call (mr1,(tl,mr2,varargs)))    -> 
                EmitInstr cenv codebuf env (I_ldftn mr1);
                EmitInstr cenv codebuf env (I_call (tl,mr2,varargs))
            |  (Ilx.EI_ld_instance_ftn_then_newobj (mr1,_,(mr2,varargs))) -> 
                EmitInstr cenv codebuf env (I_ldftn mr1);
                EmitInstr cenv codebuf env (I_newobj (mr2,varargs))

            |  _ -> failwith "an ILX instruction cannot be emitted"
        |  _ -> failwith "an IL instruction cannot be emitted"


    let mk_scope_node cenv (localSigs: _[]) (a,b,ls,ch) = 
        if (isNil ls or not cenv.generate_pdb) then ch
        else
          [ { Children= Array.of_list ch;
              StartOffset=a;
              EndOffset=b;
              Locals=
                  Array.of_list
                    (List.map
                       (fun x -> { pdbVarName=x.localName;
                                   pdbVarSig= (try localSigs.[x.localNum] with _ -> failwith ("local variable index "^string x.localNum^"in debug info does not reference a valid local"));
                                   pdbVarAttributes= x.localNum } ) 
                       (List.filter (fun v -> v.localName <> "") ls)) } ]
            
    let rec EmitCode cenv localSigs codebuf env (susp,code) = 
        match code with 
        | TryBlock (c,seh) -> 
            commit_susp codebuf susp (unique_entry_of_code c);
            let try_start = Bytebuf.length codebuf.code
            let susp,child1,scope1 = EmitCode cenv localSigs codebuf env (None,c)
            commit_susp_no_dest codebuf susp;
            let try_finish = Bytebuf.length codebuf.code
            let exn_branches = 
                match seh with 
                | FaultBlock flt -> 
                    let handler_start = Bytebuf.length codebuf.code
                    let susp,child2,scope2 = EmitCode cenv localSigs codebuf env (None,flt)
                    commit_susp_no_dest codebuf susp;
                    let handler_finish = Bytebuf.length codebuf.code
                    [ Some (try_start,(try_finish - try_start),
                            handler_start,(handler_finish - handler_start),
                            FaultClause), 
                      [(child2,scope2)] ]
                      
                | FinallyBlock flt -> 
                    let handler_start = Bytebuf.length codebuf.code
                    let susp,child2,scope2 = EmitCode cenv localSigs codebuf env (None,flt)
                    commit_susp_no_dest codebuf susp;
                    let handler_finish = Bytebuf.length codebuf.code
                    [ Some (try_start,(try_finish - try_start),
                            handler_start,(handler_finish - handler_start),
                            FinallyClause),
                      [(child2,scope2)] ]
                      
                | FilterCatchBlock clauses -> 
                    List.mapi 
                      (fun i (flt,ctch) -> 
                        match flt with 
                        | TypeFilter typ ->
                            let handler_start = Bytebuf.length codebuf.code
                            let susp,child2,scope2 = EmitCode cenv localSigs codebuf env (None,ctch)
                            commit_susp_no_dest codebuf susp;
                            let handler_finish = Bytebuf.length codebuf.code
                            Some (try_start,(try_finish - try_start),
                                  handler_start,(handler_finish - handler_start),
                                  TypeFilterClause (tdor_as_uncoded (GetTypeAsTypeDefOrRef cenv env typ))),
                            [(child2,scope2)]
                        | CodeFilter fltcode -> 
                            
                            let filter_start = Bytebuf.length codebuf.code
                            let susp,child2,scope2 = EmitCode cenv localSigs codebuf env (None,fltcode)
                            commit_susp_no_dest codebuf susp;
                            let handler_start = Bytebuf.length codebuf.code
                            let susp,child3,scope3 = EmitCode cenv localSigs codebuf env (None,ctch)
                            commit_susp_no_dest codebuf susp;
                            let handler_finish = Bytebuf.length codebuf.code
                            
                            Some (try_start,
                                  (try_finish - try_start),
                                  handler_start,
                                  (handler_finish - handler_start),
                                  FilterClause filter_start),
                            [(child2,scope2); (child3,scope3)])
                      clauses
            (None,
             Node((None,[child1])::List.map (fun (a,b) -> (a,List.map fst b)) exn_branches), 
             scope1 @ List.concat ((List.collect (fun (a,b) -> List.map snd b) exn_branches)))

        | RestrictBlock _ | GroupBlock _ -> 
            // NOTE: ensure tailcalls for critical linear loop using standard continuation technique
            let rec EmitCodeLinear (susp,b) cont =
                match b with 
                | RestrictBlock (_,code2) -> 
                    EmitCodeLinear (susp,code2) cont
                | GroupBlock (locs,codes) -> 
                    let start = Bytebuf.length codebuf.code
                    
                    // Imperative collectors for the sub-blocks
                    let new_susp = ref susp
                    let childseh = ref []
                    let childscopes = ref []
                    // Push the results of collecting one sub-block into the reference cells
                    let collect (susp,seh,scopes) = 
                        new_susp := susp;
                        childseh := seh :: !childseh;
                        childscopes := scopes :: !childscopes
                    // Close the collection by generating the (susp,node,scope-node) triple
                    let close () = 
                        let fin = Bytebuf.length codebuf.code
                        (!new_susp, 
                         Node([(None,(List.rev !childseh))]), 
                         mk_scope_node cenv localSigs (start,fin,locs,List.concat (List.rev !childscopes)))

                    begin match codes with 
                    | [c] -> 
                        // EmitCodeLinear sequence of nested blocks
                        EmitCodeLinear (!new_susp,c) (fun results -> 
                            collect results;
                            cont (close()))

                    | codes -> 
                        // Multiple blocks: leave the linear sequence and process each seperately
                        codes |> List.iter (fun c -> collect (EmitCode cenv localSigs codebuf env (!new_susp,c)));
                        cont(close())
                    end
                | c -> 
                    // leave the linear sequence
                    cont (EmitCode cenv localSigs codebuf env (susp,c))

            // OK, process the linear sequence
            EmitCodeLinear (susp,code) (fun x -> x)

        | ILBasicBlock bb ->  
            // Leaf case: one basic block
            commit_susp codebuf susp bb.bblockLabel;
            record_avail_brfixup codebuf bb.bblockLabel;
            let instrs = bb.bblockInstrs
            for i = 0 to instrs.Length - 1 do
                EmitInstr cenv codebuf env instrs.[i];
            (fallthrough_of_bblock bb), Tip, []
            
    and br_to_susp codebuf dest = record_reqd_brfixup codebuf (i_br,Some i_br_s) dest
              
    and commit_susp codebuf susp lab = 
        match susp with 
        | Some dest when dest <> lab -> br_to_susp codebuf dest
        | _ -> ()

    and commit_susp_no_dest codebuf susp = 
        match susp with 
        | Some dest -> br_to_susp codebuf dest
        | _ -> ()
     
    (* Flatten the SEH tree *)
    let rec EmitExceptionHandlerTree codebuf seh_tree = 
        match seh_tree with 
        | Tip -> ()
        | Node clauses -> List.iter (EmitExceptionHandlerTree2 codebuf) clauses

    and EmitExceptionHandlerTree2 codebuf (x,childseh) = 
        List.iter (EmitExceptionHandlerTree codebuf) childseh; (* internal first *)
        match x with 
        | None -> () 
        | Some clause -> emit_seh_clause codebuf clause

    let EmitTopCode cenv localSigs env nm code = 
        if logging then dprintn ("nm = "^nm);
        let codebuf = CreateCodeBuffer nm
        let final_susp, seh_tree, orig_scopes = 
            EmitCode cenv localSigs codebuf env (Some (unique_entry_of_code code),code)
        (match final_susp with Some dest  -> br_to_susp codebuf dest | _ -> ());
        EmitExceptionHandlerTree codebuf seh_tree;
        let orig_code = Bytebuf.close codebuf.code
        let orig_seh = List.rev codebuf.seh
        let orig_reqd_string_fixups = codebuf.reqd_string_fixups_in_method
        let orig_avail_brfixups = codebuf.avail_brfixups
        let orig_reqd_brfixups = codebuf.reqd_brfixups
        let orig_seqpoints = codebuf.seqpoints |> ResizeArray.to_array
        if logging then 
            dprintn ("length orig_seh = "^string (List.length orig_seh));
            List.iter
              (fun (st1,sz1,st2,sz2,kind) -> 
                dprintn ("st1 = "^string st1);
                dprintn ("sz1 = "^string sz1);
                dprintn ("st2 = "^string st2);
                dprintn ("sz2 = "^string sz2);) orig_seh;

        let new_code, new_reqd_string_fixups, new_seh, new_seqpoints, new_scopes = 
            apply_brfixups orig_code orig_seh orig_reqd_string_fixups orig_avail_brfixups orig_reqd_brfixups orig_seqpoints orig_scopes

        if logging then 
            dprintn ("length new_seh = "^string (List.length new_seh));
            List.iter
              (fun (st1,sz1,st2,sz2,kind) -> 
                dprintn ("st1 = "^string st1);
                dprintn ("sz1 = "^string sz1);
                dprintn ("st2 = "^string st2);
                dprintn ("sz2 = "^string sz2);) new_seh;
        let rootscope = 
            { Children= Array.of_list new_scopes;
              StartOffset=0;
              EndOffset=Bytes.length new_code;
              Locals=[| |]; }

        (new_reqd_string_fixups,new_seh, new_code, new_seqpoints, rootscope)

end

(* -------------------------------------------------------------------- 
 * Il.ILMethodBody --> bytes
 * -------------------------------------------------------------------- *)
let GetFieldDefTypeAsBlobIdx cenv env ty = 
    let bytes = 
        emit (fun bb -> Bytebuf.emit_int_as_byte bb e_IMAGE_CEE_CS_CALLCONV_FIELD;
                        EmitType cenv env bb ty)
    GetBytesAsBlobIdx cenv bytes

let GenILMethodBody mname cenv env il =
    let localSigs = 
      if cenv.generate_pdb then 
        il.ilLocals |> Array.of_list |> Array.map (fun l -> 
            // Write a fake entry for the local signature headed by e_IMAGE_CEE_CS_CALLCONV_FIELD. This is referenced by the PDB file
            ignore (FindOrAddRow cenv tab_StandAloneSig (SimpleSharedRow [| Blob (GetFieldDefTypeAsBlobIdx cenv env l.localType) |]));
            // Now write the type
            GetTypeAsBytes cenv env l.localType) 
      else [| |]

    let reqd_string_fixups,seh,code,seqpoints, scopes = Codebuf.EmitTopCode cenv localSigs env mname il.ilCode
    let code_size = Bytes.length code
    let methbuf = Bytebuf.create (code_size * 3)
    // Do we use the tiny format? 
    if isNil il.ilLocals && il.ilMaxStack <= 8 && not il.ilZeroInit  && isNil seh && code_size < 64 then
        // Use Tiny format 
        let aligned_code_size = align 4 (code_size + 1)
        let code_padding =  (aligned_code_size - (code_size + 1))
        let reqd_string_fixups' = (1,reqd_string_fixups)
        Bytebuf.emit_int_as_byte methbuf ( (code_size <<< 2 ||| e_CorILMethod_TinyFormat));
        Bytebuf.emit_bytes methbuf code;
        emit_pad methbuf code_padding;
        (reqd_string_fixups', Bytebuf.close methbuf), seqpoints, scopes
    else
        // Use Fat format 
        let flags = 
            e_CorILMethod_FatFormat |||
            (if seh <> [] then e_CorILMethod_MoreSects else 0x0) ||| 
            (if il.ilZeroInit then e_CorILMethod_InitLocals else 0x0)

        let localToken = 
            if isNil il.ilLocals then 0x0 else 
            GetUncodedToken tab_StandAloneSig
              (FindOrAddRow cenv tab_StandAloneSig (GetLocalSigAsStandAloneSigIdx cenv env il.ilLocals))

        let aligned_code_size = align 0x4 code_size
        let code_padding =  (aligned_code_size - code_size)
        
        Bytebuf.emit_int_as_byte methbuf (b0 flags); 
        Bytebuf.emit_int_as_byte methbuf 0x30; (* last four bits record size of fat header in 4 byte chunks - this is always 12 bytes = 3 four word chunks *)
        Bytebuf.emit_u16 methbuf (uint16 il.ilMaxStack);
        Bytebuf.emit_i32 methbuf code_size;
        Bytebuf.emit_i32 methbuf localToken;
        Bytebuf.emit_bytes methbuf code;
        emit_pad methbuf code_padding;

        if nonNil seh then 
            (* Can we use the small exception handling table format? *)
            let small_size = (List.length seh * 12 + 4)
            let can_use_small = 
              small_size <= 0xFF &&
              List.forall
                (fun (st1,sz1,st2,sz2,_) -> 
                  st1 <= 0xFFFF && st2 <= 0xFFFF && sz1 <= 0xFF && sz2 <= 0xFF) seh
            
            let kind_as_i32 k = 
              match k with 
                FinallyClause -> e_COR_ILEXCEPTION_CLAUSE_FINALLY
              | FaultClause -> e_COR_ILEXCEPTION_CLAUSE_FAULT
              | FilterClause _ -> e_COR_ILEXCEPTION_CLAUSE_FILTER
              | TypeFilterClause _ -> e_COR_ILEXCEPTION_CLAUSE_EXCEPTION
            let kind_as_extra_i32 k = 
              match k with 
                FinallyClause |FaultClause -> 0x0
              | FilterClause i -> i
              | TypeFilterClause uncoded -> uncoded
            
            if can_use_small then     
                if logging then dprintn ("using small SEH format for method "^mname); 
                Bytebuf.emit_int_as_byte methbuf ( e_CorILMethod_Sect_EHTable);
                Bytebuf.emit_int_as_byte methbuf (b0 small_size); 
                Bytebuf.emit_int_as_byte methbuf 0x00; 
                Bytebuf.emit_int_as_byte methbuf 0x00;
                seh |> List.iter (fun (st1,sz1,st2,sz2,kind) -> 
                    let k32 = kind_as_i32 kind
                    Bytebuf.emit_i32_as_u16 methbuf k32; 
                    Bytebuf.emit_i32_as_u16 methbuf st1; 
                    Bytebuf.emit_int_as_byte methbuf (b0 sz1); 
                    Bytebuf.emit_i32_as_u16 methbuf st2; 
                    Bytebuf.emit_int_as_byte methbuf (b0 sz2);
                    Bytebuf.emit_i32 methbuf (kind_as_extra_i32 kind))
            else 
                if logging  then dprintn ("using fat SEH format for method "^mname); 
                let big_size = (List.length seh * 24 + 4)
                Bytebuf.emit_int_as_byte methbuf ( (e_CorILMethod_Sect_EHTable ||| e_CorILMethod_Sect_FatFormat));
                Bytebuf.emit_int_as_byte methbuf (b0 big_size);
                Bytebuf.emit_int_as_byte methbuf (b1 big_size);
                Bytebuf.emit_int_as_byte methbuf (b2 big_size);
                seh |> List.iter (fun (st1,sz1,st2,sz2,kind) -> 
                    let k32 = kind_as_i32 kind
                    Bytebuf.emit_i32 methbuf k32;
                    Bytebuf.emit_i32 methbuf st1;
                    Bytebuf.emit_i32 methbuf sz1;
                    Bytebuf.emit_i32 methbuf st2;
                    Bytebuf.emit_i32 methbuf sz2;
                    Bytebuf.emit_i32 methbuf (kind_as_extra_i32 kind))
        
        let reqd_string_fixups' = (12,reqd_string_fixups)

        (reqd_string_fixups', Bytebuf.close methbuf), seqpoints, scopes

// -------------------------------------------------------------------- 
// ILFieldDef --> FieldDef Row
// -------------------------------------------------------------------- 

let rec GetFieldDefAsFieldDefRow cenv env fd = 
    let flags = 
        GetMemberAccessFlags fd.fdAccess |||
        (if fd.fdStatic then 0x0010 else 0x0) |||
        (if fd.fdInitOnly then 0x0020 else 0x0) |||
        (if fd.fdLiteral then 0x0040 else 0x0) |||
        (if fd.fdNotSerialized then 0x0080 else 0x0) |||
        (if fd.fdSpecialName then 0x0200 else 0x0) |||
        (if fd.fdSpecialName then 0x0400 else 0x0) ||| (* REVIEW: RTSpecialName *)
        (if (fd.fdInit <> None) then 0x8000 else 0x0) |||
        (if (fd.fdMarshal <> None) then 0x1000 else 0x0) |||
        (if (fd.fdData <> None) then 0x0100 else 0x0)
    UnsharedRow 
        [| UShort (uint16 flags); 
           String (GetStingHeapIdx cenv fd.fdName);
           Blob ( GetFieldDefSigAsBlobIdx cenv env fd ); |]

and GetFieldDefSigAsBlobIdx cenv env fd = GetFieldDefTypeAsBlobIdx cenv env fd.fdType

and GenFieldDefPass3 cenv env fd = 
    let fidx = AddUnsharedRow cenv tab_Field (GetFieldDefAsFieldDefRow cenv env fd)
    GenCustomAttrsPass3 cenv (hca_FieldDef,fidx) fd.fdCustomAttrs;
    // Write FieldRVA table - fixups into data section done later 
    match fd.fdData with 
    | None -> () 
    | Some b -> 
        if logging then dprintn ("field data: size = "^string (Bytes.length b));
        let offs = Bytebuf.length cenv.data
        Bytebuf.emit_bytes cenv.data b;
        AddUnsharedRow cenv tab_FieldRVA 
            (UnsharedRow [| Data (offs, false); 
                                  SimpleIndex (tab_Field,fidx) |]) |> ignore
    // Write FieldMarshal table 
    match fd.fdMarshal with 
    | None -> ()
    | Some ntyp -> 
        AddUnsharedRow cenv tab_FieldMarshal 
              (UnsharedRow [| HasFieldMarshal (hfm_FieldDef, fidx);
                              Blob (GetNativeTypeAsBlobIdx cenv ntyp) |]) |> ignore
    // Write Contant table 
    match fd.fdInit with 
    | None -> ()
    | Some i -> 
        AddUnsharedRow cenv tab_Constant 
              (UnsharedRow 
                  [| GetFieldInitFlags i;
                     HasConstant (hc_FieldDef, fidx);
                     Blob (GetFieldInitAsBlobIdx cenv env i) |]) |> ignore
    // Write FieldLayout table 
    match fd.fdOffset with 
    | None -> ()
    | Some offset -> 
        AddUnsharedRow cenv tab_FieldLayout 
              (UnsharedRow [| ULong offset;
                                    SimpleIndex (tab_Field, fidx) |]) |> ignore

                
(* -------------------------------------------------------------------- 
 * Il.ILGenericParameterDef --> GenericParam Row
 * -------------------------------------------------------------------- *)

let rec GetGenericParamAsGenericParamRow cenv env idx owner gp = 
    let flags = 
        (match  gp.gpVariance with 
           | NonVariant -> 0x0000
           | CoVariant -> 0x0001
           | ContraVariant -> 0x0002) |||
        (if gp.gpReferenceTypeConstraint then 0x0004 else 0x0000) |||
        (if gp.gpNotNullableValueTypeConstraint then 0x0008 else 0x0000) |||
        (if gp.gpDefaultConstructorConstraint then 0x0010 else 0x0000)

    let mdVersionMajor,mdVersionMinor = metadataSchemaVersionSupportedByCLRVersion cenv.desiredMetadataVersion
    if (mdVersionMajor = 1) then 
        SimpleSharedRow 
            [| UShort (uint16 idx); 
               UShort (uint16 flags);   
               TypeOrMethodDef (fst owner, snd owner);
               String (GetStingHeapIdx cenv gp.gpName);
               TypeDefOrRefOrSpec (tdor_TypeDef, 0); (* empty kind field in deprecated metadata *) |]
    else
        SimpleSharedRow 
            [| UShort (uint16 idx); 
               UShort (uint16 flags);   
               TypeOrMethodDef (fst owner, snd owner);
               String (GetStingHeapIdx cenv gp.gpName) |]

and GenTypeAsGenericParamConstraintRow cenv env gpidx ty = 
    let tdor_tag,tdor_row = GetTypeAsTypeDefOrRef cenv env ty
    UnsharedRow 
        [| SimpleIndex (tab_GenericParam, gpidx);
           TypeDefOrRefOrSpec (tdor_tag,tdor_row) |]

and GenGenericParamConstraintPass4 cenv env gpidx ty =
    AddUnsharedRow cenv tab_GenericParamConstraint (GenTypeAsGenericParamConstraintRow cenv env gpidx ty) |> ignore

and GenGenericParamPass3 cenv env idx owner gp = 
    // shared since we look it up again below in GenGenericParamPass4
    AddSharedRow cenv tab_GenericParam (GetGenericParamAsGenericParamRow cenv env idx owner gp) |> ignore

and GenGenericParamPass4 cenv env idx owner gp = 
    let gpidx = FindOrAddRow cenv tab_GenericParam (GetGenericParamAsGenericParamRow cenv env idx owner gp)
    gp.gpConstraints |> List.iter (GenGenericParamConstraintPass4 cenv env gpidx) 

// -------------------------------------------------------------------- 
// Il.param and Il.return --> Param Row
// -------------------------------------------------------------------- 

let rec GetParamAsParamRow cenv env seq param = 
    let flags = 
        (if  param.paramIn then 0x0001 else 0x0000) |||
        (if  param.paramOut then 0x0002 else 0x0000) |||
        (if  param.paramOptional then 0x0010 else 0x0000) |||
        (if param.paramDefault <> None then 0x1000 else 0x0000) |||
        (if param.paramMarshal <> None then 0x2000 else 0x0000)
    
    UnsharedRow 
        [| UShort (uint16 flags); 
           UShort (uint16 seq); 
           String (GetStringHeapIdxOption cenv param.paramName) |]  

and GenParamPass3 cenv env seq param = 
    if param.paramIn=false && param.paramOut=false && param.paramOptional=false && isNone param.paramDefault && isNone param.paramName && isNone param.paramMarshal 
    then ()
    else    
      let pidx = AddUnsharedRow cenv tab_Param (GetParamAsParamRow cenv env seq param)
      GenCustomAttrsPass3 cenv (hca_ParamDef,pidx) param.paramCustomAttrs;
      (* Write FieldRVA table - fixups into data section done later *)
      match param.paramMarshal with 
      | None -> ()
      | Some ntyp -> 
          AddUnsharedRow cenv tab_FieldMarshal 
                (UnsharedRow [| HasFieldMarshal (hfm_ParamDef, pidx);
                                      Blob (GetNativeTypeAsBlobIdx cenv ntyp) |]) |> ignore

(*  paramDefault: ILFieldInit option;  (* -- Optional parameter *)*)

let GenReturnAsParamRow cenv env returnv = 
    let flags = (if returnv.returnMarshal <> None then 0x2000 else 0x0000)
    UnsharedRow 
        [| UShort (uint16 flags); 
           UShort 0us; (* sequence num. *)
           String 0 |]  

let GenReturnPass3 cenv env returnv = 
    if isSome returnv.returnMarshal || nonNil (dest_custom_attrs returnv.returnCustomAttrs) then
        let pidx = AddUnsharedRow cenv tab_Param (GenReturnAsParamRow cenv env returnv)
        GenCustomAttrsPass3 cenv (hca_ParamDef,pidx) returnv.returnCustomAttrs;
        match returnv.returnMarshal with 
        | None -> ()
        | Some ntyp -> 
            AddUnsharedRow cenv tab_FieldMarshal   
                (UnsharedRow 
                    [| HasFieldMarshal (hfm_ParamDef, pidx);
                       Blob (GetNativeTypeAsBlobIdx cenv ntyp) |]) |> ignore

// -------------------------------------------------------------------- 
// ILMethodDef --> ILMethodDef Row
// -------------------------------------------------------------------- 

let GetMethodDefSigAsBytes cenv env mdef = 
    emit (fun bb -> 
      Bytebuf.emit_int_as_byte bb (callconv_as_byte mdef.mdGenericParams.Length mdef.mdCallconv);
      if List.length mdef.mdGenericParams > 0 then emit_z_u32 bb mdef.mdGenericParams.Length;
      emit_z_u32 bb mdef.mdParams.Length;
      EmitType cenv env bb mdef.Return.Type;
      mdef.ParameterTypes |> List.iter (EmitType cenv env bb))

let GenMethodDefSigAsBlobIdx cenv env mdef = 
    GetBytesAsBlobIdx cenv (GetMethodDefSigAsBytes cenv env mdef)

let GenMethodDefAsRow cenv env midx md = 
    let flags = 
        GetMemberAccessFlags md.mdAccess |||
        (if (match md.mdKind with
              | MethodKind_static | MethodKind_cctor -> true
              | _ -> false) then 0x0010 else 0x0) |||
        (if (match md.mdKind with MethodKind_virtual vinfo -> vinfo.virtFinal | _ -> false) then 0x0020 else 0x0) |||
        (if (match md.mdKind with MethodKind_virtual _ -> true | _ -> false) then 0x0040 else 0x0) |||
        (if md.mdHideBySig then 0x0080 else 0x0) |||
        (if (match md.mdKind with MethodKind_virtual vinfo -> vinfo.virtStrict | _ -> false) then 0x0200 else 0x0) |||
        (if (match md.mdKind with MethodKind_virtual vinfo -> vinfo.virtNewslot | _ -> false) then 0x0100 else 0x0) |||
        (if (match md.mdKind with MethodKind_virtual vinfo -> vinfo.virtAbstract | _ -> false) then 0x0400 else 0x0) |||
        (if md.mdSpecialName then 0x0800 else 0x0) |||
        (if (match dest_mbody md.mdBody with MethodBody_pinvoke _ -> true | _ -> false) then 0x2000 else 0x0) |||
        (if md.mdUnmanagedExport then 0x0008 else 0x0) |||
        (if 
          (match md.mdKind with
          | MethodKind_ctor | MethodKind_cctor -> true 
          | _ -> false) then 0x1000 else 0x0) ||| (* RTSpecialName *)
        (if md.mdReqSecObj then 0x8000 else 0x0) |||
        (if md.mdHasSecurity or dest_security_decls (md.mdSecurityDecls) <> [] then 0x4000 else 0x0)
    let implflags = 
        (match  md.mdCodeKind with 
         | MethodCodeKind_native -> 0x0001
         | MethodCodeKind_runtime -> 0x0003
         | MethodCodeKind_il  -> 0x0000) |||
        (if md.mdInternalCall then 0x1000 else 0x0000) |||
        (if md.mdManaged then 0x0000 else 0x0004) |||
        (if md.mdForwardRef then 0x0010 else 0x0000) |||
        (if md.mdPreserveSig then 0x0080 else 0x0000) |||
        (if md.mdSynchronized then 0x0020 else 0x0000) |||
        (if md.mdMustRun then 0x0040 else 0x0000) |||
        (if (match dest_mbody md.mdBody with MethodBody_il il -> il.ilNoInlining | _ -> false) then 0x0008 else 0x0000)

    if md.mdEntrypoint then 
        if cenv.entrypoint <> None then failwith "duplicate entrypoint"
        else cenv.entrypoint <- Some (true, midx);
    let code_addr = 
      (match dest_mbody md.mdBody with 
      | MethodBody_il ilmbody -> 
          let addr = cenv.next_code_addr
          if logging then dprintn ("start of code = "^string addr); 
          let (code, seqpoints, rootscope) = GenILMethodBody md.mdName cenv env ilmbody

          (* Now record the PDB record for this method - we write this out later. *)
          if cenv.generate_pdb then 
            cenv.pdbinfo.Add  
              { MethToken=GetUncodedToken tab_Method midx;
                MethName=md.mdName;
                Params= [| |]; (* REVIEW *)
                RootScope = rootscope;
                Range=  
                  begin match ilmbody.ilSource with 
                  | Some m  when cenv.generate_pdb -> 
                      let doc = (FindOrAddSharedEntry cenv.documents m.Document) - 1 in  (* table indexes are 1-based, document array indexes are 0-based *)

                      Some ({ Document=doc;
                              Line=m.Line;
                              Column=m.Column; },
                            { Document=doc;
                              Line=m.EndLine;
                              Column=m.EndColumn; })
                  | _ -> None
                  end;
                SequencePoints=seqpoints; };
         
          add_code cenv code;
          addr 
      | MethodBody_native -> 
          failwith "cannot write body of native method - Abstract IL cannot roundtrip mixed native/managed binaries";
      | _  -> 0x0000)

    UnsharedRow 
       [| ULong  code_addr ; 
          UShort (uint16 implflags); 
          UShort (uint16 flags); 
          String (GetStingHeapIdx cenv md.mdName); 
          Blob (GenMethodDefSigAsBlobIdx cenv env md); 
          SimpleIndex(tab_Param,(table cenv tab_Param).Count + 1) |]  

let GenMethodImplPass3 cenv env tgparams tidx mimpl =
    let midx_tag, midx_row = GetMethodSpecAsMethodDef cenv env (mimpl.mimplOverrideBy,None)
    let midx2_tag, midx2_row = GetOverridesSpecAsMethodDefOrRef cenv env mimpl.mimplOverrides
    AddUnsharedRow cenv tab_MethodImpl
        (UnsharedRow 
             [| SimpleIndex (tab_TypeDef, tidx);
                MethodDefOrRef (midx_tag, midx_row);
                MethodDefOrRef (midx2_tag, midx2_row) |]) |> ignore
    
let GenMethodDefPass3 cenv env (md:ILMethodDef) = 
    let midx = GetMethodDefIdx cenv md
    let idx2 = AddUnsharedRow cenv tab_Method (GenMethodDefAsRow cenv env midx md)
    if midx <> idx2 then failwith "index of method def on pass 3 does not match index on pass 2";
    GenReturnPass3 cenv env md.mdReturn;  
    md.Parameters |> List.iteri (fun n param -> GenParamPass3 cenv env (n+1) param) ;
    md.CustomAttrs |> GenCustomAttrsPass3 cenv (hca_MethodDef,midx) ;
    md.SecurityDecls |> dest_security_decls |> GenSecurityDeclsPass3 cenv (hds_MethodDef,midx);
    md.GenericParams |> List.iteri (fun n gp -> GenGenericParamPass3 cenv env n (tomd_MethodDef, midx) gp) ;
    match dest_mbody md.mdBody with 
    | MethodBody_pinvoke attr ->
        let flags = 
          begin match attr.pinvokeCallconv with 
          | PInvokeCallConvNone ->     0x0000
          | PInvokeCallConvCdecl ->    0x0200
          | PInvokeCallConvStdcall ->  0x0300
          | PInvokeCallConvThiscall -> 0x0400
          | PInvokeCallConvFastcall -> 0x0500
          | PInvokeCallConvWinapi ->   0x0100
          end |||
          begin match attr.PInvokeCharEncoding with 
          | PInvokeEncodingNone ->    0x0000
          | PInvokeEncodingAnsi ->    0x0002
          | PInvokeEncodingUnicode -> 0x0004
          | PInvokeEncodingAuto ->    0x0006
          end |||
          begin match attr.PInvokeCharBestFit with 
          | PInvokeBestFitUseAssem -> 0x0000
          | PInvokeBestFitEnabled ->  0x0010
          | PInvokeBestFitDisabled -> 0x0020
          end |||
          begin match attr.PInvokeThrowOnUnmappableChar with 
          | PInvokeThrowOnUnmappableCharUseAssem -> 0x0000
          | PInvokeThrowOnUnmappableCharEnabled ->  0x1000
          | PInvokeThrowOnUnmappableCharDisabled -> 0x2000
          end |||
          (if attr.pinvokeNoMangle then 0x0001 else 0x0000) |||
          (if attr.pinvokeLastErr then 0x0040 else 0x0000)
        AddUnsharedRow cenv tab_ImplMap
            (UnsharedRow 
               [| UShort (uint16 flags); 
                  MemberForwarded (mf_MethodDef,midx);
                  String (GetStingHeapIdx cenv attr.pinvokeName); 
                  SimpleIndex (tab_ModuleRef, GetModuleRefAsIdx cenv attr.pinvokeWhere); |]) |> ignore
    | _ -> ()

let GenMethodDefPass4 cenv env  md = 
    let midx = GetMethodDefIdx cenv md
    List.iteri (fun n gp -> GenGenericParamPass4 cenv env n (tomd_MethodDef, midx) gp) md.mdGenericParams

(*       mdExport: (i32 * string option) option; REVIEW *)
(*      mdVtableEntry: (i32 * i32) option; REVIEW *)

let GenPropertyMethodSemanticsPass3 cenv pidx kind mref =
    // REVIEW: why are we catching exceptions here?
    let midx = try GetMethodRefAsMethodDefIdx cenv mref with MethodDefNotFound -> 1
    AddUnsharedRow cenv tab_MethodSemantics
        (UnsharedRow 
           [| UShort (uint16 kind);
              SimpleIndex (tab_Method,midx);
              HasSemantics (hs_Property, pidx) |]) |> ignore
    
let rec GetPropertySigAsBlobIdx cenv env prop = 
    GetBytesAsBlobIdx cenv (GetPropertySigAsBytes cenv env prop)

and GetPropertySigAsBytes cenv env prop = 
    emit (fun bb -> 
        let b =  ((hasthis_as_byte prop.propCallconv) ||| e_IMAGE_CEE_CS_CALLCONV_PROPERTY)
        Bytebuf.emit_int_as_byte bb b;
        emit_z_u32 bb (List.length prop.propArgs);
        EmitType cenv env bb prop.propType;
        prop.propArgs |> List.iter (EmitType cenv env bb))

and GetPropertyAsPropertyRow cenv env (prop:ILPropertyDef) = 
    let flags = 
      (if prop.propSpecialName then 0x0200 else 0x0) ||| 
      (if  prop.propRTSpecialName then 0x0400 else 0x0) ||| 
      (if prop.propInit <> None then 0x1000 else 0x0)
    UnsharedRow 
       [| UShort (uint16 flags); 
          String (GetStingHeapIdx cenv prop.Name); 
          Blob (GetPropertySigAsBlobIdx cenv env prop); |]  

/// ILPropertyDef --> Property Row + MethodSemantics entries
and GenPropertyPass3 cenv env prop = 
    let pidx = AddUnsharedRow cenv tab_Property (GetPropertyAsPropertyRow cenv env prop)
    prop.propSet |> Option.iter (GenPropertyMethodSemanticsPass3 cenv pidx 0x0001) ;
    prop.propGet |> Option.iter (GenPropertyMethodSemanticsPass3 cenv pidx 0x0002) ;
    (* Write Constant table *)
    match prop.propInit with 
    | None -> ()
    | Some i -> 
        AddUnsharedRow cenv tab_Constant 
            (UnsharedRow 
                [| GetFieldInitFlags i;
                   HasConstant (hc_Property, pidx);
                   Blob (GetFieldInitAsBlobIdx cenv env i) |]) |> ignore
    GenCustomAttrsPass3 cenv (hca_Property,pidx) prop.propCustomAttrs

let rec GenEventMethodSemanticsPass3 cenv eidx kind mref =
    let add_idx = try GetMethodRefAsMethodDefIdx cenv mref with MethodDefNotFound -> 1
    AddUnsharedRow cenv tab_MethodSemantics
        (UnsharedRow 
            [| UShort (uint16 kind);
               SimpleIndex (tab_Method,add_idx);
               HasSemantics (hs_Event, eidx) |]) |> ignore

/// Il.ILEventDef --> Event Row + MethodSemantics entries
and GenEventAsEventRow cenv env md = 
    let flags = 
      (if md.eventSpecialName then 0x0200 else 0x0) ||| 
      (if  md.eventRTSpecialName then 0x0400 else 0x0)
    let tdor_tag, tdor_row = GetTypeOptionAsTypeDefOrRef cenv env md.eventType
    UnsharedRow 
       [| UShort (uint16 flags); 
          String (GetStingHeapIdx cenv md.eventName); 
          TypeDefOrRefOrSpec (tdor_tag,tdor_row) |]

and GenEventPass3 cenv env md = 
    let eidx = AddUnsharedRow cenv tab_Event (GenEventAsEventRow cenv env md)
    md.eventAddOn |> GenEventMethodSemanticsPass3 cenv eidx 0x0008  (* AddMethod *);
    md.eventRemoveOn |> GenEventMethodSemanticsPass3 cenv eidx 0x0010  (* RemoveMethod *);
    Option.iter (GenEventMethodSemanticsPass3 cenv eidx 0x0020) md.eventFire  (* Fire *);
    List.iter (GenEventMethodSemanticsPass3 cenv eidx 0x0004 (* Other *)) md.eventOther;
    GenCustomAttrsPass3 cenv (hca_Event,eidx) md.eventCustomAttrs


(* -------------------------------------------------------------------- 
 * Il.resource --> generate ...
 * -------------------------------------------------------------------- *)

let rec GetResourceAsManifestResourceRow cenv r = 
    let data,impl = 
      match r.resourceWhere with
      | Resource_local bf ->
          let b = bf()
          (* Embedded managed resources must be word-aligned.  At least I think so - resource format is not specified in ECMA.  But some mscorlib resources appear to be non-aligned - I think it doesn't matter.. *)
          if logging then dprintn ("resource data: size = "^string (Bytes.length b));
          let offs = Bytebuf.length cenv.resources
          let aligned_offs =  (align 0x8 offs)
          let pad = aligned_offs - offs
          let resource_size = (Bytes.length b)
          if logging then dprintn ("resource pad: "^string pad);
          emit_pad cenv.resources pad;
          Bytebuf.emit_i32 cenv.resources resource_size;
          Bytebuf.emit_bytes cenv.resources b;
          Data (aligned_offs,true),  (i_File, 0) 
      | Resource_file (mref,offs) -> ULong offs, (i_File, GetModuleRefAsFileIdx cenv mref)
      | Resource_assembly aref -> ULong 0x0, (i_AssemblyRef, GetAssemblyRefAsIdx cenv aref)
    UnsharedRow 
       [| data; 
          ULong (match r.resourceAccess with Resource_public -> 0x01 | Resource_private -> 0x02);
          String (GetStingHeapIdx cenv r.resourceName);    
          Implementation (fst impl, snd impl); |]

and GenResourcePass3 cenv r = 
  let idx = AddUnsharedRow cenv tab_ManifestResource (GetResourceAsManifestResourceRow cenv r)
  GenCustomAttrsPass3 cenv (hca_ManifestResource,idx) r.resourceCustomAttrs

// -------------------------------------------------------------------- 
// ILTypeDef --> generate ILFieldDef, ILMethodDef, ILPropertyDef etc. rows
// -------------------------------------------------------------------- 

let rec GenTypeDefPass3 enc cenv (td:ILTypeDef) = 
   try
      let env = env_enter_tdef (List.length td.tdGenericParams)
      let tidx = GetIdxForTypeDef cenv (TdKey(enc,td.Name))
      td.Properties |> dest_pdefs |> List.iter (GenPropertyPass3 cenv env);
      td.Events |> dest_edefs |> List.iter (GenEventPass3 cenv env);
      td.Fields |> dest_fdefs |> List.iter (GenFieldDefPass3 cenv env);
      td.Methods |> dest_mdefs |> List.iter (GenMethodDefPass3 cenv env);
      td.MethodImpls |> dest_mimpls |> List.iter (GenMethodImplPass3 cenv env  td.tdGenericParams.Length tidx);
    (* ClassLayout entry if needed *)
      match td.Layout with 
      | TypeLayout_auto -> ()
      | TypeLayout_sequential layout | TypeLayout_explicit layout ->  
          if isSome layout.typePack || isSome layout.typeSize then 
            AddUnsharedRow cenv tab_ClassLayout
                (UnsharedRow 
                    [| UShort (match layout.typePack with None -> uint16 0x0 | Some p -> p);
                       ULong (match layout.typeSize with None -> 0x0 | Some p -> p);
                       SimpleIndex (tab_TypeDef, tidx) |]) |> ignore
                       
      td.tdSecurityDecls |> dest_security_decls |> GenSecurityDeclsPass3 cenv (hds_TypeDef,tidx);
      td.CustomAttrs |> GenCustomAttrsPass3 cenv (hca_TypeDef,tidx);
      td.GenericParams |> List.iteri (fun n gp -> GenGenericParamPass3 cenv env n (tomd_TypeDef,tidx) gp) ; 
      td.NestedTypes |> dest_tdefs |> GenTypeDefsPass3 (enc@[td.Name]) cenv;
   with e ->
      failwith  ("Error in pass3 for type "^td.tdName^", error: "^e.Message);
      rethrow(); 
      raise e

and GenTypeDefsPass3 enc cenv tds =
  List.iter (GenTypeDefPass3 enc cenv) tds

/// ILTypeDef --> generate generic params on ILMethodDef: ensures
/// GenericParam table is built sorted by owner.

let rec GenTypeDefPass4 enc cenv (td:ILTypeDef) = 
   try
       let env = env_enter_tdef (List.length td.tdGenericParams)
       let tidx = GetIdxForTypeDef cenv (TdKey(enc,td.Name))
       List.iter (GenMethodDefPass4 cenv env) (dest_mdefs td.Methods);
       List.iteri (fun n gp -> GenGenericParamPass4 cenv env n (tomd_TypeDef,tidx) gp) td.GenericParams; 
       GenTypeDefsPass4 (enc@[td.Name]) cenv (dest_tdefs td.NestedTypes);
   with e ->
       failwith ("Error in pass4 for type "^td.tdName^", error: "^e.Message);
       rethrow(); 
       raise e

and GenTypeDefsPass4 enc cenv tds =
    List.iter (GenTypeDefPass4 enc cenv) tds

(* -------------------------------------------------------------------- 
 * Il.ILExportedTypes --> ILExportedType table 
 * -------------------------------------------------------------------- *)

let rec GenNestedExportedTypePass3 cenv cidx ce = 
    let flags =  GetMemberAccessFlags ce.nestedExportedTypeAccess
    let nidx = 
      AddUnsharedRow cenv tab_ExportedType 
        (UnsharedRow 
            [| ULong flags ; 
               ULong 0x0;
               String (GetStingHeapIdx cenv ce.nestedExportedTypeName); 
               String 0; 
               Implementation (i_ExportedType, cidx) |])
    GenCustomAttrsPass3 cenv (hca_ExportedType,nidx) ce.nestedExportedTypeCustomAttrs;
    GenNestedExportedTypesPass3 cenv nidx ce.nestedExportedTypeNested

and GenNestedExportedTypesPass3 cenv nidx nce =
    nce |> dest_nested_exported_types |> List.iter (GenNestedExportedTypePass3 cenv nidx)

and exported_type_pass3 cenv ce = 
    let nselem,nelem = name_as_elem_pair cenv ce.exportedTypeName
    let flags =  GetTypeAccessFlags ce.exportedTypeAccess
    let flags = if ce.exportedTypeForwarder then 0x00200000 ||| flags else flags
    let impl = scoref_as_Implementation_elem cenv ce.exportedTypeScope
    let cidx = 
      AddUnsharedRow cenv tab_ExportedType 
        (UnsharedRow 
            [| ULong flags ; 
               ULong 0x0;
               nelem; 
               nselem; 
               Implementation (fst impl, snd impl); |])
    GenCustomAttrsPass3 cenv (hca_ExportedType,cidx) ce.exportedTypeCustomAttrs;
    GenNestedExportedTypesPass3 cenv cidx ce.exportedTypeNested

and exported_types_pass3 cenv ce = 
    List.iter (exported_type_pass3 cenv) (dest_exported_types ce);

(* -------------------------------------------------------------------- 
 * Il.manifest --> generate Assembly row
 * -------------------------------------------------------------------- *)

and GetManifsetAsAssemblyRow cenv m = 
    UnsharedRow 
        [|ULong m.manifestAuxModuleHashAlgorithm;
          UShort (match m.manifestVersion with None -> 0us | Some (x,y,z,w) -> x);
          UShort (match m.manifestVersion with None -> 0us | Some (x,y,z,w) -> y);
          UShort (match m.manifestVersion with None -> 0us | Some (x,y,z,w) -> z);
          UShort (match m.manifestVersion with None -> 0us | Some (x,y,z,w) -> w);
          ULong 
            ( begin match m.manifestLongevity with 
              | LongevityUnspecified -> 0x0000
              | LongevityLibrary -> 0x0002 
              | LongevityPlatformAppDomain -> 0x0004
              | LongevityPlatformProcess -> 0x0006
              | LongevityPlatformSystem -> 0x0008
              end |||
              (if m.manifestRetargetable then 0xff else 0x0) |||
              // Setting these causes peverify errors. Hence both ilread and ilwrite ignore them and refuse to set them.
              // Any debugging customattributes will automatically propagate
              // REVIEW: No longer appears to be the case...
              (if m.manifestJitTracking then 0x8000 else 0x0) |||
              (if m.manifestDisableJitOptimizations then 0x4000 else 0x0) |||
              (match m.manifestPublicKey with None -> 0x0000 | Some _ -> 0x0001) ||| 
              0x0000);
          (match m.manifestPublicKey with None -> Blob 0 | Some x -> Blob (GetBytesAsBlobIdx cenv x));
          String (GetStingHeapIdx cenv m.manifestName);
          (match m.manifestLocale with None -> String 0 | Some x -> String (GetStingHeapIdx cenv x)); |]

and GenManifestPass3 cenv m = 
    let aidx = AddUnsharedRow cenv tab_Assembly (GetManifsetAsAssemblyRow cenv m)
    GenSecurityDeclsPass3 cenv (hds_Assembly,aidx) (dest_security_decls m.manifestSecurityDecls);
    GenCustomAttrsPass3 cenv (hca_Assembly,aidx) m.manifestCustomAttrs;
    exported_types_pass3 cenv m.manifestExportedTypes;
    (* Record the entrypoint decl if needed. *)
    match m.manifestEntrypointElsewhere with
    | Some mref -> 
        if cenv.entrypoint <> None then failwith "duplicate entrypoint"
        else cenv.entrypoint <- Some (false, GetModuleRefAsIdx cenv mref);
    | None -> ()

and new_guid modul = 
    let n = absilWriteGetTimeStamp ()
    let m = (hash n)
    let m2 = (hash modul.modulName)
    [| b0 m; b1 m; b2 m; b3 m; b0 m2; b1 m2; b2 m2; b3 m2; 0xa7; 0x45; 0x03; 0x83; b0 n; b1 n; b2 n; b3 n |]

and GetModuleAsRow cenv modul = 
    UnsharedRow 
        [| UShort (uint16 0x0); 
           String (GetStingHeapIdx cenv modul.modulName); 
           Guid (GetGuidIdx cenv (new_guid modul)); 
           Guid 0; 
           Guid 0 |]


let row_elem_compare e1 e2 = 
    match e1,e2 with 
    | SimpleIndex (Table tab1,n1), SimpleIndex(Table tab2,n2) -> 
        let c1 = compare n1 n2
        if c1 <> 0 then c1 else compare tab1 tab2 
    | TypeDefOrRefOrSpec(TypeDefOrRefOrSpecTag tag1,n1),
        TypeDefOrRefOrSpec(TypeDefOrRefOrSpecTag tag2,n2)
    | TypeOrMethodDef(TypeOrMethodDefTag tag1,n1),
        TypeOrMethodDef(TypeOrMethodDefTag tag2,n2)
    | HasConstant (HasConstantTag tag1,n1),
        HasConstant (HasConstantTag tag2,n2) 
    | HasCustomAttribute (HasCustomAttributeTag tag1,n1),
        HasCustomAttribute (HasCustomAttributeTag tag2,n2) 
    | HasFieldMarshal (HasFieldMarshalTag tag1,n1),
        HasFieldMarshal (HasFieldMarshalTag tag2,n2) 
    | HasDeclSecurity (HasDeclSecurityTag tag1,n1),
        HasDeclSecurity (HasDeclSecurityTag tag2,n2)
    | MemberRefParent (MemberRefParentTag tag1,n1),
        MemberRefParent (MemberRefParentTag tag2,n2) 
    | HasSemantics (HasSemanticsTag tag1,n1),
        HasSemantics (HasSemanticsTag tag2,n2) 
    | MethodDefOrRef (MethodDefOrRefTag tag1,n1),
        MethodDefOrRef (MethodDefOrRefTag tag2,n2) 
    | MemberForwarded (MemberForwardedTag tag1,n1),
        MemberForwarded (MemberForwardedTag tag2,n2)
    | Implementation (ImplementationTag tag1,n1),
        Implementation (ImplementationTag tag2,n2)
    | CustomAttributeType (CustomAttributeTypeTag tag1,n1),
        CustomAttributeType (CustomAttributeTypeTag tag2,n2) 
    | (ResolutionScope (ResolutionScopeTag tag1,n1),
        ResolutionScope (ResolutionScopeTag tag2,n2)) -> 
          let c1 = compare n1 n2
          if c1 <> 0 then c1 else compare tag1 tag2  
    | ULong _,ULong _ 
    | UShort _, UShort _ 
    | Guid _,Guid _ 
    | Blob _, Blob _
    | String _, String _
    | Data _,Data _ -> failwith "should not have to sort tables on this element"
    | _ -> failwith "sorting on Column where two rows have different kinds of element in this Column" 

let SortRows tab rows = 
    if List.mem_assoc tab sorted_table_info then
        let rows = rows |> List.map (fun (row:IGenericRow) -> row.GetGenericRow())
        let col = List.assoc tab sorted_table_info
        rows 
           |> List.sortWith (fun r1 r2 -> row_elem_compare r1.[col] r2.[col]) 
           |> List.map (fun arr -> (SimpleSharedRow arr) :> IGenericRow)
    else 
        rows

let GenModule cenv modul = 
    let midx = AddUnsharedRow cenv tab_Module (GetModuleAsRow cenv modul)
    List.iter (GenResourcePass3 cenv) (dest_resources modul.modulResources); 
    let tds = dest_tdefs_with_toplevel_first cenv.ilg modul.modulTypeDefs
    reportTime "Module Generation Preparation";
    GenTypeDefsPass1 [] cenv tds;
    reportTime "Module Generation Pass 1";
    GenTypeDefsPass2 0 [] cenv tds;
    reportTime "Module Generation Pass 2";
    (match modul.modulManifest with None -> () | Some m -> GenManifestPass3 cenv m);
    GenTypeDefsPass3 [] cenv tds;
    reportTime "Module Generation Pass 3";
    GenCustomAttrsPass3 cenv (hca_Module,midx) modul.modulCustomAttrs;
    // GenericParam is the only sorted table indexed by Columns in other tables (GenericParamConstraint). 
    // Hence we need to sort it before we emit any entries in GenericParamConstraint. 
    // Note this mutates the rows in a table.  'SetRowsOfTable' clears 
    // the key --> index map since it is no longer valid 
    SetRowsOfTable cenv.tables.[tag_of_table tab_GenericParam] (SortRows tab_GenericParam (GetTableEntries (table cenv tab_GenericParam)));
    GenTypeDefsPass4 [] cenv tds;
    reportTime "Module Generation Pass 4"

let gen_il reqd_data_fixups (desiredMetadataVersion,generate_pdb,mscorlib)  (m : ILModuleDef) cil_addr =
    let is_dll = m.modulDLL
    if logging then dprintn ("cil_addr = "^string cil_addr);

    let cenv = 
        { mscorlib=mscorlib;
          ilg = mk_ILGlobals mscorlib None; (* assumes mscorlib is Scope_assembly _ ILScopeRef *)
          desiredMetadataVersion=desiredMetadataVersion;
          reqd_data_fixups= reqd_data_fixups;
          reqd_string_fixups = [];
          code_chunks=Bytebuf.create 40000;
          next_code_addr = cil_addr;
          data = Bytebuf.create 200;
          resources = Bytebuf.create 200;
          tables= Array.init 64 (fun i -> NewTable ("row table "^string i,System.Collections.Generic.EqualityComparer.Default));
          AssemblyRefs = NewTable("ILAssemblyRef",System.Collections.Generic.EqualityComparer.Default);
          documents=NewTable("pdbdocs",System.Collections.Generic.EqualityComparer.Default);
          trefCache=new Dictionary<_,_>(100);
          pdbinfo= new ResizeArray<_>(200);
          fieldDefs= NewTable("field defs",System.Collections.Generic.EqualityComparer.Default);
          methodDefIdxsByKey = NewTable("method defs",System.Collections.Generic.EqualityComparer.Default);
          // This uses reference identity on ILMethodDef objects
          methodDefIdxs = new Dictionary<_,_>(100, HashIdentity.Reference);
          propertyDefs = NewTable("property defs",System.Collections.Generic.EqualityComparer.Default);
          eventDefs = NewTable("event defs",System.Collections.Generic.EqualityComparer.Default);
          typeDefs = NewTable("type defs",System.Collections.Generic.EqualityComparer.Default);
          entrypoint=None;
          generate_pdb=generate_pdb;
          // These must use structural comparison since they are keyed by arrays
          guids=NewTable("guids",HashIdentity.Structural);
          blobs= NewTable("blobs",HashIdentity.Structural);
          strings= NewTable("strings",System.Collections.Generic.EqualityComparer.Default); 
          userStrings= NewTable("user strings",System.Collections.Generic.EqualityComparer.Default); }

    // Now the main compilation step 
    GenModule cenv  m;

    // Fetch out some of the results  
    let entryPointToken = 
        match cenv.entrypoint with 
        | Some (ep_here,tok) -> 
            if logging then dprintn ("ep idx is "^string tok);
            GetUncodedToken (if ep_here then tab_Method else tab_File) tok 
        | None -> 
            if not is_dll then dprintn "warning: no entrypoint specified in executable binary";
            0x0

    let pdb_data = 
        { EntryPoint= (if is_dll then None else Some entryPointToken);
          Documents = Array.of_list (GetTableEntries cenv.documents);
          Methods= cenv.pdbinfo |> ResizeArray.to_array }

    let tidx_for_nested_tdef (tds:ILTypeDef list, td:ILTypeDef) =
        let enc = tds |> List.map (fun td -> td.Name)
        GetIdxForTypeDef cenv (TdKey(enc, td.Name))

    let strings =     Array.map Bytes.string_as_utf8_bytes_null_terminated (Array.of_list (GetTableEntries cenv.strings))
    let userStrings = Array.of_list (GetTableEntries cenv.userStrings) |> Array.map Bytes.string_as_unicode_bytes
    let blobs =       Array.of_list (GetTableEntries cenv.blobs)
    let guids =       Array.of_list (GetTableEntries cenv.guids)
    let tables =      Array.map GetTableEntries cenv.tables
    let code =        get_code cenv
    (* turn idx tbls into token maps *)
    let mappings =
     { tdefMap = (fun t ->
        GetUncodedToken tab_TypeDef (tidx_for_nested_tdef t));
       fdefMap = (fun t fd ->
        let tidx = tidx_for_nested_tdef t
        GetUncodedToken tab_Field (GetFieldDefAsFieldDefIdx cenv tidx fd));
       mdefMap = (fun t md ->
        let tidx = tidx_for_nested_tdef t
        GetUncodedToken tab_Method (FindMethodDefIdx cenv (GetKeyForMethodDef tidx md)));
       propertyMap = (fun t pd ->
        let tidx = tidx_for_nested_tdef t
        GetUncodedToken tab_Property (GetTableEntry cenv.propertyDefs (GetKeyForPropertyDef tidx pd)));
       eventMap = (fun t ed ->
        let tidx = tidx_for_nested_tdef t
        GetUncodedToken tab_Event (GetTableEntry cenv.eventDefs (EventKey (tidx, ed.eventName)))) }
    reportTime "Finalize Module Generation Results";
    (* New return the results *)
    strings,
    userStrings,
    blobs,
    guids,
    tables,
    entryPointToken,
    code,
    cenv.reqd_string_fixups,
    Bytebuf.close cenv.data,
    Bytebuf.close cenv.resources,
    pdb_data,
    mappings


(*=====================================================================
 * TABLES+BLOBS --> PHYSICAL METADATA+BLOBS
 *=====================================================================*)

type chunk = 
    { size: int32; 
      addr: int32 }

let chunk sz next = ({addr=next; size=sz},next + sz) 
let nochunk next = ({addr= 0x0;size= 0x0; } ,next)

let count f arr = 
    Array.fold_left (fun x y -> x + f y) 0x0 arr 

let write_binary_il (generate_pdb,desiredMetadataVersion,mscorlib) modul cil_addr = 

    let is_dll = modul.modulDLL
    (* When we know the real RVAs of the data section we fixup the references for the FieldRVA table. *)
    (* These references are stored as offsets into the metadata we return from this function *)
    let reqd_data_fixups = ref []

    let next = cil_addr

    let strings,userStrings,blobs,guids,tables,entryPointToken,code,reqd_string_fixups,data,resources,pdb_data,mappings = 
      gen_il reqd_data_fixups (desiredMetadataVersion,generate_pdb,mscorlib) modul cil_addr

    reportTime "Generated Tables and Code";
    let table_size (Table idx) = List.length tables.[idx]

    (* Compute a minimum version if generics were present: we give warnings if *)
    (* the version is not sufficient to support the constructs being emitted *)
    let minVersion = 
      if table_size tab_GenericParam > 0 or
        table_size tab_MethodSpec > 0 or
        table_size tab_GenericParamConstraint > 0 
      then parse_version ("2.0.0.0")  (* Whidbey Minumum *)
      else parse_version ("1.0.3705.0")

    (* Entrypoint is coded as an uncoded token *)
    if logging then dprintn ("ep token is "^string entryPointToken);

   (* Now place the code *)  
    let code_size = (Bytes.length code)
    let aligned_code_size = align 0x4 code_size
    let codep,next = chunk code_size next
    let code_padding = Array.create ( (aligned_code_size - code_size)) 0x0
    let code_paddingp,next = chunk (Array.length code_padding) next

    if logging then dprintn ("codep.size = "^string codep.size);

   (* Now layout the chunks of metadata and IL *)  
    let metadata_header_startp,next = chunk 0x10 next

    if logging then dprintn ("metadata_header_startp.addr = "^string metadata_header_startp.addr);

    let num_streams = 0x05

    let (mdtable_version_major, mdtable_version_minor) = metadataSchemaVersionSupportedByCLRVersion desiredMetadataVersion

    let version = 
      let (a,b,c,d) = desiredMetadataVersion
      string_as_utf8_intarray (Printf.sprintf "v%d.%d.%d" a b c)


    let padded_version_length = align 0x4 (Array.length version)

    (* Most addresses after this point are measured from the MD root *)
    (* Switch to md-rooted addresses *)
    let next = metadata_header_startp.size
    let metadata_header_versionp,next = chunk padded_version_length next
    let metadata_header_endp,next = chunk 0x04 next
    let tables_stream_headerp,next = chunk (0x08 + (align 4 ((String.length "#~") + 0x01))) next
    let strings_stream_headerp,next = chunk (0x08 + (align 4 ((String.length "#Strings") + 0x01))) next
    let userStrings_stream_headerp,next = chunk (0x08 + (align 4 ((String.length "#US") + 0x01))) next
    let guids_stream_headerp,next = chunk (0x08 + (align 4 ((String.length "#GUID") + 0x01))) next
    let blobs_stream_headerp,next = chunk (0x08 + (align 4 ((String.length "#Blob") + 0x01))) next

    let tables_stream_start = next

    let strings_stream_unpadded_size = count (fun s -> (Bytes.length s)) strings + 1
    let strings_stream_padded_size = align 4 strings_stream_unpadded_size
    
    let userStrings_stream_unpadded_size = count (fun s -> let n = (Bytes.length s) + 1 in n + z_u32_size n) userStrings + 1
    let userStrings_stream_padded_size = align 4 userStrings_stream_unpadded_size
    
    let guids_stream_unpadded_size = (Array.length guids) * 0x10
    let guids_stream_padded_size = align 4 guids_stream_unpadded_size
    
    let blobs_stream_unpadded_size = count (fun blob -> let n = (Bytes.length blob) in n + z_u32_size n) blobs + 1
    let blobs_stream_padded_size = align 4 blobs_stream_unpadded_size

    let guids_big = guids_stream_padded_size >= 0x10000
    let strings_big = strings_stream_padded_size >= 0x10000
    let blobs_big = blobs_stream_padded_size >= 0x10000

    (* 64bit bitvector indicating which tables are in the metadata. *)
    let (valid1,valid2),_ = 
      Array.fold_left 
        (fun ((valid1,valid2) as valid,n) rows -> 
          let valid = 
            if isNil rows then valid else
            ( (if n < 32 then  valid1 ||| (1 <<< n     ) else valid1),
              (if n >= 32 then valid2 ||| (1 <<< (n-32)) else valid2) )
          (valid,n+1))
        ((0,0), 0)
        tables

    // 64bit bitvector indicating which tables are sorted. 
    // Constant - REVIEW: make symbolic! compute from sorted table info! 
    let sorted1 = 0x3301fa00
    let sorted2 = 
      // If there are any generic parameters in the binary we're emitting then mark that 
      // table as sorted, otherwise don't.  This maximizes the number of assemblies we emit 
      // which have an ECMA-v.1. compliant set of sorted tables. 
      (if table_size (tab_GenericParam) > 0 then 0x00000400 else 0x00000000) ||| 
      (if table_size (tab_GenericParamConstraint) > 0 then 0x00001000 else 0x00000000) ||| 
      0x00000200
    
    reportTime "Layout Header of Tables";

    if logging then dprintn ("building string address table...");

    let guid_address n =   (if n = 0 then 0 else (n - 1) * 0x10 + 0x01)

    let string_address_tab = 
      let tab = Array.create (Array.length strings + 1) 0
      let pos = ref 1
      for i = 1 to Array.length strings do
          tab.[i] <- !pos;
          let s = strings.[i - 1]
          pos := !pos + (Bytes.length s)
      tab
    let string_address n = 
      if n >= Array.length string_address_tab then failwith ("string index "^string n^" out of range");
      string_address_tab.[n]
    
    let userString_address_tab = 
      let tab = Array.create (Array.length userStrings + 1) 0
      let pos = ref 1
      for i = 1 to Array.length userStrings do
          tab.[i] <- !pos;
          let s = userStrings.[i - 1]
          let n = (Bytes.length s) + 1
          pos := !pos + n + z_u32_size n
      tab
    let userString_address n = 
      if n >= Array.length userString_address_tab then failwith "userString index out of range";
      userString_address_tab.[n]
    
    let blob_address_tab = 
      let tab = Array.create (Array.length blobs + 1) 0
      let pos = ref 1
      for i = 1 to Array.length blobs do
          tab.[i] <- !pos;
          let blob = blobs.[i - 1]
          pos := !pos + (Bytes.length blob) + z_u32_size (Bytes.length blob)
      tab
    let blob_address n = 
      if n >= Array.length blob_address_tab then failwith "blob index out of range";
      blob_address_tab.[n]
    
    reportTime "Build String/Blob Address Tables";

    if logging then dprintn ("done building string/blob address table...");

    
    if logging then dprintn ("sorting tables...");

    let sortedTables = 
      Array.init 64 (fun i -> tables.[i] |>  SortRows (Table i) |> Array.of_list)
      
    reportTime "Sort Tables";

    if logging then dprintn ("encoding tables...");

    let coded_tables = 
          
        let isTableBig rows = Array.length rows >= 0x10000
        let bigness_tab = Array.map isTableBig sortedTables
        let bigness (Table idx) = bigness_tab.[idx]
        
        let coded_bigness nbits tab =
          (table_size tab) >= (0x10000 >>> nbits)
        
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
            coded_bigness 5 tab_ManifestResource  ||
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
        
        let tablesbuf =  Bytebuf.create 20000
        
    (* Now the coded tables themselves  - first the schemata header *)
        Bytebuf.emit_intarray_as_bytes tablesbuf    
            [| 0x00; 0x00; 0x00; 0x00; 
               mdtable_version_major; (* major version of table schemata *)
               mdtable_version_minor; (* minor version of table schemata *)
               
                ((if strings_big then 0x01 else 0x00) |||  (* bit vector for heap sizes *)
                 (if guids_big then 0x02 else 0x00) |||  (* bit vector for heap sizes *)
                 (if blobs_big then 0x04 else 0x00));
               0x01; (* reserved, always 1 *) |];
 
        Bytebuf.emit_i32 tablesbuf valid1;
        Bytebuf.emit_i32 tablesbuf valid2;
        Bytebuf.emit_i32 tablesbuf sorted1;
        Bytebuf.emit_i32 tablesbuf sorted2;
        
        // Numbers of rows in various tables 
        for rows in sortedTables do 
            if rows.Length <> 0 then 
                Bytebuf.emit_i32 tablesbuf rows.Length 
        
        
        let start_of_tables = Bytebuf.length tablesbuf
        reportTime "Write Header of tablebuf";

      (* The tables themselves *)
        for rows in sortedTables do
            for row in rows do 
                let row = row.GetGenericRow()
                for x in row do 
                    // Emit the coded token for the array element 
                    match x with 
                    | ULong n -> Bytebuf.emit_i32 tablesbuf n
                    | UShort n -> Bytebuf.emit_u16 tablesbuf n
                    | Guid n -> bytebuf_emit_z_untagged_index tablesbuf guids_big (guid_address n)
                    | Blob n -> bytebuf_emit_z_untagged_index tablesbuf blobs_big  (blob_address n)
                    | Data (offset,kind) -> record_reqd_data_fixup reqd_data_fixups tablesbuf (tables_stream_start + (Bytebuf.length tablesbuf)) (offset, kind)
                    | String n -> bytebuf_emit_z_untagged_index tablesbuf strings_big (string_address n)
                    | SimpleIndex (tab,n) -> bytebuf_emit_z_untagged_index tablesbuf (bigness tab) n
                    | TypeDefOrRefOrSpec(TypeDefOrRefOrSpecTag tag,n) ->  
                        bytebuf_emit_z_tagged_index tablesbuf tag 2 tdor_bigness n
                    | TypeOrMethodDef(TypeOrMethodDefTag tag,n) ->  
                        bytebuf_emit_z_tagged_index tablesbuf tag 1 tomd_bigness n
                    | HasConstant (HasConstantTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 2 hc_bigness n
                    | HasCustomAttribute (HasCustomAttributeTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 5 hca_bigness n
                    | HasFieldMarshal (HasFieldMarshalTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 1   hfm_bigness n
                    | HasDeclSecurity (HasDeclSecurityTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 2  hds_bigness n
                    | MemberRefParent (MemberRefParentTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 3  mrp_bigness n 
                    | HasSemantics (HasSemanticsTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 1  hs_bigness n 
                    | MethodDefOrRef (MethodDefOrRefTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 1  mdor_bigness n
                    | MemberForwarded (MemberForwardedTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 1  mf_bigness n
                    | Implementation (ImplementationTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 2  i_bigness n
                    | CustomAttributeType (CustomAttributeTypeTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag  3  cat_bigness n
                    | ResolutionScope (ResolutionScopeTag tag,n) -> 
                        bytebuf_emit_z_tagged_index tablesbuf tag 2  rs_bigness n
        Bytebuf.close tablesbuf

    reportTime "Write Tables to tablebuf";

    if logging then dprintn ("laying out final metadata...");
    
    let tables_stream_unpadded_size = (Bytes.length coded_tables)
    (* QUERY: extra 4 empty bytes in array.exe - why? Include some extra padding after the tables just in case there is a mistake in the ECMA spec. *)
    let tables_stream_padded_size = align 4 (tables_stream_unpadded_size + 4)
    let tables_streamp,next = chunk tables_stream_padded_size next
    let tables_stream_padding = tables_streamp.size - tables_stream_unpadded_size

    let strings_streamp,next = chunk strings_stream_padded_size next
    let strings_stream_padding = strings_streamp.size - strings_stream_unpadded_size
    let userStrings_streamp,next = chunk userStrings_stream_padded_size next
    let userStrings_stream_padding = userStrings_streamp.size - userStrings_stream_unpadded_size
    let guids_streamp,next = chunk (0x10 * guids.Length) next
    let blobs_streamp,next = chunk blobs_stream_padded_size next
    let blobs_stream_padding = blobs_streamp.size - blobs_stream_unpadded_size
    
    reportTime "Layout Metadata";

    if logging then dprintn ("producing final metadata...");
    let metadata = 
      let mdbuf =  Bytebuf.create 500000 
      Bytebuf.emit_intarray_as_bytes mdbuf 
        [| 0x42; 0x53; 0x4a; 0x42; (* Magic signature *)
           0x01; 0x00; (* Major version *)
           0x01; 0x00; (* Minor version *)
        |];
      Bytebuf.emit_i32 mdbuf 0x0; (* Reservered *)

      Bytebuf.emit_i32 mdbuf padded_version_length;
      Bytebuf.emit_intarray_as_bytes mdbuf version;
      for i = 1 to ( padded_version_length - Array.length version) do 
          Bytebuf.emit_int_as_byte mdbuf 0x00;

      Bytebuf.emit_intarray_as_bytes mdbuf 
        [| 0x00; 0x00; (* flags, reserved *)
          b0 num_streams; b1 num_streams; |];
      Bytebuf.emit_i32 mdbuf tables_streamp.addr;
      Bytebuf.emit_i32 mdbuf tables_streamp.size;
      Bytebuf.emit_intarray_as_bytes mdbuf [| 0x23; 0x7e; 0x00; 0x00; (* #~00 *)|];
      Bytebuf.emit_i32 mdbuf strings_streamp.addr;
      Bytebuf.emit_i32 mdbuf strings_streamp.size;
      Bytebuf.emit_intarray_as_bytes mdbuf  [| 0x23; 0x53; 0x74; 0x72; 0x69; 0x6e; 0x67; 0x73; 0x00; 0x00; 0x00; 0x00 (* "#Strings0000" *)|];
      Bytebuf.emit_i32 mdbuf userStrings_streamp.addr;
      Bytebuf.emit_i32 mdbuf userStrings_streamp.size;
      Bytebuf.emit_intarray_as_bytes mdbuf [| 0x23; 0x55; 0x53; 0x00; (* #US0*) |];
      Bytebuf.emit_i32 mdbuf guids_streamp.addr;
      Bytebuf.emit_i32 mdbuf guids_streamp.size;
      Bytebuf.emit_intarray_as_bytes mdbuf [| 0x23; 0x47; 0x55; 0x49; 0x44; 0x00; 0x00; 0x00; (* #GUID000 *)|];
      Bytebuf.emit_i32 mdbuf blobs_streamp.addr;
      Bytebuf.emit_i32 mdbuf blobs_streamp.size;
      Bytebuf.emit_intarray_as_bytes mdbuf [| 0x23; 0x42; 0x6c; 0x6f; 0x62; 0x00; 0x00; 0x00; (* #Blob000 *)|];
      
      reportTime "Write Metadata Header";
     (* Now the coded tables themselves *)
      Bytebuf.emit_bytes mdbuf coded_tables;    
      for i = 1 to tables_stream_padding do 
          Bytebuf.emit_int_as_byte mdbuf 0x00;
      reportTime "Write Metadata Tables";

     (* The string stream *)
      Bytebuf.emit_intarray_as_bytes mdbuf [| 0x00 |];
      for s in strings do
          Bytebuf.emit_bytes mdbuf s;
      for i = 1 to strings_stream_padding do 
          Bytebuf.emit_int_as_byte mdbuf 0x00;
      reportTime "Write Metadata Strings";
     (* The user string stream *)
      Bytebuf.emit_intarray_as_bytes mdbuf [| 0x00 |];
      for s in userStrings do
          emit_z_u32 mdbuf (s.Length + 1);
          Bytebuf.emit_bytes mdbuf s;
          Bytebuf.emit_int_as_byte mdbuf (marker_for_unicode_bytes s)
      for i = 1 to userStrings_stream_padding do 
          Bytebuf.emit_int_as_byte mdbuf 0x00;

      reportTime "Write Metadata User Strings";
    (* The GUID stream *)
      Array.iter (Bytebuf.emit_bytes mdbuf) guids;
      
    (* The blob stream *)
      Bytebuf.emit_intarray_as_bytes mdbuf [| 0x00 |];
      for s in blobs do 
          emit_z_u32 mdbuf s.Length;
          Bytebuf.emit_bytes mdbuf s
      for i = 1 to blobs_stream_padding do 
          Bytebuf.emit_int_as_byte mdbuf 0x00;
      reportTime "Write Blob Stream";
     (* Done - close the buffer and return the result. *)
      Bytebuf.close mdbuf
    

    if logging then dprintn ("fixing up strings in final metadata...");

   (* Now we know the user string tables etc. we can fixup the *)
   (* uses of strings in the code *)
    for (code_start_addr, l) in reqd_string_fixups do
        for (code_offset,userstring_idx) in l do 
              if code_start_addr < codep.addr or code_start_addr >= codep.addr + codep.size  then failwith "strings-in-code fixup: a group of fixups is located outside the code array";
              let loc_in_code =  ((code_start_addr + code_offset) - codep.addr)
              CheckFixup32 code loc_in_code 0xdeadbeef;
              let token = GetUncodedToken tab_UserStrings (userString_address userstring_idx)
              if (Bytes.get code (loc_in_code-1) <> i_ldstr) then failwith "strings-in-code fixup: not at ldstr instruction!";
              ApplyFixup32 code loc_in_code token
    reportTime "Fixup Metadata";

    if logging then dprintn ("done metadata/code...");
    entryPointToken,
    code, 
    code_padding,
    metadata,
    data,
    resources,
    !reqd_data_fixups,
    pdb_data,
    mappings



(*---------------------------------------------------------------------
 * PHYSICAL METADATA+BLOBS --> PHYSICAL PE FORMAT
 *---------------------------------------------------------------------*)

(* THIS LAYS OUT A 2-SECTION .NET PE BINARY *)
(* SECTIONS *)
(* TEXT: physical 0x0200 --> RVA 0x00020000
           e.g. raw size 0x9600, 
           e.g. virt size 0x9584
   RELOC: physical 0x9800 --> RVA 0x0000c000
      i.e. phys_base --> rva_base
      where phys_base = text_base + text raw size
           phs_rva = roundup(0x2000, 0x0002000 + text virt size)

*)


let msdos_header = 
     [| 0x4d ; 0x5a ; 0x90 ; 0x00 ; 0x03 ; 0x00 ; 0x00 ; 0x00
      ; 0x04 ; 0x00 ; 0x00 ; 0x00 ; 0xFF ; 0xFF ; 0x00 ; 0x00
      ; 0xb8 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00
      ; 0x40 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00
      ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00
      ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00
      ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00
      ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x80 ; 0x00 ; 0x00 ; 0x00
      ; 0x0e ; 0x1f ; 0xba ; 0x0e ; 0x00 ; 0xb4 ; 0x09 ; 0xcd
      ; 0x21 ; 0xb8 ; 0x01 ; 0x4c ; 0xcd ; 0x21 ; 0x54 ; 0x68
      ; 0x69 ; 0x73 ; 0x20 ; 0x70 ; 0x72 ; 0x6f ; 0x67 ; 0x72
      ; 0x61 ; 0x6d ; 0x20 ; 0x63 ; 0x61 ; 0x6e ; 0x6e ; 0x6f
      ; 0x74 ; 0x20 ; 0x62 ; 0x65 ; 0x20 ; 0x72 ; 0x75 ; 0x6e
      ; 0x20 ; 0x69 ; 0x6e ; 0x20 ; 0x44 ; 0x4f ; 0x53 ; 0x20
      ; 0x6d ; 0x6f ; 0x64 ; 0x65 ; 0x2e ; 0x0d ; 0x0d ; 0x0a
      ; 0x24 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 ; 0x00 |]

let write_i64_as_i64 os x =
    Pervasives.output_byte os (dw0 x);
    Pervasives.output_byte os (dw1 x);
    Pervasives.output_byte os (dw2 x);
    Pervasives.output_byte os (dw3 x);
    Pervasives.output_byte os (dw4 x);
    Pervasives.output_byte os (dw5 x);
    Pervasives.output_byte os (dw6 x);
    Pervasives.output_byte os (dw7 x)

let write_i32_as_i32 os x = 
    Pervasives.output_byte os  (b0 x);
    Pervasives.output_byte os  (b1 x);
    Pervasives.output_byte os  (b2 x);
    Pervasives.output_byte os  (b3 x)  

let write_i32_as_u16 os x = 
    Pervasives.output_byte os  (b0 x);
    Pervasives.output_byte os  (b1 x)
      
let write_directory os dict =
    write_i32_as_i32 os (if dict.size = 0x0 then 0x0 else dict.addr);
    write_i32_as_i32 os dict.size

let write_intarray os chunk = 
    for i = 0 to Array.length chunk - 1  do 
      let b = chunk.[i]
      if checking && (b < 0 or b > 255) then dprintn ("write: "^string b^" is not a byte at offset "^string i);
      Pervasives.output_byte os (b % 256)
    done 

let write_bytes os chunk = Bytes.output os chunk 

let write_binary_internal outfile mscorlib (pdbfile: string option) (signer: signer option) fixupOverlappingSequencePoints modul =
    (* Store the public key from the signer into the manifest.  This means it will be written *)
    (* to the binary and also acts as an indicator to leave space for delay sign *)

    reportTime "Write Started";
    let is_dll = modul.modulDLL
    
    let signer = 
        match signer,modul.modulManifest with
        | Some _, _ -> signer
        | _, None -> signer
        | None, Some {manifestPublicKey=Some pubkey} -> 
            (dprintn "Note: The output assembly will be delay-signed using the original public";
             dprintn "Note: key. In order to load it you will need to either sign it with";
             dprintn "Note: the original private key or to turn off strong-name verification";
             dprintn "Note: (use sn.exe from the .NET Framework SDK to do this, e.g. 'sn -Vr *').";
             dprintn "Note: Alternatively if this tool supports it you can provide the original";
             dprintn "Note: private key when converting the assembly, assuming you have access to";
             dprintn "Note: it.";
             Some (signerOpenPublicKey pubkey))
        | _ -> signer

    let modul = 
        let pubkey =
          match signer with 
          | None -> None
          | Some s -> 
             try Some (signerPublicKey s) 
             with e ->     
               failwith ("A call to StrongNameGetPublicKey failed ("^e.Message^")"); 
               None
        begin match modul.modulManifest with 
        | None -> () 
        | Some m -> 
           if m.manifestPublicKey <> None && m.manifestPublicKey <> pubkey then 
             dprintn "Warning: The output assembly is being signed or delay-signed with a strong name that is different to the original."
        end;
        { modul with modulManifest = match modul.modulManifest with None -> None | Some m -> Some {m with manifestPublicKey = pubkey} }

    if logging then dprintf "signerPublicKey (2): %s" outfile;

    let timestamp = absilWriteGetTimeStamp ()

    let os = try  open_out_bin outfile
             with e -> failwith ("Could not open file for writing (binary mode): " ^ outfile)    

    let  pdb_data,debug_directoryp,debug_datap,textV2P,mappings =
        try 
      
          let image_base_real = modul.modulImageBase // FIXED CHOICE
          let align_virt = modul.modulVirtAlignment // FIXED CHOICE
          let align_phys = modul.modulPhysAlignment // FIXED CHOICE
          
          let isItanium = modul.Platform = Some(IA64)
          
          let num_sections = 3 in (* .text, .sdata, .reloc *)


          (* HEADERS *)

          let next = 0x0
          let header_phys_loc = 0x0
          let header_addr = next
          let next = header_addr
          
          let msdos_header_size = 0x80
          let msdos_headerp,next = chunk msdos_header_size next
          
          let pe_signature_size = 0x04
          let pe_signaturep,next = chunk pe_signature_size next
          
          let pe_file_header_size = 0x14
          let pe_file_headerp,next = chunk pe_file_header_size next
          
          let pe_optional_header_size = if modul.Is64Bit then 0xf0 else 0xe0
          let pe_optional_headerp,next = chunk pe_optional_header_size next
          
          let text_section_header_size = 0x28
          let text_section_headerp,next = chunk text_section_header_size next
          
          let data_section_header_size = 0x28
          let data_section_headerp,next = chunk data_section_header_size next
          
          let reloc_section_header_size = 0x28
          let reloc_section_headerp,next = chunk reloc_section_header_size next
          
          let header_size = next - header_addr
          let next_phys = align align_phys (header_phys_loc + header_size)
          let header_phys_size = next_phys - header_phys_loc
          let next = align align_virt (header_addr + header_size)
          
          (* TEXT SECTION:  8 bytes IAT table 72 bytes CLI header *)

          let text_phys_loc = next_phys
          let text_addr = next
          let next = text_addr
          
          let import_addr_tabp,next = chunk 0x08 next
          let cli_header_padding = (if isItanium then (align 16 next) else next) - next
          let next = next + cli_header_padding
          let cli_headerp,next = chunk 0x48 next
          
          let desiredMetadataVersion = 
              match mscorlib with 
              | ScopeRef_local -> failwith "Expected mscorlib to be ScopeRef_assembly was ScopeRef_local" 
              | ScopeRef_module(_) -> failwith "Expected mscorlib to be ScopeRef_assembly was ScopeRef_module"
              | ScopeRef_assembly(aref) ->
                match aref.Version with
                | Some (2us,_,_,_) -> parse_version "2.0.50727.0"
                | Some v -> v
                | None -> failwith "Expected msorlib to have a version number"

          let entryPointToken,code,code_padding,metadata,data,resources,reqd_data_fixups,pdb_data,mappings = 
            write_binary_il ((pdbfile <> None), desiredMetadataVersion,mscorlib) modul next

          reportTime "Generated IL and metadata";
          let codep,next = chunk (Bytes.length code) next
          let code_paddingp,next = chunk (Array.length code_padding) next
          
          let metadatap,next = chunk (Bytes.length metadata) next
          
          let strongnamep,next = 
            match signer with 
            | None -> nochunk next
            | Some s -> chunk (signerSignatureSize s) next

          let resourcesp,next = chunk (Bytes.length resources) next
         
          let rawdatap,next = chunk (Bytes.length data) next

          let vtfixupsp,next = nochunk next in  (* REVIEW *)
          let import_tabp_pre_padding = (if isItanium then (align 16 next) else next) - next
          let next = next + import_tabp_pre_padding
          let import_tabp,next = chunk 0x28 next
          let import_lookup_tabp,next = chunk 0x14 next
          let import_name_hint_tabp,next = chunk 0x0e next
          let mscoree_stringp,next = chunk 0x0c next
          
          let next = align 0x10 (next + 0x05) - 0x05
          let import_tabp = { addr=import_tabp.addr; size = next - import_tabp.addr}
          let import_tabp_padding = import_tabp.size - (0x28 + 0x14 + 0x0e + 0x0c)
          
          let next = next + 0x03
          let entrypoint_codep,next = chunk 0x06 next
          let globalpointer_codep,next = chunk (if isItanium then 0x8 else 0x0) next
          
          let debug_directoryp,next = chunk (if pdbfile = None then 0x0 else sizeof_IMAGE_DEBUG_DIRECTORY) next
          (* The debug data is given to us by the PDB writer and appears to typically be the type of the data plus the PDB file name.  We fill this in after we've written the binary. We approximate the size according to what PDB writers seem to require and leave extra space just in case... *)
          let debug_data_just_in_case = 40
          let debug_datap,next = chunk (align 0x4 (match pdbfile with None -> 0x0 | Some f -> (24 + String.length f + debug_data_just_in_case))) next


          let text_size = next - text_addr
          let next_phys = align align_phys (text_phys_loc + text_size)
          let text_phys_size = next_phys - text_phys_loc
          let next = align align_virt (text_addr + text_size)
          
          (* .RSRC SECTION (DATA) *)
          let data_phys_loc =  next_phys
          let data_addr = next
          let dataV2P v = v - data_addr + data_phys_loc
          
          let resourceFormat = if modul.Is64Bit then Ilsupp.X64 else Ilsupp.X86
          
          let nativeResources = 
            match modul.modulNativeResources with
            | [] -> [||]
            | resources ->
                if runningOnMono then
                  [||]
                else
                  let unlinkedResources = List.map Lazy.force resources
                  begin
                    try linkNativeResources unlinkedResources next resourceFormat (Path.GetDirectoryName(outfile))
                    with e -> failwith ("Linking a native resource failed: "^e.Message^"")
                  end
                
          let native_resource_size = nativeResources.Length

          let native_resourcesp,next = chunk native_resource_size next
        
          let dummydatap,next = chunk (if next = data_addr then 0x01 else 0x0) next
          
          let data_size = next - data_addr
          let next_phys = align align_phys (data_phys_loc + data_size)
          let data_phys_size = next_phys - data_phys_loc
          let next = align align_virt (data_addr + data_size)
          
          (* .RELOC SECTION  base reloc table: 0x0c size *)
          let reloc_phys_loc =  next_phys
          let reloc_addr = next
          let base_reloc_tabp,next = chunk 0x0c next

          let reloc_size = next - reloc_addr
          let next_phys = align align_phys (reloc_phys_loc + reloc_size)
          let reloc_phys_size = next_phys - reloc_phys_loc
          let next = align align_virt (reloc_addr + reloc_size)

          
          if logging then dprintn ("fixup references into data section...");

         (* Now we know where the data section lies we can fix up the  *)
         (* references into the data section from the metadata tables. *)
          begin 
            reqd_data_fixups |> List.iter
              (fun (metadata_offset32,(data_offset,kind)) -> 
                let metadata_offset =  metadata_offset32
                if metadata_offset < 0 or metadata_offset >= Bytes.length metadata - 4  then failwith "data RVA fixup: fixup located outside metadata";
                CheckFixup32 metadata metadata_offset 0xdeaddddd;
                let data_rva = 
                  if kind then
                      let res = data_offset
                      if res >= resourcesp.size then dprintn ("resource offset bigger than resource data section");
                      res
                  else 
                      let res = rawdatap.addr + data_offset
                      if res < rawdatap.addr then dprintn ("data rva before data section");
                      if res >= rawdatap.addr + rawdatap.size then dprintn ("data rva after end of data section, data_rva = "^string res^", rawdatap.addr = "^string rawdatap.addr^", rawdatap.size = "^string rawdatap.size);
                      res
                ApplyFixup32 metadata metadata_offset data_rva);
          end;
          
         (* IMAGE TOTAL SIZE *)
          let image_end_phys_loc =  next_phys
          let image_end_addr = next

          reportTime "Layout image";
          if logging then dprintn ("writing image...");

          let write p os chunkName chunk = 
              match p with 
              | None -> () 
              | Some p' -> 
                  if (Pervasives.pos_out os) <> p' then 
                    failwith ("warning: "^chunkName^" not where expected, pos_out = "^string (Pervasives.pos_out os)^", p.addr = "^string p') 
              write_intarray os chunk 
          
          let write_padding os string sz =
              if sz < 0 then failwith "write_padding: size < 0";
              for i = 0 to sz - 1 do 
                  Pervasives.output_byte os 0
          
          (* Now we've computed all the offsets, write the image *)
          
          write (Some msdos_headerp.addr) os "msdos header" msdos_header;
          
      (* PE HEADER *)
         
          write (Some pe_signaturep.addr) os "pe_signature" [| |];
          
          write_i32_as_i32 os 0x4550;
          
          write (Some pe_file_headerp.addr) os "pe_file_header" [| |];
          
          if (modul.Platform = Some(AMD64)) then
            write_i32_as_u16 os 0x8664 (* Machine - IMAGE_FILE_MACHINE_AMD64 *)
          elif isItanium then
            write_i32_as_u16 os 0x200
          else
            write_i32_as_u16 os 0x014c;(* Machine - IMAGE_FILE_MACHINE_I386 *)
            
          write_i32_as_u16 os num_sections; 
          write_i32_as_i32 os timestamp; (* date since 1970 *)
          write_i32_as_i32 os 0x00; (* Pointer to Symbol Table Always 0 *)
       (* 00000090 *) 
          write_i32_as_i32 os 0x00; (* Number of Symbols Always 0 *)
          write_i32_as_u16 os pe_optional_header_size; (* Size of the optional header, the format is described below. *)
          
          (* 
            64bit: IMAGE_FILE_32BIT_MACHINE ||| IMAGE_FILE_LARGE_ADDRESS_AWARE
            32bit: IMAGE_FILE_32BIT_MACHINE
            
            Yes, 32BIT_MACHINE is set for AMD64...
          *)     
          let imachine_characteristic = match modul.Platform with | Some(IA64) -> 0x20 | Some(AMD64) -> 0x0120 | _ -> 0x0100
          
          write_i32_as_u16 os ((if is_dll then 0x2000 else 0x0000) ||| 0x0002 ||| 0x0004 ||| 0x0008 ||| imachine_characteristic);
          
     (* Now comes optional header *)

          let peOptionalHeaderByte = peOptionalHeaderByteByCLRVersion desiredMetadataVersion

          write (Some pe_optional_headerp.addr) os "pe_optional_header" [| |];
          if modul.Is64Bit then
            write_i32_as_u16 os 0x020B (* Magic number is 0x020B for 64-bit *)
          else
            write_i32_as_u16 os 0x010b; (* Always 0x10B (see Section 23.1). *)
          write_i32_as_u16 os peOptionalHeaderByte; (* QUERY: ECMA spec says 6, some binaries, e.g. fscmanaged.exe say 7, Whidbey binaries say 8 *)
          write_i32_as_i32 os text_phys_size; (* Size of the code (text) section, or the sum of all code sections if there are multiple sections. *)
       (* 000000a0 *) 
          write_i32_as_i32 os data_phys_size; (* Size of the initialized data section, or the sum of all such sections if there are multiple data sections. *)
          write_i32_as_i32 os 0x00; (* Size of the uninitialized data section, or the sum of all such sections if there are multiple unitinitalized data sections. *)
          write_i32_as_i32 os entrypoint_codep.addr; (* RVA of entry point , needs to point to bytes 0xFF 0x25 followed by the RVA+!0x4000000 in a section marked execute/read for EXEs or 0 for DLLs e.g. 0x0000b57e *)
          write_i32_as_i32 os text_addr; (* e.g. 0x0002000 *)
       (* 000000b0 *)
          if modul.Is64Bit then
            write_i64_as_i64 os ((int64)image_base_real) (* REVIEW: For 64-bit, we should use a 64-bit image base *)
          else             
            (write_i32_as_i32 os data_addr; (* e.g. 0x0000c000 *)          
             write_i32_as_i32 os image_base_real); (* Image Base Always 0x400000 (see Section 23.1). - QUERY : no it's not always 0x400000, e.g. 0x034f0000 *)
            
          write_i32_as_i32 os align_virt;  (*  Section Alignment Always 0x2000 (see Section 23.1). *)
          write_i32_as_i32 os align_phys; (* File Alignment Either 0x200 or 0x1000. *)
       (* 000000c0 *) 
          write_i32_as_u16 os 0x04; (*  OS Major Always 4 (see Section 23.1). *)
          write_i32_as_u16 os 0x00; (* OS Minor Always 0 (see Section 23.1). *)
          write_i32_as_u16 os 0x00; (* User Major Always 0 (see Section 23.1). *)
          write_i32_as_u16 os 0x00; (* User Minor Always 0 (see Section 23.1). *)
          write_i32_as_u16 os 0x04; (* SubSys Major Always 4 (see Section 23.1). *)
          write_i32_as_u16 os 0x00; (* SubSys Minor Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Reserved Always 0 (see Section 23.1). *)
       (* 000000d0 *) 
          write_i32_as_i32 os image_end_addr; (* Image Size: Size, in bytes, of image, including all headers and padding; shall be a multiple of Section Alignment. e.g. 0x0000e000 *)
          write_i32_as_i32 os header_phys_size; (* Header Size Combined size of MS-DOS Header, PE Header, PE Optional Header and padding; shall be a multiple of the file alignment. *)
          write_i32_as_i32 os 0x00; (* File Checksum Always 0 (see Section 23.1). QUERY: NOT ALWAYS ZERO *)
          write_i32_as_u16 os modul.modulSubSystem; (* SubSystem Subsystem required to run this image. Shall be either IMAGE_SUBSYSTEM_WINDOWS_CE_GUI (0x3) or IMAGE_SUBSYSTEM_WINDOWS_GUI (0x2). QUERY: Why is this 3 on the images ILASM produces *)
          write_i32_as_u16 os (if isItanium then 0x8540 else 0x400);  (*  DLL Flags Always 0x400 (no unmanaged windows exception handling - see Section 23.1). *)
       (* 000000e0 *)
          (* Note that the defaults differ between x86 and x64 *) 
          if modul.Is64Bit then
            (write_i64_as_i64 os ((int64)0x400000); (* Stack Reserve Size Always 0x400000 (4Mb) (see Section 23.1). *)
             write_i64_as_i64 os ((int64)0x4000); (* Stack Commit Size Always 0x4000 (16Kb) (see Section 23.1). *)
             write_i64_as_i64 os ((int64)0x100000); (* Heap Reserve Size Always 0x100000 (1Mb) (see Section 23.1). *)
             write_i64_as_i64 os ((int64)0x2000)) (* Heap Commit Size Always 0x800 (8Kb) (see Section 23.1). *)
          else
            (write_i32_as_i32 os 0x100000; (* Stack Reserve Size Always 0x100000 (1Mb) (see Section 23.1). *)
             write_i32_as_i32 os 0x1000; (* Stack Commit Size Always 0x1000 (4Kb) (see Section 23.1). *)
             write_i32_as_i32 os 0x100000; (* Heap Reserve Size Always 0x100000 (1Mb) (see Section 23.1). *)
             write_i32_as_i32 os 0x1000); (* Heap Commit Size Always 0x1000 (4Kb) (see Section 23.1). *)            
       (* 000000f0 - x86 location, moving on, for x64, add 0x10 *) 
          write_i32_as_i32 os 0x00; (* Loader Flags Always 0 (see Section 23.1) *)
          write_i32_as_i32 os 0x10; (* Number of Data Directories: Always 0x10 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; 
          write_i32_as_i32 os 0x00; (* Export Table Always 0 (see Section 23.1). *)
       (* 00000100 *) 
          write_directory os import_tabp; (* Import Table RVA of Import Table, (see clause 24.3.1). e.g. 0000b530 *) 
          (* Native Resource Table: ECMA says Always 0 (see Section 23.1), but mscorlib and other files with resources bound into executable do not.  For the moment assume the resources table is always the first resource in the file. *)
          write_directory os native_resourcesp;

       (* 00000110 *) 
          write_i32_as_i32 os 0x00; (* Exception Table Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Exception Table Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Certificate Table Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Certificate Table Always 0 (see Section 23.1). *)
       (* 00000120 *) 
          write_directory os base_reloc_tabp; 
          write_directory os debug_directoryp; (* Debug Directory *)
       (* 00000130 *) 
          write_i32_as_i32 os 0x00; (*  Copyright Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (*  Copyright Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Global Ptr Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Global Ptr Always 0 (see Section 23.1). *)
       (* 00000140 *) 
          write_i32_as_i32 os 0x00; (* Load Config Table Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Load Config Table Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* TLS Table Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* TLS Table Always 0 (see Section 23.1). *)
       (* 00000150  *) 
          write_i32_as_i32 os 0x00; (* Bound Import Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Bound Import Always 0 (see Section 23.1). *)
          write_directory os import_addr_tabp; (* Import Addr Table, (see clause 24.3.1). e.g. 0x00002000 *) 
       (* 00000160  *) 
          write_i32_as_i32 os 0x00; (* Delay Import Descriptor Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Delay Import Descriptor Always 0 (see Section 23.1). *)
          write_directory os cli_headerp;
       (* 00000170 *) 
          write_i32_as_i32 os 0x00; (* Reserved Always 0 (see Section 23.1). *)
          write_i32_as_i32 os 0x00; (* Reserved Always 0 (see Section 23.1). *)
          
          write (Some text_section_headerp.addr) os "text_section_header" [| |];
          
       (* 00000178 *) 
          write_intarray os  [| 0x2e; 0x74; 0x65; 0x78; 0x74; 0x00; 0x00; 0x00; |]; (* ".text\000\000\000" *)
       (* 00000180 *) 
          write_i32_as_i32 os text_size; (* VirtualSize: Total size of the section when loaded into memory in bytes rounded to Section Alignment. If this value is greater than Size of Raw Data, the section is zero-padded. e.g. 0x00009584 *)
          write_i32_as_i32 os text_addr; (*  VirtualAddress For executable images this is the address of the first byte of the section, when loaded into memory, relative to the image base. e.g. 0x00020000 *)
          write_i32_as_i32 os text_phys_size; (*  SizeOfRawData Size of the initialized data on disk in bytes, shall be a multiple of FileAlignment from the PE header. If this is less than VirtualSize the remainder of the section is zero filled. Because this field is rounded while the VirtualSize field is not it is possible for this to be greater than VirtualSize as well. When a section contains only uninitialized data, this field should be 0. 0x00009600 *)
          write_i32_as_i32 os text_phys_loc; (* PointerToRawData RVA to section’s first page within the PE file. This shall be a multiple of FileAlignment from the optional header. When a section contains only uninitialized data, this field should be 0. e.g. 00000200 *)
       (* 00000190 *) 
          write_i32_as_i32 os 0x00; (* PointerToRelocations RVA of Relocation section. *)
          write_i32_as_i32 os 0x00; (* PointerToLinenumbers Always 0 (see Section 23.1). *)
       (* 00000198 *) 
          write_i32_as_u16 os 0x00;(* NumberOfRelocations Number of relocations, set to 0 if unused. *)
          write_i32_as_u16 os 0x00;  (*  NumberOfLinenumbers Always 0 (see Section 23.1). *)
          write_intarray os [| 0x20; 0x00; 0x00; 0x60 |]; (*  Characteristics Flags describing section’s characteristics, see below. IMAGE_SCN_CNT_CODE || IMAGE_SCN_MEM_EXECUTE || IMAGE_SCN_MEM_READ *)
          
          write (Some data_section_headerp.addr) os "data_section_header" [| |];
          
       (* 000001a0 *) 
          write_intarray os [| 0x2e; 0x72; 0x73; 0x72; 0x63; 0x00; 0x00; 0x00; |]; (* ".rsrc\000\000\000" *)
    (*  write_intarray os [| 0x2e; 0x73; 0x64; 0x61; 0x74; 0x61; 0x00; 0x00; |]; (* ".sdata\000\000" *) *)
          write_i32_as_i32 os data_size; (* VirtualSize: Total size of the section when loaded into memory in bytes rounded to Section Alignment. If this value is greater than Size of Raw Data, the section is zero-padded. e.g. 0x0000000c *)
          write_i32_as_i32 os data_addr; (*  VirtualAddress For executable images this is the address of the first byte of the section, when loaded into memory, relative to the image base. e.g. 0x0000c000*)
       (* 000001b0 *) 
          write_i32_as_i32 os data_phys_size; (*  SizeOfRawData Size of the initialized data on disk in bytes, shall be a multiple of FileAlignment from the PE header. If this is less than VirtualSize the remainder of the section is zero filled. Because this field is rounded while the VirtualSize field is not it is possible for this to be greater than VirtualSize as well. When a section contains only uninitialized data, this field should be 0. e.g. 0x00000200 *)
          write_i32_as_i32 os data_phys_loc; (* PointerToRawData QUERY: Why does ECMA say "RVA" here? Offset to section’s first page within the PE file. This shall be a multiple of FileAlignment from the optional header. When a section contains only uninitialized data, this field should be 0. e.g. 0x00009800 *)
       (* 000001b8 *) 
          write_i32_as_i32 os 0x00; (* PointerToRelocations RVA of Relocation section. *)
          write_i32_as_i32 os 0x00; (* PointerToLinenumbers Always 0 (see Section 23.1). *)
       (* 000001c0 *) 
          write_i32_as_u16 os 0x00; (* NumberOfRelocations Number of relocations, set to 0 if unused. *)
          write_i32_as_u16 os 0x00;  (*  NumberOfLinenumbers Always 0 (see Section 23.1). *)
          write_intarray os [| 0x40; 0x00; 0x00; 0x40 |]; (*  Characteristics Flags: IMAGE_SCN_MEM_READ |  IMAGE_SCN_CNT_INITIALIZED_DATA *)
          
          write (Some reloc_section_headerp.addr) os "reloc_section_header" [| |];
       (* 000001a0 *) 
          write_intarray os [| 0x2e; 0x72; 0x65; 0x6c; 0x6f; 0x63; 0x00; 0x00; |]; (* ".reloc\000\000" *)
          write_i32_as_i32 os reloc_size; (* VirtualSize: Total size of the section when loaded into memory in bytes rounded to Section Alignment. If this value is greater than Size of Raw Data, the section is zero-padded. e.g. 0x0000000c *)
          write_i32_as_i32 os reloc_addr; (*  VirtualAddress For executable images this is the address of the first byte of the section, when loaded into memory, relative to the image base. e.g. 0x0000c000*)
       (* 000001b0 *) 
          write_i32_as_i32 os reloc_phys_size; (*  SizeOfRawData Size of the initialized reloc on disk in bytes, shall be a multiple of FileAlignment from the PE header. If this is less than VirtualSize the remainder of the section is zero filled. Because this field is rounded while the VirtualSize field is not it is possible for this to be greater than VirtualSize as well. When a section contains only uninitialized reloc, this field should be 0. e.g. 0x00000200 *)
          write_i32_as_i32 os reloc_phys_loc; (* PointerToRawData QUERY: Why does ECMA say "RVA" here? Offset to section’s first page within the PE file. This shall be a multiple of FileAlignment from the optional header. When a section contains only uninitialized reloc, this field should be 0. e.g. 0x00009800 *)
       (* 000001b8 *) 
          write_i32_as_i32 os 0x00; (* b0 relocptr; b1 relocptr; b2 relocptr; b3 relocptr; *) (* PointerToRelocations RVA of Relocation section. *)
          write_i32_as_i32 os 0x00; (* PointerToLinenumbers Always 0 (see Section 23.1). *)
       (* 000001c0 *) 
          write_i32_as_u16 os 0x00; (* b0 numreloc; b1 numreloc; *) (* NumberOfRelocations Number of relocations, set to 0 if unused. *)
          write_i32_as_u16 os 0x00;  (*  NumberOfLinenumbers Always 0 (see Section 23.1). *)
          write_intarray os [| 0x40; 0x00; 0x00; 0x42 |]; (*  Characteristics Flags: IMAGE_SCN_CNT_INITIALIZED_DATA | IMAGE_SCN_MEM_READ |  *)
          
          write_padding os "pad to text begin" (text_phys_loc - header_size);
          
          (* TEXT SECTION: e.g. 0x200 *)
          
          let textV2P v = v - text_addr + text_phys_loc
          
          (* e.g. 0x0200 *)
          write (Some (textV2P import_addr_tabp.addr)) os "import_addr_table" [| |];
          write_i32_as_i32 os import_name_hint_tabp.addr; 
          write_i32_as_i32 os 0x00;  (* QUERY 4 bytes of zeros not 2 like ECMA  24.3.1 says *)
          
          (* e.g. 0x0208 *)

          let flags = 
            (if modul.modulILonly then 0x01 else 0x00) ||| 
            (if modul.modul32bit then 0x02 else 0x00) ||| 
            (if (match signer with None -> false | Some s -> signerFullySigned s) then 0x08 else 0x00)

          let headerVersionMajor,headerVersionMinor = headerVersionSupportedByCLRVersion desiredMetadataVersion

          write_padding os "pad to cli header" cli_header_padding 
          write (Some (textV2P cli_headerp.addr)) os "cli_header"  [| |];
          write_i32_as_i32 os 0x48; (* size of header *)
          write_i32_as_u16 os headerVersionMajor; (* Major part of minimum version of CLR reqd. *)
          write_i32_as_u16 os headerVersionMinor; (* Minor part of minimum version of CLR reqd. ... *)
          (* e.g. 0x0210 *)
          write_directory os metadatap;
          write_i32_as_i32 os flags;
          
          write_i32_as_i32 os entryPointToken; 
          write None os "rest of cli header" [| |];
          
          (* e.g. 0x0220 *)
          write_directory os resourcesp;
          write_directory os strongnamep;
          (* e.g. 0x0230 *)
          write_i32_as_i32 os 0x00; (* code manager table, always 0 *)
          write_i32_as_i32 os 0x00; (* code manager table, always 0 *)
          write_directory os vtfixupsp; 
          (* e.g. 0x0240 *)
          write_i32_as_i32 os 0x00;  (* export addr table jumps, always 0 *)
          write_i32_as_i32 os 0x00;  (* export addr table jumps, always 0 *)
          write_i32_as_i32 os 0x00;  (* managed native header, always 0 *)
          write_i32_as_i32 os 0x00;  (* managed native header, always 0 *)
          
          write_bytes os code;
          write None os "code padding" code_padding;
          
          write_bytes os metadata;
          
          (* write 0x80 bytes of empty space for encrypted SHA1 hash, written by SN.EXE or call to signing API *)
          if signer <> None then 
            write (Some (textV2P strongnamep.addr)) os "strongname" (Array.create ( strongnamep.size) 0x0);
          
          write (Some (textV2P resourcesp.addr)) os "raw resources" [| |];
          write_bytes os resources;
          write (Some (textV2P rawdatap.addr)) os "raw data" [| |];
          write_bytes os data;

          write_padding os "start of import_table" import_tabp_pre_padding

          (* vtfixups would go here *)
          write (Some (textV2P import_tabp.addr)) os "import_table" [| |];
          
          write_i32_as_i32 os import_lookup_tabp.addr;
          write_i32_as_i32 os 0x00;
          write_i32_as_i32 os 0x00;
          write_i32_as_i32 os mscoree_stringp.addr;
          write_i32_as_i32 os import_addr_tabp.addr;
          write_i32_as_i32 os 0x00;
          write_i32_as_i32 os 0x00;
          write_i32_as_i32 os 0x00;
          write_i32_as_i32 os 0x00;
          write_i32_as_i32 os 0x00; 
        
          write (Some (textV2P import_lookup_tabp.addr)) os "import_lookup_table" [| |];
          write_i32_as_i32 os import_name_hint_tabp.addr; 
          write_i32_as_i32 os 0x00; 
          write_i32_as_i32 os 0x00; 
          write_i32_as_i32 os 0x00; 
          write_i32_as_i32 os 0x00; 
          

          write (Some (textV2P import_name_hint_tabp.addr)) os "import_name_hint_table" [| |];
          // Two zero bytes of hint, then Case sensitive, null-terminated ASCII string containing name to import. 
          // Shall _CorExeMain a .exe file _CorDllMain for a .dll file.
          (if  is_dll then 
            write_intarray os [| 0x00;  0x00;  
                                0x5f;  0x43 ;  0x6f;  0x72 ;  0x44;  0x6c ;  0x6c;  0x4d ;  0x61;  0x69 ;  0x6e;  0x00 |]
           else 
            write_intarray os [| 0x00;  0x00;  
                                0x5f;  0x43 ;  0x6f;  0x72 ;  0x45;  0x78 ;  0x65;  0x4d ;  0x61;  0x69 ;  0x6e;  0x00 |]);
          
          write (Some (textV2P mscoree_stringp.addr)) os "mscoree string"
            [| 0x6d;  0x73;  
              0x63;  0x6f ;  0x72;  0x65 ;  0x65;  0x2e ;  0x64;  0x6c ;  0x6c;  0x00 ; |];
          
          write_padding os "end of import tab" import_tabp_padding;
          
          write_padding os "head of entrypoint" 0x03;
          let ep = (image_base_real + text_addr)
          write (Some (textV2P entrypoint_codep.addr)) os " entrypoint code"
                 [| 0xFF; 0x25; (* x86 Instructions for entry *) b0 ep; b1 ep; b2 ep; b3 ep |];
          if isItanium then 
            write (Some (textV2P globalpointer_codep.addr)) os " itanium global pointer"
                 [| 0x0; 0x0; 0x0; 0x0; 0x0; 0x0; 0x0; 0x0 |];
          
          if pdbfile <> None then 
            write (Some (textV2P debug_directoryp.addr)) os "debug directory" (Array.create sizeof_IMAGE_DEBUG_DIRECTORY 0x0);
          
          if pdbfile <> None then 
            write (Some (textV2P debug_datap.addr)) os "debug data" (Array.create ( (debug_datap.size)) 0x0);
          
          write_padding os "end of .text" (data_phys_loc - text_phys_loc - text_size);
          
          (* DATA SECTION *)
          begin match nativeResources with
            | [||] -> ()
            | resources ->
                write (Some (dataV2P native_resourcesp.addr)) os "raw native resources" [| |];
                write_bytes os resources;
          end;

          if dummydatap.size <> 0x0 then
           write (Some (dataV2P dummydatap.addr)) os "dummy data" [| 0x0 |];

          write_padding os "end of .rsrc" (reloc_phys_loc - data_phys_loc - data_size);            
          
          (* RELOC SECTION *)

          (* See ECMA 24.3.2 *)
          
     
          let relocV2P v = v - reloc_addr + reloc_phys_loc
          
          let entrypoint_fixup_addr = entrypoint_codep.addr + 0x02
          let entrypoint_fixup_block = (entrypoint_fixup_addr / 4096) * 4096
          let entrypoint_fixup_offset = entrypoint_fixup_addr - entrypoint_fixup_block
          let reloc = (if modul.Is64Bit then 0xA000 (* IMAGE_REL_BASED_DIR64 *) else 0x3000 (* IMAGE_REL_BASED_HIGHLOW *)) ||| entrypoint_fixup_offset
          // For the itanium, you need to set a relocation entry for the global pointer
          let reloc2 = 
            if not(isItanium) then 
                0x0
            else
                0xA000 ||| (globalpointer_codep.addr - ((globalpointer_codep.addr / 4096) * 4096))
               
          write (Some (relocV2P base_reloc_tabp.addr)) os "base_reloc_table" 
             [| b0 entrypoint_fixup_block; b1 entrypoint_fixup_block; b2 entrypoint_fixup_block; b3 entrypoint_fixup_block;
              0x0c; 0x00; 0x00; 0x00;
              b0 reloc; b1 reloc; 
              b0 reloc2; b1 reloc2; |];
          write_padding os "end of .reloc" (image_end_phys_loc - reloc_phys_loc - reloc_size);

          close_out os;
          pdb_data,debug_directoryp,debug_datap,textV2P,mappings
          
        with e -> (try close_out os; System.IO.File.Delete outfile with _ -> ()); rethrow(); raise e
   

    reportTime "Writing Image";
    if logging then dprintn ("Finished writing the binary...");
     
    (* Now we've done the bulk of the binary, do the PDB file and fixup the binary. *)
    begin match pdbfile with
    | None -> ()
    | Some fpdb when runningOnMono -> warning(Error(NoPDBsOnMonoWarningE.Format,rangeCmdArgs)) ; ()
    | Some fpdb -> 
        try 
            if logging then dprintn ("Now write debug info...");  

            let idd = WritePdbInfo fixupOverlappingSequencePoints outfile fpdb pdb_data
            reportTime "Generate PDB Info";
            
          (* Now we have the debug data we can go back and fill in the debug directory in the image *)
            let os2 = open_out_gen [Open_binary; Open_wronly] 0x777 outfile
            try 
                (* write the IMAGE_DEBUG_DIRECTORY *)
                seek_out os2 ( (textV2P debug_directoryp.addr));
                write_i32_as_i32 os2 idd.iddCharacteristics; (* IMAGE_DEBUG_DIRECTORY.Characteristics *)
                write_i32_as_i32 os2 timestamp;
                write_i32_as_u16 os2 idd.iddMajorVersion;
                write_i32_as_u16 os2 idd.iddMinorVersion;
                write_i32_as_i32 os2 idd.iddType;
                write_i32_as_i32 os2 (Bytes.length idd.iddData);  (* IMAGE_DEBUG_DIRECTORY.SizeOfData *)
                write_i32_as_i32 os2 debug_datap.addr;  (* IMAGE_DEBUG_DIRECTORY.AddressOfRawData *)
                write_i32_as_i32 os2 (textV2P debug_datap.addr);(* IMAGE_DEBUG_DIRECTORY.PointerToRawData *)

                (* dprintf "idd.iddCharacteristics = %ld\n" idd.iddCharacteristics;
                dprintf "iddMajorVersion = %ld\n" idd.iddMajorVersion;
                dprintf "iddMinorVersion = %ld\n" idd.iddMinorVersion;
                dprintf "iddType = %ld\n" idd.iddType;
                dprintf "iddData = (%A) = %s\n" idd.iddData (Bytes.utf8_bytes_as_string idd.iddData); *)
                  
                (* write the debug raw data as given us by the PDB writer *)
                seek_out os2 ( (textV2P debug_datap.addr));
                if debug_datap.size < (Bytes.length idd.iddData) then 
                    failwith "Debug data area is not big enough.  Debug info may not be usable";
                let len = min ( (debug_datap.size)) (Bytes.length idd.iddData)
                write_bytes os2 idd.iddData;
                close_out os2;
            with e -> 
                failwith ("Error while writing debug directory entry: "^e.Message);
                (try close_out os2; System.IO.File.Delete outfile with _ -> ()); 
                rethrow()
        with e -> 
            rethrow()
            
    end;
    reportTime "Finalize PDB";

    (* Sign the binary.  No further changes to binary allowed past this point! *)
    begin match signer with 
    | None -> ()
    | Some s -> 
        if logging then dprintn ("Now sign the binary...");
        try 
            signerSignFile outfile s; signerClose s 
        with e -> 
            failwith ("Warning: A call to StrongNameSignatureGeneration failed ("^e.Message^")");
            (try signerClose s with _ -> ());
            (try System.IO.File.Delete outfile with _ -> ()); 
            ()
    end;
    reportTime "Signing Image";
    if logging then dprintn ("Finished writing and signing the binary and debug info...");

    mappings


type options =
 { mscorlib: ILScopeRef;
   pdbfile: string option;
   signer: signer option;
   fixupOverlappingSequencePoints: bool }


let WriteILBinary outfile args modul =
  ignore (write_binary_internal  outfile args.mscorlib args.pdbfile args.signer args.fixupOverlappingSequencePoints modul)



(******************************************************
** Notes on supporting the Itanium (jopamer)         **
*******************************************************
IA64 codegen on the CLR isn’t documented, and getting it working involved a certain amount of reverse-engineering 
peverify.exe and various binaries generated by ILAsm and other managed compiles.  Here are some lessons learned, 
documented for posterity and the 0 other people writing managed compilers for the Itanium:

- Even if you’re not utilizing the global pointer in your Itanium binary, 
you should be setting aside space for it in .text.  (Preferably near the native stub.)
- PEVerify checks for two .reloc table entries on the Itanium - one for the native stub, and one 
for the global pointer RVA.  It doesn’t matter what you set these values to - 
their addresses can be zeroed out, but they must have IMAGE_REL_BASED_DIR64 set!  
(So, yes, you may find yourself setting this flag on an empty, unnecessary table slot!)
- On the Itanium, it’s best to have your tables qword aligned.  (Though, peverify checks for dword alignment.)
- A different, weird set of DLL characteristics are necessary for the Itanium.  
I won’t detail them here, but it’s interesting given that this field isn’t supposed to vary between platforms, 
and is supposedly marked as deprecated.
- There are two schools to generating CLR binaries on for the Itanium - I’ll call them the “ALink” school 
and the “ILAsm” school.
                - The ALink school relies on some quirks in the CLR to omit a lot of stuff that, admittedly, isn’t necessary.  The binaries are basically IL-only, with some flags set to make them nominally Itanium:
                                - It omits the .reloc table
                                - It doesn’t set aside memory for global pointer storage
                                - There’s no native stub
                                - There’s no import table, mscoree reference / startup symbol hint
                                - A manifest is inserted by default. 
                These omissions are understandable, given the platform/jitting/capabilities of the language, 
                but they’re basically relying on an idiosyncracy of the runtime to get away with creating a “bad” binary.

                - The ILAsm school actually writes everything out:
                                - It has a reloc table with the requisite two entries
                                - It sets aside memory for a global pointer, even if it doesn’t utilize one
                                - It actually inserts a native stub for the Itanium!  (Though, I have no idea what 
                                instructions, specifically, are emitted, and I couldn’t dig up the sources to ILAsm to 
                                find out)
                                - There’s the requisite mscoree reference, etc.
                                - No manifest is inserted
*******************************************************)
