module Publishing

open FSharp.Core
open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Tools.Git
open System
open System.IO

open Model

let publishReleaseTag title prefix (release:Release) =
    // inspired by Deedle/tpetricek
    let tagName = prefix + "v" + release.PackageVersion
    let tagMessage = String.concat Environment.NewLine [title + " v" + release.PackageVersion; ""; release.ReleaseNotes ]
    let cmd = sprintf """tag -a %s -m "%s" """ tagName tagMessage
    CommandHelper.runSimpleGitCommand "." cmd |> printfn "%s"
    let _, remotes, _ = CommandHelper.runGitCommand "." "remote -v"
    let main = remotes |> Seq.find (fun s -> s.Contains("(push)") && s.Contains("mathnet/mathnet-" + release.RepoKey))
    let remoteName = main.Split('\t').[0]
    Branches.pushTag "." remoteName tagName

let publishNuGet (solutions: Solution list) =
    Shell.cleanDir "obj/NuGet"
    let rec impl trials (file:string) =
        Trace.log ("NuGet Push: " + System.IO.Path.GetFileName(file) + ".")
        try
            let result =
                CreateProcess.fromRawCommandLine
                    "packages/build/NuGet.CommandLine/tools/NuGet.exe"
                    (sprintf """push "%s" -Source https://api.nuget.org/v3/index.json -T 900""" (Path.getFullName file))
                |> CreateProcess.withWorkingDirectory (Path.getFullName "obj/NuGet")
                |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
                |> Proc.run
            if result.ExitCode <> 0 then failwith "Error during NuGet push."
        with exn ->
            if trials > 0 then impl (trials-1) file
            else ()
    solutions
    |> Seq.collect (fun solution -> !! (solution.OutputNuGetDir </> "*.nupkg"))
    |> Seq.distinct
    |> Seq.iter (impl 3)
    Directory.delete "obj/NuGet"

let publishDocs (release:Release) =
    let repo = "../web-mathnet-" + release.RepoKey
    Branches.pull repo "origin" "gh-pages"
    Shell.copyRecursive "out/docs" repo true |> printfn "%A"
    Staging.stageAll repo
    Commit.exec repo (sprintf "%s: %s docs update" release.Title release.PackageVersion)
    Branches.pushBranch repo "origin" "gh-pages"

let publishApi (release:Release) =
    let repo = "../web-mathnet-" + release.RepoKey
    Branches.pull repo "origin" "gh-pages"
    Shell.cleanDir (repo + "/api")
    Shell.copyRecursive "out/api" (repo + "/api") true |> printfn "%A"
    Staging.stageAll repo
    Commit.exec repo (sprintf "%s: %s api update" release.Title release.PackageVersion)
    Branches.pushBranch repo "origin" "gh-pages"

let publishNuGetToArchive (package:NuGetPackage) archivePath nupkgFile =
    let tempDir = Path.GetTempPath() </> Path.GetRandomFileName()
    let archiveDir = archivePath </> package.Id </> package.Release.PackageVersion
    Shell.cleanDirs [tempDir; archiveDir]
    nupkgFile |> Shell.copyFile archiveDir
    use sha512 = System.Security.Cryptography.SHA512.Create()
    let hash = File.ReadAllBytes nupkgFile |> sha512.ComputeHash |> Convert.ToBase64String
    File.WriteAllText ((archiveDir </> (Path.GetFileName(nupkgFile) + ".sha512")), hash)
    Zip.unzip tempDir nupkgFile
    !! (tempDir </> "*.nuspec") |> Shell.copy archiveDir
    Directory.delete tempDir

let publishArchiveManual title zipOutPath nugetOutPath (zipPackages:ZipPackage list) (nugetPackages:NuGetPackage list) =
    let archivePath = (Environment.environVarOrFail "MathNetReleaseArchive") </> title
    if Directory.Exists archivePath |> not then failwith "Release archive directory does not exists. Safety Check failed."
    for zipPackage in zipPackages do
        let zipFile = zipOutPath </> sprintf "%s-%s.zip" zipPackage.Id zipPackage.Release.PackageVersion
        if File.exists zipFile then
            zipFile |> Shell.copyFile (archivePath </> "Zip")
    for nugetPackage in nugetPackages do
        let nupkgFile = nugetOutPath </> sprintf "%s.%s.nupkg" nugetPackage.Id nugetPackage.Release.PackageVersion
        if File.exists nupkgFile then
            Trace.trace nupkgFile
            publishNuGetToArchive nugetPackage (archivePath </> "NuGet") nupkgFile
        let symbolsFile = nugetOutPath </> sprintf "%s.%s.symbols.nupkg" nugetPackage.Id nugetPackage.Release.PackageVersion
        if File.exists symbolsFile then
            symbolsFile |> Shell.copyFile (archivePath </> "Symbols")

let publishArchive (solution:Solution) =
    let zipOutPath = solution.OutputZipDir
    let nugetOutPath = solution.OutputNuGetDir
    let zipPackages = solution.ZipPackages
    let nugetPackages = solution.Projects |> List.collect projectNuGetPackages |> List.distinct
    publishArchiveManual solution.Release.Title zipOutPath nugetOutPath zipPackages nugetPackages

let publishArchives (solutions: Solution list) = solutions |> List.iter publishArchive
