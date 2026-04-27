using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Bulbul.Web;
using Cysharp.Threading.Tasks;
using FastEnumUtility;
using R3;
using UnityEngine;
using VContainer;

namespace Bulbul.Mobile;

public class FacilitySettingMobile : MonoBehaviour
{
	[Inject]
	private AudioMixerGroupContainer _audioMixer;

	[Inject]
	private LanguageSupplier _languageSupplier;

	[Inject]
	private SystemSeService _systemSeService;

	[Inject]
	private TutorialService _tutorialService;

	[Inject]
	private SettingService _settingService;

	[Inject]
	private AppAuth appAuth;

	[Inject]
	private WebApiErrorBehavior errorBehavior;

	[Inject]
	private IUICanvasProvider uiCanvasProvider;

	[Inject]
	private LoadingScreen loadingScreen;

	[Inject]
	private WebApiGate webApiGate;

	[Inject]
	private LanguageSupplier languageSupplier;

	[SerializeField]
	private SettingViewMobile _settingViewMobile;

	[SerializeField]
	private SettingTopView settingTop;

	[SerializeField]
	private SettingGeneralView settingGeneral;

	[SerializeField]
	private SettingGraphicsView settingGraphics;

	[SerializeField]
	private SettingAudioView settingAudio;

	[SerializeField]
	private SettingCreditView settingCredit;

	[SerializeField]
	private SettingShopView settingShop;

	[SerializeField]
	private SettingAccountView settingAccount;

	[SerializeField]
	private SettingNewsView settingNews;

	[SerializeField]
	private SettingContactUsView settingContactUs;

	[SerializeField]
	private RectTransform commonDialogParent;

	private NewsData[] _cachedNews;

	private GameLanguageType _cachedGameLanguage;

	private Subject<Unit> _onCloseFromCloseButton = new Subject<Unit>();

	public Observable<Unit> OnCloseFromCloseButton => _onCloseFromCloseButton;

	private void OnDestroy()
	{
		_onCloseFromCloseButton?.Dispose();
	}

	public void Setup()
	{
		_settingViewMobile.gameObject.SetActive(value: true);
		SettingTopSetup();
		SettingGeneralSetup();
		SettingGraphicsSetup();
		SettingAudioSetup();
		SettingCreditSetup();
		SettingShopSetup();
		SettingAccountSetup();
		SettingNewsSetup();
		SettingContactUsSetup();
		Deactivate();
	}

