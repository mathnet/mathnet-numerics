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

using System.Text.RegularExpressions;
using Gallio.Framework;
using Gallio.Model.Logging;
using MbUnit.Framework;
using Gallio.Model;
using WatiN.Core;
using WatiN.Core.Interfaces;
using WatiN.Core.Logging;

namespace MbUnit.Samples
{
    /// <summary>
    /// This is a simple demonstration of integration opportunities between
    /// MbUnit v3 and WatiN.
    /// </summary>
    [TestFixture]
    public class WatiNDemo
    {
        private IE ie;

        [SetUp]
        public void CreateBrowser()
        {
            ie = new IE();

            Logger.LogWriter = new WatiNStreamLogger();
        }

        [TearDown]
        public void DisposeBrowser()
        {
            if (TestContext.CurrentContext.Outcome.Status == TestStatus.Failed)
                Snapshot("Final screen when failure occurred.", TestLog.Failures);

            if (ie != null)
                ie.Dispose();
        }

        /// <summary>
        /// Demonstrates capturing a screenshot on failure automatically.  This test does
        /// not contain any special logic for caputing the screenshot; it happens as part
        /// of the TearDown phase when it detects that the test has failed.
        /// </summary>
        [Test]
        public void DemoCaptureOnFailure()
        {
            using (TestLog.BeginSection("Go to Google, enter MbUnit as a search term and click I'm Feeling Lucky"))
            {
                ie.GoTo("http://www.google.com");

                ie.TextField(Find.ByName("q")).TypeText("MbUnit");
                ie.Button(Find.ByName("btnI")).Click();
            }

            // Of course this is ridiculous, we'll be on the MbUnit homepage...
            Assert.IsTrue(ie.ContainsText("NUnit"), "Expected to find NUnit on the page.");
        }

        /// <summary>
        /// Demonstrates capturing discretionary screenshots at will and labeling them.
        /// Unlike <see cref="DemoCaptureOnFailure" /> this test does not capture an
        /// extra automatic screenshot on termination because the TearDown phase can
        /// detect that the test has passed so it does not bother to capture an image.
        /// </summary>
        [Test]
        public void DemoNoCaptureOnSuccess()
        {
            using (TestLog.BeginSection("Go to Google, enter MbUnit as a search term and click I'm Feeling Lucky"))
            {
                ie.GoTo("http://www.google.com");

                ie.TextField(Find.ByName("q")).TypeText("MbUnit");
                ie.Button(Find.ByName("btnI")).Click();
            }

            using (TestLog.BeginSection("Click on About."))
            {
                Assert.IsTrue(ie.ContainsText("MbUnit"));
                ie.Link(Find.ByUrl(new Regex(@"About\.aspx"))).Click();
            }

            Snapshot("About the MbUnit project.");
        }

        private void Snapshot(string caption)
        {
            Snapshot(caption, TestLog.Default);
        }

        private void Snapshot(string caption, TestLogStreamWriter logStreamWriter)
        {
            using (logStreamWriter.BeginSection(caption))
            {
                logStreamWriter.Write("Url: ");
                using (logStreamWriter.BeginMarker(Marker.Link(ie.Url)))
                    logStreamWriter.Write(ie.Url);
                logStreamWriter.WriteLine();

                logStreamWriter.EmbedImage(caption + ".png", new CaptureWebPage(ie).CaptureWebPageImage(false, false, 100));
            }
        }

        private class WatiNStreamLogger : ILogWriter
        {
            public void LogAction(string message)
            {
                TestLog.WriteLine(message);
            }
        }
    }
}
