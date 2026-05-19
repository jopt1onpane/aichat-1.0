@echo off
REM Start Chill TTS from this directory (manifest.json must live here).
setlocal
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0ChillTTSLauncher.ps1"
set ERR=%ERRORLEVEL%
if not "%ERR%"=="0" echo [ChillTTSLauncher] exit %ERR% & pause
exit /b %ERR%
