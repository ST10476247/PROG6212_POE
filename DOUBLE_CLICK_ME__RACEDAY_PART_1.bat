@echo off
REM =========================================================
REM  LeseDI ONE-CLICK LAUNCHER
REM  Double-click this .bat file (DO NOT rename it).
REM  It does NOT need admin. It does NOT need any PATH setup.
REM =========================================================
setlocal

REM Move into the folder THIS .bat file lives in.
cd /d "%~dp0"

echo.
echo  ================================================================
echo   RaceDay Part 1 - Lesedi Mogatwe - PROG6212_POE (ONE CLICK)
echo   Working directory: %cd%
echo  ================================================================
echo.

REM Step 1: Try every common Git install folder and ADD TO PATH IN SESSION.
set "GIT_FOUND=0"
for %%P in (
  "%ProgramFiles%\Git\cmd\git.exe"
  "%ProgramFiles(x86)%\Git\cmd\git.exe"
  "%LocalAppData%\Programs\Git\cmd\git.exe"
  "%AppData%\..\Local\Programs\Git\cmd\git.exe"
  "%UserProfile%\scoop\shims\git.exe"
  "C:\ProgramData\chocolatey\bin\git.exe"
  "C:\Program Files\Git\cmd\git.exe"
  "D:\Program Files\Git\cmd\git.exe"
  "C:\Git\cmd\git.exe"
) do (
  if exist %%~P (
    set "GIT_EXE=%%~P"
    set "GIT_FOUND=1"
    goto :git_injected
  )
)
:git_injected

if "%GIT_FOUND%"=="1" (
  for %%F in ("%GIT_EXE%") do set "GIT_DIR=%%~dpF"
  set "PATH=%GIT_DIR%;%PATH%"
  echo  [OK]   Git found at: %GIT_EXE%
  "%GIT_EXE%" --version
  echo.
) else (
  echo  [MISS] Git not installed yet. Downloading installer from git-scm.com ...
  if not exist "%TEMP%\Git-Installer-x64.exe" (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "$ProgressPreference='SilentlyContinue'; Invoke-WebRequest -UseBasicParsing -Uri 'https://github.com/git-for-windows/git/releases/latest/download/Git-2.47.0-64-bit.exe' -OutFile '%TEMP%\Git-Installer-x64.exe'"
  )
  echo  [RUN]  Installing Git silently ... wait 30-120 seconds, no window shown.
  "%TEMP%\Git-Installer-x64.exe" /VERYSILENT /NORESTART /NOCANCEL /COMPONENTS=icons,ext\reg\shellhere,assoc,assoc_sh,gitlfs,windowsterminal
  set "GIT_EXE=C:\Program Files\Git\cmd\git.exe"
  if exist "%GIT_EXE%" (
    echo  [OK]   Installed Git to C:\Program Files\Git
    set "PATH=C:\Program Files\Git\cmd;C:\Program Files\Git\usr\bin;%PATH%"
  ) else (
    echo.
    echo  [FAIL] Installer ran but C:\Program Files\Git\cmd\git.exe still absent.
    echo         Please open https://git-scm.com/download/win in your browser,
    echo         click the 64-bit download, NEXT NEXT NEXT x 6, then REBOOT.
    echo         After reboot, re-DOUBLE-CLICK this .bat file.
    echo.
    pause
    exit /b 2
  )
)

REM Step 2: Pass control to the PowerShell bootstrap that runs the 25 commits + push.
echo.
echo  [RUN]  Launching Lesedi_GIT_FIX_THEN_PUSH.ps1 ...
echo.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Lesedi_GIT_FIX_THEN_PUSH.ps1"

set "EXIT=%ERRORLEVEL%"
echo.
if "%EXIT%"=="0" (
  echo  ================================================================
echo   PUSH FINISHED - Lesedi Mogatwe - Part 1 pushed to PROG6212_POE.
  echo   Open: https://github.com/ST10476247/PROG6212_POE
  echo   Actions tab green build screenshot -> docs\screenshots\ci-green-build.png
  echo   YouTube Unlisted video -> paste URL into README placeholder
  echo   Then: copy repo URL into ARC and SUBMIT.
  echo  ================================================================
) else (
  echo   Bootstrap exited with code: %EXIT%  (see messages above)
  echo   Most likely fixes:
  echo   FIX-A = git pull --rebase origin main --allow-unrelated-histories ^&^& git push -u origin main
  echo   FIX-B = regenerate PAT with Contents=Read and write at https://github.com/settings/tokens
)
echo.
pause
endlocal
exit /b %EXIT%
