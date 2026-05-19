using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace KanKikuchi.AudioManager;

public abstract class AudioManager<T> : SingletonMonoBehaviour<T> where T : AudioManager<T>
{
	private AudioCacheType _cacheType;

	private AudioLoadType _loadType;

	private Dictionary<string, AudioClip> _audioClipDict = new Dictionary<string, AudioClip>();

	protected readonly List<AudioPlayer> _audioPlayerList = new List<AudioPlayer>();

	private int _nextAudioPlayerNo;

	private float _baseVolume = 1f;

	public static readonly string AUDIO_PARENT_DIRECTORY_PATH = "Audio";

	public IReadOnlyList<AudioPlayer> AudioPlayerList => _audioPlayerList;

	protected abstract int _audioPlayerNum { get; }

	protected abstract AudioMixerGroup _audioMixerGroup { get; }

	public int AudioPlayerNum => _audioPlayerNum;

	protected override async UniTask InitAsync()
	{
		await base.InitAsync();
		for (int i = 0; i < _audioPlayerNum; i++)
		{
			AudioSource audioSource = base.gameObject.AddComponent<AudioSource>();
			audioSource.outputAudioMixerGroup = _audioMixerGroup;
			AudioPlayer item = new AudioPlayer(audioSource);
			_audioPlayerList.Add(item);
		}
	}

	protected async UniTask LoadAudioClipAsync(string directoryPath, AudioCacheType cacheType, AudioLoadType loadType, bool isReleaseBGMCache)
	{
		_cacheType = cacheType;
		_loadType = loadType;
		if (_cacheType == AudioCacheType.All)
		{
			switch (loadType)
			{
			case AudioLoadType.Resources:
				_audioClipDict = Resources.LoadAll<AudioClip>(AUDIO_PARENT_DIRECTORY_PATH + "/" + directoryPath).ToDictionary((AudioClip clip) => clip.name, (AudioClip clip) => clip);
				break;
			case AudioLoadType.Addressable:
				try
				{
					IList<AudioClip> list = await Addressables.LoadAssetsAsync<AudioClip>(directoryPath, null).ToUniTask();
					if (list == null)
					{
						list = new List<AudioClip>();
					}
					_audioClipDict = list.ToDictionary((AudioClip clip) => clip.name, (AudioClip clip) => clip);
				}
				catch (Exception)
				{
					Debug.LogError("オーディオ読み込み失敗");
				}
				break;
			}
		}
		if (_cacheType == AudioCacheType.Used && isReleaseBGMCache)
		{
			SceneManager.sceneUnloaded += delegate
			{
				_audioClipDict.Clear();
			};
		}
	}

	private void Update()
	{
		foreach (AudioPlayer audioPlayer in _audioPlayerList)
		{
			if (audioPlayer.CurrentState != AudioPlayer.State.Wait)
			{
				audioPlayer.Update();
			}
		}
	}

	public void OnAudioReloaded()
	{
		foreach (AudioPlayer audioPlayer in _audioPlayerList)
		{
			audioPlayer.OnAudioReloaded();
		}
	}

	public void ChangeBaseVolume(float baseVolume)
	{
		_baseVolume = baseVolume;
		_audioPlayerList.Where((AudioPlayer player) => player.CurrentState != AudioPlayer.State.Wait).ToList().ForEach(delegate(AudioPlayer player)
		{
			player.ChangeVolume(_baseVolume);
		});
	}

	public List<string> GetCurrentAudioNames()
	{
		return (from player in _audioPlayerList
			where player.CurrentState != AudioPlayer.State.Wait
			select player.CurrentAudioName).ToList();
	}

	public AudioPlayer GetPlayer(AudioClip clip)
	{
		return _audioPlayerList.FirstOrDefault((AudioPlayer player) => player.AudioSource.clip == clip);
	}

