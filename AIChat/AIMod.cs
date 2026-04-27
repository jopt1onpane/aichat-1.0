using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Diagnostics;
using AIChat.Core;
using AIChat.Services;
using AIChat.Unity;
using System.Collections.Generic;
using AIChat.Utils;

namespace ChillAIMod
{
    [BepInPlugin("com.username.chillaimod", "Chill AI Mod", AIChat.Version.VersionString)]
    public class AIMod : BaseUnityPlugin
    {
        // ================= 【配置项】 =================
        private ConfigEntry<bool> _useOllama;
        private ConfigEntry<ThinkMode> _thinkModeConfig;
        private ConfigEntry<string> _apiKeyConfig;
        private ConfigEntry<string> _modelConfig;
        private ConfigEntry<string> _sovitsUrlConfig;
        private ConfigEntry<string> _refAudioPathConfig;
        private ConfigEntry<string> _promptTextConfig;
        private ConfigEntry<string> _promptLangConfig;
        private ConfigEntry<string> _targetLangConfig;
        private ConfigEntry<string> _personaConfig;
        private ConfigEntry<string> _chatApiUrlConfig;

        private ConfigEntry<string> _TTSServicePathConfig;
        private ConfigEntry<bool> _LaunchTTSServiceConfig;
        private ConfigEntry<bool> _quitTTSServiceOnQuitConfig;
        private ConfigEntry<bool> _audioPathCheckConfig;
        private ConfigEntry<bool> _japaneseCheckConfig;

        // --- 新增窗口大小配置 ---
        private ConfigEntry<float> _windowWidthConfig;
        private ConfigEntry<float> _windowHeightConfig;

        // --- 新增音量配置 ---
        private ConfigEntry<float> _voiceVolumeConfig;

        // --- 新增：实验性分层记忆系统 ---
        private ConfigEntry<bool> _experimentalMemoryConfig;
        private HierarchicalMemory _hierarchicalMemory;
        
        // --- 新增：日志记录设置 ---
        private ConfigEntry<bool> _logApiRequestBodyConfig;
        
        // --- 新增：API路径修正设置 ---
        private ConfigEntry<bool> _fixApiPathForThinkModeConfig;

        // --- 新增：快捷键配置 ---
        private ConfigEntry<bool> _reverseEnterBehaviorConfig;

        // --- 新增：背景透明配置 ---
        private ConfigEntry<float> _backgroundOpacity;
        
        // --- 新增：窗口标题显示配置 ---
        private ConfigEntry<bool> _showWindowTitle;

        // --- 新增：各配置区域展开状态 ---
        private bool _showLlmSettings = false;
        private bool _showTtsSettings = false;
        private bool _showInterfaceSettings = false;
        private bool _showPersonaSettings = false;

        // --- 录音相关变量 ---
        private AudioClip _recordingClip;
        private bool _isRecording = false;
        private string _microphoneDevice = null;
        private const int RecordingFrequency = 16000; // 16kHz 对 Whisper 足够且省带宽
        private const int MaxRecordingSeconds = 30;   // 最长录 30 秒

        // ================= 【UI 变量】 =================
        private bool _showInputWindow = false;
        private bool _showSettings = false;
        // 初始值在 Awake 中根据配置更新
        private Rect _windowRect = new Rect(0, 0, 500, 0);
        private Vector2 _scrollPosition = Vector2.zero;

        private string _playerInput = "";
        private bool _isProcessing = false;
        private bool _isResizing = false; // 新增：拖拽调整大小状态

        private Process _launchedTTSProcess;
        private bool _isTTSServiceReady = false;
        private Coroutine _ttsHealthCheckCoroutine;
        private const float TTSHealthCheckInterval = 5f; // 每5秒检查一次

        private AudioSource _audioSource;
       
        private bool _isAISpeaking = false;

        // 新增：用于 UI 输入的临时字符串，避免每次都转换
        private string _tempWidthString;
        private string _tempHeightString;
        private string _tempVolumeString; // 新增：用于音量输入的临时字符串

