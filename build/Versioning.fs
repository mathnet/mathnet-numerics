module Versioning

open FSharp.Core
open Fake.Core
open Fake.IO

open Model

let private regexes_sl = new System.Collections.Generic.Dictionary<string, System.Text.RegularExpressions.Regex>()
let private getRegexSingleLine pattern =
    match regexes_sl.TryGetValue pattern with
    | true, regex -> regex
    | _ -> (System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline))
let private regex_replace_singleline pattern (replacement : string) text = (getRegexSingleLine pattern).Replace(text, replacement)

let updateNativeResource path (release:Release) =
    File.applyReplace
        (String.regex_replace @"\d+\.\d+\.\d+\.\d+" release.AssemblyVersion
         >> String.regex_replace @"\d+,\d+,\d+,\d+" (String.replace "." "," release.AssemblyVersion))
        path

let updateProject (project:Project) =
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
