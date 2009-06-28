// (c) Microsoft Corporation 2005-2009. 

#light


//[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>] // avoid calling the type "Shared" which is keyword in some languages
namespace Microsoft.FSharp.Compiler.Server.Shared

// For FSI VS plugin, require FSI to provide services:
// e.g.
//   - interrupt
//   - intelisense completion
// 
// This is done via remoting.
// Here we define the service class.
// This dll is required for both client (fsi-vs plugin) and server (spawned fsi).

//[<assembly: System.Security.SecurityTransparent>]
[<assembly: System.Runtime.InteropServices.ComVisible(false)>]
[<assembly: System.CLSCompliant(true)>]  
do()

open System
open System.Diagnostics
open System.Runtime.Remoting.Channels
open System.Runtime.Remoting
open System.Runtime.Remoting.Lifetime

[<AbstractClass>]
type FSharpInteractiveServer() =
    inherit System.MarshalByRefObject()  
    abstract Interrupt       : unit -> unit
    abstract Completions     : prefix:string -> string array
    abstract GetDeclarations : text:string * names:string array -> (string * string * string * int) array
    default x.Interrupt() = ()

    [<CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")>]
    static member StartServer(channelName:string,server:FSharpInteractiveServer) =
        let chan = new Ipc.IpcChannel(channelName) 
        LifetimeServices.LeaseTime            <- TimeSpan(7,0,0,0); // days,hours,mins,secs 
        LifetimeServices.LeaseManagerPollTime <- TimeSpan(7,0,0,0);
        LifetimeServices.RenewOnCallTime      <- TimeSpan(7,0,0,0);
        LifetimeServices.SponsorshipTimeout   <- TimeSpan(7,0,0,0);
        ChannelServices.RegisterChannel(chan,false);
        let objRef = RemotingServices.Marshal(server,"FSIServer") 
        ()

    static member StartClient(channelName) =
        let T = Activator.GetObject(typeof<FSharpInteractiveServer>,"ipc://" ^ channelName ^ "/FSIServer") in
        let x = T :?> FSharpInteractiveServer in
        x
