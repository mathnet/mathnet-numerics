// (c) Microsoft Corporation. All rights reserved

#light

//-------------------------------------------------------------------------
// Expression and Type Specifications.  These are what we save
//------------------------------------------------------------------------- 


module (* internal *) Microsoft.FSharp.Compiler.Sreflect

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.Lib

module Bytebuf = Bytes.Bytebuf
module Bytestream = Bytes.Bytestream

let mkRLinear mk (vs,body) = List.foldBack (fun v acc -> mk (v,acc)) vs body 

type TypeVarData = { tvName: string; }
type NamedTypeData = { tcName: string; tcAssembly:  string }

type TypeCombOp = 
    | ArrayTyOp  of int (* rank *) 
    | FunTyOp
    | NamedTyOp of NamedTypeData

type TypeData =
    | VarType   of int
    | AppType   of TypeCombOp * TypeData list
  

let mkVarTy v = VarType v 
let mkFunTy (x1,x2) = AppType(FunTyOp, [x1; x2])  
let mkArrayTy (n,x) = AppType(ArrayTyOp n, [x]) 
let mkNamedTy (r,l) = AppType(NamedTyOp r,l) 


type CtorData = 
    { ctorParent: NamedTypeData; 
      ctorArgTypes: TypeData list; }

type MethodData = 
    { methParent: NamedTypeData;
      methName: string;
      methArgTypes: TypeData list;
      methRetType: TypeData; 
      numGenericArgs: int }
  
type VarData = 
    { vText: string; 
      vType: TypeData } 
type FieldData = NamedTypeData * string
type RecdFieldData = NamedTypeData * string
type PropInfoData = NamedTypeData * string * TypeData * TypeData list

type CombOp = 
    | AppOp
    | CondOp  
    | ModuleValueOp of NamedTypeData * string * bool
    | LetRecOp  
    | LetRecCombOp  
    | LetOp  
    | RecdMkOp  of NamedTypeData
    | RecdGetOp  of NamedTypeData * string
    | RecdSetOp  of NamedTypeData * string
    | SumMkOp  of NamedTypeData * string
    | SumFieldGetOp  of NamedTypeData * string * int
    | SumTagTestOp of NamedTypeData * string
    | TupleMkOp  
    | TupleGetOp of int
    | UnitOp   
    | BoolOp   of bool   
    | StringOp of string 
    | SingleOp of float32 
    | DoubleOp of float  
    | CharOp   of char   
    | SByteOp  of sbyte
    | ByteOp   of byte   
    | Int16Op  of int16  
    | UInt16Op of uint16 
    | Int32Op  of int32  
    | UInt32Op of uint32 
    | Int64Op  of int64  
    | UInt64Op of uint64 
    | PropGetOp of PropInfoData
    | FieldGetOp of NamedTypeData * string
    | CtorCallOp of CtorData 
    | MethodCallOp of MethodData 
    | CoerceOp 
    | NewArrayOp
    | DelegateOp
    | SeqOp 
    | ForLoopOp 
    | WhileLoopOp 
    | NullOp   
    | DefaultValueOp 
    | PropSetOp of PropInfoData
    | FieldSetOp  of NamedTypeData * string
    | AddressOfOp
    | AddressSetOp
    | TypeTestOp 
    | TryFinallyOp
    | TryWithOp 


/// Represents specifications of a subset of F# expressions 
type ExprData =
    | AttrExpr   of ExprData * ExprData list
    | CombExpr   of CombOp * TypeData list * ExprData list
    | VarExpr    of int
    | QuoteExpr  of ExprData 
    | LambdaExpr of VarData * ExprData
    | HoleExpr   of TypeData * int
  
let mkVar v = VarExpr v 
let mkHole (v,idx) = HoleExpr (v ,idx)
let mkApp (a,b) = CombExpr(AppOp, [], [a; b]) (* REVIEW: type arguments? *)
let mkLambda (a,b) = LambdaExpr (a,b) 
let mkQuote (a) = QuoteExpr (a) 

