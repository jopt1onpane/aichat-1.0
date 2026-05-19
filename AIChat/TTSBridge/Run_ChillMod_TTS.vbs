' Launch ChillMod_TTS_v3.bat via cmd /k (ASCII only).
Dim fso, batPath, sh
Set fso = CreateObject("Scripting.FileSystemObject")
batPath = fso.BuildPath(fso.GetParentFolderName(WScript.ScriptFullName), "ChillMod_TTS_v3.bat")
Set sh = CreateObject("WScript.Shell")
sh.Run "cmd /k " & Chr(34) & batPath & Chr(34), 1, False
