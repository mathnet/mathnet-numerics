#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net
  [ ! -f tools/paket/paket.exe ] && tools/paket/paket.bootstrapper.exe
  tools/paket/paket.exe restore
  packages/FAKE/tools/FAKE.exe build.fsx $@
else
  # use mono
  [ ! -f tools/paket/paket.exe ] && mono --runtime=v4.0 tools/paket/paket.bootstrapper.exe
  mono --runtime=v4.0 tools/paket/paket.exe restore
  mono --runtime=v4.0 packages/FAKE/tools/FAKE.exe $@ --fsiargs -d:MONO build.fsx
fi
