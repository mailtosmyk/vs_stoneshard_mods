@echo off
setlocal EnableExtensions

rem Build Necromancy.sml (dotnet build + MSLM pack).

set "ROOT=%~dp0"
if "%ROOT:~-1%"=="\" set "ROOT=%ROOT:~0,-1%"

for %%I in ("%ROOT%\..\..\msl") do set "MSL_DIR=%%~fI"
if not defined OUT_DIR set "OUT_DIR=%MSL_DIR%\Mods"
if not defined MOD_NAME set "MOD_NAME=Necromancy"
if not defined CONFIG set "CONFIG=Release"

if not defined DOTNET_ROOT (
  if exist "%USERPROFILE%\.dotnet\dotnet.exe" set "DOTNET_ROOT=%USERPROFILE%\.dotnet"
)
if defined DOTNET_ROOT set "PATH=%DOTNET_ROOT%;%USERPROFILE%\.dotnet\tools;%PATH%"

where dotnet >nul 2>&1
if errorlevel 1 (
  echo error: dotnet SDK not found ^(need .NET 6+^).
  exit /b 1
)

where python >nul 2>&1
if errorlevel 1 (
  where python3 >nul 2>&1
  if errorlevel 1 (
    echo error: python / python3 not found.
    exit /b 1
  )
  set "PYTHON=python3"
) else (
  set "PYTHON=python"
)

if not exist "%MSL_DIR%\ModShardLauncher.dll" (
  echo error: ModShardLauncher.dll not found in %MSL_DIR%
  exit /b 1
)

if not exist "%ROOT%\necromancy.json" (
  echo error: necromancy.json missing in %ROOT%
  exit /b 1
)

if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"

rem Bump first so the packed DLL and build.txt share the same version.
%PYTHON% "%ROOT%\bump_build_version.py" "%ROOT%\build.txt"
if errorlevel 1 exit /b 1

echo Embedding build.txt version + necromancy.json
powershell -NoProfile -ExecutionPolicy Bypass -File "%ROOT%\embed-json.ps1" -Root "%ROOT%"
if errorlevel 1 exit /b 1

echo Building %MOD_NAME% (%CONFIG%)...
dotnet build "%ROOT%\Necromancy.csproj" -c %CONFIG% --nologo
if errorlevel 1 exit /b 1

set "DLL=%ROOT%\bin\%CONFIG%\net6.0\%MOD_NAME%.dll"
if not exist "%DLL%" (
  echo error: build output missing: %DLL%
  exit /b 1
)

set "SML=%OUT_DIR%\%MOD_NAME%.sml"
echo Packing %SML%...
%PYTHON% "%ROOT%\pack_sml.py" --root "%ROOT%" --dll "%DLL%" --out "%SML%" --name "%MOD_NAME%" --msl "%MSL_DIR%"
if errorlevel 1 exit /b 1

copy /Y "%DLL%" "%OUT_DIR%\%MOD_NAME%.dll" >nul

echo Done.
exit /b 0
