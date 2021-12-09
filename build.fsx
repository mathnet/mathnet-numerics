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
// FAKE build script, see https://fake.build/
//

#r "paket:
nuget Fake.Core.Context
nuget Fake.Core.Environment
nuget Fake.Core.ReleaseNotes
nuget Fake.Core.String
nuget Fake.Core.Target
nuget Fake.Core.Trace
nuget Fake.DotNet.Cli
nuget Fake.DotNet.NuGet
nuget Fake.IO.FileSystem
nuget Fake.IO.Zip
nuget Fake.Tools.Git"
#load "./.fake/build.fsx/intellisense.fsx"

open FSharp.Core
open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.DotNet.NuGet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Tools.Git
open System
open System.IO

let header = File.read (__SOURCE_DIRECTORY__ </> __SOURCE_FILE__) |> Seq.take 10 |> Seq.map (fun s -> s.Substring(2)) |> String.toLines
let rootDir = Path.getFullName __SOURCE_DIRECTORY__
Environment.CurrentDirectory <- rootDir
Trace.log rootDir

let args = Target.getArguments()
let isStrongname, isSign, isIncremental =
    match args with
    | Some args ->
        args |> Seq.contains "--strongname",
        args |> Seq.contains "--sign" && Environment.isWindows,
        args |> Seq.contains "--incremental"
    | None -> false, false, false


// --------------------------------------------------------------------------------------
// .Net SDK
// --------------------------------------------------------------------------------------


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


// --------------------------------------------------------------------------------------
// Model
// --------------------------------------------------------------------------------------

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
    let info = ReleaseNotes.load releaseNotesFile
    let buildPart = "0"
    let assemblyVersion = info.AssemblyVersion + "." + buildPart
    let packageVersion = info.NugetVersion
    let notes = info.Notes |> List.map (fun l -> l.Replace("*","").Replace("`","")) |> String.toLines
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
    Trace.log header
    let titleLength = releases |> List.map (fun r -> r.Title.Length) |> List.max
    for release in releases do
        Trace.log ([ " "; release.Title.PadRight titleLength; "  v"; release.PackageVersion ] |> String.concat "")
    Trace.log ""
    dotnet rootDir "--info"
    Trace.log ""


// --------------------------------------------------------------------------------------
// PREPARE
// --------------------------------------------------------------------------------------

let private regexes_sl = new System.Collections.Generic.Dictionary<string, System.Text.RegularExpressions.Regex>()
let private getRegexSingleLine pattern =
    match regexes_sl.TryGetValue pattern with
    | true, regex -> regex
    | _ -> (System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline))
let regex_replace_singleline pattern (replacement : string) text = (getRegexSingleLine pattern).Replace(text, replacement)

let patchVersionInResource path (release:Release) =
    File.applyReplace
        (String.regex_replace @"\d+\.\d+\.\d+\.\d+" release.AssemblyVersion
         >> String.regex_replace @"\d+,\d+,\d+,\d+" (String.replace "." "," release.AssemblyVersion))
        path

let patchVersionInProjectFile (project:Project) =
    match project with
    | VisualStudio p ->
        let semverSplit = p.Release.PackageVersion.IndexOf('-')
        let prefix = if semverSplit <= 0 then p.Release.PackageVersion else p.Release.PackageVersion.Substring(0, semverSplit)
        let suffix = if semverSplit <= 0 then "" else p.Release.PackageVersion.Substring(semverSplit+1)
        File.applyReplace
            (String.regex_replace """\<PackageVersion\>.*\</PackageVersion\>""" (sprintf """<PackageVersion>%s</PackageVersion>""" p.Release.PackageVersion)
            >> String.regex_replace """\<Version\>.*\</Version\>""" (sprintf """<Version>%s</Version>""" p.Release.PackageVersion)
            >> String.regex_replace """\<AssemblyVersion\>.*\</AssemblyVersion\>""" (sprintf """<AssemblyVersion>%s</AssemblyVersion>""" p.Release.AssemblyVersion)
            >> String.regex_replace """\<FileVersion\>.*\</FileVersion\>""" (sprintf """<FileVersion>%s</FileVersion>""" p.Release.AssemblyVersion)
            >> String.regex_replace """\<VersionPrefix\>.*\</VersionPrefix\>""" (sprintf """<VersionPrefix>%s</VersionPrefix>""" prefix)
            >> String.regex_replace """\<VersionSuffix\>.*\</VersionSuffix\>""" (sprintf """<VersionSuffix>%s</VersionSuffix>""" suffix)
            >> regex_replace_singleline """\<PackageReleaseNotes\>.*\</PackageReleaseNotes\>""" (sprintf """<PackageReleaseNotes>%s</PackageReleaseNotes>""" (p.Release.ReleaseNotes.Replace("<","&lt;").Replace(">","&gt;"))))
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

