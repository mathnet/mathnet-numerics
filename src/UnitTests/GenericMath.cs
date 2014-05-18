// TAKEN FROM:
// Miscellaneous Utility Library
// http://www.yoda.arachsys.com/csharp/miscutil/
//
// "Miscellaneous Utility Library" Software Licence
//
// Version 1.0
//
// Copyright (c) 2004-2008 Jon Skeet and Marc Gravell.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// 1. Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//
// 2. Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
//
// 3. The end-user documentation included with the redistribution, if
// any, must include the following acknowledgment:
//
// "This product includes software developed by Jon Skeet
// and Marc Gravell. Contact skeet@pobox.com, or see
// http://www.pobox.com/~skeet/)."
//
// Alternately, this acknowledgment may appear in the software itself,
// if and wherever such third-party acknowledgments normally appear.
//
// 4. The name "Miscellaneous Utility Library" must not be used to endorse
// or promote products derived from this software without prior written
// permission. For written permission, please contact skeet@pobox.com.
//
// 5. Products derived from this software may not be called
// "Miscellaneous Utility Library", nor may "Miscellaneous Utility Library"
// appear in their name, without prior written permission of Jon Skeet.
//
// THIS SOFTWARE IS PROVIDED "AS IS" AND ANY EXPRESSED OR IMPLIED
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
// IN NO EVENT SHALL JON SKEET BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
// BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Linq.Expressions;

namespace MathNet.Numerics.UnitTests
{
    /// <summary>
    /// The Operator class provides easy access to the standard operators
    /// (addition, etc) for generic types, using type inference to simplify
    /// usage.
    /// </summary>
    internal static class Operator
    {

        /// <summary>
        /// Indicates if the supplied value is non-null,
        /// for reference-types or Nullable&lt;T&gt;
        /// </summary>
        /// <returns>True for non-null values, else false</returns>
        public static bool HasValue<T>(T value)
        {
            return Operator<T>.NullOp.HasValue(value);
        }

        /// <summary>
        /// Increments the accumulator only
        /// if the value is non-null. If the accumulator
        /// is null, then the accumulator is given the new
        /// value; otherwise the accumulator and value
        /// are added.
        /// </summary>
        /// <param name="accumulator">The current total to be incremented (can be null)</param>
        /// <param name="value">The value to be tested and added to the accumulator</param>
        /// <returns>True if the value is non-null, else false - i.e.
        /// "has the accumulator been updated?"</returns>
        public static bool AddIfNotNull<T>(ref T accumulator, T value)
        {
            return Operator<T>.NullOp.AddIfNotNull(ref accumulator, value);
        }

        /// <summary>
        /// Evaluates unary negation (-) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static T Negate<T>(T value)
        {
            return Operator<T>.Negate(value);
        }

        /// <summary>
        /// Evaluates bitwise not (~) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static T Not<T>(T value)
        {
            return Operator<T>.Not(value);
        }

        /// <summary>
        /// Evaluates bitwise or (|) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static T Or<T>(T value1, T value2)
        {
            return Operator<T>.Or(value1, value2);
        }

        /// <summary>
        /// Evaluates bitwise and (&amp;) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static T And<T>(T value1, T value2)
        {
            return Operator<T>.And(value1, value2);
        }

        /// <summary>
        /// Evaluates bitwise xor (^) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static T Xor<T>(T value1, T value2)
        {
            return Operator<T>.Xor(value1, value2);
        }

        /// <summary>
        /// Performs a conversion between the given types; this will throw
        /// an InvalidOperationException if the type T does not provide a suitable cast, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this cast.
        /// </summary>
        public static TTo Convert<TFrom, TTo>(TFrom value)
        {
            return Operator<TFrom, TTo>.Convert(value);
        }

        /// <summary>
        /// Evaluates binary addition (+) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static T Add<T>(T value1, T value2)
        {
            return Operator<T>.Add(value1, value2);
        }

        /// <summary>
        /// Evaluates binary addition (+) for the given type(s); this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static TArg1 AddAlternative<TArg1, TArg2>(TArg1 value1, TArg2 value2)
        {
            return Operator<TArg2, TArg1>.Add(value1, value2);
        }

        /// <summary>
        /// Evaluates binary subtraction (-) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static T Subtract<T>(T value1, T value2)
        {
            return Operator<T>.Subtract(value1, value2);
        }

        /// <summary>
        /// Evaluates binary subtraction(-) for the given type(s); this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static TArg1 SubtractAlternative<TArg1, TArg2>(TArg1 value1, TArg2 value2)
        {
            return Operator<TArg2, TArg1>.Subtract(value1, value2);
        }

        /// <summary>
        /// Evaluates binary multiplication (*) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static T Multiply<T>(T value1, T value2)
        {
            return Operator<T>.Multiply(value1, value2);
        }

