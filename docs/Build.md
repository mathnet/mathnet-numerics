Building Math.NET Numerics
==========================

If you do not want to use the official binaries, or if you like to modify,
debug or contribute, you can compile locally either using Visual Studio or
manually with the build scripts.

System Requirements
-------------------

* .NET Core SDK 3.1.1 ([download](https://dotnet.microsoft.com/download/dotnet-core/3.1))

VisualStudio and other IDEs
---------------------------

We clearly separate dependency management from the IDE and therefore recommend to
run `restore.cmd` or `restore.sh` once after every git checkout in order to restore
the dependencies exactly as defined. Otherwise Visual Studio and other IDEs
may fail to compile or provide correct IntelliSense.

Tests can be run with the usual integrated NUnit test runners or ReSharper.

Command Line Tools
------------------

Instead of a compatible IDE you can also build the solutions directly with
the .NET Core SDK build tools. You may need to run `restore.cmd` or `restore.sh`
before, once after every git checkout in order to restore the dependencies.

    restore.cmd (or ./restore.sh)
    dotnet build MathNet.Numerics.sln

FAKE
----

The fully automated build including unit tests, documentation and api
reference, NuGet and Zip packages is using [FAKE](https://fsharp.github.io/FAKE/).

FAKE itself is not included in the repository but it will download and bootstrap
itself automatically when build.cmd is run the first time. Note that this step
is *not* required when using Visual Studio or the .NET Core SDK directly.

    ./build.sh   # normal build and unit tests, when using bash shell on Windows or Linux.
    build.cmd    # normal build and unit tests, when using Windows CMD shell.

    ./build.sh build              # normal build
    ./build.sh build strongname   # normal build and also build strong-named variant

    ./build.sh test          # normal build, run unit tests
    ./build.sh test quick    # normal build, run unit tests except long running ones

    ./build.sh clean         # cleanup build artifacts
    ./build.sh docs          # generate documentation
    ./build.sh api           # generate api reference

    ./build.sh all           # build, test, docs, api reference

If the build or tests fail claiming that FSharp.Core was not be found, see
[fsharp.org](https://fsharp.org/use/windows/) or install the
[Visual F# 3.0 Tools](https://go.microsoft.com/fwlink/?LinkId=261286) directly.

Dependencies
------------

We manage NuGet and other dependencies with [Paket](https://fsprojects.github.io/Paket/).
You do not normally have to do anything with Paket as it is integrated into our
FAKE build tools, unless you want to actively manage the dependencies.

`.paket/paket.exe restore` will restore the packages
to the exact version specified in the `paket.lock` file,
`.paket/paket.exe install` will install or migrate packages after you have
made changes to the `paket.dependencies` file, `.paket/paket.exe outdated`
will show whether any packages are out of date and `.paket/paket.exe update`
will update all packages within the defined constraints. Have a look at the Paket
website for more commands and details.

Documentation
-------------

This website and documentation is automatically generated from of a set of
[CommonMark](https://commonmark.org/) structured files in `doc/content/` using
[FSharp.Formatting](https://tpetricek.github.io/FSharp.Formatting/).
The final documentation can be built by calling `build.sh docs`.

However, for editing and previewing the docs on your local machine it is more
convenient to run `build.sh DocsWatch` in a separate console instead, which
monitors the content files and incrementally regenerates the HTML output
automatically. DocsWatch will also use local/relative URIs instead of absolute
ones, so that the links and styles will work as expected locally. This can
also be enabled in a full one-time build with `build.sh DocsDev` instead
of just `Docs`.

Creating a Release
------------------

While only maintainers can make official releases published on NuGet and
referred to from the website, you can use the same tools to make your own
releases for your own purposes.

Versioning is controlled by the release notes. Before building a new version,
first add a new release header and change notes on top of the `RELEASENOTES.md`
document in the root directory. The fake builds pick this up and propagate it
to the assembly info files automatically.

The build can then be launched by calling:

    ./build.sh all

The build script will print the current version as part of the the header banner,
which is also included in the release notes document in the build artifacts.
Example:

    //  __  __       _   _       _   _ ______ _______
    // |  \/  |     | | | |     | \ | |  ____|__   __|
    // | \  / | __ _| |_| |__   |  \| | |__     | |
    // | |\/| |/ _` | __| '_ \  | . ` |  __|    | |
    // | |  | | (_| | |_| | | |_| |\  | |____   | |
    // |_|  |_|\__,_|\__|_| |_(_)_| \_|______|  |_|
    //
    // Math.NET Numerics - https://numerics.mathdotnet.com
    // Copyright (c) Math.NET - Open Source MIT/X11 License
    //
    // Math.NET Numerics                    v3.5.0
    // Math.NET Numerics MKL Provider       v1.7.0
    // Math.NET Numerics Data Extensions    v3.1.0

The artifacts are then ready in the `out/packages` directory.

Extra Packages
--------------

In addition to the core package this repository also include extra packages
like the data extensions. Most build targets are available for
these packages as well, with the following prefixes:

*   `Data` for the Data Extensions

Example: `build.sh DataBuild`

Intel MKL on Windows
--------------------

Building the Intel MKL native provider for Windows requires additionally:

* Either Intel Parallel Studio 2020 or Intel Math Kernel Library 2020 is installed
* Visual Studio 2019, with the following options
    * Desktop development with C++ workload
    * Windows 10 SDK (10.0.17763.0)
    * MSVC v142 - VS 2019 C++ x64/x86 build tools

The build can then be triggered by calling:

    ./build.sh MklWinBuild  // build both 32 and 64 bit variants
    ./build.sh MklTest      // run all tests with the MKL provider enforced
    ./build.sh MklWinAll    // build and run tests

If you run into an error with `mkl_link_tool.exe` you may need to patch a targets file,
see [MKL 2020.1, VS2019 linking bug ](https://software.intel.com/en-us/forums/intel-math-kernel-library/topic/851578).

The build puts the binaries to `out/MKL/Windows/x64` (and `x86`), the NuGet package
to `out/MKL/NuGet` and a Zip archive to `out/MKL/Zip`. You can directly use the provider from
there by setting `Control.NativeProviderPath` to the full path pointing to `out/MKL/Windows/`;
this is also what the unit tests do when you run the `MklTest` build target.

Official Release Process (Maintainers only)
-------------------------------------------

*   Update `RELEASENOTES.md` file with relevant changes, attributed by contributor (if external). Set date.
*   Update `CONTRIBUTORS.md` file (via `git shortlog -sn`)

*   Build Release:

        build.sh all --strongname

*   Commit and push release notes and (auto-updated) assembly info files with new "Release: v1.2.3" commit

*   Publish Release:

        build.sh PublishDocs
        build.sh PublishApi
        build.sh PublishTag
        build.sh PublishArchive
        build.sh PublishNuGet

*   Consider a tweet via [@MathDotNet](https://twitter.com/MathDotNet)
*   Update Wikipedia release version and date for the
    [Math.NET Numerics](https://en.wikipedia.org/wiki/Math.NET_Numerics) and
    [Comparison of numerical analysis software](https://en.wikipedia.org/wiki/Comparison_of_numerical_analysis_software) articles.
