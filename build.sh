#!/usr/bin/env bash

set -eu
set -o pipefail

cd "$(dirname "$0")"

dotnet tool restore
dotnet paket restore

FAKE_EXE=packages/build/FAKE/tools/FAKE.exe

FSIARGS=""
FSIARGS2=""
OS=${OS:-"unknown"}
if [[ "$OS" != "Windows_NT" ]]
then
  FSIARGS="--fsiargs"
  FSIARGS2="-d:MONO"

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

run $FAKE_EXE "$@" $FSIARGS $FSIARGS2 build.fsx
