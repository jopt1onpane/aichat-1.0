using System;
using System.Reflection;
using AIChat.Utils;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AIChat.Unity
{
    /// <summary>
    /// 在游戏「设置 → 常规」页底部注入「下载语音 / 模型资源」按钮。
    ///
    /// Steam PC 版走 <c>Bulbul.SettingUI</c>（右下角齿轮打开的那个设置窗口），
    /// 字段名 <c>_generalInitButton</c>。
    /// 移动端走 <c>Bulbul.Mobile.SettingGeneralView</c>，字段名 <c>generalInitButton</c>。
    /// </summary>
    internal static class SettingViewDownloadButtonHarmony
    {
        private static bool _applied;

        public static Action OnDownloadButtonClicked;

        internal static void TryApply(ManualLogSource log)
        {
            if (_applied) return;

            int patched = 0;
            patched += TryPatchType(log, "Bulbul.SettingUI", "Setup", "_generalInitButton");
            patched += TryPatchType(log, "Bulbul.Mobile.SettingGeneralView", "Setup", "generalInitButton");

            if (patched > 0)
            {
                _applied = true;
                log.LogInfo($"[下载按钮] Harmony 已挂载 {patched} 处（PC SettingUI + 可选 Mobile）");
            }
            else
            {
                log.LogWarning("[下载按钮] 未能挂载任何设置页 Patch，请检查 BepInEx 日志");
            }
        }

        private static int TryPatchType(ManualLogSource log, string typeName, string methodName, string initButtonFieldName)
        {
            try
            {
                Type t = Type.GetType(typeName + ", Assembly-CSharp");
                if (t == null)
                {
                    log.LogWarning($"[下载按钮] 未找到类型 {typeName}");
                    return 0;
                }

                MethodInfo method = t.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    log.LogWarning($"[下载按钮] 未找到 {typeName}.{methodName}");
                    return 0;
                }

                // 把字段名存到 Harmony 的 state（用静态字典按类型区分）
                _fieldNameByType[t] = initButtonFieldName;

                var harmony = new Harmony("com.username.chillaimod.settingview.downloadbutton." + t.Name);
                harmony.Patch(method, postfix: new HarmonyMethod(
                    typeof(SettingViewDownloadButtonHarmony), nameof(SetupPostfix)));
                log.LogInfo($"[下载按钮] 已 Patch {typeName}.{methodName} (模板字段={initButtonFieldName})");
                return 1;
            }
            catch (Exception ex)
            {
                log.LogWarning($"[下载按钮] Patch {typeName} 失败: {ex.Message}");
                return 0;
            }
        }

        private static readonly System.Collections.Generic.Dictionary<Type, string> _fieldNameByType
            = new System.Collections.Generic.Dictionary<Type, string>();

        private class BoundDownloadButton : MonoBehaviour { }

        private static void SetupPostfix(MonoBehaviour __instance)
        {
            try
            {
                if (__instance == null) return;
                if (__instance.GetComponent<BoundDownloadButton>() != null) return;
                __instance.gameObject.AddComponent<BoundDownloadButton>();

                string fieldName = null;
                _fieldNameByType.TryGetValue(__instance.GetType(), out fieldName);
                if (string.IsNullOrEmpty(fieldName))
                    fieldName = "_generalInitButton"; // PC 默认

                InjectDownloadButton(__instance, fieldName);
            }
            catch (Exception ex)
            {
                Log.Warning($"[下载按钮] Postfix 注入失败: {ex.Message}");
            }
        }

        private static void InjectDownloadButton(MonoBehaviour settingViewInstance, string initButtonFieldName)
        {
            Type viewType = settingViewInstance.GetType();
            FieldInfo initBtnField = viewType.GetField(initButtonFieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (initBtnField == null)
            {
                Log.Warning($"[下载按钮] {viewType.Name} 未找到字段 {initButtonFieldName}");
                return;
            }

            var templateBehaviour = initBtnField.GetValue(settingViewInstance) as MonoBehaviour;
            if (templateBehaviour == null)
            {
                Log.Warning($"[下载按钮] {initButtonFieldName} 为 null");
                return;
            }

            Transform parent = templateBehaviour.transform.parent;
            if (parent == null)
            {
                Log.Warning("[下载按钮] template parent 为 null");
                return;
            }

            // 防重复：同一 parent 下已有我们的按钮就不再克隆
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform ch = parent.GetChild(i);
                if (ch != null && ch.name == "ChillAIDownloadResourcesButton")
                {
                    Log.Info("[下载按钮] 已存在，跳过重复注入");
                    WireButtonClick(ch.gameObject);
                    return;
                }
            }

            GameObject clone = Object.Instantiate(templateBehaviour.gameObject, parent, false);
            clone.name = "ChillAIDownloadResourcesButton";
            clone.SetActive(true);

            WireButtonClick(clone);
            ReplaceButtonText(clone, "下载 AI 模型资源");

            // 关键修正：_generalInitButton 是浮在 ScrollView 底部的按钮（不是 LayoutGroup 内项），
            // 直接 Instantiate 会让 clone 和 template 完全重叠。要复制 RectTransform 的 anchor，
            // 然后把 anchoredPosition.y 上移一个按钮高度，让 clone 显示在 template 正上方。
            RectTransform templateRT = templateBehaviour.transform as RectTransform;
            RectTransform cloneRT = clone.transform as RectTransform;
            if (templateRT != null && cloneRT != null)
            {
                cloneRT.anchorMin       = templateRT.anchorMin;
                cloneRT.anchorMax       = templateRT.anchorMax;
                cloneRT.pivot           = templateRT.pivot;
                cloneRT.sizeDelta       = templateRT.sizeDelta;
                cloneRT.localScale      = templateRT.localScale;
                cloneRT.localRotation   = templateRT.localRotation;

                float yOffset = Mathf.Max(templateRT.sizeDelta.y, 50f) + 16f; // 按钮高度 + 间距
                cloneRT.anchoredPosition = templateRT.anchoredPosition + new Vector2(0f, yOffset);

                Log.Info($"[下载按钮] template anchoredPos={templateRT.anchoredPosition} size={templateRT.sizeDelta} anchorMin={templateRT.anchorMin} anchorMax={templateRT.anchorMax}");
                Log.Info($"[下载按钮] clone    anchoredPos={cloneRT.anchoredPosition}（在模板上方 {yOffset}px）");
            }
            else
            {
                Log.Warning("[下载按钮] template/clone 缺少 RectTransform，位置可能与模板重叠");
            }

            clone.transform.SetAsLastSibling();

            Log.Info($"[下载按钮] 已注入 {viewType.Name} / parent={parent.name} sibling={clone.transform.GetSiblingIndex()} childCount={parent.childCount} activeSelf={clone.activeSelf} activeInHierarchy={clone.activeInHierarchy}");
        }

        private static void ApplyDownloadButtonAccent(GameObject clone)
        {
            // 暖色强调：文字略偏珊瑚色，与设置页灰白按钮区分但不突兀
            var accentText = new Color(0.82f, 0.52f, 0.46f, 1f);
            Component tmpText = FindTmpTextRecursive(clone.transform);
            if (tmpText != null)
            {
                try
                {
                    PropertyInfo colorProp = tmpText.GetType().GetProperty("color",
                        BindingFlags.Public | BindingFlags.Instance);
                    colorProp?.SetValue(tmpText, accentText, null);
                }
                catch { /* ignore */ }
            }
            else
            {
                Text legacy = clone.GetComponentInChildren<Text>(true);
                if (legacy != null) legacy.color = accentText;
            }

            // 按钮底图轻微暖色 tint
            foreach (var img in clone.GetComponentsInChildren<Image>(true))
            {
                if (img == null) continue;
                Color c = img.color;
                img.color = new Color(
                    Mathf.Min(1f, c.r * 1.04f),
                    Mathf.Min(1f, c.g * 0.98f),
                    Mathf.Min(1f, c.b * 0.94f),
                    c.a);
            }
        }

        private static void ReplaceButtonText(GameObject clone, string newText)
        {
            Component tmpText = FindTmpTextRecursive(clone.transform);
            if (tmpText != null)
            {
                foreach (var c in tmpText.GetComponents<Component>())
                {
                    if (c != null && c.GetType().Name == "TextLocalizationBehaviour" && c is Behaviour b)
                        b.enabled = false;
                }
                try
                {
                    PropertyInfo textProp = tmpText.GetType().GetProperty("text",
                        BindingFlags.Public | BindingFlags.Instance);
                    textProp?.SetValue(tmpText, newText, null);
                }
                catch (Exception ex) { Log.Warning($"[下载按钮] 写 TMP 文本失败: {ex.Message}"); }
            }
            else
            {
                Text legacy = clone.GetComponentInChildren<Text>(true);
                if (legacy != null) legacy.text = newText;
            }
        }

        private static void WireButtonClick(GameObject clone)
        {
            Button btn = clone.GetComponentInChildren<Button>(true);
            if (btn == null)
            {
                Log.Warning("[下载按钮] 未找到 Button");
                return;
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                try { OnDownloadButtonClicked?.Invoke(); }
                catch (Exception ex) { Log.Warning($"[下载按钮] 回调异常: {ex.Message}"); }
            });
            btn.interactable = true;

            // SettingInitButton 可能有 Deactivate 灰显逻辑，强制激活外观
            foreach (var c in clone.GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                if (c.GetType().Name == "SettingInitButton")
                {
                    MethodInfo activate = c.GetType().GetMethod("Activate",
                        BindingFlags.Public | BindingFlags.Instance);
                    activate?.Invoke(c, null);
                }
            }
        }

        private static Component FindTmpTextRecursive(Transform root)
        {
            if (root == null) return null;
            foreach (var c in root.GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                Type t = c.GetType();
                while (t != null)
                {
                    if (t.Name == "TMP_Text" || t.Name == "TextMeshProUGUI" || t.Name == "TextMeshPro")
                        return c;
                    t = t.BaseType;
                }
            }
            return null;
        }
    }
}
