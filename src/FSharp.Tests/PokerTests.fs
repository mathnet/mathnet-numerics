module MathNet.Numerics.Tests.PokerTests

#if NOSYSNUMERICS
#else

#nowarn "25"

open NUnit.Framework
open FsUnitTyped

open MathNet.Numerics
open MathNet.Numerics.Probability

type Rank = int
type Suit = | Spades | Hearts | Diamonds | Clubs
type Card = Rank * Suit

let value = fst
let suit = snd

let A,K,Q,J,T = 14,13,12,11,10
let allRanksInSuit suit = [2..A] |> List.map (fun rank -> rank,suit)
let completeDeck =
  [Spades; Hearts ; Diamonds; Clubs]
    |> List.map allRanksInSuit
    |> List.concat

let isPair c1 c2 = value c1 = value c2
let isSuited c1 c2 = suit c1 = suit c2
let isConnected c1 c2 =
    let v1,v2 = value c1,value c2
    (v1 - v2 |> abs |> (=) 1) ||
    (v1 = A && v2 = 2) ||
    (v1 = 2 && v2 = A)

[<Test>]
let ``When drawing from a full deck, then the probability for an Ace should equal 4/52``() =
  completeDeck
    |> RandomVariable.selectOne
    |> RandomVariable.map fst
    |> RandomVariable.filter (fun card -> value card = A)
    |> RandomVariable.probability
    |> shouldEqual (4N/52N)

[<Test>]
let ``When drawing from a full deck, then the probability should equal 1/52``() =
  completeDeck
    |> RandomVariable.selectOne
    |> RandomVariable.map fst
    |> RandomVariable.filter ((=) (A,Spades))
    |> RandomVariable.probability
    |> shouldEqual (1N/52N)

[<Test>]
let ``When drawing from a full deck, then the probability for the Ace of Clubs and Ace of Spaces (in order) should equal 1/52 * 1/51``() =
  completeDeck
    |> RandomVariable.select 2
    |> RandomVariable.filter ((=) [A,Clubs; A,Spades])
    |> RandomVariable.probability
    |> shouldEqual (1N/52N * 1N/51N)

[<Test>]
let ``When drawing from a full deck, then the probability for the Ace of Clubs and Ace of Spaces (in any order) should equal (1/52 * 1/51) * 2``() =
  completeDeck
    |> RandomVariable.select 2
    |> RandomVariable.filterInAnyOrder [A,Clubs; A,Spades]
    |> RandomVariable.probability
    |> shouldEqual ((1N/52N * 1N/51N) * 2N)

[<Test>]
let ``When drawing the Ace of Spades and the Ace of Clubs, then the probability for drawing another Ace should equal 2/50``() =
  completeDeck
    |> RandomVariable.remove [A,Clubs; A,Spades]
    |> RandomVariable.toUniformDistribution
    |> RandomVariable.filter (fun card -> value card = A)
    |> RandomVariable.probability
    |> shouldEqual (2N/50N)


[<Test>]
let ``When drawing from the full deck, then the probability for drawing a Pair preflop should equal 1/17``() =
  completeDeck
    |> RandomVariable.select 2
    |> RandomVariable.filter (fun (c1::c2::_) -> isPair c1 c2)
    |> RandomVariable.probability
    |> shouldEqual (1N/17N)

[<Test>]
let ``When drawing from the full deck, then the probability for drawing Suited Connectors should equal 1/25``() =
  completeDeck
    |> RandomVariable.select 2
    |> RandomVariable.filter (fun (c1::c2::_) -> isSuited c1 c2 && isConnected c1 c2)
    |> RandomVariable.probability
    |> shouldEqual (2N/51N)

[<Test>]
let ``When holding 3 Spades after the flop, than the probability for drawing a flush should equal 10/47*9/46``() =
  completeDeck
    |> RandomVariable.remove [A,Clubs; A,Spades]           // preflop
    |> RandomVariable.remove [2,Clubs; 3,Spades; 7,Spades] // flop
    |> RandomVariable.select 2
    |> RandomVariable.filter (fun (c1::c2::_) -> suit c1 = Spades && suit c2 = Spades)
    |> RandomVariable.probability
    |> shouldEqual (10N/47N*9N/46N)

#endif
