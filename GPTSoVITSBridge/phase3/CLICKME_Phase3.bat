@echo off
REM ASCII only. Full path with (x86) is OK when run via double-click.
setlocal
cd /d "%~dp0"
echo [1/2] Deploy check (Release build + optional game folder^)...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Run_Deploy_Check.ps1" %*
if errorlevel 1 (
  echo.
  pause
  exit /b 1
)
echo.
echo [2/2] TTS HTTP test. Start chill_mod_tts_server.py first. Ref wav: phase3_user.ps1 or -RefWav.
echo.
pause
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Run_TTS_Http_Test.ps1" %*
echo.
pause
