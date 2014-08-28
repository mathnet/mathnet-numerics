#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net
  [ ! -f packages/FAKE/tools/FAKE.exe ] && tools/NuGet/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
  packages/FAKE/tools/FAKE.exe build.fsx $@
else
  # use mono
  [ ! -f packages/FAKE/tools/FAKE.exe ] && mono --runtime=v4.0 tools/NuGet/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
  mono --runtime=v4.0 packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx
fi
