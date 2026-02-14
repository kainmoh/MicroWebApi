#!/usr/bin/env pwsh
# Stop all microservices

Write-Host "?? Stopping All Microservices..." -ForegroundColor Red
Write-Host ""

# Kill all dotnet processes (services)
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue

if ($dotnetProcesses) {
    Write-Host "Found $($dotnetProcesses.Count) dotnet process(es) running" -ForegroundColor Yellow
    
    foreach ($process in $dotnetProcesses) {
        try {
            Write-Host "  ??  Stopping process: $($process.Id) - $($process.ProcessName)" -ForegroundColor Gray
            Stop-Process -Id $process.Id -Force
        }
        catch {
            Write-Host "  ??  Could not stop process $($process.Id)" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "? All dotnet processes stopped!" -ForegroundColor Green
}
else {
    Write-Host "??  No dotnet processes found running" -ForegroundColor Cyan
}

Write-Host ""

# Optionally kill PowerShell windows (be careful with this)
$powershellWindows = Get-Process -Name "powershell" -ErrorAction SilentlyContinue | 
    Where-Object { $_.MainWindowTitle -ne "" -and $_.Id -ne $PID }

if ($powershellWindows -and $powershellWindows.Count -gt 0) {
    Write-Host "Found $($powershellWindows.Count) PowerShell window(s) - Do you want to close them? (Y/N)" -ForegroundColor Yellow
    $response = Read-Host
    
    if ($response -eq 'Y' -or $response -eq 'y') {
        foreach ($window in $powershellWindows) {
            try {
                Stop-Process -Id $window.Id -Force
            }
            catch {
                Write-Host "  ??  Could not close window $($window.Id)" -ForegroundColor Yellow
            }
        }
        Write-Host "? PowerShell windows closed!" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "?? All services have been stopped!" -ForegroundColor Green
