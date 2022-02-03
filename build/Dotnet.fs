module Dotnet

open FSharp.Core
open Fake.Core
open Fake.DotNet

let dotnet workingDir command =
    DotNet.exec (fun c -> { c with WorkingDirectory = workingDir}) command "" |> ignore<ProcessResult>

let dotnetWeak workingDir command =
    let properties = [ ("StrongName", "False") ]
    let suffix = properties |> List.map (fun (name, value) -> sprintf """ /p:%s="%s" /nr:false """ name value) |> String.concat ""
    DotNet.exec (fun c -> { c with WorkingDirectory = workingDir }) command suffix |> ignore<ProcessResult>

let dotnetStrong workingDir command =
    let properties = [ ("StrongName", "True") ]
    let suffix = properties |> List.map (fun (name, value) -> sprintf """ /p:%s="%s" /nr:false """ name value) |> String.concat ""
    DotNet.exec (fun c -> { c with WorkingDirectory = workingDir}) command suffix |> ignore<ProcessResult>
