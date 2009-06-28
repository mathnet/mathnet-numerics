//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Quotations

#if FX_MINIMAL_REFLECTION
#else
open System
open System.IO
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Core.Operators
open Microsoft.FSharp.Primitives.Basics
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Text
open Microsoft.FSharp.Text.Printf
open Microsoft.FSharp.Text.StructuredPrintfImpl
open Microsoft.FSharp.Text.StructuredPrintfImpl.LayoutOps

//--------------------------------------------------------------------------
// RAW quotations - basic data types
//--------------------------------------------------------------------------

module Helpers = 
    let qOneOrMoreRLinear q inp =
        let rec queryAcc rvs e = 
            match q e with 
            | Some(v,body) -> queryAcc (v::rvs) body 
            | None -> 
                match rvs with 
                | [] -> None
                | _ -> Some(List.rev rvs,e) 
        queryAcc [] inp 

    let qOneOrMoreLLinear q inp =
        let rec queryAcc e rvs = 
            match q e with 
            | Some(body,v) -> queryAcc body (v::rvs) 
            | None -> 
                match rvs with 
                | [] -> None
                | _ -> Some(e,rvs) 
        queryAcc inp []

    let mkRLinear mk (vs,body) = List.foldBack (fun v acc -> mk(v,acc)) vs body 
    let mkLLinear mk (body,vs) = List.fold (fun acc v -> mk(acc,v)) body vs 

    let staticBindingFlags = BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.DeclaredOnly
    let staticOrInstanceBindingFlags = BindingFlags.Instance ||| BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.DeclaredOnly
    let instanceBindingFlags = BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.DeclaredOnly
    let publicOrPrivateBindingFlags = System.Reflection.BindingFlags.Public ||| System.Reflection.BindingFlags.NonPublic

    let isDelegateType (typ:Type) = 
        if typ.IsSubclassOf(typeof<Delegate>) then
            match typ.GetMethod("Invoke", instanceBindingFlags) with
            | null -> false
            | _ -> true
        else
            false

    let getDelegateInvoke ty = 
        if not (isDelegateType(ty)) then invalidArg  "ty" "Expecting delegate type"
        ty.GetMethod("Invoke", instanceBindingFlags)
        
open Helpers



