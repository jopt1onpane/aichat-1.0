using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using ChillAIMod;
using AIChat.Core;
using AIChat.Services;

namespace AIChat.Utils
{
    public enum ThinkMode { Default, Enable, Disable }

    public struct LLMRequestContext
    {
        public string ApiUrl;
        public string ApiKey;
        public string ModelName;
        public string SystemPrompt;
        public string UserPrompt;
        public bool UseLocalOllama;
        public bool LogApiRequestBody;
        public bool FixApiPathForThinkMode;
        public ThinkMode ThinkMode;
        public HierarchicalMemory HierarchicalMemory;
        public string LogHeader;
        public string ReferenceSnippets;  // RAG 注入：会附加到 system prompt 末尾，空字符串表示不启用

        public LLMRequestContext(
            string apiUrl = "",
            string apiKey = "",
            string modelName = "",
            string systemPrompt = "",
            string userPrompt = "",
            bool useLocalOllama = false,
            bool logApiRequestBody = false,
            ThinkMode thinkMode = ThinkMode.Default,
            HierarchicalMemory hierarchicalMemory = null,
            string logHeader = "LLMRequest",
            bool fixApiPathForThinkMode = false,
            string referenceSnippets = ""
        )
        {
            ApiUrl = apiUrl;
            ApiKey = apiKey;
            ModelName = modelName;
            SystemPrompt = systemPrompt;
            UserPrompt = userPrompt;
            UseLocalOllama = useLocalOllama;
            LogApiRequestBody = logApiRequestBody;
            ThinkMode = thinkMode;
            HierarchicalMemory = hierarchicalMemory;
            LogHeader = logHeader;
            FixApiPathForThinkMode = fixApiPathForThinkMode;
            ReferenceSnippets = referenceSnippets;
        }
    }

    public struct LLMStandardResponse
    {
        public bool Success;
        public string EmotionTag;  // 动作标签，如 [Happy], [Think] 等
        public string VoiceText;   // 用于 TTS 的文本
        public string SubtitleText;// 用于字幕显示的文本

        public LLMStandardResponse(bool success, string emotionTag, string voiceText, string subtitleText)
        {
            Success = success;
            EmotionTag = emotionTag;
            VoiceText = voiceText;
            SubtitleText = subtitleText;
        }
    }

    public static class LLMUtils
    {
        public static LLMStandardResponse ParseStandardResponse(string response)
        {
            LLMStandardResponse ret = new LLMStandardResponse(false, "Think", "", response);

            string[] parts = response.Split(new string[] { "|||" }, StringSplitOptions.None);
            if (parts.Length < 3)
            {
                parts = response.Split(new string[] { "|" }, StringSplitOptions.None);
            }

            if (parts.Length >= 3)
            {
                string tagPart = parts[0].Trim();
                ret.VoiceText = parts[1].Trim();
                ret.SubtitleText = parts[2].Trim();

                // New format: [Action:TagName] or legacy [TagName]
                var actionMatch = Regex.Match(tagPart, @"\[Action:(\w+)\]");
                if (actionMatch.Success)
                {
                    ret.EmotionTag = actionMatch.Groups[1].Value;
                }
                else
                {
                    ret.EmotionTag = tagPart.Replace("[", "").Replace("]", "").Trim();
                    // Map legacy emotion names to new action tags
                    ret.EmotionTag = MapLegacyTag(ret.EmotionTag);
                }

                ret.Success = true;
            }

            if (!ret.Success) Log.Warning($"[格式错误] AI 回复不符合格式: {response}");

            return ret;
        }

        private static string MapLegacyTag(string tag)
        {
            switch (tag)
            {
                case "Happy": return "Joy";
                case "Confused": return "Frustration";
                case "Drink": return "DrinkTea";
                default: return tag;
            }
        }

