using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using AIChat.Utils;

namespace AIChat.Unity
{
    /// <summary>
    /// 把游戏 <c>Bulbul.PomodoroService</c> 的状态机用 Harmony postfix **被动**镜像到 mod 本地变量。
    /// 这样 mod 判断「番茄钟是否在跑」就**不再依赖反射主动 poll**（那条路径在 PomodoroService 注入时机晚的情况下永远拿不到）。
    /// 镜像点：StartPomodoro / PlayPomodoroTimer(Work|Break) / Pause / UnPause / OnTimerEnd / CompletePomodoroTimer / Setup。
    /// 这 7 个方法覆盖了游戏内 `_mainState` 和 `_currentPomodoroType` 的全部写入路径。
    /// </summary>
    public static class PomodoroStateMirror
    {
        private static bool _applied;

        /// <summary>对应 PomodoroService.PomodoroType。Work=0, Break=1, Complete=2。</summary>
        public enum PhaseMirror { Complete = 2, Work = 0, Break = 1, Unknown = 99 }

        /// <summary>对应 PomodoroService.MainState。Idle=0, Work=1, Rest=2, Pause=3, ...</summary>
        public enum MainStateMirror { Idle = 0, Work = 1, Rest = 2, Pause = 3, TalkStartReady = 4, Talking = 5, TalkEnd = 6, Unknown = 99 }

        public static PhaseMirror CurrentPhase { get; private set; } = PhaseMirror.Complete;
        public static MainStateMirror CurrentMainState { get; private set; } = MainStateMirror.Idle;

        /// <summary>
        /// 番茄钟「会话进行中」（Work/Rest/Pause 任一）。这个是 mod 让位 LLM 的最终判断。
        /// 与游戏 <c>PomodoroService.IsTimerRunning()</c> 完全等价。
        /// </summary>
        public static bool IsActive
        {
            get
            {
                return CurrentMainState == MainStateMirror.Work
                    || CurrentMainState == MainStateMirror.Rest
                    || CurrentMainState == MainStateMirror.Pause;
            }
        }

        /// <summary>是否当前在工作阶段（Work）。供 prompt 用。</summary>
        public static bool IsWorking => CurrentPhase == PhaseMirror.Work;

        /// <summary>是否当前在休憩阶段（Break）。</summary>
        public static bool IsResting => CurrentPhase == PhaseMirror.Break;

        /// <summary>Harmony 是否成功挂载。挂不上时回退反射 poll。</summary>
        public static bool HookApplied => _applied;

        public static void TryApply(ManualLogSource log)
        {
            if (_applied) return;
            try
            {
                Type pomoType = Type.GetType("Bulbul.PomodoroService, Assembly-CSharp");
                if (pomoType == null)
                {
                    log.LogWarning("[PomoMirror] 未找到 Bulbul.PomodoroService，跳过。");
                    return;
                }

                var harmony = new Harmony("com.username.chillaimod.pomodoro.statemirror");

                // (1) Setup() — 游戏初始化重置：_mainState=Idle, _currentPomodoroType=Complete
                Patch(harmony, pomoType, "Setup", BindingFlags.Public | BindingFlags.Instance,
                    nameof(SetupPostfix), log);

                // (2) StartPomodoro() — 开始番茄钟会话
                Patch(harmony, pomoType, "StartPomodoro", BindingFlags.Public | BindingFlags.Instance,
                    nameof(StartPomodoroPostfix), log);

                // (3) PlayPomodoroTimer(PomodoroType) — 进入 Work 或 Break 阶段（private）
                Patch(harmony, pomoType, "PlayPomodoroTimer", BindingFlags.NonPublic | BindingFlags.Instance,
                    nameof(PlayPomodoroTimerPostfix), log);

                // (4) Pause(bool) — 进入 Pause 状态（private）
                Patch(harmony, pomoType, "Pause", BindingFlags.NonPublic | BindingFlags.Instance,
                    nameof(PausePostfix), log);

                // (5) UnPause(bool) — 退出 Pause（private）
                Patch(harmony, pomoType, "UnPause", BindingFlags.NonPublic | BindingFlags.Instance,
                    nameof(UnPausePostfix), log);

                // (6) OnTimerEnd() — 单次计时结束（Work→Break 或 Break→Work）
                Patch(harmony, pomoType, "OnTimerEnd", BindingFlags.Public | BindingFlags.Instance,
                    nameof(OnTimerEndPostfix), log);

                // (7) CompletePomodoroTimer() — 整个番茄钟会话完成（任务）
                Patch(harmony, pomoType, "CompletePomodoroTimer", BindingFlags.Public | BindingFlags.Instance,
                    nameof(CompletePomodoroTimerPostfix), log);

                _applied = true;
                log.LogInfo("[PomoMirror] Harmony 已挂载：番茄钟状态由游戏调用驱动镜像（不再依赖主动反射）");
            }
            catch (Exception ex)
            {
                log.LogWarning($"[PomoMirror] 挂载失败: {ex.Message}");
            }
        }

