//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//
// Reflection on F# values. Analyze an object to see if it the representation
// of an F# value.
//=========================================================================

namespace Microsoft.FSharp.Reflection 

open System
open System.Globalization
open System.Reflection
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.Operators
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Primitives.Basics

module internal Impl =

    let debug = false

    let inline checkNonNull argName (v: 'T) = 
        match box v with 
        | null -> nullArg argName 
        | _ -> ()
        
    let emptyArray arr = (Array.length arr = 0)
    let nonEmptyArray arr = Array.length arr > 0

    let isNamedType(typ:Type) = not (typ.IsArray || typ.IsByRef || typ.IsPointer)

    let equivHeadTypes (ty1:Type) (ty2:Type) = 
        isNamedType(ty1) &&
        if ty1.IsGenericType then 
          ty2.IsGenericType && (ty1.GetGenericTypeDefinition()).Equals(ty2.GetGenericTypeDefinition())
        else 
          ty1.Equals(ty2)

    let option = typedefof<obj option>
    let func = typedefof<(obj -> obj)>

    let isOptionType typ = equivHeadTypes typ (typeof<int option>)
    let isFunctionType typ = equivHeadTypes typ (typeof<(int -> int)>)
    let isUnitType typ = equivHeadTypes typ (typeof<unit>)
    let isListType typ = equivHeadTypes typ (typeof<int list>)

    let tuple1 = typedefof<Tuple<obj>>
    let tuple2 = typedefof<obj * obj>
    let tuple3 = typedefof<obj * obj * obj>
    let tuple4 = typedefof<obj * obj * obj * obj>
    let tuple5 = typedefof<obj * obj * obj * obj * obj>
    let tuple6 = typedefof<obj * obj * obj * obj * obj * obj>
    let tuple7 = typedefof<obj * obj * obj * obj * obj * obj * obj>
    let tuple8 = typedefof<obj * obj * obj * obj * obj * obj * obj * obj>

    let isTuple1Type typ = equivHeadTypes typ tuple1
    let isTuple2Type typ = equivHeadTypes typ tuple2
    let isTuple3Type typ = equivHeadTypes typ tuple3
    let isTuple4Type typ = equivHeadTypes typ tuple4
    let isTuple5Type typ = equivHeadTypes typ tuple5
    let isTuple6Type typ = equivHeadTypes typ tuple6
    let isTuple7Type typ = equivHeadTypes typ tuple7
    let isTuple8Type typ = equivHeadTypes typ tuple8

    let isTupleType typ = 
           isTuple1Type typ
        || isTuple2Type typ 
        || isTuple3Type typ 
        || isTuple4Type typ 
        || isTuple5Type typ 
        || isTuple6Type typ 
        || isTuple7Type typ 
        || isTuple8Type typ

    let maxTuple = 8
    // Which field holds the nested tuple?
    let tupleEncField = maxTuple-1
    
    let rec mkTupleType (tys: Type[]) = 
        match tys.Length with 
        | 1 -> tuple1.MakeGenericType(tys)
        | 2 -> tuple2.MakeGenericType(tys)
        | 3 -> tuple3.MakeGenericType(tys)
        | 4 -> tuple4.MakeGenericType(tys)
        | 5 -> tuple5.MakeGenericType(tys)
        | 6 -> tuple6.MakeGenericType(tys)
        | 7 -> tuple7.MakeGenericType(tys)
        | n when n >= maxTuple -> 
            let tysA = tys.[0..tupleEncField-1]
            let tysB = tys.[maxTuple-1..]
            let tyB = mkTupleType tysB
            tuple8.MakeGenericType(Array.append tysA [| tyB |])
        | _ -> invalidArg "tys" "this is not a valid tuple type for the F# reflection library"

    let instancePropertyFlags = BindingFlags.GetProperty ||| BindingFlags.Instance 
    let staticGetFieldFlags = BindingFlags.GetField ||| BindingFlags.Static 
    let instanceBindingFlags = BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.DeclaredOnly

    let getPropertyInfo (typ: Type,propName,bindingFlags) = typ.GetProperty(propName,instancePropertyFlags ||| bindingFlags) 
    let getPropertyInfos (typ,names,bindingFlags) = names |> Array.map (fun nm -> getPropertyInfo (typ,nm,bindingFlags)) 

    let tryFindMemberCompilationMappingAttribute (info: MemberInfo) = 
      match info.GetCustomAttributes (typeof<CompilationMappingAttribute>, false) with
      | null | [| |] -> None
      | res -> Some (res.[0] :?> CompilationMappingAttribute) 

    let findMemberCompilationMappingAttribute (info: MemberInfo) = 
      match info.GetCustomAttributes (typeof<CompilationMappingAttribute>, false) with
      | null | [| |] -> failwith "the member did not have a compilation mapping attribute"
      | res -> (res.[0] :?> CompilationMappingAttribute) 

    let sequenceNumberOfMember          (x: MemberInfo) = (findMemberCompilationMappingAttribute x).SequenceNumber
    let variantNumberOfMember           (x: MemberInfo) = (findMemberCompilationMappingAttribute x).VariantNumber

    let sortFreshArray f arr = Array.sortInPlaceWith f arr; arr

    let isFieldProperty (prop : PropertyInfo) =
        match tryFindMemberCompilationMappingAttribute(prop) with
        | None -> false
        | Some attr -> (attr.SourceConstructFlags &&& SourceConstructFlags.KindMask) = SourceConstructFlags.Field

    let fieldsPropsOfUnionCase((typ:Type), (tag:int),bindingFlags) =
        if isOptionType typ then 
            match tag with 
            | 0 (* None *) -> getPropertyInfos (typ,[| |],bindingFlags)
            | 1 (* Some *) -> getPropertyInfos (typ,[| "Value" |] ,bindingFlags)
            | _ -> failwith "fieldsPropsOfUnionCase"
        elif isListType typ then 
            match tag with 
            | 0 (* Nil *)  -> getPropertyInfos (typ,[| |],bindingFlags)
            | 1 (* Cons *) -> getPropertyInfos (typ,[| "Head"; "Tail" |],bindingFlags)
            | _ -> failwith "fieldsPropsOfUnionCase"
        else
            typ.GetProperties(instancePropertyFlags ||| bindingFlags) 
            |> Array.filter isFieldProperty
            |> Array.filter (fun prop -> variantNumberOfMember prop = tag)
            |> sortFreshArray (fun p1 p2 -> compare (sequenceNumberOfMember p1) (sequenceNumberOfMember p2))
                
    let emptyObjArray : obj[] = [| |]

    let callStaticMethod (typ:Type,name,args,bindingFlags) =
#if FX_NO_CULTURE_INFO_ARGS
        typ.InvokeMember(name, BindingFlags.Static  ||| BindingFlags.InvokeMethod  ||| bindingFlags, null, null,args)
#else
        typ.InvokeMember(name, BindingFlags.Static  ||| BindingFlags.InvokeMethod  ||| bindingFlags, null, null,args,CultureInfo.InvariantCulture(*FxCop:1304*))
#endif

    let tryFindCompilationMappingAttribute (typ:Type) =
      match typ.GetCustomAttributes (typeof<CompilationMappingAttribute>, false) with 
      | null | [| |] -> None
      | res -> Some(res.[0] :?> CompilationMappingAttribute)

    let tryFindSourceConstructFlagsOfType (typ:Type) = 
      match tryFindCompilationMappingAttribute typ with 
      | None -> None
      | Some attr -> Some attr.SourceConstructFlags 

    let rec getTupleTypeInfo    (typ:Type) = 
      if not (isTupleType (typ) ) then invalidArg "typ" "the type is not a tuple type";
      let tyargs = typ.GetGenericArguments()
      if tyargs.Length = maxTuple then 
          let tysA = tyargs.[0..tupleEncField-1]
          let tyB = tyargs.[tupleEncField]
          Array.append tysA (getTupleTypeInfo tyB)
      else 
          tyargs
      
      
    let getFunctionTypeInfo (typ:Type) =
      if not (isFunctionType typ) then invalidArg "typ" "not a function type"
      let tyargs = typ.GetGenericArguments()
      tyargs.[0], tyargs.[1]

    let isUnionType (typ:Type,bindingFlags:BindingFlags) = 
      isOptionType typ || 
      isListType typ || 
      match tryFindSourceConstructFlagsOfType(typ) with 
      | None -> false
      | Some(flags) ->
        (flags &&& SourceConstructFlags.KindMask) = SourceConstructFlags.SumType &&
        // We see private representations only if BindingFlags.NonPublic is set
        (if (flags &&& SourceConstructFlags.NonpublicRepresentation) <> enum(0) then 
            (bindingFlags &&& BindingFlags.NonPublic) <> enum(0)
         else 
            true)

    let isRecordType (typ:Type,bindingFlags:BindingFlags) = 
      match tryFindSourceConstructFlagsOfType(typ) with 
      | None -> false 
      | Some(flags) ->
        (flags &&& SourceConstructFlags.KindMask) = SourceConstructFlags.RecordType &&
        // We see private representations only if BindingFlags.NonPublic is set
        (if (flags &&& SourceConstructFlags.NonpublicRepresentation) <> enum(0) then 
            (bindingFlags &&& BindingFlags.NonPublic) <> enum(0)
         else 
            true) &&
        not (isTupleType typ)

    let isModuleType (typ:Type) = 
      match tryFindSourceConstructFlagsOfType(typ) with 
      | None -> false 
      | Some(flags) -> 
        (flags &&& SourceConstructFlags.KindMask) = SourceConstructFlags.Module 

    let fieldPropsOfRecordType(typ:Type,bindingFlags) =
      typ.GetProperties(instancePropertyFlags ||| bindingFlags) 
      |> Array.filter isFieldProperty
      |> sortFreshArray (fun p1 p2 -> compare (sequenceNumberOfMember p1) (sequenceNumberOfMember p2))

    let rec isClosureRepr typ = 
        isFunctionType typ || 
        (match typ.BaseType with null -> false | bty -> isClosureRepr bty) 

    let isFSharpObjectType typ =
      match tryFindSourceConstructFlagsOfType(typ) with 
      | None -> false 
      | Some(flags) -> 
        (flags &&& SourceConstructFlags.KindMask) = SourceConstructFlags.ObjectType 

    let discriminatorNameFromSumConstructor (m:MethodInfo) =
        let nm = m.Name
        if (m.GetParameters()).Length = 0 then
            if nm.Substring(0,4) = "get_" then nm.Substring(4) else nm
        else
            nm


    let getUnionTypeTagFields (typ:Type,bindingFlags) = 
        typ.GetFields(staticGetFieldFlags ||| bindingFlags) 
        |> Array.filter (fun (f:FieldInfo) -> f.Name.Length > 4 &&  f.Name.Substring(0,4) = "tag_") 
        
        |> sortFreshArray (fun f1 f2 -> compare (f1.GetValue(null) :?> int) (f1.GetValue(null) :?> int))

    // Check the base type - if it is also an F# type then
    // for the moment we know it is a Discriminated Union
    let isConstructorRepr (typ:Type,bindingFlags:BindingFlags) = 
        let rec get (typ:Type) = isUnionType (typ,bindingFlags) || match typ.BaseType with null -> false | b -> get b
        get typ 

    let unionTypeOfUnionCaseType (typ:Type,bindingFlags) = 
        let rec get (typ:Type) = if isUnionType (typ,bindingFlags) then typ else match typ.BaseType with null -> typ | b -> get b
        get typ 
           
    // Check the base type - if it is also an F# type then
    // for the moment we know it is a Discriminated Union
    let isExceptionRepr (typ:Type,bindingFlags) = 
        match tryFindSourceConstructFlagsOfType(typ) with 
        | None -> false 
        | Some(flags) -> 
          ((flags &&& SourceConstructFlags.KindMask) = SourceConstructFlags.Exception) &&
          // We see private representations only if BindingFlags.NonPublic is set
          (if (flags &&& SourceConstructFlags.NonpublicRepresentation) <> enum(0) then 
              (bindingFlags &&& BindingFlags.NonPublic) <> enum(0)
           else 
              true)

    let recdDescOfProps props = 
       props |> Array.to_list |> List.map (fun (p:PropertyInfo) -> p.Name, p.PropertyType) 

    let getRecd obj (props:PropertyInfo[]) = 
        props |> Array.map (fun prop -> prop.GetValue(obj,null))

    let getRecordReader(typ:Type,bindingFlags) = 
        let props = fieldPropsOfRecordType(typ,bindingFlags)
        (fun (obj:obj) -> props |> Array.map (fun prop -> prop.GetValue(obj,null)))

    let getRecordConstructorMethod(typ:Type,bindingFlags) = 
        let props = fieldPropsOfRecordType(typ,bindingFlags)
        let ctor = typ.GetConstructor(BindingFlags.Instance ||| bindingFlags,null,props |> Array.map (fun p -> p.PropertyType),null)
        checkNonNull "typ" ctor;
        ctor

    let getRecordConstructor(typ:Type,bindingFlags) = 
        let ctor = getRecordConstructorMethod(typ,bindingFlags)
        (fun (args:obj[]) -> 
            ctor.Invoke(BindingFlags.InvokeMethod  ||| BindingFlags.Instance ||| bindingFlags,null,args,null))
            
    let getTupleConstructorMethod(typ:Type,bindingFlags) =
#if FX_ATLEAST_40
          let props = typ.GetProperties()
#else
          let props = typ.GetProperties() |> Array.rev
#endif
          let ctor = typ.GetConstructor(BindingFlags.Instance ||| bindingFlags,null,props |> Array.map (fun p -> p.PropertyType),null)
          checkNonNull "typ" ctor;
          ctor
        
    let getTupleCtor(typ:Type,bindingFlags) =
          let ctor = getTupleConstructorMethod(typ,bindingFlags)
          (fun (args:obj[]) ->
              ctor.Invoke(BindingFlags.InvokeMethod ||| BindingFlags.Instance ||| bindingFlags,null,args,null))

    let getPropertyReader (typ: Type,propName,bindingFlags) =
        match getPropertyInfo(typ,propName,bindingFlags) with
        | null -> None
        | prop -> Some(fun (obj:obj) -> prop.GetValue(obj,instancePropertyFlags ||| bindingFlags,null,null,null))

    let getRecordFieldReader (typ,propName,bindingFlags) = 
        match getPropertyReader (typ,propName,bindingFlags) with 
        | None -> failwith "getRecordFieldReader"
        | Some reader -> reader

    let rec getTupleReader (typ:Type) = 
        let etys = typ.GetGenericArguments() 
        // Get the reader for the outer tuple record
#if FX_ATLEAST_40
        let props = typ.GetProperties(instancePropertyFlags ||| BindingFlags.Public)
#else
        let props = typ.GetProperties(instancePropertyFlags ||| BindingFlags.Public) |> Array.rev
#endif
        let reader = (fun (obj:obj) -> props |> Array.map (fun prop -> prop.GetValue(obj,null)))
        if etys.Length < maxTuple 
        then reader
        else
            let tyBenc = etys.[tupleEncField]
            let reader2 = getTupleReader(tyBenc)
            (fun obj ->
                let directVals = reader obj
                let encVals = reader2 directVals.[tupleEncField]
                Array.append directVals.[0..tupleEncField-1] encVals)
                
    let rec getTupleConstructor (typ:Type) = 
        let etys = typ.GetGenericArguments() 
        let maker1 =  getTupleCtor (typ,BindingFlags.Public)
        if etys.Length < maxTuple 
        then maker1
        else
            let tyBenc = etys.[tupleEncField]
            let maker2 = getTupleConstructor(tyBenc)
            (fun (args:obj[]) ->
                let encVal = maker2 args.[tupleEncField..]
                maker1 (Array.append args.[0..tupleEncField-1] [| encVal |]))
                
    let getTupleConstructorInfo (typ:Type) = 
        let etys = typ.GetGenericArguments() 
        let maker1 =  getTupleConstructorMethod (typ,BindingFlags.Public)
        if etys.Length < maxTuple then
            maker1,None
        else
            maker1,Some(etys.[tupleEncField])

    let getTupleReaderInfo (typ:Type,index:int) = 
        
        if index < 0 then invalidArg "index" "the tuple index was out of range"
#if FX_ATLEAST_40
        let props = typ.GetProperties(instancePropertyFlags ||| BindingFlags.Public)
#else
        let props = typ.GetProperties(instancePropertyFlags ||| BindingFlags.Public) |> Array.rev
#endif
        let get index = 
            if index >= props.Length then invalidArg "index" "the tuple index was out of range"
            props.[index]
        
        if index < tupleEncField then
            get index, None  
        else
            let etys = typ.GetGenericArguments()
            get tupleEncField, Some(etys.[tupleEncField],index-(maxTuple-1))
            
    let getUnionCaseRecordReader (typ:Type,tag:int,bindingFlags) = 
        let props = fieldsPropsOfUnionCase(typ,tag,bindingFlags)
        (fun (obj:obj) -> props |> Array.map (fun prop -> prop.GetValue(obj,bindingFlags,null,null,null)))

    let getUnionTagReader (typ:Type,bindingFlags) = 
        if isOptionType typ then (function null -> 0 | _ -> 1)
        else
          let tagreader = 
              match getPropertyReader (typ,"Tag",bindingFlags) with
              | Some reader -> reader
              | None -> (fun obj -> callStaticMethod(typ,"GetTag",[|obj|],bindingFlags))
          (fun obj -> (tagreader obj :?> int))
        
    let getUnionTagMemberInfo (typ:Type,bindingFlags) = 
        match getPropertyInfo (typ,"Tag",bindingFlags) with
        | null -> (typ.GetMethod("GetTag",BindingFlags.Static ||| bindingFlags) :> MemberInfo)
        | info -> (info :> MemberInfo)
        
    let getUnionTypeTagNameMap (typ:Type,bindingFlags) = 
        getUnionTypeTagFields(typ,bindingFlags) 
           |> Array.map (fun tagfield -> 
                  let constrname = tagfield.Name.Substring(4,tagfield.Name.Length - 4)
                  (tagfield.GetValue(null) :?> int),constrname)

    let getUnionTagConverter (typ:Type,bindingFlags) = 
        if isOptionType typ then (fun tag -> match tag with 0 -> "None" | 1 -> "Some" | _ -> invalidArg "tag" "tag out of range")
        elif isListType typ then (fun tag -> match tag with  0 -> "Empty" | 1 -> "Cons" | _ -> invalidArg "tag" "tag out of range")
        else 
          let tagfieldmap = getUnionTypeTagNameMap(typ,bindingFlags) |> Map.of_seq
          (fun tag -> tagfieldmap.[tag])

    let swap (x,y) = (y,x)

    let getUnionTagConverters (typ:Type,bindingFlags) = 
        let tagfields = getUnionTypeTagNameMap(typ,bindingFlags)
        let tagfieldmap1 = tagfields |> Map.of_seq
        let tagfieldmap2 = tagfields |> Array.map swap |> Map.of_seq
        tagfields.Length, (fun tag -> tagfieldmap1.[tag]), (fun tag -> tagfieldmap2.[tag])

    let getUnionCaseConstructorMethod (typ:Type,tag:int,bindingFlags) = 
        let props = fieldsPropsOfUnionCase(typ,tag,bindingFlags) 
        let constrname = getUnionTagConverter (typ,bindingFlags) tag 
        let methname = if emptyArray props then "get_"+constrname else constrname 
        match typ.GetMethod(methname, BindingFlags.Static  ||| bindingFlags) with
        | null -> failwith ("the constructor method '" + methname + "' for the union case could not be found")
        | meth -> meth

    let getUnionCaseConstructor (typ:Type,tag:int,bindingFlags) = 
        let meth = getUnionCaseConstructorMethod (typ,tag,bindingFlags)
        (fun args -> 
            meth.Invoke(null,BindingFlags.Static ||| BindingFlags.InvokeMethod ||| bindingFlags,null,args,null))

    let getTypeOfReprType (typ:Type,bindingFlags) = 
        if isExceptionRepr(typ,bindingFlags) then typ.BaseType
        elif isConstructorRepr(typ,bindingFlags) then unionTypeOfUnionCaseType(typ,bindingFlags)
        elif isClosureRepr(typ) then 
          let rec get (typ:Type) = if isFunctionType typ then typ else match typ.BaseType with null -> typ | b -> get b
          get typ 
        else typ


    let ensureType (typ:Type,obj:obj,bindingFlags) = 
            match typ with 
            | null -> 
                match obj with 
                | null -> invalidArg "obj" "the object is null and no type was given"
                | _ -> obj.GetType()
            | _ -> typ 


    let checkUnionType(unionType,bindingFlags) =
        checkNonNull "unionType" unionType;
        if not (isUnionType (unionType,bindingFlags)) then 
            if isUnionType (unionType,bindingFlags ||| BindingFlags.NonPublic) then 
                invalidArg "unionType" "The type is a union type but its representation is private. You must specify BindingFlags.NonPublic to access private type representations"
            else
                invalidArg "unionType" "The type is not a union type"

    let checkExnType(exceptionType,bindingFlags) =
        if not (isExceptionRepr (exceptionType,bindingFlags)) then 
            if isExceptionRepr (exceptionType,bindingFlags ||| BindingFlags.NonPublic) then 
                invalidArg "exceptionType" "The type is the representation of an F# exception declaration but its representation is private. You must specify BindingFlags.NonPublic to access private type representations"
            else
                invalidArg "exceptionType" "The type is not the representation of an F# exception declaration"
           

    // inline lets FxCop check that argument names match those given
    let checkRecordType(argName,recordType,bindingFlags) =
        checkNonNull "recordType" recordType;
        if not (isRecordType (recordType,bindingFlags) ) then 
            if isRecordType (recordType,bindingFlags ||| BindingFlags.NonPublic) then 
                invalidArg "recordType" "The type is a record type but its representation is private. You must specify BindingFlags.NonPublic to access private type representations"
            else
                invalidArg "recordType" "The type is not a record type"
        
    let checkTupleType(argName,tupleType) =
        checkNonNull argName tupleType;
        if not (isTupleType tupleType) then invalidArg argName "the type is not a tuple type"
        
[<Sealed>]
type UnionCaseInfo(typ: System.Type, tag:int) =
    // Cache the tag -> name map
    let mutable names = None
    member x.Name = 
        match names with 
        | None -> (let conv = Impl.getUnionTagConverter (typ,BindingFlags.Public ||| BindingFlags.NonPublic) in names <- Some(conv); conv tag)
        | Some(conv) -> conv tag
        
    member x.DeclaringType = typ
    //member x.CustomAttributes = failwith<obj[]> "nyi"
    member x.GetFields() = Impl.fieldsPropsOfUnionCase(typ,tag,BindingFlags.Public ||| BindingFlags.NonPublic) 

    member x.GetCustomAttributes() = 
        let methInfo = Impl.getUnionCaseConstructorMethod (typ,tag,BindingFlags.Public ||| BindingFlags.NonPublic) 
        methInfo.GetCustomAttributes(false)
    
    member x.GetCustomAttributes(attributeType) = 
        let methInfo = Impl.getUnionCaseConstructorMethod (typ,tag,BindingFlags.Public ||| BindingFlags.NonPublic) 
        methInfo.GetCustomAttributes(attributeType,false)

    member x.Tag = tag
    override x.ToString() = typ.Name + "." + x.Name
    override x.GetHashCode() = typ.GetHashCode() + tag
    override x.Equals(obj:obj) = 
        match obj with 
        | :? UnionCaseInfo as uci -> uci.DeclaringType = typ && uci.Tag = tag
        | _ -> false
    

[<AbstractClass; Sealed>]
type FSharpType = 

    static member IsTuple(typ:Type) =  
        Impl.checkNonNull "typ" typ;
        Impl.isTupleType typ

    static member IsRecord(typ:Type,?bindingFlags) =  
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkNonNull "typ" typ;
        Impl.isRecordType (typ,bindingFlags)

    static member IsUnion(typ:Type,?bindingFlags) =  
        Impl.checkNonNull "typ" typ;
        let typ = Impl.getTypeOfReprType (typ ,BindingFlags.Public ||| BindingFlags.NonPublic)
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.isUnionType (typ,bindingFlags)

    static member IsFunction(typ:Type) =  
        Impl.checkNonNull "typ" typ;
        let typ = Impl.getTypeOfReprType (typ ,BindingFlags.Public ||| BindingFlags.NonPublic)
        Impl.isFunctionType typ

    static member IsModule(typ:Type) =  
        Impl.checkNonNull "typ" typ;
        Impl.isModuleType typ

    static member MakeFunctionType(domain:Type,range:Type) = 
        Impl.checkNonNull "domain" domain;
        Impl.checkNonNull "range" range;
        Impl.func.MakeGenericType [| domain; range |]

    static member MakeTupleType(types:Type[]) =  
        Impl.checkNonNull "types" types;
        if types |> Array.exists (function null -> true | _ -> false) then 
             invalidArg "types" "one of the types is null"
        Impl.mkTupleType types

    static member GetTupleElements(tupleType:Type) =
        Impl.checkTupleType("tupleType",tupleType);
        Impl.getTupleTypeInfo tupleType

    static member GetFunctionElements(functionType:Type) =
        Impl.checkNonNull "functionType" functionType;
        let functionType = Impl.getTypeOfReprType (functionType ,BindingFlags.Public ||| BindingFlags.NonPublic)
        Impl.getFunctionTypeInfo functionType

    static member GetRecordFields(recordType:Type,?bindingFlags) =
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkRecordType("recordType",recordType,bindingFlags);
        Impl.fieldPropsOfRecordType(recordType,bindingFlags) 

    static member GetUnionCases (unionType:Type,?bindingFlags) = 
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        let unionType = Impl.getTypeOfReprType (unionType ,bindingFlags)
        Impl.checkUnionType(unionType,bindingFlags);
        Impl.getUnionTypeTagFields(unionType,bindingFlags) |> Array.mapi (fun i _ -> UnionCaseInfo(unionType,i))

    static member IsExceptionRepresentation(typ:Type, ?bindingFlags) = 
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkNonNull "typ" typ;
        Impl.isExceptionRepr(typ,bindingFlags)

    static member GetExceptionFields(exceptionType:Type, ?bindingFlags) = 
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkExnType(exceptionType,bindingFlags);
        Impl.fieldPropsOfRecordType (exceptionType,bindingFlags) 

type DynamicFunction<'T1,'T2> = 
    static member Make(impl : obj -> obj) : obj = 
        box<('T1 -> 'T2)> (fun inp -> unbox<'T2>(impl (box<'T1>(inp))))


