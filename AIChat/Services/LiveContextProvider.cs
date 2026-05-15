using System;
using System.Text;
using AIChat.Unity;

namespace AIChat.Services
{
    /// <summary>
    /// 把游戏侧"现在的真实状态"组装成日语 prompt section，
    /// 让聪音不再凭空捏造"最近在干什么"。
    /// 注入する情報：
    ///   1) 現実時刻と時間帯（朝 / 昼 / 夕方 / 夜）
    ///   2) 当前活動（PC作業 / 看書 / 喝茶休憩 / 听音乐 等）
    ///   3) ポモドーロ阶段（工作中 第N循環 余M分鐘 / 休憩中 / 未启动）
    ///   4) 通話開始前の今日の出来事メモ（DailyStoryGenerator が起動時に生成した素材）
    /// </summary>
    public static class LiveContextProvider
    {
        /// <summary>
        /// 返回完整可注入的日语段落（已带换行）；
        /// 如果游戏侧未连接或所有信息都为 null，则返回空字符串（不污染 prompt）。
        /// </summary>
        public static string BuildContextSnippet()
        {
            var sb = new StringBuilder();

            string timeOfDay = GetTimeOfDayJa(DateTime.Now.Hour);
            string clock = DateTime.Now.ToString("HH:mm");
            string activity = GameBridge.GetCurrentActivityJa();
            var pomo = GameBridge.GetPomodoroSnapshot();
            string dailyStory = DailyStoryGenerator.BuildSnippetForPrompt();

            // すべて空なら何も注入しない
            if (string.IsNullOrEmpty(activity) && !pomo.valid && string.IsNullOrEmpty(timeOfDay) && string.IsNullOrEmpty(dailyStory))
            {
                return string.Empty;
            }

            sb.AppendLine("=== 今この瞬間のあなたの状態（リアルタイム情報、必ず参照する） ===");
            sb.AppendLine("以下はあなた自身の今の状況。ユーザーが何をしているかではなく、あなたが何をしているか。");
            sb.AppendLine("「最近〇〇してた」「ちょうど〇〇してたんだよね」など、自然に話題に織り込んでいい。");
            sb.AppendLine("ただし、毎回全部を口に出す必要はない。話題と関係ある時だけさらっと触れる。");

            if (!string.IsNullOrEmpty(timeOfDay))
            {
                sb.AppendLine($"- 現在の時間帯：{timeOfDay}（{clock}頃）");
            }
            if (!string.IsNullOrEmpty(activity))
            {
                sb.AppendLine($"- 今やっていること：{activity}");
            }
            if (pomo.valid)
            {
                if (pomo.phase == "Work")
                {
                    sb.AppendLine($"- ポモドーロ：作業中（第{pomo.loop}サイクル、あと約{pomo.remainMinutes}分）");
                    sb.AppendLine("  ⚠ 作業中なので、君は集中したい。返答は1文以内・短く・優しく作業に戻るよう促す。雑談は休憩までお預け。");
                }
                else if (pomo.phase == "Break")
                {
                    sb.AppendLine($"- ポモドーロ：休憩中（第{pomo.loop}サイクル、あと約{pomo.remainMinutes}分）");
                    sb.AppendLine("  → 休憩中なので、雑談したり、お茶を勧めたり、ストレッチを提案したりする時間。");
                }
            }
            else if (GameBridge.IsPomodoroTimerRunning())
            {
                sb.AppendLine("- ポモドーロ：タイマー稼働中（詳細は未取得だが、作業/休憩セッション中）");
            }
            else
            {
                sb.AppendLine("- ポモドーロ：今は走らせていない");
            }

            // 通話開始前に LLM が事前生成した「今日の小さな出来事」を素材として注入する。
            // これは話題の押し付けではなく、聞かれた時 / 自然な流れの時に "思い出して語る" ための素材庫。
            if (!string.IsNullOrEmpty(dailyStory))
            {
                sb.Append(dailyStory);
            }

            sb.AppendLine();
            sb.AppendLine("【今日の出来事メモの使い方（厳守）】");
            sb.AppendLine("- 上記「今日の出来事メモ」は **素材庫**であって、話題リストではない。話の流れと関係なければ触れない。");
            sb.AppendLine("- 「今日何があった？」「最近どう？」「何か面白いことあった？」と直接聞かれた時、または会話が自然に");
            sb.AppendLine("  自分の生活共有に流れ着いた時にだけ、メモから 1 件を選んで思い出として自然に語る。");
            sb.AppendLine("- 1 ターンに使うのは **最大 1 件**。2 件まとめて披露しない。列挙しない。");
            sb.AppendLine("- 見出し（「通話開始前の今日」「今日の出来事メモ」など）や中点「· 」をそのまま読み上げない。**復唱厳禁**。");
            sb.AppendLine("- メモが空 / 関連が薄い場合は、事実を作らず「うーん…今日は大きな出来事はなかったかな。普通に作業してた感じ」と");
            sb.AppendLine("  素直に答える。**無理に** 聞き返しや話を振りに行かなくてよい（会話を続けたい流れなら軽く一句でよい）。");
            sb.AppendLine("- 「ちょっと話せないな」「思い出せない」「秘密」のような はぐらかし／拒絶は禁止（聞いてない感が出る）。");
            sb.AppendLine("- メモが空の時に「気になることがあった」「ちょっと恥ずかしい」「言えないけど」のような含みを持たせるのは禁止。");
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GetTimeOfDayJa(int hour)
        {
            if (hour > 5 && hour < 11) return "朝";
            if (hour >= 11 && hour < 17) return "昼";
            if (hour >= 17 && hour < 20) return "夕方";
            return "夜";
        }
    }
}
