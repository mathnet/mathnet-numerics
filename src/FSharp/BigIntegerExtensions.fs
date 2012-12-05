namespace System.Numerics

open System

#if PORTABLE

[<AutoOpen>]
module BigIntegerExtensions =

    let private parse str =
        let len = String.length str
        let rec build acc i =
            if i = len then
                acc
            else
                let c = str.[i]
                let d = int c - int '0'
                if 0 <= d && d <= 9 then
                    build (10I * acc + (bigint d)) (i+1)
                else
                    raise (new FormatException("The value could not be parsed"))
        build 0I 0

    type BigInteger with

        static member Parse(text: string) =
            let len = text.Length
            if len = 0 then raise (new FormatException("The value could not be parsed"))
            if text.[0..0] = "-" then
                parse text.[1..len-1] |> bigint.Negate
            else
                parse text

#endif
