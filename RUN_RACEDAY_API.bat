@echo off
REM =========================================================
REM  RaceDay Part 2 - Launch API & Open Swagger in Browser
REM  Student: Lesedi Mogatwe (ST10476247)
REM =========================================================
cd /d "%~dp0"

echo.
echo ================================================================
echo  RaceDay Part 2 - ASP.NET Core Web API & Swagger UI Launcher
echo  Running on .NET 8.0 LTS
echo ================================================================
echo.

echo [1/2] Running Unit Tests (Verifying 30 tests pass)...
dotnet test RaceDay.sln --verbosity normal
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Unit tests failed. API will not start until tests pass.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [2/2] Launching RaceDay.API on http://localhost:5000/swagger ...
start "" http://localhost:5000/swagger
dotnet run --project RaceDay.API/RaceDay.API.csproj --urls "http://localhost:5000"

pause
