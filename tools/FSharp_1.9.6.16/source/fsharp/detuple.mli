// (c) Microsoft Corporation. All rights reserved

#light

module internal Microsoft.FSharp.Compiler.Detuple 

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.Tast

(* detuple pass: *)  
val DetupleImplFile : ccu -> Env.TcGlobals -> TypedImplFile -> TypedImplFile

module GlobalUsageAnalysis = 
    val GetValsBoundInExpr : expr -> Zset.set<Val>

    type accessor = PTup of int * typ list

    /// xinfo is "expr information".
    /// This could extend to be a full graph view of the expr.
    /// Later could support "safe" change operations, and optimisations could be in terms of those.
    type xinfo =
       { /// v -> context / APP inst args 
         xinfo_uses   : Zmap.map<Val,(accessor list * typ list * expr list) list>; 
         /// v -> binding repr 
         xinfo_eqns   : Zmap.map<Val,expr>;                                    
         /// bound in a decision tree? 
         xinfo_dtree    : Zset.set<Val>;                                              
         /// v -> v list * recursive? -- the others in the mutual binding 
         xinfo_mubinds  : Zmap.map<Val,(bool * FlatVals)>;                        
         /// val not defined under lambdas 
         xinfo_toplevel : Zset.set<Val>;                                            
         /// top of expr toplevel? (true) 
         xinfo_top      : bool;                                                         
       }
    val GetUsageInfoOfImplFile :  Env.TcGlobals -> TypedImplFile -> xinfo