[<AbstractClass; Sealed>]
type FSharpValue = 

    static member MakeRecord(recordType:Type,args,?bindingFlags) = 
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkRecordType("recordType",recordType,bindingFlags);
        Impl.getRecordConstructor (recordType,bindingFlags) args

    static member GetRecordField(record:obj,info:PropertyInfo) =
        Impl.checkNonNull "info" info;
        Impl.checkNonNull "record" record;
        let reprty = record.GetType() 
        if not (Impl.isRecordType(reprty,BindingFlags.Public ||| BindingFlags.NonPublic)) then invalidArg "record" "the object is not an F# record value";
        info.GetValue(record,null)

    static member GetRecordFields(record:obj,?bindingFlags) =
        Impl.checkNonNull "record" record;
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        let typ = record.GetType() 
        if not (Impl.isRecordType(typ,bindingFlags)) then invalidArg "record" "the object is not an F# record value";
        Impl.getRecordReader (typ,bindingFlags) record

    static member PreComputeRecordFieldReader(info:PropertyInfo) = 
        Impl.checkNonNull "info" info;
        (fun (obj:obj) -> info.GetValue(obj,null))

    static member PreComputeRecordReader(recordType:Type,?bindingFlags) : (obj -> obj[]) =  
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkRecordType("recordType",recordType,bindingFlags);
        Impl.getRecordReader (recordType,bindingFlags)

    static member PreComputeRecordConstructor(recordType:Type,?bindingFlags) = 
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkRecordType("recordType",recordType,bindingFlags);
        Impl.getRecordConstructor (recordType,bindingFlags)

    static member PreComputeRecordConstructorInfo(recordType:Type, ?bindingFlags) =
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkRecordType("recordType",recordType,bindingFlags);
        Impl.getRecordConstructorMethod(recordType,bindingFlags)


    static member MakeFunction(functionType:Type,implementation:(obj->obj)) = 
        Impl.checkNonNull "functionType" functionType;
        if not (Impl.isFunctionType functionType) then invalidArg "functionType" "not a function type";
        Impl.checkNonNull "implementation" implementation;
        let domain,range = Impl.getFunctionTypeInfo functionType
        let dynCloMakerTy = typedefof<DynamicFunction<obj,obj>>
        let saverTy = dynCloMakerTy.MakeGenericType [| domain; range |]
