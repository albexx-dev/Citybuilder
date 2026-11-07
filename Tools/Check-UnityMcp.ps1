#Requires -Version 5.1
<#
.SYNOPSIS
  Проверка связки WorkBuddy AI <-> Unity MCP (и прочих MCP-серверов).
  Код выхода 0 — всё в порядке, 1 — есть проблемы (список в выводе).
.EXAMPLE
  powershell -ExecutionPolicy Bypass -File Tools\Check-UnityMcp.ps1
#>
[CmdletBinding()]
param()

$ErrorActionPreference = 'SilentlyContinue'
$failures = 0

function Check($name, [scriptblock]$test) {
    if (& $test) { Write-Host "[OK] $name" }
    else { Write-Host "[FAIL] $name"; $script:failures++ }
}

# Корень проекта — корень репо (с 039 репо само является Unity-проектом).
$projectRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$unityProject = $projectRoot

# 1. Unity открыт с нужным проектом
Check 'Unity запущен с проектом CityBuilder' {
    $found = $false
    Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | ForEach-Object {
        if ($_.CommandLine -like "*$unityProject*") { $found = $true }
    }
    $found
}

# 2. Порт моста слушается (Get-NetTCPConnection через CIM не видит loopback)
Check 'Порт 8080 слушается' {
    $client = New-Object System.Net.Sockets.TcpClient
    try {
        $async = $client.BeginConnect('127.0.0.1', 8080, $null, $null)
        $ok = $async.AsyncWaitHandle.WaitOne(1000, $false)
        $ok -and $client.Connected
    } catch { return $false }
    finally { $client.Close() }
}

# 3. Мост отвечает по HTTP
Check 'Мост отвечает на http://127.0.0.1:8080/mcp' {
    try {
        $r = Invoke-WebRequest -Uri 'http://127.0.0.1:8080/mcp' -Method Get -TimeoutSec 5 -UseBasicParsing
        $r.StatusCode -in @(400, 406)
    } catch {
        $_.Exception.Response -ne $null -and $_.Exception.Response.StatusCode.value__ -in @(400, 406)
    }
}

# 4. Конфиг WorkBuddy AI — unityMCP на месте
Check 'Конфиг WorkBuddy AI: ~/.workbuddy-ai/mcp.json с unityMCP' {
    $cfg = Join-Path $env:USERPROFILE '.workbuddy-ai\mcp.json'
    if (-not (Test-Path $cfg)) { return $false }
    try {
        $j = Get-Content $cfg -Raw | ConvertFrom-Json
        $j.mcpServers.unityMCP.url -eq 'http://127.0.0.1:8080/mcp'
    } catch { return $false }
}

# 5. Запуск из корня проекта (дубля скриптов больше нет — сверять нечего)
Check 'Текущая папка — корень проекта' {
    [IO.Path]::GetFullPath((Get-Location).Path) -eq $projectRoot
}

# 6. Все нужные MCP-серверы прописаны в конфиге
Check 'MCP-серверы прописаны: unityMCP / context7 / playwright / blender' {
    try {
        $cfg = Join-Path $env:USERPROFILE '.workbuddy-ai\mcp.json'
        $j = Get-Content $cfg -Raw | ConvertFrom-Json
        $names = @($j.mcpServers.PSObject.Properties.Name)
        $missing = @('unityMCP', 'context7', 'playwright', 'blender') | Where-Object { $names -notcontains $_ }
        $missing.Count -eq 0
    } catch { return $false }
}
# Connected-статус самого WorkBuddy извне не виден: Trust нажимается в UI коннекторов.
Write-Host '[i] Порт зелёный, а unityMCP красный? Нажми Trust в Custom Connector.'
if ($failures -gt 0) {
    Write-Host ""
    Write-Host "Проблем: $failures. Порядок чинки см. UNITY_MCP_SETUP.md"
    exit 1
}
Write-Host ""
Write-Host "Всё в порядке."
exit 0
