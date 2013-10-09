(**
Literate programming for F#
===========================

Implementation
--------------

This document is written as a literate F# script file, so the remaining text
is an overview of the implementation. The implementation uses `FSharp.Markdown.dll`
and `FSharp.CodeFormat.dll` to colorize F# source & parse Markdown:
*)

(*** hide ***)
namespace FSharp.Literate
#if INTERACTIVE
#I "../bin/"
#r "System.Web.dll"
#r "FSharp.Markdown.dll"
#r "FSharp.CodeFormat.dll"
#load "StringParsing.fs"
#endif

open System
open System.IO
open System.Web
open System.Reflection
open System.Collections.Generic

open FSharp.Patterns
open FSharp.CodeFormat
open FSharp.Markdown

(** 
### OutputKind type

The following type defines the two possible output types from literate script:
HTML and LaTeX.

*)
[<RequireQualifiedAccess>]
type OutputKind =
  | Html
  | Latex
  (*[omit:(members omitted)]*)

  /// Name of the format (used as a file extension)
  override x.ToString() = 
    match x with
    | Html -> "html"
    | Latex -> "tex"
  
  /// Format a given document as HTML or LaTeX depending on the current kind
  member x.Format(doc) =
    match x with
    | OutputKind.Html -> Markdown.WriteHtml(doc)
    | OutputKind.Latex -> Markdown.WriteLatex(doc)

  /// The name of the {tag} that is used for pasting content into a template file
  /// (the default is {document}, but that collides in LaTeX)
  member x.ContentTag =
    match x with 
    | OutputKind.Html -> "document" 
    | OutputKind.Latex -> "contents" 
  (*[/omit]*)

(**
### CommandUtils module

Utilities for parsing commands. Commands can be used in different places. We 
recognize `key1=value, key2=value` and also `key1:value, key2:value`
*)
module internal CommandUtils = 
  (*[omit:(Implementation omitted)]*)

  let (|ParseCommands|_|) (str:string) = 
    let kvs = 
      [ for cmd in str.Split(',') do
          let kv = cmd.Split([| '='; ':' |])
          if kv.Length = 2 then yield kv.[0].Trim(), kv.[1].Trim()
          elif kv.Length = 1 then yield kv.[0].Trim(), "" ] 
    if kvs <> [] then Some(dict kvs) else None
  
  let (|Command|_|) k (d:IDictionary<_, _>) =
    match d.TryGetValue(k) with
    | true, v -> Some v
    | _ -> None 
  (*[/omit]*)

