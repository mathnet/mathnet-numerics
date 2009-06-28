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
using System.Collections;
using System.Collections.Generic;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;

namespace MbUnit.Samples.ContractVerifiers.Collection
{
    public class Foo
    {
        private readonly int value;

        public Foo (int value)
	    {
            this.value = value;
	    }
    }

    public class SampleCollection : ICollection<Foo>
    {
        private readonly List<Foo> items = new List<Foo>();

        public SampleCollection()
        {
        }

        public SampleCollection(int initialCount)
        {
            for(int i=0; i<initialCount; i++)
            {
                Add(new Foo(i));
            }
        }

        public void Add(Foo item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            if (items.Contains(item))
            {
                throw new ArgumentException("The collection already contains the specified item.");
            }

            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(Foo item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return items.Contains(item);
        }

        public void CopyTo(Foo[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return items.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(Foo item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return items.Remove(item);
        }

        public IEnumerator<Foo> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in items)
            {
                yield return item;
            }
        }
    }
}
