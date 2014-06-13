#!/bin/bash
if [ ! -f packages/FAKE/tools/FAKE.exe ]; then
  mono --runtime=v4.0 tools/NuGet/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
fi
mono --runtime=v4.0 packages/FAKE/tools/FAKE.exe build.fsx $@
