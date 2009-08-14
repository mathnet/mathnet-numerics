// <copyright file="ArgumentCheckContract.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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
    using MbUnit.Framework;
    using MbUnit.Framework.ContractVerifiers;

    internal class ArgumentCheckContract<T> : AbstractContract
    {
        public Expression<Func<T>> TypicalUse { get; set; }
        public Expression<Func<T>>[] TypicalUses { get; set; }
        public Func<T> BadUse { get; set; }
        public Func<T>[] BadUses { get; set; }

        protected override IEnumerable<Test> GetContractVerificationTests()
        {
            if (TypicalUse != null)
            {
                yield return CreateNullArgumentCheckTest(TypicalUse);
            }

            if (TypicalUses != null)
            {
                foreach (var typicalUse in TypicalUses)
                {
                    yield return CreateNullArgumentCheckTest(typicalUse);
                }
            }

            if (BadUse != null)
            {
                yield return CreateBadArgumentCheckTest(BadUse);
            }

            if (BadUses != null)
            {
                foreach (var badUse in BadUses)
                {
                    yield return CreateBadArgumentCheckTest(badUse);
                }
            }
        }

        private static Test CreateNullArgumentCheckTest(Expression<Func<T>> typicalUse)
        {
            return new TestCase(
                "NullArgumentCheck " + typicalUse.Body,
                () =>
                {
                    Assert.DoesNotThrow(() => typicalUse.Compile()());

                    // we only support method calls and constructors for now.
                    if (typicalUse.Body.NodeType != ExpressionType.New
                        && typicalUse.Body.NodeType != ExpressionType.Call)
                    {
                        Assert.Fail(
                            "Subject '{0}' is neither a constructor or a method call.",
                            typicalUse.ToString());
                        return;
                    }

                    var badUses = typicalUse.ApplySingleMapEachArgument<T>(
                        t => !t.IsValueType,
                        e => Expression.Constant(null, e.Type));

                    foreach (var badUse in badUses)
                    {
                        var closureBadUseReference = badUse;

                        Assert.Throws(
                            typeof (ArgumentNullException),
                            () => closureBadUseReference.Compile()(),
                            "Subject '{0}' must check for null arguments",
                            closureBadUseReference.ToString());
                    }
                });
        }

        private static Test CreateBadArgumentCheckTest(Func<T> badUse)
        {
            return new TestCase(
                "BadArgumentCheck " + badUse.Method,
                () => Assert.Throws(
                          typeof (ArgumentException),
                          () => badUse(),
                          "Subject '{0}' must check for bad arguments",
                          badUse.ToString()));
        }
    }
}