        private static void Patch(Harmony harmony, Type t, string methodName, BindingFlags flags, string postfixName, ManualLogSource log)
        {
            MethodInfo m = t.GetMethod(methodName, flags);
            if (m == null)
            {
                log.LogWarning($"[PomoMirror] 未找到 PomodoroService.{methodName}，跳过该钩点。");
                return;
            }
            harmony.Patch(m, postfix: new HarmonyMethod(typeof(PomodoroStateMirror), postfixName));
        }

        // ============== Harmony postfixes ==============
        // 这些 postfix 在游戏方法 return 之后立即被 Harmony 调用。
        // 我们读 __instance 上的 private 字段镜像出来。

        private static FieldInfo _mainStateField;
        private static FieldInfo _currentTypeField;

        private static void RefreshFromInstance(object instance)
        {
            try
            {
                Type t = instance.GetType();
                if (_mainStateField == null) _mainStateField = t.GetField("_mainState", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_currentTypeField == null) _currentTypeField = t.GetField("_currentPomodoroType", BindingFlags.NonPublic | BindingFlags.Instance);

                if (_mainStateField != null)
                {
                    object v = _mainStateField.GetValue(instance);
                    if (v != null)
                    {
                        int ord = Convert.ToInt32(v);
                        CurrentMainState = ord >= 0 && ord <= 6 ? (MainStateMirror)ord : MainStateMirror.Unknown;
                    }
                }
                if (_currentTypeField != null)
                {
                    object v = _currentTypeField.GetValue(instance);
                    if (v != null)
                    {
                        int ord = Convert.ToInt32(v);
                        CurrentPhase = ord >= 0 && ord <= 2 ? (PhaseMirror)ord : PhaseMirror.Unknown;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[PomoMirror] 读 PomodoroService 字段失败: {ex.Message}");
            }
        }

        private static void LogChange(string trigger)
        {
            Log.Info($"[PomoMirror] {trigger} → mainState={CurrentMainState} phase={CurrentPhase} active={IsActive}");
        }

        public static void SetupPostfix(object __instance) { RefreshFromInstance(__instance); LogChange("Setup"); }
        public static void StartPomodoroPostfix(object __instance) { RefreshFromInstance(__instance); LogChange("StartPomodoro"); }
        public static void PlayPomodoroTimerPostfix(object __instance) { RefreshFromInstance(__instance); LogChange("PlayPomodoroTimer"); }
        public static void PausePostfix(object __instance) { RefreshFromInstance(__instance); LogChange("Pause"); }
        public static void UnPausePostfix(object __instance) { RefreshFromInstance(__instance); LogChange("UnPause"); }
        public static void OnTimerEndPostfix(object __instance) { RefreshFromInstance(__instance); LogChange("OnTimerEnd"); }
        public static void CompletePomodoroTimerPostfix(object __instance) { RefreshFromInstance(__instance); LogChange("CompletePomodoroTimer"); }
    }
}
