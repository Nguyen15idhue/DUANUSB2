@echo off
echo ===== UNINSTALL - UPDATE - REINSTALL SERVICE =====
echo.

echo [1/4] Uninstalling service...
"C:\Program Files\CHC Geomatics\Dongle Service\DongleSyncService.exe" uninstall
timeout /t 3 /nobreak

echo.
echo [2/4] Copying new files...
xcopy /Y /Q "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish\*.*" "C:\Program Files\CHC Geomatics\Dongle Service\"

echo.
echo [3/4] Installing service...
"C:\Program Files\CHC Geomatics\Dongle Service\DongleSyncService.exe" install

echo.
echo [4/4] Starting service...
"C:\Program Files\CHC Geomatics\Dongle Service\DongleSyncService.exe" start

echo.
echo ===== DONE! =====
pause
