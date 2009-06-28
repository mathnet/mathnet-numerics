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

namespace MbUnit.Samples.ContractVerifiers.Collection
{
    public class SampleCollectionTest
    {
        [VerifyContract]
        public readonly IContract CollectionTests = new CollectionContract<SampleCollection, Foo>
        {
            // Do not accept equal items in the collection.
            // Optional; default is true.
            AcceptEqualItems = false,

            // Do consider null references as illegal items.
            // Optional; default is false.
            AcceptNullReference = false,

            // Indicates whether the collection is supposed to be read-only.
            // Optional; default is false.
            IsReadOnly = false,

            // Specify some default instance of the collection for the contract verifier.
            // Optional, default is based on the invocation of the default constructor.
            // Overwrite the default value if the collection has no default constructor, 
            // or if you want the contract verifier to work with some particular instance.
            GetDefaultInstance = () => new SampleCollection(3),

            // Provide some valid item instances to feed the collection.
            // Better is to provide items already in the default instance of collection 
            // specified above, and items not present initially.
            DistinctInstances =
            {
                new Foo(0),
                new Foo(1),
                new Foo(2),
                new Foo(3),
                new Foo(4),
                new Foo(5),
           }
        };
    }
}
