# Quick Commands for Service Management
# Run these commands in PowerShell AS ADMINISTRATOR

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DongleSyncService - Quick Commands" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "STOP SERVICE (for testing):" -ForegroundColor Yellow
Write-Host "  Stop-Service DongleSyncService -Force" -ForegroundColor White
Write-Host ""

Write-Host "START SERVICE:" -ForegroundColor Yellow
Write-Host "  Start-Service DongleSyncService" -ForegroundColor White
Write-Host ""

Write-Host "RESTART SERVICE:" -ForegroundColor Yellow
Write-Host "  Restart-Service DongleSyncService -Force" -ForegroundColor White
Write-Host ""

Write-Host "CHECK STATUS:" -ForegroundColor Yellow
Write-Host "  Get-Service DongleSyncService | Format-List *" -ForegroundColor White
Write-Host ""

Write-Host "DELETE SYNCED DLL (force re-sync):" -ForegroundColor Yellow
Write-Host "  Remove-Item C:\ProgramData\DongleSyncService\synced_patch.dll -Force" -ForegroundColor White
Write-Host ""

Write-Host "DELETE MACHINE BINDING (allow rebind):" -ForegroundColor Yellow
Write-Host "  Remove-Item C:\ProgramData\DongleSyncService\bind.key -Force" -ForegroundColor White
Write-Host ""

Write-Host "VIEW LOGS (today):" -ForegroundColor Yellow
Write-Host "  Get-Content C:\ProgramData\DongleSyncService\logs\log-$(Get-Date -Format 'yyyyMMdd').txt -Tail 50" -ForegroundColor White
Write-Host ""

Write-Host "FULL RESET (for fresh test):" -ForegroundColor Yellow
Write-Host "  Stop-Service DongleSyncService -Force" -ForegroundColor White
Write-Host "  Remove-Item C:\ProgramData\DongleSyncService\synced_patch.dll -Force -ErrorAction SilentlyContinue" -ForegroundColor White
Write-Host "  Remove-Item C:\ProgramData\DongleSyncService\bind.key -Force -ErrorAction SilentlyContinue" -ForegroundColor White
Write-Host "  Start-Service DongleSyncService" -ForegroundColor White
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "Copy and paste commands above" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Pause to keep window open
Read-Host "Press Enter to close"
