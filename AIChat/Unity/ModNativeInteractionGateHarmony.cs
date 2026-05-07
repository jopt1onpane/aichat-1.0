using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;

namespace AIChat.Unity
{
    /// <summary>
    /// mod 整段 AI 会话（从发起到字幕/TTS/动画结束）为 true 时，
    /// Harmony 拦截 <c>FacilityClickHeroine.ReactionReady</c>，禁止原生「点击女主」与「独白 HeroineSelf」抢入口，
    /// 避免与 mod Overlay 字幕叠两层、与 mod 动画抢状态机。
    /// </summary>
    public static class ModNativeInteractionSession
    {
        public static bool SuppressNativeClickReactions;
    }

    internal static class ModNativeInteractionGateHarmony
    {
        private static bool _applied;

        internal static void TryApply(ManualLogSource log)
        {
            if (_applied) return;
            try
            {
                Type facilityClickType = Type.GetType("Bulbul.FacilityClickHeroine, Assembly-CSharp");
                if (facilityClickType == null)
                {
                    log.LogWarning("[交互门控] 未找到 Bulbul.FacilityClickHeroine，跳过 Harmony。");
                    return;
                }

                Type reactionEnum = facilityClickType.GetNestedType("ReactionType", BindingFlags.Public);
                if (reactionEnum == null)
                {
                    log.LogWarning("[交互门控] 未找到 ReactionType，跳过 Harmony。");
                    return;
                }

                MethodInfo method = facilityClickType.GetMethod(
                    "ReactionReady",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { reactionEnum },
                    null);
                if (method == null)
                {
                    log.LogWarning("[交互门控] 未找到 ReactionReady(ReactionType)，跳过 Harmony。");
                    return;
                }

                var harmony = new Harmony("com.username.chillaimod.facilityclickheroine.gate");
                harmony.Patch(method, prefix: new HarmonyMethod(typeof(ModNativeInteractionGateHarmony), nameof(ReactionReadyPrefix)));
                _applied = true;
                log.LogInfo("[交互门控] Harmony 已挂载：mod 会话中会拒绝 ReactionReady（含点击与独白）");
            }
            catch (Exception ex)
            {
                log.LogWarning($"[交互门控] Harmony 应用失败: {ex.Message}");
            }
        }

        private static bool ReactionReadyPrefix(ref bool __result)
        {
            if (ModNativeInteractionSession.SuppressNativeClickReactions)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
