@echo off
cls
if not exist packages\FAKE\tools\FAKE.exe (
  tools\NuGet\NuGet.exe install FAKE -OutputDirectory packages -ExcludeVersion
)
packages\FAKE\tools\FAKE.exe build.fsx %*