let mkCond (x1,x2,x3)          = CombExpr(CondOp,[], [x1;x2;x3])  
let mkModuleValueApp (tcref,nm,isProp,tyargs,args: ExprData list list) = CombExpr(ModuleValueOp(tcref,nm,isProp),tyargs,List.concat args)
let mkTuple (ty,x)             = CombExpr(TupleMkOp,[ty],x)
let mkLet ((v,e),b)            = CombExpr(LetOp,[],[e;mkLambda (v,b)])  (* nb. order preserves source order *)
let mkUnit ()                  = CombExpr(UnitOp, [], []) 
let mkNull ty                  = CombExpr(NullOp, [ty], []) 

let mkLetRecRaw e1 = CombExpr(LetRecOp,[],[e1])
let mkLetRecCombRaw args = CombExpr(LetRecCombOp,[], args) 
let mkLetRec (ves,body) = 
     let vs,es = List.unzip ves in 
     mkLetRecRaw(mkRLinear mkLambda  (vs, mkLetRecCombRaw (body::es)))
      
let mkRecdMk      (n,tys,args)            = CombExpr(RecdMkOp n,tys,args)  
let mkRecdGet     ((d1,d2),tyargs,args)   = CombExpr(RecdGetOp(d1,d2),tyargs,args)
let mkRecdSet     ((d1,d2),tyargs,args)   = CombExpr(RecdSetOp(d1,d2),tyargs,args)
let mkSum         ((d1,d2),tyargs,args)   = CombExpr(SumMkOp(d1,d2),tyargs,args)
let mkSumFieldGet ((d1,d2,d3),tyargs,arg) = CombExpr(SumFieldGetOp(d1,d2,d3),tyargs,[arg])
let mkSumTagTest  ((d1,d2),tyargs,arg)    = CombExpr(SumTagTestOp(d1,d2),tyargs,[arg])
let mkTupleGet    (ty,n,e)                = CombExpr(TupleGetOp n,[ty],[e]) 

let mkCoerce  (ty,arg)        = CombExpr(CoerceOp,[ty],[arg])
let mkTypeTest  (ty,arg)      = CombExpr(TypeTestOp,[ty],[arg])
let mkAddressOf  (arg)        = CombExpr(AddressOfOp,[],[arg])
let mkAddressSet  (arg1,arg2) = CombExpr(AddressSetOp,[],[arg1;arg2])
let mkDefaultValue (ty)       = CombExpr(DefaultValueOp,[ty],[])
let mkNewArray     (ty,args)  = CombExpr(NewArrayOp,[ty],args)

let mkBool   v = CombExpr(BoolOp   v,[],[]) 
let mkString v = CombExpr(StringOp v,[],[]) 
let mkSingle v = CombExpr(SingleOp v,[],[]) 
let mkDouble v = CombExpr(DoubleOp v,[],[]) 
let mkChar   v = CombExpr(CharOp   v,[],[]) 
let mkSByte  v = CombExpr(SByteOp  v,[],[]) 
let mkByte   v = CombExpr(ByteOp   v,[],[]) 
let mkInt16  v = CombExpr(Int16Op  v,[],[]) 
let mkUInt16 v = CombExpr(UInt16Op v,[],[]) 
let mkInt32  v = CombExpr(Int32Op  v,[],[]) 
let mkUInt32 v = CombExpr(UInt32Op v,[],[]) 
let mkInt64  v = CombExpr(Int64Op  v,[],[]) 
let mkUInt64 v = CombExpr(UInt64Op v,[],[]) 

