module MathNet.Numerics.Tests.RandomVariableTests

#if NOSYSNUMERICS
#else

open NUnit.Framework
open FsUnitTyped

open MathNet.Numerics
open MathNet.Numerics.Probability

[<Test>]
let ``When creating a empty randomVariable, then the probability should be 1``() =
  let actual = randomVariable { return () }
  RandomVariable.probability actual |> shouldEqual (1N/1N)

let sumOfTwoFairDices = randomVariable {
  let! d1 = RandomVariable.fairDice 6
  let! d2 = RandomVariable.fairDice 6
  return d1 + d2 }

[<Test>]
let ``When creating two fair dices, then P(Sum of 2 dices = 7) should be 1/6``() =
  sumOfTwoFairDices
    |> RandomVariable.filter ((=) 7)
    |> RandomVariable.probability
    |> shouldEqual (1N/6N)

let fairCoinAndDice = randomVariable {
  let! d = RandomVariable.fairDice 6
  let! c = RandomVariable.fairCoin
  return d,c }

[<Test>]
let ``When creating a fair coin and a fair dice, then P(Heads) should be 1/2``() =
  fairCoinAndDice
    |> RandomVariable.filter (fun (_,c) -> c = Heads)
    |> RandomVariable.probability
    |> shouldEqual (1N/2N)

[<Test>]
let ``When creating a fair coin and a fair dice, then P(Heads and dice > 3) should be 1/4``() =
  fairCoinAndDice
    |> RandomVariable.filter (fun (d,c) -> c = Heads && d > 3)
    |> RandomVariable.probability
    |> shouldEqual (1N/4N)

// MontyHall Problem
// See Martin Erwig and Steve Kollmansberger's paper
// "Functional Pearls: Probabilistic functional programming in Haskell"

type Outcome =
| Car
| Goat

let firstChoice = RandomVariable.toUniformDistribution [Car; Goat; Goat]

let switch firstCoice =
    match firstCoice with
    | Car ->
        // If you had the car and you switch ==> you lose since there are only goats left
        RandomVariable.certainly Goat
    | Goat ->
        // If you had the goat, the host has to take out another goat ==> you win
        RandomVariable.certainly Car

[<Test>]
let ``When making the first choice in a MontyHall situation, the chances to win should be 1/3``() =
  firstChoice
    |> RandomVariable.filter ((=) Car)
    |> RandomVariable.probability
    |> shouldEqual (1N/3N)

let montyHallWithSwitch = randomVariable {
    let! firstDoor = firstChoice
    return! switch firstDoor }

[<Test>]
let ``When switching in a MontyHall situation, the chances to win should be 2/3``() =
  montyHallWithSwitch
    |> RandomVariable.filter ((=) Car)
    |> RandomVariable.probability
    |> shouldEqual (2N/3N)

#endif
