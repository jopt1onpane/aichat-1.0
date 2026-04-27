using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using FastEnumUtility;
using NestopiSystem;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class SettingUI : MonoBehaviour, ISettingUI
{
	private Color32 ActiveColor = Color.white;

	private Color32 DeactiveColor = new Color32(110, 110, 147, byte.MaxValue);

	private float DisabledAlpha = 0.3f;

	[Inject]
	private SettingService _settingService;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private ChangeOrderService _changeOrderService;

	[Inject]
	private IOnClickButtonOpenTutorialProvider _onClickButtonOpenTutorialProvider;

	[Inject]
	private AppAuth _appAuth;

	[Inject]
	private IUICanvasProvider _uiCanvasProvider;

	[Inject]
	private WebApiErrorBehavior _errorBehaviour;

	[Inject]
	private LoadingScreen _loadingScreen;

	[Inject]
	private WebApiGate _webApiGate;

	[Inject]
	private LanguageSupplier _languageSupplier;

	[SerializeField]
	[Header("機能を開くボタン")]
	private InteractableUI _facilityOpenButton;

	[Header("設定種類選択")]
	[SerializeField]
	[Header("全般の親")]
	private GameObject _generalParent;

	[SerializeField]
	[Header("グラフィックの親")]
	private GameObject _graphicParent;

	[SerializeField]
	[Header("オーディオの親")]
	private GameObject _audioParent;

	[SerializeField]
	[Header("アカウントの親")]
	private GameObject _accountParent;

	[SerializeField]
	[Header("お知らせの親")]
	private GameObject _newsParent;

	[SerializeField]
	[Header("クレジットの親")]
	private GameObject _creditsParent;

	[SerializeField]
	[Header("全般のインタラクトUI")]
	private InteractableUI _generalInteractableUI;

	[SerializeField]
	private TextMeshProUGUI _generalText;

	[SerializeField]
	[Header("グラフィックのインタラクトUI")]
	private InteractableUI _graphicInteractableUI;

	[SerializeField]
	private TextMeshProUGUI _graphicText;

	[SerializeField]
	[Header("オーディオのインタラクトUI")]
	private InteractableUI _audioInteractableUI;

	[SerializeField]
	private TextMeshProUGUI _audioText;

	[SerializeField]
	[Header("アカウントのインタラクトUI")]
	private InteractableUI _accountInteractableUI;

	[SerializeField]
	private TextMeshProUGUI _accountText;

	[SerializeField]
	[Header("お知らせのインタラクトUI")]
	private InteractableUI _newsInteractableUI;

	[SerializeField]
	private TextMeshProUGUI _newsText;

	[SerializeField]
	[Header("クレジットのインタラクトUI")]
	private InteractableUI _creditsInteractableUI;

	[SerializeField]
	[Header("お問い合わせIDボタンのUIグループ")]
	private GameObject _inquiryIDGroup;

	[SerializeField]
	private TextMeshProUGUI _creditsText;

	[SerializeField]
	private TextMeshProUGUI _appVerText;

	[Space(1f)]
	[Header("全般")]
	[Header("言語")]
	[SerializeField]
	[Header("言語\u3000プルダウンリスト")]
	private PulldownListUI _languagePulldownList;

	[SerializeField]
	[Header("言語\u3000日本語\u3000インタラクトUI")]
	private InteractableUI _languageJPInteractableUI;

	[SerializeField]
	[Header("言語\u3000英語\u3000インタラクトUI")]
	private InteractableUI _languageENInteractableUI;

	[SerializeField]
	[Header("言語\u3000簡体字\u3000インタラクトUI")]
	private InteractableUI _languageSPInteractableUI;

	[SerializeField]
	[Header("言語\u3000繁体字\u3000インタラクトUI")]
	private InteractableUI _languageTCInteractableUI;

	[SerializeField]
	[Header("言語\u3000ポルトガル語\u3000インタラクトUI")]
	private InteractableUI _languagePTInteractableUI;

	[SerializeField]
	[Header("言語\u3000韓国語\u3000インタラクトUI")]
	private InteractableUI _languageKOInteractableUI;

	[SerializeField]
	[Header("言語\u3000ロシア語\u3000インタラクトUI")]
	private InteractableUI _languageRUInteractableUI;

	[Header("時間形式")]
	[SerializeField]
	[Header("時間形式\u300012AM\u3000インタラクトUI")]
	private InteractableUI _timeFormatAMPMInteractableUI;

	[SerializeField]
	[Header("時間形式\u300024\u3000インタラクトUI")]
	private InteractableUI _timeFormatAllInteractableUI;

	[Header("最前面表示")]
	[SerializeField]
	[Header("最前面表示\u3000On インタラクトUI")]
	private InteractableUI _windowTpAlwaysActivateInteractableUI;

	[SerializeField]
	[Header("最前面表示\u3000Off インタラクトUI")]
	private InteractableUI _windowTpAlwaysDeactivateInteractableUI;

	[Header("旧正月オン/オフ")]
	[SerializeField]
	[Header("旧正月表示の親")]
	private RectTransform _lunaNewYear2026Parent;

	[SerializeField]
	[Header("旧正月\u3000On インタラクトUI")]
	private InteractableUI _lunaNewYear2026ActivateInteractableUI;

	[SerializeField]
	[Header("旧正月\u3000Off インタラクトUI")]
	private InteractableUI _lunaNewYear2026DeactivateInteractableUI;

	[Header("セーブデータアップロード間隔")]
	[SerializeField]
	private GameObject saveDataSyncIntervalObj;

	[SerializeField]
	private PulldownListUI saveDataSyncIntervalPulldown;

	[SerializeField]
	private InteractableUI saveDataSyncInterval30Sec;

	[SerializeField]
	private InteractableUI saveDataSyncInterval60Sec;

	[SerializeField]
	private InteractableUI saveDataSyncInterval90Sec;

	[SerializeField]
	private TextLocalizationBehaviour currentSaveDataSyncIntervalText;

	[SerializeField]
	[Header("全般初期化ボタン")]
	private SettingInitButton _generalInitButton;

	[Space(1f)]
	[Header("グラフィック")]
	[Header("WindowMode")]
	[SerializeField]
	[Header("WindowMode\u3000ボーダーレスフルスクリーン\u3000インタラクトUI")]
	private InteractableUI _borderlessFullscreenInteractableUI;

	[SerializeField]
	[Header("WindowMode\u3000ウィンドウ\u3000インタラクトUI")]
	private InteractableUI _windowedInteractableUI;

	[Header("解像度")]
	[SerializeField]
	[Header("解像度\u3000プルダウンリスト")]
	private PulldownListUI _windowResolutionPulldownList;

	[SerializeField]
	private CanvasGroup _windowResolutionCanvasGroup;

	[SerializeField]
	[Header("解像度\u30001280*720\u3000インタラクトUI")]
	private InteractableUI _windowResolutionFirstInteractableUI;

	[SerializeField]
	[Header("解像度\u30001600*900\u3000インタラクトUI")]
	private InteractableUI _windowResolutionSecondInteractableUI;

	[SerializeField]
	[Header("解像度\u30001920*1080\u3000インタラクトUI")]
	private InteractableUI _windowResolutionThirdInteractableUI;

	[Header("垂直同期")]
	[SerializeField]
	[Header("垂直同期\u3000On インタラクトUI")]
	private InteractableUI _verticalSyncActivateInteractableUI;

	[SerializeField]
	[Header("垂直同期\u3000Off インタラクトUI")]
	private InteractableUI _verticalSyncDeactivateInteractableUI;

	[Header("フレームレート")]
	[SerializeField]
	[Header("アクティブ時 インプットフィールド")]
	private ClampIntegerInputField _activeFrameRateInputField;

	[SerializeField]
	[Header("非アクティブ時 インプットフィールド")]
	private ClampIntegerInputField _deactiveFrameRateInputField;

	[Header("描画品質")]
	[SerializeField]
	[Header("描画品質\u3000プルダウンリスト")]
	private PulldownListUI _graphicQualityPulldownList;

	[SerializeField]
	[Header("描画品質\u3000高\u3000インタラクトUI")]
	private InteractableUI _graphicQualityHighInteractableUI;

	[SerializeField]
	[Header("描画品質\u3000中\u3000インタラクトUI")]
	private InteractableUI _graphicQualityMediumInteractableUI;

	[SerializeField]
	[Header("描画品質\u3000低\u3000インタラクトUI")]
	private InteractableUI _graphicQualityLowInteractableUI;

	[Header("レンダースケール")]
	[SerializeField]
	[Header("レンダースケール")]
	private Slider _renderScaleSlider;

	[SerializeField]
	[Header("グラフィック初期化ボタン")]
	private SettingInitButton _graphicInitButton;

	[Space(1f)]
	[Header("オーディオ")]
	[SerializeField]
	[Header("Master")]
	private SettingVolumeChangeUI _masterVolumeChangeUI;

	[SerializeField]
	[Header("SystemSE")]
	private SettingVolumeChangeUI _systemSEVolumeChangeUI;

	[SerializeField]
	[Header("Voice")]
	private SettingVolumeChangeUI _voiceVolumeChangeUI;

	[SerializeField]
	[Header("環境音BGM")]
	private SettingVolumeChangeUI _ambientBGMVolumeChangeUI;

	[SerializeField]
	[Header("環境音SE")]
	private SettingVolumeChangeUI _ambientSEVolumeChangeUI;

	[SerializeField]
	[Header("ポモドーロを鳴らすか\u3000On インタラクトUI")]
	private InteractableUI _isPlayPomodoroActivateInteractableUI;

	[SerializeField]
	[Header("ポモドーロを鳴らすか\u3000Off インタラクトUI")]
	private InteractableUI _isPlayPomodoroDeactivateInteractableUI;

	[SerializeField]
	[Header("独り言を再生するか\u3000On インタラクトUI")]
	private InteractableUI _isPlaySelfTalkActivateInteractableUI;

	[SerializeField]
	[Header("独り言を再生するか\u3000Off インタラクトUI")]
	private InteractableUI _isPlaySelfTalkDeactivateInteractableUI;

	[SerializeField]
	[Header("オーディオ初期化ボタン")]
	private SettingInitButton _audioInitButton;

	[SerializeField]
	[Header("フェード用CanvasGroup")]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	[Header("クレジットのスクロールバー")]
	private ScrollRect _creditScrollRect;

	[SerializeField]
	[Header("閉じるボタン")]
	private Button _closeButton;

	[SerializeField]
	[Header("ヘルプボタン")]
	private Button _tutorialHelpButton;

	[SerializeField]
	[Header("FAQボタン")]
	private Button _faqButton;

	[SerializeField]
	[Header("お問い合わせIDボタン")]
	private Button _inquiryIDButton;

	[Space(1f)]
	[Header("アカウント")]
	[SerializeField]
	[Header("Apple連携ボタン")]
	private AuthButton _appleButton;

	[SerializeField]
	[Header("Google連携ボタン")]
	private AuthButton _googleButton;

	[SerializeField]
	[Header("利用規約ボタン")]
	private Button _termsButton;

	[SerializeField]
	[Header("プライバシーポリシーボタン")]
	private Button _privacyPolicyButton;

	[SerializeField]
	private SettingNewsView _newsView;

	private GameObject _currentSelectSettingObject;

	private InteractableUI _currentSelectSettingInteractableUI;

	private SettingVolumeChangeUI[] _volumeChangeUIArray;

	private RectTransform _rectTransform;

	private Tween _moveTween;

	private Tween _fadeTween;

	private float _fromPosY;

	private float _toPosY;

	private bool _isActive;

	private Tween _windowResolutionDisableTween;

	private NewsData[] _cachedNews;

	private GameLanguageType _cachedNewsLanguageType;

	[field: SerializeField]
	[field: Header("アカウント切り替えダイアログ")]
	public AccountTransferDialog AccountTransferDialog { get; private set; }

	public void Setup()
	{
		_appVerText.SetText("Ver" + Application.version);
		ObservableSubscribeExtensions.Subscribe(_closeButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayCancel();
			Deactivate();
		}).AddTo(this);
		_generalInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_generalInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.SelectSettingType(SettingType.General);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_graphicInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_graphicInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.SelectSettingType(SettingType.Graphic);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_audioInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_audioInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.SelectSettingType(SettingType.Audio);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_accountInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_accountInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.SelectSettingType(SettingType.Account);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_newsInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_newsInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.SelectSettingType(SettingType.News);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_creditsInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_creditsInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.SelectSettingType(SettingType.Credits);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_languagePulldownList.Setup();
		saveDataSyncIntervalPulldown.Setup();
		_windowResolutionPulldownList.Setup();
		_graphicQualityPulldownList.Setup();
		_generalText.color = DeactiveColor;
		_graphicText.color = DeactiveColor;
		_audioText.color = DeactiveColor;
		_creditsText.color = DeactiveColor;
		_settingService.SettingType.Subscribe(delegate(SettingType settingType)
		{
			if (_currentSelectSettingObject != null)
			{
				_currentSelectSettingObject.SetActive(value: false);
				_currentSelectSettingInteractableUI.DeactivateUseUI();
			}
			switch (settingType)
			{
			case SettingType.General:
				_currentSelectSettingObject = _generalParent;
				_currentSelectSettingInteractableUI = _generalInteractableUI;
				OnOpenGeneralTab();
				break;
			case SettingType.Graphic:
				_currentSelectSettingObject = _graphicParent;
				_currentSelectSettingInteractableUI = _graphicInteractableUI;
				OnOpenGraphicTab();
				break;
			case SettingType.Audio:
				_currentSelectSettingObject = _audioParent;
				_currentSelectSettingInteractableUI = _audioInteractableUI;
				OnOpenAudioTab();
				break;
			case SettingType.Account:
				_currentSelectSettingObject = _accountParent;
				_currentSelectSettingInteractableUI = _accountInteractableUI;
				OnOpenAccountTab();
				break;
			case SettingType.News:
				_currentSelectSettingObject = _newsParent;
				_currentSelectSettingInteractableUI = _newsInteractableUI;
				OnOpenNewsTab();
				break;
			case SettingType.Credits:
				_currentSelectSettingObject = _creditsParent;
				_currentSelectSettingInteractableUI = _creditsInteractableUI;
				OnOpenCreditTab();
				break;
			}
			_currentSelectSettingObject.SetActive(value: true);
			if (_currentSelectSettingObject == _creditsParent)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(_creditScrollRect.content);
				_creditScrollRect.verticalNormalizedPosition = 1f;
			}
			_currentSelectSettingInteractableUI.ActivateUseUI();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialHelpButton.OnClickAsObservable(), delegate
		{
			_onClickButtonOpenTutorialProvider.OnClickButtonOpenTutorial();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_faqButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayClick();
			OpenURLFunctions.OpenFAQ(_languageSupplier);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_inquiryIDButton.OnClickAsObservable(), async delegate
		{
			if (!SaveDataManager.Instance.AccountData.DeviceID.IsNullOrEmpty())
			{
				_systemSeService.PlayClick();
				InquiryIDDialog.Create(_uiCanvasProvider.CommonDialogParent.transform);
			}
		}).AddTo(this);
		_languageJPInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_languageJPInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGameLanguage(GameLanguageType.Japanese);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_languageENInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_languageENInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGameLanguage(GameLanguageType.English);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_languageSPInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_languageSPInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGameLanguage(GameLanguageType.ChineseSimplified);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_languageTCInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_languageTCInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGameLanguage(GameLanguageType.ChineseTraditional);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_languagePTInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_languagePTInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGameLanguage(GameLanguageType.Portuguese);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_languageKOInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_languageKOInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGameLanguage(GameLanguageType.Korean);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_languageRUInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_languageRUInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGameLanguage(GameLanguageType.Russian);
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.GameLanguage.Subscribe(delegate(GameLanguageType languageType)
		{
			ChangeLanguageView(languageType);
		}).AddTo(this);
		saveDataSyncInterval30Sec.Setup();
		ObservableSubscribeExtensions.Subscribe(saveDataSyncInterval30Sec.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeSaveDataSyncInterval(SaveDataSyncInterval.Sec30);
			saveDataSyncIntervalPulldown.ClosePullDown();
			_systemSeService.PlayClick();
		}).AddTo(this);
		saveDataSyncInterval60Sec.Setup();
		ObservableSubscribeExtensions.Subscribe(saveDataSyncInterval60Sec.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeSaveDataSyncInterval(SaveDataSyncInterval.Sec60);
			saveDataSyncIntervalPulldown.ClosePullDown();
			_systemSeService.PlayClick();
		}).AddTo(this);
		saveDataSyncInterval90Sec.Setup();
		ObservableSubscribeExtensions.Subscribe(saveDataSyncInterval90Sec.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeSaveDataSyncInterval(SaveDataSyncInterval.Sec90);
			saveDataSyncIntervalPulldown.ClosePullDown();
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.SaveDataSyncInterval.Subscribe(delegate(SaveDataSyncInterval interval)
		{
			ChangeSaveDataSyncIntervalView(interval);
		}).AddTo(this);
		ChangeSaveDataSyncIntervalView(SaveDataManager.Instance.SettingData.SaveDataSyncInterval.Value);
		_generalInitButton.Setup();
		_settingService.IsChangeGeneral.Subscribe(delegate(bool isChanged)
		{
			if (isChanged)
			{
				_generalInitButton.Activate();
			}
			else
			{
				_generalInitButton.Deactivate();
			}
		}).AddTo(this);
		if (_settingService.IsDifferenceInitSetting(SettingType.General))
		{
			_generalInitButton.Activate();
		}
		else
		{
			_generalInitButton.Deactivate();
		}
		ObservableSubscribeExtensions.Subscribe(_generalInitButton.OnClickButton, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.InitSetting(SettingType.General);
		}).AddTo(this);
		_graphicInitButton.Setup();
		_settingService.IsChangeGraphic.Subscribe(delegate(bool isChanged)
		{
			if (isChanged)
			{
				_graphicInitButton.Activate();
			}
			else
			{
				_graphicInitButton.Deactivate();
			}
		}).AddTo(this);
		if (_settingService.IsDifferenceInitSetting(SettingType.Graphic))
		{
			_graphicInitButton.Activate();
		}
		else
		{
			_graphicInitButton.Deactivate();
		}
		ObservableSubscribeExtensions.Subscribe(_graphicInitButton.OnClickButton, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.InitSetting(SettingType.Graphic);
			if (SaveDataManager.Instance.SettingData.IsUseVerticalSync.Value)
			{
				_verticalSyncActivateInteractableUI.ActivateUseUI(isUseDoComplete: true);
			}
			else
			{
				_verticalSyncDeactivateInteractableUI.ActivateUseUI(isUseDoComplete: true);
			}
		}).AddTo(this);
		_audioInitButton.Setup();
		_settingService.IsChangeAudio.Subscribe(delegate(bool isChanged)
		{
			if (isChanged)
			{
				_audioInitButton.Activate();
			}
			else
			{
				_audioInitButton.Deactivate();
			}
		}).AddTo(this);
		if (_settingService.IsDifferenceInitSetting(SettingType.Audio))
		{
			_audioInitButton.Activate();
		}
		else
		{
			_audioInitButton.Deactivate();
		}
		ObservableSubscribeExtensions.Subscribe(_audioInitButton.OnClickButton, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.InitSetting(SettingType.Audio);
			_masterVolumeChangeUI.AdjustToSaveData();
			_systemSEVolumeChangeUI.AdjustToSaveData();
			_voiceVolumeChangeUI.AdjustToSaveData();
			_ambientBGMVolumeChangeUI.AdjustToSaveData();
			_ambientSEVolumeChangeUI.AdjustToSaveData();
		}).AddTo(this);
		ChangeLanguageView(SaveDataManager.Instance.SettingData.GameLanguage.Value);
		_timeFormatAllInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_timeFormatAllInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeTimeFormat(TimeFormatType.All);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_timeFormatAMPMInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_timeFormatAMPMInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeTimeFormat(TimeFormatType.AMPM);
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.TimeFormat.Subscribe(delegate(TimeFormatType timeFormat)
		{
			ChangeTimeFormatView(timeFormat);
		}).AddTo(this);
		ChangeTimeFormatView(SaveDataManager.Instance.SettingData.TimeFormat.Value);
		_windowTpAlwaysActivateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_windowTpAlwaysActivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeAlwaysOnTop(isAlwaysOnTop: true);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_windowTpAlwaysDeactivateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_windowTpAlwaysDeactivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeAlwaysOnTop(isAlwaysOnTop: false);
			_systemSeService.PlayClick();
		}).AddTo(this);
		ScreenSystem.IsWindowTpAlways.Subscribe(delegate(bool isWindowTpAlways)
		{
			ChangeWindowTpAlwaysView(isWindowTpAlways);
		}).AddTo(this);
		ChangeWindowTpAlwaysView(SaveDataManager.Instance.SettingData.IsAlwaysOnTop);
		_lunaNewYear2026Parent.gameObject.SetActive(SpecialLunaNewYear2026Util.Instance.IsValidPeriod());
		_lunaNewYear2026ActivateInteractableUI.Setup();
		_lunaNewYear2026DeactivateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_lunaNewYear2026ActivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsValid.Value = true;
			SaveDataManager.Instance.SaveCollaborationData();
			_systemSeService.PlayClick();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_lunaNewYear2026DeactivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsValid.Value = false;
			if (SaveDataManager.Instance.CollaborationSaveData.CurrentType.CurrentValue == SpecialService.CollaborationType.LunaNewYear2026)
			{
				SaveDataManager.Instance.CollaborationSaveData.CurrentType.Value = SpecialService.CollaborationType.None;
			}
			SaveDataManager.Instance.SaveCollaborationData();
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.CollaborationSaveData.LunaNewYear2026Data.IsValid.Subscribe(delegate(bool isValid)
		{
			if (isValid)
			{
				_lunaNewYear2026ActivateInteractableUI.ActivateUseUI();
				_lunaNewYear2026DeactivateInteractableUI.DeactivateUseUI();
			}
			else
			{
				_lunaNewYear2026ActivateInteractableUI.DeactivateUseUI();
				_lunaNewYear2026DeactivateInteractableUI.ActivateUseUI();
			}
		}).AddTo(this);
		_windowedInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_windowedInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeWindowMode(WindowModeType.Window);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_borderlessFullscreenInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_borderlessFullscreenInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeWindowMode(WindowModeType.BorderlessFullScreen);
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.WindowMode.Skip(1).Subscribe(delegate(WindowModeType windowMode)
		{
			ChangeWindowModeView(windowMode);
		}).AddTo(this);
		ChangeWindowModeView(SaveDataManager.Instance.SettingData.WindowMode.Value);
		_windowResolutionFirstInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_windowResolutionFirstInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeWindowResolution(WindowResolutionType.First);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_windowResolutionSecondInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_windowResolutionSecondInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeWindowResolution(WindowResolutionType.Second);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_windowResolutionThirdInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_windowResolutionThirdInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeWindowResolution(WindowResolutionType.Third);
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.WindowResolution.Subscribe(delegate(WindowResolutionType resolutionType)
		{
			ChangeWindowResolutionView(resolutionType);
		}).AddTo(this);
		_verticalSyncActivateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_verticalSyncActivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeVerticalSync(isUse: true);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_verticalSyncDeactivateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_verticalSyncDeactivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeVerticalSync(isUse: false);
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.IsUseVerticalSync.Subscribe(delegate(bool isUseVerticalSync)
		{
			if (isUseVerticalSync)
			{
				_verticalSyncActivateInteractableUI.ActivateUseUI();
				_verticalSyncDeactivateInteractableUI.DeactivateUseUI();
			}
			else
			{
				_verticalSyncDeactivateInteractableUI.ActivateUseUI();
				_verticalSyncActivateInteractableUI.DeactivateUseUI();
			}
		}).AddTo(this);
		_activeFrameRateInputField.Setup(SaveDataManager.Instance.SettingData.ActiveFramerate.Value);
		_activeFrameRateInputField.OnEditEnd.Subscribe(delegate(int framerate)
		{
			_settingService.ChangeActiveFramerate(framerate);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.ActiveFramerate.Subscribe(delegate(int framerate)
		{
			_activeFrameRateInputField.SetText(framerate.ToString());
		}).AddTo(this);
		_deactiveFrameRateInputField.Setup(SaveDataManager.Instance.SettingData.DeactiveFramerate.Value);
		_deactiveFrameRateInputField.OnEditEnd.Subscribe(delegate(int framerate)
		{
			_settingService.ChangeDeactiveFramerate(framerate);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.DeactiveFramerate.Subscribe(delegate(int framerate)
		{
			_deactiveFrameRateInputField.SetText(framerate.ToString());
		}).AddTo(this);
		_graphicQualityHighInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_graphicQualityHighInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGraphicQuality(GraphicQualityLevel.High);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_graphicQualityMediumInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_graphicQualityMediumInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGraphicQuality(GraphicQualityLevel.Medium);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_graphicQualityLowInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_graphicQualityLowInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeGraphicQuality(GraphicQualityLevel.Low);
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.GraphicQuality.Subscribe(delegate(GraphicQualityLevel quality)
		{
			ChangeGraphicQualityView(quality);
		}).AddTo(this);
		_renderScaleSlider.minValue = 35f;
		_renderScaleSlider.maxValue = 100f;
		_renderScaleSlider.wholeNumbers = true;
		SaveDataManager.Instance.SettingData.RenderScale.Subscribe(delegate(int scale)
		{
			_renderScaleSlider.SetValueWithoutNotify(scale);
		}).AddTo(this);
		_renderScaleSlider.OnValueChangedAsObservable().Subscribe(delegate(float scale)
		{
			_settingService.ChangeRenderScale((int)scale);
		}).AddTo(this);
		_masterVolumeChangeUI.Setup();
		_masterVolumeChangeUI.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioMixerType)
		{
			_settingService.ChangeAudioMixerSwitchMute(audioMixerType);
		}).AddTo(this);
		_masterVolumeChangeUI.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			_settingService.ChangeAudioMixerVolume(info.Item1, info.value);
		}).AddTo(this);
		_systemSEVolumeChangeUI.Setup();
		_systemSEVolumeChangeUI.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioMixerType)
		{
			_settingService.ChangeAudioMixerSwitchMute(audioMixerType);
		}).AddTo(this);
		_systemSEVolumeChangeUI.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			_settingService.ChangeAudioMixerVolume(info.Item1, info.value);
		}).AddTo(this);
		_voiceVolumeChangeUI.Setup();
		_voiceVolumeChangeUI.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioMixerType)
		{
			_settingService.ChangeAudioMixerSwitchMute(audioMixerType);
		}).AddTo(this);
		_voiceVolumeChangeUI.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			_settingService.ChangeAudioMixerVolume(info.Item1, info.value);
		}).AddTo(this);
		_ambientBGMVolumeChangeUI.Setup();
		_ambientBGMVolumeChangeUI.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioMixerType)
		{
			_settingService.ChangeAudioMixerSwitchMute(audioMixerType);
		}).AddTo(this);
		_ambientBGMVolumeChangeUI.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			_settingService.ChangeAudioMixerVolume(info.Item1, info.value);
		}).AddTo(this);
		_ambientSEVolumeChangeUI.Setup();
		_ambientSEVolumeChangeUI.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioMixerType)
		{
			_settingService.ChangeAudioMixerSwitchMute(audioMixerType);
		}).AddTo(this);
		_ambientSEVolumeChangeUI.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			_settingService.ChangeAudioMixerVolume(info.Item1, info.value);
		}).AddTo(this);
		_isPlayPomodoroActivateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_isPlayPomodoroActivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeIsPlayPomodoroSound(isPlay: true);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_isPlayPomodoroDeactivateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_isPlayPomodoroDeactivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeIsPlayPomodoroSound(isPlay: false);
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.IsPlayPomodoroSe.Subscribe(delegate(bool isPlay)
		{
			_isPlayPomodoroActivateInteractableUI.DeactivateUseUI();
			_isPlayPomodoroDeactivateInteractableUI.DeactivateUseUI();
			if (isPlay)
			{
				_isPlayPomodoroActivateInteractableUI.ActivateUseUI();
			}
			else
			{
				_isPlayPomodoroDeactivateInteractableUI.ActivateUseUI();
			}
		}).AddTo(this);
		_isPlaySelfTalkActivateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_isPlaySelfTalkActivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeIsPlaySelfTalk(isPlay: true);
			_systemSeService.PlayClick();
		}).AddTo(this);
		_isPlaySelfTalkDeactivateInteractableUI.Setup();
		ObservableSubscribeExtensions.Subscribe(_isPlaySelfTalkDeactivateInteractableUI.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			_settingService.ChangeIsPlaySelfTalk(isPlay: false);
			_systemSeService.PlayClick();
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.IsPlaySelfTalk.Subscribe(delegate(bool isPlay)
		{
			_isPlaySelfTalkActivateInteractableUI.DeactivateUseUI();
			_isPlaySelfTalkDeactivateInteractableUI.DeactivateUseUI();
			if (isPlay)
			{
				_isPlaySelfTalkActivateInteractableUI.ActivateUseUI();
			}
			else
			{
				_isPlaySelfTalkDeactivateInteractableUI.ActivateUseUI();
			}
		}).AddTo(this);
		ReactiveProperty<bool> reactiveProperty = new ReactiveProperty<bool>(value: true);
		reactiveProperty.Subscribe(this, delegate(bool enable, SettingUI @this)
		{
			@this._googleButton.LinkButton.View.interactable = enable;
			@this._googleButton.UnlinkButton.View.interactable = enable;
			@this._appleButton.LinkButton.View.interactable = enable;
			@this._appleButton.UnlinkButton.View.interactable = enable;
			AccountUtil.UpdateAuthButtonState(@this._appleButton, @this._googleButton);
		}).AddTo(this);
		_googleButton.LinkButton.OnClick.Select((Unit _) => AccountType.Google).Merge(_appleButton.LinkButton.OnClick.Select((Unit _) => AccountType.Apple)).SubscribeAwait(async delegate(AccountType type, CancellationToken ct)
		{
			_systemSeService.PlayClick();
			await OnLink(type, ct);
		}, reactiveProperty)
			.AddTo(this);
		_googleButton.UnlinkButton.OnClick.Select((Unit _) => AccountType.Google).Merge(_appleButton.UnlinkButton.OnClick.Select((Unit _) => AccountType.Apple)).SubscribeAwait(async delegate(AccountType type, CancellationToken ct)
		{
			_systemSeService.PlayClick();
			await OnUnlink(type, ct);
		})
			.AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_termsButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayClick();
			OpenURLFunctions.OpenTerms(_languageSupplier);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_privacyPolicyButton.OnClickAsObservable(), delegate
		{
			_systemSeService.PlayClick();
			OpenURLFunctions.OpenPrivacyPolicy(_languageSupplier);
		}).AddTo(this);
		UpdateOfflineDisableObjs();
		_rectTransform = base.transform as RectTransform;
		_fromPosY = _rectTransform.anchoredPosition.y + -8f;
		_toPosY = _rectTransform.anchoredPosition.y;
		void ChangeGraphicQualityView(GraphicQualityLevel quality)
		{
			_graphicQualityHighInteractableUI.DeactivateUseUI();
			_graphicQualityMediumInteractableUI.DeactivateUseUI();
			_graphicQualityLowInteractableUI.DeactivateUseUI();
			var (interactableUI, localizeId) = quality switch
			{
				GraphicQualityLevel.High => (_graphicQualityHighInteractableUI, "ui_setting_high"), 
				GraphicQualityLevel.Medium => (_graphicQualityMediumInteractableUI, "ui_setting_middle"), 
				GraphicQualityLevel.Low => (_graphicQualityLowInteractableUI, "ui_setting_low"), 
				_ => (null, null), 
			};
			if (interactableUI != null)
			{
				interactableUI.ActivateUseUI();
				_graphicQualityPulldownList.ChangeSelectContentTextByLocalizeId(localizeId);
			}
		}
		void ChangeLanguageView(GameLanguageType languageType)
		{
			_languageJPInteractableUI.DeactivateUseUI();
			_languageENInteractableUI.DeactivateUseUI();
			_languageSPInteractableUI.DeactivateUseUI();
			_languageTCInteractableUI.DeactivateUseUI();
			_languagePTInteractableUI.DeactivateUseUI();
			_languageKOInteractableUI.DeactivateUseUI();
			_languageRUInteractableUI.DeactivateUseUI();
			switch (languageType)
			{
			case GameLanguageType.Japanese:
				_languageJPInteractableUI.ActivateUseUI();
				_languagePulldownList.ChangeSelectContentText("日本語");
				break;
			case GameLanguageType.English:
				_languageENInteractableUI.ActivateUseUI();
				_languagePulldownList.ChangeSelectContentText("English");
				break;
			case GameLanguageType.ChineseSimplified:
				_languageSPInteractableUI.ActivateUseUI();
				_languagePulldownList.ChangeSelectContentText("简体中文");
				break;
			case GameLanguageType.ChineseTraditional:
				_languageTCInteractableUI.ActivateUseUI();
				_languagePulldownList.ChangeSelectContentText("繁体中文");
				break;
			case GameLanguageType.Portuguese:
				_languagePTInteractableUI.ActivateUseUI();
				_languagePulldownList.ChangeSelectContentText("Português");
				break;
			case GameLanguageType.Korean:
				_languageKOInteractableUI.ActivateUseUI();
				_languagePulldownList.ChangeSelectContentText("한국어");
				break;
			case GameLanguageType.Russian:
				_languageRUInteractableUI.ActivateUseUI();
				_languagePulldownList.ChangeSelectContentText("Русский");
				break;
			}
		}
		void ChangeSaveDataSyncIntervalView(SaveDataSyncInterval interval)
		{
			saveDataSyncInterval30Sec.DeactivateUseUI();
			saveDataSyncInterval60Sec.DeactivateUseUI();
			saveDataSyncInterval90Sec.DeactivateUseUI();
			string localizeID = interval switch
			{
				SaveDataSyncInterval.Sec30 => "ui_setting_savedata_sync_interval_30sec", 
				SaveDataSyncInterval.Sec60 => "ui_setting_savedata_sync_interval_60sec", 
				SaveDataSyncInterval.Sec90 => "ui_setting_savedata_sync_interval_90sec", 
				_ => throw new ArgumentOutOfRangeException("interval", interval, null), 
			};
			currentSaveDataSyncIntervalText.Set(localizeID);
			switch (interval)
			{
			case SaveDataSyncInterval.Sec30:
				saveDataSyncInterval30Sec.ActivateUseUI();
				break;
			case SaveDataSyncInterval.Sec60:
				saveDataSyncInterval60Sec.ActivateUseUI();
				break;
			case SaveDataSyncInterval.Sec90:
				saveDataSyncInterval90Sec.ActivateUseUI();
				break;
			}
		}
		void ChangeTimeFormatView(TimeFormatType timeFormat)
		{
			switch (timeFormat)
			{
			case TimeFormatType.All:
				_timeFormatAMPMInteractableUI.DeactivateUseUI();
				_timeFormatAllInteractableUI.ActivateUseUI();
				break;
			case TimeFormatType.AMPM:
				_timeFormatAllInteractableUI.DeactivateUseUI();
				_timeFormatAMPMInteractableUI.ActivateUseUI();
				break;
			}
		}
		void ChangeWindowModeView(WindowModeType windowMode)
		{
			_borderlessFullscreenInteractableUI.DeactivateUseUI();
			_windowedInteractableUI.DeactivateUseUI();
			switch (windowMode)
			{
			case WindowModeType.Window:
				_windowedInteractableUI.ActivateUseUI();
				SetWindowResolutionUIEnabled(enable: true);
				break;
			case WindowModeType.BorderlessFullScreen:
				_borderlessFullscreenInteractableUI.ActivateUseUI();
				SetWindowResolutionUIEnabled(enable: false);
				break;
			}
		}
		void ChangeWindowResolutionView(WindowResolutionType resolutionType)
		{
			_windowResolutionFirstInteractableUI.DeactivateUseUI();
			_windowResolutionSecondInteractableUI.DeactivateUseUI();
			_windowResolutionThirdInteractableUI.DeactivateUseUI();
			switch (resolutionType)
			{
			case WindowResolutionType.First:
				_windowResolutionFirstInteractableUI.ActivateUseUI();
				_windowResolutionPulldownList.ChangeSelectContentText("1280x720");
				break;
			case WindowResolutionType.Second:
				_windowResolutionSecondInteractableUI.ActivateUseUI();
				_windowResolutionPulldownList.ChangeSelectContentText("1600x900");
				break;
			case WindowResolutionType.Third:
				_windowResolutionThirdInteractableUI.ActivateUseUI();
				_windowResolutionPulldownList.ChangeSelectContentText("1920x1080");
				break;
			case WindowResolutionType.Custom:
				_windowResolutionPulldownList.ChangeSelectContentTextByLocalizeId("ui_setting_window_resolution_custom");
				break;
			}
		}
		void ChangeWindowTpAlwaysView(bool isWindowTpAlways)
		{
			_windowTpAlwaysDeactivateInteractableUI.DeactivateUseUI();
			_windowTpAlwaysActivateInteractableUI.DeactivateUseUI();
			if (isWindowTpAlways)
			{
				_windowTpAlwaysActivateInteractableUI.ActivateUseUI();
			}
			else
			{
				_windowTpAlwaysDeactivateInteractableUI.ActivateUseUI();
			}
		}
		async UniTask OnLink(AccountType type, CancellationToken ct)
		{
			WebApiError error = await _appAuth.AccountLink(type, _uiCanvasProvider, AccountTransferDialog, ct);
			AccountUtil.UpdateAuthButtonState(_appleButton, _googleButton);
			if (error.HasErrorOrReset)
			{
				error.LogException();
				await _errorBehaviour.ErrorDialog(error, ct);
			}
		}
		async UniTask OnUnlink(AccountType type, CancellationToken ct)
		{
			CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_common_confirm";
				o.BodyID = "ui_account_unlink_body";
				o.BodySelector = (string s) => string.Format(s, type.ToName());
				o.Parent = _uiCanvasProvider.CommonDialogParent;
				o.Buttons = new CommonButton[2]
				{
					new CommonButton("ui_account_unlink", CommonButtonStyle.Submit),
					new CommonButton("ui_common_confirm_cancel", CommonButtonStyle.Normal, SystemSeType.Cancel)
				};
				o.EnableCloseOnClickButton = true;
			});
			object obj = null;
			int num = 0;
			try
			{
				if (await dialog.SubmitOrCloseWaitAsync(ct) != 0)
				{
					num = 1;
				}
			}
			catch (object obj2)
			{
				obj = obj2;
			}
			if ((object)dialog != null)
			{
				await dialog.DisposeAsync();
			}
			object obj3 = obj;
			if (obj3 != null)
			{
				ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
			}
			if (num != 1)
			{
				WebApiError error = await _appAuth.AccountUnlink(type, ct);
				if (!error.HasErrorOrReset)
				{
					AccountUtil.UpdateAuthButtonState(_appleButton, _googleButton);
					dialog = CommonDialog.Create(delegate(CommonDialogOption o)
					{
						o.TitleID = "ui_common_complete";
						o.BodyID = "ui_account_unlink_success_body";
						o.BodySelector = (string s) => string.Format(s, type.ToName());
						o.Parent = _uiCanvasProvider.CommonDialogParent;
						o.Buttons = new CommonButton[1]
						{
							new CommonButton("ui_common_confirm_ok")
						};
						o.EnableCloseOnClickButton = true;
					});
					obj = null;
					num = 0;
					try
					{
						await dialog.SubmitOrCloseWaitAsync(ct);
						num = 1;
					}
					catch (object obj2)
					{
						obj = obj2;
					}
					if ((object)dialog != null)
					{
						await dialog.DisposeAsync();
					}
					obj3 = obj;
					if (obj3 != null)
					{
						ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
					}
					if (num == 1)
					{
						return;
					}
				}
				await _errorBehaviour.ErrorDialog(error, ct);
			}
		}
		void SetWindowResolutionUIEnabled(bool enable)
		{
			_windowResolutionDisableTween?.Kill();
			_windowResolutionDisableTween = null;
			_windowResolutionCanvasGroup.interactable = enable;
			float endValue = (enable ? 1f : DisabledAlpha);
			float duration = (enable ? 0.2f : 0.2f);
			_windowResolutionDisableTween = DOTween.Sequence().Append(_windowResolutionCanvasGroup.DOFade(endValue, duration)).Join(_windowResolutionPulldownList.CurrentSelectContentText.DOColor(enable ? Color.white : Color.gray, duration))
				.SetEase(Ease.InOutQuad)
				.SetLink(base.gameObject);
		}
	}

	private void UpdateOfflineDisableObjs()
	{
		if (SaveDataManager.Instance.AccountData.DeviceID.IsNullOrEmpty())
		{
			_newsInteractableUI.gameObject.SetActive(value: false);
			_inquiryIDGroup.gameObject.SetActive(value: false);
			saveDataSyncIntervalObj.gameObject.SetActive(value: false);
		}
		else
		{
			_newsInteractableUI.gameObject.SetActive(value: true);
			_inquiryIDGroup.gameObject.SetActive(value: true);
			saveDataSyncIntervalObj.gameObject.SetActive(value: true);
			_newsView.Setup();
		}
	}

	private void ChangeSelect(SettingType settingType)
	{
		_settingService.SelectSettingType(settingType);
	}

	private void OnOpenGeneralTab()
	{
		_languagePulldownList.ClosePullDown(immediate: true);
		saveDataSyncIntervalPulldown.ClosePullDown(immediate: true);
	}

	private void OnOpenGraphicTab()
	{
		_windowResolutionPulldownList.ClosePullDown(immediate: true);
		_graphicQualityPulldownList.ClosePullDown(immediate: true);
	}

	private void OnOpenAudioTab()
	{
	}

	private void OnOpenAccountTab()
	{
	}

	private void OnOpenNewsTab()
	{
		GetNewsAsync().Forget();
	}

	private async UniTask GetNewsAsync()
	{
		if (_webApiGate.AnyClosed())
		{
			_newsView.SetNewsData(null);
			return;
		}
		WebApiResponse<GetNewsResponse> getNews = default(WebApiResponse<GetNewsResponse>);
		_newsView.Hide();
		NewsState newsState = InMemoryData.GetOrSet(() => new NewsState());
		GameLanguageType currentGameLanguage = SaveDataManager.Instance.SettingData.GameLanguage.CurrentValue;
		if (newsState.AvailableNewNews.CurrentValue || _cachedNews == null || _cachedNewsLanguageType != currentGameLanguage)
		{
			using (_loadingScreen.CreateLoadingScope())
			{
				_loadingScreen.SetBgAlpha(0f);
				getNews = await WebApi.GetAsync<GetNews, GetNewsResponse>(new GetNews(SaveDataManager.Instance.AccountData.DeviceID, currentGameLanguage), this.GetCancellationTokenOnDestroy());
			}
			if (await _errorBehaviour.ErrorDialog(getNews.Error, this.GetCancellationTokenOnDestroy()))
			{
				return;
			}
			newsState.AvailableNewNews.Value = false;
			NewsData[] newsDatas = getNews.Response.NewsDatas;
			if (newsDatas.Length != 0)
			{
				Array.Sort(newsDatas, (NewsData a, NewsData b) => b.CompareTo(a));
				SaveDataManager.Instance.PlayerData.ReadNewsID = newsDatas[0].ID;
				SaveDataManager.Instance.SavePlayerData();
			}
			_cachedNews = newsDatas;
			_cachedNewsLanguageType = currentGameLanguage;
		}
		_newsView.SetNewsData(_cachedNews);
		_newsView.Show();
	}

	private void OnOpenCreditTab()
	{
	}

	public bool IsActive()
	{
		return _isActive;
	}

	public void Activate()
	{
		UpdateOfflineDisableObjs();
		_changeOrderService.BringToFront(ChangeOrderService.OrderItemType.Setting);
		_isActive = true;
		_facilityOpenButton.ActivateUseUI();
		base.gameObject.SetActive(value: true);
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_toPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(1f, 0.2f);
	}

	public void Deactivate()
	{
		_isActive = false;
		_facilityOpenButton.DeactivateUseUI();
		_moveTween?.Kill();
		_fadeTween?.Kill();
		_moveTween = _rectTransform.DOAnchorPosY(_fromPosY, 0.2f);
		_fadeTween = _canvasGroup.DOFade(0f, 0.2f).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	private void OnEnable()
	{
		_lunaNewYear2026Parent.gameObject.SetActive(SpecialLunaNewYear2026Util.Instance.IsValidPeriod());
	}
}
