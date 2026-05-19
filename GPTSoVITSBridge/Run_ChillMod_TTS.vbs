' ASCII-only: avoids VBScript mis-p parsing UTF-8 / fullwidth quotes in comments.
' Launch ChillMod_TTS_v3.bat with cmd /k (path under Program Files (x86) safe).
Dim fso, batPath, sh
Set fso = CreateObject("Scripting.FileSystemObject")
batPath = fso.BuildPath(fso.GetParentFolderName(WScript.ScriptFullName), "ChillMod_TTS_v3.bat")
Set sh = CreateObject("WScript.Shell")
sh.Run "cmd /k " & Chr(34) & batPath & Chr(34), 1, False
