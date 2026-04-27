using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace AIChat.Unity
{
    public static class UIHelper
    {
        private static readonly Dictionary<CanvasGroup, float> _savedCanvasGroupAlpha = new Dictionary<CanvasGroup, float>();

        public static void ForceShowWindow(GameObject target, Dictionary<GameObject, bool> uiStatusMap)
        {
            target.SetActive(true);
            var p = target.transform.parent;
            while (p != null && p.name != "Canvas")
            {
                if (uiStatusMap != null && !uiStatusMap.ContainsKey(p.gameObject))
                {
                    uiStatusMap.Add(p.gameObject, p.gameObject.activeSelf);
                }
                p.gameObject.SetActive(true);
                p = p.parent;
            }

            _savedCanvasGroupAlpha.Clear();
            foreach (var cg in target.GetComponentsInParent<CanvasGroup>())
            {
                _savedCanvasGroupAlpha[cg] = cg.alpha;
                cg.alpha = 1f;
            }

            if (target.transform.parent != null && target.transform.parent.parent != null)
                target.transform.parent.parent.localScale = Vector3.one;
        }

        public static void RestoreUiStatus(Dictionary<GameObject, bool> uiStatusMap, GameObject myTextObj, GameObject originalTextObj)
        {
            foreach (var kvp in _savedCanvasGroupAlpha)
            {
                if (kvp.Key != null) kvp.Key.alpha = kvp.Value;
            }
            _savedCanvasGroupAlpha.Clear();

            if (uiStatusMap != null)
            {
                foreach (var kvp in uiStatusMap)
                {
                    if (kvp.Key != null) kvp.Key.SetActive(kvp.Value);
                }
                uiStatusMap.Clear();
            }

            if (myTextObj != null) UnityEngine.Object.Destroy(myTextObj);
            if (originalTextObj != null) originalTextObj.SetActive(true);
        }

        public static GameObject CreateOverlayText(GameObject parent)
        {
            GameObject go = new GameObject(">>> AI_TEXT <<<");
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.sizeDelta = Vector2.zero;
            Text txt = go.AddComponent<Text>();
            txt.fontSize = 26;
            txt.alignment = TextAnchor.UpperCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            Font f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f != null) txt.font = f;
            return go;
        }
    }
}
