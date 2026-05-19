# Phase 3: verify Mod build outputs and optional game plugin folder.
# Usage:
#   .\Run_Deploy_Check.ps1
#   .\Run_Deploy_Check.ps1 -GameInstallRoot "C:\...\Chill with You Lo-Fi Story"
#   .\Run_Deploy_Check.ps1 -PluginFolder "C:\...\BepInEx\plugins\AIChat"

param(
    [string] $GameInstallRoot = "",
    [string] $PluginFolder = ""
)

$ErrorActionPreference = "Continue"
$failed = $false

function Test-One {
    param([string] $Path, [string] $Label, [switch] $Strict)
    if (Test-Path -LiteralPath $Path) {
        Write-Host "  [OK] $Label" -ForegroundColor Green
    } else {
        Write-Host "  [MISS] $Label" -ForegroundColor Red
        Write-Host "         $Path"
        if ($Strict) { $script:failed = $true }
    }
}

$ScriptDir = $PSScriptRoot
$ModRoot = Resolve-Path (Join-Path $ScriptDir "..\..\AIChat")
$ReleaseDir = Join-Path $ModRoot "bin\Release\net472"

Write-Host ""
Write-Host "=== Phase3: Release build (repo) ===" -ForegroundColor Cyan
Test-One (Join-Path $ReleaseDir "AIChat.dll") "AIChat.dll" -Strict
Test-One (Join-Path $ReleaseDir "TTSBridge\Run_ChillMod_TTS.vbs") "TTSBridge\Run_ChillMod_TTS.vbs" -Strict
Test-One (Join-Path $ReleaseDir "TTSBridge\ChillMod_TTS_v3.bat") "TTSBridge\ChillMod_TTS_v3.bat" -Strict
Test-One (Join-Path $ReleaseDir "TTSBridge\DEPLOY.txt") "TTSBridge\DEPLOY.txt" -Strict

if ($PluginFolder) {
    $plug = $PluginFolder
} elseif ($GameInstallRoot) {
    $plug = Join-Path $GameInstallRoot "BepInEx\plugins\AIChat"
} else {
    $plug = ""
}

if ($plug) {
    Write-Host ""
    Write-Host "=== Game plugin folder (optional compare) ===" -ForegroundColor Cyan
    Write-Host "  Path: $plug"
    if (-not (Test-Path -LiteralPath $plug)) {
        Write-Host "  [INFO] Folder missing (skip if BepInEx not installed yet)." -ForegroundColor Yellow
    } else {
        Test-One (Join-Path $plug "AIChat.dll") "AIChat.dll"
        Test-One (Join-Path $plug "Run_ChillMod_TTS.vbs") "Run_ChillMod_TTS.vbs"
        Test-One (Join-Path $plug "ChillMod_TTS_v3.bat") "ChillMod_TTS_v3.bat"
    }
} else {
    Write-Host ""
    Write-Host "(No -GameInstallRoot / -PluginFolder: skipped game folder check)" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "=== Portable GPT-SoVITS root (manual) ===" -ForegroundColor Cyan
Write-Host "  Put chill_mod_tts_server.py next to webui.py and runtime\. Match weight names in bat." -ForegroundColor DarkGray

if ($failed) {
    Write-Host ""
    Write-Host "Some files missing: dotnet build -c Release or copy per TTSBridge\DEPLOY.txt" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Deploy check OK. Start TTS server, then Run_TTS_Http_Test.ps1 or CLICKME_Phase3.bat" -ForegroundColor Green
exit 0
