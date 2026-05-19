@echo off
REM ASCII only (ANSI cmd). Game sets CHILL_GSV_HOME from cfg GptSovits_Portable_Root.
setlocal
if not defined CHILL_GSV_HOME (
  echo [ChillBridge] CHILL_GSV_HOME not set. Set GptSovits_Portable_Root in BepInEx cfg.
  pause
  exit /b 1
)
set "GSV_HOME=%CHILL_GSV_HOME%"
pushd "%GSV_HOME%" 2>nul
if errorlevel 1 (
  echo [ChillBridge] Cannot cd: %GSV_HOME%
  pause
  exit /b 1
)
set "PYEXE=%CD%\runtime\python.exe"
if not exist "%PYEXE%" (
  echo [ChillBridge] Missing runtime\python.exe in: %CD%
  pause
  exit /b 1
)
set "GPT_FILE=GPT_weights_v3\satone-e20.ckpt"
set "SOVITS_FILE=SoVITS_weights_v3\satone_e2_s2492_l64.pth"
if not exist "%CD%\chill_mod_tts_server.py" (
  echo [ChillBridge] Copy chill_mod_tts_server.py to portable root (next to webui.py).
  pause
  exit /b 1
)
echo [ChillBridge] HOME=%CD%
echo [ChillBridge] Starting TTS...
"%PYEXE%" "%CD%\chill_mod_tts_server.py" --gptsovits-home "%CD%" --gpt "%GPT_FILE%" --sovits "%SOVITS_FILE%" --preload
set ERR=%ERRORLEVEL%
if not "%ERR%"=="0" (
  echo [ChillBridge] Python exit %ERR%
  pause
)
exit /b %ERR%
