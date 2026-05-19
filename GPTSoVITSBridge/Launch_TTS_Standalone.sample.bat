@echo off
REM Copy to your GPT-SoVITS portable ROOT as Start_Chill_TTS.bat, OR set GSV_HOME below
REM and run from anywhere. Do NOT double-click chill_mod_tts_server.py.
setlocal
set "GSV_HOME=CHANGE_ME_TO_PORTABLE_ROOT_WITH_webui.py_and_runtime"
cd /d "%GSV_HOME%"
set "PYEXE=%CD%\runtime\python.exe"
if not exist "%PYEXE%" (
  echo Edit GSV_HOME in this bat. Need webui.py and runtime\python.exe in that folder.
  pause
  exit /b 1
)
set "GPT_FILE=GPT_weights_v3\satone-e20.ckpt"
set "SOVITS_FILE=SoVITS_weights_v3\satone_e2_s2492_l64.pth"
if not exist "%CD%\chill_mod_tts_server.py" (
  echo Copy chill_mod_tts_server.py from repo GPTSoVITSBridge into this folder first.
  pause
  exit /b 1
)
echo Starting TTS...
"%PYEXE%" "%CD%\chill_mod_tts_server.py" --gptsovits-home "%CD%" --gpt "%GPT_FILE%" --sovits "%SOVITS_FILE%" --preload
pause
