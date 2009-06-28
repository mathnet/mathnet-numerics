
/// Functions associated with writing binaries which 
/// vary between supported implementations of the CLI Common Language 
/// Runtime, e.g. between the SSCLI, Mono and the Microsoft CLR.
///
/// The implementation of the functions can be found in ilsupp-*.ml
// (c) Microsoft Corporation 2005-2009.
module Microsoft.FSharp.Compiler.AbstractIL.Internal.Support
open System
open System.Runtime.InteropServices
open System.Diagnostics.SymbolStore
open Internal.Utilities
open Microsoft.FSharp.Compiler.AbstractIL
open Microsoft.FSharp.Compiler.AbstractIL.Internal

module Il = Microsoft.FSharp.Compiler.AbstractIL.IL 

open Il

type IStream = System.Runtime.InteropServices.ComTypes.IStream

type clr_kind = 
  | SSCLI (* all calls allowed *)
  | Microsoft (* all calls allowed *)
  | Mono (* not investigated *)
  | Neutral  (* no PDB writing, reading, strong name signing, clrInstallationDirectory calls allowed *)

/// Globally configure the Abstract IL toolset to use a particular
/// set of auxiliary tools, e.g. debug writer, strong-name signer etc.
/// 
/// The configurations available will depend on the flags you've
/// set when compiling the Abstract IL source code, or will depend
/// on the particular binary release you are using.  For example,
/// a typical Windows x86 binary release will support all Microsoft
/// CLR implementations and the Rotor SSCLI implementation.
val configure: clr_kind -> unit
val current_configuration: unit -> clr_kind

/// detect which configurations Abstract IL was compiled to support 
val supported_configurations: clr_kind list

val pdb_suffix_for_configuration: clr_kind -> string

/// Unmanaged resource file linker - for native resources (not managed ones).
/// The function may be called twice, once with a zero-RVA and
/// arbitrary buffer, and once with the real buffer.  The size of the
/// required buffer is returned.
type PEFileType = X86 | X64

val linkNativeResources: unlinkedResources:byte[] list ->  rva:int32 -> PEFileType -> tempFilePath:string -> byte[]
val unlinkResource: int32 -> byte[] -> byte[]

/// PDB reader and associated types
type pdb_reader
type pdb_document
type pdb_method
type pdb_variable
type pdb_method_scope

type pdb_sequence_point = 
    { pdbSeqPointOffset: int;
      pdbSeqPointDocument: pdb_document;
      pdbSeqPointLine: int;
      pdbSeqPointColumn: int;
      pdbSeqPointEndLine: int;
      pdbSeqPointEndColumn: int; }

val pdbReadOpen: string (* module *) -> string (* path *) -> pdb_reader
val pdbReadClose: pdb_reader -> unit
val pdbReaderGetMethod: pdb_reader -> int32 (* token *) -> pdb_method
val pdbReaderGetMethodFromDocumentPosition: pdb_reader -> pdb_document -> int (* line *) -> int (* col *) -> pdb_method
val pdbReaderGetDocuments: pdb_reader -> pdb_document array
val pdbReaderGetDocument: pdb_reader -> string (* url *) -> byte[] (* guid *) -> byte[] (* guid *) -> byte[] (* guid *) -> pdb_document

val pdbDocumentGetURL: pdb_document -> string
val pdbDocumentGetType: pdb_document -> byte[] (* guid *)
val pdbDocumentGetLanguage: pdb_document -> byte[] (* guid *)
val pdbDocumentGetLanguageVendor: pdb_document -> byte[] (* guid *)
val pdbDocumentFindClosestLine: pdb_document -> int -> int

val pdbMethodGetToken: pdb_method -> int32
val pdbMethodGetRootScope: pdb_method ->  pdb_method_scope
val pdbMethodGetSequencePoints: pdb_method -> pdb_sequence_point array

val pdbScopeGetChildren: pdb_method_scope -> pdb_method_scope array
val pdbScopeGetOffsets: pdb_method_scope -> int * int
val pdbScopeGetLocals: pdb_method_scope -> pdb_variable array

val pdbVariableGetName: pdb_variable -> string
val pdbVariableGetSignature: pdb_variable -> byte[]
val pdbVariableGetAddressAttributes: pdb_variable -> int32 (* kind *) * int32 (* addrField1 *)

/// Access installation directory.  Not actually used by the core
/// Abstract IL libraries but invariably useful to client programs
/// when setting up paths etc.
///
/// This returns "." is a CLR installation cannot be detected.
val clrInstallationDirectory: unit -> string
val clrVersion: unit -> string

(*---------------------------------------------------------------------
 * PDB writer.
 *---------------------------------------------------------------------*)

type pdb_writer
type pdb_document_writer

type idd =
    { iddCharacteristics: int32;
      iddMajorVersion: int32; (* actually u16 in IMAGE_DEBUG_DIRECTORY *)
      iddMinorVersion: int32; (* acutally u16 in IMAGE_DEBUG_DIRECTORY *)
      iddType: int32;
      iddData: byte[];}

val pdbInitialize: 
    string (* .exe/.dll already written and closed *) -> 
    string  (* .pdb to write *) ->
    pdb_writer
val pdbClose: pdb_writer -> unit
val pdbSetUserEntryPoint: pdb_writer -> int32 -> unit
val pdbDefineDocument: pdb_writer -> string -> pdb_document_writer
val pdbOpenMethod: pdb_writer -> int32 -> unit
val pdbCloseMethod: pdb_writer -> unit
val pdbOpenScope: pdb_writer -> int -> unit
val pdbCloseScope: pdb_writer -> int -> unit
val pdbDefineLocalVariable: pdb_writer -> string -> byte[] -> int32 -> unit
val pdbSetMethodRange: pdb_writer -> pdb_document_writer -> int -> int -> pdb_document_writer -> int -> int -> unit
val pdbDefineSequencePoints: pdb_writer -> pdb_document_writer -> (int * int * int * int * int) array -> unit
val pdbGetDebugInfo: pdb_writer -> idd

(*---------------------------------------------------------------------
 * Misc writing support
 *---------------------------------------------------------------------*)

val absilWriteGetTimeStamp: unit -> int32

(*---------------------------------------------------------------------
 * Strong name signing
 *---------------------------------------------------------------------*)

type keyContainerName = string
type keyPair = byte[]
type pubkey = byte[]

val signerOpenPublicKeyFile: string -> pubkey 
val signerOpenKeyPairFile: string -> keyPair 
val signerGetPublicKeyForKeyPair: keyPair -> pubkey 
val signerGetPublicKeyForKeyContainer: string -> pubkey 
val signerCloseKeyContainer: keyContainerName -> unit 
val signerSignatureSize: pubkey -> int 
val signerSignFileWithKeyPair: string -> keyPair -> unit 
val signerSignFileWithKeyContainer: string -> keyContainerName -> unit 
