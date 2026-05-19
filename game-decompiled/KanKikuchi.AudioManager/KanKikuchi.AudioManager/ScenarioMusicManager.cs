using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace KanKikuchi.AudioManager;

public class ScenarioMusicManager : AudioManager<ScenarioMusicManager>
{
	public static readonly string AUDIO_DIRECTORY_PATH = "Music";

	protected override int _audioPlayerNum => AudioManagerSetting.Entity.MusicAudioPlayerNum;

	protected override AudioMixerGroup _audioMixerGroup => AudioManagerSetting.Entity.MusicGroup;

	public static async UniTask CreateAndInitAsync()
	{
		GameObject gameObject = new GameObject("ScenarioMusicManager", typeof(ScenarioMusicManager));
		ScenarioMusicManager instance = gameObject.GetComponent<ScenarioMusicManager>();
		await instance.InitAsync();
		await UniTask.WaitUntil(() => instance.IsInitialized);
	}

	protected override async UniTask InitAsync()
	{
		await base.InitAsync();
		AudioManagerSetting setting = AudioManagerSetting.Entity;
		await LoadAudioClipAsync(AUDIO_DIRECTORY_PATH, AudioCacheType.All, setting.LoadType, setting.IsReleaseBGMCache);
		ChangeBaseVolume(setting.MusicBaseVolume);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public AudioPlayer Play(AudioClip audioClip, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = true, bool allowsDuplicate = false, string key = "", Action callback = null)
	{
		if (!allowsDuplicate)
		{
			Stop();
		}
		return RunPlayer(audioClip, volumeRate, delay, pitch, isLoop, callback, key, 10);
	}
}
