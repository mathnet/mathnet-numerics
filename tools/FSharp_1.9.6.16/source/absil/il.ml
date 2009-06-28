// (c) Microsoft Corporation. All rights reserved

#light

#if STANDALONE_METADATA
module (* internal *) FSharp.PowerPack.Metadata.Reader.Internal.AbstractIL.IL

open System.Collections.Generic

#else
module (* internal *) Microsoft.FSharp.Compiler.AbstractIL.IL

#nowarn "49"

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open System.Collections.Generic
 
let logging = false 

// Officially supported way to detect if we are running on Mono.
// See http://www.mono-project.com/FAQ:_Technical
// "How can I detect if am running in Mono?" section
let runningOnMono = System.Type.GetType("Mono.Runtime") <> null

let _ = if logging then dprintn "* warning: Il.logging is on"

let isNil x = match x with [] -> true | _ -> false
let nonNil x = match x with [] -> false | _ -> true
let int_order (a:int) b = compare a b
#endif

let notlazy v = Lazy.CreateFromValue v

/// A little ugly, but the idea is that if a data structure does not 
/// contain lazy values then we don't add laziness.  So if the thing to map  
/// is already evaluated then immediately apply the function.  
let lazy_map f (x:Lazy<_>) =  
      if x.IsForced then notlazy (f (x.Force())) else lazy (f (x.Force()))


  
// -------------------------------------------------------------------- 
// Ordered lists with a lookup table
// --------------------------------------------------------------------