[<Sealed>]
[<System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage","CA2218:OverrideGetHashCodeOnOverridingEquals",Justification="Equals override does not equate further objects, so default GetHashCode is still valid")>]
type Var(name: string, typ:Type, ?isMutable: bool) =

    inherit obj()
    static let mutable lastStamp = 0L
    static let globals = new Dictionary<(string*Type),Var>(11)

    let stamp = lastStamp    
    let isMutable = defaultArg isMutable false
    do lock globals (fun () -> lastStamp <- lastStamp + 1L)
    
    member v.Name = name
    member v.IsMutable = isMutable
    member v.Type = typ
    member v.Stamp = stamp
    
    static member Global(nm,typ: Type) = 
        lock globals (fun () -> 
            let mutable res = Unchecked.defaultof<Var>
            let ok = globals.TryGetValue((nm,typ),&res)
            if ok then res else
            let res = new Var(nm,typ)
            globals.[((nm,typ))] <- res
            res)

    override v.ToString() = name
    override v.GetHashCode() = base.GetHashCode()
    override v.Equals(obj:obj) = 
        match obj with 
        | :? Var as v2 -> System.Object.ReferenceEquals(v,v2)
        | _ -> false

    interface System.IComparable with 
        member v.CompareTo(obj:obj) = 
            match obj with 
            | :? Var as v2 -> 
                if System.Object.ReferenceEquals(v,v2) then 0 else
                let c = compare v.Name v2.Name in 
                if c <> 0 then c else 
#if FX_NO_REFLECTION_METADATA_TOKENS // not available on Compact Framework
#else
                let c = compare v.Type.MetadataToken v2.Type.MetadataToken in 
                if c <> 0 then c else 
                let c = compare v.Type.Module.MetadataToken v2.Type.Module.MetadataToken in 
                if c <> 0 then c else 
#endif
                let c = compare v.Type.Assembly.FullName v2.Type.Assembly.FullName in 
                if c <> 0 then c else 
                compare v.Stamp v2.Stamp
            | _ -> 0

/// Represents specifications of a subset of F# expressions 
[<StructuralEquality(true); StructuralComparison(false)>]
type Tree =
    | CombTerm   of ExprConstInfo * Expr list
    | VarTerm    of Var
    | LambdaTerm of Var * Expr 
    | HoleTerm   of Type * int

and 
  [<StructuralEquality(true); StructuralComparison(false)>]
  ExprConstInfo = 
    | AppOp
    | IfThenElseOp  
    | LetRecOp  
    | LetRecCombOp  
    | LetOp  
    | NewRecordOp      of Type
    | NewUnionCaseOp       of UnionCaseInfo
    | UnionCaseTestOp  of UnionCaseInfo
    | NewTupleOp     of Type
    | TupleGetOp    of Type * int
    | InstancePropGetOp    of PropertyInfo
    | StaticPropGetOp    of PropertyInfo
    | InstancePropSetOp    of PropertyInfo
    | StaticPropSetOp    of PropertyInfo
    | InstanceFieldGetOp   of FieldInfo
    | StaticFieldGetOp   of FieldInfo
    | InstanceFieldSetOp   of FieldInfo
    | StaticFieldSetOp   of FieldInfo
    | NewObjectOp   of ConstructorInfo 
    | InstanceMethodCallOp of MethodInfo 
    | StaticMethodCallOp of MethodInfo 
    | CoerceOp     of Type
    | NewArrayOp    of Type
    | NewDelegateOp   of Type
    | QuoteOp 
    | SequentialOp 
    | AddressOfOp 
    | VarSetOp
    | AddressSetOp 
    | TypeTestOp  of Type
    | TryWithOp 
    | TryFinallyOp 
    | ForIntegerRangeLoopOp 
    | WhileLoopOp 
    // Arbitrary spliced values - not serialized
    | ValueOp of obj * Type
    | DefaultValueOp of Type
    
and Expr(term:Tree,attribs:Expr list) =
    member x.Tree = term
    member x.CustomAttributes = attribs 
    override x.Equals(obj:obj) =
        match obj with 
        | :? Expr as yt -> x.Tree = yt.Tree
        | _ -> false

    override x.GetHashCode() = 
        x.Tree.GetHashCode() 

    override x.ToString() = 
        Microsoft.FSharp.Text.StructuredPrintfImpl.Display.layout_to_string Microsoft.FSharp.Text.StructuredPrintfImpl.FormatOptions.Default (x.GetLayout())
        
    member x.GetLayout() = 
        let expr (e:Expr ) = e.GetLayout()
        let exprs (es:Expr list) = es |> List.map expr
        let parens ls = bracketL (commaListL ls)
        let pairL l1 l2 = bracketL (l1 $$ sepL "," $$ l2)
        let listL ls = squareBracketL (commaListL ls)
        let combL nm ls = wordL nm $$ parens ls
        let noneL = wordL "None"
        let someL e = combL "Some" [expr e]
        let typeL (o: Type)  = wordL o.FullName
        let objL (o: 'T)  = wordL (sprintf "%A" o)
        let varL (v:Var) = wordL v.Name
        let (|E|) (e: Expr) = e.Tree
        let (|Lambda|_|)        (E x) = match x with LambdaTerm(a,b)  -> Some (a,b) | _ -> None 
        let (|IteratedLambda|_|) (e: Expr) = qOneOrMoreRLinear (|Lambda|_|) e
        
        let rec (|NLambdas|_|) n (e:Expr) = 
            match e with 
            | _ when n <= 0 -> Some([],e) 
            | Lambda(v,NLambdas ((-) n 1) (vs,b)) -> Some(v::vs,b)
            | _ -> None

        match x.Tree with 
        | CombTerm(AppOp,args)                     -> combL "Application" (exprs args)
        | CombTerm(IfThenElseOp,args)              -> combL "IfThenElse" (exprs args)
        | CombTerm(LetRecOp,[IteratedLambda(vs,E(CombTerm(LetRecCombOp,b2::bs)))]) -> combL "LetRec" [listL (List.map2 pairL (List.map varL vs) (exprs bs) ); b2.GetLayout()]
        | CombTerm(LetOp,[e;E(LambdaTerm(v,b))]) -> combL "Let" [varL v; e.GetLayout(); b.GetLayout()]
        | CombTerm(NewRecordOp(ty),args)           -> combL "NewRecord" (typeL ty :: exprs args)
        | CombTerm(NewUnionCaseOp(ucinfo),args)    -> combL "NewUnionCase" (objL ucinfo :: exprs args)
        | CombTerm(UnionCaseTestOp(ucinfo),args)   -> combL "UnionCaseTest" (exprs args@ [objL ucinfo])
        | CombTerm(NewTupleOp(ty),args)            -> combL "NewTuple" (exprs args)
        | CombTerm(TupleGetOp(ty,i),[arg])         -> combL "TupleGet" ([expr arg] @ [objL i])
        | CombTerm(ValueOp(v,ty),[])               -> combL "Value" [objL v]
        | CombTerm(InstanceMethodCallOp(minfo),obj::args) -> combL "Call"     [someL obj; objL minfo; listL (exprs args)]
        | CombTerm(StaticMethodCallOp(minfo),args)        -> combL "Call"     [noneL;     objL minfo; listL (exprs args)]
        | CombTerm(InstancePropGetOp(pinfo),(obj::args))  -> combL "PropGet"  [someL obj; objL pinfo; listL (exprs args)]
        | CombTerm(StaticPropGetOp(pinfo),args)           -> combL "PropGet"  [noneL;     objL pinfo; listL (exprs args)]
        | CombTerm(InstancePropSetOp(pinfo),(obj::args))  -> combL "PropSet"  [someL obj; objL pinfo; listL (exprs args)]
        | CombTerm(StaticPropSetOp(pinfo),args)           -> combL "PropSet"  [noneL;     objL pinfo; listL (exprs args)]
        | CombTerm(InstanceFieldGetOp(finfo),[obj])       -> combL "FieldGet" [someL obj; objL finfo]
        | CombTerm(StaticFieldGetOp(finfo),[])            -> combL "FieldGet" [noneL;     objL finfo]
        | CombTerm(InstanceFieldSetOp(finfo),[obj;v])       -> combL "FieldSet" [someL obj; objL finfo; expr v;]
        | CombTerm(StaticFieldSetOp(finfo),[v])            -> combL "FieldSet" [noneL;     objL finfo; expr v;]
        | CombTerm(CoerceOp(ty),[arg])                    -> combL "Coerce"  [ expr arg; typeL ty]
        | CombTerm(NewObjectOp minfo,args)   -> combL "NewObject" ([ objL minfo ] @ exprs args)
        | CombTerm(DefaultValueOp(ty),args)  -> combL "DefaultValue" ([ typeL ty ] @ exprs args)
        | CombTerm(NewArrayOp(ty),args)      -> combL "NewArray" ([ typeL ty ] @ exprs args)
        | CombTerm(TypeTestOp(ty),args)      -> combL "TypeTest" ([ typeL ty] @ exprs args)
        | CombTerm(AddressOfOp,args)         -> combL "AddressOf" (exprs args)
        | CombTerm(VarSetOp,[E(VarTerm(v)); e])  -> combL "VarSet" [varL v; expr e]
        | CombTerm(AddressSetOp,args)        -> combL "AddressSet" (exprs args)
        | CombTerm(ForIntegerRangeLoopOp,[e1;e2;E(LambdaTerm(v,e3))])     -> combL "ForIntegerRangeLoop" [varL v; expr e1; expr e2; expr e3]
        | CombTerm(WhileLoopOp,args)         -> combL "WhileLoop" (exprs args)
        | CombTerm(TryFinallyOp,args)         -> combL "TryFinally" (exprs args)
        | CombTerm(TryWithOp,[e1;Lambda(v1,e2);Lambda(v2,e3)])         -> combL "TryWith" [expr e1; varL v1; expr e2; varL v2; expr e3]
        | CombTerm(SequentialOp,args)        -> combL "Sequential" (exprs args)
        | CombTerm(NewDelegateOp(ty),[e])   -> 
            let n = (getDelegateInvoke ty).GetParameters().Length
            match e with 
            | NLambdas n (vs,e) -> combL "NewDelegate" ([typeL ty] @ (vs |> List.map varL) @ [expr e])
            | _ -> combL "NewDelegate" ([typeL ty; expr e])
        //| CombTerm(_,args)   -> combL "??" (exprs args)
        | VarTerm(v)   -> wordL v.Name
        | LambdaTerm(v,b)   -> combL "Lambda" [wordL v.Name; expr b]
        | HoleTerm _  -> wordL "_"
        | CombTerm(QuoteOp,args) -> combL "Quote" (exprs args)
        | _ -> failwithf "Unexpected term in layout %A" x.Tree

     
type Expr<'T>(term:Tree,attribs) = 
    inherit Expr(term,attribs)
    member x.Raw = (x :> Expr)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Patterns = 

    /// Represents a deserialized object that is yet to be instantiated. Representation is
    /// as a computation.
    type Instantiable<'T> = (int -> Type) -> 'T

    type ByteStream(bytes:byte[],initial:int,len:int) = 
    
        let mutable pos = initial
        let lim = initial + len
        
        member b.ReadByte() = 
            if pos >= lim then failwith "end of stream";
            let res = int32 bytes.[pos]
            pos <- pos + 1;
            res 
        
        member b.ReadBytes n  = 
            if pos + n > lim then failwith "ByteStream.ReadBytes: end of stream";
            let res = bytes.[pos..pos+n-1]
            pos <- pos + n;
            res 

        member b.ReadUtf8BytesAsString n = 
            let res = System.Text.Encoding.UTF8.GetString(bytes,pos,n)
            pos <- pos + n;
            res


    let E t = Expr< >(t,[])
    let EA (t,attribs) = Expr< >(t,attribs)
    let ES ts = List.map E ts

    let (|E|) (e: Expr) = e.Tree
    let (|ES|) (es: list<Expr>) = es |> List.map (fun e -> e.Tree)



    let funTyC = typeof<(obj -> obj)>.GetGenericTypeDefinition()  
    let exprTyC = typedefof<Expr<int>>
    let checkVoid a = if a = typeof<System.Void> then typeof<unit> else a
    let mkFunTy a b = 
        let (a, b) = checkVoid a, checkVoid b
        funTyC.MakeGenericType([| a;b |])

    let mkArrayTy (t:Type) = t.MakeArrayType();
    let mkExprTy (t:Type) = exprTyC.MakeGenericType([| t |])


    //--------------------------------------------------------------------------
    // Active patterns for decomposing quotations
    //--------------------------------------------------------------------------

    let (|Comb0|_|) (E x) = match x with CombTerm(k,[])  -> Some(k) | _ -> None
    let (|Comb1|_|) (E x) = match x with CombTerm(k,[x]) -> Some(k,x) | _ -> None
    let (|Comb2|_|) (E x) = match x with CombTerm(k,[x1;x2]) -> Some(k,x1,x2) | _ -> None
    let (|Comb3|_|) (E x) = match x with CombTerm(k,[x1;x2;x3]) -> Some(k,x1,x2,x3) | _ -> None
    
    let (|Var|_|)           (E x) = match x with VarTerm v        -> Some v     | _ -> None 
    let (|Application|_|)      x = match x with Comb2(AppOp,a,b) -> Some (a,b) | _ -> None 
    let (|Lambda|_|)        (E x) = match x with LambdaTerm(a,b)  -> Some (a,b) | _ -> None 

    let (|Quote|_|)         (E x) = match x with CombTerm(QuoteOp,[a])     -> Some (a)   | _ -> None 
    let (|IfThenElse|_|)          = function Comb3(IfThenElseOp,e1,e2,e3) -> Some(e1,e2,e3) | _ -> None

    let (|NewTuple|_|)         = function E(CombTerm(NewTupleOp(_),es)) -> Some(es) | _ -> None
    let (|DefaultValue|_|)         = function E(CombTerm(DefaultValueOp(ty),[])) -> Some(ty) | _ -> None

    let (|NewRecord|_|)          = function E(CombTerm(NewRecordOp(x),es)) -> Some(x,es) | _ -> None
    let (|NewUnionCase|_|)           = function E(CombTerm(NewUnionCaseOp(ucinfo),es)) -> Some(ucinfo,es) | _ -> None
    let (|UnionCaseTest|_|)    = function Comb1(UnionCaseTestOp(ucinfo),e) -> Some(e,ucinfo) | _ -> None
    let (|TupleGet|_|)      = function Comb1(TupleGetOp(ty,n),e) -> Some(e,n) | _ -> None
    let (|Coerce|_|)        = function Comb1(CoerceOp ty,e1) -> Some(e1,ty) | _ -> None
    let (|TypeTest|_|)        = function Comb1(TypeTestOp ty,e1) -> Some(e1,ty) | _ -> None
    let (|NewArray|_|)      = function E(CombTerm(NewArrayOp ty,es)) -> Some(ty,es) | _ -> None
    let (|AddressSet|_|)    = function E(CombTerm(AddressSetOp,[e;v])) -> Some(e,v) | _ -> None
    let (|TryFinally|_|)    = function E(CombTerm(TryFinallyOp,[e1;e2])) -> Some(e1,e2) | _ -> None
    let (|TryWith|_|)      = function E(CombTerm(TryWithOp,[e1;Lambda(v1,e2);Lambda(v2,e3)])) -> Some(e1,v1,e2,v2,e3) | _ -> None
    let (|VarSet|_|    )    = function E(CombTerm(VarSetOp,[E(VarTerm(v)); e])) -> Some(v,e) | _ -> None
    let (|Value|_|)         = function E(CombTerm(ValueOp (v,ty),_)) -> Some(v,ty) | _ -> None
    let (|ValueObj|_|)      = function E(CombTerm(ValueOp (v,_),_)) -> Some(v) | _ -> None

    let (|AddressOf|_|)       = function Comb1(AddressOfOp,e) -> Some(e) | _ -> None
    let (|Sequential|_|)    = function Comb2(SequentialOp,e1,e2) -> Some(e1,e2) | _ -> None
    let (|ForIntegerRangeLoop|_|)       = function Comb3(ForIntegerRangeLoopOp,e1,e2,Lambda(v, e3)) -> Some(v,e1,e2,e3) | _ -> None
    let (|WhileLoop|_|)     = function Comb2(WhileLoopOp,e1,e2) -> Some(e1,e2) | _ -> None
    let (|PropGet|_|)       = function E(CombTerm(StaticPropGetOp pinfo,args)) -> Some(None,pinfo,args) 
                                     | E(CombTerm(InstancePropGetOp pinfo,obj::args)) -> Some(Some(obj),pinfo,args) 
                                     | _ -> None
    let (|PropSet|_|)       = function E(CombTerm(StaticPropSetOp pinfo,v::args)) -> Some(None,pinfo,args,v) 
                                     | E(CombTerm(InstancePropSetOp pinfo,obj::v::args)) -> Some(Some(obj),pinfo,args,v) 
                                     | _ -> None
    let (|FieldGet|_|)      = function E(CombTerm(StaticFieldGetOp finfo,[])) -> Some(None,finfo) 
                                     | E(CombTerm(InstanceFieldGetOp finfo,[obj])) -> Some(Some(obj),finfo) 
                                     | _ -> None
    let (|FieldSet|_|)      = function E(CombTerm(StaticFieldSetOp finfo,[v])) -> Some(None,finfo,v) 
                                     | E(CombTerm(InstanceFieldSetOp finfo,[obj;v])) -> Some(Some(obj),finfo,v) 
                                     | _ -> None
    let (|NewObject|_|)      = function E(CombTerm(NewObjectOp ty,e)) -> Some(ty,e) | _ -> None
    let (|Call|_|)           = function E(CombTerm(StaticMethodCallOp minfo,args)) -> Some(None,minfo,args) 
                                      | E(CombTerm(InstanceMethodCallOp minfo,(obj::args))) -> Some(Some(obj),minfo,args) 
                                      | _ -> None
    let (|LetRaw|_|)        = function Comb2(LetOp,e1,e2) -> Some(e1,e2) | _ -> None
    let (|LetRecRaw|_|)     = function Comb1(LetRecOp,e1) -> Some(e1) | _ -> None

    let (|Let|_|) = function LetRaw(e,Lambda(v,body)) -> Some(v,e,body) | _ -> None

    let (|IteratedLambda|_|) (e: Expr) = qOneOrMoreRLinear (|Lambda|_|) e 
    let rec (|NLambdas|_|) n (e:Expr) = 
        match e with 
        | _ when n <= 0 -> Some([],e) 
        | Lambda(v,NLambdas ((-) n 1) (vs,b)) -> Some(v::vs,b)
        | _ -> None

    let (|NewDelegate|_|) e  = 
        match e with 
        | Comb1(NewDelegateOp(ty),e) -> 
            let n = (getDelegateInvoke ty).GetParameters().Length
            match e with 
            | NLambdas n (vs,e) -> Some(ty,vs,e) 
            | _ -> None
        | _ -> None

    let (|LetRec|_|) e = 
        match e with 
        | LetRecRaw(IteratedLambda(vs1,E(CombTerm(LetRecCombOp,body::es)))) -> Some(List.zip vs1 es,body)
        | _ -> None
    
    //--------------------------------------------------------------------------
    // Getting the type of Raw quotations
    //--------------------------------------------------------------------------

    // Returns record member specified by name
    let getRecordProperty(ty,fieldName) =    
        let mems = FSharpType.GetRecordFields(ty,publicOrPrivateBindingFlags)
        match mems |> Array.tryFind (fun minfo -> minfo.Name = fieldName) with
        | Some (m) -> m
        | _ -> invalidArg  "fieldName" "couldn't find a record member with specified name"

    let getUnionCaseInfo(ty,unionCaseName) =    
        let cases = FSharpType.GetUnionCases(ty,publicOrPrivateBindingFlags)
        match cases |> Array.tryFind (fun ucase -> ucase.Name = unionCaseName) with
        | Some(case) -> case
        | _ -> invalidArg  "unionCaseName" ("couldn't find a union case with the name '" + unionCaseName + "'")
    
    let getUnionCaseInfoField(ucinfo:UnionCaseInfo,index) =    
        let fields = ucinfo.GetFields() 
        if index < 0 || index >= fields.Length then invalidArg "index" "not a valid union case index"
        fields.[index]
 
    /// Returns type of lambda applciation - something like "(fun a -> ..) b"
    let rec typeOfAppliedLambda f =
        let fty = ((typeOf f):Type) 
        match fty.GetGenericArguments() with 
        | [|a; b|] -> b
        | _ -> failwith "ill formed expression: AppOp or LetOp"          

    /// Returns type of the Raw quotation or fails if the quotation is ill formed
    /// if 'verify' is true, verifies all branches, otherwise ignores some of them when not needed
    and typeOf<'T> (e : ('T :> Expr)) : Type = 
        let (E t) = e 
        match t with 
        | VarTerm    v        -> v.Type
        | LambdaTerm (v,b)    -> mkFunTy v.Type (typeOf b)
        | HoleTerm   (ty,_)   -> ty
        | CombTerm   (c,args) -> 
            match c,args with 
            | AppOp,[f;x] -> typeOfAppliedLambda f
            | LetOp,_ -> match e with Let(_,_,b) -> typeOf b | _ -> failwith "unreachable"
            | IfThenElseOp,[_;t;_]  -> typeOf t
            | LetRecOp,_     -> match e with LetRec(_,b) -> typeOf b | _ -> failwith "unreachable"
            | LetRecCombOp,_        -> failwith "typeOfConst: LetRecCombOp"
            | NewRecordOp ty,_         -> ty
            | NewUnionCaseOp ucinfo,_   -> ucinfo.DeclaringType
            | UnionCaseTestOp ucinfo,_ -> typeof<Boolean>
            | ValueOp (o, ty),_  -> ty
            | TupleGetOp (ty,i),_ -> FSharpType.GetTupleElements(ty).[i] 
            | NewTupleOp ty,_      -> ty
            | StaticPropGetOp prop,_    -> prop.PropertyType
            | InstancePropGetOp prop,_    -> prop.PropertyType
            | StaticPropSetOp prop ,_   -> typeof<Unit>
            | InstancePropSetOp prop,_    -> typeof<Unit>
            | InstanceFieldGetOp fld ,_   -> fld.FieldType
            | StaticFieldGetOp fld ,_   -> fld.FieldType
            | InstanceFieldSetOp fld,_    -> typeof<Unit>
            | StaticFieldSetOp fld,_    -> typeof<Unit>
            | NewObjectOp ctor,_   -> ctor.DeclaringType
            | InstanceMethodCallOp minfo,_   -> minfo.ReturnType |> checkVoid
            | StaticMethodCallOp minfo,_   -> minfo.ReturnType |> checkVoid
            | CoerceOp ty,_       -> ty
            | SequentialOp,[a;b]      -> typeOf b 
            | ForIntegerRangeLoopOp,_  -> typeof<Unit>
            | NewArrayOp ty,_      -> mkArrayTy ty
            | NewDelegateOp ty,_     -> ty
            | DefaultValueOp ty,_     -> ty
            | QuoteOp,[expr]        -> mkExprTy (typeOf expr)
            | TryFinallyOp,[e1;e2]        -> typeOf e1
            | TryWithOp,[e1;e2;e3]        -> typeOf e1
            | WhileLoopOp,_ 
            | AddressSetOp,_ -> typeof<Unit> 
            | AddressOfOp,_ -> failwith "can't take the address of this quotation"
            | _   -> failwith "unreachable"

    //--------------------------------------------------------------------------
    // Constructors for building Raw quotations
    //--------------------------------------------------------------------------
      
    let mkFEN op l = E(CombTerm(op,l))
    let mkFE0 op = E(CombTerm(op,[]))
    let mkFE1 op x = E(CombTerm(op,[(x:>Expr)]))
    let mkFE2 op (x,y) = E(CombTerm(op,[(x:>Expr);(y:>Expr)]))
    let mkFE3 op (x,y,z) = E(CombTerm(op,[(x:>Expr);(y:>Expr);(z:>Expr)])  )
    let mkOp v () = v

    let mkLetRaw v = mkFE2 LetOp v

    //--------------------------------------------------------------------------
    // Type-checked constructors for building Raw quotations
    //--------------------------------------------------------------------------
  
    // t2 is inherited from t1 / t2 implements interface t1 or t2 == t1
    let assignableFrom (t1:Type) (t2:Type) =  
        t1.IsAssignableFrom(t2)
      
    let checkTypes (expectedType: Type) (receivedType : Type)  msg1 msg2 =
        if (expectedType <> receivedType) then 
          invalidArg "receivedType" (sprintf "type mismatch when building '%s': %s. Expected '%A', got type '%A'" msg1 msg2 expectedType receivedType)

    let checkTypesWeak (expectedType: Type) (receivedType : Type)  msg1 msg2 = 
        if (not (assignableFrom expectedType receivedType)) then 
          invalidArg "receivedType" (sprintf "type mismatch when building '%s': %s. Expected '%A', got type '%A'" msg1 msg2 expectedType receivedType)
  
    let checkArgs  (paramInfos: ParameterInfo[]) (args:list<Expr>) =  
        if (paramInfos.Length <> args.Length) then invalidArg "args" "incorrect number of arguments"
        List.iter2
            ( fun (p:ParameterInfo) a -> checkTypesWeak p.ParameterType (typeOf a) "args" "invalid parameter for a method or indexer property") 
            (paramInfos |> Array.to_list) 
            args
                                                // todo: shouldn't this be "strong" type check? sometimes?

    let checkAssignableFrom ty1 ty2 = 
        if not (assignableFrom ty1 ty2) then invalidArg "ty2" "incorrect type"

    let checkObj  (membInfo: MemberInfo) (obj: Expr) = 
        if not (assignableFrom membInfo.DeclaringType (typeOf obj)) then invalidArg "obj" "incorrect instance type"

      
    // Checks lambda application for correctnes
    let checkAppliedLambda (f, v) =
        let fty = typeOf f
        let ftyG = (if fty.IsGenericType then  fty.GetGenericTypeDefinition()  else fty)
        checkTypes funTyC ftyG "f" "expected function type in function application or let binding"
        let vty = (typeOf v)
        match fty.GetGenericArguments() with 
          | [|a; b|] -> checkTypes vty a "f" "function argument type doesn't match"
          | _ -> invalidArg  "f" "invalid function type"  
  
    // Returns option (by name) of a NewUnionCase type
    let getUnionCaseFields ty str =       
        let cases = FSharpType.GetUnionCases(ty,publicOrPrivateBindingFlags)
        match cases |> Array.tryFind (fun ucase -> ucase.Name = str) with
        | Some(case) -> case.GetFields()
        | _ -> invalidArg  "ty" "type is not a union type"
  
    let checkBind(v:Var,e) = 
        let ety = typeOf e
        checkTypes v.Type ety "let" "the variable type doesn't match the type of the rhs of a let binding"
  
    // [Correct by definition]
    let mkVar v       = E(VarTerm v )
    let mkQuote(a)    = E(CombTerm(QuoteOp,[(a:>Expr)] ))
          
    let mkValue (v,ty) = mkFE0 (ValueOp(v,ty))
    let mkValueG (v:'T) = mkValue(box v, typeof<'T>)
    let mkLiftedValueOpG (v:'T) = ValueOp(box v, typeof<'T>)
    let mkUnit       () = mkValue(null, typeof<unit>)
    let mkBool        (v:bool) = mkValueG(v)
    let mkString      (v:string) = mkValueG(v)
    let mkSingle      (v:single) = mkValueG(v)
    let mkDouble      (v:double) = mkValueG(v)
    let mkChar        (v:char) = mkValueG(v)
    let mkSByte       (v:sbyte) = mkValueG(v)
    let mkByte        (v:byte) = mkValueG(v)
    let mkInt16       (v:int16) = mkValueG(v)
    let mkUInt16      (v:uint16) = mkValueG(v)
    let mkInt32       (v:int32) = mkValueG(v)
    let mkUInt32      (v:uint32) = mkValueG(v)
    let mkInt64       (v:int64) = mkValueG(v)
    let mkUInt64      (v:uint64) = mkValueG(v)
    let mkAddressOf     v = mkFE1 AddressOfOp v
    let mkSequential  (e1,e2) = mkFE2 SequentialOp (e1,e2) 
    let mkTypeTest    (e,ty) = mkFE1 (TypeTestOp(ty)) e
    let mkVarSet    (v,e) = mkFE2 VarSetOp (mkVar(v),e)
    let mkAddressSet    (e1,e2) = mkFE2 AddressSetOp (e1,e2)
    let mkLambda(var,body) = E(LambdaTerm(var,(body:>Expr)))
    let mkTryWith(e1,v1,e2,v2,e3) = mkFE3 TryWithOp (e1,mkLambda(v1,e2),mkLambda(v2,e3))
    let mkTryFinally(e1,e2) = mkFE2 TryFinallyOp (e1,e2)
    
    let mkCoerce      (ty,x) = mkFE1 (CoerceOp ty) x
    let mkNull        (ty)   = mkFE0 (ValueOp(null,ty))
    
    let mkApplication v = checkAppliedLambda v; mkFE2 AppOp v 

    // Tuples
    let mkNewTupleWithType    (ty,args:Expr list) = 
        let mems = FSharpType.GetTupleElements ty |> Array.to_list
        if (args.Length <> mems.Length) then invalidArg  "args" "incompatible tuple length"
        List.iter2(fun mt a -> checkTypes mt (typeOf a) "args" "Mismatching type of argument and tuple element." ) mems args
        mkFEN (NewTupleOp ty) args 
    
    let mkNewTuple (args) = 
        let ty = FSharpType.MakeTupleType(Array.map typeOf (Array.of_list args))
        mkFEN (NewTupleOp ty) args
    
    let mkTupleGet (ty,n,x) = 
        checkTypes ty (typeOf x) "tupleGet" "expression doesn't match the tuple type"  
        let mems = FSharpType.GetTupleElements ty 
        if (n < 0 or mems.Length <= n) then invalidArg  "n" "tuple access out of range"
        mkFE1 (TupleGetOp (ty,n)) x
    
    // Records
    let mkNewRecord (ty,args:list<Expr>) = 
        let mems = FSharpType.GetRecordFields(ty,publicOrPrivateBindingFlags) 
        if (args.Length <> mems.Length) then invalidArg  "args" "incompatible record length"
        List.iter2 (fun (minfo:PropertyInfo) a -> checkTypes minfo.PropertyType (typeOf a) "recd" "incorrect argument type for a record") (Array.to_list mems) args
        mkFEN (NewRecordOp ty) args
      
      
    // Discriminated unions        
    let mkNewUnionCase (ucinfo:UnionCaseInfo,args:list<Expr>) = 
        let sargs = ucinfo.GetFields()
        if (args.Length <> sargs.Length) then invalidArg  "args" "union type requires different number of arguments"
        List.iter2 (fun (minfo:PropertyInfo) a  -> checkTypes minfo.PropertyType (typeOf a) "sum" "incorrect argument type for a union") (Array.to_list sargs) args
        mkFEN (NewUnionCaseOp ucinfo) args
        
    let mkUnionCaseTest (ucinfo:UnionCaseInfo,expr) = 
        checkTypes ucinfo.DeclaringType (typeOf expr) "UnionCaseTagTest" "types of expression does not match"
        mkFE1 (UnionCaseTestOp ucinfo) expr

    // Conditional etc..
    let mkIfThenElse (e,t,f) = 
        checkTypes (typeOf t) (typeOf f) "cond" "types of true and false branches differ"
        checkTypes (typeof<Boolean>) (typeOf e) "cond" "condition expression must be of type bool"
        mkFE3 IfThenElseOp (e,t,f)               
        
    let mkNewArray (ty,args) = 
        List.iter (fun a -> checkTypes ty (typeOf a) "newArray" "initializer doesn't match array type") args
        mkFEN (NewArrayOp ty) args
        
    let mkInstanceFieldGet(obj,finfo:FieldInfo) =
        match finfo.IsStatic with 
        | false -> 
            checkObj finfo obj
            mkFE1 (InstanceFieldGetOp finfo) obj
        | true -> invalidArg  "finfo" "object provided for static member"
      
    let mkStaticFieldGet    (finfo:FieldInfo) =
        match finfo.IsStatic with 
        | true -> mkFE0 (StaticFieldGetOp finfo) 
        | false -> invalidArg  "finfo" "no object provided for instance member"
      
    let mkStaticFieldSet (finfo:FieldInfo,value:Expr) =
        checkTypes (typeOf value) finfo.FieldType "value" "the type of the field was incorrect"
        match finfo.IsStatic with 
        | true -> mkFE1 (StaticFieldSetOp finfo) value
        | false -> invalidArg  "finfo" "no object provided for instance member"
      
    let mkInstanceFieldSet (obj,finfo:FieldInfo,value:Expr) =
        checkTypes (typeOf value) finfo.FieldType "value" "the type of the field was incorrect"
        match finfo.IsStatic with 
        | false -> 
            checkObj finfo obj
            mkFE2 (InstanceFieldSetOp finfo) (obj,value)
        | true -> invalidArg  "finfo" "object provided for static member"
      
    let mkCtorCall (ci:ConstructorInfo,args:list<Expr>) =
        checkArgs (ci.GetParameters()) args
        mkFEN (NewObjectOp ci) args

    let mkDefaultValue (ty:Type) =
        mkFE0 (DefaultValueOp ty) 

    let mkStaticPropGet (pinfo:PropertyInfo,args:list<Expr>) = 
        if (not pinfo.CanRead) then invalidArg  "pinfo" "reading a set-only property"
        checkArgs (pinfo.GetIndexParameters()) args
        match pinfo.GetGetMethod(true).IsStatic with 
        | true -> mkFEN (StaticPropGetOp  pinfo) args
        | false -> invalidArg  "pinfo" "no object provided for instance member"

    let mkInstancePropGet (obj,pinfo:PropertyInfo,args:list<Expr>) = 
        if (not pinfo.CanRead) then invalidArg  "pinfo" "reading a set-only property"
        checkArgs (pinfo.GetIndexParameters()) args
        match pinfo.GetGetMethod(true).IsStatic with 
        | false -> 
            checkObj pinfo obj
            mkFEN (InstancePropGetOp pinfo) (obj::args)
        | true -> invalidArg  "pinfo" "object provided for static member"
          
    let mkStaticPropSet (pinfo:PropertyInfo,args:list<Expr>,value:Expr) = 
        if (not pinfo.CanWrite) then invalidArg  "pinfo" "writing a get-only property"
        checkArgs (pinfo.GetIndexParameters()) args
        match pinfo.GetSetMethod(true).IsStatic with 
        | true -> mkFEN (StaticPropSetOp pinfo) (value::args)
        | false -> invalidArg  "pinfo" "no object provided for instance member"
          
    let mkInstancePropSet (obj,pinfo:PropertyInfo,args:list<Expr>,value:Expr) = 
        if (not pinfo.CanWrite) then invalidArg  "pinfo" "writing a get-only property"
        checkArgs (pinfo.GetIndexParameters()) args
        match pinfo.GetSetMethod(true).IsStatic with 
        | false -> 
            checkObj pinfo obj
            mkFEN (InstancePropSetOp pinfo) (obj::value::args)
        | true -> invalidArg  "pinfo" "object provided for static member"
          
    let mkInstanceMethodCall (obj,minfo:MethodInfo,args:list<Expr>) =
        checkArgs (minfo.GetParameters()) args
        match minfo.IsStatic with 
        | false -> 
            checkObj minfo obj
            mkFEN (InstanceMethodCallOp minfo) (obj::args)
        | true -> invalidArg  "minfo" "object provided for static member"
    
    let mkStaticMethodCall (minfo:MethodInfo,args:list<Expr>) =
        checkArgs (minfo.GetParameters()) args
        match minfo.IsStatic with 
        | true -> mkFEN (StaticMethodCallOp minfo) args
        | false -> invalidArg  "minfo" "no object provided for instance member"
    
    let mkForLoop (v:Var,lowerBound,upperBound,body) = 
        checkTypes (typeof<int>) (typeOf lowerBound) "lowerBound" "lower bound variable must be an integer"
        checkTypes (typeof<int>) (typeOf upperBound) "upperBound" "upper bound variable must be an integer"
        checkTypes (typeof<int>) (v.Type) "for" "body of the for loop must be lambda taking integer as an argument"
        mkFE3 ForIntegerRangeLoopOp (lowerBound, upperBound, mkLambda(v,body))
      
    let mkWhileLoop (guard,body) = 
        checkTypes (typeof<bool>) (typeOf guard) "guard" "guard must return boolean"
        checkTypes (typeof<Unit>) (typeOf body) "body" "body must return unit"
        mkFE2 (WhileLoopOp) (guard,body)
    
    let mkNewDelegate (ty,e) = 
        let mi = getDelegateInvoke ty
        let ps = mi.GetParameters()
        let dlfun = Array.foldBack (fun (p:ParameterInfo) rty -> mkFunTy p.ParameterType rty) ps mi.ReturnType
        checkTypes dlfun (typeOf e) "ty" "Function type doesn't match delegate type."
        mkFE1 (NewDelegateOp ty) e
    
    let mkLet (v,e,b) = 
        checkBind (v,e);
        mkLetRaw (e,mkLambda(v,b))

    //let mkLambdas(vs,b) = mkRLinear mkLambdaRaw (vs,(b:>Expr))
    let mkTupledApplication (f,args) = 
        match args with 
        | [] -> mkApplication (f,mkUnit())
        | [x] -> mkApplication (f,x)
        | _ -> mkApplication (f,mkNewTuple args)
        
    let mkApplications(f: Expr,es:list<list<Expr>>) = mkLLinear mkTupledApplication (f,es)
    
    let mkIteratedLambdas(vs,b) = mkRLinear  mkLambda (vs,b)
    
    let mkLetRecRaw v = mkFE1 LetRecOp v
    let mkLetRecCombRaw v = mkFEN LetRecCombOp v
    let mkLetRec (ves:(Var*Expr) list,body) = 
        List.iter checkBind ves;
        let vs,es = List.unzip ves 
        mkLetRecRaw(mkIteratedLambdas (vs,mkLetRecCombRaw (body::es)))

    let ReflectedDefinitionsResourceNameBase = "ReflectedDefinitions"

    //-------------------------------------------------------------------------
    // General Method Binder

    let typeEquals     (s:Type) (t:Type) = s.Equals(t)
    let typesEqual (ss:Type list) (tt:Type list) =
      (ss.Length = tt.Length) && List.forall2 typeEquals ss tt

    let instFormal (typarEnv: Type[]) (ty:Instantiable<'T>) = ty (fun i -> typarEnv.[i])

    let getGenericArguments(tc:Type) = 
        if tc.IsGenericType then tc.GetGenericArguments() else [| |] 

    let getNumGenericArguments(tc:Type) = 
        if tc.IsGenericType then tc.GetGenericArguments().Length else 0
    
    let bindMethodBySearch (parentT:Type,nm,marity,argtys,rty) =
        let methInfos = parentT.GetMethods(staticOrInstanceBindingFlags) |> Array.to_list 
        // First, filter on name, if unique, then binding "done" 
        let tyargTs = getGenericArguments(parentT) 
        let methInfos = methInfos |> List.filter (fun methInfo -> methInfo.Name = nm)
        match methInfos with 
        | [methInfo] -> 
            methInfo
        | _ ->
            // Second, type match. Note type erased (non-generic) F# code would not type match but they have unique names 
            let select (methInfo:MethodInfo) =
                let tyargTIs = if parentT.IsGenericType then parentT.GetGenericArguments() else [| |] 
                // mref implied Types 
                let mtyargTIs = if methInfo.IsGenericMethod then methInfo.GetGenericArguments() else [| |] 
                if mtyargTIs.Length  <> marity then false (* method generic arity mismatch *) else
                let typarEnv = (Array.append tyargTs mtyargTIs) 
                let argTs = argtys |> List.map (instFormal typarEnv) 
                let resT  = instFormal typarEnv rty 
                
                // methInfo implied Types 
                let haveArgTs = 
                    let parameters = Array.to_list (methInfo.GetParameters()) 
                    parameters |> List.map (fun param -> param.ParameterType) 
                let haveResT  = methInfo.ReturnType 
                // check for match 
                if argTs.Length <> haveArgTs.Length then false (* method argument length mismatch *) else
                let res = typesEqual (resT::argTs) (haveResT::haveArgTs) 
                res
            // return MethodInfo for (generic) type's (generic) method 
            match List.tryFind select methInfos with
            | None          -> failwith "convMethodRef: could not bind to method"
            | Some methInfo -> methInfo 

    let bindMethodHelper (parentT: Type, nm,marity,argtys,rty) =
      if parentT = null then invalidArg "parentT" "parent type should not be null"
      if marity = 0 then 
          let tyargTs = if parentT.IsGenericType then parentT.GetGenericArguments() else [| |] 
          let argTs,resT = 
              let argTs = Array.of_list (List.map (instFormal tyargTs) argtys) 
              let resT  = instFormal tyargTs rty 
              argTs,resT 
          let methInfo = 
              try 
                 match parentT.GetMethod(nm,staticOrInstanceBindingFlags,null,argTs,null) with 
                 | null -> None
                 | res -> Some(res)
               with :? AmbiguousMatchException -> None 
          match methInfo with 
          | Some methInfo when (typeEquals resT methInfo.ReturnType) -> methInfo
          | _ -> bindMethodBySearch(parentT,nm,marity,argtys,rty)
      else 
          bindMethodBySearch(parentT,nm,marity,argtys,rty)

    let bindModuleProperty (ty:Type,nm) = 
        match ty.GetProperty(nm,staticBindingFlags) with
        | null -> failwith ("Couldn't bind property " + nm + " in type " + (ty.ToString()))
        | res -> res

            
    let bindModuleFunction (ty:Type,nm) = 
        match ty.GetMethod(nm,staticBindingFlags) with 
        | null -> failwith ("Couldn't bind function " + nm + " in type " + (ty.ToString()))
        | res -> res
            
    let mkNamedType (tc:Type,tyargs)  =
        match  tyargs with 
        | [] -> tc
        | _ -> tc.MakeGenericType(Array.of_list tyargs)

    let inline checkNonNullArg (arg:string,err:string) y = match box y with null -> raise (new ArgumentNullException(arg,err)) | _ -> y

    let inst (tyargs:Type list) (i: Instantiable<'T>) = i (fun idx -> tyargs.[idx]) // Note, O(n) looks, but #tyargs is always small
    
    let bindProp (tc,propName,retType,argTypes,tyargs) =
        // We search in the instantiated type, rather than searching the generic type.
        let typ = mkNamedType(tc,tyargs)
        let argtyps : Type list = argTypes |> inst tyargs
        let retType : Type = retType |> inst tyargs
        typ.GetProperty(propName, staticOrInstanceBindingFlags, null, retType, Array.of_list argtyps,null) |> checkNonNullArg ("propName","failed to bind property '"+ propName+"'") // fxcop may not see "propName" as an arg

    let bindField (tc,fldName,tyargs) =
        let typ = mkNamedType(tc,tyargs)
        typ.GetField(fldName,staticOrInstanceBindingFlags) |> checkNonNullArg ("fldName","failed to bind field '"+ fldName+"'")  // fxcop may not see "fldName" as an arg

    let bindGenericCtor (tc:Type,argTypes:Instantiable<Type list>) =
        let argtyps =  instFormal (getGenericArguments tc) argTypes
        tc.GetConstructor(instanceBindingFlags,null,Array.of_list argtyps,null) |> checkNonNullArg ("tc","failed to bind constructor")  // fxcop may not see "tc" as an arg

    let bindCtor (tc,argTypes:Instantiable<Type list>,tyargs) =
        let typ = mkNamedType(tc,tyargs)
        let argtyps = argTypes |> inst tyargs
        typ.GetConstructor(instanceBindingFlags,null,Array.of_list argtyps,null) |> checkNonNullArg ("tc","failed to bind constructor") // fxcop may not see "tc" as an arg

    let chop n xs =
        if n<0 then failwith "List.chop: -ve" else
        let rec split l = 
            match l with 
            | 0,xs    -> [],xs
            | n,x::xs -> let front,back = split (n-1,xs)
                         x::front,back
            | n,[]    -> failwith "List.chop: not enough elts list"
        split (n,xs)

    let instMeth (ngmeth: MethodInfo, methTypeArgs) = 
        if ngmeth.GetGenericArguments().Length = 0 then ngmeth(* non generic *) 
        else ngmeth.MakeGenericMethod(Array.of_list methTypeArgs) 

    let bindGenericMeth (tc:Type,argTypes : list<Instantiable<Type>>,retType,methName,numMethTyargs) =
        bindMethodHelper(tc,methName,numMethTyargs,argTypes,retType) 

    let bindMeth ((tc:Type,argTypes : list<Instantiable<Type>>,retType,methName,numMethTyargs),tyargs) =
        let ntyargs = tc.GetGenericArguments().Length 
        let enclTypeArgs,methTypeArgs = chop ntyargs tyargs
        let ty = mkNamedType(tc,enclTypeArgs)
        let ngmeth = bindMethodHelper(ty,methName,numMethTyargs,argTypes,retType) 
        instMeth(ngmeth,methTypeArgs)

    let pinfoIsStatic (pinfo:PropertyInfo) = 
        if pinfo.CanRead then pinfo.GetGetMethod(true).IsStatic
        elif pinfo.CanWrite then pinfo.GetSetMethod(true).IsStatic
        else false
        
    //--------------------------------------------------------------------------
    // Unpickling
    //--------------------------------------------------------------------------

    module SimpleUnpickle = 

        type instate = 
          { is: ByteStream; 
            istrings: string array;
            localAssembly: System.Reflection.Assembly  }

        let u_byte_as_int st = st.is.ReadByte() 

        let u_bool st = let b = u_byte_as_int st in (b = 1) 
        let u_void (is: instate) = ()
        let u_unit (is: instate) = ()
        let prim_u_int32 st = 
            let b0 =  (u_byte_as_int st)
            let b1 =  (u_byte_as_int st)
            let b2 =  (u_byte_as_int st)
            let b3 =  (u_byte_as_int st)
            b0 ||| (b1 <<< 8) ||| (b2 <<< 16) ||| (b3 <<< 24)

        let u_int32 st = 
            let b0 = u_byte_as_int st 
            if b0 <= 0x7F then b0 
            elif b0 <= 0xbf then 
                let b0 = b0 &&& 0x7f 
                let b1 = (u_byte_as_int st) 
                (b0 <<< 8) ||| b1
            else  
                prim_u_int32 st

        let u_bytes st = 
            let n = u_int32 st 
            st.is.ReadBytes(n)

        let prim_u_string st = 
            let len =  (u_int32 st) 
            st.is.ReadUtf8BytesAsString(len)

        let u_int    st = u_int32 st
        let u_sbyte  st = sbyte (u_int32 st)
        let u_byte   st = byte (u_byte_as_int st)
        let u_int16  st = int16 (u_int32 st)
        let u_uint16 st = uint16 (u_int32 st)
        let u_uint32 st = uint32 (u_int32 st)
        let u_int64  st = 
            let b1 = int64 (u_int32 st) &&& 0xFFFFFFFFL 
            let b2 = int64 (u_int32 st) 
            b1 ||| (b2 <<< 32)
        let u_uint64  st = uint64 (u_int64 st)
        let u_double st = System.BitConverter.ToDouble(System.BitConverter.GetBytes(u_int64 st),0)
        let u_float32 st = System.BitConverter.ToSingle(System.BitConverter.GetBytes(u_int32 st),0)
        let u_char st = char (int32 (u_uint16 st))
        let inline u_tup2 p1 p2 st = let a = p1 st in let b = p2 st in (a,b)
        let inline u_tup3 p1 p2 p3 st =
            let a = p1 st in let b = p2 st in let c = p3 st in (a,b,c)
        let inline u_tup4 p1 p2 p3 p4 st =
            let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in (a,b,c,d)
        let inline u_tup5 p1 p2 p3 p4 p5 st =
            let a = p1 st in let b = p2 st in let c = p3 st in let d = p4 st in let e = p5 st in (a,b,c,d,e)
        let u_uniq (tbl: _ array) st = 
            let n = u_int st 
            if n < 0 || n >= tbl.Length then failwith ("u_uniq: out of range, n = "+string n+ ", sizeof(tab) = " + string tbl.Length); 
            tbl.[n]
        let u_string st = u_uniq st.istrings st

        let rec u_list_aux f acc st = 
            let tag = u_byte_as_int st 
            match tag with
            | 0 -> List.rev acc
            | 1 -> let a = f st in u_list_aux f (a::acc) st 
            | n -> failwith ("u_list: found number " + string n)
        let u_list f st = u_list_aux f [] st
         
        let unpickle_obj localAssembly u phase2bytes =
            let phase2data = 
                let st2 = 
                   { is = new ByteStream(phase2bytes,0,phase2bytes.Length); 
                     istrings = [| |];
                     localAssembly=localAssembly }
                u_tup2 (u_list prim_u_string) u_bytes st2 
            let stringTab,phase1bytes = phase2data 
            let st1 = 
               { is = new ByteStream(phase1bytes,0,phase1bytes.Length); 
                 istrings = Array.of_list stringTab;
                   localAssembly=localAssembly } 
            let res = u st1 
            res 

    open SimpleUnpickle

    let decodeFunTy args =
        match args with 
        | [d;r] -> funTyC.MakeGenericType([| d; r |])
        | _ -> invalidArg "args" "expected two type arguments"

    let decodeArrayTy n (tys: Type list) = 
        match tys with
        | [ty] -> if (n = 1) then ty.MakeArrayType() else ty.MakeArrayType(n)  
                  // typeof<int>.MakeArrayType(1) returns "Int[*]" but we need "Int[]"
        | _ -> invalidArg "tys" "expected one type argument"
        
    let mkNamedTycon (tcName,ass:Assembly) =
        match ass.GetType(tcName) with 
        | null  -> 
            // For some reason we can get 'null' returned here even when a type with the right name exists... Hence search the slow way...
            match (ass.GetTypes() |> Array.tryFind (fun a -> a.FullName = tcName)) with 
            | Some ty -> ty
            | None -> invalidArg "tcName" "failed to bind type '%s' in assembly '%O'." tcName ass // "Available types are:\n%A" tcName ass (ass.GetTypes() |> Array.map (fun a -> a.FullName))
        | ty -> ty

    let decodeNamedTy tc tsR = mkNamedType(tc,tsR)

    let mscorlib = typeof<System.Int32>.Assembly
    let u_assref st = 
        let a = u_string st 
        if a = "" then mscorlib
        elif a = "." then st.localAssembly 
        else 
            match System.Reflection.Assembly.Load(a) with 
            | null -> failwithf "failed to bind assembly '%s' while processing quotation data" a
            | ass -> ass
        
    let u_NamedType st = 
        let a,b = u_tup2 u_string u_assref st 
        mkNamedTycon (a,b)
    let u_tyvarSpec st = let tvName = u_string st in ()
    let u_tyconstSpec st = 
      let tag = u_byte_as_int st 
      match tag with 
      | 1 -> u_unit             st |> (fun () -> decodeFunTy) 
      | 2 -> u_NamedType st |> decodeNamedTy 
      | 3 -> u_int              st |> decodeArrayTy
      | _ -> failwith "u_tyconstSpec" 

    let appL fs env = List.map (fun f -> f env) fs
    
    let rec u_dtype st : (int -> Type) -> Type = 
      let tag = u_byte_as_int st 
      match tag with 
      | 0 -> u_int                              st |> (fun x env     -> env(x)) 
      | 1 -> u_tup2 u_tyconstSpec (u_list u_dtype) st |> (fun (a,b) env -> a (appL b env))
      | _ -> failwith "u_dtype" 

    let u_dtypes st = let a = u_list u_dtype st in appL a 

    let (|NoTyArgs|) = function [] -> () | _ -> failwith "incorrect number of arguments during deserialization"
    let (|OneTyArg|) = function [x] -> x | _ -> failwith "incorrect number of arguments during deserialization"
    
    type env = 
        { vars : Map<int,Var>; varn: int; typeInst : int -> Type }
    let addVar env v = 
        { env with vars = env.vars.Add(env.varn,v); varn=env.varn+1 }
    let envClosed (types:Type[])  =
        { vars = Map.Empty; 
          varn = 0;
          typeInst = fun (n:int) -> types.[n] }

    type Bindable<'T> = env -> 'T
    
    let rec u_Expr st = 
        let tag = u_byte_as_int st 
        match tag with 
        | 0 -> u_tup3 u_constSpec u_dtypes (u_list u_Expr) st 
                |> (fun (a,b,args) (env:env) -> 
                    let tyargs = b env.typeInst 
                    E(CombTerm(a tyargs, List.map (fun e -> e env) args ))) 
        | 1 -> let x = u_VarRef st 
               (fun env -> E(VarTerm (x env)))
        | 2 -> let a = u_VarDecl st
               let b = u_Expr st
               (fun env -> let v = a env in E(LambdaTerm(v,b (addVar env v))))
        | 3 -> let a = u_dtype st
               let idx = u_int st
               (fun env -> E(HoleTerm(a env.typeInst , idx)))
        | 4 -> let a = u_Expr st
               (fun env -> mkQuote(a env))
        | 5 -> let a = u_Expr st
               let attrs = u_list u_Expr st
               (fun env -> let e = (a env) in EA(e.Tree,(e.CustomAttributes @ List.map (fun attrf -> attrf env) attrs)))
        | _ -> failwith "u_Expr"
    and u_VarDecl st = 
        let s,b = u_tup2 u_string u_dtype st in 
        (fun env -> new Var(s, b env.typeInst))
    and u_VarRef st = 
        let i = u_int st in 
        (fun env -> env.vars.[i])
    and u_RecdField st = 
        let ty,nm = u_tup2 u_NamedType u_string st  
        (fun tyargs -> getRecordProperty(mkNamedType(ty,tyargs),nm)) 
    and u_UnionCaseInfo st = 
        let ty,nm = u_tup2 u_NamedType u_string st  
        (fun tyargs -> getUnionCaseInfo(mkNamedType(ty,tyargs),nm)) 

    and u_UnionCaseField st = 
        let case,i = u_tup2 u_UnionCaseInfo u_int st  
        (fun tyargs -> getUnionCaseInfoField(case tyargs,i))

    and u_ModuleDefn st = 
        let (ty,nm,isProp) = u_tup3 u_NamedType u_string u_bool st 
        if isProp then StaticPropGetOp(bindModuleProperty(ty,nm)) 
        else StaticMethodCallOp(bindModuleFunction(ty,nm))

    and u_MethodInfoData st = 
        u_tup5 u_NamedType (u_list u_dtype) u_dtype u_string u_int st
            
    and u_PropInfoData st = 
        u_tup4 u_NamedType u_string u_dtype u_dtypes  st
        
    and u_CtorInfoData st =
        u_tup2 u_NamedType u_dtypes st
    
    and u_MethodBase st = 
        let tag = u_byte_as_int st 
        match tag with 
        | 0 -> 
            match u_ModuleDefn st with 
            | StaticMethodCallOp(minfo) -> (minfo :> MethodBase)
            | StaticPropGetOp(pinfo) -> (pinfo.GetGetMethod() :> MethodBase)
            | _ -> failwith "unreachable"
        | 1 -> 
            let data = u_MethodInfoData st
            let minfo = bindGenericMeth(data) in 
            (minfo :> MethodBase)
        | 2 -> 
            let data = u_CtorInfoData st
            let cinfo = bindGenericCtor(data) in 
            (cinfo :> MethodBase)
        | _ -> failwith "u_MethodBase" 

      
    and u_constSpec st = 
        let tag = u_byte_as_int st 
        match tag with 
        | 0 -> u_void       st |> (fun () NoTyArgs -> IfThenElseOp)
        | 1 -> u_ModuleDefn st  |> (fun op tyargs -> 
                                        match op with 
                                        | StaticMethodCallOp(minfo) -> StaticMethodCallOp(instMeth(minfo,tyargs))
                                        // OK to throw away the tyargs here since this only non-generic values in modules get represented by static properties
                                        | op -> op)
        | 2 -> u_void            st |> (fun () NoTyArgs -> LetRecOp)
        | 3 -> u_NamedType        st |> (fun x tyargs -> NewRecordOp (mkNamedType(x,tyargs)))
        | 4 -> u_RecdField       st |> (fun prop tyargs -> InstancePropGetOp(prop tyargs))
        | 5 -> u_UnionCaseInfo   st |> (fun ucinfo tyargs -> NewUnionCaseOp(ucinfo tyargs))
        | 6 -> u_UnionCaseField  st |> (fun prop tyargs -> InstancePropGetOp(prop tyargs) )
        | 7 -> u_UnionCaseInfo   st |> (fun ucinfo tyargs -> UnionCaseTestOp(ucinfo tyargs))
        | 8 -> u_void          st |> (fun () (OneTyArg(tyarg)) -> NewTupleOp tyarg)
        | 9 -> u_int           st |> (fun x (OneTyArg(tyarg)) -> TupleGetOp (tyarg,x))
        | 11 -> u_bool         st |> (fun x NoTyArgs -> mkLiftedValueOpG x)
        | 12 -> u_string       st |> (fun x NoTyArgs -> mkLiftedValueOpG x)
        | 13 -> u_float32      st |> (fun x NoTyArgs -> mkLiftedValueOpG x)
        | 14 -> u_double       st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 15 -> u_char         st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 16 -> u_sbyte        st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 17 -> u_byte         st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 18 -> u_int16        st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 19 -> u_uint16       st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 20 -> u_int32        st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 21 -> u_uint32       st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 22 -> u_int64        st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 23 -> u_uint64       st |> (fun a NoTyArgs -> mkLiftedValueOpG a)
        | 24 -> u_void         st |> (fun a NoTyArgs -> mkLiftedValueOpG ())
        | 25 -> u_PropInfoData st |> (fun (a,b,c,d) tyargs -> let pinfo = bindProp(a,b,c,d,tyargs) in if pinfoIsStatic pinfo then StaticPropGetOp(pinfo) else InstancePropGetOp(pinfo))
        | 26 -> u_CtorInfoData st |> (fun (a,b) tyargs  -> NewObjectOp (bindCtor(a,b,tyargs)))
        | 28 -> u_void         st |> (fun a (OneTyArg(ty)) -> CoerceOp ty)
        | 29 -> u_void         st |> (fun a NoTyArgs -> SequentialOp)
        | 30 -> u_void         st |> (fun a NoTyArgs -> ForIntegerRangeLoopOp)
        | 31 -> u_MethodInfoData st |> (fun p tyargs -> let minfo = bindMeth(p,tyargs) in if minfo.IsStatic then StaticMethodCallOp(minfo) else InstanceMethodCallOp(minfo))
        | 32 -> u_void           st |> (fun a (OneTyArg(ty)) -> NewArrayOp ty)
        | 33 -> u_void           st |> (fun a (OneTyArg(ty)) -> NewDelegateOp ty)
        | 34 -> u_void           st |> (fun a NoTyArgs -> WhileLoopOp)
        | 35 -> u_void           st |> (fun () NoTyArgs -> LetOp)
        | 36 -> u_RecdField      st |> (fun prop tyargs -> InstancePropSetOp(prop tyargs))
        | 37 -> u_tup2 u_NamedType u_string st |> (fun (a,b) tyargs -> let finfo = bindField(a,b,tyargs) in if finfo.IsStatic then StaticFieldGetOp(finfo) else InstanceFieldGetOp(finfo))
        | 38 -> u_void           st |> (fun () NoTyArgs -> LetRecCombOp)
        | 39 -> u_void           st |> (fun () NoTyArgs -> AppOp)
        | 40 -> u_void           st |> (fun () (OneTyArg(ty)) -> ValueOp(null,ty))
        | 41 -> u_void           st |> (fun () (OneTyArg(ty)) -> DefaultValueOp(ty))
        | 42 -> u_PropInfoData   st |> (fun (a,b,c,d) tyargs -> let pinfo = bindProp(a,b,c,d,tyargs) in if pinfoIsStatic pinfo then StaticPropSetOp(pinfo) else InstancePropSetOp(pinfo))
        | 43 -> u_tup2 u_NamedType u_string st |> (fun (a,b) tyargs -> let finfo = bindField(a,b,tyargs) in if finfo.IsStatic then StaticFieldSetOp(finfo) else InstanceFieldSetOp(finfo))
        | 44 -> u_void           st |> (fun () NoTyArgs -> AddressOfOp)
        | 45 -> u_void           st |> (fun () NoTyArgs -> AddressSetOp)
        | 46 -> u_void           st |> (fun () (OneTyArg(ty)) -> TypeTestOp(ty))
        | 47 -> u_void           st |> (fun () NoTyArgs -> TryFinallyOp)
        | 48 -> u_void           st |> (fun () NoTyArgs -> TryWithOp)
        | _ -> failwithf "u_constSpec, unrecognized tag %d" tag
    let unpickle_raw_expr (localType : System.Type) = unpickle_obj localType.Assembly u_Expr 
    let u_defn = u_tup2 u_MethodBase u_Expr
    let u_defns = u_list u_defn
    let unpickleDefns (localAssembly : System.Reflection.Assembly) = unpickle_obj localAssembly u_defns

    //--------------------------------------------------------------------------
    // General utilities that will eventually be folded into 
    // Microsoft.FSharp.Quotations.Typed
    //--------------------------------------------------------------------------
    
    /// Fill the holes in an Expr 
    let rec fillHolesInRawExpr (l:Expr[]) (E t as e) = 
        match t with 
        | VarTerm _ -> e
        | LambdaTerm (v,b) -> EA(LambdaTerm(v, fillHolesInRawExpr l b ),e.CustomAttributes)
        | CombTerm   (op,args) -> EA(CombTerm(op, args |> List.map (fillHolesInRawExpr l)),e.CustomAttributes)
        | HoleTerm   (ty,idx) ->  
           if idx < 0 or idx >= l.Length then failwith "hole index out of range";
           let h = l.[idx]
           checkTypes (typeOf h) ty "fill" "type of the argument doesn't match the hole type"
           h

    let rec freeInExprAcc bvs acc (E t) = 
        match t with 
        | HoleTerm   _  -> acc
        | CombTerm (_, ag) -> ag |> List.fold (freeInExprAcc bvs) acc
        | VarTerm    v -> if Set.contains v bvs || Set.contains v acc then acc else Set.add v acc
        | LambdaTerm (v,b) -> freeInExprAcc (Set.add v bvs) acc b
    and freeInExpr e = freeInExprAcc Set.Empty Set.Empty e

    // utility for folding
    let foldWhile f st (ie: seq<'T>)  = 
        use e = ie.GetEnumerator()
        let mutable res = Some st
        while (res.IsSome && e.MoveNext()) do
            res <-  f (match res with Some a -> a | _ -> failwith "internal error") e.Current;
        res      
    
    let mkTyparSubst (tyargs:Type[]) =
        let n = tyargs.Length 
        fun idx -> 
          if idx < n then tyargs.[idx]
          else failwith "type argument out of range"

    exception Clash of Var

    /// Replace type variables and expression variables with parameters using the
    /// given substitution functions/maps.  
    let rec substituteB bvs tmsubst (E t as e) = 
        match t with 
        | CombTerm (c, args) -> 
            let substargs = args |> List.map (fun arg -> substituteB bvs tmsubst arg) 
            EA(CombTerm(c, substargs),e.CustomAttributes)
        | VarTerm    v -> 
            match tmsubst v with 
            | None -> e 
            | Some e2 -> 
                let fvs = freeInExpr e2 
                let clashes = Set.intersect fvs bvs in
                if clashes.IsEmpty then e2
                else raise (Clash(clashes.MinimumElement)) 
        | LambdaTerm (v,b) -> 
             try EA(LambdaTerm(v,substituteB (Set.add v bvs) tmsubst b),e.CustomAttributes)
             with Clash(bv) ->
                 if v = bv then
                     let v2 = new Var(v.Name,v.Type)
                     let v2exp = E(VarTerm(v2))
                     EA(LambdaTerm(v2,substituteB bvs (fun v -> if v = bv then Some(v2exp) else tmsubst v) b),e.CustomAttributes)
                 else
                     rethrow()
        | HoleTerm _ -> e


    let substituteRaw tmsubst e = substituteB Set.Empty tmsubst e 

    let readToEnd (s : Stream) = 
        let n = int s.Length 
        let res = Array.zeroCreate n 
        let i = ref 0 
        while (!i < n) do 
          i := !i + s.Read(res,!i,(n - !i)) 
        done;
        res 

    let decodedTopResources = new Dictionary<Assembly * string, int>(10,HashIdentity.Structural)

#if FX_NO_REFLECTION_MODULE_HANDLES // not available on Silverlight
    type ModuleHandle = string
    type System.Reflection.Module with 
        member x.ModuleHandle = x.FullyQualifiedName
#else
    type ModuleHandle = System.ModuleHandle
#endif
   
#if FX_NO_REFLECTION_METADATA_TOKENS // not available on Compact Framework
    type ReflectedDefinitionTableKey = 
        | Key of Type * int * Type[]
        static member GetKey(mbase:MethodBase) = 
            Key(mbase.DeclaringType.Module.ModuleHandle,
                (if mbase.IsGenericMethod then mbase.GetGenericArguments().Length else 0), 
                mbase.GetParameters() |> Array.map (fun p -> p.Type))
#else
    type ReflectedDefinitionTableKey = 
        | Key of ModuleHandle * int
        static member GetKey(mbase:MethodBase) = 
            Key(mbase.Module.ModuleHandle,mbase.MetadataToken)
#endif

    type ReflectedDefinitionTableEntry = Entry of Bindable<Expr>

    let reflectedDefinitionTable = new Dictionary<ReflectedDefinitionTableKey,ReflectedDefinitionTableEntry>(10,HashIdentity.Structural)

    let registerReflectedDefinitions (assem : Assembly,rn,bytes:byte[]) =
        let defns = unpickleDefns assem  bytes 
        defns |> List.iter (fun (minfo,e) -> 
            //printfn "minfo = %A, handle = %A, token = %A" minfo minfo.Module.ModuleHandle minfo.MetadataToken
            let key = ReflectedDefinitionTableKey.GetKey minfo
            lock reflectedDefinitionTable (fun () -> 
                //printfn "Adding %A, hc = %d" key (key.GetHashCode());
                reflectedDefinitionTable.Add(key,Entry(e))));
        //System.Console.WriteLine("Added {0} resource {1}", assem.FullName, rn);
        decodedTopResources.Add((assem,rn),0)

    let resolveMethodBase (mbase: MethodBase,tyargs: Type []) =
        let data = 
            let assem = mbase.DeclaringType.Assembly
            let key = ReflectedDefinitionTableKey.GetKey mbase
            //printfn "Looking for %A, hc = %d, hc2 = %d" key (key.GetHashCode()) (assem.GetHashCode());
            let ok,res = lock reflectedDefinitionTable (fun () -> reflectedDefinitionTable.TryGetValue(key))
            if ok then Some(res) else
            //System.Console.WriteLine("Loading {0}", td.Assembly);
            let qdataResources = 
                // dynamic assemblies don't support the GetManifestResourceNames 
                match assem with 
                | :? System.Reflection.Emit.AssemblyBuilder -> []
                | _ -> 
                    (try assem.GetManifestResourceNames()  
                     // This raises NotSupportedException for dynamic assemblies
                     with :? NotSupportedException -> [| |])
                    |> Array.to_list 
                    |> List.filter (fun rn -> 
                          //System.Console.WriteLine("Considering resource {0}", rn);
                          rn.StartsWith(ReflectedDefinitionsResourceNameBase,StringComparison.Ordinal) &&
                          not (decodedTopResources.ContainsKey((assem,rn)))) 
                    |> List.map (fun rn -> rn,unpickleDefns assem (readToEnd (assem.GetManifestResourceStream(rn)))) 
                
            // ok, add to the table
            let ok,res = 
                lock reflectedDefinitionTable (fun () -> 
                     // check another thread didn't get in first
                     if not (reflectedDefinitionTable.ContainsKey(key)) then
                         qdataResources 
                         |> List.iter (fun (rn,defns) ->
                             defns |> List.iter (fun (mbase,e) -> 
                                reflectedDefinitionTable.[ReflectedDefinitionTableKey.GetKey mbase] <- Entry(e));
                             decodedTopResources.Add((assem,rn),0))
                     // we know it's in the table now, if it's ever going to be there
                     reflectedDefinitionTable.TryGetValue(key) 
                );

            if ok then Some(res) else None

        match data with 
        | Some (Entry(exprBuilder)) -> 
            let expectedNumTypars = 
                getNumGenericArguments(mbase.DeclaringType) + 
                (match mbase with 
                 | :? MethodInfo as minfo -> if minfo.IsGenericMethod then minfo.GetGenericArguments().Length else 0
                 | _ -> 0)
            if (expectedNumTypars <> tyargs.Length) then 
                invalidArg "tyargs" (sprintf "the method '%A' expects %d type arguments but %d were provided" mbase expectedNumTypars tyargs.Length);
            Some(exprBuilder {typeInst = mkTyparSubst tyargs; vars=Map.Empty; varn=0})
        | None -> None

    let resolveMethodBaseInstantiated (mbase:MethodBase) = 
        match mbase with 
        | :? MethodInfo as minfo -> 
               let tyargs = 
                   Array.append
                       (getGenericArguments(minfo.DeclaringType))
                       (if minfo.IsGenericMethod then minfo.GetGenericArguments() else [| |])
               resolveMethodBase(mbase,tyargs)
        | :? ConstructorInfo as cinfo -> 
               let tyargs = getGenericArguments(cinfo.DeclaringType)
               resolveMethodBase(mbase,tyargs)
        | _ -> 
               resolveMethodBase(mbase,[| |])

    let deserialize(localAssembly,types,splices,bytes) : Expr = 
        let expr = unpickle_raw_expr localAssembly bytes (envClosed  (Array.of_list types))
        fillHolesInRawExpr (Array.of_list splices) expr
        
  
    let cast (expr: Expr) : Expr<'T> = 
        checkTypes  (typeof<'T>) (typeOf expr)  "expr" "the expression has the wrong type"  
        new Expr<'T>(expr.Tree,expr.CustomAttributes)

open Patterns


type Expr with 
    member x.Substitute f = substituteRaw f x
    member x.GetFreeVars ()  = (freeInExpr x :> seq<_>)
    member x.Type = typeOf x 

    static member AddressOf(e:Expr) = mkAddressOf(e)    
    static member AddressSet(e1:Expr,e2:Expr) = mkAddressSet(e1,e2)
    static member Application(e1:Expr,e2:Expr) = mkApplication(e1,e2)
    static member Applications(f:Expr,es) = mkApplications(f,es)
    static member Call(meth:MethodInfo,args) = mkStaticMethodCall(meth,args)
    static member Call(obj:Expr,meth:MethodInfo,args) = mkInstanceMethodCall(obj,meth,args)
    static member Coerce(e:Expr,ty:Type) = mkCoerce(ty,e)
    static member IfThenElse(g:Expr,t:Expr,e:Expr) = mkIfThenElse(g,t,e)
    static member ForIntegerRangeLoop(v,start:Expr,finish:Expr,body:Expr) = mkForLoop(v,start,finish,body)
    //static member Range: Expr * Expr -> Expr 
    //static member RangeStep: Expr * Expr * Expr -> Expr 
    static member FieldGet(finfo:FieldInfo) = mkStaticFieldGet(finfo)
    static member FieldGet(obj:Expr,finfo:FieldInfo) = mkInstanceFieldGet(obj,finfo)
    static member FieldSet(finfo:FieldInfo,v:Expr) = mkStaticFieldSet(finfo,v)
    static member FieldSet(obj:Expr,finfo:FieldInfo,v:Expr) = mkInstanceFieldSet(obj,finfo,v)
    static member Lambda(v:Var,e:Expr) = mkLambda(v,e)
    //static member Lambdas(vs,e:Expr) = mkLambdas(vs,e)
    static member Let(v:Var,e:Expr,b:Expr) = mkLet(v,e,b)
    static member LetRec(binds,e:Expr) = mkLetRec(binds,e)
    static member NewObject(cinfo:ConstructorInfo,args) = mkCtorCall(cinfo,args)
    static member DefaultValue(ty:Type) = mkDefaultValue(ty)
    static member NewTuple(es) = mkNewTuple(es)
    static member NewRecord(ty:Type,args) = mkNewRecord(ty,args)
    static member NewArray(ty:Type,es) = mkNewArray(ty,es)
    static member NewDelegate(ty:Type,vs : Var list,body: Expr) = mkNewDelegate(ty,mkIteratedLambdas(vs, body))
    static member NewUnionCase(uc,es) = mkNewUnionCase(uc,es)
    static member PropGet(obj:Expr,pinfo:PropertyInfo,?args) = mkInstancePropGet(obj,pinfo,(defaultArg args []))
    static member PropGet(pinfo:PropertyInfo,?args) = mkStaticPropGet(pinfo,(defaultArg args []))
    static member PropSet(obj:Expr,pinfo:PropertyInfo,value:Expr,?args) = mkInstancePropSet(obj,pinfo,(defaultArg args []),value)
    static member PropSet(pinfo:PropertyInfo,value:Expr,?args) = mkStaticPropSet(pinfo,(defaultArg args []),value)
    static member Quote(expr:Expr) = mkQuote(expr)
    static member Sequential(e1:Expr,e2:Expr) = mkSequential(e1,e2)
    static member TryWith(e1:Expr,v2:Var,e2:Expr,v3:Var,e3:Expr) = mkTryWith(e1,v2,e2,v3,e3)
    static member TryFinally(e1:Expr,e2:Expr) = mkTryFinally(e1,e2)
    static member TupleGet(e:Expr,n) = mkTupleGet(typeOf(e),n,e)
    static member TypeTest(e:Expr,ty:Type) = mkTypeTest(e,ty)
    static member UnionCaseTest(e:Expr,uci) = mkUnionCaseTest(uci,e)
    static member Value(v:'T) = mkValue(box v, typeof<'T>)
    static member Value(obj:obj,typ:Type) = mkValue(obj, typ)
    static member Var(v) = mkVar(v)
    static member VarSet(v,e:Expr) = mkVarSet(v,e)
    static member WhileLoop(e1:Expr,e2:Expr) = mkWhileLoop(e1,e2)
    //static member IsInlinedMethodInfo(minfo:MethodInfo) = false
    static member TryGetReflectedDefinition(mbase:MethodBase) = resolveMethodBaseInstantiated(mbase)
    static member Cast(expr:Expr) = cast expr
    static member Deserialize(qualifyingType:Type,spliceTypes,spliceExprs,bytes) = deserialize(qualifyingType,spliceTypes,spliceExprs,bytes)
    static member RegisterReflectedDefinitions(assembly:Assembly,nm,bytes) = registerReflectedDefinitions(assembly,nm,bytes)
    static member GlobalVar<'T>(nm) : Expr<'T> = Expr.Var(Var.Global(nm,typeof<'T>)) |> Expr.Cast

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DerivedPatterns =
    open Patterns

    let (|Bool|_|)          = function ValueObj(:? bool   as v) -> Some(v) | _ -> None
    let (|String|_|)        = function ValueObj(:? string as v) -> Some(v) | _ -> None
    let (|Single|_|)        = function ValueObj(:? single as v) -> Some(v) | _ -> None
    let (|Double|_|)        = function ValueObj(:? double as v) -> Some(v) | _ -> None
    let (|Char|_|)          = function ValueObj(:? char   as v) -> Some(v) | _ -> None
    let (|SByte|_|)         = function ValueObj(:? sbyte  as v) -> Some(v) | _ -> None
    let (|Byte|_|)          = function ValueObj(:? byte   as v) -> Some(v) | _ -> None
    let (|Int16|_|)         = function ValueObj(:? int16  as v) -> Some(v) | _ -> None
    let (|UInt16|_|)        = function ValueObj(:? uint16 as v) -> Some(v) | _ -> None
    let (|Int32|_|)         = function ValueObj(:? int32  as v) -> Some(v) | _ -> None
    let (|UInt32|_|)        = function ValueObj(:? uint32 as v) -> Some(v) | _ -> None
    let (|Int64|_|)         = function ValueObj(:? int64  as v) -> Some(v) | _ -> None
    let (|UInt64|_|)        = function ValueObj(:? uint64 as v) -> Some(v) | _ -> None
    let (|Unit|_|)          = function Comb0(ValueOp(_,ty)) when ty = typeof<unit> -> Some() | _ -> None

    /// (fun (x,y) -> z) is represented as 'fun p -> let x = p#0 let y = p#1' etc.
    /// This reverses this encoding.
    let (|TupledLambda|_|) (lam: Expr) =
        /// Strip off the 'let' bindings for an TupledLambda
        let rec stripSuccessiveProjLets (p:Var) n expr =
            match expr with 
            | Let(v1,TupleGet(Var(pA),m),rest) 
                  when p = pA && m = n-> 
                      let restvs,b = stripSuccessiveProjLets p (n+1) rest
                      v1::restvs, b
            | _ -> ([],expr)
        match lam.Tree with 
        | LambdaTerm(v,body) ->
              match stripSuccessiveProjLets v 0 body with 
              | [],b -> Some([v], b)
              | letvs,b -> Some(letvs,b)
        | _ -> None

    let (|TupledApplication|_|) e = 
        match e with 
        | Application(f,x) -> 
            match x with 
            | Unit -> Some(f,[])
            | NewTuple(x) -> Some(f,x)
            | x -> Some(f,[x])
        | _ -> None
            
    let (|Lambdas|_|) (e: Expr) = qOneOrMoreRLinear (|TupledLambda|_|) e 
    let (|Applications|_|) (e: Expr) = qOneOrMoreLLinear (|TupledApplication|_|) e 
    /// Reverse the compilation of And and Or
    let (|AndAlso|_|) x = 
        match x with 
        | IfThenElse(x,y,Bool(false)) -> Some(x,y)
        | _ -> None
        
    let (|OrElse|_|) x = 
        match x with 
        | IfThenElse(x,Bool(true),y) -> Some(x,y)
        | _ -> None

    let (|SpecificCall|_|) templateParameter = 
        // Note: precomputation
        match templateParameter with
        | (Lambdas(_,Call(_,minfo1,_)) | Call(_,minfo1,_)) ->
            let isg1 = minfo1.IsGenericMethod 
            let gmd = if isg1 then minfo1.GetGenericMethodDefinition() else null

            // end-of-precomputation

            (fun tm -> 
               match tm with
               | Call(obj,minfo2,args) 
                  when (minfo1.MetadataToken = minfo2.MetadataToken &&
                        if isg1 then 
                          minfo2.IsGenericMethod && gmd = minfo2.GetGenericMethodDefinition()
                        else
                          minfo1 = minfo2) -> 
                   Some((minfo2.GetGenericArguments() |> Array.to_list),args)
               | _ -> None)
        | _ -> 
            invalidArg "templateParameter" "the parameter is not a recognized method name"
               

    let (|MethodWithReflectedDefinition|_|) (minfo) = 
        Expr.TryGetReflectedDefinition(minfo)
    
    let (|PropertyGetterWithReflectedDefinition|_|) (pinfo:System.Reflection.PropertyInfo) = 
        Expr.TryGetReflectedDefinition(pinfo.GetGetMethod())

    let (|PropertySetterWithReflectedDefinition|_|) (pinfo:System.Reflection.PropertyInfo) = 
        Expr.TryGetReflectedDefinition(pinfo.GetSetMethod())

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ExprShape =
    open Patterns
    let RebuildShapeCombination(a:obj,args) =  
        // preserve the attributes
        let op,attrs = unbox<ExprConstInfo * Expr list>(a)
        let e = 
            match op,args with 
            | AppOp,[f;x]        -> mkApplication(f,x)
            | IfThenElseOp,[g;t;e]     -> mkIfThenElse(g,t,e)
            | LetRecOp,[e1]   -> mkLetRecRaw(e1)     
            | LetRecCombOp,_     -> mkLetRecCombRaw(args) 
            | LetOp,[e1;e2]      -> mkLetRaw(e1,e2)      
            | NewRecordOp(ty),_     -> mkNewRecord(ty, args)
            | NewUnionCaseOp(ucinfo),_    -> mkNewUnionCase(ucinfo, args)
            | UnionCaseTestOp(ucinfo),[arg]  -> mkUnionCaseTest(ucinfo,arg)
            | NewTupleOp(ty),_    -> mkNewTupleWithType(ty, args)
            | TupleGetOp(ty,i),[arg] -> mkTupleGet(ty,i,arg)
            | InstancePropGetOp(pinfo),(obj::args)    -> mkInstancePropGet(obj,pinfo,args)
            | StaticPropGetOp(pinfo),[] -> mkStaticPropGet(pinfo,args)
            | InstancePropSetOp(pinfo),obj::v::args -> mkInstancePropSet(obj,pinfo,args,v)
            | StaticPropSetOp(pinfo),v::args -> mkStaticPropSet(pinfo,args,v)
            | InstanceFieldGetOp(finfo),[obj]   -> mkInstanceFieldGet(obj,finfo)
            | StaticFieldGetOp(finfo),[]   -> mkStaticFieldGet(finfo )
            | InstanceFieldSetOp(finfo),[obj;v]   -> mkInstanceFieldSet(obj,finfo,v)
            | StaticFieldSetOp(finfo),[v]   -> mkStaticFieldSet(finfo,v)
            | NewObjectOp minfo,_   -> mkCtorCall(minfo,args)
            | DefaultValueOp(ty),_  -> mkDefaultValue(ty)
            | StaticMethodCallOp(minfo),_ -> mkStaticMethodCall(minfo,args)
            | InstanceMethodCallOp(minfo),obj::args -> mkInstanceMethodCall(obj,minfo,args)
            | CoerceOp(ty),[arg]   -> mkCoerce(ty,arg)
            | NewArrayOp(ty),_    -> mkNewArray(ty,args)
            | NewDelegateOp(ty),[arg]   -> mkNewDelegate(ty,arg)
            | SequentialOp,[e1;e2]     -> mkSequential(e1,e2)
            | TypeTestOp(ty),[e1]     -> mkTypeTest(e1,ty)
            | AddressOfOp,[e1]     -> mkAddressOf(e1)
            | VarSetOp,[E(VarTerm(v)); e]     -> mkVarSet(v,e)
            | AddressSetOp,[e1;e2]     -> mkAddressSet(e1,e2)
            | ForIntegerRangeLoopOp,[e1;e2;E(LambdaTerm(v,e3))]     -> mkForLoop(v,e1,e2,e3)
            | WhileLoopOp,[e1;e2]     -> mkWhileLoop(e1,e2)
            | ValueOp(v,ty),[]  -> mkValue(v,ty)
            | _ -> 
                //assert(false)
                failwith "Unexpected error in mkConstApp"
        EA(e.Tree,attrs)

    let rec (|ShapeVar|ShapeLambda|ShapeCombination|) e = 
        let rec loop expr = 
            let (E(t)) = expr 
            match t with 
            | VarTerm v       -> ShapeVar(v)
            | LambdaTerm(v,b) -> ShapeLambda(v,b)
            | CombTerm(op,args) -> ShapeCombination(box<ExprConstInfo * Expr list> (op,expr.CustomAttributes),args)
            | HoleTerm _     -> invalidArg "expr" "Unexpected hole in expression"
        loop (e :> Expr)
#endif
