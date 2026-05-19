using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace KanKikuchi.AudioManager;

public class AmbientBGMManager : AudioManager<AmbientBGMManager>
{
	public static readonly string AUDIO_DIRECTORY_PATH = "AmbientBGM";

	protected override int _audioPlayerNum => AudioManagerSetting.Entity.AmbientBgmAudioPlayerNum;

	protected override AudioMixerGroup _audioMixerGroup => AudioManagerSetting.Entity.AmbientBGMGroup;

	public static async UniTask CreateAndInitAsync()
	{
		GameObject gameObject = new GameObject("AmbientBGMManager", typeof(AmbientBGMManager));
		AmbientBGMManager instance = gameObject.GetComponent<AmbientBGMManager>();
		await instance.InitAsync();
		await UniTask.WaitUntil(() => instance.IsInitialized);
	}

	protected override async UniTask InitAsync()
	{
		await base.InitAsync();
		AudioManagerSetting setting = AudioManagerSetting.Entity;
		await LoadAudioClipAsync(AUDIO_DIRECTORY_PATH, AudioCacheType.All, setting.LoadType, setting.IsReleaseBGMCache);
		ChangeBaseVolume(setting.AmbientBGMBaseVolume);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public AudioPlayer Play(AudioClip audioClip, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = true, bool allowsDuplicate = false, string key = "", Action callback = null)
	{
		if (!allowsDuplicate)
		{
			Stop();
		}
		return RunPlayer(audioClip, volumeRate, delay, pitch, isLoop, callback, key);
	}

	public AudioPlayer Play(string audioPath, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = true, bool allowsDuplicate = false, string key = "", Transform sourceTransform = null, Action callback = null)
	{
		if (!allowsDuplicate)
		{
			Stop();
		}
		return RunPlayer(audioPath, volumeRate, delay, pitch, isLoop, callback, key);
	}
}
