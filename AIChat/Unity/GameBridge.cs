using AIChat.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;

namespace AIChat.Unity
{
    public static class GameBridge
    {
        public static MonoBehaviour _heroineService;
        public static Animator _cachedAnimator;

        // 全部反射缓存就绪时为 true（_heroineService、_heroineAI、_pomodoroService、_facilityClickHeroine、_roomGameManager 都拿到）
        public static bool IsCacheComplete()
        {
            return _heroineService != null
                && _heroineAI != null
                && _pomodoroService != null
                && _facilityClickHeroine != null
                && _roomGameManager != null
                && _pomodoroIsTimerRunningMethod != null;
        }

        public static MethodInfo _changeAnimSmoothMethod;
        public static MethodInfo _lookInitMethod;
        public static MethodInfo _lookAtMethod;
        private static MethodInfo _cancelVoiceMethod;

        private static MonoBehaviour _heroineAI;
        private static FieldInfo _actionStateMachineField;
        private static PropertyInfo _currentStateProperty;
        private static object _facilityVoiceTextScenario;
        private static MethodInfo _cancelVoiceTextScenarioMethod;
        private static int _lastCancelVoiceFrame;
        private static int _lastVoiceScenarioSearchFrame;
        private static int _lastCancelNativeVoiceAudioFrame;

        // 番茄钟服务（实时上下文注入用）
        private static object _pomodoroService;
        private static PropertyInfo _currentPomodoroTypeProp;
        private static PropertyInfo _currentLoopCountProp;
        private static FieldInfo _pomodoroTimerServiceField;
        private static MethodInfo _remainTimeSpanMethod;
        private static MethodInfo _pomodoroIsTimerRunningMethod;

        // FacilityClickHeroine + RoomGameManager（番茄钟中走与点击女主相同的原生反应）
        private static object _facilityClickHeroine;
        private static MethodInfo _reactionReadyMethod;
        private static object _reactionTypeClick; // ReactionType.Click 的枚举值
        private static FieldInfo _clickHeroineMainStateField; // MainState: Idle=0, ReadyReaction, PlayingReaction, EndPlayReaction
        private static MonoBehaviour _roomGameManager;
        private static MethodInfo _playHeroineTouchReactionMethod;

        private static readonly HashSet<string> _unsafeStates = new HashSet<string>
        {
            "ScenarioState", "SleepState", "StandUpState", "SitDownState", "WalkState"
        };

        /// <summary>
        /// 扫描场景，把还没缓存的反射对象一次性补齐。即使 HeroineService 已经在了，也会继续找 HeroineAI / PomodoroService / FacilityClickHeroine。
        /// </summary>
        public static void FindHeroineService()
        {
            var allComponents = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var comp in allComponents)
            {
                string typeName = comp.GetType().FullName;
                if (typeName == "Bulbul.HeroineService" && _heroineService == null)
                {
                    _heroineService = comp;
                    _cachedAnimator = comp.GetComponent<Animator>();

                    _changeAnimSmoothMethod = comp.GetType().GetMethod("ChangeHeroineAnimationForInteger", BindingFlags.Public | BindingFlags.Instance);
                    _lookInitMethod = comp.GetType().GetMethod("LookInitSlowly", BindingFlags.Public | BindingFlags.Instance);
                    _lookAtMethod = comp.GetType().GetMethod("ChangeLookScaleAnimation", BindingFlags.Public | BindingFlags.Instance);
                    _cancelVoiceMethod = comp.GetType().GetMethod("CancelVoice", BindingFlags.Public | BindingFlags.Instance);

                    if (_changeAnimSmoothMethod != null) Log.Warning($"✅ 核心连接成功: {comp.gameObject.name}");
                }
                else if (typeName == "Bulbul.HeroineAI" && _heroineAI == null)
                {
                    _heroineAI = comp;
                    _actionStateMachineField = comp.GetType().GetField("_actionStateMachine", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (_actionStateMachineField != null)
                    {
                        Log.Info("[GameBridge] HeroineAI 状态机字段已缓存");
                    }
                }
                else if (typeName == "Bulbul.RoomGameManager")
                {
                    if (_facilityVoiceTextScenario == null) CacheFacilityVoiceTextScenario(comp);
                    if (_facilityClickHeroine == null) CacheFacilityClickHeroine(comp);
                    if (_roomGameManager == null) CacheRoomGameManagerForTouchReaction(comp);
                }
                else if (typeName == "Bulbul.FacilityClickHeroine" && _facilityClickHeroine == null)
                {
                    CacheFacilityClickHeroineFromInstance(comp);
                }
            }

            // PomodoroService 通常在 HeroineAI 之后由 VContainer 注入。每次扫描都尝试一次，直到拿到为止。
            if (_heroineAI != null && _pomodoroService == null)
            {
                CachePomodoroServiceFromHeroineAI(_heroineAI);
            }
        }

