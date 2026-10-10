#Requires -Version 5.1
<#
.SYNOPSIS
  Проверка связки OpenCode <-> Unity MCP.
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

$unityProject = 'C:\Users\admin\UnityProjects\CityBuilder'
$gitScripts = 'C:\Users\admin\Citybuilder\Assets\Citybuilder\Scripts'
$unityScripts = Join-Path $unityProject 'Assets\Citybuilder\Scripts'

# 1. Unity открыт с нужным проектом
Check 'Unity запущен с проектом CityBuilder' {
    $found = $false
    Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | ForEach-Object {
        if ($_.CommandLine -like "*$unityProject*") { $found = $true }
    }
    $found
}

# 2. Порт моста слушается
Check 'Порт 8080 слушается' {
    $null -ne (Get-NetTCPConnection -LocalPort 8080 -State Listen)
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

# 4. Конфиг OpenCode — один файл, unityMCP на месте
Check 'Единый конфиг opencode.json с unityMCP' {
    $cfg = 'C:\Users\admin\.config\opencode\opencode.json'
    $dup = 'C:\Users\admin\.config\opencode\opencode.jsonc'
    (Test-Path $cfg) -and (-not (Test-Path $dup)) -and
    ((Get-Content $cfg -Raw) -match '"unityMCP"') -and
    ((Get-Content $cfg -Raw) -match '"mcp"\s*:\s*\{\s*"servers"')
}

# 5. Скрипты git и Unity в синхроне (кроме .meta)
Check 'Скрипты git = скриптам Unity-проекта' {
    $ok = (Test-Path $gitScripts) -and (Test-Path $unityScripts)
    if ($ok) {
        Get-ChildItem $gitScripts -Recurse -File -Filter '*.cs' | ForEach-Object {
            $rel = $_.FullName.Substring($gitScripts.Length)
            $other = Join-Path $unityScripts $rel
            if (-not (Test-Path $other)) { Write-Host "  нет в Unity: $rel"; $ok = $false }
            elseif ((Get-FileHash $_.FullName).Hash -ne (Get-FileHash $other).Hash) {
                Write-Host "  отличается: $rel"; $ok = $false
            }
        }
        Get-ChildItem $unityScripts -Recurse -File -Filter '*.cs' | ForEach-Object {
            $rel = $_.FullName.Substring($unityScripts.Length)
            if (-not (Test-Path (Join-Path $gitScripts $rel))) { Write-Host "  нет в git: $rel"; $ok = $false }
        }
    }
    $ok
}

if ($failures -gt 0) {
    Write-Host ""
    Write-Host "Проблем: $failures. Порядок чинки см. UNITY_MCP_SETUP.md"
    exit 1
}
Write-Host ""
Write-Host "Всё в порядке."
exit 0
