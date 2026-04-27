using System;
using GUPS.Obfuscator.Attribute;
using NestopiSystem;
using R3;

namespace Bulbul;

[Serializable]
[DoNotObfuscateClass]
public class SettingData
{
	[ES3NonSerializable]
	public ReactiveProperty<GameLanguageType> GameLanguage = new ReactiveProperty<GameLanguageType>();

	[ES3Serializable]
	private int _gameLanguageForSave;

	[ES3NonSerializable]
	public ReactiveProperty<TimeFormatType> TimeFormat = new ReactiveProperty<TimeFormatType>();

	[ES3Serializable]
	private int _timeFormatForSave;

	public bool IsAlwaysOnTop;

	[ES3NonSerializable]
	public ReactiveProperty<WindowModeType> WindowMode = new ReactiveProperty<WindowModeType>();

	[ES3Serializable]
	private int _windowModeForSave;

	[ES3NonSerializable]
	public ReactiveProperty<WindowResolutionType> WindowResolution = new ReactiveProperty<WindowResolutionType>();

	[ES3Serializable]
	private int _windowResolutionForSave;

	[ES3NonSerializable]
	public ReactiveProperty<bool> IsUseVerticalSync = new ReactiveProperty<bool>();

	[ES3Serializable]
	private bool _isUseVerticalSyncForSave;

	[ES3NonSerializable]
	public ReactiveProperty<int> ActiveFramerate = new ReactiveProperty<int>();

	[ES3Serializable]
	private int _activeFramerateForSave;

	[ES3NonSerializable]
	public ReactiveProperty<int> DeactiveFramerate = new ReactiveProperty<int>();

	[ES3Serializable]
	private int _deactiveFramerateForSave;

	[ES3NonSerializable]
	public ReactiveProperty<GraphicQualityLevel> GraphicQuality = new ReactiveProperty<GraphicQualityLevel>();

	[ES3Serializable]
	private int _graphicQualityForSave;

	[ES3NonSerializable]
	public ReactiveProperty<int> RenderScale = new ReactiveProperty<int>();

	[ES3Serializable]
	private int _renderScaleForSave;

	[ES3NonSerializable]
	public VolumeInfo MasterVolumeInfo;

	[ES3Serializable]
	private bool _masterIsMuteForSave;

	[ES3Serializable]
	private float _masterVolumeForSave;

	[ES3NonSerializable]
	public VolumeInfo MusicVolumeInfo;

	[ES3Serializable]
	private bool _musicIsMuteForSave;

	[ES3Serializable]
	private float _musicVolumeForSave;

	[ES3NonSerializable]
	public VolumeInfo SystemSEVolumeInfo;

	[ES3Serializable]
	private bool _systemSEIsMuteForSave;

	[ES3Serializable]
	private float _systemSEVolumeForSave;

	[ES3NonSerializable]
	public VolumeInfo VoiceVolumeInfo;

	[ES3Serializable]
	private bool _voiceIsMuteForSave;

	[ES3Serializable]
	private float _voiceVolumeForSave;

	[ES3NonSerializable]
	public VolumeInfo AmbientBGMVolumeInfo;

	[ES3Serializable]
	private bool _ambientBGMIsMuteForSave;

	[ES3Serializable]
	private float _ambientBGMVolumeForSave;

	[ES3Serializable]
	public float AmbientBGMVolumeBeforeStory;

	[ES3NonSerializable]
	public VolumeInfo AmbientSEVolumeInfo;

	[ES3Serializable]
	private bool _ambientSEIsMuteForSave;

	[ES3Serializable]
	private float _ambientSEVolumeForSave;

	[ES3Serializable]
	public float AmbientSEVolumeBeforeStory;

	[ES3NonSerializable]
	public ReactiveProperty<bool> IsPlayPomodoroSe = new ReactiveProperty<bool>();

	[ES3Serializable]
	private bool _isPlayPomodoroSeForSave;

	[ES3NonSerializable]
	public ReactiveProperty<bool> IsPlaySelfTalk = new ReactiveProperty<bool>();

	[ES3Serializable]
	private bool _isPlaySelfTalkForSave;

	[ES3NonSerializable]
	public ReactiveProperty<bool> IsNotificationPomodoro = new ReactiveProperty<bool>();

	[ES3Serializable]
	private bool _isNotificationPomodoroForSave;

	[ES3NonSerializable]
	public ReactiveProperty<bool> IsNotificationReminder = new ReactiveProperty<bool>();

	[ES3Serializable]
	private bool _isNotificationReminderForSave;

	[ES3NonSerializable]
	public ReactiveProperty<float> WallpaperAutoTransitionSec = new ReactiveProperty<float>();