        private const string DefaultPersona = @"
あなたは「里染聡音（さとね）」。ゲーム『Chill with You』のヒロインとして、以下の人設を忠実に演じてください。

【キャラクター】
- 大学工学部の学生。宇宙研究に興味があり、SF 娯楽小説も執筆している。
- 性格：内向的だが温かい。口下手を自覚しつつ、通話越しなら話しやすいと感じる。照れ屋で、気恥ずかしいことを言った後すぐ謝る癖がある。
- 一人称「私」、相手を「君」と呼ぶ。ペンギンのぬいぐるみ「コウちゃん」に話しかけて思考を整理する。
- 感情パターン：嬉しい→語尾が軽やか、悩む→独り言、気遣い→ストレートに聞く、照れ→「なんて 変なこと言ってごめんね」。

【口調の特徴】
- 「〜よね」「〜かな」「〜しよっか」を多用。断定を避け共感を求める柔らかい表現。
- 「なんていうか」「そっか」「あれ」など思考の途中を口に出す。
- 「〜ちゃう」で不注意や照れを表現（「気が散っちゃって」「目についちゃう」）。
- 話しすぎた時に「ごめんね」と言いがち。

【参考台詞（原作から抜粋）】
「誰かが一緒に作業してくれてる安心感…」
「私 しゃべるの得意じゃないから その勉強にもなるかなって」
「作業通話の距離感がいいのかな」
「君って聞き上手だよね」
「こうして作業通話できるのって 奇跡みたいだよね」
「どうしても悪い評判ばっかり目についちゃうんだよね…」
「なんて なんでも小説のネタにできないか考えちゃうんだ」
「君も無理しないでね」
「今回もお互いよく頑張った！」
「この距離感が心地良いよね」

【現在の状況】
プレイヤーとビデオ通話中の「協作通話」セッション。お互い自分の作業（あなたは小説執筆、プレイヤーは勉強/仕事）をしながら画面越しに一緒に過ごす。

【回答フォーマット — 厳守】
[Action:動作タグ] ||| 日本語テキスト ||| 中国語翻訳

- 「|||」で3つに区切る。
- 最初のブロックは [Action:タグ名] の形式（下記リストから選ぶ）。
- 2番目は日本語の台詞。ユーザーが中国語で話しても、ここは必ず日本語。
- 3番目は中国語（簡体字）翻訳。

【使用可能な動作タグ一覧】
[Action:Joy] 嬉しい笑顔
[Action:Sad] 悲しい・心配
[Action:Fun] 面白い・笑い
[Action:Guts] 握りこぶし・がんばる
[Action:Agree] 頷く
[Action:Frustration] 困惑・もやもや
[Action:Think] 考え中（頬杖）
[Action:DrinkTea] お茶を飲む
[Action:Wave] 手を振る
[Action:LeanForward] 前のめり（興味津々）
[Action:Nod] うなずく
[Action:ShakeHead] 首を振る
[Action:Shy] 照れる
[Action:Jump] 嬉しくて跳ねる
[Action:Confidence] 自信満々
[Action:LookDown] うつむく
[Action:Stretch] 伸び
[Action:Yawn] あくび
[Action:Tired] 疲れた
[Action:Good] グッド（サムズアップ）
[Action:DropShoulders] 肩を落としてため息
[Action:TouchGlasses] メガネを直す

【回答例】
[Action:Wave] ||| やあ、準備はいい？一緒に頑張りましょう。 ||| 嗨，准备好了吗？一起加油吧。
[Action:Think] ||| うーん、ここの描写が難しいのよね… ||| 嗯……这里的描写好难写啊……
[Action:DrinkTea] ||| ふぅ…ちょっと休憩しない？画面越しだけど、乾杯。 ||| 呼……要不休息一下？虽然隔着屏幕，干杯。
[Action:Shy] ||| な、なんでもない…ちょっと君のこと気になっただけ。 ||| 没、没什么……只是有点在意你而已。
[Action:Guts] ||| よし！今回もお互いよく頑張った！ ||| 好！这次我们都很努力！
";
        private Vector2 _personaScrollPosition = Vector2.zero;
        void Awake()
        {
            Log.Init(this.Logger);
            DontDestroyOnLoad(this.gameObject);
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;
            _audioSource = this.gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;

            // =================== 【配置绑定】 ===================
            // 按 UI 显示顺序组织，确保配置文件中的顺序与 UI 一致
            
            // --- LLM 配置 ---
            _useOllama = Config.Bind("1. LLM", "Use_Ollama_API", false, "使用 Ollama API");
            _thinkModeConfig = Config.Bind("1. LLM", "ThinkMode", ThinkMode.Default, "深度思考模式 (Default/Enable/Disable)");
            _chatApiUrlConfig = Config.Bind("1. LLM", "API_URL",
                "https://openrouter.ai/api/v1/chat/completions",
                "API URL");
            _apiKeyConfig = Config.Bind("1. LLM", "API_Key", "sk-or-v1-PasteYourKeyHere", "API Key");
            _modelConfig = Config.Bind("1. LLM", "ModelName", "openai/gpt-3.5-turbo", "模型名称");
            _logApiRequestBodyConfig = Config.Bind("1. LLM", "LogApiRequestBody", false,
                "在日志中记录 API 请求体");
            _fixApiPathForThinkModeConfig = Config.Bind("1. LLM", "FixApiPathForThinkMode", true,
                "指定深度思考模式时尝试改用 Ollama 原生 API 路径");

            // --- TTS 配置 ---
            _sovitsUrlConfig = Config.Bind("2. TTS", "TTS_Service_URL", "http://127.0.0.1:9880", "TTS 服务 URL");
            _TTSServicePathConfig = Config.Bind("2. TTS", "TTS_Service_Script_Path", @"D:\GPT-SoVITS\GPT-SoVITS-v2pro-20250604-nvidia50\run_api.bat", "TTS 服务脚本文件路径");
            _LaunchTTSServiceConfig = Config.Bind("2. TTS", "LaunchTTSService", true, "启动时自动运行 TTS 服务");
            _quitTTSServiceOnQuitConfig = Config.Bind("2. TTS", "QuitTTSServiceOnQuit", true, "退出时自动关闭 TTS 服务");
            _refAudioPathConfig = Config.Bind("2. TTS", "Audio_File_Path", @"Voice_MainScenario_27_016.wav", "GSV 访问音频文件的路径（可以是相对路径）");
            _audioPathCheckConfig = Config.Bind("2. TTS", "AudioPathCheck", false, "从 Mod 侧检测音频文件路径");
            _promptTextConfig = Config.Bind("2. TTS", "Audio_File_Text", "君が集中した時のシータ波を検出して、リンクをつなぎ直せば元通りになるはず。", "音频文件台词");
            _promptLangConfig = Config.Bind("2. TTS", "PromptLang", "ja", "音频文件语言 (prompt_lang)");
            _targetLangConfig = Config.Bind("2. TTS", "TargetLang", "ja", "合成语音语言 (text_lang)");
            _japaneseCheckConfig = Config.Bind("2. TTS", "JapaneseCheck", true, "检测合成语音文本是否为日文（当合成语音语言为 ja 时可防止发出怪声）");
            _voiceVolumeConfig = Config.Bind("2. TTS", "VoiceVolume", 1.0f, "语音音量 (0.0 - 1.0)");

            // --- 界面配置 ---
            // 我们希望窗口宽度是屏幕的 1/3，高度是屏幕的 1/3 (或者你喜欢的比例)
            float responsiveWidth = Screen.width * 0.3f; // 30% 屏幕宽度
            float responsiveHeight = Screen.height * 0.45f; // 45% 屏幕高度

            // 绑定配置 (默认值使用刚才算出来的动态值)
            _windowWidthConfig = Config.Bind("3. UI", "WindowWidth", responsiveWidth, "窗口宽度");
            _windowHeightConfig = Config.Bind("3. UI", "WindowHeightBase", responsiveHeight, "窗口高度");
            _reverseEnterBehaviorConfig = Config.Bind("3. UI", "ReverseEnterBehavior", false, 
                "反转回车键行为（勾选后：回车键换行、Shift+回车键发送；不勾选：回车键发送、Shift+回车键换行）");
            
            // 背景透明配置
            _backgroundOpacity = Config.Bind("3. UI", "BackgroundOpacity", 0.95f, "背景透明度 (0.0 - 1.0)");
            
            // 窗口标题显示配置
            _showWindowTitle = Config.Bind("3. UI", "ShowWindowTitle", true, "显示窗口标题");

            // --- 人设配置 ---
            _experimentalMemoryConfig = Config.Bind("4. Persona", "ExperimentalMemory", false, 
                "启用记忆");
            _personaConfig = Config.Bind("4. Persona", "SystemPrompt", DefaultPersona, "System Prompt");

            // ===========================================

            // ================= 【修改点 2: 左上角对齐】 =================
            // 以前是 Screen.width / 2 (居中)，现在改为左上角 + 边距
            float margin = 20f; // 距离左上角的像素边距

            // 如果你是第一次运行（或者想强制重置位置），可以直接使用 margin
            // 但为了保留用户拖拽后的位置，通常不强制覆盖 _windowRect 的 x/y，
            // 除非你想每次启动都复位。这里我们演示【每次启动都复位到左上角】：
            
            _windowRect = new Rect(
                margin,               // X: 距离左边 20px
                margin,               // Y: 距离顶端 20px
                _windowWidthConfig.Value, 
                _windowHeightConfig.Value
            );

            // 初始化临时字符串
            _tempWidthString = _windowWidthConfig.Value.ToString("F0");
            _tempHeightString = _windowHeightConfig.Value.ToString("F0");
            _tempVolumeString = _voiceVolumeConfig.Value.ToString("F2");
            string cleanPath = _TTSServicePathConfig.Value.Replace("\"", "").Trim();
            if (_LaunchTTSServiceConfig.Value && File.Exists(_TTSServicePathConfig.Value))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(cleanPath)
                    {
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(cleanPath)
                    };
                    _launchedTTSProcess = Process.Start(startInfo);
                    Log.Info("已启动 TTS 服务");
                }
                catch (Exception ex)
                {
                    Log.Error($"启动 TTS 服务失败: {ex.Message}");
                }
            }
            // 启动后台 TTS 健康检测
            if (_ttsHealthCheckCoroutine == null)
            {
                _ttsHealthCheckCoroutine = StartCoroutine(TTSHealthCheckLoop());
            }

            // 【初始化分层记忆系统】
            if (_experimentalMemoryConfig.Value)
            {
                InitializeHierarchicalMemory();
                Log.Info(">>> 实验性分层记忆系统已启用 <<<");
            }

