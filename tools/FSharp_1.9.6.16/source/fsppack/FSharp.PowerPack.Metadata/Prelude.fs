
// (c) Microsoft Corporation. All rights reserved

//---------------------------------------------------------------------------
// This file contains definitions that mimic definitions from parts of the F#
// compiler as necessary to allow the internal compiler typed abstract
// syntax tree and metadata deserialization code to be loaded independently
// of the rest of the compiler.
//
// This is used to give FSharp.PowerPack.Metadata.dll a minimal code base support
// to ensure we can publish the source code for that DLL as an independent
// sample. At some point we may fork the implementation of this DLL completely.
//
// Ideally this file would be empty, and gradually we will align the compiler
// source to allow this to be the case.
//---------------------------------------------------------------------------

module FSharp.PowerPack.Metadata.Reader.Internal.Prelude


let isSome x = Option.isSome x

type pos =  { posLine: int; posCol: int }  

let mk_pos x y = { posLine=x; posCol=y }
type range = 
    { rangeFile: string;
      rangeBegin: pos;
      rangeEnd: pos }  

let mk_range file p1 p2 = 
    { rangeFile = file; rangeBegin=p1; rangeEnd=p2 }

let range0 =  mk_range "unknown" (mk_pos 1 0) (mk_pos 1 80)

[<Sealed>]
type ident (text:string,range:range) = 
     member x.idText = text
     member x.idRange = range
     override x.ToString() = text
let mksyn_id m s = ident(s,m)

type MemberFlags =
  { OverloadQualifier: string option; 
    MemberIsInstance: bool;
    MemberIsVirtual: bool;
    MemberIsDispatchSlot: bool;
    MemberIsOverrideOrExplicitImpl: bool;
    MemberIsFinal: bool;
    MemberKind: MemberKind }
and MemberKind = 
    | MemberKindClassConstructor
    | MemberKindConstructor
    | MemberKindMember 
    | MemberKindPropertyGet 
    | MemberKindPropertySet    
    | MemberKindPropertyGetSet    
and TyparStaticReq = 
    | NoStaticReq 
    | HeadTypeStaticReq 

and SynTypar = 
    | Typar of ident * TyparStaticReq * bool 

type LazyWithContext<'a,'ctxt> = 
    { v: Lazy<'a> }
    member x.Force(_) = x.v.Force()
    static member NotLazy (x:'a)  = { v = Lazy.CreateFromValue x }
    static member Create (f: unit -> 'a)  = { v = Lazy.Create f }

type XmlDoc = XmlDoc of string[]

let emptyXmlDoc = XmlDoc[| |]
let MergeXmlDoc (XmlDoc lines) (XmlDoc lines') = XmlDoc (Array.append lines lines')

let notlazy v = Lazy.CreateFromValue v
module String =


    let tryDropSuffix s t = 
        let lens = String.length s
        let lent = String.length t
        if (lens >= lent && (s.Substring (lens-lent, lent) = t)) then 
            Some (s.Substring (0,lens - lent))
        else
            None

    let hasSuffix s t = (tryDropSuffix s t).IsSome
    let dropSuffix s t = match (tryDropSuffix s t) with Some(res) -> res | None -> failwith "dropSuffix"


type SequencePointInfoForBinding = unit
type SequencePointInfoForTarget = unit
type SequencePointInfoForTry = unit
type SequencePointInfoForWith = unit
type SequencePointInfoForFinally = unit
type SequencePointInfoForSeq = unit
type SequencePointInfoForForLoop =  unit
type SequencePointInfoForWhileLoop = unit

let SuppressSequencePointAtTarget = ()
let NoSequencePointAtStickyBinding = ()
let NoSequencePointAtTry = ()
let NoSequencePointAtWith = ()
let NoSequencePointAtFinally = ()
let NoSequencePointAtForLoop = ()
let NoSequencePointAtWhileLoop = ()
let SuppressSequencePointOnExprOfSequential = ()

let error e = raise e
let errorR e = raise e
let Error(s,m) = Failure s
let InternalError(s,m) = Failure s
let UnresolvedReferenceNoRange s = Failure ("unresolved reference " + s)
let UnresolvedPathReferenceNoRange (s,p) = Failure ("unresolved reference " + s + " for path " + p)
type NameMap<'a> = Map<string,'a>
type ExprData = unit
type NameMultiMap<'a> = 'a list NameMap


[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module NameMap = 
    let empty = Map.empty
    let tryfind v (m: 'a NameMap) = Map.tryFind v m 
    let of_keyed_list f l = List.foldBack (fun x acc -> Map.add (f x) x acc) l Map.empty
    let range m = List.rev (Map.foldBack (fun _ x sofar -> x :: sofar) m [])
    let add v x (m: 'a NameMap) = Map.add v x m
    let foldRange f (l: 'a NameMap) acc = Map.foldBack (fun _ y acc -> f y acc) l acc
    let mem v (m: 'a NameMap) = Map.contains v m
    let find v (m: 'a NameMap) = Map.find v m

module List = 
    let frontAndBack l = 
        let rec loop acc l = 
            match l with
            | [] -> 
                System.Diagnostics.Debug.Assert(false, "empty list")
                invalidArg "l" "empty list" 
            | [h] -> List.rev acc,h
            | h::t -> loop  (h::acc) t
        loop [] l

    let mapSquared f xss = xss |> List.map (List.map f)

type cache<'a> = NoCache
let new_cache() = NoCache
let cacheOptRef _ f = f ()
let cached _ f = f()
type FlatList<'a> = List<'a>
let (===) x y = LanguagePrimitives.PhysicalEquality x y
let dprintf fmt = printf fmt
let text_of_path path = String.concat "." path
type SkipFreeVarsCache = unit
type FreeVarsCache = unit cache

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module NameMultiMap = 
    let find v (m: NameMultiMap<'a>) = match Map.tryFind v m with None -> [] | Some r -> r
    let add v x (m: NameMultiMap<'a>) = NameMap.add v (x :: find v m) m
    let empty : NameMultiMap<'a> = Map.empty
    let range (m: NameMultiMap<'a>) = Map.foldBack (fun _ x sofar -> x @ sofar) m []


type 'a nonnull_slot = 'a
let nullable_slot_empty() = Unchecked.defaultof<'a>
let nullable_slot_full(x) = x


module Bytes = 
    module Bytestream = 
        type t = { bytes: byte[]; mutable pos: int; max: int }

        let of_bytes (b:byte[]) n len = 
            if n < 0 or (n+len) > b.Length then failwith "Bytestream.of_bytes";
            { bytes = b; pos = n; max = n+len }

        let read_byte b  = 
            if b.pos >= b.max then failwith "Bytestream.of_bytes.read_byte: end of stream";
            let res = b.bytes.[b.pos] 
            b.pos <- b.pos + 1;
            int32 res 
          
        let read_bytes b n  = 
            if b.pos + n > b.max then failwith "Bytestream.read_bytes: end of stream";
            let res = Array.sub b.bytes b.pos n in
            b.pos <- b.pos + n;
            res 

        let position b = b.pos 
        let clone_and_seek b pos = { bytes=b.bytes; pos=pos; max=b.max }
        let skip b n = b.pos <- b.pos + n

        let read_utf8_bytes_as_string (b:t) n = 
            let res = System.Text.Encoding.UTF8.GetString(b.bytes,b.pos,n) 
            b.pos <- b.pos + n; res 

