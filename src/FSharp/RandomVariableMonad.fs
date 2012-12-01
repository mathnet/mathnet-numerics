namespace MathNet.Numerics

#nowarn "40"

open System
open System.Collections
open System.Collections.Generic

module RandomVariable =
    
    type 'a Outcome = {
        Value: 'a
        Probability : BigRational    }
    
    type 'a RandomVariable = 'a Outcome seq
    
    // P(A AND B) = P(A | B) * P(B)
    let bind (f: 'a -> 'b RandomVariable) (dist:'a RandomVariable) =
        dist 
        |> Seq.map (fun p1 -> 
            f p1.Value
            |> Seq.map (fun p2 -> 
                { Value = p2.Value; 
                    Probability = 
                        p1.Probability * p2.Probability}))
        |> Seq.concat : 'b RandomVariable
    
    /// Sequentially compose two actions, passing any value produced by the first as an argument to the second.
    let inline (>>=) dist f = bind f dist
    /// Flipped >>=
    let inline (=<<) f dist = bind f dist
    
    /// Inject a value into the RandomVariable type
    let returnM (value:'a) =     
        Seq.singleton { Value = value ; Probability = 1N/1N }
            : 'a RandomVariable
    
    type RandomVariableMonadBuilder() =
        member this.Bind (r, f) = bind f r
        member this.Return x = returnM x
        member this.ReturnFrom x = x
    
    let randomVariable = RandomVariableMonadBuilder()
    
    // Create some helpers
    let toUniformDistribution seq : 'a RandomVariable =
        let l = Seq.length seq
        seq 
        |> Seq.map (fun e ->
            { Value = e; 
                Probability = 1N / bignum.FromInt l })
    
    let probability (dist:'a RandomVariable) = 
        dist
        |> Seq.map (fun o -> o.Probability)
        |> Seq.sum
    
    let certainly = returnM
    let impossible<'a> :'a RandomVariable = toUniformDistribution []
    
    let fairDice sides = toUniformDistribution [1..sides]
    
    type CoinSide = 
        | Heads 
        | Tails
    
    let fairCoin = toUniformDistribution [Heads; Tails]
    
    let filter predicate (dist:'a RandomVariable) : 'a RandomVariable =
        dist |> Seq.filter (fun o -> predicate o.Value)
    
    let filterInAnyOrder items dist =
        items
        |> Seq.fold (fun d item -> filter (Seq.exists ((=) (item))) d) dist

    /// Transforms a RandomVariable value by using a specified mapping function.
    let map f (dist:'a RandomVariable) : 'b RandomVariable = 
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
