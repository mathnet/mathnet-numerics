//  __  __       _   _       _   _ ______ _______
// |  \/  |     | | | |     | \ | |  ____|__   __|
// | \  / | __ _| |_| |__   |  \| | |__     | |
// | |\/| |/ _` | __| '_ \  | . ` |  __|    | |
// | |  | | (_| | |_| | | |_| |\  | |____   | |
// |_|  |_|\__,_|\__|_| |_(_)_| \_|______|  |_|
//
// Math.NET Numerics - https://numerics.mathdotnet.com
// Copyright (c) Math.NET - Open Source MIT/X11 License
//
// Build Framework using FAKE (http://fsharp.github.io/FAKE)
//

module BuildFramework

#I "../packages/build/FAKE/tools"
#r "../packages/build/FAKE/tools/FakeLib.dll"

open FSharp.Core
open Fake
open Fake.ReleaseNotesHelper
open System
open System.IO

let rootDir = Path.GetFullPath (Path.Combine (__SOURCE_DIRECTORY__ + "/../"))
Environment.CurrentDirectory <- rootDir
trace rootDir


// --------------------------------------------------------------------------------------
// .Net SDK
// --------------------------------------------------------------------------------------


let dotnet workingDir command =
    let properties =
        [
        ]
    let suffix = properties |> List.map (fun (name, value) -> sprintf """ /p:%s="%s" /nr:false """ name value) |> String.concat ""
    DotNetCli.RunCommand
        (fun c -> { c with WorkingDir = workingDir})
        (command + suffix)

let dotnetWeak workingDir command =
    let properties =
        [
            yield "StrongName", "False"
        ]
    let suffix = properties |> List.map (fun (name, value) -> sprintf """ /p:%s="%s" /nr:false """ name value) |> String.concat ""
    DotNetCli.RunCommand
        (fun c -> { c with WorkingDir = workingDir })
        (command + suffix)

let dotnetStrong workingDir command =
    let properties =
        [
            yield "StrongName", "True"
        ]
    let suffix = properties |> List.map (fun (name, value) -> sprintf """ /p:%s="%s" /nr:false """ name value) |> String.concat ""
    DotNetCli.RunCommand
        (fun c -> { c with WorkingDir = workingDir})
        (command + suffix)


// --------------------------------------------------------------------------------------
// Header
// --------------------------------------------------------------------------------------

let header = ReadFile(__SOURCE_DIRECTORY__ </> __SOURCE_FILE__) |> Seq.take 10 |> Seq.map (fun s -> s.Substring(2)) |> toLines

type Release =
    { RepoKey: string
      Title: string
      AssemblyVersion: string
      PackageVersion: string
      ReleaseNotes: string
      ReleaseNotesFile: string }

type ZipPackage =
    { Id: string
      Release: Release
      Title: string }

type NuGetPackage =
    { Id: string
      Release: Release }

type VisualStudioProject =
    { AssemblyName: string
      ProjectFile: string
      OutputDir: string
      Release: Release
      NuGetPackages: NuGetPackage list }

type NativeVisualStudioProject =
    { BinaryName: string
      ProjectFile: string
      OutputDir: string
      Release: Release
      NuGetPackages: NuGetPackage list }

type NativeBashScriptProject =
    { BinaryName: string
      BashScriptFile: string
      OutputDir: string
      Release: Release
      NuGetPackages: NuGetPackage list }

type Project =
    | VisualStudio of VisualStudioProject
    | NativeVisualStudio of NativeVisualStudioProject
    | NativeBashScript of NativeBashScriptProject

type Solution =
    { Key: string
      SolutionFile: string
      Projects: Project list
      Release: Release
      ZipPackages: ZipPackage list
      OutputDir: string
      OutputLibDir: string
      OutputLibStrongNameDir: string
      OutputZipDir: string
      OutputNuGetDir: string }

type NuGetSpecification =
    { NuGet: NuGetPackage
      NuSpecFile: string
      Title: string }


let release repoKey title releaseNotesFile : Release =
    let info = LoadReleaseNotes releaseNotesFile
    let buildPart = "0"
    let assemblyVersion = info.AssemblyVersion + "." + buildPart
    let packageVersion = info.NugetVersion
    let notes = info.Notes |> List.map (fun l -> l.Replace("*","").Replace("`","")) |> toLines
    { Release.RepoKey = repoKey
      Title = title
      AssemblyVersion = assemblyVersion
      PackageVersion = packageVersion
      ReleaseNotes = notes
      ReleaseNotesFile = releaseNotesFile }

