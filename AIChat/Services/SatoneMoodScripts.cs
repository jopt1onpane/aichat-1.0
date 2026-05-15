using System;
using System.Text.RegularExpressions;

namespace AIChat.Services
{
    /// <summary>
    /// 「今日どんな感じ？」「調子どう」など **聡音本人の今日の体感** に答える手書き台詞。
    /// 文言は <c>game-decompiled/heroine_all_dialogues.json</c> の <b>ZhHans</b> 公式訳を第一参照に、
    /// <c>clickheroine_work_*</c> / <c>clickheroine_rest_*</c> と文体を揃える（例：创作、集中精神、专注、状态、还不错、哦、嘛）。
    /// 日本語はゲーム短台詞の語感；中国語は上記訳文の言い回しに寄せる。
    /// **空虚・寂しさの哲学モノローグ**はここに置かない（プレイヤー側の訴えは主 LLM へ）。
    /// </summary>
    public static class SatoneMoodScripts
    {
        private static readonly object _rngLock = new object();
        private static Random _rng = new Random(Environment.TickCount ^ Guid.NewGuid().GetHashCode());

        /// <summary>ユーザーが「何かあった？」系なら false（DailyStory 小事ルートへ）。</summary>
        public static bool IsConcreteEventSeeking(string prompt)
        {
            string p = prompt ?? "";
            return Regex.IsMatch(p,
                @"什么.*事|什麼.*事|什么事|什麼事|发生了什么|發生了什麼|遇到.*事|碰到|分享.*事|聊聊.*事|有沒有.*事|有没有.*事|见闻|見聞|何かあった|なにかあった|面白いこと|有趣.*事|好玩.*事|发生了什么好玩|段子");
        }

        /// <summary>聡音の **今日の調子・気分・体調** を聞いているか。（空虚/寂しさの語は含めない → 主会話へ）</summary>
        public static bool IsMoodOrWellbeingQuestion(string prompt)
        {
            if (IsConcreteEventSeeking(prompt)) return false;
            string p = prompt ?? "";
            if (p.Trim().Length == 0) return false;

            if (Regex.IsMatch(p, @"过得怎么样|過得怎樣|过得如何|过得还好吗|今天还好吗|今日还好吗|近况|近況|最近还好吗|最近怎样|最近如何|この頃")) return true;
            if (Regex.IsMatch(p, @"最近どう|最近はどう|調子はどう|調子どう|ごきげん")) return true;
            if (Regex.IsMatch(p, @"今[日天].{0,8}(怎么样|怎樣|如何|还好吗|好不)")) return true;
            if (Regex.IsMatch(p, @"心情|情绪|情緒|気分|機嫌")) return true;
            if (Regex.IsMatch(p, @"开不开心|開不開心|高不高兴|高興|快不快乐|快乐吗|開心嗎|开心吗|傷心|难过|難過|委屈")) return true;
            if (Regex.IsMatch(p, @"累不累|疲れ|疲れた|疲倦|吃力|撑不住|扛不住")) return true;
            // 干劲・前向きの聞き方
            if (Regex.IsMatch(p, @"有干劲|有幹勁|打起精神|元気|精神|順調|还顺利|還順利|状态|狀態")) return true;
            if (Regex.IsMatch(p, @"还好吗|还好吧|大丈夫|平気|没事吧|不要紧吧")) return true;
            return false;
        }

        /// <param name="pomodoroPhase">ゲームのポモフェーズ。取得できなければ null または空。<c>Work</c> / <c>Break</c>。</param>
        public static bool TryPickLine(string userPrompt, string pomodoroPhase, out string voiceJa, out string subtitleZh)
        {
            voiceJa = "";
            subtitleZh = "";
            MoodKind k = ClassifyMood(userPrompt, pomodoroPhase);
            string[] jaPool;
            string[] zhPool;
            GetPool(k, out jaPool, out zhPool);
            if (jaPool == null || zhPool == null || jaPool.Length == 0 || jaPool.Length != zhPool.Length)
                return false;

            int i;
            lock (_rngLock)
            {
                i = _rng.Next(jaPool.Length);
            }
            voiceJa = jaPool[i];
            subtitleZh = zhPool[i];
            return true;
        }

