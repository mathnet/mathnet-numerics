#!/bin/bash
if test "$OS" = "Windows_NT"
then
  # use .Net
  [ ! -f tools/paket/paket.exe ] && tools/paket/paket.bootstrapper.exe
  tools/paket/paket.exe $@
else
  # use mono
  [ ! -f tools/paket/paket.exe ] && mono --runtime=v4.0 tools/paket/paket.bootstrapper.exe
  mono --runtime=v4.0 tools/paket/paket.exe $@
fi
