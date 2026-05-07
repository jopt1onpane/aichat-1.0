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
    ///   persona 側のルールで「今日は特に変わったことはなかった」と素直に答えさせる。
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
        private static readonly object _lock = new object();

        /// <summary>
        /// 既に当日生成済みかつ日付が変わっていなければ何もしない。
        /// それ以外は async に LLM を叩き、成功したら <see cref="TodayStory"/> を更新する。
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
                if (_isReady && _generatedAt.Date == DateTime.Now.Date)
                {
                    Log.Info($"[DailyStory] 既に {_generatedAt:HH:mm} に生成済み（同日）。スキップ");
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
            }
        }

        /// <summary>LiveContextProvider 専用のフォーマッタ。</summary>
        public static string BuildSnippetForPrompt()
        {
            string s = TodayStory;
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine("- 通話開始前の今日（今日の出来事メモ ※話題に出てきた時 or 自然な流れの時だけ素材として使う。見出しや項目名をそのまま読み上げない）：");
            foreach (var line in s.Split('\n'))
            {
                string t = line.Trim();
                if (string.IsNullOrEmpty(t)) continue;
                if (!t.StartsWith("·") && !t.StartsWith("・")) t = "· " + t.TrimStart('-', '*', ' ');
                sb.AppendLine("    " + t);
            }
            return sb.ToString();
        }

        // ======================================================================
        // 内部：プロンプト構築 + LLM 呼び出し + 解析
        // ======================================================================

        private static IEnumerator GenerateInternal(BackendConfig cfg)
        {
            DateTime now = DateTime.Now;
            string sysPrompt = BuildSystemPrompt();
            string userPrompt = BuildUserPrompt(now);

            string body = BuildSimpleChatBody(cfg.ModelName, sysPrompt, userPrompt);
            string apiUrl = cfg.ApiUrl;

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

            string content = cfg.UseLocalOllama
                ? ResponseParser.ExtractContentFromOllama(raw)
                : ResponseParser.ExtractContentRegex(raw);

            string parsed = ParseTwoBullets(content);
            if (string.IsNullOrWhiteSpace(parsed))
            {
                _lastError = "解析に失敗：2 行の bullet を抽出できなかった";
                Log.Warning($"[DailyStory] 解析失敗 ({dt:F2}s)。raw 抜粋: {Truncate(content, 240)}");
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
            // ここでは「2 行の地味な日常素材を書き出すだけ」のミッションに集中させる。
            return @"あなたは「里染聡音（さとね）」という一人の人間が、
今日（このあと作業通話を始める直前）までに自分の身に起きたささやかな出来事を、
脳内で 2 つだけ思い出して書き留める、その思い出しメモを生成するアシスタントです。

【聡音の身元（要約）】
- 大学の工学部に所属。SF小説の執筆が趣味。宇宙と飛行機が好き、力学は苦手。
- 一人称「私」、口調はやわらかいタメ口。「〜よね」「〜かな」「うーん」「なんていうか」。
- 室内派で、自分の部屋・机・ベランダ・コンビニ程度の生活圏。
- ペンギンのぬいぐるみ「コウちゃん」に思考整理として独り言を投げる癖がある。
- ラジオを聞きながら作業するのが好き。鳥の聞きなしや雑学が好き。
- 自分の作品の悪い評判が気になりがち。完璧主義だが笑いに変える強さもある。

【出力フォーマット（厳守）】
- ちょうど 2 行を出力する。3 行以上、または 1 行だけは禁止。
- 各行は「· 」で始める（中点 + 半角スペース）。
- 各行は 1〜2 文、合計 80 字以内。
- 余計な見出し・前置き・解説・メタコメント・コードブロックを一切付けない。

【スタイル基準】
- 一人称「私」、口調はタメ口。語尾は「〜た」「〜だった」など過去形の独り言調。
- 各行に **微細な感情の動き** を 1 つだけ含める（小さな満足／軽い苛立ち／ふと気づいた何か／なんとなくの違和感／可笑しさ）。
- **大事件は禁止**：旅行、事故、恋愛発展、新しい人物の追加、卒業、病気、引っ越し、特別なイベント、訪問者など。
- **世界観外も禁止**：カフェに行く、外で人と会う、誰かの家に行く、買い物に出る等は聡音らしくない。
- ジャンルは「机周り・部屋・ベランダ・近所のコンビニ程度」の小事のみ。
- 各行は「今日」のことだけ。「昨日」「最近」「明日」のような時間軸の拡大は禁止。
- 2 行は **異なるテーマ**から取る（例：1 つは小説関連、もう 1 つは食べ物や身体感覚など）。

【良い例】
· 朝、ラジオで月に空気が無いって話を改めて聞いて、第3章の描写と矛盾してる気がして、慌ててノートに書き直し案をメモした。
· お昼に作ろうとしたツナマヨおにぎりが大きさバラバラで、コウちゃんに見せたら「これは芸術だね」って自分でフォローしてしまった。

【悪い例（絶対にやらない）】
· 今日は友達と新しいプロジェクトを始めることになった。 ← 大事件＋人物追加 NG
· 今日もいつも通り作業した。 ← 抽象的すぎ NG
· 散歩がてら近所のカフェに寄って読書した。 ← 室外・他人空間 NG
· 朝起きて、ご飯を食べて、散歩して、勉強して、夜寝た。 ← 列挙＋無感情 NG";
        }

        private static string BuildUserPrompt(DateTime now)
        {
            string dateStr = now.ToString("yyyy年M月d日", CultureInfo.InvariantCulture);
            string week = WeekJa(now.DayOfWeek);
            string season = SeasonJa(now.Month);
            string timeBand = TimeBandJa(now.Hour);

            // 素材プールはセッション毎にランダム 2 個に絞り、毎回違うテーマが上がりやすくする。
            string[] poolAll = new[]
            {
                "SF小説の執筆（行き詰まり、急な閃き、矛盾の発見、人物造形に悩むなど）",
                "大学の課題・力学の難しさ・宇宙関連のレポート・読みかけの専門書",
                "ペンギンのぬいぐるみ「コウちゃん」とのちょっとしたやりとり（独り言の相手）",
                "ラジオから流れてきた話題（雑学・音楽・天気予報・人生相談など）に反応した小事",
                "小腹／お茶／お菓子／コンビニの新商品／作業の合間の食べ物の小さな失敗・満足",
                "身体感覚（眠気・肩こり・目が疲れた・冷えた手・伸びをしたら骨が鳴った）",
                "天気・窓の外の鳥・洗濯物・部屋の片付けなど、ベランダや窓辺で起きた些細なこと",
                "鳥の聞きなしや雑学を思い出して一人でくすっとした瞬間"
            };

            // 当日の HashCode で再現性のある選択（同じ日なら同じプール選定）
            int seed = (now.Year * 10000 + now.Month * 100 + now.Day);
            var rnd = new System.Random(seed);
            var picked = new List<string>();
            var idxs = new List<int>();
            for (int i = 0; i < poolAll.Length; i++) idxs.Add(i);
            for (int k = 0; k < 3 && idxs.Count > 0; k++)
            {
                int j = rnd.Next(idxs.Count);
                picked.Add(poolAll[idxs[j]]);
                idxs.RemoveAt(j);
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== 今日の状況 ===");
            sb.AppendLine($"- 日付：{dateStr}（{week}）");
            sb.AppendLine($"- 季節：{season}");
            sb.AppendLine($"- 通話を始める今の時間帯：{timeBand}（{now:HH:mm}）");
            sb.AppendLine();
            sb.AppendLine("=== 今日のテーマ候補（ここから 2 つを選んで素材にする。違う系統から 1 つずつが望ましい） ===");
            foreach (var p in picked) sb.AppendLine($"- {p}");
            sb.AppendLine();
            sb.AppendLine("では、上記の制約と良い例のスタイルに厳密に従って、");
            sb.AppendLine("今日（通話開始前まで）に起きた、ちょうど 2 行の思い出しメモを出力してください。");
            sb.AppendLine("出力は bullet 2 行のみ。前置き・後書き・装飾は一切不要。");
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
                if (t.Length > 200) t = t.Substring(0, 200);
                bullets.Add(t);
                if (bullets.Count >= 2) break;
            }

            if (bullets.Count == 0) return "";
            // 2 行揃わなくても 1 行だけ採用するのはやめる（品質下振れ防止 → 空にして persona 側で素直に答えさせる）
            if (bullets.Count < 2) return "";

            var sb = new StringBuilder();
            for (int i = 0; i < bullets.Count; i++)
            {
                sb.Append("· ").AppendLine(bullets[i]);
            }
            return sb.ToString().TrimEnd();
        }

        // ======================================================================
        // backend 呼び出し用の最小チャットボディ（LLMUtils.BuildRequestBody は LIVE_CONTEXT を
        // 注入してしまい再帰的になるため、こちらでは使わず軽量版を直接組み立てる）
        // ======================================================================
        private static string BuildSimpleChatBody(string model, string sys, string user)
        {
            // gemma 系は system role を解釈しない。チャット側と同じ条件分岐を踏襲。
            if (!string.IsNullOrEmpty(model) && model.IndexOf("gemma", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string merged = $"[System Instruction]\n{sys}\n\n[User Message]\n{user}";
                return $@"{{ ""model"": ""{model}"", ""messages"": [ {{ ""role"": ""user"", ""content"": ""{ResponseParser.EscapeJson(merged)}"" }} ], ""temperature"": 0.85 }}";
            }
            return $@"{{ ""model"": ""{model}"", ""messages"": [ {{ ""role"": ""system"", ""content"": ""{ResponseParser.EscapeJson(sys)}"" }}, {{ ""role"": ""user"", ""content"": ""{ResponseParser.EscapeJson(user)}"" }} ], ""temperature"": 0.85 }}";
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