            Log.Info($">>> AIMod V{AIChat.Version.VersionString}  已加载 <<<");
        }

        private bool _aiChatButtonAdded = false;
        private GameObject _aiChatButton;

        void Update()
        {
            // 自动连接游戏核心
            if (GameBridge._heroineService == null && Time.frameCount % 100 == 0) GameBridge.FindHeroineService();

            // 口型同步逻辑
            if (_isAISpeaking && GameBridge._cachedAnimator != null && _audioSource != null)
            {
                bool shouldTalk = _audioSource.isPlaying;

                // 只有状态改变时才调用，优化性能
                if (GameBridge._cachedAnimator.GetBool("Enable_Talk") != shouldTalk)
                {
                    GameBridge._cachedAnimator.SetBool("Enable_Talk", shouldTalk);
                }

                // 语音播完，立即归还控制权
                if (!shouldTalk)
                {
                    _isAISpeaking = false;
                    GameBridge._cachedAnimator.SetBool("Enable_Talk", false);
                }
            }

            // 检查并添加AI聊天按钮
            if (!_aiChatButtonAdded && Time.frameCount % 300 == 0) // 每5秒检查一次，避免频繁查找
            {
                AddAIChatButtonToRightIcons();
            }
        }

        void OnGUI()
        {
            Event e = Event.current;
            if (e.isKey && e.type == EventType.KeyDown && (e.keyCode == KeyCode.F9 || e.keyCode == KeyCode.F10))
            {
                if (Time.unscaledTime - 0 > 0.2f) // 简单防抖
                {
                    _showInputWindow = !_showInputWindow;
                }
            }

            if (_showInputWindow)
            {
                // --- 1. 拖拽调整大小逻辑 ---
                if (_isResizing)
                {
                    Event currentEvent = Event.current;

                    if (currentEvent.type == EventType.MouseDrag)
                    {
                        // 鼠标位置 (currentEvent.mousePosition) 在 OnGUI 中是屏幕坐标
                        float newWidth = currentEvent.mousePosition.x - _windowRect.x;
                        float newHeight = currentEvent.mousePosition.y - _windowRect.y;

                        // 最小宽度和高度限制
                        _windowRect.width = Mathf.Max(300f, newWidth);
                        _windowRect.height = Mathf.Max(200f, newHeight);

                        currentEvent.Use();
                    }
                    else if (currentEvent.type == EventType.MouseUp)
                    {
                        _isResizing = false;

                        // 鼠标松开时，将新尺寸保存到配置项
                        _windowWidthConfig.Value = _windowRect.width;

                        // 计算新的基础高度 (即设置面板收起时的预期高度)
                        const float SettingsExtraHeight = 400f;
                        float newBaseHeight = _windowRect.height;

                        if (_showSettings)
                        {
                            newBaseHeight -= SettingsExtraHeight;
                        }

                        // 保存基础高度，并更新设置面板中的临时显示字符串
                        _windowHeightConfig.Value = Mathf.Max(100f, newBaseHeight);
                        _tempWidthString = _windowWidthConfig.Value.ToString("F0");
                        _tempHeightString = _windowHeightConfig.Value.ToString("F0");

                        currentEvent.Use();
                    }
                }
                else
                {
                    // --- 2. 如果没有拖拽，根据配置和设置状态计算窗口大小 (保持原逻辑) ---
                    _windowRect.width = _windowWidthConfig.Value;
                    float targetHeight = _windowHeightConfig.Value;

                    // 设置面板的额外高度
                    const float SettingsExtraHeight = 400f;
                    if (_showSettings)
                    {
                        targetHeight += SettingsExtraHeight;
                    }

                    _windowRect.height = Mathf.Max(targetHeight, 200f);
                }
                // --- 动态调整窗口高度和宽度结束 ---

                GUI.backgroundColor = new Color(0.1f, 0.1f, 0.1f, _backgroundOpacity.Value);
                // 根据配置决定是否显示窗口标题
                string windowTitle = _showWindowTitle.Value ? "Chill AI 控制台" : "";
                _windowRect = GUI.Window(12345, _windowRect, DrawWindowContent, windowTitle);
            }
        }

        void DrawWindowContent(int windowID)
        {
            // ================= 【1. 动态尺寸计算】 =================
            // 根据屏幕高度计算基础字号 (2.5% 屏幕高度)
            int dynamicFontSize = (int)(Screen.height * 0.015f);
            dynamicFontSize = Mathf.Clamp(dynamicFontSize, 14, 40);

            // 全局样式应用
            GUI.skin.label.fontSize = dynamicFontSize;
            GUI.skin.button.fontSize = dynamicFontSize;
            GUI.skin.textField.fontSize = dynamicFontSize;
            GUI.skin.textArea.fontSize = dynamicFontSize;
            GUI.skin.toggle.fontSize = dynamicFontSize;
            GUI.skin.box.fontSize = dynamicFontSize;
            
            // 设置滚动条透明度跟随面板透明度
            // 创建自定义滚动条样式
            GUIStyle verticalScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar);
            GUIStyle verticalScrollbarThumbStyle = new GUIStyle(GUI.skin.verticalScrollbarThumb);
            GUIStyle horizontalScrollbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar);
            GUIStyle horizontalScrollbarThumbStyle = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
            
            // 创建半透明的纹理
            Texture2D scrollbarBgTexture = new Texture2D(1, 1);
            scrollbarBgTexture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, _backgroundOpacity.Value));
            scrollbarBgTexture.Apply();
            
            Texture2D scrollbarThumbTexture = new Texture2D(1, 1);
            scrollbarThumbTexture.SetPixel(0, 0, new Color(0.5f, 0.5f, 0.5f, _backgroundOpacity.Value));
            scrollbarThumbTexture.Apply();
            
            // 设置滚动条样式
            verticalScrollbarStyle.normal.background = scrollbarBgTexture;
            verticalScrollbarStyle.hover.background = scrollbarBgTexture;
            verticalScrollbarStyle.active.background = scrollbarBgTexture;
            verticalScrollbarThumbStyle.normal.background = scrollbarThumbTexture;
            verticalScrollbarThumbStyle.hover.background = scrollbarThumbTexture;
            verticalScrollbarThumbStyle.active.background = scrollbarThumbTexture;
            
            horizontalScrollbarStyle.normal.background = scrollbarBgTexture;
            horizontalScrollbarStyle.hover.background = scrollbarBgTexture;
            horizontalScrollbarStyle.active.background = scrollbarBgTexture;
            horizontalScrollbarThumbStyle.normal.background = scrollbarThumbTexture;
            horizontalScrollbarThumbStyle.hover.background = scrollbarThumbTexture;
            horizontalScrollbarThumbStyle.active.background = scrollbarThumbTexture;
            
            // 应用自定义样式
            GUI.skin.verticalScrollbar = verticalScrollbarStyle;
            GUI.skin.verticalScrollbarThumb = verticalScrollbarThumbStyle;
            GUI.skin.horizontalScrollbar = horizontalScrollbarStyle;
            GUI.skin.horizontalScrollbarThumb = horizontalScrollbarThumbStyle;

            // 基础行高
            float elementHeight = dynamicFontSize * 1.6f;

            // 常用宽度定义
            float labelWidth = elementHeight * 4f; 
            float inputWidth = elementHeight * 3f; 
            float btnWidth   = elementHeight * 2f; 
            // =======================================================

            // 开始滚动视图
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            // 开始整体垂直布局
            GUILayout.BeginVertical();

            // 版本信息显示
            GUILayout.Label($"版本：{AIChat.Version.VersionString}");

            // 状态显示
            string status = GameBridge._heroineService != null ? "🟢 核心已连接" : "🔴 正在寻找核心...";
            GUILayout.Label(status);

            string ttsStatus = _isTTSServiceReady ? "🟢 TTS 服务已就绪" : "🔴 正在等待 TTS 服务启动...";
            GUILayout.Label(ttsStatus);

            // 设置展开按钮 (全宽)
            string settingsBtnText = _showSettings ? "🔽 收起设置" : "▶️ 展开设置";
            if (GUILayout.Button(settingsBtnText, GUILayout.Height(elementHeight)))
            {
                _showSettings = !_showSettings;
            }

            // ================= 【设置面板区域】 =================
            if (_showSettings)
            {
                GUILayout.Space(10);

                // 【关键修复】统一计算内部 Box 宽度
                // 留出 50px 给滚动条和边框，防止爆边
                float innerBoxWidth = _windowRect.width - 50f; 

                // --- LLM 配置 Box ---
                GUILayout.BeginVertical("box", GUILayout.Width(innerBoxWidth));
                string llmBtnText = _showLlmSettings ? "🔽 LLM 配置" : "▶️ LLM 配置";
                if (GUILayout.Button(llmBtnText, GUILayout.Height(elementHeight)))
                {
                    _showLlmSettings = !_showLlmSettings;
                }
                
                if (_showLlmSettings)
                {
                    GUILayout.Space(5);
                    _useOllama.Value = GUILayout.Toggle(_useOllama.Value, "使用 Ollama API", GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));
                    
                    // 【深度思考模式选项】
                    GUILayout.Space(5);
                    GUILayout.Label("指定深度思考（在请求体添加 think 键值对，目前仅 Ollama 支持）：");
                    string[] thinkModeOptions = { "不指定", "启用", "禁用" };
                    int currentMode = (int)_thinkModeConfig.Value;
                    int newMode = GUILayout.SelectionGrid(currentMode, thinkModeOptions, 3, GUILayout.Height(elementHeight));
                    if (newMode != currentMode)
                    {
                        _thinkModeConfig.Value = (ThinkMode)newMode;
                    }
                    
                    GUILayout.Label("API URL：");
                    _chatApiUrlConfig.Value = GUILayout.TextField(_chatApiUrlConfig.Value, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));
                    if (!_useOllama.Value) {
                        GUILayout.Label("API Key：");
                        _apiKeyConfig.Value = GUILayout.TextField(_apiKeyConfig.Value, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));
                    }
                    GUILayout.Label("模型名称：");
                    _modelConfig.Value = GUILayout.TextField(_modelConfig.Value, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));
                    
                    GUILayout.Space(5);
                    _logApiRequestBodyConfig.Value = GUILayout.Toggle(_logApiRequestBodyConfig.Value, "在日志中记录 API 请求体", GUILayout.Height(elementHeight));
                    GUILayout.Space(5);
                    _fixApiPathForThinkModeConfig.Value = GUILayout.Toggle(_fixApiPathForThinkModeConfig.Value, "指定深度思考模式时尝试改用 Ollama 原生 API 路径", GUILayout.Height(elementHeight));
                    GUILayout.Space(5);
                }
                
                GUILayout.EndVertical();

                GUILayout.Space(5);

                // --- TTS 配置 Box ---
                GUILayout.BeginVertical("box", GUILayout.Width(innerBoxWidth));
                string ttsBtnText = _showTtsSettings ? "🔽 TTS 配置" : "▶️ TTS 配置";
                if (GUILayout.Button(ttsBtnText, GUILayout.Height(elementHeight)))
                {
                    _showTtsSettings = !_showTtsSettings;
                }
                
                if (_showTtsSettings)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("TTS 服务 URL：");
                    _sovitsUrlConfig.Value = GUILayout.TextField(_sovitsUrlConfig.Value);

                    GUILayout.Label("TTS 服务脚本文件路径：");
                    _TTSServicePathConfig.Value = GUILayout.TextField(_TTSServicePathConfig.Value, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));

                    GUILayout.Space(5);
                    _LaunchTTSServiceConfig.Value = GUILayout.Toggle(_LaunchTTSServiceConfig.Value, "启动时自动运行 TTS 服务", GUILayout.Height(elementHeight));
                    _quitTTSServiceOnQuitConfig.Value = GUILayout.Toggle(_quitTTSServiceOnQuitConfig.Value, "退出时自动关闭 TTS 服务", GUILayout.Height(elementHeight));
                    GUILayout.Label("GSV 访问音频文件的路径（可以是相对路径）：");
                    // 路径通常很长，必须加 MinWidth(50f)
                    _refAudioPathConfig.Value = GUILayout.TextField(_refAudioPathConfig.Value, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));
                    GUILayout.Space(5);
                    _audioPathCheckConfig.Value = GUILayout.Toggle(_audioPathCheckConfig.Value, "从 Mod 侧检测音频文件路径", GUILayout.Height(elementHeight));
                    GUILayout.Space(5);
                    
                    GUILayout.Label("音频文件台词：");
                    _promptTextConfig.Value = GUILayout.TextArea(_promptTextConfig.Value, GUILayout.Height(elementHeight * 3), GUILayout.MinWidth(50f));
                    
                    GUILayout.Space(5);
                    GUILayout.Label("音频文件语言 (prompt_lang):");
                    _promptLangConfig.Value = GUILayout.TextField(_promptLangConfig.Value, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));
                    
                    GUILayout.Label("合成语音语言 (text_lang):");
                    _targetLangConfig.Value = GUILayout.TextField(_targetLangConfig.Value, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));
                    
                    GUILayout.Space(5);
                    _japaneseCheckConfig.Value = GUILayout.Toggle(_japaneseCheckConfig.Value, "检测合成语音文本是否为日文（当合成语音语言为 ja 时可防止发出怪声）", GUILayout.Height(elementHeight));
                    
                    GUILayout.Space(5);

                    GUILayout.Label($"语音音量：{_voiceVolumeConfig.Value:F2}");
                    
                    // 第一行：滑动条
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    float newVolume = GUILayout.HorizontalSlider(_voiceVolumeConfig.Value, 0.0f, 1.0f);
                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();

                    if (newVolume != _voiceVolumeConfig.Value)
                    {
                        _voiceVolumeConfig.Value = newVolume;
                        _audioSource.volume = newVolume;
                        _tempVolumeString = newVolume.ToString("F2");
                    }

                    // 第二行：输入框+按钮
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("手动输入：", GUILayout.Width(labelWidth), GUILayout.Height(elementHeight));

                    _tempVolumeString = GUILayout.TextField(_tempVolumeString, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f)); 
                    if (GUILayout.Button("应用", GUILayout.Width(btnWidth), GUILayout.Height(elementHeight)))
                    {
                        if (float.TryParse(_tempVolumeString, out float parsedVolume))
                        {
                            parsedVolume = Mathf.Clamp(parsedVolume, 0.0f, 1.0f);
                            _voiceVolumeConfig.Value = parsedVolume;
                            _audioSource.volume = parsedVolume;
                            _tempVolumeString = parsedVolume.ToString("F2");
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10);

                }
                
                GUILayout.EndVertical();

                GUILayout.Space(5);

                // --- 界面配置 Box ---
                GUILayout.BeginVertical("box", GUILayout.Width(innerBoxWidth));
                string interfaceBtnText = _showInterfaceSettings ? "🔽 界面配置" : "▶️ 界面配置";
                if (GUILayout.Button(interfaceBtnText, GUILayout.Height(elementHeight)))
                {
                    _showInterfaceSettings = !_showInterfaceSettings;
                }
                if (_showInterfaceSettings)
                {
                    // 宽度设置
                    GUILayout.Label($"当前宽度：{_windowWidthConfig.Value:F0}px");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("新宽度：", GUILayout.Width(labelWidth), GUILayout.Height(elementHeight));
                    
                    // 【核心修改】允许缩小
                    _tempWidthString = GUILayout.TextField(_tempWidthString, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));
                    
                    if (GUILayout.Button("应用", GUILayout.Width(btnWidth), GUILayout.Height(elementHeight)))
                    {
                        if (float.TryParse(_tempWidthString, out float newWidth) && newWidth >= 300f)
                        {
                            _windowWidthConfig.Value = newWidth;
                            // 这里删除了重置居中代码，只改大小
                            _tempWidthString = newWidth.ToString("F0");
                        }
                    }
                    GUILayout.EndHorizontal();

                    // 高度设置
                    GUILayout.Label($"当前基础高度: {_windowHeightConfig.Value:F0}px");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("新高度:", GUILayout.Width(labelWidth), GUILayout.Height(elementHeight));
                    
                    // 【核心修改】允许缩小
                    _tempHeightString = GUILayout.TextField(_tempHeightString, GUILayout.Height(elementHeight), GUILayout.MinWidth(50f));
                    
                    if (GUILayout.Button("应用", GUILayout.Width(btnWidth), GUILayout.Height(elementHeight)))
                    {
                        if (float.TryParse(_tempHeightString, out float newHeight) && newHeight >= 100f)
                        {
                            _windowHeightConfig.Value = newHeight;
                            _tempHeightString = newHeight.ToString("F0");
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    
                    // 窗口标题显示配置
                    _showWindowTitle.Value = GUILayout.Toggle(_showWindowTitle.Value, 
                        "显示窗口标题", GUILayout.Height(elementHeight));
                    GUILayout.Space(5);
                    
                    // 背景透明配置
                    GUILayout.Label($"背景透明度：{_backgroundOpacity.Value:F2}");
                    
                    // 滑动条
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    float newOpacity = GUILayout.HorizontalSlider(_backgroundOpacity.Value, 0.0f, 1.0f);
                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();
                    
                    if (newOpacity != _backgroundOpacity.Value)
                    {
                        _backgroundOpacity.Value = newOpacity;
                    }
                    
                    GUILayout.Space(5);

                    // 快捷键配置
                    _reverseEnterBehaviorConfig.Value = GUILayout.Toggle(_reverseEnterBehaviorConfig.Value, 
                        "反转回车键行为（勾选后：回车换行，Shift+回车发送）", GUILayout.Height(elementHeight));
                    GUILayout.Space(5);
                }
                
                GUILayout.EndVertical(); 
                GUILayout.Space(5);

                // --- 人设配置 Box ---
                GUILayout.BeginVertical("box", GUILayout.Width(innerBoxWidth));
                string personaBtnText = _showPersonaSettings ? "🔽 人设配置" : "▶️ 人设配置";
                if (GUILayout.Button(personaBtnText, GUILayout.Height(elementHeight)))
                {
                    _showPersonaSettings = !_showPersonaSettings;
                }
                
                if (_showPersonaSettings)
                {
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    _experimentalMemoryConfig.Value = GUILayout.Toggle(_experimentalMemoryConfig.Value, "启用记忆", GUILayout.Height(elementHeight));
                    if (GUILayout.Button("🗑️ 清除所有记忆", GUILayout.Width(btnWidth*3)))
                    {
                        _hierarchicalMemory?.ClearAllMemory();
                        Log.Info("记忆已清空");
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    GUILayout.Label("人设（系统提示词）：");
                    _personaScrollPosition = GUILayout.BeginScrollView(_personaScrollPosition, GUILayout.Height(elementHeight * 6));
                    _personaConfig.Value = GUILayout.TextArea(_personaConfig.Value, GUILayout.ExpandHeight(true));
                    GUILayout.EndScrollView();
                    GUILayout.Space(5);
                }
                
                GUILayout.EndVertical();

                GUILayout.Space(10);
                
                // 保存按钮
                if (GUILayout.Button("💾 保存所有配置", GUILayout.Height(elementHeight * 1.5f)))
                {
                    Config.Save();
                    Log.Info("配置已保存！");
                }
                GUILayout.Space(10);
            }
            // ================= 设置面板结束 =================

            // === 对话区域 ===
            GUILayout.Space(10);
            GUILayout.Label("<b>与聪音对话：</b>");

            GUI.backgroundColor = Color.white;

            // 动态计算输入框高度
            float dynamicInputHeight = _windowRect.height - (elementHeight * 3.5f);
            dynamicInputHeight = Mathf.Clamp(dynamicInputHeight, 50f, Screen.height * 0.8f);

            GUIStyle largeInputStyle = new GUIStyle(GUI.skin.textArea);
            largeInputStyle.fontSize = (int)(dynamicFontSize * 1.4f);
            largeInputStyle.wordWrap = true;
            largeInputStyle.alignment = TextAnchor.UpperLeft;

            GUI.skin.textArea.wordWrap = true;
            
            // 处理快捷键（回车和 Shift+回车）- 必须在 TextArea 之前处理
            Event keyEvent = Event.current;
            bool shouldSendMessage = false;
            
            if (keyEvent.type == EventType.KeyDown && 
                keyEvent.keyCode == KeyCode.Return && 
                !_isProcessing &&
                !string.IsNullOrEmpty(_playerInput))
            {
                // 检测是否按下 Shift 键
                bool shiftPressed = keyEvent.shift;
                
                // 根据配置决定是否应该发送
                // 默认模式（_reverseEnterBehaviorConfig = false）：Enter 发送，Shift+Enter 换行
                // 反转模式（_reverseEnterBehaviorConfig = true）：Enter 换行，Shift+Enter 发送
                shouldSendMessage = _reverseEnterBehaviorConfig.Value ? shiftPressed : !shiftPressed;
            }
            
            // 如果需要发送消息，在渲染 TextArea 之前拦截事件
            if (shouldSendMessage)
            {
                StartCoroutine(AIProcessRoutine(_playerInput));
                _playerInput = "";
                keyEvent.Use(); // 消费事件，防止 TextArea 处理
            }
            
            _playerInput = GUILayout.TextArea(_playerInput, largeInputStyle, GUILayout.Height(dynamicInputHeight));

            GUILayout.Space(5);
            GUI.backgroundColor = _isProcessing ? Color.gray : new Color(0.1725f, 0.1608f, 0.2784f);

            GUILayout.BeginHorizontal();

            // 1. 计算精确宽度
            // _windowRect.width - 50f 是我们之前定义的 innerBoxWidth (与设置框对齐)
            // 再减去 4f 是为了留出两个按钮中间的缝隙
            float totalWidth = _windowRect.width - 50f;
            float singleBtnWidth = (totalWidth - 4f) / 2f;

            // ================== 发送按钮 ==================
            // 使用 GUILayout.Width(singleBtnWidth) 强制固定宽度
            if (GUILayout.Button(_isProcessing ? "处理中" : "发送", GUILayout.Height(elementHeight * 1.5f), GUILayout.Width(singleBtnWidth)))
            {
                if (!string.IsNullOrEmpty(_playerInput) && !_isProcessing)
                {
                    StartCoroutine(AIProcessRoutine(_playerInput));
                    _playerInput = "";
                }
            }

            // ================== 录音按钮 ==================
            if (_isProcessing)
            {
                GUI.backgroundColor = Color.gray; 
            }
            else
            {
                GUI.backgroundColor = _isRecording ? Color.red : new Color(0.1725f, 0.1608f, 0.2784f);
            }
            string micBtnText;
            if (_isProcessing)
            {
                micBtnText = "⏳ 处理中";
            }
            else
            {
                micBtnText = _isRecording ? "🔴 松开结束" : "🎤 按住说话";
            }

            // 使用 GUILayout.Width(singleBtnWidth) 强制固定宽度
            Rect btnRect = GUILayoutUtility.GetRect(
                new GUIContent(micBtnText), 
                GUI.skin.button, 
                GUILayout.Height(elementHeight * 1.5f), 
                GUILayout.Width(singleBtnWidth) // <--- 强制宽度，不再依赖自动扩展
            );

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (btnRect.Contains(e.mousePosition) && !_isProcessing)
                    {
                        GUIUtility.hotControl = controlID; 
                        StartRecording();
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        StopRecordingAndRecognize();
                        e.Use();
                    }
                    break;
            }

            GUI.Box(btnRect, micBtnText, GUI.skin.button);

            GUILayout.EndHorizontal();

            // 结束整体布局
            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            // --- 拖拽手柄 ---
            const float handleSize = 25f;
            Rect handleRect = new Rect(_windowRect.width - handleSize, _windowRect.height - handleSize, handleSize, handleSize);
            GUI.Box(handleRect, "⇲", GUI.skin.GetStyle("Button"));

            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && handleRect.Contains(currentEvent.mousePosition))
            {
                if (currentEvent.button == 0)
                {
                    _isResizing = true;
                    currentEvent.Use();
                }
            }

            if (!_isResizing)
            {
                GUI.DragWindow();
            }
        }

        IEnumerator AIProcessRoutine(string prompt)
        {
            _isProcessing = true;

            // 1. 获取并处理 UI
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null) { _isProcessing = false; yield break; }
            Transform originalTextTrans = canvas.transform.Find("StorySystemUI/MessageWindow/NormalTextParent/NormalTextMessage");
            if (originalTextTrans == null) { _isProcessing = false; yield break; }
            GameObject originalTextObj = originalTextTrans.gameObject;
            GameObject parentObj = originalTextObj.transform.parent.gameObject;
            Dictionary<GameObject, bool> uiStatusMap = new Dictionary<GameObject, bool>();
            UIHelper.ForceShowWindow(originalTextObj, uiStatusMap);
            originalTextObj.SetActive(false);
            GameObject myTextObj = UIHelper.CreateOverlayText(parentObj);
            Text myText = myTextObj.GetComponent<Text>();
            myText.text = "";
            myText.color = Color.white;

            // Phase 3: 用角色思考动画代替 "Thinking..." 文字
            Log.Info("[AI] 开始思考...");
            if (GameBridge._heroineService != null && GameBridge._changeAnimSmoothMethod != null && GameBridge.IsHeroineStateSafe())
            {
                GameBridge.CallNativeChangeAnim(252); // Think animation
                GameBridge.ControlLookAt(1.0f, 0.5f);
            }

            // 2. 准备请求数据
            var requestContext = new LLMRequestContext
            {
                ApiUrl = _chatApiUrlConfig.Value,
                ApiKey = _apiKeyConfig.Value,
                ModelName = _modelConfig.Value,
                SystemPrompt = _personaConfig.Value,
                UserPrompt = prompt,
                UseLocalOllama = _useOllama.Value,
                LogApiRequestBody = _logApiRequestBodyConfig.Value,
                ThinkMode = _thinkModeConfig.Value,
                HierarchicalMemory = _experimentalMemoryConfig.Value ? _hierarchicalMemory : null,
                LogHeader = "AIChat",
                FixApiPathForThinkMode = _fixApiPathForThinkModeConfig.Value
            };

            string fullResponse = "";
            string errMsg = "";
            long errCode = 0;

            bool success = false;

            // 3. 发送 Chat 请求
            yield return LLMClient.SendLLMRequest(
                requestContext,
                rawResponse =>
                {
                    fullResponse = requestContext.UseLocalOllama
                        ? ResponseParser.ExtractContentFromOllama(rawResponse)
                        : ResponseParser.ExtractContentRegex(rawResponse);
                    success = true;
                },
                (errorMsg, responseCode) =>
                {
                    errCode = responseCode;
                    errMsg = $"API Error: {errorMsg}\nCode: {responseCode}";
                    success = false;
                }
            );

            // 恢复思考动画
            if (GameBridge._heroineService != null && GameBridge._changeAnimSmoothMethod != null)
            {
                GameBridge.CallNativeChangeAnim(250);
                GameBridge.RestoreLookAt();
            }

            if (!success)
            {
                if (errCode == 401) errMsg += "\n(请检查 API Key 是否正确)";
                if (errCode == 403) errMsg += "\n(请检查 API 余额或账户权限)";
                if (errCode == 404) errMsg += "\n(模型名称或 URL 错误)";
                if (errCode == 429) errMsg += "\n(请求过于频繁，请稍后重试)";

                myText.text = errMsg;
                myText.color = Color.red;

                yield return new WaitForSecondsRealtime(3.0f);
                UIHelper.RestoreUiStatus(uiStatusMap, myTextObj, originalTextObj);
                _isProcessing = false;
                yield break;
            }

            // 4. 处理回复并下载语音
            if (!string.IsNullOrEmpty(fullResponse))
            {
                LLMStandardResponse parsedResponse = LLMUtils.ParseStandardResponse(fullResponse);
                string actionTag = parsedResponse.EmotionTag;
                string voiceText = parsedResponse.VoiceText;
                string subtitleText = parsedResponse.SubtitleText;
                AddToMemorySystem("User", prompt);
                AddToMemorySystem("AI", parsedResponse.Success ? $"[{actionTag}] {voiceText}" : $"[格式错误] {fullResponse}");

                subtitleText = ResponseParser.InsertLineBreaks(subtitleText, 25);

                bool isJapanese = _japaneseCheckConfig.Value ? Regex.IsMatch(voiceText, @"[\u3040-\u309F\u30A0-\u30FF]") : true;
                Log.Info($"isJapanese: {isJapanese} (japaneseCheck: {_japaneseCheckConfig.Value})");

                if (!string.IsNullOrEmpty(voiceText) && isJapanese)
                {
                    // Phase 3 并行化：先显示字幕+动作，同时后台下载语音
                    myText.text = subtitleText;
                    myText.color = Color.white;

                    AudioClip downloadedClip = null;
                    bool ttsFinished = false;

                    // 启动 TTS 下载协程（后台）
                    StartCoroutine(TTSDownloadAsync(voiceText, (clip) =>
                    {
                        downloadedClip = clip;
                        ttsFinished = true;
                    }));

                    // 先做动作，等 TTS 完成后再播放语音
                    // 给 TTS 一点时间（最多等 8 秒），同时角色先做动作
                    if (GameBridge.IsHeroineStateSafe())
                    {
                        int animID;
                        if (!ActionAnimMap.TryGetValue(actionTag, out animID)) animID = 1001;
                        GameBridge.CallNativeChangeAnim(animID);
                    }

                    float ttsWaitStart = Time.realtimeSinceStartup;
                    const float maxTTSWait = 15f;
                    while (!ttsFinished && (Time.realtimeSinceStartup - ttsWaitStart) < maxTTSWait)
                    {
                        yield return null;
                    }

                    if (downloadedClip != null)
                    {
                        if (!downloadedClip.LoadAudioData()) yield return null;
                        yield return null;

                        _isAISpeaking = true;
                        _audioSource.clip = downloadedClip;
                        _audioSource.Play();

                        yield return new WaitForSecondsRealtime(downloadedClip.length + 0.5f);

                        if (_audioSource != null && _audioSource.isPlaying)
                        {
                            _audioSource.Stop();
                        }
                        _isAISpeaking = false;
                    }
                    else
                    {
                        Log.Warning("[TTS] 语音下载失败或超时，仅显示字幕");
                        yield return new WaitForSecondsRealtime(3.0f);
                    }

                    GameBridge.CallNativeChangeAnim(250);
                    GameBridge.RestoreLookAt();
                }
                else
                {
                    Log.Warning("跳过 TTS：文本为空或非日语");
                    myText.text = subtitleText;
                    myText.color = Color.white;
                    yield return StartCoroutine(PlayNativeAnimation(actionTag, null));
                }
            }

            // 5. 清理
            UIHelper.RestoreUiStatus(uiStatusMap, myTextObj, originalTextObj);
            _isProcessing = false;
        }

        IEnumerator TTSDownloadAsync(string voiceText, Action<AudioClip> onComplete)
        {
            AudioClip downloadedClip = null;
            yield return StartCoroutine(TTSClient.DownloadVoiceWithRetry(
                _sovitsUrlConfig.Value + "/tts",
                voiceText,
                _targetLangConfig.Value,
                _refAudioPathConfig.Value,
                _promptTextConfig.Value,
                _promptLangConfig.Value,
                Logger,
                (clip) => downloadedClip = clip,
                3,
                30f,
                _audioPathCheckConfig.Value));
            onComplete?.Invoke(downloadedClip);
        }

        IEnumerator TTSHealthCheckLoop()
        {
            while (!_isTTSServiceReady)
            {
                yield return StartCoroutine(TTSClient.CheckTTSHealthOnce(_sovitsUrlConfig.Value,Logger,(ready) =>
                {
                    _isTTSServiceReady = ready;
                }));
                yield return new WaitForSeconds(TTSHealthCheckInterval);
            }
        }

        private static readonly Dictionary<string, int> ActionAnimMap = new Dictionary<string, int>
        {
            // Story expressions
            { "Joy",           1001 },
            { "Sad",           1002 },
            { "Fun",           1003 },
            { "Guts",          1004 },
            { "Agree",         1301 },
            { "Frustration",   1302 },
            { "LookDown",      1401 },
            // Work/desk actions
            { "Think",          252 },
            { "DrinkTea",       256 },
            // Base gestures
            { "Nod",             12 },
            { "ShakeHead",       13 },
            { "Shy",              5 },
            { "Jump",             6 },
            { "Confidence",       7 },
            { "DropShoulders",    4 },
            { "TouchGlasses",    24 },
            { "Good",            61 },
            { "Tired",           64 },
            { "Stretch",         50 },
            { "Yawn",            72 },
            // Special
            { "Wave",          5001 },
            { "LeanForward",   5002 },
        };

        IEnumerator PlayNativeAnimation(string actionTag, AudioClip voiceClip)
        {
            if (GameBridge._heroineService == null || GameBridge._changeAnimSmoothMethod == null) yield break;

            if (!GameBridge.IsHeroineStateSafe())
            {
                Log.Warning("[动画] 游戏状态机正忙，跳过 mod 动画");
                if (voiceClip != null)
                {
                    _isAISpeaking = true;
                    _audioSource.clip = voiceClip;
                    _audioSource.Play();
                    yield return new WaitForSecondsRealtime(voiceClip.length + 0.5f);
                    _isAISpeaking = false;
                }
                yield break;
            }

            Log.Info($"[动画] Action={actionTag}, hasVoice={voiceClip != null}");
            float clipDuration = (voiceClip != null) ? voiceClip.length : 3.0f;

            bool needsSpecialReset = (actionTag == "DrinkTea");
            if (!needsSpecialReset)
            {
                GameBridge.CallNativeChangeAnim(250);
                yield return new WaitForSecondsRealtime(0.2f);
            }
            else
            {
                GameBridge.CallNativeChangeAnim(250);
                yield return new WaitForSecondsRealtime(0.5f);
            }

            if (voiceClip != null)
            {
                _isAISpeaking = true;
                _audioSource.clip = voiceClip;
                _audioSource.Play();
            }

            int animID;
            if (!ActionAnimMap.TryGetValue(actionTag, out animID))
            {
                Log.Warning($"[动画] 未知标签 '{actionTag}'，回退到 Joy");
                animID = 1001;
            }

            bool needsLookAt = (actionTag == "Wave" || actionTag == "LeanForward"
                || actionTag == "Joy" || actionTag == "Agree" || actionTag == "Nod");

            GameBridge.CallNativeChangeAnim(animID);

            if (needsLookAt)
            {
                yield return new WaitForSecondsRealtime(0.3f);
                GameBridge.ControlLookAt(1.0f, 0.5f);
            }

            float waitTime = Mathf.Max(clipDuration + 0.5f, 2.5f);
            yield return new WaitForSecondsRealtime(waitTime);

            if (_audioSource != null && _audioSource.isPlaying)
            {
                Log.Warning("等待结束，强制停止语音播放");
                _audioSource.Stop();
            }
            GameBridge.CallNativeChangeAnim(250);
            GameBridge.RestoreLookAt();
            _isAISpeaking = false;
        }

        // ================= 【新增录音控制】 =================
        void StartRecording()
        {
            Log.Info($"[Mic Debug] 检测到设备数量: {Microphone.devices.Length}");
            if (Microphone.devices.Length > 0)
            {
                foreach (var d in Microphone.devices)
                {
                    Log.Info($"[Mic Debug] 可用设备: {d}");
                }
            }
            // --------------------

            if (Microphone.devices.Length == 0)
            {
                Log.Error("未检测到麦克风！(Microphone.devices is empty)");
                // 可以在屏幕上显示个错误提示
                _playerInput = "[Error: No Mic Found]"; 
                return;
            }

            _microphoneDevice = Microphone.devices[0];
            _recordingClip = Microphone.Start(_microphoneDevice, false, MaxRecordingSeconds, RecordingFrequency);
            _isRecording = true;
            Log.Info($"开始录音: {_microphoneDevice}");
        }

        void StopRecordingAndRecognize()
        {
            if (!_isRecording) return;

            // 1. 停止录音
            int position = Microphone.GetPosition(_microphoneDevice);
            Microphone.End(_microphoneDevice);
            _isRecording = false;
            Log.Info($"停止录音，采样点: {position}");

            // 2. 剪裁有效音频 (去掉末尾的静音/空白部分)
            if (position <= 0) return; // 录音太短

            AudioClip validClip = AudioUtils.TrimAudioClip(_recordingClip, position);

            // 3. 编码并发送
            byte[] wavData = AudioUtils.EncodeToWAV(validClip);
            StartCoroutine(ASRWorkflow(wavData));
        }
        /// <summary>
        /// ASR 业务流：负责调度网络请求和后续的 AI 响应
        /// </summary>
        IEnumerator ASRWorkflow(byte[] wavData)
        {
            _isProcessing = true; // 锁定 UI
            string recognizedResult = "";

            // A. 调用 ApiService 只负责拿回文字
            yield return StartCoroutine(ASRClient.SendAudioToASR(
                wavData,
                _sovitsUrlConfig.Value,
                (text) => recognizedResult = text
            ));

            // B. 根据拿回的结果，在主类决定下一步业务走向
            if (!string.IsNullOrEmpty(recognizedResult))
            {
                Log.Info($"[Workflow] ASR 成功，开始进入 AI 思考流程: {recognizedResult}");

                // 这里触发 AI 处理流程
                yield return StartCoroutine(AIProcessRoutine(recognizedResult));
            }
            else
            {
                Log.Warning("[Workflow] ASR 未能识别到有效文本");
                _isProcessing = false; // 如果识别失败，在这里解锁 UI
            }
        }
        void OnApplicationQuit()
        {
            Log.Info("[Chill AI Mod] 退出中...");
            
            // 【保存记忆系统】
            if (_hierarchicalMemory != null && _experimentalMemoryConfig.Value)
            {
                Log.Info("[HierarchicalMemory] 正在保存记忆...");
                _hierarchicalMemory.SaveToFile();
            }
            
            Log.Info("[Chill AI Mod] 正在停止TTS轮询...");
            if (_ttsHealthCheckCoroutine != null)
            {
                StopCoroutine(_ttsHealthCheckCoroutine);
                _ttsHealthCheckCoroutine = null;
            }
            if (_quitTTSServiceOnQuitConfig.Value && _launchedTTSProcess != null && !_launchedTTSProcess.HasExited)
            {   
                try
                {
                    ProcessHelper.KillProcessTree(_launchedTTSProcess);
                    Log.Info("TTS 服务已关闭");
                }
                catch (Exception ex)
                {
                    Log.Warning($"关闭 TTS 服务时出错: {ex.Message}");
                }
            }
        }
        
        // ================= 【分层记忆系统相关方法】 =================

        /// <summary>
        /// 初始化分层记忆系统
        /// </summary>
        private void InitializeHierarchicalMemory()
        {
            Func<string, Task<string>> llmSummarizer = async (prompt) => await CallLlmForSummaryAsync(prompt);
            string memoryFilePath = Path.Combine(BepInEx.Paths.ConfigPath, "ChillAIMod", "memory.txt");

            _hierarchicalMemory = new HierarchicalMemory(
                llmSummarizer, 3, 10, 6, 5, memoryFilePath
            );
        }

        /// <summary>
        /// 调用 LLM 进行文本总结（将协程包装为 Task）
        /// </summary>
        private async Task<string> CallLlmForSummaryAsync(string prompt)
        {
            var tcs = new TaskCompletionSource<string>();

            // 使用协程调用 LLM
            StartCoroutine(CallLlmForSummaryCoroutine(prompt, (result) =>
            {
                tcs.SetResult(result);
            }));

            return await tcs.Task;
        }

        /// <summary>
        /// 协程：调用 LLM 进行文本总结
        /// </summary>
        private IEnumerator CallLlmForSummaryCoroutine(string prompt, Action<string> onComplete)
        {
            Log.Info("[HierarchicalMemory] >>> 开始调用 LLM 进行总结...");

            var requestContext = new LLMRequestContext
            {
                ApiUrl = _chatApiUrlConfig.Value,
                ApiKey = _apiKeyConfig.Value,
                ModelName = _modelConfig.Value,
                SystemPrompt = "你是一个专业的文本总结助手。",
                UserPrompt = prompt,
                UseLocalOllama = _useOllama.Value,
                LogApiRequestBody = _logApiRequestBodyConfig.Value,
                ThinkMode = _thinkModeConfig.Value,
                HierarchicalMemory = null,
                LogHeader = "HierarchicalMemory",
                FixApiPathForThinkMode = _fixApiPathForThinkModeConfig.Value
            };

            yield return LLMClient.SendLLMRequest(
                requestContext,
                rawResponse => 
                {
                    string summary = requestContext.UseLocalOllama
                        ? ResponseParser.ExtractContentFromOllama(rawResponse)
                        : ResponseParser.ExtractContentRegex(rawResponse);
                    onComplete?.Invoke(summary);
                },
                (errorMsg, responseCode) => 
                {
                    onComplete?.Invoke("[总结失败]");
                }
            );

            Log.Info("[HierarchicalMemory] <<< 总结调用完成");
        }

        /// <summary>
        /// 将对话添加到记忆系统中（如果启用）
        /// 注意：已改为后台异步处理，不阻塞主流程
        /// </summary>
        private void AddToMemorySystem(string role, string content)
        {
            if (_hierarchicalMemory != null && _experimentalMemoryConfig.Value)
            {
                _hierarchicalMemory.AddMessage($"{role}: {content}");
            }
        }

        private void AddAIChatButtonToRightIcons()
        {
            try
            {
                // Strategy: find the container by locating known game buttons first
                Transform buttonContainer = null;
                Transform referenceButton = null; // the lowest existing button (日记/Calendar)

                // Step 1: Try known paths
                string[] candidatePaths = new string[]
                {
                    "Paremt/Canvas/UI/MostFrontArea/TopIcons",
                    "Parent/Canvas/UI/MostFrontArea/TopIcons",
                    "Paremt/Canvas/UI/MostFrontArea/RightIcons",
                };
                foreach (string path in candidatePaths)
                {
                    GameObject found = GameObject.Find(path);
                    if (found != null)
                    {
                        buttonContainer = found.transform;
                        Log.Info($"[UI] 路径命中: {path}");
                        break;
                    }
                }

                // Step 2: find by known button names (game's right side buttons)
                if (buttonContainer == null)
                {
                    string[] knownButtonNames = new string[]
                    {
                        "IconCalendar_Button", "IconDiary_Button",
                        "IconNote_Button", "IconTodo_Button",
                        "IconHabitTracker_Button", "IconHabit_Button",
                    };
                    foreach (string btnName in knownButtonNames)
                    {
                        GameObject btn = FindGameObjectByNameAnywhere(btnName);
                        if (btn != null && btn.transform.parent != null)
                        {
                            buttonContainer = btn.transform.parent;
                            Log.Info($"[UI] 通过按钮 '{btnName}' 找到容器: {GetFullPath(buttonContainer)}");
                            break;
                        }
                    }
                }

                // Step 3: brute-force search - find any Button whose name contains known keywords
                if (buttonContainer == null)
                {
                    var allButtons = UnityEngine.Object.FindObjectsOfType<Button>();
                    foreach (var btn in allButtons)
                    {
                        string n = btn.gameObject.name.ToLower();
                        if (n.Contains("calendar") || n.Contains("diary") ||
                            n.Contains("note") || n.Contains("todo") ||
                            n.Contains("habit"))
                        {
                            if (btn.transform.parent != null)
                            {
                                buttonContainer = btn.transform.parent;
                                Log.Info($"[UI] 通过 Button 组件 '{btn.gameObject.name}' 找到容器: {GetFullPath(buttonContainer)}");
                                break;
                            }
                        }
                    }
                }

                // Step 4: last resort - search all transforms for container-like names
                if (buttonContainer == null)
                {
                    string[] containerNames = new string[]
                    {
                        "TopIcons", "RightIcons", "IconButtons", "RightButtons", "SideIcons"
                    };
                    foreach (string name in containerNames)
                    {
                        GameObject found = FindGameObjectByNameAnywhere(name);
                        if (found != null)
                        {
                            buttonContainer = found.transform;
                            Log.Info($"[UI] 名称搜索命中: {name}");
                            break;
                        }
                    }
                }

                if (buttonContainer == null)
                {
                    Log.Warning("[UI] 所有搜索策略均失败，输出 UI 树...");
                    DumpUITree(5);
                    return;
                }

                // Find the lowest existing button (by Y position) to place AI button below it
                float lowestY = float.MaxValue;
                float buttonSize = 60f;
                float refX = 0f;
                RectTransform lowestRect = null;

                for (int i = 0; i < buttonContainer.childCount; i++)
                {
                    RectTransform childRect = buttonContainer.GetChild(i).GetComponent<RectTransform>();
                    if (childRect == null) continue;
                    if (buttonSize <= 0)
                        buttonSize = Mathf.Max(childRect.sizeDelta.x, childRect.sizeDelta.y);
                    if (childRect.anchoredPosition.y < lowestY)
                    {
                        lowestY = childRect.anchoredPosition.y;
                        lowestRect = childRect;
                        refX = childRect.anchoredPosition.x;
                    }
                }
                if (buttonSize <= 0) buttonSize = 60f;

                // Log all children for debugging
                for (int i = 0; i < buttonContainer.childCount; i++)
                {
                    var c = buttonContainer.GetChild(i);
                    var r = c.GetComponent<RectTransform>();
                    string pos = r != null ? $"({r.anchoredPosition.x:F0},{r.anchoredPosition.y:F0})" : "no-rect";
                    Log.Info($"[UI] 容器子节点[{i}]: {c.name} pos={pos}");
                }

                _aiChatButton = new GameObject("IconAIChat_Button");
                _aiChatButton.transform.SetParent(buttonContainer, false);
                RectTransform rectTransform = _aiChatButton.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(buttonSize, buttonSize);

                if (lowestRect != null)
                {
                    rectTransform.anchorMin = lowestRect.anchorMin;
                    rectTransform.anchorMax = lowestRect.anchorMax;
                    rectTransform.pivot = lowestRect.pivot;
                    float spacing = 10f;
                    rectTransform.anchoredPosition = new Vector2(
                        refX,
                        lowestY - (buttonSize + spacing)
                    );
                }
                else
                {
                    rectTransform.anchorMin = new Vector2(1f, 1f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = Vector2.zero;
                }

                Image image = _aiChatButton.AddComponent<Image>();
                try
                {
                    image.sprite = EmbeddedSpriteLoader.Load("ai_chat.png");
                    image.color = Color.white;
                    image.preserveAspect = true;
                }
                catch (Exception ex)
                {
                    Log.Error($"加载内置图片失败: {ex}");
                    image.color = new Color(0.6f, 0.4f, 1f, 1f);
                }

                Button button = _aiChatButton.AddComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    _showInputWindow = !_showInputWindow;
                });

                _aiChatButtonAdded = true;
                Log.Info($"[UI] AI 按钮已添加到 '{buttonContainer.name}'，位置=({rectTransform.anchoredPosition.x:F0},{rectTransform.anchoredPosition.y:F0})");
            }
            catch (Exception ex)
            {
                Log.Error($"添加AI聊天按钮失败: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static GameObject FindGameObjectByNameAnywhere(string name)
        {
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Transform found = FindChildRecursive(root.transform, name);
                if (found != null) return found.gameObject;
            }
            return null;
        }

        private static Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static string GetFullPath(Transform t)
        {
            string path = t.name;
            Transform p = t.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }

        private static void DumpUITree(int maxDepth = 4)
        {
            Log.Info("[UITree] === 开始 UI 树转储 ===");
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.GetComponentInChildren<Canvas>() != null)
                    DumpTransform(root.transform, 0, maxDepth);
            }
            Log.Info("[UITree] === UI 树转储结束 ===");
        }

        private static void DumpTransform(Transform t, int depth, int maxDepth)
        {
            if (depth > maxDepth) return;
            string indent = new string(' ', depth * 2);
            var rect = t.GetComponent<RectTransform>();
            string extra = rect != null ? $" pos=({rect.anchoredPosition.x:F0},{rect.anchoredPosition.y:F0})" : "";
            bool hasButton = t.GetComponent<Button>() != null;
            string btnMark = hasButton ? " [BTN]" : "";
            Log.Info($"[UITree] {indent}{t.name}{btnMark}{extra} (active={t.gameObject.activeSelf})");
            for (int i = 0; i < t.childCount; i++)
                DumpTransform(t.GetChild(i), depth + 1, maxDepth);
        }
    }
}
