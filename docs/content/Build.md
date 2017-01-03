Building Math.NET Numerics
==========================

If you do not want to use the official binaries, or if you like to modify,
debug or contribute, you can compile locally either using Visual Studio or
manually with the build scripts.

VisualStudio or Xamarin Studio
------------------------------

We clearly separate dependency management from the IDE, you should therefore
run `restore.cmd` or `restore.sh` once after every git checkout in order to restore
the dependencies exactly as defined. Otherwise Visual Studio and other IDEs
may fail to compile or provide correct IntelliSense.

Tests can be run with the usual integrated NUnit test runners or ReSharper.

MSBuild or XBuild
-----------------

Instead of a compatible IDE you can also build the solutions directly with
`msbuild`, or on Mono with `xbuild`. You may need to run `restore.cmd` or
`restore.sh` before, once after every git checkout in order to restore the dependencies.

	restore.cmd (or restore.sh)             # restore dependencies (once)
    msbuild MathNet.Numerics.sln            # only build for .Net 4 (main solution)
    msbuild MathNet.Numerics.Net35Only.sln  # only build for .Net 3.5
    msbuild MathNet.Numerics.All.sln        # full build with .Net 4, 3.5 and PCL profiles
    xbuild MathNet.Numerics.sln             # build with Mono, e.g. on Linux or Mac

FAKE
----

The fully automated build including unit tests, documentation and api
reference, NuGet and Zip packages is using [FAKE](http://fsharp.github.io/FAKE/).

FAKE itself is not included in the repository but it will download and bootstrap
itself automatically when build.cmd is run the first time. Note that this step
is *not* required when using Visual Studio or `msbuild` directly.

    build.cmd    # normal build (.Net 4.0), run unit tests (.Net on Windows)
    ./build.sh   # normal build (.Net 4.0), run unit tests (Mono on Linux/Mac, .Net on Windows)

    build.cmd Build              # normal build (.Net 4.0)
    build.cmd Build incremental  # normal build, incremental (.Net 4.0)
    build.cmd Build all          # full build (.Net 4.0, 3.5, PCL)
    build.cmd Build net35        # compatibility build (.Net 3.5
    build.cmd Build signed       # normal build, signed/strong named (.Net 4.0)

    build.cmd Test          # normal build (.Net 4.0), run unit tests
    build.cmd Test quick    # normal build (.Net 4.0), run unit tests except long running ones
    build.cmd Test all      # full build (.Net 4.0, 3.5, PCL), run all unit tests
    build.cmd Test net35    # compatibility build (.Net 3.5), run unit tests

    build.cmd Clean         # cleanup build artifacts
    build.cmd Docs          # generate documentation
    build.cmd Api           # generate api reference
    build.cmd Zip           # generate zip packages (.Net 4.0)
    build.cmd NuGet         # generate NuGet packages (.Net 4.0)
    build.cmd NuGet all     # generate normal NuGet packages (.Net 4.0, 3.5, PCL)
    build.cmd NuGet signed  # generate signed/strong named NuGet packages (.Net 4.0)

    build.cmd All           # build, test, docs, api reference (.Net 4.0)

If the build or tests fail claiming that FSharp.Core was not be found, see
[fsharp.org](http://fsharp.org/use/windows/) or install the
[Visual F# 3.0 Tools](https://go.microsoft.com/fwlink/?LinkId=261286) directly.

Dependencies
------------

We manage NuGet and other dependencies with [Paket](http://fsprojects.github.io/Paket/).
You do not normally have to do anything with Paket as it is integrated into our
FAKE build tools, unless you want to actively manage the dependencies.

You can bootstrap or update Paket by calling `tools/paket/paket.bootstrapper.exe`.
After bootstrapping, `tools/paket/paket.exe restore` will restore the packages
to the exact version specified in the `paket.lock` file,
`tools/paket/paket.exe install` will install or migrate packages after you have
made changes to the `paket.dependencies` file, `tools/paket/paket.exe outdated`
will show whether any packages are out of date and `tools/paket/paket.exe update`
will update all packages within the defined constraints. Have a look at the Paket
website for more commands and details.

Documentation
-------------

This website and documentation is automatically generated from of a set of
[CommonMark](http://commonmark.org/) structured files in `doc/content/` using
[FSharp.Formatting](http://tpetricek.github.io/FSharp.Formatting/).
The final documentation can be built by calling `build.sh Docs`.

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

    build.sh All release    # full release build
    build.sh NuGet release  # if you only need NuGet packages
    build.sh Zip release    # if you only need Zip packages

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
like the MKL provider and the data extensions. Most build targets are available for
these packages as well, with the following prefixes:

*   `Mkl` for the MKL provider (`MklWin` or `MklLinux` if platform dependent)
*   `Data` for the Data Extensions

Example: `build.sh MklWinNuget release`

Official Release Process (Maintainers only)
-------------------------------------------

*   Update `RELEASENOTES.md` file with relevant changes, attributed by contributor (if external). Set date.
*   Update `CONTRIBUTORS.md` file (via `git shortlog -sn`)

*   Build Release:

        build.sh All release

*   Commit and push release notes and (auto-updated) assembly info files with new "Release: v1.2.3" commit

*   Publish Release:

        build.sh PublishDocs
        build.sh PublishApi
        build.sh PublishTag
        build.sh PublishMirrors
        build.sh PublishNuGet

    In theory there is also a `Publish` target to do this in one step, unfortunately
    publishing to the NuGet gallery is quite unreliable.

*   Create new GitHub release, attach Zip files (to be automated)
*   Copy artifacts to [release archive](https://1drv.ms/1lMtdNi) (to be automated)
*   Consider a tweet via [@MathDotNet](https://twitter.com/MathDotNet)
*   Consider a post to the [Google+ site](https://plus.google.com/112484567926928665204)
*   Update Wikipedia release version and date for the
    [Math.NET Numerics](https://en.wikipedia.org/wiki/Math.NET_Numerics) and
    [Comparison of numerical analysis software](https://en.wikipedia.org/wiki/Comparison_of_numerical_analysis_software) articles.
