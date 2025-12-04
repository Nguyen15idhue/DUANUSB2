@echo off
:: Reset Testing Environment
:: This will:
:: 1. Stop service
:: 2. Delete bind.key (to allow rebinding)
:: 3. Delete synced DLL
:: 4. Restart service
:: Right-click and "Run as Administrator"

echo ========================================
echo Reset Testing Environment
echo ========================================
echo.

echo [1/4] Stopping service...
net stop DongleSyncService
timeout /t 2 /nobreak > nul

echo [2/4] Deleting bind.key...
if exist "C:\ProgramData\DongleSyncService\bind.key" (
    del /f /q "C:\ProgramData\DongleSyncService\bind.key"
    echo     - bind.key deleted
) else (
    echo     - bind.key not found
)

echo [3/4] Deleting synced DLL...
if exist "C:\ProgramData\DongleSyncService\synced_patch.dll" (
    del /f /q "C:\ProgramData\DongleSyncService\synced_patch.dll"
    echo     - synced_patch.dll deleted
) else (
    echo     - synced_patch.dll not found
)

echo [4/4] Starting service...
net start DongleSyncService

echo.
echo ========================================
echo [SUCCESS] Environment reset complete!
echo ========================================
echo.
echo You can now insert USB to test fresh binding
echo.

pause
