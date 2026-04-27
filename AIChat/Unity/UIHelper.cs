using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AIChat.Utils;

namespace AIChat.Unity
{
    public static class UIHelper
    {
        /// <summary>
        /// 创建一个独立的 ScreenSpaceOverlay Canvas，专门用于 AI 字幕。
        /// 不带 GraphicRaycaster，因此不会阻挡游戏的 EventSystem 点击检测。
        /// </summary>
        public static GameObject CreateOverlayCanvas()
        {
            GameObject canvasObj = new GameObject(">>> AI_Overlay_Canvas <<<");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            return canvasObj;
        }

        public static void DestroyOverlayCanvas(GameObject overlayCanvasObj)
        {
            if (overlayCanvasObj != null) UnityEngine.Object.Destroy(overlayCanvasObj);
        }

        public static GameObject CreateOverlayText(GameObject parent)
        {
            GameObject go = new GameObject(">>> AI_TEXT <<<");
            go.transform.SetParent(parent.transform, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.05f);
            rt.anchorMax = new Vector2(0.9f, 0.25f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            GameObject bgObj = new GameObject("AI_TEXT_BG");
            bgObj.transform.SetParent(go.transform, false);
            RectTransform bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = new Vector2(-10, -10);
            bgRt.offsetMax = new Vector2(10, 10);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.6f);
            bgImage.raycastTarget = false;

            Text txt = go.AddComponent<Text>();
            txt.fontSize = 28;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            Font f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f != null) txt.font = f;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.8f);
            outline.effectDistance = new Vector2(1, -1);

            return go;
        }
    }
}