let buildVS2019x86 config subject =
    MSBuild.run
        (fun p -> {p with ToolPath = Environment.environVar "VS2019INSTALLDIR" </> @"MSBuild\Current\Bin\MSBuild.exe"})
        ""
        (if isIncremental then "Build" else "Rebuild")
        [("Configuration", config); ("Platform","Win32")]
        subject
        |> ignore<string list>
let buildVS2019x64 config subject =
    MSBuild.run
        (fun p -> {p with ToolPath = Environment.environVar "VS2019INSTALLDIR" </> @"MSBuild\Current\Bin\MSBuild.exe"})
        ""
        (if isIncremental then "Build" else "Rebuild")
        [("Configuration", config); ("Platform","x64")]
        subject
        |> ignore<string list>


// --------------------------------------------------------------------------------------
// COLLECT
// --------------------------------------------------------------------------------------

let collectBinaries (solution:Solution) =
    solution.Projects |> List.iter (function
        | VisualStudio project -> Shell.copyDir solution.OutputLibDir project.OutputDir (fun n -> n.Contains(project.AssemblyName + ".dll") || n.Contains(project.AssemblyName + ".pdb") || n.Contains(project.AssemblyName + ".xml"))
        | _ -> failwith "Project type not supported")

let collectBinariesSN (solution:Solution) =
    solution.Projects |> List.iter (function
        | VisualStudio project -> Shell.copyDir solution.OutputLibStrongNameDir project.OutputDir (fun n -> n.Contains(project.AssemblyName + ".dll") || n.Contains(project.AssemblyName + ".pdb") || n.Contains(project.AssemblyName + ".xml"))
        | _ -> failwith "Project type not supported")

