# Building Math.NET Numerics

## Managed

The Math.NET Numerics solution is structured such that it is best to compile the whole
solution (MathNet.Numerics.sln) directly in VisualStudio or by MsBuild.

All artifacts will be placed in a generated "out" directory in the solution root,
structured as follows:

/lib/Net40: Binaries for .Net 4.0
/lib/SL4: Binaries for Silverlight 4

/debug/Net40: Debug Build for .Net 4.0
/debug/SL4: Debug Build for Silverlight 4

/test/Net40: Unit Tests for .Net 4.0
/test/debug/Net40: Debug Build Unit Tests for .Net 4.0

Windows Phone 7 builds will likely be added as "WP" subdirectories.
Note that there are currently only unit tests for .Net 4.0 builds available.

## Native Bindings

TODO

## Continuous Integration

We use TeamCity both for continuous integration and release builds & deployments,
the configuration and scripts are located in this folder and subfolders