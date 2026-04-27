using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bulbul;

public class SettingVolumeChangeUI : MonoBehaviour
{
	[SerializeField]
	private AudioMixerType _audioMixerType;

	[SerializeField]
	private Button _muteButton;

	[SerializeField]
	private Sprite _muteActiveSprite;

	[SerializeField]
	private Sprite _muteDeativeSprite;

	[SerializeField]
	private TextMeshProUGUI _currentVolumeText;

	[SerializeField]
	private Slider _volumeSlider;

	private Subject<AudioMixerType> _onClickChangeMute = new Subject<AudioMixerType>();

	private Subject<(AudioMixerType, float value)> _onChangeVolume = new Subject<(AudioMixerType, float)>();

	public Observable<AudioMixerType> OnClickChangeMute => _onClickChangeMute;

	public Observable<(AudioMixerType, float value)> OnChangeVolume => _onChangeVolume;

	public void Setup()
	{
		VolumeInfo volumeInfo = SaveDataManager.Instance.SettingData.GetVolumeInfo(_audioMixerType);
		ChangeMuteButtonImage(volumeInfo.IsMute.Value);
		ObservableSubscribeExtensions.Subscribe(_muteButton.OnClickAsObservable(), delegate
		{
			_onClickChangeMute.OnNext(_audioMixerType);
		}).AddTo(this);
		volumeInfo.IsMute.Subscribe(delegate(bool isMute)
		{
			ChangeMuteButtonImage(isMute);
		}).AddTo(this);
		_volumeSlider.value = volumeInfo.RawValue.Value;
		_volumeSlider.OnValueChangedAsObservable().Subscribe(delegate(float volume)
		{
			_onChangeVolume.OnNext((_audioMixerType, volume));
		}).AddTo(this);
		volumeInfo.RawValue.Subscribe(delegate(float volume)
		{
			_currentVolumeText.text = (int)(volume * 100f) + "%";
		}).AddTo(this);
	}

	private void ChangeMuteButtonImage(bool isMute)
	{
		if (isMute)
		{
			_muteButton.image.sprite = _muteActiveSprite;
		}
		else
		{
			_muteButton.image.sprite = _muteDeativeSprite;
		}
	}

	public void AdjustToSaveData()
	{
		VolumeInfo volumeInfo = SaveDataManager.Instance.SettingData.GetVolumeInfo(_audioMixerType);
		_volumeSlider.value = volumeInfo.RawValue.Value;
		ChangeMuteButtonImage(volumeInfo.IsMute.Value);
	}
}