let collectNuGetPackages (solution:Solution) =
    solution.Projects |> List.iter (function
        | VisualStudio project -> Shell.copyDir solution.OutputNuGetDir project.OutputDir (fun n -> n.EndsWith(".nupkg"))
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
    File.readAsString "LICENSE.md"
    |> String.convertTextToWindowsLineBreaks
    |> File.replaceContent (path </> "license.txt")

let provideReadme title (release:Release) path =
    String.concat Environment.NewLine [header; " " + title; ""; File.readAsString release.ReleaseNotesFile]
    |> String.convertTextToWindowsLineBreaks
    |> File.replaceContent (path </> "readme.txt")


// SIGN

let sign fingerprint timeserver (solution: Solution) =
    let files = solution.Projects |> Seq.collect (function
        | VisualStudio project -> !! (project.OutputDir + "/**/" + project.AssemblyName + ".dll")
        | _ -> failwith "Project type not supported")
    let fileArgs = files |> Seq.map (sprintf "\"%s\"") |> String.concat " "
    let optionsArgs = sprintf """/v /fd sha256 /sha1 "%s" /tr "%s" /td sha256""" fingerprint timeserver
    let arguments = sprintf """sign %s %s""" optionsArgs fileArgs
    let result =
        CreateProcess.fromRawCommandLine (ProcessUtils.findLocalTool "SIGNTOOL" "signtool.exe" ["""C:\Program Files (x86)\Windows Kits\10\bin\x64"""]) arguments
        |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
        |> Proc.run
    if result.ExitCode <> 0 then failwithf "Error during SignTool call "

let signNuGet fingerprint timeserver (solutions: Solution list) =
    Shell.cleanDir "obj/NuGet"
    solutions
    |> Seq.collect (fun solution -> !! (solution.OutputNuGetDir </> "*.nupkg"))
    |> Seq.distinct
    |> Seq.iter (fun file ->
        let args = sprintf """sign "%s" -HashAlgorithm SHA256 -TimestampHashAlgorithm SHA256 -CertificateFingerprint "%s" -Timestamper "%s""" (Path.getFullName file) fingerprint timeserver
        let result =
            CreateProcess.fromRawCommandLine "packages/build/NuGet.CommandLine/tools/NuGet.exe" args
            |> CreateProcess.withWorkingDirectory (Path.getFullName "obj/NuGet")
            |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
            |> Proc.run
        if result.ExitCode <> 0 then failwith "Error during NuGet sign.")
    Directory.delete "obj/NuGet"


// ZIP

let zip (package:ZipPackage) zipDir filesDir filesFilter =
    Shell.cleanDir "obj/Zip"
    let workPath = "obj/Zip/" + package.Id
    Shell.copyDir workPath filesDir filesFilter
    provideLicense workPath
    provideReadme (sprintf "%s v%s" package.Title package.Release.PackageVersion) package.Release workPath
    Zip.zip "obj/Zip/" (zipDir </> sprintf "%s-%s.zip" package.Id package.Release.PackageVersion) !! (workPath + "/**/*.*")
    Directory.delete "obj/Zip"


// NUGET

let updateNuspec (nuget:NuGetPackage) outPath (spec:NuGet.NuGet.NuGetParams) =
    { spec with ToolPath = "packages/build/NuGet.CommandLine/tools/NuGet.exe"
                OutputPath = outPath
                WorkingDir = "obj/NuGet"
                Version = nuget.Release.PackageVersion
                ReleaseNotes = nuget.Release.ReleaseNotes
                Publish = false }

let nugetPackManually (solution:Solution) (packages:NuGetSpecification list) =
    Shell.cleanDir "obj/NuGet"
    for pack in packages do
        provideLicense "obj/NuGet"
        provideReadme (sprintf "%s v%s" pack.Title pack.NuGet.Release.PackageVersion) pack.NuGet.Release "obj/NuGet"
        NuGet.NuGet (updateNuspec pack.NuGet solution.OutputNuGetDir) pack.NuSpecFile
        Shell.cleanDir "obj/NuGet"
    Directory.delete "obj/NuGet"


// --------------------------------------------------------------------------------------
// Documentation
// --------------------------------------------------------------------------------------

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


// --------------------------------------------------------------------------------------
// Publishing
// Requires permissions; intended only for maintainers
// --------------------------------------------------------------------------------------

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








// --------------------------------------------------------------------------------------
// PROJECT INFO
// --------------------------------------------------------------------------------------

// VERSION OVERVIEW

let numericsRelease = release "numerics" "Math.NET Numerics" "RELEASENOTES.md"
let mklRelease = release "numerics" "MKL Provider" "RELEASENOTES-MKL.md"
let cudaRelease = release "numerics" "CUDA Provider" "RELEASENOTES-CUDA.md"
let openBlasRelease = release "numerics" "OpenBLAS Provider" "RELEASENOTES-OpenBLAS.md"
let releases = [ numericsRelease; mklRelease; openBlasRelease ] // skip cuda
traceHeader releases


// NUMERICS PACKAGES

let numericsZipPackage = zipPackage "MathNet.Numerics" "Math.NET Numerics" numericsRelease
let numericsStrongNameZipPackage = zipPackage "MathNet.Numerics.Signed" "Math.NET Numerics" numericsRelease

let numericsNuGetPackage = nugetPackage "MathNet.Numerics" numericsRelease
let numericsFSharpNuGetPackage = nugetPackage "MathNet.Numerics.FSharp" numericsRelease
let numericsProvidersMklNuGetPackage = nugetPackage "MathNet.Numerics.Providers.MKL" numericsRelease
let numericsProvidersOpenBlasNuGetPackage = nugetPackage "MathNet.Numerics.Providers.OpenBLAS" numericsRelease
let numericsProvidersCudaNuGetPackage = nugetPackage "MathNet.Numerics.Providers.CUDA" numericsRelease
let numericsDataTextNuGetPackage = nugetPackage "MathNet.Numerics.Data.Text" numericsRelease
let numericsDataMatlabNuGetPackage = nugetPackage "MathNet.Numerics.Data.Matlab" numericsRelease

let numericsStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Signed" numericsRelease
let numericsFSharpStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.FSharp.Signed" numericsRelease
let numericsProvidersMklStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Providers.MKL.Signed" numericsRelease
let numericsProvidersOpenBlasStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Providers.OpenBLAS.Signed" numericsRelease
let numericsProvidersCudaStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Providers.CUDA.Signed" numericsRelease
let numericsDataTextStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Data.Text.Signed" numericsRelease
let numericsDataMatlabStrongNameNuGetPackage = nugetPackage "MathNet.Numerics.Data.Matlab.Signed" numericsRelease

let numericsProject = project "MathNet.Numerics" "src/Numerics/Numerics.csproj" [numericsNuGetPackage; numericsStrongNameNuGetPackage]
let numericsFsharpProject = project "MathNet.Numerics.FSharp" "src/FSharp/FSharp.fsproj" [numericsFSharpNuGetPackage; numericsFSharpStrongNameNuGetPackage]
let numericsProvidersMklProject = project "MathNet.Numerics.Providers.MKL" "src/Providers.MKL/Providers.MKL.csproj" [numericsProvidersMklNuGetPackage; numericsProvidersMklStrongNameNuGetPackage]
let numericsProvidersOpenBlasProject = project "MathNet.Numerics.Providers.OpenBLAS" "src/Providers.OpenBLAS/Providers.OpenBLAS.csproj" [numericsProvidersOpenBlasNuGetPackage; numericsProvidersOpenBlasStrongNameNuGetPackage]
let numericsProvidersCudaProject = project "MathNet.Numerics.Providers.CUDA" "src/Providers.CUDA/Providers.CUDA.csproj" [numericsProvidersCudaNuGetPackage; numericsProvidersCudaStrongNameNuGetPackage]
let numericsDataTextProject = project "MathNet.Numerics.Data.Text" "src/Data.Text/Data.Text.csproj" [numericsDataTextNuGetPackage; numericsDataTextStrongNameNuGetPackage]
let numericsDataMatlabProject = project "MathNet.Numerics.Data.Matlab" "src/Data.Matlab/Data.Matlab.csproj" [numericsDataMatlabNuGetPackage; numericsDataMatlabStrongNameNuGetPackage]
let numericsSolution = solution "Numerics" "MathNet.Numerics.sln" [numericsProject; numericsFsharpProject; numericsProvidersMklProject; numericsProvidersOpenBlasProject; numericsProvidersCudaProject; numericsDataTextProject; numericsDataMatlabProject] [numericsZipPackage; numericsStrongNameZipPackage]


// MKL NATIVE PROVIDER PACKAGES

let mklWinZipPackage = zipPackage "MathNet.Numerics.MKL.Win" "Math.NET Numerics MKL Native Provider for Windows" mklRelease
let mklLinuxZipPackage = zipPackage "MathNet.Numerics.MKL.Linux" "Math.NET Numerics MKL Native Provider for Linux" mklRelease

let mklWinNuGetPackage = nugetPackage "MathNet.Numerics.MKL.Win" mklRelease
let mklWin32NuGetPackage = nugetPackage "MathNet.Numerics.MKL.Win-x86" mklRelease
let mklWin64NuGetPackage = nugetPackage "MathNet.Numerics.MKL.Win-x64" mklRelease
let mklLinuxNuGetPackage = nugetPackage "MathNet.Numerics.MKL.Linux" mklRelease
let mklLinux32NuGetPackage = nugetPackage "MathNet.Numerics.MKL.Linux-x86" mklRelease
let mklLinux64NuGetPackage = nugetPackage "MathNet.Numerics.MKL.Linux-x64" mklRelease

let mklWinProject = nativeProject "MathNet.Numerics.MKL" "src/NativeProviders/Windows/MKL/MKLWrapper.vcxproj" [mklWinNuGetPackage; mklWin32NuGetPackage; mklWin64NuGetPackage]
let mklLinuxProject = nativeBashScriptProject "MathNet.Numerics.MKL" "src/NativeProviders/Linux/mkl_build.sh" [mklLinuxNuGetPackage; mklLinux32NuGetPackage; mklLinux64NuGetPackage]
let mklSolution = solution "MKL" "MathNet.Numerics.MKL.sln" [mklWinProject; mklLinuxProject] [mklWinZipPackage; mklLinuxZipPackage]

let mklWinPack =
    { NuGet = mklWinNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Win.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x64 and x86)" }

let mklWin32Pack =
    { NuGet = mklWin32NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Win-x86.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x86)" }

let mklWin64Pack =
    { NuGet = mklWin64NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Win-x64.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Windows (x64)" }

let mklLinuxPack =
    { NuGet = mklLinuxNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Linux.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x64 and x86)" }

let mklLinux32Pack =
    { NuGet = mklLinux32NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Linux-x86.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x86)" }

let mklLinux64Pack =
    { NuGet = mklLinux64NuGetPackage
      NuSpecFile = "build/MathNet.Numerics.MKL.Linux-x64.nuspec"
      Title = "Math.NET Numerics - MKL Native Provider for Linux (x64)" }


// CUDA NATIVE PROVIDER PACKAGES

let cudaWinZipPackage = zipPackage "MathNet.Numerics.CUDA.Win" "Math.NET Numerics CUDA Native Provider for Windows" cudaRelease
let cudaWinNuGetPackage = nugetPackage "MathNet.Numerics.CUDA.Win" cudaRelease

let cudaWinProject = nativeProject "MathNet.Numerics.CUDA" "src/NativeProviders/Windows/CUDA/CUDAWrapper.vcxproj" [cudaWinNuGetPackage]
let cudaSolution = solution "CUDA" "MathNet.Numerics.CUDA.sln" [cudaWinProject] [cudaWinZipPackage]

let cudaWinPack =
    { NuGet = cudaWinNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.CUDA.Win.nuspec"
      Title = "Math.NET Numerics - CUDA Native Provider for Windows (x64)" }


// OpenBLAS NATIVE PROVIDER PACKAGES

let openBlasWinZipPackage = zipPackage "MathNet.Numerics.OpenBLAS.Win" "Math.NET Numerics OpenBLAS Native Provider for Windows" openBlasRelease
let openBlasWinNuGetPackage = nugetPackage "MathNet.Numerics.OpenBLAS.Win" openBlasRelease

let openBlasWinProject = nativeProject "MathNet.Numerics.OpenBLAS" "src/NativeProviders/Windows/OpenBLAS/OpenBLASWrapper.vcxproj" [openBlasWinNuGetPackage]
let openBlasSolution = solution "OpenBLAS" "MathNet.Numerics.OpenBLAS.sln" [openBlasWinProject] [openBlasWinZipPackage]

let openBlasWinPack =
    { NuGet = openBlasWinNuGetPackage
      NuSpecFile = "build/MathNet.Numerics.OpenBLAS.Win.nuspec"
      Title = "Math.NET Numerics - OpenBLAS Native Provider for Windows (x64 and x86)" }


// ALL

let allSolutions = [numericsSolution]
let allProjects = allSolutions |> List.collect (fun s -> s.Projects) |> List.distinct


// --------------------------------------------------------------------------------------
// PREPARE
// --------------------------------------------------------------------------------------

Target.create "Start" ignore

Target.create "Clean" (fun _ ->
    Shell.deleteDirs (!! "src/**/obj/" ++ "src/**/bin/" )
    Shell.cleanDirs [ "out/api"; "out/docs" ]
    Shell.cleanDirs [ "out/MKL"; "out/ATLAS"; "out/CUDA"; "out/OpenBLAS" ] // Native Providers
    allSolutions |> List.iter (fun solution -> Shell.cleanDirs [ solution.OutputZipDir; solution.OutputNuGetDir; solution.OutputLibDir; solution.OutputLibStrongNameDir ]))

Target.create "ApplyVersion" (fun _ ->
    allProjects |> List.iter patchVersionInProjectFile
    patchVersionInResource "src/NativeProviders/MKL/resource.rc" mklRelease
    patchVersionInResource "src/NativeProviders/CUDA/resource.rc" cudaRelease
    patchVersionInResource "src/NativeProviders/OpenBLAS/resource.rc" openBlasRelease)

Target.create "Restore" (fun _ -> allSolutions |> List.iter restoreWeak)
"Start"
  =?> ("Clean", not isIncremental)
  ==> "Restore"

Target.create "Prepare" ignore
"Start"
  =?> ("Clean", not isIncremental)
  ==> "ApplyVersion"
  ==> "Prepare"


// --------------------------------------------------------------------------------------
// BUILD, SIGN, COLLECT
// --------------------------------------------------------------------------------------

let fingerprint = "490408de3618bed0a28e68dc5face46e5a3a97dd"
let timeserver = "http://time.certum.pl/"

Target.create "Build" (fun _ ->

    // Strong Name Build (with strong name, without certificate signature)
    if isStrongname then
        Shell.cleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
        restoreStrong numericsSolution
        buildStrong numericsSolution
        if isSign then sign fingerprint timeserver numericsSolution
        collectBinariesSN numericsSolution
        zip numericsStrongNameZipPackage numericsSolution.OutputZipDir numericsSolution.OutputLibStrongNameDir (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
        packStrong numericsSolution
        collectNuGetPackages numericsSolution

    // Normal Build (without strong name, with certificate signature)
    Shell.cleanDirs (!! "src/**/obj/" ++ "src/**/bin/" )
    restoreWeak numericsSolution
    buildWeak numericsSolution
    if isSign then sign fingerprint timeserver numericsSolution
    collectBinaries numericsSolution
    zip numericsZipPackage numericsSolution.OutputZipDir numericsSolution.OutputLibDir (fun f -> f.Contains("MathNet.Numerics.") || f.Contains("System.Threading.") || f.Contains("FSharp.Core."))
    packWeak numericsSolution
    collectNuGetPackages numericsSolution

    // NuGet Sign (all or nothing)
    if isSign then signNuGet fingerprint timeserver [numericsSolution]

    )
"Prepare" ==> "Build"

Target.create "MklWinBuild" (fun _ ->

    //let result =
    //    CreateProcess.fromRawCommandLine "cmd.exe" "/c setvars.bat"
    //    |> CreateProcess.withWorkingDirectory (Environment.GetEnvironmentVariable("ONEAPI_ROOT"))
    //    |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
    //    |> Proc.run
    //if result.ExitCode <> 0 then failwith "Error while setting oneAPI environment variables."

    restoreWeak mklSolution
    buildVS2019x86 "Release-MKL" !! "MathNet.Numerics.MKL.sln"
    buildVS2019x64 "Release-MKL" !! "MathNet.Numerics.MKL.sln"
    Directory.create mklSolution.OutputZipDir
    zip mklWinZipPackage mklSolution.OutputZipDir "out/MKL/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.MKL.") || f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5md.dll"))
    Directory.create mklSolution.OutputNuGetDir
    nugetPackManually mklSolution [ mklWinPack; mklWin32Pack; mklWin64Pack ]

    // NuGet Sign (all or nothing)
    if isSign then signNuGet fingerprint timeserver [mklSolution]

    )
"Prepare" ==> "MklWinBuild"

Target.create "CudaWinBuild" (fun _ ->

    restoreWeak cudaSolution
    buildVS2019x64 "Release-CUDA" !! "MathNet.Numerics.CUDA.sln"
    Directory.create cudaSolution.OutputZipDir
    zip cudaWinZipPackage cudaSolution.OutputZipDir "out/CUDA/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.CUDA.") || f.Contains("MathNet.Numerics.CUDA.") || f.Contains("cublas") || f.Contains("cudart") || f.Contains("cusolver"))
    Directory.create cudaSolution.OutputNuGetDir
    nugetPackManually cudaSolution [ cudaWinPack ]

    // NuGet Sign (all or nothing)
    if isSign then signNuGet fingerprint timeserver [cudaSolution]

    )
