// Given a typical setup (with 'FSharp.Formatting' referenced using NuGet),
// the following will include binaries and load the literate script
#I "../bin"
#load "literate.fsx"
open FSharp.Literate

/// This functions processes a single F# Script file
let processScript templateFile outputKind =
  let file = __SOURCE_DIRECTORY__ + "\\test.fsx"
  let output = __SOURCE_DIRECTORY__ + "\\outputs\\test." + (outputKind.ToString())
  let template = __SOURCE_DIRECTORY__ + templateFile
  Literate.ProcessScriptFile(file, template, output, format = outputKind)

/// This functions processes a single Markdown document
let processDocument templateFile outputKind =
  let file = __SOURCE_DIRECTORY__ + "\\demo.md"
  let output = __SOURCE_DIRECTORY__ + "\\outputs\\demo." + (outputKind.ToString())
  let template = __SOURCE_DIRECTORY__ + templateFile
  Literate.ProcessMarkdown(file, template, output, format = outputKind)

/// This functions processes an entire directory containing
/// multiple script files (*.fsx) and Markdown documents (*.md)
/// and it specifies additional replacements for the template file
let processDirectory() =
  let dir = __SOURCE_DIRECTORY__
  let template = __SOURCE_DIRECTORY__ + "\\templates\\template-project.html"
  let projInfo =
    [ "page-description", "F# Literate Programming"
      "page-author", "Tomas Petricek"
      "github-link", "https://github.com/tpetricek/FSharp.Formatting"
      "project-name", "F# Formatting" ]

  Literate.ProcessDirectory
    ( dir, template, dir + "\\output", OutputKind.Html, 
      replacements = projInfo)

// Generate output for sample scripts & documents in both HTML & Latex
processScript "\\templates\\template-file.html" OutputKind.Html
processDocument "\\templates\\template-file.html" OutputKind.Html
processScript "\\templates\\template-color.tex" OutputKind.Latex
processDocument "\\templates\\template-color.tex" OutputKind.Latex