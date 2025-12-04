@echo off
:: Restart USB Dongle Sync Service (to test from scratch)
:: Right-click and "Run as Administrator"

echo ========================================
echo Restarting USB Dongle Sync Service
echo ========================================
echo.

echo Stopping service...
net stop DongleSyncService

timeout /t 2 /nobreak > nul

echo Starting service...
net start DongleSyncService

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] Service restarted successfully!
    echo You can now test USB insertion again
    echo.
) else (
    echo.
    echo [ERROR] Failed to restart service
    echo.
)

pause
