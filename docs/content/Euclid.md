Euclid & Number Theory
======================

The static `Euclid` class in the `MathNet.Numerics` namespace provides routines related
to the domain of integers.


Remainder vs. Canonical Modulus
-------------------------------

Remainder and modulus are closely related operations with a long tradition of confusing 
on with the other. The % operator in most computer languages implements one of the two,
but some even leave which one as an implementation detail (e.g. C-1990).

*Warning: In C#, like most languages, % is the remainder operator, not the modulus!*


#### Remainder

The **remainder** is the amount left over after performing the division of a dividend
by a divisor, $\frac{dividend}{divisor}$, which do not divide evenly, that is,
where the result of the division cannot be expressed as an integer. It is thus natural
that the **remainder has the sign of the dividend**.

In C# and F#, the remainder is available as `%` operator, in VB as `Mod`.
Alternatively you can use the Reminder function:

    [lang=csharp]
    Euclid.Remainder( 5,  3); // =  2, such that 5 = 1*3 + 2
    Euclid.Remainder(-5,  3); // = -2, such that -5 = -1*3 - 2
    Euclid.Remainder( 5, -3); // =  2, such that 5 = -1*-3 + 2
    Euclid.Remainder(-5, -3); // = -2, such that -5 = 1*-3 - 2


#### Modulus

On the other hand, in modular arithmetic numbers "wrap around" upon reaching a certain
value n, or when crossing zero. Two real numbers are said to be *congruent modulo n*
when their difference is an integer multiple of n. The modulo operator normalizes the dividend
to the fundamental or smallest values congruent modulo n, where n is the divisor, and thus
to the interval from 0 to n (including 0 but excluding n, possibly negative). It is thus natural that
the **modulus always has the sign of the divisor**.

    [lang=csharp]
    Euclid.Modulus( 5,  3); // =  2, congruent modulo 3 by 5 - 1*3
    Euclid.Modulus(-5,  3); // =  1, congruent modulo 3 by -5 + 2*3
    Euclid.Modulus( 5, -3); // = -1, congruent modulo -3 by 5 + 2*-3
    Euclid.Modulus(-5, -3); // = -2, congruent modulo -3 by -5 - 1*-3

A typical case where the modulus appears in daily life is when grouping students into 3 groups
by letting them line up and count through as 0 1 2 0 1 2 0 1 2 etc. This way, each student will
end up in the group of their order within the line modulus 3.


Integer Properties
------------------

#### Even or Odd?

Very simple question yet still somewhat error-prone to implement such that it works correctly
for both positive and negative integers: is a number even or odd?

* `IsEven(number)`
* `IsOdd(number)`


#### Powers of two and Squares

Powers of two are prevalent in computer engineering. For performance reasons it is often preferable
to align data in blocks where the size is a power of two, i.e. $2^k$. The `CeilingToPowerOfTwo` function
helps in such situations by finding the smallest perfect power of two larger than or equal to
the provided argument. There is also `IsPowerOfTwo` to determine whether a number is such a power of two,
and `PowerOfTwo` to compute it efficiently.

When switching the operands of $2^k$ we get the square $k^2$. `IsPerfectSquare` determines whether
the integer argument is a perfect square, i.e. a square of an integer.


Euclid's Algorithm
------------------

#### Greatest Common Divisor

The `GreatestCommonDivisor` evaluates the **GCD** of either two integers or a full list or array of them
using Euclid's algorithm. An extended version also returns how exactly the GCD can be composed from two
integer arguments.

    [lang=csharp]
    Euclid.GreatestCommonDivisor(10, 15, 45); // 5

    long x, y;
    Euclid.ExtendedGreatestCommonDivisor(45, 18, out x, out y) // 9
    // -> x=1, y=-2, hence 9 == 1*45 + -2*18


#### Least Common Multiple

Closely related to the GCD, `LeastCommonMultiple` returns the **LCM** of two or more integers.

    [lang=csharp]
    Euclid.LeastCommonMultiple(3, 5, 6); // 30
