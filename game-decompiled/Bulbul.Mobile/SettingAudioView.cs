using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul.Mobile;

public class SettingAudioView : MonoBehaviour
{
	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private FacilityAnimationBase viewAnimation;

	[SerializeField]
	private SettingInitButton audioInitButton;

	[SerializeField]
	private SettingVolumeChangeUI masterVolume;

	[SerializeField]
	private SettingVolumeChangeUI systemSEVolume;

	[SerializeField]
	private SettingVolumeChangeUI voiceVolume;

	[SerializeField]
	private SettingVolumeChangeUI ambientBGMVolume;

	[SerializeField]
	private SettingVolumeChangeUI ambientSEVolume;

	[SerializeField]
	private ToggleSwitch pomodoroSoundToggle;

	[SerializeField]
	private ToggleSwitch selfTalkToggle;

	private Subject<Unit> onClickClose = new Subject<Unit>();

	private Subject<Unit> onClickAudioInit = new Subject<Unit>();

	private Subject<(AudioMixerType, float value)> onChangeVolume = new Subject<(AudioMixerType, float)>();

	private Subject<AudioMixerType> onChangeMute = new Subject<AudioMixerType>();

	private Subject<bool> onPomodoroSettingChange = new Subject<bool>();

	private Subject<bool> onSelfTalkSettingChange = new Subject<bool>();

	public Observable<Unit> OnClickClose => onClickClose;

	public Observable<Unit> OnClickAudioInit => onClickAudioInit;

	public Observable<(AudioMixerType, float value)> OnChangeVolume => onChangeVolume;

	public Observable<AudioMixerType> OnChangeMute => onChangeMute;

	public Observable<bool> OnPomodoroSettingChange => onPomodoroSettingChange;

	public Observable<bool> OnSelfTalkSettingChange => onSelfTalkSettingChange;

	private void OnDestroy()
	{
		onClickClose?.Dispose();
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
			onClickClose?.OnNext(Unit.Default);
		}).AddTo(this);
		audioInitButton.Setup();
		ObservableSubscribeExtensions.Subscribe(audioInitButton.GetComponent<Button>().OnClickAsObservable(), delegate
		{
			onClickAudioInit?.OnNext(Unit.Default);
		}).AddTo(this);
		masterVolume.Setup();
		masterVolume.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioType)
		{
			onChangeMute?.OnNext(audioType);
		}).AddTo(this);
		masterVolume.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			onChangeVolume?.OnNext(info);
		}).AddTo(this);
		systemSEVolume.Setup();
		systemSEVolume.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioType)
		{
			onChangeMute?.OnNext(audioType);
		}).AddTo(this);
		systemSEVolume.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			onChangeVolume?.OnNext(info);
		}).AddTo(this);
		voiceVolume.Setup();
		voiceVolume.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioType)
		{
			onChangeMute?.OnNext(audioType);
		}).AddTo(this);
		voiceVolume.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			onChangeVolume?.OnNext(info);
		}).AddTo(this);
		ambientBGMVolume.Setup();
		ambientBGMVolume.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioType)
		{
			onChangeMute?.OnNext(audioType);
		}).AddTo(this);
		ambientBGMVolume.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			onChangeVolume?.OnNext(info);
		}).AddTo(this);
		ambientSEVolume.Setup();
		ambientSEVolume.OnClickChangeMute.Subscribe(delegate(AudioMixerType audioType)
		{
			onChangeMute?.OnNext(audioType);
		}).AddTo(this);
		ambientSEVolume.OnChangeVolume.Subscribe(delegate((AudioMixerType, float value) info)
		{
			onChangeVolume?.OnNext(info);
		}).AddTo(this);
		pomodoroSoundToggle.Setup();
		pomodoroSoundToggle.SetToggle(saveData.IsPlayPomodoroSe.Value, isImmediate: true);
		pomodoroSoundToggle.OnValueChanged.Subscribe(delegate(bool isOn)
		{
			onPomodoroSettingChange?.OnNext(isOn);
		}).AddTo(this);
		selfTalkToggle.Setup();
		selfTalkToggle.SetToggle(saveData.IsPlaySelfTalk.Value, isImmediate: true);
		selfTalkToggle.OnValueChanged.Subscribe(delegate(bool isOn)
		{
			onSelfTalkSettingChange?.OnNext(isOn);
		}).AddTo(this);
	}

	public void AdjustToSavedata()
	{
		masterVolume.AdjustToSaveData();
		systemSEVolume.AdjustToSaveData();
		voiceVolume.AdjustToSaveData();
		ambientBGMVolume.AdjustToSaveData();
		ambientSEVolume.AdjustToSaveData();
	}

	public void ChangePomodoroSE(bool isOn)
	{
		pomodoroSoundToggle.SetToggle(isOn);
	}

	public void ChangeSelfTalk(bool isOn)
	{
		selfTalkToggle.SetToggle(isOn);
	}

	public void ChangeAudioSetting(bool isChanged)
	{
		if (isChanged)
		{
			audioInitButton.Activate();
		}
		else
		{
			audioInitButton.Deactivate();
		}
	}
}
