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
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using Gallio.Model;
using MbUnit.Framework;

namespace MbUnit.Samples
{
    [TestFixture]
    public class OutcomeDemo
    {
        [Test]
        public void Passed()
        {
        }

        [Test]
        public void Failed()
        {
            Assert.Fail("Failed for demonstration purposes.");
        }

        [Test]
        public void Inconclusive()
        {
            Assert.Inconclusive("Inconclusive for demonstration purposes.");
        }

        [Test, Ignore("Skipped")]
        public void Skipped()
        {
        }

        [Test]
        public void Terminate()
        {
            Assert.Terminate(TestOutcome.Error, "Terminated for demonstration purposes.");
        }

        [Test]
        [Row(TestStatus.Passed, null)]
        [Row(TestStatus.Failed, null)]
        [Row(TestStatus.Inconclusive, null)]
        [Row(TestStatus.Skipped, null)]
        [Row(TestStatus.Failed, "error")]
        [Row(TestStatus.Skipped, "ignored")]
        public void RowsWithDifferentOutcomes(TestStatus status, string category)
        {
            Assert.TerminateSilently(new TestOutcome(status, category), "Terminated for demonstration purposes.");
        }
    }
}
