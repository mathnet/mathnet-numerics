R E A D M E
===========

Overview
--------

NBenchmark is a framework and a tool to measure performance of HW and SW, investigate system bottlenecks,
optimize code to meet performance criteria and automate performance testing.

Key features:
1. Gets _measurable_ results of performance testing (numbers you can analyze and compare)
2. Makes performance tests _reproducable_ (repeat tests in different environments
   by different people at a different time)
3. Tests specific narrow scenarios we well as realistic combinations of such scenarios
4. Measures impact of the environment (HW and SW) to the results of the tests
5. Provides a set of standard tests for HW and OS to investigate system bottlenecks, certify HW
6. Generates reports to quickly communicate the results and keep for historical records
7. Supports majority of Windows operating systems: Windows XP, Vista, 2003,
   Windows Mobile 5.0+, CE 6.0+. Makes the results of tests on different platforms _comparable_
8. Provides framework to implement application-level benchmarks
9. Integrates benchmarking into continuous building process and regression testing
10. Integrates into Visual Studio to benchmark applications and perform profiling

How to use GUI Benchmarking Tool
--------------------------------

1. Open Benchmark.exe application. You may run it with command line parameters to load tests
  and configurations (see How to use Console Benchmarking Tool)
2. Load Test Suites from external assemblies (all dependent assemblies shall be accessible for the tool)
3. Pick which tests you want to execute, adjust test frequences as needed
4. Load/change test configurations
5. Specify benchmarking settings:
   - Single threaded or multi-threaded
   - Peak (maximum, stress) benchmarking or Nominal benchmarking (at certain TPS level)
6. After benchmarking is completed see/save the report. The results are presented
   in "Transactions per Second" (TPS). How to interpret TPS depends on the test.
   For instance, if Disk test in 1 transaction writes/reads 1MB of data, then 1 TPS = 1 MB/s
7. Check environment characteristics. Update system benchmark as needed

How to use Console Benchmarking Tool
------------------------------------

1. Prepare test configuration file (see below)
2. Run ConsoleBenchmark.exe from command line with arguments (see below)
3. See/save the results

Configuration Parameters
------------------------

1. Generic parameters (used by the tool)

General.Benchmarking.NumberOfThreads=<number of threads>
General.Benchmarking.TestingMode=[ Peak | Nominal ]
General.Benchmarking.NominalPerformance=<set TPS for nominal testing>

<TestSuiteName>.<TestName>.Enabled=[ true | false ]
<TestSuiteName>.<TestName>.Frequency=<test execution frequency>

2. Parameters of Standard Benchmarking Test Suite

StandardBenchmark.FilePath=<empty default or location of disk test file>
StandardBenchmark.FileSize=<disk test file size. Default=10Mb (2Mb for CE)>
StandardBenchmark.ChunkSize=<chunk size to read/write in 1 transaction. Default=1Mb>
StandardBenchmark.OperationTypes=[read | write | all]

Command Line Arguments
----------------------

/a=<assembly file> - Assembly with test suite(s) to be loaded. You may include
                   multiple assemblies
/c=<configuration file> - File with configuration parameters to be loaded
/r=<report file> - File to save benchmarking report
/t=<minutes>     - Benchmarking time specified in minutes
/r=<report file> - File to save benchmarking report
/h               - Display this help screen
/b               - Batch mode
/e               - Benchmark environment
/m=[pick|nominal] - Testing mode: Peak or Nominal performance
/n=<nominal tps> - Nominal performance in transactions per second

Sample of Benchmark Test
------------------------

namespace DigitalLiving.NBenchmark.Implementation.Environment
{
    public class DefaultCpuBenchmarkTest : BenchmarkTest
    {
        private const string NameText = "CPU";
        private const string DescriptionText = "Default CPU Benchmark";
        private const int NumberOfAttempts = 2000;

        public DefaultCpuBenchmarkTest(StandardBenchmarkTestSuite parentTestSuite)
            : base(parentTestSuite, NameText, DescriptionText)
        {
        }

        public new StandardBenchmarkTestSuite TestSuite
        {
            get { return base.TestSuite as StandardBenchmarkTestSuite; }
        }

        public override void SetUp()
        {
        }

        public override void Execute()
        {
            for (double value = 0; value < NumberOfAttempts; value++)
            {
                double result1 = value + value;
                double result2 = result1 - value;
                double result3 = result1 * result2;
                double result4 = result2 / result3;
                Math.Log(result4);
            }
        }

        public override void TearDown()
        {
        }
    }
}


Sample of Benchmark Test Suite
------------------------------

namespace DigitalLiving.NBenchmark.Implementation.Environment
{
    public class StandardBenchmarkTestSuite : BenchmarkTestSuite
    {
        private const string NameText = "StandardBenchmark";
        private const string DescriptionText = "Standard suite of benchmark tests for measuring system performance";

        private DefaultCpuBenchmarkTest _cpuBenchmarkTest;
        private DefaultDiskBenchmarkTest _diskBenchmarkTest;
        private DefaultVideoBenchmarkTest _videoBenchmarkTest;

        public StandardBenchmarkTestSuite()
            : base(NameText, DescriptionText)
        {
            _cpuBenchmarkTest = new DefaultCpuBenchmarkTest(this);
            AddTest(_cpuBenchmarkTest);

            _diskBenchmarkTest = new DefaultDiskBenchmarkTest(this);
            AddTest(_diskBenchmarkTest);

            _videoBenchmarkTest = new DefaultVideoBenchmarkTest(this);
            AddTest(_videoBenchmarkTest);

            base.Configuration = new StandardBenchmarkConfigurationParameterCollection();
        }

        public DefaultCpuBenchmarkTest CpuBenchmarkTest
        {
            get { return _cpuBenchmarkTest; }
        }

        public DefaultDiskBenchmarkTest DiskBenchmarkTest
        {
            get { return _diskBenchmarkTest; }
        }

        public DefaultVideoBenchmarkTest VideoBenchmarkTest
        {
            get { return _videoBenchmarkTest; }
        }

        public new StandardBenchmarkConfigurationParameterCollection Configuration
        {
            get { return base.Configuration as StandardBenchmarkConfigurationParameterCollection; }
        }

        public void DisableAllTest()
        {
            foreach (BenchmarkTest test in Tests)
            {
                test.Enabled = false;
            }
        }

        public override void StartUp()
        {
        }

        public override void TearDown()
        {
        }
    }
}

How to Integrate Benchmarking into Visual Studio
------------------------------------------------

1. Implement Benchmark Tests and Test Suites
2. Go to Project Properties -> Debug and set "Run External" to Benchmark.exe or ConsoleBenchmark.exe
3. Set in command line arguments /a=<my assembly name> and other parameters
4. Run or profile the benchmarks


Content of the Package
-----------------------

1. Benchmark.exe & Benchmark.Embedded.exe - GUI Benchmarking Tool
2. ConsoleBenchmark.exe & ConsoleBenchmark.Embedded.exe - Console Benchmarking Tool
3. DigitalLiving.NBenchmark.XXX.Implementation.dll - standard benchmark tests
4. DigitalLiving.NBenchmark.Examples.XXX.SampleBenchmarkTests.dll - test benchmark tesks
5. Source Code
6. Test batch files
7. This ReadMe File


Contacts
--------

Sergey Seroukhov <seroukhov@gmail.com>
Mark Zontak <mrmark@gmail.com>
Sergey Merkuriev <mr.umka@gmail.com>