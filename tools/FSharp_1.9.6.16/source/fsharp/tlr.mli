// (c) Microsoft Corporation. All rights reserved
#light

module internal Microsoft.FSharp.Compiler.Tlr 

open Microsoft.FSharp.Compiler 

val MakeTLRDecisions : Tast.ccu -> Env.TcGlobals -> Tast.TypedImplFile -> Tast.TypedImplFile
val liftTLR : bool ref