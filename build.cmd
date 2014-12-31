@echo off
cls
if not exist tools\paket\paket.exe (
  tools\paket\paket.bootstrapper.exe
)
tools\paket\paket.exe restore
packages\FAKE\tools\FAKE.exe build.fsx %*
