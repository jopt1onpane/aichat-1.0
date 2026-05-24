using UnityEngine;

namespace AIChat.Unity
{
    /// <summary>
    /// 下载进度面板的 Lo-Fi 暖色 IMGUI 样式（与游戏设置页气质接近）。
    /// </summary>
    internal static class DownloadUiStyles
    {
        private static bool _ready;
        private static GUIStyle _window;
        private static GUIStyle _title;
        private static GUIStyle _body;
        private static GUIStyle _hint;
        private static GUIStyle _modelName;
        private static GUIStyle _phase;
        private static GUIStyle _detail;
        private static GUIStyle _primaryBtn;
        private static GUIStyle _secondaryBtn;
        private static Texture2D _texWindow;
        private static Texture2D _texTitle;
        private static Texture2D _texBtn;
        private static Texture2D _texBtnHover;
        private static Texture2D _texBtnActive;
        private static Texture2D _texSecondary;
        private static Texture2D _texTrack;
        private static Texture2D _texFill;
        private static Texture2D _texFillDone;
        private static Texture2D _texFillFail;

        // Lo-Fi 暖色板
        private static readonly Color WindowBg     = new Color(0.96f, 0.93f, 0.88f, 0.97f);
        private static readonly Color TitleBg      = new Color(0.78f, 0.70f, 0.82f, 1f);
        private static readonly Color TextMain     = new Color(0.28f, 0.24f, 0.22f, 1f);
        private static readonly Color TextMuted    = new Color(0.45f, 0.40f, 0.38f, 1f);
        private static readonly Color Accent       = new Color(0.82f, 0.58f, 0.52f, 1f);
        private static readonly Color AccentHover  = new Color(0.88f, 0.66f, 0.58f, 1f);
        private static readonly Color BtnSecondary = new Color(0.90f, 0.86f, 0.80f, 1f);
        private static readonly Color Track        = new Color(0.82f, 0.78f, 0.72f, 1f);
        private static readonly Color Fill         = new Color(0.72f, 0.58f, 0.68f, 1f);
        private static readonly Color FillDone     = new Color(0.58f, 0.72f, 0.62f, 1f);
        private static readonly Color FillFail     = new Color(0.82f, 0.48f, 0.44f, 1f);

        public static GUIStyle Window { get { Ensure(); return _window; } }
        public static GUIStyle Title { get { Ensure(); return _title; } }
        public static GUIStyle Body { get { Ensure(); return _body; } }
        public static GUIStyle Hint { get { Ensure(); return _hint; } }
        public static GUIStyle ModelName { get { Ensure(); return _modelName; } }
        public static GUIStyle Phase { get { Ensure(); return _phase; } }
        public static GUIStyle Detail { get { Ensure(); return _detail; } }
        public static GUIStyle PrimaryBtn { get { Ensure(); return _primaryBtn; } }
        public static GUIStyle SecondaryBtn { get { Ensure(); return _secondaryBtn; } }
        public static Texture2D TrackTex { get { Ensure(); return _texTrack; } }
        public static Texture2D FillTex { get { Ensure(); return _texFill; } }
        public static Texture2D FillDoneTex { get { Ensure(); return _texFillDone; } }
        public static Texture2D FillFailTex { get { Ensure(); return _texFillFail; } }

        private static void Ensure()
        {
            if (!_ready) Init();
        }

        private static void Init()
        {
            int fontSize = Mathf.Clamp((int)(Screen.height * 0.016f), 14, 22);
            int titleSize = fontSize + 2;

            _texWindow      = MakeTex(WindowBg);
            _texTitle       = MakeTex(TitleBg);
            _texBtn         = MakeTex(Accent);
            _texBtnHover    = MakeTex(AccentHover);
            _texBtnActive   = MakeTex(new Color(Accent.r * 0.9f, Accent.g * 0.9f, Accent.b * 0.9f, 1f));
            _texSecondary   = MakeTex(BtnSecondary);
            _texTrack       = MakeTex(Track);
            _texFill        = MakeTex(Fill);
            _texFillDone    = MakeTex(FillDone);
            _texFillFail    = MakeTex(FillFail);

            _window = new GUIStyle(GUI.skin.window)
            {
                fontSize = titleSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter,
                padding = new RectOffset(14, 14, 32, 10)
            };
            _window.normal.background = _texWindow;
            _window.onNormal.background = _texWindow;
            _window.normal.textColor = TextMain;
            _window.onNormal.textColor = TextMain;

            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = titleSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };
            _title.normal.textColor = TextMain;

            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                wordWrap = true,
                richText = true
            };
            _body.normal.textColor = TextMain;

            _hint = new GUIStyle(_body)
            {
                fontSize = fontSize - 1,
                fontStyle = FontStyle.Italic
            };
            _hint.normal.textColor = TextMuted;

            _modelName = new GUIStyle(_body)
            {
                fontStyle = FontStyle.Bold
            };

            _phase = new GUIStyle(_body)
            {
                fontSize = fontSize - 1
            };
            _phase.normal.textColor = TextMuted;

            _detail = new GUIStyle(_body)
            {
                fontSize = fontSize - 1
            };
            _detail.normal.textColor = FillFail;

            _primaryBtn = MakeButton(fontSize, _texBtn, _texBtnHover, _texBtnActive, Color.white);
            _secondaryBtn = MakeButton(fontSize, _texSecondary, _texSecondary, _texSecondary, TextMain);

            _ready = true;
        }

        private static GUIStyle MakeButton(int fontSize, Texture2D normal, Texture2D hover, Texture2D active, Color textColor)
        {
            var s = new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(14, 14, 8, 8),
                border = new RectOffset(4, 4, 4, 4)
            };
            s.normal.background = normal;
            s.hover.background = hover;
            s.active.background = active;
            s.normal.textColor = textColor;
            s.hover.textColor = textColor;
            s.active.textColor = textColor;
            return s;
        }

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, c);
            t.Apply();
            t.hideFlags = HideFlags.HideAndDontSave;
            return t;
        }

        public static string PhaseLabel(string phase)
        {
            switch (phase)
            {
                case "downloading": return "下载中";
                case "verifying":   return "校验中";
                case "done":        return "完成";
                case "failed":      return "失败";
                default:            return phase ?? "";
            }
        }
    }
}
