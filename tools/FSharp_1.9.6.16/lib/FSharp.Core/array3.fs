//=========================================================================
// (c) Microsoft Corporation 2005-2009. 
//=========================================================================

namespace Microsoft.FSharp.Collections

    open System.Diagnostics
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Array3D =

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let length1 (arr: 'a[,,]) =  (# "ldlen.multi 3 0" arr : int #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let length2 (arr: 'a[,,]) =  (# "ldlen.multi 3 1" arr : int #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let length3 (arr: 'a[,,]) =  (# "ldlen.multi 3 2" arr : int #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let get (arr: 'a[,,]) (n1:int) (n2:int) (n3:int) =  (# "ldelem.multi 3 !0" type ('a) arr n1 n2 n3 : 'a #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let set (arr: 'a[,,]) (n1:int) (n2:int) (n3:int) (x:'a) =  (# "stelem.multi 3 !0" type ('a) arr n1 n2 n3 x #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let zeroCreate (n1:int) (n2:int) (n3:int) = (# "newarr.multi 3 !0" type ('a) n1 n2 n3 : 'a[,,] #)
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let create (n1:int) (n2:int) (n3:int) (x:'a) =
            let arr = (zeroCreate n1 n2 n3 : 'a[,,])
            for i = 0 to n1 - 1 do 
              for j = 0 to n2 - 1 do 
                for k = 0 to n3 - 1 do 
                  arr.[i,j,k] <- x
            arr

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let init n1 n2 n3 f = 
            let arr = (zeroCreate n1 n2 n3 : 'a[,,]) 
            for i = 0 to n1 - 1 do 
              for j = 0 to n2 - 1 do 
                for k = 0 to n3 - 1 do 
                  arr.[i,j,k] <- f i j k
            arr

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iter f arr =
          let len1 = length1 arr
          let len2 = length2 arr
          let len3 = length3 arr
          for i = 0 to len1 - 1 do 
            for j = 0 to len2 - 1 do 
              for k = 0 to len3 - 1 do 
                f arr.[i,j,k]

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let map f arr =
          let len1 = length1 arr
          let len2 = length2 arr
          let len3 = length3 arr
          let res = (zeroCreate len1 len2 len3 : 'b[,,])
          for i = 0 to len1 - 1 do 
            for j = 0 to len2 - 1 do 
              for k = 0 to len3 - 1 do 
                res.[i,j,k] <-  f arr.[i,j,k]
          res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let iteri f arr =
          let len1 = length1 arr
          let len2 = length2 arr
          let len3 = length3 arr
          for i = 0 to len1 - 1 do 
            for j = 0 to len2 - 1 do 
              for k = 0 to len3 - 1 do 
                f i j k arr.[i,j,k] 

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let mapi f arr =
          let len1 = length1 arr
          let len2 = length2 arr
          let len3 = length3 arr
          let res = (zeroCreate len1 len2 len3 : 'b[,,])
          for i = 0 to len1 - 1 do 
            for j = 0 to len2 - 1 do 
              for k = 0 to len3 - 1 do 
                res.[i,j,k] <- f i j k arr.[i,j,k]
          res

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let zero_create n1 n2 n3 = zeroCreate n1 n2 n3

    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    [<RequireQualifiedAccess>]
    module Array4D =

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let length1 (arr: 'a[,,,]) =  (# "ldlen.multi 4 0" arr : int #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let length2 (arr: 'a[,,,]) =  (# "ldlen.multi 4 1" arr : int #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let length3 (arr: 'a[,,,]) =  (# "ldlen.multi 4 2" arr : int #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let length4 (arr: 'a[,,,]) =  (# "ldlen.multi 4 3" arr : int #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let zeroCreate (n1:int) (n2:int) (n3:int) (n4:int) = (# "newarr.multi 4 !0" type ('a) n1 n2 n3 n4 : 'a[,,,] #)
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let create n1 n2 n3 n4 (x:'a) =
            let arr = (zeroCreate n1 n2 n3 n4 : 'a[,,,])
            for i = 0 to n1 - 1 do 
              for j = 0 to n2 - 1 do 
                for k = 0 to n3 - 1 do 
                  for m = 0 to n4 - 1 do 
                    arr.[i,j,k,m] <- x
            arr

        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let init n1 n2 n3 n4 f = 
            let arr = (zeroCreate n1 n2 n3 n4 : 'a[,,,]) 
            for i = 0 to n1 - 1 do 
              for j = 0 to n2 - 1 do 
                for k = 0 to n3 - 1 do 
                  for m = 0 to n4 - 1 do 
                    arr.[i,j,k,m] <- f i j k m
            arr


        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let get (arr: 'a[,,,]) (n1:int) (n2:int) (n3:int) (n4:int) =  (# "ldelem.multi 4 !0" type ('a) arr n1 n2 n3  n4 : 'a #)  
 
        [<CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1709:IdentifiersShouldBeCasedCorrectly"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1707:IdentifiersShouldNotContainUnderscores"); CodeAnalysis.SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly")>]             
        let set (arr: 'a[,,,]) (n1:int) (n2:int) (n3:int) (n4:int) (x:'a) =  (# "stelem.multi 4 !0" type ('a) arr n1 n2 n3 n4 x #)  
 
