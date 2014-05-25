#!/usr/bin/env bash

apt-get update
apt-get -y install mono-complete
apt-get -y install mono-tools-devel
apt-get -y install fsharp
mozroots --import --sync
