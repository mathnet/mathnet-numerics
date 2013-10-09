#I "../../packages/FSharp.Formatting.1.0.15/lib/net40"
#load "../../packages/FSharp.Formatting.1.0.15/literate/literate.fsx"

open System
open System.IO
open FSharp.Literate

let (+/) l r = Path.GetFullPath(Path.Combine(l,r))

let source = __SOURCE_DIRECTORY__
let toolDir = source +/ "../../packages/FSharp.Formatting.1.0.15/lib/net40"
let libDir = source +/ "../../out/lib/Net40"
let templateDir = source
let inputDir = source +/ "../../src/FSharpExamples"
let outputDir = source +/ "../../out/docs"

let templateFile = "template.html"
let templateContent = ["content/style.css"; "content/tips.js"]

// Additional strings to be replaced in the HTML template
let projInfo =
  [ "page-description", ""
    "page-author", "Math.NET Team & Contributors; Christoph Ruegg"
    "project-name", "Math.NET Numerics" ]

let compilerReferences =
  [ toolDir +/ "FSharp.CompilerBinding.dll"
    toolDir +/ "FSharp.CodeFormat.dll"
    toolDir +/ "FSharp.Markdown.dll"
    "System.Web.dll"
    libDir +/ "MathNet.Numerics.dll"
    libDir +/ "MathNet.Numerics.FSharp.dll" ]

// Compiler options (reference the two dll files and System.Web.dll)
let compilerOptions = (List.fold (fun s p -> s + sprintf """--reference:"%s" """ p) "" compilerReferences).Trim()

// Copy template content files (scripts, styles, images, ..)
List.iter (fun tc -> File.Copy(templateDir +/ tc, outputDir +/ tc, true)) templateContent

// Now we can process the samples directory (with some additional references)
// and then we clean up the files & directories we had to create earlier
Literate.ProcessDirectory(inputDir, templateDir +/ templateFile, outputDir, OutputKind.Html, replacements=projInfo, compilerOptions=compilerOptions)