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

        public static MethodInfo _changeAnimSmoothMethod;
        public static MethodInfo _lookInitMethod;
        public static MethodInfo _lookAtMethod;

        private static MonoBehaviour _heroineAI;
        private static FieldInfo _actionStateMachineField;
        private static PropertyInfo _currentStateProperty;
        private static object _facilityVoiceTextScenario;
        private static MethodInfo _cancelVoiceTextScenarioMethod;
        private static int _lastCancelVoiceFrame;
        private static int _lastVoiceScenarioSearchFrame;

        private static readonly HashSet<string> _unsafeStates = new HashSet<string>
        {
            "ScenarioState", "SleepState", "StandUpState", "SitDownState", "WalkState"
        };

        public static void FindHeroineService()
        {
            var allComponents = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var comp in allComponents)
            {
                string typeName = comp.GetType().FullName;
                if (typeName == "Bulbul.HeroineService")
                {
                    _heroineService = comp;
                    _cachedAnimator = comp.GetComponent<Animator>();

                    _changeAnimSmoothMethod = comp.GetType().GetMethod("ChangeHeroineAnimationForInteger", BindingFlags.Public | BindingFlags.Instance);
                    _lookInitMethod = comp.GetType().GetMethod("LookInitSlowly", BindingFlags.Public | BindingFlags.Instance);
                    _lookAtMethod = comp.GetType().GetMethod("ChangeLookScaleAnimation", BindingFlags.Public | BindingFlags.Instance);

                    if (_changeAnimSmoothMethod != null) Log.Warning($"✅ 核心连接成功: {comp.gameObject.name}");
                }
                else if (typeName == "Bulbul.HeroineAI")
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
                    CacheFacilityVoiceTextScenario(comp);
                }
            }
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

                int stateKey = (int)keyProp.GetValue(stateMachine);

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
