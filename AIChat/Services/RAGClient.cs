using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AIChat.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace AIChat.Services
{
    /// <summary>
    /// 单条原作台词检索结果。
    /// </summary>
    public class RagSnippet
    {
        public string Category;     // smalltalk / selftalk / ...
        public string Ja;           // 日语原文
        public string Zh;           // 简中翻译
        public float Score;         // cosine 相似度 + 关键词加权
    }

    /// <summary>
    /// 索引中的一条记录（运行时常驻内存）。
    /// </summary>
    internal class IndexEntry
    {
        public byte CategoryId;
        public string Category;
        public string Id;
        public string Ja;
        public string Zh;
        public float[] Vec;       // L2 归一化后的向量
    }

    /// <summary>
    /// Satone 原作台词 RAG 检索器。
    ///
    /// - 启动时从二进制索引文件加载所有条目和向量到内存（约 2 MB / 1000+ 条）。
    /// - 查询时先调用 Ollama embedding 拿到玩家输入的向量，做暴力 cosine + 关键词加权，取 top-K。
    /// - 仅供风格参考，不参与生成。
    /// </summary>
    public static class RAGClient
    {
        // 与 Python 端 CATEGORY_MAP 必须保持一致
        private static readonly string[] CategoryNames =
        {
            "smalltalk", "selftalk", "clickheroine", "pomodoro",
            "motion", "main_general", "tutorial", "later_extra", "other"
        };

        private static readonly object _loadLock = new object();
        private static List<IndexEntry> _entries;
        private static int _vecDim;
        private static string _embedModel;
        private static bool _loaded;
        private static string _loadedPath;

        public static bool IsLoaded => _loaded;
        public static int Count => _entries?.Count ?? 0;
        public static int Dim => _vecDim;
        public static string EmbedModel => _embedModel;

        // ============================================================
        // 索引加载
        // ============================================================

        public static bool LoadIndex(string indexPath)
        {
            lock (_loadLock)
            {
                if (_loaded && _loadedPath == indexPath) return true;

                if (string.IsNullOrEmpty(indexPath) || !File.Exists(indexPath))
                {
                    Log.Warning($"[RAG] 索引文件不存在: {indexPath}");
                    Reset();
                    return false;
                }

                try
                {
                    using (var fs = File.OpenRead(indexPath))
                    using (var br = new BinaryReader(fs, Encoding.UTF8, false))
                    {
                        byte[] magic = br.ReadBytes(4);
                        if (magic.Length != 4 || magic[0] != (byte)'S' || magic[1] != (byte)'R'
                            || magic[2] != (byte)'A' || magic[3] != (byte)'G')
                        {
                            Log.Error("[RAG] 索引文件 magic 错误，可能不是有效的 RAG 索引");
                            Reset();
                            return false;
                        }

                        int version = br.ReadInt32();
                        if (version != 1)
                        {
                            Log.Error($"[RAG] 索引版本不兼容: {version}");
                            Reset();
                            return false;
                        }

                        _vecDim = br.ReadInt32();
                        int count = br.ReadInt32();
                        int modelLen = br.ReadInt32();
                        _embedModel = Encoding.UTF8.GetString(br.ReadBytes(modelLen));

                        var list = new List<IndexEntry>(count);
                        for (int i = 0; i < count; i++)
                        {
                            byte catId = br.ReadByte();
                            int idLen = br.ReadInt32();
                            string id = Encoding.UTF8.GetString(br.ReadBytes(idLen));
                            int jaLen = br.ReadInt32();
                            string ja = Encoding.UTF8.GetString(br.ReadBytes(jaLen));
                            int zhLen = br.ReadInt32();
                            string zh = Encoding.UTF8.GetString(br.ReadBytes(zhLen));

                            var vec = new float[_vecDim];
                            byte[] raw = br.ReadBytes(_vecDim * 4);
                            Buffer.BlockCopy(raw, 0, vec, 0, raw.Length);
                            NormalizeInPlace(vec);

                            list.Add(new IndexEntry
                            {
                                CategoryId = catId,
                                Category = (catId < CategoryNames.Length) ? CategoryNames[catId] : "other",
                                Id = id,
                                Ja = ja,
                                Zh = zh,
                                Vec = vec
                            });
                        }

                        _entries = list;
                        _loaded = true;
                        _loadedPath = indexPath;

                        Log.Info($"[RAG] 已加载索引: {indexPath}  ({count} 条, dim={_vecDim}, model={_embedModel})");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[RAG] 加载索引失败: {ex.Message}");
                    Reset();
                    return false;
                }
            }
        }

        private static void Reset()
        {
            _entries = null;
            _vecDim = 0;
            _embedModel = null;
            _loaded = false;
            _loadedPath = null;
        }

        // ============================================================
        // 检索（协程式：返回 IEnumerator 给 Unity 主流程使用）
        // ============================================================

        /// <summary>
        /// 异步检索 top-K 相关原作台词。
        /// </summary>
        /// <param name="ollamaBaseUrl">例如 http://127.0.0.1:11434</param>
        /// <param name="embedModel">嵌入模型名，例如 bge-m3</param>
        /// <param name="query">玩家原始输入</param>
        /// <param name="topK">取前 K 条</param>
        /// <param name="minScore">阈值，低于则丢弃</param>
        /// <param name="onComplete">完成回调，参数为命中片段（可能为空）</param>
        /// <param name="timeoutSeconds">嵌入接口超时</param>
        /// <summary>
        /// 启动时调用一次，预热 bge-m3 让其常驻显存，避免首次查询的 5-10s 冷启动。
        /// 失败也无所谓（用户可能没装/没启 Ollama），只是少一次预热而已。
        /// </summary>
        public static IEnumerator WarmUpAsync(string ollamaBaseUrl, string embedModel, float timeoutSeconds = 30f)
        {
            float t0 = Time.realtimeSinceStartup;
            float[] vec = null;
            yield return EmbedAsync(
                ollamaBaseUrl,
                embedModel,
                "warmup",
                v => vec = v,
                err => Log.Warning($"[RAG] 预热失败（不影响后续）: {err}"),
                timeoutSeconds
            );
            float elapsed = Time.realtimeSinceStartup - t0;
            if (vec != null)
                Log.Info($"[RAG] 嵌入模型预热成功 ({elapsed:F2}s, dim={vec.Length})，bge-m3 已常驻显存");
            else
                Log.Info($"[RAG] 嵌入模型预热未完成 ({elapsed:F2}s)，首次查询可能仍偏慢");
        }

        public static IEnumerator RetrieveAsync(
            string ollamaBaseUrl,
            string embedModel,
            string query,
            int topK,
            float minScore,
            Action<List<RagSnippet>> onComplete,
            float timeoutSeconds = 10f)
        {
            if (!_loaded || _entries == null || _entries.Count == 0)
            {
                onComplete?.Invoke(new List<RagSnippet>());
                yield break;
            }
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 3)
            {
                onComplete?.Invoke(new List<RagSnippet>());
                yield break;
            }

            float[] queryVec = null;
            yield return EmbedAsync(
                ollamaBaseUrl,
                embedModel,
                query,
                v => queryVec = v,
                err => Log.Warning($"[RAG] 嵌入失败: {err}"),
                timeoutSeconds
            );

            if (queryVec == null || queryVec.Length != _vecDim)
            {
                Log.Warning($"[RAG] 嵌入向量异常 (got dim={(queryVec == null ? -1 : queryVec.Length)}, expect {_vecDim})，跳过本次 RAG");
                onComplete?.Invoke(new List<RagSnippet>());
                yield break;
            }

            NormalizeInPlace(queryVec);

            List<RagSnippet> hits = ScoreAndPickTopK(queryVec, query, topK, minScore);
            onComplete?.Invoke(hits);
        }

        // ============================================================
        // Embedding 调用
        // ============================================================

        private static IEnumerator EmbedAsync(
            string ollamaBaseUrl,
            string embedModel,
            string text,
            Action<float[]> onSuccess,
            Action<string> onFailure,
            float timeoutSeconds)
        {
            string url = ollamaBaseUrl.TrimEnd('/') + "/api/embeddings";
            // keep_alive=30m 让 bge-m3 常驻显存，避免每次 5-10s 冷启动
            string body = "{\"model\":\"" + JsonEscape(embedModel) + "\",\"prompt\":\"" + JsonEscape(text) + "\",\"keep_alive\":\"30m\"}";
            byte[] payload = Encoding.UTF8.GetBytes(body);

            using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(payload);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = Mathf.Max(1, Mathf.CeilToInt(timeoutSeconds));

                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    onFailure?.Invoke($"{req.error} ({req.responseCode})");
                    yield break;
                }

                string respText = req.downloadHandler.text;
                float[] vec = ParseEmbeddingResponse(respText);
                if (vec == null)
                {
                    onFailure?.Invoke($"无法解析嵌入响应: {Truncate(respText, 200)}");
                    yield break;
                }
                onSuccess?.Invoke(vec);
            }
        }

        /// <summary>
        /// 解析 Ollama embedding 响应。常见两种格式：
        /// 1) {"embedding":[...]}   (老版本)
        /// 2) {"embeddings":[[...]]} (新版本，批量)
        /// </summary>
        private static float[] ParseEmbeddingResponse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            int idx = json.IndexOf("\"embedding\"", StringComparison.Ordinal);
            int sliceStart;
            if (idx >= 0)
            {
                int b = json.IndexOf('[', idx);
                if (b < 0) return null;
                int e = json.IndexOf(']', b);
                if (e < 0) return null;
                sliceStart = b + 1;
                return ParseFloatList(json, sliceStart, e);
            }

            idx = json.IndexOf("\"embeddings\"", StringComparison.Ordinal);
            if (idx >= 0)
            {
                // 找到第一个 "[[ ... ]]" 内层数组
                int outer = json.IndexOf('[', idx);
                if (outer < 0) return null;
                int inner = json.IndexOf('[', outer + 1);
                if (inner < 0) return null;
                int e = json.IndexOf(']', inner);
                if (e < 0) return null;
                return ParseFloatList(json, inner + 1, e);
            }

            return null;
        }

        private static float[] ParseFloatList(string json, int start, int end)
        {
            var values = new List<float>(1024);
            int i = start;
            while (i < end)
            {
                while (i < end && (json[i] == ' ' || json[i] == ',' || json[i] == '\n' || json[i] == '\r' || json[i] == '\t')) i++;
                if (i >= end) break;

                int j = i;
                while (j < end && json[j] != ',' && json[j] != ' ' && json[j] != '\n' && json[j] != '\r' && json[j] != '\t') j++;
                string token = json.Substring(i, j - i);
                if (token.Length > 0)
                {
                    if (float.TryParse(token, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float f))
                        values.Add(f);
                }
                i = j + 1;
            }
            return values.ToArray();
        }

        // ============================================================
        // 评分 + 关键词加权
        // ============================================================

        private static List<RagSnippet> ScoreAndPickTopK(float[] queryVec, string query, int topK, float minScore)
        {
            // 关键词路由：根据玩家输入命中类别，给该类别一个加分
            var catBonus = ResolveCategoryBonus(query);
            // 关键词路由：含特定词的条目本身加分（不分类别），强化精确命中
            var entryBonus = ResolveEntryBonus(query);

            int n = _entries.Count;
            int K = Mathf.Clamp(topK, 1, 20);

            // 用一个长度 K 的小堆来维护 top-K（这里直接用插入排序，K 很小）
            var top = new List<KeyValuePair<float, IndexEntry>>(K + 1);
            for (int i = 0; i < n; i++)
            {
                var entry = _entries[i];
                float dot = Dot(queryVec, entry.Vec);   // 已归一化，dot == cosine
                float score = dot;
                if (catBonus.TryGetValue(entry.Category, out float cb)) score += cb;
                foreach (var kv in entryBonus)
                {
                    if (entry.Ja.IndexOf(kv.Key, StringComparison.Ordinal) >= 0
                        || entry.Zh.IndexOf(kv.Key, StringComparison.Ordinal) >= 0)
                    {
                        score += kv.Value;
                    }
                }

                if (score < minScore) continue;
                InsertIntoTop(top, K, score, entry);
            }

            var result = new List<RagSnippet>(top.Count);
            foreach (var kv in top)
            {
                result.Add(new RagSnippet
                {
                    Category = kv.Value.Category,
                    Ja = kv.Value.Ja,
                    Zh = kv.Value.Zh,
                    Score = kv.Key
                });
            }
            return result;
        }

        private static void InsertIntoTop(List<KeyValuePair<float, IndexEntry>> top, int K, float score, IndexEntry entry)
        {
            // top 按降序保持
            int pos = top.Count;
            for (int i = 0; i < top.Count; i++)
            {
                if (score > top[i].Key) { pos = i; break; }
            }
            top.Insert(pos, new KeyValuePair<float, IndexEntry>(score, entry));
            if (top.Count > K) top.RemoveAt(top.Count - 1);
        }

        private static Dictionary<string, float> ResolveCategoryBonus(string query)
        {
            var d = new Dictionary<string, float>();
            string lower = query.ToLowerInvariant();

            void Add(string cat, float v)
            {
                if (d.TryGetValue(cat, out float cur)) d[cat] = Mathf.Max(cur, v);
                else d[cat] = v;
            }

            // 番茄钟 / 工作 / 学习
            if (lower.Contains("番茄") || lower.Contains("ポモドーロ") || lower.Contains("pomodoro")
                || lower.Contains("工作") || lower.Contains("作業") || lower.Contains("作业")
                || lower.Contains("学习") || lower.Contains("勉強") || lower.Contains("勉强"))
            {
                Add("pomodoro", 0.02f);
                Add("smalltalk", 0.01f);
            }

            // 累 / 困 / 休息
            if (lower.Contains("累") || lower.Contains("疲") || lower.Contains("困")
                || lower.Contains("眠") || lower.Contains("休") || lower.Contains("休憩"))
            {
                Add("motion", 0.02f);
                Add("clickheroine", 0.01f);
            }

            // 加油 / 鼓励
            if (lower.Contains("加油") || lower.Contains("頑張") || lower.Contains("頑张")
                || lower.Contains("一緒") || lower.Contains("一起"))
            {
                Add("smalltalk", 0.04f);
                Add("motion", 0.03f);
            }

            // 小说 / 灵感
            if (lower.Contains("小说") || lower.Contains("小説") || lower.Contains("灵感")
                || lower.Contains("ネタ") || lower.Contains("sf") || lower.Contains("剧情"))
            {
                Add("smalltalk", 0.04f);
                Add("selftalk", 0.04f);
                Add("main_general", 0.02f);
            }

            // 飞机 / 宇宙 / 工学
            if (lower.Contains("飞机") || lower.Contains("飛行") || lower.Contains("宇宙")
                || lower.Contains("工学") || lower.Contains("力学") || lower.Contains("大学"))
            {
                Add("smalltalk", 0.03f);
                Add("selftalk", 0.03f);
                Add("main_general", 0.03f);
            }

            return d;
        }

        private static Dictionary<string, float> ResolveEntryBonus(string query)
        {
            // 含特定关键词的条目加分（不限类别）
            var d = new Dictionary<string, float>(StringComparer.Ordinal);
            string lower = query.ToLowerInvariant();

            if (lower.Contains("扣扣") || query.Contains("コウ") || query.Contains("こう"))
            {
                d["コウちゃん"] = 0.10f;
                d["扣扣"] = 0.10f;
            }
            if (lower.Contains("番茄") || lower.Contains("ポモドーロ") || lower.Contains("pomodoro"))
            {
                d["ポモドーロ"] = 0.05f;
                d["番茄"] = 0.05f;
            }
            return d;
        }

        // ============================================================
        // 工具函数
        // ============================================================

        private static float Dot(float[] a, float[] b)
        {
            // 已归一化，直接点积；展开 4 路减少分支
            int n = a.Length;
            int i = 0;
            float s0 = 0, s1 = 0, s2 = 0, s3 = 0;
            for (; i + 3 < n; i += 4)
            {
                s0 += a[i] * b[i];
                s1 += a[i + 1] * b[i + 1];
                s2 += a[i + 2] * b[i + 2];
                s3 += a[i + 3] * b[i + 3];
            }
            float s = s0 + s1 + s2 + s3;
            for (; i < n; i++) s += a[i] * b[i];
            return s;
        }

        private static void NormalizeInPlace(float[] v)
        {
            double sum = 0;
            for (int i = 0; i < v.Length; i++) sum += (double)v[i] * v[i];
            float norm = (float)Math.Sqrt(sum);
            if (norm <= 1e-8f) return;
            float inv = 1f / norm;
            for (int i = 0; i < v.Length; i++) v[i] *= inv;
        }

        private static string JsonEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length + 16);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:
                        if (c < 0x20) sb.AppendFormat("\\u{0:x4}", (int)c);
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= max ? s : s.Substring(0, max) + "...";
        }

        // ============================================================
        // Prompt 注入：把 snippets 渲染成可附加到 system prompt 末尾的段落
        // ============================================================

        public static string FormatSnippetsForPrompt(List<RagSnippet> snippets)
        {
            if (snippets == null || snippets.Count == 0) return "";

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== 参考語境（語気の参考のみ） ===");
            sb.AppendLine("下記は原作で似たトピックに聡音が話していた台詞。**語気・テンション・口癖**を参考にするだけで、文や単語をそのまま使ってはいけない。");
            sb.AppendLine("【厳守事項】");
            sb.AppendLine("1. ユーザーの最新発言に直接答えること。参考台詞の話題に逸れない。");
            sb.AppendLine("2. ユーザーが既に述べた事実（例：「もう4回やった」「コーヒーを淹れに行く」など）を反問しない。");
            sb.AppendLine("3. 参考台詞中の「設計図」「ネガティブ」など特殊な単語は、ユーザーの話題と無関係なら使わない。");
            sb.AppendLine("4. 参考台詞は3つ以下しか提示されないが、すべて使う必要はない。1つも合わなければ無視してよい。");
            sb.AppendLine("【カテゴリの意味】");
            sb.AppendLine("- (selftalk)(clickheroine) は聡音が一人でつぶやく独り言。**ユーザーへの返答ではない**。返答テンプレとして使うと文脈が破綻する。");
            sb.AppendLine("- (other) は他のキャラの台詞や雑多な行。**そのまま自分の台詞にしない**。");
            sb.AppendLine("- (main_general) は本編の聡音の発言。語気だけ参考にする。");
            sb.AppendLine("--- 参考台詞 ---");
            for (int i = 0; i < snippets.Count; i++)
            {
                var s = snippets[i];
                sb.Append("- (").Append(s.Category).Append(") 「").Append(s.Ja).AppendLine("」");
            }
            return sb.ToString();
        }
    }
}
