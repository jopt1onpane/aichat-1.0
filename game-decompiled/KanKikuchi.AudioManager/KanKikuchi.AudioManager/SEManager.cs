using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace KanKikuchi.AudioManager;

public class SEManager : AudioManager<SEManager>
{
	public static readonly string AUDIO_DIRECTORY_PATH = "SE";

	[SerializeField]
	private bool _shouldAdjustVolumeRate = true;

	protected override int _audioPlayerNum => AudioManagerSetting.Entity.SEAudioPlayerNum;

	protected override AudioMixerGroup _audioMixerGroup => AudioManagerSetting.Entity.SEGroup;

	public static async UniTask CreateAndInitAsync()
	{
		GameObject gameObject = new GameObject("SEManager", typeof(SEManager));
		SEManager instance = gameObject.GetComponent<SEManager>();
		await instance.InitAsync();
		await UniTask.WaitUntil(() => instance.IsInitialized);
	}

	protected override async UniTask InitAsync()
	{
		await base.InitAsync();
		AudioManagerSetting setting = AudioManagerSetting.Entity;
		await LoadAudioClipAsync(AUDIO_DIRECTORY_PATH, setting.SECacheType, setting.LoadType, setting.IsReleaseSECache);
		_shouldAdjustVolumeRate = setting.ShouldAdjustSeVolumeRate;
		ChangeBaseVolume(setting.SEBaseVolume);
		if (!setting.IsDestroySEManager)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public AudioPlayer Play(AudioClip audioClip, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = false, Action callback = null)
	{
		volumeRate = AdjustVolumeRate(volumeRate, audioClip.name);
		if (volumeRate > 0f)
		{
			return RunPlayer(audioClip, volumeRate, delay, pitch, isLoop, callback);
		}
		return null;
	}

	public AudioPlayer Play(string audioPath, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = false, Action callback = null)
	{
		volumeRate = AdjustVolumeRate(volumeRate, audioPath);
		if (volumeRate > 0f)
		{
			return RunPlayer(audioPath, volumeRate, delay, pitch, isLoop, callback);
		}
		return null;
	}

	private float AdjustVolumeRate(float volumeRate, string audioPathOrName)
	{
		if (!_shouldAdjustVolumeRate)
		{
			return volumeRate;
		}
		string audioName = PathToName(audioPathOrName);
		if (_audioPlayerList.FindAll((AudioPlayer player) => player.CurrentAudioName == audioName).Count == 0)
		{
			return volumeRate;
		}
		foreach (AudioPlayer item in _audioPlayerList.FindAll((AudioPlayer player) => player.CurrentAudioName == audioName))
		{
			if (!(item.CurrentVolume <= 0f))
			{
				float num = item.PlayedTime;
				if (item.CurrentState == AudioPlayer.State.Delay)
				{
					num += (float)item.ElapsedDelay;
				}
				if (num <= 0.025f)
				{
					return 0f;
				}
				if (num <= 0.05f)
				{
					volumeRate *= 0.8f;
				}
				else if (num <= 0.1f)
				{
					volumeRate *= 0.9f;
				}
			}
		}
		return volumeRate;
	}
}
