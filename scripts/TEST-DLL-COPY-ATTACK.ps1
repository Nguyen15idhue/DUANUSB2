# Test DLL Copy Attack Detection
Write-Host "`n=== TESTING DLL COPY ATTACK DETECTION ===" -ForegroundColor Red
Write-Host "This test verifies Layer 5 security fix" -ForegroundColor Yellow
Write-Host ""

$dllPath = "C:\Users\ADMIN\AppData\Roaming\CHCNAV\CHC Geomatics Office 2\CHC.CGO.Common.dll"
$stateFile = "C:\ProgramData\DongleSyncService\state.json"
$logFile = Get-ChildItem "C:\ProgramData\DongleSyncService\logs" -Filter "*.log" | Sort LastWriteTime -Descending | Select -First 1

Write-Host "Monitoring DLL at:" -ForegroundColor Cyan
Write-Host "  $dllPath" -ForegroundColor White
Write-Host ""

$iteration = 0
$lastState = $null
$lastDllExists = $null

while ($true) {
    $iteration++
    Start-Sleep 2
    
    # Check DLL existence
    $dllExists = Test-Path $dllPath
    
    # Check state
    $state = Get-Content $stateFile | ConvertFrom-Json
    $isPatched = $state.isPatched
    
    # Only show changes
    if ($dllExists -ne $lastDllExists -or $isPatched -ne $lastState) {
        $time = Get-Date -Format "HH:mm:ss"
        
        Write-Host "[$time] " -NoNewline -ForegroundColor Gray
        
        if ($dllExists) {
            if ($isPatched) {
                Write-Host "DLL exists | State: PATCHED " -ForegroundColor Green -NoNewline
                Write-Host "(Normal - USB connected)" -ForegroundColor White
            } else {
                Write-Host "DLL exists | State: NOT PATCHED " -ForegroundColor Red -NoNewline
                Write-Host "← ATTACK DETECTED! Service should restore in 3s..." -ForegroundColor Yellow
            }
        } else {
            Write-Host "DLL removed | State: NOT PATCHED " -ForegroundColor White -NoNewline
            Write-Host "(Clean state)" -ForegroundColor Gray
        }
        
        $lastDllExists = $dllExists
        $lastState = $isPatched
    }
    
    # Show status every 10 iterations
    if ($iteration % 10 -eq 0) {
        Write-Host "." -NoNewline -ForegroundColor DarkGray
    }
}
