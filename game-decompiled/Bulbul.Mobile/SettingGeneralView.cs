using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SettingGeneralView : MonoBehaviour
{
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private FacilityAnimationBase viewAnimation;

	[Header("全般")]
	[SerializeField]
	private PulldownListUI timeFormatPulldown;

	[SerializeField]
	private Canvas timeFormatPulldownCanvas;

	[SerializeField]
	private ClickOutsideDetector timeFormatPulldownDetector;

	[SerializeField]
	private InteractableUI timeFormatAMPM;

	[SerializeField]
	private InteractableUI timeformatALL;

	[SerializeField]
	private PulldownListUI languagePulldown;

	[SerializeField]
	private Canvas languagePulldownCanvas;

	[SerializeField]
	private ClickOutsideDetector languagePulldownDetector;

	[SerializeField]
	private InteractableUI languageJP;

	[SerializeField]
	private InteractableUI languageEN;

	[SerializeField]
	private InteractableUI languageSP;

	[SerializeField]
	private InteractableUI languageTC;

	[SerializeField]
	private InteractableUI languagePT;

	[SerializeField]
	private InteractableUI languageKO;

	[SerializeField]
	private InteractableUI languageRU;

	[SerializeField]
	private Button privacyOptionButton;

	[SerializeField]
	private SettingInitButton generalInitButton;

	[Header("通知")]
	[SerializeField]
	private ToggleSwitch notificationPomodoro;

	[SerializeField]
	private ToggleSwitch notificationReminder;

	[SerializeField]
	private SettingInitButton notificationInitButton;

	[Header("壁紙モード")]
	[SerializeField]
	private PulldownListUI wallpaperAutoTransitionSecPulldown;

	[SerializeField]
	private Canvas wallPaperAutoTransitionSecPulldownCanvas;

	[SerializeField]
	private ClickOutsideDetector wallPaperAutoTransitionSecPulldownDetector;

	[SerializeField]
	private InteractableUI wallPaperAutoTransitionSecNone;

	[SerializeField]
	private InteractableUI wallPaperAutoTransitionSecFive;

	[SerializeField]
	private InteractableUI wallPaperAutoTransitionSecTen;

	[SerializeField]
	private InteractableUI wallPaperAutoTransitionSecFifteen;

	[SerializeField]
	private InteractableUI wallPaperAutoTransitionSecTwenty;

	[SerializeField]
	private TextLocalizationBehaviour wallPaperAutoTransitionSecNoneText;

	[SerializeField]
	private TextLocalizationBehaviour wallPaperAutoTransitionSecFiveText;

	[SerializeField]
	private TextLocalizationBehaviour wallPaperAutoTransitionSecTenText;

	[SerializeField]
	private TextLocalizationBehaviour wallPaperAutoTransitionSecFifteenText;

	[SerializeField]
	private TextLocalizationBehaviour wallPaperAutoTransitionSecTwentyText;

	[Header("自動スリープ")]
	[SerializeField]
	private PulldownListUI sleepModePulldown;

	[SerializeField]
	private Canvas sleepModePulldownCanvas;

	[SerializeField]
	private ClickOutsideDetector sleepModePulldownDetector;

	[SerializeField]
	private InteractableUI sleepDisableMode;

	[SerializeField]
	private InteractableUI sleepSystemSettingMode;

	[SerializeField]
	private TextLocalizationBehaviour currentSleepModeText;

	[Header("セーブデータアップロード間隔")]
	[SerializeField]
	private PulldownListUI saveDataSyncIntervalPulldown;

	[SerializeField]
	private Canvas saveDataSyncIntervalPulldownCanvas;

	[SerializeField]
	private ClickOutsideDetector saveDataSyncIntervalPulldownDetector;

	[SerializeField]
	private InteractableUI saveDataSyncInterval30Sec;

	[SerializeField]
	private InteractableUI saveDataSyncInterval60Sec;

	[SerializeField]
	private InteractableUI saveDataSyncInterval90Sec;

	[SerializeField]
	private TextLocalizationBehaviour currentSaveDataSyncIntervalText;

	private Subject<Unit> onClickClose = new Subject<Unit>();

	private Subject<SettingType> onChangeSettingPage = new Subject<SettingType>();

	private Subject<TimeFormatType> onChangeTimeFormat = new Subject<TimeFormatType>();

	private Subject<GameLanguageType> onChangeLanguageType = new Subject<GameLanguageType>();

	private Subject<Unit> onClickGeneralInit = new Subject<Unit>();

	private Subject<bool> onChangePomodoroNotification = new Subject<bool>();

	private Subject<bool> onChangeReminderNotification = new Subject<bool>();

	private Subject<Unit> onClickNotificationInit = new Subject<Unit>();

	private Subject<float> onChangeWallpaperAutoTransitionSec = new Subject<float>();

	private readonly Subject<ScreenSleepMode> onChangeScreenSleepMode = new Subject<ScreenSleepMode>();

	private readonly Subject<SaveDataSyncInterval> onChangeSaveDataSyncInterval = new Subject<SaveDataSyncInterval>();

	public Observable<Unit> OnClickClose => onClickClose;

	public Observable<SettingType> OnChangeSettingPage => onChangeSettingPage;

	public Observable<TimeFormatType> OnChangeTimeFormat => onChangeTimeFormat;

	public Observable<GameLanguageType> OnChangeLanguageType => onChangeLanguageType;

	public Observable<Unit> OnClickGeneralInit => onClickGeneralInit;

	public Observable<bool> OnChangePomodoroNotification => onChangePomodoroNotification;

	public Observable<bool> OnChangeReminderNotification => onChangeReminderNotification;

	public Observable<Unit> OnClickNotificationInit => onClickNotificationInit;

	public Observable<float> OnChangeWallpaperAutoTransitionSec => onChangeWallpaperAutoTransitionSec;

	public Observable<ScreenSleepMode> OnChangeScreenSleepMode => onChangeScreenSleepMode;

	public Observable<SaveDataSyncInterval> OnChangeSaveDataSyncInterval => onChangeSaveDataSyncInterval;

	private void OnDestroy()
	{
		onClickClose?.Dispose();
		onChangeLanguageType?.Dispose();
		onChangeTimeFormat?.Dispose();
		onChangeLanguageType?.Dispose();
		onClickGeneralInit?.Dispose();
		onChangePomodoroNotification?.Dispose();
		onChangeReminderNotification?.Dispose();
		onClickNotificationInit?.Dispose();
		onChangeScreenSleepMode?.Dispose();
	}

	public void Activate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: true);
		}
		else
		{
			viewAnimation.Activate().Forget();
		}
	}

	public void Deactivate()
	{
		if (viewAnimation == null)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			viewAnimation.Deactivate().Forget();
		}
	}

	public void Setup(SettingData saveData)
	{
		viewAnimation.Setup();
		ObservableSubscribeExtensions.Subscribe(closeButton.OnClickAsObservable(), delegate
		{
			onClickClose.OnNext(Unit.Default);
		}).AddTo(this);
		timeFormatPulldown.Setup();
		ObservableSubscribeExtensions.Subscribe(timeFormatPulldown.OnClickPullDownButton, delegate
		{
			timeFormatPulldownCanvas.overrideSorting = true;
			timeFormatPulldownCanvas.sortingOrder = 3;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(timeFormatPulldown.OnClosePulldownComplete, delegate
		{
			timeFormatPulldownCanvas.overrideSorting = false;
			timeFormatPulldownCanvas.sortingOrder = 2;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(timeFormatPulldownDetector.OnClickOutside, delegate
		{
			if (timeFormatPulldown.IsOpen)
			{
				timeFormatPulldown.ClosePullDown();
			}
		}).AddTo(this);
		timeFormatAMPM.Setup();
		ObservableSubscribeExtensions.Subscribe(timeFormatAMPM.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeTimeFormat.OnNext(TimeFormatType.AMPM);
			timeFormatPulldown.ClosePullDown();
		}).AddTo(this);
		timeformatALL.Setup();
		ObservableSubscribeExtensions.Subscribe(timeformatALL.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeTimeFormat.OnNext(TimeFormatType.All);
			timeFormatPulldown.ClosePullDown();
		}).AddTo(this);
		languagePulldown.Setup();
		ObservableSubscribeExtensions.Subscribe(languagePulldown.OnClickPullDownButton, delegate
		{
			languagePulldownCanvas.overrideSorting = true;
			languagePulldownCanvas.sortingOrder = 3;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(languagePulldown.OnClosePulldownComplete, delegate
		{
			languagePulldownCanvas.overrideSorting = false;
			languagePulldownCanvas.sortingOrder = 2;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(languagePulldownDetector.OnClickOutside, delegate
		{
			if (languagePulldown.IsOpen)
			{
				languagePulldown.ClosePullDown();
			}
		}).AddTo(this);
		languageJP.Setup();
		ObservableSubscribeExtensions.Subscribe(languageJP.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeLanguageType.OnNext(GameLanguageType.Japanese);
			languagePulldown.ClosePullDown();
		}).AddTo(this);
		languageEN.Setup();
		ObservableSubscribeExtensions.Subscribe(languageEN.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeLanguageType.OnNext(GameLanguageType.English);
			languagePulldown.ClosePullDown();
		}).AddTo(this);
		languageSP.Setup();
		ObservableSubscribeExtensions.Subscribe(languageSP.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeLanguageType.OnNext(GameLanguageType.ChineseSimplified);
			languagePulldown.ClosePullDown();
		}).AddTo(this);
		languageTC.Setup();
		ObservableSubscribeExtensions.Subscribe(languageTC.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeLanguageType.OnNext(GameLanguageType.ChineseTraditional);
			languagePulldown.ClosePullDown();
		}).AddTo(this);
		languagePT.Setup();
		ObservableSubscribeExtensions.Subscribe(languagePT.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeLanguageType.OnNext(GameLanguageType.Portuguese);
			languagePulldown.ClosePullDown();
		}).AddTo(this);
		languageKO.Setup();
		ObservableSubscribeExtensions.Subscribe(languageKO.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeLanguageType.OnNext(GameLanguageType.Korean);
			languagePulldown.ClosePullDown();
		}).AddTo(this);
		languageRU.Setup();
		ObservableSubscribeExtensions.Subscribe(languageRU.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeLanguageType.OnNext(GameLanguageType.Russian);
			languagePulldown.ClosePullDown();
		}).AddTo(this);
		if (AdmobCtrl.GetInstance().RequirePrivacyOption)
		{
			privacyOptionButton.gameObject.SetActive(value: true);
			ObservableSubscribeExtensions.Subscribe(privacyOptionButton.GetComponent<Button>().OnClickAsObservable(), delegate
			{
				AdmobCtrl.GetInstance().ShowPrivacyOptionsForm(ads_init: true);
			}).AddTo(this);
		}
		else
		{
			privacyOptionButton.gameObject.SetActive(value: false);
		}
		generalInitButton.Setup();
		ObservableSubscribeExtensions.Subscribe(generalInitButton.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onClickGeneralInit.OnNext(Unit.Default);
		}).AddTo(this);
		notificationPomodoro.Setup();
		notificationPomodoro.SetToggle(saveData.IsNotificationPomodoro.Value, isImmediate: true);
		notificationPomodoro.OnValueChanged.Subscribe(delegate(bool value)
		{
			onChangePomodoroNotification.OnNext(value);
		}).AddTo(this);
		notificationReminder.Setup();
		notificationReminder.SetToggle(saveData.IsNotificationReminder.Value, isImmediate: true);
		notificationReminder.OnValueChanged.Subscribe(delegate(bool value)
		{
			onChangeReminderNotification.OnNext(value);
		}).AddTo(this);
		notificationInitButton.Setup();
		ObservableSubscribeExtensions.Subscribe(notificationInitButton.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onClickNotificationInit.OnNext(Unit.Default);
		}).AddTo(this);
		wallpaperAutoTransitionSecPulldown.Setup();
		ObservableSubscribeExtensions.Subscribe(wallpaperAutoTransitionSecPulldown.OnClickPullDownButton, delegate
		{
			wallPaperAutoTransitionSecPulldownCanvas.overrideSorting = true;
			wallPaperAutoTransitionSecPulldownCanvas.sortingOrder = 3;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(wallpaperAutoTransitionSecPulldown.OnClosePulldownComplete, delegate
		{
			wallPaperAutoTransitionSecPulldownCanvas.overrideSorting = false;
			wallPaperAutoTransitionSecPulldownCanvas.sortingOrder = 2;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(wallPaperAutoTransitionSecPulldownDetector.OnClickOutside, delegate
		{
			if (wallpaperAutoTransitionSecPulldown.IsOpen)
			{
				wallpaperAutoTransitionSecPulldown.ClosePullDown();
			}
		}).AddTo(this);
		wallPaperAutoTransitionSecNone.Setup();
		ObservableSubscribeExtensions.Subscribe(wallPaperAutoTransitionSecNone.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeWallpaperAutoTransitionSec.OnNext(-100f);
			wallpaperAutoTransitionSecPulldown.ClosePullDown();
		}).AddTo(this);
		wallPaperAutoTransitionSecNoneText.Set("ui_setting_disabled");
		wallPaperAutoTransitionSecFive.Setup();
		ObservableSubscribeExtensions.Subscribe(wallPaperAutoTransitionSecFive.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeWallpaperAutoTransitionSec.OnNext(5f);
			wallpaperAutoTransitionSecPulldown.ClosePullDown();
		}).AddTo(this);
		wallPaperAutoTransitionSecFiveText.Set("ui_common_sec", (string str) => str.Replace("{0}", "5"));
		wallPaperAutoTransitionSecTen.Setup();
		ObservableSubscribeExtensions.Subscribe(wallPaperAutoTransitionSecTen.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeWallpaperAutoTransitionSec.OnNext(10f);
			wallpaperAutoTransitionSecPulldown.ClosePullDown();
		}).AddTo(this);
		wallPaperAutoTransitionSecTenText.Set("ui_common_sec", (string str) => str.Replace("{0}", "10"));
		wallPaperAutoTransitionSecFifteen.Setup();
		ObservableSubscribeExtensions.Subscribe(wallPaperAutoTransitionSecFifteen.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeWallpaperAutoTransitionSec.OnNext(15f);
			wallpaperAutoTransitionSecPulldown.ClosePullDown();
		}).AddTo(this);
		wallPaperAutoTransitionSecFifteenText.Set("ui_common_sec", (string str) => str.Replace("{0}", "15"));
		wallPaperAutoTransitionSecTwenty.Setup();
		ObservableSubscribeExtensions.Subscribe(wallPaperAutoTransitionSecTwenty.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeWallpaperAutoTransitionSec.OnNext(20f);
			wallpaperAutoTransitionSecPulldown.ClosePullDown();
		}).AddTo(this);
		wallPaperAutoTransitionSecTwentyText.Set("ui_common_sec", (string str) => str.Replace("{0}", "20"));
		sleepModePulldown.Setup();
		ObservableSubscribeExtensions.Subscribe(sleepModePulldown.OnClickPullDownButton, delegate
		{
			sleepModePulldownCanvas.overrideSorting = true;
			sleepModePulldownCanvas.sortingOrder = 3;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(sleepModePulldown.OnClosePulldownComplete, delegate
		{
			sleepModePulldownCanvas.overrideSorting = false;
			sleepModePulldownCanvas.sortingOrder = 2;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(sleepModePulldownDetector.OnClickOutside, delegate
		{
			if (sleepModePulldown.IsOpen)
			{
				sleepModePulldown.ClosePullDown();
			}
		}).AddTo(this);
		sleepDisableMode.Setup();
		ObservableSubscribeExtensions.Subscribe(sleepDisableMode.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeScreenSleepMode.OnNext(ScreenSleepMode.Disable);
			sleepModePulldown.ClosePullDown();
		}).AddTo(this);
		sleepSystemSettingMode.Setup();
		ObservableSubscribeExtensions.Subscribe(sleepSystemSettingMode.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeScreenSleepMode.OnNext(ScreenSleepMode.SystemSetting);
			sleepModePulldown.ClosePullDown();
		}).AddTo(this);
		saveDataSyncIntervalPulldown.Setup();
		ObservableSubscribeExtensions.Subscribe(saveDataSyncIntervalPulldown.OnClickPullDownButton, delegate
		{
			saveDataSyncIntervalPulldownCanvas.overrideSorting = true;
			saveDataSyncIntervalPulldownCanvas.sortingOrder = 3;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(saveDataSyncIntervalPulldown.OnClosePulldownComplete, delegate
		{
			saveDataSyncIntervalPulldownCanvas.overrideSorting = false;
			saveDataSyncIntervalPulldownCanvas.sortingOrder = 2;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(saveDataSyncIntervalPulldownDetector.OnClickOutside, delegate
		{
			if (saveDataSyncIntervalPulldown.IsOpen)
			{
				saveDataSyncIntervalPulldown.ClosePullDown();
			}
		}).AddTo(this);
		saveDataSyncInterval30Sec.Setup();
		ObservableSubscribeExtensions.Subscribe(saveDataSyncInterval30Sec.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeSaveDataSyncInterval.OnNext(SaveDataSyncInterval.Sec30);
			saveDataSyncIntervalPulldown.ClosePullDown();
		}).AddTo(this);
		saveDataSyncInterval60Sec.Setup();
		ObservableSubscribeExtensions.Subscribe(saveDataSyncInterval60Sec.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeSaveDataSyncInterval.OnNext(SaveDataSyncInterval.Sec60);
			saveDataSyncIntervalPulldown.ClosePullDown();
		}).AddTo(this);
		saveDataSyncInterval90Sec.Setup();
		ObservableSubscribeExtensions.Subscribe(saveDataSyncInterval90Sec.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onChangeSaveDataSyncInterval.OnNext(SaveDataSyncInterval.Sec90);
			saveDataSyncIntervalPulldown.ClosePullDown();
		}).AddTo(this);
	}

	public void ChangeTimeFormat(TimeFormatType format)
	{
		timeFormatAMPM.DeactivateUseUI(isUseDoComplete: true);
		timeformatALL.DeactivateUseUI(isUseDoComplete: true);
		switch (format)
		{
		case TimeFormatType.All:
			timeformatALL.ActivateUseUI();
			timeFormatPulldown.ChangeSelectContentTextByLocalizeId("ui_setting_general_timeformat_24Hour");
			break;
		case TimeFormatType.AMPM:
			timeFormatAMPM.ActivateUseUI();
			timeFormatPulldown.ChangeSelectContentTextByLocalizeId("ui_setting_general_timeformat_12Hour");
			break;
		}
	}

	public void ChangeLanguage(GameLanguageType language)
	{
		languageJP.DeactivateUseUI(isUseDoComplete: true);
		languageEN.DeactivateUseUI(isUseDoComplete: true);
		languageSP.DeactivateUseUI(isUseDoComplete: true);
		languageTC.DeactivateUseUI(isUseDoComplete: true);
		languagePT.DeactivateUseUI(isUseDoComplete: true);
		languageKO.DeactivateUseUI(isUseDoComplete: true);
		languageRU.DeactivateUseUI(isUseDoComplete: true);
		switch (language)
		{
		case GameLanguageType.Japanese:
			languageJP.ActivateUseUI();
			languagePulldown.ChangeSelectContentText("日本語");
			break;
		case GameLanguageType.English:
			languageEN.ActivateUseUI();
			languagePulldown.ChangeSelectContentText("English");
			break;
		case GameLanguageType.ChineseSimplified:
			languageSP.ActivateUseUI();
			languagePulldown.ChangeSelectContentText("简体中文");
			break;
		case GameLanguageType.ChineseTraditional:
			languageTC.ActivateUseUI();
			languagePulldown.ChangeSelectContentText("繁体中文");
			break;
		case GameLanguageType.Portuguese:
			languagePT.ActivateUseUI();
			languagePulldown.ChangeSelectContentText("Português");
			break;
		case GameLanguageType.Korean:
			languageKO.ActivateUseUI();
			languagePulldown.ChangeSelectContentText("한국어");
			break;
		case GameLanguageType.Russian:
			languageRU.ActivateUseUI();
			languagePulldown.ChangeSelectContentText("Русский");
			break;
		}
	}

	public void ChangeWallpaperAutoTransitionSec(float sec)
	{
		wallPaperAutoTransitionSecNone.DeactivateUseUI(isUseDoComplete: true);
		wallPaperAutoTransitionSecFive.DeactivateUseUI(isUseDoComplete: true);
		wallPaperAutoTransitionSecTen.DeactivateUseUI(isUseDoComplete: true);
		wallPaperAutoTransitionSecFifteen.DeactivateUseUI(isUseDoComplete: true);
		wallPaperAutoTransitionSecTwenty.DeactivateUseUI(isUseDoComplete: true);
		if (sec == -100f)
		{
			wallpaperAutoTransitionSecPulldown.ChangeSelectContentTextByLocalizeId("ui_setting_disabled");
			wallPaperAutoTransitionSecNone.ActivateUseUI();
			return;
		}
		int num = (int)sec;
		string secStr = null;
		switch (num)
		{
		case 5:
			secStr = "5";
			wallPaperAutoTransitionSecFive.ActivateUseUI();
			break;
		case 10:
			secStr = "10";
			wallPaperAutoTransitionSecTen.ActivateUseUI();
			break;
		case 15:
			secStr = "15";
			wallPaperAutoTransitionSecFifteen.ActivateUseUI();
			break;
		case 20:
			secStr = "20";
			wallPaperAutoTransitionSecTwenty.ActivateUseUI();
			break;
		default:
			secStr = "";
			wallPaperAutoTransitionSecNone.ActivateUseUI();
			break;
		}
		wallpaperAutoTransitionSecPulldown.ChangeSelectContentTextByLocalizeId("ui_common_sec", (string str) => str.Replace("{0}", secStr));
	}

	public void ChangeSleepMode(ScreenSleepMode mode)
	{
		sleepDisableMode.DeactivateUseUI(isUseDoComplete: true);
		sleepSystemSettingMode.DeactivateUseUI(isUseDoComplete: true);
		string localizeID;
		switch (mode)
		{
		case ScreenSleepMode.SystemSetting:
			localizeID = "ui_setting_sleep_system";
			sleepSystemSettingMode.ActivateUseUI();
			break;
		case ScreenSleepMode.Disable:
			localizeID = "ui_setting_sleep_disable";
			sleepDisableMode.ActivateUseUI();
			break;
		default:
			throw new ArgumentOutOfRangeException("mode", mode, null);
		}
		currentSleepModeText.Set(localizeID);
	}

	public void ChangeSaveDataSyncInterval(SaveDataSyncInterval interval)
	{
		saveDataSyncInterval30Sec.DeactivateUseUI(isUseDoComplete: true);
		saveDataSyncInterval60Sec.DeactivateUseUI(isUseDoComplete: true);
		saveDataSyncInterval90Sec.DeactivateUseUI(isUseDoComplete: true);
		string localizeID;
		switch (interval)
		{
		case SaveDataSyncInterval.Sec30:
			localizeID = "ui_setting_savedata_sync_interval_30sec";
			saveDataSyncInterval30Sec.ActivateUseUI();
			break;
		case SaveDataSyncInterval.Sec60:
			localizeID = "ui_setting_savedata_sync_interval_60sec";
			saveDataSyncInterval60Sec.ActivateUseUI();
			break;
		case SaveDataSyncInterval.Sec90:
			localizeID = "ui_setting_savedata_sync_interval_90sec";
			saveDataSyncInterval90Sec.ActivateUseUI();
			break;
		default:
			throw new ArgumentOutOfRangeException("interval", interval, null);
		}
		currentSaveDataSyncIntervalText.Set(localizeID);
	}

	public void ChangePomodoroNotification(bool flag)
	{
		notificationPomodoro.SetToggle(flag);
	}

	public void ChangeReminderNotification(bool flag)
	{
		notificationReminder.SetToggle(flag);
	}

	public void ChangeGeneralSetting(bool isChange)
	{
		if (isChange)
		{
			generalInitButton.Activate();
		}
		else
		{
			generalInitButton.Deactivate();
		}
	}

	public void ChangeNotificationSetting(bool isChange)
	{
		if (isChange)
		{
			notificationInitButton.Activate();
		}
		else
		{
			notificationInitButton.Deactivate();
		}
	}
}
