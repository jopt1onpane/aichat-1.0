using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AIChat.Utils;

namespace AIChat.Services
{
    public static class ProcessHelper
    {
        /// <summary>
        /// 结束 Mod 启动的 GPT-SoVITS TTS：监听端口（python/uvicorn）+ Chill 启动器相关 cmd/powershell 窗口。
        /// </summary>
        public static void StopChillModTtsService(int listenPort)
        {
            if (listenPort <= 0 || listenPort > 65535)
                listenPort = 9880;

            Log.Info($"[TTS Cleanup] 开始关闭 TTS 服务（端口 {listenPort}）…");

            foreach (int pid in GetListeningPidsOnPort(listenPort))
                KillProcessTreeByPid(pid, "port-listener");

            KillProcessesWhoseCommandLineContains(
                "chill_mod_tts_server",
                "ChillTTSLauncher.bat",
                "ChillTTSLauncher.ps1",
                "Run_ChillTTSLauncher",
                "Run_ChillMod_TTS");

            Log.Info("[TTS Cleanup] TTS 关闭流程已执行");
        }

        public static void KillProcessTree(Process process)
        {
            if (process == null || process.HasExited) return;
            KillProcessTreeByPid(process.Id, "tracked-process");
        }

        public static void KillProcessTreeByPid(int pid, string reason = null)
        {
            if (pid <= 0) return;
            try
            {
                string tag = string.IsNullOrEmpty(reason) ? "" : $" ({reason})";
                Log.Info($"[TTS Cleanup] taskkill /T /F /PID {pid}{tag}");
                RunTaskKill($"/T /F /PID {pid}");
            }
            catch (Exception ex)
            {
                Log.Warning($"[TTS Cleanup] 终止 PID {pid} 失败: {ex.Message}");
            }
        }

        private static HashSet<int> GetListeningPidsOnPort(int port)
        {
            var pids = new HashSet<int>();
            string portNeedle = ":" + port;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "netstat.exe",
                    Arguments = "-ano",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var proc = Process.Start(psi))
                {
                    if (proc == null) return pids;
                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(8000);
                    foreach (string line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (line.IndexOf("LISTENING", StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                        if (line.IndexOf(portNeedle, StringComparison.Ordinal) < 0)
                            continue;
                        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 0) continue;
                        if (int.TryParse(parts[parts.Length - 1], out int pid) && pid > 0)
                            pids.Add(pid);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[TTS Cleanup] netstat 失败: {ex.Message}");
            }
            return pids;
        }

        private static void KillProcessesWhoseCommandLineContains(params string[] fragments)
        {
            if (fragments == null || fragments.Length == 0) return;
            string[] keys = fragments.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
            if (keys.Length == 0) return;

            string keyLines = string.Join(Environment.NewLine, keys.Select(k => "    '" + k.Replace("'", "''") + "'"));
            string script =
                "$keys = @(\r\n" + keyLines + "\r\n)\r\n" +
                "Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object {\r\n" +
                "  $c = $_.CommandLine\r\n" +
                "  if (-not $c) { return $false }\r\n" +
                "  foreach ($k in $keys) { if ($c -like (\"*\" + $k + \"*\")) { return $true } }\r\n" +
                "  return $false\r\n" +
                "} | ForEach-Object {\r\n" +
                "  Write-Host (\"[TTS Cleanup] kill \" + $_.ProcessId + \" \" + $_.Name)\r\n" +
                "  & taskkill.exe /T /F /PID $_.ProcessId 2>$null | Out-Null\r\n" +
                "}\r\n";

            string tempPs = Path.Combine(Path.GetTempPath(), "chill_mod_tts_stop_" + Process.GetCurrentProcess().Id + ".ps1");
            try
            {
                File.WriteAllText(tempPs, script);
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass -File \"" + tempPs + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var proc = Process.Start(psi))
                {
                    if (proc == null) return;
                    string stdout = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(15000);
                    if (!string.IsNullOrWhiteSpace(stdout))
                        Log.Info(stdout.Trim());
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[TTS Cleanup] 按命令行清理失败: {ex.Message}");
            }
            finally
            {
                try { if (File.Exists(tempPs)) File.Delete(tempPs); } catch { }
            }
        }

        private static void RunTaskKill(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "taskkill.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using (var killer = Process.Start(psi))
            {
                killer?.WaitForExit(5000);
            }
        }
    }
}
