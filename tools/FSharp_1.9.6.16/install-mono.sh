#!/bin/sh

# This script is for use with CLIs on Unix, e.g. Mono
gacutil -i bin/FSharp.Core.dll
gacutil -i bin/FSharp.Compiler.dll
gacutil -i bin/FSharp.PowerPack.dll

# Trying to use --aot on Mono does not seem to work correctly,
# at least on the pre-release of Mono 2.0.
# On Un*x, it seems to work in the sense that it creates the
# .so files, but then fsi.exe does not seem to work at all.
# On Windows, it just does not work.
# For this reason, it does not seem to be advisable to try to
# use it.
#mono --aot bin/FSharp.Core.dll
#mono --aot bin/FSharp.Compiler.dll
#mono --aot bin/FSharp.PowerPack.dll
#mono --aot bin/fsc.exe
#mono --aot bin/fsi.exe
