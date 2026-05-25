using AIChat.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AIChat.Core
{
    public static class ResponseParser
    {
        public static string EscapeJson(string s)
        {
            return s?.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "").Replace("\n", "\\n") ?? "";
        }

        public static string ExtractContentFromOllama(string jsonResponse)
        {
            try
            {
                var match = Regex.Match(jsonResponse, "\"content\"\\s*:\\s*\"([^\"]*)\"");
                if (match.Success)
                {
                    return Regex.Unescape(match.Groups[1].Value);
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"[Ollama] 解析失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 从 OpenAI 兼容 /v1/chat/completions 响应中提取 assistant 正文。
        /// 旧版 (.*?) 在含转义符、换行或 Qwen3 长回复时会截断或匹配到空 content，导致 UI 无字幕。
        /// </summary>
        public static string ExtractContentRegex(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                // 优先：choices[0].message.content（标准结构）
                var choice = Regex.Match(json,
                    "\"choices\"\\s*:\\s*\\[\\s*\\{[\\s\\S]*?\"message\"\\s*:\\s*\\{[\\s\\S]*?\"content\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
                    RegexOptions.Singleline);
                if (choice.Success)
                {
                    string v = Regex.Unescape(choice.Groups[1].Value);
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }

                // 回退：取所有 "content":"..." 中最后一个非空（与 DailyStory 一致，兼容多段 JSON）
                string last = null;
                foreach (Match m in Regex.Matches(json,
                    "\"content\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"",
                    RegexOptions.Singleline))
                {
                    string part = Regex.Unescape(m.Groups[1].Value);
                    if (!string.IsNullOrWhiteSpace(part)) last = part;
                }
                return last;
            }
            catch (Exception ex)
            {
                Log.Warning($"[OpenAI] content 解析异常: {ex.Message}");
                return null;
            }
        }
        // 简易 JSON 提取辅助函数
        public static string ExtractJsonValue(string json, string key)
        {
            var match = Regex.Match(json, $"\"{key}\"\\s*:\\s*\"(.*?)\"");
            return match.Success ? Regex.Unescape(match.Groups[1].Value) : "";
        }
        // =========================================================================================
        // 【新增辅助函数】确保对话文本（字幕）强制换行，以防过长溢出屏幕。
        // =========================================================================================
        /// <summary>
        /// 在长文本中插入换行符，以确保文本在 UI 中可见。
        /// </summary>
        /// <param name="text">原始文本</param>
        /// <param name="maxLineLength">每行最大字符数</param>
        /// <returns>带有换行符的文本</returns>
        public static string InsertLineBreaks(string text, int maxLineLength = 25)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLineLength)
            {
                return text;
            }

            StringBuilder sb = new StringBuilder();
            int currentLength = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                sb.Append(c);
                currentLength++;

                if (currentLength >= maxLineLength && c != '\n')
                {
                    // 检查下一个字符是否已经是换行符，避免双重换行
                    if (i + 1 < text.Length && text[i + 1] != '\n')
                    {
                        sb.Append('\n');
                        currentLength = 0;
                    }
                }

                if (c == '\n')
                {
                    currentLength = 0;
                }
            }
            return sb.ToString();
        }
    }
}