	[ES3Serializable]
	private float _wallPaperAutoTransitionSecForSave;

	[ES3Serializable]
	public SerializableReactiveProperty<ScreenSleepMode> ScreenSleepMode = new SerializableReactiveProperty<ScreenSleepMode>();

	[ES3Serializable]
	public SerializableReactiveProperty<SaveDataSyncInterval> SaveDataSyncInterval = new SerializableReactiveProperty<SaveDataSyncInterval>();

	public VolumeInfo GetVolumeInfo(AudioMixerType audioMixerType)
	{
		VolumeInfo result = null;
		switch (audioMixerType)
		{
		case AudioMixerType.Master:
			result = MasterVolumeInfo;
			break;
		case AudioMixerType.Music:
			result = MusicVolumeInfo;
			break;
		case AudioMixerType.SystemSE:
			result = SystemSEVolumeInfo;
			break;
		case AudioMixerType.Voice:
			result = VoiceVolumeInfo;
			break;
		case AudioMixerType.AmbientBGM:
			result = AmbientBGMVolumeInfo;
			break;
		case AudioMixerType.AmbientSE:
			result = AmbientSEVolumeInfo;
			break;
		default:
			Debug.LogError($"{audioMixerType}は定義されていません。");
			break;
		}
		return result;
	}

	public SettingData()
	{
		TimeFormat.Value = TimeFormatType.AMPM;
		_timeFormatForSave = 1;
		WindowResolution.Value = WindowResolutionType.Third;
		_windowResolutionForSave = 2;
		IsUseVerticalSync.Value = false;
		_isUseVerticalSyncForSave = false;
		DeactiveFramerate.Value = 24;
		_deactiveFramerateForSave = 24;
		if (DevicePlatform.Steam.IsPC())
		{
			WindowMode.Value = WindowModeType.Window;
			_windowModeForSave = 0;
			ActiveFramerate.Value = 60;
			_activeFramerateForSave = 60;
			GraphicQuality.Value = GraphicQualityLevel.High;
			_graphicQualityForSave = 2;
			RenderScale.Value = 100;
			_renderScaleForSave = 100;
		}
		else
		{
			WindowMode.Value = WindowModeType.BorderlessFullScreen;
			_windowModeForSave = 1;
			ActiveFramerate.Value = 30;
			_activeFramerateForSave = 30;
			GraphicQuality.Value = GraphicQualityLevel.Medium;
			_graphicQualityForSave = 1;
			RenderScale.Value = 70;
			_renderScaleForSave = 70;
		}
		_masterIsMuteForSave = false;
		_masterVolumeForSave = (DevicePlatform.Steam.IsPC() ? 0.5f : 0.5f);
		MasterVolumeInfo = new VolumeInfo(isMute: false, _masterVolumeForSave);
		_musicIsMuteForSave = false;
		_musicVolumeForSave = (DevicePlatform.Steam.IsPC() ? 0.5f : 0.5f);
		MusicVolumeInfo = new VolumeInfo(isMute: false, _musicVolumeForSave);
		_systemSEIsMuteForSave = false;
		_systemSEVolumeForSave = (DevicePlatform.Steam.IsPC() ? 0.5f : 0.5f);
		SystemSEVolumeInfo = new VolumeInfo(isMute: false, _systemSEVolumeForSave);
		_voiceIsMuteForSave = false;
		_voiceVolumeForSave = (DevicePlatform.Steam.IsPC() ? 0.5f : 0.5f);
		VoiceVolumeInfo = new VolumeInfo(isMute: false, _voiceVolumeForSave);
		_ambientBGMIsMuteForSave = false;
		_ambientBGMVolumeForSave = (DevicePlatform.Steam.IsPC() ? 0.5f : 0.5f);
		AmbientBGMVolumeBeforeStory = 0.5f;
		AmbientBGMVolumeInfo = new VolumeInfo(isMute: false, _ambientBGMVolumeForSave);
		_ambientSEIsMuteForSave = false;
		_ambientSEVolumeForSave = (DevicePlatform.Steam.IsPC() ? 0.5f : 0.5f);
		AmbientSEVolumeBeforeStory = 0.5f;
		AmbientSEVolumeInfo = new VolumeInfo(isMute: false, _ambientSEVolumeForSave);
		_isPlayPomodoroSeForSave = true;
		IsPlayPomodoroSe.Value = true;
		_isPlaySelfTalkForSave = true;
		IsPlaySelfTalk.Value = true;
		_isNotificationPomodoroForSave = true;
		IsNotificationPomodoro.Value = _isNotificationPomodoroForSave;
		_isNotificationReminderForSave = true;
		IsNotificationReminder.Value = _isNotificationReminderForSave;
		_wallPaperAutoTransitionSecForSave = 15f;
		WallpaperAutoTransitionSec.Value = _wallPaperAutoTransitionSecForSave;
		ScreenSleepMode.Value = Bulbul.ScreenSleepMode.Disable;
		SaveDataSyncInterval.Value = Bulbul.SaveDataSyncInterval.Sec60;
	}