	private void SettingTopSetup()
	{
		settingTop.Activate();
		settingTop.Setup();
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickClose, delegate
		{
			_systemSeService.PlayCancel();
			settingTop.Deactivate();
			_onCloseFromCloseButton.OnNext(Unit.Default);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingTop.OnCloseTweenComplete, delegate
		{
			if (_settingService.SettingType.CurrentValue == SettingType.Top)
			{
				Deactivate();
			}
		});
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickGeneralSettingButton, delegate
		{
			_systemSeService.PlayClick();
			_settingService.SelectSettingType(SettingType.General);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickGraphicsSettingButton, delegate
		{
			_systemSeService.PlayClick();
			_settingService.SelectSettingType(SettingType.Graphic);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickAudioSettingButton, delegate
		{
			_systemSeService.PlayClick();
			_settingService.SelectSettingType(SettingType.Audio);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickCreditButton, delegate
		{
			_systemSeService.PlayClick();
			_settingService.SelectSettingType(SettingType.Credits);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickShopButton, delegate
		{
			_systemSeService.PlayClick();
			_settingService.SelectSettingType(SettingType.Shop);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickContactButton, delegate
		{
			_systemSeService.PlayClick();
			_settingService.SelectSettingType(SettingType.ContactUs);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickLinkAccountButton, delegate
		{
			_systemSeService.PlayClick();
			_settingService.SelectSettingType(SettingType.Account);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickNewsButton, delegate
		{
			_systemSeService.PlayClick();
			_settingService.SelectSettingType(SettingType.News);
		}).AddTo(this);
		_settingService.SettingType.Subscribe(delegate(SettingType type)
		{
			switch (type)
			{
			case SettingType.Top:
				settingTop.Activate();
				settingGeneral.Deactivate();
				settingGraphics.Deactivate();
				settingAudio.Deactivate();
				settingCredit.Deactivate();
				settingShop.Deactivate();
				settingAccount.Deactivate();
				settingNews.Deactivate();
				settingContactUs.Deactivate();
				break;
			case SettingType.General:
				settingTop.Deactivate();
				settingGeneral.Activate();
				settingGraphics.Deactivate();
				settingAudio.Deactivate();
				settingCredit.Deactivate();
				settingShop.Deactivate();
				settingAccount.Deactivate();
				settingNews.Deactivate();
				settingContactUs.Deactivate();
				break;
			case SettingType.Graphic:
				settingTop.Deactivate();
				settingGeneral.Deactivate();
				settingGraphics.Activate();
				settingAudio.Deactivate();
				settingCredit.Deactivate();
				settingShop.Deactivate();
				settingAccount.Deactivate();
				settingNews.Deactivate();
				settingContactUs.Deactivate();
				break;
			case SettingType.Audio:
				settingTop.Deactivate();
				settingGeneral.Deactivate();
				settingGraphics.Deactivate();
				settingAudio.Activate();
				settingCredit.Deactivate();
				settingShop.Deactivate();
				settingAccount.Deactivate();
				settingNews.Deactivate();
				settingContactUs.Deactivate();
				break;
			case SettingType.Credits:
				settingTop.Deactivate();
				settingGeneral.Deactivate();
				settingGraphics.Deactivate();
				settingAudio.Deactivate();
				settingCredit.Activate();
				settingShop.Deactivate();
				settingAccount.Deactivate();
				settingNews.Deactivate();
				settingContactUs.Deactivate();
				break;
			case SettingType.Account:
				settingTop.Deactivate();
				settingGeneral.Deactivate();
				settingGraphics.Deactivate();
				settingAudio.Deactivate();
				settingCredit.Deactivate();
				settingShop.Deactivate();
				settingAccount.Activate();
				settingNews.Deactivate();
				settingContactUs.Deactivate();
				break;
			case SettingType.Shop:
				settingTop.Deactivate();
				settingGeneral.Deactivate();
				settingGraphics.Deactivate();
				settingAudio.Deactivate();
				settingCredit.Deactivate();
				settingShop.Activate();
				settingAccount.Deactivate();
				settingNews.Deactivate();
				settingContactUs.Deactivate();
				break;
			case SettingType.News:
				settingTop.Deactivate();
				settingGeneral.Deactivate();
				settingGraphics.Deactivate();
				settingAudio.Deactivate();
				settingCredit.Deactivate();
				settingShop.Deactivate();
				settingAccount.Deactivate();
				settingNews.Activate();
				settingContactUs.Deactivate();
				break;
			case SettingType.ContactUs:
				settingTop.Deactivate();
				settingGeneral.Deactivate();
				settingGraphics.Deactivate();
				settingAudio.Deactivate();
				settingCredit.Deactivate();
				settingShop.Deactivate();
				settingAccount.Deactivate();
				settingNews.Deactivate();
				settingContactUs.Activate();
				break;
			case SettingType.Notification:
				break;
			}
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingTop.OnClickUIHelpButton, delegate
		{
			_systemSeService.PlayClick();
			_tutorialService.OpenTutorial(TutorialService.TutorialPageType.ScreenUI, TutorialService.TutorialPageOpenType.HelpAll);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_tutorialService.OnClose, delegate
		{
			settingTop.UIHelpClosed();
		}).AddTo(this);
		settingTop.OnClickSNSOpen.Subscribe(_settingService.OpenOfficialSNS).AddTo(this);
		settingTop.Deactivate();
	}

	private void SettingGeneralSetup()
	{
		settingGeneral.Activate();
		settingGeneral.Setup(SaveDataManager.Instance.SettingData);
		ObservableSubscribeExtensions.Subscribe(settingGeneral.OnClickClose, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.SelectSettingType(SettingType.Top);
		}).AddTo(this);
		settingGeneral.OnChangeSettingPage.Subscribe(delegate(SettingType settingType)
		{
			_systemSeService.PlayClick();
			_settingService.SelectSettingType(settingType);
		}).AddTo(this);
		settingGeneral.OnChangeTimeFormat.Subscribe(delegate(TimeFormatType format)
		{
			_systemSeService.PlayClick();
			_settingService.ChangeTimeFormat(format);
		}).AddTo(this);
		settingGeneral.OnChangeLanguageType.Subscribe(delegate(GameLanguageType language)
		{
			_systemSeService.PlayClick();
			_settingService.ChangeGameLanguage(language);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingGeneral.OnClickGeneralInit, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.InitSetting(SettingType.General);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.TimeFormat.Subscribe(delegate(TimeFormatType format)
		{
			settingGeneral.ChangeTimeFormat(format);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.GameLanguage.Subscribe(delegate(GameLanguageType language)
		{
			settingGeneral.ChangeLanguage(language);
		}).AddTo(this);
		settingGeneral.OnChangePomodoroNotification.Subscribe(delegate(bool value)
		{
			_systemSeService.PlayClick();
			_settingService.ChangePomodoroNotification(value);
		}).AddTo(this);
		settingGeneral.OnChangeReminderNotification.Subscribe(delegate(bool value)
		{
			_systemSeService.PlayClick();
			_settingService.ChangeReminderNotification(value);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.IsNotificationPomodoro.Subscribe(delegate(bool value)
		{
			settingGeneral.ChangePomodoroNotification(value);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.IsNotificationReminder.Subscribe(delegate(bool value)
		{
			settingGeneral.ChangeReminderNotification(value);
			NotificationCtrl.GetInstance().SetRemindPush(value);
		}).AddTo(this);
		settingGeneral.OnChangeWallpaperAutoTransitionSec.Subscribe(delegate(float sec)
		{
			_systemSeService.PlayClick();
			_settingService.ChangeWallpaperAutoTransitionSec(sec);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.WallpaperAutoTransitionSec.Subscribe(delegate(float sec)
		{
			settingGeneral.ChangeWallpaperAutoTransitionSec(sec);
		}).AddTo(this);
		settingGeneral.OnChangeScreenSleepMode.Subscribe(delegate(ScreenSleepMode mode)
		{
			_systemSeService.PlayClick();
			_settingService.ChangeSleepMode(mode);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.ScreenSleepMode.Subscribe(delegate(ScreenSleepMode mode)
		{
			settingGeneral.ChangeSleepMode(mode);
		}).AddTo(this);
		settingGeneral.OnChangeSaveDataSyncInterval.Subscribe(delegate(SaveDataSyncInterval interval)
		{
			_systemSeService.PlayClick();
			_settingService.ChangeSaveDataSyncInterval(interval);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.SaveDataSyncInterval.Subscribe(delegate(SaveDataSyncInterval interval)
		{
			settingGeneral.ChangeSaveDataSyncInterval(interval);
		}).AddTo(this);
		settingGeneral.Deactivate();
		bool flag = _settingService.IsDifferenceInitSetting(SettingType.General);
		bool flag2 = _settingService.IsDifferenceInitSetting(SettingType.Notification);
		settingGeneral.ChangeGeneralSetting(flag || flag2);
		ObservableSubscribeExtensions.Subscribe(_settingService.IsChangeGeneral, delegate
		{
			settingGeneral.ChangeGeneralSetting(_settingService.IsChangeGeneral.CurrentValue || _settingService.IsChangeNotification.CurrentValue);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(_settingService.IsChangeNotification, delegate
		{
			settingGeneral.ChangeGeneralSetting(_settingService.IsChangeGeneral.CurrentValue || _settingService.IsChangeNotification.CurrentValue);
		}).AddTo(this);
	}

	private void SettingGraphicsSetup()
	{
		settingGraphics.Activate();
		settingGraphics.Setup(SaveDataManager.Instance.SettingData);
		ObservableSubscribeExtensions.Subscribe(settingGraphics.OnClickClose, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.SelectSettingType(SettingType.Top);
		}).AddTo(this);
		settingGraphics.OnChangeQualityLevel.Subscribe(delegate(GraphicQualityLevel quality)
		{
			_systemSeService.PlayClick();
			_settingService.ChangeGraphicQuality(quality);
		}).AddTo(this);
		settingGraphics.OnChangeRenderScale.Subscribe(delegate(float renderScale)
		{
			_settingService.ChangeRenderScale((int)renderScale);
		}).AddTo(this);
		settingGraphics.OnChangeFrameRate.Subscribe(delegate(int value)
		{
			_systemSeService.PlayClick();
			_settingService.ChangeActiveFramerate(value);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingGraphics.OnClickGraphicInit, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.InitSetting(SettingType.Graphic);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.GraphicQuality.Subscribe(delegate(GraphicQualityLevel quality)
		{
			settingGraphics.ChangeGraphicQuality(quality);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.RenderScale.Subscribe(delegate(int scale)
		{
			settingGraphics.ChangeRenderScale(scale);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.ActiveFramerate.Subscribe(delegate(int frameRate)
		{
			settingGraphics.ChangeFrameRate(frameRate);
		}).AddTo(this);
		bool isChange = _settingService.IsDifferenceInitSetting(SettingType.Graphic);
		settingGraphics.ChangeGraphicSetting(isChange);
		ObservableSubscribeExtensions.Subscribe(_settingService.IsChangeGraphic, delegate
		{
			settingGraphics.ChangeGraphicSetting(_settingService.IsChangeGraphic.CurrentValue);
		}).AddTo(this);
		settingGraphics.Deactivate();
	}

	private void SettingAudioSetup()
	{
		settingAudio.Activate();
		settingAudio.Setup(SaveDataManager.Instance.SettingData);
		ObservableSubscribeExtensions.Subscribe(settingAudio.OnClickClose, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.SelectSettingType(SettingType.Top);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingAudio.OnClickAudioInit, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.InitSetting(SettingType.Audio);
			settingAudio.AdjustToSavedata();
		}).AddTo(this);
		settingAudio.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			_settingService.ChangeAudioMixerVolume(info.Item1, info.value);
		}).AddTo(this);
		settingAudio.OnChangeMute.Subscribe(delegate(AudioMixerType info)
		{
			_settingService.ChangeAudioMixerSwitchMute(info);
		}).AddTo(this);
		settingAudio.OnPomodoroSettingChange.Subscribe(delegate(bool info)
		{
			_settingService.ChangeIsPlayPomodoroSound(info);
			_systemSeService.PlayClick();
		}).AddTo(this);
		settingAudio.OnSelfTalkSettingChange.Subscribe(delegate(bool info)
		{
			_settingService.ChangeIsPlaySelfTalk(info);
			_systemSeService.PlayClick();
		}).AddTo(this);
		bool isChanged = _settingService.IsDifferenceInitSetting(SettingType.Audio);
		settingAudio.ChangeAudioSetting(isChanged);
		ObservableSubscribeExtensions.Subscribe(_settingService.IsChangeAudio, delegate
		{
			settingAudio.ChangeAudioSetting(_settingService.IsChangeAudio.CurrentValue);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.IsPlayPomodoroSe.Subscribe(delegate(bool value)
		{
			settingAudio.ChangePomodoroSE(value);
		}).AddTo(this);
		SaveDataManager.Instance.SettingData.IsPlaySelfTalk.Subscribe(delegate(bool value)
		{
			settingAudio.ChangeSelfTalk(value);
		}).AddTo(this);
		settingAudio.Deactivate();
	}

	private void SettingCreditSetup()
	{
		settingCredit.Activate();
		settingCredit.Setup();
		ObservableSubscribeExtensions.Subscribe(settingCredit.OnClickClose, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.SelectSettingType(SettingType.Top);
		}).AddTo(this);
		settingCredit.Deactivate();
	}

	private void SettingShopSetup()
	{
		settingShop.Activate();
		settingShop.Setup(SaveDataManager.Instance.SettingData);
		ObservableSubscribeExtensions.Subscribe(settingShop.OnClickClose, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.SelectSettingType(SettingType.Top);
		}).AddTo(this);
		settingShop.Deactivate();
	}

	private void SettingAccountSetup()
	{
		ObservableSubscribeExtensions.Subscribe(settingAccount.OnClickTerms, delegate
		{
			OpenURLFunctions.OpenTerms(languageSupplier);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingAccount.OnClickPrivacyPolicy, delegate
		{
			OpenURLFunctions.OpenPrivacyPolicy(languageSupplier);
		}).AddTo(this);
		ReactiveProperty<bool> reactiveProperty = new ReactiveProperty<bool>(value: true);
		reactiveProperty.AddTo(this);
		settingAccount.Activate();
		settingAccount.Setup(reactiveProperty);
		ObservableSubscribeExtensions.Subscribe(settingAccount.OnClickClose, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.SelectSettingType(SettingType.Top);
		}).AddTo(this);
		settingAccount.OnLink.SubscribeAwait(async delegate(AccountType type, CancellationToken ct)
		{
			WebApiError error = await appAuth.AccountLink(type, uiCanvasProvider, settingAccount.AccountTransferDialog, ct);
			settingAccount.UpdateState();
			if (error.HasErrorOrReset)
			{
				error.LogException();
				await errorBehavior.ErrorDialog(error, ct);
			}
		}, reactiveProperty).AddTo(this);
		settingAccount.OnUnlink.Where((AccountType x) => x != AccountType.None).SubscribeAwait(async delegate(AccountType type, CancellationToken ct)
		{
			CommonDialog dialog = CommonDialog.Create(delegate(CommonDialogOption o)
			{
				o.TitleID = "ui_common_confirm";
				o.BodyID = "ui_account_unlink_body";
				o.BodySelector = (string s) => string.Format(s, type.ToName());
				o.Parent = uiCanvasProvider.CommonDialogParent;
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
				WebApiError error = await appAuth.AccountUnlink(type, ct);
				if (!error.HasErrorOrReset)
				{
					settingAccount.UpdateState();
					dialog = CommonDialog.Create(delegate(CommonDialogOption o)
					{
						o.TitleID = "ui_common_complete";
						o.BodyID = "ui_account_unlink_success_body";
						o.BodySelector = (string s) => string.Format(s, type.ToName());
						o.Parent = uiCanvasProvider.CommonDialogParent;
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
				await errorBehavior.ErrorDialog(error, ct);
			}
		}, reactiveProperty).AddTo(this);
		settingAccount.OnDelete.SubscribeAwait(async delegate(Unit _, CancellationToken ct)
		{
			_systemSeService.PlayClick();
			WebApiError error = await appAuth.DeleteAccount(uiCanvasProvider, ct);
			if (error.HasErrorOrReset)
			{
				await errorBehavior.ErrorDialog(error, ct);
			}
		}, reactiveProperty).AddTo(this);
		settingAccount.Deactivate();
	}

	private void SettingNewsSetup()
	{
		ReactiveProperty<bool> gate = new ReactiveProperty<bool>(value: true);
		settingNews.Setup();
		ObservableSubscribeExtensions.Subscribe(settingNews.OnClickClose, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.SelectSettingType(SettingType.Top);
		}).AddTo(this);
		settingNews.OnActivateAnimationCompleted.SubscribeAwait(async delegate(Unit _, CancellationToken ct)
		{
			if (!webApiGate.AnyClosed())
			{
				WebApiResponse<GetNewsResponse> getNews = default(WebApiResponse<GetNewsResponse>);
				NewsState newsState = InMemoryData.GetOrSet(() => new NewsState());
				GameLanguageType currentGameLanguage = SaveDataManager.Instance.SettingData.GameLanguage.CurrentValue;
				if (newsState.AvailableNewNews.CurrentValue || _cachedNews == null || _cachedGameLanguage != currentGameLanguage)
				{
					using (loadingScreen.CreateLoadingScope())
					{
						getNews = await WebApi.GetAsync<GetNews, GetNewsResponse>(new GetNews(SaveDataManager.Instance.AccountData.DeviceID, currentGameLanguage), ct);
					}
					if (await errorBehavior.ErrorDialog(getNews.Error, ct))
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
					_cachedGameLanguage = currentGameLanguage;
				}
				settingNews.SetNewsData(_cachedNews);
			}
		}, gate).AddTo(this);
		settingNews.OnOpenDialog.Subscribe(delegate(NewsData newsData)
		{
			_systemSeService.PlayClick();
			settingNews.OpenNewsDialog(newsData);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingNews.OnCloseDialog, delegate
		{
			_systemSeService.PlayCancel();
			settingNews.CloseNewsDialog();
		}).AddTo(this);
	}

	private void SettingContactUsSetup()
	{
		settingContactUs.Setup();
		ObservableSubscribeExtensions.Subscribe(settingContactUs.OnClickClose, delegate
		{
			_systemSeService.PlayCancel();
			_settingService.SelectSettingType(SettingType.Top);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingContactUs.OnClickFAQ, delegate
		{
			_systemSeService.PlayClick();
			WaitOpenURL().Forget();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(settingContactUs.OnClickContactUs, delegate
		{
			_systemSeService.PlayClick();
			SettingUtility.OpenMailer();
		}).AddTo(this);
		async UniTask WaitOpenURL()
		{
			if (await CommonDialog.Create(delegate(CommonDialogOption option)
			{
				option.TitleID = null;
				option.BodyID = "ui_setting_confirm_jump_web";
				option.Parent = commonDialogParent;
				option.EnableCloseOnClickButton = true;
				option.Buttons = new CommonButton[2]
				{
					new CommonButton("ui_common_confirm_yes", CommonButtonStyle.Submit),
					new CommonButton("ui_common_confirm_no", CommonButtonStyle.Normal, SystemSeType.Cancel)
				};
			}).SubmitOrCloseWaitAsync(base.destroyCancellationToken) == 0)
			{
				OpenURLFunctions.OpenFAQ(languageSupplier);
			}
		}
	}

	public bool IsActive()
	{
		if (_settingViewMobile.gameObject.activeSelf)
		{
			return true;
		}
		return false;
	}

	public void Activate()
	{
		_settingViewMobile.gameObject.SetActive(value: true);
		_settingService.SelectSettingType(SettingType.Top);
		settingTop.Activate();
	}

	public void Deactivate()
	{
		_settingViewMobile.gameObject.SetActive(value: false);
	}
}