let zipPackage packageId title release =
    { ZipPackage.Id = packageId
      Title = title
      Release = release }

let nugetPackage packageId release =
    { NuGetPackage.Id = packageId
      Release = release }

let project assemblyName projectFile nuGetPackages =
    { VisualStudioProject.AssemblyName = assemblyName
      ProjectFile = projectFile
      OutputDir = (Path.GetDirectoryName projectFile) </> "bin" </> "Release"
      NuGetPackages = nuGetPackages
      Release = nuGetPackages |> List.map (fun p -> p.Release) |> List.distinct |> List.exactlyOne }
    |> Project.VisualStudio

let nativeProject binaryName projectFile nuGetPackages =
    { NativeVisualStudioProject.BinaryName = binaryName
      ProjectFile = projectFile
      OutputDir = (Path.GetDirectoryName projectFile) </> "bin" </> "Release"
      NuGetPackages = nuGetPackages
      Release = nuGetPackages |> List.map (fun p -> p.Release) |> List.distinct |> List.exactlyOne }
    |> Project.NativeVisualStudio

let nativeBashScriptProject binaryName bashScriptFile nuGetPackages =
    { NativeBashScriptProject.BinaryName = binaryName
      BashScriptFile = bashScriptFile
      OutputDir = (Path.GetDirectoryName bashScriptFile) </> "bin" </> "Release"
      NuGetPackages = nuGetPackages
      Release = nuGetPackages |> List.map (fun p -> p.Release) |> List.distinct |> List.exactlyOne }
    |> Project.NativeBashScript


let projectOutputDir = function
    | VisualStudio p -> p.OutputDir
    | NativeVisualStudio p -> p.OutputDir
    | NativeBashScript p -> p.OutputDir

let projectRelease = function
    | VisualStudio p -> p.Release
    | NativeVisualStudio p -> p.Release
    | NativeBashScript p -> p.Release

let projectNuGetPackages = function
    | VisualStudio p -> p.NuGetPackages
    | NativeVisualStudio p -> p.NuGetPackages
    | NativeBashScript p -> p.NuGetPackages

let solution key solutionFile projects zipPackages =
    { Solution.Key = key
      SolutionFile = solutionFile
      Projects = projects
      ZipPackages = zipPackages
      Release = List.concat [ projects |> List.map projectRelease; zipPackages |> List.map (fun p -> p.Release) ] |> List.distinct |> List.exactlyOne
      OutputDir = "out" </> key
      OutputLibDir = "out" </> key </> "Lib"
      OutputLibStrongNameDir = "out" </> key </> "Lib-StrongName"
      OutputZipDir = "out" </> key </> "Zip"
      OutputNuGetDir = "out" </> key </> "NuGet" }

let traceHeader (releases:Release list) =
    trace header
    let titleLength = releases |> List.map (fun r -> r.Title.Length) |> List.max
    for release in releases do
        trace ([ " "; release.Title.PadRight titleLength; "  v"; release.PackageVersion ] |> String.concat "")
    trace ""
    dotnet rootDir "--info"
    trace ""


// --------------------------------------------------------------------------------------
// PREPARE
// --------------------------------------------------------------------------------------

let patchVersionInAssemblyInfo path (release:Release) =
    BulkReplaceAssemblyInfoVersions path (fun f ->
        { f with
            AssemblyVersion = release.AssemblyVersion
            AssemblyFileVersion = release.AssemblyVersion
            AssemblyInformationalVersion = release.PackageVersion })

let private regexes_sl = new System.Collections.Generic.Dictionary<string, System.Text.RegularExpressions.Regex>()
let private getRegexSingleLine pattern =
    match regexes_sl.TryGetValue pattern with
    | true, regex -> regex
    | _ -> (new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline))
let regex_replace_singleline pattern (replacement : string) text = (getRegexSingleLine pattern).Replace(text, replacement)

let patchVersionInResource path (release:Release) =
    ReplaceInFile
        (regex_replace @"\d+\.\d+\.\d+\.\d+" release.AssemblyVersion
         >> regex_replace @"\d+,\d+,\d+,\d+" (replace "." "," release.AssemblyVersion))
        path

