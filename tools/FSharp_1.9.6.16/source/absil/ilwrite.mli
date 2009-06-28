(* -------------------------------------------------------------------- 
 * (c) Microsoft Corporation. All rights reserved 
 * -------------------------------------------------------------------- *)

/// The IL Binary writer 
module Microsoft.FSharp.Compiler.AbstractIL.BinaryWriter 

open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 

open Il

type signer

type options =
 { mscorlib: ILScopeRef;
   pdbfile: string option;
   signer : signer option;
   fixupOverlappingSequencePoints : bool }

/// Write a binary to the file system. Extra configuration parameters can also be specified. 
val WriteILBinary: 
    filename: string ->
    options:  options ->
    input:    ILModuleDef -> 
    unit

val signerPublicKey: signer -> byte[]
val signerOpenPublicKeyFile: string -> signer
val signerOpenPublicKey: byte[] -> signer
val signerOpenKeyPairFile: string -> signer
val signerOpenKeyContainer: string -> signer
val signerClose: signer -> unit
val signerSignatureSize: signer -> int
val signerSignFile: string -> signer -> unit
val signerFullySigned: signer -> bool

/// If this is set a report of times is sent to the Ildiag diagnostics channel
val showTimes : bool ref


