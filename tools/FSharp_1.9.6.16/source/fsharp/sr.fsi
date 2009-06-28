#light

namespace Microsoft.FSharp.Compiler 
    
    module SR =
        val GetString : string -> string
        val GetObject : string -> System.Object
            
        
    module DiagnosticMessage =
        type ResourceString<'T> =
          new : string * Printf.StringFormat<'T> -> ResourceString<'T>
          member Format : 'T

        val DeclareResourceString : string * Printf.StringFormat<'T> -> ResourceString<'T>