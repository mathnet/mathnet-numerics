// (c) Microsoft Corporation. All rights reserved
module Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX.Ilxsettings 


type call_implementation = 
  | VirtEntriesVirtCode
#if CLOSURES_VIA_POINTERS
  | VirtEntriesPtrCode
#endif

type tailcall_implementation = 
  | AllTailcalls
  | NoTailcalls

type entrypoint_implementation = 
  | MultiEntryPoints

