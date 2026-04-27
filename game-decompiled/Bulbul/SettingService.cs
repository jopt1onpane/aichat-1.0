using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NestopiSystem;
using R3;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace Bulbul;

public class SettingService : IDisposable
{
	[Inject]
	private AudioMixerGroupContainer _audioMixer;

	[Inject]
	private LanguageSupplier _languageSupplier;

	private CancellationTokenSource _cts = new CancellationTokenSource();

	private readonly ReactiveProperty<SettingType> _settingType = new ReactiveProperty<SettingType>();

	private readonly Subject<(AudioMixerType, float)> _onChangeAudioMixierVolume = new Subject<(AudioMixerType, float)>();

	private readonly ReactiveProperty<bool> _isChangeGeneral = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> _isChangeGraphic = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> _isChangeAudio = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> _isChangeNotification = new ReactiveProperty<bool>();

	private readonly CompositeDisposable _disposable = new CompositeDisposable();

	public ReadOnlyReactiveProperty<SettingType> SettingType => _settingType;

	public Observable<(AudioMixerType, float)> OnChangeAudioMixierVolume => _onChangeAudioMixierVolume;

	public ReadOnlyReactiveProperty<bool> IsChangeGeneral => _isChangeGeneral;

	public ReadOnlyReactiveProperty<bool> IsChangeGraphic => _isChangeGraphic;

	public ReadOnlyReactiveProperty<bool> IsChangeAudio => _isChangeAudio;

	public ReadOnlyReactiveProperty<bool> IsChangeNotification => _isChangeNotification;

