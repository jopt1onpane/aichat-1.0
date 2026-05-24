using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AIChat.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace AIChat.Services
{
    /// <summary>
    /// 内置 LLM（llama-server）启动器：管理 chat + embed 两个本地推理进程。
    /// 启动时如果 bundle 完整就直接拉起；模型缺失就跳过、留给「下载资源」按钮补齐。
    /// 用 CreateNoWindow=true 直接调 llama-server.exe，绝不弹控制台。
    /// </summary>
    public static class LLMServerLauncher
    {
        public enum LaunchResult
        {
            Started,
            AlreadyRunning,
            EngineMissing,
            ModelMissing,
            Failed,
            Disabled
        }

        public class ServerHandle
        {
            public string Role;         // "chat" / "embed"
            public string ModelId;
            public int Port;
            public Process Process;
            public bool ShowConsole;
        }

        // 单例存活的两条管道
        private static ServerHandle _chat;
        private static ServerHandle _embed;

        public static ServerHandle ChatHandle => _chat;
        public static ServerHandle EmbedHandle => _embed;

        // ============================================================
        // 启动 / 停止
        // ============================================================

        /// <summary>
        /// 启动指定角色的 llama-server 实例（chat 或 embed）。
        /// </summary>
        public static LaunchResult StartRole(
            string bundleRoot,
            string role,
            int port,
            bool showConsole,
            out string detail)
        {
            detail = "";
            if (string.IsNullOrWhiteSpace(bundleRoot) || !Directory.Exists(bundleRoot))
            {
                detail = $"bundleRoot 不存在: {bundleRoot}";
                return LaunchResult.EngineMissing;
            }

            string engineExe;
            string engineDir;
            if (!TryResolveLlamaServerExe(bundleRoot, out engineExe, out engineDir))
            {
                detail = $"未找到 llama-server.exe（请在 ChillLLMBundle\\engine\\ 下放置，或解压到其子文件夹如 llama-b9297-bin-win-vulkan-x64\\）";
                return LaunchResult.EngineMissing;
            }
            string enginePath = engineExe;

            string manifestPath = Path.Combine(bundleRoot, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                detail = $"未找到 manifest.json: {manifestPath}";
                return LaunchResult.EngineMissing;
            }

            ManifestModel model;
            try
            {
                model = LLMBundleManifest.FindDefaultModel(File.ReadAllText(manifestPath), role);
            }
            catch (Exception ex)
            {
                detail = $"manifest 解析失败: {ex.Message}";
                return LaunchResult.Failed;
            }

            if (model == null)
            {
                detail = $"manifest 中无 role={role} 的默认模型";
                return LaunchResult.Failed;
            }

            string modelPath = Path.Combine(bundleRoot, "models", model.File);
            if (!File.Exists(modelPath))
            {
                detail = $"模型文件未下载: {modelPath}";
                return LaunchResult.ModelMissing;
            }

            ServerHandle existing = role == "chat" ? _chat : _embed;
            if (existing != null && existing.Process != null && !existing.Process.HasExited)
            {
                detail = $"role={role} 已在 PID={existing.Process.Id} 运行";
                return LaunchResult.AlreadyRunning;
            }

            string args = LLMBundleManifest.RenderArgs(model.ServerArgs, port, modelPath);
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = enginePath,
                    Arguments = args,
                    WorkingDirectory = engineDir,
                    UseShellExecute = false,
                    CreateNoWindow = !showConsole,
                    WindowStyle = showConsole ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                    // 显示控制台时不重定向（让 stdout 直接出现在控制台中）；隐藏时重定向避免管道阻塞
                    RedirectStandardOutput = !showConsole,
                    RedirectStandardError = !showConsole
                };

                Process proc = Process.Start(psi);
                if (proc == null)
                {
                    detail = "Process.Start 返回 null";
                    return LaunchResult.Failed;
                }

                var handle = new ServerHandle
                {
                    Role = role,
                    ModelId = model.Id,
                    Port = port,
                    Process = proc,
                    ShowConsole = showConsole
                };
                if (role == "chat") _chat = handle; else _embed = handle;

                // 隐藏控制台模式下：开 daemon 线程持续 drain 输出，防止管道满了把 server 卡住
                if (!showConsole)
                {
                    DrainAsync(proc.StandardOutput, $"[LLM:{role}]");
                    DrainAsync(proc.StandardError, $"[LLM:{role}:err]");
                }

                detail = $"PID={proc.Id} 端口={port} 模型={model.Id} 控制台={(showConsole ? "显示" : "隐藏")}";
                return LaunchResult.Started;
            }
            catch (Exception ex)
            {
                detail = $"启动失败: {ex.Message}";
                return LaunchResult.Failed;
            }
        }

        public static void StopRole(string role)
        {
            ServerHandle h = role == "chat" ? _chat : _embed;
            if (h == null) return;
            try
            {
                if (h.Process != null && !h.Process.HasExited)
                    ProcessHelper.KillProcessTree(h.Process);
            }
            catch (Exception ex)
            {
                Log.Warning($"[LLM:{role}] StopRole 异常: {ex.Message}");
            }
            if (role == "chat") _chat = null; else _embed = null;
        }

        public static void StopAll()
        {
            StopRole("chat");
            StopRole("embed");
        }

        public static bool IsAlive(string role)
        {
            ServerHandle h = role == "chat" ? _chat : _embed;
            return h != null && h.Process != null && !h.Process.HasExited;
        }

        /// <summary>
        /// 协程：等待指定端口的 llama-server /health 返回 200，超时则失败。
        /// 503 = 模型仍在加载，会继续轮询。
        /// </summary>
        public static IEnumerator WaitHealthyAsync(
            int port,
            float timeoutSeconds,
            Action<bool> onDone,
            Action<float, long> onTick = null,
            Func<bool> isProcessAlive = null)
        {
            string url = $"http://127.0.0.1:{port}/health";
            float start = Time.realtimeSinceStartup;
            long lastLoggedSec = -1;

            while (Time.realtimeSinceStartup - start < timeoutSeconds)
            {
                float elapsed = Time.realtimeSinceStartup - start;
                onTick?.Invoke(elapsed, port);

                if (isProcessAlive != null && !isProcessAlive())
                {
                    Log.Error($"[LLM] 端口 {port} 的 llama-server 进程已退出，健康检查中止");
                    onDone?.Invoke(false);
                    yield break;
                }

                using (var req = UnityWebRequest.Get(url))
                {
                    req.timeout = 5;
                    yield return req.SendWebRequest();

                    if (req.result == UnityWebRequest.Result.Success && req.responseCode == 200)
                    {
                        Log.Info($"[LLM] 端口 {port} 健康检查通过（耗时 {elapsed:F0}s）");
                        onDone?.Invoke(true);
                        yield break;
                    }

                    long sec = (long)elapsed;
                    if (sec != lastLoggedSec && (sec % 15 == 0 || sec <= 3))
                    {
                        lastLoggedSec = sec;
                        Log.Info($"[LLM] 端口 {port} 等待就绪… {sec}s / {timeoutSeconds:F0}s（HTTP {(long)req.responseCode}，503=模型加载中）");
                    }
                }
                yield return new WaitForSeconds(1.0f);
            }

            Log.Warning($"[LLM] 端口 {port} 健康检查超时（{timeoutSeconds:F0}s）");
            onDone?.Invoke(false);
        }

        // ============================================================
        // 工具
        // ============================================================

        /// <summary>
        /// 解析 llama-server.exe：优先 engine\llama-server.exe，否则在 engine\ 下递归搜索（兼容解压到子文件夹）。
        /// </summary>
        public static bool TryResolveLlamaServerExe(string bundleRoot, out string exePath, out string workingDir)
        {
            exePath = null;
            workingDir = null;
            if (string.IsNullOrWhiteSpace(bundleRoot)) return false;

            string engineRoot = Path.Combine(bundleRoot, "engine");
            if (!Directory.Exists(engineRoot)) return false;

            string direct = Path.Combine(engineRoot, "llama-server.exe");
            if (File.Exists(direct))
            {
                exePath = direct;
                workingDir = engineRoot;
                return true;
            }

            try
            {
                foreach (string found in Directory.GetFiles(engineRoot, "llama-server.exe", SearchOption.AllDirectories))
                {
                    exePath = found;
                    workingDir = Path.GetDirectoryName(found) ?? engineRoot;
                    return true;
                }
            }
            catch { }

            return false;
        }

        private static void DrainAsync(System.IO.StreamReader reader, string prefix)
        {
            if (reader == null) return;
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // 不全量打日志，只打错误/警告行，避免淹没
                        if (line.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0
                            || line.IndexOf("warn", StringComparison.OrdinalIgnoreCase) >= 0
                            || line.IndexOf("exception", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            Log.Warning($"{prefix} {line}");
                        }
                    }
                }
                catch { /* server 退出时 reader 关闭，安静吃掉 */ }
            });
        }
    }

    // ============================================================
    // 轻量 manifest 解析（不引入 Newtonsoft.Json 依赖）
    // ============================================================

    public class ManifestModel
    {
        public string Id;
        public string Role;
        public string File;
        public long SizeBytes;
        public string Sha256;
        public List<string> Urls = new List<string>();
        public List<string> ServerArgs = new List<string>();
        public bool IsDefault;
        public string DisplayName;
    }

    /// <summary>
    /// 极简 manifest.json 解析。只支持本 Mod 自己写出的格式，不追求通用 JSON 解析器。
    /// </summary>
    public static class LLMBundleManifest
    {
        public static List<ManifestModel> ParseModels(string json)
        {
            var result = new List<ManifestModel>();
            if (string.IsNullOrEmpty(json)) return result;

            int modelsIdx = json.IndexOf("\"models\"", StringComparison.Ordinal);
            if (modelsIdx < 0) return result;

            int arrStart = json.IndexOf('[', modelsIdx);
            if (arrStart < 0) return result;
            int arrEnd = FindMatchingBracket(json, arrStart, '[', ']');
            if (arrEnd < 0) return result;

            // 切出每个 { ... } 对象
            int i = arrStart + 1;
            while (i < arrEnd)
            {
                while (i < arrEnd && json[i] != '{') i++;
                if (i >= arrEnd) break;
                int objStart = i;
                int objEnd = FindMatchingBracket(json, objStart, '{', '}');
                if (objEnd < 0) break;

                string objJson = json.Substring(objStart, objEnd - objStart + 1);
                var m = new ManifestModel
                {
                    Id = ReadString(objJson, "id"),
                    Role = ReadString(objJson, "role"),
                    File = ReadString(objJson, "file"),
                    Sha256 = ReadString(objJson, "sha256"),
                    DisplayName = ReadString(objJson, "display_name"),
                    SizeBytes = ReadLong(objJson, "size_bytes"),
                    IsDefault = ReadBool(objJson, "default"),
                    Urls = ReadStringArray(objJson, "urls"),
                    ServerArgs = ReadStringArray(objJson, "server_args"),
                };
                result.Add(m);

                i = objEnd + 1;
            }

            return result;
        }

        public static ManifestModel FindDefaultModel(string json, string role)
        {
            var all = ParseModels(json);
            ManifestModel any = null;
            foreach (var m in all)
            {
                if (m.Role != role) continue;
                if (any == null) any = m;
                if (m.IsDefault) return m;
            }
            return any;
        }

        public static string RenderArgs(List<string> template, int port, string modelPath)
        {
            if (template == null) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < template.Count; i++)
            {
                if (i > 0) sb.Append(' ');
                string a = template[i] ?? "";
                a = a.Replace("{port}", port.ToString());
                a = a.Replace("{model_path}", modelPath);
                bool needQuote = a.IndexOf(' ') >= 0 && !a.StartsWith("\"");
                if (needQuote) sb.Append('"').Append(a).Append('"');
                else sb.Append(a);
            }
            return sb.ToString();
        }

        // ---------- 解析小工具 ----------

        private static int FindMatchingBracket(string s, int openIdx, char open, char close)
        {
            int depth = 0;
            bool inStr = false;
            bool esc = false;
            for (int i = openIdx; i < s.Length; i++)
            {
                char c = s[i];
                if (esc) { esc = false; continue; }
                if (c == '\\' && inStr) { esc = true; continue; }
                if (c == '"') { inStr = !inStr; continue; }
                if (inStr) continue;
                if (c == open) depth++;
                else if (c == close)
                {
                    depth--;
                    if (depth == 0) return i;
                }
            }
            return -1;
        }

        private static string ReadString(string obj, string key)
        {
            int k = obj.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
            if (k < 0) return "";
            int colon = obj.IndexOf(':', k);
            if (colon < 0) return "";
            int q1 = obj.IndexOf('"', colon + 1);
            if (q1 < 0) return "";
            var sb = new StringBuilder();
            for (int i = q1 + 1; i < obj.Length; i++)
            {
                char c = obj[i];
                if (c == '\\' && i + 1 < obj.Length)
                {
                    char n = obj[i + 1];
                    if (n == 'n') sb.Append('\n');
                    else if (n == 't') sb.Append('\t');
                    else if (n == 'r') sb.Append('\r');
                    else sb.Append(n);
                    i++;
                    continue;
                }
                if (c == '"') break;
                sb.Append(c);
            }
            return sb.ToString();
        }

        private static long ReadLong(string obj, string key)
        {
            int k = obj.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
            if (k < 0) return 0;
            int colon = obj.IndexOf(':', k);
            if (colon < 0) return 0;
            int i = colon + 1;
            while (i < obj.Length && (obj[i] == ' ' || obj[i] == '\t')) i++;
            int j = i;
            while (j < obj.Length && (char.IsDigit(obj[j]) || obj[j] == '-')) j++;
            if (j == i) return 0;
            long.TryParse(obj.Substring(i, j - i), out long v);
            return v;
        }

        private static bool ReadBool(string obj, string key)
        {
            int k = obj.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
            if (k < 0) return false;
            int colon = obj.IndexOf(':', k);
            if (colon < 0) return false;
            int t = obj.IndexOf("true", colon, StringComparison.Ordinal);
            int f = obj.IndexOf("false", colon, StringComparison.Ordinal);
            int comma = obj.IndexOf(',', colon);
            int brace = obj.IndexOf('}', colon);
            int boundary = (comma >= 0 && (brace < 0 || comma < brace)) ? comma : (brace >= 0 ? brace : obj.Length);
            return (t >= 0 && t < boundary);
        }

        private static List<string> ReadStringArray(string obj, string key)
        {
            var list = new List<string>();
            int k = obj.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
            if (k < 0) return list;
            int arrStart = obj.IndexOf('[', k);
            if (arrStart < 0) return list;
            int arrEnd = FindMatchingBracket(obj, arrStart, '[', ']');
            if (arrEnd < 0) return list;

            int i = arrStart + 1;
            while (i < arrEnd)
            {
                while (i < arrEnd && obj[i] != '"') i++;
                if (i >= arrEnd) break;
                var sb = new StringBuilder();
                i++;
                while (i < arrEnd && obj[i] != '"')
                {
                    if (obj[i] == '\\' && i + 1 < arrEnd) { sb.Append(obj[i + 1]); i += 2; continue; }
                    sb.Append(obj[i]); i++;
                }
                list.Add(sb.ToString());
                i++;
            }
            return list;
        }
    }
}
