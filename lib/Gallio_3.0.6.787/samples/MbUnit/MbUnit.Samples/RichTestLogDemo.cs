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
using System.Diagnostics;
using Gallio.Framework;
using Gallio.Framework.Assertions;
using Gallio.Model.Logging;
using MbUnit.Framework;
using Gallio.Model;
using MbUnit.Samples.Properties;

namespace MbUnit.Samples
{
    /// <summary>
    /// Generates a test log with all sorts of rich content.
    /// </summary>
    [TestFixture]
    [Description("Demonstrate the rich test logging features.")]
    public class RichTestLogDemo
    {
        private static TestLogStreamWriter MbUnitRocks
        {
            get { return TestLog.Writer["MbUnit Rocks"]; }
        }

        /// <summary>
        /// Demonstrate all of the basic logging features.
        /// </summary>
        [Test]
        [Description("Shows off MbUnit execution logs.")]
        [Metadata("Gimmick", "We don't need any gimmicks!")]
        public void FeatureDemo()
        {
            MbUnitRocks.WriteLine("MbUnit Rocks!");
            MbUnitRocks.WriteLine();

            MbUnitRocks.WriteLine("You can embed images.");
            MbUnitRocks.EmbedImage("MbUnit Logo", Resources.MbUnitLogo);
            MbUnitRocks.WriteLine();

            MbUnitRocks.WriteLine("You can write out your own log streams.");

            using (MbUnitRocks.BeginSection("My section!"))
            {
                MbUnitRocks.WriteLine("And break them into sections.");

                using (MbUnitRocks.BeginSection("And"))
                using (MbUnitRocks.BeginSection("Even"))
                using (MbUnitRocks.BeginSection("Nest"))
                {
                    using (MbUnitRocks.BeginSection("Them"))
                    {
                        MbUnitRocks.WriteLine("So you can see what's happening.");
                        MbUnitRocks.WriteLine("Just in here.");
                    }

                    MbUnitRocks.WriteLine("Or here...");
                }
            }

            TestStep.RunStep("Lemmings!", delegate
            {
                MbUnitRocks.WriteLine("You can subdivide a test into nested steps.");
                MbUnitRocks.WriteLine("And run them repeatedly with independent failure conditions.");

                for (int i = 5; i > 0; i--)
                {
                    TestStep.RunStep(i.ToString(), delegate { MbUnitRocks.WriteLine("..."); });
                }

                try
                {
                    TestStep.RunStep("Uh oh!", delegate
                    {
                        TestStep.AddMetadata(MetadataKeys.Description, "The untimely death of a Blocker Lemming.");
                        TestStep.AddMetadata("Epitaph", "Did not follow the herd."); 

                        Assert.IsTrue(false, "*POP*");
                    });
                }
                catch (AssertionException)
                {
                }
            });

            TestStep.AddMetadata("Highlight", "It's dynamic!");

            TestStep.RunStep("Tag Line", delegate
            {
                MbUnitRocks.WriteLine("And so much more...");
                MbUnitRocks.WriteLine();

                MbUnitRocks.WriteLine("Welcome to MbUnit v3!");
            });
        }

        /// <summary>
        /// Demonstrate the use of multiple streams.
        /// </summary>
        [Test]
        public void MultipleStreams()
        {
            Console.Error.WriteLine("Console error messages go here.");

            Console.Out.WriteLine("Console output messages go here.");

            Debug.WriteLine("Debug...");
            Trace.WriteLine("... and Trace messages go here.");

            TestLog.Warnings.WriteLine("Warnings go here.");

            TestLog.Failures.WriteLine("Failures go here.");

            TestLog.WriteLine("Log messages go here by default.");

            TestLog.Writer["My Custom Log"].WriteLine("Log messages can also go here or to any other custom stream of your choice.");
        }
    }
}
