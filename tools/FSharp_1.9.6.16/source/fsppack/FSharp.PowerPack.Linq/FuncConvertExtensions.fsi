// (c) Microsoft Corporation 2005-2009. 

namespace Microsoft.FSharp
    open System

    [<Sealed; AbstractClass>]
    type FuncConvertExtensions =

        [<OverloadID("Func0")>]
        static member  ToFastFunc       : Func<'U>     -> (unit -> 'U)
        
        [<OverloadID("Func1")>]
        static member  ToFastFunc       : Func<'T1,'U>     -> ('T1 -> 'U)
        
        [<OverloadID("Action2")>]
        static member  ToFastFunc       : Action<'T1,'T2>           -> ('T1 -> 'T2 -> unit)
        
        [<OverloadID("Func2")>]
        static member  ToFastFunc       : Func<'T1,'T2,'U>     -> ('T1 -> 'T2 -> 'U)
        
        [<OverloadID("Action3")>]
        static member  ToFastFunc       : Action<'T1,'T2,'T3>       -> ('T1 -> 'T2 -> 'T3 -> unit)
        
        [<OverloadID("Func3")>]
        static member  ToFastFunc       : Func<'T1,'T2,'T3,'U> -> ('T1 -> 'T2 -> 'T3 -> 'U)

        [<OverloadID("Action4")>]
        static member  ToFastFunc       : Action<'T1,'T2,'T3,'T4>       -> ('T1 -> 'T2 -> 'T3 -> 'T4 -> unit)
        
        [<OverloadID("Func4")>]
        static member  ToFastFunc       : Func<'T1,'T2,'T3,'T4,'U> -> ('T1 -> 'T2 -> 'T3 -> 'T4 -> 'U)

        [<OverloadID("Func0")>]
        static member  ToTupledFastFunc : Func<'U>     -> (unit -> 'U)
        
        [<OverloadID("Func1")>]
        static member  ToTupledFastFunc : Func<'T1,'U>     -> ('T1 -> 'U)
        
        [<OverloadID("Action2")>]
        static member  ToTupledFastFunc : Action<'T1,'T2>           -> ('T1 * 'T2 -> unit)
        
        [<OverloadID("Func2")>]
        static member  ToTupledFastFunc : Func<'T1,'T2,'U>     -> ('T1 * 'T2 -> 'U)
        
        [<OverloadID("Action3")>]
        static member  ToTupledFastFunc : Action<'T1,'T2,'T3>       -> ('T1 * 'T2 * 'T3 -> unit)
        
        [<OverloadID("Func3")>]
        static member  ToTupledFastFunc : Func<'T1,'T2,'T3,'U> -> ('T1 * 'T2 * 'T3 -> 'U)

        [<OverloadID("Action4")>]
        static member  ToTupledFastFunc : Action<'T1,'T2,'T3,'T4>       -> ('T1 * 'T2 * 'T3 * 'T4 -> unit)
        
        [<OverloadID("Func4")>]
        static member  ToTupledFastFunc : Func<'T1,'T2,'T3,'T4,'U> -> ('T1 * 'T2 * 'T3 * 'T4 -> 'U)

