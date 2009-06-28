// (c) Microsoft Corporation 2005-2009. 

#nowarn "9"
   
namespace Microsoft.FSharp.Compatibility

module Array2D = 
    
    open System.Runtime.InteropServices
    
    let inline pinObjUnscoped (obj: obj) =  
        GCHandle.Alloc(obj,GCHandleType.Pinned) 

    let inline pinObj (obj: obj) f = 
        let gch = pinObjUnscoped obj 
        try f gch
        finally
            gch.Free()

    [<NoDynamicInvocation>]
    let inline pin (arr: 'T [,]) (f : nativeptr<'T> -> 'U) = 
        pinObj (box arr) (fun _ -> f (NativeInterop.NativePtr.of_array2 arr 0 0))
    
    [<NoDynamicInvocation>]
    let inline pinUnscoped (arr: 'T [,]) : nativeptr<'T> * _ = 
        let gch = pinObjUnscoped (box arr) in 
        NativeInterop.NativePtr.of_array2 arr 0 0, gch

    [<NoDynamicInvocation>]
    let inline pin_unscoped arr = pinUnscoped arr