        /// <summary>
        /// Evaluates binary multiplication (*) for the given type(s); this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static TArg1 MultiplyAlternative<TArg1, TArg2>(TArg1 value1, TArg2 value2)
        {
            return Operator<TArg2, TArg1>.Multiply(value1, value2);
        }

        /// <summary>
        /// Evaluates binary division (/) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static T Divide<T>(T value1, T value2)
        {
            return Operator<T>.Divide(value1, value2);
        }

        /// <summary>
        /// Evaluates binary division (/) for the given type(s); this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static TArg1 DivideAlternative<TArg1, TArg2>(TArg1 value1, TArg2 value2)
        {
            return Operator<TArg2, TArg1>.Divide(value1, value2);
        }

        /// <summary>
        /// Evaluates binary equality (==) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static bool Equal<T>(T value1, T value2)
        {
            return Operator<T>.Equal(value1, value2);
        }

        /// <summary>
        /// Evaluates binary inequality (!=) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static bool NotEqual<T>(T value1, T value2)
        {
            return Operator<T>.NotEqual(value1, value2);
        }

        /// <summary>
        /// Evaluates binary greater-than (&gt;) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static bool GreaterThan<T>(T value1, T value2)
        {
            return Operator<T>.GreaterThan(value1, value2);
        }

        /// <summary>
        /// Evaluates binary less-than (&lt;) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static bool LessThan<T>(T value1, T value2)
        {
            return Operator<T>.LessThan(value1, value2);
        }

        /// <summary>
        /// Evaluates binary greater-than-on-eqauls (&gt;=) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static bool GreaterThanOrEqual<T>(T value1, T value2)
        {
            return Operator<T>.GreaterThanOrEqual(value1, value2);
        }

        /// <summary>
        /// Evaluates binary less-than-or-equal (&lt;=) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static bool LessThanOrEqual<T>(T value1, T value2)
        {
            return Operator<T>.LessThanOrEqual(value1, value2);
        }

        /// <summary>
        /// Evaluates integer division (/) for the given type; this will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary><remarks>
        /// This operation is particularly useful for computing averages and
        /// similar aggregates.
        /// </remarks>
        public static T DivideInt32<T>(T value, int divisor)
        {
            return Operator<int, T>.Divide(value, divisor);
        }
    }

    /// <summary>
    /// Provides standard operators (such as addition) that operate over operands of
    /// different types. For operators, the return type is assumed to match the first
    /// operand.
    /// </summary>
    /// <seealso cref="Operator&lt;T&gt;"/>
    /// <seealso cref="Operator"/>
    internal static class Operator<TValue, TResult>
    {
        static readonly Func<TValue, TResult> convert;

        /// <summary>
        /// Returns a delegate to convert a value between two types; this delegate will throw
        /// an InvalidOperationException if the type T does not provide a suitable cast, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this cast.
        /// </summary>
        public static Func<TValue, TResult> Convert
        {
            get { return convert; }
        }

        static Operator()
        {
            convert = ExpressionUtil.CreateExpression<TValue, TResult>(body => Expression.Convert(body, typeof (TResult)));
            add = ExpressionUtil.CreateExpression<TResult, TValue, TResult>(Expression.Add, true);
            subtract = ExpressionUtil.CreateExpression<TResult, TValue, TResult>(Expression.Subtract, true);
            multiply = ExpressionUtil.CreateExpression<TResult, TValue, TResult>(Expression.Multiply, true);
            divide = ExpressionUtil.CreateExpression<TResult, TValue, TResult>(Expression.Divide, true);
        }

        static readonly Func<TResult, TValue, TResult> add, subtract, multiply, divide;

