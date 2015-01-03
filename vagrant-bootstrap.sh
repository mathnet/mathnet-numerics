#!/usr/bin/env bash

apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list

apt-get update
apt-get -y upgrade

apt-get -y install mono-complete
apt-get -y install mono-tools-devel
apt-get -y install referenceassemblies-pcl
apt-get -y install fsharp

apt-get -y autoremove

mozroots --import --sync
