
@echo off

chcp 65001 >nul

setlocal

REM === 引数1: ビルドディレクトリ
if "%~1"=="" (
    echo [ERROR] バイナリディレクトリのパスを指定してください。使用例: BuildInstaller.bat ..\..\Build\Windows
    pause
    exit /b 1
)

pushd "%~1" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] ビルドディレクトリが存在しません: %~1
    pause
    exit /b 1
)
set "MY_BUILD_DIR=%CD%"
popd

REM === 引数2: 出力ディレクトリ（省略時は BuildDirの親にInstall\Output）
if "%~2"=="" (
    for %%I in ("%MY_BUILD_DIR%\..") do set "MY_OUTPUT_DIR=%%~fI\Install"
) else (
    for %%I in ("%~2") do set "MY_OUTPUT_DIR=%%~fI"
)

set "ISCC_PATH=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
set "ISS_SCRIPT=KeikanSetupScript.iss"

echo [INFO] MY_BUILD_DIR: %MY_BUILD_DIR%
echo [INFO] MY_OUTPUT_DIR: %MY_OUTPUT_DIR%

call "%ISCC_PATH%" /DMyBuildDir="%MY_BUILD_DIR%" /DMyOutputDir="%MY_OUTPUT_DIR%" "%ISS_SCRIPT%"

pause
