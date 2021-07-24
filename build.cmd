@echo off
cls

dotnet tool restore
dotnet paket restore

if errorlevel 1 (
  exit /b %errorlevel%
)

packages\build\FAKE\tools\FAKE.exe build.fsx %*
