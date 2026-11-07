#Requires -Version 5.1
<#
.SYNOPSIS
  Start the Citybuilder MCP stack for WorkBuddy AI (Unity bridge + Blender socket).
.DESCRIPTION
  Launches Unity Editor with the project, waits for the HTTP bridge on 8080,
  launches Blender GUI, waits for the MCP socket on 9876, prints a summary.
  Does NOT touch scenes, code or git. Safe to run repeatedly.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File Tools\Start-WorkBuddyStack.ps1
#>
[CmdletBinding()]
param(
    [int]$TimeoutSec = 120
)

$ErrorActionPreference = 'SilentlyContinue'

$project     = 'C:\Users\admin\Citybuilder'
$unityExe    = 'C:\Program Files\Unity\Hub\Editor\6000.3.25f1-x86_64\Editor\Unity.exe'
$blenderExe  = 'C:\Program Files\Blender Foundation\Blender 4.5\blender.exe'
$configPath  = Join-Path $env:USERPROFILE '.workbuddy-ai\mcp.json'
$bridgeUrl   = 'http://127.0.0.1:8080/mcp'

function Test-Port {
    param([int]$Port)
    $client = New-Object System.Net.Sockets.TcpClient
    try {
        $async = $client.BeginConnect('127.0.0.1', $Port, $null, $null)
        $ok = $async.AsyncWaitHandle.WaitOne(700, $false)
        if ($ok -and $client.Connected) { return $true }
        return $false
    } finally {
        $client.Close()
    }
}

function Wait-Port {
    param([int]$Port, [int]$Seconds)
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-Port -Port $Port) { return $true }
        Start-Sleep -Seconds 2
    }
    return $false
}

function Get-UnityWithProject {
    $procs = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'"
    foreach ($p in $procs) {
        if ($p.CommandLine -like "*$project*") { return $p }
    }
    return $null
}

Write-Host '=== Citybuilder MCP stack ===' -ForegroundColor Cyan

# 1. WorkBuddy MCP config
$configOk = $false
try {
    $cfg = Get-Content $configPath -Raw | ConvertFrom-Json
    $configOk = ($cfg.mcpServers.unityMCP.url -eq $bridgeUrl)
} catch { $configOk = $false }
Write-Host ("[{0}] WorkBuddy config: {1}" -f $(if ($configOk) {'OK'} else {'FAIL'}), $configPath)

# 2. Unity Editor
$unityProc = Get-UnityWithProject
if (-not $unityProc) {
    if (-not (Test-Path $unityExe)) {
        Write-Host '[FAIL] Unity Editor not found' -ForegroundColor Red
    } else {
        Write-Host '[..] Starting Unity Editor (project import may take a minute)...'
        Start-Process -FilePath $unityExe -ArgumentList '-projectPath', $project | Out-Null
    }
} else {
    Write-Host ("[OK] Unity already running (pid {0})" -f $unityProc.ProcessId)
}

# 3. Unity HTTP bridge on 8080
Write-Host '[..] Waiting for bridge on 127.0.0.1:8080 ...'
$unityOk = Wait-Port -Port 8080 -Seconds $TimeoutSec
if ($unityOk) {
    try {
        $r = Invoke-WebRequest -Uri $bridgeUrl -Method Get -TimeoutSec 5 -UseBasicParsing
        $unityOk = $true
    } catch {
        # 400/406 is the expected answer from a streamable HTTP endpoint on GET.
        $code = $_.Exception.Response.StatusCode.value__
        $unityOk = ($code -in @(400, 406)) -or ($null -ne $_.Exception.Response)
    }
}
Write-Host ("[{0}] Unity MCP bridge {1}" -f $(if ($unityOk) {'OK'} else {'FAIL'}), $bridgeUrl)

# 4. Blender GUI + socket on 9876
$blenderProc = Get-Process blender -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $blenderProc) {
    if (-not (Test-Path $blenderExe)) {
        Write-Host '[FAIL] Blender not found' -ForegroundColor Red
    } else {
        Write-Host '[..] Starting Blender GUI ...'
        Start-Process -FilePath $blenderExe | Out-Null
    }
} else {
    Write-Host ("[OK] Blender already running (pid {0})" -f $blenderProc.Id)
}
Write-Host '[..] Waiting for Blender MCP socket on 127.0.0.1:9876 ...'
$blenderOk = Wait-Port -Port 9876 -Seconds $TimeoutSec
Write-Host ("[{0}] Blender MCP socket 9876" -f $(if ($blenderOk) {'OK'} else {'FAIL'}))

# 5. Summary
Write-Host ''
if (-not $blenderOk) {
    Write-Host '[i] Blender socket starts only in GUI mode (not in -b). In Blender: 3D View -> N -> MCP for Blender -> Start MCP Server.' -ForegroundColor Yellow
}
Write-Host '[i] If a connector is red in WorkBuddy while the port is green: Connectors -> Custom Connector -> Trust.' -ForegroundColor Yellow
Write-Host ("[i] Run diagnostics anytime: powershell -ExecutionPolicy Bypass -File {0}\Tools\Check-UnityMcp.ps1" -f $project) -ForegroundColor Yellow

if ($configOk -and $unityOk -and $blenderOk) {
    Write-Host '=== STACK READY ===' -ForegroundColor Green
    exit 0
}
Write-Host '=== STACK PARTIALLY UP (see FAIL lines above) ===' -ForegroundColor Yellow
exit 1
