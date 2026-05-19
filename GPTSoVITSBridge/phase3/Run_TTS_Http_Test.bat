@echo off
REM Double-click this instead of the .ps1 (PS1 often opens in Notepad).
setlocal
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Run_TTS_Http_Test.ps1" %*
echo.
pause