	public AudioPlayer GetAudioPlayerByName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		foreach (AudioPlayer audioPlayer in _audioPlayerList)
		{
			if (audioPlayer.CurrentAudioName == name)
			{
				return audioPlayer;
			}
		}
		return null;
	}

	public List<AudioPlayer> GetAudioPlayersByName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		return _audioPlayerList.Where((AudioPlayer x) => x.CurrentAudioName == name).ToList();
	}

	public bool IsPlaying()
	{
		return GetCurrentAudioNames().Count > 0;
	}

	public AudioPlayer GetOrCreatePlayerWithKey(string key)
	{
		AudioPlayer audioPlayer = _audioPlayerList.FirstOrDefault((AudioPlayer a) => a.Key == key);
		if (audioPlayer != null)
		{
			return audioPlayer;
		}
		AudioPlayer[] array = _audioPlayerList.Where((AudioPlayer p) => string.IsNullOrEmpty(p.Key)).ToArray();
		if (array.Length != 0)
		{
			return array[UnityEngine.Random.Range(0, array.Length)];
		}
		audioPlayer = _audioPlayerList[_nextAudioPlayerNo];
		audioPlayer.Key = key;
		return audioPlayer;
	}

	protected AudioPlayer RunPlayer(AudioClip audioClip, float volumeRate, float delay, float pitch, bool isLoop, Action callback = null, string key = "", int finishWaitFrameCount = 0)
	{
		AudioPlayer nextAudioPlayer = GetNextAudioPlayer(key);
		if (nextAudioPlayer == null)
		{
			return null;
		}
		nextAudioPlayer.Play(audioClip, _baseVolume, volumeRate, delay, pitch, isLoop, finishWaitFrameCount, callback);
		return nextAudioPlayer;
	}

	protected AudioPlayer RunPlayer(string audioPath, float volumeRate, float delay, float pitch, bool isLoop, Action callback = null, string key = "", int finishWaitFrameCount = 0)
	{
		AudioClip audioClip = GetAudioClip(audioPath);
		return RunPlayer(audioClip, volumeRate, delay, pitch, isLoop, callback, key, finishWaitFrameCount);
	}

	protected string PathToName(string audioPath)
	{
		return Path.GetFileNameWithoutExtension(audioPath);
	}

	private AudioClip GetAudioClip(string audioPath)
	{
		string text = PathToName(audioPath);
		if (_audioClipDict.ContainsKey(text))
		{
			return _audioClipDict[text];
		}
		AudioClip audioClip = null;
		if (_loadType == AudioLoadType.Resources)
		{
			audioClip = Resources.Load<AudioClip>(audioPath);
		}
		else
		{
			try
			{
				audioClip = Addressables.LoadAssetAsync<AudioClip>(text).WaitForCompletion();
			}
			catch
			{
				Debug.LogError("オーディオ読み込み失敗:" + text);
			}
		}
		if (audioClip == null)
		{
			Debug.LogError(audioPath + " not found");
		}
		if (_cacheType == AudioCacheType.Used)
		{
			_audioClipDict[text] = audioClip;
		}
		return audioClip;
	}

	private AudioPlayer GetNextAudioPlayer(string key = "")
	{
		if (!string.IsNullOrEmpty(key))
		{
			AudioPlayer audioPlayer = _audioPlayerList.FirstOrDefault((AudioPlayer a) => a.Key == key);
			if (audioPlayer != null)
			{
				return audioPlayer;
			}
			audioPlayer = _audioPlayerList.FirstOrDefault((AudioPlayer a) => a.CurrentState == AudioPlayer.State.Wait && string.IsNullOrEmpty(a.Key));
			if (audioPlayer != null)
			{
				audioPlayer.Key = key;
				return audioPlayer;
			}
			audioPlayer = _audioPlayerList.FirstOrDefault((AudioPlayer a) => string.IsNullOrEmpty(a.Key));
			if (audioPlayer != null)
			{
				audioPlayer.Key = key;
				return audioPlayer;
			}
			audioPlayer = _audioPlayerList[UnityEngine.Random.Range(0, _audioPlayerList.Count)];
			audioPlayer.Key = key;
			return audioPlayer;
		}
		AudioPlayer audioPlayer2 = _audioPlayerList.FirstOrDefault((AudioPlayer a) => a.CurrentState == AudioPlayer.State.Wait && string.IsNullOrEmpty(a.Key));
		if (audioPlayer2 != null)
		{
			return audioPlayer2;
		}
		return null;
	}

	public void DeleteAudioPlayerKey(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			AudioPlayer audioPlayer = _audioPlayerList.FirstOrDefault((AudioPlayer a) => a.Key == key);
			if (audioPlayer != null)
			{
				audioPlayer.Key = "";
			}
		}
	}

	public void Stop(string audioPathOrName)
	{
		string audioName = PathToName(audioPathOrName);
		_audioPlayerList.ForEach(delegate(AudioPlayer player)
		{
			player.Stop(audioName);
		});
	}

	public void Stop(AudioClip audioClip)
	{
		_audioPlayerList.FirstOrDefault((AudioPlayer player) => player.AudioSource.clip == audioClip)?.Stop();
	}

	public void Stop()
	{
		_audioPlayerList.ForEach(delegate(AudioPlayer player)
		{
			player.Stop();
		});
	}

	public void StopWithKey(string key)
	{
		_audioPlayerList.FirstOrDefault((AudioPlayer a) => a.Key == key)?.Stop();
	}

	public void FadeFromCurrentVolume(string audioPathOrName, float duration, float to, Action callback = null)
	{
		string audioName = PathToName(audioPathOrName);
		_audioPlayerList.ForEach(delegate(AudioPlayer player)
		{
			player.Fade(audioName, duration, player.AudioSource.volume, to, callback);
		});
	}

	public void FadeAllFromCurrentVolume(float duration, float to)
	{
		foreach (AudioPlayer audioPlayer in AudioPlayerList)
		{
			audioPlayer.Fade(duration, audioPlayer.AudioSource.volume, 0f);
		}
	}

	public void Fade(string audioPathOrName, float duration, float from, float to, Action callback = null)
	{
		string audioName = PathToName(audioPathOrName);
		_audioPlayerList.ForEach(delegate(AudioPlayer player)
		{
			player.Fade(audioName, duration, from, to, callback);
		});
	}

	public void FadeOut(string audioPathOrName, float duration = 1f, Action callback = null)
	{
		string audioPathOrName2 = PathToName(audioPathOrName);
		Fade(audioPathOrName2, duration, 1f, 0f, callback);
	}

	public void FadeIn(string audioPathOrName, float duration = 1f, Action callback = null)
	{
		string audioPathOrName2 = PathToName(audioPathOrName);
		Fade(audioPathOrName2, duration, 0f, 1f, callback);
	}

	public void Fade(float duration, float from, float to, Action callback)
	{
		_audioPlayerList.ForEach(delegate(AudioPlayer player)
		{
			player.Fade(duration, from, to, callback);
		});
	}

	public void FadeOut(float duration = 1f, Action callback = null)
	{
		Fade(duration, 1f, 0f, callback);
	}

	public void FadeIn(float duration = 1f, Action callback = null)
	{
		Fade(duration, 0f, 1f, callback);
	}

	public void Pause(string audioPathOrName)
	{
		string audioName = PathToName(audioPathOrName);
		_audioPlayerList.ForEach(delegate(AudioPlayer player)
		{
			player.Pause(audioName);
		});
	}

	public void Pause()
	{
		_audioPlayerList.ForEach(delegate(AudioPlayer player)
		{
			player.Pause();
		});
	}

	public void PauseWithKey(string key)
	{
		_audioPlayerList.FirstOrDefault((AudioPlayer a) => a.Key == key)?.Pause();
	}

	public void UnPause(string audioPathOrName)
	{
		string audioName = PathToName(audioPathOrName);
		_audioPlayerList.ForEach(delegate(AudioPlayer player)
		{
			player.UnPause(audioName);
		});
	}

	public void UnPause(AudioClip clip)
	{
		_audioPlayerList.FirstOrDefault((AudioPlayer player) => player.AudioSource.clip == clip)?.UnPause();
	}

	public void UnPause()
	{
		_audioPlayerList.ForEach(delegate(AudioPlayer player)
		{
			player.UnPause();
		});
	}

	public void UnPauseWithKey(string key)
	{
		_audioPlayerList.FirstOrDefault((AudioPlayer a) => a.Key == key)?.UnPause();
	}
}
