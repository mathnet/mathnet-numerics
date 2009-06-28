// (c) Microsoft Corporation. All rights reserved

// Driver for F# compiler. 
// 
// Roughly divides into:
//    - Parsing
//    - Flags 
//    - Importing IL assemblies
//    - Compiling (including optimizing and managing ccu_thunks)
//    - Linking (including ILX-IL transformation)

#light

module internal Microsoft.FSharp.Compiler.Driver 

open System.IO
open System.Collections.Generic
open Internal.Utilities
open Internal.Utilities.Pervasives
open Microsoft.FSharp.Compiler 
open Microsoft.FSharp.Compiler.AbstractIL 
open Microsoft.FSharp.Compiler.AbstractIL.IL 
open Microsoft.FSharp.Compiler.AbstractIL.Internal 
open Microsoft.FSharp.Compiler.AbstractIL.Extensions.ILX
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library 

module Ilsupp = Microsoft.FSharp.Compiler.AbstractIL.Internal.Support 
module Ilmorph = Microsoft.FSharp.Compiler.AbstractIL.Morphs 
module Ilwrite = Microsoft.FSharp.Compiler.AbstractIL.BinaryWriter 
module Ilprint = Microsoft.FSharp.Compiler.AbstractIL.AsciiWriter 

open System.Runtime.CompilerServices
open System.IO
open Microsoft.FSharp.Compiler.AbstractIL.Diagnostics
open Microsoft.FSharp.Compiler.AbstractIL.IL
open Microsoft.FSharp.Compiler.Ilxgen
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Range
open Microsoft.FSharp.Compiler.TypeChecker
open Microsoft.FSharp.Compiler.Infos.AccessibilityLogic
open Microsoft.FSharp.Compiler.Infos.AttributeChecking
open Microsoft.FSharp.Compiler.Tast
open Microsoft.FSharp.Compiler.Tastops
open Microsoft.FSharp.Compiler.Opt
open Microsoft.FSharp.Compiler.Env
open Microsoft.FSharp.Compiler.Build
open Microsoft.FSharp.Compiler.Lib
open Microsoft.FSharp.Compiler.Fscopts
open Microsoft.FSharp.Compiler.DiagnosticMessage


//----------------------------------------------------------------------------
// Reporting - warnings, errors
//----------------------------------------------------------------------------

let exiter = QuitProcessExiter

/// Create an error logger that counts and prints errors 
let ErrorLoggerThatQuitsAfterMaxErrors (tcConfig:TcConfig) = 

    let errors = ref 0

    { new ErrorLogger with 
           member x.ErrorSink(err) = 
                if !errors >= tcConfig.maxErrors then 
                    DoWithErrorColor true (fun () -> Printf.eprintfn "Exiting - too many errors") ; 
                    exiter.Exit 1

                DoWithErrorColor false (fun () -> 
                  (writeViaBufferWithEnvironmentNewLines stderr (OutputErrorOrWarning (tcConfig.implicitIncludeDir,tcConfig.showFullPaths,tcConfig.flatErrors,tcConfig.errorStyle,false)) err;  stderr.WriteLine()));

                incr errors

                match err with 
                | InternalError _ | Failure _ | Not_found -> 
                    match tcConfig.simulateException with
                    | Some(_) -> () // Don't show an assert for simulateException case so that unittests can run without an assert dialog.                     
                    | None -> System.Diagnostics.Debug.Assert(false,sprintf "Bug seen in compiler: %s" (err.ToString()))
                | _ -> 
                    ()
           member x.WarnSink(err) =  
                DoWithErrorColor true (fun () -> 
                    if (ReportWarningAsError tcConfig.globalWarnLevel tcConfig.specificWarnOff tcConfig.specificWarnAsError tcConfig.globalWarnAsError err) then 
                      x.ErrorSink(err)
                    elif ReportWarning tcConfig.globalWarnLevel tcConfig.specificWarnOff err then 
                      writeViaBufferWithEnvironmentNewLines stderr (OutputErrorOrWarning (tcConfig.implicitIncludeDir,tcConfig.showFullPaths,tcConfig.flatErrors,tcConfig.errorStyle,true)) err;  
                      stderr.WriteLine())
           member x.ErrorCount = !errors  }

let ErrorLoggerInitial (tcConfigB:TcConfigBuilder) = ErrorLoggerThatQuitsAfterMaxErrors(TcConfig.Create(tcConfigB,validate=false))

//let ignoreAllFailures f = try f() with _ -> ()

    


let BuildInitialDisplayEnvForDocGeneration (tcConfig:TcConfig,tcImports,tcGlobals) = 
    let denv = empty_denv tcGlobals
    let denv = 
        { denv with 
           showImperativeTyparAnnotations=true;
           showAttributes=true;
           openTopPaths=
                [ lib_MF_path ] @
                [ lib_MFCore_path ] @
                [ lib_MFColl_path ] @
                [ lib_MFControl_path ] @
                [ (IL.split_namespace lib_FSLib_Pervasives_name); ]  @
                 (if not tcConfig.compilingFslib  then 
                   [ (IL.split_namespace lib_MLLib_OCaml_name);
                     (IL.split_namespace lib_MLLib_FSharp_name);
                     (IL.split_namespace lib_MLLib_Pervasives_name); ] 
                  else [])  }
    denv.Normalize()


