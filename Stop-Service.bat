@echo off
:: Stop USB Dongle Sync Service
:: Right-click and "Run as Administrator"

echo ========================================
echo Stopping USB Dongle Sync Service
echo ========================================
echo.

net stop DongleSyncService

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] Service stopped successfully!
    echo.
) else (
    echo.
    echo [ERROR] Failed to stop service
    echo Make sure to run this as Administrator
    echo.
)

pause
