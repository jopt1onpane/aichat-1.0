' Launch ChillTTSLauncher.bat from CHILL_GSV_HOME (set by Mod from GptSovits_Portable_Root). ASCII only.
Dim fso, batPath, sh, home
Set sh = CreateObject("WScript.Shell")
Set fso = CreateObject("Scripting.FileSystemObject")
home = Trim(sh.Environment("PROCESS")("CHILL_GSV_HOME"))
If Len(home) = 0 Then
  WScript.Echo "CHILL_GSV_HOME is empty. Set GptSovits_Portable_Root in cfg."
  WScript.Quit 1
End If
batPath = fso.BuildPath(home, "ChillTTSLauncher.bat")
If Not fso.FileExists(batPath) Then
  WScript.Echo "Missing: " & batPath & " — run Deploy-ChillRuntime.ps1 on that folder."
  WScript.Quit 1
End If
sh.Run "cmd /k " & Chr(34) & batPath & Chr(34), 1, False
