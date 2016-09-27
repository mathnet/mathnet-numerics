using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathNet.Numerics.UnitTests.OptimizationTests
{
    internal static class TestCaseDataExtensions
    {
        public static TestCaseData IgnoreIf(this TestCaseData input, bool do_ignore, string reason)
        {
            if (do_ignore)
                return input.Ignore(reason);
            else
                return input;
        }
    }
}
