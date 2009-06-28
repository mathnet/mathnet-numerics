// (c) Microsoft Corporation 2005-2009. 


namespace Microsoft.FSharp
    open System
    open System.Linq

    [<Sealed; AbstractClass>]
    type FuncConvertExtensions = 

        [<OverloadID("Action2")>]
        static member  ToFastFunc( f : Action<_,_>) = (fun a1 a2 -> f.Invoke(a1,a2))
        [<OverloadID("Func0")>]
        static member  ToFastFunc( f : Func<_>) = (fun () -> f.Invoke())
        [<OverloadID("Func1")>]
        static member  ToFastFunc( f : Func<_,_>) = (fun a1 -> f.Invoke(a1))
        [<OverloadID("Func2")>]
        static member  ToFastFunc( f : Func<_,_,_>) = (fun a1 a2 -> f.Invoke(a1,a2))
        [<OverloadID("Action3")>]
        static member  ToFastFunc( f : Action<_,_,_>) = (fun a1 a2 a3 -> f.Invoke(a1,a2,a3))
        [<OverloadID("Func3")>]
        static member  ToFastFunc( f : Func<_,_,_,_>) = (fun a1 a2 a3 -> f.Invoke(a1,a2,a3))
        [<OverloadID("Action4")>]
        static member  ToFastFunc( f : Action<_,_,_,_>) = (fun a1 a2 a3 a4 -> f.Invoke(a1,a2,a3,a4))
        [<OverloadID("Func4")>]
        static member  ToFastFunc( f : Func<_,_,_,_,_>) = (fun a1 a2 a3 a4 -> f.Invoke(a1,a2,a3,a4))
        [<OverloadID("Func0")>]
        static member  ToTupledFastFunc( f : Func<_>) = (fun () -> f.Invoke())
        [<OverloadID("Func1")>]
        static member  ToTupledFastFunc( f : Func<_,_>) = (fun a1 -> f.Invoke(a1))
        [<OverloadID("Action2")>]
        static member  ToTupledFastFunc( f : Action<_,_>) = (fun (a1,a2) -> f.Invoke(a1,a2))
        [<OverloadID("Func2")>]
        static member  ToTupledFastFunc( f : Func<_,_,_>) = (fun (a1,a2) -> f.Invoke(a1,a2))
        [<OverloadID("Action3")>]
        static member  ToTupledFastFunc( f : Action<_,_,_>) = (fun (a1,a2,a3) -> f.Invoke(a1,a2,a3))
        [<OverloadID("Func3")>]
        static member  ToTupledFastFunc( f : Func<_,_,_,_>) = (fun (a1,a2,a3) -> f.Invoke(a1,a2,a3))
        [<OverloadID("Action4")>]
        static member  ToTupledFastFunc( f : Action<_,_,_,_>) = (fun (a1,a2,a3,a4) -> f.Invoke(a1,a2,a3,a4))
        [<OverloadID("Func4")>]
        static member  ToTupledFastFunc( f : Func<_,_,_,_,_>) = (fun (a1,a2,a3,a4) -> f.Invoke(a1,a2,a3,a4))


