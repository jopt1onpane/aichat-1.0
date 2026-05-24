using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using AIChat.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace AIChat.Services
{
    /// <summary>
    /// 模型资源下载服务：支持多镜像 fallback、HTTP Range 断点续传、SHA256 校验。
    ///
    /// 用法（协程）：
    ///   yield return ModelDownloadService.DownloadModelAsync(bundleRoot, model, progressCb);
    /// </summary>
    public static class ModelDownloadService
    {
        public class Progress
        {
            public string ModelId;
            public string ModelDisplayName;
            public string Phase;          // "downloading" / "verifying" / "done" / "failed"
            public long   Downloaded;     // 已下载字节
            public long   Total;          // 总字节（来自 manifest，若未知 = 0）
            public float  SpeedBytesPerSec;
            public string CurrentUrl;
            public string Detail;
            public bool   Failed;
            public bool   Done;
        }

        // 由 AIMod 在配置绑定后写入，供下载时注入 Authorization 头（可选）
        public static string HuggingFaceToken = "";

        /// <summary>
        /// 串行下载 manifest 中所有 role=chat/embed 的默认模型。
        /// </summary>
        public static IEnumerator DownloadAllDefaultModelsAsync(
            string bundleRoot,
            Action<Progress> onProgress,
            Action<bool, string> onDone)
        {
            if (string.IsNullOrEmpty(bundleRoot) || !Directory.Exists(bundleRoot))
            {
                onDone?.Invoke(false, $"bundleRoot 不存在: {bundleRoot}");
                yield break;
            }

            string manifestPath = Path.Combine(bundleRoot, "manifest.json");
            if (!File.Exists(manifestPath))
            {
                onDone?.Invoke(false, $"未找到 manifest.json: {manifestPath}");
                yield break;
            }

            List<ManifestModel> models;
            try
            {
                models = LLMBundleManifest.ParseModels(File.ReadAllText(manifestPath));
            }
            catch (Exception ex)
            {
                onDone?.Invoke(false, $"manifest 解析失败: {ex.Message}");
                yield break;
            }

            string modelsDir = Path.Combine(bundleRoot, "models");
            try { Directory.CreateDirectory(modelsDir); } catch { }

            foreach (var m in models)
            {
                if (m == null || string.IsNullOrEmpty(m.File)) continue;
                if (m.Role != "chat" && m.Role != "embed") continue;
                if (!m.IsDefault) continue;

                string targetPath = Path.Combine(modelsDir, m.File);
                bool ok = false;
                string err = null;
                yield return DownloadModelAsync(modelsDir, m, onProgress,
                    (success, detail) => { ok = success; err = detail; });
                if (!ok)
                {
                    onDone?.Invoke(false, $"模型 {m.Id} 下载失败：{err}");
                    yield break;
                }
            }

            onDone?.Invoke(true, "全部默认模型已就绪");
        }

        /// <summary>
        /// 下载单个模型：依次尝试 model.urls 列表，每个都支持断点续传。
        /// 全部失败时回调 (false, errorDetail)。
        /// </summary>
        public static IEnumerator DownloadModelAsync(
            string modelsDir,
            ManifestModel model,
            Action<Progress> onProgress,
            Action<bool, string> onDone)
        {
            string targetPath = Path.Combine(modelsDir, model.File);
            string partPath = targetPath + ".part";

            // 已经下载并校验通过的，直接跳过
            if (File.Exists(targetPath))
            {
                bool needRedownload = false;
                if (!LooksLikeGguf(targetPath))
                {
                    Log.Warning($"[Download] 已存在 {model.File} 但文件头不是 GGUF，重新下载");
                    try { File.Delete(targetPath); } catch { }
                    needRedownload = true;
                }
                else if (model.SizeBytes > 0)
                {
                    long len = SafeLength(targetPath);
                    if (len != model.SizeBytes && len > 0)
                    {
                        Log.Warning($"[Download] 已存在 {model.File} 但大小不符 ({len} vs 期望 {model.SizeBytes})，重新下载");
                        try { File.Delete(targetPath); } catch { }
                        needRedownload = true;
                    }
                }
                if (!needRedownload)
                {
                    onProgress?.Invoke(new Progress { ModelId = model.Id, ModelDisplayName = model.DisplayName,
                        Phase = "done", Downloaded = SafeLength(targetPath), Total = model.SizeBytes, Done = true });
                    onDone?.Invoke(true, "已存在");
                    yield break;
                }
            }

            string lastErr = "无候选 URL";
            foreach (string url in model.Urls)
            {
                if (string.IsNullOrWhiteSpace(url)) continue;

                // 决定是否附带 HF token
                string token = null;
                bool isHfUrl = url.IndexOf("huggingface.co", StringComparison.OrdinalIgnoreCase) >= 0
                            || url.IndexOf("hf-mirror.com",  StringComparison.OrdinalIgnoreCase) >= 0;
                if (isHfUrl && !string.IsNullOrWhiteSpace(HuggingFaceToken))
                    token = HuggingFaceToken.Trim();

                bool urlOk = false;
                string urlErr = null;
                const int maxRetriesPerUrl = 3;

                for (int attempt = 1; attempt <= maxRetriesPerUrl && !urlOk; attempt++)
                {
                    long resumeFrom = SafeLength(partPath);
                    // .part 头部若不是 GGUF，说明之前镜像返回了 HTML/鉴权错误页，必须清掉重下
                    if (resumeFrom > 0 && !LooksLikeGguf(partPath))
                    {
                        Log.Warning($"[Download] {model.File} 的 .part 文件头无效（可能为鉴权错误页），清除后从头下载");
                        try { File.Delete(partPath); } catch { }
                        resumeFrom = 0;
                    }

                    if (resumeFrom > 0)
                        Log.Info($"[Download] 断点续传 {model.File} ← {url} （已下载 {FormatBytes(resumeFrom)}，第 {attempt}/{maxRetriesPerUrl} 次）");
                    else
                        Log.Info($"[Download] 开始下载 {model.File} ← {url} （第 {attempt}/{maxRetriesPerUrl} 次）");

                    yield return DownloadFromUrlAsync(url, partPath, resumeFrom, model.SizeBytes, token,
                        (p) =>
                        {
                            p.ModelId = model.Id;
                            p.ModelDisplayName = model.DisplayName;
                            p.CurrentUrl = url;
                            p.Detail = resumeFrom > 0 ? "断点续传中" : null;
                            onProgress?.Invoke(p);
                        },
                        (success, detail) => { urlOk = success; urlErr = detail; });

                    if (urlOk) break;

                    // 416：服务端不支持 Range 或偏移无效 → 清掉 .part 从头再来一次
                    if (IsRangeNotSatisfiable(urlErr) && SafeLength(partPath) > 0)
                    {
                        Log.Warning($"[Download] {url} 不支持断点续传，已清除临时文件并从头下载");
                        try { File.Delete(partPath); } catch { }
                        yield return DownloadFromUrlAsync(url, partPath, 0, model.SizeBytes, token,
                            (p) =>
                            {
                                p.ModelId = model.Id;
                                p.ModelDisplayName = model.DisplayName;
                                p.CurrentUrl = url;
                                onProgress?.Invoke(p);
                            },
                            (success, detail) => { urlOk = success; urlErr = detail; });
                        if (urlOk) break;
                    }

                    if (!IsTransientNetworkError(urlErr) || attempt >= maxRetriesPerUrl)
                        break;

                    float waitSec = attempt * 2f;
                    Log.Warning($"[Download] {url} 临时失败：{urlErr}，{waitSec:F0}s 后从断点重试");
                    onProgress?.Invoke(new Progress
                    {
                        ModelId = model.Id,
                        ModelDisplayName = model.DisplayName,
                        Phase = "downloading",
                        Downloaded = SafeLength(partPath),
                        Total = model.SizeBytes,
                        CurrentUrl = url,
                        Detail = $"网络中断，{waitSec:F0}s 后自动续传…"
                    });
                    yield return new WaitForSeconds(waitSec);
                }

                if (!urlOk)
                {
                    Log.Warning($"[Download] {url} 失败：{urlErr}，尝试下一个镜像");
                    lastErr = urlErr;
                    // 切换镜像前清掉可能损坏的 .part，避免把 HTML 错误页拼进 GGUF
                    if (SafeLength(partPath) > 0 && !LooksLikeGguf(partPath))
                    {
                        try { File.Delete(partPath); } catch { }
                    }
                    continue;
                }

                // 下载完成后校验 GGUF 文件头（比 SHA256 更轻量，能拦截 HF 401 HTML 垃圾）
                if (!LooksLikeGguf(partPath))
                {
                    Log.Warning($"[Download] {url} 下载完成但文件头不是 GGUF（可能是鉴权/错误页），删除后尝试下一个镜像");
                    try { File.Delete(partPath); } catch { }
                    lastErr = "下载内容不是有效的 GGUF 模型文件（可能镜像返回了错误页）";
                    continue;
                }

                // 校验
                onProgress?.Invoke(new Progress
                {
                    ModelId = model.Id, ModelDisplayName = model.DisplayName,
                    Phase = "verifying", Downloaded = SafeLength(partPath), Total = model.SizeBytes, CurrentUrl = url
                });

                if (!string.IsNullOrWhiteSpace(model.Sha256)
                    && !model.Sha256.StartsWith("TODO", StringComparison.OrdinalIgnoreCase))
                {
                    string actual = null;
                    bool hashOk = false;
                    string hashErr = null;
                    yield return ComputeSha256Async(partPath, h => actual = h, e => hashErr = e);
                    if (!string.IsNullOrEmpty(hashErr))
                    {
                        Log.Warning($"[Download] 校验失败: {hashErr}");
                        lastErr = hashErr;
                        continue;
                    }
                    hashOk = string.Equals(actual, model.Sha256, StringComparison.OrdinalIgnoreCase);
                    if (!hashOk)
                    {
                        Log.Warning($"[Download] SHA256 不匹配，期望 {model.Sha256}，实际 {actual}");
                        try { File.Delete(partPath); } catch { }
                        lastErr = "SHA256 不匹配，可能传输损坏，已删除重试";
                        continue;
                    }
                }
                else
                {
                    Log.Warning($"[Download] manifest 未提供 sha256（{model.Sha256}），跳过校验");
                }

                // 重命名 .part → 最终文件
                try
                {
                    if (File.Exists(targetPath)) File.Delete(targetPath);
                    File.Move(partPath, targetPath);
                }
                catch (Exception ex)
                {
                    onDone?.Invoke(false, $"重命名失败: {ex.Message}");
                    yield break;
                }

                onProgress?.Invoke(new Progress
                {
                    ModelId = model.Id, ModelDisplayName = model.DisplayName,
                    Phase = "done", Downloaded = SafeLength(targetPath), Total = model.SizeBytes, Done = true
                });
                onDone?.Invoke(true, "下载完成");
                yield break;
            }

            onProgress?.Invoke(new Progress
            {
                ModelId = model.Id, ModelDisplayName = model.DisplayName,
                Phase = "failed", Failed = true, Detail = lastErr
            });
            onDone?.Invoke(false, lastErr);
        }

        // ============================================================
        // 单 URL 下载（支持 Range 断点续传）
        // ============================================================

        private static IEnumerator DownloadFromUrlAsync(
            string url, string partPath, long resumeFrom, long expectedTotal,
            string hfToken,
            Action<Progress> onProgress,
            Action<bool, string> onDone)
        {
            using (var req = new UnityWebRequest(url, "GET"))
            {
                req.downloadHandler = new DownloadHandlerFile(partPath, append: resumeFrom > 0)
                {
                    removeFileOnAbort = false
                };
                req.disposeDownloadHandlerOnDispose = true;
                req.timeout = 0;
                if (resumeFrom > 0)
                    req.SetRequestHeader("Range", "bytes=" + resumeFrom + "-");
                if (!string.IsNullOrEmpty(hfToken))
                {
                    req.SetRequestHeader("Authorization", "Bearer " + hfToken);
                    Log.Info("[Download] 已附加 HuggingFace Token");
                }

                var op = req.SendWebRequest();

                long downloadedAtStart = resumeFrom;
                float t0 = Time.realtimeSinceStartup;
                float tLast = t0;
                long lastBytes = resumeFrom;
                float lastProgressPushed = 0f;

                while (!op.isDone)
                {
                    long curBytes = downloadedAtStart + (long)req.downloadedBytes;
                    float now = Time.realtimeSinceStartup;
                    if (now - lastProgressPushed >= 0.5f)
                    {
                        float speed = 0f;
                        float dt = now - tLast;
                        if (dt > 0.1f)
                        {
                            speed = (curBytes - lastBytes) / dt;
                            lastBytes = curBytes;
                            tLast = now;
                        }
                        onProgress?.Invoke(new Progress
                        {
                            Phase = "downloading",
                            Downloaded = curBytes,
                            Total = expectedTotal,
                            SpeedBytesPerSec = speed,
                            CurrentUrl = url
                        });
                        lastProgressPushed = now;
                    }
                    yield return null;
                }

                if (req.result != UnityWebRequest.Result.Success)
                {
                    onDone?.Invoke(false, $"{req.error} ({req.responseCode})");
                    yield break;
                }

                // 简单 sanity check：若 manifest 有 size 且下载完比期望少，认为失败
                long finalLen = SafeLength(partPath);
                if (expectedTotal > 0 && finalLen < expectedTotal * 99 / 100)
                {
                    onDone?.Invoke(false, $"下载未完成（{finalLen}/{expectedTotal} bytes）");
                    yield break;
                }

                onDone?.Invoke(true, "");
            }
        }

        // ============================================================
        // SHA256 流式计算（边读边算，不占内存）
        // ============================================================

        private static IEnumerator ComputeSha256Async(string path, Action<string> onHash, Action<string> onError)
        {
            string hashHex = null;
            string err = null;
            bool finished = false;

            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    using (var sha = SHA256.Create())
                    using (var fs = File.OpenRead(path))
                    {
                        byte[] buf = new byte[1 << 20]; // 1MB
                        int n;
                        while ((n = fs.Read(buf, 0, buf.Length)) > 0)
                            sha.TransformBlock(buf, 0, n, null, 0);
                        sha.TransformFinalBlock(new byte[0], 0, 0);
                        var sb = new System.Text.StringBuilder();
                        foreach (byte b in sha.Hash) sb.Append(b.ToString("x2"));
                        hashHex = sb.ToString();
                    }
                }
                catch (Exception ex) { err = ex.Message; }
                finally { finished = true; }
            });

            while (!finished)
                yield return null;

            if (!string.IsNullOrEmpty(err))
                onError?.Invoke(err);
            else
                onHash?.Invoke(hashHex);
        }

        private static bool LooksLikeGguf(string path)
        {
            try
            {
                if (!File.Exists(path) || SafeLength(path) < 4) return false;
                using (var fs = File.OpenRead(path))
                {
                    var buf = new byte[4];
                    return fs.Read(buf, 0, 4) == 4
                        && buf[0] == (byte)'G' && buf[1] == (byte)'G'
                        && buf[2] == (byte)'U' && buf[3] == (byte)'F';
                }
            }
            catch { return false; }
        }

        private static long SafeLength(string path)
        {
            try { return File.Exists(path) ? new FileInfo(path).Length : 0; }
            catch { return 0; }
        }

        /// <summary>检测 models 目录下是否有未完成的 .part 临时文件。</summary>
        public static long GetIncompleteDownloadBytes(string modelsDir, string fileName)
        {
            if (string.IsNullOrEmpty(modelsDir) || string.IsNullOrEmpty(fileName)) return 0;
            return SafeLength(Path.Combine(modelsDir, fileName + ".part"));
        }

        private static bool IsRangeNotSatisfiable(string err)
        {
            return !string.IsNullOrEmpty(err)
                && err.IndexOf("(416)", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTransientNetworkError(string err)
        {
            if (string.IsNullOrEmpty(err)) return false;
            return err.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0
                || err.IndexOf("connection", StringComparison.OrdinalIgnoreCase) >= 0
                || err.IndexOf("network", StringComparison.OrdinalIgnoreCase) >= 0
                || err.IndexOf("abort", StringComparison.OrdinalIgnoreCase) >= 0
                || err.IndexOf("(0)", StringComparison.OrdinalIgnoreCase) >= 0
                || err.IndexOf("(502)", StringComparison.OrdinalIgnoreCase) >= 0
                || err.IndexOf("(503)", StringComparison.OrdinalIgnoreCase) >= 0
                || err.IndexOf("(504)", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            double kb = bytes / 1024.0;
            if (kb < 1024) return kb.ToString("F1") + " KB";
            double mb = kb / 1024.0;
            if (mb < 1024) return mb.ToString("F1") + " MB";
            double gb = mb / 1024.0;
            return gb.ToString("F2") + " GB";
        }
    }
}
