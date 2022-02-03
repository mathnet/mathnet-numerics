@echo off
cls

dotnet tool restore
dotnet paket restore

if errorlevel 1 (
  exit /b %errorlevel%
)

dotnet run --project ./build/build.fsproj -- -t %*
