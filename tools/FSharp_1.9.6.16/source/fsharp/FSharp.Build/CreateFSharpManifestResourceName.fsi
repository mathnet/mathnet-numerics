#light

namespace Microsoft.FSharp.Build

[<Class>]
type CreateFSharpManifestResourceName =
    inherit Microsoft.Build.Tasks.CreateCSharpManifestResourceName
    public new : unit -> CreateFSharpManifestResourceName