(** 
### LiterateUtils module

Utilities for processing Markdown documents - extract links for references,
add links to references, extract code blocks for colorization and replace them
with formatted HTML (after running F# code formatter)
*)
module internal LiterateUtils = 
  (*[omit:(Implementation omitted)]*)
  open CommandUtils

  /// Given Markdown document, get the keys of all IndirectLinks 
  /// (to be used when generating paragraph with all references)
  let rec collectReferences = 

    // Collect IndirectLinks in a span
    let rec collectSpanReferences span = seq { 
      match span with
      | IndirectLink(_, _, key) -> yield key
      | Matching.SpanLeaf _ -> ()
      | Matching.SpanNode(_, spans) ->
          for s in spans do yield! collectSpanReferences s }

    // Collect IndirectLinks in a paragraph
    let rec loop par = seq {
      match par with
      | Matching.ParagraphLeaf _ -> ()
      | Matching.ParagraphNested(_, pars) -> 
          for ps in pars do 
            for p in ps do yield! loop p 
      | Matching.ParagraphSpans(_, spans) ->
          for s in spans do yield! collectSpanReferences s }
    loop 

  /// Given Markdown document, add a number using the given index to all indirect 
  /// references. For example, [article][ref] becomes [article][ref] [1](#rfxyz)
  let replaceReferences (refIndex:IDictionary<string, int>) =

    // Replace IndirectLinks with a nice link given a single span element
    let rec replaceSpans = function
      | IndirectLink(body, original, key) ->
          [ yield IndirectLink(body, original, key)
            match refIndex.TryGetValue(key) with
            | true, i -> 
                yield Literal "&#160;["
                yield DirectLink([Literal (string i)], ("#rf" + DateTime.Now.ToString("yyMMddhh"), None))
                yield Literal "]"
            | _ -> () ]
      | Matching.SpanLeaf(sl) -> [Matching.SpanLeaf(sl)]
      | Matching.SpanNode(nd, spans) -> 
          [ Matching.SpanNode(nd, List.collect replaceSpans spans) ]

    // Given a paragraph, process it recursively and transform all spans
    let rec loop = function
      | Matching.ParagraphNested(pn, nested) ->
          Matching.ParagraphNested(pn, List.map (List.choose loop) nested) |> Some
      | Matching.ParagraphSpans(ps, spans) -> 
          Matching.ParagraphSpans(ps, List.collect replaceSpans spans) |> Some
      | Matching.ParagraphLeaf(pl) -> Matching.ParagraphLeaf(pl) |> Some   
    loop

  /// Iterate over Markdown document and extract all F# code snippets that we want
  /// to colorize. We skip snippets that specify non-fsharp langauge e.g. [lang=csharp].
  let rec collectCodeSnippets par = seq {
    match par with
    | CodeBlock(String.StartsWithWrapped ("[", "]") (ParseCommands cmds, String.TrimStart code)) 
        when cmds.ContainsKey("lang") && cmds.["lang"] <> "fsharp" -> ()
    | CodeBlock(String.StartsWithWrapped ("[", "]") (ParseCommands cmds, String.TrimStart code)) 
    | CodeBlock(Let (dict []) (cmds, code)) ->
        let modul = 
          match cmds.TryGetValue("module") with
          | true, v -> Some v | _ -> None
        yield modul, code
    | Matching.ParagraphLeaf _ -> ()
    | Matching.ParagraphNested(_, pars) -> 
        for ps in pars do 
          for p in ps do yield! collectCodeSnippets p 
    | Matching.ParagraphSpans(_, spans) -> () }

  /// Replace CodeBlock elements with formatted HTML that was processed by the F# snippets tool
  /// (The dictionary argument is a map from original code snippets to formatted HTML snippets.)
  let rec replaceCodeSnippets outputKind (codeLookup:IDictionary<_, _>) = function
    | CodeBlock(String.StartsWithWrapped ("[", "]") (ParseCommands cmds, String.TrimStart code)) 
        when cmds.ContainsKey("hide") -> None
    | CodeBlock(String.StartsWithWrapped ("[", "]") (ParseCommands cmds, String.TrimStart code)) 
    | CodeBlock(Let (dict []) (cmds, code)) ->
        if (cmds.ContainsKey("lang")) && cmds.["lang"] <> "fsharp" then 
            let content = 
                if outputKind = OutputKind.Html then
                   "<pre lang=\"" + cmds.["lang"] + "\">" + HttpUtility.HtmlEncode(code) + "</pre>"
                else sprintf "\\begin{lstlisting}\n%s\n\\end{lstlisting}" <| HttpUtility.HtmlDecode(code) 
            HtmlBlock(content) |> Some
        else
            let content : string = codeLookup.[code]
            HtmlBlock(content) |> Some

    // Recursively process nested paragraphs, other nodes return without change
    | Matching.ParagraphNested(pn, nested) ->
        let pars = List.map (List.choose (replaceCodeSnippets outputKind codeLookup)) nested
        Matching.ParagraphNested(pn, pars) |> Some
    | other -> Some other

  /// Try find first-level heading in the paragraph collection
  let findHeadings paragraphs (outputKind:OutputKind) =              
    paragraphs |> Seq.tryPick (function 
      | (Heading(1, text)) -> 
          let doc = MarkdownDocument([Span(text)], dict [])
          Some(outputKind.Format(doc))
      | _ -> None)
  (*[/omit]*)

(**
### CodeBlockUtils module

Parsing of F# Script files with Markdown commands. Given a parsed script file, we 
split it into a sequence of comments, snippets and commands (comment starts with 
`(**` and ending with `*)` are translated to Markdown, snippet is all other F# code 
and command looks like `(*** key1:value, key2:value ***)` (and should be single line).
*)
module internal CodeBlockUtils =
  (*[omit:(Implementation omitted)]*)
  open CommandUtils

  type Block = 
    | BlockComment of string
    | BlockSnippet of Line list 
    | BlockCommand of IDictionary<string, string>

  /// Trim blank lines from both ends of a lines list & reverse it (we accumulate 
  /// lines & we want to remove all blanks before returning BlockSnippet)
  let private trimBlanksAndReverse lines = 
    lines 
    |> Seq.skipWhile (function Line[] -> true | _ -> false)
    |> List.ofSeq |> List.rev
    |> Seq.skipWhile (function Line[] -> true | _ -> false)
    |> List.ofSeq

  /// Succeeds when a line (list of tokens) contains only Comment 
  /// tokens and returns the text from the comment as a string
  let private (|ConcatenatedComments|_|) (Line tokens) =
    let comments =
      tokens |> List.choose (function
        | Token(TokenKind.Comment, text, _) -> Some text
        | _ -> None)
    if comments.Length <> tokens.Length then None
    else Some (String.concat "" comments)

  // Process lines of an F# script file. Simple state machine with two states
  //  * collectComment - we're parsing a comment and waiting for the end
  //  * collectSnippet - we're in a normal F# code and we're waiting for a comment
  //    (in both states, we also need to recognize (*** commands ***)

  /// Waiting for the end of a comment      
  let rec private collectComment (comment:string) lines = seq {
    match lines with
    | (ConcatenatedComments(String.StartsAndEndsWith ("(***", "***)") (ParseCommands cmds)))::lines ->
        // Ended with a command, yield comment, command & parse the next as a snippet
        let cend = comment.LastIndexOf("*)")
        yield BlockComment (comment.Substring(0, cend))
        yield BlockCommand cmds
        yield! collectSnippet [] lines

    | (ConcatenatedComments text)::_ when 
        comment.LastIndexOf("*)") <> -1 && text.Trim().StartsWith("//") ->
        // Comment ended, but we found a code snippet starting with // comment
        let cend = comment.LastIndexOf("*)")
        yield BlockComment (comment.Substring(0, cend))
        yield! collectSnippet [] lines

    | (Line[Token(TokenKind.Comment, String.StartsWith "(**" text, _)])::lines ->
        // Another block of Markdown comment starting... 
        // Yield the previous snippet block and continue parsing more comments
        let cend = comment.LastIndexOf("*)")
        yield BlockComment (comment.Substring(0, cend))
        if lines <> [] then yield! collectComment text lines

    | (ConcatenatedComments text)::lines  ->
        // Continue parsing comment
        yield! collectComment (comment + "\n" + text) lines

    | lines ->
        // Ended - yield comment & continue parsing snippet
        let cend = comment.LastIndexOf("*)")
        yield BlockComment (comment.Substring(0, cend))
        if lines <> [] then yield! collectSnippet [] lines }

  /// Collecting a block of F# snippet
  and private collectSnippet acc lines = seq {
    match lines with 
    | (ConcatenatedComments(String.StartsAndEndsWith ("(***", "***)") (ParseCommands cmds)))::lines ->
        // Found a special command, yield snippet, command and parse another snippet
        if acc <> [] then yield BlockSnippet (trimBlanksAndReverse acc)
        yield BlockCommand cmds
        yield! collectSnippet [] lines

    | (Line[Token(TokenKind.Comment, String.StartsWith "(**" text, _)])::lines ->
        // Found a comment - yield snippet & switch to parsing comment state
        if acc <> [] then yield BlockSnippet (trimBlanksAndReverse acc)
        yield! collectComment text lines

    | x::xs ->  yield! collectSnippet (x::acc) xs
    | [] -> yield BlockSnippet (trimBlanksAndReverse acc) }

  /// Parse F# script file into a sequence of snippets, comments and commands
  let parseScriptFile = collectSnippet []

  /// Given a parsed script file, extract "definitions". A definition is a part of 
  /// the file that we want to include elsewhere (and hide in the original location):
  ///
  ///     (*** define:key ***)
  ///     let foo = 1 + 2
  ///
  /// This function returns 'string * Block' list containing all definitions 
  /// together with all a list of all remaining blocks that were not extracted.
  let extractDefinitions defns =
    let rec loop defns normal = function
      | [] -> defns, normal |> List.rev
      | BlockCommand(Command "hide" _)::(BlockSnippet _)::rest -> 
          loop defns normal rest
      | BlockCommand(Command "define" key)::(BlockSnippet lines)::rest -> 
          // If we have command with 'define' in it, then pick the following 
          // snippet (it should be a snippet) and return it as a definition
          loop ((key, lines)::defns) normal rest
      | current::rest ->
          loop defns (current::normal) rest
    defns |> List.ofSeq |> loop [] [] 
  (*[/omit]*)

(**
### SourceProcessors module

Functions that process `*.fsx` and `*.md` files. The function `processScriptFile`
assumes that the file is an F# script file (with text hidden in comments) while 
`processMarkdown` assumes that all F# code is included as code snippets.
*)
module internal SourceProcessors = 

  /// Specifies a context that is passed to the 
  /// code/document processing functions
  type ProcessingContext = 
    { // An instance of the F# code formatting agent
      FormatAgent : CodeFormatAgent 
      // Source code of a HTML template file
      Template : string option
      // Short prefix code added to all HTML 'id' elements
      Prefix : string 
      // Should the processing add 'References' section?
      GenerateReferences : bool
      // Additional replacements to be made in the template file
      Replacements : list<string * string> 
      // Generate line numbers for F# snippets?
      GenerateLineNumbers : bool 
      // Include the source file in the generated output as '{source}'
      IncludeSource : bool
      // Command line options for the F# compiler
      Options : string 
      // The output format
      OutputKind : OutputKind
      // Custom function for reporting errors 
      ErrorHandler : option<string * SourceError -> unit> }

  (*[omit:(Implementation omitted)]*)
  open CommandUtils
  open CodeBlockUtils
  open LiterateUtils
  
  /// Print information about all errors during the processing
  let private reportErrors ctx file (errors:seq<SourceError>) = 
    match ctx.ErrorHandler with
    | Some eh -> for e in errors do eh(file, e)
    | _ ->
        for (SourceError((sl, sc), (el, ec), kind, msg)) in errors do
          printfn "   * (%d:%d)-(%d:%d) (%A): %s" sl sc el ec kind msg
        if Seq.length errors > 0 then printfn ""

  /// Given all links defined in the Markdown document and a list of all links
  /// that are accessed somewhere from the document, generate References paragraph
  let generateReferences (definedLinks:IDictionary<_, string * string option>) refs outputKind = 
    
    // For all unique references in the document, 
    // get the link & title from definitions
    let refs = 
      refs |> set |> Seq.choose (fun ref ->
        match definedLinks.TryGetValue(ref) with
        | true, (link, Some title) -> Some (ref, link, title)
        | _ -> None)
      |> Seq.sort |> Seq.mapi (fun i v -> i+1, v)
    // Generate dictionary with a number for all references
    let refLookup = dict [ for (i, (r, _, _)) in refs -> r, i ]

    // Generate Markdown blocks paragraphs representing Reference <li> items
    let refList = 
      [ for i, (ref, link, title) in refs do 
          let colon = title.IndexOf(":")
          if colon > 0 then
            let auth = title.Substring(0, colon)
            let name = title.Substring(colon + 1, title.Length - 1 - colon)
            yield [Span [ Literal (sprintf "[%d] " i)
                          DirectLink([Literal name], (link, Some title))
                          Literal (" - " + auth)] ] 
          else
            yield [Span [ Literal (sprintf "[%d] " i)
                          DirectLink([Literal title], (link, Some title))]]  ]

    // Return the document together with dictionary for looking up indices
    let literal =
        match outputKind with
        | OutputKind.Html ->
            // Return the document together with dictionary for looking up indices
            let id = DateTime.Now.ToString("yyMMddhh")
            Literal ("<a name=\"rf" + id + "\">&#160;</a>References")
        | OutputKind.Latex ->
            // Add formatting later
            Literal ("References")
    [ Heading(3, [literal])
      ListBlock(MarkdownListKind.Unordered, refList) ], refLookup

  /// Replace {parameter} in the input string with 
  /// values defined in the specified list
  let replaceParameters parameters input = 
    match input with 
    | None ->
        // If there is no template, return just document + tooltips
        let lookup = parameters |> dict
        lookup.["document"] + "\n\n" + lookup.["tooltips"]
    | Some input ->
        // First replace keys with some uglier keys and then replace them with values
        // (in case one of the keys appears in some other value)
        let id = System.Guid.NewGuid().ToString("d")
        let input = parameters |> Seq.fold (fun (html:string) (key, value) -> 
          html.Replace("{" + key + "}", "{" + key + id + "}")) input
        let result = parameters |> Seq.fold (fun (html:string) (key, value) -> 
          html.Replace("{" + key + id + "}", value)) input
        result 

  /// Write formatted blocks to a specified string builder 
  /// and return first-level heading if there is some
  let outputBlocks (sb:Text.StringBuilder) 
      // Original blocks of the input document
      blocks
      // Sequence with just formatted BlockSnippet elements
      (snippets:seq<FormattedSnippet>)
      // Sequence with just formatted BlockComment elements
      (comments:seq<MarkdownDocument>)
      (definitions:IDictionary<_, string>) refLookup outputKind =
    
    // We traverse sequences using enumerators as we need them
    let heading = ref None
    use snippetsEn = snippets.GetEnumerator()
    use commentsEn = comments.GetEnumerator()
    let nextSnippet () = snippetsEn.MoveNext() |> ignore; snippetsEn.Current
    let nextComment () = commentsEn.MoveNext() |> ignore; commentsEn.Current

    for block in blocks do
      match block with
      // Skip known commands and comments ('hide' is removed in earlier step)
      | BlockCommand (Command "include" key) -> sb.Append(definitions.[key]) |> ignore
      | BlockCommand (Command "define" _) -> ()
      | BlockCommand cmds when cmds.Count = 1 && cmds.Keys |> Seq.head |> Seq.forall ((=) '*') -> ()
      | BlockCommand cmds -> 
          failwithf "Unsupported command: %s" (String.concat ", " [ for (KeyValue(k,v)) in cmds -> k + ":" + v ])

      // Emit next comment, but search for headings
      | BlockComment s -> 
          let mdoc = nextComment()
          let paragraphs = mdoc.Paragraphs |> List.choose (replaceReferences refLookup) 
          findHeadings paragraphs outputKind |> Option.iter (fun v -> heading := Some v)
          let doc = MarkdownDocument(paragraphs, mdoc.DefinedLinks)
          sb.Append(outputKind.Format(doc)) |> ignore
      
      // Emit next snippet (if it is not just empty list)
      | BlockSnippet lines ->
          let snip = nextSnippet()
          if lines <> [] then sb.Append(snip.Content) |> ignore
    !heading

  // ------------------------------------------------------------------------------------

  /// Process F# Script file
  let processScriptFile ctx file output =
    let name = Path.GetFileNameWithoutExtension(file)

    // Parse the entire file as an F# script file,
    // get sequence of blocks & extract definitions
    let sourceSnippets, errors = ctx.FormatAgent.ParseSource(file, File.ReadAllText(file), ctx.Options)
    reportErrors ctx file errors
    let (Snippet(_, lines)) = match sourceSnippets with [| it |] -> it | _ -> failwith "multiple snippets"
    let definitions, blocks = parseScriptFile lines |> extractDefinitions

    // Process all definitions & build a dictionary with HTML for each definition
    let snippets = [| for name, lines in definitions -> Snippet(name, lines) |] 
    let formattedDefns = 
        match ctx.OutputKind with
        | OutputKind.Html -> CodeFormat.FormatHtml(snippets, ctx.Prefix + "d", ctx.GenerateLineNumbers, false)
        | OutputKind.Latex -> CodeFormat.FormatLatex(snippets, ctx.GenerateLineNumbers)
    let definitions = dict [ for snip in formattedDefns.Snippets -> snip.Title, snip.Content ]
    
    // Process all snippet blocks in the script file (using F# formatter)
    let snippets = blocks |> List.choose (function
        | BlockSnippet(lines) -> Some(Snippet("Untitled", lines))
        | _ -> None) |> Array.ofList
    let formatted = 
        match ctx.OutputKind with
        | OutputKind.Html -> CodeFormat.FormatHtml(snippets, ctx.Prefix, ctx.GenerateLineNumbers, false)
        | OutputKind.Latex -> CodeFormat.FormatLatex(snippets, ctx.GenerateLineNumbers)

    // Parse all comment blocks in the script file (as Markdown)
    let parsedBlocks = blocks |> Array.ofSeq |> Seq.choose (function
        | BlockComment(text) -> Some(Markdown.Parse(text))
        | _ -> None) 

    // Turn all indirect links into a references & add paragraph to the document
    let refParagraph, refLookup = 
      if ctx.GenerateReferences then 
        // Union link definitions & collect all indirect links
        let definedLinks = parsedBlocks |> Seq.collect (fun mdoc -> 
          [ for (KeyValue(k, v)) in mdoc.DefinedLinks -> k, v]) |> dict
        let refs = parsedBlocks |> Seq.collect (fun mdoc -> 
          Seq.collect collectReferences mdoc.Paragraphs)
        let pars, refLookup = generateReferences definedLinks refs ctx.OutputKind
        Some pars, refLookup
      else None, dict []

    // Write all HTML content to a string builder & add References    
    let sb = Text.StringBuilder()
    let heading = outputBlocks sb blocks formatted.Snippets parsedBlocks definitions refLookup ctx.OutputKind
    refParagraph |> Option.iter (fun p -> 
      let output = ctx.OutputKind.Format(MarkdownDocument(p, dict []))
      sb.Append(output) |> ignore)  
    
    // If we want to include the source code of the script, then process
    // the entire source and generate replacement {source} => ...some html...
    let sourceReplacement, sourceTips =
      match ctx.OutputKind with
      | OutputKind.Html ->
        if ctx.IncludeSource then 
            let formatted = CodeFormat.FormatHtml(sourceSnippets, ctx.Prefix + "s")
            let content =
                match formatted.Snippets with
                | [| snip |] -> snip.Content
                | snips -> [ for s in snips -> sprintf "<h3>%s</h3>\n%s" s.Title s.Content ] |> String.concat ""
            [ "source", content ], formatted.ToolTip
        else [], ""
      | OutputKind.Latex ->
        if ctx.IncludeSource then 
            let formatted = CodeFormat.FormatLatex(sourceSnippets)
            let content =
                match formatted.Snippets with
                | [| snip |] -> snip.Content
                | snips -> [ for s in snips -> sprintf "\subsubsection{%s}\n%s" s.Title s.Content ] |> String.concat ""
            [ "source", content ], formatted.ToolTip
        else [], ""

    // Replace all parameters in the template & write to output
    let parameters = 
      ctx.Replacements @ sourceReplacement @
      [ "page-title", defaultArg heading name
        ctx.OutputKind.ContentTag, sb.ToString()
        "tooltips", formatted.ToolTip + formattedDefns.ToolTip + sourceTips ]
    File.WriteAllText(output, replaceParameters parameters ctx.Template)

  // ------------------------------------------------------------------------------------

  /// Process Markdown document
  let processMarkdown ctx file output =
    // Read file & parse Markdown document
    let name = Path.GetFileNameWithoutExtension(file)
    let originalSource = File.ReadAllText(file)
    let doc = Markdown.Parse(originalSource)

    // Turn all indirect links into a references & add paragraph to the document
    let refParagraph, refLookup = 
      if ctx.GenerateReferences then 
        // Union link definitions & collect all indirect links
        let refs = Seq.collect collectReferences doc.Paragraphs
        let pars, refLookup = generateReferences doc.DefinedLinks refs ctx.OutputKind
        Some pars, refLookup
      else None, dict []
    
    // Extract all CodeBlocks and pass them to F# snippets
    let codes = doc.Paragraphs |> Seq.collect collectCodeSnippets |> Array.ofSeq
    let codeLookup, tipsHtml = 
      if codes.Length = 0 then dict [], ""
      else
        // If there are some F# snippets, we build an F# source file
        let blocks = codes |> Seq.mapi (fun index (modul, code) ->
              match modul with
              | Some modul ->
                  // generate module & add indentation
                  "module " + modul + " =\n" +
                  "// [snippet:" + (string index) + "]\n" +
                  "    " + code.Replace("\n", "\n    ") + "\n" +
                  "// [/snippet]"
              | None ->
                  "// [snippet:" + (string index) + "]\n" +
                  code + "\n" +
                  "// [/snippet]" ) 

        // Process F# script file, report errors & build lookup table for replacement
        let modul = "module " + (new String(name |> Seq.filter Char.IsLetter |> Seq.toArray))
        let source = modul + "\r\n" + (String.concat "\n\n" blocks)
        let snippets, errors = ctx.FormatAgent.ParseSource(output + ".fs", source, ctx.Options)
        reportErrors ctx file errors
        let formatted = 
            match ctx.OutputKind with
            | OutputKind.Html -> CodeFormat.FormatHtml(snippets, ctx.Prefix, ctx.GenerateLineNumbers, false)
            | OutputKind.Latex -> CodeFormat.FormatLatex(snippets, ctx.GenerateLineNumbers)
        let snippetLookup = 
          [ for (_, code), fs in Array.zip codes formatted.Snippets -> code, fs.Content ]
        dict snippetLookup, formatted.ToolTip

    // Process all paragraphs in two steps (replace F# snippets & references)
    let paragraphs = 
      doc.Paragraphs |> List.choose (fun par ->
        par |> replaceCodeSnippets ctx.OutputKind codeLookup
            |> Option.bind (replaceReferences refLookup)) 

    // If we want to include the source code of the script, then process
    // the entire source and generate replacement {source} => ...some html...
    let sourceReplacements =
      if ctx.IncludeSource then
        let doc = MarkdownDocument([CodeBlock originalSource], dict [])
        let content = ctx.OutputKind.Format(doc)
        [ "source", content ]
      else []

    // Construct new Markdown document and write it
    let parameters = 
      ctx.Replacements @ sourceReplacements @
      [ "page-title", defaultArg (findHeadings paragraphs ctx.OutputKind) name
        ctx.OutputKind.ContentTag, ctx.OutputKind.Format(MarkdownDocument(paragraphs, doc.DefinedLinks))
        "tooltips", tipsHtml ]
    File.WriteAllText(output, replaceParameters parameters ctx.Template)
  (*[/omit]*)


(** 

## Public API

The following type provides three simple methods for calling the literate programming tool.
The `ProcessMarkdown` and `ProcessScriptFile` methods process a single Markdown document
and F# script, respectively. The `ProcessDirectory` method handles an entire directory tree
(looking for `*.fsx` and `*.md` files).
*)
open SourceProcessors
 
type Literate = 
  (*[omit:(Helper methdods omitted)]*)
  /// Provides default values for all optional parameters
  static member private DefaultArguments
      ( input, templateFile, output,format, fsharpCompiler, prefix, compilerOptions, 
        lineNumbers, references, replacements, includeSource, errorHandler) = 
    let defaultArg v f = match v with Some v -> v | _ -> f()

    let outputKind = defaultArg format (fun _ -> OutputKind.Html)

    let output = defaultArg output (fun () ->
      let dir = Path.GetDirectoryName(input)
      let file = Path.GetFileNameWithoutExtension(input)
      Path.Combine(dir, sprintf "%s.%O" file outputKind))
    let fsharpCompiler = defaultArg fsharpCompiler (fun () -> 
      Assembly.Load("FSharp.Compiler"))
    
    // Build & return processing context
    let ctx = 
      { FormatAgent = CodeFormat.CreateAgent(fsharpCompiler) 
        Template = templateFile |> Option.map (fun file -> File.ReadAllText(file))
        Prefix = defaultArg prefix (fun () -> "fs")
        Options = defaultArg compilerOptions (fun () -> "")
        GenerateLineNumbers = defaultArg lineNumbers (fun () -> true)
        GenerateReferences = defaultArg references (fun () -> false)
        Replacements = defaultArg replacements (fun () -> []) 
        IncludeSource = defaultArg includeSource (fun () -> false) 
        OutputKind = outputKind
        ErrorHandler = errorHandler }
    output, ctx(*[/omit]*)

  /// Process Markdown document
  static member ProcessMarkdown
    ( input, ?templateFile, ?output, ?format, ?fsharpCompiler, ?prefix, ?compilerOptions, 
      ?lineNumbers, ?references, ?replacements, ?includeSource, ?errorHandler ) = (*[omit:(...)]*)
    let output, ctx = 
      Literate.DefaultArguments
        ( input, templateFile, output, format, fsharpCompiler, prefix, compilerOptions, 
          lineNumbers, references, replacements, includeSource, errorHandler )
    processMarkdown ctx input output (*[/omit]*)

  /// Process F# Script file
  static member ProcessScriptFile
    ( input, ?templateFile, ?output, ?format, ?fsharpCompiler, ?prefix, ?compilerOptions, 
      ?lineNumbers, ?references, ?replacements, ?includeSource, ?errorHandler ) = (*[omit:(...)]*)
    let output, ctx = 
      Literate.DefaultArguments
        ( input, templateFile, output, format, fsharpCompiler, prefix, compilerOptions, 
          lineNumbers, references, replacements, includeSource, errorHandler )
    processScriptFile ctx input output (*[/omit]*)

  /// Process directory containing a mix of Markdown documents and F# Script files
  static member ProcessDirectory
    ( inputDirectory, ?templateFile, ?outputDirectory, ?format, ?fsharpCompiler, ?prefix, ?compilerOptions, 
      ?lineNumbers, ?references, ?replacements, ?includeSource, ?errorHandler ) = (*[omit:(...)]*)
    let _, ctx = 
      Literate.DefaultArguments
        ( "", templateFile, Some "", format, fsharpCompiler, prefix, compilerOptions, 
          lineNumbers, references, replacements, includeSource, errorHandler )
 
    /// Recursively process all files in the directory tree
    let rec processDirectory indir outdir = 
      // Create output directory if it does not exist
      if Directory.Exists(outdir) |> not then
        try Directory.CreateDirectory(outdir) |> ignore 
        with _ -> failwithf "Cannot create directory '%s'" outdir

      let fsx = [ for f in Directory.GetFiles(indir, "*.fsx") -> processScriptFile, f ]
      let mds = [ for f in Directory.GetFiles(indir, "*.md") -> processMarkdown, f ]
      for func, file in fsx @ mds do
        let name = Path.GetFileNameWithoutExtension(file)
        let output = Path.Combine(outdir, sprintf "%s.%O" name ctx.OutputKind)

        // Update only when needed
        let changeTime = File.GetLastWriteTime(file)
        let generateTime = File.GetLastWriteTime(output)
        if changeTime > generateTime then
          printfn "Generating '%s.%O'" name ctx.OutputKind
          func ctx file output

    let outputDirectory = defaultArg outputDirectory inputDirectory
    processDirectory inputDirectory outputDirectory (*[/omit]*)