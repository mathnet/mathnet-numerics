// (c) Microsoft Corporation 2005-2009. 
#light

module Microsoft.FSharp.Compiler.UnicodeLexing

//------------------------------------------------------------------
// Functions for Unicode char-based lexing (new code).
//

open Internal.Utilities
open System.IO 

open Internal.Utilities.Text.Lexing

type Lexbuf =  LexBuffer<char>

let StringAsLexbuf (s:string) : Lexbuf =
    LexBuffer<char>.FromChars [| for c in s -> c |] 
  
let FunctionAsLexbuf (bufferFiller: char[] * int * int -> int) : Lexbuf =
    LexBuffer<char>.FromFunction bufferFiller 
    
     
/// Standard utility to create a Unicode LexBuffer
///
/// One small annoyance is that LexBuffers and not IDisposable. This means 
/// we can't just return the LexBuffer object, since the file it wraps wouldn't
/// get closed when we're finished with the LexBuffer. Hence we return the stream,
/// the reader and the LexBuffer. The caller should dispose the first two when done.
let UnicodeFileAsLexbuf (filename,codePage : int option) : FileStream * StreamReader * Lexbuf =
    // Use the .NET functionality to auto-detect the unicode encoding
    let stream  = Internal.Utilities.FileSystem.File.SafeNewFileStream(filename,FileMode.Open,FileAccess.Read,FileShare.Read) 
    let reader = 
        match codePage with 
        | None -> new  StreamReader(stream,true)
        | Some n -> new  StreamReader(stream,System.Text.Encoding.GetEncoding(n)) 
    let lexbuf = LexBuffer<char>.FromCharFunction(fun buf n -> reader.Read(buf,0,n))  
    stream, reader, lexbuf

