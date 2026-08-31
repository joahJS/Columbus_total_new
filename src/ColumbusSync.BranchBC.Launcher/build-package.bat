@echo off
REM ColumbusSync.BranchBC + Launcher를 빌드해서, 바로 배포 가능한 폴더 구성을 만든다.
REM 그냥 이 파일을 탐색기에서 더블클릭하면 됩니다 — "Developer Command Prompt"를
REM 따로 열 필요 없습니다. vswhere.exe(Visual Studio 설치 시 항상 같이 깔리는 도구)로
REM msbuild.exe 위치를 스스로 찾습니다.
REM
REM 결과물: 이 스크립트가 있는 폴더 기준 ..\..\dist\ColumbusSync.BranchBC\
REM   ColumbusSync.BranchBC.Launcher.exe   <- B/C지점 PC에서 이걸 실행
REM   x86\ColumbusSync.BranchBC.exe
REM   x64\ColumbusSync.BranchBC.exe

setlocal
set SCRIPT_DIR=%~dp0
set REPO_ROOT=%SCRIPT_DIR%..\..
set DIST_DIR=%REPO_ROOT%\dist\ColumbusSync.BranchBC

call :find_msbuild
if not defined MSBUILD_EXE (
    echo [오류] msbuild.exe를 찾지 못했습니다. Visual Studio가 설치되어 있는지 확인해주세요.
    echo         ^(build tools가 아닌 일반 Visual Studio 설치가 필요합니다.^)
    pause
    exit /b 1
)
echo msbuild 위치: %MSBUILD_EXE%

echo [1/4] 워커(x86) 빌드 중...
"%MSBUILD_EXE%" "%REPO_ROOT%\src\ColumbusSync.BranchBC\ColumbusSync.BranchBC.csproj" /p:Configuration=Release /p:Platform=x86 /nologo /verbosity:minimal
if errorlevel 1 goto :error

echo [2/4] 워커(x64) 빌드 중...
"%MSBUILD_EXE%" "%REPO_ROOT%\src\ColumbusSync.BranchBC\ColumbusSync.BranchBC.csproj" /p:Configuration=Release /p:Platform=x64 /nologo /verbosity:minimal
if errorlevel 1 goto :error

echo [3/4] 런처 빌드 중...
"%MSBUILD_EXE%" "%SCRIPT_DIR%ColumbusSync.BranchBC.Launcher.csproj" /p:Configuration=Release /nologo /verbosity:minimal
if errorlevel 1 goto :error

echo [4/4] 배포 폴더 구성 중: %DIST_DIR%
if exist "%DIST_DIR%" rmdir /s /q "%DIST_DIR%"
mkdir "%DIST_DIR%\x86"
mkdir "%DIST_DIR%\x64"

copy /y "%SCRIPT_DIR%bin\Release\ColumbusSync.BranchBC.Launcher.exe" "%DIST_DIR%\" >nul
copy /y "%SCRIPT_DIR%bin\Release\ColumbusSync.BranchBC.Launcher.exe.config" "%DIST_DIR%\" >nul 2>nul

copy /y "%REPO_ROOT%\src\ColumbusSync.BranchBC\bin\x86\Release\ColumbusSync.BranchBC.exe" "%DIST_DIR%\x86\" >nul
copy /y "%REPO_ROOT%\src\ColumbusSync.BranchBC\bin\x86\Release\ColumbusSync.BranchBC.exe.config" "%DIST_DIR%\x86\" >nul 2>nul

copy /y "%REPO_ROOT%\src\ColumbusSync.BranchBC\bin\x64\Release\ColumbusSync.BranchBC.exe" "%DIST_DIR%\x64\" >nul
copy /y "%REPO_ROOT%\src\ColumbusSync.BranchBC\bin\x64\Release\ColumbusSync.BranchBC.exe.config" "%DIST_DIR%\x64\" >nul 2>nul

if not exist "%DIST_DIR%\x86\ColumbusSync.BranchBC.exe.config" (
    echo [안내] App.config가 없어 설정파일이 안 들어갔습니다. src\ColumbusSync.BranchBC\App.config.example을
    echo         App.config로 복사해 값을 채운 뒤 다시 빌드하세요.
)

echo.
echo 완료: %DIST_DIR%
echo 이 폴더를 통째로 B/C지점 PC에 복사한 뒤, ColumbusSync.BranchBC.Launcher.exe 를 실행하세요.
pause
exit /b 0

:error
echo.
echo [오류] 빌드에 실패했습니다. 위 로그를 확인하세요.
pause
exit /b 1

REM ------------------------------------------------------------------
REM vswhere.exe(Visual Studio 설치 시 항상 함께 설치되는 도구)로 msbuild.exe 경로를 찾는다.
REM 이 방법을 쓰면 "Developer Command Prompt"를 열지 않고 일반 탐색기에서 더블클릭해도 된다.
REM ------------------------------------------------------------------
:find_msbuild
set VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist %VSWHERE% set VSWHERE="%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist %VSWHERE% (
    REM vswhere가 없는 아주 오래된 VS라면, 설치 폴더 전체를 뒤져서 최후의 수단으로 찾는다.
    for /f "usebackq delims=" %%f in (`dir /s /b "%ProgramFiles(x86)%\Microsoft Visual Studio\MSBuild.exe" 2^>nul`) do set MSBUILD_EXE=%%f
    if not defined MSBUILD_EXE for /f "usebackq delims=" %%f in (`dir /s /b "%ProgramFiles%\Microsoft Visual Studio\MSBuild.exe" 2^>nul`) do set MSBUILD_EXE=%%f
    goto :eof
)

for /f "usebackq tokens=*" %%i in (`%VSWHERE% -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do (
    set MSBUILD_EXE=%%i
)
goto :eof
