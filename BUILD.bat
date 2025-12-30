@echo off
cls

echo ========================================
echo   SQL Server Manager - Build Script
echo ========================================
echo.
echo Compiling all source files...
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0compile.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ========================================
    echo   BUILD FAILED!
    echo ========================================
    pause
    exit /b 1
)

echo.
pause