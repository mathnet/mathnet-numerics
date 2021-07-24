#!/usr/bin/env bash

set -eu
set -o pipefail

cd "$(dirname "$0")"

dotnet tool restore
dotnet paket restore
