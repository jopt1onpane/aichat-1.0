# Copies bridge + launcher + manifest into an EXISTING GPT-SoVITS portable root (your local model tree).
# Does NOT download weights. Edit manifest.json after deploy if weight filenames differ.
param(
    [Parameter(Mandatory = $true)]
    [string] $TargetRoot
)
$ErrorActionPreference = "Stop"
$TargetRoot = [IO.Path]::GetFullPath($TargetRoot.Trim().TrimEnd('\', '/'))
if (-not (Test-Path -LiteralPath $TargetRoot)) {
    throw "TargetRoot does not exist: $TargetRoot"
}

$toolsDir = $PSScriptRoot
$runtimeSpecDir = Split-Path $toolsDir -Parent
$aiChatDir = Split-Path $runtimeSpecDir -Parent
$repoInner = Split-Path $aiChatDir -Parent
$bridgeSrc = Join-Path $repoInner "GPTSoVITSBridge\chill_mod_tts_server.py"
if (-not (Test-Path -LiteralPath $bridgeSrc)) {
    throw "Bridge source not found: $bridgeSrc"
}

$bridgeDstDir = Join-Path $TargetRoot "bridge"
[void][IO.Directory]::CreateDirectory($bridgeDstDir)
Copy-Item -LiteralPath $bridgeSrc -Destination (Join-Path $bridgeDstDir "chill_mod_tts_server.py") -Force

$files = @(
    "ChillTTSLauncher.ps1",
    "ChillTTSLauncher.bat",
    "manifest.sample.json"
)
foreach ($f in $files) {
    Copy-Item -LiteralPath (Join-Path $runtimeSpecDir $f) -Destination (Join-Path $TargetRoot $f) -Force
}
$sampleMf = Join-Path $TargetRoot "manifest.sample.json"
$mfOut = Join-Path $TargetRoot "manifest.json"
Copy-Item -LiteralPath $sampleMf -Destination $mfOut -Force

Write-Host "Deployed Chill stack to: $TargetRoot"
Write-Host "Edit manifest.json if GPT/SoVITS filenames differ."
Write-Host "Mod cfg: GptSovits_Portable_Root = $TargetRoot"
Write-Host "Mod cfg: TTS_Service_Script_Path = BepInEx\\plugins\\AIChat\\Run_ChillTTSLauncher.vbs (default in DLL)"