/// This is used to store event, property and field maps.
///
/// Review: this is not such a great data structure.
type LazyOrderedMultiMap<'key,'data>(keyf : 'data -> 'key, lazyItems : Lazy<'data list>) = 

    let quickMap= 
        lazyItems |> lazy_map (fun entries -> 
            let t = new Dictionary<_,_>(entries.Length)
            do entries |> List.iter (fun y -> let key = keyf y in t.[key] <- y :: (if t.ContainsKey(key) then t.[key] else [])) 
            t)

    member self.Entries() = lazyItems.Force()

    member self.Add(y) = new LazyOrderedMultiMap<'key,'data>(keyf, lazyItems |> lazy_map (fun x -> y :: x))
    
    member self.Filter(f) = new LazyOrderedMultiMap<'key,'data>(keyf, lazyItems |> lazy_map (List.filter f))

    member self.Item with get(x) = let t = quickMap.Force() in if t.ContainsKey x then t.[x] else []


(*---------------------------------------------------------------------
 * SHA1 hash-signing algorithm.  Used to get the public key token from
 * the public key.
 *---------------------------------------------------------------------*)


let b0 n =  (n &&& 0xFF)
let b1 n =  ((n >>> 8) &&& 0xFF)
let b2 n =  ((n >>> 16) &&& 0xFF)
let b3 n =  ((n >>> 24) &&& 0xFF)


module SHA1 = 
    let inline (lsr)  (x:int) (y:int)  = int32 (uint32 x >>> y)
    let f(t,b,c,d) = 
        if t < 20 then (b &&& c) ||| ((~~~b) &&& d) else
        if t < 40 then b ^^^ c ^^^ d else
        if t < 60 then (b &&& c) ||| (b &&& d) ||| (c &&& d) else
        b ^^^ c ^^^ d

    let k0to19 = 0x5A827999
    let k20to39 = 0x6ED9EBA1
    let k40to59 = 0x8F1BBCDC
    let k60to79 = 0xCA62C1D6

    let k(t) = 
        if t < 20 then k0to19 
        elif t < 40 then k20to39 
        elif t < 60 then k40to59 
        else k60to79 


    type chan = SHABytes of byte[] 
    type sha_instream = 
        { stream: chan;
          mutable pos: int;
          mutable eof:  bool; }

    let rot_left32 x n =  (x <<< n) ||| (x lsr (32-n))

    let sha_eof sha = sha.eof

    (* padding and length (in bits!) recorded at end *)
    let sha_after_eof sha  = 
        let n = sha.pos
        let len = 
          (match sha.stream with
          | SHABytes s -> s.Length)
        if n = len then 0x80
        else 
          let padded_len = (((len + 9 + 63) / 64) * 64) - 8
          if n < padded_len - 8  then 0x0  
          elif (n &&& 63) = 56 then int32 ((int64 len * int64 8) >>> 56) &&& 0xff
          elif (n &&& 63) = 57 then int32 ((int64 len * int64 8) >>> 48) &&& 0xff
          elif (n &&& 63) = 58 then int32 ((int64 len * int64 8) >>> 40) &&& 0xff
          elif (n &&& 63) = 59 then int32 ((int64 len * int64 8) >>> 32) &&& 0xff
          elif (n &&& 63) = 60 then int32 ((int64 len * int64 8) >>> 24) &&& 0xff
          elif (n &&& 63) = 61 then int32 ((int64 len * int64 8) >>> 16) &&& 0xff
          elif (n &&& 63) = 62 then int32 ((int64 len * int64 8) >>> 8) &&& 0xff
          elif (n &&& 63) = 63 then (sha.eof <- true; int32 (int64 len * int64 8) &&& 0xff)
          else 0x0

    let sha_read8 sha = 
        let b = 
            match sha.stream with 
            | SHABytes s -> if sha.pos >= s.Length then sha_after_eof sha else int32 s.[sha.pos]
        sha.pos <- sha.pos + 1; 
        b
        
    let sha_read32 sha  = 
        let b0 = sha_read8 sha
        let b1 = sha_read8 sha
        let b2 = sha_read8 sha
        let b3 = sha_read8 sha
        let res = (b0 <<< 24) ||| (b1 <<< 16) ||| (b2 <<< 8) ||| b3
        res


    let sha1_hash sha = 
        let h0 = ref 0x67452301
        let h1 = ref 0xEFCDAB89
        let h2 = ref 0x98BADCFE
        let h3 = ref 0x10325476
        let h4 = ref 0xC3D2E1F0
        let a = ref 0
        let b = ref 0
        let c = ref 0
        let d = ref 0
        let e = ref 0
        let w = Array.create 80 0x00
        while (not (sha_eof sha)) do
          for i = 0 to 15 do
            w.[i] <- sha_read32 sha
          done;
          for t = 16 to 79 do
            w.[t] <- rot_left32 (w.[t-3] ^^^ w.[t-8] ^^^ w.[t-14] ^^^ w.[t-16]) 1;
          done;
          a := !h0; 
          b := !h1; 
          c := !h2; 
          d := !h3; 
          e := !h4;
          for t = 0 to 79 do
            let temp =  (rot_left32 !a 5) + f(t,!b,!c,!d) + !e + w.[t] + k(t)
            e := !d; 
            d := !c; 
            c :=  rot_left32 !b 30; 
            b := !a; 
            a := temp;
          done;
          h0 := !h0 + !a; 
          h1 := !h1 + !b; 
          h2 := !h2 + !c;  
          h3 := !h3 + !d; 
          h4 := !h4 + !e
        done;
        (!h0,!h1,!h2,!h3,!h4)

    let sha1_hash_bytes s = 
        let (h0,h1,h2,h3,h4) = sha1_hash { stream = SHABytes s; pos = 0; eof = false } 
        Array.map byte [|  b0 h4; b1 h4; b2 h4; b3 h4; b0 h3; b1 h3; b2 h3; b3 h3; |]


let sha1_hash_bytes s = SHA1.sha1_hash_bytes s
(* --------------------------------------------------------------------
 * 
 * -------------------------------------------------------------------- *)

type ILVersionInfo = uint16 * uint16 * uint16 * uint16

type Locale = string
type PublicKey =
    | PublicKey of byte[]
    | PublicKeyToken of byte[]
    member x.IsKey=match x with PublicKey _ -> true | _ -> false
    member x.IsKeyToken=match x with PublicKeyToken _ -> true | _ -> false
    member x.Key=match x with PublicKey b -> b | _ -> invalid_arg "not a key"
    member x.KeyToken=match x with PublicKeyToken b -> b | _ -> invalid_arg "not a key token"

    member x.ToToken() = 
        match x with 
        | PublicKey bytes -> SHA1.sha1_hash_bytes bytes
        | PublicKeyToken token -> token
    static member KeyAsToken(k) = PublicKeyToken(PublicKey(k).ToToken())

type AssemblyRefData =
    { assemRefName: string;
      assemRefHash: byte[] option;
      assemRefPublicKeyInfo: PublicKey option;
      assemRefRetargetable: bool;
      assemRefVersion: ILVersionInfo option;
      assemRefLocale: Locale option; } 

/// Global state: table of all assembly references keyed by AssemblyRefData
#if STANDALONE_METADATA
#else
let AssemblyRefUniqueStampGenerator = new UniqueStampGenerator<AssemblyRefData>()
#endif

[<Sealed>]
type ILAssemblyRef(data)  =  
#if STANDALONE_METADATA
#else
    let uniqueStamp = AssemblyRefUniqueStampGenerator.Encode(data)
#endif
    member x.Name=data.assemRefName
    member x.Hash=data.assemRefHash
    member x.PublicKey=data.assemRefPublicKeyInfo
    member x.Retargetable=data.assemRefRetargetable  
    member x.Version=data.assemRefVersion
    member x.Locale=data.assemRefLocale
#if STANDALONE_METADATA
#else
    member x.UniqueStamp=uniqueStamp
    override x.GetHashCode() = uniqueStamp
    override x.Equals(yobj) = ((yobj :?> ILAssemblyRef).UniqueStamp = uniqueStamp)
    interface System.IComparable with
        override x.CompareTo(yobj) = compare (yobj :?> ILAssemblyRef).UniqueStamp uniqueStamp
#endif
    static member Create(name,hash,publicKey,retargetable,version,locale) =
        ILAssemblyRef
            { assemRefName=name;
              assemRefHash=hash;
              assemRefPublicKeyInfo=publicKey;
              assemRefRetargetable=retargetable;
              assemRefVersion=version;
              assemRefLocale=locale; } 

    static member FromAssembly(assembly:System.Reflection.Assembly) =
        let aname = assembly.GetName()
        let locale = None
        //match aname.CultureInfo with 
        //   | null -> None 
        //   | x -> Some x.Name
        let publicKey = 
           match aname.GetPublicKey()  with 
           | null -> 
               match aname.GetPublicKeyToken()  with 
               | null -> None
               | bytes -> Some (PublicKeyToken bytes)
           | bytes -> 
               Some (PublicKey bytes)
        
        let version = 
           match aname.Version with 
           | null -> None
           | v -> Some (uint16 v.Major,uint16 v.Minor,uint16 v.Build,uint16 v.Revision)

        ILAssemblyRef.Create(aname.Name,None,publicKey,false,version,locale)

    member aref.QualifiedName = 
        let b = new System.Text.StringBuilder(100)
        let add (s:string) = (b.Append(s) |> ignore)
        let addC (s:char) = (b.Append(s) |> ignore)
        add(aref.Name);
        begin match aref.Version with 
        | None -> ()
        | Some (a,b,c,d) -> 
            add ", Version=";
            add (string (int a))
            add ".";
            add (string (int b))
            add ".";
            add (string (int c))
            add ".";
            add (string (int d))
            add ", Culture="
            begin match aref.Locale with 
            | None -> add "neutral"
            | Some b -> add b
            end;
            add ", PublicKeyToken="
            begin match aref.PublicKey with 
            | None -> add "null"
            | Some pki -> 
                  let pkt = pki.ToToken()
                  let convDigit(digit) = 
                      let digitc = 
                          if digit < 10 
                          then  System.Convert.ToInt32 '0' + digit 
                          else System.Convert.ToInt32 'a' + (digit - 10) 
                      System.Convert.ToChar(digitc)
                  for i = 0 to pkt.Length-1 do
                      let v = pkt.[i]
                      addC (convDigit(System.Convert.ToInt32(v)/16))
                      addC (convDigit(System.Convert.ToInt32(v)%16))
                  done
            end
        end;
        b.ToString()


type ILModuleRef = 
    { name: string;
      hasMetadata: bool; 
      hash: byte[] option; }
    static member Create(name,hasMetadata,hash) = 
        { name=name;
          hasMetadata= hasMetadata;
          hash=hash }
    
    member x.Name=x.name
    member x.HasMetadata=x.hasMetadata
    member x.Hash=x.hash 

type ILScopeRef = 
    | ScopeRef_local
    | ScopeRef_module of ILModuleRef 
    | ScopeRef_assembly of ILAssemblyRef
    static member Local = ScopeRef_local
    static member Module(mref) = ScopeRef_module(mref)
    static member Assembly(aref) = ScopeRef_assembly(aref)
    member x.IsLocalRef   = match x with ScopeRef_local      -> true | _ -> false
    member x.IsModuleRef  = match x with ScopeRef_module _   -> true | _ -> false
    member x.IsAssemblyRef= match x with ScopeRef_assembly _ -> true | _ -> false
    member x.ModuleRef    = match x with ScopeRef_module x   -> x | _ -> failwith "not a module reference"
    member x.AssemblyRef  = match x with ScopeRef_assembly x -> x | _ -> failwith "not an assembly reference"

    member scoref.QualifiedName = 
        match scoref with 
        | ScopeRef_local -> ""
        | ScopeRef_module mref -> "module "^mref.Name
        | ScopeRef_assembly aref when aref.Name = "mscorlib" -> ""
        | ScopeRef_assembly aref -> aref.QualifiedName

    member scoref.QualifiedNameWithNoShortMscorlib = 
        match scoref with 
        | ScopeRef_local -> ""
        | ScopeRef_module mref -> "module "^mref.Name
        | ScopeRef_assembly aref -> aref.QualifiedName

type ILArrayBound = int32 option 
type ILArrayBounds = ILArrayBound * ILArrayBound
type ILArrayShape = 
    | ILArrayShape of ILArrayBounds list (* lobound/size pairs *)
    member x.Rank = (let (ILArrayShape l) = x in l.Length)


/// Calling conventions.  These are used in method pointer types.
type ILArgumentConvention = 
    | CC_default
    | CC_cdecl 
    | CC_stdcall 
    | CC_thiscall 
    | CC_fastcall 
    | CC_vararg
      
type ILThisConvention =
    | CC_instance
    | CC_instance_explicit
    | CC_static

let mutable instance_callconv : obj = new obj()
let mutable static_callconv : obj = new obj()

type ILCallingConv =
    | Callconv of ILThisConvention * ILArgumentConvention
    member x.ThisConv           = let (Callconv(a,b)) = x in a
    member x.BasicConv          = let (Callconv(a,b)) = x in b
    member x.IsInstance         = match x.ThisConv with CC_instance -> true | _ -> false
    member x.IsInstanceExplicit = match x.ThisConv with CC_instance_explicit -> true | _ -> false
    member x.IsStatic           = match x.ThisConv with CC_static -> true | _ -> false

    static member Instance : ILCallingConv = unbox(instance_callconv) 
    static member Static : ILCallingConv = unbox(static_callconv) 

do instance_callconv <- box (Callconv(CC_instance,CC_default))
do static_callconv <- box (Callconv(CC_static,CC_default))

let callconv_eq (a:ILCallingConv) b = (a = b)

type ILBoxity = 
  | AsObject 
  | AsValue


// IL type references have a pre-computed hash code to enable quick lookup tables during binary generation.
[<StructuralEquality(false); StructuralComparison(false)>]
type ILTypeRef = 
    { trefScope: ILScopeRef;
      trefEnclosing: string list;
      trefName: string; 
      hashCode : int }
      
    static member Create(scope,enclosing,name) = 
        let hashCode = hash scope * 17 ^^^ (hash enclosing * 101 <<< 1) ^^^ (hash name * 47 <<< 2)
        { trefScope=scope;
          trefEnclosing= enclosing;
          trefName=name;
          hashCode=hashCode }
          
    member x.Scope= x.trefScope
    member x.Enclosing= x.trefEnclosing
    member x.Name=x.trefName
    member x.ApproxId= x.hashCode
    override x.GetHashCode() = x.hashCode
    override x.Equals(yobj) = 
         let y = (yobj :?> ILTypeRef) 
         (x.ApproxId = y.ApproxId) && 
         (x.Scope = y.Scope) && 
         (x.Name = y.Name) && 
         (x.Enclosing = y.Enclosing)
    interface System.IComparable with
        override x.CompareTo(yobj) = 
            let y = (yobj :?> ILTypeRef) 
            let c = compare x.ApproxId y.ApproxId
            if c <> 0 then c else
            let c = compare x.Scope y.Scope
            if c <> 0 then c else
            let c = compare x.Name y.Name 
            if c <> 0 then c else
            compare x.Enclosing y.Enclosing
        
    member tref.FullName = String.concat "." (tref.Enclosing @ [tref.Name])
        
    member tref.BasicQualifiedName = 
        String.concat "+" (tref.Enclosing @ [ tref.Name ])

    member tref.AddQualifiedNameExtensionWithNoShortMscorlib(basic) = 
        let sco = tref.Scope.QualifiedNameWithNoShortMscorlib
        if sco = "" then basic else String.concat ", " [basic;sco]

    member tref.QualifiedNameWithNoShortMscorlib = 
        tref.AddQualifiedNameExtensionWithNoShortMscorlib(tref.BasicQualifiedName)

    member tref.QualifiedName = 
        let basic = tref.BasicQualifiedName
        let sco = tref.Scope.QualifiedName
        if sco = "" then basic else String.concat ", " [basic;sco]


    override x.ToString() = x.FullName

        
type ILTypeSpec = 
    { tspecTypeRef: ILTypeRef;    
      /// The type instantiation if the type is generic
      tspecInst: ILGenericArgs }    
    member x.TypeRef=x.tspecTypeRef
    member x.Scope=x.TypeRef.Scope
    member x.Enclosing=x.TypeRef.Enclosing
    member x.Name=x.TypeRef.Name
    member x.GenericArgs=x.tspecInst
    static member Create(tref,inst) = { tspecTypeRef =tref; tspecInst=inst }
    override x.ToString() = x.TypeRef.ToString() + (match x.GenericArgs with [] -> "" | _ -> "<...>")
    member x.BasicQualifiedName = 
        let tc = x.TypeRef.BasicQualifiedName
        match x.GenericArgs with 
        | [] -> tc
        | args -> tc + "[" + String.concat "," (args |> List.map (fun arg -> "[" + arg.QualifiedNameWithNoShortMscorlib + "]")) + "]"

    member x.AddQualifiedNameExtensionWithNoShortMscorlib(basic) = 
        x.TypeRef.AddQualifiedNameExtensionWithNoShortMscorlib(basic)

and ILType =
    | Type_void                   
    | Type_array    of ILArrayShape * ILType 
    | Type_value    of ILTypeSpec      
    | Type_boxed    of ILTypeSpec      
    | Type_ptr      of ILType             
    | Type_byref    of ILType           
    | Type_fptr     of ILCallingSignature 
    | Type_tyvar    of uint16              
    | Type_modified of bool * ILTypeRef * ILType

    member x.BasicQualifiedName = 
        match x with 
        | Type_tyvar n -> "!" + string n
        | Type_modified(_,ty1,ty2) -> ty2.BasicQualifiedName
        | Type_array (ILArrayShape(s),ty) -> ty.BasicQualifiedName + "[" + System.String(',',s.Length-1) + "]"
        | Type_value tr | Type_boxed tr -> tr.BasicQualifiedName
        | Type_void -> failwith "unexpected void type"
        | Type_ptr ty -> failwith "unexpected pointer type"
        | Type_byref ty -> failwith "unexpected byref type"
        | Type_fptr mref -> failwith "unexpected function pointer type"

    member x.AddQualifiedNameExtensionWithNoShortMscorlib(basic) = 
        match x with 
        | Type_tyvar n -> basic
        | Type_modified(_,ty1,ty2) -> ty2.AddQualifiedNameExtensionWithNoShortMscorlib(basic)
        | Type_array (ILArrayShape(s),ty) -> ty.AddQualifiedNameExtensionWithNoShortMscorlib(basic)
        | Type_value tr | Type_boxed tr -> tr.AddQualifiedNameExtensionWithNoShortMscorlib(basic)
        | Type_void -> failwith "unexpected void type"
        | Type_ptr ty -> failwith "unexpected pointer type"
        | Type_byref ty -> failwith "unexpected byref type"
        | Type_fptr mref -> failwith "unexpected function pointer type"
        
    member x.QualifiedNameWithNoShortMscorlib = 
        x.AddQualifiedNameExtensionWithNoShortMscorlib(x.BasicQualifiedName)

and IlxExtensionType = Ext_typ of obj

and ILCallingSignature = 
    { callsigCallconv: ILCallingConv;
      callsigArgs: ILType list;
      callsigReturn: ILType }
    member x.CallingConv = x.callsigCallconv
    member x.ArgTypes = x.callsigArgs
    member x.ReturnType = x.callsigReturn


and ILGenericParameterDefs = ILGenericParameterDef list
and ILGenericArgs = ILType list
and ILGenericVariance = 
    | NonVariant            
    | CoVariant             
    | ContraVariant         

and ILGenericParameterDef =
    { gpName: string;
      gpConstraints: ILType list;
      gpVariance: ILGenericVariance; 
      gpReferenceTypeConstraint: bool;     
      gpNotNullableValueTypeConstraint: bool;
      gpDefaultConstructorConstraint: bool; }

    member x.Name = x.gpName
    member x.Constraints = x.gpConstraints
    member x.Variance = x.gpVariance
    member x.HasReferenceTypeConstraint = x.gpReferenceTypeConstraint
    member x.HasNotNullableValueTypeConstraint = x.gpNotNullableValueTypeConstraint
    member x.HasDefaultConstructorConstraint = x.gpDefaultConstructorConstraint
    override x.ToString() = x.Name 

let mk_callsig (cc,args,ret) = { callsigArgs=args; callsigCallconv=cc; callsigReturn=ret}


type ILMethodRef =
    { mrefParent: ILTypeRef;
      mrefCallconv: ILCallingConv;
      mrefGenericArity: int; 
      mrefName: string;
      mrefArgs: ILType list;
      mrefReturn: ILType }
    member x.EnclosingTypeRef = x.mrefParent
    member x.CallingConv = x.mrefCallconv
    member x.Name = x.mrefName
    member x.GenericArity = x.mrefGenericArity
    member x.ArgCount = x.mrefArgs.Length
    member x.ArgTypes = x.mrefArgs
    member x.ReturnType = x.mrefReturn

    member x.CallingSignature = mk_callsig (x.CallingConv,x.ArgTypes,x.ReturnType)
    static member Create(a,b,c,d,e,f) = 
        { mrefParent= a;mrefCallconv=b;mrefName=c;mrefGenericArity=d; mrefArgs=e;mrefReturn=f }
    override x.ToString() = x.Name + "(...)"


type ILFieldRef = 
    { frefParent: ILTypeRef;
      frefName: string;
      frefType: ILType }
    member x.EnclosingTypeRef = x.frefParent
    member x.Name = x.frefName
    member x.Type = x.frefType

type ILMethodSpec = 
    { mspecMethodRefF: ILMethodRef;
      mspecEnclosingTypeF: ILType;          
      mspecMethodInstF: ILGenericArgs; }     
    static member Create(a,b,c) = { mspecEnclosingTypeF=a; mspecMethodRefF =b; mspecMethodInstF=c }
    member x.MethodRef = x.mspecMethodRefF
    member x.EnclosingType=x.mspecEnclosingTypeF
    member x.GenericArgs=x.mspecMethodInstF
    member x.Name=x.MethodRef.Name
    member x.CallingConv=x.MethodRef.CallingConv
    member x.GenericArity = x.MethodRef.GenericArity
    member x.FormalArgTypes = x.MethodRef.ArgTypes
    member x.FormalReturnType = x.MethodRef.ReturnType
    override x.ToString() = x.Name + "(...)"

let dest_mspec x = (x.mspecMethodRefF, x.mspecEnclosingTypeF, x.mspecMethodInstF)

type ILFieldSpec =
    { fspecFieldRef: ILFieldRef;
      fspecEnclosingType: ILType }         
    member x.FieldRef         = x.fspecFieldRef
    member x.EnclosingType    = x.fspecEnclosingType
    member x.FormalType       = x.FieldRef.Type
    member x.Name             = x.FieldRef.Name
    member x.EnclosingTypeRef = x.FieldRef.EnclosingTypeRef
    override x.ToString() = x.Name


(* --------------------------------------------------------------------
 * Debug info.                                                     
 * -------------------------------------------------------------------- *)

type Guid =  byte[]

type ILPlatform = 
    | X86
    | AMD64
    | IA64

type ILSourceDocument = 
    { sourceLanguage: Guid option; 
      sourceVendor: Guid option;
      sourceDocType: Guid option;
      sourceFile: string; }
    static member Create(language,vendor,docType,file) =
        { sourceLanguage=language; 
          sourceVendor=vendor;
          sourceDocType=docType;
          sourceFile=file; }
    member x.Language=x.sourceLanguage
    member x.Vendor=x.sourceVendor
    member x.DocumentType=x.sourceDocType
    member x.File=x.sourceFile

type ILSourceMarker =
    { sourceDocument: ILSourceDocument;
      sourceLine: int;
      sourceColumn: int;
      sourceEndLine: int;
      sourceEndColumn: int }
    static member Create(document, line, column, endLine, endColumn) = 
        { sourceDocument=document;
          sourceLine=line;
          sourceColumn=column;
          sourceEndLine=endLine;
          sourceEndColumn=endColumn }
    member x.Document=x.sourceDocument
    member x.Line=x.sourceLine
    member x.Column=x.sourceColumn
    member x.EndLine=x.sourceEndLine
    member x.EndColumn=x.sourceEndColumn
    override x.ToString() = sprintf "(%d,%d)-(%d,%d)" x.Line x.Column x.EndLine x.EndColumn

(* --------------------------------------------------------------------
 * Custom attributes                                                     
 * -------------------------------------------------------------------- *)

type ILAttributeElement =  
  | CustomElem_string of string  option
  | CustomElem_bool of bool
  | CustomElem_char of char
  | CustomElem_int8 of int8
  | CustomElem_int16 of int16
  | CustomElem_int32 of int32
  | CustomElem_int64 of int64
  | CustomElem_uint8 of uint8
  | CustomElem_uint16 of uint16
  | CustomElem_uint32 of uint32
  | CustomElem_uint64 of uint64
  | CustomElem_float32 of single
  | CustomElem_float64 of double
  | CustomElem_type of ILType
  | CustomElem_tref of ILTypeRef
  | CustomElem_array of ILAttributeElement list

type ILAttributeNamedArg =  (string * ILType * bool * ILAttributeElement)
type ILAttribute = 
    { customMethod: ILMethodSpec;
      customData: byte[] }
    member x.Data = x.customData
    member x.Method =x.customMethod

type ILAttributes = 
   CustomAttrs of Lazy<ILAttribute list>

type ILCodeLabel = int

(* --------------------------------------------------------------------
 * Instruction set.                                                     
 * -------------------------------------------------------------------- *)

type ILBasicType =
  | DT_R
  | DT_I1
  | DT_U1
  | DT_I2
  | DT_U2
  | DT_I4
  | DT_U4
  | DT_I8
  | DT_U8
  | DT_R4
  | DT_R8
  | DT_I
  | DT_U
  | DT_REF

type ILTokenSpec = 
  | Token_type of ILType 
  | Token_method of ILMethodSpec 
  | Token_field of ILFieldSpec

type ILConstSpec = 
  | NUM_I4 of int32
  | NUM_I8 of int64
  | NUM_R4 of single
  | NUM_R8 of double

type Tailcall = 
  | Tailcall
  | Normalcall

type Alignment =  
  | Aligned
  | Unaligned_1
  | Unaligned_2
  | Unaligned_4

type Volatility =  
  | Volatile
  | Nonvolatile

type ReadonlySpec =  
  | ReadonlyAddress
  | NormalAddress

type varargs = ILType list option

type ILComparisonInstr = 
  | BI_beq        
  | BI_bge        
  | BI_bge_un     
  | BI_bgt        
  | BI_bgt_un        
  | BI_ble        
  | BI_ble_un        
  | BI_blt        
  | BI_blt_un 
  | BI_bne_un 
  | BI_brfalse 
  | BI_brtrue 

type ILArithInstr = 
  | AI_add    
  | AI_add_ovf
  | AI_add_ovf_un
  | AI_and    
  | AI_div   
  | AI_div_un
  | AI_ceq      
  | AI_cgt      
  | AI_cgt_un   
  | AI_clt     
  | AI_clt_un  
  | AI_conv      of ILBasicType
  | AI_conv_ovf  of ILBasicType
  | AI_conv_ovf_un  of ILBasicType
  | AI_mul       
  | AI_mul_ovf   
  | AI_mul_ovf_un
  | AI_rem       
  | AI_rem_un       
  | AI_shl       
  | AI_shr       
  | AI_shr_un
  | AI_sub       
  | AI_sub_ovf   
  | AI_sub_ovf_un   
  | AI_xor       
  | AI_or        
  | AI_neg       
  | AI_not       
  | AI_ldnull    
  | AI_dup       
  | AI_pop
  | AI_ckfinite 
  | AI_nop
  | AI_ldc of ILBasicType * ILConstSpec 


type ILInstr = 
  | I_arith of ILArithInstr
  | I_ldarg     of uint16
  | I_ldarga    of uint16
  | I_ldind     of Alignment * Volatility * ILBasicType
  | I_ldloc     of uint16
  | I_ldloca    of uint16
  | I_starg     of uint16
  | I_stind     of  Alignment * Volatility * ILBasicType
  | I_stloc     of uint16

  | I_br    of  ILCodeLabel
  | I_jmp   of ILMethodSpec
  | I_brcmp of ILComparisonInstr * ILCodeLabel * ILCodeLabel (* second label is fall-through *)
  | I_switch    of (ILCodeLabel list * ILCodeLabel) (* last label is fallthrough *)
  | I_ret 

  | I_call     of Tailcall * ILMethodSpec * varargs
  | I_callvirt of Tailcall * ILMethodSpec * varargs
  | I_callconstraint of Tailcall * ILType * ILMethodSpec * varargs
  | I_calli    of Tailcall * ILCallingSignature * varargs
  | I_ldftn    of ILMethodSpec
  | I_newobj  of ILMethodSpec  * varargs
  
  | I_throw
  | I_endfinally
  | I_endfilter
  | I_leave     of  ILCodeLabel

  | I_ldsfld      of Volatility * ILFieldSpec
  | I_ldfld       of Alignment * Volatility * ILFieldSpec
  | I_ldsflda     of ILFieldSpec
  | I_ldflda      of ILFieldSpec 
  | I_stsfld      of Volatility  *  ILFieldSpec
  | I_stfld       of Alignment * Volatility * ILFieldSpec
  | I_ldstr       of string
  | I_isinst      of ILType
  | I_castclass   of ILType
  | I_ldtoken     of ILTokenSpec
  | I_ldvirtftn   of ILMethodSpec

  | I_cpobj       of ILType
  | I_initobj     of ILType
  | I_ldobj       of Alignment * Volatility * ILType
  | I_stobj       of Alignment * Volatility * ILType
  | I_box         of ILType
  | I_unbox       of ILType
  | I_unbox_any   of ILType
  | I_sizeof      of ILType

  | I_ldelem      of ILBasicType
  | I_stelem      of ILBasicType
  | I_ldelema     of ReadonlySpec * ILArrayShape * ILType
  | I_ldelem_any  of ILArrayShape * ILType
  | I_stelem_any  of ILArrayShape * ILType
  | I_newarr      of ILArrayShape * ILType 
  | I_ldlen

  | I_mkrefany    of ILType
  | I_refanytype  
  | I_refanyval   of ILType
  | I_rethrow

  | I_break 
  | I_seqpoint of ILSourceMarker

  | I_arglist  

  | I_localloc
  | I_cpblk of Alignment * Volatility
  | I_initblk of Alignment  * Volatility

  (* FOR EXTENSIONS, e.g. MS-ILX *)  
  | EI_ilzero of ILType
  | EI_ldlen_multi      of int32 * int32
  | I_other    of IlxExtensionInstr

and IlxExtensionInstr = Ext_instr of obj


type ILDebugMapping = 
    { localNum: int;
      localName: string; }
    member x.LocalVarIndex = x.localNum
    member x.Name = x.localName

type ILBasicBlock = 
    { bblockLabel: ILCodeLabel;
      bblockInstrs: ILInstr array }
    member x.Label = x.bblockLabel
    member x.Instructions = x.bblockInstrs

type ILCode = 
    | ILBasicBlock    of ILBasicBlock
    | GroupBlock    of ILDebugMapping list * ILCode list
    | RestrictBlock of ILCodeLabel list * ILCode
    | TryBlock      of ILCode * ILExceptionBlock

and ILExceptionBlock = 
    | FaultBlock       of ILCode 
    | FinallyBlock     of ILCode
    | FilterCatchBlock of (ILFilterBlock * ILCode) list

and ILFilterBlock = 
    | TypeFilter of ILType
    | CodeFilter of ILCode

type Local = 
    { localType: ILType;
      localPinned: bool }
    member x.Type = x.localType
    member x.IsPinned = x.localPinned
      
type ILMethodBody = 
    { ilZeroInit: bool;
      ilMaxStack: int32;
      ilNoInlining: bool;
      ilLocals: Local list;
      ilCode:  ILCode;
      ilSource: ILSourceMarker option }

type ILMemberAccess = 
    | MemAccess_assembly
    | MemAccess_compilercontrolled
    | MemAccess_famandassem
    | MemAccess_famorassem
    | MemAccess_family
    | MemAccess_private 
    | MemAccess_public 

and ILFieldInit = 
    | FieldInit_string of string
    | FieldInit_bool of bool
    | FieldInit_char of uint16
    | FieldInit_int8 of int8
    | FieldInit_int16 of int16
    | FieldInit_int32 of int32
    | FieldInit_int64 of int64
    | FieldInit_uint8 of uint8
    | FieldInit_uint16 of uint16
    | FieldInit_uint32 of uint32
    | FieldInit_uint64 of uint64
    | FieldInit_single of single
    | FieldInit_double of double
    | FieldInit_ref
  
// -------------------------------------------------------------------- 
// Native Types, for marshalling to the native C interface.
// These are taken directly from the ILASM syntax, and don't really
// correspond yet to the ECMA Spec (Partition II, 7.4).  
// -------------------------------------------------------------------- 

type ILNativeType = 
    | NativeType_empty
    | NativeType_custom of Guid * string * string * byte[] (* guid,nativeTypeName,custMarshallerName,cookieString *)
    | NativeType_fixed_sysstring of int32
    | NativeType_fixed_array of int32
    | NativeType_currency
    | NativeType_lpstr
    | NativeType_lpwstr
    | NativeType_lptstr
    | NativeType_byvalstr
    | NativeType_tbstr
    | NativeType_lpstruct
    | NativeType_struct
    | NativeType_void
    | NativeType_bool
    | NativeType_int8
    | NativeType_int16
    | NativeType_int32
    | NativeType_int64
    | NativeType_float32
    | NativeType_float64
    | NativeType_unsigned_int8
    | NativeType_unsigned_int16
    | NativeType_unsigned_int32
    | NativeType_unsigned_int64
    | NativeType_array of ILNativeType option * (int32 * int32 option) option (* optional idx of parameter giving size plus optional additive i.e. num elems *)
    | NativeType_int
    | NativeType_unsigned_int
    | NativeType_method
    | NativeType_as_any
    | (* COM interop *) NativeType_bstr
    | (* COM interop *) NativeType_iunknown
    | (* COM interop *) NativeType_idsipatch
    | (* COM interop *) NativeType_interface
    | (* COM interop *) NativeType_error               
    | (* COM interop *) NativeType_safe_array of ILNativeVariantType * string option 
    | (* COM interop *) NativeType_ansi_bstr
    | (* COM interop *) NativeType_variant_bool


  and ILNativeVariantType = 
    | VariantType_empty
    | VariantType_null
    | VariantType_variant
    | VariantType_currency
    | VariantType_decimal               
    | VariantType_date               
    | VariantType_bstr               
    | VariantType_lpstr               
    | VariantType_lpwstr               
    | VariantType_iunknown               
    | VariantType_idispatch               
    | VariantType_safearray               
    | VariantType_error               
    | VariantType_hresult               
    | VariantType_carray               
    | VariantType_userdefined               
    | VariantType_record               
    | VariantType_filetime
    | VariantType_blob               
    | VariantType_stream               
    | VariantType_storage               
    | VariantType_streamed_object               
    | VariantType_stored_object               
    | VariantType_blob_object               
    | VariantType_cf                
    | VariantType_clsid
    | VariantType_void 
    | VariantType_bool
    | VariantType_int8
    | VariantType_int16                
    | VariantType_int32                
    | VariantType_int64                
    | VariantType_float32                
    | VariantType_float64                
    | VariantType_unsigned_int8                
    | VariantType_unsigned_int16                
    | VariantType_unsigned_int32                
    | VariantType_unsigned_int64                
    | VariantType_ptr                
    | VariantType_array of ILNativeVariantType                
    | VariantType_vector of ILNativeVariantType                
    | VariantType_byref of ILNativeVariantType                
    | VariantType_int                
    | VariantType_unsigned_int                

and ILSecurityAction = 
    | SecAction_request 
    | SecAction_demand
    | SecAction_assert
    | SecAction_deny
    | SecAction_permitonly
    | SecAction_linkcheck 
    | SecAction_inheritcheck
    | SecAction_reqmin
    | SecAction_reqopt
    | SecAction_reqrefuse
    | SecAction_prejitgrant
    | SecAction_prejitdeny
    | SecAction_noncasdemand
    | SecAction_noncaslinkdemand
    | SecAction_noncasinheritance
    | SecAction_linkdemandchoice
    | SecAction_inheritancedemandchoice
    | SecAction_demandchoice

and ILPermission = 
    | PermissionSet of ILSecurityAction * byte[]

and ILPermissions =
    | SecurityDecls of Lazy<ILPermission list>

and PInvokeMethod =
    { pinvokeWhere: ILModuleRef;
      pinvokeName: string;
      pinvokeCallconv: PInvokeCallingConvention;
      PInvokeCharEncoding: PInvokeCharEncoding;
      pinvokeNoMangle: bool;
      pinvokeLastErr: bool;
      PInvokeThrowOnUnmappableChar: PInvokeThrowOnUnmappableChar;
      PInvokeCharBestFit: PInvokeCharBestFit }
    member x.Where = x.pinvokeWhere
    member x.Name = x.pinvokeName
    member x.CallingConv = x.pinvokeCallconv
    member x.CharEncoding = x.PInvokeCharEncoding
    member x.NoMangle = x.pinvokeNoMangle
    member x.LastError = x.pinvokeLastErr
    member x.ThrowOnUnmappableChar = x.PInvokeThrowOnUnmappableChar
    member x.CharBestFit = x.PInvokeCharBestFit

and PInvokeCharBestFit  = 
    | PInvokeBestFitUseAssem
    | PInvokeBestFitEnabled
    | PInvokeBestFitDisabled

and PInvokeThrowOnUnmappableChar =
    | PInvokeThrowOnUnmappableCharUseAssem
    | PInvokeThrowOnUnmappableCharEnabled
    | PInvokeThrowOnUnmappableCharDisabled

and PInvokeCallingConvention =
    | PInvokeCallConvNone
    | PInvokeCallConvCdecl
    | PInvokeCallConvStdcall
    | PInvokeCallConvThiscall
    | PInvokeCallConvFastcall
    | PInvokeCallConvWinapi

and PInvokeCharEncoding =
    | PInvokeEncodingNone
    | PInvokeEncodingAnsi
    | PInvokeEncodingUnicode
    | PInvokeEncodingAuto

and ILParameter =
    { paramName: string option;
      paramType: ILType;
      paramDefault: ILFieldInit option;  
      paramMarshal: ILNativeType option; 
      paramIn: bool;
      paramOut: bool;
      paramOptional: bool;
      paramCustomAttrs: ILAttributes }
    member x.Name = x.paramName
    member x.Type = x.paramType
    member x.Default = x.paramDefault
    member x.Marshal = x.paramMarshal
    member x.IsIn = x.paramIn
    member x.IsOut = x.paramOut
    member x.IsOptional = x.paramOptional
    member x.CustomAttrs = x.paramCustomAttrs


type ILReturnValue = 
    { returnMarshal: ILNativeType option;
      returnType: ILType; 
      returnCustomAttrs: ILAttributes }
    member x.Type =  x.returnType
    member x.Marshal = x.returnMarshal
    member x.CustomAttrs = x.returnCustomAttrs

type OverridesSpec = 
    | OverridesSpec of ILMethodRef * ILType
    member x.MethodRef = let (OverridesSpec(mr,ty)) = x in mr
    member x.EnclosingType = let (OverridesSpec(mr,ty)) = x in ty

type ILMethodVirtualInfo = 
    { virtFinal: bool; 
      virtNewslot: bool; 
      virtStrict: bool; (* mdCheckAccessOnOverride *)
      virtAbstract: bool;
    }
    member x.IsFinal = x.virtFinal
    member x.IsNewSlot = x.virtNewslot
    member x.IsCheckAccessOnOverride = x.virtStrict
    member x.IsAbstract = x.virtAbstract

type MethodKind =
    | MethodKind_static 
    | MethodKind_cctor 
    | MethodKind_ctor 
    | MethodKind_nonvirtual 
    | MethodKind_virtual of ILMethodVirtualInfo

type MethodBody =
    | MethodBody_il of ILMethodBody
    | MethodBody_pinvoke of PInvokeMethod       (* platform invoke to native  *)
    | MethodBody_abstract
    | MethodBody_native

type LazyMethodBody = LazyMethodBody of Lazy<MethodBody >

type MethodCodeKind =
    | MethodCodeKind_il
    | MethodCodeKind_native
    | MethodCodeKind_runtime

let mk_mbody mb = LazyMethodBody (Lazy.CreateFromValue mb)
let dest_mbody (LazyMethodBody mb) = mb.Force()
let mk_lazy_mbody mb = LazyMethodBody mb

let typs_of_params (ps:ILParameter list) = ps |> List.map (fun p -> p.Type) 

type ILMethodDef = 
    { mdName: string;
      mdKind: MethodKind;
      mdCallconv: ILCallingConv;
      mdParams: ILParameter list;
      mdReturn: ILReturnValue;
      mdAccess: ILMemberAccess;
      mdBody: LazyMethodBody;   
      mdCodeKind: MethodCodeKind;   
      mdInternalCall: bool;
      mdManaged: bool;
      mdForwardRef: bool;
      mdSecurityDecls: ILPermissions;
      mdHasSecurity: bool;
      mdEntrypoint:bool;
      mdReqSecObj: bool;
      mdHideBySig: bool;
      mdSpecialName: bool;
      mdUnmanagedExport: bool;
      mdSynchronized: bool;
      mdPreserveSig: bool;
      mdMustRun: bool; 
      mdExport: (int32 * string option) option;
      mdVtableEntry: (int32 * int32) option;
     
      mdGenericParams: ILGenericParameterDefs;
      mdCustomAttrs: ILAttributes; }
    member x.Name = x.mdName
    member x.CallingConv = x.mdCallconv
    member x.Parameters = x.mdParams
    member x.ParameterTypes = typs_of_params x.mdParams
    member x.Return = x.mdReturn
    member x.Access = x.mdAccess
    member x.IsInternalCall = x.mdInternalCall
    member x.IsManaged = x.mdManaged
    member x.IsForwardRef = x.mdForwardRef
    member x.SecurityDecls = x.mdSecurityDecls
    member x.HasSecurity = x.mdHasSecurity
    member x.IsEntrypoint = x.mdEntrypoint
    member x.IsReqSecObj = x.mdReqSecObj
    member x.IsHideBySig = x.mdHideBySig
    member x.IsUnmanagedExport = x.mdUnmanagedExport
    member x.IsSynchronized = x.mdSynchronized
    member x.IsPreserveSig = x.mdPreserveSig
    // Whidbey feature: SafeHandle finalizer must be run 
    member x.IsMustRun = x.mdMustRun
    member x.GenericParams = x.mdGenericParams
    member x.CustomAttrs = x.mdCustomAttrs
    member md.Code = 
          match dest_mbody md.mdBody with 
          | MethodBody_il il-> Some il.ilCode
          | _ -> None
    member x.IsIL = match dest_mbody x.mdBody with | MethodBody_il _ -> true | _ -> false
    member x.Locals = match dest_mbody x.mdBody with | MethodBody_il il -> il.ilLocals | _ -> []

    member x.MethodBody = match dest_mbody x.mdBody with MethodBody_il il -> il | _ -> failwith "ilmbody_of_mdef: not IL"

    member x.IsNoInline   = x.MethodBody.ilNoInlining  
    member x.SourceMarker = x.MethodBody.ilSource
    member x.MaxStack     = x.MethodBody.ilMaxStack  
    member x.IsZeroInit   = x.MethodBody.ilZeroInit

    member x.IsClassInitializer   = match x.mdKind with | MethodKind_cctor      -> true | _ -> false
    member x.IsConstructor        = match x.mdKind with | MethodKind_ctor       -> true | _ -> false
    member x.IsStatic             = match x.mdKind with | MethodKind_static     -> true | _ -> false
    member x.IsNonVirtualInstance = match x.mdKind with | MethodKind_nonvirtual -> true | _ -> false
    member x.IsVirtual            = match x.mdKind with | MethodKind_virtual _  -> true | _ -> false

    member x.IsFinal                = match x.mdKind with | MethodKind_virtual v -> v.virtFinal    | _ -> invalid_arg "ILMethodDef.IsFinal"
    member x.IsNewSlot              = match x.mdKind with | MethodKind_virtual v -> v.virtNewslot  | _ -> invalid_arg "ILMethodDef.IsNewSlot"
    member x.IsCheckAccessOnOverride= match x.mdKind with | MethodKind_virtual v -> v.virtStrict   | _ -> invalid_arg "ILMethodDef.IsCheckAccessOnOverride"
    member x.IsAbstract             = match x.mdKind with | MethodKind_virtual v -> v.virtAbstract | _ -> invalid_arg "ILMethodDef.IsAbstract"

/// Index table by name and arity. 
type ILMethodDefs = Methods of Lazy<ILMethodDef list * MethodDefMap>
and MethodDefMap = Map<string, ILMethodDef list>

type ILEventDef =
    { eventType: ILType option; 
      eventName: string;
      eventRTSpecialName: bool;
      eventSpecialName: bool;
      eventAddOn: ILMethodRef; 
      eventRemoveOn: ILMethodRef;
      eventFire: ILMethodRef option;
      eventOther: ILMethodRef list;
      eventCustomAttrs: ILAttributes; }
    member x.Type = x.eventType
    member x.Name = x.eventName
    member x.AddMethod = x.eventAddOn
    member x.RemoveMethod = x.eventRemoveOn
    member x.FireMethod = x.eventFire
    member x.OtherMethods = x.eventOther
    member x.CustomAttrs = x.eventCustomAttrs

(* Index table by name. *)
type ILEventDefs = Events of LazyOrderedMultiMap<string, ILEventDef>

type ILPropertyDef = 
    { propName: string;
      propRTSpecialName: bool;
      propSpecialName: bool;
      propSet: ILMethodRef option;
      propGet: ILMethodRef option;
      propCallconv: ILThisConvention;
      propType: ILType;
      propInit: ILFieldInit option;
      propArgs: ILType list;
      propCustomAttrs: ILAttributes; }
    member x.Name = x.propName
    member x.SetMethod = x.propSet
    member x.GetMethod = x.propGet
    member x.CallingConv = x.propCallconv
    member x.Type = x.propType
    member x.Init = x.propInit
    member x.Args = x.propArgs
    member x.CustomAttrs = x.propCustomAttrs
    
// Index table by name.
type PropertyDefs = Properties of LazyOrderedMultiMap<string, ILPropertyDef>

type ILFieldDef = 
    { fdName: string;
      fdType: ILType;
      fdStatic: bool;
      fdAccess: ILMemberAccess;
      fdData:  byte[] option;
      fdInit:  ILFieldInit option;
      fdOffset:  int32 option; (* -- the explicit offset in bytes *)
      fdSpecialName: bool;
      fdMarshal: ILNativeType option; 
      fdNotSerialized: bool;
      fdLiteral: bool ;
      fdInitOnly: bool;
      fdCustomAttrs: ILAttributes; }
    member x.Name = x.fdName
    member x.Type = x.fdType
    member x.IsStatic = x.fdStatic
    member x.Access = x.fdAccess
    member x.Data = x.fdData
    member x.LiteralValue = x.fdInit
    /// The explicit offset in bytes when explicit layout is used.
    member x.Offset = x.fdOffset
    member x.Marshal = x.fdMarshal
    member x.NotSerialized = x.fdNotSerialized
    member x.IsLiteral = x.fdLiteral
    member x.IsInitOnly = x.fdInitOnly
    member x.CustomAttrs = x.fdCustomAttrs


// Index table by name.  Keep a canonical list to make sure field order is not disturbed for binary manipulation.
type ILFieldDefs = 
    | Fields of LazyOrderedMultiMap<string, ILFieldDef>

type ILMethodImplDef =
    { mimplOverrides: OverridesSpec;
      mimplOverrideBy: ILMethodSpec }

// Index table by name and arity. 
type ILMethodImplDefs = 
    | MethodImpls of Lazy<MethodImplsMap>
and MethodImplsMap = Map<string * int, ILMethodImplDef list>

type ILTypeDefLayout =
    | TypeLayout_auto
    | TypeLayout_sequential of ILTypeDefLayoutInfo
    | TypeLayout_explicit of ILTypeDefLayoutInfo (* REVIEW: add field info here *)

and ILTypeDefLayoutInfo =
    { typeSize: int32 option;
      typePack: uint16 option } 
    member x.Size = x.typeSize
    member x.Pack = x.typePack

type ILTypeDefInitSemantics =
    | TypeInit_beforefield
    | TypeInit_beforeany

type ILDefaultPInvokeEncoding =
    | TypeEncoding_ansi
    | TypeEncoding_autochar
    | TypeEncoding_unicode

type ILTypeDefAccess =
    | TypeAccess_public 
    | TypeAccess_private
    | TypeAccess_nested of ILMemberAccess 

type ILTypeDefKind =
    | TypeDef_class
    | TypeDef_valuetype
    | TypeDef_interface
    | TypeDef_enum 
    | TypeDef_delegate
    | TypeDef_other of IlxExtensionTypeKind

and IlxExtensionTypeKind = Ext_type_def_kind of obj


type ILTypeDef =  
    { tdKind: ILTypeDefKind;
      tdName: string;  
      tdGenericParams: ILGenericParameterDefs;   (* class is generic *)
      tdAccess: ILTypeDefAccess;  
      tdAbstract: bool;
      tdSealed: bool; 
      tdSerializable: bool; 
      tdComInterop: bool; (* Class or interface generated for COM interop *) 
      tdLayout: ILTypeDefLayout;
      tdSpecialName: bool;
      tdEncoding: ILDefaultPInvokeEncoding;
      tdNested: ILTypeDefs;
      tdImplements: ILType list;  
      tdExtends: ILType option; 
      tdMethodDefs: ILMethodDefs;
      tdSecurityDecls: ILPermissions;
      tdHasSecurity: bool;
      tdFieldDefs: ILFieldDefs;
      tdMethodImpls: ILMethodImplDefs;
      tdInitSemantics: ILTypeDefInitSemantics;
      tdEvents: ILEventDefs;
      tdProperties: PropertyDefs;
      tdCustomAttrs: ILAttributes; }
    member x.IsClass=     (match x.tdKind with TypeDef_class -> true | _ -> false)
    member x.IsValueType= (match x.tdKind with TypeDef_valuetype -> true | _ -> false)
    member x.IsInterface= (match x.tdKind with TypeDef_interface -> true | _ -> false)
    member x.IsEnum=      (match x.tdKind with TypeDef_enum -> true | _ -> false)
    member x.IsDelegate=  (match x.tdKind with TypeDef_delegate -> true | _ -> false)
    member x.Name = x.tdName
    member x.GenericParams = x.tdGenericParams
    member x.Access = x.tdAccess
    member x.IsAbstract = x.tdAbstract
    member x.IsSealed = x.tdSealed
    member x.IsSerializable = x.tdSerializable
    member x.IsComInterop = x.tdComInterop
    member x.Layout = x.tdLayout
    member x.IsSpecialName = x.tdSpecialName
    member x.Encoding = x.tdEncoding
    member x.NestedTypes = x.tdNested
    member x.Implements = x.tdImplements
    member x.Extends = x.tdExtends
    member x.Methods = x.tdMethodDefs
    member x.SecurityDecls = x.tdSecurityDecls
    member x.HasSecurity = x.tdHasSecurity
    member x.Fields = x.tdFieldDefs
    member x.MethodImpls = x.tdMethodImpls
    member x.InitSemantics = x.tdInitSemantics
    member x.Events = x.tdEvents
    member x.Properties = x.tdProperties
    member x.CustomAttrs = x.tdCustomAttrs

and ILTypeDefs = 
    | TypeDefTable of Lazy<(string list * string * ILAttributes * Lazy<ILTypeDef>) array> * Lazy<TypeDefsMap>
    
/// keyed first on namespace then on type name.  The namespace is often a unique key for a given type map.
and TypeDefsMap = 
     Map<string list,Dictionary<string,Lazy<ILTypeDef>>>

and NamespaceAndTypename = string list * string

type ILNestedExportedType =
    { nestedExportedTypeName: string;
      nestedExportedTypeAccess: ILMemberAccess;
      nestedExportedTypeNested: ILNestedExportedTypes;
      nestedExportedTypeCustomAttrs: ILAttributes } 

and ILNestedExportedTypes = ILNestedExportedTypes of Lazy<NestedExportedTypesMap>
and NestedExportedTypesMap = Map<string,ILNestedExportedType>

and ILExportedType =
    { exportedTypeScope: ILScopeRef;
      exportedTypeName: string;
      exportedTypeForwarder: bool;
      exportedTypeAccess: ILTypeDefAccess;
      exportedTypeNested: ILNestedExportedTypes;
      exportedTypeCustomAttrs: ILAttributes } 
    member x.ScopeRef = x.exportedTypeScope
    member x.Name = x.exportedTypeName
    member x.IsForwarder = x.exportedTypeForwarder
    member x.Access = x.exportedTypeAccess
    member x.Nested = x.exportedTypeNested
    member x.CustomAttrs = x.exportedTypeCustomAttrs

and ILExportedTypes = ILExportedTypes of Lazy<ExportedTypesMap>
and ExportedTypesMap = Map<string,ILExportedType>

type ILResourceAccess = 
    | Resource_public 
    | Resource_private 

type ILResourceLocation =
    | Resource_local of (unit -> byte[])
    | Resource_file of ILModuleRef * int32
    | Resource_assembly of ILAssemblyRef

type ILResource =
    { resourceName: string;
      resourceWhere: ILResourceLocation;
      resourceAccess: ILResourceAccess;
      resourceCustomAttrs: ILAttributes }
    member x.Name = x.resourceName
    member x.Location = x.resourceWhere
    member x.Access = x.resourceAccess
    member x.CustomAttrs = x.resourceCustomAttrs

type ILResources = ILResources of Lazy<ILResource list>

(* -------------------------------------------------------------------- 
 * One module in the "current" assembly
 * -------------------------------------------------------------------- *)

type ILAssemblyLongevity =
    | LongevityUnspecified
    | LongevityLibrary
    | LongevityPlatformAppDomain
    | LongevityPlatformProcess
    | LongevityPlatformSystem


type ILAssemblyManifest = 
    { manifestName: string;
      manifestAuxModuleHashAlgorithm: int32;
      manifestSecurityDecls: ILPermissions;
      manifestPublicKey: byte[] option;
      manifestVersion: ILVersionInfo option;
      manifestLocale: Locale option;
      manifestCustomAttrs: ILAttributes;

      manifestLongevity: ILAssemblyLongevity; 
      manifestDisableJitOptimizations: bool;
      manifestJitTracking: bool;
      manifestRetargetable: bool;

      manifestExportedTypes: ILExportedTypes;
               (* -- Records the types impemented by other modules. *)
      manifestEntrypointElsewhere: ILModuleRef option; 
               (* -- Records whether the entrypoint resides in another module. *)
    } 
    member x.Name = x.manifestName
    member x.AuxModuleHashAlgorithm = x.manifestAuxModuleHashAlgorithm
    member x.SecurityDecls = x.manifestSecurityDecls
    member x.PublicKey = x.manifestPublicKey
    member x.Version = x.manifestVersion
    member x.Locale = x.manifestLocale
    member x.CustomAttrs = x.manifestCustomAttrs
    member x.AssemblyLongevity = x.manifestLongevity
    member x.DisableJitOptimizations = x.manifestDisableJitOptimizations
    member x.JitTracking = x.manifestJitTracking
    member x.Retargetable = x.manifestRetargetable
    member x.ExportedTypes = x.manifestExportedTypes
    member x.EntrypointElsewhere = x.manifestEntrypointElsewhere

type ILModuleDef = 
    { modulManifest: ILAssemblyManifest option;
      modulCustomAttrs: ILAttributes;
      modulName: string;
      modulTypeDefs: ILTypeDefs;
      (* Random bits of relatively uninteresting data *)
      modulSubSystem: int32;
      modulDLL: bool;
      modulILonly: bool;
      modulPlatform: ILPlatform option; 
      modul32bit: bool;
      modul64bit: bool;
      modulVirtAlignment: int32;
      modulPhysAlignment: int32;
      modulImageBase: int32;
      modulResources: ILResources;
      modulNativeResources: list<Lazy<byte[]>>; (* e.g. win32 resources *)
    }
    member x.Manifest = x.modulManifest
    member x.CustomAttrs = x.modulCustomAttrs
    member x.Name = x.modulName
    member x.TypeDefs = x.modulTypeDefs
    member x.SubSystemFlags = x.modulSubSystem
    member x.IsDLL = x.modulDLL
    member x.IsILOnly = x.modulILonly
    member x.Platform = x.modulPlatform
    member x.Is32Bit = x.modul32bit
    member x.Is64Bit = x.modul64bit
    member x.VirtualAlignment = x.modulVirtAlignment
    member x.PhysicalAlignment = x.modulPhysAlignment
    member x.ImageBase = x.modulImageBase
    member x.Resources = x.modulResources
    member x.NativeResources = x.modulNativeResources

    member x.ManifestOfAssembly = 
        match x.modulManifest with 
        | Some m -> m
        | None -> failwith "no manifest.  It is possible you are using an auxiliary module of an assembly in a context where the main module of an assembly is expected.  Typically the main module of an assembly must be specified first within a list of the modules in an assembly."



// -------------------------------------------------------------------- 
// Utilities: type names
// -------------------------------------------------------------------- 

let split_name_at nm idx = 
    if idx < 0 then failwith "split_name_at: idx < 0";
    let last = String.length nm - 1 
    if idx > last then failwith "split_name_at: idx > last";
    (nm.Substring(0,idx)),
    (if idx < last then nm.Substring (idx+1,last - idx) else "")

let rec split_namespace_aux (nm:string) = 
    match nm.IndexOf '.' with 
    | -1 -> [nm]
    | idx -> 
        let s1,s2 = split_name_at nm idx 
        s1::split_namespace_aux s2 

/// Global State. All namespace splits
let memoize_namespace_tab = 
    Dictionary<string,string list>(10)

let split_namespace nm =
    let mutable res = Unchecked.defaultof<_>
    let ok = memoize_namespace_tab.TryGetValue(nm,&res)
    if ok then res else
    let x = split_namespace_aux nm
    (memoize_namespace_tab.[nm] <- x; x)

let split_namespace_memoized nm = split_namespace nm

// REVIEW: CONCURRENCY: lock this table.
let memoize_namespace_array_tab = 
    Dictionary<string,_>(10)

let split_namespace_array nm =
    let mutable res = Unchecked.defaultof<_>
    let ok = memoize_namespace_array_tab.TryGetValue(nm,&res)
    if ok then res else
    let x = Array.of_list (split_namespace nm)
    (memoize_namespace_array_tab.[nm] <- x; x)


let split_type_name (nm:string) = 
    match nm.LastIndexOf '.' with
    | -1 -> [],nm
    | idx -> 
        let s1,s2 = split_name_at nm idx
        split_namespace s1,s2

let emptyStringArray = ([| |] : string[])
let split_type_name_array (nm:string) = 
    match nm.LastIndexOf '.' with
    | -1 -> emptyStringArray,nm
    | idx -> 
        let s1,s2 = split_name_at nm idx
        split_namespace_array s1,s2

let unsplit_type_name (ns,n) = 
    match ns with 
    | [] -> String.concat "." ns ^"."^n 
    | _ -> n 

// -------------------------------------------------------------------- 
// Add fields and types to tables, with decent error messages
// when clashes occur...
// -------------------------------------------------------------------- 



let dest_fdefs (Fields t) = t.Entries()
let dest_edefs (Events t) = t.Entries()
let dest_pdefs (Properties t) = t.Entries()
let dest_exported_types (ILExportedTypes ltab) = Map.foldBack (fun x y r -> y::r) (ltab.Force()) []
let dest_nested_exported_types (ILNestedExportedTypes ltab) = Map.foldBack (fun x y r -> y::r) (ltab.Force()) []
let dest_resources (ILResources ltab) = (ltab.Force())
let dest_mimpls (MethodImpls ltab) = Map.foldBack (fun x y r -> y@r) (ltab.Force()) []
let dest_lazy_tdefs (TypeDefTable (larr,tab)) = larr.Force() |> Array.to_list
let dest_tdefs tdefs = tdefs |> dest_lazy_tdefs |> List.map (fun (_,_,_,td) -> td.Force()) 
let dest_custom_attrs (CustomAttrs m) = m.Force()
let dest_security_decls (SecurityDecls m) = m.Force()

let find_tdef x (TypeDefTable (_,m)) = 
    let ns,n = split_type_name x
    m.Force().[ns].[n].Force()

let find_fdefs x (Fields t) = t.[x]
let find_edefs x (Events t) = t.[x]
let find_pdefs x (Properties t) = t.[x]
let find_exported_type x (ILExportedTypes ltab) = Map.find x (ltab.Force())

let mk_empty_gparams = ([]: ILGenericParameterDefs)
let mk_empty_gactuals = ([]: ILGenericArgs)


type ILType with
    member x.TypeSpec =
      match x with 
      | Type_boxed tr | Type_value tr -> tr
      | _ -> failwith "tspec_of_typ"
    member x.Boxity =
      match x with 
      | Type_boxed _ -> AsObject
      | Type_value _ -> AsValue
      | _ -> failwith "boxity_of_typ"
    member x.TypeRef = 
      match x with 
      | Type_boxed tspec | Type_value tspec -> tspec.TypeRef
      | _ -> failwith "tref_of_typ"
    member x.IsNominal = 
      match x with 
      | Type_boxed tr | Type_value tr -> true
      | _ -> false
    member x.GenericArgs =
      match x with 
      | Type_boxed tspec | Type_value tspec -> tspec.GenericArgs
      | _ -> mk_empty_gactuals
    member x.IsTyvar =
      match x with 
      | Type_tyvar _ -> true | _ -> false


#if STANDALONE_METADATA
#else

// -------------------------------------------------------------------- 
// Helpers for the ILX extensions
// -------------------------------------------------------------------- 

type internal_instr_extension = 
    { internalInstrExtIs: IlxExtensionInstr -> bool; 
      internalInstrExtDests: IlxExtensionInstr -> ILCodeLabel list;
      internalInstrExtFallthrough: IlxExtensionInstr -> ILCodeLabel option;
      internalInstrExtIsTailcall: IlxExtensionInstr -> bool;
      internalInstrExtRelabel: (ILCodeLabel -> ILCodeLabel) -> IlxExtensionInstr -> IlxExtensionInstr; }

type internal_type_def_kind_extension = 
    { internalTypeDefKindExtIs: IlxExtensionTypeKind -> bool; }

type 'a ILInstrSetExtension = 
    { instrExtDests: 'a -> ILCodeLabel list;
      instrExtFallthrough: 'a -> ILCodeLabel option;
      instrExtIsTailcall: 'a -> bool;
      instrExtRelabel: (ILCodeLabel -> ILCodeLabel) -> 'a -> 'a; }

let instr_extensions = ref []
let type_def_kind_extensions = ref []

let define_instr_extension  (ext: 'a ILInstrSetExtension) = 
    if nonNil !instr_extensions then failwith "define_instr_extension: only one extension currently allowed";
    let mk (x: 'a) = Ext_instr (box x)
    let test (Ext_instr x) = true
    let dest (Ext_instr x) = (unbox x : 'a)
    instr_extensions := 
       { internalInstrExtIs=test;
         internalInstrExtDests=(fun x -> ext.instrExtDests (dest x));
         internalInstrExtFallthrough=(fun x -> ext.instrExtFallthrough (dest x));
         internalInstrExtIsTailcall=(fun x -> ext.instrExtIsTailcall (dest x));
         internalInstrExtRelabel=(fun f x -> mk (ext.instrExtRelabel f (dest x))); }
         :: !instr_extensions;
    mk,test,dest

type 'a ILTypeDefKindExtension = 
    | Type_def_kind_extension

let define_type_def_kind_extension (Type_def_kind_extension : 'a ILTypeDefKindExtension) = 
    if nonNil !type_def_kind_extensions then failwith "define_type_extension: only one extension currently allowed";
    let mk (x:'a) = Ext_type_def_kind (box x)
    let test (Ext_type_def_kind x) = true
    let dest (Ext_type_def_kind x) = (unbox x: 'a)
    type_def_kind_extensions := 
       { internalTypeDefKindExtIs=test;}
         :: !type_def_kind_extensions;
    mk,test,dest

// -------------------------------------------------------------------- 
// Making assembly, module and file references
// -------------------------------------------------------------------- 

let mk_simple_assref n = 
  ILAssemblyRef.Create(n, None, None, false, None, None)

let mk_simple_modref n = 
    ILModuleRef.Create(n, true, None)

let scoref_for_modname modul = ScopeRef_module(mk_simple_modref modul)

let module_name_of_scoref = function 
    | ScopeRef_module(mref) -> mref.Name
    | _ -> failwith "module_name_of_scoref"

let module_is_mainmod m =
    match m.modulManifest with None -> false | _ -> true


let assname_of_mainmod (mainmod:ILModuleDef) = mainmod.ManifestOfAssembly.manifestName

// -------------------------------------------------------------------- 
// Types
// -------------------------------------------------------------------- 


let is_tyvar_ty = function Type_tyvar _ -> true | _ -> false
let tspec_of_typ (ty:ILType) = ty.TypeSpec
let boxity_of_typ (ty:ILType) = ty.Boxity
let tref_of_typ (ty:ILType) = ty.TypeRef
let is_tref_typ (ty:ILType) = ty.IsNominal
let inst_of_typ (ty:ILType) = ty.GenericArgs

let mk_typ boxed tspec = 
  match boxed with AsObject -> Type_boxed tspec | _ -> Type_value tspec

let mk_named_typ vc tref tinst = mk_typ vc (ILTypeSpec.Create(tref, tinst))

let mk_value_typ tref tinst = mk_named_typ AsValue tref tinst
let mk_boxed_typ tref tinst = mk_named_typ AsObject tref tinst

let mk_nongeneric_value_typ tref = mk_named_typ AsValue tref []
let mk_nongeneric_boxed_typ tref = mk_named_typ AsObject tref []


// --------------------------------------------------------------------
// Make references to ILMethodDefs
// -------------------------------------------------------------------- 

let mk_nested_tref (scope,l,nm) =  ILTypeRef.Create(scope,l,nm)
let mk_tref (scope,nm) =  mk_nested_tref (scope,[],nm)
let mk_tspec (tref,inst) =  ILTypeSpec.Create(tref, inst)
let mk_nongeneric_tspec tref =  mk_tspec (tref,[])

let mk_tref_in_tref (tref:ILTypeRef,nm) = 
  mk_nested_tref (tref.Scope,tref.Enclosing@[tref.Name],nm)

// --------------------------------------------------------------------
// The toplevel class of a module is called "<Module>"
//
// REVIEW: the  following comments from the ECMA Spec (Parition II, Section 9.8)
//
// "For an ordinary type, if the metadata merges two definitions 
// of the same type, it simply discards one definition on the 
// assumption they are equivalent and that any anomaly will be 
// discovered when the type is used.  For the special class that 
// holds global members, however, members are unioned across all 
// modules at merge time. If the same name appears to be defined 
// for cross-module use in multiple modules then there is an 
// error.  In detail:
//  - If no member of the same kind (field or method), name, and 
//    signature exists, then add this member to the output class.
//  - If there are duplicates and no more than one has an 
//    accessibility other than compilercontrolled, then add them 
//    all in the output class.
//  - If there are duplicates and two or more have an accessibility 
//    other than compilercontrolled an error has occurred."
// -------------------------------------------------------------------- 

let tname_for_toplevel = "<Module>"

let tref_for_toplevel scoref = ILTypeRef.Create(scoref,[],tname_for_toplevel)

let tspec_for_toplevel scoref = mk_nongeneric_tspec (tref_for_toplevel scoref)

let typ_for_toplevel scorefs = Type_boxed (tspec_for_toplevel scorefs)

let is_toplevel_tname d = (d = tname_for_toplevel)

let mk_mref (tref,callconv,nm,gparams,args,rty) =
  { mrefParent=tref; 
    mrefCallconv=callconv;
    mrefGenericArity=gparams;
    mrefName=nm;
    mrefArgs=args;
    mrefReturn=rty}

let mk_mref_mspec_in_typ (mref,typ,minst) = 
  { mspecMethodRefF=mref;
    mspecEnclosingTypeF=typ;
    mspecMethodInstF=minst }

let mk_mspec (mref, vc, tinst, minst) =mk_mref_mspec_in_typ (mref,mk_named_typ vc mref.EnclosingTypeRef tinst,minst)

let mk_mspec_in_tref (tref,vc,cc,nm,args,rty,tinst,minst) =
  mk_mspec (mk_mref ( tref,cc,nm,List.length minst,args,rty),vc,tinst,minst)

let mk_mspec_in_tspec (tspec:ILTypeSpec,vc,cc,nm,args,rty,minst) =
  mk_mspec_in_tref (tspec.TypeRef,vc,cc,nm,args,rty,tspec.GenericArgs,minst)

let mk_nongeneric_mspec_in_tspec (tspec,vc,cc,nm,args,rty) =
  mk_mspec_in_tspec (tspec,vc,cc,nm,args,rty,mk_empty_gactuals)

let mk_mspec_in_typ (typ,cc,nm,args,rty,minst) =
  mk_mref_mspec_in_typ (mk_mref (tref_of_typ typ,cc,nm,List.length minst,args,rty),typ,minst)

let mk_nongeneric_mspec_in_typ (typ,cc,nm,args,rty) = 
  mk_mspec_in_typ (typ,cc,nm,args,rty,mk_empty_gactuals)

let mk_instance_mspec_in_tref (tref,vc,nm,args,rty,cinst,minst) =
  mk_mspec_in_tref (tref,vc,ILCallingConv.Instance,nm,args,rty,cinst,minst)

let mk_instance_mspec_in_tspec (tspec:ILTypeSpec,vc,nm,args,rty,minst) =
  mk_instance_mspec_in_tref (tspec.TypeRef, vc,nm,args,rty,tspec.GenericArgs,minst)

let mk_instance_mspec_in_typ (typ,nm,args,rty,minst) =
  mk_instance_mspec_in_tspec (tspec_of_typ typ, boxity_of_typ typ,nm,args,rty,minst)

let mk_instance_mspec_in_boxed_tspec (tspec,nm,args,rty,minst) =
  mk_instance_mspec_in_tspec (tspec,AsObject,nm,args,rty,minst)

let mk_instance_mspec_in_nongeneric_boxed_tref(tref,nm,args,rty,minst) =
  mk_instance_mspec_in_boxed_tspec (mk_nongeneric_tspec tref,nm,args,rty,minst)

let mk_nongeneric_instance_mspec_in_tref (tref,vc,nm,args,rty,cinst) =
  mk_instance_mspec_in_tref (tref,vc,nm,args,rty,cinst,mk_empty_gactuals)

let mk_nongeneric_instance_mspec_in_tspec (tspec:ILTypeSpec,vc,nm,args,rty) =
  mk_nongeneric_instance_mspec_in_tref (tspec.TypeRef,vc,nm,args,rty,tspec.GenericArgs)
let mk_nongeneric_instance_mspec_in_typ (typ,nm,args,rty) =
  mk_nongeneric_instance_mspec_in_tspec (tspec_of_typ typ,boxity_of_typ typ,nm,args,rty)

let mk_nongeneric_instance_mspec_in_boxed_tspec (tspec,nm,args,rty) =
  mk_nongeneric_instance_mspec_in_tspec(tspec,AsObject,nm,args,rty)

let mk_nongeneric_instance_mspec_in_nongeneric_boxed_tref(tref,nm,args,rty) =
  mk_nongeneric_instance_mspec_in_boxed_tspec (mk_nongeneric_tspec tref,nm,args,rty)

let mk_nongeneric_mspec_in_tref (tref,vc,cc,nm,args,rty,cinst) =
  mk_mspec (mk_mref (tref,cc,nm,0,args,rty),vc,cinst,mk_empty_gactuals)

let mk_nongeneric_mspec_in_nongeneric_tref (tref,vc,callconv,nm,args,rty) =
  mk_nongeneric_mspec_in_tref (tref,vc,callconv,nm,args,rty,mk_empty_gactuals)

let mk_static_mref_in_tref (tref,nm,gparams,args,rty) =
  mk_mref(tref,ILCallingConv.Static,nm,gparams,args,rty)

let mk_static_mspec_in_nongeneric_boxed_tref (tref,nm,args,rty,minst) =
  mk_mspec_in_tref (tref,AsObject,ILCallingConv.Static,nm,args,rty,mk_empty_gactuals,minst)

let mk_static_mspec_in_boxed_tspec (tspec,nm,args,rty,minst) =
  mk_mspec_in_tspec (tspec,AsObject,ILCallingConv.Static,nm,args,rty,minst)

let mk_static_mspec_in_typ (typ,nm,args,rty,minst) =
  mk_mspec_in_typ (typ,ILCallingConv.Static,nm,args,rty,minst)

let mk_static_nongeneric_mspec_in_nongeneric_boxed_tref (tref,nm,args,rty) =
  mk_static_mspec_in_nongeneric_boxed_tref (tref,nm,args,rty,mk_empty_gactuals)

let mk_static_nongeneric_mspec_in_boxed_tspec (tspec,nm,args,rty) =
  mk_static_mspec_in_boxed_tspec (tspec,nm,args,rty,mk_empty_gactuals)

let mk_static_nongeneric_mspec_in_typ (typ,nm,args,rty) =
  mk_static_mspec_in_typ (typ,nm,args,rty,mk_empty_gactuals)

let mk_toplevel_static_mref scoref (nm,args,rty,gparams) =
  mk_static_mref_in_tref ( (tref_for_toplevel scoref),nm,args,rty,gparams)

let mk_toplevel_static_mspec scoref (nm,args,rty,minst) =
  mk_static_mspec_in_nongeneric_boxed_tref (tref_for_toplevel scoref,nm,args,rty,minst)

let mk_toplevel_static_nongeneric_mspec scoref (nm,args,rty) =
 mk_toplevel_static_mspec scoref (nm,args,rty,mk_empty_gactuals)

let mk_ctor_mspec (tref,vc,args,cinst) = 
  mk_mspec_in_tref(tref,vc,ILCallingConv.Instance,".ctor",args,Type_void,cinst, mk_empty_gactuals)

let mk_ctor_mspec_for_typ (ty,args) = 
  mk_mspec_in_typ(ty,ILCallingConv.Instance,".ctor",args,Type_void, mk_empty_gactuals)

let mk_nongeneric_ctor_mspec (tref,vc,args) = 
  mk_ctor_mspec (tref,vc,args,mk_empty_gactuals)

let mk_ctor_mspec_for_boxed_tspec (tspec:ILTypeSpec,argtys) =
  mk_ctor_mspec(tspec.TypeRef,AsObject,argtys, tspec.GenericArgs)

let mk_ctor_mspec_for_nongeneric_boxed_tref (tr,argtys) =
  mk_ctor_mspec(tr,AsObject,argtys, mk_empty_gactuals)

// --------------------------------------------------------------------
// Make references to fields
// -------------------------------------------------------------------- 

let mk_fref_in_tref(tref,nm,ty) = 
  { frefParent=tref;
    frefName=nm; 
    frefType=ty}

let mk_fspec (tref,ty) = 
  { fspecFieldRef= tref;
    fspecEnclosingType=ty }

let mk_fspec_in_tspec (tspec:ILTypeSpec,boxity,nm,ty) =
  mk_fspec (mk_fref_in_tref (tspec.TypeRef,nm,ty), mk_typ boxity tspec)
    
let mk_fspec_in_typ (typ,nm,fty) = 
  mk_fspec (mk_fref_in_tref ((tspec_of_typ typ).TypeRef,nm,fty), typ)
    
let mk_fspec_in_boxed_tspec (tspec,nm,ty) = 
  mk_fspec_in_tspec (tspec,AsObject,nm,ty) 
    
let mk_fspec_in_nongeneric_boxed_tref (tref,nm,ty) =
  mk_fspec_in_tspec (mk_nongeneric_tspec tref, AsObject,nm,ty)
    
let add_custom_attr_to_tab ca tab =  ca::tab
let mk_custom_attrs l = CustomAttrs (Lazy.CreateFromValue(List.foldBack add_custom_attr_to_tab l []))
let mk_computed_custom_attrs l = CustomAttrs (Lazy.Create l)

let and_tailness x y = 
  match x with Tailcall when y -> Tailcall | _ -> Normalcall

// -------------------------------------------------------------------- 
// ILAttributes on code blocks (esp. debug info)
// -------------------------------------------------------------------- 

let code_label_eq (x:ILCodeLabel) y = (x = y)
let string_of_code_label (x:int) = "L"^string x

module CodeLabels = 
    let insert (e:ILCodeLabel) l = Zset.add e l
    let remove e l = Zset.remove e l
    let fold f s acc = Zset.fold f s acc
    let add s x = Zset.add s x
    let addList s xs = Zset.addList s xs
    let diff l1 l2 = Zset.diff l1 l2
    let union l1 l2 = Zset.union l1 l2
    let inter (l1:ILCodeLabel Zset.t) l2 = Zset.inter l1 l2
    let subset (l1:ILCodeLabel Zset.t) l2 = Zset.subset l1 l2
    let empty = Zset.empty int_order
    let is_non_empty s = not (Zset.is_empty s)
    let of_list l = Zset.addList l empty
    let to_list l = Zset.elements l

// -------------------------------------------------------------------- 
// Basic operations on code.
// -------------------------------------------------------------------- 


let label_of_bblock b = b.bblockLabel

let instrs_of_bblock bk = bk.bblockInstrs

let last_of_bblock bb = 
  let n = Array.length bb.bblockInstrs
  if n = 0 then failwith "last_of_bblock: empty bblock";
  bb.bblockInstrs.[n - 1]

let rec find_extension s f l = 
  let rec look l1 = 
    match l1 with
    | [] -> failwith ("extension for "^s^" not found")
    | (h::t) -> match f h with None -> look t | Some res -> res 
  look l
          
let destinations_of_instr i = 
  match i with 
  | I_leave l | I_br l -> [l]
  | I_brcmp (_,l1,l2) -> [l1; l2]
  | I_switch (ls,l) -> CodeLabels.to_list (CodeLabels.of_list (l::ls))
  | I_endfinally | I_endfilter | I_ret | I_throw | I_rethrow 
  | I_call (Tailcall,_,_)| I_callvirt (Tailcall,_,_)| I_callconstraint (Tailcall,_,_,_)
  | I_calli (Tailcall,_,_) -> []
  | I_other e -> find_extension "instr" (fun ext -> if ext.internalInstrExtIs e then Some (ext.internalInstrExtDests e) else None) !instr_extensions
  | _ -> []

let destinations_of_bblock (bblock:ILBasicBlock) = destinations_of_instr (last_of_bblock bblock)

let fallthrough_of_bblock (bblock:ILBasicBlock) = 
  begin match last_of_bblock bblock with 
  | I_br l | I_brcmp (_,_,l) | I_switch (_,l) -> Some l
  | I_other e -> find_extension "instr" (fun ext -> if ext.internalInstrExtIs e then Some (ext.internalInstrExtFallthrough e) else None) !instr_extensions
  | _ -> None
  end

let instr_is_tailcall i = 
  match i with 
  | I_call (Tailcall,_,_)| I_callvirt (Tailcall,_,_) | I_callconstraint (Tailcall,_,_,_) | I_calli (Tailcall,_,_) -> true
  | I_other e -> find_extension "instr" (fun ext -> if ext.internalInstrExtIs e then Some (ext.internalInstrExtIsTailcall e) else None) !instr_extensions
  | _ -> false

let instr_is_bblock_end i = 
  instr_is_tailcall i or
  match i with 
  | I_leave _ | I_br _ | I_brcmp _ | I_switch _ | I_endfinally
  | I_endfilter | I_ret | I_throw | I_rethrow  ->  true
  | I_other e -> find_extension "instr" (fun ext -> if ext.internalInstrExtIs e then Some (nonNil (ext.internalInstrExtDests e)) else None) !instr_extensions
  | _ -> false

let checks = false 
let _ = if checks then dprintn "Warning - Il.checks is on"

let rec acc_entries_of_code c acc =
  match c with
  | ILBasicBlock bb -> CodeLabels.add bb.bblockLabel acc
  | GroupBlock (_,l) -> List.foldBack acc_entries_of_code l acc
  | RestrictBlock (ls,c) -> CodeLabels.union acc (CodeLabels.diff (entries_of_code' c) (CodeLabels.of_list ls))
  | TryBlock (l,r) -> acc_entries_of_code l acc
and entries_of_code' c = acc_entries_of_code c CodeLabels.empty 

let rec acc_exits_of_code c acc =
  let basic_outside_labels = 
    match c with
    | ILBasicBlock bblock -> CodeLabels.addList (destinations_of_bblock bblock) acc
    | GroupBlock (_,l) -> List.foldBack acc_exits_of_code l acc
    | RestrictBlock (ls,c) ->  CodeLabels.union acc (CodeLabels.diff (exits_of_code' c) (CodeLabels.of_list ls))
    | TryBlock (l,r) -> acc_exits_of_code l acc
  CodeLabels.diff basic_outside_labels (entries_of_code' c)
and exits_of_code' c = acc_exits_of_code c CodeLabels.empty

let entries_of_code c = CodeLabels.to_list (entries_of_code' c)
let exits_of_code c = CodeLabels.to_list (exits_of_code' c)

/// Finds all labels defined within this code block, seeing through restrictions.
/// This assumes that labels are unique within the code blocks, even if hidden behind restrictions.
///
// Note: Repeats in the list indicate this invariant is broken.
let rec acc_labels_of_code acc c =
  match c with
  | ILBasicBlock bb        -> bb.bblockLabel::acc
  | GroupBlock (_,l)     -> List.fold acc_labels_of_code acc l 
  | RestrictBlock (ls,c) -> acc_labels_of_code acc c
  | TryBlock (l,r)       -> let acc = acc_labels_of_code acc l
                            let acc = acc_labels_of_seh  acc r
                            acc
and acc_labels_of_seh acc = function
  | FaultBlock       code   -> acc_labels_of_code acc code
  | FinallyBlock     code   -> acc_labels_of_code acc code
  | FilterCatchBlock fcodes -> List.fold acc_labels_of_fcode acc fcodes
      
and acc_labels_of_fcode acc = function
  | TypeFilter typ,code  -> acc_labels_of_code acc code
  | CodeFilter test,code -> let accA = acc_labels_of_code acc code
                            let accB = acc_labels_of_code accA test
                            accB

let labels_of_code code = acc_labels_of_code [] code

(*

From the ECMA spec:

There are only two ways to enter a try block from outside its lexical body:
 - Branching to or falling into the try block’s first instruction. The branch may be made using a 37
conditional branch, an unconditional branch, or a leave instruction. 38
 - Using a leave instruction from that try’s catch block. In this case, correct CIL code may 39
branch to any instruction within the try block, not just its first instruction, so long as that 40
branch target is not protected by yet another try, nested withing the first 
*)


let check_code code = 
    if checks then begin
        match code with
        | RestrictBlock (ls,c') -> 
            (*
              if not (CodeLabels.subset ls (entries_of_code c')) then begin
                dprintn ("* warning: Restricting labels that are not declared in block, e.g. "^ (List.hd (CodeLabels.diff ls (entries_of_code c'))));
                dprintn ("* warning: Labels in block are: "^ (String.concat "," (entries_of_code c')));
                dprintn ("* warning: Labels being restricted are: "^ (String.concat "," ls));
              end;
            *)
            let cls = (CodeLabels.inter (CodeLabels.of_list ls) (exits_of_code' c'))
            if (CodeLabels.is_non_empty cls) then 
              dprintn ("* warning: restricting unsatisfied exits from a block, e.g. "^ string_of_code_label (List.hd (CodeLabels.to_list cls)));
        | TryBlock (l,r) -> 
            begin match r with 
            | FaultBlock b | FinallyBlock b -> 
                if (CodeLabels.is_non_empty (CodeLabels.inter (exits_of_code' b) (entries_of_code' b))) then 
                  dprintn "* warning: exits from fault or finally blocks must leave the block";
                let n = List.length (entries_of_code b)
                if not (n = 1) then dprintn "* warning: zero or more than one entry to a fault or finally block";
            | FilterCatchBlock r -> 
                List.iter 
                  (fun (flt,z) -> 
                    let m = List.length (entries_of_code z)
                    if not (m = 1) then dprintn "* warning: zero or more than one entry to a catch block";
                    match flt with 
                    | CodeFilter y -> 
                        if (CodeLabels.is_non_empty (exits_of_code' y)) then dprintn "* warning: exits exist from filter block - you must always exit using endfinally";
                        let n = List.length (entries_of_code y)
                        if not (n = 1) then dprintn "* warning: zero or more than one entry to a filter block";
                    | TypeFilter ty -> ())
                  r;
            end;
        | ILBasicBlock bb ->
            if (Array.length bb.bblockInstrs) = 0 then dprintn ("* warning: basic block "^string_of_code_label bb.bblockLabel^" is empty")
            elif not (instr_is_bblock_end (bb.bblockInstrs.[Array.length bb.bblockInstrs - 1])) then failwith "* warning: bblock does not end in an appropriate instruction";
            
        | _ -> ()
    end;
    match code with 
    | RestrictBlock (labs,c) when (isNil labs) -> c 
    | GroupBlock ([],[c]) -> c 
    | _ -> code


let mk_bblock bb = ILBasicBlock bb
let mk_scope_block (a,b) = GroupBlock (a,[check_code b])
let mk_group_block_from_code (internals,codes) = RestrictBlock (internals,check_code (GroupBlock ([],codes)))
let mk_group_block (internals,blocks) = mk_group_block_from_code (internals,List.map check_code blocks)

let mk_restrict_block lab c = RestrictBlock (CodeLabels.to_list (CodeLabels.remove lab (entries_of_code' c)),c)
let mk_try_finally_block (tryblock, enter_finally_lab, finallyblock) = 
  TryBlock(check_code tryblock, FinallyBlock (check_code (mk_restrict_block enter_finally_lab (check_code finallyblock))))

let mk_try_fault_block (tryblock, enter_fault_lab, faultblock) = 
  TryBlock(check_code tryblock, FaultBlock (check_code (mk_restrict_block enter_fault_lab (check_code faultblock))))

let mk_try_multi_filter_catch_block (tryblock, clauses) = 
    TryBlock
      (check_code tryblock, 
       FilterCatchBlock 
         (clauses |> List.map (fun (flt, (enter_catch_lab, catchblock)) -> 
                let fltcode = 
                  match flt with 
                  | Choice1Of2 (enter_filter_lab, filterblock) ->
                      CodeFilter (check_code (mk_restrict_block enter_filter_lab (check_code filterblock)))
                  | Choice2Of2 ty -> 
                      TypeFilter ty
                fltcode,
                check_code (mk_restrict_block enter_catch_lab (check_code catchblock)))))


let new_generator () = 
    let i = ref 0
    fun n -> 
      incr i; !i

let code_label_generator = (new_generator () : unit -> ILCodeLabel) 
let generate_code_label x  = code_label_generator x

let unique_entry_of_code c = 
    match entries_of_code c with 
    | [] -> failwith ("unique_entry_of_code: no entries to code")
    | [inlab] -> inlab
    | labs -> failwith ("unique_entry_of_code: need one entry to code, found: "^String.concat "," (List.map string_of_code_label labs))

let unique_exit_of_code c = 
    match exits_of_code c with 
    | [] -> failwith ("unique_exit_of_code: no exits from code")
    | [outlab] -> outlab
    | labs -> failwith ("unique_exit_of_code: need one exit from code, found: "^String.concat "," (List.map string_of_code_label labs))

let nonbranching_instrs inplab instrs = 
    check_code (mk_bblock {bblockLabel=inplab; bblockInstrs= Array.of_list instrs})

let nonbranching_instrs_then inplab instrs instr = 
    if nonNil instrs && instr_is_bblock_end (List.last instrs) then failwith "nonbranching_instrs_then: bblock already terminates with a control flow instruction";
    nonbranching_instrs inplab (instrs @ [ instr ]) 

let nonbranching_instrs_then_ret inplab instrs = 
    nonbranching_instrs_then inplab instrs I_ret

let nonbranching_instrs_then_br inplab instrs lab = 
    nonbranching_instrs_then inplab instrs (I_br lab)

let nonbranching_instrs_to_code instrs = 
    let inplab = (generate_code_label ())
    if nonNil instrs && instr_is_bblock_end (List.last instrs) then 
      nonbranching_instrs inplab instrs
    else
      nonbranching_instrs_then_ret inplab  instrs

let join_code code1 code2 = 
    if not (code_label_eq (unique_exit_of_code code1) (unique_entry_of_code code2))  then 
      dprintn "* warning: join_code: exit of code1 is not entry of code 2";
    check_code 
      (RestrictBlock ([unique_exit_of_code code1], 
                      (check_code (mk_group_block ([],[ code1; code2 ])))))

(* -------------------------------------------------------------------- 
 * Security declarations (2)
 * -------------------------------------------------------------------- *)

let add_security_decl_to_tab sd tab =  sd::tab
let mk_security_decls l = SecurityDecls (notlazy (List.foldBack add_security_decl_to_tab l []))
let mk_lazy_security_decls l = SecurityDecls (lazy (List.foldBack add_security_decl_to_tab (Lazy.force l) []))

(* --------------------------------------------------------------------
 * ILX stuff
 * -------------------------------------------------------------------- *)

let mk_tyvar_ty tv = Type_tyvar tv


let list_read l n = try List.nth l n with _ -> failwith "uninterp: read"
  
let inst_read (inst:ILGenericArgs) v =
  try list_read inst  (int v)
  with _ -> failwithf "type variable no. %d needs a value" v

let inst_add (x1:ILGenericArgs) (x2:ILGenericArgs) = (x1@x2 : ILGenericArgs)

let mk_simple_gparam nm =
   { gpName=nm;
     gpConstraints=[];
     gpVariance=NonVariant;
     gpReferenceTypeConstraint=false;
     gpNotNullableValueTypeConstraint=false;
     gpDefaultConstructorConstraint=false; }

let gparam_of_gactual (ga:ILType) = mk_simple_gparam "T"

let gparams_of_inst (x: ILGenericArgs) = List.map gparam_of_gactual x

let generalize_gparams (gparams:ILGenericParameterDefs)  =
    List.mapi (fun n gf -> mk_tyvar_ty (uint16 n)) gparams
 
let generalize_tref tref gparams = mk_tspec (tref,generalize_gparams gparams)

(* -------------------------------------------------------------------- 
 * Operations on class etc. defs.
 * -------------------------------------------------------------------- *)

let is_value_or_enum_tdef tdef = 
    match tdef.tdKind with
    | TypeDef_valuetype | TypeDef_enum -> true
    | _ -> false

let tref_for_nested_tdef scope (enc:ILTypeDef list,td:ILTypeDef)  = 
    mk_nested_tref(scope, (enc |> List.map (fun etd -> etd.Name)), td.Name)

let tspec_for_nested_tdef scope (enc:ILTypeDef list,td:ILTypeDef) = 
    generalize_tref (tref_for_nested_tdef scope (enc,td)) td.tdGenericParams

(* -------------------------------------------------------------------- 
 * Operations on type tables.
 * -------------------------------------------------------------------- *)

let getname ltd = 
  let td = (Lazy.force ltd)
  let ns,n = split_type_name td.tdName
  (ns,n,td.tdCustomAttrs,ltd)

let add_tdef_to_tab (ns,n,cas,ltd) tab = 
  let prev = 
     (match Map.tryfind ns tab with 
      | None -> Dictionary.create 1
      | Some prev -> prev)
  if prev.ContainsKey n then  
      let msg = sprintf "not unique type %s" (unsplit_type_name (ns,n));
      System.Diagnostics.Debug.Assert(false,msg)
      failwith msg
  prev.[n] <- ltd;
  Map.add ns prev tab

let add_lazy_tdef_to_larr ltd larr = lazy_map (fun arr -> Array.of_list (getname ltd :: Array.to_list arr)) larr

let build_tab larr = lazy_map (fun arr -> Array.foldBack add_tdef_to_tab arr Map.empty) larr
let build_types larr = TypeDefTable (larr, build_tab larr)

(* this is not performance critical *)
let add_tdef td (TypeDefTable (larr,ltab)) = build_types (add_lazy_tdef_to_larr (notlazy td) larr)       
let mk_tdefs l =  build_types (List.map (notlazy >> getname) l |> Array.of_list |> notlazy )
let mk_lazy_tdefs llist = build_types (lazy_map Array.of_list llist)
//REVIEW: Propagate the underlying array representation up through the callers of these two functions
//REVIEW: This conversion to a list causes a lot of allocations
let iter_tdefs f tdefs = dest_tdefs tdefs |> List.iter f

let replace_tdef td (TypeDefTable (larr,_)) =
    larr
    |> lazy_map (fun larr ->
           let llist = Array.to_list larr
           let (ns,n,_,_)  as data = getname (notlazy td)
           let llist = llist |> List.filter (fun (ns2,n2,_,_) -> not (ns = ns2 && n = n2))
           Array.of_list (data :: llist)) 
    |> build_types

// -------------------------------------------------------------------- 
// Operations on method tables.
//
// REVIEW: this data structure looks substandard
// -------------------------------------------------------------------- 

let dest_mdefs (Methods lpmap) = fst (Lazy.force lpmap)
let add_mdef_to_tab y tab =
  let key = y.mdName
  let prev = Map.tryFindMulti key tab
  Map.add key (y::prev) tab

let add_mdef_to_pmap y (mds,tab) = y::mds,add_mdef_to_tab y tab
let add_mdef y (Methods lpmap) = Methods (lazy_map (add_mdef_to_pmap y) lpmap)

let mk_mdefs l =  Methods (notlazy (List.foldBack add_mdef_to_pmap l ([],Map.empty)))
let mk_lazy_mdefs l =  Methods (lazy (List.foldBack add_mdef_to_pmap (Lazy.force l) ([],Map.empty)))
let add_mdef_to_tdef m cd = {cd with tdMethodDefs = add_mdef m cd.tdMethodDefs }

let filter_mdefs f (Methods lpmap) = 
    Methods (lazy_map (fun (fs,_) -> 
        let l = List.filter f fs
        (l, List.foldBack add_mdef_to_tab l Map.empty)) lpmap)

let find_mdefs_by_name nm (Methods lpmap) = 
    let t = snd (Lazy.force lpmap)
    Map.tryFindMulti nm t 

let find_mdefs_by_arity (nm,arity) tab = 
    List.filter (fun x -> x.mdParams.Length = arity) (find_mdefs_by_name nm tab)


(* -------------------------------------------------------------------- 
 * Operations and defaults for modules, assemblies etc.
 * -------------------------------------------------------------------- *)

let default_modulSubSystem = 3 (* this is what comes out of ILDASM on 30/04/2001 *)
let default_modulPhysAlignment = 512 (* this is what comes out of ILDASM on 30/04/2001 *)
let default_modulVirtAlignment = 0x2000 (* this is what comes out of ILDASM on 30/04/2001 *)
let default_modulImageBase = 0x034f0000 (* this is what comes out of ILDASM on 30/04/2001 *)

// -------------------------------------------------------------------- 
// Array types
// -------------------------------------------------------------------- 

let rank_of_array_shape (ILArrayShape l) = (List.length l)
let mk_array_ty (ty,shape) = Type_array(shape,ty)
let Rank1ArrayShape = ILArrayShape [(Some 0, None)]
let mk_sdarray_ty ty = mk_array_ty (ty,Rank1ArrayShape)

let dest_array_ty = function 
  | Type_array (shape,ty) -> shape,ty
  | _ -> failwith "dest_array_ty: bad array type"

let is_array_ty = function 
  | Type_array _ -> true
  | _ -> false

(* -------------------------------------------------------------------- 
 * Sigs of special types built-in, e.g. those needed by the verifier
 * -------------------------------------------------------------------- *)

let mscorlib_module_name =  "CommonLanguageRuntimeLibrary"

let tname_Object = "System.Object"
let tname_String = "System.String"
let tname_StringBuilder = "System.Text.StringBuilder"
let tname_AsyncCallback = "System.AsyncCallback"
let tname_IAsyncResult = "System.IAsyncResult"
let tname_IComparable = "System.IComparable"
let tname_Exception = "System.Exception"
let tname_Type = "System.Type"
let tname_Missing = "System.Reflection.Missing"
let tname_Activator = "System.Activator"
let tname_SerializationInfo = "System.Runtime.Serialization.SerializationInfo"
let tname_StreamingContext = "System.Runtime.Serialization.StreamingContext"
let tname_SecurityPermissionAttribute = "System.Security.Permissions.SecurityPermissionAttribute"
let tname_Delegate = "System.Delegate"
let tname_ValueType = "System.ValueType"
let tname_TypedReference = "System.TypedReference"
let tname_Enum = "System.Enum"
let tname_MulticastDelegate = "System.MulticastDelegate"
let tname_Array = "System.Array"

let tname_Int64 = "System.Int64"
let tname_UInt64 = "System.UInt64"
let tname_Int32 = "System.Int32"
let tname_UInt32 = "System.UInt32"
let tname_Int16 = "System.Int16"
let tname_UInt16 = "System.UInt16"
let tname_SByte = "System.SByte"
let tname_Byte = "System.Byte"
let tname_Single = "System.Single"
let tname_Double = "System.Double"
let tname_Bool = "System.Boolean"
let tname_Char = "System.Char"
let tname_IntPtr = "System.IntPtr"
let tname_UIntPtr = "System.UIntPtr"
let tname_RuntimeArgumentHandle = "System.RuntimeArgumentHandle"
let tname_RuntimeTypeHandle = "System.RuntimeTypeHandle"
let tname_RuntimeMethodHandle = "System.RuntimeMethodHandle"
let tname_RuntimeFieldHandle = "System.RuntimeFieldHandle"

[<StructuralEquality(false); StructuralComparison(false)>]
type ILGlobals = 
    { mscorlib_scoref: ILScopeRef;
      mscorlibAssemblyName: string;
      tref_Object: ILTypeRef 
      ; tspec_Object: ILTypeSpec
      ; typ_Object: ILType
      ; tref_String: ILTypeRef
      ; typ_String: ILType
      ; typ_StringBuilder: ILType
      ; typ_AsyncCallback: ILType
      ; typ_IAsyncResult: ILType
      ; typ_IComparable: ILType
      ; tref_Type: ILTypeRef
      ; typ_Type: ILType
      ; tref_Missing: ILTypeRef
      ; typ_Missing: ILType
      ; typ_Activator: ILType
      ; typ_Delegate: ILType
      ; typ_ValueType: ILType
      ; typ_Enum: ILType
      ; tspec_TypedReference: ILTypeSpec
      ; typ_TypedReference: ILType
      ; typ_MulticastDelegate: ILType
      ; typ_Array: ILType
      ; tspec_Int64: ILTypeSpec
      ; tspec_UInt64: ILTypeSpec
      ; tspec_Int32: ILTypeSpec
      ; tspec_UInt32: ILTypeSpec
      ; tspec_Int16: ILTypeSpec
      ; tspec_UInt16: ILTypeSpec
      ; tspec_SByte: ILTypeSpec
      ; tspec_Byte: ILTypeSpec
      ; tspec_Single: ILTypeSpec
      ; tspec_Double: ILTypeSpec
      ; tspec_IntPtr: ILTypeSpec
      ; tspec_UIntPtr: ILTypeSpec
      ; tspec_Char: ILTypeSpec
      ; tspec_Bool: ILTypeSpec
      ; typ_int8: ILType
      ; typ_int16: ILType
      ; typ_int32: ILType
      ; typ_int64: ILType
      ; typ_uint8: ILType
      ; typ_uint16: ILType
      ; typ_uint32: ILType
      ; typ_uint64: ILType
      ; typ_float32: ILType
      ; typ_float64: ILType
      ; typ_bool: ILType
      ; typ_char: ILType
      ; typ_IntPtr: ILType
      ; typ_UIntPtr: ILType
      ; typ_RuntimeArgumentHandle: ILType
      ; typ_RuntimeTypeHandle: ILType
      ; typ_RuntimeMethodHandle: ILType
      ; typ_RuntimeFieldHandle: ILType
      ; typ_Byte: ILType
      ; typ_Int16: ILType
      ; typ_Int32: ILType
      ; typ_Int64: ILType
      ; typ_SByte: ILType
      ; typ_UInt16: ILType
      ; typ_UInt32: ILType
      ; typ_UInt64: ILType
      ; typ_Single: ILType
      ; typ_Double: ILType
      ; typ_Bool: ILType
      ; typ_Char: ILType
      ; typ_SerializationInfo: ILType
      ; typ_StreamingContext: ILType
      ; tref_SecurityPermissionAttribute: ILTypeRef
      ; tspec_Exception: ILTypeSpec
      ; typ_Exception: ILType }

let mk_normal_call mspec = I_call (Normalcall, mspec, None)
let mk_normal_callvirt mspec = I_callvirt (Normalcall, mspec, None)
let mk_normal_callconstraint (ty,mspec) = I_callconstraint (Normalcall, ty, mspec, None)
let mk_normal_newobj mspec =  I_newobj (mspec, None)
let ldarg_0 = I_ldarg 0us
let ldarg_1 = I_ldarg 1us
let tname_CompilerGeneratedAttribute = "System.Runtime.CompilerServices.CompilerGeneratedAttribute"
let tname_DebuggableAttribute = "System.Diagnostics.DebuggableAttribute"

let mk_ILGlobals mscorlib_scoref mscorlib_assembly_name_option =
  let mscorlibAssemblyName =
    match mscorlib_assembly_name_option with
      | Some name -> name 
      | None      -> (match mscorlib_scoref with
                        | ScopeRef_assembly assref -> assref.Name
                        | _ -> failwith "mk_ILGlobals: mscorlib ILScopeRef is not an assembly ref")
  let tref_Object = mk_tref (mscorlib_scoref,tname_Object)
  let tspec_Object = mk_nongeneric_tspec tref_Object
  let typ_Object = Type_boxed tspec_Object

  let tref_String = mk_tref (mscorlib_scoref,tname_String)
  let tspec_String = mk_tspec(tref_String,mk_empty_gactuals)
  let typ_String = Type_boxed tspec_String

  let tref_StringBuilder = mk_tref (mscorlib_scoref,tname_StringBuilder)
  let tspec_StringBuilder = mk_tspec(tref_StringBuilder,mk_empty_gactuals)
  let typ_StringBuilder = Type_boxed tspec_StringBuilder

  let tref_AsyncCallback = mk_tref (mscorlib_scoref,tname_AsyncCallback)
  let tspec_AsyncCallback = mk_tspec(tref_AsyncCallback,mk_empty_gactuals)
  let typ_AsyncCallback = Type_boxed tspec_AsyncCallback

  let tref_IAsyncResult = mk_tref (mscorlib_scoref,tname_IAsyncResult)
  let tspec_IAsyncResult = mk_tspec(tref_IAsyncResult,mk_empty_gactuals)
  let typ_IAsyncResult = Type_boxed tspec_IAsyncResult

  let tref_IComparable = mk_tref (mscorlib_scoref,tname_IComparable)
  let tspec_IComparable = mk_tspec(tref_IComparable,mk_empty_gactuals)
  let typ_IComparable = Type_boxed tspec_IComparable

  let tref_Exception = mk_tref (mscorlib_scoref,tname_Exception)
  let tspec_Exception = mk_tspec(tref_Exception,mk_empty_gactuals)
  let typ_Exception = Type_boxed tspec_Exception

  let tref_Type = mk_tref(mscorlib_scoref,tname_Type)
  let tspec_Type = mk_tspec(tref_Type,mk_empty_gactuals)
  let typ_Type = Type_boxed tspec_Type

  let tref_Missing = mk_tref(mscorlib_scoref,tname_Missing)
  let tspec_Missing = mk_tspec(tref_Missing,mk_empty_gactuals)
  let typ_Missing = Type_boxed tspec_Missing


  let tref_Activator = mk_tref(mscorlib_scoref,tname_Activator)
  let tspec_Activator = mk_tspec(tref_Activator,mk_empty_gactuals)
  let typ_Activator = Type_boxed tspec_Activator

  let tref_SerializationInfo = mk_tref(mscorlib_scoref,tname_SerializationInfo)
  let tspec_SerializationInfo = mk_tspec(tref_SerializationInfo,mk_empty_gactuals)
  let typ_SerializationInfo = Type_boxed tspec_SerializationInfo

  let tref_StreamingContext = mk_tref(mscorlib_scoref,tname_StreamingContext)
  let tspec_StreamingContext = mk_tspec(tref_StreamingContext,mk_empty_gactuals)
  let typ_StreamingContext = Type_value tspec_StreamingContext

  let tref_SecurityPermissionAttribute = mk_tref(mscorlib_scoref,tname_SecurityPermissionAttribute)

  let tref_Delegate = mk_tref(mscorlib_scoref,tname_Delegate)
  let tspec_Delegate = mk_tspec(tref_Delegate,mk_empty_gactuals)
  let typ_Delegate = Type_boxed tspec_Delegate

  let tref_ValueType = mk_tref (mscorlib_scoref,tname_ValueType)
  let tspec_ValueType = mk_tspec(tref_ValueType,mk_empty_gactuals)
  let typ_ValueType = Type_boxed tspec_ValueType

  let tref_TypedReference = mk_tref (mscorlib_scoref,tname_TypedReference)
  let tspec_TypedReference = mk_tspec(tref_TypedReference,mk_empty_gactuals)
  let typ_TypedReference = Type_value tspec_TypedReference

  let tref_Enum = mk_tref (mscorlib_scoref,tname_Enum)
  let tspec_Enum = mk_tspec(tref_Enum,mk_empty_gactuals)
  let typ_Enum = Type_boxed tspec_Enum

  let tref_MulticastDelegate = mk_tref (mscorlib_scoref,tname_MulticastDelegate)
  let tspec_MulticastDelegate = mk_tspec(tref_MulticastDelegate,mk_empty_gactuals)
  let typ_MulticastDelegate = Type_boxed tspec_MulticastDelegate

  let typ_Array = Type_boxed (mk_tspec(mk_tref (mscorlib_scoref,tname_Array),mk_empty_gactuals))

  let tref_Int64 = mk_tref (mscorlib_scoref,tname_Int64)
  let tref_UInt64 = mk_tref (mscorlib_scoref,tname_UInt64)
  let tref_Int32 = mk_tref (mscorlib_scoref,tname_Int32)
  let tref_UInt32 = mk_tref (mscorlib_scoref,tname_UInt32)
  let tref_Int16 = mk_tref (mscorlib_scoref,tname_Int16)
  let tref_UInt16 = mk_tref (mscorlib_scoref,tname_UInt16)
  let tref_SByte = mk_tref (mscorlib_scoref,tname_SByte)
  let tref_Byte = mk_tref (mscorlib_scoref,tname_Byte)
  let tref_Single = mk_tref (mscorlib_scoref,tname_Single)
  let tref_Double = mk_tref (mscorlib_scoref,tname_Double)
  let tref_Bool = mk_tref (mscorlib_scoref,tname_Bool)
  let tref_Char = mk_tref (mscorlib_scoref,tname_Char)
  let tref_IntPtr = mk_tref (mscorlib_scoref,tname_IntPtr)
  let tref_UIntPtr = mk_tref (mscorlib_scoref,tname_UIntPtr)

  let tspec_Int64 = mk_tspec(tref_Int64,mk_empty_gactuals)
  let tspec_UInt64 = mk_tspec(tref_UInt64,mk_empty_gactuals)
  let tspec_Int32 = mk_tspec(tref_Int32,mk_empty_gactuals)
  let tspec_UInt32 = mk_tspec(tref_UInt32,mk_empty_gactuals)
  let tspec_Int16 = mk_tspec(tref_Int16,mk_empty_gactuals)
  let tspec_UInt16 = mk_tspec(tref_UInt16,mk_empty_gactuals)
  let tspec_SByte = mk_tspec(tref_SByte,mk_empty_gactuals)
  let tspec_Byte = mk_tspec(tref_Byte,mk_empty_gactuals)
  let tspec_Single = mk_tspec(tref_Single,mk_empty_gactuals)
  let tspec_Double = mk_tspec(tref_Double,mk_empty_gactuals)
  let tspec_IntPtr = mk_tspec(tref_IntPtr,mk_empty_gactuals)
  let tspec_UIntPtr = mk_tspec(tref_UIntPtr,mk_empty_gactuals)
  let tspec_Char = mk_tspec(tref_Char,mk_empty_gactuals)
  let tspec_Bool = mk_tspec(tref_Bool,mk_empty_gactuals)

  let typ_int8 = Type_value tspec_SByte 
  let typ_int16 = Type_value tspec_Int16
  let typ_int32 = Type_value tspec_Int32
  let typ_int64 = Type_value tspec_Int64
  let typ_uint8 = Type_value tspec_Byte
  let typ_uint16 = Type_value tspec_UInt16
  let typ_uint32 = Type_value tspec_UInt32
  let typ_uint64 = Type_value tspec_UInt64
  let typ_float32 = Type_value tspec_Single
  let typ_float64 = Type_value tspec_Double
  let typ_bool = Type_value tspec_Bool
  let typ_char = Type_value tspec_Char
  let typ_IntPtr = Type_value tspec_IntPtr
  let typ_UIntPtr = Type_value tspec_UIntPtr

  let typ_SByte = Type_value tspec_SByte
  let typ_Int16 = Type_value tspec_Int16
  let typ_Int32 = Type_value tspec_Int32
  let typ_Int64 = Type_value tspec_Int64
  let typ_Byte = Type_value tspec_Byte
  let typ_UInt16 = Type_value tspec_UInt16
  let typ_UInt32 = Type_value tspec_UInt32
  let typ_UInt64 = Type_value tspec_UInt64
  let typ_Single = Type_value tspec_Single
  let typ_Double = Type_value tspec_Double
  let typ_Bool = Type_value tspec_Bool
  let typ_Char = Type_value tspec_Char

  let tref_RuntimeArgumentHandle = mk_tref (mscorlib_scoref,tname_RuntimeArgumentHandle)
  let tspec_RuntimeArgumentHandle = mk_tspec(tref_RuntimeArgumentHandle,mk_empty_gactuals)
  let typ_RuntimeArgumentHandle = Type_value tspec_RuntimeArgumentHandle
  let tref_RuntimeTypeHandle = mk_tref (mscorlib_scoref,tname_RuntimeTypeHandle)
  let tspec_RuntimeTypeHandle = mk_tspec(tref_RuntimeTypeHandle,mk_empty_gactuals)
  let typ_RuntimeTypeHandle = Type_value tspec_RuntimeTypeHandle
  let tref_RuntimeMethodHandle = mk_tref (mscorlib_scoref,tname_RuntimeMethodHandle)
  let tspec_RuntimeMethodHandle = mk_tspec(tref_RuntimeMethodHandle,mk_empty_gactuals)
  let typ_RuntimeMethodHandle = Type_value tspec_RuntimeMethodHandle
  let tref_RuntimeFieldHandle = mk_tref (mscorlib_scoref,tname_RuntimeFieldHandle)
  let tspec_RuntimeFieldHandle = mk_tspec(tref_RuntimeFieldHandle,mk_empty_gactuals)
  let typ_RuntimeFieldHandle = Type_value tspec_RuntimeFieldHandle
  {   mscorlib_scoref            =mscorlib_scoref
    ; mscorlibAssemblyName     =mscorlibAssemblyName
    ; tref_Object                =tref_Object                  
    ; tspec_Object               =tspec_Object                 
    ; typ_Object                 =typ_Object                   
    ; tref_String                =tref_String                  
    ; typ_String                 =typ_String                   
    ; typ_StringBuilder          =typ_StringBuilder                   
    ; typ_AsyncCallback          =typ_AsyncCallback            
    ; typ_IAsyncResult           =typ_IAsyncResult             
    ; typ_IComparable            =typ_IComparable              
    ; typ_Activator              =typ_Activator                     
    ; tref_Type                  =tref_Type                    
    ; typ_Type                   =typ_Type                     
    ; tref_Missing               =tref_Missing                    
    ; typ_Missing                =typ_Missing                     
    ; typ_Delegate               =typ_Delegate                 
    ; typ_ValueType              =typ_ValueType                
    ; typ_Enum                   =typ_Enum                     
    ; tspec_TypedReference       =tspec_TypedReference         
    ; typ_TypedReference         =typ_TypedReference           
    ; typ_MulticastDelegate      =typ_MulticastDelegate        
    ; typ_Array                  =typ_Array                    
    ; tspec_Int64                =tspec_Int64                  
    ; tspec_UInt64               =tspec_UInt64                 
    ; tspec_Int32                =tspec_Int32                  
    ; tspec_UInt32               =tspec_UInt32                 
    ; tspec_Int16                =tspec_Int16                  
    ; tspec_UInt16               =tspec_UInt16                 
    ; tspec_SByte                =tspec_SByte                  
    ; tspec_Byte                 =tspec_Byte                   
    ; tspec_Single               =tspec_Single                 
    ; tspec_Double               =tspec_Double                 
    ; tspec_IntPtr               =tspec_IntPtr                 
    ; tspec_UIntPtr              =tspec_UIntPtr                
    ; tspec_Char                 =tspec_Char                   
    ; tspec_Bool                 =tspec_Bool                   
    ; typ_int8                   =typ_int8                     
    ; typ_int16                  =typ_int16                    
    ; typ_int32                  =typ_int32                    
    ; typ_int64                  =typ_int64                    
    ; typ_uint8                  =typ_uint8                    
    ; typ_uint16                 =typ_uint16                   
    ; typ_uint32                 =typ_uint32                   
    ; typ_uint64                 =typ_uint64                   
    ; typ_float32                =typ_float32                  
    ; typ_float64                =typ_float64                  
    ; typ_bool                   =typ_bool                     
    ; typ_char                   =typ_char                     
    ; typ_IntPtr                    =typ_IntPtr                      
    ; typ_UIntPtr                   =typ_UIntPtr                     
    ; typ_RuntimeArgumentHandle  =typ_RuntimeArgumentHandle    
    ; typ_RuntimeTypeHandle      =typ_RuntimeTypeHandle        
    ; typ_RuntimeMethodHandle    =typ_RuntimeMethodHandle      
    ; typ_RuntimeFieldHandle     =typ_RuntimeFieldHandle       
                                                                               
    ; typ_Byte                   =typ_Byte                     
    ; typ_Int16                  =typ_Int16                    
    ; typ_Int32                  =typ_Int32                    
    ; typ_Int64                  =typ_Int64                    
    ; typ_SByte                  =typ_SByte                    
    ; typ_UInt16                 =typ_UInt16                   
    ; typ_UInt32                 =typ_UInt32                   
    ; typ_UInt64                 =typ_UInt64                   
    ; typ_Single                 =typ_Single                   
    ; typ_Double                 =typ_Double                   
    ; typ_Bool                   =typ_Bool                     
    ; typ_Char                   =typ_Char                     
    ; typ_SerializationInfo=typ_SerializationInfo
    ; typ_StreamingContext=typ_StreamingContext
    ; tref_SecurityPermissionAttribute=tref_SecurityPermissionAttribute
    ; tspec_Exception            =tspec_Exception              
    ; typ_Exception              =typ_Exception                 }

        
(* NOTE: ecma_ prefix refers to the standard "mscorlib" *)
let ecma_mscorlib_assembly_name = "mscorlib"
let ecma_public_token = PublicKeyToken (Bytes.of_intarray [|0x96; 0x9D; 0xB8; 0x05; 0x3D; 0x33; 0x22; 0xAC |]) 
let ecma_mscorlib_assref = 
  ILAssemblyRef.Create(ecma_mscorlib_assembly_name, None, Some ecma_public_token, true, None, None)

let ecma_mscorlib_scoref = ScopeRef_assembly ecma_mscorlib_assref

let ecmaILGlobals = mk_ILGlobals ecma_mscorlib_scoref None
   
let mspec_RuntimeHelpers_InitializeArray ilg = 
  mk_static_nongeneric_mspec_in_nongeneric_boxed_tref (mk_tref(ilg.mscorlib_scoref,"System.Runtime.CompilerServices.RuntimeHelpers"),"InitializeArray", [ilg.typ_Array;ilg.typ_RuntimeFieldHandle], Type_void)
(* e.ilg. [mk_mscorlib_exn_newobj "System.InvalidCastException"] *)
let mk_mscorlib_exn_newobj ilg eclass = 
  mk_normal_newobj (mk_nongeneric_ctor_mspec (mk_tref(ilg.mscorlib_scoref,eclass),AsObject,[]))

let mspec_Console_WriteLine ilg = mk_static_nongeneric_mspec_in_nongeneric_boxed_tref (mk_tref(ilg.mscorlib_scoref,"System.Console"),"WriteLine",[ilg.typ_String],Type_void)

let mspec_RunClassConstructor ilg = 
  mk_static_nongeneric_mspec_in_nongeneric_boxed_tref (mk_tref(ilg.mscorlib_scoref,"System.Runtime.CompilerServices.RuntimeHelpers"),"RunClassConstructor",[ilg.typ_RuntimeTypeHandle],Type_void)


let mk_RunClassConstructor ilg tspec =
  [ I_ldtoken (Token_type (mk_typ AsObject tspec));
    mk_normal_call (mspec_RunClassConstructor ilg) ]

let mspec_StringBuilder_string ilg = 
  mk_ctor_mspec_for_typ(ilg.typ_StringBuilder,[ilg.typ_String])

let typ_is_boxed = function Type_boxed _ -> true | _ -> false
let typ_is_value = function Type_value _ -> true | _ -> false


let tspec_is_mscorlib ilg (tspec:ILTypeSpec) n = 
  let tref = tspec.TypeRef
  let scoref = tref.Scope
  (tref.Name = n) &&
  begin match scoref with
  | ScopeRef_assembly n -> n.Name = ilg.mscorlibAssemblyName 
  | ScopeRef_module _ -> false
  | ScopeRef_local -> true
  end

let typ_is_boxed_mscorlib_typ ilg ty n = 
  typ_is_boxed ty && tspec_is_mscorlib ilg (tspec_of_typ ty) n

let typ_is_value_mscorlib_typ ilg ty n = 
  typ_is_value ty && tspec_is_mscorlib ilg (tspec_of_typ ty) n
      
let typ_is_Object            ilg ty = typ_is_boxed_mscorlib_typ ilg ty tname_Object
(*
let typ_is_MulticastDelegate ilg ty = typ_is_boxed_mscorlib_typ ilg ty tname_MulticastDelegate
let typ_is_Delegate          ilg ty = typ_is_boxed_mscorlib_typ ilg ty tname_Delegate
let typ_is_Enum              ilg ty = typ_is_boxed_mscorlib_typ ilg ty tname_Enum
let typ_is_ValueType         ilg ty = typ_is_boxed_mscorlib_typ ilg ty tname_ValueType
*)
let typ_is_String            ilg ty = typ_is_boxed_mscorlib_typ ilg ty tname_String
let typ_is_AsyncCallback     ilg ty = typ_is_boxed_mscorlib_typ ilg ty tname_AsyncCallback
let typ_is_TypedReference    ilg ty = typ_is_value_mscorlib_typ ilg ty tname_TypedReference
let typ_is_IAsyncResult ilg ty = typ_is_boxed_mscorlib_typ ilg ty tname_IAsyncResult
let typ_is_IComparable  ilg ty = typ_is_boxed_mscorlib_typ ilg ty tname_IComparable
let typ_is_SByte        ilg ty = typ_is_value_mscorlib_typ ilg ty tname_SByte
let typ_is_Byte         ilg ty = typ_is_value_mscorlib_typ ilg ty tname_Byte
let typ_is_Int16        ilg ty = typ_is_value_mscorlib_typ ilg ty tname_Int16
let typ_is_UInt16       ilg ty = typ_is_value_mscorlib_typ ilg ty tname_UInt16
let typ_is_Int32        ilg ty = typ_is_value_mscorlib_typ ilg ty tname_Int32
let typ_is_UInt32       ilg ty = typ_is_value_mscorlib_typ ilg ty tname_UInt32
let typ_is_Int64        ilg ty = typ_is_value_mscorlib_typ ilg ty tname_Int64
let typ_is_UInt64       ilg ty = typ_is_value_mscorlib_typ ilg ty tname_UInt64
let typ_is_IntPtr       ilg ty = typ_is_value_mscorlib_typ ilg ty tname_IntPtr
let typ_is_UIntPtr      ilg ty = typ_is_value_mscorlib_typ ilg ty tname_UIntPtr
let typ_is_Bool         ilg ty = typ_is_value_mscorlib_typ ilg ty tname_Bool
let typ_is_Char         ilg ty = typ_is_value_mscorlib_typ ilg ty tname_Char
let typ_is_Single       ilg ty = typ_is_value_mscorlib_typ ilg ty tname_Single
let typ_is_Double       ilg ty = typ_is_value_mscorlib_typ ilg ty tname_Double

#endif

(* -------------------------------------------------------------------- 
 * Rescoping
 * -------------------------------------------------------------------- *)


let qrescope_scoref scoref scoref_old = 
  match scoref,scoref_old with 
  | _,ScopeRef_local -> Some scoref
  | ScopeRef_local,_ -> None
  | _,ScopeRef_module _ -> Some scoref
  | ScopeRef_module _,_ -> None
  | _ -> None
let qrescope_tref scoref (x:ILTypeRef) = 
  match qrescope_scoref scoref x.Scope with 
  | None -> None
  | Some s -> Some (ILTypeRef.Create(s,x.Enclosing,x.Name))

let rescope_scoref x y = match qrescope_scoref x y with Some x -> x | None -> y
let rescope_tref x y = match qrescope_tref x y with Some x -> x | None -> y

// ORIGINAL IMPLEMENTATION (too many allocations
//         { tspecTypeRef=rescope_tref scoref tref;
//           tspecInst=rescope_inst scoref tinst } 
let rec qrescope_tspec scoref (tspec:ILTypeSpec) = 
  let tref = tspec.TypeRef
  let tinst = tspec.GenericArgs
  let qtref = qrescope_tref scoref tref
  match tinst,qtref with 
  | [],None -> None  (* avoid reallocation in the common case *)
  | _,None -> 
      Some (ILTypeSpec.Create (tref, rescope_inst scoref tinst))
  | _,Some tref -> 
      Some (ILTypeSpec.Create (tref, rescope_inst scoref tinst))
and rescope_tspec x y = match qrescope_tspec x y with Some x -> x | None -> y
and rescope_typ scoref typ = 
    match typ with 
    | Type_ptr t -> Type_ptr (rescope_typ scoref t)
    | Type_fptr t -> Type_fptr (rescope_callsig scoref t)
    | Type_byref t -> Type_byref (rescope_typ scoref t)
    | Type_boxed cr -> 
        begin match qrescope_tspec scoref cr with 
        | Some res -> Type_boxed res
        | None -> typ  (* avoid reallocation in the common case *)
        end
       
    | Type_array (s,ty) -> Type_array (s,rescope_typ scoref ty)
    | Type_value cr -> 
        begin match qrescope_tspec scoref cr with 
        | Some res -> Type_value res
        | None -> typ  (* avoid reallocation in the common case *)
        end
    | Type_modified(b,tref,ty) -> Type_modified(b,rescope_tref scoref tref, rescope_typ scoref ty)
    | x -> x
and rescope_inst scoref i = List.map (rescope_typ scoref) i
and rescope_callsig scoref  csig = 
  mk_callsig
    (csig.callsigCallconv,List.map (rescope_typ scoref) csig.callsigArgs,rescope_typ scoref csig.callsigReturn)

let rescope_dloc scoref tref = rescope_tref scoref tref 
let rescope_mref scoref (x:ILMethodRef) =
  { mrefParent = rescope_dloc scoref x.EnclosingTypeRef;
    mrefCallconv = x.mrefCallconv;
    mrefGenericArity=x.mrefGenericArity;
    mrefName=x.mrefName;
    mrefArgs = List.map (rescope_typ scoref) x.mrefArgs;
    mrefReturn= rescope_typ scoref x.mrefReturn }

let rescope_fref scoref x = 
  { frefParent = rescope_tref scoref x.frefParent;
    frefName= x.frefName;
    frefType= rescope_typ scoref x.frefType }

let rescope_ospec scoref (OverridesSpec(mref,typ)) = 
  OverridesSpec (rescope_mref scoref mref,rescope_typ scoref typ)

let rescope_fspec scoref x = 
  { fspecFieldRef = rescope_fref scoref x.fspecFieldRef;
    fspecEnclosingType = rescope_typ scoref x.fspecEnclosingType }

let rescope_mspec scoref x =
  let x1,x2,x3 = dest_mspec x
  ILMethodSpec.Create(rescope_typ scoref x2,rescope_mref scoref x1,rescope_inst scoref x3)
  
#if STANDALONE_METADATA
#else

(* -------------------------------------------------------------------- 
 * Instantiate polymorphism in types
 * -------------------------------------------------------------------- *)

let rec inst_tspec_aux num_free inst (tspec:ILTypeSpec) = 
  ILTypeSpec.Create(tspec.TypeRef,inst_inst_aux num_free inst tspec.GenericArgs) 
  
and inst_typ_aux num_free inst typ = 
  match typ with 
  | Type_ptr t       -> Type_ptr (inst_typ_aux num_free inst t)
  | Type_fptr t      -> Type_fptr (inst_callsig_aux num_free inst t)
  | Type_array (a,t) -> Type_array (a,inst_typ_aux num_free inst t)
  | Type_byref t     -> Type_byref (inst_typ_aux num_free inst t)
  | Type_boxed cr    -> Type_boxed (inst_tspec_aux num_free inst cr)
  | Type_value cr    -> Type_value (inst_tspec_aux num_free inst cr)
  | Type_tyvar  v -> 
      let v = int v
      let top = List.length inst
      if v < num_free then typ else
      if v - num_free >= top then Type_tyvar (uint16 (v - top)) else 
      inst_read inst (uint16 (v - num_free)) 
  | x -> x
    
and inst_inst_aux num_free inst i = List.map (inst_typ_aux num_free inst) i
and inst_callsig_aux num_free inst  csig = 
  mk_callsig 
    (csig.callsigCallconv,List.map (inst_typ_aux num_free inst) csig.callsigArgs,inst_typ_aux num_free inst csig.callsigReturn)

let inst_typ     i t = inst_typ_aux 0 i t
let inst_inst    i t = inst_inst_aux 0 i t
let inst_tspec   i t = inst_tspec_aux 0 i t
let inst_callsig i t = inst_callsig_aux 0 i t

(* --------------------------------------------------------------------
 * MS-IL: Parameters, Return types and Locals
 * -------------------------------------------------------------------- *)

let mk_param (name,ty) =
    { paramName=name;
      paramDefault=None;
      paramMarshal=None;
      paramIn=false;
      paramOut=false;
      paramOptional=false;
      paramType=ty;
      paramCustomAttrs=mk_custom_attrs [] }
let mk_named_param (s,ty) = mk_param (Some s,ty)
let mk_unnamed_param ty = mk_param (None,ty)

let mk_return ty = 
    { returnMarshal=None;
      returnType=ty;
      returnCustomAttrs=mk_custom_attrs []  }

let mk_local ty = 
    { localPinned=false;
      localType=ty; }
let typ_of_local p = p.localType

let active_inst_of_fspec fspec = 
  inst_of_typ fspec.fspecEnclosingType

let actual_typ_of_fspec fr = 
  let env = active_inst_of_fspec fr
  inst_typ env fr.FormalType

(* -------------------------------------------------------------------- 
 * 
 * -------------------------------------------------------------------- *)

let mk_ldc_i32 i = I_arith (AI_ldc (DT_I4,NUM_I4 i))

(* -------------------------------------------------------------------- 
 * Make a method mbody
 * -------------------------------------------------------------------- *)

let mk_ilmbody (zeroinit,locals,maxstack,code,tag) = 
  { ilZeroInit=zeroinit;
    ilMaxStack=maxstack;
    ilNoInlining=false;
    ilLocals=locals;
    ilCode= code;
    ilSource=tag }

let mk_impl info = MethodBody_il (mk_ilmbody info)

(* -------------------------------------------------------------------- 
 * Make a constructor
 * -------------------------------------------------------------------- *)

let mk_void_return = mk_return Type_void

let mk_ctor (access,args,impl) = 
  { mdName=".ctor";
    mdKind=MethodKind_ctor;
    mdCallconv=ILCallingConv.Instance;
    mdParams=args;
    mdReturn= mk_void_return;
    mdAccess=access;
    mdBody= mk_mbody impl;
    mdCodeKind=MethodCodeKind_il;
      mdInternalCall=false;
    mdManaged=true;
    mdForwardRef=false;

    mdSecurityDecls=mk_security_decls [];
    mdHasSecurity=false;
    mdEntrypoint=false;

    mdGenericParams=mk_empty_gparams;
    mdExport=None;
    mdVtableEntry=None;
    mdReqSecObj=false;
    mdHideBySig=false;
    mdSpecialName=true;
    mdUnmanagedExport=false;
    mdSynchronized=false;
    mdMustRun=false;
    mdPreserveSig=false;
    mdCustomAttrs = mk_custom_attrs [];
 }

(* -------------------------------------------------------------------- 
 * Do-nothing ctor, just pass on to monomorphic superclass
 * -------------------------------------------------------------------- *)

let mk_ldargs args =
  [ ldarg_0; ] @
  List.mapi (fun i _ -> I_ldarg (uint16 (i+1))) args 

let mk_call_superclass_constructor_prim args mspec =
  mk_ldargs args @ [ mk_normal_call mspec ]

let mk_nongeneric_call_superclass_constructor ((args:ILType list),super_tref) =
  mk_call_superclass_constructor_prim args (mk_nongeneric_ctor_mspec (super_tref,AsObject,[]))

let mk_nongeneric_call_superclass_constructor2 ((args:ILParameter list),super_tref) =
  mk_call_superclass_constructor_prim args (mk_nongeneric_ctor_mspec (super_tref,AsObject,[]))

let mk_call_superclass_constructor ((args:ILType list),tspec) =
  mk_call_superclass_constructor_prim args (mk_ctor_mspec_for_boxed_tspec (tspec,[]))


let mk_normal_stfld fspec = I_stfld (Aligned,Nonvolatile,fspec)
let mk_normal_stsfld fspec = I_stsfld (Nonvolatile,fspec)
let mk_normal_ldsfld fspec = I_ldsfld (Nonvolatile,fspec)
let mk_normal_ldfld fspec = I_ldfld (Aligned,Nonvolatile,fspec)
let mk_normal_ldflda fspec = I_ldflda fspec
let mk_normal_stind dt = I_stind (Aligned,Nonvolatile,dt)
let mk_normal_ldind dt = I_ldind (Aligned,Nonvolatile,dt)
let mk_normal_ldobj dt = I_ldobj(Aligned,Nonvolatile,dt)
let mk_normal_stobj dt = I_stobj(Aligned,Nonvolatile,dt)
let mk_normal_cpind dt = [ I_ldind (Aligned,Nonvolatile,dt); 
                           I_stind (Aligned,Nonvolatile,dt) ]  (* REVIEW: check me *)

let mk_nongeneric_nothing_ctor tag super_tref args = 
  mk_ctor(MemAccess_public,args,
          mk_impl(false,[],8,
                  nonbranching_instrs_to_code  
                    (mk_nongeneric_call_superclass_constructor2 (args,super_tref)),tag))

(* -------------------------------------------------------------------- 
 * Make a static, top level monomophic method - very useful for
 * creating helper ILMethodDefs for internal use.
 * -------------------------------------------------------------------- *)
let mk_static_mdef (genparams,nm,access,args,ret,impl) = 
    { mdGenericParams=genparams;
      mdName=nm;
      mdCallconv = ILCallingConv.Static;
      mdKind=MethodKind_static;
      mdParams=  args;
      mdReturn= ret;
      mdAccess=access;
      mdHasSecurity=false;
      mdSecurityDecls=mk_security_decls [];
      mdEntrypoint=false;
      mdExport=None;
      mdCustomAttrs = mk_custom_attrs [];
      mdVtableEntry=None;
      mdBody= mk_mbody impl;
      mdCodeKind=MethodCodeKind_il;
      mdInternalCall=false;
      mdManaged=true;
      mdForwardRef=false;
      mdReqSecObj=false;
      mdHideBySig=false;
      mdSpecialName=false;
      mdUnmanagedExport=false;
      mdSynchronized=false;
      mdMustRun=false;
      mdPreserveSig=false; }

let mk_static_nongeneric_mdef (nm,access,args,ret,impl) = 
    mk_static_mdef (mk_empty_gparams,nm,access,args,ret,impl)

let mk_cctor impl = 
    { mdName=".cctor";
      mdCallconv=ILCallingConv.Static;
      mdGenericParams=mk_empty_gparams;
      mdKind=MethodKind_cctor;
      mdParams=[];
      mdReturn=mk_void_return;
      mdAccess=MemAccess_private; 
      mdEntrypoint=false;
      mdHasSecurity=false;
      mdSecurityDecls=mk_security_decls [];
      mdExport=None;
      mdCustomAttrs = mk_custom_attrs [];
      mdVtableEntry=None;
      mdBody= mk_mbody impl; 
      mdCodeKind=MethodCodeKind_il;
      mdInternalCall=false;
      mdManaged=true;
      mdForwardRef=false;
      mdReqSecObj=false;
      mdHideBySig=false;
      mdSpecialName=true;
      mdUnmanagedExport=false; 
      mdSynchronized=false;
      mdMustRun=false;
      mdPreserveSig=false;  } 

(* -------------------------------------------------------------------- 
 * Make a virtual method, where the overriding is simply the default
 * (i.e. overrides by name/signature)
 * -------------------------------------------------------------------- *)

let mk_ospec (typ,callconv,nm,genparams,formal_args,formal_ret) =
  OverridesSpec (mk_mref (tref_of_typ typ, callconv, nm, genparams, formal_args,formal_ret), typ)

let mk_generic_virtual_mdef (nm,access,genparams,actual_args,actual_ret,impl) = 
  { mdName=nm;
    mdGenericParams=genparams;
    mdCallconv=ILCallingConv.Instance;
    mdKind=
      MethodKind_virtual 
        { virtFinal=false; 
          // REVIEW: We'll need to start setting this eventually
          virtNewslot = false;
          virtStrict=true;
          virtAbstract=(match impl with MethodBody_abstract -> true | _ -> false) ; };
    mdParams= actual_args;
    mdReturn=actual_ret;
    mdAccess=access;
    mdEntrypoint=false;
    mdHasSecurity=false;
    mdSecurityDecls=mk_security_decls [];
    mdExport=None;
    mdCustomAttrs = mk_custom_attrs [];
    mdVtableEntry=None;
    mdBody= mk_mbody impl;
    mdCodeKind=MethodCodeKind_il;
      mdInternalCall=false;
    mdManaged=true;
    mdForwardRef=false;
    mdReqSecObj=false;
    mdHideBySig=false;
    mdSpecialName=false;
    mdUnmanagedExport=false; 
    mdSynchronized=false;
    mdMustRun=false;
    mdPreserveSig=false; }
    
let mk_virtual_mdef (nm,access,args,ret,impl) =  
  mk_generic_virtual_mdef (nm,access,mk_empty_gparams,args,ret,impl)

let mk_generic_instance_mdef (nm,access,genparams, actual_args,actual_ret, impl) = 
  { mdName=nm;
    mdGenericParams=genparams;
    mdCallconv=ILCallingConv.Instance;
    mdKind=MethodKind_nonvirtual;
    mdParams= actual_args;
    mdReturn=actual_ret;
    mdAccess=access;
    mdEntrypoint=false;
    mdHasSecurity=false;
    mdSecurityDecls=mk_security_decls [];
    mdExport=None;
    mdCustomAttrs = mk_custom_attrs [];
    mdVtableEntry=None;
    mdBody= mk_mbody impl;
    mdCodeKind=MethodCodeKind_il;
    mdInternalCall=false;
    mdManaged=true;
    mdForwardRef=false;
    mdReqSecObj=false;
    mdHideBySig=false;
    mdSpecialName=false;
    mdUnmanagedExport=false; 
    mdSynchronized=false;
    mdMustRun=false;
    mdPreserveSig=false; }
    
let mk_instance_mdef (nm,access,args,ret,impl) =  
  mk_generic_instance_mdef (nm,access,mk_empty_gparams,args,ret,impl)


(* -------------------------------------------------------------------- 
 * Add some code to the end of the .cctor for a type.  Create a .cctor
 * if one doesn't exist already.
 * -------------------------------------------------------------------- *)

let ilmbody_code2code f il  =
  {il with ilCode = f il.ilCode}

let mdef_code2code f md  =
  let il = 
    match dest_mbody md.mdBody with 
    | MethodBody_il il-> il 
    | _ -> failwith "mdef_code2code - method not IL"
  let b = MethodBody_il (ilmbody_code2code f il)
  {md with mdBody= mk_mbody b }  

let prepend_instrs_to_code c1 c2 = 
  let internalLab = generate_code_label ()
  join_code (check_code (mk_bblock {bblockLabel=internalLab;
          bblockInstrs=Array.of_list (c1 @ [ I_br (unique_entry_of_code c2)])})) c2

let prepend_instrs_to_mdef new_code md  = 
  mdef_code2code (prepend_instrs_to_code new_code) md

let cctor_id = (".cctor",0) 
(* Creates cctor if needed *)
let cdef_cctorCode2CodeOrCreate tag f cd = 
  let mdefs = cd.tdMethodDefs
  let md,mdefs = 
    match find_mdefs_by_arity cctor_id mdefs with 
    | [mdef] -> mdef,filter_mdefs (fun md -> not (md.mdName = (fst cctor_id))) mdefs
    | [] -> mk_cctor (mk_impl (false,[],1,nonbranching_instrs_to_code [ ],tag)), mdefs
    | _ -> failwith "bad method table: more than one .cctor found"
  let md' = f md
  {cd with tdMethodDefs = add_mdef md' mdefs}

let ilmbody_of_mdef m =
  match dest_mbody m.mdBody with
  | MethodBody_il il -> il
  | _ -> failwith "ilmbody_of_mdef: not IL"


let code_of_mdef (md:ILMethodDef) = 
  match md.Code with 
  | Some x -> x
  | None -> failwith "code_of_mdef: not IL" 

let argtys_of_mdef (md:ILMethodDef) = md.ParameterTypes
let retty_of_mdef (md:ILMethodDef) = md.Return.Type

let callsig_of_mdef md =
  mk_callsig (md.mdCallconv,argtys_of_mdef md,retty_of_mdef md)

let mk_mref_to_mdef (tref,md) =
  mk_mref (tref,md.mdCallconv,md.mdName,md.mdGenericParams.Length,argtys_of_mdef md,retty_of_mdef md)

let mk_fref_to_fdef (tref,fdef) =   mk_fref_in_tref (tref, fdef.fdName, fdef.fdType)

let mref_for_mdef scope (tdefs,tdef) mdef = mk_mref_to_mdef (tref_for_nested_tdef scope (tdefs,tdef), mdef)
let fref_for_fdef scope (tdefs,tdef) fdef = mk_fref_in_tref (tref_for_nested_tdef scope (tdefs,tdef), fdef.fdName, fdef.fdType)


(* Creates cctor if needed *)
let prepend_instrs_to_cctor instrs tag cd = 
  cdef_cctorCode2CodeOrCreate tag (prepend_instrs_to_mdef instrs) cd
    

let mk_fdef (isStatic,nm,ty,init,at,access) =
   { fdName=nm;
     fdType=ty;
     fdStatic = isStatic; 
     fdInit = init;
     fdData=at;
     fdOffset=None;
     fdSpecialName = false;
     fdMarshal=None; 
     fdNotSerialized=false;
     fdInitOnly = false;
     fdLiteral = false; 
     fdAccess = access; 
     fdCustomAttrs=mk_custom_attrs [] }

let mk_instance_fdef (nm,ty,init,access) = mk_fdef (false,nm,ty,init,None,access)
let mk_static_fdef (nm,ty,init,at,access) = mk_fdef (true,nm,ty,init,at,access)

(* -------------------------------------------------------------------- 
 * Scopes for allocating new temporary variables.
 * -------------------------------------------------------------------- *)

type tmps = { num_old_locals: int; newlocals: Local ResizeArray.t }
let alloc_tmp tmps loc =
  let locn = uint16(tmps.num_old_locals + ResizeArray.length tmps.newlocals)
  ResizeArray.add tmps.newlocals loc;
  locn

let get_tmps tmps = ResizeArray.to_list tmps.newlocals
let new_tmps n = { num_old_locals=n; newlocals=ResizeArray.create 10 }


let typ_of_fdef f = f.fdType
let name_of_fdef f = f.fdName 

let name_of_event e = e.eventName
let name_of_property p = p.propName

let mk_fdefs l =  Fields (LazyOrderedMultiMap(name_of_fdef,notlazy l))
let mk_lazy_fdefs l =  Fields (LazyOrderedMultiMap(name_of_fdef,l))
let filter_fdefs f (Fields t) = Fields (t.Filter(f))

let mk_events l =  Events (LazyOrderedMultiMap(name_of_event,notlazy l))
let mk_lazy_events l =  Events (LazyOrderedMultiMap(name_of_event,l))
let filter_edefs f (Events t) = Events (t.Filter(f))

let mk_properties l =  Properties (LazyOrderedMultiMap(name_of_property,notlazy l))
let mk_lazy_properties l =  Properties (LazyOrderedMultiMap(name_of_property,l) )
let filter_pdefs f (Properties t) = Properties (t.Filter(f))

let add_exported_type_to_tab y tab = Map.add y.exportedTypeName y tab
let mk_exported_types l =  ILExportedTypes (notlazy (List.foldBack add_exported_type_to_tab l Map.empty))
let mk_lazy_exported_types (l:Lazy<_>) =   ILExportedTypes (lazy (List.foldBack add_exported_type_to_tab (l.Force()) Map.empty))

let add_nested_exported_type_to_tab y tab =
  let key = y.nestedExportedTypeName
  Map.add key y tab

let mk_nested_exported_types l =  
  ILNestedExportedTypes (notlazy (List.foldBack add_nested_exported_type_to_tab l Map.empty))
let mk_lazy_nested_exported_types (l:Lazy<_>) =  
  ILNestedExportedTypes (lazy (List.foldBack add_nested_exported_type_to_tab (l.Force()) Map.empty))
let find_nested_exported_type x (ILNestedExportedTypes ltab) = Map.find x (ltab.Force())

let mk_resources l =  ILResources (notlazy l)
let mk_lazy_resources l =  ILResources l

let add_mimpl_to_tab y tab =
    let key = (y.mimplOverrides.MethodRef.Name,List.length y.mimplOverrides.MethodRef.ArgTypes)
    let prev = Map.tryFindMulti key tab
    Map.add key (y::prev) tab
let mk_mimpls l =  MethodImpls (notlazy (List.foldBack add_mimpl_to_tab l Map.empty))
let mk_lazy_mimpls l =  MethodImpls (lazy (List.foldBack add_mimpl_to_tab (Lazy.force l) Map.empty))
let filter_mimpls f (MethodImpls ltab) =  MethodImpls (lazy_map (Map.mapi (fun _ x -> List.filter f x)) ltab)

(* -------------------------------------------------------------------- 
 * Make a constructor that simply takes its arguments and stuffs
 * them in fields.  preblock is how to call the superclass constructor....
 * -------------------------------------------------------------------- *)

let mk_storage_ctor_with_param_names(tag,preblock,tspec,flds,access) = 
    mk_ctor(access,
            flds |> List.map (fun (pnm,_,ty) -> mk_named_param (pnm,ty)),
            mk_impl
              (false,[],2,
               nonbranching_instrs_to_code
                 begin 
                   (match tag with Some x -> [I_seqpoint x] | None -> []) @ 
                   preblock @
                   begin 
                     List.concat (List.mapi (fun n (pnm,nm,ty) -> 
                       [ ldarg_0;
                         I_ldarg (uint16 (n+1));
                         mk_normal_stfld (mk_fspec_in_boxed_tspec (tspec,nm,ty));
                       ])  flds)
                   end
                 end,tag))
    
let mk_simple_storage_ctor_with_param_names(tag,base_tspec,derived_tspec,flds,access) = 
    let preblock = 
      match base_tspec with 
        None -> []
      | Some tspec -> 
          ([ ldarg_0; 
             mk_normal_call (mk_ctor_mspec_for_boxed_tspec (tspec,[])) ])
    mk_storage_ctor_with_param_names(tag,preblock,derived_tspec,flds,access)

let add_param_names flds = 
    flds |> List.map (fun (nm,ty) -> (nm,nm,ty))

let mk_simple_storage_ctor(tag,base_tspec,derived_tspec,flds,access) = 
    mk_simple_storage_ctor_with_param_names(tag,base_tspec,derived_tspec, add_param_names flds, access)

let mk_storage_ctor(tag,preblock,tspec,flds,access) = mk_storage_ctor_with_param_names(tag,preblock,tspec, add_param_names flds, access)


let mk_generic_class (nm,access,genparams,extends,impl,methods,fields,props,events,attrs) =
  { tdKind=TypeDef_class;
    tdName=nm;
    tdGenericParams= genparams;
    tdAccess = access;
    tdImplements = impl;
    tdAbstract = false;
    tdSealed = false;
    tdSerializable = false;
    tdComInterop=false;
    tdSpecialName=false;
    tdLayout=TypeLayout_auto;
    tdEncoding=TypeEncoding_ansi;
    tdInitSemantics=TypeInit_beforefield;
    tdExtends = Some extends;
    tdMethodDefs= methods; 
    tdFieldDefs= fields;
    tdNested=mk_tdefs [];
    tdCustomAttrs=attrs;
    tdMethodImpls=mk_mimpls [];
    tdProperties=props;
    tdEvents=events;
    tdSecurityDecls=mk_security_decls []; 
    tdHasSecurity=false;
} 
    
let mk_rawdata_vtdef ilg (nm,size,pack) =
  { tdKind=TypeDef_valuetype;
    tdName = nm;
    tdGenericParams= [];
    tdAccess = TypeAccess_private;
    tdImplements = [];
    tdAbstract = false;
    tdSealed = true;
    tdExtends = Some ilg.typ_ValueType;
    tdComInterop=false;    
    tdSerializable = false;
    tdSpecialName=false;
    tdLayout=TypeLayout_explicit { typeSize=Some size; typePack=Some pack };
    tdEncoding=TypeEncoding_ansi;
    tdInitSemantics=TypeInit_beforefield;
    tdMethodDefs= mk_mdefs []; 
    tdFieldDefs= mk_fdefs [];
    tdNested=mk_tdefs [];
    tdCustomAttrs=mk_custom_attrs [];
    tdMethodImpls=mk_mimpls [];
    tdProperties=mk_properties [];
    tdEvents=mk_events [];
    tdSecurityDecls=mk_security_decls []; 
    tdHasSecurity=false;  }


let mk_simple_tdef ilg (nm,access,methods,fields,props,events,attrs) =
  mk_generic_class (nm,access, mk_empty_gparams, ilg.typ_Object, [], methods,fields,props,events,attrs)

let mk_toplevel_tdef ilg (methods,fields) = mk_simple_tdef ilg (tname_for_toplevel,TypeAccess_public, methods,fields,mk_properties [], mk_events [], mk_custom_attrs [])

let dest_tdefs_with_toplevel_first ilg tdefs = 
  let l = dest_tdefs tdefs
  let top,nontop = l |> List.partition (fun td -> td.Name = tname_for_toplevel)
  let top2 = if isNil top then [mk_toplevel_tdef ilg (mk_mdefs [], mk_fdefs [])] else top
  top2@nontop

let mk_simple_mainmod assname modname dll tdefs hashalg locale flags = 
    { modulManifest= 
        Some { manifestName=assname;
               manifestAuxModuleHashAlgorithm= match hashalg with | Some(alg) -> alg | _ -> 0x8004; // SHA1
               manifestSecurityDecls=mk_security_decls [];
               manifestPublicKey= None;
               manifestVersion= None;
               manifestLocale=locale
               manifestCustomAttrs=mk_custom_attrs [];
               manifestLongevity=LongevityUnspecified;
               manifestDisableJitOptimizations= 0 <> (flags &&& 0x4000);
               manifestJitTracking=0 <> (flags &&& 0x8000); // always turn these on
               manifestRetargetable= 0 <> (flags &&& 0xff);
               manifestExportedTypes=mk_exported_types [];
               manifestEntrypointElsewhere=None
             };
      modulCustomAttrs=mk_custom_attrs [];
      modulName=modname;
      modulNativeResources=[];
      
      modulTypeDefs=tdefs;
      modulSubSystem=default_modulSubSystem;
      modulDLL=dll;
      modulILonly=true;
      modulPlatform=None;
      modul32bit=false;
      modul64bit=false;
      modulPhysAlignment=default_modulPhysAlignment;
      modulVirtAlignment=default_modulVirtAlignment;
      modulImageBase=default_modulImageBase;
      modulResources=mk_resources [];
(*      modulFixups=[]; *)
    }


(*-----------------------------------------------------------------------
 * Intermediate parsing structure for exception tables....
 *----------------------------------------------------------------------*)

type ExceptionClause = 
  | SEH_finally of (ILCodeLabel * ILCodeLabel)
  | SEH_fault  of (ILCodeLabel * ILCodeLabel)
  | SEH_filter_catch of (ILCodeLabel * ILCodeLabel) * (ILCodeLabel * ILCodeLabel)
  | SEH_type_catch of ILType * (ILCodeLabel * ILCodeLabel)

type ILExceptionSpec = 
    { exnRange: (ILCodeLabel * ILCodeLabel);
      exnClauses: ExceptionClause list }

type exceptions = ILExceptionSpec list

(*-----------------------------------------------------------------------
 * [instructions_to_code] makes the basic block structure of code from
 * a primitive array of instructions.  We
 * do this be iterating over the instructions, pushing new basic blocks 
 * everytime we encounter an address that has been recorded
 * [bbstartToCodeLabelMap].
 *----------------------------------------------------------------------*)

type ILLocalSpec = 
    { locRange: (ILCodeLabel * ILCodeLabel);
      locInfos: ILDebugMapping list }

type structspec = SEH of ILExceptionSpec | LOCAL of ILLocalSpec 

let delayInsertedToWorkaroundKnownNgenBug s f = 
    (* Some random code to prevent inlining of this function *)
    let mutable res = 10
    for i = 0 to 2 do 
       res <- res + 1;
    done;
    //Printf.printf "------------------------executing NGEN bug delay '%s', calling 'f' --------------\n" s;
    let res = f()
    //Printf.printf "------------------------exiting NGEN bug delay '%s' --------------\n" s;
    res


let popRangeM lo hi (m:Zmap.map<'a,'b>) =
    let collect k v (rvs,m) = (v :: rvs) , Zmap.remove k m
    let rvs,m = Zmap.fold_section lo hi collect m ([],m)
    List.rev rvs,m

type BasicBlockStartsToCodeLabelsMap(instrs,tryspecs,localspecs,lab2pc) = 

    (* Find all the interesting looking labels that form the boundaries of basic blocks. *)
    (* These are the destinations of branches and the boundaries of both exceptions and *)
    (* those blocks where locals are live. *)
    let bbstartToCodeLabelMap = 
        let res = ref CodeLabels.empty
        let add_range (a,b) = res := CodeLabels.insert a (CodeLabels.insert b !res)
        instrs |> Array.iter (fun i -> res := CodeLabels.addList (destinations_of_instr i) !res);

        tryspecs |> List.iter (fun espec -> 
          add_range espec.exnRange;
          List.iter (function 
            | SEH_finally r1 | SEH_fault r1 | SEH_type_catch (_,r1)-> add_range r1
            | SEH_filter_catch (r1,r2) -> add_range r1; add_range r2) espec.exnClauses);

        localspecs |> List.iter (fun l -> add_range l.locRange) ;

        !res 

    (* Construct a map that gives a unique ILCodeLabel for each label that *)
    (* might be a boundary of a basic block.  These will be the labels *)
    (* for the basic blocks we end up creating. *)
    let lab2cl_map = Dictionary.create 10 
    let pc2cl_map = Dictionary.create 10 
    let add_bbstart_pc pc pcs cls = 
      if Dictionary.mem pc2cl_map pc then 
        Dictionary.find pc2cl_map pc, pcs, cls
      else 
        let cl = generate_code_label ()
        Dictionary.add pc2cl_map pc cl;
        cl, pc::pcs, CodeLabels.insert cl cls 

    let bbstart_pcs, bbstart_code_labs  = 
      CodeLabels.fold
        (fun bbstart_lab (pcs, cls) -> 
          let pc = lab2pc bbstart_lab
          if logging then dprintf "bblock starts with label %s at pc %d\n" (string_of_code_label bbstart_lab) pc;
          let cl,pcs',cls' = add_bbstart_pc pc pcs cls
          Dictionary.add lab2cl_map bbstart_lab cl;
          pcs',
          cls')
        bbstartToCodeLabelMap 
        ([], CodeLabels.empty) 
    let cl0,bbstart_pcs, bbstart_code_labs = add_bbstart_pc 0 bbstart_pcs bbstart_code_labs 
    
    
    member c.InitialCodeLabel = cl0
    member c.BasicBlockStartPositions = bbstart_pcs
    member c.BasicBlockStartCodeLabels = bbstart_code_labs

    member c.lab2cl bb_lab = try Dictionary.find lab2cl_map bb_lab  with Not_found -> failwith ("basic block label "^string_of_code_label bb_lab^" not declared")  
    member c.pc2cl pc = try Dictionary.find pc2cl_map pc with Not_found -> failwith ("internal error while mapping pc "^string pc^" to code label")  

    member c.remap_labels i =
        match i with 
        | I_leave l -> I_leave(c.lab2cl l)
        | I_br l -> I_br (c.lab2cl l)
        | I_other e -> I_other (find_extension "instr" (fun ext -> if ext.internalInstrExtIs e then Some (ext.internalInstrExtRelabel c.lab2cl e) else None) !instr_extensions)
        | I_brcmp (x,l1,l2) -> I_brcmp(x,c.lab2cl l1, c.lab2cl l2)
        | I_switch (ls,l) -> I_switch(List.map c.lab2cl ls, c.lab2cl l)
        | _ -> i 

let disjoint_range (start_pc1,end_pc1) (start_pc2,end_pc2) =
  ((start_pc1 : int) < start_pc2 && end_pc1 <= start_pc2) or
  (start_pc1 >= end_pc2 && end_pc1 > end_pc2) 

let merge_ranges (start_pc1,end_pc1) (start_pc2,end_pc2) =
  (min (start_pc1:int) start_pc2, max (end_pc1:int) end_pc2) 

let range_inside_range (start_pc1,end_pc1) (start_pc2,end_pc2)  =
  (start_pc1:int) >= start_pc2 && start_pc1 < end_pc2 &&
  (end_pc1:int) > start_pc2 && end_pc1 <= end_pc2 

let lranges_of_clause cl = 
  match cl with 
  | SEH_finally r1 -> [r1]
  | SEH_fault r1 -> [r1]
  | SEH_filter_catch (r1,r2) -> [r1;r2]
  | SEH_type_catch (ty,r1) -> [r1]  

  
type CodeOffsetViewOfLabelledItems(lab2pc) =
    member x.labels_to_range p = let (l1,l2) = p in lab2pc l1, lab2pc l2 

    member x.lrange_inside_lrange ls1 ls2 = 
      range_inside_range (x.labels_to_range ls1) (x.labels_to_range ls2) 
      
    member x.disjoint_lranges ls1 ls2 = 
      disjoint_range (x.labels_to_range ls1) (x.labels_to_range ls2) 

    member x.clause_inside_lrange cl lr =
      List.forall (fun lr1 -> x.lrange_inside_lrange lr1 lr) (lranges_of_clause cl) 

    member x.clauses_inside_lrange cls lr = 
      List.forall 
        (fun cl -> x.clause_inside_lrange cl lr)
        cls 
    
    member x.tryspec_inside_lrange tryspec1 lr =
      (x.lrange_inside_lrange tryspec1.exnRange lr &
       x.clauses_inside_lrange tryspec1.exnClauses lr) 

    member x.tryspec_inside_clause tryspec1 cl =
      List.exists (fun lr -> x.tryspec_inside_lrange tryspec1 lr) (lranges_of_clause cl) 

    member x.locspec_inside_clause locspec1 cl =
      List.exists (fun lr -> x.lrange_inside_lrange locspec1.locRange lr) (lranges_of_clause cl) 

    member x.tryspec_inside_tryspec tryspec1 tryspec2 =
      x.tryspec_inside_lrange tryspec1 tryspec2.exnRange or
      List.exists (fun c2 -> x.tryspec_inside_clause tryspec1 c2) tryspec2.exnClauses 
    
    member x.locspec_inside_tryspec locspec1 tryspec2 =
      x.lrange_inside_lrange locspec1.locRange tryspec2.exnRange or
      List.exists (fun c2 -> x.locspec_inside_clause locspec1 c2) tryspec2.exnClauses 
    
    member x.tryspec_inside_locspec tryspec1 locspec2 =
      x.tryspec_inside_lrange tryspec1 locspec2.locRange 
    
    member x.disjoint_clause_and_lrange cl lr =
      List.forall (fun lr1 -> x.disjoint_lranges lr1 lr) (lranges_of_clause cl) 
    
    member x.disjoint_clauses_and_lrange cls lr = 
      List.forall (fun cl -> x.disjoint_clause_and_lrange cl lr) cls 
    
    member x.disjoint_tryspec_and_lrange tryspec1 lr =
      (x.disjoint_lranges tryspec1.exnRange lr &
       x.disjoint_clauses_and_lrange tryspec1.exnClauses lr) 
    
    member x.disjoint_tryspec_and_clause tryspec1 cl =
      List.forall (fun lr -> x.disjoint_tryspec_and_lrange tryspec1 lr) (lranges_of_clause cl) 

    member x.tryspec_disjoint_from_tryspec tryspec1 tryspec2 =
      x.disjoint_tryspec_and_lrange tryspec1 tryspec2.exnRange &
      List.forall (fun c2 -> x.disjoint_tryspec_and_clause tryspec1 c2) tryspec2.exnClauses 
    
    member x.tryspec_disjoint_from_locspec tryspec1 locspec2 =
      x.disjoint_tryspec_and_lrange tryspec1 locspec2.locRange 
    
    member x.locspec_disjoint_from_locspec locspec1 locspec2 =
      x.disjoint_lranges locspec1.locRange locspec2.locRange 
    
    member x.locspec_inside_locspec locspec1 locspec2 =
      x.lrange_inside_lrange locspec1.locRange locspec2.locRange 
    
    member x.structspec_inside_structspec specA specB = (* only for sanity checks, then can be removed *)
        match specA,specB with
          | SEH   tryspecA,SEH   tryspecB -> x.tryspec_inside_tryspec tryspecA tryspecB
          | SEH   tryspecA,LOCAL locspecB -> x.tryspec_inside_locspec tryspecA locspecB
          | LOCAL locspecA,SEH   tryspecB -> x.locspec_inside_tryspec locspecA tryspecB
          | LOCAL locspecA,LOCAL locspecB -> x.locspec_inside_locspec locspecA locspecB
    

    (* extent (or size) is the sum of range extents *)
    (* We want to build in increasing containment-order, that's a partial order. *)
    (* Size-order implies containment-order, and size-order is a total order. *)
    member x.extent_structspec ss =  
        let extent_range (start_pc,end_pc) = end_pc - start_pc 
        let extent_lrange lrange = extent_range (x.labels_to_range lrange)  
        let extent_locspec locspec = extent_lrange locspec.locRange 
        let extent_list  extent_item items = List.fold (fun acc item -> acc + extent_item item) 0 items 
        let extent_list2 extent_item items = List.fold (fun acc item -> acc + extent_item item) 0 items 
        let extent_clause cl = extent_list extent_lrange (lranges_of_clause cl) 
        let extent_tryspec tryspec = extent_lrange tryspec.exnRange + (extent_list2 extent_clause tryspec.exnClauses) 
        
        match ss with 
        | LOCAL locspec -> extent_locspec locspec 
        | SEH tryspec -> extent_tryspec tryspec 

    (* DIAGNOSTICS: START ------------------------------ *)
    member x.string_of_structspec ss = 
        let string_of_range (l1,l2) = 
          let pc1,pc2 = x.labels_to_range ((l1,l2))
          string_of_code_label l1^"("^string pc1^")-"^ string_of_code_label l2^"("^string pc2^")" 
        let string_of_clause cl = String.concat "+" (List.map string_of_range (lranges_of_clause cl)) 
        let string_of_tryspec tryspec = "tryspec"^ string_of_range tryspec.exnRange ^ "--" ^ String.concat " / " (List.map string_of_clause tryspec.exnClauses) 
        let string_of_locspec locspec = "local "^(String.concat ";" (locspec.locInfos |> List.map (fun l -> l.localName)))^": "^ string_of_range locspec.locRange 
        match ss with 
        | SEH tryspec -> string_of_tryspec tryspec 
        | LOCAL locspec -> string_of_locspec locspec 
            


(* Stage 2b - Given an innermost tryspec, collect together the *)
(* blocks covered by it. Preserve the essential ordering of blocks. *)
let block_for_inner_tryspec (codeOffsetView:CodeOffsetViewOfLabelledItems,
                             coverage_of_codes,
                             addBlocks,
                             compute_covered_blocks,
                             bbstartToCodeLabelMap:BasicBlockStartsToCodeLabelsMap) tryspec state0 = 

    let (blocks, remaining_bblock_starts) = state0
    let try_blocks, other_blocks = compute_covered_blocks (codeOffsetView.labels_to_range tryspec.exnRange) blocks
    if isNil try_blocks then (dprintn "try block specification covers no real code"; state0) else
    let get_clause r other_blocks = 
        let clause_blocks, other_blocks = 
          compute_covered_blocks (*_rough*) (codeOffsetView.labels_to_range r) other_blocks
        if isNil clause_blocks then 
          failwith "clause block specification covers no real code";
        (* The next line computes the code label for the entry to the clause *)
        let clause_entry_lab = bbstartToCodeLabelMap.lab2cl (fst r)
        (* Now compute the overall clause, with labels still visible. *)
        let clause_block = mk_group_block ([],List.map snd clause_blocks)
        (* if logging then dprintf "-- clause entry label is %s" clause_entry_lab; *)
        (clause_entry_lab, clause_blocks, clause_block), other_blocks
    let try_code_blocks = List.map snd try_blocks
    let try_entry_lab = bbstartToCodeLabelMap.lab2cl (fst tryspec.exnRange)
    let try_hidden = 
      CodeLabels.remove try_entry_lab (List.foldBack (entries_of_code' >> CodeLabels.union) try_code_blocks CodeLabels.empty) 
    let try_block =  mk_group_block (CodeLabels.to_list try_hidden,try_code_blocks)

    match tryspec.exnClauses with 
    |  SEH_finally _ :: _ :: _ -> failwith "finally clause combined with others"
    | [ SEH_finally r ] | [ SEH_fault r ] -> 

        let maker =       
          match tryspec.exnClauses with
            [ SEH_finally _ ] -> mk_try_finally_block 
          | [ SEH_fault _ ] -> mk_try_fault_block 
          | _ -> failwith ""

        let (clause_entry_lab, clause_blocks, clause_block), other_blocks = get_clause r other_blocks
        let newblock_range = coverage_of_codes (try_blocks@clause_blocks)
        (* The next construction joins the blocks together. *)
        (* It automatically hides any internal labels used in the *)
        (* clause blocks. Only the entry to the clause is kept visible. *)
        (* We hide the entries to the try block up above. *)
        let newblock =  maker (try_block,clause_entry_lab,clause_block)
        (* None of the entries to the clause block are visible outside the *)
        (* entire try-clause construct, nor the other entries to the try block *)
        (* apart from the one at the. top *)
        let newstarts = CodeLabels.diff remaining_bblock_starts (CodeLabels.union try_hidden (entries_of_code' clause_block))
        (* Now return the new block, the remaining blocks and the new set *)
        (* of entries. *)
        addBlocks other_blocks [(newblock_range, newblock)], newstarts
    | clauses when 
      List.forall
        (function 
          | SEH_filter_catch _ -> true
          | SEH_type_catch _ -> true | _ -> false) 
        clauses   -> 
          
          let clause_infos, other_blocks (*(prior,posterior)*) = 
            List.fold 
              (fun (sofar,other_blocks) cl -> 
                match cl with 
                | SEH_filter_catch(r1,r2) -> 
                    let ((lab1,_,bl1) as info1),other_blocks =  get_clause r1 other_blocks
                    let info2,other_blocks =  get_clause r2 other_blocks
                    (sofar@[(Choice1Of2 (lab1,bl1),info2)]), other_blocks
                | SEH_type_catch(typ,r2) -> 
                    let info2,other_blocks = get_clause r2 other_blocks
                    (sofar@[(Choice2Of2 typ,info2)]), other_blocks
                | _ -> failwith "internal error")
              ([],other_blocks)
              clauses
          let newblock_range = 
            (* Ignore filter blocks when computing this range *)
            (* REVIEW: They must always come before the catch blocks. *)
            coverage_of_codes 
              (try_blocks@
               ((List.collect (fun (_,(_,blocks2,_)) -> blocks2) clause_infos)))
          
          (* The next construction joins the blocks together. *)
          (* It automatically hides any internal labels used in the *)
          (* clause blocks. Only the entry to the clause is kept visible. *)
          let newblock = 
            mk_try_multi_filter_catch_block 
              (try_block,
               List.map 
                 (fun (choice,(lab2,_,bl2)) -> choice, (lab2,bl2)) 
                 clause_infos)
          (* None of the entries to the filter or catch blocks are *)
          (* visible outside the entire exception construct. *)
          let newstarts =
            CodeLabels.diff remaining_bblock_starts 
              (CodeLabels.union try_hidden
                 (List.foldBack 
                    (fun (flt,(_,_,ctch_blck)) acc -> 
                      CodeLabels.union
                        (match flt with 
                         | Choice1Of2 (_,flt_block) -> entries_of_code' flt_block
                         | Choice2Of2 _ -> CodeLabels.empty)
                        (CodeLabels.union (entries_of_code' ctch_blck) acc)) 
                    clause_infos
                    CodeLabels.empty))
          (* Now return the new block, the remaining blocks and the new set *)
          (* of entries. *)
          addBlocks other_blocks [ (newblock_range, newblock)], newstarts
    | _ -> failwith "invalid pattern of exception constructs" 



let do_structure' (codeOffsetView:CodeOffsetViewOfLabelledItems,
                   compute_covered_blocks,
                   coverage_of_codes,
                   addBlocks,
                   bbstartToCodeLabelMap:BasicBlockStartsToCodeLabelsMap)
                 structspecs 
                 block_state =

    (* Stage 2b - Given an innermost tryspec, collect together the *)
    (* blocks covered by it. Preserve the essential ordering of blocks. *)
    let block_for_inner_locspec locspec ((blocks, remaining_bblock_starts) as state0) =
        let scope_blocks, other_blocks (*(prior,posterior)*) = compute_covered_blocks (codeOffsetView.labels_to_range locspec.locRange) blocks
        if isNil scope_blocks then (dprintn "scope block specification covers no real code"; state0) else
        let newblock =  mk_scope_block (locspec.locInfos,mk_group_block ([],List.map snd scope_blocks))
        let newblock_range = coverage_of_codes scope_blocks
        addBlocks other_blocks [ (newblock_range, newblock)], remaining_bblock_starts

    // Require items by increasing inclusion-order.
    // Order by size/extent.
    // a) size-ordering implies containment-ordering.
    // b) size-ordering is total, so works with List.sort
    let build_order sA sB = int_order (codeOffsetView.extent_structspec sA) (codeOffsetView.extent_structspec sB)

    (* checkOrder: checking is O(n^2) *)
(*
    let rec checkOrder = function
      | []      -> ()
      | sA::sBs -> List.iter (fun sB ->
                                if codeOffsetView.structspec_inside_structspec sB sA && not (codeOffsetView.structspec_inside_structspec sA sB) then (
                                  dprintf "sA = %s\n" (codeOffsetView.string_of_structspec sA);
                                  dprintf "sB = %s\n" (codeOffsetView.string_of_structspec sB);
                                  assert false
                                )) sBs;
                   checkOrder sBs
*)

    let structspecs = List.sortWith build_order structspecs

    (* if sanity_check_order then checkOrder structspecs; *) (* note: this check is n^2 *)
    let buildBlock block_state = function
      | SEH   tryspec -> (if logging then dprintn "-- checkin a tryspec";
                          block_for_inner_tryspec (codeOffsetView,coverage_of_codes,addBlocks,compute_covered_blocks,bbstartToCodeLabelMap) tryspec block_state)
      | LOCAL locspec -> (if logging then dprintn "-- checkin a locspec";
                          block_for_inner_locspec locspec block_state)
    List.fold buildBlock block_state structspecs 

            
// REVIEW: this function shows up on performance traces. If we eliminated the last ILX->IL rewrites from the
// F# compiler we could get rid of this structured code representation from Abstract IL altogether, and 
// never convert F# code into this form.
let build_code meth_name lab2pc instrs tryspecs localspecs =

    let bbstartToCodeLabelMap = BasicBlockStartsToCodeLabelsMap(instrs,tryspecs,localspecs,lab2pc)
    let codeOffsetView = CodeOffsetViewOfLabelledItems(lab2pc)

    let basic_instructions = Array.map bbstartToCodeLabelMap.remap_labels instrs
    
    (* DIAGNOSTICS: END -------------------------------- *)

    let build_code_from_instruction_array instrs =

        (* Consume instructions until we hit the end of the basic block, either *)
        (* by hitting a control-flow instruction or by hitting the start of the *)
        (* next basic block by fall-through. *)
        let rec consume_bblock_instrs instrs rinstrs (pc:int) next_bbstart_pc =
          (* rinstrs = accumulates instructions in reverse order *)
          if pc = (Array.length instrs) then 
              dprintn "* WARNING: basic block at end of method ends without a leave, branch, return or throw. Adding throw\n";
              pc,List.rev (I_throw :: rinstrs)
          // The next test is for drop-through at end of bblock, when we just insert 
          // a branch to the next bblock. 
          elif (match next_bbstart_pc with Some pc' -> pc = pc' | _ -> false) then 
              if logging then dprintf "-- pushing br, pc = next_bbstart_pc = %d\n" pc;
              pc,List.rev (I_br (bbstartToCodeLabelMap.pc2cl pc) :: rinstrs)
          else
            // Otherwise bblocks end with control-flow. 
            let i = instrs.[pc]
            let pc' = pc + 1
            if instr_is_bblock_end i then 
                if instr_is_tailcall i then 
                    if pc' = instrs.Length || (match instrs.[pc'] with I_ret -> false | _ -> true) then 
                        failwithf "a tailcall must be followed by a return, instrs = %A" instrs
                    elif (match next_bbstart_pc with Some pc'' -> pc' = pc'' | _ -> false) then
                        // In this obscure case, someone branches to the return instruction 
                        // following the tailcall, so we'd better build a basic block 
                        // containing just that return instruction. 
                        pc', List.rev (i :: rinstrs)
                    else 
                        // Otherwise skip the return instruction, but keep the tailcall. 
                        pc'+1, List.rev (i :: rinstrs)
                else 
                  pc', List.rev (i :: rinstrs)
            else
              // recursive case 
              consume_bblock_instrs instrs (i::rinstrs) pc' next_bbstart_pc

        (* type block = (int * int) * Code // a local type (alias) would be good, good for intelisense too *)
        let rec consume_one_bblock bbstart_pc next_bbstart_pc current_pc =
          if current_pc = (Array.length instrs) then None
          elif bbstart_pc < current_pc then failwith "internal error: bad basic block structure (missing bblock start marker?)"
          elif bbstart_pc > current_pc then
            (* dprintn ("* ignoring unreachable instruction in method: "^ meth_name); *)
            consume_one_bblock 
              bbstart_pc 
              next_bbstart_pc 
              (current_pc + 1)
          else
            let pc', bblock_instrs = consume_bblock_instrs instrs [] bbstart_pc next_bbstart_pc
            if logging then dprintf "-- making bblock, entry label is %s, length = %d, bbstart_pc = %d\n" (string_of_code_label (bbstartToCodeLabelMap.pc2cl bbstart_pc)) (List.length bblock_instrs) bbstart_pc;
            let bblock = mk_bblock {bblockLabel= bbstartToCodeLabelMap.pc2cl bbstart_pc; bblockInstrs=Array.of_list bblock_instrs}
            
            let bblock_range = (bbstart_pc, pc')
            (* Return the bblock and the range of instructions that the bblock covered. *)
            (* Also return any remaining instructions and the pc' for the first *)
            (* such instruction. *)
            Some ((bblock_range, bblock), pc')

        let rec fetch_bblocks bbstartToCodeLabelMap current_pc = 
          match bbstartToCodeLabelMap with 
            [] -> 
              (* if current_pc <> Array.length instrs then 
                 dprintn ("* ignoring instructions at end of method: "^ meth_name); *)
              []
          | h::t -> 
              let h2 = match t with [] -> None | h2:: _ -> assert (not (h = h2)); Some h2
              match consume_one_bblock h h2 current_pc with
              | None -> []
              | Some (bblock, current_pc') -> bblock :: fetch_bblocks t current_pc'

        let inside range (brange,_) =
          if range_inside_range brange range then true else
          if disjoint_range brange range then false else
          failwith "exception block specification overlaps the range of a basic block"

        (* A "blocks" contain blocks, ordered on startPC.
         * Recall, a block is (range,code) where range=(pcStart,pcLast+1). *)
        let addBlock m (((startPC,endPC),code) as block) =
          match Zmap.tryfind startPC m with
            | None        -> Zmap.add startPC [block] m
            | Some blocks -> Zmap.add startPC (block :: blocks) m in  (* NOTE: may reverse block *)

        let addBlocks m blocks = List.fold addBlock m blocks
              
        let mkBlocks blocks =
          let emptyBlocks = (Zmap.empty int_order :  Zmap.map<int,((int*int) * ILCode) list>)
          List.fold addBlock emptyBlocks blocks

        let sanity_check_cover = false in  (* linear check    - REVIEW: set false and elim checks *)
        let sanity_check_order = false in  (* quadratic check - REVIEW: set false and elim checks *)

        let compute_covered_blocks ((start_pc,end_pc) as range) (blocks: Zmap.map<int,((int*int) * ILCode) list> ) =
            (* It is assumed that scopes never overlap.
             * locinfo scopes could overlap if there is a bug elsewhere.
             * If overlaps are discovered, an exception is raised. see NOTE#overlap.
             *)
            let pcCovered,blocks = popRangeM start_pc (end_pc - 1) blocks
            let coveredBlocks = pcCovered |> List.concat
            (* Look for bad input, e.g. overlapping locinfo scopes. *)
            let overlapBlocks = List.filter (inside range >> not) coveredBlocks
            if not (isNil overlapBlocks) then raise Not_found; (* see NOTE#overlap *)
            if sanity_check_cover then (
              let assertIn  block = assert (inside range block)
              let assertOut block = assert (not (inside range block))
              List.iter assertIn coveredBlocks;
              Zmap.iter (fun _ bs -> List.iter assertOut bs) blocks
            );
            coveredBlocks,blocks

        let rec coverage_of_codes blocks = 
          match blocks with 
            [] -> failwith "start_of_codes"
          | [(r,_)] -> r 
          | ((r,_)::t) -> merge_ranges r (coverage_of_codes t)
        
        delayInsertedToWorkaroundKnownNgenBug "Delay4i3" <| fun () ->

        let do_structure = do_structure' (codeOffsetView, compute_covered_blocks,coverage_of_codes,addBlocks,bbstartToCodeLabelMap)
        
        (* Apply stage 1. Compute the blocks not taking exceptions into account. *)
        let bblocks = 
            fetch_bblocks (List.sort bbstartToCodeLabelMap.BasicBlockStartPositions) 0

        let bblocks = mkBlocks bblocks
        (* Apply stage 2. Compute the overall morphed blocks. *)
        let morphed_blocks,remaining_entries = 
            let specs1 = List.map (fun x -> SEH x) tryspecs
            let specs2 = List.map (fun x -> LOCAL x) localspecs
            try do_structure (specs1 @ specs2) (bblocks,bbstartToCodeLabelMap.BasicBlockStartCodeLabels) 
            with Not_found ->
                (* NOTE#overlap.
                 * Here, "Not_found" indicates overlapping scopes were found.
                 * Maybe the calling code got the locspecs scopes wrong.
                 * Try recovery by discarding locspec info...
                 *)
                let string_of_tryspec tryspec = "tryspec"
                let string_of_range (l1,l2) = 
                  let pc1,pc2 = codeOffsetView.labels_to_range ((l1,l2))
                  string_of_code_label l1^"("^string pc1^")-"^ string_of_code_label l2^"("^string pc2^")"
                let string_of_locspec locspec = "local "^(String.concat ";" (locspec.locInfos |> List.map (fun l -> l.localName)))^": "^ string_of_range locspec.locRange
                
                dprintf "\nERROR: could not find an innermost exception block or local scope, specs = \n%s\nTrying again without locals."
                  (String.concat "\n" (List.map string_of_tryspec tryspecs @ List.map string_of_locspec localspecs));
                do_structure specs1 (bblocks,bbstartToCodeLabelMap.BasicBlockStartCodeLabels) 

        delayInsertedToWorkaroundKnownNgenBug "Delay4k" <| fun () ->

        let morphed_blocks = Zmap.values morphed_blocks |> List.concat in (* NOTE: may mixup order *)
        (* Now join up all the remaining blocks into one block with one entry. *)
        if logging then dprintn "-- computing entry label";
        if logging then dprintn ("-- entry label is "^string_of_code_label bbstartToCodeLabelMap.InitialCodeLabel);
        mk_group_block 
          (CodeLabels.to_list (CodeLabels.remove bbstartToCodeLabelMap.InitialCodeLabel remaining_entries),List.map snd morphed_blocks)


    try build_code_from_instruction_array basic_instructions
    with e -> 
      dprintn ("* error while converting instructions to code for method: " ^meth_name);
      rethrow()


(* -------------------------------------------------------------------- 
 * Detecting Delegates
 * -------------------------------------------------------------------- *)

let mk_delegate_mdefs ilg (parms,rtv:ILReturnValue) = 
  let rty = rtv.Type
  let one nm args ret =
    let mdef = mk_virtual_mdef (nm,MemAccess_public,args,(mk_return ret),MethodBody_abstract)
    let mdef = 
      {mdef with mdKind=
                  match mdef.mdKind with 
                  | MethodKind_virtual vinfo -> MethodKind_virtual {vinfo with virtAbstract=false; } 
                  | k -> k }
    {mdef with 
      mdCodeKind=MethodCodeKind_runtime;
      mdHideBySig=true; }
  let ctor = mk_ctor(MemAccess_public, [ mk_named_param("object",ilg.typ_Object); mk_named_param("method",ilg.typ_IntPtr) ], MethodBody_abstract)
  let ctor = { ctor with  mdCodeKind=MethodCodeKind_runtime; mdHideBySig=true }
  [ ctor;
    one "Invoke" parms rty;
    one "BeginInvoke" (parms @ [mk_named_param("callback",ilg.typ_AsyncCallback);
                                 mk_named_param("objects",ilg.typ_Object) ] ) ilg.typ_IAsyncResult;
    one "EndInvoke" [mk_named_param("result",ilg.typ_IAsyncResult)] rty; ]
    

let mk_ctor_mspec_for_delegate ilg (tref:ILTypeRef,cinst,useUIntPtr) =
  let scoref = tref.Scope
  mk_nongeneric_instance_mspec_in_tref (tref,AsObject,".ctor",[rescope_typ scoref ilg.typ_Object; rescope_typ scoref (if useUIntPtr then ilg.typ_UIntPtr else ilg.typ_IntPtr)],Type_void,cinst) 

type ILEnumInfo =
    { enumValues: (string * ILFieldInit) list;  
      enumType: ILType }

let typ_of_enum_info info = info.enumType

let info_for_enum (tdName,tdFieldDefs) = 
  match (List.partition (fun fd -> fd.fdStatic) (dest_fdefs tdFieldDefs)) with 
  | sfds,[vfd] -> 
      { enumType = vfd.fdType; 
        enumValues = List.map (fun fd -> (fd.fdName, match fd.fdInit with Some i -> i | None -> failwith ("info_of_enum_tdef: badly formed enum "^tdName^": static field does not have an default value"))) sfds }
  | _,[] -> failwith ("info_of_enum_tdef: badly formed enum "^tdName^": no non-static field found")
  | _,_ -> failwith ("info_of_enum_tdef: badly formed enum "^tdName^": more than one non-static field found")

 
(* --------------------------------------------------------------------
 * Intern tables to save space.
 * -------------------------------------------------------------------- *)

let new_intern_table ()  =
  let idx = ref 0
  let t = new Dictionary<_,_>(100)
  fun s -> 
    if t.ContainsKey s then t.[s]
    else let i = !idx in incr idx; (t.[s] <- (s,i); (s,i))

let new_idx_intern_table idf tagf  =
  let idx = ref 0
  let t = new Dictionary<_,_>(100)
  fun s -> 
    match idf s with 
    | Some i -> s,i
    | None -> 
      if t.ContainsKey s then 
        let r = t.[s]
        let i = (match idf r with Some x -> x | None -> failwith "new_idx_intern_table: internal error")
        r,i
      else begin
        incr idx; 
        let i = !idx
        let r = (tagf s i)
        t.[s] <- r; 
        r,i
      end

let memoize_on keyf f = 
  let t = new Dictionary<_,_>(100)
  fun x -> 
    let idx = keyf x
    if t.ContainsKey idx then t.[idx]
    else let r = f x in t.[idx] <- r;  r

let memoize f = 
  let t = new Dictionary<_,_>(1000)
  fun x -> 
    if t.ContainsKey x then t.[ x ]
    else let r = f x in t.[x] <- r;  r

(*---------------------------------------------------------------------
 * Get the public key token from the public key.
 *---------------------------------------------------------------------*)


let assref_for_manifest m = 
  ILAssemblyRef.Create(m.manifestName, 
                     // REVIEW: find hash?? 
                     None, 
                     (match m.manifestPublicKey with Some k -> Some (PublicKey.KeyAsToken(k)) | None -> None),
                     false, m.manifestVersion, m.manifestLocale)

let assref_for_mainmod (mainmod:ILModuleDef) = 
  assref_for_manifest mainmod.ManifestOfAssembly




let z_unsigned_int_size n = 
  if n <= 0x7F then 1
  elif n <= 0x3FFF then 2
  else 3

let z_unsigned_int n = 
  if n >= 0 &&  n <= 0x7F then [| n |] 
  else 
      (if n >= 0x80 && n <= 0x3FFF then [| 0x80 ||| (n lsr 8); n &&& 0xFF |] 
       else [| 0xc0 ||| (n lsr 24); (n lsr 16) &&& 0xFF; (n lsr 8) &&& 0xFF; n &&& 0xFF |])

let string_as_utf8_intarray (s:string) = Bytes.to_intarray (Bytes.string_as_utf8_bytes s)

(* Little-endian encoding of int64 *)
let dw7 n = int32 ((n >>> 56) &&& (int64 0xFF))
let dw6 n = int32 ((n >>> 48) &&& (int64 0xFF))
let dw5 n = int32 ((n >>> 40) &&& (int64 0xFF))
let dw4 n = int32 ((n >>> 32) &&& (int64 0xFF))
let dw3 n = int32 ((n >>> 24) &&& (int64 0xFF))
let dw2 n = int32 ((n >>> 16) &&& (int64 0xFF))
let dw1 n = int32 ((n >>> 8)  &&& (int64 0xFF))
let dw0 n = int32 (n          &&& (int64 0xFF))

let u8_as_intarray i = [| b0 (int i) |]
let u16_as_intarray x =  let n = (int x) in [| b0 n; b1 n |]
let i32_as_intarray i = [| b0 i; b1 i; b2 i; b3 i |]
let i64_as_intarray i = [| dw0 i; dw1 i; dw2 i; dw3 i; dw4 i; dw5 i; dw6 i; dw7 i |]

let i8_as_intarray (i:sbyte) = u8_as_intarray (byte i)
let i16_as_intarray (i:int16) = u16_as_intarray (uint16 i)
let u32_as_intarray (i:uint32) = i32_as_intarray (int32 i)
let u64_as_intarray (i:uint64) = i64_as_intarray (int64 i)

let bits_of_float32 (x:float32) = System.BitConverter.ToInt32(System.BitConverter.GetBytes(x),0)
let bits_of_float (x:float) = System.BitConverter.DoubleToInt64Bits(x)

let ieee32_as_intarray i = i32_as_intarray (bits_of_float32 i)
let ieee64_as_intarray i = i64_as_intarray (bits_of_float i)

/// Given a custom attribute element, work out the type of the .NET argument for that element
let rec celem_ty ilg x = 
  match x with
        | CustomElem_string _ -> ilg.typ_String
        | CustomElem_bool _ -> ilg.typ_bool
        | CustomElem_char _ -> ilg.typ_char
        | CustomElem_int8 _ -> ilg.typ_int8
        | CustomElem_int16 _ -> ilg.typ_int16
        | CustomElem_int32 _ -> ilg.typ_int32
        | CustomElem_int64 _ -> ilg.typ_int64
        | CustomElem_uint8 _ -> ilg.typ_uint8
        | CustomElem_uint16 _ -> ilg.typ_uint16
        | CustomElem_uint32 _ -> ilg.typ_uint32
        | CustomElem_uint64 _ -> ilg.typ_uint64
        | CustomElem_type _ -> ilg.typ_Type
        | CustomElem_tref _ -> ilg.typ_Type
        | CustomElem_float32 _ -> ilg.typ_float32
        | CustomElem_float64 _ -> ilg.typ_float64
        | CustomElem_array _ -> failwith "Unexpected array element"

let et_END = 0x00
let et_VOID = 0x01
let et_BOOLEAN = 0x02
let et_CHAR = 0x03
let et_I1 = 0x04
let et_U1 = 0x05
let et_I2 = 0x06
let et_U2 = 0x07
let et_I4 = 0x08
let et_U4 = 0x09
let et_I8 = 0x0a
let et_U8 = 0x0b
let et_R4 = 0x0c
let et_R8 = 0x0d
let et_STRING = 0x0e
let et_PTR = 0x0f
let et_BYREF = 0x10
let et_VALUETYPE      = 0x11
let et_CLASS          = 0x12
let et_VAR            = 0x13
let et_ARRAY          = 0x14
let et_WITH           = 0x15
let et_TYPEDBYREF     = 0x16
let et_I              = 0x18
let et_U              = 0x19
let et_FNPTR          = 0x1B
let et_OBJECT         = 0x1C
let et_SZARRAY        = 0x1D
let et_MVAR           = 0x1e
let et_CMOD_REQD      = 0x1F
let et_CMOD_OPT       = 0x20

let version_to_string ((a,b,c,d):ILVersionInfo) = Printf.sprintf "%d.%d.%d.%d" (int a) (int b) (int c) (int d)

  

let celem_serstring s = 
  let arr = string_as_utf8_intarray s
  Array.concat [ z_unsigned_int (Array.length arr); arr ]      

let rec celem_enc_ty isNamedArg x = 
  match x with
  | Type_value tspec when tspec.Name = "System.SByte" ->  [| et_I1 |]
  | Type_value tspec when tspec.Name = "System.Byte" ->  [| et_U1 |]
  | Type_value tspec when tspec.Name = "System.Int16" ->  [| et_I2 |]
  | Type_value tspec when tspec.Name = "System.UInt16" ->  [| et_U2 |]
  | Type_value tspec when tspec.Name = "System.Int32" ->  [| et_I4 |]
  | Type_value tspec when tspec.Name = "System.UInt32" ->  [| et_U4 |]
  | Type_value tspec when tspec.Name = "System.Int64" ->  [| et_I8 |]
  | Type_value tspec when tspec.Name = "System.UInt64" ->  [| et_U8 |]
  | Type_value tspec when tspec.Name = "System.Double" ->  [| et_R8 |]
  | Type_value tspec when tspec.Name = "System.Single" ->  [| et_R4 |]
  | Type_value tspec when tspec.Name = "System.Char" ->  [| et_CHAR |]
  | Type_value tspec when tspec.Name = "System.Boolean" ->  [| et_BOOLEAN |]
  | Type_boxed tspec when tspec.Name = "System.String" ->  [| et_STRING |]
  | Type_boxed tspec when tspec.Name = "System.Object" ->  [| et_OBJECT |]
  | Type_boxed tspec when tspec.Name = "System.Type" ->  [| 0x50 |]
  | Type_value tspec ->  
       if isNamedArg then 
           Array.append [| 0x55 |] (celem_serstring tspec.TypeRef.QualifiedNameWithNoShortMscorlib)
       else (* assume it is an enumeration *) [| et_I4 |]
  | _ ->  failwith "celem_enc_ty: unrecognized custom element type"

let rec celem_dec_ty ilg x = 
  match x with
  | x when x =  et_I1 -> ilg.typ_SByte
  | x when x = et_U1 -> ilg.typ_Byte
  | x when x =  et_I2 -> ilg.typ_Int16
  | x when x =  et_U2 -> ilg.typ_UInt16
  | x when x =  et_I4 -> ilg.typ_Int32
  | x when x =  et_U4 -> ilg.typ_UInt32
  | x when x =  et_I8 -> ilg.typ_Int64
  | x when x =  et_U8 -> ilg.typ_UInt64
  | x when x =  et_R8 -> ilg.typ_Double
  | x when x =  et_R4 -> ilg.typ_Single
  | x when x = et_CHAR -> ilg.typ_Char
  | x when x =  et_BOOLEAN -> ilg.typ_Bool
  | x when x =  et_STRING -> ilg.typ_String
  | x when x =  et_OBJECT -> ilg.typ_Object
  | x when x = 0x50 -> ilg.typ_Type
  | _ ->  failwith "celem_dec_ty ilg: unrecognized custom element type"


/// Given a custom attribute element, encode it to a binary representation according to the rules in Ecma 335 Partition II.
let rec celem_val_prim c = 
  match c with 
  | CustomElem_bool b -> [| if b then 0x01 else 0x00 |]
  | CustomElem_string None -> [| 0xFF |]
  | CustomElem_string (Some(s)) -> celem_serstring s
  | CustomElem_char x -> u16_as_intarray (uint16 x)
  | CustomElem_int8 x -> i8_as_intarray x
  | CustomElem_int16 x -> i16_as_intarray x
  | CustomElem_int32 x -> i32_as_intarray x
  | CustomElem_int64 x -> i64_as_intarray x
  | CustomElem_uint8 x -> u8_as_intarray x
  | CustomElem_uint16 x -> u16_as_intarray x
  | CustomElem_uint32 x -> u32_as_intarray x
  | CustomElem_uint64 x -> u64_as_intarray x
  | CustomElem_float32 x -> ieee32_as_intarray x
  | CustomElem_float64 x -> ieee64_as_intarray x
  | CustomElem_type ty -> celem_serstring ty.QualifiedNameWithNoShortMscorlib 
  | CustomElem_tref tref -> celem_serstring tref.QualifiedNameWithNoShortMscorlib 
  | CustomElem_array _ -> failwith "unreachable"

let rec celem_val ilg ty c = 
    match ty, c with 
    | Type_boxed tspec, _ when tspec.Name = "System.Object" ->  
       Array.concat [ celem_enc_ty false (celem_ty ilg c); celem_val_prim c ]
    | Type_array(shape,elemType), CustomElem_array(elems) when shape = Rank1ArrayShape  ->  
       Array.concat [ i32_as_intarray (List.length elems); Array.concat (List.map (celem_val ilg elemType) elems) ]
    | _ -> 
       celem_val_prim c

let encode_named_arg ilg (nm,ty,prop,elem) = 
   [| yield (if prop then 0x54 else 0x53) 
      yield! celem_enc_ty true ty;
      yield! celem_serstring nm;
      yield! celem_val ilg ty elem |]

let mk_custom_attribute_mref ilg (mspec:ILMethodSpec,fixedArgs: list<_>,namedArgs: list<_>) = 
    let argtys = mspec.MethodRef.ArgTypes
    let args = 
      [| yield! [| 0x01; 0x00; |]
         for (argty,fixedArg) in Seq.zip argtys fixedArgs  do
            yield! celem_val ilg argty fixedArg
         yield! u16_as_intarray (uint16 namedArgs.Length) 
         for namedArg in namedArgs do 
             yield! encode_named_arg ilg namedArg |]

    { customMethod = mspec;
      customData = Bytes.of_intarray args }

let mk_custom_attribute ilg (tref,argtys,argvs,propvs) = 
  mk_custom_attribute_mref ilg (mk_ctor_mspec_for_nongeneric_boxed_tref (tref,argtys),argvs,propvs)

(* Q: CompilerGeneratedAttribute is new in 2.0. Unconditional generation of this attribute prevents running on 1.1 Framework. (discovered running on early mono version). *)
let tref_CompilerGeneratedAttribute   ilg = mk_tref (ilg.mscorlib_scoref,tname_CompilerGeneratedAttribute)

let tname_DebuggerNonUserCodeAttribute = "System.Diagnostics.DebuggerNonUserCodeAttribute"
let tname_DebuggableAttribute_DebuggingModes = "DebuggingModes"
let tname_DebuggerHiddenAttribute = "System.Diagnostics.DebuggerHiddenAttribute"
let tname_DebuggerDisplayAttribute = "System.Diagnostics.DebuggerDisplayAttribute"
let tname_DebuggerTypeProxyAttribute = "System.Diagnostics.DebuggerTypeProxyAttribute"
let tname_DebuggerStepThroughAttribute = "System.Diagnostics.DebuggerStepThroughAttribute"
let tname_DebuggerBrowsableAttribute = "System.Diagnostics.DebuggerBrowsableAttribute"
let tname_DebuggerBrowsableState = "System.Diagnostics.DebuggerBrowsableState"

let tref_DebuggerNonUserCodeAttribute ilg = mk_tref (ilg.mscorlib_scoref,tname_DebuggerNonUserCodeAttribute)
let tref_DebuggerHiddenAttribute ilg = mk_tref (ilg.mscorlib_scoref,tname_DebuggerHiddenAttribute)
let tref_DebuggerDisplayAttribute ilg = mk_tref (ilg.mscorlib_scoref,tname_DebuggerDisplayAttribute)
let tref_DebuggerTypeProxyAttribute ilg = mk_tref (ilg.mscorlib_scoref,tname_DebuggerTypeProxyAttribute)
let tref_DebuggerBrowsableAttribute ilg = mk_tref (ilg.mscorlib_scoref,tname_DebuggerBrowsableAttribute)
let tref_DebuggableAttribute          ilg = mk_tref (ilg.mscorlib_scoref,tname_DebuggableAttribute)
let tref_DebuggableAttribute_DebuggingModes ilg = mk_nested_tref (ilg.mscorlib_scoref,[tname_DebuggableAttribute],tname_DebuggableAttribute_DebuggingModes)

let typ_DebuggerBrowsableState ilg = 
    let tref_DebuggerBrowsableState = mk_tref(ilg.mscorlib_scoref,tname_DebuggerBrowsableState)
    Type_value (mk_tspec(tref_DebuggerBrowsableState,mk_empty_gactuals))

let mk_CompilerGeneratedAttribute   ilg = mk_custom_attribute ilg (tref_CompilerGeneratedAttribute ilg,[],[],[])
let mk_DebuggerHiddenAttribute ilg = mk_custom_attribute ilg (tref_DebuggerHiddenAttribute ilg,[],[],[])
let mk_DebuggerDisplayAttribute ilg s = mk_custom_attribute ilg (tref_DebuggerDisplayAttribute ilg,[ilg.typ_String],[CustomElem_string (Some s)],[])
let mk_DebuggerTypeProxyAttribute ilg ty = mk_custom_attribute ilg (tref_DebuggerTypeProxyAttribute ilg,[ilg.typ_Type],[CustomElem_tref (tref_of_typ ty)],[])
let mk_DebuggerBrowsableAttribute ilg n = mk_custom_attribute ilg (tref_DebuggerBrowsableAttribute ilg,[typ_DebuggerBrowsableState ilg],[CustomElem_int32 n],[])
let mk_DebuggerBrowsableNeverAttribute ilg = mk_DebuggerBrowsableAttribute  ilg 0
let mk_DebuggerBrowsableCollapsedAttribute ilg = mk_DebuggerBrowsableAttribute  ilg 2
let mk_DebuggerBrowsableRootHiddenAttribute ilg = mk_DebuggerBrowsableAttribute  ilg 3
let mk_DebuggerNonUserCodeAttribute ilg = mk_custom_attribute ilg (tref_DebuggerNonUserCodeAttribute ilg,[],[],[])
let mk_DebuggableAttribute ilg (jitTracking, jitOptimizerDisabled) = 
    mk_custom_attribute ilg (tref_DebuggableAttribute ilg,[ilg.typ_Bool;ilg.typ_Bool], [CustomElem_bool jitTracking; CustomElem_bool jitOptimizerDisabled],[])


// Bug 2129. Requests attributes to be added to compiler generated methods 
let add_generated_attrs ilg attrs = mk_custom_attrs (dest_custom_attrs attrs @ [mk_CompilerGeneratedAttribute ilg;mk_DebuggerNonUserCodeAttribute ilg])

let add_mdef_generated_attrs ilg (mdef:ILMethodDef)   = {mdef with mdCustomAttrs   = add_generated_attrs ilg mdef.mdCustomAttrs}
let add_pdef_generated_attrs ilg (pdef:ILPropertyDef) = {pdef with propCustomAttrs = add_generated_attrs ilg pdef.propCustomAttrs}
let add_fdef_generated_attrs ilg (fdef:ILFieldDef) = {fdef with fdCustomAttrs = add_generated_attrs ilg fdef.fdCustomAttrs}

let add_never_attrs ilg attrs = mk_custom_attrs (dest_custom_attrs attrs @ [mk_DebuggerBrowsableNeverAttribute ilg])
let add_pdef_never_attrs ilg (pdef:ILPropertyDef) = {pdef with propCustomAttrs = add_never_attrs ilg pdef.propCustomAttrs}
let add_fdef_never_attrs ilg (fdef:ILFieldDef) = {fdef with fdCustomAttrs = add_never_attrs ilg fdef.fdCustomAttrs}


// PermissionSet is a 'blob' having the following format:
// • A byte containing a period (.).
// • A compressed int32 containing the number of attributes encoded in the blob.
// • An array of attributes each containing the following:
// o A String, which is the fully-qualified type name of the attribute. (Strings are encoded
// as a compressed int to indicate the size followed by an array of UTF8 characters.)
// o A set of properties, encoded as the named arguments to a custom attribute would be (as
// in §23.3, beginning with NumNamed).
let mk_permission_set ilg (action,attributes: list<(ILTypeRef * (string * ILType * ILAttributeElement) list)>) = 
    let bytes = 
        [| yield (int '.');
           yield! z_unsigned_int attributes.Length;
           for (tref:ILTypeRef,props) in attributes do 
              yield! celem_serstring tref.QualifiedNameWithNoShortMscorlib 
              let bytes = 
                  [| yield! z_unsigned_int props.Length;
                      for (nm,typ,value) in props do 
                          yield! encode_named_arg ilg (nm,typ,true,value)|]
              yield! z_unsigned_int bytes.Length;
              yield! bytes |]
              
    PermissionSet(action,Bytes.of_intarray bytes)


//---------------------------------------------------------------------
// Primitives to help read signatures.  These do not use the file cursor, but
// pass around an int index
//---------------------------------------------------------------------

let sigptr_get_byte bytes sigptr = 
    Bytes.get bytes sigptr, sigptr + 1

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
    let b0,sigptr = sigptr_get_byte bytes sigptr
    let b1,sigptr = sigptr_get_byte bytes sigptr
    let b2,sigptr = sigptr_get_byte bytes sigptr
    let b3,sigptr = sigptr_get_byte bytes sigptr
    b0 ||| (b1 <<< 8) ||| (b2 <<< 16) ||| (b3 <<< 24),sigptr

let sigptr_get_u32 bytes sigptr = 
    let u,sigptr = sigptr_get_i32 bytes sigptr
    uint32 u,sigptr

let sigptr_get_i64 bytes sigptr = 
    let b0,sigptr = sigptr_get_byte bytes sigptr
    let b1,sigptr = sigptr_get_byte bytes sigptr
    let b2,sigptr = sigptr_get_byte bytes sigptr
    let b3,sigptr = sigptr_get_byte bytes sigptr
    let b4,sigptr = sigptr_get_byte bytes sigptr
    let b5,sigptr = sigptr_get_byte bytes sigptr
    let b6,sigptr = sigptr_get_byte bytes sigptr
    let b7,sigptr = sigptr_get_byte bytes sigptr
    int64 b0 ||| (int64 b1 <<< 8) ||| (int64 b2 <<< 16) ||| (int64 b3 <<< 24) |||
    (int64 b4 <<< 32) ||| (int64 b5 <<< 40) ||| (int64 b6 <<< 48) ||| (int64 b7 <<< 56),
    sigptr

let sigptr_get_u64 bytes sigptr = 
    let u,sigptr = sigptr_get_i64 bytes sigptr
    uint64 u,sigptr

let float32_of_bits (x:int32) = System.BitConverter.ToSingle(System.BitConverter.GetBytes(x),0)
let float_of_bits (x:int64) = System.BitConverter.Int64BitsToDouble(x)

let sigptr_get_ieee32 bytes sigptr = 
    let u,sigptr = sigptr_get_i32 bytes sigptr
    float32_of_bits u,sigptr

let sigptr_get_ieee64 bytes sigptr = 
    let u,sigptr = sigptr_get_i64 bytes sigptr
    float_of_bits u,sigptr

let sigptr_get_intarray n bytes sigptr = 
  let res = Bytes.zero_create n
  for i = 0 to (n - 1) do 
    Bytes.set res i (Bytes.get bytes (sigptr + i))
  done;
  res, sigptr + n

let sigptr_get_string n bytes sigptr = 
  let intarray,sigptr = sigptr_get_intarray n bytes sigptr
  Bytes.utf8_bytes_as_string intarray , sigptr
   
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

let sigptr_get_serstring  bytes sigptr = 
    let len,sigptr = sigptr_get_z_i32 bytes sigptr 
    sigptr_get_string ( len) bytes sigptr 
  
let sigptr_get_serstring_possibly_null  bytes sigptr = 
    let b0,_ = sigptr_get_byte bytes sigptr   (* throw away sigptr *)
    if b0 = 0xFF then
        None,sigptr
    else  
        let len,sigptr = sigptr_get_z_i32 bytes sigptr 
        let s, sigptr = sigptr_get_string len bytes sigptr
        Some(s),sigptr
  
let decode_il_attrib_data ilg ca = 
    let bytes = ca.customData
    let sigptr = 0
    let bb0,sigptr = sigptr_get_byte bytes sigptr
    let bb1,sigptr = sigptr_get_byte bytes sigptr
    if not (bb0 = 0x01 && bb1 = 0x00) then failwith "decode_simple_cattr_data: invalid data";
    let rec parseVal argty sigptr = 
      match argty with 
      | Type_value tspec when tspec.Name = "System.SByte" ->  
          let n,sigptr = sigptr_get_i8 bytes sigptr
          CustomElem_int8 n, sigptr
      | Type_value tspec when tspec.Name = "System.Byte" ->  
          let n,sigptr = sigptr_get_u8 bytes sigptr
          CustomElem_uint8 n, sigptr
      | Type_value tspec when tspec.Name = "System.Int16" ->  
          let n,sigptr = sigptr_get_i16 bytes sigptr
          CustomElem_int16 n, sigptr
      | Type_value tspec when tspec.Name = "System.UInt16" ->  
          let n,sigptr = sigptr_get_u16 bytes sigptr
          CustomElem_uint16 n, sigptr
      | Type_value tspec when tspec.Name = "System.Int32" ->  
          let n,sigptr = sigptr_get_i32 bytes sigptr
          CustomElem_int32 n, sigptr
      | Type_value tspec when tspec.Name = "System.UInt32" ->  
          let n,sigptr = sigptr_get_u32 bytes sigptr
          CustomElem_uint32 n, sigptr
      | Type_value tspec when tspec.Name = "System.Int64" ->  
          let n,sigptr = sigptr_get_i64 bytes sigptr
          CustomElem_int64 n, sigptr
      | Type_value tspec when tspec.Name = "System.UInt64" ->  
          let n,sigptr = sigptr_get_u64 bytes sigptr
          CustomElem_uint64 n, sigptr
      | Type_value tspec when tspec.Name = "System.Double" ->  
          let n,sigptr = sigptr_get_ieee64 bytes sigptr
          CustomElem_float64 n, sigptr
      | Type_value tspec when tspec.Name = "System.Single" ->  
          let n,sigptr = sigptr_get_ieee32 bytes sigptr
          CustomElem_float32 n, sigptr
      | Type_value tspec when tspec.Name = "System.Char" ->  
          let n,sigptr = sigptr_get_u16 bytes sigptr
          CustomElem_char (char (int32 n)), sigptr
      | Type_value tspec when tspec.Name = "System.Boolean" ->  
          let n,sigptr = sigptr_get_byte bytes sigptr
          CustomElem_bool (not (n = 0)), sigptr
      | Type_boxed tspec when tspec.Name = "System.String" ->  
          let n,sigptr = sigptr_get_serstring_possibly_null bytes sigptr
          CustomElem_string n, sigptr
      | Type_boxed tspec when tspec.Name = "System.Type" ->  
          let n,sigptr = sigptr_get_serstring bytes sigptr
          CustomElem_tref (mk_tref(ilg.mscorlib_scoref,n)), sigptr
      | Type_boxed tspec when tspec.Name = "System.Object" ->  
          let et,sigptr = sigptr_get_u8 bytes sigptr
          let ty = celem_dec_ty ilg (int et)
          parseVal ty sigptr 
      | Type_array(shape,elemTy) when shape = Rank1ArrayShape ->  
          let n,sigptr = sigptr_get_i32 bytes sigptr
          let rec parseElems acc n sigptr = 
            if n = 0 then List.rev acc else
            let v,sigptr = parseVal elemTy sigptr
            parseElems (v ::acc) (n-1) sigptr
          let elems = parseElems [] n sigptr
          CustomElem_array(elems), sigptr
      | Type_value _ ->  (* assume it is an enumeration *)
          let n,sigptr = sigptr_get_i32 bytes sigptr
          CustomElem_int32 n, sigptr
      | _ ->  failwith "decode_simple_cattr_data: attribute data involves an enum or System.Type value"
    let rec parseFixed argtys sigptr = 
      match argtys with 
        [] -> [],sigptr
      | h::t -> 
          let nh,sigptr = parseVal h sigptr
          let nt,sigptr = parseFixed t sigptr
          nh ::nt, sigptr
    let fixedArgs,sigptr = parseFixed (ca.Method.FormalArgTypes) sigptr
    let nnamed,sigptr = sigptr_get_u16 bytes sigptr
    let rec parseNamed acc n sigptr = 
      if n = 0 then List.rev acc else
      let isPropByte,sigptr = sigptr_get_u8 bytes sigptr
      let isProp = (int isPropByte = 0x54)
      let et,sigptr = sigptr_get_u8 bytes sigptr
      let ty = celem_dec_ty ilg (int et)
      let nm,sigptr = sigptr_get_serstring bytes sigptr
      let v,sigptr = parseVal ty sigptr
      parseNamed ((nm,ty,isProp,v) ::acc) (n-1) sigptr
    let named = parseNamed [] (int nnamed) sigptr
    fixedArgs,named
      

let mk_DebuggableAttribute_v2 ilg (jitTracking, ignoreSymbolStoreSequencePoints, jitOptimizerDisabled,enableEnC) = 
  mk_custom_attribute ilg 
    (tref_DebuggableAttribute ilg,[mk_nongeneric_value_typ (tref_DebuggableAttribute_DebuggingModes ilg)],
     [CustomElem_int32( 
                      (* See System.Diagnostics.DebuggableAttribute.DebuggingModes *)
                        (if jitTracking then 1 else 0) |||  
                        (if jitOptimizerDisabled then 256 else 0) |||  
                        (if ignoreSymbolStoreSequencePoints then 2 else 0) |||
                        (if enableEnC then 4 else 0))],[])

// -------------------------------------------------------------------- 
// Functions to collect up all the references in a full module or
// asssembly manifest.  The process also allocates
// a unique name to each unique internal assembly reference.
// -------------------------------------------------------------------- 

type ILReferences = 
    { refsAssembly: ILAssemblyRef list; 
      refsModul: ILModuleRef list; }
    member x.AssemblyReferences = x.refsAssembly
    member x.ModuleReferences = x.refsModul

let insert_aref (e:ILAssemblyRef) l = if List.mem e l.refsAssembly then l else {l with refsAssembly=e::l.refsAssembly}
let insert_mref (e:ILModuleRef) l = if List.mem e l.refsModul then l else {l with refsModul=e::l.refsModul}

module Hashset = 
  type 'a t = Dictionary<'a,int>
  let create n = new Dictionary<'a,int>(n:int)
  let mem (t: 'a t) x = t.ContainsKey x
  let add (t: 'a t) x = if not (t.ContainsKey x) then t.[x] <- 0
  let fold f t acc = Dictionary.fold (fun x y z -> f x z) t acc


type refstate = 
    { refsA: ILAssemblyRef Hashset.t; 
      refsM: ILModuleRef Hashset.t; }

let empty_refs = 
  { refsAssembly=[];
    refsModul = []; }

let iter_option f x = match x with None -> () | Some x -> f x
let iter_pair f1 f2 (x,y) = f1 x; f2 y 

(* Now find references. *)
let refs_of_assref s x = Hashset.add s.refsA x
let refs_of_modref s x = Hashset.add s.refsM x
    
let refs_of_scoref s x = 
    match x with 
    | ScopeRef_local -> () 
    | ScopeRef_assembly assref -> refs_of_assref s assref
    | ScopeRef_module modref -> refs_of_modref s modref  

let refs_of_tref s (x:ILTypeRef) = refs_of_scoref s x.Scope
  
let rec refs_of_typ s x = 
  match x with
  | Type_void |  Type_tyvar _ -> ()
  | Type_modified(_,ty1,ty2) -> refs_of_tref s ty1; refs_of_typ s ty2
  | Type_array (_,ty)
  | Type_ptr ty | Type_byref ty -> refs_of_typ s ty 
  | Type_value tr | Type_boxed tr -> refs_of_tspec s tr
  | Type_fptr mref -> refs_of_callsig s mref 
and refs_of_inst s i = List.iter (refs_of_typ s) i
and refs_of_tspec s (x:ILTypeSpec) = refs_of_tref s x.TypeRef;  refs_of_inst s x.GenericArgs
and refs_of_callsig s csig  = refs_of_typs s csig.callsigArgs; refs_of_typ s csig.callsigReturn
and refs_of_genparam s x = refs_of_typs s x.gpConstraints
and refs_of_genparams s b = List.iter (refs_of_genparam s) b
    
and refs_of_dloc s ts = refs_of_tref s ts
   
and refs_of_mref s (x:ILMethodRef) = 
    refs_of_dloc s x.EnclosingTypeRef  ;
    List.iter (refs_of_typ s) x.mrefArgs;
    refs_of_typ s x.mrefReturn
    
and refs_of_fref s x = refs_of_tref s x.frefParent; refs_of_typ s x.frefType
and refs_of_ospec s (OverridesSpec(mref,ty)) = refs_of_mref s mref; refs_of_typ s ty 
and refs_of_mspec s x = 
    let x1,x2,x3 = dest_mspec x
    refs_of_mref s x1;
    refs_of_typ s x2;
    refs_of_inst s x3

and refs_of_fspec s x =
    refs_of_fref s x.fspecFieldRef;
    refs_of_typ s x.fspecEnclosingType

and refs_of_typs s l = List.iter (refs_of_typ s) l
  
and refs_of_token s x = 
  match x with
  | Token_type ty -> refs_of_typ s ty
  | Token_method mr -> refs_of_mspec s mr
  | Token_field fr -> refs_of_fspec s fr
and refs_of_custom_attr s x = refs_of_mspec s x.customMethod
    
and refs_of_custom_attrs s cas = List.iter (refs_of_custom_attr s) (dest_custom_attrs cas)
and refs_of_varargs s tyso = iter_option (refs_of_typs s) tyso 
and refs_of_instr s x = 
  match x with
  | I_call (_,mr,varargs) | I_newobj (mr,varargs) | I_callvirt (_,mr,varargs) ->
      refs_of_mspec s mr;
      refs_of_varargs s varargs
  | I_callconstraint (_,tr,mr,varargs) -> 
      refs_of_typ s tr;
      refs_of_mspec s mr;
      refs_of_varargs s varargs
  | I_calli (_,callsig,varargs) ->  
      refs_of_callsig s callsig;  refs_of_varargs s varargs 
  | I_jmp mr | I_ldftn mr | I_ldvirtftn mr -> 
      refs_of_mspec s mr
  | I_ldsfld (_,fr) | I_ldfld (_,_,fr) | I_ldsflda fr | I_ldflda fr | I_stsfld (_,fr) | I_stfld (_,_,fr) -> 
      refs_of_fspec s fr
  | I_isinst ty | I_castclass ty | I_cpobj ty | I_initobj ty | I_ldobj (_,_,ty) 
  | I_stobj (_,_,ty) | I_box ty |I_unbox ty | I_unbox_any ty | I_sizeof ty
  | I_ldelem_any (_,ty) | I_ldelema (_,_,ty) |I_stelem_any (_,ty) | I_newarr (_,ty)
  | I_mkrefany ty | I_refanyval ty 
  | EI_ilzero ty ->   refs_of_typ s ty 
  | I_ldtoken token -> refs_of_token s token 
  | I_stelem _|I_ldelem _|I_ldstr _|I_switch _|I_stloc _|I_stind _
  | I_starg _|I_ldloca _|I_ldloc _|I_ldind _
  | I_ldarga _|I_ldarg _|I_leave _|I_br _
  | I_brcmp _|I_rethrow|I_refanytype|I_ldlen|I_throw|I_initblk _ |I_cpblk _ 
  | I_localloc|I_ret |I_endfilter|I_endfinally|I_arglist
  | I_other _ | I_break|I_arith _ |I_seqpoint _ | EI_ldlen_multi _ ->  ()
      
  
and refs_of_il_block s c  = 
    match c with 
    | ILBasicBlock bb -> Array.iter (refs_of_instr s) bb.bblockInstrs 
    | GroupBlock (_,l) -> List.iter (refs_of_il_code s) l 
    | RestrictBlock (nms,c) -> refs_of_il_code s c 
    | TryBlock (l,r) -> 
       refs_of_il_code s l;
       match r with 
       | FaultBlock flt -> refs_of_il_code s flt 
       | FinallyBlock flt -> refs_of_il_code s flt 
       | FilterCatchBlock clauses -> 
           List.iter 
             (fun (flt,ctch)  -> 
               refs_of_il_code s ctch;
               begin match flt with 
               | CodeFilter fltcode -> refs_of_il_code s fltcode 
               |  TypeFilter ty -> refs_of_typ s ty 
               end)
             clauses

and refs_of_il_code s c  = refs_of_il_block s c 
    
and refs_of_ilmbody s il = 
  List.iter (refs_of_local s) il.ilLocals;
  refs_of_il_code s il.ilCode 
    
and refs_of_local s loc = refs_of_typ s loc.localType
    
and refs_of_mbody s x = 
  match x with 
  | MethodBody_il il -> refs_of_ilmbody s il
  | MethodBody_pinvoke (attr) -> refs_of_modref s attr.pinvokeWhere
  | _ -> ()

and refs_of_mdef s md = 
  List.iter (refs_of_param s) md.mdParams;
  refs_of_return s md.mdReturn;
  refs_of_mbody s  (dest_mbody md.mdBody);
  refs_of_custom_attrs s  md.mdCustomAttrs;
  refs_of_genparams s  md.mdGenericParams
    
and refs_of_param s p = refs_of_typ s p.paramType 
and refs_of_return s (rt:ILReturnValue) = refs_of_typ s rt.Type
and refs_of_mdefs s x =  List.iter (refs_of_mdef s) (dest_mdefs x)
    
and refs_of_event_def s ed = 
  iter_option (refs_of_typ s)  ed.eventType ;
  refs_of_mref  s ed.eventAddOn ;
  refs_of_mref  s ed.eventRemoveOn;
  iter_option (refs_of_mref s) ed.eventFire ;
  List.iter (refs_of_mref s)  ed.eventOther ;
  refs_of_custom_attrs  s ed.eventCustomAttrs
    
and refs_of_events s x =  List.iter (refs_of_event_def s) (dest_edefs x)
    
and refs_of_property_def s pd = 
  iter_option (refs_of_mref s)  pd.propSet ;
  iter_option (refs_of_mref s)  pd.propGet ;
  refs_of_typ  s pd.propType ;
  refs_of_typs  s pd.propArgs ;
  refs_of_custom_attrs  s pd.propCustomAttrs
    
and refs_of_properties s x = List.iter (refs_of_property_def s) (dest_pdefs x)
    
and refs_of_fdef s fd = 
  refs_of_typ  s fd.fdType;
  refs_of_custom_attrs  s fd.fdCustomAttrs

and refs_of_fields s fields = List.iter (refs_of_fdef s) fields
    
and refs_of_method_impls s mimpls =  List.iter (refs_of_method_impl s) mimpls
    
and refs_of_method_impl s m = 
  refs_of_ospec s m.mimplOverrides;
  refs_of_mspec s m.mimplOverrideBy
and refs_of_tdef_kind s k =  ()
  
and refs_of_tdef s td  =  
  refs_of_types s td.tdNested;
  refs_of_genparams s  td.tdGenericParams;
  refs_of_typs  s td.tdImplements;
  iter_option (refs_of_typ s) td.tdExtends;
  refs_of_mdefs        s td.tdMethodDefs;
  refs_of_fields       s (dest_fdefs td.tdFieldDefs);
  refs_of_method_impls s (dest_mimpls td.tdMethodImpls);
  refs_of_events       s td.tdEvents;
  refs_of_tdef_kind    s td.tdKind;
  refs_of_custom_attrs s td.tdCustomAttrs;
  refs_of_properties   s td.tdProperties

and refs_of_string s _ = ()
and refs_of_types s types = List.iter  (refs_of_tdef s) (dest_tdefs types) 
    
and refs_of_exported_type s c = 
  refs_of_custom_attrs s c.exportedTypeCustomAttrs
    
and refs_of_exported_types s tab = List.iter (refs_of_exported_type s) (dest_exported_types tab)
    
and refs_of_resource_where s x = 
  match x with 
  | Resource_local _ -> ()
  | Resource_file (mref,_) -> refs_of_modref s mref
  | Resource_assembly aref -> refs_of_assref s aref
and refs_of_resource s x = 
  refs_of_resource_where s x.resourceWhere;
  refs_of_custom_attrs s x.resourceCustomAttrs
    
and refs_of_resources s tab = List.iter (refs_of_resource s) (dest_resources tab)
    
and refs_of_modul s m = 
  refs_of_types s m.modulTypeDefs;
  refs_of_resources s m.modulResources;
  iter_option (refs_of_manifest s) m.modulManifest
    
and refs_of_manifest s m = 
  refs_of_custom_attrs s m.manifestCustomAttrs;
  refs_of_exported_types s m.manifestExportedTypes

let refs_of_module modul = 
  let s = 
    { refsA = Hashset.create 10; 
      refsM = Hashset.create 5; 
      (* mspecsVisited = Visitset.create "mspecs" 1000 *) }
  refs_of_modul s modul;
  (* Visitset.report s.mspecsVisited; *)
  { refsAssembly = Hashset.fold (fun x acc -> x::acc) s.refsA [];
    refsModul =  Hashset.fold (fun x acc -> x::acc) s.refsM [] }

let tspan = System.TimeSpan(System.DateTime.Now.Ticks - System.DateTime(2000,1,1).Ticks)

let parse_version (vstr : string) = 
    // matches "v1.2.3.4" or "1.2.3.4". Note, if numbers are missing, returns -1 (not 0).
    let mutable vstr = vstr.TrimStart [|'v'|] 
    // if the version string contains wildcards, replace them
    let versionComponents = vstr.Split([|'.'|])
    
    // account for wildcards
    if versionComponents.Length > 2 then
      let defaultBuild = (uint16)tspan.Days % System.UInt16.MaxValue - 1us
      let defaultRevision = (uint16)(System.DateTime.Now.TimeOfDay.TotalSeconds / 2.0) % System.UInt16.MaxValue - 1us
      if versionComponents.[2] = "*" then
        if versionComponents.Length > 3 then
          failwith "Invalid version format"
        else
          // set the build number to the number of days since Jan 1, 2000
          versionComponents.[2] <- defaultBuild.ToString() ;
          // Set the revision number to number of seconds today / 2
          vstr <- System.String.Join(".",versionComponents) ^ "." ^ defaultRevision.ToString() ;
      elif versionComponents.Length > 3 && versionComponents.[3] = "*" then
        // Set the revision number to number of seconds today / 2
        versionComponents.[3] <- defaultRevision.ToString() ;
        vstr <- System.String.Join(".",versionComponents) ;
        
    let version = System.Version(vstr)
    let zero16 n = if n < 0s then 0us else uint16(n)
    let zero32 n = if n < 0 then 0us else uint16(n)
    (zero32 version.Major, zero32 version.Minor, zero32 version.Build, zero16 version.MinorRevision);;


let version_compare (a1,a2,a3,a4) ((b1,b2,b3,b4) : ILVersionInfo) = 
    let c = compare a1 b1
    if c <> 0 then c else
    let c = compare a2 b2
    if c <> 0 then c else
    let c = compare a3 b3
    if c <> 0 then c else
    let c = compare a4 b4
    if c <> 0 then c else
    0


let version_max a b = if version_compare a b < 0 then b else a
let version_min a b = if version_compare a b > 0 then b else a


let resolve_mref td (mref:ILMethodRef) = 
  let args = mref.ArgTypes
  let nargs = List.length args
  let nm = mref.Name
  let mid =(nm,nargs)
  let possibles = find_mdefs_by_arity mid td.tdMethodDefs
  if isNil possibles then failwith ("no method named "^nm^" found in type "^td.tdName);
  match 
    possibles |> List.filter (fun md -> 
        callconv_eq mref.CallingConv md.mdCallconv &
        List.lengthsEqAndForall2 (fun p1 p2 -> p1.paramType = p2) md.mdParams mref.ArgTypes)  with 
  | [] -> 
      failwith ("no method named "^nm^" with appropriate argument types found in type "^td.tdName);
  | [mdef] ->  mdef
  | _ -> 
      failwith ("multiple methods named "^nm^" appear with identical argument types in type "^td.tdName)
        
let modref_for_modul m =
  ILModuleRef.Create(m.modulName, true, None)


let ungenericize_tname n = 
  let sym = '`'
  if 
    String.contains n sym && 
      (* check what comes after the symbol is a number *)
    begin
      let m = String.rindex n sym
      let res = ref (m < String.length n - 1)
      for i = m + 1 to String.length n - 1 do
        res := !res && String.get n i >= '0' && String.get n i <= '9';
      done;
      !res
    end
  then 
      let pos = String.rindex n sym
      String.sub n 0 pos
  else n


(* -------------------------------------------------------------------- 
 * Augmentations
 * -------------------------------------------------------------------- *)

type ILTypeSpec with
    member x.FullName=x.TypeRef.FullName

type ILEventRef = 
    { erA: ILTypeRef; erB: string }
    static member Create(a,b) = {erA=a;erB=b}
    member x.EnclosingTypeRef = x.erA
    member x.Name = x.erB

type ILEventSpec = 
    { esA: ILEventRef; esB: ILType }
     static member Create (a,b) = {esA=a;esB=b}
     member x.EventRef = x.esA
     member x.EnclosingType = x.esB

type ILPropertyRef = 
    { prA: ILTypeRef; prB: string }
    static member Create (a,b) = {prA=a;prB=b}
    member x.EnclosingTypeRef = x.prA
    member x.Name = x.prB

type ILPropertySpec = 
    { psA: ILPropertyRef; psB: ILType }
    static member Create (a,b) = {psA=a;psB=b}
    member x.PropertyRef = x.psA
    member x.EnclosingType = x.psB


let tref_of_pref x = x.prA
let tref_of_eref x = x.erA
let name_of_pref x = x.prB
let name_of_eref x = x.erB
let mk_pref (a,b) = {prA=a;prB=b}
let mk_eref (a,b) = {erA=a;erB=b}
let mk_pspec (a,b) = {psA=a;psB=b}
let mk_espec (a,b) = {esA=a;esB=b}
let enclosing_typ_of_pspec x = x.psB
let enclosing_typ_of_espec x = x.esB
let pref_of_pspec x = x.psA
let eref_of_espec x = x.esA
let eref_for_edef scope (tdefs,tdef) (x:ILEventDef) = mk_eref (tref_for_nested_tdef scope (tdefs,tdef), x.eventName)
let pref_for_pdef scope (tdefs,tdef) (x:ILPropertyDef) = mk_pref (tref_for_nested_tdef scope (tdefs,tdef), x.propName)

type ILArrayShape with 
    static member SingleDimensional = Rank1ArrayShape    

open System.Runtime.CompilerServices
[<Dependency("FSharp.Core",LoadHint.Always)>] do ()



#endif