let mkSequential (e1,e2)       = CombExpr(SeqOp,[],[e1;e2])
let mkForLoop (x1,x2,x3)       = CombExpr(ForLoopOp,[], [x1;x2;x3])  
let mkWhileLoop (e1,e2)        = CombExpr(WhileLoopOp,[],[e1;e2])
let mkTryFinally(e1,e2)        = CombExpr(TryFinallyOp,[],[e1;e2])
let mkTryWith(e1,vf,ef,vh,eh)  = CombExpr(TryWithOp,[],[e1;mkLambda(vf,ef);mkLambda(vh,eh)])
let mkDelegate (ty,e)          = CombExpr(DelegateOp,[ty],[e])
let mkPropGet  (d,tyargs,args) = CombExpr(PropGetOp(d),tyargs,args)
let mkPropSet  (d,tyargs,args) = CombExpr(PropSetOp(d),tyargs,args)
let mkFieldGet ((d1,d2),tyargs,args) = CombExpr(FieldGetOp(d1,d2),tyargs,args)
let mkFieldSet ((d1,d2),tyargs,args) = CombExpr(FieldSetOp(d1,d2),tyargs,args)
let mkCtorCall   (d,tyargs,args) = CombExpr(CtorCallOp(d),tyargs,args)
let mkMethodCall (d,tyargs,args) = CombExpr(MethodCallOp(d),tyargs,args)
let mkAttributedExpression(e,attr) = AttrExpr(e,[attr])

//---------------------------------------------------------------------------
// Pickle/unpickle expression and type specifications in a stable format
// compatible with those read by Microsoft.FSharp.Quotations
//--------------------------------------------------------------------------- 

let pickledDefinitionsResourceNameBase = "ReflectedDefinitions"

let freshVar (n,ty) = {vText=n; vType=ty}

module SimplePickle = begin

    type 'a tbl = 
        { tbl: Hashtbl.t<'a,int>;
          mutable rows: 'a list;
          mutable count: int }

    let new_tbl () = 
      { tbl = (Hashtbl.create 20);
        rows=[];
        count=0; }

    let get_tbl tbl = List.rev tbl.rows
    let tbl_size tbl = List.length tbl.rows

    let add_entry tbl x =
        let n = tbl.count in 
        tbl.count <- tbl.count + 1;
        Hashtbl.add tbl.tbl x n;
        tbl.rows <- x :: tbl.rows;
        n

    let find_or_add_entry tbl x =
        if Hashtbl.mem tbl.tbl x then Hashtbl.find tbl.tbl x 
        else add_entry tbl x


    let tbl_find tbl x = Hashtbl.find tbl.tbl x 

    let tbl_mem tbl x = Hashtbl.mem tbl.tbl x 

    type outstate = 
        { os: Bytebuf.t; 
          ostrings: string tbl }

    let p_byte b st = Bytebuf.emit_int_as_byte st.os b
    let p_bool b st = p_byte (if b then 1 else 0) st
    let p_void (os: outstate) = ()
    let p_unit () (os: outstate) = ()
    let prim_pint32 i st = 
        p_byte (Bits.b0 i) st;
        p_byte (Bits.b1 i) st;
        p_byte (Bits.b2 i) st;
        p_byte (Bits.b3 i) st

    // compress integers according to the same scheme used by CLR metadata 
    // This halves the size of pickled data 
    let p_int32 n st = 
        if n >= 0 &&  n <= 0x7F then 
            p_byte (Bits.b0 n) st
        else if n >= 0x80 && n <= 0x3FFF then  
            p_byte  (0x80 ||| (n >>> 8)) st; 
            p_byte (n &&& 0xFF) st 
        else 
            p_byte 0xFF st;
            prim_pint32 n st

    let p_bytes s st = 
        let len = Bytes.length s 
        p_int32 (len) st;
        Bytebuf.emit_bytes st.os s

    let prim_pstring s st = 
        let bytes = Bytes.string_as_utf8_bytes s 
        let len = Bytes.length bytes 
        p_int32 (len) st;
        Bytebuf.emit_bytes st.os bytes

    let p_int (c:int) st = p_int32 c st
    let p_int8 (i:int8) st = p_int32 (int32 i) st
    let p_uint8 (i:uint8) st = p_byte (int i) st
    let p_int16 (i:int16) st = p_int32 (int32 i) st
    let p_uint16 (x:uint16) st = p_int32 (int32 x) st
    let puint32 (x:uint32) st = p_int32 (int32 x) st
    let p_int64 i st = 
        p_int32 (int32 (i &&& 0xFFFFFFFFL)) st;
        p_int32 (int32 (i >>> 32)) st

    let bits_of_float32 (x:float32) = System.BitConverter.ToInt32(System.BitConverter.GetBytes(x),0)
    let bits_of_float (x:float) = System.BitConverter.ToInt64(System.BitConverter.GetBytes(x),0)

    let p_uint64 x st = p_int64 (int64 x) st
    let p_double i st = p_int64 (bits_of_float i) st
    let p_single i st = p_int32 (bits_of_float32 i) st
    let p_char i st = p_uint16 (uint16 (int32 i)) st
    let inline p_tup2 p1 p2 (a,b) (st:outstate) = (p1 a st : unit); (p2 b st : unit)
    let inline p_tup3 p1 p2 p3 (a,b,c) st = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit)
    let inline p_tup4 p1 p2 p3 p4 (a,b,c,d) st = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit)
    let inline p_tup5 p1 p2 p3 p4 p5 (a,b,c,d,e) st = (p1 a st : unit); (p2 b st : unit); (p3 c st : unit); (p4 d st : unit); (p5 e st : unit)
    let puniq tbl key st = p_int (find_or_add_entry tbl key) st
    let p_string s st = puniq st.ostrings s st
    let rec p_list f x st =
        match x with 
        | [] -> p_byte 0 st
        | h :: t -> p_byte 1 st; f h st; p_list f t st
          

    let pickle_obj p x =
        let stringTab,phase1bytes =
            let st1 = 
                { os = Bytebuf.create 100000; 
                  ostrings=new_tbl(); } in
            p x st1;
            get_tbl st1.ostrings, Bytebuf.close st1.os in
        let phase2data = (stringTab,phase1bytes) in 
        let phase2bytes = 
            let st2 = 
               { os = Bytebuf.create 100000; 
                  ostrings=new_tbl();} in
            p_tup2 (p_list prim_pstring) p_bytes phase2data st2;
            Bytebuf.close st2.os in 
        phase2bytes