"Prepare" ==> "CudaWinBuild"

Target.create "OpenBlasWinBuild" (fun _ ->

    restoreWeak openBlasSolution
    buildVS2019x86 "Release-OpenBLAS" !! "MathNet.Numerics.OpenBLAS.sln"
    buildVS2019x64 "Release-OpenBLAS" !! "MathNet.Numerics.OpenBLAS.sln"
    Directory.create openBlasSolution.OutputZipDir
    zip openBlasWinZipPackage openBlasSolution.OutputZipDir "out/OpenBLAS/Windows" (fun f -> f.Contains("MathNet.Numerics.Providers.OpenBLAS.") || f.Contains("MathNet.Numerics.OpenBLAS.") || f.Contains("libgcc") || f.Contains("libgfortran") || f.Contains("libopenblas") || f.Contains("libquadmath"))
    Directory.create openBlasSolution.OutputNuGetDir
    nugetPackManually openBlasSolution [ openBlasWinPack ]

    // NuGet Sign (all or nothing)
    if isSign then signNuGet fingerprint timeserver [openBlasSolution]

    )
"Prepare" ==> "OpenBlasWinBuild"


// --------------------------------------------------------------------------------------
// TEST
// --------------------------------------------------------------------------------------

let testNumerics framework = test "src/Numerics.Tests" "Numerics.Tests.csproj" framework
Target.create "TestNumerics" ignore
Target.create "TestNumericsNET50"  (fun _ -> testNumerics "net5.0")
Target.create "TestNumericsNET48" (fun _ -> testNumerics "net48")
"Build" ==> "TestNumericsNET50" ==> "TestNumerics"
"Build" =?> ("TestNumericsNET48", Environment.isWindows) ==> "TestNumerics"
let testFsharp framework = test "src/FSharp.Tests" "FSharp.Tests.fsproj" framework
Target.create "TestFsharp" ignore
Target.create "TestFsharpNET50" (fun _ -> testFsharp "net5.0")
Target.create "TestFsharpNET48" (fun _ -> testFsharp "net48")
"Build" ==> "TestFsharpNET50" ==> "TestFsharp"
"Build" =?> ("TestFsharpNET48", Environment.isWindows) ==> "TestFsharp"
let testData framework = test "src/Data.Tests" "Data.Tests.csproj" framework
Target.create "TestData" ignore
Target.create "TestDataNET50" (fun _ -> testData "net5.0")
Target.create "TestDataNET48" (fun _ -> testData "net48")
"Build" ==> "TestDataNET50" ==> "TestData"
"Build" =?> ("TestDataNET48", Environment.isWindows) ==> "TestData"
Target.create "Test" ignore
"TestNumerics" ==> "Test"
"TestFsharp" ==> "Test"
"TestData" ==> "Test"

