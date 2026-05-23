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

            StopServiceByPortAndKeywords(
                "TTS Cleanup",
                listenPort,
                "chill_mod_tts_server",
                "ChillTTSLauncher.bat",
                "ChillTTSLauncher.ps1",
                "Run_ChillTTSLauncher",
                "Run_ChillMod_TTS");
        }

        /// <summary>
        /// 结束 Mod 启动的本地 LLM（llama-server）服务：监听端口 + 命令行关键字匹配。
        /// </summary>
        public static void StopChillModLlmService(int chatPort, int embedPort)
        {
            int[] ports = (chatPort == embedPort) ? new[] { chatPort } : new[] { chatPort, embedPort };
            StopServiceByPortAndKeywords(
                "LLM Cleanup",
                ports,
                "llama-server.exe",
                "ChillLLMBundle",
                "chill_mod_llm");
        }

        /// <summary>
        /// 通用清理：监听端口上的进程 + 命令行包含关键字的进程。
        /// </summary>
        public static void StopServiceByPortAndKeywords(string tag, int listenPort, params string[] keywords)
        {
            StopServiceByPortAndKeywords(tag, new[] { listenPort }, keywords);
        }

        public static void StopServiceByPortAndKeywords(string tag, int[] listenPorts, params string[] keywords)
        {
            if (string.IsNullOrEmpty(tag)) tag = "Cleanup";
            Log.Info($"[{tag}] 开始（端口 {string.Join(",", listenPorts)} / 关键字 {keywords?.Length ?? 0} 个）…");

            if (listenPorts != null)
            {
                foreach (int p in listenPorts)
                {
                    if (p <= 0 || p > 65535) continue;
                    foreach (int pid in GetListeningPidsOnPort(p))
                        KillProcessTreeByPid(pid, $"port-listener:{p}", tag);
                }
            }

            if (keywords != null && keywords.Length > 0)
                KillProcessesWhoseCommandLineContains(tag, keywords);

            Log.Info($"[{tag}] 关闭流程已执行");
        }

        public static void KillProcessTree(Process process)
        {
            if (process == null || process.HasExited) return;
            KillProcessTreeByPid(process.Id, "tracked-process");
        }

        public static void KillProcessTreeByPid(int pid, string reason = null, string logTag = "Cleanup")
        {
            if (pid <= 0) return;
            try
            {
                string suffix = string.IsNullOrEmpty(reason) ? "" : $" ({reason})";
                Log.Info($"[{logTag}] taskkill /T /F /PID {pid}{suffix}");
                RunTaskKill($"/T /F /PID {pid}");
            }
            catch (Exception ex)
            {
                Log.Warning($"[{logTag}] 终止 PID {pid} 失败: {ex.Message}");
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

        private static void KillProcessesWhoseCommandLineContains(string logTag, params string[] fragments)
        {
            if (fragments == null || fragments.Length == 0) return;
            string[] keys = fragments.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
            if (keys.Length == 0) return;
            if (string.IsNullOrEmpty(logTag)) logTag = "Cleanup";

            string keyLines = string.Join(Environment.NewLine, keys.Select(k => "    '" + k.Replace("'", "''") + "'"));
            string script =
                "$keys = @(\r\n" + keyLines + "\r\n)\r\n" +
                "Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object {\r\n" +
                "  $c = $_.CommandLine\r\n" +
                "  if (-not $c) { return $false }\r\n" +
                "  foreach ($k in $keys) { if ($c -like (\"*\" + $k + \"*\")) { return $true } }\r\n" +
                "  return $false\r\n" +
                "} | ForEach-Object {\r\n" +
                "  Write-Host (\"[" + logTag + "] kill \" + $_.ProcessId + \" \" + $_.Name)\r\n" +
                "  & taskkill.exe /T /F /PID $_.ProcessId 2>$null | Out-Null\r\n" +
                "}\r\n";

            string safeTag = new string(logTag.Where(char.IsLetterOrDigit).ToArray());
            if (string.IsNullOrEmpty(safeTag)) safeTag = "cleanup";
            string tempPs = Path.Combine(Path.GetTempPath(), "chill_mod_" + safeTag + "_stop_" + Process.GetCurrentProcess().Id + ".ps1");
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
                Log.Warning($"[{logTag}] 按命令行清理失败: {ex.Message}");
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
