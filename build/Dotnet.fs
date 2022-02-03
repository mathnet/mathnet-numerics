module Dotnet

open FSharp.Core
open Fake.Core
open Fake.DotNet

let dotnet command = DotNet.exec id command "" |> ignore<ProcessResult>
