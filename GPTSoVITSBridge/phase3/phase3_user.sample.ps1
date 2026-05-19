# Copy to phase3_user.ps1 and edit. Optional if you always pass -RefWav to Run_TTS_Http_Test.ps1.

$Phase3_TtsBaseUrl = "http://127.0.0.1:9880"

# Reference wav (same as Mod Audio_File_Path / tts_ref.wav; transcript must match prompt).
$Phase3_RefWav = "D:\path\to\your\tts_ref.wav"

# Must match reference clip (default below = AIMod Audio_File_Text).
$Phase3_PromptText = "君が集中した時のシータ波を検出して、リンクをつなぎ直せば元通りになるはず。"
$Phase3_PromptLang = "ja"

$Phase3_TestText = "おはよう"
$Phase3_TestTextLang = "ja"

$Phase3_OutWav = ""