let patchVersionInProjectFile (project:Project) =
    match project with
    | VisualStudio p ->
        let semverSplit = p.Release.PackageVersion.IndexOf('-')
        let prefix = if semverSplit <= 0 then p.Release.PackageVersion else p.Release.PackageVersion.Substring(0, semverSplit)
        let suffix = if semverSplit <= 0 then "" else p.Release.PackageVersion.Substring(semverSplit+1)
        ReplaceInFile
            (regex_replace """\<PackageVersion\>.*\</PackageVersion\>""" (sprintf """<PackageVersion>%s</PackageVersion>""" p.Release.PackageVersion)
            >> regex_replace """\<Version\>.*\</Version\>""" (sprintf """<Version>%s</Version>""" p.Release.PackageVersion)
            >> regex_replace """\<AssemblyVersion\>.*\</AssemblyVersion\>""" (sprintf """<AssemblyVersion>%s</AssemblyVersion>""" p.Release.AssemblyVersion)
            >> regex_replace """\<FileVersion\>.*\</FileVersion\>""" (sprintf """<FileVersion>%s</FileVersion>""" p.Release.AssemblyVersion)
            >> regex_replace """\<VersionPrefix\>.*\</VersionPrefix\>""" (sprintf """<VersionPrefix>%s</VersionPrefix>""" prefix)
            >> regex_replace """\<VersionSuffix\>.*\</VersionSuffix\>""" (sprintf """<VersionSuffix>%s</VersionSuffix>""" suffix)
            >> regex_replace_singleline """\<PackageReleaseNotes\>.*\</PackageReleaseNotes\>""" (sprintf """<PackageReleaseNotes>%s</PackageReleaseNotes>""" p.Release.ReleaseNotes))
            p.ProjectFile
    | NativeVisualStudio _ -> ()
    | NativeBashScript _ -> ()


// --------------------------------------------------------------------------------------
// BUILD
// --------------------------------------------------------------------------------------

let clean (solution:Solution) = dotnet rootDir (sprintf "clean %s --configuration Release --verbosity minimal" solution.SolutionFile)

let restoreWeak (solution:Solution) = dotnetWeak rootDir (sprintf "restore %s --verbosity minimal" solution.SolutionFile)
let restoreStrong (solution:Solution) = dotnetStrong rootDir (sprintf "restore %s --verbosity minimal" solution.SolutionFile)

let buildWeak (solution:Solution) = dotnetWeak rootDir (sprintf "build %s --configuration Release --no-incremental --no-restore --verbosity minimal" solution.SolutionFile)
let buildStrong (solution:Solution) = dotnetStrong rootDir (sprintf "build %s --configuration Release --no-incremental --no-restore --verbosity minimal" solution.SolutionFile)

let packWeak (solution:Solution) = dotnetWeak rootDir (sprintf "pack %s --configuration Release --no-restore --verbosity minimal" solution.SolutionFile)
let packStrong (solution:Solution) = dotnetStrong rootDir (sprintf "pack %s --configuration Release --no-restore --verbosity minimal" solution.SolutionFile)

let packProjectWeak = function
    | VisualStudio p -> dotnetWeak rootDir (sprintf "pack %s --configuration Release --no-restore --no-build" p.ProjectFile)
    | _ -> failwith "Project type not supported"
let packProjectStrong = function
    | VisualStudio p -> dotnetStrong rootDir (sprintf "pack %s --configuration Release --no-restore --no-build" p.ProjectFile)
    | _ -> failwith "Project type not supported"

//let buildConfig config subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [ "Configuration", config ] subject |> ignore
//let build subject = buildConfig "Release" subject
//let buildSigned subject = buildConfig "Release-Signed" subject
let buildConfig32 config subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration", config); ("Platform","Win32")] subject |> ignore
let buildConfig64 config subject = MSBuild "" (if hasBuildParam "incremental" then "Build" else "Rebuild") [("Configuration", config); ("Platform","x64")] subject |> ignore


// --------------------------------------------------------------------------------------
// COLLECT
// --------------------------------------------------------------------------------------