        public static string BuildRequestBody(LLMRequestContext requestContext)
        {
            // 【集成分层记忆】获取带记忆上下文的提示词
            string userPromptWithMemory = GetContextWithMemory(requestContext.HierarchicalMemory, requestContext.UserPrompt);

            // 【实时上下文注入】用游戏当前的真实状态（番茄钟 / 活动 / 时间）替换 prompt 中的 {{LIVE_CONTEXT}} 占位符
            string finalSystemPrompt = requestContext.SystemPrompt;
            string liveContext = string.Empty;
            try { liveContext = LiveContextProvider.BuildContextSnippet(); }
            catch (Exception ex) { Log.Warning($"[LiveContext] 构建失败: {ex.Message}"); }

            if (finalSystemPrompt.Contains("{{LIVE_CONTEXT}}"))
            {
                finalSystemPrompt = finalSystemPrompt.Replace("{{LIVE_CONTEXT}}", liveContext ?? string.Empty);
            }
            else if (!string.IsNullOrEmpty(liveContext))
            {
                // 兼容自定义 prompt 没有占位符的情况：直接附加到末尾
                finalSystemPrompt = finalSystemPrompt + "\n" + liveContext;
            }

            // 不依赖 LogApiRequestBody，明确记录 LiveContext 是否注入了什么内容
            if (!string.IsNullOrEmpty(liveContext))
            {
                Log.Info("[LiveContext] 注入内容如下：\n" + liveContext.TrimEnd());
            }
            else
            {
                Log.Info("[LiveContext] 本次未生成任何实时上下文（Game/Pomodoro 未连接？）");
            }

            // 【集成 RAG】将检索到的参考语境追加到 system prompt 末尾
            if (!string.IsNullOrEmpty(requestContext.ReferenceSnippets))
            {
                finalSystemPrompt = finalSystemPrompt + "\n" + requestContext.ReferenceSnippets;
            }

            string jsonBody;
            string extraJson = requestContext.UseLocalOllama ? $@",""stream"": false" : "";
            if (requestContext.UseLocalOllama)
            {
                // Ollama 原生 /api/chat 支持 keep_alive；OpenAI 兼容路径通常会忽略未知字段。
                // 这里保持模型常驻，减少多轮对话时 Qwen 重新加载导致的 10s+ 抖动。
                extraJson += @",""keep_alive"": ""30m""";
            }
            // Ollama + Default → 显式禁用思考模式（Qwen3 等模型默认开启思考，会大幅拖慢响应）
            if (requestContext.UseLocalOllama && requestContext.ThinkMode == ThinkMode.Default)
            {
                extraJson += @",""think"": false";
            }
            else
            {
                extraJson += GetThinkParameterJson(requestContext.ThinkMode);
            }

            if (requestContext.ModelName.Contains("gemma")) {
                // 将 persona 作为背景信息放在 user 消息的最前面
                string finalPrompt = $"[System Instruction]\n{finalSystemPrompt}\n\n[User Message]\n{userPromptWithMemory}";
                jsonBody = $@"{{ ""model"": ""{requestContext.ModelName}"", ""messages"": [ {{ ""role"": ""user"", ""content"": ""{ResponseParser.EscapeJson(finalPrompt)}"" }} ]{extraJson} }}";
            } else {
                // Gemini 或 Ollama (如果是 Llama3 等) 通常支持 system role
                jsonBody = $@"{{ ""model"": ""{requestContext.ModelName}"", ""messages"": [ {{ ""role"": ""system"", ""content"": ""{ResponseParser.EscapeJson(finalSystemPrompt)}"" }}, {{ ""role"": ""user"", ""content"": ""{ResponseParser.EscapeJson(userPromptWithMemory)}"" }} ]{extraJson} }}";
            }

            Log.Info($"[记忆系统] 启用状态: {requestContext.HierarchicalMemory != null}; RAG 段落字节数: {(requestContext.ReferenceSnippets ?? string.Empty).Length}");
            // 【日志】打印完整的请求体（如果启用）
            if (requestContext.LogApiRequestBody)
            {
                // 【调试日志】显示完整的请求内容
                Log.Info($"[发送给LLM的完整内容]\n========================================\n[System Prompt]\n{finalSystemPrompt}\n\n[User Content]\n{userPromptWithMemory}\n========================================");
                Log.Info($"[API请求] 完整请求体:\n{jsonBody}");
            }

            return jsonBody;
        }

        /// <summary>
        /// 获取深度思考参数的 JSON 字符串
        /// </summary>
        private static string GetThinkParameterJson(ThinkMode thinkMode)
        {
            if (thinkMode == ThinkMode.Enable)
            {
                return @",""think"": true";
            }
            else if (thinkMode == ThinkMode.Disable)
            {
                return @",""think"": false";
            }
            // Default 模式不添加 think 参数
            return "";
        }

        private static string GetContextWithMemory(HierarchicalMemory hierarchicalMemory, string currentPrompt)
        {
            if (hierarchicalMemory != null)
            {
                string memoryContext = hierarchicalMemory.GetContext();
                Log.Info($"[记忆系统] 当前记忆状态:\n{hierarchicalMemory.GetMemoryStats()}");

                // 如果有记忆内容，则拼接；否则只返回当前提示
                if (!string.IsNullOrWhiteSpace(memoryContext))
                {
                    return $"{memoryContext}\n\n【Current Input】\n{currentPrompt}";
                }
            }
            
            // 无记忆或未启用，直接返回原始 prompt
            return currentPrompt;
        }

        /// <summary>
        /// 获取适合当前think模式的API URL
        /// </summary>
        public static string GetApiUrlForThinkMode(LLMRequestContext requestContext)
        {
            string baseUrl = requestContext.ApiUrl;
            // 如果启用了API路径修正，且think模式不是Default，需要使用Ollama原生API (/api/chat)
            if (requestContext.FixApiPathForThinkMode && requestContext.ThinkMode != ThinkMode.Default)
            {
                // 将 /v1/chat/completions 替换为 /api/chat
                if (baseUrl.Contains("/v1/chat/completions"))
                {
                    baseUrl = baseUrl.Replace("/v1/chat/completions", "/api/chat");
                    Log.Info($"[Think Mode] 切换到 Ollama 原生 API: {baseUrl}");
                }
                // 如果URL已经是 /api/chat 或其他格式，保持不变
            }
            
            return baseUrl;
        }
    }
}
