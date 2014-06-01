#!/bin/bash
if [ ! -f packages/FAKE/tools/FAKE.exe ]; then
  .nuget/NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
fi
packages/FAKE/tools/FAKE.exe build.fsx $@