module InterfaceFileWriter =

    let WriteInterfaceFile (tcGlobals,tcConfig:TcConfig,tcImports,generatedCcu,TAssembly(declaredImpls)) =
        /// Use a UTF-8 Encoding with no Byte Order Mark
        let os = 
            if tcConfig.printSignatureFile="" then System.Console.Out
            else (File.CreateText tcConfig.printSignatureFile :> System.IO.TextWriter)

        if tcConfig.printSignatureFile <> "" then 
            Printf.fprintf os "#light\r\n\r\n" (* REVIEW: #light not need. and use fprintfn *)

        declaredImpls |>  List.iter (fun (TImplFile(_,_,mexpr)) ->
            let denv = BuildInitialDisplayEnvForDocGeneration(tcConfig,tcImports,tcGlobals)
            writeViaBufferWithEnvironmentNewLines os (fun os s -> Printf.bprintf os "%s\n\n" s)
              (NicePrint.InferredSigOfModuleExprL true denv mexpr |> Layout.squashTo 80 |> Layout.showL))
       
        if tcConfig.printSignatureFile <> "" then os.Close()


module XmlDocWriter =

    let getDoc xmlDoc = 
        match ProcessXmlDoc xmlDoc with
        | XmlDoc [| |] -> ""
        | XmlDoc strs  -> strs |> Array.to_list |> String.concat System.Environment.NewLine

    let writeXmlDoc (assemblyName,tcGlobals,generatedCcu:ccu,xmlfile) =
        if not (Filename.hasSuffixCaseInsensitive "xml" xmlfile ) then 
            error(Error("the documentation file has no .xml suffix", Range.rangeStartup));
        (* the xmlDocSigOf* functions encode type into string to be used in "id" *)
        let members = ref []
        let addMember id xmlDoc = 
            let doc = getDoc xmlDoc
            members := (id,doc) :: !members
        let g = tcGlobals
        let do_val ptext (v:Val)  = addMember (XmlDocSigOfVal g ptext v)   v.XmlDoc
        let do_tycon ptext (tc:Tycon) = 
            addMember (XmlDocSigOfTycon g ptext tc) tc.XmlDoc;
            List.iter (deref_val >> do_val ptext) (adhoc_of_tycon tc)

        let modulMember path (m:ModuleOrNamespace) = addMember (XmlDocSigOfSubModul g path) m.XmlDoc
        (* moduleSpec - recurses *)
        let rec do_modul path (mspec:ModuleOrNamespace) = 
            let mtype = mspec.ModuleOrNamespaceType
            let path = 
                (* skip the first item in the path which is the assembly name *)
                match path with 
                | None -> Some ""
                | Some "" -> Some (demangled_name_of_modul mspec)
                | Some p -> Some (p^"."^demangled_name_of_modul mspec)
            let ptext = match path with None -> "" | Some t -> t
            if mspec.IsModule then modulMember ptext mspec;
            let vals = 
                mtype.AllValuesAndMembers
                |> NameMap.range 
                |> List.filter (fun x  -> not x.IsCompilerGenerated) 
                |> List.filter (fun x -> x.MemberInfo.IsNone)
            List.iter (do_modul  path)  mtype.ModuleAndNamespaceDefinitions;
            List.iter (do_tycon  ptext) mtype.ExceptionDefinitions;
            List.iter (do_val    ptext) vals;
            List.iter (do_tycon  ptext) mtype.TypeDefinitions
       
        do_modul None generatedCcu.Contents;

        use os = File.CreateText(xmlfile)

        Printf.twprintfn os ("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        Printf.twprintfn os ("<doc>");
        Printf.twprintfn os ("<assembly><name>%s</name></assembly>") assemblyName;
        Printf.twprintfn os ("<members>");
        !members |> List.iter (fun (id,doc) -> 
            Printf.twprintfn os  "<member name=\"%s\">" id
            Printf.twprintfn os  "%s" doc
            Printf.twprintfn os  "</member>");
        Printf.twprintfn os "</members>"; 
        Printf.twprintfn os "</doc>";   



module HtmlDocWriter =

    // bug://1813. 
    // The generated HTML docs contain these embedded URLs.
    let urlForFSharp                = "http://research.microsoft.com/fsharp/"
    // These manual links should be links to reference copies of the library documentation directory (without the /-slash)
    let urlForFSharpCoreManual      = "http://research.microsoft.com/fsharp/manual/fslib" (* appended with "/Microsoft.FSharp.Collections.List.html" etc... *)
    let urlForFSharpPowerPackManual = "http://research.microsoft.com/fsharp/manual/mllib" (* appended with "/Microsoft.FSharp.Collections.List.html" etc... *)

    let getDoc xmlDoc = 
        match ProcessXmlDoc xmlDoc with
        | XmlDoc [| |]   -> 
            ""
        | XmlDoc strs -> 
            strs 
               |> Array.to_list 
               |> List.map (fun s -> s.Replace("<c>","<tt>").Replace("</c>","</tt>")) 
               |> String.concat "\n" 
    
    let pseudo_case_insensitive a b = 
        let c1 = compare (String.lowercase a) (String.lowercase b)
        if c1 <> 0 then c1 else 
        (* put upper case second  - makes real definitions like 'Code' come after abbreviations 'code *)
        -(compare a b)

    open Microsoft.FSharp.Compiler.Layout
    let width_val  = 80
    let width_type = 120
    let width_exn  = 80

    let rec split c str =
        if String.contains str c then
            let i = String.index str c
            String.sub str 0 i :: split c (String.sub str (i+1) (String.length str - (i+1)))
        else
            [str]


    /// String for layout squashed to a given width.
    /// HTML translation and markup for TYPE and VAL syntax.
    /// Assumes in <pre> context (since no explicit <br> linebreaks).
    let outputL width layout =
        let baseR = htmlR stringR
        let render = 
            { new render<_,_> with 
                 member r.Start () = baseR.Start()
                 member r.AddText z s = baseR.AddText z s;  (* REVIEW: escape HTML chars *)
                 member r.AddBreak z n = baseR.AddBreak z n
                 member r.Finish z = baseR.Finish z 
                 member x.AddTag z (tag,attrs,start) =
                          match tag,start with
                          | "TYPE",_     -> z
                          | "VAL" ,true  -> baseR.AddText z "<B>"
                          | "VAL" ,false -> baseR.AddText z "</B>"
                          | _     ,start -> baseR.AddTag z (tag,attrs,start)}
        renderL render (squashTo width layout)

    type kind = PathK | TypeK of int (* int is typar arity *)

    let WriteHTMLDoc (tcConfig:TcConfig,tcImports,tcGlobals,generatedCcu:ccu,outdir,append,css,nsfile,assemVerFromAttrib) =
        let assemblyName = generatedCcu.AssemblyName
        let outdir = match outdir with None -> "." | Some x -> x
        let rconcat a b = a^"/"^b
        let css = match css with None -> "msdn.css" | Some x -> x
        let nsfile = match nsfile with None -> "namespaces.html" | Some x -> x
        let nsfullfile = Path.Combine(outdir,nsfile)
        let nsbfilename = (rconcat ".." nsfile) in (* the name to use to link up to the namespace file *)    
        let wrap (oc:TextWriter) (a:string) (b:string) f = 
            oc.WriteLine a;
            f ();
            oc.WriteLine b

        let newExplicitFile append fullname bfilename title css f = 
            use oc = 
                if append && Internal.Utilities.FileSystem.File.SafeExists fullname then 
                    System.IO.File.AppendText fullname
                else 
                    System.IO.File.CreateText fullname

            fprintfn oc "<HTML><HEAD><TITLE>%s</TITLE><link rel=\"stylesheet\" type=\"text/css\"href=\"%s\"></link></HEAD><BODY>" title css;
            let backlink = (bfilename,title)
            f backlink oc;
            let ver = 
                match assemVerFromAttrib with 
                | None -> tcConfig.version.GetVersionString(tcConfig.implicitIncludeDir)
                | Some v -> IL.version_to_string v
            fprintfn oc "<br /> <br/><p><i>Documentation for assembly %s, version %s, generated using <a href='%s'>F# Programming Language</a> version %s</i></p>" assemblyName ver urlForFSharp Ilxconfig.version;
            fprintfn oc "</BODY></HTML>";

        let newFile fdir fname title f =
            let fullname = Path.Combine(outdir,Path.Combine(fdir,fname))
            newExplicitFile false fullname fname title (rconcat ".." css) f

        let hlink url text = sprintf "<a href='%s'>%s</a>" url text
       
        (* Path *)
        let path0        = []
        let path1 x kind = [(x,kind)]
        let pathExtend xs x kind = xs @ [x,kind] in (* short shallow lists *)
        let pathText     xs = String.concat "." (List.map fst xs)
        let pathFilename xs =
            let encode = function
              | x,PathK   -> x
              | x,TypeK w ->
                  // Mangle to avoid colliding upper/lower names, 'complex' and 'Complex', to different filenames 
                  // See also tastops.ml which prints type hrefs 
                  "type_" ^ (String.underscoreLowercase x) ^ (if w=0 then "" else "-" ^ string_of_int w)
            String.concat "." (List.map encode xs) ^ ".html"

        let collapseStrings xs = String.concat "." xs
        let pathWrt knownNamespaces x =
            let xs = split '.' x
            let rec collapse front back = 
              match back with 
              | [] -> (if front = [] then [] else [collapseStrings front]) @ back
              | mid::back -> 
                  if List.mem (collapseStrings (front@[mid])) knownNamespaces 
                  then [collapseStrings (front@[mid])] @ back
                  else collapse (front@[mid]) back
            List.map (fun x -> (x,PathK)) (collapse [] xs)

        let nestBlock hFile f =
            wrap hFile "<br><dl>" "</dl>"
               (fun () -> f())

        let nestItem hFile f =
            wrap hFile "<dt></dt><dd>" "</dd>"
               (fun () -> f())
          
        (* TopNav - from paths *)
        let newPathTrail hFile kind ptext =
            let rec writer prior = function
              | []         -> ()
              | [x,k]      -> fprintf hFile "%s " x
              | (x,k)::xks -> let prior = pathExtend prior x k
                              let url   = pathFilename prior
                              let sep   = if xks=[] then "" else "."
                              let item  = hlink url x
                              fprintf hFile "%s%s" item sep;
                              writer prior xks
            let uplink = sprintf "[<a href='%s'>Home</a>] " nsbfilename
            nestItem hFile (fun () ->
              fprintf hFile "<h1>%s%s " uplink kind;
              writer path0 ptext;
              fprintf hFile "</h1>";
              fprintf hFile "<br>\n")

        let newPartitions hFile f =
            nestItem hFile (fun () ->
              wrap hFile "<table>" "</table>" (fun () -> 
                f()))
        
        let newPartition hFile desc f =
            wrap hFile (sprintf "  <tr valign='top'><td>%s" desc) (sprintf "  </td></tr>") (fun () -> 
              f())

        let newSectionInPartition hFile title f =
            wrap hFile (sprintf "  <dt><h3>%s</h3></dt><dd>" title) (sprintf "  </dd>") (fun () -> 
              f())

        let newPartitionsWithSeeAlsoBacklink hFile (bfilename,btitle) f = 
            newPartitions hFile (fun () ->
              f();
              newPartition hFile "" (fun () -> 
                newSectionInPartition hFile "See Also" (fun () -> 
                  fprintf hFile "<a href=\"%s\">%s</a>" bfilename btitle)))

        let newTable0 hFile title f = 
            newSectionInPartition hFile title (fun () -> 
              wrap hFile "<table width=\"100%%\">" "</table>" (fun () -> 
                f()))

        let newTable2 hFile title width1 h1 h2 f = 
            newTable0 hFile title (fun () -> 
              wrap hFile (sprintf "<tr><th width=%d%%>%s</th><th>%s</th></tr>" width1 h1 h2) "" (fun () -> 
                f()))
        
        let newNamespaceEntry hFile fdir ptext allmods tycons desc =
            let fname = pathFilename ptext
            let title = pathText ptext
            let url = fdir ^ "/" ^ fname
            fprintf hFile "<tr valign='top'><td width='50%%'><a href='%s'>%s</a>\n" url title;
            (* Sort sub-modules into alpha order *)
            let allmods = 
                allmods 
                |> List.filter (fun (mspec:ModuleOrNamespace) -> mspec.IsModule)
                |> List.sortWith (orderOn demangled_name_of_modul pseudo_case_insensitive)
            (* Make them hyperlink to fdir/<path>.html *)
            let typeLinks = 
                tycons
                |> List.map (fun (tycon:Tycon) ->
                               let ptext = pathExtend ptext tycon.DisplayName (TypeK tycon.TyparsNoRange.Length)
                               let url   = fdir ^ "/" ^ pathFilename ptext
                               //hlink url (sprintf "[%s]" (tycon.DisplayNameWithUnderscoreTypars.Replace("<","&lt;").Replace(">","&gt;")))) 
                               hlink url (sprintf "[%s]" tycon.DisplayName)) 
                |> String.concat ", "
            let moduleLinks = 
                allmods
                |> List.map (fun modul ->
                               let ptext = pathExtend ptext (demangled_name_of_modul modul) PathK
                               let url   = fdir ^ "/" ^ pathFilename ptext
                               hlink url (sprintf "[%s]" (demangled_name_of_modul modul))) 
                |> String.concat ", "
            fprintfn hFile "</td>" ;
            fprintfn hFile  
                "<td>%s%s%s</td>" 
                desc 
                (if nonNil tycons then sprintf "<br><br>Types: %s" typeLinks else "")
                (if nonNil allmods then sprintf "<br><br>Modules: %s" moduleLinks else "");    
            fprintfn hFile "</tr>" ;
            ()

        let newEntry     hFile0 title desc = 
            fprintfn hFile0 "<tr valign=\"top\"><td>%s</td><td>%s</td></tr>" title desc

        let denv = BuildInitialDisplayEnvForDocGeneration(tcConfig,tcImports,tcGlobals)
        let denv = 
          { denv with 
              html = true;
              showObsoleteMembers=false;
              suppressInlineKeyword=true;
              shortConstraints=true;
              showOverrides=false;
              showConstraintTyparAnnotations=false;
              showImperativeTyparAnnotations=false;

              htmlAssemMap = 
               NameMap.of_list 
                 (let fslib_default = [(GetFSharpCoreLibraryName()     ,urlForFSharpCoreManual);]
                  let mllib_default = [(GetFSharpPowerPackLibraryName(),urlForFSharpPowerPackManual);]
                  if tcConfig.compilingFslib || tcConfig.htmlDocLocalLinks
                  then []
                  else (fslib_default @ mllib_default));  }

        newExplicitFile append nsfullfile nsbfilename "Namespaces" css (fun blinkNamespacesFile hNamespacesFile -> 

          wrap hNamespacesFile (sprintf "<dl><dt><br/></dt><dd><table><tr><th>Namespaces in assembly %s</th><th>Description</th></tr>" assemblyName)
                               (        "</table></dl>") (fun () ->

            let obsoleteText attribs = 
                match (TryFindAttrib tcGlobals tcGlobals.attrib_SystemObsolete attribs) with
                | Some(Attrib(_,_,(AttribStringArg(msg) :: _),_,_)) ->
                  sprintf "<p><b>Note</b>: %s</p>" msg 
                | _ -> ""
            let isObsolete attribs = 
                HasAttrib tcGlobals tcGlobals.attrib_SystemObsolete attribs 

            let IsUnseenVal (v:Val) = 
                not (IsValAccessible Infos.AccessibleFromEverywhere (mk_local_vref v)) ||
                v.IsCompilerGenerated ||
                isObsolete v.Attribs

            let IsUnseenEntity (e:Entity) = 
                not (IsEntityAccessible Infos.AccessibleFromEverywhere (mk_local_tcref e)) ||
                isObsolete e.Attribs

            let rec do_val denv fdir hFile ptext (v:Val) = 
                let denv = { denv with htmlHideRedundantKeywords=true }
                newEntry hFile ("<pre>"^outputL width_val (NicePrint.valL denv v)^"</pre>") (obsoleteText v.Attribs^getDoc v.XmlDoc)

            let rec do_vals denv fdir hFile ptext title item (vals:Val list) = 
                let vals = vals |> List.filter (fun v -> not v.IsCompilerGenerated)
                let vals = vals |> List.sortWith (orderOn (fun v -> v.DisplayName) pseudo_case_insensitive)
                if nonNil vals then 
                  newTable2 hFile title 60 item  "Description" (fun () -> 
                    vals |> List.iter (do_val denv fdir hFile ptext))

            let rec do_tycon denv fdir hFile ptext (tycon:Tycon) = 
                newSectionInPartition hFile "Full Type Signature" (fun () ->
                  fprintf hFile "<pre>%s</pre>" (outputL width_type (NicePrint.tyconL denv (match tycon.TypeOrMeasureKind  with KindMeasure -> wordL "[<Measure>] type" | KindType -> wordL "type") tycon)));
                let vals = adhoc_of_tycon tycon |> List.map deref_val
                let val_is_instance (v:Val) = 
                  assert v.IsMember;
                  (the (v.MemberInfo)).MemberFlags.MemberIsInstance
                let _,vals = vals |> List.partition IsUnseenVal
                let ivals,svals = List.partition val_is_instance vals
                do_vals denv fdir hFile ptext "Instance Members" "Member" ivals;
                do_vals denv fdir hFile ptext "Static Members" "Member"   svals;
                //do_vals denv fdir hFile ptext "Deprecated Members" "Member" dvals;

            let rec do_tycons denv fdir blinkFile hFile ptext title (tycons:Tycon list) = 
                if tycons <> [] then  
                  newTable2 hFile title 30 "Type" "Description" (fun () -> 
                    let tycons = tycons |> List.sortWith (orderOn (fun tc -> tc.DisplayName) pseudo_case_insensitive)
                    tycons |> List.iter (fun tycon ->
                      let tyname = tycon.DisplayName
                      let ptext  = pathExtend ptext tyname (TypeK tycon.TyparsNoRange.Length)
                      let fname  = pathFilename ptext
                      let title  = pathText ptext in  (* used as html page title *)
                      let text = obsoleteText tycon.Attribs
                      let text = text^(getDoc tycon.XmlDoc)
                      let text = 
                        match tycon.TypeAbbrev with 
                        | None -> text
                        | Some ty -> text ^ " Note: an abbreviation for "^("<tt>"^outputL width_type (NicePrint.typeL denv ty)^"</tt>")
                      newEntry hFile ("type " ^ hlink fname tyname) text;
                      newFile fdir fname title (fun blinkFile2 hFile2 ->
                        nestBlock hFile2 (fun () ->
                          newPathTrail hFile2 "Type" ptext;
                          newPartitionsWithSeeAlsoBacklink hFile2 blinkFile  (fun () -> 
                            newPartition hFile2 text (fun () ->
                              do_tycon denv fdir hFile2 ptext tycon))))))
            
            let rec do_exnc denv fdir hFile ptext exnc = 
                newSectionInPartition hFile "Full Signature" (fun () ->
                  fprintf hFile "<pre>%s</pre>" (outputL width_exn (NicePrint.exnDefnL denv exnc)))

            let rec do_exncs denv fdir blinkFile hFile ptext (exncs:Tycon list) = 
                if nonNil exncs then  
                    newTable2 hFile "Exceptions" 40 "Exception" "Description" (fun () -> 
                        let exncs = exncs |> List.sortWith (orderOn (fun exnc -> exnc.DemangledExceptionName) pseudo_case_insensitive)
                        exncs |> List.iter (fun exnc ->
                            let ptext  = pathExtend ptext exnc.DemangledExceptionName (TypeK exnc.TyparsNoRange.Length)
                            let fname  = pathFilename ptext
                            let exname = exnc.DemangledExceptionName
                            let title  = pathText ptext in  (* used as html page title *)
                            let text = obsoleteText exnc.Attribs
                            let text = text^(getDoc exnc.XmlDoc)
                            let text = 
                              match exnc.ExceptionInfo with
                              | TExnAbbrevRepr ecref as r -> text^" Note: an abbreviation for "^("<tt>"^outputL width_exn (NicePrint.exnDefnReprL denv r)^"</tt>")
                              | _ -> text
                            newEntry hFile ("exception " ^ hlink fname exname) text;
                            newFile fdir fname title (fun blinkFile2 hFile2 ->
                              nestBlock hFile2 (fun () ->
                                newPathTrail hFile2 "Exception" ptext;
                                newPartitionsWithSeeAlsoBacklink hFile2 blinkFile  (fun () -> 
                                  newPartition hFile2 text (fun () ->
                                    do_exnc denv fdir hFile2 ptext exnc))))))
            
            let rec do_modul denv fdir ptext blinkFile hFile modul = 
                let denv = denv_add_open_modref (mk_local_modref modul) denv
                let mtyp = modul.ModuleOrNamespaceType
                
                let moduls = 
                    mtyp.ModuleAndNamespaceDefinitions 
                    |> List.sortWith (orderOn demangled_name_of_modul pseudo_case_insensitive)
                    |> List.filter (IsUnseenEntity >> not)
                let tycons= mtyp.TypeDefinitions |> List.filter (IsUnseenEntity >> not)
                let vals = 
                    mtyp.AllValuesAndMembers 
                        |> NameMap.range 
                        |> List.filter (fun x -> not x.IsMember)
                        |> List.filter (IsUnseenVal >> not)

                let extvals = 
                    mtyp.AllValuesAndMembers 
                        |> NameMap.range 
                        |> List.filter (fun x -> x.IsExtensionMember)
                        |> List.filter (IsUnseenVal >> not)

                let apvals,vals = 
                    vals 
                        |> List.partition (mk_local_vref >> apinfo_of_vref >> isSome)

                do_moduls denv fdir blinkFile hFile ptext moduls; 
                do_tycons denv fdir blinkFile hFile ptext "Type Definitions" tycons;
                do_exncs denv fdir blinkFile hFile ptext mtyp.ExceptionDefinitions;
                do_vals denv fdir hFile ptext "Values" "Value" vals; 
                do_vals denv fdir hFile ptext "Active Patterns" "Active Pattern" apvals; 
                do_vals denv fdir hFile ptext "Extension Members" "Extension Member" extvals; 
                //do_tycons denv fdir blinkFile hFile ptext "Deprecated/Unsafe Type Definitions" dtycons;
                //do_vals denv fdir hFile ptext "Deprecated Values" "Value" dvals 

            and do_moduls denv fdir blinkFile hFile ptext moduls = 
                if moduls <> [] then  
                  newTable2 hFile (sprintf "Modules (as contributed by assembly '%s')" assemblyName) 30 "Module" "Description" (fun () -> 
                    let moduls = moduls |> List.sortWith (orderOn demangled_name_of_modul pseudo_case_insensitive)
                    moduls |> List.iter (fun modul -> 
                      let mtyp = modul.ModuleOrNamespaceType
                      let ptext = pathExtend ptext (demangled_name_of_modul modul) PathK
                      let fname = pathFilename ptext
                      let title = pathText ptext
                      let text = obsoleteText modul.Attribs
                      let text  = getDoc modul.XmlDoc
                      newEntry hFile (hlink fname title) text;
                      newFile fdir fname title (fun blinkFile2 hFile2 ->
                        nestBlock hFile2 (fun () ->
                          newPathTrail hFile2 "Module" ptext;
                          newPartitionsWithSeeAlsoBacklink hFile2 blinkFile  (fun () -> 
                            newPartition hFile2 text (fun () ->
                              do_modul denv fdir ptext blinkFile2 hFile2 modul))))))

            let rec do_namespace denv fdir ptext blinkFile hFile moduls tycons exncs = 
                do_moduls denv fdir blinkFile hFile ptext moduls;
                let _,tycons= List.partition IsUnseenEntity tycons
                do_tycons denv fdir blinkFile hFile ptext "Type Definitions" tycons;
                do_exncs denv fdir blinkFile hFile ptext exncs;
                //do_tycons denv fdir blinkFile hFile ptext "Deprecated/Unsafe Type Definitions" dtycons
              
            let rec do_possible_namespace fdir path knownNamespaces (mspec:ModuleOrNamespace) = 
                let mtype = mspec.ModuleOrNamespaceType
                let path = 
                    /// skip the first item in the path which is the assembly name 
                    match path with 
                    | None    -> Some ""
                    | Some "" -> Some (demangled_name_of_modul mspec)
                    | Some p  -> Some (p^"."^demangled_name_of_modul mspec)
                let ptext = 
                    match path with
                    | None   -> path0
                    | Some t -> pathWrt knownNamespaces t
                let allmods = mtype.ModuleAndNamespaceDefinitions
                              |> List.sortWith (orderOn demangled_name_of_modul pseudo_case_insensitive)
                let allmods = allmods |> List.filter (IsUnseenEntity >> not)
                let moduls,nsps = allmods |> List.partition (fun m -> m.IsModule) 
                let exncs = mtype.ExceptionDefinitions
                let tycons = mtype.TypeDefinitions
                let exncs = exncs |> List.filter (IsUnseenEntity >> not)
                let tycons = tycons |> List.filter (IsUnseenEntity >> not)

                // In FSharp.Core.dll filter out the attributes
                let tycons = 
                   if tcGlobals.compilingFslib  then 
                       tycons |> List.filter (fun tycon -> 
                           not (tycon.DisplayName = "PrintfFormat") && 
                           not (tycon.DisplayName = "Format") && 
                           not (tycon.DisplayName = "Unit") && 
                           not (tycon.DisplayName = "TypeFunc") && 
                           not (tycon.DisplayName = "FastFunc") && 
                           not (tycon.DisplayName.StartsWith("Tuple",System.StringComparison.Ordinal)) && 
                           not (tycon.DisplayName.StartsWith("Choice",System.StringComparison.Ordinal)) && 
                           not (tycon.DisplayName.Contains "[") && 
                           not (tycon.DisplayName.EndsWith("Flags",System.StringComparison.Ordinal)) &&
                           not (tycon.DisplayName.EndsWith("Attribute",System.StringComparison.Ordinal)))
                   else
                       tycons 

                // In FSharp.Core.dll filter out the attributes
                let moduls = 
                   if tcGlobals.compilingFslib  then 
                       moduls |> List.filter (fun modul -> 
                           not (modul.DisplayName = "OptimizedClosures"))
                   else
                       moduls 

                let knownNamespaces =
                    if mspec.IsNamespace && (nonNil moduls or nonNil tycons or nonNil exncs) then 
                        let fname = pathFilename ptext
                        let title = pathText ptext
                        newNamespaceEntry hNamespacesFile fdir ptext allmods tycons (getDoc mspec.XmlDoc);
                        newFile fdir fname title (fun blinkFile2 hFile2 ->
                          nestBlock hFile2 (fun () ->
                            newPathTrail hFile2 "Namespace" ptext;
                            newPartitionsWithSeeAlsoBacklink hFile2 blinkNamespacesFile  (fun () -> 
                              newPartition hFile2 "" (fun () ->
                                do_namespace denv fdir ptext blinkFile2 hFile2 moduls tycons exncs))));
                        title :: knownNamespaces
                    else
                        knownNamespaces
                 
                List.iter (do_possible_namespace fdir path knownNamespaces)  nsps; 
           
            do_possible_namespace assemblyName None [] generatedCcu.Contents))

(*----------------------------------------------------------------------------
!* cmd line - option state
 *--------------------------------------------------------------------------*)

let getModuleFileName() = 
    System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                           System.AppDomain.CurrentDomain.FriendlyName)  

let defaultFSharpBinariesDir = Filename.dirname (getModuleFileName())


let outpath outfile extn =
  String.concat "." (["out"; Filename.chop_extension (Path.GetFileName outfile); extn])
  


let TypeCheck (tcConfig,tcImports,tcGlobals,errorLogger:ErrorLogger,assemblyName,niceNameGen,tcEnv0,inputs) =
    try 
        if isNil inputs then error(Error("no implementation files specified",Range.rangeStartup));
        let ccuName = assemblyName
        let tcInitialState = TypecheckInitialState (rangeStartup,ccuName,tcConfig,tcGlobals,niceNameGen,tcEnv0)
        TypecheckClosedInputSet ((fun () -> errorLogger.ErrorCount = 0),tcConfig,tcImports,tcGlobals,None,tcInitialState,inputs)
    with e -> 
        errorRecovery e rangeStartup; 
        exiter.Exit 1



let GenerateInterfaceData(tcConfig:TcConfig) = 
    (* (tcConfig.target = Dll or tcConfig.target = Module) && *)
    not tcConfig.standalone && not tcConfig.noSignatureData 

let EncodeInterfaceData(tcConfig:TcConfig,tcGlobals,errorLogger:ErrorLogger,exportRemapping,generatedCcu,outfile) = 
    try 
      if GenerateInterfaceData(tcConfig) then 
        if verbose then dprintfn "Generating interface data attribute...";
        let resource = WriteSignatureData (tcConfig,tcGlobals,exportRemapping,generatedCcu,outfile)
        if verbose then dprintf "Generated interface data attribute!\n";
        [mk_SignatureDataVersionAttr tcGlobals (IL.parse_version Ilxconfig.version) ], [resource]
      else 
        [],[]
    with e -> 
        errorRecoveryNoRange e; 
        exiter.Exit 1


(*----------------------------------------------------------------------------
!* EncodeOptimizationData
 *--------------------------------------------------------------------------*)

let GenerateOptimizationData(tcConfig) = 
    (* (tcConfig.target =Dll or tcConfig.target = Module) && *)
    GenerateInterfaceData(tcConfig) 

let EncodeOptimizationData(tcGlobals,tcConfig,outfile,exportRemapping,data) = 
    if GenerateOptimizationData(tcConfig) then 
        let data = map2'2 (Opt.RemapLazyModulInfo tcGlobals exportRemapping) data
        if verbose then dprintn "Generating optimization data attribute...";
        if tcConfig.useOptimizationDataFile then 
            let ccu,modulInfo = data
            let bytes = Pickle.pickle_obj_with_dangling_ccus outfile tcGlobals ccu Opt.p_lazy_modul_info modulInfo
            let optDataFileName = (Filename.chop_extension outfile)^".optdata"
            System.IO.File.WriteAllBytes(optDataFileName,bytes);
        let data = 
            if tcConfig.onlyEssentialOptimizationData || tcConfig.useOptimizationDataFile 
            then map2'2 Opt.AbstractLazyModulInfoToEssentials data 
            else data
        [ WriteOptData tcGlobals outfile data ]
    else
        [ ]

//----------------------------------------------------------------------------
// .res file format, for encoding the assembly version attribute. 
//--------------------------------------------------------------------------

// Helpers for generating binary blobs
module BinaryGenerationUtilities = 
    // Little-endian encoding of int32 
    let b0 n =  byte (n &&& 0xFF)
    let b1 n =  byte ((n >>> 8) &&& 0xFF)
    let b2 n =  byte ((n >>> 16) &&& 0xFF)
    let b3 n =  byte ((n >>> 24) &&& 0xFF)

    let i16 (i:int32) = [| b0 i; b1 i |]
    let i32 (i:int32) = [| b0 i; b1 i; b2 i; b3 i |]

    // Emit the bytes and pad to a 32-bit alignment
    let Padded initialAlignment (v:byte[]) = 
        [| yield! v
           for i in 1..(4 - (initialAlignment + v.Length) % 4) % 4 do
               yield 0x0uy |]

// Generate nodes in a .res file format. These are then linked by Abstract IL using the 
// linkNativeResources function, which invokes the cvtres.exe utility
module ResFileFormat = 
    open BinaryGenerationUtilities
    
    let ResFileNode(dwTypeID,dwNameID,wMemFlags,wLangID,data:byte[]) =
        [| yield! i32 data.Length  // DWORD ResHdr.dwDataSize
           yield! i32 0x00000020  // dwHeaderSize
           yield! i32 ((dwTypeID <<< 16) ||| 0x0000FFFF)  // dwTypeID,sizeof(DWORD)
           yield! i32 ((dwNameID <<< 16) ||| 0x0000FFFF)   // dwNameID,sizeof(DWORD)
           yield! i32 0x00000000 // DWORD       dwDataVersion
           yield! i16 wMemFlags // WORD        wMemFlags
           yield! i16 wLangID   // WORD        wLangID
           yield! i32 0x00000000 // DWORD       dwVersion
           yield! i32 0x00000000 // DWORD       dwCharacteristics
           yield! Padded 0 data |]

    let ResFileHeader() = ResFileNode(0x0,0x0,0x0,0x0,[| |]) 

// Generate the VS_VERSION_INFO structure held in a Win32 Version Resource in a PE file
//
// Web reference: http://www.piclist.com/tecHREF/os/win/api/win32/struc/src/str24_5.htm
module VersionResourceFormat = 
    open BinaryGenerationUtilities

    let VersionInfoNode(data:byte[]) =
        [| yield! i16 (data.Length + 2) // wLength : int16; // Specifies the length, in bytes, of the VS_VERSION_INFO structure. This length does not include any padding that aligns any subsequent version resource data on a 32-bit boundary. 
           yield! data |]

    let VersionInfoElement(wType, szKey, valueOpt: byte[] option, children:byte[][], isString) =
        // for String structs, wValueLength represents the word count, not the byte count
        let wValueLength = (match valueOpt with None -> 0 | Some value -> (if isString then value.Length / 2 else value.Length))
        VersionInfoNode
            [| yield! i16 wValueLength // wValueLength: int16. Specifies the length, in words, of the Value member. This value is zero if there is no Value member associated with the current version structure. 
               yield! i16 wType        // wType : int16; Specifies the type of data in the version resource. This member is 1 if the version resource contains text data and 0 if the version resource contains binary data. 
               yield! Padded 2 szKey 
               match valueOpt with 
               | None -> yield! []
               | Some value -> yield! Padded 0 value 
               for child in children do 
                   yield! child  |]

    let Version((v1,v2,v3,v4):ILVersionInfo) = 
        [| yield! i32 (int32 v1 <<< 16 ||| int32 v2) // DWORD dwFileVersionMS; // Specifies the most significant 32 bits of the file's binary version number. This member is used with dwFileVersionLS to form a 64-bit value used for numeric comparisons. 
           yield! i32 (int32 v3 <<< 16 ||| int32 v4) // DWORD dwFileVersionLS; // Specifies the least significant 32 bits of the file's binary version number. This member is used with dwFileVersionMS to form a 64-bit value used for numeric comparisons. 
        |]

    let String(string,value) = 
        let wType = 0x1 // Specifies the type of data in the version resource. This member is 1 if the version resource contains text data and 0 if the version resource contains binary data. 
        let szKey = Bytes.string_as_unicode_bytes_null_terminated string
        VersionInfoElement(wType, szKey, Some(Bytes.string_as_unicode_bytes_null_terminated value),[| |],true)

    let StringTable(language,strings) = 
        let wType = 0x1 // Specifies the type of data in the version resource. This member is 1 if the version resource contains text data and 0 if the version resource contains binary data. 
        let szKey = Bytes.string_as_unicode_bytes_null_terminated language
             // Specifies an 8-digit hexadecimal number stored as a Unicode string. The four most significant digits represent the language identifier. The four least significant digits represent the code page for which the data is formatted. 
             // Each Microsoft Standard Language identifier contains two parts: the low-order 10 bits specify the major language, and the high-order 6 bits specify the sublanguage. For a table of valid identifiers see Language Identifiers. 
                       
        let children =  
            [| for string in strings do
                   yield String(string) |] 
        VersionInfoElement(wType, szKey, None,children,false)

    let StringFileInfo(stringTables: #seq<string * #seq<string * string> >) = 
        let wType = 0x1 // Specifies the type of data in the version resource. This member is 1 if the version resource contains text data and 0 if the version resource contains binary data. 
        let szKey = Bytes.string_as_unicode_bytes_null_terminated "StringFileInfo" // Contains the Unicode string StringFileInfo
        // Contains an array of one or more StringTable structures. Each StringTable structures szKey member indicates the appropriate language and code page for displaying the text in that StringTable structure. 
        let children =  
            [| for stringTable in stringTables do
                   yield StringTable(stringTable) |] 
        VersionInfoElement(wType, szKey, None,children,false)
        
    let VarFileInfo(vars: #seq<int32 * int32>) = 
        let wType = 0x1 // Specifies the type of data in the version resource. This member is 1 if the version resource contains text data and 0 if the version resource contains binary data. 
        let szKey = Bytes.string_as_unicode_bytes_null_terminated "VarFileInfo" // Contains the Unicode string StringFileInfo
        // Contains an array of one or more StringTable structures. Each StringTable structures szKey member indicates the appropriate language and code page for displaying the text in that StringTable structure. 
        let children =  
            [| for (lang,codePage) in vars do
                   let szKey = Bytes.string_as_unicode_bytes_null_terminated "Translation"
                   yield VersionInfoElement(0x0,szKey, Some([| yield! i16 lang
                                                               yield! i16 codePage |]), [| |],false) |] 
        VersionInfoElement(wType, szKey, None,children,false)
        
    let VS_FIXEDFILEINFO(fileVersion:ILVersionInfo,
                         productVersion:ILVersionInfo,
                         dwFileFlagsMask,
                         dwFileFlags,dwFileOS,
                         dwFileType,dwFileSubtype,
                         lwFileDate:int64) = 
        let dwStrucVersion = 0x00010000
        [| yield! i32  0xFEEF04BD // DWORD dwSignature; // Contains the value 0xFEEFO4BD. This is used with the szKey member of the VS_VERSION_INFO structure when searching a file for the VS_FIXEDFILEINFO structure. 
           yield! i32 dwStrucVersion // DWORD dwStrucVersion; // Specifies the binary version number of this structure. The high-order word of this member contains the major version number, and the low-order word contains the minor version number. 
           yield! Version fileVersion // DWORD dwFileVersionMS,dwFileVersionLS; // Specifies the most/least significant 32 bits of the file's binary version number. This member is used with dwFileVersionLS to form a 64-bit value used for numeric comparisons. 
           yield! Version productVersion // DWORD dwProductVersionMS,dwProductVersionLS; // Specifies the most/least significant 32 bits of the file's binary version number. This member is used with dwFileVersionLS to form a 64-bit value used for numeric comparisons. 
           yield! i32 dwFileFlagsMask // DWORD dwFileFlagsMask; // Contains a bitmask that specifies the valid bits in dwFileFlags. A bit is valid only if it was defined when the file was created. 
           yield! i32 dwFileFlags // DWORD dwFileFlags; // Contains a bitmask that specifies the Boolean attributes of the file. This member can include one or more of the following values: 
                  //          VS_FF_DEBUG 0x1L             The file contains debugging information or is compiled with debugging features enabled. 
                  //          VS_FF_INFOINFERRED            The file's version structure was created dynamically; therefore, some of the members in this structure may be empty or incorrect. This flag should never be set in a file's VS_VERSION_INFO data. 
                  //          VS_FF_PATCHED            The file has been modified and is not identical to the original shipping file of the same version number. 
                  //          VS_FF_PRERELEASE            The file is a development version, not a commercially released product. 
                  //          VS_FF_PRIVATEBUILD            The file was not built using standard release procedures. If this flag is set, the StringFileInfo structure should contain a PrivateBuild entry. 
                  //          VS_FF_SPECIALBUILD            The file was built by the original company using standard release procedures but is a variation of the normal file of the same version number. If this flag is set, the StringFileInfo structure should contain a SpecialBuild entry. 
           yield! i32 dwFileOS //Specifies the operating system for which this file was designed. This member can be one of the following values: Flag 
                  //VOS_DOS 0x0001L  The file was designed for MS-DOS. 
                  //VOS_NT  0x0004L  The file was designed for Windows NT. 
                  //VOS__WINDOWS16  The file was designed for 16-bit Windows. 
                  //VOS__WINDOWS32  The file was designed for the Win32 API. 
                  //VOS_OS216 0x00020000L  The file was designed for 16-bit OS/2. 
                  //VOS_OS232  0x00030000L  The file was designed for 32-bit OS/2. 
                  //VOS__PM16  The file was designed for 16-bit Presentation Manager. 
                  //VOS__PM32  The file was designed for 32-bit Presentation Manager. 
                  //VOS_UNKNOWN  The operating system for which the file was designed is unknown to Windows. 
           yield! i32 dwFileType // Specifies the general type of file. This member can be one of the following values: 
     
                //VFT_UNKNOWN The file type is unknown to Windows. 
                //VFT_APP  The file contains an application. 
                //VFT_DLL  The file contains a dynamic-link library (DLL). 
                //VFT_DRV  The file contains a device driver. If dwFileType is VFT_DRV, dwFileSubtype contains a more specific description of the driver. 
                //VFT_FONT  The file contains a font. If dwFileType is VFT_FONT, dwFileSubtype contains a more specific description of the font file. 
                //VFT_VXD  The file contains a virtual device. 
                //VFT_STATIC_LIB  The file contains a static-link library. 

           yield! i32 dwFileSubtype //     Specifies the function of the file. The possible values depend on the value of dwFileType. For all values of dwFileType not described in the following list, dwFileSubtype is zero. If dwFileType is VFT_DRV, dwFileSubtype can be one of the following values: 
                      //VFT2_UNKNOWN  The driver type is unknown by Windows. 
                      //VFT2_DRV_COMM  The file contains a communications driver. 
                      //VFT2_DRV_PRINTER  The file contains a printer driver. 
                      //VFT2_DRV_KEYBOARD  The file contains a keyboard driver. 
                      //VFT2_DRV_LANGUAGE  The file contains a language driver. 
                      //VFT2_DRV_DISPLAY  The file contains a display driver. 
                      //VFT2_DRV_MOUSE  The file contains a mouse driver. 
                      //VFT2_DRV_NETWORK  The file contains a network driver. 
                      //VFT2_DRV_SYSTEM  The file contains a system driver. 
                      //VFT2_DRV_INSTALLABLE  The file contains an installable driver. 
                      //VFT2_DRV_SOUND  The file contains a sound driver. 
                      //
                      //If dwFileType is VFT_FONT, dwFileSubtype can be one of the following values: 
                      // 
                      //VFT2_UNKNOWN  The font type is unknown by Windows. 
                      //VFT2_FONT_RASTER  The file contains a raster font. 
                      //VFT2_FONT_VECTOR  The file contains a vector font. 
                      //VFT2_FONT_TRUETYPE  The file contains a TrueType font. 
                      //
                      //If dwFileType is VFT_VXD, dwFileSubtype contains the virtual device identifier included in the virtual device control block. 
           yield! i32 (int32 (lwFileDate >>> 32)) // Specifies the most significant 32 bits of the file's 64-bit binary creation date and time stamp. 
           yield! i32 (int32 lwFileDate) //Specifies the least significant 32 bits of the file's 64-bit binary creation date and time stamp. 
         |] 


    let VS_VERSION_INFO(fixedFileInfo,stringFileInfo,varFileInfo)  =
        let wType = 0x0 
        let szKey = Bytes.string_as_unicode_bytes_null_terminated "VS_VERSION_INFO" // Contains the Unicode string VS_VERSION_INFO
        let value = VS_FIXEDFILEINFO (fixedFileInfo)
        let children =  
            [| yield StringFileInfo(stringFileInfo) 
               yield VarFileInfo(varFileInfo) 
            |] 
        VersionInfoElement(wType, szKey, Some(value),children,false)
       
    let VS_VERSION_INFO_RESOURCE(data) = 
        let dwTypeID = 0x0010
        let dwNameID = 0x0001
        let wMemFlags = 0x0030 // REVIEW: HARDWIRED TO ENGLISH
        let wLangID = 0x0409 // REVIEW: HARDWIRED TO ENGLISH
        ResFileFormat.ResFileNode(dwTypeID, dwNameID,wMemFlags,wLangID,VS_VERSION_INFO(data))
        
module ManifestResourceFormat =
    
    let VS_MANIFEST_RESOURCE(data, isLibrary) =
        let dwTypeID = 0x0018
        let dwNameID = if isLibrary then 0x2 else 0x1
        let wMemFlags = 0x0
        let wLangID = 0x0 
        ResFileFormat.ResFileNode(dwTypeID, dwNameID, wMemFlags, wLangID, data)

/// Helpers for finding attributes
module AttributeHelpers = 

    /// Try to find an attribute that takes a string argument
    let TryFindStringAttribute tcGlobals attrib attribs =
        match TryFindAttrib tcGlobals (mk_mscorlib_attrib tcGlobals attrib) attribs with
        | Some (Attrib(_,_,[ AttribStringArg(s) ],_,_))  -> Some (s)
        | _ -> None
        
    let TryFindIntAttribute tcGlobals attrib attribs =
        match TryFindAttrib tcGlobals (mk_mscorlib_attrib tcGlobals attrib) attribs with
        | Some (Attrib(_,_,[ AttribInt32Arg(i) ],_,_)) -> Some (i)
        | _ -> None
        
    let TryFindBoolAttribute tcGlobals attrib attribs =
        match TryFindAttrib tcGlobals (mk_mscorlib_attrib tcGlobals attrib) attribs with
        | Some (Attrib(_,_,[ AttribBoolArg(p) ],_,_)) -> Some (p)
        | _ -> None

    // Try to find an AssemblyVersion attribute 
    let TryFindVersionAttribute tcGlobals attrib attribs =
        match TryFindStringAttribute tcGlobals attrib attribs with
        | Some versionString ->
             try Some(IL.parse_version versionString)
             with e -> 
                 warning(Error(sprintf "The invalid version '%s' was specified in assembly attribute and has been ignored" versionString,Range.rangeStartup));
                 None
        | _ -> None




let AssemblyCultureErrorE = DeclareResourceString("AssemblyCultureError","")

module MainModuleBuilder = 
    let CreateMainModule  
            (tcConfig:TcConfig,tcGlobals,
             pdbfile,assemblyName,outfile,topAttrs,
             (iattrs,intfDataResources),optDataResources,
             codegenResults,assemVerFromAttrib) =


        if !progress then dprintf "Creating main module...\n";
        let ilTypeDefs = 
            let topTypeDef = mk_toplevel_tdef tcGlobals.ilg (mk_mdefs [], mk_fdefs [])
            mk_tdefs codegenResults.ilTypeDefs

        let mainModule = 
            let hashAlg = AttributeHelpers.TryFindIntAttribute tcGlobals "System.Reflection.AssemblyAlgorithmIdAttribute" topAttrs.assemblyAttrs
            let locale = AttributeHelpers.TryFindStringAttribute tcGlobals "System.Reflection.AssemblyCultureAttribute" topAttrs.assemblyAttrs
            let flags =  match AttributeHelpers.TryFindIntAttribute tcGlobals "System.Reflection.AssemblyFlagsAttribute" topAttrs.assemblyAttrs with | Some(f) -> f | _ -> 0x0
            
            // You're only allowed to set a locale if the assembly is a library
            if locale <> None && tcConfig.target <> Dll then
              error(Error(AssemblyCultureErrorE.Format,rangeCmdArgs))
            
            mk_simple_mainmod assemblyName (fsharpModuleName tcConfig.target assemblyName) (tcConfig.target = Dll or tcConfig.target = Module) ilTypeDefs hashAlg locale flags

        let disableJitOptimizations = not (tcConfig.optSettings.jitOpt())
        let manifestAttrs = 
             mk_custom_attrs 
                 [ if not tcConfig.internConstantStrings then 
                       yield mk_custom_attribute tcGlobals.ilg
                                 (mk_tref (tcGlobals.ilg.mscorlib_scoref, "System.Runtime.CompilerServices.CompilationRelaxationsAttribute"),
                                  [tcGlobals.ilg.typ_Int32],[CustomElem_int32( 8)], []) 
                   yield! iattrs
                   yield! codegenResults.ilAssemAttrs
                   if Option.isSome pdbfile then 
                       yield (mk_DebuggableAttribute_v2 tcGlobals.ilg (tcConfig.jitTracking, tcConfig.ignoreSymbolStoreSequencePoints, disableJitOptimizations, false (* enableEnC *) )) ]
                       
        let tcVersion = tcConfig.version.GetVersionInfo(tcConfig.implicitIncludeDir)
        let manifest = 
             if tcConfig.target = Module then None else
             let man = mainModule.ManifestOfAssembly
             let ver = 
                 match assemVerFromAttrib with 
                 | None -> tcVersion
                 | Some v -> v
             Some { man with manifestVersion= Some(ver);
                             manifestCustomAttrs = manifestAttrs;
                             manifestDisableJitOptimizations=disableJitOptimizations;
                             manifestJitTracking= tcConfig.jitTracking } 
                  
        let quotDataResources = 
                codegenResults.quotationResourceBytes |> List.map (fun bytes -> 
                    { resourceName=Sreflect.pickledDefinitionsResourceNameBase^string(new_uniq());
                      resourceWhere = Resource_local (fun () -> bytes);
                      resourceAccess= Resource_public;
                      resourceCustomAttrs = mk_custom_attrs [] }) 

        let resources = 
          mk_resources 
            [ for file in tcConfig.embedResources do
                 let name,bytes,pub = 
                     let lower = String.lowercase file
                     if List.exists (Filename.check_suffix lower) [".resx"]  then
                         let file = tcConfig.ResolveSourceFile(rangeStartup,file)
                         let outfile = (file |> Filename.chop_extension) ^ ".resources"
                         
                         let readResX(f:string) = 
                             use rsxr = new System.Resources.ResXResourceReader(f)
                             rsxr 
                             |> Seq.cast 
                             |> Seq.to_list
                             |> List.map (fun (d:System.Collections.DictionaryEntry) -> (d.Key :?> string), d.Value)
                         let writeResources((r:(string * obj) list),(f:string)) = 
                             use writer = new System.Resources.ResourceWriter(f)
                             r |> List.iter (fun (k,v) -> writer.AddResource(k,v))
                         writeResources(readResX(file),outfile);
                         let file,name,pub = TcConfigBuilder.SplitCommandLineResourceInfo outfile
                         let file = tcConfig.ResolveSourceFile(rangeStartup,file)
                         let bytes = System.IO.File.ReadAllBytes file
                         System.IO.File.Delete outfile;
                         name,bytes,pub
                     else

                         let file,name,pub = TcConfigBuilder.SplitCommandLineResourceInfo file
                         let file = tcConfig.ResolveSourceFile(rangeStartup,file)
                         let bytes = System.IO.File.ReadAllBytes file
                         name,bytes,pub
                 yield { resourceName=name; 
                         resourceWhere=Resource_local (fun () -> bytes); 
                         resourceAccess=pub; 
                         resourceCustomAttrs=mk_custom_attrs [] }
               
              yield! quotDataResources
              yield! intfDataResources
              yield! optDataResources
              for ri in tcConfig.linkResources do 
                 let file,name,pub = TcConfigBuilder.SplitCommandLineResourceInfo ri
                 yield { resourceName=name; 
                         resourceWhere=Resource_file(ILModuleRef.Create(name=file, hasMetadata=false, hash=Some (sha1_hash_bytes (System.IO.File.ReadAllBytes file))), 0);
                         resourceAccess=pub; 
                         resourceCustomAttrs=mk_custom_attrs [] } ]

        //NOTE: the culture string can be turned into a number using this:
        //    sprintf "%04x" (System.Globalization.CultureInfo.GetCultureInfo("en").KeyboardLayoutId )
        let assemblyVersionResources =
            let assemblyVersion = 
                match tcConfig.version with
                | VersionNone ->assemVerFromAttrib
                | v -> Some(tcVersion)
            match assemblyVersion with 
            | None -> []
            | Some(assemblyVersion) ->
                let FindAttribute key attrib = 
                    match AttributeHelpers.TryFindStringAttribute tcGlobals attrib topAttrs.assemblyAttrs with
                    | Some text  -> [(key,text)]
                    | _ -> []

                let fileVersion = 
                    match AttributeHelpers.TryFindVersionAttribute tcGlobals "System.Reflection.AssemblyFileVersionAttribute" topAttrs.assemblyAttrs with
                    | Some v -> v
                    | None -> assemblyVersion

                let productVersion = 
                    match AttributeHelpers.TryFindVersionAttribute tcGlobals "System.Reflection.AssemblyInformationalVersionAttribute" topAttrs.assemblyAttrs with
                    | Some v -> v
                    | None -> assemblyVersion

                let stringFileInfo = 
                     // 040904b0:
                     // Specifies an 8-digit hexadecimal number stored as a Unicode string. The four most significant digits represent the language identifier. The four least significant digits represent the code page for which the data is formatted. 
                     // Each Microsoft Standard Language identifier contains two parts: the low-order 10 bits specify the major language, and the high-order 6 bits specify the sublanguage. For a table of valid identifiers see Language Identifiers.                                           //
                     // REVIEW: HARDWIRED TO ENGLISH
                      [ ("040904b0", [ yield ("Assembly Version", (let v1,v2,v3,v4 = assemblyVersion in sprintf "%d.%d.%d.%d" v1 v2 v3 v4))
                                       yield ("FileVersion", (let v1,v2,v3,v4 = fileVersion in sprintf "%d.%d.%d.%d" v1 v2 v3 v4))
                                       yield ("ProductVersion", (let v1,v2,v3,v4 = productVersion in sprintf "%d.%d.%d.%d" v1 v2 v3 v4))
                                       yield! FindAttribute "Comments" "System.Reflection.AssemblyDescriptionAttribute" 
                                       yield! FindAttribute "FileDescription" "System.Reflection.AssemblyTitleAttribute" 
                                       yield! FindAttribute "ProductName" "System.Reflection.AssemblyProductAttribute" 
                                       yield! FindAttribute "CompanyName" "System.Reflection.AssemblyCompanyAttribute" 
                                       yield! FindAttribute "LegalCopyright" "System.Reflection.AssemblyCopyrightAttribute" 
                                       yield! FindAttribute "LegalTrademarks" "System.Reflection.AssemblyTrademarkAttribute" ]) ]

            
            // These entries listed in the MSDN documentation as "standard" string entries are not yet settable
            
            // InternalName: The Value member identifies the file's internal name, if one exists. For example, this string could contain the module name for Windows dynamic-link libraries (DLLs), a virtual device name for Windows virtual devices, or a device name for MS-DOS device drivers. 
            // OriginalFilename: The Value member identifies the original name of the file, not including a path. This enables an application to determine whether a file has been renamed by a user. This name may not be MS-DOS 8.3-format if the file is specific to a non-FAT file system. 
            // PrivateBuild: The Value member describes by whom, where, and why this private version of the file was built. This string should only be present if the VS_FF_PRIVATEBUILD flag is set in the dwFileFlags member of the VS_FIXEDFILEINFO structure. For example, Value could be 'Built by OSCAR on \OSCAR2'. 
            // SpecialBuild: The Value member describes how this version of the file differs from the normal version. This entry should only be present if the VS_FF_SPECIALBUILD flag is set in the dwFileFlags member of the VS_FIXEDFILEINFO structure. For example, Value could be 'Private build for Olivetti solving mouse problems on M250 and M250E computers'. 



                // "If you use the Var structure to list the languages your application 
                // or DLL supports instead of using multiple version resources, 
                // use the Value member to contain an array of DWORD values indicating the 
                // language and code page combinations supported by this file. The 
                // low-order word of each DWORD must contain a Microsoft language identifier, 
                // and the high-order word must contain the IBM® code page number. 
                // Either high-order or low-order word can be zero, indicating that 
                // the file is language or code page independent. If the Var structure is 
                // omitted, the file will be interpreted as both language and code page independent. "
                let varFileInfo = [ (0x0409, 0x04b0)  ]

                let fixedFileInfo = 
                    let dwFileFlagsMask = 0x3f // REVIEW: HARDWIRED
                    let dwFileFlags = 0x00 // REVIEW: HARDWIRED
                    let dwFileOS = 0x04 // REVIEW: HARDWIRED
                    let dwFileType = 0x01 // REVIEW: HARDWIRED
                    let dwFileSubtype = 0x00 // REVIEW: HARDWIRED
                    let lwFileDate = 0x00L // REVIEW: HARDWIRED
                    (fileVersion,productVersion,dwFileFlagsMask,dwFileFlags,dwFileOS,dwFileType,dwFileSubtype,lwFileDate)

                let vsVersionInfoResource = 
                    VersionResourceFormat.VS_VERSION_INFO_RESOURCE(fixedFileInfo,stringFileInfo,varFileInfo)
                
                
                let resource = 
                    [| yield! ResFileFormat.ResFileHeader()
                       yield! vsVersionInfoResource |]
#if DUMP_ASSEMBLY_RESOURCE
                for i in 0..(resource.Length+15)/16 - 1 do
                    for j in 0..15 do
                        if j % 2 = 0 then printf " " 
                        printf "%02x" resource.[min (i*16+j) (resource.Length - 1)]
                    printf " " 
                    for j in 0..15 do
                        printf "%c" (let c = char resource.[min (i*16+j) (resource.Length - 1)] in if c > ' ' && c < '~' then c else '.')
                    printfn "" 
#endif
                [ resource ]
          
        // a user cannot specify both win32res and win32manifest        
        if not(tcConfig.win32manifest = "") && not(tcConfig.win32res = "") then
            error(Error(sprintf "Conflicting options specified: win32manifest and win32res",rangeCmdArgs));
                      
        let win32Manifest =
           if not(tcConfig.win32manifest = "") then
               tcConfig.win32manifest
           elif not(tcConfig.includewin32manifest) || not(tcConfig.win32res = "") || runningOnMono then // don't embed a manifest if a native resource is being included
               ""
           else
               match Build.highestInstalledNetFrameworkVersionMajorMinor() with
               | _,"v4.0" -> System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() ^ @"default.win32manifest"
               | _,"v3.5" -> System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() ^ @"..\v3.5\default.win32manifest"
               | _,_ -> "" // only have default manifests for 3.5 and 4.0               
        
        let nativeResources = 
            [ for av in assemblyVersionResources do
                  yield Lazy.CreateFromValue av
              if not(tcConfig.win32res = "") then
                  yield Lazy.CreateFromValue (System.IO.File.ReadAllBytes tcConfig.win32res) 
              if tcConfig.includewin32manifest && not(win32Manifest = "") && not(runningOnMono) then
                  yield  Lazy.CreateFromValue [|   yield! ResFileFormat.ResFileHeader() 
                                                   yield! (ManifestResourceFormat.VS_MANIFEST_RESOURCE((System.IO.File.ReadAllBytes win32Manifest), tcConfig.target = Dll))|]]


        // Add attributes, version number, resources etc. 
        {mainModule with 
              modulName = (if tcConfig.target = Module then Path.GetFileName outfile else mainModule.modulName);
              modulSubSystem = (if tcConfig.target = WinExe then 2 else 3) ;
              modulResources= resources;
              modulImageBase = (match tcConfig.baseAddress with None -> 0x00400000l | Some b -> b);
              modulDLL=(tcConfig.target = Dll or tcConfig.target=Module);
              modulPlatform = tcConfig.platform ;
              modul32bit=(match tcConfig.platform with Some X86 -> true | _ -> false);
              modul64bit=(match tcConfig.platform with Some AMD64 | Some IA64 -> true | _ -> false);          
              modulCustomAttrs= mk_custom_attrs ((if tcConfig.target = Module then iattrs else []) @ codegenResults.ilNetModuleAttrs);
              modulNativeResources=nativeResources;
              modulManifest = manifest }



    let EraseILX (ilGlobals,ilxMainModule) =
        if !progress then dprintn "Generating IL code...";
        Ilxerase.ConvModule ilGlobals ilxMainModule



/// OPTIONAL STATIC LINKING OF ALL DLLs THAT DEPEND ON THE F# LIBRARY
module StaticLinker = 
    type node = 
        { name: string;
          data: ILModuleDef; 
          ccu: CcuThunk option ;
          refs: ILReferences;
          mutable edges: node list; 
          mutable visited: bool }


    let StaticLinkModules tcConfig ilGlobals ilxMainModule (dependentModules: (ccu option * ILModuleDef) list) = 
      if isNil dependentModules then 
        ilxMainModule,(fun x -> x) 
      else

        match dependentModules |> List.tryPick (function (Some ccu,_) when ccu.UsesQuotations -> Some ccu | _ -> None)  with
         | Some ccu -> error(Error(sprintf "The code in assembly '%s' makes uses of quotation literals. Static linking may not include components that make use of quotation literals" ccu.AssemblyName,rangeStartup));
         | None -> ()
            
        if dependentModules |> List.exists (fun (_,x) -> not x.IsDLL)  then 
            error(Error("static linking may not include a .EXE",rangeStartup))
        if dependentModules |> List.exists (fun (_,x) -> not x.IsILOnly)  then 
            error(Error("static linking may not include a mixed managed/unmanaged DLL",rangeStartup))
        let dependentModules  = dependentModules |> List.map snd
        let assems = 
            dependentModules 
            |> List.filter module_is_mainmod
            |> List.map GetNameOfILModule
            |> Set.of_list

        (* Rewrite scope references to be local references *)
        let rewriteExternalRefsToLocalRefs x = 
          if assems.Contains(GetNameOfScopeRef x) then ScopeRef_local else x
        let saved_resources = 
            let all_resources = 
                dependentModules 
                |> List.map (fun m -> dest_resources m.modulResources)
                |> List.concat
            // Save only the interface/optimization attributes of generated data 
            let intfDataResources,others = 
                let intfDataResources,others = List.partition IsSignatureDataResource all_resources
                let intfDataResources = if GenerateInterfaceData(tcConfig)  then intfDataResources else []
                intfDataResources,others
            let optDataResources,others = 
                let optDataResources,others = List.partition IsOptDataResource others
                let optDataResources = if GenerateOptimizationData(tcConfig)  then optDataResources else []
                optDataResources,others
            let rresources,others = 
                let rresources,others = List.partition IsReflectedDefinitionsResource others
                let rresources = rresources |> List.mapi (fun i r -> {r with resourceName = Sreflect.pickledDefinitionsResourceNameBase^string (i+1)})
                rresources,others
            if verbose then dprintf "#intfDataResources = %d, #optDataResources = %d, #rresources = %d\n" intfDataResources.Length optDataResources.Length rresources.Length;
            intfDataResources@optDataResources@rresources@others
        let moduls = ilxMainModule :: dependentModules
        let topTypeDefs,normalTypeDefs = 
            moduls 
            |> List.map (fun m -> m.modulTypeDefs |> dest_tdefs |> List.partition (fun td -> is_toplevel_tname td.tdName)) 
            |> List.unzip
        let topTypeDef = 
          let topTypeDefs = List.concat topTypeDefs
          mk_toplevel_tdef ilGlobals
             (mk_mdefs ((topTypeDefs |> List.collect (fun td -> dest_mdefs td.Methods))),
              mk_fdefs ((topTypeDefs |> List.collect (fun td -> dest_fdefs td.Fields))))
        let ilxMainModule = 
         { ilxMainModule with 
             modulManifest = (let m = ilxMainModule.ManifestOfAssembly in Some {m with manifestCustomAttrs = mk_custom_attrs (dest_custom_attrs m.manifestCustomAttrs)});
             modulCustomAttrs = mk_custom_attrs [ for m in moduls do yield! dest_custom_attrs m.modulCustomAttrs ];
             modulTypeDefs = mk_tdefs (topTypeDef :: List.concat normalTypeDefs);
             modulResources = mk_resources (saved_resources @ dest_resources ilxMainModule.modulResources);
             modulNativeResources = 
                // NOTE: version resources from statically linked DLLs are dropped in the binary reader/writer
                [ //yield! ilxMainModule.modulNativeResources 
                  for m in moduls do 
                      yield! m.modulNativeResources ] }
        ilxMainModule, rewriteExternalRefsToLocalRefs


    #if DEBUG
    let PrintModule outfile x = 
        use os = File.CreateText(outfile) :> TextWriter
        Ilprint.output_module os x  
    #endif

    // Compute a static linker. This only captures tcImports (a large data structure) if
    // static linking is enabled. Normally this is not the case, which lets us collect tcImports
    // prior to this point.
    let StaticLink (tcConfig:TcConfig,tcImports:TcImports,ilGlobals) = 
      if not tcConfig.standalone && tcConfig.extraStaticLinkRoots = [] 
      then (fun (ilxMainModule,outfile) -> ilxMainModule)
      else (fun (ilxMainModule,outfile)  ->
            ReportTime tcConfig "Find assembly references";
            (* Recursively find all referenced modules and add them to a module graph *)    
            let depModuleTable = Hashtbl.create 0
            let GetNode n = Hashtbl.find depModuleTable n
            if !progress then dprintn "Performing static link...";
            begin 
              let remaining = ref (refs_of_module ilxMainModule).refsAssembly
              while !remaining <> [] do
                let aref = List.hd !remaining
                remaining := List.tl !remaining;
                match aref.Name with 
                | ("mscorlib" |  "System" | "System.Core" | "System.Xml" | "Microsoft.Build.Framework" | "Microsoft.Build.Utilities") -> 
                    Hashtbl.add depModuleTable aref.Name { refs = IL.empty_refs ;
                                                            name=aref.Name;
                                                            ccu=None;
                                                            data=ilxMainModule; // any old module
                                                            edges = []; 
                                                            visited = true };
                | _ -> 
                    if not (Hashtbl.mem depModuleTable aref.Name) then 
                        if verbose then dprintn ("Finding "^aref.Name);
                        let ccu = 
                            match tcImports.FindCcuFromAssemblyRef(Range.rangeStartup,aref) with 
                            | ResolvedCcu ccu -> Some ccu
                            | UnresolvedCcu(ccuName) -> None
                        
                        let dllInfo = tcImports.FindDllInfo(Range.rangeStartup,aref.Name)
                        let modul = dllInfo.RawMetadata
                        if !progress then dprintn ("Finding references for "^aref.Name);
                        let refs = 
                             if aref.Name = GetFSharpCoreLibraryName() then 
                                 IL.empty_refs 
                             elif not modul.IsILOnly then 
                                 warning(Error(sprintf "ignoring mixed managed/unmanaged assembly %s during static linking\n" modul.modulName,rangeStartup))
                                 IL.empty_refs 
                             else
                                 try 
                                    { refsAssembly = dllInfo.ILAssemblyRefs; 
                                      refsModul = [] }
                                 with e -> 
                                     warning(Error(sprintf "** warning: could not determine dependencies of assembly %s\n** reason: %s\n" modul.modulName (e.ToString()),rangeStartup)); 
                                     IL.empty_refs 
                        Hashtbl.add depModuleTable aref.Name { refs=refs;
                                                               name=aref.Name;
                                                               ccu=ccu;
                                                               data=modul; 
                                                               edges = []; 
                                                               visited = false };
                        remaining := refs.refsAssembly @ !remaining;
              done;
            end;

            ReportTime tcConfig "Find dependencies";
            (* Add edges from modules to the modules that depend on them *)
            begin 
              Hashtbl.iter
                (fun _ n -> n.refs.refsAssembly |> List.iter(fun aref -> let n2 = GetNode aref.Name in n2.edges <- n :: n2.edges)  ) 
                depModuleTable;
            end;
            
            // Find everything that depends on FSharp.Core
            let depModules = 
                let roots = 
                    [ if tcConfig.standalone && Hashtbl.mem depModuleTable (GetFSharpCoreLibraryName()) then 
                         yield GetNode(GetFSharpCoreLibraryName())
                      for n in tcConfig.extraStaticLinkRoots  do
                          yield try GetNode n
                                with :? KeyNotFoundException -> 
                                     error(Error("Assembly "^n^" not found in dependency set of target application",rangeStartup)); ]
                          
                let remaining = ref roots
                [ while nonNil !remaining do
                    let n = List.hd !remaining
                    remaining := List.tl !remaining;
                    if not (n.visited) then 
                        if verbose then dprintn ("Module "^n.name^" depends on "^GetFSharpCoreLibraryName());
                        n.visited <- true;
                        remaining := n.edges @ !remaining
                        yield (n.ccu, n.data); ]

            ReportTime tcConfig "Static link";
            (* Glue all this stuff into ilxMainModule *)
            let ilxMainModule,rewriteExternalRefsToLocalRefs = StaticLinkModules tcConfig ilGlobals ilxMainModule depModules
               
            let ilxMainModule =
                let rewriteAssemblyRefsToMatchLibraries = NormalizeAssemblyRefs tcImports
                Ilmorph.module_tref2tref_memoized (Ilmorph.tref_scoref2scoref (rewriteExternalRefsToLocalRefs >> rewriteAssemblyRefsToMatchLibraries)) ilxMainModule

             (* Print it out if requested *)
        #if DEBUG
            if tcConfig.writeGeneratedILFiles then (let _ = PrintModule (outpath outfile "ilx.main") ilxMainModule in ());
        #endif
            ilxMainModule)
  
(*----------------------------------------------------------------------------
!* EMIT IL
 *--------------------------------------------------------------------------*)

type SigningInfo = SigningInfo of (* delaysign:*) bool * (*signer:*)  string option * (*container:*) string option

module FileWriter = 
    let EmitIL (tcConfig:TcConfig,ilGlobals,errorLogger:ErrorLogger,outfile,pdbfile,ilxMainModule,signingInfo:SigningInfo) =
        let (SigningInfo(delaysign,signer,container)) = signingInfo
        try
    #if DEBUG
            if tcConfig.writeGeneratedILFiles then dprintn "Printing module...";
            if tcConfig.writeGeneratedILFiles  then StaticLinker.PrintModule (outpath outfile "il.txt") ilxMainModule; 
    #endif
            if !progress then dprintn "Writing assembly...";
            if tcConfig.showTimes then Ilwrite.showTimes := true;
            try 
                Ilwrite.WriteILBinary 
                  outfile
                  {    mscorlib=ilGlobals.mscorlib_scoref;
                       pdbfile=pdbfile;

                       signer = 
                         begin
                          // REVIEW: favor the container over the key file - C# appears to do this
                          if isSome container then
                            Some(Ilwrite.signerOpenKeyContainer container.Value)
                          else
                            match signer with 
                            | None -> None
                            | Some(s) ->
                               try 
                                if delaysign then
                                  Some (Ilwrite.signerOpenPublicKeyFile s) 
                                else
                                  Some (Ilwrite.signerOpenKeyPairFile s) 
                               with e -> 
                                   // Note:: don't use errorR here since we really want to fail and not produce a binary
                                   error(Error(sprintf "The key file '%s' could not be opened" s,rangeCmdArgs))
                         end;
                       fixupOverlappingSequencePoints = false; } 
                  ilxMainModule
            with Failure msg -> 
                error(Error(sprintf "A problem occurred writing the binary %s: %s" outfile msg, rangeCmdArgs))
        with e -> 
            errorRecoveryNoRange e; 
            exiter.Exit 1 

      
    let WriteConfigFile(tcConfig:TcConfig,ilGlobals:ILGlobals,outfile) = 
        if tcConfig.generateConfigFile && tcConfig.target.IsExe then 
            use os = new  StreamWriter((outfile^".config"),append=false,encoding=System.Text.Encoding.UTF8)
            let (v1,v2,v3,_) = ilGlobals.mscorlib_scoref.AssemblyRef.Version.Value
            Printf.twprintf os "<?xml version =\"1.0\"?>\n\
        <configuration>\n\
            <startup>\n\
                <supportedRuntime version=\"v%d.%d.%d\" safemode=\"true\"/>\n\
                <requiredRuntime version=\"v%d.%d.%d\" safemode=\"true\"/>\n\
            </startup>\n\
        </configuration>" 
              (int v1) (int v2) (int v3) 
              (int v1) (int v2) (int v3);

    let WriteStatsFile (tcConfig:TcConfig,outfile) = 
      if tcConfig.stats then 
          try 
              use oc = new  StreamWriter((outpath outfile "stats.txt"),append=false,encoding=System.Text.Encoding.UTF8) :> TextWriter
#if STATISTICS
              Ilread.report oc;
#endif
              Ilxgen.report oc;
          with _ -> ()



let abortOnError (errorLogger:ErrorLogger) = 
    if errorLogger.ErrorCount > 0 then exiter.Exit 1 
    
let KeyFileWarningE = DeclareResourceString("KeyFileWarning","")
let KeyNameWarningE = DeclareResourceString("KeyNameWarning","")
let DelaySignWarningE = DeclareResourceString("DelaySignWarning","")
    

let ValidateKeySigningAttributes (tcConfig : TcConfig) tcGlobals topAttrs =
    let delaySignAttrib = AttributeHelpers.TryFindBoolAttribute tcGlobals "System.Reflection.AssemblyDelaySignAttribute" topAttrs.assemblyAttrs
    let signerAttrib = AttributeHelpers.TryFindStringAttribute tcGlobals "System.Reflection.AssemblyKeyFileAttribute" topAttrs.assemblyAttrs
    let containerAttrib = AttributeHelpers.TryFindStringAttribute tcGlobals "System.Reflection.AssemblyKeyNameAttribute" topAttrs.assemblyAttrs
    
    // REVIEW: C# throws a warning when these attributes are used - should we?
    
    // if delaySign is set via an attribute, validate that it wasn't set via an option
    let delaysign = 
        match delaySignAttrib with 
        | Some delaysign -> 
          if tcConfig.delaysign then
            warning(Error(DelaySignWarningE.Format,rangeCmdArgs)) ;
            tcConfig.delaysign
          else
            delaysign
        | _ -> tcConfig.delaysign
        
         
    // if signer is set via an attribute, validate that it wasn't set via an option
    let signer = 
        match signerAttrib with
        | Some signer -> 
            if tcConfig.signer.IsSome && tcConfig.signer <> Some signer then
                warning(Error(KeyFileWarningE.Format,rangeCmdArgs)) ;
                tcConfig.signer
            else
                Some signer
        | None -> tcConfig.signer
    
    // if container is set via an attribute, validate that it wasn't set via an option, and that they keyfile wasn't set
    // if keyfile was set, use that instead (silently)
    // REVIEW: This is C# behavior, but it seems kind of sketchy that we fail silently
    let container = 
        match containerAttrib with 
        | Some container -> 
            if tcConfig.container.IsSome && tcConfig.container <> Some container then
              warning(Error(KeyNameWarningE.Format,rangeCmdArgs)) ;
              tcConfig.container
            else
              Some container
        | None -> tcConfig.container
    
    SigningInfo (delaysign,signer,container)
    
    

//----------------------------------------------------------------------------
// main - split up to make sure that we can GC the
// dead data at the end of each phase.  We explicitly communicate arguments
// from one phase to the next.
//-----------------------------------------------------------------------------
  
type Args<'a> = Args  of 'a

let main1(argv) =
    let tcConfigB = Build.TcConfigBuilder.CreateNew(defaultFSharpBinariesDir, false,Directory.GetCurrentDirectory())
    // Preset: --optimize+ -g --tailcalls+ (see 4505)
    SetOptimizeSwitch tcConfigB On
    SetDebugSwitch    tcConfigB None Off
    SetTailcallSwitch On    

    let errorLogger = ErrorLoggerInitial tcConfigB

    // Install the global error logger and never remove it
    let _ = InstallGlobalErrorLogger (fun _ -> errorLogger)

    
    // process command line, flags and collect filenames 
    let sourceFiles = 

      // The ParseCompilerOptions function calls imperative function to process "real" args
      // Rather than start processing, just collect names, then process them. 
      try 
          let inputFilesRef   = ref ([] : string list)
          let collect name = 
              let lower = String.lowercase name
              if List.exists (Filename.check_suffix lower) [".resx"]  then
                  tcConfigB.AddEmbeddedResource name
              else
                  inputFilesRef := name :: !inputFilesRef
          let abbrevArgs = abbrevFlagSet tcConfigB true
          ParseCompilerOptions collect (GetCoreFscCompilerOptions tcConfigB) (List.tl (PostProcessCompilerArgs abbrevArgs argv));
          abortOnError errorLogger;
          let inputFiles = List.rev !inputFilesRef
          
          if tcConfigB.utf8output then 
              let prev = System.Console.OutputEncoding
              System.Console.OutputEncoding <- System.Text.Encoding.UTF8
              System.AppDomain.CurrentDomain.ProcessExit.Add(fun _ -> System.Console.OutputEncoding <- prev)

          (* step - get dll references *)
          let dllFiles,sourceFiles = List.partition Filename.isDll inputFiles
          match dllFiles with
          | [] -> ()
          | h::t -> deprecated (sprintf "The assembly '%s' is listed on the command line and has been added as a reference. Assemblies should now be referenced using a command line flag such as '-r'" h) rangeStartup

          tcConfigB.referencedDLLs <- tcConfigB.referencedDLLs @ (dllFiles |> List.map(fun  f -> AssemblyReference(rangeStartup,f)) );
          sourceFiles

      with 
          e -> errorRecovery e rangeStartup; exiter.Exit 1 
          
    let sourceFiles = 
        sourceFiles 
        |> List.map (fun file -> if Path.IsPathRooted(file) then file else Path.Combine(tcConfigB.implicitIncludeDir, file))
    
    tcConfigB.conditionalCompilationDefines <- "COMPILED" :: tcConfigB.conditionalCompilationDefines 
    // display the banner text, if necessary
    Microsoft.FSharp.Compiler.Fscopts.DisplayBannerText tcConfigB

    // Create tcGlobals and frameworkTcImports
    let outfile,pdbfile,assemblyName = tcConfigB.DecideNames sourceFiles
    abortOnError errorLogger; // DecideNames may give "no inputs" error. Abort on error at this point. bug://3911

    let tcConfig = TcConfig.Create(tcConfigB,validate=false)


    let errorLogger = ErrorLoggerThatQuitsAfterMaxErrors tcConfig

    // Install the global error logger and never remove it
    let _ = InstallGlobalErrorLogger (fun _ -> errorLogger)

    // Share intern'd strings across all lexing/parsing
    let lexResourceManager = new Lexhelp.LexResourceManager()
    
    
    (* step - decideNames *)  
    abortOnError errorLogger;

    // Nice name generator    
    let niceNameGen = NiceNameGenerator()
    
    let tcGlobals,tcImports,generatedCcu,typedAssembly,topAttrs,tcConfig = 
    
        ReportTime tcConfig "Import mscorlib";
        let tcConfigP = TcConfigProvider.Constant(tcConfig)

        if tcConfig.useIncrementalBuilder then 
            ReportTime tcConfig "Incremental Parse and Typecheck";
            let projectDirectory = System.IO.Directory.GetCurrentDirectory() 
            let build,_ = 
                IncrementalFSharpBuild.Create (tcConfig, projectDirectory, assemblyName, niceNameGen, lexResourceManager, sourceFiles, 
                                               false, // no need to stay reactive
                                               IncrementalFSharpBuild.BuildEvents.Default,
                                               errorLogger,
                                               errorRecovery)
            let build,tcState,topAttribs,typedAssembly,tcEnv,tcImports,tcGlobals,tcConfig = IncrementalFSharpBuild.TypeCheck(build) 
            tcGlobals,tcImports,tcState.Ccu,typedAssembly,topAttribs,tcConfig
        else 
            (* step - parse sourceFiles *)
            ReportTime tcConfig "Parse inputs";
            let inputs =
                try  
                   sourceFiles 
                   |> tcConfig.ComputeCanContainEntryPoint 
                   |> List.zip sourceFiles
                   |> List.choose (fun (input,canContainEntryPoint) -> 
                         let lower = String.lowercase input
                         ParseOneInputFile(tcConfig,lexResourceManager,["COMPILED"],input,canContainEntryPoint,errorLogger)) 
                with e -> 
                    errorRecoveryNoRange e; exiter.Exit 1
            if tcConfig.parseOnly then exiter.Exit 0 else ();
            abortOnError errorLogger;

            if tcConfig.printAst then 
                let show input =
                  let opts = Internal.Utilities.StructuredFormat.FormatOptions.Default
                  let l = Internal.Utilities.StructuredFormat.Display.any_to_layout opts input
                  Internal.Utilities.StructuredFormat.Display.output_layout opts stdout l
               
                inputs |> List.iter (fun input -> printf "AST:\n"; show input; printf "\n") 

            let tcConfig = (tcConfig,inputs) ||> List.fold ApplyMetaCommandsFromInputToTcConfig 
            let tcConfigP = TcConfigProvider.Constant(tcConfig)

            ReportTime tcConfig "Import mscorlib and FSharp.Core.dll";

            ReportTime tcConfig "Import system references";
            let sysRes,otherRes,_ = TcAssemblyResolutions.SplitNonFoundationalResolutions tcConfig
            let tcGlobals,frameworkTcImports = TcImports.BuildFrameworkTcImports (tcConfigP,sysRes)

            ReportTime tcConfig "Import non-system references";
            let tcGlobals,tcImports =  
                let tcImports = TcImports.BuildNonFrameworkTcImports(tcConfigP,tcGlobals,frameworkTcImports,otherRes)
                tcGlobals,tcImports
            abortOnError errorLogger;

            ReportTime tcConfig "Typecheck";
            let tcEnv0 = GetInitialTypecheckerEnv (Some assemblyName) rangeStartup tcConfig tcImports tcGlobals

            // typecheck 
            let tcState,topAttrs,typedAssembly,tcEnvAtEnd = 
                TypeCheck(tcConfig,tcImports,tcGlobals,errorLogger,assemblyName,niceNameGen,tcEnv0,inputs)

            let generatedCcu = tcState.Ccu
            abortOnError errorLogger;
            ReportTime tcConfig "Typechecked";

            (tcGlobals,tcImports,generatedCcu,typedAssembly,topAttrs,tcConfig)

    if tcConfig.typeCheckOnly then exiter.Exit 0;
    
    let signingInfo = ValidateKeySigningAttributes tcConfig tcGlobals topAttrs
    
    abortOnError errorLogger;

    // Build an updated errorLogger that filters according to the scopedPragmas. Then install
    // it as the updated global error logger and never remove it
    let oldLogger = errorLogger
    let errorLogger = 
        let scopedPragmas = 
            let (TAssembly(impls)) = typedAssembly 
            [ for (TImplFile(_,pragmas,_)) in impls do yield! pragmas ]
        GetErrorLoggerFilteringByScopedPragmas(true,scopedPragmas,oldLogger)

    let _ = InstallGlobalErrorLogger(fun _ -> errorLogger)


    // Try to find an AssemblyVersion attribute 
    let assemVerFromAttrib = 
        match AttributeHelpers.TryFindVersionAttribute tcGlobals "System.Reflection.AssemblyVersionAttribute" topAttrs.assemblyAttrs with
        | Some v -> 
           match tcConfig.version with 
           | VersionNone -> Some v
           | _ -> warning(Error("The 'AssemblyVersionAttribute' has been ignored because a version was given using a command line option",Range.rangeStartup)); None
        | _ -> None

    // write interface, xmldoc, generateHtmlDocs 
    begin
      ReportTime tcConfig ("Write Interface File");
      if tcConfig.printSignature   then InterfaceFileWriter.WriteInterfaceFile (tcGlobals,tcConfig,tcImports,generatedCcu,typedAssembly);
      ReportTime tcConfig ("Write XML docs");
      tcConfig.xmlDocOutputFile |> Option.iter (fun xmlFile -> XmlDocWriter.writeXmlDoc (assemblyName,tcGlobals,generatedCcu,xmlFile))
      ReportTime tcConfig ("Write HTML docs");
      if tcConfig.generateHtmlDocs then 
          HtmlDocWriter.WriteHTMLDoc (tcConfig,tcImports,tcGlobals,generatedCcu,tcConfig.htmlDocDirectory,tcConfig.htmlDocAppendFlag,tcConfig.htmlDocCssFile,tcConfig.htmlDocNamespaceFile,assemVerFromAttrib);
    end;


    // Pass on only the minimimum information required for the next phase to ensure GC kicks in.
    // In principle the JIT should be able to do good liveness analysis to clean things up, but the
    // data structures involved here are so large we can't take the risk.
    Args(tcConfig,tcImports,tcGlobals,errorLogger,generatedCcu,outfile,typedAssembly,topAttrs,pdbfile,assemblyName,assemVerFromAttrib,signingInfo)

  
let main2(Args(tcConfig,tcImports,tcGlobals,errorLogger,generatedCcu:ccu,outfile,typedAssembly,topAttrs,pdbfile,assemblyName,assemVerFromAttrib,signingInfo)) = 
      
    ReportTime tcConfig ("Encode Interface Data");
    if !verboseStamps then 
        dprintf "---------------------- START MAKE EXPORT REMAPPING ------------\n";
    let exportRemapping = MakeExportRemapping generatedCcu generatedCcu.Contents
    if !verboseStamps then 
        dprintf "---------------------- END MAKE EXPORT REMAPPING ------------\n";
    
    let idata = EncodeInterfaceData(tcConfig,tcGlobals,errorLogger,exportRemapping,generatedCcu,outfile)
        
    if !progress && tcConfig.optSettings.jitOptUser = Some false then 
        dprintf "Note, optimizations are off.\n";
    (* optimize *)
    let optEnv0 = InitialOptimizationEnv tcImports
   
    let importMap = tcImports.GetImportMap()
    let optimizedImpls,optimizationData,_ = ApplyAllOptimizations (tcConfig,tcGlobals,outfile,importMap,false,optEnv0,generatedCcu,typedAssembly)

    abortOnError errorLogger;
        
    ReportTime tcConfig ("Encoding OptData");
    let generatedOptData = EncodeOptimizationData(tcGlobals,tcConfig,outfile,exportRemapping,(generatedCcu,optimizationData))
    
    // Pass on only the minimimum information required for the next phase to ensure GC kicks in.
    // In principle the JIT should be able to do good liveness analysis to clean things up, but the
    // data structures involved here are so large we can't take the risk.
    Args(tcConfig,tcImports,tcGlobals,errorLogger,generatedCcu,outfile,optimizedImpls,topAttrs,importMap,pdbfile,assemblyName, idata,generatedOptData,assemVerFromAttrib,signingInfo)

let main2b(Args(tcConfig:TcConfig,tcImports,tcGlobals,errorLogger,generatedCcu:ccu,outfile,optimizedImpls,topAttrs,importMap,pdbfile,assemblyName,idata,generatedOptData,assemVerFromAttrib,signingInfo)) = 
  
    // Compute a static linker. 
    let ilGlobals = tcGlobals.ilg
    if tcConfig.standalone && generatedCcu.UsesQuotations then 
        error(Error("Code in this assembly makes uses of quotation literals. Static linking may not include components that make use of quotation literals",rangeStartup));
    let staticLinker = StaticLinker.StaticLink (tcConfig,tcImports,ilGlobals)

    ReportTime tcConfig "TAST -> ILX";
    let ilxGenEnv = IlxgenEnvInit(tcConfig,tcImports,tcGlobals,generatedCcu)
    let codegenResults = GenerateIlxCode(false,false,tcGlobals,tcConfig,importMap,topAttrs,optimizedImpls,generatedCcu,generatedCcu.AssemblyName,ilxGenEnv)


    let mainmodx = MainModuleBuilder.CreateMainModule  (tcConfig,tcGlobals,pdbfile,assemblyName,outfile,topAttrs,idata,generatedOptData,codegenResults,assemVerFromAttrib)
#if DEBUG
    // Print code before bailing out from the compiler due to errors 
    // in the backend of the compiler.  The partially-generated 
    // ILX code often contains useful information. 
    if tcConfig.writeGeneratedILFiles then StaticLinker.PrintModule (outpath outfile "ilx.txt") mainmodx;
#endif

    abortOnError errorLogger;
    
    Args (tcConfig,errorLogger,staticLinker,ilGlobals,outfile,pdbfile,mainmodx,signingInfo)

let main2c(Args(tcConfig,errorLogger,staticLinker,ilGlobals,outfile,pdbfile,mainmodx,signingInfo)) = 
      
    ReportTime tcConfig "ILX -> IL";
    let ilxMainModule = MainModuleBuilder.EraseILX (ilGlobals,mainmodx)
    abortOnError errorLogger;
    Args(tcConfig,errorLogger,staticLinker,ilGlobals,ilxMainModule,outfile,pdbfile,signingInfo)
  

let main3(Args(tcConfig,errorLogger:ErrorLogger,staticLinker,ilGlobals,ilxMainModule,outfile,pdbfile,signingInfo)) = 
        
    let ilxMainModule =  
        try  staticLinker (ilxMainModule,outfile)
        with e -> errorRecoveryNoRange e; exiter.Exit 1
    abortOnError errorLogger;
        
    Args (tcConfig,errorLogger,ilGlobals,ilxMainModule,outfile,pdbfile,signingInfo)

let main4(Args(tcConfig,errorLogger,ilGlobals,ilxMainModule,outfile,pdbfile,signingInfo)) = 
    ReportTime tcConfig "Write .NET Binary";
    FileWriter.EmitIL (tcConfig,ilGlobals,errorLogger,outfile,pdbfile,ilxMainModule,signingInfo); 

    ReportTime tcConfig "Write Config File";
    FileWriter.WriteConfigFile(tcConfig,ilGlobals,outfile);
    ReportTime tcConfig "Write Stats File";
    FileWriter.WriteStatsFile (tcConfig,outfile);

    abortOnError errorLogger;
    ReportTime tcConfig "Exiting"


let main(argv) = main1(argv) |> main2 |> main2b |> main2c |> main3 |> main4
