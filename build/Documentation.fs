module Documentation

open FSharp.Core
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open System

open Model

let provideDocExtraFiles extraDocs (releases:Release list) =
    for (fileName, docName) in extraDocs do Shell.copyFile ("docs" </> docName) fileName
    let menu = releases |> List.map (fun r -> sprintf "[%s](%s)" r.Title (r.ReleaseNotesFile |> String.replace "RELEASENOTES" "ReleaseNotes" |> String.replace ".md" ".html")) |> String.concat " | "
    for release in releases do
        String.concat Environment.NewLine
          [ "# " + release.Title + " Release Notes"
            menu
            ""
            File.readAsString release.ReleaseNotesFile ]
        |> File.replaceContent ("docs" </> (release.ReleaseNotesFile |> String.replace "RELEASENOTES" "ReleaseNotes"))

let private dotnet command = DotNet.exec id command "" |> ignore<ProcessResult>
let buildDocs outputDir = dotnet (sprintf """fsdocs build --ignoreprojects --noapidocs --output %s --parameters root "https://numerics.mathdotnet.com/" fsdocs-authors "Christoph Ruegg, Marcus Cuda, Jurgen Van Gael" fsdocs-logo-src "/logo.png" """ outputDir)
let watchDocs outputDir = dotnet (sprintf """fsdocs watch --ignoreprojects --noapidocs --output %s --parameters root "https://numerics.mathdotnet.com/" fsdocs-authors "Christoph Ruegg, Marcus Cuda, Jurgen Van Gael" fsdocs-logo-src "/logo.png" """ outputDir)
