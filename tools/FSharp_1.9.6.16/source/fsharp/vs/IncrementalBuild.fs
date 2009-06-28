#light

namespace Microsoft.FSharp.Compiler
#nowarn "57"
open Internal.Utilities.Debug
open System
open System.IO
open System.Reflection             
open System.Diagnostics
open System.Collections.Generic
open System

open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 

module IncrementalBuild =
    
    /// A particular node in the Expr language. Use an int for keys instead of the entire Expr to avoid extra hashing.
    type Id = 
        | Id of int
        static member toInt = function Id(id)->id
        override id.ToString() = match id with Id(n)->sprintf "Id(%d)" n
            
    type ScalarExpr = 
        | ScalarInput of Id * (*name*)string
        | ScalarDemultiplex of Id * (*name*)string * (*input*) VectorExpr * (*task function*) (obj array -> obj)
        | ScalarMap of Id * (*name*) string * (*input*) ScalarExpr * (*task function*) (obj->obj)
        /// Get the Id for the given ScalarExpr.
        static member GetId = function
            | ScalarInput(id,_)->id
            | ScalarDemultiplex(id,_,_,_)->id
            | ScalarMap(id,_,_,_)->id
        /// Get the Name for the givenScalarExpr.
        static member GetName = function
            | ScalarInput(_,n)->n                
            | ScalarDemultiplex(_,n,_,_)->n
            | ScalarMap(_,n,_,_)->n                
        override ve.ToString() = 
            match ve with 
            | ScalarInput(Id(id),name)->sprintf "InputScalar(%d,%s)" id name
            | ScalarDemultiplex(Id(id),name,_,_)->sprintf "ScalarDemultiplex(%d,%s)" id name
            | ScalarMap(Id(id),name,_,_)->sprintf "ScalarMap(%d,%s)" id name
    and VectorExpr = 
        | VectorInput of Id * (*name*)string * Type
        | VectorScanLeft of Id * (*name*)string * (*accumulator*)ScalarExpr * (*input vector*)VectorExpr * (*task function*)(obj->obj->Eventually<obj>)
        | VectorMap of Id * (*taskname*)string * (*input*)VectorExpr * (*task function*)(obj->obj) 
        | VectorStamp of Id * (*taskname*)string * (*input*)VectorExpr * (*task function*)(obj->DateTime)
        | VectorMultiplex of Id * (*taskname*)string * (*input*)ScalarExpr * (*task function*)(obj->obj array)
        /// Get the Id for the given VectorExpr.
        static member GetId = function
            | VectorInput(id,_,_)->id
            | VectorScanLeft(id,_,_,_,_)->id
            | VectorMap(id,_,_,_)->id
            | VectorStamp(id,_,_,_)->id
            | VectorMultiplex(id,_,_,_)->id
        /// Get the Name for the given VectorExpr.
        static member GetName = function
            | VectorInput(_,n,_)->n
            | VectorScanLeft(_,n,_,_,_)->n
            | VectorMap(_,n,_,_)->n
            | VectorStamp(_,n,_,_)->n
            | VectorMultiplex(_,n,_,_)->n
        override ve.ToString() = 
            match ve with 
            | VectorInput(Id(id),name,_)->sprintf "VectorInput(%d,%s)" id name
            | VectorScanLeft(Id(id),name,_,_,_)->sprintf "VectorScanLeft(%d,%s)" id name
            | VectorMap(Id(id),name,_,_)->sprintf "VectorMap(%d,%s)" id name
            | VectorStamp(Id(id),name,_,_)->sprintf "VectorStamp(%d,%s)" id name
            | VectorMultiplex(Id(id),name,_,_)->sprintf "VectorMultiplex(%d,%s)" id name
        
    type Expr =
        | ScalarExpr of ScalarExpr
        | VectorExpr of VectorExpr      
        /// Get the Id for the given Expr.
        static member GetId = function
            | ScalarExpr(se)->ScalarExpr.GetId(se)
            | VectorExpr(ve)->VectorExpr.GetId(ve)      
        /// Get the Name for the given Expr.
        static member GetName= function
            | ScalarExpr(se)->ScalarExpr.GetName(se)
            | VectorExpr(ve)->VectorExpr.GetName(ve)      
        override e.ToString() = 
            match e with 
            | ScalarExpr(se)->sprintf "ScalarExpr(se)" 
            | VectorExpr(ve)->sprintf "VectorExpr(ve)"

    // Ids of exprs            
    let nextid = ref 999 // Number ids starting with 1000 to discern them
    let NextId() =
        nextid:=!nextid+1
        Id(!nextid)                    
        
    type IScalar = 
        abstract GetScalarExpr : unit -> ScalarExpr
    type IVector =
        abstract GetVectorExpr : unit-> VectorExpr
            
    type Scalar<'T> =  interface 
        end

    type Vector<'T> = interface 
        end
    
    /// The outputs of a build        
    type NamedOutput = 
        | NamedVectorOutput of string * IVector
        | NamedScalarOutput of string * IScalar

    /// Visit each task and call op with the given accumulator.
    let ForeachExpr(rules, op, acc)=
        let rec VisitVector (ve:VectorExpr) acc = 
            match ve with
            | VectorInput(_)->op (VectorExpr ve) acc
            | VectorScanLeft(_,_,a,i,_)->op (VectorExpr ve) (VisitVector i (VisitScalar a acc))
            | VectorMap(_,_,i,_)
            | VectorStamp(_,_,i,_)->op (VectorExpr ve) (VisitVector i acc)
            | VectorMultiplex(_,_,i,_)->op (VectorExpr ve) (VisitScalar i acc)
        and VisitScalar (se:ScalarExpr) acc = 
            match se with
            | ScalarInput(_)->op (ScalarExpr se) acc
            | ScalarDemultiplex(_,_,i,_)->op (ScalarExpr se) (VisitVector i acc)
            | ScalarMap(_,_,i,_)->op (ScalarExpr se) (VisitScalar i acc)
        let rec Visit (expr:Expr) acc =  
            match expr with
            | ScalarExpr(se)->VisitScalar se acc
            | VectorExpr(ve)->VisitVector ve acc
        List.foldBack Visit (rules |> List.map(snd)) acc            
    
    /// Convert from interfaces into discriminated union.
    let ToBuild (names:NamedOutput list) : (string * Expr) list = 

        // Create the rules.
        let CreateRules() = names |> List.map(function NamedVectorOutput(n,v) -> n,VectorExpr(v.GetVectorExpr())
                                                     | NamedScalarOutput(n,s) -> n,ScalarExpr(s.GetScalarExpr()))
        
        // Ensure that all names are unique.
        let EnsureUniqueNames (expr:Expr) (acc:Map<string,Id>) = 
            let AddUniqueIdToNameMapping(id,name)=
                match acc.TryFind name with
                 | Some(priorId)-> 
                    if id<>priorId then failwith (sprintf "Two build expressions had the same name: %s" name)
                    else acc
                 | None-> Map.add name id acc
            let id = Expr.GetId(expr)
            let name = Expr.GetName(expr)
            AddUniqueIdToNameMapping(id,name)
        
        // Validate the rule tree
        let ValidateRules(rules:(string*Expr) list) =
            ForeachExpr(rules,EnsureUniqueNames,Map.empty) |> ignore
        
        // Convert and validate
        let rules = CreateRules()
        ValidateRules(rules)
        rules

    /// These describe the input conditions for a result. If conditions change then the result is invalid.
    type InputSignature =
        | SingleMappedVectorInput of InputSignature array
        | EmptyTimeStampedInput of DateTime
        | BoundInputScalar // An external input into the build
        | BoundInputVector // An external input into the build
        | IndexedValueElement of DateTime
        | UnevaluatedInput
        /// Return true if the result is fully evaluated
        member is.IsEvaluated() = 
        
            let rec IsEvaluated(is) =
                match is with
                | UnevaluatedInput -> false
                | SingleMappedVectorInput iss -> iss |> Array.forall IsEvaluated
                | _ -> true
            IsEvaluated(is)
        override is.ToString() = sprintf "%A" is
            
    
    /// A slot for holding a single result.
    type Result =
        | NotAvailable
        | InProgress of (unit -> Eventually<obj>) * DateTime 
        | Available of obj * DateTime * InputSignature
        /// Get the available result. Throw an exception if not available.
        static member GetAvailable = function Available(o,_,_)->o  | _->failwith "No available result"
        /// Get the time stamp if available. Otheriwse MaxValue.        
        static member Timestamp = function Available(_,ts,_)->ts | InProgress(_,ts) -> ts | _-> DateTime.MaxValue
        /// Get the time stamp if available. Otheriwse MaxValue.        
        static member InputSignature = function Available(_,_,signature)->signature | _-> UnevaluatedInput
        
        member x.ResultIsInProgress =  match x with | InProgress _ -> true | _ -> false
        member x.GetInProgressContinuation() =  match x with | InProgress (f,_) -> f() | _ -> failwith "not in progress"
        member x.TryGetAvailable() =  match x with | InProgress _ | NotAvailable -> None | Available(obj,dt,i) -> Some(obj,dt,i)

        override r.ToString() = 
            match r with 
            | NotAvailable -> "NotAvailable"
            | InProgress _ -> "InProgress"
            | Available(o,ts,signature) -> sprintf "Available(as of %A)" ts
            
    /// An immutable sparse vector of results.                
    type ResultVector(size,zeroElementTimestamp,map) =
        let get(slot) = 
            match Map.tryfind slot map with
            | Some(result)->result
            | None->NotAvailable                   
        let asList = lazy List.map (fun i->i,get(i)) [0..size-1]

        static member OfSize(size) = ResultVector(size,DateTime.MinValue,Map.empty)
        member rv.Size = size
        member rv.Get(slot) = get(slot)
        member rv.Resize(newsize) = 
            if size<>newsize then 
                ResultVector(newsize, zeroElementTimestamp, map|>Map.filter(fun s v -> s<newsize))
            else rv
        member rv.Set(slot,value) = 
            #if DEBUG
            if slot<0 then failwith "ResultVector slot less than zero"
            if slot>=size then failwith "ResultVector slot too big"
            #endif
            ResultVector(size, zeroElementTimestamp, Map.add slot value map)
        member rv.MaxTimestamp() =
