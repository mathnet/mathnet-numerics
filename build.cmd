@echo off
cls
if not exist tools\paket\paket.exe (
  tools\paket\paket.bootstrapper.exe
)
tools\paket\paket.exe install
packages\FAKE\tools\FAKE.exe build.fsx %*
