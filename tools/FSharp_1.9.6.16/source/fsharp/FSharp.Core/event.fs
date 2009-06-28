//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Control

    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Control
    open System.Reflection
    open System.Diagnostics

#if FX_NO_DELEGATE_DYNAMIC_METHOD 
#else

#if FX_NO_DELEGATE_DYNAMIC_INVOKE 
    module Impl = 
        type System.Delegate with 
            member d.DynamicInvoke(args: obj[]) =
                d.Method.Invoke(d.Target, BindingFlags.Default |||  BindingFlags.Public ||| BindingFlags.NonPublic , null, args, null)

    open Impl
#endif
    
    type DelegateEvent<'Del when 'Del :> System.Delegate>() = 
        let mutable multicast : System.Delegate = null
        member x.Trigger(args:obj[]) = 
            match multicast with 
            | null -> ()
            | d -> d.DynamicInvoke(args) |> ignore
        member x.Publish = 
            { new IDelegateEvent<'Del> with 
                member x.AddHandler(d) =
                    multicast <- System.Delegate.Combine(multicast, d)
                member x.RemoveHandler(d) = 
                    multicast <- System.Delegate.Remove(multicast, d) }

    [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Event")>]
    type Event<'Del,'Args when 'Del : delegate<'Args,unit> and 'Del :> System.Delegate >() = 
        let mutable multicast : System.Delegate = null
        static let argTypes = 
            let instanceBindingFlags = BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.DeclaredOnly
            let mi = typeof<'Del>.GetMethod("Invoke",instanceBindingFlags)
            mi.GetParameters() |> (fun arr -> arr.[1..]) |> Array.map (fun p -> p.ParameterType)

        member x.Trigger(sender:obj,args:'Args) = 
            match multicast with 
            | null -> ()
            | d -> 
                if argTypes.Length = 1 then 
                    d.DynamicInvoke([| sender; box args |]) |> ignore
                else
                    d.DynamicInvoke(Array.append [| sender |] (Microsoft.FSharp.Reflection.FSharpValue.GetTupleFields(box args))) |> ignore
        member x.Publish = 
            { new IEvent<'Del,'Args> with 
                member x.Add(f) =
                    multicast <- System.Delegate.Combine(multicast, new Handler<'Args>(fun sender arg -> f arg))
                member x.AddHandler(d) =
                    multicast <- System.Delegate.Combine(multicast, d)
                member x.RemoveHandler(d) = 
                    multicast <- System.Delegate.Remove(multicast, d) }
#endif

    [<CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId="Event")>]
    type Event<'T>() = 
        let mutable multicast : Handler<'T> option = None
        member x.Delegate
                 with get () =  match multicast with None -> null | Some(d) -> (d :> System.Delegate)
                 and  set v =  multicast <- (match v with null -> None | d -> Some d)
        member x.Trigger(arg:'T) = 
            match multicast with 
            | None -> ()
            | Some d -> d.Invoke(null,arg) |> ignore
        member x.Publish = 
            { new IEvent<'T> with 
                member e.AddHandler(d) =
                    x.Delegate <- (System.Delegate.Combine(x.Delegate, d) :?> Handler<'T>)
                member e.RemoveHandler(d) = 
                    x.Delegate <- (System.Delegate.Remove(x.Delegate, d) :?> Handler<'T>)
                member e.Add(f:'T -> unit) = 
                    x.Delegate <- (System.Delegate.Combine(x.Delegate, new Handler<'T>(fun sender arg -> f arg)) :?> Handler<'T>) }

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Event =
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let create<'T>() = 
            let ev = new Event<'T>() 
            ev.Trigger, ev.Publish

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let map f (w: IEvent<'Del,'T>) =
            let ev = new Event<_>() 
            w.Add(fun x -> ev.Trigger(f x));
            ev.Publish

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let filter f (w: IEvent<'Del,'T>) =
            let ev = new Event<_>() 
            w.Add(fun x -> if f x then ev.Trigger x);
            ev.Publish

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let partition f (w: IEvent<'Del,'T>) =
            let ev1 = new Event<_>() 
            let ev2 = new Event<_>() 
            w.Add(fun x -> if f x then ev1.Trigger x else ev2.Trigger x);
            ev1.Publish,ev2.Publish

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let choose f (w: IEvent<'Del,'T>) =
            let ev = new Event<_>() 
            w.Add(fun x -> match f x with None -> () | Some r -> ev.Trigger r);
            ev.Publish

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let scan f z (w: IEvent<'Del,'T>) =
            let state = ref z
            let ev = new Event<_>() 
            w.Add(fun msg ->
                 let z = !state
                 let z = f z msg
                 state := z; 
                 ev.Trigger(z));
            ev.Publish

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let listen f (w: IEvent<'Del,'T>) = w.Add(f)

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]
        let pairwise (inp : IEvent<'Del,'T>) : IEvent<'T * 'T> = 
            let ev = new Event<'T * 'T>() 
            let lastArgs = ref None
            inp.Add(fun args2 -> 
                (match !lastArgs with 
                 | None -> () 
                 | Some args1 -> ev.Trigger(args1,args2));
                lastArgs := Some args2); 

            ev.Publish

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let merge (w1: IEvent<'Del1,'T>) (w2: IEvent<'Del2,'T>) =
            let ev = new Event<_>() 
            w1.Add(fun x -> ev.Trigger(x));
            w2.Add(fun x -> ev.Trigger(x));
            ev.Publish

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let split (f : 'T -> Choice<'U1,'U2>) (w: IEvent<'Del,'T>) =
            let ev1 = new Event<_>() 
            let ev2 = new Event<_>() 
            w.Add(fun x -> match f x with Choice1Of2 y -> ev1.Trigger(y) | Choice2Of2 z -> ev2.Trigger(z));
            ev1.Publish,ev2.Publish

