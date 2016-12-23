namespace System.Numerics

#if PORTABLE
#if NOSYSNUMERICS
#else

open System

[<AutoOpen>]
module BigIntegerExtensions =
    //
    let rec private parseImpl (str : string) len acc i =
        if i = len then
            acc
        else
            let d = int str.[i] - int '0'
            if 0 <= d && d <= 9 then
                parseImpl str len (10I * acc + (bigint d)) (i + 1)
            else
                raise <| FormatException ("The value could not be parsed.")

    type BigInteger with
        //
        static member Parse (str : string) =
            let len = str.Length
            if len = 0 then
                raise <| FormatException ("The value could not be parsed.")

            if str.[0] = '-' then
                -(parseImpl str len 0I 1)
            else
                parseImpl str len 0I 0

#endif
#endif