//            use t = Trace.Call("IncrementalBuildVerbose", "MaxTimestamp",  fun _->sprintf "vector of size=%d" size)
            let Maximize (lasttimestamp:DateTime) (_,result) = 
                let thistimestamp = Result.Timestamp result
                let m = max lasttimestamp thistimestamp
//                use t = Trace.Call("IncrementalBuildVerbose", "Maximize",  fun _->sprintf "last=%s this=%s max=%s" (lasttimestamp.ToString()) (thistimestamp.ToString()) (m.ToString()))
                m
            List.fold Maximize zeroElementTimestamp (asList.Force())
        member rv.Signature() =
            let l = asList.Force()
            let l = l |> List.map(fun (_,result)->Result.InputSignature result)
            SingleMappedVectorInput (l|>List.to_array)
                                  
        member rv.FoldLeft f s : 'a = List.fold f s (asList.Force())
        override rv.ToString() = asList.ToString()   // NOTE: Force()ing this inside ToString() leads to StackOverflowException and very undesirable debugging behavior for all of F#
                
    /// A result of performing build actions
    type ResultSet =
        | ScalarResult of Result
        | VectorResult of ResultVector
        override rs.ToString() = 
            match rs with
            | ScalarResult(sr)->sprintf "ScalarResult(%s)" (sr.ToString())
            | VectorResult(rs)->sprintf "VectorResult(%s)" (rs.ToString())
                            
    /// Action timing
    module Time =     
        let sw = new Stopwatch()
        let Action<'T> taskname slot func : 'T= 
            if Trace.ShouldLog("IncrementalBuildWorkUnits") then 
                let slotMessage = 
                    if slot= -1 then sprintf "%s" taskname
                    else sprintf "%s over slot %d" taskname slot
                // Timings and memory
                let maxGen = System.GC.MaxGeneration
                let ptime = System.Diagnostics.Process.GetCurrentProcess()
                let timePrev = ptime.UserProcessorTime.TotalSeconds
                let gcPrev = [| for i in 0 .. maxGen -> System.GC.CollectionCount(i) |]
                let pbPrev = ptime.PrivateMemorySize64 in                

                // Call the function
                let result = func()
                
                // Report.
                let timeNow = ptime.UserProcessorTime.TotalSeconds
                let gcNow = [| for i in 0 .. maxGen -> System.GC.CollectionCount(i) |]
                let pbNow = ptime.PrivateMemorySize64
                let spanGC = [| for i in 0 .. maxGen -> System.GC.CollectionCount(i) - gcPrev.[i] |]
                
                Trace.PrintLine("IncrementalBuildWorkUnits", fun _ ->
                                                        sprintf "%s TIME: %4.3f MEM: %3d (delta) G0: %3d G1: %2d G2: %2d" 
                                                            slotMessage
                                                            (timeNow - timePrev) 
                                                            (pbNow - pbPrev)
                                                            spanGC.[min 0 maxGen] 
                                                            spanGC.[min 1 maxGen] 
                                                            spanGC.[min 2 maxGen])
                result
            else func()            
        
    /// Result of a particular action over the bound build tree
    type ActionResult = 
        | IndexedResult of Id * int * (*slotcount*) int * Eventually<obj> * DateTime 
        | ScalarValuedResult of Id * obj * DateTime * InputSignature
        | VectorValuedResult of Id * obj array * DateTime * InputSignature
        | ResizeResult of Id * (*slotcount*) int
        override ar.ToString() = 
            match ar with
            | IndexedResult(id,slot,slotcount,obj,dt)->sprintf "IndexedResult(%d,%d,%d,obj,%A)" (Id.toInt id) slot slotcount dt
            | ScalarValuedResult(id,obj,dt,inputsig)->sprintf "ScalarValuedResult(%d,obj,%A,%A)" (Id.toInt id) dt inputsig
            | VectorValuedResult(id,obj,dt,inputsig)->sprintf "VectorValuedResult(%d,obj array,%A,%A)" (Id.toInt id) dt inputsig
            | ResizeResult(id,slotcount)->sprintf "ResizeResult(%d,%d)" (Id.toInt id) slotcount
        
        
    /// A pending action over the bound build tree
    type Action = 
        | IndexedAction of Id * (*taskname*)string * int * (*slotcount*) int * DateTime * (unit->Eventually<obj>)
        | ScalarAction of Id * (*taskname*)string * DateTime * InputSignature * (unit->obj)
        | VectorAction of Id * (*taskname*)string * DateTime * InputSignature *  (unit->obj array)
        | ResizeResultAction of Id * (*slotcount*) int 
        /// Execute one action and return a corresponding result.
        static member Execute action = 
            match action with
            | IndexedAction(id,taskname,slot,slotcount,timestamp,func) -> IndexedResult(id,slot,slotcount,Time.Action taskname slot func,timestamp)
            | ScalarAction(id,taskname,timestamp,inputsig,func) -> ScalarValuedResult(id,Time.Action taskname (-1) func,timestamp,inputsig)
            | VectorAction(id,taskname,timestamp,inputsig,func) -> VectorValuedResult(id,Time.Action taskname (-1) func,timestamp,inputsig)
            | ResizeResultAction(id,slotcount) -> ResizeResult(id,slotcount)
     
    /// String helper functions for when there's no %A
    type String = 
        static member OfList2 l =
            " ["^String.Join(",\n ", List.to_array (l|>List.map (fun (v1,v2)->((box v1).ToString())^";"^((box v2).ToString()))))^" ]"
            
    /// A set of build rules and the corresponding, possibly partial, results from building.
    [<Sealed>]
    type Build(rules:(string * Expr) list, 
               results:Map<Id,ResultSet>) = 
        member bt.Rules = rules
        member bt.Results = results
        override bt.ToString() = 
            let sb = new System.Text.StringBuilder()
            results |> Map.iter(fun id result->
                                    let id = Id.toInt id
                                    let s = sprintf " {Id=%d,ResultSet=%s}\n" id (result.ToString())
                                    let _ = sb.Append(s)
                                    ())
            sprintf "{Rules=%s\n Results=%s}" (String.OfList2 rules) (sb.ToString())
   
    /// Given an expression, find the expected width.
    let rec GetVectorWidthByExpr(bt:Build,ve:VectorExpr) = 
        let KnownValue(ve) = 
            match bt.Results.TryFind(VectorExpr.GetId(ve)) with 
            | Some(resultSet) ->
                match resultSet with
                | VectorResult(rv)->Some(rv.Size)
                | _ -> failwith "Expected vector to have vector result."
            | None-> None
        match ve with
        | VectorScanLeft(_,_,_,i,_)
        | VectorMap(_,_,i,_)
        | VectorStamp(_,_,i,_)->
            match GetVectorWidthByExpr(bt,i) with
            | Some(width) as r -> r
            | None->KnownValue(ve)  
        | VectorInput(_,_,_) 
        | VectorMultiplex(_,_,_,_)->KnownValue(ve)  
        
    /// Given an expression name, get the corresponding expression.    
    let GetTopLevelExprByName(bt:Build, seek:string) =
        bt.Rules |> List.filter(fun(name,_)->name=seek) |> List.map(fun(_,root)->root) |> List.hd
    
    /// Get an expression matching the given name.
    let GetExprByName(bt:Build, seek:string) : Expr = 
        let MatchName (expr:Expr) (acc:Expr option) : Expr option =
            let name = Expr.GetName(expr)
            if name = seek then Some(expr) else acc
        let matchOption = ForeachExpr(bt.Rules,MatchName,None)
        Option.get matchOption

    // Given an Id, find the corresponding expression.
    let GetExprById(bt:Build, seek:Id) : Expr= 
        let rec VectorExprOfId(ve) =
            match ve with
            | VectorInput(id,_,_)->if seek=id then Some(VectorExpr(ve)) else None
            | VectorScanLeft(id,_,a,i,_)->
                if seek=id then Some(VectorExpr(ve)) else
                    let result = ScalarExprOfId(a) 
                    match result with Some(_) -> result | None->VectorExprOfId(i)
            | VectorMap(id,_,i,_)->if seek=id then Some(VectorExpr(ve)) else VectorExprOfId(i)
            | VectorStamp(id,_,i,_)->if seek=id then Some(VectorExpr(ve)) else VectorExprOfId(i)
            | VectorMultiplex(id,_,i,_)->if seek=id then Some(VectorExpr(ve)) else ScalarExprOfId(i)
        and ScalarExprOfId(se) =
            match se with
            | ScalarInput(id,_)->if seek=id then Some(ScalarExpr(se)) else None
            | ScalarDemultiplex(id,_,i,_)->if seek=id then Some(ScalarExpr(se)) else VectorExprOfId(i)
            | ScalarMap(id,_,i,_)->if seek=id then Some(ScalarExpr(se)) else ScalarExprOfId(i)
        let ExprOfId(expr:Expr) = 
            match expr with
            | ScalarExpr(se)->ScalarExprOfId(se)
            | VectorExpr(ve)->VectorExprOfId(ve)
        let exprs = bt.Rules |> List.map(fun(_,root)->ExprOfId(root)) |> List.filter(Option.is_some)
        match exprs with
        | Some(expr)::_ -> expr
        | unk -> failwith (sprintf "GetExprById did not find an expression for Id %d" (Id.toInt seek))

    let GetVectorWidthById (bt:Build) seek = 
        match GetExprById(bt,seek) with 
        | ScalarExpr(_)->failwith "Attempt to get width of scalar." 
        | VectorExpr(ve)->Option.get (GetVectorWidthByExpr(bt,ve))

    let GetScalarExprResult(bt:Build, se:ScalarExpr) =
        match bt.Results.TryFind(ScalarExpr.GetId(se)) with 
        | Some(resultSet) ->
            match se,resultSet with
            | ScalarInput(_),ScalarResult(r)
            | ScalarMap(_),ScalarResult(r)
            | ScalarDemultiplex(_),ScalarResult(r)->r
            | se,result->failwith (sprintf "GetScalarExprResult had no match for %A,%A" se result) 
        | None->NotAvailable

    let GetVectorExprResultVector(bt:Build, ve:VectorExpr) =
        match bt.Results.TryFind(VectorExpr.GetId(ve)) with 
        | Some(resultSet) ->
            match ve,resultSet with
            | VectorScanLeft(_),VectorResult(rv)
            | VectorMap(_),VectorResult(rv)
            | VectorInput(_),VectorResult(rv)
            | VectorStamp(_),VectorResult(rv)
            | VectorMultiplex(_),VectorResult(rv) -> Some(rv)
            | ve,result->failwith (sprintf "GetVectorExprResultVector had no match for %A,%A" ve result) 
        | None->None

    let GetVectorExprResult(bt:Build, ve:VectorExpr, slot) =
        match bt.Results.TryFind(VectorExpr.GetId(ve)) with 
        | Some(resultSet) ->
            match ve,resultSet with
            | VectorScanLeft(_),VectorResult(rv)
            | VectorMap(_),VectorResult(rv)
            | VectorInput(_),VectorResult(rv)
            | VectorStamp(_),VectorResult(rv) -> rv.Get(slot)
            | VectorMultiplex(_),VectorResult(rv) -> rv.Get(slot)
            | ve,result->failwith (sprintf "GetVectorExprResult had no match for %A,%A" ve result) 
        | None->NotAvailable

    /// Get the maximum build stamp for an output.
    let MaxTimestamp(bt:Build,id,inputstamp) = 
        match bt.Results.TryFind(id) with
        | Some(resultset) -> 
            match resultset with 
            | ScalarResult(rs) -> Result.Timestamp rs
            | VectorResult(rv) -> rv.MaxTimestamp()
        | None -> DateTime.MaxValue
        
    let Signature(bt:Build,id) =
        match bt.Results.TryFind(id) with
        | Some(resultset) -> 
            match resultset with 
            | ScalarResult(rs) -> Result.InputSignature rs
            | VectorResult(rv) -> rv.Signature()
        | None -> UnevaluatedInput               
     
    /// Get all the results for the given expr.
    let AllResultsOfExpr extractor (bt:Build) expr = 
        let GetAvailable (rv:ResultVector) = 
            let Extract acc (slot:int,result) = (extractor result)::acc
            List.rev (rv.FoldLeft Extract [])
        let GetVectorResultById id = 
            match bt.Results.TryFind(id) with
            | Some(found) ->
                match found with
                | VectorResult(rv)->GetAvailable rv
                | _ -> failwith "wrong result type"
            | None -> []
            
        GetVectorResultById(VectorExpr.GetId(expr))


   
        
    let AvailableAllResultsOfExpr bt expr = 
        let msg = "Expected all results to be available"
        AllResultsOfExpr (function Available(o,_,_)->o|x->failwith msg) bt expr
        
    /// Bind a set of build rules to a set of input values.
    let ToBound(build:(string*Expr) list, vectorinputs, scalarinputs) = 
        let now = DateTime.Now
        let rec ApplyScalarExpr(se,results) =
            match se with
            | ScalarInput(id,n) -> 
                let matches = scalarinputs 
                                |> List.filter (fun (inputname,_)->inputname=n) 
                                |> List.map (fun (_,inputvalue:obj)-> ScalarResult(Available(inputvalue,now,BoundInputScalar)))
                List.foldBack (Map.add id) matches results
            | ScalarMap(_,_,se,_) ->ApplyScalarExpr(se,results)
            | ScalarDemultiplex(_,_,ve,_) ->ApplyVectorExpr(ve,results)
        and ApplyVectorExpr(ve,results) =
            match ve with
            | VectorInput(id,n,t) ->
                let matches = vectorinputs 
                                |> List.filter (fun (inputname,_,_)->inputname=n) 
                                |> List.map (fun (_,size,inputvalues:obj list)->
                                                        let results = inputvalues|>List.mapi(fun i value->i,Available(value,now,BoundInputVector))
                                                        VectorResult(ResultVector(size,DateTime.MinValue,results|>Map.of_list))
                                                        )
                List.foldBack (Map.add id) matches results
            | VectorScanLeft(_,_,a,i,_)->ApplyVectorExpr(i,ApplyScalarExpr(a,results))
            | VectorMap(_,_,i,_)
            | VectorStamp(_,_,i,_)->ApplyVectorExpr(i,results)
            | VectorMultiplex(_,_,i,_)->ApplyScalarExpr(i,results)
        let ApplyExpr expr results =
            match expr with
            | ScalarExpr(se)->ApplyScalarExpr(se,results)
            | VectorExpr(ve)->ApplyVectorExpr(ve,results)
                                                                             
        // Place vector inputs into results map.
        let results = List.foldBack ApplyExpr (build|>List.map(snd)) (Map.empty)
        Build(build,results)
        
        
    /// Visit each executable action and call actionFunc with the given accumulator.
    let ForeachAction output bt (actionFunc:Action->'acc->'acc) (acc:'acc) =
        use t = Trace.Call("IncrementalBuildVerbose", "ForeachAction",  fun _->sprintf "name=%s" output)
        let seen = Dictionary<_,_>()
        let Seen(id) = 
            if seen.ContainsKey(id) then true
            else seen.[id]<-true
                 false
                 
        let HasChanged(inputtimestamp,outputtimestamp) =
           if inputtimestamp<>outputtimestamp then
               Trace.PrintLine("IncrementalBuildVerbose", fun _ -> sprintf "Input timestamp is %A. Output timestamp is %A." inputtimestamp outputtimestamp)
               true
           else false
           
           
        let ShouldEvaluate(bt,currentsig:InputSignature,id) =
            let isAvailable = currentsig.IsEvaluated()
            if isAvailable then 
                let priorsig = Signature(bt,id)
                currentsig<>priorsig
            else false
            
        /// Make sure the result vector saved matches the size of expr
        let ResizeVectorExpr(ve:VectorExpr,acc)  = 
            let id = VectorExpr.GetId(ve)
            match GetVectorWidthByExpr(bt,ve) with
            | Some(expectedWidth) ->
                match bt.Results.TryFind(id) with
                | Some(found) ->
                    match found with
                    | VectorResult(rv)->
                        if rv.Size<> expectedWidth then 
                            actionFunc (ResizeResultAction(id,expectedWidth)) acc
                        else acc
                    | _ -> acc
                | None -> acc        
            | None -> acc           
        
        let rec VisitVector ve acc =
        
            if Seen(VectorExpr.GetId(ve)) then acc
            else
                Trace.PrintLine("IncrementalBuildVerbose", fun _ -> sprintf "In ForeachAction at vector expression %s" (ve.ToString()))
                let acc = ResizeVectorExpr(ve,acc)        
                match ve with
                | VectorInput(_)->acc
                | VectorScanLeft(id,taskname,accumulatorExpr,inputExpr,func)->
                    let acc =
                        match GetVectorWidthByExpr(bt,ve) with
                        | Some(cardinality) ->                    
                            let GetInputAccumulator(slot) =
                                if slot=0 then GetScalarExprResult(bt,accumulatorExpr) 
                                else GetVectorExprResult(bt,ve,slot-1)
                        
                            let Scan slot =
                                let accumulatorResult = GetInputAccumulator(slot)
                                let inputResult = GetVectorExprResult(bt,inputExpr,slot)
                                match accumulatorResult,inputResult with 
                                | Available(accumulator,accumulatortimesamp,accumulatorInputSig),Available(input,inputtimestamp,inputSig)->
                                    let inputtimestamp = max inputtimestamp accumulatortimesamp
                                    let prevoutput = GetVectorExprResult(bt,ve,slot)
                                    let outputtimestamp = Result.Timestamp prevoutput
                                    let scanOp = 
                                        if HasChanged(inputtimestamp,outputtimestamp) then
                                            Some (fun () -> func accumulator input)
                                        elif prevoutput.ResultIsInProgress then
                                            Some prevoutput.GetInProgressContinuation
                                        else 
                                            // up-to-date and complete, no work required
                                            None
                                    match scanOp with 
                                    | Some scanOp -> Some(actionFunc (IndexedAction(id,taskname,slot,cardinality,inputtimestamp,scanOp)) acc)
                                    | None -> None
                                | _ -> None                            
                                
                            match ([0..cardinality-1]|>List.tryPick Scan) with Some(acc)->acc | None->acc
                        | None -> acc
                    
                    // Check each slot for an action that may be performed.
                    VisitVector inputExpr (VisitScalar accumulatorExpr acc)
                | VectorMap(id, taskname, inputExpr, func)->
                    let acc =
                        match GetVectorWidthByExpr(bt,ve) with
                        | Some(cardinality) ->       
                            if cardinality=0 then
                                // For vector length zero, just propagate the prior timestamp.
                                let inputtimestamp = MaxTimestamp(bt,VectorExpr.GetId(inputExpr),DateTime.MinValue)
                                let outputtimestamp = MaxTimestamp(bt,id,DateTime.MinValue)
                                if HasChanged(inputtimestamp,outputtimestamp) then
                                    Trace.PrintLine("IncrementalBuildVerbose", fun _ -> sprintf "Vector Map with cardinality zero setting output timestamp to %A." inputtimestamp)
                                    actionFunc (VectorAction(id,taskname,inputtimestamp,EmptyTimeStampedInput inputtimestamp, fun _ ->[||])) acc
                                else acc
                            else                                                
                                let MapResults acc slot =
                                    let inputtimestamp = Result.Timestamp (GetVectorExprResult(bt,inputExpr,slot))
                                    let outputtimestamp = Result.Timestamp (GetVectorExprResult(bt,ve,slot))
                                    if HasChanged(inputtimestamp,outputtimestamp) then
                                        let OneToOneOp() =
                                            Eventually.Done (func (Result.GetAvailable (GetVectorExprResult(bt,inputExpr,slot))))
                                        actionFunc (IndexedAction(id,taskname,slot,cardinality,inputtimestamp,OneToOneOp)) acc
                                    else acc
                                [0..cardinality-1] |> List.fold MapResults acc                         
                        | None -> acc
                    VisitVector inputExpr acc
                | VectorStamp(id, taskname, inputExpr, func)-> 
               
                    // For every result that is available, check time stamps.
                    let acc =
                        match GetVectorWidthByExpr(bt,ve) with
                        | Some(cardinality) ->    
                            if cardinality=0 then
                                // For vector length zero, just propagate the prior timestamp.
                                let inputtimestamp = MaxTimestamp(bt,VectorExpr.GetId(inputExpr),DateTime.MinValue)
                                let outputtimestamp = MaxTimestamp(bt,id,DateTime.MinValue)
                                if HasChanged(inputtimestamp,outputtimestamp) then
                                    Trace.PrintLine("IncrementalBuildVerbose", fun _ -> sprintf "Vector Stamp with cardinality zero setting output timestamp to %A." inputtimestamp)
                                    actionFunc (VectorAction(id,taskname,inputtimestamp,EmptyTimeStampedInput inputtimestamp,fun _ ->[||])) acc
                                else acc
                            else                 
                                let CheckStamp acc slot = 
                                    let inputresult = GetVectorExprResult(bt,inputExpr,slot)
                                    match inputresult with
                                    | Available(ires,_,inputsig)->
                                        let oldtimestamp = Result.Timestamp (GetVectorExprResult(bt,ve,slot))
                                        let newtimestamp = func ires
                                        if newtimestamp<>oldtimestamp then 
                                            Trace.PrintLine("IncrementalBuildVerbose", fun _ -> sprintf "Old timestamp was %A. New timestamp is %A." oldtimestamp newtimestamp)
                                            actionFunc (IndexedAction(id,taskname,slot,cardinality,newtimestamp, fun _ -> Eventually.Done ires)) acc
                                        else acc
                                    | _ -> acc
                                [0..cardinality-1] |> List.fold CheckStamp acc
                        | None -> acc
                    VisitVector inputExpr acc
                | VectorMultiplex(id, taskname, inputExpr, func)-> 
                    VisitScalar inputExpr
                        (match GetScalarExprResult(bt,inputExpr) with
                         | Available(inp,inputtimestamp,inputsig) ->
                           let outputtimestamp = MaxTimestamp(bt,id,inputtimestamp)
                           if HasChanged(inputtimestamp,outputtimestamp) then
                               let MultiplexOp() = func inp
                               actionFunc (VectorAction(id,taskname,inputtimestamp,inputsig,MultiplexOp)) acc
                           else acc
                         | _->acc)                
        and VisitScalar se acc =
            if Seen(ScalarExpr.GetId(se)) then acc
            else
                Trace.PrintLine("IncrementalBuildVerbose", fun _ -> sprintf "In ForeachAction at scalar expression %s" (se.ToString()))
                match se with
                | ScalarInput(_)->acc
                | ScalarDemultiplex(id,taskname,inputExpr,func)->
                    VisitVector inputExpr 
                            (
                                match GetVectorExprResultVector(bt,inputExpr) with
                                | Some(inputresult) ->   
                                    let currentsig = inputresult.Signature()
                                    if ShouldEvaluate(bt,currentsig,id) then
                                        let inputtimestamp = MaxTimestamp(bt, VectorExpr.GetId(inputExpr), DateTime.MaxValue) 
                                        let priorsig = Signature(bt,id)
                                        let DemultiplexOp() = 
                                            let input = AvailableAllResultsOfExpr bt inputExpr |> List.to_array
                                            func input
                                        actionFunc (ScalarAction(id,taskname,inputtimestamp,currentsig,DemultiplexOp)) acc
                                    else acc
                                | None -> acc
                            )
                | ScalarMap(id,taskname,inputExpr,func)->
                    VisitScalar inputExpr
                        (match GetScalarExprResult(bt,inputExpr) with
                         | Available(inp,inputtimestamp,inputsig) ->
                           let outputtimestamp = MaxTimestamp(bt, id, inputtimestamp)
                           if HasChanged(inputtimestamp,outputtimestamp) then
                               let MapOp() = func inp
                               actionFunc (ScalarAction(id,taskname,inputtimestamp,inputsig,MapOp)) acc
                           else acc
                         | _->acc)
                         
        let Visit expr acc = 
            match expr with
            | ScalarExpr(se)->VisitScalar se acc
            | VectorExpr(ve)->VisitVector ve acc                    
                    
        let filtered = bt.Rules |> List.filter (fun (s,e)->s=output) |> List.map(snd)
        List.foldBack Visit filtered acc
    
    /// Given the result of a single action, apply that action to the Build
    let ApplyResult(actionResult:ActionResult,bt:Build) = 
        use t = Trace.Call("IncrementalBuildVerbose", "ApplyResult", fun _ -> "")
        let result = 
            match actionResult with 
            | ResizeResult(id,slotcount) ->
                match bt.Results.TryFind(id) with
                | Some(resultSet) ->
                    match resultSet with 
                    | VectorResult(rv) -> 
                        let rv = rv.Resize(slotcount)
                        let results = Map.add id (VectorResult rv) bt.Results
                        Build(bt.Rules,results)
                    | _ -> failwith "Unexpected"                
                | None -> failwith "Unexpected"
            | ScalarValuedResult(id,value,timestamp,inputsig)->
                Build(bt.Rules, Map.add id (ScalarResult(Available(value,timestamp,inputsig))) bt.Results)
            | VectorValuedResult(id,values,timestamp,inputsig)->
                let Append acc slot = 
                    Map.add slot (Available(values.[slot],timestamp,inputsig)) acc
                let results = [0..values.Length-1]|>List.fold Append (Map.empty)
                let results = VectorResult(ResultVector(values.Length,timestamp,results))
                let bt = Build(bt.Rules, Map.add id results bt.Results)
                bt
                
            | IndexedResult(id,index,slotcount,value,timestamp)->
                let width = (GetVectorWidthById bt id)
                let priorResults = bt.Results.TryFind(id) 
                let prior =
                    match priorResults with
                    | Some(prior)->prior
                    | None->VectorResult(ResultVector.OfSize width)
                match prior with
                | VectorResult(rv)->                                
                    let result = 
                        match value with 
                        | Eventually.Done res -> 
                            Trace.PrintLine("FSharpBackgroundBuildVerbose", fun _ -> "Eventually.Done...")
                            Available(res,timestamp, IndexedValueElement timestamp)
                        | Eventually.NotYetDone f -> 
                            Trace.PrintLine("FSharpBackgroundBuildVerbose", fun _ -> "Eventually.NotYetDone...")
                            InProgress (f,timestamp)
                    let results = rv.Resize(slotcount).Set(index,result)
                    Build(bt.Rules, Map.add id (VectorResult(results)) bt.Results)
                | _->failwith "Unexpected"
        result
        
    /// Evaluate the result of a single output
    let EvalLeafsFirst output bt =
        use t = Trace.Call("IncrementalBuildVerbose", "EvalLeafsFirst", fun _->sprintf "name=%s" output)

        let ExecuteApply action bt = 
            let actionResult = Action.Execute(action)
            ApplyResult(actionResult,bt)
        let rec Eval(bt,gen) =
            Trace.PrintLine("FSharpBackgroundBuildVerbose", fun _ -> sprintf "---- Build generation %d ----" gen)
            #if DEBUG
            // This can happen, for example, if there is a task whose timestamp never stops increasing.
            // Possibly could detect this case directly.
            if gen>5000 then failwith "Infinite loop in incremental builder?"
            #endif
            let newBt = ForeachAction output bt ExecuteApply bt
            if newBt=bt then bt else Eval(newBt,gen+1)
        Eval(bt,0)
        
    let Step output (bt:Build) = 
        use t = Trace.Call("IncrementalBuildVerbose", "Step", fun _->sprintf "name=%s" output)
        
        let BuildActionList() = 
            use t = Trace.Call("IncrementalBuildVerbose", "BuildActionList", fun _->sprintf "name=%s" output)
            let Cons action list =  action :: list  
            // Hey look, we're building up the whole list, executing one thing and then throwing
            // the list away. What about saving the list inside the Build instance?
            ForeachAction output bt Cons []
            
        let ExecuteOneAction(worklist) = 
            use t = Trace.Call("IncrementalBuildVerbose", "ExecuteOneAction", fun _->sprintf "name=%s" output)
            match worklist with 
            | action::_ ->
                let actionResult = Action.Execute(action)
                Some(ApplyResult(actionResult,bt))
            | _->None
            
        ExecuteOneAction(BuildActionList())                
        
    /// Eval by calling step over and over until done.
    let rec EvalStepwise output bt = 
        use t = Trace.Call("IncrementalBuildVerbose", "EvalStepwise", fun _->sprintf "name=%s" output)
        let rec Evaluate(output,bt)= 
            let newBt = Step output bt
            match newBt with
            | Some(newBt)-> Evaluate(output,newBt)
            | None->bt
        Evaluate(output,bt)
        
    // Note: this discards its slot. This causes TypecheckStates to be evaluated for all files
    // even if we only need one such state. This is especially noticeable on startup of
    // large solutions, where no intellisense is available until all files have been typechecked
    let EvalSlot(output,slot,bt) = EvalLeafsFirst output bt
        
    let Eval = EvalLeafsFirst

    let GetScalarResult<'T>(name,bt) : ('T*DateTime) option = 
        use t = Trace.Call("IncrementalBuildVerbose", "GetScalarResult", fun _->sprintf "name=%s" name)
        match GetTopLevelExprByName(bt,name) with 
        | ScalarExpr(se)->
            let id = ScalarExpr.GetId(se)
            match bt.Results.TryFind(id) with
            | Some(result) ->
                match result with 
                | ScalarResult(sr) ->
                    match sr.TryGetAvailable() with                     
                    | Some(r,timestamp,inputsig) -> Some(downcast r, timestamp)
                    | None -> None
                | _ ->failwith "Expected a scalar result."
            | None->None
        | VectorExpr(ve)->failwith "Expected scalar."
    
    let GetVectorResult<'T>(name,bt) : 'T array = 
        match GetTopLevelExprByName(bt,name) with 
        | ScalarExpr(se)->failwith "Expected vector."
        | VectorExpr(ve)->AvailableAllResultsOfExpr bt ve |> List.map(unbox) |> Array.of_list
        
    let GetVectorResultBySlot<'T>(name,slot,bt) : ('T*DateTime) option = 
        match GetTopLevelExprByName(bt,name) with 
        | ScalarExpr(se)->failwith "Expected vector expression"
        | VectorExpr(ve)->
            match GetVectorExprResult(bt,ve,slot).TryGetAvailable() with
            | Some(o,timestamp,inputsig) -> Some(downcast o,timestamp)
            | None->None

    /// Given an input value, find the corresponding slot.        
    let GetSlotByInput<'T>(name:string,input:'T,build:Build,equals:'T->'T->bool) : int = 
        let expr = GetExprByName(build,name)
        let id =Expr.GetId(expr)
        let resultSet = Option.get ( build.Results.TryFind(id))
        match resultSet with 
        | VectorResult(rv)->
            let MatchNames acc (slot,result) = 
                match result with
                | Available(o,_,_)->
                    let o = o :?> 'T
                    if equals o input then Some(slot) else acc
                | _ -> acc
            let slotOption = rv.FoldLeft MatchNames None
            match slotOption with 
            | Some(slot) -> slot
            | _ -> failwith (sprintf "Could not find requested input %A in set %s" input (rv.ToString()))
        | _ -> failwith (sprintf "Could not find requested input: %A" input)

    
    // Redeclare functions in the incremental build scope-----------------------------------------------------------------------

    // Methods for declaring inputs and outputs            

    let InputVector name = 
        let expr = VectorInput(NextId(),name,typeof<'T>) 
        { new Vector<'T>
          interface IVector with
               override pe.GetVectorExpr() = expr }

    let InputScalar name = 
        let expr = ScalarInput(NextId(),name)
        { new Scalar<'T>
          interface IScalar with
               override pe.GetScalarExpr() = expr }
    
    module Scalar =
    
        let Map (taskname:string) (task:'I->'O) (input:Scalar<'I>) : Scalar<'O> =
            let BoxingMap i = box(task(unbox(i)))
            let input = (input:?>IScalar).GetScalarExpr()
            let expr = ScalarMap(NextId(),taskname,input,BoxingMap)
            { new Scalar<'O>
              interface IScalar with
                   override pe.GetScalarExpr() = expr}
                   
        let Multiplex (taskname:string) (task:'I -> 'O array) (input:Scalar<'I>) : Vector<'O> =      
            let BoxingMultiplex i = Array.map box (task(unbox(i)))
            let input = (input:?>IScalar).GetScalarExpr()
            let expr = VectorMultiplex(NextId(),taskname,input,BoxingMultiplex) 
            { new Vector<'O>
              interface IVector with
                   override pe.GetVectorExpr() = expr}    
            
    module Vector =
        let Map (taskname:string) (task:'I ->'O) (input:Vector<'I>) : Vector<'O> = 
            let BoxingMapVector i =
                box(task(unbox i))
            let input = (input:?>IVector).GetVectorExpr()
            let expr = VectorMap(NextId(),taskname,input,BoxingMapVector) 
            { new Vector<'O>
              interface IVector with
                   override pe.GetVectorExpr() = expr }            
            
        
        let ScanLeft (taskname:string) (task:'A -> 'I -> Eventually<'A>) (acc:Scalar<'A>) (input:Vector<'I>) : Vector<'A> =
            let BoxingScanLeft a i =
                Eventually.box(task (unbox a) (unbox i))
            let acc = (acc:?>IScalar).GetScalarExpr()
            let input = (input:?>IVector).GetVectorExpr()
            let expr = VectorScanLeft(NextId(),taskname,acc,input,BoxingScanLeft) 
            { new Vector<'A>
              interface IVector with
                   override pe.GetVectorExpr() = expr }    
            
        let Demultiplex (taskname:string) (task:'I array -> 'O) (input:Vector<'I>) : Scalar<'O> =
            let BoxingDemultiplex i =
                box(task (Array.map unbox i) )
            let input = (input:?>IVector).GetVectorExpr()
            let expr = ScalarDemultiplex(NextId(),taskname,input,BoxingDemultiplex)
            { new Scalar<'O>
              interface IScalar with
                   override pe.GetScalarExpr() = expr }                
            
        let Stamp (taskname:string) (task:'I -> DateTime) (input:Vector<'I>) : Vector<'I> =
            let BoxingTouch i =
                task(unbox i)
            let input = (input:?>IVector).GetVectorExpr()
            let expr = VectorStamp(NextId(),taskname,input,BoxingTouch) 
            { new Vector<'I>
              interface IVector with
                   override pe.GetVectorExpr() = expr }    
                  
        let AsScalar (taskname:string) (input:Vector<'I>) : Scalar<'I array> = 
            Demultiplex taskname (fun v->v) input
                   
    type BuildScope() =
        let outputs = ref []
        member b.DeclareScalarOutput(name,output:Scalar<'t>)=
            let output:IScalar = output:?>IScalar
            outputs := NamedScalarOutput(name,output) :: !outputs
        member b.DeclareVectorOutput(name,output:Vector<'t>)=
            let output:IVector = output:?>IVector
            outputs := NamedVectorOutput(name,output) :: !outputs
        member b.GetConcreteBuild(vectorinputs,scalarinputs) =
            ToBound(ToBuild(!outputs),vectorinputs,scalarinputs)   


// ------------------------------------------------------------------------------------------
// The incremental build definition for parsing and typechecking F#
// ------------------------------------------------------------------------------------------
module FsiGeneration =

    open Internal.Utilities
    open Internal.Utilities.Collections

    open IncrementalBuild
    open Microsoft.FSharp.Compiler.Build
    open Microsoft.FSharp.Compiler.Fscopts
    open Microsoft.FSharp.Compiler.Ast
    open Microsoft.FSharp.Compiler.ErrorLogger
    open Microsoft.FSharp.Compiler.Env
    open Microsoft.FSharp.Compiler.TypeChecker
    open Microsoft.FSharp.Compiler.Tast 
    open Microsoft.FSharp.Compiler.Range
    open Microsoft.FSharp.Compiler
    open Microsoft.FSharp.Compiler.AbstractIL.Internal

    module Tc = Microsoft.FSharp.Compiler.TypeChecker

    open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
    open Internal.Utilities.Debug

    module Renderer =
      open Microsoft.FSharp.Compiler.Layout

      type Mapping = Dictionary <string list, int * int>

      let posTrackMappingBuildingR (header:string list option) (xySt:(int * int * Mapping)) (rend: ('a, 'b) render) :  ('a * (int * int * Mapping), 'b * (int * int * Mapping)) render =
        { new render<_,_> with 
            member r.Start () = 
                let st = rend.Start ()
                let (x, y, m) = xySt
                let (x, y, st) =
                  match header with
                  | Some h -> let renderWithBreak st s =
                                let st = rend.AddText st s
                                rend.AddBreak st 0
                              let st = List.fold renderWithBreak st h
                              (0, y + List.length h, st)
                  | None   -> (x, y, st)
                (st, (x, y, m)) ;
            member r.AddText  ((st, (x, y, m))) text = (rend.AddText st text, (x + text.Length, y, m)) ;
            member r.AddBreak ((st, (x, y, m))) n = (rend.AddBreak st n, (n, y + 1, m)) ;
            member r.AddTag   ((st, ((x, y, m) as xySt))) (tag, attrs, start) = 
                let addToMap k v =
                  if m.ContainsKey(k) then () // this keeps the first binding that we find for an identifier
                  else m.Add(k,v)
                if start && tag = "goto:path" then
                      addToMap (List.map fst attrs) (x,y) 
                      (st, (x, y, m))
                else (rend.AddTag st (tag, attrs, start), xySt) ;
            member r.Finish ((st, (x, y, m))) = (rend.Finish st, (x, y, m)) } 

      /// given:
      /// initial state : (x : int * y : int * Map<full path : string list, c : int * r : int>)
      /// render a GotoDefinition-annotated AST and return a final state (mapping
      /// fully-qualified names to (x, y) positions in the rendered file                                                                    
      let showForGotoDefinition os showHeader st =
        let h =
          if showHeader
             then Some [ "// This file was automatically generated by a call to Goto Definition."
                         "#light"
                         ""
                       ]
             else None
        posTrackMappingBuildingR h st (channelR os) |> renderL
    
    type FsiGenerationResult = (string * Dictionary<string list, int * int> * string list) option

    /// Compute a probably-safe directory where .fsi's can be generated without
    /// interfering with user files. We'll create a well-known-named directory
    /// in the system-reported temp path.
    let PathForGeneratedVisualStudioFSharpTempFiles =
        let p = Filename.concat (Path.GetTempPath ()) "MicrosoftVisualStudioFSharpTemporaryFiles"
        if not (Directory.Exists p)
           then Directory.CreateDirectory p |> ignore
        p

    /// For an assembly stored in `<fullpath-to>\<name>.dll`, generate the .fsi
    /// into `<project-path>\<name>.temp.fsi`
    let GeneratedFsiNameGenerator s =
        let baseName = PathForGeneratedVisualStudioFSharpTempFiles
        let extn     = ".temp.fsi"
        s |> Filename.basename |> Filename.chop_extension |> (fun x -> x + extn) |> Filename.concat baseName
      
    /// Generate an F# signature file for an assembly; this is intended for
    /// use with GotoDefinition
    ///
    /// nameFixer is a function to convert filenames to a canonical form
    /// s         is the name of the .dll for which an .fsi ought to be
    ///           generated
    let GenerateFsiFile (tcConfig:TcConfig,tcGlobals,tcImports:TcImports,gotoCache) nameFixer s =

      let denv      = empty_denv tcGlobals
      let denv      = { denv with
                             showImperativeTyparAnnotations = true ;
                             showAttributes                 = true ;
                             openTopPaths                   = [ lib_MF_path
                                                                lib_MFCore_path
                                                                lib_MFColl_path
                                                                lib_MFControl_path
                                                                Il.split_namespace lib_FSLib_Pervasives_name
                                                                Il.split_namespace lib_MLLib_OCaml_name
                                                                Il.split_namespace lib_MLLib_FSharp_name
                                                                Il.split_namespace lib_MLLib_Pervasives_name
                                                              ]
                      }.Normalize ()

      let fixedName = nameFixer s
      match Map.tryfind fixedName !gotoCache with
      | Some (Some (outName, _, _) as res) when Internal.Utilities.FileSystem.File.SafeExists outName -> res
      | Some None                                                   -> None
      | _                                                           -> 
         let res =
             let s            = fixedName
             let outName      = GeneratedFsiNameGenerator s

             let relevantCcus         = 
                 tcImports.GetCcuInfos () 
                    |> List.map (fun asm -> asm.FSharpViewOfMetadata)
                    |> List.filter (fun ccu -> 
                                 match ccu.FileName with
                                  | Some s' -> nameFixer s' = s
                                  | None    -> false)

             let writeModul   isFirst os st (ccu:ccu) = 
                ccu.Contents |> NicePrint.AssemblyL denv |> Renderer.showForGotoDefinition os isFirst st |> snd

             match relevantCcus with
             | []      -> None
             | c :: cs -> 
                if Internal.Utilities.FileSystem.File.SafeExists outName
                   then File.SetAttributes (outName, FileAttributes.Temporary)
                        File.Delete outName

                let outFile = File.CreateText outName
                let outStrm = outFile :> System.IO.TextWriter
                let initSt  = (0, 0, new Dictionary<_,_>())

                let st              = writeModul true outStrm initSt c
                let (_, _, mapping) = List.fold (writeModul false outStrm) st cs

                outFile.Close ()
                File.SetAttributes (outName, FileAttributes.Temporary ||| FileAttributes.ReadOnly)

                Some (outName, mapping, tcConfig.referencedDLLs |> List.map (fun r -> nameFixer r.Text) )
         gotoCache := Map.add fixedName res !gotoCache
         res
    
// ------------------------------------------------------------------------------------------
// The incremental build definition for parsing and typechecking F#
// ------------------------------------------------------------------------------------------
module IncrementalFSharpBuild =

    open Internal.Utilities
    open Internal.Utilities.Collections

    open IncrementalBuild
    open Microsoft.FSharp.Compiler.Build
    open Microsoft.FSharp.Compiler.Fscopts
    open Microsoft.FSharp.Compiler.Ast
    open Microsoft.FSharp.Compiler.ErrorLogger
    open Microsoft.FSharp.Compiler.Env
    open Microsoft.FSharp.Compiler.TypeChecker
    open Microsoft.FSharp.Compiler.Tast 
    open Microsoft.FSharp.Compiler.Range
    open Microsoft.FSharp.Compiler
    open Microsoft.FSharp.Compiler.AbstractIL.Internal

    module Tc = Microsoft.FSharp.Compiler.TypeChecker

    open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics 
    open Internal.Utilities.Debug
    
    /// Callbacks for things that happen in the build.                  
    type BuildEvents = 
        { BeforeTypeCheckFile: string -> unit }
        static member Default = { BeforeTypeCheckFile=fun filename->() }
    
    type FileDependency = {
            // Name of the file
            Filename : string
            // If true, then deletion or creation of this file should trigger an entirely fresh build
            ExistenceDependency : bool
            // If true, then changing this file should trigger and call to incrementally build
            IncrementalBuildDependency : bool }        

    /// Accumulated results of type checking.
    type TypeCheckAccumulator = {
        tcState:tcState;
        tcImports:TcImports;
        tcGlobals:TcGlobals;
        tcConfig:TcConfig;
        tcEnv:tcEnv;
        topAttribs:topAttribs option;
        typedImplFiles:TypedImplFile list;
    }

    /// Maximum time share for a piece of background work before it should (cooperatively) yield
    /// to enable other requests to be serviced. Yielding means returning a continuation function
    /// (via an Eventually<_> value of case NotYetDone) that can be called as the next piece of work. 
    let maxTimeShareMilliseconds = 
        match System.Environment.GetEnvironmentVariable("mFSharp_MaxTimeShare") with 
        | null | "" -> 50L
        | s -> int64 s

      
    /// Global service state
    let private frameworkTcImportsCache = AgedLookup<(*resolvedpath*)string list * string * (*ClrRoot*)string list* (*fsharpBinaries*)string,(TcGlobals * TcImports)>(8) 

    /// This function strips the "System" assemblies from the tcConfig and returns a age-cached TcImports for them.
    let GetFrameworkTcImports(tcConfig:TcConfig) =
        // Split into installed and not installed.
        let frameworkDLLs,nonFrameworkResolutions,unresolved = TcAssemblyResolutions.SplitNonFoundationalResolutions(tcConfig)
        let frameworkDLLsKey = 
            frameworkDLLs 
            |> List.map(fun ar->ar.resolvedPath) // The cache key. Just the minimal data.
            |> List.sort  // Sort to promote cache hits.
        let tcGlobals,frameworkTcImports = 
            // Prepare the frameworkTcImportsCache
            //
            // The data elements in this key are very important. There should be nothing else in the TcConfig that logically affects
            // the import of a set of framework DLLs into F# CCUs. That is, the F# CCUs that result from a set of DLLs (including
            // FSharp.Core.dll andb mscorlib.dll) must be logically invariant of all the other compiler configuration parameters.
            let key = (frameworkDLLsKey,
                       tcConfig.mscorlibAssemblyName, 
                       tcConfig.ClrRoot,
                       tcConfig.fsharpBinariesDir)
            match frameworkTcImportsCache.TryGet key with 
            | Some(res)-> res
            | None -> 
                let tcConfigP = TcConfigProvider.Constant(tcConfig)
                let ((tcGlobals,tcImports) as res) = TcImports.BuildFrameworkTcImports (tcConfigP,frameworkDLLs)
                frameworkTcImportsCache.Put(key,res)
                tcGlobals,tcImports
        tcGlobals,frameworkTcImports,nonFrameworkResolutions,unresolved

    (*------------------------------------------------------------------------------------
     * Rules for reactive building.
     *
     * This phrases the compile as a series of vector functions and vector manipulations.
     * Rules written in this language are then transformed into a plan to execute the 
     * various steps of the process (possible in parallel).
     *-----------------------------------------------------------------------------------*)

    let Create (tcConfig : TcConfig, projectDirectory : string, assemblyName, niceNameGen, resourceManager,
                sourceFiles:string list, ensureReactive, buildEvents:BuildEvents, errorLogger:ErrorLogger, 
                errorRecovery : exn -> range -> unit)
               =
        use t = Trace.Call("IncrementalBuildVerbose", "Create", fun _ -> sprintf " tcConfig.includes = %A" tcConfig.includes)
        
        let tcConfigP = TcConfigProvider.Constant(tcConfig)
        

        /// An error logger that captures errors and eventually sends a single error or warning for all the errors and warning in a file
        let CompilationErrorLogger(sourceRange) = 
            
            let warningsSeenInScope = new ResizeArray<exn>()
            let errorsSeenInScope = new ResizeArray<exn>()

            let errorLogger = 
                { new ErrorLogger with
                    member x.WarnSink(exn) = 
                        warningsSeenInScope.Add(exn)
                        errorLogger.WarnSink(exn)
                    member x.ErrorSink(exn) = 
                        errorsSeenInScope.Add(exn)
                        errorLogger.ErrorSink(exn)
                    member x.ErrorCount = errorLogger.ErrorCount }

            let reportErrors () =
                let warns = warningsSeenInScope |> ResizeArray.to_list
                let errs = errorsSeenInScope |> ResizeArray.to_list
                if (warns.Length <> 0 || errs.Length <> 0) && (sourceRange <> rangeStartup) then
                    // Need to reoprt issues associated with a hashload file.
                    if errs.Length = 0 then warning(HashLoadedSourceHasIssues(warns,errs,sourceRange))
                    else errorR(HashLoadedSourceHasIssues(warns,errs,sourceRange))             
            // Return the error logger and a function to run when we want the errors reported
            errorLogger,reportErrors


        /// Use to reset error and warning handlers            
        let CompilationGlobalsScope(errorLogger) = 
            let savedEnvSink = !(Nameres.GlobalTypecheckResultsSink)
#if TRYING_TO_FIX_4577
#else
            let savedDirectory = System.IO.Directory.GetCurrentDirectory()
#endif
            Nameres.GlobalTypecheckResultsSink := None
#if TRYING_TO_FIX_4577
#else
            System.IO.Directory.SetCurrentDirectory(projectDirectory)
#endif
            let unwind2 = InstallGlobalErrorLogger (fun _ -> errorLogger)
            // Return the disposable object that cleans up
            {new IDisposable with
                member d.Dispose() = 
                    unwind2.Dispose();
#if TRYING_TO_FIX_4577
#else
                    System.IO.Directory.SetCurrentDirectory(savedDirectory)
#endif
                    Nameres.GlobalTypecheckResultsSink:=savedEnvSink}
                            
        
        let CompilationGlobalsAndErrorLoggerScopeWithSourceRange(sourceRange) = 
            let errorLogger,reportErrors = CompilationErrorLogger(sourceRange)
            let unwind2 = CompilationGlobalsScope (errorLogger)
            // Return the disposable object that cleans up
            errorLogger,
            {new IDisposable with
                member d.Dispose() = 
                    unwind2.Dispose();
                    reportErrors() }

        let CompilationGlobalsAndErrorLoggerScope() = 
            CompilationGlobalsAndErrorLoggerScopeWithSourceRange(rangeStartup)

        // Strip out and cache a level of "system" references.
        let tcGlobals,frameworkTcImports,nonFrameworkResolutions,unresolvedReferences = GetFrameworkTcImports(tcConfig)
        
        // Check for the existence of loaded sources and prepend them to the sources list if present.
        let sourceFiles = tcConfig.GetAvailableLoadedSources() @ (sourceFiles|>List.map(fun s -> rangeStartup,s))
        // Mark up the source files with an indicator flag indicating if they are the last source file in the project
        let sourceFiles = 
            let flags = tcConfig.ComputeCanContainEntryPoint(sourceFiles |> List.map snd)
            (sourceFiles,flags) ||> List.map2 (fun (m,nm) flag -> (m,nm,flag))
        
        // Get the original referenced assembly names
        System.Diagnostics.Debug.Assert(not((sprintf "%A" nonFrameworkResolutions).Contains("System.dll")),sprintf "Did not expect a system import here. %A" nonFrameworkResolutions)
        
        // Keep this around for disposing
        let scopeOfPriorTcImports : TcImports option ref = ref None
        
        /// Get the timestamp of the given file name.
        let StampFilename (m:range,filename:string,canContainEntryPoint:bool) =
            File.GetLastWriteTime(filename)
                            
        /// Parse the given files and return the given inputs. This function is expected to be
        /// able to be called with a subset of sourceFiles and return the corresponding subset of
        /// parsed inputs. 
        let Parse (sourceRange:range,filename:string,canContainEntryPoint) =
            let errorLogger, sDisposable = CompilationGlobalsAndErrorLoggerScopeWithSourceRange(sourceRange)
            use s = sDisposable
            Trace.Print("FSharpBackgroundBuild", fun _ -> sprintf "Parsing %s..." filename)
            
            try  
                let result = ParseOneInputFile(tcConfig,resourceManager,[],filename ,canContainEntryPoint,errorLogger)
                Trace.PrintLine("FSharpBackgroundBuildVerbose", fun _ -> sprintf "done.")
                result,sourceRange,filename
            with e -> 
                System.Diagnostics.Debug.Assert(false, sprintf "unexpected failure in IncrementalFSharpBuild.Parse\nerror = %s" (e.ToString()))
                failwith "last chance failure"  
                
        /// Get the names of all referenced assemblies.
        let GetReferencedAssemblyNames _ : (range*string*DateTime) array =
            let errorLogger, sDisposable = CompilationGlobalsAndErrorLoggerScope()
            use s = sDisposable
                                    
            let result = 
                nonFrameworkResolutions
                         |> List.map(fun r ->
                            let originaltimestamp = 
                                try 
                                    if Internal.Utilities.FileSystem.File.SafeExists(r.resolvedPath) then
                                        let result = File.GetLastWriteTime(r.resolvedPath)
                                        Trace.Print("FSharpBackgroundBuildVerbose", fun _ -> sprintf "Found referenced assembly '%s'.\n" r.resolvedPath)
                                        result
                                    else
                                        Trace.Print("FSharpBackgroundBuildVerbose", fun _ -> sprintf "Did not find referenced assembly '%s' on disk.\n" r.resolvedPath)
                                        DateTime.Now                               
                                with e -> 
                                    Trace.Print("FSharpBackgroundBuildVerbose", fun _ -> sprintf "Did not find referenced assembly '%s' due to exception.\n" r.resolvedPath)
                                    errorLogger.WarnSink(e)
                                    DateTime.Now                               
                            r.originalReference.Range,r.resolvedPath,originaltimestamp) 
                         |> List.to_array
            result
            
        
        /// Timestamps of referenced assemblies are taken from the file's timestamp.
        let TimestampReferencedAssembly (range,filename,originaltimestamp) =
            let errorLogger, sDisposable = CompilationGlobalsAndErrorLoggerScope()
            use s = sDisposable
            let timestamp = 
                try
                    if Internal.Utilities.FileSystem.File.SafeExists(filename) then
                        let ts = File.GetLastWriteTime(filename)
                        if ts<>originaltimestamp then 
                            Trace.PrintLine("FSharpBackgroundBuildVerbose", fun _ -> sprintf "Noticing change in timestamp of file %s from %A to %A" filename originaltimestamp ts)
                        else    
                            Trace.PrintLine("FSharpBackgroundBuildVerbose", fun _ -> sprintf "Noticing no change in timestamp of file %s (still %A)" filename originaltimestamp)
                        ts
                    else
                        Trace.PrintLine("FSharpBackgroundBuildVerbose", fun _ -> sprintf "Noticing that file %s was deleted, but ignoring that for timestamp checking" filename)
                        originaltimestamp
                with e -> 
                    // For example, malformed filename
                    Trace.PrintLine("FSharpBackgroundBuildVerbose", fun _ -> sprintf "Exception when checking stamp of file %s, using old stamp %A" filename originaltimestamp)
                    errorLogger.WarnSink(e)
                    originaltimestamp                      
            timestamp
                
         
        // Link all the assemblies together and produce the input typecheck accumulator               
        let CombineImportedAssemblies _ : TypeCheckAccumulator =
            let errorLogger, sDisposable = CompilationGlobalsAndErrorLoggerScope()
            use s = sDisposable
            
            let tcImports = 
                try
                    Trace.PrintLine("FSharpBackgroundBuild", fun _ -> "About to (re)create tcImports")
                    let tcImports = TcImports.BuildNonFrameworkTcImports(tcConfigP,tcGlobals,frameworkTcImports,nonFrameworkResolutions)  
                    Trace.PrintLine("FSharpBackgroundBuild", fun _ -> "(Re)created tcImports")
                    tcImports
                with e -> 
                    System.Diagnostics.Debug.Assert(false, sprintf "Could not BuildAllReferencedDllTcImports %A" e)
                    Trace.PrintLine("FSharpBackgroundBuild", fun _ -> "Failed to recreate tcImports\n  %A")
                    errorLogger.WarnSink(e)
                    frameworkTcImports                               
                    
            let tcEnv0 = GetInitialTypecheckerEnv (Some assemblyName) rangeStartup tcConfig tcImports tcGlobals
            let tcState0 = TypecheckInitialState (rangeStartup,assemblyName,tcConfig,tcGlobals,niceNameGen,tcEnv0)
            let tcAcc = {
                tcGlobals=tcGlobals
                tcImports=tcImports
                tcState=tcState0
                tcConfig=tcConfig
                tcEnv = tcEnv0
                topAttribs=None
                typedImplFiles=[]
                }   
            tcAcc    
                

        
        /// Type check all files.     
        let TypeCheck (tcAcc:TypeCheckAccumulator) input =    
            match input with 
            | Some(input),sourceRange,filename->
                
                let errorLogger,reportErrors = CompilationErrorLogger(sourceRange)
                let errorLogger = GetErrorLoggerFilteringByScopedPragmas(false,GetScopedPragmasForInput(input),errorLogger)
                let fullComputation = 
                    eventually {
                        try 
                            Trace.PrintLine("FSharpBackgroundBuild", fun _ -> sprintf "Typechecking %s..." filename)                
                            buildEvents.BeforeTypeCheckFile(filename)
                            let! (tcEnv,topAttribs,typedImplFiles),tcState = TypecheckOneInputEventually(fun () -> errorLogger.ErrorCount = 0) tcConfig tcAcc.tcImports tcAcc.tcGlobals None tcAcc.tcState input
                            Trace.PrintLine("FSharpBackgroundBuild", fun _ -> sprintf "done.")
                            return {tcAcc with tcState=tcState; tcEnv=tcEnv; topAttribs=Some(topAttribs); typedImplFiles=typedImplFiles } 
                        finally 
                            reportErrors()
                    }
                    
                // Run part of the Eventually<_> computation until a timeout is reached. If not complete, 
                // return a new Eventually<_> computation which recursively runs more of the computation.
                //   - When the whole thing is finished commit the error results sent through the errorLogger.
                //   - Each time we do real work we reinstall the CompilationGlobalsScope
                if ensureReactive then 
                    let timeSlicedComputation = 
                        fullComputation |> 
                           Eventually.repeatedlyProgressUntilDoneOrTimeShareOver 
                              maxTimeShareMilliseconds
                              (fun f -> 
                                  // Reinstall the compilation globals each time we start or restart
                                  use unwind = CompilationGlobalsScope (errorLogger) 
                                  Trace.Print("FSharpBackgroundBuildVerbose", fun _ -> sprintf "continuing %s.\n" filename)
                                  f())
                               
                    timeSlicedComputation
                else 
                    use unwind = CompilationGlobalsScope (errorLogger) 
                    fullComputation |> Eventually.force |> Eventually.Done 
            | _ -> 
                Eventually.Done tcAcc
                
        /// Finish up the typechecking to produce outputs for the rest of the compilation process
        let FinalizeTypeCheck (tcStates:TypeCheckAccumulator array) = 
            Trace.PrintLine("FSharpBackgroundBuildVerbose", fun _ -> sprintf "Finalizing Type Check" )
            let finalAcc = tcStates.[tcStates.Length-1]
            let results : (tcEnv * topAttribs * TypedImplFile list) list = tcStates |> List.of_array |> List.map (fun acc-> acc.tcEnv, (Option.get acc.topAttribs), acc.typedImplFiles)
            let (tcEnvAtEndOfLastFile,topAttrs,mimpls),tcState = TypecheckMultipleInputsFinish (results,finalAcc.tcState)
            let tcState,tassembly = TypecheckClosedInputSetFinish (mimpls,tcState)
            tcState, topAttrs, tassembly, tcEnvAtEndOfLastFile, finalAcc.tcImports, finalAcc.tcGlobals, finalAcc.tcConfig

        let gotoCache = ref (Map.empty : Map<string, FsiGeneration.FsiGenerationResult>) // avoid regenerating the same file
        
        let unresolvedFileDependencies = 
            unresolvedReferences
            |> List.map (function Microsoft.FSharp.Compiler.Build.UnresolvedReference(referenceText,ranges) -> referenceText)
            |> List.map (fun file->{Filename =  file; ExistenceDependency = true; IncrementalBuildDependency = true })
        let resolvedFileDependencies = 
            nonFrameworkResolutions |> List.map (fun r -> {Filename =  r.resolvedPath ; ExistenceDependency = true; IncrementalBuildDependency = true })
        let fileDependencies = List.concat [unresolvedFileDependencies;resolvedFileDependencies]
#if DEBUG
        resolvedFileDependencies |> List.iter (fun x -> System.Diagnostics.Debug.Assert(System.IO.Path.IsPathRooted(x.Filename), sprintf "file dependency should be absolute path: '%s'" x.Filename))
#endif        
#if TRACK_DOWN_EXTRA_BACKSLASHES        
        fileDependencies |> List.iter(fun dep ->
            Debug.Assert(not(dep.Filename.Contains(@"\\")), "IncrementalBuild.Create results in a non-canonical filename with extra backslashes: "^dep.Filename)
            )
#endif        
        
        // ---------------------------------------------------------------------------------------------            
        let build                       = new BuildScope ()

        // Inputs
        let filenames                   = InputVector<range*string*bool> "Filenames"
        let emptysource                 = InputScalar<int> "EmptySource"
        
        // Build
        let stampedFilenames            = Vector.Stamp "SourceFileTimeStamps" StampFilename filenames
        let parseTrees                  = Vector.Map "Parse" Parse stampedFilenames
        let scalarParseTree             = Vector.AsScalar "ScalarizeParseTrees" parseTrees
        let triggerOfParseTrees         = Scalar.Map "TriggerOfParseTrees" (fun _ -> 1) scalarParseTree // Create a timestamped trigger from the parse trees.
        let referencedAssemblies        = Scalar.Multiplex "GetReferencedAssemblyNames" GetReferencedAssemblyNames triggerOfParseTrees
        let stampedReferencedAssemblies = Vector.Stamp "TimestampReferencedAssembly" TimestampReferencedAssembly referencedAssemblies
        let initialTcAcc                = Vector.Demultiplex "CombineImportedAssemblies" CombineImportedAssemblies stampedReferencedAssemblies
        let tcStates                    = Vector.ScanLeft "TypeCheck" TypeCheck initialTcAcc parseTrees
        let finalizedTypeCheck          = Vector.Demultiplex "FinalizeTypeCheck" FinalizeTypeCheck tcStates
        let generatedSignatureFiles     = Scalar.Map "GenerateSignatureFiles" (fun tcAcc -> FsiGeneration.GenerateFsiFile(tcAcc.tcConfig,tcAcc.tcGlobals, tcAcc.tcImports,gotoCache)) initialTcAcc

        // Outputs
        build.DeclareVectorOutput ("TypeCheckingStates",tcStates)
        build.DeclareScalarOutput ("InitialTcAcc", initialTcAcc)
        build.DeclareScalarOutput ("FinalizeTypeCheck", finalizedTypeCheck)
        build.DeclareScalarOutput ("GenerateSignatureFiles", generatedSignatureFiles)
        // ---------------------------------------------------------------------------------------------            
        build.GetConcreteBuild (["Filenames", sourceFiles.Length, sourceFiles |> List.map box], []), fileDependencies

    // Expose methods to operate on F# build in a strongly typed way----------------------------------
    
    let Step(build) = 
        IncrementalBuild.Step "TypeCheckingStates" build
    
    let EvalTypeCheckSlot(slotOfFile,build) = 
        let build = EvalSlot("InitialTcAcc",slotOfFile,build)  
        let build = EvalSlot("TypeCheckingStates",slotOfFile,build)  
        build
        
    let GetAntecedentTypeCheckResultsBySlot(slotOfFile,build) = 
        let result = 
            match slotOfFile with
            | (*first file*) 0 -> GetScalarResult<TypeCheckAccumulator>("InitialTcAcc",build)
            | _ -> GetVectorResultBySlot<TypeCheckAccumulator>("TypeCheckingStates",slotOfFile-1,build)  
        
        match result with
        | Some({tcState=tcState; tcGlobals=tcGlobals; tcConfig=tcConfig; tcImports=tcImports},timestamp)->
            Some(tcState,tcImports,tcGlobals,tcConfig,timestamp)
        | _->None
        
    let TypeCheck(build) = 
        let build = IncrementalBuild.Eval "FinalizeTypeCheck" build
        match GetScalarResult<Build.tcState * TypeChecker.topAttribs * Tast.TypedAssembly * TypeChecker.tcEnv * Build.TcImports * Env.TcGlobals * Build.TcConfig>("FinalizeTypeCheck",build) with
        | Some((tcState,topAttribs,TypedAssembly,tcEnv,tcImports,tcGlobals,tcConfig),ts)->build,tcState,topAttribs,TypedAssembly,tcEnv,tcImports,tcGlobals,tcConfig
        | None -> failwith "Build was not evaluated."
        
    let GetSlotOfFileName(filename:string,build:Build) =
        // Get the slot of the given file and force it to build.
        let CompareFileNames (_,f1,_) (_,f2,_) = 
            let result = 
                   System.String.Compare(f1,f2,StringComparison.CurrentCultureIgnoreCase)=0
                || System.String.Compare(Path.GetFullPath(f1),Path.GetFullPath(f2),StringComparison.CurrentCultureIgnoreCase)=0
            result
        GetSlotByInput("Filenames",(rangeStartup,filename,false),build,CompareFileNames)
        
    /// Get a list of on-demand generators of F# signature files for referenced assemblies.
    let GetFsiGenerators (build : Build) : ((string -> string) -> string -> FsiGeneration.FsiGenerationResult) * Build =
      let build = IncrementalBuild.Eval "GenerateSignatureFiles" build
      let gens  = match IncrementalBuild.GetScalarResult<_> ("GenerateSignatureFiles", build) with
                  | Some (gens, _) -> gens
                  | None           -> failwith "Build was not evaluated."
      (gens, build)
