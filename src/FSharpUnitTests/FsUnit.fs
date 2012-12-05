module FsUnit
open NUnit.Framework
open NUnit.Framework.Constraints

let should (f : 'a -> #Constraint) x (y : obj) =
    let c = f x
    let y =
        match y with
        | :? (unit -> unit) -> box (TestDelegate(y :?> unit -> unit))
        | _ -> y
    Assert.That(y, c)

let equal x = EqualConstraint(x)

let equalWithin tolerance x = equal(x).Within tolerance

let contain x = ContainsConstraint(x)

let haveLength n = Has.Length.EqualTo(n)

let haveCount n = Has.Count.EqualTo(n)

let be = id

let Null = NullConstraint()

let Empty = EmptyConstraint()

let EmptyString = EmptyStringConstraint()

let NullOrEmptyString = NullOrEmptyStringConstraint()

let True = TrueConstraint()

let False = FalseConstraint()

let sameAs x = SameAsConstraint(x)

let throw = Throws.TypeOf

let greaterThan x = GreaterThanConstraint(x)

let greaterThanOrEqualTo x = GreaterThanOrEqualConstraint(x)

let lessThan x = LessThanConstraint(x)

let lessThanOrEqualTo x = LessThanOrEqualConstraint(x)

let shouldFail (f : unit -> unit) =
    TestDelegate(f) |> should throw typeof<AssertionException>

let endWith (s:string) = EndsWithConstraint s

let startWith (s:string) = StartsWithConstraint s

let ofExactType<'a> = ExactTypeConstraint(typeof<'a>)

let instanceOfType<'a> = InstanceOfTypeConstraint(typeof<'a>)

let NaN = NaNConstraint()

let unique = UniqueItemsConstraint()

let not' x = NotConstraint(x)

let array_equal = equal
let array2_equal = equal
let approximately_equal tolerance = equalWithin (10.0 ** (float -tolerance))
let approximately_vector_equal = approximately_equal
let approximately_matrix_equal = approximately_equal
