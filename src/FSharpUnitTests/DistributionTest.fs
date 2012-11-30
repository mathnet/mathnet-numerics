module MathNet.Numerics.Tests.DistributionTest

open MathNet.Numerics
open MathNet.Numerics.Distribution
open NUnit.Framework
open FsUnit

[<Test>]
let ``When creating a empty distribution, then the probability should be 1``() =
  let actual = distribution { return () }
  probability actual |> should equal (1N/1N)

let sumOfTwoFairDices = distribution {
  let! d1 = fairDice 6
  let! d2 = fairDice 6
  return d1 + d2 }

[<Test>]
let ``When creating two fair dices, then P(Sum of 2 dices = 7) should be 1/6``() =
  sumOfTwoFairDices 
    |> filter ((=) 7)
    |> probability 
    |> should equal (1N/6N)

let fairCoinAndDice = distribution {
  let! d = fairDice 6
  let! c = fairCoin
  return d,c }

[<Test>]
let ``When creating a fair coin and a fair dice, then P(Heads) should be 1/2``() =
  fairCoinAndDice 
    |> filter (fun (_,c) -> c = Heads)
    |> probability 
    |> should equal (1N/2N)

[<Test>]
let ``When creating a fair coin and a fair dice, then P(Heads and dice > 3) should be 1/4``() =
  fairCoinAndDice 
    |> filter (fun (d,c) -> c = Heads && d > 3)
    |> probability 
    |> should equal (1N/4N)

// MontyHall Problem
// See Martin Erwig and Steve Kollmansberger's paper 
// "Functional Pearls: Probabilistic functional programming in Haskell"

type Outcome = 
| Car
| Goat

let firstChoice = toUniformDistribution [Car; Goat; Goat]

let switch firstCoice =
    match firstCoice with
    | Car -> 
        // If you had the car and you switch ==> you lose since there are only goats left
        certainly Goat 
    | Goat -> 
        // If you had the goat, the host has to take out another goat ==> you win
        certainly Car 
 
[<Test>]
let ``When making the first choice in a MontyHall situation, the chances to win should be 1/3``() =
  firstChoice 
    |> filter ((=) Car)
    |> probability 
    |> should equal (1N/3N)

let montyHallWithSwitch = distribution {
    let! firstDoor = firstChoice
    return! switch firstDoor }

[<Test>]
let ``When switching in a MontyHall situation, the chances to win should be 2/3``() =
  montyHallWithSwitch
    |> filter ((=) Car)
    |> probability 
    |> should equal (2N/3N)