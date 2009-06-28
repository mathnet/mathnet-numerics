namespace Internal.Utilities
open System
open System.IO
open System.Configuration
open System.Reflection

#nowarn "44" // ConfigurationSettings is obsolete but the new stuff is horribly complicated. 

module internal FSharpEnvironment =

    let mutable private location : string option option = None

#if FX_ATLEAST_40 
    /// The .NET runtime version that F# was built against.
    let DotNetRuntime = sprintf "v%s.%s.%s" Microsoft.BuildSettings.Version.Major Microsoft.BuildSettings.Version.Minor Microsoft.BuildSettings.Version.ProductBuild

    /// Name of this F# product
    let FSharpProductName = "Microsoft Visual F# 4.0"
#endif

    let FSharpRunningVersion = 
        try match (typeof<Microsoft.FSharp.Collections.List<int>>).Assembly.GetName().Version.ToString() with
            | null -> None
            | "" -> None
            | s  -> Some(s)
        with _ -> None

    /// The default location of FSharp.Core.dll and fsc.exe based on the version of fsc.exe that is running
    let FSharpRunningBinFolder = 
        match  location with 
        | Some(location) -> location      
        | None -> 
            match FSharpRunningVersion with 
            | None -> None
            | Some v -> 
                location 
                    <- Some(// Check for an app.config setting to redirect the compiler location
                            let appConfigKey = "fsharp-compiler-location-" + v // Like fsharp-compiler-location-4.0.0.0

                            let locationFromAppConfig = ConfigurationSettings.AppSettings.[appConfigKey]
                            System.Diagnostics.Debug.Print(sprintf "Considering appConfigKey %s which has value '%s'" appConfigKey locationFromAppConfig) 

                            if not(String.IsNullOrEmpty(locationFromAppConfig)) then 
                                let exeAssemblyFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                                let locationFromAppConfig = locationFromAppConfig.Replace("{exepath}", exeAssemblyFolder)
                                System.Diagnostics.Debug.Print(sprintf "Using path %s" locationFromAppConfig) 
                                Some(locationFromAppConfig)
                            else 
#if FX_ATLEAST_40 
                                let key = sprintf @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\.NETFramework\%s\AssemblyFoldersEx\%s" DotNetRuntime FSharpProductName
#else
                                let key = @"HKEY_LOCAL_MACHINE\Software\Microsoft\.NETFramework\AssemblyFolders\Microsoft.FSharp-" + v
#endif
                                let result = 
                                    try match Microsoft.Win32.Registry.GetValue(key,null,"") :?> String with
                                        | null -> None
                                        | "" -> None
                                        | s  -> Some(s)
                                    with _ -> None

// This was failing on rolling build for staging because the prototype compiler doesn't have the key. Disable there.
#if FX_ATLEAST_40 
                                System.Diagnostics.Debug.Assert(result<>None, sprintf "Could not find location of compiler at '%s'" key)
#endif                                
                                result
                            )
                location.Value