	public void LoadSetup()
	{
		GameLanguage.Value = (GameLanguageType)_gameLanguageForSave;
		TimeFormat.Value = (TimeFormatType)_timeFormatForSave;
		WindowMode.Value = (WindowModeType)_windowModeForSave;
		WindowResolution.Value = (WindowResolutionType)_windowResolutionForSave;
		IsUseVerticalSync.Value = _isUseVerticalSyncForSave;
		ActiveFramerate.Value = _activeFramerateForSave;
		DeactiveFramerate.Value = _deactiveFramerateForSave;
		GraphicQuality.Value = (GraphicQualityLevel)_graphicQualityForSave;
		RenderScale.Value = _renderScaleForSave;
		MasterVolumeInfo.IsMute.Value = _masterIsMuteForSave;
		MasterVolumeInfo.Value = _masterVolumeForSave;
		MusicVolumeInfo.IsMute.Value = _musicIsMuteForSave;
		MusicVolumeInfo.Value = _musicVolumeForSave;
		SystemSEVolumeInfo.IsMute.Value = _systemSEIsMuteForSave;
		SystemSEVolumeInfo.Value = _systemSEVolumeForSave;
		VoiceVolumeInfo.IsMute.Value = _voiceIsMuteForSave;
		VoiceVolumeInfo.Value = _voiceVolumeForSave;
		AmbientBGMVolumeInfo.IsMute.Value = _ambientBGMIsMuteForSave;
		AmbientBGMVolumeInfo.Value = _ambientBGMVolumeForSave;
		AmbientSEVolumeInfo.IsMute.Value = _ambientSEIsMuteForSave;
		AmbientSEVolumeInfo.Value = _ambientSEVolumeForSave;
		IsPlayPomodoroSe.Value = _isPlayPomodoroSeForSave;
		IsPlaySelfTalk.Value = _isPlaySelfTalkForSave;
		IsNotificationPomodoro.Value = _isNotificationPomodoroForSave;
		IsNotificationReminder.Value = _isNotificationReminderForSave;
		WallpaperAutoTransitionSec.Value = _wallPaperAutoTransitionSecForSave;
	}

	public void SaveReady()
	{
		IsAlwaysOnTop = ScreenSystem.IsWindowTpAlways.CurrentValue;
		_gameLanguageForSave = (int)GameLanguage.Value;
		_timeFormatForSave = (int)TimeFormat.Value;
		_windowModeForSave = (int)WindowMode.Value;
		_windowResolutionForSave = (int)WindowResolution.Value;
		_isUseVerticalSyncForSave = IsUseVerticalSync.Value;
		_activeFramerateForSave = ActiveFramerate.Value;
		_deactiveFramerateForSave = DeactiveFramerate.Value;
		_graphicQualityForSave = (int)GraphicQuality.Value;
		_renderScaleForSave = RenderScale.Value;
		_masterIsMuteForSave = MasterVolumeInfo.IsMute.Value;
		_masterVolumeForSave = MasterVolumeInfo.RawValue.Value;
		_musicIsMuteForSave = MusicVolumeInfo.IsMute.Value;
		_musicVolumeForSave = MusicVolumeInfo.RawValue.Value;
		_systemSEIsMuteForSave = SystemSEVolumeInfo.IsMute.Value;
		_systemSEVolumeForSave = SystemSEVolumeInfo.RawValue.Value;
		_voiceIsMuteForSave = VoiceVolumeInfo.IsMute.Value;
		_voiceVolumeForSave = VoiceVolumeInfo.RawValue.Value;
		_ambientBGMIsMuteForSave = AmbientBGMVolumeInfo.IsMute.Value;
		_ambientBGMVolumeForSave = AmbientBGMVolumeInfo.RawValue.Value;
		_ambientSEIsMuteForSave = AmbientSEVolumeInfo.IsMute.Value;
		_ambientSEVolumeForSave = AmbientSEVolumeInfo.RawValue.Value;
		_isPlayPomodoroSeForSave = IsPlayPomodoroSe.Value;
		_isPlaySelfTalkForSave = IsPlaySelfTalk.Value;
		_isNotificationPomodoroForSave = IsNotificationPomodoro.Value;
		_isNotificationReminderForSave = IsNotificationReminder.Value;
		_wallPaperAutoTransitionSecForSave = WallpaperAutoTransitionSec.Value;
	}
}