let collectBinaries (solution:Solution) =
    solution.Projects |> List.iter (function
        | VisualStudio project -> CopyDir solution.OutputLibDir project.OutputDir (fun n -> n.Contains(project.AssemblyName + ".dll") || n.Contains(project.AssemblyName + ".pdb") || n.Contains(project.AssemblyName + ".xml"))
        | _ -> failwith "Project type not supported")

let collectBinariesSN (solution:Solution) =
    solution.Projects |> List.iter (function
        | VisualStudio project -> CopyDir solution.OutputLibStrongNameDir project.OutputDir (fun n -> n.Contains(project.AssemblyName + ".dll") || n.Contains(project.AssemblyName + ".pdb") || n.Contains(project.AssemblyName + ".xml"))
        | _ -> failwith "Project type not supported")

let collectNuGetPackages (solution:Solution) =
    solution.Projects |> List.iter (function
        | VisualStudio project -> CopyDir solution.OutputNuGetDir project.OutputDir (fun n -> n.EndsWith(".nupkg"))
        | _ -> failwith "Project type not supported")


// --------------------------------------------------------------------------------------
// TEST
// --------------------------------------------------------------------------------------

let test testsDir testsProj framework =
    dotnet testsDir (sprintf "run -p %s --configuration Release --framework %s --no-restore --no-build" testsProj framework)


// --------------------------------------------------------------------------------------
// PACKAGES
// --------------------------------------------------------------------------------------

let provideLicense path =
    ReadFileAsString "LICENSE.md"
    |> ConvertTextToWindowsLineBreaks
    |> ReplaceFile (path </> "license.txt")

let provideReadme title (release:Release) path =
    String.concat Environment.NewLine [header; " " + title; ""; ReadFileAsString release.ReleaseNotesFile]
    |> ConvertTextToWindowsLineBreaks
    |> ReplaceFile (path </> "readme.txt")


// SIGN

let sign fingerprint timeserver (solution: Solution) =
    let files = solution.Projects |> Seq.collect (function
        | VisualStudio project -> !! (project.OutputDir + "/**/" + project.AssemblyName + ".dll")
        | _ -> failwith "Project type not supported")
    let fileArgs = files |> Seq.map (sprintf "\"%s\"") |> String.concat " "
    let optionsArgs = sprintf """/v /fd sha256 /sha1 "%s" /tr "%s" /td sha256""" fingerprint timeserver
    let arguments = sprintf """sign %s %s""" optionsArgs fileArgs
    let result =
        ExecProcess (fun info ->
            info.FileName <- findToolInSubPath "signtool.exe" """C:\Program Files (x86)\Windows Kits\10\bin\x64"""
            info.Arguments <- arguments) TimeSpan.MaxValue
    if result <> 0 then
        failwithf "Error during SignTool call "

let signNuGet fingerprint timeserver (solutions: Solution list) =
    CleanDir "obj/NuGet"
    solutions
    |> Seq.collect (fun solution -> !! (solution.OutputNuGetDir </> "*.nupkg"))
    |> Seq.distinct
    |> Seq.iter (fun file ->
        let args = sprintf """sign "%s" -HashAlgorithm SHA256 -TimestampHashAlgorithm SHA256 -CertificateFingerprint "%s" -Timestamper "%s""" (FullName file) fingerprint timeserver
        let result =
            ExecProcess (fun info ->
                info.FileName <- "packages/build/NuGet.CommandLine/tools/NuGet.exe"
                info.WorkingDirectory <- FullName "obj/NuGet"
                info.Arguments <- args) (TimeSpan.FromMinutes 10.)
        if result <> 0 then failwith "Error during NuGet sign.")
    DeleteDir "obj/NuGet"


// ZIP

let zip (package:ZipPackage) zipDir filesDir filesFilter =
    CleanDir "obj/Zip"
    let workPath = "obj/Zip/" + package.Id
    CopyDir workPath filesDir filesFilter
    provideLicense workPath
    provideReadme (sprintf "%s v%s" package.Title package.Release.PackageVersion) package.Release workPath
    Zip "obj/Zip/" (zipDir </> sprintf "%s-%s.zip" package.Id package.Release.PackageVersion) !! (workPath + "/**/*.*")
    DeleteDir "obj/Zip"


// NUGET

let updateNuspec (nuget:NuGetPackage) outPath spec =
    { spec with ToolPath = "packages/build/NuGet.CommandLine/tools/NuGet.exe"
                OutputPath = outPath
                WorkingDir = "obj/NuGet"
                Version = nuget.Release.PackageVersion
                ReleaseNotes = nuget.Release.ReleaseNotes
                Publish = false }

