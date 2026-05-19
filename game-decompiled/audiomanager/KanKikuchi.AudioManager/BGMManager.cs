using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace KanKikuchi.AudioManager;

public class BGMManager : AudioManager<BGMManager>
{
	public static readonly string AUDIO_DIRECTORY_PATH = "BGM";

	protected override int _audioPlayerNum => AudioManagerSetting.Entity.BGMAudioPlayerNum;

	protected override AudioMixerGroup _audioMixerGroup => AudioManagerSetting.Entity.BGMGroup;

	private AudioPlayer _audioPlayer => _audioPlayerList[0];

	protected override async UniTask InitAsync()
	{
		await base.InitAsync();
		AudioManagerSetting setting = AudioManagerSetting.Entity;
		await LoadAudioClipAsync(AUDIO_DIRECTORY_PATH, setting.BGMCacheType, setting.LoadType, setting.IsReleaseBGMCache);
		ChangeBaseVolume(setting.BGMBaseVolume);
		if (!setting.IsDestroyBGMManager)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public AudioPlayer Play(AudioClip audioClip, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = true, bool allowsDuplicate = false)
	{
		if (!allowsDuplicate)
		{
			Stop();
		}
		return RunPlayer(audioClip, volumeRate, delay, pitch, isLoop);
	}

	public AudioPlayer Play(string audioPath, float volumeRate = 1f, float delay = 0f, float pitch = 1f, bool isLoop = true, bool allowsDuplicate = false)
	{
		if (!allowsDuplicate)
		{
			Stop();
		}
		return RunPlayer(audioPath, volumeRate, delay, pitch, isLoop);
	}
}
