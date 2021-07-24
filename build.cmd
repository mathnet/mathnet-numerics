@echo off
cls

dotnet tool restore
dotnet paket restore

if errorlevel 1 (
  exit /b %errorlevel%
)

dotnet fake run build.fsx -t %*