let testMKL framework = test "src/Numerics.Tests" "Numerics.Tests.MKL.csproj" framework
Target.create "MklTest" ignore
Target.create "MklTestNET50" (fun _ -> testMKL "net5.0")
Target.create "MklTestNET48" (fun _ -> testMKL "net48")
"MklWinBuild" ==> "MklTestNET50" ==> "MklTest"
"MklWinBuild" =?> ("MklTestNET48", Environment.isWindows) ==> "MklTest"

let testOpenBLAS framework = test "src/Numerics.Tests" "Numerics.Tests.OpenBLAS.csproj" framework
Target.create "OpenBlasTest" ignore
Target.create "OpenBlasTestNET50" (fun _ -> testOpenBLAS "net5.0")
Target.create "OpenBlasTestNET48" (fun _ -> testOpenBLAS "net48")
"OpenBlasWinBuild" ==> "OpenBlasTestNET50" ==> "OpenBlasTest"
"OpenBlasWinBuild" =?> ("OpenBlasTestNET48", Environment.isWindows) ==> "OpenBlasTest"

let testCUDA framework = test "src/Numerics.Tests" "Numerics.Tests.CUDA.csproj" framework
Target.create "CudaTest" ignore
Target.create "CudaTestNET50" (fun _ -> testCUDA "net5.0")
Target.create "CudaTestNET48" (fun _ -> testCUDA "net48")
"CudaWinBuild" ==> "CudaTestNET50" ==> "CudaTest"
"CudaWinBuild" =?> ("CudaTestNET48", Environment.isWindows) ==> "CudaTest"