	public void Setup()
	{
		ScreenSystem.Initialize();
		SettingData settingData = SaveDataManager.Instance.SettingData;
		if (settingData.IsAlwaysOnTop)
		{
			ScreenSystem.BringWindowToTopAlways();
		}
		else
		{
			ScreenSystem.ResetWindowOrder();
		}
		ScreenSystem.IsWindowTpAlways.Subscribe(delegate(bool isTpAlways)
		{
			SaveDataManager.Instance.SettingData.IsAlwaysOnTop = isTpAlways;
			CheckDifferenceInitSetting(Bulbul.SettingType.General);
		}).AddTo(_disposable);
		WindowModeType value = settingData.WindowMode.Value;
		if (value != WindowModeType.Window && value == WindowModeType.BorderlessFullScreen)
		{
			ApplyWindowModeAndSize(settingData.WindowMode.Value, settingData.WindowResolution.Value, _cts.Token).Forget();
		}
		ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.SettingData.WindowMode.Skip(1), delegate
		{
			WindowModeType value2 = SaveDataManager.Instance.SettingData.WindowMode.Value;
			WindowResolutionType value3 = SaveDataManager.Instance.SettingData.WindowResolution.Value;
			ApplyWindowModeAndSize(value2, value3, _cts.Token).Forget();
		}).AddTo(_disposable);
		ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.SettingData.WindowResolution.Skip(1), delegate
		{
			WindowModeType value2 = SaveDataManager.Instance.SettingData.WindowMode.Value;
			WindowResolutionType value3 = SaveDataManager.Instance.SettingData.WindowResolution.Value;
			ApplyWindowModeAndSize(value2, value3, _cts.Token).Forget();
		}).AddTo(_disposable);
		ObservableSubscribeExtensions.Subscribe(Observable.Interval(TimeSpan.FromSeconds(1.0)), delegate
		{
			SettingData settingData2 = SaveDataManager.Instance.SettingData;
			if (settingData2.WindowMode.Value == WindowModeType.Window && settingData2.WindowResolution.Value != WindowResolutionType.Custom)
			{
				int windowResolutionWidth = GetWindowResolutionWidth(settingData2.WindowResolution.Value);
				int windowResolutionHeight = GetWindowResolutionHeight(settingData2.WindowResolution.Value);
				if (Screen.width != windowResolutionWidth || Screen.height != windowResolutionHeight)
				{
					settingData2.WindowResolution.Value = WindowResolutionType.Custom;
				}
			}
		}).AddTo(_disposable);
		_audioMixer.ManualUpdateAllVolume();
		ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.SettingData.IsUseVerticalSync, delegate
		{
			ApplyVerticalSync(settingData.IsUseVerticalSync.Value);
		}).AddTo(_disposable);
		ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.SettingData.ActiveFramerate, delegate
		{
			UpdateFrameRateForApplicationFocus();
		}).AddTo(_disposable);
		ObservableSubscribeExtensions.Subscribe(SaveDataManager.Instance.SettingData.DeactiveFramerate, delegate
		{
			UpdateFrameRateForApplicationFocus();
		}).AddTo(_disposable);
		SaveDataManager.Instance.SettingData.GraphicQuality.Subscribe(delegate(GraphicQualityLevel quality)
		{
			ApplyGraphicQuality(quality, SaveDataManager.Instance.SettingData.RenderScale.Value);
		}).AddTo(_disposable);
		SaveDataManager.Instance.SettingData.RenderScale.Subscribe(delegate(int scale)
		{
			ApplyRenderScale(scale);
		}).AddTo(_disposable);
		SaveDataManager.Instance.SettingData.ScreenSleepMode.Subscribe(delegate(ScreenSleepMode mode)
		{
			mode.SetSleepTimeout();
		}).AddTo(_disposable);
		if (DevicePlatform.Steam.IsMobile())
		{
			CheckDifferenceInitSetting(Bulbul.SettingType.General);
			CheckDifferenceInitSetting(Bulbul.SettingType.Graphic);
			CheckDifferenceInitSetting(Bulbul.SettingType.Audio);
			CheckDifferenceInitSetting(Bulbul.SettingType.Notification);
		}
	}

	public void SelectSettingType(SettingType settingType)
	{
		_settingType.Value = settingType;
	}

	public void OpenOfficialSNS(OfficialSNS sns)
	{
		Applicationx.OpenURL(sns switch
		{
			OfficialSNS.X => "https://x.com/chill_w_you", 
			OfficialSNS.Instagram => "https://www.instagram.com/nestopi_inc", 
			OfficialSNS.YouTube => "https://www.youtube.com/@nestopiinc", 
			_ => throw new ArgumentOutOfRangeException("sns", sns, null), 
		});
	}

	public void ChangeGameLanguage(GameLanguageType language)
	{
		_languageSupplier.Set(language);
	}

	public void ChangeAlwaysOnTop(bool isAlwaysOnTop)
	{
		if (isAlwaysOnTop)
		{
			ScreenSystem.BringWindowToTopAlways();
		}
		else
		{
			ScreenSystem.ResetWindowOrder();
		}
	}

	public void ChangeTimeFormat(TimeFormatType timeFormat)
	{
		SaveDataManager.Instance.SettingData.TimeFormat.Value = timeFormat;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.General);
	}

	public void ChangeWindowMode(WindowModeType windowMode)
	{
		SaveDataManager.Instance.SettingData.WindowMode.Value = windowMode;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Graphic);
	}

	public void ChangeWindowResolution(WindowResolutionType resolutionType)
	{
		SaveDataManager.Instance.SettingData.WindowResolution.Value = resolutionType;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Graphic);
	}

	private async UniTask ApplyWindowModeAndSize(WindowModeType windowMode, WindowResolutionType resolutionType, CancellationToken cancellationToken)
	{
		switch (windowMode)
		{
		case WindowModeType.Window:
			Screen.fullScreen = false;
			if (resolutionType != WindowResolutionType.Custom)
			{
				int windowResolutionWidth = GetWindowResolutionWidth(resolutionType);
				int windowResolutionHeight = GetWindowResolutionHeight(resolutionType);
				Screen.SetResolution(windowResolutionWidth, windowResolutionHeight, fullscreen: false);
			}
			break;
		case WindowModeType.BorderlessFullScreen:
		{
			Resolution currentResolution = Screen.currentResolution;
			Screen.SetResolution(currentResolution.width, currentResolution.height, fullscreen: true);
			Screen.fullScreen = true;
			break;
		}
		}
		await UniTask.WaitForFixedUpdate(cancellationToken);
		if (SaveDataManager.Instance.SettingData.IsAlwaysOnTop)
		{
			ScreenSystem.BringWindowToTopAlways();
		}
	}

	private int GetWindowResolutionWidth(WindowResolutionType resolutionType)
	{
		int result = Screen.width;
		switch (resolutionType)
		{
		case WindowResolutionType.First:
			result = 1280;
			break;
		case WindowResolutionType.Second:
			result = 1600;
			break;
		case WindowResolutionType.Third:
			result = 1920;
			break;
		default:
			Debug.LogError($"定義されていない解像度{resolutionType}を指定しています。");
			break;
		}
		return result;
	}

	private int GetWindowResolutionHeight(WindowResolutionType resolutionType)
	{
		int result = Screen.height;
		switch (resolutionType)
		{
		case WindowResolutionType.First:
			result = 720;
			break;
		case WindowResolutionType.Second:
			result = 900;
			break;
		case WindowResolutionType.Third:
			result = 1080;
			break;
		default:
			Debug.LogError($"定義されていない解像度{resolutionType}を指定しています。");
			break;
		}
		return result;
	}

	public void ChangeVerticalSync(bool isUse)
	{
		SaveDataManager.Instance.SettingData.IsUseVerticalSync.Value = isUse;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Graphic);
	}

	public void ApplyVerticalSync(bool isUse)
	{
		if (isUse)
		{
			QualitySettings.vSyncCount = 1;
		}
		else
		{
			QualitySettings.vSyncCount = 0;
		}
	}

	public void ChangeActiveFramerate(int framerate)
	{
		SaveDataManager.Instance.SettingData.ActiveFramerate.Value = framerate;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Graphic);
	}

	public void ChangeDeactiveFramerate(int framerate)
	{
		SaveDataManager.Instance.SettingData.DeactiveFramerate.Value = framerate;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Graphic);
	}

	public void UpdateFrameRateForApplicationFocus()
	{
		Application.targetFrameRate = (Application.isFocused ? SaveDataManager.Instance.SettingData.ActiveFramerate.Value : SaveDataManager.Instance.SettingData.DeactiveFramerate.Value);
	}

	public void ChangeGraphicQuality(GraphicQualityLevel quality)
	{
		SaveDataManager.Instance.SettingData.GraphicQuality.Value = quality;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Graphic);
	}

	private void ApplyGraphicQuality(GraphicQualityLevel quality, int renderScale)
	{
		QualitySettings.SetQualityLevel((int)quality);
		ApplyRenderScale(renderScale);
	}

	public void ChangeRenderScale(int scale)
	{
		SaveDataManager.Instance.SettingData.RenderScale.Value = scale;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Graphic);
	}

	private void ApplyRenderScale(int scale)
	{
		if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset universalRenderPipelineAsset)
		{
			universalRenderPipelineAsset.renderScale = (float)scale / 100f;
			bool flag = scale < 100;
			universalRenderPipelineAsset.upscalingFilter = (flag ? UpscalingFilterSelection.FSR : UpscalingFilterSelection.Auto);
		}
	}

	public void ChangeAudioMixerVolume(AudioMixerType audioMixerType, float volume)
	{
		SaveDataManager.Instance.SettingData.GetVolumeInfo(audioMixerType).Value = volume;
		_audioMixer.ManualUpdateVolume(audioMixerType);
		_onChangeAudioMixierVolume.OnNext((audioMixerType, volume));
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Audio);
	}

	public void ChangeAudioMixerSwitchMute(AudioMixerType audioMixerType)
	{
		if (SaveDataManager.Instance.SettingData.GetVolumeInfo(audioMixerType).IsMute.Value)
		{
			DeactivateAudioMixerMute(audioMixerType);
		}
		else
		{
			ActivateAudioMixerMute(audioMixerType);
		}
		_audioMixer.ManualUpdateVolume(audioMixerType);
		CheckDifferenceInitSetting(Bulbul.SettingType.Audio);
	}

	public void ActivateAudioMixerMute(AudioMixerType audioMixerType)
	{
		SaveDataManager.Instance.SettingData.GetVolumeInfo(audioMixerType).IsMute.Value = true;
		SaveDataManager.Instance.SaveSetting();
	}

	public void DeactivateAudioMixerMute(AudioMixerType audioMixerType)
	{
		VolumeInfo volumeInfo = SaveDataManager.Instance.SettingData.GetVolumeInfo(audioMixerType);
		bool value = volumeInfo.IsMute.Value;
		volumeInfo.IsMute.Value = false;
		if (value)
		{
			ChangeAudioMixerVolume(audioMixerType, volumeInfo.RawValue.Value);
		}
		SaveDataManager.Instance.SaveSetting();
	}

	public void ChangeIsPlayPomodoroSound(bool isPlay)
	{
		SaveDataManager.Instance.SettingData.IsPlayPomodoroSe.Value = isPlay;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Audio);
	}

	public void ChangeIsPlaySelfTalk(bool isPlay)
	{
		SaveDataManager.Instance.SettingData.IsPlaySelfTalk.Value = isPlay;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Audio);
	}

	public void ChangePomodoroNotification(bool isUse)
	{
		SaveDataManager.Instance.SettingData.IsNotificationPomodoro.Value = isUse;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Notification);
	}

	public void ChangeReminderNotification(bool isUse)
	{
		SaveDataManager.Instance.SettingData.IsNotificationReminder.Value = isUse;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.Notification);
	}

	public void ChangeWallpaperAutoTransitionSec(float sec)
	{
		SaveDataManager.Instance.SettingData.WallpaperAutoTransitionSec.Value = sec;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.General);
	}

	public void ChangeSleepMode(ScreenSleepMode mode)
	{
		SaveDataManager.Instance.SettingData.ScreenSleepMode.Value = mode;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.General);
	}

	public void ChangeSaveDataSyncInterval(SaveDataSyncInterval interval)
	{
		SaveDataManager.Instance.SettingData.SaveDataSyncInterval.Value = interval;
		SaveDataManager.Instance.SaveSetting();
		CheckDifferenceInitSetting(Bulbul.SettingType.General);
	}

	private void CheckDifferenceInitSetting(SettingType settingType)
	{
		bool value = IsDifferenceInitSetting(settingType);
		switch (settingType)
		{
		case Bulbul.SettingType.General:
			_isChangeGeneral.Value = value;
			break;
		case Bulbul.SettingType.Graphic:
			_isChangeGraphic.Value = value;
			break;
		case Bulbul.SettingType.Audio:
			_isChangeAudio.Value = value;
			break;
		case Bulbul.SettingType.Notification:
			_isChangeNotification.Value = value;
			break;
		}
	}

	public bool IsDifferenceInitSetting(SettingType settingType)
	{
		switch (settingType)
		{
		case Bulbul.SettingType.General:
			if (SaveDataManager.Instance.SettingData.TimeFormat.Value != TimeFormatType.AMPM)
			{
				return true;
			}
			if (SaveDataManager.Instance.SettingData.IsAlwaysOnTop)
			{
				return true;
			}
			if (SaveDataManager.Instance.SettingData.SaveDataSyncInterval.Value != SaveDataSyncInterval.Sec60)
			{
				return true;
			}
			if (DevicePlatform.Steam.IsMobile())
			{
				if (SaveDataManager.Instance.SettingData.WallpaperAutoTransitionSec.Value != 15f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.ScreenSleepMode.Value != ScreenSleepMode.Disable)
				{
					return true;
				}
			}
			break;
		case Bulbul.SettingType.Graphic:
			if (DevicePlatform.Steam.IsPC())
			{
				if (SaveDataManager.Instance.SettingData.WindowMode.Value != WindowModeType.Window)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.WindowResolution.Value != WindowResolutionType.Third)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.IsUseVerticalSync.Value)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.ActiveFramerate.Value != 60)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.DeactiveFramerate.Value != 24)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.GraphicQuality.Value != GraphicQualityLevel.High)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.RenderScale.Value != 100)
				{
					return true;
				}
			}
			else
			{
				if (SaveDataManager.Instance.SettingData.ActiveFramerate.Value != 30)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.GraphicQuality.Value != GraphicQualityLevel.Medium)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.RenderScale.Value != 70)
				{
					return true;
				}
			}
			break;
		case Bulbul.SettingType.Audio:
			if (DevicePlatform.Steam.IsPC())
			{
				if (SaveDataManager.Instance.SettingData.MasterVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.MusicVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.SystemSEVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.VoiceVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.AmbientBGMVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.AmbientSEVolumeInfo.Value != 0.5f)
				{
					return true;
				}
			}
			else
			{
				if (SaveDataManager.Instance.SettingData.MasterVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.MusicVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.SystemSEVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.VoiceVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.AmbientBGMVolumeInfo.Value != 0.5f)
				{
					return true;
				}
				if (SaveDataManager.Instance.SettingData.AmbientSEVolumeInfo.Value != 0.5f)
				{
					return true;
				}
			}
			if (!SaveDataManager.Instance.SettingData.IsPlayPomodoroSe.Value)
			{
				return true;
			}
			if (!SaveDataManager.Instance.SettingData.IsPlaySelfTalk.Value)
			{
				return true;
			}
			break;
		case Bulbul.SettingType.Notification:
			if (!SaveDataManager.Instance.SettingData.IsNotificationPomodoro.Value)
			{
				return true;
			}
			if (!SaveDataManager.Instance.SettingData.IsNotificationReminder.Value)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public void InitSetting(SettingType settingType)
	{
		switch (settingType)
		{
		case Bulbul.SettingType.General:
			SaveDataManager.Instance.SettingData.TimeFormat.Value = TimeFormatType.AMPM;
			SaveDataManager.Instance.SettingData.IsAlwaysOnTop = false;
			ChangeAlwaysOnTop(SaveDataManager.Instance.SettingData.IsAlwaysOnTop);
			SaveDataManager.Instance.SettingData.SaveDataSyncInterval.Value = SaveDataSyncInterval.Sec60;
			if (DevicePlatform.Steam.IsMobile())
			{
				SaveDataManager.Instance.SettingData.IsNotificationPomodoro.Value = true;
				SaveDataManager.Instance.SettingData.IsNotificationReminder.Value = true;
				_isChangeNotification.Value = false;
				SaveDataManager.Instance.SettingData.WallpaperAutoTransitionSec.Value = 15f;
				SaveDataManager.Instance.SettingData.ScreenSleepMode.Value = ScreenSleepMode.Disable;
			}
			_isChangeGeneral.Value = false;
			break;
		case Bulbul.SettingType.Graphic:
			if (DevicePlatform.Steam.IsPC())
			{
				SaveDataManager.Instance.SettingData.WindowMode.Value = WindowModeType.Window;
				SaveDataManager.Instance.SettingData.WindowResolution.Value = WindowResolutionType.Third;
				SaveDataManager.Instance.SettingData.IsUseVerticalSync.Value = false;
				SaveDataManager.Instance.SettingData.ActiveFramerate.Value = 60;
				SaveDataManager.Instance.SettingData.DeactiveFramerate.Value = 24;
				SaveDataManager.Instance.SettingData.GraphicQuality.Value = GraphicQualityLevel.High;
				SaveDataManager.Instance.SettingData.RenderScale.Value = 100;
			}
			else
			{
				SaveDataManager.Instance.SettingData.WindowMode.Value = WindowModeType.BorderlessFullScreen;
				SaveDataManager.Instance.SettingData.ActiveFramerate.Value = 30;
				SaveDataManager.Instance.SettingData.GraphicQuality.Value = GraphicQualityLevel.Medium;
				SaveDataManager.Instance.SettingData.RenderScale.Value = 70;
			}
			_isChangeGraphic.Value = false;
			break;
		case Bulbul.SettingType.Audio:
			if (DevicePlatform.Steam.IsPC())
			{
				SaveDataManager.Instance.SettingData.MasterVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.MusicVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.SystemSEVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.VoiceVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.AmbientBGMVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.AmbientSEVolumeInfo.Value = 0.5f;
			}
			else
			{
				SaveDataManager.Instance.SettingData.MasterVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.MusicVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.SystemSEVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.VoiceVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.AmbientBGMVolumeInfo.Value = 0.5f;
				SaveDataManager.Instance.SettingData.AmbientSEVolumeInfo.Value = 0.5f;
			}
			SaveDataManager.Instance.SettingData.MasterVolumeInfo.IsMute.Value = false;
			SaveDataManager.Instance.SettingData.MusicVolumeInfo.IsMute.Value = false;
			SaveDataManager.Instance.SettingData.SystemSEVolumeInfo.IsMute.Value = false;
			SaveDataManager.Instance.SettingData.VoiceVolumeInfo.IsMute.Value = false;
			SaveDataManager.Instance.SettingData.AmbientBGMVolumeInfo.IsMute.Value = false;
			SaveDataManager.Instance.SettingData.AmbientSEVolumeInfo.IsMute.Value = false;
			SaveDataManager.Instance.SettingData.IsPlayPomodoroSe.Value = true;
			SaveDataManager.Instance.SettingData.IsPlaySelfTalk.Value = true;
			_isChangeAudio.Value = false;
			break;
		case Bulbul.SettingType.Notification:
			SaveDataManager.Instance.SettingData.IsNotificationPomodoro.Value = true;
			SaveDataManager.Instance.SettingData.IsNotificationReminder.Value = true;
			_isChangeNotification.Value = false;
			break;
		}
		SaveDataManager.Instance.SaveSetting();
	}

	public void Dispose()
	{
		_cts?.Cancel();
		_cts?.Dispose();
		_disposable?.Dispose();
		_settingType?.Dispose();
		_onChangeAudioMixierVolume?.Dispose();
		_isChangeGeneral?.Dispose();
		_isChangeGraphic?.Dispose();
		_isChangeAudio?.Dispose();
		_isChangeNotification?.Dispose();
	}
}