end

open SimplePickle


let p_assref x st = p_string x st

let p_NamedType x st = p_tup2 p_string p_assref (x.tcName, x.tcAssembly) st

let p_tycon x st = 
    match x with
    | FunTyOp     -> p_byte 1 st
    | NamedTyOp a -> p_byte 2 st; p_NamedType a st
    | ArrayTyOp a -> p_byte 3 st; p_int a st

let rec p_type x st =
    match x with 
    | VarType v     -> p_byte 0 st; p_int v st
    | AppType(c,ts) -> p_byte 1 st; p_tup2 p_tycon p_types (c,ts) st

and p_types x st = p_list p_type x st

let p_varDecl v st   = p_tup2 p_string p_type (v.vText,v.vType) st

let p_recdFieldSpec v st = p_tup2 p_NamedType p_string v st

let p_ucaseSpec v st   = p_tup2 p_NamedType p_string v st

let p_MethodData a st = 
    p_tup5 p_NamedType p_types p_type p_string p_int (a.methParent,a.methArgTypes,a.methRetType, a.methName, a.numGenericArgs) st

let p_CtorData a st = 
    p_tup2 p_NamedType p_types (a.ctorParent,a.ctorArgTypes) st

let p_PropInfoData a st = 
    p_tup4 p_NamedType p_string p_type p_types a st
    
