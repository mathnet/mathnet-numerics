#!/bin/bash
if [ ! -f packages/FAKE/tools/FAKE.exe ]; then
  tools/NuGet/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
fi
packages/FAKE/tools/FAKE.exe build.fsx $@
