using System;
using System.Text;
using AIChat.Unity;

namespace AIChat.Services
{
    /// <summary>
    /// 把游戏侧"现在的真实状态"组装成日语 prompt section，
    /// 让聪音不再凭空捏造"最近在干什么"。
    /// 核心三要素：
    ///   1) 现实时刻段（早晨 / 中午 / 傍晚 / 深夜）
    ///   2) 当前活动（PC作业 / 看书 / 喝茶休憩 / 听音乐 等）
    ///   3) 番茄钟阶段（工作中 第N循环 余M分钟 / 休憩中 / 未启动）
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

            // 任何一个有效就值得注入
            if (string.IsNullOrEmpty(activity) && !pomo.valid && string.IsNullOrEmpty(timeOfDay))
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

            sb.AppendLine();
            return sb.ToString();
        }

        private static string GetTimeOfDayJa(int hour)
        {
            // ゲーム本体の TimeOfDayProvider と同じ閾値
            if (hour > 5 && hour < 11) return "朝";
            if (hour >= 11 && hour < 17) return "昼";
            if (hour >= 17 && hour < 20) return "夕方";
            return "夜";
        }
    }
}