#if FX_NO_CULTURE_INFO_ARGS
        saverTy.InvokeMember("Make",BindingFlags.Static  ||| BindingFlags.InvokeMethod ||| BindingFlags.Public ||| BindingFlags.NonPublic,null,null,[| box implementation |]) 
#else
        saverTy.InvokeMember("Make",BindingFlags.Static  ||| BindingFlags.InvokeMethod ||| BindingFlags.Public ||| BindingFlags.NonPublic,null,null,[| box implementation |],CultureInfo.InvariantCulture(*FxCop:1304*)) 
#endif

    static member MakeTuple(tupleElements: obj[],tupleType:Type) =
        Impl.checkNonNull "tupleElements" tupleElements;
        Impl.checkTupleType("tupleType",tupleType) 
        Impl.getTupleConstructor tupleType tupleElements
    
    static member GetTupleFields(tuple:obj) = // argument name(s) used in error message
        Impl.checkNonNull "tuple" tuple;
        let typ = tuple.GetType() 
        if not (Impl.isTupleType typ ) then invalidArg "tuple" "the object is not a tuple";
        Impl.getTupleReader typ tuple

    static member GetTupleField(tuple:obj,index:int) = // argument name(s) used in error message
        Impl.checkNonNull "tuple" tuple;
        let typ = tuple.GetType() 
        if not (Impl.isTupleType typ ) then invalidArg "tuple" "the object is not a tuple";
        let fields = Impl.getTupleReader typ tuple
        if index < 0 || index >= fields.Length then invalidArg "index" "tuple field index out of range";
        fields.[index]
    
    static member PreComputeTupleReader(tupleType:Type) : (obj -> obj[])  =
        Impl.checkTupleType("tupleType",tupleType) 
        Impl.getTupleReader tupleType
    
    static member PreComputeTuplePropertyInfo(tupleType:Type,index:int) =
        Impl.checkTupleType("tupleType",tupleType) 
        Impl.getTupleReaderInfo (tupleType,index)
    
    static member PreComputeTupleConstructor(tupleType:Type) = 
        Impl.checkTupleType("tupleType",tupleType) 
        Impl.getTupleConstructor tupleType

    static member PreComputeTupleConstructorInfo(tupleType:Type) =
        Impl.checkTupleType("tupleType",tupleType) 
        Impl.getTupleConstructorInfo (tupleType) 

    static member MakeUnion(unionCase:UnionCaseInfo,args: obj [],?bindingFlags) = 
        Impl.checkNonNull "unionCase" unionCase;
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.getUnionCaseConstructor (unionCase.DeclaringType,unionCase.Tag,bindingFlags) args

    static member PreComputeUnionConstructor (unionCase:UnionCaseInfo,?bindingFlags) = 
        Impl.checkNonNull "unionCase" unionCase;
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.getUnionCaseConstructor (unionCase.DeclaringType,unionCase.Tag,bindingFlags)

    static member PreComputeUnionConstructorInfo(unionCase:UnionCaseInfo, ?bindingFlags) =
        Impl.checkNonNull "unionCase" unionCase;
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.getUnionCaseConstructorMethod (unionCase.DeclaringType,unionCase.Tag,bindingFlags) 

    static member GetUnionFields(obj:obj,unionType:Type,?bindingFlags) = 
        //System.Console.WriteLine("typ1 = {0}",box unionType)
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        let unionType = Impl.ensureType(unionType,obj,bindingFlags) 
        //System.Console.WriteLine("typ2 = {0}",box unionType)
        Impl.checkNonNull "unionType" unionType;
        let unionType = Impl.getTypeOfReprType (unionType ,bindingFlags)
        //System.Console.WriteLine("typ3 = {0}",box unionType)
        Impl.checkUnionType(unionType,bindingFlags);
        let tag = Impl.getUnionTagReader (unionType,bindingFlags) obj
        let flds = Impl.getUnionCaseRecordReader (unionType,tag,bindingFlags) obj 
        UnionCaseInfo(unionType,tag), flds

    static member PreComputeUnionTagReader(unionType: Type,?bindingFlags) : (obj -> int) = 
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkNonNull "unionType" unionType;
        let unionType = Impl.getTypeOfReprType (unionType ,bindingFlags)
        Impl.checkUnionType(unionType,bindingFlags);
        Impl.getUnionTagReader (unionType ,bindingFlags)

    static member PreComputeUnionTagMemberInfo(unionType: Type,?bindingFlags) = 
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        Impl.checkNonNull "unionType" unionType;
        let unionType = Impl.getTypeOfReprType (unionType ,bindingFlags)
        Impl.checkUnionType(unionType,bindingFlags);
        Impl.getUnionTagMemberInfo(unionType ,bindingFlags)

    static member PreComputeUnionReader(unionCase: UnionCaseInfo,?bindingFlags) : (obj -> obj[])  = 
        Impl.checkNonNull "unionCase" unionCase;
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        let typ = unionCase.DeclaringType 
        Impl.getUnionCaseRecordReader (typ,unionCase.Tag,bindingFlags) 
    

    static member GetExceptionFields(exn:obj, ?bindingFlags) = 
        Impl.checkNonNull "exn" exn;
        let bindingFlags = defaultArg bindingFlags BindingFlags.Public 
        let typ = exn.GetType() 
        Impl.checkExnType(typ,bindingFlags);
        Impl.getRecordReader (typ,bindingFlags) exn


