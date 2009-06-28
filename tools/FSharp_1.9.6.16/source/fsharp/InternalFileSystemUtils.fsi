#light

namespace Internal.Utilities.FileSystem
  type File =
    class
      static member SafeExists : filename:string -> bool
      static member SafeNewFileStream : filename:string * mode:System.IO.FileMode * access:System.IO.FileAccess * share:System.IO.FileShare -> System.IO.FileStream
    end
  type Path =
    class
      static member SafeGetFullPath : filename:string -> string
    end    