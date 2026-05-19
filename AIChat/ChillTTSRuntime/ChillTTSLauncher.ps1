# Chill TTS launcher: reads manifest.json next to this script, starts bridge with portable python.
# ROOT = directory containing manifest.json (same as GptSovits_Portable_Root / CHILL_GSV_HOME).
$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$mfPath = Join-Path $Root "manifest.json"
if (-not (Test-Path -LiteralPath $mfPath)) {
    Write-Host "[ChillTTSLauncher] Missing manifest.json in: $Root"
    exit 1
}
$mf = Get-Content -LiteralPath $mfPath -Raw -Encoding UTF8 | ConvertFrom-Json

function Join-Root([string] $rel) {
    if ([string]::IsNullOrWhiteSpace($rel)) { return $null }
    $rel = $rel.Trim().Replace("/", [IO.Path]::DirectorySeparatorChar)
    [IO.Path]::GetFullPath([IO.Path]::Combine($Root, $rel))
}

$py = Join-Root $mf.pythonExe
$bridge = Join-Root $mf.bridgeScript
$gpt = $mf.gptWeights.Trim().Replace("/", [IO.Path]::DirectorySeparatorChar)
$sov = $mf.sovitsWeights.Trim().Replace("/", [IO.Path]::DirectorySeparatorChar)

if (-not (Test-Path -LiteralPath $py)) {
    Write-Host "[ChillTTSLauncher] Missing python: $py"
    exit 1
}
if (-not (Test-Path -LiteralPath $bridge)) {
    Write-Host "[ChillTTSLauncher] Missing bridge: $bridge"
    exit 1
}

$preload = $true
if ($null -ne $mf.preload) { $preload = [bool]$mf.preload }

Write-Host "[ChillTTSLauncher] HOME=$Root"
Write-Host "[ChillTTSLauncher] Starting..."
# Do not use Start-Process -ArgumentList with paths containing spaces (e.g. Program Files (x86));
# PowerShell may split args and Python sees "C:\Program" as the script path.
Push-Location -LiteralPath $Root
try {
    if ($preload) {
        & $py $bridge "--gptsovits-home" $Root "--gpt" $gpt "--sovits" $sov "--preload"
    } else {
        & $py $bridge "--gptsovits-home" $Root "--gpt" $gpt "--sovits" $sov
    }
    exit $LASTEXITCODE
} finally {
    Pop-Location
}
