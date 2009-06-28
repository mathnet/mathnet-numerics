// Copyright 2005-2009 Gallio Project - http://www.gallio.org/
// Portions Copyright 2000-2004 Jonathan de Halleux
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using System.Runtime.Serialization;

namespace MbUnit.Samples.ContractVerifiers.Immutability
{
    public class SampleImmutable
    {
        private readonly int number;
        private readonly string text;
        private readonly ImmutableFoo foo;

        public SampleImmutable(int number, string text, ImmutableFoo foo)
        {
            this.number = number;
            this.text = text;
            this.foo = foo;
        }
    }

    public class ImmutableFoo
    {
        private readonly double number;

        public ImmutableFoo(double number)
        {
            this.number = number;
        }
    }
}