let p_CombOp x st = 
    match x with 
    | CondOp        -> p_byte 0 st
    | ModuleValueOp (x,y,z) -> p_byte 1 st; p_tup3 p_NamedType p_string p_bool (x,y,z) st
    | LetRecOp      -> p_byte 2 st
    | RecdMkOp  a   -> p_byte 3 st; p_NamedType a st
    | RecdGetOp  (x,y)  -> p_byte 4 st; p_recdFieldSpec (x,y) st
    | SumMkOp  (x,y)  -> p_byte 5 st; p_ucaseSpec (x,y) st
    | SumFieldGetOp (a,b,c) -> p_byte 6 st; p_tup2 p_ucaseSpec p_int ((a,b),c) st
    | SumTagTestOp (x,y) -> p_byte 7 st; p_ucaseSpec (x,y) st
    | TupleMkOp     -> p_byte 8 st
    | TupleGetOp a  -> p_byte 9 st; p_int a st
    | BoolOp   a    -> p_byte 11 st; p_bool a st
    | StringOp a    -> p_byte 12 st; p_string a st
    | SingleOp a    -> p_byte 13 st; p_single a st
    | DoubleOp a    -> p_byte 14 st; p_double a st
    | CharOp   a    -> p_byte 15 st; p_char a st
    | SByteOp  a    -> p_byte 16 st; p_int8 a st
    | ByteOp   a    -> p_byte 17 st; p_uint8 a st
    | Int16Op  a    -> p_byte 18 st; p_int16 a st
    | UInt16Op a    -> p_byte 19 st; p_uint16 a st
    | Int32Op  a    -> p_byte 20 st; p_int32 a st
    | UInt32Op a    -> p_byte 21 st; puint32 a st
    | Int64Op  a    -> p_byte 22 st; p_int64 a st
    | UInt64Op a    -> p_byte 23 st; p_uint64 a st
    | UnitOp        -> p_byte 24 st
    | PropGetOp d   -> p_byte 25 st; p_PropInfoData d st
    | CtorCallOp a  -> p_byte 26 st; p_CtorData a st
    | CoerceOp      -> p_byte 28 st
    | SeqOp         -> p_byte 29 st
    | ForLoopOp -> p_byte 30 st
    | MethodCallOp a   -> p_byte 31 st; p_MethodData a st
    | NewArrayOp        -> p_byte 32 st
    | DelegateOp       -> p_byte 33 st
    | WhileLoopOp -> p_byte 34 st
    | LetOp            -> p_byte 35 st
    | RecdSetOp  (x,y) -> p_byte 36 st; p_recdFieldSpec (x,y) st
    | FieldGetOp (a,b) -> p_byte 37 st; p_tup2 p_NamedType p_string (a, b) st
    | LetRecCombOp     -> p_byte 38 st
    | AppOp            -> p_byte 39 st
    | NullOp           -> p_byte 40 st
    | DefaultValueOp -> p_byte 41 st
    | PropSetOp d   -> p_byte 42 st; p_PropInfoData d st
    | FieldSetOp (a,b) -> p_byte 43 st; p_tup2 p_NamedType p_string (a, b) st
    | AddressOfOp      -> p_byte 44 st
    | AddressSetOp     -> p_byte 45 st
    | TypeTestOp       -> p_byte 46 st
    | TryFinallyOp     -> p_byte 47 st
    | TryWithOp        -> p_byte 48 st

let rec p_expr x st =
    match x with 
    | CombExpr(c,ts,args) -> p_byte 0 st; p_tup3 p_CombOp p_types (p_list p_expr) (c,ts,args) st
    | VarExpr v           -> p_byte 1 st; p_int v st
    | LambdaExpr(v,e)     -> p_byte 2 st; p_tup2 p_varDecl p_expr (v,e) st
    | HoleExpr(ty,idx)    -> p_byte 3 st; p_type ty st; p_int idx st
    | QuoteExpr(tm)       -> p_byte 4 st; p_expr tm st
    | AttrExpr(e,attrs)    -> p_byte 5 st; p_tup2 p_expr (p_list p_expr) (e,attrs) st
  
type ModuleDefnData = 
    { Module: NamedTypeData;
      Name: string;
      IsProperty: bool }

type MethodBaseData = 
    | ModuleDefn of ModuleDefnData
    | Method     of MethodData
    | Ctor       of CtorData

let pickle = pickle_obj p_expr

let p_MethodBase x st = 
    match x with 
    | ModuleDefn md -> 
        p_byte 0 st;
        p_NamedType md.Module st;
        p_string md.Name st;
        p_bool md.IsProperty st
    | Method md -> 
        p_byte 1 st;
        p_MethodData md st
    | Ctor md -> 
        p_byte 2 st;
        p_CtorData md st

let pdmkns defs = p_list (p_tup2 p_MethodBase p_expr) defs
let PickleDefns   = pickle_obj pdmkns


