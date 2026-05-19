using AIChat.Core;
using BepInEx.Logging;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AIChat.Services
{
    public static class TTSClient
    {
        /// <param name="sampleSteps">GPT-SoVITS v3 CFM steps (optional).</param>
        /// <param name="ifSuperResolution">v3 super-resolution post (optional).</param>
        public static IEnumerator DownloadVoiceWithRetry(
            string url,
            string textToSpeak,
            string targetLang,
            string refPath,
            string promptText,
            string promptLang,
            ManualLogSource logger,
            Action<AudioClip> onComplete,
            int maxRetries = 3,
            float timeoutSeconds = 30f,
            bool audioPathCheck = false,
            int? sampleSteps = null,
            bool? ifSuperResolution = null)
        {
            if (!string.IsNullOrEmpty(refPath))
            {
                refPath = refPath.Trim().Trim('"', '\\', ' ', '\t', '\r', '\n');
                logger.LogInfo($"[TTS] 清理后的参考音频路径: {refPath}");
            }

            if (audioPathCheck && !File.Exists(refPath))
            {
                string aiChatRef = Path.Combine(BepInEx.Paths.PluginPath, "AIChat", "tts_ref.wav");
                string legacyChill = Path.Combine(BepInEx.Paths.PluginPath, "ChillAIMod", "Voice.wav");
                if (File.Exists(aiChatRef))
                    refPath = aiChatRef;
                else if (File.Exists(legacyChill))
                    refPath = legacyChill;
                else
                {
                    logger.LogError($"[TTS] 找不到参考音频: {refPath}（也未找到 {aiChatRef}）");
                    onComplete?.Invoke(null);
                    yield break;
                }
            }

            var sb = new StringBuilder();
            sb.Append('{');
            sb.Append("\"text\":\"").Append(ResponseParser.EscapeJson(textToSpeak)).Append("\",");
            sb.Append("\"text_lang\":\"").Append(ResponseParser.EscapeJson(targetLang)).Append("\",");
            sb.Append("\"ref_audio_path\":\"").Append(ResponseParser.EscapeJson(refPath)).Append("\",");
            sb.Append("\"prompt_text\":\"").Append(ResponseParser.EscapeJson(promptText)).Append("\",");
            sb.Append("\"prompt_lang\":\"").Append(ResponseParser.EscapeJson(promptLang)).Append('"');
            if (sampleSteps.HasValue)
                sb.Append(",\"sample_steps\":").Append(sampleSteps.Value);
            if (ifSuperResolution.HasValue)
                sb.Append(",\"if_sr\":").Append(ifSuperResolution.Value ? "true" : "false");
            sb.Append('}');
            string jsonBody = sb.ToString();

            logger.LogInfo($"[TTS] 完整请求信息:");
            logger.LogInfo($"[TTS]   URL: {url}");
            logger.LogInfo($"[TTS]   Request Body: {jsonBody}");
            logger.LogInfo("[TTS] 开始生成语音...");

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
                {
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.WAV);
                    request.SetRequestHeader("Content-Type", "application/json");
                    request.timeout = (int)timeoutSeconds;

                    var requestStartTime = DateTime.UtcNow;

                    yield return request.SendWebRequest();

                    var requestDuration = (DateTime.UtcNow - requestStartTime).TotalSeconds;

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var clip = DownloadHandlerAudioClip.GetContent(request);
                        if (clip != null)
                        {
                            logger.LogInfo($"[TTS] 语音生成成功（第 {attempt} 次尝试）（耗时 {requestDuration:F2}s）");
                            onComplete?.Invoke(clip);
                            yield break;
                        }
                    }

                    logger.LogWarning($"[TTS] 第 {attempt}/{maxRetries} 次尝试失败（耗时 {requestDuration:F2}s）: {request.error}");
                    if (attempt < maxRetries)
                    {
                        yield return new WaitForSeconds(2f);
                    }
                }
            }

            logger.LogError("[TTS] 所有重试均失败，放弃生成语音");
            onComplete?.Invoke(null);
        }

        /// <summary>先尝试 GET /health（v3 桥接）；失败则退回 POST /tts 小包探测。</summary>
        public static IEnumerator CheckTTSHealthOnce(string baseUrl, ManualLogSource logger, Action<bool> onResult)
        {
            string trimmed = baseUrl.TrimEnd('/');
            string healthUrl = trimmed + "/health";

            using (UnityWebRequest h = UnityWebRequest.Get(healthUrl))
            {
                h.timeout = 10;
                yield return h.SendWebRequest();

                if (h.result == UnityWebRequest.Result.Success && h.responseCode == 200)
                {
                    logger.LogDebug("[TTS Health] GET /health OK");
                    onResult?.Invoke(true);
                    yield break;
                }
            }

            string ttsUrl = trimmed + "/tts";
            string minimalJson = @"{""text"":""ping"",""text_lang"":""ja"",""ref_audio_path"":""x"",""prompt_text"":""x"",""prompt_lang"":""ja""}";
            using (UnityWebRequest req = new UnityWebRequest(ttsUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(minimalJson);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 8;

                yield return req.SendWebRequest();

                bool isReady = false;
                if (req.result == UnityWebRequest.Result.Success)
                {
                    isReady = true;
                }
                else if (req.responseCode == 422 || req.responseCode == 400)
                {
                    isReady = true;
                }

                if (isReady)
                {
                    logger.LogDebug("[TTS Health] 通过 /tts 探测认为服务已启动");
                }
                else
                {
                    string error = req.error ?? $"HTTP {req.responseCode}";
                    logger.LogDebug($"[TTS Health] 服务未就绪: {error}");
                }

                onResult?.Invoke(isReady);
            }
        }
    }
}