// --------------------------------------------------------------------------------------
// LINUX PACKAGES
// --------------------------------------------------------------------------------------

Target.create "MklLinuxPack" ignore

Target.create "MklLinuxZip" (fun _ ->
    Directory.create mklSolution.OutputZipDir
    zip mklLinuxZipPackage mklSolution.OutputZipDir "out/MKL/Linux" (fun f -> f.Contains("MathNet.Numerics.Providers.MKL.") || f.Contains("MathNet.Numerics.MKL.") || f.Contains("libiomp5.so")))
"MklLinuxZip" ==> "MklLinuxPack"

Target.create "MklLinuxNuGet" (fun _ ->
    Directory.create mklSolution.OutputNuGetDir
    nugetPackManually mklSolution [ mklLinuxPack; mklLinux32Pack; mklLinux64Pack ])
"MklLinuxNuGet" ==> "MklLinuxPack"


// --------------------------------------------------------------------------------------
// Documentation
// --------------------------------------------------------------------------------------

// DOCS

Target.create "CleanDocs" (fun _ -> Shell.cleanDirs ["out/docs"])

let extraDocs =
    [ "LICENSE.md", "License.md"
      "CONTRIBUTING.md", "Contributing.md"
      "CONTRIBUTORS.md", "Contributors.md" ]

