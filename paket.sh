#!/usr/bin/env bash

set -eu
set -o pipefail

cd "$(dirname "$0")"

PAKET_EXE=.paket/paket.exe

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

run $PAKET_EXE "$@"
