module MathNet.Numerics.Probability

#nowarn "40"

#if NOSYSNUMERICS
#else

open System
open System.Collections
open System.Collections.Generic
open MathNet.Numerics

type Outcome<'T> = {
    Value: 'T
    Probability : BigRational }

type RandomVariable<'T> = Outcome<'T> seq

// P(A AND B) = P(A | B) * P(B)
let private bind f dist =
    dist
    |> Seq.map (fun p1 ->
        f p1.Value
        |> Seq.map (fun p2 ->
            { Value = p2.Value;
                Probability =
                    p1.Probability * p2.Probability}))
    |> Seq.concat

/// Inject a value into the RandomVariable type
let private returnM value =
    Seq.singleton { Value = value ; Probability = 1N/1N }

type RandomVariableBuilder() =
    member this.Bind (r, f) = bind f r
    member this.Return x = returnM x
    member this.ReturnFrom x = x

let randomVariable = RandomVariableBuilder()

type CoinSide =
    | Heads
    | Tails

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module RandomVariable =

    // Create some helpers
    let toUniformDistribution seq =
        let l = Seq.length seq
        seq
        |> Seq.map (fun e ->
            { Value = e;
                Probability = 1N / bignum.FromInt l })

    let probability dist =
        dist
        |> Seq.map (fun o -> o.Probability)
        |> Seq.sum

    let certainly = returnM
    let impossible<'a> :'a RandomVariable = toUniformDistribution []

    let fairDice sides = toUniformDistribution [1..sides]

    let fairCoin = toUniformDistribution [Heads; Tails]

    let filter predicate dist =
        dist |> Seq.filter (fun o -> predicate o.Value)

    let filterInAnyOrder items dist =
        items
        |> Seq.fold (fun d item -> filter (Seq.exists ((=) (item))) d) dist

    /// Transforms a RandomVariable value by using a specified mapping function.
    let map f dist =
        dist
        |> Seq.map (fun o -> { Value = f o.Value; Probability = o.Probability })

    let selectOne values =
        [for e in values -> e,values |> Seq.filter ((<>) e)]
        |> toUniformDistribution

    let rec selectMany n values =
        match n with
        | 0 -> certainly ([],values)
        | _ ->
            randomVariable {
                let! (x,c1) = selectOne values
                let! (xs,c2) = selectMany (n-1) c1
                return x::xs,c2}

    let select n values =
        selectMany n values
        |> map (fst >> List.rev)

    let remove items = Seq.filter (fun v -> Seq.forall ((<>) v) items)

#endif