        /// <summary>ゲーム台詞集（clickheroine_work / clickheroine_rest）に沿った「今日の体感」の型。</summary>
        private enum MoodKind
        {
            /// <summary>HeroineClickNormal / SmallTalk 寄り：通話のベースライン。</summary>
            GeneralCalm,
            /// <summary>やる気・調子良し（Joy/Guts の手前）。</summary>
            Upbeat,
            /// <summary>疲労（Tired / Stretch / 長時間デスク）。</summary>
            Tired,
            /// <summary>HeroineClickWork：作業に膝をつけている日。</summary>
            WorkFocus,
            /// <summary>HeroineClickBreak：休憩帯・緩み。</summary>
            BreakRelaxed,
            /// <summary>課題・力学のジレンマ（Frustration 手前、完了ではない）。</summary>
            StudyFriction,
            /// <summary>執筆・着想（Think / ものづくりの手応え）。</summary>
            CreativeTinge,
            /// <summary>「嬉しい？」直球への率直回答。</summary>
            AskedHappy
        }

        private static MoodKind ClassifyMood(string p, string pomodoroPhase)
        {
            string phase = (pomodoroPhase ?? "").Trim();

            if (Regex.IsMatch(p, @"累|疲|吃力|撑不住|扛不住")) return MoodKind.Tired;
            if (Regex.IsMatch(p, @"开不开心|開不開心|高不高兴|快不快乐|开心吗|開心嗎|快乐吗|嬉しい")) return MoodKind.AskedHappy;
            if (Regex.IsMatch(p, @"力学|課題|締切|死线|死線|焦虑|焦れ|レポート")) return MoodKind.StudyFriction;
            if (Regex.IsMatch(p, @"小说|小說|ネタ|執筆|灵感|靈感|原稿|キャラ")) return MoodKind.CreativeTinge;
            if (Regex.IsMatch(p, @"有干劲|有幹勁|打起精神|元気|精神|順調|顺利|順利|状态|狀態|調子いい")) return MoodKind.Upbeat;

            if (phase.Equals("Break", StringComparison.OrdinalIgnoreCase))
                return MoodKind.BreakRelaxed;
            if (phase.Equals("Work", StringComparison.OrdinalIgnoreCase))
                return MoodKind.WorkFocus;

            return MoodKind.GeneralCalm;
        }