Target.create "Docs" (fun _ ->
    provideDocExtraFiles extraDocs releases
    dotnet rootDir "fsdocs build --noapidocs --output out/docs")
Target.create "DocsDev" (fun _ ->
    provideDocExtraFiles extraDocs releases
    dotnet rootDir "fsdocs build --noapidocs --output out/docs")
Target.create "DocsWatch" (fun _ ->
    provideDocExtraFiles extraDocs releases
    dotnet rootDir "fsdocs build --noapidocs --output out/docs"
    dotnet rootDir "fsdocs watch --noapidocs --output out/docs")

"Build" ==> "CleanDocs" ==> "Docs"

"Start"
  =?> ("CleanDocs", not isIncremental)
  ==> "DocsDev"
  ==> "DocsWatch"


// API REFERENCE

Target.create "CleanApi" (fun _ -> Shell.cleanDirs ["out/api"])

Target.create "Api" (fun _ ->
    let result =
        CreateProcess.fromRawCommandLine
            "tools/docu/docu.exe"
            ([
                rootDir </> "src/Numerics/bin/Release/net461/MathNet.Numerics.dll" |> Path.getFullName
                "--output=" + (rootDir </> "out/api/" |> Path.getFullName)
                "--templates=" + (rootDir </> "tools/docu/templates/" |> Path.getFullName)
             ] |> String.concat " ")
        |> CreateProcess.withWorkingDirectory rootDir
        |> CreateProcess.withTimeout (TimeSpan.FromMinutes 10.)
        |> Proc.run
    if result.ExitCode <> 0 then failwith "Error during API reference generation."    )

