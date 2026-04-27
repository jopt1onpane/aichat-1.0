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
    }
}