let nugetPackManually (solution:Solution) (packages:NuGetSpecification list) =
    CleanDir "obj/NuGet"
    for pack in packages do
        provideLicense "obj/NuGet"
        provideReadme (sprintf "%s v%s" pack.Title pack.NuGet.Release.PackageVersion) pack.NuGet.Release "obj/NuGet"
        NuGet (updateNuspec pack.NuGet solution.OutputNuGetDir) pack.NuSpecFile
        CleanDir "obj/NuGet"
    DeleteDir "obj/NuGet"


// --------------------------------------------------------------------------------------
// Documentation
// --------------------------------------------------------------------------------------

let provideDocExtraFiles extraDocs (releases:Release list) =
    for (fileName, docName) in extraDocs do CopyFile ("docs/content" </> docName) fileName
    let menu = releases |> List.map (fun r -> sprintf "[%s](%s)" r.Title (r.ReleaseNotesFile |> replace "RELEASENOTES" "ReleaseNotes" |> replace ".md" ".html")) |> String.concat " | "
    for release in releases do
        String.concat Environment.NewLine
          [ "# " + release.Title + " Release Notes"
            menu
            ""
            ReadFileAsString release.ReleaseNotesFile ]
        |> ReplaceFile ("docs/content" </> (release.ReleaseNotesFile |> replace "RELEASENOTES" "ReleaseNotes"))

let buildDocumentationTarget fsiargs target =
    trace (sprintf "Building documentation (%s), this could take some time, please wait..." target)
    let fakePath = "packages" </> "build" </> "FAKE" </> "tools" </> "FAKE.exe"
    let fakeStartInfo script workingDirectory args fsiargs environmentVars =
        (fun (info: System.Diagnostics.ProcessStartInfo) ->
            info.FileName <- System.IO.Path.GetFullPath fakePath
            info.Arguments <- sprintf "%s --fsiargs -d:FAKE %s \"%s\"" args fsiargs script
            info.WorkingDirectory <- workingDirectory
            let setVar k v =
                info.EnvironmentVariables.[k] <- v
            for (k, v) in environmentVars do
                setVar k v
            setVar "MSBuild" msBuildExe
            setVar "GIT" Git.CommandHelper.gitPath
            setVar "FSI" fsiPath)
    let executeFAKEWithOutput workingDirectory script fsiargs envArgs =
        let exitCode =
            ExecProcessWithLambdas
                (fakeStartInfo script workingDirectory "" fsiargs envArgs)
                TimeSpan.MaxValue false ignore ignore
        System.Threading.Thread.Sleep 1000
        exitCode
    let exit = executeFAKEWithOutput "docs/tools" "build-docs.fsx" fsiargs ["target", target]
    if exit <> 0 then
        failwith "Generating documentation failed"
    ()

let generateDocs fail local =
    let args = if local then "" else "--define:RELEASE"
    try
        buildDocumentationTarget args "Default"
        traceImportant "Documentation generated"
    with
    | e when not fail ->
        failwith "Generating documentation failed"


// --------------------------------------------------------------------------------------
// Publishing
// Requires permissions; intended only for maintainers
// --------------------------------------------------------------------------------------

let publishReleaseTag title prefix (release:Release) =
    // inspired by Deedle/tpetricek
    let tagName = prefix + "v" + release.PackageVersion
    let tagMessage = String.concat Environment.NewLine [title + " v" + release.PackageVersion; ""; release.ReleaseNotes ]
    let cmd = sprintf """tag -a %s -m "%s" """ tagName tagMessage
    Git.CommandHelper.runSimpleGitCommand "." cmd |> printfn "%s"
    let _, remotes, _ = Git.CommandHelper.runGitCommand "." "remote -v"
    let main = remotes |> Seq.find (fun s -> s.Contains("(push)") && s.Contains("mathnet/mathnet-" + release.RepoKey))
    let remoteName = main.Split('\t').[0]
    Git.Branches.pushTag "." remoteName tagName