"Build" ==> "CleanApi" ==> "Api"


// --------------------------------------------------------------------------------------
// Publishing
// Requires permissions; intended only for maintainers
// --------------------------------------------------------------------------------------

Target.create "PublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics" "" numericsRelease)
Target.create "MklPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics MKL Provider" "mkl-" mklRelease)
Target.create "CudaPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics CUDA Provider" "cuda-" cudaRelease)
Target.create "OpenBlasPublishTag" (fun _ -> publishReleaseTag "Math.NET Numerics OpenBLAS Provider" "openblas-" openBlasRelease)

Target.create "PublishDocs" (fun _ -> publishDocs numericsRelease)
Target.create "PublishApi" (fun _ -> publishApi numericsRelease)

Target.create "PublishArchive" (fun _ -> publishArchives [numericsSolution])
Target.create "MklPublishArchive" (fun _ -> publishArchives [mklSolution])
Target.create "CudaPublishArchive" (fun _ -> publishArchives [cudaSolution])
Target.create "OpenBlasPublishArchive" (fun _ -> publishArchives [openBlasSolution])

Target.create "PublishNuGet" (fun _ -> publishNuGet [numericsSolution])
Target.create "MklPublishNuGet" (fun _ -> publishNuGet [mklSolution])
Target.create "CudaPublishNuGet" (fun _ -> publishNuGet [cudaSolution])
Target.create "OpenBlasPublishNuGet" (fun _ -> publishNuGet [openBlasSolution])

Target.create "Publish" ignore
"Publish" <== [ "PublishTag"; "PublishDocs"; "PublishApi"; "PublishArchive"; "PublishNuGet" ]

Target.create "MklPublish" ignore
"MklPublish" <== [ "MklPublishTag"; "PublishDocs"; "MklPublishArchive"; "MklPublishNuGet" ]

Target.create "CudaPublish" ignore
"CudaPublish" <== [ "CudaPublishTag"; "PublishDocs"; "CudaPublishArchive"; "CudaPublishNuGet" ]

Target.create "OpenBlasPublish" ignore
"OpenBlasPublish" <== [ "OpenBlasPublishTag"; "PublishDocs"; "OpenBlasPublishArchive"; "OpenBlasPublishNuGet" ]


// --------------------------------------------------------------------------------------
// Default Targets
// --------------------------------------------------------------------------------------

Target.create "All" ignore
"All" <== [ "Build"; "Docs"; "Api"; "Test" ]

Target.create "MklWinAll" ignore
"MklWinAll" <== [ "MklWinBuild"; "MklTest" ]

Target.create "CudaWinAll" ignore
"CudaWinAll" <== [ "CudaWinBuild"; "CudaTest" ]

Target.create "OpenBlasWinAll" ignore
"OpenBlasWinAll" <== [ "OpenBlasWinBuild"; "OpenBlasTest" ]

Target.runOrDefaultWithArguments "Test"
