Phase 3 test kit (same HTTP as in-game TTS)

1) Optional: copy phase3_user.sample.ps1 -> phase3_user.ps1 and set Phase3_RefWav (must match prompt text in cfg).

2) Start TTS: portable runtime\python.exe chill_mod_tts_server.py ... (see TTSBridge\DEPLOY.txt).

3) Double-click CLICKME_Phase3.bat
   Or: powershell -ExecutionPolicy Bypass -File Run_Deploy_Check.ps1 -GameInstallRoot "C:\...\Chill with You Lo-Fi Story"

4) HTTP test: do NOT double-click the .ps1 (Windows may open Notepad).
   Double-click Run_TTS_Http_Test.bat instead.
   Or: powershell -ExecutionPolicy Bypass -File Run_TTS_Http_Test.ps1 -RefWav "C:\path\to\tts_ref.wav"

5) Game: no need to wait for the phase3 script. Keep TTS server running; start the game whenever you like.
   Trigger dialogue when both server and Mod are ready.

Output: phase3_test_out.wav (next to these scripts). Uses Invoke-WebRequest (not curl) so paths with spaces are OK.
