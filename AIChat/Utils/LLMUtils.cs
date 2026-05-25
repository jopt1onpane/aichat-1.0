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
        /// <summary>
        /// 解析 [Tag] ||| 日语 ||| 简体 三栏格式。会剥离常见推理/思维链标签。
        /// 兼容：模型在日语正文中误用「|||」分段、或第二栏用单「|」分隔日中翻訳的情况。
        /// </summary>
        public static LLMStandardResponse ParseStandardResponse(string response)
        {
            string raw = response ?? "";
            LLMStandardResponse ret = new LLMStandardResponse(false, "Think", "", raw);

            if (string.IsNullOrWhiteSpace(raw))
            {
                ret.SubtitleText = "";
                Log.Warning("[格式错误] AI 回复为空");
                return ret;
            }

            string cleaned = StripReasoningBlocks(raw.Trim());
            cleaned = NormalizeChatTripleFormat(cleaned);

            if (TryParseVoiceAndChineseSubtitle(cleaned, ref ret))
            {
                RepairMisclassifiedSubtitle(cleaned, ref ret);
                EnsureSubtitleChineseOrClear(ref ret);
                ret.VoiceText = NormalizeVoiceTextForTts(ret.VoiceText);
                ret.Success = true;
                return ret;
            }

            string[] parts = cleaned.Split(new[] { "|||" }, StringSplitOptions.None);
            bool assigned = TryAssignTriple(parts, ref ret);

            if (!assigned)
            {
                Match tail = Regex.Match(cleaned, @"(\[[^\]]+\])\s*\|\|\|\s*(.*?)\s*\|\|\|\s*(.*)\s*$", RegexOptions.Singleline);
                if (tail.Success)
                {
                    AssignTagFromPart(ref ret, tail.Groups[1].Value.Trim());
                    ret.VoiceText = NormalizeVoiceTextForTts(tail.Groups[2].Value.Trim());
                    ret.SubtitleText = tail.Groups[3].Value.Trim();
                    assigned = !string.IsNullOrEmpty(ret.VoiceText) || !string.IsNullOrEmpty(ret.SubtitleText);
                }
            }

            if (!assigned)
            {
                Match lastGood = null;
                foreach (Match m in Regex.Matches(cleaned, @"(\[[^\]]+\])\s*\|\|\|\s*(.*?)\s*\|\|\|\s*(.*)", RegexOptions.Singleline))
                {
                    string v = m.Groups[2].Value.Trim();
                    string s = m.Groups[3].Value.Trim();
                    if (v.Length + s.Length > 0)
                        lastGood = m;
                }
                if (lastGood != null)
                {
                    AssignTagFromPart(ref ret, lastGood.Groups[1].Value.Trim());
                    ret.VoiceText = NormalizeVoiceTextForTts(lastGood.Groups[2].Value.Trim());
                    ret.SubtitleText = lastGood.Groups[3].Value.Trim();
                    assigned = true;
                }
            }

            if (assigned && string.IsNullOrEmpty(ret.VoiceText) && string.IsNullOrEmpty(ret.SubtitleText))
                assigned = false;

            if (assigned)
            {
                RepairMisclassifiedSubtitle(cleaned, ref ret);
                EnsureSubtitleChineseOrClear(ref ret);
                ret.VoiceText = NormalizeVoiceTextForTts(ret.VoiceText);
                ret.Success = true;
                return ret;
            }

            ret.Success = false;
            ret.VoiceText = "";
            ret.SubtitleText = cleaned;
            Log.Warning($"[格式错误] AI 回复不符合格式: {raw}");
            return ret;
        }

        /// <summary>去除不含台词的中间推理块，便于解析三栏格式。</summary>
        public static string StripReasoningBlocks(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string cur = text;
            for (int i = 0; i < 12; i++)
            {
                string next = cur;
                next = Regex.Replace(next, @"```[\s\S]*?```", "", RegexOptions.IgnoreCase);
                next = Regex.Replace(next, @"``[\s\S]*?``", "", RegexOptions.IgnoreCase);
                if (next == cur)
                    break;
                cur = next.Trim();
            }
            return cur.Trim();
        }

        /// <summary>将「tag ||| 日语片段 | 中文」の误写统一为「|||」三分栏（| 后须以汉字起头）。</summary>
        private static string NormalizeChatTripleFormat(string cleaned)
        {
            if (string.IsNullOrEmpty(cleaned)) return cleaned;
            // 常见：第一块后仅有「|||」，翻译又用单「|」接中文
            return Regex.Replace(
                cleaned,
                @"^(\s*\[[^\]]+\])\s*\|\|\|\s*(.*?)\s+\|\s+([\u4e00-\u9fff\u3000-\u3017][^\|]*)$",
                "$1 ||| $2 ||| $3",
                RegexOptions.Singleline);
        }

        private static int CountKana(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            return Regex.Matches(s, @"[\u3040-\u309F\u30A0-\u30FF]").Count;
        }

        private static int CountHan(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            return Regex.Matches(s, @"[\u4e00-\u9fff]").Count;
        }

        /// <summary>判断一段是否像简体中文翻译栏（与日语台词区分）。</summary>
        private static bool LooksLikeChineseSubtitle(string seg)
        {
            if (string.IsNullOrWhiteSpace(seg)) return false;
            int k = CountKana(seg);
            int h = CountHan(seg);
            if (h < 2) return false;
            if (k == 0) return true;
            return h >= k * 2;
        }

        /// <summary>从右向左找「中文栏」，左侧全部合并为日语台词（多段误用 ||| 时修复）。</summary>
        private static bool TryParseVoiceAndChineseSubtitle(string cleaned, ref LLMStandardResponse ret)
        {
            var m = Regex.Match(cleaned, @"^\s*(\[[^\]]+\])\s*(.*)$", RegexOptions.Singleline);
            if (!m.Success) return false;
            string tagPart = m.Groups[1].Value.Trim();
            string body = m.Groups[2].Value.Trim();
            if (string.IsNullOrEmpty(body) || !body.Contains("|||")) return false;

            var segs = new List<string>();
            foreach (var p in body.Split(new[] { "|||" }, StringSplitOptions.None))
            {
                string t = p.Trim();
                if (t.Length > 0) segs.Add(t);
            }
            if (segs.Count < 2) return false;

            int chineseIdx = -1;
            for (int i = segs.Count - 1; i >= 0; i--)
            {
                if (LooksLikeChineseSubtitle(segs[i]))
                {
                    chineseIdx = i;
                    break;
                }
            }
            if (chineseIdx < 0) return false;

            string voice = string.Join("\n", segs.Take(chineseIdx)).Trim();
            string sub = segs[chineseIdx].Trim();
            if (string.IsNullOrEmpty(voice) && string.IsNullOrEmpty(sub)) return false;

            AssignTagFromPart(ref ret, tagPart);
            ret.VoiceText = voice;
            ret.SubtitleText = sub;
            return true;
        }

        /// <summary>旧逻辑把日语末段当成「中文栏」时，尝试用全文最后一个 ||| 后的真中文段落替换。</summary>
        private static void RepairMisclassifiedSubtitle(string cleaned, ref LLMStandardResponse ret)
        {
            if (string.IsNullOrEmpty(ret.SubtitleText) || string.IsNullOrEmpty(cleaned)) return;
            if (LooksLikeChineseSubtitle(ret.SubtitleText)) return;

            int li = cleaned.LastIndexOf("|||", StringComparison.Ordinal);
            if (li < 0) return;
            string tail = cleaned.Substring(li + 3).Trim();
            if (!LooksLikeChineseSubtitle(tail)) return;
            ret.SubtitleText = tail;
        }

        /// <summary>第三栏若明显不是中文（模型把日语末段误放在第三栏），清空字幕以待上层兜底，避免界面仅显示短日文。</summary>
        private static void EnsureSubtitleChineseOrClear(ref LLMStandardResponse ret)
        {
            if (string.IsNullOrEmpty(ret.SubtitleText)) return;
            if (LooksLikeChineseSubtitle(ret.SubtitleText)) return;
            Log.Warning($"[字幕] 第三栏判定为非中文，已忽略以防误显示日语: {ret.SubtitleText}");
            ret.SubtitleText = "";
        }

        /// <summary>去掉 TTS 里误插入的区切符，避免念「vertical bar」。</summary>
        private static string NormalizeVoiceTextForTts(string voice)
        {
            if (string.IsNullOrEmpty(voice)) return "";
            string t = voice.Replace("|||", "。").Trim();
            t = Regex.Replace(t, @"\s*\n\s*", " ");
            return Regex.Replace(t, @"\s{2,}", " ").Trim();
        }

        private static bool TryAssignTriple(string[] parts, ref LLMStandardResponse ret)
        {
            if (parts == null || parts.Length < 3)
                return false;

            string tagPart;
            string voice;
            string sub;

            if (parts.Length == 3)
            {
                tagPart = parts[0].Trim();
                voice = parts[1].Trim();
                sub = parts[2].Trim();
            }
            else
            {
                tagPart = parts[0].Trim();
                sub = parts[parts.Length - 1].Trim();
                voice = string.Join("|||", parts.Skip(1).Take(parts.Length - 2)).Trim();
            }

            AssignTagFromPart(ref ret, tagPart);
            ret.VoiceText = voice;
            ret.SubtitleText = sub;

            return !string.IsNullOrEmpty(voice) || !string.IsNullOrEmpty(sub);
        }

        private static void AssignTagFromPart(ref LLMStandardResponse ret, string tagPart)
        {
            var actionMatch = Regex.Match(tagPart, @"\[Action:(\w+)\]");
            if (actionMatch.Success)
            {
                ret.EmotionTag = actionMatch.Groups[1].Value;
                return;
            }

            string legacy = tagPart.Replace("[", "").Replace("]", "").Trim();
            // モデルが「thoại: Sad」等の接頭辞付きラベルを出したとき末尾の英単語タグだけ採用（ActionAnimMap 不一致を防ぐ）
            if (legacy.IndexOf(':') >= 0)
            {
                Match mColon = Regex.Match(legacy, @":\s*(\w+)\s*$");
                if (mColon.Success)
                    legacy = mColon.Groups[1].Value;
            }
            ret.EmotionTag = MapLegacyTag(legacy);
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

            // 【实时上下文 + RAG】
            // 设计：system 保持「**完全静态**」以最大化 llama-server KV 前缀缓存命中率
            //       （Qwen3 8K+ 长 persona 的 prefill 每轮要 ~9s，命中缓存后可压到 ~1s）。
            // LIVE_CONTEXT 与 RAG 每轮都会变化（时间/Pomodoro/检索结果），故塞到 user 消息的前缀。
            // 兼容老逻辑：若 system 仍含 {{LIVE_CONTEXT}} 占位符（用户自定义 persona），按旧逻辑替换并把 RAG 追加到 system 尾部。
            string finalSystemPrompt = requestContext.SystemPrompt ?? string.Empty;
            string liveContext = string.Empty;
            try { liveContext = LiveContextProvider.BuildContextSnippet(); }
            catch (Exception ex) { Log.Warning($"[LiveContext] 构建失败: {ex.Message}"); }

            bool legacyPlaceholderMode = finalSystemPrompt.Contains("{{LIVE_CONTEXT}}");
            string userPromptForLlm;

            if (legacyPlaceholderMode)
            {
                // 旧路径：替换占位符（含 5 处全替换的副作用，缓存命中率会低）+ RAG 追加 system 尾部
                finalSystemPrompt = finalSystemPrompt.Replace("{{LIVE_CONTEXT}}", liveContext ?? string.Empty);
                if (!string.IsNullOrEmpty(requestContext.ReferenceSnippets))
                {
                    finalSystemPrompt = finalSystemPrompt + "\n" + requestContext.ReferenceSnippets;
                }
                userPromptForLlm = userPromptWithMemory;
                Log.Info("[LLM] 兼容模式：system 含 {{LIVE_CONTEXT}} 占位符，沿用旧拼接路径（KV 缓存难复用）。");
            }
            else
            {
                // 新路径：system 保持静态，user 前缀注入 LIVE_CONTEXT + RAG
                var userSb = new StringBuilder();
                if (!string.IsNullOrEmpty(liveContext))
                {
                    userSb.Append(liveContext.TrimEnd());
                    userSb.Append("\n\n");
                }
                if (!string.IsNullOrEmpty(requestContext.ReferenceSnippets))
                {
                    userSb.Append("=== 参考メモ（RAG） ===\n");
                    userSb.Append(requestContext.ReferenceSnippets.TrimEnd());
                    userSb.Append("\n\n");
                }
                userSb.Append("=== 現在の入力 ===\n");
                userSb.Append(userPromptWithMemory ?? string.Empty);
                userPromptForLlm = userSb.ToString();
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

            int sysChars = (finalSystemPrompt ?? string.Empty).Length;
            int ragChars = (requestContext.ReferenceSnippets ?? string.Empty).Length;
            int userChars = (userPromptForLlm ?? string.Empty).Length;
            Log.Info($"[LLM] system 约 {sysChars} 字（静态={!legacyPlaceholderMode}），user 约 {userChars} 字（含 RAG 约 {ragChars} 字）。静态 system 越长，首轮越慢但后续轮缓存命中收益越大。");

            string jsonBody;
            string extraJson = requestContext.UseLocalOllama ? $@",""stream"": false" : "";
            if (requestContext.UseLocalOllama)
            {
                // Ollama 原生 /api/chat 支持 keep_alive；OpenAI 兼容路径通常会忽略未知字段。
                extraJson += @",""keep_alive"": ""30m""";
            }

            // Ollama 原生 /api/chat 才支持 think 字段；llama-server OpenAI 兼容端点会忽略它。
            // 内嵌 Qwen3 thinking 由启动参数 --reasoning-budget 0 关闭，避免 reasoning_content 吃光输出 token。
            if (requestContext.UseLocalOllama)
            {
                if (requestContext.ThinkMode == ThinkMode.Enable)
                    extraJson += @",""think"": true";
                else
                    extraJson += @",""think"": false";
            }

            // Step1: 只限制内嵌 llama-server 的正文输出长度。prompt/context 不截断，-c 仍为 16384。
            if (IsEmbeddedLlamaServer(requestContext.ApiUrl))
            {
                extraJson += @",""max_tokens"": 256";
                Log.Info("[LLM] 内嵌 llama-server: --reasoning-budget 0 关闭 thinking，max_tokens=256");
            }

            if (requestContext.ModelName.Contains("gemma")) {
                // gemma 系不支持 system role，必须拼接到 user 消息开头
                string finalPrompt = $"[System Instruction]\n{finalSystemPrompt}\n\n[User Message]\n{userPromptForLlm}";
                jsonBody = $@"{{ ""model"": ""{requestContext.ModelName}"", ""messages"": [ {{ ""role"": ""user"", ""content"": ""{ResponseParser.EscapeJson(finalPrompt)}"" }} ]{extraJson} }}";
            } else {
                jsonBody = $@"{{ ""model"": ""{requestContext.ModelName}"", ""messages"": [ {{ ""role"": ""system"", ""content"": ""{ResponseParser.EscapeJson(finalSystemPrompt)}"" }}, {{ ""role"": ""user"", ""content"": ""{ResponseParser.EscapeJson(userPromptForLlm)}"" }} ]{extraJson} }}";
            }

            Log.Info($"[记忆系统] 启用状态: {requestContext.HierarchicalMemory != null}; RAG 段落字节数: {(requestContext.ReferenceSnippets ?? string.Empty).Length}");
            if (requestContext.LogApiRequestBody)
            {
                Log.Info($"[发送给LLM的完整内容]\n========================================\n[System Prompt]\n{finalSystemPrompt}\n\n[User Content]\n{userPromptForLlm}\n========================================");
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

        private static bool IsEmbeddedLlamaServer(string apiUrl)
        {
            if (string.IsNullOrWhiteSpace(apiUrl)) return false;
            return (apiUrl.IndexOf("127.0.0.1:8080", StringComparison.Ordinal) >= 0
                || apiUrl.IndexOf("localhost:8080", StringComparison.OrdinalIgnoreCase) >= 0)
                && apiUrl.IndexOf("/v1/chat/completions", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
