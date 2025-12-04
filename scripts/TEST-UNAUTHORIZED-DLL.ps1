Write-Host "======================================"
Write-Host "TEST: UNAUTHORIZED DLL DETECTION"
Write-Host "======================================"
Write-Host ""

$dllPath = "$env:APPDATA\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"
$backupPath = "$env:USERPROFILE\Desktop\CHC.CGO.Common.dll.backup"

Write-Host "Step 1: Checking current state..."
$state = Get-Content "C:\ProgramData\DongleSyncService\state.json" -Raw | ConvertFrom-Json
Write-Host "  isPatched: $($state.isPatched)"
Write-Host "  dllPath: $($state.dllPath)"
Write-Host "  USB GUID: $($state.usbGuid)"
Write-Host ""

if (Test-Path $dllPath) {
    Write-Host "Step 2: DLL EXISTS - Copying to backup..."
    Copy-Item $dllPath $backupPath -Force
    Write-Host "  ✅ Backup created: $backupPath"
    Write-Host ""
    
    Write-Host "Step 3: Now REMOVE the USB dongle"
    Write-Host "  (Service will detect and restore DLL within 3 seconds)"
    Write-Host ""
    Write-Host "Press Enter after USB removed..."
    Read-Host
    
    Write-Host "Step 4: Waiting 5 seconds for service to restore DLL..."
    Start-Sleep -Seconds 5
    Write-Host ""
    
    Write-Host "Step 5: Checking if DLL was restored..."
    if (Test-Path $dllPath) {
        Write-Host "  ❌ DLL STILL EXISTS (not restored yet)"
    } else {
        Write-Host "  ✅ DLL RESTORED (deleted)"
    }
    Write-Host ""
    
    Write-Host "Step 6: Pasting backup DLL back..."
    Copy-Item $backupPath $dllPath -Force
    Write-Host "  ✅ DLL pasted back to: $dllPath"
    Write-Host ""
    
    Write-Host "Step 7: Waiting 5 seconds for UNAUTHORIZED detection..."
    Start-Sleep -Seconds 5
    Write-Host ""
    
    Write-Host "Step 8: Checking logs for UNAUTHORIZED detection..."
    $unauthorized = Get-Content "C:\ProgramData\DongleSyncService\logs\service-20251204.log" | 
        Select-String -Pattern "UNAUTHORIZED" | 
        Select-Object -Last 5
    
    if ($unauthorized) {
        Write-Host "  ✅ SUCCESS - UNAUTHORIZED DLL DETECTED!"
        $unauthorized | ForEach-Object { Write-Host "    $_" }
    } else {
        Write-Host "  ❌ FAILED - No UNAUTHORIZED detection in logs"
    }
    Write-Host ""
    
    Write-Host "Step 9: Checking if DLL was auto-restored after paste..."
    if (Test-Path $dllPath) {
        Write-Host "  ❌ DLL STILL EXISTS - Not auto-restored!"
        Write-Host "  ⚠️ SECURITY BYPASS SUCCESSFUL - Bug not fixed!"
    } else {
        Write-Host "  ✅ DLL AUTO-RESTORED - Security working!"
    }
} else {
    Write-Host "❌ DLL not found at: $dllPath"
    Write-Host "Please insert USB dongle first to patch the DLL"
}

Write-Host ""
Write-Host "======================================"
Write-Host "Test completed"
Write-Host "======================================"
