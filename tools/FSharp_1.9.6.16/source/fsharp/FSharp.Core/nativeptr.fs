//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.NativeInterop

#nowarn "44";;
open Microsoft.FSharp.Core
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Primitives.Basics
open Microsoft.FSharp.Core.Operators

open System
open System.Diagnostics
open System.Runtime.InteropServices

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module NativePtr = 

    [<NoDynamicInvocation>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
    let inline of_nativeint (x:nativeint)      = (# "" x : nativeptr<'T> #)
    
    [<NoDynamicInvocation>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
    let inline to_nativeint (x: nativeptr<'T>) = (# "" x : nativeint    #)

    [<NoDynamicInvocation>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
    let inline add (x : nativeptr<'T>) (n:int) : nativeptr<'T> = to_nativeint x + nativeint n * (# "sizeof !0" type('T) : nativeint #) |> of_nativeint
    
    [<NoDynamicInvocation>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
    let inline get (p : nativeptr<'T>) n = (# "ldobj !0" type ('T) (add p n) : 'T #) 
    
    [<NoDynamicInvocation>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
    let inline set (p : nativeptr<'T>) n (x : 'T) = (# "stobj !0" type ('T) (add p n) x #)  

    [<NoDynamicInvocation>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
    let inline read (p : nativeptr<'T>) = (# "ldobj !0" type ('T) p : 'T #) 
    
    [<NoDynamicInvocation>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
    let inline write (p : nativeptr<'T>) (x : 'T) = (# "stobj !0" type ('T) p x #)  
    
    [<NoDynamicInvocation>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
    let inline of_array (arr: 'T[]) (m:int) : nativeptr<'T> = (# "ldelema !0" type('T) arr m : nativeptr<'T> #)
    
    [<NoDynamicInvocation>]
    [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
    let inline of_array2 (arr: 'T[,]) (n:int) (m:int) : nativeptr<'T> = (# "ldelema.multi 2 !0" type('T) arr n m : nativeptr<'T> #)

