using R3;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Bulbul;

public class AmbientSoundBehavior : MonoBehaviour
{
	[Inject]
	private RoomAmbientSoundService _ambientSoundService;

	[Inject]
	private EnvironmentDataService _environmentDataService;

	[SerializeField]
	private Slider _volumeSlider;

	private AmbientSoundType _ambientSoundType;

	private float _muteBeforeVolume;

	private Subject<AmbientSoundType> _onDeactivateSound = new Subject<AmbientSoundType>();

	private Subject<AmbientSoundType> _onActivateSound = new Subject<AmbientSoundType>();

	public AmbientSoundType AmbientSoundType => _ambientSoundType;

	public Observable<AmbientSoundType> OnDeactivateSound => _onDeactivateSound;

	public Observable<AmbientSoundType> OnActivateSound => _onActivateSound;

	public void Setup(AmbientSoundType ambientSoundType)
	{
		_ambientSoundType = ambientSoundType;
		var (value, flag) = _environmentDataService.GetVolume(_ambientSoundType);
		_volumeSlider.value = value;
		if (flag)
		{
			MuteActivate();
		}
	}

	public void ApplySaveData()
	{
		Setup(_ambientSoundType);
	}

	public void ChangeMute()
	{
		if (_volumeSlider.value > 0f)
		{
			MuteActivate();
		}
		else
		{
			MuteDeactivate();
		}
	}

	public void MuteActivate()
	{
		_muteBeforeVolume = _volumeSlider.value;
		_environmentDataService.SetVolume(_ambientSoundType, _muteBeforeVolume);
		_environmentDataService.SetMute(_ambientSoundType, isMute: true);
		_volumeSlider.value = 0f;
		_onDeactivateSound.OnNext(_ambientSoundType);
	}

	public void MuteDeactivate()
	{
		if (_muteBeforeVolume <= 0f)
		{
			_volumeSlider.value = 0.5f;
		}
		else
		{
			_volumeSlider.value = _muteBeforeVolume;
		}
		_onActivateSound.OnNext(_ambientSoundType);
		_environmentDataService.SetMute(_ambientSoundType, isMute: false);
	}

	public void ChangeVolume(float volume)
	{
		AmbientSoundType ambientSoundType = _ambientSoundType;
		if (ambientSoundType - 5 <= AmbientSoundType.PinkNoise)
		{
			_ambientSoundService.ChangeVolume(AmbientSoundType.Fireworks_First, volume);
			_ambientSoundService.ChangeVolume(AmbientSoundType.Fireworks_Second, volume);
		}
		else
		{
			if (!_ambientSoundService.IsPlaying(_ambientSoundType))
			{
				_ambientSoundService.Play(new AmbientSoundParam
				{
					AmbientSound = _ambientSoundType,
					Volume = volume,
					IsLoop = true,
					IsAllowsDuplicate = true
				});
			}
			_ambientSoundService.ChangeVolume(_ambientSoundType, volume);
		}
		SaveDataManager.Instance.SaveEnviromentThrottled();
	}

	public void Replay()
	{
		(float volume, bool isMute) volume = _environmentDataService.GetVolume(_ambientSoundType);
		var (volume2, _) = volume;
		if (volume.isMute)
		{
			volume2 = 0f;
		}
		_ambientSoundService.Replay(new AmbientSoundParam
		{
			AmbientSound = _ambientSoundType,
			Volume = volume2,
			IsLoop = true,
			IsAllowsDuplicate = true
		});
	}

	public void Stop()
	{
		_ambientSoundService.Stop(_ambientSoundType);
	}

	public void OnValueChangedVolumeSlider()
	{
		float num = _ambientSoundService.CurrentVolume(_ambientSoundType);
		ChangeVolume(_volumeSlider.value);
		if (_volumeSlider.value > 0f)
		{
			if (num == 0f)
			{
				_onActivateSound.OnNext(_ambientSoundType);
			}
			_environmentDataService.SetMute(_ambientSoundType, isMute: false);
			_environmentDataService.SetVolume(_ambientSoundType, _volumeSlider.value);
		}
		else if (!_environmentDataService.GetVolume(_ambientSoundType).isMute)
		{
			MuteActivate();
			_onDeactivateSound.OnNext(_ambientSoundType);
		}
	}
}