let publishNuGet (solutions: Solution list) =
    CleanDir "obj/NuGet"
    let rec impl trials (file:string) =
        trace ("NuGet Push: " + System.IO.Path.GetFileName(file) + ".")
        try
            let args = sprintf """push "%s" -Source https://api.nuget.org/v3/index.json -T 900""" (FullName file)
            let result =
                ExecProcess (fun info ->
                    info.FileName <- "packages/build/NuGet.CommandLine/tools/NuGet.exe"
                    info.WorkingDirectory <- FullName "obj/NuGet"
                    info.Arguments <- args) (TimeSpan.FromMinutes 10.)
            if result <> 0 then failwith "Error during NuGet push."
        with exn ->
            if trials > 0 then impl (trials-1) file
            else ()
    solutions
    |> Seq.collect (fun solution -> !! (solution.OutputNuGetDir </> "*.nupkg"))
    |> Seq.distinct
    |> Seq.iter (impl 3)
    DeleteDir "obj/NuGet"

let publishDocs (release:Release) =
    let repo = "../web-mathnet-" + release.RepoKey
    Git.Branches.pull repo "origin" "gh-pages"
    CopyRecursive "out/docs" repo true |> printfn "%A"
    Git.Staging.StageAll repo
    Git.Commit.Commit repo (sprintf "%s: %s docs update" release.Title release.PackageVersion)
    Git.Branches.pushBranch repo "origin" "gh-pages"

let publishApi (release:Release) =
    let repo = "../web-mathnet-" + release.RepoKey
    Git.Branches.pull repo "origin" "gh-pages"
    CleanDir (repo + "/api")
    CopyRecursive "out/api" (repo + "/api") true |> printfn "%A"
    Git.Staging.StageAll repo
    Git.Commit.Commit repo (sprintf "%s: %s api update" release.Title release.PackageVersion)
    Git.Branches.pushBranch repo "origin" "gh-pages"

let publishNuGetToArchive (package:NuGetPackage) archivePath nupkgFile =
    let tempDir = Path.GetTempPath() </> Path.GetRandomFileName()
    let archiveDir = archivePath </> package.Id </> package.Release.PackageVersion
    CleanDirs [tempDir; archiveDir]
    nupkgFile |> CopyFile archiveDir
    use sha512 = System.Security.Cryptography.SHA512.Create()
    let hash = File.ReadAllBytes nupkgFile |> sha512.ComputeHash |> Convert.ToBase64String
    File.WriteAllText ((archiveDir </> (Path.GetFileName(nupkgFile) + ".sha512")), hash)
    ZipHelper.Unzip tempDir nupkgFile
    !! (tempDir </> "*.nuspec") |> Copy archiveDir
    DeleteDir tempDir

let publishArchiveManual title zipOutPath nugetOutPath (zipPackages:ZipPackage list) (nugetPackages:NuGetPackage list) =
    let archivePath = (environVarOrFail "MathNetReleaseArchive") </> title
    if directoryExists archivePath |> not then failwith "Release archive directory does not exists. Safety Check failed."
    for zipPackage in zipPackages do
        let zipFile = zipOutPath </> sprintf "%s-%s.zip" zipPackage.Id zipPackage.Release.PackageVersion
        if FileSystemHelper.fileExists zipFile then
            zipFile |> CopyFile (archivePath </> "Zip")
    for nugetPackage in nugetPackages do
        let nupkgFile = nugetOutPath </> sprintf "%s.%s.nupkg" nugetPackage.Id nugetPackage.Release.PackageVersion
        if FileSystemHelper.fileExists nupkgFile then
            trace nupkgFile
            publishNuGetToArchive nugetPackage (archivePath </> "NuGet") nupkgFile
        let symbolsFile = nugetOutPath </> sprintf "%s.%s.symbols.nupkg" nugetPackage.Id nugetPackage.Release.PackageVersion
        if FileSystemHelper.fileExists symbolsFile then
            symbolsFile |> CopyFile (archivePath </> "Symbols")

let publishArchive (solution:Solution) =
    let zipOutPath = solution.OutputZipDir
    let nugetOutPath = solution.OutputNuGetDir
    let zipPackages = solution.ZipPackages
    let nugetPackages = solution.Projects |> List.collect projectNuGetPackages |> List.distinct
    publishArchiveManual solution.Release.Title zipOutPath nugetOutPath zipPackages nugetPackages

let publishArchives (solutions: Solution list) = solutions |> List.iter publishArchive
