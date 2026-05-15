using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using AIChat.Core;
using AIChat.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace AIChat.Services
{
    /// <summary>
    /// 「今日（このセッションが始まる前）に聡音の身に起きた、ささやかな出来事」を
    /// LLM 自身に **一度だけ** 事前生成させ、その結果を会話 prompt の LIVE_CONTEXT に注入する。
    ///
    /// 設計の前提（2026-05-07 の方針確認に基づく）：
    /// - 「単一モデル・複数権重」：チャット用と全く同じ backend を使い、system prompt と
    ///   生成パラメータだけ切り替える。将来 LLM を内嵌（local runtime）に置換しても、
    ///   ここは <see cref="BackendConfig"/> の中身が変わるだけで業務ロジックは無改修で済む。
    /// - 「失敗時は空でよい」：失敗しても <see cref="TodayStory"/> を空のまま残し、
    ///   mod 側の硬兜底と persona 规则で「今日は大きな出来事はなかった」と素直に答えさせる。
    /// - 「1 セッション 1 回」：頻回再生成は世界観の不安定化と token 浪費を招くだけ。
    ///   ユーザーが手動でリセットするか、日付が変わった時だけ作り直す。
    /// </summary>
    public static class DailyStoryGenerator
    {
        public struct BackendConfig
        {
            public string ApiUrl;
            public string ApiKey;
            public string ModelName;
            public bool UseLocalOllama;
            public bool FixApiPathForThinkMode;
            public float TimeoutSeconds;
        }

        // 注入時に LiveContextProvider から呼ぶ唯一の出口。null/空なら何も注入されない。
        public static string TodayStory => _todayStory ?? string.Empty;
        public static bool IsReady => _isReady;
        public static bool IsGenerating => _isGenerating;
        public static DateTime GeneratedAt => _generatedAt;
        public static string LastError => _lastError ?? string.Empty;

        private static volatile string _todayStory = "";
        private static volatile bool _isReady = false;
        private static volatile bool _isGenerating = false;
        private static volatile string _lastError = "";
        private static DateTime _generatedAt = DateTime.MinValue;
        private static int _directReplyCursor = 0;
        private static int _lastSharedIndex = -1;
        private static readonly object _lock = new object();

        private struct StoryEntry
        {
            public string Ja;
            public string Zh;
        }

        /// <summary>
        /// 既にこのゲーム起動セッションで生成済みなら何もしない。
        /// 静的フィールドはゲーム再起動で消えるため、次回起動時は必ず新規生成される。
        /// </summary>
        public static IEnumerator GenerateIfNeeded(BackendConfig cfg)
        {
            lock (_lock)
            {
                if (_isGenerating)
                {
                    Log.Info("[DailyStory] 既に生成中。スキップ");
                    yield break;
                }
                if (_isReady)
                {
                    Log.Info($"[DailyStory] 既に {_generatedAt:HH:mm} に生成済み（この起動セッション内）。スキップ");
                    yield break;
                }
                _isGenerating = true;
                _lastError = "";
            }

            try
            {
                yield return GenerateInternal(cfg);
            }
            finally
            {
                _isGenerating = false;
            }
        }

        /// <summary>手動で次回 GenerateIfNeeded を強制再生成させる。</summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _todayStory = "";
                _isReady = false;
                _generatedAt = DateTime.MinValue;
                _lastError = "";
                _directReplyCursor = 0;
                _lastSharedIndex = -1;
            }
        }

        /// <summary>LiveContextProvider 専用のフォーマッタ。</summary>
        public static string BuildSnippetForPrompt()
        {
            List<StoryEntry> entries = ParseStoredEntries(TodayStory);
            if (entries.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("- 通話開始前の今日（今日の出来事メモ ※話題に出てきた時 or 自然な流れの時だけ素材として使う。見出しや項目名をそのまま読み上げない）：");
            foreach (var e in entries)
            {
                string t = e.Ja.Trim();
                if (string.IsNullOrEmpty(t)) continue;
                if (!t.StartsWith("·") && !t.StartsWith("・")) t = "· " + t.TrimStart('-', '*', ' ');
                sb.AppendLine("    " + t);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 用户直接询问“今天/最近有什么事”时，mod 直接使用预生成素材回答，
        /// 不再让主模型自由发挥成“有一件小事/等你追问”的悬念。
        /// </summary>
        public static bool TryBuildDirectReply(string prompt, out string voiceJa, out string subtitleZh)
        {
            voiceJa = "";
            subtitleZh = "";
            List<StoryEntry> entries = ParseStoredEntries(TodayStory);
            if (entries.Count == 0) return false;

            bool asksAnother = IsAskingAnother(prompt);
            bool asksFollowup = IsAskingFollowup(prompt);
            int idx = PickDirectReplyIndex(entries, asksAnother, asksFollowup);
            StoryEntry e = entries[idx];
            string ja = CleanLine(e.Ja);
            string zh = CleanLine(e.Zh);
            if (string.IsNullOrWhiteSpace(ja) || string.IsNullOrWhiteSpace(zh)) return false;

            _lastSharedIndex = idx;
            if (asksAnother)
            {
                voiceJa = "他にはね、" + ja;
                subtitleZh = "还有的话，" + zh;
            }
            else if (asksFollowup)
            {
                voiceJa = "あ、そのことなんだけど、" + ja;
                subtitleZh = "啊，就是那件事，" + zh;
            }
            else
            {
                voiceJa = "うーん…今日はね、" + ja;
                subtitleZh = "嗯……今天的话，" + zh;
            }
            return true;
        }

        // ======================================================================
        // 内部：プロンプト構築 + LLM 呼び出し + 解析
        // ======================================================================

        private static IEnumerator GenerateInternal(BackendConfig cfg)
        {
            DateTime now = DateTime.Now;
            string sysPrompt = BuildSystemPrompt();
            string userPrompt = BuildUserPrompt(now);

            string body = BuildSimpleChatBody(cfg, sysPrompt, userPrompt);
            string apiUrl = cfg.ApiUrl;
            if (cfg.UseLocalOllama && cfg.FixApiPathForThinkMode && apiUrl.Contains("/v1/chat/completions"))
            {
                apiUrl = apiUrl.Replace("/v1/chat/completions", "/api/chat");
            }

            Log.Info($"[DailyStory] 生成開始 model={cfg.ModelName} url={apiUrl}");
            float t0 = Time.realtimeSinceStartup;

            string raw = null;
            string err = null;

            using (UnityWebRequest req = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                if (!cfg.UseLocalOllama)
                {
                    req.SetRequestHeader("Authorization", "Bearer " + cfg.ApiKey);
                }
                if (cfg.TimeoutSeconds > 0)
                {
                    req.timeout = Mathf.Max(1, Mathf.RoundToInt(cfg.TimeoutSeconds));
                }

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    raw = req.downloadHandler.text;
                }
                else
                {
                    err = $"{req.error} (code={req.responseCode})";
                }
            }

            float dt = Time.realtimeSinceStartup - t0;

            if (err != null)
            {
                _lastError = err;
                Log.Warning($"[DailyStory] 生成失敗 ({dt:F2}s): {err}");
                yield break;
            }

            string content = ExtractAssistantContent(raw, cfg.UseLocalOllama);

            string parsed = ParseTwoBullets(content);
            if (string.IsNullOrWhiteSpace(parsed))
            {
                _lastError = "解析に失敗：有効な bullet（||| 付き）を 2 行以上抽出できなかった";
                Log.Warning($"[DailyStory] 解析失敗 ({dt:F2}s)。raw 抜粋: {Truncate(content, 1200)}");
                yield break;
            }

            lock (_lock)
            {
                _todayStory = parsed;
                _isReady = true;
                _generatedAt = DateTime.Now;
            }
            Log.Info($"[DailyStory] 生成成功 ({dt:F2}s):\n{parsed}");
        }

        private static string BuildSystemPrompt()
        {
            // persona 全文は入れない（token 節約 + 注意散漫の回避）。
            // ここでは「最大 3 行の地味な日常メモを書き出すだけ」のミッションに集中させる。
            return @"あなたは「里染聡音（さとね）」という一人の人間が、
今日（このあと作業通話を始める直前）までに自分の身に起きたささやかな出来事を、
脳内で 3 つだけ思い出して書き留める、その思い出しメモを生成するアシスタントです。

【聡音の身元（要約）】
- 大学の工学部に所属。SF小説の執筆が趣味。宇宙と飛行機が好き、力学は苦手。
- 一人称「私」、口調はやわらかいタメ口。「〜よね」「〜かな」「うーん」「なんていうか」。
- 室内派で、自分の部屋・机・ベランダ・コンビニ程度の生活圏。
- ペンギンのぬいぐるみ「コウちゃん」に思考整理として独り言を投げる癖がある。
- ラジオや鳥の雑学も好きだが、そればかり話題にしない。日常の細部、小説、課題、食べ物、机周りにもよく反応する。
- 自分の作品の悪い評判が気になりがち。完璧主義だが笑いに変える強さもある。

【出力フォーマット（厳守）】
- ちょうど 3 行を出力する。4 行以上、または 2 行以下は禁止。
- 各行は「· 」で始める（中点 + 半角スペース）。
- 各行は「· 日本語 ||| 简体中文」の形にする。日本語は 1〜2 文、80 字以内。中国語は自然な简体中文翻译。
- 余計な見出し・前置き・解説・メタコメント・コードブロックを一切付けない。

【スタイル基準】
- 一人称「私」、口調はタメ口。語尾は「〜た」「〜だった」など過去形の独り言調。
- 各行に **微細な感情の動き** を 1 つだけ含める（小さな満足／軽い苛立ち／ふと気づいた何か／なんとなくの違和感／可笑しさ）。
- **大事件は禁止**：旅行、事故、恋愛発展、新しい人物の追加、卒業、病気、引っ越し、特別なイベント、訪問者など。
- **世界観外も禁止**：カフェに行く、外で人と会う、誰かの家に行く、買い物に出る等は聡音らしくない。
- ジャンルは「机周り・部屋・ベランダ・近所のコンビニ程度」の小事のみ。
- **トーン（錦上添花用）**：聞いて**安心・ほっとする**小さな日常。虫・害虫・ゴキブリ・汚れ・排泄・ホラー・病的な痛みの描写は禁止（肩こり・眠気・目の疲れ程度は OK）。
- 各行は「今日」のことだけ。「昨日」「最近」「明日」のような時間軸の拡大は禁止。
- 3 行は **すべて異なるテーマ**から取る（例：小説、食べ物、机周りなど）。
- ラジオ・鳥・飛行の話題は出してもよいが、**3 行のうち最大 1 行まで**。毎日の定番にしない。
- コウちゃん（ぬいぐるみ）は **3 行のうち 0 行が望ましい**。**どうしても使うなら最大 1 行**。「見せたら一緒にしゃべった」などぬいに戯曲を持たせるのは禁止（現実の物体としてのみ）。
- 無理にオチ・ダジャレ・自己ツッコミを二重につけない。出来事は一つで素直に書く。
- 「面白いことがあった」「ちょっとしたことがあった」のような予告だけで終わらせない。各行の中で必ず何があったかを具体的に言い切る。
- 中国語訳では「奇怪」「怪事」「发生了一件有趣的事」のような抽象・誤解される表現を避ける。何があったかを具体的に訳す。

【良い例】
· 朝、ノートに書いた小説の人物名がどうしてもしっくりこなくて、付箋を三枚も貼り替えたら、逆に少しだけ頭が整理された。 ||| 早上本子里的小说人物名字怎么都不顺眼，我换了三张便利贴，结果反而稍微理清了思路。
· お昼に作ろうとしたツナマヨおにぎりが大きさバラバラで、ラップを広げ直したら逆に形が「個性派」になって、ふと自分で笑っちゃった。 ||| 中午想做金枪鱼蛋黄酱饭团，结果大小完全不一样。重新摊开保鲜膜，反倒变成“个性款”饭团，我自己都乐了。

【悪い例（絶対にやらない）】
· 今日は友達と新しいプロジェクトを始めることになった。 ||| 今天和朋友开始了新项目。 ← 大事件＋人物追加 NG
· 今日もいつも通り作業した。 ||| 今天也和平时一样在做事。 ← 抽象的すぎ NG
· コウちゃんが急に動いてノートを落とした。 ||| 扣扣突然动起来把本子弄掉了。 ← ぬいぐるみが自分で行動しているので NG
· 朝ごはんの牛乳をコウちゃんに見せながら呟いた。 ||| 早餐牛奶给扣扣看并自言自语。 ← ぬい目的の「見せ芝居」過多 NG
· ちょっと面白いことがあった。 ||| 发生了一件有趣的小事。 ← 予告だけで具体性がないので NG";
        }

        private static string BuildUserPrompt(DateTime now)
        {
            string dateStr = now.ToString("yyyy年M月d日", CultureInfo.InvariantCulture);
            string week = WeekJa(now.DayOfWeek);
            string season = SeasonJa(now.Month);
            string timeBand = TimeBandJa(now.Hour);

            // 素材プールは起動セッションごとに揺らす。radio/bird は低頻度アクセント枠に落とす。
            string[] corePool = new[]
            {
                "SF小説の執筆（行き詰まり、急な閃き、矛盾の発見、人物造形に悩むなど）",
                "大学の課題・力学の難しさ・宇宙関連のレポート・読みかけの専門書",
                "窓の外の光・気温の変化・部屋の湿度など、五感で拾えるごく小さい変化",
                "小腹／お茶／お菓子／コンビニの新商品／作業の合間の食べ物の小さな失敗・満足",
                "身体感覚（眠気・肩こり・目が疲れた・冷えた手・伸びをしたら骨が鳴った）",
                "天気・洗濯物・部屋の片付け・机の上の散らかりなど、生活空間で起きた些細なこと",
                "飲み物・マグカップ・ペン・付箋・ノートなど机周りの道具にまつわる小さな失敗や満足"
            };
            string[] accentPool = new[]
            {
                "ラジオから流れてきた話題（音楽・天気予報・人生相談など）に反応した小事 ※採用するなら鳥や飛行以外を優先",
                "鳥の聞きなしや雑学を思い出して一人でくすっとした瞬間 ※低頻度枠"
            };

            // 日付固定ではなく、起動ごとに変える。本セッション内は一度しか生成しないため一貫性は保てる。
            int seed = Guid.NewGuid().GetHashCode() ^ Environment.TickCount;
            var rnd = new System.Random(seed);
            var picked = new List<string>();
            var idxs = new List<int>();
            for (int i = 0; i < corePool.Length; i++) idxs.Add(i);
            for (int k = 0; k < 3 && idxs.Count > 0; k++)
            {
                int j = rnd.Next(idxs.Count);
                picked.Add(corePool[idxs[j]]);
                idxs.RemoveAt(j);
            }
            if (rnd.NextDouble() < 0.25)
            {
                picked.Add(accentPool[rnd.Next(accentPool.Length)]);
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== 今日の状況 ===");
            sb.AppendLine($"- 日付：{dateStr}（{week}）");
            sb.AppendLine($"- 季節：{season}");
            sb.AppendLine($"- 通話を始める今の時間帯：{timeBand}（{now:HH:mm}）");
            sb.AppendLine();
            sb.AppendLine("=== 今日のテーマ候補（参考にし、違う系統を混ぜると良い） ===");
            foreach (var p in picked) sb.AppendLine($"- {p}");
            sb.AppendLine();
            sb.AppendLine("では、上記の制約と良い例のスタイルに厳密に従って、");
            sb.AppendLine("今日（通話開始前まで）に起きた、ちょうど 3 行の思い出しメモを出力してください。");
            sb.AppendLine("出力は bullet 3 行のみ。各行は必ず「· 日本語 ||| 简体中文」。前置き・後書き・装飾は一切不要。");
            return sb.ToString();
        }

        private static string ParseTwoBullets(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";
            string cleaned = LLMUtils.StripReasoningBlocks(raw).Trim();

            var bullets = new List<string>();
            foreach (var line in cleaned.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string t = line.Trim();
                if (t.Length == 0) continue;
                // 受理する prefix：· ・ - * 1. 2. （1） 等
                t = Regex.Replace(t, @"^[\s\-\*\u30FB\u00B7\u2022]+", "").Trim();
                t = Regex.Replace(t, @"^[\(\（]?\d+[\.\)\）]\s*", "").Trim();
                if (t.Length == 0) continue;

                // メタっぽい行・見出しは捨てる
                if (Regex.IsMatch(t, @"^(出力|フォーマット|スタイル|テーマ|今日|例|System|User|Assistant|注意|条件|ルール|Output|Format)[:：]")) continue;
                if (t.StartsWith("【") || t.StartsWith("===")) continue;
                if (!t.Contains("|||")) continue;
                if (t.Length > 260) t = t.Substring(0, 260);
                bullets.Add(t);
                if (bullets.Count >= 3) break;
            }

            if (bullets.Count == 0) return "";
            // 目標は 3 行だが、モデルが 2 行しか出さない場合は 2 行でも採用（解析バグで全部捨てるよりマシ）。
            if (bullets.Count < 2) return "";

            var sb = new StringBuilder();
            for (int i = 0; i < bullets.Count; i++)
            {
                sb.Append("· ").AppendLine(bullets[i]);
            }
            return sb.ToString().TrimEnd();
        }

        private static List<StoryEntry> ParseStoredEntries(string stored)
        {
            var list = new List<StoryEntry>();
            if (string.IsNullOrWhiteSpace(stored)) return list;

            foreach (var line in stored.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string t = line.Trim();
                t = Regex.Replace(t, @"^[\s\-\*\u30FB\u00B7\u2022]+", "").Trim();
                string[] parts = t.Split(new[] { "|||" }, StringSplitOptions.None);
                if (parts.Length < 2) continue;
                string ja = CleanLine(parts[0]);
                string zh = CleanLine(parts[1]);
                if (ja.Length == 0 || zh.Length == 0) continue;
                list.Add(new StoryEntry { Ja = ja, Zh = zh });
            }
            return list;
        }

        private static int PickDirectReplyIndex(List<StoryEntry> entries, bool asksAnother, bool asksFollowup)
        {
            if (entries == null || entries.Count == 0) return 0;

            if (asksFollowup && !asksAnother && _lastSharedIndex >= 0 && _lastSharedIndex < entries.Count)
            {
                return _lastSharedIndex;
            }

            int start = Math.Abs(_directReplyCursor) % entries.Count;
            for (int pass = 0; pass < entries.Count; pass++)
            {
                int idx = (start + pass) % entries.Count;
                if (!IsRadioBirdLike(entries[idx].Ja))
                {
                    _directReplyCursor = idx + 1;
                    return idx;
                }
            }

            _directReplyCursor = start + 1;
            return start;
        }

        private static bool IsAskingAnother(string prompt)
        {
            string p = prompt ?? "";
            return Regex.IsMatch(p, "还有|還有|别的|別的|另一个|另一個|他に|ほか|他には|もっと");
        }

        private static bool IsAskingFollowup(string prompt)
        {
            string p = prompt ?? "";
            return Regex.IsMatch(p, "什么事|什麼事|是什么|是什麼|哪件|具体|细说|細說|それ|そのこと|詳しく|どういう");
        }

        private static bool IsRadioBirdLike(string ja)
        {
            if (string.IsNullOrEmpty(ja)) return false;
            return Regex.IsMatch(ja, "ラジオ|鳥|鳴き声|聞きなし|飛ぶ|飛行|翼");
        }

        private static string CleanLine(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            string t = s.Trim();
            t = Regex.Replace(t, @"^[\s\-\*\u30FB\u00B7\u2022]+", "").Trim();
            return t.Trim();
        }

        // ======================================================================
        // backend 呼び出し用の最小チャットボディ（LLMUtils.BuildRequestBody は LIVE_CONTEXT を
        // 注入してしまい再帰的になるため、こちらでは使わず軽量版を直接組み立てる）
        // ======================================================================
        private static string ExtractAssistantContent(string raw, bool useLocalOllama)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "";

            // Ollama /api/chat can return either a single JSON object (stream:false)
            // or many JSONL chunks (stream:true). Aggregate every message.content chunk
            // instead of taking the first one, because Qwen3 may emit an empty first chunk.
            var sb = new StringBuilder();
            foreach (Match m in Regex.Matches(raw, "\"content\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"", RegexOptions.Singleline))
            {
                string part = Regex.Unescape(m.Groups[1].Value);
                if (!string.IsNullOrEmpty(part)) sb.Append(part);
            }
            if (sb.Length > 0) return sb.ToString();

            string fallback = useLocalOllama
                ? ResponseParser.ExtractContentFromOllama(raw)
                : ResponseParser.ExtractContentRegex(raw);
            return fallback ?? "";
        }

        private static string BuildSimpleChatBody(BackendConfig cfg, string sys, string user)
        {
            string model = cfg.ModelName ?? "";
            string extraJson = @", ""temperature"": 0.72";
            if (cfg.UseLocalOllama)
            {
                // 与主对话请求保持一致：Qwen3 默认 think/stream 会让启动素材生成变慢且难以解析。
                extraJson += @", ""stream"": false, ""keep_alive"": ""30m"", ""think"": false";
            }

            // gemma 系は system role を解釈しない。チャット側と同じ条件分岐を踏襲。
            if (!string.IsNullOrEmpty(model) && model.IndexOf("gemma", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string merged = $"[System Instruction]\n{sys}\n\n[User Message]\n{user}";
                return $@"{{ ""model"": ""{model}"", ""messages"": [ {{ ""role"": ""user"", ""content"": ""{ResponseParser.EscapeJson(merged)}"" }} ]{extraJson} }}";
            }
            return $@"{{ ""model"": ""{model}"", ""messages"": [ {{ ""role"": ""system"", ""content"": ""{ResponseParser.EscapeJson(sys)}"" }}, {{ ""role"": ""user"", ""content"": ""{ResponseParser.EscapeJson(user)}"" }} ]{extraJson} }}";
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length <= max ? s : s.Substring(0, max) + "…";
        }

        private static string WeekJa(DayOfWeek d)
        {
            switch (d)
            {
                case DayOfWeek.Monday: return "月曜日";
                case DayOfWeek.Tuesday: return "火曜日";
                case DayOfWeek.Wednesday: return "水曜日";
                case DayOfWeek.Thursday: return "木曜日";
                case DayOfWeek.Friday: return "金曜日";
                case DayOfWeek.Saturday: return "土曜日";
                default: return "日曜日";
            }
        }

        private static string SeasonJa(int month)
        {
            if (month >= 3 && month <= 5) return "春";
            if (month >= 6 && month <= 8) return "夏";
            if (month >= 9 && month <= 11) return "秋";
            return "冬";
        }

        private static string TimeBandJa(int hour)
        {
            if (hour > 5 && hour < 11) return "朝";
            if (hour >= 11 && hour < 17) return "昼";
            if (hour >= 17 && hour < 20) return "夕方";
            return "夜";
        }
    }
}
