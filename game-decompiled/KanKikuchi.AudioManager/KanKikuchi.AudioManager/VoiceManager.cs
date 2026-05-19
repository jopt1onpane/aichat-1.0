using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace KanKikuchi.AudioManager;

public class VoiceManager : AudioManager<VoiceManager>
{
	public static readonly string AUDIO_DIRECTORY_PATH = "Voice";

	protected override int _audioPlayerNum => AudioManagerSetting.Entity.VoiceAudioPlayerNum;

	protected override AudioMixerGroup _audioMixerGroup => AudioManagerSetting.Entity.VoiceGroup;

	public static async UniTask CreateAndInitAsync()
	{
		GameObject gameObject = new GameObject("VoiceManager", typeof(VoiceManager));
		VoiceManager instance = gameObject.GetComponent<VoiceManager>();
		await UniTask.Yield();
		await instance.InitAsync();
		await UniTask.WaitUntil(() => instance.IsInitialized);
	}

	protected override async UniTask InitAsync()
	{
		await base.InitAsync();
		AudioManagerSetting setting = AudioManagerSetting.Entity;
		await LoadAudioClipAsync(AUDIO_DIRECTORY_PATH, AudioCacheType.All, setting.LoadType, setting.IsReleaseSECache);
		ChangeBaseVolume(setting.VoiceBaseVolume);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public AudioPlayer Play(string audioPath, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = false, Action callback = null, string key = "", Transform sourceTransform = null)
	{
		return RunPlayer(audioPath, volumeRate, delay, pitch, isLoop, callback, key);
	}
}
