// (c) Microsoft Corporation. All rights reserved

module Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Msilxlib

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX

open Ilxsettings

let compiling_msilxlib_ref = ref false

let publickey = PublicKeyToken(Bytes.of_intarray [|103;241;202;169;071;205;028;035|])

let version = IL.parse_version Ilxconfig.version

let ilxLibraryAssemRef = ref (None : ILAssemblyRef option)

(* How are closures implemented? *)
let call_implementation = ref Ilxconfig.default_call_implementation
let tailcall_implementation = ref Ilxconfig.default_tailcall_implementation
let entrypoint_implementation = ref Ilxconfig.default_entrypoint_implementation


let assref nm = ILAssemblyRef.Create(nm, None, Some publickey,false, Some version, None)

let scoref () =
  if !compiling_msilxlib_ref then ScopeRef_local 
  else 
    ScopeRef_assembly 
      (match !ilxLibraryAssemRef with 
      | Some o -> o
      | None -> assref "FSharp.Core" (* failwith "ilxLibraryAssemRef is not set" *) )

let ilxNamespace () =  "Microsoft.FSharp.Core"

let minCLIMetadataVersion () = IL.parse_version "2.0.0"
        
