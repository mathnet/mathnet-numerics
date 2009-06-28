#light

// (c) Microsoft Corporation. All rights reserved
module Microsoft.FSharp.Compiler.Pickle 

open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.Tast

// Fixup pickled data w.r.t. a set of CCU thunks indexed by name
type PickledDataWithReferences<'rawData> = 
    { /// The data that uses a collection of CcuThunks internally
      RawData: 'rawData; 
      /// The assumptions that need to be fixed up
      FixupThunks: list<CcuThunk> } 

    member Fixup : (CcuReference -> ccu) -> 'rawData
    /// Like Fixup but loader may return None, in which case there is no fixup.
    member OptionalFixup: (CcuReference -> ccu option) -> 'rawData
    

type WriterState 
type ReaderState 

type 'a pickler = 'a -> WriterState -> unit
type 'a unpickler = ReaderState -> 'a

val internal p_byte : int -> WriterState -> unit
val internal u_byte : ReaderState -> int
val internal p_bool : bool -> WriterState -> unit
val internal u_bool : ReaderState -> bool
val internal p_int : int -> WriterState -> unit
val internal u_int : ReaderState -> int
val internal p_string : string -> WriterState -> unit
val internal u_string : ReaderState -> string
val internal p_lazy : 'a pickler -> Lazy<'a> pickler
val internal u_lazy : 'a unpickler -> Lazy<'a> unpickler

val inline  internal p_tup2 : ('a pickler) -> ('b pickler) -> ('a * 'b) pickler
val inline  internal p_tup3 : ('a pickler) -> ('b pickler) -> ('c pickler) -> ('a * 'b * 'c) pickler
val inline  internal p_tup4 : ('a pickler) -> ('b pickler) -> ('c pickler) -> ('d pickler) -> ('a * 'b * 'c * 'd) pickler
val inline  internal u_tup2 : ('b unpickler) -> ('c unpickler ) -> ('b * 'c) unpickler
val inline  internal u_tup3 : ('b unpickler) -> ('c unpickler ) -> ('d unpickler ) -> ('b * 'c * 'd) unpickler
val inline  internal u_tup4 : ('b unpickler) -> ('c unpickler ) -> ('d unpickler ) -> ('e unpickler) -> ('b * 'c * 'd * 'e) unpickler
val internal p_array : ('a pickler) -> 'a array pickler
val internal u_array : 'a unpickler -> 'a array unpickler
val internal p_namemap : ('a pickler) -> 'a Lib.NameMap pickler
val internal u_namemap : ('a unpickler) -> 'a Lib.NameMap unpickler

val pickle_obj_with_dangling_ccus : string -> Env.TcGlobals -> scope:ccu -> ('a pickler) -> 'a -> byte[]
val internal unpickle_obj_with_dangling_ccus : string -> viewedScope:ILScopeRef -> ('a  unpickler) -> byte[] ->  PickledDataWithReferences<'a>

val internal p_const : Constant pickler
val internal u_const : Constant unpickler
val internal p_vref : string -> ValRef pickler
val internal u_vref : ValRef unpickler
val internal p_tcref : string -> TyconRef pickler
val internal u_tcref : TyconRef unpickler
val internal p_ucref : UnionCaseRef pickler
val internal u_ucref : UnionCaseRef unpickler
val internal p_expr : expr pickler
val internal u_expr : expr unpickler
val internal p_typ : typ pickler
val internal u_typ : typ unpickler

val internal pickle_modul_spec : pickler<ModuleOrNamespace>
val internal unpickle_modul_spec : ReaderState -> ModuleOrNamespace
val internal PickleModuleInfo : pickler<PickledModuleInfo>
val internal UnpickleModuleInfo : ReaderState -> PickledModuleInfo
