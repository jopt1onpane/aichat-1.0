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
                if (model.SizeBytes > 0)
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

                long resumeFrom = SafeLength(partPath);
                Log.Info($"[Download] 开始下载 {model.File} ← {url} （断点 {resumeFrom} bytes）");

                bool urlOk = false;
                string urlErr = null;

                yield return DownloadFromUrlAsync(url, partPath, resumeFrom, model.SizeBytes,
                    (p) =>
                    {
                        p.ModelId = model.Id;
                        p.ModelDisplayName = model.DisplayName;
                        p.CurrentUrl = url;
                        onProgress?.Invoke(p);
                    },
                    (success, detail) => { urlOk = success; urlErr = detail; });

                if (!urlOk)
                {
                    Log.Warning($"[Download] {url} 失败：{urlErr}，尝试下一个镜像");
                    lastErr = urlErr;
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
                req.timeout = 0; // 大文件下载不能用超时；进度卡死 60s 由上层判断
                if (resumeFrom > 0)
                    req.SetRequestHeader("Range", "bytes=" + resumeFrom + "-");

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

        private static long SafeLength(string path)
        {
            try { return File.Exists(path) ? new FileInfo(path).Length : 0; }
            catch { return 0; }
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
