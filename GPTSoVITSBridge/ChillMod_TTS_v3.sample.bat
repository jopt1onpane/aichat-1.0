@echo off
setlocal ENABLEDELAYEDEXPANSION
REM 阶段 2：把本 BAT 与同目录 chill_mod_tts_server.py 复制到「GPT-SoVITS v3 便携包根目录」
REM （即该目录内可见 webui.py、GPT_SoVITS\、runtime\）。
REM 修改下面两行为你实际的权重文件名，然后可从 Mod  cfg 指向本 BAT。

cd /d "%~dp0"
set PYTHON="%CD%\runtime\python.exe"
if not exist %PYTHON% (
  echo [ChillBridge] Missing runtime\python.exe. Place this BAT in GPT-SoVITS portable ROOT.
  exit /b 1
)

set GPT_FILE=GPT_weights_v3\CHANGE_ME.ckpt
set SOVITS_FILE=SoVITS_weights_v3\CHANGE_ME.pth

%PYTHON% "%CD%\chill_mod_tts_server.py" --gptsovits-home "%CD%" --gpt "!GPT_FILE!" --sovits "!SOVITS_FILE!" --preload
exit /b %ERRORLEVEL%
