#!/usr/bin/env bash

set -eu
set -o pipefail

cd "$(dirname "$0")"

PAKET_EXE=.paket/paket.exe

OS=${OS:-"unknown"}
if [[ "$OS" != "Windows_NT" ]]
then
  # Allows NETFramework like net45 to be built using dotnet core tooling with mono
  export FrameworkPathOverride=$(dirname $(which mono))/../lib/mono/4.5/
fi

function run() {
  if [[ "$OS" != "Windows_NT" ]]
  then
    mono "$@"
  else
    "$@"
  fi
}

if [[ "$OS" != "Windows_NT" ]] && [ ! -e ~/.config/.mono/certs ]
then
  mozroots --import --sync --quiet
fi

run $PAKET_EXE restore
