#!/bin/bash
if [ ! -f packages/FAKE/tools/FAKE.exe ]; then
  mono .nuget/nuget.exe install FAKE -OutputDirectory packages -ExcludeVersion
fi
mono packages/FAKE/tools/FAKE.exe build.fsx $@
