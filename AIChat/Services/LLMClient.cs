using AIChat.Core;
using AIChat.Utils;
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

namespace AIChat.Services
{
    public static  class LLMClient
    {
        public static IEnumerator SendLLMRequest(LLMRequestContext requestContext, Action<string> onSuccess, Action<string, long> onFailure)
        {
            string jsonBody = LLMUtils.BuildRequestBody(requestContext);
            string apiUrl = LLMUtils.GetApiUrlForThinkMode(requestContext);

            using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                if (!requestContext.UseLocalOllama)
                {
                    request.SetRequestHeader("Authorization", "Bearer " + requestContext.ApiKey);
                }

                Log.Info($"[{requestContext.LogHeader}] 正在等待 LLM API 响应... url={apiUrl}, requestBytes={bodyRaw.Length}");
                var startTime = DateTime.UtcNow;

                yield return request.SendWebRequest();

                Log.Info($"[{requestContext.LogHeader}] LLM 响应完成，耗时: {(DateTime.UtcNow - startTime).TotalSeconds} 秒");

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string rawResponse = request.downloadHandler.text;
                    LogUsage(requestContext.LogHeader, rawResponse);
                    string extracted = requestContext.UseLocalOllama
                        ? ResponseParser.ExtractContentFromOllama(rawResponse)
                        : ResponseParser.ExtractContentRegex(rawResponse);
                    if (string.IsNullOrWhiteSpace(extracted))
                        Log.Warning($"[{requestContext.LogHeader}] JSON 提取 content 为空，responseBytes={Encoding.UTF8.GetByteCount(rawResponse)} 前200字={TruncateForLog(rawResponse, 200)}");
                    onSuccess(rawResponse);
                }
                else
                {
                    string errorMsg = $"{request.error}";
                    long responseCode = request.responseCode;
                    onFailure(errorMsg, responseCode);
                }
            }
        }

        private static void LogUsage(string logHeader, string rawResponse)
        {
            if (string.IsNullOrEmpty(rawResponse)) return;

            long prompt = ExtractLong(rawResponse, "prompt_tokens");
            long completion = ExtractLong(rawResponse, "completion_tokens");
            long total = ExtractLong(rawResponse, "total_tokens");
            // llama.cpp PR #19361 起 OAI chat 返回 usage.prompt_tokens_details.cached_tokens；
            // 用它来判断 KV 前缀缓存是否命中（命中率 = cached/prompt）。Ollama 不返回此字段，会得到 -1。
            long cached = ExtractCachedTokens(rawResponse);

            if (prompt >= 0 || completion >= 0 || total >= 0)
            {
                string cacheStr = "";
                if (cached >= 0 && prompt > 0)
                {
                    double hit = (double)cached / prompt * 100.0;
                    cacheStr = $" cached_tokens={cached} ({hit:F1}%)";
                }
                else if (cached >= 0)
                {
                    cacheStr = $" cached_tokens={cached}";
                }
                Log.Info($"[{logHeader}] usage prompt_tokens={prompt} completion_tokens={completion} total_tokens={total}{cacheStr} responseBytes={Encoding.UTF8.GetByteCount(rawResponse)}");
            }
            else
            {
                Log.Info($"[{logHeader}] usage unavailable responseBytes={Encoding.UTF8.GetByteCount(rawResponse)}");
            }
        }

        /// <summary>
        /// 在 usage.prompt_tokens_details 块内找 cached_tokens，避免误抓到其它位置的同名字段。
        /// 没有则返回 -1（Ollama / 旧版 llama-server 都不会返回此字段）。
        /// </summary>
        private static long ExtractCachedTokens(string json)
        {
            try
            {
                var detailsMatch = Regex.Match(
                    json,
                    "\"prompt_tokens_details\"\\s*:\\s*\\{([^}]*)\\}",
                    RegexOptions.Singleline);
                if (!detailsMatch.Success) return -1;
                var inner = detailsMatch.Groups[1].Value;
                var cachedMatch = Regex.Match(inner, "\"cached_tokens\"\\s*:\\s*(\\d+)");
                if (cachedMatch.Success && long.TryParse(cachedMatch.Groups[1].Value, out long v))
                    return v;
            }
            catch { }
            return -1;
        }

        private static long ExtractLong(string json, string key)
        {
            try
            {
                var m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(\\d+)");
                if (m.Success && long.TryParse(m.Groups[1].Value, out long v))
                    return v;
            }
            catch { }
            return -1;
        }

        private static string TruncateForLog(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length <= max ? s : s.Substring(0, max) + "…";
        }
    }
}
