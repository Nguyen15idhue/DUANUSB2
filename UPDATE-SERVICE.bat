@echo off
echo Stopping service...
net stop DongleSyncService
timeout /t 3 /nobreak

echo Copying files...
xcopy /Y "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish\*.dll" "C:\Program Files\CHC Geomatics\Dongle Service\"
xcopy /Y "F:\3.Laptrinh\DUANUSB2\src\DongleSyncService\bin\Release\net8.0\win-x64\publish\DongleSyncService.exe" "C:\Program Files\CHC Geomatics\Dongle Service\"

echo Starting service...
net start DongleSyncService

echo Done!
pause