        /// <summary>
        /// Returns a delegate to evaluate binary addition (+) for the given types; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<TResult, TValue, TResult> Add
        {
            get { return add; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary subtraction (-) for the given types; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<TResult, TValue, TResult> Subtract
        {
            get { return subtract; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary multiplication (*) for the given types; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<TResult, TValue, TResult> Multiply
        {
            get { return multiply; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary division (/) for the given types; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<TResult, TValue, TResult> Divide
        {
            get { return divide; }
        }
    }

    /// <summary>
    /// Provides standard operators (such as addition) over a single type
    /// </summary>
    /// <seealso cref="Operator"/>
    /// <seealso cref="Operator&lt;TValue,TResult&gt;"/>
    internal static class Operator<T>
    {
        static readonly INullOp<T> nullOp;

        internal static INullOp<T> NullOp
        {
            get { return nullOp; }
        }

        static readonly T zero;

        /// <summary>
        /// Returns the zero value for value-types (even full Nullable&lt;TInner&gt;) - or null for reference types
        /// </summary>
        public static T Zero
        {
            get { return zero; }
        }

        static readonly Func<T, T> negate, not;
        static readonly Func<T, T, T> or, and, xor;

        /// <summary>
        /// Returns a delegate to evaluate unary negation (-) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T> Negate
        {
            get { return negate; }
        }

        /// <summary>
        /// Returns a delegate to evaluate bitwise not (~) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T> Not
        {
            get { return not; }
        }

        /// <summary>
        /// Returns a delegate to evaluate bitwise or (|) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, T> Or
        {
            get { return or; }
        }

        /// <summary>
        /// Returns a delegate to evaluate bitwise and (&amp;) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, T> And
        {
            get { return and; }
        }

        /// <summary>
        /// Returns a delegate to evaluate bitwise xor (^) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, T> Xor
        {
            get { return xor; }
        }

        static readonly Func<T, T, T> add, subtract, multiply, divide;

        /// <summary>
        /// Returns a delegate to evaluate binary addition (+) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, T> Add
        {
            get { return add; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary subtraction (-) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, T> Subtract
        {
            get { return subtract; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary multiplication (*) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, T> Multiply
        {
            get { return multiply; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary division (/) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, T> Divide
        {
            get { return divide; }
        }


        static readonly Func<T, T, bool> equal, notEqual, greaterThan, lessThan, greaterThanOrEqual, lessThanOrEqual;

        /// <summary>
        /// Returns a delegate to evaluate binary equality (==) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, bool> Equal
        {
            get { return equal; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary inequality (!=) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, bool> NotEqual
        {
            get { return notEqual; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary greater-then (&gt;) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, bool> GreaterThan
        {
            get { return greaterThan; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary less-than (&lt;) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, bool> LessThan
        {
            get { return lessThan; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary greater-than-or-equal (&gt;=) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, bool> GreaterThanOrEqual
        {
            get { return greaterThanOrEqual; }
        }

        /// <summary>
        /// Returns a delegate to evaluate binary less-than-or-equal (&lt;=) for the given type; this delegate will throw
        /// an InvalidOperationException if the type T does not provide this operator, or for
        /// Nullable&lt;TInner&gt; if TInner does not provide this operator.
        /// </summary>
        public static Func<T, T, bool> LessThanOrEqual
        {
            get { return lessThanOrEqual; }
        }

        static Operator()
        {
            add = ExpressionUtil.CreateExpression<T, T, T>(Expression.Add);
            subtract = ExpressionUtil.CreateExpression<T, T, T>(Expression.Subtract);
            divide = ExpressionUtil.CreateExpression<T, T, T>(Expression.Divide);
            multiply = ExpressionUtil.CreateExpression<T, T, T>(Expression.Multiply);

            greaterThan = ExpressionUtil.CreateExpression<T, T, bool>(Expression.GreaterThan);
            greaterThanOrEqual = ExpressionUtil.CreateExpression<T, T, bool>(Expression.GreaterThanOrEqual);
            lessThan = ExpressionUtil.CreateExpression<T, T, bool>(Expression.LessThan);
            lessThanOrEqual = ExpressionUtil.CreateExpression<T, T, bool>(Expression.LessThanOrEqual);
            equal = ExpressionUtil.CreateExpression<T, T, bool>(Expression.Equal);
            notEqual = ExpressionUtil.CreateExpression<T, T, bool>(Expression.NotEqual);

            negate = ExpressionUtil.CreateExpression<T, T>(Expression.Negate);
            and = ExpressionUtil.CreateExpression<T, T, T>(Expression.And);
            or = ExpressionUtil.CreateExpression<T, T, T>(Expression.Or);
            not = ExpressionUtil.CreateExpression<T, T>(Expression.Not);
            xor = ExpressionUtil.CreateExpression<T, T, T>(Expression.ExclusiveOr);

            Type typeT = typeof (T);
            if (typeT.IsValueType && typeT.IsGenericType && (typeT.GetGenericTypeDefinition() == typeof (Nullable<>)))
            {
                // get the *inner* zero (not a null Nullable<TValue>, but default(TValue))
                Type nullType = typeT.GetGenericArguments()[0];
                zero = (T)Activator.CreateInstance(nullType);
                nullOp = (INullOp<T>)Activator.CreateInstance(
                    typeof (StructNullOp<>).MakeGenericType(nullType));
            }
            else
            {
                zero = default(T);
                if (typeT.IsValueType)
                {
                    nullOp = (INullOp<T>)Activator.CreateInstance(
                        typeof (StructNullOp<>).MakeGenericType(typeT));
                }
                else
                {
                    nullOp = (INullOp<T>)Activator.CreateInstance(
                        typeof (ClassNullOp<>).MakeGenericType(typeT));
                }
            }
        }
    }

    /// <summary>
    /// General purpose Expression utilities
    /// </summary>
    internal static class ExpressionUtil
    {
        /// <summary>
        /// Create a function delegate representing a unary operation
        /// </summary>
        /// <typeparam name="TArg1">The parameter type</typeparam>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <param name="body">Body factory</param>
        /// <returns>Compiled function delegate</returns>
        public static Func<TArg1, TResult> CreateExpression<TArg1, TResult>(
            Func<Expression, UnaryExpression> body)
        {
            ParameterExpression inp = Expression.Parameter(typeof (TArg1), "inp");
            try
            {
                return Expression.Lambda<Func<TArg1, TResult>>(body(inp), inp).Compile();
            }
            catch (Exception ex)
            {
                string msg = ex.Message; // avoid capture of ex itself
                return delegate { throw new InvalidOperationException(msg); };
            }
        }

        /// <summary>
        /// Create a function delegate representing a binary operation
        /// </summary>
        /// <typeparam name="TArg1">The first parameter type</typeparam>
        /// <typeparam name="TArg2">The second parameter type</typeparam>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <param name="body">Body factory</param>
        /// <returns>Compiled function delegate</returns>
        public static Func<TArg1, TArg2, TResult> CreateExpression<TArg1, TArg2, TResult>(
            Func<Expression, Expression, BinaryExpression> body)
        {
            return CreateExpression<TArg1, TArg2, TResult>(body, false);
        }

        /// <summary>
        /// Create a function delegate representing a binary operation
        /// </summary>
        /// <param name="castArgsToResultOnFailure">
        /// If no matching operation is possible, attempt to convert
        /// TArg1 and TArg2 to TResult for a match? For example, there is no
        /// "decimal operator /(decimal, int)", but by converting TArg2 (int) to
        /// TResult (decimal) a match is found.
        /// </param>
        /// <typeparam name="TArg1">The first parameter type</typeparam>
        /// <typeparam name="TArg2">The second parameter type</typeparam>
        /// <typeparam name="TResult">The return type</typeparam>
        /// <param name="body">Body factory</param>
        /// <returns>Compiled function delegate</returns>
        public static Func<TArg1, TArg2, TResult> CreateExpression<TArg1, TArg2, TResult>(
            Func<Expression, Expression, BinaryExpression> body, bool castArgsToResultOnFailure)
        {
            ParameterExpression lhs = Expression.Parameter(typeof (TArg1), "lhs");
            ParameterExpression rhs = Expression.Parameter(typeof (TArg2), "rhs");
            try
            {
                try
                {
                    return Expression.Lambda<Func<TArg1, TArg2, TResult>>(body(lhs, rhs), lhs, rhs).Compile();
                }
                catch (InvalidOperationException)
                {
                    if (castArgsToResultOnFailure && !( // if we show retry
                        typeof (TArg1) == typeof (TResult) && // and the args aren't
                        typeof (TArg2) == typeof (TResult)))
                    {
                        // already "TValue, TValue, TValue"...
                        // convert both lhs and rhs to TResult (as appropriate)
                        Expression castLhs = typeof (TArg1) == typeof (TResult) ? (Expression)lhs : Expression.Convert(lhs, typeof (TResult));
                        Expression castRhs = typeof (TArg2) == typeof (TResult) ? (Expression)rhs : Expression.Convert(rhs, typeof (TResult));

                        return Expression.Lambda<Func<TArg1, TArg2, TResult>>(
                            body(castLhs, castRhs), lhs, rhs).Compile();
                    }

                    throw;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message; // avoid capture of ex itself
                return delegate { throw new InvalidOperationException(msg); };
            }
        }
    }

    internal interface INullOp<T>
    {
        bool HasValue(T value);
        bool AddIfNotNull(ref T accumulator, T value);
    }

    internal sealed class StructNullOp<T>
        : INullOp<T>, INullOp<T?>
        where T : struct
    {
        public bool HasValue(T value)
        {
            return true;
        }

        public bool AddIfNotNull(ref T accumulator, T value)
        {
            accumulator = Operator<T>.Add(accumulator, value);
            return true;
        }

        public bool HasValue(T? value)
        {
            return value.HasValue;
        }

        public bool AddIfNotNull(ref T? accumulator, T? value)
        {
            if (value.HasValue)
            {
                accumulator = accumulator.HasValue ?
                    Operator<T>.Add(
                        accumulator.GetValueOrDefault(),
                        value.GetValueOrDefault())
                    : value;
                return true;
            }
            return false;
        }
    }

    internal sealed class ClassNullOp<T>
        : INullOp<T>
        where T : class
    {
        public bool HasValue(T value)
        {
            return value != null;
        }

        public bool AddIfNotNull(ref T accumulator, T value)
        {
            if (value != null)
            {
                accumulator = accumulator == null ?
                    value : Operator<T>.Add(accumulator, value);
                return true;
            }
            return false;
        }
    }
}