        private static void GetPool(MoodKind k, out string[] ja, out string[] zh)
        {
            switch (k)
            {
                case MoodKind.Upbeat:
                    ja = new[]
                    {
                        "結構進んだ！気がする…ってほどじゃないけど、前には進めた。",
                        "私、今日調子いいかも。ちゃんと手を動かせたから、かな。",
                        "やる気爆発！…は言いすぎ。でも『よし』って言える瞬間、いくつかあった。",
                        "悪くないよ。小さな『できた』が一個あるだけで、空気が軽くなる。"
                    };
                    zh = new[]
                    {
                        "感觉今天进展不错……虽然没到那份上，但确实有往前走一点。",
                        "我今天的状态好像不错……大概是因为手一直没停下来吧。",
                        "要说干劲爆发……那有点夸张。不过有几个瞬间能跟自己说『行』。",
                        "还不错啦。只要有一个小小的『做到了』，心情都会轻松一点。"
                    };
                    return;

                case MoodKind.Tired:
                    ja = new[]
                    {
                        "ずっと作業してると肩凝るよね…私も、今日はそんな感じだった。",
                        "眠たくなってきた？…私も、目が重い時間帯はあった。",
                        "疲れてるのは認める。でも作業の疲れだから、嫌じゃない。",
                        "ふわぁ…ってなる日もあるでしょ。それでも手、止めずにいたつもり。"
                    };
                    zh = new[]
                    {
                        "一直创作的话，肩膀会变得好僵硬呢……我今天也是这样。",
                        "开始犯困了吗？……我也有眼皮发沉的时候。",
                        "累是累……不过是创作的累，我不讨厌。",
                        "偶尔也会走神的吧……但我还是尽量没让手停下来。"
                    };
                    return;

                case MoodKind.WorkFocus:
                    ja = new[]
                    {
                        "作業中だから、反応薄かったらごめん。私も、今はここに集中してる。",
                        "私も頑張るから、君も頑張ろ？…今日は、そういう一日だった。",
                        "今は目の前の作業に集中しよ。完璧じゃないけど、離れないでいた。",
                        "休憩したい気持ちはある。それでも、今は踏ん張れた。"
                    };
                    zh = new[]
                    {
                        "正创作呢，要是刚才反应有点淡，别见怪。我现在也在专注眼前的事。",
                        "我会努力的，你也一起加油吧？……今天就是这样的一天。",
                        "现在先专注于眼前的创作吧。谈不上完美，但至少没跑开。",
                        "想休息的心情是有的……不过还是撑住了。"
                    };
                    return;

                case MoodKind.BreakRelaxed:
                    ja = new[]
                    {
                        "今は休憩タイム。休憩も大切だからね。",
                        "今なら少し話せるよ。のんびりタイム〜って感じ。",
                        "深呼吸するのもいいかも。肩の力、抜いて。",
                        "無理せず自分のペースで、ね。今日はそう思えた。"
                    };
                    zh = new[]
                    {
                        "现在是休息时间。休息也是很重要的哦。",
                        "现在可以聊会儿哦。……就是悠闲时光～那种感觉。",
                        "要不试试深呼吸吧。把肩膀放松。",
                        "不要勉强，按照自己的节奏努力吧。……今天我是这么想的。"
                    };
                    return;

                case MoodKind.StudyFriction:
                    ja = new[]
                    {
                        "力学が頭の隅でこすれてる日…でも今日は、式が一個だけ通った。それで救われた。",
                        "集中できない時間もあった。それでもノートは開けた。",
                        "やること、まだある。焦るけど、手が完全に止まった日じゃなかった。",
                        "苦手意識はある。でも『ちょっとでも前に』は進めた気がする。"
                    };
                    zh = new[]
                    {
                        "力学还是在脑子里磨人……不过今天总算有一条式子对上了，算救了我。",
                        "也有完全集中不了的时候……可笔记本还是翻开了。",
                        "要做的事还有一堆。着急归着急，至少不是彻底停笔的一天。",
                        "知道自己在这块不算擅长……但多少还是往前挪了一点。"
                    };
                    return;

                case MoodKind.CreativeTinge:
                    ja = new[]
                    {
                        "小説、今日は触る時間短め。でも一行だけ、残したい言い回しが降ってきた。",
                        "インスピレーション爆発！…じゃない。ノートの端に短い言葉が増えただけ。",
                        "考えごとは多かった。独り言も。でもネタの匂いは悪くない。",
                        "創作のスイッチ、全開じゃない。オフでもない。いつもの中途半端。"
                    };
                    zh = new[]
                    {
                        "小说今天写得不久……不过有一句话，我是真想留下来。",
                        "灵感大爆发？……那倒没有。只是笔记本边上多了一行字。",
                        "想得多，自言自语也多……不过走向还不坏。",
                        "创作的劲头没有全开，也没关掉……老样子，半吊子。"
                    };
                    return;

                case MoodKind.AskedHappy:
                    ja = new[]
                    {
                        "嬉しいかって？…直球だね。うん、悪くないよ。",
                        "ハッピー全開！は嘘。でも『今日はこれでいい』って思えた。",
                        "すっごく上向き！ってほどじゃない。それでも、まあまあ。",
                        "落ち着いてるのが、今日の私かも。それも悪くないでしょ。"
                    };
                    zh = new[]
                    {
                        "开不开心？……问得好直接。嗯，还不错。",
                        "特别开心那种话是骗人……不过『今天就到这儿吧』，我能这么想。",
                        "要说情绪高涨，那不至于……但也还算说得过去。",
                        "也许今天是比较沉得住气的我……也不坏吧？"
                    };
                    return;

                default:
                    ja = new[]
                    {
                        "今日はね…派手な日じゃなかった。でもこの部屋の明かり、いつも通りで安心した。",
                        "平凡って言うと照れるけど…『悪くない』で済んだなら、それでいい。",
                        "誰かに褒められた日じゃない。それでも、ちょっとだけ自分を褒めた。",
                        "君が調子を聞いてくれたから、一日を振り返ったよ…まあまあ、って感じ。",
                        "嵐でも凪でもない、普通の一日。私、そういうの嫌いじゃない。",
                        "通話の温度も、いつもの感じ。それがちょうどよかった。"
                    };
                    zh = new[]
                    {
                        "今天呢……不是什么特别的日子。不过这屋子的光跟往常一样，挺让人安心的。",
                        "说平凡有点不好意思……能用『还不错』打发过去的话，就够了。",
                        "今天没人夸我……但我还是悄悄夸了自己一下。",
                        "你一问状态，我就回过头想了一下……也就那样吧。",
                        "不是大起大落的一天，就是平平常常的。我不讨厌这样。",
                        "跟你通电话的感觉，也跟平常差不多……刚刚好。"
                    };
                    return;
            }
        }
    }
}
