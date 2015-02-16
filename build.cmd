@echo off
cls

.paket\paket.bootstrapper.exe
if errorlevel 1 (
    powershell.exe -ExecutionPolicy remotesigned -File bootstrap.ps1
)

.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

IF NOT EXIST build.fsx (
  .paket\paket.exe update
  packages\FAKE\tools\FAKE.exe init.fsx
)
packages\FAKE\tools\FAKE.exe build.fsx %*