        // 缓存 FacilityClickHeroine + ReactionReady(ReactionType.Click) 反射入口
        // 让 mod 在番茄钟工作中能像"用户点了女主"一样触发原生反应
        private static void CacheFacilityClickHeroine(MonoBehaviour roomGameManager)
        {
            try
            {
                FieldInfo field = roomGameManager.GetType().GetField("_facilityClickHeroine", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null)
                {
                    foreach (var f in roomGameManager.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (f.FieldType.Name == "FacilityClickHeroine") { field = f; break; }
                    }
                }
                if (field == null) { Log.Warning("[GameBridge] RoomGameManager 上没找到 _facilityClickHeroine 字段"); return; }

                object inst = field.GetValue(roomGameManager);
                if (inst == null) { Log.Warning("[GameBridge] _facilityClickHeroine 字段为 null"); return; }

                CacheFacilityClickHeroineFromInstance(inst);
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] 缓存 FacilityClickHeroine 失败: {ex.Message}");
            }
        }

        private static void CacheFacilityClickHeroineFromInstance(object inst)
        {
            try
            {
                _facilityClickHeroine = inst;
                Type t = inst.GetType();

                _reactionReadyMethod = t.GetMethod("ReactionReady", BindingFlags.Public | BindingFlags.Instance);
                if (_reactionReadyMethod == null) { Log.Warning("[GameBridge] FacilityClickHeroine.ReactionReady 方法未找到"); return; }

                Type reactionTypeEnum = t.GetNestedType("ReactionType", BindingFlags.Public);
                if (reactionTypeEnum != null && reactionTypeEnum.IsEnum)
                {
                    _reactionTypeClick = Enum.Parse(reactionTypeEnum, "Click");
                }
                else
                {
                    var p = _reactionReadyMethod.GetParameters();
                    if (p.Length == 1 && p[0].ParameterType.IsEnum)
                    {
                        _reactionTypeClick = Enum.Parse(p[0].ParameterType, "Click");
                    }
                }
                if (_reactionTypeClick == null) { Log.Warning("[GameBridge] ReactionType.Click 枚举值未拿到"); return; }

                _clickHeroineMainStateField = t.GetField("_mainState", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_clickHeroineMainStateField == null)
                    Log.Warning("[GameBridge] FacilityClickHeroine._mainState 字段未找到（无法用 busy 门控 mod）");

                Log.Info("[GameBridge] FacilityClickHeroine 已缓存（番茄钟工作中走原生反应）");
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] CacheFacilityClickHeroineFromInstance 失败: {ex.Message}");
            }
        }

        private static void CacheRoomGameManagerForTouchReaction(MonoBehaviour roomGameManager)
        {
            try
            {
                _roomGameManager = roomGameManager;
                _playHeroineTouchReactionMethod = roomGameManager.GetType().GetMethod(
                    "PlayHeroineTouchReaction",
                    BindingFlags.Public | BindingFlags.Instance);
                if (_playHeroineTouchReactionMethod == null)
                    Log.Warning("[GameBridge] RoomGameManager.PlayHeroineTouchReaction 未找到");
                else
                    Log.Info("[GameBridge] RoomGameManager.PlayHeroineTouchReaction 已缓存");
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] 缓存 RoomGameManager 触摸反应入口失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 与游戏内「点击女主 → Idle 里 PlayHeroineTouchReaction」相同：先 ReactionReady(Click)，再立刻 PlayHeroineTouchReaction，
        /// 这样才会根据 PomodoroTalkController.IsCurrentWorking 选中 HeroineClickWork 等专注短句。
        /// </summary>
        /// <summary>
        /// 原生「点击女主」短反应是否占线（含独白 HeroineSelf 的 ReactionReady 流程）。
        /// _mainState != Idle 时不应再启 mod 全流程，避免双层字幕与动画抢状态。
        /// </summary>
        public static bool IsNativeClickHeroineBusy()
        {
            try
            {
                if (_facilityClickHeroine == null || _clickHeroineMainStateField == null) return false;
                object st = _clickHeroineMainStateField.GetValue(_facilityClickHeroine);
                if (st == null) return false;
                int ord = Convert.ToInt32(st);
                return ord != 0; // 非 Idle
            }
            catch { return false; }
        }

        public static bool TriggerNativeFocusTouchReaction()
        {
            if (_facilityClickHeroine == null || _reactionReadyMethod == null || _reactionTypeClick == null)
            {
                Log.Warning("[GameBridge] TriggerNativeFocusTouchReaction: FacilityClickHeroine 未就绪");
                return false;
            }
            try
            {
                object ret = _reactionReadyMethod.Invoke(_facilityClickHeroine, new object[] { _reactionTypeClick });
                bool readyOk = ret is bool b && b;
                Log.Info($"[GameBridge] ReactionReady(Click) = {readyOk}");
                if (!readyOk)
                    return false;

                if (_roomGameManager != null && _playHeroineTouchReactionMethod != null)
                {
                    _playHeroineTouchReactionMethod.Invoke(_roomGameManager, null);
                    Log.Info("[GameBridge] PlayHeroineTouchReaction() 已调用（原生专注/休憩点击语音+动作）");
                }
                else
                {
                    Log.Warning("[GameBridge] RoomGameManager 未缓存，已仅执行 ReactionReady；下一帧应由游戏 Idle 接力");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] TriggerNativeFocusTouchReaction 异常: {ex.Message}");
                return false;
            }
        }

        // PomodoroService 不是 MonoBehaviour，无法直接 FindObjectsOfType。
        // 它被 VContainer 注入到 HeroineAI 的 _pomodoroService 字段（普通 class）。
        private static void CachePomodoroServiceFromHeroineAI(MonoBehaviour heroineAI)
        {
            try
            {
                FieldInfo field = heroineAI.GetType().GetField("_pomodoroService", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null)
                {
                    foreach (var f in heroineAI.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (f.FieldType.Name == "PomodoroService") { field = f; break; }
                    }
                }
                if (field == null)
                {
                    Log.Warning("[GameBridge] HeroineAI 上没有找到 PomodoroService 字段");
                    return;
                }

                _pomodoroService = field.GetValue(heroineAI);
                if (_pomodoroService == null)
                {
                    Log.Warning("[GameBridge] PomodoroService 字段为 null（可能初始化未完成）");
                    return;
                }

                Type pType = _pomodoroService.GetType();
                _currentPomodoroTypeProp = pType.GetProperty("CurrentPomodoroType", BindingFlags.Public | BindingFlags.Instance);
                _currentLoopCountProp = pType.GetProperty("CurrentLoopCount", BindingFlags.Public | BindingFlags.Instance);
                _pomodoroIsTimerRunningMethod = pType.GetMethod("IsTimerRunning", BindingFlags.Public | BindingFlags.Instance);
                _pomodoroTimerServiceField = pType.GetField("_pomodoroTimerService", BindingFlags.NonPublic | BindingFlags.Instance);
                if (_pomodoroTimerServiceField != null)
                {
                    object timerSvc = _pomodoroTimerServiceField.GetValue(_pomodoroService);
                    if (timerSvc != null)
                    {
                        _remainTimeSpanMethod = timerSvc.GetType().GetMethod("RemainTimeSpan", BindingFlags.Public | BindingFlags.Instance);
                    }
                }

                Log.Info("[GameBridge] PomodoroService 已缓存（用于实时上下文注入）");
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] 缓存 PomodoroService 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 当前角色活动的简短描述（日语），用于 prompt 注入。
        /// 返回 null 表示未知/未连接。
        /// </summary>
        public static string GetCurrentActivityJa()
        {
            if (_heroineAI == null || _actionStateMachineField == null) return null;
            try
            {
                object stateMachine = _actionStateMachineField.GetValue(_heroineAI);
                if (stateMachine == null) return null;

                PropertyInfo keyProp = stateMachine.GetType().GetProperty("CurrentStateKey", BindingFlags.Public | BindingFlags.Instance);
                if (keyProp == null) return null;
                object skObj = keyProp.GetValue(stateMachine);
                int stateKey = skObj is int ski ? ski : Convert.ToInt32(skObj);

                switch (stateKey)
                {
                    case 19: return "PCで作業中";
                    case 20: return "本を読んで作業中";
                    case 21: return "レポートを書いている";
                    case 5:  return "椅子に前かがみで小休憩";
                    case 6:  return "お茶を飲んで休憩中";
                    case 7:  return "本を読んで休憩中";
                    case 8:  return "映画/動画を見て休憩中";
                    case 11: return "音楽を聴いて休憩中";
                    case 16: return "うとうと寝ている";
                    default: return null;
                }
            }
            catch { return null; }
        }

        /// <summary>
        /// 番茄钟当前快照：是否在工作/休憩中、第几次、剩余分钟。
        /// </summary>
        public static (bool valid, string phase, int loop, int remainMinutes) GetPomodoroSnapshot()
        {
            try
            {
                if (_pomodoroService == null || _currentPomodoroTypeProp == null) return (false, null, 0, 0);

                object typeObj = _currentPomodoroTypeProp.GetValue(_pomodoroService);
                if (typeObj == null) return (false, null, 0, 0);
                // CurrentPomodoroType 为枚举装箱时不能 (int) 直接拆箱，会抛 InvalidCastException
                int typeVal = typeObj is int ti ? ti : Convert.ToInt32(typeObj); // Work=0, Break=1, Complete=2
                string phase;
                switch (typeVal)
                {
                    case 0: phase = "Work"; break;
                    case 1: phase = "Break"; break;
                    default: return (false, null, 0, 0); // Complete 视为未运行
                }

                int loop = 0;
                if (_currentLoopCountProp != null)
                {
                    object loopObj = _currentLoopCountProp.GetValue(_pomodoroService);
                    if (loopObj != null)
                    {
                        // ReadOnlyReactiveProperty<int>，读 .CurrentValue
                        var cur = loopObj.GetType().GetProperty("CurrentValue") ?? loopObj.GetType().GetProperty("Value");
                        if (cur != null)
                        {
                            object lv = cur.GetValue(loopObj);
                            if (lv is int lvi) loop = lvi;
                        }
                    }
                }

                int remainMin = 0;
                if (_pomodoroTimerServiceField != null && _remainTimeSpanMethod != null)
                {
                    object timerSvc = _pomodoroTimerServiceField.GetValue(_pomodoroService);
                    if (timerSvc != null)
                    {
                        object ts = _remainTimeSpanMethod.Invoke(timerSvc, null);
                        if (ts is TimeSpan tsv)
                        {
                            remainMin = (int)Math.Ceiling(tsv.TotalMinutes);
                        }
                    }
                }
                return (true, phase, loop, remainMin);
            }
            catch
            {
                return (false, null, 0, 0);
            }
        }

        /// <summary>
        /// 番茄钟当前是否处于"工作中"。这种状态下 mod 应让出对话给原生劝学逻辑。
        /// </summary>
        public static bool IsPomodoroWorking()
        {
            var snap = GetPomodoroSnapshot();
            return snap.valid && snap.phase == "Work";
        }

        /// <summary>
        /// 与游戏 PomodoroService.IsTimerRunning 一致：Work / Rest / Pause 等会话进行中为 true。
        /// mod 在番茄钟会话未结束时不要走 LLM，应转交原生点击反应。
        /// </summary>
        public static bool IsPomodoroTimerRunning()
        {
            // 关键路径：缓存还没就绪时强制重扫一次，避免「番茄钟运行中 mod 仍接管」的并行 bug
            if (_pomodoroService == null || _pomodoroIsTimerRunningMethod == null)
            {
                EnsureCachesReady("IsPomodoroTimerRunning");
            }

            try
            {
                if (_pomodoroService != null && _pomodoroIsTimerRunningMethod != null)
                {
                    object r = _pomodoroIsTimerRunningMethod.Invoke(_pomodoroService, null);
                    return r is bool b && b;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] IsTimerRunning 反射调用失败: {ex.Message}");
            }
            // 反射仍不可用时回退到 CurrentPomodoroType
            var snap = GetPomodoroSnapshot();
            return snap.valid;
        }

        private static int _lastEnsureCachesFrame = -10000;
        /// <summary>关键路径触发：缓存未齐全时强制重扫，最快每 30 帧一次。</summary>
        public static void EnsureCachesReady(string reason)
        {
            if (IsCacheComplete()) return;
            if (Time.frameCount - _lastEnsureCachesFrame < 30) return;
            _lastEnsureCachesFrame = Time.frameCount;
            Log.Info($"[GameBridge] EnsureCachesReady({reason}): heroineService={_heroineService != null}, heroineAI={_heroineAI != null}, pomodoroService={_pomodoroService != null}, facilityClickHeroine={_facilityClickHeroine != null}, roomGameManager={_roomGameManager != null}");
            FindHeroineService();
            Log.Info($"[GameBridge] EnsureCachesReady({reason}) 完成: complete={IsCacheComplete()}, pomoTimerMethod={_pomodoroIsTimerRunningMethod != null}");
        }

        private static void CacheFacilityVoiceTextScenario(MonoBehaviour roomGameManager)
        {
            try
            {
                FieldInfo field = roomGameManager.GetType().GetField("_facilityVoiceTextScenario", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null) return;

                _facilityVoiceTextScenario = field.GetValue(roomGameManager);
                if (_facilityVoiceTextScenario == null) return;

                _cancelVoiceTextScenarioMethod = _facilityVoiceTextScenario.GetType().GetMethod("CancelReaction", BindingFlags.Public | BindingFlags.Instance);
                if (_cancelVoiceTextScenarioMethod != null)
                {
                    Log.Info("[GameBridge] FacilityVoiceTextScenario 已缓存，可在 AI 说话时抑制原生语音");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] 缓存 FacilityVoiceTextScenario 失败: {ex.Message}");
            }
        }

        public static void CallNativeChangeAnim(int id)
        {
            try { _changeAnimSmoothMethod.Invoke(_heroineService, new object[] { id }); }
            catch (Exception ex) { Log.Error($"Anim Error: {ex.Message}"); }
        }

        public static void ControlLookAt(float scale, float speed)
        {
            try { _lookAtMethod.Invoke(_heroineService, new object[] { scale, speed, 0 }); }
            catch { }
        }

        public static void RestoreLookAt()
        {
            if (_lookInitMethod != null) try { _lookInitMethod.Invoke(_heroineService, null); } catch { }
        }

        public static bool IsHeroineStateSafe()
        {
            if (_heroineAI == null || _actionStateMachineField == null) return true;

            try
            {
                object stateMachine = _actionStateMachineField.GetValue(_heroineAI);
                if (stateMachine == null) return true;

                if (_currentStateProperty == null)
                {
                    _currentStateProperty = stateMachine.GetType().GetProperty("CurrentState", BindingFlags.Public | BindingFlags.Instance);
                }

                if (_currentStateProperty == null) return true;

                object currentState = _currentStateProperty.GetValue(stateMachine);
                if (currentState == null) return true;

                string stateName = currentState.GetType().Name;
                bool safe = !_unsafeStates.Contains(stateName);
                if (!safe) Log.Info($"[GameBridge] 状态不安全，跳过动画: {stateName}");
                return safe;
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] 读取状态机失败: {ex.Message}");
                return true;
            }
        }

        public static void CancelNativeVoiceTextScenario()
        {
            if ((_facilityVoiceTextScenario == null || _cancelVoiceTextScenarioMethod == null) && Time.frameCount - _lastVoiceScenarioSearchFrame > 120)
            {
                _lastVoiceScenarioSearchFrame = Time.frameCount;
                FindHeroineService();
            }

            if (_facilityVoiceTextScenario == null || _cancelVoiceTextScenarioMethod == null) return;

            // 多个系统会通过 FacilityVoiceTextScenario 排队播放原生短语音。
            // AI 正在回复时持续清理这个排队状态，避免和 TTS 撞音。
            try
            {
                _cancelVoiceTextScenarioMethod.Invoke(_facilityVoiceTextScenario, null);
                _lastCancelVoiceFrame = Time.frameCount;
            }
            catch (Exception ex)
            {
                if (Time.frameCount - _lastCancelVoiceFrame > 300)
                {
                    Log.Warning($"[GameBridge] 取消原生语音场景失败: {ex.Message}");
                    _lastCancelVoiceFrame = Time.frameCount;
                }
            }
        }

        public static void CancelNativeVoiceAudio(bool log = false)
        {
            if (_heroineService == null || _cancelVoiceMethod == null) return;

            try
            {
                _cancelVoiceMethod.Invoke(_heroineService, null);
                if (log || Time.frameCount - _lastCancelNativeVoiceAudioFrame > 300)
                {
                    Log.Info("[GameBridge] 已请求停止原生角色语音");
                    _lastCancelNativeVoiceAudioFrame = Time.frameCount;
                }
            }
            catch (Exception ex)
            {
                if (Time.frameCount - _lastCancelNativeVoiceAudioFrame > 300)
                {
                    Log.Warning($"[GameBridge] 停止原生角色语音失败: {ex.Message}");
                    _lastCancelNativeVoiceAudioFrame = Time.frameCount;
                }
            }
        }

        /// <summary>
        /// 对话结束后彻底恢复游戏状态，确保原生点击交互不受影响。
        /// </summary>
        public static void SafeResetAfterMod()
        {
            try
            {
                if (_cachedAnimator != null)
                {
                    _cachedAnimator.SetBool("Enable_Talk", false);
                }

                if (_heroineService != null && _changeAnimSmoothMethod != null)
                {
                    RestoreLookAt();

                    int safeAnimId = GetCurrentSafeAnimId();
                    CallNativeChangeAnim(safeAnimId);
                    Log.Info($"[GameBridge] SafeReset: 恢复动画到 {safeAnimId}");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] SafeReset 出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据 HeroineAI 状态机的当前状态，返回对应的安全 Animator ID。
        /// 避免强制设回 250(WorkBase002) 导致和状态机不匹配。
        /// </summary>
        private static int GetCurrentSafeAnimId()
        {
            if (_heroineAI == null || _actionStateMachineField == null) return 250;

            try
            {
                object stateMachine = _actionStateMachineField.GetValue(_heroineAI);
                if (stateMachine == null) return 250;

                PropertyInfo keyProp = stateMachine.GetType().GetProperty("CurrentStateKey", BindingFlags.Public | BindingFlags.Instance);
                if (keyProp == null) return 250;

                object skObj = keyProp.GetValue(stateMachine);
                int stateKey = skObj is int ski ? ski : Convert.ToInt32(skObj);

                switch (stateKey)
                {
                    case 19: return 300; // WorkPC -> PCBase
                    case 20: return 350; // WorkBook
                    case 21: return 400; // WorkReport
                    case 5: return 200;  // BreakForward
                    case 6: return 260;  // BreakTeaTime
                    case 7: return 270;  // BreakReadBook
                    case 8: return 280;  // BreakMovie
                    case 11: return 290; // BreakListenMusic
                    case 16: return 300; // BreakSleep
                    default:
                        Log.Info($"[GameBridge] 状态机 key={stateKey}，回退到默认动画 250");
                        return 250;
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"[GameBridge] GetCurrentSafeAnimId 出错: {ex.Message}");
                return 250;
            }
        }
    }
}
