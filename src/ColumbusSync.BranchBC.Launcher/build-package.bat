@echo off
REM ColumbusSync.BranchBC + Launcher를 빌드해서, 바로 배포 가능한 폴더 구성을 만든다.
REM Visual Studio 2017+ 개발자 명령 프롬프트(msbuild.exe가 PATH에 있는 환경)에서 실행하세요.
REM
REM 결과물: 이 스크립트가 있는 폴더 기준 ..\..\dist\ColumbusSync.BranchBC\
REM   ColumbusSync.BranchBC.Launcher.exe   <- B/C지점 PC에서 이걸 실행
REM   x86\ColumbusSync.BranchBC.exe
REM   x64\ColumbusSync.BranchBC.exe

setlocal
set SCRIPT_DIR=%~dp0
set REPO_ROOT=%SCRIPT_DIR%..\..
set DIST_DIR=%REPO_ROOT%\dist\ColumbusSync.BranchBC

where msbuild >nul 2>nul
if errorlevel 1 (
    echo [오류] msbuild.exe를 찾을 수 없습니다. "Developer Command Prompt for VS" 에서 실행해주세요.
    exit /b 1
)

echo [1/4] 워커(x86) 빌드 중...
msbuild "%REPO_ROOT%\src\ColumbusSync.BranchBC\ColumbusSync.BranchBC.csproj" /p:Configuration=Release /p:Platform=x86 /nologo /verbosity:minimal
if errorlevel 1 goto :error

echo [2/4] 워커(x64) 빌드 중...
msbuild "%REPO_ROOT%\src\ColumbusSync.BranchBC\ColumbusSync.BranchBC.csproj" /p:Configuration=Release /p:Platform=x64 /nologo /verbosity:minimal
if errorlevel 1 goto :error

echo [3/4] 런처 빌드 중...
msbuild "%SCRIPT_DIR%ColumbusSync.BranchBC.Launcher.csproj" /p:Configuration=Release /nologo /verbosity:minimal
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
exit /b 0

:error
echo.
echo [오류] 빌드에 실패했습니다. 위 로그를 확인하세요.
exit /b 1
