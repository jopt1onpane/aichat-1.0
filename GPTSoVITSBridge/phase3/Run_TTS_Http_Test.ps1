# Phase 3: GET /health and POST /tts (same contract as in-game). Start chill_mod_tts_server.py first.
# ASCII-only script (safe for Windows PowerShell 5.1). Default JA strings are UTF-8 via Base64.
#
# Usage:
#   powershell -File .\Run_TTS_Http_Test.ps1 -RefWav "C:\path\to\ref.wav"
#   Copy phase3_user.sample.ps1 -> phase3_user.ps1 and edit, then run with no args.

param(
    [string] $BaseUrl = "",
    [string] $RefWav = "",
    [string] $OutWav = ""
)

$ErrorActionPreference = "Stop"

function Decode-B64Utf8([string] $b64) {
    [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($b64))
}

# Defaults aligned with AIMod.cs Audio_File_Text / short test phrase "ohayou".
$DefaultPromptB64 = "5ZCb44GM6ZuG5Lit44GX44Gf5pmC44Gu44K344O844K/5rOi44KS5qSc5Ye644GX44Gm44CB44Oq44Oz44Kv44KS44Gk44Gq44GO55u044Gb44Gw5YWD6YCa44KK44Gr44Gq44KL44Gv44Ga44CC"
$DefaultTextB64   = "44GK44Gv44KI44GG"

$ScriptDir = $PSScriptRoot
$userFile = Join-Path $ScriptDir "phase3_user.ps1"
if (Test-Path -LiteralPath $userFile) {
    . $userFile
}

if (-not $BaseUrl -and $Phase3_TtsBaseUrl) { $BaseUrl = $Phase3_TtsBaseUrl }
if (-not $BaseUrl) { $BaseUrl = "http://127.0.0.1:9880" }
if (-not $RefWav -and $Phase3_RefWav) { $RefWav = $Phase3_RefWav }
if (-not $OutWav -and $Phase3_OutWav) { $OutWav = $Phase3_OutWav }

$promptText = $null
$promptLang = $null
$testText = $null
$testLang = $null
if ($Phase3_PromptText) { $promptText = [string]$Phase3_PromptText }
if ($Phase3_PromptLang) { $promptLang = [string]$Phase3_PromptLang }
if ($Phase3_TestText)   { $testText   = [string]$Phase3_TestText }
if ($Phase3_TestTextLang) { $testLang = [string]$Phase3_TestTextLang }
if (-not $promptText) { $promptText = Decode-B64Utf8 $DefaultPromptB64 }
if (-not $promptLang) { $promptLang = "ja" }
if (-not $testText)   { $testText   = Decode-B64Utf8 $DefaultTextB64 }
if (-not $testLang)   { $testLang   = "ja" }

if (-not $RefWav) {
    Write-Host "Set Ref wav: copy phase3_user.sample.ps1 to phase3_user.ps1 (Phase3_RefWav), or pass -RefWav." -ForegroundColor Red
    exit 1
}
if (-not (Test-Path -LiteralPath $RefWav)) {
    Write-Host "Ref wav not found: $RefWav" -ForegroundColor Red
    exit 1
}

$RefWav = (Resolve-Path -LiteralPath $RefWav).Path
$base = $BaseUrl.TrimEnd('/')

if (-not $OutWav) {
    $OutWav = Join-Path $ScriptDir "phase3_test_out.wav"
}

Write-Host ""
Write-Host "=== GET /health ===" -ForegroundColor Cyan
try {
    $healthUrl = "$base/health"
    $resp = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 15
    Write-Host "  HTTP $($resp.StatusCode)" -ForegroundColor Green
    Write-Host $resp.Content
} catch {
    Write-Host "  Failed: $_" -ForegroundColor Red
    Write-Host "  Start: portable runtime\python.exe chill_mod_tts_server.py (port 9880)." -ForegroundColor Yellow
    exit 2
}

function Escape-JsonString([string] $s) {
    if ($null -eq $s) { return '""' }
    $sb = New-Object System.Text.StringBuilder
    [void]$sb.Append('"')
    foreach ($ch in $s.ToCharArray()) {
        $c = [int][char]$ch
        switch ($ch) {
            '"' {
                [void]$sb.Append([char]92)
                [void]$sb.Append('"')
                continue
            }
            '\' {
                [void]$sb.Append([char]92)
                [void]$sb.Append([char]92)
                continue
            }
            "`n" {
                [void]$sb.Append('\n')
                continue
            }
            "`r" {
                [void]$sb.Append('\r')
                continue
            }
            "`t" {
                [void]$sb.Append('\t')
                continue
            }
            default {
                if ($c -lt 32) {
                    [void]$sb.AppendFormat('\u{0:x4}', $c)
                } else {
                    [void]$sb.Append($ch)
                }
            }
        }
    }
    [void]$sb.Append('"')
    return $sb.ToString()
}

$json = @(
    '{',
    ('  "text": ' + (Escape-JsonString $testText) + ','),
    ('  "text_lang": ' + (Escape-JsonString $testLang) + ','),
    ('  "ref_audio_path": ' + (Escape-JsonString $RefWav) + ','),
    ('  "prompt_text": ' + (Escape-JsonString $promptText) + ','),
    ('  "prompt_lang": ' + (Escape-JsonString $promptLang) + ','),
    '  "sample_steps": 32,',
    '  "if_sr": true',
    '}'
) -join "`n"

$tmp = [System.IO.Path]::GetTempFileName() + ".json"
$utf8NoBom = New-Object System.Text.UTF8Encoding $false
[System.IO.File]::WriteAllText($tmp, $json, $utf8NoBom)

Write-Host ""
Write-Host "=== POST /tts -> $OutWav ===" -ForegroundColor Cyan
$argTts = "$base/tts"
try {
    Invoke-WebRequest -Uri $argTts -Method Post -InFile $tmp -ContentType "application/json; charset=utf-8" -OutFile $OutWav -UseBasicParsing
} catch {
    Write-Host "POST /tts failed: $_" -ForegroundColor Red
    Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
    exit 3
}

Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue

if (-not (Test-Path -LiteralPath $OutWav)) {
    Write-Host "No output file." -ForegroundColor Red
    exit 4
}

$len = (Get-Item -LiteralPath $OutWav).Length
if ($len -lt 1000) {
    Write-Host "Output too small ($len bytes). Body may be JSON error:" -ForegroundColor Yellow
    Get-Content -LiteralPath $OutWav -Raw -Encoding UTF8 | Write-Host
    exit 5
}

Write-Host ""
Write-Host "[OK] Wrote $OutWav ($len bytes). Play in a media player." -ForegroundColor Green
exit 0
