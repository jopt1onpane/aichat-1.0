using System;
using System.Text.RegularExpressions;

namespace AIChat.Services
{
    /// <summary>
    /// プレイヤーが **自分** の空虚・寂しさ・虚無感などを打ち明けたときの手書き返答。
    /// （<see cref="SatoneMoodScripts"/> は「聡音の調子を聞く」専用で、空虚語は意図的に含めていない）
    /// LLM 逸脱（複数ドラフト混入・DrinkTea 誘導）を避け、**会話の継続・寄り添い**を優先する。
    /// 中文は heroine_all_dialogues の ZhHans 調（创作・通話・大丈夫系）に寄せる。
    /// </summary>
    public static class PlayerDisclosureComfortScripts
    {
        private static readonly object _rngLock = new object();
        private static Random _rng = new Random(Environment.TickCount ^ Guid.NewGuid().GetHashCode());

        /// <summary>ユーザーが自身の気持ち（空/虚/寂/无聊等）を述べているか。聡音へ「你空虚吗」だけの聞き方は除外しにくいが、主に「我/心里」を拾う。</summary>
        public static bool IsPlayerLowMoodSelfDisclosure(string prompt)
        {
            string p = prompt ?? "";
            if (p.Trim().Length == 0) return false;

            if (Regex.IsMatch(p, @"空虚|心が空|心裡空|心里空|空荡荡|空落落|空虛|虛無|虚无|むなしい|虚しい"))
            {
                if (!HasPlayerSelfAnchor(p)) return false;
                return true;
            }

            if (Regex.IsMatch(p, @"寂寞|寂し|寂しい|好寂寞|好寂莫|孤独|ぼっち"))
            {
                if (!HasPlayerSelfAnchor(p)) return false;
                return true;
            }

            if (Regex.IsMatch(p, @"(好无|好無|很无|很無|特别无|特別無)聊|无聊死了|好无聊|無聊死了|つまらない|退屈"))
            {
                if (!HasPlayerSelfAnchor(p)) return false;
                return true;
            }

            if (Regex.IsMatch(p, @"心里.*(空|堵|慌|难受|難受|发慌|發慌)|(空|堵)得慌|好空啊|覺得好空|觉得好空"))
                return true;

            return false;
        }

        private static bool HasPlayerSelfAnchor(string p)
        {
            if (Regex.IsMatch(p, @"我|俺|咱|自己|人家|心里|心裡|胸口|覺得|觉得|感覺|感觉|有些|有点|特別|特别"))
                return true;
            if (Regex.IsMatch(p, @"(好|很|挺|特别|有点|有些)\s*(空虚|空空的|寂寞|无聊|無聊)"))
                return true;
            return false;
        }

        /// <summary>共感＋雑談への誘い。action は AIMod 側で Agree 固定推奨（首肯・聞く姿勢）。</summary>
        public static bool TryPickLine(out string voiceJa, out string subtitleZh)
        {
            voiceJa = "";
            subtitleZh = "";
            string[] ja =
            {
                "うん…そっか。じゃあ、少し話そ。こっち、ちゃんと聞いてるから。",
                "大丈夫。今は無理に明るくしなくていい。このまま、通話続けよ。",
                "空っぽに感じる時もあるよ。…だからこそ、話してくれてありがと。続き、聞かせて。",
                "一人で抱え込まなくていい。ここにいるよ。ゆっくりでいいから、言葉にしてみて。"
            };
            string[] zh =
            {
                "嗯……那就聊聊天吧。我在这儿听着呢。",
                "没事的，不用硬撑着装开心。就这样通着话也好。",
                "心里发空的时候谁都会有……你愿意说出来，我已经很高兴了。还想说什么都可以哦。",
                "别一个人憋着。我在呢。慢慢说就好，想说什么都行。"
            };

            int i;
            lock (_rngLock) { i = _rng.Next(ja.Length); }
            voiceJa = ja[i];
            subtitleZh = zh[i];
            return true;
        }
    }
}
