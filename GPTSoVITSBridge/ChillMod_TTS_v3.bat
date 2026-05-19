@echo off
REM ASCII-only: cmd.exe uses system ANSI; UTF-8 Chinese breaks parsing.
setlocal
REM Portable root (contains runtime, chill_mod_tts_server.py). Edit if you moved the folder.
set "GSV_HOME=C:\Users\tempadmin\Downloads\GPT-SoVITS-v3lora-20250228\GPT-SoVITS-v3lora-20250228"

pushd "%GSV_HOME%" 2>nul
if errorlevel 1 goto :bad_cd

set "PYEXE=%CD%\runtime\python.exe"
if not exist "%PYEXE%" goto :no_py

set "GPT_FILE=GPT_weights_v3\satone-e20.ckpt"
set "SOVITS_FILE=SoVITS_weights_v3\satone_e2_s2492_l64.pth"

if not exist "%CD%\chill_mod_tts_server.py" goto :no_script

echo [ChillBridge] HOME=%CD%
echo [ChillBridge] Starting TTS; first load can take 60s+. Keep this window open.

"%PYEXE%" "%CD%\chill_mod_tts_server.py" --gptsovits-home "%CD%" --gpt "%GPT_FILE%" --sovits "%SOVITS_FILE%" --preload
set ERR=%ERRORLEVEL%
if not "%ERR%"=="0" (
  echo [ChillBridge] Python exit code %ERR%. Scroll up for errors.
  goto :end
)
goto :eof

:bad_cd
echo [ChillBridge] Cannot cd to GSV_HOME. Edit GSV_HOME in this .bat:
echo %GSV_HOME%
goto :end

:no_py
echo [ChillBridge] Missing: %PYEXE%
echo Current dir (need runtime here): %CD%
goto :end

:no_script
echo [ChillBridge] Missing chill_mod_tts_server.py in: %CD%
goto :end

:end
pause
exit /b 1
