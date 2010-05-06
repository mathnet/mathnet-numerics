// <copyright file="FunctionalHelpers.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    internal static class FunctionalHelpers
    {
        internal static Expression[] ApplySingleMap(
            IList<Expression> list,
            int index,
            Func<Expression, Expression> replace)
        {
            var newList = new Expression[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                newList[i] = (i == index) ? replace(list[i]) : list[i];
            }

            return newList;
        }

        internal static IEnumerable<Expression<Func<T>>> ApplySingleMapEachArgument<T>(
            this LambdaExpression lambda,
            Func<Type, bool> predicate,
            Func<Expression, Expression> replace)
        {
            var body = lambda.Body;
            var bodyCall = body as MethodCallExpression;
            var bodyCtor = body as NewExpression;

            if (bodyCall == null && bodyCtor == null)
            {
                throw new NotSupportedException();
            }

            var arguments =
                bodyCall != null
                    ? bodyCall.Arguments
                    : bodyCtor.Arguments;

            for (int i = 0; i < arguments.Count; i++)
            {
                if (!predicate(arguments[i].Type))
                {
                    continue;
                }

                var mappedArguments = ApplySingleMap(arguments, i, replace);

                yield return Expression.Lambda<Func<T>>(
                    bodyCtor != null
                        ? (Expression) Expression.New(
                                           bodyCtor.Constructor,
                                           mappedArguments)
                        : (Expression) Expression.Call(
                                           bodyCall.Object,
                                           bodyCall.Method,
                                           mappedArguments));
            }
        }

        internal static T ApplyReduceArgument<T>(
            this LambdaExpression lambda,
            Func<Expression, T, T> reduce,
            T init)
        {
            var body = lambda.Body;
            var arguments =
                body is NewExpression
                    ? ((NewExpression)body).Arguments
                    : ((MethodCallExpression)body).Arguments;

            T value = init;
            foreach (var argument in arguments)
            {
                value = reduce(argument, value);
            }

            return value;
        }
    }
}
