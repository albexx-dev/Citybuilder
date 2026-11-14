#Requires -Version 5.1
<#
.SYNOPSIS
  Start the Citybuilder MCP stack for WorkBuddy AI (Unity bridge + Blender socket).
.DESCRIPTION
  Paths are derived from the repository root or from CITYBUILDER_UNITY_EXE /
  CITYBUILDER_BLENDER_EXE. Does NOT touch scenes, code or git. Safe to run repeatedly.
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File Tools\Start-WorkBuddyStack.ps1
#>
[CmdletBinding()]
param(
    [int]$TimeoutSec = 120
)

$ErrorActionPreference = 'SilentlyContinue'

$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$configPath = if ($env:WORKBUDDY_MCP_CONFIG) { $env:WORKBUDDY_MCP_CONFIG } else { Join-Path $env:USERPROFILE '.workbuddy-ai\mcp.json' }
$bridgeUrl = 'http://127.0.0.1:8080/mcp'

function Get-UnityVersion {
    $versionFile = Join-Path $projectRoot 'ProjectSettings\ProjectVersion.txt'
    $line = Select-String -Path $versionFile -Pattern '^m_EditorVersion:\s*(\S+)' | Select-Object -First 1
    if ($line) { return $line.Matches.Groups[1].Value }
    return '6000.3.25f1'
}

function Resolve-UnityExe {
    # 1. Environment variable override
    if ($env:CITYBUILDER_UNITY_EXE) {
        $envPath = $env:CITYBUILDER_UNITY_EXE.Trim('"', "'", ' ')
        $envCandidates = @(
            $envPath,
            (Join-Path $envPath 'Unity.exe'),
            (Join-Path $envPath 'Editor\Unity.exe')
        )
        foreach ($c in $envCandidates) {
            if ((Test-Path -LiteralPath $c) -and -not (Test-Path -LiteralPath $c -PathType Container)) {
                return [IO.Path]::GetFullPath($c)
            }
        }
        return $envPath
    }

    $version = Get-UnityVersion

    # 2. Windows Registry lookup
    $regKeys = @(
        "HKLM:\SOFTWARE\Unity Technologies\Installer\Unity $version",
        "HKCU:\SOFTWARE\Unity Technologies\Installer\Unity $version",
        "HKLM:\SOFTWARE\WOW6432Node\Unity Technologies\Installer\Unity $version",
        "HKCU:\SOFTWARE\WOW6432Node\Unity Technologies\Installer\Unity $version"
    )
    $propNames = @('Location x64', 'Location', 'InstallLocation', 'UnityPath', 'Path')
    foreach ($rk in $regKeys) {
        if (Test-Path -LiteralPath $rk) {
            $item = Get-ItemProperty -LiteralPath $rk -ErrorAction SilentlyContinue
            if ($item) {
                foreach ($prop in $propNames) {
                    $val = $item.$prop
                    if ($val) {
                        $loc = $val.ToString().Trim('"', "'", ' ')
                        $candidates = @(
                            $loc,
                            (Join-Path $loc 'Editor\Unity.exe'),
                            (Join-Path $loc 'Unity.exe')
                        )
                        foreach ($c in $candidates) {
                            if ((Test-Path -LiteralPath $c) -and -not (Test-Path -LiteralPath $c -PathType Container)) {
                                return [IO.Path]::GetFullPath($c)
                            }
                        }
                    }
                }
            }
        }
    }

    # 3. Known fallback locations and standard drive roots
    $fallbackPaths = @(
        "D:\Unity\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe",
        "C:\Program Files\Unity\Hub\Editor\$version-x86_64\Editor\Unity.exe",
        "C:\Program Files\Unity\Editor\Unity.exe"
    )
    $roots = @($env:ProgramFiles, ${env:ProgramFiles(x86)}, $env:LOCALAPPDATA, $env:ProgramData, 'D:', 'E:', 'F:')
    $subPaths = @(
        "Unity\Hub\Editor\$version\Editor\Unity.exe",
        "Unity\Hub\Editor\$version-x86_64\Editor\Unity.exe",
        "Unity\Editor\Unity.exe"
    )
    foreach ($root in $roots) {
        if (-not $root) { continue }
        foreach ($sub in $subPaths) {
            $fallbackPaths += (Join-Path $root $sub)
        }
    }
    foreach ($cand in $fallbackPaths) {
        if ((Test-Path -LiteralPath $cand) -and -not (Test-Path -LiteralPath $cand -PathType Container)) {
            return [IO.Path]::GetFullPath($cand)
        }
    }

    # 4. PATH environment lookup
    $onPath = Get-Command Unity.exe -ErrorAction SilentlyContinue
    if ($onPath -and $onPath.Source) {
        return $onPath.Source
    }

    return $null
}

function Resolve-BlenderExe {
    if ($env:CITYBUILDER_BLENDER_EXE) { return $env:CITYBUILDER_BLENDER_EXE }
    $roots = @($env:ProgramFiles, ${env:ProgramFiles(x86)}, $env:LOCALAPPDATA)
    foreach ($root in $roots) {
        if (-not $root) { continue }
        $candidate = Join-Path $root 'Blender Foundation\Blender 4.5\blender.exe'
        if (Test-Path $candidate) { return $candidate }
    }
    $onPath = Get-Command blender.exe -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }
    return $null
}

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
        if ($p.CommandLine -like "*$projectRoot*") { return $p }
    }
    return $null
}

$unityExe = Resolve-UnityExe
$blenderExe = Resolve-BlenderExe

Write-Host '=== Citybuilder MCP stack ===' -ForegroundColor Cyan
Write-Host ("[i] Project root: {0}" -f $projectRoot)

# 1. WorkBuddy MCP config
$configOk = $false
try {
    $cfg = Get-Content $configPath -Raw | ConvertFrom-Json
    $configOk = ($cfg.mcpServers.unityMCP.url -eq $bridgeUrl)
} catch { $configOk = $false }
Write-Host ("[{0}] WorkBuddy config: {1}" -f $(if ($configOk) {'OK'} else {'FAIL'}), $configPath)
if (-not $configOk) {
    Write-Host ("[i] Template: {0}" -f (Join-Path $projectRoot 'Tools\mcp.example.json')) -ForegroundColor Yellow
}

# 2. Unity Editor
$unityProc = Get-UnityWithProject
if (-not $unityProc) {
    if (-not $unityExe -or -not (Test-Path $unityExe)) {
        Write-Host '[FAIL] Unity Editor not found. Set CITYBUILDER_UNITY_EXE.' -ForegroundColor Red
    } else {
        Write-Host '[..] Starting Unity Editor (project import may take a minute)...'
        Start-Process -FilePath $unityExe -ArgumentList '-projectPath', $projectRoot | Out-Null
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
    if (-not $blenderExe -or -not (Test-Path $blenderExe)) {
        Write-Host '[FAIL] Blender not found. Set CITYBUILDER_BLENDER_EXE.' -ForegroundColor Red
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
Write-Host ("[i] Run diagnostics anytime: powershell -ExecutionPolicy Bypass -File {0}\Tools\Check-UnityMcp.ps1" -f $projectRoot) -ForegroundColor Yellow

if ($configOk -and $unityOk -and $blenderOk) {
    Write-Host '=== STACK READY ===' -ForegroundColor Green
    exit 0
}
Write-Host '=== STACK PARTIALLY UP (see FAIL lines above) ===' -ForegroundColor Yellow
exit 1
