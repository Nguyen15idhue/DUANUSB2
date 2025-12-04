@echo off
:: Start USB Dongle Sync Service
:: Right-click and "Run as Administrator"

echo ========================================
echo Starting USB Dongle Sync Service
echo ========================================
echo.

net start DongleSyncService

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] Service started successfully!
    echo.
) else (
    echo.
    echo [ERROR] Failed to start service
    echo Make sure to run this as Administrator
    echo.
)

pause
